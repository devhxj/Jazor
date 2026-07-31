using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class ChildrenToSlotSemanticWalkerHost : SemanticWalkerHost
{
    public static ChildrenToSlotSemanticWalkerHost Instance { get; } = new();

    private ChildrenToSlotSemanticWalkerHost()
    {
    }

    public override Expression? RewriteInvocationIntrinsic(
        IInvocationOperation operation,
        Expression? instance,
        IReadOnlyList<Expression> arguments,
        SemanticInvocationLoweringContext context)
        => ChildrenToSlotIntrinsic.TryBuild(
            operation,
            operation.TargetMethod,
            arguments,
            context,
            out var expression)
                ? expression
                : null;
}
