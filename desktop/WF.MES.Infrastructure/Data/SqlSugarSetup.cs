using Microsoft.Extensions.Configuration;
using SqlSugar;
using WF.MES.Core.Exceptions;

namespace WF.MES.Infrastructure.Data;

/// <summary>SqlSugar 工厂；注册为单例 SqlSugarScope（线程安全），SQL Debug 输出。</summary>
public static class SqlSugarSetup
{
    /// <summary>创建 SqlSugarScope 单例；连接串自动启用 MARS。</summary>
    public static ISqlSugarClient CreateClient(IConfiguration configuration)
    {
        var connectionString = EnsureMultipleActiveResultSets(configuration.GetConnectionString("WfMesDb")
            ?? throw new BusinessException("err.dbConnectionNotConfigured"));

        var db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        },
        scope =>
        {
            scope.Aop.OnLogExecuting = (sql, pars) =>
            {
                Serilog.Log.Debug("SQL: {Sql}", UtilMethods.GetSqlString(DbType.SqlServer, sql, pars));
            };
        });

        return db;
    }

    private static string EnsureMultipleActiveResultSets(string connectionString)
    {
        if (connectionString.Contains("MultipleActiveResultSets", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        return connectionString.TrimEnd(';') + ";MultipleActiveResultSets=True";
    }
}
