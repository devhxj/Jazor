namespace ECMAScript.Style;

public static partial class css
{
    /// <summary>CSS-wide <c>inherit</c> keyword; uses the parent element's computed value. CSS 全局 <c>inherit</c> 关键字，使用父元素的计算值。</summary>
    public static readonly CssWideKeyword inherit = CssWideKeyword.Inherit;

    /// <summary>CSS-wide <c>initial</c> keyword; restores the specification-defined initial value. CSS 全局 <c>initial</c> 关键字，恢复规范定义的初始值。</summary>
    public static readonly CssWideKeyword initial = CssWideKeyword.Initial;

    /// <summary>CSS-wide <c>unset</c> keyword; inherits inherited properties and otherwise uses their initial value. CSS 全局 <c>unset</c> 关键字，继承型属性继承，其他属性恢复初始值。</summary>
    public static readonly CssWideKeyword unset = CssWideKeyword.Unset;

    /// <summary>CSS-wide <c>revert</c> keyword; rolls the declaration back to an earlier cascade origin. CSS 全局 <c>revert</c> 关键字，回退到更早的层叠来源。</summary>
    public static readonly CssWideKeyword revert = CssWideKeyword.Revert;

    /// <summary>CSS-wide <c>revert-layer</c> keyword; rolls the declaration back within cascade layers. CSS 全局 <c>revert-layer</c> 关键字，在层叠层内回退声明。</summary>
    [ECMAScriptName("revertLayer")]
    public static readonly CssWideKeyword revert_layer = CssWideKeyword.RevertLayer;

    /// <summary>CSS <c>auto</c> keyword for a property-defined automatic behavior. CSS <c>auto</c> 关键字，具体行为由接收属性定义。</summary>
    public static readonly CssAutoKeyword auto = CssAutoKeyword.Auto;

    /// <summary>CSS <c>none</c> keyword that disables the feature described by the receiving property. CSS <c>none</c> 关键字，禁用接收属性描述的功能。</summary>
    public static readonly CssNoneKeyword none = CssNoneKeyword.None;

    /// <summary>CSS <c>normal</c> keyword for the property-defined default behavior. CSS <c>normal</c> 关键字，使用属性定义的默认行为。</summary>
    public static readonly CssNormalKeyword normal = CssNormalKeyword.Normal;

    /// <summary>Intrinsic <c>min-content</c> size based on the smallest unwrapped content contribution. 基于最小不换行内容贡献的内在 <c>min-content</c> 尺寸。</summary>
    [ECMAScriptName("minContent")]
    public static readonly CssSizingKeyword min_content = CssSizingKeyword.MinContent;

