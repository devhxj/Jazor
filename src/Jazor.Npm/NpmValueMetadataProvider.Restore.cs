using System.Collections.Concurrent;
using Jazor.NodeHost;

namespace Jazor.Npm;

public sealed partial class NpmValueMetadataProvider
{
    private sealed record RestoreStamp(string Manifest, string Lockfile, string State, string? Configuration);

    private readonly ConcurrentDictionary<(string Project, string Store, bool Tool), RestoreStamp> _restores = new();

    private async Task<string> RestoreWithStampAsync(string project, string store, CancellationToken token, bool standaloneTool)
    {
        token.ThrowIfCancellationRequested();
        var key = (project, store, standaloneTool);
        var before = ReadRestoreStamp(project, standaloneTool);
        if (before is not null && _restores.TryGetValue(key, out var previous) && before == previous)
            return "";

        var result = await NodeToolchain.RestoreAsync(project, store, token, Console.Error,
            _engineCacheDirectory, standaloneTool);
        token.ThrowIfCancellationRequested();
        var after = ReadRestoreStamp(project, standaloneTool);
        // Cache only an install already validated by the engine against the
        // same inputs observed on both sides. A mutation during restore needs
        // another engine check before managed fast-path validation can begin.
        if (before is not null && before == after)
            _restores[key] = after;
        else
            _restores.TryRemove(key, out _);
        return result;
    }

    private static RestoreStamp? ReadRestoreStamp(string project, bool standaloneTool)
    {
        if (!standaloneTool)
            for (var parent = new DirectoryInfo(project); parent is not null; parent = parent.Parent)
                if (File.Exists(Path.Combine(parent.FullName, "pnpm-workspace.yaml"))) return null;

        // The bridge removes this stamp before an install and omits it for
        // mutable file dependencies. A missing local lock likewise stays cold.
        var state = Fingerprint(Path.Combine(project, "node_modules", ".jazor-restore-state"));
        if (state is null) return null;
        var manifest = Fingerprint(Path.Combine(project, "package.json"));
        var lockfile = Fingerprint(Path.Combine(project, "pnpm-lock.yaml"));
        return manifest is not null && lockfile is not null
            ? new(manifest, lockfile, state, Fingerprint(Path.Combine(project, ".npmrc")))
            : null;
    }
}
