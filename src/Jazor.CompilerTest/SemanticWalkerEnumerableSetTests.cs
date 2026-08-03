using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerEnumerableSetTests
{
    [TestMethod]
    public void Visit_EnumerableSetOperators_UseBoundImportsAndPreserveCallShape()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableSetScenarios
            {
                public static bool Evaluate(int[] first, int[] second)
                {
                    var distinct = first.Distinct();
                    var union = first.Union(second);
                    var except = first.Except(second);
                    var intersect = first.Intersect(second);
                    return distinct.Contains(2) && union.Contains(3) && except.Contains(1) && intersect.Contains(2);
                }

                public static bool SpanContains(int[] values)
                {
                    return System.MemoryExtensions.Contains((System.ReadOnlySpan<int>)values, 2);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        var enumerableImports = imports.Single(static pair => pair.Key == "System/Linq/EnumerableModule.js").Value
            .Select(static specifier => specifier.ToECMAScript())
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[] { "_a2bc38786226403e", "_b5fae0c231974056", "_c71d4ff9a863431d", "_d83c9e4a7bf747a8", "_e94a7db8306f4e71" },
            enumerableImports);
        foreach (var exportName in enumerableImports)
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);

        var spanBlock = GetBlockOperation(
            """
            public static class MemoryExtensionsScenario
            {
                public static bool SpanContains(int[] values)
                {
                    return System.MemoryExtensions.Contains((System.ReadOnlySpan<int>)values, 2);
                }
            }
            """,
            "SpanContains");
        var spanArgument = new SenseArgument(UseImportAliases: true);
        var spanBody = new SemanticWalker(true).Visit(spanBlock, spanArgument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");
        Assert.IsNotNull(spanBody);
        var spanImports = spanArgument.FlushImportSpecifiers().ToArray();
        var memoryExtensionsImport = spanImports.Single(static pair => pair.Key == "System/MemoryExtensionsModule.js").Value
            .Single()
            .ToECMAScript();
        Assert.AreEqual("_a4ed2b50c69946de", memoryExtensionsImport);
        StringAssert.Contains(spanBody, memoryExtensionsImport + "(", StringComparison.Ordinal);

        var spanModuleImports = spanImports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", spanModuleImports.Append("function verify() " + spanBody)));

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source, string methodName = "Evaluate")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableSetScenarios",
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
