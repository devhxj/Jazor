using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Hosting;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Rpc;
using Jazor.VueHost.Workspace;
using Jazor.VueHost.Services;
using Jazor.VueHost.VirtualDocuments.Registry;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

var useLsp = args.Any(static arg => string.Equals(arg, "--lsp", StringComparison.OrdinalIgnoreCase));
var useAnalysisStdio = args.Any(static arg => string.Equals(arg, "--analysis-stdio", StringComparison.OrdinalIgnoreCase));
var useStdio = Console.IsInputRedirected
    || args.Any(static arg => string.Equals(arg, "--stdio", StringComparison.OrdinalIgnoreCase));
var cancellationToken = CancellationToken.None;

if (useAnalysisStdio)
{
    var analysisProcessor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
    var analysisServer = new StdioVueAnalysisRpcServer(analysisProcessor);
    await analysisServer.RunAsync(Console.In, Console.Out, cancellationToken);
    return;
}

var analysisClient = VueAnalysisClientFactory.Create(args);
var workspaceStore = new InMemoryWorkspaceStore();
var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
var projectionService = new JazorProjectionService();
await using var denoFrontendHost = new DenoFrontendHost(DenoFrontendHostOptionsParser.Parse(args));
var projectionResolver = new DocumentProjectionResolver(
    new DocumentRegionClassifier(),
    virtualDocumentRegistry);
var laneRouter = new LspLaneRouter();
var resultAggregator = new LspResultAggregator();
var hostService = new VueHostService(
    workspaceStore,
    analysisClient,
    denoFrontendHost);
var entry = new VueHostServiceEntry(hostService);
var rpcDispatcher = new VueHostRpcDispatcher(hostService);
var rpcProcessor = new VueHostRpcProcessor(rpcDispatcher);

await entry.RunAsync(cancellationToken);
try
{
    if (useLsp)
    {
        var jazorDocumentService = new JazorLspDocumentService(workspaceStore, analysisClient);
        ILspLane[] lanes =
        [
            new JazorLaneService(jazorDocumentService),
            new RoslynLaneService(jazorDocumentService),
            new FrontendLaneService(jazorDocumentService, denoFrontendHost)
        ];
        var laneMap = lanes.ToDictionary(static lane => lane.LaneKind);
        var lspServer = new StdioLspServer(
            new LspSession(
                workspaceStore,
                lanes,
                laneRouter,
                new LspMessageWriter(Console.OpenStandardOutput()),
                projectionService,
                virtualDocumentRegistry,
                projectionResolver,
                resultAggregator,
                new RenameCoordinator(laneMap, laneRouter, resultAggregator),
                new CodeActionCoordinator(laneMap, laneRouter, resultAggregator)));
        await lspServer.RunAsync(
            Console.OpenStandardInput(),
            Console.OpenStandardOutput(),
            cancellationToken);
        return;
    }

    if (useStdio)
    {
        var stdioServer = new StdioVueHostRpcServer(rpcProcessor);
        await stdioServer.RunAsync(Console.In, Console.Out, cancellationToken);
        return;
    }

    var responseJson = await rpcProcessor.ProcessAsync(
        VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "startup",
            method: SharedVueHostRpcMethodNames.GetHostInfo,
            payloadJson: null)),
        cancellationToken);

    Console.WriteLine("Jazor.VueHost skeleton ready.");
    Console.WriteLine(responseJson);
}
finally
{
    await hostService.StopAsync(cancellationToken);
}
