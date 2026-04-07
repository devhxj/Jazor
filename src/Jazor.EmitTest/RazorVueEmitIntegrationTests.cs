using Jazor.Emit;
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
        var manifestPath = RazorVueModuleWriter.GetManifestPath(Path.Combine(outputDirectory, "jazor-manifest.json"));
        var sourceFilePath = Path.Combine(root, "Counter.razor");
        var modulePath = Path.Combine(outputDirectory, "components", "counter-card.mjs");
        var mapPath = modulePath + ".map";

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
            Assert.AreEqual(1, result.Written);
            Assert.IsTrue(File.Exists(modulePath));
            Assert.IsTrue(File.Exists(mapPath));
            Assert.IsTrue(File.Exists(manifestPath));

            var moduleCode = File.ReadAllText(modulePath);
            StringAssert.Contains(moduleCode, "//# sourceMappingURL=counter-card.mjs.map");

            using var map = JsonDocument.Parse(File.ReadAllText(mapPath));
            Assert.AreEqual("components/counter-card.mjs", map.RootElement.GetProperty("file").GetString());
            Assert.AreEqual(sourceFilePath, map.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual("Counter component source", map.RootElement.GetProperty("sourcesContent")[0].GetString());
            Assert.AreNotEqual(string.Empty, map.RootElement.GetProperty("mappings").GetString());

            var manifest = RazorVueManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("Demo.Components", manifest.Modules[0].AssemblyName);
            Assert.AreEqual("CounterCard", manifest.Modules[0].ComponentName);
            Assert.AreEqual("components/counter-card.mjs", manifest.Modules[0].RelativeModulePath);
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
        var manifestPath = RazorVueModuleWriter.GetManifestPath(Path.Combine(outputDirectory, "jazor-manifest.json"));

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

            var manifest = RazorVueManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.AreEqual("Demo.Host", manifest.AssemblyName);
            Assert.HasCount(2, manifest.Modules);
            CollectionAssert.AreEquivalent(
                new[] { "Demo.Components", "Demo.Widgets" },
                manifest.Modules.Select(static module => module.AssemblyName).ToArray());
            CollectionAssert.AreEqual(new[] { "feature-flags", "vuetify" }, manifest.PluginRequirements);
            CollectionAssert.AreEqual(new[] { "feature/flags.css", "vuetify/styles" }, manifest.Styles);
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
        var razorVueManifestPath = RazorVueModuleWriter.GetManifestPath(manifestPath);
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
            Assert.IsTrue(File.Exists(razorVueManifestPath));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter-card.mjs.map")));

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
            Assert.IsTrue(File.Exists(razorVueManifestPath));

            using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(razorVueManifestPath));
            Assert.IsFalse(manifest.RootElement.GetProperty("Modules").EnumerateArray().Any());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

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
