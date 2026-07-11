using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Factories;
using WF.MES.Application.Factories.Dtos;
using WF.MES.Shared.Common;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/factories")]
public class SysFactoryController(IFactoryService factoryService) : WfApiControllerBase
{
    [HttpGet("regions")]
    [WfPermission("system:factory:list")]
    public async Task<ApiResult<List<RegionDto>>> GetRegions()
        => ApiResult<List<RegionDto>>.Ok(await factoryService.GetRegionsAsync());

    [HttpGet]
    [WfPermission("system:factory:list")]
    public async Task<ApiResult<List<FactoryDto>>> GetList()
        => ApiResult<List<FactoryDto>>.Ok(await factoryService.GetListAsync());

    [HttpGet("{id:long}")]
    [WfPermission("system:factory:list")]
    public async Task<ApiResult<FactoryDto?>> GetById(long id)
        => ApiResult<FactoryDto?>.Ok(await factoryService.GetByIdAsync(id));

    [HttpPost]
    [WfPermission("system:factory:add")]
    public async Task<ApiResult<long>> Create([FromBody] CreateFactoryRequest request)
        => ApiResult<long>.Ok(await factoryService.CreateAsync(request, GetOperatorId()));

    [HttpPut("{id:long}")]
    [WfPermission("system:factory:edit")]
    public async Task<ApiResult> Update(long id, [FromBody] UpdateFactoryRequest request)
    {
        await factoryService.UpdateAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [HttpDelete("{id:long}")]
    [WfPermission("system:factory:delete")]
    public async Task<ApiResult> Delete(long id)
    {
        await factoryService.DeleteAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }
}
