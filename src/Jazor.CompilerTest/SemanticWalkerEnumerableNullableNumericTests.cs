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
public sealed class SemanticWalkerEnumerableNullableNumericTests
{
    [TestMethod]
    public void Visit_EnumerableNullableNumericTerminals_UsesExactBoundImports()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class NullableNumericScenarios
            {
                public static void Evaluate(int?[] integers, long?[] int64s, float?[] singles, double?[] doubles, decimal?[] decimals)
                {
                    var sumInteger = integers.Sum();
                    var sumInt64 = int64s.Sum();
                    var sumSingle = singles.Sum();
                    var sumDouble = doubles.Sum();
                    var sumDecimal = decimals.Sum();
                    var averageInteger = integers.Average();
                    var averageInt64 = int64s.Average();
                    var averageSingle = singles.Average();
                    var averageDouble = doubles.Average();
                    var averageDecimal = decimals.Average();
                }
            }
            """);

        var expectedMembers = new[]
        {
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float?>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double?>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int?>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float?>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double?>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)"
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
                "sumNullableInt",
                "sumNullableInt64",
                "sumNullableSingle",
                "sumNullableDouble",
                "sumNullableDecimal",
                "averageNullableInt",
                "averageNullableInt64",
                "averageNullableSingle",
                "averageNullableDouble",
                "averageNullableDecimal"
            },
            importNames);
        StringAssert.Contains(body, "sumNullableInt(integers)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumNullableInt64(int64s)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumNullableSingle(singles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumNullableDouble(doubles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumNullableDecimal(decimals)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageNullableInt(integers)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageNullableInt64(int64s)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageNullableSingle(singles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageNullableDouble(doubles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageNullableDecimal(decimals)", StringComparison.Ordinal);

        _ = new Parser().ParseModule("import { " + string.Join(", ", importNames) + " } from \"System/Linq/EnumerableModule.js\";\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "NullableNumericScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
