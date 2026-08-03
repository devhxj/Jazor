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
public sealed class SemanticWalkerEnumerableSetByTests
{
    [TestMethod]
    public void Visit_EnumerableSetByOperators_UseBoundImportsAndPreserveSelectorPipelines()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableSetByScenarios
            {
                public static int[] Evaluate(int[] first, int[] second, int[] keys)
                {
                    return first
                        .UnionBy(second, value => value % 3)
                        .ExceptBy(keys, value => value % 3)
                        .IntersectBy(second, value => value % 2)
                        .ToArray();
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
                "static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)",
                "static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)",
                "static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)",
                "static System.Linq.Enumerable.ToArray<TSource>(System.Collections.Generic.IEnumerable<TSource>)"
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
        CollectionAssert.AreEquivalent(new[] { "unionBy", "exceptBy", "intersectBy" }, importNames);
        StringAssert.Contains(body, "unionBy(first, second, value =>", StringComparison.Ordinal);
        StringAssert.Contains(body, "exceptBy(", StringComparison.Ordinal);
        StringAssert.Contains(body, "intersectBy(", StringComparison.Ordinal);
        StringAssert.Contains(body, "return value % 3;", StringComparison.Ordinal);
        StringAssert.Contains(body, "return value % 2;", StringComparison.Ordinal);
        StringAssert.Contains(body, "Array.from(__src)", StringComparison.Ordinal);

        var moduleImports = "import { " + string.Join(", ", importNames) + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImports + "\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableSetByScenarios",
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
