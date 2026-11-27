using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.Npm;

public sealed partial class NpmValueMetadataProvider(string? storeDirectory = null)
{
    private readonly string? _engineCacheDirectory;
    private readonly string _defaultStoreOwner = "Jazor";

    internal NpmValueMetadataProvider(string? storeDirectory, string? engineCacheDirectory, string defaultStoreOwner) : this(storeDirectory)
    {
        _engineCacheDirectory = engineCacheDirectory;
        _defaultStoreOwner = defaultStoreOwner;
    }
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static readonly ConcurrentDictionary<string, SemaphoreSlim> ProjectGates = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ToolGates = new();
    private static readonly ConcurrentDictionary<(Type Owner, string Prefix), string> ToolVersions = new();
    private readonly ConcurrentDictionary<(string Project, string Specifier, string Mode, string? Query), CacheEntry<ValueSnapshot>> _cache = new();

    public async Task<NpmValueExport> ReadValueAsync(string documentPath, string moduleSpecifier, string exportName,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "value", exportName, cancellationToken);
        return metadata.Value!;
    }

    public async Task<IReadOnlyList<string>> ListValueExportsAsync(string documentPath, string moduleSpecifier,
        CancellationToken cancellationToken = default)
        => await ListValueExportsAsync(documentPath, moduleSpecifier, null, cancellationToken);

    internal async Task<IReadOnlyList<string>> ListValueExportsAsync(string documentPath, string moduleSpecifier,
        Validation? validation, CancellationToken cancellationToken)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "value", null, cancellationToken, validation);
        return metadata.Exports!;
    }

    public async Task<IReadOnlyList<NpmValueExport>> ReadValuesAsync(string documentPath, string moduleSpecifier,
        IReadOnlyList<string> exportNames, CancellationToken cancellationToken = default)
        => (await ReadValueProjectionsAsync(documentPath, moduleSpecifier, exportNames, cancellationToken)).Values;

    public async Task<NpmValueProjections> ReadValueProjectionsAsync(string documentPath, string moduleSpecifier,
        IReadOnlyList<string> exportNames, CancellationToken cancellationToken = default)
        => await ReadValueProjectionsAsync(documentPath, moduleSpecifier, exportNames, null, cancellationToken);

    internal async Task<NpmValueProjections> ReadValueProjectionsAsync(string documentPath, string moduleSpecifier,
        IReadOnlyList<string> exportNames, Validation? validation, CancellationToken cancellationToken)
    {
        if (exportNames.Count == 0)
            return new([], new Dictionary<string, string>());
        var query = JsonSerializer.Serialize(exportNames.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "values", query, cancellationToken, validation);
        return new(metadata.Values!, metadata.Errors!);
    }

    public async Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        CancellationToken cancellationToken = default)
        => await ListSpecifiersAsync(documentPath, packageName, requestedNamespace: null, cancellationToken);

    public async Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        string? requestedNamespace, CancellationToken cancellationToken = default)
        => await ListSpecifiersAsync(documentPath, packageName, requestedNamespace, null, cancellationToken);

    internal async Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        string? requestedNamespace, Validation? validation, CancellationToken cancellationToken)
    {
        var project = FindProject(documentPath);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (await RestoreAsync(project, ResolveStore(), cancellationToken) != "")
                validation?.Clear();
            var file = NpmPackageExports.ManifestCandidates(project, packageName).FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException($"Could not find the installed package.json for '{packageName}' from '{project}'.");
            using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(file, cancellationToken));
            if (!metadata.RootElement.TryGetProperty("exports", out var exports) || exports.ValueKind is not JsonValueKind.Object)
                return NpmPackageExports.HasRoot(metadata.RootElement) ? [packageName] : [];

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

    private Task<ValueSnapshot> ReadSnapshotAsync(string documentPath, string moduleSpecifier, string mode, string? query,
        CancellationToken cancellationToken, Validation? validation = null)
        => ReadQueryAsync(documentPath, moduleSpecifier, mode, query, _cache, typeof(NpmValueMetadataProvider), ReadValueSnapshotAsync,
            cancellationToken, (project, entry) =>
            {
                if (entry.Data.Values is { } values)
                    foreach (var value in values)
                        _cache[(project, moduleSpecifier, "value", value.ExportName)] =
                            entry with { Data = entry.Data with { Value = value, Values = null } };
            }, validation);

    private async Task<ValueSnapshot> ReadValueSnapshotAsync(string project, string moduleSpecifier, string mode, string? query,
        string store, string typescript, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> arguments = [typescript, moduleSpecifier, mode, query ?? "", WebIdlCarriers.Metadata];
        var json = await RunMetadataAsync(project, "metadata.mjs", arguments, cancellationToken);
        var metadata = JsonSerializer.Deserialize<ValueSnapshot>(json, JsonOptions)!;
        if (metadata.NeedsLocaleTool)
        {
            // Only a recognized Composer with static message literals needs Intlify.
            var tools = await RestoreToolsAsync(store, cancellationToken, typeof(NpmValueMetadataProvider), "locale.");
            json = await RunMetadataAsync(project, "metadata.mjs", [.. arguments, Path.Combine(tools, "package.json")], cancellationToken);
            metadata = JsonSerializer.Deserialize<ValueSnapshot>(json, JsonOptions)!;
        }
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
        return metadata;
    }

    private static NpmValueExport RemoveCollidingVariants(NpmValueExport export, IReadOnlyList<string> exports)
    {
        if (export.AttachedMemberType is null && !export.Signatures.Any(signature => signature.ManagedNameSuffix is not null))
            return export;
        var name = NpmNames.ValueNameFor(export.ExportName);
        var names = exports.Select(NpmNames.ValueNameFor).ToHashSet(StringComparer.Ordinal);
        var signatures = export.Signatures.Where(signature => signature.ManagedNameSuffix is null ||
            IsIdentifier(name + signature.ManagedNameSuffix) && !names.Contains(name + signature.ManagedNameSuffix)).ToArray();
        foreach (var signature in signatures)
            names.Add(name + signature.ManagedNameSuffix);
        // Attached members are an optional projection. An unrepresentable member
        // must not prevent the existing callable overloads from binding.
        var attachedMembers = export.AttachedMembers?.Where(member =>
            IsIdentifier(NpmNames.ValueNameFor(member.Name)) && CheckType(member.Type) is null)
            .GroupBy(member => NpmNames.ValueNameFor(member.Name), StringComparer.Ordinal)
            .Where(group => group.Count() == 1).Select(group => group.Single()).ToArray();
        return export with
        {
            Signatures = signatures,
            AttachedMembers = attachedMembers,
            AttachedMemberSuffix = attachedMembers is { Length: > 0 }
                ? NpmExportSelection.ReserveAttachedMemberSuffix(name, names) : null,
        };
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

    internal string ResolveStore() => Path.GetFullPath(
        storeDirectory ?? Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _defaultStoreOwner, "pnpm-store"));

    internal async Task<string> RestoreTypeScriptAsync(string store, CancellationToken cancellationToken, Type? resourceOwner = null,
        Validation? validation = null)
    {
        var tools = await RestoreToolsAsync(store, cancellationToken, resourceOwner ?? typeof(NpmValueMetadataProvider), "", validation);
        return Path.Combine(tools, "node_modules", "typescript", "lib", "typescript.js");
    }

    private async Task<string> RestoreToolsAsync(string store, CancellationToken cancellationToken, Type resourceOwner,
        string resourcePrefix, Validation? validation = null)
    {
        // The embedded lockfile is immutable for this loaded assembly. Keep
        // validating the on-disk tool inputs on every restore.
        var version = ToolVersions.GetOrAdd((resourceOwner, resourcePrefix), static key =>
        {
            using var lockfile = OpenResource(key.Owner, key.Prefix, "pnpm-lock.yaml");
            return Convert.ToHexString(SHA256.HashData(lockfile));
        });
        var tools = Path.Combine(store, "npm-metadata", version);
        var gate = ToolGates.GetOrAdd(tools, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(tools);
            WriteResource("package.json");
            WriteResource("pnpm-lock.yaml");
            if (await RestoreWithStampAsync(tools, store, cancellationToken, standaloneTool: true) != "")
                validation?.Clear();
            return tools;
        }
        finally
        {
            gate.Release();
        }

        static Stream OpenResource(Type owner, string prefix, string name) =>
            owner.Assembly.GetManifestResourceStream(owner.Namespace + "." + prefix + name)!;

        void WriteResource(string name)
        {
            var path = Path.Combine(tools, name);
            if (File.Exists(path))
                return;
            using var resource = OpenResource(resourceOwner, resourcePrefix, name);
            // ToolGates protects only this process. Publish a complete file before another process can restore it.
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    resource.CopyTo(destination);
                File.Move(temporary, path);
            }
            catch (IOException) when (File.Exists(path)) { }
            finally
            {
                File.Delete(temporary);
            }
        }
    }

    internal Task<string> RestoreAsync(string project, string store, CancellationToken cancellationToken)
        => RestoreWithStampAsync(project, store, cancellationToken, standaloneTool: false);

    public static IReadOnlyList<string> ListPackageNames(string projectDirectory)
    {
        for (var directory = new DirectoryInfo(projectDirectory); directory is not null; directory = directory.Parent)
        {
            var manifest = Path.Combine(directory.FullName, "package.json");
            if (!File.Exists(manifest))
                continue;

            using var json = JsonDocument.Parse(File.ReadAllText(manifest));
            string[] sections = ["dependencies", "devDependencies", "peerDependencies", "optionalDependencies"];
            return sections
                .Where(section => json.RootElement.TryGetProperty(section, out var dependencies) && dependencies.ValueKind == JsonValueKind.Object)
                .SelectMany(section => json.RootElement.GetProperty(section).EnumerateObject().Select(package => package.Name))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
        }
        return [];
    }

    internal static string FindProject(string documentPath)
    {
        for (var directory = new FileInfo(documentPath).Directory; directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "package.json")))
                return directory.FullName;
        throw new FileNotFoundException($"No package.json found for '{documentPath}'.");
    }

    internal static string? Fingerprint(string path)
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

    private sealed record ValueSnapshot(NpmValueExport? Value, IReadOnlyList<NpmValueExport>? Values,
        IReadOnlyList<string>? Exports, string[] Inputs, Dictionary<string, string>? Errors, string? ModulePath,
        Dictionary<string, string>? Realpaths, bool NeedsLocaleTool = false) : IQuerySnapshot
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string?>? ObservedInputs { get; set; }
    }


    // Use the same embedded module graph as MSBuild so newly added imports resolve.
    private static readonly string[] MetadataModules = typeof(NpmValueMetadataProvider).Assembly.GetManifestResourceNames()
        .Where(name => name.StartsWith("Jazor.Npm.", StringComparison.Ordinal) && name.EndsWith(".mjs", StringComparison.Ordinal))
        .Select(name => name["Jazor.Npm.".Length..]).ToArray();

    internal static async Task<string> RunMetadataAsync(string project, string entry, IReadOnlyList<string> arguments,
        CancellationToken cancellationToken, Type? resourceOwner = null, IReadOnlyList<string>? resources = null)
    {
        resourceOwner ??= typeof(NpmValueMetadataProvider);
        resources ??= entry switch
        {
            "metadata.mjs" => MetadataModules,
            "resolve-module.mjs" => [entry, "runtime-module.mjs"],
            _ => [entry]
        };
        var modules = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in resources)
        {
            using var source = resourceOwner.Assembly.GetManifestResourceStream(resourceOwner.Namespace + "." + file)
                ?? typeof(NpmValueMetadataProvider).Assembly.GetManifestResourceStream("Jazor.Npm." + file)!;
            using var reader = new StreamReader(source);
            modules.Add(file, await reader.ReadToEndAsync(cancellationToken));
        }
        var modulePath = Path.Combine(project, ".jazor", "npm-metadata", entry);
        return await NodeModuleRunner.RunAsync(project, modulePath, arguments, cancellationToken, embeddedModules: modules);
    }
}
