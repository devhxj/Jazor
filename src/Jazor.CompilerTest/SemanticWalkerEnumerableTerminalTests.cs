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
public sealed class SemanticWalkerEnumerableTerminalTests
{
    [TestMethod]
    public void Visit_EnumerableFirstAndLast_UsesBoundImportsAndPreservesPredicateLambdas()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableTerminalScenarios
            {
                public static int Evaluate(int[] releaseIds, int threshold)
                {
                    var first = releaseIds.First();
                    var firstMatching = releaseIds.First(releaseId => releaseId > threshold);
                    var last = releaseIds.Last();
                    var lastMatching = releaseIds.Last(releaseId => releaseId > threshold);
                    return first + firstMatching + last + lastMatching;
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Where(static invocation => invocation.TargetMethod.Name is "First" or "Last")
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
                .OriginalDefinition
                .ToDisplayString(Format.StaticExtensionNameFormat))
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
                "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)",
                "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
                "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)"
            },
            staticKeys);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        Assert.HasCount(4, importNames, body);
        foreach (var importName in importNames)
            StringAssert.Contains(body, importName + "(", StringComparison.Ordinal);
        StringAssert.Contains(body, "return releaseId > threshold;", StringComparison.Ordinal);

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    [TestMethod]
    public void Visit_EnumerableSingle_UsesBoundImportsAndPreservesPredicateLambdas()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableSingleScenarios
            {
                public static int Evaluate(int[] releaseIds, int threshold)
                {
                    var single = releaseIds.Single();
                    var singleMatching = releaseIds.Single(releaseId => releaseId > threshold);
                    return single + singleMatching;
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Where(static invocation => invocation.TargetMethod.Name == "Single")
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
                .OriginalDefinition
                .ToDisplayString(Format.StaticExtensionNameFormat))
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
                "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)"
            },
            staticKeys);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        Assert.HasCount(2, importNames, body);
        foreach (var importName in importNames)
            StringAssert.Contains(body, importName + "(", StringComparison.Ordinal);
        StringAssert.Contains(body, "return releaseId > threshold;", StringComparison.Ordinal);

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableTerminalScenarios",
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
