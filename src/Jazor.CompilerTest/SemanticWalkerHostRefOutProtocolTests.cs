using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerHostRefOutProtocolTests
{
    [TestMethod]
    public void RewriteInvocationArgumentPreorder_PreservesRefOutWriteBackProtocol()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    int source = 1;
                    int result = ReadAndCopy(ref source, out var copied);
                    WriteAndCopy(ref source, out var finalCopy);
                }

                int ReadAndCopy(ref int value, out int copy)
                {
                    copy = value;
                    return value;
                }

                void WriteAndCopy(ref int value, out int copy)
                {
                    copy = value;
                }
            }
            """);
        var host = new RefOutProjectionHost();
        var script = new SemanticWalker(true)
        {
            Host = host
        }.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "this.ReadAndCopy(hostRef, undefined)", StringComparison.Ordinal);
        StringAssert.Contains(script, "this.WriteAndCopy(hostRef, undefined)", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostRef = ", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostOut = ", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostFinalOut = ", StringComparison.Ordinal);
        Assert.AreEqual(4, host.RewriteCount);
        _ = new Parser().ParseScript(script);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerHostRefOutProtocolTests",
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

    private sealed class RefOutProjectionHost : SemanticWalkerHost
    {
        public int RewriteCount { get; private set; }

        public override Acornima.Ast.Expression? RewriteInvocationArgumentPreorder(
            IInvocationOperation operation,
            IArgumentOperation argumentOperation,
            int argumentIndex,
            SenseArgument argument)
        {
            if (operation.TargetMethod.Name is not ("ReadAndCopy" or "WriteAndCopy"))
                return null;

            RewriteCount++;
            return (operation.TargetMethod.Name, argumentIndex) switch
            {
                ("ReadAndCopy", 0) => new Acornima.Ast.Identifier("hostRef"),
                ("ReadAndCopy", 1) => new Acornima.Ast.Identifier("hostOut"),
                ("WriteAndCopy", 0) => new Acornima.Ast.Identifier("hostRef"),
                ("WriteAndCopy", 1) => new Acornima.Ast.Identifier("hostFinalOut"),
                _ => null
            };
        }
    }
}
