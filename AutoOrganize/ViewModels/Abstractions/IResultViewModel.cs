using System.Threading;

namespace AutoOrganize.ViewModels.Abstractions;

public interface IResultViewModel<out TResult> : IViewModel
{
    CancellationToken CancellationToken { get; set; }
}
