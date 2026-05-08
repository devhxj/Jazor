using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// <c>watch()</c> 和 <c>watchEffect()</c> 返回的句柄。Vue 将其暴露为
/// 可调用的停止函数，带有 <c>pause()</c>、<c>resume()</c> 和 <c>stop()</c> 成员；
/// C# 使用显式方法以保持控制表面的可发现性。
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
	/// 停止侦听器并清理其响应式依赖追踪。
	/// Stop the watcher and clean up its reactive dependency tracking.
	/// </summary>
	[Description("@#stop")]
	public extern void Stop();

	/// <summary>
	/// 暂停侦听器但不释放它。
	/// Temporarily pause the watcher without disposing it.
	/// </summary>
	[Description("@#pause")]
	public extern void Pause();

	/// <summary>
	/// 恢复之前暂停的侦听器。
	/// Resume a watcher that was previously paused.
	/// </summary>
	[Description("@#resume")]
	public extern void Resume();
}

/// <summary>
/// 为当前侦听器运行注册清理回调。
/// Registers a cleanup callback for the current watcher run.
/// </summary>
/// <param name="cleanup">在侦听器重新运行或停止之前执行的清理工作。Cleanup work to execute before the watcher re-runs or stops.</param>
public delegate void VueWatchCleanupRegistration(Action cleanup);

/// <summary>
/// 接收 Vue 清理注册函数的 effect 回调。
/// Effect callback that receives Vue's cleanup registration function.
/// </summary>
/// <param name="onCleanup">用于为当前 effect 运行注册清理的函数。Function used to register cleanup for the current effect run.</param>
public delegate void VueWatchEffectCallback(VueWatchCleanupRegistration onCleanup);

/// <summary>
/// 接收新值、旧值和 Vue 清理注册函数的侦听回调。
/// Watch callback that receives new value, previous value, and Vue's cleanup registration function.
/// </summary>
/// <typeparam name="T">被侦听的值类型。The watched value type.</typeparam>
/// <param name="value">当前值。The current value.</param>
/// <param name="oldValue">之前的值。The previous value.</param>
/// <param name="onCleanup">用于为当前侦听器运行注册清理的函数。Function used to register cleanup for the current watcher run.</param>
public delegate void VueWatchCleanupCallback<T>(T value, T oldValue, VueWatchCleanupRegistration onCleanup);

/// <summary>
/// 多个相同类型值源的侦听回调。Vue 提供包含新旧值的并行数组。
/// Watch callback for multiple sources of the same value type.
/// Vue supplies parallel arrays containing the new and previous values.
/// </summary>
/// <typeparam name="T">每个被侦听源产生的值类型。The value type produced by each watched source.</typeparam>
/// <param name="values">所有源的当前值，按源顺序排列。Current values from all sources, in source order.</param>
/// <param name="oldValues">所有源的之前的值，按源顺序排列。Previous values from all sources, in source order.</param>
public delegate void VueWatchSourcesCallback<T>(T[] values, T[] oldValues);

/// <summary>
/// 带清理功能的多个相同类型值源的侦听回调。
/// Cleanup-aware watch callback for multiple sources of the same value type.
/// </summary>
/// <typeparam name="T">每个被侦听源产生的值类型。The value type produced by each watched source.</typeparam>
/// <param name="values">所有源的当前值，按源顺序排列。Current values from all sources, in source order.</param>
/// <param name="oldValues">所有源的之前的值，按源顺序排列。Previous values from all sources, in source order.</param>
/// <param name="onCleanup">用于为当前侦听器运行注册清理的函数。Function used to register cleanup for the current watcher run.</param>
public delegate void VueWatchSourcesCleanupCallback<T>(T[] values, T[] oldValues, VueWatchCleanupRegistration onCleanup);

/// <summary>
/// 接收类型化事件负载的 Vue 事件处理器回调签名。
/// Callback signature for Vue event handlers that receive a typed event payload.
/// </summary>
/// <typeparam name="T">事件负载值的类型。The type of the event payload value.</typeparam>
/// <param name="value">由源组件触发的事件负载。The event payload emitted by the source component.</param>
public delegate void VueEventHandler<T>(T value);

