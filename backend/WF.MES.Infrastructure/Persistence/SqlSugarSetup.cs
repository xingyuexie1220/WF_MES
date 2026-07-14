using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WF.MES.Application.Common;
using WF.MES.Domain.Common;

namespace WF.MES.Infrastructure.Persistence;

public static class SqlSugarSetup
{
    public static IServiceCollection AddWfSqlSugar(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddScoped<ISqlSugarClient>(sp =>
        {
            var db = CreateClient(connectionString);
            db.Aop.OnLogExecuting = (sql, pars) => Console.WriteLine($"[SqlSugar] {sql}");

            var factoryContext = sp.GetRequiredService<IFactoryContext>();
            if (factoryContext.IsFilterEnabled && factoryContext.CurrentFactoryId is long factoryId)
            {
                // 捕获当前请求的 factoryId 常量，避免静态方法/三元被译成非法 SQL（0 OR ...）。
                db.QueryFilter.AddTableFilter<IFactoryScoped>(entity => entity.FactoryId == factoryId);
            }

            return db;
        });

        return services;
    }

    private static SqlSugarClient CreateClient(string connectionString) => new(new ConnectionConfig
    {
        ConnectionString = connectionString,
        DbType = DbType.SqlServer,
        IsAutoCloseConnection = true,
        InitKeyType = InitKeyType.Attribute,
        ConfigureExternalServices = new ConfigureExternalServices
        {
            EntityService = (_, column) =>
            {
                if (column.IsPrimarykey && column.PropertyName == "Id")
                {
                    column.IsIdentity = true;
                }
            }
        }
    });
}
