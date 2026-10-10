using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签页窗口项目组件的编写代理。
/// Vuetify tabs-window-item authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VTabs")]
public sealed class VTabsWindowItem : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 值。
    /// The value used to identify this item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGroupModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifyGroupModelValue[]。数值用 Value="@(32)"；变量用 Value="@value"，无需 double 后缀。字符串用 Value="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("value")]
    public VuetifyGroupModelValue? Value { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项CSS类。
    /// CSS class applied when selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 急切加载。
    /// Forces the component to be eager-loaded.
    /// </summary>
    [Parameter]
    [ECMAScriptName("eager")]
    public bool Eager { get; set; }

    /// <summary>
    /// 过渡。
    /// Transition effect.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBooleanStringValue?；值域为 bool | string。变量用 Transition="@value"，并保持声明的分支类型。字符串用 Transition="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyBooleanStringValue? Transition { get; set; }

    /// <summary>
    /// 反向过渡。
    /// Reverse transition effect.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBooleanStringValue?；值域为 bool | string。变量用 ReverseTransition="@value"，并保持声明的分支类型。字符串用 ReverseTransition="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("reverseTransition")]
    public VuetifyBooleanStringValue? ReverseTransition { get; set; }

    /// <summary>
    /// 组选中事件。
    /// Group selected event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onGroup:selected")]
    public EventCallback<VuetifyGroupSelectedEvent> OnGroupSelected { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
