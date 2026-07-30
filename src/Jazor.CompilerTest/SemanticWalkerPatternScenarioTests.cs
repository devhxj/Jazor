using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerPatternScenarioTests
{
    private static readonly Lazy<ScenarioOperationSet> Operations = new(() =>
        ScenarioOperationSet.Create(
            "PatternScenarios",
            "PatternScenarios",
            PatternLoweringScenarioCatalog.All
                .Select(static (scenario, index) => new ScenarioOperationSource(
                    scenario.Id,
                    $"Scenario{index:D4}",
                    scenario.Source))
                .ToArray()));

    public static IEnumerable<TestDataRow<PatternLoweringScenario>> Cases
        => PatternLoweringScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<PatternLoweringScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var scenarios = PatternLoweringScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("pattern-lowering.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(scenarios.All(static scenario => scenario.ExpectedJavaScriptFragments.Count > 0));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_PatternScenario_ProducesDeterministicParsableJavaScript(PatternLoweringScenario scenario)
    {
        var block = Operations.Value.GetBlock(scenario.Id);
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, scenario.Id);
        Assert.AreEqual(first, second, scenario.Id);
        foreach (var fragment in scenario.ExpectedJavaScriptFragments)
            StringAssert.Contains(first, fragment, scenario.Id);

        _ = new Parser().ParseScript(first);
    }

}

public sealed record PatternLoweringScenario(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedJavaScriptFragments);

internal static class PatternLoweringScenarioCatalog
{
    private static readonly PatternType[] NumericTypes =
    [
        new("sbyte", "sbyte", "(sbyte)5", "2", "3", "9"),
        new("byte", "byte", "(byte)5", "2", "3", "9"),
        new("short", "short", "(short)5", "2", "3", "9"),
        new("ushort", "ushort", "(ushort)5", "2", "3", "9"),
        new("int", "int", "5", "2", "3", "9"),
        new("uint", "uint", "5u", "2", "3", "9"),
        new("long", "long", "5L", "BigInt(2)", "BigInt(3)", "BigInt(9)"),
        new("ulong", "ulong", "5UL", "BigInt(2)", "BigInt(3)", "BigInt(9)"),
        new("float", "float", "5f", "2", "3", "9"),
        new("double", "double", "5d", "2", "3", "9"),
        new("decimal", "decimal", "5m", "2", "3", "9")
    ];

    private static readonly PatternContext[] Contexts =
    [
        new("local", static expression => $"bool result = {expression};"),
        new("if", static expression => $"if ({expression}) {{ Consume(value); }}"),
        new("lambda", static expression => $"Func<bool> matches = () => {expression};"),
        new("argument", static expression => $"Consume({expression});"),
        new("array", static expression => $"var results = new[] {{ {expression} }};"),
        new("anonymous", static expression => $"var result = new {{ Match = {expression} }};"),
        new("conditional", static expression => $"var result = {expression} ? \"match\" : \"other\";"),
        new("guard", static expression => $"if ({expression} && true) {{ Consume(value); }}"),
        new("nested-if", static expression => $"if ({expression}) {{ if ({expression}) {{ Consume(value); }} }}")
    ];

    public static IReadOnlyList<PatternLoweringScenario> All { get; } = Build();

