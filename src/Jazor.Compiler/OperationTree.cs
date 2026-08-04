using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// Traverses Roslyn operation trees while preserving nested function boundaries.
/// </summary>
/// <remarks>
/// Function-level facts such as <c>yield</c> and <c>await</c> must not leak from a nested
/// lambda/local function into its containing function. <see cref="IOperation.ChildOperations"/>
/// also omits a few switch-specific edges, so callers use this shared traversal rather than
/// rediscovering a partial tree shape at each lowering site.
/// </remarks>
internal static class OperationTree
{
    internal static bool ContainsYieldOperation(IOperation operation)
        => ContainsOperation(operation, static candidate =>
            candidate.Kind is OperationKind.YieldReturn or OperationKind.YieldBreak);

    internal static bool ContainsOperation(IOperation operation, Func<IOperation, bool> predicate)
    {
        if (predicate(operation))
            return true;

        foreach (var child in EnumerateContainedOperations(operation))
        {
            // Lambda/local-function bodies own their own async/generator declaration flags.
            if (child is IAnonymousFunctionOperation or ILocalFunctionOperation)
                continue;

            if (ContainsOperation(child, predicate))
                return true;
        }

        return false;
    }

    private static IEnumerable<IOperation> EnumerateContainedOperations(IOperation operation)
    {
        foreach (var child in operation.ChildOperations)
            yield return child;

        switch (operation)
        {
            case ISwitchExpressionOperation switchExpression:
                foreach (var arm in switchExpression.Arms)
                    yield return arm;
                break;

            case ISwitchExpressionArmOperation arm:
                yield return arm.Pattern;
                if (arm.Guard is not null)
                    yield return arm.Guard;
                yield return arm.Value;
                break;

            case ISwitchOperation switchOperation:
                if (switchOperation.Value is not null)
                    yield return switchOperation.Value;
                foreach (var @case in switchOperation.Cases)
                    yield return @case;
                break;

            case ISwitchCaseOperation switchCase:
                foreach (var clause in switchCase.Clauses)
                    yield return clause;
                foreach (var body in switchCase.Body)
                    yield return body;
                break;
        }
    }
}
