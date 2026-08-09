namespace ECMAScript.Style;

[ECMAScript]
[Description("@#")]
public sealed class CssRaw : ICssBorderPart
{
    internal CssRaw() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssRaw create(string value);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssRaw left, ICssBorderPart right);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssRaw left, CssColorKeyword right);
}

/// <summary>
/// A CSS value with declaration priority. The generic argument keeps the value domain exact while
/// the compiler erases the wrapper to <c>value!important</c> at the declaration site.
/// 带声明优先级的 CSS 值；泛型参数保持值域精确，编译输出在声明处擦除为 value!important。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssImportant<TValue>
{
    internal CssImportant() { }

    [ECMAScriptInline("__arg1 + \"!important\"")]
    internal static extern CssImportant<TValue> create(TValue value);
}

[ECMAScript]
[Description("@#")]
public sealed class CssVariable : ICssBorderPart
{
    internal CssVariable() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssVariable create(string value);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssVariable left, ICssBorderPart right);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssVariable left, CssColorKeyword right);
}

[ECMAScript]
[Description("@#")]
public sealed class CssLength : ICssBorderPart
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

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssLength left, ICssBorderPart right);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssLength left, CssColorKeyword right);
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

    [ECMAScriptInline("__arg1")]
    internal static extern CssLengthPercentage create(string value);

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
public sealed class CssColor : ICssBorderPart
{
    internal CssColor() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssColor create(string value);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssColor left, ICssBorderPart right);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssColor left, CssColorKeyword right);
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
/// A composed <c>filter</c> or <c>backdrop-filter</c> function list.
/// 已组合的 <c>filter</c> 或 <c>backdrop-filter</c> 函数列表。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssFilter
{
    internal CssFilter() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssFilter create(string value);
}

/// <summary>
/// A composed border shorthand. It is produced by <c>border(...)</c> or by joining border tokens
/// with <c>|</c>, for example <c>px(1) | solid | hex("d7ebe4")</c>.
/// 已组合的 border 简写：可由 <c>border(...)</c> 或 <c>|</c> 连接 token 得到。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssBorder
{
    internal CssBorder() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssBorder create(string value);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorder left, ICssBorderPart right);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorder left, CssColorKeyword right);
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

/// <summary>
/// A one-to-four-value padding shorthand. The dedicated carrier keeps box shorthand syntax out
/// of unrelated single-length properties such as <c>width</c>.
/// 一至四值的 padding 简写；使用独立载体，避免盒模型简写误入 width 等单值属性。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssPadding
{
    internal CssPadding() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssPadding create(string value);
}

/// <summary>
/// A one-to-four-value margin shorthand, including mixed <c>auto</c> values.
/// 一至四值的 margin 简写，可在任意位置使用 <c>auto</c>。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssMargin
{
    internal CssMargin() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssMargin create(string value);
}

/// <summary>
/// A row/column gap pair. It cannot be assigned to ordinary length properties.
/// row/column 的 gap 组合值，不能赋给普通长度属性。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssGap
{
    internal CssGap() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssGap create(string value);
}

/// <summary>
/// A one-to-four-corner radius shorthand.
/// 一至四角的圆角简写。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssRadius
{
    internal CssRadius() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssRadius create(string value);
}

/// <summary>
/// A structured <c>flex-grow flex-shrink flex-basis</c> shorthand.
/// 结构化的 <c>flex-grow flex-shrink flex-basis</c> 简写。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssFlex
{
    internal CssFlex() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssFlex create(string value);
}

/// <summary>
/// A two-value background-size expression.
/// 双值 background-size 表达式。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssBackgroundSize
{
    internal CssBackgroundSize() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssBackgroundSize create(string value);
}

/// <summary>
/// A grid line or line range such as <c>1 / -1</c>.
/// 网格线或网格线区间，例如 <c>1 / -1</c>。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssGridLine
{
    internal CssGridLine() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssGridLine create(string value);
}

