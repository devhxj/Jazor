using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/tdesign/sidebar")]
public partial class TDesignSidebarMenu : JComponentBase
{
    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public AdminThemeMode Theme { get; set; } = AdminThemeMode.Light;

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedKeyChanged { get; set; }

    [Parameter]
    public string[]? ExpandedKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

    [Parameter]
    public AdminNavItems? Items { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public bool Horizontal { get; set; }

    // Header renders these menu nodes in its existing THeadMenu. A second THeadMenu would
    // create a nested horizontal scroller, unlike the Starter's MenuContent structure.
    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public bool ExpandMutex { get; set; }

    // The mobile rail uses HeadMenu's normal mode so TDesign keeps every primary branch
    // visible and exposes its children in the second row. Popup mode folds branches into a
    // hidden "more" item when the viewport is narrow, which made core routes unreachable.
    // 移动端使用 HeadMenu 平铺模式，让所有一级分支可见并在第二行显示子项；popup 模式会在窄屏
    // 把分支折叠进隐藏的“更多”项，导致核心路由无法访问。

    private TMenuValue? MenuValue
        => SelectedKey is null ? default(TMenuValue?) : (TMenuValue)SelectedKey;

    private TMenuValue[]? ExpandedMenuValues
        => ExpandedKeys is null ? null : Array.ConvertAll(ExpandedKeys, static key => (TMenuValue)key);

    private TMenuThemeValue MenuTheme
        => Theme == AdminThemeMode.Dark ? TMenuThemeValue.Dark : TMenuThemeValue.Light;

    private THeadMenuThemeValue HeadMenuTheme
        => Theme == AdminThemeMode.Dark ? THeadMenuThemeValue.Dark : THeadMenuThemeValue.Light;

    private AdminNavItems? HorizontalItems
    {
        get
        {
            if (Items?.AsArray is not { Length: > 0 } items)
                return null;

            var flattened = new AdminNavItem[items.Length];
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                flattened[index] = item.Children?.AsArray is { Length: > 0 }
                    ? new AdminNavItem
                    {
                        Key = item.Key,
                        Title = item.Title,
                        Icon = item.Icon,
                        Href = item.Href,
                        RouteTarget = item.RouteTarget,
                        Disabled = item.Disabled,
                        Children = FlattenLeaves(item.Children)
                    }
                    : item;
            }

            return flattened;
        }
    }

    // TDesign 1.20's HeadMenu normal mode derives second-row labels from each child's
    // default-slot vnode. Nested TSubmenu nodes expose an object there, which renders as
    // "[object Object]". The mobile row is intentionally flat so every reachable route is
    // textual while desktop keeps the full nested tree.
    // TDesign 1.20 的 HeadMenu 平铺模式从子项 default slot vnode 读取第二行标签；嵌套
    // TSubmenu 会暴露对象并显示“[object Object]”。移动端因此只展平叶子路由，桌面端仍保留完整层级。
    private static AdminNavItems FlattenLeaves(AdminNavItems? items)
    {
        if (items?.AsArray is not { Length: > 0 } source)
            return Array.Empty<AdminNavItem>();

        var leaves = new List<AdminNavItem>();
        foreach (var item in source)
        {
            if (item.Children?.AsArray is { Length: > 0 } children)
            {
                foreach (var leaf in FlattenLeaves(children).AsArray ?? Array.Empty<AdminNavItem>())
                    leaves.Add(leaf);
            }
            else
            {
                leaves.Add(item);
            }
        }

        return leaves.ToArray();
    }

    // routes.mjs 只导出成员函数；渲染 lambda 内直接限定 TDesignRouteMapper 会触发 phantom
    // 类名导入（浏览器模块链接失败），因此经成员位置间接映射，与 RouteBreadcrumb 保持同一形态。
    private static string? MapItemHref(AdminNavItem item)
        => TDesignRouteMapper.MapHref(item.Href, item.RouteTarget);

    private static TMenuItemToValue? MapItemMenuRoute(AdminNavItem item)
        => TDesignRouteMapper.MapMenuRoute(item.RouteTarget);

