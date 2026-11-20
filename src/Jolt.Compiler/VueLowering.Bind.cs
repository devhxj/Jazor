using Acornima;
using Acornima.Ast;

namespace Jolt.Compiler;

internal sealed partial class VueLowering
{
    private static Expression LowerModelUpdate(IReadOnlyList<Expression> arguments)
    {
        if (arguments.Count == 1)
            return arguments[0];

        var assign = Id("$joltAssign");
        var after = Id("$joltAfter");
        var value = Id("$joltModelValue");
        // Capture each delegate once when constructing the listener. The callback sees
        // the committed value and Vue receives the same result (including a Promise).
        var listener = new ArrowFunctionExpression(NodeList.From<Node>(value),
            new FunctionBody(NodeList.From<Statement>(
                new NonSpecialExpressionStatement(Call(assign, value)),
                new ReturnStatement(Call(after))), strict: true),
            expression: false, async: false);
        var capture = new ArrowFunctionExpression(NodeList.From<Node>(assign, after), listener,
            expression: true, async: false);
        return Call(capture, arguments[0], arguments[1]);
    }

    private static ArrayExpression LowerNativeBinding(ModelBinding binding, IReadOnlyList<Expression> arguments)
    {
        var helper = Id(binding.Native switch
        {
            NativeModel.Checkbox => "$joltModelCheckbox",
            NativeModel.Radio => "$joltModelRadio",
            NativeModel.Select or NativeModel.SelectMultiple => "$joltModelSelect",
            _ => "$joltModelText",
        });
        // Vue's DOM directives sync checked/value and own the input/composition listeners.
        // Passing a separate value/checked prop could overwrite an in-progress composition.
        var directive = binding.Native is NativeModel.TextChange or NativeModel.NumberChange
            ? Array(helper, arguments[0], Id("undefined"), Object(Property("lazy", Id("true"))))
            : Array(helper, arguments[0]);
        var update = arguments[1];
        if (binding.Native is NativeModel.NumberChange or NativeModel.NumberInput)
        {
            // vModelText converts valid number input and forwards the raw string on parse failure.
            // Keep empty/invalid edits in the nullable C# numeric domain.
            var value = Id("$joltNumber");
            var normalized = new ConditionalExpression(Call(Member(Id("Number"), "isFinite"), value), value, new NullLiteral("null"));
            update = NormalizeModelUpdate(update, value, normalized);
        }
        else if (binding.Native == NativeModel.Select)
        {
            // Clearing the selection gives Vue an undefined payload, while HTML's value is empty.
            var value = Id("$joltSelection");
            var normalized = new LogicalExpression(Operator.NullishCoalescing, value, String(""));
            update = NormalizeModelUpdate(update, value, normalized);
        }
        if (arguments.Count == 3 && binding.Native is NativeModel.TextChange or NativeModel.TextInput or NativeModel.NumberChange or NativeModel.NumberInput)
            return Array(String(binding.Update), LowerSplitTextBinding(update, directive, arguments[2]), new NullLiteral("null"));
        return Array(String(binding.Update), update, directive);
    }

