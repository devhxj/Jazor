namespace ECMAScript.Style;

public static partial class css
{
    public static readonly CssWideKeyword inherit = CssWideKeyword.Inherit;
    public static readonly CssWideKeyword initial = CssWideKeyword.Initial;
    public static readonly CssWideKeyword unset = CssWideKeyword.Unset;
    public static readonly CssWideKeyword revert = CssWideKeyword.Revert;
    [ECMAScriptName("revertLayer")]
    public static readonly CssWideKeyword revert_layer = CssWideKeyword.RevertLayer;
    public static readonly CssAutoKeyword auto = CssAutoKeyword.Auto;
    public static readonly CssNoneKeyword none = CssNoneKeyword.None;
    public static readonly CssNormalKeyword normal = CssNormalKeyword.Normal;
    [ECMAScriptName("minContent")]
    public static readonly CssSizingKeyword min_content = CssSizingKeyword.MinContent;
    [ECMAScriptName("maxContent")]
    public static readonly CssSizingKeyword max_content = CssSizingKeyword.MaxContent;
    /// <summary>Intrinsic sizing keyword <c>fit-content</c> without parentheses。内在尺寸关键字 <c>fit-content</c>（无括号形式）。</summary>
    [ECMAScriptName("fitContentKeyword")]
    public static readonly CssSizingFunctionKeyword fit_content_keyword = CssSizingFunctionKeyword.FitContent;
    /// <summary>Intrinsic sizing keyword <c>stretch</c>, distinct from alignment <c>stretch</c>。内在尺寸关键字 <c>stretch</c>，与 alignment 的同名值分离。</summary>
    [ECMAScriptName("sizingStretch")]
    public static readonly CssSizingFunctionKeyword sizing_stretch = CssSizingFunctionKeyword.Stretch;
    /// <summary>Intrinsic sizing keyword <c>contain</c>, distinct from object-fit <c>contain</c>。内在尺寸关键字 <c>contain</c>，与 object-fit 的同名值分离。</summary>
    [ECMAScriptName("sizingContain")]
    public static readonly CssSizingFunctionKeyword sizing_contain = CssSizingFunctionKeyword.Contain;
    /// <summary>Special <c>any</c> basis for <c>calc-size(...)</c>。<c>calc-size(...)</c> 的特殊 <c>any</c> 基值。</summary>
    [ECMAScriptName("anySize")]
    public static readonly CssCalcSizeBasisKeyword any_size = CssCalcSizeBasisKeyword.Any;
    /// <summary>Result-side <c>size</c> token for <c>calc-size(...)</c> arithmetic。用于 <c>calc-size(...)</c> 算术的结果侧 <c>size</c> token。</summary>
    public static readonly CssCalcSizeExpression size = CssCalcSizeExpression.create("size");
    /// <summary>Named-anchor side <c>inside</c>。命名锚点边 <c>inside</c>。</summary>
    [ECMAScriptName("anchorInside")]
    public static readonly CssAnchorSide anchor_inside = CssAnchorSide.Inside;
    /// <summary>Named-anchor side <c>outside</c>。命名锚点边 <c>outside</c>。</summary>
    [ECMAScriptName("anchorOutside")]
    public static readonly CssAnchorSide anchor_outside = CssAnchorSide.Outside;
    /// <summary>Named-anchor side <c>top</c>。命名锚点边 <c>top</c>。</summary>
    [ECMAScriptName("anchorTop")]
    public static readonly CssAnchorSide anchor_top = CssAnchorSide.Top;
    /// <summary>Named-anchor side <c>right</c>。命名锚点边 <c>right</c>。</summary>
    [ECMAScriptName("anchorRight")]
    public static readonly CssAnchorSide anchor_right = CssAnchorSide.Right;
    /// <summary>Named-anchor side <c>bottom</c>。命名锚点边 <c>bottom</c>。</summary>
    [ECMAScriptName("anchorBottom")]
    public static readonly CssAnchorSide anchor_bottom = CssAnchorSide.Bottom;
    /// <summary>Named-anchor side <c>left</c>。命名锚点边 <c>left</c>。</summary>
    [ECMAScriptName("anchorLeft")]
    public static readonly CssAnchorSide anchor_left = CssAnchorSide.Left;
    /// <summary>Named-anchor side <c>start</c>。命名锚点边 <c>start</c>。</summary>
    [ECMAScriptName("anchorStart")]
    public static readonly CssAnchorSide anchor_start = CssAnchorSide.Start;
    /// <summary>Named-anchor side <c>end</c>。命名锚点边 <c>end</c>。</summary>
    [ECMAScriptName("anchorEnd")]
    public static readonly CssAnchorSide anchor_end = CssAnchorSide.End;
    /// <summary>Named-anchor side <c>self-start</c>。命名锚点边 <c>self-start</c>。</summary>
    [ECMAScriptName("anchorSelfStart")]
    public static readonly CssAnchorSide anchor_self_start = CssAnchorSide.SelfStart;
    /// <summary>Named-anchor side <c>self-end</c>。命名锚点边 <c>self-end</c>。</summary>
    [ECMAScriptName("anchorSelfEnd")]
    public static readonly CssAnchorSide anchor_self_end = CssAnchorSide.SelfEnd;
    /// <summary>Named-anchor side <c>center</c>。命名锚点边 <c>center</c>。</summary>
    [ECMAScriptName("anchorCenter")]
    public static readonly CssAnchorSide anchor_center = CssAnchorSide.Center;
    /// <summary>Anchor-size axis <c>width</c>。anchor-size 维度 <c>width</c>。</summary>
    [ECMAScriptName("anchorWidth")]
    public static readonly CssAnchorSizeAxis anchor_width = CssAnchorSizeAxis.Width;
    /// <summary>Anchor-size axis <c>height</c>。anchor-size 维度 <c>height</c>。</summary>
    [ECMAScriptName("anchorHeight")]
    public static readonly CssAnchorSizeAxis anchor_height = CssAnchorSizeAxis.Height;
    /// <summary>Anchor-size axis <c>block</c>。anchor-size 维度 <c>block</c>。</summary>
    [ECMAScriptName("anchorBlock")]
    public static readonly CssAnchorSizeAxis anchor_block = CssAnchorSizeAxis.Block;
    /// <summary>Anchor-size axis <c>inline</c>。anchor-size 维度 <c>inline</c>。</summary>
    [ECMAScriptName("anchorInline")]
    public static readonly CssAnchorSizeAxis anchor_inline = CssAnchorSizeAxis.Inline;
    /// <summary>Anchor-size axis <c>self-block</c>。anchor-size 维度 <c>self-block</c>。</summary>
    [ECMAScriptName("anchorSelfBlock")]
    public static readonly CssAnchorSizeAxis anchor_self_block = CssAnchorSizeAxis.SelfBlock;
    /// <summary>Anchor-size axis <c>self-inline</c>。anchor-size 维度 <c>self-inline</c>。</summary>
    [ECMAScriptName("anchorSelfInline")]
    public static readonly CssAnchorSizeAxis anchor_self_inline = CssAnchorSizeAxis.SelfInline;
    /// <summary>Keyword <c>all</c> for <c>anchor-scope</c>。<c>anchor-scope</c> 的 <c>all</c> 关键字。</summary>
    [ECMAScriptName("anchorScopeAll")]
    public static readonly CssAnchorScopeKeyword anchor_scope_all = CssAnchorScopeKeyword.All;
    /// <summary>Keyword <c>match-parent</c> for <c>position-anchor</c>。<c>position-anchor</c> 的 <c>match-parent</c> 关键字。</summary>
    [ECMAScriptName("anchorMatchParent")]
    public static readonly CssPositionAnchorKeyword anchor_match_parent = CssPositionAnchorKeyword.MatchParent;
    /// <summary>Keyword <c>content</c> for <c>flex-basis</c>。<c>flex-basis</c> 的 <c>content</c> 关键字。</summary>
    [ECMAScriptName("flexContent")]
    public static readonly CssFlexBasisKeyword flex_content = CssFlexBasisKeyword.Content;
    public static readonly CssDisplayKeyword block = CssDisplayKeyword.Block;
    public static readonly CssDisplayKeyword inline = CssDisplayKeyword.Inline;
    [ECMAScriptName("inlineBlock")]
    public static readonly CssDisplayKeyword inline_block = CssDisplayKeyword.InlineBlock;
    public static readonly CssDisplayKeyword flex = CssDisplayKeyword.Flex;
    [ECMAScriptName("inlineFlex")]
    public static readonly CssDisplayKeyword inline_flex = CssDisplayKeyword.InlineFlex;
    public static readonly CssDisplayKeyword grid = CssDisplayKeyword.Grid;
    [ECMAScriptName("inlineGrid")]
    public static readonly CssDisplayKeyword inline_grid = CssDisplayKeyword.InlineGrid;
    [ECMAScriptName("flowRoot")]
    public static readonly CssDisplayKeyword flow_root = CssDisplayKeyword.FlowRoot;
    public static readonly CssDisplayKeyword contents = CssDisplayKeyword.Contents;
    public static readonly CssDisplayKeyword table = CssDisplayKeyword.Table;
    [ECMAScriptName("listItem")]
    public static readonly CssDisplayKeyword list_item = CssDisplayKeyword.ListItem;
    [ECMAScriptName("staticPosition")]
    public static readonly CssPositionKeyword static_position = CssPositionKeyword.Static;
    public static readonly CssPositionKeyword relative = CssPositionKeyword.Relative;
    public static readonly CssPositionKeyword absolute = CssPositionKeyword.Absolute;
    [ECMAScriptName("fixedPosition")]
    public static readonly CssPositionKeyword fixed_position = CssPositionKeyword.Fixed;
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
    [ECMAScriptName("doubleLine")]
    public static readonly CssBorderStyle double_line = CssBorderStyle.create("double");
    public static readonly CssBorderStyle groove = CssBorderStyle.create("groove");
    public static readonly CssBorderStyle ridge = CssBorderStyle.create("ridge");
    public static readonly CssBorderStyle inset = CssBorderStyle.create("inset");
    public static readonly CssBorderStyle outset = CssBorderStyle.create("outset");
    public static readonly CssColorKeyword transparent = CssColorKeyword.Transparent;
    [ECMAScriptName("currentColor")]
    public static readonly CssColorKeyword current_color = CssColorKeyword.CurrentColor;
    public static readonly CssAlignmentKeyword start = CssAlignmentKeyword.Start;
    public static readonly CssAlignmentKeyword end = CssAlignmentKeyword.End;
    public static readonly CssAlignmentKeyword center = CssAlignmentKeyword.Center;
    [ECMAScriptName("flexStart")]
    public static readonly CssAlignmentKeyword flex_start = CssAlignmentKeyword.FlexStart;
    [ECMAScriptName("flexEnd")]
    public static readonly CssAlignmentKeyword flex_end = CssAlignmentKeyword.FlexEnd;
    [ECMAScriptName("selfStart")]
    public static readonly CssAlignmentKeyword self_start = CssAlignmentKeyword.SelfStart;
    [ECMAScriptName("selfEnd")]
    public static readonly CssAlignmentKeyword self_end = CssAlignmentKeyword.SelfEnd;
    public static readonly CssAlignmentKeyword left = CssAlignmentKeyword.Left;
    public static readonly CssAlignmentKeyword right = CssAlignmentKeyword.Right;
    public static readonly CssAlignmentKeyword stretch = CssAlignmentKeyword.Stretch;
    public static readonly CssAlignmentKeyword baseline = CssAlignmentKeyword.Baseline;
    [ECMAScriptName("spaceBetween")]
    public static readonly CssAlignmentKeyword space_between = CssAlignmentKeyword.SpaceBetween;
    [ECMAScriptName("spaceAround")]
    public static readonly CssAlignmentKeyword space_around = CssAlignmentKeyword.SpaceAround;
    [ECMAScriptName("spaceEvenly")]
    public static readonly CssAlignmentKeyword space_evenly = CssAlignmentKeyword.SpaceEvenly;
    public static readonly CssFlexDirectionKeyword row = CssFlexDirectionKeyword.Row;
    [ECMAScriptName("rowReverse")]
    public static readonly CssFlexDirectionKeyword row_reverse = CssFlexDirectionKeyword.RowReverse;
    public static readonly CssFlexDirectionKeyword column = CssFlexDirectionKeyword.Column;
    [ECMAScriptName("columnReverse")]
    public static readonly CssFlexDirectionKeyword column_reverse = CssFlexDirectionKeyword.ColumnReverse;
    [ECMAScriptName("noWrap")]
    public static readonly CssFlexWrapKeyword no_wrap = CssFlexWrapKeyword.NoWrap;
    public static readonly CssFlexWrapKeyword wrap = CssFlexWrapKeyword.Wrap;
    [ECMAScriptName("wrapReverse")]
    public static readonly CssFlexWrapKeyword wrap_reverse = CssFlexWrapKeyword.WrapReverse;
    public static readonly CssBackgroundSizeKeyword cover = CssBackgroundSizeKeyword.Cover;
    public static readonly CssBackgroundSizeKeyword contain = CssBackgroundSizeKeyword.Contain;
    [ECMAScriptName("borderBox")]
    public static readonly CssBoxSizingKeyword border_box = CssBoxSizingKeyword.BorderBox;
    [ECMAScriptName("contentBox")]
    public static readonly CssBoxSizingKeyword content_box = CssBoxSizingKeyword.ContentBox;
    [ECMAScriptName("defaultCursor")]
    public static readonly CssCursorKeyword default_cursor = CssCursorKeyword.Default;
    public static readonly CssCursorKeyword pointer = CssCursorKeyword.Pointer;
    [ECMAScriptName("notAllowed")]
    public static readonly CssCursorKeyword not_allowed = CssCursorKeyword.NotAllowed;
    [ECMAScriptName("textCursor")]
    public static readonly CssCursorKeyword text_cursor = CssCursorKeyword.Text;
    public static readonly CssTextTransformKeyword capitalize = CssTextTransformKeyword.Capitalize;
    public static readonly CssTextTransformKeyword uppercase = CssTextTransformKeyword.Uppercase;
    public static readonly CssTextTransformKeyword lowercase = CssTextTransformKeyword.Lowercase;
    public static readonly CssWhiteSpaceKeyword nowrap = CssWhiteSpaceKeyword.NoWrap;
    public static readonly CssWhiteSpaceKeyword pre = CssWhiteSpaceKeyword.Pre;
    [ECMAScriptName("preWrap")]
    public static readonly CssWhiteSpaceKeyword pre_wrap = CssWhiteSpaceKeyword.PreWrap;
    [ECMAScriptName("preLine")]
    public static readonly CssWhiteSpaceKeyword pre_line = CssWhiteSpaceKeyword.PreLine;
    public static readonly CssTextOverflowKeyword ellipsis = CssTextOverflowKeyword.Ellipsis;
    public static readonly CssIsolationKeyword isolate = CssIsolationKeyword.Isolate;
    public static readonly CssColorSchemeKeyword light = CssColorSchemeKeyword.Light;
    public static readonly CssColorSchemeKeyword dark = CssColorSchemeKeyword.Dark;
    public static readonly CssTimingFunctionKeyword linear = CssTimingFunctionKeyword.Linear;
    public static readonly CssTimingFunctionKeyword ease = CssTimingFunctionKeyword.Ease;
    [ECMAScriptName("easeIn")]
    public static readonly CssTimingFunctionKeyword ease_in = CssTimingFunctionKeyword.EaseIn;
    [ECMAScriptName("easeOut")]
    public static readonly CssTimingFunctionKeyword ease_out = CssTimingFunctionKeyword.EaseOut;
    [ECMAScriptName("easeInOut")]
    public static readonly CssTimingFunctionKeyword ease_in_out = CssTimingFunctionKeyword.EaseInOut;

