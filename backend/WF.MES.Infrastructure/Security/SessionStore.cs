using WF.MES.Infrastructure.Cache;
using WF.MES.Shared.Enums;

namespace WF.MES.Infrastructure.Security;

public interface ISessionStore
{
    string BuildKey(long userId, ClientType clientType, long? factoryId = null);
    Task SetActiveSessionAsync(long userId, ClientType clientType, string sessionId, TimeSpan expiry, long? factoryId = null, CancellationToken cancellationToken = default);
    Task<bool> IsActiveSessionAsync(long userId, ClientType clientType, string sessionId, long? factoryId = null, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(long userId, ClientType clientType, long? factoryId = null, CancellationToken cancellationToken = default);
}

public class RedisSessionStore(IRedisCacheService redis) : ISessionStore
{
    private const string KeyPrefix = "wf:session:";

    public string BuildKey(long userId, ClientType clientType, long? factoryId = null)
        => factoryId.HasValue
            ? $"{KeyPrefix}{factoryId.Value}:{userId}:{(int)clientType}"
            : $"{KeyPrefix}{userId}:{(int)clientType}";

    public async Task SetActiveSessionAsync(long userId, ClientType clientType, string sessionId, TimeSpan expiry, long? factoryId = null, CancellationToken cancellationToken = default)
    {
        await redis.SetStringAsync(BuildKey(userId, clientType, factoryId), sessionId, expiry);
    }

    public async Task<bool> IsActiveSessionAsync(long userId, ClientType clientType, string sessionId, long? factoryId = null, CancellationToken cancellationToken = default)
    {
        var key = BuildKey(userId, clientType, factoryId);
        var exists = await redis.KeyExistsAsync(key);
        if (exists is null)
        {
            return true;
        }

        if (exists == false)
        {
            return false;
        }

        var active = await redis.GetStringAsync(key);
        return !string.IsNullOrEmpty(active)
            && string.Equals(active, sessionId, StringComparison.Ordinal);
    }

    public async Task RemoveSessionAsync(long userId, ClientType clientType, long? factoryId = null, CancellationToken cancellationToken = default)
    {
        await redis.RemoveAsync(BuildKey(userId, clientType, factoryId));
    }
}
