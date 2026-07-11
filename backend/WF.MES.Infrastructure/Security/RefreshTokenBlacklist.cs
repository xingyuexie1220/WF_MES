using System.Security.Cryptography;
using System.Text;
using WF.MES.Infrastructure.Cache;

namespace WF.MES.Infrastructure.Security;

public interface IRefreshTokenBlacklist
{
    Task AddAsync(string refreshToken, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task<bool> IsBlacklistedAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public class RedisRefreshTokenBlacklist(IRedisCacheService redis) : IRefreshTokenBlacklist
{
    private const string Prefix = "wf:rt:blacklist:";

    public Task AddAsync(string refreshToken, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Task.CompletedTask;
        }

        return redis.SetStringAsync(Prefix + HashToken(refreshToken), "1", expiry);
    }

    public async Task<bool> IsBlacklistedAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var exists = await redis.KeyExistsAsync(Prefix + HashToken(refreshToken));
        return exists == true;
    }

    private static string HashToken(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
