namespace WF.MES.Models.Dtos;

/// <summary>用户可见的功能模块及下属二级小菜单。</summary>
public class ModuleMenuPermissionDto
{
    public int ModuleId { get; init; }

    /// <summary>功能模块名称。</summary>
    public string ModuleName { get; init; } = string.Empty;

    public string? Icon { get; init; }

    /// <summary>功能模块下的二级小菜单列表。</summary>
    public IList<MenuPermissionDto> Menus { get; init; } = [];
}

public class MenuPermissionDto
{
    public int MenuId { get; init; }

    public int ModuleId { get; init; }

    /// <summary>二级小菜单名称。</summary>
    public string MenuName { get; init; } = string.Empty;

    public string ViewName { get; init; } = string.Empty;
}
