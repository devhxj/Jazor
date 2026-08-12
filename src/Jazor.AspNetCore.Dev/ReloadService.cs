using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;

namespace Jazor.AspNetCore.Dev;

/// <summary>Coordinates file observation, HMR classification, and browser reload delivery.</summary>
internal sealed class ReloadService : IHostedService, IAsyncDisposable
{
    internal const string PathBaseAttributeName = "data-jazor-path-base";

    private readonly JazorReloadOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly IReloadRuntimeSignals _runtimeSignals;
    private readonly ILogger<ReloadService> _logger;
    private readonly ReloadHub _reloadHub = new();
    // Watcher callbacks and polling callbacks can arrive concurrently. The queue collapses
    // bursts before manifest classification, preserving one ordered reload decision per batch.
    private readonly PathChangeQueue _fileChangeQueue = new();
    private readonly Dictionary<string, ObservedFileSnapshot?> _lastBroadcastSnapshots = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lastBroadcastSnapshotsLock = new();
    private readonly string _serverInstanceId = Guid.NewGuid().ToString("N")[..8];
    private readonly List<FileSystemWatcher> _fileWatchers = [];
    private readonly List<FileSnapshotPoller> _fileSnapshotPollers = [];
    private readonly List<WatchRootRegistration> _watchRegistrations = [];
    private readonly List<HmrArtifactRegistration> _hmrArtifactRegistrations = [];
    private FileChangeDebouncer? _fileChangeDebouncer;
    private HmrManifestTracker? _hmrManifestTracker;
    private Task? _fileChangePump;
    private CancellationTokenSource? _fileChangeCancellationSource;
    private long _reloadSequence;
    private int _disposeState;

    public ReloadService(
        IOptions<JazorReloadOptions> options,
        IWebHostEnvironment environment,
        IReloadRuntimeSignals runtimeSignals,
        ILogger<ReloadService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _runtimeSignals = runtimeSignals ?? throw new ArgumentNullException(nameof(runtimeSignals));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ClientScriptContent = ReloadClientScript.Build(
            _options.WebSocketPath.Value!,
            PathBaseAttributeName,
            _options.SuppressReconnectReloadForExternalRefresh
                && _runtimeSignals.IsExternalBrowserRefreshActive);
    }

    public JazorReloadOptions Options => _options;

    public bool IsEnabled => _environment.IsDevelopment();

    public string ClientScriptContent { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!IsEnabled)
            return;

        if (_fileChangePump is not null)
            return;

        // Resolve paths once at startup so all watcher and manifest comparisons use absolute paths.
        ResolveHmrArtifactRegistrations();
        // HMR eligibility is derived from two manifest snapshots, never from file names alone.
        // That keeps logic changes on the full-reload path even when a .mjs file was updated.
        _hmrManifestTracker = new HmrManifestTracker(_hmrArtifactRegistrations);
        _hmrManifestTracker.Initialize();
        ResolveWatchRegistrations();
        _fileChangeCancellationSource = new CancellationTokenSource();
        _fileChangePump = PumpFileChangesAsync(_fileChangeCancellationSource.Token);
        _fileChangeDebouncer = new FileChangeDebouncer(_options.DebounceInterval);
        _fileChangeDebouncer.DebouncedChange += OnDebouncedFileChanges;

        foreach (var registration in _watchRegistrations)
        {
            TryStartFileWatcher(registration);
            var snapshotPoller = new FileSnapshotPoller(
                registration.RootPath,
                _options.PollingInterval,
                OnDebouncedFileChanges);
            snapshotPoller.Start();
            _fileSnapshotPollers.Add(snapshotPoller);
        }

        if (_watchRegistrations.Count == 0)
        {
            _logger.LogDebug("Jazor reload started without file watchers because no watch roots were configured.");
            return;
        }

