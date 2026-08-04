using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerMixedNumericBinaryScenarioTests
{
    private static readonly Lazy<ScenarioOperationSet> Operations = new(() =>
        ScenarioOperationSet.Create(
            "MixedNumericBinaryOperatorScenarios",
            "MixedNumericBinaryOperatorScenarios",
            MixedNumericBinaryOperatorScenarioCatalog.All
                .Select(static (scenario, index) => new ScenarioOperationSource(
                    scenario.Id,
                    $"Scenario{index:D4}",
                    scenario.Source))
                .ToArray()));

    public static IEnumerable<TestDataRow<MixedNumericBinaryOperatorScenario>> Cases
        => MixedNumericBinaryOperatorScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<MixedNumericBinaryOperatorScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var scenarios = MixedNumericBinaryOperatorScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("mixed-numeric-binary.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.ExpectedJavaScriptExpression)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_MixedNumericBinaryOperatorScenario_ProducesDeterministicParsableJavaScript(
        MixedNumericBinaryOperatorScenario scenario)
    {
        var block = Operations.Value.GetBlock(scenario.Id);
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, scenario.Id);
        Assert.AreEqual(first, second, scenario.Id);
        StringAssert.Contains(first, scenario.ExpectedJavaScriptExpression, scenario.Id);
        _ = new Parser().ParseScript(first);
    }
}

public sealed record MixedNumericBinaryOperatorScenario(
    string Id,
    string Dimension,
    string Source,
    string ExpectedJavaScriptExpression);

internal static class MixedNumericBinaryOperatorScenarioCatalog
{
    private static readonly MixedNumericPair[] Pairs =
    [
        NumberPair("sbyte-short", "sbyte", "(sbyte)5", "short", "(short)2", "int"),
        NumberPair("byte-short", "byte", "(byte)5", "short", "(short)2", "int"),
        BigIntPair("short-uint", "short", "(short)5", "uint", "2u", "long", "BigInt(left)", "BigInt(right)"),
        BigIntPair("int-uint", "int", "5", "uint", "2u", "long", "BigInt(left)", "BigInt(right)"),
        BigIntPair("int-long", "int", "5", "long", "2L", "long", "BigInt(left)", "right"),
        BigIntPair("uint-long", "uint", "5u", "long", "2L", "long", "BigInt(left)", "right"),
        BigIntPair("uint-ulong", "uint", "5u", "ulong", "2UL", "ulong", "BigInt(left)", "right"),
        NumberPair("float-int", "float", "5f", "int", "2", "float"),
        NumberPair("float-uint", "float", "5f", "uint", "2u", "float"),
        NumberPair("double-int", "double", "5d", "int", "2", "double"),
        NumberPair("double-long", "double", "5d", "long", "2L", "double", "left", "Number(right)"),
        NumberPair("decimal-int", "decimal", "5m", "int", "2", "decimal"),
        NumberPair("decimal-long", "decimal", "5m", "long", "2L", "decimal", "left", "Number(right)"),
        NumberPair("decimal-uint", "decimal", "5m", "uint", "2u", "decimal"),
        BigIntPair(
            "int128-long",
            "Int128",
            "5",
            "long",
            "2L",
            "Int128",
            divideAlias: "_6357de67d5760485",
            remainderAlias: "_6521eedba51d7990"),
        BigIntPair(
            "uint128-ulong",
            "UInt128",
            "5",
            "ulong",
            "2UL",
            "UInt128",
            divideAlias: "_30e28339559d8888",
            remainderAlias: "_4541585272909795"),
        NumberPair("long-double", "long", "5L", "double", "2d", "double", "Number(left)", "right")
    ];

    private static readonly BinaryOperator[] ArithmeticOperators =
    [
        new("add", "+", "+"),
        new("subtract", "-", "-"),
        new("multiply", "*", "*"),
        new("divide", "/", "/"),
        new("remainder", "%", "%")
    ];

    private static readonly BinaryOperator[] ComparisonOperators =
    [
        new("equal", "==", "===", "bool"),
        new("not-equal", "!=", "!==", "bool"),
        new("less-than", "<", "<", "bool"),
        new("less-than-or-equal", "<=", "<=", "bool"),
        new("greater-than", ">", ">", "bool"),
        new("greater-than-or-equal", ">=", ">=", "bool")
    ];

