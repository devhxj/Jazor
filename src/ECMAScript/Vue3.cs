namespace ECMAScript;

/// <summary>
/// Handle returned by <c>watch()</c> and <c>watchEffect()</c>. Vue exposes this as a
/// callable stop function with <c>pause()</c>, <c>resume()</c>, and <c>stop()</c> members;
/// C# uses explicit methods so the control surface remains discoverable.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueWatchHandle
{
	protected VueWatchHandle()
	{
	}

	/// <summary>
	/// Stop the watcher and clean up its reactive dependency tracking.
	/// </summary>
	[Description("@#stop")]
	public extern void Stop();

	/// <summary>
	/// Temporarily pause the watcher without disposing it.
	/// </summary>
	[Description("@#pause")]
	public extern void Pause();

	/// <summary>
	/// Resume a watcher that was previously paused.
	/// </summary>
	[Description("@#resume")]
	public extern void Resume();
}

/// <summary>
/// Registers a cleanup callback for the current watcher run.
/// </summary>
/// <param name="cleanup">Cleanup work to execute before the watcher re-runs or stops.</param>
public delegate void VueWatchCleanupRegistration(Action cleanup);

/// <summary>
/// Effect callback that receives Vue's cleanup registration function.
/// </summary>
/// <param name="onCleanup">Function used to register cleanup for the current effect run.</param>
public delegate void VueWatchEffectCallback(VueWatchCleanupRegistration onCleanup);

/// <summary>
/// Watch callback that receives new value, previous value, and Vue's cleanup registration function.
/// </summary>
/// <typeparam name="T">The watched value type.</typeparam>
/// <param name="value">The current value.</param>
/// <param name="oldValue">The previous value.</param>
/// <param name="onCleanup">Function used to register cleanup for the current watcher run.</param>
public delegate void VueWatchCleanupCallback<T>(T value, T oldValue, VueWatchCleanupRegistration onCleanup);

/// <summary>
/// Watch callback for multiple sources of the same value type.
/// Vue supplies parallel arrays containing the new and previous values.
/// </summary>
/// <typeparam name="T">The value type produced by each watched source.</typeparam>
/// <param name="values">Current values from all sources, in source order.</param>
/// <param name="oldValues">Previous values from all sources, in source order.</param>
public delegate void VueWatchSourcesCallback<T>(T[] values, T[] oldValues);

/// <summary>
/// Cleanup-aware watch callback for multiple sources of the same value type.
/// </summary>
/// <typeparam name="T">The value type produced by each watched source.</typeparam>
/// <param name="values">Current values from all sources, in source order.</param>
/// <param name="oldValues">Previous values from all sources, in source order.</param>
/// <param name="onCleanup">Function used to register cleanup for the current watcher run.</param>
public delegate void VueWatchSourcesCleanupCallback<T>(T[] values, T[] oldValues, VueWatchCleanupRegistration onCleanup);

/// <summary>
/// Callback signature for Vue event handlers that receive a typed event payload.
/// </summary>
/// <typeparam name="T">The type of the event payload value.</typeparam>
/// <param name="value">The event payload emitted by the source component.</param>
public delegate void VueEventHandler<T>(T value);

/// <summary>
/// Factory callback for a Vue prop default value.
/// </summary>
/// <typeparam name="TValue">The prop value type.</typeparam>
/// <returns>The default value Vue should use when the prop is absent.</returns>
public delegate TValue VuePropDefaultFactory<TValue>();

/// <summary>
/// Factory callback for a Vue prop default value that needs access to the raw props object.
/// </summary>
/// <typeparam name="TValue">The prop value type.</typeparam>
/// <param name="rawProps">The raw prop values supplied to the component instance.</param>
/// <returns>The default value Vue should use when the prop is absent.</returns>
public delegate TValue VuePropRawPropsDefaultFactory<TValue>(Vue3.VueProps rawProps);

/// <summary>
/// Validator callback for a Vue prop declaration.
/// </summary>
/// <typeparam name="TValue">The prop value type.</typeparam>
/// <param name="value">The prop value being validated.</param>
/// <returns><c>true</c> when the value is accepted.</returns>
public delegate bool VuePropValidator<TValue>(TValue value);

/// <summary>
/// Validator callback for a Vue prop declaration that needs access to all raw props.
/// </summary>
/// <typeparam name="TValue">The prop value type.</typeparam>
/// <param name="value">The prop value being validated.</param>
/// <param name="rawProps">The raw prop values supplied to the component instance.</param>
/// <returns><c>true</c> when the value is accepted.</returns>
public delegate bool VuePropRawPropsValidator<TValue>(TValue value, Vue3.VueProps rawProps);

/// <summary>
/// Validator callback for a no-payload Vue emit declaration.
/// </summary>
/// <returns><c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator();

/// <summary>
/// Validator callback for a Vue emit declaration with one payload value.
/// </summary>
/// <typeparam name="T0">The first emitted payload type.</typeparam>
/// <param name="arg0">The first emitted payload value.</param>
/// <returns><c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0>(T0 arg0);

/// <summary>
/// Validator callback for a Vue emit declaration with two payload values.
/// </summary>
/// <typeparam name="T0">The first emitted payload type.</typeparam>
/// <typeparam name="T1">The second emitted payload type.</typeparam>
/// <param name="arg0">The first emitted payload value.</param>
/// <param name="arg1">The second emitted payload value.</param>
/// <returns><c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0, T1>(T0 arg0, T1 arg1);

/// <summary>
/// Validator callback for a Vue emit declaration with three payload values.
/// </summary>
/// <typeparam name="T0">The first emitted payload type.</typeparam>
/// <typeparam name="T1">The second emitted payload type.</typeparam>
/// <typeparam name="T2">The third emitted payload type.</typeparam>
/// <param name="arg0">The first emitted payload value.</param>
/// <param name="arg1">The second emitted payload value.</param>
/// <param name="arg2">The third emitted payload value.</param>
/// <returns><c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);

/// <summary>
/// Validator callback for a Vue emit declaration with four payload values.
/// </summary>
/// <typeparam name="T0">The first emitted payload type.</typeparam>
/// <typeparam name="T1">The second emitted payload type.</typeparam>
/// <typeparam name="T2">The third emitted payload type.</typeparam>
/// <typeparam name="T3">The fourth emitted payload type.</typeparam>
/// <param name="arg0">The first emitted payload value.</param>
/// <param name="arg1">The second emitted payload value.</param>
/// <param name="arg2">The third emitted payload value.</param>
/// <param name="arg3">The fourth emitted payload value.</param>
/// <returns><c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);

/// <summary>
/// Callback that returns a render tree (VNode). Used as the return type of <c>setup()</c>
/// to provide the component's render function.
/// </summary>
/// <returns>A root VNode representing the rendered component output.</returns>
public delegate Vue3.IVNode VueRenderCallback();

/// <summary>
/// Callback that returns a VNode from a slot with no scoped data.
/// </summary>
/// <returns>A VNode produced by the slot, or <c>null</c> if the slot is empty.</returns>
public delegate Vue3.IVNode VueSlotCallback();

/// <summary>
/// Callback that returns a VNode from a scoped slot that receives slot props.
/// </summary>
/// <typeparam name="TScope">The type of the scoped data passed into the slot.</typeparam>
/// <param name="scope">The scoped data object provided by the parent component to the slot.</param>
/// <returns>A VNode produced by the slot, or <c>null</c> if the slot is empty.</returns>
public delegate Vue3.IVNode VueSlotCallback<TScope>(TScope scope);

/// <summary>
/// Callback signature for a component <c>setup()</c> function with no typed props.
/// The setup function runs before the component is mounted and returns a render callback.
/// </summary>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueSetupCallback();

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives typed props.
/// </summary>
/// <typeparam name="TProps">The props record type, inheriting from <see cref="Vue3.VueProps"/>.</typeparam>
/// <param name="props">The reactive props object passed by the parent component.</param>
/// <param name="context">The setup context providing <c>attrs</c>, <c>slots</c>, <c>emit</c>, and <c>expose</c>.</param>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSetupCallback<TProps>(TProps props, Vue3.VueSetupContext context)
	where TProps : Vue3.VueProps;

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives typed slots but no typed props.
/// </summary>
/// <typeparam name="TSlots">The slots record type, inheriting from <see cref="Vue3.VueSlots"/>.</typeparam>
/// <param name="context">The typed setup context providing typed <c>slots</c> in addition to the standard context members.</param>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSlotSetupCallback<TSlots>(Vue3.VueSetupContext<TSlots> context)
	where TSlots : Vue3.VueSlots;

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives both typed props and typed slots.
/// </summary>
/// <typeparam name="TProps">The props record type, inheriting from <see cref="Vue3.VueProps"/>.</typeparam>
/// <typeparam name="TSlots">The slots record type, inheriting from <see cref="Vue3.VueSlots"/>.</typeparam>
/// <param name="props">The reactive props object passed by the parent component.</param>
/// <param name="context">The typed setup context providing typed <c>slots</c> in addition to the standard context members.</param>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSetupCallback<TProps, TSlots>(TProps props, Vue3.VueSetupContext<TSlots> context)
	where TProps : Vue3.VueProps
	where TSlots : Vue3.VueSlots;

/// <summary>
/// Callback signature for Options API <c>data()</c>. The returned record is lowered to
/// the plain object that Vue makes reactive for each component instance.
/// </summary>
/// <returns>A fresh state object for one component instance.</returns>
public delegate Vue3.VueProps VueDataCallback();

/// <summary>
/// Callback signature for a function-form Vue plugin installation entrypoint.
/// </summary>
/// <param name="app">The Vue application instance currently being configured.</param>
public delegate void VuePluginInstallCallback(Vue3.VueApp app);

/// <summary>
/// Callback signature for a function-form or object-form Vue plugin installation entrypoint
/// that receives strongly typed install options.
/// </summary>
/// <typeparam name="TOptions">The typed plugin options contract.</typeparam>
/// <param name="app">The Vue application instance currently being configured.</param>
/// <param name="options">The strongly typed options passed to <c>app.use(plugin, options)</c>.</param>
public delegate void VuePluginInstallCallback<TOptions>(Vue3.VueApp app, TOptions options)
	where TOptions : Vue3.VuePluginOptions;

/// <summary>
/// Callback used by <c>defineCustomElement()</c> to configure the app instance
/// created for a Vue custom element.
/// </summary>
/// <param name="app">The custom element's internally created Vue application instance.</param>
public delegate void VueCustomElementConfigureAppCallback(Vue3.VueApp app);

/// <summary>
/// Callback signature for a Vue directive lifecycle hook that does not need a previous value.
/// </summary>
/// <param name="element">The target DOM element currently controlled by the directive.</param>
/// <param name="binding">The current directive binding payload.</param>
/// <param name="vnode">The current VNode associated with the element.</param>
public delegate void VueDirectiveHook(Element element, Vue3.VueDirectiveBinding binding, Vue3.IVNode vnode);

/// <summary>
/// Callback signature for a typed Vue directive lifecycle hook that does not need a previous value.
/// </summary>
/// <typeparam name="TValue">The typed contract of the directive's current binding value.</typeparam>
/// <param name="element">The target DOM element currently controlled by the directive.</param>
/// <param name="binding">The current typed directive binding payload.</param>
/// <param name="vnode">The current VNode associated with the element.</param>
public delegate void VueDirectiveHook<TValue>(Element element, Vue3.VueDirectiveBinding<TValue> binding, Vue3.IVNode vnode);

/// <summary>
/// Callback signature for a Vue directive function shorthand. Vue treats this as the
/// same callback for both <c>mounted</c> and <c>updated</c>.
/// </summary>
/// <param name="element">The target DOM element currently controlled by the directive.</param>
/// <param name="binding">The current directive binding payload.</param>
public delegate void VueDirectiveFunction(Element element, Vue3.VueDirectiveBinding binding);

/// <summary>
/// Callback signature for a typed Vue directive function shorthand. Vue treats this as the
/// same callback for both <c>mounted</c> and <c>updated</c>.
/// </summary>
/// <typeparam name="TValue">The typed contract of the directive's current binding value.</typeparam>
/// <param name="element">The target DOM element currently controlled by the directive.</param>
/// <param name="binding">The current typed directive binding payload.</param>
public delegate void VueDirectiveFunction<TValue>(Element element, Vue3.VueDirectiveBinding<TValue> binding);

/// <summary>
/// Callback signature for a Vue directive update hook that also needs the previous binding value.
/// </summary>
/// <param name="element">The target DOM element currently controlled by the directive.</param>
/// <param name="binding">The current directive update binding payload.</param>
/// <param name="vnode">The current VNode associated with the element.</param>
/// <param name="previousVNode">The previous VNode associated with the same element.</param>
public delegate void VueDirectiveUpdateHook(Element element, Vue3.VueDirectiveUpdateBinding binding, Vue3.IVNode vnode, Vue3.IVNode previousVNode);

/// <summary>
/// Callback signature for a typed Vue directive update hook that also needs the previous binding value.
/// </summary>
/// <typeparam name="TValue">The typed contract of the directive's current and previous binding values.</typeparam>
/// <param name="element">The target DOM element currently controlled by the directive.</param>
/// <param name="binding">The current typed directive update binding payload.</param>
/// <param name="vnode">The current VNode associated with the element.</param>
/// <param name="previousVNode">The previous VNode associated with the same element.</param>
public delegate void VueDirectiveUpdateHook<TValue>(Element element, Vue3.VueDirectiveUpdateBinding<TValue> binding, Vue3.IVNode vnode, Vue3.IVNode previousVNode);

/// <summary>
/// Callback signature for a Vue directive SSR hook that returns props to merge into the rendered element.
/// </summary>
/// <param name="binding">The current directive binding payload.</param>
/// <param name="vnode">The current VNode associated with the element.</param>
/// <returns>Additional props that should be merged into the SSR-rendered element.</returns>
public delegate Vue3.VueProps? VueDirectiveSsrPropsCallback(Vue3.VueDirectiveBinding binding, Vue3.IVNode vnode);

/// <summary>
/// Callback signature for a typed Vue directive SSR hook that returns props to merge into the rendered element.
/// </summary>
/// <typeparam name="TValue">The typed contract of the directive's current binding value.</typeparam>
/// <param name="binding">The current typed directive binding payload.</param>
/// <param name="vnode">The current VNode associated with the element.</param>
/// <returns>Additional props that should be merged into the SSR-rendered element.</returns>
public delegate Vue3.VueProps? VueDirectiveSsrPropsCallback<TValue>(Vue3.VueDirectiveBinding<TValue> binding, Vue3.IVNode vnode);

/// <summary>
/// Loader callback for a Vue async component. It returns a JavaScript promise that
/// resolves to the component definition.
/// </summary>
/// <returns>A promise resolving to the async component definition.</returns>
public delegate IPromise<Vue3.IVueComponent> VueAsyncComponentLoader();

/// <summary>
/// Loader callback for a strongly typed Vue async component.
/// </summary>
/// <typeparam name="TComponent">The component contract produced by the loader.</typeparam>
/// <returns>A promise resolving to the typed async component definition.</returns>
public delegate IPromise<TComponent> VueAsyncComponentLoader<TComponent>()
	where TComponent : Vue3.IVueComponent;

/// <summary>
/// Callback used by Vue async component error handling to retry or fail the load.
/// </summary>
public delegate void VueAsyncComponentRetryCallback();

/// <summary>
/// Error callback for async component loading. Vue supplies the thrown error, retry
/// callback, fail callback, and current attempt count.
/// </summary>
/// <param name="error">The JavaScript error raised while loading the component.</param>
/// <param name="retry">Retry the async component loader.</param>
/// <param name="fail">Fail the async component load.</param>
/// <param name="attempts">The number of load attempts so far.</param>
public delegate void VueAsyncComponentErrorCallback(Error error, VueAsyncComponentRetryCallback retry, VueAsyncComponentRetryCallback fail, Number attempts);

/// <summary>
/// Factory callback for <c>customRef()</c>. Vue supplies <c>track</c> and
/// <c>trigger</c> callbacks, and the factory returns the custom ref get/set handlers.
/// </summary>
/// <typeparam name="T">The custom ref value type.</typeparam>
/// <param name="track">Call when the custom getter should track a dependency.</param>
/// <param name="trigger">Call when the custom setter should trigger dependents.</param>
/// <returns>The get/set handlers used by the custom ref.</returns>
public delegate Vue3.VueCustomRefHandlers<T> VueCustomRefFactory<T>(Action track, Action trigger);

/// <summary>
/// Callback used by Vue watcher debug hooks such as <c>onTrack</c> and
/// <c>onTrigger</c>.
/// </summary>
/// <param name="event">The debugger event emitted by Vue's reactivity system.</param>
public delegate void VueDebuggerCallback(Vue3.VueDebuggerEvent @event);

/// <summary>
/// Error-captured callback that can stop Vue error propagation by returning
/// <c>false</c>.
/// </summary>
/// <param name="error">The unknown-like error value captured by Vue.</param>
/// <param name="instance">The component public instance where the error originated, when available.</param>
/// <param name="info">Vue's error context string.</param>
/// <returns><c>false</c> to stop propagation; <c>true</c> to continue.</returns>
public delegate bool VueErrorCapturedCallback(Vue3.VueValue? error, Vue3.VueComponentPublicInstance? instance, string info);

/// <summary>
/// Error-captured handler for cases that only observe captured errors and always let
/// Vue continue propagation.
/// </summary>
/// <param name="error">The unknown-like error value captured by Vue.</param>
/// <param name="instance">The component public instance where the error originated, when available.</param>
/// <param name="info">Vue's error context string.</param>
public delegate void VueErrorCapturedHandler(Vue3.VueValue? error, Vue3.VueComponentPublicInstance? instance, string info);

/// <summary>
/// Server-prefetch callback that returns a JavaScript promise.
/// </summary>
/// <returns>The promise Vue should await during server rendering.</returns>
public delegate IPromise VueServerPrefetchPromiseCallback();