    private RenderFragment RenderItem(AdminNavItem item) => builder =>
    {
        RenderFragment? icon = string.IsNullOrWhiteSpace(item.Icon)
            ? null
            : iconBuilder =>
            {
                iconBuilder.OpenComponent<TIcon>(0);
                iconBuilder.AddComponentParameter(1, nameof(TIcon.Name), item.Icon);
                iconBuilder.AddComponentParameter(2, nameof(TIcon.Size), "18px");
                iconBuilder.AddComponentParameter(3, "aria-hidden", "true");
                iconBuilder.CloseComponent();
            };

        if (item.Children?.AsArray is { Length: > 0 } children)
        {
            builder.OpenComponent<TSubmenu>(0);
            builder.AddAttribute(1, nameof(TSubmenu.Value), (TMenuValue)item.Key);
            builder.AddAttribute(2, nameof(TSubmenu.IconContent), icon);
            builder.AddAttribute(3, nameof(TSubmenu.TitleContent), (RenderFragment)(titleBuilder =>
            {
                titleBuilder.OpenElement(0, "span");
                titleBuilder.AddAttribute(1, "data-nav-command", "toggle");
                titleBuilder.AddContent(2, item.Title);
                titleBuilder.CloseElement();
            }));
            builder.AddAttribute(4, nameof(TSubmenu.Disabled), item.Disabled ?? false);
            builder.AddAttribute(5, "data-nav-key", item.Key);
            builder.AddAttribute(6, "data-nav-kind", "branch");
            builder.AddAttribute(7, "data-nav-expanded", IsExpanded(item.Key));
            builder.AddAttribute(8, nameof(TSubmenu.ChildContent), (RenderFragment)(childBuilder =>
            {
                foreach (var child in children)
                {
                    RenderItem(child)(childBuilder);
                }
            }));
            builder.CloseComponent();
        }
        else
        {
            var href = MapItemHref(item);
            var route = MapItemMenuRoute(item);

            builder.OpenComponent<TMenuItem>(10);
            builder.AddAttribute(11, nameof(TMenuItem.Value), (TMenuValue)item.Key);
            builder.AddAttribute(12, nameof(TMenuItem.IconContent), icon);
            builder.AddAttribute(13, nameof(TMenuItem.ChildContent), (RenderFragment)(childBuilder =>
            {
                // HeadMenu reads the first default-slot vnode's children as the tab label;
                // use a text-bearing element so the upstream adapter receives a string.
                // HeadMenu 会读取 default slot 第一个 vnode 的 children 作为标签，因此用
                // 包含文本的元素承载标题，避免纯文本节点被当作缺少 label。
                childBuilder.OpenElement(0, "span");
                childBuilder.AddAttribute(1, "data-nav-label", item.Key);
                childBuilder.AddContent(2, item.Title);
                childBuilder.CloseElement();
            }));
            builder.AddAttribute(14, nameof(TMenuItem.Disabled), item.Disabled ?? false);
            builder.AddAttribute(15, "data-nav-key", item.Key);
            builder.AddAttribute(16, "data-nav-kind", "item");
            builder.AddAttribute(17, "data-nav-selected", item.Key == SelectedKey);
            if (href is not null)
            {
                builder.AddAttribute(18, nameof(TMenuItem.Href), href);
            }

            if (href is null && route.HasValue)
            {
                builder.AddAttribute(19, nameof(TMenuItem.RouterLink), true);
                builder.AddAttribute(20, nameof(TMenuItem.To), route.Value);
            }
            builder.CloseComponent();
        }
    };

    private async Task OnMenuChanged(TMenuValue value)
    {
        if (value.Value is string key)
        {
            await SelectedKeyChanged.InvokeAsync(key);
        }
    }

    private async Task OnMenuExpanded(TMenuValue[] values)
    {
        var expandedKeys = new List<string>();
        foreach (var value in values)
        {
            if (value.Value is string key && !string.IsNullOrWhiteSpace(key))
                expandedKeys.Add(key);
        }

        await ExpandedKeysChanged.InvokeAsync(expandedKeys.ToArray());
    }

    private bool IsExpanded(string key)
    {
        if (ExpandedKeys is null)
            return false;

        foreach (var expandedKey in ExpandedKeys)
        {
            if (expandedKey == key)
                return true;
        }

        return false;
    }
}
