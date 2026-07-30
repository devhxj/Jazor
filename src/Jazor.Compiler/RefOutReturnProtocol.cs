using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

internal static class RefOutReturnProtocol
{
    // Callee 返回布局必须与 VisitInvocation 的 caller 回写索引一致；nested functions own their return protocol.
    public static FunctionBody Apply(
        FunctionBody body,
        IReadOnlyList<Expression> refParameters,
        bool hasReturnValue)
    {
        var rewriter = new ReturnRewriter(refParameters, hasReturnValue);
        var rewritten = (FunctionBody)rewriter.Visit(body)!;
        if (hasReturnValue)
            return rewritten;

        var statements = rewritten.Body.ToList();
        statements.Add(new ReturnStatement(CreateReturnExpression(null, refParameters, hasReturnValue)));
        return new FunctionBody(NodeList.From(statements), rewritten.Strict);
    }

    private static ArrayExpression CreateReturnExpression(
        Expression? returnValue,
        IReadOnlyList<Expression> refParameters,
        bool hasReturnValue)
    {
        var elements = new List<Expression>();
        if (hasReturnValue)
            elements.Add(returnValue!);
        elements.AddRange(refParameters);
        return new ArrayExpression(NodeList.From<Expression?>(elements));
    }

    private sealed class ReturnRewriter(
        IReadOnlyList<Expression> refParameters,
        bool hasReturnValue) : AstRewriter
    {
        protected override object? VisitReturnStatement(ReturnStatement node)
            => new ReturnStatement(CreateReturnExpression(node.Argument, refParameters, hasReturnValue));

        protected override object VisitFunctionExpression(FunctionExpression node) => node;
        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node) => node;
        protected override object VisitFunctionDeclaration(FunctionDeclaration node) => node;
    }
}
