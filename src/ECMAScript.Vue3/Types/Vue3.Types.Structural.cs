using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue 结构化值、枚举值域和通用对象形状定义。</summary>
/// <remarks>这些类型优先按 structural lowering 处理，不自动引入 nominal runtime declaration。</remarks>
public static partial class Vue3
{
	/// <summary>
	/// Vue 内置 <c>Transition</c> 和 <c>TransitionGroup</c> 组件使用的过渡实现类型。
	/// Transition implementation type used by Vue's <c>Transition</c> and
	/// <c>TransitionGroup</c> built-in components.
	/// </summary>
	[String]
	public enum VueTransitionType
	{
		[Description("@#transition")]
		Transition,

		[Description("@#animation")]
		Animation
	}

	/// <summary>
	/// <c>Transition</c> 内置组件的过渡排序模式。
	/// Transition sequencing mode for the <c>Transition</c> built-in component.
	/// </summary>
	[String]
	public enum VueTransitionMode
	{
		[Description("@#in-out")]
		InOut,

		[Description("@#out-in")]
		OutIn
	}

	/// <summary>
	/// 进入和离开阶段的对象形式过渡持续时间。
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
	/// 接收过渡元素的生命周期钩子。
	/// Transition lifecycle hook receiving the transitioning element.
	/// </summary>
	/// <param name="element">当前正在进入或离开的元素。</param>
	public delegate void VueTransitionHook(Element element);

	/// <summary>
	/// 可显式完成异步过渡的生命周期钩子。
	/// Transition lifecycle hook that can explicitly complete async transitions.
	/// </summary>
	/// <param name="element">当前正在进入或离开的元素。</param>
	/// <param name="done">当过渡阶段完成时调用的回调。</param>
	public delegate void VueTransitionDoneHook(Element element, Action done);

	/// <summary>
	/// Vue 内置 <c>Transition</c> 组件的属性。
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
		public VueTransitionDurationValue? Duration { get; init; }

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
	/// Vue 内置 <c>TransitionGroup</c> 组件的属性。
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
		public VueTransitionDurationValue? Duration { get; init; }

