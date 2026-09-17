import { cloneDeepUnref } from "./utils.js";
import { QueryCache as QueryCache$1 } from "@tanstack/query-core";
//#region src/queryCache.ts
/**
* Vue-aware subclass of `@tanstack/query-core`'s `QueryCache`. `find`/`findAll` also accept a
* {@link MaybeRefDeep} filters object, so `ref`s can be passed directly without unwrapping. Access it via
* `queryClient.getQueryCache()` — `QueryClient` constructs one of these by default.
*/
var QueryCache = class extends QueryCache$1 {
	find(filters) {
		return super.find(cloneDeepUnref(filters));
	}
	findAll(filters = {}) {
		return super.findAll(cloneDeepUnref(filters));
	}
};
//#endregion
export { QueryCache };

//# sourceMappingURL=queryCache.js.map