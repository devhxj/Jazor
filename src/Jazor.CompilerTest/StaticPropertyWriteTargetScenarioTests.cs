using ECMAScript;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class StaticPropertyWriteTargetScenarioTests
{
    public static IEnumerable<TestDataRow<StaticPropertyWriteSuccessScenario>> SuccessCases
        => StaticPropertyWriteTargetScenarioCatalog.Successes.Select(static testCase =>
            new TestDataRow<StaticPropertyWriteSuccessScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<StaticPropertyWriteFailureScenario>> FailureCases
        => StaticPropertyWriteTargetScenarioCatalog.Failures.Select(static testCase =>
            new TestDataRow<StaticPropertyWriteFailureScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsKindsSourcesAndOutputs()
    {
        var allIds = StaticPropertyWriteTargetScenarioCatalog.Successes.Select(static testCase => testCase.Id)
            .Concat(StaticPropertyWriteTargetScenarioCatalog.Failures.Select(static testCase => testCase.Id))
            .ToArray();
        var allDimensions = StaticPropertyWriteTargetScenarioCatalog.Successes.Select(static testCase => testCase.Dimension)
            .Concat(StaticPropertyWriteTargetScenarioCatalog.Failures.Select(static testCase => testCase.Dimension))
            .ToArray();
        var allSources = StaticPropertyWriteTargetScenarioCatalog.Successes.Select(static testCase => testCase.Source)
            .Concat(StaticPropertyWriteTargetScenarioCatalog.Failures.Select(static testCase => testCase.Source))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allDimensions.Length, allDimensions.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allSources.Length, allSources.Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            StaticPropertyWriteTargetScenarioCatalog.Successes.Count,
            StaticPropertyWriteTargetScenarioCatalog.Successes
                .Select(static testCase => testCase.ExpectedScript)
                .Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            Enum.GetValues<StaticPropertyWriteSuccessKind>().Length,
            StaticPropertyWriteTargetScenarioCatalog.Successes.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<StaticPropertyWriteFailureKind>().Length,
            StaticPropertyWriteTargetScenarioCatalog.Failures.Select(static testCase => testCase.Kind).Distinct());
        Assert.IsTrue(allIds.All(static id => id.StartsWith("static-property-write.", StringComparison.Ordinal)));
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void VisitSimpleAssignment_UsesBoundStaticRuntimeTarget(
        StaticPropertyWriteSuccessScenario testCase)
    {
        var block = CompileBlock(testCase.Source, testCase.Id);
        var walker = new SemanticWalker(true);

        var first = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.AreEqual(testCase.ExpectedScript.ReplaceLineEndings("\n"), first?.ReplaceLineEndings("\n"), testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(FailureCases))]
    public void VisitSimpleAssignment_RejectsUnsupportedStaticSourceHost(
        StaticPropertyWriteFailureScenario testCase)
    {
        var block = CompileBlock(testCase.Source, testCase.Id);

        var first = CaptureFailure(block);
        var second = CaptureFailure(block);

        Assert.AreEqual(OperationKind.PropertyReference, first.Kind, testCase.Id);
        foreach (var fragment in testCase.ExpectedDiagnosticFragments)
            StringAssert.Contains(first.Message, fragment, StringComparison.Ordinal, testCase.Id);

        Assert.AreEqual(first.Message, second.Message, testCase.Id);
        Assert.AreEqual(testCase.Id + ".cs", Path.GetFileName(ReadLocation<string>(first, "location.path", testCase.Id)), testCase.Id);
        Assert.AreEqual(testCase.ExpectedLocation.StartLine, ReadLocation<int>(first, "location.startLine", testCase.Id), testCase.Id);
        Assert.AreEqual(testCase.ExpectedLocation.StartColumn, ReadLocation<int>(first, "location.startColumn", testCase.Id), testCase.Id);
        Assert.AreEqual(testCase.ExpectedLocation.EndLine, ReadLocation<int>(first, "location.endLine", testCase.Id), testCase.Id);
        Assert.AreEqual(testCase.ExpectedLocation.EndColumn, ReadLocation<int>(first, "location.endColumn", testCase.Id), testCase.Id);
        Assert.AreEqual(ReadLocation<string>(first, "location.path", testCase.Id), ReadLocation<string>(second, "location.path", testCase.Id), testCase.Id);
        Assert.AreEqual(ReadLocation<int>(first, "location.startLine", testCase.Id), ReadLocation<int>(second, "location.startLine", testCase.Id), testCase.Id);
        Assert.AreEqual(ReadLocation<int>(first, "location.startColumn", testCase.Id), ReadLocation<int>(second, "location.startColumn", testCase.Id), testCase.Id);
        Assert.AreEqual(ReadLocation<int>(first, "location.endLine", testCase.Id), ReadLocation<int>(second, "location.endLine", testCase.Id), testCase.Id);
        Assert.AreEqual(ReadLocation<int>(first, "location.endColumn", testCase.Id), ReadLocation<int>(second, "location.endColumn", testCase.Id), testCase.Id);
    }

    private static OperationTransformationException CaptureFailure(IBlockOperation block)
        => Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

    private static T ReadLocation<T>(Exception exception, string key, string scenarioId)
    {
        var value = exception.Data[key];
        Assert.IsInstanceOfType<T>(value, $"{scenarioId}: metadata '{key}'.");
        return (T)value;
    }

    private static IBlockOperation CompileBlock(string source, string scenarioId)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            scenarioId + ".cs");
        var compilation = CSharpCompilation.Create(
            "StaticPropertyWrite_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, scenarioId + ": " + string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Run");
        var operation = model.GetOperation(method.Body!);
        Assert.IsInstanceOfType<IBlockOperation>(operation, scenarioId);
        return (IBlockOperation)operation;
    }
}