    /// <summary>Intrinsic <c>max-content</c> size based on the preferred unwrapped content contribution. 基于首选不换行内容贡献的内在 <c>max-content</c> 尺寸。</summary>
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
    /// <summary>Block-level display mode that creates a block box. 创建块级盒的 <c>display: block</c> 模式。</summary>
    public static readonly CssDisplayKeyword block = CssDisplayKeyword.Block;
    /// <summary>Inline display mode that participates in inline layout. 参与行内布局的 <c>display: inline</c> 模式。</summary>
    public static readonly CssDisplayKeyword inline = CssDisplayKeyword.Inline;
    /// <summary>Inline-level box with an internal block formatting context. 具有内部块格式化上下文的行内级盒。</summary>
    [ECMAScriptName("inlineBlock")]
    public static readonly CssDisplayKeyword inline_block = CssDisplayKeyword.InlineBlock;
    /// <summary>Flex container display mode. 弹性布局容器的显示模式。</summary>
    public static readonly CssDisplayKeyword flex = CssDisplayKeyword.Flex;
    /// <summary>Inline-level flex container display mode. 行内级弹性布局容器的显示模式。</summary>
    [ECMAScriptName("inlineFlex")]
    public static readonly CssDisplayKeyword inline_flex = CssDisplayKeyword.InlineFlex;
    /// <summary>Grid container display mode. 网格布局容器的显示模式。</summary>
    public static readonly CssDisplayKeyword grid = CssDisplayKeyword.Grid;
    /// <summary>Inline-level grid container display mode. 行内级网格布局容器的显示模式。</summary>
    [ECMAScriptName("inlineGrid")]
    public static readonly CssDisplayKeyword inline_grid = CssDisplayKeyword.InlineGrid;
    /// <summary>Block box that establishes a new block formatting context. 建立新块格式化上下文的块盒。</summary>
    [ECMAScriptName("flowRoot")]
    public static readonly CssDisplayKeyword flow_root = CssDisplayKeyword.FlowRoot;
    /// <summary>Suppresses the element's principal box while keeping its children in layout. 不生成元素主盒，但保留子元素参与布局。</summary>
    public static readonly CssDisplayKeyword contents = CssDisplayKeyword.Contents;
    /// <summary>Table formatting display mode. 表格格式化显示模式。</summary>
    public static readonly CssDisplayKeyword table = CssDisplayKeyword.Table;
    /// <summary>List-item display mode, which creates a marker box where applicable. 列表项显示模式，在适用时创建标记盒。</summary>
    [ECMAScriptName("listItem")]
    public static readonly CssDisplayKeyword list_item = CssDisplayKeyword.ListItem;
    /// <summary>Normal-flow positioning with no offsets applied. 不应用偏移的常规流定位。</summary>
    [ECMAScriptName("staticPosition")]
    public static readonly CssPositionKeyword static_position = CssPositionKeyword.Static;
    /// <summary>Normal-flow positioning whose visual box may be offset. 仍参与常规流、但可对视觉盒应用偏移的定位。</summary>
    public static readonly CssPositionKeyword relative = CssPositionKeyword.Relative;
    /// <summary>Out-of-flow positioning relative to the containing block. 相对于包含块的脱离常规流定位。</summary>
    public static readonly CssPositionKeyword absolute = CssPositionKeyword.Absolute;
    /// <summary>Viewport-fixed positioning. 相对视口固定的定位。</summary>
    [ECMAScriptName("fixedPosition")]
    public static readonly CssPositionKeyword fixed_position = CssPositionKeyword.Fixed;
    /// <summary>Hybrid relative/fixed positioning that sticks at scroll thresholds. 在滚动阈值处吸附的相对/固定混合定位。</summary>
    public static readonly CssPositionKeyword sticky = CssPositionKeyword.Sticky;
    /// <summary>Leaves overflow visible without clipping. 不裁剪溢出内容。</summary>
    public static readonly CssOverflowKeyword visible = CssOverflowKeyword.Visible;
    /// <summary>Clips overflow while retaining programmatic scrolling where supported. 裁剪溢出内容，并在支持时保留程序化滚动。</summary>
    public static readonly CssOverflowKeyword hidden = CssOverflowKeyword.Hidden;
    /// <summary>Clips overflow without creating a scroll container. 裁剪溢出内容且不创建滚动容器。</summary>
    public static readonly CssOverflowKeyword clip = CssOverflowKeyword.Clip;
    /// <summary>Always creates a scroll container and exposes scrollbars as needed. 创建滚动容器，并按需显示滚动条。</summary>
    public static readonly CssOverflowKeyword scroll = CssOverflowKeyword.Scroll;
    /// <summary>Named thin border width. 具名细边框宽度。</summary>
    public static readonly CssBorderWidth thin = CssBorderWidth.create("thin");
    /// <summary>Named medium border width. 具名中等边框宽度。</summary>
    public static readonly CssBorderWidth medium = CssBorderWidth.create("medium");
    /// <summary>Named thick border width. 具名粗边框宽度。</summary>
    public static readonly CssBorderWidth thick = CssBorderWidth.create("thick");
    /// <summary>Dotted border or outline style. 点状边框或轮廓线型。</summary>
    public static readonly CssBorderStyle dotted = CssBorderStyle.create("dotted");
    /// <summary>Dashed border or outline style. 虚线边框或轮廓线型。</summary>
    public static readonly CssBorderStyle dashed = CssBorderStyle.create("dashed");
    /// <summary>Solid border or outline style. 实线边框或轮廓线型。</summary>
    public static readonly CssBorderStyle solid = CssBorderStyle.create("solid");
    /// <summary>Double-line border or outline style. 双线边框或轮廓线型。</summary>
    [ECMAScriptName("doubleLine")]
    public static readonly CssBorderStyle double_line = CssBorderStyle.create("double");
    /// <summary>Grooved border style with a carved appearance. 凹槽视觉效果的边框线型。</summary>
    public static readonly CssBorderStyle groove = CssBorderStyle.create("groove");
    /// <summary>Ridged border style with a raised appearance. 凸起视觉效果的边框线型。</summary>
    public static readonly CssBorderStyle ridge = CssBorderStyle.create("ridge");
    /// <summary>Inset border style. 内嵌视觉效果的边框线型。</summary>
    public static readonly CssBorderStyle inset = CssBorderStyle.create("inset");
    /// <summary>Outset border style. 外凸视觉效果的边框线型。</summary>
    public static readonly CssBorderStyle outset = CssBorderStyle.create("outset");
    /// <summary>Fully transparent color keyword. 完全透明的颜色关键字。</summary>
    public static readonly CssColorKeyword transparent = CssColorKeyword.Transparent;
    /// <summary>Uses the element's computed <c>color</c> value. 使用元素计算后的 <c>color</c> 值。</summary>
    [ECMAScriptName("currentColor")]
    public static readonly CssColorKeyword current_color = CssColorKeyword.CurrentColor;
    /// <summary>Aligns to the logical start edge. 对齐到逻辑起始边。</summary>
    public static readonly CssAlignmentKeyword start = CssAlignmentKeyword.Start;
    /// <summary>Aligns to the logical end edge. 对齐到逻辑结束边。</summary>
    public static readonly CssAlignmentKeyword end = CssAlignmentKeyword.End;
    /// <summary>Centers along the alignment axis. 沿对齐轴居中。</summary>
    public static readonly CssAlignmentKeyword center = CssAlignmentKeyword.Center;
    /// <summary>Aligns to the flex container's main-start or cross-start edge. 对齐到弹性容器的起始边。</summary>
    [ECMAScriptName("flexStart")]
    public static readonly CssAlignmentKeyword flex_start = CssAlignmentKeyword.FlexStart;
    /// <summary>Aligns to the flex container's main-end or cross-end edge. 对齐到弹性容器的结束边。</summary>
    [ECMAScriptName("flexEnd")]
    public static readonly CssAlignmentKeyword flex_end = CssAlignmentKeyword.FlexEnd;
    /// <summary>Aligns to the item's own logical start edge. 对齐到项目自身的逻辑起始边。</summary>
    [ECMAScriptName("selfStart")]
    public static readonly CssAlignmentKeyword self_start = CssAlignmentKeyword.SelfStart;
    /// <summary>Aligns to the item's own logical end edge. 对齐到项目自身的逻辑结束边。</summary>
    [ECMAScriptName("selfEnd")]
    public static readonly CssAlignmentKeyword self_end = CssAlignmentKeyword.SelfEnd;
    /// <summary>Aligns to the physical left edge when the property grammar permits it. 在属性语法允许时对齐到物理左边。</summary>
    public static readonly CssAlignmentKeyword left = CssAlignmentKeyword.Left;
    /// <summary>Aligns to the physical right edge when the property grammar permits it. 在属性语法允许时对齐到物理右边。</summary>
    public static readonly CssAlignmentKeyword right = CssAlignmentKeyword.Right;
    /// <summary>Stretches auto-sized items to fill the alignment axis. 将自动尺寸项目拉伸以填满对齐轴。</summary>
    public static readonly CssAlignmentKeyword stretch = CssAlignmentKeyword.Stretch;
    /// <summary>Aligns items by their baselines. 按项目基线对齐。</summary>
    public static readonly CssAlignmentKeyword baseline = CssAlignmentKeyword.Baseline;
    /// <summary>Distributes free space only between items. 仅在项目之间分配剩余空间。</summary>
    [ECMAScriptName("spaceBetween")]
    public static readonly CssAlignmentKeyword space_between = CssAlignmentKeyword.SpaceBetween;
    /// <summary>Distributes free space around items, with half-size outer gaps. 在项目周围分配剩余空间，外侧间隙为内部的一半。</summary>
    [ECMAScriptName("spaceAround")]
    public static readonly CssAlignmentKeyword space_around = CssAlignmentKeyword.SpaceAround;
    /// <summary>Distributes equal free space between items and at both outer edges. 在项目之间及两端均匀分配剩余空间。</summary>
    [ECMAScriptName("spaceEvenly")]
    public static readonly CssAlignmentKeyword space_evenly = CssAlignmentKeyword.SpaceEvenly;
    /// <summary>Uses the inline axis as the flex main axis. 使用行内轴作为弹性主轴。</summary>
    public static readonly CssFlexDirectionKeyword row = CssFlexDirectionKeyword.Row;
    /// <summary>Uses the reversed inline axis as the flex main axis. 使用反向行内轴作为弹性主轴。</summary>
    [ECMAScriptName("rowReverse")]
    public static readonly CssFlexDirectionKeyword row_reverse = CssFlexDirectionKeyword.RowReverse;
    /// <summary>Uses the block axis as the flex main axis. 使用块轴作为弹性主轴。</summary>
    public static readonly CssFlexDirectionKeyword column = CssFlexDirectionKeyword.Column;
    /// <summary>Uses the reversed block axis as the flex main axis. 使用反向块轴作为弹性主轴。</summary>
    [ECMAScriptName("columnReverse")]
    public static readonly CssFlexDirectionKeyword column_reverse = CssFlexDirectionKeyword.ColumnReverse;
    /// <summary>Keeps flex items on one line. 让弹性项目保持单行。</summary>
    [ECMAScriptName("noWrap")]
    public static readonly CssFlexWrapKeyword no_wrap = CssFlexWrapKeyword.NoWrap;
    /// <summary>Allows flex items to wrap onto additional lines. 允许弹性项目换行到额外行。</summary>
    public static readonly CssFlexWrapKeyword wrap = CssFlexWrapKeyword.Wrap;
    /// <summary>Allows wrapped flex lines in reverse cross-axis order. 允许换行，并反转交叉轴上的行顺序。</summary>
    [ECMAScriptName("wrapReverse")]
    public static readonly CssFlexWrapKeyword wrap_reverse = CssFlexWrapKeyword.WrapReverse;
    /// <summary>Scales a background image to cover the positioning area, allowing cropping. 缩放背景图以覆盖定位区域，允许裁切。</summary>
    public static readonly CssBackgroundSizeKeyword cover = CssBackgroundSizeKeyword.Cover;
    /// <summary>Scales a background image to fit inside the positioning area without cropping. 缩放背景图以完整容纳在定位区域内。</summary>
    public static readonly CssBackgroundSizeKeyword contain = CssBackgroundSizeKeyword.Contain;
    /// <summary>Includes borders and padding in declared width and height. 声明的宽高包含边框和内边距。</summary>
    [ECMAScriptName("borderBox")]
    public static readonly CssBoxSizingKeyword border_box = CssBoxSizingKeyword.BorderBox;
    /// <summary>Applies declared width and height to the content box only. 声明的宽高仅作用于内容盒。</summary>
    [ECMAScriptName("contentBox")]
    public static readonly CssBoxSizingKeyword content_box = CssBoxSizingKeyword.ContentBox;
    /// <summary>Uses the platform default cursor. 使用平台默认鼠标指针。</summary>
    [ECMAScriptName("defaultCursor")]
    public static readonly CssCursorKeyword default_cursor = CssCursorKeyword.Default;
    /// <summary>Uses the pointer cursor for clickable targets. 为可点击目标使用指针鼠标样式。</summary>
    public static readonly CssCursorKeyword pointer = CssCursorKeyword.Pointer;
    /// <summary>Uses the prohibited-action cursor. 使用禁止操作鼠标样式。</summary>
    [ECMAScriptName("notAllowed")]
    public static readonly CssCursorKeyword not_allowed = CssCursorKeyword.NotAllowed;
    /// <summary>Uses the text-selection cursor. 使用文本选择鼠标样式。</summary>
    [ECMAScriptName("textCursor")]
    public static readonly CssCursorKeyword text_cursor = CssCursorKeyword.Text;
    /// <summary>Capitalizes the first typographic letter of each word where supported. 在支持时将每个词的首个排版字母大写。</summary>
    public static readonly CssTextTransformKeyword capitalize = CssTextTransformKeyword.Capitalize;
    /// <summary>Converts text to uppercase. 将文本转换为大写。</summary>
    public static readonly CssTextTransformKeyword uppercase = CssTextTransformKeyword.Uppercase;
    /// <summary>Converts text to lowercase. 将文本转换为小写。</summary>
    public static readonly CssTextTransformKeyword lowercase = CssTextTransformKeyword.Lowercase;
    /// <summary>Collapses whitespace and prevents line wrapping. 合并空白字符且禁止自动换行。</summary>
    public static readonly CssWhiteSpaceKeyword nowrap = CssWhiteSpaceKeyword.NoWrap;
    /// <summary>Preserves whitespace and line breaks in preformatted text. 为预格式化文本保留空白和换行。</summary>
    public static readonly CssWhiteSpaceKeyword pre = CssWhiteSpaceKeyword.Pre;
    /// <summary>Preserves whitespace while allowing wrapping. 保留空白字符，同时允许自动换行。</summary>
    [ECMAScriptName("preWrap")]
    public static readonly CssWhiteSpaceKeyword pre_wrap = CssWhiteSpaceKeyword.PreWrap;
    /// <summary>Collapses whitespace but preserves newline characters as breaks. 合并空白字符，但保留换行符作为换行。</summary>
    [ECMAScriptName("preLine")]
    public static readonly CssWhiteSpaceKeyword pre_line = CssWhiteSpaceKeyword.PreLine;
    /// <summary>Uses an ellipsis marker for clipped inline text. 为被裁剪的行内文本显示省略号标记。</summary>
    public static readonly CssTextOverflowKeyword ellipsis = CssTextOverflowKeyword.Ellipsis;
    /// <summary>Creates a new stacking context for the element. 为元素创建新的堆叠上下文。</summary>
    public static readonly CssIsolationKeyword isolate = CssIsolationKeyword.Isolate;
    /// <summary>Declares support for a light color scheme. 声明支持浅色配色方案。</summary>
    public static readonly CssColorSchemeKeyword light = CssColorSchemeKeyword.Light;
    /// <summary>Declares support for a dark color scheme. 声明支持深色配色方案。</summary>
    public static readonly CssColorSchemeKeyword dark = CssColorSchemeKeyword.Dark;
    /// <summary>Uses constant-rate animation timing. 使用恒定速率的动画时间函数。</summary>
    public static readonly CssTimingFunctionKeyword linear = CssTimingFunctionKeyword.Linear;
    /// <summary>Uses the default ease timing curve. 使用默认的缓入缓出时间曲线。</summary>
    public static readonly CssTimingFunctionKeyword ease = CssTimingFunctionKeyword.Ease;
    /// <summary>Uses an ease-in timing curve. 使用缓入时间曲线。</summary>
    [ECMAScriptName("easeIn")]
    public static readonly CssTimingFunctionKeyword ease_in = CssTimingFunctionKeyword.EaseIn;
    /// <summary>Uses an ease-out timing curve. 使用缓出时间曲线。</summary>
    [ECMAScriptName("easeOut")]
    public static readonly CssTimingFunctionKeyword ease_out = CssTimingFunctionKeyword.EaseOut;
    /// <summary>Uses an ease-in-out timing curve. 使用缓入缓出时间曲线。</summary>
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
    /// References a custom property with a typed fallback, including another <c>var(...)</c>.
    /// The browser resolves substitution; the fallback is not a runtime type check.
    /// 引用自定义属性并提供类型化后备值（可嵌套 var）；替换由浏览器解析，fallback 并非运行时类型检查。
    /// </summary>
    [ECMAScriptName("variableFallback")]
    public static CssVariable var(string name, CssValue fallback)
        => var_or(name, fallback);

