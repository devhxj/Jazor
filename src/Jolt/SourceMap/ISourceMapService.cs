namespace Jolt.SourceMap;

/// <summary>
/// Source Map registry and lookup service for dev-server outputs and debugger consumers.
/// Line and column values are zero-based, matching Source Map v3 coordinates.
/// </summary>
public interface ISourceMapService
{
    void Register(string generatedPath, string sourceMapJson);

    void Unregister(string generatedPath);

    void Clear();

    string? GetSourceMapJson(string generatedPath);

    OriginalPosition? OriginalPositionFor(string generatedPath, int line, int column);

    GeneratedPosition? GeneratedPositionFor(string sourcePath, int line, int column);

    string? GetSourceContent(string generatedPath, int sourceIndex);
}

public sealed record OriginalPosition(
    string SourcePath,
    int Line,
    int Column,
    int SourceIndex);

public sealed record GeneratedPosition(
    string GeneratedPath,
    int Line,
    int Column);
