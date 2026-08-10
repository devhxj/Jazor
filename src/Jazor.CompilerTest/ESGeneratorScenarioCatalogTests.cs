using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class ESGeneratorScenarioCatalogTests
{
    public static IEnumerable<TestDataRow<ESGeneratorScenario>> Cases
        => ESGeneratorScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<ESGeneratorScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSourcePaths()
    {
        var cases = ESGeneratorScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase => testCase.Id.StartsWith("es-generator.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(cases.All(static testCase => testCase.Sources.Count > 0));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Sources.Select(static source => source.Path).Distinct(StringComparer.Ordinal).Count() ==
            testCase.Sources.Count));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void RunGenerator_MatchesScenarioContract(ESGeneratorScenario testCase)
    {
        var compilation = CreateCompilation(testCase);
        AssertNoCompilationErrors(compilation, testCase.Id, "input");

        var (outputCompilation, runResult) = RunGenerator(compilation);
        var generatorDiagnostics = runResult.Results
            .SelectMany(static result => result.Diagnostics)
            .ToArray();
        var moduleErrors = generatorDiagnostics
            .Where(static diagnostic => diagnostic.Id == "JAZORG001")
            .ToArray();
        var unexpectedDiagnostics = generatorDiagnostics
            .Where(static diagnostic => diagnostic.Id is not "JAZORG001")
            .ToArray();

        Assert.HasCount(testCase.ExpectedModuleErrors, moduleErrors, testCase.Id);
        Assert.HasCount(0, unexpectedDiagnostics, FormatDiagnostics(testCase.Id, generatorDiagnostics));
        if (testCase.ExpectedDiagnosticFragment is not null)
        {
            Assert.IsTrue(
                moduleErrors.Any(diagnostic => diagnostic.GetMessage().Contains(
                    testCase.ExpectedDiagnosticFragment,
                    StringComparison.Ordinal)),
                FormatDiagnostics(testCase.Id, generatorDiagnostics));
        }

        var generatedSources = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .ToArray();
        var hintNames = generatedSources
            .Select(static source => source.HintName)
            .ToArray();

        switch (testCase.Outcome)
        {
            case ESGeneratorCatalogOutcome.None:
                Assert.HasCount(0, generatedSources, testCase.Id);
                Assert.HasCount(0, testCase.ExpectedOrderedPaths, testCase.Id);
                break;
            case ESGeneratorCatalogOutcome.ModuleOnly:
                CollectionAssert.AreEquivalent(
                    new[] { "Jazor.Generated.ModuleCatalog.g.cs" },
                    hintNames,
                    testCase.Id);
                AssertModuleCatalog(runResult, testCase);
                break;
            case ESGeneratorCatalogOutcome.ModuleAndSourceMap:
                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        "Jazor.Generated.ModuleCatalog.g.cs",
                        "Jazor.Generated.ModuleSourceMapCatalog.g.cs"
                    },
                    hintNames,
                    testCase.Id);
                AssertModuleCatalog(runResult, testCase);
                AssertSourceMapCatalog(runResult, testCase);
                break;
            default:
                Assert.Fail($"{testCase.Id}: unsupported expected outcome '{testCase.Outcome}'.");
                break;
        }

        AssertNoCompilationErrors(outputCompilation, testCase.Id, "generated output");
    }

    private static CSharpCompilation CreateCompilation(ESGeneratorScenario testCase)
        => CSharpCompilation.Create(
            assemblyName: testCase.AssemblyName,
            syntaxTrees: testCase.Sources.Select(source =>
                CSharpSyntaxTree.ParseText(
                    source.Text,
                    TestMetadataReferences.PreviewParseOptions,
                    path: source.Path)),
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static (Compilation OutputCompilation, GeneratorDriverRunResult RunResult) RunGenerator(
        CSharpCompilation compilation)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ESGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.First().Options);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return (outputCompilation, driver.GetRunResult());
    }

    private static void AssertModuleCatalog(
        GeneratorDriverRunResult runResult,
        ESGeneratorScenario testCase)
    {
        var catalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleCatalog.g.cs");
        var previousIndex = -1;
        foreach (var path in testCase.ExpectedOrderedPaths)
        {
            var marker = $"relativePath: \"{path}\"";
            var index = catalog.IndexOf(marker, StringComparison.Ordinal);
            Assert.IsGreaterThan(previousIndex, index, $"{testCase.Id}: missing or unordered marker '{marker}'.");
            previousIndex = index;
        }

        foreach (var fragment in testCase.ExpectedModuleCatalogFragments)
            StringAssert.Contains(catalog, fragment, testCase.Id);
    }

    private static void AssertSourceMapCatalog(
        GeneratorDriverRunResult runResult,
        ESGeneratorScenario testCase)
    {
        var catalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleSourceMapCatalog.g.cs");
        foreach (var path in testCase.ExpectedOrderedPaths)
            StringAssert.Contains(catalog, $"sourceMapRelativePath: \"{path}.map\"", testCase.Id);

        foreach (var fragment in testCase.ExpectedSourceMapCatalogFragments)
            StringAssert.Contains(catalog, fragment, testCase.Id);
    }

    private static string GetGeneratedSource(GeneratorDriverRunResult runResult, string hintName)
        => runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Single(source => source.HintName == hintName)
            .SourceText
            .ToString();

    private static void AssertNoCompilationErrors(
        Compilation compilation,
        string scenarioId,
        string stage)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, $"{scenarioId} ({stage}){Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString()))}");
    }

    private static string FormatDiagnostics(string scenarioId, IReadOnlyList<Diagnostic> diagnostics)
        => $"{scenarioId}{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))}";
}

