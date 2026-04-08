using Jazor.VueContracts.Protocol;
using Jazor.Vue.Analysis.Runtime;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Rpc;
using Jazor.VueHost.Services;
using Jazor.VueHost.Workspace;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostTests
{
    [TestMethod]
    public async Task JazorVueHost_GetFrontendContext_ReturnsTrackedFrontendDocuments()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var vueDocument = new DocumentSnapshot(
            "Components/UserCard.vue",
            DocumentKind.Vue,
            "<template><div /></template>",
            "1");
        var tsDocument = new DocumentSnapshot(
            "Scripts/user-card.ts",
            DocumentKind.TypeScript,
            "export const x = 1;",
            "1");

        await host.OpenDocumentAsync(vueDocument, CancellationToken.None);
        await host.OpenDocumentAsync(tsDocument, CancellationToken.None);

        var response = await host.GetFrontendContextAsync(
            new GetFrontendContextRequest(
                "Features/Counter.jazor",
                ["Components/UserCard.vue", "Scripts/user-card.ts"]),
            CancellationToken.None);

        Assert.AreEqual("frontend", response.SemanticContext.ContextKind);
        Assert.AreEqual(2, response.SemanticContext.RelatedDocuments.Count);
        Assert.AreEqual("Components/UserCard.vue", response.SemanticContext.RelatedDocuments[0].DocumentPath);
        Assert.AreEqual("2", response.SemanticContext.Properties["relatedDocumentCount"]);
    }

    [TestMethod]
    public async Task JazorVueHost_AnalyzeJazor_DelegatesToAnalysisClient()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreEqual(0, response.Diagnostics.Count);
        Assert.AreEqual(0, response.Imports.Count);
        Assert.AreEqual(0, response.Artifacts.Count);
        Assert.AreEqual(0, response.SourceMaps.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_AnalyzeJazor_UsesInjectedAnalysisClient()
    {
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "5"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);
        var expectedResponse = new AnalyzeJazorResponse(
            diagnostics:
            [
                new DiagnosticRecord(
                    id: "JZ1001",
                    severity: DiagnosticSeverityKind.Warning,
                    message: "test",
                    documentPath: "Features/Counter.jazor",
                    start: 3,
                    length: 2)
            ],
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var analysisClient = new RecordingVueAnalysisClient(expectedResponse);
        var host = new VueHostService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreSame(request, analysisClient.LastRequest);
        Assert.AreEqual(expectedResponse.Diagnostics[0].Id, response.Diagnostics[0].Id);
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_UsesNullFallbackWhenTransportMissing()
    {
        var client = VueAnalysisClientFactory.CreateDefault();

        Assert.IsInstanceOfType<NullVueAnalysisClient>(client);
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_ParseRecognizesTransportMode()
    {
        var options = VueAnalysisClientFactory.Parse(
        [
            "--analysis-client=transport",
            "--analysis-command=dotnet",
            "--analysis-args=run --project src/Jazor.Vue.Analysis"
        ]);

        Assert.AreEqual(VueAnalysisClientMode.Transport, options.Mode);
        Assert.AreEqual("dotnet", options.Command);
        Assert.AreEqual("run --project src/Jazor.Vue.Analysis", options.Arguments);
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_FallsBackToNullWhenTransportModeHasNoCommand()
    {
        var client = VueAnalysisClientFactory.Create(
            new VueAnalysisClientOptions(
                VueAnalysisClientMode.Transport,
                command: null,
                arguments: null));

        Assert.IsInstanceOfType<NullVueAnalysisClient>(client);
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_UsesRpcClientWhenTransportProvided()
    {
        var client = VueAnalysisClientFactory.CreateFromTransport(
            new StubVueAnalysisRpcTransport(new RpcResponseEnvelope(
                id: "analysis-factory",
                success: true,
                payloadJson: VueHostRpcSerializer.Serialize(new AnalyzeJazorResponse(
                    diagnostics: Array.Empty<DiagnosticRecord>(),
                    imports: Array.Empty<ImportDescriptor>(),
                    artifacts: Array.Empty<ArtifactRecord>(),
                    sourceMaps: Array.Empty<SourceMapDescriptor>())),
                error: null)));

        Assert.IsInstanceOfType<RpcVueAnalysisClient>(client);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_UsesSharedEnvelopeAndMethodName()
    {
        var response = new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var transport = new StubVueAnalysisRpcTransport(
            new RpcResponseEnvelope(
                id: "analysis-1",
                success: true,
                payloadJson: VueHostRpcSerializer.Serialize(response),
                error: null));
        var client = new RpcVueAnalysisClient(transport);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot("Features/Counter.jazor", DocumentKind.Jazor, "<template/>", "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var result = await client.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.IsNotNull(transport.LastRequest);
        Assert.AreEqual(VueAnalysisRpcMethodNames.AnalyzeJazor, transport.LastRequest.Method);
        Assert.IsFalse(string.IsNullOrWhiteSpace(transport.LastRequest.Id));
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_ThrowsOnErrorEnvelope()
    {
        var transport = new StubVueAnalysisRpcTransport(
            new RpcResponseEnvelope(
                id: "analysis-err",
                success: false,
                payloadJson: null,
                error: new RpcErrorRecord("analysis_failure", "boom", null)));
        var client = new RpcVueAnalysisClient(transport);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot("Features/Counter.jazor", DocumentKind.Jazor, "<template/>", "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        InvalidOperationException? exception = null;
        try
        {
            await client.AnalyzeJazorAsync(request, CancellationToken.None);
        }
        catch (InvalidOperationException caught)
        {
            exception = caught;
        }

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Message.Contains("analysis_failure", StringComparison.Ordinal));
        Assert.IsTrue(exception.Message.Contains("boom", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_CanBridgeToVueAnalysisProcessor()
    {
        var analysisProcessor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var transport = new DelegateAnalysisRpcTransport(async (request, cancellationToken) =>
        {
            var responseJson = await analysisProcessor.ProcessAsync(
                VueAnalysisRpcSerializer.Serialize(request),
                cancellationToken);

            return VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson)
                ?? throw new InvalidOperationException("Expected a valid analysis RPC response envelope.");
        });
        var host = new VueHostService(
            new InMemoryWorkspaceStore(),
            VueAnalysisClientFactory.CreateFromTransport(transport));
        await host.StartAsync(CancellationToken.None);

        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
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
                "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_InteropsWithVueAnalysisRpcProcessor()
    {
        var analysisProcessor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var transport = new DelegateAnalysisRpcTransport(async (request, cancellationToken) =>
        {
            var requestJson = VueAnalysisRpcSerializer.Serialize(request);
            var responseJson = await analysisProcessor.ProcessAsync(requestJson, cancellationToken);
            return VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson)
                ?? throw new InvalidOperationException("Failed to deserialize VueAnalysis RPC response.");
        });
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new VueHostService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
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
                    "6"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
    }

    [TestMethod]
    public async Task JazorVueHost_ProcessAnalysisRpcTransport_InteropsWithAnalysisHostProcess()
    {
        var analysisHostProject = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Jazor.Vue.Analysis.Host",
            "Jazor.Vue.Analysis.Host.csproj");
        var transport = new ProcessAnalysisRpcTransport(
            "dotnet",
            $"run --project \"{analysisHostProject}\" -- --stdio");
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new VueHostService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @jsimport dayjs from "dayjs"

                    <template>
                      <div />
                    </template>
                    """,
                    "8"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_ProcessAnalysisRpcTransport_InteropsWithVueAnalysisHostProcess()
    {
        var repositoryRoot = GetRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vue.Analysis.Host",
            "Jazor.Vue.Analysis.Host.csproj");
        Assert.IsTrue(File.Exists(projectPath), "Expected Jazor.Vue.Analysis.Host project to exist.");

        var transport = new ProcessAnalysisRpcTransport(
            command: "dotnet",
            arguments: $"run --project \"{projectPath}\" -- --stdio");
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new VueHostService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @vueimport UserCard from "./UserCard.vue"

                    <template>
                      <UserCard />
                    </template>

                    @code {
                        [Prop] public string Title { get; set; } = "";
                    }
                    """,
                    "8"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("UserCard", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcProcessor_GetOpenDocuments_ReturnsSerializedEnvelope()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div /></template>",
                "7"),
            CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-1",
            method: SharedVueHostRpcMethodNames.GetOpenDocuments,
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var documents = response?.PayloadJson is null
            ? Array.Empty<DocumentSnapshot>()
            : VueHostRpcSerializer.Deserialize<DocumentSnapshot[]>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.AreEqual("req-1", response.Id);
        Assert.IsTrue(response.Success);
        Assert.IsNull(response.Error);
        Assert.IsNotNull(documents);
        Assert.AreEqual(1, documents.Length);
        Assert.AreEqual("Components/UserCard.vue", documents[0].DocumentPath);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcProcessor_UnknownMethod_ReturnsErrorEnvelope()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-unknown",
            method: "vuehost/unknown",
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.IsNotNull(response.Error);
        Assert.AreEqual("unknown_method", response.Error.Code);
        Assert.AreEqual("req-unknown", response.Id);
        Assert.IsTrue(response.Error.Message.Contains("Unknown Jazor.VueHost RPC method", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_RpcProcessor_GetHostInfo_ReturnsCapabilities()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-host-info",
            method: SharedVueHostRpcMethodNames.GetHostInfo,
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var hostInfo = response?.PayloadJson is null
            ? null
            : VueHostRpcSerializer.Deserialize<GetHostInfoResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.IsNotNull(hostInfo);
        Assert.AreEqual("Jazor.VueHost", hostInfo.HostName);
        Assert.AreEqual("0.1", hostInfo.ProtocolVersion);
        Assert.IsTrue(hostInfo.Capabilities.Any(static capability => capability.Name == SharedVueHostRpcMethodNames.GetHostInfo));
    }

    [TestMethod]
    public async Task JazorVueHost_StdioServer_ProcessesPingRequest()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-ping",
            method: SharedVueHostRpcMethodNames.Ping,
            payloadJson: null));

        using var input = new StringReader(requestJson + Environment.NewLine);
        using var output = new StringWriter();
        var server = new StdioVueHostRpcServer(processor);

        await server.RunAsync(input, output, CancellationToken.None);

        var responseJson = output.ToString().Trim();
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var ping = response?.PayloadJson is null
            ? null
            : VueHostRpcSerializer.Deserialize<PingResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual("req-ping", response.Id);
        Assert.IsNotNull(ping);
        Assert.AreEqual("pong", ping.Message);
        Assert.AreEqual("0.1", ping.ProtocolVersion);
    }

    private sealed class RecordingVueAnalysisClient : IVueAnalysisClient
    {
        private readonly AnalyzeJazorResponse _response;

        public RecordingVueAnalysisClient(AnalyzeJazorResponse response)
        {
            _response = response;
        }

        public AnalyzeJazorRequest? LastRequest { get; private set; }

        public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(AnalyzeJazorRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return ValueTask.FromResult(_response);
        }
    }

    private sealed class StubVueAnalysisRpcTransport : IAnalysisRpcTransport
    {
        private readonly RpcResponseEnvelope _responseEnvelope;

        public StubVueAnalysisRpcTransport(RpcResponseEnvelope responseEnvelope)
        {
            _responseEnvelope = responseEnvelope;
        }

        public RpcRequestEnvelope? LastRequest { get; private set; }

        public ValueTask<RpcResponseEnvelope> SendAsync(
            RpcRequestEnvelope request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return ValueTask.FromResult(_responseEnvelope);
        }
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
