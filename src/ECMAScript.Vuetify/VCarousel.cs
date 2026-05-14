using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 轮播组件创作代理，用于分组幻灯片导航。
/// Vuetify carousel authoring proxy for grouped slide navigation.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCarousel")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
[VueSlot(nameof(Prev), Name = "prev")]
[VueSlot(nameof(Next), Name = "next")]
[VueSlot(nameof(Item), Name = "item")]
public sealed class VCarousel : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 当前轮播激活项的绑定值。
    /// The bound value of the currently active carousel item.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 激活项变更时的回调。
    /// Callback invoked when the active item changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 组件的主题色。
    /// The theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 是否启用自动轮播循环。
    /// Whether to enable automatic carousel cycling.
    /// </summary>
    [Parameter]
    public bool Cycle { get; set; }

    /// <summary>
    /// 分隔符图标。
    /// The delimiter icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? DelimiterIcon { get; set; }

    /// <summary>
    /// 轮播高度。
    /// The height of the carousel.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 是否隐藏分隔符。
    /// Whether to hide the delimiters.
    /// </summary>
    [Parameter]
    public bool HideDelimiters { get; set; }

    /// <summary>
    /// 是否隐藏分隔符背景。
    /// Whether to hide the delimiter background.
    /// </summary>
    [Parameter]
    public bool HideDelimiterBackground { get; set; }

    /// <summary>
    /// 自动轮播间隔时间。
    /// The interval for automatic carousel cycling.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Interval { get; set; }

    /// <summary>
    /// 进度条显示方式。
    /// The progress indicator display mode.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Progress { get; set; }

    /// <summary>
    /// 垂直分隔符的位置。
    /// The position of vertical delimiters.
    /// </summary>
    [Parameter]
    public VuetifyCarouselVerticalDelimiters? VerticalDelimiters { get; set; }

    /// <summary>
    /// 是否在末尾循环回开头。
    /// Whether to loop back to the start after the last item.
    /// </summary>
    [Parameter]
    public bool Continuous { get; set; } = true;

    /// <summary>
    /// 下一项图标。
    /// The icon for the next control.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一项图标。
    /// The icon for the previous control.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 是否反转轮播方向。
    /// Whether to reverse the carousel direction.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// 是否显示导航箭头。
    /// Whether to show navigation arrows.
    /// </summary>
    [Parameter]
    public VuetifyWindowShowArrowsValue? ShowArrows { get; set; } = true;

    /// <summary>
    /// 触摸交互配置。
    /// The touch interaction configuration.
    /// </summary>
    [Parameter]
    public VuetifyTouchValue? Touch { get; set; }

    /// <summary>
    /// 轮播切换方向。
    /// The transition direction of the carousel.
    /// </summary>
    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 是否禁用轮播交互。
    /// Whether to disable carousel interaction.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项应用的 CSS 类名。
    /// The CSS class applied to the selected item.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 是否强制选择。
    /// Whether selection is mandatory.
    /// </summary>
    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; } = VuetifyMandatoryMode.Force;

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// The component theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，轮播项内容。
    /// Default slot for carousel item content.
    /// </summary>
    [Parameter]
    public RenderFragment<VWindowSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 上一项控制按钮插槽。
    /// Slot for the previous control button.
    /// </summary>
    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一项控制按钮插槽。
    /// Slot for the next control button.
    /// </summary>
    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Next { get; set; }

    /// <summary>
    /// 轮播项插槽。
    /// Slot for each carousel item.
    /// </summary>
    [Parameter]
    public RenderFragment<VCarouselItemSlotContext>? Item { get; set; }
}
