using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
    public Expression? CompileNullableGetValueOrDefault(
        ISymbol symbol,
        SenseArgument context,
        Expression? handler,
        Expression?[] args,
        IOperation? originOperation)
    {
        if (handler is null)
            throw new InvalidOperationException("Nullable<T>.GetValueOrDefault() requires an instance handler.");
        if (args.Length != 0)
            throw new InvalidOperationException("Nullable<T>.GetValueOrDefault() does not accept explicit arguments.");
        if (originOperation is not IInvocationOperation invocation ||
            invocation.Instance?.Type is not INamedTypeSymbol nullableType ||
            nullableType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
        {
            throw new InvalidOperationException(
                "Nullable<T>.GetValueOrDefault() requires a direct invocation with a closed nullable receiver type.");
        }

        var underlyingType = nullableType.TypeArguments[0];
        var defaultValue = BuildDefaultValueExpression(invocation, underlyingType, context);
        return new LogicalExpression(Operator.NullishCoalescing, handler, defaultValue);
    }
}
