using System.Diagnostics;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Rpc;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostProcessTests
{
    [TestMethod]
    public async Task JazorVueHost_StdioProcess_AnalyzeJazor_DelegatesThroughVueHostAnalysisMode()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var repositoryRoot = GetRepositoryRoot();
        var hostProjectPath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.VueHost",
            "Jazor.VueHost.csproj");

        Assert.IsTrue(File.Exists(hostProjectPath), "Expected Jazor.VueHost project to exist.");

        using var process = CreateVueHostProcess(hostProjectPath);
        Assert.IsTrue(process.Start(), "Expected Jazor.VueHost process to start.");

        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module dayjs from "dayjs"

                <template>
                  <div>{{ dayjs }}</div>
                </template>
                """,
                "process-1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "host-process-1",
            method: SharedVueHostRpcMethodNames.AnalyzeJazor,
            payloadJson: VueHostRpcSerializer.Serialize(request)));

        await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), cancellationSource.Token);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        var responseLine = await process.StandardOutput.ReadLineAsync(cancellationSource.Token);
        var errorOutput = await process.StandardError.ReadToEndAsync(cancellationSource.Token);
        await process.WaitForExitAsync(cancellationSource.Token);

        Assert.IsFalse(string.IsNullOrWhiteSpace(responseLine), errorOutput);

        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseLine!);
        var payload = response?.PayloadJson is null
            ? null
            : VueHostRpcSerializer.Deserialize<AnalyzeJazorResponse>(response.PayloadJson);

        Assert.IsNotNull(response, errorOutput);
        Assert.IsTrue(response.Success, errorOutput);
        Assert.IsNotNull(payload);
        Assert.AreEqual(1, payload.Imports.Count);
        Assert.AreEqual("dayjs", payload.Imports[0].LocalName);
        Assert.AreEqual(2, payload.Artifacts.Count);
        Assert.AreEqual("vue-sfc", payload.Artifacts[0].ArtifactKind);
    }

    private static Process CreateVueHostProcess(string hostProjectPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(hostProjectPath);
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add("--stdio");
        startInfo.ArgumentList.Add("--analysis-client=transport");
        startInfo.ArgumentList.Add("--analysis-command=dotnet");
        startInfo.ArgumentList.Add($"--analysis-args=run --no-build --no-restore --project \"{hostProjectPath}\" -- --analysis-stdio");

        return new Process
        {
            StartInfo = startInfo
        };
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
