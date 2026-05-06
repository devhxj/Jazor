using Basic.Reference.Assemblies;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScript.PiniaTestingTests;

[TestClass]
public sealed class EcmaScriptPiniaTestingImportTests
{
	private static async Task<string?> ConvertModuleAsync(string code, string className)
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(code, path: "/src/TestModule.cs");
		var compilation = CSharpCompilation.Create(
			"TestAssembly",
			[syntaxTree],
			Net100.References.All.Concat(
			[
				MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ECMAScript.VueContract.IVueComponent).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Vue3).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Pinia).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(PiniaTesting).Assembly.Location)
			]),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var diagnostics = compilation.GetDiagnostics()
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();
		Assert.IsFalse(diagnostics.Length > 0, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

		var semanticModel = compilation.GetSemanticModel(syntaxTree);
		var classDeclaration = syntaxTree.GetRoot()
			.DescendantNodes()
			.OfType<ClassDeclarationSyntax>()
			.Single(node => node.Identifier.Text == className);
		var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

		Assert.IsNotNull(classSymbol);
		var converter = new AstConverter(classSymbol, semanticModel);
		var module = await converter.Convert();
		return module?.ToKnRECMAScript();
	}

	[TestMethod]
	public async Task Convert_ClassUsingCreateTestingPinia_GeneratesTestingPackageImport()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				[ECMAScriptModule("tests/testing-root.mjs")]
				public static class CounterTestingModule
				{
					public static Pinia.PiniaInstance CreateRoot()
						=> CreateTestingPinia();
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterTestingModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createTestingPinia } from \"@pinia/testing\";");
		StringAssert.Contains(script, "return createTestingPinia();");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTestingOptions_GeneratesTypedOptionsObject()
	{
		var code = """
			using System;
			using ECMAScript;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				public sealed record CounterInitialState : TestingInitialState
				{
					public CounterPatch Counter { get; init; } = default!;
				}

				public sealed record CounterPatch : Pinia.PiniaStatePatch<CounterState>
				{
					public int? Count { get; init; }
				}

				public sealed record CounterState : Pinia.PiniaStateTree
				{
					public int Count { get; init; }
				}

				[ECMAScriptModule("tests/testing-options.mjs")]
				public static class CounterTestingModule
				{
					public static Pinia.PiniaInstance CreateRoot()
						=> CreateTestingPinia(new TestingOptions
						{
							InitialState = new CounterInitialState
							{
								Counter = new CounterPatch
								{
									Count = 7
								}
							},
							StubActions = true,
							StubPatch = false,
							StubReset = true,
							FakeApp = true,
							CreateSpy = WrapSpy
						});

					private static Delegate WrapSpy(Delegate? callback)
						=> callback ?? ((Action)Noop);

					private static void Noop()
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterTestingModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createTestingPinia } from \"@pinia/testing\";");
		StringAssert.Contains(script, "return createTestingPinia({");
		StringAssert.Contains(script, "initialState: { counter: { count: 7 } }");
		StringAssert.Contains(script, "stubActions: true");
		StringAssert.Contains(script, "stubPatch: false");
		StringAssert.Contains(script, "stubReset: true");
		StringAssert.Contains(script, "fakeApp: true");
		StringAssert.Contains(script, "createSpy: wrapSpy");
	}
}
