using WF.MES.Shared.Enums;

namespace WF.MES.Application.Users.Dtos;

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? Email { get; set; }
    public long DeptId { get; set; }
    public string? DeptName { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public long? CreateBy { get; set; }
    public string? CreateByName { get; set; }
    public DateTime? UpdateTime { get; set; }
    public long? UpdateBy { get; set; }
    public string? UpdateByName { get; set; }
    public List<long> RoleIds { get; set; } = [];
    public List<long> PositionIds { get; set; } = [];
}

public class UserQueryRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? UserName { get; set; }
    public UserStatus? Status { get; set; }
    public long? DeptId { get; set; }
}

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? Email { get; set; }
    public long DeptId { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Enabled;
    public string? Remark { get; set; }
    public List<long> RoleIds { get; set; } = [];
    public List<long> PositionIds { get; set; } = [];
}

public class UpdateUserRequest
{
    public string? NickName { get; set; }
    public string? Email { get; set; }
    public long DeptId { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
    public List<long> RoleIds { get; set; } = [];
    public List<long> PositionIds { get; set; } = [];
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
