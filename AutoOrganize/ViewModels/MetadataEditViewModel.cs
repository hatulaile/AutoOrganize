using System;
using System.Collections.Generic;
using System.Linq;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;
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
public sealed partial class MetadataEditViewModel : ViewModelBase, INavigationViewModel<MetadataEditOption>
{
    private readonly INavigationService _navigationService;

    private MetadataRoot _metadataRoot = new();

    private FailedMetadataRoot _failedMetadataSystemRoot = new();

    [ObservableProperty]
    public partial FileMetadataBase? SelectedMetadata { get; set; }

    public IReadOnlyList<HierarchicalNode<FileMetadataBase>>? Rows => Model?.Flattened;

    public AvaloniaList<FileMetadataBase> Source { get; } = [];

    public HierarchicalModel<FileMetadataBase>? Model { get; private set; }

    public RoutingState RoutingState { get; }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        _navigationService.NavigateTo<FileTransferProcessedViewModel, FileTransferProcessedOption>(HostScreens.Home,
            new FileTransferProcessedOption(GetAllFileMetadataEntries(_metadataRoot)));
    }

    [RelayCommand]
    public void Back()
    {
        _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
    }

    public bool CanNext()
    {
        return Source.Any(x => x is not FailedMetadataRoot);
    }

    partial void OnSelectedMetadataChanged(FileMetadataBase? value)
    {
        switch (value)
        {
            case IFileMetadata<MetadataBase> metadata:
                _navigationService.NavigateTo<MetadataViewModel, MetadataBase>
                    (HostScreens.MetadataEdit, metadata.Metadata);
                break;
            case FileModel fileMetadata:
                _navigationService.NavigateTo<FileMetadataViewModel, FileModel>
                    (HostScreens.MetadataEdit, fileMetadata);
                break;
            case FailedDirectoryModel failedDirectoryMetadata:
                _navigationService.NavigateTo<FailedDirectoryMetadataViewModel, FailedDirectoryModel>(
                    HostScreens.MetadataEdit, failedDirectoryMetadata);
                break;
            case FailedMetadataRoot failedFileMetadataRoot:
                _navigationService.NavigateTo<FailedFileMetadataRootViewModel, FailedMetadataRoot>(
                    HostScreens.MetadataEdit, failedFileMetadataRoot);
                break;
            case FailedFile failedMetadata:
                _navigationService.NavigateTo<FailedFileMetadataViewModel, FailedFile>
                    (HostScreens.MetadataEdit, failedMetadata);
                break;
            default:
                _navigationService.Clear(HostScreens.MetadataEdit);
                break;
        }
    }

    public void OnParametersChanged(MetadataEditOption option)
    {
        CreateSource(option);
    }

    private void CreateSource(MetadataEditOption options)
    {
        if (options.IsClear)
        {
            _metadataRoot = new MetadataRoot();
            _failedMetadataSystemRoot = new FailedMetadataRoot();
            Source.Clear();
        }

        if (options.FileProcessResultInfos is null || options.FileProcessOptions is null)
            return;

        foreach (FileMetadataProcessingResult result in options.FileProcessResultInfos)
        {
            if (result.IsSuccess)
            {
                try
                {
                    _metadataRoot.AddFile(result.Metadata, result.FilePath);
                }
                catch (Exception e)
                {
                    _failedMetadataSystemRoot.AddOrGetFailedMetadata(result.FilePath, e);
                }
            }
            else
            {
                _failedMetadataSystemRoot.AddOrGetFailedMetadata(result.FilePath, result.Error,
                    options.FileProcessOptions.Value);
            }
        }

        if (options.IsClear)
        {
            if (_failedMetadataSystemRoot.Children.Count > 0)
                Source.Add(_failedMetadataSystemRoot);
            Source.AddRange(_metadataRoot.Children);
        }

        if (Model is null)
        {
            Model = new HierarchicalModel<FileMetadataBase>(new HierarchicalOptions<FileMetadataBase>
            {
                ChildrenSelector = x => x.Children,
                IsLeafSelector = x => !x.HasChildren,
                VirtualizeChildren = true,
            });

            Model.SetRoots(Source);
        }
    }

    private static IEnumerable<FileMetadataEntry> GetAllFileMetadataEntries(FileMetadataBase fileMetadata)
    {
        if (fileMetadata is FileEpisodeMetadata episodeMetadata)
        {
            foreach (FileMetadataBase episodeMetadataChild in episodeMetadata.Children)
            {
                if (episodeMetadataChild is not FileModel metadataChild)
                    continue;
                yield return new FileMetadataEntry(metadataChild.FullPath, episodeMetadata.Metadata);
            }
        }

        if (!fileMetadata.HasChildren) yield break;
        foreach (FileMetadataBase fileMetadataChildren in fileMetadata.Children)
        {
            foreach (var allFileMetadataEntry in GetAllFileMetadataEntries(fileMetadataChildren))
            {
                yield return allFileMetadataEntry;
            }
        }
    }

    public MetadataEditViewModel(
        INavigationService navigationViewModel,
        [FromKeyedServices(HostScreens.MetadataEdit)]
        RoutingState routingState)
    {
        _navigationService = navigationViewModel;
        RoutingState = routingState;
        RoutingState.SetOwnerViewModel(this);
    }
}