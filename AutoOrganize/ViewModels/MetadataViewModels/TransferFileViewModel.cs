using System.IO;
using System.Threading.Tasks;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Services.TopLevelServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.MetadataViewModels;

[ViewModelRegistration]
public sealed partial class TransferFileViewModel : MetadataViewModelBase<TransferFileModel>
{
    private readonly ILauncherServices _launcherServices;
    private readonly IClipboardServices _clipboardServices;

    [ObservableProperty]
    public partial FileInfo? FileInfo { get; set; }

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

    protected override void MetadataChanging(TransferFileModel? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            FileInfo = new FileInfo(value.FullPath);
        }
    }

    public TransferFileViewModel(ILauncherServices launcherServices, IClipboardServices clipboardServices)
    {
        _launcherServices = launcherServices;
        _clipboardServices = clipboardServices;
    }
}