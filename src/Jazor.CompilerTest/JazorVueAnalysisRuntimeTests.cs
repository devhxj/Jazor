using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jolt.Analysis;

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
                @module { debounce } from "lodash-es"

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
        Assert.AreEqual(2, response.SourceMaps.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
        Assert.AreEqual(response.Artifacts[0].ArtifactName, response.SourceMaps[0].GeneratedPath);
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
    public async Task JazorVueAnalysisService_AnalyzeJazor_LegacyVueImport_ReportsUnsupportedDiagnostic()
    {
        var service = new JazorVueAnalysisService();
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                "legacy-vueimport"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await service.AnalyzeJazorAsync(request, CancellationToken.None);
        var diagnostic = response.Diagnostics.FirstOrDefault(static record =>
            string.Equals(record.Id, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal));

        Assert.IsNotNull(diagnostic);
        Assert.AreEqual(DiagnosticSeverityKind.Error, diagnostic.Severity);
        Assert.AreEqual(0, diagnostic.Start);
        Assert.AreEqual("@vueimport".Length, diagnostic.Length);
        StringAssert.Contains(diagnostic.Message, "@vueimport");
        StringAssert.Contains(diagnostic.Message, "Use @module");
    }

    [TestMethod]
    public async Task JazorVueAnalysisService_AnalyzeJazor_LegacyJsImport_ReportsUnsupportedDiagnostic()
    {
        var service = new JazorVueAnalysisService();
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """
                @jsimport dayjs from "dayjs"
                <template><div /></template>
                """,
                "legacy-jsimport"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await service.AnalyzeJazorAsync(request, CancellationToken.None);
        var diagnostic = response.Diagnostics.FirstOrDefault(static record =>
            string.Equals(record.Id, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal));

        Assert.IsNotNull(diagnostic);
        Assert.AreEqual(DiagnosticSeverityKind.Error, diagnostic.Severity);
        Assert.AreEqual(0, diagnostic.Start);
        Assert.AreEqual("@jsimport".Length, diagnostic.Length);
        StringAssert.Contains(diagnostic.Message, "@jsimport");
        StringAssert.Contains(diagnostic.Message, "Use @module");
    }

    [TestMethod]
    public async Task JazorVueAnalysisService_AnalyzeJazor_LegacyImport_ReportsUnsupportedDiagnostic()
    {
        var service = new JazorVueAnalysisService();
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """
                @import dayjs from "dayjs"
                <template><div /></template>
                """,
                "legacy-import"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await service.AnalyzeJazorAsync(request, CancellationToken.None);
        var diagnostic = response.Diagnostics.FirstOrDefault(static record =>
            string.Equals(record.Id, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal));

        Assert.IsNotNull(diagnostic);
        Assert.AreEqual(DiagnosticSeverityKind.Error, diagnostic.Severity);
        Assert.AreEqual(0, diagnostic.Start);
        Assert.AreEqual("@import".Length, diagnostic.Length);
        StringAssert.Contains(diagnostic.Message, "@import");
        StringAssert.Contains(diagnostic.Message, "Use @module");
    }

    [TestMethod]
    public async Task JazorVueAnalysisService_AnalyzeJazor_IgnoresLegacyImportDirectivesInsideCommentsAndCodeStrings()
    {
        var service = new JazorVueAnalysisService();
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                """"
                @module dayjs from "dayjs"
                @*
                @jsimport fakeComment from "./fake-comment.ts"
                *@

                <template>
                  <div />
                </template>

                @code {
                    private string LegacyMarker => """
                    @import fakeRaw from "./fake-raw.ts"
                    """;
                }
                """",
                "legacy-ignore"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await service.AnalyzeJazorAsync(request, CancellationToken.None);
        var diagnostics = response.Diagnostics
            .Where(static record => string.Equals(record.Id, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal))
            .ToArray();

        Assert.AreEqual(0, diagnostics.Length);
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
                @module dayjs from "dayjs"

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

    [TestMethod]
    public async Task JazorVueAnalysisRpcProcessor_AnalyzeJazor_MissingPayload_ReturnsInvalidPayloadError()
    {
        var processor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var requestJson = VueAnalysisRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "analysis-invalid-payload",
            method: VueAnalysisRpcMethodNames.AnalyzeJazor,
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.AreEqual("analysis-invalid-payload", response.Id);
        Assert.IsNotNull(response.Error);
        Assert.AreEqual("invalid_payload", response.Error.Code);
    }

    [TestMethod]
    public async Task JazorVueAnalysisRpcProcessor_WhenServiceCancellationBubbles_ReturnsCancelledError()
    {
        var processor = new VueAnalysisRpcProcessor(new CancelledAnalysisService());
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "5"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);
        var requestJson = VueAnalysisRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "analysis-cancelled",
            method: VueAnalysisRpcMethodNames.AnalyzeJazor,
            payloadJson: VueAnalysisRpcSerializer.Serialize(request)));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.AreEqual("analysis-cancelled", response.Id);
        Assert.IsNotNull(response.Error);
        Assert.AreEqual("cancelled", response.Error.Code);
    }

    [TestMethod]
    public async Task JazorVueAnalysisStdioServer_SkipsBlankLinesAndContinuesProcessing()
    {
        var processor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var server = new StdioVueAnalysisRpcServer(processor);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "6"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);
        var requestJson = VueAnalysisRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "analysis-stdio-blank-lines",
            method: VueAnalysisRpcMethodNames.AnalyzeJazor,
            payloadJson: VueAnalysisRpcSerializer.Serialize(request)));

        using var input = new StringReader(Environment.NewLine + requestJson + Environment.NewLine + Environment.NewLine);
        using var output = new StringWriter();

        await server.RunAsync(input, output, CancellationToken.None);

        var responseLines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.AreEqual(1, responseLines.Length);

        var response = VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseLines[0]);
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual("analysis-stdio-blank-lines", response.Id);
    }

    private sealed class CancelledAnalysisService : IVueAnalysisRpcService
    {
        public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(AnalyzeJazorRequest request, CancellationToken cancellationToken)
            => throw new OperationCanceledException("cancelled for test");
    }
}
