import { t as e } from "./useConfig-DlNpz6P8.js";
import { computed as t, isRef as n, markRaw as r, watchEffect as i } from "vue";
//#region src/getVueDataUiConfig.js
var a = /* @__PURE__ */ "vue_ui_3d_bar.vue_ui_accordion.vue_ui_age_pyramid.vue_ui_annotator.vue_ui_bullet.vue_ui_bump.vue_ui_candlestick.vue_ui_carousel_table.vue_ui_chestnut.vue_ui_chord.vue_ui_circle_pack.vue_ui_cursor.vue_ui_dag.vue_ui_dashboard.vue_ui_digits.vue_ui_donut.vue_ui_donut_evolution.vue_ui_dumbbell.vue_ui_flow.vue_ui_funnel.vue_ui_galaxy.vue_ui_gauge.vue_ui_geo.vue_ui_gizmo.vue_ui_heatmap.vue_ui_hill.vue_ui_history_plot.vue_ui_horizontal_bar.vue_ui_kpi.vue_ui_mini_loader.vue_ui_molecule.vue_ui_mood_radar.vue_ui_nested_donuts.vue_ui_onion.vue_ui_parallel_coordinate_plot.vue_ui_quadrant.vue_ui_quick_chart.vue_ui_radar.vue_ui_rating.vue_ui_relation_circle.vue_ui_ridgeline.vue_ui_rings.vue_ui_scatter.vue_ui_skeleton.vue_ui_smiley.vue_ui_spark_trend.vue_ui_sparkbar.vue_ui_sparkgauge.vue_ui_sparkhistogram.vue_ui_sparkline.vue_ui_sparkstackbar.vue_ui_stackbar.vue_ui_stackline.vue_ui_strip_plot.vue_ui_table.vue_ui_table_heatmap.vue_ui_table_sparkline.vue_ui_thermometer.vue_ui_timer.vue_ui_tiremarks.vue_ui_treemap.vue_ui_vertical_bar.vue_ui_waffle.vue_ui_wheel.vue_ui_word_cloud.vue_ui_world.vue_ui_xy.vue_ui_xy_canvas".split(".");
function o(t, n = {}) {
	return a.includes(t) ? e(n)[t] : (console.error(`VueDataUi - getVueDataUiConfig : ${t} is not a valid component name for this utility.\nUse snake case names, for example 'vue_ui_xy', 'vue_ui_donut', etc.`), {});
}
//#endregion
//#region src/getThemeConfig.js
var s = Object.fromEntries(Object.entries(/* @__PURE__ */ Object.assign({
	"./themes/vue_ui_3d_bar.json": () => import("./vue_ui_3d_bar-C4R7o-yX.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_age_pyramid.json": () => import("./vue_ui_age_pyramid-BY6c-oX_.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_bullet.json": () => import("./vue_ui_bullet-ClzdLoOv.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_bump.json": () => import("./vue_ui_bump-Vl-zYAtG.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_candlestick.json": () => import("./vue_ui_candlestick-J8jmJvxP.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_chestnut.json": () => import("./vue_ui_chestnut-D2oUhad6.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_chord.json": () => import("./vue_ui_chord-DPfS1Umc.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_circle_pack.json": () => import("./vue_ui_circle_pack-DZC_rdfn.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_dag.json": () => import("./vue_ui_dag-TsJ_azQq.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_donut.json": () => import("./vue_ui_donut-BDGqG07h.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_donut_evolution.json": () => import("./vue_ui_donut_evolution-D1yAAIHr.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_dumbbell.json": () => import("./vue_ui_dumbbell-Bfe_jFyi.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_flow.json": () => import("./vue_ui_flow-BewZjjKG.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_funnel.json": () => import("./vue_ui_funnel-_Og4EEkO.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_galaxy.json": () => import("./vue_ui_galaxy-Ig0cc1_h.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_gauge.json": () => import("./vue_ui_gauge-Cf1RZc9q.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_geo.json": () => import("./vue_ui_geo-B8TODs-G.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_heatmap.json": () => import("./vue_ui_heatmap-B2BBBSWG.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_hill.json": () => import("./vue_ui_hill-dnltu0L-.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_history_plot.json": () => import("./vue_ui_history_plot-CuN63VEc.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_horizontal_bar.json": () => import("./vue_ui_horizontal_bar-C4J4QzXf.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_molecule.json": () => import("./vue_ui_molecule-CO9L59SF.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_mood_radar.json": () => import("./vue_ui_mood_radar-BA6LAKhk.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_nested_donuts.json": () => import("./vue_ui_nested_donuts-B8csIoVO.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_onion.json": () => import("./vue_ui_onion-1FTFFS46.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_parallel_coordinate_plot.json": () => import("./vue_ui_parallel_coordinate_plot-CBiOBira.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_quadrant.json": () => import("./vue_ui_quadrant-CDSTKTJz.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_quick_chart.json": () => import("./vue_ui_quick_chart-mZBdml3Z.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_radar.json": () => import("./vue_ui_radar-jafTED5j.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_relation_circle.json": () => import("./vue_ui_relation_circle-D0p4mXmv.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_ridgeline.json": () => import("./vue_ui_ridgeline-VM8_mx4J.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_rings.json": () => import("./vue_ui_rings-BVgD2aMn.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_scatter.json": () => import("./vue_ui_scatter-I0POnicu.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_spark_trend.json": () => import("./vue_ui_spark_trend-DxVmpkmC.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_sparkbar.json": () => import("./vue_ui_sparkbar-z6qO--Kf.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_sparkgauge.json": () => import("./vue_ui_sparkgauge-BX1MS3bA.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_sparkhistogram.json": () => import("./vue_ui_sparkhistogram-BRgvKUH6.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_sparkline.json": () => import("./vue_ui_sparkline-auZhap6Y.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_sparkstackbar.json": () => import("./vue_ui_sparkstackbar-BOjuQnZd.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_stackbar.json": () => import("./vue_ui_stackbar-COOrQQdK.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_stackline.json": () => import("./vue_ui_stackline-DQqKPA9z.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_strip_plot.json": () => import("./vue_ui_strip_plot-BaHbnnwN.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_table_heatmap.json": () => import("./vue_ui_table_heatmap-w8vx5k6f.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_table_sparkline.json": () => import("./vue_ui_table_sparkline-DAbkUrNz.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_thermometer.json": () => import("./vue_ui_thermometer-DxgqWKlE.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_tiremarks.json": () => import("./vue_ui_tiremarks-CdEPieWV.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_treemap.json": () => import("./vue_ui_treemap-DoEtkRN6.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_vertical_bar.json": () => import("./vue_ui_vertical_bar-CP-MKz1j.js").then((e) => e.default),
	"./themes/vue_ui_waffle.json": () => import("./vue_ui_waffle-DIARFc7g.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_wheel.json": () => import("./vue_ui_wheel-DZ_nR--t.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_word_cloud.json": () => import("./vue_ui_word_cloud-C-qIMNLu.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_xy.json": () => import("./vue_ui_xy-BA3-_LCx.js").then((e) => e.n).then((e) => e.default),
	"./themes/vue_ui_xy_canvas.json": () => import("./vue_ui_xy_canvas-Cb6dg3eK.js").then((e) => e.n).then((e) => e.default)
})).map(([e, t]) => {
	let n = e.match(/\/themes\/(.+)\.json$/);
	return [n ? n[1] : e, t];
})), c = /* @__PURE__ */ new Map(), l = async (e) => {
	let t = s[e];
	if (!t) {
		let e = /* @__PURE__ */ Error("ENOENT");
		throw e.code = "ENOENT", e;
	}
	return await t();
};
async function u(e) {
	if (!e) return null;
	if (c.has(e)) return c.get(e);
	try {
		let t = await l(e);
		return c.set(e, t), t;
	} catch (t) {
		return console.warn(`[getThemeConfig] Missing theme file: ${e}.json`, t), null;
	}
}
//#endregion
//#region src/data-correction.js
function d(e, t) {
	if (t <= 0 || e.length < 3) return e;
	let n = e.length, r = Array.from({ length: n });
	for (let i = 0; i < n; i += 1) {
		let n = Math.max(0, i - t), a = 0;
		for (let t = n; t <= i; t += 1) a += e[t].value;
		r[i] = a / (i - n + 1);
	}
	let i = Array.from({ length: n });
	for (let r = 0; r < n; r += 1) {
		let a = Math.min(n - 1, r + t), o = 0;
		for (let t = r; t <= a; t += 1) o += e[t].value;
		i[r] = o / (a - r + 1);
	}
	let a = e.map((e) => ({ ...e }));
	for (let e = 1; e < n - 1; e += 1) {
		let t = e / (n - 1);
		a[e].value = (1 - t) * r[e] + t * i[e];
	}
	return a;
}
function f(e, t) {
	if (t <= 0 || e.length < 3) return e;
	let n = 1 / (1 + t), r = e.length, i = Array.from({ length: r });
	i[0] = e[0].value;
	for (let t = 1; t < r; t += 1) i[t] = n * e[t].value + (1 - n) * i[t - 1];
	let a = Array.from({ length: r });
	a[r - 1] = e[r - 1].value;
	for (let t = r - 2; t >= 0; --t) a[t] = n * e[t].value + (1 - n) * a[t + 1];
	let o = e.map((e) => ({ ...e }));
	for (let e = 1; e < r - 1; e += 1) {
		let t = e / (r - 1);
		o[e].value = (1 - t) * i[e] + t * a[e];
	}
	return o;
}
function p(e, t) {
	let n = e;
	return n = d(n, t.averageWindow), n = f(n, t.smoothingTau), n;
}
//#endregion
//#region src/useObjectBindings.js
function m(e, t = [], n = !0) {
	let r = [];
	if (e && typeof e == "object") {
		if (Array.isArray(e) && n) return [];
		for (let i of Object.keys(e)) {
			let a = e[i];
			if (Array.isArray(a) && n) continue;
			let o = t.concat(i);
			a && typeof a == "object" ? r.push(...m(a, o, n)) : r.push(o);
		}
	}
	return r;
}
function h(e, t) {
	return t.reduce((e, t) => e?.[t], e);
}
function g(e, t, r) {
	let i = n(e) ? e.value : e;
	for (let e = 0; e < t.length - 1; e += 1) {
		let n = t[e];
		(!Object.prototype.hasOwnProperty.call(i, n) || typeof i[n] != "object") && (i[n] = {}), i = i[n];
	}
	i[t[t.length - 1]] = r;
}
function _(e, t, n, r) {
	let i = t.split(r), a = e;
	for (let e = 0; e < i.length - 1; e += 1) {
		let t = i[e];
		a[t] || (a[t] = {}), a = a[t];
	}
	a[i[i.length - 1]] = n;
}
function v(e, n) {
	let { delimiter: a = ".", skipArrays: o = !0 } = n || {}, s = {};
	function c() {
		Object.keys(s).forEach((e) => delete s[e]);
		let n = m(e.value, [], o);
		for (let r of n) {
			let n = r.join(a);
			s[n] = t({
				get: () => h(e.value, r),
				set: (t) => g(e.value, r, t)
			});
		}
	}
	return i(c), c(), r(new Proxy(s, {
		get(n, r) {
			return typeof r == "string" || r.startsWith("__v_") ? r in n ? Reflect.get(n, r) : (_(e.value, r, void 0, a), s[r] = t({
				get: () => h(e.value, r),
				set: (t) => g(e.value, r, t)
			}), r.startsWith("__v_") || console.warn(`Vue Data UI - useObjectBindings: no binding found for key "${r}". Please verify you are binding to a property path which exists on the object.`), "") : !0;
		},
		set(n, r, i) {
			return typeof r == "string" || r.startsWith("__v_") ? r in n ? Reflect.set(n, r, i) : (_(e.value, r, i, a), s[r] = t({
				get: () => h(e.value, r),
				set: (t) => g(e.value, r, t)
			}), r.startsWith("__v_") || console.warn(`Vue Data UI - useObjectBindings: cannot set unknown binding "${r}".`), !0) : !0;
		}
	}));
}
//#endregion
export { o as i, p as n, u as r, v as t };
