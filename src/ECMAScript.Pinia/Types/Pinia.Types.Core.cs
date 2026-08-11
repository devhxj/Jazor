using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	/// <summary>
	/// Pinia store 定义选项包的基础 record。
	/// Base record for Pinia store-definition option bags.
	/// </summary>
	public abstract record DefineStoreOptionsBase : Vue.VueProps;

	/// <summary>
	/// 通过 <c>PiniaPluginContext.Options</c> 提供的插件可见 store 定义选项包。
	/// Pinia 保证此处有归一化的 <c>actions</c> 包，即使 store 是通过 setup-store 形式编写的。
	/// Plugin-visible store-definition options bag supplied through
	/// <c>PiniaPluginContext.Options</c>.
	/// Pinia guarantees the normalized <c>actions</c> bag here even when the store was
	/// authored through the setup-store form.
	/// </summary>
	public record DefineStoreOptionsInPlugin : DefineStoreOptionsBase
	{
		/// <summary>
		/// 插件可见的归一化 action 声明。
		/// Normalized action declarations visible to plugins.
		/// </summary>
		[Description("@#actions")]
		public Vue.VueProps Actions { get; init; } = default!;
	}

	/// <summary>
	/// 带有强类型 option-store state 投影的插件可见 store 定义选项包。
	/// Plugin-visible store-definition options bag with a strongly typed option-store
	/// state projection.
	/// </summary>
	/// <typeparam name="TState">store 的 <c>state()</c> 工厂返回的类型化 state record。The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	public record DefineStoreOptionsInPlugin<TState> : DefineStoreOptionsInPlugin
		where TState : PiniaStateTree
	{
		/// <summary>
		/// 当 store 以 option 形式编写时的 option-store state 工厂。
		/// Setup store 在运行时可能将此保持为 null 类值。
		/// Option-store state factory when the current store was authored in option form.
		/// Setup stores may leave this null-like at runtime.
		/// </summary>
		[Description("@#state")]
		public Func<TState>? State { get; init; }

		/// <summary>
		/// 插件可见的 getter 声明包。
		/// Getter declarations bag visible to plugins.
		/// </summary>
		[Description("@#getters")]
		public Vue.VueProps? Getters { get; init; }

		/// <summary>
		/// 插件可见的可选 hydration 钩子。
		/// Optional hydration hook visible to plugins.
		/// </summary>
		[Description("@#hydrate")]
		public PiniaHydrateCallback<TState>? Hydrate { get; init; }
	}

	/// <summary>
	/// 带有强类型 state、getter 和 action 投影的插件可见 store 定义选项包。
	/// Plugin-visible store-definition options bag with strongly typed state, getter,
	/// and action projections.
	/// </summary>
	/// <typeparam name="TState">store 的 <c>state()</c> 工厂返回的类型化 state record。The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	/// <typeparam name="TGetters">插件可见的类型化 getters 包。The typed getters bag visible to plugins.</typeparam>
	/// <typeparam name="TActions">插件可见的类型化 actions 包。The typed actions bag visible to plugins.</typeparam>
	public record DefineStoreOptionsInPlugin<TState, TGetters, TActions> : DefineStoreOptionsInPlugin<TState>
		where TState : PiniaStateTree
		where TGetters : Vue.VueProps
		where TActions : Vue.VueProps
	{
		/// <summary>
		/// 插件可见的强类型 getter 声明包。
		/// Strongly typed getter declarations bag visible to plugins.
		/// </summary>
		[Description("@#getters")]
		public new TGetters? Getters { get; init; }

		/// <summary>
		/// 插件可见的强类型 action 声明包。
		/// Strongly typed action declarations bag visible to plugins.
		/// </summary>
		[Description("@#actions")]
		public new TActions Actions { get; init; } = default!;
	}

	/// <summary>
	/// Option 风格的 store 定义包。
	/// Option-style store definition bag.
	/// </summary>
	/// <typeparam name="TState">store 的 <c>state()</c> 工厂返回的类型化 state record。The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	public record DefineStoreOptions<TState> : DefineStoreOptionsBase
		where TState : PiniaStateTree
	{
		/// <summary>
		/// 为每个 store 实例返回一个全新 state 对象的工厂。
		/// Factory that returns one fresh state object per store instance.
		/// </summary>
		[Description("@#state")]
		public Func<TState> State { get; init; } = default!;

		/// <summary>
		/// Getter 声明包。当 store 暴露异构 getter 签名时，
		/// 请使用类型化的 <see cref="Vue.VueProps"/> record。
		/// Getter declarations bag. Use a typed <see cref="Vue.VueProps"/> record when
		/// the store exposes heterogeneous getter signatures.
		/// </summary>
		[Description("@#getters")]
		public Vue.VueProps? Getters { get; init; }

		/// <summary>
		/// Action 声明包。当 store 暴露异构 action 签名时，
		/// 请使用类型化的 <see cref="Vue.VueProps"/> record。
		/// Action declarations bag. Use a typed <see cref="Vue.VueProps"/> record when
		/// the store exposes heterogeneous action signatures.
		/// </summary>
		[Description("@#actions")]
		public Vue.VueProps? Actions { get; init; }

		/// <summary>
		/// 用于 SSR/客户端 hydration 边界的可选 hydration 钩子。
		/// Optional hydration hook for SSR/client hydration boundaries.
		/// </summary>
		[Description("@#hydrate")]
		public PiniaHydrateCallback<TState>? Hydrate { get; init; }
	}

	/// <summary>
	/// 提供给 <c>defineStore(id, setup, ...)</c> 的 setup-store 辅助工具。
	/// Pinia 当前暴露 <c>action(fn, name?)</c> 辅助方法，
	/// 以便 setup store 能将函数标记为受追踪的 store action。
	/// Setup-store helpers supplied to <c>defineStore(id, setup, ...)</c>.
	/// Pinia currently exposes the <c>action(fn, name?)</c> helper so setup stores can
	/// mark functions as tracked store actions.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class SetupStoreHelpers
	{
		protected SetupStoreHelpers()
		{
		}

		/// <summary>
		/// 将回调函数标记为可追踪的 store action。
		/// Marks the callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action Action(global::System.Action callback, string? name = null);

		/// <summary>
		/// 将带一个参数的回调函数标记为可追踪的 store action。
		/// Marks the single-parameter callback as a tracked store action.
		/// </summary>
		/// <typeparam name="T1">回调参数类型。The callback parameter type.</typeparam>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1> Action<T1>(global::System.Action<T1> callback, string? name = null);

		/// <summary>
		/// 将带两个参数的回调函数标记为可追踪的 store action。
		/// Marks the two-parameter callback as a tracked store action.
		/// </summary>
		/// <typeparam name="T1">第一个回调参数类型。The first callback parameter type.</typeparam>
		/// <typeparam name="T2">第二个回调参数类型。The second callback parameter type.</typeparam>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2> Action<T1, T2>(global::System.Action<T1, T2> callback, string? name = null);

		/// <summary>
		/// 将带三个参数的回调函数标记为可追踪的 store action。
		/// Marks the three-parameter callback as a tracked store action.
		/// </summary>
		/// <typeparam name="T1">第一个回调参数类型。The first callback parameter type.</typeparam>
		/// <typeparam name="T2">第二个回调参数类型。The second callback parameter type.</typeparam>
		/// <typeparam name="T3">第三个回调参数类型。The third callback parameter type.</typeparam>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3> Action<T1, T2, T3>(global::System.Action<T1, T2, T3> callback, string? name = null);

		/// <summary>
		/// 将带四个参数的回调函数标记为可追踪的 store action。
		/// Marks the four-parameter callback as a tracked store action.
		/// </summary>
		/// <typeparam name="T1">第一个回调参数类型。The first callback parameter type.</typeparam>
		/// <typeparam name="T2">第二个回调参数类型。The second callback parameter type.</typeparam>
		/// <typeparam name="T3">第三个回调参数类型。The third callback parameter type.</typeparam>
		/// <typeparam name="T4">第四个回调参数类型。The fourth callback parameter type.</typeparam>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4> Action<T1, T2, T3, T4>(global::System.Action<T1, T2, T3, T4> callback, string? name = null);

		/// <summary>
		/// 将带五个参数的回调函数标记为可追踪的 store action。
		/// Marks the five-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5> Action<T1, T2, T3, T4, T5>(global::System.Action<T1, T2, T3, T4, T5> callback, string? name = null);

		/// <summary>
		/// 将带六个参数的回调函数标记为可追踪的 store action。
		/// Marks the six-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6> Action<T1, T2, T3, T4, T5, T6>(global::System.Action<T1, T2, T3, T4, T5, T6> callback, string? name = null);

		/// <summary>
		/// 将带七个参数的回调函数标记为可追踪的 store action。
		/// Marks the seven-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7> Action<T1, T2, T3, T4, T5, T6, T7>(global::System.Action<T1, T2, T3, T4, T5, T6, T7> callback, string? name = null);

		/// <summary>
		/// 将带八个参数的回调函数标记为可追踪的 store action。
		/// Marks the eight-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8> Action<T1, T2, T3, T4, T5, T6, T7, T8>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8> callback, string? name = null);

		/// <summary>
		/// 将带九个参数的回调函数标记为可追踪的 store action。
		/// Marks the nine-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback, string? name = null);

		/// <summary>
		/// 将带十个参数的回调函数标记为可追踪的 store action。
		/// Marks the ten-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback, string? name = null);

		/// <summary>
		/// 将带十一个参数的回调函数标记为可追踪的 store action。
		/// Marks the eleven-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback, string? name = null);

		/// <summary>
		/// 将带十二个参数的回调函数标记为可追踪的 store action。
		/// Marks the twelve-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> callback, string? name = null);

		/// <summary>
		/// 将带十三个参数的回调函数标记为可追踪的 store action。
		/// Marks the thirteen-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> callback, string? name = null);

		/// <summary>
		/// 将带十四个参数的回调函数标记为可追踪的 store action。
		/// Marks the fourteen-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> callback, string? name = null);

		/// <summary>
		/// 将带十五个参数的回调函数标记为可追踪的 store action。
		/// Marks the fifteen-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> callback, string? name = null);

		/// <summary>
		/// 将带十六个参数的回调函数标记为可追踪的 store action。
		/// Marks the sixteen-parameter callback as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> callback, string? name = null);

		/// <summary>
		/// 将无参数带返回值的回调函数标记为可追踪的 store action。
		/// Marks the parameterless callback with return value as a tracked store action.
		/// </summary>
		/// <typeparam name="TResult">回调返回值类型。The callback return type.</typeparam>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<TResult> Action<TResult>(global::System.Func<TResult> callback, string? name = null);

		/// <summary>
		/// 将带一个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the single-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <typeparam name="T1">回调参数类型。The callback parameter type.</typeparam>
		/// <typeparam name="TResult">回调返回值类型。The callback return type.</typeparam>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, TResult> Action<T1, TResult>(global::System.Func<T1, TResult> callback, string? name = null);

		/// <summary>
		/// 将带两个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the two-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <typeparam name="T1">第一个回调参数类型。The first callback parameter type.</typeparam>
		/// <typeparam name="T2">第二个回调参数类型。The second callback parameter type.</typeparam>
		/// <typeparam name="TResult">回调返回值类型。The callback return type.</typeparam>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, TResult> Action<T1, T2, TResult>(global::System.Func<T1, T2, TResult> callback, string? name = null);

		/// <summary>
		/// 将带三个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the three-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, TResult> Action<T1, T2, T3, TResult>(global::System.Func<T1, T2, T3, TResult> callback, string? name = null);

		/// <summary>
		/// 将带四个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the four-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, TResult> Action<T1, T2, T3, T4, TResult>(global::System.Func<T1, T2, T3, T4, TResult> callback, string? name = null);

		/// <summary>
		/// 将带五个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the five-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, TResult> Action<T1, T2, T3, T4, T5, TResult>(global::System.Func<T1, T2, T3, T4, T5, TResult> callback, string? name = null);

		/// <summary>
		/// 将带六个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the six-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, TResult> Action<T1, T2, T3, T4, T5, T6, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, TResult> callback, string? name = null);

		/// <summary>
		/// 将带七个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the seven-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, TResult> Action<T1, T2, T3, T4, T5, T6, T7, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, TResult> callback, string? name = null);

		/// <summary>
		/// 将带八个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the eight-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> callback, string? name = null);

		/// <summary>
		/// 将带九个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the nine-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> callback, string? name = null);

		/// <summary>
		/// 将带十个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the ten-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> callback, string? name = null);

		/// <summary>
		/// 将带十一个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the eleven-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> callback, string? name = null);

		/// <summary>
		/// 将带十二个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the twelve-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> callback, string? name = null);

		/// <summary>
		/// 将带十三个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the thirteen-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> callback, string? name = null);

		/// <summary>
		/// 将带十四个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the fourteen-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> callback, string? name = null);

		/// <summary>
		/// 将带十五个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the fifteen-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> callback, string? name = null);

		/// <summary>
		/// 将带十六个参数和返回值的回调函数标记为可追踪的 store action。
		/// Marks the sixteen-parameter callback with return value as a tracked store action.
		/// </summary>
		/// <param name="callback">要包装的回调函数。The callback to wrap.</param>
		/// <param name="name">可选的 action 名称。Optional action name.</param>
		/// <returns>包装后的 action。The wrapped action.</returns>
		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> callback, string? name = null);
	}

	/// <summary>
	/// Setup 风格的 store 选项包。
	/// Pinia 当前保持此接口精简；主要的稳定字段是
	/// 插件和高级 store 编写所使用的归一化 <c>actions</c> 包。
	/// Setup-style store options bag.
	/// Pinia currently keeps this surface small; the primary stable field is the
	/// normalized <c>actions</c> bag used by plugins and advanced store authoring.
	/// </summary>
	public record DefineSetupStoreOptions : DefineStoreOptionsBase
	{
		/// <summary>
		/// 与 setup store 关联的归一化 action 声明。
		/// 这主要是高级/插件面向的契约，而非日常默认的编写路径。
		/// Normalized action declarations associated with the setup store.
		/// This is primarily an advanced/plugin-facing contract rather than the default
		/// day-to-day authoring path.
		/// </summary>
		[Description("@#actions")]
		public Vue.VueProps Actions { get; init; } = default!;
	}

	/// <summary>
	/// 强类型 Setup 风格 store 选项包。
	/// Strongly typed setup-style store options bag.
	/// </summary>
	/// <typeparam name="TActions">与 setup store 关联的类型化 action 声明。The typed action declarations associated with the setup store.</typeparam>
	public record DefineSetupStoreOptions<TActions> : DefineSetupStoreOptions
		where TActions : Vue.VueProps
	{
		/// <summary>
		/// 与 setup store 关联的强类型 action 声明。
		/// Strongly typed action declarations associated with the setup store.
		/// </summary>
		[Description("@#actions")]
		public new TActions Actions { get; init; } = default!;
	}

	/// <summary>
	/// 强类型 store-state 投影的基础 record。
	/// Base record for strongly typed store-state projections.
	/// </summary>
	public abstract record PiniaStateTree : Vue.VueProps;

	/// <summary>
	/// 对象形式 <c>$patch({ ... })</c> 负载的基础 record。
	/// Pinia 将此建模为深度部分 state 树；C# 绑定保持该契约显式，
	/// 而非假装负载是完整的 <typeparamref name="TState"/>。
	/// 具体 store 应声明专用的 patch record，其可空/可选成员
	/// 与它们打算 patch 的子集相匹配。
	/// Base record for object-form <c>$patch({ ... })</c> payloads.
	/// Pinia models this as a deep-partial state tree; the C# binding keeps that
	/// contract explicit instead of pretending the payload is a full <typeparamref name="TState"/>.
	/// Concrete stores should declare dedicated patch records with nullable/optional
	/// members matching the subset they intend to patch.
	/// </summary>
	/// <typeparam name="TState">被 patch 的 state record。The state record being patched.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract record PiniaStatePatch<TState> : Vue.VueProps
		where TState : PiniaStateTree;

	/// <summary>
	/// 用于 action-listener 参数和 action 结果的 unknown 类值桥接。
	/// 这使公共接口不直接使用原始 <see cref="object"/>，
	/// 同时仍允许典型的 JavaScript 负载形态。
	/// Unknown-like value bridge used by action-listener arguments and action results.
	/// This keeps the public surface free of raw <see cref="object"/> while still
	/// allowing typical JavaScript payload shapes.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class PiniaValue
	{
		private PiniaValue()
		{
		}

		/// <summary>
		/// 从字符串隐式转换为 Pinia 值。
		/// Implicitly converts a string to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的字符串值。The string value to convert.</param>
		public extern static implicit operator PiniaValue(string value);

		/// <summary>
		/// 从布尔值隐式转换为 Pinia 值。
		/// Implicitly converts a bool to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的布尔值。The bool value to convert.</param>
		public extern static implicit operator PiniaValue(bool value);

		/// <summary>
		/// 从 Number 隐式转换为 Pinia 值。
		/// Implicitly converts a Number to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 Number 值。The Number value to convert.</param>
		public extern static implicit operator PiniaValue(Number value);

		/// <summary>
		/// 从 BigInt 隐式转换为 Pinia 值。
		/// Implicitly converts a BigInt to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 BigInt 值。The BigInt value to convert.</param>
		public extern static implicit operator PiniaValue(BigInt value);

		/// <summary>
		/// 从字符隐式转换为 Pinia 值。
		/// Implicitly converts a char to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的字符值。The char value to convert.</param>
		public extern static implicit operator PiniaValue(char value);

		/// <summary>
		/// 从 double 隐式转换为 Pinia 值。
		/// Implicitly converts a double to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的双精度浮点值。The double value to convert.</param>
		public extern static implicit operator PiniaValue(double value);

		/// <summary>
		/// 从 float 隐式转换为 Pinia 值。
		/// Implicitly converts a float to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的单精度浮点值。The float value to convert.</param>
		public extern static implicit operator PiniaValue(float value);

		/// <summary>
		/// 从 int 隐式转换为 Pinia 值。
		/// Implicitly converts an int to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 32 位整数值。The int value to convert.</param>
		public extern static implicit operator PiniaValue(int value);

		/// <summary>
		/// 从 long 隐式转换为 Pinia 值。
		/// Implicitly converts a long to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 64 位整数值。The long value to convert.</param>
		public extern static implicit operator PiniaValue(long value);

		/// <summary>
		/// 从 short 隐式转换为 Pinia 值。
		/// Implicitly converts a short to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 16 位整数值。The short value to convert.</param>
		public extern static implicit operator PiniaValue(short value);

		/// <summary>
		/// 从 ushort 隐式转换为 Pinia 值。
		/// Implicitly converts a ushort to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的无符号 16 位整数值。The ushort value to convert.</param>
		public extern static implicit operator PiniaValue(ushort value);

		/// <summary>
		/// 从 byte 隐式转换为 Pinia 值。
		/// Implicitly converts a byte to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的无符号 8 位整数值。The byte value to convert.</param>
		public extern static implicit operator PiniaValue(byte value);

		/// <summary>
		/// 从 sbyte 隐式转换为 Pinia 值。
		/// Implicitly converts an sbyte to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的有符号 8 位整数值。The sbyte value to convert.</param>
		public extern static implicit operator PiniaValue(sbyte value);

		/// <summary>
		/// 从 uint 隐式转换为 Pinia 值。
		/// Implicitly converts a uint to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的无符号 32 位整数值。The uint value to convert.</param>
		public extern static implicit operator PiniaValue(uint value);

		/// <summary>
		/// 从 ulong 隐式转换为 Pinia 值。
		/// Implicitly converts a ulong to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的无符号 64 位整数值。The ulong value to convert.</param>
		public extern static implicit operator PiniaValue(ulong value);

		/// <summary>
		/// 从 decimal 隐式转换为 Pinia 值。
		/// Implicitly converts a decimal to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 decimal 值。The decimal value to convert.</param>
		public extern static implicit operator PiniaValue(decimal value);

		/// <summary>
		/// 从 Error 隐式转换为 Pinia 值。
		/// Implicitly converts an Error to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 Error 值。The Error value to convert.</param>
		public extern static implicit operator PiniaValue(Error value);

		/// <summary>
		/// 从 VueProps 隐式转换为 Pinia 值。
		/// Implicitly converts a VueProps to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 VueProps 值。The VueProps value to convert.</param>
		public extern static implicit operator PiniaValue(Vue.VueProps value);

		/// <summary>
		/// 从 PiniaValue 数组隐式转换为 Pinia 值。
		/// Implicitly converts a PiniaValue array to a Pinia value.
		/// </summary>
		/// <param name="value">要转换的 PiniaValue 数组。The PiniaValue array to convert.</param>
		public extern static implicit operator PiniaValue(PiniaValue[] value);
	}

	/// <summary>
	/// Pinia 订阅报告的变更类型。
	/// Mutation kind reported by Pinia subscriptions.
	/// </summary>
	[String]
	public enum MutationType
	{
		/// <summary>
		/// 直接 state 赋值。
		/// Direct state assignment.
		/// </summary>
		[Description("@#direct")]
		Direct,

		/// <summary>
		/// <c>$patch({ ... })</c> 对象 patch。
		/// <c>$patch({ ... })</c> object patch.
		/// </summary>
		[Description("@#patch object")]
		PatchObject,

		/// <summary>
		/// <c>$patch((state) =&gt; ...)</c> 函数 patch。
		/// <c>$patch((state) =&gt; ...)</c> function patch.
		/// </summary>
		[Description("@#patch function")]
		PatchFunction
	}

	/// <summary>
	/// <c>$subscribe()</c> 的选项。
	/// Options for <c>$subscribe()</c>.
	/// </summary>
	public record SubscribeOptions : Vue.VueWatchOptions
	{
		/// <summary>
		/// 即使当前没有组件正在使用该 store，也保持订阅活跃。
		/// Keep the subscription alive even when no component is currently using the store.
		/// </summary>
		[Description("@#detached")]
		public bool? Detached { get; init; }
	}

	/// <summary>
	/// 用户自定义 <c>storeToRefs()</c> 投影的类型化基类。
	/// Typed base for user-defined <c>storeToRefs()</c> projections.
	/// </summary>
	/// <typeparam name="TStore">正在转换为 refs 的 store 契约。The store contract being converted to refs.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class StoreRefs<TStore> : Vue.VueRefs<TStore>
		where TStore : class
	{
		protected StoreRefs()
		{
		}
	}
}
