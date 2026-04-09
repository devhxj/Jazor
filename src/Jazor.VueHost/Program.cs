using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Hosting;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Razor.InProc;
using Jazor.VueHost.Razor.Toolset;
using Jazor.VueHost.LanguageServers;
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
var useAnalysisStdio = args.Any(static arg => string.Equals(arg, "--analysis-stdio", StringComparison.OrdinalIgnoreCase));
var inspectLanguageServers = args.Any(static arg => string.Equals(arg, "--inspect-language-servers", StringComparison.OrdinalIgnoreCase));
var inspectRazorToolset = args.Any(static arg => string.Equals(arg, "--inspect-razor-toolset", StringComparison.OrdinalIgnoreCase));
var probeLanguageServers = args.Any(static arg => string.Equals(arg, "--probe-language-servers", StringComparison.OrdinalIgnoreCase));
var probeInProcRazorPath = GetOptionValue(args, "--probe-inproc-razor");
var useExternalRoslyn = args.Any(static arg => string.Equals(arg, "--external-roslyn", StringComparison.OrdinalIgnoreCase));
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
// Keep one shared in-proc projection pipeline so LSP lanes and virtual-document projection
// observe the same Razor->C# mapping behavior.
var razorSdkToolsetHost = new RazorSdkToolsetHost();
var razorProjectionService = new RazorDesignTimeCodeProjectionService(razorSdkToolsetHost);
var inProcRoslynCodeService = new InProcRoslynCodeService(razorProjectionService);
var projectionService = new JazorProjectionService(inProcRoslynCodeService);
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
var lspLaneResources = new List<IAsyncDisposable>();
var languageServerCatalog = LanguageServerCatalog.CreateDefault();

