using System.Collections.Concurrent;

namespace PixApiRest.Services;

public class RateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();
    private readonly int _limitPerDay;
    private readonly ILogger<RateLimitService> _logger;

    public RateLimitService(ILogger<RateLimitService> logger)
    {
        _logger = logger;
        _limitPerDay = int.TryParse(Environment.GetEnvironmentVariable("LIMIT_REQUESTS_DAY_BY_IP"), out var limit)
            ? limit
            : 20; // Default: 20 requisições por dia
    }

    public bool IsAllowed(string ipAddress, out int remainingRequests)
    {
        var today = DateTime.UtcNow.Date;
        var info = _requestCounts.AddOrUpdate(
            ipAddress,
            _ => new RateLimitInfo { Date = today, Count = 1 },
            (_, existing) =>
            {
                if (existing.Date != today)
                {
                    // Reset counter for new day
                    return new RateLimitInfo { Date = today, Count = 1 };
                }
                existing.Count++;
                return existing;
            });

        remainingRequests = Math.Max(0, _limitPerDay - info.Count);

        if (info.Count > _limitPerDay)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}. Count: {Count}", ipAddress, info.Count);
            return false;
        }

        return true;
    }

    public int GetLimit() => _limitPerDay;

    public (int used, int remaining, int limit) GetUsageInfo(string ipAddress)
    {
        var today = DateTime.UtcNow.Date;
        if (_requestCounts.TryGetValue(ipAddress, out var info) && info.Date == today)
        {
            return (info.Count, Math.Max(0, _limitPerDay - info.Count), _limitPerDay);
        }
        return (0, _limitPerDay, _limitPerDay);
    }

    private class RateLimitInfo
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
