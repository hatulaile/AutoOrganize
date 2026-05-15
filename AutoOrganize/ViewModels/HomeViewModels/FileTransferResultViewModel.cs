using System.Collections.Generic;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Services.FileTransferBatchServices;
using AutoOrganize.Models;
using AutoOrganize.Models.MetadataViewModels;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Models.MetadataViewModels.Metadata;
using AutoOrganize.Models.Options;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.ViewModels.HomeViewModels.MetadataViewModels;
using Avalonia.Collections;
using Avalonia.Controls.DataGridHierarchical;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public partial class FileTransferResultViewModel : ViewModelBase, INavigationViewModel<FileTransferResultOptions>
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<FileTransferResultViewModel> _logger;

    private MetadataRoot? _metadataRoot;

    public AvaloniaList<IFileTransferBatchInfo> FileTransferBatchInfos { get; } = [];

    public RoutingState RoutingState { get; }

    [ObservableProperty]
    public partial HierarchicalModel<FileMetadataBase>? Model { get; set; }

    [ObservableProperty]
    public partial FileMetadataBase? SelectedMetadata { get; set; }

    [ObservableProperty]
    public partial FileTransferFilterType FileTransferFilterType { get; set; }

    partial void OnFileTransferFilterTypeChanged(FileTransferFilterType value)
    {
        _logger.LogDebug("传输结果筛选条件变更: {Filter}", value);
        CreateHierarchicalModel();
    }

    public void OnParametersChanged(FileTransferResultOptions args)
    {
        if (args.IsClear) FileTransferBatchInfos.Clear();

        if (args.BatchInfos is not null)
            FileTransferBatchInfos.AddRange(args.BatchInfos);

        CreateHierarchicalModel();
    }

    partial void OnSelectedMetadataChanged(FileMetadataBase? value)
    {
        if (value is null)
        {
            _logger.LogDebug("取消选中传输结果项");
            return;
        }

        _logger.LogDebug("选中传输结果项: {Type} - {Name}", value.GetType().Name, value.Title);
        if (value is IFileMetadata fileMetadata)
        {
            _navigationService.NavigateTo<MetadataViewModel, MetadataBase>(HostScreens.TransferResult,
                fileMetadata.Metadata);
            return;
        }

        switch (value)
        {
            case TransferFileModel transferFileModel:
                _navigationService.NavigateTo<TransferFileViewModel, TransferFileModel>
                    (HostScreens.TransferResult, transferFileModel);
                break;

            case FailedTransferFileModel failedTransferFileModel:
                _navigationService.NavigateTo<FailedTransferFileViewModel, FailedTransferFileModel>
                    (HostScreens.TransferResult, failedTransferFileModel);
                break;
        }
    }

    public void CreateHierarchicalModel()
    {
        _logger.LogDebug("开始构建传输结果分层模型，当前筛选: {Filter}, 批次总数: {Count}",
            FileTransferFilterType, FileTransferBatchInfos.Count);

        if (Model is null)
        {
            Model = new HierarchicalModel<FileMetadataBase>(new HierarchicalOptions<FileMetadataBase>
            {
                ChildrenSelector = x => x.Children,
                IsLeafSelector = x => !x.HasChildren,
                VirtualizeChildren = true,
            });
            _logger.LogDebug("分层模型实例初始化成功");
        }

        SelectedMetadata = null;
        _metadataRoot = new MetadataRoot();
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
        _navigationService.NavigateTo<SelectFilesViewModel, IEnumerable<string>?>(HostScreens.Home, null);
    }

    [RelayCommand]
    public void NavigateToMetadataEditViewModel()
    {
        _logger.LogDebug("导航到元数据编辑页面");
        _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
            new MetadataEditOption()
            {
                IsClear = false
            });
    }

    public FileTransferResultViewModel([FromKeyedServices(HostScreens.TransferResult)] RoutingState routingState,
        INavigationService navigationService, ILogger<FileTransferResultViewModel> logger)
    {
        _navigationService = navigationService;
        _logger = logger;
        RoutingState = routingState;
        routingState.SetOwnerViewModel(this);
    }
}