    /// <summary>
    /// Creates a typed custom-property reference with a typed CSS fallback.
    /// Fallback serialization keeps its original union branch rather than accepting an arbitrary string.
    /// 创建带类型化 CSS 后备值的自定义属性引用。后备值序列化会保持原始 union 分支，而非接受任意字符串。
    /// </summary>
    [ECMAScriptName("varOr")]
    public static CssVariable var_or(string name, CssValue fallback)
    {
        var normalized = normalizeVariableName(name);
        return CssVariable.create("var(" + normalized + "," + StringValue(fallback.Value) + ")");
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
            output.Push(StringValue(value));
        return CssAnchorNameList.create(output.Join(" "));
    }

    /// <summary>
    /// References the requested side of the implicit anchor with <c>anchor(...)</c>.
    /// 使用 <c>anchor(...)</c> 引用隐式锚点的指定边。
    /// </summary>
    public static CssAnchor anchor(CssAnchorSideValue side)
        => CssAnchor.create("anchor(" + StringValue(side.Value) + ")");

    /// <summary>
    /// References a specified named anchor side with <c>anchor(...)</c>.
    /// 使用 <c>anchor(...)</c> 引用指定命名锚点的一条边。
    /// </summary>
    [ECMAScriptName("anchorNamed")]
    public static CssAnchor anchor(CssAnchorName name, CssAnchorSideValue side)
        => CssAnchor.create("anchor(" + StringValue(name) + " " + StringValue(side.Value) + ")");

