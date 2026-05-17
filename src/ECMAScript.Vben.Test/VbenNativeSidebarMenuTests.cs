using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativeSidebarMenuTests
{
    [TestMethod]
    public void Vben_SidebarMenu_WithoutLogoOrItems_DoesNotRenderEmptyRootOrList()
    {
        var frames = RenderSidebarMenu(new VbenSidebarMenu());

        Assert.IsFalse(frames.ContainsElement("nav"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("ul", "vben-sidebar__list"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-sidebar__logo"));
    }

    [TestMethod]
    public void Vben_SidebarMenu_WithOnlyLogo_RendersLogoWithoutEmptyList()
    {
        var component = new VbenSidebarMenu();
        SetParameter(
            component,
            nameof(VbenSidebarMenu.Logo),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "sidebar-brand");
                builder.AddContent(2, "Jazor");
                builder.CloseElement();
            }));

        var frames = RenderSidebarMenu(component);

        Assert.IsTrue(frames.ContainsElement("nav"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-sidebar__logo"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("span", "sidebar-brand"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("ul", "vben-sidebar__list"));
    }

    [TestMethod]
    public void Vben_SidebarMenu_EmptyLogo_DoesNotRenderEmptyRootOrLogoWrapper()
    {
        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.Logo), EmptyFragment);

        var frames = RenderSidebarMenu(component);

        Assert.IsFalse(frames.ContainsElement("nav"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-sidebar__logo"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("ul", "vben-sidebar__list"));
    }

    [TestMethod]
    public void Vben_SidebarMenu_WhitespaceOnlyLogo_DoesNotRenderEmptyRootOrLogoWrapper()
    {
        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.Logo), WhitespaceFragment);

        var frames = RenderSidebarMenu(component);

        Assert.IsFalse(frames.ContainsElement("nav"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-sidebar__logo"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("ul", "vben-sidebar__list"));
    }

    [TestMethod]
    public void Vben_SidebarMenu_NullOnlyItems_DoNotRenderEmptyRootOrList()
    {
        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.Items), new VbenNavItems([null!]));

        var frames = RenderSidebarMenu(component);

        Assert.IsFalse(frames.ContainsElement("nav"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("ul", "vben-sidebar__list"));
        Assert.IsFalse(frames.ContainsAttribute("data-key"));
    }

    [TestMethod]
    public void Vben_SidebarMenu_WhitespaceOnlyTitleItems_DoNotRenderEmptyRootOrList()
    {
        var component = new VbenSidebarMenu();
        SetParameter(
            component,
            nameof(VbenSidebarMenu.Items),
            new VbenNavItems(
            [
                new VbenNavItem
                {
                    Key = "empty",
                    Title = "   ",
                    Target = "/ignored"
                }
            ]));

        var frames = RenderSidebarMenu(component);

        Assert.IsFalse(frames.ContainsElement("nav"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("ul", "vben-sidebar__list"));
        Assert.IsFalse(frames.ContainsAttribute("data-key", "empty"));
        Assert.IsFalse(frames.ContainsAttribute("href", "/ignored"));
    }

    [TestMethod]
    public void Vben_SidebarMenu_NullRootAndChildEntries_AreIgnoredDuringRender()
    {
        VbenNavItems items =
        [
            null!,
            new VbenNavItem
            {
                Key = "analytics",
                Title = "Analytics",
                Children =
                [
                    null!,
                    new VbenNavItem
                    {
                        Key = "analytics.metrics",
                        Title = "Metrics"
                    }
                ]
            }
        ];

        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.Items), items);
        SetParameter(component, nameof(VbenSidebarMenu.SelectedKey), "analytics.metrics");

        var frames = RenderSidebarMenu(component);

        Assert.IsTrue(frames.ContainsElement("nav"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("ul", "vben-sidebar__list"));
        Assert.IsTrue(frames.ContainsAttribute("data-key", "analytics"));
        Assert.IsTrue(frames.ContainsAttribute("data-key", "analytics.metrics"));
        Assert.IsTrue(frames.ContainsClassToken("is-expanded"));
    }

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
    public void Vben_SidebarMenu_DisabledBranch_WithExplicitExpandedKey_DoesNotRenderExpandedChildren()
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

        var component = new VbenSidebarMenu();
        SetParameter(component, nameof(VbenSidebarMenu.Items), new VbenNavItems([disabledParent]));
        SetParameter(component, nameof(VbenSidebarMenu.ExpandedKeys), new[] { "settings" });

        var frames = RenderSidebarMenu(component);

        Assert.IsTrue(frames.ContainsAttribute("data-key", "settings"));
        Assert.IsFalse(frames.ContainsClassToken("is-expanded"));
        Assert.IsFalse(frames.ContainsAttribute("data-key", "settings.audit"));
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

    [TestMethod]
    public void Vben_SidebarMenu_RouteLocationLeaf_RendersRouterLinkTarget()
    {
        var item = new VbenNavItem
        {
            Key = "reports",
            Title = "Reports",
            Target = new VbenRouteLocation
            {
                Name = "reports.daily",
                Hash = "summary"
            }
        };

        var frames = RenderSingleItem(item);

        Assert.IsTrue(ContainsElement(frames, "router-link"));
        Assert.IsTrue(ContainsAttribute(frames, "to"));
        Assert.IsFalse(ContainsAttribute(frames, "href"));
    }

    [TestMethod]
    public void Vben_SidebarMenu_OnItemSelected_NavigableLeaf_InvokesSelectedKeyChanged()
    {
        var component = new VbenSidebarMenu();
        var recorder = new EventRecorder<string>();
        SetParameter(
            component,
            nameof(VbenSidebarMenu.SelectedKeyChanged),
            EventCallback.Factory.Create<string>(recorder, recorder.Record));

        InvokeOnItemSelected(
            component,
            new VbenNavItem
            {
                Key = "reports",
                Title = "Reports",
                Target = "/reports"
            });

        CollectionAssert.AreEqual(new[] { "reports" }, recorder.Values.ToArray());
    }

    [TestMethod]
    public void Vben_SidebarMenu_OnItemSelected_DisabledLeaf_DoesNotInvokeSelectedKeyChanged()
    {
        var component = new VbenSidebarMenu();
        var recorder = new EventRecorder<string>();
        SetParameter(
            component,
            nameof(VbenSidebarMenu.SelectedKeyChanged),
            EventCallback.Factory.Create<string>(recorder, recorder.Record));

        InvokeOnItemSelected(
            component,
            new VbenNavItem
            {
                Key = "reports",
                Title = "Reports",
                Disabled = true,
                Target = "/reports"
            });

        Assert.AreEqual(0, recorder.Values.Count);
    }

    [TestMethod]
    public void Vben_SidebarMenu_OnBranchToggled_AppendsExpandedKeyInSortedOrder()
    {
        var alphaChild = new VbenNavItem
        {
            Key = "reports.daily",
            Title = "Daily"
        };

        var alphaParent = new VbenNavItem
        {
            Key = "reports.alpha",
            Title = "Reports",
            Children = [alphaChild]
        };

        var zetaChild = new VbenNavItem
        {
            Key = "reports.zeta.child",
            Title = "Archive"
        };

        var zetaParent = new VbenNavItem
        {
            Key = "reports.zeta",
            Title = "Reports archive",
            Children = [zetaChild]
        };

        var component = new VbenSidebarMenu();
        var recorder = new EventRecorder<string[]>();
        SetParameter(component, nameof(VbenSidebarMenu.Items), new VbenNavItems([alphaParent, zetaParent]));
        SetParameter(component, nameof(VbenSidebarMenu.ExpandedKeys), new[] { "reports.zeta" });
        SetParameter(
            component,
            nameof(VbenSidebarMenu.ExpandedKeysChanged),
            EventCallback.Factory.Create<string[]>(recorder, recorder.Record));

        InvokeOnBranchToggled(component, alphaParent);

        Assert.AreEqual(1, recorder.Values.Count);
        CollectionAssert.AreEqual(new[] { "reports.alpha", "reports.zeta" }, recorder.Values[0]);
    }

    [TestMethod]
    public void Vben_SidebarMenu_OnBranchToggled_DropsDisabledAndUnknownExpandedKeysFromChangedPayload()
    {
        var validChild = new VbenNavItem
        {
            Key = "reports.daily",
            Title = "Daily"
        };

        var validParent = new VbenNavItem
        {
            Key = "reports",
            Title = "Reports",
            Children = [validChild]
        };

        var disabledChild = new VbenNavItem
        {
            Key = "settings.audit",
            Title = "Audit"
        };

        var disabledParent = new VbenNavItem
        {
            Key = "settings",
            Title = "Settings",
            Disabled = true,
            Children = [disabledChild]
        };

        var component = new VbenSidebarMenu();
        var recorder = new EventRecorder<string[]>();
        SetParameter(component, nameof(VbenSidebarMenu.Items), new VbenNavItems([validParent, disabledParent]));
        SetParameter(component, nameof(VbenSidebarMenu.ExpandedKeys), new[] { "missing", "settings" });
        SetParameter(
            component,
            nameof(VbenSidebarMenu.ExpandedKeysChanged),
            EventCallback.Factory.Create<string[]>(recorder, recorder.Record));

        InvokeOnBranchToggled(component, validParent);

        Assert.AreEqual(1, recorder.Values.Count);
        CollectionAssert.AreEqual(new[] { "reports" }, recorder.Values[0]);
    }

    [TestMethod]
    public void Vben_SidebarMenu_OnBranchToggled_ImplicitSelectionExpansion_CanBeCollapsed()
    {
        var child = new VbenNavItem
        {
            Key = "reports.daily",
            Title = "Daily"
        };

        var parent = new VbenNavItem
        {
            Key = "reports",
            Title = "Reports",
            Children = [child]
        };

        var component = new VbenSidebarMenu();
        var recorder = new EventRecorder<string[]>();
        SetParameter(component, nameof(VbenSidebarMenu.Items), new VbenNavItems([parent]));
        SetParameter(component, nameof(VbenSidebarMenu.SelectedKey), "reports.daily");
        SetParameter(
            component,
            nameof(VbenSidebarMenu.ExpandedKeysChanged),
            EventCallback.Factory.Create<string[]>(recorder, recorder.Record));

        InvokeOnBranchToggled(component, parent);

        Assert.AreEqual(1, recorder.Values.Count);
        Assert.AreEqual(0, recorder.Values[0].Length);
    }

    [TestMethod]
    public void Vben_SidebarMenu_OnBranchToggled_DisabledBranch_DoesNotInvokeExpandedKeysChanged()
    {
        var child = new VbenNavItem
        {
            Key = "reports.daily",
            Title = "Daily"
        };

        var parent = new VbenNavItem
        {
            Key = "reports",
            Title = "Reports",
            Disabled = true,
            Children = [child]
        };

        var component = new VbenSidebarMenu();
        var recorder = new EventRecorder<string[]>();
        SetParameter(component, nameof(VbenSidebarMenu.Items), new VbenNavItems([parent]));
        SetParameter(
            component,
            nameof(VbenSidebarMenu.ExpandedKeysChanged),
            EventCallback.Factory.Create<string[]>(recorder, recorder.Record));

        InvokeOnBranchToggled(component, parent);

        Assert.AreEqual(0, recorder.Values.Count);
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

    private static void InvokeOnItemSelected(VbenSidebarMenu component, VbenNavItem item)
    {
        VbenNativeRenderTreeTestHelper.InvokeInstanceMethod<Task>(component, "OnItemSelected", item)
            .GetAwaiter()
            .GetResult();
    }

    private static void InvokeOnBranchToggled(VbenSidebarMenu component, VbenNavItem item)
    {
        VbenNativeRenderTreeTestHelper.InvokeInstanceMethod<Task>(component, "OnBranchToggled", item)
            .GetAwaiter()
            .GetResult();
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

    private static NativeRenderTreeSnapshot RenderSidebarMenu(VbenSidebarMenu component)
        => VbenNativeRenderTreeTestHelper.RenderComponent(component);

    private static bool ContainsElement(NativeRenderTreeSnapshot frames, string elementName)
        => frames.ContainsElement(elementName);

    private static bool ContainsAttribute(
        NativeRenderTreeSnapshot frames,
        string attributeName,
        string? expectedValue = null)
        => frames.ContainsAttribute(attributeName, expectedValue);

    private static readonly RenderFragment EmptyFragment = _ => { };
    private static readonly RenderFragment WhitespaceFragment = builder => builder.AddContent(0, "   ");

    private sealed class EventRecorder<TValue>
    {
        public List<TValue> Values { get; } = [];

        public Task Record(TValue value)
        {
            Values.Add(value);
            return Task.CompletedTask;
        }
    }
}
