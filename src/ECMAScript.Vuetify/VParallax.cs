using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 视差滚动组件的编写代理。
/// Vuetify parallax authoring proxy for image-backed parallax sections.
/// </summary>
[VueLibraryComponent("vuetify/components", "VParallax")]
public sealed class VParallax : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 视差滚动缩放比例。
    /// The parallax scroll scale factor.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scale")]
    public VueStringNumberValue? Scale { get; set; }

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
    /// 图片加载前显示的占位内容。
    /// Content displayed while the image is loading.
    /// </summary>
    [Parameter]
    [ECMAScriptName("placeholder")]
    public RenderFragment? Placeholder { get; set; }

    /// <summary>
    /// 图片加载失败时显示的错误内容。
    /// Content displayed when the image fails to load.
    /// </summary>
    [Parameter]
    [ECMAScriptName("error")]
    public RenderFragment? Error { get; set; }

    /// <summary>
    /// 用于自定义图片来源的插槽。
    /// Slot for customizing image sources.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sources")]
    public RenderFragment? Sources { get; set; }
}
