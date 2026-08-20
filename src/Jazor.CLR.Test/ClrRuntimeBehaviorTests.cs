namespace Jazor.CLR.Test;

[TestClass]
public sealed class ClrRuntimeBehaviorTests
{
    public static IEnumerable<TestDataRow<string>> Scenarios
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
            foreach (var invocation in EnumerateInvocations(scenario.Arguments))
            {
                var invocationMapping = ClrRuntimeMappingCatalog.GetImport(invocation.Member);
                Assert.AreEqual(invocation.ModulePath, invocationMapping.ModulePath, scenario.Id);
                Assert.AreEqual(invocation.ExportName, invocationMapping.ExportName, scenario.Id);
            }

            Assert.AreNotEqual(scenario.ExpectedValue is null, scenario.ExpectedErrorContains is null, scenario.Id);
            if (scenario.ExpectedArguments is not null)
            {
                Assert.IsNotNull(scenario.ExpectedValue, scenario.Id);
                Assert.HasCount(scenario.Arguments.Count, scenario.ExpectedArguments, scenario.Id);
            }
        }
    }

    [TestMethod]
    public void ScenarioCatalog_CoversEveryRuntimeImportContract()
    {
        var coveredMembers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var scenario in ClrRuntimeScenarioCatalog.All)
        {
            coveredMembers.Add(scenario.Member);
            foreach (var invocation in EnumerateInvocations(scenario.Arguments))
                coveredMembers.Add(invocation.Member);
        }

        var uncoveredMembers = ClrRuntimeMappingCatalog.Imports
            .Where(static mapping => !mapping.IsExternalRuntime)
            .Select(static mapping => mapping.Member)
            .Where(member => !coveredMembers.Contains(member))
            .OrderBy(static member => member, StringComparer.Ordinal)
            .ToArray();

        Assert.IsEmpty(
            uncoveredMembers,
            "Every Op.Import mapping must be exercised by a Deno runtime scenario. Missing: " +
            string.Join(", ", uncoveredMembers));
    }

    private static IEnumerable<ClrRuntimeInvocationValue> EnumerateInvocations(
        IEnumerable<ClrRuntimeValue> values)
    {
        foreach (var value in values)
        {
            if (value.Invocation is not null)
            {
                yield return value.Invocation;
                foreach (var nested in EnumerateInvocations(value.Invocation.Arguments))
                    yield return nested;
            }

            if (value.Items is not null)
            {
                foreach (var nested in EnumerateInvocations(value.Items))
                    yield return nested;
            }

            if (value.Properties is not null)
            {
                foreach (var nested in EnumerateInvocations(value.Properties.Values))
                    yield return nested;
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(Scenarios))]
    public async Task RuntimeScenario_MatchesExpectedBehavior(string scenarioId)
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

        if (scenario.ExpectedArguments is not null)
        {
            Assert.IsNotNull(result.Arguments, scenarioId);
            Assert.HasCount(scenario.ExpectedArguments.Count, result.Arguments, scenarioId);
            for (var index = 0; index < scenario.ExpectedArguments.Count; index++)
            {
                AssertValue(
                    scenario.ExpectedArguments[index],
                    result.Arguments[index],
                    $"{scenarioId}.arguments[{index}]");
            }
        }
    }

    private static void AssertValue(ClrRuntimeValue expected, ClrRuntimeValue actual, string path)
    {
        Assert.AreEqual(expected.Kind, actual.Kind, path);
        if (expected.Kind == ClrRuntimeValueKind.Number)
        {
            Assert.IsNotNull(expected.Scalar, path);
            Assert.IsNotNull(actual.Scalar, path);
            var expectedNumber = double.Parse(
                expected.Scalar,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture);
            var actualNumber = double.Parse(
                actual.Scalar,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture);

            if (double.IsNaN(expectedNumber))
            {
                Assert.IsTrue(double.IsNaN(actualNumber), path);
            }
            else
            {
                Assert.AreEqual(
                    BitConverter.DoubleToInt64Bits(expectedNumber),
                    BitConverter.DoubleToInt64Bits(actualNumber),
                    path);
            }
        }
        else
        {
            Assert.AreEqual(expected.Scalar, actual.Scalar, path);
        }

        var expectedItems = expected.Items ?? [];
        var actualItems = actual.Items ?? [];
        Assert.HasCount(expectedItems.Count, actualItems, path);
        for (var index = 0; index < expectedItems.Count; index++)
            AssertValue(expectedItems[index], actualItems[index], $"{path}[{index}]");

        var expectedProperties = expected.Properties ?? new Dictionary<string, ClrRuntimeValue>(StringComparer.Ordinal);
        var actualProperties = actual.Properties ?? new Dictionary<string, ClrRuntimeValue>(StringComparer.Ordinal);
        Assert.HasCount(expectedProperties.Count, actualProperties, path);
        foreach (var (name, expectedValue) in expectedProperties)
        {
            Assert.IsTrue(actualProperties.TryGetValue(name, out var actualValue), $"{path}.{name}");
            AssertValue(expectedValue, actualValue, $"{path}.{name}");
        }
    }
}
