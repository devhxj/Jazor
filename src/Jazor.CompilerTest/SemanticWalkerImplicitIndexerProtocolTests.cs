using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerImplicitIndexerProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IBlockOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<ImplicitIndexerProtocolCase>> SuccessCases
        => ImplicitIndexerProtocolCatalog.SuccessCases.Select(static testCase =>
            new TestDataRow<ImplicitIndexerProtocolCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndBodies()
    {
        var all = ImplicitIndexerProtocolCatalog.SuccessCases
            .Select(static testCase => (testCase.Id, testCase.Dimension, testCase.Body))
            .ToArray();

        Assert.IsNotEmpty(all);
        Assert.HasCount(all.Length, all.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(all.Length, all.Select(static item => item.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(all.Length, all.Select(static item => item.Body).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(all.All(static item => item.Id.StartsWith("implicit-indexer.", StringComparison.Ordinal)));
        Assert.IsTrue(ImplicitIndexerProtocolCatalog.SuccessCases.All(static item => item.ExpectedFragments.Count > 0));
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void Visit_ImplicitIndexerProtocol_PreservesBoundAccessAndEvaluation(ImplicitIndexerProtocolCase testCase)
    {
        var block = Operations.Value[testCase.Id];
        var operation = EnumerateOperations(block).OfType<IImplicitIndexerReferenceOperation>().Single();
        Assert.IsNotNull(operation.LengthSymbol, testCase.Id);
        Assert.IsNotNull(operation.IndexerSymbol, testCase.Id);

        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);
        foreach (var fragment in testCase.ExpectedFragments)
            StringAssert.Contains(first, fragment, testCase.Id);

        foreach (var fragment in testCase.SingleOccurrenceFragments)
            Assert.AreEqual(1, CountOccurrences(first, fragment), $"{testCase.Id}: {fragment}");

        var previousIndex = -1;
        foreach (var fragment in testCase.OrderedFragments)
        {
            var index = first.IndexOf(fragment, StringComparison.Ordinal);
            Assert.IsGreaterThan(previousIndex, index, $"{testCase.Id}: {fragment}");
            previousIndex = index;
        }

        _ = new Parser().ParseScript(first);
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

    private static IEnumerable<IOperation> EnumerateOperations(IOperation root)
    {
        yield return root;
        foreach (var child in root.ChildOperations)
        {
            foreach (var descendant in EnumerateOperations(child))
                yield return descendant;
        }
    }

    private static IReadOnlyDictionary<string, IBlockOperation> CreateOperations()
    {
        var all = ImplicitIndexerProtocolCatalog.SuccessCases
            .Select(static testCase => (testCase.Id, testCase.Body))
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

            public sealed class ImplicitIndexerProtocolScenarios
            {
                public sealed class LengthBuffer
                {
                    public int Length => 4;
                    public int this[int index]
                    {
                        get => index + 1;
                        set { }
                    }
                }

                public sealed class CountBuffer
                {
                    public int Count => 4;
                    public int this[int index]
                    {
                        get => index + 1;
                        set { }
                    }
                }

                public sealed class NullableLengthBuffer
                {
                    public int Length => 4;
                    public string? this[int index]
                    {
                        get => null;
                        set { }
                    }
                }

                public sealed class SliceBuffer
                {
                    public int Length => 6;
                    public int this[int index]
                    {
                        get => index + 1;
                        set { }
                    }

                    public int[] Slice(int start, int length) => [];
                }

                private static LengthBuffer GetLengthBuffer() => new();
                private static NullableLengthBuffer GetNullableLengthBuffer() => new();
                private static SliceBuffer GetSliceBuffer() => new();
                private static int NextOffset() => 1;
                private static int NextValue() => 7;
                private static string NextText() => "fallback";

            {{methods}}
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "ImplicitIndexerProtocolScenarios",
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

public sealed record ImplicitIndexerProtocolCase(
    string Id,
    string Dimension,
    string Body,
    IReadOnlyList<string> ExpectedFragments,
    IReadOnlyList<string> SingleOccurrenceFragments,
    IReadOnlyList<string> OrderedFragments);

internal static class ImplicitIndexerProtocolCatalog
{
    public static IReadOnlyList<ImplicitIndexerProtocolCase> SuccessCases { get; } =
    [
        Success(
            "index.length-local",
            "form=index;bound=from-end;length=Length;receiver=local;access=read",
            """
                        var buffer = new LengthBuffer();
                        var value = buffer[^1];
                """,
            ["buffer[buffer.length - 1]"],
            [],
            []),
        Success(
            "index.count-local",
            "form=index;bound=from-end;length=Count;operand=local;access=read",
            """
                        var buffer = new CountBuffer();
                        var offset = 2;
                        var value = buffer[^offset];
                """,
            ["buffer[buffer.count - offset]"],
            [],
            []),
        Success(
            "index.from-start-conversion",
            "form=index;bound=from-start;operand=explicit-Index-conversion;access=read",
            """
                        var buffer = new LengthBuffer();
                        var value = buffer[(Index)NextOffset()];
                """,
            ["buffer[ImplicitIndexerProtocolScenarios.nextOffset()]"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()"],
            []),
        Success(
            "index.complex-receiver",
            "form=index;bound=from-end;receiver=invocation;operand=invocation;evaluation=single",
            """
                        var value = GetLengthBuffer()[^NextOffset()];
                """,
            [".length - ImplicitIndexerProtocolScenarios.nextOffset()"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()"]),
        Success(
            "index.assignment",
            "form=index;bound=from-end;receiver=invocation;access=assignment;evaluation=left-to-right",
            """
                        GetLengthBuffer()[^NextOffset()] = NextValue();
                """,
            ["] = ImplicitIndexerProtocolScenarios.nextValue()"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.nextValue()"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.nextValue()"]),
        Success(
            "index.compound-assignment",
            "form=index;bound=from-end;receiver=invocation;access=compound-add;evaluation=single",
            """
                        GetLengthBuffer()[^NextOffset()] += NextValue();
                """,
            [" + ImplicitIndexerProtocolScenarios.nextValue()"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.nextValue()"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.nextValue()"]),
        Success(
            "index.postincrement",
            "form=index;bound=from-end;receiver=invocation;access=postincrement;result=discarded",
            """
                        GetLengthBuffer()[^NextOffset()]++;
                """,
            [" + 1"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()"],
            ["ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()"]),
        Success(
            "index.coalesce-assignment",
            "form=index;bound=from-end;receiver=invocation;access=coalesce-assignment;evaluation=single;fallback=computed-write",
            """
                        var value = GetNullableLengthBuffer()[^NextOffset()] ??= NextText();
                """,
            [" == null ?", "v$0[v$1] = v$2"],
            ["ImplicitIndexerProtocolScenarios.getNullableLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.nextText()"],
            ["ImplicitIndexerProtocolScenarios.getNullableLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.nextText()"]),
        Success(
            "range.bounded",
            "form=range;bounds=from-start/from-end;slice=Slice(int,int);receiver=local",
            """
                        var buffer = new SliceBuffer();
                        var middle = buffer[1..^1];
                """,
            ["buffer.slice(1, buffer.length - 1 - 1)"],
            [],
            []),
        Success(
            "range.open-end",
            "form=range;bounds=from-start/open;slice=Slice(int,int);receiver=local",
            """
                        var buffer = new SliceBuffer();
                        var tail = buffer[2..];
                """,
            ["buffer.slice(2, buffer.length - 2)"],
            [],
            []),
        Success(
            "range.all-complex-receiver",
            "form=range;bounds=open/open;slice=Slice(int,int);receiver=invocation;evaluation=single",
            """
                        var copy = GetSliceBuffer()[..];
                """,
            [".slice(0,", ".length)"],
            ["ImplicitIndexerProtocolScenarios.getSliceBuffer()"],
            []),
        Success(
            "range.from-end-bounds",
            "form=range;bounds=from-end/from-end;slice=Slice(int,int);receiver=local",
            """
                        var buffer = new SliceBuffer();
                        var middle = buffer[^3..^1];
                """,
            ["buffer.slice(buffer.length - 3, buffer.length - 1 - (buffer.length - 3))"],
            [],
            []),
        Success(
            "index.standalone-from-start-value",
            "form=index;operand=System.Index.FromStart;access=read;offset=mapped-carrier",
            """
                        var buffer = new LengthBuffer();
                        Index index = Index.FromStart(NextOffset());
                        var value = buffer[index];
                """,
            ["_1b0e1c2ab6c4cd39(ImplicitIndexerProtocolScenarios.nextOffset())", "buffer[_9b817e75f3f8f58f(index, buffer.length)]"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()"],
            []),
        Success(
            "index.standalone-value",
            "form=index;operand=System.Index-local;access=read;offset=mapped-carrier",
            """
                        var buffer = new LengthBuffer();
                        Index index = ^1;
                        var value = buffer[index];
                """,
            ["_ce8b9229a41c8545(1)", "buffer[_9b817e75f3f8f58f(index, buffer.length)]"],
            [],
            []),
        Success(
            "index.standalone-assignment",
            "form=index;operand=System.Index-local;access=assignment;offset=mapped-carrier",
            """
                        Index index = ^NextOffset();
                        GetLengthBuffer()[index] = NextValue();
                """,
            ["_ce8b9229a41c8545(ImplicitIndexerProtocolScenarios.nextOffset())", "_9b817e75f3f8f58f(index,"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextValue()"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextValue()"]),
        Success(
            "index.standalone-compound-assignment",
            "form=index;operand=System.Index-local;access=compound-add;evaluation=single;offset=mapped-carrier",
            """
                        Index index = ^NextOffset();
                        GetLengthBuffer()[index] += NextValue();
                """,
            ["_ce8b9229a41c8545(ImplicitIndexerProtocolScenarios.nextOffset())", "_9b817e75f3f8f58f(index,"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextValue()"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.getLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextValue()"]),
        Success(
            "index.standalone-coalesce-assignment",
            "form=index;operand=System.Index-local;access=coalesce-assignment;evaluation=single;offset=mapped-carrier",
            """
                        Index index = ^NextOffset();
                        var value = GetNullableLengthBuffer()[index] ??= NextText();
                """,
            ["_ce8b9229a41c8545(ImplicitIndexerProtocolScenarios.nextOffset())", "_9b817e75f3f8f58f(index,"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.getNullableLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextText()"],
            ["ImplicitIndexerProtocolScenarios.nextOffset()", "ImplicitIndexerProtocolScenarios.getNullableLengthBuffer()", "ImplicitIndexerProtocolScenarios.nextText()"]),
        Success(
            "range.standalone-value",
            "form=range;operand=System.Range-local;access=slice;offset-length=mapped-carrier",
            """
                        var buffer = new SliceBuffer();
                        Range range = 1..^1;
                        var value = buffer[range];
                """,
            ["_fc3dfc5dbaa397eb", "_1c7a1e658ed790ff(range, buffer.length)", ".offset", ".length"],
            ["_1c7a1e658ed790ff(range, buffer.length)"],
            [])
    ];

    private static ImplicitIndexerProtocolCase Success(
        string id,
        string dimension,
        string body,
        IReadOnlyList<string> expectedFragments,
        IReadOnlyList<string> singleOccurrenceFragments,
        IReadOnlyList<string> orderedFragments)
        => new(
            $"implicit-indexer.{id}",
            dimension,
            body,
            expectedFragments,
            singleOccurrenceFragments,
            orderedFragments);

}
