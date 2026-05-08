using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// 注册在组件首次挂载到 DOM 后运行的回调。该回调只运行一次；后续重渲染请使用 <see cref="OnUpdated"/>。
	/// Registers a callback to run after the component's initial mount into the DOM.
	/// The callback runs once; use <see cref="OnUpdated"/> for subsequent re-renders.
	/// </summary>
	/// <param name="callback">组件挂载后执行的函数，可访问已渲染的 DOM。The function to execute after the component is mounted. Has access to the live DOM.</param>
	[Description("@#onMounted")]
	public extern static void OnMounted(Action callback);

	/// <summary>
	/// 注册在组件挂载之前运行的回调。
	/// Registers a callback to run right before the component is mounted.
	/// </summary>
	/// <param name="callback">挂载前执行的函数。The function to execute before mount.</param>
	[Description("@#onBeforeMount")]
	public extern static void OnBeforeMount(Action callback);

	/// <summary>
	/// 注册在组件卸载（从 DOM 中移除）后运行的回调。用于清理：停止定时器、移除事件监听器、断开可观察对象等。
	/// Registers a callback to run after the component is unmounted (removed from the DOM).
	/// Use this for cleanup: stopping timers, removing event listeners, disconnecting observables, etc.
	/// </summary>
	/// <param name="callback">组件卸载后执行的清理函数。The cleanup function to execute after the component is unmounted.</param>
	[Description("@#onUnmounted")]
	public extern static void OnUnmounted(Action callback);

	/// <summary>
	/// 注册回调以观察从后代组件的渲染、事件处理器、生命周期钩子、setup 函数和侦听器回调中捕获的错误。
	/// Registers a callback to observe errors captured from descendant component
	/// renders, event handlers, lifecycle hooks, setup functions, and watcher callbacks.
	/// </summary>
	/// <param name="callback">观察捕获错误的处理器，允许 Vue 继续传播。A handler that observes the captured error and lets Vue continue propagation.</param>
	[Description("@#onErrorCaptured")]
	public extern static void OnErrorCaptured(VueErrorCapturedHandler callback);

	/// <summary>
	/// 注册回调以观察后代错误，可通过返回 <c>false</c> 阻止错误继续传播。
	/// Registers a callback to observe captured descendant errors and optionally stop
	/// propagation by returning <c>false</c>.
	/// </summary>
	/// <param name="callback">返回 <c>false</c> 时阻止传播的回调。A callback returning <c>false</c> when propagation should stop.</param>
	[Description("@#onErrorCaptured")]
	public extern static void OnErrorCaptured(VueErrorCapturedCallback callback);

	/// <summary>
	/// 注册在组件卸载之前运行的回调。
	/// Registers a callback to run right before the component is unmounted.
	/// </summary>
	/// <param name="callback">卸载前执行的函数。The function to execute before unmount.</param>
	[Description("@#onBeforeUnmount")]
	public extern static void OnBeforeUnmount(Action callback);

	/// <summary>
	/// 注册在响应式状态变更导致组件 DOM 更新后运行的回调。每次重渲染后都会触发，不仅限于首次。
	/// Registers a callback to run after a reactive state change causes the component's
	/// DOM to be updated. Fires after every re-render, not just the first.
	/// </summary>
	/// <param name="callback">响应式状态变更导致 DOM 更新后执行的函数。The function to execute after each DOM update caused by a reactive state change.</param>
	[Description("@#onUpdated")]
	public extern static void OnUpdated(Action callback);

	/// <summary>
	/// 注册开发模式回调，在组件渲染期间跟踪响应式依赖时运行。
	/// Registers a development-mode callback that runs when a reactive dependency is
	/// tracked during component render.
	/// </summary>
	/// <param name="callback">接收 Vue 渲染追踪事件的调试回调。The debugger callback receiving Vue's render tracking event.</param>
	[Description("@#onRenderTracked")]
	public extern static void OnRenderTracked(VueDebuggerCallback callback);

	/// <summary>
	/// 注册开发模式回调，在依赖触发组件渲染更新时运行。
	/// Registers a development-mode callback that runs when a dependency triggers a
	/// component render update.
	/// </summary>
	/// <param name="callback">接收 Vue 渲染触发事件的调试回调。The debugger callback receiving Vue's render trigger event.</param>
	[Description("@#onRenderTriggered")]
	public extern static void OnRenderTriggered(VueDebuggerCallback callback);

	/// <summary>
	/// 注册在响应式更新修补 DOM 之前运行的回调。
	/// Registers a callback to run right before a reactive update patches the DOM.
	/// </summary>
	/// <param name="callback">每次更新前执行的函数。The function to execute before each update.</param>
	[Description("@#onBeforeUpdate")]
	public extern static void OnBeforeUpdate(Action callback);

	/// <summary>
	/// 注册在被缓存的组件插入 DOM 时运行的回调。
	/// Registers a callback to run when a cached component is inserted into the DOM.
	/// </summary>
	/// <param name="callback">组件激活时执行的函数。The function to execute when activated.</param>
	[Description("@#onActivated")]
	public extern static void OnActivated(Action callback);

	/// <summary>
	/// 注册在被缓存的组件从 DOM 中移除时运行的回调。
	/// Registers a callback to run when a cached component is removed from the DOM.
	/// </summary>
	/// <param name="callback">组件停用时执行的函数。The function to execute when deactivated.</param>
	[Description("@#onDeactivated")]
	public extern static void OnDeactivated(Action callback);

	/// <summary>
	/// 注册在服务端渲染期间等待的异步依赖。
	/// Registers an async dependency to await during server-side rendering.
	/// </summary>
	/// <param name="callback">返回 JavaScript promise 的回调。A callback returning a JavaScript promise.</param>
	[Description("@#onServerPrefetch")]
	public extern static void OnServerPrefetch(VueServerPrefetchPromiseCallback callback);

	/// <summary>
	/// 注册在服务端渲染期间等待的异步依赖。
	/// Registers an async dependency to await during server-side rendering.
	/// </summary>
	/// <param name="callback">编译器降低的异步回调，返回桥接 promise 结果。A compiler-lowered async callback returning a bridge promise result.</param>
	[Description("@#onServerPrefetch")]
	public extern static void OnServerPrefetch(VueServerPrefetchCallback callback);
}
