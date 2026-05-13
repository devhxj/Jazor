using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室树形视图组件的创作代理。
/// Vuetify labs treeview authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VTreeview")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ActivatedChanged), VueEmitKind.ModelUpdate, Name = "update:activated")]
[VueLibraryEmit(nameof(SelectedChanged), VueEmitKind.ModelUpdate, Name = "update:selected")]
[VueLibraryEmit(nameof(OpenedChanged), VueEmitKind.ModelUpdate, Name = "update:opened")]
[VueLibraryEmit(nameof(OpenClicked), VueEmitKind.LibrarySpecific, Name = "click:open")]
[VueLibraryEmit(nameof(SelectClicked), VueEmitKind.LibrarySpecific, Name = "click:select")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
[VueLibrarySlot(nameof(ItemContent), Name = "item")]
[VueLibrarySlot(nameof(Header), Name = "header")]
[VueLibrarySlot(nameof(Divider), Name = "divider")]
[VueLibrarySlot(nameof(Subheader), Name = "subheader")]
public sealed class VTreeview : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewValues? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> ModelValueChanged { get; set; }

    /// <summary>
    /// 项。
    /// Tree items.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewItems? Items { get; set; }

    /// <summary>
    /// 项标题字段。
    /// Item title field.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    /// <summary>
    /// 项值字段。
    /// Item value field.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 子项字段。
    /// Item children field.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    /// <summary>
    /// 项属性。
    /// Item props.
    /// </summary>
    [Parameter]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    /// <summary>
    /// 返回对象。
    /// Returns the selected item object instead of its value.
    /// </summary>
    [Parameter]
    public bool ReturnObject { get; set; }

    /// <summary>
    /// 激活项。
    /// Activated items.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewValues? Activated { get; set; }

    /// <summary>
    /// 激活项变化事件。
    /// Activated changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> ActivatedChanged { get; set; }

    /// <summary>
    /// 选中项。
    /// Selected items.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewValues? Selected { get; set; }

    /// <summary>
    /// 选中项变化事件。
    /// Selected changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> SelectedChanged { get; set; }

    /// <summary>
    /// 已展开项。
    /// Opened items.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewValues? Opened { get; set; }

    /// <summary>
    /// 已展开项变化事件。
    /// Opened changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> OpenedChanged { get; set; }

    /// <summary>
    /// 强制选中。
    /// Mandatory selection.
    /// </summary>
    [Parameter]
    public bool Mandatory { get; set; }

    /// <summary>
    /// 可激活。
    /// Activatable.
    /// </summary>
    [Parameter]
    public bool Activatable { get; set; }

    /// <summary>
    /// 可选。
    /// Selectable.
    /// </summary>
    [Parameter]
    public bool Selectable { get; set; }

    /// <summary>
    /// 选择策略。
    /// Active strategy.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewActiveStrategyValue? ActiveStrategy { get; set; }

    /// <summary>
    /// 选择策略。
    /// Select strategy.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewSelectStrategyValue? SelectStrategy { get; set; }

    /// <summary>
    /// 加载子项。
    /// Load children callback.
    /// </summary>
    [Parameter]
    public VuetifyTreeviewLoadChildrenCallback? LoadChildren { get; set; }

    /// <summary>
    /// 点击打开。
    /// Opens items on click.
    /// </summary>
    [Parameter]
    public bool? OpenOnClick { get; set; }

    /// <summary>
    /// 全部展开。
    /// Opens all items initially.
    /// </summary>
    [Parameter]
    public bool OpenAll { get; set; }

    /// <summary>
    /// 搜索值。
    /// Search value.
    /// </summary>
    [Parameter]
    public string? Search { get; set; }

    /// <summary>
    /// CSS类。
    /// CSS class.
    /// </summary>
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 行内样式。
    /// Inline style.
    /// </summary>
    [Parameter]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 自定义过滤。
    /// Custom filter function.
    /// </summary>
    [Parameter]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    /// <summary>
    /// 自定义键过滤。
    /// Custom key filter functions.
    /// </summary>
    [Parameter]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    /// <summary>
    /// 过滤键。
    /// Filter keys.
    /// </summary>
    [Parameter]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    /// <summary>
    /// 过滤模式。
    /// Filter mode.
    /// </summary>
    [Parameter]
    public VuetifyFilterMode? FilterMode { get; set; }

    /// <summary>
    /// 禁用过滤。
    /// Disables filtering.
    /// </summary>
    [Parameter]
    public bool NoFilter { get; set; }

    /// <summary>
    /// 折叠图标。
    /// Collapse icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? CollapseIcon { get; set; }

    /// <summary>
    /// 展开图标。
    /// Expand icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? ExpandIcon { get; set; }

    /// <summary>
    /// 不确定状态图标。
    /// Indeterminate icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? IndeterminateIcon { get; set; }

    /// <summary>
    /// 未选中图标。
    /// False icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? FalseIcon { get; set; }

    /// <summary>
    /// 已选中图标。
    /// True icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? TrueIcon { get; set; }

    /// <summary>
    /// 加载图标。
    /// Loading icon.
    /// </summary>
    [Parameter]
    public string? LoadingIcon { get; set; }

    /// <summary>
    /// 选中颜色。
    /// Selected color.
    /// </summary>
    [Parameter]
    public string? SelectedColor { get; set; }

    /// <summary>
    /// 激活颜色。
    /// Active color.
    /// </summary>
    [Parameter]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 激活CSS类。
    /// Active CSS class.
    /// </summary>
    [Parameter]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// 基础颜色。
    /// Base color.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 背景颜色。
    /// Background color.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 主题颜色。
    /// Theme color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 变体。
    /// Variant.
    /// </summary>
    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 紧凑度。
    /// Density.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 线条样式。
    /// Lines style.
    /// </summary>
    [Parameter]
    public VuetifyListLines? Lines { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the treeview.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 纤细。
    /// Slim mode.
    /// </summary>
    [Parameter]
    public bool Slim { get; set; }

    /// <summary>
    /// 流体宽度。
    /// Fluid width.
    /// </summary>
    [Parameter]
    public bool Fluid { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角。
    /// Border radius.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 边框。
    /// Border.
    /// </summary>
    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 阴影。
    /// Elevation shadow.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 高。
    /// Height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 宽。
    /// Width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 最大高。
    /// Maximum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 最大宽。
    /// Maximum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 最小高。
    /// Minimum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 最小宽。
    /// Minimum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 值比较器。
    /// Value comparator.
    /// </summary>
    [Parameter]
    public VuetifySelectValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 展开点击事件。
    /// Open click event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTreeviewClickPayload> OpenClicked { get; set; }

    /// <summary>
    /// 选中点击事件。
    /// Select click event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTreeviewClickPayload> SelectClicked { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 前置插槽。
    /// Prepend slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewNodeSlotContext>? Prepend { get; set; }

    /// <summary>
    /// 后置插槽。
    /// Append slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewNodeSlotContext>? Append { get; set; }

    /// <summary>
    /// 标题内容插槽。
    /// Title content slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewTitleSlotContext>? TitleContent { get; set; }

    /// <summary>
    /// 副标题内容插槽。
    /// Subtitle content slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewSubtitleSlotContext>? SubtitleContent { get; set; }

    /// <summary>
    /// 项内容插槽。
    /// Item content slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewItemSlotContext>? ItemContent { get; set; }

    /// <summary>
    /// 头部插槽。
    /// Header slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Header { get; set; }

    /// <summary>
    /// 分隔线插槽。
    /// Divider slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Divider { get; set; }

    /// <summary>
    /// 子标题插槽。
    /// Subheader slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Subheader { get; set; }
}
