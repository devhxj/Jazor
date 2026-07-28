using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 警告框类型。
/// Vuetify alert type.
/// </summary>
[String]
public enum VuetifyAlertType
{
    [Description("@#success")]
    Success,

    [Description("@#info")]
    Info,

    [Description("@#warning")]
    Warning,

    [Description("@#error")]
    Error
}

[String]
public enum VuetifyAlertBorderSide
{
    [Description("@#top")]
    Top,

    [Description("@#end")]
    End,

    [Description("@#bottom")]
    Bottom,

    [Description("@#start")]
    Start
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyAlertBorderValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyAlertBorderSide? _side;

    public VuetifyAlertBorderValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _side = default;
    }

    public VuetifyAlertBorderValue(VuetifyAlertBorderSide value)
    {
        _kind = 2;
        _bool = default;
        _side = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyAlertBorderSide? AsSide => _kind == 2 ? _side : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsSide,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAlertBorderValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAlertBorderValue From(VuetifyAlertBorderSide value);

    public static implicit operator VuetifyAlertBorderValue(bool value)
        => new(value);

    public static implicit operator VuetifyAlertBorderValue(VuetifyAlertBorderSide value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyAlertIconValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Symbol? _symbol;
    private readonly VueProps? _props;

    public VuetifyAlertIconValue(string value)
    {
        _kind = 1;
        _string = value;
        _symbol = default;
        _props = default;
    }

    public VuetifyAlertIconValue(Symbol value)
    {
        _kind = 2;
        _string = default;
        _symbol = value;
        _props = default;
    }

    public VuetifyAlertIconValue(VueProps value)
    {
        _kind = 3;
        _string = default;
        _symbol = default;
        _props = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Symbol? AsSymbol => _kind == 2 ? _symbol : default;

    public VueProps? AsProps => _kind == 3 ? _props : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsSymbol,
        3 => AsProps,
        _ => default
    };

    [ECMAScriptInline("false")]
    public extern static VuetifyAlertIconValue None();

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAlertIconValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAlertIconValue From(Symbol value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAlertIconValue From(VueProps value);

    public static implicit operator VuetifyAlertIconValue(string value)
        => new(value);

    public static implicit operator VuetifyAlertIconValue(Symbol value)
        => new(value);

    public static implicit operator VuetifyAlertIconValue(VueProps value)
        => new(value);

    public static implicit operator VuetifyAlertIconValue(VueDictionary value)
        => new(value);
}

[String]
public enum VuetifyDensity
{
    [Description("@#default")]
    Default,

    [Description("@#comfortable")]
    Comfortable,

    [Description("@#compact")]
    Compact
}

[String]
public enum VuetifyVariant
{
    [Description("@#elevated")]
    Elevated,

    [Description("@#flat")]
    Flat,

    [Description("@#outlined")]
    Outlined,

    [Description("@#text")]
    Text,

    [Description("@#tonal")]
    Tonal,

    [Description("@#plain")]
    Plain
}

[String]
public enum VuetifyFieldVariant
{
    [Description("@#underlined")]
    Underlined,

    [Description("@#outlined")]
    Outlined,

    [Description("@#filled")]
    Filled,

    [Description("@#solo")]
    Solo,

    [Description("@#solo-inverted")]
    SoloInverted,

    [Description("@#solo-filled")]
    SoloFilled,

    [Description("@#plain")]
    Plain
}

[String]
public enum VuetifyInputType
{
    [Description("@#color")]
    Color,

    [Description("@#date")]
    Date,

    [Description("@#datetime-local")]
    DatetimeLocal,

    [Description("@#email")]
    Email,

    [Description("@#month")]
    Month,

    [Description("@#number")]
    Number,

    [Description("@#password")]
    Password,

    [Description("@#search")]
    Search,

    [Description("@#tel")]
    Tel,

    [Description("@#text")]
    Text,

    [Description("@#time")]
    Time,

    [Description("@#url")]
    Url,

    [Description("@#week")]
    Week
}

[String]
public enum VuetifyAutoSelectFirstMode
{
    [Description("@#exact")]
    Exact
}

[String]
public enum VuetifyNumberInputControlVariant
{
    [Description("@#default")]
    Default,

    [Description("@#stacked")]
    Stacked,

    [Description("@#split")]
    Split,

    [Description("@#hidden")]
    Hidden
}

public enum VuetifyFileSizeBase
{
    Decimal = 1000,
    Binary = 1024
}

[String]
public enum VuetifyAlwaysMode
{
    [Description("@#always")]
    Always
}

[String]
public enum VuetifySliderDirection
{
    [Description("@#horizontal")]
    Horizontal,

    [Description("@#vertical")]
    Vertical
}

[String]
public enum VuetifyBottomNavigationMode
{
    [Description("@#horizontal")]
    Horizontal,

    [Description("@#shift")]
    Shift
}

[String]
public enum VuetifyScrollStrategy
{
    [Description("@#block")]
    Block,

    [Description("@#close")]
    Close,

    [Description("@#none")]
    None,

    [Description("@#reposition")]
    Reposition
}

[String]
public enum VuetifyLocationStrategy
{
    [Description("@#static")]
    Static,

    [Description("@#connected")]
    Connected
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyStyleValuesCollectionBuilder), nameof(VuetifyStyleValuesCollectionBuilder.Create))]
public readonly struct VuetifyStyleValues : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyStyleValue>
{
    private readonly VuetifyStyleValue[]? _values;

    public VuetifyStyleValues(VuetifyStyleValue[] values)
    {
        _values = values;
    }

    public VuetifyStyleValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValues From(VuetifyStyleValue[] values);

    public static implicit operator VuetifyStyleValues(VuetifyStyleValue[] values)
        => new(values);

    public static implicit operator VuetifyStyleValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    public static implicit operator VuetifyStyleValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    public static implicit operator VuetifyStyleValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    IEnumerator<VuetifyStyleValue> IEnumerable<VuetifyStyleValue>.GetEnumerator()
        => ((IEnumerable<VuetifyStyleValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyStyleValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyStyleValuesCollectionBuilder
{
    public static VuetifyStyleValues Create(ReadOnlySpan<VuetifyStyleValue> values)
        => values.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyStyleValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly VueProps? _props;
    private readonly VuetifyStyleValues? _values;

    public VuetifyStyleValue(string value)
    {
        _kind = 1;
        _string = value;
        _props = default;
        _values = default;
    }

    public VuetifyStyleValue(VueProps value)
    {
        _kind = 2;
        _string = default;
        _props = value;
        _values = default;
    }

    public VuetifyStyleValue(VuetifyStyleValues value)
    {
        _kind = 3;
        _string = default;
        _props = default;
        _values = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public VueProps? AsProps => _kind == 2 ? _props : default;

    public VuetifyStyleValues? AsValues => _kind == 3 ? _values : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsProps,
        3 => AsValues,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValue From(VueProps value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValue From(VuetifyStyleValues value);

    public static implicit operator VuetifyStyleValue(string value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VueProps value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VuetifyStyleValues value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VuetifyStyleValue[] value)
        => new((VuetifyStyleValues)value);

    public static implicit operator VuetifyStyleValue(string[] value)
        => new((VuetifyStyleValues)value);

    public static implicit operator VuetifyStyleValue(VueProps[] value)
        => new((VuetifyStyleValues)value);

    public static implicit operator VuetifyStyleValue(VueDictionary[] value)
        => new((VuetifyStyleValues)value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyAttachTarget : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _selector;
    private readonly Element? _element;

    public VuetifyAttachTarget(bool value)
    {
        _kind = 1;
        _bool = value;
        _selector = default;
        _element = default;
    }

    public VuetifyAttachTarget(string value)
    {
        _kind = 2;
        _bool = default;
        _selector = value;
        _element = default;
    }

    public VuetifyAttachTarget(Element value)
    {
        _kind = 3;
        _bool = default;
        _selector = default;
        _element = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsSelector => _kind == 2 ? _selector : default;

    public Element? AsElement => _kind == 3 ? _element : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsSelector,
        3 => AsElement,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAttachTarget From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAttachTarget From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAttachTarget From(Element value);

    public static implicit operator VuetifyAttachTarget(bool value)
        => new(value);

    public static implicit operator VuetifyAttachTarget(string value)
        => new(value);

    public static implicit operator VuetifyAttachTarget(Element value)
        => new(value);
}

[String]
public enum VuetifyLocation
{
    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom,

    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right,

    [Description("@#center")]
    Center,

    [Description("@#center center")]
    CenterCenter,

    [Description("@#top start")]
    TopStart,

    [Description("@#top center")]
    TopCenter,

    [Description("@#top end")]
    TopEnd,

    [Description("@#bottom start")]
    BottomStart,

    [Description("@#bottom center")]
    BottomCenter,

    [Description("@#bottom end")]
    BottomEnd,

    [Description("@#start top")]
    StartTop,

    [Description("@#start center")]
    StartCenter,

    [Description("@#start bottom")]
    StartBottom,

    [Description("@#end top")]
    EndTop,

    [Description("@#end center")]
    EndCenter,

    [Description("@#end bottom")]
    EndBottom,

    [Description("@#left top")]
    LeftTop,

    [Description("@#left center")]
    LeftCenter,

    [Description("@#left bottom")]
    LeftBottom,

    [Description("@#right top")]
    RightTop,

    [Description("@#right center")]
    RightCenter,

    [Description("@#right bottom")]
    RightBottom
}

[String]
public enum VuetifyAppBarLocation
{
    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom
}

[String]
public enum VuetifyListLineMode
{
    [Description("@#one")]
    One,

    [Description("@#two")]
    Two,

    [Description("@#three")]
    Three
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyListLines : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyListLineMode? _mode;

    public VuetifyListLines(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    public VuetifyListLines(VuetifyListLineMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyListLineMode? AsMode => _kind == 2 ? _mode : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsMode,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyListLines From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyListLines From(VuetifyListLineMode value);

    public static implicit operator VuetifyListLines(bool value)
        => new(value);

    public static implicit operator VuetifyListLines(VuetifyListLineMode value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyRippleValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VueProps? _options;

    public VuetifyRippleValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _options = default;
    }

    public VuetifyRippleValue(VueProps value)
    {
        _kind = 2;
        _bool = default;
        _options = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VueProps? AsOptions => _kind == 2 ? _options : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsOptions,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyRippleValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyRippleValue From(VueProps value);

    public static implicit operator VuetifyRippleValue(bool value)
        => new(value);

    public static implicit operator VuetifyRippleValue(VueProps value)
        => new(value);
}

[String]
public enum VuetifyNavigationDrawerLocation
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right,

    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyScrimValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;

    public VuetifyScrimValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
    }

    public VuetifyScrimValue(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyScrimValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyScrimValue From(string value);

    public static implicit operator VuetifyScrimValue(bool value)
        => new(value);

    public static implicit operator VuetifyScrimValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyTransitionValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;
    private readonly VueTransitionProps? _props;

    public VuetifyTransitionValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
        _props = default;
    }

    public VuetifyTransitionValue(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
        _props = default;
    }

    public VuetifyTransitionValue(VueTransitionProps value)
    {
        _kind = 3;
        _bool = default;
        _string = default;
        _props = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

    public VueTransitionProps? AsProps => _kind == 3 ? _props : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsString,
        3 => AsProps,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTransitionValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTransitionValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTransitionValue From(VueTransitionProps value);

    public static implicit operator VuetifyTransitionValue(bool value)
        => new(value);

    public static implicit operator VuetifyTransitionValue(string value)
        => new(value);

    public static implicit operator VuetifyTransitionValue(VueTransitionProps value)
        => new(value);
}

[String]
public enum VuetifyHideDetailsMode
{
    [Description("@#auto")]
    Auto
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyHideDetailsValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyHideDetailsMode? _mode;

    public VuetifyHideDetailsValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    public VuetifyHideDetailsValue(VuetifyHideDetailsMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyHideDetailsMode? AsMode => _kind == 2 ? _mode : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsMode,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyHideDetailsValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyHideDetailsValue From(VuetifyHideDetailsMode value);

    public static implicit operator VuetifyHideDetailsValue(bool value)
        => new(value);

    public static implicit operator VuetifyHideDetailsValue(VuetifyHideDetailsMode value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyMessagesValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _strings;

    public VuetifyMessagesValue(string value)
    {
        _kind = 1;
        _string = value;
        _strings = default;
    }

    public VuetifyMessagesValue(string[] value)
    {
        _kind = 2;
        _string = default;
        _strings = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public string[]? AsStrings => _kind == 2 ? _strings : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsStrings,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyMessagesValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyMessagesValue From(string[] value);

    public static implicit operator VuetifyMessagesValue(string value)
        => new(value);

    public static implicit operator VuetifyMessagesValue(string[] value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyAutoSelectFirstValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyAutoSelectFirstMode? _mode;

    public VuetifyAutoSelectFirstValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    public VuetifyAutoSelectFirstValue(VuetifyAutoSelectFirstMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyAutoSelectFirstMode? AsMode => _kind == 2 ? _mode : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsMode,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAutoSelectFirstValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyAutoSelectFirstValue From(VuetifyAutoSelectFirstMode value);

    public static implicit operator VuetifyAutoSelectFirstValue(bool value)
        => new(value);

    public static implicit operator VuetifyAutoSelectFirstValue(VuetifyAutoSelectFirstMode value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyFileShowSizeValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyFileSizeBase? _base;

    public VuetifyFileShowSizeValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _base = default;
    }

    public VuetifyFileShowSizeValue(VuetifyFileSizeBase value)
    {
        _kind = 2;
        _bool = default;
        _base = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyFileSizeBase? AsBase => _kind == 2 ? _base : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsBase,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFileShowSizeValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFileShowSizeValue From(VuetifyFileSizeBase value);

    public static implicit operator VuetifyFileShowSizeValue(bool value)
        => new(value);

    public static implicit operator VuetifyFileShowSizeValue(VuetifyFileSizeBase value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyBooleanAlwaysValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyAlwaysMode? _mode;

    public VuetifyBooleanAlwaysValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    public VuetifyBooleanAlwaysValue(VuetifyAlwaysMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyAlwaysMode? AsMode => _kind == 2 ? _mode : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsMode,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBooleanAlwaysValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBooleanAlwaysValue From(VuetifyAlwaysMode value);

    public static implicit operator VuetifyBooleanAlwaysValue(bool value)
        => new(value);

    public static implicit operator VuetifyBooleanAlwaysValue(VuetifyAlwaysMode value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyBooleanStringValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;

    public VuetifyBooleanStringValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
    }

    public VuetifyBooleanStringValue(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBooleanStringValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBooleanStringValue From(string value);

    public static implicit operator VuetifyBooleanStringValue(bool value)
        => new(value);

    public static implicit operator VuetifyBooleanStringValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCounterValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    public VuetifyCounterValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    public VuetifyCounterValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    public VuetifyCounterValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCounterValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCounterValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCounterValue From(string value);

    public static implicit operator VuetifyCounterValue(bool value)
        => new(value);

    public static implicit operator VuetifyCounterValue(Number value)
        => new(value);

    public static implicit operator VuetifyCounterValue(string value)
        => new(value);

    public static implicit operator VuetifyCounterValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyTextValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly bool? _bool;

    public VuetifyTextValue(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _bool = default;
    }

    public VuetifyTextValue(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _bool = default;
    }

    public VuetifyTextValue(bool value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _bool = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public bool? AsBool => _kind == 3 ? _bool : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsNumber,
        3 => AsBool,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTextValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTextValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTextValue From(bool value);

    public static implicit operator VuetifyTextValue(string value)
        => new(value);

    public static implicit operator VuetifyTextValue(Number value)
        => new(value);

    public static implicit operator VuetifyTextValue(bool value)
        => new(value);

    public static implicit operator VuetifyTextValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyTextValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyRoundedValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    public VuetifyRoundedValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    public VuetifyRoundedValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    public VuetifyRoundedValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyRoundedValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyRoundedValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyRoundedValue From(string value);

    public static implicit operator VuetifyRoundedValue(bool value)
        => new(value);

    public static implicit operator VuetifyRoundedValue(Number value)
        => new(value);

    public static implicit operator VuetifyRoundedValue(string value)
        => new(value);

    public static implicit operator VuetifyRoundedValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyRoundedValue(decimal value)
        => new((Number)value);
}

[String]
public enum VuetifyProgressCircularIndeterminateMode
{
    [Description("@#disable-shrink")]
    DisableShrink
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyProgressCircularIndeterminateValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyProgressCircularIndeterminateMode? _mode;

    public VuetifyProgressCircularIndeterminateValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    public VuetifyProgressCircularIndeterminateValue(VuetifyProgressCircularIndeterminateMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyProgressCircularIndeterminateMode? AsMode => _kind == 2 ? _mode : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsMode,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyProgressCircularIndeterminateValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyProgressCircularIndeterminateValue From(VuetifyProgressCircularIndeterminateMode value);

    public static implicit operator VuetifyProgressCircularIndeterminateValue(bool value)
        => new(value);

    public static implicit operator VuetifyProgressCircularIndeterminateValue(VuetifyProgressCircularIndeterminateMode value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyFileModelValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly File? _file;
    private readonly File[]? _files;

    public VuetifyFileModelValue(File value)
    {
        _kind = 1;
        _file = value;
        _files = default;
    }

    public VuetifyFileModelValue(File[] value)
    {
        _kind = 2;
        _file = default;
        _files = value;
    }

    public File? AsFile => _kind == 1 ? _file : default;

    public File[]? AsFiles => _kind == 2 ? _files : default;

    public object? Value => _kind switch
    {
        1 => AsFile,
        2 => AsFiles,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFileModelValue From(File value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFileModelValue From(File[] value);

    public static implicit operator VuetifyFileModelValue(File value)
        => new(value);

    public static implicit operator VuetifyFileModelValue(File[] value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyRangeSliderModelValue : System.Runtime.CompilerServices.IUnion
{
    private readonly Number[]? _values;

    public VuetifyRangeSliderModelValue(Number[] values)
    {
        _values = values;
    }

    public Number[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyRangeSliderModelValue From(Number[] values);

    [ECMAScriptInline("[__arg1, __arg2]")]
    public extern static VuetifyRangeSliderModelValue From(Number start, Number end);

    public static implicit operator VuetifyRangeSliderModelValue(Number[] values)
        => new(values);

    public static implicit operator VuetifyRangeSliderModelValue(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    public static implicit operator VuetifyRangeSliderModelValue(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));
}

[String]
public enum VuetifyValidateOn
{
    [Description("@#input")]
    Input,

    [Description("@#blur")]
    Blur,

    [Description("@#submit")]
    Submit,

    [Description("@#invalid-input")]
    InvalidInput,

    [Description("@#lazy")]
    Lazy,

    [Description("@#eager")]
    Eager,

    [Description("@#input lazy")]
    InputLazy,

    [Description("@#input eager")]
    InputEager,

    [Description("@#blur lazy")]
    BlurLazy,

    [Description("@#blur eager")]
    BlurEager,

    [Description("@#submit lazy")]
    SubmitLazy,

    [Description("@#submit eager")]
    SubmitEager,

    [Description("@#invalid-input lazy")]
    InvalidInputLazy,

    [Description("@#invalid-input eager")]
    InvalidInputEager,

    [Description("@#lazy input")]
    LazyInput,

    [Description("@#eager input")]
    EagerInput,

    [Description("@#lazy blur")]
    LazyBlur,

    [Description("@#eager blur")]
    EagerBlur,

    [Description("@#lazy submit")]
    LazySubmit,

    [Description("@#eager submit")]
    EagerSubmit,

    [Description("@#lazy invalid-input")]
    LazyInvalidInput,

    [Description("@#eager invalid-input")]
    EagerInvalidInput
}

[String]
public enum VuetifyInputDirection
{
    [Description("@#horizontal")]
    Horizontal,

    [Description("@#vertical")]
    Vertical
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyNullableBoolean : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;

    public VuetifyNullableBoolean(bool value)
    {
        _kind = 1;
        _bool = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyNullableBoolean From(bool value);

    [ECMAScriptInline("null")]
    public extern static VuetifyNullableBoolean Null();

    public static implicit operator VuetifyNullableBoolean(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyIconColorValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;

    public VuetifyIconColorValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
    }

    public VuetifyIconColorValue(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIconColorValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIconColorValue From(string value);

    public static implicit operator VuetifyIconColorValue(bool value)
        => new(value);

    public static implicit operator VuetifyIconColorValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyValidationResult : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;

    public VuetifyValidationResult(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
    }

    public VuetifyValidationResult(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyValidationResult From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyValidationResult From(string value);

    public static implicit operator VuetifyValidationResult(bool value)
        => new(value);

    public static implicit operator VuetifyValidationResult(string value)
        => new(value);
}

public delegate VuetifyValidationResult VuetifyValidationRuleResolver(VueValue? value);

public delegate IPromise<VuetifyValidationResult> VuetifyAsyncValidationRuleResolver(VueValue? value);

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyValidationRule : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyValidationResult? _result;
    private readonly VuetifyValidationRuleResolver? _resolver;
    private readonly IPromise<VuetifyValidationResult>? _promise;
    private readonly VuetifyAsyncValidationRuleResolver? _asyncResolver;

    public VuetifyValidationRule(VuetifyValidationResult value)
    {
        _kind = 1;
        _result = value;
        _resolver = default;
        _promise = default;
        _asyncResolver = default;
    }

    public VuetifyValidationRule(VuetifyValidationRuleResolver value)
    {
        _kind = 2;
        _result = default;
        _resolver = value;
        _promise = default;
        _asyncResolver = default;
    }

    public VuetifyValidationRule(IPromise<VuetifyValidationResult> value)
    {
        _kind = 3;
        _result = default;
        _resolver = default;
        _promise = value;
        _asyncResolver = default;
    }

    public VuetifyValidationRule(VuetifyAsyncValidationRuleResolver value)
    {
        _kind = 4;
        _result = default;
        _resolver = default;
        _promise = default;
        _asyncResolver = value;
    }

    public VuetifyValidationResult? AsResult => _kind == 1 ? _result : default;

    public VuetifyValidationRuleResolver? AsResolver => _kind == 2 ? _resolver : default;

    public IPromise<VuetifyValidationResult>? AsPromise => _kind == 3 ? _promise : default;

    public VuetifyAsyncValidationRuleResolver? AsAsyncResolver => _kind == 4 ? _asyncResolver : default;

    public object? Value => _kind switch
    {
        1 => AsResult,
        2 => AsResolver,
        3 => AsPromise,
        4 => AsAsyncResolver,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyValidationRule From(VuetifyValidationResult value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyValidationRule From(VuetifyValidationRuleResolver value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyValidationRule From(IPromise<VuetifyValidationResult> value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyValidationRule From(VuetifyAsyncValidationRuleResolver value);

    public static implicit operator VuetifyValidationRule(VuetifyValidationResult value)
        => new(value);

    public static implicit operator VuetifyValidationRule(bool value)
        => new((VuetifyValidationResult)value);

    public static implicit operator VuetifyValidationRule(string value)
        => new((VuetifyValidationResult)value);

    public static implicit operator VuetifyValidationRule(VuetifyValidationRuleResolver value)
        => new(value);

    public static implicit operator VuetifyValidationRule(VuetifyAsyncValidationRuleResolver value)
        => new(value);
}

[String]
public enum VuetifyPosition
{
    [Description("@#static")]
    Static,

    [Description("@#relative")]
    Relative,

    [Description("@#fixed")]
    Fixed,

    [Description("@#absolute")]
    Absolute,

    [Description("@#sticky")]
    Sticky
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyMobileValue : System.Runtime.CompilerServices.IUnion
{
    private readonly bool? _value;

    public VuetifyMobileValue(bool value)
    {
        _value = value;
    }

    public bool? AsBool => _value;

    public object? Value => AsBool;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyMobileValue From(bool value);

    [ECMAScriptInline("null")]
    public extern static VuetifyMobileValue Auto();

    public static implicit operator VuetifyMobileValue(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyBorderValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    public VuetifyBorderValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    public VuetifyBorderValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    public VuetifyBorderValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBorderValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBorderValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBorderValue From(string value);

    public static implicit operator VuetifyBorderValue(bool value)
        => new(value);

    public static implicit operator VuetifyBorderValue(Number value)
        => new(value);

    public static implicit operator VuetifyBorderValue(string value)
        => new(value);

    public static implicit operator VuetifyBorderValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyBorderValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyIconValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;
    private readonly Symbol? _symbol;
    private readonly VueProps? _component;

    public VuetifyIconValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
        _symbol = default;
        _component = default;
    }

    public VuetifyIconValue(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
        _symbol = default;
        _component = default;
    }

    public VuetifyIconValue(Symbol value)
    {
        _kind = 3;
        _bool = default;
        _string = default;
        _symbol = value;
        _component = default;
    }

    public VuetifyIconValue(VueProps value)
    {
        _kind = 4;
        _bool = default;
        _string = default;
        _symbol = default;
        _component = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

    public Symbol? AsSymbol => _kind == 3 ? _symbol : default;

    public VueProps? AsComponent => _kind == 4 ? _component : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsString,
        3 => AsSymbol,
        4 => AsComponent,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIconValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIconValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIconValue From(Symbol value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIconValue From(VueProps value);

    public static implicit operator VuetifyIconValue(bool value)
        => new(value);

    public static implicit operator VuetifyIconValue(string value)
        => new(value);

    public static implicit operator VuetifyIconValue(Symbol value)
        => new(value);

    public static implicit operator VuetifyIconValue(VueProps value)
        => new(value);

    public static implicit operator VuetifyIconValue(VueDictionary value)
        => new(value);
}

public delegate Number VuetifyCounterValueResolver(string? value);

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCounterValueSource : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Number? _number;
    private readonly VuetifyCounterValueResolver? _resolver;

    public VuetifyCounterValueSource(Number value)
    {
        _kind = 1;
        _number = value;
        _resolver = default;
    }

    public VuetifyCounterValueSource(VuetifyCounterValueResolver value)
    {
        _kind = 2;
        _number = default;
        _resolver = value;
    }

    public Number? AsNumber => _kind == 1 ? _number : default;

    public VuetifyCounterValueResolver? AsResolver => _kind == 2 ? _resolver : default;

    public object? Value => _kind switch
    {
        1 => AsNumber,
        2 => AsResolver,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCounterValueSource From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCounterValueSource From(VuetifyCounterValueResolver value);

    public static implicit operator VuetifyCounterValueSource(Number value)
        => new(value);

    public static implicit operator VuetifyCounterValueSource(VuetifyCounterValueResolver value)
        => new(value);

    public static implicit operator VuetifyCounterValueSource(byte value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(short value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(int value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(uint value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(float value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(double value)
        => new((Number)value);

    public static implicit operator VuetifyCounterValueSource(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
public sealed record VuetifyTextModelModifiers : VueProps
{
    [Description("@#trim")]
    public bool? Trim { get; init; }

    [Description("@#number")]
    public bool? Number { get; init; }

    [Description("@#lazy")]
    public bool? Lazy { get; init; }
}
