using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// query/mutation 状态；字符串值与 TanStack Query 的 <c>QueryStatus</c> 值域一致。
/// The query/mutation status; values mirror the TanStack Query <c>QueryStatus</c> domain.
/// </summary>
[String]
public enum VueQueryStatus
{
    /// <summary>尚无数据且未发生错误。No data yet and no error occurred.</summary>
    [Description("@#pending")]
    Pending,

    /// <summary>最近一次执行失败。The most recent execution failed.</summary>
    [Description("@#error")]
    Error,

    /// <summary>已成功取得数据。Data was fetched successfully.</summary>
    [Description("@#success")]
    Success
}

/// <summary>
/// 失效/重取过滤时选择的查询集合。
/// The query set selected when invalidating or refetching.
/// </summary>
[String]
public enum VueQueryFilterType
{
    /// <summary>全部查询。Every query.</summary>
    [Description("@#all")]
    All,

    /// <summary>仅当前活跃的查询。Only currently active queries.</summary>
    [Description("@#active")]
    Active,

    /// <summary>仅当前不活跃的查询。Only currently inactive queries.</summary>
    [Description("@#inactive")]
    Inactive
}

/// <summary>
/// query key 的组成部分：字符串、数字、布尔或普通对象。
/// One part of a query key: a string, a number, a boolean, or a plain object.
/// </summary>
/// <remarks>
/// query key 在 JavaScript 侧是任意 <c>unknown</c> 数组。本联合类型把值域收窄到常见分支，
/// 避免把 <c>object</c> 暴露到作者面；需要嵌套结构时使用 <see cref="Vue.VueProps"/> 分支。
/// A query key is an arbitrary <c>unknown</c> array in JavaScript. This union narrows the domain to
/// the common branches so <c>object</c> never reaches the authoring surface; use the
/// <see cref="Vue.VueProps"/> branch for nested structures.
/// </remarks>
[ECMAScript]
[Description("@#")]
public readonly union VueQueryKeyPart(string, Number, bool, Vue.VueProps)
{
    /// <summary>读取字符串分支；当前值不属于该分支时返回 null。Reads the string branch; null when the value is not in that branch.</summary>
    public string? AsString => Value as string;

    /// <summary>读取数字分支；当前值不属于该分支时返回 null。Reads the number branch; null when the value is not in that branch.</summary>
    public Number? AsNumber => Value is Number number ? number : default;

    /// <summary>读取布尔分支；当前值不属于该分支时返回 null。Reads the boolean branch; null when the value is not in that branch.</summary>
    public bool? AsBoolean => Value is bool flag ? flag : default;

    /// <summary>读取对象分支；当前值不属于该分支时返回 null。Reads the object branch; null when the value is not in that branch.</summary>
    public Vue.VueProps? AsProps => Value as Vue.VueProps;
}

/// <summary>
/// 查询函数；由 TanStack Query 在需要取数时调用。
/// The query function; TanStack Query invokes it whenever data must be fetched.
/// </summary>
/// <typeparam name="TData">查询结果类型。The query result type.</typeparam>
public delegate PromiseResult<TData> VueQueryQueryFunction<TData>();

/// <summary>
/// mutation 函数；接收变量并返回提交结果。
/// The mutation function; it receives the variables and returns the submission result.
/// </summary>
/// <typeparam name="TData">提交结果类型。The submission result type.</typeparam>
/// <typeparam name="TVariables">变量类型。The variables type.</typeparam>
public delegate PromiseResult<TData> VueQueryMutationFunction<TData, TVariables>(TVariables variables);

/// <summary>
/// <c>VueQueryPlugin</c> 的安装选项：指定已有 client 或让插件按配置创建。
/// Install options for <c>VueQueryPlugin</c>: supply an existing client or let the plugin create one from the config.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueQueryPluginOptions : Vue.VuePluginOptions
{
    /// <summary>由调用方创建并注入的 query client。The query client created and injected by the caller.</summary>
    [Description("@#queryClient")]
    public VueQueryClient? QueryClient { get; init; }

    /// <summary>插件创建 client 时使用的默认配置。The default configuration used when the plugin creates a client.</summary>
    [Description("@#queryClientConfig")]
    public VueQueryClientConfig? QueryClientConfig { get; init; }
}

