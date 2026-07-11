namespace WF.MES.Core.Interfaces;

/// <summary>Backend API 连通性探测（无需登录）。</summary>
public interface IApiHealthService
{
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
}
