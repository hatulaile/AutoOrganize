using System.Collections.Specialized;
using AsyncImageLoader.Loaders;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.FileTransferBatchServices;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Models;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Models.MetadataNodes.Metadata;
using AutoOrganize.Models.Args;
using AutoOrganize.Models.MenuItemViewModelContext;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.Services.WindowManagers;
using AutoOrganize.ViewModels.Abstractions;
using AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;
using Avalonia.Collections;
using Avalonia.Controls.DataGridHierarchical;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public partial class FileTransferResultViewModel : SubNavigateViewModelBase,
    INavigationViewModel<FileTransferResultArgs>
{
    private readonly INavigationService _navigationService;
    private readonly ILauncherServices _launcherServices;
    private readonly IFileTransferBatchService _fileTransferBatchService;
    private readonly IClipboardServices _clipboardServices;
    private readonly IFileConfigManager _fileConfigManager;
    private readonly INotificationServices _notificationServices;
    private readonly IWindowService _windowService;
    private readonly ILogger<FileTransferResultViewModel> _logger;
    private readonly FileTransferResultMenuItemContext _menuItemContext;

    private MetadataTreeRoot? _metadataRoot;

    public AvaloniaList<IFileTransferBatchInfo> FileTransferBatchInfos { get; } = [];

    public AvaloniaList<MetadataTreeNodeBase> SelectItems { get; }

    [ObservableProperty]
    public partial HierarchicalModel<MetadataTreeNodeBase>? Model { get; set; }

    [ObservableProperty]
    public partial IMenuItemContext MenuItemContext { get; set; }

    [ObservableProperty]
    public partial FileTransferFilterType FileTransferFilterType { get; set; }

    public FileTransferResultViewModel(INavigationService navigationService,
        ILauncherServices launcherServices, IFileTransferBatchService fileTransferBatchService,
        IClipboardServices clipboardServices, IFileConfigManager fileConfigManager,
        INotificationServices notificationServices, IWindowService windowService,
        ILogger<FileTransferResultViewModel> logger)
    {
        _navigationService = navigationService;
        _launcherServices = launcherServices;
        _fileTransferBatchService = fileTransferBatchService;
        _clipboardServices = clipboardServices;
        _fileConfigManager = fileConfigManager;
        _notificationServices = notificationServices;
        _windowService = windowService;
        _logger = logger;

        SelectItems = [];
        SelectItems.CollectionChanged += SelectItemsOnCollectionChanged;
        MenuItemContext = _menuItemContext = new FileTransferResultMenuItemContext
        {
            SelectedItems = SelectItems,
        };
    }

    partial void OnFileTransferFilterTypeChanged(FileTransferFilterType value)
    {
        _logger.LogDebug("传输结果筛选条件变更: {Filter}", value);
        CreateHierarchicalModel();
    }

    public void OnParametersChanged(FileTransferResultArgs args)
    {
        if (args.IsClear) FileTransferBatchInfos.Clear();

        if (args.BatchInfos is not null)
            FileTransferBatchInfos.AddRange(args.BatchInfos);

        CreateHierarchicalModel();
    }

    private void SelectItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not { Count: > 0 })
        {
            _navigationService.Clear(RoutingState);
            return;
        }

        var selectedItem = (MetadataTreeNodeBase?)e.NewItems[0];
        if (selectedItem is null)
            return;

        _logger.LogDebug("选中传输结果项: {Type} - {Name}", selectedItem.GetType().Name, selectedItem.Title);
        if (selectedItem is IFileMetadata fileMetadata)
        {
            _navigationService.Replace<MetadataViewModel, MetadataBase>(RoutingState, fileMetadata.Metadata);
            return;
        }

        switch (selectedItem)
        {
            case TransferredFileNode transferFileModel:
                _navigationService.Replace<TransferredFileViewModel, TransferredFileNode>
                    (RoutingState, transferFileModel);
                break;

            case FailedTransferFileNode failedTransferFileModel:
                _navigationService.Replace<FailedTransferFileViewModel, FailedTransferFileNode>
                    (RoutingState, failedTransferFileModel);
                break;
        }
    }

    public void CreateHierarchicalModel()
    {
        _logger.LogDebug("开始构建传输结果分层模型，当前筛选: {Filter}, 批次总数: {Count}",
            FileTransferFilterType, FileTransferBatchInfos.Count);

        if (Model is null)
        {
            Model = new HierarchicalModel<MetadataTreeNodeBase>(new HierarchicalOptions<MetadataTreeNodeBase>
            {
                ChildrenSelector = x => x.Children,
                IsLeafSelector = x => !x.HasChildren,
                VirtualizeChildren = true,
            });
            _logger.LogDebug("分层模型实例初始化成功");
        }

        SelectItems.Clear();
        _metadataRoot = new MetadataTreeRoot();
        int successCount = 0, failedCount = 0;
        foreach (IFileTransferBatchInfo info in FileTransferBatchInfos)
        {
            switch (info)
            {
                case FileTransferBatchInfo batchInfo:
                    if (FileTransferFilterType is FileTransferFilterType.FailedOnly) break;
                    _metadataRoot.AddTransferFile(batchInfo.FilePath, batchInfo.OutputPath, batchInfo.Metadata);
                    successCount++;
                    break;
                case FileTransferBatchErrorInfo errorInfo:
                    if (FileTransferFilterType is FileTransferFilterType.SuccessOnly) break;
                    _metadataRoot.AddFailedTransferFile(errorInfo.FilePath, errorInfo.OutputPath, errorInfo.Metadata,
                        errorInfo.Exception);
                    failedCount++;
                    break;
                default:
                    _logger.LogWarning("未知的传输结果类型: {Type}", info.GetType().Name);
                    break;
            }
        }

        _logger.LogDebug(
            "传输结果分层模型构建完成: 成功 {Success}, 失败 {Failed}", successCount, failedCount);
        Model.SetRoots(_metadataRoot.Children);
    }

    [RelayCommand]
    public void NavigateToSelectFilesViewModel()
    {
        _logger.LogDebug("导航到文件选择页面");
        _navigationService.Replace<SelectFilesViewModel>(this);

        if (AsyncImageLoader.ImageLoader.AsyncImageLoader is RamCachedWebImageLoader ram)
            ram.ClearRamCache();
    }

    [RelayCommand]
    public void NavigateToMetadataEditViewModel()
    {
        _logger.LogDebug("导航到元数据编辑页面");
        _navigationService.Replace<MetadataEditorViewModel, MetadataEditArgs>(this,
            new MetadataEditArgs()
            {
                IsClear = false
            });
    }
}