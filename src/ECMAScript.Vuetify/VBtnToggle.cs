using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VBtnToggle")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
/// <summary>
/// Vuetify 按钮切换组件。
/// Vuetify button toggle component.
/// </summary>
public sealed class VBtnToggle : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 基础颜色。
    /// Base color.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 边框设置。
    /// Border configuration.
    /// </summary>
    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否在按钮之间添加分隔线。
    /// Adds dividers between buttons.
    /// </summary>
    [Parameter]
    public bool Divided { get; set; }

    /// <summary>
    /// 组件的阴影高度级别。
    /// Elevation shadow level.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件的最大高度。
    /// Max height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// Max width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件的最小高度。
    /// Min height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// Min width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件的圆角大小。
    /// Border radius size.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件的视觉变体样式。
    /// Visual variant style.
    /// </summary>
    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否强制必须选中一个项。
    /// Requires at least one item to be selected.
    /// </summary>
    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 最大可选数量。
    /// Maximum number of selectable items.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 是否允许多选。
    /// Allows multiple selections.
    /// </summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// 选中项应用的 CSS 类。
    /// CSS class applied to selected items.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
