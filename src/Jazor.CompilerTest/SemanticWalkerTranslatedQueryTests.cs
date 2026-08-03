using Acornima;
using DenoHost.Core;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTranslatedQueryTests
{
	[TestMethod]
	public async Task Visit_TranslatedQuery_LetWhereSelectToArray_PreservesProjectionChainOnDenoHost()
	{
		var block = GetBlockOperation("""
			using System.Linq;

			public static class QueryScenarios
			{
				public static int[] SelectNormalizedValues(int[] values)
				{
					return (from value in values
							let doubled = value * 2
							where doubled > 3
							select doubled + 1).ToArray();
				}
			}
			""", "SelectNormalizedValues");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		Assert.IsEmpty(argument.FlushImportSpecifiers(), body);
		Assert.IsFalse(body.Contains("<>h__TransparentIdentifier", StringComparison.Ordinal), body);
		StringAssert.Contains(body, "value * 2", StringComparison.Ordinal);
		StringAssert.Contains(body, "doubled > 3", StringComparison.Ordinal);
		StringAssert.Contains(body, "doubled + 1", StringComparison.Ordinal);
		Assert.IsFalse(body.Contains("TranslatedQuery", StringComparison.Ordinal), body);

		var module = "export function selectNormalizedValues(values) " + body;
		try
		{
			_ = new Parser().ParseModule(module);
		}
		catch (SyntaxErrorException exception)
		{
			Assert.Fail($"{exception.Message}{Environment.NewLine}{module}");
		}

		var root = Path.Combine(
			Path.GetTempPath(),
			"jazor-translated-query-let-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(root);

		try
		{
			var modulePath = Path.Combine(root, "query-let.mjs");
			var testPath = Path.Combine(root, "query-let.test.mjs");
			await System.IO.File.WriteAllTextAsync(
				modulePath,
				module,
				new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			await System.IO.File.WriteAllTextAsync(
				testPath,
				"""
				import { selectNormalizedValues } from "./query-let.mjs";

				Deno.test("query let preserves its projected value through where and select", () => {
				  const actual = selectNormalizedValues([1, 2, 3]);
				  if (actual.length !== 2 || actual[0] !== 5 || actual[1] !== 7)
				    throw new Error(`expected [5, 7], got ${JSON.stringify(actual)}`);
				});
				""",
				new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

			using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			await Deno.Execute(
				new DenoExecuteBaseOptions { WorkingDirectory = root },
				["test", "--quiet", "--allow-read", testPath],
				timeout.Token);
		}
		finally
		{
			if (Directory.Exists(root))
				Directory.Delete(root, recursive: true);
		}
	}

	[TestMethod]
	public void Visit_TranslatedQuery_WhereSelectAndToArray_UsesEnumerableIntrinsicAndLambdaContracts()
    {
        var block = GetBlockOperation("""
            using System.Linq;

            public static class QueryScenarios
            {
                public static int[] SelectEvenTriples(int[] values)
                {
                    return (from value in values
                            where value % 2 == 0
                            select value * 3).ToArray();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers()
            .Select(static pair =>
            {
                var specifiers = string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript()));
                return "import " + specifiers + " from \"" + pair.Key + "\";";
            })
            .ToArray();
        var module = string.Join("\n", imports.Append("function verify() " + body));

        Assert.IsEmpty(imports, body);
        StringAssert.Contains(body, "return __src.filter(__callback);");
        StringAssert.Contains(body, "return Array.from(__src).map(__callback);");
        StringAssert.Contains(body, "return Array.from(__src);");
        StringAssert.Contains(body, "value =>");
        StringAssert.Contains(body, "value % 2 === 0");
        StringAssert.Contains(body, "value * 3");
        Assert.IsFalse(body.Contains("TranslatedQuery", StringComparison.Ordinal), body);

        _ = new Parser().ParseModule(module);
    }

	private static IBlockOperation GetBlockOperation(string source, string methodName = "SelectEvenTriples")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TranslatedQueryScenarios",
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
			.Single(candidate => candidate.Identifier.ValueText == methodName);
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
