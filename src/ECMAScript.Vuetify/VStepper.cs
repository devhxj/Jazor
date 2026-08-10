using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 步骤条组件的编写代理，用于多步骤工作流。
/// Vuetify stepper authoring proxy for multi-step workflows.
/// </summary>
[VueLibraryComponent("vuetify/components", "VStepper")]
public sealed class VStepper : ComponentBase
{
    /// <summary>
    /// 步骤条当前选中的步骤值。
    /// Currently selected step value of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 选中步骤变化时触发的回调。
    /// Callback invoked when the selected step changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否使用替代标签布局。
    /// Whether to use an alternative label layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("altLabels")]
    public bool AltLabels { get; set; }

    /// <summary>
    /// 步骤条的背景颜色。
    /// Background color of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 已完成步骤的图标。
    /// Icon displayed for completed steps.
    /// </summary>
    [Parameter]
    [ECMAScriptName("completeIcon")]
    public VuetifyIconValue? CompleteIcon { get; set; }

    /// <summary>
    /// 可编辑步骤的图标。
    /// Icon displayed for editable steps.
    /// </summary>
    [Parameter]
    [ECMAScriptName("editIcon")]
    public VuetifyIconValue? EditIcon { get; set; }

    /// <summary>
    /// 是否允许用户编辑已完成的步骤。
    /// Whether to allow editing of completed steps.
    /// </summary>
    [Parameter]
    [ECMAScriptName("editable")]
    public bool Editable { get; set; }

    /// <summary>
    /// 错误步骤的图标。
    /// Icon displayed for steps with errors.
    /// </summary>
    [Parameter]
    [ECMAScriptName("errorIcon")]
    public VuetifyIconValue? ErrorIcon { get; set; }

    /// <summary>
    /// 是否隐藏步骤条的操作按钮。
    /// Whether to hide the stepper action buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideActions")]
    public bool HideActions { get; set; }

    /// <summary>
    /// 步骤条的数据项列表。
    /// Data items for the stepper steps.
    /// </summary>
    [Parameter]
    [ECMAScriptName("items")]
    public VuetifyStepperItems? Items { get; set; }

    /// <summary>
    /// 数据项中取标题的属性名。
    /// Property name to extract the title from each data item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemTitle")]
    public string? ItemTitle { get; set; }

    /// <summary>
    /// 数据项中取值的属性名。
    /// Property name to extract the value from each data item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemValue")]
    public string? ItemValue { get; set; }

    /// <summary>
    /// 是否允许非线性的步骤导航。
    /// Whether to allow non-linear step navigation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nonLinear")]
    public bool NonLinear { get; set; }

    /// <summary>
    /// 是否使用扁平风格。
    /// Whether to use a flat style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("flat")]
    public bool Flat { get; set; }

    /// <summary>
    /// 移动端布局配置。
    /// Mobile layout configuration.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mobile")]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 触发移动端布局的断点。
    /// Display breakpoint that triggers mobile layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mobileBreakpoint")]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    /// <summary>
    /// 是否允许多个步骤同时展开。
    /// Whether to allow multiple steps to be open simultaneously.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// 是否强制必须选中一个步骤。
    /// Whether to force at least one step to be selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; } = VuetifyMandatoryMode.Force;

    /// <summary>
    /// 可同时选中的最大步骤数。
    /// Maximum number of steps that can be selected simultaneously.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public int? Max { get; set; }

    /// <summary>
    /// 选中步骤时应用的 CSS 类名。
    /// CSS class name applied to the selected step.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 是否禁用步骤条。
    /// Whether the stepper is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 上一步按钮的文本。
    /// Text for the previous step button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prevText")]
    public string? PrevText { get; set; }

    /// <summary>
    /// 下一步按钮的文本。
    /// Text for the next step button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nextText")]
    public string? NextText { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 组件根元素的 HTML 标签名。
    /// HTML tag name for the component root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 步骤条的圆角大小。
    /// Border radius of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 步骤条的定位方式。
    /// Positioning strategy of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 步骤条出现的位置。
    /// Position where the stepper appears.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 步骤条的阴影高度。
    /// Elevation shadow of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 步骤条的高度。
    /// Height of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 步骤条的最大高度。
    /// Maximum height of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 步骤条的最大宽度。
    /// Maximum width of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 步骤条的最小高度。
    /// Minimum height of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 步骤条的最小宽度。
    /// Minimum width of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 步骤条的宽度。
    /// Width of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 步骤条的边框配置。
    /// Border configuration of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 步骤条的颜色。
    /// Color of the stepper.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，步骤条的主体内容。
    /// Default slot for the stepper body content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VStepperNavigationSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 操作插槽，步骤条的操作按钮区域。
    /// Actions slot for the stepper action buttons area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("actions")]
    public RenderFragment<VStepperNavigationSlotContext>? Actions { get; set; }

    /// <summary>
    /// 头部插槽，步骤条的头部区域。
    /// Header slot for the stepper header area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header")]
    public RenderFragment<VStepperItemSlotContext>? Header { get; set; }

    /// <summary>
    /// 头部项插槽，自定义单个步骤的头部渲染。
    /// Header-item slot for customizing a single step header rendering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header-item")]
    public RenderFragment<VStepperItemSlotContext>? HeaderItem { get; set; }

    /// <summary>
    /// 图标插槽，自定义步骤图标的渲染。
    /// Icon slot for customizing step icon rendering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("icon")]
    public RenderFragment<VStepperItemSlotContext>? Icon { get; set; }

    /// <summary>
    /// 标题插槽，自定义步骤标题的渲染。
    /// Title slot for customizing step title rendering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment<VStepperItemSlotContext>? TitleContent { get; set; }

    /// <summary>
    /// 副标题插槽，自定义步骤副标题的渲染。
    /// Subtitle slot for customizing step subtitle rendering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("subtitle")]
    public RenderFragment<VStepperItemSlotContext>? SubtitleContent { get; set; }

    /// <summary>
    /// 内容项插槽，自定义步骤内容的渲染。
    /// Item slot for customizing step content rendering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("item")]
    public RenderFragment<VStepperContentItemSlotContext>? Item { get; set; }

    /// <summary>
    /// 上一步按钮插槽。
    /// Previous step button slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prev")]
    public RenderFragment<VStepperActionButtonSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一步按钮插槽。
    /// Next step button slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("next")]
    public RenderFragment<VStepperActionButtonSlotContext>? Next { get; set; }
}
