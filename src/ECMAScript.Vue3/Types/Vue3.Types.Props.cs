using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// String-keyed event listener bag for render-function props. Keys are final Vue
	/// listener prop names such as <c>onClick</c>; values are no-payload handlers.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueEventHandlers : VueProps
	{
		/// <summary>
		/// Gets or sets an event listener by its final Vue listener prop key.
		/// </summary>
		/// <param name="key">The final emitted listener key, for example <c>onClick</c>.</param>
		/// <returns>The registered event listener.</returns>
		public extern Action? this[string key] { get; set; }
	}

	/// <summary>
	/// String-keyed event listener bag for render-function props with a typed event
	/// payload contract.
	/// </summary>
	/// <typeparam name="TEvent">The event payload supplied by Vue when the listener runs.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEventHandlers<TEvent> : VueEventHandlers
	{
		/// <summary>
		/// Gets or sets a typed event listener by its final Vue listener prop key.
		/// </summary>
		/// <param name="key">The final emitted listener key, for example <c>onMousemove</c>.</param>
		/// <returns>The registered typed event listener.</returns>
		public new extern VueEventHandler<TEvent>? this[string key] { get; set; }
	}

	/// <summary>
	/// Read-side fallthrough listener projection for <c>useAttrs()</c> / <c>context.attrs</c>.
	/// Use this when arbitrary <c>on*</c> keys should remain callable without defining
	/// one property per listener.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueAttributeListeners : VueProps
	{
		/// <summary>
		/// Reads a no-payload listener by its final Vue listener key, for example
		/// <c>onClick</c>.
		/// </summary>
		/// <param name="key">The final listener key to read.</param>
		/// <returns>The listener callback when present.</returns>
		public extern Action? this[string key] { get; set; }
	}

	/// <summary>
	/// Typed read-side fallthrough listener projection for <c>useAttrs()</c> /
	/// <c>context.attrs</c>.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type expected by each listener.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueAttributeListeners<TEvent> : VueAttributeListeners
	{
		/// <summary>
		/// Reads a typed listener by its final Vue listener key.
		/// </summary>
		/// <param name="key">The final listener key to read.</param>
		/// <returns>The typed listener callback when present.</returns>
		public new extern VueEventHandler<TEvent>? this[string key] { get; set; }
	}

	/// <summary>
	/// Object-form Vue prop declaration for a strongly typed prop value.
	/// Use <see cref="Type"/> for a single constructor, <see cref="Types"/> for a
	/// constructor array, and one of the default / validator members as needed.
	/// Members that map to the same Vue key are mutually exclusive by convention.
	/// </summary>
	/// <typeparam name="TValue">The prop value type accepted by setup/render code.</typeparam>
	public record VuePropOptions<TValue> : VueProps
	{
		/// <summary>
		/// Single JavaScript constructor used by Vue's runtime prop type check.
		/// </summary>
		[Description("@#type")]
		public VuePropType? Type { get; init; }

		/// <summary>
		/// Constructor array used by Vue's runtime prop type check. Elements may be
		/// <c>null</c> to express Vue's nullable type form.
		/// </summary>
		[Description("@#type")]
		public VuePropType?[]? Types { get; init; }

		/// <summary>
		/// Whether the prop must be supplied by the parent.
		/// </summary>
		[Description("@#required")]
		public bool? Required { get; init; }

		/// <summary>
		/// Literal default value used when the prop is absent.
		/// </summary>
		[Description("@#default")]
		public TValue? Default { get; init; }

		/// <summary>
		/// Factory default used when the prop is absent. Prefer this for object and
		/// array defaults so each component instance receives a fresh value.
		/// </summary>
		[Description("@#default")]
		public VuePropDefaultFactory<TValue>? DefaultFactory { get; init; }

		/// <summary>
		/// Factory default that receives the raw props object supplied to the component.
		/// </summary>
		[Description("@#default")]
		public VuePropRawPropsDefaultFactory<TValue>? DefaultFactoryWithProps { get; init; }

		/// <summary>
		/// Prop validator that observes only the current prop value.
		/// </summary>
		[Description("@#validator")]
		public VuePropValidator<TValue>? Validator { get; init; }

		/// <summary>
		/// Prop validator that also observes the raw props object supplied to the component.
		/// </summary>
		[Description("@#validator")]
		public VuePropRawPropsValidator<TValue>? ValidatorWithProps { get; init; }
	}

	/// <summary>
	/// Non-generic prop declaration for cases where the value contract is intentionally
	/// unknown-like but still typed as <see cref="VueValue"/> instead of <c>object</c>.
	/// </summary>
	public record VuePropOptions : VuePropOptions<VueValue>;

	/// <summary>
	/// String-keyed object-form props registry for declarations that share one value type.
	/// For heterogeneous prop values, declare a custom <see cref="VueProps"/> record with
	/// <see cref="VuePropOptions{TValue}"/> properties.
	/// </summary>
	/// <typeparam name="TValue">The prop value type used by all registry entries.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VuePropRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets one object-form prop declaration by final prop key.
		/// </summary>
		/// <param name="key">The final Vue prop key.</param>
		/// <returns>The declaration for the given prop key.</returns>
		public extern Either<VuePropType, VuePropType?[], VuePropOptions<TValue>>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropType type);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropType?[] types);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropOptions<TValue> options);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Non-generic object-form props registry using <see cref="VueValue"/> for each
	/// declaration's value contract.
	/// </summary>
	public record VuePropRegistry : VuePropRegistry<VueValue>;

	/// <summary>
	/// String-keyed object-form emits registry for no-payload validators.
	/// For plain event declarations without validators, prefer the existing
	/// array-form <c>EmitNames</c> surface.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for one-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for two-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	/// <typeparam name="T1">The second emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0, T1> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0, T1>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0, T1> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for three-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	/// <typeparam name="T1">The second emitted payload type.</typeparam>
	/// <typeparam name="T2">The third emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0, T1, T2> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0, T1, T2>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0, T1, T2> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// String-keyed object-form emits registry for four-payload validators.
	/// </summary>
	/// <typeparam name="T0">The first emitted payload type.</typeparam>
	/// <typeparam name="T1">The second emitted payload type.</typeparam>
	/// <typeparam name="T2">The third emitted payload type.</typeparam>
	/// <typeparam name="T3">The fourth emitted payload type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEmitRegistry<T0, T1, T2, T3> : VueProps, System.Collections.IEnumerable
	{
		public extern VueEmitValidator<T0, T1, T2, T3>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VueEmitValidator<T0, T1, T2, T3> validator);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// General-purpose Vue object authoring surface for <c>h()</c> props and root props.
	/// This remains a record so it participates in the compiler's structural object lowering.
	/// In addition to the common convenience members, it also exposes a string-keyed
	/// dictionary surface for direct object-literal authoring.
	/// </summary>
	public record VueObject : VueDictionary
	{
		/// <summary>
		/// Vue special <c>is</c> attribute for customized built-in elements.
		/// Dynamic components should use the component-valued <c>H(...)</c> overloads directly.
		/// </summary>
		[Description("@#is")]
		public string? Is { get; init; }

		/// <summary>
		/// Vue VNode <c>key</c>. Accepts string, number, or symbol values through
		/// <see cref="VueKey"/>.
		/// </summary>
		[Description("@#key")]
		public VueKey? Key { get; init; }

		/// <summary>
		/// Standard Vue <c>class</c> binding. Accepts string, string array, object forms, or
		/// mixed class arrays via <see cref="VueValue"/>.
		/// </summary>
		[Description("@#class")]
		public Either<string, string[], VueProps, VueValue[]>? Class { get; init; }

		/// <summary>
		/// Standard Vue <c>style</c> binding. Use a typed record or the convenience
		/// <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Description("@#style")]
		public VueProps? Style { get; init; }

		/// <summary>
		/// Named template ref key, intended to pair with <see cref="UseTemplateRef{TElement}(string)"/>.
		/// Callback and ref-object forms remain a separate typed authoring design surface.
		/// </summary>
		[Description("@#ref")]
		public string? Ref { get; init; }

		/// <summary>
		/// Standard <c>for</c> attribute, commonly used by labels to target form controls.
		/// </summary>
		[Description("@#for")]
		public string? For { get; init; }

		/// <summary>
		/// Standard <c>spellcheck</c> attribute.
		/// </summary>
		[Description("@#spellcheck")]
		public bool? Spellcheck { get; init; }

		/// <summary>
		/// Standard <c>rows</c> attribute, commonly used by textareas.
		/// </summary>
		[Description("@#rows")]
		public int? Rows { get; init; }

		/// <summary>
		/// Standard <c>cols</c> attribute, commonly used by textareas.
		/// </summary>
		[Description("@#cols")]
		public int? Cols { get; init; }

		/// <summary>
		/// Standard <c>value</c> attribute for element authoring convenience.
		/// </summary>
		[Description("@#value")]
		public string? Value { get; init; }

		/// <summary>
		/// Standard <c>min</c> attribute. Accepts numeric values or string literals such as
		/// date-like forms used by native inputs.
		/// </summary>
		[Description("@#min")]
		public Either<double, string>? Min { get; init; }

		/// <summary>
		/// Standard <c>max</c> attribute. Accepts numeric values or string literals such as
		/// date-like forms used by native inputs.
		/// </summary>
		[Description("@#max")]
		public Either<double, string>? Max { get; init; }

		/// <summary>
		/// Standard <c>step</c> attribute. Accepts numeric values or string literals such as
		/// <c>any</c>.
		/// </summary>
		[Description("@#step")]
		public Either<double, string>? Step { get; init; }

		/// <summary>
		/// Standard <c>minlength</c> attribute.
		/// </summary>
		[Description("@#minlength")]
		public int? MinLength { get; init; }

		/// <summary>
		/// Standard <c>maxlength</c> attribute.
		/// </summary>
		[Description("@#maxlength")]
		public int? MaxLength { get; init; }

		/// <summary>
		/// Standard <c>pattern</c> attribute.
		/// </summary>
		[Description("@#pattern")]
		public string? Pattern { get; init; }

		/// <summary>
		/// Standard <c>accept</c> attribute.
		/// </summary>
		[Description("@#accept")]
		public string? Accept { get; init; }

		/// <summary>
		/// Standard <c>wrap</c> attribute, commonly used by textareas.
		/// </summary>
		[Description("@#wrap")]
		public string? Wrap { get; init; }

		/// <summary>
		/// Standard <c>name</c> attribute.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Standard <c>type</c> attribute.
		/// </summary>
		[Description("@#type")]
		public string? Type { get; init; }

		/// <summary>
		/// Standard <c>placeholder</c> attribute.
		/// </summary>
		[Description("@#placeholder")]
		public string? Placeholder { get; init; }

		/// <summary>
		/// Standard <c>autocomplete</c> attribute.
		/// </summary>
		[Description("@#autocomplete")]
		public string? AutoComplete { get; init; }

		/// <summary>
		/// Standard <c>autofocus</c> attribute.
		/// </summary>
		[Description("@#autofocus")]
		public bool? AutoFocus { get; init; }

		/// <summary>
		/// Standard <c>disabled</c> attribute.
		/// </summary>
		[Description("@#disabled")]
		public bool? Disabled { get; init; }

		/// <summary>
		/// Standard <c>checked</c> attribute.
		/// </summary>
		[Description("@#checked")]
		public bool? Checked { get; init; }

		/// <summary>
		/// Standard <c>readonly</c> attribute.
		/// </summary>
		[Description("@#readonly")]
		public bool? ReadOnly { get; init; }

		/// <summary>
		/// Standard <c>required</c> attribute.
		/// </summary>
		[Description("@#required")]
		public bool? Required { get; init; }

		/// <summary>
		/// Standard <c>multiple</c> attribute.
		/// </summary>
		[Description("@#multiple")]
		public bool? Multiple { get; init; }

		/// <summary>
		/// Standard <c>selected</c> attribute.
		/// </summary>
		[Description("@#selected")]
		public bool? Selected { get; init; }

		/// <summary>
		/// Standard <c>tabindex</c> attribute.
		/// </summary>
		[Description("@#tabindex")]
		public int? TabIndex { get; init; }

		/// <summary>
		/// Standard <c>role</c> attribute.
		/// </summary>
		[Description("@#role")]
		public string? Role { get; init; }

		/// <summary>
		/// Standard <c>href</c> attribute.
		/// </summary>
		[Description("@#href")]
		public string? Href { get; init; }

		/// <summary>
		/// Standard <c>target</c> attribute.
		/// </summary>
		[Description("@#target")]
		public string? Target { get; init; }

		/// <summary>
		/// Standard <c>rel</c> attribute.
		/// </summary>
		[Description("@#rel")]
		public string? Rel { get; init; }

		/// <summary>
		/// Standard <c>src</c> attribute.
		/// </summary>
		[Description("@#src")]
		public string? Src { get; init; }

		/// <summary>
		/// Standard <c>alt</c> attribute.
		/// </summary>
		[Description("@#alt")]
		public string? Alt { get; init; }

		/// <summary>
		/// Standard <c>action</c> attribute, commonly used by forms.
		/// </summary>
		[Description("@#action")]
		public string? Action { get; init; }

		/// <summary>
		/// Standard <c>method</c> attribute, commonly used by forms.
		/// </summary>
		[Description("@#method")]
		public string? Method { get; init; }

		/// <summary>
		/// Event listeners flattened into the current Vue props object. Listener keys must
		/// be final Vue render-function prop names, such as <c>onClick</c>.
		/// </summary>
		[Spread]
		public VueEventHandlers? Events { get; init; }

		/// <summary>
		/// Standard <c>id</c> attribute.
		/// </summary>
		[Description("@#id")]
		public string? Id { get; init; }

		/// <summary>
		/// Standard <c>title</c> attribute.
		/// </summary>
		[Description("@#title")]
		public string? Title { get; init; }

		/// <summary>
		/// Additional properties to flatten directly into the current Vue object.
		/// Supports both typed records and <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Attrs { get; init; }

		/// <summary>
		/// Dataset attributes flattened into the current Vue object.
		/// Expected property names should already map to their final <c>data-*</c> keys.
		/// Supports both typed records and <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Dataset { get; init; }

		/// <summary>
		/// Raw attributes flattened into the current Vue object without additional Vue-specific
		/// interpretation. Supports both typed records and <see cref="VueDictionary"/>
		/// for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Raw { get; init; }
	}

	/// <summary>
	/// Typed Vue object authoring surface that can both flatten a typed props bag and carry
	/// the common convenience members declared on <see cref="VueObject"/>.
	/// </summary>
	/// <typeparam name="TProps">The typed props record that should be flattened into the output object.</typeparam>
	public record VueObject<TProps> : VueObject
		where TProps : VueProps
	{
		/// <summary>
		/// Typed props bag flattened into the current Vue object.
		/// </summary>
		[Spread]
		public TProps? Props { get; init; }
	}

	/// <summary>
	/// Base record for component slot declarations. This can be used directly as a
	/// string-keyed bag for parameterless slot callbacks, or inherited when a component
	/// wants a stronger typed slot contract. Maps to a plain JS object in Vue's
	/// <c>slots</c> option.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueSlots : IVueOptionsBag
	{
		/// <summary>
		/// Gets or sets a parameterless slot callback by its final emitted slot name.
		/// Scoped slots still require an explicit typed slot record with
		/// <see cref="VueSlotCallback{TScope}"/> properties.
		/// </summary>
		/// <param name="key">The final Vue slot name.</param>
		/// <returns>The parameterless slot callback registered for that name.</returns>
		public extern VueSlotCallback? this[string key] { get; set; }
	}

	/// <summary>
	/// Generic read/write slot projection for scoped slots that share one scope type.
	/// This can be used with <c>UseSlots&lt;TSlots&gt;()</c> to read runtime scoped slot
	/// callbacks without defining an explicit slot record for each key.
	/// </summary>
	/// <typeparam name="TScope">The scope payload type passed to each slot callback.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueScopedSlots<TScope> : VueSlots
	{
		/// <summary>
		/// Reads or writes a scoped slot callback by its final emitted slot name.
		/// </summary>
		/// <param name="key">The final Vue slot key.</param>
		/// <returns>The scoped slot callback registered for that name.</returns>
		public new extern VueSlotCallback<TScope>? this[string key] { get; set; }

		/// <summary>
		/// Reads or writes the default scoped slot callback.
		/// </summary>
		[Description("@#default")]
		public extern VueSlotCallback<TScope>? Default { get; set; }
	}

}
