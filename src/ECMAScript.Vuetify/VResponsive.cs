using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 响应式容器组件的编写代理。
/// Vuetify responsive container authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VResponsive")]
public sealed class VResponsive : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 容器的宽高比。
    /// The aspect ratio of the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("aspectRatio")]
    public VueStringNumberValue? AspectRatio { get; set; }

    /// <summary>
    /// 应用于内容区域的 CSS 类。
    /// CSS classes applied to the content area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("contentClass")]
    public string? ContentClass { get; set; }

    /// <summary>
    /// 是否使用行内布局。
    /// Whether to use inline layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inline")]
    public bool Inline { get; set; }

    /// <summary>
    /// 组件的高度。
    /// The height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件的最大高度。
    /// The maximum height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// The maximum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件的最小高度。
    /// The minimum height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// The minimum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件的宽度。
    /// The width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 额外区域的自定义内容。
    /// Custom content for the additional area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("additional")]
    public RenderFragment? Additional { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// The default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
