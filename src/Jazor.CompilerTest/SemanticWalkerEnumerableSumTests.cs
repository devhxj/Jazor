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
public sealed class SemanticWalkerEnumerableSumTests
{
    [TestMethod]
    public void Visit_EnumerableSum_UsesExactNumericBoundImports()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableSumScenarios
            {
                public static void Evaluate(int[] integers, long[] int64s, float[] singles, double[] doubles, decimal[] decimals)
                {
                    var integer = integers.Sum();
                    var int64 = int64s.Sum();
                    var single = singles.Sum();
                    var number = doubles.Sum();
                    var decimalValue = decimals.Sum();
                }
            }
            """);

        var expectedMembers = new[]
        {
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double>)",
            "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)"
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
        CollectionAssert.AreEquivalent(new[] { "sumInt", "sumInt64", "sumSingle", "sumDouble", "sumDecimal" }, importNames);
        StringAssert.Contains(body, "sumInt(integers)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumInt64(int64s)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumSingle(singles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumDouble(doubles)", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumDecimal(decimals)", StringComparison.Ordinal);

        _ = new Parser().ParseModule("import { " + string.Join(", ", importNames) + " } from \"System/Linq/EnumerableModule.js\";\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableSumScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
