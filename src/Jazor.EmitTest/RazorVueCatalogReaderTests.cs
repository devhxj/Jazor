using Basic.Reference.Assemblies;
using ECMAScript.Contract;
using Jazor.Emit;
using Jazor.RazorVue;
using Jazor.RazorVue.Analysis;
using Jazor.RazorVue.Artifacts;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Reflection;

namespace Jazor.EmitTest
{
    [TestClass]
    public sealed class RazorVueCatalogReaderTests
    {
        [TestMethod]
        public void RazorVueCatalogReader_ReadsGeneratedCatalogFromAssembly()
        {
            var catalog = RazorVueCatalogReader.TryRead(typeof(RazorVueCatalogReaderTests).Assembly);

            Assert.IsNotNull(catalog);
            Assert.AreEqual("RazorVue.Reader.Tests", catalog.AssemblyName);
            Assert.HasCount(1, catalog.Artifacts);

            var artifact = catalog.Artifacts[0];
            Assert.AreEqual("CounterCard", artifact.ComponentName);
            Assert.AreEqual("components/counter-card.mjs", artifact.RelativeModulePath);
            CollectionAssert.AreEquivalent(new[] { "vue", "./button.mjs" }, artifact.Imports.ToArray());
            CollectionAssert.AreEquivalent(new[] { "vuetify/styles" }, artifact.Styles.ToArray());
            CollectionAssert.AreEquivalent(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
            Assert.IsTrue(artifact.Hints.RequiresVueRuntime);
            Assert.IsTrue(artifact.Hints.SupportsSsr);
            Assert.HasCount(1, artifact.SourceOrigins);
            Assert.AreEqual(RazorVueMappingQualityRecord.MappedFromGenerated, artifact.SourceOrigins[0].MappingQuality);
            Assert.AreEqual("components/counter-card.mjs", artifact.SourceOrigins[0].GeneratedFilePath);
            Assert.AreEqual(0, artifact.SourceOrigins[0].GeneratedSpanStart);
            Assert.AreEqual(38, artifact.SourceOrigins[0].GeneratedSpanLength);
        }

        [TestMethod]
        public void RazorVueCatalogReader_ReadsGeneratedCatalogFromRealGeneratorAssembly()
        {
            const string sourcePath = "CounterCard.razor";
            var compilation = CreateRazorVueCompilation(
                "RazorVue.Reader.Integration.Tests",
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

                namespace Demo.Components
                {
                    [ECMAScript.ECMAScriptModule("./components/counter-card")]
                    public class CounterCard : ComponentBase, IVueComponent
                    {
                        [Parameter]
                        public int Value { get; set; }
                    }
                }
                """,
                sourcePath);
            var location = compilation.GetTypeByMetadataName("Demo.Components.CounterCard")!
                .Locations
                .Single(static item => item.IsInSource);
            var expectedOrigin = RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Component);
            var assembly = CompileRazorVueGeneratedAssembly(compilation, "legacy");

            var catalog = RazorVueCatalogReader.TryRead(assembly);

            Assert.IsNotNull(catalog);
            Assert.HasCount(1, catalog.Artifacts);
            Assert.HasCount(1, catalog.Artifacts[0].SourceOrigins);
            Assert.AreEqual(expectedOrigin.GeneratedFilePath, catalog.Artifacts[0].SourceOrigins[0].GeneratedFilePath);
            Assert.AreEqual(expectedOrigin.GeneratedSpanStart, catalog.Artifacts[0].SourceOrigins[0].GeneratedSpanStart);
            Assert.AreEqual(expectedOrigin.GeneratedSpanLength, catalog.Artifacts[0].SourceOrigins[0].GeneratedSpanLength);
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenSourceOriginsIsNull()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullSourceOrigins",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[] Imports => new[] { "vue" };
                public string[] Styles => Array.Empty<string>();
                public string[] PluginRequirements => Array.Empty<string>();
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[]? SourceOrigins => null;
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "SourceOrigins");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenSourceOriginsContainsNullEntry()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullSourceOriginEntry",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[] Imports => new[] { "vue" };
                public string[] Styles => Array.Empty<string>();
                public string[] PluginRequirements => Array.Empty<string>();
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[] SourceOrigins => new GeneratedOrigin?[] { null };
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "SourceOrigins");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenImportsIsNull()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullImports",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[]? Imports => null;
                public string[] Styles => Array.Empty<string>();
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "Imports");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenImportsContainsNullEntry()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullImportEntry",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[] Imports => new string?[] { null };
                public string[] Styles => Array.Empty<string>();
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "Imports");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenStylesIsNull()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullStyles",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[] Imports => new[] { "vue" };
                public string[]? Styles => null;
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "Styles");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenStylesContainsNullEntry()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullStyleEntry",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[] Imports => new[] { "vue" };
                public string[] Styles => new string?[] { null };
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "Styles");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenPluginRequirementsIsNull()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullPluginRequirements",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[] Imports => new[] { "vue" };
                public string[] Styles => Array.Empty<string>();
                public string[]? PluginRequirements => null;
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "PluginRequirements");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenPluginRequirementsContainsNullEntry()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullPluginRequirementEntry",
                """
                public string ComponentName => "CounterCard";
                public string RelativeModulePath => "components/counter-card.mjs";
                public string ModuleCode => "export default {};";
                public string[] Imports => new[] { "vue" };
                public string[] Styles => Array.Empty<string>();
                public string[] PluginRequirements => new string?[] { null };
                public GeneratedIdentity Identity => new();
                public GeneratedHints Hints => new();
                public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "PluginRequirements");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenGetArtifactsContainsNullEntry()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.NullArtifactEntry",
                DefaultGeneratedArtifactMembers,
                getArtifactsExpression: "new object?[] { null }");

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "null artifact entry");
        }