/// <summary>
/// query client 配置：缓存实例与默认选项。
/// The query client configuration: cache instances and default options.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueQueryClientConfig
{
    /// <summary>查询缓存实现；为空时使用默认缓存。The query cache implementation; the default cache is used when omitted.</summary>
    [Description("@#queryCache")]
    public VueQueryQueryCache? QueryCache { get; init; }

    /// <summary>mutation 缓存实现；为空时使用默认缓存。The mutation cache implementation; the default cache is used when omitted.</summary>
    [Description("@#mutationCache")]
    public VueQueryMutationCache? MutationCache { get; init; }

    /// <summary>查询默认选项。The default query options.</summary>
    [Description("@#defaultOptions")]
    public VueQueryDefaultOptions? DefaultOptions { get; init; }
}

/// <summary>
/// query client 的默认选项集合。
/// The default option set of a query client.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueQueryDefaultOptions
{
    /// <summary>查询默认选项。The default query options.</summary>
    [Description("@#queries")]
    public VueQueryQueryDefaults? Queries { get; init; }

    /// <summary>mutation 默认选项。The default mutation options.</summary>
    [Description("@#mutations")]
    public VueQueryMutationDefaults? Mutations { get; init; }
}

/// <summary>
/// 查询默认选项；只收录首期需要的确定行为。
/// Default query options; only the deterministic behaviors required by the first slice.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueQueryQueryDefaults
{
    /// <summary>数据被视为过期前的毫秒数。Milliseconds before data is considered stale.</summary>
    [Description("@#staleTime")]
    public Number? StaleTime { get; init; }

    /// <summary>未使用查询被回收前的毫秒数。Milliseconds before unused queries are garbage collected.</summary>
    [Description("@#gcTime")]
    public Number? GcTime { get; init; }

    /// <summary>失败后的重试次数。The number of retries after a failure.</summary>
    [Description("@#retry")]
    public Number? Retry { get; init; }

    /// <summary>窗口重新获得焦点时是否自动重取。Whether to refetch automatically when the window regains focus.</summary>
    [Description("@#refetchOnWindowFocus")]
    public bool? RefetchOnWindowFocus { get; init; }

    /// <summary>网络重连时是否自动重取。Whether to refetch automatically when the network reconnects.</summary>
    [Description("@#refetchOnReconnect")]
    public bool? RefetchOnReconnect { get; init; }
}

/// <summary>
/// mutation 默认选项。
/// Default mutation options.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueQueryMutationDefaults
{
    /// <summary>失败后的重试次数。The number of retries after a failure.</summary>
    [Description("@#retry")]
    public Number? Retry { get; init; }
}

/// <summary>
/// 查询缓存；由 TanStack Query 创建并维护。
/// The query cache; created and maintained by TanStack Query.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VueQueryQueryCache
{
    private VueQueryQueryCache()
    {
    }
}

/// <summary>
/// mutation 缓存；由 TanStack Query 创建并维护。
/// The mutation cache; created and maintained by TanStack Query.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VueQueryMutationCache
{
    private VueQueryMutationCache()
    {
    }
}

/// <summary>
/// <c>useQuery()</c> 与 <c>fetchQuery()</c> 共享的选项对象。
/// Options shared by <c>useQuery()</c> and <c>fetchQuery()</c>.
/// </summary>
/// <typeparam name="TData">查询结果类型。The query result type.</typeparam>
[ECMAScript]
[Description("@#")]
public record VueQueryQueryOptions<TData>
{
    /// <summary>query key；决定缓存身份。The query key; it determines the cache identity.</summary>
    [Description("@#queryKey")]
    public VueQueryKeyPart[] QueryKey { get; init; } = default!;

    /// <summary>取数函数。The fetch function.</summary>
    [Description("@#queryFn")]
    public VueQueryQueryFunction<TData> QueryFn { get; init; } = default!;

    /// <summary>是否启用该查询；为 false 时不会自动取数。Whether the query is enabled; when false it does not fetch automatically.</summary>
    [Description("@#enabled")]
    public bool? Enabled { get; init; }

