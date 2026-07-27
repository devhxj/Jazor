using System.Collections.Generic;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Optional host seam for consumers that need to project selected Roslyn operations
/// into a different runtime surface while still reusing SemanticWalker for the rest
/// of the expression tree.
/// </summary>
public abstract class SemanticWalkerHost
{
    public virtual Expression? RewriteConversionPreorder(IConversionOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        => null;

    public virtual bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
        => false;

    public virtual Expression? RewriteObjectCreation(
        IObjectCreationOperation operation,
        SenseArgument argument,
        IReadOnlyList<Expression> arguments)
        => null;

    public virtual void ObserveTypeReference(ITypeSymbol type, SenseArgument argument)
    {
    }

    public virtual VariableDeclarator? RewriteVariableDeclaratorPreorder(IVariableDeclaratorOperation operation, SenseArgument argument)
        => null;

    public virtual bool ShouldSkipVariableDeclarator(IVariableDeclaratorOperation operation, SenseArgument argument)
        => false;

    public virtual Identifier? RewriteLocalDeclarationIdentifier(ILocalSymbol local, IOperation operation, SenseArgument argument)
        => null;

    public virtual Identifier? RewriteCatchClauseParameterIdentifier(ICatchClauseOperation operation, ILocalSymbol local, SenseArgument argument)
        => null;

    public virtual Expression? RewriteSimpleAssignmentPreorder(ISimpleAssignmentOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteInvocationArgumentPreorder(
        IInvocationOperation operation,
        IArgumentOperation argumentOperation,
        int argumentIndex,
        SenseArgument argument)
        => null;

    public virtual bool ShouldSkipLocalFunctionDeclaration(ILocalFunctionOperation operation, SenseArgument argument)
        => false;

    public virtual Expression? RewriteFieldReference(
        IFieldReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => null;

    public virtual Expression? RewritePropertyReference(
        IPropertyReferenceOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
        => null;

    public virtual Expression? RewriteMethodReference(
        IMethodReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => null;

    public virtual Expression? RewriteMethodReferencePreorder(IMethodReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
        => null;

    public virtual Expression? RewriteInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument)
        => null;
}
