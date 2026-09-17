using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// TanStack Vue Query 入口；提供 Vue 插件、query/mutation 组合式函数与 query client。
/// TanStack Vue Query entry; provides the Vue plugin, query/mutation composables, and the query client.
/// </summary>
/// <remarks>
/// 作者入口为 <c>@tanstack/vue-query</c>；<c>@tanstack/query-core</c>、<c>@tanstack/match-sorter-utils</c>
/// 与 <c>vue-demi</c> 由 manifest 资源闭包提供，不在 C# 侧单独绑定。
/// 首期未绑定 <c>useQueries</c>、<c>useInfiniteQuery</c>、<c>useMutationState</c>、
/// <c>usePrefetchQuery</c>/<c>usePrefetchInfiniteQuery</c>、<c>queryOptions</c>/<c>infiniteQueryOptions</c>/<c>mutationOptions</c>
/// 辅助函数，以及直接 <c>new QueryClient()</c>。
/// The author entry is <c>@tanstack/vue-query</c>; <c>@tanstack/query-core</c>,
/// <c>@tanstack/match-sorter-utils</c>, and <c>vue-demi</c> are supplied by the manifest resource
/// closure rather than separate C# bindings. The first slice does not bind <c>useQueries</c>,
/// <c>useInfiniteQuery</c>, <c>useMutationState</c>, <c>usePrefetchQuery</c>/<c>usePrefetchInfiniteQuery</c>,
/// the <c>queryOptions</c>/<c>infiniteQueryOptions</c>/<c>mutationOptions</c> helpers, or a direct
/// <c>new QueryClient()</c> constructor.
/// </remarks>
[ECMAScript("@tanstack/vue-query")]
[Description("@#")]
public static partial class VueQuery
{
}
