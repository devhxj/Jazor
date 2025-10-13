namespace ECMAScript.CSS;

/// <summary>
/// CSSRule
/// </summary>
[ECMAScript]
[Description("@#CSSRule")]
public partial class CSSRule
{
    /// <summary>
    /// SUPPORTS_RULE
    /// </summary>
    [Description("@#SUPPORTS_RULE")]
    public const ushort SUPPORTS_RULE = 12;

    /// <summary>
    /// cssText
    /// </summary>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

    /// <summary>
    /// parentRule
    /// </summary>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }

    /// <summary>
    /// parentStyleSheet
    /// </summary>
    [Description("@#parentStyleSheet")]
    public extern CSSStyleSheet? ParentStyleSheet { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern ushort Type { get; }

    /// <summary>
    /// STYLE_RULE
    /// </summary>
    [Description("@#STYLE_RULE")]
    public const ushort STYLE_RULE = 1;

    /// <summary>
    /// CHARSET_RULE
    /// </summary>
    [Description("@#CHARSET_RULE")]
    public const ushort CHARSET_RULE = 2;

    /// <summary>
    /// IMPORT_RULE
    /// </summary>
    [Description("@#IMPORT_RULE")]
    public const ushort IMPORT_RULE = 3;

    /// <summary>
    /// MEDIA_RULE
    /// </summary>
    [Description("@#MEDIA_RULE")]
    public const ushort MEDIA_RULE = 4;

    /// <summary>
    /// FONT_FACE_RULE
    /// </summary>
    [Description("@#FONT_FACE_RULE")]
    public const ushort FONT_FACE_RULE = 5;

    /// <summary>
    /// PAGE_RULE
    /// </summary>
    [Description("@#PAGE_RULE")]
    public const ushort PAGE_RULE = 6;

    /// <summary>
    /// MARGIN_RULE
    /// </summary>
    [Description("@#MARGIN_RULE")]
    public const ushort MARGIN_RULE = 9;

    /// <summary>
    /// NAMESPACE_RULE
    /// </summary>
    [Description("@#NAMESPACE_RULE")]
    public const ushort NAMESPACE_RULE = 10;
}

/// <summary>
/// CSSConditionRule
/// </summary>
[ECMAScript]
[Description("@#CSSConditionRule")]
public class CSSConditionRule : CSSGroupingRule
{
    /// <summary>
    /// conditionText
    /// </summary>
    [Description("@#conditionText")]
    public extern string ConditionText { get; }
}

/// <summary>
/// CSSMediaRule
/// </summary>
[ECMAScript]
[Description("@#CSSMediaRule")]
public class CSSMediaRule : CSSConditionRule
{
    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern MediaList Media { get; }

    /// <summary>
    /// matches
    /// </summary>
    [Description("@#matches")]
    public extern bool Matches { get; }
}

/// <summary>
/// CSSSupportsRule
/// </summary>
[ECMAScript]
[Description("@#CSSSupportsRule")]
public class CSSSupportsRule : CSSConditionRule
{
    /// <summary>
    /// matches
    /// </summary>
    [Description("@#matches")]
    public extern bool Matches { get; }
}