    /// <summary>
    /// References an implicit anchor side and supplies the CSS fallback used when no anchor resolves.
    /// 引用隐式锚点边，并提供无法解析锚点时使用的 CSS 后备值。
    /// </summary>
    [ECMAScriptName("anchorFallback")]
    public static CssAnchor anchor(CssAnchorSideValue side, CssLengthPercentageValue fallback)
        => CssAnchor.create("anchor(" + StringValue(side.Value) + "," + StringValue(fallback.Value) + ")");

    /// <summary>
    /// References a named anchor side and supplies its CSS fallback.
    /// 引用命名锚点边，并提供其 CSS 后备值。
    /// </summary>
    [ECMAScriptName("anchorNamedFallback")]
    public static CssAnchor anchor(CssAnchorName name, CssAnchorSideValue side, CssLengthPercentageValue fallback)
        => CssAnchor.create("anchor(" + StringValue(name) + " " + StringValue(side.Value) + "," + StringValue(fallback.Value) + ")");

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
        => CssAnchorSize.create("anchor-size(" + StringValue(name) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for one axis of the implicit anchor.
    /// 为隐式锚点的一个轴创建 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeAxis")]
    public static CssAnchorSize anchor_size(CssAnchorSizeAxis axis)
        => CssAnchorSize.create("anchor-size(" + StringValue(axis) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for one axis of a named anchor.
    /// 为命名锚点的一个轴创建 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeNamedAxis")]
    public static CssAnchorSize anchor_size(CssAnchorName name, CssAnchorSizeAxis axis)
        => CssAnchorSize.create("anchor-size(" + StringValue(name) + " " + StringValue(axis) + ")");

    /// <summary>
    /// Creates <c>anchor-size(, fallback)</c> for the implicit anchor when its size cannot resolve.
    /// 在隐式锚点尺寸无法解析时创建 <c>anchor-size(, fallback)</c>。
    /// </summary>
    [ECMAScriptName("anchorSizeFallback")]
    public static CssAnchorSize anchor_size(CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(," + StringValue(fallback.Value) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for a named anchor with a fallback.
    /// 为命名锚点创建带后备值的 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeNamedFallback")]
    public static CssAnchorSize anchor_size(CssAnchorName name, CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(" + StringValue(name) + "," + StringValue(fallback.Value) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for an implicit anchor axis with a fallback.
    /// 为隐式锚点轴创建带后备值的 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeAxisFallback")]
    public static CssAnchorSize anchor_size(CssAnchorSizeAxis axis, CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(" + StringValue(axis) + "," + StringValue(fallback.Value) + ")");

    /// <summary>
    /// Creates an <c>anchor-size()</c> expression for a named anchor axis with a fallback.
    /// 为命名锚点轴创建带后备值的 <c>anchor-size()</c> 表达式。
    /// </summary>
    [ECMAScriptName("anchorSizeNamedAxisFallback")]
    public static CssAnchorSize anchor_size(CssAnchorName name, CssAnchorSizeAxis axis, CssLengthPercentageValue fallback)
        => CssAnchorSize.create("anchor-size(" + StringValue(name) + " " + StringValue(axis) + "," + StringValue(fallback.Value) + ")");

    /// <summary>
    /// Creates <c>calc-size(basis, calculation)</c> using the typed result-side expression rooted
    /// at <c>size</c>.
    /// 使用以 <c>size</c> 为根的类型化结果表达式创建 <c>calc-size(basis, calculation)</c>。
    /// </summary>
    [ECMAScriptName("calcSize")]
    public static CssCalcSize calc_size(CssCalcSizeBasis basis, CssCalcSizeExpression calculation)
        => CssCalcSize.create("calc-size(" + StringValue(basis.Value) + "," + StringValue(calculation) + ")");

    /// <summary>
    /// Creates <c>calc-size(basis, calculation)</c> when the calculation is already a typed
    /// length-percentage expression.
    /// 当计算式已经是类型化 length-percentage 表达式时，创建 <c>calc-size(basis, calculation)</c>。
    /// </summary>
    [ECMAScriptName("calcSizeValue")]
    public static CssCalcSize calc_size(CssCalcSizeBasis basis, CssLengthPercentageValue calculation)
        => CssCalcSize.create("calc-size(" + StringValue(basis.Value) + "," + StringValue(calculation.Value) + ")");

    /// <summary>
    /// Creates a pixel CSS length. Unit factories return <see cref="CssLength"/>, so they cannot accidentally
    /// occupy percentage-only or time-only properties.
    /// 创建像素 CSS 长度。单位工厂返回 <see cref="CssLength"/>，因此不能意外用于仅百分比或仅时间属性。
    /// </summary>
    public static CssLength px(double value) => CssLength.create(number(value) + "px");

    /// <summary>Creates a root-font-relative CSS length。创建相对于根字体大小的 CSS 长度。</summary>
    public static CssLength rem(double value) => CssLength.create(number(value) + "rem");
    /// <summary>Creates an <c>em</c> length relative to the element's font size. 创建相对于元素字体大小的 <c>em</c> 长度。</summary>
    public static CssLength em(double value) => CssLength.create(number(value) + "em");
    /// <summary>Creates an <c>ex</c> length relative to the font's x-height. 创建相对于字体 x 高度的 <c>ex</c> 长度。</summary>
    public static CssLength ex(double value) => CssLength.create(number(value) + "ex");
    /// <summary>Creates a <c>ch</c> length relative to the advance measure of the zero glyph. 创建相对于数字零字形前进宽度的 <c>ch</c> 长度。</summary>
    public static CssLength ch(double value) => CssLength.create(number(value) + "ch");
    /// <summary>Creates a <c>cap</c> length relative to the font's cap height. 创建相对于字体大写字母高度的 <c>cap</c> 长度。</summary>
    public static CssLength cap(double value) => CssLength.create(number(value) + "cap");
    /// <summary>Creates an <c>ic</c> length relative to an ideographic character. 创建相对于表意字符尺寸的 <c>ic</c> 长度。</summary>
    public static CssLength ic(double value) => CssLength.create(number(value) + "ic");
    /// <summary>Creates an <c>lh</c> length relative to the element's computed line height. 创建相对于元素计算行高的 <c>lh</c> 长度。</summary>
    public static CssLength lh(double value) => CssLength.create(number(value) + "lh");
    /// <summary>Creates an <c>rlh</c> length relative to the root element's line height. 创建相对于根元素行高的 <c>rlh</c> 长度。</summary>
    public static CssLength rlh(double value) => CssLength.create(number(value) + "rlh");
    /// <summary>Creates a viewport-width-relative <c>vw</c> length. 创建相对于布局视口宽度的 <c>vw</c> 长度。</summary>
    public static CssLength vw(double value) => CssLength.create(number(value) + "vw");
    /// <summary>Creates a viewport-height-relative <c>vh</c> length. 创建相对于布局视口高度的 <c>vh</c> 长度。</summary>
    public static CssLength vh(double value) => CssLength.create(number(value) + "vh");
    /// <summary>Creates a length relative to the smaller viewport dimension. 创建相对于较小视口维度的长度。</summary>
    public static CssLength vmin(double value) => CssLength.create(number(value) + "vmin");
    /// <summary>Creates a length relative to the larger viewport dimension. 创建相对于较大视口维度的长度。</summary>
    public static CssLength vmax(double value) => CssLength.create(number(value) + "vmax");
    /// <summary>Creates a small-viewport-width-relative <c>svw</c> length. 创建相对于最小视口宽度的 <c>svw</c> 长度。</summary>
    public static CssLength svw(double value) => CssLength.create(number(value) + "svw");
    /// <summary>Creates a small-viewport-height-relative <c>svh</c> length. 创建相对于最小视口高度的 <c>svh</c> 长度。</summary>
    public static CssLength svh(double value) => CssLength.create(number(value) + "svh");
    /// <summary>Creates a large-viewport-width-relative <c>lvw</c> length. 创建相对于最大视口宽度的 <c>lvw</c> 长度。</summary>
    public static CssLength lvw(double value) => CssLength.create(number(value) + "lvw");
    /// <summary>Creates a large-viewport-height-relative <c>lvh</c> length. 创建相对于最大视口高度的 <c>lvh</c> 长度。</summary>
    public static CssLength lvh(double value) => CssLength.create(number(value) + "lvh");
    /// <summary>Creates a dynamic-viewport-width-relative <c>dvw</c> length. 创建相对于动态视口宽度的 <c>dvw</c> 长度。</summary>
    public static CssLength dvw(double value) => CssLength.create(number(value) + "dvw");
    /// <summary>Creates a dynamic-viewport-height-relative <c>dvh</c> length. 创建相对于动态视口高度的 <c>dvh</c> 长度。</summary>
    public static CssLength dvh(double value) => CssLength.create(number(value) + "dvh");
    /// <summary>Creates an absolute centimeter CSS length. 创建绝对厘米 CSS 长度。</summary>
    public static CssLength cm(double value) => CssLength.create(number(value) + "cm");
    /// <summary>Creates an absolute millimeter CSS length. 创建绝对毫米 CSS 长度。</summary>
    public static CssLength mm(double value) => CssLength.create(number(value) + "mm");
    /// <summary>Creates a quarter-millimeter <c>Q</c> CSS length. 创建四分之一毫米的 <c>Q</c> CSS 长度。</summary>
    public static CssLength q(double value) => CssLength.create(number(value) + "Q");
    /// <summary>Creates an absolute inch CSS length. 创建绝对英寸 CSS 长度。</summary>
    public static CssLength inch(double value) => CssLength.create(number(value) + "in");
    /// <summary>Creates an absolute point CSS length. 创建绝对 point CSS 长度。</summary>
    public static CssLength pt(double value) => CssLength.create(number(value) + "pt");
    /// <summary>Creates an absolute pica CSS length. 创建绝对 pica CSS 长度。</summary>
    public static CssLength pc(double value) => CssLength.create(number(value) + "pc");
    /// <summary>Creates a CSS percentage, distinct from a unitless number。创建 CSS 百分比，与无单位数值分离。</summary>
    public static CssPercentage percent(double value) => CssPercentage.create(number(value) + "%");

    /// <summary>Creates a length-only <c>min(...)</c> expression。创建仅长度的 <c>min(...)</c> 表达式。</summary>
    public static CssLength min(CssLength first, CssLength second)
        => CssLength.create("min(" + StringValue(first) + "," + StringValue(second) + ")");

    /// <summary>Creates a mixed length-percentage <c>min(...)</c> expression。创建混合长度/百分比 <c>min(...)</c> 表达式。</summary>
    [ECMAScriptName("minLengthPercentage")]
    public static CssLengthPercentage min(CssLengthPercentageValue first, CssLengthPercentageValue second)
        => CssLengthPercentage.create("min(" + StringValue(first.Value) + "," + StringValue(second.Value) + ")");

    /// <summary>Creates a length-only <c>max(...)</c> expression。创建仅长度的 <c>max(...)</c> 表达式。</summary>
    public static CssLength max(CssLength first, CssLength second)
        => CssLength.create("max(" + StringValue(first) + "," + StringValue(second) + ")");

    /// <summary>Creates a length-only <c>clamp(minimum, preferred, maximum)</c> expression。创建仅长度的 <c>clamp(minimum, preferred, maximum)</c> 表达式。</summary>
    public static CssLength clamp(CssLength minimum, CssLength preferred, CssLength maximum)
        => CssLength.create("clamp(" + StringValue(minimum) + "," + StringValue(preferred) + "," + StringValue(maximum) + ")");

    /// <summary>Creates a CSS angle in degrees. 创建以度为单位的 CSS 角度。</summary>
    public static CssAngle deg(double value) => CssAngle.create(number(value) + "deg");
    /// <summary>Creates a CSS angle in gradians. 创建以百分度为单位的 CSS 角度。</summary>
    public static CssAngle grad(double value) => CssAngle.create(number(value) + "grad");
    /// <summary>Creates a CSS angle in radians. 创建以弧度为单位的 CSS 角度。</summary>
    public static CssAngle rad(double value) => CssAngle.create(number(value) + "rad");
    /// <summary>Creates a CSS angle in complete turns. 创建以完整圈为单位的 CSS 角度。</summary>
    public static CssAngle turn(double value) => CssAngle.create(number(value) + "turn");
    /// <summary>Creates a CSS time in milliseconds. 创建以毫秒为单位的 CSS 时间。</summary>
    public static CssTime ms(double value) => CssTime.create(number(value) + "ms");
    /// <summary>Creates a CSS time in seconds. 创建以秒为单位的 CSS 时间。</summary>
    public static CssTime seconds(double value) => CssTime.create(number(value) + "s");
    /// <summary>Creates a CSS frequency in hertz. 创建以赫兹为单位的 CSS 频率。</summary>
    public static CssFrequency hz(double value) => CssFrequency.create(number(value) + "Hz");
    /// <summary>Creates a CSS frequency in kilohertz. 创建以千赫兹为单位的 CSS 频率。</summary>
    public static CssFrequency khz(double value) => CssFrequency.create(number(value) + "kHz");
    /// <summary>Creates a CSS resolution in dots per inch. 创建以每英寸点数为单位的 CSS 分辨率。</summary>
    public static CssResolution dpi(double value) => CssResolution.create(number(value) + "dpi");
    /// <summary>Creates a CSS resolution in dots per centimeter. 创建以每厘米点数为单位的 CSS 分辨率。</summary>
    public static CssResolution dpcm(double value) => CssResolution.create(number(value) + "dpcm");
    /// <summary>Creates a CSS resolution in dots per CSS pixel. 创建以每 CSS 像素点数为单位的 CSS 分辨率。</summary>
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
        return CssColor.create("rgb(" + StringValue(red) + " " + StringValue(green) + " " + StringValue(blue) + ")");
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
        return CssColor.create("rgb(" + StringValue(red) + " " + StringValue(green) + " " + StringValue(blue) + " / " + number(alpha) + ")");
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
        => CssTrack.create("minmax(" + StringValue(minimum.Value) + "," + StringValue(maximum.Value) + ")");

    /// <summary>
    /// Creates the functional <c>fit-content(...)</c> size. The returned carrier is accepted by
    /// grid tracks, box sizing, and column-width without becoming a general track value.
    /// 创建函数形式的 <c>fit-content(...)</c> 尺寸。返回的载体可用于 grid track、盒尺寸和
    /// column-width，同时不会退化为通用轨道值。
    /// </summary>
    [ECMAScriptName("fitContent")]
    public static CssFitContent fit_content(CssLengthPercentageValue limit)
        => CssFitContent.create("fit-content(" + StringValue(limit.Value) + ")");

    /// <summary>
    /// Creates a fixed-count grid <c>repeat(...)</c> function. The count is validated as positive before emission.
    /// 创建固定次数的 grid <c>repeat(...)</c> 函数。次数会在输出前验证为正数。
    /// </summary>
    public static CssTrack repeat(int count, CssTrackValue track)
    {
        if (count <= 0)
            Fail("CSS repeat count must be greater than zero.");
        return CssTrack.create("repeat(" + StringValue(count) + "," + StringValue(track.Value) + ")");
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
            output.Push(StringValue(value.Value));
        return CssTrack.create(output.Join(" "));
    }

    /// <summary>
    /// Creates a CSS padding shorthand. Its overloads map to the standard one-, two-, three-, and four-side forms.
    /// 创建 CSS padding 简写。各重载分别映射标准的一、二、三、四边形式。
    /// </summary>
    public static CssPadding padding(CssPaddingPart value)
        => CssPadding.create(StringValue(value.Value));

    /// <summary>Creates <c>padding: vertical horizontal</c>; each value applies to its opposing sides. 创建 <c>padding: vertical horizontal</c>，两个值分别作用于相对边。</summary>
    [ECMAScriptName("padding2")]
    public static CssPadding padding(CssPaddingPart vertical, CssPaddingPart horizontal)
        => CssPadding.create(join(StringValue(vertical.Value), StringValue(horizontal.Value)));

    /// <summary>Creates <c>padding: top horizontal bottom</c>; the horizontal value applies to both left and right. 创建 <c>padding: top horizontal bottom</c>，中间值同时作用于左右边。</summary>
    [ECMAScriptName("padding3")]
    public static CssPadding padding(CssPaddingPart top, CssPaddingPart horizontal, CssPaddingPart bottom)
        => CssPadding.create(join(StringValue(top.Value), StringValue(horizontal.Value), StringValue(bottom.Value)));

    /// <summary>Creates <c>padding: top right bottom left</c> with four explicit sides. 按上、右、下、左顺序创建四边 <c>padding</c>。</summary>
    [ECMAScriptName("padding4")]
    public static CssPadding padding(CssPaddingPart top, CssPaddingPart right, CssPaddingPart bottom, CssPaddingPart left)
        => CssPadding.create(join(StringValue(top.Value), StringValue(right.Value), StringValue(bottom.Value), StringValue(left.Value)));

    /// <summary>
    /// Creates a CSS margin shorthand. Anchor-size values remain allowed only through <see cref="CssMarginPart"/>.
    /// 创建 CSS margin 简写。anchor-size 值只会通过 <see cref="CssMarginPart"/> 被允许。
    /// </summary>
    public static CssMargin margin(CssMarginPart value)
        => CssMargin.create(StringValue(value.Value));

    /// <summary>Creates <c>margin: vertical horizontal</c>; each value applies to its opposing sides. 创建 <c>margin: vertical horizontal</c>，两个值分别作用于相对边。</summary>
    [ECMAScriptName("margin2")]
    public static CssMargin margin(CssMarginPart vertical, CssMarginPart horizontal)
        => CssMargin.create(join(StringValue(vertical.Value), StringValue(horizontal.Value)));

    /// <summary>Creates <c>margin: top horizontal bottom</c>; the horizontal value applies to both left and right. 创建 <c>margin: top horizontal bottom</c>，中间值同时作用于左右边。</summary>
    [ECMAScriptName("margin3")]
    public static CssMargin margin(CssMarginPart top, CssMarginPart horizontal, CssMarginPart bottom)
        => CssMargin.create(join(StringValue(top.Value), StringValue(horizontal.Value), StringValue(bottom.Value)));

    /// <summary>Creates <c>margin: top right bottom left</c> with four explicit sides. 按上、右、下、左顺序创建四边 <c>margin</c>。</summary>
    [ECMAScriptName("margin4")]
    public static CssMargin margin(CssMarginPart top, CssMarginPart right, CssMarginPart bottom, CssMarginPart left)
        => CssMargin.create(join(StringValue(top.Value), StringValue(right.Value), StringValue(bottom.Value), StringValue(left.Value)));

    /// <summary>
    /// Creates a one-side <c>inset</c> shorthand. Use the overloads for two to four sides when the
    /// shorthand is assigned to <c>inset</c>, <c>inset-block</c>, or <c>inset-inline</c>.
    /// 创建单边 <c>inset</c> 简写。赋给 <c>inset</c>、<c>inset-block</c> 或 <c>inset-inline</c>
    /// 时，可使用二至四边重载。
    /// </summary>
    [ECMAScriptName("insetSides")]
    public static CssInset inset_sides(CssInsetPart value)
        => CssInset.create(StringValue(value.Value));

    /// <summary>Creates a two-side <c>inset</c> shorthand。创建双边 <c>inset</c> 简写。</summary>
    [ECMAScriptName("insetSides2")]
    public static CssInset inset_sides(CssInsetPart vertical, CssInsetPart horizontal)
        => CssInset.create(join(StringValue(vertical.Value), StringValue(horizontal.Value)));

    /// <summary>Creates a three-side <c>inset</c> shorthand。创建三值 <c>inset</c> 简写。</summary>
    [ECMAScriptName("insetSides3")]
    public static CssInset inset_sides(CssInsetPart top, CssInsetPart horizontal, CssInsetPart bottom)
        => CssInset.create(join(StringValue(top.Value), StringValue(horizontal.Value), StringValue(bottom.Value)));

    /// <summary>Creates a four-side <c>inset</c> shorthand。创建四边 <c>inset</c> 简写。</summary>
    [ECMAScriptName("insetSides4")]
    public static CssInset inset_sides(CssInsetPart top, CssInsetPart right, CssInsetPart bottom, CssInsetPart left)
        => CssInset.create(join(StringValue(top.Value), StringValue(right.Value), StringValue(bottom.Value), StringValue(left.Value)));

    /// <summary>Creates a one- or two-axis CSS gap shorthand。创建单轴或双轴 CSS gap 简写。</summary>
    public static CssGap gap(CssGapPart value)
        => CssGap.create(StringValue(value.Value));

    /// <summary>Creates <c>gap: row column</c> with independent row and column gaps. 创建行间距和列间距独立的 <c>gap: row column</c>。</summary>
    [ECMAScriptName("gap2")]
    public static CssGap gap(CssGapPart row, CssGapPart column)
        => CssGap.create(join(StringValue(row.Value), StringValue(column.Value)));

    /// <summary>Creates a one- to four-corner radius shorthand。创建一至四值 radius 简写。</summary>
    public static CssRadius radius(CssRadiusPart value)
        => CssRadius.create(StringValue(value.Value));

    /// <summary>Creates two corner radii; the first applies to top-left/bottom-right and the second to top-right/bottom-left. 创建双值圆角：第一个作用于左上/右下，第二个作用于右上/左下。</summary>
    [ECMAScriptName("radius2")]
    public static CssRadius radius(CssRadiusPart vertical, CssRadiusPart horizontal)
        => CssRadius.create(join(StringValue(vertical.Value), StringValue(horizontal.Value)));

    /// <summary>Creates three radii: top-left, top-right/bottom-left, bottom-right。三值依次为左上、右上/左下、右下。</summary>
    [ECMAScriptName("radius3")]
    public static CssRadius radius(CssRadiusPart topLeft, CssRadiusPart topRightAndBottomLeft, CssRadiusPart bottomRight)
        => CssRadius.create(join(StringValue(topLeft.Value), StringValue(topRightAndBottomLeft.Value), StringValue(bottomRight.Value)));

    /// <summary>Creates four corner radii in top-left, top-right, bottom-right, bottom-left order. 按左上、右上、右下、左下顺序创建四个圆角。</summary>
    [ECMAScriptName("radius4")]
    public static CssRadius radius(CssRadiusPart topLeft, CssRadiusPart topRight, CssRadiusPart bottomRight, CssRadiusPart bottomLeft)
        => CssRadius.create(join(StringValue(topLeft.Value), StringValue(topRight.Value), StringValue(bottomRight.Value), StringValue(bottomLeft.Value)));

    /// <summary>Creates the structured <c>flex-grow flex-shrink flex-basis</c> shorthand。创建结构化 <c>flex-grow flex-shrink flex-basis</c> 简写。</summary>
    [ECMAScriptName("flexBox")]
    public static CssFlex flex_box(double grow, double shrink, CssLengthPercentageValue basis)
        => CssFlex.create(number(grow) + " " + number(shrink) + " " + StringValue(basis.Value));

    /// <summary>Creates a two-dimensional background-size value。创建二维 background-size 值。</summary>
    [ECMAScriptName("backgroundSize")]
    public static CssBackgroundSize background_size(CssLengthPercentageValue width, CssLengthPercentageValue height)
        => CssBackgroundSize.create(join(StringValue(width.Value), StringValue(height.Value)));

    /// <summary>Creates a grid line reference or start/end line range。创建 grid 线引用或 start/end 线区间。</summary>
    [ECMAScriptName("gridLine")]
    public static CssGridLine grid_line(int line)
        => CssGridLine.create(StringValue(line));

    /// <summary>Creates a grid line range from its start line to its end line. 根据起始和结束网格线创建网格线区间。</summary>
    [ECMAScriptName("gridLine2")]
    public static CssGridLine grid_line(int start, int end)
        => CssGridLine.create(StringValue(start) + " / " + StringValue(end));

    /// <summary>Creates a gradient stop, optionally with a point or range position。创建 gradient stop，可选单点或区间位置。</summary>
    public static CssGradientStop stop(CssColorValue color)
        => new(color);

    /// <summary>Creates a gradient color stop at one length or percentage position. 在一个长度或百分比位置创建渐变颜色 stop。</summary>
    [ECMAScriptName("stopAt")]
    public static CssGradientStop stop(CssColorValue color, CssLengthPercentageValue at)
        => new(color, at);

    /// <summary>Creates a gradient color stop with a start and end position for a hard range. 使用起始和结束位置创建硬边范围渐变颜色 stop。</summary>
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
        => CssAnimation.create(StringValue(name) + " " + StringValue(duration) + " " + StringValue(timing));

    /// <summary>Creates a quoted font-family entry。创建带引号的 font-family 条目。</summary>
    public static CssFontFamilyName font(string value)
        => CssFontFamilyName.create(quote(value));

    /// <summary>Creates a validated generic font-family identifier without quotes。创建不带引号、已验证的 generic font-family 标识符。</summary>
    [ECMAScriptName("genericFont")]
    public static CssFontFamilyName generic_font(string value)
        => CssFontFamilyName.create(StringValue(ident(value)));

    /// <summary>Creates an ordered font fallback list that contains at least one family。创建至少含一个字体族的有序后备列表。</summary>
    [ECMAScriptName("fontFamily")]
    public static CssFontFamily font_family([Preserve] params CssFontFamilyName[] names)
    {
        if (names.Length == 0)
            Fail("CSS font-family requires at least one family.");

        var output = new Array<string>();
        foreach (var name in names)
            output.Push(StringValue(name));
        return CssFontFamily.create(output.Join(","));
    }

    /// <summary>Creates an X-axis translate transform from a length-percentage value。根据长度/百分比值创建 X 轴 translate transform。</summary>
    [ECMAScriptName("translateX")]
    public static CssTransform translate_x(CssLengthPercentageValue value)
        => CssTransform.create("translateX(" + StringValue(value.Value) + ")");

    /// <summary>Creates a Y-axis translate transform from a length-percentage value。根据长度/百分比值创建 Y 轴 translate transform。</summary>
    [ECMAScriptName("translateY")]
    public static CssTransform translate_y(CssLengthPercentageValue value)
        => CssTransform.create("translateY(" + StringValue(value.Value) + ")");

    /// <summary>Creates a two-axis translate transform。创建双轴 translate transform。</summary>
    public static CssTransform translate(CssLengthPercentageValue x, CssLengthPercentageValue y)
        => CssTransform.create("translate(" + StringValue(x.Value) + "," + StringValue(y.Value) + ")");

    /// <summary>Creates a rotate transform from a typed angle。根据类型化角度创建 rotate transform。</summary>
    public static CssTransform rotate(CssAngle angle)
        => CssTransform.create("rotate(" + StringValue(angle) + ")");

    /// <summary>Creates a uniform scale transform。创建统一缩放的 scale transform。</summary>
    public static CssTransform scale(double value)
        => CssTransform.create("scale(" + number(value) + ")");

    /// <summary>Creates a non-uniform scale transform with independent X and Y factors. 使用独立 X/Y 系数创建非均匀缩放 transform。</summary>
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
            output.Push(StringValue(item));
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

            parts.Push(StringValue(shadow.OffsetX.Value));
            parts.Push(StringValue(shadow.OffsetY.Value));

            var blur = shadow.Blur;
            if (blur is not null)
                parts.Push(StringValue(blur.Value.Value));

            var spread = shadow.Spread;
            if (spread is not null)
                parts.Push(StringValue(spread.Value.Value));

            var colorValue = shadow.Color;
            if (colorValue is not null)
                parts.Push(StringValue(colorValue.Value.Value));

            output.Push(parts.Join(" "));
        }

        return CssShadowList.create(output.Join(","));
    }

    /// <summary>Creates a positive CSS ratio and rejects zero or negative terms。创建正 CSS 比率，并拒绝零或负分量。</summary>
    public static CssRatio ratio(int numerator, int denominator = 1)
    {
        if (numerator <= 0 || denominator <= 0)
            Fail("CSS ratio terms must be greater than zero.");
        return CssRatio.create(StringValue(numerator) + " / " + StringValue(denominator));
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
            parts.Push(StringValue(width.Value.Value));
        if (style is not null)
            parts.Push(StringValue(style.Value.Value));
        if (color is not null)
            parts.Push(StringValue(color.Value.Value));
        return CssBorder.create(parts.Join(" "));
    }

    /// <summary>Creates a blur filter from a typed length。根据类型化长度创建 blur filter。</summary>
    public static CssFilter blur(CssLength value)
        => CssFilter.create("blur(" + StringValue(value) + ")");

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
            output.Push(StringValue(value));
        return CssFilter.create(output.Join(" "));
    }

    private static CssGradient gradient(string name, CssGradientStop[] stops)
    {
        if (stops.Length < 2)
            Fail("CSS gradient requires at least two stops.");

        var output = new Array<string>();
        foreach (var stop in stops)
        {
            var value = StringValue(stop.Color.Value);
            if (stop.From is not null)
                value += " " + StringValue(stop.From.Value.Value);
            if (stop.To is not null)
                value += " " + StringValue(stop.To.Value.Value);
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
        return StringValue(value);
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
            .Replace("\r", "\\d ")
            .Replace("\n", "\\a ") + "\"";

}
