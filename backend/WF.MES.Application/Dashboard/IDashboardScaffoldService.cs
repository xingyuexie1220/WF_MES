namespace WF.MES.Application.Dashboard;

public interface IDashboardScaffoldService
{
    Task<object> GetReportOverviewAsync(CancellationToken cancellationToken = default);
    Task<object> GetBigScreenOverviewAsync(CancellationToken cancellationToken = default);
}
