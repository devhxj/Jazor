using Basic.Reference.Assemblies;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScript.PiniaTests;

[TestClass]
public sealed class EcmaScriptPiniaImportTests
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
				MetadataReference.CreateFromFile(typeof(Pinia).Assembly.Location)
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
	public async Task Convert_ClassUsingPiniaBindings_GeneratesPlainPiniaImports()
	{
		var code = """
			using ECMAScript;
			using ECMAScript.VueContract;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				[ECMAScriptModule("stores/counter.mjs")]
				public static class CounterStoreModule
				{
					public static StoreDefinition<Store<CounterState>> CreateStore()
						=> DefineStore<CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

					public static PiniaInstance CreateRoot()
						=> CreatePinia();

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createPinia, defineStore } from \"pinia\";");
		StringAssert.Contains(script, "export function createStore()");
		StringAssert.Contains(script, "return defineStore(\"counter\", { state: createState });");
		StringAssert.Contains(script, "export function createRoot()");
		StringAssert.Contains(script, "return createPinia();");
		StringAssert.Contains(script, "function createState()");
		StringAssert.Contains(script, "return { count: 0 };");
	}

	[TestMethod]
	public async Task Convert_ClassUsingStoreDefinitionUse_LowersToCallableStoreFactoryInvocation()
	{
		var code = """
			using ECMAScript;
			using ECMAScript.VueContract;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				[ECMAScriptModule("stores/use-counter.mjs")]
				public static class CounterStoreModule
				{
					public static Store<CounterState> Resolve(PiniaInstance pinia)
					{
						var useCounterStore = DefineStore<CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

						return useCounterStore.Use(pinia);
					}

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore } from \"pinia\";");
		StringAssert.Contains(script, "let useCounterStore = defineStore(\"counter\", { state: createState });");
		StringAssert.Contains(script, "return useCounterStore(pinia);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingMapStateObjectForm_GeneratesMappedComputedObject()
	{
		var code = """
			using ECMAScript;
			using ECMAScript.VueContract;
			using static ECMAScript.Pinia;
			using static ECMAScript.Vue3;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				public abstract class CounterStore : Store<CounterState>
				{
				}

				[ECMAScriptModule("components/counter-panel.mjs")]
				public static class CounterPanelModule
				{
					public static IVueComponent Component = DefineComponent(new VueComponentOptions
					{
						Name = "CounterPanel",
						Computed = CreateComputed(),
						Render = Render
					});

					private static VueProps CreateComputed()
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

						return MapState(useCounterStore, new PiniaStateMapper<CounterStore>
						{
							{ "count", "count" },
							{ "doubleCount", "double" },
							{ "tripleCount", (PiniaMapStateSelector<CounterStore>)ReadTriple }
						});
					}

					public static IVNode Render()
						=> H("section", "ready");

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};

					private static PiniaValue ReadTriple(CounterStore store)
						=> store.State.Count * 3;
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterPanelModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore, mapState } from \"pinia\";");
		StringAssert.Contains(script, "return mapState(useCounterStore, {");
		StringAssert.Contains(script, "doubleCount: \"double\"");
		StringAssert.Contains(script, "tripleCount: readTriple");
	}

	[TestMethod]
	public async Task Convert_ClassUsingMapWritableStateAndMapActions_GeneratesHelperImports()
	{
		var code = """
			using ECMAScript;
			using ECMAScript.VueContract;
			using static ECMAScript.Pinia;
			using static ECMAScript.Vue3;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Increment();
				}

				[ECMAScriptModule("components/counter-actions.mjs")]
				public static class CounterActionsModule
				{
					public static IVueComponent Component = DefineComponent(new VueComponentOptions
					{
						Name = "CounterActions",
						Computed = CreateComputed(),
						Methods = CreateMethods(),
						Render = Render
					});

					private static VueProps CreateComputed()
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

						return MapWritableState(useCounterStore, ["count"]);
					}

					private static VueProps CreateMethods()
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

						return MapActions(useCounterStore, new PiniaKeyMapper
						{
							{ "add", "increment" }
						});
					}

					public static IVNode Render()
						=> H("section", "ready");

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterActionsModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore, mapActions, mapWritableState } from \"pinia\";");
		StringAssert.Contains(script, "return mapWritableState(useCounterStore, [\"count\"]);");
		StringAssert.Contains(script, "return mapActions(useCounterStore, { add: \"increment\" });");
	}

	[TestMethod]
	public async Task Convert_ClassUsingMapStoresAndSetMapStoreSuffix_GeneratesVarargStoreProjection()
	{
		var code = """
			using ECMAScript;
			using ECMAScript.VueContract;
			using static ECMAScript.Pinia;
			using static ECMAScript.Vue3;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				public sealed record TodoState : PiniaStateTree
				{
					public int Total { get; init; }
				}

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public abstract class TodoStore : Store<TodoState>
				{
				}

				[ECMAScriptModule("components/store-panel.mjs")]
				public static class StorePanelModule
				{
					public static IVueComponent Component = DefineComponent(new VueComponentOptions
					{
						Name = "StorePanel",
						Computed = CreateComputed(),
						Render = Render
					});

					private static VueProps CreateComputed()
					{
						SetMapStoreSuffix("");

						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateCounterState
						});
						var useTodoStore = DefineStore<TodoStore, TodoState>("todo", new DefineStoreOptions<TodoState>
						{
							State = CreateTodoState
						});

						return MapStores(useCounterStore, useTodoStore);
					}

					public static IVNode Render()
						=> H("section", "ready");

					private static CounterState CreateCounterState()
						=> new CounterState
						{
							Count = 0
						};

					private static TodoState CreateTodoState()
						=> new TodoState
						{
							Total = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "StorePanelModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore, mapStores, setMapStoreSuffix } from \"pinia\";");
		StringAssert.Contains(script, "setMapStoreSuffix(\"\");");
		StringAssert.Contains(script, "return mapStores(useCounterStore, useTodoStore);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingMapGetters_GeneratesDeprecatedAliasImport()
	{
		var code = """
			using ECMAScript;
			using ECMAScript.VueContract;
			using static ECMAScript.Pinia;
			using static ECMAScript.Vue3;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				public abstract class CounterStore : Store<CounterState>
				{
				}

				[ECMAScriptModule("components/counter-getters.mjs")]
				public static class CounterGetterModule
				{
					public static IVueComponent Component = DefineComponent(new VueComponentOptions
					{
						Name = "CounterGetterPanel",
						Computed = CreateComputed(),
						Render = Render
					});

					private static VueProps CreateComputed()
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

						return MapGetters(useCounterStore, ["double"]);
					}

					public static IVNode Render()
						=> H("section", "ready");

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterGetterModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore, mapGetters } from \"pinia\";");
		StringAssert.Contains(script, "return mapGetters(useCounterStore, [\"double\"]);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingStoreToRefs_GeneratesStoreToRefsImport()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				[ECMAScriptModule("stores/refs-counter.mjs")]
				public static class CounterStoreModule
				{
					public static StoreRefs<Store<CounterState>> CreateRefs(Store<CounterState> store)
						=> StoreToRefs(store);
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { storeToRefs } from \"pinia\";");
		StringAssert.Contains(script, "return storeToRefs(store);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingAcceptHMRUpdate_GeneratesHmrHandlerImport()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }
				}

				[ECMAScriptModule("stores/hmr-counter.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaHotUpdateHandler CreateHotHandler(IObject hot)
					{
						var useCounterStore = DefineStore<CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

						return AcceptHMRUpdate(useCounterStore, hot);
					}

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { acceptHMRUpdate, defineStore } from \"pinia\";");
		StringAssert.Contains(script, "return acceptHMRUpdate(useCounterStore, hot);");
	}
}