    private static CallExpression LowerSplitTextBinding(Expression update, ArrayExpression directive, Expression read)
    {
        var nativeDirective = Id("$joltNativeDirective");
        var assign = Id("$joltAssign");
        var getter = Id("$joltGetter");
        var element = Id("$joltInput");
        var value = Id("$joltValue");
        var result = Id("$joltResult");
        var current = Id("$joltCurrent");
        // Register the element before Vue's directive runs. Vue still owns event and
        // composition handling, and a synchronous setter can veto or normalize the edit.
        var register = new ArrowFunctionExpression(NodeList.From<Node>(value),
            new AssignmentExpression(Operator.Assignment, element, value), expression: true, async: false);
        var registration = Array(Object(Property("created", register), Property("beforeUpdate", register)));
        var domValue = Member(element, "value");
        var callback = new ArrowFunctionExpression(NodeList.From<Node>(value),
            new FunctionBody(NodeList.From<Statement>(
                new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From(new VariableDeclarator(result, Call(assign, value)))),
                new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From(new VariableDeclarator(current,
                    Call(Id("String"), new LogicalExpression(Operator.NullishCoalescing, Call(getter), String("")))))),
                new IfStatement(new NonLogicalBinaryExpression(Operator.StrictInequality, domValue, current),
                    new NonSpecialExpressionStatement(new AssignmentExpression(Operator.Assignment, domValue, current)), null),
                new ReturnStatement(result)), strict: true), expression: false, async: false);
        var capture = new ArrowFunctionExpression(NodeList.From<Node>(nativeDirective, assign, getter),
            new FunctionBody(NodeList.From<Statement>(
                new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(new VariableDeclarator(element, null))),
                new ReturnStatement(Array(callback, Array(registration, nativeDirective)))), strict: true),
            expression: false, async: false);
        return Call(capture, directive, update, read);
    }

    private static Expression NormalizeModelUpdate(Expression update, Identifier value, Expression normalized)
    {
        // Capture the authored callback when evaluating the binding, before its first update.
        var callback = Id("$joltModelUpdate");
        var listener = new ArrowFunctionExpression(NodeList.From<Node>(value), Call(callback, normalized), expression: true, async: false);
        var capture = new ArrowFunctionExpression(NodeList.From<Node>(callback), listener, expression: true, async: false);
        return Call(capture, update);
    }

    private Expression LowerElement(Intrinsic intrinsic, IReadOnlyList<Expression> arguments)
    {
        var tag = intrinsic.Component is null ? (Expression)String(intrinsic.Name!) : Id(_aliases[intrinsic.Component]);
        // Teleport consumes eager VNode children. Transition reads a lazy default slot.
        var children = intrinsic.Component is null || intrinsic.Component == VueBuiltIns.Teleport
            ? arguments[1] : Properties(arguments[1]);
        if (arguments[0] is ArrayExpression array)
        {
            // Entries are generated by the projection: the optional third field carries
            // directive metadata and the first two fields remain the VNode prop pair.
            for (int index = 0; index < array.Elements.Count; index++)
            {
                if (array.Elements[index] is not ArrayExpression { Elements.Count: 3 } entry)
                    continue;
                // Materialize entries so the binding getter runs once and in authored
                // attribute order. Reuse its captured directive after creating the VNode.
                var entries = Id("$joltEntries");
                bool splitText = entry.Elements[2] is NullLiteral;
                var captured = EntryAt(entries, index);
                var directives = splitText ? (Expression)EntryAt(EntryAt(captured, 1), 1) : Array(EntryAt(captured, 2));
                var vnode = Call(Id("$joltH"), tag, LowerElementProperties(array, entries), children);
                var apply = new ArrowFunctionExpression(NodeList.From<Node>(entries),
                    Call(Id("$joltWithDirectives"), vnode, directives), expression: true, async: false);
                return Call(apply, array);
            }
            return Call(Id("$joltH"), tag, LowerElementProperties(array), children);
        }
        return Call(Id("$joltH"), tag, Properties(arguments[0]), children);
    }

    private void AddModelImports(List<ImportSpecifier> imports)
    {
        var modes = projection.Intrinsics.Values.Select(intrinsic => intrinsic.Binding?.Native).OfType<NativeModel>().ToHashSet();
        if (modes.Count == 0)
            return;
        imports.Add(Import("withDirectives", "$joltWithDirectives"));
        if (modes.Contains(NativeModel.Checkbox))
            imports.Add(Import("vModelCheckbox", "$joltModelCheckbox"));
        if (modes.Contains(NativeModel.Radio))
            imports.Add(Import("vModelRadio", "$joltModelRadio"));
        if (modes.Contains(NativeModel.Select) || modes.Contains(NativeModel.SelectMultiple))
            imports.Add(Import("vModelSelect", "$joltModelSelect"));
        if (modes.Any(mode => mode is NativeModel.TextChange or NativeModel.TextInput or NativeModel.NumberChange or NativeModel.NumberInput))
            imports.Add(Import("vModelText", "$joltModelText"));
    }
}
