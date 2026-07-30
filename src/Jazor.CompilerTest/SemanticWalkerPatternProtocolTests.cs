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

        foreach (var fragment in testCase.SingleOccurrenceJavaScriptFragments)
            Assert.AreEqual(1, CountOccurrences(first, fragment), $"{testCase.Id}: {fragment}");

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

                public sealed class BoundedSliceBuffer
                {
                    public int Length => 4;
                    public int this[int index] => index + 1;
                    public int[] Slice(int start, int length) => [];
                }

                public sealed class PropertyPatternSource
                {
                    private int reads;
                    public int Reads => reads;
                    public int Value => ++reads;
                }

                private static LengthBuffer GetLengthBuffer() => new();
                private static int GetNumber() => 1;
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

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += fragment.Length;
        }

        return count;
    }
}

public sealed record PatternProtocolSuccessCase(
    string Id,
    string Dimension,
    string Body,
    IReadOnlyList<string> ExpectedJavaScriptFragments)
{
    public IReadOnlyList<string> SingleOccurrenceJavaScriptFragments { get; init; } = [];
}

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
            "count.trailing-element",
            "carrier=custom;length=Count;indexer=int;slice=discard;position=middle;length=cached",
            """
                        var buffer = new CountBuffer();
                        bool matches = buffer is [1, .., 3];
                """,
            "v$0 = buffer.count",
            "v$0 >= 2",
            "buffer[0] === 1",
            "buffer[v$0 - 1] === 3") with
            {
                SingleOccurrenceJavaScriptFragments = ["buffer.count"]
            },
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
            "rest = values.slice(1, 1 + (values.length - 1))"),
        Success(
            "nested.fixed-arrays",
            "carrier=int[][];pattern=nested-list;position=all;shape=fixed",
            """
                        int[][] values = [[1, 2], [3, 4]];
                        bool matches = values is [[1, 2], [3, 4]];
            """,
            "Array.isArray(values)",
            "v$0 = values[0]",
            "Array.isArray(v$0)",
            "v$0[0] === 1",
            "v$1 = values[1]",
            "Array.isArray(v$1)",
            "v$1[1] === 4"),
        Success(
            "nested.after-discard-slice",
            "carrier=int[][];pattern=nested-list;position=after-slice;index=from-end",
            """
                        int[][] values = [[1, 2], [3, 4]];
                        bool matches = values is [.., [3, 4]];
            """,
            "values.length >= 1",
            "v$0 = values[values.length - 1]",
            "Array.isArray(v$0)",
            "v$0[0] === 3",
            "v$0[1] === 4"),
        Success(
            "slice.array.tail",
            "carrier=int[];slice=intrinsic-array;position=tail;binding=declaration",
            """
                        int[] values = [1, 2, 3, 4];
                        if (values is [var first, .. var tail])
                        {
                            Consume(first);
                            Consume(tail.Length);
                        }
                """,
            "values.length >= 1",
            "first = values[0]",
            "tail = values.slice(1)"),
        Success(
            "slice.array.middle",
            "carrier=int[];slice=intrinsic-array;position=middle;binding=declaration",
            """
                        int[] values = [1, 2, 3, 4];
                        if (values is [1, .. var middle, 4])
                            Consume(middle.Length);
                """,
            "values.length >= 2",
            "values[0] === 1",
            "middle = values.slice(1, -1)",
            "values[values.length - 1] === 4"),
        Success(
            "slice.custom-bounded-method",
            "carrier=custom;slice=Slice(int,int);position=middle;length=cached;binding=declaration",
            """
                        var buffer = new BoundedSliceBuffer();
                        if (buffer is [1, .. var middle, 4])
                            Consume(middle.Length);
                """,
            "v$0 = buffer.length",
            "v$0 >= 2",
            "buffer[0] === 1",
            "middle = buffer.slice(1, v$0 - 2)",
            "buffer[v$0 - 1] === 4"),
        Success(
            "slice.custom-whole-method",
            "carrier=custom;slice=Slice(int,int);position=whole;length=cached;binding=declaration",
            """
                        var buffer = new BoundedSliceBuffer();
                        if (buffer is [.. var all])
                            Consume(all.Length);
                """,
            "v$0 = buffer.length",
            "all = buffer.slice(0, v$0)"),
        Success(
            "slice.array-recursive-subpattern",
            "carrier=int[];slice=intrinsic-array;subpattern=recursive-property;binding=none",
            """
                        int[] values = [1, 2];
                        bool matches = values is [.. { Length: 2 }];
                """,
            "values.length >= 0",
            "v$0 = values.slice(0)",
            "Array.isArray(v$0)",
            "v$0.length === 2"),
        Success(
            "binary.property-getter-input",
            "carrier=source-class;input=property-getter;pattern=relational-and;evaluation=single",
            """
                        var holder = new PropertyPatternSource();
                        bool matches = holder.Value is > 0 and < 10;
                        Consume(matches);
                        Consume(holder.Reads);
            """,
            "v$0 = holder.value",
            "v$0 > 0",
            "v$0 < 10") with
            {
                SingleOccurrenceJavaScriptFragments = ["holder.value"]
            },
        Success(
            "recursive.value-type-empty",
            "carrier=int;input=invocation;pattern=empty-recursive;result=constant-true;evaluation=preserved",
            """
                        bool matches = GetNumber() is { };
                        Consume(matches);
            """,
            "PatternProtocolScenarios.getNumber()",
            "true") with
            {
                SingleOccurrenceJavaScriptFragments = ["PatternProtocolScenarios.getNumber()"]
            },
        Success(
            "recursive.nullable-value-empty",
            "carrier=int?;input=nullable-local;pattern=empty-recursive;result=runtime-non-null",
            """
                        int? value = GetNumber();
                        bool matches = value is { };
                        Consume(matches);
            """,
            "typeof value === \"number\""),
        Success(
            "recursive.boxed-value-empty",
            "carrier=object;input=boxed-local;pattern=int-empty-recursive;result=runtime-narrowing",
            """
                        object value = GetNumber();
                        bool matches = value is int { };
                        Consume(matches);
            """,
            "typeof value === \"number\"")
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