    /// <summary>数据被视为过期前的毫秒数。Milliseconds before data is considered stale.</summary>
    [Description("@#staleTime")]
    public Number? StaleTime { get; init; }

    /// <summary>未使用数据被回收前的毫秒数。Milliseconds before unused data is garbage collected.</summary>
    [Description("@#gcTime")]
    public Number? GcTime { get; init; }

    /// <summary>失败后的重试次数。The number of retries after a failure.</summary>
    [Description("@#retry")]
    public Number? Retry { get; init; }

    /// <summary>窗口重新获得焦点时是否重取。Whether to refetch when the window regains focus.</summary>
    [Description("@#refetchOnWindowFocus")]
    public bool? RefetchOnWindowFocus { get; init; }

    /// <summary>挂载时是否重取。Whether to refetch on mount.</summary>
    [Description("@#refetchOnMount")]
    public bool? RefetchOnMount { get; init; }

    /// <summary>网络重连时是否重取。Whether to refetch when the network reconnects.</summary>
    [Description("@#refetchOnReconnect")]
    public bool? RefetchOnReconnect { get; init; }

    /// <summary>首次取数前的占位数据。Placeholder data used before the first fetch.</summary>
    [Description("@#placeholderData")]
    public TData? PlaceholderData { get; init; }

    /// <summary>初始数据；写入后直接进入 success 状态。Initial data; the query starts in the success state.</summary>
    [Description("@#initialData")]
    public TData? InitialData { get; init; }

    /// <summary>随查询携带的元数据。Metadata carried with the query.</summary>
    [Description("@#meta")]
    public Vue.VueDictionary? Meta { get; init; }
}

/// <summary>
/// 失效、重取、移除等批量操作的过滤条件。
/// Filter criteria for bulk operations such as invalidation, refetching, and removal.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueQueryFilterOptions
{
    /// <summary>按 query key 前缀过滤。Filter by query key prefix.</summary>
    [Description("@#queryKey")]
    public VueQueryKeyPart[]? QueryKey { get; init; }

    /// <summary>是否要求 key 完全相等。Whether the key must match exactly.</summary>
    [Description("@#exact")]
    public bool? Exact { get; init; }

    /// <summary>是否只匹配已过期的数据。Whether to match stale data only.</summary>
    [Description("@#stale")]
    public bool? Stale { get; init; }

    /// <summary>选择的查询集合。The query set to select.</summary>
    [Description("@#type")]
    public VueQueryFilterType? Type { get; init; }
}

/// <summary>
/// <c>useQuery()</c> 的返回值：数据、错误、状态与手动重取入口。
/// The <c>useQuery()</c> return value: data, error, status, and a manual refetch entry.
/// </summary>
/// <typeparam name="TData">查询结果类型。The query result type.</typeparam>
[ECMAScript]
[Description("@#")]
public abstract class VueQueryQueryReturn<TData>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueQueryQueryReturn()
    {
    }

    /// <summary>查询数据；尚无结果时为 undefined。The query data; undefined while no result exists.</summary>
    [Description("@#data")]
    public extern Vue.VueReadonlyRef<TData?> Data { get; }

    /// <summary>最近一次执行的错误；无错误时为 undefined。The error from the most recent execution, undefined when none.</summary>
    [Description("@#error")]
    public extern Vue.VueReadonlyRef<Error?> Error { get; }

    /// <summary>查询状态。The query status.</summary>
    [Description("@#status")]
    public extern Vue.VueReadonlyRef<VueQueryStatus> Status { get; }

    /// <summary>是否仍无数据且未出错。Whether the query has no data and no error yet.</summary>
    [Description("@#isPending")]
    public extern Vue.VueReadonlyRef<bool> IsPending { get; }

    /// <summary>是否正在首次加载。Whether the first load is in progress.</summary>
    [Description("@#isLoading")]
    public extern Vue.VueReadonlyRef<bool> IsLoading { get; }

    /// <summary>是否正在取数（含后台重取）。Whether a fetch is in progress, including background refetches.</summary>
    [Description("@#isFetching")]
    public extern Vue.VueReadonlyRef<bool> IsFetching { get; }

    /// <summary>是否已成功取得数据。Whether data was fetched successfully.</summary>
    [Description("@#isSuccess")]
    public extern Vue.VueReadonlyRef<bool> IsSuccess { get; }

    /// <summary>最近一次执行是否失败。Whether the most recent execution failed.</summary>
    [Description("@#isError")]
    public extern Vue.VueReadonlyRef<bool> IsError { get; }

    /// <summary>数据是否已过期。Whether the data is stale.</summary>
    [Description("@#isStale")]
    public extern Vue.VueReadonlyRef<bool> IsStale { get; }

    /// <summary>当前数据是否为占位数据。Whether the current data is placeholder data.</summary>
    [Description("@#isPlaceholderData")]
    public extern Vue.VueReadonlyRef<bool> IsPlaceholderData { get; }

    /// <summary>连续失败次数。The consecutive failure count.</summary>
    [Description("@#failureCount")]
    public extern Vue.VueReadonlyRef<Number> FailureCount { get; }

    /// <summary>最近一次成功取数的时间戳。The timestamp of the most recent successful fetch.</summary>
    [Description("@#dataUpdatedAt")]
    public extern Vue.VueReadonlyRef<Number> DataUpdatedAt { get; }

    /// <summary>手动触发一次重取。Triggers a refetch manually.</summary>
    [Description("@#refetch")]
    public extern PromiseResult Refetch();
}

