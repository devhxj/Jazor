// File: RefOutReturnProtocol.cs
// Purpose: Implements the ordinary method ref/out caller-callee return-array protocol.
// 通过 compiler-owned slots 保留回写顺序与返回值，不试图重建 CLR 地址模型。
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// Implements the caller/callee protocol used to lower ordinary C# <c>ref</c>/<c>out</c>
/// methods into JavaScript.
/// </summary>
/// <remarks>
/// JavaScript has no writable argument slots. A lowered callee therefore returns an array:
/// <c>[returnValue?, refOrOut0, refOrOut1, ...]</c>. <c>VisitInvocation</c> owns the inverse
/// operation: it receives the array, writes each slot back to the already-materialized C# target,
/// and yields the original return value when one exists.
/// <para/>
/// 该数组是 compiler-owned protocol，不是公开 runtime API。返回槽与回写槽的顺序必须和
/// 调用端保持一致；嵌套 function/lambda 有自己的协议，重写时绝不能穿透其函数边界。
/// </remarks>
internal static class RefOutReturnProtocol
{
    // Callee 返回布局必须与 VisitInvocation 的 caller 回写索引一致；nested functions own
    // their return protocol. 这里重写的是当前函数的 return，不能意外改写词法嵌套函数。
    public static FunctionBody Apply(
        FunctionBody body,
        IReadOnlyList<Expression> refParameters,
        bool hasReturnValue)
    {
        var rewriter = new ReturnRewriter(refParameters, hasReturnValue);
        var rewritten = (FunctionBody)rewriter.Visit(body)!;
        if (hasReturnValue)
            return rewritten;

        // A void C# method can fall through without an explicit return. JavaScript would then
        // return undefined and lose all write-back slots, so append the protocol result on the
        // normal-completion path. 无显式 return 的正常落点也必须回传 ref/out 值。
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
