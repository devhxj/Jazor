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
    }

    internal sealed record CacheEntry<T>(T Data, Dictionary<string, string?> Inputs) where T : IQuerySnapshot;
    internal Action<IReadOnlyDictionary<string, string?>>? ObserveQueryInputs { get; set; }

    internal async Task<T> ReadQueryAsync<T>(string documentPath, string moduleSpecifier, string mode, string? query,
        ConcurrentDictionary<(string Project, string Specifier, string Mode, string? Query), CacheEntry<T>> cache,
        Type owner, Func<string, string, string, string?, string, string, CancellationToken, Task<T>> read,
        CancellationToken cancellationToken, Action<string, CacheEntry<T>>? remember = null) where T : IQuerySnapshot
    {
        var project = FindProject(documentPath);
        var key = (project, moduleSpecifier, mode, query);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var store = ResolveStore();
            var typescript = await RestoreTypeScriptAsync(store, cancellationToken);
            await RestoreAsync(project, store, cancellationToken);
            if (cache.TryGetValue(key, out var cached) && MatchesInputs(cached) &&
                await MatchesModuleAsync(project, moduleSpecifier, cached, cancellationToken))
            {
                ReportInputs(cached.Inputs, typescript);
                return cached.Data;
            }
            var cacheFile = SnapshotCachePath(project, moduleSpecifier, mode, query, typescript, owner);
            if (await ReadCachedSnapshotAsync<T>(cacheFile, cancellationToken) is { } previous &&
                await MatchesModuleAsync(project, moduleSpecifier, previous, cancellationToken))
            {
                Remember(previous);
                return previous.Data;
            }

            var metadata = await read(project, moduleSpecifier, mode, query, store, typescript, cancellationToken);
            var inputs = metadata.Inputs.Concat(PackageInputs(project)).Distinct(StringComparer.Ordinal);
            var entry = new CacheEntry<T>(metadata, inputs.ToDictionary(path => path, Fingerprint, StringComparer.Ordinal));
            Remember(entry);
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

    private static bool MatchesInputs<T>(CacheEntry<T> entry) where T : IQuerySnapshot
        => entry.Inputs.Count > 0 && entry.Inputs.All(input => Fingerprint(input.Key) == input.Value);

    private static async Task<bool> MatchesModuleAsync<T>(string project, string specifier, CacheEntry<T> entry,
        CancellationToken cancellationToken) where T : IQuerySnapshot
    {
        var realpaths = JsonSerializer.Serialize(entry.Data.Realpaths, JsonOptions);
        var resolved = await RunMetadataAsync(project, "resolve-module.mjs", [specifier, realpaths], cancellationToken);
        return JsonSerializer.Deserialize<string>(resolved) == entry.Data.ModulePath;
    }

    private static async Task<CacheEntry<T>?> ReadCachedSnapshotAsync<T>(string path, CancellationToken cancellationToken) where T : IQuerySnapshot
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var entry = await JsonSerializer.DeserializeAsync<CacheEntry<T>>(stream, JsonOptions, cancellationToken);
            return entry is { Data.ModulePath: not null, Inputs: not null } &&
                MatchesInputs(entry) ? entry : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (JsonException) { return null; }
    }

    private static async Task WriteCachedSnapshotAsync<T>(string path, CacheEntry<T> entry, CancellationToken cancellationToken) where T : IQuerySnapshot
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
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        finally
        {
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
