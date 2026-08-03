using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerExpressionTreeBoundaryTests
{
    [TestMethod]
    public void Visit_ExpressionTreeLambda_RejectsSymbolicRuntimeMaterialization()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Linq.Expressions;

            public static class ExpressionTreeScenarios
            {
                public static void Create()
                {
                    Expression<Func<int, int>> expression = value => value + 1;
                }
            }
            """);

        var conversion = block.Descendants().OfType<IConversionOperation>().Single();
        Assert.IsInstanceOfType<IAnonymousFunctionOperation>(conversion.Operand);
        Assert.AreEqual("System.Linq.Expressions.Expression<System.Func<int, int>>", conversion.Type?.ToDisplayString());

        var exception = Assert.Throws<OperationTransformationException>(() => new SemanticWalker(true).Visit(block, new SenseArgument()));
        Assert.AreEqual(OperationKind.Conversion, exception.Kind);
        StringAssert.Contains(exception.Message, "Expression tree lambda conversions are not supported");
    }

    [TestMethod]
    public void Visit_IQueryableQuery_RejectsExpressionTreeCallbacksBeforeQueryableInvocation()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class QueryableScenarios
            {
                public static IQueryable<int> FilterAndProject(IQueryable<int> source)
                {
                    return from value in source
                           where value > 0
                           select value * 2;
                }
            }
            """);

        Assert.IsTrue(block.Descendants().OfType<ITranslatedQueryOperation>().Any());
        Assert.HasCount(2, block.Descendants().OfType<IConversionOperation>());

        var exception = Assert.Throws<OperationTransformationException>(() => new SemanticWalker(true).Visit(block, new SenseArgument()));
        Assert.AreEqual(OperationKind.Conversion, exception.Kind);
        StringAssert.Contains(exception.Message, "IQueryable<T>");
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "ExpressionTreeBoundaryScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
