using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// 创建对象的深层响应式代理。
	/// Creates a deep reactive proxy of an object. Vue recursively converts all nested
	/// properties into reactive getters/setters, so reads and writes at any depth are tracked.
	/// </summary>
	/// <typeparam name="T">要使其变为响应式的对象类型（必须是引用类型）。The type of the object to make reactive (must be a reference type).</typeparam>
	/// <param name="value">要包装为响应式代理的普通对象。The plain object to wrap in a reactive proxy.</param>
	/// <returns>相同类型的响应式代理。所有属性访问都会被追踪。A reactive proxy of the same type. All property accesses are tracked.</returns>
	[Description("@#reactive")]
	public extern static T Reactive<T>(T value) where T : class;

	/// <summary>
	/// 创建响应式（或普通）对象的只读代理。
	/// Creates a readonly proxy of a reactive (or plain) object. Attempts to write to
	/// properties on the returned object will trigger a runtime warning and be ignored.
	/// </summary>
	/// <typeparam name="T">要使其变为只读的对象类型（必须是引用类型）。The type of the object to make readonly (must be a reference type).</typeparam>
	/// <param name="value">要包装为只读代理的对象。可以是响应式代理或普通对象。The object to wrap in a readonly proxy. Can be a reactive proxy or a plain object.</param>
	/// <returns>相同类型的只读代理。读取被追踪；写入被阻止。A readonly proxy of the same type. Reads are tracked; writes are blocked.</returns>
	[Description("@#readonly")]
	public extern static T Readonly<T>(T value) where T : class;

	/// <summary>
	/// 创建对象的浅层响应式代理。
	/// Creates a shallow reactive proxy of an object.
	/// </summary>
	/// <typeparam name="T">要包装的对象类型。The object type to wrap.</typeparam>
	/// <param name="value">要包装的对象。The object to wrap.</param>
	/// <returns>相同类型的浅层响应式代理。A shallow reactive proxy of the same type.</returns>
	[Description("@#shallowReactive")]
	public extern static T ShallowReactive<T>(T value) where T : class;

	/// <summary>
	/// 创建对象的浅层只读代理。
	/// Creates a shallow readonly proxy of an object.
	/// </summary>
	/// <typeparam name="T">要包装的对象类型。The object type to wrap.</typeparam>
	/// <param name="value">要包装的对象。The object to wrap.</param>
	/// <returns>相同类型的浅层只读代理。A shallow readonly proxy of the same type.</returns>
	[Description("@#shallowReadonly")]
	public extern static T ShallowReadonly<T>(T value) where T : class;

	/// <summary>
	/// 返回 Vue 代理背后的原始对象。
	/// Returns the raw object behind a Vue proxy.
	/// </summary>
	/// <typeparam name="T">对象的静态类型。The static object type.</typeparam>
	/// <param name="value">代理值。The proxy value.</param>
	/// <returns>原始的未包装对象。The original raw object.</returns>
	[Description("@#toRaw")]
	public extern static T ToRaw<T>(T value) where T : class;

	/// <summary>
	/// 标记对象，使 Vue 永远不会将其转换为代理。
	/// Marks an object so Vue will never convert it to a proxy.
	/// </summary>
	/// <typeparam name="T">要标记的对象类型。The object type to mark.</typeparam>
	/// <param name="value">要标记为原始对象的对象。The object to mark as raw.</param>
	/// <returns>相同的对象。The same object.</returns>
	[Description("@#markRaw")]
	public extern static T MarkRaw<T>(T value) where T : class;

	/// <summary>
	/// 返回一个值是否为 Vue 创建的代理。
	/// Returns whether a value is any Vue-created proxy.
	/// </summary>
	/// <typeparam name="T">被测试的运行时值的静态类型。The static type of the runtime value being tested.</typeparam>
	/// <param name="value">要测试的运行时值。The runtime value to test.</param>
	/// <returns>当值为 Vue 代理时返回 <c>true</c>。<c>true</c> when the value is a Vue proxy.</returns>
	[Description("@#isProxy")]
	public extern static bool IsProxy<T>(T value);

	/// <summary>
	/// 返回一个值是否为响应式代理。
	/// Returns whether a value is a reactive proxy.
	/// </summary>
	/// <typeparam name="T">被测试的运行时值的静态类型。The static type of the runtime value being tested.</typeparam>
	/// <param name="value">要测试的运行时值。The runtime value to test.</param>
	/// <returns>当值为响应式时返回 <c>true</c>。<c>true</c> when the value is reactive.</returns>
	[Description("@#isReactive")]
	public extern static bool IsReactive<T>(T value);

	/// <summary>
	/// 返回一个值是否为只读代理。
	/// Returns whether a value is a readonly proxy.
	/// </summary>
	/// <typeparam name="T">被测试的运行时值的静态类型。The static type of the runtime value being tested.</typeparam>
	/// <param name="value">要测试的运行时值。The runtime value to test.</param>
	/// <returns>当值为只读时返回 <c>true</c>。<c>true</c> when the value is readonly.</returns>
	[Description("@#isReadonly")]
	public extern static bool IsReadonly<T>(T value);

	/// <summary>
	/// 创建包装单个值的响应式 ref。与 <see cref="Reactive{T}"/> 不同，
	/// <c>ref()</c> 包装的是整个值，而非其属性。通过 <see cref="IVueRef{T}.Value"/> 访问值。
	/// Creates a reactive ref wrapping a single value. Unlike <see cref="Reactive{T}"/>,
	/// <c>ref()</c> wraps the entire value, not its properties. Access the value via
	/// <see cref="IVueRef{T}.Value"/>.
	/// </summary>
	/// <typeparam name="T">要包装的值的类型。The type of the value to wrap.</typeparam>
	/// <param name="value">ref 的初始值。The initial value of the ref.</param>
	/// <returns>一个响应式 ref，其 <c>Value</c> 属性可读写被包装的值。A reactive ref whose <c>Value</c> property reads and writes the wrapped value.</returns>
	[Description("@#ref")]
	public extern static IVueRef<T> Ref<T>(T value);

	/// <summary>
	/// 创建浅层响应式 ref，仅追踪 <c>Value</c> 的替换，不追踪值本身的变更。
	/// 适用于深层追踪不必要的大型对象或值被整体替换的场景。
	/// Creates a shallow reactive ref that only tracks replacements of <c>Value</c>, not
	/// mutations of the value itself. Use this for large objects where deep tracking is
	/// unnecessary or when the value is replaced wholesale.
	/// </summary>
	/// <typeparam name="T">要包装的值的类型。The type of the value to wrap.</typeparam>
	/// <param name="value">浅层 ref 的初始值。The initial value of the shallow ref.</param>
	/// <returns>浅层 ref，其 <c>Value</c> 属性仅在替换时触发，不在深层变更时触发。A shallow ref whose <c>Value</c> property only triggers on replacement, not on deep mutation.</returns>
	[Description("@#shallowRef")]
	public extern static VueShallowRef<T> ShallowRef<T>(T value);

	/// <summary>
	/// 强制依赖浅层 ref 的副作用重新执行。
	/// Forces effects depending on a shallow ref to re-run.
	/// </summary>
	/// <typeparam name="T">ref 值的类型。The type of the ref value.</typeparam>
	/// <param name="value">要触发的 ref。The ref to trigger.</param>
	[Description("@#triggerRef")]
	public extern static void TriggerRef<T>(VueShallowRef<T> value);

	/// <summary>
	/// 创建自定义 ref，其依赖追踪和触发由用户提供的 get/set 处理器控制。
	/// Creates a custom ref whose dependency tracking and triggering are controlled by
	/// user-provided get/set handlers.
	/// </summary>
	/// <typeparam name="T">自定义 ref 的值类型。The custom ref value type.</typeparam>
	/// <param name="factory">接收 Vue 的追踪/触发回调并返回 get/set 处理器的工厂函数。Factory receiving Vue's track/trigger callbacks and returning get/set handlers.</param>
	/// <returns>由所提供工厂控制的响应式 ref。A reactive ref controlled by the supplied factory.</returns>
	[Description("@#customRef")]
	public extern static IVueRef<T> CustomRef<T>(VueCustomRefFactory<T> factory);

	/// <summary>
	/// 返回运行时值是否为 Vue ref。
	/// Returns whether a runtime value is a Vue ref.
	/// </summary>
	/// <typeparam name="T">被测试的运行时值的静态类型。The static type of the runtime value being tested.</typeparam>
	/// <param name="value">要测试的运行时值。The runtime value to test.</param>
	/// <returns>当值为 ref 时返回 <c>true</c>。<c>true</c> when the value is a ref.</returns>
	[Description("@#isRef")]
	public extern static bool IsRef<T>(T value);

	/// <summary>
	/// 将普通值原样返回。
	/// Returns a normal value unchanged.
	/// </summary>
	/// <typeparam name="T">值的类型。The value type.</typeparam>
	/// <param name="value">要规范化的值。The value to normalize.</param>
	/// <returns>提供的值。The supplied value.</returns>
	[Description("@#unref")]
	public extern static T Unref<T>(T value);

	/// <summary>
	/// 将 Vue ref 解包为其当前值。
	/// Unwraps a Vue ref to its current value.
	/// </summary>
	/// <typeparam name="T">ref 值的类型。The ref value type.</typeparam>
	/// <param name="value">要解包的 ref。The ref to unwrap.</param>
	/// <returns>ref 的当前值。The current ref value.</returns>
	[Description("@#unref")]
	public extern static T Unref<T>(IVueRef<T> value);

	/// <summary>
	/// 将普通值规范化为 ref。
	/// Normalizes a plain value into a ref.
	/// </summary>
	/// <typeparam name="T">值的类型。The value type.</typeparam>
	/// <param name="value">要包装的值。The value to wrap.</param>
	/// <returns>包装所提供值的 ref。A ref for the supplied value.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<T> ToRef<T>(T value);

	/// <summary>
	/// 将已有的 ref 原样返回。
	/// Returns an existing ref unchanged.
	/// </summary>
	/// <typeparam name="T">ref 值的类型。The ref value type.</typeparam>
	/// <param name="value">要规范化的 ref。The ref to normalize.</param>
	/// <returns>提供的 ref。The supplied ref.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<T> ToRef<T>(IVueRef<T> value);

	/// <summary>
	/// 将 getter 规范化为只读 ref。
	/// Normalizes a getter into a readonly ref.
	/// </summary>
	/// <typeparam name="T">getter 的返回类型。The getter result type.</typeparam>
	/// <param name="getter">要包装的 getter。The getter to wrap.</param>
	/// <returns>由所提供 getter 支持的计算只读 ref。A computed readonly ref backed by the supplied getter.</returns>
	[Description("@#toRef")]
	public extern static VueComputedRef<T> ToRef<T>(Func<T> getter);

	/// <summary>
	/// 为响应式对象上的某个属性创建关联 ref。
	/// 值类型需要显式指定，因为 C# 无法从字符串键推断。
	/// Creates a linked ref for a property on a reactive object. The value type is
	/// explicit because C# cannot infer it from a string key.
	/// </summary>
	/// <typeparam name="TSource">源对象类型。The source object type.</typeparam>
	/// <typeparam name="TValue">关联属性的值类型。The linked property value type.</typeparam>
	/// <param name="source">源响应式对象。The source reactive object.</param>
	/// <param name="key">最终运行时属性名。The final runtime property name.</param>
	/// <returns>与 <paramref name="source"/>[<paramref name="key"/>] 关联的 ref。A ref linked to <paramref name="source"/>[<paramref name="key"/>].</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TSource, TValue>(TSource source, string key)
		where TSource : class;

	/// <summary>
	/// 为响应式对象上的某个属性创建关联 ref，当属性不存在时使用默认值。
	/// Creates a linked ref for a property on a reactive object, using a default value
	/// when the property is absent.
	/// </summary>
	/// <typeparam name="TSource">源对象类型。The source object type.</typeparam>
	/// <typeparam name="TValue">关联属性的值类型。The linked property value type.</typeparam>
	/// <param name="source">源响应式对象。The source reactive object.</param>
	/// <param name="key">最终运行时属性名。The final runtime property name.</param>
	/// <param name="defaultValue">当源属性不存在时 Vue 使用的默认值。The value Vue uses when the source property is absent.</param>
	/// <returns>与 <paramref name="source"/>[<paramref name="key"/>] 关联的 ref。A ref linked to <paramref name="source"/>[<paramref name="key"/>].</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TSource, TValue>(TSource source, string key, TValue defaultValue)
		where TSource : class;

	/// <summary>
	/// 为字典形式的 Vue 对象中的某个键创建关联 ref。
	/// Creates a linked ref for a key in a dictionary-shaped Vue object.
	/// </summary>
	/// <typeparam name="TValue">字典的值类型。The dictionary value type.</typeparam>
	/// <param name="source">源字典形式对象。The source dictionary-shaped object.</param>
	/// <param name="key">最终运行时属性名。The final runtime property name.</param>
	/// <returns>与字典条目关联的 ref。A ref linked to the dictionary entry.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TValue>(VueDictionary<TValue> source, string key);

	/// <summary>
	/// 将响应式对象上的每个可枚举属性转换为关联 ref。
	/// Converts each enumerable property on a reactive object into a linked ref.
	/// </summary>
	/// <typeparam name="TSource">源对象类型。The source object type.</typeparam>
	/// <param name="source">源响应式对象。The source reactive object.</param>
	/// <returns>基于索引器的 refs 集合。An indexer-based refs bag.</returns>
	[Description("@#toRefs")]
	public extern static VueRefs<TSource> ToRefs<TSource>(TSource source)
		where TSource : class;

	/// <summary>
	/// 将 props 风格的对象转换为用户声明的类型化 refs 投影。
	/// Converts a props-style object into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">用户声明的 refs 投影类型。The user-declared refs projection type.</typeparam>
	/// <param name="source">源 props 风格对象。The source props-style object.</param>
	/// <returns>Vue 返回的类型化 refs 投影。The typed refs projection returned by Vue.</returns>
	[Description("@#toRefs")]
	public extern static TRefs ToRefs<TRefs>(VueProps source)
		where TRefs : VueRefs;

	/// <summary>
	/// 将任意响应式对象转换为用户声明的类型化 refs 投影。
	/// Converts an arbitrary reactive object into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">用户声明的 refs 投影类型。The user-declared refs projection type.</typeparam>
	/// <typeparam name="TSource">源对象类型。The source object type.</typeparam>
	/// <param name="source">源响应式对象。The source reactive object.</param>
	/// <returns>Vue 返回的类型化 refs 投影。The typed refs projection returned by Vue.</returns>
	[Description("@#toRefs")]
	public extern static TRefs ToRefs<TRefs, TSource>(TSource source)
		where TRefs : VueRefs<TSource>
		where TSource : class;

	/// <summary>
	/// 创建从 getter 函数派生的计算属性值。
	/// getter 延迟求值并缓存；仅在其响应式依赖变更时重新求值。返回的 ref 是只读的。
	/// Creates a computed reactive value derived from a getter function. The getter is
	/// evaluated lazily and cached; it is re-evaluated only when its reactive dependencies
	/// change. The returned ref is readonly.
	/// </summary>
	/// <typeparam name="T">计算值的类型。The type of the computed value.</typeparam>
	/// <param name="getter">计算派生值的函数。内部访问的响应式值被追踪为依赖。A function that computes the derived value. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>计算只读 ref，其 <c>Value</c> 为最新的计算结果。A computed readonly ref whose <c>Value</c> is the latest computed result.</returns>
	[Description("@#computed")]
	public extern static VueComputedRef<T> Computed<T>(Func<T> getter);

	/// <summary>
	/// 从显式 get/set 委托创建可写计算 ref。
	/// Creates a writable computed ref from explicit get/set delegates.
	/// </summary>
	/// <typeparam name="T">计算值的类型。The computed value type.</typeparam>
	/// <param name="options">包含 <c>get</c> 和 <c>set</c> 的普通 Vue 计算选项。Plain Vue computed options containing <c>get</c> and <c>set</c>.</param>
	/// <returns>可写的计算 ref。A writable computed ref.</returns>
	[Description("@#computed")]
	public extern static VueWritableComputedRef<T> Computed<T>(VueWritableComputedOptions<T> options);

	/// <summary>
	/// 侦听响应式源并在其变更时调用回调。回调接收新值和旧值。
	/// 返回可调用的句柄以停止侦听器。
	/// Watches a reactive source and calls the callback when it changes. The callback
	/// receives both the new value and the previous value. Returns a handle that can be
	/// called to stop the watcher.
	/// </summary>
	/// <typeparam name="T">被侦听值的类型。The type of the watched value.</typeparam>
	/// <param name="source">返回要侦听的响应式值的 getter 函数。每次求值周期都会调用。A getter function that returns the reactive value to watch. Called on each evaluation cycle.</param>
	/// <param name="callback">当源的返回值变更时以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c> whenever the source's return value changes.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback);

	/// <summary>
	/// 使用显式侦听器选项侦听 getter 源。
	/// Watches a getter source with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">被侦听值的类型。The type of the watched value.</typeparam>
	/// <param name="source">返回要侦听的响应式值的 getter 函数。A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// 侦听 getter 源并向回调暴露 Vue 的清理注册函数。
	/// Watches a getter source and exposes Vue's cleanup registration function to the callback.
	/// </summary>
	/// <typeparam name="T">被侦听值的类型。The type of the watched value.</typeparam>
	/// <param name="source">返回要侦听的响应式值的 getter 函数。A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// 使用支持清理的回调和显式侦听器选项侦听 getter 源。
	/// Watches a getter source with cleanup-aware callback and explicit watcher options.
	/// </summary>
	/// <typeparam name="T">被侦听值的类型。The type of the watched value.</typeparam>
	/// <param name="source">返回要侦听的响应式值的 getter 函数。A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 直接侦听响应式对象源。Vue 隐式将其视为对象响应式图的深层侦听器。
	/// Watches a reactive object source directly. Vue implicitly treats this as a deep
	/// watcher over the object's reactive graph.
	/// </summary>
	/// <typeparam name="TSource">响应式对象类型。The reactive object type.</typeparam>
	/// <param name="source">要侦听的响应式对象。The reactive object to watch.</param>
	/// <param name="callback">以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, Action<TSource, TSource> callback)
		where TSource : class;

	/// <summary>
	/// 使用显式侦听器选项直接侦听响应式对象源。
	/// Watches a reactive object source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="TSource">响应式对象类型。The reactive object type.</typeparam>
	/// <param name="source">要侦听的响应式对象。The reactive object to watch.</param>
	/// <param name="callback">以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, Action<TSource, TSource> callback, VueWatchOptions options)
		where TSource : class;

	/// <summary>
	/// 直接侦听响应式对象源并向回调暴露 Vue 的清理注册函数。
	/// Watches a reactive object source directly and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="TSource">响应式对象类型。The reactive object type.</typeparam>
	/// <param name="source">要侦听的响应式对象。The reactive object to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, VueWatchCleanupCallback<TSource> callback)
		where TSource : class;

	/// <summary>
	/// 使用支持清理的回调和显式侦听器选项直接侦听响应式对象源。
	/// Watches a reactive object source directly with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="TSource">响应式对象类型。The reactive object type.</typeparam>
	/// <param name="source">要侦听的响应式对象。The reactive object to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, VueWatchCleanupCallback<TSource> callback, VueWatchOptions options)
		where TSource : class;

	/// <summary>
	/// 直接侦听 ref 源。
	/// Watches a ref source directly.
	/// </summary>
	/// <typeparam name="T">ref 中存储的类型。The type stored in the ref.</typeparam>
	/// <param name="source">要侦听的 ref。The ref to watch.</param>
	/// <param name="callback">以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, Action<T, T> callback);

	/// <summary>
	/// 使用显式侦听器选项直接侦听 ref 源。
	/// Watches a ref source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">ref 中存储的类型。The type stored in the ref.</typeparam>
	/// <param name="source">要侦听的 ref。The ref to watch.</param>
	/// <param name="callback">以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// 直接侦听 ref 源并向回调暴露 Vue 的清理注册函数。
	/// Watches a ref source directly and exposes Vue's cleanup registration function to the callback.
	/// </summary>
	/// <typeparam name="T">ref 中存储的类型。The type stored in the ref.</typeparam>
	/// <param name="source">要侦听的 ref。The ref to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// 使用支持清理的回调和显式侦听器选项直接侦听 ref 源。
	/// Watches a ref source directly with cleanup-aware callback and explicit watcher options.
	/// </summary>
	/// <typeparam name="T">ref 中存储的类型。The type stored in the ref.</typeparam>
	/// <param name="source">要侦听的 ref。The ref to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 直接侦听只读 ref 源。
	/// Watches a readonly ref source directly.
	/// </summary>
	/// <typeparam name="T">只读 ref 暴露的类型。The type exposed by the readonly ref.</typeparam>
	/// <param name="source">要侦听的只读 ref。The readonly ref to watch.</param>
	/// <param name="callback">以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, Action<T, T> callback);

	/// <summary>
	/// 使用显式侦听器选项直接侦听只读 ref 源。
	/// Watches a readonly ref source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">只读 ref 暴露的类型。The type exposed by the readonly ref.</typeparam>
	/// <param name="source">要侦听的只读 ref。The readonly ref to watch.</param>
	/// <param name="callback">以 <c>(newValue, oldValue)</c> 调用的回调。A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// 直接侦听只读 ref 源并向回调暴露 Vue 的清理注册函数。
	/// Watches a readonly ref source directly and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">只读 ref 暴露的类型。The type exposed by the readonly ref.</typeparam>
	/// <param name="source">要侦听的只读 ref。The readonly ref to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// 使用支持清理的回调和显式侦听器选项直接侦听只读 ref 源。
	/// Watches a readonly ref source directly with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">只读 ref 暴露的类型。The type exposed by the readonly ref.</typeparam>
	/// <param name="source">要侦听的只读 ref。The readonly ref to watch.</param>
	/// <param name="callback">以值、旧值和清理注册调用的回调。A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 侦听多个同类型的可写 ref。Vue 以与源数组相同的顺序传入当前值和旧值数组调用回调。
	/// Watches multiple same-typed writable refs. Vue invokes the callback with arrays of
	/// current and previous values in the same order as the source array.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 ref 源。The ref sources to watch.</param>
	/// <param name="callback">以当前值和旧值数组调用的回调。A callback invoked with current and previous value arrays.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// 使用显式侦听器选项侦听多个同类型的可写 ref。
	/// Watches multiple same-typed writable refs with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 ref 源。The ref sources to watch.</param>
	/// <param name="callback">以当前值和旧值数组调用的回调。A callback invoked with current and previous value arrays.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 侦听多个同类型的可写 ref 并向回调暴露 Vue 的清理注册函数。
	/// Watches multiple same-typed writable refs and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 ref 源。The ref sources to watch.</param>
	/// <param name="callback">支持清理的回调，以当前值和旧值数组调用。A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// 使用支持清理的回调和显式侦听器选项侦听多个同类型的可写 ref。
	/// Watches multiple same-typed writable refs with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 ref 源。The ref sources to watch.</param>
	/// <param name="callback">支持清理的回调，以当前值和旧值数组调用。A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 侦听多个同类型的只读 ref，例如计算 ref。
	/// Watches multiple same-typed readonly refs, such as computed refs.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的只读 ref 源。The readonly ref sources to watch.</param>
	/// <param name="callback">以当前值和旧值数组调用的回调。A callback invoked with current and previous value arrays.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// 使用显式侦听器选项侦听多个同类型的只读 ref。
	/// Watches multiple same-typed readonly refs with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的只读 ref 源。The readonly ref sources to watch.</param>
	/// <param name="callback">以当前值和旧值数组调用的回调。A callback invoked with current and previous value arrays.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 侦听多个同类型的只读 ref 并向回调暴露 Vue 的清理注册函数。
	/// Watches multiple same-typed readonly refs and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的只读 ref 源。The readonly ref sources to watch.</param>
	/// <param name="callback">支持清理的回调，以当前值和旧值数组调用。A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// 使用支持清理的回调和显式侦听器选项侦听多个同类型的只读 ref。
	/// Watches multiple same-typed readonly refs with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的只读 ref 源。The readonly ref sources to watch.</param>
	/// <param name="callback">支持清理的回调，以当前值和旧值数组调用。A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 侦听多个同类型的 getter 源。
	/// Watches multiple same-typed getter sources.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 getter 源。The getter sources to watch.</param>
	/// <param name="callback">以当前值和旧值数组调用的回调。A callback invoked with current and previous value arrays.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// 使用显式侦听器选项侦听多个同类型的 getter 源。
	/// Watches multiple same-typed getter sources with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 getter 源。The getter sources to watch.</param>
	/// <param name="callback">以当前值和旧值数组调用的回调。A callback invoked with current and previous value arrays.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 侦听多个同类型的 getter 源并向回调暴露 Vue 的清理注册函数。
	/// Watches multiple same-typed getter sources and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 getter 源。The getter sources to watch.</param>
	/// <param name="callback">支持清理的回调，以当前值和旧值数组调用。A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// 使用支持清理的回调和显式侦听器选项侦听多个同类型的 getter 源。
	/// Watches multiple same-typed getter sources with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">每个源产生的值类型。The value type produced by each source.</typeparam>
	/// <param name="sources">要侦听的 getter 源。The getter sources to watch.</param>
	/// <param name="callback">支持清理的回调，以当前值和旧值数组调用。A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">侦听器选项，如 <c>Immediate</c>、<c>Deep</c>、<c>Once</c> 和 <c>Flush</c>。Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// 立即运行副作用函数，并在其响应式依赖变更时重新运行。
	/// 与 <see cref="Watch{T}"/> 不同，此函数不接收新旧值——它只是重新执行整个副作用。
	/// Runs a side-effect function immediately and re-runs it whenever its reactive
	/// dependencies change. Unlike <see cref="Watch{T}"/>, this does not receive old/new
	/// values — it simply re-executes the entire effect.
	/// </summary>
	/// <param name="effect">要运行的副作用函数。内部访问的响应式值被追踪为依赖。The side-effect function to run. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect);

	/// <summary>
	/// 使用显式 effect 选项运行侦听器副作用。
	/// Runs a watcher effect with explicit effect options.
	/// </summary>
	/// <param name="effect">要运行的副作用函数。The side-effect function to run.</param>
	/// <param name="options">effect 选项，如 <c>Flush</c>。Effect options such as <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect, VueWatchEffectOptions options);

	/// <summary>
	/// 运行侦听器副作用并向回调暴露 Vue 的清理注册函数。
	/// Runs a watcher effect and exposes Vue's cleanup registration function.
	/// </summary>
	/// <param name="effect">支持清理的副作用函数。The cleanup-aware side-effect function to run.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// 使用支持清理的回调和显式 effect 选项运行侦听器副作用。
	/// Runs a cleanup-aware watcher effect with explicit effect options.
	/// </summary>
	/// <param name="effect">支持清理的副作用函数。The cleanup-aware side-effect function to run.</param>
	/// <param name="options">effect 选项，如 <c>Flush</c>。Effect options such as <c>Flush</c>.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(VueWatchEffectCallback effect, VueWatchEffectOptions options);

	/// <summary>
	/// 在组件更新刷新之后运行侦听器副作用。
	/// Runs a watcher effect after component updates flush.
	/// </summary>
	/// <param name="effect">要运行的副作用函数。The side-effect function to run.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchPostEffect")]
	public extern static VueWatchHandle WatchPostEffect(Action effect);

	/// <summary>
	/// 在组件更新刷新之后运行支持清理的侦听器副作用。
	/// Runs a cleanup-aware watcher effect after component updates flush.
	/// </summary>
	/// <param name="effect">支持清理的副作用函数。The cleanup-aware side-effect function to run.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchPostEffect")]
	public extern static VueWatchHandle WatchPostEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// 在依赖变更时同步运行侦听器副作用。
	/// Runs a watcher effect synchronously when dependencies change.
	/// </summary>
	/// <param name="effect">要运行的副作用函数。The side-effect function to run.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchSyncEffect")]
	public extern static VueWatchHandle WatchSyncEffect(Action effect);

	/// <summary>
	/// 在依赖变更时同步运行支持清理的侦听器副作用。
	/// Runs a cleanup-aware watcher effect synchronously when dependencies change.
	/// </summary>
	/// <param name="effect">支持清理的副作用函数。The cleanup-aware side-effect function to run.</param>
	/// <returns>可停止、暂停或恢复侦听器的侦听器句柄。A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchSyncEffect")]
	public extern static VueWatchHandle WatchSyncEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// 为当前活跃的侦听器注册清理回调。
	/// Registers a cleanup callback for the currently active watcher.
	/// </summary>
	/// <param name="cleanup">在侦听器重新运行或停止之前执行的清理工作。Cleanup work to execute before the watcher re-runs or stops.</param>
	[Description("@#onWatcherCleanup")]
	public extern static void OnWatcherCleanup(Action cleanup);

	/// <summary>
	/// 为当前活跃的侦听器注册清理回调。
	/// Registers a cleanup callback for the currently active watcher.
	/// </summary>
	/// <param name="cleanup">在侦听器重新运行或停止之前执行的清理工作。Cleanup work to execute before the watcher re-runs or stops.</param>
	/// <param name="failSilently">当为 <c>true</c> 时，Vue 会抑制缺少侦听器的警告。When <c>true</c>, Vue suppresses the missing-watcher warning.</param>
	[Description("@#onWatcherCleanup")]
	public extern static void OnWatcherCleanup(Action cleanup, bool failSilently);

	/// <summary>
	/// 将普通值、ref 或 getter 规范化为其当前值。此重载将普通值原样返回。
	/// Normalizes a plain value, ref, or getter into its current value. This overload
	/// returns plain values unchanged.
	/// </summary>
	/// <typeparam name="T">值的类型。The value type.</typeparam>
	/// <param name="value">要规范化的值。The value to normalize.</param>
	/// <returns>提供的值。The supplied value.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(T value);

	/// <summary>
	/// 将 ref 规范化为其当前值。
	/// Normalizes a ref into its current value.
	/// </summary>
	/// <typeparam name="T">ref 值的类型。The ref value type.</typeparam>
	/// <param name="value">要解包的 ref。The ref to unwrap.</param>
	/// <returns>ref 的当前值。The current ref value.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(IVueRef<T> value);

	/// <summary>
	/// 将 getter 规范化为其返回值。
	/// Normalizes a getter into its returned value.
	/// </summary>
	/// <typeparam name="T">getter 的返回类型。The getter return type.</typeparam>
	/// <param name="getter">通过 Vue 规范化语义调用的 getter。The getter to invoke through Vue normalization semantics.</param>
	/// <returns>getter 的结果。The getter result.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(Func<T> getter);

	/// <summary>
	/// 等待下一次 DOM 更新周期完成。在修改响应式状态后使用此方法，确保 DOM 已更新后再对渲染输出进行断言。
	/// Waits for the next DOM update cycle to complete. Use this after modifying reactive
	/// state to ensure the DOM has been updated before asserting on the rendered output.
	/// </summary>
	/// <returns>在 DOM 更新刷新后解析的 <see cref="PromiseResult"/>。A <see cref="PromiseResult"/> that resolves after the DOM update flush.</returns>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick();

	/// <summary>
	/// 等待下一次 DOM 更新周期并在刷新后运行回调。
	/// Waits for the next DOM update cycle and runs a callback after the flush.
	/// </summary>
	/// <param name="callback">Vue 在下一次 DOM 更新刷新后调用的回调。The callback Vue invokes after the next DOM update flush.</param>
	/// <returns>在回调运行后解析的 <see cref="PromiseResult"/>。A <see cref="PromiseResult"/> that resolves after the callback has run.</returns>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick(Action callback);

}
