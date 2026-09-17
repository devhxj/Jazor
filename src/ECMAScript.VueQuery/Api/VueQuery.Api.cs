using System.ComponentModel;

namespace ECMAScript;

public static partial class VueQuery
{
    /// <summary>
    /// Vue Query 插件；安装后组件才能通过 <c>useQueryClient()</c> 取得 client。
    /// The Vue Query plugin; components can only resolve a client through <c>useQueryClient()</c> after it is installed.
    /// </summary>
    /// <remarks>
    /// 通过 <c>app.Use(VueQueryPlugin, new VueQueryPluginOptions { ... })</c> 安装。
    /// Install it with <c>app.Use(VueQueryPlugin, new VueQueryPluginOptions { ... })</c>.
    /// </remarks>
    [Description("@#VueQueryPlugin")]
    public extern static Vue.VuePlugin VueQueryPlugin { get; }

    // ---------- 组合式函数 Composables ----------

    /// <summary>
    /// 取得由插件注入的 query client。
    /// Resolves the query client injected by the plugin.
    /// </summary>
    [Description("@#useQueryClient")]
    public extern static VueQueryClient UseQueryClient();

    /// <summary>
    /// 创建查询：返回数据、状态与手动重取入口。
    /// Creates a query, returning data, status, and a manual refetch entry.
    /// </summary>
    /// <param name="options">查询选项。The query options.</param>
    /// <typeparam name="TData">查询结果类型。The query result type.</typeparam>
    [Description("@#useQuery")]
    public extern static VueQueryQueryReturn<TData> UseQuery<TData>(VueQueryQueryOptions<TData> options);

    /// <summary>
    /// 创建提交：返回提交入口与提交状态。
    /// Creates a mutation, returning the submission entry and the submission state.
    /// </summary>
    /// <param name="options">提交选项。The mutation options.</param>
    /// <typeparam name="TData">提交结果类型。The submission result type.</typeparam>
    /// <typeparam name="TVariables">变量类型。The variables type.</typeparam>
    [Description("@#useMutation")]
    public extern static VueQueryMutationReturn<TData, TVariables> UseMutation<TData, TVariables>(
        VueQueryMutationOptions<TData, TVariables> options);

    /// <summary>
    /// 当前正在后台取数的查询数量。
    /// The number of queries currently fetching in the background.
    /// </summary>
    /// <param name="filters">可选的过滤条件。Optional filter criteria.</param>
    [Description("@#useIsFetching")]
    public extern static Vue.VueReadonlyRef<Number> UseIsFetching(VueQueryFilterOptions? filters = null);

    /// <summary>
    /// 当前正在进行的提交数量。
    /// The number of mutations currently in progress.
    /// </summary>
    /// <param name="filters">可选的过滤条件。Optional filter criteria.</param>
    [Description("@#useIsMutating")]
    public extern static Vue.VueReadonlyRef<Number> UseIsMutating(VueQueryFilterOptions? filters = null);
}
