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
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueComputedValue<TValue>
	{
		private readonly byte _kind;
		private readonly Func<TValue>? _getter;
		private readonly VueWritableComputedOptions<TValue>? _options;

		/// <summary>
		/// 从 getter 回调构造。
		/// Constructs from a getter callback.
		/// </summary>
		/// <param name="getter">用于计算属性值的 getter 函数。The getter function for the computed value.</param>
		private VueComputedValue(Func<TValue> getter)
		{
			_kind = 1;
			_getter = getter;
			_options = default;
		}

		/// <summary>
		/// 从可写计算属性选项构造。
		/// Constructs from writable computed options.
		/// </summary>
		/// <param name="options">包含 get 和 set 的可写计算属性选项。Writable computed options with get and set.</param>
		private VueComputedValue(VueWritableComputedOptions<TValue> options)
		{
			_kind = 2;
			_getter = default;
			_options = options;
		}

		/// <summary>
		/// 当值为 getter 回调时返回该回调；否则返回 null。
		/// Returns the getter callback when the value was created from a getter; otherwise null.
		/// </summary>
		public Func<TValue>? AsGetter => _kind == 1 ? _getter : default;

		/// <summary>
		/// 当值为可写计算属性选项时返回该选项；否则返回 null。
		/// Returns the writable computed options when the value was created from options; otherwise null.
		/// </summary>
		public VueWritableComputedOptions<TValue>? AsOptions => _kind == 2 ? _options : default;

		/// <summary>
		/// 从 getter 回调隐式转换为计算属性值。
		/// Implicitly converts a getter callback to a computed value declaration.
		/// </summary>
		/// <param name="getter">用于计算属性值的 getter 函数。The getter function for the computed value.</param>
		public static implicit operator VueComputedValue<TValue>(Func<TValue> getter)
			=> new(getter);

		/// <summary>
		/// 从可写计算属性选项隐式转换为计算属性值。
		/// Implicitly converts writable computed options to a computed value declaration.
		/// </summary>
		/// <param name="options">包含 get 和 set 的可写计算属性选项。Writable computed options with get and set.</param>
		public static implicit operator VueComputedValue<TValue>(VueWritableComputedOptions<TValue> options)
			=> new(options);
	}

	/// <summary>
	/// Options API watch 声明的联合类型。可以是一个方法名字符串、回调函数、带清理注册的回调、
	/// 处理器选项、带清理的处理器选项、具名处理器选项或 watch 条目数组。
	/// Union type for Options API watch declarations. Can be a method name string, callback function,
	/// cleanup-aware callback, handler options, cleanup handler options, named handler options, or watch entries array.
	/// </summary>
	/// <typeparam name="TValue">被侦听值的类型。The watched value type.</typeparam>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueWatchDeclaration<TValue>
	{
		private readonly byte _kind;
		private readonly string? _methodName;
		private readonly Action<TValue, TValue>? _handler;
		private readonly VueWatchCleanupCallback<TValue>? _cleanupHandler;
		private readonly VueWatchHandlerOptions<TValue>? _handlerOptions;
		private readonly VueWatchCleanupHandlerOptions<TValue>? _cleanupHandlerOptions;
		private readonly VueWatchNamedHandlerOptions? _namedHandlerOptions;
		private readonly VueWatchEntries<TValue>? _entries;

		/// <summary>
		/// 从方法名字符串构造。
		/// Constructs from a method name string.
		/// </summary>
		/// <param name="methodName">从组件 methods 中解析的方法名。The method name to resolve from component methods.</param>
		private VueWatchDeclaration(string methodName)
		{
			_kind = 1;
			_methodName = methodName;
			_handler = default;
			_cleanupHandler = default;
			_handlerOptions = default;
			_cleanupHandlerOptions = default;
			_namedHandlerOptions = default;
			_entries = default;
		}

		/// <summary>
		/// 从回调处理器构造。
		/// Constructs from a callback handler.
		/// </summary>
		/// <param name="handler">接收当前值和旧值的回调。Callback receiving current and previous values.</param>
		private VueWatchDeclaration(Action<TValue, TValue> handler)
		{
			_kind = 2;
			_methodName = default;
			_handler = handler;
			_cleanupHandler = default;
			_handlerOptions = default;
			_cleanupHandlerOptions = default;
			_namedHandlerOptions = default;
			_entries = default;
		}

		/// <summary>
		/// 从带清理注册的回调处理器构造。
		/// Constructs from a cleanup-aware callback handler.
		/// </summary>
		/// <param name="cleanupHandler">带清理注册的回调。Cleanup-aware callback.</param>
		private VueWatchDeclaration(VueWatchCleanupCallback<TValue> cleanupHandler)
		{
			_kind = 3;
			_methodName = default;
			_handler = default;
			_cleanupHandler = cleanupHandler;
			_handlerOptions = default;
			_cleanupHandlerOptions = default;
			_namedHandlerOptions = default;
			_entries = default;
		}

		/// <summary>
		/// 从处理器选项构造。
		/// Constructs from handler options.
		/// </summary>
		/// <param name="handlerOptions">包含处理器和 watch 选项的对象。Object containing handler and watch options.</param>
		private VueWatchDeclaration(VueWatchHandlerOptions<TValue> handlerOptions)
		{
			_kind = 4;
			_methodName = default;
			_handler = default;
			_cleanupHandler = default;
			_handlerOptions = handlerOptions;
			_cleanupHandlerOptions = default;
			_namedHandlerOptions = default;
			_entries = default;
		}

		/// <summary>
		/// 从带清理注册的处理器选项构造。
		/// Constructs from cleanup-aware handler options.
		/// </summary>
		/// <param name="cleanupHandlerOptions">包含带清理的处理器和 watch 选项的对象。Object containing cleanup-aware handler and watch options.</param>
		private VueWatchDeclaration(VueWatchCleanupHandlerOptions<TValue> cleanupHandlerOptions)
		{
			_kind = 5;
			_methodName = default;
			_handler = default;
			_cleanupHandler = default;
			_handlerOptions = default;
			_cleanupHandlerOptions = cleanupHandlerOptions;
			_namedHandlerOptions = default;
			_entries = default;
		}

		/// <summary>
		/// 从具名处理器选项构造。
		/// Constructs from named handler options.
		/// </summary>
		/// <param name="namedHandlerOptions">包含方法名和 watch 选项的对象。Object containing method name and watch options.</param>
		private VueWatchDeclaration(VueWatchNamedHandlerOptions namedHandlerOptions)
		{
			_kind = 6;
			_methodName = default;
			_handler = default;
			_cleanupHandler = default;
			_handlerOptions = default;
			_cleanupHandlerOptions = default;
			_namedHandlerOptions = namedHandlerOptions;
			_entries = default;
		}

		/// <summary>
		/// 从 watch 条目数组构造。
		/// Constructs from an array of watch entries.
		/// </summary>
		/// <param name="entries">多个 watch 声明条目的数组。Array of multiple watch declaration entries.</param>
		private VueWatchDeclaration(VueWatchEntries<TValue> entries)
		{
			_kind = 7;
			_methodName = default;
			_handler = default;
			_cleanupHandler = default;
			_handlerOptions = default;
			_cleanupHandlerOptions = default;
			_namedHandlerOptions = default;
			_entries = entries;
		}

		/// <summary>
		/// 当值为方法名时返回该名称；否则返回 null。
		/// Returns the method name when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsMethodName => _kind == 1 ? _methodName : default;

		/// <summary>
		/// 当值为回调处理器时返回该处理器；否则返回 null。
		/// Returns the handler callback when the value was created from a handler; otherwise null.
		/// </summary>
		public Action<TValue, TValue>? AsHandler => _kind == 2 ? _handler : default;

		/// <summary>
		/// 当值为带清理的回调处理器时返回该处理器；否则返回 null。
		/// Returns the cleanup-aware handler when the value was created from a cleanup handler; otherwise null.
		/// </summary>
		public VueWatchCleanupCallback<TValue>? AsCleanupHandler => _kind == 3 ? _cleanupHandler : default;

		/// <summary>
		/// 当值为处理器选项时返回该选项；否则返回 null。
		/// Returns the handler options when the value was created from options; otherwise null.
		/// </summary>
		public VueWatchHandlerOptions<TValue>? AsHandlerOptions => _kind == 4 ? _handlerOptions : default;

		/// <summary>
		/// 当值为带清理的处理器选项时返回该选项；否则返回 null。
		/// Returns the cleanup handler options when the value was created from cleanup options; otherwise null.
		/// </summary>
		public VueWatchCleanupHandlerOptions<TValue>? AsCleanupHandlerOptions => _kind == 5 ? _cleanupHandlerOptions : default;

		/// <summary>
		/// 当值为具名处理器选项时返回该选项；否则返回 null。
		/// Returns the named handler options when the value was created from named options; otherwise null.
		/// </summary>
		public VueWatchNamedHandlerOptions? AsNamedHandlerOptions => _kind == 6 ? _namedHandlerOptions : default;

		/// <summary>
		/// 当值为 watch 条目数组时返回该数组；否则返回 null。
		/// Returns the watch entries when the value was created from entries; otherwise null.
		/// </summary>
		public VueWatchEntries<TValue>? AsEntries => _kind == 7 ? _entries : default;

		/// <summary>
		/// 从方法名字符串隐式转换为 watch 声明。
		/// Implicitly converts a method name string to a watch declaration.
		/// </summary>
		/// <param name="methodName">从组件 methods 中解析的方法名。The method name to resolve from component methods.</param>
		public static implicit operator VueWatchDeclaration<TValue>(string methodName)
			=> new(methodName);

		/// <summary>
		/// 从回调处理器隐式转换为 watch 声明。
		/// Implicitly converts a callback handler to a watch declaration.
		/// </summary>
		/// <param name="handler">接收当前值和旧值的回调。Callback receiving current and previous values.</param>
		public static implicit operator VueWatchDeclaration<TValue>(Action<TValue, TValue> handler)
			=> new(handler);

		/// <summary>
		/// 从带清理注册的回调处理器隐式转换为 watch 声明。
		/// Implicitly converts a cleanup-aware callback handler to a watch declaration.
		/// </summary>
		/// <param name="cleanupHandler">带清理注册的回调。Cleanup-aware callback.</param>
		public static implicit operator VueWatchDeclaration<TValue>(VueWatchCleanupCallback<TValue> cleanupHandler)
			=> new(cleanupHandler);

		/// <summary>
		/// 从处理器选项隐式转换为 watch 声明。
		/// Implicitly converts handler options to a watch declaration.
		/// </summary>
		/// <param name="handlerOptions">包含处理器和 watch 选项的对象。Object containing handler and watch options.</param>
		public static implicit operator VueWatchDeclaration<TValue>(VueWatchHandlerOptions<TValue> handlerOptions)
			=> new(handlerOptions);

		/// <summary>
		/// 从带清理的处理器选项隐式转换为 watch 声明。
		/// Implicitly converts cleanup handler options to a watch declaration.
		/// </summary>
		/// <param name="cleanupHandlerOptions">包含带清理的处理器和 watch 选项的对象。Object containing cleanup-aware handler and watch options.</param>
		public static implicit operator VueWatchDeclaration<TValue>(VueWatchCleanupHandlerOptions<TValue> cleanupHandlerOptions)
			=> new(cleanupHandlerOptions);

		/// <summary>
		/// 从具名处理器选项隐式转换为 watch 声明。
		/// Implicitly converts named handler options to a watch declaration.
		/// </summary>
		/// <param name="namedHandlerOptions">包含方法名和 watch 选项的对象。Object containing method name and watch options.</param>
		public static implicit operator VueWatchDeclaration<TValue>(VueWatchNamedHandlerOptions namedHandlerOptions)
			=> new(namedHandlerOptions);

		/// <summary>
		/// 从 watch 条目数组隐式转换为 watch 声明。
		/// Implicitly converts an array of watch entries to a watch declaration.
		/// </summary>
		/// <param name="entries">多个 watch 声明条目的数组。Array of multiple watch declaration entries.</param>
		public static implicit operator VueWatchDeclaration<TValue>(VueWatchEntries<TValue> entries)
			=> new(entries);
	}

	/// <summary>
	/// Options API inject 声明中 <c>from</c> 字段的联合类型。
	/// 可以是字符串键、强类型注入键或 JavaScript Symbol。
	/// Union type for the <c>from</c> field in Options API inject declarations.
	/// Can be a string key, a strongly typed injection key, or a JavaScript Symbol.
	/// </summary>
	/// <typeparam name="TValue">注入值的类型。The injected value type.</typeparam>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueInjectFrom<TValue>
	{
		private readonly byte _kind;
		private readonly string? _string;
		private readonly VueInjectionKey<TValue>? _key;
		private readonly Symbol? _symbol;

		/// <summary>
		/// 从字符串键构造。
		/// Constructs from a string key.
		/// </summary>
		/// <param name="value">字符串形式的注入源键。The injection source key as a string.</param>
		private VueInjectFrom(string value)
		{
			_kind = 1;
			_string = value;
			_key = default;
			_symbol = default;
		}

		/// <summary>
		/// 从强类型注入键构造。
		/// Constructs from a strongly typed injection key.
		/// </summary>
		/// <param name="value">强类型注入键。The strongly typed injection key.</param>
		private VueInjectFrom(VueInjectionKey<TValue> value)
		{
			_kind = 2;
			_string = default;
			_key = value;
			_symbol = default;
		}

		/// <summary>
		/// 从 JavaScript Symbol 构造。
		/// Constructs from a JavaScript Symbol.
		/// </summary>
		/// <param name="value">用作注入源键的 JavaScript Symbol。The JavaScript Symbol used as the injection source key.</param>
		private VueInjectFrom(Symbol value)
		{
			_kind = 3;
			_string = default;
			_key = default;
			_symbol = value;
		}

		/// <summary>
		/// 当值为字符串键时返回该字符串；否则返回 null。
		/// Returns the string key when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => _kind == 1 ? _string : default;

		/// <summary>
		/// 当值为强类型注入键时返回该键；否则返回 null。
		/// Returns the typed injection key when the value was created from a key; otherwise null.
		/// </summary>
		public VueInjectionKey<TValue>? AsKey => _kind == 2 ? _key : default;

		/// <summary>
		/// 当值为 Symbol 时返回该 Symbol；否则返回 null。
		/// Returns the Symbol when the value was created from a Symbol; otherwise null.
		/// </summary>
		public Symbol? AsSymbol => _kind == 3 ? _symbol : default;

		/// <summary>
		/// 从字符串键隐式转换为注入源。
		/// Implicitly converts a string key to an inject-from value.
		/// </summary>
		/// <param name="value">字符串形式的注入源键。The injection source key as a string.</param>
		public static implicit operator VueInjectFrom<TValue>(string value)
			=> new(value);

		/// <summary>
		/// 从强类型注入键隐式转换为注入源。
		/// Implicitly converts a typed injection key to an inject-from value.
		/// </summary>
		/// <param name="value">强类型注入键。The strongly typed injection key.</param>
		public static implicit operator VueInjectFrom<TValue>(VueInjectionKey<TValue> value)
			=> new(value);

		/// <summary>
		/// 从 JavaScript Symbol 隐式转换为注入源。
		/// Implicitly converts a JavaScript Symbol to an inject-from value.
		/// </summary>
		/// <param name="value">用作注入源键的 JavaScript Symbol。The JavaScript Symbol used as the injection source key.</param>
		public static implicit operator VueInjectFrom<TValue>(Symbol value)
			=> new(value);
	}

	/// <summary>
	/// Vue prop 声明的联合类型。可以是单个构造器类型、构造器类型数组或完整的 prop 选项对象。
	/// Union type for Vue prop declarations. Can be a single constructor type, a constructor type array, or full prop options.
	/// </summary>
	/// <typeparam name="TValue">prop 值的类型。The prop value type.</typeparam>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VuePropDeclaration<TValue>
	{
		private readonly byte _kind;
		private readonly VuePropType? _type;
		private readonly VuePropType?[]? _types;
		private readonly VuePropOptions<TValue>? _options;

		/// <summary>
		/// 从单个构造器类型构造。
		/// Constructs from a single constructor type.
		/// </summary>
		/// <param name="type">用于 Vue 运行时类型检查的 JavaScript 构造器。The JavaScript constructor for Vue runtime type checking.</param>
		private VuePropDeclaration(VuePropType type)
		{
			_kind = 1;
			_type = type;
			_types = default;
			_options = default;
		}

		/// <summary>
		/// 从构造器类型数组构造。
		/// Constructs from a constructor type array.
		/// </summary>
		/// <param name="types">用于 Vue 运行时类型检查的 JavaScript 构造器数组。Array of JavaScript constructors for Vue runtime type checking.</param>
		private VuePropDeclaration(VuePropType?[] types)
		{
			_kind = 2;
			_type = default;
			_types = types;
			_options = default;
		}

		/// <summary>
		/// 从完整的 prop 选项对象构造。
		/// Constructs from full prop options.
		/// </summary>
		/// <param name="options">包含类型、默认值和验证器的 prop 选项。Prop options with type, default, and validator.</param>
		private VuePropDeclaration(VuePropOptions<TValue> options)
		{
			_kind = 3;
			_type = default;
			_types = default;
			_options = options;
		}

		/// <summary>
		/// 当值为单个构造器类型时返回该类型；否则返回 null。
		/// Returns the single constructor type when the value was created from a type; otherwise null.
		/// </summary>
		public VuePropType? AsType => _kind == 1 ? _type : default;

		/// <summary>
		/// 当值为构造器类型数组时返回该数组；否则返回 null。
		/// Returns the constructor type array when the value was created from types; otherwise null.
		/// </summary>
		public VuePropType?[]? AsTypes => _kind == 2 ? _types : default;

		/// <summary>
		/// 当值为完整 prop 选项时返回该选项；否则返回 null。
		/// Returns the prop options when the value was created from options; otherwise null.
		/// </summary>
		public VuePropOptions<TValue>? AsOptions => _kind == 3 ? _options : default;

		/// <summary>
		/// 从单个构造器类型隐式转换为 prop 声明。
		/// Implicitly converts a single constructor type to a prop declaration.
		/// </summary>
		/// <param name="type">用于 Vue 运行时类型检查的 JavaScript 构造器。The JavaScript constructor for Vue runtime type checking.</param>
		public static implicit operator VuePropDeclaration<TValue>(VuePropType type)
			=> new(type);

		/// <summary>
		/// 从构造器类型数组隐式转换为 prop 声明。
		/// Implicitly converts a constructor type array to a prop declaration.
		/// </summary>
		/// <param name="types">用于 Vue 运行时类型检查的 JavaScript 构造器数组。Array of JavaScript constructors for Vue runtime type checking.</param>
		public static implicit operator VuePropDeclaration<TValue>(VuePropType?[] types)
			=> new(types);

		/// <summary>
		/// 从完整 prop 选项隐式转换为 prop 声明。
		/// Implicitly converts full prop options to a prop declaration.
		/// </summary>
		/// <param name="options">包含类型、默认值和验证器的 prop 选项。Prop options with type, default, and validator.</param>
		public static implicit operator VuePropDeclaration<TValue>(VuePropOptions<TValue> options)
			=> new(options);
	}

	/// <summary>
	/// Vue <c>class</c> 绑定值的联合类型。可以是字符串、字符串数组、对象形式或混合值数组。
	/// Union type for Vue <c>class</c> binding values. Can be a string, string array, object form, or mixed value array.
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueClassValue
	{
		private readonly byte _kind;
		private readonly string? _string;
		private readonly string[]? _strings;
		private readonly VueProps? _props;
		private readonly VueValue[]? _values;

		/// <summary>
		/// 从字符串构造。
		/// Constructs from a string.
		/// </summary>
		/// <param name="value">CSS 类名字符串。The CSS class name string.</param>
		private VueClassValue(string value)
		{
			_kind = 1;
			_string = value;
			_strings = default;
			_props = default;
			_values = default;
		}

		/// <summary>
		/// 从字符串数组构造。
		/// Constructs from a string array.
		/// </summary>
		/// <param name="value">CSS 类名字符串数组。Array of CSS class name strings.</param>
		private VueClassValue(string[] value)
		{
			_kind = 2;
			_string = default;
			_strings = value;
			_props = default;
			_values = default;
		}

		/// <summary>
		/// 从对象形式构造（键为类名，值为布尔开关）。
		/// Constructs from object form (keys are class names, values are boolean toggles).
		/// </summary>
		/// <param name="value">对象形式的类绑定。Object-form class binding.</param>
		private VueClassValue(VueProps value)
		{
			_kind = 3;
			_string = default;
			_strings = default;
			_props = value;
			_values = default;
		}

		/// <summary>
		/// 从混合值数组构造。
		/// Constructs from a mixed value array.
		/// </summary>
		/// <param name="value">混合类型的类绑定值数组。Array of mixed-type class binding values.</param>
		private VueClassValue(VueValue[] value)
		{
			_kind = 4;
			_string = default;
			_strings = default;
			_props = default;
			_values = value;
		}

		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => _kind == 1 ? _string : default;

		/// <summary>
		/// 当值为字符串数组时返回该数组；否则返回 null。
		/// Returns the string array when the value was created from strings; otherwise null.
		/// </summary>
		public string[]? AsStrings => _kind == 2 ? _strings : default;

		/// <summary>
		/// 当值为对象形式时返回该对象；否则返回 null。
		/// Returns the props object when the value was created from object form; otherwise null.
		/// </summary>
		public VueProps? AsProps => _kind == 3 ? _props : default;

		/// <summary>
		/// 当值为混合值数组时返回该数组；否则返回 null。
		/// Returns the mixed value array when the value was created from values; otherwise null.
		/// </summary>
		public VueValue[]? AsValues => _kind == 4 ? _values : default;

		/// <summary>
		/// 从字符串隐式转换为 class 绑定值。
		/// Implicitly converts a string to a class value.
		/// </summary>
		/// <param name="value">CSS 类名字符串。The CSS class name string.</param>
		public static implicit operator VueClassValue(string value)
			=> new(value);

		/// <summary>
		/// 从字符串数组隐式转换为 class 绑定值。
		/// Implicitly converts a string array to a class value.
		/// </summary>
		/// <param name="value">CSS 类名字符串数组。Array of CSS class name strings.</param>
		public static implicit operator VueClassValue(string[] value)
			=> new(value);

		/// <summary>
		/// 从对象形式隐式转换为 class 绑定值。
		/// Implicitly converts object form to a class value.
		/// </summary>
		/// <param name="value">对象形式的类绑定。Object-form class binding.</param>
		public static implicit operator VueClassValue(VueProps value)
			=> new(value);

		/// <summary>
		/// 从混合值数组隐式转换为 class 绑定值。
		/// Implicitly converts a mixed value array to a class value.
		/// </summary>
		/// <param name="value">混合类型的类绑定值数组。Array of mixed-type class binding values.</param>
		public static implicit operator VueClassValue(VueValue[] value)
			=> new(value);
	}

	/// <summary>
	/// 接受字符串或数字值的 HTML 属性联合类型（如 <c>min</c>、<c>max</c>、<c>step</c>）。
	/// Union type for HTML attributes that accept either string or numeric values (e.g. <c>min</c>, <c>max</c>, <c>step</c>).
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueStringNumberValue
	{
		private readonly byte _kind;
		private readonly double? _number;
		private readonly string? _string;

		/// <summary>
		/// 从数字值构造。
		/// Constructs from a numeric value.
		/// </summary>
		/// <param name="value">数字形式的属性值。The attribute value as a number.</param>
		private VueStringNumberValue(double value)
		{
			_kind = 1;
			_number = value;
			_string = default;
		}

		/// <summary>
		/// 从字符串值构造。
		/// Constructs from a string value.
		/// </summary>
		/// <param name="value">字符串形式的属性值。The attribute value as a string.</param>
		private VueStringNumberValue(string value)
		{
			_kind = 2;
			_number = default;
			_string = value;
		}

		/// <summary>
		/// 当值为数字时返回该数字；否则返回 null。
		/// Returns the number when the value was created from a number; otherwise null.
		/// </summary>
		public double? AsNumber => _kind == 1 ? _number : default;

		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => _kind == 2 ? _string : default;

		/// <summary>
		/// 从数字值隐式转换。
		/// Implicitly converts from a numeric value.
		/// </summary>
		/// <param name="value">数字形式的属性值。The attribute value as a number.</param>
		public static implicit operator VueStringNumberValue(double value)
			=> new(value);

		/// <summary>
		/// 从字符串值隐式转换。
		/// Implicitly converts from a string value.
		/// </summary>
		/// <param name="value">字符串形式的属性值。The attribute value as a string.</param>
		public static implicit operator VueStringNumberValue(string value)
			=> new(value);
	}

	/// <summary>
	/// <c>watch()</c> 的 <c>deep</c> 选项联合类型。可以是布尔值（启用/禁用深层遍历）或整数（限制遍历深度）。
	/// Union type for the <c>deep</c> option of <c>watch()</c>. Can be a boolean (enable/disable deep traversal) or an integer (depth limit).
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueWatchDeep
	{
		private readonly byte _kind;
		private readonly bool? _bool;
		private readonly int? _int;

		/// <summary>
		/// 从布尔值构造。
		/// Constructs from a boolean value.
		/// </summary>
		/// <param name="value">是否启用深层遍历。Whether to enable deep traversal.</param>
		private VueWatchDeep(bool value)
		{
			_kind = 1;
			_bool = value;
			_int = default;
		}

		/// <summary>
		/// 从整数值构造（作为深度限制）。
		/// Constructs from an integer value (as a depth limit).
		/// </summary>
		/// <param name="value">遍历深度限制。The traversal depth limit.</param>
		private VueWatchDeep(int value)
		{
			_kind = 2;
			_bool = default;
			_int = value;
		}

		/// <summary>
		/// 当值为布尔值时返回该值；否则返回 null。
		/// Returns the boolean when the value was created from a bool; otherwise null.
		/// </summary>
		public bool? AsBool => _kind == 1 ? _bool : default;

		/// <summary>
		/// 当值为整数时返回该值；否则返回 null。
		/// Returns the integer when the value was created from an int; otherwise null.
		/// </summary>
		public int? AsInt => _kind == 2 ? _int : default;

		/// <summary>
		/// 从布尔值隐式转换。
		/// Implicitly converts from a boolean value.
		/// </summary>
		/// <param name="value">是否启用深层遍历。Whether to enable deep traversal.</param>
		public static implicit operator VueWatchDeep(bool value)
			=> new(value);

		/// <summary>
		/// 从整数值隐式转换。
		/// Implicitly converts from an integer value.
		/// </summary>
		/// <param name="value">遍历深度限制。The traversal depth limit.</param>
		public static implicit operator VueWatchDeep(int value)
			=> new(value);
	}

	/// <summary>
	/// Transition 持续时间的联合类型。可以是数字（毫秒）或包含进入/离开阶段时间的对象。
	/// Union type for Transition duration. Can be a number (milliseconds) or an object with enter/leave phase durations.
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueTransitionDurationValue
	{
		private readonly byte _kind;
		private readonly Number? _number;
		private readonly VueTransitionDuration? _duration;

		/// <summary>
		/// 从数字值构造（毫秒数）。
		/// Constructs from a numeric value (milliseconds).
		/// </summary>
		/// <param name="value">过渡持续时间（毫秒）。The transition duration in milliseconds.</param>
		private VueTransitionDurationValue(Number value)
		{
			_kind = 1;
			_number = value;
			_duration = default;
		}

		/// <summary>
		/// 从包含进入/离开阶段时间的对象构造。
		/// Constructs from an object with enter/leave phase durations.
		/// </summary>
		/// <param name="value">分别指定进入和离开阶段的持续时间。Specifies enter and leave phase durations separately.</param>
		private VueTransitionDurationValue(VueTransitionDuration value)
		{
			_kind = 2;
			_number = default;
			_duration = value;
		}

		/// <summary>
		/// 当值为数字时返回该数字；否则返回 null。
		/// Returns the number when the value was created from a number; otherwise null.
		/// </summary>
		public Number? AsNumber => _kind == 1 ? _number : default;

		/// <summary>
		/// 当值为持续时间对象时返回该对象；否则返回 null。
		/// Returns the duration object when the value was created from a duration; otherwise null.
		/// </summary>
		public VueTransitionDuration? AsDuration => _kind == 2 ? _duration : default;

		/// <summary>
		/// 从数字值隐式转换。
		/// Implicitly converts from a numeric value.
		/// </summary>
		/// <param name="value">过渡持续时间（毫秒）。The transition duration in milliseconds.</param>
		public static implicit operator VueTransitionDurationValue(Number value)
			=> new(value);

		/// <summary>
		/// 从持续时间对象隐式转换。
		/// Implicitly converts from a duration object.
		/// </summary>
		/// <param name="value">分别指定进入和离开阶段的持续时间。Specifies enter and leave phase durations separately.</param>
		public static implicit operator VueTransitionDurationValue(VueTransitionDuration value)
			=> new(value);
	}

	/// <summary>
	/// <c>KeepAlive</c> 的 <c>include</c>/<c>exclude</c> 匹配值的联合类型。
	/// 可以是字符串、正则表达式、字符串数组或正则表达式数组。
	/// Union type for <c>KeepAlive</c> <c>include</c>/<c>exclude</c> match values.
	/// Can be a string, RegExp, string array, or RegExp array.
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueKeepAliveMatch
	{
		private readonly byte _kind;
		private readonly string? _string;
		private readonly RegExp? _regexp;
		private readonly string[]? _strings;
		private readonly RegExp[]? _regexps;

		/// <summary>
		/// 从字符串构造（组件名精确匹配）。
		/// Constructs from a string (exact component name match).
		/// </summary>
		/// <param name="value">要匹配的组件名称。The component name to match.</param>
		private VueKeepAliveMatch(string value)
		{
			_kind = 1;
			_string = value;
			_regexp = default;
			_strings = default;
			_regexps = default;
		}

		/// <summary>
		/// 从正则表达式构造。
		/// Constructs from a RegExp.
		/// </summary>
		/// <param name="value">用于匹配组件名称的正则表达式。The RegExp to match component names against.</param>
		private VueKeepAliveMatch(RegExp value)
		{
			_kind = 2;
			_string = default;
			_regexp = value;
			_strings = default;
			_regexps = default;
		}

		/// <summary>
		/// 从字符串数组构造（多个组件名精确匹配）。
		/// Constructs from a string array (exact match for multiple component names).
		/// </summary>
		/// <param name="value">要匹配的组件名称数组。Array of component names to match.</param>
		private VueKeepAliveMatch(string[] value)
		{
			_kind = 3;
			_string = default;
			_regexp = default;
			_strings = value;
			_regexps = default;
		}

		/// <summary>
		/// 从正则表达式数组构造。
		/// Constructs from a RegExp array.
		/// </summary>
		/// <param name="value">用于匹配组件名称的正则表达式数组。Array of RegExps to match component names against.</param>
		private VueKeepAliveMatch(RegExp[] value)
		{
			_kind = 4;
			_string = default;
			_regexp = default;
			_strings = default;
			_regexps = value;
		}

		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => _kind == 1 ? _string : default;

		/// <summary>
		/// 当值为正则表达式时返回该正则；否则返回 null。
		/// Returns the RegExp when the value was created from a RegExp; otherwise null.
		/// </summary>
		public RegExp? AsRegExp => _kind == 2 ? _regexp : default;

		/// <summary>
		/// 当值为字符串数组时返回该数组；否则返回 null。
		/// Returns the string array when the value was created from strings; otherwise null.
		/// </summary>
		public string[]? AsStrings => _kind == 3 ? _strings : default;

		/// <summary>
		/// 当值为正则表达式数组时返回该数组；否则返回 null。
		/// Returns the RegExp array when the value was created from RegExps; otherwise null.
		/// </summary>
		public RegExp[]? AsRegExps => _kind == 4 ? _regexps : default;

		/// <summary>
		/// 从字符串隐式转换。
		/// Implicitly converts from a string.
		/// </summary>
		/// <param name="value">要匹配的组件名称。The component name to match.</param>
		public static implicit operator VueKeepAliveMatch(string value)
			=> new(value);

		/// <summary>
		/// 从正则表达式隐式转换。
		/// Implicitly converts from a RegExp.
		/// </summary>
		/// <param name="value">用于匹配组件名称的正则表达式。The RegExp to match component names against.</param>
		public static implicit operator VueKeepAliveMatch(RegExp value)
			=> new(value);

		/// <summary>
		/// 从字符串数组隐式转换。
		/// Implicitly converts from a string array.
		/// </summary>
		/// <param name="value">要匹配的组件名称数组。Array of component names to match.</param>
		public static implicit operator VueKeepAliveMatch(string[] value)
			=> new(value);

		/// <summary>
		/// 从正则表达式数组隐式转换。
		/// Implicitly converts from a RegExp array.
		/// </summary>
		/// <param name="value">用于匹配组件名称的正则表达式数组。Array of RegExps to match component names against.</param>
		public static implicit operator VueKeepAliveMatch(RegExp[] value)
			=> new(value);
	}

	/// <summary>
	/// 接受整数或字符串值的 HTML 属性联合类型（如 <c>max</c> 用于 <c>KeepAlive</c>）。
	/// Union type for HTML attributes that accept either integer or string values (e.g. <c>max</c> for <c>KeepAlive</c>).
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueIntStringValue
	{
		private readonly byte _kind;
		private readonly int? _int;
		private readonly string? _string;

		/// <summary>
		/// 从整数值构造。
		/// Constructs from an integer value.
		/// </summary>
		/// <param name="value">整数形式的属性值。The attribute value as an integer.</param>
		private VueIntStringValue(int value)
		{
			_kind = 1;
			_int = value;
			_string = default;
		}

		/// <summary>
		/// 从字符串值构造。
		/// Constructs from a string value.
		/// </summary>
		/// <param name="value">字符串形式的属性值。The attribute value as a string.</param>
		private VueIntStringValue(string value)
		{
			_kind = 2;
			_int = default;
			_string = value;
		}

		/// <summary>
		/// 当值为整数时返回该值；否则返回 null。
		/// Returns the integer when the value was created from an int; otherwise null.
		/// </summary>
		public int? AsInt => _kind == 1 ? _int : default;

		/// <summary>
		/// 当值为字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => _kind == 2 ? _string : default;

		/// <summary>
		/// 从整数值隐式转换。
		/// Implicitly converts from an integer value.
		/// </summary>
		/// <param name="value">整数形式的属性值。The attribute value as an integer.</param>
		public static implicit operator VueIntStringValue(int value)
			=> new(value);

		/// <summary>
		/// 从字符串值隐式转换。
		/// Implicitly converts from a string value.
		/// </summary>
		/// <param name="value">字符串形式的属性值。The attribute value as a string.</param>
		public static implicit operator VueIntStringValue(string value)
			=> new(value);
	}

	/// <summary>
	/// <c>Teleport</c> 目标容器的联合类型。可以是 CSS 选择器字符串或 DOM 元素。
	/// Union type for <c>Teleport</c> target container. Can be a CSS selector string or a DOM element.
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueTeleportTarget
	{
		private readonly byte _kind;
		private readonly string? _string;
		private readonly Element? _element;

		/// <summary>
		/// 从 CSS 选择器字符串构造。
		/// Constructs from a CSS selector string.
		/// </summary>
		/// <param name="value">目标容器的 CSS 选择器。CSS selector for the target container.</param>
		private VueTeleportTarget(string value)
		{
			_kind = 1;
			_string = value;
			_element = default;
		}

		/// <summary>
		/// 从 DOM 元素构造。
		/// Constructs from a DOM element.
		/// </summary>
		/// <param name="value">目标 DOM 元素。The target DOM element.</param>
		private VueTeleportTarget(Element value)
		{
			_kind = 2;
			_string = default;
			_element = value;
		}

		/// <summary>
		/// 当值为 CSS 选择器字符串时返回该字符串；否则返回 null。
		/// Returns the string when the value was created from a string; otherwise null.
		/// </summary>
		public string? AsString => _kind == 1 ? _string : default;

		/// <summary>
		/// 当值为 DOM 元素时返回该元素；否则返回 null。
		/// Returns the element when the value was created from an element; otherwise null.
		/// </summary>
		public Element? AsElement => _kind == 2 ? _element : default;

		/// <summary>
		/// 从 CSS 选择器字符串隐式转换。
		/// Implicitly converts from a CSS selector string.
		/// </summary>
		/// <param name="value">目标容器的 CSS 选择器。CSS selector for the target container.</param>
		public static implicit operator VueTeleportTarget(string value)
			=> new(value);

		/// <summary>
		/// 从 DOM 元素隐式转换。
		/// Implicitly converts from a DOM element.
		/// </summary>
		/// <param name="value">目标 DOM 元素。The target DOM element.</param>
		public static implicit operator VueTeleportTarget(Element value)
			=> new(value);
	}
}
