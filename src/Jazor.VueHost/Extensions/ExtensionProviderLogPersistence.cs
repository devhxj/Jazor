using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace Jazor.VueHost.Extensions;

internal static class ExtensionProviderLogPersistence
{
    private const string ExtensionProviderEventType = "extensionProvider";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly Lock AppendGate = new();

    public static void Replay(
        IExtensionRegistry registry,
        string? logFilePath)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (string.IsNullOrWhiteSpace(logFilePath) || !File.Exists(logFilePath))
        {
            return;
        }

        var skippedCount = 0;
        try
        {
            foreach (var line in File.ReadLines(logFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!TryParseInvocation(line, out var invocation))
                {
                    skippedCount++;
                    continue;
                }

                registry.ReportProviderInvocation(invocation);
            }
        }
        catch (IOException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionProviderReplayFailed",
                logFilePath,
                $"failed to replay extension provider log: {exception.Message}");
            return;
        }
        catch (UnauthorizedAccessException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionProviderReplayFailed",
                logFilePath,
                $"failed to replay extension provider log: {exception.Message}");
            return;
        }
        catch (NotSupportedException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionProviderReplayFailed",
                logFilePath,
                $"failed to replay extension provider log: {exception.Message}");
            return;
        }

        if (skippedCount > 0)
        {
            WritePersistenceEvent(
                eventType: "extensionProviderReplayPartial",
                logFilePath,
                $"skipped {skippedCount} malformed log line(s)");
        }
    }

    public static void Append(
        ExtensionProviderInvocation invocation,
        string? logFilePath)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var persisted = new PersistedExtensionProviderInvocation
            {
                EventType = ExtensionProviderEventType,
                ProviderName = invocation.ProviderName,
                Capability = invocation.Capability,
                DurationMs = invocation.Duration.TotalMilliseconds,
                Succeeded = invocation.Succeeded,
                TimedOut = invocation.TimedOut,
                Skipped = invocation.Skipped,
                ErrorMessage = invocation.ErrorMessage,
                Timestamp = DateTimeOffset.UtcNow
            };
            var line = JsonSerializer.Serialize(persisted);

            lock (AppendGate)
            {
                File.AppendAllText(logFilePath, line + Environment.NewLine);
            }
        }
        catch (IOException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionProviderPersistFailed",
                logFilePath,
                $"failed to append extension provider event: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionProviderPersistFailed",
                logFilePath,
                $"failed to append extension provider event: {exception.Message}");
        }
        catch (NotSupportedException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionProviderPersistFailed",
                logFilePath,
                $"failed to append extension provider event: {exception.Message}");
        }
    }

    private static bool TryParseInvocation(
        string line,
        [NotNullWhen(true)] out ExtensionProviderInvocation? invocation)
    {
        invocation = null;
        PersistedExtensionProviderInvocation? persisted;
        try
        {
            persisted = JsonSerializer.Deserialize<PersistedExtensionProviderInvocation>(line, JsonOptions);
        }
        catch (JsonException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        if (persisted is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(persisted.EventType)
            && !string.Equals(persisted.EventType, ExtensionProviderEventType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(persisted.ProviderName)
            || string.IsNullOrWhiteSpace(persisted.Capability))
        {
            return false;
        }

        invocation = new ExtensionProviderInvocation(
            ProviderName: persisted.ProviderName.Trim(),
            Capability: persisted.Capability.Trim(),
            Duration: TimeSpan.FromMilliseconds(Math.Max(0, persisted.DurationMs)),
            Succeeded: persisted.Succeeded,
            TimedOut: persisted.TimedOut,
            Skipped: persisted.Skipped,
            ErrorMessage: persisted.ErrorMessage);
        return true;
    }

    private static void WritePersistenceEvent(
        string eventType,
        string logFilePath,
        string message)
    {
        var payload = new
        {
            eventType,
            logFilePath,
            message,
            timestamp = DateTimeOffset.UtcNow
        };
        Console.Error.WriteLine(JsonSerializer.Serialize(payload));
    }

    private sealed class PersistedExtensionProviderInvocation
    {
        public string? EventType { get; init; }

        public string? ProviderName { get; init; }

        public string? Capability { get; init; }

        public double DurationMs { get; init; }

        public bool Succeeded { get; init; }

        public bool TimedOut { get; init; }

        public bool Skipped { get; init; }

        public string? ErrorMessage { get; init; }

        public DateTimeOffset Timestamp { get; init; }
    }
}
