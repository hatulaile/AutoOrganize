using System.IO;
using System.Threading.Tasks;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Services.TopLevelServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.MetadataViewModels;

[ViewModelRegistration]
public sealed partial class FailedMetadataViewModel : MetadataViewModelBase<FailedFileModel>
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

    protected override void MetadataChanging(FailedFileModel? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            FileInfo = new FileInfo(value.FullPath);
        }
    }

    protected override void MetadataChanged(FailedFileModel? value)
    {
        base.MetadataChanged(value);
    }

    public FailedMetadataViewModel(ILauncherServices launcherServices, IClipboardServices clipboardServices)
    {
        _launcherServices = launcherServices;
        _clipboardServices = clipboardServices;
    }
}