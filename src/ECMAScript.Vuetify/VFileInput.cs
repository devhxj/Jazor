using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 文件输入组件。
/// Vuetify file-input component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VFileInput")]
public sealed class VFileInput : ComponentBase
{
    /// <summary>
    /// 输入框的标签文本。
    /// Label text of the input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 限制可选择的文件类型。
    /// Accepted file types for the input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("accept")]
    public string? Accept { get; set; }

    /// <summary>
    /// 是否以芯片样式显示已选文件。
    /// Whether to display selected files as chips.
    /// </summary>
    [Parameter]
    [ECMAScriptName("chips")]
    public bool Chips { get; set; }

    /// <summary>
    /// 是否显示已选文件数量。
    /// Whether to show the count of selected files.
    /// </summary>
    [Parameter]
    [ECMAScriptName("counter")]
    public bool Counter { get; set; }

    /// <summary>
    /// 是否显示文件大小及显示方式。
    /// Whether and how to show file sizes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showSize")]
    public VuetifyFileShowSizeValue? ShowSize { get; set; }

    /// <summary>
    /// 是否允许选择多个文件。
    /// Whether to allow selecting multiple files.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// 是否显示清除按钮。
    /// Whether the input is clearable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("clearable")]
    public bool Clearable { get; set; }

    /// <summary>
    /// 是否禁用文件输入。
    /// Whether the file input is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否将文件输入设为只读。
    /// Whether the file input is read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 输入框的紧凑程度。
    /// Density of the input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 输入框的外观变体。
    /// Visual variant of the input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyFieldVariant? Variant { get; set; }

    /// <summary>
    /// 空白时的占位文本。
    /// Placeholder text when empty.
    /// </summary>
    [Parameter]
    [ECMAScriptName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// 输入框的提示文本。
    /// Hint text for the input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hint")]
    public string? Hint { get; set; }

    /// <summary>
    /// 是否始终显示提示文本。
    /// Whether to always show the hint text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("persistentHint")]
    public bool PersistentHint { get; set; }

    /// <summary>
    /// 是否隐藏验证提示及隐藏方式。
    /// Whether and how to hide validation details.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDetails")]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 显示在输入框下方的消息。
    /// Messages displayed below the input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("messages")]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 文件输入的绑定值。
    /// Bound value of the file input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyFileModelValue? ModelValue { get; set; }

    /// <summary>
    /// 文件输入绑定值变化时的回调。
    /// Callback when the file input value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyFileModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
