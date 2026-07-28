using Acornima;
using Acornima.Ast;

namespace Jazor.RazorVue.RazorSdk;

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
