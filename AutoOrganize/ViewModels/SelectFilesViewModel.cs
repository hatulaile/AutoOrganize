using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Models;
using AutoOrganize.Models.Options;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.TopLevelServices;
using Avalonia.Collections;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class SelectFilesViewModel : ViewModelBase, INavigationViewModel<IEnumerable<string>?>
{
    private readonly INavigationService _navigationService;
    private readonly IStorageServices _storageProvider;
    private readonly ILogger<SelectFilesViewModel> _logger;

    [ObservableProperty]
    public partial MetadataType SelectedMetadataType { get; set; } = MetadataType.Tv;

    public AvaloniaList<string> Source { get; } = [];

    [ObservableProperty]
    public partial AvaloniaList<string> SelectionItems { get; set; }

    [RelayCommand]
    public async Task AddFiles()
    {
        var selectFiles = await _storageProvider.GetSelectFilesAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.All],
            Title = "选择添加的文件"
        });

        _logger.LogDebug("用户添加了 {Count} 个文件", selectFiles.Count);

        foreach (IStorageFile storageFile in selectFiles)
        {
            string? localPath = storageFile.TryGetLocalPath();
            if (localPath is null)
            {
                _logger.LogDebug("跳过一个无法获取本地路径的文件");
                continue;
            }

            Source.Add(localPath);
        }
    }

    [RelayCommand]
    public async Task AddDirectory()
    {
        var selectFolders = await _storageProvider.GetSelectFoldersAsync(
            new FolderPickerOpenOptions()
            {
                AllowMultiple = true,
                Title = "选择添加的文件夹"
            });

        _logger.LogDebug("用户添加了 {Count} 个文件夹", selectFolders.Count);

        foreach (IStorageFolder storageFolder in selectFolders)
        {
            string? localPath = storageFolder.TryGetLocalPath();
            if (localPath is null)
            {
                _logger.LogDebug("跳过一个无法获取本地路径的文件夹");
                continue;
            }

            Source.Add(localPath);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    public void Remove()
    {
        _logger.LogDebug("用户移除了 {Count} 个选中项", SelectionItems.Count);
        foreach (string item in SelectionItems.ToArray())
        {
            Source.Remove(item);
        }
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    public void Clear()
    {
        _logger.LogDebug("用户清空了文件列表, 之前共 {Count} 个文件", Source.Count);
        Source.Clear();
    }

    [RelayCommand]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "只有在 DEBUG 下才会调用, 所以性能损失抑制")]
    public void DropFiles(IEnumerable<IStorageItem> files)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("用户拖放了 {Count} 个文件", files.Count());
        }

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
        _logger.LogInformation("用户前往 FileMetadataProgressViewModel, 类型: {Type}, 文件数量: {Count}", SelectedMetadataType,
            Source.Count);
        _navigationService.NavigateTo<FileMetadataProgressViewModel, FileProcessOptions>(HostScreens.Home,
            new FileProcessOptions
            {
                Type = SelectedMetadataType,
                FilesPaths = Source
            });
    }

    public bool CanRemove() => SelectionItems.Count > 0;

    public bool CanClear() => Source.Count > 0;

    public bool CanNext() => Source.Count > 0;

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "只有在 DEBUG 下才会调用, 所以性能损失抑制")]
    public void OnNavigatingTo(IEnumerable<string>? strings)
    {
        if (strings is null) return;
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("导航到文件选择页, 携带 {Count} 个文件路径", strings.Count());
        }

        Source.Clear();
        Source.AddRange(strings);
    }

    public SelectFilesViewModel(INavigationService navigationService, IStorageServices storageProvider,
        ILogger<SelectFilesViewModel> logger)
    {
        _navigationService = navigationService;
        _storageProvider = storageProvider;
        _logger = logger;
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