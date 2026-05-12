using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 窗口组件的创作代理，用于分组面板导航。
/// Vuetify window authoring proxy for grouped panel navigation.
/// </summary>
[VueLibraryComponent("vuetify/components", "VWindow")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Additional), Name = "additional")]
[VueLibrarySlot(nameof(Prev), Name = "prev")]
[VueLibrarySlot(nameof(Next), Name = "next")]
public sealed class VWindow : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 连续。
    /// Continuous loop.
    /// </summary>
    [Parameter]
    public bool Continuous { get; set; }

    /// <summary>
    /// 下图标。
    /// Next icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上图标。
    /// Previous icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 反转。
    /// Reverses the transition direction.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// 显示箭头。
    /// Shows navigation arrows.
    /// </summary>
    [Parameter]
    public VuetifyWindowShowArrowsValue? ShowArrows { get; set; }

    /// <summary>
    /// 触摸。
    /// Touch interaction.
    /// </summary>
    [Parameter]
    public VuetifyTouchValue? Touch { get; set; }

    /// <summary>
    /// 滑动方向。
    /// Slide direction.
    /// </summary>
    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the window.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项CSS类。
    /// CSS class applied to the selected item.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 强制选中。
    /// Mandatory selection.
    /// </summary>
    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

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
    public RenderFragment<VWindowSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 附加插槽。
    /// Additional slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VWindowSlotContext>? Additional { get; set; }

    /// <summary>
    /// 上一项插槽。
    /// Previous control slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一项插槽。
    /// Next control slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Next { get; set; }
}
