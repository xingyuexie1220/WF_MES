using WF.MES.Shared.Enums;

namespace WF.MES.Application.Positions.Dtos;

public class PositionDto
{
    public long Id { get; set; }
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string? ProcessCode { get; set; }
    public long? DeptId { get; set; }
    public string? DeptName { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
}

public class PositionQueryRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? PositionName { get; set; }
    public long? DeptId { get; set; }
    public UserStatus? Status { get; set; }
}

public class CreatePositionRequest
{
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string? ProcessCode { get; set; }
    public long? DeptId { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Enabled;
    public string? Remark { get; set; }
}

public class UpdatePositionRequest
{
    public string PositionName { get; set; } = string.Empty;
    public string? ProcessCode { get; set; }
    public long? DeptId { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
}
