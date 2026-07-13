using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Menus;
using WF.MES.Application.Menus.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Enums;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/menus")]
public class SystemMenuController(IMenuService menuService) : WfApiControllerBase
{
    [WfPermission("system:menu:list")]
    [HttpGet("tree")]
    public async Task<ApiResult<List<MenuDto>>> GetTree([FromQuery] ClientType? clientType = null)
        => ApiResult<List<MenuDto>>.Ok(await menuService.GetTreeAsync(clientType));

    [WfPermission("system:menu:list")]
    [HttpGet("{id:long}")]
    public async Task<ApiResult<MenuDto>> GetById(long id)
    {
        var menu = await menuService.GetByIdAsync(id);
        return menu is null ? ApiResult<MenuDto>.FailByCode(WfMessageCodes.CommonNotFound, 404) : ApiResult<MenuDto>.Ok(menu);
    }

    [WfPermission("system:menu:add")]
    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] CreateMenuRequest request)
        => ApiResult<long>.Ok(await menuService.CreateAsync(request, GetOperatorId()));

    [WfPermission("system:menu:edit")]
    [HttpPost("update/{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] UpdateMenuRequest request)
    {
        await menuService.UpdateAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:menu:delete")]
    [HttpPost("delete/{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await menuService.DeleteAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }
}