/// <summary>
/// Vue prop 默认值的工厂回调。
/// Factory callback for a Vue prop default value.
/// </summary>
/// <typeparam name="TValue">prop 值类型。The prop value type.</typeparam>
/// <returns>当 prop 缺失时 Vue 应使用的默认值。The default value Vue should use when the prop is absent.</returns>
public delegate TValue VuePropDefaultFactory<TValue>();

/// <summary>
/// 需要访问原始 props 对象的 Vue prop 默认值工厂回调。
/// Factory callback for a Vue prop default value that needs access to the raw props object.
/// </summary>
/// <typeparam name="TValue">prop 值类型。The prop value type.</typeparam>
/// <param name="rawProps">传递给组件实例的原始 prop 值。The raw prop values supplied to the component instance.</param>
/// <returns>当 prop 缺失时 Vue 应使用的默认值。The default value Vue should use when the prop is absent.</returns>
public delegate TValue VuePropRawPropsDefaultFactory<TValue>(Vue3.VueProps rawProps);

/// <summary>
/// Vue prop 声明的验证回调。
/// Validator callback for a Vue prop declaration.
/// </summary>
/// <typeparam name="TValue">prop 值类型。The prop value type.</typeparam>
/// <param name="value">正在验证的 prop 值。The prop value being validated.</param>
/// <returns>值被接受时返回 <c>true</c>。<c>true</c> when the value is accepted.</returns>
public delegate bool VuePropValidator<TValue>(TValue value);

/// <summary>
/// 需要访问所有原始 props 的 Vue prop 声明验证回调。
/// Validator callback for a Vue prop declaration that needs access to all raw props.
/// </summary>
/// <typeparam name="TValue">prop 值类型。The prop value type.</typeparam>
/// <param name="value">正在验证的 prop 值。The prop value being validated.</param>
/// <param name="rawProps">传递给组件实例的原始 prop 值。The raw prop values supplied to the component instance.</param>
/// <returns>值被接受时返回 <c>true</c>。<c>true</c> when the value is accepted.</returns>
public delegate bool VuePropRawPropsValidator<TValue>(TValue value, Vue3.VueProps rawProps);

/// <summary>
/// 无负载 Vue emit 声明的验证回调。
/// Validator callback for a no-payload Vue emit declaration.
/// </summary>
/// <returns>emit 负载被接受时返回 <c>true</c>。<c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator();

/// <summary>
/// 带一个负载值的 Vue emit 声明的验证回调。
/// Validator callback for a Vue emit declaration with one payload value.
/// </summary>
/// <typeparam name="T0">第一个 emit 负载类型。The first emitted payload type.</typeparam>
/// <param name="arg0">第一个 emit 负载值。The first emitted payload value.</param>
/// <returns>emit 负载被接受时返回 <c>true</c>。<c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0>(T0 arg0);

/// <summary>
/// 带两个负载值的 Vue emit 声明的验证回调。
/// Validator callback for a Vue emit declaration with two payload values.
/// </summary>
/// <typeparam name="T0">第一个 emit 负载类型。The first emitted payload type.</typeparam>
/// <typeparam name="T1">第二个 emit 负载类型。The second emitted payload type.</typeparam>
/// <param name="arg0">第一个 emit 负载值。The first emitted payload value.</param>
/// <param name="arg1">第二个 emit 负载值。The second emitted payload value.</param>
/// <returns>emit 负载被接受时返回 <c>true</c>。<c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0, T1>(T0 arg0, T1 arg1);

