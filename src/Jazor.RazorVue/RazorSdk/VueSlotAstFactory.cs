using Acornima;
using Acornima.Ast;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>Builds Vue-only slot AST framing after component expressions have been compiler-lowered.</summary>
internal static class VueSlotAstFactory
{
    public static Expression NormalizeContent(Expression expression)
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
