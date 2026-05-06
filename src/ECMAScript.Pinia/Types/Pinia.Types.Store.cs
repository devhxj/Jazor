using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	/// <summary>
	/// Pinia root instance created by <c>createPinia()</c>. The same object is both a
	/// Pinia runtime root and a Vue plugin install target.
	/// </summary>
	public abstract record PiniaInstance : Vue3.VuePlugin
	{
		/// <summary>
		/// Pinia's root state tree, keyed by store id.
		/// </summary>
		[Description("@#state")]
		public extern Vue3.IVueRef<Vue3.VueDictionary<PiniaStateTree>> State { get; }

		/// <summary>
		/// Registers a Pinia plugin on this root instance.
		/// </summary>
		/// <param name="plugin">The plugin callback to register.</param>
		/// <returns>The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use(PiniaPlugin plugin);

		/// <summary>
		/// Registers a typed Pinia plugin on this root instance.
		/// </summary>
		/// <typeparam name="TStore">The typed store projection supplied to the plugin context.</typeparam>
		/// <param name="plugin">The typed plugin callback to register.</param>
		/// <returns>The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore>(PiniaPlugin<TStore> plugin)
			where TStore : class;

		/// <summary>
		/// Registers a typed Pinia plugin on this root instance with a strongly typed
		/// plugin-visible options projection.
		/// </summary>
		/// <typeparam name="TStore">The typed store projection supplied to the plugin context.</typeparam>
		/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
		/// <param name="plugin">The typed plugin callback to register.</param>
		/// <returns>The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions>(PiniaPlugin<TStore, TOptions> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin;

		/// <summary>
		/// Registers a fully typed Pinia plugin on this root instance, including the
		/// merged extension-object return shape.
		/// </summary>
		/// <typeparam name="TStore">The typed store projection supplied to the plugin context.</typeparam>
		/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
		/// <typeparam name="TExtension">The typed extension object returned by the plugin.</typeparam>
		/// <param name="plugin">The typed plugin callback to register.</param>
		/// <returns>The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions, TExtension>(PiniaPlugin<TStore, TOptions, TExtension> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin
			where TExtension : Vue3.VueProps;

		/// <summary>
		/// Registers a fully typed Pinia plugin on this root instance whose context also
		/// projects the current store to explicit custom-properties visible from earlier
		/// plugins.
		/// </summary>
		/// <typeparam name="TStore">The base typed store projection supplied by the plugin context.</typeparam>
		/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
		/// <typeparam name="TCustomProperties">The plugin-added custom store properties already visible on the current store.</typeparam>
		/// <typeparam name="TExtension">The typed extension object returned by the plugin.</typeparam>
		/// <param name="plugin">The typed plugin callback to register.</param>
		/// <returns>The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions, TCustomProperties, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TExtension> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin
			where TCustomProperties : Vue3.VueProps
			where TExtension : Vue3.VueProps;

		/// <summary>
		/// Registers a fully typed Pinia plugin on this root instance whose context also
		/// projects the current store to explicit custom-properties and custom-state
		/// views visible from earlier plugins.
		/// </summary>
		/// <typeparam name="TStore">The base typed store projection supplied by the plugin context.</typeparam>
		/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
		/// <typeparam name="TCustomProperties">The plugin-added custom store properties already visible on the current store.</typeparam>
		/// <typeparam name="TCustomState">The plugin-added custom state already visible on <c>store.$state</c>.</typeparam>
		/// <typeparam name="TExtension">The typed extension object returned by the plugin.</typeparam>
		/// <param name="plugin">The typed plugin callback to register.</param>
		/// <returns>The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions, TCustomProperties, TCustomState, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TCustomState, TExtension> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin
			where TCustomProperties : Vue3.VueProps
			where TCustomState : PiniaStateTree
			where TExtension : Vue3.VueProps;
	}

	/// <summary>
	/// Plugin context passed to <c>pinia.use(...)</c>.
	/// </summary>
	public abstract class PiniaPluginContext
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// The Vue application instance this Pinia root was installed on.
		/// </summary>
		[Description("@#app")]
		public extern Vue3.VueApp App { get; }

		/// <summary>
		/// The Pinia root instance currently invoking the plugin.
		/// </summary>
		[Description("@#pinia")]
		public extern PiniaInstance Pinia { get; }

		/// <summary>
		/// The concrete store currently being extended.
		/// </summary>
		[Description("@#store")]
		public extern StoreGeneric Store { get; }

		/// <summary>
		/// The defining options object for the current store.
		/// </summary>
		[Description("@#options")]
		public extern DefineStoreOptionsInPlugin Options { get; }
	}

	/// <summary>
	/// Typed plugin context passed to <c>pinia.use(...)</c> when the current store is
	/// projected to a stronger user-declared type.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection supplied by the plugin context.</typeparam>
	public abstract class PiniaPluginContext<TStore> : PiniaPluginContext
		where TStore : class
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// The concrete store currently being extended, projected to a stronger
		/// user-declared type.
		/// </summary>
		[Description("@#store")]
		public new extern TStore Store { get; }
	}

	/// <summary>
	/// Typed plugin context passed to <c>pinia.use(...)</c> when both the current store
	/// and the plugin-visible options bag are projected to stronger user-declared types.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
	public abstract class PiniaPluginContext<TStore, TOptions> : PiniaPluginContext<TStore>
		where TStore : class
		where TOptions : DefineStoreOptionsInPlugin
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// The defining options object for the current store, projected to a stronger
		/// user-declared plugin-visible type.
		/// </summary>
		[Description("@#options")]
		public new extern TOptions Options { get; }
	}

	/// <summary>
	/// Typed plugin context passed to <c>pinia.use(...)</c> when the current store
	/// should also be viewed through explicit plugin-added custom properties that were
	/// installed by earlier plugins.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties already visible on the current store.</typeparam>
	public abstract class PiniaPluginContext<TStore, TOptions, TCustomProperties> : PiniaPluginContext<TStore, TOptions>
		where TStore : class
		where TOptions : DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// The concrete store currently being extended, projected to both its base store
		/// contract and the plugin-added custom-properties view.
		/// </summary>
		[Description("@#store")]
		public new extern ProjectedStore<TStore, TCustomProperties> Store { get; }
	}

	/// <summary>
	/// Typed plugin context passed to <c>pinia.use(...)</c> when the current store
	/// should also be viewed through explicit plugin-added custom properties and
	/// custom state installed by earlier plugins.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties already visible on the current store.</typeparam>
	/// <typeparam name="TCustomState">The plugin-added custom state already visible on <c>store.$state</c>.</typeparam>
	public abstract class PiniaPluginContext<TStore, TOptions, TCustomProperties, TCustomState> : PiniaPluginContext<TStore, TOptions, TCustomProperties>
		where TStore : class
		where TOptions : DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// The concrete store currently being extended, projected to both its base store
		/// contract and the plugin-added custom-properties/custom-state views.
		/// </summary>
		[Description("@#store")]
		public new extern ProjectedStore<TStore, TCustomProperties, TCustomState> Store { get; }
	}

	/// <summary>
	/// Store properties shared across every Pinia store instance.
	/// </summary>
	public abstract class StoreProperties
	{
		protected StoreProperties()
		{
		}

		/// <summary>
		/// Store identifier.
		/// </summary>
		[Description("@#$id")]
		public extern string Id { get; }

		/// <summary>
		/// Custom store properties registered by Pinia plugins.
		/// </summary>
		[Description("@#_customProperties")]
		public extern Set<string> CustomProperties { get; }
	}

	/// <summary>
	/// Minimal store runtime-shape base shared by every Pinia store instance.
	/// </summary>
	public abstract class StoreGeneric : StoreProperties
	{
		protected StoreGeneric()
		{
		}

		/// <summary>
		/// Registers an action listener for this store.
		/// </summary>
		/// <param name="callback">The listener callback.</param>
		/// <returns>A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction(PiniaStoreActionListener callback);

		/// <summary>
		/// Registers a typed action listener for this store.
		/// </summary>
		/// <typeparam name="TStore">The typed store projection supplied to the listener context.</typeparam>
		/// <param name="callback">The typed listener callback.</param>
		/// <returns>A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction<TStore>(PiniaStoreActionListener<TStore> callback)
			where TStore : class;

		/// <summary>
		/// Registers an action listener detached from the current component scope.
		/// </summary>
		/// <param name="callback">The listener callback.</param>
		/// <param name="detached">Whether the listener should outlive the current component scope.</param>
		/// <returns>A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction(PiniaStoreActionListener callback, bool detached);

		/// <summary>
		/// Registers a typed action listener detached from the current component scope.
		/// </summary>
		/// <typeparam name="TStore">The typed store projection supplied to the listener context.</typeparam>
		/// <param name="callback">The typed listener callback.</param>
		/// <param name="detached">Whether the listener should outlive the current component scope.</param>
		/// <returns>A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction<TStore>(PiniaStoreActionListener<TStore> callback, bool detached)
			where TStore : class;

		/// <summary>
		/// Disposes the store instance and tears down its reactive scope.
		/// </summary>
		[Description("@#$dispose")]
		public extern void Dispose();
	}

	/// <summary>
	/// Typed Pinia store base exposing the common <c>$state</c> / <c>$patch</c> /
	/// <c>$reset</c> / <c>$subscribe</c> surface.
	/// </summary>
	/// <typeparam name="TState">The typed store-state projection.</typeparam>
	public abstract class Store<TState> : StoreGeneric
		where TState : PiniaStateTree
	{
		protected Store()
		{
		}

		/// <summary>
		/// Current live store state.
		/// </summary>
		[Description("@#$state")]
		public extern TState State { get; set; }

		/// <summary>
		/// Applies a partial object patch to the current store state.
		/// </summary>
		/// <param name="partialState">The partial state object to merge into the store.</param>
		[Description("@#$patch")]
		public extern void Patch(TState partialState);

		/// <summary>
		/// Applies a function patch to the current store state.
		/// </summary>
		/// <param name="patcher">The callback that mutates the current state in place.</param>
		[Description("@#$patch")]
		public extern void Patch(PiniaStatePatchCallback<TState> patcher);

		/// <summary>
		/// Resets the store state back to the original <c>state()</c> factory value.
		/// </summary>
		[Description("@#$reset")]
		public extern void Reset();

		/// <summary>
		/// Subscribes to store state mutations.
		/// </summary>
		/// <param name="callback">The subscription callback.</param>
		/// <returns>A callback that detaches the subscription.</returns>
		[Description("@#$subscribe")]
		public extern PiniaDetachCallback Subscribe(PiniaSubscriptionCallback<TState> callback);

		/// <summary>
		/// Subscribes to store state mutations with explicit subscription options.
		/// </summary>
		/// <param name="callback">The subscription callback.</param>
		/// <param name="options">Subscription options controlling watcher flush behavior and detach scope.</param>
		/// <returns>A callback that detaches the subscription.</returns>
		[Description("@#$subscribe")]
		public extern PiniaDetachCallback Subscribe(PiniaSubscriptionCallback<TState> callback, SubscribeOptions options);
	}

	/// <summary>
	/// Dev-only mutation debug events supplied to <c>$subscribe()</c>.
	/// Pinia reports either one debugger event or an event batch depending on the
	/// mutation kind.
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct SubscriptionMutationEvents
	{
		private readonly byte _kind;
		private readonly Vue3.VueDebuggerEvent? _event;
		private readonly Vue3.VueDebuggerEvent[]? _batch;

		private SubscriptionMutationEvents(Vue3.VueDebuggerEvent value)
		{
			_kind = 1;
			_event = value;
			_batch = default;
		}

		private SubscriptionMutationEvents(Vue3.VueDebuggerEvent[] value)
		{
			_kind = 2;
			_event = default;
			_batch = value;
		}

		public Vue3.VueDebuggerEvent? AsEvent => _kind == 1 ? _event : default;

		public Vue3.VueDebuggerEvent[]? AsBatch => _kind == 2 ? _batch : default;

		public static implicit operator SubscriptionMutationEvents(Vue3.VueDebuggerEvent value)
			=> new(value);

		public static implicit operator SubscriptionMutationEvents(Vue3.VueDebuggerEvent[] value)
			=> new(value);
	}

	/// <summary>
	/// Base mutation metadata supplied to <c>$subscribe()</c>.
	/// Concrete runtime shapes are modeled by
	/// <see cref="SubscriptionMutationDirect{TState}"/>,
	/// <see cref="SubscriptionMutationPatchFunction{TState}"/>, and
	/// <see cref="SubscriptionMutationPatchObject{TState}"/>.
	/// </summary>
	/// <typeparam name="TState">The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutation()
		{
		}

		/// <summary>
		/// The kind of mutation that triggered the callback.
		/// </summary>
		[Description("@#type")]
		public extern MutationType Type { get; }

		/// <summary>
		/// The store id that triggered the callback.
		/// </summary>
		[Description("@#storeId")]
		public extern string StoreId { get; }

		/// <summary>
		/// Dev-only debugger events emitted for the current mutation.
		/// Depending on mutation kind this can be a single event or an event batch.
		/// </summary>
		[Description("@#events")]
		public extern SubscriptionMutationEvents? Events { get; }
	}

	/// <summary>
	/// Direct assignment mutation metadata supplied to <c>$subscribe()</c>.
	/// </summary>
	/// <typeparam name="TState">The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutationDirect<TState> : SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutationDirect()
		{
		}

		/// <summary>
		/// The debugger event emitted for the direct assignment.
		/// </summary>
		[Description("@#events")]
		public new extern Vue3.VueDebuggerEvent Events { get; }
	}

	/// <summary>
	/// Function-patch mutation metadata supplied to <c>$subscribe()</c>.
	/// </summary>
	/// <typeparam name="TState">The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutationPatchFunction<TState> : SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutationPatchFunction()
		{
		}

		/// <summary>
		/// The debugger events emitted for the function patch.
		/// </summary>
		[Description("@#events")]
		public new extern Vue3.VueDebuggerEvent[] Events { get; }
	}

	/// <summary>
	/// Object-patch mutation metadata supplied to <c>$subscribe()</c>.
	/// </summary>
	/// <typeparam name="TState">The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutationPatchObject<TState> : SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutationPatchObject()
		{
		}

		/// <summary>
		/// The object patch payload applied to the store.
		/// </summary>
		[Description("@#payload")]
		public extern TState Payload { get; }

		/// <summary>
		/// The debugger events emitted for the object patch.
		/// </summary>
		[Description("@#events")]
		public new extern Vue3.VueDebuggerEvent[] Events { get; }
	}

	/// <summary>
	/// Untyped action listener context supplied to <c>$onAction()</c>.
	/// </summary>
	public abstract class StoreActionListenerContext
	{
		protected StoreActionListenerContext()
		{
		}

		/// <summary>
		/// The action name being invoked.
		/// </summary>
		[Description("@#name")]
		public extern string Name { get; }

		/// <summary>
		/// The raw action arguments passed by the caller.
		/// </summary>
		[Description("@#args")]
		public extern PiniaValue[] Args { get; }

		/// <summary>
		/// Registers a callback that runs after the action completes.
		/// </summary>
		/// <param name="callback">The callback to invoke after action completion.</param>
		[Description("@#after")]
		public extern void After(Action callback);

		/// <summary>
		/// Registers a callback that receives the action result after the action completes.
		/// </summary>
		/// <param name="callback">The callback to invoke after action completion.</param>
		[Description("@#after")]
		public extern void After(Action<PiniaValue?> callback);

		/// <summary>
		/// Registers a callback that runs when the action throws.
		/// </summary>
		/// <param name="callback">The callback to invoke when the action throws.</param>
		[Description("@#onError")]
		public extern void OnError(Action<Error> callback);
	}

	/// <summary>
	/// Typed action listener context supplied to <c>$onAction()</c>.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection.</typeparam>
	public abstract class StoreActionListenerContext<TStore> : StoreActionListenerContext
		where TStore : class
	{
		protected StoreActionListenerContext()
		{
		}

		/// <summary>
		/// The concrete store instance invoking the action.
		/// </summary>
		[Description("@#store")]
		public extern TStore Store { get; }
	}

	/// <summary>
	/// Non-generic store-definition base used by helper APIs such as <c>mapStores()</c>
	/// that accept heterogeneous store-definition lists.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class StoreDefinition
	{
		protected StoreDefinition()
		{
		}
	}

	/// <summary>
	/// Callable store-definition wrapper returned by <c>defineStore()</c>.
	/// Pinia exposes this as a function object; C# wraps the call surface in explicit
	/// <c>Use(...)</c> methods so the API stays discoverable and does not rely on
	/// compiler-specific function-object magic.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection returned by the wrapper.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class StoreDefinition<TStore> : StoreDefinition
		where TStore : class
	{
		protected StoreDefinition()
		{
		}

		/// <summary>
		/// Store identifier declared at definition time.
		/// </summary>
		[Description("@#$id")]
		public extern string Id { get; }

		/// <summary>
		/// Creates or retrieves the store instance from the currently active Pinia root.
		/// </summary>
		/// <returns>The concrete store instance.</returns>
		[ECMAScriptInline("__arg1()")]
		public extern TStore Use();

		/// <summary>
		/// Creates or retrieves the store instance from the supplied Pinia root.
		/// </summary>
		/// <param name="pinia">The Pinia root instance to resolve the store against.</param>
		/// <returns>The concrete store instance.</returns>
		[ECMAScriptInline("__arg1(__arg2)")]
		public extern TStore Use(PiniaInstance pinia);

		/// <summary>
		/// Internal/advanced call shape used by Pinia HMR flows.
		/// </summary>
		/// <param name="pinia">The Pinia root instance to resolve the store against.</param>
		/// <param name="hot">The existing hot store instance supplied by the HMR runtime.</param>
		/// <returns>The concrete store instance.</returns>
		[ECMAScriptInline("__arg1(__arg2, __arg3)")]
		public extern TStore Use(PiniaInstance pinia, StoreGeneric hot);
	}

	/// <summary>
	/// Explicit projected view over a live Pinia store when plugins add custom
	/// properties.
	/// This wrapper does not create a new runtime object; it only exposes
	/// additional typed views over the same store instance.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties projection.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStore<TStore, TCustomProperties>
		where TStore : class
		where TCustomProperties : Vue3.VueProps
	{
		protected ProjectedStore()
		{
		}

		/// <summary>
		/// Returns the same runtime store projected to its base store contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern TStore AsStore();

		/// <summary>
		/// Returns the same runtime store projected to the plugin-added custom
		/// properties contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern TCustomProperties AsCustomProperties();
	}

	/// <summary>
	/// Explicit projected view over a live Pinia store when plugins add both
	/// custom store properties and custom state properties.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties projection.</typeparam>
	/// <typeparam name="TCustomState">The plugin-added custom state projection on <c>store.$state</c>.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStore<TStore, TCustomProperties, TCustomState> : ProjectedStore<TStore, TCustomProperties>
		where TStore : class
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree
	{
		protected ProjectedStore()
		{
		}

		/// <summary>
		/// Returns the current <c>store.$state</c> object projected to the
		/// plugin-added custom state contract.
		/// </summary>
		[ECMAScriptInline("__arg1.$state")]
		public extern TCustomState AsCustomState();
	}

	/// <summary>
	/// Explicit projected view over a store definition when plugin-added custom
	/// properties should propagate through <c>Use(...)</c>.
	/// This wrapper keeps the same runtime store definition function object and only
	/// changes the typed result of the call surface.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties projection.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStoreDefinition<TStore, TCustomProperties> : StoreDefinition
		where TStore : class
		where TCustomProperties : Vue3.VueProps
	{
		protected ProjectedStoreDefinition()
		{
		}

		/// <summary>
		/// Store identifier declared at definition time.
		/// </summary>
		[Description("@#$id")]
		public extern string Id { get; }

		/// <summary>
		/// Returns the same runtime store definition projected back to the base
		/// typed store-definition contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern StoreDefinition<TStore> AsDefinition();

		/// <summary>
		/// Creates or retrieves the projected store instance from the currently
		/// active Pinia root.
		/// </summary>
		[ECMAScriptInline("__arg1()")]
		public extern ProjectedStore<TStore, TCustomProperties> Use();

		/// <summary>
		/// Creates or retrieves the projected store instance from the supplied Pinia root.
		/// </summary>
		[ECMAScriptInline("__arg1(__arg2)")]
		public extern ProjectedStore<TStore, TCustomProperties> Use(PiniaInstance pinia);

		/// <summary>
		/// Internal/advanced call shape used by Pinia HMR flows.
		/// </summary>
		[ECMAScriptInline("__arg1(__arg2, __arg3)")]
		public extern ProjectedStore<TStore, TCustomProperties> Use(PiniaInstance pinia, StoreGeneric hot);
	}

	/// <summary>
	/// Explicit projected view over a store definition when plugin-added custom
	/// properties and custom state should propagate through <c>Use(...)</c>.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties projection.</typeparam>
	/// <typeparam name="TCustomState">The plugin-added custom state projection on <c>store.$state</c>.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStoreDefinition<TStore, TCustomProperties, TCustomState> : StoreDefinition
		where TStore : class
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree
	{
		protected ProjectedStoreDefinition()
		{
		}

		/// <summary>
		/// Store identifier declared at definition time.
		/// </summary>
		[Description("@#$id")]
		public extern string Id { get; }

		/// <summary>
		/// Returns the same runtime store definition projected back to the base
		/// typed store-definition contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern StoreDefinition<TStore> AsDefinition();

		/// <summary>
		/// Creates or retrieves the projected store instance from the currently
		/// active Pinia root.
		/// </summary>
		[ECMAScriptInline("__arg1()")]
		public extern ProjectedStore<TStore, TCustomProperties, TCustomState> Use();

		/// <summary>
		/// Creates or retrieves the projected store instance from the supplied Pinia root.
		/// </summary>
		[ECMAScriptInline("__arg1(__arg2)")]
		public extern ProjectedStore<TStore, TCustomProperties, TCustomState> Use(PiniaInstance pinia);

		/// <summary>
		/// Internal/advanced call shape used by Pinia HMR flows.
		/// </summary>
		[ECMAScriptInline("__arg1(__arg2, __arg3)")]
		public extern ProjectedStore<TStore, TCustomProperties, TCustomState> Use(PiniaInstance pinia, StoreGeneric hot);
	}
}
