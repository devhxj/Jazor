using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.Vben;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Vben.ElementPlusInject.Library;

[ECMAScriptModule("./components/vben-dashboard-app")]
public partial class VbenDashboardApp : ComponentBase, IVueComponent
{
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
    public VbenNavItems? NavItems { get; set; } = new VbenNavItem[]
    {
        new()
        {
            Key = "overview",
            Title = "Overview",
            Href = "/overview"
        },
        new()
        {
            Key = "release.pipeline",
            Title = "Pipelines",
            Href = "/release/pipelines"
        },
        new()
        {
            Key = "release.approvals",
            Title = "Approvals",
            Href = "/release/approvals"
        },
        new()
        {
            Key = "runtime.host",
            Title = "Host requirements",
            Href = "/runtime/host"
        },
        new()
        {
            Key = "runtime.consumer",
            Title = "Consumer bridge",
            Href = "/runtime/consumer"
        }
    };

    [Parameter]
    public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; } = CreateDefaultBreadcrumbItems();

    [Parameter]
    public VbenPageAction[]? PageActions { get; set; } = CreateDefaultPageActions();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<VbenAdminLayout>(0);
        builder.AddComponentParameter(1, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Mixed);
        builder.AddComponentParameter(2, nameof(VbenAdminLayout.NavItems), NavItems);
        builder.AddComponentParameter(3, nameof(VbenAdminLayout.Title), "ECMAScript.Vben");
        builder.AddComponentParameter(4, nameof(VbenAdminLayout.Subtitle), "Element Plus injected shell");
        builder.AddComponentParameter(5, nameof(VbenAdminLayout.Collapsed), Collapsed);
        builder.AddComponentParameter(6, nameof(VbenAdminLayout.CollapsedChanged), CollapsedChanged);
        builder.AddComponentParameter(7, nameof(VbenAdminLayout.SelectedKey), SelectedKey);
        builder.AddComponentParameter(8, nameof(VbenAdminLayout.SelectedKeyChanged), SelectedKeyChanged);
        builder.AddComponentParameter(9, nameof(VbenAdminLayout.ExpandedKeys), ExpandedKeys);
        builder.AddComponentParameter(10, nameof(VbenAdminLayout.ExpandedKeysChanged), ExpandedKeysChanged);
        builder.AddComponentParameter(11, nameof(VbenAdminLayout.HeaderActions), (RenderFragment)(slotBuilder =>
        {
            slotBuilder.OpenComponent<ElButton>(0);
            slotBuilder.AddComponentParameter(1, nameof(ElButton.Text), true);
            slotBuilder.AddComponentParameter(2, nameof(ElButton.Type), "primary");
            slotBuilder.AddComponentParameter(3, nameof(ElButton.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(4, "Publish")));
            slotBuilder.CloseComponent();

            slotBuilder.OpenComponent<ElButton>(5);
            slotBuilder.AddComponentParameter(6, nameof(ElButton.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(7, "Refresh")));
            slotBuilder.CloseComponent();
        }));
        builder.AddComponentParameter(12, nameof(VbenAdminLayout.UserRegion), (RenderFragment)(slotBuilder =>
        {
            slotBuilder.OpenComponent<ElTag>(0);
            slotBuilder.AddComponentParameter(1, nameof(ElTag.Type), "success");
            slotBuilder.AddComponentParameter(2, nameof(ElTag.Effect), "light");
            slotBuilder.AddComponentParameter(3, nameof(ElTag.Round), true);
            slotBuilder.AddComponentParameter(4, nameof(ElTag.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(5, "ops@prod")));
            slotBuilder.CloseComponent();
        }));
        builder.AddComponentParameter(13, nameof(VbenContentComponentBase.ChildContent), (RenderFragment)(slotBuilder =>
        {
            slotBuilder.OpenComponent<VbenPageContainer>(0);
            slotBuilder.AddComponentParameter(1, nameof(VbenPageContainer.Title), "Operations overview");
            slotBuilder.AddComponentParameter(2, nameof(VbenPageContainer.Subtitle), "Compile-time injected Vben shell composed with Element Plus");
            slotBuilder.AddComponentParameter(3, nameof(VbenPageContainer.BreadcrumbItems), BreadcrumbItems);
            slotBuilder.AddComponentParameter(4, nameof(VbenPageContainer.Actions), PageActions);
            slotBuilder.AddComponentParameter(5, nameof(VbenPageContainer.Extra), (RenderFragment)(extraBuilder =>
            {
                extraBuilder.OpenComponent<ElButtonGroup>(0);
                extraBuilder.AddComponentParameter(1, nameof(ElButtonGroup.ChildContent), (RenderFragment)(groupBuilder =>
                {
                    groupBuilder.OpenComponent<ElButton>(0);
                    groupBuilder.AddComponentParameter(1, nameof(ElButton.Type), "primary");
                    groupBuilder.AddComponentParameter(2, nameof(ElButton.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(3, "Create release")));
                    groupBuilder.CloseComponent();

                    groupBuilder.OpenComponent<ElButton>(4);
                    groupBuilder.AddComponentParameter(5, nameof(ElButton.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(6, "Export report")));
                    groupBuilder.CloseComponent();
                }));
                extraBuilder.CloseComponent();
            }));
            slotBuilder.AddComponentParameter(6, nameof(VbenContentComponentBase.ChildContent), (RenderFragment)(contentBuilder =>
            {
                contentBuilder.OpenElement(0, "section");
                contentBuilder.AddAttribute(1, "class", "vben-demo-metrics");

                contentBuilder.OpenComponent<ElCard>(2);
                contentBuilder.AddComponentParameter(3, nameof(ElCard.Header), "Build pipeline");
                contentBuilder.AddComponentParameter(4, nameof(ElCard.BodyClass), "vben-demo-card__body");
                contentBuilder.AddComponentParameter(5, nameof(ElCard.ChildContent), (RenderFragment)(cardBuilder =>
                {
                    cardBuilder.OpenElement(0, "div");
                    cardBuilder.AddAttribute(1, "class", "vben-demo-card__metric");

                    cardBuilder.OpenComponent<ElText>(2);
                    cardBuilder.AddComponentParameter(3, nameof(ElText.Tag), "strong");
                    cardBuilder.AddComponentParameter(4, nameof(ElText.ChildContent), (RenderFragment)(textBuilder => textBuilder.AddContent(5, "99.96%")));
                    cardBuilder.CloseComponent();

                    cardBuilder.OpenComponent<ElTag>(6);
                    cardBuilder.AddComponentParameter(7, nameof(ElTag.Type), "success");
                    cardBuilder.AddComponentParameter(8, nameof(ElTag.Effect), "light");
                    cardBuilder.AddComponentParameter(9, nameof(ElTag.ChildContent), (RenderFragment)(tagBuilder => tagBuilder.AddContent(10, "stable")));
                    cardBuilder.CloseComponent();

                    cardBuilder.CloseElement();

                    cardBuilder.OpenElement(11, "p");
                    cardBuilder.AddAttribute(12, "class", "vben-demo-card__copy");
                    cardBuilder.AddContent(13, "This sample is pending migration from the retired SFC bridge path to the current render-function artifact path.");
                    cardBuilder.CloseElement();
                }));
                contentBuilder.CloseComponent();

                contentBuilder.OpenComponent<ElCard>(30);
                contentBuilder.AddComponentParameter(31, nameof(ElCard.Header), "Container injection");
                contentBuilder.AddComponentParameter(32, nameof(ElCard.BodyClass), "vben-demo-card__body");
                contentBuilder.AddComponentParameter(33, nameof(ElCard.ChildContent), (RenderFragment)(cardBuilder =>
                {
                    cardBuilder.OpenElement(0, "div");
                    cardBuilder.AddAttribute(1, "class", "vben-demo-card__metric");

                    cardBuilder.OpenComponent<ElText>(2);
                    cardBuilder.AddComponentParameter(3, nameof(ElText.Tag), "strong");
                    cardBuilder.AddComponentParameter(4, nameof(ElText.ChildContent), (RenderFragment)(textBuilder => textBuilder.AddContent(5, "4/4")));
                    cardBuilder.CloseComponent();

                    cardBuilder.OpenComponent<ElTag>(6);
                    cardBuilder.AddComponentParameter(7, nameof(ElTag.Type), "primary");
                    cardBuilder.AddComponentParameter(8, nameof(ElTag.Effect), "light");
                    cardBuilder.AddComponentParameter(9, nameof(ElTag.ChildContent), (RenderFragment)(tagBuilder => tagBuilder.AddContent(10, "shell contracts")));
                    cardBuilder.CloseComponent();

                    cardBuilder.CloseElement();

                    cardBuilder.OpenElement(11, "p");
                    cardBuilder.AddAttribute(12, "class", "vben-demo-card__copy");
                    cardBuilder.AddContent(13, "The app authors only against Vben contracts. Concrete Element Plus layout components are resolved by assembly-level [VueInject].");
                    cardBuilder.CloseElement();
                }));
                contentBuilder.CloseComponent();

                contentBuilder.OpenComponent<ElCard>(60);
                contentBuilder.AddComponentParameter(61, nameof(ElCard.Header), "Consumer contract");
                contentBuilder.AddComponentParameter(62, nameof(ElCard.BodyClass), "vben-demo-card__body");
                contentBuilder.AddComponentParameter(63, nameof(ElCard.ChildContent), (RenderFragment)(cardBuilder =>
                {
                    cardBuilder.OpenElement(0, "div");
                    cardBuilder.AddAttribute(1, "class", "vben-demo-card__metric");

                    cardBuilder.OpenComponent<ElText>(2);
                    cardBuilder.AddComponentParameter(3, nameof(ElText.Tag), "strong");
                    cardBuilder.AddComponentParameter(4, nameof(ElText.ChildContent), (RenderFragment)(textBuilder => textBuilder.AddContent(5, "Deno only")));
                    cardBuilder.CloseComponent();

                    cardBuilder.OpenComponent<ElTag>(6);
                    cardBuilder.AddComponentParameter(7, nameof(ElTag.Type), "warning");
                    cardBuilder.AddComponentParameter(8, nameof(ElTag.Effect), "light");
                    cardBuilder.AddComponentParameter(9, nameof(ElTag.ChildContent), (RenderFragment)(tagBuilder => tagBuilder.AddContent(10, "migration pending")));
                    cardBuilder.CloseComponent();

                    cardBuilder.CloseElement();

                    cardBuilder.OpenElement(11, "p");
                    cardBuilder.AddAttribute(12, "class", "vben-demo-card__copy");
                    cardBuilder.AddContent(13, "Browser assets will be revalidated after this sample moves to render-function .mjs artifacts.");
                    cardBuilder.CloseElement();
                }));
                contentBuilder.CloseComponent();

                contentBuilder.CloseElement();
            }));
            slotBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }

    private static VbenBreadcrumbItem[] CreateDefaultBreadcrumbItems() =>
    new VbenBreadcrumbItem[]
    {
        new()
        {
            Key = "home",
            Title = "Admin"
        },
        new()
        {
            Key = "operations",
            Title = "Operations overview"
        }
    };

    private static VbenPageAction[] CreateDefaultPageActions() =>
    new VbenPageAction[]
    {
        new()
        {
            Key = "create",
            Text = "Create release",
            Kind = VbenPageActionKind.Primary
        },
        new()
        {
            Key = "sync",
            Text = "Sync status",
            Kind = VbenPageActionKind.Secondary
        }
    };
}
