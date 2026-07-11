using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WF.MES.Domain.Common;

namespace WF.MES.Infrastructure.Persistence;

public static class SqlSugarSetup
{
    public static IServiceCollection AddWfSqlSugar(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddSingleton<ISqlSugarClient>(sp =>
        {
            var db = new SqlSugarScope(new ConnectionConfig
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
            }, scope =>
            {
                scope.Aop.OnLogExecuting = (sql, pars) => Console.WriteLine($"[SqlSugar] {sql}");

                scope.QueryFilter.AddTableFilter<IFactoryScoped>(entity =>
                    !SqlSugarFactoryFilter.IsFilterEnabled()
                    || entity.FactoryId == SqlSugarFactoryFilter.GetFactoryId());
            });

            return db;
        });

        return services;
    }
}
