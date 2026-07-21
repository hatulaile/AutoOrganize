namespace AutoOrganize.Library.Services.RequestCoalescers;

public sealed class MultiKeyLease : ILease
{
    private bool _isDisposed;

    private readonly IEnumerable<string> _keys;
    private readonly IFlightCoordinator _flightCoordinator;

    public void Release()
    {
        Dispose();
    }

    public MultiKeyLease(IEnumerable<string> keys, IFlightCoordinator flightCoordinator)
    {
        _keys = keys;
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
        }

        _flightCoordinator.Release(_keys);
        _isDisposed = true;
    }

    ~MultiKeyLease()
    {
        Dispose(false);
    }
}