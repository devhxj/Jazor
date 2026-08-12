namespace Jazor.AspNetCore.Dev;

/// <summary>Coalesces bursty watcher notifications into one deterministic path batch.</summary>
internal sealed class FileChangeDebouncer : IDisposable
{
    private readonly TimeSpan _debounceInterval;
    private readonly Lock _gate = new();
    private readonly HashSet<string> _pendingPaths = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _flushCancellationSource;
    private bool _disposed;

    /// <summary>Raised once after the quiet interval with the ordered, deduplicated changed paths.</summary>
    public event Action<IReadOnlyList<string>>? DebouncedChange;

    public FileChangeDebouncer(TimeSpan debounceInterval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(debounceInterval, TimeSpan.Zero);
        _debounceInterval = debounceInterval;
    }

    public void Record(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Task flushTask;
        lock (_gate)
        {
            if (_disposed)
                return;

            // A rebuild commonly writes several generated files; retain only one ordered batch.
            _pendingPaths.Add(Path.GetFullPath(path));
            _flushCancellationSource?.Cancel();
            _flushCancellationSource?.Dispose();
            _flushCancellationSource = new CancellationTokenSource();
            flushTask = ScheduleFlushAsync(_flushCancellationSource);
        }

        _ = flushTask;
    }

    public void Dispose()
    {
        CancellationTokenSource? flushCancellationSource;
        lock (_gate)
        {
            if (_disposed)
                return;

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
        catch (ObjectDisposedException)
        {
            return;
        }

        IReadOnlyList<string>? changedPaths = null;
        lock (_gate)
        {
            if (_disposed || !ReferenceEquals(_flushCancellationSource, flushCancellationSource))
                return;

            if (_pendingPaths.Count > 0)
            {
                changedPaths = _pendingPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray();
                _pendingPaths.Clear();
            }

            _flushCancellationSource = null;
        }

        flushCancellationSource.Dispose();
        if (changedPaths is not null)
            DebouncedChange?.Invoke(changedPaths);
    }
}
