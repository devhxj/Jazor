using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SenseArgumentModuleNameScenarioTests
{
    public static IEnumerable<TestDataRow<SenseArgumentModuleNameScenario>> Cases
        => SenseArgumentModuleNameScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<SenseArgumentModuleNameScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsKindsAndInputs()
    {
        var scenarios = SenseArgumentModuleNameScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            scenarios.Count,
            scenarios.Select(static scenario => scenario.InputIdentity).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            Enum.GetValues<SenseArgumentImportedNameNodeKind>().Length,
            scenarios.Select(static scenario => scenario.ImportedNameNodeKind).Distinct());
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("sense-argument-module-name.", StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(scenario.Dimension) &&
            !string.IsNullOrWhiteSpace(scenario.ModulePath) &&
            !string.IsNullOrWhiteSpace(scenario.ImportedName)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void BindImportSpecifier_ModuleExportNameUsesValidStableLocalBinding(
        SenseArgumentModuleNameScenario scenario)
    {
        var bindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var localBindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var argument = new SenseArgument(UseImportAliases: true)
            .WithImportContext(
                bindings,
                localBindings,
                new HashSet<string>(StringComparer.Ordinal),
                currentModuleImportPath: null,
                new HashSet<string>(StringComparer.Ordinal));

        var first = argument.BindImportSpecifier(scenario.ModulePath, scenario.ImportedName);
        var second = argument.BindImportSpecifier(scenario.ModulePath, scenario.ImportedName);

        var expectedLocalName = scenario.RequiresAlias
            ? $"i${Format.HashName($"{scenario.ModulePath}\0{scenario.ImportedName}").TrimStart('_')}"
            : scenario.ImportedName;
        Assert.AreEqual(expectedLocalName, first.Name, scenario.Id);
        Assert.AreEqual(first.Name, second.Name, scenario.Id);
        Assert.AreEqual(expectedLocalName, bindings[$"{scenario.ModulePath}\0{scenario.ImportedName}"], scenario.Id);
        Assert.AreEqual($"{scenario.ModulePath}\0{scenario.ImportedName}", localBindings[expectedLocalName], scenario.Id);

        var group = argument.FlushImportSpecifiers().Single();
        Assert.AreEqual(scenario.ModulePath, group.Key, scenario.Id);
        var specifier = group.Value.Single();
        switch (scenario.ImportedNameNodeKind)
        {
            case SenseArgumentImportedNameNodeKind.Identifier:
                Assert.IsInstanceOfType<ImportSpecifier>(specifier, scenario.Id);
                var identifierSpecifier = (ImportSpecifier)specifier;
                Assert.IsInstanceOfType<Identifier>(identifierSpecifier.Imported, scenario.Id);
                Assert.AreEqual(
                    scenario.ImportedName,
                    ((Identifier)identifierSpecifier.Imported).Name,
                    scenario.Id);
                Assert.AreEqual(expectedLocalName, identifierSpecifier.Local.Name, scenario.Id);
                break;
            case SenseArgumentImportedNameNodeKind.StringLiteral:
                Assert.IsInstanceOfType<ImportSpecifier>(specifier, scenario.Id);
                var stringSpecifier = (ImportSpecifier)specifier;
                Assert.IsInstanceOfType<StringLiteral>(stringSpecifier.Imported, scenario.Id);
                Assert.AreEqual(
                    scenario.ImportedName,
                    ((StringLiteral)stringSpecifier.Imported).Value,
                    scenario.Id);
                Assert.AreEqual(expectedLocalName, stringSpecifier.Local.Name, scenario.Id);
                break;
            case SenseArgumentImportedNameNodeKind.Default:
                Assert.IsInstanceOfType<ImportDefaultSpecifier>(specifier, scenario.Id);
                Assert.AreEqual(expectedLocalName, specifier.Local.Name, scenario.Id);
                break;
            default:
                throw new InvalidOperationException(
                    $"{scenario.Id}: unsupported imported-name node kind '{scenario.ImportedNameNodeKind}'.");
        }
    }
}

public enum SenseArgumentImportedNameNodeKind
{
    Identifier,
    StringLiteral,
    Default
}

public sealed record SenseArgumentModuleNameScenario(
    string Id,
    string Dimension,
    string ModulePath,
    string ImportedName,
    bool RequiresAlias,
    SenseArgumentImportedNameNodeKind ImportedNameNodeKind)
{
    public string InputIdentity =>
        $"{ModulePath}|{ImportedName}|{RequiresAlias}|{ImportedNameNodeKind}";
}

internal static class SenseArgumentModuleNameScenarioCatalog
{
    public static IReadOnlyList<SenseArgumentModuleNameScenario> All { get; } =
    [
        Scenario(
            "hyphenated-name",
            "non-identifier-module-export-name-uses-string-import-and-alias",
            "features",
            "feature-name",
            requiresAlias: true,
            SenseArgumentImportedNameNodeKind.StringLiteral),
        Scenario(
            "spaced-name",
            "whitespace-module-export-name-uses-string-import-and-alias",
            "features",
            "feature name",
            requiresAlias: true,
            SenseArgumentImportedNameNodeKind.StringLiteral),
        Scenario(
            "quoted-name",
            "quoted-module-export-name-retains-decoded-string-value",
            "features",
            "feature\"name",
            requiresAlias: true,
            SenseArgumentImportedNameNodeKind.StringLiteral),
        Scenario(
            "reserved-class",
            "reserved-keyword-import-uses-identifier-name-and-local-alias",
            "features",
            "class",
            requiresAlias: true,
            SenseArgumentImportedNameNodeKind.Identifier),
        Scenario(
            "module-await",
            "module-reserved-await-import-uses-local-alias",
            "features",
            "await",
            requiresAlias: true,
            SenseArgumentImportedNameNodeKind.Identifier),
        Scenario(
            "default-export",
            "default-export-import-retains-default-specifier-contract",
            "features",
            "default",
            requiresAlias: true,
            SenseArgumentImportedNameNodeKind.Default),
        Scenario(
            "dollar-binding",
            "dollar-prefixed-import-name-remains-direct-binding",
            "features",
            "$feature",
            requiresAlias: false,
            SenseArgumentImportedNameNodeKind.Identifier),
        Scenario(
            "unicode-binding",
            "unicode-import-name-remains-direct-binding",
            "features",
            "发布",
            requiresAlias: false,
            SenseArgumentImportedNameNodeKind.Identifier)
    ];

    private static SenseArgumentModuleNameScenario Scenario(
        string id,
        string dimension,
        string modulePath,
        string importedName,
        bool requiresAlias,
        SenseArgumentImportedNameNodeKind importedNameNodeKind)
        => new(
            $"sense-argument-module-name.{id}",
            dimension,
            modulePath,
            importedName,
            requiresAlias,
            importedNameNodeKind);
}
