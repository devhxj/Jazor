namespace Jazor.Compiler;

/// <summary>
/// Tracks the primary C# source location that produced a JavaScript AST node.
/// Coordinates are zero-based and map to the Roslyn source span.
/// </summary>
internal sealed record SourceOrigin(
    string? SourcePath,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    string? Name = null,
    bool IsSynthetic = false);
