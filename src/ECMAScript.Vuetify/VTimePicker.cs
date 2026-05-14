using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室时间选择器组件的编写代理。
/// Vuetify labs time-picker authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VTimePicker")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ViewModeChanged), VueEmitKind.ModelUpdate, Name = "update:viewMode")]
[VueLibraryEmit(nameof(PeriodChanged), VueEmitKind.ModelUpdate, Name = "update:period")]
[VueLibraryEmit(nameof(HourChanged), VueEmitKind.ModelUpdate, Name = "update:hour")]
[VueLibraryEmit(nameof(MinuteChanged), VueEmitKind.ModelUpdate, Name = "update:minute")]
[VueLibraryEmit(nameof(SecondChanged), VueEmitKind.ModelUpdate, Name = "update:second")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
[VueSlot(nameof(Actions), Name = "actions")]
[VueSlot(nameof(TitleContent), Name = "title")]
public sealed class VTimePicker : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    public VuetifyTimePickerModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 允许的小时值。
    /// Allowed hours.
    /// </summary>
    [Parameter]
    public VuetifyTimePickerAllowedUnitValue? AllowedHours { get; set; }

    /// <summary>
    /// 允许的分钟值。
    /// Allowed minutes.
    /// </summary>
    [Parameter]
    public VuetifyTimePickerAllowedUnitValue? AllowedMinutes { get; set; }

    /// <summary>
    /// 允许的秒数值。
    /// Allowed seconds.
    /// </summary>
    [Parameter]
    public VuetifyTimePickerAllowedUnitValue? AllowedSeconds { get; set; }

    /// <summary>
    /// 标题中显示 AM/PM。
    /// Shows AM/PM in the title.
    /// </summary>
    [Parameter]
    public bool AmpmInTitle { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the picker.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 时间格式。
    /// Time format.
    /// </summary>
    [Parameter]
    public VuetifyTimePickerFormat? Format { get; set; }

    /// <summary>
    /// 最大时间。
    /// Maximum allowed time.
    /// </summary>
    [Parameter]
    public string? Max { get; set; }

    /// <summary>
    /// 最小时间。
    /// Minimum allowed time.
    /// </summary>
    [Parameter]
    public string? Min { get; set; }

    /// <summary>
    /// 视图模式。
    /// View mode.
    /// </summary>
    [Parameter]
    public VuetifyTimePickerViewMode? ViewMode { get; set; }

    /// <summary>
    /// 视图模式变化事件。
    /// View mode changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTimePickerViewMode> ViewModeChanged { get; set; }

    /// <summary>
    /// AM/PM变化事件。
    /// Period changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyTimePickerPeriod> PeriodChanged { get; set; }

    /// <summary>
    /// 小时变化事件。
    /// Hour changed event.
    /// </summary>
    [Parameter]
    public EventCallback<Number> HourChanged { get; set; }

    /// <summary>
    /// 分钟变化事件。
    /// Minute changed event.
    /// </summary>
    [Parameter]
    public EventCallback<Number> MinuteChanged { get; set; }

    /// <summary>
    /// 秒数变化事件。
    /// Second changed event.
    /// </summary>
    [Parameter]
    public EventCallback<Number> SecondChanged { get; set; }

    /// <summary>
    /// 只读。
    /// Puts the picker in readonly mode.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 可滚动。
    /// Enables scroll interaction.
    /// </summary>
    [Parameter]
    public bool Scrollable { get; set; }

    /// <summary>
    /// 使用秒数。
    /// Enables seconds selection.
    /// </summary>
    [Parameter]
    public bool UseSeconds { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角。
    /// Border radius.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// CSS定位。
    /// CSS position.
    /// </summary>
    [Parameter]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 位置。
    /// Location of the component.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 阴影。
    /// Elevation shadow.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 高。
    /// Height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 最大高。
    /// Maximum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 最大宽。
    /// Maximum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 最小高。
    /// Minimum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 最小宽。
    /// Minimum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 宽。
    /// Width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 边框。
    /// Border.
    /// </summary>
    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 主题颜色。
    /// Theme color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 背景颜色。
    /// Background color.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 分隔线。
    /// Adds a divider.
    /// </summary>
    [Parameter]
    public bool Divided { get; set; }

    /// <summary>
    /// 标题。
    /// Title text.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 隐藏头部。
    /// Hides the header.
    /// </summary>
    [Parameter]
    public bool HideHeader { get; set; }

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

    /// <summary>
    /// 操作插槽。
    /// Actions slot.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 标题内容插槽。
    /// Title content slot.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
