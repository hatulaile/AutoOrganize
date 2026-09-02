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

namespace AutoOrganize.ViewModels.HomeViewModels.EditorViewModels.File;

[ViewModelRegistration]
public partial class TvFileEditorViewModel
    : ProviderIdsViewModelBase, IResultNavigationViewModel<TvFileEditorArgs, IEnumerable<TvIdentifyResult>?>
{
    private readonly INavigationService _navigationService;
    private readonly IToastServices _toastServices;
    private readonly IMetadataService _metadataService;
    private readonly INameParserService _nameParserService;
    private readonly ILogger<TvFileEditorViewModel> _logger;

    public IReadOnlyList<SourceFileNode> Nodes { get; private set; } = [];

    public CancellationToken CancellationToken { get; set; }

    public int? Season
    {
        get => Request?.SeasonNumber is not -1 ? Request?.SeasonNumber : null;
        set => Request?.SeasonNumber = value ?? -1;
    }

    public long? Episode
    {
        get => Request?.EpisodeNumber is not -1 ? Request?.EpisodeNumber : null;
        set => Request?.EpisodeNumber = value ?? -1;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Season))]
    [NotifyPropertyChangedFor(nameof(Episode))]
    public partial EpisodeMetadataRequest? Request { get; set; }

    protected override bool CanSetProviderId => Request is not null;

    public TvFileEditorViewModel(
        INavigationService navigationService,
        IToastServices toastServices,
        IMetadataService metadataService,
        INameParserService nameParserService,
        ILogger<TvFileEditorViewModel> logger)
    {
        _navigationService = navigationService;
        _toastServices = toastServices;
        _metadataService = metadataService;
        _nameParserService = nameParserService;
        _logger = logger;
    }

    public void OnNavigatingTo(TvFileEditorArgs args)
    {
        Nodes = args.Nodes;
        Request = new EpisodeMetadataRequest();
        ProviderIds = InitProviderIds();
        OnPropertyChanged(nameof(TmdbId));
    }

    [RelayCommand]
    private void AutoFill()
    {
        if (Nodes is not [{ FullPath: var path }])
            return;

        TvParseResult parse = _nameParserService.ParseTv(path);
        Request = new EpisodeMetadataRequest
        {
            SeriesName = parse.Title,
            SeasonNumber = parse.Season ?? 0,
            EpisodeNumber = parse.Episode ?? 0,
            Year = parse.Year,
            Language = "zh-cn"
        };
    }

    [RelayCommand]
    private async Task AutoIdentifyAsync(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            var results = new List<TvIdentifyResult>(Nodes.Count);
            foreach (SourceFileNode fileNode in Nodes)
            {
                token.ThrowIfCancellationRequested();
                TvParseResult parseResult = _nameParserService.ParseTv(fileNode.FullPath);
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

                if (episodeMetadata is null)
                {
                    results.Add(new TvIdentifyResult(fileNode, null, new Exception("无法获取数据")));
                    continue;
                }

                results.Add(new TvIdentifyResult(fileNode, episodeMetadata, null));
            }

            if (results.Count == 0)
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
            _logger.LogError(e, "自动识别文件为剧集失败");
        }
    }

    [RelayCommand]
    private async Task SupplementApplyFromNameAsync(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            SeriesSearchResult? selected = await TrySearchSeriesAsync(token);
            var results = new List<TvIdentifyResult>(Nodes.Count);
            foreach (SourceFileNode fileNode in Nodes)
            {
                token.ThrowIfCancellationRequested();
                TvParseResult parseResult = _nameParserService.ParseTv(fileNode.FullPath);
                parseResult.Season ??= 1;
                int? season = Season ?? parseResult.Season;
                long? episode = Episode ?? parseResult.Episode;
                if (season is null || episode is null)
                    continue;

                EpisodeMetadata? episodeMetadata = await _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                {
                    SeriesName = selected?.Name ?? parseResult.Title,
                    Year = selected?.FirstAirDate?.Year ?? Request?.Year ?? parseResult.Year,
                    SeasonNumber = season.Value,
                    EpisodeNumber = episode.Value,
                    Language = "zh-cn",
                    SeriesProviderIds = selected?.ProviderIds ?? ProviderIds
                }, token: token);

                if (episodeMetadata is null)
                {
                    results.Add(new TvIdentifyResult(fileNode, null, new Exception("无法获取数据")));
                    continue;
                }

                results.Add(new TvIdentifyResult(fileNode, episodeMetadata, null));
            }

            if (results.Count == 0)
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
            _logger.LogError(e, "按名称补充剧集元数据失败");
        }
    }

    [RelayCommand]
    private async Task SupplementApplyFromMetadataAsync(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            SeriesSearchResult? selected = await TrySearchSeriesAsync(token);
            var results = new List<TvIdentifyResult>(Nodes.Count);
            foreach (SourceFileNode fileNode in Nodes)
            {
                token.ThrowIfCancellationRequested();
                EpisodeMetadata? episodeMetadata;
                if (fileNode.Parent is EpisodeMetadataTreeNode
                    {
                        Metadata:
                        {
                            SeasonNumber: not null,
                            EpisodeNumber: not null
                        } oldEpisodeMetadata
                    })
                {
                    episodeMetadata = await _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                    {
                        SeriesName = selected?.Name ?? oldEpisodeMetadata.Name,
                        Year = selected?.FirstAirDate?.Year ?? Request?.Year ?? oldEpisodeMetadata.AirDate?.Year,
                        SeasonNumber = Season ?? oldEpisodeMetadata.SeasonNumber.Value,
                        EpisodeNumber = Episode ?? oldEpisodeMetadata.EpisodeNumber.Value,
                        Language = "zh-cn",
                        SeriesProviderIds = selected?.ProviderIds ?? ProviderIds
                    }, token: token);
                }
                else
                {
                    TvParseResult parseResult = _nameParserService.ParseTv(fileNode.FullPath);
                    parseResult.Season ??= 1;
                    int? season = Season ?? parseResult.Season;
                    long? episode = Episode ?? parseResult.Episode;
                    if (season is null || episode is null)
                        continue;

                    episodeMetadata = await _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                    {
                        SeriesName = selected?.Name ?? parseResult.Title,
                        Year = selected?.FirstAirDate?.Year ?? Request?.Year ?? parseResult.Year,
                        SeasonNumber = season.Value,
                        EpisodeNumber = episode.Value,
                        Language = "zh-cn",
                        SeriesProviderIds = selected?.ProviderIds ?? ProviderIds
                    }, token: token);
                }


                if (episodeMetadata is null)
                {
                    results.Add(new TvIdentifyResult(fileNode, null, new Exception("无法获取数据")));
                    continue;
                }

                results.Add(new TvIdentifyResult(fileNode, episodeMetadata, null));
            }

            if (results.Count == 0)
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
            _logger.LogError(e, "按已有元数据补充剧集信息失败");
        }
    }

    [RelayCommand]
    private async Task ApplyAsync(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        if (string.IsNullOrEmpty(Request?.SeriesName) && ProviderIds?.Count is not > 0)
        {
            _toastServices.Show(new Toast("至少应该填入名称或ID", NotificationType.Error), this);
            return;
        }

        if (Season is null || Episode is null)
        {
            _toastServices.Show(new Toast("需要填入季号和集号", NotificationType.Error), this);
            return;
        }

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            SeriesSearchResult? selected = await TrySearchSeriesAsync(token);
            if (selected is null)
            {
                _toastServices.Show(new Toast("未找到对应剧集", NotificationType.Error), this);
                return;
            }

            EpisodeMetadata? episode = await _metadataService.GetEpisodeAsync(
                new EpisodeMetadataRequest
                {
                    SeasonNumber = Request?.SeasonNumber ?? 0,
                    EpisodeNumber = Request?.EpisodeNumber ?? 0,
                    Language = Request?.Language,
                    ImageLanguages = Request?.ImageLanguages,
                    SeriesProviderIds = selected.ProviderIds
                }, token);
            if (episode is null)
            {
                _toastServices.Show(new Toast("未找到对应剧集", NotificationType.Error), this);
                return;
            }

            _navigationService.Pop<IEnumerable<TvIdentifyResult>?>(this,
                Nodes.Select(x => new TvIdentifyResult(x, episode, null)));
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "手动应用剧集元数据失败");
        }
    }

    [RelayCommand]
    private void CancelLoading()
    {
        if (AutoIdentifyCommand.IsRunning)
            AutoIdentifyCommand.Cancel();
        if (SupplementApplyFromNameCommand.IsRunning)
            SupplementApplyFromNameCommand.Cancel();
        if (SupplementApplyFromMetadataCommand.IsRunning)
            SupplementApplyFromMetadataCommand.Cancel();
        if (ApplyCommand.IsRunning)
            ApplyCommand.Cancel();
    }

    [RelayCommand]
    public void Cancel()
    {
        _navigationService.Pop<IEnumerable<TvIdentifyResult>?>(this, null);
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
                    FirstAirDateYear = Request?.Year,
                    Language = "zh-cn",
                    ProviderIds = ProviderIds
                }),
                token);
    }
}