/// <summary>
/// <c>useMutation()</c> 的选项对象。
/// Options for <c>useMutation()</c>.
/// </summary>
/// <typeparam name="TData">提交结果类型。The submission result type.</typeparam>
/// <typeparam name="TVariables">变量类型。The variables type.</typeparam>
[ECMAScript]
[Description("@#")]
public record VueQueryMutationOptions<TData, TVariables>
{
    /// <summary>mutation key；用于 devtools 与缓存身份。The mutation key; used by devtools and the cache identity.</summary>
    [Description("@#mutationKey")]
    public VueQueryKeyPart[]? MutationKey { get; init; }

    /// <summary>提交函数。The submission function.</summary>
    [Description("@#mutationFn")]
    public VueQueryMutationFunction<TData, TVariables> MutationFn { get; init; } = default!;

    /// <summary>失败后的重试次数。The number of retries after a failure.</summary>
    [Description("@#retry")]
    public Number? Retry { get; init; }

    /// <summary>提交成功后失效的 query key 列表。Query keys invalidated after a successful submission.</summary>
    [Description("@#onSuccessInvalidate")]
    public VueQueryKeyPart[]? OnSuccessInvalidate { get; init; }

    /// <summary>随 mutation 携带的元数据。Metadata carried with the mutation.</summary>
    [Description("@#meta")]
    public Vue.VueDictionary? Meta { get; init; }
}

/// <summary>
/// <c>useMutation()</c> 的返回值：提交入口与提交状态。
/// The <c>useMutation()</c> return value: the submission entry and the submission state.
/// </summary>
/// <typeparam name="TData">提交结果类型。The submission result type.</typeparam>
/// <typeparam name="TVariables">变量类型。The variables type.</typeparam>
[ECMAScript]
[Description("@#")]
public abstract class VueQueryMutationReturn<TData, TVariables>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueQueryMutationReturn()
    {
    }

    /// <summary>提交结果数据；尚无结果时为 undefined。The submission result data; undefined while no result exists.</summary>
    [Description("@#data")]
    public extern Vue.VueReadonlyRef<TData?> Data { get; }

    /// <summary>最近一次提交的错误；无错误时为 undefined。The error from the most recent submission, undefined when none.</summary>
    [Description("@#error")]
    public extern Vue.VueReadonlyRef<Error?> Error { get; }

    /// <summary>提交状态。The submission status.</summary>
    [Description("@#status")]
    public extern Vue.VueReadonlyRef<VueQueryStatus> Status { get; }

    /// <summary>提交是否正在进行。Whether a submission is in progress.</summary>
    [Description("@#isPending")]
    public extern Vue.VueReadonlyRef<bool> IsPending { get; }

    /// <summary>最近一次提交是否成功。Whether the most recent submission succeeded.</summary>
    [Description("@#isSuccess")]
    public extern Vue.VueReadonlyRef<bool> IsSuccess { get; }

    /// <summary>最近一次提交是否失败。Whether the most recent submission failed.</summary>
    [Description("@#isError")]
    public extern Vue.VueReadonlyRef<bool> IsError { get; }

    /// <summary>最近一次提交使用的变量。The variables used by the most recent submission.</summary>
    [Description("@#variables")]
    public extern Vue.VueReadonlyRef<TVariables?> Variables { get; }

    /// <summary>连续失败次数。The consecutive failure count.</summary>
    [Description("@#failureCount")]
    public extern Vue.VueReadonlyRef<Number> FailureCount { get; }

    /// <summary>触发一次提交；不返回 promise。Triggers a submission without returning a promise.</summary>
    /// <param name="variables">本次提交的变量。The variables for this submission.</param>
    [Description("@#mutate")]
    public extern void Mutate(TVariables variables);

    /// <summary>触发一次提交并返回其 promise。Triggers a submission and returns its promise.</summary>
    /// <param name="variables">本次提交的变量。The variables for this submission.</param>
    [Description("@#mutateAsync")]
    public extern PromiseResult<TData> MutateAsync(TVariables variables);

    /// <summary>清空提交状态。Clears the submission state.</summary>
    [Description("@#reset")]
    public extern void Reset();
}

