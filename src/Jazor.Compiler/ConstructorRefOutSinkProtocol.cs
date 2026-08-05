// File: ConstructorRefOutSinkProtocol.cs
// Purpose: Implements constructor-specific ref/out write-back through a compiler-owned sink.
// JavaScript constructor 不能返回普通 ref/out 数组；本文件保住实例 identity 与 C# 回写结果。
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// Preserves C# constructor <c>ref</c>/<c>out</c> write-back without changing JavaScript
/// constructor identity. 在保持 JavaScript <c>new</c> 结果为实例的前提下模拟 C# 回写。
/// </summary>
/// <remarks>
/// A JavaScript <c>constructor</c> cannot return the ordinary array used by method ref/out
/// lowering because a returned object would replace <c>this</c>. The generated constructor
/// therefore receives a compiler-owned final sink array and copies writable parameters into it
/// before every return path and on normal completion. The caller owns reading that sink back into
/// the bound C# argument targets.
/// <para/>
/// 不能复用普通方法的 <c>[returnValue, ...]</c> 协议：JavaScript constructor 返回对象会替换
/// <c>this</c>，从而破坏实例创建。sink 是额外的 compiler-owned 最后参数，调用端创建实例后
/// 才按固定槽位回写到 C# 的 <c>ref/out</c> 目标。
/// </remarks>
internal static class ConstructorRefOutSinkProtocol
{
    public static FunctionBody Apply(
        FunctionBody body,
        IReadOnlyList<Expression> writableParameters,
        Identifier sink)
    {
        var rewriter = new ReturnRewriter(writableParameters, sink);
        var rewritten = (FunctionBody)rewriter.Visit(body)!;
        var statements = rewritten.Body.ToList();
        statements.AddRange(CreateSinkWrites(writableParameters, sink));
        return new FunctionBody(NodeList.From(statements), rewritten.Strict);
    }

    private static IReadOnlyList<Statement> CreateSinkWrites(
        IReadOnlyList<Expression> writableParameters,
        Identifier sink)
    {
        var statements = new List<Statement>(writableParameters.Count);
        for (var index = 0; index < writableParameters.Count; index++)
        {
            statements.Add(new NonSpecialExpressionStatement(
                new AssignmentExpression(
                    Operator.Assignment,
                    new MemberExpression(
                        sink,
                        new NumericLiteral(index, index.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                        computed: true,
                        optional: false),
                    writableParameters[index])));
        }

        return statements;
    }

    private sealed class ReturnRewriter(
        IReadOnlyList<Expression> writableParameters,
        Identifier sink) : AstRewriter
    {
        protected override object? VisitReturnStatement(ReturnStatement node)
        {
            if (node.Argument is not null)
            {
                throw new NotSupportedException(
                    "Constructor ref/out lowering received a value-return statement, which is not valid for a C# constructor.");
            }

            var statements = CreateSinkWrites(writableParameters, sink).ToList();
            statements.Add(new ReturnStatement(null));
            return new NestedBlockStatement(NodeList.From(statements));
        }

        // Nested callable bodies establish their own ref/out protocol and must not write to the
        // constructor's sink merely because they appear lexically inside the constructor body.
        // 嵌套函数在语法上位于构造器内，不代表其 return 属于构造器的 sink 生命周期。
        protected override object VisitFunctionExpression(FunctionExpression node) => node;
        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node) => node;
        protected override object VisitFunctionDeclaration(FunctionDeclaration node) => node;
    }
}
