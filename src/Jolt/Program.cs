using Jolt.Analysis;
using Jolt.Build;
using Jolt.Debug;
using Jolt.DevServer;
using Jolt.Extensions;
using Jolt.Extensions.Builtin;
using Jolt.Hosting;
using Jazor.Vue;
using Jolt.Jazor.Projection;
using Jolt.Lsp;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jazor.Common.VueContracts.Protocol;
using Jolt.Razor.InProc;
using Jolt.Razor.Toolset;
using Jolt.Roslyn.InProc;
using Jolt.Rpc;
using Jolt.Services;
using Jolt.SourceMap;
using Jolt.VirtualDocuments.Registry;
using Jolt.Volar.Deno.Hosting;
using Jolt.Workspace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using SharedJoltRpcMethodNames = Jazor.Common.VueContracts.Protocol.JoltRpcMethodNames;

var useLsp = args.Any(static arg => string.Equals(arg, "--lsp", StringComparison.OrdinalIgnoreCase));
var useDev = args.Any(static arg => string.Equals(arg, "--dev", StringComparison.OrdinalIgnoreCase));
var useBuild = args.Any(static arg => string.Equals(arg, "--build", StringComparison.OrdinalIgnoreCase));
var usePreview = args.Any(static arg => string.Equals(arg, "--preview", StringComparison.OrdinalIgnoreCase));
var useAnalysisStdio = args.Any(static arg => string.Equals(arg, "--analysis-stdio", StringComparison.OrdinalIgnoreCase));
var useDap = args.Any(static arg => string.Equals(arg, "--dap", StringComparison.OrdinalIgnoreCase));
var inspectRazorToolset = args.Any(static arg => string.Equals(arg, "--inspect-razor-toolset", StringComparison.OrdinalIgnoreCase));
var probeInProcRazorPath = GetOptionValue(args, "--probe-inproc-razor");
var useExtensionWorker = args.Any(static arg => string.Equals(arg, "--extension-worker", StringComparison.OrdinalIgnoreCase));
var useStdio = Console.IsInputRedirected
    || args.Any(static arg => string.Equals(arg, "--stdio", StringComparison.OrdinalIgnoreCase));
var cancellationToken = CancellationToken.None;

if (useExtensionWorker)
{
    var workerServer = new ExtensionWorkerServer();
    await workerServer.RunAsync(
        Console.OpenStandardInput(),
        Console.OpenStandardOutput(),
        cancellationToken);
    return;
}

if (useBuild)
{
    var rootDir = GetOptionValue(args, "--dev-root") ?? Directory.GetCurrentDirectory();
    rootDir = Path.GetFullPath(rootDir);
    if (!TryLoadJoltConfigForCommand(rootDir, out var config))
    {
        return;
    }

    if (!TryResolveBuildOptionsForCommand(args, rootDir, config, out var buildOptions))
    {
        return;
    }

    var orchestrator = new BuildOrchestrator();
    var result = await orchestrator.BuildAsync(buildOptions, cancellationToken);

    if (!result.Success)
    {
        Console.Error.WriteLine("Build failed:");
        foreach (var diag in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
        {
            Console.Error.WriteLine($"  {diag.Message}");
        }

        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine($"Build completed in {result.Duration.TotalSeconds:F2}s");
    Console.WriteLine($"Output: {result.OutDirectory}");
    if (!string.IsNullOrWhiteSpace(result.ManifestPath))
    {
        Console.WriteLine($"Manifest: {result.ManifestPath}");
    }
    Console.WriteLine($"Total size: {result.TotalSize / 1024:N0} KB");

    foreach (var diag in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning))
    {
        Console.WriteLine($"  Warning: {diag.Message}");
    }

    return;
}

if (usePreview)
{
    var rootDir = GetOptionValue(args, "--dev-root") ?? Directory.GetCurrentDirectory();
    rootDir = Path.GetFullPath(rootDir);
    if (!TryLoadJoltConfigForCommand(rootDir, out var config))
    {
        return;
    }

    if (!TryResolveOutputDirectoryForCommand(args, rootDir, config, out var distDir))
    {
        return;
    }

    if (!Directory.Exists(distDir))
    {
        Console.Error.WriteLine($"Preview output directory '{distDir}' not found. Run --build first.");
        Environment.ExitCode = 1;
        return;
    }

    var portValue = GetOptionValue(args, "--dev-port");
    var previewPort = portValue is not null && int.TryParse(portValue, out var p) ? p : 4173;
    Console.WriteLine($"Preview server running at http://localhost:{previewPort}");

    var builder = WebApplication.CreateSlimBuilder();
    builder.WebHost.UseUrls($"http://localhost:{previewPort}");

    var app = builder.Build();
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(distDir),
        RequestPath = "",
        EnableDirectoryBrowsing = false
    });
    app.MapFallbackToFile("index.html");

    using var shutdownSource = new CancellationTokenSource();
    ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        shutdownSource.Cancel();
    };
    Console.CancelKeyPress += cancelHandler;

    try
    {
        await app.StartAsync(shutdownSource.Token);
        Console.WriteLine("Press Ctrl+C to stop.");
        await Task.Delay(Timeout.InfiniteTimeSpan, shutdownSource.Token);
    }
    catch (OperationCanceledException)
    {
        // Ctrl+C shutdown.
    }
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
        await app.DisposeAsync();
    }

    return;
}

