using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VCardText", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
/// <summary>
/// Vuetify 卡片文本区域组件。
/// Vuetify card text section component.
/// </summary>
public sealed class VCardText : ComponentBase
{
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
