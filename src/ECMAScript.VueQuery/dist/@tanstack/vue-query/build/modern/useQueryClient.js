import { getClientKey } from "./utils.js";
import { hasInjectionContext, inject } from "vue-demi";
//#region src/useQueryClient.ts
/**
* Retrieves the `QueryClient` installed by `VueQueryPlugin`, via Vue's `inject`. Must be called inside
* `setup()` or another function that supports an injection context.
*
* @param id - The `queryClientKey` passed to `VueQueryPlugin` — only needed when multiple `QueryClient`s are
* installed in the same app.
* @throws If called outside an injection context, or if no `QueryClient` was installed via `VueQueryPlugin`.
*
* @example
* ```vue
* <script setup lang="ts">
* import { useQueryClient } from '@tanstack/vue-query'
*
* const queryClient = useQueryClient()
*
* function invalidate() {
*   queryClient.invalidateQueries({ queryKey: ['todos'] })
* }
* <\/script>
* ```
*/
function useQueryClient(id = "") {
	if (!hasInjectionContext()) throw new Error("vue-query hooks can only be used inside setup() function or functions that support injection context.");
	const key = getClientKey(id);
	const queryClient = inject(key);
	if (!queryClient) throw new Error("No 'queryClient' found in Vue context, use 'VueQueryPlugin' to properly initialize the library.");
	return queryClient;
}
//#endregion
export { useQueryClient };

//# sourceMappingURL=useQueryClient.js.map