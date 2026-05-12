using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择系列组件共享的编写基类。
/// Shared Vuetify select-family authoring surface.
/// </summary>
public abstract class VSelectLikeComponentBase : ComponentBase
{
    /// <summary>
    /// 输入框的标签文本。
    /// The label text of the input.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 输入框的占位提示文本。
    /// The placeholder text of the input.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// 输入框的提示信息。
    /// The hint text of the input.
    /// </summary>
    [Parameter]
    public string? Hint { get; set; }

    /// <summary>
    /// 是否始终显示提示信息。
    /// Whether to always show the hint text.
    /// </summary>
    [Parameter]
    public bool PersistentHint { get; set; }

    /// <summary>
    /// 是否始终显示占位提示文本。
    /// Whether to always show the placeholder text.
    /// </summary>
    [Parameter]
    public bool PersistentPlaceholder { get; set; }

    /// <summary>
    /// 是否在非聚焦时仍然显示清除按钮。
    /// Whether to always show the clear button.
    /// </summary>
    [Parameter]
    public bool PersistentClear { get; set; }

    /// <summary>
    /// 输入框的前缀文本。
    /// The prefix text of the input.
    /// </summary>
    [Parameter]
    public string? Prefix { get; set; }

    /// <summary>
    /// 输入框的后缀文本。
    /// The suffix text of the input.
    /// </summary>
    [Parameter]
    public string? Suffix { get; set; }

    /// <summary>
    /// 输入元素的唯一标识符。
    /// The unique identifier for the input element.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 表单元素的 name 属性。
    /// The name attribute for the form element.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// 输入框的颜色。
    /// The color of the input.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 输入框的基础颜色。
    /// The base color of the input.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 输入框的背景颜色。
    /// The background color of the input.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否禁用输入框。
    /// Whether the input is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the input is read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否处于聚焦状态。
    /// Whether the input is focused.
    /// </summary>
    [Parameter]
    public bool Focused { get; set; }

    /// <summary>
    /// 是否处于错误状态。
    /// Whether the input is in an error state.
    /// </summary>
    [Parameter]
    public bool Error { get; set; }

    /// <summary>
    /// 错误状态下显示的消息。
    /// Messages displayed in the error state.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    /// <summary>
    /// 显示的提示消息。
    /// The hint messages to display.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 是否隐藏提示详细信息。
    /// Whether to hide the details/hints section.
    /// </summary>
    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 验证触发时机。
    /// When to trigger validation.
    /// </summary>
    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    /// <summary>
    /// 输入框前置图标的名称。
    /// The icon name prepended to the input.
    /// </summary>
    [Parameter]
    public string? PrependIcon { get; set; }

    /// <summary>
    /// 输入框后置图标的名称。
    /// The icon name appended to the input.
    /// </summary>
    [Parameter]
    public string? AppendIcon { get; set; }

    /// <summary>
    /// 输入框内部前置图标的名称。
    /// The icon name prepended inside the input.
    /// </summary>
    [Parameter]
    public string? PrependInnerIcon { get; set; }

    /// <summary>
    /// 清除按钮的图标名称。
    /// The icon name for the clear button.
    /// </summary>
    [Parameter]
    public string? ClearIcon { get; set; }

    /// <summary>
    /// 是否显示清除按钮。
    /// Whether to show the clear button.
    /// </summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>
    /// 输入框的外观变体。
    /// The visual variant of the input field.
    /// </summary>
    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 下拉选项的数据源。
    /// The data source for the dropdown items.
    /// </summary>
    [Parameter]
    public VuetifySelectItems? Items { get; set; }

    /// <summary>
    /// 指定选项对象中用作标题的字段名。
    /// The property name used as the item title.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    /// <summary>
    /// 指定选项对象中用作值的字段名。
    /// The property name used as the item value.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 指定选项对象中用作子级的字段名。
    /// The property name used as the item children.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    /// <summary>
    /// 指定传递给每个选项的额外属性。
    /// The selector for additional props passed to each item.
    /// </summary>
    [Parameter]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    /// <summary>
    /// 用于比较选项值的自定义比较器。
    /// The custom comparator for comparing item values.
    /// </summary>
    [Parameter]
    public VuetifySelectValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 是否支持多选。
    /// Whether to support multiple selections.
    /// </summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// 是否返回完整对象而非仅值。
    /// Whether to return the full object instead of just the value.
    /// </summary>
    [Parameter]
    public bool ReturnObject { get; set; }

