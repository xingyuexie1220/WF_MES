using SqlSugar;
using WF.MES.Application.Notices;
using WF.MES.Application.Notices.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Shared.Common;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class NoticeService(ISqlSugarClient db) : INoticeService
{
    public async Task<PagedResult<NoticeDto>> GetPagedListAsync(NoticeQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var items = await db.Queryable<SysNotice>()
            .Where(n => !n.IsDeleted)
            .WhereIF(!string.IsNullOrWhiteSpace(request.Title), n => n.Title.Contains(request.Title!))
            .WhereIF(request.NoticeType.HasValue, n => n.NoticeType == request.NoticeType)
            .WhereIF(request.Status.HasValue, n => n.Status == request.Status)
            .OrderByDescending(n => n.CreateTime)
            .Select(n => new NoticeDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                NoticeType = n.NoticeType,
                Status = n.Status,
                PublishTime = n.PublishTime,
                CreateBy = n.CreateBy,
                CreateTime = n.CreateTime,
                UpdateBy = n.UpdateBy,
                UpdateTime = n.UpdateTime
            })
            .ToPageListAsync(request.PageIndex, request.PageSize, total);

        await FillAuditNamesAsync(items, cancellationToken);

        return new PagedResult<NoticeDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<List<NoticePushDto>> GetPublishedRecentAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return await db.Queryable<SysNotice>()
            .Where(n => !n.IsDeleted && n.Status == 1)
            .OrderByDescending(n => n.PublishTime)
            .Take(count)
            .Select(n => new NoticePushDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                NoticeType = n.NoticeType,
                PublishTime = n.PublishTime
            })
            .ToListAsync();
    }

    public async Task<NoticeDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await db.Queryable<SysNotice>()
            .Where(n => n.Id == id && !n.IsDeleted)
            .Select(n => new NoticeDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                NoticeType = n.NoticeType,
                Status = n.Status,
                PublishTime = n.PublishTime,
                CreateBy = n.CreateBy,
                CreateTime = n.CreateTime,
                UpdateBy = n.UpdateBy,
                UpdateTime = n.UpdateTime
            })
            .FirstAsync();

        if (item is null)
        {
            return null;
        }

        await FillAuditNamesAsync([item], cancellationToken);
        return item;
    }

    public async Task<long> CreateAsync(CreateNoticeRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var notice = new SysNotice
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            NoticeType = request.NoticeType,
            Status = request.Status,
            PublishTime = request.Status == 1 ? now : null,
            CreateBy = operatorId,
            CreateTime = now
        };

        return await db.Insertable(notice).ExecuteReturnIdentityAsync();
    }

    public async Task UpdateAsync(long id, UpdateNoticeRequest request, long operatorId, CancellationToken cancellationToken = default)
    {
        var notice = await db.Queryable<SysNotice>().FirstAsync(n => n.Id == id && !n.IsDeleted)
            ?? throw new BusinessException("公告不存在");

        notice.Title = request.Title.Trim();
        notice.Content = request.Content.Trim();
        notice.NoticeType = request.NoticeType;
        notice.Status = request.Status;
        notice.PublishTime = request.Status == 1 ? notice.PublishTime ?? DateTime.Now : null;
        notice.UpdateBy = operatorId;
        notice.UpdateTime = DateTime.Now;

        await db.Updateable(notice).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        var notice = await db.Queryable<SysNotice>().FirstAsync(n => n.Id == id && !n.IsDeleted)
            ?? throw new BusinessException("公告不存在");

        notice.IsDeleted = true;
        notice.UpdateBy = operatorId;
        notice.UpdateTime = DateTime.Now;
        await db.Updateable(notice).ExecuteCommandAsync();
    }

    public async Task<NoticePushDto> PublishAsync(long id, long operatorId, CancellationToken cancellationToken = default)
    {
        var notice = await db.Queryable<SysNotice>().FirstAsync(n => n.Id == id && !n.IsDeleted)
            ?? throw new BusinessException("公告不存在");

        notice.Status = 1;
        notice.PublishTime = DateTime.Now;
        notice.UpdateBy = operatorId;
        notice.UpdateTime = DateTime.Now;
        await db.Updateable(notice).ExecuteCommandAsync();

        return new NoticePushDto
        {
            Id = notice.Id,
            Title = notice.Title,
            Content = notice.Content,
            NoticeType = notice.NoticeType,
            PublishTime = notice.PublishTime
        };
    }

    private async Task FillAuditNamesAsync(List<NoticeDto> items, CancellationToken cancellationToken)
    {
        var userIds = items
            .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var users = await db.Queryable<SysUser>()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.NickName, u.UserName })
            .ToListAsync();

        var map = users.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.NickName) ? u.UserName : u.NickName);

        foreach (var item in items)
        {
            if (item.CreateBy.HasValue && map.TryGetValue(item.CreateBy.Value, out var createName))
            {
                item.CreateByName = createName;
            }

            if (item.UpdateBy.HasValue && map.TryGetValue(item.UpdateBy.Value, out var updateName))
            {
                item.UpdateByName = updateName;
            }
        }
    }
}
