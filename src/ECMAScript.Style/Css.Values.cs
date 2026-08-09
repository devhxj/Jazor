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
    public static readonly CssBorderWidth thin = CssBorderWidth.create("thin");
    public static readonly CssBorderWidth medium = CssBorderWidth.create("medium");
    public static readonly CssBorderWidth thick = CssBorderWidth.create("thick");
    public static readonly CssBorderStyle dotted = CssBorderStyle.create("dotted");
    public static readonly CssBorderStyle dashed = CssBorderStyle.create("dashed");
    public static readonly CssBorderStyle solid = CssBorderStyle.create("solid");
    public static readonly CssBorderStyle doubleLine = CssBorderStyle.create("double");
    public static readonly CssBorderStyle groove = CssBorderStyle.create("groove");
    public static readonly CssBorderStyle ridge = CssBorderStyle.create("ridge");
    public static readonly CssBorderStyle inset = CssBorderStyle.create("inset");
    public static readonly CssBorderStyle outset = CssBorderStyle.create("outset");
    public static readonly CssColorKeyword transparent = CssColorKeyword.Transparent;
    public static readonly CssColorKeyword currentColor = CssColorKeyword.CurrentColor;
    public static readonly CssAlignmentKeyword start = CssAlignmentKeyword.Start;
    public static readonly CssAlignmentKeyword end = CssAlignmentKeyword.End;
    public static readonly CssAlignmentKeyword center = CssAlignmentKeyword.Center;
    public static readonly CssAlignmentKeyword flexStart = CssAlignmentKeyword.FlexStart;
    public static readonly CssAlignmentKeyword flexEnd = CssAlignmentKeyword.FlexEnd;
    public static readonly CssAlignmentKeyword selfStart = CssAlignmentKeyword.SelfStart;
    public static readonly CssAlignmentKeyword selfEnd = CssAlignmentKeyword.SelfEnd;
    public static readonly CssAlignmentKeyword left = CssAlignmentKeyword.Left;
    public static readonly CssAlignmentKeyword right = CssAlignmentKeyword.Right;
    public static readonly CssAlignmentKeyword stretch = CssAlignmentKeyword.Stretch;
    public static readonly CssAlignmentKeyword baseline = CssAlignmentKeyword.Baseline;
    public static readonly CssAlignmentKeyword spaceBetween = CssAlignmentKeyword.SpaceBetween;
    public static readonly CssAlignmentKeyword spaceAround = CssAlignmentKeyword.SpaceAround;
    public static readonly CssAlignmentKeyword spaceEvenly = CssAlignmentKeyword.SpaceEvenly;
    public static readonly CssFlexDirectionKeyword row = CssFlexDirectionKeyword.Row;
    public static readonly CssFlexDirectionKeyword rowReverse = CssFlexDirectionKeyword.RowReverse;
    public static readonly CssFlexDirectionKeyword column = CssFlexDirectionKeyword.Column;
    public static readonly CssFlexDirectionKeyword columnReverse = CssFlexDirectionKeyword.ColumnReverse;
    public static readonly CssFlexWrapKeyword noWrap = CssFlexWrapKeyword.NoWrap;
    public static readonly CssFlexWrapKeyword wrap = CssFlexWrapKeyword.Wrap;
    public static readonly CssFlexWrapKeyword wrapReverse = CssFlexWrapKeyword.WrapReverse;
    public static readonly CssBackgroundSizeKeyword cover = CssBackgroundSizeKeyword.Cover;
    public static readonly CssBackgroundSizeKeyword contain = CssBackgroundSizeKeyword.Contain;
    public static readonly CssBoxSizingKeyword borderBox = CssBoxSizingKeyword.BorderBox;
    public static readonly CssBoxSizingKeyword contentBox = CssBoxSizingKeyword.ContentBox;
    public static readonly CssCursorKeyword defaultCursor = CssCursorKeyword.Default;
    public static readonly CssCursorKeyword pointer = CssCursorKeyword.Pointer;
    public static readonly CssCursorKeyword notAllowed = CssCursorKeyword.NotAllowed;
    public static readonly CssCursorKeyword textCursor = CssCursorKeyword.Text;
    public static readonly CssTextTransformKeyword capitalize = CssTextTransformKeyword.Capitalize;
    public static readonly CssTextTransformKeyword uppercase = CssTextTransformKeyword.Uppercase;
    public static readonly CssTextTransformKeyword lowercase = CssTextTransformKeyword.Lowercase;
    public static readonly CssWhiteSpaceKeyword nowrap = CssWhiteSpaceKeyword.NoWrap;
    public static readonly CssWhiteSpaceKeyword pre = CssWhiteSpaceKeyword.Pre;
    public static readonly CssWhiteSpaceKeyword preWrap = CssWhiteSpaceKeyword.PreWrap;
    public static readonly CssWhiteSpaceKeyword preLine = CssWhiteSpaceKeyword.PreLine;
    public static readonly CssTextOverflowKeyword ellipsis = CssTextOverflowKeyword.Ellipsis;
    public static readonly CssIsolationKeyword isolate = CssIsolationKeyword.Isolate;
    public static readonly CssColorSchemeKeyword light = CssColorSchemeKeyword.Light;
    public static readonly CssColorSchemeKeyword dark = CssColorSchemeKeyword.Dark;
    public static readonly CssTimingFunctionKeyword linear = CssTimingFunctionKeyword.Linear;
    public static readonly CssTimingFunctionKeyword ease = CssTimingFunctionKeyword.Ease;
    public static readonly CssTimingFunctionKeyword easeIn = CssTimingFunctionKeyword.EaseIn;
    public static readonly CssTimingFunctionKeyword easeOut = CssTimingFunctionKeyword.EaseOut;
    public static readonly CssTimingFunctionKeyword easeInOut = CssTimingFunctionKeyword.EaseInOut;

    public static CssRaw raw(string value)
        => CssRaw.create(value);

    public static CssDeclaration declaration(string name, CssValue value)
        => new(name, value);

    public static CssDeclaration important(string name, CssValue value)
        => new(name, value, CssDeclarationPriority.Important);

    /// <summary>Marks one strongly typed property value as important.</summary>
    [ECMAScriptName("importantValue")]
    public static CssImportant<TValue> important<TValue>(TValue value)
        => CssImportant<TValue>.create(value);

    [ECMAScriptName("importantFrom")]
    public static CssDeclaration important(ICssDeclaration value)
        => new(value.Name, value.Value, CssDeclarationPriority.Important);

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

    [ECMAScriptName("minLengthPercentage")]
    public static CssLengthPercentage min(CssLengthPercentageValue first, CssLengthPercentageValue second)
        => CssLengthPercentage.create("min(" + StringFn(first.Value) + "," + StringFn(second.Value) + ")");

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

    public static CssTrack tracks([PreserveParamsArray] params CssTrackValue[] values)
    {
        if (values.Length == 0)
            Fail("CSS grid track list requires at least one track.");

        var output = new Array<string>();
        foreach (var value in values)
            output.Push(StringFn(value.Value));
        return CssTrack.create(output.Join(" "));
    }

    public static CssPadding padding(CssPaddingPart value)
        => CssPadding.create(StringFn(value.Value));

    [ECMAScriptName("padding2")]
    public static CssPadding padding(CssPaddingPart vertical, CssPaddingPart horizontal)
        => CssPadding.create(join(StringFn(vertical.Value), StringFn(horizontal.Value)));

    [ECMAScriptName("padding3")]
    public static CssPadding padding(CssPaddingPart top, CssPaddingPart horizontal, CssPaddingPart bottom)
        => CssPadding.create(join(StringFn(top.Value), StringFn(horizontal.Value), StringFn(bottom.Value)));

    [ECMAScriptName("padding4")]
    public static CssPadding padding(CssPaddingPart top, CssPaddingPart right, CssPaddingPart bottom, CssPaddingPart left)
        => CssPadding.create(join(StringFn(top.Value), StringFn(right.Value), StringFn(bottom.Value), StringFn(left.Value)));

    public static CssMargin margin(CssMarginPart value)
        => CssMargin.create(StringFn(value.Value));

    [ECMAScriptName("margin2")]
    public static CssMargin margin(CssMarginPart vertical, CssMarginPart horizontal)
        => CssMargin.create(join(StringFn(vertical.Value), StringFn(horizontal.Value)));

    [ECMAScriptName("margin3")]
    public static CssMargin margin(CssMarginPart top, CssMarginPart horizontal, CssMarginPart bottom)
        => CssMargin.create(join(StringFn(top.Value), StringFn(horizontal.Value), StringFn(bottom.Value)));

    [ECMAScriptName("margin4")]
    public static CssMargin margin(CssMarginPart top, CssMarginPart right, CssMarginPart bottom, CssMarginPart left)
        => CssMargin.create(join(StringFn(top.Value), StringFn(right.Value), StringFn(bottom.Value), StringFn(left.Value)));

    public static CssGap gap(CssGapPart value)
        => CssGap.create(StringFn(value.Value));

    [ECMAScriptName("gap2")]
    public static CssGap gap(CssGapPart row, CssGapPart column)
        => CssGap.create(join(StringFn(row.Value), StringFn(column.Value)));

    public static CssRadius radius(CssRadiusPart value)
        => CssRadius.create(StringFn(value.Value));

    [ECMAScriptName("radius2")]
    public static CssRadius radius(CssRadiusPart vertical, CssRadiusPart horizontal)
        => CssRadius.create(join(StringFn(vertical.Value), StringFn(horizontal.Value)));

    [ECMAScriptName("radius4")]
    public static CssRadius radius(CssRadiusPart topLeft, CssRadiusPart topRight, CssRadiusPart bottomRight, CssRadiusPart bottomLeft)
        => CssRadius.create(join(StringFn(topLeft.Value), StringFn(topRight.Value), StringFn(bottomRight.Value), StringFn(bottomLeft.Value)));

    public static CssFlex flexBox(double grow, double shrink, CssLengthPercentageValue basis)
        => CssFlex.create(number(grow) + " " + number(shrink) + " " + StringFn(basis.Value));

    public static CssBackgroundSize backgroundSize(CssLengthPercentageValue width, CssLengthPercentageValue height)
        => CssBackgroundSize.create(join(StringFn(width.Value), StringFn(height.Value)));

    public static CssGridLine gridLine(int line)
        => CssGridLine.create(StringFn(line));

    [ECMAScriptName("gridLine2")]
    public static CssGridLine gridLine(int start, int end)
        => CssGridLine.create(StringFn(start) + " / " + StringFn(end));

    public static CssGradientStop stop(CssColorValue color)
        => new(color);

    [ECMAScriptName("stopAt")]
    public static CssGradientStop stop(CssColorValue color, CssLengthPercentageValue at)
        => new(color, at);

    [ECMAScriptName("stopRange")]
    public static CssGradientStop stop(CssColorValue color, CssLengthPercentageValue from, CssLengthPercentageValue to)
        => new(color, from, to);

    public static CssGradient linearGradient([PreserveParamsArray] params CssGradientStop[] stops)
        => gradient("linear-gradient", stops);

    public static CssGradient conicGradient([PreserveParamsArray] params CssGradientStop[] stops)
        => gradient("conic-gradient", stops);

    public static CssAnimation animation(CssIdent name, CssTime duration, CssTimingFunctionKeyword timing)
        => CssAnimation.create(StringFn(name) + " " + StringFn(duration) + " " + StringFn(timing));

    public static CssFontFamilyName font(string value)
        => CssFontFamilyName.create(quote(value));

    public static CssFontFamilyName genericFont(string value)
        => CssFontFamilyName.create(StringFn(ident(value)));

    public static CssFontFamily fontFamily([PreserveParamsArray] params CssFontFamilyName[] names)
    {
        if (names.Length == 0)
            Fail("CSS font-family requires at least one family.");

        var output = new Array<string>();
        foreach (var name in names)
            output.Push(StringFn(name));
        return CssFontFamily.create(output.Join(","));
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

    public static CssBorder border(
        CssLineWidthValue? width = null,
        CssLineStyleValue? style = null,
        CssColorValue? color = null)
    {
        if (width is null && style is null && color is null)
            Fail("CSS border requires a width, style, or color.");

        var parts = new Array<string>();
        if (width is not null)
            parts.Push(StringFn(width.Value.Value));
        if (style is not null)
            parts.Push(StringFn(style.Value.Value));
        if (color is not null)
            parts.Push(StringFn(color.Value.Value));
        return CssBorder.create(parts.Join(" "));
    }

    public static CssFilter blur(CssLength value)
        => CssFilter.create("blur(" + StringFn(value) + ")");

    public static CssFilter grayscale(double amount)
    {
        validateUnitInterval(amount, "grayscale amount");
        return CssFilter.create("grayscale(" + number(amount) + ")");
    }

    public static CssFilter saturate(double amount)
    {
        if (!IsFinite(amount) || amount < 0)
            Fail("CSS saturate amount must be non-negative and finite.");
        return CssFilter.create("saturate(" + number(amount) + ")");
    }

    public static CssFilter filters([PreserveParamsArray] params CssFilter[] values)
    {
        if (values.Length == 0)
            Fail("CSS filter requires at least one function.");

        var output = new Array<string>();
        foreach (var value in values)
            output.Push(StringFn(value));
        return CssFilter.create(output.Join(" "));
    }

    private static CssGradient gradient(string name, CssGradientStop[] stops)
    {
        if (stops.Length < 2)
            Fail("CSS gradient requires at least two stops.");

        var output = new Array<string>();
        foreach (var stop in stops)
        {
            var value = StringFn(stop.Color.Value);
            if (stop.From is not null)
                value += " " + StringFn(stop.From.Value.Value);
            if (stop.To is not null)
                value += " " + StringFn(stop.To.Value.Value);
            output.Push(value);
        }

        return CssGradient.create(name + "(" + output.Join(",") + ")");
    }

    private static string join([PreserveParamsArray] params string[] values)
    {
        var output = new Array<string>();
        foreach (var value in values)
            output.Push(value);
        return output.Join(" ");
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
