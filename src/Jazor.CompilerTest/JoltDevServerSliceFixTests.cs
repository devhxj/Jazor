using Jolt.DevServer;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltDevServerSliceFixTests
{
    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JavaScriptFile_RecordsDependenciesInDevMode()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var mainPath = Path.Combine(rootDirectory, "main.js");
            var childPath = Path.Combine(rootDirectory, "child.js");
            await File.WriteAllTextAsync(
                mainPath,
                """
                import "./child.js";
                export const value = 1;
                """);
            await File.WriteAllTextAsync(childPath, "export const child = 1;");

            var moduleResolver = new ModuleResolver(rootDirectory);
            var dependencyGraph = new DependencyGraph(moduleResolver);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler: null,
                new CompilationCache(),
                dependencyGraph,
                moduleResolver);

            var result = await compiler.CompileAsync(mainPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            CollectionAssert.Contains(result.Dependencies.ToArray(), "./child.js");
            CollectionAssert.AreEqual(
                new[] { childPath },
                dependencyGraph.GetDependencies(mainPath).ToArray());
            CollectionAssert.AreEqual(
                new[] { mainPath },
                dependencyGraph.GetDependents(childPath).ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-devserver-slice-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }
}
