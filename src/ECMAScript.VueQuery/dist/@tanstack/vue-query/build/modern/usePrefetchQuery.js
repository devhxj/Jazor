import { cloneDeepUnref } from "./utils.js";
import { useQueryClient } from "./useQueryClient.js";
import { noop } from "@tanstack/query-core";
import { getCurrentScope, unref, watchEffect } from "vue-demi";
//#region src/usePrefetchQuery.ts
function isGetter(value) {
	return typeof value === "function";
}
/**
* `usePrefetchQuery` does not return anything — it fires a prefetch as a reactive side effect, useful for
* kicking off a fetch ahead of the component that will actually render the data with `useQuery`. You can pass
* everything to `usePrefetchQuery` that you can pass to `queryClient.query`, though `queryKey` is always
* required, and `queryFn` is required unless a default query function has been defined.
*
* The prefetch is skipped if the query already has any cached state — including a `pending`/`error` state left
* over from a previous attempt — so it won't refetch data that's already there or already in flight. It
* re-runs whenever a reactive dependency in `options` (built with `queryOptions`, for example) changes.
*
* Fire this during render, before a suspense boundary that wraps a component using `useQuery`'s `suspense()`
* — see the {@link https://tanstack.com/query/latest/docs/framework/vue/guides/suspense | Suspense guide}.
*
* @param options - A `ref`, plain value, or reactive getter resolving to the {@link UsePrefetchQueryOptions} to
* use — everything you can pass to `queryClient.query`.
* @param queryClient - Use this to use a custom `QueryClient`. Otherwise, the one provided by `VueQueryPlugin`
* will be used.
* @returns `void` — nothing is returned.
*
* @example
* ```vue
* <script setup lang="ts">
* import { usePrefetchQuery } from '@tanstack/vue-query'
* import Posts from './Posts.vue'
*
* // Fire the prefetch as soon as this component runs, before `Posts` mounts and calls `useQuery`.
* usePrefetchQuery({
*   queryKey: ['posts'],
*   queryFn: fetchPosts,
* })
* <\/script>
*
* <template>
*   <Posts />
* </template>
* ```
*/
function usePrefetchQuery(options, queryClient) {
	if (process.env.NODE_ENV === "development") {
		if (!getCurrentScope()) console.warn("vue-query composable like \"useQuery()\" should only be used inside a \"setup()\" function or a running effect scope. They might otherwise lead to memory leaks.");
	}
	const client = queryClient || useQueryClient();
	watchEffect(() => {
		const resolvedOptions = isGetter(options) ? options() : unref(options);
		const clonedOptions = cloneDeepUnref(resolvedOptions);
		if (!client.getQueryState(clonedOptions.queryKey)) client.query(clonedOptions).catch(noop);
	});
}
//#endregion
export { usePrefetchQuery };

//# sourceMappingURL=usePrefetchQuery.js.map