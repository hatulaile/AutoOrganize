namespace AutoOrganize.Library.Services.RequestCoalescers;

public interface IFlightCoordinator
{
    ValueTask<AcquireResult> AcquireAsync(string key, CancellationToken token = default);

    ValueTask<AcquireResult> AcquireAsync(IEnumerable<string> key, CancellationToken token = default);

    void Release(string key);

    void Release(IEnumerable<string> keys);
}