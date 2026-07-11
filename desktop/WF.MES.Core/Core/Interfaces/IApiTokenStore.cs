namespace WF.MES.Core.Interfaces;

/// <summary>进程内 JWT 令牌与当前工厂存储。</summary>
public interface IApiTokenStore
{
    string? AccessToken { get; }

    string? RefreshToken { get; }

    long? FactoryId { get; }

    DateTimeOffset? AccessTokenExpiresAt { get; }

    void SetTokens(string accessToken, string refreshToken, int expiresInSeconds, long? factoryId = null);

    void Clear();
}
