using Jazor.Vite;
using Jazor.Vite.VueHost;
using Jazor.VueContracts.Protocol;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorViteTests
{
    [TestMethod]
    public void JazorViteOptions_Parse_RecognizesHostAndBunSettings()
    {
        var options = JazorViteOptions.Parse(
        [
            "--run-dev",
            "--bun-command=bunx",
            "--bun-args=vite dev --host",
            "--vuehost-command=dotnet",
            "--vuehost-args=run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --stdio",
            "--working-directory=src/Frontend"
        ]);

        Assert.AreEqual(JazorViteMode.RunDevServer, options.Mode);
        Assert.AreEqual("bunx", options.BunCommand);
        Assert.AreEqual("vite dev --host", options.BunArguments);
        Assert.AreEqual("dotnet", options.VueHostCommand);
        Assert.AreEqual("run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --stdio", options.VueHostArguments);
        Assert.AreEqual("src/Frontend", options.WorkingDirectory);
    }

    [TestMethod]
    public async Task JazorVite_ProcessVueHostRpcClient_GetHostInfo_InteropsWithVueHostProcess()
    {
        var hostProject = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Jazor.VueHost",
            "Jazor.VueHost.csproj");
        var client = new ProcessVueHostRpcClient(
            "dotnet",
            $"run --project \"{hostProject}\" -- --stdio");

        var hostInfo = await client.GetHostInfoAsync(CancellationToken.None);

        Assert.AreEqual("Jazor.VueHost", hostInfo.HostName);
        Assert.AreEqual("0.1", hostInfo.ProtocolVersion);
        Assert.IsTrue(hostInfo.Capabilities.Any(static capability => capability.Name == VueHostRpcMethodNames.GetHostInfo));
    }

    private static string GetRepositoryRoot()
        => Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));
}
