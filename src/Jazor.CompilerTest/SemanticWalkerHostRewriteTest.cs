using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerHostRewriteTest
{
    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesDeclarationPatternDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    object value = 42;
                    if (value is int props)
                    {
                        Console.WriteLine(props);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(__alias = value, true)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesRecursivePatternDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    object value = 42;
                    if (value is int { } props)
                    {
                        Console.WriteLine(props);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(__alias = value, true)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesListPatternDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    int[] value = [1, 2, 3];
                    if (value is [1, ..] props)
                    {
                        Console.WriteLine(props.Length);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(__alias = value, true)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias.length);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesOutDeclarationDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse("42", out var props))
                    {
                        Console.WriteLine(props);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias", StringComparison.Ordinal);
        StringAssert.Contains(script!, "__alias = ", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesOutDeclarationExpressionWithoutReferenceRewrite()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse("42", out var props))
                    {
                        Console.WriteLine("ok");
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new DeclarationOnlyAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(\"42\", __alias)", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("\"42\", props", StringComparison.Ordinal), script);
    }

    private static IBlockOperation GetBlockOperation(string code)
    {
        var usings =
            """
            global using System;
            global using ECMAScript;
            global using static ECMAScript.Global;
            """;

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings),
                CSharpSyntaxTree.ParseText(code)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException(string.Join("\n", errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(static method => method.Identifier.ValueText == "TestMethod");

        return semanticModel.GetOperation(methodDeclaration.Body!) as IBlockOperation
            ?? throw new InvalidOperationException("Method body operation was not available.");
    }

    private sealed class LocalAliasHost : SemanticWalkerHost
    {
        private readonly string _sourceName;
        private readonly string _alias;

        public LocalAliasHost(string sourceName, string alias)
        {
            _sourceName = sourceName;
            _alias = alias;
        }

        public override Identifier? RewriteLocalDeclarationIdentifier(ILocalSymbol local, IOperation operation, SenseArgument argument)
            => string.Equals(local.Name, _sourceName, StringComparison.Ordinal)
                ? new Identifier(_alias)
                : null;

        public override Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
            => string.Equals(operation.Local.Name, _sourceName, StringComparison.Ordinal)
                ? new Identifier(_alias)
                : null;
    }

    private sealed class DeclarationOnlyAliasHost : SemanticWalkerHost
    {
        private readonly string _sourceName;
        private readonly string _alias;

        public DeclarationOnlyAliasHost(string sourceName, string alias)
        {
            _sourceName = sourceName;
            _alias = alias;
        }

        public override Identifier? RewriteLocalDeclarationIdentifier(ILocalSymbol local, IOperation operation, SenseArgument argument)
            => string.Equals(local.Name, _sourceName, StringComparison.Ordinal)
                ? new Identifier(_alias)
                : null;
    }
}
