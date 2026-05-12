namespace ECMAScript.Vuetify;

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

[ECMAScript]
[Description("@#")]
public readonly union VImgSource(string, VImgSourceObject)
{
    public string? AsString => Value as string;

    public VImgSourceObject? AsObject => Value as VImgSourceObject;
}

[String]
public enum VImgDraggable
{
    [Description("@#true")]
    True,

    [Description("@#false")]
    False
}

[ECMAScript]
[Description("@#")]
public readonly union VImgDraggableValue(bool, VImgDraggable)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VImgDraggable? AsMode => Value is VImgDraggable value ? value : default(VImgDraggable?);
}

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
