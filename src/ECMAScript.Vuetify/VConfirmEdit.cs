using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 确认编辑创作代理，用于可编辑值的确认流程。
/// Vuetify confirm-edit authoring proxy for editable value confirmation flows.
/// </summary>
[VueLibraryComponent("vuetify/components", "VConfirmEdit")]
public sealed class VConfirmEdit : ComponentBase
{
    /// <summary>
    /// 编辑绑定值。
    /// The bound value being edited.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VueValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 操作按钮的主题色。
    /// The theme color for action buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 取消按钮文本。
    /// The text for the cancel button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("cancelText")]
    public string? CancelText { get; set; }

    /// <summary>
    /// 确认按钮文本。
    /// The text for the confirm button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("okText")]
    public string? OkText { get; set; }

    /// <summary>
    /// 禁用配置。
    /// The disabled configuration.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public VuetifyConfirmEditDisabled? Disabled { get; set; }

    /// <summary>
    /// 是否隐藏操作按钮。
    /// Whether to hide the action buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideActions")]
    public bool HideActions { get; set; }

    /// <summary>
    /// 保存事件回调。
    /// Callback invoked when the value is saved.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onSave")]
    public EventCallback<VueValue?> OnSave { get; set; }

    /// <summary>
    /// 取消事件回调。
    /// Callback invoked when editing is cancelled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onCancel")]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VConfirmEditSlotContext>? ChildContent { get; set; }
}
