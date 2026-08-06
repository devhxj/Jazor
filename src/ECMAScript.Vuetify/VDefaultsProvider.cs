using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 默认值提供者创作代理，用于作用域组件默认值。
/// Vuetify defaults-provider authoring proxy for scoped component defaults.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDefaultsProvider")]
public sealed class VDefaultsProvider : ComponentBase
{
    /// <summary>
    /// 子组件的默认属性值。
    /// Default prop values for descendant components.
    /// </summary>
    [Parameter]
    public VueProps? Defaults { get; set; }

    /// <summary>
    /// 是否禁用默认值提供。
    /// Whether to disable the defaults provider.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 重置默认值的作用域深度。
    /// Scope depth at which to reset defaults.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Reset { get; set; }

    /// <summary>
    /// 是否作为根默认值提供者。
    /// Whether to act as the root defaults provider.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Root { get; set; }

    /// <summary>
    /// 是否将默认值限制在当前作用域内。
    /// Whether to scope defaults to the current provider only.
    /// </summary>
    [Parameter]
    public bool Scoped { get; set; }

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