    /// <summary>
    /// Preserves an explicitly supplied CSS fragment as a <see cref="CssRaw"/> value.
    /// Use it only for grammar that has no typed carrier yet; typed factories remain preferable because they
    /// preserve property-domain checking.
    /// 将显式提供的 CSS 片段保留为 <see cref="CssRaw"/> 值。仅在尚无类型化载体的 grammar 中使用；
    /// 应优先使用类型化工厂，以保留属性值域检查。
    /// </summary>
    public static CssRaw raw(string value)
        => CssRaw.create(value);

    /// <summary>
    /// Creates one explicitly ordered declaration for <see cref="CssDeclarations.additional"/>.
    /// The value must still be a closed <see cref="CssValue"/> branch; this API does not accept raw strings.
    /// 为 <see cref="CssDeclarations.additional"/> 创建一条显式排序的声明。值仍必须是封闭的
    /// <see cref="CssValue"/> 分支；该 API 不接受原始字符串。
    /// </summary>
    public static CssDeclaration declaration(string name, CssValue value)
        => new(name, value);

    /// <summary>
    /// Creates an explicitly ordered declaration with <c>!important</c> priority.
    /// For normal typed properties, prefer <c>important(value)</c> so the original value domain remains exact.
    /// 创建带 <c>!important</c> 优先级的显式排序声明。对于普通类型化属性，应优先使用
    /// <c>important(value)</c>，以保持原始值域精确。
    /// </summary>
    public static CssDeclaration important(string name, CssValue value)
        => new(name, value, CssDeclarationPriority.Important);

