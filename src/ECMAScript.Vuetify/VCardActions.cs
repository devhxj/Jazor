using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VCardActions")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
/// <summary>
/// Vuetify 卡片操作区域组件。
/// Vuetify card actions section component.
/// </summary>
public sealed class VCardActions : ComponentBase, IVueLibraryComponent
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
