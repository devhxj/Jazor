using System;
using Microsoft.CodeAnalysis;

public sealed class SymbolTransformationException : Exception
{
    public ISymbol Symbol { get; }

    public SymbolTransformationException(ISymbol symbol, string? message) : base(message)
    {
        Symbol = symbol;
    }

    public SymbolTransformationException(ISymbol symbol, string? message, Exception innerException) : base(message, innerException)
    {
        Symbol = symbol;
    }
}

public sealed class OperationTransformationException : Exception
{
    public IOperation? Operation { get; }

    public OperationTransformationException(IOperation? operation, string? message) : base(message)
    {
        Operation = operation;
    }

    public OperationTransformationException(IOperation? operation, string? message, Exception innerException) : base(message, innerException)
    {
        Operation = operation;
    }
}

public sealed class SyntaxNodeTransformationException : Exception
{
    public SyntaxNode? Node { get; }

    public SyntaxNodeTransformationException(SyntaxNode? node, string? message) : base(message)
    {
        Node = node;
    }

    public SyntaxNodeTransformationException(SyntaxNode? node, string? message, Exception innerException) : base(message, innerException)
    {
        Node = node;
    }
}