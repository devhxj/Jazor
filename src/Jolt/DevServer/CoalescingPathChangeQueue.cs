using System.Threading.Channels;

namespace Jolt.DevServer;

internal sealed class CoalescingPathChangeQueue
{
    private readonly Channel<byte> _signals = Channel.CreateBounded<byte>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite
        });
    private readonly HashSet<string> _pendingPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public void Enqueue(IReadOnlyList<string> changedPaths)
    {
        ArgumentNullException.ThrowIfNull(changedPaths);

        var hasPendingChanges = false;
        lock (_lock)
        {
            foreach (var path in changedPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                _pendingPaths.Add(path);
                hasPendingChanges = true;
            }
        }

        if (hasPendingChanges)
        {
            _signals.Writer.TryWrite(0);
        }
    }

    public async ValueTask<IReadOnlyList<string>> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signals.Reader.ReadAsync(cancellationToken);

        lock (_lock)
        {
            if (_pendingPaths.Count == 0)
            {
                return [];
            }

            var changedPaths = _pendingPaths
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            _pendingPaths.Clear();
            return changedPaths;
        }
    }
}
