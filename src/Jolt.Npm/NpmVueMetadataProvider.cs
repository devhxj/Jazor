using System.Collections.Concurrent;
using System.Text.Json;
using Jazor.Npm;
using static Jazor.Npm.NpmValueMetadataProvider;

namespace Jolt.Npm;

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

    public async Task<NpmVueComponent> ReadAsync(string documentPath, string moduleSpecifier, string exportName,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadComponentSnapshotAsync(documentPath, moduleSpecifier, exportName, cancellationToken);
        return metadata.Component!;
    }

    public async Task<IReadOnlyList<string>> ListExportsAsync(string documentPath, string moduleSpecifier,
        CancellationToken cancellationToken = default)
    {
        var metadata = await ReadComponentSnapshotAsync(documentPath, moduleSpecifier, null, cancellationToken);
        return metadata.Exports!;
    }

    private Task<ComponentSnapshot> ReadComponentSnapshotAsync(string documentPath, string moduleSpecifier, string? exportName,
        CancellationToken cancellationToken)
        => _values.ReadQueryAsync(documentPath, moduleSpecifier, "component", exportName, _components,
            typeof(NpmVueMetadataProvider), ReadComponentAsync, cancellationToken);

    private static async Task<ComponentSnapshot> ReadComponentAsync(string project, string moduleSpecifier, string mode,
        string? exportName, string store, string typescript, CancellationToken cancellationToken)
    {
        var result = await RunMetadataAsync(project, "component-metadata.mjs",
            [typescript, moduleSpecifier, exportName ?? "", WebIdlCarriers.Metadata], cancellationToken,
            typeof(NpmVueMetadataProvider), ["component-metadata.mjs", "package-checker.mjs", "vue-contract.mjs", "web-idl-projection.mjs"]);
        return JsonSerializer.Deserialize<ComponentSnapshot>(result, JsonOptions)!;
    }

    private sealed record ComponentSnapshot(NpmVueComponent? Component, IReadOnlyList<string>? Exports,
        string[] Inputs, string? ModulePath, Dictionary<string, string>? Realpaths) : IQuerySnapshot;

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
}
