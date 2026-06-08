namespace AutoOrganize.ViewModels.Abstractions;

public interface INavigationViewModel : IViewModel
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