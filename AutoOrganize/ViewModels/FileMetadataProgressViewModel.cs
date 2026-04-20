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
using AutoOrganize.Utils;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoOrganize.ViewModels;

public sealed partial class FileMetadataProgressViewModel : ViewModelBase, INavigationViewModel<FileProcessOptions?>,
    IDisposable, IAsyncDisposable
{
    public const int PROGRESS_MAX = 128;
    public static string ProgressMax => PROGRESS_MAX.ToString();

    private readonly INameParserManager _nameParserManager;
    private readonly IMetadataManager _metadataManager;
    private readonly INavigationService _navigationService;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _progressSemaphore = new(PROGRESS_MAX, PROGRESS_MAX);
    public FileProcessOptions? NavigationParameter { get; set; }

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
        _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
        Dispose();
    }

    private async Task StartAsync(FileProcessOptions options, CancellationToken token)
    {
        bool hasFiles = false;
        var tcs = new TaskCompletionSource();

        foreach (string file in GetFiles(options.FilesPaths))
        {
            token.ThrowIfCancellationRequested();
            if (!VideoUtils.IsVideoFile(file)) continue;

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

        if (Results.Count > 0)
        {
            _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
                new MetadataEditOption
                {
                    FileProcessResultInfos = Results,
                    FileProcessOptions = options
                });
            return;
        }

        _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
    }

    private async Task ProgressAndAddFileAsync(string filePath, MetadataType type, CancellationToken token)
    {
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
            // Ignore
        }
        catch (Exception exception)
        {
            Results.Add(new FileMetadataProcessingResult(filePath, exception));
        }
    }

    private async Task<FileMetadataProcessingResult> ProcessMovieFileAsync(string filePath, CancellationToken token)
    {
        var movieParse = _nameParserManager.ParseMovie(filePath);
        if (!movieParse.IsComplete())
            return new FileMetadataProcessingResult(filePath,
                new MetadataParseException(filePath, "movie", "无法解析成一个可用的电影元数据"));

        MetadataBase? metadata =
            await _metadataManager.SearchMovieSingleAsync(new SearchQuery(movieParse.Title, movieParse.Year), token);

        if (metadata is null)
            throw new MetadataNotFoundException(filePath, "movie", "未找到匹配的电影元数据");
        return new FileMetadataProcessingResult(filePath, metadata);
    }

    private async Task<FileMetadataProcessingResult> ProcessTvFileAsync(string filePath, CancellationToken token)
    {
        var tvParse = _nameParserManager.ParseTv(filePath);
        tvParse.Season ??= 1;
        if (!tvParse.IsComplete())
            return new FileMetadataProcessingResult(filePath,
                new MetadataParseException(filePath, "tv", "无法解析成一个可用的电视元数据"));
        MetadataBase? metadata =
            await _metadataManager.SearchEpisodeAsync(new SearchQuery(tvParse.Title, tvParse.Year),
                tvParse.Season.Value, tvParse.Episode.Value, token);

        if (metadata is null)
            throw new MetadataNotFoundException(filePath, "tv", "未找到匹配的电视元数据");
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

    public void OnNavigatedTo()
    {
        ArgumentNullException.ThrowIfNull(NavigationParameter);
        _ = StartAsync(NavigationParameter.Value, _cancellationTokenSource.Token);
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
        INavigationService navigationService)
    {
        _nameParserManager = nameParserManager;
        _metadataManager = metadataManager;
        _navigationService = navigationService;

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