if (useAnalysisStdio)
{
    var analysisProcessor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
    var analysisServer = new StdioVueAnalysisRpcServer(analysisProcessor);
    await analysisServer.RunAsync(Console.In, Console.Out, cancellationToken);
    return;
}

if (useDap && !useLsp)
{
    await using var dapVolarHost = useDev ? new DenoVolarHost(DenoVolarHostOptionsParser.Parse(args)) : null;
    await using var devRuntime = useDev && dapVolarHost is not null
        ? CreateDevServerRuntime(DevServerOptionsParser.Parse(args), dapVolarHost)
        : null;

    if (devRuntime is not null)
    {
        await devRuntime.Server.StartAsync(cancellationToken);
        Console.Error.WriteLine($"Jolt dev server listening on {devRuntime.Server.ListeningUri ?? new Uri($"http://{devRuntime.Options.Host}:{devRuntime.Options.Port}")}");
    }

    var sourceMapService = devRuntime?.SourceMapService ?? new InMemorySourceMapService();
    var launchConfiguration = LaunchConfiguration.ResolveFromArgs(args, Directory.GetCurrentDirectory());
    await using var cdpClient = await TryCreateCdpClientAsync(launchConfiguration, cancellationToken);

    var dapSession = new DapSession(cdpClient);
    if (cdpClient is null)
    {
        dapSession.SeedFallbackPause();
    }

    var dapServer = new DapServer(
        new DapRequestHandler(
            dapSession,
            new BreakpointManager(sourceMapService),
            new CallStackMapper(sourceMapService)));
    await dapServer.RunStdioAsync(cancellationToken);
    return;
}

if (useDev && !useLsp)
{
    await using var devVolarHost = new DenoVolarHost(DenoVolarHostOptionsParser.Parse(args));
    var devOptions = DevServerOptionsParser.Parse(args);
    await using var devRuntime = CreateDevServerRuntime(devOptions, devVolarHost);
    var devServer = devRuntime.Server;
    using var shutdownSource = new CancellationTokenSource();
    ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        shutdownSource.Cancel();
    };
    Console.CancelKeyPress += cancelHandler;

    try
    {
        await devServer.StartAsync(shutdownSource.Token);
        Console.WriteLine($"Jolt dev server listening on {devServer.ListeningUri ?? new Uri($"http://{devOptions.Host}:{devOptions.Port}")}");
        await Task.Delay(Timeout.InfiniteTimeSpan, shutdownSource.Token);
    }
    catch (OperationCanceledException)
    {
        // Ctrl+C shutdown.
    }
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
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
var hostService = new JoltService(
    workspaceStore,
    analysisClient,
    denoVolarHost);
var entry = new JoltServiceEntry(hostService);
var rpcDispatcher = new JoltRpcDispatcher(hostService);
var rpcProcessor = new JoltRpcProcessor(rpcDispatcher);

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
        var hostRootDirectory = Path.GetFullPath(GetOptionValue(args, "--dev-root") ?? Directory.GetCurrentDirectory());
        if (!TryLoadJoltConfigForCommand(hostRootDirectory, out var hostConfig))
        {
            return;
        }

        var extensionHostOptions = ExtensionHostOptionsResolver.Resolve(args, hostRootDirectory, hostConfig);
        var extensionRegistry = new ExtensionRegistry(
            loadEventRetention: extensionHostOptions.LoadEventRetention,
            providerEventRetention: extensionHostOptions.ProviderEventRetention);
        ExtensionLoadLogPersistence.Replay(extensionRegistry, extensionHostOptions.LoadLogFilePath);
        ExtensionProviderLogPersistence.Replay(extensionRegistry, extensionHostOptions.ProviderLogFilePath);
        extensionRegistry.ProviderInvocationReported += invocation =>
        {
            WriteExtensionProviderLog(invocation);
            ExtensionProviderLogPersistence.Append(invocation, extensionHostOptions.ProviderLogFilePath);
        };
        await using var extensionLoader = new ExtensionLoader(
            extensionRegistry,
            invocation =>
            {
                WriteExtensionLoadLog(invocation);
                ExtensionLoadLogPersistence.Append(invocation, extensionHostOptions.LoadLogFilePath);
            });
        await extensionLoader.LoadBuiltinExtensionsAsync(
            builtinExtensions: BuiltinExtensionCatalog.Create(),
            rootDirectory: extensionHostOptions.RootDirectory,
            cancellationToken);
        await extensionLoader.LoadUserExtensionsAsync(extensionHostOptions, cancellationToken);

        DevServerRuntime? devRuntime = null;
        DevHttpServer? devServer = null;
        try
        {
            if (useDev)
            {
                var devOptions = DevServerOptionsParser.Parse(args);
                devRuntime = CreateWorkspaceDevServerRuntime(devOptions, denoVolarHost, workspaceStore);
                devServer = devRuntime.Server;
                await devServer.StartAsync(cancellationToken);
                Console.Error.WriteLine($"Jolt dev server listening on {devServer.ListeningUri ?? new Uri($"http://{devOptions.Host}:{devOptions.Port}")}");
            }

            var jazorDocumentService = new JazorLspDocumentService(workspaceStore, analysisClient, markupComponentBridge);
            ILspLane[] lanes =
            [
                new JazorLaneService(jazorDocumentService),
                new RoslynLaneService(workspaceStore, inProcRoslynCodeService),
                new VolarLaneService(workspaceStore, hostService, virtualDocumentRegistry, denoVolarHost, markupComponentBridge)
            ];
            var laneMap = lanes.ToDictionary(static lane => lane.LaneKind);
            var standardOutput = Console.OpenStandardOutput();
            var lspWriter = new LspMessageWriter(standardOutput);
            var lspServer = new StdioLspServer(
                new LspSession(
                    workspaceStore,
                    lanes,
                    laneRouter,
                    lspWriter,
                    projectionService,
                    virtualDocumentRegistry,
                    projectionResolver,
                    resultAggregator,
                    markupBridgeFanoutCoordinator,
                    new ReferenceCoordinator(laneMap, laneRouter, markupBridgeFanoutCoordinator),
                    new RenameCoordinator(laneMap, laneRouter, resultAggregator, markupBridgeFanoutCoordinator),
                    new CodeActionCoordinator(laneMap, laneRouter, resultAggregator),
                    devServer,
                    extensionRegistry),
                lspWriter);

            await lspServer.RunAsync(
                Console.OpenStandardInput(),
                standardOutput,
                cancellationToken);
        }
        finally
        {
            if (devRuntime is not null)
            {
                await devRuntime.DisposeAsync();
            }
        }

        return;
    }

    if (useStdio)
    {
        var stdioServer = new StdioJoltRpcServer(rpcProcessor);
        await stdioServer.RunAsync(Console.In, Console.Out, cancellationToken);
        return;
    }

    var responseJson = await rpcProcessor.ProcessAsync(
        JoltRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "startup",
            method: SharedJoltRpcMethodNames.GetHostInfo,
            payloadJson: null)),
        cancellationToken);

    Console.WriteLine("Jolt skeleton ready.");
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

