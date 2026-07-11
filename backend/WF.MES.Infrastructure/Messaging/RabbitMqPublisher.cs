using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WF.MES.Infrastructure.Options;

namespace WF.MES.Infrastructure.Messaging;

public interface IRabbitMqPublisher
{
    void Publish(string queueName, string message);
}

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public void Publish(string queueName, string message)
    {
        try
        {
            EnsureConnection();
            if (_connection is null || !_connection.IsOpen)
            {
                _logger.LogWarning("RabbitMQ is unavailable, message skipped: {Message}", message);
                return;
            }

            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: null, body: body);
            _logger.LogInformation("RabbitMQ published to {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ publish failed, message skipped");
        }
    }

    private void EnsureConnection()
    {
        if (_connection is { IsOpen: true })
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnection();
    }

    public void Dispose() => _connection?.Dispose();
}
