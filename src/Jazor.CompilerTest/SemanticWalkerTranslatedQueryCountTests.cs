using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTranslatedQueryCountTests
{
    [TestMethod]
    public void Visit_TranslatedQuery_FilterThenCount_UsesBoundCountImport()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class QueryCountScenarios
            {
                public static int CountReadyReleases(int[] releaseIds)
                {
                    return (from releaseId in releaseIds
                            where releaseId > 0
                            select releaseId).Count();
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
            importNames.Contains("_1cb3ec9a7fb8aaab", StringComparer.Ordinal),
            $"Expected Count import. Actual imports: {string.Join(", ", importNames)}{Environment.NewLine}{body}");
        StringAssert.Contains(body, "_1cb3ec9a7fb8aaab(");
        StringAssert.Contains(body, "return releaseId > 0;");
        Assert.IsFalse(body.Contains("TranslatedQuery", StringComparison.Ordinal), body);

        _ = new Parser().ParseModule("function verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TranslatedQueryCountScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "CountReadyReleases");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
