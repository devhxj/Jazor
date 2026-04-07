using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using ECMAScript.UI.Vue.Vuetify;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue;
using Jazor.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class RazorVueComponentRegistryTests
{
    [TestMethod]
    public void RazorVue_Registry_ResolveImportedUserComponent()
    {
        var registry = CreateRegistry(
        [
            CreateUserComponent("Counter", "Demo.Components")
        ]);

        var result = registry.Resolve(
            "Counter",
            VueComponentResolutionContext.Create("Demo.Pages", "Demo.Components"));

        Assert.AreEqual(VueComponentResolutionStatus.Resolved, result.Status);
        Assert.IsNotNull(result.Descriptor);
        Assert.IsEmpty(result.Issues);
        Assert.AreEqual("Demo.Components.Counter", result.Descriptor.FullName);
    }

    [TestMethod]
    public void RazorVue_Registry_ResolveFullyQualifiedName_WithoutImports()
    {
        var registry = CreateRegistry(
        [
            CreateUserComponent("Counter", "Demo.Components")
        ]);

        var result = registry.Resolve(
            "Demo.Components.Counter",
            VueComponentResolutionContext.Create("Demo.Pages"));

        Assert.AreEqual(VueComponentResolutionStatus.Resolved, result.Status);
        Assert.IsNotNull(result.Descriptor);
        Assert.IsEmpty(result.Issues);
        Assert.AreEqual("Counter", result.Descriptor.Name);
    }

    [TestMethod]
    public void RazorVue_Registry_ResolveLibraryComponent_FromUsing()
    {
        var registry = CreateRegistry(
            [],
        [
            CreateLibraryComponent("VBtn", "ECMAScript.UI.Vue.Vuetify")
        ]);

        var result = registry.Resolve(
            "VBtn",
            VueComponentResolutionContext.Create("Demo.Pages", "ECMAScript.UI.Vue.Vuetify"));

        Assert.AreEqual(VueComponentResolutionStatus.Resolved, result.Status);
        Assert.IsNotNull(result.Descriptor);
        Assert.IsEmpty(result.Issues);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, result.Descriptor.SourceKind);
        Assert.AreEqual("vuetify/components", result.Descriptor.ImportSpecifier);
    }

    [TestMethod]
    public void RazorVue_Registry_AmbiguousShortName_ReturnsAmbiguous()
    {
        var registry = CreateRegistry(
            [],
        [
            CreateLibraryComponent("Dialog", "Demo.Ui.Primary"),
            CreateLibraryComponent("Dialog", "Demo.Ui.Secondary")
        ]);

        var result = registry.Resolve(
            "Dialog",
            VueComponentResolutionContext.Create("Demo.Pages", "Demo.Ui.Primary", "Demo.Ui.Secondary"));

        Assert.AreEqual(VueComponentResolutionStatus.Ambiguous, result.Status);
        Assert.HasCount(2, result.Candidates);
        Assert.HasCount(1, result.Issues);
        Assert.AreEqual(RazorVueIssueCode.AmbiguousComponentName, result.Issues[0].Code);
        CollectionAssert.AreEquivalent(
            new[] { "Demo.Ui.Primary.Dialog", "Demo.Ui.Secondary.Dialog" },
            result.Issues[0].RelatedComponentNames.ToArray());
    }

    [TestMethod]
    public void RazorVue_Registry_IntrinsicName_IsReservedAgainstVisibleUserComponent()
    {
        var registry = CreateRegistry(
        [
            CreateUserComponent("Teleport", "Demo.Components")
        ]);

        var result = registry.Resolve(
            "Teleport",
            VueComponentResolutionContext.Create("Demo.Pages", "Demo.Components"));

        Assert.AreEqual(VueComponentResolutionStatus.ReservedIntrinsicName, result.Status);
        Assert.HasCount(2, result.Candidates);
        Assert.HasCount(1, result.Issues);
        Assert.AreEqual(RazorVueIssueCode.ReservedIntrinsicNameCollision, result.Issues[0].Code);
        Assert.IsTrue(result.Candidates.Any(candidate => candidate.SourceKind == VueComponentSourceKind.Intrinsic));
        Assert.IsTrue(result.Candidates.Any(candidate => candidate.SourceKind == VueComponentSourceKind.UserComponent));
    }

    [TestMethod]
    public void RazorVue_Registry_IntrinsicComponent_IsAlwaysResolvable()
    {
        var registry = CreateRegistry([]);

        var result = registry.Resolve(
            "Teleport",
            VueComponentResolutionContext.Create("Demo.Pages"));

        Assert.AreEqual(VueComponentResolutionStatus.Resolved, result.Status);
        Assert.IsNotNull(result.Descriptor);
        Assert.IsEmpty(result.Issues);
        Assert.AreEqual(VueComponentSourceKind.Intrinsic, result.Descriptor.SourceKind);
        Assert.AreEqual("vue", result.Descriptor.ImportSpecifier);
    }

    [TestMethod]
    public void RazorVue_Registry_MissingComponent_ReturnsCompilerIssue()
    {
        var registry = CreateRegistry([]);

        var result = registry.Resolve(
            "MissingCard",
            VueComponentResolutionContext.Create("Demo.Pages", "Demo.Components"));

        Assert.AreEqual(VueComponentResolutionStatus.NotFound, result.Status);
        Assert.IsNull(result.Descriptor);
        Assert.HasCount(1, result.Issues);
        Assert.AreEqual(RazorVueIssueCode.ComponentNotFound, result.Issues[0].Code);
        Assert.AreEqual(RazorVueIssueSeverity.Error, result.Issues[0].Severity);
        Assert.AreEqual("Component 'MissingCard' is not visible in the current RazorVue resolution scope.", result.Issues[0].Message);
    }

    [TestMethod]
    public void RazorVue_Registry_CreateFromCompilationContext_ResolvesDiscoveredLibraryComponent()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;
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

            namespace Demo.Ui.Custom
            {
                [VueLibraryComponent("demo/components", "DemoButton")]
                [VueLibraryStyle("demo/styles")]
                [VueLibraryPluginRequirement("demo-host")]
                public sealed class DemoButton : VueLibraryComponent
                {
                    [Parameter]
                    public string? Text { get; set; }
                }
            }
            """);

        var registry = context.CreateComponentRegistry();
        var result = registry.Resolve(
            "DemoButton",
            VueComponentResolutionContext.Create("Demo.Pages", "Demo.Ui.Custom"));

        Assert.AreEqual(VueComponentResolutionStatus.Resolved, result.Status);
        Assert.IsNotNull(result.Descriptor);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, result.Descriptor.SourceKind);
        Assert.AreEqual("demo/components", result.Descriptor.ImportSpecifier);
        CollectionAssert.AreEqual(new[] { "demo/styles" }, result.Descriptor.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "demo-host" }, result.Descriptor.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void RazorVue_Registry_CreateFromCompilationContext_ResolvesVuetifyPackageComponents()
    {
        var context = CreateContext(
            """
            using System;
            using Jazor.RazorVue;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/host-card")]
                public class HostCard : VueComponent
                {
                }
            }
            """);

        var registry = context.CreateComponentRegistry();
        foreach (var componentName in new[] { "VBtn", "VCard", "VCardText", "VCardTitle", "VCheckbox", "VCol", "VContainer", "VDialog", "VDivider", "VIcon", "VRow", "VSheet", "VSpacer", "VTextField", "VToolbar", "VToolbarTitle" })
        {
            var result = registry.Resolve(
                componentName,
                VueComponentResolutionContext.Create("Demo.Pages", "ECMAScript.UI.Vue.Vuetify"));

            Assert.AreEqual(VueComponentResolutionStatus.Resolved, result.Status, componentName);
            Assert.IsNotNull(result.Descriptor, componentName);
            Assert.AreEqual(VueComponentSourceKind.LibraryComponent, result.Descriptor.SourceKind, componentName);
            Assert.AreEqual("vuetify/components", result.Descriptor.ImportSpecifier, componentName);
            CollectionAssert.AreEqual(new[] { "vuetify" }, result.Descriptor.PluginRequirements.ToArray(), componentName);
        }
    }

    private static VueComponentRegistry CreateRegistry(
        ImmutableArray<VueComponentDescriptor> userComponents,
        ImmutableArray<VueComponentDescriptor> libraryComponents = default(ImmutableArray<VueComponentDescriptor>))
        => VueComponentRegistry.Create(userComponents, libraryComponents);

    private static VueComponentDescriptor CreateUserComponent(string name, string resolutionNamespace)
        => new(
            Name: name,
            FullName: string.IsNullOrEmpty(resolutionNamespace) ? name : resolutionNamespace + "." + name,
            SourceKind: VueComponentSourceKind.UserComponent,
            ResolutionNamespace: resolutionNamespace,
            ImportSpecifier: "./" + name + ".mjs",
            ExportName: "default",
            Props: [],
            Emits: [],
            Slots: [],
            StyleDependencies: [],
            PluginRequirements: [],
            Flags: VueComponentFlags.None);

    private static VueComponentDescriptor CreateLibraryComponent(string name, string resolutionNamespace)
        => new(
            Name: name,
            FullName: resolutionNamespace + "." + name,
            SourceKind: VueComponentSourceKind.LibraryComponent,
            ResolutionNamespace: resolutionNamespace,
            ImportSpecifier: "vuetify/components",
            ExportName: name,
            Props: [],
            Emits: [],
            Slots: [],
            StyleDependencies: [],
            PluginRequirements: ["vuetify"],
            Flags: VueComponentFlags.None);

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VueComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VBtn).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).BaseType!.Assembly.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.Registry.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }
}

