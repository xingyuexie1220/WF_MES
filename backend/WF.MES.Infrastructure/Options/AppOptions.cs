namespace WF.MES.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "WF.MES";
    public string Audience { get; set; } = "WF.MES.Client";
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpireMinutes { get; set; } = 30;
    public int RefreshTokenExpireDays { get; set; } = 7;
}

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public int DefaultDatabase { get; set; } = 0;
}

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class CacheOptions
{
    public const string SectionName = "Cache";

    public int DefaultTtlMinutes { get; set; } = 30;
    public int MenuTtlMinutes { get; set; } = 15;
    public int DictTtlMinutes { get; set; } = 30;
    public int DashboardTtlMinutes { get; set; } = 1;
}

public class MqttOptions
{
    public const string SectionName = "Mqtt";

    public bool Enabled { get; set; }
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "wf-mes-api";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseTls { get; set; }
    public int QoS { get; set; } = 1;
    public string[] SubscribeTopics { get; set; } =
    [
        "wf/+/equipment/+/telemetry",
        "wf/+/equipment/+/alarm",
        "wf/+/equipment/+/status"
    ];
}