/// <summary>
/// 带三个负载值的 Vue emit 声明的验证回调。
/// Validator callback for a Vue emit declaration with three payload values.
/// </summary>
/// <typeparam name="T0">第一个 emit 负载类型。The first emitted payload type.</typeparam>
/// <typeparam name="T1">第二个 emit 负载类型。The second emitted payload type.</typeparam>
/// <typeparam name="T2">第三个 emit 负载类型。The third emitted payload type.</typeparam>
/// <param name="arg0">第一个 emit 负载值。The first emitted payload value.</param>
/// <param name="arg1">第二个 emit 负载值。The second emitted payload value.</param>
/// <param name="arg2">第三个 emit 负载值。The third emitted payload value.</param>
/// <returns>emit 负载被接受时返回 <c>true</c>。<c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);

/// <summary>
/// 带四个负载值的 Vue emit 声明的验证回调。
/// Validator callback for a Vue emit declaration with four payload values.
/// </summary>
/// <typeparam name="T0">第一个 emit 负载类型。The first emitted payload type.</typeparam>
/// <typeparam name="T1">第二个 emit 负载类型。The second emitted payload type.</typeparam>
/// <typeparam name="T2">第三个 emit 负载类型。The third emitted payload type.</typeparam>
/// <typeparam name="T3">第四个 emit 负载类型。The fourth emitted payload type.</typeparam>
/// <param name="arg0">第一个 emit 负载值。The first emitted payload value.</param>
/// <param name="arg1">第二个 emit 负载值。The second emitted payload value.</param>
/// <param name="arg2">第三个 emit 负载值。The third emitted payload value.</param>
/// <param name="arg3">第四个 emit 负载值。The fourth emitted payload value.</param>
/// <returns>emit 负载被接受时返回 <c>true</c>。<c>true</c> when the emit payload is accepted.</returns>
public delegate bool VueEmitValidator<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);

/// <summary>
/// 返回渲染树（VNode）的回调。用作 <c>setup()</c> 的返回类型以提供组件的渲染函数。
/// Callback that returns a render tree (VNode). Used as the return type of <c>setup()</c>
/// to provide the component's render function.
/// </summary>
/// <returns>表示组件渲染输出的根 VNode。A root VNode representing the rendered component output.</returns>
public delegate Vue3.IVNode VueRenderCallback();

/// <summary>
/// 从无作用域数据的插槽返回 VNode 的回调。
/// Callback that returns a VNode from a slot with no scoped data.
/// </summary>
/// <returns>插槽产生的 VNode，如果插槽为空则返回 <c>null</c>。A VNode produced by the slot, or <c>null</c> if the slot is empty.</returns>
public delegate Vue3.IVNode VueSlotCallback();

/// <summary>
/// 从接收插槽 props 的作用域插槽返回 VNode 的回调。
/// Callback that returns a VNode from a scoped slot that receives slot props.
/// </summary>
/// <typeparam name="TScope">传递给插槽的作用域数据类型。The type of the scoped data passed into the slot.</typeparam>
/// <param name="scope">父组件提供给插槽的作用域数据对象。The scoped data object provided by the parent component to the slot.</param>
/// <returns>插槽产生的 VNode，如果插槽为空则返回 <c>null</c>。A VNode produced by the slot, or <c>null</c> if the slot is empty.</returns>
public delegate Vue3.IVNode VueSlotCallback<TScope>(TScope scope);

/// <summary>
/// 无类型 props 的组件 <c>setup()</c> 函数的回调签名。
/// setup 函数在组件挂载之前运行，返回一个渲染回调。
/// Callback signature for a component <c>setup()</c> function with no typed props.
/// The setup function runs before the component is mounted and returns a render callback.
/// </summary>
/// <returns>框架调用以产生组件 VNode 树的 <see cref="VueRenderCallback"/>。A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueSetupCallback();

