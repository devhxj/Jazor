using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	private const string IdentityInlineTemplate = "__arg1";

	private const string ClearActivePiniaInlineTemplate = "setActivePinia(undefined)";

	private const string TryProjectActionContextInlineTemplate = "(__arg1.name === __arg2 ? __arg1 : null)";

	/// <summary>
	/// 创建一个 Pinia 根实例，可通过 <c>app.use(createPinia())</c> 安装到 Vue 应用上。
	/// Creates a Pinia root instance that can be installed on a Vue app via
	/// <c>app.use(createPinia())</c>.
	/// </summary>
	[Description("@#createPinia")]
	public extern static PiniaInstance CreatePinia();

	/// <summary>
	/// 定义一个选项式 store，并返回可调用的 store 定义包装器。
	/// Defines an option-style store and returns the callable store-definition wrapper.
	/// </summary>
	/// <typeparam name="TState">store 的 <c>state()</c> 工厂返回的类型化状态记录。The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	/// <param name="id">唯一存储标识符。The unique store identifier.</param>
	/// <param name="options">选项式 store 定义对象。The option-style store definition object.</param>
	/// <returns>一个可调用的 store 定义包装器，用于创建或检索 store 实例。A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<Store<TState>> DefineStore<TState>(string id, DefineStoreOptions<TState> options)
		where TState : PiniaStateTree;

	/// <summary>
	/// 定义一个选项式 store，同时将运行时结果投影到更强的用户声明 store 类型。
	/// Defines an option-style store while projecting the runtime result to a stronger
	/// user-declared store type.
	/// </summary>
	/// <typeparam name="TStore">包装器返回的类型化 store 投影。The typed store projection returned by the wrapper.</typeparam>
	/// <typeparam name="TState">store 的 <c>state()</c> 工厂返回的类型化状态记录。The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	/// <param name="id">唯一存储标识符。The unique store identifier.</param>
	/// <param name="options">选项式 store 定义对象。The option-style store definition object.</param>
	/// <returns>一个可调用的 store 定义包装器，用于创建或检索 store 实例。A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore, TState>(string id, DefineStoreOptions<TState> options)
		where TStore : class
		where TState : PiniaStateTree;

	/// <summary>
	/// 通过无参 setup 回调定义一个 setup 风格的 store。
	/// Defines a setup-style store from a parameterless setup callback.
	/// </summary>
	/// <typeparam name="TStore">setup 回调返回的类型化 store 投影。The typed store projection returned by the setup callback.</typeparam>
	/// <param name="id">唯一存储标识符。The unique store identifier.</param>
	/// <param name="storeSetup">声明 store 表面的无参 setup 回调。The parameterless setup callback that declares the store surface.</param>
	/// <returns>一个可调用的 store 定义包装器，用于创建或检索 store 实例。A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore>(string id, Func<TStore> storeSetup)
		where TStore : class;

	/// <summary>
	/// 通过带有 helper 的 setup 回调定义一个 setup 风格的 store。
	/// Defines a setup-style store from a helper-aware setup callback.
	/// </summary>
	/// <typeparam name="TStore">setup 回调返回的类型化 store 投影。The typed store projection returned by the setup callback.</typeparam>
	/// <param name="id">唯一存储标识符。The unique store identifier.</param>
	/// <param name="storeSetup">接收 Pinia 的 setup-store helper 的 setup 回调。The setup callback that receives Pinia's setup-store helpers.</param>
	/// <returns>一个可调用的 store 定义包装器，用于创建或检索 store 实例。A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore>(string id, PiniaSetupStoreFactory<TStore> storeSetup)
		where TStore : class;

	/// <summary>
	/// 通过无参 setup 回调加上 setup-store 选项定义一个 setup 风格的 store。
	/// Defines a setup-style store from a parameterless setup callback plus setup-store options.
	/// </summary>
	/// <typeparam name="TStore">setup 回调返回的类型化 store 投影。The typed store projection returned by the setup callback.</typeparam>
	/// <param name="id">唯一存储标识符。The unique store identifier.</param>
	/// <param name="storeSetup">声明 store 表面的无参 setup 回调。The parameterless setup callback that declares the store surface.</param>
	/// <param name="options">附加的 setup-store 选项。Additional setup-store options.</param>
	/// <returns>一个可调用的 store 定义包装器，用于创建或检索 store 实例。A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore>(string id, Func<TStore> storeSetup, DefineSetupStoreOptions options)
		where TStore : class;

	/// <summary>
	/// 通过带有 helper 的 setup 回调加上 setup-store 选项定义一个 setup 风格的 store。
	/// Defines a setup-style store from a helper-aware setup callback plus setup-store options.
	/// </summary>
	/// <typeparam name="TStore">setup 回调返回的类型化 store 投影。The typed store projection returned by the setup callback.</typeparam>
	/// <param name="id">唯一存储标识符。The unique store identifier.</param>
	/// <param name="storeSetup">接收 Pinia 的 setup-store helper 的 setup 回调。The setup callback that receives Pinia's setup-store helpers.</param>
	/// <param name="options">附加的 setup-store 选项。Additional setup-store options.</param>
	/// <returns>一个可调用的 store 定义包装器，用于创建或检索 store 实例。A callable store-definition wrapper that creates or retrieves the store instance.</returns>
	[Description("@#defineStore")]
	public extern static StoreDefinition<TStore> DefineStore<TStore>(string id, PiniaSetupStoreFactory<TStore> storeSetup, DefineSetupStoreOptions options)
		where TStore : class;

	/// <summary>
	/// 返回当前活跃的 Pinia 根实例（如果已设置）。
	/// Returns the currently active Pinia root instance, if one has been set.
	/// </summary>
	[Description("@#getActivePinia")]
	public extern static PiniaInstance? GetActivePinia();

	/// <summary>
	/// 设置当前活跃的 Pinia 根实例。
	/// Sets the currently active Pinia root instance.
	/// </summary>
	/// <param name="pinia">应为后续 store 解析变为活跃状态的 Pinia 实例。The Pinia instance that should become active for subsequent store resolution.</param>
	/// <returns>相同的 <paramref name="pinia"/> 实例。The same <paramref name="pinia"/> instance.</returns>
	[Description("@#setActivePinia")]
	public extern static PiniaInstance SetActivePinia(PiniaInstance pinia);

	/// <summary>
	/// 清除当前活跃的 Pinia 根实例。
	/// 这是 Pinia 的 <c>setActivePinia(undefined)</c> 契约的显式宿主绑定。
	/// Clears the currently active Pinia root instance.
	/// This is the explicit host binding for Pinia's <c>setActivePinia(undefined)</c> contract.
	/// </summary>
	/// <returns>被清除的活跃根，在宿主端映射为 <c>null</c> / <c>undefined</c>。The cleared active root, which maps to <c>null</c> / <c>undefined</c> on the host side.</returns>
	[Description("@#setActivePinia")]
	[ECMAScriptInline(ClearActivePiniaInlineTemplate)]
	public extern static PiniaInstance? ClearActivePinia();

	/// <summary>
	/// 释放一个 Pinia 根实例及其上挂载的所有 store。
	/// Disposes a Pinia root instance and every store attached to it.
	/// </summary>
	/// <param name="pinia">要释放的 Pinia 实例。The Pinia instance to dispose.</param>
	[Description("@#disposePinia")]
	public extern static void DisposePinia(PiniaInstance pinia);

	/// <summary>
	/// 标记一个对象，使 Pinia 跳过对其的水合处理。
	/// Marks an object so Pinia skips hydration for it.
	/// </summary>
	/// <typeparam name="T">被标记的对象类型。The object type being marked.</typeparam>
	/// <param name="value">要标记的对象。The object to mark.</param>
	/// <returns>相同的 <paramref name="value"/> 实例。The same <paramref name="value"/> instance.</returns>
	[Description("@#skipHydrate")]
	public extern static T SkipHydrate<T>(T value)
		where T : class;

	/// <summary>
	/// 返回 Pinia 是否应对提供的运行时值进行水合。
	/// Returns whether Pinia should hydrate the supplied runtime value.
	/// </summary>
	/// <typeparam name="T">被测试的运行时值的静态类型。The static type of the runtime value being tested.</typeparam>
	/// <param name="value">要测试的运行时值。The runtime value to test.</param>
	/// <returns>当该值应参与水合时为 <c>true</c>。<c>true</c> when the value should participate in hydration.</returns>
	[Description("@#shouldHydrate")]
	public extern static bool ShouldHydrate<T>(T value);

	/// <summary>
	/// 使用默认的基于索引器的 refs 包，将 store 的响应式状态和 getter 转换为 ref。
	/// Converts a store's reactive state and getters into refs using the default
	/// indexer-based refs bag.
	/// </summary>
	/// <typeparam name="TStore">类型化 store 投影。The typed store projection.</typeparam>
	/// <param name="store">store 实例。The store instance.</param>
	/// <returns>映射 store 响应式成员的 refs 包。A refs bag mirroring the store's reactive members.</returns>
	[Description("@#storeToRefs")]
	public extern static StoreRefs<TStore> StoreToRefs<TStore>(TStore store)
		where TStore : class;

	/// <summary>
	/// 将 store 的响应式状态和 getter 转换为用户声明的类型化 refs 投影。
	/// Converts a store's reactive state and getters into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">用户声明的 refs 投影类型。The user-declared refs projection type.</typeparam>
	/// <typeparam name="TStore">类型化 store 投影。The typed store projection.</typeparam>
	/// <param name="store">store 实例。The store instance.</param>
	/// <returns>由 Pinia 产生的类型化 refs 投影。The typed refs projection produced by Pinia.</returns>
	[Description("@#storeToRefs")]
	public extern static TRefs StoreToRefs<TRefs, TStore>(TStore store)
		where TRefs : StoreRefs<TStore>
		where TStore : class;

	/// <summary>
	/// 使用数组形式的成员名，将 store 状态和 getter 映射到 Vue Options API 的
	/// <c>computed</c> 对象。
	/// Maps store state and getters into a Vue Options API <c>computed</c> object using
	/// array-form member names.
	/// </summary>
	/// <typeparam name="TStore">store 定义返回的类型化 store 投影。The typed store projection returned by the store definition.</typeparam>
	/// <param name="useStore">由 <c>defineStore()</c> 返回的 store 定义包装器。The store definition wrapper returned by <c>defineStore()</c>.</param>
	/// <param name="keys">应投影为 computed 条目的 store 成员名。The store member names that should be projected into computed entries.</param>
	/// <returns>适用于 <c>computed:</c> 的 Vue 选项包。A Vue options bag suitable for <c>computed:</c>.</returns>
	[Description("@#mapState")]
	public extern static Vue3.VueProps MapState<TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TStore : class;

	/// <summary>
	/// 使用数组形式的成员名，将 store 状态和 getter 映射到用户声明的类型化
	/// Vue Options API <c>computed</c> 投影。
	/// Maps store state and getters into a user-declared typed Vue Options API
	/// <c>computed</c> projection using array-form member names.
	/// </summary>
	/// <typeparam name="TComputed">用户声明的 computed 选项投影类型。The user-declared computed options projection type.</typeparam>
	/// <typeparam name="TStore">store 定义返回的类型化 store 投影。The typed store projection returned by the store definition.</typeparam>
	/// <param name="useStore">由 <c>defineStore()</c> 返回的 store 定义包装器。The store definition wrapper returned by <c>defineStore()</c>.</param>
	/// <param name="keys">应投影为 computed 条目的 store 成员名。The store member names that should be projected into computed entries.</param>
	/// <returns>适用于 <c>computed:</c> 的类型化 Vue 选项包。The typed Vue options bag suitable for <c>computed:</c>.</returns>
	[Description("@#mapState")]
	public extern static TComputed MapState<TComputed, TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TComputed : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// 使用对象形式的键映射，将 store 状态和 getter 映射到 Vue Options API 的
	/// <c>computed</c> 对象。
	/// Maps store state and getters into a Vue Options API <c>computed</c> object using
	/// object-form key mapping.
	/// </summary>
	/// <typeparam name="TStore">store 定义返回的类型化 store 投影。The typed store projection returned by the store definition.</typeparam>
	/// <param name="useStore">由 <c>defineStore()</c> 返回的 store 定义包装器。The store definition wrapper returned by <c>defineStore()</c>.</param>
	/// <param name="keyMapper">键映射器对象，其值为 store 成员名或自定义选择器。The key-mapper object whose values are store member names or custom selectors.</param>
	/// <returns>适用于 <c>computed:</c> 的 Vue 选项包。A Vue options bag suitable for <c>computed:</c>.</returns>
	[Description("@#mapState")]
	public extern static Vue3.VueProps MapState<TStore>(StoreDefinition<TStore> useStore, PiniaStateMapper<TStore> keyMapper)
		where TStore : class;

	/// <summary>
	/// 使用对象形式的键映射，将 store 状态和 getter 映射到用户声明的类型化
	/// Vue Options API <c>computed</c> 投影。
	/// Maps store state and getters into a user-declared typed Vue Options API
	/// <c>computed</c> projection using object-form key mapping.
	/// </summary>
	/// <typeparam name="TComputed">用户声明的 computed 选项投影类型。The user-declared computed options projection type.</typeparam>
	/// <typeparam name="TStore">store 定义返回的类型化 store 投影。The typed store projection returned by the store definition.</typeparam>
	/// <param name="useStore">由 <c>defineStore()</c> 返回的 store 定义包装器。The store definition wrapper returned by <c>defineStore()</c>.</param>
	/// <param name="keyMapper">键映射器对象，其值为 store 成员名或自定义选择器。The key-mapper object whose values are store member names or custom selectors.</param>
	/// <returns>适用于 <c>computed:</c> 的类型化 Vue 选项包。The typed Vue options bag suitable for <c>computed:</c>.</returns>
	[Description("@#mapState")]
	public extern static TComputed MapState<TComputed, TStore>(StoreDefinition<TStore> useStore, PiniaStateMapper<TStore> keyMapper)
		where TComputed : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// <see cref="MapState{TStore}(StoreDefinition{TStore},string[])"/> 的已弃用别名。
	/// Deprecated alias of <see cref="MapState{TStore}(StoreDefinition{TStore},string[])"/>.
	/// </summary>
	[Obsolete("Pinia's mapGetters is an alias for mapState; prefer MapState.")]
	[Description("@#mapGetters")]
	public extern static Vue3.VueProps MapGetters<TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TStore : class;

	/// <summary>
	/// <see cref="MapState{TComputed,TStore}(StoreDefinition{TStore},string[])"/> 的已弃用类型化别名。
	/// Deprecated typed alias of <see cref="MapState{TComputed,TStore}(StoreDefinition{TStore},string[])"/>.
	/// </summary>
	[Obsolete("Pinia's mapGetters is an alias for mapState; prefer MapState.")]
	[Description("@#mapGetters")]
	public extern static TComputed MapGetters<TComputed, TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TComputed : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// <see cref="MapState{TStore}(StoreDefinition{TStore},PiniaStateMapper{TStore})"/> 的已弃用别名。
	/// Deprecated alias of <see cref="MapState{TStore}(StoreDefinition{TStore},PiniaStateMapper{TStore})"/>.
	/// </summary>
	[Obsolete("Pinia's mapGetters is an alias for mapState; prefer MapState.")]
	[Description("@#mapGetters")]
	public extern static Vue3.VueProps MapGetters<TStore>(StoreDefinition<TStore> useStore, PiniaStateMapper<TStore> keyMapper)
		where TStore : class;

	/// <summary>
	/// <see cref="MapState{TComputed,TStore}(StoreDefinition{TStore},PiniaStateMapper{TStore})"/> 的已弃用类型化别名。
	/// Deprecated typed alias of <see cref="MapState{TComputed,TStore}(StoreDefinition{TStore},PiniaStateMapper{TStore})"/>.
	/// </summary>
	[Obsolete("Pinia's mapGetters is an alias for mapState; prefer MapState.")]
	[Description("@#mapGetters")]
	public extern static TComputed MapGetters<TComputed, TStore>(StoreDefinition<TStore> useStore, PiniaStateMapper<TStore> keyMapper)
		where TComputed : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// 使用数组形式的成员名，将可写的 store 状态映射到 Vue Options API 的
	/// <c>computed</c> 对象。
	/// Maps writable store state into a Vue Options API <c>computed</c> object using
	/// array-form member names.
	/// </summary>
	/// <typeparam name="TStore">store 定义返回的类型化 store 投影。The typed store projection returned by the store definition.</typeparam>
	/// <param name="useStore">由 <c>defineStore()</c> 返回的 store 定义包装器。The store definition wrapper returned by <c>defineStore()</c>.</param>
	/// <param name="keys">应投影为 computed 条目的可写状态成员名。The writable state member names that should be projected into computed entries.</param>
	/// <returns>适用于 <c>computed:</c> 的 Vue 选项包。A Vue options bag suitable for <c>computed:</c>.</returns>
	[Description("@#mapWritableState")]
	public extern static Vue3.VueProps MapWritableState<TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TStore : class;

	/// <summary>
	/// 使用数组形式的成员名，将可写的 store 状态映射到用户声明的类型化
	/// Vue Options API <c>computed</c> 投影。
	/// Maps writable store state into a user-declared typed Vue Options API
	/// <c>computed</c> projection using array-form member names.
	/// </summary>
	[Description("@#mapWritableState")]
	public extern static TComputed MapWritableState<TComputed, TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TComputed : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// 使用对象形式的键映射，将可写的 store 状态映射到 Vue Options API 的
	/// <c>computed</c> 对象。
	/// Maps writable store state into a Vue Options API <c>computed</c> object using
	/// object-form key mapping.
	/// </summary>
	[Description("@#mapWritableState")]
	public extern static Vue3.VueProps MapWritableState<TStore>(StoreDefinition<TStore> useStore, PiniaKeyMapper keyMapper)
		where TStore : class;

	/// <summary>
	/// 使用对象形式的键映射，将可写的 store 状态映射到用户声明的类型化
	/// Vue Options API <c>computed</c> 投影。
	/// Maps writable store state into a user-declared typed Vue Options API
	/// <c>computed</c> projection using object-form key mapping.
	/// </summary>
	[Description("@#mapWritableState")]
	public extern static TComputed MapWritableState<TComputed, TStore>(StoreDefinition<TStore> useStore, PiniaKeyMapper keyMapper)
		where TComputed : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// 使用数组形式的 action 名，将 store action 映射到 Vue Options API 的
	/// <c>methods</c> 对象。
	/// Maps store actions into a Vue Options API <c>methods</c> object using array-form
	/// action names.
	/// </summary>
	[Description("@#mapActions")]
	public extern static Vue3.VueProps MapActions<TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TStore : class;

	/// <summary>
	/// 使用数组形式的 action 名，将 store action 映射到用户声明的类型化
	/// Vue Options API <c>methods</c> 投影。
	/// Maps store actions into a user-declared typed Vue Options API <c>methods</c>
	/// projection using array-form action names.
	/// </summary>
	[Description("@#mapActions")]
	public extern static TMethods MapActions<TMethods, TStore>(StoreDefinition<TStore> useStore, string[] keys)
		where TMethods : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// 使用对象形式的键映射，将 store action 映射到 Vue Options API 的
	/// <c>methods</c> 对象。
	/// Maps store actions into a Vue Options API <c>methods</c> object using object-form
	/// key mapping.
	/// </summary>
	[Description("@#mapActions")]
	public extern static Vue3.VueProps MapActions<TStore>(StoreDefinition<TStore> useStore, PiniaKeyMapper keyMapper)
		where TStore : class;

	/// <summary>
	/// 使用对象形式的键映射，将 store action 映射到用户声明的类型化
	/// Vue Options API <c>methods</c> 投影。
	/// Maps store actions into a user-declared typed Vue Options API <c>methods</c>
	/// projection using object-form key mapping.
	/// </summary>
	[Description("@#mapActions")]
	public extern static TMethods MapActions<TMethods, TStore>(StoreDefinition<TStore> useStore, PiniaKeyMapper keyMapper)
		where TMethods : Vue3.VueProps
		where TStore : class;

	/// <summary>
	/// 使用 Pinia 的 store 后缀命名约定，将多个 store 映射到 Vue Options API 的
	/// <c>computed</c> 对象。
	/// Maps multiple stores into a Vue Options API <c>computed</c> object using Pinia's
	/// store-suffix naming convention.
	/// </summary>
	/// <param name="stores">应投影到组件实例上的 store 定义。The store definitions that should be projected onto the component instance.</param>
	/// <returns>适用于 <c>computed:</c> 的 Vue 选项包。A Vue options bag suitable for <c>computed:</c>.</returns>
	[Description("@#mapStores")]
	public extern static Vue3.VueProps MapStores(params StoreDefinition[] stores);

	/// <summary>
	/// 使用 Pinia 的 store 后缀命名约定，将多个 store 映射到用户声明的类型化
	/// Vue Options API <c>computed</c> 投影。
	/// Maps multiple stores into a user-declared typed Vue Options API <c>computed</c>
	/// projection using Pinia's store-suffix naming convention.
	/// </summary>
	[Description("@#mapStores")]
	public extern static TComputed MapStores<TComputed>(params StoreDefinition[] stores)
		where TComputed : Vue3.VueProps;

	/// <summary>
	/// 更改 <c>mapStores()</c> 在将 store 定义投影到组件实例属性时使用的后缀。
	/// Changes the suffix used by <c>mapStores()</c> when projecting store definitions
	/// onto component instance properties.
	/// </summary>
	/// <param name="suffix">附加到每个映射 store id 的后缀。The suffix appended to each mapped store id.</param>
	[Description("@#setMapStoreSuffix")]
	public extern static void SetMapStoreSuffix(string suffix);

	/// <summary>
	/// 将一个活跃的 store 实例投影到一个显式包装器，该包装器暴露基础 store 契约
	/// 和类型化的插件添加自定义属性视图。
	/// 这仅是编译期投影，不会创建新的运行时对象。
	/// Projects a live store instance to an explicit wrapper exposing both the base
	/// store contract and a typed plugin-added custom-properties view.
	/// This is a compile-time projection only and does not create a new runtime object.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	/// <param name="store">要投影的活跃 store 实例。The live store instance to project.</param>
	/// <returns>覆盖相同运行时 store 对象的显式投影包装器。An explicit projected wrapper over the same runtime store object.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static ProjectedStore<TStore, TCustomProperties> ProjectStore<TStore, TCustomProperties>(TStore store)
		where TStore : class
		where TCustomProperties : Vue3.VueProps;

	/// <summary>
	/// 将一个类型化的 action 监听器上下文投影到一个显式包装器，将 action 名称和
	/// 参数数组视图绑定到更强的用户声明契约。
	/// 这仅是编译期投影，不会创建新的运行时对象。
	/// Projects a typed action-listener context to an explicit wrapper that binds the
	/// action name and argument-array view to stronger user-declared contracts.
	/// This is a compile-time projection only and does not create a new runtime object.
	/// </summary>
	/// <typeparam name="TStore">监听器上下文提供的基础类型化 store 投影。The base typed store projection supplied by the listener context.</typeparam>
	/// <typeparam name="TActionName">调用方期望的显式 action 名称契约。The explicit action-name contract expected by the caller.</typeparam>
	/// <typeparam name="TArgs">调用方期望的显式参数数组视图契约。The explicit argument-array view contract expected by the caller.</typeparam>
	/// <param name="context">要投影的类型化 action 监听器上下文。The typed action-listener context to project.</param>
	/// <returns>覆盖相同运行时 action 上下文对象的显式投影包装器。An explicit projected wrapper over the same runtime action context object.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static ProjectedActionContext<TStore, TActionName, TArgs> ProjectActionContext<TStore, TActionName, TArgs>(StoreActionListenerContext<TStore> context)
		where TStore : class
		where TArgs : class;

	/// <summary>
	/// 仅当运行时 action 名称与调用方提供的预期字面量 action 名称匹配时，才将
	/// 类型化的 action 监听器上下文投影到显式包装器。
	/// 成功时保持相同的运行时上下文对象，否则返回 <c>null</c>。
	/// Projects a typed action-listener context to an explicit wrapper only when the
	/// runtime action name matches the expected literal action name supplied by the
	/// caller. This keeps the same runtime context object on success and returns
	/// <c>null</c> otherwise.
	/// </summary>
	/// <typeparam name="TStore">监听器上下文提供的基础类型化 store 投影。The base typed store projection supplied by the listener context.</typeparam>
	/// <typeparam name="TActionName">调用方期望的显式 action 名称契约。The explicit action-name contract expected by the caller.</typeparam>
	/// <typeparam name="TArgs">调用方期望的显式参数数组视图契约。The explicit argument-array view contract expected by the caller.</typeparam>
	/// <param name="context">要投影的类型化 action 监听器上下文。The typed action-listener context to project.</param>
	/// <param name="expectedActionName">投影成功所必须匹配的运行时 action 名称。The runtime action name that must match for the projection to succeed.</param>
	/// <returns>名称匹配时投影到更强契约的相同运行时 action 上下文；否则为 <c>null</c>。The same runtime action context projected to the stronger contract when the name matches; otherwise <c>null</c>.</returns>
	[ECMAScriptInline(TryProjectActionContextInlineTemplate)]
	public extern static ProjectedActionContext<TStore, TActionName, TArgs>? TryProjectActionContext<TStore, TActionName, TArgs>(StoreActionListenerContext<TStore> context, string expectedActionName)
		where TStore : class
		where TArgs : class;

	/// <summary>
	/// 将一个活跃的 store 实例投影到一个显式包装器，暴露基础 store 契约、
	/// 插件添加的自定义 store 属性和插件添加的自定义状态。
	/// 这仅是编译期投影，不会创建新的运行时对象。
	/// Projects a live store instance to an explicit wrapper exposing the base store
	/// contract, plugin-added custom store properties, and plugin-added custom state.
	/// This is a compile-time projection only and does not create a new runtime object.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	/// <typeparam name="TCustomState"><c>store.$state</c> 上的插件添加的自定义状态投影。The plugin-added custom state projection on <c>store.$state</c>.</typeparam>
	/// <param name="store">要投影的活跃 store 实例。The live store instance to project.</param>
	/// <returns>覆盖相同运行时 store 对象的显式投影包装器。An explicit projected wrapper over the same runtime store object.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static ProjectedStore<TStore, TCustomProperties, TCustomState> ProjectStore<TStore, TCustomProperties, TCustomState>(TStore store)
		where TStore : class
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree;

	/// <summary>
	/// 投影一个 store 定义，使插件添加的自定义属性通过其 <c>Use(...)</c>
	/// 调用面传播。
	/// 这仅是编译期投影，不会创建新的运行时对象。
	/// Projects a store definition so plugin-added custom properties propagate through
	/// its <c>Use(...)</c> call surface.
	/// This is a compile-time projection only and does not create a new runtime object.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	/// <param name="storeDefinition">要投影的 store 定义。The store definition to project.</param>
	/// <returns>覆盖相同运行时 store 定义函数对象的显式投影包装器。An explicit projected wrapper over the same runtime store-definition function object.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static ProjectedStoreDefinition<TStore, TCustomProperties> ProjectStoreDefinition<TStore, TCustomProperties>(StoreDefinition<TStore> storeDefinition)
		where TStore : class
		where TCustomProperties : Vue3.VueProps;

	/// <summary>
	/// 投影一个 store 定义，使插件添加的自定义属性和自定义状态通过其
	/// <c>Use(...)</c> 调用面传播。
	/// 这仅是编译期投影，不会创建新的运行时对象。
	/// Projects a store definition so plugin-added custom properties and custom state
	/// propagate through its <c>Use(...)</c> call surface.
	/// This is a compile-time projection only and does not create a new runtime object.
	/// </summary>
	/// <typeparam name="TStore">基础类型化 store 投影。The base typed store projection.</typeparam>
	/// <typeparam name="TCustomProperties">插件添加的自定义 store 属性投影。The plugin-added custom store properties projection.</typeparam>
	/// <typeparam name="TCustomState"><c>store.$state</c> 上的插件添加的自定义状态投影。The plugin-added custom state projection on <c>store.$state</c>.</typeparam>
	/// <param name="storeDefinition">要投影的 store 定义。The store definition to project.</param>
	/// <returns>覆盖相同运行时 store 定义函数对象的显式投影包装器。An explicit projected wrapper over the same runtime store-definition function object.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static ProjectedStoreDefinition<TStore, TCustomProperties, TCustomState> ProjectStoreDefinition<TStore, TCustomProperties, TCustomState>(StoreDefinition<TStore> storeDefinition)
		where TStore : class
		where TCustomProperties : Vue3.VueProps
		where TCustomState : PiniaStateTree;

	/// <summary>
	/// 为先前声明的 store 定义创建一个 HMR 接受处理器。
	/// Creates an HMR accept handler for a previously declared store definition.
	/// </summary>
	/// <typeparam name="TStore">store 定义返回的类型化 store 投影。The typed store projection returned by the store definition.</typeparam>
	/// <param name="initialUseStore">原始 store 定义包装器。The original store definition wrapper.</param>
	/// <param name="hot">宿主热模块对象（例如 Vite 的 <c>import.meta.hot</c>）。The host hot-module object (for example, Vite's <c>import.meta.hot</c>).</param>
	/// <returns>应接入宿主 HMR 接受流程的回调。A callback that should be wired into the host HMR accept flow.</returns>
	[Description("@#acceptHMRUpdate")]
	public extern static PiniaHotUpdateHandler AcceptHMRUpdate<TStore>(StoreDefinition<TStore> initialUseStore, IObject hot)
		where TStore : class;
}
