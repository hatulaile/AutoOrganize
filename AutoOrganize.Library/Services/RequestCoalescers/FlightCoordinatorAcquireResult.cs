using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Services.RequestCoalescers;

public class FlightCoordinatorAcquireResult(bool acquired, IFlightLease? lease)
{
    [MemberNotNullWhen(true, nameof(Lease))]
    public bool Acquired { get; set; } = acquired;

    public IFlightLease? Lease { get; set; } = lease;

    public void Deconstruct(out bool acquired, out IFlightLease? lease)
    {
        acquired = this.Acquired;
        lease = this.Lease;
    }
}