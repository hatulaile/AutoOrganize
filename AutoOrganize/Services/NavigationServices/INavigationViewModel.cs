namespace AutoOrganize.Services.NavigationServices;

public interface INavigationViewModel
{
    void OnNavigatingFrom()
    {
    }

    void OnNavigatedFrom()
    {
    }

    void OnNavigatingTo()
    {
    }

    void OnNavigatedTo()
    {
    }
}

public interface INavigationViewModel<TArgs> : INavigationViewModel
{
    TArgs? NavigationParameter { get; set; }

    void OnParameterChanged()
    {
    }
}