using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.Vben;
using ECMAScript.VueContract;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Vben.ElementPlusInject.Library.Components;

[ECMAScriptModule("./components/element-header-bar")]
public partial class ElementHeaderBar : VbenComponentBase, IVueContainerImplementation<VbenHeaderBar>
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "vben-ep-header");
        builder.AddAttribute(2, "style", CssStyle);
#pragma warning disable CS8619, CS8620
        builder.AddMultipleAttributes(3, AdditionalAttributes);
#pragma warning restore CS8619, CS8620

        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "vben-ep-header__identity");

        if (Logo is not null)
        {
            builder.OpenElement(6, "div");
            builder.AddAttribute(7, "class", "vben-ep-header__logo");
            builder.AddContent(8, Logo);
            builder.CloseElement();
        }

        builder.OpenElement(9, "div");
        builder.AddAttribute(10, "class", "vben-ep-header__titles");

        if (!string.IsNullOrWhiteSpace(Title))
        {
            builder.OpenComponent<ElText>(11);
            builder.AddComponentParameter(12, nameof(ElText.Tag), "strong");
            builder.AddComponentParameter(13, nameof(ElText.CssClass), "vben-ep-header__title");
            builder.AddComponentParameter(14, nameof(ElText.ChildContent), (RenderFragment)(textBuilder => textBuilder.AddContent(15, Title)));
            builder.CloseComponent();
        }

        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            builder.OpenComponent<ElText>(20);
            builder.AddComponentParameter(21, nameof(ElText.Tag), "span");
            builder.AddComponentParameter(22, nameof(ElText.Type), "info");
            builder.AddComponentParameter(23, nameof(ElText.CssClass), "vben-ep-header__subtitle");
            builder.AddComponentParameter(24, nameof(ElText.ChildContent), (RenderFragment)(textBuilder => textBuilder.AddContent(25, Subtitle)));
            builder.CloseComponent();
        }

        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(30, "div");
        builder.AddAttribute(31, "class", "vben-ep-header__actions");
        builder.AddContent(32, Actions);
        builder.AddContent(33, UserRegion);
        builder.CloseElement();

        builder.CloseElement();
    }
}
