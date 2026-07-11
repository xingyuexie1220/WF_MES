using WF.MES.Shared.Enums;

namespace WF.MES.Application.Menus.Dtos;

public class MenuDto
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public MenuType MenuType { get; set; }
    public ClientType ClientType { get; set; }
    public string? I18nKey { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Permission { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
    public bool Visible { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public long? CreateBy { get; set; }
    public string? CreateByName { get; set; }
    public DateTime? UpdateTime { get; set; }
    public long? UpdateBy { get; set; }
    public string? UpdateByName { get; set; }
    public List<MenuDto> Children { get; set; } = [];
}

public class CreateMenuRequest
{
    public long ParentId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public MenuType MenuType { get; set; }
    public ClientType ClientType { get; set; } = ClientType.Web;
    public string? I18nKey { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Permission { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
    public bool Visible { get; set; } = true;
    public UserStatus Status { get; set; } = UserStatus.Enabled;
    public string? Remark { get; set; }
}

public class UpdateMenuRequest
{
    public long ParentId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public MenuType MenuType { get; set; }
    public ClientType ClientType { get; set; } = ClientType.Web;
    public string? I18nKey { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Permission { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
    public bool Visible { get; set; }
    public UserStatus Status { get; set; }
    public string? Remark { get; set; }
}

public class RouterMenuDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Component { get; set; }
    public string? Redirect { get; set; }
    public RouterMetaDto Meta { get; set; } = new();
    public List<RouterMenuDto> Children { get; set; } = [];
}

public class RouterMetaDto
{
    public string Title { get; set; } = string.Empty;
    public string? I18nKey { get; set; }
    public string? Icon { get; set; }
    public bool Hidden { get; set; }
    public List<string> Permissions { get; set; } = [];
}

/// <summary>uni-app 手机端菜单树</summary>
public class MobileMenuDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? I18nKey { get; set; }
    public string? Icon { get; set; }
    /// <summary>uni-app 页面路由，如 /pages/warehouse/scan</summary>
    public string? Path { get; set; }
    /// <summary>uni-app 页面路径，如 pages/warehouse/scan</summary>
    public string? PagePath { get; set; }
    public int Sort { get; set; }
    public List<MobileMenuDto> Children { get; set; } = [];
}

/// <summary>三端统一客户端菜单树（Web / Mobile / Desktop）</summary>
public class ClientMenuDto
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public MenuType MenuType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? I18nKey { get; set; }
    public string? Icon { get; set; }
    public string? Path { get; set; }
    /// <summary>Web 组件 / WPF Prism 导航名 / Mobile pagePath</summary>
    public string? Component { get; set; }
    public string? Permission { get; set; }
    public bool Visible { get; set; } = true;
    public int Sort { get; set; }
    public List<ClientMenuDto> Children { get; set; } = [];
}
