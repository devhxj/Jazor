using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;

namespace Jolt.Hosting;

internal static class FallbackTelemetry
{
    private static readonly ConcurrentDictionary<string, byte> ReportedKeys = new(StringComparer.Ordinal);
    private static readonly AsyncLocal<FallbackTelemetryTestState?> TestState = new();

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

        var testState = TestState.Value;
        var reportedKeys = testState?.ReportedKeys ?? ReportedKeys;
        var key = string.Concat(component, "|", mode, "|", reason, "|", documentPath ?? string.Empty);
        if (oncePerKey && !reportedKeys.TryAdd(key, 0))
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
        var sink = testState?.Sink;
        if (sink is not null)
        {
            sink(message);
            return;
        }

        Console.Error.WriteLine(message);
    }

    internal static void ResetForTests()
    {
        TestState.Value = null;
    }

    internal static void SetTestSinkForTests(Action<string>? sink)
    {
        var state = TestState.Value;
        if (sink is null)
        {
            if (state is not null)
            {
                state.Sink = null;
            }

            return;
        }

        state ??= new FallbackTelemetryTestState();
        state.Sink = sink;
        TestState.Value = state;
    }

    private sealed class FallbackTelemetryTestState
    {
        public ConcurrentDictionary<string, byte> ReportedKeys { get; } = new(StringComparer.Ordinal);

        public Action<string>? Sink { get; set; }
    }
}
