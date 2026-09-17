import { cloneDeepUnref } from "./utils.js";
import { MutationCache as MutationCache$1 } from "@tanstack/query-core";
//#region src/mutationCache.ts
/**
* Vue-aware subclass of `@tanstack/query-core`'s `MutationCache`. `find`/`findAll` also accept a
* {@link MaybeRefDeep} filters object, so `ref`s can be passed directly without unwrapping. Access it via
* `queryClient.getMutationCache()` — `QueryClient` constructs one of these by default.
*/
var MutationCache = class extends MutationCache$1 {
	find(filters) {
		return super.find(cloneDeepUnref(filters));
	}
	findAll(filters = {}) {
		return super.findAll(cloneDeepUnref(filters));
	}
};
//#endregion
export { MutationCache };

//# sourceMappingURL=mutationCache.js.map