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
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration]
[ViewModelConfigRegistration(typeof(FileTransferConfig))]
public sealed partial class FileTransferProcessedViewModel : ViewModelBase,
    INavigationViewModel<FileTransferProcessedOption>, IDisposable
{
    private readonly INavigationService _navigationService;
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
    public void OnParametersChanged(FileTransferProcessedOption args)
    {
        Entries = new AvaloniaList<FileMetadataEntry>(args.FileMetadataEntries);
        SuccessProcessedCount = 0;
        FailedProcessedCount = 0;
        Task.Run(StartProcessFileAsync);
    }

    private async Task StartProcessFileAsync()
    {
        if (Entries is null)
        {
            _navigationService.NavigateTo<MetadataEditViewModel>(HostScreens.Home);
            return;
        }

        _transferBatchInfos = [];
        ProcessObserver<FileTransferBatchInfo, FileTransferBatchResult, FileTransferBatchErrorInfo> observer = new();
        observer.Failure += info =>
        {
            //todo: logger
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
            _transferBatchInfos.Add(result);
            Dispatcher.UIThread.Post(() =>
            {
                if (result.Total <= 0)
                {
                    _notificationServices.Show(
                        new Notification("没有文件需要处理", "没有任何文件需要处理, 很奇怪的错误!", NotificationType.Error), this);
                    _notificationServices.Show(new Notification("前往", "前往元数据编辑页"), this);
                    GoBack();
                    return;
                }

                if (result.Succeed <= 0)
                {
                    _notificationServices.Show(
                        new Notification("没有成功的文件", $"没有任何成功的文件, 失败了 {result.Failed} 个, 请查看日志!",
                            NotificationType.Error),
                        this);
                }
                else if (result.Failed > 0)
                {
                    _notificationServices.Show(
                        new Notification("出现错误",
                            $"总共 {result.Total} 个, {result.Succeed} 个成功, {result.Failed} 个错误. 错误请查看日志!",
                            NotificationType.Warning), this);
                }
                else
                {
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
        Dispose();
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
            new MetadataEditOption
            {
                IsClear = false
            });
        Dispose();
    }

    public FileTransferProcessedViewModel(INavigationService navigationService,
        IFileTransferBatchService fileTransferBatchService, INotificationServices notificationServices)
    {
        _navigationService = navigationService;
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