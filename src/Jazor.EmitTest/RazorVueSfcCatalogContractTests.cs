using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class RazorVueSfcCatalogContractTests
{
    [TestMethod]
    public void RazorVueEmitSfcArtifactRecord_PreservesBlockAwareMetadata()
    {
        var componentOrigin = CreateOrigin(RazorVueSfcOriginKindRecord.Component, "Counter.razor", "components/counter-card.vue", 0, 128);
        var templateOrigin = CreateOrigin(RazorVueSfcOriginKindRecord.Template, "Counter.razor", "components/counter-card.vue", 0, 48);
        var scriptOrigin = CreateOrigin(RazorVueSfcOriginKindRecord.Logic, "Counter.razor", "components/counter-card.vue", 49, 40);
        var styleOrigin = CreateOrigin(RazorVueSfcOriginKindRecord.Style, "Counter.razor.css", "components/counter-card.vue", 90, 24);
        var customOrigin = CreateOrigin(RazorVueSfcOriginKindRecord.CustomBlock, "Counter.razor", "components/counter-card.vue", 115, 10);

        var artifact = new RazorVueEmitSfcArtifactRecord(
            ComponentName: "CounterCard",
            RelativeSfcPath: "components/counter-card.vue",
            SfcText:
            """
            <template><div>{{ value }}</div></template>
            <script setup lang="ts">
            const value = 1;
            </script>
            <style scoped>
            .card { color: red; }
            </style>
            """,
            TemplateBlock: new RazorVueEmitSfcTemplateBlockRecord(
                Text: "<div>{{ value }}</div>",
                SourceOrigins: [templateOrigin]),
            ScriptSetupBlock: new RazorVueEmitSfcScriptSetupBlockRecord(
                Text: "const value = 1;",
                Language: "ts",
                SourceOrigins: [scriptOrigin]),
            StyleBlocks:
            [
                new RazorVueEmitSfcStyleBlockRecord(
                    Text: ".card { color: red; }",
                    IsScoped: true,
                    ModuleName: "card",
                    Language: "css",
                    SourceFilePath: "Counter.razor.css",
                    SourceOrigins: [styleOrigin])
            ],
            CustomBlocks:
            [
                new RazorVueEmitSfcCustomBlockRecord(
                    Name: "docs",
                    Text: "{ \"category\": \"demo\" }",
                    Language: "json",
                    Attributes:
                    [
                        new RazorVueEmitSfcAttributeRecord("category", "demo")
                    ],
                    SourceFilePath: "Counter.razor",
                    SourceOrigins: [customOrigin])
            ],
            RouteTemplates: ["/", "/counter"],
            Imports: ["vue"],
            Styles: ["vuetify/styles"],
            PluginRequirements: ["vuetify"],
            Identity: new RazorVueEmitSfcArtifactIdentity(
                ComponentId: "Demo.Components.CounterCard",
                ModuleId: "components/counter-card.vue",
                DescriptorHash: "descriptor-hash",
                TemplateHash: "template-hash",
                LogicHash: "logic-hash",
                StyleHash: "style-hash",
                HmrBoundaryKind: RazorVueHmrBoundaryKind.LogicSafe),
            Hints: new RazorVueEmitRuntimeHints(
                RequiresVueRuntime: true,
                RequiresHydration: false,
                SupportsSsr: true,
                UsesTeleport: false,
                UsesSuspense: false,
                UsesKeepAlive: false),
            SourceOrigins: [componentOrigin, templateOrigin, scriptOrigin, styleOrigin, customOrigin]);

        var catalog = new RazorVueSfcCatalogRecord("Demo.Components", [artifact]);

        Assert.AreEqual("Demo.Components", catalog.AssemblyName);
        Assert.HasCount(1, catalog.Artifacts);
        Assert.AreEqual("components/counter-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual("<div>{{ value }}</div>", artifact.TemplateText);
        Assert.AreEqual("const value = 1;", artifact.ScriptSetupText);
        Assert.AreEqual("style-hash", artifact.Identity.StyleHash);
        CollectionAssert.AreEqual(new[] { "/", "/counter" }, artifact.RouteTemplates.ToArray());
        Assert.AreEqual(RazorVueSfcOriginKindRecord.Style, artifact.StyleBlocks[0].SourceOrigins[0].OriginKind);
        Assert.AreEqual("card", artifact.StyleBlocks[0].ModuleName);
        Assert.AreEqual(RazorVueSfcOriginKindRecord.CustomBlock, artifact.CustomBlocks[0].SourceOrigins[0].OriginKind);
        Assert.AreEqual("category", artifact.CustomBlocks[0].Attributes[0].Name);
        CollectionAssert.AreEqual(new[] { "vue" }, artifact.Imports.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
    }

    private static RazorVueEmitSfcSourceOriginRecord CreateOrigin(
        RazorVueSfcOriginKindRecord originKind,
        string sourceFilePath,
        string generatedFilePath,
        int generatedSpanStart,
        int generatedSpanLength)
        => new(
            OriginKind: originKind,
            SourceFilePath: sourceFilePath,
            SourceSpanStart: generatedSpanStart,
            SourceSpanLength: generatedSpanLength,
            GeneratedFilePath: generatedFilePath,
            GeneratedSpanStart: generatedSpanStart,
            GeneratedSpanLength: generatedSpanLength,
            StartLine: 1,
            StartColumn: 1,
            MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
            Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation);
}
