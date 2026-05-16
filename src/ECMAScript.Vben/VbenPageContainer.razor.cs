namespace ECMAScript.Vben;

[ECMAScriptModule("./components/vben-page-container")]
public partial class VbenPageContainer : VbenContentComponentBase, IVueContainerComponent
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

    private VueClassValue RootCssClass
        => BuildCssClass("vben-page");

    private PageHeaderRenderState BuildHeaderRenderState()
    {
        var breadcrumbItems = FilterRenderableItems(BreadcrumbItems);
        var actions = FilterRenderableItems(Actions);
        var hasTitles =
            breadcrumbItems.Length > 0
            || !string.IsNullOrWhiteSpace(Title)
            || !string.IsNullOrWhiteSpace(Subtitle);
        var hasActions = actions.Length > 0 || Extra is not null;

        return new(
            breadcrumbItems,
            actions,
            hasTitles,
            hasActions);
    }

    private RenderFragment RenderBreadcrumbItem(VbenBreadcrumbItem item) => builder =>
    {
        var isDisabled = item.Disabled ?? false;
        var href = VbenNavigationTargetResolver.TryResolveHref(item.Target);
        var cssClass = BuildBreadcrumbItemCssClass(item, isDisabled, href is not null);

        if (!isDisabled && href is not null)
        {
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "class", cssClass);
            builder.AddAttribute(2, "href", href);
            builder.AddContent(3, item.Title);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(10, "span");
        builder.AddAttribute(11, "class", cssClass);
        if (isDisabled)
        {
            builder.AddAttribute(12, "aria-disabled", true);
        }

        builder.AddContent(13, item.Title);
        builder.CloseElement();
    };

    private RenderFragment RenderAction(VbenPageAction action) => builder =>
    {
        var isDisabled = action.Disabled ?? false;
        var href = VbenNavigationTargetResolver.TryResolveHref(action.Target);
        var cssClass = BuildActionCssClass(action);

        if (!isDisabled && href is not null)
        {
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "class", cssClass);
            builder.AddAttribute(2, "href", href);
            builder.AddContent(3, action.Text);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(10, "button");
        builder.AddAttribute(11, "type", "button");
        builder.AddAttribute(12, "class", cssClass);
        builder.AddAttribute(13, "disabled", isDisabled);
        if (isDisabled && href is not null)
        {
            builder.AddAttribute(14, "aria-disabled", true);
        }

        builder.AddContent(15, action.Text);
        builder.CloseElement();
    };

    private static string BuildBreadcrumbItemCssClass(
        VbenBreadcrumbItem item,
        bool isDisabled,
        bool hasHref)
    {
        var classes = new List<string>(4)
        {
            "vben-page__breadcrumb-item"
        };

        if (hasHref)
        {
            classes.Add("is-link");
        }

        if (isDisabled)
        {
            classes.Add("is-disabled");
        }

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            classes.Add("is-empty");
        }

        return string.Join(" ", classes);
    }

    private static string BuildActionCssClass(VbenPageAction action)
    {
        var classes = new List<string>(4)
        {
            "vben-page__action",
            $"vben-page__action--{MapActionKindSuffix(action.Kind)}"
        };

        if (action.Disabled ?? false)
        {
            classes.Add("is-disabled");
        }

        return string.Join(" ", classes);
    }

    private static string MapActionKindSuffix(VbenPageActionKind? kind) => kind switch
    {
        VbenPageActionKind.Primary => "primary",
        VbenPageActionKind.Secondary => "secondary",
        VbenPageActionKind.Link => "link",
        VbenPageActionKind.Danger => "danger",
        _ => "default"
    };

    private static TItem[] FilterRenderableItems<TItem>(TItem[]? items)
        where TItem : class
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<TItem>();
        }

        List<TItem>? filtered = null;
        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            filtered ??= new List<TItem>(items.Length);
            filtered.Add(item);
        }

        return filtered is null
            ? Array.Empty<TItem>()
            : filtered.ToArray();
    }

    private readonly record struct PageHeaderRenderState(
        VbenBreadcrumbItem[] BreadcrumbItems,
        VbenPageAction[] Actions,
        bool HasTitleRegion,
        bool HasActionsRegion)
    {
        public bool HasHeader
            => HasTitleRegion || HasActionsRegion;
    }
}
