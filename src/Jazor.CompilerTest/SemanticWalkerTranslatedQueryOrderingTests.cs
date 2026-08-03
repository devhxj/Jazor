using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTranslatedQueryOrderingTests
{
    [TestMethod]
    public void Visit_TranslatedQuery_WhereOrderedProjectionAndTake_UsesBoundEnumerableCalls()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class QueryOrderingScenarios
            {
                public static int[] SelectTopPriorities(int[] values)
                {
                    return (from value in values
                            where value > 0
                            orderby value % 3 descending, value
                            select value * 2).Take(2).ToArray();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers()
            .ToArray();
        var module = "function verify() " + body;

        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value
            .Select(static specifier => specifier.ToECMAScript())
            .ToArray();
        CollectionAssert.Contains(importNames, "_c955435630a10962");
        CollectionAssert.Contains(importNames, "_b9eeb5472648105d");
        CollectionAssert.Contains(importNames, "_4abc4f56a4100834");
        StringAssert.Contains(body, "_c955435630a10962(");
        StringAssert.Contains(body, "_b9eeb5472648105d(");
        StringAssert.Contains(body, "_4abc4f56a4100834(");
        StringAssert.Contains(body, "return __src.filter(__callback);");
        StringAssert.Contains(body, "return Array.from(__src).map(__callback);");
        StringAssert.Contains(body, "return Array.from(__src);");
        StringAssert.Contains(body, "return value > 0;");
        StringAssert.Contains(body, "return value % 3;");
        StringAssert.Contains(body, "return value * 2;");
        Assert.IsFalse(body.Contains("TranslatedQuery", StringComparison.Ordinal), body);

        _ = new Parser().ParseModule(module);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TranslatedQueryOrderingScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "SelectTopPriorities");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
