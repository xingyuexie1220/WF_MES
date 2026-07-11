using WF.MES.Application.Auth.Dtos;
using WF.MES.Application.Factories.Dtos;
using WF.MES.Application.Menus.Dtos;
using WF.MES.Shared.Enums;

namespace WF.MES.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> SelectFactoryAsync(SelectFactoryRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> SwitchFactoryAsync(long userId, SwitchFactoryRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(long userId, ClientType clientType, long? factoryId, CancellationToken cancellationToken = default);
    Task<UserInfoDto> GetCurrentUserInfoAsync(long userId, CancellationToken cancellationToken = default);
    Task<List<ClientMenuDto>> GetCurrentUserMenusAsync(long userId, ClientType clientType, CancellationToken cancellationToken = default);
    Task<List<RouterMenuDto>> GetCurrentUserRoutersAsync(long userId, CancellationToken cancellationToken = default);
    Task<List<MobileMenuDto>> GetCurrentUserMobileMenusAsync(long userId, CancellationToken cancellationToken = default);
    Task<UserInfoDto> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
