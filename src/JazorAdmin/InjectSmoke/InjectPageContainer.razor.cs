using ECMAScript;
using Jazor.Admin;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using static ECMAScript.Vue3;

[assembly: VueInject(typeof(PageContainer), typeof(JazorAdmin.InjectSmoke.InjectPageContainer))]

namespace JazorAdmin.InjectSmoke;

[ECMAScriptModule("./components/jazor-admin-inject-page-container")]
public sealed class InjectPageContainer : ComponentBase, IVueComponent,
    IVueContainerImplementation<PageContainer>
{
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    public VueStyleValue? CssStyle { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    [ECMAScriptName("injected-content")]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    [ECMAScriptName("injectedTitle")]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public AdminBreadcrumbItem[]? BreadcrumbItems { get; set; }

    [Parameter]
    public AdminPageAction[]? Actions { get; set; }

    [Parameter]
    [ECMAScriptName("injected-extra")]
    public RenderFragment? Extra { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var title = Normalize(Title);
        var subtitle = Normalize(Subtitle);
        var breadcrumbs = FilterBreadcrumbs(BreadcrumbItems);
        var actions = FilterActions(Actions);

        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "class", BuildCssClass("jazor-admin-inject-page"));
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddAttribute(3, "data-vue-inject", "page-container");
        builder.AddMultipleAttributes(4, AdditionalAttributes);

        builder.OpenElement(5, "header");
        builder.AddAttribute(6, "class", "jazor-admin-inject-page__header");
        if (breadcrumbs.Length > 0)
        {
            builder.OpenElement(7, "nav");
            builder.AddAttribute(8, "aria-label", "Breadcrumb");
            builder.AddAttribute(9, "class", "jazor-admin-inject-page__breadcrumbs");
            foreach (var breadcrumb in breadcrumbs)
                builder.AddContent(10, RenderBreadcrumb(breadcrumb));
            builder.CloseElement();
        }

        if (title is not null)
        {
            builder.OpenElement(11, "h1");
            builder.AddAttribute(12, "class", "jazor-admin-inject-page__title");
            builder.AddContent(13, title);
            builder.CloseElement();
        }

        if (subtitle is not null)
        {
            builder.OpenElement(14, "p");
            builder.AddAttribute(15, "class", "jazor-admin-inject-page__subtitle");
            builder.AddContent(16, subtitle);
            builder.CloseElement();
        }

        if (actions.Length > 0 || Extra is not null)
        {
            builder.OpenElement(17, "div");
            builder.AddAttribute(18, "class", "jazor-admin-inject-page__actions");
            foreach (var action in actions)
                builder.AddContent(19, RenderAction(action));
            builder.AddContent(20, Extra);
            builder.CloseElement();
        }
        builder.CloseElement();

        builder.OpenElement(21, "div");
        builder.AddAttribute(22, "class", "jazor-admin-inject-page__body");
        builder.AddContent(23, ChildContent);
        builder.CloseElement();
        builder.CloseElement();
    }

    private static RenderFragment RenderBreadcrumb(AdminBreadcrumbItem item) => builder =>
    {
        var title = Normalize(item.Title);
        if (title is null)
            return;

        var disabled = item.Disabled ?? false;
        if (!disabled && item.RouteTarget.HasValue)
        {
            builder.OpenComponent<VueRouterLink>(0);
            builder.AddComponentParameter(1, nameof(VueRouterLink.To), item.RouteTarget.Value);
            builder.AddComponentParameter(2, nameof(VueRouterLink.ChildContent),
                (RenderFragment)(childBuilder => childBuilder.AddContent(0, title)));
            builder.CloseComponent();
            return;
        }

        if (!disabled && Normalize(item.Href) is { } href)
        {
            builder.OpenElement(3, "a");
            builder.AddAttribute(4, "href", href);
            builder.AddContent(5, title);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(6, "span");
        if (disabled)
            builder.AddAttribute(7, "aria-disabled", true);
        builder.AddContent(8, title);
        builder.CloseElement();
    };

    private static RenderFragment RenderAction(AdminPageAction action) => builder =>
    {
        var text = Normalize(action.Text);
        if (text is null)
            return;

        var disabled = action.Disabled ?? false;
        var cssClass = "jazor-admin-inject-page__action jazor-admin-inject-page__action--" +
                       MapActionKind(action.Kind);
        if (!disabled && action.RouteTarget.HasValue)
        {
            builder.OpenComponent<VueRouterLink>(0);
            builder.AddComponentParameter(1, nameof(VueRouterLink.CssClass), (VueClassValue)cssClass);
            builder.AddComponentParameter(2, nameof(VueRouterLink.To), action.RouteTarget.Value);
            builder.AddComponentParameter(3, nameof(VueRouterLink.ChildContent),
                (RenderFragment)(childBuilder => childBuilder.AddContent(0, text)));
            builder.CloseComponent();
            return;
        }

        if (!disabled && Normalize(action.Href) is { } href)
        {
            builder.OpenElement(4, "a");
            builder.AddAttribute(5, "class", cssClass);
            builder.AddAttribute(6, "href", href);
            builder.AddContent(7, text);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(8, "button");
        builder.AddAttribute(9, "type", "button");
        builder.AddAttribute(10, "class", cssClass);
        builder.AddAttribute(11, "data-inject-action", action.Key);
        builder.AddAttribute(12, "disabled", disabled);
        builder.AddAttribute(13, "onclick", action.Click);
        builder.AddContent(14, text);
        builder.CloseElement();
    };

    private static string MapActionKind(AdminPageActionKind? kind) => kind switch
    {
        AdminPageActionKind.Primary => "primary",
        AdminPageActionKind.Secondary => "secondary",
        AdminPageActionKind.Link => "link",
        AdminPageActionKind.Danger => "danger",
        _ => "default"
    };

    private static AdminBreadcrumbItem[] FilterBreadcrumbs(AdminBreadcrumbItem[]? items)
        => items is { Length: > 0 }
            ? items.Where(static item => item is not null && Normalize(item.Title) is not null).ToArray()
            : [];

    private static AdminPageAction[] FilterActions(AdminPageAction[]? items)
        => items is { Length: > 0 }
            ? items.Where(static item => item is not null && Normalize(item.Text) is not null).ToArray()
            : [];

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private VueClassValue BuildCssClass(string frameworkClass)
    {
        if (CssClass is null)
            return frameworkClass;

        var values = new List<VueValue> { frameworkClass };
        AppendCssClass(values, CssClass.Value);
        return values.ToArray();
    }

    private static void AppendCssClass(List<VueValue> values, VueClassValue cssClass)
    {
        if (cssClass.AsString is { } cssClassString)
        {
            values.Add(cssClassString);
            return;
        }

        if (cssClass.AsStrings is { } cssClassStrings)
        {
            foreach (var cssClassValue in cssClassStrings)
                values.Add(cssClassValue);
            return;
        }

        if (cssClass.AsProps is { } cssClassProps)
        {
            values.Add(cssClassProps);
            return;
        }

        if (cssClass.AsValues is { } cssClassValues)
        {
            foreach (var cssClassValue in cssClassValues)
                values.Add(cssClassValue);
        }
    }
}
