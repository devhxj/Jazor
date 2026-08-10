using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerNumericConversionScenarioTests
{
    private static readonly Lazy<ScenarioOperationSet> Operations = new(() =>
        ScenarioOperationSet.Create(
            "NumericConversionScenarios",
            "NumericConversionScenarios",
            NumericConversionScenarioCatalog.All
                .Select(static (scenario, index) => new ScenarioOperationSource(
                    scenario.Id,
                    $"Scenario{index:D4}",
                    scenario.Source))
                .ToArray()));

    public static IEnumerable<TestDataRow<NumericConversionScenario>> Cases
        => NumericConversionScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<NumericConversionScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var scenarios = NumericConversionScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("numeric-conversion.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.ExpectedJavaScriptFragment)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_NumericConversionScenario_ProducesDeterministicParsableJavaScript(NumericConversionScenario scenario)
    {
        var block = Operations.Value.GetBlock(scenario.Id);
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, scenario.Id);
        Assert.AreEqual(first, second, scenario.Id);
        StringAssert.Contains(first, scenario.ExpectedJavaScriptFragment, scenario.Id);
        _ = new Parser().ParseScript(first);
    }

}

public sealed record NumericConversionScenario(
    string Id,
    string Dimension,
    string Source,
    string ExpectedJavaScriptFragment);

internal static class NumericConversionScenarioCatalog
{
    private static readonly ConversionType[] Types =
    [
        new("sbyte", "sbyte", "(sbyte)65", ConversionCarrier.Number),
        new("byte", "byte", "(byte)65", ConversionCarrier.Number),
        new("short", "short", "(short)65", ConversionCarrier.Number),
        new("ushort", "ushort", "(ushort)65", ConversionCarrier.Number),
        new("int", "int", "65", ConversionCarrier.Number),
        new("uint", "uint", "65u", ConversionCarrier.Number),
        new("long", "long", "65L", ConversionCarrier.BigInt),
        new("ulong", "ulong", "65UL", ConversionCarrier.BigInt),
        new("float", "float", "65f", ConversionCarrier.Number),
        new("double", "double", "65d", ConversionCarrier.Number),
        new("decimal", "decimal", "65m", ConversionCarrier.Number),
        new("char", "char", "'A'", ConversionCarrier.Char)
    ];

    private static readonly ConversionContext[] Contexts =
    [
        new("local", static (expression, targetType) =>
            ($"{targetType} result = {expression};", $"let result = {Lower(expression)};")),
        new("expression-lambda", static (expression, targetType) =>
            ($"Func<{targetType}> convert = () => {expression};", $"return {Lower(expression)};")),
        new("block-lambda", static (expression, targetType) =>
            ($"Func<{targetType}> convert = () => {{ return {expression}; }};", $"return {Lower(expression)};")),
        new("argument", static (expression, _) =>
            ($"Consume({expression});", $"NumericConversionScenarios.Consume({Lower(expression)})")),
        new("array-element", static (expression, targetType) =>
            ($"{targetType}[] results = [{expression}];", $"[{Lower(expression)}]")),
        new("anonymous-property", static (expression, _) =>
            ($"var result = new {{ Value = {expression} }};", $"Value: {Lower(expression)}")),
        new("conditional-arm", static (expression, _) =>
            ($"var result = true ? {expression} : {expression};", $"true ? {Lower(expression)} : {Lower(expression)}"))
    ];

    public static IReadOnlyList<NumericConversionScenario> All { get; } = Build();

    private static IReadOnlyList<NumericConversionScenario> Build()
    {
        var scenarios = new List<NumericConversionScenario>();
        foreach (var sourceType in Types)
        {
            foreach (var targetType in Types)
            {
                var loweredExpression = GetLoweredExpression(sourceType.Carrier, targetType.Carrier);
                var authoredExpression = $"({targetType.DeclarationType})source";
                foreach (var context in Contexts)
                {
                    var contextExpression = authoredExpression.Replace("source", loweredExpression, StringComparison.Ordinal);
                    var (contextBody, expected) = context.Build(contextExpression, targetType.DeclarationType);
                    var id = $"numeric-conversion.{sourceType.Id}.to-{targetType.Id}.{context.Id}";
                    var body = $$"""
                                    {{sourceType.DeclarationType}} source = {{sourceType.Value}};
                                    {{contextBody.Replace(contextExpression, authoredExpression, StringComparison.Ordinal)}}
                        """;
                    scenarios.Add(new NumericConversionScenario(
                        id,
                        $"source={sourceType.Id};target={targetType.Id};context={context.Id}",
                        body,
                        expected));
                }
            }
        }

        return scenarios;
    }

    private static string GetLoweredExpression(ConversionCarrier source, ConversionCarrier target)
    {
        if (source == ConversionCarrier.Char)
        {
            return target switch
            {
                ConversionCarrier.Number => "source.charCodeAt(0)",
                ConversionCarrier.BigInt => "BigInt(source.charCodeAt(0))",
                _ => "source"
            };
        }

        if (target == ConversionCarrier.Char)
        {
            return source == ConversionCarrier.BigInt
                ? "String.fromCharCode(Number(source))"
                : "String.fromCharCode(source)";
        }

        return (source, target) switch
        {
            (ConversionCarrier.Number, ConversionCarrier.BigInt) => "BigInt(source)",
            (ConversionCarrier.BigInt, ConversionCarrier.Number) => "Number(source)",
            _ => "source"
        };
    }

    private static string Lower(string expression)
    {
        const string authoredPrefix = "(";
        var closingParenthesis = expression.IndexOf(')');
        return expression.StartsWith(authoredPrefix, StringComparison.Ordinal) && closingParenthesis >= 0
            ? expression[(closingParenthesis + 1)..]
            : expression;
    }

    private sealed record ConversionType(
        string Id,
        string DeclarationType,
        string Value,
        ConversionCarrier Carrier);

    private sealed record ConversionContext(
        string Id,
        Func<string, string, (string Body, string Expected)> Build);

    private enum ConversionCarrier
    {
        Number,
        BigInt,
        Char
    }
}
