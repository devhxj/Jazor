using Jazor.Emit;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Jazor.EmitTest;

[TestClass]
public sealed class RazorVueEmitIntegrationTests
{
    [TestMethod]
    public void ModuleCollector_Collect_ReadsRazorVueArtifacts_FromAssemblyCatalog()
    {
        var loadContext = new EmitLoadContext(typeof(RazorVueEmitIntegrationTests).Assembly.Location);
        var collector = new ModuleCollector(loadContext);
        collector.AddAssembly(typeof(RazorVueEmitIntegrationTests).Assembly.Location);

        var result = collector.Collect(failOnPathConflict: true);

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsEmpty(result.Modules);
        Assert.AreEqual(1, result.RazorVueCatalogCount);
        Assert.HasCount(1, result.RazorVueCatalogs);
        Assert.HasCount(1, result.RazorVueArtifacts);
        Assert.AreEqual("RazorVue.Reader.Tests", result.RazorVueCatalogs[0].AssemblyName);
        Assert.AreEqual("components/counter-card.mjs", result.RazorVueArtifacts[0].RelativeModulePath);
    }

    [TestMethod]
    public void RazorVueModuleWriter_WritesArtifactsAndManifest()
    {
        var writer = new RazorVueModuleWriter();
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var sourceFilePath = Path.Combine(root, "Counter.razor");
        var hostRequirementsModulePath = RazorVueModuleWriter.GetHostRequirementsModulePath(outputDirectory);
        var modulePath = Path.Combine(outputDirectory, "components", "counter-card.mjs");
        var mapPath = modulePath + ".map";
        var originMapPath = modulePath + ".origins.json";

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(sourceFilePath, "Counter component source");

            var result = writer.Write(
                rootAssemblyPath: Path.Combine(root, "Demo.Host.dll"),
                outputDirectory,
                manifestPath,
                [
                    new RazorVueCatalogRecord(
                        "Demo.Components",
                        [
                            new RazorVueEmitArtifactRecord(
                                "CounterCard",
                                "components/counter-card.mjs",
                                "export default { name: \"CounterCard\" };",
                                ["/", "/counter"],
                                ["vue"],
                                ["vuetify/styles"],
                                ["vuetify"],
                                new RazorVueEmitArtifactIdentity(
                                    "Demo.Components.CounterCard",
                                    "components/counter-card.mjs",
                                    "descriptor-hash",
                                    "template-hash",
                                    "logic-hash",
                                    RazorVueHmrBoundaryKind.LogicSafe),
                                new RazorVueEmitRuntimeHints(true, false, true, false, false, false),
                                [
                                    new RazorVueEmitSourceOriginRecord(
                                        sourceFilePath,
                                        12,
                                        8,
                                        "components/counter-card.mjs",
                                        0,
                                        38,
                                        2,
                                        4,
                                        RazorVueMappingQualityRecord.MappedFromGenerated,
                                        RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
                                ])
                        ])
                ],
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            Assert.AreEqual(2, result.Written);
            Assert.IsTrue(File.Exists(modulePath));
            Assert.IsTrue(File.Exists(mapPath));
            Assert.IsTrue(File.Exists(originMapPath));
            Assert.IsTrue(File.Exists(manifestPath));
            Assert.IsTrue(File.Exists(hostRequirementsModulePath));

            var moduleCode = File.ReadAllText(modulePath);
            StringAssert.Contains(moduleCode, "//# sourceMappingURL=counter-card.mjs.map");
            var hostRequirementsCode = File.ReadAllText(hostRequirementsModulePath);
            StringAssert.Contains(hostRequirementsCode, "export const razorVueHostAssemblyName = \"Demo.Components\";");
            StringAssert.Contains(hostRequirementsCode, "export const razorVueHostModules = Object.freeze([");
            StringAssert.Contains(hostRequirementsCode, "\"assemblyName\":\"Demo.Components\"");
            StringAssert.Contains(hostRequirementsCode, "\"componentId\":\"Demo.Components.CounterCard\"");
            StringAssert.Contains(hostRequirementsCode, "\"moduleId\":\"components/counter-card.mjs\"");
            StringAssert.Contains(hostRequirementsCode, "export const razorVueStyles = Object.freeze([\"vuetify/styles\"]);");
            StringAssert.Contains(hostRequirementsCode, "export const razorVuePluginRequirements = Object.freeze([\"vuetify\"]);");
            StringAssert.Contains(hostRequirementsCode, "\"componentName\":\"CounterCard\"");
            StringAssert.Contains(hostRequirementsCode, "\"relativeModulePath\":\"components/counter-card.mjs\"");
            StringAssert.Contains(hostRequirementsCode, "\"sourceMapPath\":\"components/counter-card.mjs.map\"");
            StringAssert.Contains(hostRequirementsCode, "\"originMapPath\":\"components/counter-card.mjs.origins.json\"");
            StringAssert.Contains(hostRequirementsCode, "\"descriptorHash\":\"descriptor-hash\"");
            StringAssert.Contains(hostRequirementsCode, "\"hmrBoundaryKind\":2");

            using var originMap = JsonDocument.Parse(File.ReadAllText(originMapPath));
            Assert.AreEqual("Demo.Components.CounterCard", originMap.RootElement.GetProperty("componentId").GetString());
            Assert.AreEqual("components/counter-card.mjs", originMap.RootElement.GetProperty("moduleId").GetString());
            Assert.AreEqual(sourceFilePath, originMap.RootElement.GetProperty("origins")[0].GetProperty("sourceFilePath").GetString());

            using var map = JsonDocument.Parse(File.ReadAllText(mapPath));
            Assert.AreEqual("components/counter-card.mjs", map.RootElement.GetProperty("file").GetString());
            Assert.AreEqual(sourceFilePath, map.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual("Counter component source", map.RootElement.GetProperty("sourcesContent")[0].GetString());
            Assert.AreNotEqual(string.Empty, map.RootElement.GetProperty("mappings").GetString());

            using var manifestJson = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var manifestModule = manifestJson.RootElement.GetProperty("modules")[0];
            Assert.AreEqual("mjs", manifestModule.GetProperty("kind").GetString());
            Assert.AreEqual("components/counter-card.mjs", manifestModule.GetProperty("relativePath").GetString());
            var component = manifestModule.GetProperty("component");
            Assert.AreEqual("h", component.GetProperty("model").GetString());
            CollectionAssert.AreEqual(
                new[] { "vuetify/styles" },
                component.GetProperty("styles").EnumerateArray().Select(static item => item.GetString()).OfType<string>().ToArray());
            CollectionAssert.AreEqual(
                new[] { "vuetify" },
                component.GetProperty("pluginRequirements").EnumerateArray().Select(static item => item.GetString()).OfType<string>().ToArray());

            var manifest = RazorVueManifestSerializer.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("Demo.Components", manifest.Modules[0].AssemblyName);
            Assert.AreEqual("Demo.Components.CounterCard", manifest.Modules[0].ComponentId);
            Assert.AreEqual("components/counter-card.mjs", manifest.Modules[0].ModuleId);
            Assert.AreEqual("CounterCard", manifest.Modules[0].ComponentName);
            Assert.AreEqual("components/counter-card.mjs", manifest.Modules[0].RelativeModulePath);
            Assert.AreEqual("components/counter-card.mjs.map", manifest.Modules[0].SourceMapPath);
            Assert.AreEqual("components/counter-card.mjs.origins.json", manifest.Modules[0].OriginMapPath);
            CollectionAssert.AreEqual(new[] { "vuetify/styles" }, manifest.Styles);
            CollectionAssert.AreEqual(new[] { "vuetify" }, manifest.Modules[0].PluginRequirements);
            CollectionAssert.AreEqual(new[] { "vuetify" }, manifest.PluginRequirements);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void RazorVueModuleWriter_WritesAggregateManifest_WithPerAssemblyOrigins()
    {
        var writer = new RazorVueModuleWriter();
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");

        try
        {
            var result = writer.Write(
                rootAssemblyPath: Path.Combine(root, "Demo.Host.dll"),
                outputDirectory,
                manifestPath,
                [
                    new RazorVueCatalogRecord(
                        "Demo.Components",
                        [
                            new RazorVueEmitArtifactRecord(
                                "CounterCard",
                                "components/counter-card.mjs",
                                "export default { name: \"CounterCard\" };",
                                ["/", "/counter"],
                                ["vue"],
                                ["vuetify/styles"],
                                ["vuetify"],
                                new RazorVueEmitArtifactIdentity(
                                    "Demo.Components.CounterCard",
                                    "components/counter-card.mjs",
                                    "descriptor-a",
                                    "template-a",
                                    "logic-a",
                                    RazorVueHmrBoundaryKind.LogicSafe),
                                new RazorVueEmitRuntimeHints(true, false, true, false, false, false),
                                [])
                        ]),
                    new RazorVueCatalogRecord(
                        "Demo.Widgets",
                        [
                            new RazorVueEmitArtifactRecord(
                                "StatusBadge",
                                "widgets/status-badge.mjs",
                                "export default { name: \"StatusBadge\" };",
                                ["/status"],
                                ["vue"],
                                ["feature/flags.css"],
                                ["feature-flags"],
                                new RazorVueEmitArtifactIdentity(
                                    "Demo.Widgets.StatusBadge",
                                    "widgets/status-badge.mjs",
                                    "descriptor-b",
                                    "template-b",
                                    "logic-b",
                                    RazorVueHmrBoundaryKind.TemplateOnly),
                                new RazorVueEmitRuntimeHints(true, false, false, false, false, false),
                                [])
                        ])
                ],
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

            var manifest = RazorVueManifestSerializer.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.AreEqual("Demo.Host", manifest.AssemblyName);
            Assert.HasCount(2, manifest.Modules);
            CollectionAssert.AreEquivalent(
                new[] { "Demo.Components", "Demo.Widgets" },
                manifest.Modules.Select(static module => module.AssemblyName).ToArray());
            CollectionAssert.AreEqual(new[] { "feature-flags", "vuetify" }, manifest.PluginRequirements);
            CollectionAssert.AreEqual(new[] { "feature/flags.css", "vuetify/styles" }, manifest.Styles);

            using var manifestJson = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var manifestModules = manifestJson.RootElement.GetProperty("modules")
                .EnumerateArray()
                .OrderBy(static module => module.GetProperty("relativePath").GetString(), StringComparer.Ordinal)
                .ToArray();
            CollectionAssert.AreEqual(
                new[] { "mjs", "mjs" },
                manifestModules.Select(static module => module.GetProperty("kind").GetString()).ToArray());
            CollectionAssert.AreEqual(
                new[] { "h", "h" },
                manifestModules.Select(static module => module.GetProperty("component").GetProperty("model").GetString()).ToArray());
            CollectionAssert.AreEqual(
                new[] { "feature/flags.css", "vuetify/styles" },
                manifestModules
                    .SelectMany(static module => module.GetProperty("component").GetProperty("styles").EnumerateArray())
                    .Select(static item => item.GetString())
                    .OfType<string>()
                    .OrderBy(static item => item, StringComparer.Ordinal)
                    .ToArray());
            CollectionAssert.AreEqual(
                new[] { "feature-flags", "vuetify" },
                manifestModules
                    .SelectMany(static module => module.GetProperty("component").GetProperty("pluginRequirements").EnumerateArray())
                    .Select(static item => item.GetString())
                    .OfType<string>()
                    .OrderBy(static item => item, StringComparer.Ordinal)
                    .ToArray());

            var hostRequirementsModulePath = RazorVueModuleWriter.GetHostRequirementsModulePath(outputDirectory);
            var hostRequirementsCode = File.ReadAllText(hostRequirementsModulePath);
            StringAssert.Contains(hostRequirementsCode, "\"componentName\":\"CounterCard\"");
            StringAssert.Contains(hostRequirementsCode, "\"componentName\":\"StatusBadge\"");
            StringAssert.Contains(hostRequirementsCode, "\"sourceMapPath\":\"widgets/status-badge.mjs.map\"");
            StringAssert.Contains(hostRequirementsCode, "\"originMapPath\":\"widgets/status-badge.mjs.origins.json\"");
            StringAssert.Contains(hostRequirementsCode, "\"templateHash\":\"template-b\"");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task EmitCli_Clean_RemovesStaleRazorVueOutputs_WhenNextRunHasNoRazorVueArtifacts()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var hostRequirementsModulePath = RazorVueModuleWriter.GetHostRequirementsModulePath(outputDirectory);
        var emitAssemblyPath = typeof(EmitOptions).Assembly.Location;
        var razorVueSourceAssemblyPath = typeof(RazorVueEmitIntegrationTests).Assembly.Location;
        var plainSourceAssemblyPath = emitAssemblyPath;

        try
        {
            Directory.CreateDirectory(root);

            var firstRun = await RunDotNetAsync(root,
                [
                    "exec",
                    emitAssemblyPath,
                    "--root",
                    razorVueSourceAssemblyPath,
                    "--out",
                    outputDirectory,
                    "--write-manifest",
                    manifestPath,
                    "--clean",
                    "true",
                    "--fail-on-path-conflict",
                    "true"
                ]);

            Assert.AreEqual(0, firstRun.ExitCode, firstRun.ToString());
            Assert.IsTrue(File.Exists(manifestPath));
            Assert.IsTrue(File.Exists(hostRequirementsModulePath));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs.map")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs.origins.json")));

            var secondRun = await RunDotNetAsync(root,
                [
                    "exec",
                    emitAssemblyPath,
                    "--root",
                    plainSourceAssemblyPath,
                    "--out",
                    outputDirectory,
                    "--write-manifest",
                    manifestPath,
                    "--clean",
                    "true",
                    "--fail-on-path-conflict",
                    "true"
                ]);

            Assert.AreEqual(0, secondRun.ExitCode, secondRun.ToString());
            Assert.IsFalse(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs")));
            Assert.IsFalse(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs.map")));
            Assert.IsFalse(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs.origins.json")));
            Assert.IsFalse(File.Exists(hostRequirementsModulePath));
            Assert.IsTrue(File.Exists(manifestPath));

            using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
            Assert.IsFalse(manifest.RootElement.GetProperty("modules").EnumerateArray().Any());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task EmitCli_RazorVueDiff_WritesUpdatePlan()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var emitAssemblyPath = typeof(EmitOptions).Assembly.Location;
        var previousManifestPath = Path.Combine(root, "previous-jazor-manifest.json");
        var currentManifestPath = Path.Combine(root, "current-jazor-manifest.json");
        var planPath = Path.Combine(root, "razorvue-update-plan.json");

        try
        {
            Directory.CreateDirectory(root);

            var rootAssemblyPath = Path.Combine(root, "Demo.Host.dll");
            SaveUnifiedManifest(
                previousManifestPath,
                rootAssemblyPath,
                CreateManifest("template-a", "logic-a", "content-a", RazorVueHmrBoundaryKind.TemplateOnly));
            SaveUnifiedManifest(
                currentManifestPath,
                rootAssemblyPath,
                CreateManifest("template-b", "logic-a", "content-b", RazorVueHmrBoundaryKind.TemplateOnly));

            var result = await RunDotNetAsync(root,
                [
                    "exec",
                    emitAssemblyPath,
                    "razorvue-diff",
                    "--previous",
                    previousManifestPath,
                    "--current",
                    currentManifestPath,
                    "--out",
                    planPath
                ]);

            Assert.AreEqual(0, result.ExitCode, result.ToString());
            Assert.IsTrue(File.Exists(planPath));

            using var plan = JsonDocument.Parse(await File.ReadAllTextAsync(planPath));
            Assert.AreEqual("TemplatePatch", plan.RootElement.GetProperty("Action").GetString());
            Assert.AreEqual("Demo.Host.ProfileForm", plan.RootElement.GetProperty("Modules")[0].GetProperty("ComponentId").GetString());
            Assert.AreEqual("TemplatePatch", plan.RootElement.GetProperty("Modules")[0].GetProperty("Action").GetString());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task EmitCli_RazorVueDiff_AcceptsLegacyWritePlanAlias()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var emitAssemblyPath = typeof(EmitOptions).Assembly.Location;
        var previousManifestPath = Path.Combine(root, "previous-jazor-manifest.json");
        var currentManifestPath = Path.Combine(root, "current-jazor-manifest.json");
        var planPath = Path.Combine(root, "razorvue-update-plan.json");

        try
        {
            Directory.CreateDirectory(root);

            var rootAssemblyPath = Path.Combine(root, "Demo.Host.dll");
            SaveUnifiedManifest(
                previousManifestPath,
                rootAssemblyPath,
                CreateManifest("template-a", "logic-a", "content-a", RazorVueHmrBoundaryKind.TemplateOnly));
            SaveUnifiedManifest(
                currentManifestPath,
                rootAssemblyPath,
                CreateManifest("template-b", "logic-a", "content-b", RazorVueHmrBoundaryKind.TemplateOnly));

            var result = await RunDotNetAsync(root,
                [
                    "exec",
                    emitAssemblyPath,
                    "razorvue-diff",
                    "--previous",
                    previousManifestPath,
                    "--current",
                    currentManifestPath,
                    "--write-plan",
                    planPath
                ]);

            Assert.AreEqual(0, result.ExitCode, result.ToString());
            Assert.IsTrue(File.Exists(planPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task EmitCli_Clean_WhenPlainModuleSupersedesLegacyRazorVuePath_PreservesNewPlainModule()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var emitAssemblyPath = typeof(EmitOptions).Assembly.Location;
        var plainAssemblyPath = Path.Combine(root, "Plain.Host.dll");
        var modulePath = Path.Combine(outputDirectory, "components", "counter-card.mjs");
        var mapPath = modulePath + ".map";
        var originPath = modulePath + ".origins.json";

        try
        {
            Directory.CreateDirectory(root);
            CreatePlainCatalogAssembly(plainAssemblyPath);
            Directory.CreateDirectory(Path.GetDirectoryName(modulePath)!);
            File.WriteAllText(modulePath, "export default { legacy: true };\n");
            File.WriteAllText(mapPath, "legacy-map");
            File.WriteAllText(originPath, "legacy-origin");

            new ManifestModel(
                Path.Combine(root, "Sample.Host.dll"),
                new DateTime(2026, 5, 14, 0, 0, 0, DateTimeKind.Utc),
                [
                    new ManifestModuleEntry(
                        "Sample.Host",
                        "Demo.Components.CounterCard",
                        "Demo.Components.CounterCard",
                        "components/counter-card.mjs",
                        "legacy-component-hash",
                        "components/counter-card.mjs.map",
                        "legacy-component-map-hash",
                        ManifestModuleKind.Mjs,
                        new ManifestComponentMetadata(
                            ManifestComponentModel.H,
                            "Demo.Components.CounterCard",
                            "components/counter-card.mjs",
                            "CounterCard",
                            ["/counter"],
                            "components/counter-card.mjs.origins.json",
                            ["vue"],
                            ["vuetify/styles"],
                            ["vuetify"],
                            "descriptor-hash",
                            "template-hash",
                            "logic-hash",
                            "legacy-component-hash",
                            RazorVueHmrBoundaryKind.LogicSafe,
                            RequiresHydration: false,
                            SupportsSsr: true))
                ]).Save(manifestPath);

            var result = await RunDotNetAsync(root,
                [
                    "exec",
                    emitAssemblyPath,
                    "--root",
                    plainAssemblyPath,
                    "--out",
                    outputDirectory,
                    "--write-manifest",
                    manifestPath,
                    "--clean",
                    "true",
                    "--fail-on-path-conflict",
                    "true"
                ]);

            Assert.AreEqual(0, result.ExitCode, result.ToString());
            Assert.IsTrue(File.Exists(modulePath), "Plain module output was deleted by a stale RazorVue clean pass after taking ownership of the path.");
            Assert.IsTrue(File.Exists(mapPath), "Plain module sourcemap should remain after ownership transfer.");
            Assert.IsFalse(File.Exists(originPath), "Legacy RazorVue origin sidecar must be removed when the path becomes a plain module.");

            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            var module = manifest.Modules.Single(static module => string.Equals(module.RelativePath, "components/counter-card.mjs", StringComparison.OrdinalIgnoreCase));
            Assert.IsNull(module.Component, "Unified manifest should describe the path as a plain module after ownership transfer.");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static void CreatePlainCatalogAssembly(string assemblyPath)
    {
        const string source =
            """
            namespace Jazor.Generated
            {
                internal static partial class ModuleCatalog
                {
                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly GeneratedModule[] _modules =
                    [
                        new GeneratedModule(
                            assemblyName: "Sample.Host",
                            typeName: "Demo.Modules.CounterCard",
                            id: "Demo.Modules.CounterCard",
                            relativePath: "components/counter-card.mjs",
                            content: "export const counter = 1;",
                            hash: "plain-hash")
                    ];

                    private sealed class GeneratedModule
                    {
                        public GeneratedModule(string assemblyName, string typeName, string id, string relativePath, string content, string hash)
                        {
                            AssemblyName = assemblyName;
                            TypeName = typeName;
                            Id = id;
                            RelativePath = relativePath;
                            Content = content;
                            Hash = hash;
                        }

                        public string AssemblyName { get; }
                        public string TypeName { get; }
                        public string Id { get; }
                        public string RelativePath { get; }
                        public string Content { get; }
                        public string Hash { get; }
                    }
                }

                internal static partial class ModuleSourceMapCatalog
                {
                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly GeneratedModuleSourceMap[] _modules =
                    [
                        new GeneratedModuleSourceMap(
                            id: "Demo.Modules.CounterCard",
                            sourceMapRelativePath: "components/counter-card.mjs.map",
                            sourceMapContent: "{\"version\":3,\"file\":\"components/counter-card.mjs\",\"sources\":[],\"names\":[],\"mappings\":\"\"}",
                            mapHash: "plain-map-hash")
                    ];

                    private sealed class GeneratedModuleSourceMap
                    {
                        public GeneratedModuleSourceMap(string id, string sourceMapRelativePath, string sourceMapContent, string mapHash)
                        {
                            Id = id;
                            SourceMapRelativePath = sourceMapRelativePath;
                            SourceMapContent = sourceMapContent;
                            MapHash = mapHash;
                        }

                        public string Id { get; }
                        public string SourceMapRelativePath { get; }
                        public string SourceMapContent { get; }
                        public string MapHash { get; }
                    }
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetFileNameWithoutExtension(assemblyPath),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(source, path: "Plain.Host.g.cs")
            ],
            references: Net110.References.All.Cast<MetadataReference>(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var directory = Path.GetDirectoryName(assemblyPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var stream = new FileStream(assemblyPath, FileMode.Create, FileAccess.Write, FileShare.None);
        var emitResult = compilation.Emit(stream);
        Assert.IsTrue(emitResult.Success, string.Join("\n", emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    private static RazorVueManifestModel CreateManifest(string templateHash, string logicHash, string contentHash, RazorVueHmrBoundaryKind boundaryKind)
        => new(
            "Demo.Host",
            new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
            [
                new RazorVueManifestEntry(
                    "Demo.Host",
                    "Demo.Host.ProfileForm",
                    "components/profile-form.mjs",
                    "ProfileForm",
                    ["/profile"],
                    "components/profile-form.mjs",
                    "components/profile-form.mjs.map",
                    "components/profile-form.mjs.origins.json",
                    ["vue"],
                    ["vuetify/styles"],
                    ["vuetify"],
                    "descriptor-hash",
                    templateHash,
                    logicHash,
                    contentHash,
                    boundaryKind,
                    false,
                    true)
            ],
            ["vuetify/styles"],
            ["vuetify"]);

    private static void SaveUnifiedManifest(
        string manifestPath,
        string rootAssemblyPath,
        RazorVueManifestModel razorVueManifest)
        => new ManifestModel(rootAssemblyPath, DateTime.UtcNow, [])
            .WithRazorVueManifest(razorVueManifest, ManifestComponentModel.H)
            .Save(manifestPath);

    private static async Task<ProcessResult> RunDotNetAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        startInfo.Environment["DOTNET_CLI_HOME"] = FindDotNetCliHome();
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            await standardOutput,
            await standardError);
    }

    private static string FindDotNetCliHome()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".dotnet");
            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository .dotnet directory.");
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"ExitCode: {ExitCode}");

            if (!string.IsNullOrWhiteSpace(StandardOutput))
            {
                builder.AppendLine("STDOUT:");
                builder.AppendLine(StandardOutput);
            }

            if (!string.IsNullOrWhiteSpace(StandardError))
            {
                builder.AppendLine("STDERR:");
                builder.AppendLine(StandardError);
            }

            return builder.ToString();
        }
    }
}
