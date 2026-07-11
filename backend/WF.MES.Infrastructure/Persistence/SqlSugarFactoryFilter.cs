namespace WF.MES.Infrastructure.Persistence;

/// <summary>供 SqlSugar 全局过滤器读取当前工厂（Middleware 写入）。</summary>
public static class SqlSugarFactoryFilter
{
    private static readonly AsyncLocal<long?> CurrentFactoryId = new();
    private static readonly AsyncLocal<bool> FilterEnabled = new();

    public static void Set(long? factoryId, bool enabled)
    {
        CurrentFactoryId.Value = factoryId;
        FilterEnabled.Value = enabled;
    }

    public static void Clear()
    {
        CurrentFactoryId.Value = null;
        FilterEnabled.Value = false;
    }

    public static bool IsFilterEnabled() => FilterEnabled.Value;

    public static long GetFactoryId() => CurrentFactoryId.Value ?? 0;
}
