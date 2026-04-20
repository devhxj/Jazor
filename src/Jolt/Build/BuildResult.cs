namespace Jolt.Build;

internal sealed class BuildResult
{
    public bool Success { get; init; }

    public string? OutDirectory { get; init; }

    public string? ManifestPath { get; init; }

    public IReadOnlyList<ChunkInfo> Chunks { get; init; } = [];

    public IReadOnlyList<AssetInfo> CssAssets { get; init; } = [];

    public IReadOnlyList<AssetInfo> StaticAssets { get; init; } = [];

    public IReadOnlyList<BuildDiagnostic> Diagnostics { get; init; } = [];

    public TimeSpan Duration { get; init; }

    public long TotalSize { get; init; }
}

internal sealed class ChunkInfo
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required long Size { get; init; }
    public bool IsEntry { get; init; }
    public bool IsDynamic { get; init; }
    public IReadOnlyList<string> Imports { get; init; } = [];
    public IReadOnlyList<string> Css { get; init; } = [];
    public string? SourceMapPath { get; init; }
}

internal sealed class AssetInfo
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required long Size { get; init; }
    public string? SourceMapPath { get; init; }
    public string? OriginalPath { get; init; }
    public IReadOnlyList<string> SourceModulePaths { get; init; } = [];
    public IReadOnlyList<string> OwnerChunkFilePaths { get; init; } = [];
    public string? OwnerChunkFilePath { get; init; }
}

internal sealed class BuildDiagnostic
{
    public required DiagnosticSeverity Severity { get; init; }
    public required string Message { get; init; }
    public string? File { get; init; }
    public (int Line, int Column)? Location { get; init; }
}

internal enum DiagnosticSeverity
{
    Error,
    Warning,
    Info
}
