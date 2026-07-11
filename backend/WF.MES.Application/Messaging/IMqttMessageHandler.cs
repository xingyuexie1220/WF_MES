namespace WF.MES.Application.Messaging;

public interface IMqttMessageHandler
{
    Task HandleAsync(string topic, string payload, CancellationToken cancellationToken = default);
}