public enum StaticPropertyWriteSuccessKind
{
    RuntimeHost,
    RuntimeHostAlias,
    ImplicitRuntimeHost,
    GlobalBoundary
}

public enum StaticPropertyWriteFailureKind
{
    UnsupportedSourceHost
}

public sealed record StaticPropertyWriteSuccessScenario(
    string Id,
    string Dimension,
    StaticPropertyWriteSuccessKind Kind,
    string Source,
    string ExpectedScript);

public sealed record StaticPropertyWriteFailureScenario(
    string Id,
    string Dimension,
    StaticPropertyWriteFailureKind Kind,
    string Source,
    IReadOnlyList<string> ExpectedDiagnosticFragments,
    StaticPropertyWriteLocation ExpectedLocation);

public readonly record struct StaticPropertyWriteLocation(
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn);

internal static class StaticPropertyWriteTargetScenarioCatalog
{
    public static IReadOnlyList<StaticPropertyWriteSuccessScenario> Successes { get; } =
    [
        new(
            "static-property-write.runtime-host",
            "ecmascript-runtime-host-preserved",
            StaticPropertyWriteSuccessKind.RuntimeHost,
            """
            using ECMAScript;

            [ECMAScript]
            public static class RuntimeState
            {
                public static int Value { get; set; }
            }

            public sealed class Scenario
            {
                void Run()
                {
                    RuntimeState.Value = 1;
                }
            }
            """,
            """
            {
              RuntimeState.Value = 1;
            }
            """),
        new(
            "static-property-write.runtime-host-alias",
            "using-alias-resolves-to-runtime-host",
            StaticPropertyWriteSuccessKind.RuntimeHostAlias,
            """
            using ECMAScript;
            using State = RuntimeState;

            [ECMAScript]
            public static class RuntimeState
            {
                public static int Value { get; set; }
            }

            public sealed class Scenario
            {
                void Run()
                {
                    State.Value = 2;
                }
            }
            """,
            """
            {
              RuntimeState.Value = 2;
            }
            """),
        new(
            "static-property-write.implicit-runtime-host",
            "implicit-static-access-recovers-declaring-runtime-host",
            StaticPropertyWriteSuccessKind.ImplicitRuntimeHost,
            """
            using ECMAScript;

            [ECMAScript]
            public static class RuntimeState
            {
                public static int Value { get; set; }

                public static void Run()
                {
                    Value = 3;
                }
            }
            """,
            """
            {
              RuntimeState.Value = 3;
            }
            """),
        new(
            "static-property-write.global-boundary",
            "name-boundary-emits-global-property-identifier",
            StaticPropertyWriteSuccessKind.GlobalBoundary,
            """
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScript]
            [Description("@#")]
            public static class GlobalState
            {
                [Description("@#globalValue")]
                public static int Value { get; set; }
            }

            public sealed class Scenario
            {
                void Run()
                {
                    GlobalState.Value = 4;
                }
            }
            """,
            """
            {
              globalValue = 4;
            }
            """)
    ];

    public static IReadOnlyList<StaticPropertyWriteFailureScenario> Failures { get; } =
    [
        new(
            "static-property-write.source-host",
            "unsupported-source-static-host-rejected-at-use-site",
            StaticPropertyWriteFailureKind.UnsupportedSourceHost,
            """
            public static class SourceState
            {
                public static int Value { get; set; }
            }

            public sealed class Scenario
            {
                void Run()
                {
                    SourceState.Value = 5;
                }
            }
            """,
            [
                "External type 'SourceState' is not supported and cannot be used for property assignment.",
                "Only [ECMAScript]/[ECMAScriptModule] types (or nested under such types) and whitelist types are supported."
            ],
            new(10, 9, 10, 26))
    ];
}
