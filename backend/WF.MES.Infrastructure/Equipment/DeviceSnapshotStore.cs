using WF.MES.Application.Equipment.Dtos;
using WF.MES.Infrastructure.Cache;

namespace WF.MES.Infrastructure.Equipment;

public interface IDeviceSnapshotStore
{
    Task SetSnapshotAsync(DeviceSnapshotDto snapshot, CancellationToken cancellationToken = default);

    Task<DeviceSnapshotDto?> GetSnapshotAsync(string factoryCode, string deviceId, CancellationToken cancellationToken = default);

    Task<List<DeviceSnapshotDto>> GetRecentSnapshotsAsync(string factoryCode, CancellationToken cancellationToken = default);
}

public class RedisDeviceSnapshotStore(IRedisCacheService redis) : IDeviceSnapshotStore
{
    private const string SnapshotPrefix = "wf:device:";
    private static readonly TimeSpan SnapshotTtl = TimeSpan.FromHours(24);

    public async Task SetSnapshotAsync(DeviceSnapshotDto snapshot, CancellationToken cancellationToken = default)
    {
        await redis.SetJsonAsync(BuildSnapshotKey(snapshot.FactoryCode, snapshot.DeviceId), snapshot, SnapshotTtl);
        await AddToFactoryIndexAsync(snapshot.FactoryCode, snapshot.DeviceId);
    }

    public Task<DeviceSnapshotDto?> GetSnapshotAsync(string factoryCode, string deviceId, CancellationToken cancellationToken = default)
        => redis.GetJsonAsync<DeviceSnapshotDto>(BuildSnapshotKey(factoryCode, deviceId));

    public async Task<List<DeviceSnapshotDto>> GetRecentSnapshotsAsync(string factoryCode, CancellationToken cancellationToken = default)
    {
        var indexKey = BuildIndexKey(factoryCode);
        var ids = await redis.GetJsonAsync<List<string>>(indexKey) ?? [];
        var snapshots = new List<DeviceSnapshotDto>();
        var activeIds = new List<string>();

        foreach (var deviceId in ids)
        {
            var snapshot = await GetSnapshotAsync(factoryCode, deviceId, cancellationToken);
            if (snapshot is not null)
            {
                snapshots.Add(snapshot);
                activeIds.Add(deviceId);
            }
        }

        if (activeIds.Count != ids.Count)
        {
            await redis.SetJsonAsync(indexKey, activeIds, SnapshotTtl);
        }

        return snapshots;
    }

    private async Task AddToFactoryIndexAsync(string factoryCode, string deviceId)
    {
        var indexKey = BuildIndexKey(factoryCode);
        var ids = await redis.GetJsonAsync<List<string>>(indexKey) ?? [];
        if (!ids.Contains(deviceId, StringComparer.OrdinalIgnoreCase))
        {
            ids.Add(deviceId);
            await redis.SetJsonAsync(indexKey, ids, SnapshotTtl);
        }
    }

    private static string BuildSnapshotKey(string factoryCode, string deviceId)
        => $"{SnapshotPrefix}{factoryCode}:{deviceId}:snapshot";

    private static string BuildIndexKey(string factoryCode)
        => $"{SnapshotPrefix}{factoryCode}:index";
}
