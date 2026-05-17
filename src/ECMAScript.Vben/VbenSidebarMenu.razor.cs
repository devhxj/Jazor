namespace ECMAScript.Vben;

[ECMAScriptModule("./components/vben-sidebar-menu")]
public partial class VbenSidebarMenu : VbenComponentBase, IVueContainerComponent
{
    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedKeyChanged { get; set; }

    [Parameter]
    public string[]? ExpandedKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

    [Parameter]
    public VbenNavItems? Items { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    private bool HasItems
        => VbenNavItemRenderHelper.HasRenderableItems(Items);

    private bool HasContent
        => Logo is not null || HasItems;

    private VueClassValue RootCssClass
        => Collapsed
            ? BuildCssClass("vben-sidebar", "vben-sidebar--collapsed")
            : BuildCssClass("vben-sidebar");

    private RenderFragment RenderItem(VbenNavItem item) => builder =>
    {
        var isExpanded = IsExpanded(item);
        var isSelected = IsSelected(item);
        var hasSelectedDescendant = HasSelectedDescendant(item);
        var isDisabled = item.Disabled ?? false;
        var canNavigate = CanNavigate(item);
        var hasNavigableChildren = HasNavigableChildren(item);
        var hasChildren = GetChildren(item).Length > 0;

        builder.OpenElement(0, "li");
        builder.AddAttribute(1, "class", BuildItemCssClass(item, isExpanded, isSelected, hasSelectedDescendant));
        builder.AddAttribute(2, "data-key", item.Key);

        builder.OpenElement(3, "div");
        builder.AddAttribute(4, "class", "vben-sidebar__item-content");

        if (hasChildren && !canNavigate)
        {
            builder.OpenElement(5, "button");
            builder.AddAttribute(6, "type", "button");
            builder.AddAttribute(7, "class", "vben-sidebar__button vben-sidebar__button--branch");
            builder.AddAttribute(8, "disabled", isDisabled || !hasNavigableChildren);
            builder.AddAttribute(9, "aria-expanded", isExpanded);
            builder.AddAttribute(10, "onclick", EventCallback.Factory.Create(this, () => OnBranchToggled(item)));
            builder.AddContent(11, item.Title);
            builder.CloseElement();
        }
        else
        {
            RenderNavigationElement(item, isDisabled)(builder);
        }

        if (hasChildren && canNavigate && hasNavigableChildren)
        {
            builder.OpenElement(20, "button");
            builder.AddAttribute(21, "type", "button");
            builder.AddAttribute(22, "class", "vben-sidebar__toggle");
            builder.AddAttribute(23, "disabled", isDisabled);
            builder.AddAttribute(24, "aria-expanded", isExpanded);
            builder.AddAttribute(25, "onclick", EventCallback.Factory.Create(this, () => OnBranchToggled(item)));
            builder.AddContent(26, isExpanded ? "-" : "+");
            builder.CloseElement();
        }

        builder.CloseElement();

        if (hasChildren && isExpanded)
        {
            builder.OpenElement(30, "ul");
            builder.AddAttribute(31, "class", "vben-sidebar__children");
            foreach (var child in GetChildren(item))
            {
                RenderItem(child)(builder);
            }
            builder.CloseElement();
        }

        builder.CloseElement();
    };

    private HashSet<string> GetEffectiveExpandedKeySet()
    {
        if (ExpandedKeys is not null)
        {
            return NormalizeExpandedKeys(ExpandedKeys);
        }

        if (string.IsNullOrWhiteSpace(SelectedKey))
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var expandedKeys = new HashSet<string>(StringComparer.Ordinal);
        if (Items?.AsArray is not { Length: > 0 } items)
        {
            return expandedKeys;
        }

        foreach (var item in VbenNavItemRenderHelper.FilterRenderableItems(items))
        {
            CollectExpandedKeysForSelection(item, SelectedKey, expandedKeys);
        }

        return expandedKeys;
    }

    private bool IsExpanded(VbenNavItem item)
        => !(item.Disabled ?? false)
           && HasNavigableChildren(item)
           && GetEffectiveExpandedKeySet().Contains(item.Key);

    private bool IsSelected(VbenNavItem item)
        => !(item.Disabled ?? false)
           && !string.IsNullOrWhiteSpace(SelectedKey)
           && StringComparer.Ordinal.Equals(item.Key, SelectedKey);

    private bool HasSelectedDescendant(VbenNavItem item)
    {
        if (item.Disabled ?? false || string.IsNullOrWhiteSpace(SelectedKey))
        {
            return false;
        }

        foreach (var child in GetChildren(item))
        {
            if (ContainsSelectedItem(child))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanNavigate(VbenNavItem item)
    {
        if (item.Disabled ?? false)
        {
            return false;
        }

        if (ResolveNavigationTarget(item).IsNavigable)
        {
            return true;
        }

        return GetChildren(item).Length == 0;
    }

    private static bool HasNavigableChildren(VbenNavItem item)
    {
        if (item.Disabled ?? false)
        {
            return false;
        }

        foreach (var child in GetChildren(item))
        {
            if (CanNavigate(child) || HasNavigableChildren(child))
            {
                return true;
            }
        }

        return false;
    }

    private bool ContainsSelectedItem(VbenNavItem item)
    {
        if (item.Disabled ?? false)
        {
            return false;
        }

        if (IsSelected(item))
        {
            return true;
        }

        foreach (var child in GetChildren(item))
        {
            if (ContainsSelectedItem(child))
            {
                return true;
            }
        }

        return false;
    }

    private bool CollectExpandedKeysForSelection(
        VbenNavItem item,
        string selectedKey,
        HashSet<string> expandedKeys)
    {
        if (item.Disabled ?? false)
        {
            return false;
        }

        var subtreeContainsSelection = StringComparer.Ordinal.Equals(item.Key, selectedKey);
        foreach (var child in GetChildren(item))
        {
            if (CollectExpandedKeysForSelection(child, selectedKey, expandedKeys))
            {
                subtreeContainsSelection = true;
            }
        }

        if (subtreeContainsSelection && HasNavigableChildren(item))
        {
            expandedKeys.Add(item.Key);
        }

        return subtreeContainsSelection;
    }

    private RenderFragment RenderNavigationElement(VbenNavItem item, bool isDisabled) => builder =>
    {
        var navigationTarget = ResolveNavigationTarget(item);

        if (!isDisabled && navigationTarget.HasRoute)
        {
            builder.OpenElement(0, "router-link");
            builder.AddAttribute(1, "class", "vben-sidebar__link");
            builder.AddAttribute(2, "to", navigationTarget.Route);
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, () => OnItemSelected(item)));
            builder.AddContent(4, item.Title);
            builder.CloseElement();
            return;
        }

        if (!isDisabled && navigationTarget.HasHref)
        {
            builder.OpenElement(10, "a");
            builder.AddAttribute(11, "class", "vben-sidebar__link");
            builder.AddAttribute(12, "href", navigationTarget.Href);
            builder.AddAttribute(13, "onclick", EventCallback.Factory.Create(this, () => OnItemSelected(item)));
            builder.AddContent(14, item.Title);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(20, "button");
        builder.AddAttribute(21, "type", "button");
        builder.AddAttribute(22, "class", "vben-sidebar__button");
        builder.AddAttribute(23, "disabled", isDisabled);
        builder.AddAttribute(24, "onclick", EventCallback.Factory.Create(this, () => OnItemSelected(item)));
        if (navigationTarget.IsNavigable)
        {
            builder.AddAttribute(25, "aria-disabled", true);
        }

        builder.AddContent(26, item.Title);
        builder.CloseElement();
    };

    private async Task OnItemSelected(VbenNavItem item)
    {
        if (!CanNavigate(item))
        {
            return;
        }

        await SelectedKeyChanged.InvokeAsync(item.Key);
    }

    private async Task OnBranchToggled(VbenNavItem item)
    {
        if (item.Disabled ?? false || !HasNavigableChildren(item))
        {
            return;
        }

        var expandedKeys = GetEffectiveExpandedKeySet();
        if (!expandedKeys.Add(item.Key))
        {
            expandedKeys.Remove(item.Key);
        }

        await ExpandedKeysChanged.InvokeAsync(ToOrderedArray(expandedKeys));
    }

    private static string BuildItemCssClass(
        VbenNavItem item,
        bool isExpanded,
        bool isSelected,
        bool hasSelectedDescendant)
    {
        var classes = new List<string>(6)
        {
            "vben-sidebar__item"
        };

        if (GetChildren(item).Length > 0)
        {
            classes.Add("has-children");
        }

        if (item.Disabled ?? false)
        {
            classes.Add("is-disabled");
        }

        if (isExpanded)
        {
            classes.Add("is-expanded");
        }

        if (isSelected)
        {
            classes.Add("is-active");
            classes.Add("is-selected");
        }
        else if (hasSelectedDescendant)
        {
            classes.Add("is-ancestor-selected");
        }

        return string.Join(" ", classes);
    }

    private static HashSet<string> NormalizeKeys(IEnumerable<string> keys)
    {
        var normalized = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                normalized.Add(key);
            }
        }

        return normalized;
    }

