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
public sealed class SemanticWalkerEnumerableReverseTests
{
    [TestMethod]
    public void Visit_EnumerableReverse_UsesBoundImportAndMaterializationContract()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableReverseScenarios
            {
                public static int[] ReverseVisibleReleaseIds(int[] releaseIds)
                {
                    return releaseIds.Reverse().ToArray();
                }
            }
            """);

        var reverseInvocation = block.Descendants()
            .OfType<IInvocationOperation>()
            .Single(static invocation => invocation.TargetMethod.Name == "Reverse");
        var reverseDeclarationType = (reverseInvocation.TargetMethod.ReducedFrom ?? reverseInvocation.TargetMethod)
            .ContainingType
            .ToDisplayString();
        Assert.AreEqual("System.Linq.Enumerable", reverseDeclarationType, reverseInvocation.TargetMethod.ToDisplayString());
        var reverseStaticKey = (reverseInvocation.TargetMethod.ReducedFrom ?? reverseInvocation.TargetMethod)
            .OriginalDefinition
            .ToDisplayString(Format.StaticExtensionNameFormat);
        Assert.AreEqual(
            "static System.Linq.Enumerable.Reverse<TSource>(TSource[])",
            reverseStaticKey);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.Contains(importNames, "reverseArray");
        StringAssert.Contains(body, "reverseArray(");
        StringAssert.Contains(body, "return Array.from(");

        var moduleImports = imports.Select(static pair =>
            "import { " + string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript())) + " } from \"" + pair.Key + "\";");
        _ = new Parser().ParseModule(string.Join("\n", moduleImports.Append("function verify() " + body)));
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableReverseScenarios",
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
            .Single(static candidate => candidate.Identifier.ValueText == "ReverseVisibleReleaseIds");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
