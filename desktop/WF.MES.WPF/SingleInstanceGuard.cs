namespace WF.MES.WPF;

/// <summary>
/// 限制同一台电脑仅运行一个客户端进程（Global Mutex）。
/// 同账号单设备策略由 Backend API + Redis 会话负责。
/// </summary>
public sealed class SingleInstanceGuard : IDisposable
{
    // Global\ 前缀：本机所有用户会话共享，防止切换 Windows 用户后重复启动
    private const string MutexName = @"Global\WF.MES.WPF.SingleInstance";

    private readonly Mutex _mutex;
    private bool _disposed;

    private SingleInstanceGuard(Mutex mutex)
    {
        _mutex = mutex;
    }

    public static bool TryAcquire(out SingleInstanceGuard? guard)
    {
        var mutex = new Mutex(true, MutexName, out var createdNew);
        if (createdNew)
        {
            guard = new SingleInstanceGuard(mutex);
            return true;
        }

        mutex.Dispose();
        guard = null;
        return false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _mutex.ReleaseMutex();
        _mutex.Dispose();
    }
}
