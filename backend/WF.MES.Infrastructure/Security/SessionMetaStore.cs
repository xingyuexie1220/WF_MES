using WF.MES.Infrastructure.Cache;

namespace WF.MES.Infrastructure.Security;

public class SessionMeta
{
    public string SessionId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; }
}

public interface ISessionMetaStore
{
    Task SetAsync(SessionMeta meta, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task<SessionMeta?> GetAsync(string sessionId, CancellationToken cancellationToken = default);
}

public class RedisSessionMetaStore(IRedisCacheService redis) : ISessionMetaStore
{
    private const string Prefix = "wf:session:meta:";

    public Task SetAsync(SessionMeta meta, TimeSpan expiry, CancellationToken cancellationToken = default)
        => redis.SetJsonAsync(Prefix + meta.SessionId, meta, expiry);

    public Task<SessionMeta?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
        => redis.GetJsonAsync<SessionMeta>(Prefix + sessionId);
}
