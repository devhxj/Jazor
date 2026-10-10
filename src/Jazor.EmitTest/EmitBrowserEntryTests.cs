using System.Runtime.InteropServices;
using Basic.Reference.Assemblies;
using Jazor.Common;
using Jazor.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.EmitTest;

[TestClass]
public sealed class EmitBrowserEntryTests
{
    [TestMethod]
    public void ModuleWriter_ExplicitBrowserEntries_AreCanonicalAndLeaveAllModulesMaterialized()
    {
        using var workspace = new Workspace();
        var rootAssembly = Path.Combine(workspace.Root, "Host.dll");
        var modules = new[]
        {
            CreateModule(rootAssembly, "Host", "app.js", "export const app = true;"),
            CreateModule(rootAssembly, "Host", "pages/editor.js", "export const editor = true;"),
            CreateModule(rootAssembly, "Library", "library.js", "export const library = true;")
        };
        var manifestPath = Path.Combine(workspace.Root, "manifest.json");

        var first = ModuleWriter.Write(rootAssembly, workspace.Output, manifestPath, modules,
            ["./APP.js", "pages\\editor.js", "app.js"]);
        Assert.IsTrue(first.IsSuccess, first.Error);
        var manifest = ManifestModel.TryLoad(manifestPath)!;
        CollectionAssert.AreEqual(new[] { "app.js", "pages/editor.js" }, manifest.Entries.ToArray());
        Assert.HasCount(3, manifest.Modules);
        foreach (var module in modules)
            Assert.AreEqual(module.Content, File.ReadAllText(Path.Combine(workspace.Output, module.RelativePath)));

        var second = ModuleWriter.Write(rootAssembly, workspace.Output, manifestPath, modules,
            ["pages/editor.js", "app.js"]);
        Assert.IsTrue(second.IsSuccess, second.Error);
        Assert.AreEqual(0, second.Written);
        CollectionAssert.AreEqual(manifest.Entries.ToArray(), ManifestModel.TryLoad(manifestPath)!.Entries.ToArray());

        var defaultResult = ModuleWriter.Write(rootAssembly, workspace.Output, manifestPath, modules);
        Assert.IsTrue(defaultResult.IsSuccess, defaultResult.Error);
        CollectionAssert.AreEqual(new[] { "app.js", "pages/editor.js" }, ManifestModel.TryLoad(manifestPath)!.Entries.ToArray());
        var hostWithoutCatalog = new ManifestModel("Server.dll", manifest.Modules);
        CollectionAssert.AreEqual(new[] { "app.js", "library.js", "pages/editor.js" }, hostWithoutCatalog.Entries.ToArray());
    }

