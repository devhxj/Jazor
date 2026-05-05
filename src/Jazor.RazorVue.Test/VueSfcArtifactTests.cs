using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class VueSfcArtifactTests
{
    [TestMethod]
    public void VueSfcArtifact_ExposesBlockAwareMetadata()
    {
        var componentOrigin = CreateOrigin(RazorVueOriginKind.Component, "Counter.razor", "components/counter-card.vue", 0, 128);
        var templateOrigin = CreateOrigin(RazorVueOriginKind.Template, "Counter.razor", "components/counter-card.vue", 0, 48);
        var scriptOrigin = CreateOrigin(RazorVueOriginKind.Logic, "Counter.razor", "components/counter-card.vue", 49, 40);
        var styleOrigin = CreateOrigin(RazorVueOriginKind.Style, "Counter.razor.css", "components/counter-card.vue", 90, 24);
        var customOrigin = CreateOrigin(RazorVueOriginKind.CustomBlock, "Counter.razor", "components/counter-card.vue", 115, 10);

        var artifact = new VueSfcArtifact(
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
            TemplateBlock: new VueSfcTemplateBlock(
                Text: "<div>{{ value }}</div>",
                SourceOrigins: [templateOrigin]),
            ScriptSetupBlock: new VueSfcScriptSetupBlock(
                Text: "const value = 1;",
                Language: "ts",
                SourceOrigins: [scriptOrigin]),
            StyleBlocks:
            [
                new VueSfcStyleBlock(
                    Text: ".card { color: red; }",
                    IsScoped: true,
                    ModuleName: null,
                    Language: "css",
                    SourceFilePath: "Counter.razor.css",
                    SourceOrigins: [styleOrigin])
            ],
            CustomBlocks:
            [
                new VueSfcCustomBlock(
                    Name: "docs",
                    Text: "{ \"category\": \"demo\" }",
                    Language: "json",
                    Attributes:
                    [
                        new VueSfcAttribute("category", "demo")
                    ],
                    SourceFilePath: "Counter.razor",
                    SourceOrigins: [customOrigin])
            ],
            Imports: ["vue"],
            Styles: ["vuetify/styles"],
            PluginRequirements: ["vuetify"],
            Identity: new VueSfcArtifactIdentity(
                ComponentId: "Demo.Components.CounterCard",
                ModuleId: "components/counter-card.vue",
                DescriptorHash: "descriptor-hash",
                TemplateHash: "template-hash",
                LogicHash: "logic-hash",
                StyleHash: "style-hash",
                HmrBoundaryKind: HmrBoundaryKind.LogicSafe),
            Hints: new VueRuntimeHints(
                RequiresVueRuntime: true,
                RequiresHydration: false,
                SupportsSsr: true,
                UsesTeleport: false,
                UsesSuspense: false,
                UsesKeepAlive: false),
            SourceOrigins: [componentOrigin, templateOrigin, scriptOrigin, styleOrigin, customOrigin]);

        Assert.AreEqual("components/counter-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual("<div>{{ value }}</div>", artifact.TemplateText);
        Assert.AreEqual("const value = 1;", artifact.ScriptSetupText);
        Assert.AreEqual("style-hash", artifact.Identity.StyleHash);
        Assert.IsTrue(artifact.StyleBlocks[0].IsScoped);
        Assert.AreEqual("css", artifact.StyleBlocks[0].Language);
        Assert.AreEqual(RazorVueOriginKind.Style, artifact.StyleBlocks[0].SourceOrigins[0].OriginKind);
        Assert.AreEqual("category", artifact.CustomBlocks[0].Attributes[0].Name);
        Assert.AreEqual(RazorVueOriginKind.CustomBlock, artifact.CustomBlocks[0].SourceOrigins[0].OriginKind);
        CollectionAssert.AreEqual(new[] { "vue" }, artifact.Imports.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify/styles" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
        Assert.HasCount(5, artifact.SourceOrigins);
    }

    private static RazorVueSourceOrigin CreateOrigin(
        RazorVueOriginKind originKind,
        string sourceFilePath,
        string generatedFilePath,
        int generatedSpanStart,
        int generatedSpanLength)
        => new(
            OriginKind: originKind,
            SourceFilePath: sourceFilePath,
            SourceSpanStart: generatedSpanStart,
            SourceSpanLength: generatedSpanLength,
            StartLine: 1,
            StartColumn: 1,
            GeneratedFilePath: generatedFilePath,
            GeneratedSpanStart: generatedSpanStart,
            GeneratedSpanLength: generatedSpanLength,
            MappingQuality: RazorVueMappingQuality.MappedFromGenerated,
            Provenance: RazorVueOriginProvenance.GeneratedSyntaxLocation);
}
