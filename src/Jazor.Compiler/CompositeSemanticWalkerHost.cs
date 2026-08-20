// File: CompositeSemanticWalkerHost.cs
// Purpose: Composes multiple product hosts for SemanticWalker extension points.
// 固定 rewrite 的 first-handler-wins 与观察通知 fan-out 规则，避免宿主处理顺序含糊。
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Composes ordered host extensions without exposing <see cref="SemanticWalker"/> for inheritance.
/// </summary>
/// <remarks>
/// Rewrite hooks use first-handler-wins ordering. Observation hooks fan out to every host, while
/// boolean skip/claim hooks use logical OR. Hosts should therefore be ordered from the most
/// specific product projection to broader protocol projections.
/// </remarks>
public class CompositeSemanticWalkerHost : SemanticWalkerHost
{
    private readonly IReadOnlyList<SemanticWalkerHost> _hosts;

    public CompositeSemanticWalkerHost(params SemanticWalkerHost[] hosts)
    {
        _hosts = hosts ?? throw new ArgumentNullException(nameof(hosts));
    }

    public override Expression? RewriteConversionPreorder(IConversionOperation operation, SenseArgument argument)
        => First(host => host.RewriteConversionPreorder(operation, argument));

    public override Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        => First(host => host.RewriteObjectCreationPreorder(operation, argument));

    public override bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
        => _hosts.Any(host => host.ShouldRewriteObjectCreation(operation));

    public override Expression? RewriteObjectCreation(
        IObjectCreationOperation operation,
        SenseArgument argument,
        IReadOnlyList<Expression> arguments)
        => First(host => host.RewriteObjectCreation(operation, argument, arguments));

    public override void ObserveTypeReference(ITypeSymbol type, SenseArgument argument)
    {
        foreach (var host in _hosts)
            host.ObserveTypeReference(type, argument);
    }

    public override VariableDeclarator? RewriteVariableDeclaratorPreorder(
        IVariableDeclaratorOperation operation,
        SenseArgument argument)
        => First(host => host.RewriteVariableDeclaratorPreorder(operation, argument));

    public override bool ShouldSkipVariableDeclarator(
        IVariableDeclaratorOperation operation,
        SenseArgument argument)
        => _hosts.Any(host => host.ShouldSkipVariableDeclarator(operation, argument));

    public override Identifier? RewriteLocalDeclarationIdentifier(
        ILocalSymbol local,
        IOperation operation,
        SenseArgument argument)
        => First(host => host.RewriteLocalDeclarationIdentifier(local, operation, argument));

    public override Identifier? RewriteCatchClauseParameterIdentifier(
        ICatchClauseOperation operation,
        ILocalSymbol local,
        SenseArgument argument)
        => First(host => host.RewriteCatchClauseParameterIdentifier(operation, local, argument));

    public override Expression? RewriteSimpleAssignmentPreorder(
        ISimpleAssignmentOperation operation,
        SenseArgument argument)
        => First(host => host.RewriteSimpleAssignmentPreorder(operation, argument));

    public override Expression? RewriteSimpleAssignmentPostorder(
        ISimpleAssignmentOperation operation,
        SenseArgument argument,
        Expression value)
        => First(host => host.RewriteSimpleAssignmentPostorder(operation, argument, value));

    public override Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
        => First(host => host.RewriteLocalReference(operation, argument));

    public override Expression? RewriteParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
        => First(host => host.RewriteParameterReference(operation, argument));

    public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
        => First(host => host.RewriteInvocationPreorder(operation, argument));

    public override Expression? RewriteInvocationArgumentPreorder(
        IInvocationOperation operation,
        IArgumentOperation argumentOperation,
        int argumentIndex,
        SenseArgument argument)
        => First(host => host.RewriteInvocationArgumentPreorder(operation, argumentOperation, argumentIndex, argument));

    public override bool ShouldSkipLocalFunctionDeclaration(
        ILocalFunctionOperation operation,
        SenseArgument argument)
        => _hosts.Any(host => host.ShouldSkipLocalFunctionDeclaration(operation, argument));

    public override Expression? RewriteFieldReference(
        IFieldReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => First(host => host.RewriteFieldReference(operation, argument, instance));

    public override Expression? RewritePropertyReference(
        IPropertyReferenceOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
        => First(host => host.RewritePropertyReference(operation, argument, instance, arguments));

    public override Expression? RewriteMethodReference(
        IMethodReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => First(host => host.RewriteMethodReference(operation, argument, instance));

    public override Expression? RewriteMethodReferencePreorder(
        IMethodReferenceOperation operation,
        SenseArgument argument)
        => First(host => host.RewriteMethodReferencePreorder(operation, argument));

    public override Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
        => First(host => host.RewriteInvocation(operation, argument, instance, arguments));

    public override Expression? RewriteInvocationIntrinsic(
        IInvocationOperation operation,
        Expression? instance,
        IReadOnlyList<Expression> arguments,
        SemanticInvocationLoweringContext context)
        => First(host => host.RewriteInvocationIntrinsic(operation, instance, arguments, context));

    public override Expression? RewriteInstanceReference(
        IInstanceReferenceOperation operation,
        SenseArgument argument)
        => First(host => host.RewriteInstanceReference(operation, argument));

    public override Expression? RewriteEventAssignment(
        IEventAssignmentOperation operation,
        SenseArgument argument)
        => First(host => host.RewriteEventAssignment(operation, argument));

    public override Expression? RewriteEventReference(
        IEventReferenceOperation operation,
        SenseArgument argument)
        => First(host => host.RewriteEventReference(operation, argument));

    private TNode? First<TNode>(Func<SemanticWalkerHost, TNode?> rewrite)
        where TNode : Node
    {
        foreach (var host in _hosts)
        {
            if (rewrite(host) is TNode node)
                return node;
        }

        return null;
    }
}
