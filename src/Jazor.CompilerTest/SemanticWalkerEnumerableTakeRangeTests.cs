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
public sealed class SemanticWalkerEnumerableTakeRangeTests
{
    [TestMethod]
    public void Visit_EnumerableTakeRange_UsesBoundRangeImport()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class TakeRangeScenarios
            {
                public static void Evaluate(int[] values)
                {
                    var prefix = values.Take(..2).ToArray();
                    var middle = values.Take(1..^1).ToArray();
                    var suffix = values.Take(^2..).ToArray();
                    var all = values.Take(..).ToArray();
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod).OriginalDefinition.ToDisplayString(Format.StaticExtensionNameFormat))
            .Where(static key => key.Contains("System.Linq.Enumerable.Take", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[] { "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Range)" },
            staticKeys,
            string.Join(Environment.NewLine, staticKeys));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        var importsByModule = imports.ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.IsTrue(importsByModule.TryGetValue("System/Linq/EnumerableModule.js", out var enumerableImports), body);
        CollectionAssert.Contains(enumerableImports.Select(static specifier => specifier.ToECMAScript()).ToArray(), "takeRange");
        Assert.IsTrue(importsByModule.ContainsKey("System/RangeModule.js"), body);
        StringAssert.Contains(body, "takeRange(values", StringComparison.Ordinal);

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TakeRangeScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
