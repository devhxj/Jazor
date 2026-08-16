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

    /// <summary>
    /// Original Roslyn source location when the failing symbol has one.
    /// 保留 typed location 供上层产品直接传递，不能依赖异常 <see cref="Exception.Data"/> 还原位置。
    /// </summary>
    public Location SourceLocation { get; }

    public SymbolTransformationException(SymbolKind kind, string? message)
        : this(kind, message, Location.None)
    {
    }

    public SymbolTransformationException(SymbolKind kind, string? message, Exception innerException)
        : this(kind, message, Location.None, innerException)
    {
    }

    public SymbolTransformationException(SymbolKind kind, string? message, Location sourceLocation)
        : base(message)
    {
        Kind = kind;
        SourceLocation = sourceLocation ?? Location.None;
    }

    public SymbolTransformationException(
        SymbolKind kind,
        string? message,
        Location sourceLocation,
        Exception innerException)
        : base(message, innerException)
    {
        Kind = kind;
        SourceLocation = sourceLocation ?? Location.None;
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

    /// <summary>
    /// Original Roslyn operation location. Product boundaries can map this directly to authored
    /// source; <see cref="Exception.Data"/> remains only as a legacy diagnostic compatibility surface.
    /// 原始 operation 位置是正式契约，Data 中的行列信息不再作为跨层协议。
    /// </summary>
    public Location SourceLocation { get; }

    public OperationTransformationException(OperationKind kind, string? message)
        : this(kind, message, Location.None)
    {
    }

    public OperationTransformationException(OperationKind kind, string? message, Exception innerException)
        : this(kind, message, Location.None, innerException)
    {
    }

    public OperationTransformationException(OperationKind kind, string? message, Location sourceLocation)
        : base(message)
    {
        Kind = kind;
        SourceLocation = sourceLocation ?? Location.None;
        AttachLocationMetadata(this, SourceLocation);
    }

    public OperationTransformationException(
        OperationKind kind,
        string? message,
        Location sourceLocation,
        Exception innerException)
        : base(message, innerException)
    {
        Kind = kind;
        SourceLocation = sourceLocation ?? Location.None;
        AttachLocationMetadata(this, SourceLocation);
    }

    public OperationTransformationException(IOperation operation, string? message)
        : this(
            (operation ?? throw new ArgumentNullException(nameof(operation))).Kind,
            message,
            operation.Syntax.GetLocation())
    {
    }

    private static void AttachLocationMetadata(Exception exception, Location location)
    {
        if (location == Location.None)
            return;

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

    /// <summary>
    /// Original Roslyn syntax location when the failure is source-bound.
    /// </summary>
    public Location SourceLocation { get; }

    public SyntaxNodeTransformationException(SyntaxKind kind, string? message)
        : this(kind, message, Location.None)
    {
    }

    public SyntaxNodeTransformationException(SyntaxKind kind, string? message, Exception innerException)
        : this(kind, message, Location.None, innerException)
    {
    }

    public SyntaxNodeTransformationException(SyntaxKind kind, string? message, Location sourceLocation)
        : base(message)
    {
        Kind = kind;
        SourceLocation = sourceLocation ?? Location.None;
        AttachLocationMetadata(this, SourceLocation);
    }

    public SyntaxNodeTransformationException(
        SyntaxKind kind,
        string? message,
        Location sourceLocation,
        Exception innerException)
        : base(message, innerException)
    {
        Kind = kind;
        SourceLocation = sourceLocation ?? Location.None;
        AttachLocationMetadata(this, SourceLocation);
    }

    private static void AttachLocationMetadata(Exception exception, Location location)
    {
        if (location == Location.None)
            return;

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