public sealed record ESGeneratorSource(string Path, string Text);

public enum ESGeneratorCatalogOutcome
{
    None,
    ModuleOnly,
    ModuleAndSourceMap
}

public sealed record ESGeneratorScenario(
    string Id,
    string Dimension,
    string? AssemblyName,
    IReadOnlyList<ESGeneratorSource> Sources,
    ESGeneratorCatalogOutcome Outcome,
    IReadOnlyList<string> ExpectedOrderedPaths,
    IReadOnlyList<string> ExpectedModuleCatalogFragments,
    IReadOnlyList<string> ExpectedSourceMapCatalogFragments,
    int ExpectedModuleErrors,
    string? ExpectedDiagnosticFragment);

internal static class ESGeneratorScenarioCatalog
{
    private const string AttributeDeclaration = """
        using System;

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }
        """;

    public static IReadOnlyList<ESGeneratorScenario> All { get; } =
    [
        Case(
            "es-generator.no-attributed-types",
            "empty-candidate-set",
            "Generator.NoCandidates",
            [Source("Plain.cs", "public static class Plain { public static int Value => 1; }")],
            ESGeneratorCatalogOutcome.None),
        Case(
            "es-generator.instance-module-ignored",
            "non-static-module-filter",
            "Generator.InstanceIgnored",
            [Source("InstanceModule.cs", "[ECMAScript.ECMAScriptModule] public sealed class InstanceModule { }")],
            ESGeneratorCatalogOutcome.None),
        Case(
            "es-generator.global-default-path",
            "global-namespace-default-path",
            "Generator.Global.Default",
            [Source("GlobalModule.cs", "[ECMAScript.ECMAScriptModule] public static class GlobalModule { public static int Read() => 1; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["Generator.Global.Default/GlobalModule.mjs"],
            ["function Read()"]),
        Case(
            "es-generator.namespace-default-path",
            "namespace-derived-default-path",
            "Generator.Namespace.Default",
            [Source(
                "NamespacedModule.cs",
                "namespace Demo.Tools { [ECMAScript.ECMAScriptModule] public static class NamespacedModule { public static int Read() => 2; } }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["Generator.Namespace.Default/Demo/Tools/NamespacedModule.mjs"],
            ["function Read()"]),
        Case(
            "es-generator.missing-assembly-name",
            "assembly-name-fallback",
            null,
            [Source(
                "FallbackAssemblyModule.cs",
                "[ECMAScript.ECMAScriptModule] public static class FallbackAssemblyModule { public static int Read() => 7; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["Jazor.Assembly/FallbackAssemblyModule.mjs"],
            ["assemblyName: \"Jazor.Assembly\""]),
        Case(
            "es-generator.null-configured-path",
            "null-configured-path-defaulting",
            "Generator.Configured.Null",
            [Source(
                "NullPathModule.cs",
                "[ECMAScript.ECMAScriptModule(null)] public static class NullPathModule { public static int Read() => 8; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["Generator.Configured.Null/NullPathModule.mjs"],
            ["function Read()"]),
        Case(
            "es-generator.configured-dot-path",
            "configured-relative-path-normalization",
            "Generator.Configured.DotPath",
            [Source(
                "MathModule.cs",
                "[ECMAScript.ECMAScriptModule(\"./features/math\")] public static class MathModule { public static int Add(int left, int right) => left + right; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["features/math.mjs"],
            ["function Add(left, right)"]),
        Case(
            "es-generator.configured-js-extension",
            "configured-js-extension-preservation",
            "Generator.Configured.Js",
            [Source(
                "ToolModule.cs",
                "[ECMAScript.ECMAScriptModule(\"scripts/tool.js\")] public static class ToolModule { public static bool Ready() => true; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["scripts/tool.js"],
            ["function Ready()"]),
        Case(
            "es-generator.configured-backslash-path",
            "configured-platform-separator-normalization",
            "Generator.Configured.Backslash",
            [Source(
                "BackslashModule.cs",
                "[ECMAScript.ECMAScriptModule(@\"tools\\format\")] public static class BackslashModule { public static string Apply(string value) => value; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["tools/format.mjs"],
            ["function Apply(value)"]),
        Case(
            "es-generator.empty-configured-path",
            "empty-configured-path-defaulting",
            "Generator.Configured.Empty",
            [Source(
                "EmptyPathModule.cs",
                "[ECMAScript.ECMAScriptModule(\"\")] public static class EmptyPathModule { public static int Read() => 3; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["Generator.Configured.Empty/EmptyPathModule.mjs"],
            ["function Read()"]),
        Case(
            "es-generator.erased-declarations-empty-module",
            "declaration-erasure-empty-artifact",
            "Generator.ErasedDeclarations",
            [Source(
                "ContractsModule.cs",
                """
                [ECMAScript.ECMAScriptModule("contracts/types")]
                public static class ContractsModule
                {
                    public enum Mode { None, Active }
                    public interface IContract { int Value { get; } }
                    public sealed record Item(int Value);
                }
                """)],
            ESGeneratorCatalogOutcome.ModuleOnly,
            ["contracts/types.mjs"],
            ["hash: \"E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855\""]),
        Case(
            "es-generator.traversal-path-rejected",
            "configured-path-boundary-failure",
            "Generator.Invalid.Traversal",
            [Source(
                "TraversalModule.cs",
                "[ECMAScript.ECMAScriptModule(\"../outside\")] public static class TraversalModule { public static int Read() => 1; }")],
            ESGeneratorCatalogOutcome.None,
            expectedModuleErrors: 1,
            expectedDiagnosticFragment: "TraversalModule"),
        Case(
            "es-generator.unsupported-event-reported",
            "module-conversion-failure-diagnostic",
            "Generator.Invalid.Event",
            [Source(
                "BrokenModule.cs",
                "[ECMAScript.ECMAScriptModule(\"broken\")] public static class BrokenModule { public static event Action? Changed; }")],
            ESGeneratorCatalogOutcome.None,
            expectedModuleErrors: 1,
            expectedDiagnosticFragment: "BrokenModule"),
        Case(
            "es-generator.valid-and-invalid-modules",
            "partial-success-with-diagnostic",
            "Generator.PartialSuccess",
            [Source(
                "MixedModules.cs",
                """
                [ECMAScript.ECMAScriptModule("valid")]
                public static class ValidModule
                {
                    public static int Read() => 4;
                }

                [ECMAScript.ECMAScriptModule("invalid")]
                public static class InvalidModule
                {
                    public static event Action? Changed;
                }
                """)],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["valid.mjs"],
            ["function Read()"],
            expectedModuleErrors: 1,
            expectedDiagnosticFragment: "InvalidModule"),
        Case(
            "es-generator.multiple-modules-sorted",
            "deterministic-relative-path-order",
            "Generator.Ordered",
            [Source(
                "OrderedModules.cs",
                """
                [ECMAScript.ECMAScriptModule("zeta")]
                public static class ZetaModule { public static int Read() => 1; }

                [ECMAScript.ECMAScriptModule("Alpha")]
                public static class AlphaModule { public static int Read() => 2; }

                [ECMAScript.ECMAScriptModule("beta")]
                public static class BetaModule { public static int Read() => 3; }
                """)],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["Alpha.mjs", "beta.mjs", "zeta.mjs"]),
        Case(
            "es-generator.in-memory-source-without-path",
            "pathless-source-map-generation",
            "Generator.Pathless",
            [Source(
                string.Empty,
                "[ECMAScript.ECMAScriptModule(\"memory/module\")] public static class MemoryModule { public static int Read() => 5; }")],
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["memory/module.mjs"],
            ["function Read()"]),
        Case(
            "es-generator.rooted-multi-source-map",
            "common-source-root-and-content-lookup",
            "Generator.RootedSources",
            RootedSources(),
            ESGeneratorCatalogOutcome.ModuleAndSourceMap,
            ["rooted/module.mjs"],
            ["function Read()"],
            ["Modules/RootedModule.cs"])
    ];

    private static ESGeneratorScenario Case(
        string id,
        string dimension,
        string? assemblyName,
        IReadOnlyList<ESGeneratorSource> sources,
        ESGeneratorCatalogOutcome outcome,
        IReadOnlyList<string>? expectedOrderedPaths = null,
        IReadOnlyList<string>? expectedModuleCatalogFragments = null,
        IReadOnlyList<string>? expectedSourceMapCatalogFragments = null,
        int expectedModuleErrors = 0,
        string? expectedDiagnosticFragment = null)
        => new(
            id,
            dimension,
            assemblyName,
            sources,
            outcome,
            expectedOrderedPaths ?? [],
            expectedModuleCatalogFragments ?? [],
            expectedSourceMapCatalogFragments ?? [],
            expectedModuleErrors,
            expectedDiagnosticFragment);

    private static ESGeneratorSource Source(string path, string declaration)
        => new(path, AttributeDeclaration + Environment.NewLine + declaration);

    private static IReadOnlyList<ESGeneratorSource> RootedSources()
    {
        var root = Path.Combine(Path.GetTempPath(), "jazor-esgenerator-scenarios");
        return
        [
            new ESGeneratorSource(
                Path.Combine(root, "Contracts", "ECMAScriptModuleAttribute.cs"),
                AttributeDeclaration),
            new ESGeneratorSource(
                Path.Combine(root, "Modules", "RootedModule.cs"),
                "[ECMAScript.ECMAScriptModule(\"rooted/module\")] public static class RootedModule { public static int Read() => 6; }")
        ];
    }
}
