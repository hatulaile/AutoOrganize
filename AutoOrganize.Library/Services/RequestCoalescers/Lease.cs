namespace AutoOrganize.Library.Services.RequestCoalescers;

public sealed partial class FlightCoordinator
{
    public sealed class Lease : ILease
    {
        private bool _isDisposed;

        private readonly string _key;
        private readonly IFlightCoordinator _flightCoordinator;

        public void Release()
        {
            Dispose();
        }

        public Lease(string key, IFlightCoordinator flightCoordinator)
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
            }

            _flightCoordinator.Release(_key);
            _isDisposed = true;
        }

        ~Lease()
        {
            Dispose(false);
        }
    }
}