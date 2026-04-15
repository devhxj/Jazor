using Jazor.VueContracts.Protocol;
using Jazor.VueHost.DevServer;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Hosting;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Razor.InProc;
using Jazor.VueHost.Razor.Toolset;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Rpc;
using Jazor.VueHost.Roslyn.InProc;
using Jazor.VueHost.Workspace;
using Jazor.VueHost.Services;
using Jazor.VueHost.VirtualDocuments.Registry;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

var useLsp = args.Any(static arg => string.Equals(arg, "--lsp", StringComparison.OrdinalIgnoreCase));
var useDev = args.Any(static arg => string.Equals(arg, "--dev", StringComparison.OrdinalIgnoreCase));
var useAnalysisStdio = args.Any(static arg => string.Equals(arg, "--analysis-stdio", StringComparison.OrdinalIgnoreCase));
var inspectRazorToolset = args.Any(static arg => string.Equals(arg, "--inspect-razor-toolset", StringComparison.OrdinalIgnoreCase));
var probeInProcRazorPath = GetOptionValue(args, "--probe-inproc-razor");
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

if (useDev && !useLsp)
{
    await using var denoFrontendHost = new DenoVolarHost(DenoVolarHostOptionsParser.Parse(args));
    var devOptions = DevServerOptionsParser.Parse(args);
    await using var devServer = CreateDevServer(devOptions, denoFrontendHost);
    using var shutdownSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        shutdownSource.Cancel();
    };

    await devServer.StartAsync(shutdownSource.Token);
    Console.WriteLine($"Jazor.VueHost dev server listening on {devServer.ListeningUri ?? new Uri($"http://{devOptions.Host}:{devOptions.Port}")}");

    try
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, shutdownSource.Token);
    }
    catch (OperationCanceledException)
    {
        // Ctrl+C shutdown.
    }

    return;
}

var analysisClient = VueAnalysisClientFactory.Create(args);
var workspaceStore = new InMemoryWorkspaceStore();
var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
// Keep one shared in-proc projection pipeline so LSP lanes and virtual-document projection
// observe the same Razor->C# mapping behavior.
var razorSdkToolsetHost = new RazorSdkToolsetHost();
var razorProjectionService = new RazorDesignTimeCodeProjectionService(razorSdkToolsetHost);
var inProcRoslynCodeService = new InProcRoslynCodeService(razorProjectionService);
var projectionService = new JazorProjectionService(inProcRoslynCodeService);
await using var denoVolarHost = new DenoVolarHost(DenoVolarHostOptionsParser.Parse(args));
var projectionResolver = new DocumentProjectionResolver(
    new DocumentRegionClassifier(),
    virtualDocumentRegistry);
var laneRouter = new LspLaneRouter();
var resultAggregator = new LspResultAggregator();
var markupComponentBridge = new MarkupComponentBridgeService(workspaceStore);
var markupBridgeFanoutCoordinator = new MarkupBridgeFanoutCoordinator(markupComponentBridge, resultAggregator);
var hostService = new VueHostService(
    workspaceStore,
    analysisClient,
    denoVolarHost);
var entry = new VueHostServiceEntry(hostService);
var rpcDispatcher = new VueHostRpcDispatcher(hostService);
var rpcProcessor = new VueHostRpcProcessor(rpcDispatcher);

await entry.RunAsync(cancellationToken);
try
{
    if (inspectRazorToolset)
    {
        Console.WriteLine(razorSdkToolsetHost.Describe());
        return;
    }

    if (!string.IsNullOrWhiteSpace(probeInProcRazorPath))
    {
        ProbeInProcRazor(probeInProcRazorPath, razorProjectionService);
        return;
    }

    if (useLsp)
    {
        DevHttpServer? devServer = null;
        if (useDev)
        {
            var devOptions = DevServerOptionsParser.Parse(args);
            devServer = CreateDevServer(devOptions, denoVolarHost);
            await devServer.StartAsync(cancellationToken);
            Console.Error.WriteLine($"Jazor.VueHost dev server listening on {devServer.ListeningUri ?? new Uri($"http://{devOptions.Host}:{devOptions.Port}")}");
        }

        var jazorDocumentService = new JazorLspDocumentService(workspaceStore, analysisClient, markupComponentBridge);
        ILspLane[] lanes =
        [
            new JazorLaneService(jazorDocumentService),
            new RoslynLaneService(workspaceStore, inProcRoslynCodeService),
            new VolarLaneService(workspaceStore, hostService, virtualDocumentRegistry, denoVolarHost, markupComponentBridge)
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
                markupBridgeFanoutCoordinator,
                new ReferenceCoordinator(laneMap, laneRouter, markupBridgeFanoutCoordinator),
                new RenameCoordinator(laneMap, laneRouter, resultAggregator, markupBridgeFanoutCoordinator),
                new CodeActionCoordinator(laneMap, laneRouter, resultAggregator),
                devServer));
        try
        {
            await lspServer.RunAsync(
                Console.OpenStandardInput(),
                Console.OpenStandardOutput(),
                cancellationToken);
        }
        finally
        {
            if (devServer is not null)
            {
                await devServer.DisposeAsync();
            }
        }

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

static void ProbeInProcRazor(
    string documentPath,
    RazorDesignTimeCodeProjectionService service)
{
    var fullPath = Path.GetFullPath(documentPath);
    if (!File.Exists(fullPath))
    {
        Console.WriteLine($"In-proc Razor probe: missing file '{fullPath}'.");
        return;
    }

    var document = new DocumentSnapshot(
        fullPath,
        DocumentKind.Jazor,
        File.ReadAllText(fullPath),
        version: null);

    if (!service.TryCreateProjection(document, out var projection))
    {
        Console.WriteLine("In-proc Razor probe: projection unavailable.");
        return;
    }

    Console.WriteLine("In-proc Razor probe: ok");
    Console.WriteLine($"  source:    {fullPath}");
    Console.WriteLine($"  projected: {projection.ProjectedDocumentPath}");
    Console.WriteLine($"  segments:  {projection.ProjectionMap.Segments.Count}");
    Console.WriteLine("  preview:");
    foreach (var line in projection.SourceText
                 .Replace("\r\n", "\n", StringComparison.Ordinal)
                 .Split('\n')
                 .Take(8))
    {
        Console.WriteLine("    " + line);
    }
}

static string? GetOptionValue(string[] args, string optionName)
{
    foreach (var arg in args)
    {
        if (arg.StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
        {
            return arg[(optionName.Length + 1)..];
        }
    }

    return null;
}

static DevHttpServer CreateDevServer(
    DevServerOptions devOptions,
    IDenoVolarHost denoFrontendHost)
{
    var moduleResolver = new ModuleResolver(devOptions.RootDirectory);
    IFrontendModuleCompiler frontendCompiler = string.Equals(
        devOptions.FrontendCompiler,
        "stub",
        StringComparison.OrdinalIgnoreCase)
        ? new StubFrontendModuleCompiler()
        : new DenoFrontendModuleCompiler(denoFrontendHost);
    var compiler = new OnDemandCompiler(
        new Jazor.Vue.JazorVueParser(),
        new Jazor.Vue.JazorVueCompiler(),
        frontendCompiler,
        new CompilationCache(),
        new DependencyGraph(moduleResolver),
        moduleResolver);
    return new DevHttpServer(
        devOptions,
        compiler,
        moduleResolver,
        new HtmlTransformer(devOptions));
}
