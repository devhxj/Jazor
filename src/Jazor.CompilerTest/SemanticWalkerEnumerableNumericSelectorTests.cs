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
public sealed class SemanticWalkerEnumerableNumericSelectorTests
{
    [TestMethod]
    public void Visit_EnumerableNumericSelectorTerminals_UseExactBoundImports()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableNumericSelectorScenarios
            {
                public sealed class Entry
                {
                    public int Int32;
                    public long Int64;
                    public float Single;
                    public double Double;
                    public decimal Decimal;
                }

                public static void Evaluate(Entry[] entries)
                {
                    var sumInt = entries.Sum(entry => entry.Int32);
                    var sumInt64 = entries.Sum(entry => entry.Int64);
                    var sumSingle = entries.Sum(entry => entry.Single);
                    var sumDouble = entries.Sum(entry => entry.Double);
                    var sumDecimal = entries.Sum(entry => entry.Decimal);
                    var averageInt = entries.Average(entry => entry.Int32);
                    var averageInt64 = entries.Average(entry => entry.Int64);
                    var averageSingle = entries.Average(entry => entry.Single);
                    var averageDouble = entries.Average(entry => entry.Double);
                    var averageDecimal = entries.Average(entry => entry.Decimal);
                    var minInt = entries.Min(entry => entry.Int32);
                    var minInt64 = entries.Min(entry => entry.Int64);
                    var minSingle = entries.Min(entry => entry.Single);
                    var minDouble = entries.Min(entry => entry.Double);
                    var minDecimal = entries.Min(entry => entry.Decimal);
                    var maxInt = entries.Max(entry => entry.Int32);
                    var maxInt64 = entries.Max(entry => entry.Int64);
                    var maxSingle = entries.Max(entry => entry.Single);
                    var maxDouble = entries.Max(entry => entry.Double);
                    var maxDecimal = entries.Max(entry => entry.Decimal);
                }
            }
            """);

        var expectedMembers = new[]
        {
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)",
            "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)",
            "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)",
            "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)",
            "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)"
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
                "sumIntBy", "sumInt64By", "sumSingleBy", "sumDoubleBy", "sumDecimalBy",
                "averageIntBy", "averageInt64By", "averageSingleBy", "averageDoubleBy", "averageDecimalBy",
                "minIntBy", "minInt64By", "minSingleBy", "minDoubleBy", "minDecimalBy",
                "maxIntBy", "maxInt64By", "maxSingleBy", "maxDoubleBy", "maxDecimalBy"
            },
            importNames);
        StringAssert.Contains(body, "sumIntBy(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumDecimalBy(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageIntBy(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageDecimalBy(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "minIntBy(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "minDecimalBy(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "maxIntBy(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "maxDecimalBy(entries", StringComparison.Ordinal);

        _ = new Parser().ParseModule("import { " + string.Join(", ", importNames) + " } from \"System/Linq/EnumerableModule.js\";\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableNumericSelectorScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
