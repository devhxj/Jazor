using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-tdesign-sidebar-menu")]
public partial class TDesignSidebarMenu : AdminComponentBase
{
    [Parameter]
    public bool Collapsed { get; set; }

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

    private TDesignMenuValue? MenuValue
        => SelectedKey is null ? default(TDesignMenuValue?) : (TDesignMenuValue)SelectedKey;

    private TDesignMenuValue[]? ExpandedMenuValues
        => ExpandedKeys is null ? null : Array.ConvertAll(ExpandedKeys, static key => (TDesignMenuValue)key);

    private RenderFragment RenderItem(AdminNavItem item) => builder =>
    {
        if (item.Children?.AsArray is { Length: > 0 } children)
        {
            builder.OpenComponent<TSubmenu>(0);
            builder.AddAttribute(1, nameof(TSubmenu.Value), (TDesignMenuValue)item.Key);
            builder.AddAttribute(2, nameof(TSubmenu.TitleContent), (RenderFragment)(titleBuilder =>
            {
                titleBuilder.OpenElement(0, "span");
                titleBuilder.AddAttribute(1, "data-nav-command", "toggle");
                titleBuilder.AddContent(2, item.Title);
                titleBuilder.CloseElement();
            }));
            builder.AddAttribute(3, nameof(TSubmenu.Disabled), item.Disabled ?? false);
            builder.AddAttribute(4, "data-nav-key", item.Key);
            builder.AddAttribute(5, "data-nav-kind", "branch");
            builder.AddAttribute(6, "data-nav-expanded", IsExpanded(item.Key));
            builder.AddAttribute(7, nameof(TSubmenu.ChildContent), (RenderFragment)(childBuilder =>
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
            var route = TDesignRouteMapper.MapRoute(item.RouteTarget);

            builder.OpenComponent<TMenuItem>(10);
            builder.AddAttribute(11, nameof(TMenuItem.Value), (TDesignMenuValue)item.Key);
            builder.AddAttribute(12, nameof(TMenuItem.ChildContent), (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, item.Title);
            }));
            builder.AddAttribute(13, nameof(TMenuItem.Disabled), item.Disabled ?? false);
            builder.AddAttribute(14, "data-nav-key", item.Key);
            builder.AddAttribute(15, "data-nav-kind", "item");
            builder.AddAttribute(16, "data-nav-selected", item.Key == SelectedKey);
            if (href is not null)
            {
                builder.AddAttribute(17, nameof(TMenuItem.Href), href);
            }

            if (href is null && route.HasValue)
            {
                builder.AddAttribute(18, nameof(TMenuItem.RouterLink), true);
                builder.AddAttribute(19, nameof(TMenuItem.To), route.Value);
            }
            builder.CloseComponent();
        }
    };

    private async Task OnMenuChanged(TDesignMenuValue value)
    {
        if (value.AsString is string key)
        {
            await SelectedKeyChanged.InvokeAsync(key);
        }
    }

    private async Task OnMenuExpanded(TDesignMenuValue[] values)
    {
        var expandedKeys = new List<string>();
        foreach (var value in values)
        {
            var key = value.AsString;
            if (!string.IsNullOrWhiteSpace(key))
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
