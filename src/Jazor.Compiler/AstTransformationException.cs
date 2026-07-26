using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

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

    private static void AttachLocationMetadata(Exception exception, Location? location)
    {
        if (location is null)
        {
            exception.Data["location.path"] = "<unknown>";
            return;
        }

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
