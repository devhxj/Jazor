namespace Jazor.Admin;

[ECMAScriptModule("./components/admin/page")]
public partial class PageContainer : AdminContentComponentBase, IVueContainerComponent
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public AdminBreadcrumbItem[]? BreadcrumbItems { get; set; }

    [Parameter]
    public AdminPageAction[]? Actions { get; set; }

    [Parameter]
    public RenderFragment? Extra { get; set; }

    private string? NormalizedTitle
        => AdminDisplayTextHelper.Normalize(Title);

    private string? NormalizedSubtitle
        => AdminDisplayTextHelper.Normalize(Subtitle);

    private VueClassValue RootCssClass
        => BuildCssClass("ja-page");

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var header = BuildHeaderRenderState();

        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        if (header.HasHeader)
        {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "ja-page__header");

            if (header.HasTitleRegion)
            {
                builder.OpenElement(6, "div");
                builder.AddAttribute(7, "class", "ja-page__titles");

                if (header.BreadcrumbItems.Length > 0)
                {
                    builder.OpenElement(8, "nav");
                    builder.AddAttribute(9, "class", "ja-page__breadcrumb");
                    foreach (var item in header.BreadcrumbItems)
                    {
                        builder.AddContent(10, RenderBreadcrumbItem(item));
                    }
                    builder.CloseElement();
                }

                if (NormalizedTitle is not null)
                {
                    builder.OpenElement(11, "h1");
                    builder.AddAttribute(12, "class", "ja-page__title");
                    builder.AddContent(13, NormalizedTitle);
                    builder.CloseElement();
                }

                if (NormalizedSubtitle is not null)
                {
                    builder.OpenElement(14, "p");
                    builder.AddAttribute(15, "class", "ja-page__subtitle");
                    builder.AddContent(16, NormalizedSubtitle);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }

            if (header.HasActionsRegion)
            {
                builder.OpenElement(17, "div");
                builder.AddAttribute(18, "class", "ja-page__actions");
                foreach (var action in header.Actions)
                {
                    builder.AddContent(19, RenderAction(action));
                }
                builder.AddContent(20, header.Extra);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.OpenElement(21, "div");
        builder.AddAttribute(22, "class", "ja-page__body");
        builder.AddContent(23, ChildContent);
        builder.CloseElement();

        builder.CloseElement();
    }

    private PageHeaderRenderState BuildHeaderRenderState()
    {
        var breadcrumbItems = FilterRenderableBreadcrumbItems(BreadcrumbItems);
        var actions = FilterRenderableActions(Actions);
        var extra = Extra;
        var hasTitles =
            breadcrumbItems.Length > 0
            || NormalizedTitle is not null
            || NormalizedSubtitle is not null;
        var hasActions = actions.Length > 0 || extra is not null;

        return new(
            breadcrumbItems,
            actions,
            extra,
            hasTitles,
            hasActions);
    }

    private RenderFragment RenderBreadcrumbItem(AdminBreadcrumbItem item) => builder =>
    {
        var title = AdminDisplayTextHelper.Normalize(item.Title);
        if (title is null)
        {
            return;
        }

        var isDisabled = item.Disabled ?? false;
        var navigationTarget = AdminNavigationTargetResolver.Resolve(item.Href, item.RouteTarget);
        var cssClass = BuildBreadcrumbItemCssClass(item, isDisabled, navigationTarget.IsNavigable);

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

        builder.AddContent(23, title);
        builder.CloseElement();
    };

    private RenderFragment RenderAction(AdminPageAction action) => builder =>
    {
        var text = AdminDisplayTextHelper.Normalize(action.Text);
        if (text is null)
        {
            return;
        }

        var isDisabled = action.Disabled ?? false;
        var navigationTarget = AdminNavigationTargetResolver.Resolve(action.Href, action.RouteTarget);
        var cssClass = BuildActionCssClass(action);

        if (!isDisabled && navigationTarget.HasRoute)
        {
            builder.OpenComponent<VueRouterLink>(0);
            builder.AddAttribute(1, nameof(VueRouterLink.CssClass), (VueClassValue)cssClass);
            builder.AddAttribute(2, nameof(VueRouterLink.To), navigationTarget.Route);
            builder.AddAttribute(3, "data-action-key", action.Key);
            builder.AddAttribute(4, nameof(VueRouterLink.ChildContent), (RenderFragment)(childBuilder => childBuilder.AddContent(0, text)));
            builder.CloseComponent();
            return;
        }

        if (!isDisabled && navigationTarget.HasHref)
        {
            builder.OpenElement(10, "a");
            builder.AddAttribute(11, "class", cssClass);
            builder.AddAttribute(12, "href", navigationTarget.Href);
            builder.AddAttribute(13, "data-action-key", action.Key);
            builder.AddContent(14, text);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(20, "button");
        builder.AddAttribute(21, "type", "button");
        builder.AddAttribute(22, "class", cssClass);
        builder.AddAttribute(23, "disabled", isDisabled);
        builder.AddAttribute(24, "data-action-key", action.Key);
        builder.AddAttribute(25, "onclick", action.Click);
        if (isDisabled && navigationTarget.IsNavigable)
        {
            builder.AddAttribute(26, "aria-disabled", true);
        }

        builder.AddContent(27, text);
        builder.CloseElement();
    };

    private static string BuildBreadcrumbItemCssClass(
        AdminBreadcrumbItem item,
        bool isDisabled,
        bool hasHref)
    {
        var classes = new List<string>(4)
        {
            "ja-page__breadcrumb-item"
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

    private static string BuildActionCssClass(AdminPageAction action)
    {
        var classes = new List<string>(4)
        {
            "ja-page__action",
            $"ja-page__action--{MapActionKindSuffix(action.Kind)}"
        };

        if (action.Disabled ?? false)
        {
            classes.Add("is-disabled");
        }

        return string.Join(" ", classes);
    }

    private static string MapActionKindSuffix(AdminPageActionKind? kind) => kind switch
    {
        AdminPageActionKind.Primary => "primary",
        AdminPageActionKind.Secondary => "secondary",
        AdminPageActionKind.Link => "link",
        AdminPageActionKind.Danger => "danger",
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

    private static AdminBreadcrumbItem[] FilterRenderableBreadcrumbItems(AdminBreadcrumbItem[]? items)
    {
        var filtered = FilterRenderableItems(items);
        if (filtered.Length == 0)
        {
            return filtered;
        }

        List<AdminBreadcrumbItem>? renderable = null;
        foreach (var item in filtered)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                continue;
            }

            renderable ??= new List<AdminBreadcrumbItem>(filtered.Length);
            renderable.Add(item);
        }

        return renderable is null
            ? Array.Empty<AdminBreadcrumbItem>()
            : renderable.Count == filtered.Length
                ? filtered
                : renderable.ToArray();
    }

    private static AdminPageAction[] FilterRenderableActions(AdminPageAction[]? items)
    {
        var filtered = FilterRenderableItems(items);
        if (filtered.Length == 0)
        {
            return filtered;
        }

        List<AdminPageAction>? renderable = null;
        foreach (var item in filtered)
        {
            if (string.IsNullOrWhiteSpace(item.Text))
            {
                continue;
            }

            renderable ??= new List<AdminPageAction>(filtered.Length);
            renderable.Add(item);
        }

        return renderable is null
            ? Array.Empty<AdminPageAction>()
            : renderable.Count == filtered.Length
                ? filtered
                : renderable.ToArray();
    }

    private sealed class PageHeaderRenderState
    {
        public PageHeaderRenderState(
            AdminBreadcrumbItem[] breadcrumbItems,
            AdminPageAction[] actions,
            RenderFragment? extra,
            bool hasTitleRegion,
            bool hasActionsRegion)
        {
            BreadcrumbItems = breadcrumbItems;
            Actions = actions;
            Extra = extra;
            HasTitleRegion = hasTitleRegion;
            HasActionsRegion = hasActionsRegion;
        }

        public AdminBreadcrumbItem[] BreadcrumbItems { get; }

        public AdminPageAction[] Actions { get; }

        public RenderFragment? Extra { get; }

        public bool HasTitleRegion { get; }

        public bool HasActionsRegion { get; }

        public bool HasHeader
            => HasTitleRegion || HasActionsRegion;
    }
}
