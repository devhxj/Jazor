using System.Collections.Generic;

namespace Jazor.Common.SourceMaps;

public sealed record SourceMapDocument(
    string File,
    IReadOnlyList<SourceMapSource> Sources,
    IReadOnlyList<SourceMapSegment> Segments);

public sealed record SourceMapSource(
    string Path,
    string? Content);

public sealed record SourceMapSegment(
    int GeneratedLine,
    int GeneratedColumn,
    int SourceIndex,
    int SourceLine,
    int SourceColumn);
