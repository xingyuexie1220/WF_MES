namespace WF.MES.Infrastructure.Services;

/// <summary>角色菜单权限辅助：勾选子菜单时自动补全祖先目录节点。</summary>
internal static class MenuPermissionHelper
{
    internal static HashSet<long> ExpandWithAncestors(
        IEnumerable<long> menuIds,
        IReadOnlyDictionary<long, long> parentMap)
    {
        var result = new HashSet<long>(menuIds);
        foreach (var menuId in menuIds)
        {
            var current = menuId;
            while (parentMap.TryGetValue(current, out var parentId) && parentId > 0)
            {
                result.Add(parentId);
                current = parentId;
            }
        }

        return result;
    }
}
