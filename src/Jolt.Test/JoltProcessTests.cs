using System.Diagnostics;
using ECMAScript.Internal.VueContracts.Protocol;
using Jolt.Rpc;
using SharedJoltRpcMethodNames = ECMAScript.Internal.VueContracts.Protocol.JoltRpcMethodNames;

namespace Jolt.Test;

[TestClass]
public sealed class JoltProcessTests
{
    [TestMethod]
    public async Task Jolt_StdioProcess_AnalyzeJazor_DelegatesThroughJoltAnalysisMode()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var repositoryRoot = GetRepositoryRoot();
        var hostProjectPath = Path.Combine(
            repositoryRoot,
            "src",
            "Jolt",
            "Jolt.csproj");

        Assert.IsTrue(File.Exists(hostProjectPath), "Expected Jolt project to exist.");

        using var process = CreateJoltProcess(hostProjectPath);
        Assert.IsTrue(process.Start(), "Expected Jolt process to start.");

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
            volarContext: null);
        var requestJson = JoltRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "host-process-1",
            method: SharedJoltRpcMethodNames.AnalyzeJazor,
            payloadJson: JoltRpcSerializer.Serialize(request)));

        await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), cancellationSource.Token);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        var responseLine = await process.StandardOutput.ReadLineAsync(cancellationSource.Token);
        var errorOutput = await process.StandardError.ReadToEndAsync(cancellationSource.Token);
        await process.WaitForExitAsync(cancellationSource.Token);

        Assert.IsFalse(string.IsNullOrWhiteSpace(responseLine), errorOutput);

        var response = JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseLine!);
        var payload = response?.PayloadJson is null
            ? null
            : JoltRpcSerializer.Deserialize<AnalyzeJazorResponse>(response.PayloadJson);

        Assert.IsNotNull(response, errorOutput);
        Assert.IsTrue(response.Success, errorOutput);
        Assert.IsNotNull(payload);
        Assert.AreEqual(1, payload.Imports.Count);
        Assert.AreEqual("dayjs", payload.Imports[0].LocalName);
        Assert.AreEqual(2, payload.Artifacts.Count);
        Assert.AreEqual("vue-sfc", payload.Artifacts[0].ArtifactKind);
    }

    private static Process CreateJoltProcess(string hostProjectPath)
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