    /// <summary>
    /// Marks one strongly typed property value as <c>!important</c> without widening its value domain.
    /// 将一个强类型属性值标记为 <c>!important</c>，同时不扩大其值域。
    /// </summary>
    [ECMAScriptName("importantValue")]
    public static CssImportant<TValue> important<TValue>(TValue value)
        => CssImportant<TValue>.create(value);

    /// <summary>
    /// Copies an explicit declaration and upgrades only its priority to <c>!important</c>.
    /// 复制一条显式声明，并且只将其优先级升级为 <c>!important</c>。
    /// </summary>
    [ECMAScriptName("importantFrom")]
    public static CssDeclaration important(ICssDeclaration value)
        => new(value.Name, value.Value, CssDeclarationPriority.Important);

    /// <summary>
    /// Validates a CSS keyword token that cannot use a custom-property prefix.
    /// Prefer a closed keyword enum when the target property has a modeled vocabulary.
    /// 验证不能使用自定义属性前缀的 CSS keyword token。目标属性已有建模词汇时，应优先使用封闭 keyword enum。
    /// </summary>
    public static CssKeyword keyword(string value)
    {
        var normalized = value.Trim();
        validateToken(normalized, "CSS keyword", allowCustomPropertyPrefix: false);
        return CssKeyword.create(normalized);
    }