/// <summary>
/// 接收类型化 props 的组件 <c>setup()</c> 函数的回调签名。
/// Callback signature for a component <c>setup()</c> function that receives typed props.
/// </summary>
/// <typeparam name="TProps">props 记录类型，继承自 <see cref="Vue3.VueProps"/>。The props record type, inheriting from <see cref="Vue3.VueProps"/>.</typeparam>
/// <param name="props">父组件传递的响应式 props 对象。The reactive props object passed by the parent component.</param>
/// <param name="context">提供 <c>attrs</c>、<c>slots</c>、<c>emit</c> 和 <c>expose</c> 的 setup 上下文。The setup context providing <c>attrs</c>, <c>slots</c>, <c>emit</c>, and <c>expose</c>.</param>
/// <returns>框架调用以产生组件 VNode 树的 <see cref="VueRenderCallback"/>。A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSetupCallback<TProps>(TProps props, Vue3.VueSetupContext context)
	where TProps : Vue3.VueProps;

/// <summary>
/// 接收类型化插槽但无类型 props 的组件 <c>setup()</c> 函数的回调签名。
/// Callback signature for a component <c>setup()</c> function that receives typed slots but no typed props.
/// </summary>
/// <typeparam name="TSlots">插槽记录类型，继承自 <see cref="Vue3.VueSlots"/>。The slots record type, inheriting from <see cref="Vue3.VueSlots"/>.</typeparam>
/// <param name="context">除标准上下文成员外还提供类型化 <c>slots</c> 的类型化 setup 上下文。The typed setup context providing typed <c>slots</c> in addition to the standard context members.</param>
/// <returns>框架调用以产生组件 VNode 树的 <see cref="VueRenderCallback"/>。A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSlotSetupCallback<TSlots>(Vue3.VueSetupContext<TSlots> context)
	where TSlots : Vue3.VueSlots;

/// <summary>
/// 同时接收类型化 props 和类型化插槽的组件 <c>setup()</c> 函数的回调签名。
/// Callback signature for a component <c>setup()</c> function that receives both typed props and typed slots.
/// </summary>
/// <typeparam name="TProps">props 记录类型，继承自 <see cref="Vue3.VueProps"/>。The props record type, inheriting from <see cref="Vue3.VueProps"/>.</typeparam>
/// <typeparam name="TSlots">插槽记录类型，继承自 <see cref="Vue3.VueSlots"/>。The slots record type, inheriting from <see cref="Vue3.VueSlots"/>.</typeparam>
/// <param name="props">父组件传递的响应式 props 对象。The reactive props object passed by the parent component.</param>
/// <param name="context">除标准上下文成员外还提供类型化 <c>slots</c> 的类型化 setup 上下文。The typed setup context providing typed <c>slots</c> in addition to the standard context members.</param>
/// <returns>框架调用以产生组件 VNode 树的 <see cref="VueRenderCallback"/>。A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSetupCallback<TProps, TSlots>(TProps props, Vue3.VueSetupContext<TSlots> context)
	where TProps : Vue3.VueProps
	where TSlots : Vue3.VueSlots;

/// <summary>
/// Options API <c>data()</c> 的回调签名。返回的记录被降级为
/// Vue 为每个组件实例创建响应式的普通对象。
/// Callback signature for Options API <c>data()</c>. The returned record is lowered to
/// the plain object that Vue makes reactive for each component instance.
/// </summary>
/// <returns>一个组件实例的新状态对象。A fresh state object for one component instance.</returns>
public delegate Vue3.VueProps VueDataCallback();

/// <summary>
/// 函数形式的 Vue 插件安装入口回调签名。
/// Callback signature for a function-form Vue plugin installation entrypoint.
/// </summary>
/// <param name="app">当前正在配置的 Vue 应用实例。The Vue application instance currently being configured.</param>
public delegate void VuePluginInstallCallback(Vue3.VueApp app);

/// <summary>
/// 接收强类型安装选项的函数形式或对象形式的 Vue 插件安装入口回调签名。
/// Callback signature for a function-form or object-form Vue plugin installation entrypoint
/// that receives strongly typed install options.
/// </summary>
/// <typeparam name="TOptions">类型化的插件选项契约。The typed plugin options contract.</typeparam>
/// <param name="app">当前正在配置的 Vue 应用实例。The Vue application instance currently being configured.</param>
/// <param name="options">传递给 <c>app.use(plugin, options)</c> 的强类型选项。The strongly typed options passed to <c>app.use(plugin, options)</c>.</param>
public delegate void VuePluginInstallCallback<TOptions>(Vue3.VueApp app, TOptions options)
	where TOptions : Vue3.VuePluginOptions;

