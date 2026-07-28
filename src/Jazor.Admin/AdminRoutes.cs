namespace Jazor.Admin;

[ECMAScript]
[Description("@#")]
public sealed record AdminRouteDefinition : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#path")]
    public string? Path { get; init; }

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#subtitle")]
    public string? Subtitle { get; init; }

    [Description("@#icon")]
    public string? Icon { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#children")]
    public AdminRouteDefinition[]? Children { get; init; }
}

[ECMAScriptModule("components/jazor-admin-admin-route-catalog.mjs")]
public static class AdminRouteCatalog
{
    private const string CatchAllPath = "/:pathMatch(.*)*";

    public static bool ContainsPath(AdminRouteDefinition[] routes, string path)
        => FindByPath(routes, path) is not null;

    public static AdminRouteDefinition Resolve(
        AdminRouteDefinition[] routes,
        string path,
        string fallbackKey)
        => FindByPath(routes, path)
           ?? FindByKey(routes, fallbackKey)
           ?? routes[0];

    public static AdminNavItems BuildNavigation(AdminRouteDefinition[] routes)
    {
        var items = new AdminNavItem[routes.Length];
        for (var index = 0; index < routes.Length; index++)
        {
            items[index] = BuildNavigationItem(routes[index]);
        }

        return items;
    }

    public static AdminBreadcrumbItem[] BuildBreadcrumbs(
        AdminRouteDefinition[] routes,
        string selectedKey,
        AdminBreadcrumbItem? root = null)
    {
        var routePath = new List<AdminRouteDefinition>();
        _ = CollectRoutePath(routes, selectedKey, routePath);

        var offset = root is null ? 0 : 1;
        var items = new AdminBreadcrumbItem[routePath.Count + offset];
        if (root is not null)
        {
            items[0] = root;
        }

        for (var index = 0; index < routePath.Count; index++)
        {
            var route = routePath[index];
            items[index + offset] = new AdminBreadcrumbItem
            {
                Key = route.Key,
                Title = route.Title,
                RouteTarget = CreateRouteTarget(route.Path),
                Disabled = route.Path is null
            };
        }

        return items;
    }

    public static string[] BuildExpandedKeys(
        AdminRouteDefinition[] routes,
        string selectedKey,
        string[]? expandedKeys)
    {
        var keys = new HashSet<string>(expandedKeys ?? Array.Empty<string>());
        var routePath = new List<AdminRouteDefinition>();
        if (CollectRoutePath(routes, selectedKey, routePath))
        {
            for (var index = 0; index < routePath.Count - 1; index++)
            {
                keys.Add(routePath[index].Key);
            }
        }

        var resultList = new List<string>(keys.Count);
        foreach (var key in keys)
        {
            resultList.Add(key);
        }

        var result = resultList.ToArray();
        Array.Sort(result);
        return result;
    }

    public static RouteRecordRaw[] BuildRouteRecords(
        AdminRouteDefinition[] routes,
        IVueComponent shellComponent,
        string fallbackKey)
    {
        var records = new List<RouteRecordRaw>();
        AddRouteRecords(routes, shellComponent, records);

        if (!ContainsPath(routes, CatchAllPath))
        {
            var fallback = FindByKey(routes, fallbackKey) ?? routes[0];
            records.Add(new RouteRecordRedirect
            {
                Path = CatchAllPath,
                Redirect = RouteRecordRedirectOption.From((RouteLocationRaw)(fallback.Path ?? "/"))
            });
        }

        return records.ToArray();
    }

    private static AdminNavItem BuildNavigationItem(AdminRouteDefinition route)
        => new()
        {
            Key = route.Key,
            Title = route.Title,
            Icon = route.Icon,
            RouteTarget = CreateRouteTarget(route.Path),
            Disabled = route.Disabled,
            Children = route.Children is { Length: > 0 }
                ? BuildNavigation(route.Children)
                : null
        };

    private static RouteLocationRaw? CreateRouteTarget(string? path)
        => path is null
            ? (RouteLocationRaw?)null
            : (RouteLocationRaw)path;

    private static AdminRouteDefinition? FindByPath(
        AdminRouteDefinition[] routes,
        string path)
    {
        foreach (var route in routes)
        {
            if (route.Path == path)
            {
                return route;
            }

            if (route.Children is { Length: > 0 })
            {
                var child = FindByPath(route.Children, path);
                if (child is not null)
                {
                    return child;
                }
            }
        }

        return null;
    }

    private static AdminRouteDefinition? FindByKey(
        AdminRouteDefinition[] routes,
        string key)
    {
        foreach (var route in routes)
        {
            if (route.Key == key)
            {
                return route;
            }

            if (route.Children is { Length: > 0 })
            {
                var child = FindByKey(route.Children, key);
                if (child is not null)
                {
                    return child;
                }
            }
        }

        return null;
    }

    private static bool CollectRoutePath(
        AdminRouteDefinition[] routes,
        string selectedKey,
        List<AdminRouteDefinition> routePath)
    {
        foreach (var route in routes)
        {
            routePath.Add(route);
            if (route.Key == selectedKey)
            {
                return true;
            }

            if (route.Children is { Length: > 0 }
                && CollectRoutePath(route.Children, selectedKey, routePath))
            {
                return true;
            }

            routePath.RemoveAt(routePath.Count - 1);
        }

        return false;
    }

    private static void AddRouteRecords(
        AdminRouteDefinition[] routes,
        IVueComponent shellComponent,
        List<RouteRecordRaw> records)
    {
        foreach (var route in routes)
        {
            if (route.Path is not null)
            {
                records.Add(new RouteRecordSingleView
                {
                    Path = route.Path,
                    Name = route.Key,
                    Component = RawRouteComponent.From(shellComponent)
                });
            }

            if (route.Children is { Length: > 0 })
            {
                AddRouteRecords(route.Children, shellComponent, records);
            }
        }
    }
}