		[Description("@#appear")]
		public bool? Appear { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>KeepAlive</c> 组件的属性。
	/// Props for Vue's built-in <c>KeepAlive</c> component.
	/// </summary>
	public record VueKeepAliveProps : VueProps
	{
		[Description("@#include")]
		public VueKeepAliveMatch? Include { get; init; }

		[Description("@#exclude")]
		public VueKeepAliveMatch? Exclude { get; init; }

		[Description("@#max")]
		public VueIntStringValue? Max { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>Teleport</c> 组件的属性。
	/// Props for Vue's built-in <c>Teleport</c> component.
	/// </summary>
	public record VueTeleportProps : VueProps
	{
		[Description("@#to")]
		public VueTeleportTarget? To { get; init; }

		[Description("@#disabled")]
		public bool? Disabled { get; init; }

		[Description("@#defer")]
		public bool? Defer { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>Suspense</c> 组件的属性。
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
	/// 接受单个默认插槽的 Vue 内置组件使用的插槽契约。
	/// Slots accepted by Vue built-ins whose child content is the default slot.
	/// </summary>
	/// <remarks>
	/// <c>default</c> 是 Vue runtime ABI，不由 RazorVue 根据成员名或组件类型推断。
	/// This explicit metadata keeps direct <c>h(..., child)</c> lowering deterministic.
	/// </remarks>
	public record VueDefaultSlots : VueSlots
	{
		[Description("@#default")]
		public VueSlotCallback? Default { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>Suspense</c> 组件接受的插槽。
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
	/// 通过 <c>app.config</c> 配置的应用级未捕获错误处理程序。Vue 的错误值为 unknown 类型，因此使用 <see cref="VueValue"/> 而非在公共 Vue 界面上暴露 <c>object</c>。
	/// Application-level uncaught error handler configured through <c>app.config</c>.
	/// Vue's error value is unknown-like, so this uses <see cref="VueValue"/> instead
	/// of exposing <c>object</c> on the public Vue surface.
	/// </summary>
	public delegate void VueAppErrorHandler(VueValue? error, VueComponentPublicInstance? instance, string info);

	/// <summary>
	/// 通过 <c>app.config</c> 配置的应用级运行时警告处理程序。
	/// Application-level runtime warning handler configured through <c>app.config</c>.
	/// </summary>
	public delegate void VueAppWarnHandler(string message, VueComponentPublicInstance? instance, string trace);

	/// <summary>
	/// 将标签标记为原生自定义元素的运行时编译器谓词。
	/// Runtime compiler predicate that marks tags as native custom elements.
	/// </summary>
	public delegate bool VueIsCustomElementCallback(string tag);

	/// <summary>
	/// 自定义 Options API 选项键的合并函数。
	/// Merge function for custom Options API option keys.
	/// </summary>
	public delegate VueValue? VueOptionMergeFunction(VueValue? parent, VueValue? child);

	/// <summary>
	/// 运行时编译器空白处理模式。
	/// Runtime compiler whitespace handling mode.
	/// </summary>
	[String]
	public enum VueCompilerWhitespace
	{
		[Description("@#condense")]
		Condense,

		[Description("@#preserve")]
		Preserve
	}

	/// <summary>
	/// 每个组件实例上可用的应用级全局属性包。
	/// Bag of app-level global properties available on every component instance.
	/// </summary>
	public abstract class VueGlobalProperties
	{
		protected VueGlobalProperties()
		{
		}

		/// <summary>
		/// 通过最终运行时键获取或设置全局属性。
		/// Gets or sets a global property by its final runtime key.
		/// </summary>
		public extern VueValue? this[string key] { get; set; }
	}

	/// <summary>
	/// 应用级自定义选项合并策略包。
	/// Bag of app-level custom option merge strategies.
	/// </summary>
	public abstract class VueOptionMergeStrategies
	{
		protected VueOptionMergeStrategies()
		{
		}

		/// <summary>
		/// 通过自定义选项名获取或设置合并策略。
		/// Gets or sets a merge strategy by custom option name.
		/// </summary>
		public extern VueOptionMergeFunction? this[string key] { get; set; }
	}

	/// <summary>
	/// 通过 <c>app.config.compilerOptions</c> 暴露的运行时编译器选项。仅影响使用 Vue 浏览器内模板编译器的应用。
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
	/// 由 <c>app.config</c> 暴露的 Vue 应用配置。
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
	/// 只读响应式引用。仅 <c>value</c> getter 可用；不允许写入。通常由 <see cref="Computed{T}"/> 或 <c>readonly()</c> 创建。
	/// A readonly reactive reference. Only the <c>value</c> getter is available; writes
	/// are not permitted. Typically created by <see cref="Computed{T}"/> or <c>readonly()</c>.
	/// </summary>
	/// <typeparam name="T">包装值的类型。</typeparam>
	public class VueReadonlyRef<T>
	{
		/// <summary>
		/// 获取当前值。读取会被追踪为响应式依赖。
		/// Gets the current value. Reads are tracked as reactive dependencies.
		/// </summary>
		[Description("@#value")]
		public extern T Value { get; }
	}

	/// <summary>
	/// Vue <c>computed(getter)</c> 产生的只读计算引用。与更广泛的 <see cref="VueReadonlyRef{T}"/> 契约保持区分，以便库界面可以保留专门保证计算语义的官方 API。
	/// A readonly computed ref produced by Vue's <c>computed(getter)</c>. This remains
	/// distinct from the broader <see cref="VueReadonlyRef{T}"/> contract so library
	/// surfaces can preserve official APIs that specifically guarantee computed semantics.
	/// </summary>
	/// <typeparam name="T">计算属性值类型。</typeparam>
	public abstract class VueComputedRef<T> : VueReadonlyRef<T>
	{
		protected VueComputedRef()
		{
		}
	}

	/// <summary>
	/// Vue <c>computed({ get, set })</c> 重载产生的可写计算引用。与通用可写引用保持区分，以便高层级库可以编码官方的可写计算契约，而不将其折叠为 <see cref="IVueRef{T}"/>。
	/// A writable computed ref produced by Vue's <c>computed({ get, set })</c> overload.
	/// This remains distinct from generic writable refs so higher-level libraries can
	/// encode official writable-computed contracts without collapsing them into
	/// <see cref="IVueRef{T}"/>.
	/// </summary>
	/// <typeparam name="T">计算属性值类型。</typeparam>
	public abstract class VueWritableComputedRef<T> : IVueRef<T>
	{
		protected VueWritableComputedRef()
		{
		}

		/// <summary>
		/// 获取或设置当前计算值。
		/// Gets or sets the current computed value.
		/// </summary>
		[Description("@#value")]
		public extern T Value { get; set; }
	}

	/// <summary>
	/// Vue <c>shallowRef()</c> 产生的浅层引用。外部 <c>value</c> 槽位是响应式的，而嵌套对象成员不会被 Vue 递归转换为深层响应式代理。
	/// A shallow ref produced by Vue's <c>shallowRef()</c>. The outer <c>value</c> slot
	/// is reactive while nested object members are not recursively converted to deep
	/// reactive proxies by Vue.
	/// </summary>
	/// <typeparam name="T">包装值的类型。</typeparam>
	public abstract class VueShallowRef<T> : IVueRef<T>
	{
		protected VueShallowRef()
		{
		}

		/// <summary>
		/// 获取或设置包装的浅层响应式值。
		/// Gets or sets the wrapped shallow reactive value.
		/// </summary>
		[Description("@#value")]
		public extern T Value { get; set; }
	}

	/// <summary>
	/// <c>toRefs()</c> 返回的非类型化引用对象。键为最终运行时属性名，值为这些属性的关联引用。
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
		/// 通过运行时属性名读取关联引用。
		/// Reads a linked ref by runtime property name.
		/// </summary>
		/// <param name="key">源对象的最终运行时属性名。</param>
		/// <returns>存在时返回关联引用；否则为 <c>null</c> / <c>undefined</c>。</returns>
		public extern IVueRef<VueValue>? this[string key] { get; }
	}

	/// <summary>
	/// 用户定义 <c>toRefs()</c> 投影的类型化基类。继承此类型并声明 <c>IVueRef&lt;T&gt;</c> 属性，以获得 Vue 返回的引用对象上的 C# IntelliSense。
	/// Typed base for user-defined <c>toRefs()</c> projections. Inherit from this type
	/// and declare <c>IVueRef&lt;T&gt;</c> properties to get C# IntelliSense over the
	/// refs object returned by Vue.
	/// </summary>
	/// <typeparam name="TSource">源响应式对象契约。</typeparam>
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
	/// Vue 侦听器回调的刷新时机。
	/// Flush timing for Vue watcher callbacks.
	/// </summary>
	[String]
	public enum VueWatchFlush
	{
		/// <summary>
		/// 在组件渲染之前运行。这是 Vue 的默认侦听器刷新时机。
		/// Run before component rendering. This is Vue's default watcher flush timing.
		/// </summary>
		[Description("@#pre")]
		Pre,

		/// <summary>
		/// 在组件渲染刷新之后运行。
		/// Run after component rendering has flushed.
		/// </summary>
		[Description("@#post")]
		Post,

		/// <summary>
		/// 在依赖变更时同步运行。
		/// Run synchronously when a dependency changes.
		/// </summary>
		[Description("@#sync")]
		Sync
	}

	/// <summary>
	/// 传递给侦听器调试钩子的响应式调试器事件操作类型。
	/// Reactivity debugger event operation kind supplied to watcher debug hooks.
	/// </summary>
	[String]
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
	/// 传递给 <c>onTrack</c> 和 <c>onTrigger</c> 侦听器选项的调试信息。运行时值为 unknown 类型的 Vue 内部对象，因此承载值的成员使用 <see cref="VueValue"/> 而非 <c>object</c>。
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

}
