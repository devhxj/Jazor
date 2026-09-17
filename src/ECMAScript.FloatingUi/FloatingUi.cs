using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Floating UI 入口；提供浮层定位、中间件与自动更新能力。
/// Floating UI entry; provides floating-element positioning, middleware, and auto-update capabilities.
/// </summary>
/// <remarks>
/// 作者入口为 <c>@floating-ui/vue</c>，它再导出 <c>@floating-ui/dom</c> 的定位与中间件函数；
/// <c>@floating-ui/dom</c>、<c>@floating-ui/core</c> 与 <c>@floating-ui/utils</c> 由 manifest 资源闭包提供，不在 C# 侧单独绑定。
/// 首期未绑定 <c>platform</c>、<c>detectOverflow</c>、<c>getOverflowAncestors</c> 与 MiddlewareState 级别的自定义中间件。
/// The author entry is <c>@floating-ui/vue</c>, which re-exports the positioning and middleware
/// functions of <c>@floating-ui/dom</c>; those packages plus <c>@floating-ui/core</c> and
/// <c>@floating-ui/utils</c> are supplied by the manifest resource closure rather than separate
/// C# bindings. The first slice does not bind <c>platform</c>, <c>detectOverflow</c>,
/// <c>getOverflowAncestors</c>, or MiddlewareState-level custom middleware.
/// </remarks>
[ECMAScript("@floating-ui/vue")]
[Description("@#")]
public static partial class FloatingUi
{
}