/// <summary>
/// <c>defineCustomElement()</c> 用来配置为 Vue 自定义元素创建的应用实例的回调。
/// Callback used by <c>defineCustomElement()</c> to configure the app instance
/// created for a Vue custom element.
/// </summary>
/// <param name="app">自定义元素内部创建的 Vue 应用实例。The custom element's internally created Vue application instance.</param>
public delegate void VueCustomElementConfigureAppCallback(Vue3.VueApp app);

/// <summary>
/// 不需要之前值的 Vue 指令生命周期钩子回调签名。
/// Callback signature for a Vue directive lifecycle hook that does not need a previous value.
/// </summary>
/// <param name="element">当前受指令控制的目标 DOM 元素。The target DOM element currently controlled by the directive.</param>
/// <param name="binding">当前的指令绑定负载。The current directive binding payload.</param>
/// <param name="vnode">与元素关联的当前 VNode。The current VNode associated with the element.</param>
public delegate void VueDirectiveHook(Element element, Vue3.VueDirectiveBinding binding, Vue3.IVNode vnode);

/// <summary>
/// 类型化的 Vue 指令生命周期钩子（不需要之前值）回调签名。
/// Callback signature for a typed Vue directive lifecycle hook that does not need a previous value.
/// </summary>
/// <typeparam name="TValue">指令当前绑定值的类型化契约。The typed contract of the directive's current binding value.</typeparam>
/// <param name="element">当前受指令控制的目标 DOM 元素。The target DOM element currently controlled by the directive.</param>
/// <param name="binding">当前的类型化指令绑定负载。The current typed directive binding payload.</param>
/// <param name="vnode">与元素关联的当前 VNode。The current VNode associated with the element.</param>
public delegate void VueDirectiveHook<TValue>(Element element, Vue3.VueDirectiveBinding<TValue> binding, Vue3.IVNode vnode);

/// <summary>
/// Vue 指令函数简写形式的回调签名。Vue 将其视为 <c>mounted</c> 和 <c>updated</c> 的相同回调。
/// Callback signature for a Vue directive function shorthand. Vue treats this as the
/// same callback for both <c>mounted</c> and <c>updated</c>.
/// </summary>
/// <param name="element">当前受指令控制的目标 DOM 元素。The target DOM element currently controlled by the directive.</param>
/// <param name="binding">当前的指令绑定负载。The current directive binding payload.</param>
public delegate void VueDirectiveFunction(Element element, Vue3.VueDirectiveBinding binding);

/// <summary>
/// 类型化的 Vue 指令函数简写形式的回调签名。Vue 将其视为 <c>mounted</c> 和 <c>updated</c> 的相同回调。
/// Callback signature for a typed Vue directive function shorthand. Vue treats this as the
/// same callback for both <c>mounted</c> and <c>updated</c>.
/// </summary>
/// <typeparam name="TValue">指令当前绑定值的类型化契约。The typed contract of the directive's current binding value.</typeparam>
/// <param name="element">当前受指令控制的目标 DOM 元素。The target DOM element currently controlled by the directive.</param>
/// <param name="binding">当前的类型化指令绑定负载。The current typed directive binding payload.</param>
public delegate void VueDirectiveFunction<TValue>(Element element, Vue3.VueDirectiveBinding<TValue> binding);

/// <summary>
/// 还需要之前绑定值的 Vue 指令更新钩子回调签名。
/// Callback signature for a Vue directive update hook that also needs the previous binding value.
/// </summary>
/// <param name="element">当前受指令控制的目标 DOM 元素。The target DOM element currently controlled by the directive.</param>
/// <param name="binding">当前的指令更新绑定负载。The current directive update binding payload.</param>
/// <param name="vnode">与元素关联的当前 VNode。The current VNode associated with the element.</param>
/// <param name="previousVNode">与同一元素关联的之前的 VNode。The previous VNode associated with the same element.</param>
public delegate void VueDirectiveUpdateHook(Element element, Vue3.VueDirectiveUpdateBinding binding, Vue3.IVNode vnode, Vue3.IVNode previousVNode);

