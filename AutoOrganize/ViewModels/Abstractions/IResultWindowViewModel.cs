namespace AutoOrganize.ViewModels.Abstractions;

public interface IResultWindowViewModel<out TResult> : IWindowViewModel, IResultViewModel<TResult>
{
}

public interface IResultWindowViewModel<in TArgs, out TResult> : IWindowViewModel<TArgs>, IResultWindowViewModel<TResult>
{
}
