namespace WF.MES.Application.MasterData;

public interface IMasterDataScaffoldService
{
    Task<object> GetMaterialsAsync(CancellationToken cancellationToken = default);
    Task<object> GetRoutesAsync(CancellationToken cancellationToken = default);
    Task<object> GetStationsAsync(CancellationToken cancellationToken = default);
    Task<object> GetWorkCentersAsync(CancellationToken cancellationToken = default);
}
