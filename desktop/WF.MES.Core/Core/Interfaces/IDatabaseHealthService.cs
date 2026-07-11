namespace WF.MES.Core.Interfaces;

/// <summary>数据库连通性探测。</summary>
public interface IDatabaseHealthService
{
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
}
