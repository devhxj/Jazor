using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// 由 Pinia 订阅和动作侦听器注册 API 返回的回调。调用它会分离先前注册的侦听器。
/// Callback returned by Pinia subscription and action-listener registration APIs.
/// Calling it detaches the previously registered listener.
/// </summary>
public delegate void PiniaDetachCallback();

/// <summary>
/// 传递给 <c>$patch((state) =&gt; ...)</c> 的回调。
/// Callback passed to <c>$patch((state) =&gt; ...)</c>.
/// </summary>
/// <typeparam name="TState">强类型的 store 状态投影。The typed store-state projection.</typeparam>
/// <param name="state">当前可变的 store 状态。The current mutable store state.</param>
public delegate void PiniaStatePatchCallback<TState>(TState state)
	where TState : Pinia.PiniaStateTree;

/// <summary>
/// 传递给 <c>defineStore(id, setup, ...)</c> 的 setup-store 回调。
/// Setup-store callback passed to <c>defineStore(id, setup, ...)</c>.
/// </summary>
/// <typeparam name="TStore">setup 回调返回的强类型 store 投影。The typed store projection returned by the setup callback.</typeparam>
/// <param name="helpers">Pinia 提供的 setup-store 辅助对象。The setup-store helpers object supplied by Pinia.</param>
/// <returns>回调返回的 setup-store 接口。The setup-store surface returned by the callback.</returns>
public delegate TStore PiniaSetupStoreFactory<TStore>(Pinia.SetupStoreHelpers helpers)
	where TStore : class;

/// <summary>
/// 在 SSR/客户端注水期间，使用当前活跃 store 状态和传入的初始状态调用的选项式 store 注水钩子。
/// Option-store hydration hook invoked with the live store state and the incoming
/// initial state during SSR/client hydration.
/// </summary>
/// <typeparam name="TState">强类型的 store 状态投影。The typed store-state projection.</typeparam>
/// <param name="storeState">当前活跃的 store 状态对象。The current live store state object.</param>
/// <param name="initialState">正在注水到 store 中的初始序列化状态。The initial serialized state being hydrated into the store.</param>
public delegate void PiniaHydrateCallback<TState>(TState storeState, TState initialState)
	where TState : Pinia.PiniaStateTree;

/// <summary>
/// <c>$subscribe()</c> 使用的回调。
/// Callback used by <c>$subscribe()</c>.
/// </summary>
/// <typeparam name="TState">强类型的 store 状态投影。The typed store-state projection.</typeparam>
/// <param name="mutation">描述触发订阅回调的变更的元数据。Metadata describing the mutation that triggered the subscription callback.</param>
/// <param name="state">变更完成后的当前 store 状态。The current store state after the mutation completed.</param>
public delegate void PiniaSubscriptionCallback<TState>(Pinia.SubscriptionMutation<TState> mutation, TState state)
	where TState : Pinia.PiniaStateTree;

/// <summary>
/// <c>$onAction()</c> 使用的无类型动作侦听器回调。
/// Untyped action-listener callback used by <c>$onAction()</c>.
/// </summary>
/// <param name="context">描述当前动作调用的动作侦听器上下文。The action listener context describing the current action invocation.</param>
public delegate void PiniaStoreActionListener(Pinia.StoreActionListenerContext context);

/// <summary>
/// <c>$onAction()</c> 使用的强类型动作侦听器回调。
/// Typed action-listener callback used by <c>$onAction()</c>.
/// </summary>
/// <typeparam name="TStore">侦听器上下文提供的强类型 store 投影。The typed store projection supplied by the listener context.</typeparam>
/// <param name="context">描述当前动作调用的强类型动作侦听器上下文。The typed action listener context describing the current action invocation.</param>
public delegate void PiniaStoreActionListener<TStore>(Pinia.StoreActionListenerContext<TStore> context)
	where TStore : class;

/// <summary>
/// 对象形式 <c>mapState()</c> / <c>mapGetters()</c> 使用的自定义选择器回调。
/// Custom selector callback used by object-form <c>mapState()</c> / <c>mapGetters()</c>.
/// </summary>
/// <typeparam name="TStore">store 定义提供的强类型 store 投影。The typed store projection supplied by the store definition.</typeparam>
/// <param name="store">强类型的 store 实例。The typed store instance.</param>
/// <returns>应作为计算属性条目后备的映射值。The mapped value that should back the computed entry.</returns>
public delegate Pinia.PiniaValue? PiniaMapStateSelector<TStore>(TStore store)
	where TStore : class;

/// <summary>
/// 通过 <c>pinia.use(...)</c> 注册的 Pinia 插件回调。
/// Pinia plugin callback registered through <c>pinia.use(...)</c>.
/// </summary>
/// <param name="context">包含应用、pinia 实例、store 和定义选项的插件上下文。The plugin context containing the app, pinia instance, store, and defining options.</param>
/// <returns>可选的附加属性，将合并到 store 实例中。Optional additional properties to merge into the store instance.</returns>
public delegate Vue3.VueProps? PiniaPlugin(Pinia.PiniaPluginContext context);

/// <summary>
/// 上下文将当前 store 投影到更强用户声明类型的强类型 Pinia 插件回调。
/// Typed Pinia plugin callback whose context projects the current store to a
/// stronger user-declared type.
/// </summary>
/// <typeparam name="TStore">插件上下文提供的强类型 store 投影。The typed store projection supplied by the plugin context.</typeparam>
/// <param name="context">当前 store 的强类型插件上下文。The typed plugin context for the current store.</param>
/// <returns>可选的附加属性，将合并到 store 实例中。Optional additional properties to merge into the store instance.</returns>
public delegate Vue3.VueProps? PiniaPlugin<TStore>(Pinia.PiniaPluginContext<TStore> context)
	where TStore : class;

