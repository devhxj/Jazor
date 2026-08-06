using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VCardItem")]
/// <summary>
/// Vuetify 卡片项分组组件，用于组织标题、副标题和前后缀。
/// Vuetify card item grouping component for organizing title, subtitle, and prepend/append content.
/// </summary>
public sealed class VCardItem : ComponentBase
{
    /// <summary>
    /// 后缀头像图片的 URL。
    /// URL for the append avatar image.
    /// </summary>
    [Parameter]
    public string? AppendAvatar { get; set; }

    /// <summary>
    /// 后缀图标名称。
    /// Append icon name.
    /// </summary>
    [Parameter]
    public string? AppendIcon { get; set; }

    /// <summary>
    /// 前缀头像图片的 URL。
    /// URL for the prepend avatar image.
    /// </summary>
    [Parameter]
    public string? PrependAvatar { get; set; }

    /// <summary>
    /// 前缀图标名称。
    /// Prepend icon name.
    /// </summary>
    [Parameter]
    public string? PrependIcon { get; set; }

    /// <summary>
    /// 卡片项的副标题文本。
    /// Subtitle text of the card item.
    /// </summary>
    [Parameter]
    public VuetifyTextValue? Subtitle { get; set; }

    /// <summary>
    /// 卡片项的标题文本。
    /// Title text of the card item.
    /// </summary>
    [Parameter]
    public VuetifyTextValue? Title { get; set; }

    /// <summary>
    /// 附加到组件根元素的额外属性。
    /// Additional attributes applied to the component root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 前缀插槽内容。
    /// Prepend slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Prepend { get; set; }

    /// <summary>
    /// 后缀插槽内容。
    /// Append slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Append { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Title slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// 副标题插槽内容。
    /// Subtitle slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? SubtitleContent { get; set; }

    /// <summary>
    /// 组件的子内容。
    /// Child content of the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
