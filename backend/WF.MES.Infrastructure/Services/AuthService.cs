using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SqlSugar;
using WF.MES.Application.Auth;
using WF.MES.Application.Auth.Dtos;
using WF.MES.Application.Common;
using WF.MES.Application.Factories;
using WF.MES.Application.Factories.Dtos;
using WF.MES.Application.Menus.Dtos;
using WF.MES.Domain.Entities;
using WF.MES.Infrastructure.Options;
using WF.MES.Infrastructure.Security;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Enums;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Infrastructure.Services;

public class AuthService(
    ISqlSugarClient db,
    JwtTokenService jwtTokenService,
    ISessionStore sessionStore,
    ISessionMetaStore sessionMetaStore,
    IRefreshTokenBlacklist refreshTokenBlacklist,
    IFactoryService factoryService,
    ICurrentUserService currentUserService,
    Application.Common.ICacheService appCache,
    IHttpContextAccessor httpContextAccessor,
    IOptions<JwtOptions> jwtOptions,
    IOptions<CacheOptions> cacheOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await ValidateCredentialsAsync(request.UserName, request.Password, cancellationToken);
        var factories = await factoryService.GetAccessibleFactoriesAsync(user.Id, cancellationToken);

        if (factories.Count == 0)
        {
            throw new BusinessException("用户未分配可访问工厂", 403, WfMessageCodes.AuthFactoryForbidden);
        }

        if (!request.FactoryId.HasValue)
        {
            if (factories.Count == 1)
            {
                request.FactoryId = factories[0].Id;
            }
            else
            {
                var defaultFactory = factories.FirstOrDefault(f => f.IsDefault) ?? factories[0];
                return new LoginResponse
                {
                    NeedSelectFactory = true,
                    Factories = factories,
                    UserInfo = new UserInfoDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        NickName = user.NickName,
                        AccessibleFactories = factories
                    }
                };
            }
        }

        await factoryService.EnsureUserCanAccessFactoryAsync(user.Id, request.FactoryId.Value, cancellationToken);
        return await IssueTokensAsync(user, request.ClientType, request.FactoryId.Value, cancellationToken);
    }

    public async Task<LoginResponse> SelectFactoryAsync(SelectFactoryRequest request, CancellationToken cancellationToken = default)
    {
        var user = await ValidateCredentialsAsync(request.UserName, request.Password, cancellationToken);
        await factoryService.EnsureUserCanAccessFactoryAsync(user.Id, request.FactoryId, cancellationToken);
        return await IssueTokensAsync(user, request.ClientType, request.FactoryId, cancellationToken);
    }

    public async Task<LoginResponse> SwitchFactoryAsync(long userId, SwitchFactoryRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Queryable<SystemUser>().FirstAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new BusinessException("用户不存在", 404, WfMessageCodes.AuthUserNotFound);

        if (user.Status != UserStatus.Enabled)
        {
            throw new BusinessException("账号已禁用", 403, WfMessageCodes.AuthUserDisabled);
        }

        await factoryService.EnsureUserCanAccessFactoryAsync(userId, request.FactoryId, cancellationToken);

        var clientType = await GetLatestClientTypeAsync(userId, cancellationToken);
        return await IssueTokensAsync(user, clientType, request.FactoryId, cancellationToken);
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (await refreshTokenBlacklist.IsBlacklistedAsync(request.RefreshToken, cancellationToken))
        {
            throw new BusinessException("Refresh Token 无效或已过期", 401, WfMessageCodes.AuthRefreshInvalid);
        }

        var tokenEntity = await db.Queryable<SystemRefreshToken>()
            .FirstAsync(t => t.Token == request.RefreshToken && !t.IsRevoked, cancellationToken);

        if (tokenEntity is null || tokenEntity.ExpireTime < DateTime.Now)
        {
            throw new BusinessException("Refresh Token 无效或已过期", 401, WfMessageCodes.AuthRefreshInvalid);
        }

        var user = await db.Queryable<SystemUser>().FirstAsync(u => u.Id == tokenEntity.UserId && !u.IsDeleted, cancellationToken);
        if (user is null || user.Status != UserStatus.Enabled)
        {
            throw new BusinessException("用户不存在或已禁用", 401, WfMessageCodes.AuthUserNotFound);
        }

        if (!tokenEntity.FactoryId.HasValue)
        {
            throw new BusinessException("会话缺少工厂上下文", 401, WfMessageCodes.AuthFactoryRequired);
        }

        await BlacklistRefreshTokenAsync(tokenEntity.Token, tokenEntity.ExpireTime, cancellationToken);

        tokenEntity.IsRevoked = true;
        await db.Updateable(tokenEntity).UpdateColumns(t => new { t.IsRevoked }).ExecuteCommandAsync(cancellationToken);

        return await IssueTokensAsync(user, tokenEntity.ClientType, tokenEntity.FactoryId.Value, cancellationToken, tokenEntity.SessionId);
    }

    public async Task LogoutAsync(long userId, ClientType clientType, long? factoryId, CancellationToken cancellationToken = default)
    {
        var tokens = await db.Queryable<SystemRefreshToken>()
            .Where(t => t.UserId == userId && t.ClientType == clientType && !t.IsRevoked)
            .WhereIF(factoryId.HasValue, t => t.FactoryId == factoryId)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            await BlacklistRefreshTokenAsync(token.Token, token.ExpireTime, cancellationToken);
        }

        await db.Updateable<SystemRefreshToken>()
            .SetColumns(t => t.IsRevoked == true)
            .Where(t => t.UserId == userId && t.ClientType == clientType && !t.IsRevoked)
            .WhereIF(factoryId.HasValue, t => t.FactoryId == factoryId)
            .ExecuteCommandAsync(cancellationToken);

        await sessionStore.RemoveSessionAsync(userId, clientType, factoryId, cancellationToken);
    }

    public async Task<UserInfoDto> GetCurrentUserInfoAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Queryable<SystemUser>().FirstAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new BusinessException("用户不存在", 404, WfMessageCodes.AuthUserNotFound);

        var roles = await GetUserRolesAsync(userId);
        var permissions = await GetUserPermissionsAsync(userId);
        var factoryId = currentUserService.FactoryId ?? user.DefaultFactoryId;
        return await BuildUserInfoAsync(user, roles, permissions, factoryId, cancellationToken);
    }

    public async Task<UserInfoDto> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new BusinessException("旧密码和新密码不能为空");
        }

        if (request.NewPassword.Length < 6)
        {
            throw new BusinessException("新密码长度不能少于 6 位", 400, WfMessageCodes.AuthPasswordTooShort);
        }

        if (request.OldPassword == request.NewPassword)
        {
            throw new BusinessException("新密码不能与当前密码相同", 400, WfMessageCodes.AuthPasswordSameAsOld);
        }

        var user = await db.Queryable<SystemUser>().FirstAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new BusinessException("用户不存在", 404, WfMessageCodes.AuthUserNotFound);

        if (!PasswordHasher.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new BusinessException("当前密码不正确", 400, WfMessageCodes.AuthInvalidCredentials);
        }

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        user.UpdateBy = userId;
        user.UpdateTime = DateTime.Now;
        await db.Updateable(user).UpdateColumns(u => new { u.PasswordHash, u.MustChangePassword, u.UpdateBy, u.UpdateTime }).ExecuteCommandAsync(cancellationToken);

        var roles = await GetUserRolesAsync(userId);
        var permissions = await GetUserPermissionsAsync(userId);
        return await BuildUserInfoAsync(user, roles, permissions, user.DefaultFactoryId, cancellationToken);
    }

    public async Task<List<ClientMenuDto>> GetCurrentUserMenusAsync(long userId, ClientType clientType, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"menus:{userId}:{(int)clientType}";
        var cached = await appCache.GetAsync<List<ClientMenuDto>>("menu", cacheKey, cancellationToken: cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var menus = await GetPermittedMenusAsync(userId, clientType, cancellationToken);
        var result = BuildClientMenuTree(menus, 0);
        await appCache.SetAsync(
            "menu",
            cacheKey,
            result,
            TimeSpan.FromMinutes(_cacheOptions.MenuTtlMinutes),
            cancellationToken: cancellationToken);
        return result;
    }

    public async Task<List<RouterMenuDto>> GetCurrentUserRoutersAsync(long userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"routers:{userId}";
        var cached = await appCache.GetAsync<List<RouterMenuDto>>("menu", cacheKey, cancellationToken: cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var menus = await GetPermittedMenusAsync(userId, ClientType.Web, cancellationToken);
        var result = BuildRouterTree(menus, 0);
        await appCache.SetAsync(
            "menu",
            cacheKey,
            result,
            TimeSpan.FromMinutes(_cacheOptions.MenuTtlMinutes),
            cancellationToken: cancellationToken);
        return result;
    }

    public async Task<List<MobileMenuDto>> GetCurrentUserMobileMenusAsync(long userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"mobile:{userId}";
        var cached = await appCache.GetAsync<List<MobileMenuDto>>("menu", cacheKey, cancellationToken: cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var menus = await GetPermittedMenusAsync(userId, ClientType.Mobile, cancellationToken);
        var result = BuildMobileMenuTree(menus, 0);
        await appCache.SetAsync(
            "menu",
            cacheKey,
            result,
            TimeSpan.FromMinutes(_cacheOptions.MenuTtlMinutes),
            cancellationToken: cancellationToken);
        return result;
    }

    private async Task<LoginResponse> IssueTokensAsync(
        SystemUser user,
        ClientType clientType,
        long factoryId,
        CancellationToken cancellationToken,
        string? existingSessionId = null)
    {
        var factory = await db.Queryable<SystemFactory>().FirstAsync(f => f.Id == factoryId && !f.IsDeleted, cancellationToken)
            ?? throw new BusinessException("工厂不存在", 404, WfMessageCodes.FactoryNotFound);

        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);
        var sessionId = existingSessionId ?? Guid.NewGuid().ToString("N");

        await RevokeClientTokensAsync(user.Id, clientType, factoryId, cancellationToken);

        var (accessToken, _) = jwtTokenService.CreateAccessToken(user, roles, permissions, clientType, sessionId, factoryId, factory.FactoryCode);
        var refreshToken = jwtTokenService.CreateRefreshToken();
        var sessionExpiry = TimeSpan.FromDays(_jwtOptions.RefreshTokenExpireDays);

        await db.Insertable(new SystemRefreshToken
        {
            UserId = user.Id,
            ClientType = clientType,
            FactoryId = factoryId,
            SessionId = sessionId,
            Token = refreshToken,
            ExpireTime = DateTime.Now.Add(sessionExpiry),
            CreateTime = DateTime.Now
        }).ExecuteCommandAsync(cancellationToken);

        await sessionStore.SetActiveSessionAsync(user.Id, clientType, sessionId, sessionExpiry, factoryId, cancellationToken);

        var httpContext = httpContextAccessor.HttpContext;
        await sessionMetaStore.SetAsync(new SessionMeta
        {
            SessionId = sessionId,
            UserId = user.Id,
            UserName = user.UserName,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            LoginTime = DateTime.Now
        }, sessionExpiry, cancellationToken);

        user.LastLoginTime = DateTime.Now;
        user.DefaultFactoryId = factoryId;
        await db.Updateable(user).UpdateColumns(u => new { u.LastLoginTime, u.DefaultFactoryId }).ExecuteCommandAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtOptions.AccessTokenExpireMinutes * 60,
            UserInfo = await BuildUserInfoAsync(user, roles, permissions, factoryId, cancellationToken, factory)
        };
    }

    private async Task<SystemUser> ValidateCredentialsAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var user = await db.Queryable<SystemUser>()
            .FirstAsync(u => u.UserName == userName && !u.IsDeleted, cancellationToken);

        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            throw new BusinessException("用户名或密码错误", 401, WfMessageCodes.AuthInvalidCredentials);
        }

        if (user.Status != UserStatus.Enabled)
        {
            throw new BusinessException("账号已禁用", 403, WfMessageCodes.AuthUserDisabled);
        }

        return user;
    }

    private async Task<ClientType> GetLatestClientTypeAsync(long userId, CancellationToken cancellationToken)
    {
        var latest = await db.Queryable<SystemRefreshToken>()
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .OrderByDescending(t => t.CreateTime)
            .Select(t => t.ClientType)
            .FirstAsync(cancellationToken);

        return latest == 0 ? ClientType.Web : latest;
    }

    private async Task<List<SystemMenu>> GetPermittedMenusAsync(long userId, ClientType clientType, CancellationToken cancellationToken)
    {
        var roleIds = await db.Queryable<SystemUserRole>().Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync();
        if (roleIds.Count == 0)
        {
            return [];
        }

        var menuIds = await db.Queryable<SystemRoleMenu>().Where(rm => roleIds.Contains(rm.RoleId)).Select(rm => rm.MenuId).Distinct().ToListAsync();
        if (menuIds.Count == 0)
        {
            return [];
        }

        var parentMap = await BuildMenuParentMapAsync(clientType, cancellationToken);
        var expandedMenuIds = MenuPermissionHelper.ExpandWithAncestors(menuIds, parentMap);

        return await db.Queryable<SystemMenu>()
            .Where(m => expandedMenuIds.Contains(m.Id) && !m.IsDeleted && m.Status == UserStatus.Enabled
                && m.MenuType != MenuType.Button && m.ClientType == clientType)
            .OrderBy(m => m.Sort)
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<long, long>> BuildMenuParentMapAsync(ClientType clientType, CancellationToken cancellationToken)
    {
        // 含按钮，便于 role_menu 仅有按钮 id 时也能展开到页面/目录
        var menus = await db.Queryable<SystemMenu>()
            .Where(m => !m.IsDeleted && m.ClientType == clientType)
            .Select(m => new { m.Id, m.ParentId })
            .ToListAsync(cancellationToken);

        return menus.ToDictionary(m => m.Id, m => m.ParentId);
    }

    private async Task RevokeClientTokensAsync(long userId, ClientType clientType, long factoryId, CancellationToken cancellationToken)
    {
        var tokens = await db.Queryable<SystemRefreshToken>()
            .Where(t => t.UserId == userId && t.ClientType == clientType && t.FactoryId == factoryId && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            await BlacklistRefreshTokenAsync(token.Token, token.ExpireTime, cancellationToken);
        }

        await db.Updateable<SystemRefreshToken>()
            .SetColumns(t => t.IsRevoked == true)
            .Where(t => t.UserId == userId && t.ClientType == clientType && t.FactoryId == factoryId && !t.IsRevoked)
            .ExecuteCommandAsync(cancellationToken);

        await sessionStore.RemoveSessionAsync(userId, clientType, factoryId, cancellationToken);
    }

    private async Task BlacklistRefreshTokenAsync(string refreshToken, DateTime expireTime, CancellationToken cancellationToken)
    {
        var remaining = expireTime - DateTime.Now;
        if (remaining > TimeSpan.Zero)
        {
            await refreshTokenBlacklist.AddAsync(refreshToken, remaining, cancellationToken);
        }
    }

    private async Task<UserInfoDto> BuildUserInfoAsync(
        SystemUser user,
        List<string> roles,
        List<string> permissions,
        long? factoryId,
        CancellationToken cancellationToken,
        SystemFactory? factory = null)
    {
        var deptName = await db.Queryable<SystemDept>().Where(d => d.Id == user.DeptId).Select(d => d.DeptName).FirstAsync(cancellationToken);
        var accessibleFactories = await factoryService.GetAccessibleFactoriesAsync(user.Id, cancellationToken);

        if (factory is null && factoryId.HasValue)
        {
            factory = await db.Queryable<SystemFactory>().FirstAsync(f => f.Id == factoryId && !f.IsDeleted, cancellationToken);
        }

        return new UserInfoDto
        {
            Id = user.Id,
            UserName = user.UserName,
            NickName = user.NickName,
            DeptId = user.DeptId,
            DeptName = deptName,
            FactoryId = factory?.Id ?? factoryId,
            FactoryCode = factory?.FactoryCode,
            FactoryName = factory?.FactoryName,
            MustChangePassword = user.MustChangePassword,
            Roles = roles,
            Permissions = permissions,
            AccessibleFactories = accessibleFactories
        };
    }

    private async Task<List<string>> GetUserRolesAsync(long userId)
    {
        return await db.Queryable<SystemUserRole>()
            .InnerJoin<SystemRole>((ur, r) => ur.RoleId == r.Id)
            .Where((ur, r) => ur.UserId == userId && !r.IsDeleted && r.Status == UserStatus.Enabled)
            .Select((ur, r) => r.RoleCode)
            .ToListAsync();
    }

    private async Task<List<string>> GetUserPermissionsAsync(long userId)
    {
        var roleIds = await db.Queryable<SystemUserRole>().Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync();
        if (roleIds.Count == 0)
        {
            return [];
        }

        return await db.Queryable<SystemRoleMenu>()
            .InnerJoin<SystemMenu>((rm, m) => rm.MenuId == m.Id)
            .Where((rm, m) => roleIds.Contains(rm.RoleId) && !m.IsDeleted && m.Status == UserStatus.Enabled && m.Permission != null)
            .Select((rm, m) => m.Permission!)
            .Distinct()
            .ToListAsync();
    }

    private static List<ClientMenuDto> BuildClientMenuTree(List<SystemMenu> menus, long parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .Select(m => new ClientMenuDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                MenuType = m.MenuType,
                Title = m.MenuName,
                I18nKey = m.I18nKey,
                Icon = m.Icon,
                Path = m.Path,
                Component = m.Component,
                Permission = m.Permission,
                Visible = m.Visible,
                Sort = m.Sort,
                Children = BuildClientMenuTree(menus, m.Id)
            })
            .ToList();
    }

    private static List<RouterMenuDto> BuildRouterTree(List<SystemMenu> menus, long parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .Select(m => new RouterMenuDto
            {
                Id = m.Id,
                Name = m.Path?.Replace("/", "") ?? $"menu_{m.Id}",
                Path = m.Path ?? $"/menu-{m.Id}",
                Component = m.Component,
                Meta = new RouterMetaDto
                {
                    Title = m.MenuName,
                    I18nKey = m.I18nKey,
                    Icon = m.Icon,
                    Hidden = !m.Visible
                },
                Children = BuildRouterTree(menus, m.Id)
            })
            .ToList();
    }

    private static List<MobileMenuDto> BuildMobileMenuTree(List<SystemMenu> menus, long parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .Select(m => new MobileMenuDto
            {
                Id = m.Id,
                Title = m.MenuName,
                I18nKey = m.I18nKey,
                Icon = m.Icon,
                Path = m.Path,
                PagePath = m.Component,
                Sort = m.Sort,
                Children = BuildMobileMenuTree(menus, m.Id)
            })
            .ToList();
    }
}
