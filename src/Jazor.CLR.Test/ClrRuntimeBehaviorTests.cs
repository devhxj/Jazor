namespace Jazor.CLR.Test;

[TestClass]
public sealed class ClrRuntimeBehaviorTests
{
    public static IEnumerable<TestDataRow<string>> GuidScenarios
        => ClrRuntimeScenarioCatalog.All.Select(static scenario => new TestDataRow<string>(scenario.Id)
        {
            DisplayName = scenario.Id
        });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsAndResolvedImportContracts()
    {
        var scenarios = ClrRuntimeScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        foreach (var scenario in scenarios)
        {
            var mapping = ClrRuntimeMappingCatalog.GetImport(scenario.Member);
            Assert.AreEqual(scenario.ModulePath, mapping.ModulePath, scenario.Id);
            Assert.AreNotEqual(scenario.ExpectedValue is null, scenario.ExpectedErrorContains is null, scenario.Id);
        }
    }

    [TestMethod]
    [DynamicData(nameof(GuidScenarios))]
    public async Task GuidRuntimeScenario_MatchesExpectedBehavior(string scenarioId)
    {
        var scenario = ClrRuntimeScenarioCatalog.Get(scenarioId);
        var results = await ClrRuntimeTestHost.RunAsync();
        Assert.IsTrue(results.TryGetValue(scenarioId, out var result), scenarioId);

        if (scenario.ExpectedErrorContains is not null)
        {
            Assert.IsFalse(result.Succeeded, scenarioId);
            Assert.IsNotNull(result.Error, scenarioId);
            StringAssert.Contains(result.Error, scenario.ExpectedErrorContains, scenarioId);
            return;
        }

        Assert.IsTrue(result.Succeeded, result.Error ?? scenarioId);
        Assert.IsNotNull(scenario.ExpectedValue, scenarioId);
        Assert.IsNotNull(result.Value, scenarioId);
        AssertValue(scenario.ExpectedValue, result.Value, scenarioId);
    }

    private static void AssertValue(ClrRuntimeValue expected, ClrRuntimeValue actual, string path)
    {
        Assert.AreEqual(expected.Kind, actual.Kind, path);
        Assert.AreEqual(expected.Scalar, actual.Scalar, path);

        var expectedItems = expected.Items ?? [];
        var actualItems = actual.Items ?? [];
        Assert.HasCount(expectedItems.Count, actualItems, path);
        for (var index = 0; index < expectedItems.Count; index++)
            AssertValue(expectedItems[index], actualItems[index], $"{path}[{index}]");
    }
}