/// <summary>
/// 还需要之前绑定值的类型化 Vue 指令更新钩子回调签名。
/// Callback signature for a typed Vue directive update hook that also needs the previous binding value.
/// </summary>
/// <typeparam name="TValue">指令当前和之前绑定值的类型化契约。The typed contract of the directive's current and previous binding values.</typeparam>
/// <param name="element">当前受指令控制的目标 DOM 元素。The target DOM element currently controlled by the directive.</param>
/// <param name="binding">当前的类型化指令更新绑定负载。The current typed directive update binding payload.</param>
/// <param name="vnode">与元素关联的当前 VNode。The current VNode associated with the element.</param>
/// <param name="previousVNode">与同一元素关联的之前的 VNode。The previous VNode associated with the same element.</param>
public delegate void VueDirectiveUpdateHook<TValue>(Element element, Vue3.VueDirectiveUpdateBinding<TValue> binding, Vue3.IVNode vnode, Vue3.IVNode previousVNode);

/// <summary>
/// 返回要合并到渲染元素中的 props 的 Vue 指令 SSR 钩子回调签名。
/// Callback signature for a Vue directive SSR hook that returns props to merge into the rendered element.
/// </summary>
/// <param name="binding">当前的指令绑定负载。The current directive binding payload.</param>
/// <param name="vnode">与元素关联的当前 VNode。The current VNode associated with the element.</param>
/// <returns>应合并到 SSR 渲染元素中的额外 props。Additional props that should be merged into the SSR-rendered element.</returns>
public delegate Vue3.VueProps? VueDirectiveSSRPropsCallback(Vue3.VueDirectiveBinding binding, Vue3.IVNode vnode);

/// <summary>
/// 返回要合并到渲染元素中的 props 的类型化 Vue 指令 SSR 钩子回调签名。
/// Callback signature for a typed Vue directive SSR hook that returns props to merge into the rendered element.
/// </summary>
/// <typeparam name="TValue">指令当前绑定值的类型化契约。The typed contract of the directive's current binding value.</typeparam>
/// <param name="binding">当前的类型化指令绑定负载。The current typed directive binding payload.</param>
/// <param name="vnode">与元素关联的当前 VNode。The current VNode associated with the element.</param>
/// <returns>应合并到 SSR 渲染元素中的额外 props。Additional props that should be merged into the SSR-rendered element.</returns>
public delegate Vue3.VueProps? VueDirectiveSSRPropsCallback<TValue>(Vue3.VueDirectiveBinding<TValue> binding, Vue3.IVNode vnode);

/// <summary>
/// Vue 异步组件的加载回调。返回一个解析为组件定义的 JavaScript promise。
/// Loader callback for a Vue async component. It returns a JavaScript promise that
/// resolves to the component definition.
/// </summary>
/// <returns>解析为异步组件定义的 promise。A promise resolving to the async component definition.</returns>
public delegate IPromise<Vue3.IVueComponent> VueAsyncComponentLoader();

/// <summary>
/// 强类型 Vue 异步组件的加载回调。
/// Loader callback for a strongly typed Vue async component.
/// </summary>
/// <typeparam name="TComponent">加载器产生的组件契约。The component contract produced by the loader.</typeparam>
/// <returns>解析为类型化异步组件定义的 promise。A promise resolving to the typed async component definition.</returns>
public delegate IPromise<TComponent> VueAsyncComponentLoader<TComponent>()
	where TComponent : ECMAScript.Vue3.IVueComponent;

/// <summary>
/// Vue 异步组件错误处理中用于重试或使加载失败的回调。
/// Callback used by Vue async component error handling to retry or fail the load.
/// </summary>
public delegate void VueAsyncComponentRetryCallback();

