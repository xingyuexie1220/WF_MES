using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WF.MES.Api.Authorization;
using WF.MES.Api.Hubs;
using WF.MES.Application.Notices;
using WF.MES.Application.Notices.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;

namespace WF.MES.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/system/notices")]
public class SystemNoticeController(
    INoticeService noticeService,
    IHubContext<NoticeHub> hubContext) : WfApiControllerBase
{
    [WfPermission("system:notice:list")]
    [HttpGet]
    public async Task<ApiResult<PagedResult<NoticeDto>>> GetPagedList([FromQuery] NoticeQueryRequest request)
        => ApiResult<PagedResult<NoticeDto>>.Ok(await noticeService.GetPagedListAsync(request));

    [WfPermission("system:notice:list")]
    [HttpGet("published")]
    public async Task<ApiResult<List<NoticePushDto>>> GetPublishedRecent([FromQuery] int count = 20)
        => ApiResult<List<NoticePushDto>>.Ok(await noticeService.GetPublishedRecentAsync(count));

    [WfPermission("system:notice:list")]
    [HttpGet("{id:long}")]
    public async Task<ApiResult<NoticeDto>> GetById(long id)
    {
        var notice = await noticeService.GetByIdAsync(id);
        return notice is null ? ApiResult<NoticeDto>.FailByCode(WfMessageCodes.CommonNotFound, 404) : ApiResult<NoticeDto>.Ok(notice);
    }

    [WfPermission("system:notice:add")]
    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] CreateNoticeRequest request)
    {
        var id = await noticeService.CreateAsync(request, GetOperatorId());
        if (request.Status == 1)
        {
            var push = await noticeService.PublishAsync(id, GetOperatorId());
            await hubContext.Clients.All.SendAsync("ReceiveNotice", push);
        }

        return ApiResult<long>.Ok(id);
    }

    [WfPermission("system:notice:edit")]
    [HttpPost("update/{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] UpdateNoticeRequest request)
    {
        await noticeService.UpdateAsync(id, request, GetOperatorId());
        if (request.Status == 1)
        {
            var push = await noticeService.PublishAsync(id, GetOperatorId());
            await hubContext.Clients.All.SendAsync("ReceiveNotice", push);
        }

        return ApiResult.Ok();
    }

    [WfPermission("system:notice:delete")]
    [HttpPost("delete/{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await noticeService.DeleteAsync(id, GetOperatorId());
        return ApiResult.Ok();
    }

    [WfPermission("system:notice:edit")]
    [HttpPost("publish/{id:long}")]
    public async Task<ApiResult> Publish(long id)
    {
        var push = await noticeService.PublishAsync(id, GetOperatorId());
        await hubContext.Clients.All.SendAsync("ReceiveNotice", push);
        return ApiResult.Ok();
    }
}
