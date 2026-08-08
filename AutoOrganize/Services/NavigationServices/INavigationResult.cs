using System.Threading;
using System.Threading.Tasks;

namespace AutoOrganize.Services.NavigationServices;

public interface INavigationCompletion
{
    Task Task { get; }

    void Complete();

    void Cancel();
}

public interface INavigationResult : INavigationCompletion
{
    new Task<object?> Task { get; }

    Task INavigationCompletion.Task => Task;

    void Complete(object? result);

    void INavigationCompletion.Complete()
    {
        Cancel();
    }
}

public interface INavigationResult<TResult> : INavigationResult
{
    new Task<TResult> Task { get; }

    Task<object?> INavigationResult.Task => Task.ContinueWith(static task => (object?)task.Result);

    void Complete(TResult result);

    void INavigationResult.Complete(object? result)
    {
        if (result is not TResult r)
        {
            Cancel();
            return;
        }

        Complete(r);
    }
}

public sealed class TaskCompletion<TResult> : INavigationResult<TResult>
{
    private readonly TaskCompletionSource<TResult> _completionSource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly CancellationTokenRegistration _registration;

    public Task<TResult> Task => _completionSource.Task;

    public TaskCompletion(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.CanBeCanceled)
            _registration = cancellationToken.Register(() => _completionSource.TrySetCanceled());
    }

    public void Complete(TResult result)
    {
        _completionSource.TrySetResult(result);
        _registration.Dispose();
    }

    public void Cancel()
    {
        _completionSource.TrySetCanceled();
        _registration.Dispose();
    }
}