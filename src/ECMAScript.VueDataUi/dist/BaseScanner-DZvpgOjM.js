import { t as e } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { computed as t, createElementBlock as n, openBlock as r, ref as i, unref as a, watchEffect as o } from "vue";
//#region src/useLoading.js
function s({ config: e, dataset: n, skeletonDataset: r, skeletonConfig: s, FINAL_CONFIG: c, prepareConfig: l, callback: u = null, dsIsNumber: d = !1, allowEmptyDataset: f = !1 }) {
	let p = i(!1), m = t(() => {
		let t = a(e)?.loading ?? !1, r = a(n), i = f ? !1 : d ? [null, void 0].includes(r) : r == null || Array.isArray(r) && r.length === 0 || Object.keys(r).length === 0;
		return p.value || t || i;
	}), h = i(a(n));
	return o(() => {
		h.value = m.value ? r : a(n), c.value = m.value ? s : l(), u && u();
	}), {
		loading: m,
		FINAL_DATASET: h,
		manualLoading: p,
		skeletonDataset: r,
		skeletonConfig: s
	};
}
//#endregion
//#region src/atoms/BaseScanner.vue
var c = {}, l = { class: "vue-data-ui-scanner" };
function u(e, t) {
	return r(), n("div", l);
}
var d = /*#__PURE__*/ e(c, [["render", u], ["__scopeId", "data-v-8c8b2e12"]]);
//#endregion
export { s as n, d as t };
