namespace ECMAScript.Vuetify;

/// <summary>
/// 图片源对象，包含多分辨率源和宽高比信息。
/// Image source object containing multi-resolution sources and aspect ratio.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VImgSourceObject : VueProps
{
    [Description("@#src")]
    public string? Src { get; init; }

    [Description("@#srcset")]
    public string? Srcset { get; init; }

    [Description("@#lazySrc")]
    public string? LazySrc { get; init; }

    [Description("@#aspect")]
    public required Number Aspect { get; init; }
}

/// <summary>
/// 图片源，可以是 URL 字符串或结构化源对象。
/// Image source, either a URL string or a structured source object.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VImgSource(string, VImgSourceObject)
{
    public string? AsString => Value as string;

    public VImgSourceObject? AsObject => Value as VImgSourceObject;
}

/// <summary>
/// 图片拖拽枚举值。
/// Image draggable enum values.
/// </summary>
[String]
public enum VImgDraggable
{
    [Description("@#true")]
    True,

    [Description("@#false")]
    False
}

/// <summary>
/// 图片拖拽值，支持布尔或枚举模式。
/// Image draggable value supporting boolean or enum mode.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VImgDraggableValue(bool, VImgDraggable)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VImgDraggable? AsMode => Value is VImgDraggable value ? value : default(VImgDraggable?);
}

/// <summary>
/// 图片跨域策略枚举。
/// Image cross-origin policy enumeration.
/// </summary>
[String]
public enum VImgCrossOrigin
{
    [Description("@#")]
    Empty,

    [Description("@#anonymous")]
    Anonymous,

    [Description("@#use-credentials")]
    UseCredentials
}

/// <summary>
/// 图片引用策略枚举。
/// Image referrer policy enumeration.
/// </summary>
[String]
public enum VImgReferrerPolicy
{
    [Description("@#no-referrer")]
    NoReferrer,

    [Description("@#no-referrer-when-downgrade")]
    NoReferrerWhenDowngrade,

    [Description("@#origin")]
    Origin,

    [Description("@#origin-when-cross-origin")]
    OriginWhenCrossOrigin,

    [Description("@#same-origin")]
    SameOrigin,

    [Description("@#strict-origin")]
    StrictOrigin,

    [Description("@#strict-origin-when-cross-origin")]
    StrictOriginWhenCrossOrigin,

    [Description("@#unsafe-url")]
    UnsafeUrl
}
