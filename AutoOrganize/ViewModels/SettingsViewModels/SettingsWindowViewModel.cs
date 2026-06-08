using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.ViewModels.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class SettingsWindowViewModel : SubNavigateViewModelBase, IWindowViewModel
{
    private readonly INavigationService _navigationService;

    private readonly HashSet<ISettingsViewModel> _settingsViewModels = [];

    private ISettingsViewModel? CurrentSettingsViewModel => (ISettingsViewModel?)RoutingState.CurrentPageViewModel;

    [ObservableProperty]
    public partial IPageModel? SelectedItem { get; set; }

    public IReadOnlyList<IPageModel> Pages { get; }

    [RelayCommand]
    public void Navigate(IPageModel page)
    {
        if (page.ViewModelType is null)
            return;

        if (CurrentSettingsViewModel is not null &&
            !_settingsViewModels.Contains(CurrentSettingsViewModel) && CurrentSettingsViewModel.HasConfigChanged())
            _settingsViewModels.Add(CurrentSettingsViewModel);

        _navigationService.NavigateTo(RoutingState, page.ViewModelType);

        if (CurrentSettingsViewModel is not null)
            _settingsViewModels.Remove(CurrentSettingsViewModel);

        if (!page.Equals(SelectedItem))
            SelectedItem = page;
    }

    [RelayCommand]
    public async Task ApplyConfigAsync(CancellationToken token)
    {
        foreach (ISettingsViewModel settingsViewModel in _settingsViewModels)
            await settingsViewModel.ApplyConfigAsync(token);

        if (CurrentSettingsViewModel?.HasConfigChanged() ?? false)
            await CurrentSettingsViewModel.ApplyConfigAsync(token);
    }

    [RelayCommand]
    public async Task CancelConfigAsync(CancellationToken token)
    {
        foreach (ISettingsViewModel settingsViewModel in _settingsViewModels)
            await settingsViewModel.CancelConfigChangeAsync(token);

        if (CurrentSettingsViewModel?.HasConfigChanged() ?? false)
            await CurrentSettingsViewModel.CancelConfigChangeAsync(token);
    }

    public SettingsWindowViewModel(INavigationService navigationService)
    {
        Pages =
        [
            new PageModel<LoggerSettingsViewModel>("日志设置", string.Empty),
            new PageModel<FileNameSettingsViewModel>("文件命名", string.Empty),
            new PageModel<FileTransferSettingsViewModel>("文件传输", string.Empty),
            new CategoryPageModel("元数据设置", string.Empty,
                new PageModel<ThemoviedbMetadataProviderSettingsViewModel>("TMDB设置", string.Empty))
        ];
        _navigationService = navigationService;
        RoutingState.SetOwnerViewModel(this);
        IPageModel page = Pages.First(x => x.ViewModelType is not null);
        Navigate(page);
    }
}