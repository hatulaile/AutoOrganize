using System.Buffers;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.RequestCoalescers;

public sealed partial class FlightCoordinator : IFlightCoordinator
{
    private readonly ConcurrentDictionary<string, KeyState> _keyStates = new();
    private readonly ILogger<FlightCoordinator> _logger;

    public async ValueTask<AcquireResult> AcquireAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var keyState = _keyStates.GetOrAdd(key, _ => new KeyState());

        if (Interlocked.CompareExchange(ref keyState.IsLeased, 1, 0) == 0)
            return new AcquireResult(true, new Lease(key, this));

        token.ThrowIfCancellationRequested();
        var tcs = new TaskCompletionSource();
        var previous = Interlocked.CompareExchange(ref keyState.WaitingTask, tcs, null);
        if (previous != null)
            tcs = previous;

        await tcs.Task.WaitAsync(token).ConfigureAwait(false);
        return new AcquireResult(false, null);
    }

    public async ValueTask<AcquireResult> AcquireAsync(IEnumerable<string> keys, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        string[] uniqueKeys = [.. keys.Distinct().Order()];
        var keyStates = ArrayPool<KeyAndKeyState>.Shared.Rent(uniqueKeys.Length);
        int failKeyCount = 0;
        var failKeyStates = ArrayPool<KeyAndKeyState>.Shared.Rent(uniqueKeys.Length);
        try
        {
            for (var i = 0; i < uniqueKeys.Length; i++)
            {
                KeyState keyState = _keyStates.GetOrAdd(uniqueKeys[i], static _ => new KeyState());
                if (Interlocked.CompareExchange(ref keyState.IsLeased, 1, 0) == 1)
                {
                    failKeyStates[failKeyCount++] = new KeyAndKeyState(uniqueKeys[i], keyState);
                    continue;
                }

                keyStates[i - failKeyCount] = new KeyAndKeyState(uniqueKeys[i], keyState);
            }

            if (failKeyCount == 0)
                return new AcquireResult(true,
                    new MultiKeyLease([.. keyStates.Take(uniqueKeys.Length).Select(x => x.Key)], this));

            Task[] tasks = ArrayPool<Task>.Shared.Rent(failKeyCount);
            for (var i = 0; i < failKeyCount; i++)
            {
                var tcs = new TaskCompletionSource();
                TaskCompletionSource? source = Interlocked.CompareExchange(ref failKeyStates[i].State.WaitingTask, tcs, null);
                tasks[i] = source is null ? tcs.Task : source.Task;
            }

            await Task.WhenAll(tasks.AsSpan()[..failKeyCount]).WaitAsync(token).ConfigureAwait(false);
            for (var i = 0; i < uniqueKeys.Length - failKeyCount; i++)
                Release(keyStates[i].Key);
            return new AcquireResult(false, null);
        }
        finally
        {
            ArrayPool<KeyAndKeyState>.Shared.Return(keyStates);
            ArrayPool<KeyAndKeyState>.Shared.Return(failKeyStates);
        }
    }

    public void Release(string key)
    {
        if (_keyStates.TryRemove(key, out var keyState))
        {
            ReleaseInternal(keyState);
        }
    }

    public void Release(IEnumerable<string> keys)
    {
        foreach (var key in keys.Order())
        {
            Release(key);
        }
    }

    private static void ReleaseInternal(KeyState keyState)
    {
        Interlocked.Exchange(ref keyState.IsLeased, 0);
        keyState.WaitingTask?.TrySetResult();
    }

    public FlightCoordinator(ILogger<FlightCoordinator> logger)
    {
        _logger = logger;
    }


    private sealed record KeyAndKeyState(string Key, KeyState State);

    private sealed class KeyState
    {
        public volatile byte IsLeased;
        public TaskCompletionSource? WaitingTask;
    }
}