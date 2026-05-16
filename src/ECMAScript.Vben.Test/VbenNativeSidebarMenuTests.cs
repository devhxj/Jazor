using System.Reflection;
using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

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
        return (bool)typeof(VbenSidebarMenu)
            .GetMethod("IsExpanded", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { item })!;
    }

    private static bool InvokeIsSelected(string? selectedKey, VbenNavItem item)
    {
        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.SelectedKey), selectedKey);

        return (bool)typeof(VbenSidebarMenu)
            .GetMethod("IsSelected", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { item })!;
    }

    private static bool InvokeHasSelectedDescendant(string? selectedKey, VbenNavItem item)
    {
        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.SelectedKey), selectedKey);

        return (bool)typeof(VbenSidebarMenu)
            .GetMethod("HasSelectedDescendant", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { item })!;
    }

    private static bool InvokeCanNavigate(VbenNavItem item)
    {
        return (bool)typeof(VbenSidebarMenu)
            .GetMethod("CanNavigate", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { item })!;
    }

    private static bool InvokeHasNavigableChildren(VbenNavItem item)
    {
        return (bool)typeof(VbenSidebarMenu)
            .GetMethod("HasNavigableChildren", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { item })!;
    }

    private static HashSet<string> InvokeGetEffectiveExpandedKeySet(VbenSidebarMenu component)
    {
        return (HashSet<string>)typeof(VbenSidebarMenu)
            .GetMethod("GetEffectiveExpandedKeySet", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, Array.Empty<object>())!;
    }

    private static void SetParameter<TValue>(VbenSidebarMenu component, string parameterName, TValue value)
    {
        typeof(VbenSidebarMenu)
            .GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(component, value);
    }

    #pragma warning disable BL0006
    private static ArrayRange<RenderTreeFrame> RenderSingleItem(VbenNavItem item)
    {
        var component = new VbenSidebarMenu();
        var renderItem = (RenderFragment)typeof(VbenSidebarMenu)
            .GetMethod("RenderItem", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { item })!;

        var builder = new RenderTreeBuilder();
        renderItem(builder);

        return builder.GetFrames();
    }

    private static bool ContainsElement(ArrayRange<RenderTreeFrame> frames, string elementName)
    {
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType == RenderTreeFrameType.Element
                && string.Equals(frame.ElementName, elementName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAttribute(
        ArrayRange<RenderTreeFrame> frames,
        string attributeName,
        string? expectedValue = null)
    {
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType != RenderTreeFrameType.Attribute
                || !string.Equals(frame.AttributeName, attributeName, StringComparison.Ordinal))
            {
                continue;
            }

            if (expectedValue is null)
            {
                return true;
            }

            if (string.Equals(frame.AttributeValue?.ToString(), expectedValue, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
    #pragma warning restore BL0006
}
