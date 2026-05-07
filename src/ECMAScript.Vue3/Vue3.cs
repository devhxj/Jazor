using System;
using System.ComponentModel;
using ECMAScript.Contract;

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
public delegate Vue3.VueProps? VueDirectiveSSRPropsCallback(Vue3.VueDirectiveBinding binding, Vue3.IVNode vnode);

/// <summary>
/// Callback signature for a typed Vue directive SSR hook that returns props to merge into the rendered element.
/// </summary>
/// <typeparam name="TValue">The typed contract of the directive's current binding value.</typeparam>
/// <param name="binding">The current typed directive binding payload.</param>
/// <param name="vnode">The current VNode associated with the element.</param>
/// <returns>Additional props that should be merged into the SSR-rendered element.</returns>
public delegate Vue3.VueProps? VueDirectiveSSRPropsCallback<TValue>(Vue3.VueDirectiveBinding<TValue> binding, Vue3.IVNode vnode);

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
	where TComponent : ECMAScript.Vue3.IVueComponent;

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
public static partial class Vue3
{
}
