using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	/// <summary>
	/// Creates a Pinia root instance that can be installed on a Vue app via
	/// <c>app.use(createPinia())</c>.
	/// </summary>
	[Description("@#createPinia")]
	public extern static PiniaInstance CreatePinia();

	/// <summary>
	/// Defines an option-style store and returns the callable store-definition wrapper.
	/// </summary>
	/// <typeparam name="TState">The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	/// <param name="id">The unique store identifier.</param>
	/// <param name="options">The option-style store definition object.</param>
	/// <returns>A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<Store<TState>> DefineStore<TState>(string id, DefineStoreOptions<TState> options)
		where TState : PiniaStateTree;

	/// <summary>
	/// Defines an option-style store while projecting the runtime result to a stronger
	/// user-declared store type.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection returned by the wrapper.</typeparam>
	/// <typeparam name="TState">The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	/// <param name="id">The unique store identifier.</param>
	/// <param name="options">The option-style store definition object.</param>
	/// <returns>A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore, TState>(string id, DefineStoreOptions<TState> options)
		where TStore : class
		where TState : PiniaStateTree;

	/// <summary>
	/// Defines a setup-style store from a parameterless setup callback.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection returned by the setup callback.</typeparam>
	/// <param name="id">The unique store identifier.</param>
	/// <param name="storeSetup">The setup callback that declares the store surface.</param>
	/// <returns>A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore>(string id, Func<TStore> storeSetup)
		where TStore : class;

	/// <summary>
	/// Defines a setup-style store from a parameterless setup callback plus setup-store options.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection returned by the setup callback.</typeparam>
	/// <param name="id">The unique store identifier.</param>
	/// <param name="storeSetup">The setup callback that declares the store surface.</param>
	/// <param name="options">Additional setup-store options.</param>
	/// <returns>A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore>(string id, Func<TStore> storeSetup, DefineSetupStoreOptions options)
		where TStore : class;

	/// <summary>
	/// Returns the currently active Pinia root instance, if one has been set.
	/// </summary>
	[Description("@#getActivePinia")]
	public extern static PiniaInstance? GetActivePinia();

	/// <summary>
	/// Sets the currently active Pinia root instance.
	/// </summary>
	/// <param name="pinia">The Pinia instance that should become active for subsequent store resolution.</param>
	/// <returns>The same <paramref name="pinia"/> instance.</returns>
	[Description("@#setActivePinia")]
	public extern static PiniaInstance SetActivePinia(PiniaInstance pinia);

	/// <summary>
	/// Disposes a Pinia root instance and every store attached to it.
	/// </summary>
	/// <param name="pinia">The Pinia instance to dispose.</param>
	[Description("@#disposePinia")]
	public extern static void DisposePinia(PiniaInstance pinia);

	/// <summary>
	/// Marks an object so Pinia skips hydration for it.
	/// </summary>
	/// <typeparam name="T">The object type being marked.</typeparam>
	/// <param name="value">The object to mark.</param>
	/// <returns>The same <paramref name="value"/> instance.</returns>
	[Description("@#skipHydrate")]
	public extern static T SkipHydrate<T>(T value)
		where T : class;

	/// <summary>
	/// Returns whether Pinia should hydrate the supplied runtime value.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value should participate in hydration.</returns>
	[Description("@#shouldHydrate")]
	public extern static bool ShouldHydrate<T>(T value);

	/// <summary>
	/// Converts a store's reactive state and getters into refs using the default
	/// indexer-based refs bag.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection.</typeparam>
	/// <param name="store">The store instance.</param>
	/// <returns>A refs bag mirroring the store's reactive members.</returns>
	[Description("@#storeToRefs")]
	public extern static StoreRefs<TStore> StoreToRefs<TStore>(TStore store)
		where TStore : class;

	/// <summary>
	/// Converts a store's reactive state and getters into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">The user-declared refs projection type.</typeparam>
	/// <typeparam name="TStore">The typed store projection.</typeparam>
	/// <param name="store">The store instance.</param>
	/// <returns>The typed refs projection produced by Pinia.</returns>
	[Description("@#storeToRefs")]
	public extern static TRefs StoreToRefs<TRefs, TStore>(TStore store)
		where TRefs : StoreRefs<TStore>
		where TStore : class;

	/// <summary>
	/// Creates an HMR accept handler for a previously declared store definition.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection returned by the store definition.</typeparam>
	/// <param name="initialUseStore">The original store definition wrapper.</param>
	/// <param name="hot">The host hot-module object (for example, Vite's <c>import.meta.hot</c>).</param>
	/// <returns>A callback that should be wired into the host HMR accept flow.</returns>
	[Description("@#acceptHMRUpdate")]
	public extern static PiniaHotUpdateHandler AcceptHMRUpdate<TStore>(StoreDefinition<TStore> initialUseStore, IObject hot)
		where TStore : class;
}
