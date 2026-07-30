using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue render-function props、attrs、events 和 slots 的结构化类型分片。</summary>
/// <remarks>这些类型用于静态键和值域约束，最终直接落入 h()/组件调用的对象形状。</remarks>
public static partial class Vue3
{
	/// <summary>
	/// 渲染函数属性用的字符串键事件侦听器集合。键为最终 Vue 侦听器属性名（如 <c>onClick</c>），值为无载荷回调。
	/// String-keyed event listener bag for render-function props. Keys are final Vue
	/// listener prop names such as <c>onClick</c>; values are no-payload handlers.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueEventHandlers : VueProps
	{
		/// <summary>
		/// 通过最终 Vue 侦听器属性键获取或设置事件侦听器。
		/// Gets or sets an event listener by its final Vue listener prop key.
		/// </summary>
		/// <param name="key">最终的事件侦听器键，例如 <c>onClick</c>。</param>
		/// <returns>已注册的事件侦听器。</returns>
		public extern Action? this[string key] { get; set; }
	}

	/// <summary>
	/// 带类型化事件载荷契约的渲染函数属性用字符串键事件侦听器集合。
	/// String-keyed event listener bag for render-function props with a typed event
	/// payload contract.
	/// </summary>
	/// <typeparam name="TEvent">侦听器运行时 Vue 提供的事件载荷类型。</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueEventHandlers<TEvent> : VueEventHandlers
	{
		/// <summary>
		/// 通过最终 Vue 侦听器属性键获取或设置类型化事件侦听器。
		/// Gets or sets a typed event listener by its final Vue listener prop key.
		/// </summary>
		/// <param name="key">最终的事件侦听器键，例如 <c>onMousemove</c>。</param>
		/// <returns>已注册的类型化事件侦听器。</returns>
		public new extern VueEventHandler<TEvent>? this[string key] { get; set; }
	}

	/// <summary>
	/// 用于 <c>useAttrs()</c> / <c>context.attrs</c> 的只读端透传侦听器投影。当任意 <c>on*</c> 键应保持可调用而不需要为每个侦听器定义单独属性时使用。
	/// Read-side fallthrough listener projection for <c>useAttrs()</c> / <c>context.attrs</c>.
	/// Use this when arbitrary <c>on*</c> keys should remain callable without defining
	/// one property per listener.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueAttributeListeners : VueProps
	{
		/// <summary>
		/// 通过最终 Vue 侦听器键（如 <c>onClick</c>）读取无载荷侦听器。
		/// Reads a no-payload listener by its final Vue listener key, for example
		/// <c>onClick</c>.
		/// </summary>
		/// <param name="key">要读取的最终侦听器键。</param>
		/// <returns>存在时返回侦听器回调。</returns>
		public extern Action? this[string key] { get; set; }
	}

	/// <summary>
	/// 用于 <c>useAttrs()</c> / <c>context.attrs</c> 的类型化只读端透传侦听器投影。
	/// Typed read-side fallthrough listener projection for <c>useAttrs()</c> /
	/// <c>context.attrs</c>.
	/// </summary>
	/// <typeparam name="TEvent">每个侦听器所期望的事件载荷类型。</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueAttributeListeners<TEvent> : VueAttributeListeners
	{
		/// <summary>
		/// 通过最终 Vue 侦听器键读取类型化侦听器。
		/// Reads a typed listener by its final Vue listener key.
		/// </summary>
		/// <param name="key">要读取的最终侦听器键。</param>
		/// <returns>存在时返回类型化侦听器回调。</returns>
		public new extern VueEventHandler<TEvent>? this[string key] { get; set; }
	}

