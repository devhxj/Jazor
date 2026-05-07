using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// Base record for component definition objects passed to <c>defineComponent()</c>.
	/// Holds options shared by all component option shapes.
	/// </summary>
	public abstract record VueComponentDefinition : IVueOptionsBag
	{
		/// <summary>
		/// Controls whether fallthrough attributes are automatically applied to the
		/// component's root element.
		/// </summary>
		[Description("@#inheritAttrs")]
		public bool? InheritAttrs { get; init; }

		/// <summary>
		/// Option-form public instance expose declaration. Only listed member names are
		/// available through template refs on the component public instance.
		/// </summary>
		[Description("@#expose")]
		public string[]? Expose { get; init; }

		/// <summary>
		/// Options API provide object. Use a typed <see cref="VueProps"/> record or
		/// <see cref="VueDictionary"/> when the provide keys are dynamic or library-defined.
		/// Use <see cref="ProvideFactory"/> when Vue should call a function to create the
		/// provide object per component instance.
		/// </summary>
		[Description("@#provide")]
		public VueProps? Provide { get; init; }

		/// <summary>
		/// Options API function-form provide callback. Use this when the provided values
		/// should be produced lazily per component instance. For this-bound authoring,
		/// combine this property with <see cref="Vue3.BindThis{TThis}(VueThisDataCallback{TThis})"/>.
		/// </summary>
		[Description("@#provide")]
		public VueDataCallback? ProvideFactory { get; init; }

		/// <summary>
		/// Options API inject declaration. Array-form injection uses <c>string[]</c>;
		/// object-form injection can be expressed with a typed <see cref="VueProps"/>
		/// record or <see cref="VueDictionary"/>.
		/// </summary>
		[Description("@#inject")]
		public VueNamesOrOptions? Inject { get; init; }

		/// <summary>
		/// Local mixins merged into this component by Vue's Options API merge strategy.
		/// Prefer Composition API for new reusable logic; this property exists as a
		/// low-level compatibility binding for Vue options objects.
		/// </summary>
		[Description("@#mixins")]
		public VueComponentDefinition[]? Mixins { get; init; }

		/// <summary>
		/// Base component options object merged into this component by Vue's Options API
		/// <c>extends</c> strategy. This is a low-level compatibility binding rather than
		/// a C# inheritance model.
		/// </summary>
		[Description("@#extends")]
		public VueComponentDefinition? Extends { get; init; }

		/// <summary>
		/// Options API <c>data()</c> factory. Return a <see cref="VueProps"/> record so Vue
		/// receives a fresh plain object for each component instance. Instance-bound
		/// <c>data(vm)</c> / <c>this</c> authoring is intentionally left to the broader
		/// this-bound Options API design.
		/// </summary>
		[Description("@#data")]
		public VueDataCallback? Data { get; init; }

		/// <summary>
		/// Options API computed object. Use <see cref="VueComputedRegistry{TValue}"/> for
		/// dynamic keys with one value type, or a custom <see cref="VueProps"/> record for
		/// heterogeneous strongly typed computed declarations.
		/// </summary>
		[Description("@#computed")]
		public VueProps? Computed { get; init; }

		/// <summary>
		/// Options API methods object. Use <see cref="VueMethodRegistry{TDelegate}"/> for
		/// dynamic keys with one delegate signature, or a custom <see cref="VueProps"/>
		/// record for heterogeneous strongly typed method declarations.
		/// </summary>
		[Description("@#methods")]
		public VueProps? Methods { get; init; }

		/// <summary>
		/// Options API watch object. Use <see cref="VueWatchRegistry{TValue}"/> for dynamic
		/// keys that observe one value type, or a custom <see cref="VueProps"/> record for
		/// heterogeneous strongly typed watch declarations.
		/// </summary>
		[Description("@#watch")]
		public VueProps? Watch { get; init; }

		/// <summary>
		/// Options API hook invoked immediately after the component instance is initialized.
		/// This C# surface models the no-<c>this</c> callback form; this-bound Options API
		/// authoring remains a separate design problem.
		/// </summary>
		[Description("@#beforeCreate")]
		public Action? BeforeCreate { get; init; }

		/// <summary>
		/// Options API hook invoked after reactive state has been initialized.
		/// </summary>
		[Description("@#created")]
		public Action? Created { get; init; }

		/// <summary>
		/// Options API hook invoked right before the component is mounted.
		/// </summary>
		[Description("@#beforeMount")]
		public Action? BeforeMount { get; init; }

		/// <summary>
		/// Options API hook invoked after the component has been mounted.
		/// </summary>
		[Description("@#mounted")]
		public Action? Mounted { get; init; }

		/// <summary>
		/// Options API hook invoked right before a reactive update patches the DOM.
		/// </summary>
		[Description("@#beforeUpdate")]
		public Action? BeforeUpdate { get; init; }

		/// <summary>
		/// Options API hook invoked after a reactive update has patched the DOM.
		/// </summary>
		[Description("@#updated")]
		public Action? Updated { get; init; }

		/// <summary>
		/// Options API hook invoked right before the component is unmounted.
		/// </summary>
		[Description("@#beforeUnmount")]
		public Action? BeforeUnmount { get; init; }

		/// <summary>
		/// Options API hook invoked after the component has been unmounted.
		/// </summary>
		[Description("@#unmounted")]
		public Action? Unmounted { get; init; }

		/// <summary>
		/// Options API hook invoked when a kept-alive component is inserted back into the DOM.
		/// </summary>
		[Description("@#activated")]
		public Action? Activated { get; init; }

		/// <summary>
		/// Options API hook invoked when a kept-alive component is removed from the DOM cache outlet.
		/// </summary>
		[Description("@#deactivated")]
		public Action? Deactivated { get; init; }

		/// <summary>
		/// Options API hook invoked when an error from a descendant component is captured.
		/// Return <c>false</c> to stop propagation according to Vue runtime semantics.
		/// </summary>
		[Description("@#errorCaptured")]
		public VueErrorCapturedCallback? ErrorCaptured { get; init; }

		/// <summary>
		/// Development-only Options API hook invoked when a reactive dependency is tracked during render.
		/// </summary>
		[Description("@#renderTracked")]
		public VueDebuggerCallback? RenderTracked { get; init; }

		/// <summary>
		/// Development-only Options API hook invoked when a reactive dependency triggers a render update.
		/// </summary>
		[Description("@#renderTriggered")]
		public VueDebuggerCallback? RenderTriggered { get; init; }

		/// <summary>
		/// Server-rendering hook invoked before the component is rendered on the server.
		/// </summary>
		[Description("@#serverPrefetch")]
		public VueServerPrefetchPromiseCallback? ServerPrefetch { get; init; }
	}

	/// <summary>
	/// Registry of child components that the current component can use in its template.
	/// This can be used directly as a string-keyed bag, or inherited when a library wants
	/// a more strongly-typed registry surface. Maps to Vue's <c>components</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueComponentRegistry : IVueOptionsBag
	{
		/// <summary>
		/// Gets or sets a component registration by its final emitted name.
		/// </summary>
		/// <param name="key">The final Vue component registration name.</param>
		/// <returns>The component registered for that name.</returns>
		public extern IVueComponent? this[string key] { get; set; }
	}

	/// <summary>
	/// Registry of custom directives that the current component can use in its template.
	/// This can be used directly as a string-keyed bag, or inherited when a library wants
	/// a more strongly-typed registry surface. Maps to Vue's <c>directives</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDirectiveRegistry : IVueOptionsBag, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets a directive registration by its final emitted name.
		/// </summary>
		/// <param name="key">The final Vue directive registration name.</param>
		/// <returns>The directive registered for that name.</returns>
		public extern VueDirective? this[string key] { get; set; }

		/// <summary>
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
		/// Gets or sets an arbitrary plugin option by its final emitted key.
		/// </summary>
		/// <param name="key">The final JavaScript object key to emit.</param>
		/// <returns>The option value mapped to the given key.</returns>
		public extern VueValue? this[string key] { get; set; }
	}

	/// <summary>
	/// Options API computed registry for computed properties that share one value type.
	/// For heterogeneous computed values, declare a custom <see cref="VueProps"/> record
	/// with typed properties instead.
	/// </summary>
	/// <typeparam name="TValue">The computed property value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueComputedRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets a computed property declaration by its final emitted key.
		/// Values can be getter callbacks or writable computed get/set options.
		/// </summary>
		/// <param name="key">The final computed property key.</param>
		/// <returns>The computed declaration for the given key.</returns>
		public extern VueComputedValue<TValue> this[string key] { get; set; }

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of getter-form computed entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, Func<TValue> getter);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of writable computed entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWritableComputedOptions<TValue> options);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Options API method registry for methods that share one delegate signature.
	/// For heterogeneous method signatures, declare a custom <see cref="VueProps"/>
	/// record with typed delegate properties instead.
	/// </summary>
	/// <typeparam name="TDelegate">The method delegate signature.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueMethodRegistry<TDelegate> : VueProps, System.Collections.IEnumerable
		where TDelegate : Delegate
	{
		/// <summary>
		/// Gets or sets a method declaration by its final emitted key.
		/// </summary>
		/// <param name="key">The final method key.</param>
		/// <returns>The method delegate registered for the given key.</returns>
		public extern TDelegate? this[string key] { get; set; }

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of method entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, TDelegate method);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Single Options API watch declaration entry. This wrapper keeps
	/// watch handler unions strongly typed while allowing natural C#
	/// assignments through implicit conversions.
	/// </summary>
	/// <typeparam name="TValue">The watched value type.</typeparam>
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
	/// Array-form Options API watch declaration entries. Vue runtime accepts
	/// watch value arrays that mix method-name, callback, and object-form
	/// handlers; this wrapper models that surface without requiring compiler
	/// special casing.
	/// </summary>
	/// <typeparam name="TValue">The watched value type.</typeparam>
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
	/// Options API watch registry for watch declarations that share one observed value type.
	/// For heterogeneous watched value types, declare a custom <see cref="VueProps"/> record
	/// with typed watch declaration properties instead.
	/// </summary>
	/// <typeparam name="TValue">The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueWatchRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets a watch declaration by its final emitted key. Keys can be property
		/// names or Vue-supported simple dot paths.
		/// </summary>
		/// <param name="key">The final watch source key.</param>
		/// <returns>The watch declaration for the given key.</returns>
		public extern VueWatchDeclaration<TValue> this[string key] { get; set; }

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of method-name watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, string methodName);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of callback watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, Action<TValue, TValue> handler);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of cleanup-aware watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchCleanupCallback<TValue> handler);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of callback watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchHandlerOptions<TValue> options);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of cleanup-aware watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchCleanupHandlerOptions<TValue> options);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of method-name watch options.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchNamedHandlerOptions options);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of array-form watch entries.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueWatchEntries<TValue> entries);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Object-form Options API inject declaration for one injected value type.
	/// Vue accepts either a source key string/symbol or an object with
	/// <c>from</c> / <c>default</c>; this helper models the object form while keeping
	/// typed injection keys, default literals, and default factories strongly typed.
	/// </summary>
	/// <typeparam name="TValue">The injected value contract.</typeparam>
	public record VueInjectOptions<TValue> : IVueOptionsBag
	{
		/// <summary>
		/// Source injection key to resolve. When omitted, Vue uses the local object key.
		/// Accepts the final string key, a raw JavaScript <see cref="Symbol"/>, or a
		/// strongly typed <see cref="VueInjectionKey{TValue}"/>.
		/// </summary>
		[Description("@#from")]
		public VueInjectFrom<TValue>? From { get; init; }

		/// <summary>
		/// Default value used when no provider matches.
		/// </summary>
		[Description("@#default")]
		public TValue? Default { get; init; }

		/// <summary>
		/// Factory default used when no provider matches.
		/// </summary>
		[Description("@#default")]
		public Func<TValue>? DefaultFactory { get; init; }
	}

	/// <summary>
	/// Non-generic inject options using <see cref="VueValue"/> for the injected value contract.
	/// </summary>
	public record VueInjectOptions : VueInjectOptions<VueValue>;

	/// <summary>
	/// Single Options API inject entry. This wrapper keeps object-form inject authoring
	/// ergonomic for both typed custom records and string-keyed registries.
	/// </summary>
	/// <typeparam name="TValue">The injected value contract.</typeparam>
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
	/// Non-generic inject entry using <see cref="VueValue"/> for the injected value contract.
	/// </summary>
	public class VueInjectEntry : VueInjectEntry<VueValue>;

	/// <summary>
	/// String-keyed object-form Options API inject registry for declarations that share
	/// one injected value contract.
	/// </summary>
	/// <typeparam name="TValue">The injected value type used by all entries.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueInjectRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets one object-form inject declaration by its local injection key.
		/// </summary>
		/// <param name="key">The local inject property key.</param>
		/// <returns>The declaration for the given key.</returns>
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
	/// Non-generic object-form inject registry using <see cref="VueValue"/> for each entry.
	/// </summary>
	public record VueInjectRegistry : VueInjectRegistry<VueValue>;

	/// <summary>
	/// Custom-element-specific options accepted by <c>defineCustomElement()</c> as
	/// its second argument. Normal component options remain authored through
	/// <see cref="VueComponentDefinition"/> and its typed variants.
	/// </summary>
	public record VueCustomElementOptions : IVueOptionsBag
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with no typed props or slots. Use this variant
	/// for simple components that rely on untyped props or have no props at all.
	/// </summary>
	public record VueComponentOptions : VueComponentDefinition
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives no props and
		/// must return a <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueSetupCallback? Setup { get; init; }

		/// <summary>
		/// Render function called directly to produce the component's VNode tree. This is
		/// an alternative to <see cref="Setup"/>; if both are provided, <c>render</c> takes
		/// precedence over the setup return value.
		/// </summary>
		[Description("@#render")]
		public VueRenderCallback? Render { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed props. The generic parameter drives
	/// C# setup and <c>h(...)</c> type checking; runtime <c>props</c> / <c>emits</c>
	/// declarations should be supplied explicitly through the option members when needed.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	public record VueComponentOptions<TProps> : VueComponentDefinition
		where TProps : VueProps
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a setup context, and must return a <see cref="VueRenderCallback"/> that produces
		/// the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with both typed props and typed slots. The
	/// generic parameters drive C# setup, slot, and <c>h(...)</c> type checking; runtime
	/// <c>props</c> / <c>emits</c> declarations remain explicit option members.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public record VueComponentOptions<TProps, TSlots> : VueComponentDefinition
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a typed setup context (with typed slot access), and must return a
		/// <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps, TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed slots but no typed props. Use this
	/// variant for components that accept named slots but do not declare typed props.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public record VueSlotComponentOptions<TSlots> : VueComponentDefinition
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Explicit Vue <c>props</c> declaration, using either array-form names or
		/// object-form validators/defaults/type checks.
		/// </summary>
		[Description("@#props")]
		public VueNamesOrOptions? Props { get; init; }

		/// <summary>
		/// Explicit Vue <c>emits</c> declaration, using either array-form event names or
		/// object-form validators.
		/// </summary>
		[Description("@#emits")]
		public VueNamesOrOptions? Emits { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives a typed setup
		/// context with typed slot access, and must return a <see cref="VueRenderCallback"/>
		/// that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSlotSetupCallback<TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// normal untyped component options with custom-element-only options such as
	/// <c>styles</c> and <c>shadowRoot</c>.
	/// </summary>
	public record VueCustomElementComponentOptions : VueComponentOptions
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-props component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TProps">The props contract accepted by the custom element component.</typeparam>
	public record VueCustomElementComponentOptions<TProps> : VueComponentOptions<TProps>
		where TProps : VueProps
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-props/typed-slots component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TProps">The props contract accepted by the custom element component.</typeparam>
	/// <typeparam name="TSlots">The slots contract accepted by the custom element component.</typeparam>
	public record VueCustomElementComponentOptions<TProps, TSlots> : VueComponentOptions<TProps, TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// One-argument <c>defineCustomElement(...)</c> authoring surface that merges
	/// typed-slots component options with custom-element-only options.
	/// </summary>
	/// <typeparam name="TSlots">The slots contract accepted by the custom element component.</typeparam>
	public record VueCustomElementSlotComponentOptions<TSlots> : VueSlotComponentOptions<TSlots>
		where TSlots : VueSlots
	{
		/// <summary>
		/// CSS strings injected into the custom element's shadow root.
		/// </summary>
		[Description("@#styles")]
		public string[]? Styles { get; init; }

		/// <summary>
		/// Callback used to configure the internally created Vue application before mount.
		/// </summary>
		[Description("@#configureApp")]
		public VueCustomElementConfigureAppCallback? ConfigureApp { get; init; }

		/// <summary>
		/// Controls whether Vue attaches a shadow root for this custom element.
		/// Set <c>false</c> for light-DOM rendering.
		/// </summary>
		[Description("@#shadowRoot")]
		public bool? ShadowRoot { get; init; }

		/// <summary>
		/// Native shadow-root initialization options forwarded when Vue creates the
		/// element's shadow root.
		/// </summary>
		[Description("@#shadowRootOptions")]
		public ShadowRootInit? ShadowRootOptions { get; init; }

		/// <summary>
		/// Nonce applied to injected style tags for Content Security Policy support.
		/// </summary>
		[Description("@#nonce")]
		public string? Nonce { get; init; }
	}

	/// <summary>
	/// Options for <c>defineAsyncComponent()</c>. Vue accepts either a loader function
	/// directly or this object form for loading/error components, timing, suspense, and
	/// retry behavior.
	/// </summary>
	public record VueAsyncComponentOptions : IVueOptionsBag
	{
		/// <summary>
		/// Function that loads and resolves the component definition.
		/// </summary>
		[Description("@#loader")]
		public VueAsyncComponentLoader Loader { get; init; } = default!;

		/// <summary>
		/// Component rendered while the async component is loading.
		/// </summary>
		[Description("@#loadingComponent")]
		public ECMAScript.Vue3.IVueComponent? LoadingComponent { get; init; }

		/// <summary>
		/// Component rendered when the async component fails to load.
		/// </summary>
		[Description("@#errorComponent")]
		public ECMAScript.Vue3.IVueComponent? ErrorComponent { get; init; }

		/// <summary>
		/// Delay in milliseconds before showing the loading component.
		/// </summary>
		[Description("@#delay")]
		public Number? Delay { get; init; }

		/// <summary>
		/// Timeout in milliseconds before Vue treats loading as failed.
		/// </summary>
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		/// <summary>
		/// Whether the async component can participate in a parent <c>Suspense</c> boundary.
		/// </summary>
		[Description("@#suspensible")]
		public bool? Suspensible { get; init; }

		/// <summary>
		/// Callback invoked when loading fails; it can retry or fail the async load.
		/// </summary>
		[Description("@#onError")]
		public VueAsyncComponentErrorCallback? OnError { get; init; }
	}

	/// <summary>
	/// Strongly typed options for <c>defineAsyncComponent()</c>. The generic component
	/// contract is preserved by the returned async component reference.
	/// </summary>
	/// <typeparam name="TComponent">The component contract produced by the loader.</typeparam>
	public record VueAsyncComponentOptions<TComponent> : IVueOptionsBag
		where TComponent : ECMAScript.Vue3.IVueComponent
	{
		/// <summary>
		/// Function that loads and resolves the typed component definition.
		/// </summary>
		[Description("@#loader")]
		public VueAsyncComponentLoader<TComponent> Loader { get; init; } = default!;

		/// <summary>
		/// Component rendered while the async component is loading.
		/// </summary>
		[Description("@#loadingComponent")]
		public ECMAScript.Vue3.IVueComponent? LoadingComponent { get; init; }

		/// <summary>
		/// Component rendered when the async component fails to load.
		/// </summary>
		[Description("@#errorComponent")]
		public ECMAScript.Vue3.IVueComponent? ErrorComponent { get; init; }

		/// <summary>
		/// Delay in milliseconds before showing the loading component.
		/// </summary>
		[Description("@#delay")]
		public Number? Delay { get; init; }

		/// <summary>
		/// Timeout in milliseconds before Vue treats loading as failed.
		/// </summary>
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		/// <summary>
		/// Whether the async component can participate in a parent <c>Suspense</c> boundary.
		/// </summary>
		[Description("@#suspensible")]
		public bool? Suspensible { get; init; }

		/// <summary>
		/// Callback invoked when loading fails; it can retry or fail the async load.
		/// </summary>
		[Description("@#onError")]
		public VueAsyncComponentErrorCallback? OnError { get; init; }
	}

}
