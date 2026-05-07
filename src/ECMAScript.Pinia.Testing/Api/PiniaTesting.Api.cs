using System.ComponentModel;

namespace ECMAScript;

public static partial class PiniaTesting
{
	private const string IdentityInlineTemplate = "__arg1";

	/// <summary>
	/// Creates a Pinia root instance configured for component/unit testing.
	/// This mirrors <c>createTestingPinia(options?)</c> from <c>@pinia/testing</c>.
	/// </summary>
	/// <param name="options">Optional testing configuration.</param>
	/// <returns>A Pinia instance suitable for test-time store resolution.</returns>
	[Description("@#createTestingPinia")]
	public extern static TestingPinia CreateTestingPinia();

	/// <summary>
	/// Creates a Pinia root instance configured for component/unit testing.
	/// This mirrors <c>createTestingPinia(options?)</c> from <c>@pinia/testing</c>.
	/// </summary>
	/// <param name="options">Optional testing configuration.</param>
	/// <returns>A Pinia instance suitable for test-time store resolution.</returns>
	[Description("@#createTestingPinia")]
	public extern static TestingPinia CreateTestingPinia(TestingOptions options);

	/// <summary>
	/// Projects a typed Pinia plugin callback to the untyped runtime plugin shape
	/// required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection supplied by the plugin context.</typeparam>
	/// <param name="plugin">The typed plugin callback to project.</param>
	/// <returns>The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore>(PiniaPlugin<TStore> plugin)
		where TStore : class;

	/// <summary>
	/// Projects a typed <c>stubActions</c> predicate to the untyped runtime predicate
	/// shape required by <see cref="TestingStubActions"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection supplied to the predicate.</typeparam>
	/// <param name="predicate">The typed stub-action predicate to project.</param>
	/// <returns>The same runtime predicate projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaTestingStubActionPredicate ProjectStubActionPredicate<TStore>(PiniaTestingStubActionPredicate<TStore> predicate)
		where TStore : class;

	/// <summary>
	/// Projects a typed <c>stubActions</c> predicate directly to the typed testing
	/// union surface used by <see cref="TestingOptions{TDelegate,TStore}.StubActions"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// predicate or wrapper object.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection supplied to the predicate.</typeparam>
	/// <param name="predicate">The typed stub-action predicate to project.</param>
	/// <returns>The same runtime predicate projected to the typed <c>stubActions</c> union surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static TestingStubActions<TStore> ProjectStubActions<TStore>(PiniaTestingStubActionPredicate<TStore> predicate)
		where TStore : class;

	/// <summary>
	/// Projects a typed Pinia plugin callback with typed store-definition options to
	/// the untyped runtime plugin shape required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
	/// <param name="plugin">The typed plugin callback to project.</param>
	/// <returns>The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions>(PiniaPlugin<TStore, TOptions> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin;

	/// <summary>
	/// Projects a fully typed Pinia plugin callback whose merged extension object is
	/// also typed to the untyped runtime plugin shape required by
	/// <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">The typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TExtension">The typed extension object returned by the plugin.</typeparam>
	/// <param name="plugin">The typed plugin callback to project.</param>
	/// <returns>The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions, TExtension>(PiniaPlugin<TStore, TOptions, TExtension> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin
		where TExtension : Vue3.VueProps;

	/// <summary>
	/// Projects a typed Pinia plugin callback whose context also exposes explicit
	/// plugin-added custom store properties to the untyped runtime plugin shape
	/// required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties already visible on the current store.</typeparam>
	/// <typeparam name="TExtension">The typed extension object returned by the plugin.</typeparam>
	/// <param name="plugin">The typed plugin callback to project.</param>
	/// <returns>The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions, TCustomProperties, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TExtension> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
		where TExtension : Vue3.VueProps;

	/// <summary>
	/// Projects a typed Pinia plugin callback whose context also exposes explicit
	/// plugin-added custom store properties and custom state to the untyped runtime
	/// plugin shape required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">The plugin-added custom store properties already visible on the current store.</typeparam>
	/// <typeparam name="TCustomState">The plugin-added custom state already visible on <c>store.$state</c>.</typeparam>
	/// <typeparam name="TExtension">The typed extension object returned by the plugin.</typeparam>
	/// <param name="plugin">The typed plugin callback to project.</param>
	/// <returns>The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions, TCustomProperties, TCustomState, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TCustomState, TExtension> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
		where TCustomState : Pinia.PiniaStateTree
		where TExtension : Vue3.VueProps;
}