/// <summary>
/// 异步组件加载的错误回调。Vue 提供抛出的错误、重试回调、失败回调和当前尝试次数。
/// Error callback for async component loading. Vue supplies the thrown error, retry
/// callback, fail callback, and current attempt count.
/// </summary>
/// <param name="error">加载组件时引发的 JavaScript 错误。The JavaScript error raised while loading the component.</param>
/// <param name="retry">重试异步组件加载器。Retry the async component loader.</param>
/// <param name="fail">使异步组件加载失败。Fail the async component load.</param>
/// <param name="attempts">到目前为止的加载尝试次数。The number of load attempts so far.</param>
public delegate void VueAsyncComponentErrorCallback(Error error, VueAsyncComponentRetryCallback retry, VueAsyncComponentRetryCallback fail, Number attempts);

/// <summary>
/// <c>customRef()</c> 的工厂回调。Vue 提供 <c>track</c> 和
/// <c>trigger</c> 回调，工厂返回自定义 ref 的 get/set 处理器。
/// Factory callback for <c>customRef()</c>. Vue supplies <c>track</c> and
/// <c>trigger</c> callbacks, and the factory returns the custom ref get/set handlers.
/// </summary>
/// <typeparam name="T">自定义 ref 的值类型。The custom ref value type.</typeparam>
/// <param name="track">当自定义 getter 应追踪依赖时调用。Call when the custom getter should track a dependency.</param>
/// <param name="trigger">当自定义 setter 应触发依赖更新时调用。Call when the custom setter should trigger dependents.</param>
/// <returns>自定义 ref 使用的 get/set 处理器。The get/set handlers used by the custom ref.</returns>
public delegate Vue3.VueCustomRefHandlers<T> VueCustomRefFactory<T>(Action track, Action trigger);

/// <summary>
/// Vue 侦听器调试钩子（如 <c>onTrack</c> 和 <c>onTrigger</c>）使用的回调。
/// Callback used by Vue watcher debug hooks such as <c>onTrack</c> and
/// <c>onTrigger</c>.
/// </summary>
/// <param name="event">Vue 响应式系统发出的调试器事件。The debugger event emitted by Vue's reactivity system.</param>
public delegate void VueDebuggerCallback(Vue3.VueDebuggerEvent @event);

/// <summary>
/// 可通过返回 <c>false</c> 阻止 Vue 错误传播的错误捕获回调。
/// Error-captured callback that can stop Vue error propagation by returning
/// <c>false</c>.
/// </summary>
/// <param name="error">Vue 捕获的 unknown 类型的错误值。The unknown-like error value captured by Vue.</param>
/// <param name="instance">错误来源的组件公开实例（如可用）。The component public instance where the error originated, when available.</param>
/// <param name="info">Vue 的错误上下文字符串。Vue's error context string.</param>
/// <returns>返回 <c>false</c> 阻止传播；返回 <c>true</c> 继续传播。<c>false</c> to stop propagation; <c>true</c> to continue.</returns>
public delegate bool VueErrorCapturedCallback(Vue3.VueValue? error, Vue3.VueComponentPublicInstance? instance, string info);

/// <summary>
/// 仅观察捕获的错误并始终让 Vue 继续传播的错误捕获处理器。
/// Error-captured handler for cases that only observe captured errors and always let
/// Vue continue propagation.
/// </summary>
/// <param name="error">Vue 捕获的 unknown 类型的错误值。The unknown-like error value captured by Vue.</param>
/// <param name="instance">错误来源的组件公开实例（如可用）。The component public instance where the error originated, when available.</param>
/// <param name="info">Vue 的错误上下文字符串。Vue's error context string.</param>
public delegate void VueErrorCapturedHandler(Vue3.VueValue? error, Vue3.VueComponentPublicInstance? instance, string info);

/// <summary>
/// 返回 JavaScript promise 的服务端预取回调。
/// Server-prefetch callback that returns a JavaScript promise.
/// </summary>
/// <returns>Vue 在服务端渲染期间应等待的 promise。The promise Vue should await during server rendering.</returns>
public delegate IPromise VueServerPrefetchPromiseCallback();

