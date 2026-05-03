using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	private const string IdentityInlineTemplate = "__arg1";

	private const string DefaultModelNameInlineTemplate = "\"modelValue\"";

	private const string ModelUpdateEventNameInlineTemplate = "`update:${__arg1}`";

	/// <summary>
	/// Returns the fallthrough attributes from the current setup context.
	/// </summary>
	/// <returns>The current component's fallthrough attribute bag.</returns>
	[Description("@#useAttrs")]
	public extern static VueAttributeBag UseAttrs();

	/// <summary>
	/// Returns the fallthrough attributes as a user-declared typed projection.
	/// This does not convert the runtime object; it only gives C# IntelliSense for
	/// known attribute keys.
	/// </summary>
	/// <typeparam name="TAttrs">The typed attribute projection record.</typeparam>
	/// <returns>The current component's fallthrough attributes projected as <typeparamref name="TAttrs"/>.</returns>
	[Description("@#useAttrs")]
	public extern static TAttrs UseAttrs<TAttrs>()
		where TAttrs : VueProps;

	/// <summary>
	/// Returns the slots object from the current setup context.
	/// </summary>
	/// <returns>The current component's slot bag.</returns>
	[Description("@#useSlots")]
	public extern static VueSlotBag UseSlots();

	/// <summary>
	/// Returns the slots object as a user-declared typed slot projection.
	/// </summary>
	/// <typeparam name="TSlots">The typed slots projection record.</typeparam>
	/// <returns>The current component's slots projected as <typeparamref name="TSlots"/>.</returns>
	[Description("@#useSlots")]
	public extern static TSlots UseSlots<TSlots>()
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a readonly template ref linked to a template <c>ref</c> key.
	/// </summary>
	/// <typeparam name="TElement">The element or component instance type expected for the ref.</typeparam>
	/// <param name="key">The template ref key.</param>
	/// <returns>A readonly ref whose value is populated after mount and reset on unmount.</returns>
	[Description("@#useTemplateRef")]
	public extern static VueReadonlyRef<TElement?> UseTemplateRef<TElement>(string key)
		where TElement : class;

	/// <summary>
	/// Generates a stable per-application unique id that is safe for SSR hydration.
	/// </summary>
	/// <returns>A unique id string for the current app instance.</returns>
	[Description("@#useId")]
	public extern static string UseId();

	/// <summary>
	/// Creates a writable model ref backed by a declared prop and its corresponding
	/// <c>update:*</c> event. The component must still declare the prop and emit entry.
	/// </summary>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="props">The setup props object supplied by Vue.</param>
	/// <param name="key">The final runtime prop key, such as <c>"modelValue"</c>.</param>
	/// <returns>A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TValue>(VueProps props, string key);

	/// <summary>
	/// Creates a writable model ref backed by a strongly typed model-name contract.
	/// The model wrapper still erases to the final runtime prop key string.
	/// </summary>
	/// <typeparam name="TProps">The typed props contract used by the current component.</typeparam>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="props">The setup props object supplied by Vue.</param>
	/// <param name="model">The typed model-name contract describing the prop key.</param>
	/// <returns>A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TProps, TValue>(TProps props, VueModelName<TProps, TValue> model)
		where TProps : VueProps;

	/// <summary>
	/// Creates a writable model ref with read/write transforms.
	/// </summary>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="props">The setup props object supplied by Vue.</param>
	/// <param name="key">The final runtime prop key, such as <c>"modelValue"</c>.</param>
	/// <param name="options">Read/write transforms applied by Vue's model helper.</param>
	/// <returns>A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TValue>(VueProps props, string key, VueModelOptions<TValue> options);

	/// <summary>
	/// Creates a writable model ref with read/write transforms using a strongly typed
	/// model-name contract.
	/// </summary>
	/// <typeparam name="TProps">The typed props contract used by the current component.</typeparam>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="props">The setup props object supplied by Vue.</param>
	/// <param name="model">The typed model-name contract describing the prop key.</param>
	/// <param name="options">Read/write transforms applied by Vue's model helper.</param>
	/// <returns>A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TProps, TValue>(TProps props, VueModelName<TProps, TValue> model, VueModelOptions<TValue> options)
		where TProps : VueProps;

	/// <summary>
	/// Creates the default Vue model-name contract for <c>modelValue</c>.
	/// This is a compile-time helper only and erases to the final runtime prop key.
	/// </summary>
	/// <typeparam name="TProps">The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <returns>A typed model-name contract for <c>modelValue</c>.</returns>
	[ECMAScriptInline(DefaultModelNameInlineTemplate)]
	public extern static VueModelName<TProps, TValue> ModelName<TProps, TValue>()
		where TProps : VueProps;

	/// <summary>
	/// Creates a strongly typed model-name contract for a named model prop.
	/// </summary>
	/// <typeparam name="TProps">The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="key">The final runtime prop key, such as <c>"count"</c>.</param>
	/// <returns>A typed model-name contract for the supplied runtime prop key.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static VueModelName<TProps, TValue> ModelName<TProps, TValue>(string key)
		where TProps : VueProps;

	/// <summary>
	/// Reads the runtime prop key from a typed model-name contract.
	/// This is useful when declaring <c>props</c> arrays without repeating string literals.
	/// </summary>
	/// <typeparam name="TProps">The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="model">The typed model-name contract.</param>
	/// <returns>The final runtime prop key.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static string ModelPropName<TProps, TValue>(VueModelName<TProps, TValue> model)
		where TProps : VueProps;

	/// <summary>
	/// Builds the corresponding <c>update:*</c> event name for a typed model-name contract.
	/// This keeps named-model event declarations aligned with the same source model key.
	/// </summary>
	/// <typeparam name="TProps">The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">The model value type.</typeparam>
	/// <param name="model">The typed model-name contract.</param>
	/// <returns>The final runtime update event name, such as <c>"update:count"</c>.</returns>
	[ECMAScriptInline(ModelUpdateEventNameInlineTemplate)]
	public extern static string ModelUpdateEventName<TProps, TValue>(VueModelName<TProps, TValue> model)
		where TProps : VueProps;

	/// <summary>
	/// Returns the current Vue custom element host while running inside a custom
	/// element setup context.
	/// </summary>
	/// <returns>The current custom element host, or <c>null</c> when unavailable.</returns>
	[Description("@#useHost")]
	public extern static HTMLElement? UseHost();

	/// <summary>
	/// Returns the current Vue custom element host projected to a typed host element.
	/// This is a typed projection only and does not create a new runtime wrapper.
	/// </summary>
	/// <typeparam name="THost">The expected custom element host type.</typeparam>
	/// <returns>The current custom element host, or <c>null</c> when unavailable.</returns>
	[Description("@#useHost")]
	public extern static THost? UseHost<THost>()
		where THost : HTMLElement;

	/// <summary>
	/// Returns the current Vue custom element shadow root while running inside a
	/// custom element setup context.
	/// </summary>
	/// <returns>The current custom element shadow root, or <c>null</c> when unavailable.</returns>
	[Description("@#useShadowRoot")]
	public extern static ShadowRoot? UseShadowRoot();

	/// <summary>
	/// Provides a value from the current component setup context to descendant components.
	/// </summary>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <param name="value">The value to provide.</param>
	[Description("@#provide")]
	public extern static void Provide<TValue>(string key, TValue value);

	/// <summary>
	/// Provides a value from the current component setup context using a strongly typed
	/// injection key.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <param name="value">The value to provide.</param>
	[Description("@#provide")]
	public extern static void Provide<TValue>(VueInjectionKey<TValue> key, TValue value);

	/// <summary>
	/// Injects a value from the nearest ancestor provider using a string key.
	/// </summary>
	/// <typeparam name="TValue">The expected value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <returns>The injected value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#inject")]
	public extern static TValue? Inject<TValue>(string key);

	/// <summary>
	/// Injects a value from the nearest ancestor provider using a strongly typed
	/// injection key.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <returns>The injected value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#inject")]
	public extern static TValue? Inject<TValue>(VueInjectionKey<TValue> key);

	/// <summary>
	/// Injects a value using a string key, returning a default value when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The expected value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <param name="defaultValue">The default value used when no provider exists.</param>
	/// <returns>The injected value or the supplied default value.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(string key, TValue defaultValue);

	/// <summary>
	/// Injects a value using a strongly typed injection key, returning a default value
	/// when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <param name="defaultValue">The default value used when no provider exists.</param>
	/// <returns>The injected value or the supplied default value.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(VueInjectionKey<TValue> key, TValue defaultValue);

	/// <summary>
	/// Injects a value using a string key, evaluating a default factory when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The expected value type.</typeparam>
	/// <param name="key">The injection key.</param>
	/// <param name="defaultFactory">Factory used when no provider exists.</param>
	/// <param name="treatDefaultAsFactory">Pass <c>true</c> so Vue treats the second argument as a factory.</param>
	/// <returns>The injected value or the factory result.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(string key, Func<TValue> defaultFactory, bool treatDefaultAsFactory = true);

	/// <summary>
	/// Injects a value using a strongly typed injection key, evaluating a default factory
	/// when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
	/// <param name="key">The typed injection key symbol.</param>
	/// <param name="defaultFactory">Factory used when no provider exists.</param>
	/// <param name="treatDefaultAsFactory">Pass <c>true</c> so Vue treats the second argument as a factory.</param>
	/// <returns>The injected value or the factory result.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(VueInjectionKey<TValue> key, Func<TValue> defaultFactory, bool treatDefaultAsFactory = true);

	/// <summary>
	/// Returns whether the current call stack has an active injection context.
	/// </summary>
	/// <returns><c>true</c> when <c>inject()</c> can be used without a warning.</returns>
	[Description("@#hasInjectionContext")]
	public extern static bool HasInjectionContext();

	/// <summary>
	/// Creates a new effect scope. Effects created inside the scope can be stopped together.
	/// </summary>
	/// <param name="detached">When <c>true</c>, create a detached scope not linked to the current active scope.</param>
	/// <returns>A new effect scope.</returns>
	[Description("@#effectScope")]
	public extern static VueEffectScope EffectScope(bool detached = false);

	/// <summary>
	/// Returns the currently active effect scope, if one exists.
	/// </summary>
	/// <returns>The current effect scope when available; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#getCurrentScope")]
	public extern static VueEffectScope? GetCurrentScope();

	/// <summary>
	/// Registers a cleanup callback on the current active effect scope.
	/// </summary>
	/// <param name="callback">The cleanup callback to run when the current scope is stopped.</param>
	[Description("@#onScopeDispose")]
	public extern static void OnScopeDispose(Action callback);

	/// <summary>
	/// Registers a cleanup callback on the current active effect scope.
	/// </summary>
	/// <param name="callback">The cleanup callback to run when the current scope is stopped.</param>
	/// <param name="failSilently">When <c>true</c>, Vue suppresses the missing-scope warning.</param>
	[Description("@#onScopeDispose")]
	public extern static void OnScopeDispose(Action callback, bool failSilently);

}
