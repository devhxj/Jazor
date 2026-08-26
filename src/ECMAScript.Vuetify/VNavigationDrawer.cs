using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 导航抽屉组件。
/// Vuetify navigation drawer component.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VNavigationDrawer")]
public sealed class VNavigationDrawer : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 导航抽屉是否可见。
    /// Whether the navigation drawer is visible.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool? ModelValue { get; set; }

    /// <summary>
    /// 导航抽屉可见状态变更时触发的回调。
    /// Callback invoked when the drawer visibility changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否以轨道模式显示精简抽屉。
    /// Whether to show a compact rail drawer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rail")]
    public bool? Rail { get; set; }

    /// <summary>
    /// 轨道模式状态变更时触发的回调。
    /// Callback invoked when the rail mode changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:rail")]
    public EventCallback<bool?> RailChanged { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否永久显示抽屉（不受响应式条件影响）。
    /// Whether to permanently show the drawer (unaffected by responsive conditions).
    /// </summary>
    [Parameter]
    [ECMAScriptName("permanent")]
    public bool Permanent { get; set; }

    /// <summary>
    /// 是否为临时抽屉（仅在触发时出现）。
    /// Whether the drawer is temporary (only appears when triggered).
    /// </summary>
    [Parameter]
    [ECMAScriptName("temporary")]
    public bool Temporary { get; set; }

    /// <summary>
    /// 是否为持久抽屉（在移动端可通过遮罩层关闭）。
    /// Whether the drawer is persistent (can be closed via scrim on mobile).
    /// </summary>
    [Parameter]
    [ECMAScriptName("persistent")]
    public bool Persistent { get; set; }

    /// <summary>
    /// 鼠标悬停时是否展开抽屉。
    /// Whether to expand the drawer on hover.
    /// </summary>
    [Parameter]
    [ECMAScriptName("expandOnHover")]
    public bool ExpandOnHover { get; set; }

    /// <summary>
    /// 是否以浮动模式显示抽屉。
    /// Whether to display the drawer in floating mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("floating")]
    public bool Floating { get; set; }

    /// <summary>
    /// 是否将抽屉固定在可见位置。
    /// Whether to stick the drawer in a visible position.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sticky")]
    public bool Sticky { get; set; }

    /// <summary>
    /// 是否禁用触摸滑动手势。
    /// Whether to disable touch swipe gestures.
    /// </summary>
    [Parameter]
    [ECMAScriptName("touchless")]
    public bool Touchless { get; set; }

    /// <summary>
    /// 是否禁用窗口尺寸变化监听器。
    /// Whether to disable the resize watcher.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disableResizeWatcher")]
    public bool DisableResizeWatcher { get; set; }

    /// <summary>
    /// 是否禁用路由变化监听器。
    /// Whether to disable the route watcher.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disableRouteWatcher")]
    public bool DisableRouteWatcher { get; set; }

    /// <summary>
    /// 导航抽屉的宽度。
    /// Width of the navigation drawer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 轨道模式下抽屉的宽度。
    /// Width of the drawer in rail mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("railWidth")]
    public VueStringNumberValue? RailWidth { get; set; }

    /// <summary>
    /// 抽屉打开时的遮罩层配置。
    /// Scrim configuration when the drawer is open.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrim")]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 抽屉背景图片的 URL。
    /// URL of the drawer background image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// 导航抽屉的显示位置。
    /// Display position of the navigation drawer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyNavigationDrawerLocation? Location { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
