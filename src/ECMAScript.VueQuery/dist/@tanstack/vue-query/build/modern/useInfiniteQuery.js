import { useBaseQuery } from "./useBaseQuery.js";
import { InfiniteQueryObserver } from "@tanstack/query-core";
//#region src/useInfiniteQuery.ts
function useInfiniteQuery(options, queryClient) {
	return useBaseQuery(InfiniteQueryObserver, options, queryClient);
}
//#endregion
export { useInfiniteQuery };

//# sourceMappingURL=useInfiniteQuery.js.map