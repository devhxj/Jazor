using Acornima;
using Acornima.Ast;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Builds Vue-only slot AST framing after component expressions have been compiler-lowered.
/// 这里只组装 Vue 的最终 slot 外壳，C# 表达式语义仍由 SemanticWalker 负责。
/// </summary>
internal static class VueSlotAstFactory
{
    public static Expression NormalizeContent(Expression expression)
        // Vue slot content must be an array; concat flattens fragment-like values while the
        // nullish fallback keeps omitted children empty. slot 内容不能以 null 进入 vnode children。
        => new CallExpression(
            new MemberExpression(
                EmptyArray(),
                new Identifier("concat"),
                computed: false,
                optional: false),
            NodeList.From<Expression>(new LogicalExpression(
                Operator.NullishCoalescing,
                expression,
                EmptyArray())),
            optional: false);

    private static ArrayExpression EmptyArray()
        => new(NodeList.Empty<Expression?>());
}
