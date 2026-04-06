namespace Jazor.Emit.SourceMaps;

internal sealed record SourceMapDocument(
    string File,
    IReadOnlyList<SourceMapSource> Sources,
    IReadOnlyList<SourceMapSegment> Segments);

internal sealed record SourceMapSource(
    string Path,
    string? Content);

internal sealed record SourceMapSegment(
    int GeneratedLine,
    int GeneratedColumn,
    int SourceIndex,
    int SourceLine,
    int SourceColumn);