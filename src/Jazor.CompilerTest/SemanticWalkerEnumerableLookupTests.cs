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
public sealed class SemanticWalkerEnumerableLookupTests
{
    [TestMethod]
    public void Visit_EnumerableToLookup_UsesBoundLookupCarrierMembers()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class LookupScenarios
            {
                public static void Evaluate(int[] values)
                {
                    var lookup = values.ToLookup(value => value % 2);
                    var count = lookup.Count;
                    var hasEven = lookup.Contains(0);
                    var evens = lookup[0].ToArray();
                    var projected = values.ToLookup(value => value % 2, value => value * 10);
                    var odds = projected[1].ToArray();
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod).OriginalDefinition.ToDisplayString(Format.StaticExtensionNameFormat))
            .Where(static key => key.Contains("System.Linq.Enumerable.ToLookup", StringComparison.Ordinal))
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)",
                "static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)"
            },
            staticKeys,
            string.Join(Environment.NewLine, staticKeys));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        var enumerableImports = imports.Single(static pair => pair.Key == "System/Linq/EnumerableModule.js").Value
            .Select(static specifier => specifier.ToECMAScript())
            .ToArray();
        CollectionAssert.Contains(enumerableImports, "toLookup");
        CollectionAssert.Contains(enumerableImports, "toLookupElement");
        CollectionAssert.Contains(enumerableImports, "lookupCount");
        CollectionAssert.Contains(enumerableImports, "lookupContains");
        CollectionAssert.Contains(enumerableImports, "lookupGet");
        StringAssert.Contains(body, "toLookup(values", StringComparison.Ordinal);
        StringAssert.Contains(body, "toLookupElement(values", StringComparison.Ordinal);
        StringAssert.Contains(body, "lookupCount(lookup)", StringComparison.Ordinal);
        StringAssert.Contains(body, "lookupContains(lookup, 0)", StringComparison.Ordinal);
        StringAssert.Contains(body, "lookupGet(lookup, 0)", StringComparison.Ordinal);
        StringAssert.Contains(body, "lookupGet(projected, 1)", StringComparison.Ordinal);

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "LookupScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