/// <summary>
/// A typed CSS gradient accepted by image/background domains.
/// 可用于 image/background 值域的强类型 CSS 渐变。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssGradient
{
    internal CssGradient() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssGradient create(string value);
}

/// <summary>
/// A structured animation shorthand.
/// 结构化 animation 简写。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssAnimation
{
    internal CssAnimation() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssAnimation create(string value);
}

/// <summary>
/// A comma-separated font-family fallback list.
/// 以逗号连接的 font-family 后备列表。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssFontFamily
{
    internal CssFontFamily() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssFontFamily create(string value);
}

/// <summary>
/// One quoted or generic font-family entry.
/// 单个引号字体族或通用字体族条目。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssFontFamilyName
{
    internal CssFontFamilyName() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssFontFamilyName create(string value);
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

/// <summary>
/// A named CSS line width which can participate in a border shorthand. It is a token object,
/// rather than an enum, because C# enums cannot define the <c>|</c> composition operator.
/// 可参与 border 简写的具名线宽。使用 token 类型而非 enum，原因是 C# enum 不能定义 <c>|</c>。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssBorderWidth : ICssBorderPart
{
    internal CssBorderWidth() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssBorderWidth create(string value);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorderWidth left, ICssBorderPart right);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorderWidth left, CssColorKeyword right);
}

/// <summary>
/// A CSS border/outline line-style token. It is composable so C# can retain CSS shorthand order.
/// CSS border/outline 线型 token，可通过 <c>|</c> 保留简写表达顺序。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssBorderStyle : ICssBorderPart
{
    internal CssBorderStyle() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssBorderStyle create(string value);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorderStyle left, ICssBorderPart right);

    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorderStyle left, CssColorKeyword right);
}

/// <summary>
/// Marks values valid as one token of a border shorthand. The resulting <see cref="CssBorder"/>
/// remains restricted to border properties instead of becoming a general untyped CSS string.
/// 标记可作为 border 简写 token 的值；组合结果仍是 CssBorder，不会退化成通用字符串。
/// </summary>
public interface ICssBorderPart
{
}

[String]
public enum CssColorKeyword
{
    [Description("@#transparent")] Transparent,
    [Description("@#currentColor")] CurrentColor
}

[String]
public enum CssAlignmentKeyword
{
    [Description("@#start")] Start,
    [Description("@#end")] End,
    [Description("@#center")] Center,
    [Description("@#flex-start")] FlexStart,
    [Description("@#flex-end")] FlexEnd,
    [Description("@#self-start")] SelfStart,
    [Description("@#self-end")] SelfEnd,
    [Description("@#left")] Left,
    [Description("@#right")] Right,
    [Description("@#stretch")] Stretch,
    [Description("@#baseline")] Baseline,
    [Description("@#space-between")] SpaceBetween,
    [Description("@#space-around")] SpaceAround,
    [Description("@#space-evenly")] SpaceEvenly
}

[String]
public enum CssFlexDirectionKeyword
{
    [Description("@#row")] Row,
    [Description("@#row-reverse")] RowReverse,
    [Description("@#column")] Column,
    [Description("@#column-reverse")] ColumnReverse
}

[String]
public enum CssFlexWrapKeyword
{
    [Description("@#nowrap")] NoWrap,
    [Description("@#wrap")] Wrap,
    [Description("@#wrap-reverse")] WrapReverse
}

[String]
public enum CssBackgroundSizeKeyword
{
    [Description("@#cover")] Cover,
    [Description("@#contain")] Contain
}

[String]
public enum CssBoxSizingKeyword
{
    [Description("@#border-box")] BorderBox,
    [Description("@#content-box")] ContentBox
}

[String]
public enum CssCursorKeyword
{
    [Description("@#default")] Default,
    [Description("@#pointer")] Pointer,
    [Description("@#not-allowed")] NotAllowed,
    [Description("@#text")] Text,
    [Description("@#move")] Move,
    [Description("@#grab")] Grab,
    [Description("@#grabbing")] Grabbing
}

