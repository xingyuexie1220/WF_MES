using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WF.MES.Infrastructure.Options;

namespace WF.MES.Infrastructure.Cache;

public interface IRedisCacheService
{
    Task<string?> GetStringAsync(string key);

    /// <summary>null 表示 Redis 不可用；否则表示 key 是否存在。</summary>
    Task<bool?> KeyExistsAsync(string key);

    Task SetStringAsync(string key, string value, TimeSpan? expiry = null);

    Task RemoveAsync(string key);

    Task<T?> GetJsonAsync<T>(string key);

    Task SetJsonAsync<T>(string key, T value, TimeSpan? expiry = null);

    Task RemoveByPrefixAsync(string prefix);
}

public class RedisCacheService : IRedisCacheService, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RedisOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    private ConnectionMultiplexer? _connection;
    private readonly object _lock = new();

    public RedisCacheService(IOptions<RedisOptions> options, ILogger<RedisCacheService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var database = GetDatabase();
        if (database is null)
        {
            return null;
        }

        var value = await database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool?> KeyExistsAsync(string key)
    {
        var database = GetDatabase();
        if (database is null)
        {
            return null;
        }

        return await database.KeyExistsAsync(key);
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        var database = GetDatabase();
        if (database is null)
        {
            return;
        }

        if (expiry.HasValue)
        {
            await database.StringSetAsync(key, value, expiry.Value);
            return;
        }

        await database.StringSetAsync(key, value);
    }

    public async Task RemoveAsync(string key)
    {
        var database = GetDatabase();
        if (database is not null)
        {
            await database.KeyDeleteAsync(key);
        }
    }

    public async Task<T?> GetJsonAsync<T>(string key)
    {
        var json = await GetStringAsync(key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Redis JSON deserialize failed for key {Key}", key);
            return default;
        }
    }

    public async Task SetJsonAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await SetStringAsync(key, json, expiry);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var connection = GetConnection();
        if (connection is null)
        {
            return;
        }

        try
        {
            var server = connection.GetServer(connection.GetEndPoints().First());
            var database = connection.GetDatabase(_options.DefaultDatabase);
            var batch = new List<RedisKey>();

            await foreach (var key in server.KeysAsync(database: _options.DefaultDatabase, pattern: $"{prefix}*"))
            {
                batch.Add(key);
                if (batch.Count < 500)
                {
                    continue;
                }

                await database.KeyDeleteAsync(batch.ToArray());
                batch.Clear();
            }

            if (batch.Count > 0)
            {
                await database.KeyDeleteAsync(batch.ToArray());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis prefix delete failed for {Prefix}", prefix);
        }
    }

    private IDatabase? GetDatabase() => GetConnection()?.GetDatabase(_options.DefaultDatabase);

    private ConnectionMultiplexer? GetConnection()
    {
        try
        {
            if (_connection is { IsConnected: true })
            {
                return _connection;
            }

            lock (_lock)
            {
                if (_connection is { IsConnected: true })
                {
                    return _connection;
                }

                if (_connection is not null)
                {
                    try
                    {
                        _connection.Dispose();
                    }
                    catch (Exception disposeEx)
                    {
                        _logger.LogDebug(disposeEx, "Redis connection dispose failed during reconnect");
                    }

                    _connection = null;
                }

                _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
                return _connection;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable");
            return null;
        }
    }

    public void Dispose() => _connection?.Dispose();
}
