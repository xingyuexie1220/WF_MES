using WF.MES.Shared.Enums;

namespace WF.MES.Application.Depts.Dtos;

public class DeptDto
{
    public long Id { get; set; }
    public long FactoryId { get; set; }
    public long ParentId { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public DeptType DeptType { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public long? CreateBy { get; set; }
    public string? CreateByName { get; set; }
    public DateTime? UpdateTime { get; set; }
    public long? UpdateBy { get; set; }
    public string? UpdateByName { get; set; }
    public List<DeptDto> Children { get; set; } = [];
}

public class CreateDeptRequest
{
    public long ParentId { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public DeptType DeptType { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Enabled;
    public string? Remark { get; set; }
}

public class UpdateDeptRequest
{
    public long ParentId { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public DeptType DeptType { get; set; }
    public int Sort { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
}
