using WF.MES.Application.Equipment.Dtos;

namespace WF.MES.Application.Equipment;

public interface IDeviceTelemetryNotifier
{
    Task NotifyAsync(DeviceTelemetryDto telemetry, CancellationToken cancellationToken = default);
}
