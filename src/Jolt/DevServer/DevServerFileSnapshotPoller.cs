namespace Jolt.DevServer;

internal sealed class DevServerFileSnapshotPoller : IAsyncDisposable
{
    private readonly string _rootDirectory;
    private readonly TimeSpan _pollInterval;
    private readonly Action<IReadOnlyList<string>> _onChangedPaths;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollTask;

    public DevServerFileSnapshotPoller(
        string rootDirectory,
        TimeSpan pollInterval,
        Action<IReadOnlyList<string>> onChangedPaths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        if (pollInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval));
        }

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _pollInterval = pollInterval;
        _onChangedPaths = onChangedPaths ?? throw new ArgumentNullException(nameof(onChangedPaths));
    }

    public void Start()
    {
        if (_pollTask is not null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _pollTask = PollAsync(_cancellationTokenSource.Token);
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (_pollTask is not null)
        {
            try
            {
                await _pollTask;
            }
            catch (OperationCanceledException)
            {
            }

            _pollTask = null;
        }
    }

    internal static IReadOnlyDictionary<string, FileSnapshotEntry> CaptureSnapshot(string rootDirectory)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var snapshot = new Dictionary<string, FileSnapshotEntry>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(fullRootDirectory))
        {
            return snapshot;
        }

        foreach (var filePath in EnumerateObservedFiles(fullRootDirectory))
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                snapshot[filePath] = new FileSnapshotEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return snapshot;
    }

    internal static IReadOnlyList<string> GetChangedPaths(
        IReadOnlyDictionary<string, FileSnapshotEntry> previousSnapshot,
        IReadOnlyDictionary<string, FileSnapshotEntry> currentSnapshot)
    {
        var changedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, previousEntry) in previousSnapshot)
        {
            if (!currentSnapshot.TryGetValue(path, out var currentEntry)
                || !previousEntry.Equals(currentEntry))
            {
                changedPaths.Add(path);
            }
        }

        foreach (var path in currentSnapshot.Keys)
        {
            if (!previousSnapshot.ContainsKey(path))
            {
                changedPaths.Add(path);
            }
        }

        return changedPaths
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        var previousSnapshot = CaptureSnapshot(_rootDirectory);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_pollInterval, cancellationToken);
            var currentSnapshot = CaptureSnapshot(_rootDirectory);
            var changedPaths = GetChangedPaths(previousSnapshot, currentSnapshot);
            previousSnapshot = currentSnapshot;

            if (changedPaths.Count > 0)
            {
                _onChangedPaths(changedPaths);
            }
        }
    }

    private static IEnumerable<string> EnumerateObservedFiles(string rootDirectory)
    {
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(rootDirectory);

        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = pendingDirectories.Pop();
            IEnumerable<string> childDirectories;
            try
            {
                childDirectories = Directory.EnumerateDirectories(currentDirectory);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var childDirectory in childDirectories)
            {
                if (DevServerFileWatchFilter.IsIgnoredDirectoryName(Path.GetFileName(childDirectory)))
                {
                    continue;
                }

                pendingDirectories.Push(childDirectory);
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(currentDirectory);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var filePath in files)
            {
                if (DevServerFileWatchFilter.ShouldObserve(rootDirectory, filePath))
                {
                    yield return filePath;
                }
            }
        }
    }

    internal readonly record struct FileSnapshotEntry(long Length, long LastWriteTimeUtcTicks);
}
