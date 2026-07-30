using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerPatternProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IBlockOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<PatternProtocolSuccessCase>> SuccessCases
        => PatternProtocolCatalog.SuccessCases.Select(static testCase =>
            new TestDataRow<PatternProtocolSuccessCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<PatternProtocolFailureCase>> FailureCases
        => PatternProtocolCatalog.FailureCases.Select(static testCase =>
            new TestDataRow<PatternProtocolFailureCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndBodies()
    {
        var all = PatternProtocolCatalog.SuccessCases
            .Select(static testCase => (testCase.Id, testCase.Dimension, testCase.Body))
            .Concat(PatternProtocolCatalog.FailureCases.Select(static testCase =>
                (testCase.Id, testCase.Dimension, testCase.Body)))
            .ToArray();

        Assert.IsNotEmpty(all);
        Assert.HasCount(all.Length, all.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(all.Length, all.Select(static item => item.Body).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(all.All(static item => item.Id.StartsWith("pattern-protocol.", StringComparison.Ordinal)));
        Assert.IsTrue(all.All(static item => !string.IsNullOrWhiteSpace(item.Dimension)));
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void Visit_ListPatternProtocol_ProducesDeterministicParsableJavaScript(PatternProtocolSuccessCase testCase)
    {
        var block = Operations.Value[testCase.Id];
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);
        foreach (var fragment in testCase.ExpectedJavaScriptFragments)
            StringAssert.Contains(first, fragment, testCase.Id);

        _ = new Parser().ParseScript(first);
    }

    [TestMethod]
    [DynamicData(nameof(FailureCases))]
    public void Visit_ListPatternProtocol_RejectsUnrepresentableSliceContract(PatternProtocolFailureCase testCase)
    {
        var block = Operations.Value[testCase.Id];

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        foreach (var fragment in testCase.ExpectedDiagnosticFragments)
            StringAssert.Contains(exception.Message, fragment, testCase.Id);
    }

    private static IReadOnlyDictionary<string, IBlockOperation> CreateOperations()
    {
        var all = PatternProtocolCatalog.SuccessCases
            .Select(static testCase => (testCase.Id, testCase.Body))
            .Concat(PatternProtocolCatalog.FailureCases.Select(static testCase => (testCase.Id, testCase.Body)))
            .ToArray();
        var methods = string.Join(
            Environment.NewLine,
            all.Select(static (testCase, index) => $$"""
                    public void Scenario{{index:D2}}()
                    {
                {{testCase.Body}}
                    }
                """));
        var source = $$"""
            using System;
            using System.Collections.Generic;

            public sealed class PatternProtocolScenarios
            {
                public sealed class LengthBuffer
                {
                    public int Length => 4;
                    public int this[int index] => index + 1;
                }

                public sealed class CountBuffer
                {
                    public int Count => 3;
                    public int this[int index] => index + 1;
                }

                public sealed class RangePropertyBuffer
                {
                    public int Length => 4;
                    public int this[int index] => index + 1;
                    public int[] this[Range range] => [];
                }

                private static LengthBuffer GetLengthBuffer() => new();
                private static void Consume<T>(T value) { }

            {{methods}}
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "PatternProtocolScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var methodBlocks = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static method => method.Identifier.ValueText.StartsWith("Scenario", StringComparison.Ordinal))
            .OrderBy(static method => method.Identifier.ValueText, StringComparer.Ordinal)
            .Select(method => Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!)))
            .ToArray();

        return all.Select(static item => item.Id)
            .Zip(methodBlocks, static (id, block) => (id, block))
            .ToDictionary(static item => item.id, static item => item.block, StringComparer.Ordinal);
    }
}

public sealed record PatternProtocolSuccessCase(
    string Id,
    string Dimension,
    string Body,
    IReadOnlyList<string> ExpectedJavaScriptFragments);

public sealed record PatternProtocolFailureCase(
    string Id,
    string Dimension,
    string Body,
    IReadOnlyList<string> ExpectedDiagnosticFragments);

internal static class PatternProtocolCatalog
{
    public static IReadOnlyList<PatternProtocolSuccessCase> SuccessCases { get; } =
    [
        Success(
            "length.fixed",
            "carrier=custom;length=Length;indexer=int;slice=none",
            """
                        var buffer = new LengthBuffer();
                        bool matches = buffer is [1, 2, 3, 4];
                """,
            "buffer != null",
            "buffer.length === 4",
            "buffer[0] === 1",
            "buffer[3] === 4"),
        Success(
            "count.fixed",
            "carrier=custom;length=Count;indexer=int;slice=none",
            """
                        var buffer = new CountBuffer();
                        bool matches = buffer is [1, 2, 3];
                """,
            "buffer != null",
            "buffer.count === 3",
            "buffer[0] === 1",
            "buffer[2] === 3"),
        Success(
            "length.declaration-bindings",
            "carrier=custom;length=Length;indexer=int;patterns=declaration",
            """
                        var buffer = new LengthBuffer();
                        if (buffer is [var first, 2, var third, 4])
                        {
                            Consume(first);
                            Consume(third);
                        }
                """,
            "buffer.length === 4",
            "(first = buffer[0], true)",
            "buffer[1] === 2",
            "(third = buffer[2], true)",
            "buffer[3] === 4"),
        Success(
            "length.side-effecting-input",
            "carrier=custom;input=invocation;length=Length;indexer=int;evaluation=single",
            """
                        bool matches = GetLengthBuffer() is [1, 2, 3, 4];
                """,
            "v$0 = PatternProtocolScenarios.getLengthBuffer()",
            "v$0.length === 4",
            "v$0[0] === 1",
            "v$0[3] === 4"),
        Success(
            "slice.string.middle",
            "carrier=string;length=Length;indexer=int;slice=runtime;position=middle",
            """
                        string text = "abcd";
                        if (text is ['a', .. var middle, 'd'])
                            Consume(middle.Length);
                """,
            "typeof text === \"string\"",
            "text.length >= 2",
            "middle = text.substring(1, 1 + (text.length - 2))"),
        Success(
            "slice.list.tail",
            "carrier=List<int>;length=Count;indexer=int;slice=runtime;position=tail",
            """
                        List<int> values = [1, 2, 3, 4];
                        if (values is [var first, .. var rest])
                        {
                            Consume(first);
                            Consume(rest.Count);
                        }
                """,
            "Array.isArray(values)",
            "values.length >= 1",
            "rest = values.slice(1, 1 + (values.length - 1))")
    ];

    public static IReadOnlyList<PatternProtocolFailureCase> FailureCases { get; } =
    [
        Failure(
            "slice.range-property",
            "carrier=custom;slice=indexer(Range);result=rejected",
            """
                        var buffer = new RangePropertyBuffer();
                        bool matches = buffer is [1, .. var middle, 4];
                """,
            "Range-based slice property",
            "Expose a Slice(int, int) member")
    ];

    private static PatternProtocolSuccessCase Success(
        string id,
        string dimension,
        string body,
        params string[] expectedJavaScriptFragments)
        => new($"pattern-protocol.{id}", dimension, body, expectedJavaScriptFragments);

    private static PatternProtocolFailureCase Failure(
        string id,
        string dimension,
        string body,
        params string[] expectedDiagnosticFragments)
        => new($"pattern-protocol.{id}", dimension, body, expectedDiagnosticFragments);
}
