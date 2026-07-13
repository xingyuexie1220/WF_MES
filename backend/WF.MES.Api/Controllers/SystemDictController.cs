using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.MES.Api.Authorization;
using WF.MES.Application.Dicts;
using WF.MES.Application.Dicts.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/dicts")]
public class SystemDictController(IDictService dictService) : WfApiControllerBase
{
    [WfPermission("system:dict:list")]
    [HttpGet("types")]
    public async Task<ApiResult<PagedResult<DictTypeDto>>> GetTypePagedList([FromQuery] DictTypeQueryRequest request)
        => ApiResult<PagedResult<DictTypeDto>>.Ok(await dictService.GetTypePagedListAsync(request));

    [WfPermission("system:dict:list")]
    [HttpGet("types/all")]
    public async Task<ApiResult<List<DictTypeDto>>> GetAllTypes()
        => ApiResult<List<DictTypeDto>>.Ok(await dictService.GetAllTypesAsync());

    [WfPermission("system:dict:list")]
    [HttpGet("types/{id:long}")]
    public async Task<ApiResult<DictTypeDto>> GetTypeById(long id)
    {
        var item = await dictService.GetTypeByIdAsync(id);
        return item is null ? ApiResult<DictTypeDto>.FailByCode(WfMessageCodes.CommonNotFound, 404) : ApiResult<DictTypeDto>.Ok(item);
    }

    [WfPermission("system:dict:add")]
    [HttpPost("types")]
    public async Task<ApiResult<long>> CreateType([FromBody] CreateDictTypeRequest request)
        => ApiResult<long>.Ok(await dictService.CreateTypeAsync(request, GetOperatorId()));

    [WfPermission("system:dict:edit")]
    [HttpPost("types/update/{id:long}")]
    public async Task<ApiResult> UpdateType(long id, [FromBody] UpdateDictTypeRequest request)
    {
        await dictService.UpdateTypeAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:dict:delete")]
    [HttpPost("types/delete/{id:long}")]
    public async Task<ApiResult> DeleteType(long id)
    {
        await dictService.DeleteTypeAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:dict:list")]
    [HttpGet("data")]
    public async Task<ApiResult<PagedResult<DictDataDto>>> GetDataPagedList([FromQuery] DictDataQueryRequest request)
        => ApiResult<PagedResult<DictDataDto>>.Ok(await dictService.GetDataPagedListAsync(request));

    [WfPermission("system:dict:list")]
    [HttpGet("data/by-type/{dictType}")]
    public async Task<ApiResult<List<DictDataDto>>> GetDataByType(string dictType)
        => ApiResult<List<DictDataDto>>.Ok(await dictService.GetDataByTypeAsync(dictType));

    /// <summary>业务页读取字典选项（仅需登录，不要求字典管理权限）</summary>
    [HttpGet("data/options/{dictType}")]
    public async Task<ApiResult<List<DictDataDto>>> GetOptionsByType(string dictType)
        => ApiResult<List<DictDataDto>>.Ok(await dictService.GetDataByTypeAsync(dictType));

    [WfPermission("system:dict:list")]
    [HttpGet("data/{id:long}")]
    public async Task<ApiResult<DictDataDto>> GetDataById(long id)
    {
        var item = await dictService.GetDataByIdAsync(id);
        return item is null ? ApiResult<DictDataDto>.FailByCode(WfMessageCodes.CommonNotFound, 404) : ApiResult<DictDataDto>.Ok(item);
    }

    [WfPermission("system:dict:add")]
    [HttpPost("data")]
    public async Task<ApiResult<long>> CreateData([FromBody] CreateDictDataRequest request)
        => ApiResult<long>.Ok(await dictService.CreateDataAsync(request, GetOperatorId()));

    [WfPermission("system:dict:edit")]
    [HttpPost("data/update/{id:long}")]
    public async Task<ApiResult> UpdateData(long id, [FromBody] UpdateDictDataRequest request)
    {
        await dictService.UpdateDataAsync(id, request, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:dict:delete")]
    [HttpPost("data/delete/{id:long}")]
    public async Task<ApiResult> DeleteData(long id)
    {
        await dictService.DeleteDataAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }
}
