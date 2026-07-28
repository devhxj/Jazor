using Acornima;
using Acornima.Ast;
using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueSlotAstFactoryMatrixTests
{
    public static IEnumerable<TestDataRow<SlotExpressionCase>> SlotExpressions
        => SlotExpressionCase.All.Select(static testCase => new TestDataRow<SlotExpressionCase>(testCase)
        {
            DisplayName = "NormalizeContent_" + testCase.Id
        });

    [TestMethod]
    [DynamicData(nameof(SlotExpressions))]
    public void NormalizeContent_PreservesExpressionInsideArrayNormalization(SlotExpressionCase testCase)
    {
        var expression = new Parser().ParseExpression(testCase.Source, sourceFile: null, strict: true);

        var normalized = VueSlotAstFactory.NormalizeContent(expression);

        Assert.IsInstanceOfType<CallExpression>(normalized);
        var call = (CallExpression)normalized;
        Assert.IsFalse(call.Optional);
        Assert.HasCount(1, call.Arguments);

        Assert.IsInstanceOfType<MemberExpression>(call.Callee);
        var member = (MemberExpression)call.Callee;
        Assert.IsFalse(member.Computed);
        Assert.IsFalse(member.Optional);
        Assert.IsInstanceOfType<ArrayExpression>(member.Object);
        Assert.IsEmpty(((ArrayExpression)member.Object).Elements);
        Assert.IsInstanceOfType<Identifier>(member.Property);
        Assert.AreEqual("concat", ((Identifier)member.Property).Name);

        Assert.IsInstanceOfType<LogicalExpression>(call.Arguments[0]);
        var fallback = (LogicalExpression)call.Arguments[0];
        Assert.AreEqual(Operator.NullishCoalescing, fallback.Operator);
        Assert.AreSame(expression, fallback.Left);
        Assert.AreEqual(testCase.ExpectedNodeType, fallback.Left.Type);
        Assert.IsInstanceOfType<ArrayExpression>(fallback.Right);
        Assert.IsEmpty(((ArrayExpression)fallback.Right).Elements);
    }
}

public sealed record SlotExpressionCase(string Id, string Source, NodeType ExpectedNodeType)
{
    public static IReadOnlyList<SlotExpressionCase> All { get; } =
    [
        new("identifier", "value", NodeType.Identifier),
        new("null", "null", NodeType.Literal),
        new("undefined", "undefined", NodeType.Identifier),
        new("string", "\"text\"", NodeType.Literal),
        new("number", "42", NodeType.Literal),
        new("boolean", "true", NodeType.Literal),
        new("empty_array", "[]", NodeType.ArrayExpression),
        new("single_array", "[first]", NodeType.ArrayExpression),
        new("multi_array", "[first, second]", NodeType.ArrayExpression),
        new("spread_array", "[...items]", NodeType.ArrayExpression),
        new("call", "h(\"div\")", NodeType.CallExpression),
        new("optional_call", "slots.default?.()", NodeType.ChainExpression),
        new("member", "props.items", NodeType.MemberExpression),
        new("computed_member", "value[index]", NodeType.MemberExpression),
        new("conditional", "condition ? first : second", NodeType.ConditionalExpression),
        new("logical_and", "left && right", NodeType.LogicalExpression),
        new("nullish", "left ?? right", NodeType.LogicalExpression),
        new("binary", "value + suffix", NodeType.BinaryExpression),
        new("comparison", "value === null", NodeType.BinaryExpression),
        new("unary", "!visible", NodeType.UnaryExpression),
        new("typeof", "typeof value", NodeType.UnaryExpression),
        new("update", "++index", NodeType.UpdateExpression),
        new("sequence", "(first, second)", NodeType.SequenceExpression),
        new("object", "({ default: slot })", NodeType.ObjectExpression),
        new("spread_object", "({ ...props })", NodeType.ObjectExpression),
        new("arrow", "() => child", NodeType.ArrowFunctionExpression),
        new("arrow_parameter", "item => [item]", NodeType.ArrowFunctionExpression),
        new("async_arrow", "async () => child", NodeType.ArrowFunctionExpression),
        new("function", "function () { return child; }", NodeType.FunctionExpression),
        new("generator", "function* () { yield child; }", NodeType.FunctionExpression),
        new("new", "new Widget()", NodeType.NewExpression),
        new("template", "`hello ${name}`", NodeType.TemplateLiteral)
    ];
}
