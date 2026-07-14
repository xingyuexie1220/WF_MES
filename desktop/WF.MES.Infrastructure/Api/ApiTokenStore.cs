using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Api;

public sealed class ApiTokenStore : IApiTokenStore
{
    public string? AccessToken { get; private set; }

    public long? FactoryId { get; private set; }

    public DateTimeOffset? AccessTokenExpiresAt { get; private set; }

    public void SetAccessToken(string accessToken, int expiresInSeconds, long? factoryId = null)
    {
        AccessToken = accessToken;
        FactoryId = factoryId;
        AccessTokenExpiresAt = expiresInSeconds > 0
            ? DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds)
            : null;
    }

    public void Clear()
    {
        AccessToken = null;
        FactoryId = null;
        AccessTokenExpiresAt = null;
    }
}
