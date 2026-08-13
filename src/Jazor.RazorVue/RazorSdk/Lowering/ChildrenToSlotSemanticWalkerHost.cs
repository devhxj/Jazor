using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Supplies slot-specific intrinsic rewrites to the compiler host pipeline.
/// 只认领官方 Razor children transport 调用，普通 invocation 不会被此 host 改写。
/// </summary>
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