static JazorConfig? LoadJoltConfig(string rootDirectory)
    => JoltConfigLoader.Load(rootDirectory);

static bool TryLoadJoltConfigForCommand(string rootDirectory, out JazorConfig? config)
{
    try
    {
        config = LoadJoltConfig(rootDirectory);
        return true;
    }
    catch (InvalidOperationException exception)
    {
        Console.Error.WriteLine(exception.Message);
        Environment.ExitCode = 1;
        config = null;
        return false;
    }
}

static bool TryResolveBuildOptionsForCommand(
    string[] args,
    string rootDirectory,
    JazorConfig? config,
    out BuildOptions buildOptions)
{
    try
    {
        buildOptions = BuildCommandOptionsResolver.ResolveBuildOptions(args, rootDirectory, config);
        return true;
    }
    catch (InvalidOperationException exception)
    {
        Console.Error.WriteLine(exception.Message);
        Environment.ExitCode = 1;
        buildOptions = new BuildOptions { RootDirectory = rootDirectory };
        return false;
    }
}

static bool TryResolveOutputDirectoryForCommand(
    string[] args,
    string rootDirectory,
    JazorConfig? config,
    out string outputDirectory)
{
    try
    {
        outputDirectory = BuildCommandOptionsResolver.ResolveOutputDirectory(args, rootDirectory, config);
        return true;
    }
    catch (InvalidOperationException exception)
    {
        Console.Error.WriteLine(exception.Message);
        Environment.ExitCode = 1;
        outputDirectory = string.Empty;
        return false;
    }
}

