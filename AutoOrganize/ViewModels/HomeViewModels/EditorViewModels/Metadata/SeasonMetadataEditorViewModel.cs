using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;
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
public partial class SeasonMetadataEditorViewModel
    : ProviderIdsViewModelBase, IResultNavigationViewModel<SeasonMetadataEditorArgs, IEnumerable<TvIdentifyResult>?>
{
    private readonly INavigationService _navigationService;
    private readonly IToastServices _toastServices;
    private readonly IMetadataService _metadataService;
    private readonly INameParserService _nameParserService;
    private readonly ILogger<SeasonMetadataEditorViewModel> _logger;

    public IReadOnlyList<SeasonMetadataTreeNode> Nodes { get; private set; } = [];

    public CancellationToken CancellationToken { get; set; }

    [ObservableProperty]
    public partial SeasonMetadataRequest? Request { get; set; }

    protected override bool CanSetProviderId => Request is not null;

    public SeasonMetadataEditorViewModel(
        INavigationService navigationService,
        IToastServices toastServices,
        IMetadataService metadataService,
        INameParserService nameParserService,
        ILogger<SeasonMetadataEditorViewModel> logger)
    {
        _navigationService = navigationService;
        _toastServices = toastServices;
        _metadataService = metadataService;
        _nameParserService = nameParserService;
        _logger = logger;
    }

    public void OnNavigatingTo(SeasonMetadataEditorArgs args)
    {
        Nodes = args.Nodes;
        Request = new SeasonMetadataRequest();
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

            if (string.IsNullOrEmpty(Request?.SeriesName) && string.IsNullOrEmpty(TmdbId))
            {
                _toastServices.Show(new Toast("至少应该填入名称或ID", NotificationType.Error), this);
                return;
            }

            SeriesSearchResult? selected = await TrySearchSeriesAsync(token);
            if (selected?.ProviderIds is null)
                return;

            var seasonRequest = new SeasonMetadataRequest
            {
                SeriesName = Request?.SeriesName,
                SeasonNumber = Request?.SeasonNumber ?? 0,
                Language = Request?.Language,
                ImageLanguages = Request?.ImageLanguages,
                SeriesProviderIds = selected.ProviderIds
            };

            SeasonMetadata? season = await _metadataService.GetSeasonAsync(seasonRequest, token);
            if (season is null)
            {
                _toastServices.Show(new Toast("未找到对应季", NotificationType.Error), this);
                return;
            }

            var results = new List<TvIdentifyResult>();
            foreach (SeasonMetadataTreeNode node in Nodes)
            {
                int count = node.Children.Count;
                Task<EpisodeMetadata?>[] tasks = new Task<EpisodeMetadata?>[count];
                int i = 0;
                foreach (EpisodeMetadataTreeNode episodeNode in node.Children.Cast<EpisodeMetadataTreeNode>())
                {
                    tasks[i++] = _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                    {
                        SeasonNumber = season.SeasonNumber ?? 0,
                        EpisodeNumber = episodeNode.Metadata.EpisodeNumber ?? 0,
                        Language = Request?.Language,
                        ImageLanguages = Request?.ImageLanguages,
                        SeriesProviderIds = season.Series?.ProviderIds
                    }, token);
                }

                for (i = 0; i < count; i++)
                {
                    var episodeNode = (EpisodeMetadataTreeNode)node.Children[i];
                    EpisodeMetadata? episode = null;
                    bool isException = false;
                    try
                    {
                        episode = await tasks[i];
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        _logger.LogWarning(e, "获取集元数据失败: 季 {Season}, 集 {Episode}",
                            season.SeasonNumber ?? 0, episodeNode.Metadata.EpisodeNumber ?? 0);

                        results.AddRange(episodeNode.Children.Cast<SourceFileNode>().Select(file =>
                            new TvIdentifyResult(file, null, e)));

                        isException = true;
                    }

                    if (!isException)
                    {
                        results.AddRange(episodeNode.Children.Cast<SourceFileNode>().Select(file =>
                            new TvIdentifyResult(file, episode, episode is null ? new Exception("未获取到集元数据") : null)));
                    }
                }
            }

            if (!results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能获取到任何文件信息", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop<IEnumerable<TvIdentifyResult>>(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "手动应用季元数据失败");
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

            var results = new List<TvIdentifyResult>();
            foreach (SeasonMetadataTreeNode node in Nodes)
            {
                foreach (SourceFileNode file in node.FindChildren<SourceFileNode>())
                {
                    token.ThrowIfCancellationRequested();

                    TvParseResult parseResult = _nameParserService.ParseTv(file.FullPath);
                    parseResult.Season ??= 1;
                    if (parseResult is not { Title: { } title, Season: { } season, Episode: { } episode })
                        continue;

                    EpisodeMetadata? episodeMetadata = await _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                    {
                        SeriesName = title,
                        Year = parseResult.Year,
                        SeasonNumber = season,
                        EpisodeNumber = episode,
                        Language = "zh-cn"
                    }, token: token);

                    results.Add(episodeMetadata is null
                        ? new TvIdentifyResult(file, null, new Exception("无法获取数据"))
                        : new TvIdentifyResult(file, episodeMetadata, null));
                }
            }

            if (!results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能自动识别任何文件,请手动识别", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop<IEnumerable<TvIdentifyResult>>(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "自动识别季失败");
        }
    }

    [RelayCommand]
    private void CancelApply() => ApplyCommand.Cancel();

    [RelayCommand]
    public void Cancel()
    {
        _navigationService.Pop<IEnumerable<TvIdentifyResult>?>(this, null);
    }

    private static SeasonMetadataRequest CreateRequest(SeasonMetadataTreeNode node)
    {
        var metadata = node.Metadata;
        var series = node.Parent as SeriesMetadataTreeNode;
        return new SeasonMetadataRequest
        {
            SeriesName = series?.Metadata.Name,
            SeasonNumber = metadata.SeasonNumber ?? 0,
            Language = "zh-cn"
        };
    }

    private async Task<SeriesSearchResult?> TrySearchSeriesAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Request?.SeriesName) && ProviderIds?.Count is not > 0)
            return null;

        return await _navigationService
            .RequestAsync<SeriesSearchViewModel, SeriesSearchArgs, SeriesSearchResult?>(
                this, new SeriesSearchArgs(new SeriesSearchRequest
                {
                    Name = Request?.SeriesName,
                    FirstAirDateYear = null,
                    Language = "zh-cn",
                    ProviderIds = ProviderIds
                }),
                token);
    }
}