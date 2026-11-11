using Jazor.NodeHost;
using System.Text.Json;
using Jazor.Common;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class StandardPackageProjectTests
{
    [TestMethod]
    public async Task ProjectBuild_UsesAuthoredViteConfigAndOutputLayout_AndPropagatesFailure()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.ProjectBuildTest", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "package.json"), """
                {"type":"module","devDependencies":{"vite":"8.3.0"},"scripts":{"build":"vite build"}}
                """);
            File.WriteAllText(Path.Combine(root, "app.js"), "import './app.css'; export const built = true;");
            File.WriteAllText(Path.Combine(root, "app.css"), ".probe { color: rgb(255, 0, 0); padding: 0px 0px 0px 0px; }");
            File.WriteAllText(Path.Combine(root, "vite.config.js"), """
                export default { build: { outDir: 'custom-output', rolldownOptions: {
                  input: 'app.js', preserveEntrySignatures: 'strict', output: { entryFileNames: 'custom-output.js' }
                } } };
                """);
            await NodeToolchain.RestoreAsync(root, Path.Combine(RepositoryTemp.Root, "nodeapi-store"), CancellationToken.None);
            var builder = new JavaScriptProjectBuilder();
            var built = await builder.BuildAsync(root);
            Assert.IsTrue(built.IsSuccess, built.Diagnostic?.Message);
            Assert.IsTrue(File.Exists(Path.Combine(root, "custom-output", "custom-output.js")));
            var cssFiles = Directory.GetFiles(Path.Combine(root, "custom-output"), "*.css", SearchOption.AllDirectories);
            Assert.HasCount(1, cssFiles);
            StringAssert.Contains(File.ReadAllText(cssFiles[0]), "padding:0");
            using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "package.json")));
            Assert.AreEqual("npm:lightningcss-wasm@1.33.0",
                manifest.RootElement.GetProperty("pnpm").GetProperty("overrides").GetProperty("vite>lightningcss").GetString());
            Assert.IsFalse(Directory.Exists(Path.Combine(root, "dist")));
            File.WriteAllText(Path.Combine(root, "vite.config.js"), "throw new Error('authored build failed');");
            using var progress = new StringWriter();
            var failed = await builder.BuildAsync(root, progress: progress);
            Assert.IsFalse(failed.IsSuccess);
            Assert.AreEqual(1, failed.ExitCode);
            StringAssert.Contains(failed.Diagnostic!.Message, "authored build failed");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void Regeneration_PreservesAuthoredBuildToolAndUnmanagedDependencies()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "package.json"), """
                {"name":"custom-project","type":"module",
                 "scripts":{"build":"custom-tool build","dev":"custom-tool serve","ssr":"deno run custom-server.js"},
                 "devDependencies":{"custom-tool":"1.0.0"},
                 "dependencies":{"authored":"2.0.0","retired":"1.0.0"},
                 "jazor":{"packages":{"retired":{"source":"npm","version":"1.0.0"}}}}
                """);
            var libraries = new LibraryAssets(
                new Dictionary<string, string>(), new Dictionary<string, string>(),
                [], [], [], [], [], [],
                new Dictionary<string, LibraryPackageReference> { ["generated"] = new("generated", "3.0.0", "npm") });
            LibraryPackageWriter.WritePackageProject(root, libraries, hasSsrEntry: true);
            LibraryPackageWriter.WritePackageProject(root, libraries, hasSsrEntry: true);
            using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "package.json")));
            Assert.AreEqual("custom-project", package.RootElement.GetProperty("name").GetString());
            Assert.AreEqual("custom-tool build", package.RootElement.GetProperty("scripts").GetProperty("build").GetString());
            Assert.AreEqual("deno run custom-server.js", package.RootElement.GetProperty("scripts").GetProperty("ssr").GetString());
            Assert.AreEqual("1.0.0", package.RootElement.GetProperty("devDependencies").GetProperty("custom-tool").GetString());
            Assert.IsFalse(package.RootElement.GetProperty("devDependencies").TryGetProperty("vite", out _));
            Assert.AreEqual("2.0.0", package.RootElement.GetProperty("dependencies").GetProperty("authored").GetString());
            Assert.AreEqual("3.0.0", package.RootElement.GetProperty("dependencies").GetProperty("generated").GetString());
            Assert.IsFalse(package.RootElement.GetProperty("dependencies").TryGetProperty("retired", out _));
            Assert.IsFalse(File.Exists(Path.Combine(root, "vite.config.js")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task NpmAlias_RestoreProjectsAuthoredPackageNameForNodeApiAndVite()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitTest", "npm-alias", Guid.NewGuid().ToString("N"));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Directory.CreateDirectory(root);
        try
        {
            var appPath = Path.Combine(root, "app.js");
            await File.WriteAllTextAsync(
                appPath,
                "import { placements } from '@popperjs/core'; console.log(placements.includes('bottom'));",
                timeout.Token);
            var libraries = new LibraryAssets(
                new Dictionary<string, string>(), new Dictionary<string, string>(),
                [], [], [], [], [], [],
                new Dictionary<string, LibraryPackageReference>
                {
                    ["@popperjs/core"] = new(
                        "@popperjs/core",
                        "npm:@sxzz/popperjs-es@2.11.8",
                        "npm")
                });
            LibraryPackageWriter.WritePackageProject(root, libraries);
            var entries = ProjectEntryWriter.Write(root, ["app.js"]);

            await NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store"));

            var aliasRoot = Path.Combine(root, "node_modules", "@popperjs", "core");
            Assert.IsTrue(File.Exists(Path.Combine(aliasRoot, "package.json")));
            using (var aliasPackage = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(aliasRoot, "package.json"), timeout.Token)))
                Assert.AreEqual("@sxzz/popperjs-es", aliasPackage.RootElement.GetProperty("name").GetString());
            Assert.AreEqual("true", await RunNodeAsync(root, "entry.js", timeout.Token));

            // A source-only HMR update must preserve both the installed graph and alias projection.
            // Hold a package file against writes to model Windows Vite/native-module ownership.
            using (var loadedPackage = new FileStream(Path.Combine(aliasRoot, "package.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await File.WriteAllTextAsync(appPath, "import { placements } from '@popperjs/core'; console.log(placements.includes('top'));", timeout.Token);
                using var incrementalProgress = new StringWriter();
                await NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, incrementalProgress, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store"));
                StringAssert.Contains(incrementalProgress.ToString(), "\"status\":\"skipped\"");
                StringAssert.Contains(incrementalProgress.ToString(), "\"operation\":\"check\"");
            }
            Assert.AreEqual("true", await RunNodeAsync(root, "entry.js", timeout.Token));

            // A successful restore receipt cannot mask a new authored-module failure.
            await File.WriteAllTextAsync(appPath, "import './missing-authored-module.js';", timeout.Token);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store")));
            await File.WriteAllTextAsync(appPath, "import { placements } from '@popperjs/core'; console.log(placements.includes('bottom'));", timeout.Token);

            Directory.Delete(Path.Combine(root, "node_modules"), recursive: true);
            using var recoveredProgress = new StringWriter();
            await NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, recoveredProgress, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store"));
            StringAssert.Contains(recoveredProgress.ToString(), "\"status\":\"completed\"");
            Assert.IsTrue(File.Exists(Path.Combine(aliasRoot, "package.json")));

            var result = await new JavaScriptProjectBuilder().BuildAsync(root, cancellationToken: timeout.Token);
            Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message);
            Assert.AreEqual("true", await RunNodeAsync(root, "dist/bundle.js", timeout.Token));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task VueBindingPeerClosures_RestorePinnedVueForNodeApiAndVite()
    {
        var cases = new[]
        {
            (
                Manifest: "ECMAScript.FilePond",
                Specifier: "vue-filepond",
                Source: "import vueFilePond from 'vue-filepond'; console.log(Boolean(vueFilePond));"),
            (
                Manifest: "ECMAScript.FloatingUi",
                Specifier: "@floating-ui/vue",
                Source: "import { useFloating } from '@floating-ui/vue'; console.log(Boolean(useFloating));"),
            (
                Manifest: "ECMAScript.Lucide",
                Specifier: "lucide-vue-next",
                Source: "import { Circle } from 'lucide-vue-next'; console.log(Boolean(Circle));")
        };
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));

        foreach (var testCase in cases)
        {
            var root = Path.Combine(
                RepositoryTemp.Root,
                "Jazor.EmitTest",
                "binding-peer",
                testCase.Manifest,
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var appPath = Path.Combine(root, "app.js");
                await File.WriteAllTextAsync(appPath, testCase.Source, timeout.Token);
                var libraries = new LibraryMaterializer().Materialize(
                    [FindLibraryManifest(testCase.Manifest), FindLibraryManifest("ECMAScript.Vue")],
                    root,
                    BuildMode.Production,
                    [testCase.Specifier]);
                LibraryPackageWriter.WritePackageProject(root, libraries);
                var entries = ProjectEntryWriter.Write(root, ["app.js"]);

                await NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store"));

                using (var vuePackage = JsonDocument.Parse(await File.ReadAllTextAsync(
                           Path.Combine(root, "node_modules", "vue", "package.json"),
                           timeout.Token)))
                {
                    Assert.AreEqual(
                        "3.5.42",
                        vuePackage.RootElement.GetProperty("version").GetString(),
                        $"{testCase.Manifest} restored an implicit Vue peer instead of the declared identity.");
                }

                var result = await new JavaScriptProjectBuilder().BuildAsync(root, cancellationToken: timeout.Token);
                Assert.IsTrue(result.IsSuccess, $"{testCase.Manifest}: {result.Diagnostic?.Message}");
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task NpmAndJsr_DeclaredKeysResolveThroughNodeApiAndViteWithoutPrivateManifests()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitTest", "standard-project", Guid.NewGuid().ToString("N"));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Directory.CreateDirectory(root);
        try
        {
            const string source = """
                import { addDays } from "date-fns/addDays";
                import { join } from "@jsr/std__path/posix";
                console.log(join("npm", String(addDays(new Date(2020, 0, 1), 2).getDate())));
                """;
            var appPath = Path.Combine(root, "app.js");
            await File.WriteAllTextAsync(appPath, source, timeout.Token);
            var libraries = new LibraryAssets(
                new Dictionary<string, string>(), new Dictionary<string, string>(),
                [], [], [], [], [], [],
                new Dictionary<string, LibraryPackageReference>
                {
                    ["date-fns"] = new("date-fns", "4.1.0", "npm"),
                    ["@jsr/std__path"] = new("@jsr/std__path", "jsr:@std/path@1.0.8", "jsr")
                });
            LibraryPackageWriter.WritePackageProject(root, libraries);
            var entries = ProjectEntryWriter.Write(root, ["app.js"]);
            using var package = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "package.json"), timeout.Token));
            var dependencies = package.RootElement.GetProperty("dependencies");
            Assert.AreEqual("npm:@jsr/std__path@1.0.8", dependencies.GetProperty("@jsr/std__path").GetString());
            Assert.AreEqual("4.1.0", dependencies.GetProperty("date-fns").GetString());

            await NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store"));
            var lockPath = Path.Combine(root, "pnpm-lock.yaml");
            var locked = await File.ReadAllTextAsync(lockPath, timeout.Token);
            using var repeatedProgress = new StringWriter();
            await NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, repeatedProgress, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store"));
            StringAssert.Contains(repeatedProgress.ToString(), "\"status\":\"skipped\"");
            Assert.AreEqual(locked, await File.ReadAllTextAsync(lockPath, timeout.Token));
            Assert.AreEqual("npm/3", await RunNodeAsync(root, "entry.js", timeout.Token));

            var result = await new JavaScriptProjectBuilder().BuildAsync(root, cancellationToken: timeout.Token);
            Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message);
            Assert.AreEqual("npm/3", await RunNodeAsync(root, "dist/bundle.js", timeout.Token));

            // Enabling SSR later must not require Emit to overwrite the authored Vite config.
            var config = await File.ReadAllTextAsync(Path.Combine(root, "vite.config.js"), timeout.Token);
            await File.WriteAllTextAsync(Path.Combine(root, "hydration.js"), "export const hydrate = () => 'ready';", timeout.Token);
            LibraryPackageWriter.WritePackageProject(root, libraries, hasSsrEntry: true);
            Assert.AreEqual(config, await File.ReadAllTextAsync(Path.Combine(root, "vite.config.js"), timeout.Token));
            var withHydration = await new JavaScriptProjectBuilder().BuildAsync(root, cancellationToken: timeout.Token);
            Assert.IsTrue(withHydration.IsSuccess, withHydration.Diagnostic?.Message);
            Assert.IsTrue(File.Exists(Path.Combine(root, "dist", "hydration.js")));

            // A patch version is a new identity, even when its JSR package and major are unchanged.
            var upgradedReferences = libraries.PackageReferences.ToDictionary(pair => pair.Key, pair => pair.Value);
            upgradedReferences["@jsr/std__path"] = new("@jsr/std__path", "jsr:@std/path@1.0.9", "jsr");
            var upgradedLibraries = libraries with { PackageReferences = upgradedReferences };
            LibraryPackageWriter.WritePackageProject(root, upgradedLibraries);
            using var upgradedProgress = new StringWriter();
            await NodePackageRestorer.RestoreAndCheckAsync(root, entries, timeout.Token, upgradedProgress, storeDirectory: Path.Combine(RepositoryTemp.Root, "nodeapi-store"));
            StringAssert.Contains(upgradedProgress.ToString(), "\"status\":\"completed\"");
            var upgradedLock = await File.ReadAllTextAsync(lockPath, timeout.Token);
            Assert.AreNotEqual(locked, upgradedLock);
            StringAssert.Contains(upgradedLock, "@jsr/std__path@1.0.9");
            Assert.AreEqual("npm/3", await RunNodeAsync(root, "entry.js", timeout.Token));
            Assert.IsFalse(File.Exists(Path.Combine(root, "importmap.json")));
            Assert.IsFalse(File.Exists(Path.Combine(root, "jazor-manifest.json")));
            Assert.IsFalse(File.Exists(Path.Combine(root, ".jazor-deno-check-importmap.json")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static Task<string> RunNodeAsync(string root, string entry, CancellationToken cancellationToken)
        => NodeModuleRunner.RunAsync(root, Path.Combine(root, entry), cancellationToken: cancellationToken);
    private static string FindLibraryManifest(string projectName)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", projectName, "manifest.json");
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Could not locate the manifest for '{projectName}'.");
    }
}
