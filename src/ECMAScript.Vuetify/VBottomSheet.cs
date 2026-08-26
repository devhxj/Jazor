using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[ECMAScript("vuetify/components", Transform.Component, "VBottomSheet")]
/// <summary>
/// Vuetify 底部抽屉组件。
/// Vuetify bottom sheet component.
/// </summary>
public sealed class VBottomSheet : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否内嵌显示。
    /// Insets the sheet.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inset")]
    public bool Inset { get; set; }

    /// <summary>
    /// 点击外部时不关闭。
    /// Prevents closing on outside click.
    /// </summary>
    [Parameter]
    [ECMAScriptName("persistent")]
    public bool Persistent { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// Max width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 滚动策略。
    /// Scroll strategy.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrollStrategy")]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    /// <summary>
    /// 过渡动画。
    /// Transition animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 激活器的属性。
    /// Props for the activator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activatorProps")]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 内容容器的属性。
    /// Props for the content container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("contentProps")]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 是否立即渲染内容。
    /// Renders content eagerly.
    /// </summary>
    [Parameter]
    [ECMAScriptName("eager")]
    public bool Eager { get; set; }

    /// <summary>
    /// 禁用点击动画。
    /// Disables click animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noClickAnimation")]
    public bool NoClickAnimation { get; set; }

    /// <summary>
    /// 遮罩层设置。
    /// Scrim overlay setting.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrim")]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 激活器插槽内容。
    /// Activator slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activator")]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
