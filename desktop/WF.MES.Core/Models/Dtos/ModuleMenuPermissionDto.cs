namespace WF.MES.Models.Dtos;

/// <summary>用户可见的功能模块及下属二级小菜单（标题在 UI 渲染时按 I18nKey 翻译）。</summary>
public class ModuleMenuPermissionDto
{
    public int ModuleId { get; init; }

    /// <summary>菜单 i18n 键；为空时用 <see cref="TitleFallback"/>。</summary>
    public string? I18nKey { get; init; }

    /// <summary>服务端 Title 回退。</summary>
    public string TitleFallback { get; init; } = string.Empty;

    public string? Icon { get; init; }

    /// <summary>功能模块下的二级小菜单列表。</summary>
    public IList<MenuPermissionDto> Menus { get; init; } = [];
}

public class MenuPermissionDto
{
    public int MenuId { get; init; }

    public int ModuleId { get; init; }

    public string? I18nKey { get; init; }

    public string TitleFallback { get; init; } = string.Empty;

    public string ViewName { get; init; } = string.Empty;
}
