using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Exceptions;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.NameParsers;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.Utils;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration]
public sealed partial class FileMetadataProgressViewModel : ViewModelBase, INavigationViewModel<FileProcessOptions>,
    IDisposable, IAsyncDisposable
{
    public const int PROGRESS_MAX = 128;
    public static string ProgressMax => PROGRESS_MAX.ToString();

    private readonly INameParserManager _nameParserManager;
    private readonly IMetadataManager _metadataManager;
    private readonly INavigationService _navigationService;
    private readonly INotificationServices _notificationServices;
    private readonly ILogger<FileMetadataProgressViewModel> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _progressSemaphore = new(PROGRESS_MAX, PROGRESS_MAX);

    [ObservableProperty]
    public partial bool IsFileEnumerationCompleted { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCount))]
    public partial int CurrentProgress { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCount))]
    [NotifyPropertyChangedFor(nameof(TotalProgress))]
    public partial int SuccessCount { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCount))]
    [NotifyPropertyChangedFor(nameof(TotalProgress))]
    public partial int FailedCound { get; set; }

    public int TotalProgress => SuccessCount + FailedCound;
    public int TotalCount => SuccessCount + FailedCound + CurrentProgress;

    public AvaloniaList<FileMetadataProcessingResult> Results { get; } = [];

    [RelayCommand]
    public void GoBack()
    {
        _logger.LogDebug($"触发返回方法, 前往 {nameof(SelectFilesViewModel)}.");
        _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
        Dispose();
    }

    private async Task StartAsync(FileProcessOptions options, CancellationToken token)
    {
        _logger.LogDebug($"触发{nameof(StartAsync)}方法.");

        bool hasFiles = false;
        var tcs = new TaskCompletionSource();

        foreach (string file in GetFiles(options.FilesPaths))
        {
            token.ThrowIfCancellationRequested();
            if (!VideoUtils.IsVideoFile(file))
            {
                _logger.LogTrace("{file} 不是一个视频, 跳过!", file);
                continue;
            }

            await _progressSemaphore.WaitAsync(token);
            CurrentProgress++;
            hasFiles = true;
            _ = ProgressAndAddFileAsync(file, options.Type, token)
                .ContinueWith(_ =>
                {
                    _progressSemaphore.Release();
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        CurrentProgress--;
                        if (CurrentProgress == 0 && IsFileEnumerationCompleted)
                            tcs.SetResult();
                    });
                }, token);
        }

        IsFileEnumerationCompleted = true;
        if (token.IsCancellationRequested) tcs.SetCanceled(token);
        else if (!hasFiles || CurrentProgress == 0) tcs.SetResult();
        await tcs.Task.WaitAsync(token);

        if (SuccessCount > 0)
        {
            _logger.LogInformation("源数据处理完成, 成功: {successCount}, 失败: {failedCound}", SuccessCount, FailedCound);
            _notificationServices.Show(
                FailedCound == 0
                    ? new Notification("处理结果", $"成功 {SuccessCount} 个, 无一失败.", NotificationType.Success)
                    : new Notification("处理结果", $"成功 {SuccessCount} 个, 失败 {FailedCound} 个.", NotificationType.Warning),
                this);


            _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
                new MetadataEditOption
                {
                    FileProcessResultInfos = Results,
                    FileProcessOptions = options
                });
            return;
        }

        _logger.LogWarning("源数据处理全部失败: {failedCound}", FailedCound);
        _notificationServices.Show(new Notification("处理结果", "没有任何成功的文件", NotificationType.Error), this);
        _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
    }

    private async Task ProgressAndAddFileAsync(string filePath, MetadataType type, CancellationToken token)
    {
        _logger.LogDebug("开始处理文件: {FilePath}, 类型: {Type}", filePath, type);

        try
        {
            FileMetadataProcessingResult processingResult = type switch
            {
                MetadataType.Movie => await ProcessMovieFileAsync(filePath, token),
                MetadataType.Tv => await ProcessTvFileAsync(filePath, token),
                MetadataType.None => throw new NotSupportedException("Metadata type cannot be None"),
                _ => throw new NotSupportedException($"Unsupported metadata type: {type}")
            };

            Results.Add(processingResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("文件处理被取消: {FilePath}", filePath);
            // Ignore
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "文件处理失败: {FilePath}, 类型: {Type}", filePath, type);
            Results.Add(new FileMetadataProcessingResult(filePath, exception));
        }
    }

    private async Task<FileMetadataProcessingResult> ProcessMovieFileAsync(string filePath, CancellationToken token)
    {
        var movieParse = _nameParserManager.ParseMovie(filePath);
        if (!movieParse.IsComplete())
            throw new MetadataParseException(filePath, "movie", "无法解析成一个可用的电影元数据");

        var metadata =
            await _metadataManager.SearchMovieSingleAsync(new SearchQuery(movieParse.Title, movieParse.Year), token);

        if (metadata is null)
            throw new MetadataNotFoundException(filePath, "movie", "未找到匹配的电影元数据");

        _logger.LogDebug("电影元数据匹配成功: {FilePath} -> {Title}", filePath, metadata.Name);
        return new FileMetadataProcessingResult(filePath, metadata);
    }

    private async Task<FileMetadataProcessingResult> ProcessTvFileAsync(string filePath, CancellationToken token)
    {
        var tvParse = _nameParserManager.ParseTv(filePath);

        if (tvParse.Season is null)
        {
            _logger.LogWarning("文件 {FilePath} 未解析到 Season，使用默认第一季.", filePath);
            tvParse.Season ??= 1;
        }

        if (!tvParse.IsComplete())
            throw new MetadataParseException(filePath, "tv", "无法解析成一个可用的电视元数据");

        var metadata =
            await _metadataManager.SearchEpisodeAsync(new SearchQuery(tvParse.Title, tvParse.Year),
                tvParse.Season.Value, tvParse.Episode.Value, token);

        if (metadata is null)
            throw new MetadataNotFoundException(filePath, "tv", "未找到匹配的电视元数据");

        _logger.LogDebug("电视元数据匹配成功: {SeriesName} ({Year}) - {SeasonName} - {EpisodeName}", metadata.Series?.Name, metadata.AirDate?.Year,
            metadata.Season?.Name, metadata.Name);
        return new FileMetadataProcessingResult(filePath, metadata);
    }

    private static IEnumerable<string> GetFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    yield return file;
                }
            }
            else if (File.Exists(path))
            {
                yield return Path.GetFullPath(path);
            }
        }
    }

    public void OnNavigatedTo(FileProcessOptions args)
    {
        _logger.LogDebug("导航到文件处理页，类型: {Type}", args.Type);
        _ = StartAsync(args, _cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        _progressSemaphore.Dispose();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _progressSemaphore.Dispose();
        }
    }

    ~FileMetadataProgressViewModel()
    {
        Dispose(false);
    }

    public FileMetadataProgressViewModel(INameParserManager nameParserManager, IMetadataManager metadataManager,
        INavigationService navigationService, INotificationServices notificationServices,
        ILogger<FileMetadataProgressViewModel> logger)
    {
        _nameParserManager = nameParserManager;
        _metadataManager = metadataManager;
        _navigationService = navigationService;
        _notificationServices = notificationServices;
        _logger = logger;

        Results.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is null)
                return;

            foreach (FileMetadataProcessingResult info in args.NewItems)
            {
                if (info.IsSuccess) SuccessCount++;
                else FailedCound++;
            }
        };
    }
}