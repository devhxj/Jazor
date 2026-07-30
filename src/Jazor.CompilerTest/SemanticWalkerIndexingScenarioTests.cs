using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerIndexingScenarioTests
{
    public static IEnumerable<TestDataRow<IndexingLoweringScenario>> Cases
        => IndexingLoweringScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<IndexingLoweringScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsAndDimensions()
    {
        var scenarios = IndexingLoweringScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("indexing-lowering.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(scenarios.All(static scenario => scenario.ExpectedJavaScriptFragments.Count > 0));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_IndexingScenario_ProducesDeterministicParsableJavaScript(IndexingLoweringScenario scenario)
    {
        var block = GetBlockOperation(scenario.Source);
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, scenario.Id);
        Assert.AreEqual(first, second, scenario.Id);
        foreach (var fragment in scenario.ExpectedJavaScriptFragments)
            StringAssert.Contains(first, fragment, scenario.Id);

        _ = new Parser().ParseScript(first);
    }

    private static IBlockOperation GetBlockOperation(string body)
    {
        var source = $$"""
            using System;
            using System.Collections.Generic;

            public sealed class IndexingScenarios
            {
                public void TestMethod()
                {
            {{body}}
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "IndexingLoweringScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}

public sealed record IndexingLoweringScenario(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedJavaScriptFragments);

internal static class IndexingLoweringScenarioCatalog
{
    public static IReadOnlyList<IndexingLoweringScenario> All { get; } =
    [
        Case("array.from-end.read", "array-from-end-read", """
                    int[] values = [1, 2, 3];
                    int last = values[^1];
            """, "values[values.length - 1]"),
        Case("array.from-end.dynamic-offset", "array-from-end-dynamic-offset", """
                    int[] values = [1, 2, 3];
                    int offset = 2;
                    int item = values[^offset];
            """, "values[values.length - offset]"),
        Case("array.range.fixed", "array-range-bounded", """
                    int[] values = [1, 2, 3, 4];
                    int[] middle = values[1..3];
            """, "values.slice(1, 3)"),
        Case("array.range.from-end", "array-range-end-from-end", """
                    int[] values = [1, 2, 3, 4];
                    int[] middle = values[1..^1];
            """, "values.slice(1, values.length - 1)"),
        Case("array.range.both-from-end", "array-range-both-from-end", """
                    int[] values = [1, 2, 3, 4, 5];
                    int[] middle = values[^4..^1];
            """, "values.slice(values.length - 4, values.length - 1)"),
        Case("array.range.open-start", "array-range-open-start", """
                    int[] values = [1, 2, 3];
                    int[] prefix = values[..2];
            """, "values.slice(0, 2)"),
        Case("array.range.open-end", "array-range-open-end", """
                    int[] values = [1, 2, 3];
                    int[] suffix = values[1..];
            """, "values.slice(1)"),
        Case("array.range.full", "array-range-full-copy", """
                    int[] values = [1, 2, 3];
                    int[] copy = values[..];
            """, "values.slice()"),
        Case("array.from-end.postfix", "array-from-end-postfix-mutation", """
                    int[] values = [1, 2, 3];
                    int previous = values[^1]++;
            """, "values.length - 1", "previous"),
        Case("array.from-end.prefix", "array-from-end-prefix-mutation", """
                    int[] values = [1, 2, 3];
                    int current = --values[^1];
            """, "values.length - 1", "current"),
        Case("array.from-end.compound", "array-from-end-compound-mutation", """
                    int[] values = [1, 2, 3];
                    values[^1] += 4;
            """, "values.length - 1", "+= 4"),
        Case("array.from-end.side-effecting-receiver", "array-from-end-single-evaluation", """
                    int[] values = [1, 2, 3];
                    Func<int[]> next = () => values;
                    int last = next()[^1];
            """, "next()", ".length - 1"),
        Case("jagged-array.from-end", "jagged-array-nested-from-end", """
                    int[][] values = [[1, 2], [3, 4]];
                    int last = values[^1][^1];
            """, "v$0 = values[values.length - 1]", "v$0[v$0.length - 1]"),
        Case("string.from-end", "string-from-end-indexer", """
                    string text = "release";
                    char last = text[^1];
            """, "text.length - 1"),
        Case("string.range", "string-range-indexer", """
                    string text = "release";
                    string middle = text[1..^1];
            """, "text", "length - 1"),
        Case("list.from-end", "list-from-end-indexer", """
                    List<int> values = [1, 2, 3];
                    int last = values[^1];
            """, "values", "length - 1"),
        Case("list.from-end.compound", "list-from-end-compound-mutation", """
                    List<int> values = [1, 2, 3];
                    values[^1] += 4;
            """, "values", "length - 1", "+ 4")
    ];

    private static IndexingLoweringScenario Case(string id, string dimension, string source, params string[] expectedJavaScriptFragments)
        => new($"indexing-lowering.{id}", dimension, source, expectedJavaScriptFragments);
}
