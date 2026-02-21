using System.IO;
using System.Threading.Tasks;
using AutoOrganize.Models.FileMetadataModels.FailedMetadata;
using AutoOrganize.Services.TopLevelServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoOrganize.ViewModels.FileMetadataViewModels;

public sealed partial class FailedDirectoryMetadataViewModel : MetadataViewModelBase<FailedDirectoryMetadata>
{
    private readonly ILauncherServices _launcherServices;
    private readonly IClipboardServices _clipboardServices;

    [ObservableProperty] private DirectoryInfo? _directoryInfo;

    [RelayCommand]
    private async Task OpenContainingFolder()
    {
        if (DirectoryInfo is null) return;
        await _launcherServices.LaunchDirectoryInfoAsync(DirectoryInfo, this);
    }

    [RelayCommand]
    private async Task CopyString(string? str)
    {
        if (string.IsNullOrEmpty(str)) return;
        await _clipboardServices.SetTextAsync(str);
    }

    protected override void MetadataChanging(FailedDirectoryMetadata? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            DirectoryInfo = new DirectoryInfo(value.FullPath);
        }
    }

    protected override void MetadataChanged(FailedDirectoryMetadata? value)
    {
        base.MetadataChanged(value);
    }

    public FailedDirectoryMetadataViewModel(ILauncherServices launcherServices, IClipboardServices clipboardServices)
    {
        _launcherServices = launcherServices;
        _clipboardServices = clipboardServices;
    }
}