using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 默认值提供者创作代理，用于作用域组件默认值。
/// Vuetify defaults-provider authoring proxy for scoped component defaults.
/// </summary>
[ECMAScript("vuetify/components/VDefaultsProvider")]
public sealed class VDefaultsProvider : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 子组件的默认属性值。
    /// Default prop values for descendant components.
    /// </summary>
    [Parameter]
    [ECMAScriptName("defaults")]
    public VueProps? Defaults { get; set; }

    /// <summary>
    /// 是否禁用默认值提供。
    /// Whether to disable the defaults provider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 重置默认值的作用域深度。
    /// Scope depth at which to reset defaults.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Reset="@(32)"；变量用 Reset="@value"，无需 double 后缀。字符串用 Reset="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("reset")]
    public VueStringNumberValue? Reset { get; set; }

    /// <summary>
    /// 是否作为根默认值提供者。
    /// Whether to act as the root defaults provider.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBooleanStringValue?；值域为 bool | string。变量用 Root="@value"，并保持声明的分支类型。字符串用 Root="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("root")]
    public VuetifyBooleanStringValue? Root { get; set; }

    /// <summary>
    /// 是否将默认值限制在当前作用域内。
    /// Whether to scope defaults to the current provider only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scoped")]
    public bool Scoped { get; set; }

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
