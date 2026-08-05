namespace ECMAScript.Style;

public static partial class css
{
    public static readonly CssWideKeyword inherit = CssWideKeyword.Inherit;
    public static readonly CssWideKeyword initial = CssWideKeyword.Initial;
    public static readonly CssWideKeyword unset = CssWideKeyword.Unset;
    public static readonly CssWideKeyword revert = CssWideKeyword.Revert;
    public static readonly CssWideKeyword revertLayer = CssWideKeyword.RevertLayer;
    public static readonly CssAutoKeyword auto = CssAutoKeyword.Auto;
    public static readonly CssNoneKeyword none = CssNoneKeyword.None;
    public static readonly CssNormalKeyword normal = CssNormalKeyword.Normal;
    public static readonly CssSizingKeyword minContent = CssSizingKeyword.MinContent;
    public static readonly CssSizingKeyword maxContent = CssSizingKeyword.MaxContent;
    public static readonly CssDisplayKeyword block = CssDisplayKeyword.Block;
    public static readonly CssDisplayKeyword inline = CssDisplayKeyword.Inline;
    public static readonly CssDisplayKeyword inlineBlock = CssDisplayKeyword.InlineBlock;
    public static readonly CssDisplayKeyword flex = CssDisplayKeyword.Flex;
    public static readonly CssDisplayKeyword inlineFlex = CssDisplayKeyword.InlineFlex;
    public static readonly CssDisplayKeyword grid = CssDisplayKeyword.Grid;
    public static readonly CssDisplayKeyword inlineGrid = CssDisplayKeyword.InlineGrid;
    public static readonly CssDisplayKeyword flowRoot = CssDisplayKeyword.FlowRoot;
    public static readonly CssDisplayKeyword contents = CssDisplayKeyword.Contents;
    public static readonly CssDisplayKeyword table = CssDisplayKeyword.Table;
    public static readonly CssDisplayKeyword listItem = CssDisplayKeyword.ListItem;
    public static readonly CssPositionKeyword staticPosition = CssPositionKeyword.Static;
    public static readonly CssPositionKeyword relative = CssPositionKeyword.Relative;
    public static readonly CssPositionKeyword absolute = CssPositionKeyword.Absolute;
    public static readonly CssPositionKeyword fixedPosition = CssPositionKeyword.Fixed;
    public static readonly CssPositionKeyword sticky = CssPositionKeyword.Sticky;
    public static readonly CssOverflowKeyword visible = CssOverflowKeyword.Visible;
    public static readonly CssOverflowKeyword hidden = CssOverflowKeyword.Hidden;
    public static readonly CssOverflowKeyword clip = CssOverflowKeyword.Clip;
    public static readonly CssOverflowKeyword scroll = CssOverflowKeyword.Scroll;
    public static readonly CssLineWidthKeyword thin = CssLineWidthKeyword.Thin;
    public static readonly CssLineWidthKeyword medium = CssLineWidthKeyword.Medium;
    public static readonly CssLineWidthKeyword thick = CssLineWidthKeyword.Thick;
    public static readonly CssLineStyleKeyword dotted = CssLineStyleKeyword.Dotted;
    public static readonly CssLineStyleKeyword dashed = CssLineStyleKeyword.Dashed;
    public static readonly CssLineStyleKeyword solid = CssLineStyleKeyword.Solid;
    public static readonly CssLineStyleKeyword doubleLine = CssLineStyleKeyword.Double;
    public static readonly CssLineStyleKeyword groove = CssLineStyleKeyword.Groove;
    public static readonly CssLineStyleKeyword ridge = CssLineStyleKeyword.Ridge;
    public static readonly CssLineStyleKeyword inset = CssLineStyleKeyword.Inset;
    public static readonly CssLineStyleKeyword outset = CssLineStyleKeyword.Outset;
    public static readonly CssColorKeyword transparent = CssColorKeyword.Transparent;
    public static readonly CssColorKeyword currentColor = CssColorKeyword.CurrentColor;

    public static CssRaw raw(string value)
        => CssRaw.create(value);

    public static CssKeyword keyword(string value)
    {
        var normalized = value.Trim();
        validateToken(normalized, "CSS keyword", allowCustomPropertyPrefix: false);
        return CssKeyword.create(normalized);
    }

