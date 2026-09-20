namespace Jazor.Emit;

/// <summary>File-system inputs consumed by the Netpack bundler.</summary>
internal sealed record BundleOptions(
    string ProjectRoot,
    string ManifestPath,
    string OutputPath,
    IReadOnlyList<string>? LibraryManifests = null,
    LibraryAssets? MaterializedLibraries = null,
    bool SourceMaps = true,
    bool Minify = false);