[String]
public enum CssTextTransformKeyword
{
    [Description("@#capitalize")] Capitalize,
    [Description("@#uppercase")] Uppercase,
    [Description("@#lowercase")] Lowercase,
    [Description("@#full-width")] FullWidth,
    [Description("@#full-size-kana")] FullSizeKana
}

[String]
public enum CssWhiteSpaceKeyword
{
    [Description("@#nowrap")] NoWrap,
    [Description("@#pre")] Pre,
    [Description("@#pre-wrap")] PreWrap,
    [Description("@#pre-line")] PreLine,
    [Description("@#break-spaces")] BreakSpaces
}

[String]
public enum CssTextOverflowKeyword
{
    [Description("@#clip")] Clip,
    [Description("@#ellipsis")] Ellipsis
}

[String]
public enum CssIsolationKeyword
{
    [Description("@#isolate")] Isolate
}

[String]
public enum CssColorSchemeKeyword
{
    [Description("@#light")] Light,
    [Description("@#dark")] Dark,
    [Description("@#light dark")] LightDark
}

[String]
public enum CssTimingFunctionKeyword
{
    [Description("@#linear")] Linear,
    [Description("@#ease")] Ease,
    [Description("@#ease-in")] EaseIn,
    [Description("@#ease-out")] EaseOut,
    [Description("@#ease-in-out")] EaseInOut,
    [Description("@#step-start")] StepStart,
    [Description("@#step-end")] StepEnd
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
    CssFilter,
    CssBorder,
    CssShadowList,
    CssTrack,
    CssPadding,
    CssMargin,
    CssGap,
    CssRadius,
    CssFlex,
    CssBackgroundSize,
    CssGridLine,
    CssGradient,
    CssAnimation,
    CssFontFamily,
    CssRatio,
    CssWideKeyword,
    CssAutoKeyword,
    CssNoneKeyword,
    CssNormalKeyword,
    CssSizingKeyword,
    CssDisplayKeyword,
    CssPositionKeyword,
    CssOverflowKeyword,
    CssBorderWidth,
    CssBorderStyle,
    CssColorKeyword,
    CssAlignmentKeyword,
    CssFlexDirectionKeyword,
    CssFlexWrapKeyword,
    CssBackgroundSizeKeyword,
    CssBoxSizingKeyword,
    CssCursorKeyword,
    CssTextTransformKeyword,
    CssWhiteSpaceKeyword,
    CssTextOverflowKeyword,
    CssIsolationKeyword,
    CssColorSchemeKeyword,
    CssTimingFunctionKeyword,
    CssImportant<double>,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssAngle>, CssImportant<CssTime>, CssImportant<CssFrequency>,
    CssImportant<CssResolution>, CssImportant<CssColor>, CssImportant<CssImage>, CssImportant<CssUrl>,
    CssImportant<CssString>, CssImportant<CssIdent>, CssImportant<CssKeyword>, CssImportant<CssTransform>,
    CssImportant<CssFilter>, CssImportant<CssBorder>, CssImportant<CssShadowList>, CssImportant<CssTrack>,
    CssImportant<CssPadding>, CssImportant<CssMargin>, CssImportant<CssGap>, CssImportant<CssRadius>,
    CssImportant<CssFlex>, CssImportant<CssBackgroundSize>, CssImportant<CssGridLine>, CssImportant<CssGradient>,
    CssImportant<CssAnimation>, CssImportant<CssFontFamily>, CssImportant<CssRatio>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>,
    CssImportant<CssSizingKeyword>, CssImportant<CssDisplayKeyword>, CssImportant<CssPositionKeyword>,
    CssImportant<CssOverflowKeyword>, CssImportant<CssBorderWidth>, CssImportant<CssBorderStyle>,
    CssImportant<CssColorKeyword>, CssImportant<CssAlignmentKeyword>, CssImportant<CssFlexDirectionKeyword>,
    CssImportant<CssFlexWrapKeyword>, CssImportant<CssBackgroundSizeKeyword>, CssImportant<CssBoxSizingKeyword>,
    CssImportant<CssCursorKeyword>, CssImportant<CssTextTransformKeyword>, CssImportant<CssWhiteSpaceKeyword>,
    CssImportant<CssTextOverflowKeyword>, CssImportant<CssIsolationKeyword>, CssImportant<CssColorSchemeKeyword>,
    CssImportant<CssTimingFunctionKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssKeywordValue(
    CssRaw, CssVariable, CssIdent, CssKeyword, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssSizingKeyword, CssDisplayKeyword, CssPositionKeyword, CssOverflowKeyword, CssBorderWidth,
    CssBorderStyle, CssColorKeyword, CssAlignmentKeyword, CssFlexDirectionKeyword, CssFlexWrapKeyword,
    CssBackgroundSizeKeyword, CssBoxSizingKeyword, CssCursorKeyword, CssTextTransformKeyword, CssWhiteSpaceKeyword,
    CssTextOverflowKeyword, CssIsolationKeyword, CssColorSchemeKeyword, CssTimingFunctionKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssIdent>, CssImportant<CssKeyword>,
    CssImportant<CssWideKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>,
    CssImportant<CssNormalKeyword>, CssImportant<CssSizingKeyword>, CssImportant<CssDisplayKeyword>,
    CssImportant<CssPositionKeyword>, CssImportant<CssOverflowKeyword>, CssImportant<CssBorderWidth>,
    CssImportant<CssBorderStyle>, CssImportant<CssColorKeyword>, CssImportant<CssAlignmentKeyword>,
    CssImportant<CssFlexDirectionKeyword>, CssImportant<CssFlexWrapKeyword>, CssImportant<CssBackgroundSizeKeyword>,
    CssImportant<CssBoxSizingKeyword>, CssImportant<CssCursorKeyword>, CssImportant<CssTextTransformKeyword>,
    CssImportant<CssWhiteSpaceKeyword>, CssImportant<CssTextOverflowKeyword>, CssImportant<CssIsolationKeyword>,
    CssImportant<CssColorSchemeKeyword>, CssImportant<CssTimingFunctionKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssPaddingPart(CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage);

[ECMAScript]
[Description("@#")]
public readonly union CssMarginPart(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssAutoKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssGapPart(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssNormalKeyword);

[ECMAScript]
[Description("@#")]
public readonly union CssRadiusPart(CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage);

[ECMAScript]
[Description("@#")]
public readonly union CssPaddingValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssPadding, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssPadding>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssMarginValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssMargin, CssAutoKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssMargin>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssGapValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssGap, CssNormalKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssGap>, CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssRadiusValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssRadius, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssRadius>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssFlexValue(
    double, CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssFlex, CssAutoKeyword,
    CssNoneKeyword, CssWideKeyword, CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssLength>, CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssFlex>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssGridLineValue(
    int, CssRaw, CssVariable, CssIdent, CssGridLine, CssAutoKeyword, CssWideKeyword, CssImportant<int>,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssIdent>, CssImportant<CssGridLine>,
    CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssTextAlignValue(
    CssRaw, CssVariable, CssKeyword, CssAlignmentKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssKeyword>, CssImportant<CssAlignmentKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssLengthValue(
    CssRaw, CssVariable, CssLength, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssSizingKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>,
    CssImportant<CssWideKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>,
    CssImportant<CssNormalKeyword>, CssImportant<CssSizingKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssLengthPercentageValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssSizingKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>,
    CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssSizingKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssLengthPercentageNumberValue(
    double, CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssSizingKeyword, CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssLength>, CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssSizingKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssPercentageValue(
    CssRaw, CssVariable, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssPercentage>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssNumberValue(
    double, CssRaw, CssVariable, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssIntegerValue(
    int, CssRaw, CssVariable, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<int>, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssNumberPercentageValue(
    double, CssRaw, CssVariable, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssPercentage>, CssImportant<CssWideKeyword>, CssImportant<CssAutoKeyword>,
    CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssAngleValue(
    CssRaw, CssVariable, CssAngle, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssAngle>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssTimeValue(
    CssRaw, CssVariable, CssTime, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssTime>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssFrequencyValue(
    CssRaw, CssVariable, CssFrequency, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssFrequency>,
    CssImportant<CssPercentage>, CssImportant<CssWideKeyword>, CssImportant<CssAutoKeyword>,
    CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssResolutionValue(
    CssRaw, CssVariable, CssResolution, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssResolution>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssColorValue(
    CssRaw, CssVariable, CssColor, CssColorKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssColor>, CssImportant<CssColorKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssShadowLength(CssLength, CssVariable);

[ECMAScript]
[Description("@#")]
public readonly union CssShadowColor(CssColor, CssColorKeyword, CssVariable);

[ECMAScript]
[Description("@#")]
public readonly union CssBoxShadowValue(
    CssRaw, CssVariable, CssNoneKeyword, CssWideKeyword, CssShadowList, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>, CssImportant<CssShadowList>);

[ECMAScript]
[Description("@#")]
public readonly union CssImageValue(
    CssRaw, CssVariable, CssImage, CssUrl, CssGradient, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssImage>, CssImportant<CssUrl>, CssImportant<CssGradient>,
    CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssStringValue(
    CssRaw, CssVariable, CssString, CssNoneKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssString>, CssImportant<CssNoneKeyword>,
    CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssTransformValue(
    CssRaw, CssVariable, CssTransform, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssTransform>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssLineWidthValue(
    CssRaw, CssVariable, CssLength, CssBorderWidth, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssLength>, CssImportant<CssBorderWidth>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssLineStyleValue(
    CssRaw, CssVariable, CssNoneKeyword, CssBorderStyle, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssNoneKeyword>, CssImportant<CssBorderStyle>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssDisplayValue(
    CssRaw, CssVariable, CssNoneKeyword, CssDisplayKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssNoneKeyword>, CssImportant<CssDisplayKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssPositionValue(
    CssRaw, CssVariable, CssPositionKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssPositionKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssOverflowValue(
    CssRaw, CssVariable, CssAutoKeyword, CssOverflowKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssAutoKeyword>, CssImportant<CssOverflowKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssTrackValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssTrack, CssAutoKeyword, CssSizingKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssTrack>, CssImportant<CssAutoKeyword>, CssImportant<CssSizingKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssRatioValue(
    CssRaw, CssVariable, CssRatio, CssAutoKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssRatio>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssAlignmentValue(
    CssRaw, CssVariable, CssAlignmentKeyword, CssAutoKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssAlignmentKeyword>, CssImportant<CssAutoKeyword>,
    CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssFlexDirectionValue(
    CssRaw, CssVariable, CssFlexDirectionKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssFlexDirectionKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssFlexWrapValue(
    CssRaw, CssVariable, CssFlexWrapKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssFlexWrapKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssBackgroundSizeValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssBackgroundSize, CssBackgroundSizeKeyword, CssAutoKeyword,
    CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>,
    CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssBackgroundSize>,
    CssImportant<CssBackgroundSizeKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssBoxSizingValue(
    CssRaw, CssVariable, CssBoxSizingKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssBoxSizingKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssCursorValue(
    CssRaw, CssVariable, CssCursorKeyword, CssAutoKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssCursorKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssTextTransformValue(
    CssRaw, CssVariable, CssTextTransformKeyword, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssTextTransformKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssWhiteSpaceValue(
    CssRaw, CssVariable, CssWhiteSpaceKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssWhiteSpaceKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssTextOverflowValue(
    CssRaw, CssVariable, CssTextOverflowKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssTextOverflowKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssIsolationValue(
    CssRaw, CssVariable, CssIsolationKeyword, CssAutoKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssIsolationKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssColorSchemeValue(
    CssRaw, CssVariable, CssColorSchemeKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssColorSchemeKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssFilterValue(
    CssRaw, CssVariable, CssFilter, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssFilter>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

[ECMAScript]
[Description("@#")]
public readonly union CssBorderValue(
    CssRaw, CssVariable, CssBorder, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssBorder>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);