/// <summary>
/// query client：缓存读写、失效与批量操作入口。
/// The query client: cache reads and writes, invalidation, and bulk operation entries.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueQueryClient
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueQueryClient()
    {
    }

    /// <summary>读取缓存中的查询数据。Reads cached query data.</summary>
    /// <param name="queryKey">query key。The query key.</param>
    /// <typeparam name="TData">查询结果类型。The query result type.</typeparam>
    [Description("@#getQueryData")]
    public extern TData? GetQueryData<TData>(VueQueryKeyPart[] queryKey);

    /// <summary>直接写入缓存中的查询数据。Writes cached query data directly.</summary>
    /// <param name="queryKey">query key。The query key.</param>
    /// <param name="data">要写入的数据。The data to write.</param>
    /// <typeparam name="TData">查询结果类型。The query result type.</typeparam>
    [Description("@#setQueryData")]
    public extern void SetQueryData<TData>(VueQueryKeyPart[] queryKey, TData data);

    /// <summary>主动取数并写入缓存。Fetches data on demand and writes it to the cache.</summary>
    /// <param name="options">查询选项。The query options.</param>
    /// <typeparam name="TData">查询结果类型。The query result type.</typeparam>
    [Description("@#fetchQuery")]
    public extern PromiseResult<TData> FetchQuery<TData>(VueQueryQueryOptions<TData> options);

    /// <summary>把匹配的查询标记为过期并触发重取。Marks matching queries as stale and triggers a refetch.</summary>
    /// <param name="filters">过滤条件。The filter criteria.</param>
    [Description("@#invalidateQueries")]
    public extern PromiseResult InvalidateQueries(VueQueryFilterOptions? filters = null);

    /// <summary>重取匹配的查询。Refetches matching queries.</summary>
    /// <param name="filters">过滤条件。The filter criteria.</param>
    [Description("@#refetchQueries")]
    public extern PromiseResult RefetchQueries(VueQueryFilterOptions? filters = null);

    /// <summary>重置匹配的查询到初始状态。Resets matching queries to their initial state.</summary>
    /// <param name="filters">过滤条件。The filter criteria.</param>
    [Description("@#resetQueries")]
    public extern PromiseResult ResetQueries(VueQueryFilterOptions? filters = null);

    /// <summary>取消匹配的进行中请求。Cancels in-flight requests for matching queries.</summary>
    /// <param name="filters">过滤条件。The filter criteria.</param>
    [Description("@#cancelQueries")]
    public extern PromiseResult CancelQueries(VueQueryFilterOptions? filters = null);

    /// <summary>从缓存移除匹配的查询。Removes matching queries from the cache.</summary>
    /// <param name="filters">过滤条件。The filter criteria.</param>
    [Description("@#removeQueries")]
    public extern void RemoveQueries(VueQueryFilterOptions? filters = null);

    /// <summary>清空全部缓存。Clears the whole cache.</summary>
    [Description("@#clear")]
    public extern void Clear();
}
