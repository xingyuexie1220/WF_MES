namespace WF.MES.Infrastructure.Services;

using Microsoft.Extensions.Options;
using WF.MES.Application.Common;
using WF.MES.Infrastructure.Equipment;
using WF.MES.Infrastructure.Options;

public class MasterDataScaffoldService : Application.MasterData.IMasterDataScaffoldService
{
    public Task<object> GetMaterialsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", items = Array.Empty<object>(), total = 0 });

    public Task<object> GetRoutesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", items = Array.Empty<object>(), total = 0 });

    public Task<object> GetStationsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", items = Array.Empty<object>(), total = 0 });

    public Task<object> GetWorkCentersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", items = Array.Empty<object>(), total = 0 });
}

public class ProductionScaffoldService : Application.Production.IProductionScaffoldService
{
    public Task<object> GetWorkOrdersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", items = Array.Empty<object>(), total = 0 });

    public Task<object> PassStationAsync(object request, CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", accepted = true, request });
}

public class WarehouseScaffoldService : Application.Warehouse.IWarehouseScaffoldService
{
    public Task<object> GetInboundListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", items = Array.Empty<object>(), total = 0 });

    public Task<object> SubmitScanAsync(object request, CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", accepted = true, request });
}

public class DashboardScaffoldService(
    Application.Common.ICacheService cache,
    ICurrentUserService currentUser,
    IOptions<CacheOptions> cacheOptions) : Application.Dashboard.IDashboardScaffoldService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;

    public Task<object> GetReportOverviewAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", charts = Array.Empty<object>() });

    public async Task<object> GetBigScreenOverviewAsync(CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetAsync<object>("dashboard", "bigscreen", currentUser.FactoryId, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var data = new
        {
            status = "scaffold",
            kpis = new object[]
            {
                new { name = "产量", value = 0 },
                new { name = "良品率", value = "100%" },
                new { name = "设备在线", value = 0 }
            },
            cachedAt = DateTime.Now
        };

        await cache.SetAsync(
            "dashboard",
            "bigscreen",
            data,
            TimeSpan.FromMinutes(_cacheOptions.DashboardTtlMinutes),
            currentUser.FactoryId,
            cancellationToken);

        return data;
    }
}

public class EquipmentScaffoldService(
    IDeviceSnapshotStore snapshotStore,
    IFactoryContext factoryContext) : Application.Equipment.IEquipmentScaffoldService
{
    public async Task<object> GetTestRecordsAsync(CancellationToken cancellationToken = default)
    {
        var factoryCode = factoryContext.CurrentFactoryCode;
        if (string.IsNullOrWhiteSpace(factoryCode))
        {
            return new { status = "scaffold", items = Array.Empty<object>(), total = 0 };
        }

        var snapshots = await snapshotStore.GetRecentSnapshotsAsync(factoryCode, cancellationToken);
        var items = snapshots.Select(s => new
        {
            deviceId = s.DeviceId,
            messageType = s.LastMessageType,
            payload = s.LastPayload,
            receivedAt = s.LastReceivedAt
        }).ToList();

        return new { status = snapshots.Count > 0 ? "live" : "scaffold", items, total = items.Count };
    }

    public Task<object> SubmitTestAsync(object request, CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { status = "scaffold", accepted = true, request });
}
