using SqlSugar;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Services;

public sealed class DatabaseHealthService : IDatabaseHealthService
{
    private readonly ISqlSugarClient _db;

    public DatabaseHealthService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.Ado.GetIntAsync("SELECT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
