using Basic.Reference.Assemblies;
using Jazor.Compiler;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScript.PiniaTests;

[TestClass]
public sealed class EcmaScriptPiniaImportTests
{
	private static string ImportBindingName(string modulePath, string importedName)
		=> $"i${Format.HashName($"{modulePath}\0{importedName}").TrimStart('_')}";

	private static async Task<string?> ConvertModuleAsync(string code, string className)
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(
			code,
			CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			path: "/src/TestModule.cs");
		var compilation = CSharpCompilation.Create(
			"TestAssembly",
			[syntaxTree],
			Net110.References.All.Concat(
			[
				MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ECMAScript.VueContract.VueLibraryComponentAttribute).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3.IVueComponent).Assembly.Location),
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
			using System;
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
	public async Task Convert_ClassUsingMapStateObjectFormFactories_GeneratesMappedComputedObject()
	{
		var code = """
			using System;
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

				[ECMAScriptModule("components/counter-panel-factories.mjs")]
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
							{ "count", PiniaStateMapValue<CounterStore>.From("count") },
							{ "doubleCount", PiniaStateMapValue<CounterStore>.From("double") },
							{ "tripleCount", PiniaStateMapValue<CounterStore>.From(ReadTriple) }
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
		StringAssert.Contains(script, "count: \"count\"");
		StringAssert.Contains(script, "doubleCount: \"double\"");
		StringAssert.Contains(script, "tripleCount: readTriple");
	}

	[TestMethod]
	public async Task Convert_ClassUsingMapWritableStateAndMapActions_GeneratesHelperImports()
	{
		var code = """
			using System;
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
			using System;
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
	public async Task Convert_ClassUsingMapGettersObjectFormFactories_GeneratesMappedComputedObject()
	{
		var code = """
			using System;
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

				[ECMAScriptModule("components/counter-getters-factories.mjs")]
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

						return MapGetters(useCounterStore, new PiniaStateMapper<CounterStore>
						{
							{ "count", PiniaStateMapValue<CounterStore>.From("count") },
							{ "doubleCount", PiniaStateMapValue<CounterStore>.From("double") },
							{ "tripleCount", PiniaStateMapValue<CounterStore>.From(ReadTriple) }
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

		var script = await ConvertModuleAsync(code, "CounterGetterModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore, mapGetters } from \"pinia\";");
		StringAssert.Contains(script, "return mapGetters(useCounterStore, {");
		StringAssert.Contains(script, "count: \"count\"");
		StringAssert.Contains(script, "doubleCount: \"double\"");
		StringAssert.Contains(script, "tripleCount: readTriple");
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
	public async Task Convert_ClassUsingStoreToRefsWithProjectedStore_LowersToStoreToRefsIdentityCall()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				[ECMAScriptModule("stores/projected-refs-counter.mjs")]
				public static class CounterStoreModule
				{
					public static StoreRefs<ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>> CreateRefs(
						ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState> store)
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
	public async Task Convert_ClassUsingSetupStoreHelpers_LowersToHelperAwareDefineStoreCallback()
	{
		var code = """
			using System;
			using System.ComponentModel;
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				[ECMAScript]
				[Description("@#")]
				public sealed record CounterSetupStore : Vue3.VueProps
				{
					public int Count { get; init; }

					public Action Increment { get; init; } = default!;
				}

				[ECMAScriptModule("stores/setup-counter.mjs")]
				public static class CounterSetupModule
				{
					public static StoreDefinition<CounterSetupStore> CreateStore()
						=> DefineStore<CounterSetupStore>("counter", Setup);

					private static CounterSetupStore Setup(SetupStoreHelpers helpers)
						=> new CounterSetupStore
						{
							Count = 1,
							Increment = helpers.Action(Increment, "increment")
						};

					private static void Increment()
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterSetupModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return defineStore(\"counter\", setup);");
		StringAssert.Contains(script, "function setup(helpers)");
		StringAssert.Contains(script, "increment: helpers.action(increment, \"increment\")");
	}

	[TestMethod]
	public async Task Convert_ClassUsingHighAritySetupStoreHelpers_LowersToHelperAwareCallbacks()
	{
		var code = """
			using System;
			using System.ComponentModel;
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				[ECMAScript]
				[Description("@#")]
				public sealed record CounterSetupStore : Vue3.VueProps
				{
					public Action<int, int, int, int> Report { get; init; } = default!;

					public Func<int, int, int, int, int> Sum { get; init; } = default!;
				}

				[ECMAScriptModule("stores/setup-counter-arity.mjs")]
				public static class CounterSetupModule
				{
					public static StoreDefinition<CounterSetupStore> CreateStore()
						=> DefineStore<CounterSetupStore>("counter", Setup);

					private static CounterSetupStore Setup(SetupStoreHelpers helpers)
						=> new CounterSetupStore
						{
							Report = helpers.Action<int, int, int, int>(Report, "report"),
							Sum = helpers.Action<int, int, int, int, int>(Sum, "sum")
						};

					private static void Report(int left, int right, int top, int bottom)
					{
					}

					private static int Sum(int a, int b, int c, int d)
						=> a + b + c + d;
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterSetupModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return defineStore(\"counter\", setup);");
		StringAssert.Contains(script, "report: helpers.action(report, \"report\")");
		StringAssert.Contains(script, "sum: helpers.action(sum, \"sum\")");
	}

	[TestMethod]
	public async Task Convert_ClassUsingSetupStoreOptionsActions_LowersToThirdDefineStoreArgument()
	{
		var code = """
			using System;
			using System.ComponentModel;
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				[ECMAScript]
				[Description("@#")]
				public sealed record CounterSetupStore : Vue3.VueProps
				{
					public Action Increment { get; init; } = default!;
				}

				[ECMAScript]
				[Description("@#")]
				public sealed record CounterSetupActions : Vue3.VueProps
				{
					public Action Increment { get; init; } = default!;
				}

				[ECMAScriptModule("stores/setup-counter-options.mjs")]
				public static class CounterSetupModule
				{
					public static StoreDefinition<CounterSetupStore> CreateStore()
						=> DefineStore<CounterSetupStore>(
							"counter",
							Setup,
							new DefineSetupStoreOptions<CounterSetupActions>
							{
								Actions = new CounterSetupActions
								{
									Increment = Increment
								}
							});

					private static CounterSetupStore Setup(SetupStoreHelpers helpers)
						=> new CounterSetupStore
						{
							Increment = helpers.Action(Increment)
						};

					private static void Increment()
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterSetupModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return defineStore(\"counter\", setup, {");
		StringAssert.Contains(script, "actions: { increment: increment }");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTypedOnActionProxy_LowersToStoreOnActionCall()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Increment();
				}

				[ECMAScriptModule("stores/action-proxy.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction, true);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						context.After(AfterAction);
						context.OnError(HandleError);
					}

					private static void AfterAction()
					{
					}

					private static void HandleError(Error error)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return store.$onAction(handleAction, true);");
		StringAssert.Contains(script, "context.after(afterAction);");
		StringAssert.Contains(script, "context.onError(handleError);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTypedOnActionAfterResultProxy_LowersTypedAfterCallback()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern int IncrementAndGet();
				}

				[ECMAScriptModule("stores/action-after-result-proxy.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						context.After<int>(AfterCount);
					}

					private static void AfterCount(int count)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return store.$onAction(handleAction);");
		StringAssert.Contains(script, "context.after(afterCount);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingUnknownLikeOnActionErrorProxy_LowersUnknownErrorCallback()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Increment();
				}

				[ECMAScriptModule("stores/action-error-proxy.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						context.OnAnyError(HandleAnyError);
					}

					private static void HandleAnyError(PiniaValue? error)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return store.$onAction(handleAction);");
		StringAssert.Contains(script, "context.onError(handleAnyError);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTypedOnActionErrorProxy_LowersTypedErrorCallback()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Increment();
				}

				[ECMAScriptModule("stores/action-typed-error-proxy.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						context.OnError<string>(HandleMessage);
					}

					private static void HandleMessage(string message)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return store.$onAction(handleAction);");
		StringAssert.Contains(script, "context.onError(handleMessage);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingProjectedOnActionContext_LowersTypedActionNameAndArraySlotAccess()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern int RenameAndReport(int count, string label);
				}

				[ECMAScriptModule("stores/action-projected-context.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						var rename = ProjectActionContext<CounterStore, string, ActionArgsView<int, string>>(context);
						var summary = rename.ActionName + ":" + rename.ActionArgs.Arg0 + ":" + rename.ActionArgs.Arg1;
						rename.After<string>(result => Capture(summary + "|" + result));
					}

					private static void Capture(string value)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return store.$onAction(handleAction);");
		StringAssert.Contains(script, "let rename = context;");
		StringAssert.Contains(script, "let summary = rename.name + \":\" + rename.args[0] + \":\" + rename.args[1];");
		StringAssert.Contains(script, "rename.after(result => {");
		StringAssert.Contains(script, "capture(summary + \"|\" + result);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingHighArityProjectedOnActionContext_LowersMultipleTypedArraySlots()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Track(int count, string label, bool enabled, double weight, long version);
				}

				[ECMAScriptModule("stores/action-projected-context-arity.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						var track = ProjectActionContext<CounterStore, string, ActionArgsView<int, string, bool, double, long>>(context);
						Capture(
							track.ActionName
							+ ":"
							+ track.ActionArgs.Arg0
							+ ":"
							+ track.ActionArgs.Arg1
							+ ":"
							+ track.ActionArgs.Arg2
							+ ":"
							+ track.ActionArgs.Arg3
							+ ":"
							+ track.ActionArgs.Arg4
							+ ":"
							+ track.ActionArgs.Length);
					}

					private static void Capture(string value)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let track = context;");
		StringAssert.Contains(script, "track.name");
		StringAssert.Contains(script, "track.args[0]");
		StringAssert.Contains(script, "track.args[1]");
		StringAssert.Contains(script, "track.args[2]");
		StringAssert.Contains(script, "track.args[3]");
		StringAssert.Contains(script, "track.args[4]");
		StringAssert.Contains(script, "track.args.length");
	}

	[TestMethod]
	public async Task Convert_ClassUsingMaxProjectedOnActionContext_LowersTailArraySlotAccess()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Audit(int a0, int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10, int a11, int a12, int a13, int a14, string a15);
				}

				[ECMAScriptModule("stores/action-projected-context-max-arity.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						var audit = ProjectActionContext<CounterStore, string, ActionArgsView<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, string>>(context);
						Capture(audit.ActionName + ":" + audit.ActionArgs.Arg0 + ":" + audit.ActionArgs.Arg15 + ":" + audit.ActionArgs.Length);
					}

					private static void Capture(string value)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let audit = context;");
		StringAssert.Contains(script, "audit.args[0]");
		StringAssert.Contains(script, "audit.args[15]");
		StringAssert.Contains(script, "audit.args.length");
	}

	[TestMethod]
	public async Task Convert_ClassUsingGuardedProjectedOnActionContext_LowersExplicitActionNameGuard()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Rename(int count, string label);
				}

				[ECMAScriptModule("stores/action-projected-context-guarded.mjs")]
				public static class CounterStoreModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.OnAction<CounterStore>(HandleAction);

					private static void HandleAction(StoreActionListenerContext<CounterStore> context)
					{
						var rename = TryProjectActionContext<CounterStore, string, ActionArgsView<int, string>>(context, "rename");
						if (rename == null)
							return;

						Capture(rename.ActionArgs.Arg0 + ":" + rename.ActionArgs.Arg1 + ":" + rename.ActionName);
					}

					private static void Capture(string value)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let rename = context.name === \"rename\" ? context : null;");
		StringAssert.Contains(script, "if (rename === null)");
		StringAssert.Contains(script, "return;");
		StringAssert.Contains(script, "capture(rename.args[0] + \":\" + rename.args[1] + \":\" + rename.name);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTypedPiniaPluginContext_LowersToPiniaUseCall()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginOptions : DefineStoreOptionsInPlugin<CounterState>
				{
					public bool? Persist { get; init; }
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				[ECMAScriptModule("stores/plugin-proxy.mjs")]
				public static class CounterPluginModule
				{
					public static PiniaInstance CreateConfiguredRoot()
						=> CreatePinia().Use<CounterStore, CounterPluginOptions, CounterPluginExtensions>(Install);

					private static CounterPluginExtensions Install(PiniaPluginContext<CounterStore, CounterPluginOptions> context)
						=> new CounterPluginExtensions
						{
							AuditTag = context.Options.Persist == true ? context.Store.Id : "volatile"
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterPluginModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createPinia } from \"pinia\";");
		StringAssert.Contains(script, "return createPinia().use(install);");
		StringAssert.Contains(script, "auditTag: context.options.persist === true ? context.store.$id : \"volatile\"");
	}

	[TestMethod]
	public async Task Convert_ClassUsingProjectedPiniaPluginContext_LowersToProjectedStoreViews()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginOptions : DefineStoreOptionsInPlugin<CounterState>
				{
					public bool? Persist { get; init; }
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				public sealed record CounterPluginOutput : Vue3.VueProps
				{
					public string MirrorTag { get; init; } = "";
				}

				[ECMAScriptModule("stores/plugin-projected-context.mjs")]
				public static class CounterPluginModule
				{
					public static PiniaInstance CreateConfiguredRoot()
						=> CreatePinia().Use<CounterStore, CounterPluginOptions, CounterPluginExtensions, CounterPluginState, CounterPluginOutput>(Install);

					private static CounterPluginOutput Install(PiniaPluginContext<CounterStore, CounterPluginOptions, CounterPluginExtensions, CounterPluginState> context)
						=> new CounterPluginOutput
						{
							MirrorTag = context.Store.AsStore().Id
								+ ":"
								+ context.Store.AsCustomProperties().AuditTag
								+ ":"
								+ context.Store.AsCustomState().PersistedAt
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterPluginModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createPinia } from \"pinia\";");
		StringAssert.Contains(script, "return createPinia().use(install);");
		StringAssert.Contains(script, "mirrorTag: context.store.$id + \":\" + context.store.auditTag + \":\" + context.store.$state.persistedAt");
	}

	[TestMethod]
	public async Task Convert_ClassUsingProjectedStoreDefinition_LowersToIdentityProjectionViews()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				[ECMAScriptModule("stores/projected-definition.mjs")]
				public static class CounterProjectionModule
				{
					public static string ReadProjectedStore(PiniaInstance pinia)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions, CounterPluginState>(useCounterStore);
						var baseStore = projectedUseCounterStore.AsDefinition().Use(pinia);
						var projectedStore = projectedUseCounterStore.Use(pinia);
						return baseStore.Id + projectedStore.AsCustomProperties().AuditTag + projectedStore.AsCustomState().PersistedAt;
					}

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterProjectionModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore } from \"pinia\";");
		StringAssert.Contains(script, "let projectedUseCounterStore = useCounterStore;");
		StringAssert.Contains(script, "let baseStore = projectedUseCounterStore(pinia);");
		StringAssert.Contains(script, "let projectedStore = projectedUseCounterStore(pinia);");
		StringAssert.Contains(script, "return baseStore.$id + projectedStore.auditTag + projectedStore.$state.persistedAt;");
		Assert.IsFalse(script.Contains("projectStoreDefinition", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task Convert_ClassUsingProjectedStore_LowersToIdentityProjectionViews()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				[ECMAScriptModule("stores/projected-store.mjs")]
				public static class CounterProjectionModule
				{
					public static string ReadProjectedStore(PiniaInstance pinia)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var store = useCounterStore.Use(pinia);
						var projectedStore = ProjectStore<CounterStore, CounterPluginExtensions, CounterPluginState>(store);
						return store.Id + projectedStore.AsCustomProperties().AuditTag + projectedStore.AsCustomState().PersistedAt;
					}

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterProjectionModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore } from \"pinia\";");
		StringAssert.Contains(script, "let store = useCounterStore(pinia);");
		StringAssert.Contains(script, "let projectedStore = store;");
		StringAssert.Contains(script, "return store.$id + projectedStore.auditTag + projectedStore.$state.persistedAt;");
		Assert.IsFalse(script.Contains("projectStore", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task Convert_ClassUsingProjectedStoreDefinitionWithAcceptHMRUpdate_LowersToStoreDefinitionIdentity()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				[ECMAScriptModule("stores/projected-hmr-counter.mjs")]
				public static class CounterProjectionModule
				{
					public static PiniaHotUpdateHandler CreateHotHandler(IObject hot)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions>(useCounterStore);
						return AcceptHMRUpdate(projectedUseCounterStore, hot);
					}

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterProjectionModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { acceptHMRUpdate, defineStore } from \"pinia\";");
		StringAssert.Contains(script, "let projectedUseCounterStore = useCounterStore;");
		StringAssert.Contains(script, "return acceptHMRUpdate(projectedUseCounterStore, hot);");
		Assert.IsFalse(script.Contains("projectStoreDefinition", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task Convert_ClassUsingStoreDefinitionUseWithHotStore_LowersToCallableStoreFactoryHotInvocation()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				[ECMAScriptModule("stores/hot-use-counter.mjs")]
				public static class CounterStoreModule
				{
					public static CounterStore Resolve(PiniaInstance pinia, StoreGeneric hot)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});

						return useCounterStore.Use(pinia, hot);
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
		StringAssert.Contains(script, "return useCounterStore(pinia, hot);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingConfiguredPiniaRootAndHmrCookbook_LowersPluginAndHmrPaths()
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

				public abstract class CounterStore : Store<CounterState>
				{
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				[ECMAScriptModule("stores/hmr-cookbook.mjs")]
				public static class CounterCookbookModule
				{
					public static PiniaInstance CreateConfiguredRoot()
					{
						var pinia = CreatePinia();
						pinia.Use(Install);
						return pinia;
					}

					public static PiniaHotUpdateHandler CreateHotHandler(IObject hot)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions, CounterPluginState>(useCounterStore);
						return AcceptHMRUpdate(projectedUseCounterStore, hot);
					}

					public static ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState> ResolveProjectedStore(PiniaInstance pinia, StoreGeneric hot)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions, CounterPluginState>(useCounterStore);
						return projectedUseCounterStore.Use(pinia, hot);
					}

					private static CounterPluginExtensions Install(PiniaPluginContext context)
						=> new CounterPluginExtensions
						{
							AuditTag = context.Store.Id + ":audited"
						};

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterCookbookModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { acceptHMRUpdate, createPinia, defineStore } from \"pinia\";");
		StringAssert.Contains(script, "pinia.use(install);");
		StringAssert.Contains(script, "return pinia;");
		StringAssert.Contains(script, "let projectedUseCounterStore = useCounterStore;");
		StringAssert.Contains(script, "return acceptHMRUpdate(projectedUseCounterStore, hot);");
		StringAssert.Contains(script, "return projectedUseCounterStore(pinia, hot);");
		StringAssert.Contains(script, "auditTag: context.store.$id + \":audited\"");
	}

	[TestMethod]
	public async Task Convert_ClassUsingOptionsApiHelpersWithProjectedStoreDefinition_LowersToPlainPiniaHelpers()
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
					public extern int DoubleCount { get; }

					public extern void Increment();
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				[ECMAScriptModule("components/projected-options-api.mjs")]
				public static class CounterPanelModule
				{
					public static IVueComponent Component = DefineComponent(new VueComponentOptions
					{
						Name = "CounterPanel",
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
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions>(useCounterStore);
						return MapState(projectedUseCounterStore, ["count", "auditTag"]);
					}

					private static VueProps CreateMethods()
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions>(useCounterStore);
						return MapActions(projectedUseCounterStore, ["increment"]);
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

		var script = await ConvertModuleAsync(code, "CounterPanelModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore, mapActions, mapState } from \"pinia\";");
		StringAssert.Contains(script, "let projectedUseCounterStore = useCounterStore;");
		StringAssert.Contains(script, "return mapState(projectedUseCounterStore, [\"count\", \"auditTag\"]);");
		StringAssert.Contains(script, "return mapActions(projectedUseCounterStore, [\"increment\"]);");
		Assert.IsFalse(script.Contains("projectStoreDefinition", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task Convert_ClassUsingProjectedStorePluginCookbook_LowersAcrossRefsHelpersAndStateViews()
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

					public string Status { get; init; } = "";
				}

				public abstract class CounterStore : Store<CounterState>
				{
					public extern void Increment();
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				public sealed record CounterComputed : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterMethods : Vue3.VueProps
				{
					public System.Action Increment { get; init; } = default!;
				}

				[ECMAScriptModule("components/projected-cookbook.mjs")]
				public static class CounterCookbookModule
				{
					public static string ReadCookbook(PiniaInstance pinia)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions, CounterPluginState>(useCounterStore);
						var projectedStore = projectedUseCounterStore.Use(pinia);
						var refs = StoreToRefs(projectedStore);
						var computed = MapState<CounterComputed, ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>(projectedUseCounterStore, ["auditTag"]);
						var methods = MapActions<CounterMethods, ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>(projectedUseCounterStore, ["increment"]);
						return projectedStore.AsCustomProperties().AuditTag
							+ projectedStore.AsCustomState().PersistedAt
							+ refs["status"]!.Value
							+ computed.AuditTag
							+ methods.Increment;
					}

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 0,
							Status = "ready"
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterCookbookModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let projectedUseCounterStore = useCounterStore;");
		StringAssert.Contains(script, "let projectedStore = projectedUseCounterStore(pinia);");
		StringAssert.Contains(script, "let refs = storeToRefs(projectedStore);");
		StringAssert.Contains(script, "let computed = mapState(projectedUseCounterStore, [\"auditTag\"]);");
		StringAssert.Contains(script, "let methods = mapActions(projectedUseCounterStore, [\"increment\"]);");
		StringAssert.Contains(script, "return projectedStore.auditTag + projectedStore.$state.persistedAt + refs[\"status\"].value + computed.auditTag + methods.increment;");
	}

	[TestMethod]
	public async Task Convert_ClassUsingMultiStoreCookbook_LowersMapStoresAndEmptySuffixConfiguration()
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

					public string Status { get; init; } = "";
				}

				public sealed record ActivityState : PiniaStateTree
				{
					public int CompletedActions { get; init; }

					public string Summary { get; init; } = "";
				}

				public abstract class CounterStore : Store<CounterState>
				{
					public extern int Count { get; set; }

					public extern string Status { get; set; }
				}

				public abstract class ActivityStore : Store<ActivityState>
				{
					public extern int CompletedActions { get; set; }

					public extern string Summary { get; set; }
				}

				public sealed record MultiStoreComputed : Vue3.VueProps
				{
					public CounterStore Counter { get; init; } = default!;

					public ActivityStore Activity { get; init; } = default!;
				}

				[ECMAScript]
				[System.ComponentModel.Description("@#")]
				public abstract class MultiStoreThis
				{
					public extern CounterStore Counter { get; }

					public extern ActivityStore Activity { get; }
				}

				[ECMAScriptModule("components/multi-store-cookbook.mjs")]
				public static class MultiStoreCookbookModule
				{
					public static IVueComponent Component = DefineComponent(new VueComponentOptions
					{
						Name = "MultiStoreCookbook",
						Computed = CreateComputed(),
						Mounted = BindThis<MultiStoreThis>(CaptureMappedStores),
						Setup = Setup
					});

					private static MultiStoreComputed CreateComputed()
					{
						SetMapStoreSuffix("");
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateCounterState
						});
						var useActivityStore = DefineStore<ActivityStore, ActivityState>("activity", new DefineStoreOptions<ActivityState>
						{
							State = CreateActivityState
						});
						return MapStores<MultiStoreComputed>(useCounterStore, useActivityStore);
					}

					private static VueRenderCallback Setup()
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateCounterState
						});
						var useActivityStore = DefineStore<ActivityStore, ActivityState>("activity", new DefineStoreOptions<ActivityState>
						{
							State = CreateActivityState
						});
						var counter = useCounterStore.Use();
						var activity = useActivityStore.Use();
						var summary = Computed(() => counter.Status + " | " + activity.Summary);

						return () => H("section", summary.Value);
					}

					private static void CaptureMappedStores(MultiStoreThis self)
					{
						self.Counter.Status = self.Activity.Summary;
					}

					private static CounterState CreateCounterState()
						=> new CounterState
						{
							Count = 1,
							Status = "counter ready"
						};

					private static ActivityState CreateActivityState()
						=> new ActivityState
						{
							CompletedActions = 2,
							Summary = "activity ready"
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "MultiStoreCookbookModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { computed, defineComponent, h } from \"npm:vue@3\";");
		StringAssert.Contains(script, "import { defineStore, mapStores, setMapStoreSuffix } from \"pinia\";");
		StringAssert.Contains(script, "setMapStoreSuffix(\"\");");
		StringAssert.Contains(script, "return mapStores(useCounterStore, useActivityStore);");
		StringAssert.Contains(script, "mounted: (__cb => function() {");
		StringAssert.Contains(script, "let summary = computed(() => {");
		StringAssert.Contains(script, "return counter.status + \" | \" + activity.summary;");
		StringAssert.Contains(script, "return h(\"section\", summary.value);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingSubscriptionCookbook_LowersSubscribeWithOptionsAndPatchVariants()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;
			using static ECMAScript.Vue3;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; set; }

					public string Status { get; set; } = "";
				}

				public sealed record CounterPatch : PiniaStatePatch<CounterState>
				{
					public int? Count { get; init; }

					public string? Status { get; init; }
				}

				public abstract class CounterStore : Store<CounterState>
				{
					public extern int Count { get; set; }

					public extern string Status { get; set; }
				}

				[ECMAScriptModule("components/subscription-cookbook.mjs")]
				public static class SubscriptionCookbookModule
				{
					public static string Observe(CounterStore store)
					{
						string mutationKind = "";
						string payloadSummary = "";
						string eventShape = "";
						var detach = store.Subscribe(HandleMutation, new SubscribeOptions
						{
							Detached = true,
							Flush = VueWatchFlush.Sync
						});

						store.Count += 1;
						store.Patch(new CounterPatch
						{
							Count = 5,
							Status = "patched"
						});
						store.Patch(state =>
						{
							state.Count += 2;
							state.Status = "patched by callback";
						});
						detach();
						return mutationKind + "|" + payloadSummary + "|" + eventShape;

						void HandleMutation(SubscriptionMutation<CounterState> mutation, CounterState state)
						{
							mutationKind = DescribeMutationType(mutation.Type) + ":" + mutation.StoreId + ":" + state.Status;
							payloadSummary = ReadMutationSummary(mutation);
							eventShape = DescribeEvents(mutation.Events);
						}
					}

					private static string DescribeMutationType(MutationType type)
						=> type switch
						{
							MutationType.Direct => "direct",
							MutationType.PatchObject => "patch object",
							MutationType.PatchFunction => "patch function",
							_ => "other"
						};

					private static string ReadMutationSummary(SubscriptionMutation<CounterState> mutation)
					{
						if (mutation.Type == MutationType.PatchObject)
						{
							var patchMutation = (SubscriptionMutationPatchObject<CounterState>)mutation;
							var payload = (CounterPatch)patchMutation.Payload;
							return payload.Status ?? "missing";
						}

						if (mutation.Type == MutationType.PatchFunction)
						{
							return "function";
						}

						return "direct";
					}

					private static string DescribeEvents(SubscriptionMutationEvents? events)
						=> events == null ? "none" : "reported";
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "SubscriptionCookbookModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let detach = store.$subscribe(this.HandleMutation.bind(this), { detached: true, flush: \"sync\" });");
		StringAssert.Contains(script, "store.count += 1;");
		StringAssert.Contains(script, "store.$patch({ count: 5, status: \"patched\" });");
		StringAssert.Contains(script, "store.$patch(state => {");
		StringAssert.Contains(script, "=== \"patch object\"");
		StringAssert.Contains(script, "=== \"patch function\"");
		StringAssert.Contains(script, "let patchMutation = mutation;");
		StringAssert.Contains(script, "let payload = patchMutation.payload;");
		StringAssert.Contains(script, "return payload.status ?? \"missing\";");
		StringAssert.Contains(script, "return events === null ? \"none\" : \"reported\";");
	}

	[TestMethod]
	public async Task Convert_ClassUsingSubscriptionWatchOptions_LowersFullWatchOptionsSurface()
	{
		var code = """
			using ECMAScript;
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

				[ECMAScriptModule("components/subscription-watch-options.mjs")]
				public static class SubscriptionWatchOptionsModule
				{
					public static PiniaDetachCallback Observe(CounterStore store)
						=> store.Subscribe(HandleMutation, new SubscribeOptions
						{
							Detached = true,
							Flush = VueWatchFlush.Sync,
							Immediate = true,
							Deep = 2,
							Once = true,
							OnTrack = HandleTrack,
							OnTrigger = HandleTrigger
						});

					private static void HandleMutation(SubscriptionMutation<CounterState> mutation, CounterState state)
					{
					}

					private static void HandleTrack(VueDebuggerEvent @event)
					{
					}

					private static void HandleTrigger(VueDebuggerEvent @event)
					{
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "SubscriptionWatchOptionsModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return store.$subscribe(handleMutation, {");
		StringAssert.Contains(script, "detached: true");
		StringAssert.Contains(script, "flush: \"sync\"");
		StringAssert.Contains(script, "immediate: true");
		StringAssert.Contains(script, "deep: 2");
		StringAssert.Contains(script, "once: true");
		StringAssert.Contains(script, "onTrack: handleTrack");
		StringAssert.Contains(script, "onTrigger: handleTrigger");
	}

	[TestMethod]
	public async Task Convert_ClassUsingSubscriptionMutationVariants_LowersToMutationMetadataProperties()
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

				[ECMAScriptModule("stores/subscription-metadata.mjs")]
				public static class CounterSubscriptionModule
				{
					public static SubscriptionMutationEvents? ReadBaseEvents(SubscriptionMutation<CounterState> mutation)
						=> mutation.Events;

					public static Vue3.VueDebuggerEvent ReadDirectEvent(SubscriptionMutationDirect<CounterState> mutation)
						=> mutation.Events;

					public static Vue3.VueDebuggerEvent[] ReadPatchFunctionEvents(SubscriptionMutationPatchFunction<CounterState> mutation)
						=> mutation.Events;

					public static PiniaStatePatch<CounterState> ReadPatchPayload(SubscriptionMutationPatchObject<CounterState> mutation)
						=> mutation.Payload;
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterSubscriptionModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return mutation.events;");
		StringAssert.Contains(script, "return mutation.payload;");
	}

	[TestMethod]
	public async Task Convert_ClassUsingSubscriptionMutationEventsValueProjection_GeneratesNativeValue()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				[ECMAScriptModule("stores/subscription-events-value.mjs")]
				public static class CounterSubscriptionModule
				{
					public static object? Read(SubscriptionMutationEvents value)
						=> value.Value;
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterSubscriptionModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "return value;");
	}

	[TestMethod]
	public async Task Convert_ClassUsingTypedStatePatch_LowersToPatchObjectCall()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; init; }

					public string Status { get; init; } = "";
				}

				public sealed record CounterStatePatch : PiniaStatePatch<CounterState>
				{
					public int? Count { get; init; }

					public string? Status { get; init; }
				}

				[ECMAScriptModule("stores/patch-counter.mjs")]
				public static class CounterPatchModule
				{
					public static void ApplyPatch(Store<CounterState> store)
						=> store.Patch(new CounterStatePatch
						{
							Count = 7,
							Status = "patched"
						});
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterPatchModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "store.$patch({ count: 7, status: \"patched\" });");
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

	[TestMethod]
	public async Task Convert_ClassUsingHydrationHelpersAndOptionStoreHydrate_LowersToPiniaHydrationContracts()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;
			using static ECMAScript.Vue3;

			namespace Demo
			{
				public sealed record CounterState : PiniaStateTree
				{
					public int Count { get; set; }

					public string Status { get; set; } = "";

					public Vue3.IVueRef<string> ClientOnlyNote { get; set; } = default!;
				}

				[ECMAScriptModule("stores/hydration-counter.mjs")]
				public static class CounterStoreModule
				{
					public static StoreDefinition<Store<CounterState>> CreateStore()
						=> DefineStore<CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState,
							Hydrate = HydrateState
						});

					public static bool ReadHydrationEligibility(CounterState state)
						=> ShouldHydrate(state.ClientOnlyNote);

					private static CounterState CreateState()
					{
						var clientOnlyNote = SkipHydrate(Ref("client"));
						return new CounterState
						{
							Count = 1,
							Status = "ready",
							ClientOnlyNote = clientOnlyNote
						};
					}

					private static void HydrateState(CounterState storeState, CounterState initialState)
					{
						storeState.Count = initialState.Count;
						storeState.Status = initialState.Status;
						storeState.ClientOnlyNote = SkipHydrate(Ref(initialState.Status + " hydrated"));
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterStoreModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { ref } from \"npm:vue@3\";");
		StringAssert.Contains(script, "import { defineStore, shouldHydrate, skipHydrate } from \"pinia\";");
		StringAssert.Contains(script, "return defineStore(\"counter\", {");
		StringAssert.Contains(script, "hydrate: hydrateState");
		StringAssert.Contains(script, "let clientOnlyNote = skipHydrate(ref(\"client\"));");
		StringAssert.Contains(script, "return shouldHydrate(state.clientOnlyNote);");
		StringAssert.Contains(script, "storeState.clientOnlyNote = skipHydrate(ref(initialState.status + \" hydrated\"));");
	}

	[TestMethod]
	public async Task Convert_ClassUsingPiniaRootLifecycleHelpers_LowersToActiveAndDisposeCalls()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				[ECMAScriptModule("stores/pinia-lifecycle.mjs")]
				public static class PiniaLifecycleModule
				{
					public static PiniaInstance PrepareRoot()
					{
						var pinia = CreatePinia();
						SetActivePinia(pinia);
						return GetActivePinia()!;
					}

					public static PiniaInstance? ReleaseRoot()
						=> ClearActivePinia();

					public static void CleanupRoot(PiniaInstance pinia)
						=> DisposePinia(pinia);
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "PiniaLifecycleModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { createPinia, disposePinia, getActivePinia, setActivePinia } from \"pinia\";");
		StringAssert.Contains(script, "let pinia = createPinia();");
		StringAssert.Contains(script, "setActivePinia(pinia);");
		StringAssert.Contains(script, "return getActivePinia();");
		StringAssert.Contains(script, "export function releaseRoot()");
		StringAssert.Contains(script, "return setActivePinia(undefined);");
		StringAssert.Contains(script, "disposePinia(pinia);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingClearActivePiniaOnly_ImportsSetActivePiniaAndLowersUndefinedArgument()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				[ECMAScriptModule("stores/pinia-clear-active.mjs")]
				public static class PiniaLifecycleModule
				{
					public static PiniaInstance? ReleaseRoot()
						=> ClearActivePinia();
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "PiniaLifecycleModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { setActivePinia } from \"pinia\";");
		StringAssert.Contains(script, "return setActivePinia(undefined);");
	}

	[TestMethod]
	public async Task Convert_ClassUsingClearActivePiniaWithShadowedLocalName_UsesAliasedImportBinding()
	{
		var code = """
			using ECMAScript;
			using static ECMAScript.Pinia;

			namespace Demo
			{
				[ECMAScriptModule("stores/pinia-clear-active-shadowed.mjs")]
				public static class PiniaLifecycleModule
				{
					public static string ReleaseRoot()
					{
						var setActivePinia = "local";
						ClearActivePinia();
						return setActivePinia;
					}
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "PiniaLifecycleModule");
		var importBinding = ImportBindingName("pinia", "setActivePinia");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, $"import {{ setActivePinia as {importBinding} }} from \"pinia\";");
		StringAssert.Contains(script, $"{importBinding}(undefined);");
		StringAssert.Contains(script, "let setActivePinia = \"local\";");
	}

	[TestMethod]
	public async Task Convert_ClassUsingExplicitMultiRootStoreResolution_KeepsPiniaIsolationCalls()
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

				public abstract class CounterStore : Store<CounterState>
				{
					public extern int Count { get; set; }

					public extern void Increment();
				}

				public sealed record CounterPluginExtensions : Vue3.VueProps
				{
					public string AuditTag { get; init; } = "";
				}

				public sealed record CounterPluginState : PiniaStateTree
				{
					public string PersistedAt { get; init; } = "";
				}

				[ECMAScriptModule("stores/pinia-isolation.mjs")]
				public static class CounterIsolationModule
				{
					public static string CompareRoots(PiniaInstance leftPinia, PiniaInstance rightPinia)
					{
						var useCounterStore = DefineStore<CounterStore, CounterState>("counter", new DefineStoreOptions<CounterState>
						{
							State = CreateState
						});
						var projectedUseCounterStore = ProjectStoreDefinition<CounterStore, CounterPluginExtensions, CounterPluginState>(useCounterStore);
						var leftStore = useCounterStore.Use(leftPinia);
						var rightStore = useCounterStore.Use(rightPinia);
						var leftProjected = projectedUseCounterStore.Use(leftPinia);
						var rightProjected = projectedUseCounterStore.Use(rightPinia);

						leftStore.Increment();
						return leftStore.Count
							+ "|"
							+ rightStore.Count
							+ "|"
							+ leftProjected.AsCustomProperties().AuditTag
							+ "|"
							+ rightProjected.AsCustomState().PersistedAt;
					}

					private static CounterState CreateState()
						=> new CounterState
						{
							Count = 2
						};
				}
			}
			""";

		var script = await ConvertModuleAsync(code, "CounterIsolationModule");

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "import { defineStore } from \"pinia\";");
		StringAssert.Contains(script, "let projectedUseCounterStore = useCounterStore;");
		StringAssert.Contains(script, "let leftStore = useCounterStore(leftPinia);");
		StringAssert.Contains(script, "let rightStore = useCounterStore(rightPinia);");
		StringAssert.Contains(script, "let leftProjected = projectedUseCounterStore(leftPinia);");
		StringAssert.Contains(script, "let rightProjected = projectedUseCounterStore(rightPinia);");
		StringAssert.Contains(script, "leftStore.increment();");
		StringAssert.Contains(script, "return leftStore.count + \"|\" + rightStore.count + \"|\" + leftProjected.auditTag + \"|\" + rightProjected.$state.persistedAt;");
	}
}
