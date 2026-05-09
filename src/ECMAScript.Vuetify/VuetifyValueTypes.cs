using System.ComponentModel;

namespace ECMAScript.Vuetify;

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
public enum VuetifyHideDetailsMode
{
    [Description("@#auto")]
    Auto
}

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

    public static implicit operator VuetifyHideDetailsValue(bool value)
        => new(value);

    public static implicit operator VuetifyHideDetailsValue(VuetifyHideDetailsMode value)
        => new(value);
}

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

    public static implicit operator VuetifyMessagesValue(string value)
        => new(value);

    public static implicit operator VuetifyMessagesValue(string[] value)
        => new(value);
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
