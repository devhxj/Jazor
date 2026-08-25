using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 区域提供者创作代理，用于作用域区域设置、回退、RTL 和消息。
/// Vuetify locale-provider authoring proxy for scoped locale, fallback, RTL, and messages.
/// </summary>
[VueLibraryComponent("vuetify/components", "VLocaleProvider")]
public sealed class VLocaleProvider : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 当前作用域的区域设置标识符。
    /// Locale identifier for the current scope.
    /// </summary>
    [Parameter]
    [ECMAScriptName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// 区域设置不可用时的回退区域标识符。
    /// Fallback locale identifier when the primary locale is unavailable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fallbackLocale")]
    public string? FallbackLocale { get; set; }

    /// <summary>
    /// 国际化消息键值对。
    /// Internationalization message key-value pairs.
    /// </summary>
    [Parameter]
    [ECMAScriptName("messages")]
    public VueProps? Messages { get; set; }

    /// <summary>
    /// 是否启用从右到左的文本方向。
    /// Whether to enable right-to-left text direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rtl")]
    public bool? Rtl { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
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
