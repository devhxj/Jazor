using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Workspace;

namespace Jazor.VueHost.DevServer;

internal sealed class DevHttpServer : IAsyncDisposable, IWorkspaceDocumentChangeSink
{
    private static readonly TimeSpan FileChangeDebounceInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan FileChangePollingInterval = TimeSpan.FromSeconds(1);

    private readonly DevServerOptions _options;
    private readonly OnDemandCompiler _compiler;
    private readonly ModuleResolver _moduleResolver;
    private readonly HtmlTransformer _htmlTransformer;
    private readonly IVueHostWorkspaceStore? _workspaceStore;
    private readonly DevServerProxy? _proxy;
    private readonly DevServerReloadHub _reloadHub = new();
    private readonly Channel<IReadOnlyList<string>> _fileChangeChannel = Channel.CreateUnbounded<IReadOnlyList<string>>();
    private readonly Dictionary<string, DevServerObservedFileSnapshot?> _lastBroadcastSnapshots = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _pendingWorkspaceBroadcastHashes = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lastBroadcastSnapshotsLock = new();
    private readonly ChangeProcessor _changeProcessor;
    private WebApplication? _application;
    private FileSystemWatcher? _fileWatcher;
    private FileChangeDebouncer? _fileChangeDebouncer;
    private DevServerFileSnapshotPoller? _fileSnapshotPoller;
    private Task? _fileChangePump;
    private CancellationTokenSource? _fileChangeCancellationSource;

    public Uri? ListeningUri { get; private set; }

    public DevHttpServer(
        DevServerOptions options,
        OnDemandCompiler compiler,
        ModuleResolver moduleResolver,
        HtmlTransformer htmlTransformer,
        IVueHostWorkspaceStore? workspaceStore = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _moduleResolver = moduleResolver ?? throw new ArgumentNullException(nameof(moduleResolver));
        _htmlTransformer = htmlTransformer ?? throw new ArgumentNullException(nameof(htmlTransformer));
        _workspaceStore = workspaceStore;
        _proxy = options.ProxyRules.Count == 0 ? null : new DevServerProxy(options.ProxyRules);
        _changeProcessor = new ChangeProcessor(
            _compiler,
            _moduleResolver,
            compiler.DependencyGraph ?? new DependencyGraph(_moduleResolver));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_application is not null)
        {
            return;
        }

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls($"http://{_options.Host}:{_options.Port}");

        var application = builder.Build();
        if (_options.HmrEnabled || _options.ProxyRules.Values.Any(static target => target.WebSocket))
        {
            application.UseWebSockets();
        }

