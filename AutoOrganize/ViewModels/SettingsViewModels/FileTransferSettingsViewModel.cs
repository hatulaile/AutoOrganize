using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.FileTransferServices;
using AutoOrganize.Library.Utils;
using AutoOrganize.Services.TopLevelServices;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class FileTransferSettingsViewModel : SettingsViewModelBase<FileTransferConfig>
{
    private readonly IStorageServices _storageServices;

    public bool IsOutputPathValid => PathUtils.IsValidPath(NewConfig.OutputDirectory);

    public FileTransferSettingsViewModel(IStorageServices storageServices, IFileConfigManager configManager) :
        base(configManager)
    {
        _storageServices = storageServices;
    }

    public override void ApplyConfig()
    {
        if (!IsOutputPathValid)
            NewConfig.OutputDirectory = Config.OutputDirectory;
        base.ApplyConfig();
    }

    [RelayCommand]
    private async Task BrowseFolder(CancellationToken token)
    {
        foreach (IStorageFolder storageFolder in await _storageServices.GetSelectFoldersAsync(
                     new FolderPickerOpenOptions { AllowMultiple = false }, this))
        {
            NewConfig.OutputDirectory = storageFolder.Path.LocalPath;
        }
    }
}