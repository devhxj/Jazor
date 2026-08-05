// File: SemanticWalker.cs.Argument.cs
// Purpose: Lowers bound invocation and constructor arguments into JavaScript call positions.
// 保留 C# 源码求值顺序、具名参数槽位重排与 ref/out 回写目标单次物化。
using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// 提供按 Roslyn 已绑定形参重组调用实参的通用 lowering。
/// </summary>
/// <remarks>
/// C# 具名实参仍按源码书写顺序求值，但最终进入由 <see cref="IArgumentOperation.Parameter"/>
/// 指定的形参槽位。JavaScript 没有具名调用语法，因此重排时必须先在第一个实参位置缓存
/// 全部源码实参，再按形参顺序消费缓存，不能直接排序 AST 而改变副作用顺序。
/// <para/>
/// 例如 <c>M(second: Next(), first: Read())</c> 必须先执行 <c>Next()</c>、再执行
/// <c>Read()</c>，但 JavaScript 调用仍必须成为 <c>M(firstValue, secondValue)</c>。
/// 本文件同时承载 <c>ref/out</c> 调用端协议：callee 返回回写值，caller 只对已经物化的
/// JavaScript location 写回，避免再次计算数组下标、属性 receiver 等可能有副作用的目标。
/// </remarks>
public partial class SemanticWalker
{
    private sealed record LoweredBoundArgument(
        IArgumentOperation Operation,
        Expression Value,
        Expression? WriteBackTarget);

    private LoweredBoundArgument LowerBoundArgument(
        IOperation ownerOperation,
        IArgumentOperation operation,
        SenseArgument argument,
        Expression? rewrittenValue = null)
    {
        var parameter = operation.Parameter!;
        var refKind = parameter.RefKind;
        var argumentContext = refKind is RefKind.Out
            ? argument.With(Sense.OutParameter)
            : argument;

        if (refKind is not (RefKind.Ref or RefKind.Out))
        {
            return new LoweredBoundArgument(
                operation,
                rewrittenValue ?? TranslateTupleForTarget(operation.Value, parameter.Type, argumentContext),
                null);
        }

        if (operation.Value is IDiscardOperation)
        {
            // `out _` has no caller-visible storage. Its callee slot still exists, but C# does
            // not read an incoming value before the callee assigns it.
            // `out _` 没有回写目标；传入 undefined 仅用于保留 callee 参数位置，并不表示
            // C# 会读取该值。
            return new LoweredBoundArgument(operation, CreateUndefined(), null);
        }

        List<Expression> initializations;
        Expression writeBackTarget;
        if (rewrittenValue is null && UnwrapImplicitConversions(operation.Value) is IArrayElementReferenceOperation arrayElement)
        {
            // Array locations may contain a side-effecting receiver, index, from-end offset, or
            // intermediate dimension. Reuse the mutation-target lowering so the call read and
            // protocol write-back share one materialized JavaScript location.
            initializations = [];
            writeBackTarget = BuildArrayElementMutationTarget(
                arrayElement,
                argument,
                initializations,
                cacheForRepeatedReadWrite: true);
        }
        else
        {
            var translatedValue = rewrittenValue ?? TranslateTupleForTarget(
                operation.Value,
                parameter.Type,
                argumentContext);
            initializations = [];
            writeBackTarget = PrepareRepeatedReadWriteTarget(
                translatedValue,
                ownerOperation,
                argument,
                initializations);
        }

        if (!IsRefOutWriteBackTarget(writeBackTarget))
        {
            return HandleTransformationFailure<LoweredBoundArgument>(
                operation.Value,
                $"ref/out argument for '{parameter.Name}' requires an assignable JavaScript location after lowering. " +
                "Expose a writable field, array element, or ref-return member with a direct JavaScript location.");
        }

        var callValue = refKind == RefKind.Out
            ? CreateOutArgumentValue(initializations)
            : CreateRefArgumentValue(initializations, writeBackTarget);
        return new LoweredBoundArgument(operation, callValue, writeBackTarget);
    }

