using System;
using System.Collections.Generic;
using System.Linq;
using AutoOrganize.Exceptions.NavigationExceptions;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Models;
using AutoOrganize.Models.FileMetadataModels;
using AutoOrganize.Models.FileMetadataModels.FailedMetadata;
using AutoOrganize.Models.FileMetadataModels.SuccessMetadata;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Utils;
using AutoOrganize.ViewModels.FileMetadataViewModels;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class MetadataEditViewModel : ViewModelBase, INavigationViewModel<MetadataEditOption>
{
    private readonly INavigationService _navigationService;
    public MetadataEditOption? NavigationParameter { get; set; }

    private FileMetadataRoot _fileMetadataRoot = new();

    private FailedFileMetadataRoot _failedFileMetadataSystemRoot = new();

    [ObservableProperty] private FileMetadataBase? _selectedMetadata;

    private readonly AvaloniaList<FileMetadataBase> _source = [];

    public HierarchicalTreeDataGridSource<FileMetadataBase>? Source { get; private set; }

    public RoutingState RoutingState { get; }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        _navigationService.NavigateTo<FileTransferProcessedViewModel, FileTransferProcessedOption>(HostScreens.Home,
            new FileTransferProcessedOption(GetAllFileMetadataEntries(_fileMetadataRoot)));
    }

    [RelayCommand]
    public void Back()
    {
        _navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
    }

    public bool CanNext()
    {
        return _source.Any(x => x is not FailedFileMetadataRoot);
    }

    partial void OnSelectedMetadataChanged(FileMetadataBase? value)
    {
        switch (value)
        {
            case IFileMetadata<MetadataBase> metadata:
                _navigationService.NavigateTo<SuccessMetadataViewModel, MetadataBase>
                    (HostScreens.MetadataEdit, metadata.Metadata);
                break;
            case FileMetadata fileMetadata:
                _navigationService.NavigateTo<FileMetadataViewModel, FileMetadata>
                    (HostScreens.MetadataEdit, fileMetadata);
                break;
            case FailedDirectoryMetadata failedDirectoryMetadata:
                _navigationService.NavigateTo<FailedDirectoryMetadataViewModel, FailedDirectoryMetadata>(
                    HostScreens.MetadataEdit, failedDirectoryMetadata);
                break;
            case FailedFileMetadataRoot failedFileMetadataRoot:
                _navigationService.NavigateTo<FailedFileMetadataRootViewModel, FailedFileMetadataRoot>(
                    HostScreens.MetadataEdit, failedFileMetadataRoot);
                break;
            case FailedFileMetadata failedMetadata:
                _navigationService.NavigateTo<FailedMetadataViewModel, FailedFileMetadata>
                    (HostScreens.MetadataEdit, failedMetadata);
                break;
            default:
                _navigationService.Clear(HostScreens.MetadataEdit);
                break;
        }
    }

    public void OnNavigatingTo()
    {
        if (NavigationParameter is null)
            throw new NavigationParameterNullException(nameof(MetadataEditViewModel), nameof(MetadataEditOption));
        CreateSource(NavigationParameter);
    }

    private void CreateSource(MetadataEditOption options)
    {
        if (options.IsClear)
        {
            _fileMetadataRoot = new FileMetadataRoot();
            _failedFileMetadataSystemRoot = new FailedFileMetadataRoot();
            _source.Clear();
        }

        if (options.FileProcessResultInfos is null || options.FileProcessOptions is null)
            return;

        foreach (FileMetadataProcessingResult result in options.FileProcessResultInfos)
        {
            if (result.IsSuccess)
            {
                try
                {
                    _fileMetadataRoot.AddFile(result);
                }
                catch (Exception e)
                {
                    _failedFileMetadataSystemRoot.AddOrGetFailedMetadata(result.FilePath, e);
                }
            }
            else
            {
                _failedFileMetadataSystemRoot.AddOrGetFailedMetadata(result, options.FileProcessOptions.Value);
            }
        }

        if (_failedFileMetadataSystemRoot.Children.Count > 0)
            _source.Add(_failedFileMetadataSystemRoot);

        _source.AddRange(_fileMetadataRoot.Children);

        Source = new HierarchicalTreeDataGridSource<FileMetadataBase>(_source)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<FileMetadataBase>(
                    new TextColumn<FileMetadataBase, string?>("Name", x => x.Title,
                        options: new TextColumnOptions<FileMetadataBase>
                            { TextAlignment = TextAlignment.Center }),
                    x => x.Children, x => x.HasChildren),

                new TextColumn<FileMetadataBase, string?>("Subtitle",
                    x => FileMetadataTreeUtils.IfHasSubtitleGetSubtitle(x),
                    options: new TextColumnOptions<FileMetadataBase>
                        { TextAlignment = TextAlignment.Center }),

                new TextColumn<FileMetadataBase, string?>("Full Path",
                    x => FileMetadataTreeUtils.IfHasFullPathGetFullPath(x),
                    options: new TextColumnOptions<FileMetadataBase>
                        { TextAlignment = TextAlignment.Center }),

                new TextColumn<FileMetadataBase, string>("Type",
                    x => FileMetadataTreeUtils.GetTypeDisplayName(x.Type),
                    options: new TextColumnOptions<FileMetadataBase>
                        { TextAlignment = TextAlignment.Center }),

                new TextColumn<FileMetadataBase, string?>("Error",
                    x => FileMetadataTreeUtils.IfHasExceptionGetMessage(x),
                    options: new TextColumnOptions<FileMetadataBase>
                        { TextAlignment = TextAlignment.Center, }),
            }
        };


        ((TreeDataGridRowSelectionModel<FileMetadataBase>)Source.Selection!).SelectionChanged += (_, args) =>
        {
            if (args.SelectedItems.Count > 0)
            {
                SelectedMetadata = args.SelectedItems[0];
                return;
            }

            SelectedMetadata = null;
        };
    }

    private static IEnumerable<FileMetadataEntry> GetAllFileMetadataEntries(FileMetadataBase fileMetadata)
    {
        if (fileMetadata is FileEpisodeMetadata episodeMetadata)
        {
            foreach (FileMetadataBase episodeMetadataChild in episodeMetadata.Children)
            {
                if (episodeMetadataChild is not FileMetadata metadataChild)
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