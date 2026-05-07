using System;
using System.ComponentModel;
using System.Linq;
using ECMAScript.Contract;

namespace ECMAScript;

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordName
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Symbol? _symbol;

	private RouteRecordName(string value)
	{
		_kind = 1;
		_string = value;
		_symbol = default;
	}

	private RouteRecordName(Symbol value)
	{
		_kind = 2;
		_string = default;
		_symbol = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Symbol? AsSymbol => _kind == 2 ? _symbol : default;

	public static implicit operator RouteRecordName(string value)
		=> new(value);

	public static implicit operator RouteRecordName(Symbol value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordAlias
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly string[]? _strings;

	private RouteRecordAlias(string value)
	{
		_kind = 1;
		_string = value;
		_strings = default;
	}

	private RouteRecordAlias(string[] value)
	{
		_kind = 2;
		_string = default;
		_strings = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public string[]? AsStrings => _kind == 2 ? _strings : default;

	public static implicit operator RouteRecordAlias(string value)
		=> new(value);

	public static implicit operator RouteRecordAlias(string[] value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteLocationRaw
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly RouteLocationAsPath? _path;
	private readonly RouteLocationAsRelative? _relative;

	private RouteLocationRaw(string value)
	{
		_kind = 1;
		_string = value;
		_path = default;
		_relative = default;
	}

	private RouteLocationRaw(RouteLocationAsPath value)
	{
		_kind = 2;
		_string = default;
		_path = value;
		_relative = default;
	}

	private RouteLocationRaw(RouteLocationAsRelative value)
	{
		_kind = 3;
		_string = default;
		_path = default;
		_relative = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public RouteLocationAsPath? AsPath => _kind == 2 ? _path : default;

	public RouteLocationAsRelative? AsRelative => _kind == 3 ? _relative : default;

	public static implicit operator RouteLocationRaw(string value)
		=> new(value);

	public static implicit operator RouteLocationRaw(RouteLocationAsPath value)
		=> new(value);

	public static implicit operator RouteLocationRaw(RouteLocationAsRelative value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteLocationRawMaybeRef
{
	private readonly byte _kind;
	private readonly RouteLocationRaw? _value;
	private readonly Vue3.IVueRef<RouteLocationRaw>? _ref;
	private readonly Vue3.VueReadonlyRef<RouteLocationRaw>? _readonlyRef;
	private readonly Vue3.IVueRef<string>? _stringRef;
	private readonly Vue3.IVueRef<RouteLocationAsPath>? _pathRef;
	private readonly Vue3.IVueRef<RouteLocationAsRelative>? _relativeRef;
	private readonly Vue3.VueReadonlyRef<string>? _readonlyStringRef;
	private readonly Vue3.VueReadonlyRef<RouteLocationAsPath>? _readonlyPathRef;
	private readonly Vue3.VueReadonlyRef<RouteLocationAsRelative>? _readonlyRelativeRef;

	private RouteLocationRawMaybeRef(RouteLocationRaw value)
	{
		_kind = 1;
		_value = value;
		_ref = default;
		_readonlyRef = default;
		_stringRef = default;
		_pathRef = default;
		_relativeRef = default;
		_readonlyStringRef = default;
		_readonlyPathRef = default;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.IVueRef<RouteLocationRaw> value)
	{
		_kind = 2;
		_value = default;
		_ref = value;
		_readonlyRef = default;
		_stringRef = default;
		_pathRef = default;
		_relativeRef = default;
		_readonlyStringRef = default;
		_readonlyPathRef = default;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationRaw> value)
	{
		_kind = 3;
		_value = default;
		_ref = default;
		_readonlyRef = value;
		_stringRef = default;
		_pathRef = default;
		_relativeRef = default;
		_readonlyStringRef = default;
		_readonlyPathRef = default;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.IVueRef<string> value)
	{
		_kind = 4;
		_value = default;
		_ref = default;
		_readonlyRef = default;
		_stringRef = value;
		_pathRef = default;
		_relativeRef = default;
		_readonlyStringRef = default;
		_readonlyPathRef = default;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.IVueRef<RouteLocationAsPath> value)
	{
		_kind = 5;
		_value = default;
		_ref = default;
		_readonlyRef = default;
		_stringRef = default;
		_pathRef = value;
		_relativeRef = default;
		_readonlyStringRef = default;
		_readonlyPathRef = default;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.IVueRef<RouteLocationAsRelative> value)
	{
		_kind = 6;
		_value = default;
		_ref = default;
		_readonlyRef = default;
		_stringRef = default;
		_pathRef = default;
		_relativeRef = value;
		_readonlyStringRef = default;
		_readonlyPathRef = default;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<string> value)
	{
		_kind = 7;
		_value = default;
		_ref = default;
		_readonlyRef = default;
		_stringRef = default;
		_pathRef = default;
		_relativeRef = default;
		_readonlyStringRef = value;
		_readonlyPathRef = default;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationAsPath> value)
	{
		_kind = 8;
		_value = default;
		_ref = default;
		_readonlyRef = default;
		_stringRef = default;
		_pathRef = default;
		_relativeRef = default;
		_readonlyStringRef = default;
		_readonlyPathRef = value;
		_readonlyRelativeRef = default;
	}

	private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationAsRelative> value)
	{
		_kind = 9;
		_value = default;
		_ref = default;
		_readonlyRef = default;
		_stringRef = default;
		_pathRef = default;
		_relativeRef = default;
		_readonlyStringRef = default;
		_readonlyPathRef = default;
		_readonlyRelativeRef = value;
	}

	public RouteLocationRaw? AsValue => _kind == 1 ? _value : default;

	public Vue3.IVueRef<RouteLocationRaw>? AsRef => _kind == 2 ? _ref : default;

	public Vue3.VueReadonlyRef<RouteLocationRaw>? AsReadonlyRef => _kind == 3 ? _readonlyRef : default;

	public Vue3.IVueRef<string>? AsStringRef => _kind == 4 ? _stringRef : default;

	public Vue3.IVueRef<RouteLocationAsPath>? AsPathRef => _kind == 5 ? _pathRef : default;

	public Vue3.IVueRef<RouteLocationAsRelative>? AsRelativeRef => _kind == 6 ? _relativeRef : default;

	public Vue3.VueReadonlyRef<string>? AsReadonlyStringRef => _kind == 7 ? _readonlyStringRef : default;

	public Vue3.VueReadonlyRef<RouteLocationAsPath>? AsReadonlyPathRef => _kind == 8 ? _readonlyPathRef : default;

	public Vue3.VueReadonlyRef<RouteLocationAsRelative>? AsReadonlyRelativeRef => _kind == 9 ? _readonlyRelativeRef : default;

	public static implicit operator RouteLocationRawMaybeRef(RouteLocationRaw value)
		=> new(value);

	public static implicit operator RouteLocationRawMaybeRef(string value)
		=> new(value);

	public static implicit operator RouteLocationRawMaybeRef(RouteLocationAsPath value)
		=> new(value);

	public static implicit operator RouteLocationRawMaybeRef(RouteLocationAsRelative value)
		=> new(value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<RouteLocationRaw> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<RouteLocationRaw> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<string> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<RouteLocationAsPath> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<RouteLocationAsRelative> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<string> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<RouteLocationAsPath> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<RouteLocationAsRelative> value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteBooleanMaybeRef
{
	private readonly byte _kind;
	private readonly bool? _value;
	private readonly Vue3.IVueRef<bool>? _ref;
	private readonly Vue3.VueReadonlyRef<bool>? _readonlyRef;

	private RouteBooleanMaybeRef(bool value)
	{
		_kind = 1;
		_value = value;
		_ref = default;
		_readonlyRef = default;
	}

	private RouteBooleanMaybeRef(Vue3.IVueRef<bool> value)
	{
		_kind = 2;
		_value = default;
		_ref = value;
		_readonlyRef = default;
	}

	private RouteBooleanMaybeRef(Vue3.VueReadonlyRef<bool> value)
	{
		_kind = 3;
		_value = default;
		_ref = default;
		_readonlyRef = value;
	}

	public bool? AsValue => _kind == 1 ? _value : default;

	public Vue3.IVueRef<bool>? AsRef => _kind == 2 ? _ref : default;

	public Vue3.VueReadonlyRef<bool>? AsReadonlyRef => _kind == 3 ? _readonlyRef : default;

	public static implicit operator RouteBooleanMaybeRef(bool value)
		=> new(value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteBooleanMaybeRef From(Vue3.IVueRef<bool> value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteBooleanMaybeRef From(Vue3.VueReadonlyRef<bool> value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HistoryStateValue
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Number? _number;
	private readonly bool? _bool;
	private readonly HistoryState? _object;
	private readonly Array<HistoryStateValue?>? _array;

	private HistoryStateValue(string value)
	{
		_kind = 1;
		_string = value;
		_number = default;
		_bool = default;
		_object = default;
		_array = default;
	}

	private HistoryStateValue(Number value)
	{
		_kind = 2;
		_string = default;
		_number = value;
		_bool = default;
		_object = default;
		_array = default;
	}

	private HistoryStateValue(bool value)
	{
		_kind = 3;
		_string = default;
		_number = default;
		_bool = value;
		_object = default;
		_array = default;
	}

	private HistoryStateValue(HistoryState value)
	{
		_kind = 4;
		_string = default;
		_number = default;
		_bool = default;
		_object = value;
		_array = default;
	}

	private HistoryStateValue(Array<HistoryStateValue?> value)
	{
		_kind = 5;
		_string = default;
		_number = default;
		_bool = default;
		_object = default;
		_array = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Number? AsNumber => _kind == 2 ? _number : default;

	public bool? AsBool => _kind == 3 ? _bool : default;

	public HistoryState? AsObject => _kind == 4 ? _object : default;

	public Array<HistoryStateValue?>? AsArray => _kind == 5 ? _array : default;

	public static implicit operator HistoryStateValue(string value)
		=> new(value);

	public static implicit operator HistoryStateValue(Number value)
		=> new(value);

	public static implicit operator HistoryStateValue(bool value)
		=> new(value);

	public static implicit operator HistoryStateValue(HistoryState value)
		=> new(value);

	public static implicit operator HistoryStateValue(Array<HistoryStateValue?> value)
		=> new(value);

	public static implicit operator HistoryStateValue(HistoryStateValue?[] value)
		=> new((Array<HistoryStateValue?>)value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouterErrorValue
{
	private readonly byte _kind;
	private readonly Error? _error;
	private readonly string? _string;
	private readonly Number? _number;
	private readonly bool? _bool;
	private readonly BigInt? _bigInt;
	private readonly Symbol? _symbol;
	private readonly IObject? _object;
	private readonly Array<RouterErrorValue?>? _array;

	private RouterErrorValue(Error value)
	{
		_kind = 1;
		_error = value;
		_string = default;
		_number = default;
		_bool = default;
		_bigInt = default;
		_symbol = default;
		_object = default;
		_array = default;
	}

	private RouterErrorValue(string value)
	{
		_kind = 2;
		_error = default;
		_string = value;
		_number = default;
		_bool = default;
		_bigInt = default;
		_symbol = default;
		_object = default;
		_array = default;
	}

	private RouterErrorValue(Number value)
	{
		_kind = 3;
		_error = default;
		_string = default;
		_number = value;
		_bool = default;
		_bigInt = default;
		_symbol = default;
		_object = default;
		_array = default;
	}

	private RouterErrorValue(bool value)
	{
		_kind = 4;
		_error = default;
		_string = default;
		_number = default;
		_bool = value;
		_bigInt = default;
		_symbol = default;
		_object = default;
		_array = default;
	}

	private RouterErrorValue(BigInt value)
	{
		_kind = 5;
		_error = default;
		_string = default;
		_number = default;
		_bool = default;
		_bigInt = value;
		_symbol = default;
		_object = default;
		_array = default;
	}

	private RouterErrorValue(Symbol value)
	{
		_kind = 6;
		_error = default;
		_string = default;
		_number = default;
		_bool = default;
		_bigInt = default;
		_symbol = value;
		_object = default;
		_array = default;
	}

	private RouterErrorValue(IObject value)
	{
		_kind = 7;
		_error = default;
		_string = default;
		_number = default;
		_bool = default;
		_bigInt = default;
		_symbol = default;
		_object = value;
		_array = default;
	}

	private RouterErrorValue(Array<RouterErrorValue?> value)
	{
		_kind = 8;
		_error = default;
		_string = default;
		_number = default;
		_bool = default;
		_bigInt = default;
		_symbol = default;
		_object = default;
		_array = value;
	}

	public Error? AsError => _kind == 1 ? _error : default;

	public string? AsString => _kind == 2 ? _string : default;

	public Number? AsNumber => _kind == 3 ? _number : default;

	public bool? AsBool => _kind == 4 ? _bool : default;

	public BigInt? AsBigInt => _kind == 5 ? _bigInt : default;

	public Symbol? AsSymbol => _kind == 6 ? _symbol : default;

	public IObject? AsObject => _kind == 7 ? _object : default;

	public Array<RouterErrorValue?>? AsArray => _kind == 8 ? _array : default;

	public static implicit operator RouterErrorValue(Error value)
		=> new(value);

	public static implicit operator RouterErrorValue(string value)
		=> new(value);

	public static implicit operator RouterErrorValue(Number value)
		=> new(value);

	public static implicit operator RouterErrorValue(bool value)
		=> new(value);

	public static implicit operator RouterErrorValue(BigInt value)
		=> new(value);

	public static implicit operator RouterErrorValue(Symbol value)
		=> new(value);

	public static implicit operator RouterErrorValue(Array<RouterErrorValue?> value)
		=> new(value);

	public static implicit operator RouterErrorValue(RouterErrorValue?[] value)
		=> new((Array<RouterErrorValue?>)value);

	[ECMAScriptInline("__arg1")]
	public extern static RouterErrorValue From(IObject value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RawRouteComponent
{
	private readonly byte _kind;
	private readonly IVueComponent? _component;
	private readonly RouteComponentLoader? _loader;

	private RawRouteComponent(IVueComponent value)
	{
		_kind = 1;
		_component = value;
		_loader = default;
	}

	private RawRouteComponent(RouteComponentLoader value)
	{
		_kind = 2;
		_component = default;
		_loader = value;
	}

	public IVueComponent? AsComponent => _kind == 1 ? _component : default;

	public RouteComponentLoader? AsLoader => _kind == 2 ? _loader : default;

	[ECMAScriptInline("__arg1")]
	public extern static RawRouteComponent From(IVueComponent value);

	[ECMAScriptInline("__arg1")]
	public extern static RawRouteComponent From(RouteComponentLoader value);

	public static implicit operator RawRouteComponent(RouteComponentLoader value)
		=> new(value);

	public static implicit operator RawRouteComponent(RouteComponent value)
		=> new(value.AsComponent!);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteComponent
{
	private readonly byte _kind;
	private readonly IVueComponent? _component;
	private readonly RouteComponentLoader? _loader;

	private RouteComponent(IVueComponent value)
	{
		_kind = 1;
		_component = value;
		_loader = default;
	}

	private RouteComponent(RouteComponentLoader value)
	{
		_kind = 2;
		_component = default;
		_loader = value;
	}

	public IVueComponent? AsComponent => _kind == 1 ? _component : default;

	public RouteComponentLoader? AsLoader => _kind == 2 ? _loader : default;

	[ECMAScriptInline("__arg1")]
	public extern static RouteComponent From(IVueComponent value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteComponent From(RouteComponentLoader value);

	public static implicit operator RouteComponent(RouteComponentLoader value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordProps
{
	private readonly byte _kind;
	private readonly bool? _bool;
	private readonly Vue3.VueProps? _props;
	private readonly RouteRecordPropsResolver? _resolver;

	private RouteRecordProps(bool value)
	{
		_kind = 1;
		_bool = value;
		_props = default;
		_resolver = default;
	}

	private RouteRecordProps(Vue3.VueProps value)
	{
		_kind = 2;
		_bool = default;
		_props = value;
		_resolver = default;
	}

	private RouteRecordProps(RouteRecordPropsResolver value)
	{
		_kind = 3;
		_bool = default;
		_props = default;
		_resolver = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public Vue3.VueProps? AsProps => _kind == 2 ? _props : default;

	public RouteRecordPropsResolver? AsResolver => _kind == 3 ? _resolver : default;

	public static implicit operator RouteRecordProps(bool value)
		=> new(value);

	public static implicit operator RouteRecordProps(Vue3.VueProps value)
		=> new(value);

	public static implicit operator RouteRecordProps(RouteRecordPropsResolver value)
		=> new(value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordProps From(bool value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordProps From(Vue3.VueProps value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordProps From(RouteRecordPropsResolver value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordNamedViewProps
{
	private readonly byte _kind;
	private readonly bool? _bool;
	private readonly RouteNamedProps? _namedProps;

	private RouteRecordNamedViewProps(bool value)
	{
		_kind = 1;
		_bool = value;
		_namedProps = default;
	}

	private RouteRecordNamedViewProps(RouteNamedProps value)
	{
		_kind = 2;
		_bool = default;
		_namedProps = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public RouteNamedProps? AsNamedProps => _kind == 2 ? _namedProps : default;

	public static implicit operator RouteRecordNamedViewProps(bool value)
		=> new(value);

	public static implicit operator RouteRecordNamedViewProps(RouteNamedProps value)
		=> new(value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordNamedViewProps From(bool value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordNamedViewProps From(RouteNamedProps value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigationGuardNextArgument
{
	private readonly byte _kind;
	private readonly bool? _bool;
	private readonly RouteLocationRaw? _location;
	private readonly NavigationGuardNextCallback? _callback;
	private readonly Error? _error;

	private NavigationGuardNextArgument(bool value)
	{
		_kind = 1;
		_bool = value;
		_location = default;
		_callback = default;
		_error = default;
	}

	private NavigationGuardNextArgument(RouteLocationRaw value)
	{
		_kind = 2;
		_bool = default;
		_location = value;
		_callback = default;
		_error = default;
	}

	private NavigationGuardNextArgument(NavigationGuardNextCallback value)
	{
		_kind = 3;
		_bool = default;
		_location = default;
		_callback = value;
		_error = default;
	}

	private NavigationGuardNextArgument(Error value)
	{
		_kind = 4;
		_bool = default;
		_location = default;
		_callback = default;
		_error = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public RouteLocationRaw? AsLocation => _kind == 2 ? _location : default;

	public NavigationGuardNextCallback? AsCallback => _kind == 3 ? _callback : default;

	public Error? AsError => _kind == 4 ? _error : default;

	public static implicit operator NavigationGuardNextArgument(bool value)
		=> new(value);

	public static implicit operator NavigationGuardNextArgument(RouteLocationRaw value)
		=> new(value);

	public static implicit operator NavigationGuardNextArgument(string value)
		=> new(value);

	public static implicit operator NavigationGuardNextArgument(RouteLocationAsPath value)
		=> new(value);

	public static implicit operator NavigationGuardNextArgument(RouteLocationAsRelative value)
		=> new(value);

	public static implicit operator NavigationGuardNextArgument(NavigationGuardNextCallback value)
		=> new(value);

	public static implicit operator NavigationGuardNextArgument(Error value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigationGuardReturn
{
	private readonly byte _kind;
	private readonly bool? _bool;
	private readonly RouteLocationRaw? _location;
	private readonly Error? _error;

	private NavigationGuardReturn(bool value)
	{
		_kind = 1;
		_bool = value;
		_location = default;
		_error = default;
	}

	private NavigationGuardReturn(RouteLocationRaw value)
	{
		_kind = 2;
		_bool = default;
		_location = value;
		_error = default;
	}

	private NavigationGuardReturn(Error value)
	{
		_kind = 3;
		_bool = default;
		_location = default;
		_error = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public RouteLocationRaw? AsLocation => _kind == 2 ? _location : default;

	public Error? AsError => _kind == 3 ? _error : default;

	public static implicit operator NavigationGuardReturn(bool value)
		=> new(value);

	public static implicit operator NavigationGuardReturn(RouteLocationRaw value)
		=> new(value);

	public static implicit operator NavigationGuardReturn(string value)
		=> new(value);

	public static implicit operator NavigationGuardReturn(RouteLocationAsPath value)
		=> new(value);

	public static implicit operator NavigationGuardReturn(RouteLocationAsRelative value)
		=> new(value);

	public static implicit operator NavigationGuardReturn(Error value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteNavigationResult
{
	private readonly byte _kind;
	private readonly NavigationFailure? _failure;

	private RouteNavigationResult(NavigationFailure value)
	{
		_kind = 1;
		_failure = value;
	}

	public NavigationFailure? AsFailure => _kind == 1 ? _failure : default;

	public static implicit operator RouteNavigationResult(NavigationFailure value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigationGuardHandler
{
	private readonly byte _kind;
	private readonly RouteNavigationGuard? _sync;
	private readonly AsyncRouteNavigationGuard? _async;
	private readonly LegacyRouteNavigationGuard? _legacySync;
	private readonly LegacyAsyncRouteNavigationGuard? _legacyAsync;

	private NavigationGuardHandler(RouteNavigationGuard value)
	{
		_kind = 1;
		_sync = value;
		_async = default;
		_legacySync = default;
		_legacyAsync = default;
	}

	private NavigationGuardHandler(AsyncRouteNavigationGuard value)
	{
		_kind = 2;
		_sync = default;
		_async = value;
		_legacySync = default;
		_legacyAsync = default;
	}

	private NavigationGuardHandler(LegacyRouteNavigationGuard value)
	{
		_kind = 3;
		_sync = default;
		_async = default;
		_legacySync = value;
		_legacyAsync = default;
	}

	private NavigationGuardHandler(LegacyAsyncRouteNavigationGuard value)
	{
		_kind = 4;
		_sync = default;
		_async = default;
		_legacySync = default;
		_legacyAsync = value;
	}

	public RouteNavigationGuard? AsSync => _kind == 1 ? _sync : default;

	public AsyncRouteNavigationGuard? AsAsync => _kind == 2 ? _async : default;

	public LegacyRouteNavigationGuard? AsLegacySync => _kind == 3 ? _legacySync : default;

	public LegacyAsyncRouteNavigationGuard? AsLegacyAsync => _kind == 4 ? _legacyAsync : default;

	public static implicit operator NavigationGuardHandler(RouteNavigationGuard value)
		=> new(value);

	public static implicit operator NavigationGuardHandler(AsyncRouteNavigationGuard value)
		=> new(value);

	public static implicit operator NavigationGuardHandler(LegacyRouteNavigationGuard value)
		=> new(value);

	public static implicit operator NavigationGuardHandler(LegacyAsyncRouteNavigationGuard value)
		=> new(value);

	[ECMAScriptInline("__arg1")]
	public extern static NavigationGuardHandler From(RouteNavigationGuard value);

	[ECMAScriptInline("__arg1")]
	public extern static NavigationGuardHandler From(AsyncRouteNavigationGuard value);

	[ECMAScriptInline("__arg1")]
	public extern static NavigationGuardHandler From(LegacyRouteNavigationGuard value);

	[ECMAScriptInline("__arg1")]
	public extern static NavigationGuardHandler From(LegacyAsyncRouteNavigationGuard value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordBeforeEnter
{
	private readonly byte _kind;
	private readonly NavigationGuardHandler? _guard;
	private readonly NavigationGuardHandler[]? _guards;

	private RouteRecordBeforeEnter(NavigationGuardHandler value)
	{
		_kind = 1;
		_guard = value;
		_guards = default;
	}

	private RouteRecordBeforeEnter(NavigationGuardHandler[] value)
	{
		_kind = 2;
		_guard = default;
		_guards = value;
	}

	public NavigationGuardHandler? AsGuard => _kind == 1 ? _guard : default;

	public NavigationGuardHandler[]? AsGuards => _kind == 2 ? _guards : default;

	public static implicit operator RouteRecordBeforeEnter(NavigationGuardHandler value)
		=> new(value);

	public static implicit operator RouteRecordBeforeEnter(NavigationGuardHandler[] value)
		=> new(value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordBeforeEnter From(NavigationGuardHandler value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordBeforeEnter From(RouteNavigationGuard value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordBeforeEnter From(AsyncRouteNavigationGuard value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordBeforeEnter From(LegacyRouteNavigationGuard value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRecordBeforeEnter From(LegacyAsyncRouteNavigationGuard value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRedirectOption
{
	private readonly byte _kind;
	private readonly RouteLocationRaw? _location;
	private readonly RouteRedirectCallback? _callback;

	private RouteRedirectOption(RouteLocationRaw value)
	{
		_kind = 1;
		_location = value;
		_callback = default;
	}

	private RouteRedirectOption(RouteRedirectCallback value)
	{
		_kind = 2;
		_location = default;
		_callback = value;
	}

	public RouteLocationRaw? AsLocation => _kind == 1 ? _location : default;

	public RouteRedirectCallback? AsCallback => _kind == 2 ? _callback : default;

	public static implicit operator RouteRedirectOption(RouteLocationRaw value)
		=> new(value);

	public static implicit operator RouteRedirectOption(string value)
		=> new(value);

	public static implicit operator RouteRedirectOption(RouteLocationAsPath value)
		=> new(value);

	public static implicit operator RouteRedirectOption(RouteLocationAsRelative value)
		=> new(value);

	public static implicit operator RouteRedirectOption(RouteRedirectCallback value)
		=> new(value);

	public static implicit operator RouteRedirectOption(Func<RouteLocation, RouteLocationNormalizedLoaded, RouteLocationRaw> value)
		=> new(new RouteRedirectCallback(value));

	[ECMAScriptInline("__arg1")]
	public extern static RouteRedirectOption From(RouteLocationRaw value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteRedirectOption From(RouteRedirectCallback value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordRaw
{
	private readonly byte _kind;
	private readonly RouteRecordSingleView? _singleView;
	private readonly RouteRecordSingleViewWithChildren? _singleViewWithChildren;
	private readonly RouteRecordMultipleViews? _multipleViews;
	private readonly RouteRecordMultipleViewsWithChildren? _multipleViewsWithChildren;
	private readonly RouteRecordRedirect? _redirect;

	private RouteRecordRaw(RouteRecordSingleView value)
	{
		_kind = 1;
		_singleView = value;
		_singleViewWithChildren = default;
		_multipleViews = default;
		_multipleViewsWithChildren = default;
		_redirect = default;
	}

	private RouteRecordRaw(RouteRecordSingleViewWithChildren value)
	{
		_kind = 2;
		_singleView = default;
		_singleViewWithChildren = value;
		_multipleViews = default;
		_multipleViewsWithChildren = default;
		_redirect = default;
	}

	private RouteRecordRaw(RouteRecordMultipleViews value)
	{
		_kind = 3;
		_singleView = default;
		_singleViewWithChildren = default;
		_multipleViews = value;
		_multipleViewsWithChildren = default;
		_redirect = default;
	}

	private RouteRecordRaw(RouteRecordMultipleViewsWithChildren value)
	{
		_kind = 4;
		_singleView = default;
		_singleViewWithChildren = default;
		_multipleViews = default;
		_multipleViewsWithChildren = value;
		_redirect = default;
	}

	private RouteRecordRaw(RouteRecordRedirect value)
	{
		_kind = 5;
		_singleView = default;
		_singleViewWithChildren = default;
		_multipleViews = default;
		_multipleViewsWithChildren = default;
		_redirect = value;
	}

	public RouteRecordSingleView? AsSingleView => _kind == 1 ? _singleView : default;

	public RouteRecordSingleViewWithChildren? AsSingleViewWithChildren => _kind == 2 ? _singleViewWithChildren : default;

	public RouteRecordMultipleViews? AsMultipleViews => _kind == 3 ? _multipleViews : default;

	public RouteRecordMultipleViewsWithChildren? AsMultipleViewsWithChildren => _kind == 4 ? _multipleViewsWithChildren : default;

	public RouteRecordRedirect? AsRedirect => _kind == 5 ? _redirect : default;

	public static implicit operator RouteRecordRaw(RouteRecordSingleView value)
		=> new(value);

	public static implicit operator RouteRecordRaw(RouteRecordSingleViewWithChildren value)
		=> new(value);

	public static implicit operator RouteRecordRaw(RouteRecordMultipleViews value)
		=> new(value);

	public static implicit operator RouteRecordRaw(RouteRecordMultipleViewsWithChildren value)
		=> new(value);

	public static implicit operator RouteRecordRaw(RouteRecordRedirect value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteParam
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly string[]? _strings;

	private RouteParam(string value)
	{
		_kind = 1;
		_string = value;
		_strings = default;
	}

	private RouteParam(string[] value)
	{
		_kind = 2;
		_string = default;
		_strings = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public string[]? AsStrings => _kind == 2 ? _strings : default;

	public static implicit operator RouteParam(string value)
		=> new(value);

	public static implicit operator RouteParam(string[] value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteParamRaw
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Array<RouteParamRaw>? _array;
	private readonly Number? _number;

	private RouteParamRaw(string value)
	{
		_kind = 1;
		_string = value;
		_array = default;
		_number = default;
	}

	private RouteParamRaw(Array<RouteParamRaw> value)
	{
		_kind = 2;
		_string = default;
		_array = value;
		_number = default;
	}

	private RouteParamRaw(Number value)
	{
		_kind = 3;
		_string = default;
		_array = default;
		_number = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Array<RouteParamRaw>? AsArray => _kind == 2 ? _array : default;

	public Number? AsNumber => _kind == 3 ? _number : default;

	public static implicit operator RouteParamRaw(string value)
		=> new(value);

	public static implicit operator RouteParamRaw(string[] value)
		=> new((Array<RouteParamRaw>)value.Select(static item => (RouteParamRaw)item).ToArray());

	public static implicit operator RouteParamRaw(Number value)
		=> new(value);

	public static implicit operator RouteParamRaw(Array<RouteParamRaw> value)
		=> new(value);

	public static implicit operator RouteParamRaw(RouteParamRaw[] value)
		=> new((Array<RouteParamRaw>)value);

	public static implicit operator RouteParamRaw(Number[] value)
		=> new((Array<RouteParamRaw>)value.Select(static item => (RouteParamRaw)item).ToArray());
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct LocationQueryValue
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Array<string?>? _array;

	private LocationQueryValue(string value)
	{
		_kind = 1;
		_string = value;
		_array = default;
	}

	private LocationQueryValue(Array<string?> value)
	{
		_kind = 2;
		_string = default;
		_array = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Array<string?>? AsArray => _kind == 2 ? _array : default;

	public static implicit operator LocationQueryValue(string value)
		=> new(value);

	public static implicit operator LocationQueryValue(string[] value)
		=> new((Array<string?>)value);

	public static implicit operator LocationQueryValue(Array<string?> value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct LocationQueryValueRaw
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Array<LocationQueryValueRaw?>? _array;
	private readonly Number? _number;

	private LocationQueryValueRaw(string value)
	{
		_kind = 1;
		_string = value;
		_array = default;
		_number = default;
	}

	private LocationQueryValueRaw(Array<LocationQueryValueRaw?> value)
	{
		_kind = 2;
		_string = default;
		_array = value;
		_number = default;
	}

	private LocationQueryValueRaw(Number value)
	{
		_kind = 3;
		_string = default;
		_array = default;
		_number = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Array<LocationQueryValueRaw?>? AsArray => _kind == 2 ? _array : default;

	public Number? AsNumber => _kind == 3 ? _number : default;

	public static implicit operator LocationQueryValueRaw(string value)
		=> new(value);

	public static implicit operator LocationQueryValueRaw(string[] value)
		=> new((Array<LocationQueryValueRaw?>)value.Select(static item => (LocationQueryValueRaw?)item).ToArray());

	public static implicit operator LocationQueryValueRaw(Number value)
		=> new(value);

	public static implicit operator LocationQueryValueRaw(Array<LocationQueryValueRaw?> value)
		=> new(value);

	public static implicit operator LocationQueryValueRaw(LocationQueryValueRaw?[] value)
		=> new((Array<LocationQueryValueRaw?>)value);

	public static implicit operator LocationQueryValueRaw(Number[] value)
		=> new((Array<LocationQueryValueRaw?>)value.Select(static item => (LocationQueryValueRaw?)item).ToArray());
}
