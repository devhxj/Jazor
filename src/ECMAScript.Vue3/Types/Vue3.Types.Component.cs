using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue defineComponent 和组件选项对象的类型化 authoring surface。</summary>
/// <remarks>record 主要表达结构化对象形状，不意味着生成 CLR 风格组件类。</remarks>
public static partial class Vue3
{
	/// <summary>
	/// 传递给 <c>defineComponent()</c> 的组件定义对象的基础 record。包含所有组件选项形态共享的选项。
	/// Base record for component definition objects passed to <c>defineComponent()</c>.
	/// Holds options shared by all component option shapes.
	/// </summary>
	public abstract record VueComponentDefinition : IVueOptionsBag
	{
		/// <summary>
		/// 控制透传属性是否自动应用到组件的根元素。
		/// Controls whether fallthrough attributes are automatically applied to the
		/// component's root element.
		/// </summary>
		[Description("@#inheritAttrs")]
		public bool? InheritAttrs { get; init; }

		/// <summary>
		/// 选项式 API 的公开实例声明。只有列出的成员名称可以通过模板 ref 在组件公开实例上访问。
		/// Option-form public instance expose declaration. Only listed member names are
		/// available through template refs on the component public instance.
		/// </summary>
		[Description("@#expose")]
		public string[]? Expose { get; init; }

		/// <summary>
		/// 选项式 API 的 provide 对象。当 provide 的键是动态的或由库定义时，使用类型化的 <see cref="VueProps"/> record 或 <see cref="VueDictionary"/>。当 Vue 需要为每个组件实例调用函数来创建 provide 对象时，使用 <see cref="ProvideFactory"/>。
		/// Options API provide object. Use a typed <see cref="VueProps"/> record or
		/// <see cref="VueDictionary"/> when the provide keys are dynamic or library-defined.
		/// Use <see cref="ProvideFactory"/> when Vue should call a function to create the
		/// provide object per component instance.
		/// </summary>
		[Description("@#provide")]
		public VueProps? Provide { get; init; }

		/// <summary>
		/// 选项式 API 的函数形式 provide 回调。当提供的值应该为每个组件实例惰性生成时使用。对于绑定 this 的编写方式，将此属性与 <see cref="Vue3.BindThis{TThis}(VueThisDataCallback{TThis})"/> 结合使用。
		/// Options API function-form provide callback. Use this when the provided values
		/// should be produced lazily per component instance. For this-bound authoring,
		/// combine this property with <see cref="Vue3.BindThis{TThis}(VueThisDataCallback{TThis})"/>.
		/// </summary>
		[Description("@#provide")]
		public VueDataCallback? ProvideFactory { get; init; }

		/// <summary>
		/// 选项式 API 的 inject 声明。数组形式注入使用 <c>string[]</c>；对象形式注入可以使用类型化的 <see cref="VueProps"/> record 或 <see cref="VueDictionary"/>。
		/// Options API inject declaration. Array-form injection uses <c>string[]</c>;
		/// object-form injection can be expressed with a typed <see cref="VueProps"/>
		/// record or <see cref="VueDictionary"/>.
		/// </summary>
		[Description("@#inject")]
		public VueNamesOrOptions? Inject { get; init; }

		/// <summary>
		/// 通过 Vue 的选项式 API 合并策略合并到当前组件的局部混入。新的可复用逻辑优先使用组合式 API；此属性作为 Vue 选项对象的底层兼容绑定而存在。
		/// Local mixins merged into this component by Vue's Options API merge strategy.
		/// Prefer Composition API for new reusable logic; this property exists as a
		/// low-level compatibility binding for Vue options objects.
		/// </summary>
		[Description("@#mixins")]
		public VueComponentDefinition[]? Mixins { get; init; }

		/// <summary>
		/// 通过 Vue 的选项式 API <c>extends</c> 策略合并到当前组件的基础组件选项对象。这是底层兼容绑定，而非 C# 继承模型。
		/// Base component options object merged into this component by Vue's Options API
		/// <c>extends</c> strategy. This is a low-level compatibility binding rather than
		/// a C# inheritance model.
		/// </summary>
		[Description("@#extends")]
		public VueComponentDefinition? Extends { get; init; }

		/// <summary>
		/// 选项式 API 的 <c>data()</c> 工厂函数。返回 <see cref="VueProps"/> record，使 Vue 为每个组件实例获取新的普通对象。绑定实例的 <c>data(vm)</c> / <c>this</c> 编写方式专门留给更广泛的 this 绑定选项式 API 设计。
		/// Options API <c>data()</c> factory. Return a <see cref="VueProps"/> record so Vue
		/// receives a fresh plain object for each component instance. Instance-bound
		/// <c>data(vm)</c> / <c>this</c> authoring is intentionally left to the broader
		/// this-bound Options API design.
		/// </summary>
		[Description("@#data")]
		public VueDataCallback? Data { get; init; }