        _logger.LogDebug(
            "Jazor reload watching {WatchRootCount} roots for browser reload.",
            _watchRegistrations.Count);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
    }

    public async Task AcceptWebSocketAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(socket);

        await _reloadHub.AcceptAsync(
            socket,
            _serverInstanceId,
            Volatile.Read(ref _reloadSequence),
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
            return;

        foreach (var fileWatcher in _fileWatchers)
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Changed -= OnFileChanged;
            fileWatcher.Created -= OnFileChanged;
            fileWatcher.Deleted -= OnFileChanged;
            fileWatcher.Renamed -= OnFileRenamed;
            fileWatcher.Dispose();
        }
        _fileWatchers.Clear();

        if (_fileChangeDebouncer is not null)
        {
            _fileChangeDebouncer.DebouncedChange -= OnDebouncedFileChanges;
            _fileChangeDebouncer.Dispose();
            _fileChangeDebouncer = null;
        }

        foreach (var fileSnapshotPoller in _fileSnapshotPollers)
        {
            await fileSnapshotPoller.DisposeAsync();
        }
        _fileSnapshotPollers.Clear();

        if (_fileChangeCancellationSource is not null)
        {
            var cancellationSource = _fileChangeCancellationSource;
            _fileChangeCancellationSource = null;
            try
            {
                await cancellationSource.CancelAsync();
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                cancellationSource.Dispose();
            }
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
        _hmrManifestTracker = null;
    }

    private void ResolveHmrArtifactRegistrations()
    {
        _hmrArtifactRegistrations.Clear();
        foreach (var mapping in _options.HmrMappings.Where(static mapping => mapping is not null))
        {
            var artifactRootPath = ResolveContentRootPath(mapping.ArtifactRootPath);
            var requestPath = mapping.RequestPath.Value?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(requestPath))
                continue;

            _hmrArtifactRegistrations.Add(new HmrArtifactRegistration(
                artifactRootPath,
                Path.Combine(artifactRootPath, "jazor-manifest.json"),
                requestPath));
        }
    }

    private void ResolveWatchRegistrations()
    {
        _watchRegistrations.Clear();
        var suppressExternalBrowserRefreshRoots =
            _options.SuppressExternalRefreshPaths
            && _runtimeSignals.IsExternalBrowserRefreshActive;

        // An HMR mapping is not useful unless its artifacts are observed. Include it
        // automatically so custom output roots do not require duplicated configuration.
        var configuredRoots = _options.WatchPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(static path => path.Trim())
            .Concat(_hmrArtifactRegistrations.Select(static registration => registration.ArtifactRootPath))
            .Select(ResolveContentRootPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path.Length)
            .ThenBy(static path => path, StringComparer.OrdinalIgnoreCase);

        foreach (var rootPath in configuredRoots)
        {
            if (_watchRegistrations.Any(registration => IsSamePathOrDescendant(rootPath, registration.RootPath)))
                continue;

            if (suppressExternalBrowserRefreshRoots && IsHandledByExternalBrowserRefresh(rootPath))
            {
                _logger.LogDebug(
                    "Jazor reload skipped watch root '{WatchRoot}' because an external browser refresh pipeline already owns that static root.",
                    rootPath);
                continue;
            }

            var watcherPath = ResolveWatcherPath(rootPath);
            if (watcherPath is null)
            {
                _logger.LogDebug(
                    "Jazor reload skipped watch root '{WatchRoot}' because no existing ancestor directory could be resolved.",
                    rootPath);
                continue;
            }

            _watchRegistrations.Add(new WatchRootRegistration(rootPath, watcherPath));
        }
    }

    private string ResolveContentRootPath(string configuredPath)
        => Path.GetFullPath(
            Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(_environment.ContentRootPath, configuredPath));

    private bool IsHandledByExternalBrowserRefresh(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
            return false;

        return IsSamePathOrDescendant(
            Path.GetFullPath(rootPath),
            Path.GetFullPath(_environment.WebRootPath));
    }

    private void TryStartFileWatcher(WatchRootRegistration registration)
    {
        try
        {
            var watcher = new FileSystemWatcher(registration.WatcherPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Size
            };
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileChanged;
            watcher.Renamed += OnFileRenamed;
            watcher.EnableRaisingEvents = true;
            _fileWatchers.Add(watcher);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            _logger.LogWarning(
                exception,
                "Jazor reload could not start a file watcher for '{WatchRoot}'. Snapshot polling will remain active.",
                registration.RootPath);
        }
    }

    private static string? ResolveWatcherPath(string rootPath)
    {
        var currentDirectory = Path.GetFullPath(rootPath);
        while (!Directory.Exists(currentDirectory))
        {
            var parentDirectory = Directory.GetParent(currentDirectory);
            if (parentDirectory is null)
                return null;

            currentDirectory = parentDirectory.FullName;
        }

        return currentDirectory;
    }

    private static bool IsSamePathOrDescendant(string path, string ancestorPath)
    {
        var relativePath = Path.GetRelativePath(ancestorPath, path);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
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
        if (!ShouldObservePath(path))
            return;

        try
        {
            _fileChangeDebouncer?.Record(path);
        }
        catch (ObjectDisposedException)
        {
            // Shutdown can race file watcher callbacks; ignore late events.
        }
    }

    private bool ShouldObservePath(string path)
    {
        foreach (var registration in _watchRegistrations)
        {
            if (FileWatchFilter.ShouldObserve(registration.RootPath, path))
                return true;
        }

        return false;
    }

    private void OnDebouncedFileChanges(IReadOnlyList<string> changedPaths)
        => _fileChangeQueue.Enqueue(changedPaths);

    private async Task PumpFileChangesAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var changedPaths = await _fileChangeQueue.DequeueAsync(cancellationToken);
            if (changedPaths.Count == 0)
                continue;

            try
            {
                await BroadcastReloadForChangedPathsAsync(changedPaths, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogDebug(
                    exception,
                    "Jazor reload ignored a file-change processing failure for browser broadcast.");
            }
        }
    }

    private async Task BroadcastReloadForChangedPathsAsync(
        IReadOnlyList<string> changedPaths,
        CancellationToken cancellationToken)
    {
        var changedPathsToProcess = FilterAlreadyBroadcastChanges(changedPaths);
        if (changedPathsToProcess.Count == 0)
            return;

        RecordBroadcastSnapshots(changedPathsToProcess);
        var hmrDecision = _hmrManifestTracker?.Evaluate(changedPathsToProcess)
            ?? HmrDecision.FullReload("hmr-metadata-unavailable");
        _logger.LogDebug(
            "Jazor reload selected {ReloadKind} ({ReloadReason}) for {ChangedPathCount} observed changes.",
            hmrDecision.Kind,
            hmrDecision.Reason,
            changedPathsToProcess.Count);
        if (hmrDecision.Kind == HmrDecisionKind.None)
            return;

        var reloadSequence = Interlocked.Increment(ref _reloadSequence);
        var reason = hmrDecision.Kind == HmrDecisionKind.FullReload &&
                     IsGenericFileChangeFallback(hmrDecision.Reason)
            ? BuildReloadReason(changedPathsToProcess)
            : hmrDecision.Reason;
        if (hmrDecision.Kind == HmrDecisionKind.ModuleUpdate)
        {
            await _reloadHub.BroadcastModuleUpdateAsync(
                _serverInstanceId,
                reloadSequence,
                reason,
                hmrDecision.Updates,
                cancellationToken);
            return;
        }

        await _reloadHub.BroadcastReloadAsync(
            _serverInstanceId,
            reloadSequence,
            reason,
            cancellationToken);
    }

    private static bool IsGenericFileChangeFallback(string reason)
        => reason is "hmr-unmapped-change" or "hmr-non-module-change" or "hmr-manifest-unavailable";

    private string BuildReloadReason(IReadOnlyList<string> changedPaths)
    {
        if (changedPaths.Count != 1)
            return "file-change";

        var changedPath = changedPaths[0];
        foreach (var registration in _watchRegistrations)
        {
            if (!FileWatchFilter.ShouldObserve(registration.RootPath, changedPath))
                continue;

            var relativePath = Path.GetRelativePath(registration.RootPath, changedPath)
                .Replace('\\', '/');
            return "file-change:" + relativePath;
        }

        return "file-change";
    }

    private IReadOnlyList<string> FilterAlreadyBroadcastChanges(IReadOnlyList<string> changedPaths)
    {
        if (changedPaths.Count == 0)
            return changedPaths;

        var pathsToProcess = new List<string>(changedPaths.Count);
        lock (_lastBroadcastSnapshotsLock)
        {
            foreach (var path in changedPaths
                         .Where(static path => !string.IsNullOrWhiteSpace(path))
                         .Select(Path.GetFullPath)
                         .Where(ShouldObservePath)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Order(StringComparer.OrdinalIgnoreCase))
            {
                var snapshot = CaptureObservedFileSnapshot(path);
                if (_lastBroadcastSnapshots.TryGetValue(path, out var previousSnapshot)
                    && Nullable.Equals(previousSnapshot, snapshot))
                {
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
            return;

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

    private static ObservedFileSnapshot? CaptureObservedFileSnapshot(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            return fileInfo.Exists
                ? new ObservedFileSnapshot(fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks)
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

    /// <summary>
    /// Keeps the configured root separate from the existing ancestor supplied to FileSystemWatcher.
    /// The latter lets reload begin before a generated <c>jazor</c> directory exists.
    /// </summary>
    private sealed record WatchRootRegistration(string RootPath, string WatcherPath);
}

/// <summary>Minimal file state used to suppress duplicate watcher and polling notifications.</summary>
internal readonly record struct ObservedFileSnapshot(
    long Length,
    long LastWriteTimeUtcTicks);
