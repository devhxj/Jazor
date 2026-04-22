using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jolt.Analysis;
using Jolt.DevServer;
using Jolt.Hosting;

namespace Jolt.Test;

[TestClass]
public sealed class JoltFallbackTelemetryTests
{
    [TestMethod]
    public async Task CompatibilityNullAnalysisClient_AnalyzeJazorAsync_ReportsFallbackOncePerDocumentPath()
    {
        var events = new List<string>();
        FallbackTelemetry.ResetForTests();
        FallbackTelemetry.SetTestSinkForTests(events.Add);

        try
        {
            var client = new TestNullVueAnalysisClient();
            await client.AnalyzeJazorAsync(CreateAnalyzeRequest("Features/Counter.jazor"), CancellationToken.None);
            await client.AnalyzeJazorAsync(CreateAnalyzeRequest("Features/Counter.jazor"), CancellationToken.None);
            await client.AnalyzeJazorAsync(CreateAnalyzeRequest("Features/Other.jazor"), CancellationToken.None);
        }
        finally
        {
            FallbackTelemetry.SetTestSinkForTests(null);
            FallbackTelemetry.ResetForTests();
        }

        Assert.AreEqual(2, events.Count);
        AssertFallbackEvent(
            events[0],
            component: "analysisClient",
            mode: "null",
            reason: "analysis-transport-unavailable",
            documentPath: "Features/Counter.jazor");
        AssertFallbackEvent(
            events[1],
            component: "analysisClient",
            mode: "null",
            reason: "analysis-transport-unavailable",
            documentPath: "Features/Other.jazor");
    }

    [TestMethod]
    public async Task StubFrontendModuleCompiler_CompileMethods_ReportsFallbackOnce()
    {
        var events = new List<string>();
        FallbackTelemetry.ResetForTests();
        FallbackTelemetry.SetTestSinkForTests(events.Add);

        try
        {
            var compiler = new StubFrontendModuleCompiler();
            await compiler.CompileSfcAsync(
                "Features/Counter.vue",
                "<template><div>Counter</div></template>",
                CancellationToken.None);
            await compiler.CompileTypeScriptAsync(
                "Features/counter.ts",
                "export const count = 1;",
                CancellationToken.None);
        }
        finally
        {
            FallbackTelemetry.SetTestSinkForTests(null);
            FallbackTelemetry.ResetForTests();
        }

        Assert.AreEqual(1, events.Count);
        AssertFallbackEvent(
            events[0],
            component: "frontendCompiler",
            mode: "stub",
            reason: "deno-frontend-unavailable",
            documentPath: null);
    }

    [TestMethod]
    public void VueAnalysisClientFactory_CreateDefault_DoesNotReportFallbackTelemetry()
    {
        var events = new List<string>();
        FallbackTelemetry.ResetForTests();
        FallbackTelemetry.SetTestSinkForTests(events.Add);

        try
        {
            _ = VueAnalysisClientFactory.CreateDefault();
            _ = VueAnalysisClientFactory.CreateDefault();
        }
        finally
        {
            FallbackTelemetry.SetTestSinkForTests(null);
            FallbackTelemetry.ResetForTests();
        }

        Assert.AreEqual(0, events.Count);
    }

    private static AnalyzeJazorRequest CreateAnalyzeRequest(string documentPath)
        => new(
            new DocumentSnapshot(
                documentPath,
                DocumentKind.Jazor,
                "<template><div /></template>",
                "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

    private static void AssertFallbackEvent(
        string payload,
        string component,
        string mode,
        string reason,
        string? documentPath)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        Assert.AreEqual("vueHostFallbackActivated", root.GetProperty("eventType").GetString());
        Assert.AreEqual(component, root.GetProperty("component").GetString());
        Assert.AreEqual(mode, root.GetProperty("mode").GetString());
        Assert.AreEqual(reason, root.GetProperty("reason").GetString());
        if (documentPath is null)
        {
            Assert.IsTrue(root.GetProperty("documentPath").ValueKind is JsonValueKind.Null);
        }
        else
        {
            Assert.AreEqual(documentPath, root.GetProperty("documentPath").GetString());
        }
    }

    private sealed class TestNullVueAnalysisClient : IVueAnalysisClient
    {
        public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
            AnalyzeJazorRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            FallbackTelemetry.ReportActivation(
                component: "analysisClient",
                mode: "null",
                reason: "analysis-transport-unavailable",
                documentPath: request.JazorDocument.DocumentPath);

            return ValueTask.FromResult(new AnalyzeJazorResponse(
                diagnostics: Array.Empty<DiagnosticRecord>(),
                imports: Array.Empty<ImportDescriptor>(),
                artifacts: Array.Empty<ArtifactRecord>(),
                sourceMaps: Array.Empty<SourceMapDescriptor>()));
        }
    }
}
