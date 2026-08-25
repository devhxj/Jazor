using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VBottomNavigation")]
/// <summary>
/// Vuetify 底部导航组件。
/// Vuetify bottom navigation component.
/// </summary>
public sealed class VBottomNavigation : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 激活状态。
    /// Active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool Active { get; set; } = true;

    /// <summary>
    /// 激活状态变化时触发的事件。
    /// Event fired when active state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:active")]
    public EventCallback<bool> ActiveChanged { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Uses absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 边框设置。
    /// Border configuration.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 基础颜色。
    /// Base color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 背景颜色。
    /// Background color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 组件的阴影高度级别。
    /// Elevation shadow level.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 子元素是否自动填充空间。
    /// Grows children to fill space.
    /// </summary>
    [Parameter]
    [ECMAScriptName("grow")]
    public bool Grow { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 是否强制必须选中一个项。
    /// Requires at least one item to be selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 最大可选数量。
    /// Maximum number of selectable items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public Number? Max { get; set; }

    /// <summary>
    /// 底部导航的模式。
    /// Bottom navigation mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mode")]
    public VuetifyBottomNavigationMode? Mode { get; set; }

    /// <summary>
    /// 是否允许多选。
    /// Allows multiple selections.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// 组件的名称。
    /// Component name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 元素排序顺序。
    /// Element order.
    /// </summary>
    [Parameter]
    [ECMAScriptName("order")]
    public VueStringNumberValue? Order { get; set; }

    /// <summary>
    /// 组件的圆角大小。
    /// Border radius size.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 选中项应用的 CSS 类。
    /// CSS class applied to selected items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 自定义 CSS 类。
    /// Custom CSS class(es).
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 自定义行内样式。
    /// Custom inline style(s).
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

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
