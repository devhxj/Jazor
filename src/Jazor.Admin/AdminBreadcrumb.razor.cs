namespace Jazor.Admin;

/// <summary>
/// Standalone breadcrumb renderer shared with <see cref="PageContainer" />.
/// 导航语义与 PageContainer 一致：RouteTarget 优先于 Href，两者皆无时渲染纯文本。
/// </summary>
[ECMAScriptModule("./components/admin/breadcrumb")]
public partial class AdminBreadcrumb : AdminComponentBase, IVueContainerComponent
{
    [Parameter]
    public AdminBreadcrumbItem[]? Items { get; set; }

    private AdminBreadcrumbItem[] EffectiveItems => FilterRenderableItems(Items);

    private VueClassValue RootCssClass => BuildCssClass("ja-breadcrumb");

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var items = EffectiveItems;
        if (items.Length == 0)
        {
            return;
        }

        // RazorVue 直线降低只接受 SG 形状的 foreach；当前页判断需在循环外预取末项。
        var lastItem = items[^1];

        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        foreach (var item in items)
        {
            builder.AddContent(4, RenderItem(item, ReferenceEquals(item, lastItem)));
        }

        builder.CloseElement();
    }

    private RenderFragment RenderItem(AdminBreadcrumbItem item, bool isCurrent) => builder =>
    {
        var title = NormalizeTitle(item);
        if (title is null)
        {
            return;
        }

        var isDisabled = item.Disabled ?? false;
        var navigationTarget = ResolveTarget(item);
        var cssClass = BuildItemCssClass(isDisabled, navigationTarget.IsNavigable);

        if (!isDisabled && navigationTarget.HasRoute)
        {
            builder.OpenComponent<VueRouterLink>(0);
            builder.AddAttribute(1, nameof(VueRouterLink.CssClass), (VueClassValue)cssClass);
            builder.AddAttribute(2, nameof(VueRouterLink.To), navigationTarget.Route);
            builder.AddAttribute(3, nameof(VueRouterLink.ChildContent), (RenderFragment)(childBuilder => childBuilder.AddContent(0, title)));
            builder.CloseComponent();
            return;
        }

        if (!isDisabled && navigationTarget.HasHref)
        {
            builder.OpenElement(10, "a");
            builder.AddAttribute(11, "class", cssClass);
            builder.AddAttribute(12, "href", navigationTarget.Href);
            builder.AddContent(13, title);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(20, "span");
        builder.AddAttribute(21, "class", cssClass);
        if (isDisabled)
        {
            builder.AddAttribute(22, "aria-disabled", true);
        }

        // 末项通常是当前页；仅在非链接形态标注，避免与链接语义冲突。
        if (isCurrent)
        {
            builder.AddAttribute(23, "aria-current", "page");
        }

        builder.AddContent(24, title);
        builder.CloseElement();
    };

    // Member-position wrapper for the display-text host call; see ApplicationFrame.LanguageTag
    // for why the helper must not be called directly from render-position code.
    private static string? NormalizeTitle(AdminBreadcrumbItem item)
        => AdminDisplayTextHelper.Normalize(item.Title);

    private static AdminNavigationTargetResolver.ResolvedNavigationTarget ResolveTarget(AdminBreadcrumbItem item)
        => AdminNavigationTargetResolver.Resolve(item.Href, item.RouteTarget);

    private static string BuildItemCssClass(bool isDisabled, bool isNavigable)
    {
        var classes = new List<string>(3)
        {
            "ja-breadcrumb__item"
        };

        if (isNavigable)
        {
            classes.Add("is-link");
        }

        if (isDisabled)
        {
            classes.Add("is-disabled");
        }

        return string.Join(" ", classes);
    }

    private static AdminBreadcrumbItem[] FilterRenderableItems(AdminBreadcrumbItem[]? items)
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<AdminBreadcrumbItem>();
        }

        List<AdminBreadcrumbItem>? renderable = null;
        foreach (var item in items)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Title))
            {
                continue;
            }

            renderable ??= new List<AdminBreadcrumbItem>(items.Length);
            renderable.Add(item);
        }

        return renderable is null
            ? Array.Empty<AdminBreadcrumbItem>()
            : renderable.ToArray();
    }
}
