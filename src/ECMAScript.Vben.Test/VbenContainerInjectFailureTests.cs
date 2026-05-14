using System.Collections.Immutable;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.CSharp;

namespace ECMAScript.Vben.Test;

public sealed partial class VbenContainerInjectTests
{
    [TestMethod]
    public void Vben_PageContainer_ContainerInject_WithMissingImplementationProp_ThrowsInvalidContainerInjectDeclaration()
    {
        var exception = ResolveInvalidContainerInject(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenPageContainer),
                typeof(Demo.Implementations.InvalidPageContainer))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElCard")]
                public sealed class InvalidPageContainer : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenPageContainer>
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; }

                    [Parameter]
                    public VbenPageAction[]? Actions { get; set; }

                    [Parameter]
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Extra { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/dashboard-page")]
                public sealed class DashboardPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "missing compatible prop");
        StringAssert.Contains(exception.Issue.Message, nameof(VbenPageContainer.Subtitle));
    }

    [TestMethod]
    public void Vben_HeaderBar_ContainerInject_WithPropTypeMismatch_ThrowsInvalidContainerInjectDeclaration()
    {
        var exception = ResolveInvalidContainerInject(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenHeaderBar),
                typeof(Demo.Implementations.InvalidHeaderBar))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("demo-shell/components", "AppHeader")]
                public sealed class InvalidHeaderBar : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenHeaderBar>
                {
                    [Parameter]
                    public int Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }

                    [Parameter]
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Logo { get; set; }

                    [Parameter]
                    public RenderFragment? Actions { get; set; }

                    [Parameter]
                    public RenderFragment? UserRegion { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/header-page")]
                public sealed class HeaderPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenHeaderBar));
                        builder.CloseComponent();
                    }
                }
            }
            """,
            pageComponentName: "HeaderPage");

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "prop");
        StringAssert.Contains(exception.Issue.Message, nameof(VbenHeaderBar.Title));
        StringAssert.Contains(exception.Issue.Message, "string?");
        StringAssert.Contains(exception.Issue.Message, "int");
    }

    [TestMethod]
    public void Vben_SidebarMenu_ContainerInject_WithEmitPayloadMismatch_ThrowsInvalidContainerInjectDeclaration()
    {
        var exception = ResolveInvalidContainerInject(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenSidebarMenu),
                typeof(Demo.Implementations.InvalidSidebarMenu))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueProp(nameof(VbenSidebarMenu.SelectedKey), VuePropKind.Model, Name = "selected-key", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(VbenSidebarMenu.SelectedKeyChanged), VueEmitKind.ModelUpdate, Name = "update:selected-key", PayloadTypeName = "System.Int32")]
                public sealed class InvalidSidebarMenu : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenSidebarMenu>
                {
                    [Parameter]
                    public bool Collapsed { get; set; }

                    [Parameter]
                    public string? SelectedKey { get; set; }

                    [Parameter]
                    public EventCallback<int> SelectedKeyChanged { get; set; }

                    [Parameter]
                    public string[]? ExpandedKeys { get; set; }

                    [Parameter]
                    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

                    [Parameter]
                    public VbenNavItems? Items { get; set; }

                    [Parameter]
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Logo { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/sidebar-page")]
                public sealed class SidebarPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenSidebarMenu));
                        builder.CloseComponent();
                    }
                }
            }
            """,
            pageComponentName: "SidebarPage");

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "emit");
        StringAssert.Contains(exception.Issue.Message, nameof(VbenSidebarMenu.SelectedKeyChanged));
        StringAssert.Contains(exception.Issue.Message, "string");
        StringAssert.Contains(exception.Issue.Message, "System.Int32");
    }

    [TestMethod]
    public void Vben_PageContainer_ContainerInject_WithDefaultSlotMismatch_ThrowsInvalidContainerInjectDeclaration()
    {
        var exception = ResolveInvalidContainerInject(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenPageContainer),
                typeof(Demo.Implementations.InvalidPageContainer))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElCard")]
                [VueSlot(nameof(VbenPageContainer.Extra), Name = "header-extra")]
                [VueSlot(nameof(VbenContentComponentBase.ChildContent), Name = "body", IsDefault = false)]
                public sealed class InvalidPageContainer : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenPageContainer>
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
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Extra { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/dashboard-page")]
                public sealed class DashboardPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "slot");
        StringAssert.Contains(exception.Issue.Message, nameof(VbenContentComponentBase.ChildContent));
        StringAssert.Contains(exception.Issue.Message, "IsDefault=False");
        StringAssert.Contains(exception.Issue.Message, "IsDefault=True");
    }

    [TestMethod]
    public void Vben_PageContainer_ContainerInject_WithCaptureUnmatchedValuesMismatch_ThrowsInvalidContainerInjectDeclaration()
    {
        var exception = ResolveInvalidContainerInject(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenPageContainer),
                typeof(Demo.Implementations.InvalidPageContainer))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElCard")]
                public sealed class InvalidPageContainer : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenPageContainer>
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
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Extra { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/dashboard-page")]
                public sealed class DashboardPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, nameof(VbenComponentBase.AdditionalAttributes));
        StringAssert.Contains(exception.Issue.Message, "CaptureUnmatchedValues=False");
        StringAssert.Contains(exception.Issue.Message, "CaptureUnmatchedValues=True");
    }

    [TestMethod]
    public void Vben_AdminLayout_ContainerInject_WithDuplicateRegistrations_ThrowsInvalidContainerInjectDeclaration()
    {
        var exception = ResolveInvalidContainerInject(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenAdminLayout),
                typeof(Demo.Implementations.ElementAdminLayout))]
            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenAdminLayout),
                typeof(Demo.Implementations.VuetifyAdminLayout))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElContainer")]
                public sealed class ElementAdminLayout : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenAdminLayout>
                {
                    [Parameter] public VbenLayoutMode Mode { get; set; }
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public VbenNavItems? NavItems { get; set; }
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Header { get; set; }
                    [Parameter] public RenderFragment? Sidebar { get; set; }
                    [Parameter] public RenderFragment? HeaderActions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }

                [VueLibraryComponent("vuetify/components", "VLayout")]
                public sealed class VuetifyAdminLayout : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenAdminLayout>
                {
                    [Parameter] public VbenLayoutMode Mode { get; set; }
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public VbenNavItems? NavItems { get; set; }
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Header { get; set; }
                    [Parameter] public RenderFragment? Sidebar { get; set; }
                    [Parameter] public RenderFragment? HeaderActions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/layout-page")]
                public sealed class LayoutPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenAdminLayout));
                        builder.CloseComponent();
                    }
                }
            }
            """,
            pageComponentName: "LayoutPage");

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "duplicate implementations");
        StringAssert.Contains(exception.Issue.Message, "ECMAScript.Vben.VbenAdminLayout");
    }

    [TestMethod]
    public void Vben_SidebarMenu_ContainerInject_WithMismatchedImplementationContract_ThrowsInvalidContainerInjectDeclaration()
    {
        var exception = ResolveInvalidContainerInject(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenSidebarMenu),
                typeof(Demo.Implementations.WrongSidebarMenu))]

            namespace Demo.Containers
            {
                [ECMAScriptModule("./contracts/secondary-page-container")]
                public sealed class SecondaryPageContainer : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter] public string? Title { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                public sealed class WrongSidebarMenu : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.SecondaryPageContainer>
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/sidebar-page")]
                public sealed class SidebarPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenSidebarMenu));
                        builder.CloseComponent();
                    }
                }
            }
            """,
            pageComponentName: "SidebarPage");

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "declares container contract");
        StringAssert.Contains(exception.Issue.Message, "Demo.Containers.SecondaryPageContainer");
    }
}
