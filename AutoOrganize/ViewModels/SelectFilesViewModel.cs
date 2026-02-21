using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class SelectFilesViewModel : ViewModelBase, INavigationViewModel<IEnumerable<string>?>
{
    public IEnumerable<string>? NavigationParameter { get; set; }

    private readonly INavigationService _navigationService;
    private readonly IStorageServices _storageProvider;

    [ObservableProperty]
    private MetadataType _selectedMetadataType = MetadataType.Tv;

    private readonly AvaloniaList<string> _origin = [];

    public FlatTreeDataGridSource<string> Source { get; }

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
            _origin.Add(localPath);
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
            _origin.Add(localPath);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    public void Remove()
    {
        if (Source.RowSelection is null)
            return;

        foreach (string? item in Source.RowSelection.SelectedItems.ToArray())
        {
            if (item is null) continue;
            _origin.Remove(item);
        }
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    public void Clear() => _origin.Clear();

    [RelayCommand]
    public void DropFiles(IEnumerable<IStorageItem> files)
    {
        foreach (IStorageItem item in files)
        {
            string? path = item.TryGetLocalPath();
            if (path is null) continue;
            _origin.Add(path);
        }
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        _navigationService.NavigateTo<FileMetadataProgressViewModel, FileProcessOptions?>(
            HostScreens.Home, new FileProcessOptions(SelectedMetadataType, _origin.ToArray()));
    }

    public bool CanRemove() => Source.RowSelection is not null && Source.RowSelection.SelectedItems.Count > 0;

    public bool CanClear() => _origin.Count > 0;

    public bool CanNext() => _origin.Count > 0;

    public void OnNavigatingTo()
    {
        if (NavigationParameter is null) return;
        _origin.Clear();
        _origin.AddRange(NavigationParameter);
    }

    public SelectFilesViewModel(INavigationService navigationService, IStorageServices storageProvider)
    {
        _navigationService = navigationService;
        _storageProvider = storageProvider;
        Source = new FlatTreeDataGridSource<string>(_origin)
        {
            Columns =
            {
                new TextColumn<string, string>(null, x => x, GridLength.Star),
            }
        };
        Source.RowSelection!.SingleSelect = false;

        _origin.CollectionChanged += (_, _) =>
        {
            ClearCommand.NotifyCanExecuteChanged();
            RemoveCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        };

        Source.RowSelection!.SelectionChanged += (_, _) => { RemoveCommand.NotifyCanExecuteChanged(); };
    }
}