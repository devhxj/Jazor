using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RenderTree;

internal static class RazorVueOperationLocalCollector
{
    public static HashSet<ILocalSymbol> CollectDeclaredLocals(IEnumerable<IOperation> operations)
    {
        var declaredLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        foreach (var operation in operations)
            CollectDeclaredLocals(operation, declaredLocals);

        return declaredLocals;
    }

    public static HashSet<ILocalSymbol> CollectDeclaredLocals(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive)
    {
        var declaredLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        for (var index = startIndex; index < endExclusive; index++)
            CollectDeclaredLocals(operations[index], declaredLocals);

        return declaredLocals;
    }

    public static IEnumerable<IOperation> EnumerateSelfAndDescendants(IOperation operation)
        => EnumerateSelfAndDescendants(operation, includeLocalFunctionBodies: false);

    public static IEnumerable<IOperation> EnumerateSelfAndDescendants(
        IOperation operation,
        bool includeLocalFunctionBodies)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            yield break;

        yield return current;
        if (current is IAnonymousFunctionOperation ||
            (current is ILocalFunctionOperation && !includeLocalFunctionBodies))
        {
            yield break;
        }

        foreach (var child in EnumerateChildren(current))
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateSelfAndDescendants(child, includeLocalFunctionBodies))
                yield return nested;
        }
    }

    private static void CollectDeclaredLocals(
        IOperation operation,
        HashSet<ILocalSymbol> declaredLocals)
    {
        foreach (var current in EnumerateSelfAndDescendants(operation))
        {
            switch (current)
            {
                case IVariableDeclarationGroupOperation declarationGroup:
                    CollectVariableDeclarationGroupLocals(declarationGroup, declaredLocals);
                    break;
                case IVariableDeclarationOperation declarationOperation:
                    CollectVariableDeclarationLocals(declarationOperation, declaredLocals);
                    break;
                case IVariableDeclaratorOperation declarator:
                    declaredLocals.Add(declarator.Symbol);
                    break;
                case IForEachLoopOperation foreachLoop:
                    foreach (var local in foreachLoop.Locals)
                        declaredLocals.Add(local);
                    break;
                case IForLoopOperation forLoop:
                    foreach (var local in forLoop.Locals)
                        declaredLocals.Add(local);
                    break;
                case IUsingDeclarationOperation usingDeclaration when usingDeclaration.DeclarationGroup is not null:
                    CollectVariableDeclarationGroupLocals(usingDeclaration.DeclarationGroup, declaredLocals);
                    break;
                case ICatchClauseOperation catchClause:
                    CollectCatchDeclarationLocals(catchClause, declaredLocals);
                    break;
                case IDeconstructionAssignmentOperation deconstruction:
                    CollectDeconstructionTargetLocals(deconstruction.Target, declaredLocals);
                    break;
                case IDeclarationExpressionOperation declarationExpression:
                    CollectDeclarationExpressionLocals(declarationExpression, declaredLocals);
                    break;
            }
        }
    }

    private static void CollectVariableDeclarationGroupLocals(
        IVariableDeclarationGroupOperation declarationGroup,
        HashSet<ILocalSymbol> locals)
    {
        foreach (var declaration in declarationGroup.Declarations)
            CollectVariableDeclarationLocals(declaration, locals);
    }

    private static void CollectVariableDeclarationLocals(
        IVariableDeclarationOperation declaration,
        HashSet<ILocalSymbol> locals)
    {
        foreach (var declarator in declaration.Declarators)
            locals.Add(declarator.Symbol);
    }

    private static void CollectCatchDeclarationLocals(
        ICatchClauseOperation catchClause,
        HashSet<ILocalSymbol> locals)
    {
        switch (RazorVueOperationNormalizer.Unwrap(catchClause.ExceptionDeclarationOrExpression))
        {
            case ILocalReferenceOperation localReference:
                locals.Add(localReference.Local);
                break;
            case IVariableDeclaratorOperation declarator:
                locals.Add(declarator.Symbol);
                break;
        }
    }

    private static void CollectDeconstructionTargetLocals(
        IOperation target,
        HashSet<ILocalSymbol> locals)
    {
        switch (RazorVueOperationNormalizer.Unwrap(target))
        {
            case IDeclarationExpressionOperation declarationExpression:
                CollectDeclarationExpressionLocals(declarationExpression, locals);
                break;
            case ITupleOperation tuple:
                foreach (var element in tuple.Elements)
                    CollectDeconstructionTargetLocals(element, locals);
                break;
        }
    }

    private static void CollectDeclarationExpressionLocals(
        IDeclarationExpressionOperation declarationExpression,
        HashSet<ILocalSymbol> locals)
    {
        CollectDeclarationExpressionLocalTargets(declarationExpression.Expression, locals);
    }

    private static void CollectDeclarationExpressionLocalTargets(
        IOperation operation,
        HashSet<ILocalSymbol> locals)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case ILocalReferenceOperation localReference:
                locals.Add(localReference.Local);
                break;
            case ITupleOperation tuple:
                foreach (var element in tuple.Elements)
                    CollectDeclarationExpressionLocalTargets(element, locals);

                break;
            case IDeclarationExpressionOperation nestedDeclaration:
                CollectDeclarationExpressionLocals(nestedDeclaration, locals);
                break;
        }
    }

    private static IEnumerable<IOperation?> EnumerateChildren(IOperation operation)
    {
        foreach (var child in operation.ChildOperations)
            yield return child;

        switch (operation)
        {
            case IBlockOperation block:
                foreach (var child in block.Operations)
                    yield return child;
                break;
            case IConditionalOperation conditional:
                yield return conditional.Condition;
                yield return conditional.WhenTrue;
                yield return conditional.WhenFalse;
                break;
            case IWhileLoopOperation whileLoop:
                yield return whileLoop.Condition;
                yield return whileLoop.Body;
                break;
            case IForLoopOperation forLoop:
                foreach (var child in forLoop.Before)
                    yield return child;
                yield return forLoop.Condition;
                foreach (var child in forLoop.AtLoopBottom)
                    yield return child;
                yield return forLoop.Body;
                break;
            case IForEachLoopOperation forEachLoop:
                yield return forEachLoop.Collection;
                yield return forEachLoop.Body;
                break;
            case ISwitchOperation switchOperation:
                yield return switchOperation.Value;
                foreach (var switchCase in switchOperation.Cases)
                    yield return switchCase;
                break;
            case ISwitchCaseOperation switchCase:
                foreach (var clause in switchCase.Clauses)
                    yield return clause;
                foreach (var child in switchCase.Body)
                    yield return child;
                break;
            case ITryOperation tryOperation:
                yield return tryOperation.Body;
                foreach (var catchClause in tryOperation.Catches)
                    yield return catchClause;
                yield return tryOperation.Finally;
                break;
            case ICatchClauseOperation catchClause:
                yield return catchClause.ExceptionDeclarationOrExpression;
                yield return catchClause.Filter;
                yield return catchClause.Handler;
                break;
            case IUsingOperation usingOperation:
                yield return usingOperation.Resources;
                yield return usingOperation.Body;
                break;
            case IUsingDeclarationOperation usingDeclaration:
                yield return usingDeclaration.DeclarationGroup;
                break;
            case ILockOperation lockOperation:
                yield return lockOperation.LockedValue;
                yield return lockOperation.Body;
                break;
            case ILabeledOperation labeled:
                yield return labeled.Operation;
                break;
        }
    }
}
