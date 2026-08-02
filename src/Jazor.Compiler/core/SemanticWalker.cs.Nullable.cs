using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
    public Expression? CompileNullableValue(
        ISymbol symbol,
        SenseArgument context,
        Expression? handler,
        Expression?[] args,
        IOperation? originOperation)
    {
        if (handler is null)
            throw new InvalidOperationException("Nullable<T>.Value requires an instance handler.");
        if (args.Length != 0)
            throw new InvalidOperationException("Nullable<T>.Value does not accept explicit arguments.");
        if (originOperation is not IPropertyReferenceOperation propertyReference ||
            propertyReference.Instance?.Type is not INamedTypeSymbol nullableType ||
            nullableType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
        {
            throw new InvalidOperationException(
                "Nullable<T>.Value requires a direct property reference with a closed nullable receiver type.");
        }

        // `??` evaluates the receiver exactly once. The throw remains inside the right operand so
        // populated nullable values preserve their direct value representation and control flow.
        var missingValueThrow = new ThrowStatement(
            new NewExpression(
                new Identifier("Error"),
                NodeList.From<Expression>(
                    CreateStringLiteral("InvalidOperationException: Nullable object must have a value."))));
        var throwBody = new FunctionBody(NodeList.From<Statement>(missingValueThrow), strict: true);
        var throwExpression = new CallExpression(
            new ArrowFunctionExpression(
                NodeList.From<Node>(),
                throwBody,
                expression: false,
                async: false),
            NodeList.From<Expression>(),
            optional: false);
        return new LogicalExpression(Operator.NullishCoalescing, handler, throwExpression);
    }

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
