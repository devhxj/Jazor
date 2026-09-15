namespace ECMAScript.Vuetify;

/// <summary>
/// 图片源对象，包含多分辨率源和宽高比信息。
/// Image source object containing multi-resolution sources and aspect ratio.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VImgSourceObject : VueProps
{
    /// <summary>
    /// 主图片的 URL。
    /// </summary>
    [Description("@#src")]
    public string? Src { get; init; }

    /// <summary>
    /// 供浏览器根据像素密度或显示宽度选择资源的 srcset 候选列表。
    /// </summary>
    [Description("@#srcset")]
    public string? Srcset { get; init; }

    /// <summary>
    /// 主图片加载完成前显示的占位图片 URL。
    /// </summary>
    [Description("@#lazySrc")]
    public string? LazySrc { get; init; }

    /// <summary>
    /// 图片的宽高比，等于宽度除以高度。
    /// </summary>
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
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 VImgSourceObject 分支；不属于该分支时返回 null。
    /// </summary>
    public VImgSourceObject? AsObject => Value as VImgSourceObject;
}

/// <summary>
/// 图片拖拽枚举值。
/// Image draggable enum values.
/// </summary>
[String]
public enum VImgDraggable
{
    /// <summary>
    /// 允许浏览器拖拽图像；上游取值为 “true”。
    /// </summary>
    [Description("@#true")]
    True,

    /// <summary>
    /// 禁止浏览器拖拽图像；上游取值为 “false”。
    /// </summary>
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
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VImgDraggable 分支；不属于该分支时返回 null。
    /// </summary>
    public VImgDraggable? AsMode => Value is VImgDraggable value ? value : default(VImgDraggable?);
}

/// <summary>
/// 图片跨域策略枚举。
/// Image cross-origin policy enumeration.
/// </summary>
[String]
public enum VImgCrossOrigin
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 匿名跨源资源请求，同源情况下保留凭据；上游取值为 “anonymous”。
    /// </summary>
    [Description("@#anonymous")]
    Anonymous,

    /// <summary>
    /// 跨源资源请求携带凭据；上游取值为 “use-credentials”。
    /// </summary>
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
    /// <summary>
    /// 不发送 Referer；上游取值为 “no-referrer”。
    /// </summary>
    [Description("@#no-referrer")]
    NoReferrer,

    /// <summary>
    /// 安全协议降级时不发送 Referer，否则发送完整来源；上游取值为 “no-referrer-when-downgrade”。
    /// </summary>
    [Description("@#no-referrer-when-downgrade")]
    NoReferrerWhenDowngrade,

    /// <summary>
    /// 仅发送来源的协议、主机和端口；上游取值为 “origin”。
    /// </summary>
    [Description("@#origin")]
    Origin,

    /// <summary>
    /// 同源发送完整来源，跨源仅发送源；上游取值为 “origin-when-cross-origin”。
    /// </summary>
    [Description("@#origin-when-cross-origin")]
    OriginWhenCrossOrigin,

    /// <summary>
    /// 只对同源请求发送来源；上游取值为 “same-origin”。
    /// </summary>
    [Description("@#same-origin")]
    SameOrigin,

    /// <summary>
    /// 仅发送源且安全协议降级时不发送；上游取值为 “strict-origin”。
    /// </summary>
    [Description("@#strict-origin")]
    StrictOrigin,

    /// <summary>
    /// 同源发送完整来源，跨源仅发送源，安全降级时不发送；上游取值为 “strict-origin-when-cross-origin”。
    /// </summary>
    [Description("@#strict-origin-when-cross-origin")]
    StrictOriginWhenCrossOrigin,

    /// <summary>
    /// 发送来源 URL 的源、路径和查询参数；上游取值为 “unsafe-url”。
    /// </summary>
    [Description("@#unsafe-url")]
    UnsafeUrl
}