using System.Collections.Immutable;
using Jazor.RazorVue.Analysis.Artifacts;

namespace Jazor.ComplierTest;

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

    private static VueCompiledArtifact CreateArtifact(string componentName, string relativeModulePath)
        => new(
            ComponentName: componentName,
            RelativeModulePath: relativeModulePath,
            ModuleCode: $"export default {{ name: \"{componentName}\" }};",
            Imports: ImmutableArray.Create("vue"),
            Styles: ImmutableArray<string>.Empty,
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

