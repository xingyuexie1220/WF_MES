using SqlSugar;
using WF.MES.Domain.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("System_User")]
public class SystemUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? NickName { get; set; }

    public string? Email { get; set; }

    public long DeptId { get; set; }

    public long? DefaultFactoryId { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Enabled;

    public string? Remark { get; set; }

    public DateTime? LastLoginTime { get; set; }

    /// <summary>新建账号或管理员重置密码后，下次登录须修改密码。</summary>
    public bool MustChangePassword { get; set; }
}
