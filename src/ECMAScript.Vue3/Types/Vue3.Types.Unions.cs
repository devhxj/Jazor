using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// Options API computed 属性声明的联合类型。可以是一个 getter 回调或可写计算属性的 get/set 选项。
	/// Union type for Options API computed property declarations. Can be a getter callback or writable computed get/set options.
	/// </summary>
	/// <typeparam name="TValue">计算属性值的类型。The computed property value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueComputedValue<TValue>(Func<TValue>, VueWritableComputedOptions<TValue>)
	{
		/// <summary>
		/// 当值为 getter 回调时返回该回调；否则返回 null。
		/// Returns the getter callback when the value was created from a getter; otherwise null.
		/// </summary>
		public Func<TValue>? AsGetter => Value as Func<TValue>;

		/// <summary>
		/// 当值为可写计算属性选项时返回该选项；否则返回 null。
		/// Returns the writable computed options when the value was created from options; otherwise null.
		/// </summary>
		public VueWritableComputedOptions<TValue>? AsOptions => Value as VueWritableComputedOptions<TValue>;
	}

	/// <summary>
	/// Options API watch 声明的联合类型。可以是一个方法名字符串、回调函数、带清理注册的回调、
	/// 处理器选项、带清理的处理器选项、具名处理器选项或 watch 条目数组。
	/// Union type for Options API watch declarations. Can be a method name string, callback function,
	/// cleanup-aware callback, handler options, cleanup handler options, named handler options, or watch entries array.
	/// </summary>
	/// <typeparam name="TValue">被侦听值的类型。The watched value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueWatchDeclaration<TValue>(
		string,
		Action<TValue, TValue>,
		VueWatchCleanupCallback<TValue>,
		VueWatchHandlerOptions<TValue>,
		VueWatchCleanupHandlerOptions<TValue>,
		VueWatchNamedHandlerOptions,
		VueWatchEntries<TValue>)
	{
		/// <summary>
		/// 当值为方法名时返回该名称；否则返回 null。
		/// Returns the method name when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsMethodName => Value as string;

		/// <summary>
		/// 当值为回调处理器时返回该处理器；否则返回 null。
		/// Returns the handler callback when the value was created from a handler; otherwise null.
		/// </summary>
		public Action<TValue, TValue>? AsHandler => Value as Action<TValue, TValue>;

		/// <summary>
		/// 当值为带清理的回调处理器时返回该处理器；否则返回 null。
		/// Returns the cleanup-aware handler when the value was created from a cleanup handler; otherwise null.
		/// </summary>
		public VueWatchCleanupCallback<TValue>? AsCleanupHandler => Value as VueWatchCleanupCallback<TValue>;

		/// <summary>
		/// 当值为处理器选项时返回该选项；否则返回 null。
		/// Returns the handler options when the value was created from options; otherwise null.
		/// </summary>
		public VueWatchHandlerOptions<TValue>? AsHandlerOptions => Value as VueWatchHandlerOptions<TValue>;

		/// <summary>
		/// 当值为带清理的处理器选项时返回该选项；否则返回 null。
		/// Returns the cleanup handler options when the value was created from cleanup options; otherwise null.
		/// </summary>
		public VueWatchCleanupHandlerOptions<TValue>? AsCleanupHandlerOptions => Value as VueWatchCleanupHandlerOptions<TValue>;

		/// <summary>
		/// 当值为具名处理器选项时返回该选项；否则返回 null。
		/// Returns the named handler options when the value was created from named options; otherwise null.
		/// </summary>
		public VueWatchNamedHandlerOptions? AsNamedHandlerOptions => Value as VueWatchNamedHandlerOptions;

		/// <summary>
		/// 当值为 watch 条目数组时返回该数组；否则返回 null。
		/// Returns the watch entries when the value was created from entries; otherwise null.
		/// </summary>
		public VueWatchEntries<TValue>? AsEntries => Value as VueWatchEntries<TValue>;
	}

	/// <summary>
	/// Options API inject 声明中 <c>from</c> 字段的联合类型。
	/// 可以是字符串键、强类型注入键或 JavaScript Symbol。
	/// Union type for the <c>from</c> field in Options API inject declarations.
	/// Can be a string key, a strongly typed injection key, or a JavaScript Symbol.
	/// </summary>
	/// <typeparam name="TValue">注入值的类型。The injected value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueInjectFrom<TValue>(string, VueInjectionKey<TValue>, Symbol)
	{
		/// <summary>
		/// 当值为字符串键时返回该字符串；否则返回 null。
		/// Returns the string key when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为强类型注入键时返回该键；否则返回 null。
		/// Returns the typed injection key when the value was created from a key; otherwise null.
		/// </summary>
		public VueInjectionKey<TValue>? AsKey => Value as VueInjectionKey<TValue>;

		/// <summary>
		/// 当值为 Symbol 时返回该 Symbol；否则返回 null。
		/// Returns the Symbol when the value was created from a Symbol; otherwise null.
		/// </summary>
		public Symbol? AsSymbol => Value as Symbol;
	}

	/// <summary>
	/// Vue prop 声明的联合类型。可以是单个构造器类型、构造器类型数组或完整的 prop 选项对象。
	/// Union type for Vue prop declarations. Can be a single constructor type, a constructor type array, or full prop options.
	/// </summary>
	/// <typeparam name="TValue">prop 值的类型。The prop value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public readonly union VuePropDeclaration<TValue>(VuePropType, VuePropType?[], VuePropOptions<TValue>)
	{
		/// <summary>
		/// 当值为单个构造器类型时返回该类型；否则返回 null。
		/// Returns the single constructor type when the value was created from a type; otherwise null.
		/// </summary>
		public VuePropType? AsType => Value as VuePropType;

		/// <summary>
		/// 当值为构造器类型数组时返回该数组；否则返回 null。
		/// Returns the constructor type array when the value was created from types; otherwise null.
		/// </summary>
		public VuePropType?[]? AsTypes => Value as VuePropType?[];

		/// <summary>
		/// 当值为完整 prop 选项时返回该选项；否则返回 null。
		/// Returns the prop options when the value was created from options; otherwise null.
		/// </summary>
		public VuePropOptions<TValue>? AsOptions => Value as VuePropOptions<TValue>;
	}

	/// <summary>
	/// Vue <c>class</c> 绑定值的联合类型。可以是字符串、字符串数组、对象形式或混合值数组。
	/// Union type for Vue <c>class</c> binding values. Can be a string, string array, object form, or mixed value array.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueClassValue(string, string[], VueProps, VueValue[])
	{
		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为字符串数组时返回该数组；否则返回 null。
		/// Returns the string array when the value was created from strings; otherwise null.
		/// </summary>
		public string[]? AsStrings => Value as string[];

		/// <summary>
		/// 当值为对象形式时返回该对象；否则返回 null。
		/// Returns the props object when the value was created from object form; otherwise null.
		/// </summary>
		public VueProps? AsProps => Value as VueProps;

		/// <summary>
		/// 当值为混合值数组时返回该数组；否则返回 null。
		/// Returns the mixed value array when the value was created from values; otherwise null.
		/// </summary>
		public VueValue[]? AsValues => Value as VueValue[];
	}

	/// <summary>
	/// 接受字符串或数字值的 HTML 属性联合类型（如 <c>min</c>、<c>max</c>、<c>step</c>）。
	/// Union type for HTML attributes that accept either string or numeric values (e.g. <c>min</c>, <c>max</c>, <c>step</c>).
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringNumberValue(double, string)
	{
		/// <summary>
		/// 当值为数字时返回该数字；否则返回 null。
		/// Returns the number when the value was created from a number; otherwise null.
		/// </summary>
		public double? AsNumber => Value is double value ? value : default(double?);

		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;
	}

	/// <summary>
	/// <c>watch()</c> 的 <c>deep</c> 选项联合类型。可以是布尔值（启用/禁用深层遍历）或整数（限制遍历深度）。
	/// Union type for the <c>deep</c> option of <c>watch()</c>. Can be a boolean (enable/disable deep traversal) or an integer (depth limit).
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueWatchDeep(bool, int)
	{
		/// <summary>
		/// 当值为布尔值时返回该值；否则返回 null。
		/// Returns the boolean when the value was created from a bool; otherwise null.
		/// </summary>
		public bool? AsBool => Value is bool value ? value : default(bool?);

		/// <summary>
		/// 当值为整数时返回该值；否则返回 null。
		/// Returns the integer when the value was created from an int; otherwise null.
		/// </summary>
		public int? AsInt => Value is int value ? value : default(int?);
	}

	/// <summary>
	/// Transition 持续时间的联合类型。可以是数字（毫秒）或包含进入/离开阶段时间的对象。
	/// Union type for Transition duration. Can be a number (milliseconds) or an object with enter/leave phase durations.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueTransitionDurationValue(Number, VueTransitionDuration)
	{
		/// <summary>
		/// 当值为数字时返回该数字；否则返回 null。
		/// Returns the number when the value was created from a number; otherwise null.
		/// </summary>
		public Number? AsNumber => Value is Number value ? value : default(Number?);

		/// <summary>
		/// 当值为持续时间对象时返回该对象；否则返回 null。
		/// Returns the duration object when the value was created from a duration; otherwise null.
		/// </summary>
		public VueTransitionDuration? AsDuration => Value as VueTransitionDuration;
	}

	/// <summary>
	/// <c>KeepAlive</c> 的 <c>include</c>/<c>exclude</c> 匹配值的联合类型。
	/// 可以是字符串、正则表达式、字符串数组或正则表达式数组。
	/// Union type for <c>KeepAlive</c> <c>include</c>/<c>exclude</c> match values.
	/// Can be a string, RegExp, string array, or RegExp array.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueKeepAliveMatch(string, RegExp, string[], RegExp[])
	{
		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为正则表达式时返回该正则；否则返回 null。
		/// Returns the RegExp when the value was created from a RegExp; otherwise null.
		/// </summary>
		public RegExp? AsRegExp => Value as RegExp;

		/// <summary>
		/// 当值为字符串数组时返回该数组；否则返回 null。
		/// Returns the string array when the value was created from strings; otherwise null.
		/// </summary>
		public string[]? AsStrings => Value as string[];

		/// <summary>
		/// 当值为正则表达式数组时返回该数组；否则返回 null。
		/// Returns the RegExp array when the value was created from RegExps; otherwise null.
		/// </summary>
		public RegExp[]? AsRegExps => Value as RegExp[];
	}

	/// <summary>
	/// 接受整数或字符串值的 HTML 属性联合类型（如 <c>max</c> 用于 <c>KeepAlive</c>）。
	/// Union type for HTML attributes that accept either integer or string values (e.g. <c>max</c> for <c>KeepAlive</c>).
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueIntStringValue(int, string)
	{
		/// <summary>
		/// 当值为整数时返回该值；否则返回 null。
		/// Returns the integer when the value was created from an int; otherwise null.
		/// </summary>
		public int? AsInt => Value is int value ? value : default(int?);

		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;
	}

	/// <summary>
	/// <c>Teleport</c> 目标容器的联合类型。可以是 CSS 选择器字符串或 DOM 元素。
	/// Union type for <c>Teleport</c> target container. Can be a CSS selector string or a DOM element.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueTeleportTarget(string, Element)
	{
		/// <summary>
		/// 当值为 CSS 选择器字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为 DOM 元素时返回该元素；否则返回 null。
		/// Returns the element when the value was created from an element; otherwise null.
		/// </summary>
		public Element? AsElement => Value as Element;
	}
}
