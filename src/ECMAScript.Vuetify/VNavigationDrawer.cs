using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 导航抽屉组件。
/// Vuetify navigation drawer component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VNavigationDrawer")]
public sealed class VNavigationDrawer : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 导航抽屉是否可见。
    /// Whether the navigation drawer is visible.
    /// </summary>
    [Parameter]
    public bool? ModelValue { get; set; }

    /// <summary>
    /// 导航抽屉可见状态变更时触发的回调。
    /// Callback invoked when the drawer visibility changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否以轨道模式显示精简抽屉。
    /// Whether to show a compact rail drawer.
    /// </summary>
    [Parameter]
    public bool? Rail { get; set; }

    /// <summary>
    /// 轨道模式状态变更时触发的回调。
    /// Callback invoked when the rail mode changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool?> RailChanged { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 是否永久显示抽屉（不受响应式条件影响）。
    /// Whether to permanently show the drawer (unaffected by responsive conditions).
    /// </summary>
    [Parameter]
    public bool Permanent { get; set; }

    /// <summary>
    /// 是否为临时抽屉（仅在触发时出现）。
    /// Whether the drawer is temporary (only appears when triggered).
    /// </summary>
    [Parameter]
    public bool Temporary { get; set; }

    /// <summary>
    /// 是否为持久抽屉（在移动端可通过遮罩层关闭）。
    /// Whether the drawer is persistent (can be closed via scrim on mobile).
    /// </summary>
    [Parameter]
    public bool Persistent { get; set; }

    /// <summary>
    /// 鼠标悬停时是否展开抽屉。
    /// Whether to expand the drawer on hover.
    /// </summary>
    [Parameter]
    public bool ExpandOnHover { get; set; }

    /// <summary>
    /// 是否以浮动模式显示抽屉。
    /// Whether to display the drawer in floating mode.
    /// </summary>
    [Parameter]
    public bool Floating { get; set; }

    /// <summary>
    /// 是否将抽屉固定在可见位置。
    /// Whether to stick the drawer in a visible position.
    /// </summary>
    [Parameter]
    public bool Sticky { get; set; }

    /// <summary>
    /// 是否禁用触摸滑动手势。
    /// Whether to disable touch swipe gestures.
    /// </summary>
    [Parameter]
    public bool Touchless { get; set; }

    /// <summary>
    /// 是否禁用窗口尺寸变化监听器。
    /// Whether to disable the resize watcher.
    /// </summary>
    [Parameter]
    public bool DisableResizeWatcher { get; set; }

    /// <summary>
    /// 是否禁用路由变化监听器。
    /// Whether to disable the route watcher.
    /// </summary>
    [Parameter]
    public bool DisableRouteWatcher { get; set; }

    /// <summary>
    /// 导航抽屉的宽度。
    /// Width of the navigation drawer.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 轨道模式下抽屉的宽度。
    /// Width of the drawer in rail mode.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? RailWidth { get; set; }

    /// <summary>
    /// 抽屉打开时的遮罩层配置。
    /// Scrim configuration when the drawer is open.
    /// </summary>
    [Parameter]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 抽屉背景图片的 URL。
    /// URL of the drawer background image.
    /// </summary>
    [Parameter]
    public string? Image { get; set; }

    /// <summary>
    /// 导航抽屉的显示位置。
    /// Display position of the navigation drawer.
    /// </summary>
    [Parameter]
    public VuetifyNavigationDrawerLocation? Location { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
