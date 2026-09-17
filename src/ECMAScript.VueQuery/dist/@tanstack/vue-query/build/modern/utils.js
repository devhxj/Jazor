import { isRef, unref } from "vue-demi";
//#region src/utils.ts
/** @internal Base Vue injection key `VueQueryPlugin` provides the `QueryClient` under. */
const VUE_QUERY_CLIENT = "VUE_QUERY_CLIENT";
/** @internal Builds the injection key `useQueryClient`/`VueQueryPlugin` use for a given `queryClientKey`. */
function getClientKey(key) {
	const suffix = key ? `:${key}` : "";
	return `${VUE_QUERY_CLIENT}${suffix}`;
}
/** @internal Copies each property from `update` onto `state`, in place, for every key already on `state`. */
function updateState(state, update) {
	Object.keys(state).forEach((key) => {
		state[key] = update[key];
	});
}
function _cloneDeep(value, customize, currentKey = "", currentLevel = 0) {
	if (customize) {
		const result = customize(value, currentKey, currentLevel);
		if (result === void 0 && isRef(value)) return result;
		if (result !== void 0) return result;
	}
	if (Array.isArray(value)) return value.map((val, index) => _cloneDeep(val, customize, String(index), currentLevel + 1));
	if (typeof value === "object" && isPlainObject(value)) {
		const entries = Object.entries(value).map(([key, val]) => [key, _cloneDeep(val, customize, key, currentLevel + 1)]);
		return Object.fromEntries(entries);
	}
	return value;
}
/**
* @internal Deep-clones `value`, recursing into arrays and plain objects. `customize`, if provided, can
* intercept any node (by key and nesting level) and substitute its own return value instead of recursing
* further.
*/
function cloneDeep(value, customize) {
	return _cloneDeep(value, customize);
}
/**
* @internal Deep-clones `value` like {@link cloneDeep}, additionally unwrapping any `ref`s it encounters (and,
* if `unrefGetters` is `true`, calling any functions it encounters and unwrapping their result too). Always
* resolves `queryKey` this way, regardless of `unrefGetters` — this is what lets a `queryKey` containing `ref`s
* be passed straight through to `@tanstack/query-core`.
*/
function cloneDeepUnref(obj, unrefGetters = false) {
	return cloneDeep(obj, (val, key, level) => {
		if (level === 1 && key === "queryKey") return cloneDeepUnref(val, true);
		if (unrefGetters && isFunction(val)) return cloneDeepUnref(val(), unrefGetters);
		if (isRef(val)) return cloneDeepUnref(unref(val), unrefGetters);
	});
}
function isPlainObject(value) {
	if (Object.prototype.toString.call(value) !== "[object Object]") return false;
	const prototype = Object.getPrototypeOf(value);
	return prototype === null || prototype === Object.prototype;
}
function isFunction(value) {
	return typeof value === "function";
}
/** @internal Resolves `source` to a plain value — calls it if it's a function, otherwise deep-unwraps it. */
function toValueDeep(source) {
	return isFunction(source) ? source() : cloneDeepUnref(source);
}
//#endregion
export { VUE_QUERY_CLIENT, cloneDeep, cloneDeepUnref, getClientKey, toValueDeep, updateState };

//# sourceMappingURL=utils.js.map