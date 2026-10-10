using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 网格列组件创作代理。
/// Vuetify grid column component authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VGrid")]
public sealed class VCol : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

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
    /// 列的自身垂直对齐方式。
    /// The vertical alignment of the column itself.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignSelf")]
    public string? AlignSelf { get; set; }

    /// <summary>
    /// 小型屏幕上的排序顺序。
    /// The sort order on small screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OrderSm="@(32)"；变量用 OrderSm="@value"，无需 double 后缀。字符串用 OrderSm="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("orderSm")]
    public VueStringNumberValue? OrderSm { get; set; }

    /// <summary>
    /// 中型屏幕上的排序顺序。
    /// The sort order on medium screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OrderMd="@(32)"；变量用 OrderMd="@value"，无需 double 后缀。字符串用 OrderMd="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("orderMd")]
    public VueStringNumberValue? OrderMd { get; set; }

    /// <summary>
    /// 大型屏幕上的排序顺序。
    /// The sort order on large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OrderLg="@(32)"；变量用 OrderLg="@value"，无需 double 后缀。字符串用 OrderLg="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("orderLg")]
    public VueStringNumberValue? OrderLg { get; set; }

    /// <summary>
    /// 超大型屏幕上的排序顺序。
    /// The sort order on extra-large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OrderXl="@(32)"；变量用 OrderXl="@value"，无需 double 后缀。字符串用 OrderXl="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("orderXl")]
    public VueStringNumberValue? OrderXl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的排序顺序。
    /// The sort order on extra-extra-large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OrderXxl="@(32)"；变量用 OrderXxl="@value"，无需 double 后缀。字符串用 OrderXxl="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("orderXxl")]
    public VueStringNumberValue? OrderXxl { get; set; }

    /// <summary>
    /// 默认排序顺序。
    /// The default sort order.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Order="@(32)"；变量用 Order="@value"，无需 double 后缀。字符串用 Order="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("order")]
    public VueStringNumberValue? Order { get; set; }

    /// <summary>
    /// 小型屏幕上的列偏移量。
    /// The column offset on small screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OffsetSm="@(32)"；变量用 OffsetSm="@value"，无需 double 后缀。字符串用 OffsetSm="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("offsetSm")]
    public VueStringNumberValue? OffsetSm { get; set; }

    /// <summary>
    /// 中型屏幕上的列偏移量。
    /// The column offset on medium screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OffsetMd="@(32)"；变量用 OffsetMd="@value"，无需 double 后缀。字符串用 OffsetMd="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("offsetMd")]
    public VueStringNumberValue? OffsetMd { get; set; }

    /// <summary>
    /// 大型屏幕上的列偏移量。
    /// The column offset on large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OffsetLg="@(32)"；变量用 OffsetLg="@value"，无需 double 后缀。字符串用 OffsetLg="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("offsetLg")]
    public VueStringNumberValue? OffsetLg { get; set; }

    /// <summary>
    /// 超大型屏幕上的列偏移量。
    /// The column offset on extra-large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OffsetXl="@(32)"；变量用 OffsetXl="@value"，无需 double 后缀。字符串用 OffsetXl="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("offsetXl")]
    public VueStringNumberValue? OffsetXl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的列偏移量。
    /// The column offset on extra-extra-large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OffsetXxl="@(32)"；变量用 OffsetXxl="@value"，无需 double 后缀。字符串用 OffsetXxl="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("offsetXxl")]
    public VueStringNumberValue? OffsetXxl { get; set; }

    /// <summary>
    /// 默认列偏移量。
    /// The default column offset.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Offset="@(32)"；变量用 Offset="@value"，无需 double 后缀。字符串用 Offset="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("offset")]
    public VueStringNumberValue? Offset { get; set; }

    /// <summary>
    /// 小型屏幕上的列跨度。
    /// The column span on small screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGridSpanValue?；值域为 bool | Number | string。数值用 Sm="@(32)"；变量用 Sm="@value"，无需 double 后缀。字符串用 Sm="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("sm")]
    public VuetifyGridSpanValue? Sm { get; set; }

    /// <summary>
    /// 中型屏幕上的列跨度。
    /// The column span on medium screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGridSpanValue?；值域为 bool | Number | string。数值用 Md="@(32)"；变量用 Md="@value"，无需 double 后缀。字符串用 Md="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("md")]
    public VuetifyGridSpanValue? Md { get; set; }

    /// <summary>
    /// 大型屏幕上的列跨度。
    /// The column span on large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGridSpanValue?；值域为 bool | Number | string。数值用 Lg="@(32)"；变量用 Lg="@value"，无需 double 后缀。字符串用 Lg="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("lg")]
    public VuetifyGridSpanValue? Lg { get; set; }

    /// <summary>
    /// 超大型屏幕上的列跨度。
    /// The column span on extra-large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGridSpanValue?；值域为 bool | Number | string。数值用 Xl="@(32)"；变量用 Xl="@value"，无需 double 后缀。字符串用 Xl="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("xl")]
    public VuetifyGridSpanValue? Xl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的列跨度。
    /// The column span on extra-extra-large screens.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGridSpanValue?；值域为 bool | Number | string。数值用 Xxl="@(32)"；变量用 Xxl="@value"，无需 double 后缀。字符串用 Xxl="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("xxl")]
    public VuetifyGridSpanValue? Xxl { get; set; }

    /// <summary>
    /// 默认列跨度。
    /// The default column span.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGridSpanValue?；值域为 bool | Number | string。数值用 Cols="@(32)"；变量用 Cols="@value"，无需 double 后缀。字符串用 Cols="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("cols")]
    public VuetifyGridSpanValue? Cols { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
