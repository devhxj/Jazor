using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.TDesign;

[ECMAScriptModule("./components/vben-tdesign-sidebar-menu")]
public partial class VbenTDesignSidebarMenu : VbenComponentBase
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
    public VbenNavItems? Items { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    private TDesignMenuValue? MenuValue
        => SelectedKey is null ? default(TDesignMenuValue?) : (TDesignMenuValue)SelectedKey;

    private TDesignMenuValue[]? ExpandedMenuValues
        => ExpandedKeys is null ? null : Array.ConvertAll(ExpandedKeys, static key => (TDesignMenuValue)key);

    private RenderFragment RenderItem(VbenNavItem item) => builder =>
    {
        if (item.Children?.AsArray is { Length: > 0 } children)
        {
            builder.OpenComponent<TSubmenu>(0);
            builder.AddAttribute(1, nameof(TSubmenu.Value), (TDesignMenuValue)item.Key);
            builder.AddAttribute(2, nameof(TSubmenu.Title), item.Title);
            builder.AddAttribute(3, nameof(TSubmenu.Disabled), item.Disabled ?? false);
            builder.AddAttribute(4, nameof(TSubmenu.ChildContent), (RenderFragment)(childBuilder =>
            {
                foreach (var child in children)
                {
                    RenderItem(child)(childBuilder);
                }
            }));
            builder.CloseComponent();
            return;
        }

        builder.OpenComponent<TMenuItem>(10);
        builder.AddAttribute(11, nameof(TMenuItem.Value), (TDesignMenuValue)item.Key);
        builder.AddAttribute(12, nameof(TMenuItem.Text), item.Title);
        builder.AddAttribute(13, nameof(TMenuItem.Disabled), item.Disabled ?? false);
        if (item.Target?.AsHref is string href)
        {
            builder.AddAttribute(14, nameof(TMenuItem.Href), href);
        }
        else if (item.Target?.AsRoute is VbenRouteLocation route)
        {
            builder.AddAttribute(15, nameof(TMenuItem.RouterLink), true);
            builder.AddAttribute(16, nameof(TMenuItem.To), new TDesignMenuRoute
            {
                Path = route.Path,
                Name = route.Name,
                Hash = route.Hash
            });
        }
        builder.CloseComponent();
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
        var expandedKeys = values
            .Select(static value => value.AsString)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();

        await ExpandedKeysChanged.InvokeAsync(expandedKeys);
    }
}