static void WriteExtensionLoadLog(ExtensionLoadInvocation invocation)
{
    var payload = new
    {
        eventType = "extensionLoad",
        invocation.Timestamp,
        invocation.Source,
        invocation.ExtensionId,
        invocation.Status,
        invocation.Reason,
        invocation.ExtensionDirectory,
        invocation.ManifestPath,
        invocation.AssemblyPath
    };

    Console.Error.WriteLine(JsonSerializer.Serialize(payload));
}

static void WriteExtensionProviderLog(ExtensionProviderInvocation invocation)
{
    var payload = new
    {
        eventType = "extensionProvider",
        timestamp = DateTimeOffset.UtcNow,
        invocation.ProviderName,
        invocation.Capability,
        durationMs = invocation.Duration.TotalMilliseconds,
        invocation.Succeeded,
        invocation.TimedOut,
        invocation.Skipped,
        invocation.ErrorMessage
    };

    Console.Error.WriteLine(JsonSerializer.Serialize(payload));
}

static DevServerRuntime CreateDevServerRuntime(
    DevServerOptions devOptions,
    IDenoVolarHost denoVolarHost)
{
    return CreateDevServerRuntimeCore(devOptions, denoVolarHost, workspaceStore: null);
}

static DevServerRuntime CreateWorkspaceDevServerRuntime(
    DevServerOptions devOptions,
    IDenoVolarHost denoVolarHost,
    IJoltWorkspaceStore workspaceStore)
{
    ArgumentNullException.ThrowIfNull(workspaceStore);
    return CreateDevServerRuntimeCore(devOptions, denoVolarHost, workspaceStore);
}

