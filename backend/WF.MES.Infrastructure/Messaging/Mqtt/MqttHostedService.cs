using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WF.MES.Application.Equipment;
using WF.MES.Application.Equipment.Dtos;
using WF.MES.Application.Messaging;
using WF.MES.Infrastructure.Equipment;
using WF.MES.Infrastructure.Options;

namespace WF.MES.Infrastructure.Messaging.Mqtt;

public partial class EquipmentTelemetryHandler(
    IDeviceSnapshotStore snapshotStore,
    IDeviceTelemetryNotifier notifier,
    ILogger<EquipmentTelemetryHandler> logger) : IMqttMessageHandler
{
    private const int MaxPayloadBytes = 64 * 1024;
    private const int MaxSegmentLength = 64;

    public async Task HandleAsync(string topic, string payload, CancellationToken cancellationToken = default)
    {
        if (Encoding.UTF8.GetByteCount(payload) > MaxPayloadBytes)
        {
            logger.LogWarning("MQTT payload too large on topic {Topic}", topic);
            return;
        }

        var envelope = ParseTopic(topic, payload);
        if (envelope is null)
        {
            logger.LogDebug("Ignored MQTT topic {Topic}", topic);
            return;
        }

        logger.LogInformation(
            "MQTT {MessageType} from {Factory}/{Device}: {PayloadLength} bytes",
            envelope.MessageType,
            envelope.FactoryCode,
            envelope.DeviceId,
            payload.Length);

        var snapshot = new DeviceSnapshotDto
        {
            FactoryCode = envelope.FactoryCode,
            DeviceId = envelope.DeviceId,
            LastMessageType = envelope.MessageType,
            LastPayload = envelope.Payload,
            LastReceivedAt = envelope.ReceivedAt
        };
        await snapshotStore.SetSnapshotAsync(snapshot, cancellationToken);

        await notifier.NotifyAsync(new DeviceTelemetryDto
        {
            FactoryCode = envelope.FactoryCode,
            DeviceId = envelope.DeviceId,
            Topic = envelope.Topic,
            MessageType = envelope.MessageType,
            Payload = envelope.Payload,
            ReceivedAt = envelope.ReceivedAt
        }, cancellationToken);
    }

    internal static MqttMessageEnvelope? ParseTopic(string topic, string payload)
    {
        // wf/{factoryCode}/equipment/{deviceId}/{messageType}
        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5
            || !string.Equals(parts[0], "wf", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(parts[2], "equipment", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!IsValidSegment(parts[1]) || !IsValidSegment(parts[3]) || !IsValidMessageType(parts[4]))
        {
            return null;
        }

        return new MqttMessageEnvelope
        {
            FactoryCode = parts[1],
            DeviceId = parts[3],
            MessageType = parts[4],
            Topic = topic,
            Payload = payload,
            ReceivedAt = DateTime.Now
        };
    }

    private static bool IsValidSegment(string value)
        => !string.IsNullOrWhiteSpace(value)
            && value.Length <= MaxSegmentLength
            && TopicSegmentPattern().IsMatch(value);

    private static bool IsValidMessageType(string value)
        => string.Equals(value, "telemetry", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "alarm", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "status", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex("^[A-Za-z0-9._-]+$")]
    private static partial Regex TopicSegmentPattern();
}

public class MqttMessageEnvelope
{
    public string FactoryCode { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}

public class MqttConnectionTracker
{
    private int _isConnected;

    public bool IsConnected
    {
        get => Interlocked.CompareExchange(ref _isConnected, 0, 0) == 1;
        set => Interlocked.Exchange(ref _isConnected, value ? 1 : 0);
    }

    public DateTime? LastConnectedAt { get; set; }
    public string? LastError { get; set; }
}

public class MqttHostedService(
    IOptions<MqttOptions> options,
    IEnumerable<IMqttMessageHandler> handlers,
    MqttConnectionTracker tracker,
    ILogger<MqttHostedService> logger) : BackgroundService
{
    private readonly MqttOptions _options = options.Value;
    private readonly SemaphoreSlim _reconnectGate = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("MQTT subscriber disabled by configuration");
            return;
        }

        if (_options.SubscribeTopics.Length == 0)
        {
            logger.LogWarning("MQTT enabled but SubscribeTopics is empty");
            return;
        }

        var factory = new MQTTnet.MqttFactory();
        using var client = factory.CreateMqttClient();

        client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic ?? string.Empty;
            var payloadSegment = e.ApplicationMessage.PayloadSegment;
            var payload = payloadSegment.Array is null
                ? string.Empty
                : Encoding.UTF8.GetString(payloadSegment.Array, payloadSegment.Offset, payloadSegment.Count);

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(topic, payload, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "MQTT handler failed for topic {Topic}", topic);
                }
            }
        };

        client.DisconnectedAsync += async e =>
        {
            tracker.IsConnected = false;
            tracker.LastError = e.Exception?.Message;

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            logger.LogWarning(e.Exception, "MQTT disconnected, reconnecting in 5s");

            if (!await _reconnectGate.WaitAsync(TimeSpan.FromSeconds(1), stoppingToken))
            {
                return;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                if (!stoppingToken.IsCancellationRequested && !client.IsConnected)
                {
                    await ConnectAndSubscribeAsync(client, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            catch (Exception reconnectEx)
            {
                logger.LogError(reconnectEx, "MQTT reconnect failed");
            }
            finally
            {
                _reconnectGate.Release();
            }
        };

        await ConnectAndSubscribeAsync(client, stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }

        if (client.IsConnected)
        {
            await client.DisconnectAsync(new MQTTnet.Client.MqttClientDisconnectOptions(), CancellationToken.None);
        }
    }

    private async Task ConnectAndSubscribeAsync(MQTTnet.Client.IMqttClient client, CancellationToken cancellationToken)
    {
        var builder = new MQTTnet.Client.MqttClientOptionsBuilder()
            .WithClientId(_options.ClientId)
            .WithTcpServer(_options.Host, _options.Port);

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            builder.WithCredentials(_options.Username, _options.Password);
        }

        if (_options.UseTls)
        {
            builder.WithTlsOptions(o => o.UseTls());
        }

        await client.ConnectAsync(builder.Build(), cancellationToken);
        tracker.IsConnected = true;
        tracker.LastConnectedAt = DateTime.Now;
        tracker.LastError = null;
        logger.LogInformation("MQTT connected to {Host}:{Port}", _options.Host, _options.Port);

        var qos = _options.QoS switch
        {
            0 => MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce,
            2 => MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce,
            _ => MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce
        };

        var filters = _options.SubscribeTopics
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => new MQTTnet.Packets.MqttTopicFilter
            {
                Topic = topic,
                QualityOfServiceLevel = qos
            })
            .ToList();

        if (filters.Count == 0)
        {
            logger.LogWarning("MQTT has no valid subscribe topics after filtering");
            return;
        }

        await client.SubscribeAsync(new MQTTnet.Client.MqttClientSubscribeOptions
        {
            TopicFilters = filters
        }, cancellationToken);

        logger.LogInformation("MQTT subscribed to {Count} topics", filters.Count);
    }
}

public class MqttHealthCheck(IOptions<MqttOptions> options, MqttConnectionTracker tracker) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("MQTT disabled"));
        }

        if (tracker.IsConnected)
        {
            return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("MQTT connected"));
        }

        return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
            "MQTT not connected"));
    }
}
