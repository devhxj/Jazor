using Acornima;
using Acornima.Ast;

namespace Jolt.Compiler;

internal sealed partial class VueLowering
{
    private Expression LowerReference(string index, IReadOnlyList<Expression> arguments)
    {
        // Vue clears the previous object ref when a VNode is reused with a new
        // target. Keep each site's identity stable and bypass reactive ref unwrapping
        // for the private storage. The accessor itself still writes reactive state.
        var state = Call(Id("$joltToRaw"), arguments[0]);
        var storage = Member(state, "$joltRef" + index);
        var target = projection.ReferenceTargets[int.Parse(index, System.Globalization.CultureInfo.InvariantCulture)];
        var owner = Member(state, "$joltRefOwner$" + target);
        var value = Id("$joltValue");
        var site = String(index);
        var nil = new NullLiteral("null");
        var set = new ArrowFunctionExpression(NodeList.From<Node>(value),
            new FunctionBody(NodeList.From<Statement>(
                // A previous site's cleanup can arrive after another site has
                // captured a value for this same field. Only the owner may clear it.
                new IfStatement(new NonLogicalBinaryExpression(Operator.StrictInequality, value, nil),
                    Assign(owner, site),
                    new IfStatement(new NonLogicalBinaryExpression(Operator.StrictInequality, owner, site),
                        new ReturnStatement(null), Assign(owner, nil))),
                new NonSpecialExpressionStatement(Call(arguments[2], value))), strict: true),
            expression: false, async: false);
        var accessors = Object(Property("get", arguments[1]), Property("set", set));
        var factory = new ArrowFunctionExpression(NodeList.Empty<Node>(), accessors, expression: true, async: false);
        return Array(String("ref"),
            new AssignmentExpression(Operator.NullishCoalescingAssignment, storage, Call(Id("$joltCustomRef"), factory)));
    }
}
