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
public sealed class SemanticWalkerEnumerableNullableNumericSelectorTests
{
    [TestMethod]
    public void Visit_EnumerableNullableNumericSelectors_UsesExactBoundImports()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class NullableNumericSelectorScenarios
            {
                public static void Evaluate(int[] values)
                {
                    var sumInteger = values.Sum(value => value % 2 == 0 ? value : null);
                    var sumInt64 = values.Sum(value => value % 2 == 0 ? (long)value : null);
                    var sumSingle = values.Sum(value => value % 2 == 0 ? (float)value : null);
                    var sumDouble = values.Sum(value => value % 2 == 0 ? (double)value : null);
                    var sumDecimal = values.Sum(value => value % 2 == 0 ? (decimal)value : null);
                    var averageInteger = values.Average(value => value % 2 == 0 ? value : null);
                    var averageInt64 = values.Average(value => value % 2 == 0 ? (long)value : null);
                    var averageSingle = values.Average(value => value % 2 == 0 ? (float)value : null);
                    var averageDouble = values.Average(value => value % 2 == 0 ? (double)value : null);
                    var averageDecimal = values.Average(value => value % 2 == 0 ? (decimal)value : null);
                    var minInteger = values.Min(value => value % 2 == 0 ? value : null);
                    var minInt64 = values.Min(value => value % 2 == 0 ? (long)value : null);
                    var minSingle = values.Min(value => value % 2 == 0 ? (float)value : null);
                    var minDouble = values.Min(value => value % 2 == 0 ? (double)value : null);
                    var minDecimal = values.Min(value => value % 2 == 0 ? (decimal)value : null);
                    var maxInteger = values.Max(value => value % 2 == 0 ? value : null);
                    var maxInt64 = values.Max(value => value % 2 == 0 ? (long)value : null);
                    var maxSingle = values.Max(value => value % 2 == 0 ? (float)value : null);
                    var maxDouble = values.Max(value => value % 2 == 0 ? (double)value : null);
                    var maxDecimal = values.Max(value => value % 2 == 0 ? (decimal)value : null);
                }
            }
            """);

        var expectedMembers = new[]
        {
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)"
        };
        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod).OriginalDefinition.ToDisplayString(Format.StaticExtensionNameFormat))
            .ToArray();
        CollectionAssert.AreEquivalent(expectedMembers, staticKeys, string.Join(Environment.NewLine, staticKeys));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "sumNullableIntBy",
                "sumNullableInt64By",
                "sumNullableSingleBy",
                "sumNullableDoubleBy",
                "sumNullableDecimalBy",
                "averageNullableIntBy",
                "averageNullableInt64By",
                "averageNullableSingleBy",
                "averageNullableDoubleBy",
                "averageNullableDecimalBy",
                "minNullableIntBy",
                "minNullableInt64By",
                "minNullableSingleBy",
                "minNullableDoubleBy",
                "minNullableDecimalBy",
                "maxNullableIntBy",
                "maxNullableInt64By",
                "maxNullableSingleBy",
                "maxNullableDoubleBy",
                "maxNullableDecimalBy"
            },
            importNames);
        StringAssert.Contains(body, "sumNullableIntBy(values", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageNullableInt64By(values", StringComparison.Ordinal);
        StringAssert.Contains(body, "minNullableSingleBy(values", StringComparison.Ordinal);
        StringAssert.Contains(body, "maxNullableDoubleBy(values", StringComparison.Ordinal);
        StringAssert.Contains(body, "maxNullableDecimalBy(values", StringComparison.Ordinal);

        _ = new Parser().ParseModule("import { " + string.Join(", ", importNames) + " } from \"System/Linq/EnumerableModule.js\";\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "NullableNumericSelectorScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
