using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Users;
using WF.MES.Application.Users.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/users")]
public class SysUserController(IUserService userService) : WfApiControllerBase
{
    [WfPermission("system:user:list")]
    [HttpGet]
    public async Task<ApiResult<PagedResult<UserDto>>> GetPagedList([FromQuery] UserQueryRequest request)
        => ApiResult<PagedResult<UserDto>>.Ok(await userService.GetPagedListAsync(request));

    [WfPermission("system:user:list")]
    [HttpGet("{id:long}")]
    public async Task<ApiResult<UserDto>> GetById(long id)
    {
        var user = await userService.GetByIdAsync(id);
        return user is null ? ApiResult<UserDto>.Fail("用户不存在", 404) : ApiResult<UserDto>.Ok(user);
    }

    [WfPermission("system:user:add")]
    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] CreateUserRequest request)
        => ApiResult<long>.Ok(await userService.CreateAsync(request, GetOperatorId()));

    [WfPermission("system:user:edit")]
    [HttpPost("update/{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] UpdateUserRequest request)
    {
        await userService.UpdateAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:user:delete")]
    [HttpPost("delete/{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await userService.DeleteAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:user:edit")]
    [HttpPost("reset-password/{id:long}")]
    public async Task<ApiResult> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        await userService.ResetPasswordAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }
}
