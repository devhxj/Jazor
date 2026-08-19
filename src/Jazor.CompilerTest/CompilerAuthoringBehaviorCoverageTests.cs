using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerAuthoringBehaviorCoverageTests
{
    [TestMethod]
    public void Visit_CurrentModuleAutoPropertyMutations_PreservesReadModifyWriteSemantics()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                public int Count { get; set; }

                void TestMethod()
                {
                    Count += 2;
                    Count++;
                    --Count;
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "Count", StringComparison.Ordinal);
        StringAssert.Contains(script, "+= 2", StringComparison.Ordinal);
        StringAssert.Contains(script, "++", StringComparison.Ordinal);
        StringAssert.Contains(script, "--", StringComparison.Ordinal);
    }

    private static string VisitBlock(IBlockOperation block)
    {
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second);
        _ = new Parser().ParseScript(first);
        return first;
    }

    private static IBlockOperation CreateBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerAuthoringBehavior_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
