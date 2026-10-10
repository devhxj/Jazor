using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 文件输入组件。
/// Vuetify file-input component.
/// </summary>
[ECMAScript("vuetify/components/VFileInput")]
public sealed class VFileInput : ComponentBase, IVuetifyComponent
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
    /// <remarks data-authoring="types">C# union VuetifyFileShowSizeValue?；值域为 bool | VuetifyFileSizeBase。变量用 ShowSize="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsBase；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VuetifyHideDetailsValue?；值域为 bool | VuetifyHideDetailsMode。变量用 HideDetails="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("hideDetails")]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 显示在输入框下方的消息。
    /// Messages displayed below the input.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMessagesValue?；值域为 string | string[]。变量用 Messages="@value"，并保持声明的分支类型。字符串用 Messages="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("messages")]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 文件输入的绑定值。
    /// Bound value of the file input.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyFileModelValue?；值域为 FileRef | FileRef[]。变量用 ModelValue="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsFile、value?.AsFiles；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyFileModelValue? ModelValue { get; set; }

    /// <summary>
    /// 文件输入绑定值变化时的回调。
    /// Callback when the file input value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyFileModelValue?；保持与模型相同的强类型。投影：value?.AsFile、value?.AsFiles；不匹配返回 null，0/false 保留。</remarks>
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
