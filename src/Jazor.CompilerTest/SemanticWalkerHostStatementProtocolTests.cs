using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerHostStatementProtocolTests
{
    [TestMethod]
    public void Host_SkipsOnlyAnEntireGeneratedLocalDeclarationGroup()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    int generated = 1;
                    int retained = 2;
                }
            }
            """);

        var script = new SemanticWalker(true)
        {
            Host = new GeneratedDeclarationHost()
        }.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("generated", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "let retained = 2;", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Host_DropsOnlyTheRequestedDeclaratorFromASharedDeclaration()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    int generated = 1, retained = 2;
                }
            }
            """);

        var script = new SemanticWalker(true)
        {
            Host = new GeneratedDeclarationHost()
        }.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("generated", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "let retained = 2;", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Host_SkipsGeneratedLocalFunctionsBeforeStatementLowering()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    int Generated() => 1;
                    int Retained() => 2;
                    int result = Retained();
                }
            }
            """);

        var script = new SemanticWalker(true)
        {
            Host = new GeneratedDeclarationHost()
        }.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("Generated", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function Retained()", StringComparison.Ordinal);
        StringAssert.Contains(script, "let result = Retained();", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerHostStatementProtocolTests",
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

    private sealed class GeneratedDeclarationHost : SemanticWalkerHost
    {
        public override bool ShouldSkipVariableDeclarator(IVariableDeclaratorOperation operation, SenseArgument argument)
            => operation.Symbol.Name == "generated";

        public override bool ShouldSkipLocalFunctionDeclaration(ILocalFunctionOperation operation, SenseArgument argument)
            => operation.Symbol.Name == "Generated";
    }
}
