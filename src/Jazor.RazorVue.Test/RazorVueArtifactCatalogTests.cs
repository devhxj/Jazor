using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueArtifactCatalogTests
{
    [TestMethod]
    public void RazorVue_CatalogBuilder_SortsAndNormalizesArtifactPaths()
    {
        var builder = new RazorVueCatalogBuilder();
        var catalog = builder.Build(
            "Demo.Assembly",
            [
                CreateArtifact("ZetaCard", "\\components\\zeta-card.mjs"),
                CreateArtifact("AlphaCard", "components/alpha-card.mjs")
            ]);

        Assert.AreEqual("Demo.Assembly", catalog.AssemblyName);
        Assert.HasCount(2, catalog.Artifacts);
        Assert.AreEqual("AlphaCard", catalog.Artifacts[0].ComponentName);
        Assert.AreEqual("components/alpha-card.mjs", catalog.Artifacts[0].RelativeModulePath);
        Assert.AreEqual("ZetaCard", catalog.Artifacts[1].ComponentName);
        Assert.AreEqual("components/zeta-card.mjs", catalog.Artifacts[1].RelativeModulePath);
    }

    [TestMethod]
    public void RazorVue_CatalogBuilder_PreservesRouteTemplates()
    {
        var builder = new RazorVueCatalogBuilder();
        var catalog = builder.Build(
            "Demo.Assembly",
            [
                CreateArtifact("CatalogPage", "components/catalog-page.mjs", "/", "/catalog")
            ]);

        CollectionAssert.AreEqual(
            new[] { "/", "/catalog" },
            catalog.Artifacts[0].RouteTemplates.ToArray());
    }

    [TestMethod]
    public void RazorVue_CatalogBuilder_RejectsEscapingPaths()
    {
        var builder = new RazorVueCatalogBuilder();
        InvalidOperationException? exception = null;
        try
        {
            builder.Build(
                "Demo.Assembly",
                [
                    CreateArtifact("EscapingCard", "../escape/card.mjs")
                ]);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "cannot escape output directory");
    }

    [TestMethod]
    public void RazorVue_CatalogBuilder_RejectsAbsolutePaths()
    {
        var builder = new RazorVueCatalogBuilder();
        InvalidOperationException? exception = null;
        try
        {
            builder.Build(
                "Demo.Assembly",
                [
                    CreateArtifact("AbsoluteCard", "C:/temp/absolute-card.mjs")
                ]);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "must be relative");
    }

    [TestMethod]
    public void RazorVue_CatalogBuilder_RejectsEmptyAssemblyName()
    {
        var builder = new RazorVueCatalogBuilder();
        ArgumentException? exception = null;
        try
        {
            builder.Build(
                "",
                [
                    CreateArtifact("Card", "components/card.mjs")
                ]);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "Assembly name cannot be empty");
    }

    private static VueCompiledArtifact CreateArtifact(string componentName, string relativeModulePath, params string[] routeTemplates)
        => new(
            ComponentName: componentName,
            RelativeModulePath: relativeModulePath,
            ModuleCode: $"export default {{ name: \"{componentName}\" }};",
            RouteTemplates: routeTemplates.ToImmutableArray(),
            Imports: ImmutableArray.Create("vue"),
            Styles: ImmutableArray<string>.Empty,
            PluginRequirements: ImmutableArray<string>.Empty,
            Identity: new VueArtifactIdentity(
                ComponentId: componentName,
                ModuleId: relativeModulePath,
                DescriptorHash: "descriptor-hash",
                TemplateHash: "template-hash",
                LogicHash: "logic-hash",
                HmrBoundaryKind: HmrBoundaryKind.Unknown),
            Hints: new VueRuntimeHints(
                RequiresVueRuntime: true,
                RequiresHydration: false,
                SupportsSsr: true,
                UsesTeleport: false,
                UsesSuspense: false,
                UsesKeepAlive: false),
            SourceOrigins:
            [
                new RazorVueSourceOrigin(
                    RazorVueOriginKind.Component,
                    "Counter.razor",
                    0,
                    10,
                    1,
                    1,
                    "Counter.razor.g.cs",
                    0,
                    10,
                    RazorVueMappingQuality.MappedFromGenerated,
                    RazorVueOriginProvenance.GeneratedSyntaxLocation)
            ]);
}
