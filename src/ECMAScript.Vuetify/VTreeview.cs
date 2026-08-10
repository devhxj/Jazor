using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室树形视图组件的创作代理。
/// Vuetify labs treeview authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VTreeview")]
public sealed class VTreeview : ComponentBase
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyTreeviewValues? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyTreeviewValues?> ModelValueChanged { get; set; }

    /// <summary>
    /// 项。
    /// Tree items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("items")]
    public VuetifyTreeviewItems? Items { get; set; }

    /// <summary>
    /// 项标题字段。
    /// Item title field.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemTitle")]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    /// <summary>
    /// 项值字段。
    /// Item value field.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemValue")]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 子项字段。
    /// Item children field.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemChildren")]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    /// <summary>
    /// 项属性。
    /// Item props.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemProps")]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    /// <summary>
    /// 返回对象。
    /// Returns the selected item object instead of its value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("returnObject")]
    public bool ReturnObject { get; set; }

    /// <summary>
    /// 激活项。
    /// Activated items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activated")]
    public VuetifyTreeviewValues? Activated { get; set; }

    /// <summary>
    /// 激活项变化事件。
    /// Activated changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:activated")]
    public EventCallback<VuetifyTreeviewValues?> ActivatedChanged { get; set; }

    /// <summary>
    /// 选中项。
    /// Selected items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selected")]
    public VuetifyTreeviewValues? Selected { get; set; }

    /// <summary>
    /// 选中项变化事件。
    /// Selected changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:selected")]
    public EventCallback<VuetifyTreeviewValues?> SelectedChanged { get; set; }

    /// <summary>
    /// 已展开项。
    /// Opened items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("opened")]
    public VuetifyTreeviewValues? Opened { get; set; }

    /// <summary>
    /// 已展开项变化事件。
    /// Opened changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:opened")]
    public EventCallback<VuetifyTreeviewValues?> OpenedChanged { get; set; }

    /// <summary>
    /// 强制选中。
    /// Mandatory selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public bool Mandatory { get; set; }

    /// <summary>
    /// 可激活。
    /// Activatable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activatable")]
    public bool Activatable { get; set; }

    /// <summary>
    /// 可选。
    /// Selectable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectable")]
    public bool Selectable { get; set; }

    /// <summary>
    /// 选择策略。
    /// Active strategy.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeStrategy")]
    public VuetifyTreeviewActiveStrategyValue? ActiveStrategy { get; set; }

    /// <summary>
    /// 选择策略。
    /// Select strategy.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectStrategy")]
    public VuetifyTreeviewSelectStrategyValue? SelectStrategy { get; set; }

    /// <summary>
    /// 加载子项。
    /// Load children callback.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loadChildren")]
    public VuetifyTreeviewLoadChildrenCallback? LoadChildren { get; set; }

    /// <summary>
    /// 点击打开。
    /// Opens items on click.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnClick")]
    public bool? OpenOnClick { get; set; }

    /// <summary>
    /// 全部展开。
    /// Opens all items initially.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openAll")]
    public bool OpenAll { get; set; }

    /// <summary>
    /// 搜索值。
    /// Search value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("search")]
    public string? Search { get; set; }

    /// <summary>
    /// CSS类。
    /// CSS class.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 行内样式。
    /// Inline style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 自定义过滤。
    /// Custom filter function.
    /// </summary>
    [Parameter]
    [ECMAScriptName("customFilter")]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    /// <summary>
    /// 自定义键过滤。
    /// Custom key filter functions.
    /// </summary>
    [Parameter]
    [ECMAScriptName("customKeyFilter")]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    /// <summary>
    /// 过滤键。
    /// Filter keys.
    /// </summary>
    [Parameter]
    [ECMAScriptName("filterKeys")]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    /// <summary>
    /// 过滤模式。
    /// Filter mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("filterMode")]
    public VuetifyFilterMode? FilterMode { get; set; }

    /// <summary>
    /// 禁用过滤。
    /// Disables filtering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noFilter")]
    public bool NoFilter { get; set; }

    /// <summary>
    /// 折叠图标。
    /// Collapse icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("collapseIcon")]
    public VuetifyIconValue? CollapseIcon { get; set; }

    /// <summary>
    /// 展开图标。
    /// Expand icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("expandIcon")]
    public VuetifyIconValue? ExpandIcon { get; set; }

    /// <summary>
    /// 不确定状态图标。
    /// Indeterminate icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("indeterminateIcon")]
    public VuetifyIconValue? IndeterminateIcon { get; set; }

    /// <summary>
    /// 未选中图标。
    /// False icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("falseIcon")]
    public VuetifyIconValue? FalseIcon { get; set; }

    /// <summary>
    /// 已选中图标。
    /// True icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("trueIcon")]
    public VuetifyIconValue? TrueIcon { get; set; }

    /// <summary>
    /// 加载图标。
    /// Loading icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loadingIcon")]
    public string? LoadingIcon { get; set; }

    /// <summary>
    /// 选中颜色。
    /// Selected color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedColor")]
    public string? SelectedColor { get; set; }

    /// <summary>
    /// 激活颜色。
    /// Active color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeColor")]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 激活CSS类。
    /// Active CSS class.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeClass")]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// 基础颜色。
    /// Base color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 背景颜色。
    /// Background color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 主题颜色。
    /// Theme color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 变体。
    /// Variant.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 紧凑度。
    /// Density.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 线条样式。
    /// Lines style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lines")]
    public VuetifyListLines? Lines { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the treeview.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 纤细。
    /// Slim mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("slim")]
    public bool Slim { get; set; }

    /// <summary>
    /// 流体宽度。
    /// Fluid width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fluid")]
    public bool Fluid { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角。
    /// Border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 边框。
    /// Border.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 阴影。
    /// Elevation shadow.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 高。
    /// Height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 宽。
    /// Width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 最大高。
    /// Maximum height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 最大宽。
    /// Maximum width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 最小高。
    /// Minimum height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 最小宽。
    /// Minimum width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 值比较器。
    /// Value comparator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("valueComparator")]
    public VuetifySelectValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 展开点击事件。
    /// Open click event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick:open")]
    public EventCallback<VuetifyTreeviewClickPayload> OnOpenClick { get; set; }

    /// <summary>
    /// 选中点击事件。
    /// Select click event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick:select")]
    public EventCallback<VuetifyTreeviewClickPayload> OnSelectClick { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 前置插槽。
    /// Prepend slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prepend")]
    public RenderFragment<VTreeviewNodeSlotContext>? Prepend { get; set; }

    /// <summary>
    /// 后置插槽。
    /// Append slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("append")]
    public RenderFragment<VTreeviewNodeSlotContext>? Append { get; set; }

    /// <summary>
    /// 标题内容插槽。
    /// Title content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment<VTreeviewTitleSlotContext>? TitleContent { get; set; }

    /// <summary>
    /// 副标题内容插槽。
    /// Subtitle content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("subtitle")]
    public RenderFragment<VTreeviewSubtitleSlotContext>? SubtitleContent { get; set; }

    /// <summary>
    /// 项内容插槽。
    /// Item content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("item")]
    public RenderFragment<VTreeviewItemSlotContext>? ItemContent { get; set; }

    /// <summary>
    /// 头部插槽。
    /// Header slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header")]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Header { get; set; }

    /// <summary>
    /// 分隔线插槽。
    /// Divider slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("divider")]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Divider { get; set; }

    /// <summary>
    /// 子标题插槽。
    /// Subheader slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("subheader")]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Subheader { get; set; }
}
