namespace WF.MES.Application.Warehouse;

public interface IWarehouseScaffoldService
{
    Task<object> GetInboundListAsync(CancellationToken cancellationToken = default);
    Task<object> SubmitScanAsync(object request, CancellationToken cancellationToken = default);
}
