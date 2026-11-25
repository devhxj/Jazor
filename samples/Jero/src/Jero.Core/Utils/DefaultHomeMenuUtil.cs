namespace Jero.Core;

/// <summary>
/// 后台默认首页菜单解析工具
/// </summary>
public static class DefaultHomeMenuUtil
{
    /// <summary>
    /// 根据配置的默认首页地址查找菜单。匹配不到配置地址时回退到系统兜底首页。
    /// </summary>
    public static SysMenu? FindDefaultHomeMenu(IEnumerable<SysMenu> menus, string? defaultHomePath)
    {
        var menuList = menus.Where(u => u.Type != MenuTypeEnum.Btn).ToList();
        if (menuList.Count == 0) return null;

        var configuredPath = GetComparableRoutePath(DefaultHomePathUtil.NormalizeOrFallback(defaultHomePath));
        var menu = FindMenuByPath(menuList, configuredPath);
        if (menu != null) return menu;

        var fallbackPath = GetComparableRoutePath(DefaultHomePathUtil.FallbackHomePath);
        menu = FindMenuByPath(menuList, fallbackPath);
        if (menu != null) return menu;

        return menuList.FirstOrDefault(u => string.Equals(u.Name, "home", StringComparison.OrdinalIgnoreCase))
            ?? menuList.FirstOrDefault(u => u.IsAffix);
    }

    /// <summary>
    /// 获取默认首页菜单、父级菜单和子级菜单。
    /// </summary>
    public static IReadOnlyList<SysMenu> GetDefaultHomeMenuTree(IEnumerable<SysMenu> menus, string? defaultHomePath)
    {
        var menuList = menus.ToList();
        var homeMenu = FindDefaultHomeMenu(menuList, defaultHomePath);
        if (homeMenu == null) return [];

        var result = new List<SysMenu>();
        AddAncestors(menuList, homeMenu, result);
        AddUnique(result, homeMenu);
        AddDescendants(menuList, homeMenu.Id, result);
        return result;
    }

    /// <summary>
    /// 将固定页签移动到当前默认首页菜单。
    /// </summary>
    public static void ApplyDefaultHomeAffix(IEnumerable<SysMenu> menus, string? defaultHomePath)
    {
        var menuList = menus.ToList();
        var homeMenu = FindDefaultHomeMenu(menuList, defaultHomePath);
        if (homeMenu == null) return;

        foreach (var menu in menuList.Where(u => u.Type != MenuTypeEnum.Btn))
        {
            menu.IsAffix = menu.Id == homeMenu.Id;
        }
    }

    private static SysMenu? FindMenuByPath(IEnumerable<SysMenu> menus, string path)
    {
        return menus.FirstOrDefault(u => string.Equals(GetComparableRoutePath(u.Path), path, StringComparison.Ordinal));
    }

    private static string GetComparableRoutePath(string? value)
    {
        var path = (value ?? string.Empty).Trim();
        var queryIndex = path.IndexOfAny(new[] { '?', '#' });
        if (queryIndex >= 0) path = path[..queryIndex];
        return path.Length > 1 ? path.TrimEnd('/') : path;
    }

    private static void AddAncestors(IReadOnlyCollection<SysMenu> menus, SysMenu menu, ICollection<SysMenu> result)
    {
        if (menu.Pid == 0) return;

        var parent = menus.FirstOrDefault(u => u.Id == menu.Pid);
        if (parent == null) return;

        AddAncestors(menus, parent, result);
        AddUnique(result, parent);
    }

    private static void AddDescendants(IReadOnlyCollection<SysMenu> menus, long parentId, ICollection<SysMenu> result)
    {
        foreach (var child in menus.Where(u => u.Pid == parentId).OrderBy(u => u.OrderNo).ThenBy(u => u.Id))
        {
            AddUnique(result, child);
            AddDescendants(menus, child.Id, result);
        }
    }

    private static void AddUnique(ICollection<SysMenu> result, SysMenu menu)
    {
        if (!result.Any(u => u.Id == menu.Id)) result.Add(menu);
    }
}
