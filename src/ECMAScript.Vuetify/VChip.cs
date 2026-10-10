 using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 芯片组件创作代理。
/// Vuetify chip component authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VChip")]
public sealed class VChip : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 芯片的选中绑定值。
    /// The bound selected state of the chip.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; } = true;

    /// <summary>
    /// 选中状态变更回调。
    /// Callback invoked when the selected state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 组件的主题色。
    /// The theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 芯片的视觉变体样式。
    /// The visual variant style of the chip.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// The component theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 芯片尺寸。
    /// The size of the chip.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Size="@(32)"；变量用 Size="@value"，无需 double 后缀。字符串用 Size="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 链接的目标 URL。
    /// The target URL for the link.
    /// </summary>
    [Parameter]
    [ECMAScriptName("href")]
    public string? Href { get; set; }

    /// <summary>
    /// 是否替换当前历史记录条目。
    /// Whether to replace the current history entry.
    /// </summary>
    [Parameter]
    [ECMAScriptName("replace")]
    public bool Replace { get; set; }

    /// <summary>
    /// 路由链接的目标路径。
    /// The target route path for the link.
    /// </summary>
    [Parameter]
    [ECMAScriptName("to")]
    public string? To { get; set; }

    /// <summary>
    /// 是否要求精确路由匹配。
    /// Whether to require an exact route match.
    /// </summary>
    [Parameter]
    [ECMAScriptName("exact")]
    public bool Exact { get; set; }

    /// <summary>
    /// 圆角大小。
    /// The border radius size.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 芯片在组中的值。
    /// The value of the chip within a chip group.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGroupModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifyGroupModelValue[]。数值用 Value="@(32)"；变量用 Value="@value"，无需 double 后缀。字符串用 Value="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("value")]
    public VuetifyGroupModelValue? Value { get; set; }

    /// <summary>
    /// 是否禁用交互。
    /// Whether to disable interaction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中时应用的 CSS 类名。
    /// The CSS class applied when selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 组件的海拔阴影高度。
    /// The elevation shadow height of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Elevation="@(32)"；变量用 Elevation="@value"，无需 double 后缀。字符串用 Elevation="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 应用的 CSS 类。
    /// The CSS class to apply.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用的内联样式。
    /// The inline style to apply.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyStyleValue?；值域为 string | VueProps | VuetifyStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 边框样式。
    /// The border style.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBorderValue?；值域为 bool | Number | string。数值用 Border="@(32)"；变量用 Border="@value"，无需 double 后缀。字符串用 Border="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 激活时应用的 CSS 类名。
    /// The CSS class applied when active.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeClass")]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// 尾部头像 URL。
    /// The URL of the append avatar image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("appendAvatar")]
    public string? AppendAvatar { get; set; }

    /// <summary>
    /// 尾部图标。
    /// The append icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 AppendIcon="@value"，并保持声明的分支类型。字符串用 AppendIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("appendIcon")]
    public VuetifyIconValue? AppendIcon { get; set; }

    /// <summary>
    /// 基础颜色。
    /// The base color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 是否显示关闭按钮。
    /// Whether to show the close button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closable")]
    public bool Closable { get; set; }

    /// <summary>
    /// 关闭按钮图标。
    /// The icon for the close button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 CloseIcon="@value"，并保持声明的分支类型。字符串用 CloseIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("closeIcon")]
    public VuetifyIconValue? CloseIcon { get; set; }

    /// <summary>
    /// 关闭按钮的无障碍标签。
    /// The accessibility label for the close button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeLabel")]
    public string? CloseLabel { get; set; }

    /// <summary>
    /// 是否可拖拽。
    /// Whether the chip is draggable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("draggable")]
    public bool Draggable { get; set; }

    /// <summary>
    /// 是否显示筛选图标。
    /// Whether to show the filter icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("filter")]
    public bool Filter { get; set; }

    /// <summary>
    /// 筛选图标。
    /// The filter icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 FilterIcon="@value"，并保持声明的分支类型。字符串用 FilterIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("filterIcon")]
    public VuetifyIconValue? FilterIcon { get; set; }

    /// <summary>
    /// 是否以标签样式显示（无圆角）。
    /// Whether to display in label style (no border radius).
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public bool Label { get; set; }

    /// <summary>
    /// 是否渲染为链接样式。
    /// Whether to render as a link style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("link")]
    public bool? Link { get; set; }

    /// <summary>
    /// 是否以药丸形状显示。
    /// Whether to display in pill shape.
    /// </summary>
    [Parameter]
    [ECMAScriptName("pill")]
    public bool Pill { get; set; }

    /// <summary>
    /// 前置头像 URL。
    /// The URL of the prepend avatar image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prependAvatar")]
    public string? PrependAvatar { get; set; }

    /// <summary>
    /// 前置图标。
    /// The prepend icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 PrependIcon="@value"，并保持声明的分支类型。字符串用 PrependIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("prependIcon")]
    public VuetifyIconValue? PrependIcon { get; set; }

    /// <summary>
    /// 涟漪效果配置。
    /// The ripple effect configuration.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRippleValue?；值域为 bool | VueProps。变量用 Ripple="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsOptions；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("ripple")]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 芯片文本内容。
    /// The text content of the chip.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTextValue?；值域为 string | Number | bool。数值用 Text="@(32)"；变量用 Text="@value"，无需 double 后缀。字符串用 Text="text"；数字字符串保持 string。投影：value?.AsString、value?.AsNumber、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("text")]
    public VuetifyTextValue? Text { get; set; }

    /// <summary>
    /// 点击事件回调。
    /// Callback invoked when the chip is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick")]
    public EventCallback OnClick { get; set; }

    /// <summary>
    /// 点击关闭按钮的事件回调。
    /// Callback invoked when the close button is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick:close")]
    public EventCallback<MouseEvent> OnClickClose { get; set; }

    /// <summary>
    /// 组选中事件回调。
    /// Callback invoked when the chip is selected in a group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onGroup:selected")]
    public EventCallback<VuetifyGroupSelectedEvent> OnGroupSelected { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认内容插槽。
    /// Default content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VChipDefaultSlotContext>? DefaultContent { get; set; }

    /// <summary>
    /// 标签内容插槽。
    /// Slot for the label content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// 前置内容插槽。
    /// Slot for the prepend content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prepend")]
    public RenderFragment? Prepend { get; set; }

    /// <summary>
    /// 尾部内容插槽。
    /// Slot for the append content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("append")]
    public RenderFragment? Append { get; set; }

    /// <summary>
    /// 关闭按钮内容插槽。
    /// Slot for the close button content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("close")]
    public RenderFragment? Close { get; set; }

    /// <summary>
    /// 筛选图标内容插槽。
    /// Slot for the filter icon content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("filter")]
    public RenderFragment? FilterContent { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
