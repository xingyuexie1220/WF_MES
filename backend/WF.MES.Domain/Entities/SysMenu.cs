using SqlSugar;
using WF.MES.Domain.Common;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Menu")]
public class SysMenu : BaseEntity
{
    public long ParentId { get; set; }

    public string MenuName { get; set; } = string.Empty;

    public MenuType MenuType { get; set; }

    public string? Path { get; set; }

    public string? Component { get; set; }

    public string? Permission { get; set; }

    public string? Icon { get; set; }

    public ClientType ClientType { get; set; } = ClientType.Web;

    public string? I18nKey { get; set; }

    public int Sort { get; set; }

    public bool Visible { get; set; } = true;

    public UserStatus Status { get; set; } = UserStatus.Enabled;

    public string? Remark { get; set; }
}
