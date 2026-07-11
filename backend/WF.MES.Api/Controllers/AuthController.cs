using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Auth;
using WF.MES.Application.Auth.Dtos;
using WF.MES.Application.Factories.Dtos;
using WF.MES.Application.Common;
using WF.MES.Application.Menus.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService, ICurrentUserService currentUserService) : WfApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ApiResult<LoginResponse>> Login([FromBody] LoginRequest request)
        => ApiResult<LoginResponse>.Ok(await authService.LoginAsync(request));

    [AllowAnonymous]
    [HttpPost("select-factory")]
    public async Task<ApiResult<LoginResponse>> SelectFactory([FromBody] SelectFactoryRequest request)
        => ApiResult<LoginResponse>.Ok(await authService.SelectFactoryAsync(request));

    [Authorize]
    [HttpPost("switch-factory")]
    public async Task<ApiResult<LoginResponse>> SwitchFactory([FromBody] SwitchFactoryRequest request)
        => ApiResult<LoginResponse>.Ok(await authService.SwitchFactoryAsync(GetOperatorId(), request));

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ApiResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
        => ApiResult<LoginResponse>.Ok(await authService.RefreshTokenAsync(request));

    [Authorize]
    [HttpPost("logout")]
    public async Task<ApiResult> Logout()
    {
        if (currentUserService.UserId.HasValue && currentUserService.ClientType.HasValue)
        {
            await authService.LogoutAsync(
                currentUserService.UserId.Value,
                currentUserService.ClientType.Value,
                currentUserService.FactoryId);
        }

        return ApiResult.Ok();
    }

    [Authorize]
    [HttpGet("info")]
    public async Task<ApiResult<UserInfoDto>> GetInfo()
        => ApiResult<UserInfoDto>.Ok(await authService.GetCurrentUserInfoAsync(GetOperatorId()));

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ApiResult<UserInfoDto>> ChangePassword([FromBody] ChangePasswordRequest request)
        => ApiResult<UserInfoDto>.Ok(await authService.ChangePasswordAsync(GetOperatorId(), request));

    [Authorize]
    [HttpGet("menus")]
    public async Task<ApiResult<List<ClientMenuDto>>> GetMenus([FromQuery] ClientType clientType)
        => ApiResult<List<ClientMenuDto>>.Ok(await authService.GetCurrentUserMenusAsync(GetOperatorId(), clientType));

    [Authorize]
    [HttpGet("routers")]
    public async Task<ApiResult<List<RouterMenuDto>>> GetRouters()
        => ApiResult<List<RouterMenuDto>>.Ok(await authService.GetCurrentUserRoutersAsync(GetOperatorId()));

    [Authorize]
    [HttpGet("mobile-menus")]
    public async Task<ApiResult<List<MobileMenuDto>>> GetMobileMenus()
        => ApiResult<List<MobileMenuDto>>.Ok(await authService.GetCurrentUserMobileMenusAsync(GetOperatorId()));

    [Authorize]
    [HttpGet("desktop-menus")]
    public async Task<ApiResult<List<ClientMenuDto>>> GetDesktopMenus()
        => ApiResult<List<ClientMenuDto>>.Ok(await authService.GetCurrentUserMenusAsync(GetOperatorId(), ClientType.Desktop));
}
