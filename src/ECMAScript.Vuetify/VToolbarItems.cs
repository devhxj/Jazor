using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 工具栏项目组件的编写代理，用于分组工具栏操作。
/// Vuetify toolbar-items authoring proxy for grouped toolbar actions.
/// </summary>
[VueLibraryComponent("vuetify/components", "VToolbarItems")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VToolbarItems : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 主题颜色。
    /// Theme color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 变体。
    /// Variant.
    /// </summary>
    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// CSS类。
    /// CSS class.
    /// </summary>
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 行内样式。
    /// Inline style.
    /// </summary>
    [Parameter]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
