namespace AutoOrganize.Library.Services.RequestCoalescers;

public interface ILease : IDisposable
{
    void Release();
}