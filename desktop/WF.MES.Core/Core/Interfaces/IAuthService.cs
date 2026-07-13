using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>通过 Backend API 完成登录、会话心跳与改密（桌面无 Refresh Token，过期须重新登录）。</summary>
public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);

    Task<LoginResultDto> SelectFactoryAsync(string userName, string password, long factoryId, CancellationToken cancellationToken = default);

    Task<LoginResultDto> SwitchFactoryAsync(long factoryId, CancellationToken cancellationToken = default);

    /// <summary>心跳：检测 Access Token 是否有效、是否被其他设备踢下线（不自动 Refresh）。</summary>
    Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<UserInfoDto?> ChangePasswordAsync(string oldPassword, string newPassword, CancellationToken cancellationToken = default);
}
