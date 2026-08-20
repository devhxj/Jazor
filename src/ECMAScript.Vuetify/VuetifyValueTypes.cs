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
[Description("@#")]
public readonly union VuetifyAlertBorderValue(bool, VuetifyAlertBorderSide)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VuetifyAlertBorderSide? AsSide
        => Value is VuetifyAlertBorderSide value ? value : default(VuetifyAlertBorderSide?);

    public static implicit operator VuetifyAlertBorderValue(bool value)
        => new(value);

    public static implicit operator VuetifyAlertBorderValue(VuetifyAlertBorderSide value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyAlertIconValue(
    string,
    Symbol,
    VueProps)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public Symbol? AsSymbol
        => Value is Symbol value ? value : default(Symbol?);

    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    [ECMAScriptInline("false")]
    public extern static VuetifyAlertIconValue None();

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
[Description("@#")]
[CollectionBuilder(typeof(VuetifyStyleValuesCollectionBuilder), nameof(VuetifyStyleValuesCollectionBuilder.Create))]
public readonly union VuetifyStyleValues(VuetifyStyleValue[]) : IEnumerable<VuetifyStyleValue>
{
    public VuetifyStyleValue[]? AsArray
        => Value is VuetifyStyleValue[] value ? value : default(VuetifyStyleValue[]?);

    public static implicit operator VuetifyStyleValues(VuetifyStyleValue[] values)
        => new(values);

    public static implicit operator VuetifyStyleValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    public static implicit operator VuetifyStyleValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    public static implicit operator VuetifyStyleValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    IEnumerator<VuetifyStyleValue> IEnumerable<VuetifyStyleValue>.GetEnumerator()
        => ((IEnumerable<VuetifyStyleValue>)(AsArray ?? [])).GetEnumerator();

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
[Description("@#")]
public readonly union VuetifyStyleValue(
    string,
    VueProps,
    VuetifyStyleValues)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    public VuetifyStyleValues? AsValues
        => Value is VuetifyStyleValues value ? value : default(VuetifyStyleValues?);

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
[Description("@#")]
public readonly union VuetifyAttachTarget(
    bool,
    string,
    Element)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public string? AsSelector
        => Value is string value ? value : default(string?);

    public Element? AsElement
        => Value is Element value ? value : default(Element?);

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
[Description("@#")]
public readonly union VuetifyListLines(bool, VuetifyListLineMode)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VuetifyListLineMode? AsMode
        => Value is VuetifyListLineMode value ? value : default(VuetifyListLineMode?);

    public static implicit operator VuetifyListLines(bool value)
        => new(value);

    public static implicit operator VuetifyListLines(VuetifyListLineMode value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyRippleValue(bool, VueProps)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VueProps? AsOptions
        => Value is VueProps value ? value : default(VueProps?);

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
[Description("@#")]
public readonly union VuetifyScrimValue(bool, string)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public static implicit operator VuetifyScrimValue(bool value)
        => new(value);

    public static implicit operator VuetifyScrimValue(string value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyTransitionValue(
    bool,
    string,
    VueTransitionProps)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public VueTransitionProps? AsProps
        => Value is VueTransitionProps value ? value : default(VueTransitionProps?);

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
[Description("@#")]
public readonly union VuetifyHideDetailsValue(bool, VuetifyHideDetailsMode)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VuetifyHideDetailsMode? AsMode
        => Value is VuetifyHideDetailsMode value ? value : default(VuetifyHideDetailsMode?);

    public static implicit operator VuetifyHideDetailsValue(bool value)
        => new(value);

    public static implicit operator VuetifyHideDetailsValue(VuetifyHideDetailsMode value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyMessagesValue(string, string[])
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public string[]? AsStrings
        => Value is string[] value ? value : default(string[]?);

    public static implicit operator VuetifyMessagesValue(string value)
        => new(value);

    public static implicit operator VuetifyMessagesValue(string[] value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyAutoSelectFirstValue(bool, VuetifyAutoSelectFirstMode)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VuetifyAutoSelectFirstMode? AsMode
        => Value is VuetifyAutoSelectFirstMode value ? value : default(VuetifyAutoSelectFirstMode?);

    public static implicit operator VuetifyAutoSelectFirstValue(bool value)
        => new(value);

    public static implicit operator VuetifyAutoSelectFirstValue(VuetifyAutoSelectFirstMode value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyFileShowSizeValue(bool, VuetifyFileSizeBase)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VuetifyFileSizeBase? AsBase
        => Value is VuetifyFileSizeBase value ? value : default(VuetifyFileSizeBase?);

    public static implicit operator VuetifyFileShowSizeValue(bool value)
        => new(value);

    public static implicit operator VuetifyFileShowSizeValue(VuetifyFileSizeBase value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyBooleanAlwaysValue(bool, VuetifyAlwaysMode)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VuetifyAlwaysMode? AsMode
        => Value is VuetifyAlwaysMode value ? value : default(VuetifyAlwaysMode?);

    public static implicit operator VuetifyBooleanAlwaysValue(bool value)
        => new(value);

    public static implicit operator VuetifyBooleanAlwaysValue(VuetifyAlwaysMode value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyBooleanStringValue(bool, string)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public static implicit operator VuetifyBooleanStringValue(bool value)
        => new(value);

    public static implicit operator VuetifyBooleanStringValue(string value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyCounterValue(
    bool,
    Number,
    string)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public string? AsString
        => Value is string value ? value : default(string?);

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
[Description("@#")]
public readonly union VuetifyTextValue(
    string,
    Number,
    bool)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public bool? AsBool
        => Value is bool value ? value : default(bool?);

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
[Description("@#")]
public readonly union VuetifyRoundedValue(
    bool,
    Number,
    string)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public string? AsString
        => Value is string value ? value : default(string?);

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
[Description("@#")]
public readonly union VuetifyProgressCircularIndeterminateValue(bool, VuetifyProgressCircularIndeterminateMode)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VuetifyProgressCircularIndeterminateMode? AsMode
        => Value is VuetifyProgressCircularIndeterminateMode value ? value : default(VuetifyProgressCircularIndeterminateMode?);

    public static implicit operator VuetifyProgressCircularIndeterminateValue(bool value)
        => new(value);

    public static implicit operator VuetifyProgressCircularIndeterminateValue(VuetifyProgressCircularIndeterminateMode value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyFileModelValue(JazorFile, JazorFile[])
{
    public JazorFile? AsFile
        => Value is JazorFile value ? value : default(JazorFile?);

    public JazorFile[]? AsFiles
        => Value is JazorFile[] value ? value : default(JazorFile[]?);

    public static implicit operator VuetifyFileModelValue(JazorFile value)
        => new(value);

    public static implicit operator VuetifyFileModelValue(JazorFile[] value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyRangeSliderModelValue(Number[], string[])
{
    public Number[]? AsArray
        => Value is Number[] value ? value : default(Number[]?);

    public string[]? AsStrings
        => Value as string[];

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
[Description("@#")]
public readonly union VuetifyNullableBoolean(bool)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    [ECMAScriptInline("null")]
    public extern static VuetifyNullableBoolean Null();

    public static implicit operator VuetifyNullableBoolean(bool value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyIconColorValue(bool, string)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public static implicit operator VuetifyIconColorValue(bool value)
        => new(value);

    public static implicit operator VuetifyIconColorValue(string value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyValidationResult(bool, string)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public static implicit operator VuetifyValidationResult(bool value)
        => new(value);

    public static implicit operator VuetifyValidationResult(string value)
        => new(value);
}


public delegate VuetifyValidationResult VuetifyValidationRuleResolver(VueValue? value);

public delegate IPromise<VuetifyValidationResult> VuetifyAsyncValidationRuleResolver(VueValue? value);

[ECMAScript]
[Description("@#")]
public readonly union VuetifyValidationRule(
    VuetifyValidationResult,
    VuetifyValidationRuleResolver,
    IPromise<VuetifyValidationResult>,
    VuetifyAsyncValidationRuleResolver)
{
    public VuetifyValidationResult? AsResult
        => Value is VuetifyValidationResult value ? value : default(VuetifyValidationResult?);

    public VuetifyValidationRuleResolver? AsResolver
        => Value is VuetifyValidationRuleResolver value ? value : default(VuetifyValidationRuleResolver?);

    public IPromise<VuetifyValidationResult>? AsPromise
        => Value is IPromise<VuetifyValidationResult> value ? value : default(IPromise<VuetifyValidationResult>?);

    public VuetifyAsyncValidationRuleResolver? AsAsyncResolver
        => Value is VuetifyAsyncValidationRuleResolver value ? value : default(VuetifyAsyncValidationRuleResolver?);

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
[Description("@#")]
public readonly union VuetifyMobileValue(bool)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    [ECMAScriptInline("null")]
    public extern static VuetifyMobileValue Auto();

    public static implicit operator VuetifyMobileValue(bool value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyBorderValue(
    bool,
    Number,
    string)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public string? AsString
        => Value is string value ? value : default(string?);

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
[Description("@#")]
public readonly union VuetifyIconValue(
    bool,
    string,
    Symbol,
    VueProps)
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public Symbol? AsSymbol
        => Value is Symbol value ? value : default(Symbol?);

    public VueProps? AsComponent
        => Value is VueProps value ? value : default(VueProps?);

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
[Description("@#")]
public readonly union VuetifyCounterValueSource(Number, VuetifyCounterValueResolver)
{
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public VuetifyCounterValueResolver? AsResolver
        => Value is VuetifyCounterValueResolver value ? value : default(VuetifyCounterValueResolver?);

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
