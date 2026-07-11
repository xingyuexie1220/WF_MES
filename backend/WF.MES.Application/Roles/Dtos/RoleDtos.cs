using WF.MES.Shared.Enums;

namespace WF.MES.Application.Roles.Dtos;

public class RoleDto
{
    public long Id { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int Sort { get; set; }
    public DataScopeType DataScope { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
    public long? CreateBy { get; set; }
    public string? CreateByName { get; set; }
    public DateTime CreateTime { get; set; }
    public long? UpdateBy { get; set; }
    public string? UpdateByName { get; set; }
    public DateTime? UpdateTime { get; set; }
    public List<long> MenuIds { get; set; } = [];
    public List<long> DeptIds { get; set; } = [];
}

public class RoleQueryRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? RoleName { get; set; }
    public UserStatus? Status { get; set; }
}

public class CreateRoleRequest
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int Sort { get; set; }
    public DataScopeType DataScope { get; set; } = DataScopeType.Dept;
    public UserStatus Status { get; set; } = UserStatus.Enabled;
    public string? Remark { get; set; }
    public List<long> MenuIds { get; set; } = [];
    public List<long> DeptIds { get; set; } = [];
}

public class UpdateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public int Sort { get; set; }
    public DataScopeType DataScope { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
    public List<long> MenuIds { get; set; } = [];
    public List<long> DeptIds { get; set; } = [];
}
