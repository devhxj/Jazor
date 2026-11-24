using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jazor.Npm;
using static Jazor.Npm.NpmValueMetadataProvider;

namespace Jolt.Npm;

public sealed record NpmVueComponentResult(NpmVueComponent? Component, string? Error);

public sealed partial class NpmVueMetadataProvider
{
    private readonly NpmValueMetadataProvider _values;
    private readonly ConcurrentDictionary<(string Project, string Specifier, string Mode, string? Query), CacheEntry<ComponentSnapshot>> _components = new();

    public NpmVueMetadataProvider(string? storeDirectory = null)
        => _values = new(storeDirectory, engineCacheDirectory: null, defaultStoreOwner: "Jolt");

    internal NpmVueMetadataProvider(string storeDirectory, string engineCacheDirectory)
        => _values = new(storeDirectory, engineCacheDirectory, defaultStoreOwner: "Jolt");

    internal Action<IReadOnlyDictionary<string, string?>>? ObserveQueryInputs
    {
        set => _values.ObserveQueryInputs = value;
    }

    internal NpmVueMetadataProvider(string storeDirectory, string portableCacheDirectory, Action observeChecker)
        => _values = new(storeDirectory, engineCacheDirectory: null, defaultStoreOwner: "Jolt")
        { PortableCacheDirectory = portableCacheDirectory, ObserveChecker = observeChecker };

    internal static string FindProject(string documentPath) => NpmValueMetadataProvider.FindProject(documentPath);

    public async Task<NpmVueComponent> ReadAsync(string documentPath, string moduleSpecifier, string exportName,
        CancellationToken cancellationToken = default)
    {
        var result = (await ReadComponentsAsync(documentPath, moduleSpecifier, [exportName], cancellationToken))[exportName];
        return result.Component ?? throw new InvalidOperationException(result.Error);
    }

    public async Task<IReadOnlyDictionary<string, NpmVueComponentResult>> ReadComponentsAsync(string documentPath,
        string moduleSpecifier, IReadOnlyList<string> exportNames, CancellationToken cancellationToken = default)
        => await ReadComponentsAsync(documentPath, moduleSpecifier, exportNames, null, cancellationToken);

    internal async Task<IReadOnlyDictionary<string, NpmVueComponentResult>> ReadComponentsAsync(string documentPath,
        string moduleSpecifier, IReadOnlyList<string> exportNames, Validation? validation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var names = exportNames.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (names.Length == 0)
            return new Dictionary<string, NpmVueComponentResult>(StringComparer.Ordinal);
        var metadata = await ReadComponentSnapshotAsync(documentPath, moduleSpecifier, JsonSerializer.Serialize(names), validation, cancellationToken);
        return metadata.Components!.ToDictionary(entry => entry.ExportName, entry => entry.Result, StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<string>> ListExportsAsync(string documentPath, string moduleSpecifier,
        CancellationToken cancellationToken = default)
        => await ListExportsAsync(documentPath, moduleSpecifier, null, cancellationToken);

    internal async Task<IReadOnlyList<string>> ListExportsAsync(string documentPath, string moduleSpecifier,
        Validation? validation, CancellationToken cancellationToken)
    {
        var metadata = await ReadComponentSnapshotAsync(documentPath, moduleSpecifier, null, validation, cancellationToken);
        return metadata.Exports!;
    }

    private Task<ComponentSnapshot> ReadComponentSnapshotAsync(string documentPath, string moduleSpecifier, string? query,
        Validation? validation, CancellationToken cancellationToken)
        => _values.ReadQueryAsync(documentPath, moduleSpecifier, "component", query, _components,
            typeof(NpmVueMetadataProvider), ReadComponentAsync, cancellationToken, validation: validation);

    private static async Task<ComponentSnapshot> ReadComponentAsync(string project, string moduleSpecifier, string mode,
        string? query, string store, string typescript, CancellationToken cancellationToken)
    {
        var result = await RunMetadataAsync(project, "component-metadata.mjs",
            [typescript, moduleSpecifier, query ?? "[]", WebIdlCarriers.Metadata], cancellationToken,
            typeof(NpmVueMetadataProvider), ["component-metadata.mjs", "package-checker.mjs", "declaration-documentation.mjs", "typescript-input.mjs", "runtime-module.mjs", "vue-contract.mjs", "web-idl-projection.mjs"]);
        return JsonSerializer.Deserialize<ComponentSnapshot>(result, JsonOptions)!;
    }

    private sealed record ComponentEntry(string ExportName, NpmVueComponentResult Result);

    private sealed record ComponentSnapshot(IReadOnlyList<ComponentEntry>? Components, IReadOnlyList<string>? Exports,
        string[] Inputs, string? ModulePath, Dictionary<string, string>? Realpaths) : IQuerySnapshot
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string?>? ObservedInputs { get; set; }
    }

    public Task<NpmValueExport> ReadValueAsync(string documentPath, string moduleSpecifier, string exportName,
        CancellationToken cancellationToken = default)
        => _values.ReadValueAsync(documentPath, moduleSpecifier, exportName, cancellationToken);

    public Task<IReadOnlyList<string>> ListValueExportsAsync(string documentPath, string moduleSpecifier,
        CancellationToken cancellationToken = default)
        => _values.ListValueExportsAsync(documentPath, moduleSpecifier, cancellationToken);

    public Task<IReadOnlyList<NpmValueExport>> ReadValuesAsync(string documentPath, string moduleSpecifier,
        IReadOnlyList<string> exportNames, CancellationToken cancellationToken = default)
        => _values.ReadValuesAsync(documentPath, moduleSpecifier, exportNames, cancellationToken);

    public Task<NpmValueProjections> ReadValueProjectionsAsync(string documentPath, string moduleSpecifier,
        IReadOnlyList<string> exportNames, CancellationToken cancellationToken = default)
        => _values.ReadValueProjectionsAsync(documentPath, moduleSpecifier, exportNames, cancellationToken);

    public Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        CancellationToken cancellationToken = default)
        => _values.ListSpecifiersAsync(documentPath, packageName, cancellationToken);

    public Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        string? requestedNamespace, CancellationToken cancellationToken = default)
        => _values.ListSpecifiersAsync(documentPath, packageName, requestedNamespace, cancellationToken);

    internal Task<IReadOnlyList<string>> ListSpecifiersAsync(string documentPath, string packageName,
        string? requestedNamespace, Validation validation, CancellationToken cancellationToken)
        => _values.ListSpecifiersAsync(documentPath, packageName, requestedNamespace, validation, cancellationToken);
}
