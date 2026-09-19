# ECMAScript.VueQuery

TanStack Vue Query 的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游多包版本，
`manifest.json`（schema 2）和 `inventory.json` 记录 npm package、入口、完整性与许可证元数据。

Strongly typed C# bindings for TanStack Vue Query, shipped as a Jazor JS resource library with locked
upstream package versions. Runtime files remain in npm; metadata describes the dependency graph for
the generated `jazor` project.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| 作者入口 | `@tanstack/vue-query` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 闭包包 | `@tanstack/vue-query`、`@tanstack/query-core`、`@tanstack/match-sorter-utils`、`vue-demi` |
| 许可证 | MIT（`licenses/*-LICENSE`） |
| peer 依赖 | `vue` 与 `@vue/devtools-api`（均由 `ECMAScript.Vue` 资源库提供） |

`@tanstack/vue-query` 的 `build/modern` 是多模块 ESM 树，闭包按相对导入静态推导；`vue-demi` 是 Vue 3 兼容垫片，在本闭包内固定为 Vue 3 模式。各包版本分别锁定并记入 `inventory.json`。

## 首期范围 First slice

- `VueQueryPlugin` 与 `VueQueryPluginOptions`（注入现有 client 或按 `queryClientConfig` 创建）
- `useQueryClient`：缓存读写（`getQueryData`/`setQueryData`/`fetchQuery`）、失效、重取、重置、取消、移除与 `clear`
- `useQuery`：数据、错误、状态、加载/过期/占位标志与 `refetch`
- `useMutation`：`mutate`/`mutateAsync`/`reset` 与提交状态
- `useIsFetching`、`useIsMutating`

未绑定：`useQueries`、`useInfiniteQuery`、`useMutationState`、`usePrefetchQuery`/`usePrefetchInfiniteQuery`、`queryOptions`/`infiniteQueryOptions`/`mutationOptions` 辅助函数、直接 `new QueryClient()`；`queryFn` 目前不接受 TanStack 的 `{ queryKey, signal, meta }` 上下文参数，取数所需变量请在闭包中捕获；`select` 变换与响应式（ref）query key 也不在首期内。

Not bound: `useQueries`, `useInfiniteQuery`, `useMutationState`, `usePrefetchQuery`/`usePrefetchInfiniteQuery`, the `queryOptions`/`infiniteQueryOptions`/`mutationOptions` helpers, and a direct `new QueryClient()`. `queryFn` does not receive the TanStack `{ queryKey, signal, meta }` context yet, so capture fetch variables in the closure; `select` transforms and reactive (ref) query keys are also outside the first slice.

## SSR 边界 SSR boundary

`useQuery`/`useMutation` 依赖组件实例与 Vue 响应式作用域，只能在组件 setup 作用域内调用。缓存读写与 `useQueryClient` 在 SSR 期间同样需要已安装的插件上下文；本包不提供服务器端取数等价实现，SSR 数据请通过应用自有 endpoint 预取。

## 使用示例 Authoring

```csharp
using static ECMAScript.VueQuery;

var todos = UseQuery(new VueQueryQueryOptions<Todo[]>
{
    QueryKey = ["todos"],
    QueryFn = () => FetchTodos(),
    StaleTime = 30_000
});

var create = UseMutation<Todo, NewTodo>(new VueQueryMutationOptions<Todo, NewTodo>
{
    MutationFn = todo => CreateTodo(todo)
});

var client = UseQueryClient();
_ = client.InvalidateQueries(new VueQueryFilterOptions { QueryKey = ["todos"] });
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-vue-query.cs -- --version <version>
dotnet run --file scripts/csharp/generate-vue-query.cs -- --source .tmp/p3b/node_modules
```

## 测试 Tests

`src/ECMAScript.VueQuery.Test` 覆盖 import/manifest/inventory 元数据、npm integrity、上游导出 drift 与编译器 emission；`Jazor.EmitTest` 覆盖真实 package graph materialization。
