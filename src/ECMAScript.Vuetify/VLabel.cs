using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签组件，用于表单控件标签显示。
/// Vuetify label component for form control label display.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VLabel")]
public sealed class VLabel : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 标签显示的文本内容。
    /// Text content displayed by the label.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 点击标签时触发的回调。
    /// Callback invoked when the label is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick")]
    public EventCallback<MouseEvent> OnClick { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
