namespace Jazor.Emit;

/// <summary>File-system inputs consumed by the Netpack bundler.</summary>
internal sealed record BundleOptions(
    string InputDirectory,
    string ManifestPath,
    string OutputPath,
    string? SourceRoot = null,
    IReadOnlyList<string>? LibraryManifests = null);