		/// <summary>
		/// 选项式 API 的 computed 对象。对于具有同一值类型的动态键，使用 <see cref="VueComputedRegistry{TValue}"/>；对于异构强类型计算属性声明，使用自定义 <see cref="VueProps"/> record。
		/// Options API computed object. Use <see cref="VueComputedRegistry{TValue}"/> for
		/// dynamic keys with one value type, or a custom <see cref="VueProps"/> record for
		/// heterogeneous strongly typed computed declarations.
		/// </summary>
		[Description("@#computed")]
		public VueProps? Computed { get; init; }

		/// <summary>
		/// 选项式 API 的 methods 对象。对于具有同一委托签名的动态键，使用 <see cref="VueMethodRegistry{TDelegate}"/>；对于异构强类型方法声明，使用自定义 <see cref="VueProps"/> record。
		/// Options API methods object. Use <see cref="VueMethodRegistry{TDelegate}"/> for
		/// dynamic keys with one delegate signature, or a custom <see cref="VueProps"/>
		/// record for heterogeneous strongly typed method declarations.
		/// </summary>
		[Description("@#methods")]
		public VueProps? Methods { get; init; }

		/// <summary>
		/// 选项式 API 的 watch 对象。对于观察同一值类型的动态键，使用 <see cref="VueWatchRegistry{TValue}"/>；对于异构强类型 watch 声明，使用自定义 <see cref="VueProps"/> record。
		/// Options API watch object. Use <see cref="VueWatchRegistry{TValue}"/> for dynamic
		/// keys that observe one value type, or a custom <see cref="VueProps"/> record for
		/// heterogeneous strongly typed watch declarations.
		/// </summary>
		[Description("@#watch")]
		public VueProps? Watch { get; init; }

		/// <summary>
		/// 在组件实例初始化之后立即调用的选项式 API 钩子。此 C# 表面建模的是无 <c>this</c> 的回调形式；绑定 this 的选项式 API 编写是一个独立的设计问题。
		/// Options API hook invoked immediately after the component instance is initialized.
		/// This C# surface models the no-<c>this</c> callback form; this-bound Options API
		/// authoring remains a separate design problem.
		/// </summary>
		[Description("@#beforeCreate")]
		public Action? BeforeCreate { get; init; }

		/// <summary>
		/// 在响应式状态初始化之后调用的选项式 API 钩子。
		/// Options API hook invoked after reactive state has been initialized.
		/// </summary>
		[Description("@#created")]
		public Action? Created { get; init; }

		/// <summary>
		/// 在组件挂载之前调用的选项式 API 钩子。
		/// Options API hook invoked right before the component is mounted.
		/// </summary>
		[Description("@#beforeMount")]
		public Action? BeforeMount { get; init; }

		/// <summary>
		/// 在组件挂载之后调用的选项式 API 钩子。
		/// Options API hook invoked after the component has been mounted.
		/// </summary>
		[Description("@#mounted")]
		public Action? Mounted { get; init; }

		/// <summary>
		/// 在响应式更新修补 DOM 之前调用的选项式 API 钩子。
		/// Options API hook invoked right before a reactive update patches the DOM.
		/// </summary>
		[Description("@#beforeUpdate")]
		public Action? BeforeUpdate { get; init; }

		/// <summary>
		/// 在响应式更新修补 DOM 之后调用的选项式 API 钩子。
		/// Options API hook invoked after a reactive update has patched the DOM.
		/// </summary>
		[Description("@#updated")]
		public Action? Updated { get; init; }

		/// <summary>
		/// 在组件卸载之前调用的选项式 API 钩子。
		/// Options API hook invoked right before the component is unmounted.
		/// </summary>
		[Description("@#beforeUnmount")]
		public Action? BeforeUnmount { get; init; }

		/// <summary>
		/// 在组件卸载之后调用的选项式 API 钩子。
		/// Options API hook invoked after the component has been unmounted.
		/// </summary>
		[Description("@#unmounted")]
		public Action? Unmounted { get; init; }

		/// <summary>
		/// 当被 keep-alive 缓存的组件重新插入 DOM 时调用的选项式 API 钩子。
		/// Options API hook invoked when a kept-alive component is inserted back into the DOM.
		/// </summary>
		[Description("@#activated")]
		public Action? Activated { get; init; }

		/// <summary>
		/// 当被 keep-alive 缓存的组件从 DOM 缓存出口移除时调用的选项式 API 钩子。
		/// Options API hook invoked when a kept-alive component is removed from the DOM cache outlet.
		/// </summary>
		[Description("@#deactivated")]
		public Action? Deactivated { get; init; }

		/// <summary>
		/// 当捕获到后代组件的错误时调用的选项式 API 钩子。返回 <c>false</c> 可根据 Vue 运行时语义阻止错误传播。
		/// Options API hook invoked when an error from a descendant component is captured.
		/// Return <c>false</c> to stop propagation according to Vue runtime semantics.
		/// </summary>
		[Description("@#errorCaptured")]
		public VueErrorCapturedCallback? ErrorCaptured { get; init; }

