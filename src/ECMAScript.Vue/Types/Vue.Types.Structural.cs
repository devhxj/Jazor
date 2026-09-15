using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue 结构化值、枚举值域和通用对象形状定义。</summary>
/// <remarks>这些类型优先按 structural lowering 处理，不自动引入 nominal runtime declaration。</remarks>
public static partial class Vue
{
	/// <summary>
	/// Vue 内置 <c>Transition</c> 和 <c>TransitionGroup</c> 组件使用的过渡实现类型。
	/// Transition implementation type used by Vue's <c>Transition</c> and
	/// <c>TransitionGroup</c> built-in components.
	/// </summary>
	[String]
	public enum VueTransitionType
	{
		/// <summary>
		/// 监听 CSS transitionend 判断过渡结束。
		/// </summary>
		[Description("@#transition")]
		Transition,

		/// <summary>
		/// 监听 CSS animationend 判断过渡结束。
		/// </summary>
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
		/// <summary>
		/// 先完成新元素的进入过渡，再开始旧元素的离开过渡。
		/// </summary>
		[Description("@#in-out")]
		InOut,

		/// <summary>
		/// 先完成旧元素的离开过渡，再开始新元素的进入过渡。
		/// </summary>
		[Description("@#out-in")]
		OutIn
	}

	/// <summary>
	/// 进入和离开阶段的对象形式过渡持续时间。
	/// Object-form transition duration for entering and leaving phases.
	/// </summary>
	public record VueTransitionDuration : VueProps
	{
		/// <summary>
		/// 进入过渡的显式持续时间，单位为毫秒。用于覆盖自动结束事件检测。
		/// </summary>
		[Description("@#enter")]
		public Number? Enter { get; init; }

		/// <summary>
		/// 离开过渡的显式持续时间，单位为毫秒。用于覆盖自动结束事件检测。
		/// </summary>
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
		/// <summary>
		/// Used to automatically generate transition CSS class names.
		/// e.g. `name: 'fade'` will auto expand to `.fade-enter`,
		/// `.fade-enter-active`, etc.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Whether to apply CSS transition classes.
		/// Default: true
		/// </summary>
		[Description("@#css")]
		public bool? Css { get; init; }

		/// <summary>
		/// Specifies the type of transition events to wait for to
		/// determine transition end timing.
		/// Default behavior is auto detecting the type that has
		/// longer duration.
		/// </summary>
		[Description("@#type")]
		public VueTransitionType? Type { get; init; }

		/// <summary>
		/// Specifies explicit durations of the transition.
		/// Default behavior is wait for the first `transitionend`
		/// or `animationend` event on the root transition element.
		/// </summary>
		[Description("@#duration")]
		public VueTransitionDurationValue? Duration { get; init; }

		/// <summary>
		/// Controls the timing sequence of leaving/entering transitions.
		/// Default behavior is simultaneous.
		/// </summary>
		[Description("@#mode")]
		public VueTransitionMode? Mode { get; init; }

		/// <summary>
		/// Whether to apply transition on initial render.
		/// Default: false
		/// </summary>
		[Description("@#appear")]
		public bool? Appear { get; init; }

		/// <summary>
		/// Props for customizing transition classes.
		/// Use kebab-case in templates, e.g. enter-from-class=&quot;xxx&quot;
		/// </summary>
		[Description("@#enterFromClass")]
		public string? EnterFromClass { get; init; }

		/// <summary>
		/// 覆盖进入过渡整个持续阶段的 CSS 类；通常在此定义过渡时长和缓动。
		/// </summary>
		[Description("@#enterActiveClass")]
		public string? EnterActiveClass { get; init; }

		/// <summary>
		/// 覆盖进入过渡的结束状态 CSS 类。
		/// </summary>
		[Description("@#enterToClass")]
		public string? EnterToClass { get; init; }

