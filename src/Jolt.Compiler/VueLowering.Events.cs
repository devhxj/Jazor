using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jolt.Compiler;

internal sealed partial class VueLowering
{
    private static Expression LowerDomEvent(string name, IInvocationOperation operation, IReadOnlyList<Expression> arguments)
    {
        Expression handler = arguments[0];
        if (arguments.Count == 1)
            return Array(String(name), handler);

        var runHandler = Id("$joltHandler");
        var runModifiers = Id("$joltModifiers");
        var domEvent = Id("$joltEvent");
        var delegateType = (INamedTypeSymbol)operation.TargetMethod.Parameters[0].Type;
        var invoke = delegateType.DelegateInvokeMethod!.Parameters.Length == 0
            ? Call(runHandler)
            : Call(runHandler, domEvent);

        // Capture both projected callbacks when rendering. Returning the handler result
        // preserves Vue's promise error handling for async event methods.
        var listener = new ArrowFunctionExpression(NodeList.From<Node>(domEvent),
            new FunctionBody(NodeList.From<Statement>(
                new NonSpecialExpressionStatement(Call(runModifiers, domEvent)),
                new ReturnStatement(invoke)), strict: true),
            expression: false, async: false);
        var wrap = new ArrowFunctionExpression(NodeList.From<Node>(runHandler, runModifiers), listener,
            expression: true, async: false);
        return Array(String(name), Call(wrap, handler, arguments[1]));
    }
}
