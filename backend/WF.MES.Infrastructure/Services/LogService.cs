using SqlSugar;
using WF.MES.Application.Logs;
using WF.MES.Application.Logs.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Shared.Common;

namespace WF.MES.Infrastructure.Services;

public class LogService(ISqlSugarClient db) : ILogService
{
    public async Task<PagedResult<OperationLogDto>> GetOperationPagedListAsync(OperationLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var items = await BuildOperationQuery(request)
            .OrderByDescending(l => l.OperTime)
            .Select(l => new OperationLogDto
            {
                Id = l.Id,
                Module = l.Module,
                Title = l.Title,
                BusinessType = l.BusinessType,
                Method = l.Method,
                RequestMethod = l.RequestMethod,
                OperUrl = l.OperUrl,
                OperIp = l.OperIp,
                OperParam = l.OperParam,
                Status = l.Status,
                ErrorMsg = l.ErrorMsg,
                OperUserId = l.OperUserId,
                OperUserName = l.OperUserName,
                OperTime = l.OperTime,
                CostTime = l.CostTime
            })
            .ToPageListAsync(request.PageIndex, request.PageSize, total);

        return new PagedResult<OperationLogDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<PagedResult<ExceptionLogDto>> GetExceptionPagedListAsync(ExceptionLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var items = await BuildExceptionQuery(request)
            .OrderByDescending(l => l.ExceptionTime)
            .Select(l => new ExceptionLogDto
            {
                Id = l.Id,
                Module = l.Module,
                Message = l.Message,
                StackTrace = l.StackTrace,
                RequestUrl = l.RequestUrl,
                RequestMethod = l.RequestMethod,
                RequestParam = l.RequestParam,
                OperIp = l.OperIp,
                OperUserId = l.OperUserId,
                OperUserName = l.OperUserName,
                ExceptionTime = l.ExceptionTime
            })
            .ToPageListAsync(request.PageIndex, request.PageSize, total);

        return new PagedResult<ExceptionLogDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<List<OperationLogDto>> ExportOperationLogsAsync(OperationLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        return await BuildOperationQuery(request)
            .OrderByDescending(l => l.OperTime)
            .Select(l => new OperationLogDto
            {
                Id = l.Id,
                Module = l.Module,
                Title = l.Title,
                BusinessType = l.BusinessType,
                Method = l.Method,
                RequestMethod = l.RequestMethod,
                OperUrl = l.OperUrl,
                OperIp = l.OperIp,
                OperParam = l.OperParam,
                Status = l.Status,
                ErrorMsg = l.ErrorMsg,
                OperUserId = l.OperUserId,
                OperUserName = l.OperUserName,
                OperTime = l.OperTime,
                CostTime = l.CostTime
            })
            .Take(10000)
            .ToListAsync();
    }

    public async Task<List<ExceptionLogDto>> ExportExceptionLogsAsync(ExceptionLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        return await BuildExceptionQuery(request)
            .OrderByDescending(l => l.ExceptionTime)
            .Select(l => new ExceptionLogDto
            {
                Id = l.Id,
                Module = l.Module,
                Message = l.Message,
                StackTrace = l.StackTrace,
                RequestUrl = l.RequestUrl,
                RequestMethod = l.RequestMethod,
                RequestParam = l.RequestParam,
                OperIp = l.OperIp,
                OperUserId = l.OperUserId,
                OperUserName = l.OperUserName,
                ExceptionTime = l.ExceptionTime
            })
            .Take(10000)
            .ToListAsync();
    }

    public async Task ClearOperationLogsAsync(ClearLogRequest request, CancellationToken cancellationToken = default)
    {
        if (request.BeforeTime.HasValue)
        {
            await db.Deleteable<SystemOperationLog>()
                .Where(l => l.OperTime < request.BeforeTime.Value)
                .ExecuteCommandAsync();
            return;
        }

        await db.Deleteable<SystemOperationLog>().ExecuteCommandAsync();
    }

    public async Task ClearExceptionLogsAsync(ClearLogRequest request, CancellationToken cancellationToken = default)
    {
        if (request.BeforeTime.HasValue)
        {
            await db.Deleteable<SystemExceptionLog>()
                .Where(l => l.ExceptionTime < request.BeforeTime.Value)
                .ExecuteCommandAsync();
            return;
        }

        await db.Deleteable<SystemExceptionLog>().ExecuteCommandAsync();
    }

    public async Task WriteExceptionLogAsync(ExceptionLogDto log, CancellationToken cancellationToken = default)
    {
        var entity = new SystemExceptionLog
        {
            Module = log.Module,
            Message = log.Message,
            StackTrace = log.StackTrace,
            RequestUrl = log.RequestUrl,
            RequestMethod = log.RequestMethod,
            RequestParam = log.RequestParam,
            OperIp = log.OperIp,
            OperUserId = log.OperUserId,
            OperUserName = log.OperUserName,
            ExceptionTime = DateTime.Now
        };

        await db.Insertable(entity).ExecuteCommandAsync();
    }

    private ISugarQueryable<SystemOperationLog> BuildOperationQuery(OperationLogQueryRequest request)
    {
        return db.Queryable<SystemOperationLog>()
            .WhereIF(!string.IsNullOrWhiteSpace(request.Module), l => l.Module!.Contains(request.Module!))
            .WhereIF(!string.IsNullOrWhiteSpace(request.OperUserName), l => l.OperUserName!.Contains(request.OperUserName!))
            .WhereIF(request.Status.HasValue, l => l.Status == request.Status)
            .WhereIF(request.BeginTime.HasValue, l => l.OperTime >= request.BeginTime)
            .WhereIF(request.EndTime.HasValue, l => l.OperTime <= request.EndTime);
    }

    private ISugarQueryable<SystemExceptionLog> BuildExceptionQuery(ExceptionLogQueryRequest request)
    {
        return db.Queryable<SystemExceptionLog>()
            .WhereIF(!string.IsNullOrWhiteSpace(request.Module), l => l.Module!.Contains(request.Module!))
            .WhereIF(!string.IsNullOrWhiteSpace(request.OperUserName), l => l.OperUserName!.Contains(request.OperUserName!))
            .WhereIF(request.BeginTime.HasValue, l => l.ExceptionTime >= request.BeginTime)
            .WhereIF(request.EndTime.HasValue, l => l.ExceptionTime <= request.EndTime);
    }
}
