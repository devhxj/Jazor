using Basic.Reference.Assemblies;
using Jazor.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Text.Json;

namespace Jazor.EmitTest;

[TestClass]
public sealed class RazorVueSfcEmitIntegrationTests
{
    [TestMethod]
    public void ModuleCollector_Collect_ReadsRazorVueSfcArtifacts_FromAssemblyCatalog()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var assemblyPath = Path.Combine(root, "RazorVue.Sfc.Reader.Tests.dll");
        EmitLoadContext? loadContext = null;

        try
        {
            Directory.CreateDirectory(root);
            WriteAssembly(assemblyPath, "RazorVue.Sfc.Reader.Tests", BuildSfcCatalogSource("RazorVue.Sfc.Reader.Tests", "components/counter-card.vue"));

            loadContext = new EmitLoadContext(assemblyPath);
            var collector = new ModuleCollector(loadContext);
            collector.AddAssembly(assemblyPath);

            var result = collector.Collect(failOnPathConflict: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            Assert.AreEqual(0, result.RazorVueCatalogCount);
            Assert.AreEqual(1, result.RazorVueSfcCatalogCount);
            Assert.HasCount(1, result.RazorVueSfcCatalogs);
            Assert.HasCount(1, result.RazorVueSfcArtifacts);
            Assert.AreEqual("components/counter-card.vue", result.RazorVueSfcArtifacts[0].RelativeSfcPath);
        }
        finally
        {
            loadContext?.Unload();
            ForceCollectibleLoadContextCleanup();
            TryDeleteDirectory(root);
        }
    }

    [TestMethod]
    public void ModuleCollector_Collect_RejectsMixedLegacyAndSfcRazorVueCatalogs()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var legacyAssemblyPath = Path.Combine(root, "RazorVue.Legacy.Reader.Tests.dll");
        var sfcAssemblyPath = Path.Combine(root, "RazorVue.Sfc.Reader.Tests.dll");
        EmitLoadContext? loadContext = null;