    [TestMethod]
    [DataRow("missing.js")]
    [DataRow("../app.js")]
    [DataRow("/app.js")]
    public async Task ExecuteAsync_InvalidBrowserEntry_FailsBeforeWriting(string entry)
    {
        using var workspace = new Workspace();
        var assembly = CompileCatalog(workspace.Root);
        var result = await new EmitPipeline().ExecuteAsync(CreateOptions(workspace, assembly, [entry]));

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Error, "failed during 'collect modules'");
        Assert.IsFalse(Directory.Exists(workspace.Output));
    }

    [TestMethod]
    public async Task ExecuteAsync_ExplicitRoot_ReleaseKeepsEditorInLazyChunk()
    {
        using var workspace = new Workspace();
        var assembly = CompileCatalog(workspace.Root);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var result = await new EmitPipeline().ExecuteAsync(
            CreateOptions(workspace, assembly, ["app.js"]) with
            {
                Mode = BuildMode.Production,
                DenoExecutablePath = DenoPath()
            }, timeout.Token);

        Assert.IsTrue(result.IsSuccess, result.Error);
        Assert.AreEqual("export * from \"./app.js\";\n", File.ReadAllText(Path.Combine(workspace.Output, "entry.js")));
        var manifest = ManifestModel.TryLoad(Path.Combine(workspace.Root, "manifest.json"))!;
        CollectionAssert.AreEqual(new[] { "app.js" }, manifest.Entries.ToArray());
        Assert.HasCount(3, manifest.Modules);
        Assert.IsTrue(File.Exists(Path.Combine(workspace.Output, "pages", "editor.js")));
        var bundledEntry = File.ReadAllText(Path.Combine(workspace.Output, "dist", "bundle.js"));
        StringAssert.Contains(bundledEntry, "import(");
        Assert.IsFalse(bundledEntry.Contains("LAZY_EDITOR_SENTINEL", StringComparison.Ordinal));
        var chunks = Directory.GetFiles(Path.Combine(workspace.Output, "dist"), "*.js", SearchOption.AllDirectories);
        Assert.IsTrue(chunks.Any(path => File.ReadAllText(path).Contains("LAZY_EDITOR_SENTINEL", StringComparison.Ordinal)));
        Assert.IsFalse(chunks.Any(path => File.ReadAllText(path).Contains("UNUSED_MODULE_SENTINEL", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task ExecuteAsync_ExplicitRoot_StillChecksUnreferencedAuthoredModules()
    {
        using var workspace = new Workspace();
        var assembly = CompileCatalog(workspace.Root, unusedContent: "import './missing-authored.js';");
        Directory.CreateDirectory(workspace.Output);
        // A local-only project exercises authored-module checks without a package restore.
        File.WriteAllText(Path.Combine(workspace.Output, "package.json"), """{"type":"module","scripts":{"build":"custom-tool"}}""");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await new EmitPipeline().ExecuteAsync(
            CreateOptions(workspace, assembly, ["app.js"]) with { DenoExecutablePath = DenoPath() }, timeout.Token);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Error, "failed during 'restore/check packages'");
        StringAssert.Contains(result.Error, "missing-authored.js");
        Assert.AreEqual("export * from \"./app.js\";\n", File.ReadAllText(Path.Combine(workspace.Output, "entry.js")));
    }

    [TestMethod]
    public void ProjectEntryWriter_ExplicitBrowserRoot_PreservesHydrationAndSsrRoots()
    {
        using var workspace = new Workspace();
        ProjectEntryWriter.Write(workspace.Output, ["app.js"], enableSsr: true,
            hydrationRoots: ["pages/editor.js", "pages/home.js"]);

        foreach (var name in new[] { ProjectEntryWriter.HydrationEntryFileName, ProjectEntryWriter.SsrBundleSourceFileName })
        {
            var content = File.ReadAllText(Path.Combine(workspace.Output, name));
            StringAssert.Contains(content, "\"pages/editor.js\": () => import(\"./pages/editor.js\")");
            StringAssert.Contains(content, "\"pages/home.js\": () => import(\"./pages/home.js\")");
            Assert.IsFalse(content.Contains("\"app.js\": () => import", StringComparison.Ordinal));
        }
    }

    private static EmitOptions CreateOptions(Workspace workspace, string assembly, IReadOnlyList<string> entries)
        => new(assembly, [], workspace.Output, Path.Combine(workspace.Root, "manifest.json"),
            BuildMode.Development, null, [], EnableSsr: false, BrowserEntries: entries);

    private static string DenoPath()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier,
            "native", OperatingSystem.IsWindows() ? "deno.exe" : "deno");
        Assert.IsTrue(File.Exists(path), "Use the runtime from the current test output.");
        return path;
    }

    private static ModuleRecord CreateModule(string assembly, string assemblyName, string path, string content)
        => new(assembly, assemblyName, path, path, path, content, ArtifactHash.ComputeSha256(content));

    private static string CompileCatalog(string directory, string unusedContent = "export const unused = 'UNUSED_MODULE_SENTINEL';")
    {
        var declarations = new[]
        {
            (Path: "app.js", Content: "export const openEditor = () => import('./pages/editor.js');"),
            (Path: "pages/editor.js", Content: "export const editor = 'LAZY_EDITOR_SENTINEL';"),
            (Path: "unused.js", Content: unusedContent)
        }.Select(module => $"new Module({Literal(module.Path)}, {Literal(module.Content)}, {Literal(ArtifactHash.ComputeSha256(module.Content))})");
        var source = $$"""
            namespace Jazor.Generated;
            internal static class ModuleCatalog
            {
                internal const int SchemaVersion = 2;
                internal const string AssemblyName = "BrowserEntryHost";
                internal static System.Collections.IEnumerable GetModules() => new object[] { {{string.Join(",", declarations)}} };
                private sealed class Module(string path, string content, string hash)
                {
                    public string Id => path;
                    public string TypeName => path;
                    public string RelativePath => path;
                    public string Content => content;
                    public string Hash => hash;
                    public string[] Dependencies => new string[0];
                }
            }
            """;
        var compilation = CSharpCompilation.Create("BrowserEntryHost",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            Net110.References.All.Cast<MetadataReference>(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var assembly = Path.Combine(directory, "BrowserEntryHost.dll");
        using var stream = File.Create(assembly);
        var result = compilation.Emit(stream);
        Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        return assembly;
    }

    private static string Literal(string value) => Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(value, quote: true);

    private sealed class Workspace : IDisposable
    {
        public Workspace()
        {
            Root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitTest", "browser-entry", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }
        public string Root { get; }
        public string Output => Path.Combine(Root, "project");
        public void Dispose()
        {
            // Emit's collectible load context must release temporary catalog assemblies first.
            for (var attempt = 0; attempt < 10; attempt++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            Directory.Delete(Root, recursive: true);
        }
    }
}
