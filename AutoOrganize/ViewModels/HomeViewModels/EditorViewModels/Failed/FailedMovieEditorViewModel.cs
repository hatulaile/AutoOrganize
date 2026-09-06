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
using AutoOrganize.Models.Args;
using AutoOrganize.Models.Args.EditorArgs;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
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

namespace AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.Failed;

[ViewModelRegistration]
public partial class FailedMovieEditorViewModel :
    ProviderIdsViewModelBase,
    IResultNavigationViewModel<FailedMovieEditorArgs, IEnumerable<FailedMovieIdentifyResult>?>
{
    private readonly INavigationService _navigationService;
    private readonly IToastServices _toastServices;
    private readonly IMetadataService _metadataService;
    private readonly INameParserService _nameParserService;
    private readonly ILogger<FailedMovieEditorViewModel> _logger;

    public IReadOnlyList<IFailedNode> Nodes { get; private set; } = [];

    public CancellationToken CancellationToken { get; set; }

    [ObservableProperty]
    public partial MovieMetadataRequest? Request { get; set; }

    protected override bool CanSetProviderId => Request is not null;

    public FailedMovieEditorViewModel(
        INavigationService navigationService,
        IToastServices toastServices,
        IMetadataService metadataService,
        INameParserService nameParserService,
        ILogger<FailedMovieEditorViewModel> logger)
    {
        _navigationService = navigationService;
        _toastServices = toastServices;
        _metadataService = metadataService;
        _nameParserService = nameParserService;
        _logger = logger;
    }

    public void OnNavigatingTo(FailedMovieEditorArgs args)
    {
        Nodes = args.Nodes;
        Request = new MovieMetadataRequest();
        ProviderIds = InitProviderIds();
        OnPropertyChanged(nameof(TmdbId));
    }

    [RelayCommand]
    private void AutoFill()
    {
        if (Nodes is not [IFileSystemNode { FullPath: var path }])
            return;

        MovieParseResult parse = _nameParserService.ParseMovie(path);
        var request = new MovieMetadataRequest
        {
            Title = parse.Title,
            Year = parse.Year
        };
        Request = request;
    }

    [RelayCommand]
    private async Task Apply(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        if (string.IsNullOrEmpty(Request?.Title) && ProviderIds?.Count is not > 0)
        {
            _toastServices.Show(new Toast("至少应该填入名称或ID", NotificationType.Error), this);
            return;
        }

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            MovieSearchResult? selected = await TrySearchMovieAsync(token);
            if (selected?.ProviderIds is null)
                return;

            var movieRequest = new MovieMetadataRequest
            {
                Title = Request?.Title,
                Language = "zh-cn",
                ImageLanguages = Request?.ImageLanguages,
                ProviderIds = selected.ProviderIds
            };

            MovieMetadata? movie = await _metadataService.GetMovieAsync(movieRequest, token);
            if (movie is null)
            {
                _toastServices.Show(new Toast("未找到对应电影", NotificationType.Error), this);
                return;
            }

            IReadOnlyList<FailedMovieIdentifyResult> results =
            [
                .. GetFailedFiles(Nodes)
                    .Select(file => new FailedMovieIdentifyResult(file, movie, null))
            ];

            _navigationService.Pop<IEnumerable<FailedMovieIdentifyResult>>(this, results);
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
    private async Task SupplementApply(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            var results = new List<FailedMovieIdentifyResult>();

            MovieSearchResult? selected = await TrySearchMovieAsync(token);
            foreach (FailedFileNode file in GetFailedFiles(Nodes))
            {
                token.ThrowIfCancellationRequested();

                MovieParseResult parse = _nameParserService.ParseMovie(file.FullPath);
                var movie = await _metadataService.GetMovieAsync(new MovieMetadataRequest
                {
                    Title = selected?.Title ?? Request?.Title,
                    Year = selected?.ReleaseDate?.Year ?? Request?.Year ?? parse.Year,
                    Language = "zh-cn",
                    ProviderIds = selected?.ProviderIds ?? ProviderIds
                }, token);

                if (movie is null)
                {
                    results.Add(new FailedMovieIdentifyResult(file, null, new Exception("无法获取数据")));
                    continue;
                }

                results.Add(new FailedMovieIdentifyResult(file, movie, null));
            }

            if (!results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能获取到任何文件信息", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop<IEnumerable<FailedMovieIdentifyResult>>(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "补充电影元数据失败");
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

            var results = new List<FailedMovieIdentifyResult>();
            foreach (FailedFileNode file in GetFailedFiles(Nodes))
            {
                token.ThrowIfCancellationRequested();

                MovieParseResult parse = _nameParserService.ParseMovie(file.FullPath);
                if (parse is not { Title: { } title })
                    continue;

                MovieMetadata? movie = await _metadataService.GetMovieAsync(new MovieMetadataRequest
                {
                    Title = title,
                    Year = parse.Year,
                    Language = "zh-cn",
                }, token);

                if (movie is null)
                {
                    results.Add(new FailedMovieIdentifyResult(file, null, new Exception("无法获取数据")));
                    continue;
                }

                results.Add(new FailedMovieIdentifyResult(file, movie, null));
            }

            if (!results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能自动识别任何文件,请手动识别", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop<IEnumerable<FailedMovieIdentifyResult>>(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "自动识别失败文件为电影失败");
        }
    }

    [RelayCommand]
    private void CancelLoading()
    {
        if (AutoIdentifyCommand.IsRunning)
            AutoIdentifyCommand.Cancel();
        if (ApplyCommand.IsRunning)
            ApplyCommand.Cancel();
        if (SupplementApplyCommand.IsRunning)
            SupplementApplyCommand.Cancel();
    }

    [RelayCommand]
    public void Cancel()
    {
        _navigationService.Pop(this);
    }

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

    private static IEnumerable<FailedFileNode> GetFailedFiles(IReadOnlyList<IFailedNode> nodes) =>
        nodes.SelectMany(GetFailedFiles);

    private static IEnumerable<FailedFileNode> GetFailedFiles(IFailedNode node) =>
        node switch
        {
            FailedFileNode file => [file],
            FailedDirectoryNode directory => directory.FindChildren<FailedFileNode>(),
            _ => []
        };
}