        try
        {
            Directory.CreateDirectory(root);
            WriteAssembly(legacyAssemblyPath, "RazorVue.Legacy.Reader.Tests", BuildLegacyCatalogSource("RazorVue.Legacy.Reader.Tests", "components/counter-card.mjs"));
            WriteAssembly(sfcAssemblyPath, "RazorVue.Sfc.Reader.Tests", BuildSfcCatalogSource("RazorVue.Sfc.Reader.Tests", "components/counter-card.vue"));

            loadContext = new EmitLoadContext(legacyAssemblyPath);
            var collector = new ModuleCollector(loadContext);
            collector.AddAssembly(legacyAssemblyPath);
            collector.AddAssembly(sfcAssemblyPath);

            var result = collector.Collect(failOnPathConflict: true);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Error, "Mixed legacy and SFC RazorVue catalogs");
        }
        finally
        {
            loadContext?.Unload();
            ForceCollectibleLoadContextCleanup();
            TryDeleteDirectory(root);
        }
    }

    [TestMethod]
    public void RazorVueSfcModuleWriter_WritesArtifactsAndManifest()
    {
        var writer = new RazorVueSfcModuleWriter();
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = RazorVueModuleWriter.GetManifestPath(Path.Combine(outputDirectory, "jazor-manifest.json"));
        var sourceFilePath = Path.Combine(root, "Counter.razor");
        var styleSourceFilePath = Path.Combine(root, "Counter.razor.css");
        var hostRequirementsModulePath = RazorVueModuleWriter.GetHostRequirementsModulePath(outputDirectory);
        var sfcPath = Path.Combine(outputDirectory, "components", "counter-card.vue");
        var mapPath = sfcPath + ".map";
        var originMapPath = sfcPath + ".origins.json";
        const string sfcText =
            """
            <template><div>{{ value }}</div></template>
            <script setup lang="ts">
            const value = 1;
            </script>
            <style scoped>
            .card { color: red; }
            </style>
            """;

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(sourceFilePath, "Counter component source");
            File.WriteAllText(styleSourceFilePath, ".card { color: red; }");

            var result = writer.Write(
                rootAssemblyPath: Path.Combine(root, "Demo.Host.dll"),
                outputDirectory,
                manifestPath,
                [
                    new RazorVueSfcCatalogRecord(
                        "Demo.Components",
                        [
                            CreateArtifact(sourceFilePath, styleSourceFilePath, sfcText)
                        ])
                ],
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            Assert.AreEqual(2, result.Written);
            Assert.IsTrue(File.Exists(sfcPath));
            Assert.IsTrue(File.Exists(mapPath));
            Assert.IsTrue(File.Exists(originMapPath));
            Assert.IsTrue(File.Exists(manifestPath));
            Assert.IsTrue(File.Exists(hostRequirementsModulePath));

            Assert.AreEqual(sfcText.ReplaceLineEndings("\n"), File.ReadAllText(sfcPath));
            Assert.IsFalse(File.ReadAllText(sfcPath).Contains("sourceMappingURL", StringComparison.Ordinal));

            using var originMap = JsonDocument.Parse(File.ReadAllText(originMapPath));
            Assert.AreEqual("components/counter-card.vue", originMap.RootElement.GetProperty("relativeSfcPath").GetString());
            Assert.AreEqual("style-hash", originMap.RootElement.GetProperty("styleHash").GetString());
            Assert.AreEqual("Style", originMap.RootElement.GetProperty("styleBlocks")[0].GetProperty("origins")[0].GetProperty("originKind").GetString());
            Assert.AreEqual("CustomBlock", originMap.RootElement.GetProperty("customBlocks")[0].GetProperty("origins")[0].GetProperty("originKind").GetString());

            using var map = JsonDocument.Parse(File.ReadAllText(mapPath));
            Assert.AreEqual("components/counter-card.vue", map.RootElement.GetProperty("file").GetString());
            Assert.AreEqual(sourceFilePath, map.RootElement.GetProperty("sources")[0].GetString());

            var manifest = RazorVueManifestSerializer.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("components/counter-card.vue", manifest.Modules[0].RelativeModulePath);
            Assert.AreEqual("style-hash", manifest.Modules[0].StyleHash);
            Assert.AreEqual("components/counter-card.vue.map", manifest.Modules[0].SourceMapPath);
            Assert.AreEqual("components/counter-card.vue.origins.json", manifest.Modules[0].OriginMapPath);

            var hostRequirementsCode = File.ReadAllText(hostRequirementsModulePath);
            StringAssert.Contains(hostRequirementsCode, "\"relativeModulePath\":\"components/counter-card.vue\"");
            StringAssert.Contains(hostRequirementsCode, "\"styleHash\":\"style-hash\"");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static RazorVueEmitSfcArtifactRecord CreateArtifact(string sourceFilePath, string styleSourceFilePath, string sfcText)
        => new(
            ComponentName: "CounterCard",
            RelativeSfcPath: "components/counter-card.vue",
            SfcText: sfcText.ReplaceLineEndings("\n"),
            TemplateBlock: new RazorVueEmitSfcTemplateBlockRecord(
                "<div>{{ value }}</div>",
                [
                    CreateOrigin(RazorVueSfcOriginKindRecord.Template, sourceFilePath, "components/counter-card.vue", 0, 48)
                ]),
            ScriptSetupBlock: new RazorVueEmitSfcScriptSetupBlockRecord(
                "const value = 1;",
                "ts",
                [
                    CreateOrigin(RazorVueSfcOriginKindRecord.Logic, sourceFilePath, "components/counter-card.vue", 49, 40)
                ]),
            StyleBlocks:
            [
                new RazorVueEmitSfcStyleBlockRecord(
                    ".card { color: red; }",
                    IsScoped: true,
                    ModuleName: null,
                    Language: "css",
                    SourceFilePath: styleSourceFilePath,
                    SourceOrigins:
                    [
                        CreateOrigin(RazorVueSfcOriginKindRecord.Style, styleSourceFilePath, "components/counter-card.vue", 90, 24)
                    ])
            ],
            CustomBlocks:
            [
                new RazorVueEmitSfcCustomBlockRecord(
                    "docs",
                    "{ \"category\": \"demo\" }",
                    "json",
                    [
                        new RazorVueEmitSfcAttributeRecord("category", "demo")
                    ],
                    sourceFilePath,
                    [
                        CreateOrigin(RazorVueSfcOriginKindRecord.CustomBlock, sourceFilePath, "components/counter-card.vue", 115, 10)
                    ])
            ],
            Imports: ["vue"],
            Styles: ["vuetify/styles"],
            PluginRequirements: ["vuetify"],
            Identity: new RazorVueEmitSfcArtifactIdentity(
                "Demo.Components.CounterCard",
                "components/counter-card.vue",
                "descriptor-hash",
                "template-hash",
                "logic-hash",
                "style-hash",
                RazorVueHmrBoundaryKind.LogicSafe),
            Hints: new RazorVueEmitRuntimeHints(true, false, true, false, false, false),
            SourceOrigins:
            [
                CreateOrigin(RazorVueSfcOriginKindRecord.Component, sourceFilePath, "components/counter-card.vue", 0, 128),
                CreateOrigin(RazorVueSfcOriginKindRecord.Template, sourceFilePath, "components/counter-card.vue", 0, 48),
                CreateOrigin(RazorVueSfcOriginKindRecord.Logic, sourceFilePath, "components/counter-card.vue", 49, 40),
                CreateOrigin(RazorVueSfcOriginKindRecord.Style, styleSourceFilePath, "components/counter-card.vue", 90, 24),
                CreateOrigin(RazorVueSfcOriginKindRecord.CustomBlock, sourceFilePath, "components/counter-card.vue", 115, 10)
            ]);

    private static RazorVueEmitSfcSourceOriginRecord CreateOrigin(
        RazorVueSfcOriginKindRecord originKind,
        string sourceFilePath,
        string generatedFilePath,
        int generatedSpanStart,
        int generatedSpanLength)
        => new(
            originKind,
            sourceFilePath,
            generatedSpanStart,
            generatedSpanLength,
            generatedFilePath,
            generatedSpanStart,
            generatedSpanLength,
            1,
            1,
            RazorVueMappingQualityRecord.MappedFromGenerated,
            RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation);

    private static void WriteAssembly(string assemblyPath, string assemblyName, string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, path: $"{assemblyName}.g.cs")],
            Net100.References.All.Cast<MetadataReference>(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = File.Create(assemblyPath);
        var emitResult = compilation.Emit(stream);
        Assert.IsTrue(emitResult.Success, string.Join("\n", emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    private static void ForceCollectibleLoadContextCleanup()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static string BuildLegacyCatalogSource(string assemblyName, string relativeModulePath)
        => $$"""
        using System;

        namespace Jazor.Generated
        {
            internal static class RazorVueCatalog
            {
                internal static string AssemblyName => "{{assemblyName}}";

                internal static System.Collections.IEnumerable GetArtifacts()
                    => new object[] { new GeneratedArtifact() };

                private sealed class GeneratedArtifact
                {
                    public string ComponentName => "CounterCard";
                    public string RelativeModulePath => "{{relativeModulePath}}";
                    public string ModuleCode => "export default {};";
                    public string[] Imports => new[] { "vue" };
                    public string[] Styles => Array.Empty<string>();
                    public string[] PluginRequirements => Array.Empty<string>();
                    public GeneratedIdentity Identity => new GeneratedIdentity();
                    public GeneratedHints Hints => new GeneratedHints();
                    public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
                }

                private sealed class GeneratedIdentity
                {
                    public string ComponentId => "Demo.Components.CounterCard";
                    public string ModuleId => "{{relativeModulePath}}";
                    public string DescriptorHash => "descriptor-hash";
                    public string TemplateHash => "template-hash";
                    public string LogicHash => "logic-hash";
                    public GeneratedHmrBoundaryKind HmrBoundaryKind => GeneratedHmrBoundaryKind.LogicSafe;
                }

                private sealed class GeneratedHints
                {
                    public bool RequiresVueRuntime => true;
                    public bool RequiresHydration => false;
                    public bool SupportsSsr => true;
                    public bool UsesTeleport => false;
                    public bool UsesSuspense => false;
                    public bool UsesKeepAlive => false;
                }

                private sealed class GeneratedOrigin
                {
                    public string SourceFilePath => "Counter.razor";
                    public int SourceSpanStart => 0;
                    public int SourceSpanLength => 16;
                    public string GeneratedFilePath => "{{relativeModulePath}}";
                    public int GeneratedSpanStart => 0;
                    public int GeneratedSpanLength => 16;
                    public int StartLine => 1;
                    public int StartColumn => 1;
                    public GeneratedMappingQuality MappingQuality => GeneratedMappingQuality.MappedFromGenerated;
                    public GeneratedOriginProvenance Provenance => GeneratedOriginProvenance.GeneratedSyntaxLocation;
                }

                private enum GeneratedHmrBoundaryKind
                {
                    Unknown,
                    TemplateOnly,
                    LogicSafe,
                    FullReloadRequired
                }

                private enum GeneratedMappingQuality
                {
                    ExactSource,
                    MappedFromGenerated,
                    GeneratedOnly
                }

                private enum GeneratedOriginProvenance
                {
                    RazorSourceMap,
                    GeneratedSyntaxLocation,
                    GeneratedFallback
                }
            }
        }
        """;

    private static string BuildSfcCatalogSource(string assemblyName, string relativeSfcPath)
        => $$$"""
        using System;

        namespace Jazor.Generated
        {
            internal static class RazorVueCatalog
            {
                internal static string AssemblyName => "{{{assemblyName}}}";

                internal static System.Collections.IEnumerable GetArtifacts()
                    => new object[] { new GeneratedArtifact() };

                private sealed class GeneratedArtifact
                {
                    public string ComponentName => "CounterCard";
                    public string RelativeSfcPath => "{{{relativeSfcPath}}}";
                    public string SfcText => "<template><div>{{ value }}</div></template>";
                    public GeneratedTemplateBlock TemplateBlock => new GeneratedTemplateBlock();
                    public GeneratedScriptSetupBlock ScriptSetupBlock => new GeneratedScriptSetupBlock();
                    public GeneratedStyleBlock[] StyleBlocks => new[] { new GeneratedStyleBlock() };
                    public GeneratedCustomBlock[] CustomBlocks => Array.Empty<GeneratedCustomBlock>();
                    public string[] Imports => new[] { "vue" };
                    public string[] Styles => Array.Empty<string>();
                    public string[] PluginRequirements => Array.Empty<string>();
                    public GeneratedIdentity Identity => new GeneratedIdentity();
                    public GeneratedHints Hints => new GeneratedHints();
                    public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin(GeneratedOriginKind.Component) };
                }

                private sealed class GeneratedTemplateBlock
                {
                    public string Text => "<div>{{ value }}</div>";
                    public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin(GeneratedOriginKind.Template) };
                }

                private sealed class GeneratedScriptSetupBlock
                {
                    public string Text => "const value = 1;";
                    public string Language => "ts";
                    public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin(GeneratedOriginKind.Logic) };
                }

                private sealed class GeneratedStyleBlock
                {
                    public string Text => ".card { color: red; }";
                    public bool IsScoped => true;
                    public string? ModuleName => null;
                    public string Language => "css";
                    public string SourceFilePath => "Counter.razor.css";
                    public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin(GeneratedOriginKind.Style) };
                }

                private sealed class GeneratedCustomBlock
                {
                    public string Name => "docs";
                    public string Text => "{ }";
                    public string? Language => "json";
                    public GeneratedAttribute[] Attributes => Array.Empty<GeneratedAttribute>();
                    public string SourceFilePath => "Counter.razor";
                    public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin(GeneratedOriginKind.CustomBlock) };
                }

                private sealed class GeneratedAttribute
                {
                    public string Name => "kind";
                    public string? Value => "demo";
                }

                private sealed class GeneratedIdentity
                {
                    public string ComponentId => "Demo.Components.CounterCard";
                    public string ModuleId => "{{{relativeSfcPath}}}";
                    public string DescriptorHash => "descriptor-hash";
                    public string TemplateHash => "template-hash";
                    public string LogicHash => "logic-hash";
                    public string StyleHash => "style-hash";
                    public GeneratedHmrBoundaryKind HmrBoundaryKind => GeneratedHmrBoundaryKind.LogicSafe;
                }

                private sealed class GeneratedHints
                {
                    public bool RequiresVueRuntime => true;
                    public bool RequiresHydration => false;
                    public bool SupportsSsr => true;
                    public bool UsesTeleport => false;
                    public bool UsesSuspense => false;
                    public bool UsesKeepAlive => false;
                }

                private sealed class GeneratedOrigin
                {
                    public GeneratedOrigin(GeneratedOriginKind originKind)
                    {
                        OriginKind = originKind;
                    }

                    public GeneratedOriginKind OriginKind { get; }
                    public string SourceFilePath => "Counter.razor";
                    public int SourceSpanStart => 0;
                    public int SourceSpanLength => 16;
                    public string GeneratedFilePath => "{{{relativeSfcPath}}}";
                    public int GeneratedSpanStart => 0;
                    public int GeneratedSpanLength => 16;
                    public int StartLine => 1;
                    public int StartColumn => 1;
                    public GeneratedMappingQuality MappingQuality => GeneratedMappingQuality.MappedFromGenerated;
                    public GeneratedOriginProvenance Provenance => GeneratedOriginProvenance.GeneratedSyntaxLocation;
                }

                private enum GeneratedHmrBoundaryKind
                {
                    Unknown,
                    TemplateOnly,
                    LogicSafe,
                    FullReloadRequired
                }

                private enum GeneratedOriginKind
                {
                    Component,
                    Descriptor,
                    Template,
                    Logic,
                    GeneratedRender,
                    Style,
                    CustomBlock
                }

                private enum GeneratedMappingQuality
                {
                    ExactSource,
                    MappedFromGenerated,
                    GeneratedOnly
                }

                private enum GeneratedOriginProvenance
                {
                    RazorSourceMap,
                    GeneratedSyntaxLocation,
                    GeneratedFallback
                }
            }
        }
        """;
}
