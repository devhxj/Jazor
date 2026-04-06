using Jazor.Emit;

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
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
            Assert.IsTrue(artifact.Hints.RequiresVueRuntime);
            Assert.IsTrue(artifact.Hints.SupportsSsr);
            Assert.HasCount(1, artifact.SourceOrigins);
            Assert.AreEqual(RazorVueMappingQualityRecord.MappedFromGenerated, artifact.SourceOrigins[0].MappingQuality);
        }

        [TestMethod]
        public void RazorVueManifestModel_CreateSaveAndLoad_PreservesArtifactMetadata()
        {
            var catalog = RazorVueCatalogReader.TryRead(typeof(RazorVueCatalogReaderTests).Assembly);
            Assert.IsNotNull(catalog);

            var manifest = RazorVueManifestModel.Create(catalog);

            Assert.AreEqual("RazorVue.Reader.Tests", manifest.AssemblyName);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("RazorVue.Reader.Tests", manifest.Modules[0].AssemblyName);
            Assert.AreEqual("CounterCard", manifest.Modules[0].ComponentName);
            Assert.AreEqual("components/counter-card.mjs", manifest.Modules[0].RelativeModulePath);
            Assert.AreEqual("descriptor-hash", manifest.Modules[0].DescriptorHash);
            Assert.AreEqual("template-hash", manifest.Modules[0].TemplateHash);
            Assert.AreEqual("logic-hash", manifest.Modules[0].LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, manifest.Modules[0].HmrBoundaryKind);
            Assert.IsTrue(manifest.Modules[0].SupportsSsr);
            Assert.IsFalse(string.IsNullOrWhiteSpace(manifest.Modules[0].ContentHash));

            var manifestPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"), "razorvue-manifest.json");
            try
            {
                manifest.Save(manifestPath);
                var loaded = RazorVueManifestModel.TryLoad(manifestPath);

                Assert.IsNotNull(loaded);
                Assert.AreEqual(manifest.AssemblyName, loaded.AssemblyName);
                Assert.HasCount(1, loaded.Modules);
                Assert.AreEqual(manifest.Modules[0].ContentHash, loaded.Modules[0].ContentHash);
                CollectionAssert.AreEquivalent(manifest.Modules[0].Imports, loaded.Modules[0].Imports);
                CollectionAssert.AreEquivalent(manifest.Modules[0].Styles, loaded.Modules[0].Styles);
            }
            finally
            {
                var directory = Path.GetDirectoryName(manifestPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
        }
    }
}

namespace Jazor.Generated
{
    public static partial class RazorVueCatalog
    {
        public static string AssemblyName { get; } = "RazorVue.Reader.Tests";

        public static System.Collections.IEnumerable GetArtifacts()
            => _artifacts;

        private static readonly GeneratedArtifact[] _artifacts =
        [
            new GeneratedArtifact(
                componentName: "CounterCard",
                relativeModulePath: "components/counter-card.mjs",
                moduleCode: "export default { name: \"CounterCard\" };",
                imports: ["vue", "./button.mjs"],
                styles: ["vuetify/styles"],
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
            GeneratedIdentity identity,
            GeneratedHints hints,
            GeneratedOrigin[] sourceOrigins)
        {
            public string ComponentName { get; } = componentName;
            public string RelativeModulePath { get; } = relativeModulePath;
            public string ModuleCode { get; } = moduleCode;
            public string[] Imports { get; } = imports;
            public string[] Styles { get; } = styles;
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
