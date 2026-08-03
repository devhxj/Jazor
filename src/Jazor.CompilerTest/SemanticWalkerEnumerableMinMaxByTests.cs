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
public sealed class SemanticWalkerEnumerableMinMaxByTests
{
    [TestMethod]
    public void Visit_EnumerableMinByAndMaxBy_UseBoundImportsAndSelectorLambdas()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableMinMaxByScenarios
            {
                public static int Evaluate(int[] releaseIds, bool maximum)
                {
                    return maximum
                        ? releaseIds.MaxBy(releaseId => releaseId % 10)
                        : releaseIds.MinBy(releaseId => releaseId % 10);
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
                .OriginalDefinition
                .ToDisplayString(Format.StaticExtensionNameFormat))
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)",
                "static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)"
            },
            staticKeys,
            string.Join(Environment.NewLine, staticKeys));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.AreEquivalent(new[] { "minBy", "maxBy" }, importNames);
        StringAssert.Contains(body, "minBy(releaseIds, releaseId => {", StringComparison.Ordinal);
        StringAssert.Contains(body, "maxBy(releaseIds, releaseId => {", StringComparison.Ordinal);
        StringAssert.Contains(body, "return releaseId % 10;", StringComparison.Ordinal);

        var moduleImports = "import { " + string.Join(", ", importNames) + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImports + "\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableMinMaxByScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
