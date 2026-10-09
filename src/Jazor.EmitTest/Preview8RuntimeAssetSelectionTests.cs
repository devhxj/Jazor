using System.Runtime.CompilerServices;
using System.Text.Json;
using Jazor.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.EmitTest;

[TestClass]
public sealed class Preview8RuntimeAssetSelectionTests
{
    [TestMethod]
    public void Collect_TargetRidFiltersForeignAssetsBeforeIdentityInspection()
    {
        using var fixture = new AssemblyFixture();
        var host = fixture.Compile("host", "Host", "1.0.0.0");
        var windows = fixture.Compile("runtimes/win-x64/lib/net11.0", "Management", "1.0.0.0");
        var unix = fixture.Compile("runtimes/unix/lib/net11.0", "Management", "2.0.0.0");
        // A foreign architecture cannot even be inspected as a managed input. Selection must
        // happen before AssemblyName.GetAssemblyName, not only before the later load.
        var foreign = fixture.Write("runtimes/linux-arm64/lib/net11.0/Foreign.dll", "invalid assembly");
        var native = fixture.Write("runtimes/win-x64/native/Native.dll", "native DLL");
        var result = Collect(host, "win-x64", null, windows, unix, foreign, native);

        Assert.IsTrue(result.IsSuccess, result.Error);
        Assert.AreEqual(2, result.AssemblyCount);
    }

    [TestMethod]
    public void Collect_RuntimeGraphChoosesClosestCompatibleIdentityBeforeDedupe()
    {
        using var fixture = new AssemblyFixture();
        var host = fixture.Compile("host", "Host", "1.0.0.0");
        var neutral = fixture.Compile("lib/net11.0", "Management", "3.0.0.0");
        var unix = fixture.Compile("runtimes/unix/lib/net11.0", "Management", "2.0.0.0");
        var linux = fixture.Compile("runtimes/linux/lib/net11.0", "Management", "1.0.0.0");
        var graph = fixture.Write("runtime.json", JsonSerializer.Serialize(new
        {
            runtimes = new Dictionary<string, object>
            {
                ["linux-x64"] = new Dictionary<string, string[]> { ["#import"] = ["linux", "unix-x64"] },
                ["linux"] = new Dictionary<string, string[]> { ["#import"] = ["unix"] },
                ["unix-x64"] = new Dictionary<string, string[]> { ["#import"] = ["unix"] }
            }
        }));
        var result = Collect(host, "linux-x64", graph, neutral, unix, linux);

        Assert.IsTrue(result.IsSuccess, result.Error);
        Assert.AreEqual(2, result.AssemblyCount);
        var selection = RuntimeAssetSelection.Create("linux-x64", graph);
        Assert.IsLessThan(selection.GetPriority("unix"), selection.GetPriority("unix-x64"),
            "A direct SDK graph fallback must precede its generic ancestor.");
    }

    [TestMethod]
    public void Collect_ConflictingCompatibleIdentitiesReportsPathsRidAndSelectionReason()
    {
        using var fixture = new AssemblyFixture();
        var host = fixture.Compile("host", "Host", "1.0.0.0");
        var first = fixture.Compile("first/runtimes/win-x64/lib/net11.0", "Management", "1.0.0.0");
        var second = fixture.Compile("second/runtimes/win-x64/lib/net11.0", "Management", "2.0.0.0");
        var result = Collect(host, "win-x64", null, first, second);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(2, result.ExitCode);
        StringAssert.Contains(result.Error!, first);
        StringAssert.Contains(result.Error!, second);
        StringAssert.Contains(result.Error!, "win-x64");
        StringAssert.Contains(result.Error!, "exact target RID");
        StringAssert.Contains(result.Error!, "Version=1.0.0.0");
        StringAssert.Contains(result.Error!, "Version=2.0.0.0");
    }

    [TestMethod]
    public void TryParse_RuntimeSelectionCarriesExplicitTargetAndSdkGraph()
    {
        var success = EmitOptions.TryParse(
            ["--root", "host.dll", "--out", "out", "--write-manifest", "state.json",
             "--runtime-identifier", "linux-arm64", "--runtime-identifier-graph", "runtime.json"],
            out var options, out var error);

        Assert.IsTrue(success, error);
        Assert.AreEqual("linux-arm64", options!.RuntimeIdentifier);
        Assert.AreEqual(Path.GetFullPath("runtime.json"), options.RuntimeIdentifierGraphPath);
    }

    private static CollectResult Collect(string root, string rid, string? graph, params string[] inputs)
    {
        var collection = CollectCore(root, rid, graph, inputs);
        // The collectible context may remain live until a GC even after Unload. Wait outside
        // the collection stack frame before fixture cleanup, including parallel test runs.
        for (var attempt = 0; attempt < 20 && collection.Context.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(collection.Context.IsAlive, "The test's isolated assembly context must unload before deleting its DLLs.");
        return collection.Result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (CollectResult Result, WeakReference Context) CollectCore(string root, string rid, string? graph, string[] inputs)
    {
        var context = new EmitLoadContext(root);
        var weakContext = new WeakReference(context);
        try
        {
            var collector = new ModuleCollector(context, rid, graph);
            collector.AddRootAssembly(root);
            foreach (var input in inputs)
                collector.AddAssembly(input);
            return (collector.Collect(root), weakContext);
        }
        finally
        {
            context.Unload();
        }
    }

    private sealed class AssemblyFixture : IDisposable
    {
        private readonly string _root = Path.Combine(RepositoryTemp.Root, "preview8-runtime-selection", Guid.NewGuid().ToString("N"));

        public string Compile(string directory, string name, string version)
        {
            var path = Path.Combine(_root, directory.Replace('/', Path.DirectorySeparatorChar), name + ".dll");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var compilation = CSharpCompilation.Create(name,
                [CSharpSyntaxTree.ParseText($"[assembly: System.Reflection.AssemblyVersion(\"{version}\")] public static class Marker {{ }}")],
                [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var stream = File.Create(path);
            var emitted = compilation.Emit(stream);
            Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
            return path;
        }

        public string Write(string relativePath, string content)
        {
            var path = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return path;
        }

        public void Dispose()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }
}
