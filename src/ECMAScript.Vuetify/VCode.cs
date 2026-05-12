using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 代码组件创作代理，用于内联或块级代码容器。
/// Vuetify code authoring proxy for inline or block code containers.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCode")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VCode : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 组件的 HTML 标签名。
    /// HTML tag name for the component.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 附加到组件根元素的额外属性。
    /// Additional attributes applied to the component root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 组件的子内容。
    /// Child content of the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