	/// <summary>
	/// 显式 Vue 选项声明，接受数组形式的名称列表或对象形式的 <see cref="VueProps"/> 记录。此包装器保留了集合表达式编写方式（如 <c>["title"]</c>），适用于 <c>Props</c>、<c>Emits</c> 和 <c>Inject</c> 等规范成员。
	/// Explicit Vue option declaration that accepts either an array-form name list or
	/// an object-form <see cref="VueProps"/> record.
	/// This wrapper preserves collection-expression authoring such as <c>["title"]</c>
	/// on canonical members like <c>Props</c>, <c>Emits</c>, and <c>Inject</c>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	[CollectionBuilder(typeof(VueNamesOrOptionsCollectionBuilder), nameof(VueNamesOrOptionsCollectionBuilder.Create))]
	public readonly union VueNamesOrOptions(string[], VueProps) : IEnumerable<string>
	{
		/// <summary>
		/// 当此值由字符串集合创建时，获取数组形式的名称。
		/// Gets the array-form names when this value was created from a string collection.
		/// </summary>
		public string[]? AsNames => Value as string[];

		/// <summary>
		/// 当此值由 Vue 选项记录创建时，获取对象形式的选项。
		/// Gets the object-form options when this value was created from a Vue options record.
		/// </summary>
		public VueProps? AsOptions => Value as VueProps;

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
			=> ((IEnumerable<string>)(AsNames ?? Array.Empty<string>())).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<string>)this).GetEnumerator();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class VueNamesOrOptionsCollectionBuilder
	{
		public static VueNamesOrOptions Create(ReadOnlySpan<string> items)
			=> items.ToArray();
	}

	/// <summary>
	/// 强类型属性值的对象形式 Vue 属性声明。使用 <see cref="Type"/> 指定单个构造函数，<see cref="Types"/> 指定构造函数数组，并按需使用默认值/验证器成员。映射到同一 Vue 键的成员按约定互斥。
	/// Object-form Vue prop declaration for a strongly typed prop value.
	/// Use <see cref="Type"/> for a single constructor, <see cref="Types"/> for a
	/// constructor array, and one of the default / validator members as needed.
	/// Members that map to the same Vue key are mutually exclusive by convention.
	/// </summary>
	/// <typeparam name="TValue">setup/render 代码所接受的属性值类型。</typeparam>
	public record VuePropOptions<TValue> : VueProps
	{
		/// <summary>
		/// Vue 运行时属性类型检查所使用的单个 JavaScript 构造函数。
		/// Single JavaScript constructor used by Vue's runtime prop type check.
		/// </summary>
		[Description("@#type")]
		public VuePropType? Type { get; init; }

		/// <summary>
		/// Vue 运行时属性类型检查所使用的构造函数数组。元素可为 <c>null</c> 以表示 Vue 的可空类型形式。
		/// Constructor array used by Vue's runtime prop type check. Elements may be
		/// <c>null</c> to express Vue's nullable type form.
		/// </summary>
		[Description("@#type")]
		public VuePropType?[]? Types { get; init; }

		/// <summary>
		/// 该属性是否必须由父组件提供。
		/// Whether the prop must be supplied by the parent.
		/// </summary>
		[Description("@#required")]
		public bool? Required { get; init; }

		/// <summary>
		/// 属性缺失时使用的字面量默认值。
		/// Literal default value used when the prop is absent.
		/// </summary>
		[Description("@#default")]
		public TValue? Default { get; init; }

		/// <summary>
		/// 属性缺失时使用的工厂默认值。对于对象和数组默认值优先使用此选项，以便每个组件实例获得一个新值。
		/// Factory default used when the prop is absent. Prefer this for object and
		/// array defaults so each component instance receives a fresh value.
		/// </summary>
		[Description("@#default")]
		public VuePropDefaultFactory<TValue>? DefaultFactory { get; init; }

		/// <summary>
		/// 接收传递给组件的原始属性对象的工厂默认值。
		/// Factory default that receives the raw props object supplied to the component.
		/// </summary>
		[Description("@#default")]
		public VuePropRawPropsDefaultFactory<TValue>? DefaultFactoryWithProps { get; init; }

		/// <summary>
		/// 仅观察当前属性值的属性验证器。
		/// Prop validator that observes only the current prop value.
		/// </summary>
		[Description("@#validator")]
		public VuePropValidator<TValue>? Validator { get; init; }

		/// <summary>
		/// 同时观察传递给组件的原始属性对象的属性验证器。
		/// Prop validator that also observes the raw props object supplied to the component.
		/// </summary>
		[Description("@#validator")]
		public VuePropRawPropsValidator<TValue>? ValidatorWithProps { get; init; }
	}

	/// <summary>
	/// 非泛型属性声明，用于值契约有意为 unknown 类但仍类型化为 <see cref="VueValue"/> 而非 <c>object</c> 的场景。
	/// Non-generic prop declaration for cases where the value contract is intentionally
	/// unknown-like but still typed as <see cref="VueValue"/> instead of <c>object</c>.
	/// </summary>
	public record VuePropOptions : VuePropOptions<VueValue>;

	/// <summary>
	/// 共享同一值类型的字符串键对象形式属性注册表。对于异构属性值，请声明自定义 <see cref="VueProps"/> 记录并使用 <see cref="VuePropOptions{TValue}"/> 属性。
	/// String-keyed object-form props registry for declarations that share one value type.
	/// For heterogeneous prop values, declare a custom <see cref="VueProps"/> record with
	/// <see cref="VuePropOptions{TValue}"/> properties.
	/// </summary>
	/// <typeparam name="TValue">所有注册表条目使用的属性值类型。</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VuePropRegistry<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// 通过最终属性键获取或设置一个对象形式的属性声明。
		/// Gets or sets one object-form prop declaration by final prop key.
		/// </summary>
		/// <param name="key">最终的 Vue 属性键。</param>
		/// <returns>给定属性键对应的声明。</returns>
		public extern VuePropDeclaration<TValue>? this[string key] { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropType type);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropType?[] types);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, VuePropOptions<TValue> options);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// 使用 <see cref="VueValue"/> 作为每个声明的值契约的非泛型对象形式属性注册表。
	/// Non-generic object-form props registry using <see cref="VueValue"/> for each
	/// declaration's value contract.
	/// </summary>
	public record VuePropRegistry : VuePropRegistry<VueValue>;

	/// <summary>
	/// 无载荷验证器的字符串键对象形式事件注册表。对于不带验证器的普通事件声明，优先使用已有的数组形式 <c>Emits</c> 接口。
	/// String-keyed object-form emits registry for no-payload validators.
	/// For plain event declarations without validators, prefer the existing
	/// array-form <c>Emits</c> surface.
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
	/// 单载荷验证器的字符串键对象形式事件注册表。
	/// String-keyed object-form emits registry for one-payload validators.
	/// </summary>
	/// <typeparam name="T0">第一个事件载荷类型。</typeparam>
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
	/// 双载荷验证器的字符串键对象形式事件注册表。
	/// String-keyed object-form emits registry for two-payload validators.
	/// </summary>
	/// <typeparam name="T0">第一个事件载荷类型。</typeparam>
	/// <typeparam name="T1">第二个事件载荷类型。</typeparam>
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
	/// 三载荷验证器的字符串键对象形式事件注册表。
	/// String-keyed object-form emits registry for three-payload validators.
	/// </summary>
	/// <typeparam name="T0">第一个事件载荷类型。</typeparam>
	/// <typeparam name="T1">第二个事件载荷类型。</typeparam>
	/// <typeparam name="T2">第三个事件载荷类型。</typeparam>
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
	/// 四载荷验证器的字符串键对象形式事件注册表。
	/// String-keyed object-form emits registry for four-payload validators.
	/// </summary>
	/// <typeparam name="T0">第一个事件载荷类型。</typeparam>
	/// <typeparam name="T1">第二个事件载荷类型。</typeparam>
	/// <typeparam name="T2">第三个事件载荷类型。</typeparam>
	/// <typeparam name="T3">第四个事件载荷类型。</typeparam>
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
	/// 用于 <c>h()</c> 属性和根属性的通用 Vue 对象编写界面。保持为 record 以参与编译器的结构化对象 lowering。除了常用便捷成员外，还公开了用于直接对象字面量编写的字符串键字典接口。
	/// General-purpose Vue object authoring surface for <c>h()</c> props and root props.
	/// This remains a record so it participates in the compiler's structural object lowering.
	/// In addition to the common convenience members, it also exposes a string-keyed
	/// dictionary surface for direct object-literal authoring.
	/// </summary>
	public record VueObject : VueDictionary
	{
		/// <summary>
		/// Vue 特殊 <c>is</c> 属性，用于自定义内置元素。动态组件应直接使用组件值的 <c>H(...)</c> 重载。
		/// Vue special <c>is</c> attribute for customized built-in elements.
		/// Dynamic components should use the component-valued <c>H(...)</c> overloads directly.
		/// </summary>
		[Description("@#is")]
		public string? Is { get; init; }

		/// <summary>
		/// Vue VNode <c>key</c>。通过 <see cref="VueKey"/> 接受字符串、数字或符号值。
		/// Vue VNode <c>key</c>. Accepts string, number, or symbol values through
		/// <see cref="VueKey"/>.
		/// </summary>
		[Description("@#key")]
		public VueKey? Key { get; init; }

		/// <summary>
		/// 标准 Vue <c>class</c> 绑定。通过 <see cref="VueValue"/> 接受字符串、字符串数组、对象形式或混合类数组。
		/// Standard Vue <c>class</c> binding. Accepts string, string array, object forms, or
		/// mixed class arrays via <see cref="VueValue"/>.
		/// </summary>
		[Description("@#class")]
		public VueClassValue? Class { get; init; }

		/// <summary>
		/// 标准 Vue <c>style</c> 绑定。使用类型化记录或便捷的 <see cref="VueDictionary"/> 处理任意键。
		/// Standard Vue <c>style</c> binding. Accepts string, object forms, or mixed
		/// arrays through <see cref="VueStyleValue"/>.
		/// </summary>
		[Description("@#style")]
		public VueStyleValue? Style { get; init; }

		/// <summary>
		/// 命名模板引用键，旨在与 <see cref="UseTemplateRef{TElement}(string)"/> 配对使用。回调和 ref 对象形式仍是独立的类型化编写设计界面。
		/// Named template ref key, intended to pair with <see cref="UseTemplateRef{TElement}(string)"/>.
		/// Callback and ref-object forms remain a separate typed authoring design surface.
		/// </summary>
		[Description("@#ref")]
		public string? Ref { get; init; }

		/// <summary>
		/// 标准 <c>for</c> 属性，通常由标签用于关联表单控件。
		/// Standard <c>for</c> attribute, commonly used by labels to target form controls.
		/// </summary>
		[Description("@#for")]
		public string? For { get; init; }

		/// <summary>
		/// 标准 <c>spellcheck</c> 属性。
		/// Standard <c>spellcheck</c> attribute.
		/// </summary>
		[Description("@#spellcheck")]
		public bool? Spellcheck { get; init; }

		/// <summary>
		/// 标准 <c>rows</c> 属性，通常由文本域使用。
		/// Standard <c>rows</c> attribute, commonly used by textareas.
		/// </summary>
		[Description("@#rows")]
		public int? Rows { get; init; }

		/// <summary>
		/// 标准 <c>cols</c> 属性，通常由文本域使用。
		/// Standard <c>cols</c> attribute, commonly used by textareas.
		/// </summary>
		[Description("@#cols")]
		public int? Cols { get; init; }

		/// <summary>
		/// 标准 <c>value</c> 属性，用于元素编写便捷。
		/// Standard <c>value</c> attribute for element authoring convenience.
		/// </summary>
		[Description("@#value")]
		public string? Value { get; init; }

		/// <summary>
		/// 标准 <c>min</c> 属性。接受数字值或字符串字面量（如原生输入使用的日期形式）。
		/// Standard <c>min</c> attribute. Accepts numeric values or string literals such as
		/// date-like forms used by native inputs.
		/// </summary>
		[Description("@#min")]
		public VueStringNumberValue? Min { get; init; }

		/// <summary>
		/// 标准 <c>max</c> 属性。接受数字值或字符串字面量（如原生输入使用的日期形式）。
		/// Standard <c>max</c> attribute. Accepts numeric values or string literals such as
		/// date-like forms used by native inputs.
		/// </summary>
		[Description("@#max")]
		public VueStringNumberValue? Max { get; init; }

		/// <summary>
		/// 标准 <c>step</c> 属性。接受数字值或字符串字面量（如 <c>any</c>）。
		/// Standard <c>step</c> attribute. Accepts numeric values or string literals such as
		/// <c>any</c>.
		/// </summary>
		[Description("@#step")]
		public VueStringNumberValue? Step { get; init; }

		/// <summary>
		/// 标准 <c>minlength</c> 属性。
		/// Standard <c>minlength</c> attribute.
		/// </summary>
		[Description("@#minlength")]
		public int? Minlength { get; init; }

		/// <summary>
		/// 标准 <c>maxlength</c> 属性。
		/// Standard <c>maxlength</c> attribute.
		/// </summary>
		[Description("@#maxlength")]
		public int? Maxlength { get; init; }

		/// <summary>
		/// 标准 <c>pattern</c> 属性。
		/// Standard <c>pattern</c> attribute.
		/// </summary>
		[Description("@#pattern")]
		public string? Pattern { get; init; }

		/// <summary>
		/// 标准 <c>accept</c> 属性。
		/// Standard <c>accept</c> attribute.
		/// </summary>
		[Description("@#accept")]
		public string? Accept { get; init; }

		/// <summary>
		/// 标准 <c>wrap</c> 属性，通常由文本域使用。
		/// Standard <c>wrap</c> attribute, commonly used by textareas.
		/// </summary>
		[Description("@#wrap")]
		public string? Wrap { get; init; }

		/// <summary>
		/// 标准 <c>name</c> 属性。
		/// Standard <c>name</c> attribute.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// 标准 <c>type</c> 属性。
		/// Standard <c>type</c> attribute.
		/// </summary>
		[Description("@#type")]
		public string? Type { get; init; }

		/// <summary>
		/// 标准 <c>placeholder</c> 属性。
		/// Standard <c>placeholder</c> attribute.
		/// </summary>
		[Description("@#placeholder")]
		public string? Placeholder { get; init; }

		/// <summary>
		/// 标准 <c>autocomplete</c> 属性。
		/// Standard <c>autocomplete</c> attribute.
		/// </summary>
		[Description("@#autocomplete")]
		public string? Autocomplete { get; init; }

		/// <summary>
		/// 标准 <c>autofocus</c> 属性。
		/// Standard <c>autofocus</c> attribute.
		/// </summary>
		[Description("@#autofocus")]
		public bool? Autofocus { get; init; }

		/// <summary>
		/// 标准 <c>disabled</c> 属性。
		/// Standard <c>disabled</c> attribute.
		/// </summary>
		[Description("@#disabled")]
		public bool? Disabled { get; init; }

		/// <summary>
		/// 标准 <c>checked</c> 属性。
		/// Standard <c>checked</c> attribute.
		/// </summary>
		[Description("@#checked")]
		public bool? Checked { get; init; }

		/// <summary>
		/// 标准 <c>readonly</c> 属性。
		/// Standard <c>readonly</c> attribute.
		/// </summary>
		[Description("@#readonly")]
		public bool? Readonly { get; init; }

		/// <summary>
		/// 标准 <c>required</c> 属性。
		/// Standard <c>required</c> attribute.
		/// </summary>
		[Description("@#required")]
		public bool? Required { get; init; }

		/// <summary>
		/// 标准 <c>multiple</c> 属性。
		/// Standard <c>multiple</c> attribute.
		/// </summary>
		[Description("@#multiple")]
		public bool? Multiple { get; init; }

		/// <summary>
		/// 标准 <c>selected</c> 属性。
		/// Standard <c>selected</c> attribute.
		/// </summary>
		[Description("@#selected")]
		public bool? Selected { get; init; }

		/// <summary>
		/// 标准 <c>tabindex</c> 属性。
		/// Standard <c>tabindex</c> attribute.
		/// </summary>
		[Description("@#tabindex")]
		public int? Tabindex { get; init; }

		/// <summary>
		/// 标准 <c>role</c> 属性。
		/// Standard <c>role</c> attribute.
		/// </summary>
		[Description("@#role")]
		public string? Role { get; init; }

		/// <summary>
		/// 标准 <c>href</c> 属性。
		/// Standard <c>href</c> attribute.
		/// </summary>
		[Description("@#href")]
		public string? Href { get; init; }

		/// <summary>
		/// 标准 <c>target</c> 属性。
		/// Standard <c>target</c> attribute.
		/// </summary>
		[Description("@#target")]
		public string? Target { get; init; }

		/// <summary>
		/// 标准 <c>rel</c> 属性。
		/// Standard <c>rel</c> attribute.
		/// </summary>
		[Description("@#rel")]
		public string? Rel { get; init; }

		/// <summary>
		/// 标准 <c>src</c> 属性。
		/// Standard <c>src</c> attribute.
		/// </summary>
		[Description("@#src")]
		public string? Src { get; init; }

		/// <summary>
		/// 标准 <c>alt</c> 属性。
		/// Standard <c>alt</c> attribute.
		/// </summary>
		[Description("@#alt")]
		public string? Alt { get; init; }

		/// <summary>
		/// 标准 <c>action</c> 属性，通常由表单使用。
		/// Standard <c>action</c> attribute, commonly used by forms.
		/// </summary>
		[Description("@#action")]
		public string? Action { get; init; }

		/// <summary>
		/// 标准 <c>method</c> 属性，通常由表单使用。
		/// Standard <c>method</c> attribute, commonly used by forms.
		/// </summary>
		[Description("@#method")]
		public string? Method { get; init; }

		/// <summary>
		/// 展平到当前 Vue 属性对象中的事件侦听器。侦听器键必须为最终的 Vue 渲染函数属性名，如 <c>onClick</c>。
		/// Event listeners flattened into the current Vue props object. Listener keys must
		/// be final Vue render-function prop names, such as <c>onClick</c>.
		/// </summary>
		[Spread]
		public VueEventHandlers? Events { get; init; }

		/// <summary>
		/// 标准 <c>id</c> 属性。
		/// Standard <c>id</c> attribute.
		/// </summary>
		[Description("@#id")]
		public string? Id { get; init; }

		/// <summary>
		/// 标准 <c>title</c> 属性。
		/// Standard <c>title</c> attribute.
		/// </summary>
		[Description("@#title")]
		public string? Title { get; init; }

		/// <summary>
		/// 直接展平到当前 Vue 对象中的附加属性。支持类型化记录和用于任意键的 <see cref="VueDictionary"/>。
		/// Additional properties to flatten directly into the current Vue object.
		/// Supports both typed records and <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Attrs { get; init; }

		/// <summary>
		/// 展平到当前 Vue 对象中的数据集属性。属性名应已映射到其最终的 <c>data-*</c> 键。支持类型化记录和用于任意键的 <see cref="VueDictionary"/>。
		/// Dataset attributes flattened into the current Vue object.
		/// Expected property names should already map to their final <c>data-*</c> keys.
		/// Supports both typed records and <see cref="VueDictionary"/> for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Dataset { get; init; }

		/// <summary>
		/// 不经额外 Vue 特定解释直接展平到当前 Vue 对象中的原始属性。支持类型化记录和用于任意键的 <see cref="VueDictionary"/>。
		/// Raw attributes flattened into the current Vue object without additional Vue-specific
		/// interpretation. Supports both typed records and <see cref="VueDictionary"/>
		/// for arbitrary keys.
		/// </summary>
		[Spread]
		public VueProps? Raw { get; init; }
	}

	/// <summary>
	/// 可同时展平类型化属性包并携带 <see cref="VueObject"/> 上声明的常用便捷成员的类型化 Vue 对象编写界面。
	/// Typed Vue object authoring surface that can both flatten a typed props bag and carry
	/// the common convenience members declared on <see cref="VueObject"/>.
	/// </summary>
	/// <typeparam name="TProps">应展平到输出对象中的类型化属性记录。</typeparam>
	public record VueObject<TProps> : VueObject
		where TProps : VueProps
	{
		/// <summary>
		/// 展平到当前 Vue 对象中的类型化属性包。
		/// Typed props bag flattened into the current Vue object.
		/// </summary>
		[Spread]
		public TProps? Props { get; init; }
	}

	/// <summary>
	/// 组件插槽声明的基记录。可直接用作无参数插槽回调的字符串键集合，或在组件需要更强的类型化插槽契约时被继承。映射到 Vue <c>slots</c> 选项中的普通 JS 对象。
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
		/// 通过最终插槽名获取或设置无参数插槽回调。作用域插槽仍需使用带 <see cref="VueSlotCallback{TScope}"/> 属性的显式类型化插槽记录。
		/// Gets or sets a parameterless slot callback by its final emitted slot name.
		/// Scoped slots still require an explicit typed slot record with
		/// <see cref="VueSlotCallback{TScope}"/> properties.
		/// </summary>
		/// <param name="key">最终的 Vue 插槽名。</param>
		/// <returns>为该名称注册的无参数插槽回调。</returns>
		public extern VueSlotCallback? this[string key] { get; set; }
	}

	/// <summary>
	/// 共享同一作用域类型的通用读/写作用域插槽投影。可与 <c>UseSlots&lt;TSlots&gt;()</c> 一起使用，以读取运行时作用域插槽回调而无需为每个键定义显式插槽记录。
	/// Generic read/write slot projection for scoped slots that share one scope type.
	/// This can be used with <c>UseSlots&lt;TSlots&gt;()</c> to read runtime scoped slot
	/// callbacks without defining an explicit slot record for each key.
	/// </summary>
	/// <typeparam name="TScope">传递给每个插槽回调的作用域载荷类型。</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueScopedSlots<TScope> : VueSlots
	{
		/// <summary>
		/// 通过最终插槽名读取或写入作用域插槽回调。
		/// Reads or writes a scoped slot callback by its final emitted slot name.
		/// </summary>
		/// <param name="key">最终的 Vue 插槽键。</param>
		/// <returns>为该名称注册的作用域插槽回调。</returns>
		public new extern VueSlotCallback<TScope>? this[string key] { get; set; }

		/// <summary>
		/// 读取或写入默认作用域插槽回调。
		/// Reads or writes the default scoped slot callback.
		/// </summary>
		[Description("@#default")]
		public extern VueSlotCallback<TScope>? Default { get; set; }
	}

}
