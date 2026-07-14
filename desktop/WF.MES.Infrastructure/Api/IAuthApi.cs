using Refit;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Api;

/// <summary>后端认证 / 会话 / 桌面菜单 Refit 契约。</summary>
public interface IAuthApi
{
    [Post("/api/v1/auth/login")]
    Task<ApiResultDto<LoginResponseDto>> LoginAsync([Body] LoginRequestDto request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/select-factory")]
    Task<ApiResultDto<LoginResponseDto>> SelectFactoryAsync([Body] SelectFactoryRequestDto request, CancellationToken cancellationToken = default);

    [Get("/api/v1/auth/info")]
    Task<ApiResultDto<UserInfoDto>> GetInfoAsync(CancellationToken cancellationToken = default);

    [Get("/api/v1/auth/desktop-menus")]
    Task<ApiResultDto<List<ClientMenuDto>>> GetDesktopMenusAsync(CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/change-password")]
    Task<ApiResultDto<UserInfoDto>> ChangePasswordAsync([Body] ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/logout")]
    Task<ApiResultDto<object>> LogoutAsync(CancellationToken cancellationToken = default);
}
