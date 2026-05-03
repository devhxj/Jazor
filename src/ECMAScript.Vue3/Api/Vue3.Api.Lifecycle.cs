using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
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