    /// <summary>
    /// Validates a CSS identifier, including a dashed custom-property-style identifier where the grammar permits it.
    /// 验证 CSS 标识符；在 grammar 允许时可使用带短横线的自定义属性风格标识符。
    /// </summary>
    public static CssIdent ident(string value)
    {
        var normalized = value.Trim();
        validateToken(normalized, "CSS identifier", allowCustomPropertyPrefix: true);
        return CssIdent.create(normalized);
    }

    /// <summary>
    /// Creates a typed CSS custom-property reference in <c>var(--name)</c> form.
    /// 创建 <c>var(--name)</c> 形式的类型化 CSS 自定义属性引用。
    /// </summary>
    [ECMAScriptName("variable")]
    public static CssVariable var(string name)
    {
        var normalized = normalizeVariableName(name);
        return CssVariable.create("var(" + normalized + ")");
    }

    /// <summary>
    /// Creates a typed custom-property reference with a typed CSS fallback.
    /// Fallback serialization keeps its original union branch rather than accepting an arbitrary string.
    /// 创建带类型化 CSS 后备值的自定义属性引用。后备值序列化会保持原始 union 分支，而非接受任意字符串。
    /// </summary>
    [ECMAScriptName("varOr")]
    public static CssVariable var_or(string name, CssValue fallback)
    {
        var normalized = normalizeVariableName(name);
        return CssVariable.create("var(" + normalized + "," + StringFn(fallback.Value) + ")");
    }

    /// <summary>
    /// Creates one CSS named-anchor identifier. The name must use the <c>--name</c> form required
    /// by the anchor-positioning grammar; this is separate from an arbitrary <see cref="CssIdent"/>.
    /// 创建一个 CSS 命名锚点标识符。名称必须符合锚点定位语法要求的 <c>--name</c> 形式；它与任意
    /// <see cref="CssIdent"/> 分离。
    /// </summary>
    [ECMAScriptName("anchorName")]
    public static CssAnchorName anchor_name(string value)
        => CssAnchorName.create(normalizeAnchorName(value));

    /// <summary>
    /// Creates a space-separated list for <c>anchor-name</c> or <c>anchor-scope</c>. At least one
    /// already validated <see cref="CssAnchorName"/> is required.
    /// 创建用于 <c>anchor-name</c> 或 <c>anchor-scope</c> 的空格分隔列表。至少需要一个已验证的
    /// <see cref="CssAnchorName"/>。
    /// </summary>
    [ECMAScriptName("anchorNames")]
    public static CssAnchorNameList anchor_names([Preserve] params CssAnchorName[] values)
    {
        if (values.Length == 0)
            Fail("CSS anchor-name requires at least one anchor name.");

        var output = new Array<string>();
        foreach (var value in values)
            output.Push(StringFn(value));
        return CssAnchorNameList.create(output.Join(" "));
    }

    /// <summary>
    /// References the requested side of the implicit anchor with <c>anchor(...)</c>.
    /// 使用 <c>anchor(...)</c> 引用隐式锚点的指定边。
    /// </summary>
    public static CssAnchor anchor(CssAnchorSideValue side)
        => CssAnchor.create("anchor(" + StringFn(side.Value) + ")");

    /// <summary>
    /// References a specified named anchor side with <c>anchor(...)</c>.
    /// 使用 <c>anchor(...)</c> 引用指定命名锚点的一条边。
    /// </summary>
    [ECMAScriptName("anchorNamed")]
    public static CssAnchor anchor(CssAnchorName name, CssAnchorSideValue side)
        => CssAnchor.create("anchor(" + StringFn(name) + " " + StringFn(side.Value) + ")");

    /// <summary>
    /// References an implicit anchor side and supplies the CSS fallback used when no anchor resolves.
    /// 引用隐式锚点边，并提供无法解析锚点时使用的 CSS 后备值。
    /// </summary>
    [ECMAScriptName("anchorFallback")]
    public static CssAnchor anchor(CssAnchorSideValue side, CssLengthPercentageValue fallback)
        => CssAnchor.create("anchor(" + StringFn(side.Value) + "," + StringFn(fallback.Value) + ")");

