using Acornima;
using Acornima.Ast;

namespace Jolt.Compiler;

internal sealed partial class VueLowering
{
    private Expression ReadLocalProp(Expression instance, Expression props, string property, Expression read,
        Expression fallback)
    {
        var model = contract.LocalModels.FirstOrDefault(candidate => candidate.Callback == property);
        var alias = contract.VuePropertyAliases.GetValueOrDefault(property);
        if (model is not null || alias is not null)
        {
            var vnodeProps = Call(Member(instance, "$joltReadVNodeProps"));
            Expression HasRawProp(string name)
            {
                var value = new ChainExpression(new MemberExpression(vnodeProps, String(name), computed: true, optional: true));
                return new NonLogicalBinaryExpression(Operator.StrictInequality, value, Id("undefined"));
            }

            Expression HasVueProp(string name, string prefix = "", string suffix = "")
            {
                var raw = HasRawProp(prefix + name + suffix);
                var kebab = string.Concat(name.Select(character => char.IsUpper(character)
                    ? "-" + char.ToLowerInvariant(character) : character.ToString()));
                return name == kebab ? raw : new LogicalExpression(Operator.LogicalOr, raw, HasRawProp(prefix + kebab + suffix));
            }

            if (model is not null)
            {
                var hasListener = new LogicalExpression(Operator.LogicalOr,
                    HasVueProp(model.VueProperty, "onUpdate:"), HasVueProp(model.VueProperty, "onUpdate:", "Once"));
                var value = Id("$joltModelValue");
                var emit = new ArrowFunctionExpression(NodeList.From<Node>(value),
                    Call(Member(instance, "$joltEmit"), String(model.Event), value), expression: true, async: false);
                // Keep the legacy prop and its initializer observable. A Vue listener
                // provides the adapter only when the explicit callback was omitted.
                fallback = new ConditionalExpression(hasListener, emit, fallback);
            }
            if (alias is not null)
            {
                var canonical = new ChainExpression(new MemberExpression(props, String(alias), computed: true, optional: true));
                // Vue can retain the prior PascalCase prop across alias changes. The
                // current VNode chooses the spelling; the reactive prop supplies its value.
                return new ConditionalExpression(HasRawProp(property), read,
                    new ConditionalExpression(HasVueProp(alias), canonical, fallback));
            }
        }
        return ReadProvidedProp(read, fallback);
    }

    private static ConditionalExpression ReadProvidedProp(Expression read, Expression fallback) => new(
        new NonLogicalBinaryExpression(Operator.StrictInequality, read, Id("undefined")), read, fallback);
}
