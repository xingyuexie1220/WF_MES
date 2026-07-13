namespace WF.MES.Core.Interfaces;

/// <summary>进程内 Access Token 与当前工厂（仅内存，桌面不使用 Refresh Token）。</summary>
public interface IApiTokenStore
{
    string? AccessToken { get; }

    long? FactoryId { get; }

    DateTimeOffset? AccessTokenExpiresAt { get; }

    void SetAccessToken(string accessToken, int expiresInSeconds, long? factoryId = null);

    void Clear();
}
