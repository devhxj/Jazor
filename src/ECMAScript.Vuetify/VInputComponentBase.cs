using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// 共享的 Vuetify 文本输入创作基类，提供字段和输入属性。
/// Shared Vuetify text input authoring surface for field and input props.
/// </summary>
public abstract class VInputComponentBase : ComponentBase
{
    /// <summary>
    /// 输入元素的唯一标识符。
    /// Unique identifier for the input element.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 输入元素的 name 属性。
    /// Name attribute for the input element.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// 输入框的标签文本。
    /// Label text for the input field.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 输入框的占位符文本。
    /// Placeholder text for the input field.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// 输入框的提示文本。
    /// Hint text for the input field.
    /// </summary>
    [Parameter]
    public string? Hint { get; set; }

    /// <summary>
    /// 是否始终显示提示文本。
    /// Whether to always show the hint text.
    /// </summary>
    [Parameter]
    public bool PersistentHint { get; set; }

    /// <summary>
    /// 是否始终显示占位符文本。
    /// Whether to always show the placeholder text.
    /// </summary>
    [Parameter]
    public bool PersistentPlaceholder { get; set; }

    /// <summary>
    /// 输入框的前缀文本。
    /// Prefix text for the input field.
    /// </summary>
    [Parameter]
    public string? Prefix { get; set; }

    /// <summary>
    /// 输入框的后缀文本。
    /// Suffix text for the input field.
    /// </summary>
    [Parameter]
    public string? Suffix { get; set; }

    /// <summary>
    /// 输入框的主题颜色。
    /// Theme color of the input field.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 输入框的基础颜色。
    /// Base color of the input field.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 输入框的背景颜色。
    /// Background color of the input field.
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
    /// 是否将输入框设为只读。
    /// Whether the input is read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 输入框是否处于聚焦状态。
    /// Whether the input is focused.
    /// </summary>
    [Parameter]
    public bool Focused { get; set; }

    /// <summary>
    /// 聚焦状态变化时的回调。
    /// Callback when the focused state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    /// <summary>
    /// 是否将输入框置于错误状态。
    /// Whether the input is in an error state.
    /// </summary>
    [Parameter]
    public bool Error { get; set; }

    /// <summary>
    /// 输入框的错误消息。
    /// Error messages for the input field.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    /// <summary>
    /// 输入框的提示消息。
    /// Hint messages for the input field.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 是否隐藏输入框的详情区域。
    /// Whether to hide the details area of the input field.
    /// </summary>
    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 输入验证的触发时机。
    /// When to trigger input validation.
    /// </summary>
    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    /// <summary>
    /// 输入框外层前置图标。
    /// Prepend icon outside the input field.
    /// </summary>
    [Parameter]
    public string? PrependIcon { get; set; }

    /// <summary>
    /// 输入框外层后置图标。
    /// Append icon outside the input field.
    /// </summary>
    [Parameter]
    public string? AppendIcon { get; set; }

    /// <summary>
    /// 输入框内层前置图标。
    /// Prepend icon inside the input field.
    /// </summary>
    [Parameter]
    public string? PrependInnerIcon { get; set; }

    /// <summary>
    /// 输入框内层后置图标。
    /// Append icon inside the input field.
    /// </summary>
    [Parameter]
    public string? AppendInnerIcon { get; set; }

    /// <summary>
    /// 清除按钮的图标。
    /// Icon for the clear button.
    /// </summary>
    [Parameter]
    public string? ClearIcon { get; set; }

    /// <summary>
    /// 是否允许清除输入内容。
    /// Whether the input content can be cleared.
    /// </summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>
    /// 是否始终显示清除按钮。
    /// Whether to always show the clear icon.
    /// </summary>
    [Parameter]
    public bool PersistentClear { get; set; }

    /// <summary>
    /// 输入值是否已被修改。
    /// Whether the input value has been modified.
    /// </summary>
    [Parameter]
    public bool Dirty { get; set; }

    /// <summary>
    /// 输入框的外观变体。
    /// Visual variant of the input field.
    /// </summary>
    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    /// <summary>
    /// 输入框的紧凑程度。
    /// Density/compactness of the input field.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 输入框的圆角样式。
    /// Border radius style of the input field.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否显示字符计数器。
    /// Whether to show a character counter.
    /// </summary>
    [Parameter]
    public VuetifyCounterValue? Counter { get; set; }

    /// <summary>
    /// 字符计数器的自定义值来源。
    /// Custom value source for the character counter.
    /// </summary>
    [Parameter]
    public VuetifyCounterValueSource? CounterValue { get; set; }

    /// <summary>
    /// 输入框的绑定值。
    /// Bound value of the input field.
    /// </summary>
    [Parameter]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变化时的回调。
    /// Callback when the bound value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 文本输入的模型修饰符。
    /// Model modifiers for text input.
    /// </summary>
    [Parameter]
    public VuetifyTextModelModifiers? ModelModifiers { get; set; }

    /// <summary>
    /// 输入框外层前置插槽内容。
    /// Prepend slot content outside the input field.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? Prepend { get; set; }

    /// <summary>
    /// 输入框外层后置插槽内容。
    /// Append slot content outside the input field.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? Append { get; set; }

    /// <summary>
    /// 输入框内层前置插槽内容。
    /// Prepend slot content inside the input field.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? PrependInner { get; set; }

    /// <summary>
    /// 输入框内层后置插槽内容。
    /// Append slot content inside the input field.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? AppendInner { get; set; }

    /// <summary>
    /// 清除按钮的插槽内容。
    /// Slot content for the clear button.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? Clear { get; set; }

    /// <summary>
    /// 标签的自定义插槽内容。
    /// Custom slot content for the label.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? LabelContent { get; set; }

    /// <summary>
    /// 详情区域的自定义插槽内容。
    /// Custom slot content for the details area.
    /// </summary>
    [Parameter]
    public RenderFragment<VInputDetailsSlotContext>? Details { get; set; }

    /// <summary>
    /// 计数器的自定义插槽内容。
    /// Custom slot content for the counter.
    /// </summary>
    [Parameter]
    public RenderFragment<VCounterSlotContext>? CounterContent { get; set; }
}
