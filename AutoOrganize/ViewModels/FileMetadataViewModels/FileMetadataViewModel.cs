using System.IO;
using System.Threading.Tasks;
using AutoOrganize.Models.FileMetadataModels.SuccessMetadata;
using AutoOrganize.Services.TopLevelServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoOrganize.ViewModels.FileMetadataViewModels;

public sealed partial class FileMetadataViewModel : MetadataViewModelBase<FileMetadata>
{
    private readonly ILauncherServices _launcherServices;
    private readonly IClipboardServices _clipboardServices;
    [ObservableProperty] private FileInfo? _fileInfo;

    [RelayCommand]
    private async Task OpenFile()
    {
        if (FileInfo is null) return;
        await _launcherServices.LaunchFileInfoAsync(FileInfo, this);
    }

    [RelayCommand]
    private async Task OpenContainingFolder()
    {
        if (FileInfo?.Directory is null) return;
        await _launcherServices.LaunchDirectoryInfoAsync(FileInfo.Directory, this);
    }

    [RelayCommand]
    private async Task CopyString(string? str)
    {
        if (string.IsNullOrEmpty(str)) return;
        await _clipboardServices.SetTextAsync(str);
    }

    protected override void MetadataChanging(FileMetadata? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            FileInfo = new FileInfo(value.FullPath);
        }
    }

    protected override void MetadataChanged(FileMetadata? value)
    {
        base.MetadataChanged(value);
    }

    public FileMetadataViewModel(ILauncherServices launcherServices, IClipboardServices clipboardServices)
    {
        _launcherServices = launcherServices;
        _clipboardServices = clipboardServices;
    }
}