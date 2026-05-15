using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.Vben;
using ECMAScript.VueContract;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Vben.ElementPlusInject.Library.Components;

[ECMAScriptModule("./components/element-page-container")]
public partial class ElementPageContainer : VbenContentComponentBase, IVueContainerImplementation<VbenPageContainer>
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; }

    [Parameter]
    public VbenPageAction[]? Actions { get; set; }

    [Parameter]
    public RenderFragment? Extra { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "class", "vben-ep-page");
        builder.AddAttribute(2, "style", CssStyle);
#pragma warning disable CS8619, CS8620
        builder.AddMultipleAttributes(3, AdditionalAttributes);
#pragma warning restore CS8619, CS8620

        builder.OpenComponent<ElCard>(4);
        builder.AddComponentParameter(5, nameof(ElCard.BodyClass), "vben-ep-page-card__body");
        builder.AddComponentParameter(6, nameof(ElCard.HeaderSlot), (RenderFragment)(headerBuilder => BuildHeader(headerBuilder)));
        builder.AddComponentParameter(7, nameof(ElCard.ChildContent), (RenderFragment)(bodyBuilder => BuildBody(bodyBuilder)));
        builder.CloseComponent();
        builder.CloseElement();
    }

    private void BuildHeader(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "vben-ep-page__header");

        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "vben-ep-page__title-group");

        if (BreadcrumbItems is { Length: > 0 })
        {
            builder.OpenComponent<ElBreadcrumb>(4);
            builder.AddComponentParameter(5, nameof(ElBreadcrumb.ChildContent), (RenderFragment)(breadcrumbBuilder =>
            {
                foreach (var item in BreadcrumbItems)
                {
                    breadcrumbBuilder.OpenComponent<ElBreadcrumbItem>(0);
                    breadcrumbBuilder.AddAttribute(1, nameof(ElBreadcrumbItem.ChildContent), (RenderFragment)(itemBuilder => itemBuilder.AddContent(2, item.Title)));
                    breadcrumbBuilder.CloseComponent();
                }
            }));
            builder.CloseComponent();
        }

        if (!string.IsNullOrWhiteSpace(Title))
        {
            builder.OpenElement(10, "h1");
            builder.AddAttribute(11, "class", "vben-ep-page__title");
            builder.AddContent(12, Title);
            builder.CloseElement();
        }

        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            builder.OpenElement(13, "p");
            builder.AddAttribute(14, "class", "vben-ep-page__subtitle");
            builder.AddContent(15, Subtitle);
            builder.CloseElement();
        }

        builder.CloseElement();

        if ((Actions is { Length: > 0 }) || Extra is not null)
        {
            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "class", "vben-ep-page__toolbar");

            if (Actions is { Length: > 0 })
            {
                builder.OpenComponent<ElButtonGroup>(22);
                builder.AddComponentParameter(23, nameof(ElButtonGroup.ChildContent), (RenderFragment)(groupBuilder =>
                {
                    foreach (var action in Actions)
                    {
                        groupBuilder.OpenComponent<ElButton>(0);
                        groupBuilder.AddAttribute(1, nameof(ElButton.Type), ResolveButtonType(action.Kind));
                        groupBuilder.AddAttribute(2, nameof(ElButton.Disabled), action.Disabled ?? false);
                        groupBuilder.AddAttribute(3, nameof(ElButton.ChildContent), (RenderFragment)(buttonBuilder => buttonBuilder.AddContent(4, action.Text)));
                        groupBuilder.CloseComponent();
                    }
                }));
                builder.CloseComponent();
            }

            if (Extra is not null)
            {
                builder.OpenElement(30, "div");
                builder.AddAttribute(31, "class", "vben-ep-page__extra");
                builder.AddContent(32, Extra);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private void BuildBody(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "vben-ep-page__body");
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }

    private static string ResolveButtonType(VbenPageActionKind? kind) => kind switch
    {
        VbenPageActionKind.Primary => "primary",
        VbenPageActionKind.Danger => "danger",
        VbenPageActionKind.Link => "info",
        VbenPageActionKind.Secondary => "default",
        _ => "default"
    };
}
