namespace AutoOrganize.Library.Models;

public sealed class RateLimiterOption
{
    public int MaxCount { get; set; }

    public long Delay { get; set; }

    public RateLimiterOption(int maxCount, long delay)
    {
        MaxCount = maxCount;
        Delay = delay;
    }
}