		/// <summary>
		/// 仅开发模式下，在渲染期间追踪到响应式依赖时调用的选项式 API 钩子。
		/// Development-only Options API hook invoked when a reactive dependency is tracked during render.
		/// </summary>
		[Description("@#renderTracked")]
		public VueDebuggerCallback? RenderTracked { get; init; }

		/// <summary>
		/// 仅开发模式下，当响应式依赖触发渲染更新时调用的选项式 API 钩子。
		/// Development-only Options API hook invoked when a reactive dependency triggers a render update.
		/// </summary>
		[Description("@#renderTriggered")]
		public VueDebuggerCallback? RenderTriggered { get; init; }

		/// <summary>
		/// 在服务端渲染组件之前调用的服务端渲染钩子。
		/// Server-rendering hook invoked before the component is rendered on the server.
		/// </summary>
		[Description("@#serverPrefetch")]
		public VueServerPrefetchPromiseCallback? ServerPrefetch { get; init; }
	}

	/// <summary>
	/// 当前组件可在其模板中使用的子组件注册表。可以直接作为字符串键的 bag 使用，或在库需要更强类型注册表表面时继承。映射到 Vue 的 <c>components</c> 选项。
	/// Registry of child components that the current component can use in its template.
	/// This can be used directly as a string-keyed bag, or inherited when a library wants
	/// a more strongly-typed registry surface. Maps to Vue's <c>components</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueComponentRegistry : IVueOptionsBag
	{
		/// <summary>
		/// 通过最终发射名称获取或设置组件注册。
		/// Gets or sets a component registration by its final emitted name.
		/// </summary>
		/// <param name="key">最终的 Vue 组件注册名称。The final Vue component registration name.</param>
		/// <returns>为该名称注册的组件。The component registered for that name.</returns>
		public extern IVueComponent? this[string key] { get; set; }
	}

	/// <summary>
	/// 当前组件可在其模板中使用的自定义指令注册表。可以直接作为字符串键的 bag 使用，或在库需要更强类型注册表表面时继承。映射到 Vue 的 <c>directives</c> 选项。
	/// Registry of custom directives that the current component can use in its template.
	/// This can be used directly as a string-keyed bag, or inherited when a library wants
	/// a more strongly-typed registry surface. Maps to Vue's <c>directives</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDirectiveRegistry : IVueOptionsBag, System.Collections.IEnumerable
	{
		/// <summary>
		/// 通过最终发射名称获取或设置指令注册。
		/// Gets or sets a directive registration by its final emitted name.
		/// </summary>
		/// <param name="key">最终的 Vue 指令注册名称。The final Vue directive registration name.</param>
		/// <returns>为该名称注册的指令。The directive registered for that name.</returns>
		public extern VueDirective? this[string key] { get; set; }

		/// <summary>
		/// 仅用于集合初始化器编写方式的 CLR 桥接成员。编译器将其降低为普通对象字面量属性，而非发射运行时 <c>Add(...)</c> 调用。
		/// CLR bridge members kept only for collection-initializer authoring. The compiler
		/// lowers these into plain object literal properties instead of emitting runtime
		/// <c>Add(...)</c> calls.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueDirective directive);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add<TValue>(string key, VueDirective<TValue> directive);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueDirectiveFunction directive);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add<TValue>(string key, VueDirectiveFunction<TValue> directive);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// 作为 <c>app.use(plugin, options)</c> 第二个参数传递的插件配置选项 bag。可以直接作为字符串键的选项 bag 使用，或在插件需要更强类型配置表面时继承。
	/// Options bag for plugin configuration passed as the second argument to
	/// <c>app.use(plugin, options)</c>. This can be used directly as a string-keyed
	/// options bag, or inherited when a plugin wants a stronger typed configuration
	/// surface.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VuePluginOptions : IVueOptionsBag
	{
		/// <summary>
		/// 通过最终发射键获取或设置任意插件选项。
		/// Gets or sets an arbitrary plugin option by its final emitted key.
		/// </summary>
		/// <param name="key">要发射的最终 JavaScript 对象键。The final JavaScript object key to emit.</param>
		/// <returns>映射到给定键的选项值。The option value mapped to the given key.</returns>
		public extern VueValue? this[string key] { get; set; }
	}

	/// <summary>
	/// 共享同一值类型的计算属性的选项式 API computed 注册表。对于异构计算值，请改为声明具有类型化属性的自定义 <see cref="VueProps"/> record。
	/// Options API computed registry for computed properties that share one value type.
	/// For heterogeneous computed values, declare a custom <see cref="VueProps"/> record
	/// with typed properties instead.
	/// </summary>
	/// <typeparam name="TValue">计算属性的值类型。The computed property value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueComputedRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// 通过最终发射键获取或设置计算属性声明。值可以是 getter 回调或可写计算属性的 get/set 选项。
		/// Gets or sets a computed property declaration by its final emitted key.
		/// Values can be getter callbacks or writable computed get/set options.
		/// </summary>
		/// <param name="key">最终的计算属性键。The final computed property key.</param>
		/// <returns>给定键的计算声明。The computed declaration for the given key.</returns>
		public extern VueComputedValue<TValue> this[string key] { get; set; }

		/// <summary>
		/// 仅用于集合初始化器编写 getter 形式计算条目的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of getter-form computed entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, Func<TValue> getter);

		/// <summary>
		/// 仅用于集合初始化器编写可写计算条目的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of writable computed entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWritableComputedOptions<TValue> options);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// 共享同一委托签名的方法的选项式 API methods 注册表。对于异构方法签名，请改为声明具有类型化委托属性的自定义 <see cref="VueProps"/> record。
	/// Options API method registry for methods that share one delegate signature.
	/// For heterogeneous method signatures, declare a custom <see cref="VueProps"/>
	/// record with typed delegate properties instead.
	/// </summary>
	/// <typeparam name="TDelegate">方法委托签名。The method delegate signature.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueMethodRegistry<TDelegate> : VueProps, System.Collections.IEnumerable
		where TDelegate : Delegate
	{
		/// <summary>
		/// 通过最终发射键获取或设置方法声明。
		/// Gets or sets a method declaration by its final emitted key.
		/// </summary>
		/// <param name="key">最终的方法键。The final method key.</param>
		/// <returns>为给定键注册的方法委托。The method delegate registered for the given key.</returns>
		public extern TDelegate? this[string key] { get; set; }

		/// <summary>
		/// 仅用于集合初始化器编写方法条目的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of method entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, TDelegate method);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// 单个选项式 API watch 声明条目。此包装器在保持 watch 处理器联合体强类型的同时，允许通过隐式转换进行自然的 C# 赋值。
	/// Single Options API watch declaration entry. This wrapper keeps
	/// watch handler unions strongly typed while allowing natural C#
	/// assignments through implicit conversions.
	/// </summary>
	/// <typeparam name="TValue">被观察的值类型。The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueWatchEntry<TValue>
	{
		private VueWatchEntry()
		{
		}

		public extern static implicit operator VueWatchEntry<TValue>(string methodName);

		public extern static implicit operator VueWatchEntry<TValue>(Action<TValue, TValue> handler);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchCleanupCallback<TValue> handler);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchHandlerOptions<TValue> options);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchCleanupHandlerOptions<TValue> options);

