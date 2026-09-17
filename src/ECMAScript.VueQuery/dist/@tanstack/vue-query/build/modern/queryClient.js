import { cloneDeepUnref } from "./utils.js";
import { QueryCache as QueryCache$1 } from "./queryCache.js";
import { MutationCache as MutationCache$1 } from "./mutationCache.js";
import { QueryClient as QueryClient$1 } from "@tanstack/query-core";
import { nextTick, ref } from "vue-demi";
//#region src/queryClient.ts
/**
* Vue-aware subclass of `@tanstack/query-core`'s `QueryClient`. Methods that accept `options` (such as
* `CancelOptions` or `InvalidateOptions`) or filters (such as the `QueryFilters` accepted by
* `invalidateQueries`) also accept a {@link MaybeRefDeep} version of it, so you can pass `ref`s directly
* without unwrapping them yourself — e.g. `queryClient.invalidateQueries({ queryKey: ['post', myRef] })`.
* Install one on your app with `VueQueryPlugin`, or retrieve it with `useQueryClient`.
*/
var QueryClient = class extends QueryClient$1 {
	constructor(config = {}) {
		const vueQueryConfig = {
			defaultOptions: config.defaultOptions,
			queryCache: config.queryCache || new QueryCache$1(),
			mutationCache: config.mutationCache || new MutationCache$1()
		};
		super(vueQueryConfig);
		this.isRestoring = ref(false);
	}
	isFetching(filters = {}) {
		return super.isFetching(cloneDeepUnref(filters));
	}
	isMutating(filters = {}) {
		return super.isMutating(cloneDeepUnref(filters));
	}
	getQueryData(queryKey) {
		return super.getQueryData(cloneDeepUnref(queryKey));
	}
	ensureQueryData(options) {
		return super.ensureQueryData(cloneDeepUnref(options));
	}
	getQueriesData(filters) {
		return super.getQueriesData(cloneDeepUnref(filters));
	}
	setQueryData(queryKey, updater, options = {}) {
		return super.setQueryData(cloneDeepUnref(queryKey), updater, cloneDeepUnref(options));
	}
	setQueriesData(filters, updater, options = {}) {
		return super.setQueriesData(cloneDeepUnref(filters), updater, cloneDeepUnref(options));
	}
	getQueryState(queryKey) {
		return super.getQueryState(cloneDeepUnref(queryKey));
	}
	removeQueries(filters = {}) {
		return super.removeQueries(cloneDeepUnref(filters));
	}
	resetQueries(filters = {}, options = {}) {
		return super.resetQueries(cloneDeepUnref(filters), cloneDeepUnref(options));
	}
	cancelQueries(filters = {}, options = {}) {
		return super.cancelQueries(cloneDeepUnref(filters), cloneDeepUnref(options));
	}
	invalidateQueries(filters = {}, options = {}) {
		const filtersCloned = cloneDeepUnref(filters);
		const optionsCloned = cloneDeepUnref(options);
		super.invalidateQueries({
			...filtersCloned,
			refetchType: "none"
		}, optionsCloned);
		if (filtersCloned.refetchType === "none") return Promise.resolve();
		const refetchFilters = {
			...filtersCloned,
			type: filtersCloned.refetchType ?? filtersCloned.type ?? "active"
		};
		return nextTick().then(() => {
			return super.refetchQueries(refetchFilters, optionsCloned);
		});
	}
	refetchQueries(filters = {}, options = {}) {
		return super.refetchQueries(cloneDeepUnref(filters), cloneDeepUnref(options));
	}
	query(options) {
		return super.query(cloneDeepUnref(options));
	}
	fetchQuery(options) {
		return super.fetchQuery(cloneDeepUnref(options));
	}
	prefetchQuery(options) {
		return super.prefetchQuery(cloneDeepUnref(options));
	}
	infiniteQuery(options) {
		return super.infiniteQuery(cloneDeepUnref(options));
	}
	fetchInfiniteQuery(options) {
		return super.fetchInfiniteQuery(cloneDeepUnref(options));
	}
	prefetchInfiniteQuery(options) {
		return super.prefetchInfiniteQuery(cloneDeepUnref(options));
	}
	setDefaultOptions(options) {
		super.setDefaultOptions(cloneDeepUnref(options));
	}
	setQueryDefaults(queryKey, options) {
		super.setQueryDefaults(cloneDeepUnref(queryKey), cloneDeepUnref(options));
	}
	getQueryDefaults(queryKey) {
		return super.getQueryDefaults(cloneDeepUnref(queryKey));
	}
	setMutationDefaults(mutationKey, options) {
		super.setMutationDefaults(cloneDeepUnref(mutationKey), cloneDeepUnref(options));
	}
	getMutationDefaults(mutationKey) {
		return super.getMutationDefaults(cloneDeepUnref(mutationKey));
	}
};
//#endregion
export { QueryClient };

//# sourceMappingURL=queryClient.js.map