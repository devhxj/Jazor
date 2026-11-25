using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jazor.Npm;

public sealed partial class NpmValueMetadataProvider
{
    private sealed record PortableSnapshot(JsonNode Data, Dictionary<string, string?> Inputs, string[] MissingAncestorProbes);
    private const string ProjectPrefix = "\0project/";
    private const string ToolPrefix = "\0tool/";

    private string PortableSnapshotPath(string specifier, string mode, string? query, Type owner)
    {
        var key = JsonSerializer.SerializeToUtf8Bytes(new[] { CacheVersion, owner.Module.ModuleVersionId.ToString(), specifier, mode, query });
        return Path.Combine(PortableCacheDirectory, Convert.ToHexString(SHA256.HashData(key)));
    }

    private static IEnumerable<string> PortableCandidates(string path)
    {
        try
        {
            return Directory.GetFiles(path, "*.json").OrderBy(file => file, StringComparer.Ordinal);
        }
        catch (IOException) { return []; }
        catch (UnauthorizedAccessException) { return []; }
    }

    private static async Task<CacheEntry<T>?> ReadPortableSnapshotAsync<T>(string path, string project, string typescript,
        Validation? validation, CancellationToken token) where T : IQuerySnapshot
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var snapshot = await JsonSerializer.DeserializeAsync<PortableSnapshot>(stream, JsonOptions, token);
            if (snapshot is null || snapshot.Inputs.Count == 0) return null;
            var paths = new SnapshotPaths(project, typescript);
            var inputs = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var (label, expected) in snapshot.Inputs)
            {
                token.ThrowIfCancellationRequested();
                if (paths.Expand(label) is not { } absolute) return null;
                if (label == ProjectPrefix + "package.json")
                {
                    var (full, selectors) = ReadPackageFingerprints(absolute);
                    if (selectors != expected) return null;
                    inputs[absolute] = full;
                }
                else
                {
                    var actual = ReadFingerprint(absolute, validation);
                    if (actual != expected) return null;
                    inputs[absolute] = actual;
                }
            }
            // A different checkout may have more ancestor search locations than
            // the producer. Replay missing lookup probes at every ancestor.
            foreach (var ancestor in paths.Ancestors)
            foreach (var probe in snapshot.MissingAncestorProbes)
            {
                var absolute = Path.Combine(ancestor, probe);
                if (ReadFingerprint(absolute, validation) is not null) return null;
                inputs[absolute] = null;
            }
            var data = RebaseSnapshot(snapshot.Data, paths.Expand).Deserialize<T>(JsonOptions);
            if (data is not { ModulePath: not null }) return null;
            foreach (var input in PackageInputs(project))
                if (!inputs.ContainsKey(input)) inputs.Add(input, ReadFingerprint(input, validation));
            return new(data, inputs);
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (JsonException) { return null; }
    }

    private static async Task<bool> WritePortableSnapshotAsync<T>(string path, CacheEntry<T> entry, string project,
        string typescript, CancellationToken token) where T : IQuerySnapshot
    {
        var paths = new SnapshotPaths(project, typescript);
        if (entry.Data.ModulePath is not { } runtime || paths.Label(runtime) is not { } module ||
            !module.StartsWith(ProjectPrefix + "node_modules/", StringComparison.Ordinal))
            return false;
        var packageInputs = PackageInputs(project).ToHashSet(StringComparer.Ordinal);
        var inputs = new Dictionary<string, string?>(StringComparer.Ordinal);
        var probes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (absolute, expected) in entry.Inputs)
        {
            token.ThrowIfCancellationRequested();
            if (paths.Label(absolute) is { } label)
            {
                if (label == ProjectPrefix + "package.json")
                {
                    var (full, selectors) = ReadPackageFingerprints(absolute);
                    if (full != expected) return false;
                    inputs[label] = selectors;
                }
                else inputs[label] = expected;
            }
            else if (expected is null && paths.AncestorProbe(absolute) is { } probe)
                probes.Add(probe);
            else if (!packageInputs.Contains(absolute) && !paths.IsTransientProbe(absolute, expected))
                return false;
        }

        // Rebase all nested definitions, declaration paths and resolver keys.
        // A linked file outside the portable roots prevents publication.
        var portable = true;
        var payload = JsonSerializer.SerializeToNode(entry.Data, JsonOptions)!.AsObject();
        payload["inputs"] = new JsonArray(entry.Data.Inputs.Where(input => paths.Label(input) is not null)
            .Select(input => (JsonNode?)JsonValue.Create(input)).ToArray());
        var data = RebaseSnapshot(payload, value =>
        {
            if (paths.Label(value) is { } label) return label;
            if (Path.IsPathFullyQualified(value) || value.Contains(project, StringComparison.Ordinal))
                portable = false;
            return null;
        });
        if (data is not JsonObject snapshot || !portable) return false;
        var sortedInputs = inputs.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var sortedProbes = probes.OrderBy(probe => probe, StringComparer.Ordinal).ToArray();
        var realpaths = snapshot["realpaths"]?.AsObject().OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
        var identity = JsonSerializer.SerializeToUtf8Bytes(new { Inputs = sortedInputs, Probes = sortedProbes, Module = module, Realpaths = realpaths });
        var file = Path.Combine(path, Convert.ToHexString(SHA256.HashData(identity)) + ".json");
        return File.Exists(file) ||
            await WriteCachedSnapshotAsync(file, new PortableSnapshot(snapshot, sortedInputs, sortedProbes), token);
    }

    private static (string? Full, string? Selectors) ReadPackageFingerprints(string absolute)
    {
        if (!File.Exists(absolute)) return (null, null);
        var bytes = File.ReadAllBytes(absolute);
        var json = bytes.AsMemory();
        if (bytes.AsSpan().StartsWith("\uFEFF"u8))
            json = json[3..];
        using var document = JsonDocument.Parse(json);
        // Only package scope and self-reference selectors affect the synthetic
        // entry. Restore dependencies are checked through their actual probes.
        var metadata = document.RootElement;
        var relevant = metadata.EnumerateObject().Where(property => property.Name is
            "type" or "imports" or "exports" or "types" or "typings" or "typesVersions" or "main" or "module" or
            "peerDependencies" || property.Name is "name" or "version" && metadata.TryGetProperty("exports", out _))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToDictionary(property => property.Name, property => property.Value);
        return (Convert.ToHexString(SHA256.HashData(bytes)),
            Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(relevant))));
    }

    private static JsonNode? RebaseSnapshot(JsonNode? node, Func<string, string?> rebase, string? key = null)
    {
        JsonNode? RebasePath(JsonNode? item) => item is JsonValue value && value.TryGetValue<string>(out var text)
            ? JsonValue.Create(rebase(text) ?? text) : item?.DeepClone();

        return node switch
        {
            JsonObject map when key == "realpaths" => new JsonObject(map.Select(property =>
                KeyValuePair.Create(rebase(property.Key) ?? property.Key, RebasePath(property.Value)))),
            JsonObject obj => new JsonObject(obj.Select(property =>
                KeyValuePair.Create(property.Key, property.Key switch
                {
                    "modulePath" or "declarationPath" => RebasePath(property.Value),
                    "path" when IsDefinition(obj) => RebasePath(property.Value),
                    // JSDoc parameter names are authored data. A parameter called
                    // "modulePath" must not turn its description into a filesystem path.
                    "documentation" when IsDefinition(obj) => property.Value?.DeepClone(),
                    _ => RebaseSnapshot(property.Value, rebase, property.Key),
                }))),
            JsonArray array when key == "inputs" => new JsonArray(array.Select(RebasePath).ToArray()),
            JsonArray array => new JsonArray(array.Select(item => RebaseSnapshot(item, rebase)).ToArray()),
            _ => node?.DeepClone(),
        };

        static bool IsDefinition(JsonObject obj) => obj.ContainsKey("start") && obj.ContainsKey("length") && obj.ContainsKey("path");
    }

    private sealed class SnapshotPaths(string project, string typescript)
    {
        private readonly string _tools = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typescript)!, "..", "..", ".."));

        internal IEnumerable<string> Ancestors
        {
            get
            {
                for (var parent = Directory.GetParent(project); parent is not null; parent = parent.Parent)
                    yield return parent.FullName;
            }
        }

        internal string? Label(string value)
        {
            if (!Path.IsPathFullyQualified(value)) return null;
            var toolPath = Relative(_tools, value);
            if (toolPath is not null) return ToolPrefix + toolPath;
            var projectPath = Relative(project, value);
            return projectPath is "." or "package.json" || projectPath == "node_modules" ||
                projectPath?.StartsWith("node_modules/", StringComparison.Ordinal) == true ||
                projectPath?.StartsWith(".jazor/node_modules/", StringComparison.Ordinal) == true
                ? ProjectPrefix + projectPath : null;
        }

        internal string? Expand(string label)
        {
            var index = label.IndexOf('/');
            if (index < 0) return null;
            var relative = label[(index + 1)..];
            var root = label[..(index + 1)] switch { ProjectPrefix => project, ToolPrefix => _tools, _ => null };
            if (root is null || Path.IsPathRooted(relative)) return null;
            var absolute = Path.GetFullPath(Path.Combine(root, relative));
            return Relative(root, absolute) is null ? null : absolute;
        }

        internal string? AncestorProbe(string absolute)
        {
            foreach (var ancestor in Ancestors)
            {
                var relative = Relative(ancestor, absolute);
                if (relative == "node_modules" || relative?.StartsWith("node_modules/", StringComparison.Ordinal) == true)
                    return relative;
            }
            return null;
        }

        internal bool IsTransientProbe(string absolute, string? expected) => expected is null &&
            Relative(project, absolute)?.StartsWith(".jazor/npm-metadata/", StringComparison.Ordinal) == true;

        private static string? Relative(string root, string value)
        {
            var relative = Path.GetRelativePath(root, value);
            return Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                ? null : relative.Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
