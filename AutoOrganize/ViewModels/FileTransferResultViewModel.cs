using System.Collections.Generic;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Services.FileTransferBatchServices;
using AutoOrganize.Models;
using AutoOrganize.Models.MetadataViewModels;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Models.MetadataViewModels.Metadata;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.ViewModels.MetadataViewModels;
using Avalonia.Collections;
using Avalonia.Controls.DataGridHierarchical;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public partial class FileTransferResultViewModel : ViewModelBase, INavigationViewModel<FileTransferResultOptions>
{
    private readonly INavigationService _navigationService;

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
        if (Model is null)
        {
            Model = new HierarchicalModel<FileMetadataBase>(new HierarchicalOptions<FileMetadataBase>
            {
                ChildrenSelector = x => x.Children,
                IsLeafSelector = x => !x.HasChildren,
                VirtualizeChildren = true,
            });
        }

        SelectedMetadata = null;
        _metadataRoot = new MetadataRoot();
        foreach (IFileTransferBatchInfo info in FileTransferBatchInfos)
        {
            switch (info)
            {
                case FileTransferBatchInfo batchInfo:
                    if (FileTransferFilterType is FileTransferFilterType.FailedOnly) break;
                    _metadataRoot.AddTransferFile(batchInfo.FilePath, batchInfo.OutputPath, batchInfo.Metadata);
                    break;
                case FileTransferBatchErrorInfo errorInfo:
                    if (FileTransferFilterType is FileTransferFilterType.SuccessOnly) break;
                    _metadataRoot.AddFailedTransferFile(errorInfo.FilePath, errorInfo.OutputPath, errorInfo.Metadata,
                        errorInfo.Exception);
                    break;
                default:
                    continue;
            }
        }

        Model.SetRoots(_metadataRoot.Children);
    }

    [RelayCommand]
    public void NavigateToSelectFilesViewModel()
    {
        _navigationService.NavigateTo<SelectFilesViewModel, IEnumerable<string>?>(HostScreens.Home, null);
    }

    [RelayCommand]
    public void NavigateToMetadataEditViewModel()
    {
        _navigationService.NavigateTo<MetadataEditViewModel, MetadataEditOption>(HostScreens.Home,
            new MetadataEditOption()
            {
                IsClear = false
            });
    }

    public FileTransferResultViewModel([FromKeyedServices(HostScreens.TransferResult)] RoutingState routingState,
        INavigationService navigationService)
    {
        _navigationService = navigationService;
        RoutingState = routingState;
        routingState.SetOwnerViewModel(this);
    }
}