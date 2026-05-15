using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.Vben;
using ECMAScript.VueContract;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Vben.ElementPlusInject.Library.Components;

[ECMAScriptModule("./components/element-admin-layout")]
public partial class ElementAdminLayout : VbenContentComponentBase, IVueContainerImplementation<VbenAdminLayout>
{
    [Parameter]
    public VbenLayoutMode Mode { get; set; }

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedKeyChanged { get; set; }

    [Parameter]
    public string[]? ExpandedKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

    [Parameter]
    public VbenNavItems? NavItems { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? Sidebar { get; set; }

    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<ElContainer>(0);
        builder.AddComponentParameter(1, nameof(ElContainer.Direction), "vertical");
        builder.AddComponentParameter(2, nameof(ElContainer.CssStyle), CssStyle);
        builder.AddComponentParameter(3, nameof(ElContainer.AdditionalAttributes), AdditionalAttributes);
        builder.AddComponentParameter(4, nameof(ElContainer.ChildContent), (RenderFragment)(shellBuilder =>
        {
            shellBuilder.OpenComponent<ElHeader>(5);
            shellBuilder.AddComponentParameter(6, nameof(ElHeader.Height), "72px");
            shellBuilder.AddComponentParameter(7, nameof(ElHeader.CssClass), "vben-ep-shell__header");
            shellBuilder.AddComponentParameter(8, nameof(ElHeader.ChildContent), (RenderFragment)(headerBuilder =>
            {
                if (Header is not null)
                {
                    headerBuilder.AddContent(9, Header);
                }
                else
                {
                    headerBuilder.OpenComponent<VbenHeaderBar>(10);
                    headerBuilder.AddComponentParameter(11, nameof(VbenHeaderBar.Title), Title);
                    headerBuilder.AddComponentParameter(12, nameof(VbenHeaderBar.Subtitle), Subtitle);
                    headerBuilder.AddComponentParameter(13, nameof(VbenHeaderBar.Logo), Logo);
                    headerBuilder.AddComponentParameter(14, nameof(VbenHeaderBar.Actions), HeaderActions);
                    headerBuilder.AddComponentParameter(15, nameof(VbenHeaderBar.UserRegion), UserRegion);
                    headerBuilder.CloseComponent();
                }
            }));
            shellBuilder.CloseComponent();

            shellBuilder.OpenComponent<ElContainer>(20);
            shellBuilder.AddComponentParameter(21, nameof(ElContainer.CssClass), "vben-ep-shell__body");
            shellBuilder.AddComponentParameter(22, nameof(ElContainer.ChildContent), (RenderFragment)(bodyBuilder =>
            {
                bodyBuilder.OpenComponent<ElAside>(23);
                bodyBuilder.AddComponentParameter(24, nameof(ElAside.Width), "280px");
                bodyBuilder.AddComponentParameter(25, nameof(ElAside.CssClass), "vben-ep-shell__aside");
                bodyBuilder.AddComponentParameter(26, nameof(ElAside.ChildContent), (RenderFragment)(asideBuilder =>
                {
                    if (Sidebar is not null)
                    {
                        asideBuilder.AddContent(27, Sidebar);
                    }
                    else
                    {
                        asideBuilder.OpenComponent<VbenSidebarMenu>(28);
                        asideBuilder.AddComponentParameter(29, nameof(VbenSidebarMenu.Items), NavItems);
                        asideBuilder.AddComponentParameter(30, nameof(VbenSidebarMenu.Collapsed), Collapsed);
                        asideBuilder.AddComponentParameter(31, nameof(VbenSidebarMenu.SelectedKey), SelectedKey);
                        asideBuilder.AddComponentParameter(32, nameof(VbenSidebarMenu.ExpandedKeys), ExpandedKeys);
                        asideBuilder.AddComponentParameter(33, nameof(VbenSidebarMenu.SelectedKeyChanged), SelectedKeyChanged);
                        asideBuilder.AddComponentParameter(34, nameof(VbenSidebarMenu.ExpandedKeysChanged), ExpandedKeysChanged);
                        asideBuilder.AddComponentParameter(35, nameof(VbenSidebarMenu.Logo), Logo);
                        asideBuilder.CloseComponent();
                    }
                }));
                bodyBuilder.CloseComponent();

                bodyBuilder.OpenComponent<ElMain>(40);
                bodyBuilder.AddComponentParameter(41, nameof(ElMain.CssClass), "vben-ep-shell__main");
                bodyBuilder.AddComponentParameter(42, nameof(ElMain.ChildContent), ChildContent);
                bodyBuilder.CloseComponent();
            }));
            shellBuilder.CloseComponent();
        }));

        builder.CloseComponent();
    }
}
