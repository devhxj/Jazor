namespace ECMAScript.Style;

/// <summary>
/// Represents an explicitly authored CSS fragment whose grammar is not yet modeled by a dedicated carrier.
/// Use <c>raw(...)</c> only at a deliberate boundary: it bypasses property-specific authoring checks but still
/// remains a CSS value, not a plain string declaration.
/// 表示尚未由专用载体建模的显式 CSS 片段。只能在明确的边界处使用 <c>raw(...)</c>：它会绕过
/// 属性特定的作者检查，但仍是 CSS 值，而不是普通字符串声明。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssRaw : ICssBorderPart
{
    internal CssRaw() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssRaw create(string value);

    /// <summary>Appends a typed border token to a raw fragment and returns a border-only value. 将类型化边框 token 追加到原始片段，并返回仅限边框属性使用的值。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssRaw left, ICssBorderPart right);

    /// <summary>Appends a closed color keyword to a raw border fragment. 将封闭颜色关键字追加到原始边框片段。</summary>
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

/// <summary>
/// Represents a resolved <c>var(--name)</c> reference, optionally with a typed fallback.
/// The carrier is accepted only by value domains that permit CSS custom-property substitution.
/// 表示已解析的 <c>var(--name)</c> 引用，可带类型化后备值。该载体只会被允许 CSS 自定义属性替换的
/// 值域接受。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssVariable : ICssBorderPart
{
    internal CssVariable() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssVariable create(string value);

    /// <summary>Appends a typed border token after a custom-property reference. 在自定义属性引用后追加类型化边框 token。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssVariable left, ICssBorderPart right);

    /// <summary>Appends a closed color keyword after a custom-property reference in a border shorthand. 在边框简写中将封闭颜色关键字追加到自定义属性引用后。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssVariable left, CssColorKeyword right);
}

/// <summary>
/// Represents one CSS length and preserves its length-only domain through arithmetic.
/// Combining a length with a percentage produces <see cref="CssLengthPercentage"/>, so properties can retain
/// the grammar distinction instead of accepting all numeric expressions.
/// 表示一个 CSS 长度，并在算术中保持仅长度值域。长度与百分比组合后会得到
/// <see cref="CssLengthPercentage"/>，从而让属性保留 grammar 区别而不接受所有数值表达式。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed partial class CssLength : ICssBorderPart
{
    internal CssLength() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssLength create(string value);

    /// <summary>Adds two lengths as a typed <c>calc(...)</c> length expression. 将两个长度相加为类型化 <c>calc(...)</c> 长度表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLength operator +(CssLength left, CssLength right);

    /// <summary>Adds a length and percentage, widening only to the length-percentage domain. 将长度与百分比相加，只扩展为长度/百分比值域。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLength left, CssPercentage right);

    /// <summary>Subtracts two lengths as a typed <c>calc(...)</c> length expression. 将两个长度相减为类型化 <c>calc(...)</c> 长度表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLength operator -(CssLength left, CssLength right);

    /// <summary>Subtracts a percentage from a length and returns a length-percentage expression. 从长度减去百分比并返回长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLength left, CssPercentage right);

    /// <summary>Scales a length by a unitless factor. 使用无单位系数缩放长度。</summary>
    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLength operator *(CssLength value, double factor);

    /// <summary>Scales a length by a unitless factor written on the left. 使用写在左侧的无单位系数缩放长度。</summary>
    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLength operator *(double factor, CssLength value);

    /// <summary>Divides a length by a unitless divisor. 使用无单位除数除以长度。</summary>
    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssLength operator /(CssLength value, double divisor);

    /// <summary>Negates a length while retaining its length-only domain. 对长度取负，同时保持仅长度值域。</summary>
    [ECMAScriptInline("`calc(-1 * ${__arg1})`")]
    public static extern CssLength operator -(CssLength value);

    /// <summary>
    /// Joins two dimensions for multi-value properties; the result cannot be used as a single length.
    /// This exact overload takes precedence over border-part composition.
    /// 为多值属性组合两个尺寸，结果不能用作单个长度；精确重载优先于 border-part 组合。
    /// </summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssLength left, CssLength right);

    /// <summary>Appends a border token after a length in a border shorthand. 在边框简写中将边框 token 追加到长度后。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssLength left, ICssBorderPart right);

    /// <summary>Appends a color keyword after a length in a border shorthand. 在边框简写中将颜色关键字追加到长度后。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssLength left, CssColorKeyword right);
}

/// <summary>
/// Represents one CSS percentage. It remains distinct from a raw number because its reference box or basis is
/// defined by the receiving property; mixed length arithmetic widens only to <see cref="CssLengthPercentage"/>.
/// 表示一个 CSS 百分比。它不同于原始数值，因为其参考框或基值由接收属性定义；与长度混合运算只会
/// 扩展为 <see cref="CssLengthPercentage"/>。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed partial class CssPercentage
{
    internal CssPercentage() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssPercentage create(string value);

    /// <summary>Adds two percentages as a typed <c>calc(...)</c> percentage expression. 将两个百分比相加为类型化 <c>calc(...)</c> 百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssPercentage operator +(CssPercentage left, CssPercentage right);

    /// <summary>Adds a percentage and length, widening only to the length-percentage domain. 将百分比与长度相加，只扩展为长度/百分比值域。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssPercentage left, CssLength right);

    /// <summary>Subtracts two percentages as a typed <c>calc(...)</c> percentage expression. 将两个百分比相减为类型化 <c>calc(...)</c> 百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssPercentage operator -(CssPercentage left, CssPercentage right);

    /// <summary>Subtracts a length from a percentage and returns a length-percentage expression. 从百分比减去长度并返回长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssPercentage left, CssLength right);

    /// <summary>Scales a percentage by a unitless factor. 使用无单位系数缩放百分比。</summary>
    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssPercentage operator *(CssPercentage value, double factor);

    /// <summary>Divides a percentage by a unitless divisor. 使用无单位除数除以百分比。</summary>
    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssPercentage operator /(CssPercentage value, double divisor);
}

/// <summary>
/// Represents a typed <c>calc(...)</c> length-percentage expression. It is the only arithmetic result that may
/// cross the length/percentage boundary, preventing unrelated numeric domains from being silently admitted.
/// 表示类型化的 <c>calc(...)</c> length-percentage 表达式。它是唯一可以跨越长度/百分比边界的
/// 算术结果，避免无关数值域被静默接受。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed partial class CssLengthPercentage
{
    internal CssLengthPercentage() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssLengthPercentage create(string value);

    /// <summary>Adds two length-percentage expressions. 将两个长度/百分比表达式相加。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLengthPercentage left, CssLengthPercentage right);

    /// <summary>Adds a length to a length-percentage expression. 将长度加到长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLengthPercentage left, CssLength right);

    /// <summary>Adds a length-percentage expression to a length written on the left. 将长度/百分比表达式加到左侧的长度。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLength left, CssLengthPercentage right);

    /// <summary>Adds a percentage to a length-percentage expression. 将百分比加到长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssLengthPercentage left, CssPercentage right);

    /// <summary>Adds a length-percentage expression to a percentage written on the left. 将长度/百分比表达式加到左侧的百分比。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssLengthPercentage operator +(CssPercentage left, CssLengthPercentage right);

    /// <summary>Subtracts two length-percentage expressions. 将两个长度/百分比表达式相减。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage left, CssLengthPercentage right);

    /// <summary>Subtracts a length from a length-percentage expression. 从长度/百分比表达式减去长度。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage left, CssLength right);

    /// <summary>Subtracts a length-percentage expression from a length. 从长度减去长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLength left, CssLengthPercentage right);

    /// <summary>Subtracts a percentage from a length-percentage expression. 从长度/百分比表达式减去百分比。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage left, CssPercentage right);

    /// <summary>Subtracts a length-percentage expression from a percentage. 从百分比减去长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssLengthPercentage operator -(CssPercentage left, CssLengthPercentage right);

    /// <summary>Scales a length-percentage expression by a unitless factor. 使用无单位系数缩放长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLengthPercentage operator *(CssLengthPercentage value, double factor);

    /// <summary>Scales a length-percentage expression by a unitless factor written on the left. 使用写在左侧的无单位系数缩放长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssLengthPercentage operator *(double factor, CssLengthPercentage value);

    /// <summary>Divides a length-percentage expression by a unitless divisor. 使用无单位除数除以长度/百分比表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssLengthPercentage operator /(CssLengthPercentage value, double divisor);

    /// <summary>Negates a length-percentage expression. 对长度/百分比表达式取负。</summary>
    [ECMAScriptInline("`calc(-1 * ${__arg1})`")]
    public static extern CssLengthPercentage operator -(CssLengthPercentage value);
}

/// <summary>
/// Represents the <c>fit-content(...)</c> sizing function. It is intentionally distinct from
/// <see cref="CssTrack"/> so the same function can be admitted by grid-track and box-sizing
/// properties without admitting arbitrary track expressions.
/// 表示 <c>fit-content(...)</c> 尺寸函数。它刻意不同于 <see cref="CssTrack"/>，因此网格轨道和
/// 盒尺寸属性都可接受该函数，同时不会意外接受任意轨道表达式。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssFitContent
{
    internal CssFitContent() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssFitContent create(string value);
}

/// <summary>
/// Represents a named anchor reference such as <c>--card</c>. Anchor names are authored through
/// <c>anchor_name(...)</c>, which enforces the CSS dashed-identifier shape.
/// 表示如 <c>--card</c> 的命名锚点引用。应通过 <c>anchor_name(...)</c> 创建，它会约束 CSS
/// dashed-identifier 形式。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssAnchorName
{
    internal CssAnchorName() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssAnchorName create(string value);
}

/// <summary>
/// Represents a space-separated anchor-name list used by <c>anchor-name</c> and
/// <c>anchor-scope</c>. The list carrier prevents an unrelated arbitrary identifier list from
/// being assigned to those properties.
/// 表示 <c>anchor-name</c> 和 <c>anchor-scope</c> 使用的空格分隔锚点名称列表。专用载体可避免
/// 无关的任意标识符列表被赋给这些属性。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssAnchorNameList
{
    internal CssAnchorNameList() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssAnchorNameList create(string value);
}

/// <summary>
/// Represents an <c>anchor(...)</c> position expression. It is accepted only by anchor-aware
/// inset properties and cannot be used as a general length value.
/// 表示 <c>anchor(...)</c> 位置表达式。它仅可用于支持锚点的 inset 属性，不能作为通用长度值使用。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssAnchor
{
    internal CssAnchor() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssAnchor create(string value);
}

/// <summary>
/// Represents an <c>anchor-size(...)</c> sizing expression. It remains separate from
/// <see cref="CssAnchor"/> because CSS permits the two functions in different property domains.
/// 表示 <c>anchor-size(...)</c> 尺寸表达式。它与 <see cref="CssAnchor"/> 分离，因为 CSS 在不同
/// 属性值域中分别允许这两个函数。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssAnchorSize
{
    internal CssAnchorSize() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssAnchorSize create(string value);
}

/// <summary>
/// Represents a <c>calc-size(...)</c> result. Its basis and calculation are assembled through
/// <c>calc_size(...)</c>; this carrier cannot be mistaken for a normal <c>calc(...)</c> length.
/// 表示 <c>calc-size(...)</c> 的结果。其基值和计算式通过 <c>calc_size(...)</c> 组合；该载体不能
/// 被误认为普通的 <c>calc(...)</c> 长度。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssCalcSize
{
    internal CssCalcSize() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssCalcSize create(string value);
}

/// <summary>
/// Represents the result-side arithmetic expression inside <c>calc-size(...)</c>. Start with
/// <c>size</c> and use the provided arithmetic operators so the emitted CSS preserves the
/// expression rather than falling back to a raw string.
/// 表示 <c>calc-size(...)</c> 内的结果侧算术表达式。应从 <c>size</c> 开始并使用提供的算术运算符，
/// 以保留输出 CSS 表达式，而不是退回为原始字符串。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssCalcSizeExpression
{
    internal CssCalcSizeExpression() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssCalcSizeExpression create(string value);

    /// <summary>Adds a length to a <c>calc-size(...)</c> result expression. 将长度加到 <c>calc-size(...)</c> 的结果表达式。</summary>
    [ECMAScriptInline("__arg1 + \" + \" + __arg2")]
    public static extern CssCalcSizeExpression operator +(CssCalcSizeExpression left, CssLength right);

    /// <summary>Adds a percentage to a <c>calc-size(...)</c> result expression. 将百分比加到 <c>calc-size(...)</c> 的结果表达式。</summary>
    [ECMAScriptInline("__arg1 + \" + \" + __arg2")]
    public static extern CssCalcSizeExpression operator +(CssCalcSizeExpression left, CssPercentage right);

    /// <summary>Adds a length-percentage expression to a <c>calc-size(...)</c> result expression. 将长度/百分比表达式加到 <c>calc-size(...)</c> 的结果表达式。</summary>
    [ECMAScriptInline("__arg1 + \" + \" + __arg2")]
    public static extern CssCalcSizeExpression operator +(CssCalcSizeExpression left, CssLengthPercentage right);

    /// <summary>Subtracts a length from a <c>calc-size(...)</c> result expression. 从 <c>calc-size(...)</c> 的结果表达式减去长度。</summary>
    [ECMAScriptInline("__arg1 + \" - \" + __arg2")]
    public static extern CssCalcSizeExpression operator -(CssCalcSizeExpression left, CssLength right);

    /// <summary>Subtracts a percentage from a <c>calc-size(...)</c> result expression. 从 <c>calc-size(...)</c> 的结果表达式减去百分比。</summary>
    [ECMAScriptInline("__arg1 + \" - \" + __arg2")]
    public static extern CssCalcSizeExpression operator -(CssCalcSizeExpression left, CssPercentage right);

    /// <summary>Subtracts a length-percentage expression from a <c>calc-size(...)</c> result expression. 从 <c>calc-size(...)</c> 的结果表达式减去长度/百分比表达式。</summary>
    [ECMAScriptInline("__arg1 + \" - \" + __arg2")]
    public static extern CssCalcSizeExpression operator -(CssCalcSizeExpression left, CssLengthPercentage right);

    /// <summary>Scales a <c>calc-size(...)</c> result expression by a unitless factor. 使用无单位系数缩放 <c>calc-size(...)</c> 的结果表达式。</summary>
    [ECMAScriptInline("__arg1 + \" * \" + __arg2")]
    public static extern CssCalcSizeExpression operator *(CssCalcSizeExpression value, double factor);

    /// <summary>Scales a <c>calc-size(...)</c> result expression by a left-side unitless factor. 使用左侧无单位系数缩放 <c>calc-size(...)</c> 的结果表达式。</summary>
    [ECMAScriptInline("__arg1 + \" * \" + __arg2")]
    public static extern CssCalcSizeExpression operator *(double factor, CssCalcSizeExpression value);

    /// <summary>Divides a <c>calc-size(...)</c> result expression by a unitless divisor. 使用无单位除数除以 <c>calc-size(...)</c> 的结果表达式。</summary>
    [ECMAScriptInline("__arg1 + \" / \" + __arg2")]
    public static extern CssCalcSizeExpression operator /(CssCalcSizeExpression value, double divisor);
}

/// <summary>
/// Represents an angle value or a typed <c>calc(...)</c> angle expression for transform, gradient, and other
/// angle-valued properties. It cannot be assigned where a length or time is required.
/// 表示 transform、gradient 等角度属性使用的角度值或类型化 <c>calc(...)</c> 角度表达式。它不能赋给
/// 需要长度或时间的属性。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssAngle
{
    internal CssAngle() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssAngle create(string value);

    /// <summary>Adds two CSS angles as a typed <c>calc(...)</c> expression. 将两个 CSS 角度相加为类型化 <c>calc(...)</c> 表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssAngle operator +(CssAngle left, CssAngle right);

    /// <summary>Subtracts two CSS angles as a typed <c>calc(...)</c> expression. 将两个 CSS 角度相减为类型化 <c>calc(...)</c> 表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssAngle operator -(CssAngle left, CssAngle right);

    /// <summary>Scales a CSS angle by a unitless factor. 使用无单位系数缩放 CSS 角度。</summary>
    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssAngle operator *(CssAngle value, double factor);

    /// <summary>Divides a CSS angle by a unitless divisor. 使用无单位除数除以 CSS 角度。</summary>
    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssAngle operator /(CssAngle value, double divisor);
}

/// <summary>
/// Represents a CSS time value or typed time arithmetic for transitions and animations.
/// The dedicated carrier prevents duration and delay values from being confused with ordinary numbers.
/// 表示 transition、animation 使用的 CSS 时间值或类型化时间算术。专用载体避免 duration、delay
/// 与普通数值混淆。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssTime
{
    internal CssTime() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssTime create(string value);

    /// <summary>Adds two CSS time values as a typed <c>calc(...)</c> expression. 将两个 CSS 时间值相加为类型化 <c>calc(...)</c> 表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} + ${__arg2})`")]
    public static extern CssTime operator +(CssTime left, CssTime right);

    /// <summary>Subtracts two CSS time values as a typed <c>calc(...)</c> expression. 将两个 CSS 时间值相减为类型化 <c>calc(...)</c> 表达式。</summary>
    [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
    public static extern CssTime operator -(CssTime left, CssTime right);

    /// <summary>Scales a CSS time value by a unitless factor. 使用无单位系数缩放 CSS 时间值。</summary>
    [ECMAScriptInline("`calc(${__arg1} * ${__arg2})`")]
    public static extern CssTime operator *(CssTime value, double factor);

    /// <summary>Divides a CSS time value by a unitless divisor. 使用无单位除数除以 CSS 时间值。</summary>
    [ECMAScriptInline("`calc(${__arg1} / ${__arg2})`")]
    public static extern CssTime operator /(CssTime value, double divisor);
}

/// <summary>
/// Represents a CSS frequency value for grammar positions such as aural or media-related values.
/// It remains separate from numeric and time domains even though all serialize to text at runtime.
/// 表示声音或媒体相关 grammar 位置使用的 CSS 频率值。即使运行时都会序列化为文本，它仍与数值和时间值域分离。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssFrequency
{
    internal CssFrequency() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssFrequency create(string value);
}

/// <summary>
/// Represents a CSS resolution value such as <c>dpi</c> or <c>dppx</c>.
/// Use it only in properties whose grammar explicitly accepts a resolution.
/// 表示如 <c>dpi</c>、<c>dppx</c> 的 CSS 分辨率值。仅应在 grammar 明确接受分辨率的属性中使用。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssResolution
{
    internal CssResolution() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssResolution create(string value);
}

/// <summary>
/// Represents a concrete CSS color function, hexadecimal color, or validated named color.
/// It can participate in border shorthand composition but does not widen into arbitrary CSS text.
/// 表示具体 CSS 颜色函数、十六进制颜色或已验证的具名颜色。它可参与 border 简写组合，但不会扩展为任意 CSS 文本。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssColor : ICssBorderPart
{
    internal CssColor() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssColor create(string value);

    /// <summary>Appends a typed border token after a concrete color in a border shorthand. 在边框简写中将类型化边框 token 追加到具体颜色后。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssColor left, ICssBorderPart right);

    /// <summary>Appends a closed color keyword after a concrete color in a border shorthand. 在边框简写中将封闭颜色关键字追加到具体颜色后。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssColor left, CssColorKeyword right);
}

/// <summary>
/// Represents an image-producing CSS value. It is distinct from <see cref="CssUrl"/> because a property may
/// accept gradients or other image functions without accepting a standalone URL branch.
/// 表示产生图像的 CSS 值。它与 <see cref="CssUrl"/> 分离，因为属性可能接受 gradient 或其他图像函数，
/// 却不接受独立 URL 分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssImage
{
    internal CssImage() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssImage create(string value);
}

/// <summary>
/// Represents a safely quoted CSS <c>url(...)</c> function. The source text is quoted by <c>url(...)</c>
/// rather than treated as unescaped CSS syntax.
/// 表示安全引用的 CSS <c>url(...)</c> 函数。源文本由 <c>url(...)</c> 负责引号处理，而不作为未转义的 CSS 语法。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssUrl
{
    internal CssUrl() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssUrl create(string value);
}

/// <summary>
/// Represents a quoted CSS string token. Use <c>str(...)</c> instead of concatenating quote characters into a raw value.
/// 表示带引号的 CSS 字符串 token。应使用 <c>str(...)</c>，而不是把引号手动拼接进 raw 值。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssString
{
    internal CssString() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssString create(string value);
}

/// <summary>
/// Represents a validated CSS identifier, including identifiers permitted to use the custom-property prefix.
/// It is intentionally different from <see cref="CssKeyword"/>, whose input cannot use that prefix.
/// 表示经过验证的 CSS 标识符，包括允许自定义属性前缀的标识符。它刻意不同于
/// <see cref="CssKeyword"/>，后者的输入不能使用该前缀。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssIdent
{
    internal CssIdent() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssIdent create(string value);
}

/// <summary>
/// Represents a validated CSS keyword token that is not tied to one closed enum domain.
/// Prefer a dedicated enum or value carrier when the target property's grammar exposes one.
/// 表示不属于某个封闭 enum 值域的已验证 CSS 关键字 token。目标属性已有专用 enum 或值载体时应优先使用它。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssKeyword
{
    internal CssKeyword() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssKeyword create(string value);
}

/// <summary>
/// Represents one CSS transform function or an ordered transform-function composition.
/// A transform carrier cannot be assigned to geometry properties merely because its arguments are lengths.
/// 表示一个 CSS transform 函数或有序 transform 函数组合。即使参数是长度，transform 载体也不能赋给几何属性。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssTransform
{
    internal CssTransform() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssTransform create(string value);

    /// <summary>Composes transforms in application order; order is observable。按书写顺序组合变换，顺序影响结果。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssTransform operator |(CssTransform left, CssTransform right);
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

    /// <summary>Composes filters in application order; never sorts or deduplicates them。按书写顺序组合滤镜，不排序或去重。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssFilter operator |(CssFilter left, CssFilter right);
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

    /// <summary>Appends another typed border token while preserving shorthand order. 追加另一个类型化边框 token，并保留简写顺序。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorder left, ICssBorderPart right);

    /// <summary>Appends a closed color keyword to an existing border shorthand. 将封闭颜色关键字追加到已有边框简写。</summary>
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

/// <summary>
/// Represents one grid track function or a composed grid track list fragment.
/// It is intentionally not a box-size value: <c>fr</c>, <c>repeat(...)</c>, and <c>minmax(...)</c> have
/// grid-only semantics even when they contain length-like arguments.
/// 表示一个 grid track 函数或组合后的 grid track 列表片段。它刻意不是盒尺寸值：<c>fr</c>、
/// <c>repeat(...)</c>、<c>minmax(...)</c> 即使包含类似长度的参数，也具有仅限网格的语义。
/// </summary>
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
/// A one-to-four-value <c>inset</c> shorthand. Its parts may use anchor-position functions,
/// but the composed carrier cannot be assigned to an individual side such as <c>top</c>.
/// 一至四值的 <c>inset</c> 简写。其组成部分可使用锚点定位函数，但组合后的载体不能赋给
/// <c>top</c> 等单独边。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssInset
{
    internal CssInset() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssInset create(string value);
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

/// <summary>
/// Represents a positive CSS ratio such as <c>16 / 9</c> for <c>aspect-ratio</c>.
/// The factory validates both sides so an invalid denominator is rejected before CSS serialization.
/// 表示如 <c>16 / 9</c> 的正 CSS 比率，用于 <c>aspect-ratio</c>。工厂会验证两个分量，
/// 因而在 CSS 序列化前拒绝无效分母。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class CssRatio
{
    internal CssRatio() { }

    [ECMAScriptInline("__arg1")]
    internal static extern CssRatio create(string value);
}

/// <summary>
/// Lists CSS-wide cascade keywords. They are modeled separately so properties may opt in only where their
/// grammar permits CSS-wide values.
/// 列出 CSS-wide cascade 关键字。它们单独建模，使各属性只在自身 grammar 允许时才接纳这些值。
/// </summary>
[String]
public enum CssWideKeyword
{
    /// <summary>Uses the parent element's computed value. 使用父元素的计算值。</summary>
    [Description("@#inherit")] Inherit,
    /// <summary>Uses the specification-defined initial value. 使用规范定义的初始值。</summary>
    [Description("@#initial")] Initial,
    /// <summary>Inherits when the property is inherited; otherwise uses its initial value. 继承型属性继承，其他属性使用初始值。</summary>
    [Description("@#unset")] Unset,
    /// <summary>Reverts this declaration to an earlier cascade origin. 将该声明回退到更早的层叠来源。</summary>
    [Description("@#revert")] Revert,
    /// <summary>Reverts this declaration within the current cascade-layer model. 在当前层叠层模型中回退该声明。</summary>
    [Description("@#revert-layer")] RevertLayer
}

/// <summary>
/// Represents the CSS <c>auto</c> keyword where a property's grammar has an automatic branch.
/// 表示属性 grammar 具有自动分支时可使用的 CSS <c>auto</c> 关键字。
/// </summary>
[String]
public enum CssAutoKeyword
{
    /// <summary>Lets the receiving property's automatic behavior determine the value. 由接收属性的自动行为决定值。</summary>
    [Description("@#auto")] Auto
}

/// <summary>
/// Represents the CSS <c>none</c> keyword. It is distinct from a missing declaration and can only enter
/// value domains that explicitly permit disabling a feature.
/// 表示 CSS <c>none</c> 关键字。它不同于未输出声明，且只能进入明确允许禁用功能的值域。
/// </summary>
[String]
public enum CssNoneKeyword
{
    /// <summary>Disables the feature represented by the receiving property. 禁用接收属性所表示的功能。</summary>
    [Description("@#none")] None
}

/// <summary>
/// Represents the CSS <c>normal</c> keyword for property grammars that define a normal behavior.
/// 表示定义了 normal 行为的属性 grammar 所使用的 CSS <c>normal</c> 关键字。
/// </summary>
[String]
public enum CssNormalKeyword
{
    /// <summary>Uses the normal behavior defined by the receiving property. 使用接收属性定义的 normal 行为。</summary>
    [Description("@#normal")] Normal
}

/// <summary>
/// Lists intrinsic minimum and maximum content sizing keywords.
/// They are separate from function sizing keywords because they occur in a different CSS grammar branch.
/// 列出内在的最小/最大内容尺寸关键字。它们与函数式尺寸关键字分离，因为位于不同的 CSS grammar 分支。
/// </summary>
[String]
public enum CssSizingKeyword
{
    /// <summary>Uses the smallest intrinsic content contribution. 使用最小内在内容贡献。</summary>
    [Description("@#min-content")] MinContent,
    /// <summary>Uses the largest intrinsic content contribution. 使用最大内在内容贡献。</summary>
    [Description("@#max-content")] MaxContent
}

/// <summary>
/// Specifies the modern intrinsic sizing keywords that are valid for box-size properties but
/// are not interchangeable with alignment or object-fit keywords bearing the same CSS text.
/// 指定盒尺寸属性可用的现代内在尺寸关键字。即使部分 CSS 文本与 alignment 或 object-fit 相同，
/// 它们也不与那些值域互换。
/// </summary>
[String]
public enum CssSizingFunctionKeyword
{
    /// <summary>Uses the keyword form of intrinsic <c>fit-content</c>, without a limit argument. 使用无上限参数的内在 <c>fit-content</c> 关键字形式。</summary>
    [Description("@#fit-content")] FitContent,
    /// <summary>Stretches to the available size where the property grammar permits it. 在属性语法允许时拉伸到可用尺寸。</summary>
    [Description("@#stretch")] Stretch,
    /// <summary>Uses the intrinsic containment sizing behavior. 使用内在 containment 尺寸行为。</summary>
    [Description("@#contain")] Contain
}

/// <summary>
/// Selects the geometric side read by <c>anchor(...)</c>. A percentage side is represented by
/// <see cref="CssPercentage"/> through <see cref="CssAnchorSideValue"/>.
/// 选择 <c>anchor(...)</c> 读取的几何边。百分比边通过 <see cref="CssAnchorSideValue"/> 中的
/// <see cref="CssPercentage"/> 表示。
/// </summary>
[String]
public enum CssAnchorSide
{
    /// <summary>Uses the inner side of the named anchor. 使用命名锚点的内侧。</summary>
    [Description("@#inside")] Inside,
    /// <summary>Uses the outer side of the named anchor. 使用命名锚点的外侧。</summary>
    [Description("@#outside")] Outside,
    /// <summary>Uses the anchor's physical top side. 使用锚点的物理上边。</summary>
    [Description("@#top")] Top,
    /// <summary>Uses the anchor's physical right side. 使用锚点的物理右边。</summary>
    [Description("@#right")] Right,
    /// <summary>Uses the anchor's physical bottom side. 使用锚点的物理下边。</summary>
    [Description("@#bottom")] Bottom,
    /// <summary>Uses the anchor's physical left side. 使用锚点的物理左边。</summary>
    [Description("@#left")] Left,
    /// <summary>Uses the anchor's logical start side. 使用锚点的逻辑起始边。</summary>
    [Description("@#start")] Start,
    /// <summary>Uses the anchor's logical end side. 使用锚点的逻辑结束边。</summary>
    [Description("@#end")] End,
    /// <summary>Uses the anchor's own logical start side. 使用锚点自身的逻辑起始边。</summary>
    [Description("@#self-start")] SelfStart,
    /// <summary>Uses the anchor's own logical end side. 使用锚点自身的逻辑结束边。</summary>
    [Description("@#self-end")] SelfEnd,
    /// <summary>Uses the center of the named anchor. 使用命名锚点的中心。</summary>
    [Description("@#center")] Center
}

/// <summary>
/// Selects the dimension read by <c>anchor-size(...)</c>.
/// 选择 <c>anchor-size(...)</c> 读取的尺寸维度。
/// </summary>
[String]
public enum CssAnchorSizeAxis
{
    /// <summary>Reads the anchor's physical width. 读取锚点的物理宽度。</summary>
    [Description("@#width")] Width,
    /// <summary>Reads the anchor's physical height. 读取锚点的物理高度。</summary>
    [Description("@#height")] Height,
    /// <summary>Reads the anchor's block-axis size. 读取锚点的块轴尺寸。</summary>
    [Description("@#block")] Block,
    /// <summary>Reads the anchor's inline-axis size. 读取锚点的行内轴尺寸。</summary>
    [Description("@#inline")] Inline,
    /// <summary>Reads the anchor's own block-axis size. 读取锚点自身的块轴尺寸。</summary>
    [Description("@#self-block")] SelfBlock,
    /// <summary>Reads the anchor's own inline-axis size. 读取锚点自身的行内轴尺寸。</summary>
    [Description("@#self-inline")] SelfInline
}

/// <summary>
/// Provides the special <c>any</c> basis accepted by <c>calc-size(...)</c>.
/// 提供 <c>calc-size(...)</c> 接受的特殊 <c>any</c> 基值。
/// </summary>
[String]
public enum CssCalcSizeBasisKeyword
{
    /// <summary>Uses the special <c>any</c> basis accepted by <c>calc-size(...)</c>. 使用 <c>calc-size(...)</c> 接受的特殊 <c>any</c> 基值。</summary>
    [Description("@#any")] Any
}

/// <summary>
/// Specifies the non-anchor keyword unique to <c>position-anchor</c>.
/// 指定 <c>position-anchor</c> 特有的非锚点关键字。
/// </summary>
[String]
public enum CssPositionAnchorKeyword
{
    /// <summary>Uses the containing block's position anchor where the grammar permits it. 在语法允许时使用包含块的 position anchor。</summary>
    [Description("@#match-parent")] MatchParent
}

/// <summary>
/// Specifies the <c>all</c> keyword unique to <c>anchor-scope</c>.
/// 指定 <c>anchor-scope</c> 特有的 <c>all</c> 关键字。
/// </summary>
[String]
public enum CssAnchorScopeKeyword
{
    /// <summary>Makes all eligible descendant anchors visible to the scope. 让作用域内所有符合条件的后代锚点可见。</summary>
    [Description("@#all")] All
}

/// <summary>
/// Specifies the <c>content</c> keyword unique to <c>flex-basis</c>.
/// 指定 <c>flex-basis</c> 特有的 <c>content</c> 关键字。
/// </summary>
[String]
public enum CssFlexBasisKeyword
{
    /// <summary>Uses the flex item's content size as its flex basis. 使用弹性项目的内容尺寸作为 flex basis。</summary>
    [Description("@#content")] Content
}

/// <summary>
/// Lists display-mode keywords accepted by display-oriented property domains.
/// The enum avoids accepting similarly spelled values from unrelated CSS property families.
/// 列出 display 类属性值域接受的显示模式关键字。该 enum 避免接纳其他 CSS 属性族中拼写相同的值。
/// </summary>
[String]
public enum CssDisplayKeyword
{
    /// <summary>Creates a block-level box. 创建块级盒。</summary>
    [Description("@#block")] Block,
    /// <summary>Participates in inline layout. 参与行内布局。</summary>
    [Description("@#inline")] Inline,
    /// <summary>Creates an inline-level box with block-like inner layout. 创建具有块级内部布局的行内级盒。</summary>
    [Description("@#inline-block")] InlineBlock,
    /// <summary>Creates a flex container. 创建弹性布局容器。</summary>
    [Description("@#flex")] Flex,
    /// <summary>Creates an inline-level flex container. 创建行内级弹性布局容器。</summary>
    [Description("@#inline-flex")] InlineFlex,
    /// <summary>Creates a grid container. 创建网格布局容器。</summary>
    [Description("@#grid")] Grid,
    /// <summary>Creates an inline-level grid container. 创建行内级网格布局容器。</summary>
    [Description("@#inline-grid")] InlineGrid,
    /// <summary>Creates a block box with a new block formatting context. 创建具有新块格式化上下文的块盒。</summary>
    [Description("@#flow-root")] FlowRoot,
    /// <summary>Suppresses the principal box while preserving child layout. 抑制主盒，同时保留子元素布局。</summary>
    [Description("@#contents")] Contents,
    /// <summary>Uses table formatting. 使用表格格式化。</summary>
    [Description("@#table")] Table,
    /// <summary>Creates a list-item box and marker where applicable. 在适用时创建列表项盒和标记。</summary>
    [Description("@#list-item")] ListItem
}

/// <summary>
/// Lists positioning scheme keywords used by the <c>position</c> property.
/// It is independent from anchor-positioning values, which are represented by dedicated anchor carriers.
/// 列出 <c>position</c> 属性使用的定位方案关键字。它独立于锚点定位值，后者由专用 anchor 载体表示。
/// </summary>
[String]
public enum CssPositionKeyword
{
    /// <summary>Keeps the box in normal flow without offsets. 保持盒位于常规流中且不应用偏移。</summary>
    [Description("@#static")] Static,
    /// <summary>Keeps the box in normal flow while allowing visual offsets. 保持盒位于常规流中，同时允许视觉偏移。</summary>
    [Description("@#relative")] Relative,
    /// <summary>Positions the box out of normal flow relative to its containing block. 相对于包含块将盒脱离常规流定位。</summary>
    [Description("@#absolute")] Absolute,
    /// <summary>Fixes the box to the viewport or its fixed-position containing block. 将盒固定到视口或其固定定位包含块。</summary>
    [Description("@#fixed")] Fixed,
    /// <summary>Uses relative positioning until a scroll threshold, then sticks. 滚动到阈值前使用相对定位，随后吸附。</summary>
    [Description("@#sticky")] Sticky
}

/// <summary>
/// Lists overflow behavior keywords accepted by overflow property domains.
/// The automatic branch is represented by <see cref="CssAutoKeyword"/> rather than duplicated here.
/// 列出 overflow 属性值域接受的溢出行为关键字。自动分支由 <see cref="CssAutoKeyword"/> 表示，
/// 因此不会在这里重复定义。
/// </summary>
[String]
public enum CssOverflowKeyword
{
    /// <summary>Allows overflow to paint outside the box. 允许溢出内容绘制到盒外。</summary>
    [Description("@#visible")] Visible,
    /// <summary>Clips overflow while retaining supported programmatic scrolling. 裁剪溢出内容，同时保留支持的程序化滚动。</summary>
    [Description("@#hidden")] Hidden,
    /// <summary>Clips overflow without establishing a scroll container. 裁剪溢出内容且不建立滚动容器。</summary>
    [Description("@#clip")] Clip,
    /// <summary>Establishes a scroll container. 建立滚动容器。</summary>
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

    /// <summary>Starts or extends a border shorthand from a named border width. 使用具名边框宽度开始或扩展边框简写。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorderWidth left, ICssBorderPart right);

    /// <summary>Combines a named border width with a closed color keyword. 将具名边框宽度与封闭颜色关键字组合。</summary>
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

    /// <summary>Starts or extends a border shorthand from a border style token. 使用边框线型 token 开始或扩展边框简写。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssBorder operator |(CssBorderStyle left, ICssBorderPart right);

    /// <summary>Combines a border style token with a closed color keyword. 将边框线型 token 与封闭颜色关键字组合。</summary>
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

/// <summary>
/// Lists the closed color keywords that can safely participate in strongly typed color and border domains.
/// 任意命名颜色应使用 <c>color(...)</c>；此 enum 仅保留可安全参与强类型颜色和 border 值域的封闭关键字。
/// </summary>
[String]
public enum CssColorKeyword
{
    /// <summary>Represents a fully transparent color. 表示完全透明的颜色。</summary>
    [Description("@#transparent")] Transparent,
    /// <summary>Uses the element's computed <c>color</c> value. 使用元素计算后的 <c>color</c> 值。</summary>
    [Description("@#currentColor")] CurrentColor
}

/// <summary>
/// Lists alignment keywords shared by flex, grid, and alignment properties.
/// It intentionally remains separate from sizing <c>stretch</c> to preserve each property's grammar.
/// 列出 flex、grid 和对齐属性共享的对齐关键字。它刻意与尺寸的 <c>stretch</c> 分离，以保留各属性 grammar。
/// </summary>
[String]
public enum CssAlignmentKeyword
{
    /// <summary>Aligns to the logical start edge. 对齐到逻辑起始边。</summary>
    [Description("@#start")] Start,
    /// <summary>Aligns to the logical end edge. 对齐到逻辑结束边。</summary>
    [Description("@#end")] End,
    /// <summary>Centers on the alignment axis. 沿对齐轴居中。</summary>
    [Description("@#center")] Center,
    /// <summary>Aligns to the flex container's start edge. 对齐到弹性容器起始边。</summary>
    [Description("@#flex-start")] FlexStart,
    /// <summary>Aligns to the flex container's end edge. 对齐到弹性容器结束边。</summary>
    [Description("@#flex-end")] FlexEnd,
    /// <summary>Aligns to the item's own logical start edge. 对齐到项目自身逻辑起始边。</summary>
    [Description("@#self-start")] SelfStart,
    /// <summary>Aligns to the item's own logical end edge. 对齐到项目自身逻辑结束边。</summary>
    [Description("@#self-end")] SelfEnd,
    /// <summary>Aligns to the physical left edge where permitted. 在允许时对齐到物理左边。</summary>
    [Description("@#left")] Left,
    /// <summary>Aligns to the physical right edge where permitted. 在允许时对齐到物理右边。</summary>
    [Description("@#right")] Right,
    /// <summary>Stretches auto-sized items across the alignment axis. 沿对齐轴拉伸自动尺寸项目。</summary>
    [Description("@#stretch")] Stretch,
    /// <summary>Aligns participating items by baseline. 按参与项目的基线对齐。</summary>
    [Description("@#baseline")] Baseline,
    /// <summary>Places free space only between items. 仅在项目之间放置剩余空间。</summary>
    [Description("@#space-between")] SpaceBetween,
    /// <summary>Places free space around items with half-sized outer gaps. 在项目周围放置剩余空间，外侧间隙为内部的一半。</summary>
    [Description("@#space-around")] SpaceAround,
    /// <summary>Places equal free space between items and at both ends. 在项目之间和两端放置相等的剩余空间。</summary>
    [Description("@#space-evenly")] SpaceEvenly
}

/// <summary>Lists values for <c>flex-direction</c>。列出 <c>flex-direction</c> 的可用值。</summary>
[String]
public enum CssFlexDirectionKeyword
{
    /// <summary>Uses the inline axis as the flex main axis. 使用行内轴作为弹性主轴。</summary>
    [Description("@#row")] Row,
    /// <summary>Uses the reversed inline axis as the flex main axis. 使用反向行内轴作为弹性主轴。</summary>
    [Description("@#row-reverse")] RowReverse,
    /// <summary>Uses the block axis as the flex main axis. 使用块轴作为弹性主轴。</summary>
    [Description("@#column")] Column,
    /// <summary>Uses the reversed block axis as the flex main axis. 使用反向块轴作为弹性主轴。</summary>
    [Description("@#column-reverse")] ColumnReverse
}

/// <summary>Lists values for <c>flex-wrap</c>。列出 <c>flex-wrap</c> 的可用值。</summary>
[String]
public enum CssFlexWrapKeyword
{
    /// <summary>Keeps all flex items on a single line. 让所有弹性项目保持在单行。</summary>
    [Description("@#nowrap")] NoWrap,
    /// <summary>Allows flex items to wrap onto additional lines. 允许弹性项目换行到额外行。</summary>
    [Description("@#wrap")] Wrap,
    /// <summary>Wraps flex items with reversed cross-axis line order. 换行时反转交叉轴上的行顺序。</summary>
    [Description("@#wrap-reverse")] WrapReverse
}

/// <summary>Lists keyword branches for <c>background-size</c>。列出 <c>background-size</c> 的关键字分支。</summary>
[String]
public enum CssBackgroundSizeKeyword
{
    /// <summary>Scales the image to cover the background positioning area, allowing cropping. 缩放图像以覆盖背景定位区域，允许裁切。</summary>
    [Description("@#cover")] Cover,
    /// <summary>Scales the image to fit inside the background positioning area without cropping. 缩放图像以完整容纳在背景定位区域内。</summary>
    [Description("@#contain")] Contain
}

/// <summary>Lists box-sizing algorithms。列出 box-sizing 的计算模型。</summary>
[String]
public enum CssBoxSizingKeyword
{
    /// <summary>Includes padding and border in declared width and height. 声明的宽高包含内边距和边框。</summary>
    [Description("@#border-box")] BorderBox,
    /// <summary>Applies declared width and height to the content box only. 声明的宽高仅作用于内容盒。</summary>
    [Description("@#content-box")] ContentBox
}

/// <summary>Lists the curated cursor keyword domain。列出经过筛选的 cursor 关键字值域。</summary>
[String]
public enum CssCursorKeyword
{
    /// <summary>Uses the platform default cursor. 使用平台默认鼠标指针。</summary>
    [Description("@#default")] Default,
    /// <summary>Uses the pointer cursor for clickable targets. 为可点击目标使用指针样式。</summary>
    [Description("@#pointer")] Pointer,
    /// <summary>Uses the prohibited-action cursor. 使用禁止操作指针样式。</summary>
    [Description("@#not-allowed")] NotAllowed,
    /// <summary>Uses the text-selection cursor. 使用文本选择指针样式。</summary>
    [Description("@#text")] Text,
    /// <summary>Uses the move cursor for draggable content. 为可拖动内容使用移动指针样式。</summary>
    [Description("@#move")] Move,
    /// <summary>Uses the grab cursor before a drag begins. 在拖动开始前使用抓取指针样式。</summary>
    [Description("@#grab")] Grab,
    /// <summary>Uses the grabbing cursor during an active drag. 在正在拖动时使用抓住指针样式。</summary>
    [Description("@#grabbing")] Grabbing
}

/// <summary>Lists text-transform keyword values。列出 text-transform 的关键字值。</summary>
[String]
public enum CssTextTransformKeyword
{
    /// <summary>Capitalizes the first typographic letter of each word where supported. 在支持时大写每个词的首个排版字母。</summary>
    [Description("@#capitalize")] Capitalize,
    /// <summary>Converts text to uppercase. 将文本转换为大写。</summary>
    [Description("@#uppercase")] Uppercase,
    /// <summary>Converts text to lowercase. 将文本转换为小写。</summary>
    [Description("@#lowercase")] Lowercase,
    /// <summary>Transforms text to full-width forms where applicable. 在适用时将文本转换为全角形式。</summary>
    [Description("@#full-width")] FullWidth,
    /// <summary>Transforms kana text to full-size kana where applicable. 在适用时将假名转换为全尺寸假名。</summary>
    [Description("@#full-size-kana")] FullSizeKana
}

/// <summary>Lists white-space processing keyword values。列出 white-space 处理关键字值。</summary>
[String]
public enum CssWhiteSpaceKeyword
{
    /// <summary>Collapses whitespace and prevents automatic wrapping. 合并空白字符并禁止自动换行。</summary>
    [Description("@#nowrap")] NoWrap,
    /// <summary>Preserves whitespace and line breaks in preformatted text. 为预格式化文本保留空白和换行。</summary>
    [Description("@#pre")] Pre,
    /// <summary>Preserves whitespace while allowing automatic wrapping. 保留空白字符，同时允许自动换行。</summary>
    [Description("@#pre-wrap")] PreWrap,
    /// <summary>Collapses whitespace while preserving newline characters as line breaks. 合并空白字符，同时保留换行符作为换行。</summary>
    [Description("@#pre-line")] PreLine,
    /// <summary>Preserves sequences of spaces and allows wrapping at every preserved space. 保留连续空格，并允许在每个保留空格处换行。</summary>
    [Description("@#break-spaces")] BreakSpaces
}

/// <summary>Lists text-overflow marker keywords。列出 text-overflow 标记关键字。</summary>
[String]
public enum CssTextOverflowKeyword
{
    /// <summary>Clips overflowing inline text without an overflow marker. 裁剪溢出的行内文本且不显示溢出标记。</summary>
    [Description("@#clip")] Clip,
    /// <summary>Displays an ellipsis marker for clipped inline text. 为被裁剪的行内文本显示省略号标记。</summary>
    [Description("@#ellipsis")] Ellipsis
}

/// <summary>Represents the isolation keyword accepted by isolation properties。表示 isolation 属性接受的隔离关键字。</summary>
[String]
public enum CssIsolationKeyword
{
    /// <summary>Creates a new stacking context for the element. 为元素创建新的堆叠上下文。</summary>
    [Description("@#isolate")] Isolate
}

/// <summary>Lists color-scheme preference keywords。列出 color-scheme 偏好关键字。</summary>
[String]
public enum CssColorSchemeKeyword
{
    /// <summary>Declares support for a light color scheme. 声明支持浅色配色方案。</summary>
    [Description("@#light")] Light,
    /// <summary>Declares support for a dark color scheme. 声明支持深色配色方案。</summary>
    [Description("@#dark")] Dark,
    /// <summary>Declares support for both light and dark color schemes. 声明同时支持浅色和深色配色方案。</summary>
    [Description("@#light dark")] LightDark
}

/// <summary>Lists built-in animation timing keywords。列出内置 animation timing 关键字。</summary>
[String]
public enum CssTimingFunctionKeyword
{
    /// <summary>Uses a constant-rate timing curve. 使用恒定速率的时间曲线。</summary>
    [Description("@#linear")] Linear,
    /// <summary>Uses the default ease timing curve. 使用默认缓入缓出时间曲线。</summary>
    [Description("@#ease")] Ease,
    /// <summary>Uses an ease-in timing curve. 使用缓入时间曲线。</summary>
    [Description("@#ease-in")] EaseIn,
    /// <summary>Uses an ease-out timing curve. 使用缓出时间曲线。</summary>
    [Description("@#ease-out")] EaseOut,
    /// <summary>Uses an ease-in-out timing curve. 使用缓入缓出时间曲线。</summary>
    [Description("@#ease-in-out")] EaseInOut,
    /// <summary>Uses a step timing curve that jumps at the start of each interval. 使用每个区间起始处跳变的步进时间曲线。</summary>
    [Description("@#step-start")] StepStart,
    /// <summary>Uses a step timing curve that jumps at the end of each interval. 使用每个区间结束处跳变的步进时间曲线。</summary>
    [Description("@#step-end")] StepEnd
}

/// <summary>
/// The erased carrier used by explicit declarations and the dynamic declaration indexer.
/// It is a closed union of supported CSS token families; property setters normally expose narrower unions so
/// using this type directly is an intentional escape from property-specific checking.
/// 显式声明和动态声明索引器使用的擦除载体。它是受支持 CSS token 家族的封闭 union；属性 setter 通常
/// 暴露更窄的 union，因此直接使用该类型意味着有意绕开属性特定检查。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssValue(
    double,
    CssRaw,
    CssVariable,
    CssLength,
    CssPercentage,
    CssLengthPercentage,
    CssFitContent,
    CssAnchorName,
    CssAnchorNameList,
    CssAnchor,
    CssAnchorSize,
    CssCalcSize,
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
    CssPaddingPair,
    CssPaddingTriple,
    CssMargin,
    CssInset,
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
    CssSizingFunctionKeyword,
    CssPositionAnchorKeyword,
    CssAnchorScopeKeyword,
    CssFlexBasisKeyword,
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
    CssImportant<CssLengthPercentage>, CssImportant<CssFitContent>, CssImportant<CssAnchorName>,
    CssImportant<CssAnchorNameList>, CssImportant<CssAnchor>, CssImportant<CssAnchorSize>, CssImportant<CssCalcSize>,
    CssImportant<CssAngle>, CssImportant<CssTime>, CssImportant<CssFrequency>,
    CssImportant<CssResolution>, CssImportant<CssColor>, CssImportant<CssImage>, CssImportant<CssUrl>,
    CssImportant<CssString>, CssImportant<CssIdent>, CssImportant<CssKeyword>, CssImportant<CssTransform>,
    CssImportant<CssFilter>, CssImportant<CssBorder>, CssImportant<CssShadowList>, CssImportant<CssTrack>,
    CssImportant<CssPadding>, CssImportant<CssPaddingPair>, CssImportant<CssPaddingTriple>,
    CssImportant<CssMargin>, CssImportant<CssInset>, CssImportant<CssGap>, CssImportant<CssRadius>,
    CssImportant<CssFlex>, CssImportant<CssBackgroundSize>, CssImportant<CssGridLine>, CssImportant<CssGradient>,
    CssImportant<CssAnimation>, CssImportant<CssFontFamily>, CssImportant<CssRatio>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>,
    CssImportant<CssSizingKeyword>, CssImportant<CssSizingFunctionKeyword>, CssImportant<CssPositionAnchorKeyword>,
    CssImportant<CssAnchorScopeKeyword>, CssImportant<CssFlexBasisKeyword>, CssImportant<CssDisplayKeyword>, CssImportant<CssPositionKeyword>,
    CssImportant<CssOverflowKeyword>, CssImportant<CssBorderWidth>, CssImportant<CssBorderStyle>,
    CssImportant<CssColorKeyword>, CssImportant<CssAlignmentKeyword>, CssImportant<CssFlexDirectionKeyword>,
    CssImportant<CssFlexWrapKeyword>, CssImportant<CssBackgroundSizeKeyword>, CssImportant<CssBoxSizingKeyword>,
    CssImportant<CssCursorKeyword>, CssImportant<CssTextTransformKeyword>, CssImportant<CssWhiteSpaceKeyword>,
    CssImportant<CssTextOverflowKeyword>, CssImportant<CssIsolationKeyword>, CssImportant<CssColorSchemeKeyword>,
    CssImportant<CssTimingFunctionKeyword>);

/// <summary>
/// Accepts only keyword-like CSS values, custom-property references, raw escape values, and declaration priority.
/// It is used by properties whose grammar does not admit dimensional functions or structural shorthands.
/// 仅接受关键字类 CSS 值、自定义属性引用、raw 逃生值和声明优先级。它用于 grammar 不允许尺寸函数或结构化
/// 简写的属性。
/// </summary>
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

/// <summary>One side accepted by padding shorthands。padding 简写接受的单边值。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssPaddingPart(CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage);

/// <summary>One side accepted by margin shorthands, including anchor-size and auto。margin 简写接受的单边值，可含 anchor-size 和 auto。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssMarginPart(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssAnchorSize, CssAutoKeyword);

/// <summary>One row or column gap component。单个 row 或 column gap 分量。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssGapPart(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssNormalKeyword);

/// <summary>One corner component accepted by radius shorthands。radius 简写接受的单角分量。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssRadiusPart(CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage);

/// <summary>Value domain for padding properties, including the dedicated shorthand carrier。padding 属性值域，包含专用简写载体。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssPaddingValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssPadding, CssPaddingPair, CssPaddingTriple, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssPadding>, CssImportant<CssPaddingPair>,
    CssImportant<CssPaddingTriple>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for margin properties, including anchor-size where CSS permits it。margin 属性值域，在 CSS 允许处包含 anchor-size。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssMarginValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssAnchorSize, CssMargin, CssAutoKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssAnchorSize>, CssImportant<CssMargin>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for gap, row-gap, and column-gap properties。gap、row-gap 与 column-gap 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssGapValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssGap, CssNormalKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssGap>, CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for corner-radius properties and their shorthand。corner-radius 属性及其简写的值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssRadiusValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssRadius, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssRadius>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for the flex shorthand, preserving its structured carrier。flex 简写的值域，保留其结构化载体。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssFlexValue(
    double, CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssFlex, CssAutoKeyword,
    CssNoneKeyword, CssWideKeyword, CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssLength>, CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssFlex>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for grid line placement, including a line/range carrier。grid 线定位值域，包含线/区间专用载体。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssGridLineValue(
    int, CssRaw, CssVariable, CssIdent, CssGridLine, CssAutoKeyword, CssWideKeyword, CssImportant<int>,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssIdent>, CssImportant<CssGridLine>,
    CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for text alignment properties。文本对齐属性的值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssTextAlignValue(
    CssRaw, CssVariable, CssKeyword, CssAlignmentKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssKeyword>, CssImportant<CssAlignmentKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Length-only property domain; percentages and length-percentage arithmetic remain excluded。仅长度属性值域；百分比与 length-percentage 算术仍被排除。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssLengthValue(
    CssRaw, CssVariable, CssLength, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssSizingKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>,
    CssImportant<CssWideKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>,
    CssImportant<CssNormalKeyword>, CssImportant<CssSizingKeyword>);

/// <summary>Property domain that permits lengths, percentages, and their typed calc result。允许长度、百分比及其类型化 calc 结果的属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssLengthPercentageValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssSizingKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>,
    CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssSizingKeyword>);

/// <summary>
/// Accepts one side selector for <c>anchor(...)</c>. It intentionally permits only the standard
/// side keywords or a percentage, not arbitrary identifier text.
/// 接受 <c>anchor(...)</c> 的一个边选择器。它只允许标准边关键字或百分比，而不接受任意标识符文本。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssAnchorSideValue(CssAnchorSide, CssPercentage);

/// <summary>
/// Accepts a valid basis for <c>calc-size(...)</c>, including nested <c>calc-size()</c> and
/// intrinsic sizing keywords. It is not exposed as a normal declaration value domain.
/// 接受 <c>calc-size(...)</c> 的合法基值，包括嵌套 <c>calc-size()</c> 与内在尺寸关键字。
/// 它不会作为普通声明值域暴露。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssCalcSizeBasis(
    CssRaw, CssVariable, CssAutoKeyword, CssSizingKeyword, CssSizingFunctionKeyword, CssFitContent,
    CssCalcSize, CssCalcSizeBasisKeyword);

/// <summary>
/// Represents modern box-size values such as lengths, intrinsic keywords, <c>fit-content()</c>,
/// <c>calc-size()</c>, and <c>anchor-size()</c>. It deliberately excludes angles, border shorthands,
/// and generic CSS values.
/// 表示现代盒尺寸值，例如长度、内在关键字、<c>fit-content()</c>、<c>calc-size()</c> 和
/// <c>anchor-size()</c>。它刻意排除角度、border 简写和通用 CSS 值。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssSizingValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssFitContent, CssCalcSize, CssAnchorSize,
    CssAutoKeyword, CssNoneKeyword, CssSizingKeyword, CssSizingFunctionKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssFitContent>, CssImportant<CssCalcSize>, CssImportant<CssAnchorSize>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssSizingKeyword>,
    CssImportant<CssSizingFunctionKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents values accepted by <c>flex-basis</c>. It extends the box-size domain only with the
/// CSS-specific <c>content</c> keyword instead of accepting every keyword.
/// 表示 <c>flex-basis</c> 可接受的值。它只在盒尺寸值域基础上增加 CSS 专用 <c>content</c> 关键字，
/// 不会接受所有关键字。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssFlexBasisValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssFitContent, CssCalcSize, CssAnchorSize,
    CssAutoKeyword, CssNoneKeyword, CssSizingKeyword, CssSizingFunctionKeyword, CssFlexBasisKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssFitContent>, CssImportant<CssCalcSize>, CssImportant<CssAnchorSize>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssSizingKeyword>,
    CssImportant<CssSizingFunctionKeyword>, CssImportant<CssFlexBasisKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents the narrower column-width grammar: a length, intrinsic minimum/maximum keyword,
/// or <c>fit-content(...)</c>. Percentages remain rejected unless expressed by the function form.
/// 表示较窄的 column-width 语法：长度、内在最小/最大关键字或 <c>fit-content(...)</c>。
/// 除函数形式外，百分比仍会被拒绝。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssColumnWidthValue(
    CssRaw, CssVariable, CssLength, CssFitContent, CssAutoKeyword, CssSizingKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssFitContent>,
    CssImportant<CssAutoKeyword>, CssImportant<CssSizingKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents one anchor-aware inset side. The value may use a length/percentage, <c>anchor()</c>,
/// or <c>anchor-size()</c>, but does not admit box shorthands.
/// 表示一个支持锚点的 inset 边。该值可使用长度/百分比、<c>anchor()</c> 或
/// <c>anchor-size()</c>，但不接受盒模型简写。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssAnchorPositionValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssAnchor, CssAnchorSize, CssAutoKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssAnchor>, CssImportant<CssAnchorSize>,
    CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents one anchor-aware margin side. It admits <c>anchor-size()</c> but deliberately omits
/// <see cref="CssMargin"/>, so a four-value margin shorthand cannot be assigned to <c>margin-top</c>.
/// 表示一个支持锚点的 margin 边。它允许 <c>anchor-size()</c>，但刻意排除 <see cref="CssMargin"/>，
/// 因而四值 margin 简写不能赋给 <c>margin-top</c>。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssAnchorMarginValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssAnchorSize, CssAutoKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssAnchorSize>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents a one-to-four-side anchor-aware <c>inset</c> shorthand.
/// 表示一至四边的支持锚点 <c>inset</c> 简写。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssInsetPart(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssAnchor, CssAnchorSize, CssAutoKeyword);

/// <summary>
/// Represents values accepted by <c>inset</c>, <c>inset-block</c>, and <c>inset-inline</c>.
///  It preserves both a single side and the dedicated shorthand carrier.
/// 表示 <c>inset</c>、<c>inset-block</c> 和 <c>inset-inline</c> 可接受的值，同时保留单边值和
/// 专用简写载体。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssInsetValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssAnchor, CssAnchorSize, CssInset,
    CssAutoKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>,
    CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssAnchor>, CssImportant<CssAnchorSize>,
    CssImportant<CssInset>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents values accepted by <c>anchor-name</c>, including a single name or an explicit name list.
/// 表示 <c>anchor-name</c> 可接受的值，包括单个名称或显式名称列表。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssAnchorNameValue(
    CssRaw, CssVariable, CssAnchorName, CssAnchorNameList, CssNoneKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssAnchorName>, CssImportant<CssAnchorNameList>,
    CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents values accepted by <c>anchor-scope</c>, including the dedicated <c>all</c> keyword.
/// 表示 <c>anchor-scope</c> 可接受的值，包括专用 <c>all</c> 关键字。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssAnchorScopeValue(
    CssRaw, CssVariable, CssAnchorName, CssAnchorNameList, CssNoneKeyword, CssAnchorScopeKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssAnchorName>, CssImportant<CssAnchorNameList>,
    CssImportant<CssNoneKeyword>, CssImportant<CssAnchorScopeKeyword>, CssImportant<CssWideKeyword>);

/// <summary>
/// Represents values accepted by <c>position-anchor</c>, whose named-anchor and keyword branches
/// have a narrower contract than generic position or identifier values.
/// 表示 <c>position-anchor</c> 可接受的值；其命名锚点和关键字分支比通用 position 或标识符值具有
/// 更窄的契约。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union CssPositionAnchorValue(
    CssRaw, CssVariable, CssAnchorName, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword, CssPositionAnchorKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssAnchorName>, CssImportant<CssAutoKeyword>,
    CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssPositionAnchorKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Domain for properties that allow a number, length, or percentage. 数值、长度或百分比均可用的属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssLengthPercentageNumberValue(
    double, CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssSizingKeyword, CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssLength>, CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssSizingKeyword>);

/// <summary>Percentage-only property domain with CSS-wide and variable branches. 百分比专用属性值域，包含 CSS-wide 和变量分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssPercentageValue(
    CssRaw, CssVariable, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssPercentage>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Numeric property domain that accepts a scalar but not dimensional units. 只接受标量、不接受带单位值的数值属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssNumberValue(
    double, CssRaw, CssVariable, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Integer-only property domain, for example count-like CSS grammar branches. 整数专用属性值域，例如计数类 CSS grammar 分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssIntegerValue(
    int, CssRaw, CssVariable, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<int>, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Domain for properties that allow either a unitless number or percentage. 允许无单位数或百分比的属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssNumberPercentageValue(
    double, CssRaw, CssVariable, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssImportant<double>, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssPercentage>, CssImportant<CssWideKeyword>, CssImportant<CssAutoKeyword>,
    CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Angle-valued property domain, including variables and CSS-wide branches. 角度属性值域，包含变量与 CSS-wide 分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssAngleValue(
    CssRaw, CssVariable, CssAngle, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssAngle>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Time-valued property domain for animation and transition timing. animation 与 transition 时间属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssTimeValue(
    CssRaw, CssVariable, CssTime, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssTime>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Frequency-valued property domain. 频率属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssFrequencyValue(
    CssRaw, CssVariable, CssFrequency, CssPercentage, CssWideKeyword, CssAutoKeyword, CssNoneKeyword,
    CssNormalKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssFrequency>,
    CssImportant<CssPercentage>, CssImportant<CssWideKeyword>, CssImportant<CssAutoKeyword>,
    CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Resolution-valued property domain. 分辨率属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssResolutionValue(
    CssRaw, CssVariable, CssResolution, CssWideKeyword, CssAutoKeyword, CssNoneKeyword, CssNormalKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssResolution>, CssImportant<CssWideKeyword>,
    CssImportant<CssAutoKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssNormalKeyword>);

/// <summary>Color property domain, deliberately excluding image and identifier carriers. 颜色属性值域，刻意排除 image 和 identifier 载体。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssColorValue(
    CssRaw, CssVariable, CssColor, CssColorKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssColor>, CssImportant<CssColorKeyword>, CssImportant<CssWideKeyword>);

/// <summary>One length component inside a typed box-shadow descriptor。类型化 box-shadow 描述符中的长度分量。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssShadowLength(CssLength, CssVariable);

/// <summary>One color component inside a typed box-shadow descriptor。类型化 box-shadow 描述符中的颜色分量。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssShadowColor(CssColor, CssColorKeyword, CssVariable);

/// <summary>Value domain for box-shadow properties, including the dedicated shadow-list carrier。box-shadow 属性值域，包含专用 shadow-list 载体。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssBoxShadowValue(
    CssRaw, CssVariable, CssNoneKeyword, CssWideKeyword, CssShadowList, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>, CssImportant<CssShadowList>);

/// <summary>Value domain for image-taking properties such as backgrounds and masks. background、mask 等图像属性的值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssImageValue(
    CssRaw, CssVariable, CssImage, CssUrl, CssGradient, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssImage>, CssImportant<CssUrl>, CssImportant<CssGradient>,
    CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for quoted CSS strings and the keyword branches that permit them. 引号 CSS 字符串及其允许关键字分支的值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssStringValue(
    CssRaw, CssVariable, CssString, CssNoneKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssString>, CssImportant<CssNoneKeyword>,
    CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for transform properties, including none and the typed transform carrier。transform 属性值域，包含 none 与类型化 transform 载体。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssTransformValue(
    CssRaw, CssVariable, CssTransform, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssTransform>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for border and outline line width. border、outline 线宽值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssLineWidthValue(
    CssRaw, CssVariable, CssLength, CssBorderWidth, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssLength>, CssImportant<CssBorderWidth>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for border and outline line style. border、outline 线型值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssLineStyleValue(
    CssRaw, CssVariable, CssNoneKeyword, CssBorderStyle, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssNoneKeyword>, CssImportant<CssBorderStyle>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for display properties. display 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssDisplayValue(
    CssRaw, CssVariable, CssNoneKeyword, CssDisplayKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssNoneKeyword>, CssImportant<CssDisplayKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for the position property, separate from position-anchor. position 属性值域，与 position-anchor 分离。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssPositionValue(
    CssRaw, CssVariable, CssPositionKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssPositionKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for overflow properties. overflow 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssOverflowValue(
    CssRaw, CssVariable, CssAutoKeyword, CssOverflowKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssAutoKeyword>, CssImportant<CssOverflowKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for grid-template tracks and track-size positions. grid-template 轨道和 track-size 位置的值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssTrackValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssTrack, CssFitContent, CssAutoKeyword, CssSizingKeyword, CssWideKeyword,
    CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>, CssImportant<CssPercentage>,
    CssImportant<CssLengthPercentage>, CssImportant<CssTrack>, CssImportant<CssFitContent>, CssImportant<CssAutoKeyword>, CssImportant<CssSizingKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for aspect-ratio-like properties. aspect-ratio 类属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssRatioValue(
    CssRaw, CssVariable, CssRatio, CssAutoKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssRatio>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for alignment properties shared by layout modules. layout 模块共享的对齐属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssAlignmentValue(
    CssRaw, CssVariable, CssAlignmentKeyword, CssAutoKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssAlignmentKeyword>, CssImportant<CssAutoKeyword>,
    CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for flex-direction. flex-direction 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssFlexDirectionValue(
    CssRaw, CssVariable, CssFlexDirectionKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssFlexDirectionKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for flex-wrap. flex-wrap 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssFlexWrapValue(
    CssRaw, CssVariable, CssFlexWrapKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssFlexWrapKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for background-size, including its paired size carrier. background-size 属性值域，包含成对尺寸载体。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssBackgroundSizeValue(
    CssRaw, CssVariable, CssLength, CssPercentage, CssLengthPercentage, CssBackgroundSize, CssBackgroundSizeKeyword, CssAutoKeyword,
    CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>, CssImportant<CssLength>,
    CssImportant<CssPercentage>, CssImportant<CssLengthPercentage>, CssImportant<CssBackgroundSize>,
    CssImportant<CssBackgroundSizeKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for box-sizing. box-sizing 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssBoxSizingValue(
    CssRaw, CssVariable, CssBoxSizingKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssBoxSizingKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for cursor properties. cursor 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssCursorValue(
    CssRaw, CssVariable, CssCursorKeyword, CssAutoKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssCursorKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for text-transform properties. text-transform 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssTextTransformValue(
    CssRaw, CssVariable, CssTextTransformKeyword, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssTextTransformKeyword>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for white-space processing properties. white-space 处理属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssWhiteSpaceValue(
    CssRaw, CssVariable, CssWhiteSpaceKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssWhiteSpaceKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for text-overflow. text-overflow 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssTextOverflowValue(
    CssRaw, CssVariable, CssTextOverflowKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssTextOverflowKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for isolation. isolation 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssIsolationValue(
    CssRaw, CssVariable, CssIsolationKeyword, CssAutoKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssIsolationKeyword>, CssImportant<CssAutoKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for color-scheme. color-scheme 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssColorSchemeValue(
    CssRaw, CssVariable, CssColorSchemeKeyword, CssNormalKeyword, CssWideKeyword, CssImportant<CssRaw>,
    CssImportant<CssVariable>, CssImportant<CssColorSchemeKeyword>, CssImportant<CssNormalKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for filter and backdrop-filter. filter 与 backdrop-filter 属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssFilterValue(
    CssRaw, CssVariable, CssFilter, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssFilter>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);

/// <summary>Value domain for border shorthand properties. border 简写属性值域。</summary>
[ECMAScript]
[Description("@#")]
public readonly union CssBorderValue(
    CssRaw, CssVariable, CssBorder, CssNoneKeyword, CssWideKeyword, CssImportant<CssRaw>, CssImportant<CssVariable>,
    CssImportant<CssBorder>, CssImportant<CssNoneKeyword>, CssImportant<CssWideKeyword>);
