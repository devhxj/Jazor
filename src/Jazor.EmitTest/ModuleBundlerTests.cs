using System.Text;
using System.Text.Json;
using Jazor.Emit;
using Jazor.Emit.SourceMaps;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ModuleBundlerTests
{
    [TestMethod]
    public async Task BundleAsync_SingleRootModule_PreservesExports()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "SdkSmoke/SampleModule.mjs",
            """
            export let Value = 42;
            export function Add(left, right) {
              return left + right;
            }
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "SdkSmoke.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("SdkSmoke", "SampleModule", "SampleModule", "SdkSmoke/SampleModule.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();

        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.OutputPath));

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.AreNotEqual(string.Empty, script);
        Assert.Contains("function Add(left, right)", script);
        Assert.Contains("var Value = 42;", script);
        Assert.Contains("export {", script);
        Assert.Contains("Add", script);
        Assert.Contains("Value", script);
    }

    [TestMethod]
    public async Task BundleAsync_MultiProjectHostBundle_ReExportsRootAssemblyMembers()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "shared/greetings.mjs",
            """
            export function Prefix() {
              return "Hello";
            }

            export function Compose(name) {
              return `${Prefix()}, ${name}`;
            }
            """);
        WriteModule(workspace.InputDirectory, "features/greeter.mjs",
            """
            import { Compose } from "shared/greetings.mjs";
            export function Greet(name) {
              return Compose(name);
            }
            """);
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            import { Greet } from "features/greeter.mjs";
            export function Boot() {
              return Greet("Jazor");
            }
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Contracts", "Sample.Contracts.GreetingSharedModule", "Sample.Contracts.GreetingSharedModule", "shared/greetings.mjs", "hash-1"),
                new ManifestModuleEntry("Sample.Features", "Sample.Features.GreeterModule", "Sample.Features.GreeterModule", "features/greeter.mjs", "hash-2"),
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-3")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();

        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.OutputPath));

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.AreNotEqual(string.Empty, script);
        Assert.Contains("function Prefix()", script);
        Assert.Contains("function Compose(name)", script);
        Assert.Contains("function Greet(name)", script);
        Assert.Contains("function Boot()", script);
        Assert.Contains("export {", script);
        Assert.Contains("Boot", script);
    }

    [TestMethod]
    public async Task BundleAsync_WritesExternalSourceMapAndSourceMappingUrl()
    {
        using var workspace = new TestWorkspace();
        WriteMappedModule(
            workspace.InputDirectory,
            "host/app.mjs",
            """
            export function Boot() {
              return "ready";
            }
            """,
            new SourceMapDocument(
                "host/app.mjs",
                [new SourceMapSource("Pages/App.razor", "<App />")],
                [
                    new SourceMapSegment(0, 0, 0, 0, 0),
                    new SourceMapSegment(1, 0, 0, 1, 0)
                ]));

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.OutputPath));
        Assert.IsTrue(File.Exists(workspace.OutputMapPath));

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("//# sourceMappingURL=bundle.js.map", script);

        using var map = await ReadJsonAsync(workspace.OutputMapPath);
        Assert.AreEqual("bundle.js", map.RootElement.GetProperty("file").GetString(), map.RootElement.ToString());
        Assert.IsTrue(map.RootElement.TryGetProperty("mappings", out var mappingsProperty));
        Assert.AreNotEqual(string.Empty, mappingsProperty.GetString());
    }

    [TestMethod]
    public async Task BundleAsync_ChainsBundleMapBackToOriginalSources()
    {
        using var workspace = new TestWorkspace();
        WriteMappedModule(
            workspace.InputDirectory,
            "host/app.mjs",
            """
            export const Value = 42;
            export function Boot() {
              return Value;
            }
            """,
            new SourceMapDocument(
                "host/app.mjs",
                [new SourceMapSource("Pages/App.razor", "<div>@Value</div>")],
                [
                    new SourceMapSegment(0, 0, 0, 4, 2),
                    new SourceMapSegment(1, 0, 0, 5, 0),
                    new SourceMapSegment(2, 0, 0, 6, 0)
                ]));

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        using var map = await ReadJsonAsync(workspace.OutputMapPath);
        var sources = map.RootElement.GetProperty("sources")
            .EnumerateArray()
            .Select(static item => item.GetString())
            .Where(static item => item is not null)
            .Cast<string>()
            .ToArray();
        var sourcesContent = map.RootElement.GetProperty("sourcesContent")
            .EnumerateArray()
            .Select(static item => item.GetString())
            .ToArray();

        CollectionAssert.AreEquivalent(new[] { "Pages/App.razor" }, sources, map.RootElement.ToString());
        CollectionAssert.DoesNotContain(sources, "host/app.mjs");
        Assert.IsTrue(sources.All(static source => !source.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)));
        CollectionAssert.Contains(sourcesContent, "<div>@Value</div>");
    }

    private static void WriteModule(string rootDirectory, string relativePath, string content)
    {
        var fullPath = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content.ReplaceLineEndings("\n"), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void WriteMappedModule(string rootDirectory, string relativePath, string content, SourceMapDocument map)
    {
        var writer = new SourceMapWriter();
        var fullPath = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var mapPath = fullPath + ".map";
        var script = writer.AppendSourceMappingUrl(content.ReplaceLineEndings("\n"), Path.GetFileName(mapPath));
        WriteModule(rootDirectory, relativePath, script);
        File.WriteAllText(mapPath, writer.Write(map), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static async Task<JsonDocument> ReadJsonAsync(string path)
        => JsonDocument.Parse(await File.ReadAllTextAsync(path, CancellationToken.None));

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            InputDirectory = Path.Combine(RootPath, "modules");
            ManifestPath = Path.Combine(InputDirectory, "jazor-manifest.json");
            OutputPath = Path.Combine(RootPath, "bundle.js");
            OutputMapPath = Path.Combine(RootPath, "bundle.js.map");
            Directory.CreateDirectory(InputDirectory);
        }

        public string RootPath { get; }

        public string InputDirectory { get; }

        public string ManifestPath { get; }

        public string OutputPath { get; }

        public string OutputMapPath { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(RootPath))
                    Directory.Delete(RootPath, recursive: true);
            }
            catch
            {
            }
        }
    }

    public TestContext TestContext { get; set; }
}
