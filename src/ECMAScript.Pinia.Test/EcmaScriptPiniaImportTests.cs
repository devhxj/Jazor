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

					public static CounterState ReadPatchPayload(SubscriptionMutationPatchObject<CounterState> mutation)
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
