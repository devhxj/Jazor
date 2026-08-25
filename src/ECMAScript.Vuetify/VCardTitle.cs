using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VCardTitle")]
/// <summary>
/// Vuetify 卡片标题组件。
/// Vuetify card title component.
/// </summary>
public sealed class VCardTitle : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 附加到组件根元素的额外属性。
    /// Additional attributes applied to the component root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 组件的子内容。
    /// Child content of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
