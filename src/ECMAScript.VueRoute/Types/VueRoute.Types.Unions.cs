using System;
using System.ComponentModel;
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

	private RouteRecordProps(bool value)
	{
		_kind = 1;
		_bool = value;
		_props = default;
	}

	private RouteRecordProps(Vue3.VueProps value)
	{
		_kind = 2;
		_bool = default;
		_props = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public Vue3.VueProps? AsProps => _kind == 2 ? _props : default;

	public static implicit operator RouteRecordProps(bool value)
		=> new(value);

	public static implicit operator RouteRecordProps(Vue3.VueProps value)
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

	private NavigationGuardReturn(bool value)
	{
		_kind = 1;
		_bool = value;
		_location = default;
	}

	private NavigationGuardReturn(RouteLocationRaw value)
	{
		_kind = 2;
		_bool = default;
		_location = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public RouteLocationRaw? AsLocation => _kind == 2 ? _location : default;

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
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigationGuardHandler
{
	private readonly byte _kind;
	private readonly RouteNavigationGuard? _sync;
	private readonly AsyncRouteNavigationGuard? _async;

	private NavigationGuardHandler(RouteNavigationGuard value)
	{
		_kind = 1;
		_sync = value;
		_async = default;
	}

	private NavigationGuardHandler(AsyncRouteNavigationGuard value)
	{
		_kind = 2;
		_sync = default;
		_async = value;
	}

	public RouteNavigationGuard? AsSync => _kind == 1 ? _sync : default;

	public AsyncRouteNavigationGuard? AsAsync => _kind == 2 ? _async : default;

	public static implicit operator NavigationGuardHandler(RouteNavigationGuard value)
		=> new(value);

	public static implicit operator NavigationGuardHandler(AsyncRouteNavigationGuard value)
		=> new(value);
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
	private readonly string[]? _strings;
	private readonly Number? _number;
	private readonly Number[]? _numbers;

	private RouteParamRaw(string value)
	{
		_kind = 1;
		_string = value;
		_strings = default;
		_number = default;
		_numbers = default;
	}

	private RouteParamRaw(string[] value)
	{
		_kind = 2;
		_string = default;
		_strings = value;
		_number = default;
		_numbers = default;
	}

	private RouteParamRaw(Number value)
	{
		_kind = 3;
		_string = default;
		_strings = default;
		_number = value;
		_numbers = default;
	}

	private RouteParamRaw(Number[] value)
	{
		_kind = 4;
		_string = default;
		_strings = default;
		_number = default;
		_numbers = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public string[]? AsStrings => _kind == 2 ? _strings : default;

	public Number? AsNumber => _kind == 3 ? _number : default;

	public Number[]? AsNumbers => _kind == 4 ? _numbers : default;

	public static implicit operator RouteParamRaw(string value)
		=> new(value);

	public static implicit operator RouteParamRaw(string[] value)
		=> new(value);

	public static implicit operator RouteParamRaw(Number value)
		=> new(value);

	public static implicit operator RouteParamRaw(Number[] value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct LocationQueryValue
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly string[]? _strings;

	private LocationQueryValue(string value)
	{
		_kind = 1;
		_string = value;
		_strings = default;
	}

	private LocationQueryValue(string[] value)
	{
		_kind = 2;
		_string = default;
		_strings = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public string[]? AsStrings => _kind == 2 ? _strings : default;

	public static implicit operator LocationQueryValue(string value)
		=> new(value);

	public static implicit operator LocationQueryValue(string[] value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct LocationQueryValueRaw
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly string[]? _strings;
	private readonly Number? _number;
	private readonly Number[]? _numbers;

	private LocationQueryValueRaw(string value)
	{
		_kind = 1;
		_string = value;
		_strings = default;
		_number = default;
		_numbers = default;
	}

	private LocationQueryValueRaw(string[] value)
	{
		_kind = 2;
		_string = default;
		_strings = value;
		_number = default;
		_numbers = default;
	}

	private LocationQueryValueRaw(Number value)
	{
		_kind = 3;
		_string = default;
		_strings = default;
		_number = value;
		_numbers = default;
	}

	private LocationQueryValueRaw(Number[] value)
	{
		_kind = 4;
		_string = default;
		_strings = default;
		_number = default;
		_numbers = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public string[]? AsStrings => _kind == 2 ? _strings : default;

	public Number? AsNumber => _kind == 3 ? _number : default;

	public Number[]? AsNumbers => _kind == 4 ? _numbers : default;

	public static implicit operator LocationQueryValueRaw(string value)
		=> new(value);

	public static implicit operator LocationQueryValueRaw(string[] value)
		=> new(value);

	public static implicit operator LocationQueryValueRaw(Number value)
		=> new(value);

	public static implicit operator LocationQueryValueRaw(Number[] value)
		=> new(value);
}
