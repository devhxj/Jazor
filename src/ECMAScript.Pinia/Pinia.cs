using System;
using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Callback returned by Pinia subscription and action-listener registration APIs.
/// Calling it detaches the previously registered listener.
/// </summary>
public delegate void PiniaDetachCallback();

/// <summary>
/// Callback passed to <c>$patch((state) =&gt; ...)</c>.
/// </summary>
/// <typeparam name="TState">The typed store-state projection.</typeparam>
/// <param name="state">The current mutable store state.</param>
public delegate void PiniaStatePatchCallback<TState>(TState state)
	where TState : Pinia.PiniaStateTree;

/// <summary>
/// Option-store hydration hook invoked with the live store state and the incoming
/// initial state during SSR/client hydration.
/// </summary>
/// <typeparam name="TState">The typed store-state projection.</typeparam>
/// <param name="storeState">The current live store state object.</param>
/// <param name="initialState">The initial serialized state being hydrated into the store.</param>
public delegate void PiniaHydrateCallback<TState>(TState storeState, TState initialState)
	where TState : Pinia.PiniaStateTree;

/// <summary>
/// Callback used by <c>$subscribe()</c>.
/// </summary>
/// <typeparam name="TState">The typed store-state projection.</typeparam>
/// <param name="mutation">Metadata describing the mutation that triggered the subscription callback.</param>
/// <param name="state">The current store state after the mutation completed.</param>
public delegate void PiniaSubscriptionCallback<TState>(Pinia.SubscriptionMutation<TState> mutation, TState state)
	where TState : Pinia.PiniaStateTree;

/// <summary>
/// Untyped action-listener callback used by <c>$onAction()</c>.
/// </summary>
/// <param name="context">The action listener context describing the current action invocation.</param>
public delegate void PiniaStoreActionListener(Pinia.StoreActionListenerContext context);

/// <summary>
/// Typed action-listener callback used by <c>$onAction()</c>.
/// </summary>
/// <typeparam name="TStore">The typed store projection supplied by the listener context.</typeparam>
/// <param name="context">The typed action listener context describing the current action invocation.</param>
public delegate void PiniaStoreActionListener<TStore>(Pinia.StoreActionListenerContext<TStore> context)
	where TStore : class;

/// <summary>
/// Custom selector callback used by object-form <c>mapState()</c> / <c>mapGetters()</c>.
/// </summary>
/// <typeparam name="TStore">The typed store projection supplied by the store definition.</typeparam>
/// <param name="store">The typed store instance.</param>
/// <returns>The mapped value that should back the computed entry.</returns>
public delegate Pinia.PiniaValue? PiniaMapStateSelector<TStore>(TStore store)
	where TStore : class;

/// <summary>
/// Pinia plugin callback registered through <c>pinia.use(...)</c>.
/// </summary>
/// <param name="context">The plugin context containing the app, pinia instance, store, and defining options.</param>
/// <returns>Optional additional properties to merge into the store instance.</returns>
public delegate Vue3.VueProps? PiniaPlugin(Pinia.PiniaPluginContext context);

/// <summary>
/// Hot-module-replacement accept callback returned by <c>acceptHMRUpdate()</c>.
/// </summary>
/// <param name="newModule">The new hot module object supplied by the host HMR runtime.</param>
public delegate void PiniaHotUpdateHandler(IObject newModule);

[ECMAScript("pinia")]
[Description("@#")]
public static partial class Pinia
{
}
