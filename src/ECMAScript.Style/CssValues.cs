namespace ECMAScript.Style;

[ECMAScript]
[Description("@#")]
public sealed class CssRaw
{
    internal CssRaw() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssRaw create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssVariable
{
    internal CssVariable() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssVariable create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssLength
{
    internal CssLength() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssLength create(string value);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLength operator +(CssLength left, CssLength right);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLength left, CssPercentage right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLength operator -(CssLength left, CssLength right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLength left, CssPercentage right);

    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLength operator *(CssLength value, double factor);

    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLength operator *(double factor, CssLength value);

    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssLength operator /(CssLength value, double divisor);

    [ECMAScriptInline("`calc(-1 * ${__arg1})`")]
    public static extern CssLength operator -(CssLength value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssPercentage
{
    internal CssPercentage() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssPercentage create(string value);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssPercentage operator +(CssPercentage left, CssPercentage right);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssPercentage left, CssLength right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssPercentage operator -(CssPercentage left, CssPercentage right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssPercentage left, CssLength right);

    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssPercentage operator *(CssPercentage value, double factor);

    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssPercentage operator /(CssPercentage value, double divisor);
}

[ECMAScript]
[Description("@#")]
public sealed class CssLengthPercentage
{
    internal CssLengthPercentage() { }

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLengthPercentage left, CssLengthPercentage right);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLengthPercentage left, CssLength right);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLength left, CssLengthPercentage right);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLengthPercentage left, CssPercentage right);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssPercentage left, CssLengthPercentage right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage left, CssLengthPercentage right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage left, CssLength right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLength left, CssLengthPercentage right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage left, CssPercentage right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssPercentage left, CssLengthPercentage right);

    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLengthPercentage operator *(CssLengthPercentage value, double factor);

    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLengthPercentage operator *(double factor, CssLengthPercentage value);

    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssLengthPercentage operator /(CssLengthPercentage value, double divisor);

    [ECMAScriptInline("`calc(-1 * ${__arg1})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssAngle
{
    internal CssAngle() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssAngle create(string value);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssAngle operator +(CssAngle left, CssAngle right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssAngle operator -(CssAngle left, CssAngle right);

    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssAngle operator *(CssAngle value, double factor);

    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssAngle operator /(CssAngle value, double divisor);
}

[ECMAScript]
[Description("@#")]
public sealed class CssTime
{
    internal CssTime() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssTime create(string value);

    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssTime operator +(CssTime left, CssTime right);

    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssTime operator -(CssTime left, CssTime right);

    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssTime operator *(CssTime value, double factor);

    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssTime operator /(CssTime value, double divisor);
}

[ECMAScript]
[Description("@#")]
public sealed class CssFrequency
{
    internal CssFrequency() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssFrequency create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssResolution
{
    internal CssResolution() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssResolution create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssColor
{
    internal CssColor() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssColor create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssImage
{
    internal CssImage() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssImage create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssUrl
{
    internal CssUrl() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssUrl create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssString
{
    internal CssString() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssString create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssIdent
{
    internal CssIdent() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssIdent create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssKeyword
{
    internal CssKeyword() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssKeyword create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssTransform
{
    internal CssTransform() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssTransform create(string value);
}

/// <summary>
/// A serialized <c>box-shadow</c> list. The value stays opaque so it cannot be
/// accidentally assigned to an unrelated CSS domain.
/// 已序列化的 <c>box-shadow</c> 列表；保持不透明，避免误赋值到其他 CSS 属性域。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssShadowList
{
    internal CssShadowList() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssShadowList create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssTrack
{
    internal CssTrack() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssTrack create(string value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssRatio
{
    internal CssRatio() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssRatio create(string value);
}

[String]
public enum CssWideKeyword
{
    [Description("@#inherit")] Inherit,
    [Description("@#initial")] Initial,
    [Description("@#unset")] Unset,
    [Description("@#revert")] Revert,
    [Description("@#revert-layer")] RevertLayer
}

[String]
public enum CssAutoKeyword
{
    [Description("@#auto")] Auto
}

[String]
public enum CssNoneKeyword
{
    [Description("@#none")] None
}

[String]
public enum CssNormalKeyword
{
    [Description("@#normal")] Normal
}

[String]
public enum CssSizingKeyword
{
    [Description("@#min-content")] MinContent,
    [Description("@#max-content")] MaxContent
}

[String]
public enum CssDisplayKeyword
{
    [Description("@#block")] Block,
    [Description("@#inline")] Inline,
    [Description("@#inline-block")] InlineBlock,
    [Description("@#flex")] Flex,
    [Description("@#inline-flex")] InlineFlex,
    [Description("@#grid")] Grid,
    [Description("@#inline-grid")] InlineGrid,
    [Description("@#flow-root")] FlowRoot,
    [Description("@#contents")] Contents,
    [Description("@#table")] Table,
    [Description("@#list-item")] ListItem
}

[String]
public enum CssPositionKeyword
{
    [Description("@#static")] Static,
    [Description("@#relative")] Relative,
    [Description("@#absolute")] Absolute,
    [Description("@#fixed")] Fixed,
    [Description("@#sticky")] Sticky
}

[String]
public enum CssOverflowKeyword
{
    [Description("@#visible")] Visible,
    [Description("@#hidden")] Hidden,
    [Description("@#clip")] Clip,
    [Description("@#scroll")] Scroll
}

[String]
public enum CssLineWidthKeyword
{
    [Description("@#thin")] Thin,
    [Description("@#medium")] Medium,
    [Description("@#thick")] Thick
}

[String]
public enum CssLineStyleKeyword
{
    [Description("@#hidden")] Hidden,
    [Description("@#dotted")] Dotted,
    [Description("@#dashed")] Dashed,
    [Description("@#solid")] Solid,
    [Description("@#double")] Double,
    [Description("@#groove")] Groove,
    [Description("@#ridge")] Ridge,
    [Description("@#inset")] Inset,
    [Description("@#outset")] Outset
}

[String]
public enum CssColorKeyword
{
    [Description("@#transparent")] Transparent,
    [Description("@#currentColor")] CurrentColor
}

[ECMAScript]
[Description("@#")]
public readonly union CssValue(
    double,
    CssRaw,
    CssVariable,
    CssLength,
    CssPercentage,
    CssLengthPercentage,
    CssAngle,
    CssTime,
    CssFrequency,
    CssResolution,
    CssColor,
    CssImage,
    CssUrl,
    CssString,
    CssIdent,
    CssKeyword,
    CssTransform,
    CssShadowList,
    CssTrack,
    CssRatio,
    CssWideKeyword,
    CssAutoKeyword,
    CssNoneKeyword,
    CssNormalKeyword,
    CssSizingKeyword,
    CssDisplayKeyword,
    CssPositionKeyword,
    CssOverflowKeyword,
    CssLineWidthKeyword,
    CssLineStyleKeyword,
    CssColorKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssKeywordValue(
    CssRaw, CssVariable, CssIdent, CssKeyword, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssSizingKeyword, CssDisplayKeyword, CssPositionKeyword, CssOverflowKeyword, CssLineWidthKeyword,
    CssLineStyleKeyword, CssColorKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssLengthValue(
    CssRaw, CssVariable, CssLength, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssSizingKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssLengthPercentageValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssSizingKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssLengthPercentageNumberValue(
    double, CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssSizingKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssPercentageValue(
    CssRaw, CssVariable, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssNumberValue(
    double, CssRaw, CssVariable, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssIntegerValue(
    int, CssRaw, CssVariable, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssNumberPercentageValue(
    double, CssRaw, CssVariable, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssAngleValue(
    CssRaw, CssVariable, CssAngle, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssTimeValue(
    CssRaw, CssVariable, CssTime, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssFrequencyValue(
    CssRaw, CssVariable, CssFrequency, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssResolutionValue(
    CssRaw, CssVariable, CssResolution, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssColorValue(
    CssRaw, CssVariable, CssColor, CssColorKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssShadowLength(CssLength, CssVariable);

[ECMAScript]
[Description("@#")]
public readonly union CssShadowColor(CssColor, CssColorKeyword, CssVariable);

[ECMAScript]
[Description("@#")]
public readonly union CssBoxShadowValue(
    CssRaw, CssVariable, CssNoneKeyword, CssWideKeyword, CssShadowList);

[ECMAScript]
[Description("@#")]
public readonly union CssImageValue(
    CssRaw, CssVariable, CssImage, CssUrl, CssNoneKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssStringValue(
    CssRaw, CssVariable, CssString, CssNoneKeyword, CssNormalKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssTransformValue(
    CssRaw, CssVariable, CssTransform, CssNoneKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssLineWidthValue(
    CssRaw, CssVariable, CssLength, CssLineWidthKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssLineStyleValue(
    CssRaw, CssVariable, CssNoneKeyword, CssLineStyleKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssDisplayValue(
    CssRaw, CssVariable, CssNoneKeyword, CssDisplayKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssPositionValue(
    CssRaw, CssVariable, CssPositionKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssOverflowValue(
    CssRaw, CssVariable, CssAutoKeyword, CssOverflowKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssTrackValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssTrack, CssAutoKeyword, CssSizingKeyword, CssWideKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssRatioValue(
    CssRaw, CssVariable, CssRatio, CssAutoKeyword, CssWideKeyword);