        [TestMethod]
        public void RazorVueCatalogReader_ThrowsWhenGeneratedSpanStartIsNotInt32OrNull()
        {
            var assembly = CompileCatalogAssembly(
                "RazorVue.Reader.BadGeneratedSpanStart",
                DefaultGeneratedArtifactMembers,
                """
                private sealed class GeneratedOrigin
                {
                    public string SourceFilePath => "Counter.razor";
                    public int SourceSpanStart => 12;
                    public int SourceSpanLength => 8;
                    public string? GeneratedFilePath => "components/counter-card.mjs";
                    public string GeneratedSpanStart => "0";
                    public int? GeneratedSpanLength => 38;
                    public int StartLine => 2;
                    public int StartColumn => 4;
                    public GeneratedMappingQuality MappingQuality => GeneratedMappingQuality.MappedFromGenerated;
                    public GeneratedOriginProvenance Provenance => GeneratedOriginProvenance.GeneratedSyntaxLocation;
                }
                """);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueCatalogReader.TryRead(assembly));
            StringAssert.Contains(exception.Message, "GeneratedSpanStart");
        }

        [TestMethod]
        public void RazorVueManifestModel_CreateSaveAndLoad_PreservesArtifactMetadata()
        {
            var catalog = RazorVueCatalogReader.TryRead(typeof(RazorVueCatalogReaderTests).Assembly);
            Assert.IsNotNull(catalog);

            var manifest = RazorVueManifestFactory.Create(catalog);

            Assert.AreEqual("RazorVue.Reader.Tests", manifest.AssemblyName);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("RazorVue.Reader.Tests", manifest.Modules[0].AssemblyName);
            Assert.AreEqual("CounterCard", manifest.Modules[0].ComponentId);
            Assert.AreEqual("components/counter-card.mjs", manifest.Modules[0].ModuleId);
            Assert.AreEqual("CounterCard", manifest.Modules[0].ComponentName);
            Assert.AreEqual("components/counter-card.mjs", manifest.Modules[0].RelativeModulePath);
            Assert.AreEqual("components/counter-card.mjs.map", manifest.Modules[0].SourceMapPath);
            Assert.AreEqual("components/counter-card.mjs.origins.json", manifest.Modules[0].OriginMapPath);
            Assert.AreEqual("descriptor-hash", manifest.Modules[0].DescriptorHash);
            Assert.AreEqual("template-hash", manifest.Modules[0].TemplateHash);
            Assert.AreEqual("logic-hash", manifest.Modules[0].LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, manifest.Modules[0].HmrBoundaryKind);
            Assert.IsTrue(manifest.Modules[0].SupportsSsr);
            CollectionAssert.AreEquivalent(new[] { "vuetify" }, manifest.Modules[0].PluginRequirements);
            CollectionAssert.AreEquivalent(new[] { "vuetify" }, manifest.PluginRequirements);
            Assert.IsFalse(string.IsNullOrWhiteSpace(manifest.Modules[0].ContentHash));

            var manifestPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"), "razorvue-manifest.json");
            try
            {
                manifest.Save(manifestPath);
                var loaded = RazorVueManifestSerializer.TryLoad(manifestPath);

                Assert.IsNotNull(loaded);
                Assert.AreEqual(manifest.AssemblyName, loaded.AssemblyName);
                Assert.HasCount(1, loaded.Modules);
                Assert.AreEqual(manifest.Modules[0].ComponentId, loaded.Modules[0].ComponentId);
                Assert.AreEqual(manifest.Modules[0].ModuleId, loaded.Modules[0].ModuleId);
                Assert.AreEqual(manifest.Modules[0].SourceMapPath, loaded.Modules[0].SourceMapPath);
                Assert.AreEqual(manifest.Modules[0].OriginMapPath, loaded.Modules[0].OriginMapPath);
                Assert.AreEqual(manifest.Modules[0].ContentHash, loaded.Modules[0].ContentHash);
                CollectionAssert.AreEquivalent(manifest.Modules[0].Imports, loaded.Modules[0].Imports);
                CollectionAssert.AreEquivalent(manifest.Modules[0].Styles, loaded.Modules[0].Styles);
                CollectionAssert.AreEquivalent(manifest.Modules[0].PluginRequirements, loaded.Modules[0].PluginRequirements);
                CollectionAssert.AreEquivalent(manifest.Styles, loaded.Styles);
                CollectionAssert.AreEquivalent(manifest.PluginRequirements, loaded.PluginRequirements);
            }
            finally
            {
                var directory = Path.GetDirectoryName(manifestPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
        }

        [TestMethod]
        public void RazorVueManifestModel_Create_NormalizesHostRequirementOrdering()
        {
            var catalog = new RazorVueCatalogRecord(
                "Demo.Components",
                [
                    new RazorVueEmitArtifactRecord(
                        "DashboardCard",
                        "components/dashboard-card.mjs",
                        "export default { name: \"DashboardCard\" };",
                        ["vue", "vuetify/components"],
                        ["vuetify/styles", "vuetify/base", "vuetify/styles"],
                        ["vuetify", "alpha-host", "vuetify"],
                        new RazorVueEmitArtifactIdentity(
                            "Demo.Components.DashboardCard",
                            "components/dashboard-card.mjs",
                            "descriptor-hash",
                            "template-hash",
                            "logic-hash",
                            RazorVueHmrBoundaryKind.LogicSafe),
                        new RazorVueEmitRuntimeHints(true, false, true, false, false, false),
                        []),
                ]);

            var manifest = RazorVueManifestFactory.Create(catalog);

            CollectionAssert.AreEqual(
                new[] { "vuetify/base", "vuetify/styles" },
                manifest.Modules[0].Styles);
            CollectionAssert.AreEqual(
                new[] { "alpha-host", "vuetify" },
                manifest.Modules[0].PluginRequirements);
            CollectionAssert.AreEqual(
                new[] { "vuetify/base", "vuetify/styles" },
                manifest.Styles);
            CollectionAssert.AreEqual(
                new[] { "alpha-host", "vuetify" },
                manifest.PluginRequirements);
        }

        [TestMethod]
        public void RazorVueManifestModel_TryLoad_BackfillsTopLevelRequirements_FromLegacyManifestJson()
        {
            var manifestPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"), "legacy-razorvue-manifest.json");

            try
            {
                var directory = Path.GetDirectoryName(manifestPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(
                    manifestPath,
                    """
                    {
                      "AssemblyName": "Demo.Host",
                      "GeneratedAtUtc": "2026-04-07T00:00:00Z",
                      "Modules": [
                        {
                          "AssemblyName": "Demo.Components",
                          "ComponentId": null,
                          "ModuleId": null,
                          "ComponentName": "CounterCard",
                          "RelativeModulePath": "components/counter-card.mjs",
                          "SourceMapPath": null,
                          "OriginMapPath": null,
                          "Imports": [ "vue" ],
                          "Styles": [ "vuetify/styles", " feature/flags.css ", "vuetify/styles" ],
                          "PluginRequirements": [ "vuetify", " feature-flags ", "vuetify" ],
                          "DescriptorHash": "descriptor-hash",
                          "TemplateHash": "template-hash",
                          "LogicHash": "logic-hash",
                          "ContentHash": "content-hash",
                          "HmrBoundaryKind": 2,
                          "RequiresHydration": false,
                          "SupportsSsr": true
                        }
                      ]
                    }
                    """.ReplaceLineEndings("\n"));

                var manifest = RazorVueManifestSerializer.TryLoad(manifestPath);

                Assert.IsNotNull(manifest);
                CollectionAssert.AreEqual(
                    new[] { "feature/flags.css", "vuetify/styles" },
                    manifest.Styles);
                CollectionAssert.AreEqual(
                    new[] { "feature-flags", "vuetify" },
                    manifest.PluginRequirements);
                CollectionAssert.AreEqual(
                    new[] { "feature/flags.css", "vuetify/styles" },
                    manifest.Modules[0].Styles);
                CollectionAssert.AreEqual(
                    new[] { "feature-flags", "vuetify" },
                    manifest.Modules[0].PluginRequirements);
                Assert.AreEqual("Demo.Components::CounterCard", manifest.Modules[0].ComponentId);
                Assert.AreEqual("components/counter-card.mjs", manifest.Modules[0].ModuleId);
                Assert.AreEqual("components/counter-card.mjs.map", manifest.Modules[0].SourceMapPath);
                Assert.AreEqual("components/counter-card.mjs.origins.json", manifest.Modules[0].OriginMapPath);
            }
            finally
            {
                var directory = Path.GetDirectoryName(manifestPath);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
        }

        private const string DefaultGeneratedArtifactMembers =
            """
            public string ComponentName => "CounterCard";
            public string RelativeModulePath => "components/counter-card.mjs";
            public string ModuleCode => "export default {};";
            public string[] Imports => new[] { "vue" };
            public string[] Styles => Array.Empty<string>();
            public string[] PluginRequirements => new[] { "vuetify" };
            public GeneratedIdentity Identity => new();
            public GeneratedHints Hints => new();
            public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin() };
            """;

        private static Assembly CompileCatalogAssembly(string assemblyName, string artifactMembers, string? generatedOriginType = null, string? getArtifactsExpression = null)
        {
            var source = $$"""
            using System;

            namespace Jazor.Generated
            {
                internal static class RazorVueCatalog
                {
                    internal static string AssemblyName => "{{assemblyName}}";

                    internal static System.Collections.IEnumerable GetArtifacts()
                        => {{getArtifactsExpression ?? "new object[] { new GeneratedArtifact() }"}};

                    private sealed class GeneratedArtifact
                    {
            {{artifactMembers}}
                    }

                    private sealed class GeneratedIdentity
                    {
                        public string ComponentId => "CounterCard";
                        public string ModuleId => "components/counter-card.mjs";
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

            {{generatedOriginType ??
            """
                    private sealed class GeneratedOrigin
                    {
                        public string SourceFilePath => "Counter.razor";
                        public int SourceSpanStart => 12;
                        public int SourceSpanLength => 8;
                        public string? GeneratedFilePath => "components/counter-card.mjs";
                        public int? GeneratedSpanStart => 0;
                        public int? GeneratedSpanLength => 38;
                        public int StartLine => 2;
                        public int StartColumn => 4;
                        public GeneratedMappingQuality MappingQuality => GeneratedMappingQuality.MappedFromGenerated;
                        public GeneratedOriginProvenance Provenance => GeneratedOriginProvenance.GeneratedSyntaxLocation;
                    }
            """}}

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

            return CompileAssembly(CreateCompilation(assemblyName, source, $"{assemblyName}.g.cs", CreateBaseReferences()));
        }

        private static CSharpCompilation CreateRazorVueCompilation(string assemblyName, string source, string sourcePath)
            => CreateCompilation(assemblyName, source, sourcePath, CreateRazorVueReferences());

        private static CSharpCompilation CreateCompilation(string assemblyName, string source, string sourcePath, IEnumerable<MetadataReference> references)
            => CSharpCompilation.Create(
                assemblyName: assemblyName,
                syntaxTrees: [CSharpSyntaxTree.ParseText(source, path: sourcePath)],
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        private static Assembly CompileRazorVueGeneratedAssembly(Compilation compilation, string razorVueOutputMode)
        {
            GeneratorDriver driver = CSharpGeneratorDriver.Create(
                [new RazorVueGenerator().AsSourceGenerator()],
                additionalTexts: null,
                parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options,
                optionsProvider: CreateAnalyzerConfigOptionsProvider(razorVueOutputMode),
                driverOptions: default);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            var diagnostics = outputCompilation.GetDiagnostics()
                .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .ToArray();
            Assert.AreEqual(0, diagnostics.Length, string.Join("\n", diagnostics.Select(static diagnostic => diagnostic.ToString())));

            return CompileAssembly(outputCompilation);
        }

        private static Assembly CompileAssembly(Compilation compilation)
        {
            using var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);
            Assert.IsTrue(emitResult.Success, string.Join("\n", emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
            stream.Position = 0;
            return Assembly.Load(stream.ToArray());
        }

        private static IEnumerable<MetadataReference> CreateBaseReferences()
            => Net100.References.All.Cast<MetadataReference>();

        private static AnalyzerConfigOptionsProvider CreateAnalyzerConfigOptionsProvider(string razorVueOutputMode)
            => new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.JazorRazorVueOutputMode"] = razorVueOutputMode
            });

		private static IEnumerable<MetadataReference> CreateRazorVueReferences()
			=> CreateBaseReferences().Concat([
				MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3.IVueComponent).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ComponentBase).Assembly.Location)
            ]);

        private sealed class TestAnalyzerConfigOptionsProvider(IReadOnlyDictionary<string, string> globalOptions) : AnalyzerConfigOptionsProvider
        {
            private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
            private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(StringComparer.Ordinal));

            public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
                => EmptyOptions;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
                => EmptyOptions;
        }

        private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
        {
            private readonly IReadOnlyDictionary<string, string> _values = values;

            public override bool TryGetValue(string key, out string value)
                => _values.TryGetValue(key, out value!);
        }
    }
}

namespace Jazor.Generated
{
    internal static partial class RazorVueCatalog
    {
        internal static string AssemblyName { get; } = "RazorVue.Reader.Tests";

        internal static System.Collections.IEnumerable GetArtifacts()
            => _artifacts;

        private static readonly GeneratedArtifact[] _artifacts =
        [
            new GeneratedArtifact(
                componentName: "CounterCard",
                relativeModulePath: "components/counter-card.mjs",
                moduleCode: "export default { name: \"CounterCard\" };",
                imports: ["vue", "./button.mjs"],
                styles: ["vuetify/styles"],
                pluginRequirements: ["vuetify"],
                identity: new GeneratedIdentity(
                    componentId: "CounterCard",
                    moduleId: "components/counter-card.mjs",
                    descriptorHash: "descriptor-hash",
                    templateHash: "template-hash",
                    logicHash: "logic-hash",
                    hmrBoundaryKind: GeneratedHmrBoundaryKind.LogicSafe),
                hints: new GeneratedHints(
                    requiresVueRuntime: true,
                    requiresHydration: false,
                    supportsSsr: true,
                    usesTeleport: false,
                    usesSuspense: false,
                    usesKeepAlive: false),
                sourceOrigins:
                [
                    new GeneratedOrigin(
                        sourceFilePath: "Counter.razor",
                        sourceSpanStart: 12,
                        sourceSpanLength: 8,
                        generatedFilePath: "components/counter-card.mjs",
                        generatedSpanStart: 0,
                        generatedSpanLength: 38,
                        startLine: 2,
                        startColumn: 4,
                        mappingQuality: GeneratedMappingQuality.MappedFromGenerated,
                        provenance: GeneratedOriginProvenance.GeneratedSyntaxLocation)
                ])
        ];

        private sealed class GeneratedArtifact(
            string componentName,
            string relativeModulePath,
            string moduleCode,
            string[] imports,
            string[] styles,
            string[] pluginRequirements,
            GeneratedIdentity identity,
            GeneratedHints hints,
            GeneratedOrigin[] sourceOrigins)
        {
            public string ComponentName { get; } = componentName;
            public string RelativeModulePath { get; } = relativeModulePath;
            public string ModuleCode { get; } = moduleCode;
            public string[] Imports { get; } = imports;
            public string[] Styles { get; } = styles;
            public string[] PluginRequirements { get; } = pluginRequirements;
            public GeneratedIdentity Identity { get; } = identity;
            public GeneratedHints Hints { get; } = hints;
            public GeneratedOrigin[] SourceOrigins { get; } = sourceOrigins;
        }

        private sealed class GeneratedIdentity(
            string componentId,
            string moduleId,
            string descriptorHash,
            string templateHash,
            string logicHash,
            GeneratedHmrBoundaryKind hmrBoundaryKind)
        {
            public string ComponentId { get; } = componentId;
            public string ModuleId { get; } = moduleId;
            public string DescriptorHash { get; } = descriptorHash;
            public string TemplateHash { get; } = templateHash;
            public string LogicHash { get; } = logicHash;
            public GeneratedHmrBoundaryKind HmrBoundaryKind { get; } = hmrBoundaryKind;
        }

        private sealed class GeneratedHints(
            bool requiresVueRuntime,
            bool requiresHydration,
            bool supportsSsr,
            bool usesTeleport,
            bool usesSuspense,
            bool usesKeepAlive)
        {
            public bool RequiresVueRuntime { get; } = requiresVueRuntime;
            public bool RequiresHydration { get; } = requiresHydration;
            public bool SupportsSsr { get; } = supportsSsr;
            public bool UsesTeleport { get; } = usesTeleport;
            public bool UsesSuspense { get; } = usesSuspense;
            public bool UsesKeepAlive { get; } = usesKeepAlive;
        }

        private sealed class GeneratedOrigin(
            string sourceFilePath,
            int sourceSpanStart,
            int sourceSpanLength,
            string? generatedFilePath,
            int? generatedSpanStart,
            int? generatedSpanLength,
            int startLine,
            int startColumn,
            GeneratedMappingQuality mappingQuality,
            GeneratedOriginProvenance provenance)
        {
            public string SourceFilePath { get; } = sourceFilePath;
            public int SourceSpanStart { get; } = sourceSpanStart;
            public int SourceSpanLength { get; } = sourceSpanLength;
            public string? GeneratedFilePath { get; } = generatedFilePath;
            public int? GeneratedSpanStart { get; } = generatedSpanStart;
            public int? GeneratedSpanLength { get; } = generatedSpanLength;
            public int StartLine { get; } = startLine;
            public int StartColumn { get; } = startColumn;
            public GeneratedMappingQuality MappingQuality { get; } = mappingQuality;
            public GeneratedOriginProvenance Provenance { get; } = provenance;
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
