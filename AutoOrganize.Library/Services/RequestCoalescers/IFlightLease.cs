namespace AutoOrganize.Library.Services.RequestCoalescers;

public interface IFlightLease : IDisposable
{
    void Release();
}