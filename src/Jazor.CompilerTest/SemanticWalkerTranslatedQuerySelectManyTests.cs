using Acornima;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTranslatedQuerySelectManyTests
{
    [TestMethod]
    public void Visit_TranslatedQuery_MultipleFromClauses_UsesBoundSelectManyResultSelector()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class QuerySelectManyScenarios
            {
                public static int[] ExpandPositiveValues(int[] values)
                {
                    return (from outer in values
                            where outer > 0
                            from inner in new[] { outer, outer * 10 }
                            select outer + inner).ToArray();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);

        var importNames = imports[0].Value
            .Select(static specifier => specifier.ToECMAScript())
            .ToArray();
        Assert.IsTrue(
            importNames.Contains("_aacc82f5a0d854d2", StringComparer.Ordinal),
            $"Expected SelectMany result-selector import. Actual imports: {string.Join(", ", importNames)}{Environment.NewLine}{body}");
        StringAssert.Contains(body, "_aacc82f5a0d854d2(");
        StringAssert.Contains(body, "return outer > 0;");
        StringAssert.Contains(body, "return [outer, outer * 10];");
        StringAssert.Contains(body, "return outer + inner;");
        StringAssert.Contains(body, "return Array.from(__src);");
        Assert.IsFalse(body.Contains("TranslatedQuery", StringComparison.Ordinal), body);

        _ = new Parser().ParseModule("function verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TranslatedQuerySelectManyScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "ExpandPositiveValues");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
