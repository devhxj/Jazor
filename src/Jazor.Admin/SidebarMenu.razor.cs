namespace Jazor.Admin;

[ECMAScriptModule("./components/admin/sidebar")]
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

    /// <summary>
    /// Optional per-item icon renderer. 不提供时仅渲染带 <c>data-icon</c> 的占位 span，
    /// 库不绑定任何第三方图标实现；应用可通过该模板注入例如 TIcon。
    /// </summary>
    [Parameter]
    public RenderFragment<AdminNavItem>? IconTemplate { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    private AdminNavItemRenderHelper.EffectiveNavItem[] EffectiveItems
        => AdminNavItemRenderHelper.BuildEffectiveItems(Items?.AsArray);

    private VueClassValue RootCssClass
        => Collapsed
            ? BuildCssClass("ja-sidebar", "ja-sidebar--collapsed")
            : BuildCssClass("ja-sidebar");

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
            builder.AddAttribute(5, "class", "ja-sidebar__logo");
            builder.AddContent(6, logo);
            builder.CloseElement();
        }

        if (items.Length > 0)
        {
            builder.OpenElement(7, "ul");
            builder.AddAttribute(8, "class", "ja-sidebar__list");
            foreach (var item in items)
            {
                builder.AddContent(9, RenderEffectiveItem(item));
            }
            builder.CloseElement();
        }

        builder.CloseElement();
    }

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
        builder.AddAttribute(4, "class", "ja-sidebar__item-content");

        if (hasChildren && !canNavigate)
        {
            builder.OpenElement(5, "button");
            builder.AddAttribute(6, "type", "button");
            builder.AddAttribute(7, "class", "ja-sidebar__button ja-sidebar__button--branch");
            builder.AddAttribute(8, "disabled", isDisabled || !hasNavigableChildren);
            builder.AddAttribute(9, "aria-expanded", isExpanded);
            builder.AddAttribute(10, "onclick", EventCallback.Factory.Create(this, () => OnBranchToggledCore(item)));
            builder.AddContent(11, RenderItemContent(item));
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
            builder.AddAttribute(22, "class", "ja-sidebar__toggle");
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
            builder.AddAttribute(31, "class", "ja-sidebar__children");
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

    private RenderFragment RenderNavigationElement(AdminNavItemRenderHelper.EffectiveNavItem item, bool isDisabled) => builder =>
    {
        var navigationTarget = ResolveNavigationTarget(item);

        if (!isDisabled && navigationTarget.HasRoute)
        {
            builder.OpenComponent<VueRouterLink>(0);
            builder.AddAttribute(1, nameof(VueRouterLink.CssClass), (VueClassValue)"ja-sidebar__link");
            builder.AddAttribute(2, nameof(VueRouterLink.To), navigationTarget.Route);
            builder.AddAttribute(3, nameof(VueRouterLink.OnClick), EventCallback.Factory.Create<MouseEvent>(this, _ => OnItemSelectedCore(item)));
            builder.AddAttribute(4, nameof(VueRouterLink.ChildContent), RenderItemContent(item));
            builder.CloseComponent();
            return;
        }

        if (!isDisabled && navigationTarget.HasHref)
        {
            builder.OpenElement(10, "a");
            builder.AddAttribute(11, "class", "ja-sidebar__link");
            builder.AddAttribute(12, "href", navigationTarget.Href);
            builder.AddAttribute(13, "onclick", EventCallback.Factory.Create(this, () => OnItemSelectedCore(item)));
            builder.AddContent(14, RenderItemContent(item));
            builder.CloseElement();
            return;
        }

        builder.OpenElement(20, "button");
        builder.AddAttribute(21, "type", "button");
        builder.AddAttribute(22, "class", "ja-sidebar__button");
        builder.AddAttribute(23, "disabled", isDisabled);
        builder.AddAttribute(24, "onclick", EventCallback.Factory.Create(this, () => OnItemSelectedCore(item)));
        if (navigationTarget.IsNavigable)
        {
            builder.AddAttribute(25, "aria-disabled", true);
        }

        builder.AddContent(26, RenderItemContent(item));
        builder.CloseElement();
    };

    /// <summary>
    /// Icon plus title shared by every navigation element variant so link, anchor,
    /// button and branch button cannot drift in icon placement.
    /// </summary>
    private RenderFragment RenderItemContent(AdminNavItemRenderHelper.EffectiveNavItem item) => builder =>
    {
        RenderIcon(builder, item);
        builder.AddContent(1, item.Title);
    };

    private void RenderIcon(RenderTreeBuilder builder, AdminNavItemRenderHelper.EffectiveNavItem item)
    {
        var icon = AdminDisplayTextHelper.Normalize(item.Source.Icon);
        if (icon is null)
        {
            return;
        }

        if (IconTemplate is not null)
        {
            builder.AddContent(0, IconTemplate, item.Source);
            return;
        }

        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "ja-sidebar__icon");
        builder.AddAttribute(2, "data-icon", icon);
        builder.AddAttribute(3, "aria-hidden", true);
        builder.CloseElement();
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
            "ja-sidebar__item"
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

    private static AdminNavigationTargetResolver.ResolvedNavigationTarget ResolveNavigationTarget(AdminNavItemRenderHelper.EffectiveNavItem item)
        => AdminNavigationTargetResolver.Resolve(item.Source.Href, item.Source.RouteTarget);
}
