using SqlSugar;
using WF.MES.Application.Auth;
using WF.MES.Application.Common;
using WF.MES.Application.Sessions;
using WF.MES.Application.Sessions.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Infrastructure.Security;
using WF.MES.Shared.Common;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class SessionAdminService(
    ISqlSugarClient db,
    ISessionStore sessionStore,
    ISessionMetaStore sessionMetaStore,
    IAuthService authService,
    ICurrentUserService currentUser) : ISessionAdminService
{
    public async Task<PagedResult<SessionDto>> GetPagedListAsync(SessionQueryRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var tokens = await db.Queryable<SysRefreshToken>()
            .Where(t => !t.IsRevoked && t.ExpireTime > now)
            .ToListAsync(cancellationToken);

        var latestTokens = tokens
            .GroupBy(t => new { t.UserId, t.ClientType })
            .Select(g => g.OrderByDescending(t => t.CreateTime).First())
            .ToList();

        if (latestTokens.Count == 0)
        {
            return EmptyPage(request);
        }

        var userIds = latestTokens.Select(t => t.UserId).Distinct().ToList();
        var users = await db.Queryable<SysUser>()
            .Where(u => userIds.Contains(u.Id) && !u.IsDeleted)
            .ToListAsync(cancellationToken);
        var userMap = users.ToDictionary(u => u.Id);

        var sessions = new List<SessionDto>();
        foreach (var token in latestTokens)
        {
            if (!userMap.TryGetValue(token.UserId, out var user))
            {
                continue;
            }

            if (!MatchesUserName(user, request.UserName))
            {
                continue;
            }

            if (request.ClientType.HasValue && token.ClientType != request.ClientType.Value)
            {
                continue;
            }

            var sessionId = token.SessionId ?? string.Empty;
            var isActive = !string.IsNullOrEmpty(sessionId)
                && await sessionStore.IsActiveSessionAsync(token.UserId, token.ClientType, sessionId, token.FactoryId, cancellationToken);

            if (request.OnlyActive && !isActive)
            {
                continue;
            }

            var meta = !string.IsNullOrEmpty(sessionId)
                ? await sessionMetaStore.GetAsync(sessionId, cancellationToken)
                : null;

            sessions.Add(new SessionDto
            {
                UserId = token.UserId,
                UserName = user.UserName,
                NickName = user.NickName,
                ClientType = token.ClientType,
                SessionId = sessionId,
                LoginTime = token.CreateTime,
                ExpireTime = token.ExpireTime,
                IsActive = isActive,
                IpAddress = meta?.IpAddress,
                UserAgent = meta?.UserAgent
            });
        }

        var ordered = sessions
            .OrderByDescending(s => s.LoginTime)
            .ToList();

        var pageIndex = Math.Max(request.PageIndex, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var skip = (pageIndex - 1) * pageSize;

        return new PagedResult<SessionDto>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Total = ordered.Count,
            Items = ordered.Skip(skip).Take(pageSize).ToList()
        };
    }

    public async Task KickAsync(KickSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId == request.UserId && currentUser.ClientType == request.ClientType)
        {
            throw new BusinessException("不能强制下线当前会话");
        }

        var user = await db.Queryable<SysUser>()
            .FirstAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            throw new BusinessException("用户不存在", 404);
        }

        await authService.LogoutAsync(request.UserId, request.ClientType, null, cancellationToken);
    }

    private static bool MatchesUserName(SysUser user, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        return user.UserName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || (user.NickName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static PagedResult<SessionDto> EmptyPage(SessionQueryRequest request)
        => new()
        {
            PageIndex = Math.Max(request.PageIndex, 1),
            PageSize = Math.Clamp(request.PageSize, 1, 200),
            Total = 0,
            Items = []
        };
}