    /// <summary>
    /// References a named anchor side and supplies its CSS fallback.
    /// 引用命名锚点边，并提供其 CSS 后备值。
    /// </summary>
    [ECMAScriptName("anchorNamedFallback")]
    public static CssAnchor anchor(CssAnchorName name, CssAnchorSideValue side, CssLengthPercentageValue fallback)
        => CssAnchor.create("anchor(" + StringFn(name) + " " + StringFn(side.Value) + "," + StringFn(fallback.Value) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression using the implicit anchor and its default axis.
    /// 使用隐式锚点及默认轴创建 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSize")]
    public static CssAnchorSize anchor_size()
        => CssAnchorSize.create("anchor-size()");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for one named anchor.
    /// 为一个命名锚点创建 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeNamed")]
    public static CssAnchorSize anchor_size(CssAnchorName name)
        => CssAnchorSize.create("anchor-size(" + StringFn(name) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for one axis of the implicit anchor.
    /// 为隐式锚点的一个轴创建 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeAxis")]
    public static CssAnchorSize anchor_size(CssAnchorSizeAxis axis)
        => CssAnchorSize.create("anchor-size(" + StringFn(axis) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for one axis of a named anchor.
    /// 为命名锚点的一个轴创建 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeNamedAxis")]
    public static CssAnchorSize anchor_size(CssAnchorName name, CssAnchorSizeAxis axis)
        => CssAnchorSize.create("anchor-size(" + StringFn(name) + " " + StringFn(axis) + ")");

    /// <summary>
    /// Creates <c>anchor-size(, fallback)</c> for the implicit anchor when its size cannot resolve.
    /// 在隐式锚点尺寸无法解析时创建 <c>anchor-size(, fallback)</c>。
    /// </summary>
    [ECMAScriptName("anchorSizeFallback")]
    public static CssAnchorSize anchor_size(CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(," + StringFn(fallback.Value) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for a named anchor with a fallback.
    /// 为命名锚点创建带后备值的 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeNamedFallback")]
    public static CssAnchorSize anchor_size(CssAnchorName name, CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(" + StringFn(name) + "," + StringFn(fallback.Value) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for an implicit anchor axis with a fallback.
    /// 为隐式锚点轴创建带后备值的 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeAxisFallback")]
    public static CssAnchorSize anchor_size(CssAnchorSizeAxis axis, CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(" + StringFn(axis) + "," + StringFn(fallback.Value) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for a named anchor axis with a fallback.
    /// 为命名锚点轴创建带后备值的 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeNamedAxisFallback")]
    public static CssAnchorSize anchor_size(CssAnchorName name, CssAnchorSizeAxis axis, CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(" + StringFn(name) + " " + StringFn(axis) + "," + StringFn(fallback.Value) + ")");

    /// <summary>
    /// Creates <c>calc-size(basis, calculation)</c> using the typed result-side expression rooted
    /// at <c>size</c>.
    /// 使用以 <c>size</c> 为根的类型化结果表达式创建 <c>calc-size(basis, calculation)</c>。
    /// </summary>
    [ECMAScriptName("calcSize")]
    public static CssCalcSize calc_size(CssCalcSizeBasis basis, CssCalcSizeExpression calculation)
        => CssCalcSize.create("calc-size(" + StringFn(basis.Value) + "," + StringFn(calculation) + ")");

    /// <summary>
    /// Creates <c>calc-size(basis, calculation)</c> when the calculation is already a typed
    /// length-percentage expression.
    /// 当计算式已经是类型化 length-percentage 表达式时，创建 <c>calc-size(basis, calculation)</c>。
    /// </summary>
    [ECMAScriptName("calcSizeValue")]
    public static CssCalcSize calc_size(CssCalcSizeBasis basis, CssLengthPercentageValue calculation)
        => CssCalcSize.create("calc-size(" + StringFn(basis.Value) + "," + StringFn(calculation.Value) + ")");

    /// <summary>
    /// Creates a pixel CSS length. Unit factories return <see cref="CssLength"/>, so they cannot accidentally
    /// occupy percentage-only or time-only properties.
    /// 创建像素 CSS 长度。单位工厂返回 <see cref="CssLength"/>，因此不能意外用于仅百分比或仅时间属性。
    /// </summary>
    public static CssLength px(double value) => CssLength.create(number(value) + "px");

    /// <summary>Creates a root-font-relative CSS length。创建相对于根字体大小的 CSS 长度。</summary>
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
    /// <summary>Creates a CSS percentage, distinct from a unitless number。创建 CSS 百分比，与无单位数值分离。</summary>
    public static CssPercentage percent(double value) => CssPercentage.create(number(value) + "%");

    /// <summary>Creates a length-only <c>min(...)</c> expression。创建仅长度的 <c>min(...)</c> 表达式。</summary>
    public static CssLength min(CssLength first, CssLength second)
        => CssLength.create("min(" + StringFn(first) + "," + StringFn(second) + ")");

    /// <summary>Creates a mixed length-percentage <c>min(...)</c> expression。创建混合长度/百分比 <c>min(...)</c> 表达式。</summary>
    [ECMAScriptName("minLengthPercentage")]
    public static CssLengthPercentage min(CssLengthPercentageValue first, CssLengthPercentageValue second)
        => CssLengthPercentage.create("min(" + StringFn(first.Value) + "," + StringFn(second.Value) + ")");

    /// <summary>Creates a length-only <c>max(...)</c> expression。创建仅长度的 <c>max(...)</c> 表达式。</summary>
    public static CssLength max(CssLength first, CssLength second)
        => CssLength.create("max(" + StringFn(first) + "," + StringFn(second) + ")");

    /// <summary>Creates a length-only <c>clamp(minimum, preferred, maximum)</c> expression。创建仅长度的 <c>clamp(minimum, preferred, maximum)</c> 表达式。</summary>
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

    /// <summary>
    /// Creates an opaque RGB color after validating each channel in the inclusive byte range.
    /// 创建不透明 RGB 颜色，并验证每个通道位于包含端点的字节范围内。
    /// </summary>
    public static CssColor rgb(int red, int green, int blue)
    {
        validateByte(red, "red");
        validateByte(green, "green");
        validateByte(blue, "blue");
        return CssColor.create("rgb(" + StringFn(red) + " " + StringFn(green) + " " + StringFn(blue) + ")");
    }

    /// <summary>
    /// Creates a validated named CSS color. Use <see cref="hex(string)"/>, <see cref="rgb(int, int, int)"/>,
    /// or <see cref="hsl(double, double, double)"/> when the color's construction should be explicit.
    /// 创建经过验证的 CSS 命名颜色。需要显式构造颜色时，应使用 <see cref="hex(string)"/>、
    /// <see cref="rgb(int, int, int)"/> 或 <see cref="hsl(double, double, double)"/>。
    /// </summary>
    public static CssColor color(string value)
    {
        var normalized = value.Trim();
        validateToken(normalized, "CSS color keyword", allowCustomPropertyPrefix: false);
        return CssColor.create(normalized);
    }

    /// <summary>Creates an RGB color with a validated 0..1 alpha channel。创建带经验证 0..1 alpha 通道的 RGB 颜色。</summary>
    public static CssColor rgba(int red, int green, int blue, double alpha)
    {
        validateByte(red, "red");
        validateByte(green, "green");
        validateByte(blue, "blue");
        validateUnitInterval(alpha, "alpha");
        return CssColor.create("rgb(" + StringFn(red) + " " + StringFn(green) + " " + StringFn(blue) + " / " + number(alpha) + ")");
    }

    /// <summary>Creates an HSL color with percentage saturation and lightness。创建 saturation、lightness 为百分比的 HSL 颜色。</summary>
    public static CssColor hsl(double hue, double saturation, double lightness)
    {
        validatePercentage(saturation, "saturation");
        validatePercentage(lightness, "lightness");
        return CssColor.create("hsl(" + number(hue) + " " + number(saturation) + "% " + number(lightness) + "%)");
    }

    /// <summary>Creates an HSL color with a validated alpha channel。创建带经验证 alpha 通道的 HSL 颜色。</summary>
    public static CssColor hsla(double hue, double saturation, double lightness, double alpha)
    {
        validatePercentage(saturation, "saturation");
        validatePercentage(lightness, "lightness");
        validateUnitInterval(alpha, "alpha");
        return CssColor.create("hsl(" + number(hue) + " " + number(saturation) + "% " + number(lightness) + "% / " + number(alpha) + ")");
    }

    /// <summary>
    /// Creates a normalized hexadecimal CSS color. The optional leading <c>#</c> is accepted; only 3, 4, 6, and 8
    /// hexadecimal digit forms are emitted.
    /// 创建规范化十六进制 CSS 颜色。可省略前导 <c>#</c>；仅输出 3、4、6、8 位十六进制形式。
    /// </summary>
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

    /// <summary>Creates a quoted CSS <c>url(...)</c> token。创建带引号的 CSS <c>url(...)</c> token。</summary>
    public static CssUrl url(string value)
        => CssUrl.create("url(" + quote(value) + ")");

    /// <summary>Creates a quoted CSS string token。创建带引号的 CSS 字符串 token。</summary>
    public static CssString str(string value)
        => CssString.create(quote(value));

    /// <summary>Creates a grid-only fractional track size。创建仅用于 grid 的分数轨道尺寸。</summary>
    public static CssTrack fr(double value)
        => CssTrack.create(number(value) + "fr");

    /// <summary>Creates a typed grid <c>minmax(...)</c> track function。创建类型化 grid <c>minmax(...)</c> 轨道函数。</summary>
    [ECMAScriptName("minMax")]
    public static CssTrack min_max(CssTrackValue minimum, CssTrackValue maximum)
        => CssTrack.create("minmax(" + StringFn(minimum.Value) + "," + StringFn(maximum.Value) + ")");

    /// <summary>
    /// Creates the functional <c>fit-content(...)</c> size. The returned carrier is accepted by
    /// grid tracks, box sizing, and column-width without becoming a general track value.
    /// 创建函数形式的 <c>fit-content(...)</c> 尺寸。返回的载体可用于 grid track、盒尺寸和
    /// column-width，同时不会退化为通用轨道值。
    /// </summary>
    [ECMAScriptName("fitContent")]
    public static CssFitContent fit_content(CssLengthPercentageValue limit)
        => CssFitContent.create("fit-content(" + StringFn(limit.Value) + ")");

    /// <summary>
    /// Creates a fixed-count grid <c>repeat(...)</c> function. The count is validated as positive before emission.
    /// 创建固定次数的 grid <c>repeat(...)</c> 函数。次数会在输出前验证为正数。
    /// </summary>
    public static CssTrack repeat(int count, CssTrackValue track)
    {
        if (count <= 0)
            Fail("CSS repeat count must be greater than zero.");
        return CssTrack.create("repeat(" + StringFn(count) + "," + StringFn(track.Value) + ")");
    }

    /// <summary>
    /// Joins one or more typed grid tracks in authored order.
    /// The result remains <see cref="CssTrack"/> rather than a generic string so it cannot leave grid-track properties.
    /// 按作者顺序连接一个或多个类型化 grid track。结果仍为 <see cref="CssTrack"/> 而不是通用字符串，
    /// 因而不能离开 grid-track 属性。
    /// </summary>
    public static CssTrack tracks([Preserve] params CssTrackValue[] values)
    {
        if (values.Length == 0)
            Fail("CSS grid track list requires at least one track.");

        var output = new Array<string>();
        foreach (var value in values)
            output.Push(StringFn(value.Value));
        return CssTrack.create(output.Join(" "));
    }

    /// <summary>
    /// Creates a CSS padding shorthand. Its overloads map to the standard one-, two-, three-, and four-side forms.
    /// 创建 CSS padding 简写。各重载分别映射标准的一、二、三、四边形式。
    /// </summary>
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

    /// <summary>
    /// Creates a CSS margin shorthand. Anchor-size values remain allowed only through <see cref="CssMarginPart"/>.
    /// 创建 CSS margin 简写。anchor-size 值只会通过 <see cref="CssMarginPart"/> 被允许。
    /// </summary>
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

    /// <summary>
    /// Creates a one-side <c>inset</c> shorthand. Use the overloads for two to four sides when the
    /// shorthand is assigned to <c>inset</c>, <c>inset-block</c>, or <c>inset-inline</c>.
    /// 创建单边 <c>inset</c> 简写。赋给 <c>inset</c>、<c>inset-block</c> 或 <c>inset-inline</c>
    /// 时，可使用二至四边重载。
    /// </summary>
    [ECMAScriptName("insetSides")]
    public static CssInset inset_sides(CssInsetPart value)
        => CssInset.create(StringFn(value.Value));

    /// <summary>Creates a two-side <c>inset</c> shorthand。创建双边 <c>inset</c> 简写。</summary>
    [ECMAScriptName("insetSides2")]
    public static CssInset inset_sides(CssInsetPart vertical, CssInsetPart horizontal)
        => CssInset.create(join(StringFn(vertical.Value), StringFn(horizontal.Value)));

    /// <summary>Creates a three-side <c>inset</c> shorthand。创建三值 <c>inset</c> 简写。</summary>
    [ECMAScriptName("insetSides3")]
    public static CssInset inset_sides(CssInsetPart top, CssInsetPart horizontal, CssInsetPart bottom)
        => CssInset.create(join(StringFn(top.Value), StringFn(horizontal.Value), StringFn(bottom.Value)));

    /// <summary>Creates a four-side <c>inset</c> shorthand。创建四边 <c>inset</c> 简写。</summary>
    [ECMAScriptName("insetSides4")]
    public static CssInset inset_sides(CssInsetPart top, CssInsetPart right, CssInsetPart bottom, CssInsetPart left)
        => CssInset.create(join(StringFn(top.Value), StringFn(right.Value), StringFn(bottom.Value), StringFn(left.Value)));

    /// <summary>Creates a one- or two-axis CSS gap shorthand。创建单轴或双轴 CSS gap 简写。</summary>
    public static CssGap gap(CssGapPart value)
        => CssGap.create(StringFn(value.Value));

    [ECMAScriptName("gap2")]
    public static CssGap gap(CssGapPart row, CssGapPart column)
        => CssGap.create(join(StringFn(row.Value), StringFn(column.Value)));

    /// <summary>Creates a one-, two-, or four-corner radius shorthand。创建一、二或四角 radius 简写。</summary>
    public static CssRadius radius(CssRadiusPart value)
        => CssRadius.create(StringFn(value.Value));

    [ECMAScriptName("radius2")]
    public static CssRadius radius(CssRadiusPart vertical, CssRadiusPart horizontal)
        => CssRadius.create(join(StringFn(vertical.Value), StringFn(horizontal.Value)));

    [ECMAScriptName("radius4")]
    public static CssRadius radius(CssRadiusPart topLeft, CssRadiusPart topRight, CssRadiusPart bottomRight, CssRadiusPart bottomLeft)
        => CssRadius.create(join(StringFn(topLeft.Value), StringFn(topRight.Value), StringFn(bottomRight.Value), StringFn(bottomLeft.Value)));

    /// <summary>Creates the structured <c>flex-grow flex-shrink flex-basis</c> shorthand。创建结构化 <c>flex-grow flex-shrink flex-basis</c> 简写。</summary>
    [ECMAScriptName("flexBox")]
    public static CssFlex flex_box(double grow, double shrink, CssLengthPercentageValue basis)
        => CssFlex.create(number(grow) + " " + number(shrink) + " " + StringFn(basis.Value));

    /// <summary>Creates a two-dimensional background-size value。创建二维 background-size 值。</summary>
    [ECMAScriptName("backgroundSize")]
    public static CssBackgroundSize background_size(CssLengthPercentageValue width, CssLengthPercentageValue height)
        => CssBackgroundSize.create(join(StringFn(width.Value), StringFn(height.Value)));

    /// <summary>Creates a grid line reference or start/end line range。创建 grid 线引用或 start/end 线区间。</summary>
    [ECMAScriptName("gridLine")]
    public static CssGridLine grid_line(int line)
        => CssGridLine.create(StringFn(line));

    [ECMAScriptName("gridLine2")]
    public static CssGridLine grid_line(int start, int end)
        => CssGridLine.create(StringFn(start) + " / " + StringFn(end));

    /// <summary>Creates a gradient stop, optionally with a point or range position。创建 gradient stop，可选单点或区间位置。</summary>
    public static CssGradientStop stop(CssColorValue color)
        => new(color);

    [ECMAScriptName("stopAt")]
    public static CssGradientStop stop(CssColorValue color, CssLengthPercentageValue at)
        => new(color, at);

    [ECMAScriptName("stopRange")]
    public static CssGradientStop stop(CssColorValue color, CssLengthPercentageValue from, CssLengthPercentageValue to)
        => new(color, from, to);

    /// <summary>Creates a linear gradient with at least two typed stops。创建至少含两个类型化 stop 的线性渐变。</summary>
    [ECMAScriptName("linearGradient")]
    public static CssGradient linear_gradient([Preserve] params CssGradientStop[] stops)
        => gradient("linear-gradient", stops);

    /// <summary>Creates a conic gradient with at least two typed stops。创建至少含两个类型化 stop 的圆锥渐变。</summary>
    [ECMAScriptName("conicGradient")]
    public static CssGradient conic_gradient([Preserve] params CssGradientStop[] stops)
        => gradient("conic-gradient", stops);

    /// <summary>Creates a compact animation shorthand from a typed name, duration, and timing keyword。根据类型化名称、duration、timing keyword 创建紧凑 animation 简写。</summary>
    public static CssAnimation animation(CssIdent name, CssTime duration, CssTimingFunctionKeyword timing)
        => CssAnimation.create(StringFn(name) + " " + StringFn(duration) + " " + StringFn(timing));

    /// <summary>Creates a quoted font-family entry。创建带引号的 font-family 条目。</summary>
    public static CssFontFamilyName font(string value)
        => CssFontFamilyName.create(quote(value));

    /// <summary>Creates a validated generic font-family identifier without quotes。创建不带引号、已验证的 generic font-family 标识符。</summary>
    [ECMAScriptName("genericFont")]
    public static CssFontFamilyName generic_font(string value)
        => CssFontFamilyName.create(StringFn(ident(value)));

    /// <summary>Creates an ordered font fallback list that contains at least one family。创建至少含一个字体族的有序后备列表。</summary>
    [ECMAScriptName("fontFamily")]
    public static CssFontFamily font_family([Preserve] params CssFontFamilyName[] names)
    {
        if (names.Length == 0)
            Fail("CSS font-family requires at least one family.");

        var output = new Array<string>();
        foreach (var name in names)
            output.Push(StringFn(name));
        return CssFontFamily.create(output.Join(","));
    }

    /// <summary>Creates an X-axis translate transform from a length-percentage value。根据长度/百分比值创建 X 轴 translate transform。</summary>
    [ECMAScriptName("translateX")]
    public static CssTransform translate_x(CssLengthPercentageValue value)
        => CssTransform.create("translateX(" + StringFn(value.Value) + ")");

    /// <summary>Creates a Y-axis translate transform from a length-percentage value。根据长度/百分比值创建 Y 轴 translate transform。</summary>
    [ECMAScriptName("translateY")]
    public static CssTransform translate_y(CssLengthPercentageValue value)
        => CssTransform.create("translateY(" + StringFn(value.Value) + ")");

    /// <summary>Creates a two-axis translate transform。创建双轴 translate transform。</summary>
    public static CssTransform translate(CssLengthPercentageValue x, CssLengthPercentageValue y)
        => CssTransform.create("translate(" + StringFn(x.Value) + "," + StringFn(y.Value) + ")");

    /// <summary>Creates a rotate transform from a typed angle。根据类型化角度创建 rotate transform。</summary>
    public static CssTransform rotate(CssAngle angle)
        => CssTransform.create("rotate(" + StringFn(angle) + ")");

    /// <summary>Creates a uniform scale transform。创建统一缩放的 scale transform。</summary>
    public static CssTransform scale(double value)
        => CssTransform.create("scale(" + number(value) + ")");

    [ECMAScriptName("scale2")]
    public static CssTransform scale(double x, double y)
        => CssTransform.create("scale(" + number(x) + "," + number(y) + ")");

    /// <summary>
    /// Joins one or more transform functions in authored order. The result remains a transform-only carrier.
    /// 按作者顺序连接一个或多个 transform 函数。结果仍保持 transform 专用载体。
    /// </summary>
    public static CssTransform transform([Preserve] params CssTransform[] functions)
    {
        if (functions.Length == 0)
            Fail("CSS transform requires at least one function.");

        var output = new Array<string>();
        foreach (var item in functions)
            output.Push(StringFn(item));
        return CssTransform.create(output.Join(" "));
    }

    /// <summary>
    /// Creates a deterministic comma-separated box-shadow list. Optional shadow fields are emitted in one stable order
    /// so semantically equal records receive the same CSS and generated class hash.
    /// 创建确定性的逗号分隔 box-shadow 列表。可选字段会按固定顺序输出，使语义相等的 record 获得相同 CSS
    /// 与生成 class hash。
    /// </summary>
    public static CssShadowList shadows([Preserve] params CssShadow[] values)
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

    /// <summary>Creates a positive CSS ratio and rejects zero or negative terms。创建正 CSS 比率，并拒绝零或负分量。</summary>
    public static CssRatio ratio(int numerator, int denominator = 1)
    {
        if (numerator <= 0 || denominator <= 0)
            Fail("CSS ratio terms must be greater than zero.");
        return CssRatio.create(StringFn(numerator) + " / " + StringFn(denominator));
    }

    /// <summary>
    /// Creates a border shorthand from its typed width, style, and color branches.
    /// At least one branch is required; use the <c>|</c> operators when incremental composition reads better.
    /// 根据类型化宽度、样式和颜色分支创建 border 简写。至少需要一个分支；增量组合更易读时可使用 <c>|</c> 运算符。
    /// </summary>
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

    /// <summary>Creates a blur filter from a typed length。根据类型化长度创建 blur filter。</summary>
    public static CssFilter blur(CssLength value)
        => CssFilter.create("blur(" + StringFn(value) + ")");

    /// <summary>Creates a grayscale filter with a validated 0..1 amount。创建 amount 已验证为 0..1 的 grayscale filter。</summary>
    public static CssFilter grayscale(double amount)
    {
        validateUnitInterval(amount, "grayscale amount");
        return CssFilter.create("grayscale(" + number(amount) + ")");
    }

    /// <summary>Creates a saturate filter with a finite non-negative amount。创建 amount 为有限非负数的 saturate filter。</summary>
    public static CssFilter saturate(double amount)
    {
        if (!IsFinite(amount) || amount < 0)
            Fail("CSS saturate amount must be non-negative and finite.");
        return CssFilter.create("saturate(" + number(amount) + ")");
    }

    /// <summary>Joins one or more typed filter functions in authored order。按作者顺序连接一个或多个类型化 filter 函数。</summary>
    public static CssFilter filters([Preserve] params CssFilter[] values)
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

    private static string join([Preserve] params string[] values)
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

    private static string normalizeAnchorName(string value)
    {
        var normalized = value.Trim();
        if (!normalized.StartsWith("--"))
            Fail("CSS anchor names must start with '--'.");
        validateToken(normalized, "CSS anchor name", allowCustomPropertyPrefix: true);
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
