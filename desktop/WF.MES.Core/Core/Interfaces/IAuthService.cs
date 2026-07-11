using WF.MES.Models.Dtos;

namespace WF.MES.Core.Interfaces;

/// <summary>通过 Backend API 完成登录、会话校验与改密。</summary>
public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);

    Task<LoginResultDto> SelectFactoryAsync(string userName, string password, long factoryId, CancellationToken cancellationToken = default);

    Task<LoginResultDto> SwitchFactoryAsync(long factoryId, CancellationToken cancellationToken = default);

    Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default);

    Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<UserInfoDto?> ChangePasswordAsync(string oldPassword, string newPassword, CancellationToken cancellationToken = default);
}
