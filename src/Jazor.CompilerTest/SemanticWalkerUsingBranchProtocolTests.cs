using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerUsingBranchProtocolTests
{
    [TestMethod]
    public void VisitUsing_TypeParameterDisposable_UsesTheBoundConstraintContract()
    {
        var block = GetBlockOperation(
            """
            using System;

            class TestClass
            {
                void TestMethod<T>(T resource) where T : IDisposable
                {
                    using (resource)
                    {
                        Console.WriteLine("active");
                    }
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "try {", StringComparison.Ordinal);
        StringAssert.Contains(script, "_6f97d94b6f2e4bc1(resource);", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void VisitUsing_TypeParameterAsyncDisposable_AwaitsTheBoundConstraintContract()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Threading.Tasks;

            class TestClass
            {
                async Task TestMethod<T>(T resource) where T : IAsyncDisposable
                {
                    await using (resource)
                    {
                        Console.WriteLine("active");
                    }
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "await _d17f7fbf9eb14eef(resource);", StringComparison.Ordinal);
        _ = new Parser().ParseScript($"async function test() {script}");
    }

    [TestMethod]
    public void VisitUsingDeclaration_TupleDeconstructionBody_MaterializesTheWholeSequence()
    {
        var block = GetBlockOperation(
            """
            using System;

            class TestClass
            {
                sealed class Resource : IDisposable
                {
                    public void Dispose() { }
                }

                void TestMethod()
                {
                    using var resource = new Resource();
                    int left;
                    int right;
                    (left, right) = (1, 2);
                    Console.WriteLine(left + right);
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "left = 1, right = 2;", StringComparison.Ordinal);
        StringAssert.Contains(script, "console.log(left + right);", StringComparison.Ordinal);
        StringAssert.Contains(script, "resource.dispose();", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void VisitUsingDeclaration_DirectVisitor_RejectsMissingEnclosingLifetimeScope()
    {
        var block = GetBlockOperation(
            """
            using System;

            class TestClass
            {
                sealed class Resource : IDisposable
                {
                    public void Dispose() { }
                }

                void TestMethod()
                {
                    using var resource = new Resource();
                    Console.WriteLine("active");
                }
            }
            """);
        var usingDeclaration = block.Operations.OfType<IUsingDeclarationOperation>().Single();

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).VisitUsingDeclaration(usingDeclaration, new SenseArgument()));

        StringAssert.Contains(
            exception.Message,
            "must be lowered by the enclosing sequential statement translator",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void TranslateStatementSequence_HostOwnedDeclarationGroups_AreOmittedWithoutDroppingNeighbors()
    {
        var block = GetBlockOperation(
            """
            using System;

            class TestClass
            {
                void TestMethod()
                {
                    Type childType = typeof(string), parentType = typeof(object);
                    Console.WriteLine("ready");
                }
            }
            """);
        var group = block.Operations.OfType<IVariableDeclarationGroupOperation>().Single();
        var host = new TypeCarrierSkipHost();
        var walker = new SemanticWalker(true) { Host = host };

        var statements = walker.TranslateStatementSequence(block.Operations, new SenseArgument());
        var declarationStatements = walker.TranslateStatementSequence(group.Declarations, new SenseArgument());
        var script = new FunctionBody(NodeList.From(statements), strict: false).ToKnRECMAScript();

        Assert.AreEqual(0, declarationStatements.Count);
        Assert.HasCount(2, host.ClaimedSymbols);
        Assert.IsFalse(script.Contains("childType", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("parentType", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "console.log(\"ready\");", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerUsingBranchProtocolTests",
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

    private sealed class TypeCarrierSkipHost : SemanticWalkerHost
    {
        public HashSet<string> ClaimedSymbols { get; } = [];

        public override bool ShouldSkipVariableDeclarator(
            IVariableDeclaratorOperation operation,
            SenseArgument argument)
        {
            if (operation.Symbol.Type.ToDisplayString() != "System.Type")
                return false;

            ClaimedSymbols.Add(operation.Symbol.Name);
            return true;
        }
    }
}
