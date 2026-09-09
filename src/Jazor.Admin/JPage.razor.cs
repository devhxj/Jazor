namespace Jazor.Admin;

[ECMAScriptModule("./components/admin/page")]
public partial class JPage : JContentComponentBase, IVueContainerComponent
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

    // Keep filtering and region decisions in C#: the Razor template only projects
    // this stable state, preserving one evaluation per render for slot content.
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
