import { cloneDeepUnref } from "./utils.js";
import { useQueryClient } from "./useQueryClient.js";
import { noop } from "@tanstack/query-core";
import { getCurrentScope, unref, watchEffect } from "vue-demi";
//#region src/usePrefetchInfiniteQuery.ts
function isGetter(value) {
	return typeof value === "function";
}
/**
* `usePrefetchInfiniteQuery` does not return anything — it fires a prefetch as a reactive side effect, useful
* for kicking off a fetch ahead of the component that will actually render the data with `useInfiniteQuery`.
* You can pass everything to `usePrefetchInfiniteQuery` that you can pass to `queryClient.infiniteQuery`,
* though `queryKey`, `initialPageParam`, and `getNextPageParam` are always required, and `queryFn` is required
* unless a default query function has been defined.
*
* `getNextPageParam` receives both the last page of the infinite list of data and the full array of all pages,
* as well as pageParam information, and should return a single variable that will be passed to your query
* function as `context.pageParam`. Return `undefined` or `null` to indicate there is no next page available.
*
* The prefetch is skipped if the query already has any cached state — including a `pending`/`error` state left
* over from a previous attempt — so it won't refetch data that's already there or already in flight. It
* re-runs whenever a reactive dependency in `options` (built with `infiniteQueryOptions`, for example) changes.
*
* Fire this during render, before a suspense boundary that wraps a component using `useInfiniteQuery`'s
* `suspense()` — see the {@link https://tanstack.com/query/latest/docs/framework/vue/guides/suspense | Suspense guide}.
*
* @param options - A `ref`, plain value, or reactive getter resolving to the
* {@link UsePrefetchInfiniteQueryOptions} to use — everything you can pass to `queryClient.infiniteQuery`.
* @param queryClient - Use this to use a custom `QueryClient`. Otherwise, the one provided by `VueQueryPlugin`
* will be used.
* @returns `void` — nothing is returned.
*
* @example
* ```vue
* <script setup lang="ts">
* import { infiniteQueryOptions, usePrefetchInfiniteQuery } from '@tanstack/vue-query'
* import Projects from './Projects.vue'
*
* const projectsOptions = infiniteQueryOptions({
*   queryKey: ['projects'],
*   queryFn: ({ pageParam }) => fetchProjects(pageParam),
*   initialPageParam: 0,
*   getNextPageParam: (lastPage) => lastPage.nextId,
* })
*
* // Fire the prefetch as soon as this component runs, before `Projects` mounts and calls `useInfiniteQuery`.
* usePrefetchInfiniteQuery(projectsOptions)
* <\/script>
*
* <template>
*   <Projects />
* </template>
* ```
*/
function usePrefetchInfiniteQuery(options, queryClient) {
	if (process.env.NODE_ENV === "development") {
		if (!getCurrentScope()) console.warn("vue-query composable like \"useQuery()\" should only be used inside a \"setup()\" function or a running effect scope. They might otherwise lead to memory leaks.");
	}
	const client = queryClient || useQueryClient();
	watchEffect(() => {
		const resolvedOptions = isGetter(options) ? options() : unref(options);
		const clonedOptions = cloneDeepUnref(resolvedOptions);
		if (!client.getQueryState(clonedOptions.queryKey)) client.infiniteQuery(clonedOptions).then(noop).catch(noop);
	});
}
//#endregion
export { usePrefetchInfiniteQuery };

//# sourceMappingURL=usePrefetchInfiniteQuery.js.map