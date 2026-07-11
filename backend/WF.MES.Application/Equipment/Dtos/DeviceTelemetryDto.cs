namespace WF.MES.Application.Equipment.Dtos;

public class DeviceTelemetryDto
{
    public string FactoryCode { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}

public class DeviceSnapshotDto
{
    public string FactoryCode { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string LastMessageType { get; set; } = string.Empty;
    public string LastPayload { get; set; } = string.Empty;
    public DateTime LastReceivedAt { get; set; }
}
