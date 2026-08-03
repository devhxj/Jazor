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
public sealed class SemanticWalkerEnumerableSequenceEqualTests
{
    [TestMethod]
    public void Visit_EnumerableSequenceEqual_UsesBoundImportAndPreservesSequenceOperands()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableSequenceEqualScenarios
            {
                public static bool Evaluate(int[] expectedReleaseIds, int[] actualReleaseIds)
                {
                    return expectedReleaseIds.SequenceEqual(actualReleaseIds);
                }
            }
            """);

        var invocation = block.Descendants().OfType<IInvocationOperation>().Single();
        var staticKey = (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
            .OriginalDefinition
            .ToDisplayString(Format.StaticExtensionNameFormat);
        Assert.AreEqual(
            "static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)",
            staticKey);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importName = Assert.IsInstanceOfType<string>(imports[0].Value.Single().ToECMAScript());
        StringAssert.Contains(importName, "sequenceEqual", StringComparison.Ordinal);
        StringAssert.Contains(body, importName + "(expectedReleaseIds, actualReleaseIds)", StringComparison.Ordinal);

        var moduleImports = "import { " + importName + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImports + "\nfunction verify() " + body);
    }

    [TestMethod]
    public void Visit_ArraySequenceEqual_UsesBoundReadOnlySpanImport()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class ArraySequenceEqualScenarios
            {
                public static bool Evaluate(int[] expectedReleaseIds, int[] actualReleaseIds)
                {
                    return expectedReleaseIds.SequenceEqual(actualReleaseIds);
                }
            }
            """);

        var invocation = block.Descendants().OfType<IInvocationOperation>().Single();
        Assert.AreEqual(
            "System.ReadOnlySpan<T>.SequenceEqual<T>(System.ReadOnlySpan<T>)",
            invocation.TargetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/MemoryExtensionsModule.js", imports[0].Key);
        var importName = Assert.IsInstanceOfType<string>(imports[0].Value.Single().ToECMAScript());
        StringAssert.Contains(importName, "sequenceEqual", StringComparison.Ordinal);
        StringAssert.Contains(body, importName + "(expectedReleaseIds, actualReleaseIds)", StringComparison.Ordinal);

        var moduleImports = "import { " + importName + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImports + "\nfunction verify() " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableSequenceEqualScenarios",
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
