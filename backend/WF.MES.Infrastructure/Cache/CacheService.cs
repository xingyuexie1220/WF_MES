using Microsoft.Extensions.Options;
using WF.MES.Infrastructure.Options;

namespace WF.MES.Infrastructure.Cache;

public class CacheService(IRedisCacheService redis, IOptions<CacheOptions> options) : Application.Common.ICacheService
{
    private readonly CacheOptions _options = options.Value;

    public Task<T?> GetAsync<T>(string category, string key, long? factoryId = null, CancellationToken cancellationToken = default)
        => redis.GetJsonAsync<T>(BuildKey(category, key, factoryId));

    public Task SetAsync<T>(
        string category,
        string key,
        T value,
        TimeSpan? expiry = null,
        long? factoryId = null,
        CancellationToken cancellationToken = default)
        => redis.SetJsonAsync(BuildKey(category, key, factoryId), value, expiry ?? TimeSpan.FromMinutes(_options.DefaultTtlMinutes));

    public Task RemoveAsync(string category, string key, long? factoryId = null, CancellationToken cancellationToken = default)
        => redis.RemoveAsync(BuildKey(category, key, factoryId));

    public Task RemoveCategoryAsync(string category, long? factoryId = null, CancellationToken cancellationToken = default)
        => redis.RemoveByPrefixAsync(BuildPrefix(category, factoryId));

    private static string BuildPrefix(string category, long? factoryId)
        => factoryId.HasValue
            ? $"wf:cache:{factoryId.Value}:{category}:"
            : $"wf:cache:global:{category}:";

    private static string BuildKey(string category, string key, long? factoryId)
        => $"{BuildPrefix(category, factoryId)}{key}";
}
