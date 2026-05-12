using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 菜单组件。
/// Vuetify menu component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VMenu")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(Activator), Name = "activator")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VMenu : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 菜单是否可见。
    /// Whether the menu is visible.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 菜单可见状态变更时触发的回调。
    /// Callback invoked when the menu visibility changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 点击菜单内容时是否关闭菜单。
    /// Whether to close the menu when its content is clicked.
    /// </summary>
    [Parameter]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 按下返回键时是否关闭菜单。
    /// Whether to close the menu on the back button press.
    /// </summary>
    [Parameter]
    public bool CloseOnBack { get; set; }

    /// <summary>
    /// 点击菜单外部时是否关闭菜单。
    /// Whether to close the menu when clicking outside.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; }

    /// <summary>
    /// 是否在点击激活器时打开菜单。
    /// Whether to open the menu on activator click.
    /// </summary>
    [Parameter]
    public bool OpenOnClick { get; set; }

    /// <summary>
    /// 是否在鼠标悬停时打开菜单。
    /// Whether to open the menu on hover.
    /// </summary>
    [Parameter]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 是否在聚焦时打开菜单。
    /// Whether to open the menu on focus.
    /// </summary>
    [Parameter]
    public bool OpenOnFocus { get; set; }

    /// <summary>
    /// 打开菜单的延迟时间（毫秒）。
    /// Delay before opening the menu (in milliseconds).
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 关闭菜单的延迟时间（毫秒）。
    /// Delay before closing the menu (in milliseconds).
    /// </summary>
    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 菜单相对于激活器的弹出位置。
    /// Position of the menu relative to the activator.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 菜单动画的起始位置。
    /// Origin point for the menu animation.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Origin { get; set; }

    /// <summary>
    /// 菜单与激活器之间的偏移距离。
    /// Offset distance between the menu and the activator.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    /// <summary>
    /// 页面滚动时的菜单行为策略。
    /// Scroll strategy for the menu when the page scrolls.
    /// </summary>
    [Parameter]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    /// <summary>
    /// 是否在点击外部时不关闭菜单。
    /// Whether the menu persists when clicking outside.
    /// </summary>
    [Parameter]
    public bool Persistent { get; set; }

    /// <summary>
    /// 是否禁用菜单。
    /// Whether to disable the menu.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 菜单的最小宽度。
    /// Minimum width of the menu.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 菜单的最大宽度。
    /// Maximum width of the menu.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 菜单的宽度。
    /// Width of the menu.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 菜单打开/关闭时的过渡动画。
    /// Transition animation when the menu opens or closes.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 应用于激活器元素的额外属性。
    /// Additional props applied to the activator element.
    /// </summary>
    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 应用于菜单内容元素的额外属性。
    /// Additional props applied to the menu content element.
    /// </summary>
    [Parameter]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 激活器插槽内容，提供覆盖层激活器上下文。
    /// Activator slot content, providing overlay activator context.
    /// </summary>
    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
