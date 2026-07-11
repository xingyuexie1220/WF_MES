using Microsoft.AspNetCore.SignalR;
using WF.MES.Api.Hubs;
using WF.MES.Application.Equipment;
using WF.MES.Application.Equipment.Dtos;

namespace WF.MES.Api.Services;

public class DeviceTelemetryNotifier(IHubContext<NoticeHub> hubContext) : IDeviceTelemetryNotifier
{
    public Task NotifyAsync(DeviceTelemetryDto telemetry, CancellationToken cancellationToken = default)
        => hubContext.Clients
            .Group(telemetry.FactoryCode)
            .SendAsync("DeviceTelemetryReceived", telemetry, cancellationToken);
}
