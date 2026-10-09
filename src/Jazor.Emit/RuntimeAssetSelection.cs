using System.Text.Json;

namespace Jazor.Emit;

/// <summary>Uses the SDK/NuGet RID graph, rather than guessing platform compatibility from RID names.</summary>
internal sealed class RuntimeAssetSelection(string targetRuntimeIdentifier, IReadOnlyList<string> compatibleRuntimeIdentifiers)
{
    public string TargetRuntimeIdentifier { get; } = targetRuntimeIdentifier;

    public static RuntimeAssetSelection Create(string runtimeIdentifier, string? runtimeIdentifierGraphPath)
    {
        var compatible = new List<string>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        if (runtimeIdentifierGraphPath is null)
        {
            compatible.Add(runtimeIdentifier);
            return new RuntimeAssetSelection(runtimeIdentifier, compatible);
        }

        using var graph = JsonDocument.Parse(File.ReadAllText(runtimeIdentifierGraphPath));
        var runtimes = graph.RootElement.GetProperty("runtimes");
        var pending = new Queue<string>();
        pending.Enqueue(runtimeIdentifier);
        while (pending.TryDequeue(out var rid))
        {
            if (!visited.Add(rid))
                continue;
            compatible.Add(rid);
            if (runtimes.TryGetProperty(rid, out var runtime) && runtime.TryGetProperty("#import", out var imports))
            {
                foreach (var import in imports.EnumerateArray())
                    pending.Enqueue(import.GetString()!);
            }
        }

        return new RuntimeAssetSelection(runtimeIdentifier, compatible);
    }

    public int GetPriority(string? runtimeIdentifier)
    {
        if (runtimeIdentifier is null)
            return compatibleRuntimeIdentifiers.Count;
        for (var index = 0; index < compatibleRuntimeIdentifiers.Count; index++)
        {
            if (string.Equals(runtimeIdentifier, compatibleRuntimeIdentifiers[index], StringComparison.Ordinal))
                return index;
        }

        return -1;
    }

    public string Describe(string? runtimeIdentifier, int priority)
        => runtimeIdentifier is null
            ? $"RID-neutral asset (fallback after target RID '{TargetRuntimeIdentifier}')"
            : priority == 0
                ? $"RID '{runtimeIdentifier}' (exact target RID)"
                : $"RID '{runtimeIdentifier}' (SDK runtime-graph fallback #{priority} for '{TargetRuntimeIdentifier}')";

    public static (string? RuntimeIdentifier, bool IsNative) GetRuntimeAsset(string path)
    {
        var segments = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index + 2 < segments.Length; index++)
        {
            if (!string.Equals(segments[index], "runtimes", StringComparison.OrdinalIgnoreCase))
                continue;
            if (string.Equals(segments[index + 2], "lib", StringComparison.OrdinalIgnoreCase))
                return (segments[index + 1], false);
            if (string.Equals(segments[index + 2], "native", StringComparison.OrdinalIgnoreCase))
                return (segments[index + 1], true);
        }

        return (null, false);
    }
}
