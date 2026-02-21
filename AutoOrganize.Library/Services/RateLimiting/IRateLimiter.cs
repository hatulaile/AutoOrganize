using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.RateLimiting;

public interface IRateLimiter
{
    public RateLimiterOption Option { get; set; }

    Task WaitAsync(CancellationToken token = default);
}