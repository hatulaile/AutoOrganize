namespace AutoOrganize.ViewModels;

public class HostWindowViewModel : ViewModelBase
{
    public ViewModelBase CurrentViewModel { get; set; }

    public HostWindowViewModel(ViewModelBase viewmodel)
    {
        CurrentViewModel = viewmodel;
    }
}