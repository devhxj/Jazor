using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 视差滚动组件的编写代理。
/// Vuetify parallax authoring proxy for image-backed parallax sections.
/// </summary>
[ECMAScript("vuetify/components/VParallax")]
public sealed class VParallax : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 视差滚动缩放比例。
    /// The parallax scroll scale factor.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Scale="@(32)"；变量用 Scale="@value"，无需 double 后缀。字符串用 Scale="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
