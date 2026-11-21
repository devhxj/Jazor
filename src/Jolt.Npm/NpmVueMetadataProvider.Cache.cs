using System.Security.Cryptography;
using System.Text.Json;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis.CSharp;

namespace Jolt.Npm;

public sealed partial class NpmVueMetadataProvider
{
    private static readonly string CacheVersion = string.Join("|",
        typeof(NpmVueMetadataProvider).Module.ModuleVersionId,
        typeof(NodeToolchain).Module.ModuleVersionId,
        WebIdlCarriers.Version,
        typeof(CSharpCompilation).Module.ModuleVersionId,
        Environment.Version);

    private static string SnapshotCachePath(string project, string moduleSpecifier, string mode, string? query, string typescript)
    {
        // The provider MVID covers its embedded scripts and tooling lockfile.
        // Results also carry absolute source paths used by definition targets.
        var key = JsonSerializer.SerializeToUtf8Bytes(new[] { CacheVersion, project, moduleSpecifier, mode, query, typescript });
        return Path.Combine(project, ".jazor", "cache", "npm-metadata", Convert.ToHexString(SHA256.HashData(key)) + ".json");
    }

    private static bool MatchesInputs(CacheEntry entry)
        => entry.Inputs.Count > 0 && entry.Inputs.All(input => Fingerprint(input.Key) == input.Value);

    private static async Task<bool> MatchesModuleAsync(string project, string specifier, CacheEntry entry,
        CancellationToken cancellationToken)
    {
        var realpaths = JsonSerializer.Serialize(entry.Data.Realpaths, JsonOptions);
        var resolved = await RunMetadataAsync(project, "resolve-module.mjs", [specifier, realpaths], cancellationToken);
        return JsonSerializer.Deserialize<string>(resolved) == entry.Data.ModulePath;
    }

    private static async Task<CacheEntry?> ReadCachedSnapshotAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var entry = await JsonSerializer.DeserializeAsync<CacheEntry>(stream, JsonOptions, cancellationToken);
            return entry is { Data.Exports: not null, Inputs: not null } && MatchesInputs(entry) ? entry : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (JsonException) { return null; }
    }

    private static async Task WriteCachedSnapshotAsync(string path, CacheEntry entry, CancellationToken cancellationToken)
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
