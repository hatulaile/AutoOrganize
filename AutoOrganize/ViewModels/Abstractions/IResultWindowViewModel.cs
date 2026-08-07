using System.Threading;

namespace AutoOrganize.ViewModels.Abstractions;

public interface IResultWindowViewModel<out TResult> : IWindowViewModel
{
    CancellationToken CancellationToken { get; set; }
}

public interface IResultWindowViewModel<in TArgs, out TResult> : IWindowViewModel<TArgs>, IResultWindowViewModel<TResult>
{
}