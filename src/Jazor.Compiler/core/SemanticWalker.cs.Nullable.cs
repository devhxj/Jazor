// File: SemanticWalker.cs.Nullable.cs
// Purpose: Lowers supported Nullable<T> members while preserving null, throw, and evaluation-order behavior.
// Nullable 擦除为 JS 值或 nullish；不同重载的惰性/急切求值差异必须在这里显式表达。
using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Lowers the supported <c>Nullable&lt;T&gt;</c> members without creating a CLR nullable object.
/// </summary>
/// <remarks>
/// Nullable values erase to either the underlying JavaScript value or <c>null</c>/<c>undefined</c>.
/// The important distinction is evaluation timing: <c>Value</c> throws only for a missing value,
/// parameterless <c>GetValueOrDefault()</c> can use a direct fallback, while
/// <c>GetValueOrDefault(defaultValue)</c> must evaluate <paramref name="defaultValue"/> before
/// choosing the result because C# evaluates invocation arguments eagerly.
/// <para/>
/// Nullable 在运行时不是包装对象；这里保的是使用点结果、抛错行为与求值顺序，不能把
/// <c>??</c> 直接套到所有重载上，否则会漏掉显式默认参数的急切求值。
/// </remarks>
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
        // 用 IIFE 承载 throw 是因为 throw 是 statement；它只会在缺值分支执行。
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

    public Expression? CompileNullableGetValueOrDefaultWithDefault(
        ISymbol symbol,
        SenseArgument context,
        Expression? handler,
        Expression?[] args,
        IOperation? originOperation)
    {
        // C# evaluates the receiver and default argument before the invocation. Passing both as
        // IIFE arguments preserves left-to-right eager evaluation before `??` chooses its result.
        // A direct `handler ?? args[0]` would incorrectly skip side effects in defaultValue when
        // handler is populated. generated Compile mapping 保证这里恰有实例和一个已绑定实参。
        var nullableParameter = new Identifier("nullable");
        var defaultValueParameter = new Identifier("defaultValue");
        var coalesce = new LogicalExpression(
            Operator.NullishCoalescing,
            nullableParameter,
            defaultValueParameter);
        var eagerCoalesce = new ArrowFunctionExpression(
            NodeList.From<Node>(nullableParameter, defaultValueParameter),
            coalesce,
            expression: true,
            async: false);
        return new CallExpression(
            eagerCoalesce,
            NodeList.From<Expression>(handler!, args[0]!),
            optional: false);
    }
}
