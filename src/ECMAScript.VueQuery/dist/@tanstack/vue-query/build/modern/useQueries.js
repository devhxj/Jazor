import { cloneDeepUnref } from "./utils.js";
import { useQueryClient } from "./useQueryClient.js";
import { QueriesObserver } from "@tanstack/query-core";
import { computed, getCurrentScope, onScopeDispose, readonly, shallowReadonly, shallowRef, unref, watch } from "vue-demi";
//#region src/useQueries.ts
/**
* The `useQueries` composable can be used to fetch a variable number of queries.
*
* The `queries` option accepts an array of query option objects mostly identical to `useQuery`'s. It may be a
* plain array, a `ref`/reactive array (each entry tracked individually), or a getter function — pass a getter
* if the array itself (its length, or which queries are in it) depends on other reactive state.
*
* Having the same query key more than once in the array of query objects may cause some data to be shared
* between queries. To avoid this, consider de-duplicating the queries and map the results back to the desired
* structure.
*
* The `combine` option can be used to combine the results of the queries into a single value. The result will
* be structurally shared to be as referentially stable as possible.
*
* `placeholderData` is supported here too, but unlike `useQuery`, it doesn't receive information from
* previously rendered queries, because the number of queries can differ between renders.
*
* @param queryClient - Use this to use a custom `QueryClient`. Otherwise, the one provided by `VueQueryPlugin`
* will be used.
* @returns A `ref` to the combined result. Without `combine`, this is an array with all the query results, in
* the same order as the input. When `combine` is provided, this is the value returned by `combine` instead.
*
* @example
* ```vue
* <script setup lang="ts">
* import { useQueries } from '@tanstack/vue-query'
*
* const props = defineProps<{ ids: Array<number> }>()
*
* const postQueries = useQueries({
*   queries: () =>
*     props.ids.map((id) => ({
*       queryKey: ['post', id],
*       queryFn: () => fetchPost(id),
*       staleTime: Infinity,
*     })),
* })
* <\/script>
*
* <template>
*   <ul>
*     <li v-for="(query, index) in postQueries" :key="props.ids[index]">
*       <span v-if="query.isPending">Loading...</span>
*       <span v-else-if="query.isError">Error: {{ query.error.message }}</span>
*       <span v-else>{{ query.data.title }}</span>
*     </li>
*   </ul>
* </template>
* ```
*
* @example
* Combining results into a single value:
* ```vue
* <script setup lang="ts">
* import { useQueries } from '@tanstack/vue-query'
*
* const props = defineProps<{ ids: Array<number> }>()
*
* const combined = useQueries({
*   queries: () =>
*     props.ids.map((id) => ({
*       queryKey: ['post', id],
*       queryFn: () => fetchPost(id),
*     })),
*   combine: (postQueries) => ({
*     data: postQueries.map((query) => query.data),
*     isPending: postQueries.some((query) => query.isPending),
*     isError: postQueries.some((query) => query.isError),
*   }),
* })
* <\/script>
*
* <template>
*   <span v-if="combined.isPending">Loading...</span>
*   <span v-else-if="combined.isError">Error!</span>
*   <ul v-else>
*     <li v-for="post in combined.data" :key="post.id">{{ post.title }}</li>
*   </ul>
* </template>
* ```
*
* @example
* Typing `select` via {@link queryOptions}. Note that spreading a `queryOptions` result and overriding
* `select` inline still falls back to `unknown` — wrap the spread in `queryOptions` again so the override is
* resolved before it reaches `useQueries`:
* ```vue
* <script setup lang="ts">
* import { queryOptions, useQueries } from '@tanstack/vue-query'
*
* const props = defineProps<{ id: number }>()
*
* const postOptions = (id: number) =>
*   queryOptions({
*     queryKey: ['post', id],
*     queryFn: () => fetchPost(id),
*   })
*
* const [{ data: broken }] = useQueries({
*   queries: () => [
*     {
*       ...postOptions(props.id),
*       // ❌ `data` is `unknown` here
*       select: (data) => data.title,
*     },
*   ],
* })
*
* const [{ data: fixed }] = useQueries({
*   queries: () => [
*     queryOptions({
*       ...postOptions(props.id),
*       // ✅ `data` is `Post`
*       select: (data) => data.title,
*     }),
*   ],
* })
* <\/script>
*
* <template>
*   <h1>{{ fixed }}</h1>
* </template>
* ```
*/
function useQueries({ queries, ...options }, queryClient) {
	if (process.env.NODE_ENV === "development") {
		if (!getCurrentScope()) console.warn("vue-query composable like \"useQuery()\" should only be used inside a \"setup()\" function or a running effect scope. They might otherwise lead to memory leaks.");
	}
	const client = queryClient || useQueryClient();
	const defaultedQueries = computed(() => {
		const resolvedQueries = typeof queries === "function" ? queries() : queries;
		return unref(resolvedQueries).map((queryOptions) => {
			const clonedOptions = cloneDeepUnref(queryOptions);
			if (typeof clonedOptions.enabled === "function") clonedOptions.enabled = queryOptions.enabled();
			const defaulted = client.defaultQueryOptions(clonedOptions);
			defaulted._optimisticResults = client.isRestoring?.value ? "isRestoring" : "optimistic";
			return defaulted;
		});
	});
	const observer = new QueriesObserver(client, defaultedQueries.value, options);
	const getOptimisticResult = () => {
		const [results, getCombinedResult] = observer.getOptimisticResult(defaultedQueries.value, options.combine);
		return getCombinedResult(results.map((result, index) => {
			return {
				...result,
				refetch: async (...args) => {
					const [{ [index]: query }] = observer.getOptimisticResult(defaultedQueries.value, options.combine);
					return query.refetch(...args);
				}
			};
		}));
	};
	const state = shallowRef(getOptimisticResult());
	let unsubscribe = () => {};
	if (client.isRestoring) watch(client.isRestoring, (isRestoring) => {
		if (!isRestoring) {
			unsubscribe();
			unsubscribe = observer.subscribe(() => {
				state.value = getOptimisticResult();
			});
			state.value = getOptimisticResult();
		}
	}, { immediate: true });
	watch(defaultedQueries, (queriesValue) => {
		observer.setQueries(queriesValue, options);
		state.value = getOptimisticResult();
	});
	onScopeDispose(() => {
		unsubscribe();
	});
	return options.shallow ? shallowReadonly(state) : readonly(state);
}
//#endregion
export { useQueries };

//# sourceMappingURL=useQueries.js.map