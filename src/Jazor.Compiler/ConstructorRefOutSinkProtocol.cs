using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// Preserves C# constructor <c>ref</c>/<c>out</c> write-back without changing JavaScript
/// constructor identity.
/// </summary>
/// <remarks>
/// A JavaScript <c>constructor</c> cannot return the ordinary array used by method ref/out
/// lowering because a returned object would replace <c>this</c>. The generated constructor
/// therefore receives a compiler-owned final sink array and copies writable parameters into it
/// before every return path and on normal completion. The caller owns reading that sink back into
/// the bound C# argument targets.
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
        protected override object VisitFunctionExpression(FunctionExpression node) => node;
        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node) => node;
        protected override object VisitFunctionDeclaration(FunctionDeclaration node) => node;
    }
}
