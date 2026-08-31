using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class ESGeneratorModulePathCollisionTests
{
    public static IEnumerable<TestDataRow<ModulePathScenario>> Cases
        => ModulePathScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<ModulePathScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsAndSemanticDimensions()
    {
        var cases = ModulePathScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase => testCase.Id.StartsWith("es-generator.module-path.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(cases.All(static testCase => testCase.Sources.Count > 0));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Sources.Select(static source => source.Path).Distinct(StringComparer.Ordinal).Count() ==
            testCase.Sources.Count));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void RunGenerator_EnforcesUniqueNormalizedOutputPaths(ModulePathScenario testCase)
    {
        var compilation = CreateCompilation(testCase);
        AssertNoCompilationErrors(compilation, testCase.Id, "input");

        var (outputCompilation, runResult) = RunGenerator(compilation);
        var diagnostics = runResult.Results
            .SelectMany(static result => result.Diagnostics)
            .ToArray();
        var duplicatePathDiagnostics = diagnostics
            .Where(static diagnostic => diagnostic.Id == "JAZORG003")
            .ToArray();
        var unexpectedDiagnostics = diagnostics
            .Where(static diagnostic => diagnostic.Id != "JAZORG003")
            .ToArray();

        Assert.HasCount(0, unexpectedDiagnostics, FormatDiagnostics(testCase.Id, diagnostics));
        AssertDuplicatePathDiagnostics(testCase, duplicatePathDiagnostics);
        AssertGeneratedCatalogs(testCase, runResult);
        AssertNoCompilationErrors(outputCompilation, testCase.Id, "generated output");
    }

    private static void AssertDuplicatePathDiagnostics(
        ModulePathScenario testCase,
        IReadOnlyList<Diagnostic> diagnostics)
    {
        Assert.HasCount(testCase.Conflicts.Count, diagnostics, FormatDiagnostics(testCase.Id, diagnostics));
        if (testCase.DuplicatePath is null)
            return;

        var orderedTypeNames = testCase.Conflicts
            .Select(static conflict => conflict.TypeName)
            .OrderBy(static typeName => typeName, StringComparer.Ordinal);
        var expectedMessage =
            $"JavaScript module path '{testCase.DuplicatePath}' is produced by multiple module types: {string.Join(", ", orderedTypeNames)}";

        foreach (var diagnostic in diagnostics)
        {
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity, testCase.Id);
            Assert.AreEqual(expectedMessage, diagnostic.GetMessage(), testCase.Id);
            Assert.IsTrue(diagnostic.Location.IsInSource, testCase.Id);

            var sourcePath = diagnostic.Location.SourceTree?.FilePath;
            Assert.IsNotNull(sourcePath, testCase.Id);
            var conflict = testCase.Conflicts.Single(item => item.SourcePath == sourcePath);
            var sourceText = diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);
            StringAssert.Contains(sourceText, $"class {conflict.TypeName}", testCase.Id);
        }

        CollectionAssert.AreEquivalent(
            testCase.Conflicts.Select(static conflict => conflict.SourcePath).ToArray(),
            diagnostics.Select(static diagnostic => diagnostic.Location.SourceTree!.FilePath).ToArray(),
            testCase.Id);
    }

    private static void AssertGeneratedCatalogs(
        ModulePathScenario testCase,
        GeneratorDriverRunResult runResult)
    {
        var generatedSources = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .ToArray();

        if (testCase.ExpectedOrderedPaths.Count == 0)
        {
            Assert.HasCount(0, generatedSources, testCase.Id);
            return;
        }

        CollectionAssert.AreEquivalent(
            new[] { "Jazor.Generated.ModuleCatalog.g.cs" },
            generatedSources.Select(static source => source.HintName).ToArray(),
            testCase.Id);

        var moduleCatalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleCatalog.g.cs");
        Assert.HasCount(
            testCase.ExpectedOrderedPaths.Count,
            FindAllIndexes(moduleCatalog, "relativePath: \"").ToArray(),
            testCase.Id);

        var previousIndex = -1;
        foreach (var path in testCase.ExpectedOrderedPaths)
        {
            var marker = $"relativePath: \"{path}\"";
            var index = moduleCatalog.IndexOf(marker, StringComparison.Ordinal);
            Assert.IsGreaterThan(previousIndex, index, $"{testCase.Id}: missing or unordered marker '{marker}'.");
            previousIndex = index;
            StringAssert.Contains(moduleCatalog, $"sourceMapRelativePath: \"{path}.map\"", testCase.Id);
        }

        foreach (var conflict in testCase.Conflicts)
            Assert.AreEqual(
                -1,
                moduleCatalog.IndexOf($"typeName: \"{conflict.TypeName}\"", StringComparison.Ordinal),
                testCase.Id);
    }

    private static IEnumerable<int> FindAllIndexes(string text, string value)
    {
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            yield return index;
            index += value.Length;
        }
    }

    private static CSharpCompilation CreateCompilation(ModulePathScenario testCase)
        => CSharpCompilation.Create(
            assemblyName: "ESGenerator.ModulePath.Tests",
            syntaxTrees: ModulePathScenarioCatalog.AttributeSource
                .Concat(testCase.Sources)
                .Select(source => CSharpSyntaxTree.ParseText(
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
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId} ({stage}){Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString()))}");
    }

    private static string FormatDiagnostics(string scenarioId, IReadOnlyList<Diagnostic> diagnostics)
        => $"{scenarioId}{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))}";
}

