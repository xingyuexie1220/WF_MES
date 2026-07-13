using Refit;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Api;

public interface IAuthApi
{
    [Post("/api/v1/auth/login")]
    Task<ApiResultDto<LoginResponseDto>> LoginAsync([Body] LoginRequestDto request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/select-factory")]
    Task<ApiResultDto<LoginResponseDto>> SelectFactoryAsync([Body] SelectFactoryRequestDto request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/switch-factory")]
    Task<ApiResultDto<LoginResponseDto>> SwitchFactoryAsync([Body] SwitchFactoryRequestDto request, CancellationToken cancellationToken = default);

    [Get("/api/v1/auth/info")]
    Task<ApiResultDto<UserInfoDto>> GetInfoAsync(CancellationToken cancellationToken = default);

    [Get("/api/v1/auth/desktop-menus")]
    Task<ApiResultDto<List<ClientMenuDto>>> GetDesktopMenusAsync(CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/change-password")]
    Task<ApiResultDto<UserInfoDto>> ChangePasswordAsync([Body] ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/logout")]
    Task<ApiResultDto<object>> LogoutAsync(CancellationToken cancellationToken = default);
}
