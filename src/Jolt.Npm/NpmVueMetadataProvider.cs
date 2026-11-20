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
    {
        if (exportNames.Count == 0)
            return [];
        var query = JsonSerializer.Serialize(exportNames.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, "values", query, cancellationToken);
        return metadata.Values!;
    }

    public async Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        CancellationToken cancellationToken = default)
    {
        var project = FindProject(documentPath);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var file = Path.Combine(project, "node_modules", packageName, "package.json");
            // Generated namespaces discover declared public subpaths. A pattern is
            // open-ended and can still be passed explicitly to ListExportsAsync.
            await RestoreAsync(project, ResolveStore(), cancellationToken);
            using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(file, cancellationToken));
            if (!metadata.RootElement.TryGetProperty("exports", out var exports))
                return [packageName];
            if (exports.ValueKind is not JsonValueKind.Object)
                return exports.ValueKind is JsonValueKind.Null ? [] : [packageName];

            var properties = exports.EnumerateObject().ToArray();
            if (properties.All(property => !property.Name.StartsWith('.')))
                return [packageName];
            return properties
                .Where(property => property.Value.ValueKind != JsonValueKind.Null &&
                    (property.Name == "." || property.Name.StartsWith("./", StringComparison.Ordinal) &&
                    !property.Name.Contains('*')))
                .Select(property => property.Name == "." ? packageName : packageName + property.Name[1..])
                .Order(StringComparer.Ordinal)
                .ToArray();
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<Snapshot> ReadSnapshotAsync(string documentPath, string moduleSpecifier, string mode, string? query,
        CancellationToken cancellationToken)
    {
        var project = FindProject(documentPath);
        var key = (project, moduleSpecifier, mode, query);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(key, out var cached) && cached.Inputs.All(input => Fingerprint(input.Key) == input.Value))
                return cached.Data;

            var store = ResolveStore();
            var typescript = await RestoreTypeScriptAsync(store, cancellationToken);
            await RestoreAsync(project, store, cancellationToken);
            IReadOnlyList<string> arguments = [typescript, moduleSpecifier, mode, query ?? ""];
            var json = await RunMetadataAsync(project, "metadata.mjs", arguments, cancellationToken);
            var metadata = JsonSerializer.Deserialize<Snapshot>(json, JsonOptions)!;
            if (metadata.Value is { } export && GetProjectionError(export, metadata.Exports!) is { } error)
                throw new InvalidOperationException(error);
            if (metadata.Values is { } values)
                metadata = metadata with { Values = values.Where(value => GetProjectionError(value, metadata.Exports!) is null).ToArray() };
            var inputs = metadata.Inputs.Concat([
                Path.Combine(project, "package.json"),
                Path.Combine(project, "pnpm-lock.yaml"),
                Path.Combine(project, ".npmrc"),
            ]).Distinct(StringComparer.Ordinal);
            var fingerprints = inputs.ToDictionary(path => path, Fingerprint, StringComparer.Ordinal);
            _cache[key] = new(metadata, fingerprints);
            if (metadata.Values is not null)
                foreach (var value in metadata.Values)
                    _cache[(project, moduleSpecifier, "value", value.ExportName)] = new(metadata with { Value = value, Values = null }, fingerprints);
            return metadata;
        }
        finally
        {
            gate.Release();
        }
    }

    private static string? GetProjectionError(NpmValueExport export, IReadOnlyList<string> exports)
    {
        var projected = NpmNames.ValueNameFor(export.ExportName);
        if (!IsIdentifier(projected) || projected == "Exports")
            return Unsupported($"export name '{export.ExportName}' cannot form a C# member of Exports");
        var collisions = exports.Where(name => name != export.ExportName && NpmNames.ValueNameFor(name) == projected).ToArray();
        if (collisions.Length != 0)
            return Unsupported($"export names '{export.ExportName}' and '{string.Join("', '", collisions)}' both project to '{projected}'");

        var error = CheckSignatures(export.Signatures);
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
        if (type.ElementType is { } element)
            return CheckType(element);
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
            return null;
        using var input = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(input));
    }

    private sealed record Snapshot(NpmVueComponent? Component, NpmValueExport? Value, IReadOnlyList<NpmValueExport>? Values,
        IReadOnlyList<string>? Exports, string[] Inputs);
    private sealed record CacheEntry(Snapshot Data, Dictionary<string, string?> Inputs);
}