    public static CssIdent ident(string value)
    {
        var normalized = value.Trim();
        validateToken(normalized, "CSS identifier", allowCustomPropertyPrefix: true);
        return CssIdent.create(normalized);
    }

    [ECMAScriptName("variable")]
    public static CssVariable var(string name)
    {
        var normalized = normalizeVariableName(name);
        return CssVariable.create("var(" + normalized + ")");
    }

    public static CssVariable varOr(string name, CssValue fallback)
    {
        var normalized = normalizeVariableName(name);
        return CssVariable.create("var(" + normalized + "," + StringFn(fallback.Value) + ")");
    }

    public static CssLength px(double value) => CssLength.create(number(value) + "px");
    public static CssLength rem(double value) => CssLength.create(number(value) + "rem");
    public static CssLength em(double value) => CssLength.create(number(value) + "em");
    public static CssLength ex(double value) => CssLength.create(number(value) + "ex");
    public static CssLength ch(double value) => CssLength.create(number(value) + "ch");
    public static CssLength cap(double value) => CssLength.create(number(value) + "cap");
    public static CssLength ic(double value) => CssLength.create(number(value) + "ic");
    public static CssLength lh(double value) => CssLength.create(number(value) + "lh");
    public static CssLength rlh(double value) => CssLength.create(number(value) + "rlh");
    public static CssLength vw(double value) => CssLength.create(number(value) + "vw");
    public static CssLength vh(double value) => CssLength.create(number(value) + "vh");
    public static CssLength vmin(double value) => CssLength.create(number(value) + "vmin");
    public static CssLength vmax(double value) => CssLength.create(number(value) + "vmax");
    public static CssLength svw(double value) => CssLength.create(number(value) + "svw");
    public static CssLength svh(double value) => CssLength.create(number(value) + "svh");
    public static CssLength lvw(double value) => CssLength.create(number(value) + "lvw");
    public static CssLength lvh(double value) => CssLength.create(number(value) + "lvh");
    public static CssLength dvw(double value) => CssLength.create(number(value) + "dvw");
    public static CssLength dvh(double value) => CssLength.create(number(value) + "dvh");
    public static CssLength cm(double value) => CssLength.create(number(value) + "cm");
    public static CssLength mm(double value) => CssLength.create(number(value) + "mm");
    public static CssLength q(double value) => CssLength.create(number(value) + "Q");
    public static CssLength inch(double value) => CssLength.create(number(value) + "in");
    public static CssLength pt(double value) => CssLength.create(number(value) + "pt");
    public static CssLength pc(double value) => CssLength.create(number(value) + "pc");
    public static CssPercentage percent(double value) => CssPercentage.create(number(value) + "%");

    public static CssLength min(CssLength first, CssLength second)
        => CssLength.create("min(" + StringFn(first) + "," + StringFn(second) + ")");

    public static CssLength max(CssLength first, CssLength second)
        => CssLength.create("max(" + StringFn(first) + "," + StringFn(second) + ")");

    public static CssLength clamp(CssLength minimum, CssLength preferred, CssLength maximum)
        => CssLength.create("clamp(" + StringFn(minimum) + "," + StringFn(preferred) + "," + StringFn(maximum) + ")");

    public static CssAngle deg(double value) => CssAngle.create(number(value) + "deg");
    public static CssAngle grad(double value) => CssAngle.create(number(value) + "grad");
    public static CssAngle rad(double value) => CssAngle.create(number(value) + "rad");
    public static CssAngle turn(double value) => CssAngle.create(number(value) + "turn");
    public static CssTime ms(double value) => CssTime.create(number(value) + "ms");
    public static CssTime seconds(double value) => CssTime.create(number(value) + "s");
    public static CssFrequency hz(double value) => CssFrequency.create(number(value) + "Hz");
    public static CssFrequency khz(double value) => CssFrequency.create(number(value) + "kHz");
    public static CssResolution dpi(double value) => CssResolution.create(number(value) + "dpi");
    public static CssResolution dpcm(double value) => CssResolution.create(number(value) + "dpcm");
    public static CssResolution dppx(double value) => CssResolution.create(number(value) + "dppx");

