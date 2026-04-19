using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace Jazor.VueHost.Extensions;

internal static class ExtensionLoadLogPersistence
{
    private const string ExtensionLoadEventType = "extensionLoad";
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

                registry.ReportExtensionLoad(invocation);
            }
        }
        catch (IOException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionLoadReplayFailed",
                logFilePath,
                $"failed to replay extension load log: {exception.Message}");
            return;
        }
        catch (UnauthorizedAccessException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionLoadReplayFailed",
                logFilePath,
                $"failed to replay extension load log: {exception.Message}");
            return;
        }
        catch (NotSupportedException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionLoadReplayFailed",
                logFilePath,
                $"failed to replay extension load log: {exception.Message}");
            return;
        }

        if (skippedCount > 0)
        {
            WritePersistenceEvent(
                eventType: "extensionLoadReplayPartial",
                logFilePath,
                $"skipped {skippedCount} malformed log line(s)");
        }
    }

    public static void Append(
        ExtensionLoadInvocation invocation,
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

            var persisted = new PersistedExtensionLoadInvocation
            {
                EventType = ExtensionLoadEventType,
                ExtensionId = invocation.ExtensionId,
                Source = invocation.Source,
                ExtensionDirectory = invocation.ExtensionDirectory,
                ManifestPath = invocation.ManifestPath,
                AssemblyPath = invocation.AssemblyPath,
                Status = invocation.Status,
                Reason = invocation.Reason,
                Timestamp = invocation.Timestamp
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
                eventType: "extensionLoadPersistFailed",
                logFilePath,
                $"failed to append extension load event: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionLoadPersistFailed",
                logFilePath,
                $"failed to append extension load event: {exception.Message}");
        }
        catch (NotSupportedException exception)
        {
            WritePersistenceEvent(
                eventType: "extensionLoadPersistFailed",
                logFilePath,
                $"failed to append extension load event: {exception.Message}");
        }
    }

    private static bool TryParseInvocation(
        string line,
        [NotNullWhen(true)] out ExtensionLoadInvocation? invocation)
    {
        invocation = null;
        PersistedExtensionLoadInvocation? persisted;
        try
        {
            persisted = JsonSerializer.Deserialize<PersistedExtensionLoadInvocation>(line, JsonOptions);
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
            && !string.Equals(persisted.EventType, ExtensionLoadEventType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(persisted.ExtensionId)
            || string.IsNullOrWhiteSpace(persisted.Source)
            || string.IsNullOrWhiteSpace(persisted.Status))
        {
            return false;
        }

        invocation = new ExtensionLoadInvocation(
            ExtensionId: persisted.ExtensionId.Trim(),
            Source: persisted.Source.Trim(),
            ExtensionDirectory: persisted.ExtensionDirectory?.Trim() ?? string.Empty,
            ManifestPath: string.IsNullOrWhiteSpace(persisted.ManifestPath)
                ? null
                : persisted.ManifestPath.Trim(),
            AssemblyPath: string.IsNullOrWhiteSpace(persisted.AssemblyPath)
                ? null
                : persisted.AssemblyPath.Trim(),
            Status: persisted.Status.Trim(),
            Reason: persisted.Reason ?? string.Empty,
            Timestamp: persisted.Timestamp == default
                ? DateTimeOffset.UtcNow
                : persisted.Timestamp);
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

    private sealed class PersistedExtensionLoadInvocation
    {
        public string? EventType { get; init; }

        public string? ExtensionId { get; init; }

        public string? Source { get; init; }

        public string? ExtensionDirectory { get; init; }

        public string? ManifestPath { get; init; }

        public string? AssemblyPath { get; init; }

        public string? Status { get; init; }

        public string? Reason { get; init; }

        public DateTimeOffset Timestamp { get; init; }
    }
}
