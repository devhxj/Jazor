using System.Collections.Generic;

namespace Jazor.Compiler;

internal sealed record GeneratedSourceMap(
    string File,
    IReadOnlyList<GeneratedSourceMapSource> Sources,
    IReadOnlyList<GeneratedSourceMapSegment> Segments);

internal sealed record GeneratedSourceMapSource(
    string Path,
    string? Content);

internal sealed record GeneratedSourceMapSegment(
    int GeneratedLine,
    int GeneratedColumn,
    int SourceIndex,
    int SourceLine,
    int SourceColumn);