		/// <summary>
		/// 覆盖首次显示过渡的起始状态 CSS 类。
		/// </summary>
		[Description("@#appearFromClass")]
		public string? AppearFromClass { get; init; }

		/// <summary>
		/// 覆盖首次显示过渡整个持续阶段的 CSS 类；通常在此定义过渡时长和缓动。
		/// </summary>
		[Description("@#appearActiveClass")]
		public string? AppearActiveClass { get; init; }

		/// <summary>
		/// 覆盖首次显示过渡的结束状态 CSS 类。
		/// </summary>
		[Description("@#appearToClass")]
		public string? AppearToClass { get; init; }

		/// <summary>
		/// 覆盖离开过渡的起始状态 CSS 类。
		/// </summary>
		[Description("@#leaveFromClass")]
		public string? LeaveFromClass { get; init; }

		/// <summary>
		/// 覆盖离开过渡整个持续阶段的 CSS 类；通常在此定义过渡时长和缓动。
		/// </summary>
		[Description("@#leaveActiveClass")]
		public string? LeaveActiveClass { get; init; }

		/// <summary>
		/// 覆盖离开过渡的结束状态 CSS 类。
		/// </summary>
		[Description("@#leaveToClass")]
		public string? LeaveToClass { get; init; }

		/// <summary>
		/// 在进入过渡开始前调用，可准备元素的初始状态。
		/// </summary>
		[Description("@#onBeforeEnter")]
		public VueTransitionHook? OnBeforeEnter { get; init; }

		/// <summary>
		/// 执行进入过渡的 JavaScript 钩子；异步动画完成时调用 done。
		/// </summary>
		[Description("@#onEnter")]
		public VueTransitionDoneHook? OnEnter { get; init; }

		/// <summary>
		/// 在进入过渡完成后调用。
		/// </summary>
		[Description("@#onAfterEnter")]
		public VueTransitionHook? OnAfterEnter { get; init; }

		/// <summary>
		/// 在进入过渡被取消时调用，可清理动画状态。
		/// </summary>
		[Description("@#onEnterCancelled")]
		public VueTransitionHook? OnEnterCancelled { get; init; }

		/// <summary>
		/// 在离开过渡开始前调用，可准备元素的初始状态。
		/// </summary>
		[Description("@#onBeforeLeave")]
		public VueTransitionHook? OnBeforeLeave { get; init; }

		/// <summary>
		/// 执行离开过渡的 JavaScript 钩子；异步动画完成时调用 done。
		/// </summary>
		[Description("@#onLeave")]
		public VueTransitionDoneHook? OnLeave { get; init; }

		/// <summary>
		/// 在离开过渡完成后调用。
		/// </summary>
		[Description("@#onAfterLeave")]
		public VueTransitionHook? OnAfterLeave { get; init; }

		/// <summary>
		/// 在离开过渡被取消时调用，可清理动画状态。
		/// </summary>
		[Description("@#onLeaveCancelled")]
		public VueTransitionHook? OnLeaveCancelled { get; init; }

		/// <summary>
		/// 在首次显示过渡开始前调用，可准备元素的初始状态。
		/// </summary>
		[Description("@#onBeforeAppear")]
		public VueTransitionHook? OnBeforeAppear { get; init; }

		/// <summary>
		/// 执行首次显示过渡的 JavaScript 钩子；异步动画完成时调用 done。
		/// </summary>
		[Description("@#onAppear")]
		public VueTransitionDoneHook? OnAppear { get; init; }

		/// <summary>
		/// 在首次显示过渡完成后调用。
		/// </summary>
		[Description("@#onAfterAppear")]
		public VueTransitionHook? OnAfterAppear { get; init; }

