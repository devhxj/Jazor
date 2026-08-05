// File: AstTransformationException.cs
// Purpose: Carries a source-aware failure from the lowering pipeline.
// 将不支持的语义与 Roslyn source location 关联，保证 fail-fast 诊断可定位、可解释。
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// 表示某个 Roslyn 符号无法转换为目标 JavaScript 语义。
/// </summary>
/// <remarks>
/// 异常保留 SymbolKind，便于诊断区分类型、方法、属性等失败位置；它不是用来吞掉错误的
/// fallback 容器，调用方应保留原始诊断上下文。
/// </remarks>
public sealed class SymbolTransformationException : Exception
{
    public SymbolKind Kind { get; }

    public SymbolTransformationException(SymbolKind kind, string? message) : base(message)
    {
        Kind = kind;
    }

    public SymbolTransformationException(SymbolKind kind, string? message, Exception innerException) : base(message, innerException)
    {
        Kind = kind;
    }
}

/// <summary>
/// 表示某个 Roslyn operation 无法转换为目标 JavaScript AST。
/// </summary>
/// <remarks>
/// OperationKind 用于定位具体语义节点。转换失败应在使用点明确暴露，而不是返回一个可能
/// 改变行为的近似节点。
/// </remarks>
public sealed class OperationTransformationException : Exception
{
    public OperationKind Kind { get; }

    public OperationTransformationException(OperationKind kind, string? message) : base(message)
    {
        Kind = kind;
    }

    public OperationTransformationException(OperationKind kind, string? message, Exception innerException) : base(message, innerException)
    {
        Kind = kind;
    }

    public OperationTransformationException(IOperation operation, string? message)
        : this((operation ?? throw new ArgumentNullException(nameof(operation))).Kind, message)
    {
        AttachLocationMetadata(this, operation.Syntax.GetLocation());
    }

    private static void AttachLocationMetadata(Exception exception, Location location)
    {
        var lineSpan = location.GetLineSpan();
        var path = !string.IsNullOrWhiteSpace(lineSpan.Path)
            ? lineSpan.Path
            : location.SourceTree?.FilePath;
        if (string.IsNullOrWhiteSpace(path))
            path = "<unknown>";

        exception.Data["location.path"] = path;
        exception.Data["location.startLine"] = lineSpan.StartLinePosition.Line + 1;
        exception.Data["location.startColumn"] = lineSpan.StartLinePosition.Character + 1;
        exception.Data["location.endLine"] = lineSpan.EndLinePosition.Line + 1;
        exception.Data["location.endColumn"] = lineSpan.EndLinePosition.Character + 1;
    }
}

/// <summary>
/// 表示某个 C# 语法节点无法转换为目标 JavaScript AST。
/// </summary>
/// <remarks>
/// 该异常用于语法辅助路径；若语义信息只能从 Roslyn operation 得到，应优先使用
/// OperationTransformationException，以便保留更准确的 operation 上下文。
/// </remarks>
public sealed class SyntaxNodeTransformationException : Exception
{
    public SyntaxKind Kind { get; }

    public SyntaxNodeTransformationException(SyntaxKind kind, string? message) : base(message)
    {
        Kind = kind;
    }

    public SyntaxNodeTransformationException(SyntaxKind kind, string? message, Exception innerException) : base(message, innerException)
    {
        Kind = kind;
    }
}
