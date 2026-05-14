using Jazor.RazorVue;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class VueContainerComponentInjectTests
{
    private static RazorVueSfcArtifactFactory CreateBuildRenderTreeArtifactFactory()
        => new(BuildRenderTreeTemplateFrontend.Instance);

    private static RazorVuePipeline CreateBuildRenderTreePipeline()
        => new(BuildRenderTreeTemplateFrontend.Instance);

    [TestMethod]
    public void RazorVue_ContainerComponentInject_ResolvesConfiguredImplementationDescriptor()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueLibraryStyle("element-plus/dist/index.css")]
                [VueLibraryPluginRequirement("element-plus")]
                [VueProp(nameof(Title), Name = "title")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var resolvedComponents = RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);

        Assert.IsTrue(resolvedComponents.TryGetValue("NavShell", out var resolvedDescriptor));
        Assert.IsNotNull(resolvedDescriptor);
        Assert.AreEqual("Demo.Containers.NavShell", resolvedDescriptor!.FullName);
        Assert.AreEqual("NavShell", resolvedDescriptor.Name);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, resolvedDescriptor.SourceKind);
        Assert.AreEqual("element-plus", resolvedDescriptor.ImportSpecifier);
        Assert.AreEqual("ElMenu", resolvedDescriptor.ExportName);
        Assert.AreEqual("Demo.Containers.NavShell", resolvedDescriptor.ContainerContractFullName);
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_MergesContractAuthoringWithImplementationRuntimeShape()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Contracts
            {
                public sealed record HeaderContext;
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueLibraryStyle("element-plus/dist/index.css")]
                [VueLibraryPluginRequirement("element-plus")]
                [VueProp(nameof(Title), Name = "menuTitle")]
                [VueProp(nameof(Value), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue", PayloadTypeName = "System.String?")]
                [VueSlot(nameof(Header), Name = "header", ContextTypeName = "Demo.Contracts.HeaderContext", ContextParameterName = "headerContext")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
                        builder.AddComponentParameter(2, nameof(Demo.Containers.NavShell.Value), Value);
                        builder.AddComponentParameter(3, nameof(Demo.Containers.NavShell.ValueChanged), ValueChanged);
                        builder.AddComponentParameter(4, nameof(Demo.Containers.NavShell.Header), Header);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var resolvedComponents = RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);
        var resolvedDescriptor = resolvedComponents["NavShell"];

        Assert.AreEqual("Demo.Containers.NavShell", resolvedDescriptor.FullName);
        Assert.AreEqual("NavShell", resolvedDescriptor.Name);
        Assert.AreEqual("Demo.Containers", resolvedDescriptor.ResolutionNamespace);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, resolvedDescriptor.SourceKind);
        Assert.AreEqual("element-plus", resolvedDescriptor.ImportSpecifier);
        Assert.AreEqual("ElMenu", resolvedDescriptor.ExportName);
        CollectionAssert.AreEqual(new[] { "element-plus/dist/index.css" }, resolvedDescriptor.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, resolvedDescriptor.PluginRequirements.ToArray());
        Assert.AreEqual("Demo.Containers.NavShell", resolvedDescriptor.ContainerContractFullName);
        Assert.AreEqual(VueComponentFlags.None, resolvedDescriptor.Flags);

        var titleProp = resolvedDescriptor.Props.Single(static item => item.PublicName == "Title");
        Assert.AreEqual("menuTitle", titleProp.Name);
        Assert.AreEqual("Title", titleProp.PublicName);
        Assert.AreEqual("string?", titleProp.TypeName);
        Assert.IsFalse(titleProp.Required);
        Assert.IsFalse(titleProp.AcceptsBinding);
        Assert.AreEqual((string?)null, titleProp.DefaultExpression);
        Assert.AreEqual(VuePropKind.Normal, titleProp.Kind);

        var valueProp = resolvedDescriptor.Props.Single(static item => item.PublicName == "Value");
        Assert.AreEqual("modelValue", valueProp.Name);
        Assert.IsTrue(valueProp.AcceptsBinding);
        Assert.AreEqual(VuePropKind.Model, valueProp.Kind);

        var valueChangedEmit = resolvedDescriptor.Emits.Single(static item => item.RazorAlias == "ValueChanged");
        Assert.AreEqual("update:modelValue", valueChangedEmit.Name);
        Assert.AreEqual("string?", valueChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, valueChangedEmit.Kind);

        var headerSlot = resolvedDescriptor.Slots.Single(static item => item.PublicName == "Header");
        Assert.AreEqual("header", headerSlot.Name);
        Assert.AreEqual("Header", headerSlot.PublicName);
        Assert.IsFalse(headerSlot.Required);
        Assert.AreEqual("context", headerSlot.Parameters[0].Name);
        Assert.AreEqual("Demo.Contracts.HeaderContext", headerSlot.Parameters[0].TypeName);
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_WithMissingImplementationProp_ThrowsInvalidContainerInjectDeclaration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueProp(nameof(Title), Name = "title")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree));

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "missing compatible prop");
        StringAssert.Contains(exception.Issue.Message, "Subtitle");
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_WithPropTypeMismatch_ThrowsInvalidContainerInjectDeclaration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public int Title { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree));

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "prop");
        StringAssert.Contains(exception.Issue.Message, "Title");
        StringAssert.Contains(exception.Issue.Message, "string?");
        StringAssert.Contains(exception.Issue.Message, "int");
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_WithEmitPayloadMismatch_ThrowsInvalidContainerInjectDeclaration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueProp(nameof(Value), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue", PayloadTypeName = "System.Int32")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree));

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "emit");
        StringAssert.Contains(exception.Issue.Message, "ValueChanged");
        StringAssert.Contains(exception.Issue.Message, "string?");
        StringAssert.Contains(exception.Issue.Message, "System.Int32");
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_WithSlotContextMismatch_ThrowsInvalidContainerInjectDeclaration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Contracts
            {
                public sealed record HeaderContext;
                public sealed record AlternativeHeaderContext;
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueSlot(nameof(Header), Name = "header", ContextTypeName = "Demo.Contracts.AlternativeHeaderContext")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public RenderFragment<Demo.Contracts.AlternativeHeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree));

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "slot");
        StringAssert.Contains(exception.Issue.Message, "Header");
        StringAssert.Contains(exception.Issue.Message, "Demo.Contracts.HeaderContext");
        StringAssert.Contains(exception.Issue.Message, "Demo.Contracts.AlternativeHeaderContext");
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_WithDuplicateRegistrations_ThrowsInvalidContainerInjectDeclaration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]
            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.VuetifyNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }
                }

                [VueLibraryComponent("vuetify/components", "VNavigationDrawer")]
                public sealed class VuetifyNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree));

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "duplicate implementations");
        StringAssert.Contains(exception.Issue.Message, "Demo.Containers.NavShell");
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_WithMismatchedImplementationContract_ThrowsInvalidContainerInjectDeclaration()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.WrongNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./containers/secondary-shell")]
                public sealed class SecondaryShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                public sealed class WrongNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.SecondaryShell>
                {
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree));

        Assert.AreEqual(RazorVueIssueCode.InvalidContainerInjectDeclaration, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "declares container contract");
        StringAssert.Contains(exception.Issue.Message, "Demo.Containers.SecondaryShell");
    }

    [TestMethod]
    public void RazorVue_ContainerContractDescriptor_TracksOwnContractFullName()
    {
        var snapshot = CreateSingleSnapshot(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """);

        Assert.AreEqual("Demo.Containers.NavShell", snapshot.Descriptor.ContainerContractFullName);
        Assert.AreEqual("Demo.Containers.NavShell", snapshot.Descriptor.FullName);
        Assert.AreEqual(VueComponentSourceKind.UserComponent, snapshot.Descriptor.SourceKind);
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_LowersInjectedRuntimeShape_IntoVueSfcArtifact()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Contracts
            {
                public sealed record HeaderContext(string Title);
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueLibraryStyle("element-plus/dist/index.css")]
                [VueLibraryPluginRequirement("element-plus")]
                [VueProp(nameof(Title), Name = "menuTitle")]
                [VueProp(nameof(Value), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue", PayloadTypeName = "System.String?")]
                [VueSlot(nameof(Header), Name = "header", ContextTypeName = "Demo.Contracts.HeaderContext", ContextParameterName = "headerContext")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
                        builder.AddComponentParameter(2, nameof(Demo.Containers.NavShell.Value), Value);
                        builder.AddComponentParameter(3, nameof(Demo.Containers.NavShell.ValueChanged), ValueChanged);
                        builder.AddComponentParameter(4, nameof(Demo.Containers.NavShell.Header), Header);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HomePage");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import { ElMenu as ElMenu } from \"element-plus\";");
        StringAssert.Contains(artifact.TemplateText, "<ElMenu menuTitle=\"Overview\" :modelValue=\"props.value\" @update:modelValue=\"(__value) =&gt; emit(&quot;update:value&quot;, __value)\">");
        StringAssert.Contains(artifact.TemplateText, "<template #header=\"context\">");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"header\" v-bind=\"context\" />");
        Assert.IsFalse(artifact.TemplateText.Contains("title=\"Overview\"", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains(":value=\"props.value\"", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains("@valueChanged=", StringComparison.Ordinal), artifact.TemplateText);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "element-plus");
        CollectionAssert.AreEqual(new[] { "element-plus/dist/index.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_ContainerComponentInject_LowersInjectedRuntimeShape_IntoPipelineArtifact()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Contracts
            {
                public sealed record HeaderContext(string Title);
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueLibraryStyle("element-plus/dist/index.css")]
                [VueLibraryPluginRequirement("element-plus")]
                [VueProp(nameof(Title), Name = "menuTitle")]
                [VueProp(nameof(Value), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue", PayloadTypeName = "System.String?")]
                [VueSlot(nameof(Header), Name = "header", ContextTypeName = "Demo.Contracts.HeaderContext", ContextParameterName = "headerContext")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./pages/home-page")]
                public sealed class HomePage : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Value { get; set; }

                    [Parameter]
                    public EventCallback<string?> ValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(Demo.Containers.NavShell));
                        builder.AddComponentParameter(1, nameof(Demo.Containers.NavShell.Title), "Overview");
                        builder.AddComponentParameter(2, nameof(Demo.Containers.NavShell.Value), Value);
                        builder.AddComponentParameter(3, nameof(Demo.Containers.NavShell.ValueChanged), ValueChanged);
                        builder.AddComponentParameter(4, nameof(Demo.Containers.NavShell.Header), Header);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "HomePage");

        StringAssert.Contains(artifact.ModuleCode, "import { ElMenu as NavShellComponent } from \"element-plus\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(NavShellComponent, { \"menuTitle\": \"Overview\", \"modelValue\": props.value, \"onUpdate:modelValue\": (__value) => emit(\"update:value\", __value) }, { header: (context) => slots.header ? slots.header(context) : null });");
        Assert.IsFalse(artifact.ModuleCode.Contains("\"title\": \"Overview\"", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"value\": props.value", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"onValueChanged\"", StringComparison.Ordinal), artifact.ModuleCode);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "element-plus");
        CollectionAssert.AreEqual(new[] { "element-plus/dist/index.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, artifact.PluginRequirements.ToArray());
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.ContainerInject.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context!;
    }

    private static Jazor.RazorVue.Artifacts.RazorVueSemanticSnapshot CreateSingleSnapshot(string source)
    {
        var context = CreateContext(source);
        var snapshots = context.CreateSemanticSnapshots();
        Assert.AreEqual(1, snapshots.Length);
        return snapshots[0];
    }
}
