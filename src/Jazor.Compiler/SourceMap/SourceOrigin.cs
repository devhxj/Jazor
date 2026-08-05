// File: SourceOrigin.cs
// Purpose: Represents the source-level origin attached to a lowered AST node.
// lowering 同时产出 AST shape 与调试锚点；emit 阶段只消费该信息，不应重新猜测来源。
namespace Jazor.Compiler;

/// <summary>
/// Tracks the primary C# source location that produced a JavaScript AST node.
/// Coordinates are zero-based and map to the Roslyn source span.
/// </summary>
/// <remarks>
/// SourceOrigin 记录的是“主要来源”，不是完整的语法树映射。一个 synthetic 节点可能由多个
/// C# operation 共同产生，因此应锚定到触发该 lowering 的 operation，而不是虚构一段源代码。
/// </remarks>
public sealed record SourceOrigin(
    string? SourcePath,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    string? Name = null,
    bool IsSynthetic = false);
