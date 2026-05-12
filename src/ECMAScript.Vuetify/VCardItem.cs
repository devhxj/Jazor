using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VCardItem")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
/// <summary>
/// Vuetify 卡片项分组组件，用于组织标题、副标题和前后缀。
/// Vuetify card item grouping component for organizing title, subtitle, and prepend/append content.
/// </summary>
public sealed class VCardItem : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? AppendAvatar { get; set; }

    [Parameter]
    public string? AppendIcon { get; set; }

    [Parameter]
    public string? PrependAvatar { get; set; }

    [Parameter]
    public string? PrependIcon { get; set; }

    [Parameter]
    public VuetifyTextValue? Subtitle { get; set; }

    [Parameter]
    public VuetifyTextValue? Title { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? Prepend { get; set; }

    [Parameter]
    public RenderFragment? Append { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    [Parameter]
    public RenderFragment? SubtitleContent { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
