using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCollectionExpressionCarrierTest
{
	private static IBlockOperation GetBlockOperation(string code)
	{
		var usings = @"
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        global using System.Numerics;
		global using ECMAScript;
		global using static ECMAScript.Global;";

		var references = TestMetadataReferences.Net11
			.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
		var compilation = CSharpCompilation.Create(
			assemblyName: "TestAssembly",
			syntaxTrees: [
			  CSharpSyntaxTree.ParseText(usings),
			  CSharpSyntaxTree.ParseText(code)
			],
			references: references,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var diagnostics = compilation.GetDiagnostics();
		var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		if (errors.Count > 0)
		{
			var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
			throw new InvalidOperationException(errorMessages);
		}

		var syntaxTree = compilation.SyntaxTrees.Last();
		var semanticModel = compilation.GetSemanticModel(syntaxTree);
		var root = syntaxTree.GetRoot();
		var methodDeclaration = root.DescendantNodes()
			.OfType<MethodDeclarationSyntax>()
			.FirstOrDefault(static method => method.Identifier.ValueText == "TestMethod" && method.Body is not null)
			?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
		if (methodDeclaration?.Body is not null &&
			semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation operation)
			return operation;

		throw new InvalidOperationException("未找到可分析的操作");
	}

	private static void AssertScriptEqual(string expected, string? actual)
		=> Assert.AreEqual(expected.ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

	[TestMethod]
	public void Visit_CollectionExpression_ReadOnlySpanWithErasedUnsupportedElementType_Allows()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    ReadOnlySpan<Random> values = [];
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [];
}", script);
	}

	[TestMethod]
	public void Visit_CollectionExpression_SpanWithErasedUnsupportedElementType_Allows()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Span<Random> values = [];
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [];
}", script);
	}

	[TestMethod]
	public void Visit_CollectionExpression_IReadOnlyListWithErasedUnsupportedElementType_Allows()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    IReadOnlyList<Random> values = [];
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [];
}", script);
	}

	[TestMethod]
	public void Visit_CollectionExpression_IReadOnlyCollectionWithErasedUnsupportedElementType_Allows()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    IReadOnlyCollection<Random> values = [];
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [];
}", script);
	}
}
