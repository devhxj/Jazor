using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// 首批 Vuetify 分页组件桩，用于 RazorVue 编写。
/// First-wave Vuetify pagination stub for RazorVue authoring.
/// </summary>
[ECMAScript("vuetify/components/VPagination")]
public sealed class VPagination : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 当前选中的页码。
    /// The currently selected page number.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 int；Razor 数值写 ModelValue="@(32)" 或 ModelValue="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public int ModelValue { get; set; }

    /// <summary>
    /// 页码变更时触发的回调。
    /// Callback invoked when the page number changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 int；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<int> ModelValueChanged { get; set; }

    /// <summary>
    /// 总页数。
    /// The total number of pages.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Length="@(32)"；变量用 Length="@value"，无需 double 后缀。字符串用 Length="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("length")]
    public VueStringNumberValue? Length { get; set; }

    /// <summary>
    /// 可见页码按钮的数量。
    /// The number of visible pagination buttons.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 TotalVisible="@(32)"；变量用 TotalVisible="@value"，无需 double 后缀。字符串用 TotalVisible="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("totalVisible")]
    public VueStringNumberValue? TotalVisible { get; set; }

    /// <summary>
    /// 是否禁用分页组件。
    /// Whether the pagination is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
