using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 开关组件的编写代理。
/// Vuetify switch authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSwitch")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(Details), Name = "details")]
[VueLibrarySlot(nameof(Message), Name = "message")]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Input), Name = "input")]
[VueLibrarySlot(nameof(Loader), Name = "loader")]
[VueLibrarySlot(nameof(Thumb), Name = "thumb")]
[VueLibrarySlot(nameof(TrackTrue), Name = "track-true")]
[VueLibrarySlot(nameof(TrackFalse), Name = "track-false")]
public sealed class VSwitch : VSelectionControlComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 是否使用缩进样式。
    /// Whether to use inset style.
    /// </summary>
    [Parameter]
    public bool Inset { get; set; }

    /// <summary>
    /// 加载状态。
    /// Loading state.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 是否移除阴影效果。
    /// Removes box-shadow.
    /// </summary>
    [Parameter]
    public bool Flat { get; set; }

    /// <summary>
    /// 加载插槽内容。
    /// Loader slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VuetifyLoaderSlotContext>? Loader { get; set; }

    /// <summary>
    /// 滑块插槽内容。
    /// Thumb slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VSwitchSlotContext>? Thumb { get; set; }

    /// <summary>
    /// 开启轨道插槽内容。
    /// Track-true slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VSwitchSlotContext>? TrackTrue { get; set; }

    /// <summary>
    /// 关闭轨道插槽内容。
    /// Track-false slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VSwitchSlotContext>? TrackFalse { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
