using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

internal sealed class JazorDevelopmentReloadService : IHostedService, IAsyncDisposable
{
    internal const string PathBaseAttributeName = "data-jazor-path-base";

    private readonly JazorDevelopmentReloadOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly IJazorDevelopmentRuntimeSignals _runtimeSignals;
    private readonly ILogger<JazorDevelopmentReloadService> _logger;
    private readonly JazorDevelopmentReloadHub _reloadHub = new();
    private readonly JazorDevelopmentCoalescingPathChangeQueue _fileChangeQueue = new();
    private readonly Dictionary<string, JazorDevelopmentObservedFileSnapshot?> _lastBroadcastSnapshots = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lastBroadcastSnapshotsLock = new();
    private readonly string _serverInstanceId = Guid.NewGuid().ToString("N")[..8];
    private readonly List<FileSystemWatcher> _fileWatchers = [];
    private readonly List<JazorDevelopmentFileSnapshotPoller> _fileSnapshotPollers = [];
    private readonly List<WatchRootRegistration> _watchRegistrations = [];
    private JazorDevelopmentFileChangeDebouncer? _fileChangeDebouncer;
    private Task? _fileChangePump;
    private CancellationTokenSource? _fileChangeCancellationSource;
    private long _reloadSequence;
    private int _disposeState;

    public JazorDevelopmentReloadService(
        IOptions<JazorDevelopmentReloadOptions> options,
        IWebHostEnvironment environment,
        IJazorDevelopmentRuntimeSignals runtimeSignals,
        ILogger<JazorDevelopmentReloadService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _runtimeSignals = runtimeSignals ?? throw new ArgumentNullException(nameof(runtimeSignals));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ClientScriptContent = JazorDevelopmentClientScriptFactory.Build(
            _options.WebSocketPath.Value!,
            PathBaseAttributeName,
            _options.SuppressReloadOnReconnectWhenExternalBrowserRefreshIsActive
                && _runtimeSignals.IsExternalBrowserRefreshActive);
    }

    public JazorDevelopmentReloadOptions Options => _options;

    public bool IsEnabled => _environment.IsDevelopment();

    public string ClientScriptContent { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!IsEnabled)
            return;

        if (_fileChangePump is not null)
            return;

        ResolveWatchRegistrations();
        _fileChangeCancellationSource = new CancellationTokenSource();
        _fileChangePump = PumpFileChangesAsync(_fileChangeCancellationSource.Token);
        _fileChangeDebouncer = new JazorDevelopmentFileChangeDebouncer(_options.FileChangeDebounceInterval);
        _fileChangeDebouncer.DebouncedChange += OnDebouncedFileChanges;

        foreach (var registration in _watchRegistrations)
        {
            TryStartFileWatcher(registration);
            var snapshotPoller = new JazorDevelopmentFileSnapshotPoller(
                registration.RootPath,
                _options.FileChangePollingInterval,
                OnDebouncedFileChanges);
            snapshotPoller.Start();
            _fileSnapshotPollers.Add(snapshotPoller);
        }

        if (_watchRegistrations.Count == 0)
        {
            _logger.LogDebug("Jazor development reload started without file watchers because no watch roots were configured.");
            return;
        }

        _logger.LogDebug(
            "Jazor development reload watching {WatchRootCount} roots for browser full reload.",
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
    }

    private void ResolveWatchRegistrations()
    {
        _watchRegistrations.Clear();
        var suppressExternalBrowserRefreshRoots =
            _options.SuppressWatchRootsHandledByExternalBrowserRefresh
            && _runtimeSignals.IsExternalBrowserRefreshActive;

        foreach (var configuredRoot in _options.WatchRootPaths
                     .Where(static path => !string.IsNullOrWhiteSpace(path))
                     .Select(static path => path.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var rootPath = Path.GetFullPath(
                Path.IsPathRooted(configuredRoot)
                    ? configuredRoot
                    : Path.Combine(_environment.ContentRootPath, configuredRoot));
            if (suppressExternalBrowserRefreshRoots && IsHandledByExternalBrowserRefresh(rootPath))
            {
                _logger.LogDebug(
                    "Jazor development reload skipped watch root '{WatchRoot}' because an external browser refresh pipeline already owns that static root.",
                    rootPath);
                continue;
            }

            var watcherPath = ResolveWatcherPath(rootPath);
            if (watcherPath is null)
            {
                _logger.LogDebug(
                    "Jazor development reload skipped watch root '{WatchRoot}' because no existing ancestor directory could be resolved.",
                    rootPath);
                continue;
            }

            _watchRegistrations.Add(new WatchRootRegistration(rootPath, watcherPath));
        }
    }

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
                "Jazor development reload could not start a file watcher for '{WatchRoot}'. Snapshot polling will remain active.",
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
            if (JazorDevelopmentFileWatchFilter.ShouldObserve(registration.RootPath, path))
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
                    "Jazor development reload ignored a file-change processing failure for full-reload broadcast.");
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
        var reloadSequence = Interlocked.Increment(ref _reloadSequence);
        var reason = BuildReloadReason(changedPathsToProcess);
        if (CanOfferModuleUpdate(changedPathsToProcess))
        {
            await _reloadHub.BroadcastModuleUpdateAsync(
                _serverInstanceId,
                reloadSequence,
                reason,
                BuildModuleUpdatePaths(changedPathsToProcess),
                cancellationToken);
            return;
        }

        await _reloadHub.BroadcastReloadAsync(
            _serverInstanceId,
            reloadSequence,
            reason,
            cancellationToken);
    }

    private static bool CanOfferModuleUpdate(IReadOnlyList<string> changedPaths)
        => changedPaths.Count > 0
            && changedPaths.All(static path => path.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase));

    private IReadOnlyList<string> BuildModuleUpdatePaths(IReadOnlyList<string> changedPaths)
    {
        var modulePaths = new List<string>(changedPaths.Count);
        foreach (var changedPath in changedPaths)
        {
            foreach (var registration in _watchRegistrations)
            {
                if (!JazorDevelopmentFileWatchFilter.ShouldObserve(registration.RootPath, changedPath))
                    continue;

                // This is a logical path relative to the watched root, not a server file-system path or URL.
                modulePaths.Add(Path.GetRelativePath(registration.RootPath, changedPath).Replace('\\', '/'));
                break;
            }
        }

        return modulePaths;
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

    private string BuildReloadReason(IReadOnlyList<string> changedPaths)
    {
        if (changedPaths.Count != 1)
            return "file-change";

        var changedPath = changedPaths[0];
        foreach (var registration in _watchRegistrations)
        {
            if (!JazorDevelopmentFileWatchFilter.ShouldObserve(registration.RootPath, changedPath))
                continue;

            var relativePath = Path.GetRelativePath(registration.RootPath, changedPath)
                .Replace('\\', '/');
            return "file-change:" + relativePath;
        }

        return "file-change";
    }

    private static JazorDevelopmentObservedFileSnapshot? CaptureObservedFileSnapshot(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            return fileInfo.Exists
                ? new JazorDevelopmentObservedFileSnapshot(fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks)
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

    private sealed record WatchRootRegistration(string RootPath, string WatcherPath);
}

internal readonly record struct JazorDevelopmentObservedFileSnapshot(
    long Length,
    long LastWriteTimeUtcTicks);
