using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.Vben;
using ECMAScript.VueContract;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Vben.ElementPlusInject.Library.Components;

[ECMAScriptModule("./components/element-sidebar-menu")]
public partial class ElementSidebarMenu : VbenComponentBase, IVueContainerImplementation<VbenSidebarMenu>
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", Collapsed ? "vben-ep-sidebar vben-ep-sidebar--collapsed" : "vben-ep-sidebar");
        builder.AddAttribute(2, "style", CssStyle);
#pragma warning disable CS8619, CS8620
        builder.AddMultipleAttributes(3, AdditionalAttributes);
#pragma warning restore CS8619, CS8620

        if (Logo is not null)
        {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "vben-ep-sidebar__logo");
            builder.AddContent(6, Logo);
            builder.CloseElement();
        }

        builder.OpenComponent<ElScrollbar>(10);
        builder.AddComponentParameter(11, nameof(ElScrollbar.CssClass), "vben-ep-sidebar__scroll");
        builder.AddComponentParameter(12, nameof(ElScrollbar.ChildContent), (RenderFragment)(scrollBuilder =>
        {
            scrollBuilder.OpenComponent<ElMenu>(13);
            scrollBuilder.AddComponentParameter(14, nameof(ElMenu.Mode), "vertical");
            scrollBuilder.AddComponentParameter(15, nameof(ElMenu.Collapse), Collapsed);
            scrollBuilder.AddComponentParameter(16, nameof(ElMenu.DefaultActive), SelectedKey);
            scrollBuilder.AddComponentParameter(17, nameof(ElMenu.DefaultOpeneds), ExpandedKeys);
            scrollBuilder.AddComponentParameter(18, nameof(ElMenu.CssClass), "vben-ep-sidebar__menu");
            scrollBuilder.AddComponentParameter(19, nameof(ElMenu.ChildContent), (RenderFragment)(menuBuilder =>
            {
                if (Items is not null && Items.Value.AsArray is not null && Items.Value.AsArray.Length > 0)
                {
                    foreach (var item in Items.Value.AsArray)
                    {
                        menuBuilder.OpenComponent<ElementSidebarMenuNode>(20);
                        menuBuilder.AddAttribute(21, nameof(ElementSidebarMenuNode.Item), item);
                        menuBuilder.AddAttribute(22, nameof(ElementSidebarMenuNode.SelectedKey), SelectedKey);
                        menuBuilder.AddAttribute(23, nameof(ElementSidebarMenuNode.SelectedKeyChanged), SelectedKeyChanged);
                        menuBuilder.CloseComponent();
                    }
                }
            }));
            scrollBuilder.CloseComponent();
        }));
        builder.CloseComponent();

        builder.CloseElement();
    }
}
