using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTranslatedQueryJoinTests
{
    [TestMethod]
    public void Visit_TranslatedQuery_JoinAndGroupJoinUseBoundEnumerableCalls()
    {
        var source =
            """
            using System.Linq;

            public static class QueryJoinScenarios
            {
                public static int[] JoinByParity(int[] outerValues, int[] innerValues)
                {
                    return (from outer in outerValues
                            join inner in innerValues on outer % 2 equals inner % 2
                            select outer * 100 + inner).ToArray();
                }

                public static int[] GroupJoinByParity(int[] outerValues, int[] innerValues)
                {
                    return (from outer in outerValues
                            join inner in innerValues on outer % 2 equals inner % 2 into matches
                            select outer * 10 + matches.Count()).ToArray();
                }
            }
            """;

        AssertBodyUsesImports(
            GetBlockOperation(source, "JoinByParity"),
            ["_f10104b4c52b4f96"],
            ["return outer % 2;", "return inner % 2;", "return outer * 100 + inner;"]);
        AssertBodyUsesImports(
            GetBlockOperation(source, "GroupJoinByParity"),
            ["_b61f41d1ac124b69", "_1cb3ec9a7fb8aaab"],
            ["return outer % 2;", "return inner % 2;", "return outer * 10 + _1cb3ec9a7fb8aaab(matches);"]);
    }

    private static void AssertBodyUsesImports(
        IBlockOperation block,
        IReadOnlyList<string> expectedEnumerableImports,
        IReadOnlyList<string> expectedBodyFragments)
    {
        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        var enumerableImport = imports.Single(static pair => pair.Key == "System/Linq/EnumerableModule.js");
        var names = enumerableImport.Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        foreach (var expectedImport in expectedEnumerableImports)
            CollectionAssert.Contains(names, expectedImport);
        foreach (var expectedBodyFragment in expectedBodyFragments)
            StringAssert.Contains(body, expectedBodyFragment);

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source, string methodName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TranslatedQueryJoinScenarios",
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
            .Single(candidate => candidate.Identifier.ValueText == methodName);
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