		public extern static implicit operator VueWatchEntry<TValue>(VueWatchNamedHandlerOptions options);
	}

	/// <summary>
	/// 数组形式的选项式 API watch 声明条目。Vue 运行时接受混合方法名、回调和对象形式处理器的 watch 值数组；此包装器对该表面建模，无需编译器特殊处理。
	/// Array-form Options API watch declaration entries. Vue runtime accepts
	/// watch value arrays that mix method-name, callback, and object-form
	/// handlers; this wrapper models that surface without requiring compiler
	/// special casing.
	/// </summary>
	/// <typeparam name="TValue">被观察的值类型。The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueWatchEntries<TValue>
	{
		private VueWatchEntries()
		{
		}

		public extern static implicit operator VueWatchEntries<TValue>(string[] methodNames);

		public extern static implicit operator VueWatchEntries<TValue>(Action<TValue, TValue>[] handlers);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchCleanupCallback<TValue>[] handlers);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchHandlerOptions<TValue>[] options);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchCleanupHandlerOptions<TValue>[] options);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchNamedHandlerOptions[] options);

		public extern static implicit operator VueWatchEntries<TValue>(VueWatchEntry<TValue>[] entries);
	}

	/// <summary>
	/// 共享同一被观察值类型的 watch 声明的选项式 API watch 注册表。对于异构被观察值类型，请改为声明具有类型化 watch 声明属性的自定义 <see cref="VueProps"/> record。
	/// Options API watch registry for watch declarations that share one observed value type.
	/// For heterogeneous watched value types, declare a custom <see cref="VueProps"/> record
	/// with typed watch declaration properties instead.
	/// </summary>
	/// <typeparam name="TValue">被观察的值类型。The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueWatchRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// 通过最终发射键获取或设置 watch 声明。键可以是属性名称或 Vue 支持的简单点路径。
		/// Gets or sets a watch declaration by its final emitted key. Keys can be property
		/// names or Vue-supported simple dot paths.
		/// </summary>
		/// <param name="key">最终的 watch 源键。The final watch source key.</param>
		/// <returns>给定键的 watch 声明。The watch declaration for the given key.</returns>
		public extern VueWatchDeclaration<TValue> this[string key] { get; set; }

		/// <summary>
		/// 仅用于集合初始化器编写方法名 watch 条目的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of method-name watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, string methodName);

		/// <summary>
		/// 仅用于集合初始化器编写回调 watch 条目的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of callback watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, Action<TValue, TValue> handler);

		/// <summary>
		/// 仅用于集合初始化器编写带清理的 watch 条目的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of cleanup-aware watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchCleanupCallback<TValue> handler);

		/// <summary>
		/// 仅用于集合初始化器编写回调 watch 选项的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of callback watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchHandlerOptions<TValue> options);

		/// <summary>
		/// 仅用于集合初始化器编写带清理的 watch 选项的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of cleanup-aware watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchCleanupHandlerOptions<TValue> options);

		/// <summary>
		/// 仅用于集合初始化器编写方法名 watch 选项的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of method-name watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchNamedHandlerOptions options);

		/// <summary>
		/// 仅用于集合初始化器编写数组形式 watch 条目的 CLR 桥接。
		/// CLR bridge kept for collection-initializer authoring of array-form watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchEntries<TValue> entries);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// 用于单一注入值类型的对象形式选项式 API inject 声明。Vue 接受源键字符串/符号或带有 <c>from</c> / <c>default</c> 的对象；此辅助类型对对象形式建模，同时保持类型化注入键、默认字面量和默认工厂的强类型。
	/// Object-form Options API inject declaration for one injected value type.
	/// Vue accepts either a source key string/symbol or an object with
	/// <c>from</c> / <c>default</c>; this helper models the object form while keeping
	/// typed injection keys, default literals, and default factories strongly typed.
	/// </summary>
	/// <typeparam name="TValue">注入的值契约。The injected value contract.</typeparam>
	public record VueInjectOptions<TValue> : IVueOptionsBag
	{
		/// <summary>
		/// 要解析的源注入键。省略时 Vue 使用本地对象键。接受最终的字符串键、原始 JavaScript <see cref="Symbol"/> 或强类型 <see cref="VueInjectionKey{TValue}"/>。
		/// Source injection key to resolve. When omitted, Vue uses the local object key.
		/// Accepts the final string key, a raw JavaScript <see cref="Symbol"/>, or a
		/// strongly typed <see cref="VueInjectionKey{TValue}"/>.
		/// </summary>
		[Description("@#from")]
		public VueInjectFrom<TValue>? From { get; init; }

		/// <summary>
		/// 没有匹配 provider 时使用的默认值。
		/// Default value used when no provider matches.
		/// </summary>
		[Description("@#default")]
		public TValue? Default { get; init; }

		/// <summary>
		/// 没有匹配 provider 时使用的工厂默认值。
		/// Factory default used when no provider matches.
		/// </summary>
		[Description("@#default")]
		public Func<TValue>? DefaultFactory { get; init; }
	}

	/// <summary>
	/// 使用 <see cref="VueValue"/> 作为注入值契约的非泛型 inject 选项。
	/// Non-generic inject options using <see cref="VueValue"/> for the injected value contract.
	/// </summary>
	public record VueInjectOptions : VueInjectOptions<VueValue>;

	/// <summary>
	/// 单个选项式 API inject 条目。此包装器使对象形式 inject 编写对类型化自定义 record 和字符串键注册表都保持便捷。
	/// Single Options API inject entry. This wrapper keeps object-form inject authoring
	/// ergonomic for both typed custom records and string-keyed registries.
	/// </summary>
	/// <typeparam name="TValue">注入的值契约。The injected value contract.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public class VueInjectEntry<TValue>
	{
		protected VueInjectEntry()
		{
		}

		public extern static implicit operator VueInjectEntry<TValue>(string from);

		public extern static implicit operator VueInjectEntry<TValue>(VueInjectionKey<TValue> from);

		public extern static implicit operator VueInjectEntry<TValue>(Symbol from);

		public extern static implicit operator VueInjectEntry<TValue>(VueInjectOptions<TValue> options);
	}

	/// <summary>
	/// 使用 <see cref="VueValue"/> 作为注入值契约的非泛型 inject 条目。
	/// Non-generic inject entry using <see cref="VueValue"/> for the injected value contract.
	/// </summary>
	public class VueInjectEntry : VueInjectEntry<VueValue>;

	/// <summary>
	/// 共享同一注入值契约的声明的字符串键对象形式选项式 API inject 注册表。
	/// String-keyed object-form Options API inject registry for declarations that share
	/// one injected value contract.
	/// </summary>
	/// <typeparam name="TValue">所有条目使用的注入值类型。The injected value type used by all entries.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueInjectRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// 通过本地注入键获取或设置一个对象形式 inject 声明。
		/// Gets or sets one object-form inject declaration by its local injection key.
		/// </summary>
		/// <param name="key">本地 inject 属性键。The local inject property key.</param>
		/// <returns>给定键的声明。The declaration for the given key.</returns>
		public extern VueInjectEntry<TValue>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, string from);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueInjectOptions<TValue> options);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueInjectEntry<TValue> entry);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// 使用 <see cref="VueValue"/> 作为每个条目值契约的非泛型对象形式 inject 注册表。
	/// Non-generic object-form inject registry using <see cref="VueValue"/> for each entry.
	/// </summary>
	public record VueInjectRegistry : VueInjectRegistry<VueValue>;

	/// <summary>
	/// <c>defineCustomElement()</c> 作为其第二个参数接受的自定义元素专属选项。普通组件选项仍通过 <see cref="VueComponentDefinition"/> 及其类型化变体编写。
	/// Custom-element-specific options accepted by <c>defineCustomElement()</c> as
	/// its second argument. Normal component options remain authored through
	/// <see cref="VueComponentDefinition"/> and its typed variants.
	/// </summary>
	public record VueCustomElementOptions : IVueOptionsBag
	{
		/// <summary>
		/// 注入到自定义元素影子根的 CSS 字符串。
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// 用于在挂载前配置内部创建的 Vue 应用的回调。
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// 控制 Vue 是否为此自定义元素附加影子根。
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// 在 Vue 创建元素的影子根时转发到的原生影子根初始化选项。
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// 应用于注入的样式标签的 nonce 值，用于内容安全策略支持。
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// 没有类型化 props 或 slots 的 <c>defineComponent()</c> 选项。适用于依赖非类型化 props 或完全没有 props 的简单组件。
	/// Options for <c>defineComponent()</c> with no typed props or slots. Use this variant
	/// for simple components that rely on untyped props or have no props at all.
	/// </summary>
	public record VueComponentOptions : VueComponentDefinition
	{
		/// <summary>
		/// 用于 devtools 显示、递归自引用和警告消息的组件名称。省略时 Vue 从文件或变量推断名称。
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// 注册在当前组件上的子组件，使其在渲染函数中按名称可用。
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// 注册在当前组件上的自定义指令，使其在渲染函数中按名称可用。
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// 显式的 Vue <c>props</c> 声明，使用数组形式名称或对象形式的验证器/默认值/类型检查。
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// 显式的 Vue <c>emits</c> 声明，使用数组形式事件名称或对象形式验证器。
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// 在组件挂载之前调用的 setup 函数。不接收 props，必须返回产生组件 VNode 树的 <see cref="VueRenderCallback"/>。
		/// Setup function called before the component is mounted. Receives no props and
		/// must return a <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueSetupCallback? Setup { get; init; }

		/// <summary>
		/// 直接调用以产生组件 VNode 树的渲染函数。这是 <see cref="Setup"/> 的替代方案；如果两者都提供，<c>render</c> 优先于 setup 返回值。
		/// Render function called directly to produce the component's VNode tree. This is
		/// an alternative to <see cref="Setup"/>; if both are provided, <c>render</c> takes
		/// precedence over the setup return value.
		/// </summary>
		[Description("@#render")]
		public VueRenderCallback? Render { get; init; }
	}

	/// <summary>
	/// 带有类型化 props 的 <c>defineComponent()</c> 选项。泛型参数驱动 C# setup 和 <c>h(...)</c> 类型检查；运行时 <c>props</c> / <c>emits</c> 声明在需要时应通过选项成员显式提供。
	/// Options for <c>defineComponent()</c> with typed props. The generic parameter drives
	/// C# setup and <c>h(...)</c> type checking; runtime <c>props</c> / <c>emits</c>
	/// declarations should be supplied explicitly through the option members when needed.
	/// </summary>
	/// <typeparam name="TProps">描述组件所接受 props 的 props record 类型。The props record type describing the component's accepted props.</typeparam>
	public record VueComponentOptions<TProps> : VueComponentDefinition
		where TProps : VueProps
	{
		/// <summary>
		/// 用于 devtools 显示、递归自引用和警告消息的组件名称。省略时 Vue 从文件或变量推断名称。
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// 注册在当前组件上的子组件，使其在渲染函数中按名称可用。
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// 注册在当前组件上的自定义指令，使其在渲染函数中按名称可用。
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// 显式的 Vue <c>props</c> 声明，使用数组形式名称或对象形式的验证器/默认值/类型检查。
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// 显式的 Vue <c>emits</c> 声明，使用数组形式事件名称或对象形式验证器。
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// 在组件挂载之前调用的 setup 函数。接收类型化 props 和 setup 上下文，必须返回产生组件 VNode 树的 <see cref="VueRenderCallback"/>。
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a setup context, and must return a <see cref="VueRenderCallback"/> that produces
		/// the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps>? Setup { get; init; }
	}

	/// <summary>
	/// 同时带有类型化 props 和类型化 slots 的 <c>defineComponent()</c> 选项。泛型参数驱动 C# setup、slot 和 <c>h(...)</c> 类型检查；运行时 <c>props</c> / <c>emits</c> 声明仍为显式选项成员。
	/// Options for <c>defineComponent()</c> with both typed props and typed slots. The
	/// generic parameters drive C# setup, slot, and <c>h(...)</c> type checking; runtime
	/// <c>props</c> / <c>emits</c> declarations remain explicit option members.
	/// </summary>
	/// <typeparam name="TProps">描述组件所接受 props 的 props record 类型。The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">描述组件所接受 slots 的 slots record 类型。The slots record type describing the component's accepted slots.</typeparam>
	public record VueComponentOptions<TProps, TSlots> : VueComponentDefinition
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// 用于 devtools 显示、递归自引用和警告消息的组件名称。省略时 Vue 从文件或变量推断名称。
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// 注册在当前组件上的子组件，使其在渲染函数中按名称可用。
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// 注册在当前组件上的自定义指令，使其在渲染函数中按名称可用。
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// 显式的 Vue <c>props</c> 声明，使用数组形式名称或对象形式的验证器/默认值/类型检查。
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// 显式的 Vue <c>emits</c> 声明，使用数组形式事件名称或对象形式验证器。
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// 在组件挂载之前调用的 setup 函数。接收类型化 props 和带有类型化 slot 访问的类型化 setup 上下文，必须返回产生组件 VNode 树的 <see cref="VueRenderCallback"/>。
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a typed setup context (with typed slot access), and must return a
		/// <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps, TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// 带有类型化 slots 但没有类型化 props 的 <c>defineComponent()</c> 选项。适用于接受命名插槽但不声明类型化 props 的组件。
	/// Options for <c>defineComponent()</c> with typed slots but no typed props. Use this
	/// variant for components that accept named slots but do not declare typed props.
	/// </summary>
	/// <typeparam name="TSlots">描述组件所接受 slots 的 slots record 类型。The slots record type describing the component's accepted slots.</typeparam>
	public record VueSlotComponentOptions<TSlots> : VueComponentDefinition
		where TSlots : VueSlots
	{
		/// <summary>
		/// 用于 devtools 显示、递归自引用和警告消息的组件名称。省略时 Vue 从文件或变量推断名称。
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// 注册在当前组件上的子组件，使其在渲染函数中按名称可用。
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// 注册在当前组件上的自定义指令，使其在渲染函数中按名称可用。
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// 显式的 Vue <c>props</c> 声明，使用数组形式名称或对象形式的验证器/默认值/类型检查。
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// 显式的 Vue <c>emits</c> 声明，使用数组形式事件名称或对象形式验证器。
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// 在组件挂载之前调用的 setup 函数。接收带有类型化 slot 访问的类型化 setup 上下文，必须返回产生组件 VNode 树的 <see cref="VueRenderCallback"/>。
		/// Setup function called before the component is mounted. Receives a typed setup
		/// context with typed slot access, and must return a <see cref="VueRenderCallback"/>
		/// that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSlotSetupCallback<TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// 单参数 <c>defineCustomElement(...)</c> 编写表面，将普通非类型化组件选项与 <c>styles</c> 和 <c>shadowRoot</c> 等自定义元素专属选项合并。
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// normal untyped component options with custom-element-only options such as
	/// <c>styles</c> and <c>shadowRoot</c>.
	/// </summary>
	public record VueCustomElementComponentOptions : VueComponentOptions
	{
		/// <summary>
		/// 注入到自定义元素影子根的 CSS 字符串。
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// 用于在挂载前配置内部创建的 Vue 应用的回调。
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// 控制 Vue 是否为此自定义元素附加影子根。设为 <c>false</c> 以进行 light-DOM 渲染。
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// 在 Vue 创建元素的影子根时转发到的原生影子根初始化选项。
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// 应用于注入的样式标签的 nonce 值，用于内容安全策略支持。
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// 单参数 <c>defineCustomElement(...)</c> 编写表面，将类型化 props 组件选项与自定义元素专属选项合并。
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-props component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TProps">自定义元素组件所接受的 props 契约。The props contract accepted by the custom element component.</typeparam>
	public record VueCustomElementComponentOptions<TProps> : VueComponentOptions<TProps>
		where TProps : VueProps
	{
		/// <summary>
		/// 注入到自定义元素影子根的 CSS 字符串。
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// 用于在挂载前配置内部创建的 Vue 应用的回调。
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// 控制 Vue 是否为此自定义元素附加影子根。设为 <c>false</c> 以进行 light-DOM 渲染。
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// 在 Vue 创建元素的影子根时转发到的原生影子根初始化选项。
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// 应用于注入的样式标签的 nonce 值，用于内容安全策略支持。
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// 单参数 <c>defineCustomElement(...)</c> 编写表面，将类型化 props/类型化 slots 组件选项与自定义元素专属选项合并。
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-props/typed-slots component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TProps">自定义元素组件所接受的 props 契约。The props contract accepted by the custom element component.</typeparam>
	/// <typeparam name="TSlots">自定义元素组件所接受的 slots 契约。The slots contract accepted by the custom element component.</typeparam>
	public record VueCustomElementComponentOptions<TProps, TSlots> : VueComponentOptions<TProps, TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// 注入到自定义元素影子根的 CSS 字符串。
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// 用于在挂载前配置内部创建的 Vue 应用的回调。
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// 控制 Vue 是否为此自定义元素附加影子根。设为 <c>false</c> 以进行 light-DOM 渲染。
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// 在 Vue 创建元素的影子根时转发到的原生影子根初始化选项。
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// 应用于注入的样式标签的 nonce 值，用于内容安全策略支持。
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// 单参数 <c>defineCustomElement(...)</c> 编写表面，将类型化 slots 组件选项与自定义元素专属选项合并。
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-slots component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TSlots">自定义元素组件所接受的 slots 契约。The slots contract accepted by the custom element component.</typeparam>
	public record VueCustomElementSlotComponentOptions<TSlots> : VueSlotComponentOptions<TSlots>
		where TSlots : VueSlots
	{
		/// <summary>
		/// 注入到自定义元素影子根的 CSS 字符串。
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// 用于在挂载前配置内部创建的 Vue 应用的回调。
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// 控制 Vue 是否为此自定义元素附加影子根。设为 <c>false</c> 以进行 light-DOM 渲染。
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// 在 Vue 创建元素的影子根时转发到的原生影子根初始化选项。
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// 应用于注入的样式标签的 nonce 值，用于内容安全策略支持。
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// <c>defineAsyncComponent()</c> 的选项。Vue 接受直接传入 loader 函数或此对象形式，用于加载/错误组件、计时、suspense 和重试行为。
	/// Options for <c>defineAsyncComponent()</c>. Vue accepts either a loader function
	/// directly or this object form for loading/error components, timing, suspense, and
	/// retry behavior.
	/// </summary>
	public record VueAsyncComponentOptions : IVueOptionsBag
	{
		/// <summary>
		/// 加载并解析组件定义的函数。
		/// Function that loads and resolves the component definition.
		/// </summary>
		[Description("@#loader")]
		public VueAsyncComponentLoader Loader { get; init; } = default!;

		/// <summary>
		/// 在异步组件加载期间渲染的组件。
		/// Component rendered while the async component is loading.
		/// </summary>
		[Description("@#loadingComponent")]
		public ECMAScript.Vue3.IVueComponent? LoadingComponent { get; init; }

		/// <summary>
		/// 在异步组件加载失败时渲染的组件。
		/// Component rendered when the async component fails to load.
		/// </summary>
		[Description("@#errorComponent")]
		public ECMAScript.Vue3.IVueComponent? ErrorComponent { get; init; }

		/// <summary>
		/// 显示加载组件前的延迟毫秒数。
		/// Delay in milliseconds before showing the loading component.
		/// </summary>
		[Description("@#delay")]
		public Number? Delay { get; init; }

		/// <summary>
		/// Vue 将加载视为失败之前的超时毫秒数。
		/// Timeout in milliseconds before Vue treats loading as failed.
		/// </summary>
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		/// <summary>
		/// 异步组件是否可以参与父级 <c>Suspense</c> 边界。
		/// Whether the async component can participate in a parent <c>Suspense</c> boundary.
		/// </summary>
		[Description("@#suspensible")]
		public bool? Suspensible { get; init; }

		/// <summary>
		/// 加载失败时调用的回调；可以重试或使异步加载失败。
		/// Callback invoked when loading fails; it can retry or fail the async load.
		/// </summary>
		[Description("@#onError")]
		public VueAsyncComponentErrorCallback? OnError { get; init; }
	}

	/// <summary>
	/// <c>defineAsyncComponent()</c> 的强类型选项。泛型组件契约由返回的异步组件引用保留。
	/// Strongly typed options for <c>defineAsyncComponent()</c>. The generic component
	/// contract is preserved by the returned async component reference.
	/// </summary>
	/// <typeparam name="TComponent">由 loader 生成的组件契约。The component contract produced by the loader.</typeparam>
	public record VueAsyncComponentOptions<TComponent> : IVueOptionsBag
		where TComponent : ECMAScript.Vue3.IVueComponent
	{
		/// <summary>
		/// 加载并解析类型化组件定义的函数。
		/// Function that loads and resolves the typed component definition.
		/// </summary>
		[Description("@#loader")]
		public VueAsyncComponentLoader<TComponent> Loader { get; init; } = default!;

		/// <summary>
		/// 在异步组件加载期间渲染的组件。
		/// Component rendered while the async component is loading.
		/// </summary>
		[Description("@#loadingComponent")]
		public ECMAScript.Vue3.IVueComponent? LoadingComponent { get; init; }

		/// <summary>
		/// 在异步组件加载失败时渲染的组件。
		/// Component rendered when the async component fails to load.
		/// </summary>
		[Description("@#errorComponent")]
		public ECMAScript.Vue3.IVueComponent? ErrorComponent { get; init; }

		/// <summary>
		/// 显示加载组件前的延迟毫秒数。
		/// Delay in milliseconds before showing the loading component.
		/// </summary>
		[Description("@#delay")]
		public Number? Delay { get; init; }

		/// <summary>
		/// Vue 将加载视为失败之前的超时毫秒数。
		/// Timeout in milliseconds before Vue treats loading as failed.
		/// </summary>
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		/// <summary>
		/// 异步组件是否可以参与父级 <c>Suspense</c> 边界。
		/// Whether the async component can participate in a parent <c>Suspense</c> boundary.
		/// </summary>
		[Description("@#suspensible")]
		public bool? Suspensible { get; init; }

		/// <summary>
		/// 加载失败时调用的回调；可以重试或使异步加载失败。
		/// Callback invoked when loading fails; it can retry or fail the async load.
		/// </summary>
		[Description("@#onError")]
		public VueAsyncComponentErrorCallback? OnError { get; init; }
	}

}
