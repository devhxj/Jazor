using Microsoft.AspNetCore.Components;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

/// <summary>
/// Maintains the visited-route tab list used by the TDesign Starter layout.
/// 路由变化只追加一个页签；关闭、拖拽和右键操作均作用于同一份有序状态。
/// </summary>
[ECMAScriptModule("./components/route-tabs")]
public partial class RouteTabs : AdminComponentBase
{
    [Parameter]
    public AdminRouteDefinition[] Routes { get; set; } = [];

    [Parameter]
    public string CurrentPath { get; set; } = "/";

    [Parameter]
    public string HomePath { get; set; } = "/";

    private readonly Router router = UseRouter();
    private RouteTab[] tabs = [];

    private TTabValue ActiveValue => (TTabValue)CurrentPath;

    protected override void OnParametersSet()
    {
        var home = FindRoute(HomePath);
        if (home is not null && FindTab(HomePath) < 0)
            Append(CreateTab(home, true));

        var current = FindRoute(CurrentPath);
        if (current is null)
            return;

        var index = FindTab(CurrentPath);
        var updated = CreateTab(current, CurrentPath == HomePath);
        if (index < 0)
        {
            Append(updated);
            return;
        }

        Replace(index, updated);
    }

    private void Navigate(TTabValue value)
    {
        if (value.Value is string path && path != CurrentPath)
            _ = router.Push((RouteLocationRaw)path);
    }

    private void Remove(TTabsRemoveEventOptions options)
    {
        if (options.Value.Value is not string path)
            return;

        var index = FindTab(path);
        if (index <= 0)
            return;

        var next = index + 1 < tabs.Length ? tabs[index + 1] : tabs[index - 1];
        RemoveAt(index);
        if (path == CurrentPath)
            _ = router.Push((RouteLocationRaw)next.Path);
    }

    private void Reorder(TTabsDragSortContext context)
    {
        var currentIndex = (int)context.CurrentIndex;
        var targetIndex = (int)context.TargetIndex;
        if (currentIndex <= 0 || targetIndex <= 0 || currentIndex >= tabs.Length || targetIndex >= tabs.Length)
            return;

        var updated = CopyTabs();
        var current = updated[currentIndex];
        updated[currentIndex] = updated[targetIndex];
        updated[targetIndex] = current;
        tabs = updated;
    }

    private TDropdownOption[] MenuOptions(RouteTab tab, int index)
    {
        var options = new List<TDropdownOption>
        {
            Command("Refresh", "refresh", () => Refresh(tab.Path))
        };

        if (index > 1)
            options.Add(Command("Close left", "close-left", () => CloseLeft(index)));

        if (index < tabs.Length - 1)
            options.Add(Command("Close right", "close-right", () => CloseRight(index)));

        if (tabs.Length > 2)
            options.Add(Command("Close others", "close-others", () => CloseOthers(index)));

        return options.ToArray();
    }

    private static TDropdownOption Command(string text, string value, Action action)
        => new()
        {
            Content = (TdDropdownItemPropsContent)text,
            Value = (TdDropdownItemPropsValue)value,
            OnClick = (_, context) => action()
        };

    private void Refresh(string path)
    {
        // Router replacement follows Starter's refresh contract and re-runs route lifecycle.
        _ = router.Replace((RouteLocationRaw)path);
    }

    private void CloseLeft(int index)
    {
        var currentWasRemoved = FindTab(CurrentPath) > 0 && FindTab(CurrentPath) < index;
        var anchor = tabs[index];
        var updated = new RouteTab[tabs.Length - index + 1];
        updated[0] = tabs[0];
        for (int source = index, target = 1; source < tabs.Length; source++, target++)
            updated[target] = tabs[source];
        tabs = updated;

        if (currentWasRemoved)
            _ = router.Push((RouteLocationRaw)anchor.Path);
    }

    private void CloseRight(int index)
    {
        var currentWasRemoved = FindTab(CurrentPath) > index;
        var anchor = tabs[index];
        var updated = new RouteTab[index + 1];
        for (var source = 0; source <= index; source++)
            updated[source] = tabs[source];
        tabs = updated;

        if (currentWasRemoved)
            _ = router.Push((RouteLocationRaw)anchor.Path);
    }

    private void CloseOthers(int index)
    {
        var anchor = tabs[index];
        var currentWasRemoved = CurrentPath != anchor.Path;
        if (anchor.IsHome)
        {
            var updated = new RouteTab[1];
            updated[0] = anchor;
            tabs = updated;
        }
        else
        {
            var updated = new RouteTab[2];
            updated[0] = tabs[0];
            updated[1] = anchor;
            tabs = updated;
        }

        if (currentWasRemoved)
            _ = router.Push((RouteLocationRaw)anchor.Path);
    }

    private void Append(RouteTab tab)
    {
        var updated = new RouteTab[tabs.Length + 1];
        for (var index = 0; index < tabs.Length; index++)
            updated[index] = tabs[index];
        updated[tabs.Length] = tab;
        tabs = updated;
    }

    private void Replace(int index, RouteTab tab)
    {
        var updated = CopyTabs();
        updated[index] = tab;
        tabs = updated;
    }

    private void RemoveAt(int index)
    {
        var updated = new RouteTab[tabs.Length - 1];
        for (int source = 0, target = 0; source < tabs.Length; source++)
        {
            if (source == index)
                continue;
            updated[target++] = tabs[source];
        }
        tabs = updated;
    }

    private RouteTab[] CopyTabs()
    {
        var updated = new RouteTab[tabs.Length];
        for (var index = 0; index < tabs.Length; index++)
            updated[index] = tabs[index];
        return updated;
    }

    private int FindTab(string path)
    {
        for (var index = 0; index < tabs.Length; index++)
        {
            if (tabs[index].Path == path)
                return index;
        }

        return -1;
    }

    private AdminRouteDefinition? FindRoute(string path)
    {
        foreach (var route in Routes)
        {
            var found = FindRoute(route, path);
            if (found is not null)
                return found;
        }

        return null;
    }

    private static AdminRouteDefinition? FindRoute(AdminRouteDefinition route, string path)
    {
        if (route.Path == path)
            return route;

        if (route.Children is not { Length: > 0 } children)
            return null;

        foreach (var child in children)
        {
            var found = FindRoute(child, path);
            if (found is not null)
                return found;
        }

        return null;
    }

    private static RouteTab CreateTab(AdminRouteDefinition route, bool isHome)
        => new(route.Path ?? "/", route.Title ?? string.Empty, isHome);

    private sealed record RouteTab(string Path, string Title, bool IsHome);
}
