import { computed, createApp, isReactive, isRef, toRaw, triggerRef } from "vue";
import { createPinia, setActivePinia } from "pinia";
import { defineDiagnostics } from "nostics";
//#region src/diagnostics.ts
/**
* Catalog of user-facing `@pinia/testing` diagnostics. These are thrown, so no
* reporter is attached: the diagnostic is raised by the caller and its message
* surfaces through the thrown error (attaching a console reporter would print
* the same message twice).
*/
const diagnostics = /*#__PURE__*/ defineDiagnostics({ codes: {
	PINIA_TESTING_C0001: {
		why: "You must configure the \"createSpy\" option.",
		fix: "Pass a createSpy function such as vi.fn or jest.fn to createTestingPinia().",
		docs: "https://pinia.vuejs.org/cookbook/testing.html#Specifying-the-createSpy-function"
	},
	PINIA_TESTING_C0002: {
		why: "Invalid \"createSpy\" option.",
		fix: "Pass the function itself (e.g. vi.fn), not a called spy like vi.fn().",
		docs: "https://pinia.vuejs.org/cookbook/testing.html#Specifying-the-createSpy-function"
	}
} });
//#endregion
//#region src/testing.ts
/**
* Creates a pinia instance designed for unit tests that **requires mocking**
* the stores. By default, **all actions are mocked** and therefore not
* executed. This allows you to unit test your store and components separately.
* You can change this with the `stubActions` option. If you are using jest,
* they are replaced with `jest.fn()`, otherwise, you must provide your own
* `createSpy` option.
*
* @param options - options to configure the testing pinia
* @returns a augmented pinia instance
*/
function createTestingPinia({ initialState = {}, plugins = [], stubActions = true, stubPatch = false, stubReset = false, fakeApp = false, createSpy: _createSpy } = {}) {
	const pinia = createPinia();
	pinia._p.push(({ store }) => {
		if (initialState[store.$id]) mergeReactiveObjects(store.$state, initialState[store.$id]);
	});
	plugins.forEach((plugin) => pinia._p.push(plugin));
	pinia._p.push(WritableComputed);
	const createSpy = _createSpy || typeof jest !== "undefined" && jest.fn || typeof vi !== "undefined" && vi.fn;
	/* istanbul ignore if */
	if (!createSpy) throw diagnostics.PINIA_TESTING_C0001();
	else if (typeof createSpy !== "function" || "mockReturnValue" in createSpy) throw diagnostics.PINIA_TESTING_C0002();
	pinia._p.push(({ store, options }) => {
		Object.keys(options.actions).forEach((action) => {
			if (action === "$reset") return;
			store[action] = shouldStubAction(stubActions, action, store) ? createSpy() : createSpy(store[action]);
		});
		store.$patch = stubPatch ? createSpy() : createSpy(store.$patch);
		store.$reset = stubReset ? createSpy() : createSpy(store.$reset);
	});
	if (fakeApp) createApp({}).use(pinia);
	pinia._testing = true;
	setActivePinia(pinia);
	Object.defineProperty(pinia, "app", {
		configurable: true,
		enumerable: true,
		get() {
			return this._a;
		}
	});
	return pinia;
}
function mergeReactiveObjects(target, patchToApply) {
	for (const key in patchToApply) {
		if (!Object.hasOwn(patchToApply, key)) continue;
		const subPatch = patchToApply[key];
		const targetValue = target[key];
		if (isPlainObject(targetValue) && isPlainObject(subPatch) && Object.hasOwn(target, key) && !isRef(subPatch) && !isReactive(subPatch)) target[key] = mergeReactiveObjects(targetValue, subPatch);
		else target[key] = subPatch;
	}
	return target;
}
function isPlainObject(o) {
	return o && typeof o === "object" && Object.prototype.toString.call(o) === "[object Object]" && typeof o.toJSON !== "function";
}
function isComputed(v) {
	return !!v && isRef(v) && "effect" in v;
}
function WritableComputed({ store }) {
	const rawStore = toRaw(store);
	for (const key in rawStore) {
		const originalComputed = rawStore[key];
		if (isComputed(originalComputed)) {
			const originalFn = originalComputed.fn;
			const overriddenFn = () => originalComputed._value;
			rawStore[key] = computed({
				get() {
					return originalComputed.value;
				},
				set(newValue) {
					if (newValue === void 0) {
						originalComputed.fn = originalFn;
						delete originalComputed._value;
						originalComputed._dirty = true;
					} else {
						originalComputed.fn = overriddenFn;
						originalComputed._value = newValue;
					}
					triggerRef(originalComputed);
				}
			});
		}
	}
}
/**
* Should the given action be stubbed?
*
* @param stubActions - config option
* @param action - action name
* @param store - Store instance
*/
function shouldStubAction(stubActions, action, store) {
	if (typeof stubActions === "boolean") return stubActions;
	else if (Array.isArray(stubActions)) return stubActions.includes(action);
	else if (typeof stubActions === "function") return stubActions(action, store);
	return false;
}
//#endregion
export { createTestingPinia };
