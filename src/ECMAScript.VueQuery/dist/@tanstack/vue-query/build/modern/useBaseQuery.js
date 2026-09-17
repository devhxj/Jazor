import { cloneDeepUnref, updateState } from "./utils.js";
import { useQueryClient } from "./useQueryClient.js";
import { shouldThrowError } from "@tanstack/query-core";
import { computed, getCurrentScope, onScopeDispose, reactive, readonly, shallowReactive, shallowReadonly, toRefs, watch } from "vue-demi";
//#region src/useBaseQuery.ts
/**
* Base implementation shared by `useQuery` and `useInfiniteQuery`.
*
* @param Observer - The observer class from query-core (`QueryObserver` or `InfiniteQueryObserver`).
* @param options - A `ref`, plain value, or reactive getter resolving to the query options.
* @param queryClient - Use this to use a custom `QueryClient`. Otherwise, the one provided by `VueQueryPlugin`
* will be used.
*/
function useBaseQuery(Observer, options, queryClient) {
	if (process.env.NODE_ENV === "development") {
		if (!getCurrentScope()) console.warn("vue-query composable like \"useQuery()\" should only be used inside a \"setup()\" function or a running effect scope. They might otherwise lead to memory leaks.");
	}
	const client = queryClient || useQueryClient();
	const defaultedOptions = computed(() => {
		let resolvedOptions = options;
		if (typeof resolvedOptions === "function") resolvedOptions = resolvedOptions();
		const clonedOptions = cloneDeepUnref(resolvedOptions);
		if (typeof clonedOptions.enabled === "function") clonedOptions.enabled = clonedOptions.enabled();
		const defaulted = client.defaultQueryOptions(clonedOptions);
		defaulted._optimisticResults = client.isRestoring?.value ? "isRestoring" : "optimistic";
		return defaulted;
	});
	const observer = new Observer(client, defaultedOptions.value);
	const state = defaultedOptions.value.shallow ? shallowReactive(observer.getCurrentResult()) : reactive(observer.getCurrentResult());
	let unsubscribe = () => {};
	if (client.isRestoring) watch(client.isRestoring, (isRestoring) => {
		if (!isRestoring) {
			unsubscribe();
			unsubscribe = observer.subscribe((result) => {
				updateState(state, result);
			});
		}
	}, { immediate: true });
	const updater = () => {
		observer.setOptions(defaultedOptions.value);
		updateState(state, observer.getCurrentResult());
	};
	watch(defaultedOptions, updater);
	onScopeDispose(() => {
		unsubscribe();
	});
	const refetch = (...args) => {
		updater();
		return state.refetch(...args);
	};
	const suspense = () => {
		return new Promise((resolve, reject) => {
			let stopWatch = () => {};
			const run = () => {
				if (defaultedOptions.value.enabled !== false) {
					observer.setOptions(defaultedOptions.value);
					const optimisticResult = observer.getOptimisticResult(defaultedOptions.value);
					if (optimisticResult.isStale) {
						stopWatch();
						observer.fetchOptimistic(defaultedOptions.value).then(resolve, (error) => {
							if (shouldThrowError(defaultedOptions.value.throwOnError, [error, observer.getCurrentQuery()])) reject(error);
							else resolve(observer.getCurrentResult());
						});
					} else {
						stopWatch();
						resolve(optimisticResult);
					}
				}
			};
			run();
			stopWatch = watch(defaultedOptions, run);
		});
	};
	watch(() => state.error, (error) => {
		if (state.isError && !state.isFetching && shouldThrowError(defaultedOptions.value.throwOnError, [error, observer.getCurrentQuery()])) throw error;
	});
	const readonlyState = defaultedOptions.value.shallow ? shallowReadonly(state) : readonly(state);
	const object = toRefs(readonlyState);
	for (const key in state) if (typeof state[key] === "function") object[key] = state[key];
	object.suspense = suspense;
	object.refetch = refetch;
	return object;
}
//#endregion
export { useBaseQuery };

//# sourceMappingURL=useBaseQuery.js.map