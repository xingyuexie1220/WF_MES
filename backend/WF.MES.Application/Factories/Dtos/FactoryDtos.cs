using WF.MES.Shared.Enums;

namespace WF.MES.Application.Factories.Dtos;

public class FactoryDto
{
    public long Id { get; set; }
    public long RegionId { get; set; }
    public string? RegionName { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}

public class FactorySummaryDto
{
    public long Id { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class RegionDto
{
    public long Id { get; set; }
    public string RegionCode { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public int Sort { get; set; }
    public UserStatus Status { get; set; }
}

public class CreateFactoryRequest
{
    public long RegionId { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Enabled;
    public string? Remark { get; set; }
}

public class UpdateFactoryRequest
{
    public long RegionId { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
}

public class SwitchFactoryRequest
{
    public long FactoryId { get; set; }
}

public class SelectFactoryRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public long FactoryId { get; set; }
    public ClientType ClientType { get; set; } = ClientType.Web;
    public string? Locale { get; set; }
}