/// <summary>
/// 上下文同时将当前 store 和插件可见的 store 定义选项投影到更强用户声明类型的强类型 Pinia 插件回调。
/// Typed Pinia plugin callback whose context projects both the current store and
/// the plugin-visible store-definition options to stronger user-declared types.
/// </summary>
/// <typeparam name="TStore">插件上下文提供的强类型 store 投影。The typed store projection supplied by the plugin context.</typeparam>
/// <typeparam name="TOptions">强类型的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
/// <param name="context">当前 store 的强类型插件上下文。The typed plugin context for the current store.</param>
/// <returns>可选的附加属性，将合并到 store 实例中。Optional additional properties to merge into the store instance.</returns>
public delegate Vue3.VueProps? PiniaPlugin<TStore, TOptions>(Pinia.PiniaPluginContext<TStore, TOptions> context)
	where TStore : class
	where TOptions : Pinia.DefineStoreOptionsInPlugin;

/// <summary>
/// 完全强类型的 Pinia 插件回调，其合并的扩展对象也被投影到更强的用户声明类型。
/// Fully typed Pinia plugin callback whose merged extension object is also
/// projected to a stronger user-declared type.
/// </summary>
/// <typeparam name="TStore">插件上下文提供的强类型 store 投影。The typed store projection supplied by the plugin context.</typeparam>
/// <typeparam name="TOptions">强类型的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
/// <typeparam name="TExtension">插件返回的强类型扩展对象。The typed extension object returned by the plugin.</typeparam>
/// <param name="context">当前 store 的强类型插件上下文。The typed plugin context for the current store.</param>
/// <returns>可选的附加属性，将合并到 store 实例中。Optional additional properties to merge into the store instance.</returns>
public delegate TExtension? PiniaPlugin<TStore, TOptions, TExtension>(Pinia.PiniaPluginContext<TStore, TOptions> context)
	where TStore : class
	where TOptions : Pinia.DefineStoreOptionsInPlugin
	where TExtension : Vue3.VueProps;

/// <summary>
/// 完全强类型的 Pinia 插件回调，其上下文还将当前 store 投影到由先前插件产生的显式插件自定义属性视图。
/// Fully typed Pinia plugin callback whose context also projects the current store
/// to an explicit plugin-custom-properties view produced by earlier plugins.
/// </summary>
/// <typeparam name="TStore">插件上下文提供的基础强类型 store 投影。The base typed store projection supplied by the plugin context.</typeparam>
/// <typeparam name="TOptions">强类型的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
/// <typeparam name="TCustomProperties">已在当前 store 上可见的插件添加的自定义 store 属性。The plugin-added custom store properties already visible on the current store.</typeparam>
/// <typeparam name="TExtension">插件返回的强类型扩展对象。The typed extension object returned by the plugin.</typeparam>
/// <param name="context">当前 store 的强类型插件上下文。The typed plugin context for the current store.</param>
/// <returns>可选的附加属性，将合并到 store 实例中。Optional additional properties to merge into the store instance.</returns>
public delegate TExtension? PiniaPlugin<TStore, TOptions, TCustomProperties, TExtension>(Pinia.PiniaPluginContext<TStore, TOptions, TCustomProperties> context)
	where TStore : class
	where TOptions : Pinia.DefineStoreOptionsInPlugin
	where TCustomProperties : Vue3.VueProps
	where TExtension : Vue3.VueProps;

/// <summary>
/// 完全强类型的 Pinia 插件回调，其上下文将当前 store 投影到显式自定义属性视图，并将其 <c>$state</c> 投影到由先前插件产生的显式插件自定义状态视图。
/// Fully typed Pinia plugin callback whose context projects the current store to an
/// explicit custom-properties view and its <c>$state</c> to an explicit
/// plugin-custom-state view produced by earlier plugins.
/// </summary>
/// <typeparam name="TStore">插件上下文提供的基础强类型 store 投影。The base typed store projection supplied by the plugin context.</typeparam>
/// <typeparam name="TOptions">强类型的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
/// <typeparam name="TCustomProperties">已在当前 store 上可见的插件添加的自定义 store 属性。The plugin-added custom store properties already visible on the current store.</typeparam>
/// <typeparam name="TCustomState">已在 <c>store.$state</c> 上可见的插件添加的自定义状态。The plugin-added custom state already visible on <c>store.$state</c>.</typeparam>
/// <typeparam name="TExtension">插件返回的强类型扩展对象。The typed extension object returned by the plugin.</typeparam>
/// <param name="context">当前 store 的强类型插件上下文。The typed plugin context for the current store.</param>
/// <returns>可选的附加属性，将合并到 store 实例中。Optional additional properties to merge into the store instance.</returns>
public delegate TExtension? PiniaPlugin<TStore, TOptions, TCustomProperties, TCustomState, TExtension>(Pinia.PiniaPluginContext<TStore, TOptions, TCustomProperties, TCustomState> context)
	where TStore : class
	where TOptions : Pinia.DefineStoreOptionsInPlugin
	where TCustomProperties : Vue3.VueProps
	where TCustomState : Pinia.PiniaStateTree
	where TExtension : Vue3.VueProps;

/// <summary>
/// <c>acceptHMRUpdate()</c> 返回的热模块替换接受回调。
/// Hot-module-replacement accept callback returned by <c>acceptHMRUpdate()</c>.
/// </summary>
/// <param name="newModule">宿主 HMR 运行时提供的新热模块对象。The new hot module object supplied by the host HMR runtime.</param>
public delegate void PiniaHotUpdateHandler(IObject newModule);

[ECMAScript("pinia")]
[Description("@#")]
public static partial class Pinia
{
}
