using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Vue3
{
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueComputedValue<TValue>
	{
		private readonly byte _kind;
		private readonly Func<TValue>? _getter;
		private readonly VueWritableComputedOptions<TValue>? _options;

		private VueComputedValue(Func<TValue> getter)
		{
			_kind = 1;
			_getter = getter;
			_options = default;
		}

		private VueComputedValue(VueWritableComputedOptions<TValue> options)
		{
			_kind = 2;
			_getter = default;
			_options = options;
		}

		public Func<TValue>? AsGetter => _kind == 1 ? _getter : default;

		public VueWritableComputedOptions<TValue>? AsOptions => _kind == 2 ? _options : default;

		public static implicit operator VueComputedValue<TValue>(Func<TValue> getter)
			=> new(getter);

		public static implicit operator VueComputedValue<TValue>(VueWritableComputedOptions<TValue> options)
			=> new(options);
	}

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

		public string? AsMethodName => _kind == 1 ? _methodName : default;

		public Action<TValue, TValue>? AsHandler => _kind == 2 ? _handler : default;

		public VueWatchCleanupCallback<TValue>? AsCleanupHandler => _kind == 3 ? _cleanupHandler : default;

		public VueWatchHandlerOptions<TValue>? AsHandlerOptions => _kind == 4 ? _handlerOptions : default;

		public VueWatchCleanupHandlerOptions<TValue>? AsCleanupHandlerOptions => _kind == 5 ? _cleanupHandlerOptions : default;

		public VueWatchNamedHandlerOptions? AsNamedHandlerOptions => _kind == 6 ? _namedHandlerOptions : default;

		public VueWatchEntries<TValue>? AsEntries => _kind == 7 ? _entries : default;

		public static implicit operator VueWatchDeclaration<TValue>(string methodName)
			=> new(methodName);

		public static implicit operator VueWatchDeclaration<TValue>(Action<TValue, TValue> handler)
			=> new(handler);

		public static implicit operator VueWatchDeclaration<TValue>(VueWatchCleanupCallback<TValue> cleanupHandler)
			=> new(cleanupHandler);

		public static implicit operator VueWatchDeclaration<TValue>(VueWatchHandlerOptions<TValue> handlerOptions)
			=> new(handlerOptions);

		public static implicit operator VueWatchDeclaration<TValue>(VueWatchCleanupHandlerOptions<TValue> cleanupHandlerOptions)
			=> new(cleanupHandlerOptions);

		public static implicit operator VueWatchDeclaration<TValue>(VueWatchNamedHandlerOptions namedHandlerOptions)
			=> new(namedHandlerOptions);

		public static implicit operator VueWatchDeclaration<TValue>(VueWatchEntries<TValue> entries)
			=> new(entries);
	}

	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueInjectFrom<TValue>
	{
		private readonly byte _kind;
		private readonly string? _string;
		private readonly VueInjectionKey<TValue>? _key;
		private readonly Symbol? _symbol;

		private VueInjectFrom(string value)
		{
			_kind = 1;
			_string = value;
			_key = default;
			_symbol = default;
		}

		private VueInjectFrom(VueInjectionKey<TValue> value)
		{
			_kind = 2;
			_string = default;
			_key = value;
			_symbol = default;
		}

		private VueInjectFrom(Symbol value)
		{
			_kind = 3;
			_string = default;
			_key = default;
			_symbol = value;
		}

		public string? AsString => _kind == 1 ? _string : default;

		public VueInjectionKey<TValue>? AsKey => _kind == 2 ? _key : default;

		public Symbol? AsSymbol => _kind == 3 ? _symbol : default;

		public static implicit operator VueInjectFrom<TValue>(string value)
			=> new(value);

		public static implicit operator VueInjectFrom<TValue>(VueInjectionKey<TValue> value)
			=> new(value);

		public static implicit operator VueInjectFrom<TValue>(Symbol value)
			=> new(value);
	}

	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VuePropDeclaration<TValue>
	{
		private readonly byte _kind;
		private readonly VuePropType? _type;
		private readonly VuePropType?[]? _types;
		private readonly VuePropOptions<TValue>? _options;

		private VuePropDeclaration(VuePropType type)
		{
			_kind = 1;
			_type = type;
			_types = default;
			_options = default;
		}

		private VuePropDeclaration(VuePropType?[] types)
		{
			_kind = 2;
			_type = default;
			_types = types;
			_options = default;
		}

		private VuePropDeclaration(VuePropOptions<TValue> options)
		{
			_kind = 3;
			_type = default;
			_types = default;
			_options = options;
		}

		public VuePropType? AsType => _kind == 1 ? _type : default;

		public VuePropType?[]? AsTypes => _kind == 2 ? _types : default;

		public VuePropOptions<TValue>? AsOptions => _kind == 3 ? _options : default;

		public static implicit operator VuePropDeclaration<TValue>(VuePropType type)
			=> new(type);

		public static implicit operator VuePropDeclaration<TValue>(VuePropType?[] types)
			=> new(types);

		public static implicit operator VuePropDeclaration<TValue>(VuePropOptions<TValue> options)
			=> new(options);
	}

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

		private VueClassValue(string value)
		{
			_kind = 1;
			_string = value;
			_strings = default;
			_props = default;
			_values = default;
		}

		private VueClassValue(string[] value)
		{
			_kind = 2;
			_string = default;
			_strings = value;
			_props = default;
			_values = default;
		}

		private VueClassValue(VueProps value)
		{
			_kind = 3;
			_string = default;
			_strings = default;
			_props = value;
			_values = default;
		}

		private VueClassValue(VueValue[] value)
		{
			_kind = 4;
			_string = default;
			_strings = default;
			_props = default;
			_values = value;
		}

		public string? AsString => _kind == 1 ? _string : default;

		public string[]? AsStrings => _kind == 2 ? _strings : default;

		public VueProps? AsProps => _kind == 3 ? _props : default;

		public VueValue[]? AsValues => _kind == 4 ? _values : default;

		public static implicit operator VueClassValue(string value)
			=> new(value);

		public static implicit operator VueClassValue(string[] value)
			=> new(value);

		public static implicit operator VueClassValue(VueProps value)
			=> new(value);

		public static implicit operator VueClassValue(VueValue[] value)
			=> new(value);
	}

	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueStringNumberValue
	{
		private readonly byte _kind;
		private readonly double? _number;
		private readonly string? _string;

		private VueStringNumberValue(double value)
		{
			_kind = 1;
			_number = value;
			_string = default;
		}

		private VueStringNumberValue(string value)
		{
			_kind = 2;
			_number = default;
			_string = value;
		}

		public double? AsNumber => _kind == 1 ? _number : default;

		public string? AsString => _kind == 2 ? _string : default;

		public static implicit operator VueStringNumberValue(double value)
			=> new(value);

		public static implicit operator VueStringNumberValue(string value)
			=> new(value);
	}

	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueWatchDeep
	{
		private readonly byte _kind;
		private readonly bool? _bool;
		private readonly int? _int;

		private VueWatchDeep(bool value)
		{
			_kind = 1;
			_bool = value;
			_int = default;
		}

		private VueWatchDeep(int value)
		{
			_kind = 2;
			_bool = default;
			_int = value;
		}

		public bool? AsBool => _kind == 1 ? _bool : default;

		public int? AsInt => _kind == 2 ? _int : default;

		public static implicit operator VueWatchDeep(bool value)
			=> new(value);

		public static implicit operator VueWatchDeep(int value)
			=> new(value);
	}

	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueTransitionDurationValue
	{
		private readonly byte _kind;
		private readonly Number? _number;
		private readonly VueTransitionDuration? _duration;

		private VueTransitionDurationValue(Number value)
		{
			_kind = 1;
			_number = value;
			_duration = default;
		}

		private VueTransitionDurationValue(VueTransitionDuration value)
		{
			_kind = 2;
			_number = default;
			_duration = value;
		}

		public Number? AsNumber => _kind == 1 ? _number : default;

		public VueTransitionDuration? AsDuration => _kind == 2 ? _duration : default;

		public static implicit operator VueTransitionDurationValue(Number value)
			=> new(value);

		public static implicit operator VueTransitionDurationValue(VueTransitionDuration value)
			=> new(value);
	}

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

		private VueKeepAliveMatch(string value)
		{
			_kind = 1;
			_string = value;
			_regexp = default;
			_strings = default;
			_regexps = default;
		}

		private VueKeepAliveMatch(RegExp value)
		{
			_kind = 2;
			_string = default;
			_regexp = value;
			_strings = default;
			_regexps = default;
		}

		private VueKeepAliveMatch(string[] value)
		{
			_kind = 3;
			_string = default;
			_regexp = default;
			_strings = value;
			_regexps = default;
		}

		private VueKeepAliveMatch(RegExp[] value)
		{
			_kind = 4;
			_string = default;
			_regexp = default;
			_strings = default;
			_regexps = value;
		}

		public string? AsString => _kind == 1 ? _string : default;

		public RegExp? AsRegExp => _kind == 2 ? _regexp : default;

		public string[]? AsStrings => _kind == 3 ? _strings : default;

		public RegExp[]? AsRegExps => _kind == 4 ? _regexps : default;

		public static implicit operator VueKeepAliveMatch(string value)
			=> new(value);

		public static implicit operator VueKeepAliveMatch(RegExp value)
			=> new(value);

		public static implicit operator VueKeepAliveMatch(string[] value)
			=> new(value);

		public static implicit operator VueKeepAliveMatch(RegExp[] value)
			=> new(value);
	}

	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueIntStringValue
	{
		private readonly byte _kind;
		private readonly int? _int;
		private readonly string? _string;

		private VueIntStringValue(int value)
		{
			_kind = 1;
			_int = value;
			_string = default;
		}

		private VueIntStringValue(string value)
		{
			_kind = 2;
			_int = default;
			_string = value;
		}

		public int? AsInt => _kind == 1 ? _int : default;

		public string? AsString => _kind == 2 ? _string : default;

		public static implicit operator VueIntStringValue(int value)
			=> new(value);

		public static implicit operator VueIntStringValue(string value)
			=> new(value);
	}

	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct VueTeleportTarget
	{
		private readonly byte _kind;
		private readonly string? _string;
		private readonly Element? _element;

		private VueTeleportTarget(string value)
		{
			_kind = 1;
			_string = value;
			_element = default;
		}

		private VueTeleportTarget(Element value)
		{
			_kind = 2;
			_string = default;
			_element = value;
		}

		public string? AsString => _kind == 1 ? _string : default;

		public Element? AsElement => _kind == 2 ? _element : default;

		public static implicit operator VueTeleportTarget(string value)
			=> new(value);

		public static implicit operator VueTeleportTarget(Element value)
			=> new(value);
	}
}
