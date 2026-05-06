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
		public extern DefineStoreOptionsBase Options { get; }
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
		/// Registers an action listener detached from the current component scope.
		/// </summary>
		/// <param name="callback">The listener callback.</param>
		/// <param name="detached">Whether the listener should outlive the current component scope.</param>
		/// <returns>A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction(PiniaStoreActionListener callback, bool detached);

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
	/// Mutation metadata supplied to <c>$subscribe()</c>.
	/// For <see cref="MutationType.PatchObject"/> the <see cref="Payload"/> member
	/// carries the patch object; for other mutation kinds it remains null-like.
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
		/// The object patch payload when <see cref="Type"/> is
		/// <see cref="MutationType.PatchObject"/>.
		/// </summary>
		[Description("@#payload")]
		public extern TState? Payload { get; }
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
	/// Callable store-definition wrapper returned by <c>defineStore()</c>.
	/// Pinia exposes this as a function object; C# wraps the call surface in explicit
	/// <c>Use(...)</c> methods so the API stays discoverable and does not rely on
	/// compiler-specific function-object magic.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection returned by the wrapper.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class StoreDefinition<TStore>
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
}