    private HashSet<string> NormalizeExpandedKeys(IEnumerable<string> keys)
    {
        var normalized = NormalizeKeys(keys);
        if (normalized.Count == 0 || Items?.AsArray is not { Length: > 0 } items)
        {
            return normalized.Count == 0
                ? normalized
                : new HashSet<string>(StringComparer.Ordinal);
        }

        var expandableKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in VbenNavItemRenderHelper.FilterRenderableItems(items))
        {
            CollectExpandableKeys(item, expandableKeys);
        }

        if (expandableKeys.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        normalized.IntersectWith(expandableKeys);
        return normalized;
    }

    private static bool CollectExpandableKeys(
        VbenNavItem item,
        HashSet<string> expandableKeys)
    {
        if (item.Disabled ?? false)
        {
            return false;
        }

        var hasNavigableDescendant = false;
        foreach (var child in GetChildren(item))
        {
            if (CanNavigate(child) || CollectExpandableKeys(child, expandableKeys))
            {
                hasNavigableDescendant = true;
            }
        }

        if (hasNavigableDescendant)
        {
            expandableKeys.Add(item.Key);
        }

        return hasNavigableDescendant;
    }

    private static string[] ToOrderedArray(HashSet<string> keys)
    {
        if (keys.Count == 0)
        {
            return Array.Empty<string>();
        }

        var orderedKeys = new string[keys.Count];
        keys.CopyTo(orderedKeys);
        Array.Sort(orderedKeys, StringComparer.Ordinal);
        return orderedKeys;
    }

    private static VbenNavItem[] GetChildren(VbenNavItem item)
        => VbenNavItemRenderHelper.FilterRenderableItems(item.Children);

    private static VbenResolvedNavigationTarget ResolveNavigationTarget(VbenNavItem item)
        => VbenNavigationTargetResolver.Resolve(item.Target);
}
