using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Roles;
using WF.MES.Application.Roles.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/roles")]
public class SystemRoleController(IRoleService roleService) : WfApiControllerBase
{
    [WfPermission("system:role:list")]
    [HttpGet]
    public async Task<ApiResult<PagedResult<RoleDto>>> GetPagedList([FromQuery] RoleQueryRequest request)
        => ApiResult<PagedResult<RoleDto>>.Ok(await roleService.GetPagedListAsync(request));

    [WfPermission("system:role:list")]
    [HttpGet("all")]
    public async Task<ApiResult<List<RoleDto>>> GetAll()
        => ApiResult<List<RoleDto>>.Ok(await roleService.GetAllAsync());

    [WfPermission("system:role:list")]
    [HttpGet("{id:long}")]
    public async Task<ApiResult<RoleDto>> GetById(long id)
    {
        var role = await roleService.GetByIdAsync(id);
        return role is null ? ApiResult<RoleDto>.FailByCode(WfMessageCodes.CommonNotFound, 404) : ApiResult<RoleDto>.Ok(role);
    }

    [WfPermission("system:role:add")]
    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] CreateRoleRequest request)
        => ApiResult<long>.Ok(await roleService.CreateAsync(request, GetOperatorId()));

    [WfPermission("system:role:edit")]
    [HttpPost("update/{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] UpdateRoleRequest request)
    {
        await roleService.UpdateAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:role:delete")]
    [HttpPost("delete/{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await roleService.DeleteAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }
}
