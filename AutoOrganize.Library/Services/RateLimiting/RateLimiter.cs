using System.Diagnostics;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.RateLimiting;

public class RateLimiter : IRateLimiter
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    private readonly LinkedList<long> _requestTimes = [];

    public RateLimiterOption Option { get; set; }

    public RateLimiter(RateLimiterOption option)
    {
        Option = option;
    }

    public async Task WaitAsync(CancellationToken token = default)
    {
        await _semaphoreSlim.WaitAsync(token).ConfigureAwait(false);

        try
        {
            while (true)
            {
                while (_requestTimes.First?.Value + Option.Delay < Stopwatch.GetTimestamp())
                    _requestTimes.RemoveFirst();

                if (_requestTimes.Count < Option.MaxCount)
                {
                    _requestTimes.AddLast(Stopwatch.GetTimestamp());
                    break;
                }

                //因为 Task.Delay 不一定准确, 加一个10ms的缓冲
                var waitTime =
                    _requestTimes.First!.Value + Option.Delay - Stopwatch.GetTimestamp() + 10L;
                await Task.Delay(TimeSpan.FromMilliseconds(waitTime), token).ConfigureAwait(false);
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}