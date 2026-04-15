namespace Jazor.VueHost.DevServer;

internal sealed class FileChangeDebouncer : IDisposable
{
    private readonly TimeSpan _debounceInterval;
    private readonly Lock _gate = new();
    private readonly HashSet<string> _pendingPaths = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _flushCancellationSource;
    private bool _disposed;

    public event Action<IReadOnlyList<string>>? DebouncedChange;

    public FileChangeDebouncer(TimeSpan debounceInterval)
    {
        if (debounceInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(debounceInterval));
        }

        _debounceInterval = debounceInterval;
    }

    public void Record(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        CancellationTokenSource flushCancellationSource;
        lock (_gate)
        {
            ThrowIfDisposed();

            _pendingPaths.Add(Path.GetFullPath(path));
            _flushCancellationSource?.Cancel();
            _flushCancellationSource?.Dispose();
            _flushCancellationSource = new CancellationTokenSource();
            flushCancellationSource = _flushCancellationSource;
        }

        _ = ScheduleFlushAsync(flushCancellationSource);
    }

    public void Dispose()
    {
        CancellationTokenSource? flushCancellationSource;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            flushCancellationSource = _flushCancellationSource;
            _flushCancellationSource = null;
            _pendingPaths.Clear();
        }

        flushCancellationSource?.Cancel();
        flushCancellationSource?.Dispose();
    }

    private async Task ScheduleFlushAsync(CancellationTokenSource flushCancellationSource)
    {
        try
        {
            await Task.Delay(_debounceInterval, flushCancellationSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        IReadOnlyList<string>? changedPaths = null;
        lock (_gate)
        {
            if (_disposed || !ReferenceEquals(_flushCancellationSource, flushCancellationSource))
            {
                return;
            }

            if (_pendingPaths.Count > 0)
            {
                changedPaths = _pendingPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray();
                _pendingPaths.Clear();
            }

            _flushCancellationSource = null;
        }

        flushCancellationSource.Dispose();
        if (changedPaths is not null)
        {
            DebouncedChange?.Invoke(changedPaths);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(FileChangeDebouncer));
        }
    }
}
