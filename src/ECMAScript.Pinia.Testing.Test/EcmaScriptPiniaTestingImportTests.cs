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
					public static PiniaTesting.TestingPinia CreateRoot()
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
					public static PiniaTesting.TestingPinia CreateRoot()
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

	[TestMethod]
	public async Task Convert_ClassUsingTestingOptionsPredicateStubActions_GeneratesPredicateConfiguration()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				[ECMAScriptModule("tests/testing-options-predicate.mjs")]
				public static class CounterTestingModule
				{
					public static PiniaTesting.TestingPinia CreateRoot()
						=> CreateTestingPinia(new TestingOptions
						{
							StubActions = (PiniaTestingStubActionPredicate)ShouldStub,
							WritableComputed = true
						});

					private static bool ShouldStub(string actionName, Pinia.StoreGeneric store)
						=> actionName == "increment" && store.Id == "counter";
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterTestingModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createTestingPinia } from \"@pinia/testing\";");
		StringAssert.Contains(script, "return createTestingPinia({");
		StringAssert.Contains(script, "stubActions: shouldStub");
		StringAssert.Contains(script, "writableComputed: true");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTestingOptionsNamedStubActions_GeneratesArrayConfiguration()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				[ECMAScriptModule("tests/testing-options-named-stubs.mjs")]
				public static class CounterTestingModule
				{
					public static PiniaTesting.TestingPinia CreateRoot()
						=> CreateTestingPinia(new TestingOptions
						{
							StubActions = new[] { "increment", "resetStatus" },
							StubPatch = true
						});
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterTestingModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createTestingPinia } from \"@pinia/testing\";");
		StringAssert.Contains(script, "return createTestingPinia({");
		StringAssert.Contains(script, "stubActions: [\"increment\", \"resetStatus\"]");
		StringAssert.Contains(script, "stubPatch: true");
	}

	[TestMethod]
	public async Task Convert_ClassReadingTestingPiniaApp_LowersToTestingRootAppAccess()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				[ECMAScriptModule("tests/testing-root-app.mjs")]
				public static class CounterTestingModule
				{
					public static Vue3.VueApp ResolveApp()
					{
						var pinia = CreateTestingPinia(new TestingOptions
						{
							FakeApp = true
						});
						return pinia.App;
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterTestingModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let pinia = createTestingPinia({ fakeApp: true });");
		StringAssert.Contains(script, "fakeApp: true");
		StringAssert.Contains(script, "return pinia.app;");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTestingOptionsPluginsAndInitialStatePatch_GeneratesCookbookConfiguration()
	{
		var code = """
			using System;
			using ECMAScript;
			using static ECMAScript.Pinia;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }

					public string Status { get; init; } = "";
				}

				public sealed record CounterPatch : PiniaStatePatch<CounterState>
				{
					public int? Count { get; init; }

					public string? Status { get; init; }
				}

				public sealed record CounterInitialState : TestingInitialState
				{
					public CounterPatch Counter { get; init; } = default!;
				}

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginOutput : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				[ECMAScriptModule("tests/testing-cookbook.mjs")]
				public static class CounterTestingModule
				{
					public static TestingPinia CreateRoot()
						=> CreateTestingPinia(new TestingOptions
						{
							InitialState = new CounterInitialState
							{
								Counter = new CounterPatch
								{
									Count = 11,
									Status = "testing"
								}
							},
							Plugins =
							[
								InstallPlugin
							],
							StubActions = false,
							WritableComputed = true,
							CreateSpy = WrapSpy
						});

					private static Vue3.VueProps? InstallPlugin(PiniaPluginContext context)
						=> new CounterPluginOutput
						{
							AuditTag = context.Store.Id + ":testing"
						};

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
		StringAssert.Contains(script, "return createTestingPinia({");
		StringAssert.Contains(script, "initialState: { counter: { count: 11, status: \"testing\" } }");
		StringAssert.Contains(script, "plugins: [installPlugin]");
		StringAssert.Contains(script, "stubActions: false");
		StringAssert.Contains(script, "writableComputed: true");
		StringAssert.Contains(script, "createSpy: wrapSpy");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTypedTestingOptionsCreateSpy_GeneratesSameRuntimeCreateSpyConfiguration()
	{
		var code = """
			using System;
			using ECMAScript;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				[ECMAScriptModule("tests/testing-options-typed-create-spy.mjs")]
				public static class CounterTestingModule
				{
					public static TestingPinia CreateRoot()
						=> CreateTestingPinia(new TestingOptions<Action<int>>
						{
							StubActions = false,
							CreateSpy = WrapSpy
						});

					private static Action<int> WrapSpy(Action<int>? callback)
						=> callback ?? Noop;

					private static void Noop(int value)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterTestingModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createTestingPinia } from \"@pinia/testing\";");
		StringAssert.Contains(script, "return createTestingPinia({");
		StringAssert.Contains(script, "stubActions: false");
		StringAssert.Contains(script, "createSpy: wrapSpy");
	}

	[TestMethod]
	public async Task Convert_ClassUsingProjectedTypedPlugins_GeneratesSameRuntimePluginsConfiguration()
	{
		var code = """
			using System;
			using ECMAScript;
			using static ECMAScript.Pinia;
			using static ECMAScript.PiniaTesting;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterGetters : Vue3.VueProps
				{
					public Func<int> DoubleCount { get; init; } = default!;
				}

				public sealed record CounterActions : Vue3.VueProps
				{
					public Action Increment { get; init; } = default!;
				}

				public sealed record CounterPluginOptions : DefineStoreOptionsInPlugin
				{
					public string AuditMode { get; init; } = "";
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				[ECMAScriptModule("tests/testing-options-projected-plugins.mjs")]
				public static class CounterTestingModule
				{
					public static TestingPinia CreateRoot()
						=> CreateTestingPinia(new TestingOptions
						{
							Plugins =
							[
								ProjectPlugin<CounterStore, DefineStoreOptionsInPlugin<CounterState, CounterGetters, CounterActions>, CounterPluginExtensions>(InstallTypedPlugin),
								ProjectPlugin<CounterStore, DefineStoreOptionsInPlugin<CounterState, CounterGetters, CounterActions>, CounterPluginExtensions, CounterPluginState, CounterPluginExtensions>(InstallProjectedPlugin)
							]
						});

					private static CounterPluginExtensions? InstallTypedPlugin(PiniaPluginContext<CounterStore, DefineStoreOptionsInPlugin<CounterState, CounterGetters, CounterActions>> context)
						=> new CounterPluginExtensions
						{
							AuditTag = context.Store.Id + ":typed"
						};

					private static CounterPluginExtensions? InstallProjectedPlugin(PiniaPluginContext<CounterStore, DefineStoreOptionsInPlugin<CounterState, CounterGetters, CounterActions>, CounterPluginExtensions, CounterPluginState> context)
						=> new CounterPluginExtensions
						{
							AuditTag = context.Store.AsCustomProperties().AuditTag + ":projected"
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterTestingModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createTestingPinia } from \"@pinia/testing\";");
		StringAssert.Contains(script, "return createTestingPinia({");
		StringAssert.Contains(script, "plugins: [installTypedPlugin, installProjectedPlugin]");
	}
}
