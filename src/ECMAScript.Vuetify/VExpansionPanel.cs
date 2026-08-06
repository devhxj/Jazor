using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 展开面板组件。
/// Vuetify expansion-panel component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VExpansionPanel", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VExpansionPanel : ComponentBase
{
    /// <summary>
    /// 面板的背景颜色。
    /// Background color of the panel.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 面板收起时显示的图标。
    /// Icon displayed when the panel is collapsed.
    /// </summary>
    [Parameter]
    public string? CollapseIcon { get; set; }

    /// <summary>
    /// 面板的主题颜色。
    /// Theme color of the panel.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 是否禁用面板的展开/收起操作。
    /// Whether to disable expand/collapse interaction on the panel.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 面板的阴影高度。
    /// Elevation shadow level of the panel.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 面板展开时显示的图标。
    /// Icon displayed when the panel is expanded.
    /// </summary>
    [Parameter]
    public string? ExpandIcon { get; set; }

    /// <summary>
    /// 是否隐藏展开/收起操作按钮。
    /// Whether to hide the expand/collapse action icons.
    /// </summary>
    [Parameter]
    public string? HideActions { get; set; }

    /// <summary>
    /// 是否将面板设为只读状态，不可交互。
    /// Whether the panel is read-only and non-interactive.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 面板的圆角样式。
    /// Border radius style of the panel.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 面板的正文文本内容。
    /// Text content of the panel body.
    /// </summary>
    [Parameter]
    public VuetifyTextValue? Text { get; set; }

    /// <summary>
    /// 面板的标题文本。
    /// Title text of the panel.
    /// </summary>
    [Parameter]
    public VuetifyTextValue? Title { get; set; }

    /// <summary>
    /// 面板的绑定值，用于在面板组中标识此面板。
    /// Bound value used to identify this panel within a panel group.
    /// </summary>
    [Parameter]
    public VueValue? Value { get; set; }

    /// <summary>
    /// 附加到根元素的自定义属性。
    /// Additional custom attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 面板标题的自定义内容插槽。
    /// Custom content slot for the panel title.
    /// </summary>
    [Parameter]
    public RenderFragment<VExpansionPanelTitleSlotContext>? TitleContent { get; set; }

    /// <summary>
    /// 面板正文的自定义内容插槽。
    /// Custom content slot for the panel text.
    /// </summary>
    [Parameter]
    public RenderFragment? TextContent { get; set; }

    /// <summary>
    /// 面板的默认子内容插槽。
    /// Default child content slot of the panel.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
