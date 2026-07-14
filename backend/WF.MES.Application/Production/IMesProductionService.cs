using WF.MES.Application.Production.Dtos;

namespace WF.MES.Application.Production;

public interface IMesProductionService
{
    Task<List<MesWorkOrderDto>> GetWorkOrdersAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<MesWorkOrderDto?> GetWorkOrderByNoAsync(string workOrderNo, CancellationToken cancellationToken = default);
    Task<long> SaveWorkOrderAsync(SaveMesWorkOrderRequest request, long operatorId, CancellationToken cancellationToken = default);
    Task CloseWorkOrderAsync(long id, string? remark, long operatorId, CancellationToken cancellationToken = default);
    Task<MesReportResultDto> SubmitReportAsync(MesReportRequest request, string operatorName, long operatorId, CancellationToken cancellationToken = default);
    Task<List<MesReportRecordDto>> GetRecentReportsAsync(string? workOrderNo = null, int take = 50, CancellationToken cancellationToken = default);
}
