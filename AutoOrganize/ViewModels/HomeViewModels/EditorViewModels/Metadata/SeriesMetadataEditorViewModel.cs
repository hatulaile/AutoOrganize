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
public partial class SeriesMetadataEditorViewModel
    : ProviderIdsViewModelBase, IResultNavigationViewModel<SeriesMetadataEditorArgs, IEnumerable<TvIdentifyResult>?>
{
    private readonly INavigationService _navigationService;
    private readonly IToastServices _toastServices;
    private readonly IMetadataService _metadataService;
    private readonly INameParserService _nameParserService;
    private readonly ILogger<SeriesMetadataEditorViewModel> _logger;

    public IReadOnlyList<SeriesMetadataTreeNode> Nodes { get; private set; } = [];

    public CancellationToken CancellationToken { get; set; }

    [ObservableProperty]
    public partial SeriesMetadataRequest? Request { get; set; }

    public int? Year { get; set; }

    protected override bool CanSetProviderId => Request is not null;

    public SeriesMetadataEditorViewModel(
        INavigationService navigationService,
        IToastServices toastServices,
        IMetadataService metadataService,
        INameParserService nameParserService,
        ILogger<SeriesMetadataEditorViewModel> logger)
    {
        _navigationService = navigationService;
        _toastServices = toastServices;
        _metadataService = metadataService;
        _nameParserService = nameParserService;
        _logger = logger;
    }

    public void OnNavigatingTo(SeriesMetadataEditorArgs args)
    {
        Nodes = args.Nodes;
        Request = new SeriesMetadataRequest();
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

            if (string.IsNullOrEmpty(Request?.Name) && string.IsNullOrEmpty(TmdbId))
            {
                _toastServices.Show(new Toast("至少应该填入名称或ID", NotificationType.Error), this);
                return;
            }

            SeriesSearchResult? selected = await TrySearchSeriesAsync(token);
            if (selected?.ProviderIds is null)
                return;

            var seriesRequest = new SeriesMetadataRequest
            {
                Name = Request?.Name,
                Language = Request?.Language,
                ImageLanguages = Request?.ImageLanguages,
                ProviderIds = selected.ProviderIds
            };

            SeriesMetadata? series = await _metadataService.GetSeriesAsync(seriesRequest, token);
            if (series is null)
            {
                _toastServices.Show(new Toast("未找到对应剧集", NotificationType.Error), this);
                return;
            }

            var results = new List<TvIdentifyResult>();
            foreach (SeriesMetadataTreeNode node in Nodes)
            {
                int seasonCount = node.Children.Count;
                Task<SeasonMetadata?>[] seasonTasks = new Task<SeasonMetadata?>[seasonCount];
                int i = 0;
                foreach (SeasonMetadataTreeNode seasonNode in node.Children.Cast<SeasonMetadataTreeNode>())
                {
                    seasonTasks[i++] = _metadataService.GetSeasonAsync(new SeasonMetadataRequest
                    {
                        SeriesName = series.Name,
                        SeasonNumber = seasonNode.Metadata.SeasonNumber ?? 0,
                        Language = "zh-cn",
                        SeriesProviderIds = series.ProviderIds
                    }, token);
                }

                for (i = 0; i < seasonCount; i++)
                {
                    var seasonNode = (SeasonMetadataTreeNode)node.Children[i];
                    SeasonMetadata? season;
                    try
                    {
                        season = await seasonTasks[i];
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        _logger.LogWarning(e, "获取季元数据失败: 季 {Season}", seasonNode.Metadata.SeasonNumber ?? 0);
                        season = null;
                    }

                    int episodeCount = seasonNode.Children.Count;
                    Task<EpisodeMetadata?>[] episodeTasks = new Task<EpisodeMetadata?>[episodeCount];
                    int j = 0;
                    foreach (EpisodeMetadataTreeNode episodeNode in seasonNode.Children.Cast<EpisodeMetadataTreeNode>())
                    {
                        episodeTasks[j++] = season is null
                            ? Task.FromResult<EpisodeMetadata?>(null)
                            : _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                            {
                                SeasonNumber = season.SeasonNumber ?? 0,
                                EpisodeNumber = episodeNode.Metadata.EpisodeNumber ?? 0,
                                Language = "zh-cn",
                                SeriesProviderIds = season.Series?.ProviderIds
                            }, token);
                    }

                    for (j = 0; j < episodeCount; j++)
                    {
                        var episodeNode = (EpisodeMetadataTreeNode)seasonNode.Children[j];
                        EpisodeMetadata? episode = null;
                        bool isException = false;
                        try
                        {
                            episode = await episodeTasks[j];
                        }
                        catch (Exception e) when (e is not OperationCanceledException)
                        {
                            _logger.LogWarning(e, "获取集元数据失败: 季 {Season}, 集 {Episode}",
                                season?.SeasonNumber ?? 0, episodeNode.Metadata.EpisodeNumber ?? 0);

                            results.AddRange(episodeNode.Children.Cast<SourceFileNode>()
                                .Select(file => new TvIdentifyResult(file, null, e)));
                            isException = true;
                        }

                        if (!isException)
                        {
                            results.AddRange(episodeNode.Children.Cast<SourceFileNode>().Select(file =>
                                new TvIdentifyResult(file, episode, episode is null ? new Exception("未获取到集元数据") : null)));
                        }
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
            _logger.LogError(e, "手动应用剧元数据失败");
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
            foreach (SeriesMetadataTreeNode node in Nodes)
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
            _logger.LogError(e, "自动识别剧失败");
        }
    }

    [RelayCommand]
    private void CancelApply() => ApplyCommand.Cancel();

    [RelayCommand]
    public void Cancel()
    {
        _navigationService.Pop<IEnumerable<TvIdentifyResult>?>(this, null);
    }

    private static SeriesMetadataRequest CreateRequest(SeriesMetadataTreeNode node) =>
        new()
        {
            Name = node.Metadata.Name,
            Language = "zh-cn"
        };

    private async Task<SeriesSearchResult?> TrySearchSeriesAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Request?.Name) && ProviderIds?.Count is not > 0)
            return null;

        return await _navigationService
            .RequestAsync<SeriesSearchViewModel, SeriesSearchArgs, SeriesSearchResult?>(
                this, new SeriesSearchArgs(new SeriesSearchRequest
                {
                    Name = Request?.Name,
                    FirstAirDateYear = Year,
                    Language = "zh-cn",
                    ProviderIds = ProviderIds
                }),
                token);
    }
}