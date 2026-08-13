using ECMAScript;
using ECMAScript.Contract;

namespace Jazor.RazorVue.RazorSdk.Catalog;

/// <summary>
/// 声明 WebRenderTreeBuilder 的事件 preventDefault/stopPropagation 扩展白名单。
/// Provides the static adapter surface that keeps Razor SG extension calls bindable.
/// </summary>
/// <remarks>
/// 扩展方法本身不在 CLR runtime 中实现；它们由 RazorVue 宿主转换为事件修饰语义，
/// 因此这里只能加入 compiler host 已经能消费的成员。
/// </remarks>
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions")]
public static class WebRenderTreeBuilderExtensionsCatalog
{
    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)")]
    public extern static void _9cb974e03e3dc909(object builder, int sequence, string eventName, bool value);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions.AddEventStopPropagationAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)")]
    public extern static void _7f14d52bd2c5bcd5(object builder, int sequence, string eventName, bool value);
}
