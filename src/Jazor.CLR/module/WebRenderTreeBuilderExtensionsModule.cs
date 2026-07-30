namespace Jazor.CLR;

/// <summary>
/// 声明 WebRenderTreeBuilder 的事件 preventDefault/stopPropagation 扩展白名单。
/// </summary>
/// <remarks>
/// 扩展方法本身不在 CLR runtime 中实现；它们由 RazorVue 宿主转换为事件修饰语义，
/// 因此这里只能加入 compiler host 已经能消费的成员。
/// </remarks>
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions")]
public static class WebRenderTreeBuilderExtensionsModule
{
    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)")]
    public extern static void _9cb974e03e3dc909(object builder, Number sequence, string eventName, bool value);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions.AddEventStopPropagationAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)")]
    public extern static void _7f14d52bd2c5bcd5(object builder, Number sequence, string eventName, bool value);
}
