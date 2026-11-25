using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.Npm;

public sealed partial class NpmValueMetadataProvider
{
    private static readonly string CacheVersion = string.Join("|",
        typeof(NpmValueMetadataProvider).Module.ModuleVersionId,
        typeof(NodeToolchain).Module.ModuleVersionId,
        WebIdlCarriers.Version,
        typeof(CSharpCompilation).Module.ModuleVersionId,
        Environment.Version);

    internal interface IQuerySnapshot
    {
        string[] Inputs { get; }
        string? ModulePath { get; }
        Dictionary<string, string>? Realpaths { get; }
        Dictionary<string, string?>? ObservedInputs { get; set; }
    }

    private static bool CanShareSnapshot(IQuerySnapshot snapshot, IReadOnlyDictionary<string, string?> fingerprints)
    {
        // An editor may save a declaration before the checker exits. Publish only if
        // the validation hashes still describe the bytes it actually consumed.
        var observed = snapshot.ObservedInputs;
        snapshot.ObservedInputs = null;
        return observed is not null && observed.All(input =>
            fingerprints.TryGetValue(input.Key, out var fingerprint) && input.Value == fingerprint);
    }

    internal sealed record CacheEntry<T>(T Data, Dictionary<string, string?> Inputs) where T : IQuerySnapshot;
    internal Action<IReadOnlyDictionary<string, string?>>? ObserveQueryInputs { get; set; }
    internal Action? ObserveChecker { get; set; }
    internal string PortableCacheDirectory { get; init; } =
        Path.Combine(NpmProjectionCache.UserCacheDirectory, "npm-metadata", "v1");

    // One loader invocation owns these observations. Independent loads validate
    // the actual bytes and resolution again, including same-timestamp edits.
    internal sealed class Validation
    {
        private readonly Dictionary<string, string?> _fingerprints = new(StringComparer.Ordinal);
        internal Dictionary<(string Project, string Specifier, string Realpaths), string?> Resolutions { get; } = new();

        internal string? ReadFingerprint(string path)
        {
            if (!_fingerprints.TryGetValue(path, out var hash))
                _fingerprints.Add(path, hash = Fingerprint(path));
            return hash;
        }

        internal void Clear()
        {
            _fingerprints.Clear();
            Resolutions.Clear();
        }
    }

