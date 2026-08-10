using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 窗口组件的创作代理，用于分组面板导航。
/// Vuetify window authoring proxy for grouped panel navigation.
/// </summary>
[VueLibraryComponent("vuetify/components", "VWindow")]
public sealed class VWindow : ComponentBase
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 连续。
    /// Continuous loop.
    /// </summary>
    [Parameter]
    [ECMAScriptName("continuous")]
    public bool Continuous { get; set; }

    /// <summary>
    /// 下图标。
    /// Next icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上图标。
    /// Previous icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prevIcon")]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 反转。
    /// Reverses the transition direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("reverse")]
    public bool Reverse { get; set; }

    /// <summary>
    /// 显示箭头。
    /// Shows navigation arrows.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showArrows")]
    public VuetifyWindowShowArrowsValue? ShowArrows { get; set; }

    /// <summary>
    /// 触摸。
    /// Touch interaction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("touch")]
    public VuetifyTouchValue? Touch { get; set; }

    /// <summary>
    /// 滑动方向。
    /// Slide direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the window.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项CSS类。
    /// CSS class applied to the selected item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 强制选中。
    /// Mandatory selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VWindowSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 附加插槽。
    /// Additional slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("additional")]
    public RenderFragment<VWindowSlotContext>? Additional { get; set; }

    /// <summary>
    /// 上一项插槽。
    /// Previous control slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prev")]
    public RenderFragment<VWindowControlSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一项插槽。
    /// Next control slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("next")]
    public RenderFragment<VWindowControlSlotContext>? Next { get; set; }
}