        if (_options.HmrEnabled)
        {
            application.Map(
                "/@jazor/hmr",
                async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    using var socket = await context.WebSockets.AcceptWebSocketAsync();
                    await _reloadHub.AcceptAsync(socket, context.RequestAborted);
                });
        }

        application.MapGet(
            "/@jazor/client",
            static (HttpContext context) =>
            {
                ApplyNoCacheHeaders(context.Response);
                return Results.Text(HtmlTransformer.GetDevClientScript(), "text/javascript");
            });
        application.Map(
            "/{**requestPath}",
            async (HttpContext context) =>
            {
                if (_proxy is not null && await _proxy.TryProxyAsync(context))
                {
                    return;
                }

                if (!HttpMethods.IsGet(context.Request.Method)
                    && !HttpMethods.IsHead(context.Request.Method))
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    return;
                }

                var result = await HandleRequestAsync(context);
                await result.ExecuteAsync(context);
            });

        await application.StartAsync(cancellationToken);
        _application = application;
        ListeningUri = ResolveListeningUri(application);
        StartFileWatcher();
    }

    public async ValueTask DisposeAsync()
    {
        _fileWatcher?.Dispose();
        _fileWatcher = null;
        _fileChangeDebouncer?.Dispose();
        _fileChangeDebouncer = null;
        if (_fileSnapshotPoller is not null)
        {
            await _fileSnapshotPoller.DisposeAsync();
            _fileSnapshotPoller = null;
        }

        if (_fileChangeCancellationSource is not null)
        {
            await _fileChangeCancellationSource.CancelAsync();
            _fileChangeCancellationSource.Dispose();
            _fileChangeCancellationSource = null;
        }

        if (_fileChangePump is not null)
        {
            try
            {
                await _fileChangePump;
            }
            catch (OperationCanceledException)
            {
            }

            _fileChangePump = null;
        }

        await _reloadHub.DisposeAsync();
        _proxy?.Dispose();

        if (_application is null)
        {
            return;
        }

        await _application.DisposeAsync();
        _application = null;
        ListeningUri = null;
    }

    private async Task<IResult> HandleRequestAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.HasValue
            ? context.Request.Path.Value!
            : "/";

        if (TryGetSourceMapRequestPath(requestPath, out var sourceRequestPath))
        {
            ApplyNoCacheHeaders(context.Response);
            return await HandleSourceMapRequestAsync(sourceRequestPath, context.RequestAborted);
        }

        var resolved = _moduleResolver.Resolve(requestPath);
        if (!resolved.Found)
        {
            return Results.NotFound(resolved.Error);
        }

        ApplyNoCacheHeaders(context.Response);

        if (resolved.ResolvedUrl.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            var html = await File.ReadAllTextAsync(resolved.AbsolutePath, context.RequestAborted);
            return Results.Text(_htmlTransformer.Transform(html, resolved.AbsolutePath), "text/html");
        }

        var result = await CompileResolvedRequestAsync(resolved.AbsolutePath, context.RequestAborted);
        if (result.IsError)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        return Results.Text(result.Content, result.ContentType);
    }

    private async Task<IResult> HandleSourceMapRequestAsync(
        string sourceRequestPath,
        CancellationToken cancellationToken)
    {
        var resolved = _moduleResolver.Resolve(sourceRequestPath);
        if (!resolved.Found)
        {
            return Results.NotFound(resolved.Error);
        }

        var result = await CompileResolvedRequestAsync(resolved.AbsolutePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(result.SourceMap))
        {
            return Results.NotFound($"Source map for '{sourceRequestPath}' is not available.");
        }

        return Results.Text(result.SourceMap, "application/json");
    }

    private async Task<CompilationResult> CompileResolvedRequestAsync(
        string absolutePath,
        CancellationToken cancellationToken)
    {
        if (_workspaceStore is null)
        {
            return await _compiler.CompileAsync(absolutePath, cancellationToken);
        }

        var trackedDocument = await _workspaceStore.GetDocumentAsync(absolutePath, cancellationToken);
        if (!absolutePath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return trackedDocument is null
                ? await _compiler.CompileAsync(absolutePath, cancellationToken)
                : await _compiler.CompileAsync(absolutePath, trackedDocument.Text, cancellationToken);
        }

        var trackedCompanionDocuments = await GetTrackedCompanionDocumentsAsync(absolutePath, cancellationToken);
        if (trackedDocument is not null)
        {
            return await _compiler.CompileAsync(absolutePath, trackedDocument.Text, trackedCompanionDocuments, cancellationToken);
        }

        if (trackedCompanionDocuments.Count > 0)
        {
            return await _compiler.CompileAsync(
                absolutePath,
                await File.ReadAllTextAsync(absolutePath, cancellationToken),
                trackedCompanionDocuments,
                cancellationToken);
        }

        return await _compiler.CompileAsync(absolutePath, cancellationToken);
    }

    private async Task<IReadOnlyList<DocumentSnapshot>> GetTrackedCompanionDocumentsAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        if (_workspaceStore is null || !documentPath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<DocumentSnapshot>();
        }

        var trackedDocuments = await _workspaceStore.GetDocumentsAsync(
            VueHostWorkspaceResolver.GetCoLocatedCodeBehindPaths(documentPath)
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            cancellationToken);

        return trackedDocuments
            .Where(static document => document.DocumentKind == DocumentKind.CSharp)
            .Select(static document => new DocumentSnapshot(
                Path.GetFullPath(document.DocumentPath),
                document.DocumentKind,
                document.Text,
                document.Version))
            .ToArray();
    }

    private void StartFileWatcher()
    {
        if (!_options.HmrEnabled || !Directory.Exists(_options.RootDirectory))
        {
            return;
        }

        _fileChangeCancellationSource = new CancellationTokenSource();
        _fileChangePump = PumpFileChangesAsync(_fileChangeCancellationSource.Token);
        _fileChangeDebouncer = new FileChangeDebouncer(FileChangeDebounceInterval);
        _fileChangeDebouncer.DebouncedChange += OnDebouncedFileChanges;
        _fileWatcher = new FileSystemWatcher(_options.RootDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                | NotifyFilters.DirectoryName
                | NotifyFilters.LastWrite
                | NotifyFilters.Size
        };
        _fileWatcher.Changed += OnFileChanged;
        _fileWatcher.Created += OnFileChanged;
        _fileWatcher.Deleted += OnFileChanged;
        _fileWatcher.Renamed += OnFileRenamed;
        _fileWatcher.EnableRaisingEvents = true;
        _fileSnapshotPoller = new DevServerFileSnapshotPoller(
            _options.RootDirectory,
            FileChangePollingInterval,
            OnDebouncedFileChanges);
        _fileSnapshotPoller.Start();
    }

    private void OnFileChanged(object sender, FileSystemEventArgs eventArgs)
        => QueueFileChange(eventArgs.FullPath);

    private void OnFileRenamed(object sender, RenamedEventArgs eventArgs)
    {
        QueueFileChange(eventArgs.OldFullPath);
        QueueFileChange(eventArgs.FullPath);
    }

    private void QueueFileChange(string path)
    {
        if (DevServerFileWatchFilter.ShouldObserve(_options.RootDirectory, path))
        {
            _fileChangeDebouncer?.Record(path);
        }
    }

    private void OnDebouncedFileChanges(IReadOnlyList<string> changedPaths)
        => _fileChangeChannel.Writer.TryWrite(changedPaths);

    private async Task PumpFileChangesAsync(CancellationToken cancellationToken)
    {
        await foreach (var changedPaths in _fileChangeChannel.Reader.ReadAllAsync(cancellationToken))
        {
            await ProcessAndBroadcastChangesAsync(changedPaths, cancellationToken);
        }
    }

    public async ValueTask OnWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => await OnWorkspaceDocumentChangedAsync(document, [document], cancellationToken);

    public async ValueTask OnWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(openDocuments);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.HmrEnabled || _application is null)
        {
            return;
        }

        if (document.DocumentKind is not (
                DocumentKind.Jazor
                or DocumentKind.Vue
                or DocumentKind.JavaScript
                or DocumentKind.TypeScript
                or DocumentKind.Css
                or DocumentKind.CSharp))
        {
            return;
        }

        var fullPath = Path.GetFullPath(document.DocumentPath);
        if (!DevServerFileWatchFilter.ShouldObserve(_options.RootDirectory, fullPath))
        {
            return;
        }

        if (document.DocumentKind is DocumentKind.Jazor
            or DocumentKind.Vue
            or DocumentKind.JavaScript
            or DocumentKind.TypeScript
            or DocumentKind.Css)
        {
            var normalizedDocument = new DocumentSnapshot(fullPath, document.DocumentKind, document.Text, document.Version);
            if (ShouldSuppressWorkspaceBroadcastForDiskSyncedSnapshot(normalizedDocument))
            {
                return;
            }

            await ProcessAndBroadcastWorkspaceDocumentChangeAsync(normalizedDocument, openDocuments, cancellationToken);
            return;
        }

        if (document.DocumentKind == DocumentKind.CSharp
            && VueHostWorkspaceResolver.TryResolveOwningJazorPath(fullPath, out _))
        {
            var normalizedDocument = new DocumentSnapshot(fullPath, document.DocumentKind, document.Text, document.Version);
            if (ShouldSuppressWorkspaceBroadcastForDiskSyncedSnapshot(normalizedDocument))
            {
                return;
            }

            await ProcessAndBroadcastWorkspaceDocumentChangeAsync(normalizedDocument, openDocuments, cancellationToken);
            return;
        }

        if (!File.Exists(fullPath))
        {
            return;
        }

        var diskText = await File.ReadAllTextAsync(fullPath, cancellationToken);
        if (!string.Equals(diskText, document.Text, StringComparison.Ordinal))
        {
            return;
        }

        await ProcessAndBroadcastChangesAsync([fullPath], cancellationToken);
    }

    private async Task ProcessAndBroadcastChangesAsync(
        IReadOnlyList<string> changedPaths,
        CancellationToken cancellationToken)
    {
        var changedPathsToProcess = FilterAlreadyBroadcastChanges(changedPaths);
        if (changedPathsToProcess.Count == 0)
        {
            return;
        }

        var result = await _changeProcessor.ProcessChangesAsync(changedPathsToProcess, cancellationToken);
        RecordBroadcastSnapshots(result.ChangedPaths);
        await BroadcastChangeResultAsync(result, cancellationToken);
    }

    private async Task ProcessAndBroadcastWorkspaceDocumentChangeAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        // Register the workspace hash before compilation so watcher events emitted by
        // the same disk write can be suppressed even if they are processed first.
        RecordPendingWorkspaceBroadcastHash(document.DocumentPath, document.Text);
        var result = await _changeProcessor.ProcessWorkspaceDocumentChangeAsync(document, openDocuments, cancellationToken);
        RecordBroadcastSnapshots(result.ChangedPaths);
        await BroadcastChangeResultAsync(result, cancellationToken);
    }

    private async Task BroadcastChangeResultAsync(
        ChangeProcessingResult result,
        CancellationToken cancellationToken)
    {
        if (result.UpdateKind == ChangeUpdateKind.StyleUpdate)
        {
            await _reloadHub.BroadcastStyleUpdateAsync(
                result.ChangedCssUrls,
                result.InlineStyleUpdates,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                cancellationToken);
            return;
        }

        if (result.UpdateKind == ChangeUpdateKind.JavaScriptUpdate)
        {
            await _reloadHub.BroadcastJavaScriptUpdateAsync(
                result.JavaScriptUpdates,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                cancellationToken);
            return;
        }

        if (result.UpdateKind == ChangeUpdateKind.Error)
        {
            await _reloadHub.BroadcastErrorAsync(result.ErrorMessage, cancellationToken);
            return;
        }

        await _reloadHub.BroadcastReloadAsync(result.FullReloadReason, cancellationToken);
    }

    private IReadOnlyList<string> FilterAlreadyBroadcastChanges(IReadOnlyList<string> changedPaths)
    {
        if (changedPaths.Count == 0)
        {
            return changedPaths;
        }

        var pathsToProcess = new List<string>(changedPaths.Count);
        lock (_lastBroadcastSnapshotsLock)
        {
            foreach (var path in changedPaths
                         .Where(static path => !string.IsNullOrWhiteSpace(path))
                         .Select(Path.GetFullPath)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Order(StringComparer.OrdinalIgnoreCase))
            {
                var snapshot = CaptureObservedFileSnapshot(path);
                if (_lastBroadcastSnapshots.TryGetValue(path, out var previousSnapshot)
                    && Nullable.Equals(previousSnapshot, snapshot))
                {
                    continue;
                }

                if (_pendingWorkspaceBroadcastHashes.TryGetValue(path, out var pendingHash)
                    && TryComputeFileContentHash(path, out var currentHash)
                    && string.Equals(pendingHash, currentHash, StringComparison.Ordinal))
                {
                    _pendingWorkspaceBroadcastHashes.Remove(path);
                    _lastBroadcastSnapshots[path] = snapshot;
                    continue;
                }

                pathsToProcess.Add(path);
            }
        }

        return pathsToProcess;
    }

    private void RecordBroadcastSnapshots(IReadOnlyList<string> changedPaths)
    {
        if (changedPaths.Count == 0)
        {
            return;
        }

        lock (_lastBroadcastSnapshotsLock)
        {
            foreach (var path in changedPaths
                         .Where(static path => !string.IsNullOrWhiteSpace(path))
                         .Select(Path.GetFullPath)
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                _lastBroadcastSnapshots[path] = CaptureObservedFileSnapshot(path);
            }
        }
    }

    private void RecordPendingWorkspaceBroadcastHash(string path, string text)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        lock (_lastBroadcastSnapshotsLock)
        {
            _pendingWorkspaceBroadcastHashes[Path.GetFullPath(path)] = ComputeContentHash(text);
        }
    }

    private bool ShouldSuppressWorkspaceBroadcastForDiskSyncedSnapshot(DocumentSnapshot document)
    {
        var fullPath = Path.GetFullPath(document.DocumentPath);
        if (!TryComputeFileContentHash(fullPath, out var diskHash))
        {
            return false;
        }

        var workspaceHash = ComputeContentHash(document.Text);
        if (!string.Equals(diskHash, workspaceHash, StringComparison.Ordinal))
        {
            return false;
        }

        var snapshot = CaptureObservedFileSnapshot(fullPath);
        lock (_lastBroadcastSnapshotsLock)
        {
            return _lastBroadcastSnapshots.TryGetValue(fullPath, out var previousSnapshot)
                && Nullable.Equals(previousSnapshot, snapshot);
        }
    }

    private static DevServerObservedFileSnapshot? CaptureObservedFileSnapshot(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            return fileInfo.Exists
                ? new DevServerObservedFileSnapshot(fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks)
                : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static bool TryComputeFileContentHash(string path, out string contentHash)
    {
        contentHash = string.Empty;
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            contentHash = ComputeContentHash(File.ReadAllText(path));
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static string ComputeContentHash(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }

    private static Uri? ResolveListeningUri(WebApplication application)
    {
        foreach (var address in application.Urls)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                return uri;
            }
        }

        return null;
    }

    private static bool TryGetSourceMapRequestPath(string requestPath, out string sourceRequestPath)
    {
        if (requestPath.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            sourceRequestPath = requestPath[..^4];
            return !string.IsNullOrWhiteSpace(sourceRequestPath);
        }

        sourceRequestPath = string.Empty;
        return false;
    }

    private static void ApplyNoCacheHeaders(HttpResponse response)
    {
        response.Headers.CacheControl = "no-store";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
    }
}

internal readonly record struct DevServerObservedFileSnapshot(
    long Length,
    long LastWriteTimeUtcTicks);