    /// <summary>
    /// 是否以芯片样式显示选中项。
    /// Whether to display selected items as chips.
    /// </summary>
    [Parameter]
    public bool Chips { get; set; }

    /// <summary>
    /// 选中的芯片是否可关闭。
    /// Whether selected chips are closable.
    /// </summary>
    [Parameter]
    public bool ClosableChips { get; set; }

    /// <summary>
    /// 是否在首次挂载时就渲染下拉内容。
    /// Whether to eagerly render the dropdown content on mount.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否在无数据时隐藏下拉菜单。
    /// Whether to hide the dropdown when there is no data.
    /// </summary>
    [Parameter]
    public bool HideNoData { get; set; }

    /// <summary>
    /// 是否隐藏已选中的选项。
    /// Whether to hide already selected items from the dropdown.
    /// </summary>
    [Parameter]
    public bool HideSelected { get; set; }

    /// <summary>
    /// 传递给内部列表组件的属性。
    /// Props passed to the internal list component.
    /// </summary>
    [Parameter]
    public VueProps? ListProps { get; set; }

    /// <summary>
    /// 下拉菜单是否处于打开状态。
    /// Whether the dropdown menu is open.
    /// </summary>
    [Parameter]
    public bool Menu { get; set; }

    /// <summary>
    /// 菜单状态变更时触发的回调。
    /// Callback invoked when the menu state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> MenuChanged { get; set; }

    /// <summary>
    /// 菜单图标的名称。
    /// The icon name for the menu toggle.
    /// </summary>
    [Parameter]
    public string? MenuIcon { get; set; }

    /// <summary>
    /// 传递给内部菜单组件的属性。
    /// Props passed to the internal menu component.
    /// </summary>
    [Parameter]
    public VueProps? MenuProps { get; set; }

    /// <summary>
    /// 无数据时显示的文本。
    /// The text displayed when there is no data.
    /// </summary>
    [Parameter]
    public string? NoDataText { get; set; }

    /// <summary>
    /// 清除选择后是否自动打开下拉菜单。
    /// Whether to open the dropdown after clearing the selection.
    /// </summary>
    [Parameter]
    public bool OpenOnClear { get; set; }

    /// <summary>
    /// 关闭下拉菜单的辅助文本。
    /// The accessibility text for closing the dropdown.
    /// </summary>
    [Parameter]
    public string? CloseText { get; set; }

    /// <summary>
    /// 打开下拉菜单的辅助文本。
    /// The accessibility text for opening the dropdown.
    /// </summary>
    [Parameter]
    public string? OpenText { get; set; }

    /// <summary>
    /// 下拉选项的颜色。
    /// The color of the dropdown items.
    /// </summary>
    [Parameter]
    public string? ItemColor { get; set; }

    /// <summary>
    /// 聚焦状态变更时触发的回调。
    /// Callback invoked when the focus state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    /// <summary>
    /// 自定义下拉选项渲染的插槽。
    /// Custom slot for rendering dropdown items.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectItemSlotContext>? Item { get; set; }

    /// <summary>
    /// 自定义选中芯片渲染的插槽。
    /// Custom slot for rendering selected chips.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectChipSlotContext>? Chip { get; set; }

    /// <summary>
    /// 自定义选中项渲染的插槽。
    /// Custom slot for rendering selected items.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectSelectionSlotContext>? Selection { get; set; }

    /// <summary>
    /// 下拉列表前置内容的插槽。
    /// Slot for content prepended to the dropdown list.
    /// </summary>
    [Parameter]
    public RenderFragment? PrependItem { get; set; }

    /// <summary>
    /// 下拉列表后置内容的插槽。
    /// Slot for content appended to the dropdown list.
    /// </summary>
    [Parameter]
    public RenderFragment? AppendItem { get; set; }

    /// <summary>
    /// 无数据时显示的自定义内容。
    /// Custom content displayed when there is no data.
    /// </summary>
    [Parameter]
    public RenderFragment? NoData { get; set; }
}
