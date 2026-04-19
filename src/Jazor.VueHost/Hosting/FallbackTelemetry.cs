using System.Collections.Concurrent;
using System.Text.Json;

namespace Jazor.VueHost.Hosting;

internal static class FallbackTelemetry
{
    private static readonly ConcurrentDictionary<string, byte> ReportedKeys = new(StringComparer.Ordinal);
    private static Action<string>? TestSink;

    public static void ReportActivation(
        string component,
        string mode,
        string reason,
        string? documentPath = null,
        bool oncePerKey = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(component);
        ArgumentException.ThrowIfNullOrWhiteSpace(mode);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var key = string.Concat(component, "|", mode, "|", reason, "|", documentPath ?? string.Empty);
        if (oncePerKey && !ReportedKeys.TryAdd(key, 0))
        {
            return;
        }

        var payload = new
        {
            eventType = "vueHostFallbackActivated",
            component,
            mode,
            reason,
            documentPath,
            timestamp = DateTimeOffset.UtcNow
        };
        var message = JsonSerializer.Serialize(payload);
        var sink = TestSink;
        if (sink is not null)
        {
            sink(message);
            return;
        }

        Console.Error.WriteLine(message);
    }

    internal static void ResetForTests()
    {
        ReportedKeys.Clear();
        TestSink = null;
    }

    internal static void SetTestSinkForTests(Action<string>? sink)
        => TestSink = sink;
}
