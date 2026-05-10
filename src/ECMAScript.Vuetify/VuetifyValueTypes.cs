using System.ComponentModel;

namespace ECMAScript.Vuetify;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyListLines
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyListLineMode? _mode;

    private VuetifyListLines(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    private VuetifyListLines(VuetifyListLineMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyListLineMode? AsMode => _kind == 2 ? _mode : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyRippleValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VueProps? _options;

    private VuetifyRippleValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _options = default;
    }

    private VuetifyRippleValue(VueProps value)
    {
        _kind = 2;
        _bool = default;
        _options = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VueProps? AsOptions => _kind == 2 ? _options : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyScrimValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;

    private VuetifyScrimValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
    }

    private VuetifyScrimValue(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyScrimValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyScrimValue From(string value);

    public static implicit operator VuetifyScrimValue(bool value)
        => new(value);

    public static implicit operator VuetifyScrimValue(string value)
        => new(value);
}

[String]
public enum VuetifyHideDetailsMode
{
    [Description("@#auto")]
    Auto
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyHideDetailsValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyHideDetailsMode? _mode;

    private VuetifyHideDetailsValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    private VuetifyHideDetailsValue(VuetifyHideDetailsMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyHideDetailsMode? AsMode => _kind == 2 ? _mode : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyMessagesValue
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _strings;

    private VuetifyMessagesValue(string value)
    {
        _kind = 1;
        _string = value;
        _strings = default;
    }

    private VuetifyMessagesValue(string[] value)
    {
        _kind = 2;
        _string = default;
        _strings = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public string[]? AsStrings => _kind == 2 ? _strings : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyAutoSelectFirstValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyAutoSelectFirstMode? _mode;

    private VuetifyAutoSelectFirstValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    private VuetifyAutoSelectFirstValue(VuetifyAutoSelectFirstMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyAutoSelectFirstMode? AsMode => _kind == 2 ? _mode : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyFileShowSizeValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyFileSizeBase? _base;

    private VuetifyFileShowSizeValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _base = default;
    }

    private VuetifyFileShowSizeValue(VuetifyFileSizeBase value)
    {
        _kind = 2;
        _bool = default;
        _base = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyFileSizeBase? AsBase => _kind == 2 ? _base : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyBooleanAlwaysValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyAlwaysMode? _mode;

    private VuetifyBooleanAlwaysValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    private VuetifyBooleanAlwaysValue(VuetifyAlwaysMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyAlwaysMode? AsMode => _kind == 2 ? _mode : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyBooleanStringValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string? _string;

    private VuetifyBooleanStringValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _string = default;
    }

    private VuetifyBooleanStringValue(string value)
    {
        _kind = 2;
        _bool = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string? AsString => _kind == 2 ? _string : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyCounterValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    private VuetifyCounterValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    private VuetifyCounterValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    private VuetifyCounterValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyTextValue
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly bool? _bool;

    private VuetifyTextValue(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _bool = default;
    }

    private VuetifyTextValue(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _bool = default;
    }

    private VuetifyTextValue(bool value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _bool = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public bool? AsBool => _kind == 3 ? _bool : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyRoundedValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    private VuetifyRoundedValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    private VuetifyRoundedValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    private VuetifyRoundedValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyProgressCircularIndeterminateValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyProgressCircularIndeterminateMode? _mode;

    private VuetifyProgressCircularIndeterminateValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    private VuetifyProgressCircularIndeterminateValue(VuetifyProgressCircularIndeterminateMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyProgressCircularIndeterminateMode? AsMode => _kind == 2 ? _mode : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyFileModelValue
{
    private readonly byte _kind;
    private readonly File? _file;
    private readonly File[]? _files;

    private VuetifyFileModelValue(File value)
    {
        _kind = 1;
        _file = value;
        _files = default;
    }

    private VuetifyFileModelValue(File[] value)
    {
        _kind = 2;
        _file = default;
        _files = value;
    }

    public File? AsFile => _kind == 1 ? _file : default;

    public File[]? AsFiles => _kind == 2 ? _files : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyRangeSliderModelValue
{
    private readonly Number[]? _values;

    private VuetifyRangeSliderModelValue(Number[] values)
    {
        _values = values;
    }

    public Number[]? AsArray => _values;

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
