using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerUsageBoundaryTests
{
    [TestMethod]
    [DataRow("public void Test<T>() where T : new() { T result = new T(); }",
        "Type-parameter object creation")]
    [DataRow("private static int _value; private delegate ref int Reader(); public void Test() { Reader result = () => ref _value; }",
        "Source ref returns require a storage-location protocol")]
    public void Visit_UnsupportedClrUsage_ReportsSpecificTransformationFailure(string declaration, string expected)
    {
        var tree = CSharpSyntaxTree.ParseText("public class Fixture { " + declaration + " }",
            TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(nameof(CompilerUsageBoundaryTests), [tree],
            TestMetadataReferences.Net11, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        var block = Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(tree).GetOperation(method.Body!));
        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));
        StringAssert.Contains(exception.Message, expected);
    }
}