    public static CssColor rgb(int red, int green, int blue)
    {
        validateByte(red, "red");
        validateByte(green, "green");
        validateByte(blue, "blue");
        return CssColor.create("rgb(" + StringFn(red) + " " + StringFn(green) + " " + StringFn(blue) + ")");
    }

    public static CssColor color(string value)
    {
        var normalized = value.Trim();
        validateToken(normalized, "CSS color keyword", allowCustomPropertyPrefix: false);
        return CssColor.create(normalized);
    }

    public static CssColor rgba(int red, int green, int blue, double alpha)
    {
        validateByte(red, "red");
        validateByte(green, "green");
        validateByte(blue, "blue");
        validateUnitInterval(alpha, "alpha");
        return CssColor.create("rgb(" + StringFn(red) + " " + StringFn(green) + " " + StringFn(blue) + " / " + number(alpha) + ")");
    }

    public static CssColor hsl(double hue, double saturation, double lightness)
    {
        validatePercentage(saturation, "saturation");
        validatePercentage(lightness, "lightness");
        return CssColor.create("hsl(" + number(hue) + " " + number(saturation) + "% " + number(lightness) + "%)");
    }

    public static CssColor hsla(double hue, double saturation, double lightness, double alpha)
    {
        validatePercentage(saturation, "saturation");
        validatePercentage(lightness, "lightness");
        validateUnitInterval(alpha, "alpha");
        return CssColor.create("hsl(" + number(hue) + " " + number(saturation) + "% " + number(lightness) + "% / " + number(alpha) + ")");
    }

    public static CssColor hex(string value)
    {
        var trimmed = value.Trim();
        var normalized = trimmed.StartsWith("#") ? trimmed : "#" + trimmed;
        var digits = normalized.Substring(1);
        if (digits.Length != 3 && digits.Length != 4 && digits.Length != 6 && digits.Length != 8)
            Fail("CSS hex color must contain 3, 4, 6, or 8 hexadecimal digits.");

        for (var index = 0; index < digits.Length; index++)
        {
            var code = (int)digits.CharCodeAt(index);
            var isDigit = code >= 48 && code <= 57;
            var isLower = code >= 97 && code <= 102;
            var isUpper = code >= 65 && code <= 70;
            if (!isDigit && !isLower && !isUpper)
                Fail("CSS hex color contains a non-hexadecimal character.");
        }

        return CssColor.create(normalized.ToLowerInvariant());
    }

    public static CssUrl url(string value)
        => CssUrl.create("url(" + quote(value) + ")");

    public static CssString str(string value)
        => CssString.create(quote(value));

    public static CssTrack fr(double value)
        => CssTrack.create(number(value) + "fr");

    public static CssTrack minMax(CssTrackValue minimum, CssTrackValue maximum)
        => CssTrack.create("minmax(" + StringFn(minimum.Value) + "," + StringFn(maximum.Value) + ")");

    public static CssTrack fitContent(CssLengthPercentageValue limit)
        => CssTrack.create("fit-content(" + StringFn(limit.Value) + ")");

    public static CssTrack repeat(int count, CssTrackValue track)
    {
        if (count <= 0)
            Fail("CSS repeat count must be greater than zero.");
        return CssTrack.create("repeat(" + StringFn(count) + "," + StringFn(track.Value) + ")");
    }

    public static CssTransform translateX(CssLengthPercentageValue value)
        => CssTransform.create("translateX(" + StringFn(value.Value) + ")");

    public static CssTransform translateY(CssLengthPercentageValue value)
        => CssTransform.create("translateY(" + StringFn(value.Value) + ")");

    public static CssTransform translate(CssLengthPercentageValue x, CssLengthPercentageValue y)
        => CssTransform.create("translate(" + StringFn(x.Value) + "," + StringFn(y.Value) + ")");

    public static CssTransform rotate(CssAngle angle)
        => CssTransform.create("rotate(" + StringFn(angle) + ")");

    public static CssTransform scale(double value)
        => CssTransform.create("scale(" + number(value) + ")");

    [ECMAScriptName("scale2")]
    public static CssTransform scale(double x, double y)
        => CssTransform.create("scale(" + number(x) + "," + number(y) + ")");

