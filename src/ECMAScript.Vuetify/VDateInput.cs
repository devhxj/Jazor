using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs 日期输入创作代理。
/// Vuetify labs date-input authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VDateInput")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(Save), VueEmitKind.LibrarySpecific, Name = "save")]
[VueLibraryEmit(nameof(Cancel), VueEmitKind.LibrarySpecific, Name = "cancel")]
[VueSlot(nameof(Prepend), Name = "prepend")]
[VueSlot(nameof(Append), Name = "append")]
[VueSlot(nameof(PrependInner), Name = "prepend-inner")]
[VueSlot(nameof(AppendInner), Name = "append-inner")]
[VueSlot(nameof(Clear), Name = "clear")]
[VueSlot(nameof(LabelContent), Name = "label")]
[VueSlot(nameof(Details), Name = "details")]
[VueSlot(nameof(CounterContent), Name = "counter")]
[VueSlot(nameof(Actions), Name = "actions")]
public sealed class VDateInput : VInputComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 允许选择的最小日期。
    /// Minimum selectable date.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerModelValue? Min { get; set; }

    /// <summary>
    /// 允许选择的最大日期。
    /// Maximum selectable date.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerModelValue? Max { get; set; }

    /// <summary>
    /// 取消按钮的文本。
    /// Text for the cancel button.
    /// </summary>
    [Parameter]
    public string? CancelText { get; set; }

    /// <summary>
    /// 确认按钮的文本。
    /// Text for the OK/confirm button.
    /// </summary>
    [Parameter]
    public string? OkText { get; set; }

    /// <summary>
    /// 是否隐藏操作按钮区域。
    /// Whether to hide the actions area.
    /// </summary>
    [Parameter]
    public bool HideActions { get; set; }

    /// <summary>
    /// 是否为移动端显示模式。
    /// Whether to display in mobile mode.
    /// </summary>
    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 移动端断点阈值。
    /// Breakpoint threshold for mobile mode.
    /// </summary>
    [Parameter]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    /// <summary>
    /// 日期的显示格式。
    /// Display format for the date.
    /// </summary>
    [Parameter]
    public VDateInputDisplayFormatValue? DisplayFormat { get; set; }

    /// <summary>
    /// 选择器的弹出位置。
    /// Popup location of the picker.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 保存时触发的事件回调。
    /// Event callback fired on save.
    /// </summary>
    [Parameter]
    public EventCallback<string> Save { get; set; }

    /// <summary>
    /// 取消时触发的事件回调。
    /// Event callback fired on cancel.
    /// </summary>
    [Parameter]
    public EventCallback Cancel { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 操作区域插槽内容。
    /// Slot content for the actions area.
    /// </summary>
    [Parameter]
    public RenderFragment<VDateInputActionsSlotContext>? Actions { get; set; }
}
