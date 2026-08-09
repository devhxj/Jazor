using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/tdesign/sidebar")]
public partial class TDesignSidebarMenu : AdminComponentBase
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

    private TMenuValue? MenuValue
        => SelectedKey is null ? default(TMenuValue?) : (TMenuValue)SelectedKey;

    private TMenuValue[]? ExpandedMenuValues
        => ExpandedKeys is null ? null : Array.ConvertAll(ExpandedKeys, static key => (TMenuValue)key);

    private TMenuThemeValue MenuTheme
        => Theme == AdminThemeMode.Dark ? TMenuThemeValue.Dark : TMenuThemeValue.Light;

    private THeadMenuThemeValue HeadMenuTheme
        => Theme == AdminThemeMode.Dark ? THeadMenuThemeValue.Dark : THeadMenuThemeValue.Light;

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
            var href = TDesignRouteMapper.MapHref(item.Href, item.RouteTarget);
            var route = TDesignRouteMapper.MapMenuRoute(item.RouteTarget);

            builder.OpenComponent<TMenuItem>(10);
            builder.AddAttribute(11, nameof(TMenuItem.Value), (TMenuValue)item.Key);
            builder.AddAttribute(12, nameof(TMenuItem.IconContent), icon);
            builder.AddAttribute(13, nameof(TMenuItem.ChildContent), (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, item.Title);
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
