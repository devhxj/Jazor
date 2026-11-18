using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Jazor.NodeHost;

namespace Jolt.Npm;

public sealed class NpmVueMetadataProvider(string? storeDirectory = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ProjectGates = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ToolGates = new();
    private readonly ConcurrentDictionary<(string Project, string Specifier, string? Export), CacheEntry> _cache = new();

    public async Task<NpmVueComponent> ReadAsync(string documentPath, string moduleSpecifier, string exportName,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, exportName, cancellationToken);
        return metadata.Component!;
    }

    public async Task<IReadOnlyList<string>> ListExportsAsync(string documentPath, string moduleSpecifier,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadSnapshotAsync(documentPath, moduleSpecifier, null, cancellationToken);
        return metadata.Exports!;
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
            await NodeToolchain.RestoreAsync(project, ResolveStore(), cancellationToken, Console.Error);
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

    private async Task<Snapshot> ReadSnapshotAsync(string documentPath, string moduleSpecifier, string? exportName,
        CancellationToken cancellationToken)
    {
        var project = FindProject(documentPath);
        var key = (project, moduleSpecifier, exportName);
        var gate = ProjectGates.GetOrAdd(project, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(key, out var cached) && cached.Inputs.All(input => Fingerprint(input.Key) == input.Value))
                return cached.Data;

            var store = ResolveStore();
            var typescript = await RestoreTypeScriptAsync(store, cancellationToken);
            await NodeToolchain.RestoreAsync(project, store, cancellationToken, Console.Error);
            var module = Path.Combine(project, ".jazor", $"npm-metadata-{Guid.NewGuid():N}.mjs");
            try
            {
                using var source = typeof(NpmVueMetadataProvider).Assembly.GetManifestResourceStream("Jolt.Npm.metadata.mjs")!;
                await using (var output = File.Create(module))
                    await source.CopyToAsync(output, cancellationToken);
                IReadOnlyList<string> arguments = exportName is null
                    ? [typescript, moduleSpecifier]
                    : [typescript, moduleSpecifier, exportName];
                var json = await NodeModuleRunner.RunAsync(project, module, arguments, cancellationToken);
                var metadata = JsonSerializer.Deserialize<Snapshot>(json, JsonOptions)!;
                var inputs = metadata.Inputs.Concat([
                    Path.Combine(project, "package.json"),
                    Path.Combine(project, "pnpm-lock.yaml"),
                    Path.Combine(project, ".npmrc"),
                ]).Distinct(StringComparer.Ordinal);
                _cache[key] = new(metadata, inputs.ToDictionary(path => path, Fingerprint, StringComparer.Ordinal));
                return metadata;
            }
            finally
            {
                File.Delete(module);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    private string ResolveStore() => Path.GetFullPath(
        storeDirectory ?? Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolt", "pnpm-store"));

    private static async Task<string> RestoreTypeScriptAsync(string store, CancellationToken cancellationToken)
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
            await NodeToolchain.RestoreAsync(tools, store, cancellationToken, Console.Error);
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

    private sealed record Snapshot(NpmVueComponent? Component, IReadOnlyList<string>? Exports, string[] Inputs);
    private sealed record CacheEntry(Snapshot Data, Dictionary<string, string?> Inputs);
}
