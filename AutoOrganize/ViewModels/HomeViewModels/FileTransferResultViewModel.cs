using System;
using System.Collections.Specialized;
using System.Linq;
using AsyncImageLoader.Loaders;
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
    private readonly IFileTransferBatchService _fileTransferBatchService;
    private readonly INotificationServices _notificationServices;
    private readonly IWindowService _windowService;
    private readonly ILogger<FileTransferResultViewModel> _logger;

    private readonly MetadataTreeRoot _metadataRoot = new();

    public AvaloniaList<IFileTransferBatchInfo> FileTransferBatchInfos { get; } = [];

    public AvaloniaList<MetadataTreeNodeBase> SelectItems { get; }

    [ObservableProperty]
    public partial HierarchicalModel<MetadataTreeNodeBase>? Model { get; set; }

    [ObservableProperty]
    public partial IMenuItemContext MenuItemContext { get; set; }

    [ObservableProperty]
    public partial FileTransferFilterType FileTransferFilterType { get; set; }

    public FileTransferResultViewModel(INavigationService navigationService,
        IFileTransferBatchService fileTransferBatchService, INotificationServices notificationServices,
        IWindowService windowService, ILogger<FileTransferResultViewModel> logger)
    {
        _navigationService = navigationService;
        _fileTransferBatchService = fileTransferBatchService;
        _notificationServices = notificationServices;
        _windowService = windowService;
        _logger = logger;

        SelectItems = [];
        SelectItems.CollectionChanged += SelectItemsOnCollectionChanged;
        MenuItemContext = new FileTransferResultMenuItemContext
        {
            SelectedItems = SelectItems,
        };
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

    partial void OnFileTransferFilterTypeChanged(FileTransferFilterType oldValue, FileTransferFilterType newValue)
    {
        _logger.LogDebug("传输结果筛选条件变更: {Filter}", newValue);

        switch (oldValue, newValue)
        {
            case (FileTransferFilterType.None, FileTransferFilterType.SuccessOnly):
                _metadataRoot.RemoveChildAndEmptyParent<FailedTransferFileNode>(static _ => true, false);
                break;
            case (FileTransferFilterType.None, FileTransferFilterType.FailedOnly):
                _metadataRoot.RemoveChildAndEmptyParent<TransferredFileNode>(static _ => true, false);
                break;
            case (FileTransferFilterType.SuccessOnly, FileTransferFilterType.FailedOnly):
            case (FileTransferFilterType.FailedOnly, FileTransferFilterType.SuccessOnly):
                _metadataRoot.ClearChildren();
                foreach (IFileTransferBatchInfo info in FileTransferBatchInfos.Where(IsVisible))
                    AddTreeNode(info);
                break;
            case (FileTransferFilterType.SuccessOnly, FileTransferFilterType.None):
                foreach (FileTransferBatchErrorInfo info in FileTransferBatchInfos.OfType<FileTransferBatchErrorInfo>())
                    AddTreeNode(info);
                break;
            case (FileTransferFilterType.FailedOnly, FileTransferFilterType.None):
                foreach (FileTransferBatchInfo info in FileTransferBatchInfos.OfType<FileTransferBatchInfo>())
                    AddTreeNode(info);
                break;
        }

        RemoveDetachedSelectedItems();
    }

    public void OnParametersChanged(FileTransferResultArgs args)
    {
        EnsureHierarchicalModel();

        if (args.IsClear)
        {
            FileTransferBatchInfos.Clear();
            _metadataRoot.ClearChildren();
            RemoveDetachedSelectedItems();
        }

        if (args.BatchInfos is not null)
        {
            if (FileTransferBatchInfos.Count == 0)
            {
                FileTransferBatchInfos.AddRange(args.BatchInfos);
                foreach (IFileTransferBatchInfo info in args.BatchInfos)
                    AddTreeNode(info);
            }
            else
            {
                foreach (IFileTransferBatchInfo info in args.BatchInfos)
                    UpsertBatchInfo(info);
            }
        }
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

    private void UpsertBatchInfo(IFileTransferBatchInfo info)
    {
        EnsureHierarchicalModel();
        string filePath = GetInfoFilePath(info);
        int index = -1;
        for (int i = 0; i < FileTransferBatchInfos.Count; i++)
        {
            if (!string.Equals(GetInfoFilePath(FileTransferBatchInfos[i]), filePath, StringComparison.Ordinal))
                continue;

            index = i;
            break;
        }

        if (index >= 0) FileTransferBatchInfos[index] = info;
        else FileTransferBatchInfos.Add(info);

        MetadataBase metadata = GetInfoMetadata(info);
        MetadataTreeNodeBase metadataNode = _metadataRoot.AddOrGetMetadata(metadata);
        metadataNode.RemoveChild<ITransferredFileNode>(x =>
            x.FullPath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase));
        RemoveDetachedSelectedItems();

        if (IsVisible(info)) AddTreeNode(info, metadataNode);
    }

    private void RemoveDetachedSelectedItems()
    {
        for (int i = SelectItems.Count - 1; i >= 0; i--)
        {
            if (!SelectItems[i].HasParent(_metadataRoot))
                SelectItems.RemoveAt(i);
        }
    }

    private void EnsureHierarchicalModel()
    {
        if (Model is not null)
            return;

        Model = new HierarchicalModel<MetadataTreeNodeBase>(new HierarchicalOptions<MetadataTreeNodeBase>
        {
            ChildrenSelector = x => x.Children,
            IsLeafSelector = x => !x.HasChildren,
            VirtualizeChildren = true,
        });
        Model.SetRoots(_metadataRoot.Children);
        _logger.LogDebug("传输结果分层模型实例初始化成功");
    }

    private void AddTreeNode(IFileTransferBatchInfo info, MetadataTreeNodeBase? metadataNode = null)
    {
        if (metadataNode is not null)
        {
            switch (info)
            {
                case FileTransferBatchInfo batchInfo:
                    metadataNode.AddChild(new TransferredFileNode(batchInfo.FilePath, batchInfo.OutputPath));
                    break;
                case FileTransferBatchErrorInfo errorInfo:
                    metadataNode.AddChild(new FailedTransferFileNode(errorInfo.FilePath, errorInfo.OutputPath,
                        errorInfo.Exception));
                    break;
                default:
                    _logger.LogWarning("未知的传输结果类型: {Type}", info.GetType().Name);
                    break;
            }

            return;
        }

        switch (info)
        {
            case FileTransferBatchInfo batchInfo:
                _metadataRoot.AddTransferFile(batchInfo.FilePath, batchInfo.OutputPath, batchInfo.Metadata);
                break;
            case FileTransferBatchErrorInfo errorInfo:
                _metadataRoot.AddFailedTransferFile(errorInfo.FilePath, errorInfo.OutputPath, errorInfo.Metadata,
                    errorInfo.Exception);
                break;
            default:
                _logger.LogWarning("未知的传输结果类型: {Type}", info.GetType().Name);
                break;
        }
    }

    private bool IsVisible(IFileTransferBatchInfo info) =>
        FileTransferFilterType switch
        {
            FileTransferFilterType.SuccessOnly => info is FileTransferBatchInfo,
            FileTransferFilterType.FailedOnly => info is FileTransferBatchErrorInfo,
            _ => true,
        };

    private static MetadataBase GetInfoMetadata(IFileTransferBatchInfo info) =>
        info switch
        {
            FileTransferBatchInfo batch => batch.Metadata,
            FileTransferBatchErrorInfo error => error.Metadata,
            _ => throw new ArgumentOutOfRangeException(nameof(info), info, null),
        };

    private static string GetInfoFilePath(IFileTransferBatchInfo info) =>
        info switch
        {
            FileTransferBatchInfo batch => batch.FilePath,
            FileTransferBatchErrorInfo error => error.FilePath,
            _ => throw new ArgumentOutOfRangeException(nameof(info), info, null),
        };
}