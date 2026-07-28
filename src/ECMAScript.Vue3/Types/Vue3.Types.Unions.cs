using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
	/// Vue authoring 中常见的布尔或字符串联合值。适用于运行时同时接受启用标记或具名字面量的高频属性。
	/// Common boolean-or-string union value for Vue authoring. Use this when a runtime
	/// contract accepts either an enable/disable flag or a named string literal.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueBooleanStringValue(bool, string)
	{
		/// <summary>
		/// 当值为布尔值时返回该值；否则返回 null。
		/// Returns the boolean when the value was created from a bool; otherwise null.
		/// </summary>
		public bool? AsBool => Value is bool value ? value : default(bool?);

		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;
	}

	/// <summary>
	/// Vue authoring 中常见的布尔、字符串或数字联合值。适用于 modelValue、activeValue 等同时接受
	/// 布尔开关、具名字面量或标量编号的高频属性。
	/// Common boolean-or-string-or-number union value for Vue authoring. Use this when a
	/// runtime contract accepts booleans, named string literals, or scalar numeric values
	/// such as modelValue or active/inactive state props.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueBooleanStringNumberValue(bool, double, string)
	{
		/// <summary>
		/// 当值为布尔值时返回该值；否则返回 null。
		/// Returns the boolean when the value was created from a bool; otherwise null.
		/// </summary>
		public bool? AsBool => Value is bool value ? value : default(bool?);

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
	/// Vue authoring 中常见的布尔或数字联合值。适用于既接受开关也接受固定数值尺寸的高频属性。
	/// Common boolean-or-number union value for Vue authoring. Use this when a runtime
	/// contract accepts either a simple enable/disable flag or a scalar numeric override.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueBooleanNumberValue(bool, double)
	{
		/// <summary>
		/// 当值为布尔值时返回该值；否则返回 null。
		/// Returns the boolean when the value was created from a bool; otherwise null.
		/// </summary>
		public bool? AsBool => Value is bool value ? value : default(bool?);

		/// <summary>
		/// 当值为数字时返回该数字；否则返回 null。
		/// Returns the number when the value was created from a number; otherwise null.
		/// </summary>
		public double? AsNumber => Value is double value ? value : default(double?);
	}

	/// <summary>
	/// 接受字符串名或组件值的公共 Vue 联合类型。适用于图标、动态子组件入口或组件覆写槽位等高频 authoring 场景。
	/// Shared Vue union type that accepts either a string token or a component value.
	/// This fits common authoring scenarios such as icon/component overrides and
	/// dynamic component entry points.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringComponentValue(string, IVueComponent)
	{
		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为组件时返回该组件；否则返回 null。
		/// Returns the component when the value was created from a component; otherwise null.
		/// </summary>
		public IVueComponent? AsComponent => Value as IVueComponent;
	}

	/// <summary>
	/// Vue <c>style</c> 绑定值的联合类型。可以是字符串、对象形式或混合样式数组。
	/// Union type for Vue <c>style</c> binding values. Can be a string, object form, or
	/// mixed style arrays.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStyleValue(string, VueProps, VueStyleValues)
	{
		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为对象形式时返回该对象；否则返回 null。
		/// Returns the props object when the value was created from object form; otherwise null.
		/// </summary>
		public VueProps? AsProps => Value as VueProps;

		/// <summary>
		/// 当值为样式值数组时返回该数组；否则返回 null。
		/// Returns the style values when the value was created from an array; otherwise null.
		/// </summary>
		public VueStyleValues? AsValues => Value is VueStyleValues value ? value : default(VueStyleValues?);

		public static implicit operator VueStyleValue(string value)
			=> new(value);

		public static implicit operator VueStyleValue(VueProps value)
			=> new(value);

		public static implicit operator VueStyleValue(VueDictionary value)
			=> new(value);

		public static implicit operator VueStyleValue(VueStyleValues value)
			=> new(value);

		public static implicit operator VueStyleValue(VueStyleValue[] values)
			=> new((VueStyleValues)values);

		public static implicit operator VueStyleValue(string[] values)
			=> new((VueStyleValues)values);

		public static implicit operator VueStyleValue(VueProps[] values)
			=> new((VueStyleValues)values);

		public static implicit operator VueStyleValue(VueDictionary[] values)
			=> new((VueStyleValues)values);
	}

	/// <summary>
	/// Vue <c>style</c> 数组 authoring 值。保留集合表达式 authoring。
	/// Array authoring surface for Vue <c>style</c> values, preserving collection-expression
	/// authoring.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	[CollectionBuilder(typeof(VueStyleValuesCollectionBuilder), nameof(VueStyleValuesCollectionBuilder.Create))]
	public readonly union VueStyleValues(VueStyleValue[]) : IEnumerable<VueStyleValue>
	{
		public VueStyleValue[]? AsArray => Value as VueStyleValue[];

		IEnumerator<VueStyleValue> IEnumerable<VueStyleValue>.GetEnumerator()
			=> ((IEnumerable<VueStyleValue>)(AsArray ?? Array.Empty<VueStyleValue>())).GetEnumerator();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> ((IEnumerable<VueStyleValue>)this).GetEnumerator();

		public static implicit operator VueStyleValues(VueStyleValue[] values)
			=> new(values);

		public static implicit operator VueStyleValues(string[] values)
			=> new(Array.ConvertAll(values, static value => (VueStyleValue)value));

		public static implicit operator VueStyleValues(VueProps[] values)
			=> new(Array.ConvertAll(values, static value => (VueStyleValue)value));

		public static implicit operator VueStyleValues(VueDictionary[] values)
			=> new(Array.ConvertAll(values, static value => (VueStyleValue)value));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class VueStyleValuesCollectionBuilder
	{
		public static VueStyleValues Create(ReadOnlySpan<VueStyleValue> values)
			=> values.ToArray();
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
	/// Vue 日期/时间输入面常见的“字符串、数字或日期”标量联合类型。用于对齐官方
	/// <c>DateModelType = string | number | Date</c> 一类公开合同。
	/// Common Vue scalar union for date/time authoring surfaces that accept a string,
	/// a number, or a date. Use this for official <c>DateModelType = string | number | Date</c>-style contracts.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringNumberDateValue(double, string, Date)
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

		/// <summary>
		/// 当值为日期时返回该日期；否则返回 null。
		/// Returns the date when the value was created from a date; otherwise null.
		/// </summary>
		public Date? AsDate => Value is Date value ? value : default(Date?);
	}

	/// <summary>
	/// Vue authoring 中常见的“字符串/数字标量或其数组”联合类型。适用于官方公开合同既接受
	/// 单个字符串/数字值，也接受同域数组值的高频属性，例如 collapse active names 一类输入面。
	/// Common Vue union that accepts either a scalar string/number value or an array of the same domain.
	/// Use this for public contracts such as collapse active names that officially allow one value or many values.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringNumberArrayableValue(VueStringNumberValue, VueStringNumberValue[]) : IEnumerable<VueStringNumberValue>
	{
		/// <summary>
		/// 当值为单个字符串/数字时返回该值；否则返回 null。
		/// Returns the scalar string/number value when the union was created from one value; otherwise null.
		/// </summary>
		public VueStringNumberValue? AsSingle
			=> Value is VueStringNumberValue value ? value : default(VueStringNumberValue?);

		/// <summary>
		/// 当值为字符串/数字数组时返回该数组；否则返回 null。
		/// Returns the string/number array when the union was created from multiple values; otherwise null.
		/// </summary>
		public VueStringNumberValue[]? AsMultiple => Value as VueStringNumberValue[];

		public static implicit operator VueStringNumberArrayableValue(double value)
			=> new((VueStringNumberValue)value);

		public static implicit operator VueStringNumberArrayableValue(string value)
			=> new((VueStringNumberValue)value);

		public static implicit operator VueStringNumberArrayableValue(VueStringNumberValue[] values)
			=> new(values);

		public static implicit operator VueStringNumberArrayableValue(double[] values)
			=> new(Array.ConvertAll(values, static value => (VueStringNumberValue)value));

		public static implicit operator VueStringNumberArrayableValue(string[] values)
			=> new(Array.ConvertAll(values, static value => (VueStringNumberValue)value));

		IEnumerator<VueStringNumberValue> IEnumerable<VueStringNumberValue>.GetEnumerator()
			=> ((IEnumerable<VueStringNumberValue>)(AsMultiple ?? Array.Empty<VueStringNumberValue>())).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<VueStringNumberValue>)this).GetEnumerator();
	}

	/// <summary>
	/// Vue 日期/时间输入面常见的“字符串/数字/日期标量或其同域数组”联合类型。用于对齐官方
	/// <c>ModelValueType = string | number | Date | string[] | number[] | Date[]</c> 这类公开合同，
	/// 并保持数组分支仍然是同域数组而不是被放宽成混合元素数组。
	/// Common Vue union for date/time authoring surfaces that accept a string/number/date scalar
	/// or a homogeneous array of the same domain. This matches official
	/// <c>ModelValueType = string | number | Date | string[] | number[] | Date[]</c>-style contracts
	/// without widening arrays into mixed-element unions.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringNumberDateArrayableValue(
		VueStringNumberDateValue,
		Number[],
		string[],
		Date[])
	{
		/// <summary>
		/// 当值为单个字符串/数字/日期时返回该值；否则返回 null。
		/// Returns the scalar string/number/date value when the union was created from one value; otherwise null.
		/// </summary>
		public VueStringNumberDateValue? AsSingle
			=> Value is VueStringNumberDateValue value ? value : default(VueStringNumberDateValue?);

		/// <summary>
		/// 当值为数字数组时返回该数组；否则返回 null。
		/// Returns the numeric array when the union was created from multiple numbers; otherwise null.
		/// </summary>
		public Number[]? AsNumbers => Value is Number[] value ? value : default(Number[]?);

		/// <summary>
		/// 当值为字符串数组时返回该数组；否则返回 null。
		/// Returns the string array when the union was created from multiple strings; otherwise null.
		/// </summary>
		public string[]? AsStrings => Value as string[];

		/// <summary>
		/// 当值为日期数组时返回该数组；否则返回 null。
		/// Returns the date array when the union was created from multiple dates; otherwise null.
		/// </summary>
		public Date[]? AsDates => Value as Date[];

		public static implicit operator VueStringNumberDateArrayableValue(double value)
			=> new((VueStringNumberDateValue)value);

		public static implicit operator VueStringNumberDateArrayableValue(string value)
			=> new((VueStringNumberDateValue)value);

		public static implicit operator VueStringNumberDateArrayableValue(Date value)
			=> new((VueStringNumberDateValue)value);

		public static implicit operator VueStringNumberDateArrayableValue(Number[] values)
			=> new(values);

		public static implicit operator VueStringNumberDateArrayableValue(double[] values)
			=> new(Array.ConvertAll(values, static value => (Number)value));

		public static implicit operator VueStringNumberDateArrayableValue(string[] values)
			=> new(values);

		public static implicit operator VueStringNumberDateArrayableValue(Date[] values)
			=> new(values);
	}

	/// <summary>
	/// Vue authoring 中常见的“单个数字或数字数组”联合类型。适用于官方公开合同既接受标量数值，
	/// 也接受数值序列的高频属性，例如 slider/range modelValue 一类输入面。
	/// Common Vue union that accepts either a single numeric value or a numeric array. Use this
	/// for public contracts that officially allow one number or many numbers, such as slider/range model values.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueNumberOrNumbersValue(double, Number[]) : IEnumerable<Number>
	{
		/// <summary>
		/// 当值为单个数字时返回该数字；否则返回 null。
		/// Returns the number when the value was created from a scalar number; otherwise null.
		/// </summary>
		public double? AsNumber => Value is double value ? value : default(double?);

		/// <summary>
		/// 当值为数值数组时返回该数组；否则返回 null。
		/// Returns the numeric array when the value was created from multiple numbers; otherwise null.
		/// </summary>
		public Number[]? AsNumbers => Value is Number[] value ? value : default(Number[]?);

		public static implicit operator VueNumberOrNumbersValue(Number[] values)
			=> new(values);

		public static implicit operator VueNumberOrNumbersValue(double[] values)
			=> new(Array.ConvertAll(values, static value => (Number)value));

		IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
			=> ((IEnumerable<Number>)(AsNumbers ?? Array.Empty<Number>())).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<Number>)this).GetEnumerator();
	}

	/// <summary>
	/// Vue authoring 中常见的“字符串、数字或对象”联合类型。适用于官方公开合同允许标量选择值
	/// 或对象载荷的常见属性，例如命令值、选项值等。
	/// Common Vue union that accepts a string, a number, or an object payload. Use this
	/// for public contracts that officially allow scalar selection values or object values.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringNumberObjectValue(double, string, VueProps)
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

		/// <summary>
		/// 当值为对象时返回该对象；否则返回 null。
		/// Returns the object payload when the value was created from an object; otherwise null.
		/// </summary>
		public VueProps? AsProps => Value as VueProps;

		public static implicit operator VueStringNumberObjectValue(VueDictionary value)
			=> (VueProps)value;
	}

	/// <summary>
	/// Vue authoring 中常见的“布尔、字符串、数字或对象”联合类型。适用于像 checkbox value/label
	/// 这类既允许标量值也允许对象值的公开合同。
	/// Common Vue union that accepts a boolean, a string, a number, or an object payload.
	/// This fits public contracts such as checkbox value/label that allow scalar or object values.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueBooleanStringNumberObjectValue(bool, double, string, VueProps)
	{
		/// <summary>
		/// 当值为布尔值时返回该值；否则返回 null。
		/// Returns the boolean when the value was created from a bool; otherwise null.
		/// </summary>
		public bool? AsBool => Value is bool value ? value : default(bool?);

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

		/// <summary>
		/// 当值为对象时返回该对象；否则返回 null。
		/// Returns the object payload when the value was created from an object; otherwise null.
		/// </summary>
		public VueProps? AsProps => Value as VueProps;

		public static implicit operator VueBooleanStringNumberObjectValue(VueDictionary value)
			=> (VueProps)value;
	}

	/// <summary>
	/// Vue authoring 中常见的“布尔/字符串/数字/对象标量或其数组”联合类型。适用于官方公开合同既接受
	/// 单个选择值，也接受同域多值数组的输入面，例如 select/tree-select modelValue。
	/// Common Vue union that accepts either a scalar boolean/string/number/object value or an array of the same domain.
	/// Use this for public contracts such as select/tree-select model values that officially allow one value or many values.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueBooleanStringNumberObjectArrayableValue(
		VueBooleanStringNumberObjectValue,
		VueBooleanStringNumberObjectValue[]) : IEnumerable<VueBooleanStringNumberObjectValue>
	{
		/// <summary>
		/// 当值为单个布尔/字符串/数字/对象值时返回该值；否则返回 null。
		/// Returns the scalar boolean/string/number/object value when the union was created from one value; otherwise null.
		/// </summary>
		public VueBooleanStringNumberObjectValue? AsSingle
			=> Value is VueBooleanStringNumberObjectValue value ? value : default(VueBooleanStringNumberObjectValue?);

		/// <summary>
		/// 当值为布尔/字符串/数字/对象数组时返回该数组包装；否则返回 null。
		/// Returns the array when the union was created from multiple values; otherwise null.
		/// </summary>
		public VueBooleanStringNumberObjectValue[]? AsMultiple => Value as VueBooleanStringNumberObjectValue[];

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(bool value)
			=> new((VueBooleanStringNumberObjectValue)value);

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(double value)
			=> new((VueBooleanStringNumberObjectValue)value);

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(string value)
			=> new((VueBooleanStringNumberObjectValue)value);

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(VueProps value)
		{
			VueBooleanStringNumberObjectValue scalar = value;
			return new(scalar);
		}

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(VueDictionary value)
		{
			VueBooleanStringNumberObjectValue scalar = value;
			return new(scalar);
		}

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(
			VueBooleanStringNumberObjectValue[] values)
			=> new(values);

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(bool[] values)
			=> new(Array.ConvertAll(values, static value => (VueBooleanStringNumberObjectValue)value));

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(double[] values)
			=> new(Array.ConvertAll(values, static value => (VueBooleanStringNumberObjectValue)value));

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(string[] values)
			=> new(Array.ConvertAll(values, static value => (VueBooleanStringNumberObjectValue)value));

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(VueProps[] values)
			=> new(Array.ConvertAll(values, static value =>
			{
				VueBooleanStringNumberObjectValue scalar = value;
				return scalar;
			}));

		public static implicit operator VueBooleanStringNumberObjectArrayableValue(VueDictionary[] values)
			=> new(Array.ConvertAll(values, static value =>
			{
				VueBooleanStringNumberObjectValue scalar = value;
				return scalar;
			}));

		IEnumerator<VueBooleanStringNumberObjectValue> IEnumerable<VueBooleanStringNumberObjectValue>.GetEnumerator()
			=> ((IEnumerable<VueBooleanStringNumberObjectValue>)(AsMultiple ?? Array.Empty<VueBooleanStringNumberObjectValue>())).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<VueBooleanStringNumberObjectValue>)this).GetEnumerator();
	}

	/// <summary>
	/// Vue 生态中常见的“单个字符串或字符串数组”联合类型。适用于既接受单个字符串，
	/// 也接受多字符串 authoring 的高频公开合同。
	/// Common Vue union that accepts either a single string or a string array. Use this
	/// for public authoring contracts that officially allow one string or many strings.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringOrStringsValue(string, string[])
	{
		/// <summary>
		/// 当值为单个字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a single string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为字符串数组时返回该数组；否则返回 null。
		/// Returns the string array when the value was created from multiple strings; otherwise null.
		/// </summary>
		public string[]? AsStrings => Value as string[];
	}

	/// <summary>
	/// 精确由两个数值构成的公共 Vue 二元组。用于镜像官方 `[number, number]`
	/// 这类“恰好两个数”的 authoring contract，并保留集合表达式 authoring。
	/// Exact two-number Vue pair. This mirrors official `[number, number]` contracts
	/// while preserving collection-expression authoring.
	/// </summary>
	[ECMAScript]
	[Union]
	[Description("@#")]
	[CollectionBuilder(typeof(VueNumberPairCollectionBuilder), nameof(VueNumberPairCollectionBuilder.Create))]
	public readonly struct VueNumberPair : IUnion, IEnumerable<Number>
	{
		private readonly Number[]? _values;

		public VueNumberPair(Number[] values)
		{
			ArgumentNullException.ThrowIfNull(values);
			if (values.Length != 2)
				throw new ArgumentException("Vue number pair values require exactly two items.", nameof(values));

			_values = values;
		}

		/// <summary>
		/// 获取这组数值。始终要求恰好两个值。
		/// Gets the numeric pair. The contract always requires exactly two items.
		/// </summary>
		public Number[]? AsValues => _values;

		/// <summary>
		/// 获取第一个数值；如果未设置则返回 null。
		/// Gets the first numeric value, or null when unset.
		/// </summary>
		public Number? First => _values is { Length: > 0 } values ? values[0] : default(Number?);

		/// <summary>
		/// 获取第二个数值；如果未设置则返回 null。
		/// Gets the second numeric value, or null when unset.
		/// </summary>
		public Number? Second => _values is { Length: > 1 } values ? values[1] : default(Number?);

		public object? Value => _values;

		[ECMAScriptInline("__arg1")]
		public extern static VueNumberPair From(Number[] values);

		public static implicit operator VueNumberPair(Number[] values)
			=> new(values);

		public static implicit operator VueNumberPair(double[] values)
			=> new(Array.ConvertAll(values, static value => (Number)value));

		IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
			=> ((IEnumerable<Number>)(_values ?? Array.Empty<Number>())).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<Number>)this).GetEnumerator();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class VueNumberPairCollectionBuilder
	{
		public static VueNumberPair Create(ReadOnlySpan<Number> values)
			=> values.ToArray();
	}

	/// <summary>
	/// 同时接受字符串、数字或单个 VNode 的公共 Vue authoring 联合类型。
	/// 适用于官方公开合同显式允许这三种运行时分支的场景。
	/// Common Vue authoring union that accepts a string, a number, or a single VNode.
	/// Use this when the official public contract explicitly allows these three runtime branches.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringNumberVNodeValue(string, Number, IVNode)
	{
		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为数字时返回该数字；否则返回 null。
		/// Returns the number when the value was created from a number; otherwise null.
		/// </summary>
		public Number? AsNumber => Value is Number value ? value : default(Number?);

		/// <summary>
		/// 当值为 VNode 时返回该节点；否则返回 null。
		/// Returns the VNode when the value was created from a VNode; otherwise null.
		/// </summary>
		public IVNode? AsVNode => Value as IVNode;

		public static implicit operator VueStringNumberVNodeValue(double value)
			=> new((Number)value);
	}

	/// <summary>
	/// Vue 生态中常见的“字符串或正则”联合类型。适用于分隔符、过滤规则等官方公开合同。
	/// Common Vue union that accepts either a string or a regular expression. Use this
	/// for official public contracts such as delimiters and simple pattern inputs.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringRegExpValue(string, RegExp)
	{
		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为正则时返回该对象；否则返回 null。
		/// Returns the regular expression when the value was created from a RegExp; otherwise null.
		/// </summary>
		public RegExp? AsRegExp => Value as RegExp;
	}

	/// <summary>
	/// Vue authoring 中常见的字符串或 HTML 元素联合值。适用于宿主合同接受 CSS 选择器字符串或现有 DOM 元素的场景。
	/// Common Vue union that accepts either a string or an HTML element. Use this for host
	/// contracts that allow either a CSS selector string or an existing DOM element instance.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringHtmlElementValue(string, HTMLElement)
	{
		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为 HTML 元素时返回该元素；否则返回 null。
		/// Returns the HTML element when the value was created from an HTMLElement; otherwise null.
		/// </summary>
		public HTMLElement? AsElement => Value as HTMLElement;
	}

	/// <summary>
	/// Vue authoring 中常见的 Headers 或字典联合值。适用于宿主合同接受 Fetch `Headers`
	/// 或普通对象字典作为请求头集合的场景。
	/// Common Vue union that accepts either Fetch `Headers` or a plain object dictionary.
	/// Use this for host contracts that allow either runtime `Headers` or object-form request headers.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueHeadersValue(Headers, VueDictionary)
	{
		/// <summary>
		/// 当值为 Headers 时返回该实例；否则返回 null。
		/// Returns the Headers instance when the value was created from Headers; otherwise null.
		/// </summary>
		public Headers? AsHeaders => Value as Headers;

		/// <summary>
		/// 当值为字典对象时返回该对象；否则返回 null。
		/// Returns the dictionary when the value was created from object form; otherwise null.
		/// </summary>
		public VueDictionary? AsDictionary => Value as VueDictionary;
	}

	/// <summary>
	/// 精确由两个字符串构成的公共 Vue 区间值。用于镜像 Vue 生态中 <c>SingleOrRange&lt;string&gt;</c>
	/// 这类“单值或双值区间”契约的区间分支，并保留集合表达式 authoring。
	/// Exact two-string Vue range value. This mirrors the range branch of
	/// <c>SingleOrRange&lt;string&gt;</c>-style contracts while preserving collection-expression
	/// authoring.
	/// </summary>
	[ECMAScript]
	[Union]
	[Description("@#")]
	[CollectionBuilder(typeof(VueStringPairCollectionBuilder), nameof(VueStringPairCollectionBuilder.Create))]
	public readonly struct VueStringPair : IUnion, IEnumerable<string>
	{
		private readonly string[]? _values;

		public VueStringPair(string[] values)
		{
			ArgumentNullException.ThrowIfNull(values);
			if (values.Length != 2)
				throw new ArgumentException("Vue string pair values require exactly two items.", nameof(values));

			_values = values;
		}

		/// <summary>
		/// 获取这组区间字符串。始终要求恰好两个值。
		/// Gets the range strings. The contract always requires exactly two items.
		/// </summary>
		public string[]? AsValues => _values;

		/// <summary>
		/// 获取第一个区间值；如果未设置则返回 null。
		/// Gets the first range value, or null when unset.
		/// </summary>
		public string? First => _values is { Length: > 0 } values ? values[0] : null;

		/// <summary>
		/// 获取第二个区间值；如果未设置则返回 null。
		/// Gets the second range value, or null when unset.
		/// </summary>
		public string? Second => _values is { Length: > 1 } values ? values[1] : null;

		public object? Value => _values;

		[ECMAScriptInline("__arg1")]
		public extern static VueStringPair From(string[] values);

		public static implicit operator VueStringPair(string[] values)
			=> new(values);

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
			=> ((IEnumerable<string>)(_values ?? Array.Empty<string>())).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<string>)this).GetEnumerator();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class VueStringPairCollectionBuilder
	{
		public static VueStringPair Create(ReadOnlySpan<string> values)
			=> values.ToArray();
	}

	/// <summary>
	/// Vue 生态中常见的“单个字符串或双值字符串区间”联合类型。用于对齐官方
	/// <c>SingleOrRange&lt;string&gt;</c> 这类 authoring contract。
	/// Common Vue union that accepts either a single string or an exact two-value string
	/// range. Use this to mirror official <c>SingleOrRange&lt;string&gt;</c>-style contracts.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueStringSingleOrRangeValue(string, VueStringPair)
	{
		/// <summary>
		/// 当值为单个字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a single string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为双值区间时返回该区间；否则返回 null。
		/// Returns the two-value range when the value was created from a range; otherwise null.
		/// </summary>
		public VueStringPair? AsRange => Value is VueStringPair value ? value : default(VueStringPair?);

		public static implicit operator VueStringSingleOrRangeValue(string[] values)
			=> new((VueStringPair)values);
	}

	/// <summary>
	/// 精确由两个日期构成的公共 Vue 区间值。用于镜像 Vue 生态中 <c>SingleOrRange&lt;Date&gt;</c>
	/// 这类“单值或双值区间”契约的区间分支，并保留集合表达式 authoring。
	/// Exact two-date Vue range value. This mirrors the range branch of
	/// <c>SingleOrRange&lt;Date&gt;</c>-style contracts while preserving collection-expression
	/// authoring.
	/// </summary>
	[ECMAScript]
	[Union]
	[Description("@#")]
	[CollectionBuilder(typeof(VueDatePairCollectionBuilder), nameof(VueDatePairCollectionBuilder.Create))]
	public readonly struct VueDatePair : IUnion, IEnumerable<Date>
	{
		private readonly Date[]? _values;

		public VueDatePair(Date[] values)
		{
			ArgumentNullException.ThrowIfNull(values);
			if (values.Length != 2)
				throw new ArgumentException("Vue date pair values require exactly two items.", nameof(values));

			_values = values;
		}

		/// <summary>
		/// 获取这组区间日期。始终要求恰好两个值。
		/// Gets the range dates. The contract always requires exactly two items.
		/// </summary>
		public Date[]? AsValues => _values;

		/// <summary>
		/// 获取第一个区间日期；如果未设置则返回 null。
		/// Gets the first range date, or null when unset.
		/// </summary>
		public Date? First => _values is { Length: > 0 } values ? values[0] : null;

		/// <summary>
		/// 获取第二个区间日期；如果未设置则返回 null。
		/// Gets the second range date, or null when unset.
		/// </summary>
		public Date? Second => _values is { Length: > 1 } values ? values[1] : null;

		public object? Value => _values;

		[ECMAScriptInline("__arg1")]
		public extern static VueDatePair From(Date[] values);

		public static implicit operator VueDatePair(Date[] values)
			=> new(values);

		IEnumerator<Date> IEnumerable<Date>.GetEnumerator()
			=> ((IEnumerable<Date>)(_values ?? Array.Empty<Date>())).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<Date>)this).GetEnumerator();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class VueDatePairCollectionBuilder
	{
		public static VueDatePair Create(ReadOnlySpan<Date> values)
			=> values.ToArray();
	}

	/// <summary>
	/// Vue 生态中常见的“单个日期或双值日期区间”联合类型。用于对齐官方
	/// <c>SingleOrRange&lt;Date&gt;</c> 这类 authoring contract。
	/// Common Vue union that accepts either a single date or an exact two-value date
	/// range. Use this to mirror official <c>SingleOrRange&lt;Date&gt;</c>-style contracts.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueDateSingleOrRangeValue(Date, VueDatePair)
	{
		/// <summary>
		/// 当值为单个日期时返回该日期；否则返回 null。
		/// Returns the date when the value was created from a single date; otherwise null.
		/// </summary>
		public Date? AsDate => Value is Date value ? value : default(Date?);

		/// <summary>
		/// 当值为双值区间时返回该区间；否则返回 null。
		/// Returns the two-value range when the value was created from a range; otherwise null.
		/// </summary>
		public VueDatePair? AsRange => Value is VueDatePair value ? value : default(VueDatePair?);

		public static implicit operator VueDateSingleOrRangeValue(Date[] values)
			=> new((VueDatePair)values);
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
	/// Vue Transition 配置值的联合类型。可以是过渡名称字符串或完整的 Transition props 对象。
	/// Union type for Vue Transition configuration values. Can be a transition name string
	/// or a full Transition props object.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public readonly union VueTransitionValue(string, VueTransitionProps)
	{
		/// <summary>
		/// 当值为过渡名称字符串时返回该字符串；否则返回 null。
		/// Returns the transition name when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => Value as string;

		/// <summary>
		/// 当值为 Transition props 对象时返回该对象；否则返回 null。
		/// Returns the Transition props object when the value was created from props; otherwise null.
		/// </summary>
		public VueTransitionProps? AsProps => Value as VueTransitionProps;
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
