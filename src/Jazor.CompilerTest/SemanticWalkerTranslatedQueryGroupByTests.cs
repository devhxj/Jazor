using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTranslatedQueryGroupByTests
{
    [TestMethod]
    public void Visit_TranslatedQuery_GroupContinuation_UsesBoundGroupingCarrierAndEnumerableCalls()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class QueryGroupByScenarios
            {
                public static int[] SummarizeBuckets(int[] values)
                {
                    return (from value in values
                            group value * 10 by value % 2 into bucket
                            select bucket.Key * 100 + bucket.Count()).ToArray();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        var importsByModule = imports.ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.IsTrue(importsByModule.TryGetValue("System/Linq/EnumerableModule.js", out var enumerableImports), body);
        Assert.IsTrue(importsByModule.TryGetValue("System/Linq/GroupingT2Module.js", out var groupingImports), body);

        var enumerableNames = enumerableImports.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.Contains(enumerableNames, "_e62121525c074f74");
        CollectionAssert.Contains(enumerableNames, "_1cb3ec9a7fb8aaab");
        var groupingNames = groupingImports.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.Contains(groupingNames, "_44a1c9f2c4f246e9");

        StringAssert.Contains(body, "_e62121525c074f74(");
        StringAssert.Contains(body, "_44a1c9f2c4f246e9(");
        StringAssert.Contains(body, "_1cb3ec9a7fb8aaab(");
        StringAssert.Contains(body, "return value * 10;");
        StringAssert.Contains(body, "return value % 2;");
        Assert.IsFalse(body.Contains("TranslatedQuery", StringComparison.Ordinal), body);

        var moduleImports = imports
            .Select(static pair =>
                "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TranslatedQueryGroupByScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "SummarizeBuckets");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
