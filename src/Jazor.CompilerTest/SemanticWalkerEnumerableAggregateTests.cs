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
public sealed class SemanticWalkerEnumerableAggregateTests
{
    [TestMethod]
    public void Visit_EnumerableAggregateOverloads_UseBoundImportsAndPreserveLambdaRoles()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableAggregateScenarios
            {
                public static int Evaluate(int[] releaseIds, int seed)
                {
                    var fromFirst = releaseIds.Aggregate((total, releaseId) => total + releaseId);
                    var fromSeed = releaseIds.Aggregate(seed, (total, releaseId) => total + releaseId);
                    return releaseIds.Aggregate(seed, (total, releaseId) => total + releaseId, total => total * 2) + fromFirst + fromSeed;
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Where(static invocation => invocation.TargetMethod.Name == "Aggregate")
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
                .OriginalDefinition
                .ToDisplayString(Format.StaticExtensionNameFormat))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "static System.Linq.Enumerable.Aggregate<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TSource, TSource>)",
                "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>)",
                "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate, TResult>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Func<TAccumulate, TResult>)"
            },
            staticKeys);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        Assert.HasCount(3, importNames, body);
        foreach (var importName in importNames)
            StringAssert.Contains(body, importName + "(", StringComparison.Ordinal);
        StringAssert.Contains(body, "return total + releaseId;", StringComparison.Ordinal);
        StringAssert.Contains(body, "return total * 2;", StringComparison.Ordinal);

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableAggregateScenarios",
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