/// <summary>
/// Server-prefetch callback shape used by compiler-lowered async callbacks.
/// </summary>
/// <returns>The bridge promise result Vue should await during server rendering.</returns>
public delegate PromiseResult VueServerPrefetchCallback();

/// <summary>
/// this-bound data callback for Options API authoring. The first parameter receives
/// the component public instance (<c>this</c>) at runtime.
/// </summary>
/// <typeparam name="TThis">Typed view of the component public instance.</typeparam>
/// <param name="self">The runtime component public instance.</param>
/// <returns>The data object for the current component instance.</returns>
public delegate Vue3.VueProps VueThisDataCallback<TThis>(TThis self)
	where TThis : class;

/// <summary>
/// this-bound action callback with no explicit runtime arguments.
/// </summary>
/// <typeparam name="TThis">Typed view of the component public instance.</typeparam>
/// <param name="self">The runtime component public instance.</param>
public delegate void VueThisAction<TThis>(TThis self)
	where TThis : class;

/// <summary>
/// this-bound action callback with one runtime argument.
/// </summary>
public delegate void VueThisAction<TThis, T1>(TThis self, T1 arg1)
	where TThis : class;

/// <summary>
/// this-bound action callback with two runtime arguments.
/// </summary>
public delegate void VueThisAction<TThis, T1, T2>(TThis self, T1 arg1, T2 arg2)
	where TThis : class;

/// <summary>
/// this-bound action callback with three runtime arguments.
/// </summary>
public delegate void VueThisAction<TThis, T1, T2, T3>(TThis self, T1 arg1, T2 arg2, T3 arg3)
	where TThis : class;

/// <summary>
/// this-bound function callback with no explicit runtime arguments.
/// </summary>
public delegate TResult VueThisFunc<TThis, TResult>(TThis self)
	where TThis : class;

/// <summary>
/// this-bound function callback with one runtime argument.
/// </summary>
public delegate TResult VueThisFunc<TThis, T1, TResult>(TThis self, T1 arg1)
	where TThis : class;

/// <summary>
/// this-bound function callback with two runtime arguments.
/// </summary>
public delegate TResult VueThisFunc<TThis, T1, T2, TResult>(TThis self, T1 arg1, T2 arg2)
	where TThis : class;

/// <summary>
/// this-bound function callback with three runtime arguments.
/// </summary>
public delegate TResult VueThisFunc<TThis, T1, T2, T3, TResult>(TThis self, T1 arg1, T2 arg2, T3 arg3)
	where TThis : class;

/// <summary>
/// this-bound watch callback that includes Vue's cleanup registration argument.
/// </summary>
public delegate void VueThisWatchCleanupCallback<TThis, TValue>(TThis self, TValue value, TValue oldValue, VueWatchCleanupRegistration onCleanup)
	where TThis : class;

[ECMAScript("npm:vue@3")]
[Description("@#")]
public static class Vue3
{
	/// <summary>
	/// Marker interface for a Vue component reference. Implemented by all component types
	/// produced by <c>defineComponent()</c> and consumed by <c>h()</c>.
	/// </summary>
	public interface IVueComponent : IUIComponent { }

	/// <summary>
	/// A Vue component that declares typed props. The compiler uses this interface
	/// to select the correct <c>h()</c> overload for props-only components.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	public interface IVueComponent<TProps> : IVueComponent
		where TProps : VueProps
	{
	}

	/// <summary>
	/// A Vue component that declares typed slots but no typed props. The compiler uses
	/// this interface to select the correct <c>h()</c> overload for slots-only components.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public interface IVueSlotComponent<TSlots> : IVueComponent
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// A Vue component that declares both typed props and typed slots. The compiler uses
	/// this interface to select the correct <c>h()</c> overload for components with both.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public interface IVueComponent<TProps, TSlots> : IVueComponent<TProps>, IVueSlotComponent<TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// Represents a Vue virtual DOM node (VNode) returned by <c>h()</c>. VNodes are the
	/// building blocks of Vue's render tree and are diffed/patched by the runtime.
	/// </summary>
	public interface IVNode { }

	/// <summary>
	/// A reactive reference wrapper. Reading <c>Value</c> tracks the ref as a reactive
	/// dependency; writing <c>Value</c> triggers any watchers depending on this ref.
	/// </summary>
	/// <typeparam name="T">The type of the wrapped value.</typeparam>
	public interface IVueRef<T>
	{
		/// <summary>
		/// Gets or sets the underlying reactive value. Reads are tracked; writes notify watchers.
		/// </summary>
		[Description("@#value")]
		public T Value { get; set; }
	}

	/// <summary>
	/// Marker interface for option bags that map to plain JavaScript objects in Vue component
	/// options, plugin configuration, and registries.
	/// </summary>
	public interface IVueOptionsBag { }

	/// <summary>
	/// Strongly typed Vue dependency-injection key. At runtime this is still the
	/// JavaScript <see cref="Symbol"/> value supplied by the user; the generic argument
	/// only constrains matching <c>Provide</c> / <c>Inject</c> calls in C#.
	/// </summary>
	/// <typeparam name="TValue">The value contract associated with this injection key.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueInjectionKey<TValue>
	{
		private VueInjectionKey()
		{
		}

		/// <summary>
		/// Treat a JavaScript symbol as a typed Vue injection key. This erases to the
		/// original symbol value at emission time.
		/// </summary>
		/// <param name="key">The JavaScript symbol used as the injection key.</param>
		public extern static implicit operator VueInjectionKey<TValue>(Symbol key);

		/// <summary>
		/// Exposes the underlying JavaScript symbol when an API needs a raw symbol key.
		/// </summary>
		/// <param name="key">The typed Vue injection key.</param>
		public extern static implicit operator Symbol(VueInjectionKey<TValue> key);
	}

	/// <summary>
	/// Base record for component prop declarations. Inherit from this record and declare
	/// properties to define the props a component accepts. Maps to a plain JS object in
	/// Vue's <c>props</c> option.
	/// </summary>
	public abstract record VueProps : IVueOptionsBag;

	/// <summary>
	/// Generic dictionary-style Vue object authoring surface for arbitrary string keys.
	/// This remains a record so it participates in structural object lowering and emits
	/// a plain JavaScript object rather than a runtime <c>Map</c>.
	/// </summary>
	/// <typeparam name="TValue">The value contract for each arbitrary key.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueDictionary<TValue> : VueProps
	{
		/// <summary>
		/// Gets or sets an arbitrary Vue/object property by its final emitted key.
		/// </summary>
		/// <param name="key">The final JavaScript object key to emit.</param>
		/// <returns>The value mapped to the given key.</returns>
		public extern TValue? this[string key] { get; set; }
	}

	/// <summary>
	/// Generic Vue value contract for dictionary/indexer authoring surfaces.
	/// This is a compile-time wrapper only; implicit conversions erase to the
	/// underlying JavaScript value at emission time.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueValue
	{
		private VueValue()
		{
		}

		public extern static implicit operator VueValue(string value);

		public extern static implicit operator VueValue(bool value);

		public extern static implicit operator VueValue(Number value);

		public extern static implicit operator VueValue(BigInt value);

		public extern static implicit operator VueValue(char value);

		public extern static implicit operator VueValue(double value);

		public extern static implicit operator VueValue(float value);

		public extern static implicit operator VueValue(int value);

		public extern static implicit operator VueValue(long value);

		public extern static implicit operator VueValue(short value);

		public extern static implicit operator VueValue(ushort value);

		public extern static implicit operator VueValue(byte value);

		public extern static implicit operator VueValue(sbyte value);

		public extern static implicit operator VueValue(uint value);

		public extern static implicit operator VueValue(ulong value);

		public extern static implicit operator VueValue(decimal value);

		public extern static implicit operator VueValue(Action value);

		public extern static implicit operator VueValue(VueProps value);

		public extern static implicit operator VueValue(VueValue[] value);
	}

	/// <summary>
	/// Canonical child value contract for <c>h(...)</c> overloads.
	/// This preserves JS-facing flexibility (VNode / text / number / boolean / VNode array)
	/// while keeping the C# public surface compact and stable.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueChild
	{
		private VueChild()
		{
		}

		public extern static implicit operator VueChild(string value);

		public extern static implicit operator VueChild(Number value);

		public extern static implicit operator VueChild(byte value);

		public extern static implicit operator VueChild(sbyte value);

		public extern static implicit operator VueChild(short value);

		public extern static implicit operator VueChild(ushort value);

		public extern static implicit operator VueChild(int value);

		public extern static implicit operator VueChild(uint value);

		public extern static implicit operator VueChild(long value);

		public extern static implicit operator VueChild(ulong value);

		public extern static implicit operator VueChild(float value);

		public extern static implicit operator VueChild(double value);

		public extern static implicit operator VueChild(decimal value);

		public extern static implicit operator VueChild(bool value);

		public extern static implicit operator VueChild(IVNode[] value);
	}

	/// <summary>
	/// Vue VNode key contract. Vue accepts string, number, and symbol keys; this wrapper
	/// keeps that union strongly typed while allowing natural C# assignments without
	/// relying on chained implicit conversions through <see cref="Number"/>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueKey
	{
		private VueKey()
		{
		}

		public extern static implicit operator VueKey(string value);

		public extern static implicit operator VueKey(Symbol value);

		public extern static implicit operator VueKey(Number value);

		public extern static implicit operator VueKey(byte value);

		public extern static implicit operator VueKey(sbyte value);

		public extern static implicit operator VueKey(short value);

		public extern static implicit operator VueKey(ushort value);

		public extern static implicit operator VueKey(int value);

		public extern static implicit operator VueKey(uint value);

		public extern static implicit operator VueKey(long value);

		public extern static implicit operator VueKey(ulong value);

		public extern static implicit operator VueKey(float value);

		public extern static implicit operator VueKey(double value);

		public extern static implicit operator VueKey(decimal value);
	}

	/// <summary>
	/// JavaScript constructor values accepted by Vue prop declarations.
	/// These properties emit the raw constructor identifiers such as <c>String</c>,
	/// <c>Number</c>, and <c>Boolean</c>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VuePropType
	{
		private VuePropType()
		{
		}

		[Description("@#String")]
		public extern static VuePropType String { get; }

		[Description("@#Number")]
		public extern static VuePropType Number { get; }

		[Description("@#Boolean")]
		public extern static VuePropType Boolean { get; }

		[Description("@#Array")]
		public extern static VuePropType Array { get; }

		[Description("@#Object")]
		public extern static VuePropType Object { get; }

		[Description("@#Date")]
		public extern static VuePropType Date { get; }

		[Description("@#Function")]
		public extern static VuePropType Function { get; }

		[Description("@#Symbol")]
		public extern static VuePropType Symbol { get; }

		[Description("@#Error")]
		public extern static VuePropType Error { get; }
	}

	/// <summary>
	/// Convenience non-generic dictionary surface for common Vue object authoring.
	/// This is the direct default when the value contract is the general <see cref="VueValue"/>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDictionary : VueDictionary<VueValue>
	{
	}

	/// <summary>
	/// String-keyed event listener bag for render-function props. Keys are final Vue
	/// listener prop names such as <c>onClick</c>; values are no-payload handlers.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueEventHandlers : VueProps
	{
		/// <summary>
		/// Gets or sets an event listener by its final Vue listener prop key.
		/// </summary>
		/// <param name="key">The final emitted listener key, for example <c>onClick</c>.</param>
		/// <returns>The registered event listener.</returns>
		public extern Action? this[string key] { get; set; }
	}

	/// <summary>
	/// String-keyed event listener bag for render-function props with a typed event
	/// payload contract.
	/// </summary>
	/// <typeparam name="TEvent">The event payload supplied by Vue when the listener runs.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEventHandlers<TEvent> : VueEventHandlers
	{
		/// <summary>
		/// Gets or sets a typed event listener by its final Vue listener prop key.
		/// </summary>
		/// <param name="key">The final emitted listener key, for example <c>onMousemove</c>.</param>
		/// <returns>The registered typed event listener.</returns>
		public new extern VueEventHandler<TEvent>? this[string key] { get; set; }
	}

	/// <summary>
	/// Read-side fallthrough listener projection for <c>useAttrs()</c> / <c>context.attrs</c>.
	/// Use this when arbitrary <c>on*</c> keys should remain callable without defining
	/// one property per listener.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueAttributeListeners : VueProps
	{
		/// <summary>
		/// Reads a no-payload listener by its final Vue listener key, for example
		/// <c>onClick</c>.
		/// </summary>
		/// <param name="key">The final listener key to read.</param>
		/// <returns>The listener callback when present.</returns>
		public extern Action? this[string key] { get; set; }
	}

	/// <summary>
	/// Typed read-side fallthrough listener projection for <c>useAttrs()</c> /
	/// <c>context.attrs</c>.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type expected by each listener.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueAttributeListeners<TEvent> : VueAttributeListeners
	{
		/// <summary>
		/// Reads a typed listener by its final Vue listener key.
		/// </summary>
		/// <param name="key">The final listener key to read.</param>
		/// <returns>The typed listener callback when present.</returns>
		public new extern VueEventHandler<TEvent>? this[string key] { get; set; }
	}

	/// <summary>
	/// Object-form Vue prop declaration for a strongly typed prop value.
	/// Use <see cref="Type"/> for a single constructor, <see cref="Types"/> for a
	/// constructor array, and one of the default / validator members as needed.
	/// Members that map to the same Vue key are mutually exclusive by convention.
	/// </summary>
	/// <typeparam name="TValue">The prop value type accepted by setup/render code.</typeparam>
	public record VuePropOptions<TValue> : VueProps
	{
		/// <summary>
		/// Single JavaScript constructor used by Vue's runtime prop type check.
		/// </summary>
		[Description("@#type")]
		public VuePropType? Type { get; init; }

		/// <summary>
		/// Constructor array used by Vue's runtime prop type check. Elements may be
		/// <c>null</c> to express Vue's nullable type form.
		/// </summary>
		[Description("@#type")]
		public VuePropType?[]? Types { get; init; }

		/// <summary>
		/// Whether the prop must be supplied by the parent.
		/// </summary>
		[Description("@#required")]
		public bool? Required { get; init; }

		/// <summary>
		/// Literal default value used when the prop is absent.
		/// </summary>
		[Description("@#default")]
		public TValue? Default { get; init; }

		/// <summary>
		/// Factory default used when the prop is absent. Prefer this for object and
		/// array defaults so each component instance receives a fresh value.
		/// </summary>
		[Description("@#default")]
		public VuePropDefaultFactory<TValue>? DefaultFactory { get; init; }

		/// <summary>
		/// Factory default that receives the raw props object supplied to the component.
		/// </summary>
		[Description("@#default")]
		public VuePropRawPropsDefaultFactory<TValue>? DefaultFactoryWithProps { get; init; }

		/// <summary>
		/// Prop validator that observes only the current prop value.
		/// </summary>
		[Description("@#validator")]
		public VuePropValidator<TValue>? Validator { get; init; }

		/// <summary>
		/// Prop validator that also observes the raw props object supplied to the component.
		/// </summary>
		[Description("@#validator")]
		public VuePropRawPropsValidator<TValue>? ValidatorWithProps { get; init; }
	}

	/// <summary>
	/// Non-generic prop declaration for cases where the value contract is intentionally
	/// unknown-like but still typed as <see cref="VueValue"/> instead of <c>object</c>.
	/// </summary>
	public record VuePropOptions : VuePropOptions<VueValue>;

