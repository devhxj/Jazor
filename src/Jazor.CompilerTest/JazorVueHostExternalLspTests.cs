using System.Text.Json;
using Jazor.VueHost.LanguageServers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostExternalLspTests
{
    [TestMethod]
    public async Task ExternalLspClient_CanInitialize_AgainstVueHostLoopback()
    {
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        await using var client = new ExternalLspClient(
            new ExternalProcessOptions
            {
                Name = "LoopbackVueHost",
                FileName = "dotnet",
                Arguments =
                [
                    hostAssemblyPath,
                    "--lsp"
                ]
            });

        var result = await client.InitializeAsync(
            rootPath: Path.GetDirectoryName(hostAssemblyPath),
            cancellationToken: CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual("Jazor.VueHost", result!.Name);
    }

    [TestMethod]
    public void LanguageServerCatalog_CanDiscover_LocalRoslynAndFrontendServers()
    {
        var catalog = LanguageServerCatalog.CreateDefault();

        if (catalog.Roslyn is null || catalog.Volar is null || catalog.TypeScript is null)
        {
            Assert.Inconclusive("Roslyn/Volar/TypeScript discovery depends on locally installed editor components.");
        }

        Assert.IsFalse(string.IsNullOrWhiteSpace(catalog.Roslyn!.FileName));
        Assert.IsFalse(string.IsNullOrWhiteSpace(catalog.Volar!.FileName));
        Assert.IsFalse(string.IsNullOrWhiteSpace(catalog.TypeScript!.FileName));
    }

    private static string GetBuiltAssemblyPath(string projectName, string assemblyName)
    {
        var rootDirectory = FindRepositoryRoot();
        var binDirectory = Path.Combine(rootDirectory, "src", projectName, "bin", "Debug", "net10.0");
        var assemblyPath = Path.Combine(binDirectory, assemblyName);
        Assert.IsTrue(File.Exists(assemblyPath), $"Expected built assembly at '{assemblyPath}'.");
        return assemblyPath;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        Assert.Fail("Could not locate repository root.");
        return string.Empty;
    }
}
