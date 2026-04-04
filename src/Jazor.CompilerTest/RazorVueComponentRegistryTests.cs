using System.Collections.Immutable;
using Jazor.RazorVue.Analysis.Descriptor;

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
            Flags: VueComponentFlags.None);
}

