using Jazor.Vue.Analysis.Runtime;
using Jazor.VueContracts.Protocol;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueAnalysisRuntimeTests
{
    [TestMethod]
    public async Task JazorVueAnalysisService_AnalyzeJazor_ReturnsImportsAndArtifacts()
    {
        var service = new JazorVueAnalysisService();
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """
                @jsimport { debounce } from "lodash-es"
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>

                @code {
                    [Prop] public string Title { get; set; } = "";
                }
                """,
                "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await service.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreEqual(2, response.Imports.Count);
        Assert.AreEqual("debounce", response.Imports[0].LocalName);
        Assert.AreEqual("./UserCard.vue", response.Imports[1].Source);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
        StringAssert.Contains(response.Artifacts[0].Content, "<template>");
        StringAssert.Contains(response.Artifacts[1].Content, "__VueComponentSymbol");
    }

    [TestMethod]
    public async Task JazorVueAnalysisService_AnalyzeJazor_MapsCompilerDiagnostics()
    {
        var service = new JazorVueAnalysisService();
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """
                <template>
                  <div />
                </template>

                @code {
                    private void Hidden()
                    {
                    }
                }
                """,
                "2"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await service.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreEqual(1, response.Diagnostics.Count);
        Assert.AreEqual("JAZORVUE001", response.Diagnostics[0].Id);
        Assert.AreEqual(DiagnosticSeverityKind.Warning, response.Diagnostics[0].Severity);
    }

    [TestMethod]
    public async Task JazorVueAnalysisRpcProcessor_AnalyzeJazor_ReturnsResponseEnvelope()
    {
        var service = new JazorVueAnalysisService();
        var processor = new VueAnalysisRpcProcessor(service);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """
                @jsimport dayjs from "dayjs"

                <template>
                  <div />
                </template>

                @code {
                    public void Tick()
                    {
                    }
                }
                """,
                "3"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);
        var requestJson = VueAnalysisRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "analysis-req-1",
            method: VueAnalysisRpcMethodNames.AnalyzeJazor,
            payloadJson: VueAnalysisRpcSerializer.Serialize(request)));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var payload = response?.PayloadJson is null
            ? null
            : VueAnalysisRpcSerializer.Deserialize<AnalyzeJazorResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual("analysis-req-1", response.Id);
        Assert.IsNotNull(payload);
        Assert.AreEqual(1, payload.Imports.Count);
        Assert.AreEqual("dayjs", payload.Imports[0].LocalName);
    }

    [TestMethod]
    public async Task JazorVueAnalysisRpcProcessor_UnknownMethod_ReturnsErrorEnvelope()
    {
        var processor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var requestJson = VueAnalysisRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "analysis-unknown",
            method: "vueanalysis/unknown",
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.AreEqual("analysis-unknown", response.Id);
        Assert.IsNotNull(response.Error);
        Assert.AreEqual("unknown_method", response.Error.Code);
    }

    [TestMethod]
    public async Task JazorVueAnalysisStdioServer_ProcessesAnalyzeJazorRequest()
    {
        var processor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var server = new StdioVueAnalysisRpcServer(processor);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """
                <template>
                  <div />
                </template>

                @code {
                    public void Tick()
                    {
                    }
                }
                """,
                "4"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);
        var requestJson = VueAnalysisRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "analysis-stdio",
            method: VueAnalysisRpcMethodNames.AnalyzeJazor,
            payloadJson: VueAnalysisRpcSerializer.Serialize(request)));

        using var input = new StringReader(requestJson + Environment.NewLine);
        using var output = new StringWriter();

        await server.RunAsync(input, output, CancellationToken.None);

        var responseJson = output.ToString().Trim();
        var response = VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual("analysis-stdio", response.Id);
    }
}
