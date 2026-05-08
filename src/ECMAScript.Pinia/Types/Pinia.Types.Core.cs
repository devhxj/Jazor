using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	/// <summary>
	/// Pinia store 定义选项包的基础 record。
	/// Base record for Pinia store-definition option bags.
	/// </summary>
	public abstract record DefineStoreOptionsBase : Vue3.VueProps;

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
		public Vue3.VueProps Actions { get; init; } = default!;
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
		public Vue3.VueProps? Getters { get; init; }

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
		where TGetters : Vue3.VueProps
		where TActions : Vue3.VueProps
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
		/// 请使用类型化的 <see cref="Vue3.VueProps"/> record。
		/// Getter declarations bag. Use a typed <see cref="Vue3.VueProps"/> record when
		/// the store exposes heterogeneous getter signatures.
		/// </summary>
		[Description("@#getters")]
		public Vue3.VueProps? Getters { get; init; }

		/// <summary>
		/// Action 声明包。当 store 暴露异构 action 签名时，
		/// 请使用类型化的 <see cref="Vue3.VueProps"/> record。
		/// Action declarations bag. Use a typed <see cref="Vue3.VueProps"/> record when
		/// the store exposes heterogeneous action signatures.
		/// </summary>
		[Description("@#actions")]
		public Vue3.VueProps? Actions { get; init; }

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

		[Description("@#action")]
		public extern global::System.Action Action(global::System.Action callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1> Action<T1>(global::System.Action<T1> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2> Action<T1, T2>(global::System.Action<T1, T2> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3> Action<T1, T2, T3>(global::System.Action<T1, T2, T3> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4> Action<T1, T2, T3, T4>(global::System.Action<T1, T2, T3, T4> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5> Action<T1, T2, T3, T4, T5>(global::System.Action<T1, T2, T3, T4, T5> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6> Action<T1, T2, T3, T4, T5, T6>(global::System.Action<T1, T2, T3, T4, T5, T6> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7> Action<T1, T2, T3, T4, T5, T6, T7>(global::System.Action<T1, T2, T3, T4, T5, T6, T7> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8> Action<T1, T2, T3, T4, T5, T6, T7, T8>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(global::System.Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<TResult> Action<TResult>(global::System.Func<TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, TResult> Action<T1, TResult>(global::System.Func<T1, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, TResult> Action<T1, T2, TResult>(global::System.Func<T1, T2, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, TResult> Action<T1, T2, T3, TResult>(global::System.Func<T1, T2, T3, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, TResult> Action<T1, T2, T3, T4, TResult>(global::System.Func<T1, T2, T3, T4, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, TResult> Action<T1, T2, T3, T4, T5, TResult>(global::System.Func<T1, T2, T3, T4, T5, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, TResult> Action<T1, T2, T3, T4, T5, T6, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, TResult> Action<T1, T2, T3, T4, T5, T6, T7, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> callback, string? name = null);

		[Description("@#action")]
		public extern global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(global::System.Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> callback, string? name = null);

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
		public Vue3.VueProps Actions { get; init; } = default!;
	}

	/// <summary>
	/// 强类型 Setup 风格 store 选项包。
	/// Strongly typed setup-style store options bag.
	/// </summary>
	/// <typeparam name="TActions">与 setup store 关联的类型化 action 声明。The typed action declarations associated with the setup store.</typeparam>
	public record DefineSetupStoreOptions<TActions> : DefineSetupStoreOptions
		where TActions : Vue3.VueProps
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
	public abstract record PiniaStateTree : Vue3.VueProps;

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
	public abstract record PiniaStatePatch<TState> : Vue3.VueProps
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

		public extern static implicit operator PiniaValue(string value);

		public extern static implicit operator PiniaValue(bool value);

		public extern static implicit operator PiniaValue(Number value);

		public extern static implicit operator PiniaValue(BigInt value);

		public extern static implicit operator PiniaValue(char value);

		public extern static implicit operator PiniaValue(double value);

		public extern static implicit operator PiniaValue(float value);

		public extern static implicit operator PiniaValue(int value);

		public extern static implicit operator PiniaValue(long value);

		public extern static implicit operator PiniaValue(short value);

		public extern static implicit operator PiniaValue(ushort value);

		public extern static implicit operator PiniaValue(byte value);

		public extern static implicit operator PiniaValue(sbyte value);

		public extern static implicit operator PiniaValue(uint value);

		public extern static implicit operator PiniaValue(ulong value);

		public extern static implicit operator PiniaValue(decimal value);

		public extern static implicit operator PiniaValue(Error value);

		public extern static implicit operator PiniaValue(Vue3.VueProps value);

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
	public record SubscribeOptions : Vue3.VueWatchOptions
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
	public abstract class StoreRefs<TStore> : Vue3.VueRefs<TStore>
		where TStore : class
	{
		protected StoreRefs()
		{
		}
	}
}
