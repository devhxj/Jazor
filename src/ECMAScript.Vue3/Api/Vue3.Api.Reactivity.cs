using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// Creates a deep reactive proxy of an object. Vue recursively converts all nested
	/// properties into reactive getters/setters, so reads and writes at any depth are tracked.
	/// </summary>
	/// <typeparam name="T">The type of the object to make reactive (must be a reference type).</typeparam>
	/// <param name="value">The plain object to wrap in a reactive proxy.</param>
	/// <returns>A reactive proxy of the same type. All property accesses are tracked.</returns>
	[Description("@#reactive")]
	public extern static T Reactive<T>(T value) where T : class;

	/// <summary>
	/// Creates a readonly proxy of a reactive (or plain) object. Attempts to write to
	/// properties on the returned object will trigger a runtime warning and be ignored.
	/// </summary>
	/// <typeparam name="T">The type of the object to make readonly (must be a reference type).</typeparam>
	/// <param name="value">The object to wrap in a readonly proxy. Can be a reactive proxy or a plain object.</param>
	/// <returns>A readonly proxy of the same type. Reads are tracked; writes are blocked.</returns>
	[Description("@#readonly")]
	public extern static T Readonly<T>(T value) where T : class;

	/// <summary>
	/// Creates a shallow reactive proxy of an object.
	/// </summary>
	/// <typeparam name="T">The object type to wrap.</typeparam>
	/// <param name="value">The object to wrap.</param>
	/// <returns>A shallow reactive proxy of the same type.</returns>
	[Description("@#shallowReactive")]
	public extern static T ShallowReactive<T>(T value) where T : class;

	/// <summary>
	/// Creates a shallow readonly proxy of an object.
	/// </summary>
	/// <typeparam name="T">The object type to wrap.</typeparam>
	/// <param name="value">The object to wrap.</param>
	/// <returns>A shallow readonly proxy of the same type.</returns>
	[Description("@#shallowReadonly")]
	public extern static T ShallowReadonly<T>(T value) where T : class;

	/// <summary>
	/// Returns the raw object behind a Vue proxy.
	/// </summary>
	/// <typeparam name="T">The static object type.</typeparam>
	/// <param name="value">The proxy value.</param>
	/// <returns>The original raw object.</returns>
	[Description("@#toRaw")]
	public extern static T ToRaw<T>(T value) where T : class;

	/// <summary>
	/// Marks an object so Vue will never convert it to a proxy.
	/// </summary>
	/// <typeparam name="T">The object type to mark.</typeparam>
	/// <param name="value">The object to mark as raw.</param>
	/// <returns>The same object.</returns>
	[Description("@#markRaw")]
	public extern static T MarkRaw<T>(T value) where T : class;

	/// <summary>
	/// Returns whether a value is any Vue-created proxy.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is a Vue proxy.</returns>
	[Description("@#isProxy")]
	public extern static bool IsProxy<T>(T value);

	/// <summary>
	/// Returns whether a value is a reactive proxy.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is reactive.</returns>
	[Description("@#isReactive")]
	public extern static bool IsReactive<T>(T value);

	/// <summary>
	/// Returns whether a value is a readonly proxy.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is readonly.</returns>
	[Description("@#isReadonly")]
	public extern static bool IsReadonly<T>(T value);

	/// <summary>
	/// Creates a reactive ref wrapping a single value. Unlike <see cref="Reactive{T}"/>,
	/// <c>ref()</c> wraps the entire value, not its properties. Access the value via
	/// <see cref="IVueRef{T}.Value"/>.
	/// </summary>
	/// <typeparam name="T">The type of the value to wrap.</typeparam>
	/// <param name="value">The initial value of the ref.</param>
	/// <returns>A reactive ref whose <c>Value</c> property reads and writes the wrapped value.</returns>
	[Description("@#ref")]
	public extern static IVueRef<T> Ref<T>(T value);

	/// <summary>
	/// Creates a shallow reactive ref that only tracks replacements of <c>Value</c>, not
	/// mutations of the value itself. Use this for large objects where deep tracking is
	/// unnecessary or when the value is replaced wholesale.
	/// </summary>
	/// <typeparam name="T">The type of the value to wrap.</typeparam>
	/// <param name="value">The initial value of the shallow ref.</param>
	/// <returns>A shallow ref whose <c>Value</c> property only triggers on replacement, not on deep mutation.</returns>
	[Description("@#shallowRef")]
	public extern static IVueRef<T> ShallowRef<T>(T value);

	/// <summary>
	/// Forces effects depending on a shallow ref to re-run.
	/// </summary>
	/// <typeparam name="T">The type of the ref value.</typeparam>
	/// <param name="value">The ref to trigger.</param>
	[Description("@#triggerRef")]
	public extern static void TriggerRef<T>(IVueRef<T> value);

	/// <summary>
	/// Creates a custom ref whose dependency tracking and triggering are controlled by
	/// user-provided get/set handlers.
	/// </summary>
	/// <typeparam name="T">The custom ref value type.</typeparam>
	/// <param name="factory">Factory receiving Vue's track/trigger callbacks and returning get/set handlers.</param>
	/// <returns>A reactive ref controlled by the supplied factory.</returns>
	[Description("@#customRef")]
	public extern static IVueRef<T> CustomRef<T>(VueCustomRefFactory<T> factory);

	/// <summary>
	/// Returns whether a runtime value is a Vue ref.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is a ref.</returns>
	[Description("@#isRef")]
	public extern static bool IsRef<T>(T value);

	/// <summary>
	/// Returns a normal value unchanged.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The value to normalize.</param>
	/// <returns>The supplied value.</returns>
	[Description("@#unref")]
	public extern static T Unref<T>(T value);

	/// <summary>
	/// Unwraps a Vue ref to its current value.
	/// </summary>
	/// <typeparam name="T">The ref value type.</typeparam>
	/// <param name="value">The ref to unwrap.</param>
	/// <returns>The current ref value.</returns>
	[Description("@#unref")]
	public extern static T Unref<T>(IVueRef<T> value);

	/// <summary>
	/// Normalizes a plain value into a ref.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The value to wrap.</param>
	/// <returns>A ref for the supplied value.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<T> ToRef<T>(T value);

	/// <summary>
	/// Returns an existing ref unchanged.
	/// </summary>
	/// <typeparam name="T">The ref value type.</typeparam>
	/// <param name="value">The ref to normalize.</param>
	/// <returns>The supplied ref.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<T> ToRef<T>(IVueRef<T> value);

	/// <summary>
	/// Normalizes a getter into a readonly ref.
	/// </summary>
	/// <typeparam name="T">The getter result type.</typeparam>
	/// <param name="getter">The getter to wrap.</param>
	/// <returns>A readonly ref backed by the supplied getter.</returns>
	[Description("@#toRef")]
	public extern static VueReadonlyRef<T> ToRef<T>(Func<T> getter);

	/// <summary>
	/// Creates a linked ref for a property on a reactive object. The value type is
	/// explicit because C# cannot infer it from a string key.
	/// </summary>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <typeparam name="TValue">The linked property value type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <param name="key">The final runtime property name.</param>
	/// <returns>A ref linked to <paramref name="source"/>[<paramref name="key"/>].</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TSource, TValue>(TSource source, string key)
		where TSource : class;

	/// <summary>
	/// Creates a linked ref for a property on a reactive object, using a default value
	/// when the property is absent.
	/// </summary>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <typeparam name="TValue">The linked property value type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <param name="key">The final runtime property name.</param>
	/// <param name="defaultValue">The value Vue uses when the source property is absent.</param>
	/// <returns>A ref linked to <paramref name="source"/>[<paramref name="key"/>].</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TSource, TValue>(TSource source, string key, TValue defaultValue)
		where TSource : class;

	/// <summary>
	/// Creates a linked ref for a key in a dictionary-shaped Vue object.
	/// </summary>
	/// <typeparam name="TValue">The dictionary value type.</typeparam>
	/// <param name="source">The source dictionary-shaped object.</param>
	/// <param name="key">The final runtime property name.</param>
	/// <returns>A ref linked to the dictionary entry.</returns>
	[Description("@#toRef")]
	public extern static IVueRef<TValue> ToRef<TValue>(VueDictionary<TValue> source, string key);

	/// <summary>
	/// Converts each enumerable property on a reactive object into a linked ref.
	/// </summary>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <returns>An indexer-based refs bag.</returns>
	[Description("@#toRefs")]
	public extern static VueRefs<TSource> ToRefs<TSource>(TSource source)
		where TSource : class;

	/// <summary>
	/// Converts a props-style object into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">The user-declared refs projection type.</typeparam>
	/// <param name="source">The source props-style object.</param>
	/// <returns>The typed refs projection returned by Vue.</returns>
	[Description("@#toRefs")]
	public extern static TRefs ToRefs<TRefs>(VueProps source)
		where TRefs : VueRefs;

	/// <summary>
	/// Converts an arbitrary reactive object into a user-declared typed refs projection.
	/// </summary>
	/// <typeparam name="TRefs">The user-declared refs projection type.</typeparam>
	/// <typeparam name="TSource">The source object type.</typeparam>
	/// <param name="source">The source reactive object.</param>
	/// <returns>The typed refs projection returned by Vue.</returns>
	[Description("@#toRefs")]
	public extern static TRefs ToRefs<TRefs, TSource>(TSource source)
		where TRefs : VueRefs<TSource>
		where TSource : class;

	/// <summary>
	/// Creates a computed reactive value derived from a getter function. The getter is
	/// evaluated lazily and cached; it is re-evaluated only when its reactive dependencies
	/// change. The returned ref is readonly.
	/// </summary>
	/// <typeparam name="T">The type of the computed value.</typeparam>
	/// <param name="getter">A function that computes the derived value. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>A readonly ref whose <c>Value</c> is the latest computed result.</returns>
	[Description("@#computed")]
	public extern static VueReadonlyRef<T> Computed<T>(Func<T> getter);

	/// <summary>
	/// Creates a writable computed ref from explicit get/set delegates.
	/// </summary>
	/// <typeparam name="T">The computed value type.</typeparam>
	/// <param name="options">Plain Vue computed options containing <c>get</c> and <c>set</c>.</param>
	/// <returns>A writable computed ref.</returns>
	[Description("@#computed")]
	public extern static IVueRef<T> Computed<T>(VueWritableComputedOptions<T> options);

	/// <summary>
	/// Watches a reactive source and calls the callback when it changes. The callback
	/// receives both the new value and the previous value. Returns a handle that can be
	/// called to stop the watcher.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch. Called on each evaluation cycle.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c> whenever the source's return value changes.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback);

	/// <summary>
	/// Watches a getter source with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a getter source and exposes Vue's cleanup registration function to the callback.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// Watches a getter source with cleanup-aware callback and explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a reactive object source directly. Vue implicitly treats this as a deep
	/// watcher over the object's reactive graph.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, Action<TSource, TSource> callback)
		where TSource : class;

	/// <summary>
	/// Watches a reactive object source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, Action<TSource, TSource> callback, VueWatchOptions options)
		where TSource : class;

	/// <summary>
	/// Watches a reactive object source directly and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, VueWatchCleanupCallback<TSource> callback)
		where TSource : class;

	/// <summary>
	/// Watches a reactive object source directly with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="TSource">The reactive object type.</typeparam>
	/// <param name="source">The reactive object to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<TSource>(TSource source, VueWatchCleanupCallback<TSource> callback, VueWatchOptions options)
		where TSource : class;

	/// <summary>
	/// Watches a ref source directly.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, Action<T, T> callback);

	/// <summary>
	/// Watches a ref source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a ref source directly and exposes Vue's cleanup registration function to the callback.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// Watches a ref source directly with cleanup-aware callback and explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type stored in the ref.</typeparam>
	/// <param name="source">The ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a readonly ref source directly.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, Action<T, T> callback);

	/// <summary>
	/// Watches a readonly ref source directly with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c>.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, Action<T, T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches a readonly ref source directly and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, VueWatchCleanupCallback<T> callback);

	/// <summary>
	/// Watches a readonly ref source directly with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The type exposed by the readonly ref.</typeparam>
	/// <param name="source">The readonly ref to watch.</param>
	/// <param name="callback">A callback invoked with value, previous value, and cleanup registration.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T> source, VueWatchCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed writable refs. Vue invokes the callback with arrays of
	/// current and previous values in the same order as the source array.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed writable refs with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed writable refs and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed writable refs with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(IVueRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed readonly refs, such as computed refs.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed readonly refs with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed readonly refs and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed readonly refs with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The readonly ref sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(VueReadonlyRef<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed getter sources.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed getter sources with explicit watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Watches multiple same-typed getter sources and exposes Vue's cleanup registration
	/// function to the callback.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCleanupCallback<T> callback);

	/// <summary>
	/// Watches multiple same-typed getter sources with cleanup-aware callback and explicit
	/// watcher options.
	/// </summary>
	/// <typeparam name="T">The value type produced by each source.</typeparam>
	/// <param name="sources">The getter sources to watch.</param>
	/// <param name="callback">A cleanup-aware callback invoked with current and previous value arrays.</param>
	/// <param name="options">Watcher options such as <c>Immediate</c>, <c>Deep</c>, <c>Once</c>, and <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T>[] sources, VueWatchSourcesCleanupCallback<T> callback, VueWatchOptions options);

	/// <summary>
	/// Runs a side-effect function immediately and re-runs it whenever its reactive
	/// dependencies change. Unlike <see cref="Watch{T}"/>, this does not receive old/new
	/// values — it simply re-executes the entire effect.
	/// </summary>
	/// <param name="effect">The side-effect function to run. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect);

	/// <summary>
	/// Runs a watcher effect with explicit effect options.
	/// </summary>
	/// <param name="effect">The side-effect function to run.</param>
	/// <param name="options">Effect options such as <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect, VueWatchEffectOptions options);

	/// <summary>
	/// Runs a watcher effect and exposes Vue's cleanup registration function.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// Runs a cleanup-aware watcher effect with explicit effect options.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <param name="options">Effect options such as <c>Flush</c>.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(VueWatchEffectCallback effect, VueWatchEffectOptions options);

	/// <summary>
	/// Runs a watcher effect after component updates flush.
	/// </summary>
	/// <param name="effect">The side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchPostEffect")]
	public extern static VueWatchHandle WatchPostEffect(Action effect);

	/// <summary>
	/// Runs a cleanup-aware watcher effect after component updates flush.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchPostEffect")]
	public extern static VueWatchHandle WatchPostEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// Runs a watcher effect synchronously when dependencies change.
	/// </summary>
	/// <param name="effect">The side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchSyncEffect")]
	public extern static VueWatchHandle WatchSyncEffect(Action effect);

	/// <summary>
	/// Runs a cleanup-aware watcher effect synchronously when dependencies change.
	/// </summary>
	/// <param name="effect">The cleanup-aware side-effect function to run.</param>
	/// <returns>A watcher handle that can stop, pause, or resume the watcher.</returns>
	[Description("@#watchSyncEffect")]
	public extern static VueWatchHandle WatchSyncEffect(VueWatchEffectCallback effect);

	/// <summary>
	/// Registers a cleanup callback for the currently active watcher.
	/// </summary>
	/// <param name="cleanup">Cleanup work to execute before the watcher re-runs or stops.</param>
	[Description("@#onWatcherCleanup")]
	public extern static void OnWatcherCleanup(Action cleanup);

	/// <summary>
	/// Registers a cleanup callback for the currently active watcher.
	/// </summary>
	/// <param name="cleanup">Cleanup work to execute before the watcher re-runs or stops.</param>
	/// <param name="failSilently">When <c>true</c>, Vue suppresses the missing-watcher warning.</param>
	[Description("@#onWatcherCleanup")]
	public extern static void OnWatcherCleanup(Action cleanup, bool failSilently);

	/// <summary>
	/// Normalizes a plain value, ref, or getter into its current value. This overload
	/// returns plain values unchanged.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The value to normalize.</param>
	/// <returns>The supplied value.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(T value);

	/// <summary>
	/// Normalizes a ref into its current value.
	/// </summary>
	/// <typeparam name="T">The ref value type.</typeparam>
	/// <param name="value">The ref to unwrap.</param>
	/// <returns>The current ref value.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(IVueRef<T> value);

	/// <summary>
	/// Normalizes a getter into its returned value.
	/// </summary>
	/// <typeparam name="T">The getter return type.</typeparam>
	/// <param name="getter">The getter to invoke through Vue normalization semantics.</param>
	/// <returns>The getter result.</returns>
	[Description("@#toValue")]
	public extern static T ToValue<T>(Func<T> getter);

	/// <summary>
	/// Waits for the next DOM update cycle to complete. Use this after modifying reactive
	/// state to ensure the DOM has been updated before asserting on the rendered output.
	/// </summary>
	/// <returns>A <see cref="PromiseResult"/> that resolves after the DOM update flush.</returns>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick();

	/// <summary>
	/// Waits for the next DOM update cycle and runs a callback after the flush.
	/// </summary>
	/// <param name="callback">The callback Vue invokes after the next DOM update flush.</param>
	/// <returns>A <see cref="PromiseResult"/> that resolves after the callback has run.</returns>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick(Action callback);

}