    public static CssTransform transform([PreserveParamsArray] params CssTransform[] functions)
    {
        if (functions.Length == 0)
            Fail("CSS transform requires at least one function.");

        var output = new Array<string>();
        foreach (var item in functions)
            output.Push(StringFn(item));
        return CssTransform.create(output.Join(" "));
    }

    public static CssShadowList shadows([PreserveParamsArray] params CssShadow[] values)
    {
        if (values.Length == 0)
            Fail("CSS box-shadow requires at least one shadow.");

        var output = new Array<string>();
        foreach (var shadow in values)
        {
            // CSS accepts the optional tokens in several orders. Emit one stable order so
            // hashes remain deterministic while the C# record stays easy to scan.
            // CSS 可选 token 的顺序较自由；这里固定输出顺序以保证 hash 稳定，同时保持 record 易读。
            var parts = new Array<string>();
            if (shadow.Inset)
                parts.Push("inset");

            parts.Push(StringFn(shadow.OffsetX.Value));
            parts.Push(StringFn(shadow.OffsetY.Value));

            var blur = shadow.Blur;
            if (blur is not null)
                parts.Push(StringFn(blur.Value.Value));

            var spread = shadow.Spread;
            if (spread is not null)
                parts.Push(StringFn(spread.Value.Value));

            var colorValue = shadow.Color;
            if (colorValue is not null)
                parts.Push(StringFn(colorValue.Value.Value));

            output.Push(parts.Join(" "));
        }

        return CssShadowList.create(output.Join(","));
    }

    public static CssRatio ratio(int numerator, int denominator = 1)
    {
        if (numerator <= 0 || denominator <= 0)
            Fail("CSS ratio terms must be greater than zero.");
        return CssRatio.create(StringFn(numerator) + " / " + StringFn(denominator));
    }

    private static string number(double value)
    {
        if (!IsFinite(value))
            Fail("CSS numeric values must be finite.");
        return StringFn(value);
    }

    private static void validateByte(int value, string channel)
    {
        if (value < 0 || value > 255)
            Fail("CSS RGB " + channel + " channel must be between 0 and 255.");
    }

    private static void validateUnitInterval(double value, string name)
    {
        if (!IsFinite(value) || value < 0 || value > 1)
            Fail("CSS " + name + " must be between 0 and 1.");
    }

    private static void validatePercentage(double value, string name)
    {
        if (!IsFinite(value) || value < 0 || value > 100)
            Fail("CSS " + name + " must be between 0 and 100.");
    }

    private static string normalizeVariableName(string value)
    {
        var normalized = value.Trim();
        if (!normalized.StartsWith("--"))
            Fail("CSS variable names must start with '--'.");
        validateToken(normalized, "CSS variable name", allowCustomPropertyPrefix: true);
        return normalized;
    }

    private static void validateToken(string value, string label, bool allowCustomPropertyPrefix)
    {
        if (value.Length == 0)
            Fail(label + " cannot be empty.");
        if (!allowCustomPropertyPrefix && value.StartsWith("--"))
            Fail(label + " cannot use a custom-property name.");

        var firstCode = (int)value.CharCodeAt(0);
        var startsWithDigit = firstCode >= 48 && firstCode <= 57;
        var startsWithHyphenDigit = value.StartsWith("-") &&
            (value.Length == 1 || (int)value.CharCodeAt(1) >= 48 && (int)value.CharCodeAt(1) <= 57);
        if (startsWithDigit || startsWithHyphenDigit)
            Fail(label + " must start with a letter, underscore, or non-numeric hyphen sequence.");

        for (var index = 0; index < value.Length; index++)
        {
            var character = value.Substring(index, 1);
            var code = (int)value.CharCodeAt(index);
            var isAsciiLetter = code >= 65 && code <= 90 || code >= 97 && code <= 122;
            var isDigit = code >= 48 && code <= 57;
            if (!isAsciiLetter && !isDigit && character != "-" && character != "_")
                Fail(label + " contains an unsupported character. Use raw(...) for escaped or future syntax.");
        }
    }

    private static string quote(string value)
        => "\"" + value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\d " )
            .Replace("\n", "\\a " ) + "\"";

}
