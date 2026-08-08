namespace AutoOrganize.ViewModels.Abstractions;

public interface IResultNavigationViewModel<out TResult> : INavigationViewModel, IResultViewModel<TResult>
{
}

public interface IResultNavigationViewModel<in TArgs, out TResult>
    : INavigationViewModel<TArgs>, IResultNavigationViewModel<TResult>
{
}
