using System.Linq;
using System.Threading;
using AutoOrganize.Models.Args;
using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Services.WindowManagers;
using AutoOrganize.ViewModels.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels;

[ViewModelRegistration]
public partial class RemoveTransferConfirmWindowViewModel : ViewModelBase,
    IResultWindowViewModel<RemoveTransferConfirmArgs, RemoveTransferConfirmResult>
{
    private readonly IWindowService _windowService;

    public CancellationToken CancellationToken { get; set; }

    [ObservableProperty]
    public partial bool IsDeleteFilesOnDisk { get; set; }

    [ObservableProperty]
    public partial int SuccessCount { get; set; }

    [ObservableProperty]
    public partial int FailedCount { get; set; }

    public RemoveTransferConfirmWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
    }

    public void OnOpenWindow(RemoveTransferConfirmArgs args)
    {
        SuccessCount += args.SuccessCount;
        FailedCount += args.FailedCount;
    }

    [RelayCommand]
    private void Confirm() =>
        _windowService.Close(this, new RemoveTransferConfirmResult(true, IsDeleteFilesOnDisk));

    [RelayCommand]
    private void Cancel() =>
        _windowService.Close(this, new RemoveTransferConfirmResult(false, false));
}