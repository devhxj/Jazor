using System.Text;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class FrontendToolchainRunnerTests
{
    [TestMethod]
    public void TryParse_BuildCommand_CreatesExplicitDenoProductionRequest()
    {
        var parsed = FrontendToolchainCommand.TryParse(
            [
                "build",
                "--toolchain", "Deno",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out var command,
            out var error);

        Assert.IsTrue(parsed, error);
        Assert.IsNotNull(command);
        Assert.AreEqual(FrontendBuildMode.Production, command.Mode);
        Assert.AreEqual(FrontendToolchainKind.Deno, command.Request.Toolchain);
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.ProductionBuild));
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.SourceMaps));
        Assert.IsFalse(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.Hmr));
    }

    [TestMethod]
    public void TryParse_ServeCommand_CreatesDevelopmentRequestWithHmrCapability()
    {
        var parsed = FrontendToolchainCommand.TryParse(
            [
                "serve",
                "--toolchain", "Netpack",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out var command,
            out var error);

        Assert.IsTrue(parsed, error);
        Assert.IsNotNull(command);
        Assert.AreEqual(FrontendBuildMode.Development, command.Mode);
        Assert.AreEqual(FrontendToolchainKind.Netpack, command.Request.Toolchain);
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.DevelopmentServer));
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.Hmr));
    }

    [TestMethod]
    public void TryParse_RejectsMissingExplicitArtifactRoot()
    {
        var parsed = FrontendToolchainCommand.TryParse(
            [
                "build",
                "--toolchain", "Deno",
                "--manifest", "manifest.json",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out _,
            out var error);

        Assert.IsFalse(parsed);
        Assert.AreEqual("Missing required argument --artifacts.", error);
    }

    [TestMethod]
    public void Create_NormalizesExplicitToolchainContractPaths()
    {
        using var workspace = new TestWorkspace();

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        Assert.AreEqual(Path.GetFullPath(workspace.ManifestPath), request.ManifestPath);
        Assert.AreEqual(Path.GetFullPath(workspace.ArtifactRoot), request.ArtifactRoot);
        Assert.AreEqual(Path.GetFullPath(workspace.SourceRoot), request.SourceRoot);
        Assert.AreEqual(Path.GetFullPath(workspace.OutputRoot), request.OutputRoot);
        Assert.AreEqual(Path.Combine(request.OutputRoot, "bundle.js"), request.BundleOutputPath);
        CollectionAssert.AreEquivalent(
            new[]
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            },
            request.RequiredCapabilities.ToArray());
    }

    [TestMethod]
    public async Task BuildAsync_DenoProduction_ConsumesUnifiedRequestAndWritesBundle()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            export function Boot() {
              return "ready";
            }
            """);
        WriteManifest(workspace, "host/app.mjs");

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.AreEqual(FrontendToolchainKind.Deno, result.Toolchain);
        Assert.AreEqual(request.BundleOutputPath, result.OutputPath);
        Assert.AreEqual(1, result.ModuleCount);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("function Boot()", script);
        Assert.Contains("export {", script);
    }

    [TestMethod]
    public async Task BuildAsync_DenoProduction_UsesExplicitSourceRootForRegisteredVueSfcAsset()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import LocalCard from "./LocalCard.vue";

            export const ComponentName = LocalCard.name;
            export default LocalCard;
            """);
        WriteModule(workspace.SourceRoot, "components/LocalCard.vue",
            """
            <template>
              <section>Toolchain SFC</section>
            </template>

            <script>
            export default {
              name: "ToolchainLocalCard"
            };
            </script>
            """);
        WriteManifest(
            workspace,
            "host/app.mjs",
            [
                new ManifestAssetEntry(
                    "components/LocalCard.vue",
                    "host/LocalCard.vue",
                    ManifestAssetEntry.KindVueSfc,
                    "hash-asset-1")
            ]);

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("ToolchainLocalCard", script);
        Assert.Contains("Toolchain SFC", script);
        Assert.DoesNotContain("./LocalCard.vue", script);
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_ReturnsTypedUnsupportedWithoutDenoFallback()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            export const Value = 42;
            """);
        WriteManifest(workspace, "host/app.mjs");

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Netpack,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FrontendToolchainKind.Netpack, result.Toolchain);
        Assert.AreEqual("JAZOR_TOOLCHAIN_NETPACK_NOT_IMPLEMENTED", result.Diagnostic?.Code);
        Assert.IsFalse(File.Exists(request.BundleOutputPath));
    }

    [TestMethod]
    public async Task BuildAsync_MissingArtifactRoot_ReturnsTypedContractDiagnostic()
    {
        using var workspace = new TestWorkspace();
        WriteManifest(workspace, "host/app.mjs");
        var manifestPath = Path.Combine(workspace.RootPath, "jazor-manifest.json");
        File.Copy(workspace.ManifestPath, manifestPath);
        Directory.Delete(workspace.ArtifactRoot, recursive: true);

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            manifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot);

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FrontendToolchainKind.Deno, result.Toolchain);
        Assert.AreEqual("JAZOR_TOOLCHAIN_ARTIFACT_ROOT_NOT_FOUND", result.Diagnostic?.Code);
        Assert.Contains(workspace.ArtifactRoot, result.Diagnostic?.Message ?? string.Empty);
    }

    private static void WriteManifest(
        TestWorkspace workspace,
        string relativePath,
        IReadOnlyList<ManifestAssetEntry>? assets = null)
    {
        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", relativePath, "hash-1")
            ]);
        if (assets is not null)
            manifest.Assets.AddRange(assets);

        manifest.Save(workspace.ManifestPath);
    }

    private static void WriteModule(string rootDirectory, string relativePath, string content)
    {
        var fullPath = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content.ReplaceLineEndings("\n"), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            ArtifactRoot = Path.Combine(RootPath, "artifacts");
            SourceRoot = Path.Combine(RootPath, "src");
            OutputRoot = Path.Combine(RootPath, "dist");
            ManifestPath = Path.Combine(ArtifactRoot, "jazor-manifest.json");
            Directory.CreateDirectory(ArtifactRoot);
            Directory.CreateDirectory(SourceRoot);
        }

        public string RootPath { get; }

        public string ArtifactRoot { get; }

        public string SourceRoot { get; }

        public string OutputRoot { get; }

        public string ManifestPath { get; }

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
