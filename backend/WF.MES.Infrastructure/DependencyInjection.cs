using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using WF.MES.Application.Auth;
using WF.MES.Application.Common;
using WF.MES.Application.Depts;
using WF.MES.Application.Dicts;
using WF.MES.Application.Factories;
using WF.MES.Application.Jobs;
using WF.MES.Application.Logs;
using WF.MES.Application.Menus;
using WF.MES.Application.Notices;
using WF.MES.Application.Positions;
using WF.MES.Application.Roles;
using WF.MES.Application.Sessions;
using WF.MES.Application.Users;
using WF.MES.Infrastructure.Cache;
using WF.MES.Infrastructure.Filters;
using WF.MES.Infrastructure.Jobs;
using WF.MES.Application.Messaging;
using WF.MES.Infrastructure.Equipment;
using WF.MES.Infrastructure.Messaging;
using WF.MES.Infrastructure.Messaging.Mqtt;
using WF.MES.Infrastructure.Modules;
using WF.MES.Infrastructure.Options;
using WF.MES.Infrastructure.Persistence;
using WF.MES.Infrastructure.Security;
using WF.MES.Infrastructure.Services;

namespace WF.MES.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWfInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.Configure<LogCleanupOptions>(configuration.GetSection(LogCleanupOptions.SectionName));

        services.AddWfSqlSugar(configuration);
        services.AddHttpContextAccessor();

        services.AddScoped<IFactoryContextAccessor, FactoryContextAccessor>();
        services.AddScoped<IFactoryContext, FactoryContext>();
        services.AddScoped<JwtTokenService>();
        services.AddSingleton<ISessionStore, RedisSessionStore>();
        services.AddSingleton<ISessionMetaStore, RedisSessionMetaStore>();
        services.AddSingleton<IRefreshTokenBlacklist, RedisRefreshTokenBlacklist>();
        services.AddSingleton<Application.Common.ICacheService, CacheService>();
        services.AddSingleton<IDeviceSnapshotStore, RedisDeviceSnapshotStore>();
        services.AddSingleton<MqttConnectionTracker>();
        services.AddSingleton<IMqttMessageHandler, EquipmentTelemetryHandler>();
        services.AddHostedService<MqttHostedService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDataScopeQueryFilter, DataScopeQueryFilter>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFactoryService, FactoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IDeptService, DeptService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<INoticeService, NoticeService>();
        services.AddScoped<IDictService, DictService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<ISessionAdminService, SessionAdminService>();
        services.AddScoped<OperationLogFilter>();

        services.AddWfModules();

        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(WfQuartzJobKeys.LogCleanupJob);
            q.AddJob<WfLogCleanupJob>(opts => opts
                .WithIdentity(jobKey)
                .WithDescription("清理7天前本地 Serilog 日志文件"));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(WfQuartzJobKeys.LogCleanupTrigger)
                .WithCronSchedule(WfQuartzJobKeys.LogCleanupCron));
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
