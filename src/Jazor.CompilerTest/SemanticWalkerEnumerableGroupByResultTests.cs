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
public sealed class SemanticWalkerEnumerableGroupByResultTests
{
    [TestMethod]
    public void Visit_EnumerableGroupByResultSelectors_UseExactBoundImports()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class GroupByResultScenarios
            {
                public sealed class Entry
                {
                    public int Key;
                    public int Value;
                }

                public static void Evaluate(Entry[] entries)
                {
                    var sourceResult = entries.GroupBy(
                        entry => entry.Key,
                        (key, group) => key + group.Sum(entry => entry.Value)).ToArray();
                    var elementResult = entries.GroupBy(
                        entry => entry.Key,
                        entry => entry.Value,
                        (key, values) => key + values.Sum()).ToArray();
                }
            }
            """);

        var expectedMembers = new[]
        {
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>)",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>)"
        };
        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod).OriginalDefinition.ToDisplayString(Format.StaticExtensionNameFormat))
            .Where(static key => key.Contains("System.Linq.Enumerable.GroupBy", StringComparison.Ordinal))
            .ToArray();
        CollectionAssert.AreEquivalent(expectedMembers, staticKeys, string.Join(Environment.NewLine, staticKeys));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        var enumerableImports = imports.Single(static pair => pair.Key == "System/Linq/EnumerableModule.js").Value
            .Select(static specifier => specifier.ToECMAScript())
            .ToArray();
        CollectionAssert.Contains(enumerableImports, "groupByResult");
        CollectionAssert.Contains(enumerableImports, "groupByElementResult");
        CollectionAssert.Contains(enumerableImports, "sumInt");
        CollectionAssert.Contains(enumerableImports, "sumIntBy");
        StringAssert.Contains(body, "groupByResult(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "groupByElementResult(entries", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumIntBy(group", StringComparison.Ordinal);
        StringAssert.Contains(body, "sumInt(values", StringComparison.Ordinal);

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "GroupByResultScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
