using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// VueUse 入口；以组合式函数提供响应式工具、浏览器 API 封装、存储、事件与状态能力。
/// VueUse entry; provides reactive utilities, browser API wrappers, storage, events, and state
/// capabilities as composables.
/// </summary>
/// <remarks>
/// 上游 <c>@vueuse/core</c> 是单一自包含 bundle，并再导出 <c>@vueuse/shared</c> 的基础工具；
/// 两者 peer 依赖 Vue，由 <c>ECMAScript.Vue</c> 资源库提供。首期按策展清单绑定高频 composable，
/// 未绑定的上游导出可按需在 <c>Api/VueUse.Api.cs</c> 追加。
/// The upstream <c>@vueuse/core</c> is a single self-contained bundle that re-exports the base
/// utilities of <c>@vueuse/shared</c>; both peer-depend on Vue, supplied by the
/// <c>ECMAScript.Vue</c> resource library. The first slice binds a curated list of
/// high-frequency composables; further upstream exports can be appended in <c>Api/VueUse.Api.cs</c>.
/// </remarks>
[ECMAScript("@vueuse/core")]
[Description("@#")]
public static partial class VueUse
{
}
