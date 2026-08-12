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
public partial class FailedTvEditorViewModel :
    ProviderIdsViewModelBase,
    IResultNavigationViewModel<FailedTvEditorArgs, IEnumerable<FailedTvIdentifyResult>?>
{
    private readonly INavigationService _navigationService;
    private readonly IToastServices _toastServices;
    private readonly IMetadataService _metadataService;
    private readonly INameParserService _nameParserService;
    private readonly ILogger<FailedTvEditorViewModel> _logger;

    public IReadOnlyList<IFailedNode> Nodes { get; private set; } = [];

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

    public FailedTvEditorViewModel(
        INavigationService navigationService,
        IToastServices toastServices,
        IMetadataService metadataService,
        INameParserService nameParserService,
        ILogger<FailedTvEditorViewModel> logger)
    {
        _navigationService = navigationService;
        _toastServices = toastServices;
        _metadataService = metadataService;
        _nameParserService = nameParserService;
        _logger = logger;
    }

    public void OnNavigatingTo(FailedTvEditorArgs args)
    {
        Nodes = args.Nodes;
        Request = new EpisodeMetadataRequest();
        ProviderIds = InitProviderIds();
        OnPropertyChanged(nameof(TmdbId));
    }

    [RelayCommand]
    private void AutoFill()
    {
        if (Nodes is not [IFullPath { FullPath: var path }])
            return;

        var request = new EpisodeMetadataRequest();
        TvParseResult parse = _nameParserService.ParseTv(path);
        request.SeriesName = parse.Title;
        request.Year = parse.Year;
        request.SeasonNumber = parse.Season ?? -1;
        request.EpisodeNumber = parse.Episode ?? -1L;
        Request = request;
    }

    [RelayCommand]
    private async Task AutoApply(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            List<FailedTvIdentifyResult> results = [];
            foreach (FailedFileNode fileNode in GetFailedFiles(Nodes))
            {
                var parseResult = _nameParserService.ParseTv(fileNode.FullPath);
                parseResult.Season ??= 1;
                if (parseResult is not { Title: not null, Season: not null, Episode: not null })
                    continue;

                EpisodeMetadata? episodeMetadata = await _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                {
                    SeriesName = parseResult.Title,
                    Year = parseResult.Year,
                    SeasonNumber = parseResult.Season.Value,
                    EpisodeNumber = parseResult.Episode.Value,
                    Language = "zh-cn"
                }, token: token);

                if (episodeMetadata is null)
                {
                    results.Add(new FailedTvIdentifyResult(fileNode, null, new Exception("无法获取数据")));
                    continue;
                }

                results.Add(new FailedTvIdentifyResult(fileNode, episodeMetadata, null));
            }

            if (results.Count == 0 || !results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能自动识别任何文件,请手动识别", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop<IEnumerable<FailedTvIdentifyResult>>(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "自动识别失败文件为剧集失败");
        }
    }

    [RelayCommand]
    private async Task Apply(CancellationToken cancellationToken)
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
            EpisodeMetadata? episodeMetadata = await _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
            {
                SeriesName = selected?.Name,
                Year = selected?.FirstAirDate?.Year,
                SeasonNumber = Season.Value,
                EpisodeNumber = Episode.Value,
                Language = "zh-cn",
                SeriesProviderIds = selected?.ProviderIds
            }, token: token);

            List<FailedTvIdentifyResult> results =
            [
                .. GetFailedFiles(Nodes)
                    .Select(fileNode => new FailedTvIdentifyResult(fileNode, episodeMetadata, null))
            ];

            if (results.Count == 0 || !results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能识别任何文件", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop<IEnumerable<FailedTvIdentifyResult>?>(this, results);
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
    private async Task SupplementApply(CancellationToken cancellationToken)
    {
        if (Nodes.Count == 0)
            return;

        try
        {
            CancellationToken token = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, CancellationToken).Token;

            List<FailedTvIdentifyResult> results = new(32);

            SeriesSearchResult? selected = await TrySearchSeriesAsync(token);
            foreach (FailedFileNode fileNode in GetFailedFiles(Nodes))
            {
                TvParseResult parseResult = _nameParserService.ParseTv(fileNode.FullPath);
                if (Season is null && parseResult.Season is null || Episode is null && parseResult.Episode is null)
                    continue;

                EpisodeMetadata? episodeMetadata = await _metadataService.GetEpisodeAsync(new EpisodeMetadataRequest
                {
                    SeriesName = selected?.Name ?? parseResult.Title,
                    Year = selected?.FirstAirDate?.Year ?? Request?.Year ?? parseResult.Year,
                    //这里应该不会为null，上面判断了 ...应该？
                    SeasonNumber = Season ?? parseResult.Season!.Value,
                    EpisodeNumber = Episode ?? parseResult.Episode!.Value,
                    Language = "zh-cn",
                    SeriesProviderIds = selected?.ProviderIds ?? ProviderIds
                }, token: token);

                if (episodeMetadata is null)
                {
                    results.Add(new FailedTvIdentifyResult(fileNode, null, new Exception("无法获取数据")));
                    continue;
                }

                results.Add(new FailedTvIdentifyResult(fileNode, episodeMetadata, null));
            }

            if (results.Count == 0 || !results.Any(result => result.Metadata is not null))
            {
                _toastServices.Show(new Toast("未能获取到任何文件信息", NotificationType.Warning), this);
                return;
            }

            _navigationService.Pop(this, results);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "补充剧集元数据失败");
        }
    }

    [RelayCommand]
    private void CancelLoading()
    {
        if (AutoApplyCommand.IsRunning)
            AutoApplyCommand.Cancel();
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