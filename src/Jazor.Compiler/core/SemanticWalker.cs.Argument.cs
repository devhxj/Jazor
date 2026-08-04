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
/// </remarks>
public partial class SemanticWalker
{
    private sealed record LoweredBoundArgument(
        IArgumentOperation Operation,
        Expression Value,
        Expression? WriteBackTarget);

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
        // Nesting the source-order cache here retains that ordering without an IIFE.
        evaluations.Add(ordered[0].Value);
        ordered[0] = ordered[0] with
        {
            Value = new SequenceExpression(NodeList.From(evaluations))
        };

        return ordered;
    }
}
