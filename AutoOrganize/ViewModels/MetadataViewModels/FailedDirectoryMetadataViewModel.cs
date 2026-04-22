using System.IO;
using System.Threading.Tasks;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Services.TopLevelServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.MetadataViewModels;

[ViewModelRegistration]
public sealed partial class FailedDirectoryMetadataViewModel : MetadataViewModelBase<FailedDirectoryModel>
{
    private readonly ILauncherServices _launcherServices;
    private readonly IClipboardServices _clipboardServices;

    [ObservableProperty]
    public partial DirectoryInfo? DirectoryInfo { get; set; }

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

    protected override void MetadataChanging(FailedDirectoryModel? value)
    {
        base.MetadataChanging(value);
        if (value is not null)
        {
            DirectoryInfo = new DirectoryInfo(value.FullPath);
        }
    }

    public FailedDirectoryMetadataViewModel(ILauncherServices launcherServices, IClipboardServices clipboardServices)
    {
        _launcherServices = launcherServices;
        _clipboardServices = clipboardServices;
    }
}