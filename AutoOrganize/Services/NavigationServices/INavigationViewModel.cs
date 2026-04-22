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

public interface INavigationViewModel<in TArgs> : INavigationViewModel
{
    void OnNavigatingTo(TArgs args)
    {

    }

    void OnNavigatedTo(TArgs args)
    {

    }

    void OnParametersChanged(TArgs args)
    {
    }
}