using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using Avalonia.Collections;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class SelectFilesViewModel : ViewModelBase, INavigationViewModel<IEnumerable<string>?>
{
    private readonly INavigationService _navigationService;
    private readonly IStorageServices _storageProvider;

    [ObservableProperty]
    public partial MetadataType SelectedMetadataType { get; set; } = MetadataType.Tv;

    public AvaloniaList<string> Source { get; } = [];

    [ObservableProperty]
    public partial AvaloniaList<string> SelectionItems { get; set; }

    [RelayCommand]
    public async Task AddFiles()
    {
        foreach (IStorageFile storageFile in await _storageProvider.GetSelectFilesAsync(new FilePickerOpenOptions()
                 {
                     AllowMultiple = true,
                     FileTypeFilter = [FilePickerFileTypes.All],
                     Title = "选择添加的文件"
                 }))
        {
            string? localPath = storageFile.TryGetLocalPath();
            if (localPath is null)
                continue;
            Source.Add(localPath);
        }
    }

    [RelayCommand]
    public async Task AddDirectory()
    {
        foreach (IStorageFolder storageFolder in await _storageProvider.GetSelectFoldersAsync(
                     new FolderPickerOpenOptions()
                     {
                         AllowMultiple = true,
                         Title = "选择添加的文件夹"
                     }))
        {
            string? localPath = storageFolder.TryGetLocalPath();
            if (localPath is null)
                continue;
            Source.Add(localPath);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    public void Remove()
    {
        foreach (string item in SelectionItems.ToArray())
        {
            Source.Remove(item);
        }
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    public void Clear() => Source.Clear();

    [RelayCommand]
    public void DropFiles(IEnumerable<IStorageItem> files)
    {
        foreach (IStorageItem item in files)
        {
            string? path = item.TryGetLocalPath();
            if (path is null) continue;
            Source.Add(path);
        }
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        _navigationService.NavigateTo<FileMetadataProgressViewModel, FileProcessOptions>(
            HostScreens.Home, new FileProcessOptions(SelectedMetadataType, Source));
    }

    public bool CanRemove() => SelectionItems.Count > 0;

    public bool CanClear() => Source.Count > 0;

    public bool CanNext() => Source.Count > 0;

    public void OnNavigatingTo(IEnumerable<string>? strings)
    {
        if (strings is null) return;
        Source.Clear();
        Source.AddRange(strings);
    }

    public SelectFilesViewModel(INavigationService navigationService, IStorageServices storageProvider)
    {
        _navigationService = navigationService;
        _storageProvider = storageProvider;
        Source.CollectionChanged += (_, _) =>
        {
            ClearCommand.NotifyCanExecuteChanged();
            RemoveCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        };

        SelectionItems = [];
        SelectionItems.CollectionChanged += (_, _) => RemoveCommand.NotifyCanExecuteChanged();
    }
}