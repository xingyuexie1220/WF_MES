namespace WF.MES.Application.Equipment;

public interface IEquipmentScaffoldService
{
    Task<object> GetTestRecordsAsync(CancellationToken cancellationToken = default);
    Task<object> SubmitTestAsync(object request, CancellationToken cancellationToken = default);
}
