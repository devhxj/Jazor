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
public sealed class SemanticWalkerEnumerableAverageTests
{
    [TestMethod]
    public void Visit_EnumerableAverage_UsesExactNumericBoundImports()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableAverageScenarios
            {
                public static void Evaluate(int[] integers, long[] int64s, float[] singles, double[] doubles, decimal[] decimals)
                {
                    var integer = integers.Average();
                    var int64 = int64s.Average();
                    var single = singles.Average();
                    var number = doubles.Average();
                    var decimalValue = decimals.Average();
                }
            }
            """);

        var expectedMembers = new[]
        {
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)",
            "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)"
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
        CollectionAssert.AreEquivalent(new[] { "averageInt", "averageInt64", "averageSingle", "averageDouble", "averageDecimal" }, importNames);
        StringAssert.Contains(body, "averageInt(integers)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageInt64(int64s)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageSingle(singles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageDouble(doubles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "averageDecimal(decimals)", StringComparison.Ordinal);

        _ = new Parser().ParseModule("import { " + string.Join(", ", importNames) + " } from \"System/Linq/EnumerableModule.js\";\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableAverageScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
