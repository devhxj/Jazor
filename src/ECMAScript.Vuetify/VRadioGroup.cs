using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 单选按钮组组件。
/// Vuetify radio group component.
/// </summary>
[ECMAScript("vuetify/components/VRadioGroup")]
public sealed class VRadioGroup : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 单选按钮组的标签文本。
    /// The label text of the radio group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 单选按钮组的颜色。
    /// The color of the radio group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the radio group is read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否隐藏提示详细信息。
    /// Whether to hide the details/hints section.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyHideDetailsValue?；值域为 bool | VuetifyHideDetailsMode。变量用 HideDetails="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("hideDetails")]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 显示的提示消息。
    /// The hint messages to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMessagesValue?；值域为 string | string[]。变量用 Messages="@value"，并保持声明的分支类型。字符串用 Messages="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("messages")]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 是否禁用整个单选按钮组。
    /// Whether the entire radio group is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否将单选按钮横向排列。
    /// Whether to display radio buttons inline horizontally.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inline")]
    public bool Inline { get; set; }

    /// <summary>
    /// 当前选中的值。
    /// The currently selected value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 选中值变更时触发的回调。
    /// Callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容，用于放置单选按钮。
    /// The default slot for placing radio buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
