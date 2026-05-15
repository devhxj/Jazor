using ECMAScript;
using ECMAScript.ElementPlus;
using static ECMAScript.Vue3;
using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Vben.ElementPlusInject.Library.Components;

[ECMAScriptModule("./components/element-sidebar-menu-node")]
public sealed class ElementSidebarMenuNode : ComponentBase, IVueComponent
{
    [Parameter]
    public VbenNavItem Item { get; set; } = default!;

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedKeyChanged { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Item.Children is not null && Item.Children.Value.AsArray is not null && Item.Children.Value.AsArray.Length > 0)
        {
            builder.OpenComponent<ElSubMenu>(0);
            builder.AddAttribute(1, nameof(ElSubMenu.Index), Item.Key);
            builder.AddAttribute(2, nameof(ElSubMenu.Title), (RenderFragment)(titleBuilder => titleBuilder.AddContent(3, Item.Title)));
            builder.AddAttribute(4, nameof(ElSubMenu.ChildContent), (RenderFragment)(childBuilder =>
            {
                foreach (var child in Item.Children.Value.AsArray)
                {
                    childBuilder.OpenComponent<ElementSidebarMenuNode>(0);
                    childBuilder.AddAttribute(1, nameof(Item), child);
                    childBuilder.AddAttribute(2, nameof(SelectedKey), SelectedKey);
                    childBuilder.AddAttribute(3, nameof(SelectedKeyChanged), SelectedKeyChanged);
                    childBuilder.CloseComponent();
                }
            }));
            builder.CloseComponent();
        }
        else
        {
            builder.OpenComponent<ElMenuItem>(10);
            builder.AddAttribute(11, nameof(ElMenuItem.Index), Item.Key);
            builder.AddAttribute(12, nameof(ElMenuItem.Disabled), Item.Disabled ?? false);
            builder.AddAttribute(13, nameof(ElMenuItem.OnClick), EventCallback.Factory.Create(this, OnItemClick));
            builder.AddAttribute(14, nameof(ElMenuItem.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(15, Item.Title)));
            builder.CloseComponent();
        }
    }

    private Task OnItemClick()
        => SelectedKeyChanged.InvokeAsync(Item.Key);
}
