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
                    // 面包屑渲染委托给 AdminBreadcrumb，保持库内单一实现；
                    // 这里只保留布局槽位与可渲染性判断。
                    builder.OpenComponent<AdminBreadcrumb>(8);
                    builder.AddComponentParameter(9, nameof(AdminBreadcrumb.Items), header.BreadcrumbItems);
                    builder.CloseComponent();
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
                    builder.OpenComponent<PageAction>(19);
                    builder.AddComponentParameter(20, nameof(PageAction.Action), action);
                    builder.CloseComponent();
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
