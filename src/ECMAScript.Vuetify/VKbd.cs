using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 键盘输入样式组件。
/// Vuetify keyboard input styling component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VKbd")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VKbd : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Tag { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
