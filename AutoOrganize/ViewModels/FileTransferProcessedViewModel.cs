using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Exceptions.NavigationExceptions;
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

    [ObservableProperty] public partial AvaloniaList<FileMetadataEntry>? Entries { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalProcessedCount))]
    public partial int SuccessProcessedCount { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalProcessedCount))]
    public partial int FailedProcessedCount { get; set; }

    public int TotalProcessedCount => SuccessProcessedCount + FailedProcessedCount;

    public FileTransferProcessedOption? NavigationParameter { get; set; }

    public void OnNavigatedFrom()
    {
        Entries = null;
    }

    [MemberNotNull(nameof(Entries))]
    public void OnNavigatedTo()
    {
        if (NavigationParameter is null) throw new NavigationParameterNullException(nameof(FileTransferProcessedViewModel), nameof(FileTransferProcessedOption));
        Entries = new AvaloniaList<FileMetadataEntry>(NavigationParameter.FileMetadataEntries);
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

        ProcessObserver<FileTransferBatchInfo, FileTransferBatchResult> observer = new();
        observer.Failure += exception =>
        {
            //todo: logger
            Dispatcher.UIThread.Post(() => FailedProcessedCount++);
        };

        observer.Success += _ => Dispatcher.UIThread.Post(() => SuccessProcessedCount++);

        observer.Completed += result =>
            Dispatcher.UIThread.Post(() =>
            {
                if (result.Total <= 0)
                {
                    _notificationServices.Show(
                        new Notification("没有文件需要处理", "没有任何文件需要处理, 很奇怪的错误!", NotificationType.Error), this);
                    _notificationServices.Show(new Notification("前往", "前往元数据编辑页"), this);
                    _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
                        new MetadataEditOption(null, null)
                        {
                            IsClear = false
                        });
                    return;
                }

                if (result.Succeed <= 0)
                {
                    _notificationServices.Show(
                        new Notification("没有成功的文件", $"没有任何成功的文件, 失败了 {result.Failed}, 请查看日志!", NotificationType.Error),
                        this);
                    _notificationServices.Show(new Notification("前往", "前往元数据编辑页"), this);
                    _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
                        new MetadataEditOption(null, null)
                        {
                            IsClear = false
                        });
                    return;
                }

                if (result.Failed > 0)
                {
                    _notificationServices.Show(
                        new Notification("出现错误",
                            $"总共 {result.Total} 个, {result.Succeed} 个成功, {result.Failed} 个错误. 错误请查看日志!",
                            NotificationType.Warning), this);
                    _notificationServices.Show(new Notification("前往", "前往元数据编辑页"), this);
                    _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
                        new MetadataEditOption(null, null)
                        {
                            IsClear = false
                        });
                    return;
                }

                _notificationServices.Show(
                    new Notification("成功", $"一共处理了 {result.Succeed} 个", NotificationType.Success),
                    this);
                _notificationServices.Show(new Notification("前往", "前往首页"), this);
                _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
            });

        await _fileTransferBatchService.ProcessFilesAsync(Entries, observer, _cancellationTokenSource.Token);
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<MetadataEditViewModel>(HostScreens.Home);
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