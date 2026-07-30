using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerArrayCreationScenarioTests
{
    private static readonly Lazy<ScenarioOperationSet> Operations = new(() =>
        ScenarioOperationSet.Create(
            "ArrayCreationScenarios",
            "ArrayCreationScenarios",
            ArrayCreationScenarioCatalog.All
                .Select(static (scenario, index) => new ScenarioOperationSource(
                    scenario.Id,
                    $"Scenario{index:D3}",
                    scenario.Source))
                .ToArray()));

    public static IEnumerable<TestDataRow<ArrayCreationScenario>> Cases
        => ArrayCreationScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<ArrayCreationScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var scenarios = ArrayCreationScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario => scenario.Id.StartsWith("array-creation.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => scenario.ExpectedJavaScriptFragments.Count > 0));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_ZeroLengthArrayCreation_PreservesIntegralConstantWidth(ArrayCreationScenario scenario)
    {
        var block = Operations.Value.GetBlock(scenario.Id);
        Assert.HasCount(1, EnumerateOperations(block).OfType<IArrayCreationOperation>(), scenario.Id);

        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, scenario.Id);
        Assert.AreEqual(first, second, scenario.Id);
        foreach (var fragment in scenario.ExpectedJavaScriptFragments)
            StringAssert.Contains(first, fragment, scenario.Id);

        _ = new Parser().ParseScript(first);
    }

    private static IEnumerable<IOperation> EnumerateOperations(IOperation root)
    {
        yield return root;
        foreach (var child in root.ChildOperations)
        {
            foreach (var descendant in EnumerateOperations(child))
                yield return descendant;
        }
    }
}

public sealed record ArrayCreationScenario(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedJavaScriptFragments);

internal static class ArrayCreationScenarioCatalog
{
    public static IReadOnlyList<ArrayCreationScenario> All { get; } =
    [
        Case("zero.int", "rank=1;size=constant;constant-type=int;size=zero", "var value = new int[0];", "let value = []"),
        Case("zero.uint", "rank=1;size=constant;constant-type=uint;size=zero", "var value = new int[0u];", "let value = []"),
        Case("zero.long", "rank=1;size=constant;constant-type=long;size=zero", "var value = new int[0L];", "let value = []"),
        Case("zero.ulong", "rank=1;size=constant;constant-type=ulong;size=zero", "var value = new int[0UL];", "let value = []")
    ];

    private static ArrayCreationScenario Case(
        string id,
        string dimension,
        string source,
        params string[] expectedJavaScriptFragments)
        => new($"array-creation.{id}", dimension, source, expectedJavaScriptFragments);
}
