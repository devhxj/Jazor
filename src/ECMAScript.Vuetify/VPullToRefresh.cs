using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify Labs 下拉刷新组件的编写代理。
/// Vuetify labs pull-to-refresh authoring proxy.
/// </summary>
[ECMAScript("vuetify/labs/components", Transform.Component, "VPullToRefresh")]
public sealed class VPullToRefresh : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否禁用下拉刷新功能。
    /// Whether the pull-to-refresh is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 触发下拉刷新的阈值距离。
    /// The threshold distance to trigger a pull-to-refresh.
    /// </summary>
    [Parameter]
    [ECMAScriptName("pullDownThreshold")]
    public Number? PullDownThreshold { get; set; }

    /// <summary>
    /// 下拉刷新加载时触发的回调。
    /// Callback invoked when a pull-to-refresh load is triggered.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onLoad")]
    public EventCallback<VPullToRefreshLoadOptions> OnLoad { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// The default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 下拉面板的自定义内容。
    /// Custom content for the pull-down panel.
    /// </summary>
    [Parameter]
    [ECMAScriptName("pull-down-panel")]
    public RenderFragment<VPullToRefreshPanelSlotContext>? PullDownPanel { get; set; }
}
