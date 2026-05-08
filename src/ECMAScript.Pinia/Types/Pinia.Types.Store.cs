using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	/// <summary>
	/// 由 <c>createPinia()</c> 创建的 Pinia 根实例。同一对象既是 Pinia 运行时根，也是 Vue 插件安装目标。
	/// Pinia root instance created by <c>createPinia()</c>. The same object is both a
	/// Pinia runtime root and a Vue plugin install target.
	/// </summary>
	public abstract record PiniaInstance : Vue3.VuePlugin
	{
		/// <summary>
		/// Pinia 的根状态树，以 store id 为键。
		/// Pinia's root state tree, keyed by store id.
		/// </summary>
		[Description("@#state")]
		public extern Vue3.IVueRef<Vue3.VueDictionary<PiniaStateTree>> State { get; }

		/// <summary>
		/// 在此根实例上注册一个 Pinia 插件。
		/// Registers a Pinia plugin on this root instance.
		/// </summary>
		/// <param name="plugin">要注册的插件回调。The plugin callback to register.</param>
		/// <returns>同一个 Pinia 实例。The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use(PiniaPlugin plugin);

		/// <summary>
		/// 在此根实例上注册一个带类型的 Pinia 插件。
		/// Registers a typed Pinia plugin on this root instance.
		/// </summary>
		/// <typeparam name="TStore">提供给插件上下文的类型化 store 投影。The typed store projection supplied to the plugin context.</typeparam>
		/// <param name="plugin">带类型的插件回调。The typed plugin callback to register.</param>
		/// <returns>同一个 Pinia 实例。The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore>(PiniaPlugin<TStore> plugin)
			where TStore : class;

		/// <summary>
		/// 在此根实例上注册一个带类型的 Pinia 插件，该插件具有强类型的插件可见选项投影。
		/// Registers a typed Pinia plugin on this root instance with a strongly typed
		/// plugin-visible options projection.
		/// </summary>
		/// <typeparam name="TStore">提供给插件上下文的类型化 store 投影。The typed store projection supplied to the plugin context.</typeparam>
		/// <typeparam name="TOptions">类型化的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
		/// <param name="plugin">带类型的插件回调。The typed plugin callback to register.</param>
		/// <returns>同一个 Pinia 实例。The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions>(PiniaPlugin<TStore, TOptions> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin;

		/// <summary>
		/// 在此根实例上注册一个完全类型化的 Pinia 插件，包括合并的扩展对象返回形状。
		/// Registers a fully typed Pinia plugin on this root instance, including the
		/// merged extension-object return shape.
		/// </summary>
		/// <typeparam name="TStore">提供给插件上下文的类型化 store 投影。The typed store projection supplied to the plugin context.</typeparam>
		/// <typeparam name="TOptions">类型化的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
		/// <typeparam name="TExtension">插件返回的类型化扩展对象。The typed extension object returned by the plugin.</typeparam>
		/// <param name="plugin">带类型的插件回调。The typed plugin callback to register.</param>
		/// <returns>同一个 Pinia 实例。The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions, TExtension>(PiniaPlugin<TStore, TOptions, TExtension> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin
			where TExtension : Vue3.VueProps;

		/// <summary>
		/// 在此根实例上注册一个完全类型化的 Pinia 插件，其上下文还将当前 store 投影到先前插件可见的显式自定义属性。
		/// Registers a fully typed Pinia plugin on this root instance whose context also
		/// projects the current store to explicit custom-properties visible from earlier
		/// plugins.
		/// </summary>
		/// <typeparam name="TStore">插件上下文提供的基础类型化 store 投影。The base typed store projection supplied by the plugin context.</typeparam>
		/// <typeparam name="TOptions">类型化的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
		/// <typeparam name="TCustomProperties">当前 store 上已可见的插件添加的自定义 store 属性。The plugin-added custom store properties already visible on the current store.</typeparam>
		/// <typeparam name="TExtension">插件返回的类型化扩展对象。The typed extension object returned by the plugin.</typeparam>
		/// <param name="plugin">带类型的插件回调。The typed plugin callback to register.</param>
		/// <returns>同一个 Pinia 实例。The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions, TCustomProperties, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TExtension> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin
			where TCustomProperties : Vue3.VueProps
			where TExtension : Vue3.VueProps;

		/// <summary>
		/// 在此根实例上注册一个完全类型化的 Pinia 插件，其上下文还将当前 store 投影到先前插件可见的显式自定义属性和自定义状态视图。
		/// Registers a fully typed Pinia plugin on this root instance whose context also
		/// projects the current store to explicit custom-properties and custom-state
		/// views visible from earlier plugins.
		/// </summary>
		/// <typeparam name="TStore">插件上下文提供的基础类型化 store 投影。The base typed store projection supplied by the plugin context.</typeparam>
		/// <typeparam name="TOptions">类型化的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
		/// <typeparam name="TCustomProperties">当前 store 上已可见的插件添加的自定义 store 属性。The plugin-added custom store properties already visible on the current store.</typeparam>
		/// <typeparam name="TCustomState"><c>store.$state</c> 上已可见的插件添加的自定义状态。The plugin-added custom state already visible on <c>store.$state</c>.</typeparam>
		/// <typeparam name="TExtension">插件返回的类型化扩展对象。The typed extension object returned by the plugin.</typeparam>
		/// <param name="plugin">带类型的插件回调。The typed plugin callback to register.</param>
		/// <returns>同一个 Pinia 实例。The same Pinia instance.</returns>
		[Description("@#use")]
		public extern PiniaInstance Use<TStore, TOptions, TCustomProperties, TCustomState, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TCustomState, TExtension> plugin)
			where TStore : class
			where TOptions : DefineStoreOptionsInPlugin
			where TCustomProperties : Vue3.VueProps
			where TCustomState : PiniaStateTree
			where TExtension : Vue3.VueProps;
	}

	/// <summary>
	/// 传递给 <c>pinia.use(...)</c> 的插件上下文。
	/// Plugin context passed to <c>pinia.use(...)</c>.
	/// </summary>
	public abstract class PiniaPluginContext
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// 此 Pinia 根所安装的 Vue 应用实例。
		/// The Vue application instance this Pinia root was installed on.
		/// </summary>
		[Description("@#app")]
		public extern Vue3.VueApp App { get; }

		/// <summary>
		/// 当前正在调用插件的 Pinia 根实例。
		/// The Pinia root instance currently invoking the plugin.
		/// </summary>
		[Description("@#pinia")]
		public extern PiniaInstance Pinia { get; }

		/// <summary>
		/// 当前正在扩展的具体 store。
		/// The concrete store currently being extended.
		/// </summary>
		[Description("@#store")]
		public extern StoreGeneric Store { get; }

		/// <summary>
		/// 当前 store 的定义选项对象。
		/// The defining options object for the current store.
		/// </summary>
		[Description("@#options")]
		public extern DefineStoreOptionsInPlugin Options { get; }
	}

	/// <summary>
	/// 当当前 store 被投影到更强的用户声明类型时，传递给 <c>pinia.use(...)</c> 的带类型插件上下文。
	/// Typed plugin context passed to <c>pinia.use(...)</c> when the current store is
	/// projected to a stronger user-declared type.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的类型化 store 投影。The typed store projection supplied by the plugin context.</typeparam>
	public abstract class PiniaPluginContext<TStore> : PiniaPluginContext
		where TStore : class
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// 当前正在扩展的具体 store，已投影到更强的用户声明类型。
		/// The concrete store currently being extended, projected to a stronger
		/// user-declared type.
		/// </summary>
		[Description("@#store")]
		public new extern TStore Store { get; }
	}

	/// <summary>
	/// 当当前 store 和插件可见选项包都被投影到更强的用户声明类型时，传递给 <c>pinia.use(...)</c> 的带类型插件上下文。
	/// Typed plugin context passed to <c>pinia.use(...)</c> when both the current store
	/// and the plugin-visible options bag are projected to stronger user-declared types.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的类型化 store 投影。The typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">类型化的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
	public abstract class PiniaPluginContext<TStore, TOptions> : PiniaPluginContext<TStore>
		where TStore : class
		where TOptions : DefineStoreOptionsInPlugin
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// 当前 store 的定义选项对象，已投影到更强的用户声明插件可见类型。
		/// The defining options object for the current store, projected to a stronger
		/// user-declared plugin-visible type.
		/// </summary>
		[Description("@#options")]
		public new extern TOptions Options { get; }
	}

	/// <summary>
	/// 当当前 store 还应通过先前插件安装的显式插件添加自定义属性来查看时，传递给 <c>pinia.use(...)</c> 的带类型插件上下文。
	/// Typed plugin context passed to <c>pinia.use(...)</c> when the current store
	/// should also be viewed through explicit plugin-added custom properties that were
	/// installed by earlier plugins.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的基础类型化 store 投影。The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">类型化的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">当前 store 上已可见的插件添加的自定义 store 属性。The plugin-added custom store properties already visible on the current store.</typeparam>
	public abstract class PiniaPluginContext<TStore, TOptions, TCustomProperties> : PiniaPluginContext<TStore, TOptions>
		where TStore : class
		where TOptions : DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// 当前正在扩展的具体 store，已投影到其基础 store 契约和插件添加的自定义属性视图。
		/// The concrete store currently being extended, projected to both its base store
		/// contract and the plugin-added custom-properties view.
		/// </summary>
		[Description("@#store")]
		public new extern ProjectedStore<TStore, TCustomProperties> Store { get; }
	}

	/// <summary>
	/// 当当前 store 还应通过先前插件安装的显式插件添加自定义属性和自定义状态来查看时，传递给 <c>pinia.use(...)</c> 的带类型插件上下文。
	/// Typed plugin context passed to <c>pinia.use(...)</c> when the current store
	/// should also be viewed through explicit plugin-added custom properties and
	/// custom state installed by earlier plugins.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的基础类型化 store 投影。The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">类型化的插件可见选项投影。The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">当前 store 上已可见的插件添加的自定义 store 属性。The plugin-added custom store properties already visible on the current store.</typeparam>
	/// <typeparam name="TCustomState"><c>store.$state</c> 上已可见的插件添加的自定义状态。The plugin-added custom state already visible on <c>store.$state</c>.</typeparam>
	public abstract class PiniaPluginContext<TStore, TOptions, TCustomProperties, TCustomState> : PiniaPluginContext<TStore, TOptions, TCustomProperties>
		where TStore : class
		where TOptions : DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree
	{
		protected PiniaPluginContext()
		{
		}

		/// <summary>
		/// 当前正在扩展的具体 store，已投影到其基础 store 契约和插件添加的自定义属性/自定义状态视图。
		/// The concrete store currently being extended, projected to both its base store
		/// contract and the plugin-added custom-properties/custom-state views.
		/// </summary>
		[Description("@#store")]
		public new extern ProjectedStore<TStore, TCustomProperties, TCustomState> Store { get; }
	}

	/// <summary>
	/// 每个 Pinia store 实例共享的 store 属性。
	/// Store properties shared across every Pinia store instance.
	/// </summary>
	public abstract class StoreProperties
	{
		protected StoreProperties()
		{
		}

		/// <summary>
		/// Store 标识符。
		/// Store identifier.
		/// </summary>
		[Description("@#$id")]
		public extern string Id { get; }

		/// <summary>
		/// 由 Pinia 插件注册的自定义 store 属性。
		/// Custom store properties registered by Pinia plugins.
		/// </summary>
		[Description("@#_customProperties")]
		public extern Set<string> CustomProperties { get; }
	}

	/// <summary>
	/// 每个 Pinia store 实例共享的最小 store 运行时形状基类。
	/// Minimal store runtime-shape base shared by every Pinia store instance.
	/// </summary>
	public abstract class StoreGeneric : StoreProperties
	{
		protected StoreGeneric()
		{
		}

		/// <summary>
		/// 为此 store 注册一个 action 监听器。
		/// Registers an action listener for this store.
		/// </summary>
		/// <param name="callback">监听器回调。The listener callback.</param>
		/// <returns>用于分离监听器的回调。A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction(PiniaStoreActionListener callback);

		/// <summary>
		/// 为此 store 注册一个带类型的 action 监听器。
		/// Registers a typed action listener for this store.
		/// </summary>
		/// <typeparam name="TStore">提供给监听器上下文的类型化 store 投影。The typed store projection supplied to the listener context.</typeparam>
		/// <param name="callback">带类型的监听器回调。The typed listener callback.</param>
		/// <returns>用于分离监听器的回调。A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction<TStore>(PiniaStoreActionListener<TStore> callback)
			where TStore : class;

		/// <summary>
		/// 注册一个与当前组件作用域分离的 action 监听器。
		/// Registers an action listener detached from the current component scope.
		/// </summary>
		/// <param name="callback">监听器回调。The listener callback.</param>
		/// <param name="detached">监听器是否应比当前组件作用域存活更久。Whether the listener should outlive the current component scope.</param>
		/// <returns>用于分离监听器的回调。A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction(PiniaStoreActionListener callback, bool detached);

		/// <summary>
		/// 注册一个与当前组件作用域分离的带类型 action 监听器。
		/// Registers a typed action listener detached from the current component scope.
		/// </summary>
		/// <typeparam name="TStore">提供给监听器上下文的类型化 store 投影。The typed store projection supplied to the listener context.</typeparam>
		/// <param name="callback">带类型的监听器回调。The typed listener callback.</param>
		/// <param name="detached">监听器是否应比当前组件作用域存活更久。Whether the listener should outlive the current component scope.</param>
		/// <returns>用于分离监听器的回调。A callback that detaches the listener.</returns>
		[Description("@#$onAction")]
		public extern PiniaDetachCallback OnAction<TStore>(PiniaStoreActionListener<TStore> callback, bool detached)
			where TStore : class;

		/// <summary>
		/// 销毁 store 实例并拆除其响应式作用域。
		/// Disposes the store instance and tears down its reactive scope.
		/// </summary>
		[Description("@#$dispose")]
		public extern void Dispose();
	}

	/// <summary>
	/// 带类型的 Pinia store 基类，公开常见的 <c>$state</c> / <c>$patch</c> / <c>$reset</c> / <c>$subscribe</c> 接口。
	/// Typed Pinia store base exposing the common <c>$state</c> / <c>$patch</c> /
	/// <c>$reset</c> / <c>$subscribe</c> surface.
	/// </summary>
	/// <typeparam name="TState">类型化的 store 状态投影。The typed store-state projection.</typeparam>
	public abstract class Store<TState> : StoreGeneric
		where TState : PiniaStateTree
	{
		protected Store()
		{
		}

		/// <summary>
		/// 当前实时的 store 状态。
		/// Current live store state.
		/// </summary>
		[Description("@#$state")]
		public extern TState State { get; set; }

		/// <summary>
		/// 对当前 store 状态应用部分对象补丁。
		/// Applies a partial object patch to the current store state.
		/// </summary>
		/// <param name="partialState">要合并到 store 中的部分状态对象。The partial state object to merge into the store.</param>
		[Description("@#$patch")]
		public extern void Patch(PiniaStatePatch<TState> partialState);

		/// <summary>
		/// 对当前 store 状态应用函数补丁。
		/// Applies a function patch to the current store state.
		/// </summary>
		/// <param name="patcher">原地修改当前状态的回调。The callback that mutates the current state in place.</param>
		[Description("@#$patch")]
		public extern void Patch(PiniaStatePatchCallback<TState> patcher);

		/// <summary>
		/// 将 store 状态重置为原始 <c>state()</c> 工厂值。
		/// Resets the store state back to the original <c>state()</c> factory value.
		/// </summary>
		[Description("@#$reset")]
		public extern void Reset();

		/// <summary>
		/// 订阅 store 状态变更。
		/// Subscribes to store state mutations.
		/// </summary>
		/// <param name="callback">订阅回调。The subscription callback.</param>
		/// <returns>用于分离订阅的回调。A callback that detaches the subscription.</returns>
		[Description("@#$subscribe")]
		public extern PiniaDetachCallback Subscribe(PiniaSubscriptionCallback<TState> callback);

		/// <summary>
		/// 使用显式订阅选项订阅 store 状态变更。
		/// Subscribes to store state mutations with explicit subscription options.
		/// </summary>
		/// <param name="callback">订阅回调。The subscription callback.</param>
		/// <param name="options">控制 watcher 刷新行为和分离作用域的订阅选项。Subscription options controlling watcher flush behavior and detach scope.</param>
		/// <returns>用于分离订阅的回调。A callback that detaches the subscription.</returns>
		[Description("@#$subscribe")]
		public extern PiniaDetachCallback Subscribe(PiniaSubscriptionCallback<TState> callback, SubscribeOptions options);
	}

	/// <summary>
	/// 仅开发环境下的变更调试事件，提供给 <c>$subscribe()</c>。
	/// Pinia 根据变更类型报告单个调试事件或事件批次。
	/// Dev-only mutation debug events supplied to <c>$subscribe()</c>.
	/// Pinia reports either one debugger event or an event batch depending on the
	/// mutation kind.
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct SubscriptionMutationEvents
	{
		private readonly byte _kind;
		private readonly Vue3.VueDebuggerEvent? _event;
		private readonly Vue3.VueDebuggerEvent[]? _batch;

		private SubscriptionMutationEvents(Vue3.VueDebuggerEvent value)
		{
			_kind = 1;
			_event = value;
			_batch = default;
		}

		private SubscriptionMutationEvents(Vue3.VueDebuggerEvent[] value)
		{
			_kind = 2;
			_event = default;
			_batch = value;
		}

		public Vue3.VueDebuggerEvent? AsEvent => _kind == 1 ? _event : default;

		public Vue3.VueDebuggerEvent[]? AsBatch => _kind == 2 ? _batch : default;

		public static implicit operator SubscriptionMutationEvents(Vue3.VueDebuggerEvent value)
			=> new(value);

		public static implicit operator SubscriptionMutationEvents(Vue3.VueDebuggerEvent[] value)
			=> new(value);
	}

	/// <summary>
	/// 提供给 <c>$subscribe()</c> 的基础变更元数据。
	/// 具体运行时形状由
	/// <see cref="SubscriptionMutationDirect{TState}"/>、
	/// <see cref="SubscriptionMutationPatchFunction{TState}"/> 和
	/// <see cref="SubscriptionMutationPatchObject{TState}"/> 建模。
	/// Base mutation metadata supplied to <c>$subscribe()</c>.
	/// Concrete runtime shapes are modeled by
	/// <see cref="SubscriptionMutationDirect{TState}"/>,
	/// <see cref="SubscriptionMutationPatchFunction{TState}"/>, and
	/// <see cref="SubscriptionMutationPatchObject{TState}"/>.
	/// </summary>
	/// <typeparam name="TState">类型化的 store 状态投影。The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutation()
		{
		}

		/// <summary>
		/// 触发回调的变更类型。
		/// The kind of mutation that triggered the callback.
		/// </summary>
		[Description("@#type")]
		public extern MutationType Type { get; }

		/// <summary>
		/// 触发回调的 store id。
		/// The store id that triggered the callback.
		/// </summary>
		[Description("@#storeId")]
		public extern string StoreId { get; }

		/// <summary>
		/// 为当前变更发出的仅开发环境调试器事件。
		/// 根据变更类型，可以是单个事件或事件批次。
		/// Dev-only debugger events emitted for the current mutation.
		/// Depending on mutation kind this can be a single event or an event batch.
		/// </summary>
		[Description("@#events")]
		public extern SubscriptionMutationEvents? Events { get; }
	}

	/// <summary>
	/// 提供给 <c>$subscribe()</c> 的直接赋值变更元数据。
	/// Direct assignment mutation metadata supplied to <c>$subscribe()</c>.
	/// </summary>
	/// <typeparam name="TState">类型化的 store 状态投影。The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutationDirect<TState> : SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutationDirect()
		{
		}

		/// <summary>
		/// 为直接赋值发出的调试器事件。
		/// The debugger event emitted for the direct assignment.
		/// </summary>
		[Description("@#events")]
		public new extern Vue3.VueDebuggerEvent Events { get; }
	}

	/// <summary>
	/// 提供给 <c>$subscribe()</c> 的函数补丁变更元数据。
	/// Function-patch mutation metadata supplied to <c>$subscribe()</c>.
	/// </summary>
	/// <typeparam name="TState">类型化的 store 状态投影。The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutationPatchFunction<TState> : SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutationPatchFunction()
		{
		}

		/// <summary>
		/// 为函数补丁发出的调试器事件。
		/// The debugger events emitted for the function patch.
		/// </summary>
		[Description("@#events")]
		public new extern Vue3.VueDebuggerEvent[] Events { get; }
	}

	/// <summary>
	/// 提供给 <c>$subscribe()</c> 的对象补丁变更元数据。
	/// Object-patch mutation metadata supplied to <c>$subscribe()</c>.
	/// </summary>
	/// <typeparam name="TState">类型化的 store 状态投影。The typed store-state projection.</typeparam>
	public abstract class SubscriptionMutationPatchObject<TState> : SubscriptionMutation<TState>
		where TState : PiniaStateTree
	{
		protected SubscriptionMutationPatchObject()
		{
		}

		/// <summary>
		/// 应用于 store 的对象补丁负载。
		/// The object patch payload applied to the store.
		/// </summary>
		[Description("@#payload")]
		public extern PiniaStatePatch<TState> Payload { get; }

		/// <summary>
		/// 为对象补丁发出的调试器事件。
		/// The debugger events emitted for the object patch.
		/// </summary>
		[Description("@#events")]
		public new extern Vue3.VueDebuggerEvent[] Events { get; }
	}

	/// <summary>
	/// 提供给 <c>$onAction()</c> 的无类型 action 监听器上下文。
	/// Untyped action listener context supplied to <c>$onAction()</c>.
	/// </summary>
	public abstract class StoreActionListenerContext
	{
		protected StoreActionListenerContext()
		{
		}

		/// <summary>
		/// 正在调用的 action 名称。
		/// The action name being invoked.
		/// </summary>
		[Description("@#name")]
		public extern string Name { get; }

		/// <summary>
		/// 调用方传递的原始 action 参数。
		/// The raw action arguments passed by the caller.
		/// </summary>
		[Description("@#args")]
		public extern PiniaValue[] Args { get; }

		/// <summary>
		/// 注册一个在 action 完成后运行的回调。
		/// Registers a callback that runs after the action completes.
		/// </summary>
		/// <param name="callback">action 完成后调用的回调。The callback to invoke after action completion.</param>
		[Description("@#after")]
		public extern void After(Action callback);

		/// <summary>
		/// 注册一个在 action 完成后接收 action 结果的回调。
		/// Registers a callback that receives the action result after the action completes.
		/// </summary>
		/// <param name="callback">action 完成后调用的回调。The callback to invoke after action completion.</param>
		[Description("@#after")]
		public extern void After(Action<PiniaValue?> callback);

		/// <summary>
		/// 注册一个在 action 完成后接收 action 结果的回调，结果投影到显式的用户声明结果类型。
		/// Registers a callback that receives the action result after the action completes,
		/// projected to an explicit user-declared result type.
		/// </summary>
		/// <typeparam name="TResult">期望的 action 结果类型。The expected action result type.</typeparam>
		/// <param name="callback">action 完成后调用的回调。The callback to invoke after action completion.</param>
		[Description("@#after")]
		public extern void After<TResult>(Action<TResult> callback);

		/// <summary>
		/// 注册一个在 action 抛出或拒绝时运行的回调，使用桥接的 unknown-like <see cref="PiniaValue"/> 投影。
		/// Registers a callback that runs when the action throws or rejects, using the
		/// bridge's unknown-like <see cref="PiniaValue"/> projection.
		/// </summary>
		/// <param name="callback">action 抛出时调用的回调。The callback to invoke when the action throws.</param>
		[Description("@#onError")]
		public extern void OnAnyError(Action<PiniaValue?> callback);

		/// <summary>
		/// 注册一个在 action 抛出或拒绝时运行的回调，投影到显式的用户声明错误类型。
		/// Registers a callback that runs when the action throws or rejects, projected
		/// to an explicit user-declared error type.
		/// </summary>
		/// <typeparam name="TError">期望的错误值类型。The expected error value type.</typeparam>
		/// <param name="callback">action 抛出时调用的回调。The callback to invoke when the action throws.</param>
		[Description("@#onError")]
		public extern void OnError<TError>(Action<TError> callback);

		/// <summary>
		/// 注册一个在 action 抛出时运行的回调，使用 CLR 风格的 <see cref="Error"/> 便利投影用于常见宿主路径。
		/// Registers a callback that runs when the action throws, using the CLR-like
		/// <see cref="Error"/> convenience projection for common host paths.
		/// </summary>
		/// <param name="callback">action 抛出时调用的回调。The callback to invoke when the action throws.</param>
		[Description("@#onError")]
		public extern void OnError(Action<Error> callback);
	}

	/// <summary>
	/// 提供给 <c>$onAction()</c> 的带类型 action 监听器上下文。
	/// Typed action listener context supplied to <c>$onAction()</c>.
	/// </summary>
	/// <typeparam name="TStore">类型化的 store 投影。The typed store projection.</typeparam>
	public abstract class StoreActionListenerContext<TStore> : StoreActionListenerContext
		where TStore : class
	{
		protected StoreActionListenerContext()
		{
		}

		/// <summary>
		/// 正在调用 action 的具体 store 实例。
		/// The concrete store instance invoking the action.
		/// </summary>
		[Description("@#store")]
		public extern TStore Store { get; }
	}

	/// <summary>
	/// 当调用方希望将 action 名称和参数数组视图绑定到更强的用户声明契约时，对带类型的 Pinia action 监听器上下文的显式投影视图。
	/// 此包装器不创建新的运行时对象；它仅在同一个 action 上下文实例上公开额外的类型化视图。
	/// Explicit projected view over a typed Pinia action-listener context when the
	/// caller wants to bind the action name and argument-array view to stronger
	/// user-declared contracts.
	/// This wrapper does not create a new runtime object; it only exposes additional
	/// typed views over the same action context instance.
	/// </summary>
	/// <typeparam name="TStore">监听器上下文提供的基础类型化 store 投影。The base typed store projection supplied by the listener context.</typeparam>
	/// <typeparam name="TActionName">调用方期望的显式 action 名称契约。The explicit action-name contract expected by the caller.</typeparam>
	/// <typeparam name="TArgs">调用方期望的显式参数数组视图契约。The explicit argument-array view contract expected by the caller.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedActionContext<TStore, TActionName, TArgs> : StoreActionListenerContext<TStore>
		where TStore : class
		where TArgs : class
	{
		protected ProjectedActionContext()
		{
		}

		/// <summary>
		/// 返回投影到更强的用户声明 action 名称契约的同一运行时 action 名称。
		/// Returns the same runtime action name projected to a stronger user-declared
		/// action-name contract.
		/// </summary>
		[Description("@#name")]
		public extern TActionName ActionName { get; }

		/// <summary>
		/// 返回投影到更强的用户声明参数视图契约的同一运行时参数数组。
		/// Returns the same runtime argument array projected to a stronger user-declared
		/// argument-view contract.
		/// </summary>
		[Description("@#args")]
		public extern TArgs ActionArgs { get; }
	}

	/// <summary>
	/// Pinia action 参数数组的显式类型化视图。
	/// 此包装器保持同一运行时数组对象，并公开高元数类型化槽位投影所使用的共享数组契约。
	/// Explicit typed view over a Pinia action argument array.
	/// This wrapper keeps the same runtime array object and exposes the shared array
	/// contract used by higher-arity typed slot projections.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView
	{
		protected ActionArgsView()
		{
		}

		/// <summary>
		/// 运行时数组长度。
		/// The runtime array length.
		/// </summary>
		[Description("@#length")]
		public extern int Length { get; }
	}

	/// <summary>
	/// 具有一个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with one typed slot.
	/// </summary>
	/// <typeparam name="TArg0">第一个 action 参数的类型。The type of the first action argument.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0> : ActionArgsView
	{
		protected ActionArgsView()
		{
		}

		/// <summary>
		/// 投影到更强的用户声明类型的第一个运行时参数。
		/// The first runtime argument projected to a stronger user-declared type.
		/// </summary>
		[Description("@#[0]")]
		public extern TArg0 Arg0 { get; }
	}

	/// <summary>
	/// 具有两个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with two typed slots.
	/// </summary>
	/// <typeparam name="TArg0">第一个 action 参数的类型。The type of the first action argument.</typeparam>
	/// <typeparam name="TArg1">第二个 action 参数的类型。The type of the second action argument.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1> : ActionArgsView<TArg0>
	{
		protected ActionArgsView()
		{
		}

		/// <summary>
		/// 投影到更强的用户声明类型的第二个运行时参数。
		/// The second runtime argument projected to a stronger user-declared type.
		/// </summary>
		[Description("@#[1]")]
		public extern TArg1 Arg1 { get; }
	}

	/// <summary>
	/// 具有三个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with three typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2> : ActionArgsView<TArg0, TArg1>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[2]")]
		public extern TArg2 Arg2 { get; }
	}

	/// <summary>
	/// 具有四个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with four typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3> : ActionArgsView<TArg0, TArg1, TArg2>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[3]")]
		public extern TArg3 Arg3 { get; }
	}

	/// <summary>
	/// 具有五个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with five typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4> : ActionArgsView<TArg0, TArg1, TArg2, TArg3>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[4]")]
		public extern TArg4 Arg4 { get; }
	}

	/// <summary>
	/// 具有六个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with six typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[5]")]
		public extern TArg5 Arg5 { get; }
	}

	/// <summary>
	/// 具有七个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with seven typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[6]")]
		public extern TArg6 Arg6 { get; }
	}

	/// <summary>
	/// 具有八个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with eight typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[7]")]
		public extern TArg7 Arg7 { get; }
	}

	/// <summary>
	/// 具有九个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with nine typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[8]")]
		public extern TArg8 Arg8 { get; }
	}

	/// <summary>
	/// 具有十个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with ten typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[9]")]
		public extern TArg9 Arg9 { get; }
	}

	/// <summary>
	/// 具有十一个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with eleven typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[10]")]
		public extern TArg10 Arg10 { get; }
	}

	/// <summary>
	/// 具有十二个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with twelve typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[11]")]
		public extern TArg11 Arg11 { get; }
	}

	/// <summary>
	/// 具有十三个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with thirteen typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[12]")]
		public extern TArg12 Arg12 { get; }
	}

	/// <summary>
	/// 具有十四个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with fourteen typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[13]")]
		public extern TArg13 Arg13 { get; }
	}

	/// <summary>
	/// 具有十五个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with fifteen typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[14]")]
		public extern TArg14 Arg14 { get; }
	}

	/// <summary>
	/// 具有十六个类型化槽位的 Pinia action 参数数组的显式类型化视图。
	/// Explicit typed view over a Pinia action argument array with sixteen typed slots.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> : ActionArgsView<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>
	{
		protected ActionArgsView()
		{
		}

		[Description("@#[15]")]
		public extern TArg15 Arg15 { get; }
	}

	/// <summary>
	/// 非泛型 store 定义基类，由 <c>mapStores()</c> 等接受异构 store 定义列表的辅助 API 使用。
	/// Non-generic store-definition base used by helper APIs such as <c>mapStores()</c>
	/// that accept heterogeneous store-definition lists.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class StoreDefinition
	{
		protected StoreDefinition()
		{
		}
	}

	/// <summary>
	/// 由 <c>defineStore()</c> 返回的可调用 store 定义包装器。
	/// Pinia 将其公开为函数对象；C# 通过显式的 <c>Use(...)</c> 方法包装调用接口，
	/// 使 API 保持可发现性，且不依赖编译器特定的函数对象魔术。
	/// Callable store-definition wrapper returned by <c>defineStore()</c>.
	/// Pinia exposes this as a function object; C# wraps the call surface in explicit
	/// <c>Use(...)</c> methods so the API stays discoverable and does not rely on
	/// compiler-specific function-object magic.
	/// </summary>
	/// <typeparam name="TStore">包装器返回的类型化 store 投影。The typed store projection returned by the wrapper.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class StoreDefinition<TStore> : StoreDefinition
		where TStore : class
	{
		protected StoreDefinition()
		{
		}

		/// <summary>
		/// 定义时声明的 store 标识符。
		/// Store identifier declared at definition time.
		/// </summary>
		[Description("@#$id")]
		public extern string Id { get; }

		/// <summary>
		/// 从当前活动的 Pinia 根创建或获取 store 实例。
		/// Creates or retrieves the store instance from the currently active Pinia root.
		/// </summary>
		/// <returns>具体的 store 实例。The concrete store instance.</returns>
		[ECMAScriptInline("__arg1()")]
		public extern TStore Use();

		/// <summary>
		/// 从指定的 Pinia 根创建或获取 store 实例。
		/// Creates or retrieves the store instance from the supplied Pinia root.
		/// </summary>
		/// <param name="pinia">用于解析 store 的 Pinia 根实例。The Pinia root instance to resolve the store against.</param>
		/// <returns>具体的 store 实例。The concrete store instance.</returns>
		[ECMAScriptInline("__arg1(__arg2)")]
		public extern TStore Use(PiniaInstance pinia);

		/// <summary>
		/// Pinia HMR 流程使用的内部/高级调用形式。
		/// Internal/advanced call shape used by Pinia HMR flows.
		/// </summary>
		/// <param name="pinia">用于解析 store 的 Pinia 根实例。The Pinia root instance to resolve the store against.</param>
		/// <param name="hot">HMR 运行时提供的现有热 store 实例。The existing hot store instance supplied by the HMR runtime.</param>
		/// <returns>具体的 store 实例。The concrete store instance.</returns>
		[ECMAScriptInline("__arg1(__arg2, __arg3)")]
		public extern TStore Use(PiniaInstance pinia, StoreGeneric hot);
	}

	/// <summary>
	/// 当插件添加自定义属性时，对活动 Pinia store 的显式投影视图。
	/// 此包装器不创建新的运行时对象；它仅在同一个 store 实例上公开额外的类型化视图。
	/// Explicit projected view over a live Pinia store when plugins add custom
	/// properties.
	/// This wrapper does not create a new runtime object; it only exposes
	/// additional typed views over the same store instance.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStore<TStore, TCustomProperties>
		where TStore : class
		where TCustomProperties : Vue3.VueProps
	{
		protected ProjectedStore()
		{
		}

		/// <summary>
		/// 返回投影到其基础 store 契约的同一运行时 store。
		/// Returns the same runtime store projected to its base store contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern TStore AsStore();

		/// <summary>
		/// 返回投影到插件添加的自定义属性契约的同一运行时 store。
		/// Returns the same runtime store projected to the plugin-added custom
		/// properties contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern TCustomProperties AsCustomProperties();
	}

	/// <summary>
	/// 当插件同时添加自定义 store 属性和自定义状态属性时，对活动 Pinia store 的显式投影视图。
	/// Explicit projected view over a live Pinia store when plugins add both
	/// custom store properties and custom state properties.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	/// <typeparam name="TCustomState"><c>store.$state</c> 上插件添加的自定义状态投影。The plugin-added custom state projection on <c>store.$state</c>.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStore<TStore, TCustomProperties, TCustomState> : ProjectedStore<TStore, TCustomProperties>
		where TStore : class
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree
	{
		protected ProjectedStore()
		{
		}

		/// <summary>
		/// 返回投影到插件添加的自定义状态契约的当前 <c>store.$state</c> 对象。
		/// Returns the current <c>store.$state</c> object projected to the
		/// plugin-added custom state contract.
		/// </summary>
		[ECMAScriptInline("__arg1.$state")]
		public extern TCustomState AsCustomState();
	}

	/// <summary>
	/// 当插件添加的自定义属性应通过 <c>Use(...)</c> 传播时，对 store 定义的显式投影视图。
	/// 此包装器保持同一运行时 store 定义函数对象，仅更改调用接口的类型化结果。
	/// Explicit projected view over a store definition when plugin-added custom
	/// properties should propagate through <c>Use(...)</c>.
	/// This wrapper keeps the same runtime store definition function object and only
	/// changes the typed result of the call surface.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStoreDefinition<TStore, TCustomProperties> : StoreDefinition<ProjectedStore<TStore, TCustomProperties>>
		where TStore : class
		where TCustomProperties : Vue3.VueProps
	{
		protected ProjectedStoreDefinition()
		{
		}

		/// <summary>
		/// 返回投影回基础类型化 store 定义契约的同一运行时 store 定义。
		/// Returns the same runtime store definition projected back to the base
		/// typed store-definition contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern StoreDefinition<TStore> AsDefinition();
	}

	/// <summary>
	/// 当插件添加的自定义属性和自定义状态应通过 <c>Use(...)</c> 传播时，对 store 定义的显式投影视图。
	/// Explicit projected view over a store definition when plugin-added custom
	/// properties and custom state should propagate through <c>Use(...)</c>.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	/// <typeparam name="TCustomState"><c>store.$state</c> 上插件添加的自定义状态投影。The plugin-added custom state projection on <c>store.$state</c>.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class ProjectedStoreDefinition<TStore, TCustomProperties, TCustomState> : StoreDefinition<ProjectedStore<TStore, TCustomProperties, TCustomState>>
		where TStore : class
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree
	{
		protected ProjectedStoreDefinition()
		{
		}

		/// <summary>
		/// 返回投影回基础类型化 store 定义契约的同一运行时 store 定义。
		/// Returns the same runtime store definition projected back to the base
		/// typed store-definition contract.
		/// </summary>
		[ECMAScriptInline("__arg1")]
		public extern StoreDefinition<TStore> AsDefinition();
	}
}
