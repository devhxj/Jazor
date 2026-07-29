using ECMAScript;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using RoslynLocation = Microsoft.CodeAnalysis.Location;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerInterpolatedStringHandlerScenarioTests
{
    public static IEnumerable<TestDataRow<InterpolatedStringHandlerScenario>> Cases
        => InterpolatedStringHandlerScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<InterpolatedStringHandlerScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndKinds()
    {
        var cases = InterpolatedStringHandlerScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Id.StartsWith("interpolated-handler.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.HasCount(
            Enum.GetValues<InterpolatedStringHandlerScenarioKind>().Length,
            cases.Select(static testCase => testCase.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_RejectsCustomHandlerOperationWithControlledDiagnostic(
        InterpolatedStringHandlerScenario testCase)
    {
        var block = GetBlockOperation(testCase.InvocationExpression);
        var operation = SelectOperation(block, testCase.Kind);
        RoslynLocation? reportedLocation = null;
        string? reportedMessage = null;
        var walker = new SemanticWalker((location, message) =>
        {
            reportedLocation = location;
            reportedMessage = message;
        });

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            walker.Visit(operation, new()));

        Assert.AreEqual(testCase.ExpectedOperationKind, exception.Kind, testCase.Id);
        StringAssert.Contains(exception.Message, testCase.ExpectedMessageFragment, testCase.Id);
        Assert.IsNotNull(reportedLocation, testCase.Id);
        Assert.AreEqual("HandlerScenario.cs", reportedLocation.GetLineSpan().Path, testCase.Id);
        Assert.AreEqual(exception.Message, reportedMessage, testCase.Id);
    }

    private static IOperation SelectOperation(
        IBlockOperation block,
        InterpolatedStringHandlerScenarioKind kind)
    {
        var operations = EnumerateOperations(block).ToArray();
        return kind switch
        {
            InterpolatedStringHandlerScenarioKind.SingleHandlerCreation or
            InterpolatedStringHandlerScenarioKind.AdditionHandlerCreation =>
                block,
            InterpolatedStringHandlerScenarioKind.DirectAddition or
            InterpolatedStringHandlerScenarioKind.NestedDirectAddition =>
                operations.OfType<IInterpolatedStringAdditionOperation>().First(),
            InterpolatedStringHandlerScenarioKind.DirectAppendLiteral =>
                operations.OfType<IInterpolatedStringAppendOperation>()
                    .First(static operation =>
                        operation.Kind == OperationKind.InterpolatedStringAppendLiteral),
            InterpolatedStringHandlerScenarioKind.DirectAppendFormatted =>
                operations.OfType<IInterpolatedStringAppendOperation>()
                    .First(static operation =>
                        operation.Kind == OperationKind.InterpolatedStringAppendFormatted),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    private static IBlockOperation GetBlockOperation(string invocationExpression)
    {
        var source = $$"""
            using System.Runtime.CompilerServices;

            [InterpolatedStringHandler]
            public ref struct TraceHandler
            {
                public TraceHandler(int literalLength, int formattedCount) { }
                public void AppendLiteral(string value) { }
                public void AppendFormatted<T>(T value) { }
            }

            public static class HandlerScenario
            {
                private static void Write(TraceHandler handler) { }

                public static void TestMethod(int value)
                {
                    {{invocationExpression}}
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "HandlerScenario.cs");
        var compilation = CSharpCompilation.Create(
            "InterpolatedStringHandlerScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return (IBlockOperation?)compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!)
            ?? throw new InvalidOperationException("Expected a block operation for TestMethod.");
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

public enum InterpolatedStringHandlerScenarioKind
{
    SingleHandlerCreation,
    AdditionHandlerCreation,
    DirectAddition,
    NestedDirectAddition,
    DirectAppendLiteral,
    DirectAppendFormatted
}

public sealed record InterpolatedStringHandlerScenario(
    string Id,
    string Dimension,
    InterpolatedStringHandlerScenarioKind Kind,
    string InvocationExpression,
    OperationKind ExpectedOperationKind,
    string ExpectedMessageFragment);

internal static class InterpolatedStringHandlerScenarioCatalog
{
    private const string HandlerCreationMessage =
        "Interpolated string handler creation operations are not supported";
    private const string AdditionMessage =
        "Interpolated string addition operations are not supported";
    private const string AppendMessage =
        "Interpolated string append operations are not supported";

    public static IReadOnlyList<InterpolatedStringHandlerScenario> All { get; } =
    [
        Case(
            "single.creation",
            "single-interpolated-string-handler-boundary",
            InterpolatedStringHandlerScenarioKind.SingleHandlerCreation,
            "Write($\"value {value}\");",
            OperationKind.InterpolatedStringHandlerCreation,
            HandlerCreationMessage),
        Case(
            "addition.creation",
            "addition-handler-mainline-boundary",
            InterpolatedStringHandlerScenarioKind.AdditionHandlerCreation,
            "Write($\"left {value}\" + $\" right {value + 1}\");",
            OperationKind.InterpolatedStringHandlerCreation,
            HandlerCreationMessage),
        Case(
            "addition.direct",
            "direct-addition-operation-boundary",
            InterpolatedStringHandlerScenarioKind.DirectAddition,
            "Write($\"left {value}\" + $\" right {value + 1}\");",
            OperationKind.InterpolatedStringAddition,
            AdditionMessage),
        Case(
            "addition.nested",
            "nested-addition-tree-boundary",
            InterpolatedStringHandlerScenarioKind.NestedDirectAddition,
            "Write($\"first {value}\" + $\" second\" + $\" third {value + 1}\");",
            OperationKind.InterpolatedStringAddition,
            AdditionMessage),
        Case(
            "append.literal",
            "handler-append-literal-boundary",
            InterpolatedStringHandlerScenarioKind.DirectAppendLiteral,
            "Write($\"prefix {value}\");",
            OperationKind.InterpolatedStringAppendLiteral,
            AppendMessage),
        Case(
            "append.formatted",
            "handler-append-formatted-boundary",
            InterpolatedStringHandlerScenarioKind.DirectAppendFormatted,
            "Write($\"prefix {value}\");",
            OperationKind.InterpolatedStringAppendFormatted,
            AppendMessage)
    ];

    private static InterpolatedStringHandlerScenario Case(
        string id,
        string dimension,
        InterpolatedStringHandlerScenarioKind kind,
        string invocationExpression,
        OperationKind expectedOperationKind,
        string expectedMessageFragment)
        => new(
            $"interpolated-handler.{id}",
            dimension,
            kind,
            invocationExpression,
            expectedOperationKind,
            expectedMessageFragment);
}
