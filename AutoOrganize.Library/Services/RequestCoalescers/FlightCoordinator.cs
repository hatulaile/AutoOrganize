using System.Collections.Concurrent;
using Nito.AsyncEx;

namespace AutoOrganize.Library.Services.RequestCoalescers;

public sealed class FlightCoordinator : IFlightCoordinator
{
    private readonly ConcurrentDictionary<string, AsyncManualResetEvent> _locks = new();

    public async Task<FlightCoordinatorAcquireResult> AcquireAsync(string key, CancellationToken token = default)
    {
        if (_locks.TryAdd(key, new AsyncManualResetEvent(false)))
        {
            return new FlightCoordinatorAcquireResult(
                acquired: true,
                lease: new FlightLease(key, this));
        }

        if (_locks.TryGetValue(key, out var waitHandle))
        {
            await waitHandle.WaitAsync(token).ConfigureAwait(false);
        }

        return new FlightCoordinatorAcquireResult(
            acquired: false,
            lease: null);
    }

    public sealed class FlightLease : IFlightLease
    {
        private bool _isDisposed;

        private readonly string _key;
        private readonly FlightCoordinator _flightCoordinator;

        public void Release()
        {
            Dispose();
        }

        public FlightLease(string key, FlightCoordinator flightCoordinator)
        {
            _key = key;
            _flightCoordinator = flightCoordinator;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                if (_flightCoordinator._locks.TryRemove(_key, out var value))
                    value.Set();
            }

            _isDisposed = true;
        }

        ~FlightLease()
        {
            Dispose(false);
        }
    }
}