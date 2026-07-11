namespace WF.MES.Application.Common;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string category, string key, long? factoryId = null, CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string category,
        string key,
        T value,
        TimeSpan? expiry = null,
        long? factoryId = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string category, string key, long? factoryId = null, CancellationToken cancellationToken = default);

    Task RemoveCategoryAsync(string category, long? factoryId = null, CancellationToken cancellationToken = default);
}
