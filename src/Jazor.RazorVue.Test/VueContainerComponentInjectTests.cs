using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class VueContainerComponentInjectTests
{
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
        Assert.AreEqual("Demo.Implementations.ElementPlusNavShell", resolvedDescriptor!.FullName);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, resolvedDescriptor.SourceKind);
        Assert.AreEqual("element-plus", resolvedDescriptor.ImportSpecifier);
        Assert.AreEqual("ElMenu", resolvedDescriptor.ExportName);
        Assert.AreEqual("Demo.Containers.NavShell", resolvedDescriptor.ContainerContractFullName);
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