    private static IReadOnlyList<PatternLoweringScenario> Build()
    {
        var scenarios = new List<PatternLoweringScenario>();
        AddNumericScenarios(scenarios);
        AddSimpleScenarios(
            scenarios,
            "boolean",
            "bool",
            "true",
            [
                new("true", "value is true", ["value === true"]),
                new("false", "value is false", ["value === false"]),
                new("not-true", "value is not true", ["!(value === true)"]),
                new("or", "value is true or false", ["value === true || value === false"]),
                new("and", "value is not false and true", ["!(value === false) && value === true"])
            ]);
        AddSimpleScenarios(
            scenarios,
            "string",
            "string",
            "\"ready\"",
            [
                new("equal", "value is \"ready\"", ["value === \"ready\""]),
                new("not-equal", "value is not \"ready\"", ["!(value === \"ready\")"]),
                new("or", "value is \"ready\" or \"queued\"", ["value === \"ready\" || value === \"queued\""]),
                new("not-or", "value is not (\"ready\" or \"queued\")", ["!(value === \"ready\" || value === \"queued\")"]),
                new("property", "value is { Length: > 3 }", ["value != null", "value.length > 3"])
            ]);
        AddSimpleScenarios(
            scenarios,
            "char",
            "char",
            "'R'",
            [
                new("equal", "value is 'R'", ["value === \"R\""]),
                new("not-equal", "value is not 'R'", ["!(value === \"R\")"]),
                new("or", "value is 'R' or 'Q'", ["value === \"R\" || value === \"Q\""]),
                new("not-or", "value is not ('R' or 'Q')", ["!(value === \"R\" || value === \"Q\")"])
            ]);
        AddSimpleScenarios(
            scenarios,
            "nullable-int",
            "int?",
            "5",
            [
                new("greater-than", "value is > 2", ["value > 2"]),
                new("constant", "value is 2", ["value === 2"]),
                new("not-constant", "value is not 2", ["!(value === 2)"]),
                new("range", "value is > 2 and < 9", ["value > 2 && value < 9"])
            ]);
        AddSimpleScenarios(
            scenarios,
            "nullable-string",
            "string?",
            "\"ready\"",
            [
                new("null", "value is null", ["value == null"]),
                new("not-null", "value is not null", ["!(value == null)"]),
                new("equal", "value is \"ready\"", ["value === \"ready\""]),
                new("or", "value is \"ready\" or \"queued\"", ["value === \"ready\" || value === \"queued\""]),
                new("property", "value is { Length: > 3 }", ["value != null", "value.length > 3"])
            ]);

        return scenarios;
    }

    private static void AddNumericScenarios(List<PatternLoweringScenario> scenarios)
    {
        foreach (var type in NumericTypes)
        {
            AddSimpleScenarios(
                scenarios,
                $"numeric-{type.Id}",
                type.DeclarationType,
                type.Value,
                [
                    new("greater-than", "value is > 2", [$"value > {type.Two}"]),
                    new("greater-than-or-equal", "value is >= 2", [$"value >= {type.Two}"]),
                    new("less-than", "value is < 9", [$"value < {type.Nine}"]),
                    new("less-than-or-equal", "value is <= 9", [$"value <= {type.Nine}"]),
                    new("constant", "value is 2", [$"value === {type.Two}"]),
                    new("not-constant", "value is not 2", [$"!(value === {type.Two})"]),
                    new("or", "value is 2 or 3", [$"value === {type.Two} || value === {type.Three}"]),
                    new("range", "value is > 2 and < 9", [$"value > {type.Two} && value < {type.Nine}"])
                ]);
        }
    }

    private static void AddSimpleScenarios(
        List<PatternLoweringScenario> scenarios,
        string family,
        string declarationType,
        string value,
        IReadOnlyList<PatternDefinition> patterns)
    {
        foreach (var pattern in patterns)
        {
            foreach (var context in Contexts)
            {
                var id = $"pattern-lowering.{family}.{pattern.Id}.{context.Id}";
                var body = $$"""
                                {{declarationType}} value = {{value}};
                                {{context.Build(pattern.Expression)}}
                    """;
                scenarios.Add(new PatternLoweringScenario(
                    id,
                    $"family={family};pattern={pattern.Id};context={context.Id}",
                    body,
                    pattern.ExpectedJavaScriptFragments));
            }
        }
    }

    private sealed record PatternType(
        string Id,
        string DeclarationType,
        string Value,
        string Two,
        string Three,
        string Nine);

    private sealed record PatternDefinition(
        string Id,
        string Expression,
        IReadOnlyList<string> ExpectedJavaScriptFragments);

    private sealed record PatternContext(string Id, Func<string, string> Build);
}