    internal async Task<T> ReadQueryAsync<T>(string documentPath, string moduleSpecifier, string mode, string? query,
        ConcurrentDictionary<(string Project, string Specifier, string Mode, string? Query), CacheEntry<T>> cache,
        Type owner, Func<string, string, string, string?, string, string, CancellationToken, Task<T>> read,
        CancellationToken cancellationToken, Action<string, CacheEntry<T>>? remember = null, Validation? validation = null) where T : IQuerySnapshot
    {
        var project = FindProject(documentPath);
        var key = (project, moduleSpecifier, mode, query);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var store = ResolveStore();
            var typescript = await RestoreTypeScriptAsync(store, cancellationToken, validation: validation);
            if (await RestoreAsync(project, store, cancellationToken) != "")
                validation?.Clear();
            if (cache.TryGetValue(key, out var cached) && MatchesInputs(cached, validation) &&
                await MatchesModuleAsync(project, moduleSpecifier, cached, validation, cancellationToken))
            {
                ReportInputs(cached.Inputs, typescript);
                return cached.Data;
            }
            var portableFile = PortableSnapshotPath(moduleSpecifier, mode, query, owner);
            foreach (var candidate in PortableCandidates(portableFile))
            {
                if (await ReadPortableSnapshotAsync<T>(candidate, project, typescript, validation, cancellationToken) is not { } shared ||
                    !await MatchesModuleAsync(project, moduleSpecifier, shared, validation, cancellationToken))
                    continue;
                Remember(shared);
                return shared.Data;
            }
            var cacheFile = SnapshotCachePath(project, moduleSpecifier, mode, query, typescript, owner);
            if (await ReadCachedSnapshotAsync<T>(cacheFile, validation, cancellationToken) is { } previous &&
                await MatchesModuleAsync(project, moduleSpecifier, previous, validation, cancellationToken))
            {
                Remember(previous);
                return previous.Data;
            }

            validation?.Clear();
            ObserveChecker?.Invoke();
            var metadata = await read(project, moduleSpecifier, mode, query, store, typescript, cancellationToken);
            var inputs = metadata.Inputs.Concat(PackageInputs(project)).Distinct(StringComparer.Ordinal);
            // Validation was cleared before the checker ran. Reuse the fresh
            // publication hashes when this load queries the same graph again.
            var entry = new CacheEntry<T>(metadata,
                inputs.ToDictionary(path => path, path => ReadFingerprint(path, validation), StringComparer.Ordinal));
            if (!CanShareSnapshot(metadata, entry.Inputs))
            {
                ReportInputs(entry.Inputs, typescript);
                return metadata;
            }
            Remember(entry);
            if (!await WritePortableSnapshotAsync(portableFile, entry, project, typescript, cancellationToken))
                await WriteCachedSnapshotAsync(cacheFile, entry, cancellationToken);
            return metadata;

            void Remember(CacheEntry<T> entry)
            {
                cache[key] = entry;
                remember?.Invoke(project, entry);
                ReportInputs(entry.Inputs, typescript);
            }

        }
        finally
        {
            gate.Release();
        }
    }

    internal static IEnumerable<string> PackageInputs(string project)
    {
        yield return Path.Combine(project, "package.json");
        yield return Path.Combine(project, ".npmrc");
        for (var parent = new DirectoryInfo(project); parent is not null; parent = parent.Parent)
        {
            var workspace = Path.Combine(parent.FullName, "pnpm-workspace.yaml");
            if (!File.Exists(workspace))
                continue;
            yield return workspace;
            yield return Path.Combine(parent.FullName, "pnpm-lock.yaml");
            yield return Path.Combine(parent.FullName, "package.json");
            yield return Path.Combine(parent.FullName, ".npmrc");
            yield break;
        }
        yield return Path.Combine(project, "pnpm-lock.yaml");
    }

    internal void ReportInputs(IReadOnlyDictionary<string, string?> inputs, string typescript)
    {
        if (ObserveQueryInputs is not { } observe)
            return;
        var toolLibrary = Path.GetDirectoryName(typescript)! + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        observe(inputs.Where(input => !input.Key.StartsWith(toolLibrary, comparison))
            .ToDictionary(input => input.Key, input => input.Value, StringComparer.Ordinal));
    }

    private static string SnapshotCachePath(string project, string moduleSpecifier, string mode, string? query, string typescript, Type owner)
    {
        // The provider MVID covers its embedded scripts and tooling lockfile.
        // Results also carry absolute source paths used by definition targets.
        var key = JsonSerializer.SerializeToUtf8Bytes(new[] { CacheVersion, owner.Module.ModuleVersionId.ToString(), project, moduleSpecifier, mode, query, typescript });
        return Path.Combine(project, ".jazor", "cache", "npm-metadata", Convert.ToHexString(SHA256.HashData(key)) + ".json");
    }

    private static string? ReadFingerprint(string path, Validation? validation)
        => validation is null ? Fingerprint(path) : validation.ReadFingerprint(path);

    private static bool MatchesInputs<T>(CacheEntry<T> entry, Validation? validation) where T : IQuerySnapshot
        => entry.Inputs.Count > 0 && entry.Inputs.All(input => ReadFingerprint(input.Key, validation) == input.Value);

    private static async Task<bool> MatchesModuleAsync<T>(string project, string specifier, CacheEntry<T> entry,
        Validation? validation, CancellationToken cancellationToken) where T : IQuerySnapshot
    {
        cancellationToken.ThrowIfCancellationRequested();
        var realpaths = JsonSerializer.Serialize(entry.Data.Realpaths, JsonOptions);
        var key = (project, specifier, realpaths);
        if (validation is not null && validation.Resolutions.TryGetValue(key, out var previous))
            return previous == entry.Data.ModulePath;
        var resolved = await RunMetadataAsync(project, "resolve-module.mjs", [specifier, realpaths], cancellationToken);
        var module = JsonSerializer.Deserialize<string>(resolved);
        validation?.Resolutions.Add(key, module);
        return module == entry.Data.ModulePath;
    }

    private static async Task<CacheEntry<T>?> ReadCachedSnapshotAsync<T>(string path, Validation? validation, CancellationToken cancellationToken) where T : IQuerySnapshot
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var entry = await JsonSerializer.DeserializeAsync<CacheEntry<T>>(stream, JsonOptions, cancellationToken);
            return entry is { Data.ModulePath: not null, Inputs: not null } &&
                MatchesInputs(entry, validation) ? entry : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (JsonException) { return null; }
    }

    private static async Task<bool> WriteCachedSnapshotAsync<T>(string path, T entry, CancellationToken cancellationToken)
    {
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                await JsonSerializer.SerializeAsync(stream, entry, JsonOptions, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            // Separate processes may derive the same snapshot. Readers see
            // either complete version of the cache file.
            File.Move(temporary, path, overwrite: true);
            return true;
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        finally
        {
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        return false;
    }
}
