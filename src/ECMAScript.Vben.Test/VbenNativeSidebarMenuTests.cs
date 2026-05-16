using ECMAScript.Vben;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativeSidebarMenuTests
{
    [TestMethod]
    public void Vben_SidebarMenu_ExpandedState_UsesExplicitExpandedKeysAndSelectedAncestorFallback()
    {
        var reportsLeaf = new VbenNavItem
        {
            Key = "reports.daily",
            Title = "Daily reports"
        };

        var reportsParent = new VbenNavItem
        {
            Key = "reports",
            Title = "Reports",
            Children = [reportsLeaf]
        };

        var expandedBySelection = InvokeIsExpanded(
            selectedKey: "reports.daily",
            expandedKeys: null,
            item: reportsParent,
            allExpandedKeys: out var expandedKeysFromSelection);

        Assert.IsTrue(expandedBySelection);
        CollectionAssert.AreEquivalent(new[] { "reports" }, expandedKeysFromSelection.ToArray());

        var expandedByExplicitState = InvokeIsExpanded(
            selectedKey: null,
            expandedKeys: ["reports"],
            item: reportsParent,
            allExpandedKeys: out var expandedKeysFromExplicitState);

        Assert.IsTrue(expandedByExplicitState);
        CollectionAssert.AreEquivalent(new[] { "reports" }, expandedKeysFromExplicitState.ToArray());

        var explicitEmptyArrayOverridesSelectedFallback = InvokeIsExpanded(
            selectedKey: "reports.daily",
            expandedKeys: Array.Empty<string>(),
            item: reportsParent,
            allExpandedKeys: out var expandedKeysFromExplicitEmptyArray);

        Assert.IsFalse(explicitEmptyArrayOverridesSelectedFallback);
        Assert.AreEqual(0, expandedKeysFromExplicitEmptyArray.Count);

        var notExpanded = InvokeIsExpanded(
            selectedKey: null,
            expandedKeys: null,
            item: reportsParent,
            allExpandedKeys: out var expandedKeysWithoutState);

        Assert.IsFalse(notExpanded);
        Assert.AreEqual(0, expandedKeysWithoutState.Count);
    }

    [TestMethod]
    public void Vben_SidebarMenu_SelectionState_FollowsSelectedNodeAndAncestorChain()
    {
        var metricsLeaf = new VbenNavItem
        {
            Key = "analytics.metrics",
            Title = "Metrics"
        };

        var analyticsParent = new VbenNavItem
        {
            Key = "analytics",
            Title = "Analytics",
            Children = [metricsLeaf]
        };

        Assert.IsTrue(InvokeIsSelected("analytics.metrics", metricsLeaf));
        Assert.IsFalse(InvokeIsSelected("analytics", metricsLeaf));
        Assert.IsTrue(InvokeHasSelectedDescendant("analytics.metrics", analyticsParent));
        Assert.IsFalse(InvokeHasSelectedDescendant("other", analyticsParent));
    }

    [TestMethod]
    public void Vben_SidebarMenu_DisabledBranch_DoesNotReportSelectableChildren()
    {
        var child = new VbenNavItem
        {
            Key = "settings.audit",
            Title = "Audit"
        };

        var disabledParent = new VbenNavItem
        {
            Key = "settings",
            Title = "Settings",
            Disabled = true,
            Children = [child]
        };

        Assert.IsFalse(InvokeCanNavigate(disabledParent));
        Assert.IsFalse(InvokeHasNavigableChildren(disabledParent));
    }

    [TestMethod]
    public void Vben_SidebarMenu_DisabledHrefLeaf_RendersAsDisabledButtonInsteadOfNavigableLink()
    {
        var item = new VbenNavItem
        {
            Key = "help",
            Title = "Help",
            Disabled = true,
            Target = "/help"
        };

        var frames = RenderSingleItem(item);

        Assert.IsTrue(ContainsElement(frames, "button"));
        Assert.IsTrue(ContainsAttribute(frames, "disabled"));
        Assert.IsFalse(ContainsAttribute(frames, "href", "/help"));
    }

    private static bool InvokeIsExpanded(
        string? selectedKey,
        string[]? expandedKeys,
        VbenNavItem item,
        out IReadOnlyCollection<string> allExpandedKeys)
    {
        var component = new VbenSidebarMenu
        {
        };
        SetParameter(component, nameof(VbenSidebarMenu.SelectedKey), selectedKey);
        SetParameter(component, nameof(VbenSidebarMenu.ExpandedKeys), expandedKeys);
        SetParameter(component, nameof(VbenSidebarMenu.Items), new VbenNavItems([item]));

        allExpandedKeys = InvokeGetEffectiveExpandedKeySet(component).ToArray();
        return VbenNativeRenderTreeTestHelper.InvokeInstanceMethod<bool>(component, "IsExpanded", item);
    }

    private static bool InvokeIsSelected(string? selectedKey, VbenNavItem item)
    {
        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.SelectedKey), selectedKey);

        return VbenNativeRenderTreeTestHelper.InvokeInstanceMethod<bool>(component, "IsSelected", item);
    }

    private static bool InvokeHasSelectedDescendant(string? selectedKey, VbenNavItem item)
    {
        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.SelectedKey), selectedKey);

        return VbenNativeRenderTreeTestHelper.InvokeInstanceMethod<bool>(component, "HasSelectedDescendant", item);
    }

    private static bool InvokeCanNavigate(VbenNavItem item)
    {
        return VbenNativeRenderTreeTestHelper.InvokeStaticMethod<bool>(typeof(VbenSidebarMenu), "CanNavigate", item);
    }

    private static bool InvokeHasNavigableChildren(VbenNavItem item)
    {
        return VbenNativeRenderTreeTestHelper.InvokeStaticMethod<bool>(typeof(VbenSidebarMenu), "HasNavigableChildren", item);
    }

    private static HashSet<string> InvokeGetEffectiveExpandedKeySet(VbenSidebarMenu component)
    {
        return VbenNativeRenderTreeTestHelper.InvokeInstanceMethod<HashSet<string>>(component, "GetEffectiveExpandedKeySet");
    }

    private static void SetParameter<TValue>(VbenSidebarMenu component, string parameterName, TValue value)
    {
        VbenNativeRenderTreeTestHelper.SetParameter(component, parameterName, value);
    }

    private static NativeRenderTreeSnapshot RenderSingleItem(VbenNavItem item)
    {
        return VbenNativeRenderTreeTestHelper.RenderFragmentFromInstanceMethod(
            new VbenSidebarMenu(),
            "RenderItem",
            item);
    }

    private static bool ContainsElement(NativeRenderTreeSnapshot frames, string elementName)
        => frames.ContainsElement(elementName);

    private static bool ContainsAttribute(
        NativeRenderTreeSnapshot frames,
        string attributeName,
        string? expectedValue = null)
        => frames.ContainsAttribute(attributeName, expectedValue);
}
