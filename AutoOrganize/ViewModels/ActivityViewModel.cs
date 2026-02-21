using AutoOrganize.Services.NavigationServices;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class ActivityViewModel : ViewModelBase, INavigationViewModel
{
    public void OnNavigatedTo()
    {
    }
}