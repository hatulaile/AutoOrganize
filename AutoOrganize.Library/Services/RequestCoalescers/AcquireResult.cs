using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Services.RequestCoalescers;

public struct AcquireResult(bool acquired, ILease? lease)
{
    [MemberNotNullWhen(true, nameof(Lease))]
    public bool Acquired { get; set; } = acquired;

    public ILease? Lease { get; set; } = lease;

    public void Deconstruct(out bool acquired, out ILease? lease)
    {
        acquired = this.Acquired;
        lease = this.Lease;
    }
}