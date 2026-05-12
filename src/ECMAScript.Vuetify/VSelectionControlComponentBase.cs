using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择控件组件共享的编写基类。
/// Shared Vuetify selection-control authoring surface.
/// </summary>
public abstract class VSelectionControlComponentBase : ComponentBase
{
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
    /// 控件的标签文本。
    /// The label text of the control.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 控件的颜色。
    /// The color of the control.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 控件的基础颜色。
    /// The base color of the control.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 控件的背景颜色。
    /// The background color of the control.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用控件。
    /// Whether the control is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the control is read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否处于错误状态。
    /// Whether the control is in an error state.
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
    /// 是否处于聚焦状态。
    /// Whether the control is focused.
    /// </summary>
    [Parameter]
    public bool Focused { get; set; }

    /// <summary>
    /// 聚焦状态变更时触发的回调。
    /// Callback invoked when the focus state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    /// <summary>
    /// 值是否已被修改。
    /// Whether the value has been modified.
    /// </summary>
    [Parameter]
    public bool Dirty { get; set; }

    /// <summary>
    /// 控件的当前绑定值。
    /// The current bound value of the control.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更时触发的回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 控件的提交值。
    /// The submission value of the control.
    /// </summary>
    [Parameter]
    public VueValue? Value { get; set; }

    /// <summary>
    /// 选中时对应的值。
    /// The value when the control is checked.
    /// </summary>
    [Parameter]
    public VueValue? TrueValue { get; set; }

    /// <summary>
    /// 未选中时对应的值。
    /// The value when the control is unchecked.
    /// </summary>
    [Parameter]
    public VueValue? FalseValue { get; set; }

    /// <summary>
    /// 是否处于不确定状态。
    /// Whether the control is in an indeterminate state.
    /// </summary>
    [Parameter]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// 是否将控件横向排列。
    /// Whether to display controls inline horizontally.
    /// </summary>
    [Parameter]
    public bool Inline { get; set; }

    /// <summary>
    /// 是否支持多选。
    /// Whether to support multiple selections.
    /// </summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// 未选中状态下显示的图标。
    /// The icon displayed when unchecked.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? FalseIcon { get; set; }

    /// <summary>
    /// 选中状态下显示的图标。
    /// The icon displayed when checked.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? TrueIcon { get; set; }

    /// <summary>
    /// 不确定状态下显示的图标。
    /// The icon displayed when indeterminate.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? IndeterminateIcon { get; set; }

    /// <summary>
    /// 输入区域默认插槽内容。
    /// The default slot content of the input area.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectionControlInputDefaultSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 自定义标签内容的插槽。
    /// Custom content for the label slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectionControlLabelSlotContext>? LabelContent { get; set; }

    /// <summary>
    /// 输入框前置内容的插槽。
    /// Slot for content prepended to the input.
    /// </summary>
    [Parameter]
    public RenderFragment<VInputSlotContext>? Prepend { get; set; }

    /// <summary>
    /// 输入框后置内容的插槽。
    /// Slot for content appended to the input.
    /// </summary>
    [Parameter]
    public RenderFragment<VInputSlotContext>? Append { get; set; }

    /// <summary>
    /// 详细信息区域的自定义插槽。
    /// Custom slot for the details section.
    /// </summary>
    [Parameter]
    public RenderFragment<VInputDetailsSlotContext>? Details { get; set; }

    /// <summary>
    /// 单条消息的自定义插槽。
    /// Custom slot for individual messages.
    /// </summary>
    [Parameter]
    public RenderFragment<VMessagesMessageSlotContext>? Message { get; set; }

    /// <summary>
    /// 自定义输入元素内容的插槽。
    /// Custom content for the input element slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectionControlInputSlotContext>? Input { get; set; }
}
