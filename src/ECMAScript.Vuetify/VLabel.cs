using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签组件，用于表单控件标签显示。
/// Vuetify label component for form control label display.
/// </summary>
[VueLibraryComponent("vuetify/components", "VLabel")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VLabel : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