static DevServerRuntime CreateDevServerRuntimeCore(
    DevServerOptions devOptions,
    IDenoVolarHost denoVolarHost,
    IJoltWorkspaceStore? workspaceStore)
{
    // Null is intentional for standalone dev/DAP modes where no LSP workspace exists.
    // LSP+dev callers must use the non-null overload above so workspace-backed HMR is wired.
    var moduleResolver = new ModuleResolver(devOptions.RootDirectory, devOptions.ResolveAliases);
    var sourceMapService = new InMemorySourceMapService();
    IVolarModuleCompiler frontendCompiler = string.Equals(
        devOptions.VolarCompiler,
        "stub",
        StringComparison.OrdinalIgnoreCase)
        ? new StubVolarModuleCompiler()
        : new DenoVolarModuleCompiler(denoVolarHost);
    var compiler = new OnDemandCompiler(
        new JazorVueParser(),
        new JazorVueCompiler(),
        frontendCompiler,
        new CompilationCache(),
        new DependencyGraph(moduleResolver),
        moduleResolver,
        sourceMapService: sourceMapService);
    return new DevServerRuntime(
        new DevHttpServer(
            devOptions,
            compiler,
            moduleResolver,
            new HtmlTransformer(devOptions),
            workspaceStore),
        sourceMapService,
        devOptions);
}

static async Task<CdpClient?> TryCreateCdpClientAsync(
    LaunchConfiguration? launchConfiguration,
    CancellationToken cancellationToken)
{
    var endpoint = launchConfiguration?.CdpWebSocketUrl;
    if (string.IsNullOrWhiteSpace(endpoint))
    {
        return null;
    }

    if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
    {
        Console.Error.WriteLine($"DAP: ignoring invalid CDP endpoint '{endpoint}'.");
        return null;
    }

    var cdpClient = new CdpClient(new CdpConnection());
    try
    {
        await cdpClient.ConnectAsync(endpointUri, cancellationToken);
        await cdpClient.InitializeAsync(cancellationToken);
        Console.Error.WriteLine($"DAP: connected to CDP endpoint {endpointUri}.");
        return cdpClient;
    }
    catch (ObjectDisposedException exception)
    {
        Console.Error.WriteLine($"DAP: failed to connect CDP endpoint '{endpointUri}': {exception.Message}");
        await cdpClient.DisposeAsync();
        return null;
    }
    catch (InvalidOperationException exception)
    {
        Console.Error.WriteLine($"DAP: failed to connect CDP endpoint '{endpointUri}': {exception.Message}");
        await cdpClient.DisposeAsync();
        return null;
    }
    catch (OperationCanceledException exception)
    {
        Console.Error.WriteLine($"DAP: failed to connect CDP endpoint '{endpointUri}': {exception.Message}");
        await cdpClient.DisposeAsync();
        return null;
    }
    catch (System.Net.WebSockets.WebSocketException exception)
    {
        Console.Error.WriteLine($"DAP: failed to connect CDP endpoint '{endpointUri}': {exception.Message}");
        await cdpClient.DisposeAsync();
        return null;
    }
    catch (IOException exception)
    {
        Console.Error.WriteLine($"DAP: failed to connect CDP endpoint '{endpointUri}': {exception.Message}");
        await cdpClient.DisposeAsync();
        return null;
    }
    catch (System.Text.Json.JsonException exception)
    {
        Console.Error.WriteLine($"DAP: failed to connect CDP endpoint '{endpointUri}': {exception.Message}");
        await cdpClient.DisposeAsync();
        return null;
    }
    catch (NotSupportedException exception)
    {
        Console.Error.WriteLine($"DAP: failed to connect CDP endpoint '{endpointUri}': {exception.Message}");
        await cdpClient.DisposeAsync();
        return null;
    }
}

file sealed class DevServerRuntime(
    DevHttpServer server,
    ISourceMapService sourceMapService,
    DevServerOptions options) : IAsyncDisposable
{
    public DevHttpServer Server { get; } = server;

    public ISourceMapService SourceMapService { get; } = sourceMapService;

    public DevServerOptions Options { get; } = options;

    public ValueTask DisposeAsync()
        => Server.DisposeAsync();
}