public sealed record ModulePathSource(string Path, string Text);

public sealed record ModulePathConflict(string TypeName, string SourcePath);

public sealed record ModulePathScenario(
    string Id,
    string Dimension,
    IReadOnlyList<ModulePathSource> Sources,
    string? DuplicatePath,
    IReadOnlyList<ModulePathConflict> Conflicts,
    IReadOnlyList<string> ExpectedOrderedPaths);

internal static class ModulePathScenarioCatalog
{
    public static IReadOnlyList<ModulePathSource> AttributeSource { get; } =
    [
        new(
            "ECMAScriptModuleAttribute.cs",
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }
            """)
    ];

    public static IReadOnlyList<ModulePathScenario> All { get; } =
    [
        Collision(
            "es-generator.module-path.exact-duplicate",
            "exact-normalized-output-path-collision",
            "shared.mjs",
            [Module("SecondModule", "\"shared\"", 2), Module("FirstModule", "\"shared\"", 1)]),
        Collision(
            "es-generator.module-path.extension-normalized",
            "relative-prefix-and-extension-equivalence",
            "feature/tool.mjs",
            [Module("FirstModule", "\"./feature/tool\"", 1), Module("SecondModule", "\"feature/tool.mjs\"", 2)]),
        Collision(
            "es-generator.module-path.separator-normalized",
            "platform-separator-equivalence",
            "feature/tool.mjs",
            [Module("FirstModule", "@\"feature\\tool\"", 1), Module("SecondModule", "\"feature/tool\"", 2)]),
        Collision(
            "es-generator.module-path.case-insensitive",
            "cross-platform-case-equivalence",
            "Feature/Tool.mjs",
            [Module("FirstModule", "\"feature/tool\"", 1), Module("SecondModule", "\"Feature/Tool.mjs\"", 2)]),
        Collision(
            "es-generator.module-path.three-way",
            "multi-owner-diagnostic-completeness",
            "shared.mjs",
            [
                Module("GammaModule", "@\"shared\\\"", 3),
                Module("AlphaModule", "\"shared\"", 1),
                Module("BetaModule", "\"./shared.mjs\"", 2)
            ]),
        new(
            "es-generator.module-path.collision-with-valid-module",
            "conflict-isolation-and-partial-generation",
            [
                Module("FirstModule", "\"shared/module\"", 1),
                Module("SecondModule", "\"./shared/module.mjs\"", 2),
                Module("ValidModule", "\"valid/module\"", 3)
            ],
            "shared/module.mjs",
            [Conflict("FirstModule"), Conflict("SecondModule")],
            ["valid/module.mjs"]),
        new(
            "es-generator.module-path.partial-symbol",
            "partial-declarations-remain-one-module",
            [
                new(
                    "PartialModule.First.cs",
                    """
                    [ECMAScript.ECMAScriptModule("partial/module")]
                    public static partial class PartialModule
                    {
                        public static int First() => 1;
                    }
                    """),
                new(
                    "PartialModule.Second.cs",
                    """
                    public static partial class PartialModule
                    {
                        public static int Second() => 2;
                    }
                    """)
            ],
            null,
            [],
            ["partial/module.mjs"]),
        new(
            "es-generator.module-path.distinct-js-extensions",
            "js-and-mjs-paths-remain-distinct",
            [Module("JavaScriptModule", "\"feature/tool.js\"", 1), Module("EcmaScriptModule", "\"feature/tool.mjs\"", 2)],
            null,
            [],
            ["feature/tool.js", "feature/tool.mjs"])
    ];

    private static ModulePathScenario Collision(
        string id,
        string dimension,
        string duplicatePath,
        IReadOnlyList<ModulePathSource> sources)
        => new(
            id,
            dimension,
            sources,
            duplicatePath,
            sources.Select(static source => Conflict(Path.GetFileNameWithoutExtension(source.Path))).ToArray(),
            []);

    private static ModulePathSource Module(string typeName, string importPathExpression, int result)
        => new(
            $"{typeName}.cs",
            $$"""
            [ECMAScript.ECMAScriptModule({{importPathExpression}})]
            public static class {{typeName}}
            {
                public static int Read() => {{result}};
            }
            """);

    private static ModulePathConflict Conflict(string typeName)
        => new(typeName, $"{typeName}.cs");
}