await entry.RunAsync(cancellationToken);
try
{
    if (inspectLanguageServers)
    {
        PrintLanguageServerCatalog(languageServerCatalog);
        return;
    }

    if (inspectRazorToolset)
    {
        Console.WriteLine(razorSdkToolsetHost.Describe());
        return;
    }

    if (probeLanguageServers)
    {
        await ProbeLanguageServersAsync(languageServerCatalog, cancellationToken);
        return;
    }

    if (!string.IsNullOrWhiteSpace(probeInProcRazorPath))
    {
        ProbeInProcRazor(probeInProcRazorPath, razorProjectionService);
        return;
    }

    if (useLsp)
    {
        var jazorDocumentService = new JazorLspDocumentService(workspaceStore, analysisClient);
        ProjectedLanguageServerLaneHost? roslynLaneHost = null;
        if (useExternalRoslyn && languageServerCatalog.Roslyn is not null)
        {
            roslynLaneHost = new ProjectedLanguageServerLaneHost(
                rootPath: Directory.GetCurrentDirectory(),
                languageId: "csharp",
                virtualDocumentRegistry,
                new ExternalLspClient(languageServerCatalog.Roslyn));
            lspLaneResources.Add(roslynLaneHost);
        }

        var frontendFallbackLane = new FrontendLaneService(jazorDocumentService, denoFrontendHost);
        ProjectedLanguageServerLaneHost? volarLaneHost = null;
        if (languageServerCatalog.Volar is not null && languageServerCatalog.TypeScript is not null)
        {
            var tsServerClient = new TypeScriptServerClient(languageServerCatalog.TypeScript);
            lspLaneResources.Add(tsServerClient);
            var volarHandler = new VolarTsServerNotificationHandler(tsServerClient);
            var volarClient = new ExternalLspClient(
                languageServerCatalog.Volar,
                [
                    volarHandler
                ]);
            volarHandler.AttachClient(volarClient);
            volarLaneHost = new ProjectedLanguageServerLaneHost(
                rootPath: Directory.GetCurrentDirectory(),
                languageId: "vue",
                virtualDocumentRegistry,
                volarClient);
            lspLaneResources.Add(volarLaneHost);
        }

        ILspLane[] lanes =
        [
            new JazorLaneService(jazorDocumentService),
            new RoslynLaneService(jazorDocumentService, workspaceStore, inProcRoslynCodeService, host: roslynLaneHost),
            new VolarFrontendLaneService(volarLaneHost, frontendFallbackLane)
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
    foreach (var resource in lspLaneResources)
    {
        await resource.DisposeAsync();
    }

    await hostService.StopAsync(cancellationToken);
}

static void PrintLanguageServerCatalog(LanguageServerCatalog catalog)
{
    PrintProcess("Roslyn", catalog.Roslyn);
    PrintProcess("Volar", catalog.Volar);
    PrintProcess("TypeScript", catalog.TypeScript);

    Console.WriteLine("Razor integration:");
    Console.WriteLine($"  extension:         {catalog.RoslynExtensionAssemblyPath ?? "<missing>"}");
    Console.WriteLine($"  source generator:  {catalog.RazorSourceGeneratorPath ?? "<missing>"}");
    Console.WriteLine($"  razor targets:     {catalog.RazorDesignTimePath ?? "<missing>"}");
    Console.WriteLine($"  csharp targets:    {catalog.CSharpDesignTimePath ?? "<missing>"}");
    Console.WriteLine($"  servicehub root:   {catalog.RazorServiceHubRoot ?? "<missing>"}");
}

static async Task ProbeLanguageServersAsync(
    LanguageServerCatalog catalog,
    CancellationToken cancellationToken)
{
    PrintLanguageServerCatalog(catalog);
    Console.WriteLine();

    await ProbeLspServerAsync("Roslyn", catalog.Roslyn, cancellationToken);

    if (catalog.Volar is not null && catalog.TypeScript is not null)
    {
        Console.WriteLine();
        await ProbeVolarAsync(catalog.Volar, catalog.TypeScript, cancellationToken);
    }
}

static async Task ProbeLspServerAsync(
    string label,
    ExternalProcessOptions? options,
    CancellationToken cancellationToken)
{
    if (options is null)
    {
        Console.WriteLine($"{label} probe: skipped (not discovered)");
        return;
    }

    try
    {
        await using var client = new ExternalLspClient(options);
        var result = await client.InitializeAsync(Directory.GetCurrentDirectory(), cancellationToken);
        Console.WriteLine($"{label} probe: ok");
        Console.WriteLine($"  server: {result?.Name ?? "<unknown>"} {result?.Version}".TrimEnd());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{label} probe: failed");
        Console.WriteLine($"  detail: {ex.Message}");
    }
}

static async Task ProbeVolarAsync(
    ExternalProcessOptions volarOptions,
    ExternalProcessOptions typeScriptOptions,
    CancellationToken cancellationToken)
{
    try
    {
        await using var tsServerClient = new TypeScriptServerClient(typeScriptOptions);
        var volarHandler = new VolarTsServerNotificationHandler(tsServerClient);
        await using var volarClient = new ExternalLspClient(volarOptions, [volarHandler]);
        volarHandler.AttachClient(volarClient);
        var result = await volarClient.InitializeAsync(Directory.GetCurrentDirectory(), cancellationToken);
        Console.WriteLine("Volar probe: ok");
        Console.WriteLine($"  server: {result?.Name ?? "<unknown>"} {result?.Version}".TrimEnd());
    }
    catch (Exception ex)
    {
        Console.WriteLine("Volar probe: failed");
        Console.WriteLine($"  detail: {ex.Message}");
    }
}

static void PrintProcess(string label, ExternalProcessOptions? options)
{
    Console.WriteLine($"{label}:");
    if (options is null)
    {
        Console.WriteLine("  status: unavailable");
        return;
    }

    Console.WriteLine("  status: available");
    Console.WriteLine($"  file:   {options.FileName}");
    Console.WriteLine($"  args:   {string.Join(" ", options.Arguments)}");
    if (!string.IsNullOrWhiteSpace(options.WorkingDirectory))
    {
        Console.WriteLine($"  cwd:    {options.WorkingDirectory}");
    }
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