		/// <summary>
		/// 在首次显示过渡被取消时调用，可清理动画状态。
		/// </summary>
		[Description("@#onAppearCancelled")]
		public VueTransitionHook? OnAppearCancelled { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>TransitionGroup</c> 组件的属性。
	/// Props for Vue's built-in <c>TransitionGroup</c> component.
	/// </summary>
	public record VueTransitionGroupProps : VueProps
	{
		/// <summary>
		/// Used to automatically generate transition CSS class names.
		/// e.g. `name: 'fade'` will auto expand to `.fade-enter`,
		/// `.fade-enter-active`, etc.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// If not defined, renders as a fragment.
		/// </summary>
		[Description("@#tag")]
		public string? Tag { get; init; }

		/// <summary>
		/// For customizing the CSS class applied during move transitions.
		/// Use kebab-case in templates, e.g. move-class=&quot;xxx&quot;
		/// </summary>
		[Description("@#moveClass")]
		public string? MoveClass { get; init; }

		/// <summary>
		/// Whether to apply CSS transition classes.
		/// Default: true
		/// </summary>
		[Description("@#css")]
		public bool? Css { get; init; }

		/// <summary>
		/// Specifies the type of transition events to wait for to
		/// determine transition end timing.
		/// Default behavior is auto detecting the type that has
		/// longer duration.
		/// </summary>
		[Description("@#type")]
		public VueTransitionType? Type { get; init; }

		/// <summary>
		/// Specifies explicit durations of the transition.
		/// Default behavior is wait for the first `transitionend`
		/// or `animationend` event on the root transition element.
		/// </summary>
		[Description("@#duration")]
		public VueTransitionDurationValue? Duration { get; init; }

		/// <summary>
		/// Whether to apply transition on initial render.
		/// Default: false
		/// </summary>
		[Description("@#appear")]
		public bool? Appear { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>KeepAlive</c> 组件的属性。
	/// Props for Vue's built-in <c>KeepAlive</c> component.
	/// </summary>
	public record VueKeepAliveProps : VueProps
	{
		/// <summary>
		/// If specified, only components with names matched by
		/// `include` will be cached.
		/// </summary>
		[Description("@#include")]
		public VueKeepAliveMatch? Include { get; init; }

		/// <summary>
		/// Any component with a name matched by `exclude` will
		/// not be cached.
		/// </summary>
		[Description("@#exclude")]
		public VueKeepAliveMatch? Exclude { get; init; }

		/// <summary>
		/// The maximum number of component instances to cache.
		/// </summary>
		[Description("@#max")]
		public VueIntStringValue? Max { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>Teleport</c> 组件的属性。
	/// Props for Vue's built-in <c>Teleport</c> component.
	/// </summary>
	public record VueTeleportProps : VueProps
	{
		/// <summary>
		/// Required. Specify target container.
		/// Can either be a selector or an actual element.
		/// </summary>
		[Description("@#to")]
		public VueTeleportTarget? To { get; init; }

		/// <summary>
		/// When `true`, the content will remain in its original
		/// location instead of moved into the target container.
		/// Can be changed dynamically.
		/// </summary>
		[Description("@#disabled")]
		public bool? Disabled { get; init; }

		/// <summary>
		/// When `true`, the Teleport will defer until other
		/// parts of the application have been mounted before
		/// resolving its target. (3.5+)
		/// </summary>
		[Description("@#defer")]
		public bool? Defer { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>Suspense</c> 组件的属性。
	/// Props for Vue's built-in <c>Suspense</c> component.
	/// </summary>
	public record VueSuspenseProps : VueProps
	{
		/// <summary>
		/// Switch to fallback content if it takes longer than `timeout` milliseconds to render the new default content.
		/// A `timeout` value of `0` will cause the fallback content to be displayed immediately when default content is replaced.
		/// </summary>
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		/// <summary>
		/// 进入等待异步依赖的 pending 状态时调用。
		/// </summary>
		[Description("@#onPending")]
		public Action? OnPending { get; init; }

		/// <summary>
		/// 所有待处理异步依赖完成并切换到默认插槽内容后调用。
		/// </summary>
		[Description("@#onResolve")]
		public Action? OnResolve { get; init; }

		/// <summary>
		/// 实际显示 fallback 插槽内容时调用。
		/// </summary>
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
		/// <summary>
		/// 默认插槽的渲染函数；返回要插入组件默认内容区域的子节点。
		/// </summary>
		[Description("@#default")]
		public VueSlotCallback? Default { get; init; }
	}

	/// <summary>
	/// Vue 内置 <c>Suspense</c> 组件接受的插槽。
	/// Slots accepted by Vue's built-in <c>Suspense</c> component.
	/// </summary>
	public record VueSuspenseSlots : VueSlots
	{
		/// <summary>
		/// 包含异步组件或 async setup 依赖的主要内容；依赖完成后显示。
		/// </summary>
		[Description("@#default")]
		public VueSlotCallback? Default { get; init; }

		/// <summary>
		/// 主要内容等待异步依赖时显示的占位内容。
		/// </summary>
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
		/// <summary>
		/// 压缩连续空白，并移除符合编译器规则的元素间空白；这是 Vue 默认策略。
		/// </summary>
		[Description("@#condense")]
		Condense,

		/// <summary>
		/// 保留模板中可保留的空白文本，适用于空白影响布局或文本呈现的场景。
		/// </summary>
		[Description("@#preserve")]
		Preserve
	}

	/// <summary>
	/// 每个组件实例上可用的应用级全局属性包。
	/// Bag of app-level global properties available on every component instance.
	/// </summary>
	public abstract class VueGlobalProperties
	{
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
		protected VueAppCompilerOptions()
		{
		}

		/// <summary>
		/// 判断标签是否应作为原生自定义元素处理；返回 true 时跳过 Vue 组件解析。
		/// </summary>
		[Description("@#isCustomElement")]
		public extern VueIsCustomElementCallback? IsCustomElement { get; set; }

		/// <summary>
		/// 模板空白处理策略；condense 压缩连续空白，preserve 保留可保留的空白。
		/// </summary>
		[Description("@#whitespace")]
		public extern VueCompilerWhitespace? Whitespace { get; set; }

		/// <summary>
		/// 模板文本插值的开始、结束分隔符；数组必须包含两个字符串，例如 [&quot;[[&quot;, &quot;]]&quot; ]。
		/// </summary>
		[Description("@#delimiters")]
		public extern string[]? Delimiters { get; set; }

		/// <summary>
		/// 是否在生产模板编译结果中保留 HTML 注释；仅对包含运行时模板编译器的 Vue 构建生效。
		/// </summary>
		[Description("@#comments")]
		public extern bool Comments { get; set; }
	}

	/// <summary>
	/// 由 <c>app.config</c> 暴露的 Vue 应用配置。
	/// Vue application configuration exposed by <c>app.config</c>.
	/// </summary>
	public abstract class VueAppConfig
	{
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
		protected VueAppConfig()
		{
		}

		/// <summary>
		/// 捕获组件渲染、生命周期、事件等过程中传播的错误；回调接收错误、组件实例及错误来源信息。
		/// </summary>
		[Description("@#errorHandler")]
		public extern VueAppErrorHandler? ErrorHandler { get; set; }

		/// <summary>
		/// 自定义开发环境警告处理函数；接收警告文本、组件实例和组件追踪信息，生产构建中不调用。
		/// </summary>
		[Description("@#warnHandler")]
		public extern VueAppWarnHandler? WarnHandler { get; set; }

		/// <summary>
		/// 在浏览器 Performance 面板记录组件初始化、编译、渲染和更新的性能标记；仅开发模式且支持 performance.mark 时生效。
		/// </summary>
		[Description("@#performance")]
		public extern bool Performance { get; set; }

		/// <summary>
		/// Options to pass to `@vue/compiler-dom`.
		/// Only supported in runtime compiler build.
		/// </summary>
		[Description("@#compilerOptions")]
		public extern VueAppCompilerOptions CompilerOptions { get; }

		/// <summary>
		/// 暴露给本应用所有组件实例的全局属性；组件自身同名属性优先。
		/// </summary>
		[Description("@#globalProperties")]
		public extern VueGlobalProperties GlobalProperties { get; }

		/// <summary>
		/// 为自定义组件选项指定合并函数；用于 mixins 和 extends 等选项合并过程。
		/// </summary>
		[Description("@#optionMergeStrategies")]
		public extern VueOptionMergeStrategies OptionMergeStrategies { get; }

		/// <summary>
		/// Prefix for all useId() calls within this app
		/// </summary>
		[Description("@#idPrefix")]
		public extern string? IdPrefix { get; set; }

		/// <summary>
		/// Whether to throw unhandled errors in production.
		/// Default is `false` to avoid crashing on any error (and only logs it)
		/// But in some cases, e.g. SSR, throwing might be more desirable.
		/// </summary>
		[Description("@#throwUnhandledErrorInProduction")]
		public extern bool ThrowUnhandledErrorInProduction { get; set; }
	}

	/// <summary>
	/// 只读响应式引用。仅 <c>value</c> getter 可用；不允许写入。通常由 <see cref="Computed{T}(Func{T})"/> 或 <c>readonly()</c> 创建。
	/// A readonly reactive reference. Only the <c>value</c> getter is available; writes
	/// are not permitted. Typically created by <see cref="Computed{T}(Func{T})"/> or <c>readonly()</c>.
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
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
		/// <summary>
		/// 读取属性值时记录依赖。
		/// </summary>
		[Description("@#get")]
		Get,

		/// <summary>
		/// 检查键是否存在时记录依赖。
		/// </summary>
		[Description("@#has")]
		Has,

		/// <summary>
		/// 枚举键或遍历集合时记录迭代依赖。
		/// </summary>
		[Description("@#iterate")]
		Iterate,

		/// <summary>
		/// 修改目标已有属性或集合项的值，触发相关依赖更新。
		/// </summary>
		[Description("@#set")]
		Set,

		/// <summary>
		/// 向目标添加此前不存在的属性或集合项，触发相关依赖更新。
		/// </summary>
		[Description("@#add")]
		Add,

		/// <summary>
		/// 删除目标属性或集合项，触发相关依赖更新。
		/// </summary>
		[Description("@#delete")]
		Delete,

		/// <summary>
		/// 清空响应式集合，触发依赖该集合的 effect。
		/// </summary>
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
		/// <summary>
		/// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
		/// </summary>
		protected VueDebuggerEvent()
		{
		}

		/// <summary>
		/// 触发依赖追踪或更新的响应式 effect；用于定位哪个订阅参与本次调试事件。
		/// </summary>
		[Description("@#effect")]
		public extern VueValue? Effect { get; }

		/// <summary>
		/// 本次依赖操作的原始响应式目标对象。
		/// </summary>
		[Description("@#target")]
		public extern VueValue? Target { get; }

		/// <summary>
		/// 触发追踪或通知的操作类别，区分 get、has、iterate、set、add、delete、clear。
		/// </summary>
		[Description("@#type")]
		public extern VueDebuggerEventType Type { get; }

		/// <summary>
		/// 被读取、写入、删除或迭代的依赖键；迭代事件可能使用内部 Symbol。
		/// </summary>
		[Description("@#key")]
		public extern VueValue? Key { get; }

		/// <summary>
		/// 触发写入或添加操作的新值；读取事件中通常未设置。
		/// </summary>
		[Description("@#newValue")]
		public extern VueValue? NewValue { get; }

		/// <summary>
		/// 触发修改或删除之前的旧值；读取事件中通常未设置。
		/// </summary>
		[Description("@#oldValue")]
		public extern VueValue? OldValue { get; }

		/// <summary>
		/// 集合 clear 操作之前保存的集合快照；用于开发模式依赖调试。
		/// </summary>
		[Description("@#oldTarget")]
		public extern VueValue? OldTarget { get; }
	}

}