    private static readonly BinaryContext[] Contexts =
    [
        new("local", static (expression, _) => $"var result = {expression};"),
        new("expression-lambda", static (expression, resultType) => $"Func<{resultType}> calculate = () => {expression};"),
        new("block-lambda", static (expression, resultType) => $"Func<{resultType}> calculate = () => {{ return {expression}; }};"),
        new("argument", static (expression, _) => $"Consume({expression});"),
        new("array-element", static (expression, _) => $"var results = new[] {{ {expression} }};"),
        new("anonymous-property", static (expression, _) => $"var result = new {{ Value = {expression} }};"),
        new("tuple-element", static (expression, _) => $"var result = (Value: {expression}, Marker: 1);"),
        new("conditional-arm", static (expression, _) => $"var result = true ? {expression} : {expression};"),
        new("switch-arm", static (expression, _) => $"var result = 0 switch {{ 0 => {expression}, _ => {expression} }};")
    ];

    public static IReadOnlyList<MixedNumericBinaryOperatorScenario> All { get; } = Build();

    private static IReadOnlyList<MixedNumericBinaryOperatorScenario> Build()
    {
        var scenarios = new List<MixedNumericBinaryOperatorScenario>();
        foreach (var pair in Pairs)
        {
            AddOperators(scenarios, pair, "arithmetic", ArithmeticOperators);
            AddOperators(scenarios, pair, "comparison", ComparisonOperators);
        }

        return scenarios;
    }

    private static void AddOperators(
        List<MixedNumericBinaryOperatorScenario> scenarios,
        MixedNumericPair pair,
        string family,
        IReadOnlyList<BinaryOperator> operators)
    {
        foreach (var binaryOperator in operators)
        {
            var resultType = binaryOperator.ResultType ?? pair.ResultType;
            var expression = $"left {binaryOperator.CSharpToken} right";
            var expected = GetExpectedJavaScriptExpression(pair, binaryOperator);
            foreach (var context in Contexts)
            {
                var id = $"mixed-numeric-binary.{family}.{pair.Id}.{binaryOperator.Id}.{context.Id}";
                var body = $$"""
                                {{pair.LeftType}} left = {{pair.LeftValue}};
                                {{pair.RightType}} right = {{pair.RightValue}};
                                {{context.BuildBody(expression, resultType)}}
                    """;
                scenarios.Add(new MixedNumericBinaryOperatorScenario(
                    id,
                    $"family={family};left={pair.LeftType};right={pair.RightType};result={resultType};operator={binaryOperator.Id};context={context.Id}",
                    body,
                    expected));
            }
        }
    }

    private static string GetExpectedJavaScriptExpression(MixedNumericPair pair, BinaryOperator binaryOperator)
    {
        var alias = binaryOperator.Id switch
        {
            "divide" => pair.DivideAlias,
            "remainder" => pair.RemainderAlias,
            _ => null
        };
        return alias is null
            ? $"{pair.JavaScriptLeft} {binaryOperator.JavaScriptToken} {pair.JavaScriptRight}"
            : $"{alias}({pair.JavaScriptLeft}, {pair.JavaScriptRight})";
    }

    private static MixedNumericPair NumberPair(
        string id,
        string leftType,
        string leftValue,
        string rightType,
        string rightValue,
        string resultType,
        string javaScriptLeft = "left",
        string javaScriptRight = "right",
        string? divideAlias = null,
        string? remainderAlias = null)
        => new(id, leftType, leftValue, rightType, rightValue, resultType, javaScriptLeft, javaScriptRight, divideAlias, remainderAlias);

    private static MixedNumericPair BigIntPair(
        string id,
        string leftType,
        string leftValue,
        string rightType,
        string rightValue,
        string resultType,
        string javaScriptLeft = "left",
        string javaScriptRight = "right",
        string? divideAlias = null,
        string? remainderAlias = null)
        => new(id, leftType, leftValue, rightType, rightValue, resultType, javaScriptLeft, javaScriptRight, divideAlias, remainderAlias);

    private sealed record MixedNumericPair(
        string Id,
        string LeftType,
        string LeftValue,
        string RightType,
        string RightValue,
        string ResultType,
        string JavaScriptLeft,
        string JavaScriptRight,
        string? DivideAlias,
        string? RemainderAlias);

    private sealed record BinaryOperator(
        string Id,
        string CSharpToken,
        string JavaScriptToken,
        string? ResultType = null);

    private sealed record BinaryContext(
        string Id,
        Func<string, string, string> BuildBody);
}
