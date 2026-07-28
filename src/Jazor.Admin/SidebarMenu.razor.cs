namespace Jazor.Admin;

[ECMAScriptModule("./components/jazor-admin-sidebar-menu")]
public partial class SidebarMenu : AdminComponentBase, IVueContainerComponent
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
    public AdminNavItems? Items { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    private AdminNavItemRenderHelper.EffectiveNavItem[] EffectiveItems
        => AdminNavItemRenderHelper.BuildEffectiveItems(Items?.AsArray);

    private VueClassValue RootCssClass
        => Collapsed
            ? BuildCssClass("jazor-admin-sidebar", "jazor-admin-sidebar--collapsed")
            : BuildCssClass("jazor-admin-sidebar");

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var logo = Logo;
        var items = EffectiveItems;
        if (logo is null && items.Length == 0)
        {
            return;
        }

        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        if (logo is not null)
        {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "jazor-admin-sidebar__logo");
            builder.AddContent(6, logo);
            builder.CloseElement();
        }

        if (items.Length > 0)
        {
            builder.OpenElement(7, "ul");
            builder.AddAttribute(8, "class", "jazor-admin-sidebar__list");
            foreach (var item in items)
            {
                builder.AddContent(9, RenderEffectiveItem(item));
            }
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private RenderFragment RenderItem(AdminNavItem item) => builder =>
    {
        if (!TryResolveEffectiveItem(item, out var effectiveItem))
        {
            return;
        }

        RenderEffectiveItem(effectiveItem)(builder);
    };

    private RenderFragment RenderEffectiveItem(AdminNavItemRenderHelper.EffectiveNavItem item) => builder =>
    {
        var isExpanded = IsExpandedCore(item);
        var isSelected = IsSelectedCore(item);
        var hasSelectedDescendant = HasSelectedDescendantCore(item);
        var isDisabled = item.Source.Disabled ?? false;
        var canNavigate = CanNavigateCore(item);
        var hasNavigableChildren = HasNavigableChildrenCore(item);
        var hasChildren = item.Children.Length > 0;

        builder.OpenElement(0, "li");
        builder.AddAttribute(1, "class", BuildItemCssClass(item, isExpanded, isSelected, hasSelectedDescendant));
        builder.AddAttribute(2, "data-key", item.Key);

        builder.OpenElement(3, "div");
        builder.AddAttribute(4, "class", "jazor-admin-sidebar__item-content");

        if (hasChildren && !canNavigate)
        {
            builder.OpenElement(5, "button");
            builder.AddAttribute(6, "type", "button");
            builder.AddAttribute(7, "class", "jazor-admin-sidebar__button jazor-admin-sidebar__button--branch");
            builder.AddAttribute(8, "disabled", isDisabled || !hasNavigableChildren);
            builder.AddAttribute(9, "aria-expanded", isExpanded);
            builder.AddAttribute(10, "onclick", EventCallback.Factory.Create(this, () => OnBranchToggledCore(item)));
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
            builder.AddAttribute(22, "class", "jazor-admin-sidebar__toggle");
            builder.AddAttribute(23, "disabled", isDisabled);
            builder.AddAttribute(24, "aria-expanded", isExpanded);
            builder.AddAttribute(25, "onclick", EventCallback.Factory.Create(this, () => OnBranchToggledCore(item)));
            builder.AddContent(26, isExpanded ? "-" : "+");
            builder.CloseElement();
        }

        builder.CloseElement();

        if (hasChildren && isExpanded)
        {
            builder.OpenElement(30, "ul");
            builder.AddAttribute(31, "class", "jazor-admin-sidebar__children");
            foreach (var child in item.Children)
            {
                RenderEffectiveItem(child)(builder);
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

        var selectedKey = AdminNavigationKeyHelper.Normalize(SelectedKey);
        if (selectedKey is null)
        {
            return new HashSet<string>();
        }

        var expandedKeys = new HashSet<string>();
        if (EffectiveItems is not { Length: > 0 } items)
        {
            return expandedKeys;
        }

        foreach (var item in items)
        {
            CollectExpandedKeysForSelectionCore(item, selectedKey, expandedKeys);
        }

        return expandedKeys;
    }

    private bool IsExpanded(AdminNavItem item)
    {
        return TryResolveEffectiveItem(item, out var effectiveItem)
               && IsExpandedCore(effectiveItem);
    }

    private bool IsSelected(AdminNavItem item)
    {
        return TryResolveEffectiveItem(item, out var effectiveItem)
               && IsSelectedCore(effectiveItem);
    }

    private bool HasSelectedDescendant(AdminNavItem item)
    {
        return TryResolveEffectiveItem(item, out var effectiveItem)
               && HasSelectedDescendantCore(effectiveItem);
    }

    private static bool CanNavigate(AdminNavItem item)
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

    private static bool HasNavigableChildren(AdminNavItem item)
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

    private bool ContainsSelectedItem(AdminNavItem item)
    {
        return TryResolveEffectiveItem(item, out var effectiveItem)
               && ContainsSelectedItemCore(effectiveItem);
    }

    private bool CollectExpandedKeysForSelection(
        AdminNavItem item,
        string selectedKey,
        HashSet<string> expandedKeys)
    {
        return TryResolveEffectiveItem(item, out var effectiveItem)
               && CollectExpandedKeysForSelectionCore(effectiveItem, selectedKey, expandedKeys);
    }

    private RenderFragment RenderNavigationElement(AdminNavItemRenderHelper.EffectiveNavItem item, bool isDisabled) => builder =>
    {
        var navigationTarget = ResolveNavigationTarget(item);

        if (!isDisabled && navigationTarget.HasRoute)
        {
            builder.OpenComponent<VueRouterLink>(0);
            builder.AddAttribute(1, nameof(VueRouterLink.CssClass), (VueClassValue)"jazor-admin-sidebar__link");
            builder.AddAttribute(2, nameof(VueRouterLink.To), navigationTarget.Route);
            builder.AddAttribute(3, nameof(VueRouterLink.OnClick), EventCallback.Factory.Create<MouseEvent>(this, _ => OnItemSelectedCore(item)));
            builder.AddAttribute(4, nameof(VueRouterLink.ChildContent), (RenderFragment)(childBuilder => childBuilder.AddContent(0, item.Title)));
            builder.CloseComponent();
            return;
        }

        if (!isDisabled && navigationTarget.HasHref)
        {
            builder.OpenElement(10, "a");
            builder.AddAttribute(11, "class", "jazor-admin-sidebar__link");
            builder.AddAttribute(12, "href", navigationTarget.Href);
            builder.AddAttribute(13, "onclick", EventCallback.Factory.Create(this, () => OnItemSelectedCore(item)));
            builder.AddContent(14, item.Title);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(20, "button");
        builder.AddAttribute(21, "type", "button");
        builder.AddAttribute(22, "class", "jazor-admin-sidebar__button");
        builder.AddAttribute(23, "disabled", isDisabled);
        builder.AddAttribute(24, "onclick", EventCallback.Factory.Create(this, () => OnItemSelectedCore(item)));
        if (navigationTarget.IsNavigable)
        {
            builder.AddAttribute(25, "aria-disabled", true);
        }

        builder.AddContent(26, item.Title);
        builder.CloseElement();
    };

    private async Task OnItemSelected(AdminNavItem item)
    {
        if (!TryResolveEffectiveItem(item, out var effectiveItem))
        {
            return;
        }

        await OnItemSelectedCore(effectiveItem);
    }

    private async Task OnBranchToggled(AdminNavItem item)
    {
        if (!TryResolveEffectiveItem(item, out var effectiveItem))
        {
            return;
        }

        await OnBranchToggledCore(effectiveItem);
    }

    private async Task OnItemSelectedCore(AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        if (!CanNavigateCore(item))
        {
            return;
        }

        await SelectedKeyChanged.InvokeAsync(item.Key);
    }

    private async Task OnBranchToggledCore(AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        if ((item.Source.Disabled ?? false) || !HasNavigableChildrenCore(item))
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
        AdminNavItemRenderHelper.EffectiveNavItem item,
        bool isExpanded,
        bool isSelected,
        bool hasSelectedDescendant)
    {
        var classes = new List<string>(6)
        {
            "jazor-admin-sidebar__item"
        };

        if (item.Children.Length > 0)
        {
            classes.Add("has-children");
        }

        if (item.Source.Disabled ?? false)
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
        var normalized = new HashSet<string>();
        foreach (var key in keys)
        {
            var normalizedKey = AdminNavigationKeyHelper.Normalize(key);
            if (normalizedKey is not null)
            {
                normalized.Add(normalizedKey);
            }
        }

        return normalized;
    }

    private HashSet<string> NormalizeExpandedKeys(IEnumerable<string> keys)
    {
        var normalized = NormalizeKeys(keys);
        if (normalized.Count == 0 || EffectiveItems is not { Length: > 0 } items)
        {
            return normalized.Count == 0
                ? normalized
                : new HashSet<string>();
        }

        var expandableKeys = new HashSet<string>();
        foreach (var item in items)
        {
            CollectExpandableKeysCore(item, expandableKeys);
        }

        if (expandableKeys.Count == 0)
        {
            return new HashSet<string>();
        }

        normalized.IntersectWith(expandableKeys);
        return normalized;
    }

    private static bool CollectExpandableKeys(
        AdminNavItem item,
        HashSet<string> expandableKeys)
    {
        var itemKey = AdminNavigationKeyHelper.Normalize(item.Key);
        if (item.Disabled ?? false || itemKey is null)
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
            expandableKeys.Add(itemKey!);
        }

        return hasNavigableDescendant;
    }

    private bool IsExpandedCore(AdminNavItemRenderHelper.EffectiveNavItem item)
        => !(item.Source.Disabled ?? false)
           && HasNavigableChildrenCore(item)
           && GetEffectiveExpandedKeySet().Contains(item.Key);

    private bool IsSelectedCore(AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        var selectedKey = AdminNavigationKeyHelper.Normalize(SelectedKey);
        return selectedKey is not null
               && !(item.Source.Disabled ?? false)
               && item.Key == selectedKey;
    }

    private bool HasSelectedDescendantCore(AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        if ((item.Source.Disabled ?? false) || AdminNavigationKeyHelper.Normalize(SelectedKey) is null)
        {
            return false;
        }

        foreach (var child in item.Children)
        {
            if (ContainsSelectedItemCore(child))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanNavigateCore(AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        if (item.Source.Disabled ?? false)
        {
            return false;
        }

        if (ResolveNavigationTarget(item).IsNavigable)
        {
            return true;
        }

        return item.Children.Length == 0;
    }

    private static bool HasNavigableChildrenCore(AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        if (item.Source.Disabled ?? false)
        {
            return false;
        }

        foreach (var child in item.Children)
        {
            if (CanNavigateCore(child) || HasNavigableChildrenCore(child))
            {
                return true;
            }
        }

        return false;
    }

    private bool ContainsSelectedItemCore(AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        if (item.Source.Disabled ?? false)
        {
            return false;
        }

        if (IsSelectedCore(item))
        {
            return true;
        }

        foreach (var child in item.Children)
        {
            if (ContainsSelectedItemCore(child))
            {
                return true;
            }
        }

        return false;
    }

    private bool CollectExpandedKeysForSelectionCore(
        AdminNavItemRenderHelper.EffectiveNavItem item,
        string selectedKey,
        HashSet<string> expandedKeys)
    {
        if (item.Source.Disabled ?? false)
        {
            return false;
        }

        var subtreeContainsSelection = item.Key == selectedKey;
        foreach (var child in item.Children)
        {
            if (CollectExpandedKeysForSelectionCore(child, selectedKey, expandedKeys))
            {
                subtreeContainsSelection = true;
            }
        }

        if (subtreeContainsSelection && HasNavigableChildrenCore(item))
        {
            expandedKeys.Add(item.Key);
        }

        return subtreeContainsSelection;
    }

    private static bool CollectExpandableKeysCore(
        AdminNavItemRenderHelper.EffectiveNavItem item,
        HashSet<string> expandableKeys)
    {
        if (item.Source.Disabled ?? false)
        {
            return false;
        }

        var hasNavigableDescendant = false;
        foreach (var child in item.Children)
        {
            if (CanNavigateCore(child) || CollectExpandableKeysCore(child, expandableKeys))
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

        var orderedKeyList = new List<string>(keys.Count);
        foreach (var key in keys)
        {
            orderedKeyList.Add(key);
        }

        var orderedKeys = orderedKeyList.ToArray();
        Array.Sort(orderedKeys);
        return orderedKeys;
    }

    private static AdminNavItem[] GetChildren(AdminNavItem item)
        => AdminNavItemRenderHelper.FilterRenderableItems(item.Children?.AsArray);

    private static AdminNavigationTargetResolver.ResolvedNavigationTarget ResolveNavigationTarget(AdminNavItem item)
        => AdminNavigationTargetResolver.Resolve(item.Href, item.RouteTarget);

    private static AdminNavigationTargetResolver.ResolvedNavigationTarget ResolveNavigationTarget(AdminNavItemRenderHelper.EffectiveNavItem item)
        => AdminNavigationTargetResolver.Resolve(item.Source.Href, item.Source.RouteTarget);

    private bool TryResolveEffectiveItem(AdminNavItem item, out AdminNavItemRenderHelper.EffectiveNavItem effectiveItem)
    {
        if (TryFindEffectiveItem(EffectiveItems, item, out effectiveItem))
        {
            return true;
        }

        if (Items?.AsArray is not { Length: > 0 })
        {
            var standaloneItems = AdminNavItemRenderHelper.BuildEffectiveItems([item]);
            if (standaloneItems.Length == 1)
            {
                effectiveItem = standaloneItems[0];
                return true;
            }
        }

        effectiveItem = null!;
        return false;
    }

    private static bool TryFindEffectiveItem(
        AdminNavItemRenderHelper.EffectiveNavItem[] items,
        AdminNavItem source,
        out AdminNavItemRenderHelper.EffectiveNavItem effectiveItem)
    {
        foreach (var item in items)
        {
            if (ReferenceEquals(item.Source, source))
            {
                effectiveItem = item;
                return true;
            }

            if (TryFindEffectiveItem(item.Children, source, out effectiveItem))
            {
                return true;
            }
        }

        effectiveItem = null!;
        return false;
    }
}
