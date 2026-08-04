using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerNumericFormatIntrinsicTests
{
    [TestMethod]
    public void Visit_Int32AndUInt32HexFormats_UseTheBoundIntrinsicFallback()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod(int signed, uint unsigned)
                {
                    string upperSigned = signed.ToString("X");
                    string lowerSigned = signed.ToString("x");
                    string upperUnsigned = unsigned.ToString("X");
                    string lowerUnsigned = unsigned.ToString("x");
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "(signed >>> 0).toString(16).toUpperCase()", StringComparison.Ordinal);
        StringAssert.Contains(script, "(signed >>> 0).toString(16).toLowerCase()", StringComparison.Ordinal);
        StringAssert.Contains(script, "unsigned.toString(16).toUpperCase()", StringComparison.Ordinal);
        StringAssert.Contains(script, "unsigned.toString(16).toLowerCase()", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerNumericFormatIntrinsicTests",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
