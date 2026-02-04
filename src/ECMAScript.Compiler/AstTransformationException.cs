using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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