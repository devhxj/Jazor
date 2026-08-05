using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 工具提示组件的编写代理，用于 RazorVue 创作的第一波存根。
/// First-wave Vuetify tooltip stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTooltip")]
public sealed class VTooltip : ComponentBase
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 元素ID。
    /// Element ID.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 可交互。
    /// Makes the tooltip interactive.
    /// </summary>
    [Parameter]
    public bool Interactive { get; set; }

    /// <summary>
    /// 文本。
    /// Text content.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// 位置。
    /// Location of the tooltip.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 原点。
    /// Origin point for the transition.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Origin { get; set; }

    /// <summary>
    /// 偏移。
    /// Offset from the activator.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    /// <summary>
    /// 点击打开。
    /// Opens on click.
    /// </summary>
    [Parameter]
    public bool OpenOnClick { get; set; }

    /// <summary>
    /// 悬停打开。
    /// Opens on hover.
    /// </summary>
    [Parameter]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 聚焦打开。
    /// Opens on focus.
    /// </summary>
    [Parameter]
    public bool OpenOnFocus { get; set; }

    /// <summary>
    /// 打开延迟。
    /// Open delay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 关闭延迟。
    /// Close delay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the tooltip.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 急切加载。
    /// Forces eager rendering.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 最小宽。
    /// Minimum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 最大宽。
    /// Maximum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 宽。
    /// Width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 过渡。
    /// Transition effect.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 激活器属性。
    /// Activator element props.
    /// </summary>
    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 内容属性。
    /// Content element props.
    /// </summary>
    [Parameter]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 激活器插槽。
    /// Activator slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
