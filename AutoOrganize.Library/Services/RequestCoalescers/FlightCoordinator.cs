using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace AutoOrganize.Library.Services.RequestCoalescers;

public sealed class FlightCoordinator : IFlightCoordinator
{
    private readonly ConcurrentDictionary<string, AsyncManualResetEvent> _locks = new();
    private readonly ILogger<FlightCoordinator> _logger;

    public async Task<FlightCoordinatorAcquireResult> AcquireAsync(string key, CancellationToken token = default)
    {
        if (_locks.TryAdd(key, new AsyncManualResetEvent(false)))
        {
            _logger.LogDebug("获取协调器许可成功: {Key}", key);
            return new FlightCoordinatorAcquireResult(
                acquired: true,
                lease: new FlightLease(key, this));
        }

        _logger.LogDebug("协调器许可等待中: {Key}", key);
        if (_locks.TryGetValue(key, out var waitHandle))
        {
            await waitHandle.WaitAsync(token).ConfigureAwait(false);
            _logger.LogDebug("协调器许可通过: {Key}", key);
        }

        return new FlightCoordinatorAcquireResult(
            acquired: false,
            lease: null);
    }

    public FlightCoordinator(ILogger<FlightCoordinator> logger)
    {
        _logger = logger;
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