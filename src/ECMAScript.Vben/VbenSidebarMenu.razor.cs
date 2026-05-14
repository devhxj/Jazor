namespace ECMAScript.Vben;

[ECMAScriptModule("./components/vben-sidebar-menu")]
public partial class VbenSidebarMenu : VbenComponentBase, IVueContainerComponent
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

    private VueClassValue RootCssClass
        => Collapsed
            ? BuildCssClass("vben-sidebar", "vben-sidebar--collapsed")
            : BuildCssClass("vben-sidebar");

    private RenderFragment RenderItem(VbenNavItem item) => builder =>
    {
        builder.OpenElement(0, "li");
        builder.AddAttribute(1, "class", item.Key == SelectedKey ? "vben-sidebar__item is-active" : "vben-sidebar__item");

        if (item.Target?.AsHref is string href)
        {
            builder.OpenElement(2, "a");
            builder.AddAttribute(3, "href", href);
            builder.AddContent(4, item.Title);
            builder.CloseElement();
        }
        else
        {
            builder.OpenElement(5, "button");
            builder.AddAttribute(6, "type", "button");
            builder.AddAttribute(7, "disabled", item.Disabled ?? false);
            builder.AddAttribute(8, "onclick", EventCallback.Factory.Create(this, () => OnItemSelected(item.Key)));
            builder.AddContent(9, item.Title);
            builder.CloseElement();
        }

        if (item.Children?.AsArray is { Length: > 0 } children)
        {
            builder.OpenElement(10, "ul");
            builder.AddAttribute(11, "class", "vben-sidebar__children");
            foreach (var child in children)
            {
                RenderItem(child)(builder);
            }
            builder.CloseElement();
        }

        builder.CloseElement();
    };

    private async Task OnItemSelected(string key)
    {
        await SelectedKeyChanged.InvokeAsync(key);
    }
}
