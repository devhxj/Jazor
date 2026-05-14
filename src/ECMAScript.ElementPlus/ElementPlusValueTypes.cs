namespace ECMAScript.ElementPlus;

[String]
public enum ElementPlusComponentSize
{
    [Description("@#large")]
    Large,

    [Description("@#default")]
    Default,

    [Description("@#small")]
    Small
}

[String]
public enum ElementPlusPopperEffect
{
    [Description("@#dark")]
    Dark,

    [Description("@#light")]
    Light
}

[ECMAScript]
[Description("@#Styles")]
public sealed record ElementPlusStyles : VueDictionary<VueStringNumberValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusDirectiveValue(bool, VueProps)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VueProps? AsProps => Value as VueProps;

    public static implicit operator ElementPlusDirectiveValue(VueDictionary value) => (VueProps)value;
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusLoadingOptions : VueProps
{
    [Description("@#target")]
    public VueTeleportTarget? Target { get; init; }

    [Description("@#body")]
    public bool? Body { get; init; }

    [Description("@#lock")]
    public bool? Lock { get; init; }

    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#spinner")]
    public string? Spinner { get; init; }

    [Description("@#svg")]
    public string? Svg { get; init; }

    [Description("@#svgViewBox")]
    public string? SvgViewBox { get; init; }

    [Description("@#background")]
    public string? Background { get; init; }

    [Description("@#customClass")]
    public string? CustomClass { get; init; }

    [Description("@#fullscreen")]
    public bool? Fullscreen { get; init; }
}

[ECMAScript]
[Description("@#ButtonConfigContext")]
public sealed record ElementPlusButtonConfig : VueProps
{
    [Description("@#autoInsertSpace")]
    public bool? AutoInsertSpace { get; init; }

    [Description("@#type")]
    public string? Type { get; init; }

    [Description("@#plain")]
    public bool? Plain { get; init; }

    [Description("@#text")]
    public bool? Text { get; init; }

    [Description("@#round")]
    public bool? Round { get; init; }

    [Description("@#dashed")]
    public bool? Dashed { get; init; }
}

[ECMAScript]
[Description("@#CardConfigContext")]
public sealed record ElementPlusCardConfig : VueProps
{
    [Description("@#shadow")]
    public string? Shadow { get; init; }
}

[ECMAScript]
[Description("@#DialogConfigContext")]
public sealed record ElementPlusDialogConfig : VueProps
{
    [Description("@#draggable")]
    public bool? Draggable { get; init; }
}

[ECMAScript]
[Description("@#LinkConfigContext")]
public sealed record ElementPlusLinkConfig : VueProps
{
    [Description("@#underline")]
    public VueBooleanStringValue? Underline { get; init; }

    [Description("@#type")]
    public string? Type { get; init; }
}

[ECMAScript]
[Description("@#MessageConfigContext")]
public sealed record ElementPlusMessageConfig : VueProps
{
    [Description("@#max")]
    public Number? Max { get; init; }

    [Description("@#grouping")]
    public bool? Grouping { get; init; }

    [Description("@#duration")]
    public Number? Duration { get; init; }

    [Description("@#offset")]
    public Number? Offset { get; init; }
}

[ECMAScript]
[Description("@#TableConfigContext")]
public sealed record ElementPlusTableConfig : VueProps
{
    [Description("@#showOverflowTooltip")]
    public bool? ShowOverflowTooltip { get; init; }
}
