using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签页组件的编写代理。
/// Vuetify tab authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTab")]
public sealed class VTab : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 标签页的文本内容。
    /// Text content of the tab.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// 标签页的值。
    /// Value of the tab.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
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
