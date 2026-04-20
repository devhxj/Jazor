namespace Jolt.Build;

internal sealed class BuildManifest
{
    public required string Entry { get; init; }

    public IReadOnlyList<BuildManifestChunk> Chunks { get; init; } = [];

    public IReadOnlyList<string> Css { get; init; } = [];

    public IReadOnlyList<BuildManifestStaticAsset> StaticAssets { get; init; } = [];

    public long TotalSize { get; init; }
}

internal sealed class BuildManifestChunk
{
    public required string File { get; init; }

    public bool IsEntry { get; init; }

    public IReadOnlyList<string> Imports { get; init; } = [];

    public IReadOnlyList<string> Css { get; init; } = [];

    public string? SourceMap { get; init; }
}

internal sealed class BuildManifestStaticAsset
{
    public required string File { get; init; }

    public required string OriginalPath { get; init; }
}
