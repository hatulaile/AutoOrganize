namespace AutoOrganize.Library.Services.RequestCoalescers;

public interface IFlightCoordinator
{
    Task<FlightCoordinatorAcquireResult> AcquireAsync(string key, CancellationToken token = default);
}