/// <summary>
/// 编译器降级的异步回调所使用的服务端预取回调形状。
/// Server-prefetch callback shape used by compiler-lowered async callbacks.
/// </summary>
/// <returns>Vue 在服务端渲染期间应等待的桥接 promise 结果。The bridge promise result Vue should await during server rendering.</returns>
public delegate PromiseResult VueServerPrefetchCallback();

/// <summary>
/// Options API 创作用的 this 绑定数据回调。第一个参数在运行时接收组件公开实例（<c>this</c>）。
/// this-bound data callback for Options API authoring. The first parameter receives
/// the component public instance (<c>this</c>) at runtime.
/// </summary>
/// <typeparam name="TThis">组件公开实例的类型化视图。Typed view of the component public instance.</typeparam>
/// <param name="self">运行时的组件公开实例。The runtime component public instance.</param>
/// <returns>当前组件实例的数据对象。The data object for the current component instance.</returns>
public delegate Vue3.VueProps VueThisDataCallback<TThis>(TThis self)
	where TThis : class;

/// <summary>
/// 无显式运行时参数的 this 绑定 action 回调。
/// this-bound action callback with no explicit runtime arguments.
/// </summary>
/// <typeparam name="TThis">组件公开实例的类型化视图。Typed view of the component public instance.</typeparam>
/// <param name="self">运行时的组件公开实例。The runtime component public instance.</param>
public delegate void VueThisAction<TThis>(TThis self)
	where TThis : class;

/// <summary>
/// 带一个运行时参数的 this 绑定 action 回调。
/// this-bound action callback with one runtime argument.
/// </summary>
public delegate void VueThisAction<TThis, T1>(TThis self, T1 arg1)
	where TThis : class;

/// <summary>
/// 带两个运行时参数的 this 绑定 action 回调。
/// this-bound action callback with two runtime arguments.
/// </summary>
public delegate void VueThisAction<TThis, T1, T2>(TThis self, T1 arg1, T2 arg2)
	where TThis : class;

/// <summary>
/// 带三个运行时参数的 this 绑定 action 回调。
/// this-bound action callback with three runtime arguments.
/// </summary>
public delegate void VueThisAction<TThis, T1, T2, T3>(TThis self, T1 arg1, T2 arg2, T3 arg3)
	where TThis : class;

/// <summary>
/// 无显式运行时参数的 this 绑定 function 回调。
/// this-bound function callback with no explicit runtime arguments.
/// </summary>
public delegate TResult VueThisFunc<TThis, TResult>(TThis self)
	where TThis : class;

/// <summary>
/// 带一个运行时参数的 this 绑定 function 回调。
/// this-bound function callback with one runtime argument.
/// </summary>
public delegate TResult VueThisFunc<TThis, T1, TResult>(TThis self, T1 arg1)
	where TThis : class;

/// <summary>
/// 带两个运行时参数的 this 绑定 function 回调。
/// this-bound function callback with two runtime arguments.
/// </summary>
public delegate TResult VueThisFunc<TThis, T1, T2, TResult>(TThis self, T1 arg1, T2 arg2)
	where TThis : class;

/// <summary>
/// 带三个运行时参数的 this 绑定 function 回调。
/// this-bound function callback with three runtime arguments.
/// </summary>
public delegate TResult VueThisFunc<TThis, T1, T2, T3, TResult>(TThis self, T1 arg1, T2 arg2, T3 arg3)
	where TThis : class;

/// <summary>
/// 包含 Vue 清理注册参数的 this 绑定侦听回调。
/// this-bound watch callback that includes Vue's cleanup registration argument.
/// </summary>
public delegate void VueThisWatchCleanupCallback<TThis, TValue>(TThis self, TValue value, TValue oldValue, VueWatchCleanupRegistration onCleanup)
	where TThis : class;

[ECMAScript("npm:vue@3")]
[Description("@#")]
public static partial class Vue3
{
}
