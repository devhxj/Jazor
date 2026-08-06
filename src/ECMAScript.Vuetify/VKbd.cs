using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 键盘输入样式组件。
/// Vuetify keyboard input styling component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VKbd", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VKbd : ComponentBase
{
    /// <summary>
    /// 组件的 HTML 标签名。
    /// HTML tag name for the component.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