    private static bool IsRefOutWriteBackTarget(Expression expression)
        => expression is Identifier or MemberExpression { Optional: false };

    private static Expression CreateRefArgumentValue(
        IReadOnlyList<Expression> initializations,
        Expression writeBackTarget)
    {
        if (initializations.Count == 0)
            return writeBackTarget;

        var expressions = new List<Expression>(initializations.Count + 1);
        expressions.AddRange(initializations);
        expressions.Add(writeBackTarget);
        return new SequenceExpression(NodeList.From(expressions));
    }

    private static Expression CreateOutArgumentValue(IReadOnlyList<Expression> initializations)
    {
        if (initializations.Count == 0)
            return CreateUndefined();

        var expressions = new List<Expression>(initializations.Count + 1);
        expressions.AddRange(initializations);
        expressions.Add(CreateUndefined());
        return new SequenceExpression(NodeList.From(expressions));
    }

    private static Expression CreateUndefined()
        => new Identifier("undefined");

    private static bool RequiresBoundArgumentCanonicalization(IReadOnlyList<IArgumentOperation> arguments)
    {
        var lastOrdinal = -1;
        foreach (var argument in arguments)
        {
            var ordinal = argument.Parameter!.Ordinal;
            if (ordinal < lastOrdinal)
                return true;

            lastOrdinal = ordinal;
        }

        return false;
    }

    private static bool RequiresBoundArgumentCanonicalization(IReadOnlyList<LoweredBoundArgument> arguments)
    {
        var lastOrdinal = -1;
        foreach (var argument in arguments)
        {
            var ordinal = argument.Operation.Parameter!.Ordinal;
            if (ordinal < lastOrdinal)
                return true;

            lastOrdinal = ordinal;
        }

        return false;
    }

    private static int GetLastSuppliedParameterOrdinal(IReadOnlyList<IArgumentOperation> arguments)
    {
        var lastOrdinal = -1;
        foreach (var argument in arguments)
        {
            if (argument.ArgumentKind != ArgumentKind.DefaultValue)
                lastOrdinal = Math.Max(lastOrdinal, argument.Parameter!.Ordinal);
        }

        return lastOrdinal;
    }

    private IReadOnlyList<LoweredBoundArgument> CanonicalizeBoundArguments(
        IOperation ownerOperation,
        IReadOnlyList<LoweredBoundArgument> arguments,
        SenseArgument argument)
    {
        if (arguments.Count <= 1 || !RequiresBoundArgumentCanonicalization(arguments))
            return arguments;

        var evaluations = new List<Expression>(arguments.Count + 1);
        var cached = new List<(LoweredBoundArgument Argument, int SourceIndex)>(arguments.Count);
        for (var sourceIndex = 0; sourceIndex < arguments.Count; sourceIndex++)
        {
            var source = arguments[sourceIndex];
            var parameter = source.Operation.Parameter!;
            var temporary = new Identifier(AllocateUniqueName(
                ownerOperation,
                argument,
                LoweringSite.BoundArgumentTemp(parameter.Ordinal, sourceIndex)));
            argument.AddVarDeclarator(new VariableDeclarator(temporary, null), _recursionDepth);
            evaluations.Add(new AssignmentExpression(Operator.Assignment, temporary, source.Value));
            cached.Add((source with { Value = temporary }, sourceIndex));
        }

        var ordered = cached
            .OrderBy(static item => item.Argument.Operation.Parameter!.Ordinal)
            .ThenBy(static item => item.SourceIndex)
            .Select(static item => item.Argument)
            .ToList();

        // The callee (including an instance receiver) is evaluated before its first argument.
        // Put the source-order assignments in the first emitted argument so `receiver` still
        // runs first, then each source argument runs once, and only then the reordered values
        // reach the callee. Nesting this SequenceExpression avoids an IIFE while preserving
        // C# evaluation order. 将缓存嵌入首个实参是刻意的顺序约束，不能改成调用前的普通语句。
        evaluations.Add(ordered[0].Value);
        ordered[0] = ordered[0] with
        {
            Value = new SequenceExpression(NodeList.From(evaluations))
        };

        return ordered;
    }
}
