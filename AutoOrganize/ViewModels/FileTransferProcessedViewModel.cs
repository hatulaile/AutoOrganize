using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.FileTransferBatchServices;
using AutoOrganize.Library.Services.FileTransferServices;
using AutoOrganize.Library.Services.Observers;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration]
[ViewModelConfigRegistration(typeof(FileTransferConfig))]
public sealed partial class FileTransferProcessedViewModel : ViewModelBase,
    INavigationViewModel<FileTransferProcessedOption>, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<FileTransferProcessedViewModel> _logger;
    private readonly IFileTransferBatchService _fileTransferBatchService;
    private readonly INotificationServices _notificationServices;
    private CancellationTokenSource _cancellationTokenSource = new();
    private ConcurrentBag<IFileTransferBatchInfo>? _transferBatchInfos;

    [ObservableProperty]
    public partial AvaloniaList<FileMetadataEntry>? Entries { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalProcessedCount))]
    public partial int SuccessProcessedCount { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalProcessedCount))]
    public partial int FailedProcessedCount { get; set; }

    public int TotalProcessedCount => SuccessProcessedCount + FailedProcessedCount;

    public void OnNavigatedFrom()
    {
        Entries = null;
    }

    [MemberNotNull(nameof(Entries))]
    public void OnNavigatedTo(FileTransferProcessedOption args)
    {
        _logger.LogDebug("开始处理文件");

        Entries = new AvaloniaList<FileMetadataEntry>(args.FileMetadataEntries);
        SuccessProcessedCount = 0;
        FailedProcessedCount = 0;
        Task.Run(StartProcessFileAsync);
    }

    private async Task StartProcessFileAsync()
    {
        _logger.LogDebug("开始处理文件");

        if (Entries is null)
        {
            _logger.LogWarning("Entries 为空, 前往编辑页");
            _navigationService.NavigateTo<MetadataEditViewModel>(HostScreens.Home);
            return;
        }

        _transferBatchInfos = [];
        ProcessObserver<FileTransferBatchInfo, FileTransferBatchResult, FileTransferBatchErrorInfo> observer = new();
        observer.Failure += info =>
        {
            _transferBatchInfos.Add(info);
            Dispatcher.UIThread.Post(() => FailedProcessedCount++);
        };

        observer.Success += info =>
        {
            _transferBatchInfos.Add(info);
            Dispatcher.UIThread.Post(() => SuccessProcessedCount++);
        };

        observer.Completed += result =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (result.Total <= 0)
                {
                    _logger.LogWarning("没有文件需要处理. Total: {Total}, Succeed: {Succeed}, Failed: {Failed}",
                        result.Total, result.Succeed, result.Failed);
                    _notificationServices.Show(
                        new Notification("没有文件需要处理", "没有任何文件需要处理, 很奇怪的错误!", NotificationType.Error), this);
                    _notificationServices.Show(new Notification("前往", "前往元数据编辑页"), this);
                    GoBack();
                    return;
                }

                if (result.Succeed <= 0)
                {
                    _logger.LogError("所有文件处理失败. Total: {Total}, Failed: {Failed}",
                        result.Total, result.Failed);
                    _notificationServices.Show(
                        new Notification("没有成功的文件", $"没有任何成功的文件, 失败了 {result.Failed} 个, 请查看日志!",
                            NotificationType.Error), this);
                }
                else if (result.Failed > 0)
                {
                    _logger.LogWarning("部分文件处理失败. Total: {Total}, Succeed: {Succeed}, Failed: {Failed}",
                        result.Total, result.Succeed, result.Failed);
                    _notificationServices.Show(
                        new Notification("出现错误",
                            $"总共 {result.Total} 个, {result.Succeed} 个成功, {result.Failed} 个错误. 错误请查看日志!",
                            NotificationType.Warning), this);
                }
                else
                {
                    _logger.LogInformation("所有文件处理成功. Total: {Total}, Succeed: {Succeed}",
                        result.Total, result.Succeed);
                    _notificationServices.Show(
                        new Notification("成功", $"一共处理了 {result.Succeed} 个", NotificationType.Success),
                        this);
                }

                _navigationService.NavigateTo<FileTransferResultViewModel, FileTransferResultOptions>(HostScreens.Home,
                    new FileTransferResultOptions
                    {
                        BatchInfos = _transferBatchInfos
                    });
            });
        };

        await _fileTransferBatchService.ProcessFilesAsync(Entries, observer, _cancellationTokenSource.Token);
        _logger.LogDebug("处理完成, 释放资源.");
        Dispose();
    }

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogDebug($"触发了返回方法, 返回{nameof(MetadataEditViewModel)}");
        _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
            new MetadataEditOption
            {
                IsClear = false
            });
        Dispose();
    }

    public FileTransferProcessedViewModel(INavigationService navigationService,
        ILogger<FileTransferProcessedViewModel> logger,
        IFileTransferBatchService fileTransferBatchService, INotificationServices notificationServices)
    {
        logger.LogDebug($"构建 {nameof(FileTransferProcessedViewModel)}.");
        _navigationService = navigationService;
        _logger = logger;
        _fileTransferBatchService = fileTransferBatchService;
        _notificationServices = notificationServices;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }

    ~FileTransferProcessedViewModel()
    {
        Dispose(false);
    }
}