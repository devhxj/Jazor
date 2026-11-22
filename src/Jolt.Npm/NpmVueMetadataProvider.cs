using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jolt.Npm;

public sealed partial class NpmVueMetadataProvider(string? storeDirectory = null)
{
    private readonly string? _engineCacheDirectory;

    internal NpmVueMetadataProvider(string storeDirectory, string engineCacheDirectory) : this(storeDirectory)
        => _engineCacheDirectory = engineCacheDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ProjectGates = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ToolGates = new();
    private readonly ConcurrentDictionary<(string Project, string Specifier, string Mode, string? Query), CacheEntry> _cache = new();

    public async Task<NpmVueComponent> ReadAsync(string documentPath, string moduleSpecifier, string exportName,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "component", exportName, cancellationToken);
        return metadata.Component!;
    }

    public async Task<IReadOnlyList<string>> ListExportsAsync(string documentPath, string moduleSpecifier,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "component", null, cancellationToken);
        return metadata.Exports!;
    }

    public async Task<NpmValueExport> ReadValueAsync(string documentPath, string moduleSpecifier, string exportName,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "value", exportName, cancellationToken);
        return metadata.Value!;
    }

    public async Task<IReadOnlyList<string>> ListValueExportsAsync(string documentPath, string moduleSpecifier,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "value", null, cancellationToken);
        return metadata.Exports!;
    }

    public async Task<IReadOnlyList<NpmValueExport>> ReadValuesAsync(string documentPath, string moduleSpecifier,
        IReadOnlyList<string> exportNames, CancellationToken cancellationToken = default)
        => (await ReadValueProjectionsAsync(documentPath, moduleSpecifier, exportNames, cancellationToken)).Values;

    public async Task<NpmValueProjections> ReadValueProjectionsAsync(string documentPath, string moduleSpecifier,
        IReadOnlyList<string> exportNames, CancellationToken cancellationToken = default)
    {
        if (exportNames.Count == 0)
            return new([], new Dictionary<string, string>());
        var query = JsonSerializer.Serialize(exportNames.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "values", query, cancellationToken);
        return new(metadata.Values!, metadata.Errors!);
    }

    public async Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        CancellationToken cancellationToken = default)
        => await ListSpecifiersAsync(documentPath, packageName, requestedNamespace: null, cancellationToken);

    public async Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        string? requestedNamespace, CancellationToken cancellationToken = default)
    {
        var project = FindProject(documentPath);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var file = Path.Combine(project, "node_modules", packageName, "package.json");
            await RestoreAsync(project, ResolveStore(), cancellationToken);
            using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(file, cancellationToken));
            if (!metadata.RootElement.TryGetProperty("exports", out var exports))
                return [packageName];
            if (exports.ValueKind is not JsonValueKind.Object)
                return exports.ValueKind is JsonValueKind.Null ? [] : [packageName];

            var properties = exports.EnumerateObject().ToArray();
            if (properties.All(property => !property.Name.StartsWith('.')))
                return [packageName];
            var exact = properties
                .Where(property => property.Value.ValueKind != JsonValueKind.Null &&
                    (property.Name == "." || property.Name.StartsWith("./", StringComparison.Ordinal) &&
                    !property.Name.Contains('*')))
                .Select(property => property.Name == "." ? packageName : packageName + property.Name[1..]);
            if (requestedNamespace is null)
                return exact.Order(StringComparer.Ordinal).ToArray();

            return exact.Concat(MatchPatternSpecifiers(Path.GetDirectoryName(file)!, packageName, requestedNamespace, properties))
                .Where(specifier => NpmNames.NamespaceFor(specifier) == requestedNamespace)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
        }
        finally
        {
            gate.Release();
        }
    }

    private static IEnumerable<string> MatchPatternSpecifiers(string packageDirectory, string packageName,
        string requestedNamespace, JsonProperty[] exports)
    {
        var patterns = exports.Where(property => property.Name.StartsWith("./", StringComparison.Ordinal) &&
                property.Name.Count(character => character == '*') == 1)
            // Package exports prefer the longest prefix, then the longest key.
            .OrderByDescending(property => property.Name.IndexOf('*')).ThenByDescending(property => property.Name.Length).ToArray();
        foreach (var pattern in patterns)
        {
            var split = pattern.Name.IndexOf('*');
            var keyPrefix = pattern.Name[..split];
            var keySuffix = pattern.Name[(split + 1)..];
            foreach (var target in GetTargets(pattern.Value))
            {
                if (!target.StartsWith("./", StringComparison.Ordinal) || target.Count(character => character == '*') != 1)
                    continue;
                var targetSplit = target.IndexOf('*');
                var targetPrefix = target[2..targetSplit];
                var targetSuffix = target[(targetSplit + 1)..];
                var directoryLength = targetPrefix.LastIndexOf('/') + 1;
                foreach (var capture in FindCaptures(Path.Combine(packageDirectory, targetPrefix[..directoryLength]),
                    targetPrefix[directoryLength..], ""))
                {
                    var key = keyPrefix + capture + keySuffix;
                    if (exports.Any(export => export.Name == key) ||
                        patterns.First(export => PatternMatches(export.Name, key)).Name != pattern.Name)
                        continue;
                    yield return packageName + key[1..];
                }

                IEnumerable<string> FindCaptures(string directory, string prefix, string capturePrefix)
                {
                    if (!Directory.Exists(directory))
                        yield break;
                    foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
                    {
                        var name = Path.GetFileName(entry);
                        if (!name.StartsWith(prefix, StringComparison.Ordinal))
                            continue;
                        var capture = capturePrefix + name[prefix.Length..];
                        if (name.EndsWith(targetSuffix, StringComparison.Ordinal) &&
                            name.Length >= prefix.Length + targetSuffix.Length && File.Exists(entry))
                            yield return capture[..(capture.Length - targetSuffix.Length)];

                        // Only visit directories whose public export prefix can lead to
                        // this @using namespace. Skip the rest of the package tree.
                        var nextNamespace = NpmNames.NamespaceFor(packageName + keyPrefix[1..] + capture);
                        if (requestedNamespace.StartsWith(nextNamespace + ".", StringComparison.Ordinal) && Directory.Exists(entry))
                            foreach (var nested in FindCaptures(entry, "", capture + "/"))
                                yield return nested;
                    }
                }
            }
        }
    }

    private static bool PatternMatches(string pattern, string key)
    {
        var split = pattern.IndexOf('*');
        return key.Length >= pattern.Length - 1 &&
            key.StartsWith(pattern[..split], StringComparison.Ordinal) &&
            key.EndsWith(pattern[(split + 1)..], StringComparison.Ordinal);
    }

    private static IEnumerable<string> GetTargets(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => [value.GetString()!],
        JsonValueKind.Object => value.EnumerateObject().Where(property => property.Name != "types")
            .SelectMany(property => GetTargets(property.Value)),
        JsonValueKind.Array => value.EnumerateArray().SelectMany(GetTargets),
        _ => [],
    };

    private async Task<Snapshot> ReadSnapshotAsync(string documentPath, string moduleSpecifier, string mode, string? query,
        CancellationToken cancellationToken)
    {
        var project = FindProject(documentPath);
        var key = (project, moduleSpecifier, mode, query);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var persistent = mode is "value" or "values";
            if (!persistent && _cache.TryGetValue(key, out var component) && MatchesInputs(component))
                return component.Data;
            var store = ResolveStore();
            var typescript = await RestoreTypeScriptAsync(store, cancellationToken);
            await RestoreAsync(project, store, cancellationToken);
            if (persistent && _cache.TryGetValue(key, out var cached) && MatchesInputs(cached) &&
                await MatchesModuleAsync(project, moduleSpecifier, cached, cancellationToken))
                return cached.Data;
            var cacheFile = persistent ? SnapshotCachePath(project, moduleSpecifier, mode, query, typescript) : null;
            if (cacheFile is not null && await ReadCachedSnapshotAsync(cacheFile, cancellationToken) is { } previous &&
                await MatchesModuleAsync(project, moduleSpecifier, previous, cancellationToken))
            {
                Remember(previous);
                return previous.Data;
            }
            IReadOnlyList<string> arguments = [typescript, moduleSpecifier, mode, query ?? "", WebIdlCarriers.Metadata];
            var json = await RunMetadataAsync(project, "metadata.mjs", arguments, cancellationToken);
            var metadata = JsonSerializer.Deserialize<Snapshot>(json, JsonOptions)!;
            if (metadata.Value is { } valueWithVariants)
                metadata = metadata with { Value = RemoveCollidingVariants(valueWithVariants, metadata.Exports!) };
            if (metadata.Values is { } batchWithVariants)
                metadata = metadata with { Values = batchWithVariants.Select(value => RemoveCollidingVariants(value, metadata.Exports!)).ToArray() };
            if (metadata.Value is { } export && GetProjectionError(export, metadata.Exports!) is { } error)
                throw new InvalidOperationException(error);
            if (metadata.Values is { } values)
            {
                var accepted = new List<NpmValueExport>(values.Count);
                foreach (var value in values)
                {
                    if (GetProjectionError(value, metadata.Exports!) is { } reason)
                        metadata.Errors!.Add(value.ExportName, reason);
                    else
                        accepted.Add(value);
                }
                metadata = metadata with { Values = accepted };
            }
            var inputs = metadata.Inputs.Concat([
                Path.Combine(project, "package.json"),
                Path.Combine(project, "pnpm-lock.yaml"),
                Path.Combine(project, ".npmrc"),
            ]).Distinct(StringComparer.Ordinal);
            var fingerprints = inputs.ToDictionary(path => path, Fingerprint, StringComparer.Ordinal);
            var entry = new CacheEntry(metadata, fingerprints);
            Remember(entry);
            if (cacheFile is not null)
                await WriteCachedSnapshotAsync(cacheFile, entry, cancellationToken);
            return metadata;

            void Remember(CacheEntry entry)
            {
                _cache[key] = entry;
                if (entry.Data.Values is { } values)
                    foreach (var value in values)
                        _cache[(project, moduleSpecifier, "value", value.ExportName)] =
                            entry with { Data = entry.Data with { Value = value, Values = null } };
            }
        }
        finally
        {
            gate.Release();
        }
    }

    private static NpmValueExport RemoveCollidingVariants(NpmValueExport export, IReadOnlyList<string> exports)
    {
        if (!export.Signatures.Any(signature => signature.ManagedNameSuffix is not null))
            return export;
        var name = NpmNames.ValueNameFor(export.ExportName);
        var names = exports.Select(NpmNames.ValueNameFor).ToHashSet(StringComparer.Ordinal);
        return export with { Signatures = export.Signatures.Where(signature => signature.ManagedNameSuffix is null ||
            IsIdentifier(name + signature.ManagedNameSuffix) && !names.Contains(name + signature.ManagedNameSuffix)).ToArray() };
    }

    private static string? GetProjectionError(NpmValueExport export, IReadOnlyList<string> exports)
    {
        var projected = NpmNames.ValueNameFor(export.ExportName);
        if (!IsIdentifier(projected) || projected == "Exports")
            return Unsupported($"export name '{export.ExportName}' cannot form a C# member of Exports");
        var collisions = exports.Where(name => name != export.ExportName && NpmNames.ValueNameFor(name) == projected).ToArray();
        if (collisions.Length != 0)
            return Unsupported($"export names '{export.ExportName}' and '{string.Join("', '", collisions)}' both project to '{projected}'");
        if (export.ConstantType is null && export.Signatures.Count == 0)
            return Unsupported("no callable form has an available C# name");

        var error = CheckSignatures(export.Signatures) ??
            (export.ConstantType is { } constant ? CheckType(constant) : null);
        return error is null ? null : Unsupported(error);

        string Unsupported(string detail) =>
            $"Unsupported npm value export '{export.ExportName}' from '{export.ModuleSpecifier}': {detail}.";
    }

    private static string? CheckSignatures(IReadOnlyList<NpmValueSignature> signatures)
    {
        foreach (var signature in signatures)
        {
            foreach (var parameter in signature.Parameters)
            {
                if (!IsIdentifier(parameter.Name))
                    return $"parameter name '{parameter.Name}' cannot form a C# identifier";
                if (CheckType(parameter.Type) is { } parameterError)
                    return parameterError;
            }
            if (CheckType(signature.ReturnType) is { } returnError)
                return returnError;
        }
        return null;
    }

    private static string? CheckType(NpmValueType type)
    {
        if (type.ElementType is { } element && CheckType(element) is { } elementError)
            return elementError;
        if (type.WriteType is { } write && CheckType(write) is { } writeError)
            return writeError;
        if (type.Signatures is { } signatures)
            return CheckSignatures(signatures);
        if (type.Members is not { } members)
            return null;

        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var member in members)
        {
            var name = NpmNames.ValueNameFor(member.Name);
            if (!IsIdentifier(name))
                return $"member name '{member.Name}' cannot form a C# identifier";
            if (!names.TryAdd(name, member.Name))
                return $"member names '{names[name]}' and '{member.Name}' both project to '{name}'";
            if (CheckType(member.Type) is { } error)
                return error;
        }
        return null;
    }

    private static bool IsIdentifier(string name)
    {
        // The projection emits verbatim identifiers. Check token identity too,
        // since Roslyn can remove format characters and decode Unicode escapes.
        var token = SyntaxFactory.ParseToken("@" + name);
        return token.IsKind(SyntaxKind.IdentifierToken) && !token.ContainsDiagnostics &&
            token.Text == "@" + name && token.ValueText == name;
    }

    private string ResolveStore() => Path.GetFullPath(
        storeDirectory ?? Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolt", "pnpm-store"));

    private async Task<string> RestoreTypeScriptAsync(string store, CancellationToken cancellationToken)
    {
        using var manifest = OpenResource("package.json");
        using var lockfile = OpenResource("pnpm-lock.yaml");
        var version = Convert.ToHexString(SHA256.HashData(lockfile));
        lockfile.Position = 0;
        var tools = Path.Combine(store, "jolt-metadata", version);
        var gate = ToolGates.GetOrAdd(tools, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(tools);
            WriteResource(manifest, Path.Combine(tools, "package.json"));
            WriteResource(lockfile, Path.Combine(tools, "pnpm-lock.yaml"));
            await RestoreAsync(tools, store, cancellationToken);
            return Path.Combine(tools, "node_modules", "typescript", "lib", "typescript.js");
        }
        finally
        {
            gate.Release();
        }

        static Stream OpenResource(string name) =>
            typeof(NpmVueMetadataProvider).Assembly.GetManifestResourceStream("Jolt.Npm." + name)!;

        static void WriteResource(Stream resource, string path)
        {
            if (File.Exists(path))
                return;
            using var destination = File.Create(path);
            resource.CopyTo(destination);
        }
    }

    private Task<string> RestoreAsync(string project, string store, CancellationToken cancellationToken)
        => NodeToolchain.RestoreAsync(project, store, cancellationToken, Console.Error, _engineCacheDirectory);

    private static string FindProject(string documentPath)
    {
        for (var directory = new FileInfo(documentPath).Directory; directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "package.json")))
                return directory.FullName;
        throw new FileNotFoundException($"No package.json found for '{documentPath}'.");
    }

    private static string? Fingerprint(string path)
    {
        if (!File.Exists(path))
            return Directory.Exists(path) ? "directory" : null;
        try
        {
            using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return Convert.ToHexString(SHA256.HashData(input));
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private sealed record Snapshot(NpmVueComponent? Component, NpmValueExport? Value, IReadOnlyList<NpmValueExport>? Values,
        IReadOnlyList<string>? Exports, string[] Inputs, Dictionary<string, string>? Errors, string? ModulePath,
        Dictionary<string, string>? Realpaths);
    private sealed record CacheEntry(Snapshot Data, Dictionary<string, string?> Inputs);
}
