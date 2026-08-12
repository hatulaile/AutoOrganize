using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;
using AutoOrganize.Library.Services.NameParsers;
using AutoOrganize.Models;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Models.MetadataNodes.Metadata;
using AutoOrganize.Models.Args;
using AutoOrganize.Models.Args.EditorArgs;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ursa.Controls;
using ViewModelRegistrationGenerator;

using AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Search;
namespace AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Metadata;

[ViewModelRegistration]
public partial class MovieMetadataEditorViewModel
    : ProviderIdsViewModelBase, IResultNavigationViewModel<MovieMetadataEditorArgs, IEnumerable<MovieIdentifyResult>?>
{
    private readonly INavigationService _navigationService;
    private readonly IToastServices _toastServices;
    private readonly IMetadataService _metadataService;
    private readonly INameParserService _nameParserService;
    private readonly ILogger<MovieMetadataEditorViewModel> _logger;

    public IReadOnlyList<MovieMetadataTreeNode> Nodes { get; private set; } = [];

    public CancellationToken CancellationToken { get; set; }

    [ObservableProperty]
    public partial MovieMetadataRequest? Request { get; set; }

    protected override bool CanSetProviderId => Request is not null;

    public MovieMetadataEditorViewModel(
        INavigationService navigationService,
        IToastServices toastServices,
        IMetadataService metadataService,
        INameParserService nameParserService,
        ILogger<MovieMetadataEditorViewModel> logger)
    {
        _navigationService = navigationService;
        _toastServices = toastServices;
        _metadataService = metadataService;
        _nameParserService = nameParserService;
        _logger = logger;
    }

    public void OnNavigatingTo(MovieMetadataEditorArgs args)
    {
        Nodes = args.Nodes;
        Request = new MovieMetadataRequest();
        ProviderIds = InitProviderIds();
        OnPropertyChanged(nameof(TmdbId));
    }

    [RelayCommand]
    private void AutoFill()
    {
        if (Nodes is not [var node])
            return;

        Request = CreateRequest(node);
    }

    [RelayCommand]
    private async Task ApplyAsync(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            if (string.IsNullOrEmpty(Request?.Title) && string.IsNullOrEmpty(TmdbId))
            {
                _toastServices.Show(new Toast("至少应该填入名称或ID", NotificationType.Error), this);
                return;
            }

            MovieSearchResult? selected = await TrySearchMovieAsync(token);
            if (selected?.ProviderIds is null)
                return;

            var movieRequest = new MovieMetadataRequest
            {
                Title = Request?.Title,
                Language = Request?.Language,
                ImageLanguages = Request?.ImageLanguages,
                ProviderIds = selected.ProviderIds
            };

            MovieMetadata? movie = await _metadataService.GetMovieAsync(movieRequest, token);
            if (movie is null)
            {
                _toastServices.Show(new Toast("未找到对应电影", NotificationType.Error), this);
                return;
            }

            var results = new List<MovieIdentifyResult>();
            foreach (MovieMetadataTreeNode node in Nodes)
            {
                foreach (SourceFileNode file in node.Children.Cast<SourceFileNode>())
                    results.Add(new MovieIdentifyResult(file, movie, null));
            }

            _navigationService.Pop<IEnumerable<MovieIdentifyResult>>(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "手动应用电影元数据失败");
        }
    }

    [RelayCommand]
    private async Task AutoIdentify(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            var results = new List<MovieIdentifyResult>();
            foreach (MovieMetadataTreeNode node in Nodes)
            {
                foreach (SourceFileNode file in node.Children.Cast<SourceFileNode>())
                {
                    token.ThrowIfCancellationRequested();

                    MovieParseResult parse = _nameParserService.ParseMovie(file.FullPath);
                    if (parse is not { Title: { } title })
                        continue;

                    MovieMetadata? movie = await _metadataService.GetMovieAsync(new MovieMetadataRequest
                    {
                        Title = title,
                        Year = parse.Year,
                        Language = "zh-cn"
                    }, token);

                    results.Add(movie is null
                        ? new MovieIdentifyResult(file, null, new Exception("无法获取数据"))
                        : new MovieIdentifyResult(file, movie, null));
                }
            }

            if (!results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能自动识别任何文件,请手动识别", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop<IEnumerable<MovieIdentifyResult>>(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "自动识别电影失败");
        }
    }

    [RelayCommand]
    private void CancelApply() => ApplyCommand.Cancel();

    [RelayCommand]
    public void Cancel()
    {
        _navigationService.Pop<IEnumerable<MovieIdentifyResult>?>(this, null);
    }

    private static MovieMetadataRequest CreateRequest(MovieMetadataTreeNode node) =>
        new()
        {
            Title = node.Metadata.Name,
            Language = "zh-cn"
        };

    private async Task<MovieSearchResult?> TrySearchMovieAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Request?.Title) && ProviderIds?.Count is not > 0)
            return null;

        return await _navigationService
            .RequestAsync<MovieSearchViewModel, MovieSearchArgs, MovieSearchResult?>(
                this, new MovieSearchArgs(new MovieSearchRequest
                {
                    Name = Request?.Title,
                    Year = Request?.Year,
                    Language = "zh-cn",
                    ProviderIds = ProviderIds
                }),
                token);
    }
}