	/// <summary>
	/// String-keyed object-form props registry for declarations that share one value type.
	/// For heterogeneous prop values, declare a custom <see cref="VueProps"/> record with
	/// <see cref="VuePropOptions{TValue}"/> properties.
	/// </summary>
	/// <typeparam name="TValue">The prop value type used by all registry entries.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VuePropRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets one object-form prop declaration by final prop key.
		/// </summary>
		/// <param name="key">The final Vue prop key.</param>
		/// <returns>The declaration for the given prop key.</returns>
		public extern Either<VuePropType, VuePropType?[], VuePropOptions<TValue>>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropType type);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropType?[] types);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropOptions<TValue> options);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Non-generic object-form props registry using <see cref="VueValue"/> for each
	/// declaration's value contract.
	/// </summary>
	public record VuePropRegistry : VuePropRegistry<VueValue>;

	/// <summary>
	/// String-keyed object-form emits registry for no-payload validators.
	/// For plain event declarations without validators, prefer the existing
	/// array-form <c>EmitNames</c> surface.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for one-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for two-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	/// <typeparam name="T1">The second emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0, T1> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0, T1>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0, T1> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for three-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	/// <typeparam name="T1">The second emitted payload type.</typeparam>
	/// <typeparam name="T2">The third emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0, T1, T2> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0, T1, T2>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0, T1, T2> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for four-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	/// <typeparam name="T1">The second emitted payload type.</typeparam>
	/// <typeparam name="T2">The third emitted payload type.</typeparam>
	/// <typeparam name="T3">The fourth emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0, T1, T2, T3> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0, T1, T2, T3>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0, T1, T2, T3> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// General-purpose Vue object authoring surface for <c>h()</c> props and root props.
	/// This remains a record so it participates in the compiler's structural object lowering.
	/// In addition to the common convenience members, it also exposes a string-keyed
	/// dictionary surface for direct object-literal authoring.
	/// </summary>
	public record VueObject : VueDictionary
	{
		/// <summary>
		/// Vue special <c>is</c> attribute for customized built-in elements.
		/// Dynamic components should use the component-valued <c>H(...)</c> overloads directly.
		/// </summary>
		[Description("@#is")]
		public string? Is { get; init; }

		/// <summary>
		/// Vue VNode <c>key</c>. Accepts string, number, or symbol values through
		/// <see cref="VueKey"/>.
		/// </summary>
		[Description("@#key")]
		public VueKey? Key { get; init; }

		/// <summary>
		/// Standard Vue <c>class</c> binding. Accepts string, string array, object forms, or
		/// mixed class arrays via <see cref="VueValue"/>.
		/// </summary>
		[Description("@#class")]
		public Either<string, string[], VueProps, VueValue[]>? Class { get; init; }

		/// <summary>
		/// Standard Vue <c>style</c> binding. Use a typed record or the convenience
		/// <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Description("@#style")]
		public VueProps? Style { get; init; }

		/// <summary>
		/// Named template ref key, intended to pair with <see cref="UseTemplateRef{TElement}(string)"/>.
		/// Callback and ref-object forms remain a separate typed authoring design surface.
		/// </summary>
		[Description("@#ref")]
		public string? Ref { get; init; }

		/// <summary>
		/// Event listeners flattened into the current Vue props object. Listener keys must
		/// be final Vue render-function prop names, such as <c>onClick</c>.
		/// </summary>
		[Spread]
		public VueEventHandlers? Events { get; init; }

		/// <summary>
		/// Standard <c>id</c> attribute.
		/// </summary>
		[Description("@#id")]
		public string? Id { get; init; }

		/// <summary>
		/// Standard <c>title</c> attribute.
		/// </summary>
		[Description("@#title")]
		public string? Title { get; init; }

		/// <summary>
		/// Additional properties to flatten directly into the current Vue object.
		/// Supports both typed records and <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Attrs { get; init; }

		/// <summary>
		/// Dataset attributes flattened into the current Vue object.
		/// Expected property names should already map to their final <c>data-*</c> keys.
		/// Supports both typed records and <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Dataset { get; init; }

		/// <summary>
		/// Raw attributes flattened into the current Vue object without additional Vue-specific
		/// interpretation. Supports both typed records and <see cref="VueDictionary"/>
		/// for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Raw { get; init; }
	}

	/// <summary>
	/// Typed Vue object authoring surface that can both flatten a typed props bag and carry
	/// the common convenience members declared on <see cref="VueObject"/>.
	/// </summary>
	/// <typeparam name="TProps">The typed props record that should be flattened into the output object.</typeparam>
	public record VueObject<TProps> : VueObject
		where TProps : VueProps
	{
		/// <summary>
		/// Typed props bag flattened into the current Vue object.
		/// </summary>
		[Spread]
		public TProps? Props { get; init; }
	}

	/// <summary>
	/// Base record for component slot declarations. This can be used directly as a
	/// string-keyed bag for parameterless slot callbacks, or inherited when a component
	/// wants a stronger typed slot contract. Maps to a plain JS object in Vue's
	/// <c>slots</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueSlots : IVueOptionsBag
	{
		/// <summary>
		/// Gets or sets a parameterless slot callback by its final emitted slot name.
		/// Scoped slots still require an explicit typed slot record with
		/// <see cref="VueSlotCallback{TScope}"/> properties.
		/// </summary>
		/// <param name="key">The final Vue slot name.</param>
		/// <returns>The parameterless slot callback registered for that name.</returns>
		public extern VueSlotCallback? this[string key] { get; set; }
	}

	/// <summary>
	/// Generic read/write slot projection for scoped slots that share one scope type.
	/// This can be used with <c>UseSlots&lt;TSlots&gt;()</c> to read runtime scoped slot
	/// callbacks without defining an explicit slot record for each key.
	/// </summary>
	/// <typeparam name="TScope">The scope payload type passed to each slot callback.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueScopedSlots<TScope> : VueSlots
	{
		/// <summary>
		/// Reads or writes a scoped slot callback by its final emitted slot name.
		/// </summary>
		/// <param name="key">The final Vue slot key.</param>
		/// <returns>The scoped slot callback registered for that name.</returns>
		public new extern VueSlotCallback<TScope>? this[string key] { get; set; }

		/// <summary>
		/// Reads or writes the default scoped slot callback.
		/// </summary>
		[Description("@#default")]
		public extern VueSlotCallback<TScope>? Default { get; set; }
	}

	/// <summary>
	/// Base record for component definition objects passed to <c>defineComponent()</c>.
	/// Holds options shared by all component option shapes.
	/// </summary>
	public abstract record VueComponentDefinition : IVueOptionsBag
	{
		/// <summary>
		/// Controls whether fallthrough attributes are automatically applied to the
		/// component's root element.
		/// </summary>
		[Description("@#inheritAttrs")]
		public bool? InheritAttrs { get; init; }

		/// <summary>
		/// Option-form public instance expose declaration. Only listed member names are
		/// available through template refs on the component public instance.
		/// </summary>
		[Description("@#expose")]
		public string[]? Expose { get; init; }

		/// <summary>
		/// Options API provide object. Use a typed <see cref="VueProps"/> record or
		/// <see cref="VueDictionary"/> when the provide keys are dynamic or library-defined.
		/// Function-form provide that depends on Vue instance <c>this</c> is intentionally
		/// left to the broader this-bound Options API design.
		/// </summary>
		[Description("@#provide")]
		public VueProps? Provide { get; init; }

		/// <summary>
		/// Options API inject declaration. Array-form injection uses <c>string[]</c>;
		/// object-form injection can be expressed with a typed <see cref="VueProps"/>
		/// record or <see cref="VueDictionary"/>.
		/// </summary>
		[Description("@#inject")]
		public Either<string[], VueProps>? Inject { get; init; }

		/// <summary>
		/// Local mixins merged into this component by Vue's Options API merge strategy.
		/// Prefer Composition API for new reusable logic; this property exists as a
		/// low-level compatibility binding for Vue options objects.
		/// </summary>
		[Description("@#mixins")]
		public VueComponentDefinition[]? Mixins { get; init; }

		/// <summary>
		/// Base component options object merged into this component by Vue's Options API
		/// <c>extends</c> strategy. This is a low-level compatibility binding rather than
		/// a C# inheritance model.
		/// </summary>
		[Description("@#extends")]
		public VueComponentDefinition? Extends { get; init; }

		/// <summary>
		/// Options API <c>data()</c> factory. Return a <see cref="VueProps"/> record so Vue
		/// receives a fresh plain object for each component instance. Instance-bound
		/// <c>data(vm)</c> / <c>this</c> authoring is intentionally left to the broader
		/// this-bound Options API design.
		/// </summary>
		[Description("@#data")]
		public VueDataCallback? Data { get; init; }

		/// <summary>
		/// Options API computed object. Use <see cref="VueComputedRegistry{TValue}"/> for
		/// dynamic keys with one value type, or a custom <see cref="VueProps"/> record for
		/// heterogeneous strongly typed computed declarations.
		/// </summary>
		[Description("@#computed")]
		public VueProps? Computed { get; init; }

		/// <summary>
		/// Options API methods object. Use <see cref="VueMethodRegistry{TDelegate}"/> for
		/// dynamic keys with one delegate signature, or a custom <see cref="VueProps"/>
		/// record for heterogeneous strongly typed method declarations.
		/// </summary>
		[Description("@#methods")]
		public VueProps? Methods { get; init; }

		/// <summary>
		/// Options API watch object. Use <see cref="VueWatchRegistry{TValue}"/> for dynamic
		/// keys that observe one value type, or a custom <see cref="VueProps"/> record for
		/// heterogeneous strongly typed watch declarations.
		/// </summary>
		[Description("@#watch")]
		public VueProps? Watch { get; init; }

		/// <summary>
		/// Options API hook invoked immediately after the component instance is initialized.
		/// This C# surface models the no-<c>this</c> callback form; this-bound Options API
		/// authoring remains a separate design problem.
		/// </summary>
		[Description("@#beforeCreate")]
		public Action? BeforeCreate { get; init; }

		/// <summary>
		/// Options API hook invoked after reactive state has been initialized.
		/// </summary>
		[Description("@#created")]
		public Action? Created { get; init; }

		/// <summary>
		/// Options API hook invoked right before the component is mounted.
		/// </summary>
		[Description("@#beforeMount")]
		public Action? BeforeMount { get; init; }

		/// <summary>
		/// Options API hook invoked after the component has been mounted.
		/// </summary>
		[Description("@#mounted")]
		public Action? Mounted { get; init; }

		/// <summary>
		/// Options API hook invoked right before a reactive update patches the DOM.
		/// </summary>
		[Description("@#beforeUpdate")]
		public Action? BeforeUpdate { get; init; }

		/// <summary>
		/// Options API hook invoked after a reactive update has patched the DOM.
		/// </summary>
		[Description("@#updated")]
		public Action? Updated { get; init; }

		/// <summary>
		/// Options API hook invoked right before the component is unmounted.
		/// </summary>
		[Description("@#beforeUnmount")]
		public Action? BeforeUnmount { get; init; }

		/// <summary>
		/// Options API hook invoked after the component has been unmounted.
		/// </summary>
		[Description("@#unmounted")]
		public Action? Unmounted { get; init; }

		/// <summary>
		/// Options API hook invoked when a kept-alive component is inserted back into the DOM.
		/// </summary>
		[Description("@#activated")]
		public Action? Activated { get; init; }

		/// <summary>
		/// Options API hook invoked when a kept-alive component is removed from the DOM cache outlet.
		/// </summary>
		[Description("@#deactivated")]
		public Action? Deactivated { get; init; }

		/// <summary>
		/// Options API hook invoked when an error from a descendant component is captured.
		/// Return <c>false</c> to stop propagation according to Vue runtime semantics.
		/// </summary>
		[Description("@#errorCaptured")]
		public VueErrorCapturedCallback? ErrorCaptured { get; init; }

		/// <summary>
		/// Development-only Options API hook invoked when a reactive dependency is tracked during render.
		/// </summary>
		[Description("@#renderTracked")]
		public VueDebuggerCallback? RenderTracked { get; init; }

		/// <summary>
		/// Development-only Options API hook invoked when a reactive dependency triggers a render update.
		/// </summary>
		[Description("@#renderTriggered")]
		public VueDebuggerCallback? RenderTriggered { get; init; }

		/// <summary>
		/// Server-rendering hook invoked before the component is rendered on the server.
		/// </summary>
		[Description("@#serverPrefetch")]
		public VueServerPrefetchPromiseCallback? ServerPrefetch { get; init; }
	}

	/// <summary>
	/// Registry of child components that the current component can use in its template.
	/// This can be used directly as a string-keyed bag, or inherited when a library wants
	/// a more strongly-typed registry surface. Maps to Vue's <c>components</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueComponentRegistry : IVueOptionsBag
	{
		/// <summary>
		/// Gets or sets a component registration by its final emitted name.
		/// </summary>
		/// <param name="key">The final Vue component registration name.</param>
		/// <returns>The component registered for that name.</returns>
		public extern IVueComponent? this[string key] { get; set; }
	}

	/// <summary>
	/// Registry of custom directives that the current component can use in its template.
	/// This can be used directly as a string-keyed bag, or inherited when a library wants
	/// a more strongly-typed registry surface. Maps to Vue's <c>directives</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDirectiveRegistry : IVueOptionsBag, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets a directive registration by its final emitted name.
		/// </summary>
		/// <param name="key">The final Vue directive registration name.</param>
		/// <returns>The directive registered for that name.</returns>
		public extern VueDirective? this[string key] { get; set; }

		/// <summary>
		/// CLR bridge members kept only for collection-initializer authoring. The compiler
		/// lowers these into plain object literal properties instead of emitting runtime
		/// <c>Add(...)</c> calls.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueDirective directive);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add<TValue>(string key, VueDirective<TValue> directive);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueDirectiveFunction directive);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add<TValue>(string key, VueDirectiveFunction<TValue> directive);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Options bag for plugin configuration passed as the second argument to
	/// <c>app.use(plugin, options)</c>. This can be used directly as a string-keyed
	/// options bag, or inherited when a plugin wants a stronger typed configuration
	/// surface.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VuePluginOptions : IVueOptionsBag
	{
		/// <summary>
		/// Gets or sets an arbitrary plugin option by its final emitted key.
		/// </summary>
		/// <param name="key">The final JavaScript object key to emit.</param>
		/// <returns>The option value mapped to the given key.</returns>
		public extern VueValue? this[string key] { get; set; }
	}

	/// <summary>
	/// Options API computed registry for computed properties that share one value type.
	/// For heterogeneous computed values, declare a custom <see cref="VueProps"/> record
	/// with typed properties instead.
	/// </summary>
	/// <typeparam name="TValue">The computed property value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueComputedRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets a computed property declaration by its final emitted key.
		/// Values can be getter callbacks or writable computed get/set options.
		/// </summary>
		/// <param name="key">The final computed property key.</param>
		/// <returns>The computed declaration for the given key.</returns>
		public extern Either<Func<TValue>, VueWritableComputedOptions<TValue>> this[string key] { get; set; }

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of getter-form computed entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, Func<TValue> getter);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of writable computed entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWritableComputedOptions<TValue> options);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Options API method registry for methods that share one delegate signature.
	/// For heterogeneous method signatures, declare a custom <see cref="VueProps"/>
	/// record with typed delegate properties instead.
	/// </summary>
	/// <typeparam name="TDelegate">The method delegate signature.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueMethodRegistry<TDelegate> : VueProps, System.Collections.IEnumerable
		where TDelegate : Delegate
	{
		/// <summary>
		/// Gets or sets a method declaration by its final emitted key.
		/// </summary>
		/// <param name="key">The final method key.</param>
		/// <returns>The method delegate registered for the given key.</returns>
		public extern TDelegate? this[string key] { get; set; }

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of method entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, TDelegate method);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Single Options API watch declaration entry. This wrapper keeps
	/// watch handler unions strongly typed while allowing natural C#
	/// assignments through implicit conversions.
	/// </summary>
	/// <typeparam name="TValue">The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueWatchEntry<TValue>
	{
		private VueWatchEntry()
		{
		}

		public extern static implicit operator VueWatchEntry<TValue>(string methodName);

		public extern static implicit operator VueWatchEntry<TValue>(Action<TValue, TValue> handler);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchCleanupCallback<TValue> handler);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchHandlerOptions<TValue> options);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchCleanupHandlerOptions<TValue> options);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchNamedHandlerOptions options);
	}

	/// <summary>
	/// Array-form Options API watch declaration entries. Vue runtime accepts
	/// watch value arrays that mix method-name, callback, and object-form
	/// handlers; this wrapper models that surface without requiring compiler
	/// special casing.
	/// </summary>
	/// <typeparam name="TValue">The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueWatchEntries<TValue>
	{
		private VueWatchEntries()
		{
		}

		public extern static implicit operator VueWatchEntries<TValue>(string[] methodNames);

		public extern static implicit operator VueWatchEntries<TValue>(Action<TValue, TValue>[] handlers);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchCleanupCallback<TValue>[] handlers);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchHandlerOptions<TValue>[] options);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchCleanupHandlerOptions<TValue>[] options);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchNamedHandlerOptions[] options);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchEntry<TValue>[] entries);
	}

	/// <summary>
	/// Options API watch registry for watch declarations that share one observed value type.
	/// For heterogeneous watched value types, declare a custom <see cref="VueProps"/> record
	/// with typed watch declaration properties instead.
	/// </summary>
	/// <typeparam name="TValue">The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueWatchRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets a watch declaration by its final emitted key. Keys can be property
		/// names or Vue-supported simple dot paths.
		/// </summary>
		/// <param name="key">The final watch source key.</param>
		/// <returns>The watch declaration for the given key.</returns>
		public extern Either<string, Action<TValue, TValue>, VueWatchCleanupCallback<TValue>, VueWatchHandlerOptions<TValue>, VueWatchCleanupHandlerOptions<TValue>, VueWatchNamedHandlerOptions, VueWatchEntries<TValue>> this[string key] { get; set; }

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of method-name watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, string methodName);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of callback watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, Action<TValue, TValue> handler);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of cleanup-aware watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchCleanupCallback<TValue> handler);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of callback watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchHandlerOptions<TValue> options);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of cleanup-aware watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchCleanupHandlerOptions<TValue> options);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of method-name watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchNamedHandlerOptions options);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of array-form watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchEntries<TValue> entries);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Custom-element-specific options accepted by <c>defineCustomElement()</c> as
	/// its second argument. Normal component options remain authored through
	/// <see cref="VueComponentDefinition"/> and its typed variants.
	/// </summary>
	public record VueCustomElementOptions : IVueOptionsBag
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with no typed props or slots. Use this variant
	/// for simple components that rely on untyped props or have no props at all.
	/// </summary>
	public record VueComponentOptions : VueComponentDefinition
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Object-form prop declarations for validators, defaults, and runtime type checks.
		/// Use either this member or <see cref="PropNames"/>, not both.
		/// </summary>
		[Description("@#props")]
		public VueProps? PropOptions { get; init; }

		/// <summary>
		/// Explicit array-form prop names for untyped components. Use
		/// <see cref="VueComponentOptions{TProps}"/> when a strong props contract is available.
		/// </summary>
		[Description("@#props")]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Object-form emit declarations with runtime validators. Use either this member
		/// or <see cref="EmitNames"/>, not both.
		/// </summary>
		[Description("@#emits")]
		public VueProps? EmitOptions { get; init; }

		/// <summary>
		/// Declared emit event names for this component. Only events listed here will
		/// be emitted to the parent. If omitted, all event listeners passed by the
		/// parent are treated as fallthrough attributes.
		/// </summary>
		[Description("@#emits")]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives no props and
		/// must return a <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueSetupCallback? Setup { get; init; }

		/// <summary>
		/// Render function called directly to produce the component's VNode tree. This is
		/// an alternative to <see cref="Setup"/>; if both are provided, <c>render</c> takes
		/// precedence over the setup return value.
		/// </summary>
		[Description("@#render")]
		public VueRenderCallback? Render { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed props. The generic parameter drives
	/// C# setup and <c>h(...)</c> type checking; runtime <c>props</c> / <c>emits</c>
	/// declarations should be supplied explicitly through the option members when needed.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	public record VueComponentOptions<TProps> : VueComponentDefinition
		where TProps : VueProps
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Object-form prop declarations for validators, defaults, and runtime type checks.
		/// Use either this member or <see cref="PropNames"/>, not both.
		/// </summary>
		[Description("@#props")]
		public VueProps? PropOptions { get; init; }

		/// <summary>
		/// Explicit array-form prop names. Set this when Vue runtime prop declaration is
		/// needed but validators/defaults are not.
		/// </summary>
		[Description("@#props")]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Object-form emit declarations with runtime validators. Use either this member
		/// or <see cref="EmitNames"/>, not both.
		/// </summary>
		[Description("@#emits")]
		public VueProps? EmitOptions { get; init; }

		/// <summary>
		/// Explicit array-form emit event names. Set this when Vue should distinguish
		/// component events from fallthrough listener attributes.
		/// </summary>
		[Description("@#emits")]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a setup context, and must return a <see cref="VueRenderCallback"/> that produces
		/// the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with both typed props and typed slots. The
	/// generic parameters drive C# setup, slot, and <c>h(...)</c> type checking; runtime
	/// <c>props</c> / <c>emits</c> declarations remain explicit option members.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public record VueComponentOptions<TProps, TSlots> : VueComponentDefinition
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Object-form prop declarations for validators, defaults, and runtime type checks.
		/// Use either this member or <see cref="PropNames"/>, not both.
		/// </summary>
		[Description("@#props")]
		public VueProps? PropOptions { get; init; }

		/// <summary>
		/// Explicit array-form prop names. Set this when Vue runtime prop declaration is
		/// needed but validators/defaults are not.
		/// </summary>
		[Description("@#props")]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Object-form emit declarations with runtime validators. Use either this member
		/// or <see cref="EmitNames"/>, not both.
		/// </summary>
		[Description("@#emits")]
		public VueProps? EmitOptions { get; init; }

		/// <summary>
		/// Explicit array-form emit event names. Set this when Vue should distinguish
		/// component events from fallthrough listener attributes.
		/// </summary>
		[Description("@#emits")]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a typed setup context (with typed slot access), and must return a
		/// <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps, TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed slots but no typed props. Use this
	/// variant for components that accept named slots but do not declare typed props.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public record VueSlotComponentOptions<TSlots> : VueComponentDefinition
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Object-form prop declarations for validators, defaults, and runtime type checks.
		/// Use either this member or <see cref="PropNames"/>, not both.
		/// </summary>
		[Description("@#props")]
		public VueProps? PropOptions { get; init; }

		/// <summary>
		/// Explicit array-form prop names for slot-typed components that do not declare
		/// a typed props record.
		/// </summary>
		[Description("@#props")]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Object-form emit declarations with runtime validators. Use either this member
		/// or <see cref="EmitNames"/>, not both.
		/// </summary>
		[Description("@#emits")]
		public VueProps? EmitOptions { get; init; }

		/// <summary>
		/// Emit event names declared by this component. If omitted, all event listeners
		/// passed by the parent are treated as fallthrough attributes.
		/// </summary>
		[Description("@#emits")]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives a typed setup
		/// context with typed slot access, and must return a <see cref="VueRenderCallback"/>
		/// that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSlotSetupCallback<TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// normal untyped component options with custom-element-only options such as
	/// <c>styles</c> and <c>shadowRoot</c>.
	/// </summary>
	public record VueCustomElementComponentOptions : VueComponentOptions
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-props component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TProps">The props contract accepted by the custom element component.</typeparam>
	public record VueCustomElementComponentOptions<TProps> : VueComponentOptions<TProps>
		where TProps : VueProps
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-props/typed-slots component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TProps">The props contract accepted by the custom element component.</typeparam>
	/// <typeparam name="TSlots">The slots contract accepted by the custom element component.</typeparam>
	public record VueCustomElementComponentOptions<TProps, TSlots> : VueComponentOptions<TProps, TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-slots component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TSlots">The slots contract accepted by the custom element component.</typeparam>
	public record VueCustomElementSlotComponentOptions<TSlots> : VueSlotComponentOptions<TSlots>
		where TSlots : VueSlots
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// Options for <c>defineAsyncComponent()</c>. Vue accepts either a loader function
	/// directly or this object form for loading/error components, timing, suspense, and
	/// retry behavior.
	/// </summary>
	public record VueAsyncComponentOptions : IVueOptionsBag
	{
		/// <summary>
		/// Function that loads and resolves the component definition.
		/// </summary>
		[Description("@#loader")]
		public VueAsyncComponentLoader Loader { get; init; } = default!;

		/// <summary>
		/// Component rendered while the async component is loading.
		/// </summary>
		[Description("@#loadingComponent")]
		public IVueComponent? LoadingComponent { get; init; }

		/// <summary>
		/// Component rendered when the async component fails to load.
		/// </summary>
		[Description("@#errorComponent")]
		public IVueComponent? ErrorComponent { get; init; }

		/// <summary>
		/// Delay in milliseconds before showing the loading component.
		/// </summary>
		[Description("@#delay")]
		public Number? Delay { get; init; }

		/// <summary>
		/// Timeout in milliseconds before Vue treats loading as failed.
		/// </summary>
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		/// <summary>
		/// Whether the async component can participate in a parent <c>Suspense</c> boundary.
		/// </summary>
		[Description("@#suspensible")]
		public bool? Suspensible { get; init; }

		/// <summary>
		/// Callback invoked when loading fails; it can retry or fail the async load.
		/// </summary>
		[Description("@#onError")]
		public VueAsyncComponentErrorCallback? OnError { get; init; }
	}

	/// <summary>
	/// Strongly typed options for <c>defineAsyncComponent()</c>. The generic component
	/// contract is preserved by the returned async component reference.
	/// </summary>
	/// <typeparam name="TComponent">The component contract produced by the loader.</typeparam>
	public record VueAsyncComponentOptions<TComponent> : IVueOptionsBag
		where TComponent : IVueComponent
	{
		/// <summary>
		/// Function that loads and resolves the typed component definition.
		/// </summary>
		[Description("@#loader")]
		public VueAsyncComponentLoader<TComponent> Loader { get; init; } = default!;

		/// <summary>
		/// Component rendered while the async component is loading.
		/// </summary>
		[Description("@#loadingComponent")]
		public IVueComponent? LoadingComponent { get; init; }

		/// <summary>
		/// Component rendered when the async component fails to load.
		/// </summary>
		[Description("@#errorComponent")]
		public IVueComponent? ErrorComponent { get; init; }

		/// <summary>
		/// Delay in milliseconds before showing the loading component.
		/// </summary>
		[Description("@#delay")]
		public Number? Delay { get; init; }

		/// <summary>
		/// Timeout in milliseconds before Vue treats loading as failed.
		/// </summary>
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		/// <summary>
		/// Whether the async component can participate in a parent <c>Suspense</c> boundary.
		/// </summary>
		[Description("@#suspensible")]
		public bool? Suspensible { get; init; }

		/// <summary>
		/// Callback invoked when loading fails; it can retry or fail the async load.
		/// </summary>
		[Description("@#onError")]
		public VueAsyncComponentErrorCallback? OnError { get; init; }
	}

	/// <summary>
	/// Transition implementation type used by Vue's <c>Transition</c> and
	/// <c>TransitionGroup</c> built-in components.
	/// </summary>
	public enum VueTransitionType
	{
		[Description("@#transition")]
		Transition,

		[Description("@#animation")]
		Animation
	}

	/// <summary>
	/// Transition sequencing mode for the <c>Transition</c> built-in component.
	/// </summary>
	public enum VueTransitionMode
	{
		[Description("@#in-out")]
		InOut,

		[Description("@#out-in")]
		OutIn
	}

	/// <summary>
	/// Object-form transition duration for entering and leaving phases.
	/// </summary>
	public record VueTransitionDuration : VueProps
	{
		[Description("@#enter")]
		public Number? Enter { get; init; }

		[Description("@#leave")]
		public Number? Leave { get; init; }
	}

	/// <summary>
	/// Transition lifecycle hook receiving the transitioning element.
	/// </summary>
	/// <param name="element">The element currently entering or leaving.</param>
	public delegate void VueTransitionHook(Element element);

	/// <summary>
	/// Transition lifecycle hook that can explicitly complete async transitions.
	/// </summary>
	/// <param name="element">The element currently entering or leaving.</param>
	/// <param name="done">Callback to invoke when the transition phase has completed.</param>
	public delegate void VueTransitionDoneHook(Element element, Action done);

	/// <summary>
	/// Props for Vue's built-in <c>Transition</c> component.
	/// </summary>
	public record VueTransitionProps : VueProps
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#css")]
		public bool? Css { get; init; }

		[Description("@#type")]
		public VueTransitionType? Type { get; init; }

		[Description("@#duration")]
		public Either<Number, VueTransitionDuration>? Duration { get; init; }

		[Description("@#mode")]
		public VueTransitionMode? Mode { get; init; }

		[Description("@#appear")]
		public bool? Appear { get; init; }

		[Description("@#enterFromClass")]
		public string? EnterFromClass { get; init; }

		[Description("@#enterActiveClass")]
		public string? EnterActiveClass { get; init; }

		[Description("@#enterToClass")]
		public string? EnterToClass { get; init; }

		[Description("@#appearFromClass")]
		public string? AppearFromClass { get; init; }

		[Description("@#appearActiveClass")]
		public string? AppearActiveClass { get; init; }

		[Description("@#appearToClass")]
		public string? AppearToClass { get; init; }

		[Description("@#leaveFromClass")]
		public string? LeaveFromClass { get; init; }

		[Description("@#leaveActiveClass")]
		public string? LeaveActiveClass { get; init; }

		[Description("@#leaveToClass")]
		public string? LeaveToClass { get; init; }

		[Description("@#onBeforeEnter")]
		public VueTransitionHook? OnBeforeEnter { get; init; }

		[Description("@#onEnter")]
		public VueTransitionDoneHook? OnEnter { get; init; }

		[Description("@#onAfterEnter")]
		public VueTransitionHook? OnAfterEnter { get; init; }

		[Description("@#onEnterCancelled")]
		public VueTransitionHook? OnEnterCancelled { get; init; }

		[Description("@#onBeforeLeave")]
		public VueTransitionHook? OnBeforeLeave { get; init; }

		[Description("@#onLeave")]
		public VueTransitionDoneHook? OnLeave { get; init; }

		[Description("@#onAfterLeave")]
		public VueTransitionHook? OnAfterLeave { get; init; }

		[Description("@#onLeaveCancelled")]
		public VueTransitionHook? OnLeaveCancelled { get; init; }

		[Description("@#onBeforeAppear")]
		public VueTransitionHook? OnBeforeAppear { get; init; }

		[Description("@#onAppear")]
		public VueTransitionDoneHook? OnAppear { get; init; }

		[Description("@#onAfterAppear")]
		public VueTransitionHook? OnAfterAppear { get; init; }

		[Description("@#onAppearCancelled")]
		public VueTransitionHook? OnAppearCancelled { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>TransitionGroup</c> component.
	/// </summary>
	public record VueTransitionGroupProps : VueProps
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#tag")]
		public string? Tag { get; init; }

		[Description("@#moveClass")]
		public string? MoveClass { get; init; }

		[Description("@#css")]
		public bool? Css { get; init; }

		[Description("@#type")]
		public VueTransitionType? Type { get; init; }

		[Description("@#duration")]
		public Either<Number, VueTransitionDuration>? Duration { get; init; }

		[Description("@#appear")]
		public bool? Appear { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>KeepAlive</c> component.
	/// </summary>
	public record VueKeepAliveProps : VueProps
	{
		[Description("@#include")]
		public Either<string, RegExp, string[], RegExp[]>? Include { get; init; }

		[Description("@#exclude")]
		public Either<string, RegExp, string[], RegExp[]>? Exclude { get; init; }

		[Description("@#max")]
		public Either<int, string>? Max { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>Teleport</c> component.
	/// </summary>
	public record VueTeleportProps : VueProps
	{
		[Description("@#to")]
		public Either<string, Element>? To { get; init; }

		[Description("@#disabled")]
		public bool? Disabled { get; init; }

		[Description("@#defer")]
		public bool? Defer { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>Suspense</c> component.
	/// </summary>
	public record VueSuspenseProps : VueProps
	{
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		[Description("@#onPending")]
		public Action? OnPending { get; init; }

		[Description("@#onResolve")]
		public Action? OnResolve { get; init; }

		[Description("@#onFallback")]
		public Action? OnFallback { get; init; }
	}

	/// <summary>
	/// Slots accepted by Vue's built-in <c>Suspense</c> component.
	/// </summary>
	public record VueSuspenseSlots : VueSlots
	{
		[Description("@#default")]
		public VueSlotCallback? Default { get; init; }

		[Description("@#fallback")]
		public VueSlotCallback? Fallback { get; init; }
	}

	/// <summary>
	/// Application-level uncaught error handler configured through <c>app.config</c>.
	/// Vue's error value is unknown-like, so this uses <see cref="VueValue"/> instead
	/// of exposing <c>object</c> on the public Vue surface.
	/// </summary>
	public delegate void VueAppErrorHandler(VueValue? error, VueComponentPublicInstance? instance, string info);

	/// <summary>
	/// Application-level runtime warning handler configured through <c>app.config</c>.
	/// </summary>
	public delegate void VueAppWarnHandler(string message, VueComponentPublicInstance? instance, string trace);

	/// <summary>
	/// Runtime compiler predicate that marks tags as native custom elements.
	/// </summary>
	public delegate bool VueIsCustomElementCallback(string tag);

	/// <summary>
	/// Merge function for custom Options API option keys.
	/// </summary>
	public delegate VueValue? VueOptionMergeFunction(VueValue? parent, VueValue? child);

	/// <summary>
	/// Runtime compiler whitespace handling mode.
	/// </summary>
	public enum VueCompilerWhitespace
	{
		[Description("@#condense")]
		Condense,

		[Description("@#preserve")]
		Preserve
	}

	/// <summary>
	/// Bag of app-level global properties available on every component instance.
	/// </summary>
	public abstract class VueGlobalProperties
	{
		protected VueGlobalProperties()
		{
		}

		/// <summary>
		/// Gets or sets a global property by its final runtime key.
		/// </summary>
		public extern VueValue? this[string key] { get; set; }
	}

	/// <summary>
	/// Bag of app-level custom option merge strategies.
	/// </summary>
	public abstract class VueOptionMergeStrategies
	{
		protected VueOptionMergeStrategies()
		{
		}

		/// <summary>
		/// Gets or sets a merge strategy by custom option name.
		/// </summary>
		public extern VueOptionMergeFunction? this[string key] { get; set; }
	}

	/// <summary>
	/// Runtime compiler options exposed through <c>app.config.compilerOptions</c>.
	/// These only affect apps using Vue's in-browser template compiler.
	/// </summary>
	public abstract class VueAppCompilerOptions
	{
		protected VueAppCompilerOptions()
		{
		}

		[Description("@#isCustomElement")]
		public extern VueIsCustomElementCallback? IsCustomElement { get; set; }

		[Description("@#whitespace")]
		public extern VueCompilerWhitespace? Whitespace { get; set; }

		[Description("@#delimiters")]
		public extern string[]? Delimiters { get; set; }

		[Description("@#comments")]
		public extern bool Comments { get; set; }
	}

	/// <summary>
	/// Vue application configuration exposed by <c>app.config</c>.
	/// </summary>
	public abstract class VueAppConfig
	{
		protected VueAppConfig()
		{
		}

		[Description("@#errorHandler")]
		public extern VueAppErrorHandler? ErrorHandler { get; set; }

		[Description("@#warnHandler")]
		public extern VueAppWarnHandler? WarnHandler { get; set; }

		[Description("@#performance")]
		public extern bool Performance { get; set; }

		[Description("@#compilerOptions")]
		public extern VueAppCompilerOptions CompilerOptions { get; }

		[Description("@#globalProperties")]
		public extern VueGlobalProperties GlobalProperties { get; }

		[Description("@#optionMergeStrategies")]
		public extern VueOptionMergeStrategies OptionMergeStrategies { get; }

		[Description("@#idPrefix")]
		public extern string? IdPrefix { get; set; }

		[Description("@#throwUnhandledErrorInProduction")]
		public extern bool ThrowUnhandledErrorInProduction { get; set; }
	}

	/// <summary>
	/// A readonly reactive reference. Only the <c>value</c> getter is available; writes
	/// are not permitted. Typically created by <see cref="Computed{T}"/> or <c>readonly()</c>.
	/// </summary>
	/// <typeparam name="T">The type of the wrapped value.</typeparam>
	public class VueReadonlyRef<T>
	{
		/// <summary>
		/// Gets the current value. Reads are tracked as reactive dependencies.
		/// </summary>
		[Description("@#value")]
		public extern T Value { get; }
	}

	/// <summary>
	/// Untyped refs object returned by <c>toRefs()</c>. Keys are final runtime property
	/// names and values are linked refs for those properties.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueRefs
	{
		protected VueRefs()
		{
		}

		/// <summary>
		/// Reads a linked ref by runtime property name.
		/// </summary>
		/// <param name="key">The source object's final runtime property name.</param>
		/// <returns>The linked ref when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
		public extern IVueRef<VueValue>? this[string key] { get; }
	}

	/// <summary>
	/// Typed base for user-defined <c>toRefs()</c> projections. Inherit from this type
	/// and declare <c>IVueRef&lt;T&gt;</c> properties to get C# IntelliSense over the
	/// refs object returned by Vue.
	/// </summary>
	/// <typeparam name="TSource">The source reactive object contract.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueRefs<TSource> : VueRefs
		where TSource : class
	{
		protected VueRefs()
		{
		}
	}

	/// <summary>
	/// Flush timing for Vue watcher callbacks.
	/// </summary>
	public enum VueWatchFlush
	{
		/// <summary>
		/// Run before component rendering. This is Vue's default watcher flush timing.
		/// </summary>
		[Description("@#pre")]
		Pre,

		/// <summary>
		/// Run after component rendering has flushed.
		/// </summary>
		[Description("@#post")]
		Post,

		/// <summary>
		/// Run synchronously when a dependency changes.
		/// </summary>
		[Description("@#sync")]
		Sync
	}

	/// <summary>
	/// Reactivity debugger event operation kind supplied to watcher debug hooks.
	/// </summary>
	public enum VueDebuggerEventType
	{
		[Description("@#get")]
		Get,

		[Description("@#has")]
		Has,

		[Description("@#iterate")]
		Iterate,

		[Description("@#set")]
		Set,

		[Description("@#add")]
		Add,

		[Description("@#delete")]
		Delete,

		[Description("@#clear")]
		Clear
	}

	/// <summary>
	/// Debug information supplied to <c>onTrack</c> and <c>onTrigger</c> watcher
	/// options. Runtime values are unknown-like Vue internals, so the value-bearing
	/// members use <see cref="VueValue"/> instead of <c>object</c>.
	/// </summary>
	public abstract class VueDebuggerEvent
	{
		protected VueDebuggerEvent()
		{
		}

		[Description("@#effect")]
		public extern VueValue? Effect { get; }

		[Description("@#target")]
		public extern VueValue? Target { get; }

		[Description("@#type")]
		public extern VueDebuggerEventType Type { get; }

		[Description("@#key")]
		public extern VueValue? Key { get; }

		[Description("@#newValue")]
		public extern VueValue? NewValue { get; }

		[Description("@#oldValue")]
		public extern VueValue? OldValue { get; }

		[Description("@#oldTarget")]
		public extern VueValue? OldTarget { get; }
	}

	/// <summary>
	/// Options shared by <c>watchEffect()</c>, <c>watchPostEffect()</c>, and
	/// <c>watchSyncEffect()</c>. Maps directly to Vue's plain options object.
	/// </summary>
	public record VueWatchEffectOptions : IVueOptionsBag
	{
		/// <summary>
		/// Controls when the watcher callback is flushed relative to component rendering.
		/// </summary>
		[Description("@#flush")]
		public VueWatchFlush? Flush { get; init; }

		/// <summary>
		/// Debug callback invoked when reactive dependencies are tracked.
		/// </summary>
		[Description("@#onTrack")]
		public VueDebuggerCallback? OnTrack { get; init; }

		/// <summary>
		/// Debug callback invoked when a tracked dependency triggers the watcher.
		/// </summary>
		[Description("@#onTrigger")]
		public VueDebuggerCallback? OnTrigger { get; init; }
	}

	/// <summary>
	/// Options for <c>watch()</c>. This extends effect options with source-specific
	/// behavior such as eager execution, deep traversal, and one-shot watches.
	/// </summary>
	public record VueWatchOptions : VueWatchEffectOptions
	{
		/// <summary>
		/// Run the callback immediately with the current value.
		/// </summary>
		[Description("@#immediate")]
		public bool? Immediate { get; init; }

		/// <summary>
		/// Traverse nested properties. Use <c>true</c> for full traversal or an integer
		/// depth limit when only a bounded traversal is needed.
		/// </summary>
		[Description("@#deep")]
		public Either<bool, int>? Deep { get; init; }

		/// <summary>
		/// Stop the watcher automatically after the first callback run.
		/// </summary>
		[Description("@#once")]
		public bool? Once { get; init; }
	}

	/// <summary>
	/// Options API watch declaration whose handler is a strongly typed callback.
	/// </summary>
	/// <typeparam name="T">The watched value type.</typeparam>
	public record VueWatchHandlerOptions<T> : VueWatchOptions
	{
		/// <summary>
		/// Callback invoked with the current and previous values.
		/// </summary>
		[Description("@#handler")]
		public Action<T, T> Handler { get; init; } = default!;
	}

	/// <summary>
	/// Options API watch declaration whose handler receives Vue's cleanup registration
	/// callback in addition to the current and previous values.
	/// </summary>
	/// <typeparam name="T">The watched value type.</typeparam>
	public record VueWatchCleanupHandlerOptions<T> : VueWatchOptions
	{
		/// <summary>
		/// Cleanup-aware callback invoked with the current value, previous value, and cleanup registration.
		/// </summary>
		[Description("@#handler")]
		public VueWatchCleanupCallback<T> Handler { get; init; } = default!;
	}

	/// <summary>
	/// Options API watch declaration whose handler is resolved by Vue from the component
	/// <c>methods</c> object.
	/// </summary>
	public record VueWatchNamedHandlerOptions : VueWatchOptions
	{
		/// <summary>
		/// Method name to resolve from the same component's <c>methods</c> option.
		/// </summary>
		[Description("@#handler")]
		public string Handler { get; init; } = default!;
	}

	/// <summary>
	/// Options for <c>useModel()</c>. Vue applies these transforms when reading from
	/// and writing to the model ref.
	/// </summary>
	/// <typeparam name="T">The model value type.</typeparam>
	public record VueModelOptions<T> : IVueOptionsBag
	{
		/// <summary>
		/// Transform the prop value when reading the model ref.
		/// </summary>
		[Description("@#get")]
		public Func<T, T>? Get { get; init; }

		/// <summary>
		/// Transform the assigned value before Vue emits the update event.
		/// </summary>
		[Description("@#set")]
		public Func<T, T>? Set { get; init; }
	}

	/// <summary>
	/// Writable computed options. Vue expects a plain object with <c>get</c> and
	/// <c>set</c> members; C# exposes those as strongly typed delegates.
	/// </summary>
	/// <typeparam name="T">The computed value type.</typeparam>
	public record VueWritableComputedOptions<T> : IVueOptionsBag
	{
		/// <summary>
		/// Getter used by Vue to compute the current value.
		/// </summary>
		[Description("@#get")]
		public Func<T> Get { get; init; } = default!;

		/// <summary>
		/// Setter invoked when the computed ref is assigned.
		/// </summary>
		[Description("@#set")]
		public Action<T> Set { get; init; } = default!;
	}

	/// <summary>
	/// Get/set handlers returned by a <c>customRef()</c> factory.
	/// </summary>
	/// <typeparam name="T">The custom ref value type.</typeparam>
	public record VueCustomRefHandlers<T> : IVueOptionsBag
	{
		/// <summary>
		/// Getter used by Vue when the custom ref's <c>value</c> is read.
		/// </summary>
		[Description("@#get")]
		public Func<T> Get { get; init; } = default!;

		/// <summary>
		/// Setter used by Vue when the custom ref's <c>value</c> is assigned.
		/// </summary>
		[Description("@#set")]
		public Action<T> Set { get; init; } = default!;
	}

	/// <summary>
	/// Runtime effect scope returned by <c>effectScope()</c>. Effects created while a
	/// scope is active can be stopped together through the scope.
	/// </summary>
	public abstract class VueEffectScope
	{
		protected VueEffectScope()
		{
		}

		/// <summary>
		/// Run a callback inside this effect scope.
		/// </summary>
		/// <typeparam name="TResult">The callback return type.</typeparam>
		/// <param name="callback">The callback to execute while this scope is active.</param>
		/// <returns>The callback result.</returns>
		[Description("@#run")]
		public extern TResult Run<TResult>(Func<TResult> callback);

		/// <summary>
		/// Stop every effect captured by this scope.
		/// </summary>
		[Description("@#stop")]
		public extern void Stop();
	}

	/// <summary>
	/// Represents the public instance of a mounted Vue component. Obtained from
	/// <see cref="VueApp.Mount(string)"/> and used for testing or programmatic access
	/// to the component's public properties exposed via <c>expose()</c>.
	/// </summary>
	public sealed class VueComponentPublicInstance
	{
		private VueComponentPublicInstance()
		{
		}
	}

	/// <summary>
	/// Setup context available inside the <c>setup()</c> function. Provides access to
	/// fallthrough attributes, slots, event emission, and public instance exposure.
	/// </summary>
	public abstract class VueSetupContext
	{
		/// <summary>
		/// Fallthrough attributes passed to the component but not declared as props.
		/// Includes <c>class</c>, <c>style</c>, and event listeners when <c>inheritAttrs</c> is <c>true</c>.
		/// </summary>
		[Description("@#attrs")]
		public extern VueAttributeBag Attrs { get; }

		/// <summary>
		/// Slots available in the component. Use this to render default or named slot content
		/// via <c>context.slots.default?.()</c>.
		/// </summary>
		[Description("@#slots")]
		public extern VueSlotBag Slots { get; }

		/// <summary>
		/// Emit a custom event by name with no payload. The parent component can listen
		/// via <c>v-on:eventName</c> or <c>@eventName</c>.
		/// </summary>
		/// <param name="eventName">The name of the event to emit (e.g. <c>"close"</c>).</param>
		[Description("@#emit")]
		public extern void Emit(string eventName);

		/// <summary>
		/// Emit a custom event by name with a single typed payload value.
		/// </summary>
		/// <typeparam name="TValue">The type of the event payload.</typeparam>
		/// <param name="eventName">The name of the event to emit (e.g. <c>"update:modelValue"</c>).</param>
		/// <param name="value">The payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<TValue>(string eventName, TValue value);

		/// <summary>
		/// Emit a custom event by name with two typed payload values.
		/// </summary>
		/// <typeparam name="T0">The type of the first payload value.</typeparam>
		/// <typeparam name="T1">The type of the second payload value.</typeparam>
		/// <param name="eventName">The name of the event to emit (e.g. <c>"update"</c>).</param>
		/// <param name="value0">The first payload value sent with the event.</param>
		/// <param name="value1">The second payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1>(string eventName, T0 value0, T1 value1);

		/// <summary>
		/// Emit a custom event by name with three typed payload values.
		/// </summary>
		/// <typeparam name="T0">The type of the first payload value.</typeparam>
		/// <typeparam name="T1">The type of the second payload value.</typeparam>
		/// <typeparam name="T2">The type of the third payload value.</typeparam>
		/// <param name="eventName">The name of the event to emit.</param>
		/// <param name="value0">The first payload value sent with the event.</param>
		/// <param name="value1">The second payload value sent with the event.</param>
		/// <param name="value2">The third payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1, T2>(string eventName, T0 value0, T1 value1, T2 value2);

		/// <summary>
		/// Emit a custom event by name with four typed payload values.
		/// </summary>
		/// <typeparam name="T0">The type of the first payload value.</typeparam>
		/// <typeparam name="T1">The type of the second payload value.</typeparam>
		/// <typeparam name="T2">The type of the third payload value.</typeparam>
		/// <typeparam name="T3">The type of the fourth payload value.</typeparam>
		/// <param name="eventName">The name of the event to emit.</param>
		/// <param name="value0">The first payload value sent with the event.</param>
		/// <param name="value1">The second payload value sent with the event.</param>
		/// <param name="value2">The third payload value sent with the event.</param>
		/// <param name="value3">The fourth payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1, T2, T3>(string eventName, T0 value0, T1 value1, T2 value2, T3 value3);

		/// <summary>
		/// Expose a value on the component's public instance so parent components can
		/// access it via template refs (<c>ref="..."</c>). Only exposed values are
		/// accessible from the parent; all other internal state is hidden.
		/// </summary>
		/// <typeparam name="TValue">The type of the exposed value (must be a reference type).</typeparam>
		/// <param name="exposed">The object or value to expose on the public instance.</param>
		[Description("@#expose")]
		public extern void Expose<TValue>(TValue exposed) where TValue : class;
	}

	/// <summary>
	/// Typed setup context that provides typed slot access in addition to the standard
	/// <see cref="VueSetupContext"/> members. The <c>Slots</c> property returns the
	/// typed <typeparamref name="TSlots"/> record instead of the untyped <see cref="VueSlotBag"/>.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type declared by the component.</typeparam>
	public abstract class VueSetupContext<TSlots> : VueSetupContext
		where TSlots : VueSlots
	{
		/// <summary>
		/// Typed slots available in the component. Each property on <typeparamref name="TSlots"/>
		/// maps to a named slot that can be invoked to produce its VNode content.
		/// </summary>
		[Description("@#slots")]
		public new extern TSlots Slots { get; }
	}

	/// <summary>
	/// Bag of fallthrough attributes (<c>v-bind="$attrs"</c>). Contains attributes
	/// passed to the component that are not declared as props, including <c>class</c>,
	/// <c>style</c>, and event listeners.
	/// </summary>
	public abstract class VueAttributeBag
	{
		protected VueAttributeBag()
		{
		}

		/// <summary>
		/// Reads an arbitrary fallthrough attribute by its final emitted key.
		/// </summary>
		/// <param name="key">The final JavaScript attribute key.</param>
		/// <returns>The attribute value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
		public extern VueValue? this[string key] { get; }

		/// <summary>
		/// Reads the fallthrough <c>class</c> binding.
		/// </summary>
		[Description("@#class")]
		public extern Either<string, string[], VueProps, VueValue[]>? Class { get; }

		/// <summary>
		/// Reads the fallthrough <c>style</c> binding.
		/// </summary>
		[Description("@#style")]
		public extern VueProps? Style { get; }

		/// <summary>
		/// Reads the fallthrough <c>id</c> attribute.
		/// </summary>
		[Description("@#id")]
		public extern string? Id { get; }

		/// <summary>
		/// Reads the fallthrough <c>title</c> attribute.
		/// </summary>
		[Description("@#title")]
		public extern string? Title { get; }
	}

	/// <summary>
	/// Bag of available slots (<c>$slots</c>). Each property is a callable slot
	/// function that returns VNode content.
	/// </summary>
	public abstract class VueSlotBag
	{
		protected VueSlotBag()
		{
		}

		/// <summary>
		/// Reads an arbitrary slot callback by its final slot name.
		/// </summary>
		/// <param name="key">The final Vue slot key.</param>
		/// <returns>The slot callback when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
		public extern VueSlotCallback? this[string key] { get; }

		/// <summary>
		/// Reads the default slot callback when present.
		/// </summary>
		[Description("@#default")]
		public extern VueSlotCallback? Default { get; }
	}

	/// <summary>
	/// Bag of directive modifiers. Each key corresponds to a modifier name used at the
	/// directive call site, for example <c>v-colorize.primary</c> exposing
	/// <c>binding.modifiers["primary"]</c>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveModifiers
	{
		protected VueDirectiveModifiers()
		{
		}

		/// <summary>
		/// Returns whether the given modifier flag is present on the current directive usage.
		/// </summary>
		/// <param name="key">The modifier name to check.</param>
		/// <returns><c>true</c> when the modifier is present; otherwise <c>false</c>.</returns>
		public extern bool this[string key] { get; }
	}

	/// <summary>
	/// Write-side modifier object used in <c>withDirectives()</c> directive argument
	/// tuples. Keys are final modifier names and values indicate whether the modifier is
	/// present.
	/// </summary>
	public record VueDirectiveModifierBag : VueDictionary<bool>;

	/// <summary>
	/// One directive argument tuple accepted by Vue's <c>withDirectives()</c> helper.
	/// This maps to JavaScript <c>Array</c> so the emitted runtime shape is compatible
	/// with Vue's <c>[directive, value, argument, modifiers]</c> tuple contract.
	/// </summary>
	[ECMAScript]
	[Description("@#Array")]
	public class VueDirectiveArguments
	{
		protected VueDirectiveArguments()
		{
		}

		/// <summary>
		/// Applies a directive with no explicit value, argument, or modifiers.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive);

		/// <summary>
		/// Applies a directive with a value.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		/// <param name="value">The directive value.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value);

		/// <summary>
		/// Applies a directive with a value and argument.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value, string arg);

		/// <summary>
		/// Applies a directive with the full Vue tuple shape.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument. Use <c>null</c> when only modifiers are needed.</param>
		/// <param name="modifiers">The directive modifier flags.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value, string? arg, VueDirectiveModifierBag modifiers);
	}

	/// <summary>
	/// Strongly typed directive argument tuple. The generic argument keeps the supplied
	/// value aligned with the typed directive definition while preserving Vue's runtime
	/// array tuple shape.
	/// </summary>
	/// <typeparam name="TValue">The directive value contract.</typeparam>
	[ECMAScript]
	[Description("@#Array")]
	public sealed class VueDirectiveArguments<TValue> : VueDirectiveArguments
	{
		/// <summary>
		/// Applies a typed directive with a typed value.
		/// </summary>
		/// <param name="directive">The typed directive definition.</param>
		/// <param name="value">The directive value.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value);

		/// <summary>
		/// Applies a typed directive with a typed value and argument.
		/// </summary>
		/// <param name="directive">The typed directive definition.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value, string arg);

		/// <summary>
		/// Applies a typed directive with the full Vue tuple shape.
		/// </summary>
		/// <param name="directive">The typed directive definition.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument. Use <c>null</c> when only modifiers are needed.</param>
		/// <param name="modifiers">The directive modifier flags.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value, string? arg, VueDirectiveModifierBag modifiers);
	}

	/// <summary>
	/// Current runtime binding payload for a directive lifecycle hook.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveBinding
	{
		protected VueDirectiveBinding()
		{
		}

		/// <summary>
		/// Current directive value passed by the user. For richer non-primitive contracts,
		/// prefer the generic <see cref="VueDirectiveBinding{TValue}"/>.
		/// </summary>
		[Description("@#value")]
		public extern VueValue Value { get; }

		/// <summary>
		/// Dynamic argument segment provided to the directive, such as <c>focus</c> in
		/// <c>v-demo:focus</c>.
		/// </summary>
		[Description("@#arg")]
		public extern string? Arg { get; }

		/// <summary>
		/// Modifier flags provided to the directive call site.
		/// </summary>
		[Description("@#modifiers")]
		public extern VueDirectiveModifiers Modifiers { get; }

		/// <summary>
		/// Component public instance that owns the directive usage, when available.
		/// </summary>
		[Description("@#instance")]
		public extern VueComponentPublicInstance? Instance { get; }

		/// <summary>
		/// The directive definition currently being invoked.
		/// </summary>
		[Description("@#dir")]
		public extern VueDirective Dir { get; }
	}

	/// <summary>
	/// Current runtime binding payload for a directive lifecycle hook with a strongly typed value contract.
	/// </summary>
	/// <typeparam name="TValue">The typed contract of the directive's current binding value.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveBinding<TValue> : VueDirectiveBinding
	{
		protected VueDirectiveBinding()
		{
		}

		/// <summary>
		/// Current directive value passed by the user.
		/// </summary>
		[Description("@#value")]
		public new extern TValue Value { get; }

		/// <summary>
		/// The typed directive definition currently being invoked.
		/// </summary>
		[Description("@#dir")]
		public new extern VueDirective<TValue> Dir { get; }
	}

	/// <summary>
	/// Runtime binding payload for a directive update hook, including the previous value.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveUpdateBinding : VueDirectiveBinding
	{
		protected VueDirectiveUpdateBinding()
		{
		}

		/// <summary>
		/// Previous directive value observed on the same element during the preceding update cycle.
		/// </summary>
		[Description("@#oldValue")]
		public extern VueValue OldValue { get; }
	}

	/// <summary>
	/// Typed runtime binding payload for a directive update hook, including the previous value.
	/// </summary>
	/// <typeparam name="TValue">The typed contract of the directive's current and previous binding values.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveUpdateBinding<TValue> : VueDirectiveBinding<TValue>
	{
		protected VueDirectiveUpdateBinding()
		{
		}

		/// <summary>
		/// Previous directive value observed on the same element during the preceding update cycle.
		/// </summary>
		[Description("@#oldValue")]
		public extern TValue OldValue { get; }
	}

	/// <summary>
	/// Union-like directive value contract used at registration and retrieval boundaries
	/// where Vue accepts either an object-form directive or a function shorthand.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public class VueDirectiveValue
	{
		protected VueDirectiveValue()
		{
		}

		public extern static implicit operator VueDirectiveValue(VueDirective value);

		public extern static implicit operator VueDirectiveValue(VueDirectiveFunction value);
	}

	/// <summary>
	/// Direct object-form Vue directive authoring surface. This maps to a plain JavaScript
	/// directive object whose lifecycle hooks are invoked by Vue.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDirective : IVueOptionsBag
	{
		/// <summary>
		/// Marks the directive as deep, so Vue traverses nested values for change detection.
		/// </summary>
		[Description("@#deep")]
		public bool? Deep { get; init; }

		/// <summary>
		/// Called before any attributes or listeners are applied to the element.
		/// </summary>
		[Description("@#created")]
		public VueDirectiveHook? Created { get; init; }

		/// <summary>
		/// Called right before the element is inserted into the DOM.
		/// </summary>
		[Description("@#beforeMount")]
		public VueDirectiveHook? BeforeMount { get; init; }

		/// <summary>
		/// Called after the element is inserted into the DOM.
		/// </summary>
		[Description("@#mounted")]
		public VueDirectiveHook? Mounted { get; init; }

		/// <summary>
		/// Called right before the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#beforeUpdate")]
		public VueDirectiveUpdateHook? BeforeUpdate { get; init; }

		/// <summary>
		/// Called after the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#updated")]
		public VueDirectiveUpdateHook? Updated { get; init; }

		/// <summary>
		/// Called right before the containing component unmounts the element.
		/// </summary>
		[Description("@#beforeUnmount")]
		public VueDirectiveHook? BeforeUnmount { get; init; }

		/// <summary>
		/// Called after the containing component unmounts the element.
		/// </summary>
		[Description("@#unmounted")]
		public VueDirectiveHook? Unmounted { get; init; }

		/// <summary>
		/// Called during SSR to contribute additional props to the rendered element.
		/// </summary>
		[Description("@#getSSRProps")]
		public VueDirectiveSsrPropsCallback? GetSsrProps { get; init; }
	}

	/// <summary>
	/// Typed object-form Vue directive authoring surface. This keeps the directive's binding
	/// value strongly typed while still lowering to the same plain JavaScript directive object.
	/// </summary>
	/// <typeparam name="TValue">The typed contract of the directive's binding value.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueDirective<TValue> : VueDirective
	{
		/// <summary>
		/// Called before any attributes or listeners are applied to the element.
		/// </summary>
		[Description("@#created")]
		public new VueDirectiveHook<TValue>? Created { get; init; }

		/// <summary>
		/// Called right before the element is inserted into the DOM.
		/// </summary>
		[Description("@#beforeMount")]
		public new VueDirectiveHook<TValue>? BeforeMount { get; init; }

		/// <summary>
		/// Called after the element is inserted into the DOM.
		/// </summary>
		[Description("@#mounted")]
		public new VueDirectiveHook<TValue>? Mounted { get; init; }

		/// <summary>
		/// Called right before the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#beforeUpdate")]
		public new VueDirectiveUpdateHook<TValue>? BeforeUpdate { get; init; }

		/// <summary>
		/// Called after the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#updated")]
		public new VueDirectiveUpdateHook<TValue>? Updated { get; init; }

		/// <summary>
		/// Called right before the containing component unmounts the element.
		/// </summary>
		[Description("@#beforeUnmount")]
		public new VueDirectiveHook<TValue>? BeforeUnmount { get; init; }

		/// <summary>
		/// Called after the containing component unmounts the element.
		/// </summary>
		[Description("@#unmounted")]
		public new VueDirectiveHook<TValue>? Unmounted { get; init; }

		/// <summary>
		/// Called during SSR to contribute additional props to the rendered element.
		/// </summary>
		[Description("@#getSSRProps")]
		public new VueDirectiveSsrPropsCallback<TValue>? GetSsrProps { get; init; }
	}

	/// <summary>
	/// Direct object-form Vue plugin authoring surface. This maps to a plain JavaScript
	/// object with an <c>install(app)</c> function and can be passed directly to
	/// <see cref="VueApp.Use(VuePlugin)"/> or <see cref="VueApp.Use(VuePlugin, VuePluginOptions)"/>.
	/// For typed install options, use <see cref="VuePlugin{TOptions}"/>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VuePlugin : IVueOptionsBag
	{
		/// <summary>
		/// Plugin installation entrypoint. Vue calls this when <c>app.use(plugin)</c> runs.
		/// </summary>
		[Description("@#install")]
		public VuePluginInstallCallback? Install { get; init; }
	}

	/// <summary>
	/// Typed object-form Vue plugin authoring surface. This maps to a plain JavaScript
	/// object with an <c>install(app, options)</c> function, where the options value
	/// keeps the declared <typeparamref name="TOptions"/> contract at authoring time.
	/// </summary>
	/// <typeparam name="TOptions">The typed install options contract.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VuePlugin<TOptions> : IVueOptionsBag
		where TOptions : VuePluginOptions
	{
		/// <summary>
		/// Plugin installation entrypoint. Vue calls this when
		/// <c>app.use(plugin, options)</c> runs for the current plugin instance.
		/// </summary>
		[Description("@#install")]
		public VuePluginInstallCallback<TOptions>? Install { get; init; }
	}

	/// <summary>
	/// A Vue application instance created by <c>createApp()</c>. Provides methods for
	/// mounting, configuration, and global registration of components, directives,
	/// and plugins.
	/// </summary>
	public abstract class VueApp
	{
		/// <summary>
		/// The version of Vue that created this application instance.
		/// </summary>
		[Description("@#version")]
		public extern string Version { get; }

		/// <summary>
		/// Application-scoped Vue configuration object. Mutate this before mounting the
		/// app to configure error handling, runtime compiler behavior, globals, and
		/// custom option merge strategies.
		/// </summary>
		[Description("@#config")]
		public extern VueAppConfig Config { get; }

		/// <summary>
		/// Mount the application to the first DOM element matching the given CSS selector.
		/// The mounted component becomes the root of the application's component tree.
		/// </summary>
		/// <param name="selector">A CSS selector string (e.g. <c>"#app"</c>) identifying the mount point.</param>
		/// <returns>The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(string selector);

		/// <summary>
		/// Mount the application directly to a specific DOM element.
		/// </summary>
		/// <param name="container">The DOM element to mount into. The element's existing content is replaced.</param>
		/// <returns>The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(Element container);

		/// <summary>
		/// Unmount the application, destroying the component tree and cleaning up all
		/// reactive effects, watchers, and event listeners.
		/// </summary>
		[Description("@#unmount")]
		public extern void Unmount();

		/// <summary>
		/// Register a callback to run when the application is unmounted.
		/// </summary>
		/// <param name="callback">The callback to run during application unmount.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#onUnmount")]
		public extern VueApp OnUnmount(Action callback);

		/// <summary>
		/// Install a Vue plugin with no configuration options. The plugin's <c>install()</c>
		/// method receives the app instance.
		/// </summary>
		/// <param name="plugin">The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin);

		/// <summary>
		/// Install a Vue plugin with configuration options. The plugin's <c>install()</c>
		/// method receives the app instance and the options object.
		/// </summary>
		/// <param name="plugin">The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <param name="options">Plugin-specific configuration. Must inherit from <see cref="VuePluginOptions"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin, VuePluginOptions options);

		/// <summary>
		/// Install a function-form Vue plugin with no configuration options. The callback
		/// itself acts as the plugin installation entrypoint.
		/// </summary>
		/// <param name="plugin">The function-form plugin install callback.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePluginInstallCallback plugin);

		/// <summary>
		/// Install a function-form Vue plugin with configuration options. Vue passes the
		/// supplied options as the second argument when invoking the plugin callback.
		/// </summary>
		/// <param name="plugin">The function-form plugin install callback.</param>
		/// <param name="options">Plugin-specific configuration.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePluginInstallCallback plugin, VuePluginOptions options);

		/// <summary>
		/// Install a typed object-form Vue plugin with strongly typed configuration options.
		/// </summary>
		/// <typeparam name="TOptions">The typed plugin options contract.</typeparam>
		/// <param name="plugin">The typed object-form plugin to install.</param>
		/// <param name="options">The strongly typed options value passed to the plugin.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use<TOptions>(VuePlugin<TOptions> plugin, TOptions options)
			where TOptions : VuePluginOptions;

		/// <summary>
		/// Install a function-form Vue plugin with strongly typed configuration options.
		/// </summary>
		/// <typeparam name="TOptions">The typed plugin options contract.</typeparam>
		/// <param name="plugin">The typed function-form plugin install callback.</param>
		/// <param name="options">The strongly typed options value passed to the plugin.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use<TOptions>(VuePluginInstallCallback<TOptions> plugin, TOptions options)
			where TOptions : VuePluginOptions;

		/// <summary>
		/// Apply a global mixin to every component instance created in this app. Vue's
		/// documentation does not recommend global mixins for application code; prefer
		/// explicit composition unless a library integration specifically needs this hook.
		/// </summary>
		/// <param name="mixin">The component options object to merge into every component.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#mixin")]
		public extern VueApp Mixin(VueComponentDefinition mixin);

		/// <summary>
		/// Register a global component by name, making it available in all component
		/// templates without explicit import.
		/// </summary>
		/// <param name="name">The component name to register (e.g. <c>"MyButton"</c>).</param>
		/// <param name="component">The component definition, produced by <c>defineComponent()</c>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#component")]
		public extern VueApp Component(string name, IVueComponent component);

		/// <summary>
		/// Retrieve a previously registered global component by name.
		/// </summary>
		/// <param name="name">The registered component name to look up.</param>
		/// <returns>The component definition registered under the given name.</returns>
		[Description("@#component")]
		public extern IVueComponent Component(string name);

		/// <summary>
		/// Register a global custom directive by name, making it available in all component
		/// templates as <c>v-name</c>.
		/// </summary>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix, e.g. <c>"focus"</c>).</param>
		/// <param name="directive">The directive definition. Must inherit from <see cref="VueDirective"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirective directive);

		/// <summary>
		/// Register a global custom directive by name with a strongly typed binding value contract.
		/// </summary>
		/// <typeparam name="TValue">The typed contract of the directive's binding value.</typeparam>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">The typed directive definition to register.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive<TValue>(string name, VueDirective<TValue> directive);

		/// <summary>
		/// Register a global custom directive by name using Vue's function shorthand.
		/// Vue invokes the same callback for both the <c>mounted</c> and <c>updated</c> phases.
		/// </summary>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">The function shorthand callback.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirectiveFunction directive);

		/// <summary>
		/// Register a global custom directive by name using Vue's function shorthand with a strongly typed binding value contract.
		/// Vue invokes the same callback for both the <c>mounted</c> and <c>updated</c> phases.
		/// </summary>
		/// <typeparam name="TValue">The typed contract of the directive's binding value.</typeparam>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">The typed function shorthand callback.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive<TValue>(string name, VueDirectiveFunction<TValue> directive);

		/// <summary>
		/// Retrieve a previously registered global directive by name.
		/// </summary>
		/// <param name="name">The registered directive name to look up (without the <c>v-</c> prefix).</param>
		/// <returns>The directive definition registered under the given name.</returns>
		[Description("@#directive")]
		public extern VueDirectiveValue Directive(string name);

		/// <summary>
		/// Provide a value at the application level, injectable by any descendant component
		/// via <c>inject()</c>. Application-level provides are available to all components
		/// in the tree, regardless of nesting depth.
		/// </summary>
		/// <typeparam name="TValue">The type of the provided value.</typeparam>
		/// <param name="key">The injection key string used by <c>inject()</c> to retrieve the value.</param>
		/// <param name="value">The value to provide. Can be any type: primitives, objects, functions, etc.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(string key, TValue value);

		/// <summary>
		/// Provide a value at the application level using a strongly typed injection key.
		/// </summary>
		/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
		/// <param name="key">The typed injection key symbol used by <c>inject()</c>.</param>
		/// <param name="value">The value to provide.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(VueInjectionKey<TValue> key, TValue value);

		/// <summary>
		/// Run a callback with this app as the active injection context.
		/// </summary>
		/// <typeparam name="TResult">The callback return type.</typeparam>
		/// <param name="callback">The callback to execute in this app context.</param>
		/// <returns>The callback result.</returns>
		[Description("@#runWithContext")]
		public extern TResult RunWithContext<TResult>(Func<TResult> callback);
	}

	/// <summary>
	/// The current Vue runtime version.
	/// </summary>
	[Description("@#version")]
	public extern static string Version { get; }

	/// <summary>
	/// Creates a Vue application instance from a root component. The returned
	/// <see cref="VueApp"/> can be configured with plugins, global components, and
	/// directives before mounting.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent);

	/// <summary>
	/// Creates a Vue application instance with root props passed to the root component
	/// during mounting.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <param name="rootProps">Props to pass to the root component when it mounts.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps>(IVueComponent<TProps> rootComponent, TProps rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/>.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps>(IVueComponent<TProps> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props for a component
	/// that also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, TProps rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/> for a component that
	/// also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a Vue application instance in SSR (server-side rendering) mode. In SSR mode,
	/// Vue renders the component tree to HTML strings instead of DOM nodes.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp(IVueComponent rootComponent);

	/// <summary>
	/// Creates a Vue application instance in SSR mode with root props.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <param name="rootProps">Props to pass to the root component during server-side rendering.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp<TProps>(IVueComponent<TProps> rootComponent, TProps rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/>.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp<TProps>(IVueComponent<TProps> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props for a component
	/// that also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, TProps rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/> for a component that
	/// also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Defines a Vue component from an options object with no typed props. Use this overload
	/// for simple components that do not declare typed props or slots.
	/// </summary>
	/// <param name="options">The component options including setup/render, name, and registrations.</param>
	/// <returns>An <see cref="IVueComponent"/> that can be passed to <c>h()</c> or registered globally.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent DefineComponent(VueComponentDefinition options);

	/// <summary>
	/// Defines a Vue component with typed props. The generic parameter enforces C# props
	/// authoring; runtime prop and emit declarations are emitted only when supplied on
	/// the options object.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <param name="options">The typed component options including setup, name, and registrations.</param>
	/// <returns>An <see cref="IVueComponent{TProps}"/> that enforces typed props in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps> DefineComponent<TProps>(VueComponentOptions<TProps> options)
		where TProps : VueProps;

	/// <summary>
	/// Defines a Vue component with typed slots but no typed props. Use this overload for
	/// components that accept named slots but do not declare typed props.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	/// <param name="options">The typed component options including setup and registrations.</param>
	/// <returns>An <see cref="IVueSlotComponent{TSlots}"/> that enforces typed slots in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueSlotComponent<TSlots> DefineComponent<TSlots>(VueSlotComponentOptions<TSlots> options)
		where TSlots : VueSlots;

	/// <summary>
	/// Defines a Vue component with both typed props and typed slots. This is the most
	/// strongly-typed overload, enforcing both prop and slot types in <c>h()</c> calls.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	/// <param name="options">The fully typed component options including setup and registrations.</param>
	/// <returns>An <see cref="IVueComponent{TProps, TSlots}"/> that enforces both props and slots in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps, TSlots> DefineComponent<TProps, TSlots>(VueComponentOptions<TProps, TSlots> options)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Defines a Vue-powered custom element from an existing component options object.
	/// The returned constructor can be registered with the browser's
	/// <see cref="CustomElementRegistry"/>.
	/// </summary>
	/// <param name="options">The component options used to render the custom element.</param>
	/// <returns>A custom element constructor compatible with <c>customElements.define()</c>.</returns>
	[Description("@#defineCustomElement")]
	public extern static CustomElementConstructor DefineCustomElement(VueComponentDefinition options);

	/// <summary>
	/// Defines a Vue-powered custom element from component options plus
	/// custom-element-specific runtime options.
	/// </summary>
	/// <param name="options">The component options used to render the custom element.</param>
	/// <param name="customElementOptions">Custom-element-specific styles, app configuration, shadow root, and CSP options.</param>
	/// <returns>A custom element constructor compatible with <c>customElements.define()</c>.</returns>
	[Description("@#defineCustomElement")]
	public extern static CustomElementConstructor DefineCustomElement(VueComponentDefinition options, VueCustomElementOptions customElementOptions);

	/// <summary>
	/// Defines an async component from a loader callback.
	/// </summary>
	/// <param name="loader">A callback returning a promise that resolves to the component definition.</param>
	/// <returns>An async component reference that can be rendered or registered like a normal component.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static IVueComponent DefineAsyncComponent(VueAsyncComponentLoader loader);

	/// <summary>
	/// Defines an async component from object-form options.
	/// </summary>
	/// <param name="options">Async component loading, error, timing, and retry options.</param>
	/// <returns>An async component reference that can be rendered or registered like a normal component.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static IVueComponent DefineAsyncComponent(VueAsyncComponentOptions options);

	/// <summary>
	/// Defines a strongly typed async component from object-form options.
	/// </summary>
	/// <typeparam name="TComponent">The component contract produced by the loader.</typeparam>
	/// <param name="options">Typed async component loading, error, timing, and retry options.</param>
	/// <returns>A typed async component reference that preserves prop/slot contracts.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static TComponent DefineAsyncComponent<TComponent>(VueAsyncComponentOptions<TComponent> options)
		where TComponent : IVueComponent;

	/// <summary>
	/// Vue's built-in <c>Transition</c> component for animating a single element or
	/// component entering and leaving.
	/// </summary>
	[Description("@#Transition")]
	public extern static IVueComponent<VueTransitionProps> Transition { get; }

	/// <summary>
	/// Vue's built-in <c>TransitionGroup</c> component for animating list insertions,
	/// removals, and moves.
	/// </summary>
	[Description("@#TransitionGroup")]
	public extern static IVueComponent<VueTransitionGroupProps> TransitionGroup { get; }

	/// <summary>
	/// Vue's built-in <c>KeepAlive</c> component for caching inactive dynamic component
	/// instances.
	/// </summary>
	[Description("@#KeepAlive")]
	public extern static IVueComponent<VueKeepAliveProps> KeepAlive { get; }

	/// <summary>
	/// Vue's built-in <c>Teleport</c> component for rendering children into another DOM
	/// container.
	/// </summary>
	[Description("@#Teleport")]
	public extern static IVueComponent<VueTeleportProps> Teleport { get; }

	/// <summary>
	/// Vue's built-in <c>Suspense</c> component for coordinating async dependencies
	/// with default and fallback slots.
	/// </summary>
	[Description("@#Suspense")]
	public extern static IVueComponent<VueSuspenseProps, VueSuspenseSlots> Suspense { get; }

	/// <summary>
	/// Merges multiple props objects using Vue's VNode props merge semantics.
	/// </summary>
	/// <param name="props">The props objects to merge.</param>
	/// <returns>A merged props object suitable for <c>h(...)</c> and <c>cloneVNode(...)</c>.</returns>
	[Description("@#mergeProps")]
	public extern static VueProps MergeProps(params VueProps[] props);

	/// <summary>
	/// Clones an existing VNode.
	/// </summary>
	/// <param name="vnode">The VNode to clone.</param>
	/// <returns>A cloned VNode.</returns>
	[Description("@#cloneVNode")]
	public extern static IVNode CloneVNode(IVNode vnode);

	/// <summary>
	/// Clones an existing VNode and merges extra props into it.
	/// </summary>
	/// <param name="vnode">The VNode to clone.</param>
	/// <param name="extraProps">Additional props to merge into the clone.</param>
	/// <returns>A cloned VNode with merged props.</returns>
	[Description("@#cloneVNode")]
	public extern static IVNode CloneVNode(IVNode vnode, VueProps extraProps);

	/// <summary>
	/// Returns whether the supplied runtime value is a Vue VNode.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is a VNode.</returns>
	[Description("@#isVNode")]
	public extern static bool IsVNode<T>(T value);

	/// <summary>
	/// Resolves a component by name from the current component/app context.
	/// </summary>
	/// <param name="name">The registered component name.</param>
	/// <returns>The resolved component.</returns>
	[Description("@#resolveComponent")]
	public extern static IVueComponent ResolveComponent(string name);

	/// <summary>
	/// Resolves a directive by name from the current component/app context.
	/// </summary>
	/// <param name="name">The registered directive name.</param>
	/// <returns>The resolved directive, or <c>null</c> when unavailable.</returns>
	[Description("@#resolveDirective")]
	public extern static VueDirectiveValue? ResolveDirective(string name);

	/// <summary>
	/// Applies runtime directives to a VNode created by <see cref="H(string)"/> or a
	/// component render call.
	/// </summary>
	/// <param name="vnode">The VNode to decorate.</param>
	/// <param name="directives">Directive argument tuples matching Vue's runtime contract.</param>
	/// <returns>The same VNode with directive metadata attached.</returns>
	[Description("@#withDirectives")]
	public extern static IVNode WithDirectives(IVNode vnode, [PreserveParamsArray] params VueDirectiveArguments[] directives);

	/// <summary>
	/// Wraps a parameterless event handler with Vue event modifiers such as
	/// <c>stop</c>, <c>prevent</c>, or <c>self</c>.
	/// </summary>
	/// <param name="handler">The original event handler.</param>
	/// <param name="modifiers">Modifier names in Vue runtime form.</param>
	/// <returns>A wrapped event handler.</returns>
	[Description("@#withModifiers")]
	public extern static Action WithModifiers(Action handler, [PreserveParamsArray] params string[] modifiers);

	/// <summary>
	/// Wraps a typed event handler with Vue event modifiers.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <param name="handler">The original typed event handler.</param>
	/// <param name="modifiers">Modifier names in Vue runtime form.</param>
	/// <returns>A wrapped typed event handler.</returns>
	[Description("@#withModifiers")]
	public extern static VueEventHandler<TEvent> WithModifiers<TEvent>(VueEventHandler<TEvent> handler, [PreserveParamsArray] params string[] modifiers);

	/// <summary>
	/// Binds a this-aware data callback to Vue's Options API <c>data()</c> runtime shape.
	/// </summary>
	/// <typeparam name="TThis">Typed view of the component public instance.</typeparam>
	/// <param name="callback">The callback that receives runtime <c>this</c> first.</param>
	/// <returns>A standard Vue data callback.</returns>
	private const string BindThisInlineTemplate = "((__cb) => function(){ return __cb(this, ...arguments); })(__arg1)";

	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static VueDataCallback BindThis<TThis>(VueThisDataCallback<TThis> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with no explicit runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action BindThis<TThis>(VueThisAction<TThis> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with one runtime argument.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action<T1> BindThis<TThis, T1>(VueThisAction<TThis, T1> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with two runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action<T1, T2> BindThis<TThis, T1, T2>(VueThisAction<TThis, T1, T2> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with three runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action<T1, T2, T3> BindThis<TThis, T1, T2, T3>(VueThisAction<TThis, T1, T2, T3> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware watch cleanup callback.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static VueWatchCleanupCallback<TValue> BindThis<TThis, TValue>(VueThisWatchCleanupCallback<TThis, TValue> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with no explicit runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<TResult> BindThis<TThis, TResult>(VueThisFunc<TThis, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with one runtime argument.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<T1, TResult> BindThis<TThis, T1, TResult>(VueThisFunc<TThis, T1, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with two runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<T1, T2, TResult> BindThis<TThis, T1, T2, TResult>(VueThisFunc<TThis, T1, T2, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with three runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<T1, T2, T3, TResult> BindThis<TThis, T1, T2, T3, TResult>(VueThisFunc<TThis, T1, T2, T3, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Creates a VNode for an HTML element with no props or children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type);

	/// <summary>
	/// Creates a VNode for an HTML element with direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueChild child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and no children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props);

	/// <summary>
	/// Creates a VNode for an HTML element with props and direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, VueChild child);

	/// <summary>
	/// Creates a VNode for an untyped component with no props or children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component);

	/// <summary>
	/// Creates a VNode for an untyped component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, IVNode child);

	/// <summary>
	/// Creates a VNode for an untyped component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueChild child);

	/// <summary>
	/// Creates a VNode for an untyped component with named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueSlots slots);

	/// <summary>
	/// Creates a VNode for an untyped component with props and no slots/children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props);

	/// <summary>
	/// Creates a VNode for an untyped component with props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for an untyped component with props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, VueChild child);

	/// <summary>
	/// Creates a VNode for an untyped component with props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, VueSlots slots);

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with a typed Vue object.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, IVNode child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueChild child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueSlots slots)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props, IVNode child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props, VueChild child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props, IVNode child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props, VueChild child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props, VueSlots slots)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed Vue object props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props, VueSlots slots)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-slots component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, VueChild child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, TSlots slots)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueChild child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using typed props.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using a typed Vue object.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, VueChild child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props, VueChild child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed props and typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed Vue object props and typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a deep reactive proxy of an object. Vue recursively converts all nested
	/// properties into reactive getters/setters, so reads and writes at any depth are tracked.
	/// </summary>
	/// <typeparam name="T">The type of the object to make reactive (must be a reference type).</typeparam>
	/// <param name="value">The plain object to wrap in a reactive proxy.</param>
	/// <returns>A reactive proxy of the same type. All property accesses are tracked.</returns>
	[Description("@#reactive")]
	public extern static T Reactive<T>(T value) where T : class;

	/// <summary>
	/// Creates a readonly proxy of a reactive (or plain) object. Attempts to write to
	/// properties on the returned object will trigger a runtime warning and be ignored.
	/// </summary>
	/// <typeparam name="T">The type of the object to make readonly (must be a reference type).</typeparam>
	/// <param name="value">The object to wrap in a readonly proxy. Can be a reactive proxy or a plain object.</param>
	/// <returns>A readonly proxy of the same type. Reads are tracked; writes are blocked.</returns>
	[Description("@#readonly")]
	public extern static T Readonly<T>(T value) where T : class;

	/// <summary>
	/// Creates a shallow reactive proxy of an object.
	/// </summary>
	/// <typeparam name="T">The object type to wrap.</typeparam>
	/// <param name="value">The object to wrap.</param>
	/// <returns>A shallow reactive proxy of the same type.</returns>
	[Description("@#shallowReactive")]
	public extern static T ShallowReactive<T>(T value) where T : class;

	/// <summary>
	/// Creates a shallow readonly proxy of an object.
	/// </summary>
	/// <typeparam name="T">The object type to wrap.</typeparam>
	/// <param name="value">The object to wrap.</param>
	/// <returns>A shallow readonly proxy of the same type.</returns>
	[Description("@#shallowReadonly")]
	public extern static T ShallowReadonly<T>(T value) where T : class;

	/// <summary>
	/// Returns the raw object behind a Vue proxy.
	/// </summary>
	/// <typeparam name="T">The static object type.</typeparam>
	/// <param name="value">The proxy value.</param>
	/// <returns>The original raw object.</returns>
	[Description("@#toRaw")]
	public extern static T ToRaw<T>(T value) where T : class;

	/// <summary>
	/// Marks an object so Vue will never convert it to a proxy.
	/// </summary>
	/// <typeparam name="T">The object type to mark.</typeparam>
	/// <param name="value">The object to mark as raw.</param>
	/// <returns>The same object.</returns>
	[Description("@#markRaw")]
	public extern static T MarkRaw<T>(T value) where T : class;

	/// <summary>
	/// Returns whether a value is any Vue-created proxy.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is a Vue proxy.</returns>
	[Description("@#isProxy")]
	public extern static bool IsProxy<T>(T value);

	/// <summary>
	/// Returns whether a value is a reactive proxy.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is reactive.</returns>
	[Description("@#isReactive")]
	public extern static bool IsReactive<T>(T value);

	/// <summary>
	/// Returns whether a value is a readonly proxy.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is readonly.</returns>
	[Description("@#isReadonly")]
	public extern static bool IsReadonly<T>(T value);

	/// <summary>
	/// Creates a reactive ref wrapping a single value. Unlike <see cref="Reactive{T}"/>,
	/// <c>ref()</c> wraps the entire value, not its properties. Access the value via
	/// <see cref="IVueRef{T}.Value"/>.
	/// </summary>
	/// <typeparam name="T">The type of the value to wrap.</typeparam>
	/// <param name="value">The initial value of the ref.</param>
	/// <returns>A reactive ref whose <c>Value</c> property reads and writes the wrapped value.</returns>
	[Description("@#ref")]
	public extern static IVueRef<T> Ref<T>(T value);

	/// <summary>
	/// Creates a shallow reactive ref that only tracks replacements of <c>Value</c>, not
	/// mutations of the value itself. Use this for large objects where deep tracking is
	/// unnecessary or when the value is replaced wholesale.
	/// </summary>
	/// <typeparam name="T">The type of the value to wrap.</typeparam>
	/// <param name="value">The initial value of the shallow ref.</param>
	/// <returns>A shallow ref whose <c>Value</c> property only triggers on replacement, not on deep mutation.</returns>
	[Description("@#shallowRef")]
	public extern static IVueRef<T> ShallowRef<T>(T value);

	/// <summary>
	/// Forces effects depending on a shallow ref to re-run.
	/// </summary>
	/// <typeparam name="T">The type of the ref value.</typeparam>
	/// <param name="value">The ref to trigger.</param>
	[Description("@#triggerRef")]
	public extern static void TriggerRef<T>(IVueRef<T> value);

	/// <summary>
	/// Creates a custom ref whose dependency tracking and triggering are controlled by
	/// user-provided get/set handlers.
	/// </summary>
	/// <typeparam name="T">The custom ref value type.</typeparam>
	/// <param name="factory">Factory receiving Vue's track/trigger callbacks and returning get/set handlers.</param>
	/// <returns>A reactive ref controlled by the supplied factory.</returns>
	[Description("@#customRef")]
	public extern static IVueRef<T> CustomRef<T>(VueCustomRefFactory<T> factory);

	/// <summary>
	/// Returns whether a runtime value is a Vue ref.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is a ref.</returns>
	[Description("@#isRef")]
	public extern static bool IsRef<T>(T value);

	/// <summary>
	/// Returns a normal value unchanged.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The value to normalize.</param>
	/// <returns>The supplied value.</returns>
	[Description("@#unref")]
	public extern static T Unref<T>(T value);

	/// <summary>
	/// Unwraps a Vue ref to its current value.
	/// </summary>
	/// <typeparam name="T">The ref value type.</typeparam>
	/// <param name="value">The ref to unwrap.</param>
	/// <returns>The current ref value.</returns>
	[Description("@#unref")]
	public extern static T Unref<T>(IVueRef<T> value);

	/// <summary>
	/// Normalizes a plain value into a ref.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The value to wrap.</param>
	/// <returns>A ref for the supplied value.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<T> ToRef<T>(T value);

	/// <summary>
	/// Returns an existing ref unchanged.
	/// </summary>
	/// <typeparam name="T">The ref value type.</typeparam>
	/// <param name="value">The ref to normalize.</param>
	/// <returns>The supplied ref.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<T> ToRef<T>(IVueRef<T> value);

	/// <summary>
	/// Normalizes a getter into a readonly ref.
	/// </summary>
	/// <typeparam name="T">The getter result type.</typeparam>
	/// <param name="getter">The getter to wrap.</param>
	/// <returns>A readonly ref backed by the supplied getter.</returns>
	[Description("@#toRef")]
	public extern static VueReadonlyRef<T> ToRef<T>(Func<T> getter);

	/// <summary>
	/// Creates a linked ref for a property on a reactive object. The value type is
	/// explicit because C# cannot infer it from a string key.
	/// </summary>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <typeparam name="TValue">The linked property value type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <param name="key">The final runtime property name.</param>
	/// <returns>A ref linked to <paramref name="source"/>[<paramref name="key"/>].</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TSource, TValue>(TSource source, string key)
		where TSource : class;

	/// <summary>
	/// Creates a linked ref for a property on a reactive object, using a default value
	/// when the property is absent.
	/// </summary>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <typeparam name="TValue">The linked property value type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <param name="key">The final runtime property name.</param>
	/// <param name="defaultValue">The value Vue uses when the source property is absent.</param>
	/// <returns>A ref linked to <paramref name="source"/>[<paramref name="key"/>].</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TSource, TValue>(TSource source, string key, TValue defaultValue)
		where TSource : class;

	/// <summary>
	/// Creates a linked ref for a key in a dictionary-shaped Vue object.
	/// </summary>
	/// <typeparam name="TValue">The dictionary value type.</typeparam>
	/// <param name="source">The source dictionary-shaped object.</param>
	/// <param name="key">The final runtime property name.</param>
	/// <returns>A ref linked to the dictionary entry.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TValue>(VueDictionary<TValue> source, string key);

	/// <summary>
	/// Converts each enumerable property on a reactive object into a linked ref.
	/// </summary>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <returns>An indexer-based refs bag.</returns>
	[Description("@#toRefs")]
	public extern static VueRefs<TSource> ToRefs<TSource>(TSource source)
		where TSource : class;

	/// <summary>
	/// Converts a props-style object into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">The user-declared refs projection type.</typeparam>
	/// <param name="source">The source props-style object.</param>
	/// <returns>The typed refs projection returned by Vue.</returns>
	[Description("@#toRefs")]
	public extern static TRefs ToRefs<TRefs>(VueProps source)
		where TRefs : VueRefs;

	/// <summary>
	/// Converts an arbitrary reactive object into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">The user-declared refs projection type.</typeparam>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <returns>The typed refs projection returned by Vue.</returns>
	[Description("@#toRefs")]
	public extern static TRefs ToRefs<TRefs, TSource>(TSource source)
		where TRefs : VueRefs<TSource>
		where TSource : class;

	/// <summary>
	/// Creates a computed reactive value derived from a getter function. The getter is
	/// evaluated lazily and cached; it is re-evaluated only when its reactive dependencies
	/// change. The returned ref is readonly.
	/// </summary>
	/// <typeparam name="T">The type of the computed value.</typeparam>
	/// <param name="getter">A function that computes the derived value. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>A readonly ref whose <c>Value</c> is the latest computed result.</returns>
	[Description("@#computed")]
	public extern static VueReadonlyRef<T> Computed<T>(Func<T> getter);

	/// <summary>
	/// Creates a writable computed ref from explicit get/set delegates.
	/// </summary>
	/// <typeparam name="T">The computed value type.</typeparam>
	/// <param name="options">Plain Vue computed options containing <c>get</c> and <c>set</c>.</param>
	/// <returns>A writable computed ref.</returns>
	[Description("@#computed")]
	public extern static IVueRef<T> Computed<T>(VueWritableComputedOptions<T> options);

	/// <summary>
	/// Watches a reactive source and calls the callback when it changes. The callback
	/// receives both the new value and the previous value. Returns a handle that can be
	/// called to stop the watcher.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch. Called on each evaluation cycle.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c> whenever the source's return value changes.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback);

	/// <summary>
	/// Watches a getter source with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a getter source and exposes Vue's cleanup registration function to the callback.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// Watches a getter source with cleanup-aware callback and explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a reactive object source directly. Vue implicitly treats this as a deep
	/// watcher over the object's reactive graph.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, Action<TSource, TSource> callback)
		where TSource : class;

	/// <summary>
	/// Watches a reactive object source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, Action<TSource, TSource> callback, VueWatchOptions options)
		where TSource : class;

	/// <summary>
	/// Watches a reactive object source directly and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, VueWatchCleanupCallback<TSource> callback)
		where TSource : class;

	/// <summary>
	/// Watches a reactive object source directly with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, VueWatchCleanupCallback<TSource> callback, VueWatchOptions options)
		where TSource : class;

	/// <summary>
	/// Watches a ref source directly.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, Action<T, T> callback);

	/// <summary>
	/// Watches a ref source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a ref source directly and exposes Vue's cleanup registration function to the callback.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// Watches a ref source directly with cleanup-aware callback and explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a readonly ref source directly.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, Action<T, T> callback);

	/// <summary>
	/// Watches a readonly ref source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a readonly ref source directly and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// Watches a readonly ref source directly with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed writable refs. Vue invokes the callback with arrays of
	/// current and previous values in the same order as the source array.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed writable refs with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed writable refs and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed writable refs with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed readonly refs, such as computed refs.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed readonly refs with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed readonly refs and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed readonly refs with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed getter sources.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed getter sources with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed getter sources and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed getter sources with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Runs a side-effect function immediately and re-runs it whenever its reactive
	/// dependencies change. Unlike <see cref="Watch{T}"/>, this does not receive old/new
	/// values — it simply re-executes the entire effect.
	/// </summary>
	/// <param name="effect">The side-effect function to run. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect);

	/// <summary>
	/// Runs a watcher effect with explicit effect options.
	/// </summary>
	/// <param name="effect">The side-effect function to run.</param>
	/// <param name="options">Effect options such as <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect, VueWatchEffectOptions options);

	/// <summary>
	/// Runs a watcher effect and exposes Vue's cleanup registration function.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// Runs a cleanup-aware watcher effect with explicit effect options.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <param name="options">Effect options such as <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(VueWatchEffectCallback effect, VueWatchEffectOptions options);

	/// <summary>
	/// Runs a watcher effect after component updates flush.
	/// </summary>
	/// <param name="effect">The side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchPostEffect")]
	public extern static VueWatchHandle WatchPostEffect(Action effect);

	/// <summary>
	/// Runs a cleanup-aware watcher effect after component updates flush.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchPostEffect")]
	public extern static VueWatchHandle WatchPostEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// Runs a watcher effect synchronously when dependencies change.
	/// </summary>
	/// <param name="effect">The side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchSyncEffect")]
	public extern static VueWatchHandle WatchSyncEffect(Action effect);

	/// <summary>
	/// Runs a cleanup-aware watcher effect synchronously when dependencies change.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchSyncEffect")]
	public extern static VueWatchHandle WatchSyncEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// Registers a cleanup callback for the currently active watcher.
	/// </summary>
	/// <param name="cleanup">Cleanup work to execute before the watcher re-runs or stops.</param>
	[Description("@#onWatcherCleanup")]
	public extern static void OnWatcherCleanup(Action cleanup);

	/// <summary>
	/// Registers a cleanup callback for the currently active watcher.
	/// </summary>
	/// <param name="cleanup">Cleanup work to execute before the watcher re-runs or stops.</param>
	/// <param name="failSilently">When <c>true</c>, Vue suppresses the missing-watcher warning.</param>
	[Description("@#onWatcherCleanup")]
	public extern static void OnWatcherCleanup(Action cleanup, bool failSilently);

	/// <summary>
	/// Normalizes a plain value, ref, or getter into its current value. This overload
	/// returns plain values unchanged.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The value to normalize.</param>
	/// <returns>The supplied value.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(T value);

	/// <summary>
	/// Normalizes a ref into its current value.
	/// </summary>
	/// <typeparam name="T">The ref value type.</typeparam>
	/// <param name="value">The ref to unwrap.</param>
	/// <returns>The current ref value.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(IVueRef<T> value);

	/// <summary>
	/// Normalizes a getter into its returned value.
	/// </summary>
	/// <typeparam name="T">The getter return type.</typeparam>
	/// <param name="getter">The getter to invoke through Vue normalization semantics.</param>
	/// <returns>The getter result.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(Func<T> getter);

	/// <summary>
	/// Waits for the next DOM update cycle to complete. Use this after modifying reactive
	/// state to ensure the DOM has been updated before asserting on the rendered output.
	/// </summary>
	/// <returns>A <see cref="PromiseResult"/> that resolves after the DOM update flush.</returns>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick();

	/// <summary>
	/// Waits for the next DOM update cycle and runs a callback after the flush.
	/// </summary>
	/// <param name="callback">The callback Vue invokes after the next DOM update flush.</param>
	/// <returns>A <see cref="PromiseResult"/> that resolves after the callback has run.</returns>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick(Action callback);

	/// <summary>
	/// Returns the fallthrough attributes from the current setup context.
	/// </summary>
	/// <returns>The current component's fallthrough attribute bag.</returns>
	[Description("@#useAttrs")]
	public extern static VueAttributeBag UseAttrs();

	/// <summary>
	/// Returns the fallthrough attributes as a user-declared typed projection.
	/// This does not convert the runtime object; it only gives C# IntelliSense for
	/// known attribute keys.
	/// </summary>
	/// <typeparam name="TAttrs">The typed attribute projection record.</typeparam>
	/// <returns>The current component's fallthrough attributes projected as <typeparamref name="TAttrs"/>.</returns>
	[Description("@#useAttrs")]
	public extern static TAttrs UseAttrs<TAttrs>()
		where TAttrs : VueProps;

	/// <summary>
	/// Returns the slots object from the current setup context.
	/// </summary>
	/// <returns>The current component's slot bag.</returns>
	[Description("@#useSlots")]
	public extern static VueSlotBag UseSlots();

	/// <summary>
	/// Returns the slots object as a user-declared typed slot projection.
	/// </summary>
	/// <typeparam name="TSlots">The typed slots projection record.</typeparam>
	/// <returns>The current component's slots projected as <typeparamref name="TSlots"/>.</returns>
	[Description("@#useSlots")]
	public extern static TSlots UseSlots<TSlots>()
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a readonly template ref linked to a template <c>ref</c> key.
	/// </summary>
	/// <typeparam name="TElement">The element or component instance type expected for the ref.</typeparam>
	/// <param name="key">The template ref key.</param>
	/// <returns>A readonly ref whose value is populated after mount and reset on unmount.</returns>
	[Description("@#useTemplateRef")]
	public extern static VueReadonlyRef<TElement?> UseTemplateRef<TElement>(string key)
		where TElement : class;

	/// <summary>
	/// Generates a stable per-application unique id that is safe for SSR hydration.
	/// </summary>
	/// <returns>A unique id string for the current app instance.</returns>
	[Description("@#useId")]
	public extern static string UseId();

	/// <summary>
	/// Creates a writable model ref backed by a declared prop and its corresponding
	/// <c>update:*</c> event. The component must still declare the prop and emit entry.
	/// </summary>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="props">The setup props object supplied by Vue.</param>
	/// <param name="key">The final runtime prop key, such as <c>"modelValue"</c>.</param>
	/// <returns>A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static IVueRef<TValue> UseModel<TValue>(VueProps props, string key);

	/// <summary>
	/// Creates a writable model ref with read/write transforms.
	/// </summary>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="props">The setup props object supplied by Vue.</param>
	/// <param name="key">The final runtime prop key, such as <c>"modelValue"</c>.</param>
	/// <param name="options">Read/write transforms applied by Vue's model helper.</param>
	/// <returns>A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static IVueRef<TValue> UseModel<TValue>(VueProps props, string key, VueModelOptions<TValue> options);

	/// <summary>
	/// Returns the current Vue custom element host while running inside a custom
	/// element setup context.
	/// </summary>
	/// <returns>The current custom element host, or <c>null</c> when unavailable.</returns>
	[Description("@#useHost")]
	public extern static HTMLElement? UseHost();

	/// <summary>
	/// Returns the current Vue custom element host projected to a typed host element.
	/// This is a typed projection only and does not create a new runtime wrapper.
	/// </summary>
	/// <typeparam name="THost">The expected custom element host type.</typeparam>
	/// <returns>The current custom element host, or <c>null</c> when unavailable.</returns>
	[Description("@#useHost")]
	public extern static THost? UseHost<THost>()
		where THost : HTMLElement;

	/// <summary>
	/// Returns the current Vue custom element shadow root while running inside a
	/// custom element setup context.
	/// </summary>
	/// <returns>The current custom element shadow root, or <c>null</c> when unavailable.</returns>
	[Description("@#useShadowRoot")]
	public extern static ShadowRoot? UseShadowRoot();

	/// <summary>
	/// Provides a value from the current component setup context to descendant components.
	/// </summary>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <param name="value">The value to provide.</param>
	[Description("@#provide")]
	public extern static void Provide<TValue>(string key, TValue value);

	/// <summary>
	/// Provides a value from the current component setup context using a strongly typed
	/// injection key.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <param name="value">The value to provide.</param>
	[Description("@#provide")]
	public extern static void Provide<TValue>(VueInjectionKey<TValue> key, TValue value);

	/// <summary>
	/// Injects a value from the nearest ancestor provider using a string key.
	/// </summary>
	/// <typeparam name="TValue">The expected value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <returns>The injected value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#inject")]
	public extern static TValue? Inject<TValue>(string key);

	/// <summary>
	/// Injects a value from the nearest ancestor provider using a strongly typed
	/// injection key.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <returns>The injected value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#inject")]
	public extern static TValue? Inject<TValue>(VueInjectionKey<TValue> key);

	/// <summary>
	/// Injects a value using a string key, returning a default value when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The expected value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <param name="defaultValue">The default value used when no provider exists.</param>
	/// <returns>The injected value or the supplied default value.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(string key, TValue defaultValue);

	/// <summary>
	/// Injects a value using a strongly typed injection key, returning a default value
	/// when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <param name="defaultValue">The default value used when no provider exists.</param>
	/// <returns>The injected value or the supplied default value.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(VueInjectionKey<TValue> key, TValue defaultValue);

	/// <summary>
	/// Injects a value using a string key, evaluating a default factory when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The expected value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <param name="defaultFactory">Factory used when no provider exists.</param>
	/// <param name="treatDefaultAsFactory">Pass <c>true</c> so Vue treats the second argument as a factory.</param>
	/// <returns>The injected value or the factory result.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(string key, Func<TValue> defaultFactory, bool treatDefaultAsFactory = true);

	/// <summary>
	/// Injects a value using a strongly typed injection key, evaluating a default factory
	/// when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <param name="defaultFactory">Factory used when no provider exists.</param>
	/// <param name="treatDefaultAsFactory">Pass <c>true</c> so Vue treats the second argument as a factory.</param>
	/// <returns>The injected value or the factory result.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(VueInjectionKey<TValue> key, Func<TValue> defaultFactory, bool treatDefaultAsFactory = true);

	/// <summary>
	/// Returns whether the current call stack has an active injection context.
	/// </summary>
	/// <returns><c>true</c> when <c>inject()</c> can be used without a warning.</returns>
	[Description("@#hasInjectionContext")]
	public extern static bool HasInjectionContext();

	/// <summary>
	/// Creates a new effect scope. Effects created inside the scope can be stopped together.
	/// </summary>
	/// <param name="detached">When <c>true</c>, create a detached scope not linked to the current active scope.</param>
	/// <returns>A new effect scope.</returns>
	[Description("@#effectScope")]
	public extern static VueEffectScope EffectScope(bool detached = false);

	/// <summary>
	/// Returns the currently active effect scope, if one exists.
	/// </summary>
	/// <returns>The current effect scope when available; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#getCurrentScope")]
	public extern static VueEffectScope? GetCurrentScope();

	/// <summary>
	/// Registers a cleanup callback on the current active effect scope.
	/// </summary>
	/// <param name="callback">The cleanup callback to run when the current scope is stopped.</param>
	[Description("@#onScopeDispose")]
	public extern static void OnScopeDispose(Action callback);

	/// <summary>
	/// Registers a cleanup callback on the current active effect scope.
	/// </summary>
	/// <param name="callback">The cleanup callback to run when the current scope is stopped.</param>
	/// <param name="failSilently">When <c>true</c>, Vue suppresses the missing-scope warning.</param>
	[Description("@#onScopeDispose")]
	public extern static void OnScopeDispose(Action callback, bool failSilently);

	/// <summary>
	/// Registers a callback to run after the component's initial mount into the DOM.
	/// The callback runs once; use <see cref="OnUpdated"/> for subsequent re-renders.
	/// </summary>
	/// <param name="callback">The function to execute after the component is mounted. Has access to the live DOM.</param>
	[Description("@#onMounted")]
	public extern static void OnMounted(Action callback);

	/// <summary>
	/// Registers a callback to run right before the component is mounted.
	/// </summary>
	/// <param name="callback">The function to execute before mount.</param>
	[Description("@#onBeforeMount")]
	public extern static void OnBeforeMount(Action callback);

	/// <summary>
	/// Registers a callback to run after the component is unmounted (removed from the DOM).
	/// Use this for cleanup: stopping timers, removing event listeners, disconnecting observables, etc.
	/// </summary>
	/// <param name="callback">The cleanup function to execute after the component is unmounted.</param>
	[Description("@#onUnmounted")]
	public extern static void OnUnmounted(Action callback);

	/// <summary>
	/// Registers a callback to observe errors captured from descendant component
	/// renders, event handlers, lifecycle hooks, setup functions, and watcher callbacks.
	/// </summary>
	/// <param name="callback">A handler that observes the captured error and lets Vue continue propagation.</param>
	[Description("@#onErrorCaptured")]
	public extern static void OnErrorCaptured(VueErrorCapturedHandler callback);

	/// <summary>
	/// Registers a callback to observe captured descendant errors and optionally stop
	/// propagation by returning <c>false</c>.
	/// </summary>
	/// <param name="callback">A callback returning <c>false</c> when propagation should stop.</param>
	[Description("@#onErrorCaptured")]
	public extern static void OnErrorCaptured(VueErrorCapturedCallback callback);

	/// <summary>
	/// Registers a callback to run right before the component is unmounted.
	/// </summary>
	/// <param name="callback">The function to execute before unmount.</param>
	[Description("@#onBeforeUnmount")]
	public extern static void OnBeforeUnmount(Action callback);

	/// <summary>
	/// Registers a callback to run after a reactive state change causes the component's
	/// DOM to be updated. Fires after every re-render, not just the first.
	/// </summary>
	/// <param name="callback">The function to execute after each DOM update caused by a reactive state change.</param>
	[Description("@#onUpdated")]
	public extern static void OnUpdated(Action callback);

	/// <summary>
	/// Registers a development-mode callback that runs when a reactive dependency is
	/// tracked during component render.
	/// </summary>
	/// <param name="callback">The debugger callback receiving Vue's render tracking event.</param>
	[Description("@#onRenderTracked")]
	public extern static void OnRenderTracked(VueDebuggerCallback callback);

	/// <summary>
	/// Registers a development-mode callback that runs when a dependency triggers a
	/// component render update.
	/// </summary>
	/// <param name="callback">The debugger callback receiving Vue's render trigger event.</param>
	[Description("@#onRenderTriggered")]
	public extern static void OnRenderTriggered(VueDebuggerCallback callback);

	/// <summary>
	/// Registers a callback to run right before a reactive update patches the DOM.
	/// </summary>
	/// <param name="callback">The function to execute before each update.</param>
	[Description("@#onBeforeUpdate")]
	public extern static void OnBeforeUpdate(Action callback);

	/// <summary>
	/// Registers a callback to run when a cached component is inserted into the DOM.
	/// </summary>
	/// <param name="callback">The function to execute when activated.</param>
	[Description("@#onActivated")]
	public extern static void OnActivated(Action callback);

	/// <summary>
	/// Registers a callback to run when a cached component is removed from the DOM.
	/// </summary>
	/// <param name="callback">The function to execute when deactivated.</param>
	[Description("@#onDeactivated")]
	public extern static void OnDeactivated(Action callback);

	/// <summary>
	/// Registers an async dependency to await during server-side rendering.
	/// </summary>
	/// <param name="callback">A callback returning a JavaScript promise.</param>
	[Description("@#onServerPrefetch")]
	public extern static void OnServerPrefetch(VueServerPrefetchPromiseCallback callback);

	/// <summary>
	/// Registers an async dependency to await during server-side rendering.
	/// </summary>
	/// <param name="callback">A compiler-lowered async callback returning a bridge promise result.</param>
	[Description("@#onServerPrefetch")]
	public extern static void OnServerPrefetch(VueServerPrefetchCallback callback);
}
