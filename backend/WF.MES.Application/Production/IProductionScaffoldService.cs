namespace WF.MES.Application.Production;

public interface IProductionScaffoldService
{
    Task<object> GetWorkOrdersAsync(CancellationToken cancellationToken = default);
    Task<object> PassStationAsync(object request, CancellationToken cancellationToken = default);
}
