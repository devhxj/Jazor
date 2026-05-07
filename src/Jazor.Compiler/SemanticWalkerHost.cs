using System.Collections.Generic;
using Acornima.Ast;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Optional host seam for consumers that need to project selected Roslyn operations
/// into a different runtime surface while still reusing SemanticWalker for the rest
/// of the expression tree.
/// </summary>
public abstract class SemanticWalkerHost
{
    public virtual Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
        => null;

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

    public virtual Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
        => null;

    public virtual Expression? RewriteInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument)
        => null;
}
