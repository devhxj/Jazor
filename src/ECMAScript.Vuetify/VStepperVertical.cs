using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室垂直步骤条组件的编写代理，基于展开面板工作流。
/// Vuetify labs vertical stepper authoring proxy for expansion-panel based workflows.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VStepperVertical")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Icon), Name = "icon")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
[VueLibrarySlot(nameof(Prev), Name = "prev")]
[VueLibrarySlot(nameof(Next), Name = "next")]
[VueLibrarySlot(nameof(HeaderItem), Name = "header-item", NamePattern = "header-item.${string}", PatternOnly = true)]
[VueLibrarySlot(nameof(Item), Name = "item", NamePattern = "item.${string}", PatternOnly = true)]
public sealed class VStepperVertical : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 垂直步骤条当前选中的步骤值。
    /// Currently selected step value of the vertical stepper.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 选中步骤变化时触发的回调。
    /// Callback invoked when the selected step changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否使用扁平风格。
    /// Whether to use a flat style.
    /// </summary>
    [Parameter]
    public bool Flat { get; set; }

    /// <summary>
    /// 垂直步骤条的视觉变体。
    /// Visual variant of the vertical stepper.
    /// </summary>
    [Parameter]
    public VuetifyExpansionPanelVariant? Variant { get; set; } = VuetifyExpansionPanelVariant.Accordion;

    /// <summary>
    /// 可同时选中的最大步骤数。
    /// Maximum number of steps that can be selected simultaneously.
    /// </summary>
    [Parameter]
    public int? Max { get; set; }

    /// <summary>
    /// 垂直步骤条的颜色。
    /// Color of the vertical stepper.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 应用于组件的 CSS 类。
    /// CSS classes applied to the component.
    /// </summary>
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于组件的内联样式。
    /// Inline styles applied to the component.
    /// </summary>
    [Parameter]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 是否在首次渲染时强制加载内容。
    /// Whether to eagerly mount the content on first render.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否禁用垂直步骤条。
    /// Whether the vertical stepper is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否允许多个步骤同时展开。
    /// Whether to allow multiple steps to be open simultaneously.
    /// </summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// 是否设为只读模式。
    /// Whether the vertical stepper is read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 组件根元素的 HTML 标签名。
    /// HTML tag name for the component root element.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 是否强制必须选中一个步骤。
    /// Whether to force at least one step to be selected.
    /// </summary>
    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; } = VuetifyMandatoryMode.Force;

    /// <summary>
    /// 垂直步骤条的阴影高度。
    /// Elevation shadow of the vertical stepper.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 是否允许键盘聚焦步骤。
    /// Whether steps are focusable via keyboard.
    /// </summary>
    [Parameter]
    public bool Focusable { get; set; }

    /// <summary>
    /// 垂直步骤条的圆角大小。
    /// Border radius of the vertical stepper.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 选中步骤时应用的 CSS 类名。
    /// CSS class name applied to the selected step.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 垂直步骤条的背景颜色。
    /// Background color of the vertical stepper.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否启用水波纹点击效果。
    /// Whether to enable the ripple click effect.
    /// </summary>
    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 步骤折叠时显示的图标。
    /// Icon displayed when a step is collapsed.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? CollapseIcon { get; set; }

    /// <summary>
    /// 步骤展开时显示的图标。
    /// Icon displayed when a step is expanded.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? ExpandIcon { get; set; }

    /// <summary>
    /// 是否隐藏步骤的操作按钮。
    /// Whether to hide step action buttons.
    /// </summary>
    [Parameter]
    public bool HideActions { get; set; }

    /// <summary>
    /// 移动端布局配置。
    /// Mobile layout configuration.
    /// </summary>
    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 触发移动端布局的断点。
    /// Display breakpoint that triggers mobile layout.
    /// </summary>
    [Parameter]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    /// <summary>
    /// 是否使用替代标签布局。
    /// Whether to use an alternative label layout.
    /// </summary>
    [Parameter]
    public bool AltLabels { get; set; }

    /// <summary>
    /// 已完成步骤的图标。
    /// Icon displayed for completed steps.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? CompleteIcon { get; set; }

    /// <summary>
    /// 可编辑步骤的图标。
    /// Icon displayed for editable steps.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? EditIcon { get; set; }

    /// <summary>
    /// 是否允许用户编辑已完成的步骤。
    /// Whether to allow editing of completed steps.
    /// </summary>
    [Parameter]
    public bool Editable { get; set; }

    /// <summary>
    /// 错误步骤的图标。
    /// Icon displayed for steps with errors.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? ErrorIcon { get; set; }

    /// <summary>
    /// 垂直步骤条的数据项列表。
    /// Data items for the vertical stepper steps.
    /// </summary>
    [Parameter]
    public VuetifyStepperItems? Items { get; set; }

    /// <summary>
    /// 数据项中取标题的属性名。
    /// Property name to extract the title from each data item.
    /// </summary>
    [Parameter]
    public string? ItemTitle { get; set; }

    /// <summary>
    /// 数据项中取值的属性名。
    /// Property name to extract the value from each data item.
    /// </summary>
    [Parameter]
    public string? ItemValue { get; set; }

    /// <summary>
    /// 是否允许非线性的步骤导航。
    /// Whether to allow non-linear step navigation.
    /// </summary>
    [Parameter]
    public bool NonLinear { get; set; }

    /// <summary>
    /// 上一步按钮的文本。
    /// Text for the previous step button.
    /// </summary>
    [Parameter]
    public string? PrevText { get; set; }

    /// <summary>
    /// 下一步按钮的文本。
    /// Text for the next step button.
    /// </summary>
    [Parameter]
    public string? NextText { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 操作插槽，垂直步骤条的操作按钮区域。
    /// Actions slot for the vertical stepper action buttons area.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalActionSlotContext>? Actions { get; set; }

    /// <summary>
    /// 默认插槽，垂直步骤条的主体内容。
    /// Default slot for the vertical stepper body content.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 图标插槽，自定义步骤图标的渲染。
    /// Icon slot for customizing step icon rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? Icon { get; set; }

    /// <summary>
    /// 标题插槽，自定义步骤标题的渲染。
    /// Title slot for customizing step title rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? TitleContent { get; set; }

    /// <summary>
    /// 副标题插槽，自定义步骤副标题的渲染。
    /// Subtitle slot for customizing step subtitle rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? SubtitleContent { get; set; }

    /// <summary>
    /// 上一步按钮插槽。
    /// Previous step button slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalActionSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一步按钮插槽。
    /// Next step button slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalActionSlotContext>? Next { get; set; }

    /// <summary>
    /// 头部项插槽，自定义单个步骤的头部渲染。
    /// Header-item slot for customizing a single step header rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? HeaderItem { get; set; }

    /// <summary>
    /// 内容项插槽，自定义步骤内容的渲染。
    /// Item slot for customizing step content rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? Item { get; set; }
}
