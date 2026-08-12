using System.Threading.Channels;

namespace Jazor.AspNetCore.Dev;

/// <summary>Provides one bounded signal for the latest accumulated set of changed paths.</summary>
internal sealed class PathChangeQueue
{
    // The signal is intentionally capacity-one: paths are accumulated under the lock and a
    // reader drains the complete set, so additional wakeups during a burst are redundant.
    private readonly Channel<byte> _signals = Channel.CreateBounded<byte>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite
        });
    private readonly HashSet<string> _pendingPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lock = new();

    /// <summary>Accumulates changed paths and signals the single consumer.</summary>
    public void Enqueue(IReadOnlyList<string> changedPaths)
    {
        ArgumentNullException.ThrowIfNull(changedPaths);

        var hasPendingChanges = false;
        lock (_lock)
        {
            foreach (var path in changedPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                _pendingPaths.Add(path);
                hasPendingChanges = true;
            }
        }

        if (hasPendingChanges)
            _signals.Writer.TryWrite(0);
    }

    /// <summary>Waits for a signal and returns the complete ordered batch observed so far.</summary>
    public async ValueTask<IReadOnlyList<string>> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signals.Reader.ReadAsync(cancellationToken);

        lock (_lock)
        {
            if (_pendingPaths.Count == 0)
                return [];

            var changedPaths = _pendingPaths
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            _pendingPaths.Clear();
            return changedPaths;
        }
    }
}
