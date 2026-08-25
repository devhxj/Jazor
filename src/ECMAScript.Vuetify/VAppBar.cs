using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VAppBar")]
/// <summary>
/// Vuetify 应用栏组件。
/// Vuetify app bar component.
/// </summary>
public sealed class VAppBar : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; } = true;

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否移除阴影效果。
    /// Removes box-shadow.
    /// </summary>
    [Parameter]
    [ECMAScriptName("flat")]
    public bool Flat { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 应用栏在容器中的定位位置。
    /// Position of the app bar within container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyAppBarLocation? Location { get; set; }

    /// <summary>
    /// 滚动行为模式。
    /// Scroll behavior mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrollBehavior")]
    public string? ScrollBehavior { get; set; }

    /// <summary>
    /// 背景图片 URL。
    /// Background image URL.
    /// </summary>
    [Parameter]
    [ECMAScriptName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
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
