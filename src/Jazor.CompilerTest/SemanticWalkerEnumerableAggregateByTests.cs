using Acornima;
using ECMAScript;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerEnumerableAggregateByTests
{
    [TestMethod]
    public void Visit_EnumerableAggregationBy_UsesBoundImportsAndTwoSlotKeyValuePairs()
    {
        var block = GetBlockOperation(
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class EnumerableAggregateByScenarios
            {
                public static int Evaluate(int[] values, IEqualityComparer<int> comparer)
                {
                    var counts = values.CountBy(value => value, comparer).ToArray();
                    var fixedSums = values.AggregateBy(value => value, 10, (sum, value) => sum + value, comparer).ToArray();
                    var keySums = values.AggregateBy(value => value, key => key * 2, (sum, value) => sum + value, comparer).ToArray();
                    var defaultCounts = values.CountBy(value => value).ToArray();
                    var defaultFixedSums = values.AggregateBy(value => value, 10, (sum, value) => sum + value).ToArray();
                    var defaultKeySums = values.AggregateBy(value => value, key => key * 2, (sum, value) => sum + value).ToArray();
                    var created = new KeyValuePair<int, int>(5, 6);
                    var total = counts[0].Key + counts[0].Value + fixedSums[0].Value + keySums[0].Value
                        + defaultCounts[0].Value + defaultFixedSums[0].Value + defaultKeySums[0].Value
                        + created.Key + created.Value;
                    foreach (var (key, value) in counts)
                        total += key + value;
                    return total;
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
                .OriginalDefinition
                .ToDisplayString(Format.StaticExtensionNameFormat))
            .Where(static key => key.Contains("Enumerable.CountBy", StringComparison.Ordinal) || key.Contains("Enumerable.AggregateBy", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "static System.Linq.Enumerable.CountBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
                "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)",
                "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, TAccumulate>, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)"
            },
            staticKeys,
            string.Join(Environment.NewLine, staticKeys));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.AreEquivalent(new[] { "countBy", "aggregateBy", "aggregateByWithSeedSelector" }, importNames);
        StringAssert.Contains(body, "counts[0][0]", StringComparison.Ordinal);
        StringAssert.Contains(body, "counts[0][1]", StringComparison.Ordinal);
        StringAssert.Contains(body, "fixedSums[0][1]", StringComparison.Ordinal);
        StringAssert.Contains(body, "keySums[0][1]", StringComparison.Ordinal);
        StringAssert.Contains(body, "defaultCounts[0][1]", StringComparison.Ordinal);
        StringAssert.Contains(body, "defaultFixedSums[0][1]", StringComparison.Ordinal);
        StringAssert.Contains(body, "defaultKeySums[0][1]", StringComparison.Ordinal);
        Assert.AreEqual(2, CountOccurrences(body, "countBy("), body);
        Assert.AreEqual(2, CountOccurrences(body, "aggregateBy("), body);
        Assert.AreEqual(2, CountOccurrences(body, "aggregateByWithSeedSelector("), body);
        StringAssert.Contains(body, "created = [5, 6]", StringComparison.Ordinal);
        StringAssert.Contains(body, "created[0]", StringComparison.Ordinal);
        StringAssert.Contains(body, "created[1]", StringComparison.Ordinal);
        StringAssert.Contains(body, "[key, value] of counts", StringComparison.Ordinal);

        var moduleImport = "import { " + string.Join(", ", importNames) + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImport + "\nexport function evaluate(values, comparer) " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableAggregateByScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    private static int CountOccurrences(string value, string fragment)
        => value.Split([fragment], StringSplitOptions.None).Length - 1;
}
