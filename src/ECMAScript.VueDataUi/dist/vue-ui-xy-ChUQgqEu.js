import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, A as n, B as r, Bt as i, Ct as a, D as o, Et as s, G as c, Gt as l, H as u, I as d, Jt as ee, Kt as te, L as ne, Lt as f, M as p, N as re, P as ie, Pt as ae, S as oe, St as se, U as ce, V as le, Vt as ue, W as de, X as fe, _ as pe, b as m, c as h, d as g, dt as me, i as _, jt as he, k as ge, l as _e, n as ve, o as ye, ot as v, q as be, qt as xe, r as Se, tt as Ce, v as we, vt as Te, w as Ee, xt as De, z as Oe, zt as ke } from "./lib-Bttd6u5E.js";
import { n as Ae, t as je } from "./useHints-Dq_w2E8B.js";
import { n as Me, r as Ne, t as Pe } from "./useTimeLabels-d2f-W1L4.js";
import { t as Fe } from "./useConfig-DlNpz6P8.js";
import { t as Ie } from "./usePrinter-DN5bYhTG.js";
import { n as Le, t as Re } from "./BaseScanner-DZvpgOjM.js";
import { t as ze } from "./useNestedProp-vPNvh7rV.js";
import { t as Be } from "./useThemeCheck-C43Tcqmk.js";
import { t as Ve } from "./useChartExport-DNiwdPmb.js";
import { t as He } from "./useTransitions-g_zBREk2.js";
import { t as Ue } from "./useStableElementSize-C7KADDKj.js";
import { t as We } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as Ge } from "./img-Bnokohej.js";
import { n as Ke } from "./Title-BE3qg9xl.js";
import { t as qe } from "./vue_ui_xy-BA3-_LCx.js";
import { t as Je } from "./Shape-C21CMlWS.js";
import { t as Ye } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as Xe } from "./DefGrad-DVBqDjhO.js";
import { t as Ze } from "./SlicerPreview-wUw1hFwe.js";
import { t as Qe } from "./vue-ui-accordion-DegI2lzR.js";
import { t as $e } from "./BaseLegendToggle-DZVucLnv.js";
import { t as et } from "./A11yDataTable-DdRsVULz.js";
import { Fragment as y, Teleport as tt, computed as b, createBlock as nt, createCommentVNode as x, createElementBlock as S, createElementVNode as C, createSlots as rt, createTextVNode as it, createVNode as at, defineAsyncComponent as ot, getCurrentInstance as st, guardReactiveProps as ct, mergeProps as lt, nextTick as ut, normalizeClass as w, normalizeProps as dt, normalizeStyle as T, onBeforeUnmount as ft, onMounted as pt, openBlock as E, ref as D, renderList as O, renderSlot as k, resolveDynamicComponent as mt, shallowRef as ht, toDisplayString as gt, toRefs as _t, unref as A, useSlots as vt, vModelCheckbox as yt, watch as bt, watchEffect as xt, withCtx as j, withDirectives as St } from "vue";
//#region src/utils/xy.js
function Ct(e) {
	return ![
		null,
		void 0,
		NaN,
		Infinity,
		-Infinity
	].includes(e);
}
function wt(e, t) {
	let n = jt(e), r = Array(n).fill(0);
	for (let e = 0; e < t.length && e < n; e += 1) r[e] = t[e] ?? null;
	return r;
}
function Tt(e, t) {
	let n = Object.create(null);
	for (let r = 0; r < e.length; r += 1) {
		let i = e[r], a = String(t(i));
		n[a] || (n[a] = []), n[a].push(i);
	}
	return n;
}
function Et(e) {
	return e && typeof e == "object" && Number.isFinite(Number(e.x)) && Number.isFinite(Number(e.y));
}
function Dt(e, t) {
	let n = Number(e), r = Number(t);
	return !Number.isFinite(n) || !Number.isFinite(r) ? !1 : !Object.is(n, r);
}
function Ot(e) {
	let t = /* @__PURE__ */ new WeakMap();
	return (n, ...r) => {
		let i = t.get(n), a = JSON.stringify(r);
		if (i && i.has(a)) return i.get(a);
		let o = e(n, ...r);
		return i || (i = /* @__PURE__ */ new Map(), t.set(n, i)), i.set(a, o), o;
	};
}
function kt(e, t) {
	let n = Number.isFinite(e) ? e : 0, r = Number.isFinite(t) ? t : 1;
	return n === r ? r = n + 1 : n > r && ([n, r] = [r, n]), {
		min: n,
		max: r
	};
}
function At(e, t, n = 0) {
	return Number.isFinite(e) && Number.isFinite(t) && Math.abs(t) > 1e-9 ? e / t : n;
}
function jt(e) {
	return Number.isFinite(e) ? Math.max(0, Math.floor(e)) : 0;
}
//#endregion
//#region src/components/vue-ui-xy.vue
var Mt = /* @__PURE__ */ e({ default: () => M }), Nt = ["id"], Pt = ["id"], Ft = ["id"], It = { style: { position: "relative" } }, Lt = [
	"viewBox",
	"aria-label",
	"aria-describedby"
], Rt = [
	"x",
	"y",
	"width",
	"height"
], zt = { key: 1 }, Bt = { class: "vue-ui-xy-grid" }, Vt = [
	"stroke",
	"x1",
	"x2",
	"y1",
	"y2"
], Ht = [
	"stroke",
	"x1",
	"x2",
	"y1",
	"y2"
], Ut = { key: 1 }, Wt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], Gt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], Kt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], qt = { key: 3 }, Jt = ["d", "stroke"], Yt = { key: 4 }, Xt = ["d", "stroke"], Zt = { key: 0 }, Qt = ["id"], $t = ["stop-color", "offset"], en = [
	"x",
	"y",
	"height",
	"width",
	"fill"
], tn = [
	"x",
	"y",
	"width"
], nn = { key: 0 }, rn = [
	"x",
	"y",
	"height",
	"width",
	"fill"
], an = [
	"x",
	"y",
	"height",
	"width",
	"rx",
	"fill",
	"stroke",
	"stroke-width"
], on = [
	"x",
	"y",
	"height",
	"width",
	"rx",
	"fill",
	"stroke",
	"stroke-width"
], sn = [
	"width",
	"x",
	"y"
], cn = [
	"stroke",
	"x1",
	"x2",
	"y1",
	"y2"
], ln = { key: 3 }, un = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], dn = {
	key: 4,
	class: "vue-ui-xy-crosshair-selection"
}, fn = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], pn = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], mn = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], hn = [
	"x",
	"y",
	"width",
	"height",
	"stroke",
	"stroke-width",
	"stroke-linecap",
	"stroke-linejoin",
	"stroke-dasharray"
], gn = ["opacity"], _n = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], vn = [
	"fill",
	"font-size",
	"transform"
], yn = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], bn = [
	"transform",
	"text-anchor",
	"font-size",
	"fill"
], xn = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], Sn = [
	"transform",
	"font-size",
	"text-anchor",
	"fill"
], Cn = {
	key: 7,
	class: "vue-ui-xy-crosshair-selection"
}, wn = [
	"transform",
	"font-size",
	"text-anchor",
	"fill"
], Tn = {
	key: 8,
	class: "vue-ui-xy-crosshair-selection"
}, En = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Dn = [
	"width",
	"x",
	"y"
], On = { style: { width: "100%" } }, kn = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], An = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], jn = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Mn = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Nn = { key: 9 }, Pn = { key: 10 }, Fn = [
	"d",
	"fill",
	"fill-opacity"
], In = { key: 0 }, Ln = ["d", "fill"], Rn = ["d", "fill"], zn = ["d", "fill"], Bn = ["d", "fill"], Vn = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Hn = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Un = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Wn = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Gn = [
	"width",
	"x",
	"y"
], Kn = { style: { width: "100%" } }, qn = { key: 11 }, Jn = [
	"text-anchor",
	"font-size",
	"transform",
	"fill",
	"stroke",
	"innerHTML"
], Yn = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], Xn = { key: 12 }, Zn = [
	"transform",
	"text-anchor",
	"font-size",
	"fill",
	"stroke",
	"innerHTML"
], Qn = { key: 13 }, $n = ["x", "y"], er = ["innerHTML"], tr = ["x", "y"], nr = ["innerHTML"], rr = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], ir = { key: 14 }, ar = [
	"transform",
	"text-anchor",
	"font-size",
	"fill",
	"stroke",
	"innerHTML"
], or = { key: 15 }, sr = ["x", "y"], cr = ["innerHTML"], lr = ["x", "y"], ur = ["innerHTML"], dr = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], fr = [
	"x",
	"y",
	"font-size",
	"fill",
	"innerHTML"
], pr = [
	"x",
	"y",
	"font-size",
	"fill",
	"innerHTML"
], mr = [
	"x",
	"y",
	"font-size",
	"fill",
	"innerHTML"
], hr = [
	"x",
	"y",
	"font-size",
	"fill",
	"innerHTML"
], gr = { key: 0 }, _r = ["id"], vr = ["fill", "stroke"], yr = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"marker-end"
], br = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"marker-end"
], xr = [
	"transform",
	"font-size",
	"fill",
	"stroke"
], Sr = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"onMouseenter"
], Cr = [
	"font-size",
	"fill",
	"transform"
], wr = [
	"x",
	"y",
	"font-size",
	"fill"
], Tr = ["opacity"], Er = [
	"text-anchor",
	"font-size",
	"fill",
	"transform",
	"onClick"
], Dr = [
	"text-anchor",
	"font-size",
	"fill",
	"transform",
	"innerHTML",
	"onClick"
], Or = { key: 18 }, kr = [
	"text-anchor",
	"font-size",
	"fill",
	"transform",
	"innerHTML"
], Ar = { key: 19 }, jr = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Mr = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Nr = [
	"y",
	"x",
	"width",
	"height",
	"fill"
], Pr = [
	"id",
	"transform",
	"font-size",
	"fill",
	"text-anchor"
], Fr = {
	key: 20,
	style: { "pointer-events": "none" }
}, Ir = ["x", "y"], Lr = ["innerHTML"], Rr = [
	"cx",
	"cy",
	"r",
	"fill"
], zr = ["data-start", "data-end"], Br = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, Vr = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Hr = {
	class: "vue-ui-xy-tag-arrow",
	height: "20",
	viewBox: "0 0 10 20",
	style: {
		position: "absolute",
		right: "100%",
		top: "50%",
		transform: "translateY(-50%)"
	}
}, Ur = ["fill"], Wr = ["innerHTML"], Gr = {
	class: "vue-ui-xy-tag-arrow",
	height: "100%",
	viewBox: "0 0 10 20",
	style: {
		position: "absolute",
		left: "100%",
		top: "50%",
		transform: "translateY(-50%)"
	}
}, Kr = ["fill"], qr = ["innerHTML"], Jr = {
	class: "vue-ui-xy-tag-arrow",
	height: "20",
	viewBox: "0 0 10 20",
	style: {
		position: "absolute",
		right: "100%",
		top: "50%",
		transform: "translateY(-50%)"
	}
}, Yr = ["fill"], Xr = ["innerHTML"], Zr = {
	class: "vue-ui-xy-tag-arrow",
	height: "100%",
	viewBox: "0 0 10 20",
	style: {
		position: "absolute",
		left: "100%",
		top: "50%",
		transform: "translateY(-50%)"
	}
}, Qr = ["fill"], $r = ["innerHTML"], ei = ["id"], ti = ["onClick", "onKeydown"], ni = {
	key: 0,
	viewBox: "0 0 20 12",
	height: "1em",
	width: "1.43em",
	"aria-hidden": "true"
}, ri = ["stroke", "fill"], ii = {
	key: 1,
	viewBox: "0 0 40 40",
	height: "1em",
	width: "1em",
	"aria-hidden": "true"
}, ai = ["fill"], oi = ["fill"], si = {
	key: 2,
	viewBox: "0 0 12 12",
	height: "1em",
	width: "1em",
	"aria-hidden": "true"
}, ci = {
	style: {
		display: "flex",
		"flex-direction": "row",
		gap: "6px",
		"align-items": "center",
		"padding-left": "6px"
	},
	"data-dom-to-png-ignore": ""
}, li = ["innerHTML"], M = /*#__PURE__*/ Ye({
	__name: "vue-ui-xy",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: Array,
			default() {
				return [];
			}
		},
		selectedXIndex: {
			type: Number,
			default: void 0
		}
	},
	emits: [
		"selectTimeLabel",
		"selectX",
		"selectLegend",
		"zoomStart",
		"zoomEnd",
		"zoomReset",
		"copyAlt"
	],
	setup(e, { expose: Ye, emit: Mt }) {
		let M = e, ui = ot(() => import("./DataTable-BbKgJ5UI.js")), di = ot(() => import("./Tooltip-DhjyfHwz.js")), fi = ot(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), pi = ot(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), mi = ot(() => import("./vue-ui-table-sparkline-Dc6HEQUQ.js").then((e) => e.n)), hi = ot(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), gi = ot(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), _i = ot(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), vi = Mt, yi = vt(), bi = st(), { vue_ui_xy: xi } = Fe(), { isThemeValid: Si, warnInvalidTheme: Ci } = Be(), wi = D(null), Ti = D(null), Ei = D(null), Di = D(null), Oi = D(null), ki = D(null), Ai = D(null), ji = D(null), Mi = D(null), Ni = D(null), Pi = D(null), Fi = D(null), Ii = D(null), Li = D(null), Ri = D(0), N = D(null), zi = D(!0), Bi = D(600), P = D(1e3), Vi = D("0 0 1000 600"), Hi = D({
			x: 0,
			y: 0
		}), Ui = D({
			line: "line",
			bar: "bar",
			plot: "plot"
		}), Wi = D(!1), Gi = D(!1), Ki = D(!1), qi = D(null), Ji = D([]), Yi = b(() => new Set(Ji.value)), F = D(be()), Xi = D(0), Zi = D(0), Qi = D(0), $i = D(!0), ea = D(0), I = D(null), ta = D(!1), na = D(!0), ra = D(!0), L = D(null), ia = D({}), aa = D(null), oa = D(!1), sa = D(null), ca = D(null), R = D(null), la = D(null), ua = D({
			x: 0,
			y: 0
		}), da = D(null), fa = D(null), z = D(null), pa = D(null), ma = D(null), ha = ht(null), ga = D(!1), _a = D(0), va = D(0), ya = Ue({
			elementRef: ha,
			minimumWidth: 2,
			minimumHeight: 2,
			stableFramesRequired: 2,
			once: !1,
			onSizeAccepted: () => {
				Sa();
			}
		});
		function ba() {
			ha.value = wi.value?.parentNode ?? null;
		}
		function xa() {
			return new Promise((e) => {
				requestAnimationFrame(() => {
					requestAnimationFrame(e);
				});
			});
		}
		async function Sa() {
			let e = ++va.value;
			ga.value = !1, await ut(), await xa(), await xa(), e === va.value && (_a.value += 1, ga.value = !0);
		}
		let Ca = D(!1);
		function wa() {
			Ca.value || (Ca.value = !0, ut(() => {
				Ca.value = !1, ba(), Sa();
			}));
		}
		let Ta = b(() => ({
			height: Bi.value,
			width: P.value
		})), Ea = D(!1), Da = D(null), Oa = D(null), B = D({
			xAxis: 18,
			yAxis: 12,
			dataLabels: 20,
			plotLabels: 10
		}), ka = D({
			plot: 3,
			line: 3,
			selectedLine: 3
		}), Aa = b(() => Math.max(ka.value.line * 1.5, ka.value.selectedLine));
		pt(() => {
			oa.value = !0, M.dataset.length && La.value && M.dataset.forEach((e, t) => {
				[null, void 0].includes(e.series) && Ce({
					componentName: "VueUiXy",
					type: "datasetSerieAttribute",
					property: "series (number[])",
					index: t
				});
			}), ba(), ya.start(), Sa();
		});
		function ja() {
			if (!Object.keys(M.config || {}).length) return xi;
			let e = ze({
				userConfig: M.config,
				defaultConfig: xi
			});
			M.config && Te(M.config, "chart.highlightArea") && (Array.isArray(M.config.chart.highlightArea) ? e.chart.highlightArea = M.config.chart.highlightArea : e.chart.highlightArea = [M.config.chart.highlightArea]), M.config && Te(M.config, "chart.annotations") && Array.isArray(M.config.chart.annotations) && M.config.chart.annotations.length ? e.chart.annotations = M.config.chart.annotations.map((e) => ze({
				defaultConfig: xi.chart.annotations[0],
				userConfig: e
			})) : e.chart.annotations = [], M.config && Te(M.config, "chart.grid.position") && M.config.chart.grid.position === "start" && M.dataset.length && M.dataset.some((e) => e.type === "bar") && (e.chart.grid.position = "middle", Te(M.config, "debug") && console.warn("Vue Data UI - VueUiXy - config.chart.grid.position was overriden to `middle` because your dataset contains a bar")), M.config && Te(M.config, "chart.highlightArea") && (Array.isArray(M.config.chart.highlightArea) ? e.chart.highlightArea = M.config.chart.highlightArea.map((e) => Ma({
				defaultConfig: xi.chart.highlightArea,
				userConfig: e
			})) : e.chart.highlightArea = Ma({
				defaultConfig: xi.chart.highlightArea,
				userConfig: M.config.chart.highlightArea
			}));
			let t = e.theme;
			if (!t) return e;
			if (!Si.value(e)) return Ci(e), e;
			let n = ze({
				userConfig: qe[t] || M.config,
				defaultConfig: e
			}), r = ze({
				userConfig: M.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : te[t] || ae
			};
		}
		function Ma({ defaultConfig: e, userConfig: t }) {
			return ze({
				defaultConfig: e,
				userConfig: t
			});
		}
		let Na = b({
			get() {
				return !!M.dataset && M.dataset.length;
			},
			set(e) {
				return e;
			}
		}), V = D(ja());
		Ae({
			config: () => V.value,
			dataset: () => M.dataset,
			component: "VueUiXy",
			rules: [
				je.emptyArray,
				{
					test: (e) => e.some((e) => e.series.length > 365),
					message: [
						"👀 One or more series have more than 365 datapoints. Consider if you really need this level of detail.",
						"",
						"▶️ Use larger time scales, or aggregated values",
						"",
						"▶️ Filter the time range by adding date inputs in your UI."
					]
				},
				{
					test: (e) => e.some((e) => e.series.length > 1095),
					message: [
						"👀 One or more series have more than 1095 datapoints. Above this threshold, the dataset is computed through an LTTB algorithm, to preserve the shape of the data without increasing the number of datapoints.",
						"",
						"▶️ If you need this level of detail, you can change config.downsample.threshold and set a higher value. Note that performance will be impacted."
					]
				},
				{
					test: (e) => e.length > 6,
					message: [
						"👀 The number of series is greater than 6. If your chart is hard to read, you might consider other ways of charting the data:",
						"",
						"▶️ A summary table with sparklines (custom, with VueUiSparkline), or VueUiTableSparkline.",
						"",
						"▶️ Individual charts for individual series, or fewer series with comparable scopes.",
						"",
						"▶️ Add filters to reduce the number of visible series on the chart, to reduce cognitive load."
					]
				}
			]
		});
		let { transitionEnabled: Pa } = He({
			config: () => V.value.transitions,
			dataset: () => M.dataset
		}), Fa = b(() => V.value.chart.userOptions.useCursorPointer), H = D({
			dataLabels: { show: !0 },
			showTooltip: !0,
			showTable: !1,
			isStacked: !1,
			useIndividualScale: !1
		});
		function Ia() {
			let e = V.value.chart.grid.labels.yAxis.stacked, t = V.value.chart.grid.labels.yAxis.useIndividualScale;
			if (!Ea.value) {
				H.value = {
					dataLabels: { show: !0 },
					showTooltip: V.value.chart.tooltip.show === !0,
					showTable: V.value.showTable === !0,
					isStacked: e,
					useIndividualScale: t
				}, Da.value = e, Oa.value = t, Ea.value = !0;
				return;
			}
			H.value.showTooltip = V.value.chart.tooltip.show === !0, e !== Da.value && (H.value.isStacked = e, Da.value = e), t !== Oa.value && (H.value.useIndividualScale = t, Oa.value = t), H.value.isStacked && (H.value.useIndividualScale = !0);
		}
		let La = b(() => V.value.debug), Ra = b(() => ee({
			defaultConfig: {
				useCssAnimation: !1,
				showTable: !1,
				chart: {
					annotations: [],
					highlightArea: [],
					backgroundColor: "#99999930",
					grid: {
						stroke: "#6A6A6A",
						labels: {
							show: !1,
							axis: {
								yLabel: "",
								xLabel: ""
							},
							xAxisLabels: { show: !1 },
							yAxis: {
								commonScaleSteps: 10,
								useNiceScale: !0,
								scaleMin: 0,
								scaleMax: 134
							},
							zeroLine: { show: !0 }
						}
					},
					padding: {
						top: 12,
						bottom: 24,
						left: 24,
						right: 24
					},
					userOptions: { show: !1 },
					zoom: {
						show: !1,
						startIndex: null,
						endIndex: null
					}
				},
				bar: {
					serieName: { show: !1 },
					labels: { show: !1 },
					border: {
						useSerieColor: !1,
						stroke: "#999999"
					}
				},
				line: {
					dot: {
						useSerieColor: !1,
						fill: "#8A8A8A"
					},
					labels: { show: !1 }
				}
			},
			userConfig: V.value.skeletonConfig ?? {}
		})), za = b(() => M.config?.skeletonDataset ?? [{
			name: "",
			series: [
				0,
				1,
				2,
				3,
				5,
				8,
				13,
				21,
				34,
				55,
				89,
				134
			],
			type: "line",
			smooth: !0,
			color: "#BABABA"
		}, {
			name: "",
			series: [
				0,
				.5,
				1,
				1.5,
				2.5,
				4,
				6.5,
				10.5,
				17,
				27.5,
				44.5,
				67
			],
			type: "bar",
			color: "#CACACA"
		}]), { loading: Ba, FINAL_DATASET: Va, manualLoading: Ha } = Le({
			..._t(M),
			FINAL_CONFIG: V,
			prepareConfig: ja,
			callback: () => {
				Promise.resolve().then(async () => {
					(!V.value.chart.zoom.keepState || !is.value || U.value.start === 0 && U.value.end === 0) && await ss(), H.value.showTable = V.value.showTable;
				});
			},
			skeletonDataset: za.value,
			skeletonConfig: ee({
				defaultConfig: V.value,
				userConfig: Ra.value
			})
		}), Ua = Ot((e, t) => s({
			data: e,
			threshold: t
		})), Wa = (e) => Ua(e, V.value.downsample.threshold), Ga = b(() => {
			let e = V.value.downsample.threshold;
			return Va.value.map((t) => Ua(t.series, e).length);
		}), Ka = b(() => {
			let e = -Infinity;
			for (let t = 0; t < Ga.value.length; t += 1) {
				let n = Ga.value[t];
				n > e && (e = n);
			}
			return e;
		}), qa = b(() => {
			let e = 0;
			for (let t = 0; t < Ga.value.length; t += 1) {
				let n = Ga.value[t];
				n > e && (e = n);
			}
			return e;
		}), Ja = b(() => Ka.value), U = D({
			start: 0,
			end: Ja.value
		}), W = D({
			start: 0,
			end: Ja.value
		}), Ya = b(() => V.value.chart.zoom.preview.enable && (W.value.start !== U.value.start || W.value.end !== U.value.end));
		function Xa(e, t) {
			W.value[e] = t;
		}
		function Za(e, t) {
			if (Ei.value) {
				if (K.value && Ds.value) {
					Ei.value.setStartValue(-Number(t)), Ei.value.setEndValue(-Number(e));
					return;
				}
				Ei.value.setStartValue(e), Ei.value.setEndValue(t);
			}
		}
		function Qa(e, t) {
			if (!K.value || !Ds.value) {
				Xa(e, t);
				return;
			}
			let n = Number(t);
			Number.isFinite(n) && Xa(e === "start" ? "end" : "start", -n);
		}
		function $a() {
			if (K.value) {
				let e = ml.value.min, t = ml.value.max, n = Number(U.value.start), r = Number(U.value.end);
				Number.isFinite(n) || (n = e), Number.isFinite(r) || (r = t), n = Math.max(e, Math.min(n, t)), r = Math.max(n, Math.min(r, t)), r <= n && (n = e, r = t), U.value = {
					start: n,
					end: r
				}, W.value.start = n, W.value.end = r, Za(n, r);
				return;
			}
			let e = Math.max(1, ...Va.value.map((e) => Wa(e.series).length)), t = Math.max(0, Math.min(U.value.start ?? 0, e - 1)), n = Math.max(t + 1, Math.min(U.value.end ?? e, e));
			(!Number.isFinite(t) || !Number.isFinite(n) || n <= t) && (t = 0, n = e), U.value = {
				start: t,
				end: n
			}, W.value.start = t, W.value.end = n, Za(t, n);
		}
		let eo = b(() => {
			let { left: e, top: t, width: n, height: r } = Y.value, i = U.value.start, a = U.value.end, o = a - i, s = n / o, c = K.value && Ds.value, l = c ? a - W.value.end : W.value.start - i, u = c ? a - W.value.start : W.value.end - i, d = Math.max(0, Math.min(o, l)), ee = Math.max(0, Math.min(o, u));
			return {
				x: e + d * s,
				y: t,
				width: (ee - d) * s,
				height: r,
				fill: V.value.chart.zoom.preview.fill,
				stroke: V.value.chart.zoom.preview.stroke,
				"stroke-width": V.value.chart.zoom.preview.strokeWidth,
				"stroke-dasharray": V.value.chart.zoom.preview.strokeDasharray,
				"stroke-linecap": "round",
				"stroke-linejoin": "round",
				style: {
					pointerEvents: "none",
					transition: "none !important",
					animation: "none !important"
				}
			};
		});
		bt(() => M.selectedXIndex, (e) => {
			if ([null, void 0].includes(M.selectedXIndex)) {
				R.value = null;
				return;
			}
			let t = e - U.value.start;
			t < 0 || e >= U.value.end ? R.value = null : R.value = t ?? null;
		}, { immediate: !0 });
		let { isPrinting: to, isImaging: no, generatePdf: ro, generateImage: io } = Ie({
			elementId: `vue-ui-xy_${F.value}`,
			fileName: V.value.chart.title.text || "vue-ui-xy",
			options: V.value.chart.userOptions.print
		}), ao = D(!1), oo = b(() => Ee(V.value.customPalette)), so = b(() => {
			let e = V.value.chart.grid.labels.yAxis.scaleMin;
			if (e == null) return null;
			let t = Number(e);
			return Number.isFinite(t) ? t : null;
		}), co = b(() => so.value === null ? ks.value : Y.value.bottom), lo = b(() => {
			let e = V.value.chart.grid.labels.yAxis.scaleMax;
			if (e == null) return null;
			let t = Number(e);
			return Number.isFinite(t) ? t : null;
		}), uo = b(() => so.value !== null || lo.value !== null), fo = b(() => rl(xo.value.filter((e) => !Yi.value.has(e.id)), 0, 1)), po = b(() => {
			let { min: e, max: t } = fo.value;
			if (!uo.value) {
				let t = e;
				return t > 0 ? 0 : t;
			}
			let n = so.value === null ? e > 0 ? 0 : e : so.value, r = lo.value === null ? t : lo.value;
			return kt(e < n ? e : n, t > r ? t : r).min;
		}), mo = b(() => {
			let { min: e, max: t } = fo.value;
			if (!uo.value) {
				let e = t;
				return po.value === e ? e + 1 : e;
			}
			let n = so.value === null ? e > 0 ? 0 : e : so.value, r = lo.value === null ? t : lo.value;
			return kt(e < n ? e : n, t > r ? t : r).max;
		}), G = b(() => V.value.chart.grid.labels.yAxis.useNiceScale ? pe(po.value, mo.value < 0 ? 0 : mo.value, V.value.chart.grid.labels.yAxis.commonScaleSteps) : we(po.value, mo.value < 0 ? 0 : mo.value, V.value.chart.grid.labels.yAxis.commonScaleSteps)), ho = b(() => [null, void 0].includes(V.value.chart.grid.labels.yAxis.scaleMin) ? G.value.min >= 0 ? 0 : Math.abs(G.value.min) : -G.value.min), K = b(() => Va.value.some((e) => Array.isArray(e.series) && e.series.some(Et)));
		xt(() => {
			K.value && Va.value.some((e) => !vo(e)) && console.warn("Vue Data UI - VueUiXy: mixed continuous and non-continuous series are not supported in the same chart.\n\nContinuous mode requires all visible 'line' and 'plot' series to use coordinate-based datasets:\n[{ x: number, y: number }]\n\nSeries of type 'bar' and standard numeric series are ignored in continuous mode to prevent runtime errors.\n\nIf you need to mix 'bar', 'line', and 'plot' series in the same chart, all series must use the standard [number, number, number...] format.");
		});
		function go(e) {
			return e && typeof e == "object" && e.x === null && e.y === null;
		}
		function _o(e) {
			return Array.isArray(e) && e.some((e) => Et(e) || go(e));
		}
		function vo(e) {
			return !K.value || ["line", "plot"].includes(e.type) && _o(e.series);
		}
		function yo(e, t) {
			return go(e) ? {
				x: null,
				y: null,
				index: t,
				raw: e,
				isNull: !0
			} : !e || typeof e != "object" ? {
				x: null,
				y: null,
				index: t,
				raw: e,
				isNull: !0,
				isInvalid: !0
			} : {
				x: Number(e.x),
				y: se(e.y) ? Number(e.y) : null,
				index: t,
				raw: e,
				isNull: !1,
				isInvalid: !1
			};
		}
		function bo(e) {
			return K.value ? e && typeof e == "object" ? e.y : null : e;
		}
		let xo = b(() => !zi.value && !K.value ? Va.value : Va.value.map((e, t) => {
			let n = `uniqueId_${t}`, r = vo(e), i = K.value ? r ? e.series : [] : Wa(e.series);
			return {
				...e,
				slotAbsoluteIndex: t,
				series: K.value ? i.map((e, t) => yo(e, t)).filter((e) => e.isInvalid ? !1 : e.x === null && e.y === null ? U.value.start <= 0 && U.value.end >= 0 : e.x >= U.value.start && e.x <= U.value.end) : i.map((e) => se(e) ? e : null).slice(U.value.start, U.value.end),
				color: oe(e.color ? e.color : oo.value[t] ? oo.value[t] : ae[t]),
				id: n,
				scaleLabel: e.scaleLabel || n
			};
		})), So = b(() => xo.value.map((e, t) => ({
			absoluteIndex: t,
			...e,
			series: K.value ? e.series.map((e) => ({
				...e,
				y: e.y === null ? null : e.y + ho.value
			})) : e.series.map((e) => e + ho.value),
			absoluteValues: K.value ? e.series.map((e) => e.y) : e.series,
			segregate: () => ic(e),
			isSegregated: Yi.value.has(e.id)
		}))), q = b(() => xo.value.map((e) => ({
			...e,
			series: K.value ? e.series.map((e) => ({
				...e,
				y: e.y === null ? null : e.y + ho.value
			})) : e.series.map((e) => e + ho.value),
			absoluteValues: K.value ? e.series.map((e) => e.y) : e.series
		})).filter((e) => !Yi.value.has(e.id))), Co = b(() => Ji.value.length === So.value.length), J = b(() => V.value.chart.grid.labels.yAxis.position === "right");
		function wo() {
			let e = 0;
			Pi.value && (e = Array.from(Pi.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = Mi.value ? Mi.value.getBoundingClientRect().width + V.value.chart.grid.labels.axis.yLabelOffsetX + B.value.yAxis : 0, n = e + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + V.value.chart.grid.labels.yAxis.crosshairSize;
			return {
				left: J.value ? 0 : n + t,
				right: J.value ? n + t : 0,
				scaleLabelsOffset: n,
				yAxisLabelWidth: t
			};
		}
		function To(e) {
			let t = Ws(e);
			if (!t) {
				Vs.value = [], Hs.value = null, ca.value = null;
				return;
			}
			let n = Number(t.point.x), r = Us.value.matchingPointsByXValue.get(n) || [];
			Vs.value = r.map((e) => ({
				...e,
				distance: 0
			})), Hs.value = pl({ x: n }), ca.value = n;
		}
		function Eo(e) {
			if (!Number.isFinite(Number(e))) {
				Vs.value = [], Hs.value = null;
				return;
			}
			let t = pl({ x: Number(e) });
			To(t), Hs.value = t;
		}
		let Do = D(0), Oo = D(0);
		function ko() {
			let e = Ni.value;
			if (!e) {
				Do.value = 0, Oo.value = 0;
				return;
			}
			try {
				let t = e.getBBox();
				Do.value = t?.height ?? 0, Oo.value = t?.x ?? 0;
			} catch {
				Do.value = 0, Oo.value = 0;
			}
		}
		let Ao = b(() => {
			let e = 0;
			if (ji.value) try {
				e = ji.value.getBBox().height || 0;
			} catch {
				e = 0;
			}
			return e + Do.value + B.value.xAxis;
		}), jo = b(() => Va.value.some((e) => e.useProgression));
		function Mo() {
			return V.value.chart.grid.labels.yAxis.crosshairSize + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + 5 + V.value.chart.grid.labels.yAxis.labelWidth + V.value.chart.grid.labels.axis.yLabelOffsetX + B.value.dataLabels * .8;
		}
		let Y = b(() => {
			_a.value;
			let e = 0, t = 0, n = 0, r = 0;
			if (V.value.chart.grid.labels.show) if (H.value.useIndividualScale && !H.value.isStacked) J.value ? t = (Va.value.length - Ji.value.length) * Mo() : e = (Va.value.length - Ji.value.length) * (V.value.chart.grid.labels.yAxis.labelWidth + 36);
			else if (H.value.useIndividualScale && H.value.isStacked) {
				let n = V.value.chart.grid.labels.yAxis.labelWidth + 36;
				J.value ? t = n : e = n;
			} else {
				let i = wo();
				e = i.left, t = i.right, n = i.scaleLabelsOffset, r = i.yAxisLabelWidth;
			}
			let i = V.value.chart.labels.fontSize * 1.1, a = jo.value ? 24 : 6;
			Ni.value && Oo.value < 0 && (e += Math.abs(Oo.value));
			let o = P.value - e - t - a - V.value.chart.padding?.left - V.value.chart.padding?.right;
			return {
				top: V.value.chart.padding?.top + i,
				right: e + (J.value ? 0 : V.value.chart.grid.labels.yAxis.crosshairSize) + V.value.chart.padding?.left + o,
				bottom: Bi.value - Ao.value - V.value.chart.padding?.bottom - V.value.chart.grid.labels.axis.xLabelOffsetY,
				left: e + (J.value ? 0 : V.value.chart.grid.labels.yAxis.crosshairSize) + V.value.chart.padding?.left,
				height: Bi.value - Ao.value - V.value.chart.padding?.top - V.value.chart.padding?.bottom - i - V.value.chart.grid.labels.axis.xLabelOffsetY,
				width: o,
				scaleLabelX: e,
				rightScaleLabelX: t,
				scaleLabelsOffset: n,
				yAxisLabelWidth: r,
				individualOffsetX: 36
			};
		}), No = b(() => {
			let e = +(V.value.chart.grid.position === "middle"), t = Z.value + e, n = v(Y.value?.top), r = v(Y.value?.bottom);
			return K.value ? V.value.chart.grid.position === "middle" ? dl.value.map((e, t, i) => {
				if (t === 0) return null;
				let a = i[t - 1], o = a.x + (e.x - a.x) / 2;
				return `M${o},${n} L${o},${r}`;
			}).filter(Boolean).join(" ") : dl.value.map((e) => `M${e.x},${n} L${e.x},${r}`).join(" ") : Array.from({ length: t }).map((e, t) => {
				let i = V.value.chart.grid.position === "middle" ? vc(t) : _c(t);
				return `M${i},${n} L${i},${r}`;
			}).join(" ");
		}), Po = b(() => {
			if (!V.value.chart.grid.labels.xAxis.showCrosshairs) return "";
			let e = V.value.chart.grid.labels.xAxis.crosshairSize, t = V.value.chart.grid.labels.xAxis.crosshairsAlwaysAtZero;
			return (K.value ? dl.value : Jo.value).map((n, r) => {
				if (!n || !n.text) return null;
				let i = K.value ? n.x : _c(r);
				return `M${i},${t ? ks.value - (ks.value === Y.value?.bottom ? 0 : e / 2) : Y.value?.bottom} L${i},${t ? ks.value + e / (ks.value === Y.value?.bottom ? 1 : 2) : Y.value?.bottom + e}`;
			}).filter(Boolean).join(" ");
		});
		function Fo() {
			return !!bi?.vnode.props?.onSelectTimeLabel;
		}
		function Io(e, t, n) {
			if (!aa.value) {
				let e = document.createElement("canvas");
				aa.value = e.getContext("2d");
			}
			return aa.value.font = `${n || "normal"} ${e}px ${t || "sans-serif"}`, aa.value;
		}
		function Lo() {
			let e = wi.value.querySelectorAll(".vue-ui-xy-tag");
			e.length && Array.from(e).forEach((e) => e.style.opacity = "0");
		}
		function Ro(e, t, n, r, i) {
			n && (ia.value[`${e}_${t}_${r}_${i}`] = n);
		}
		let zo = D(!1);
		async function Bo(e = !1) {
			await ut(), zo.value = e, ta.value && (ra.value = e);
		}
		function Vo() {
			Bo(!0);
		}
		function Ho() {
			Bo(!1);
		}
		function Uo() {
			Wi.value = !Wi.value;
		}
		let X = D([]), Wo = D([]), Go = 0;
		xt(() => {
			let e = ++Go;
			(async () => {
				let t = Ka.value, n = await Pe({
					values: V.value.chart.grid.labels.xAxisLabels.values,
					maxDatapoints: t,
					formatter: V.value.chart.grid.labels.xAxisLabels.datetimeFormatter,
					start: U.value.start,
					end: U.value.end
				});
				e === Go && (X.value = n);
			})();
		});
		let Ko = 0;
		xt(() => {
			let e = ++Ko;
			(async () => {
				let t = Ka.value, n = await Pe({
					values: V.value.chart.grid.labels.xAxisLabels.values,
					maxDatapoints: t,
					formatter: V.value.chart.grid.labels.xAxisLabels.datetimeFormatter,
					start: 0,
					end: Ja.value
				});
				e === Ko && (Wo.value = n);
			})();
		});
		let qo = b(() => {
			let e = V.value.chart.grid.labels.xAxisLabels.modulo;
			return X.value.length ? Math.min(e, [...new Set(X.value.map((e) => e.text))].length) : e;
		}), Jo = b(() => {
			let e = V.value.chart.grid.labels.xAxisLabels, t = X.value || [], n = Wo.value || [], r = U.value.start ?? 0, i = R.value, a = Z.value, o = t.map((e) => e?.text ?? ""), s = n.map((e) => e?.text ?? "");
			return h(!!e.showOnlyFirstAndLast, !!e.showOnlyAtModulo, Math.max(1, qo.value || 1), o, s, r, i, a);
		}), Yo = b(() => (Jo.value || []).map((e) => e?.text ?? "").join("|"));
		pt(() => {
			requestAnimationFrame(() => {
				ko();
			}), bt([
				() => Yo.value,
				() => V.value.chart.grid.labels.xAxisLabels.rotation,
				() => B.value.xAxis,
				() => P.value,
				() => Bi.value
			], async () => {
				await ut(), requestAnimationFrame(() => {
					ko();
				});
			}, { flush: "post" });
		}), ft(() => {
			Do.value = 0, Oo.value = 0, ya.stop(), da.value &&= (da.value.disconnect(), null);
		});
		function Xo(e, t) {
			let n = q.value.map((e) => ({
				shape: e.shape ?? e.type === "bar" ? "square" : "circle",
				name: e.name,
				color: e.color,
				type: e.type,
				value: e.absoluteValues.find((e, n) => n === t),
				comments: e.comments || [],
				prefix: e.prefix || V.value.chart.labels.prefix,
				suffix: e.suffix || V.value.chart.labels.suffix
			}));
			vi("selectTimeLabel", {
				datapoint: n,
				absoluteIndex: e.absoluteIndex,
				label: e.text
			});
		}
		let Z = b(() => {
			let e = jt((U.value.end ?? 0) - (U.value.start ?? 0));
			return Math.max(1, e);
		});
		function Zo(e) {
			K.value || (I.value = e);
		}
		function Qo(e) {
			if (!K.value || !a(e)) {
				ca.value = null, Vs.value = [], Hs.value = null;
				return;
			}
			let t = Number(e), n = Ds.value ? -t : t;
			ca.value = n, Eo(n);
		}
		let $o = b(() => K.value ? a(ca.value) || a(Hs.value) : ![null, void 0].includes(R.value) || ![null, void 0].includes(I.value));
		function es() {
			H.value.isStacked = !H.value.isStacked, H.value.isStacked ? H.value.useIndividualScale = !0 : H.value.useIndividualScale = V.value.chart.grid.labels.yAxis.useIndividualScale;
		}
		function ts(e) {
			La.value && e.autoScaling && (V.value.chart.grid.labels.yAxis.useIndividualScale || console.warn(`VueUiXy (datapoint: ${e.name}) : autoScaling only works when config.chart.grid.labels.yAxis.useIndividualScale is set to true`), V.value.chart.grid.labels.yAxis.stacked || console.warn(`VueUiXy (datapoint: ${e.name}) : autoScaling only works when config.chart.grid.labels.yAxis.stacked is set to true`));
		}
		function ns(e) {
			let t = Ka.value;
			return e > t ? t : e < 0 || V.value.chart.zoom.startIndex !== null && e < V.value.chart.zoom.startIndex ? V.value.chart.zoom.startIndex === null ? 1 : V.value.chart.zoom.startIndex + 1 : e;
		}
		let rs = D(!1), is = D(!1), as = D(0), os = D(0);
		function ss() {
			if (!rs.value) {
				rs.value = !0;
				try {
					if (K.value) {
						let e = ml.value.min, t = ml.value.max;
						as.value = e, os.value = t, U.value.start = e, U.value.end = t, W.value.start = e, W.value.end = t, is.value = !0, Za(e, t);
						return;
					}
					let { startIndex: e, endIndex: t } = V.value.chart.zoom, n = V.value.chart.zoom.keepState ? qa.value : Ka.value;
					if (V.value.chart.zoom.keepState && n <= 0) return;
					as.value = 0, os.value = n;
					let r = e ?? 0, i = t == null ? n : Math.min(ns(t + 1), n);
					cs.value = !0, U.value.start = r, U.value.end = i, W.value.start = r, W.value.end = i, $a(), is.value = !0;
				} finally {
					queueMicrotask(() => {
						cs.value = !1;
					}), rs.value = !1;
				}
			}
		}
		let cs = D(!1), ls = D(!1), us = null, ds = null, fs = {};
		function ps(e = U.value) {
			return Number(e.end) - Number(e.start);
		}
		function ms() {
			us &&= (cancelAnimationFrame(us), null), ds &&= (cancelAnimationFrame(ds), null);
		}
		function hs(e) {
			fs = {
				...fs,
				...e
			}, us && cancelAnimationFrame(us), us = requestAnimationFrame(() => {
				let e = ps(), t = {
					...U.value,
					...fs
				}, n = ps(t);
				ls.value = n !== e, U.value = t, W.value = { ...t }, fs = {}, us = null, $a(), ds && cancelAnimationFrame(ds), ds = requestAnimationFrame(() => {
					ls.value = !1, ds = null;
				});
			});
		}
		function gs(e) {
			if (K.value && Ds.value) {
				let t = Number(e);
				if (!Number.isFinite(t)) return;
				ys(-t);
				return;
			}
			vs(e);
		}
		function _s(e) {
			if (K.value && Ds.value) {
				let t = Number(e);
				if (!Number.isFinite(t)) return;
				vs(-t);
				return;
			}
			ys(e);
		}
		function vs(e) {
			if (rs.value || cs.value) return;
			let t = Number(e);
			vi("zoomStart", {
				index: t,
				isZoom: Dt(t, as.value)
			}), Number.isFinite(t) && t !== U.value.start && hs({ start: t });
		}
		function ys(e) {
			if (rs.value || cs.value) return;
			if (K.value) {
				let t = Number(e);
				if (!Number.isFinite(t)) return;
				let n = Math.max(Number(U.value.start) + 1 / 10 ** V.value.chart.grid.labels.xAxis.rounding, Math.min(t, ml.value.max));
				if (vi("zoomEnd", {
					index: n,
					isZoom: Dt(n, os.value)
				}), n === U.value.end) return;
				hs({ end: n });
				return;
			}
			let t = ns(e);
			vi("zoomEnd", {
				index: t,
				isZoom: Dt(t, os.value)
			}), t !== U.value.end && hs({ end: t });
		}
		async function bs() {
			let e = ps();
			ls.value = !0, await ss(), ps() === e ? ls.value = !1 : (await ut(), ds && cancelAnimationFrame(ds), ds = requestAnimationFrame(() => {
				ls.value = !1, ds = null;
			})), vi("zoomReset");
		}
		let xs = b(() => G.value.max + ho.value);
		function Ss(e) {
			return e / (Ct(xs.value) ? xs.value : 1);
		}
		let Cs = b(() => V.value.chart.grid.labels.yAxis.reverse);
		function ws(e) {
			return Cs.value ? 1 - e : e;
		}
		function Ts({ ratio: e, yOffset: t = 0, individualHeight: n }) {
			let r = ws(e);
			return Y.value?.bottom - t - n * r;
		}
		function Es({ value: e, scaleMin: t, scaleMax: n, yOffset: r = 0, individualHeight: i }) {
			return Ts({
				ratio: (e - t) / (n - t || 1),
				yOffset: r,
				individualHeight: i
			});
		}
		let Ds = b(() => V.value.chart.grid.labels.xAxis.reverse);
		function Os(e) {
			return Ds.value ? 1 - e : e;
		}
		let ks = b(() => isNaN(Ss(ho.value)) ? Y.value?.bottom : Y.value?.bottom - Y.value.height * Ss(ho.value));
		function As(e) {
			let t = G.value.min, n = G.value.max - t;
			return !Number.isFinite(e) || !Number.isFinite(n) || n === 0 ? Y.value?.bottom : Y.value?.bottom - Y.value.height * ((e - t) / n);
		}
		let js = b(() => G.value.min <= 0 && G.value.max >= 0 ? As(0) : G.value.min > 0 ? As(G.value.min) : As(G.value.max));
		function Ms(e) {
			let t = ![null, void 0].includes(V.value.chart.grid.labels.yAxis.scaleMin) && V.value.chart.grid.labels.yAxis.scaleMin > 0 && po.value >= 0 ? Y.value?.bottom : ks.value;
			return e.value >= 0 ? m(t - e.y <= 0 ? 1e-5 : t - e.y) : m(e.y - ks.value <= 0 ? 1e-5 : e.y - ks.value);
		}
		function Ns(e) {
			return e.value >= 0 ? m(e.zeroPosition - e.y <= 0 ? 1e-5 : e.zeroPosition - e.y) : m(e.y - e.zeroPosition <= 0 ? 1e-5 : e.zeroPosition - e.y);
		}
		let Ps = b(() => {
			let e = Math.max(1, Z.value);
			return {
				bar: At(Math.max(1, Y.value.width), e * Nc.value, 1),
				plot: hc.value,
				line: hc.value
			};
		});
		function Fs() {
			return H.value.useIndividualScale && H.value.isStacked ? Ps.value.line - Y.value.width / Z.value * .1 : Ps.value.bar;
		}
		function Is(e) {
			return H.value.useIndividualScale && H.value.isStacked ? e.x + Y.value.width / Z.value * .05 : e.x + Ps.value.bar / 2;
		}
		function Ls(e) {
			return H.value.useIndividualScale && H.value.isStacked ? e.x + Ps.value.line / 2 : Is(e) + Fs() / 2 - Rc.value / 2;
		}
		function Rs(e) {
			return e.value >= 0 ? e.y : [
				null,
				void 0,
				NaN,
				Infinity,
				-Infinity
			].includes(ks.value) ? Y?.bottom.value : ks.value;
		}
		function zs(e) {
			return e.value >= 0 ? e.y : [
				null,
				void 0,
				NaN,
				Infinity,
				-Infinity
			].includes(e.zeroPosition) ? 0 : e.zeroPosition;
		}
		let Bs = D(null), Vs = D([]), Hs = D(null), Us = b(() => {
			if (!K.value) return {
				sortedPoints: [],
				matchingPointsByXValue: /* @__PURE__ */ new Map()
			};
			let e = [], t = /* @__PURE__ */ new Map(), n = 0;
			for (let r = 0; r < q.value.length; r += 1) {
				let i = q.value[r];
				if (["line", "plot"].includes(i.type) && Array.isArray(i.series)) for (let r = 0; r < i.series.length; r += 1) {
					let o = i.series[r];
					if (!a(o?.x) || !a(o?.y)) continue;
					let s = Number(o.x), c = pl(o), l = {
						datapoint: i,
						point: o,
						index: r,
						x: c,
						y: o.y,
						distance: 0,
						sourceOrder: n
					};
					e.push({
						...l,
						xValue: s
					}), t.has(s) || t.set(s, []), t.get(s).push(l), n += 1;
				}
			}
			return e.sort((e, t) => e.x === t.x ? e.sourceOrder - t.sourceOrder : e.x - t.x), {
				sortedPoints: e,
				matchingPointsByXValue: t
			};
		});
		function Ws(e) {
			let t = Us.value.sortedPoints;
			if (!t.length) return null;
			let n = 0, r = t.length - 1;
			for (; n <= r;) {
				let i = Math.floor((n + r) / 2);
				t[i].x < e ? n = i + 1 : r = i - 1;
			}
			let i = /* @__PURE__ */ new Set();
			n >= 0 && n < t.length && i.add(n), n - 1 >= 0 && n - 1 < t.length && i.add(n - 1), n + 1 >= 0 && n + 1 < t.length && i.add(n + 1);
			let a = null;
			return i.forEach((n) => {
				let r = t[n], i = Math.abs(r.x - e);
				(!a || i < a.distance || i === a.distance && r.sourceOrder < a.sourceOrder) && (a = {
					...r,
					distance: i
				});
			}), a;
		}
		let Gs = b(() => K.value ? Array.from(Us.value.matchingPointsByXValue.keys()).sort((e, t) => pl({ x: e }) - pl({ x: t })) : []);
		function Ks(e) {
			let t = Gs.value[e];
			if (!a(t)) {
				Vs.value = [], Hs.value = null, ca.value = null, Ki.value = !1;
				return;
			}
			Eo(Number(t));
			let n = pl({ x: Number(t) }), r = Y.value.top + Y.value.height / 2, i = l(n, r, L.value);
			i && (ua.value = i), R.value = e, la.value = e, Ki.value = !0;
		}
		function qs(e) {
			let t = L.value;
			if (!t) return null;
			if (t.createSVGPoint && t.getScreenCTM) {
				let n = t.createSVGPoint();
				n.x = e.clientX, n.y = e.clientY;
				let r = t.getScreenCTM();
				if (r) {
					let e = n.matrixTransform(r.inverse());
					return {
						x: e.x,
						y: e.y,
						ok: !0
					};
				}
			}
			let n = t.getBoundingClientRect(), r = t.viewBox?.baseVal || {
				x: 0,
				y: 0,
				width: n.width,
				height: n.height
			}, i = Math.min(n.width / r.width, n.height / r.height), a = r.width * i, o = r.height * i, s = (n.width - a) / 2, c = (n.height - o) / 2;
			return {
				x: (e.clientX - n?.left - s) / i + r.x,
				y: (e.clientY - n?.top - c) / i + r.y,
				ok: !0
			};
		}
		let Js = 0;
		function Ys(e) {
			la.value = null, !Wi.value && (Js && cancelAnimationFrame(Js), Js = requestAnimationFrame(() => {
				Js = 0;
				let t = qs(e);
				if (!t || !L.value) {
					Xs();
					return;
				}
				let { left: n, right: r, top: i, bottom: a } = Y.value;
				if (t.x < n || t.x > r || t.y < i || t.y > a) {
					Xs();
					return;
				}
				if (K.value) {
					To(t.x), $l(!0, R.value ?? 0);
					return;
				}
				let o = xc(t.x);
				o == null ? Xs() : Bs.value !== o && (Bs.value = o, $l(!0, o));
			}));
		}
		function Xs() {
			Js &&= (cancelAnimationFrame(Js), 0), Bs.value = null, I.value = null, ca.value = null, $l(!1, null), Vs.value = [], Hs.value = null;
		}
		function Zs(e) {
			let t = qs(e);
			if (t && L.value) {
				let { left: e, right: n, top: r, bottom: i } = Y.value;
				if (t.x >= e && t.x <= n && t.y >= r && t.y <= i) {
					let e = xc(t.x);
					if (e != null) {
						Qs(e);
						return;
					}
				}
			}
			Bs.value != null && Qs(Bs.value);
		}
		function Qs(e) {
			let t = q.value.map((t) => ({
				name: t.name,
				value: [
					null,
					void 0,
					NaN
				].includes(t.absoluteValues[e]) ? null : t.absoluteValues[e],
				color: t.color,
				type: t.type
			}));
			vi("selectX", {
				dataset: t,
				index: e,
				indexLabel: V.value.chart.grid.labels.xAxisLabels.values[e]
			}), V.value.events.datapointClick && V.value.events.datapointClick({
				datapoint: t,
				seriesIndex: e + U.value.start
			});
		}
		function $s() {
			return So.value.map((e) => ({
				values: e.absoluteValues,
				color: e.color,
				name: e.name,
				type: e.type
			}));
		}
		async function ec({ scale: e = 2 } = {}) {
			if (!wi.value) return;
			let { imageUri: t, base64: n } = await Ge({
				domElement: wi.value,
				base64: !0,
				img: !0,
				scale: e
			}), r = wi.value.getBoundingClientRect(), i = {
				width: r.width,
				height: r.height,
				aspectRatio: r.height ? r.width / r.height : 0
			}, a = await me(t, e) ?? i;
			return {
				imageUri: t,
				base64: n,
				title: V.value.chart.title.text,
				...a
			};
		}
		function tc() {
			Ji.value.length ? Ji.value = [] : So.value.forEach((e) => {
				Ji.value.push(e.id);
			}), rc();
		}
		function nc(e, t) {
			(e.key === "Enter" || e.key === " ") && (e.preventDefault(), ic(t));
		}
		function rc() {
			vi("selectLegend", q.value.map((e) => ({
				name: e.name,
				values: e.absoluteValues,
				color: e.color,
				type: e.type
			})));
		}
		function ic(e) {
			if (ls.value = !1, ds &&= (cancelAnimationFrame(ds), null), Yi.value.has(e.id)) Ji.value = Ji.value.filter((t) => t !== e.id);
			else {
				if (Ji.value.length + 1 === xo.value.length) return;
				Ji.value.push(e.id);
			}
			rc(), ea.value += 1;
		}
		function ac(e) {
			return So.value.length ? So.value.find((t) => t.name === e) || (La.value && console.warn(`VueUiXy - Series name not found "${e}"`), null) : (La.value && console.warn("VueUiXy - There are no series to show."), null);
		}
		function oc(e) {
			let t = ac(e);
			t !== null && Yi.value.has(t.id) && ic({ id: t.id });
		}
		function sc(e) {
			let t = ac(e);
			t !== null && (Yi.value.has(t.id) || ic({ id: t.id }));
		}
		let cc = b(() => `${V.value.chart.title.text || "Chart visualization"}. ${V.value.chart.title.subtitle.text || ""}`), lc = b(() => ({ linePlot: Z.value > V.value.line.dot.hideAboveMaxSerieLength })), uc = b(() => V.value.chart.userOptions.show && (!V.value.chart.title.show || !V.value.chart.title.text)), dc = b(() => {
			if (Array.isArray(V.value.chart.highlightArea)) return V.value.chart.highlightArea.map((e) => {
				let t = Math.min(e.to, Ja.value - 1);
				return {
					...e,
					span: e.from === t ? 1 : t < e.from ? 0 : t - e.from + 1
				};
			});
			let e = {
				...V.value.chart.highlightArea,
				to: Math.min(V.value.chart.highlightArea.to, Ja.value - 1)
			};
			return [{
				...e,
				span: e.from === e.to ? 1 : e.to < e.from ? 0 : e.to - e.from + 1
			}];
		}), fc = b(() => zi.value ? Va.value.map((e, t) => ({
			...e,
			series: Wa(e.series),
			id: `uniqueId_${t}`,
			color: oe(e.color ? e.color : oo.value[t] ? oo.value[t] : ae[t])
		})) : Va.value), pc = b(() => q.value.map((e) => {
			let t = e.absoluteValues.map((e) => [void 0, null].includes(e) ? null : e);
			return {
				id: e.id,
				name: e.name,
				color: e.color,
				values: wt(Z.value, t)
			};
		})), mc = b(() => ({
			responsiveBreakpoint: V.value.table.responsiveBreakpoint,
			roundingValues: V.value.table.rounding,
			showAverage: !1,
			showMedian: !1,
			showTotal: !1,
			fontFamily: V.value.chart.fontFamily,
			prefix: V.value.chart.labels.prefix,
			suffix: V.value.chart.labels.suffix,
			colNames: X.value.map((e, t) => V.value.table.useDefaultTimeFormat ? e.text : Fl.value(t + U.value.start, V.value.table.timeFormat)),
			thead: {
				backgroundColor: V.value.table.th.backgroundColor,
				color: V.value.table.th.color,
				outline: V.value.table.th.outline
			},
			tbody: {
				backgroundColor: V.value.table.td.backgroundColor,
				color: V.value.table.td.color,
				outline: V.value.table.td.outline
			},
			userOptions: { show: !1 },
			sparkline: {
				animation: { show: !1 },
				cutNullValues: V.value.line.cutNullValues
			}
		})), hc = b(() => {
			let e = Math.max(0, Y.value?.width || 0);
			return V.value.chart.grid.position === "middle" ? At(e, Math.max(1, Z.value), 1) : At(e, Math.max(1, Z.value - 1), 0);
		}), gc = b(() => V.value.chart.grid.position === "middle" ? hc.value : Z.value <= 1 ? Math.max(0, Y.value?.width || 0) : hc.value);
		function _c(e) {
			let t = Y.value?.left || 0;
			return V.value.chart.grid.position === "middle" ? t + hc.value / 2 + hc.value * e : t + hc.value * e;
		}
		function vc(e) {
			let t = Y.value?.left || 0;
			return V.value.chart.grid.position === "middle" ? t + hc.value * e : Z.value <= 1 ? t : t + hc.value * e - hc.value / 2;
		}
		function yc(e) {
			return Math.max(Y.value?.left || 0, vc(e));
		}
		function bc(e) {
			let t = Y.value?.left || 0, n = Y.value?.right || 0, r = vc(e), i = r + gc.value;
			return Math.max(1e-5, Math.min(n, i) - Math.max(t, r));
		}
		function xc(e) {
			let t = Y.value?.left || 0, n = Y.value?.right || 0, r = Z.value;
			if (r <= 0 || e < t || e > n) return null;
			let i = hc.value;
			if (i <= 0) return 0;
			let a;
			return a = V.value.chart.grid.position === "middle" ? Math.ceil((e - t) / i) - 1 : r <= 1 ? 0 : Math.ceil((e - t) / i - .5), a < 0 ? 0 : a >= r ? r - 1 : a;
		}
		function Sc(e) {
			let t = Y.value?.left || 0, n = Y.value?.right || 0, r = vc(e), i = r + gc.value;
			return Math.max(1e-5, Math.min(n, i) - Math.max(t, r));
		}
		function Cc(e) {
			return Math.max(1e-5, gc.value * e);
		}
		let wc = b(() => ye(So.value.filter((e) => !Yi.value.has(e.id)))), Tc = [
			"bar",
			"line",
			"plot"
		], Ec = b(() => wc.value.filter((e) => Tc.includes(e.type))), Dc = b(() => Ec.value.length), Oc = b(() => Ec.value.filter((e) => e.type === "bar")), kc = b(() => Ec.value.filter((e) => e.type === "line")), Ac = b(() => Ec.value.filter((e) => e.type === "plot")), jc = b(() => xo.value.filter((e) => e.type === "bar" && !Yi.value.has(e.id))), Mc = b(() => jc.value.length), Nc = b(() => Math.max(1, Mc.value)), Pc = b(() => {
			let e = Tt(wc.value, (e) => e.scaleLabel), t = {};
			for (let [n, r] of Object.entries(e)) {
				let e = Infinity, i = -Infinity, a = !1;
				for (let t = 0; t < r.length; t += 1) {
					let n = r[t].absoluteValues || [];
					for (let t = 0; t < n.length; t += 1) {
						let r = n[t], o = r === null ? 0 : Number(r);
						if (Number.isNaN(o)) {
							e = NaN, i = NaN, a = !0;
							break;
						}
						a = !0, o < e && (e = o), o > i && (i = o);
					}
					if (Number.isNaN(e) || Number.isNaN(i)) break;
				}
				t[n] = {
					min: a ? e : 0,
					max: a ? i : 1,
					groupId: sl("scale_group", [n])
				};
			}
			return t;
		}), Fc = b(() => wc.value.reduce((e, t) => {
			let n = t.scaleLabel || "";
			return e[n] = (e[n] || 0) + 1, e;
		}, {}));
		function Ic({ datapoint: e, scaleYLabels: t, autoScaleYLabels: n, zeroPosition: r, autoScaleZeroPosition: i, individualMax: a, autoScaleMax: o, yOffset: s, individualHeight: c }) {
			let l = e.scaleLabel || "";
			return {
				...Pc.value[l] || {},
				name: e.name,
				groupName: l,
				groupColor: V.value.chart.grid.labels.yAxis.groupColor || e.color,
				color: e.color,
				scaleYLabels: e.autoScaling ? n : t,
				zeroPosition: e.autoScaling ? i : r,
				individualMax: e.autoScaling ? o : a,
				scaleLabel: l,
				id: e.id,
				yOffset: s,
				individualHeight: c,
				autoScaleYLabels: n,
				unique: Fc.value[l] === 1
			};
		}
		let Lc = b(() => Y.value.width / Z.value / Mc.value - Rc.value * Mc.value), Rc = b(() => Ps.value.line * V.value.bar.periodGap), zc = b(() => Math.max(1e-5, Fs() - (H.value.useIndividualScale && H.value.isStacked ? 0 : Rc.value))), Bc = b(() => zc.value * Math.min(Math.abs(V.value.bar.innerGap), .95)), Vc = b(() => {
			if (!V.value.chart.zoom.minimap.show) return [];
			let e = fc.value.filter((e) => !Yi.value.has(e.id)), t = 0;
			for (let n = 0; n < e.length; n += 1) {
				let r = e[n].series.length;
				r > t && (t = r);
			}
			let n = [];
			for (let r = 0; r < t; r += 1) {
				let t = 0;
				for (let n = 0; n < e.length; n += 1) t += e[n].series[r] || 0;
				n.push(t);
			}
			let r = al(n, 0, 1).min;
			return n.map((e) => e + (r < 0 ? Math.abs(r) : 0));
		});
		function Hc(e) {
			if ([null, void 0].includes(e)) return e;
			let t = Number(e);
			return Number.isFinite(t) ? -t : e;
		}
		let Uc = b(() => {
			if (!V.value.chart.zoom.minimap.show) return [];
			let e = K.value && Ds.value, t = K.value && Cs.value;
			return fc.value.map((n) => {
				let r = t ? {
					scaleMin: Hc(n.scaleMax),
					scaleMax: Hc(n.scaleMin)
				} : {};
				return {
					...n,
					...r,
					series: e || t ? n.series.map((n) => Et(n) ? {
						...n,
						x: e ? -n.x : n.x,
						y: t ? -n.y : n.y
					} : n) : n.series,
					isVisible: !Yi.value.has(n.id)
				};
			});
		}), Wc = b(() => !K.value || !Ds.value ? hl.value : -gl.value), Gc = b(() => !K.value || !Ds.value ? gl.value : -hl.value), Kc = b(() => !K.value || !Ds.value ? U.value.start : -Number(U.value.end)), qc = b(() => !K.value || !Ds.value ? U.value.end : -Number(U.value.start)), Jc = b(() => !K.value || !a(ca.value) ? ca.value : Ds.value ? -Number(ca.value) : Number(ca.value)), Yc = b(() => {
			let e = V.value.chart.grid.labels.yAxis.scaleMin, t = V.value.chart.grid.labels.yAxis.scaleMax;
			return !K.value || !Cs.value ? e : Hc(t);
		}), Xc = b(() => {
			let e = V.value.chart.grid.labels.yAxis.scaleMin, t = V.value.chart.grid.labels.yAxis.scaleMax;
			return !K.value || !Cs.value ? t : Hc(e);
		}), Zc = b(() => X.value.length ? K.value && Ds.value ? X.value.at(-1)?.text || "" : X.value[0]?.text || "" : ""), Qc = b(() => X.value.length ? K.value && Ds.value ? X.value[0]?.text || "" : X.value.at(-1)?.text || "" : ""), $c = b(() => K.value ? Vs.value.map((e) => ({
			slotAbsoluteIndex: e.datapoint.slotAbsoluteIndex,
			shape: e.datapoint.shape || e.datapoint.type === "bar" ? "square" : "circle",
			name: e.datapoint.name,
			color: e.datapoint.color,
			type: e.datapoint.type,
			value: e.point.raw?.y ?? e.point.y,
			x: e.point.raw?.x ?? e.point.x,
			comments: e.datapoint.comments || [],
			prefix: e.datapoint.prefix || V.value.chart.labels.prefix,
			suffix: e.datapoint.suffix || V.value.chart.labels.suffix
		})) : q.value.map((e) => ({
			slotAbsoluteIndex: e.slotAbsoluteIndex,
			shape: e.shape || e.type === "bar" ? "square" : "circle",
			name: e.name,
			color: e.color,
			type: e.type,
			value: e.absoluteValues.find((e, t) => t === R.value),
			comments: e.comments || [],
			prefix: e.prefix || V.value.chart.labels.prefix,
			suffix: e.suffix || V.value.chart.labels.suffix
		}))), el = b(() => G.value.ticks.map((e) => {
			let t = e >= 0 ? ks.value - Y.value.height * Ss(e) : ks.value + Y.value.height * Ss(Math.abs(e));
			return {
				y: Cs.value ? Y.value.top + Y.value.bottom - t : t,
				value: e,
				prefix: V.value.chart.labels.prefix,
				suffix: V.value.chart.labels.suffix
			};
		})), tl = b(() => {
			let e = V.value.chart.annotations;
			if (!e || !Array.isArray(e) || e.every((e) => !e.show)) return [];
			let t = e.filter((e) => e.show && (e.yAxis.yTop != null || e.yAxis.yBottom != null));
			if (!t.length) return [];
			let { left: n, right: r } = Y.value, i = ks.value, a = Y.value.height, o = G.value.min, s = G.value.max - o, c = (e) => {
				let t = (e - 0) / s;
				return i - t * a;
			};
			return t.map((e, t) => {
				let { yAxis: { yTop: i, yBottom: a, label: o } } = e, s = i != null && a != null && i !== a, l = i == null ? null : c(i), u = a == null ? null : c(a), d = Io(o.fontSize);
				d.font = `${o.fontSize}px sans-serif`;
				let ee = d.measureText(o.text).width, te = o.fontSize, ne = (o.position === "start" ? n + o.padding?.left : r - o.padding?.right) + o.offsetX, f = (l != null && u != null ? Math.min(l, u) : l ?? u) - o.fontSize / 3 + o.offsetY - o.padding?.top, p;
				p = o.textAnchor === "middle" ? ne - ee / 2 - o.padding?.left : o.textAnchor === "end" ? ne - ee - o.padding?.right : ne - o.padding?.left;
				let re = f - te * .75 - o.padding?.top;
				return {
					show: ![
						l,
						u,
						re
					].includes(NaN),
					id: sl("annotation_y", [
						t,
						i,
						a,
						o.text
					]),
					hasArea: s,
					areaHeight: s ? Math.abs(l - u) : 0,
					yTop: l,
					yBottom: u,
					config: e.yAxis,
					x1: n,
					x2: r,
					_text: {
						x: ne,
						y: f
					},
					_box: {
						x: p,
						y: re,
						width: ee + o.padding?.left + o.padding?.right,
						height: te + o.padding?.top + o.padding?.bottom,
						fill: o.backgroundColor,
						stroke: o.border.stroke,
						rx: o.border.rx,
						ry: o.border.ry,
						strokeWidth: o.border.strokeWidth
					}
				};
			});
		});
		function nl(e, t) {
			let n = e[t - 1], r = e[t + 1], i = !!n && !!r && n.value == null && r.value == null || !n && !!r && r.value == null || !!n && !r && n.value == null;
			return Ct(e[t].value) && i && V.value.line.cutNullValues;
		}
		function rl(e, t = 0, n = 1) {
			let r = Infinity, i = -Infinity, a = !1;
			for (let t = 0; t < e.length; t += 1) {
				let n = e[t];
				if (Array.isArray(n.series)) for (let e = 0; e < n.series.length; e += 1) {
					let t = bo(n.series[e]);
					Number.isFinite(t) && (a = !0, t < r && (r = t), t > i && (i = t));
				}
			}
			return a ? {
				min: r,
				max: i
			} : {
				min: t,
				max: n
			};
		}
		function il(e, t = 0, n = 1) {
			let r = Infinity, i = -Infinity, a = !1;
			for (let t = 0; t < e.length; t += 1) {
				let n = e[t];
				if (Array.isArray(n.series)) for (let e = 0; e < n.series.length; e += 1) {
					let t = n.series[e];
					if (!Et(t)) continue;
					let o = Number(t.x);
					Number.isFinite(o) && (a = !0, o < r && (r = o), o > i && (i = o));
				}
			}
			return a ? {
				min: r,
				max: r === i ? i + 1 : i
			} : {
				min: t,
				max: n
			};
		}
		function al(e, t = 0, n = 1) {
			if (!Array.isArray(e) || e.length === 0) return {
				min: t,
				max: n
			};
			let r = Infinity, i = -Infinity;
			for (let t = 0; t < e.length; t += 1) {
				let n = e[t], a = n === null ? 0 : Number(n);
				if (Number.isNaN(a)) return {
					min: NaN,
					max: NaN
				};
				a < r && (r = a), a > i && (i = a);
			}
			return {
				min: r,
				max: i
			};
		}
		function ol(e) {
			let t = String(e ?? ""), n = 0;
			for (let e = 0; e < t.length; e += 1) n = (n << 5) - n + t.charCodeAt(e), n |= 0;
			return Math.abs(n).toString(36);
		}
		function sl(e, t) {
			return `${e}_${ol(t.join("|"))}`;
		}
		function cl() {
			return Ba.value || ls.value || !V.value.line.showTransition ? void 0 : `all ${V.value.line.transitionDurationMs}ms ease-in-out`;
		}
		let ll = b(() => il(xo.value, 0, 1)), ul = b(() => {
			let e = V.value.chart.grid.labels.xAxis.commonScaleSteps, t = V.value.chart.grid.labels.xAxis.scaleMin, n = V.value.chart.grid.labels.xAxis.scaleMax, r = t !== null && Number.isFinite(Number(t)) ? Number(t) : ll.value.min, i = n !== null && Number.isFinite(Number(n)) ? Number(n) : ll.value.max, a = V.value.chart.grid.labels.xAxis.useNiceScale ? pe(r, i, e) : we(r, i, e);
			if (V.value.chart.grid.position !== "start") return a;
			let o = a.ticks.length > 1 ? a.ticks[1] - a.ticks[0] : 1;
			return {
				...a,
				max: a.max + o
			};
		}), dl = b(() => K.value ? (Ds.value ? [...ul.value.ticks].reverse() : [...ul.value.ticks]).map((e, t) => ({
			id: `continuous_x_label_${t}`,
			text: _(V.value.chart.grid.labels.xAxis.formatter, e, fe({
				v: e,
				s: V.value.chart.labels.prefix,
				p: V.value.chart.labels.suffix,
				r: V.value.chart.grid.labels.xAxis.rounding
			}), {
				datapoint: e,
				seriesIndex: t
			}),
			value: e,
			x: pl({ x: e }),
			index: t
		})) : Jo.value);
		function fl(e, t) {
			return K.value ? e.x : _c(t);
		}
		function pl({ x: e }) {
			let t = ul.value.max - ul.value.min || 1, n = Os((e - ul.value.min) / t);
			return Y.value.left + Y.value.width * n;
		}
		let ml = b(() => il(Va.value, 0, 1)), hl = b(() => K.value ? ml.value.min : 0), gl = b(() => K.value ? ml.value.max : Ja.value);
		function _l(e, t) {
			return K.value ? e.x == null || e.y == null ? null : pl(e) : _c(t);
		}
		function vl(e) {
			return !K.value || !e ? null : e.x ?? null;
		}
		function yl({ datapoint: e, totalSeries: t, gap: n, usableHeight: r, autoScaleValueMin: i, autoScaleValueMax: a, individualExtremes: o, forceExactScale: s = !1 }) {
			let c = e.scaleSteps || V.value.chart.grid.labels.yAxis.commonScaleSteps, l = 1.0000001, u = s || !V.value.chart.grid.labels.yAxis.useNiceScale ? we : pe, d = o.max === o.min ? o.max * l : o.max, ee = a === i ? a * l : a, te = u(o.min, d, c), ne = u(i, ee, c), f = te.min >= 0 ? 0 : Math.abs(te.min), p = te.max + Math.abs(f), re = ne.max + 0, ie = e.stackIndex, ae = t - 1 - ie, oe = H.value.isStacked ? 1 - e.cumulatedStackRatio : 0, se = H.value.isStacked ? r * oe + n * ae : 0, ce = H.value.isStacked ? r * e.stackRatio : Y.value.height;
			return {
				scaleSteps: c,
				individualScale: te,
				autoScaleSteps: ne,
				individualZero: f,
				autoScaleZero: 0,
				individualMax: p,
				autoScaleMax: re,
				yOffset: se,
				individualHeight: ce,
				zeroPosition: Y.value?.bottom - se - ce * f / p,
				autoScaleZeroPosition: Y.value?.bottom - se - ce * 0 / re
			};
		}
		function bl({ datapoint: e, clampNegativeAutoScaleMaximum: t = !1 }) {
			let n = Pc.value[e.scaleLabel], r = n.min, i = n.max;
			return {
				ratios: e.absoluteValues.filter((e) => ![null, void 0].includes(e)).map((e) => (e - r) / (i - r)),
				valueMin: r,
				valueMax: t && i < 0 ? 0 : i
			};
		}
		function xl({ datapoint: e, filterMinimumValues: t = !1, preserveLooseMinimumExpression: n = !1 }) {
			let r = al(e.absoluteValues, 0, 1), i = al(t ? e.absoluteValues.filter((e) => ![null, void 0].includes(e)) : e.absoluteValues, 0, 1).min;
			return {
				max: e.scaleMax || r.max || 1,
				min: n ? e.scaleMin || i > 0 ? 0 : i : e.scaleMin || (i > 0 ? 0 : i)
			};
		}
		function Sl({ values: e, autoScaleSteps: t }) {
			return e.map((e) => t.min >= 0 ? (e - Math.abs(t.min)) / (t.max - Math.abs(t.min)) : (e + Math.abs(t.min)) / (t.max + Math.abs(t.min)));
		}
		function Cl({ datapoint: e, individualScale: t, yOffset: n, individualHeight: r }) {
			return t.ticks.map((i) => ({
				y: Es({
					value: i,
					scaleMin: t.min,
					scaleMax: t.max,
					yOffset: n,
					individualHeight: r
				}),
				value: i,
				prefix: e.prefix || V.value.chart.labels.prefix,
				suffix: e.suffix || V.value.chart.labels.suffix,
				datapoint: e
			}));
		}
		function wl({ datapoint: e, autoScaleSteps: t, yOffset: n, individualHeight: r }) {
			return t.ticks.map((i) => ({
				y: Ts({
					ratio: (i - t.min) / (t.max - t.min || 1),
					yOffset: n,
					individualHeight: r
				}),
				value: i,
				prefix: e.prefix || V.value.chart.labels.prefix,
				suffix: e.suffix || V.value.chart.labels.suffix,
				datapoint: e
			}));
		}
		function Tl({ datapoint: e, totalSeries: t, gap: n, usableHeight: r, forceExactScale: i = !1, clampNegativeAutoScaleMaximum: a = !1, filterMinimumValues: o = !1, preserveLooseMinimumExpression: s = !1 }) {
			ts(e);
			let c = bl({
				datapoint: e,
				clampNegativeAutoScaleMaximum: a
			}), l = xl({
				datapoint: e,
				filterMinimumValues: o,
				preserveLooseMinimumExpression: s
			}), u = yl({
				datapoint: e,
				totalSeries: t,
				gap: n,
				usableHeight: r,
				autoScaleValueMin: c.valueMin,
				autoScaleValueMax: c.valueMax,
				individualExtremes: l,
				forceExactScale: i
			});
			return {
				autoScale: c,
				individualExtremes: l,
				scaleYLabels: Cl({
					datapoint: e,
					individualScale: u.individualScale,
					yOffset: u.yOffset,
					individualHeight: u.individualHeight
				}),
				autoScaleYLabels: wl({
					datapoint: e,
					autoScaleSteps: u.autoScaleSteps,
					yOffset: u.yOffset,
					individualHeight: u.individualHeight
				}),
				autoScaleRatiosToNiceScale: Sl({
					values: e.absoluteValues,
					autoScaleSteps: u.autoScaleSteps
				}),
				...u
			};
		}
		function El(e) {
			return Array.isArray(e) ? e.slice(U.value.start, U.value.end) : [];
		}
		function Dl(e, t) {
			return Array.isArray(e) && e[t + U.value.start] || "";
		}
		let Ol = b(() => {
			let e = Dc.value, t = V.value.chart.grid.labels.yAxis.gap, n = H.value.isStacked ? t * (e - 1) : 0, r = Y.value.height - n;
			return Oc.value.map((n, i) => {
				let { individualScale: a, autoScaleSteps: o, individualZero: s, individualMax: c, autoScaleMax: l, yOffset: u, individualHeight: d, zeroPosition: ee, autoScaleZeroPosition: te, scaleYLabels: ne, autoScaleYLabels: f, autoScaleRatiosToNiceScale: p } = Tl({
					datapoint: n,
					totalSeries: e,
					gap: t,
					usableHeight: r,
					clampNegativeAutoScaleMaximum: !0,
					filterMinimumValues: !0,
					preserveLooseMinimumExpression: !0
				}), re = Mc.value, ie = El(n.comments), ae = n.series.map((e, t) => {
					let r = H.value.useIndividualScale ? (n.absoluteValues[t] + s) / c : Ss(e), a = H.value.useIndividualScale && H.value.isStacked ? Y.value?.left + Y.value.width / Z.value * t : Y.value?.left + Ps.value.bar * i + Ps.value.bar * t * re - Lc.value / 2 - i * Rc.value;
					return {
						yOffset: m(u),
						individualHeight: m(d),
						x: m(a),
						y: Ts({
							ratio: r,
							yOffset: u,
							individualHeight: d
						}),
						value: n.absoluteValues[t],
						zeroPosition: m(ee),
						individualMax: m(c),
						comment: ie[t] || ""
					};
				}), oe = n.series.map((e, t) => {
					let r = H.value.useIndividualScale && H.value.isStacked ? Y.value?.left + Y.value.width / Z.value * t : Y.value?.left - Ps.value.bar / 2 + Ps.value.bar * i + Ps.value.bar * t * So.value.filter((e) => e.type === "bar").filter((e) => !Yi.value.has(e.id)).length;
					return {
						yOffset: m(u),
						individualHeight: m(d),
						x: m(r),
						y: m(Ts({
							ratio: p[t] || 0,
							yOffset: m(u),
							individualHeight: m(d)
						})),
						value: n.absoluteValues[t],
						zeroPosition: m(ee),
						individualMax: m(c),
						comment: ie[t] || ""
					};
				}), se = Ic({
					datapoint: n,
					scaleYLabels: ne,
					autoScaleYLabels: f,
					zeroPosition: ee,
					autoScaleZeroPosition: te,
					individualMax: c,
					autoScaleMax: l,
					yOffset: u,
					individualHeight: d
				});
				return {
					...n,
					yOffset: u,
					autoScaleYLabels: f,
					individualHeight: d,
					scaleYLabels: n.autoScaling ? f : ne,
					individualScale: n.autoScaling ? o : a,
					individualMax: n.autoScaling ? l : c,
					zeroPosition: n.autoScaling ? te : ee,
					plots: n.autoScaling ? oe : ae,
					scaleGroup: se,
					groupId: se.groupId
				};
			});
		}), Q = b(() => {
			let e = Dc.value, t = V.value.chart.grid.labels.yAxis.gap, i = H.value.isStacked ? t * (e - 1) : 0, a = Y.value.height - i;
			return kc.value.map((i, o) => {
				let { individualScale: s, autoScaleSteps: c, individualZero: l, individualMax: ee, autoScaleMax: te, yOffset: f, individualHeight: p, zeroPosition: ae, autoScaleZeroPosition: se, scaleYLabels: ue, autoScaleYLabels: de, autoScaleRatiosToNiceScale: fe } = Tl({
					datapoint: i,
					totalSeries: e,
					gap: t,
					usableHeight: a
				}), pe = El(i.comments), h = i.series.map((e, t) => {
					if (K.value && (e.x == null || e.y == null)) return {
						index: t,
						x: null,
						y: null,
						value: null,
						datasetXValue: vl(e),
						comment: i.comments && i.comments.slice(U.value.start, U.value.end)[t] || ""
					};
					let n = H.value.useIndividualScale ? (i.absoluteValues[t] + Math.abs(l)) / ee : Ss(bo(e));
					return {
						index: t,
						x: m(_l(e, t)),
						datasetXValue: vl(e),
						y: m(Ts({
							ratio: n,
							yOffset: f,
							individualHeight: p
						})),
						value: i.absoluteValues[t],
						comment: pe[t] || ""
					};
				}), g = i.series.map((e, t) => K.value && (e.x === null || e.y === null) ? {
					index: t,
					x: null,
					y: null,
					datasetXValue: vl(e),
					value: null,
					comment: pe[t] || ""
				} : [void 0, null].includes(i.absoluteValues[t]) ? {
					index: t,
					x: m(_l(e, t)),
					y: ae,
					value: i.absoluteValues[t],
					comment: pe[t] || ""
				} : {
					index: t,
					x: m(_l(e, t)),
					datasetXValue: vl(e),
					y: m(Ts({
						ratio: fe[t] || 0,
						yOffset: f,
						individualHeight: p
					})),
					value: i.absoluteValues[t],
					comment: pe[t] || ""
				}), me = i.dashIndices && Array.isArray(i.dashIndices) && i?.dashIndices?.length > 0, _ = V.value.line.cutNullValues ? d(h) : ie(h.filter((e) => e.value !== null)), he = V.value.line.cutNullValues ? d(g) : ie(g.filter((e) => e.value !== null)), _e = V.value.line.cutNullValues ? u(h) : le(h.filter((e) => e.value !== null)), ve = V.value.line.cutNullValues ? u(g) : le(g.filter((e) => e.value !== null)), ye = r(V.value.line.cutNullValues ? h : h.filter((e) => e.value !== null)), v = r(V.value.line.cutNullValues ? g : g.filter((e) => e.value !== null)), be = me ? ce(V.value.line.cutNullValues ? h : h.filter((e) => e.value !== null), i.dashIndices.map((e) => e - U.value.start)) : [], xe = me ? ne(V.value.line.cutNullValues ? h : h.filter((e) => e.value !== null), i.dashIndices.map((e) => e - U.value.start)) : [], Se = Ic({
					datapoint: i,
					scaleYLabels: ue,
					autoScaleYLabels: de,
					zeroPosition: ae,
					autoScaleZeroPosition: se,
					individualMax: ee,
					autoScaleMax: te,
					yOffset: f,
					individualHeight: p
				}), Ce = H.value.useIndividualScale ? i.autoScaling ? se : ae : co.value, we = Math.max(Math.max(i.autoScaling ? se : ue.at(-1) ? ue.at(-1).y : 0, Y.value?.top), Ce), Te = i.autoScaling ? g : h, Ee = V.value.line.cutNullValues ? Te : Te.filter((e) => e.value !== null), De = i.absoluteValues.filter((e) => ![
					null,
					void 0,
					NaN
				].includes(e)), Oe = !!i.temperatureColors && new Set(De).size <= 1;
				return {
					...i,
					isFlatTemperatureLine: Oe,
					temperatureColors: i.temperatureColors ? i.temperatureColors.map((e) => oe(e)) : null,
					yOffset: f,
					autoScaleYLabels: de,
					individualHeight: p,
					scaleYLabels: i.autoScaling ? de : ue,
					individualScale: i.autoScaling ? c : s,
					individualMax: i.autoScaling ? te : ee,
					zeroPosition: i.autoScaling ? se : ae,
					curve: i.useStepper ? i.autoScaling ? v : ye : i.autoScaling ? he : _,
					plots: i.autoScaling ? g : h,
					dashedStraight: be,
					dashedSmooth: xe,
					hasDashedSegments: me,
					area: i.useArea ? i.useStepper ? r(Ee, we) : H.value.useIndividualScale ? V.value.line.cutNullValues ? n(i.autoScaling ? g : h, we) : ge(i.autoScaling ? g.filter((e) => e.value !== null) : h.filter((e) => e.value !== null), we) : V.value.line.cutNullValues ? n(h, we) : ge(h.filter((e) => e.value !== null), we) : "",
					curveAreas: i.useArea ? i.useStepper ? r(Ee, we).split(";").filter(Boolean).map((e) => `M${e}Z`) : re(i.autoScaling ? V.value.line.cutNullValues ? g : g.filter((e) => e.value !== null) : V.value.line.cutNullValues ? h : h.filter((e) => e.value !== null), we, V.value.line.cutNullValues) : [],
					straight: i.useStepper ? i.autoScaling ? v : ye : i.autoScaling ? ve : _e,
					scaleGroup: Se,
					groupId: Se.groupId
				};
			});
		}), $ = b(() => {
			let e = Dc.value, t = V.value.chart.grid.labels.yAxis.gap, n = H.value.isStacked ? t * (e - 1) : 0, r = Y.value.height - n;
			return Ac.value.map((n) => {
				let { individualScale: i, autoScaleSteps: a, individualZero: o, individualMax: s, autoScaleMax: c, yOffset: l, individualHeight: u, zeroPosition: d, autoScaleZeroPosition: ee, scaleYLabels: te, autoScaleYLabels: ne, autoScaleRatiosToNiceScale: f } = Tl({
					datapoint: n,
					totalSeries: e,
					gap: t,
					usableHeight: r,
					forceExactScale: !0,
					preserveLooseMinimumExpression: !0
				}), p = El(n.comments), re = n.series.map((e, t) => {
					if (K.value && (e.x == null || e.y == null)) return {
						index: t,
						x: null,
						datasetXValue: vl(e),
						y: null,
						value: null,
						comment: p[t] || ""
					};
					let r = H.value.useIndividualScale ? (n.absoluteValues[t] + Math.abs(o)) / s : Ss(bo(e));
					return {
						index: t,
						x: m(_l(e, t)),
						datasetXValue: vl(e),
						y: m(Ts({
							ratio: r,
							yOffset: l,
							individualHeight: u
						})),
						value: n.absoluteValues[t],
						comment: p[t] || ""
					};
				}), ie = n.series.map((e, t) => K.value && (e.x === null || e.y === null) ? {
					index: t,
					x: null,
					datasetXValue: vl(e),
					y: null,
					value: null,
					comment: p[t] || ""
				} : {
					index: t,
					x: m(_l(e, t)),
					datasetXValue: vl(e),
					y: m(Ts({
						ratio: f[t] || 0,
						yOffset: l,
						individualHeight: u
					})),
					value: n.absoluteValues[t],
					comment: p[t] || ""
				}), ae = Ic({
					datapoint: n,
					scaleYLabels: te,
					autoScaleYLabels: ne,
					zeroPosition: d,
					autoScaleZeroPosition: ee,
					individualMax: s,
					autoScaleMax: c,
					yOffset: l,
					individualHeight: u
				});
				return {
					...n,
					yOffset: l,
					autoScaleYLabels: ne,
					individualHeight: u,
					scaleYLabels: n.autoScaling ? ne : te,
					individualScale: n.autoScaling ? a : i,
					individualMax: n.autoScaling ? c : s,
					zeroPosition: n.autoScaling ? ee : d,
					plots: n.autoScaling ? ie : re,
					scaleGroup: ae,
					groupId: ae.groupId
				};
			});
		}), kl = b(() => {
			let e = Object.entries(Pc.value).reduce((e, [t, n]) => (e[t] = {
				...n,
				scaleLabel: t
			}, e), {});
			return [
				...Q.value,
				...Ol.value,
				...$.value
			].forEach((t) => {
				t.scaleGroup && (e[t.scaleLabel || ""] = {
					...e[t.scaleLabel || ""],
					...t.scaleGroup
				});
			}), e;
		}), Al = b(() => {
			let e = Q.value.map((e) => ({
				name: e.name,
				color: e.color,
				scale: e.individualScale,
				scaleYLabels: e.scaleYLabels,
				zero: e.zeroPosition,
				max: e.individualMax,
				scaleLabel: e.scaleLabel || "",
				id: e.id,
				yOffset: e.yOffset || 0,
				individualHeight: e.individualHeight || Y.value.height,
				autoScaleYLabels: e.autoScaleYLabels
			})), t = Ol.value.map((e) => ({
				name: e.name,
				color: e.color,
				scale: e.individualScale,
				scaleYLabels: e.scaleYLabels,
				zero: e.zeroPosition,
				max: e.individualMax,
				scaleLabel: e.scaleLabel || "",
				id: e.id,
				yOffset: e.yOffset || 0,
				individualHeight: e.individualHeight || Y.value.height
			})), n = $.value.map((e) => ({
				name: e.name,
				color: e.color,
				scale: e.individualScale,
				scaleYLabels: e.scaleYLabels,
				zero: e.zeroPosition,
				max: e.individualMax,
				scaleLabel: e.scaleLabel || "",
				id: e.id,
				yOffset: e.yOffset || 0,
				individualHeight: e.individualHeight || Y.value.height
			})), r = H.value.useIndividualScale && !H.value.isStacked ? Object.values(kl.value) : [
				...e,
				...t,
				...n
			], i = r.flatMap((e) => e).length;
			return r.flatMap((e, t) => {
				let n = 0;
				n = H.value.isStacked ? J.value ? Y.value?.right : Y.value?.left : J.value ? Y.value?.right + Mo() * t : Y.value?.left / i * (t + 1);
				let r = H.value.useIndividualScale && !H.value.isStacked ? e.unique ? e.name : e.groupName : e.name;
				return {
					unique: e.unique,
					id: e.id,
					groupId: e.groupId,
					scaleLabel: e.scaleLabel,
					name: _(V.value.chart.grid.labels.yAxis.serieNameFormatter, r, r, e),
					color: H.value.useIndividualScale && !H.value.isStacked ? e.unique ? e.color : e.groupColor : e.color,
					scale: e.scale,
					yOffset: e.yOffset,
					individualHeight: e.individualHeight,
					x: n,
					yLabels: e.scaleYLabels || e.scale.ticks.map((t) => ({
						y: Es({
							value: t,
							scaleMin: e.scale.min,
							scaleMax: e.scale.max,
							yOffset: e.yOffset || 0,
							individualHeight: e.individualHeight
						}),
						value: t
					}))
				};
			});
		}), jl = b(() => {
			let e = V.value.line.interLine || {}, t = e.pairs || [], n = e.colors || [];
			if (!t.length) return [];
			let r = [];
			return t.forEach((e, t) => {
				let [i, a] = Array.isArray(e) ? e : [e.a, e.b];
				if (!i || !a) return;
				let o = Q.value.find((e) => e.name === i), s = Q.value.find((e) => e.name === a);
				if (!o || !s || o.type !== "line" || s.type !== "line") return;
				let c = n?.[t]?.[0] ?? o.color, l = n?.[t]?.[1] ?? s.color;
				_e({
					lineA: o.plots,
					lineB: s.plots,
					smoothA: !!o.smooth,
					smoothB: !!s.smooth,
					stepperA: !!o.useStepper,
					stepperB: !!s.useStepper,
					colorLineA: c,
					colorLineB: l,
					sampleStepPx: 2,
					cutNullValues: V.value.line.cutNullValues
				}).forEach((e, n) => {
					r.push({
						...e,
						key: `inter_${i}_${a}_${t}_${n}`
					});
				});
			}), r;
		}), Ml = b(() => {
			let e = Vs.value[0]?.index ?? null, t = K.value ? e : R.value;
			return {
				timeLabel: K.value ? {
					text: String(Vs.value[0]?.point.raw?.x ?? Vs.value[0]?.point.x),
					absoluteIndex: e
				} : X.value[R.value],
				datapoint: $c.value,
				seriesIndex: R.value,
				selectedIndex: t,
				absoluteIndex: K.value ? e : R.value + U.value.start,
				series: So.value,
				bars: Ol.value,
				lines: Q.value,
				plots: $.value,
				config: V.value
			};
		}), Nl = D({
			months: [],
			shortMonths: [],
			days: [],
			shortDays: []
		}), Pl = 0;
		xt(() => {
			let e = ++Pl, t = V.value.chart.grid.labels.xAxisLabels.datetimeFormatter;
			(async () => {
				let n = await Ne(t.locale).catch(() => Ne("en"));
				e === Pl && (Nl.value = n.data);
			})();
		});
		let Fl = b(() => {
			let e = V.value.chart.grid.labels.xAxisLabels.datetimeFormatter, t = Me({
				useUTC: e.useUTC,
				locale: Nl.value,
				januaryAsYear: e.januaryAsYear
			});
			return (e, n) => {
				let r = V.value.chart.grid.labels.xAxisLabels.values?.[e];
				return r == null ? "" : t.formatDate(new Date(r), n);
			};
		}), Il = b(() => (V.value.chart.grid.labels.xAxisLabels.values || []).map((e, t) => ({
			text: Fl.value(t, V.value.chart.zoom.timeFormat),
			absoluteIndex: t
		}))), Ll = /* @__PURE__ */ new Map();
		function Rl(e) {
			return {
				absoluteIndex: R.value + U.value.start,
				seriesIndex: R.value,
				datapoint: $c.value,
				series: So.value,
				bars: Ol.value,
				lines: Q.value,
				plots: $.value,
				config: V.value,
				dateLabel: e
			};
		}
		function zl(e) {
			return [
				Ui.value[e.type],
				e.shape || "",
				e.color,
				V.value.chart.tooltip.backgroundColor,
				!!yi.pattern,
				F.value,
				e.slotAbsoluteIndex
			].join("|");
		}
		function Bl(e) {
			let t = zl(e);
			if (Ll.has(t)) return Ll.get(t);
			let n = V.value.chart.tooltip.backgroundColor, r = "", i = "";
			switch (Ui.value[e.type]) {
				case "bar":
					r = `<svg viewBox="0 0 40 40" height="14" width="14">${yi.pattern ? `<rect x="0" y="0" rx="1" stroke="none" height="40" width="40" fill="${e.color}" />` : ""}<rect x="0" y="0" rx="1" stroke="none" height="40" width="40" fill="${yi.pattern ? `url(#pattern_${F.value}_${e.slotAbsoluteIndex})` : e.color}" /></svg>`;
					break;
				case "line":
					!e.shape || ![
						"star",
						"triangle",
						"square",
						"diamond",
						"pentagon",
						"hexagon"
					].includes(e.shape) ? i = `<circle cx="10" cy="8" r="4" stroke="${n}" stroke-width="0.5" fill="${e.color}" />` : e.shape === "triangle" ? i = `<path d="${p({
						plot: {
							x: 10,
							y: 8
						},
						radius: 4,
						sides: 3,
						rotation: .52
					}).path}" fill="${e.color}" stroke="${n}" stroke-width="0.5" />` : e.shape === "square" ? i = `<path d="${p({
						plot: {
							x: 10,
							y: 8
						},
						radius: 4,
						sides: 4,
						rotation: .8
					}).path}" fill="${e.color}" stroke="${n}" stroke-width="0.5" />` : e.shape === "diamond" ? i = `<path d="${p({
						plot: {
							x: 10,
							y: 8
						},
						radius: 4,
						sides: 4,
						rotation: 0
					}).path}" fill="${e.color}" stroke="${n}" stroke-width="0.5" />` : e.shape === "pentagon" ? i = `<path d="${p({
						plot: {
							x: 10,
							y: 8
						},
						radius: 4,
						sides: 5,
						rotation: .95
					}).path}" fill="${e.color}" stroke="${n}" stroke-width="0.5" />` : e.shape === "hexagon" ? i = `<path d="${p({
						plot: {
							x: 10,
							y: 8
						},
						radius: 4,
						sides: 6,
						rotation: 0
					}).path}" fill="${e.color}" stroke="${n}" stroke-width="0.5" />` : e.shape === "star" && (i = `<polygon stroke="${n}" stroke-width="0.5" fill="${e.color}" points="${Oe({
						plot: {
							x: 10,
							y: 8
						},
						radius: 4
					})}" />`), r = `<svg viewBox="0 0 20 12" height="14" width="20"><rect rx="1.5" x="0" y="6.5" stroke="${n}" stroke-width="0.5" height="3" width="20" fill="${e.color}" />${i}</svg>`;
					break;
				case "plot":
					if (!e.shape || ![
						"star",
						"triangle",
						"square",
						"diamond",
						"pentagon",
						"hexagon"
					].includes(e.shape)) {
						r = `<svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="${n}" stroke-width="1" fill="${e.color}" /></svg>`;
						break;
					}
					if (e.shape === "star") {
						r = `<svg viewBox="0 0 12 12" height="14" width="14"><polygon stroke="${n}" stroke-width="1" fill="${e.color}" points="${Oe({
							plot: {
								x: 6,
								y: 6
							},
							radius: 5
						})}" /></svg>`;
						break;
					}
					if (e.shape === "triangle") {
						r = `<svg viewBox="0 0 12 12" height="14" width="14"><path d="${p({
							plot: {
								x: 6,
								y: 6
							},
							radius: 6,
							sides: 3,
							rotation: .52
						}).path}" fill="${e.color}" stroke="${n}" stroke-width="1" /></svg>`;
						break;
					}
					if (e.shape === "square") {
						r = `<svg viewBox="0 0 12 12" height="14" width="14"><path d="${p({
							plot: {
								x: 6,
								y: 6
							},
							radius: 6,
							sides: 4,
							rotation: .8
						}).path}" fill="${e.color}" stroke="${n}" stroke-width="1" /></svg>`;
						break;
					}
					if (e.shape === "diamond") {
						r = `<svg viewBox="0 0 12 12" height="14" width="14"><path d="${p({
							plot: {
								x: 6,
								y: 6
							},
							radius: 5,
							sides: 4,
							rotation: 0
						}).path}" fill="${e.color}" stroke="${n}" stroke-width="1" /></svg>`;
						break;
					}
					if (e.shape === "pentagon") {
						r = `<svg viewBox="0 0 12 12" height="14" width="14"><path d="${p({
							plot: {
								x: 6,
								y: 6
							},
							radius: 5,
							sides: 5,
							rotation: .95
						}).path}" fill="${e.color}" stroke="${n}" stroke-width="1" /></svg>`;
						break;
					}
					if (e.shape === "hexagon") {
						r = `<svg viewBox="0 0 12 12" height="14" width="14"><path d="${p({
							plot: {
								x: 6,
								y: 6
							},
							radius: 5,
							sides: 6,
							rotation: 0
						}).path}" fill="${e.color}" stroke="${n}" stroke-width="1" /></svg>`;
						break;
					}
			}
			return Ll.set(t, r), r;
		}
		let Vl = b(() => {
			let e = "", t = $c.value.map((e) => e.value).filter((e) => se(e) && e !== null).reduce((e, t) => Math.abs(e) + Math.abs(t), 0), n = K.value ? { text: _(V.value.chart.grid.labels.xAxis.formatter, Vs.value[0]?.point.raw?.x ?? Vs.value[0]?.point.x, fe({
				v: Vs.value[0]?.point.raw?.x ?? Vs.value[0]?.point.x,
				r: V.value.chart.tooltip.roundingValue
			}), {
				datapoint: null,
				serriesIndex: null
			}) } : X.value[R.value], r = V.value.chart.tooltip.customFormat;
			if (De(r)) try {
				let e = r(Rl(n));
				if (typeof e == "string") return e;
			} catch (e) {
				console.warn("Vue Data UI - VueUiXy: custom tooltip formatter failed.", e);
			}
			if (n && n.text && V.value.chart.tooltip.showTimeLabel) {
				let t = Fl.value(R.value + U.value.start, V.value.chart.tooltip.timeFormat);
				K.value || (e += `<div style="padding-bottom: 6px; margin-bottom: 4px; border-bottom: 1px solid ${V.value.chart.tooltip.borderColor}; width:100%">${V.value.chart.grid.labels.xAxisLabels.datetimeFormatter.enable && !V.value.chart.tooltip.useDefaultTimeFormat ? t : n.text}</div>`);
			}
			return $c.value.forEach((r) => {
				if (se(r.value)) {
					let i = Bl(r), a = V.value.chart.tooltip.showValue ? _(r.type === "line" ? V.value.line.labels.formatter : r.type === "bar" ? V.value.bar.labels.formatter : V.value.plot.labels.formatter, r.value, fe({
						p: r.prefix,
						v: r.value,
						s: r.suffix,
						r: V.value.chart.tooltip.roundingValue
					}), { datapoint: r }) : "", o = V.value.chart.tooltip.showValue ? n?.text ?? "" : "", s = K.value ? `
                    <span>${V.value.chart.grid.labels.axis.xLabel || "X"}: </span>
                    <span>${o}</span>, 
                    <span>${V.value.chart.grid.labels.axis.yLabel || "Y"}: </span>
                    <span>${a}</span>
                ` : a;
					K.value ? e += `
                        <div style="display:flex;flex-direction:column;">
                            <div style="display:flex; flex-direction:row; gap:3px;">
                                <div style="width:20px">${i}</div>
                                <div style="display:flex;flex-direction:column;">
                                    <div>${r.name}</div>
                                    <div>${s}</div>
                                </div>
                            </div>
                        </div>
                    ` : e += `<div style="display:flex;flex-direction:row; align-items:center;gap:3px;white-space:nowrap;"><div style="width:20px">${i}</div> ${r.name}: <b>${s}</b> ${V.value.chart.tooltip.showPercentage ? `(${fe({
						v: m(Math.abs(r.value) / t * 100),
						s: "%",
						r: V.value.chart.tooltip.roundingPercentage
					})})` : ""}</div>`;
					let c = Dl(r.comments, R.value);
					V.value.chart.comments.showInTooltip && c && (e += `<div class="vue-data-ui-tooltip-comment" style="background:${r.color}20; padding: 6px; margin-bottom: 6px; border-left: 1px solid ${r.color}">${c}</div>`);
				}
			}), `<div style="border-radius:4px;padding:12px;font-variant-numeric: tabular-nums;color:${V.value.chart.tooltip.color}">${e}</div>`;
		}), Hl = b(() => {
			if (!K.value) return [];
			let e = /* @__PURE__ */ new Set(), t = [];
			return q.value.forEach((n) => {
				n.series.forEach((n) => {
					let r = Number(n?.x);
					if (!Number.isFinite(r)) return;
					let i = String(r);
					e.has(i) || (e.add(i), t.push(r));
				});
			}), t.sort((e, t) => Ds.value ? t - e : e - t);
		});
		function Ul(e) {
			let t = Number(e);
			return Number.isFinite(t) ? _(V.value.chart.grid.labels.xAxis.formatter, t, fe({
				v: t,
				r: V.value.table.rounding
			}), {
				datapoint: t,
				seriesIndex: null
			}) : "-";
		}
		function Wl(e) {
			return K.value ? Ul(e) : V.value.table.useDefaultTimeFormat ? X.value[e]?.text ?? "-" : Fl.value(e + U.value.start, V.value.table.timeFormat);
		}
		function Gl(e, t) {
			if (K.value) {
				let n = Number(t);
				return Number.isFinite(n) ? e.series.find((e) => Number(e?.x) === n)?.y ?? null : null;
			}
			return e.absoluteValues[t];
		}
		function Kl(e, t) {
			let n = Gl(e, t);
			return Ct(n) ? Number(n.toFixed(V.value.table.rounding)) : "";
		}
		function ql(e, t) {
			let n = Gl(e, t);
			return Ct(n) ? _(e.type === "line" ? V.value.line.labels.formatter : e.type === "bar" ? V.value.bar.labels.formatter : V.value.plot.labels.formatter, n, fe({
				p: e.prefix || V.value.chart.labels.prefix,
				v: n,
				s: e.suffix || V.value.chart.labels.suffix,
				r: V.value.table.rounding
			})) : "";
		}
		function Jl(e) {
			return q.value.map((t) => Gl(t, e) ?? 0).reduce((e, t) => e + t, 0);
		}
		let Yl = b(() => {
			if (K.value) return Hl.value.map((e) => {
				let t = Jl(e);
				return {
					period: Wl(e),
					csvValues: q.value.map((t) => Kl(t, e)),
					formattedValues: q.value.map((t) => ql(t, e)),
					sum: t,
					formattedSum: t.toFixed(V.value.table.rounding)
				};
			});
			let e = [];
			for (let t = 0; t < Z.value; t += 1) {
				let n = Jl(t);
				e.push({
					period: Wl(t),
					csvValues: q.value.map((e) => Kl(e, t)),
					formattedValues: q.value.map((e) => ql(e, t)),
					sum: n,
					formattedSum: n.toFixed(V.value.table.rounding)
				});
			}
			return e;
		}), Xl = b(() => xo.value.length === 0 ? {
			head: [],
			body: [],
			config: {},
			columnNames: []
		} : {
			head: q.value.map((e) => ({
				label: e.name,
				color: e.color,
				type: e.type
			})),
			body: Yl.value.map((e) => [e.period].concat(e.csvValues))
		}), Zl = b(() => {
			let e = V.value.table.showSum, t = [K.value ? V.value.chart.grid.labels.axis.xLabel || "X" : ""].concat(q.value.map((e) => e.name));
			e && (t = t.concat(" <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>"));
			let n = Yl.value.map((t) => [t.period].concat(t.formattedValues, e ? t.formattedSum : [])), r = {
				th: {
					backgroundColor: V.value.table.th.backgroundColor,
					color: V.value.table.th.color,
					outline: V.value.table.th.outline
				},
				td: {
					backgroundColor: V.value.table.td.backgroundColor,
					color: V.value.table.td.color,
					outline: V.value.table.td.outline
				},
				breakpoint: V.value.table.responsiveBreakpoint
			}, i = [K.value ? V.value.chart.grid.labels.axis.xLabel || "X" : V.value.table.columnNames.period].concat(q.value.map((e) => e.name), V.value.table.columnNames.total);
			return {
				head: t,
				body: n,
				config: r,
				colNames: i
			};
		});
		function Ql(e = null) {
			let n = [
				[V.value.chart.title.text],
				[V.value.chart.title.subtitle.text],
				[""]
			], r = [K.value ? V.value.chart.grid.labels.axis.xLabel || "X" : V.value.table.columnNames.period, ...Xl.value.head.map((e) => e.label)], i = Xl.value.body, a = n.concat([r]).concat(i), s = o(a);
			e ? e(s) : t({
				csvContent: s,
				title: V.value.chart.title.text || "vue-ui-xy"
			});
		}
		function $l(e, t = null) {
			if (Co.value) return;
			Ki.value = e;
			let n = q.value.map((e) => ({
				name: e.name,
				value: [
					null,
					void 0,
					NaN
				].includes(e.absoluteValues[t]) ? null : e.absoluteValues[t],
				color: e.color,
				type: e.type
			}));
			e ? (R.value = t, V.value.events.datapointEnter && V.value.events.datapointEnter({
				datapoint: n,
				seriesIndex: t + U.value.start
			})) : (R.value = null, V.value.events.datapointLeave && V.value.events.datapointLeave({
				datapoint: n,
				seriesIndex: t + U.value.start
			}));
		}
		function eu() {
			H.value.showTable = !H.value.showTable;
		}
		function tu() {
			H.value.dataLabels.show = !H.value.dataLabels.show;
		}
		function nu() {
			H.value.showTooltip = !H.value.showTooltip;
		}
		function ru(e) {
			Gi.value = e, Xi.value += 1;
		}
		function iu() {
			if (!V.value.responsiveProportionalSizing) {
				B.value.dataLabels = V.value.chart.grid.labels.fontSize, B.value.yAxis = V.value.chart.grid.labels.axis.fontSize, B.value.xAxis = V.value.chart.grid.labels.xAxisLabels.fontSize, B.value.plotLabels = V.value.chart.labels.fontSize, ka.value.plot = V.value.plot.radius, ka.value.line = V.value.line.radius, ka.value.selectedLine = V.value.line.dot.selectedRadius;
				return;
			}
			B.value.dataLabels = xe({
				relator: Bi.value,
				adjuster: 400,
				source: V.value.chart.grid.labels.fontSize,
				threshold: 10,
				fallback: 10
			}), B.value.yAxis = xe({
				relator: P.value,
				adjuster: 1e3,
				source: V.value.chart.grid.labels.axis.fontSize,
				threshold: 10,
				fallback: 10
			}), B.value.xAxis = xe({
				relator: P.value,
				adjuster: 1e3,
				source: V.value.chart.grid.labels.xAxisLabels.fontSize,
				threshold: 10,
				fallback: 10
			}), B.value.plotLabels = xe({
				relator: P.value,
				adjuster: 800,
				source: V.value.chart.labels.fontSize,
				threshold: 10,
				fallback: 10
			}), ka.value.plot = xe({
				relator: P.value,
				adjuster: 800,
				source: V.value.plot.radius,
				threshold: 1,
				fallback: 1
			}), ka.value.line = xe({
				relator: P.value,
				adjuster: 800,
				source: V.value.line.radius,
				threshold: 1,
				fallback: 0
			}), ka.value.selectedLine = xe({
				relator: P.value,
				adjuster: 800,
				source: V.value.line.dot.selectedRadius,
				threshold: 1,
				fallback: 0
			});
		}
		function au() {
			if (he(M.dataset) ? (Ce({
				componentName: "VueUiXy",
				type: "dataset",
				debug: La.value
			}), Ha.value = !0) : M.dataset.forEach((e, t) => {
				[null, void 0].includes(e.name) && (Ce({
					componentName: "VueUiXy",
					type: "datasetSerieAttribute",
					property: "name (string)",
					index: t,
					debug: La.value
				}), Ha.value = !0);
			}), La.value && M.dataset.forEach((e) => {
				e.series.forEach((t, n) => {
					se(t) || console.warn(`VueUiXy has detected an unsafe value in your dataset:\n-----> The serie '${e.name}' contains the value '${t}' at index ${n}.\n'${t}' was converted to null to allow the chart to display.`);
				});
			}), he(M.dataset) || (Ha.value = V.value.loading), ta.value = V.value.chart.userOptions.showOnChartHover, na.value = V.value.chart.userOptions.keepStateOnChartLeave, ra.value = !V.value.chart.userOptions.showOnChartHover, Ia(), V.value.responsive) {
				let e = wi.value.parentNode;
				Ii.value && (Li.value && Ii.value.unobserve(Li.value), Ii.value.disconnect(), Ii.value = null, Li.value = null);
				let { height: t, width: n } = e.getBoundingClientRect(), r = null, i = 0;
				V.value.chart.title.show && Ti.value && (r = Ti.value, i = r.getBoundingClientRect().height);
				let a = null, o = 0;
				V.value.chart.zoom.show && Ja.value > 6 && Na.value && Ei.value && Ei.value.$el && (a = Ei.value.$el, o = a.getBoundingClientRect().height);
				let s = null, c = 0;
				V.value.chart.legend.show && Di.value && (s = Di.value, c = s.getBoundingClientRect().height);
				let l = 0;
				Oi.value && (l = Oi.value.getBoundingClientRect().height);
				let u = 0;
				ki.value && (u = ki.value.getBoundingClientRect().height), Bi.value = t - i - c - o - l - u - 12, P.value = n, Vi.value = `0 0 ${P.value < 0 ? 10 : P.value} ${Bi.value < 0 ? 10 : Bi.value}`, iu();
				let d = new ResizeObserver((e) => {
					for (let t of e) i = V.value.chart.title.show && Ti.value ? Ti.value.getBoundingClientRect().height : 0, o = Ei.value && Ei.value.$el ? Ei.value.$el.getBoundingClientRect().height : 0, c = Di.value ? Di.value.getBoundingClientRect().height : 0, l = Oi.value ? Oi.value.getBoundingClientRect().height : 0, u = ki.value ? ki.value.getBoundingClientRect().height : 0, requestAnimationFrame(() => {
						Bi.value = t.contentRect.height - i - c - o - l - u - 12, P.value = t.contentBoxSize[0].inlineSize ?? t.contentRect.width, Vi.value = `0 0 ${P.value < 0 ? 10 : P.value} ${Bi.value < 0 ? 10 : Bi.value}`, iu(), wa();
					});
				});
				Ii.value = d, Li.value = e, d.observe(e);
			} else Bi.value = V.value.chart.height, P.value = V.value.chart.width, B.value.dataLabels = V.value.chart.grid.labels.fontSize, B.value.yAxis = V.value.chart.grid.labels.axis.fontSize, B.value.xAxis = V.value.chart.grid.labels.xAxisLabels.fontSize, B.value.plotLabels = V.value.chart.labels.fontSize, ka.value.plot = V.value.plot.radius, ka.value.line = V.value.line.radius, ka.value.selectedLine = V.value.line.dot.selectedRadius, Vi.value = `0 0 ${P.value} ${Bi.value}`;
			wa();
		}
		function ou(e) {
			Hi.value = {
				x: e.clientX,
				y: e.clientY
			};
		}
		pt(() => {
			au(), ss(), document.addEventListener("mousemove", ou, { passive: !0 }), document.addEventListener("scroll", Lo, { passive: !0 });
		}), ft(() => {
			document.removeEventListener("scroll", Lo, { passive: !0 }), document.removeEventListener("mousemove", ou, { passive: !0 }), Ii.value && (Li.value && Ii.value.unobserve(Li.value), Ii.value.disconnect(), Ii.value = null, Li.value = null), mu(), ms();
		}), We({
			timeLabelsEls: Ni,
			timeLabels: X,
			slicer: U,
			configRef: V,
			rotationPath: [
				"chart",
				"grid",
				"labels",
				"xAxisLabels",
				"rotation"
			],
			autoRotatePath: [
				"chart",
				"grid",
				"labels",
				"xAxisLabels",
				"autoRotate",
				"enable"
			],
			isAutoSize: ao,
			height: Bi,
			width: P,
			rotation: V.value.chart.grid.labels.xAxisLabels.autoRotate.angle
		});
		let su = D(!1), cu = D(null), lu = D(200), uu = b(() => R.value ?? I.value ?? 0), du = b(() => {
			if (!K.value) return null;
			if (a(ca.value)) return Number(ca.value);
			let e = Vs.value[0];
			return a(e?.point?.x) ? Number(e.point.x) : null;
		});
		function fu() {
			let e = Math.ceil(lu.value || 200);
			return Math.min(Math.max(e, 1), 200);
		}
		function pu() {
			let e = fu(), t = (K.value && a(du.value) ? pl({ x: du.value }) : _c(uu.value)) - e / 2 - (200 - e) / 2, n = Y.value?.left - (200 - e) / 2, r = Y.value?.right - (200 + e) / 2;
			return m(Math.max(n, Math.min(t, r)));
		}
		function mu() {
			pa.value !== null && (cancelAnimationFrame(pa.value), pa.value = null), fa.value && z.value && fa.value.unobserve(z.value), fa.value &&= (fa.value.disconnect(), null), ma.value &&= (ma.value(), null), z.value = null;
		}
		function hu(e) {
			pa.value !== null && cancelAnimationFrame(pa.value), pa.value = requestAnimationFrame(() => {
				lu.value = Math.min(Math.max(Math.ceil(e || 0), 1), 200), pa.value = null;
			});
		}
		function gu(e) {
			!fa.value || !e || (z.value && z.value !== e && (fa.value.unobserve(z.value), z.value = null), e !== z.value && (ut(() => {
				e.offsetParent !== null && hu(e.offsetWidth || e.getBoundingClientRect().width || 200);
			}), fa.value.observe(e), z.value = e));
		}
		pt(() => {
			fa.value = new ResizeObserver((e) => {
				let t = e.find((e) => e.target === z.value) || e[0];
				t && hu(t.contentRect.width || 200);
			}), ma.value = xt((e) => {
				let t = cu.value;
				if (!t) {
					fa.value && z.value && (fa.value.unobserve(z.value), z.value = null);
					return;
				}
				gu(t), e(() => {
					fa.value && z.value && z.value !== cu.value && (fa.value.unobserve(z.value), z.value = null);
				});
			});
		});
		let _u = b(() => {
			if (K.value) {
				if (!Number.isFinite(Number(du.value))) return "";
			} else if ([null, void 0].includes(R.value) && [null, void 0].includes(I.value)) return "";
			let e = (R.value == null ? 0 : R.value) || (I.value == null ? 0 : I.value), t = V.value.chart.timeTag.customFormat;
			if (su.value = !1, K.value) {
				let e = du.value;
				if (De(t)) try {
					let n = t({
						absoluteIndex: e,
						seriesIndex: e,
						datapoint: $c.value,
						bars: Ol.value,
						lines: Q.value,
						plots: $.value,
						config: V.value
					});
					if (typeof n == "string") return su.value = !0, n;
				} catch {
					console.warn("Custom format cannot be applied on timeTag."), su.value = !1;
				}
				return fe({
					v: e,
					r: V.value.chart.grid.labels.xAxis.rounding
				});
			}
			if (De(t)) try {
				let n = t({
					absoluteIndex: e + U.value.start,
					seriesIndex: e,
					datapoint: $c.value,
					bars: Ol.value,
					lines: Q.value,
					plots: $.value,
					config: V.value
				});
				if (typeof n == "string") return su.value = !0, n;
			} catch {
				console.warn("Custom format cannot be applied on timeTag."), su.value = !1;
			}
			if (!su.value) return [null, void 0].includes(X.value[e]) ? "" : V.value.chart.grid.labels.xAxisLabels.datetimeFormatter.enable && !V.value.chart.timeTag.useDefaultFormat ? Fl.value(e + U.value.start, V.value.chart.timeTag.timeFormat) : X.value[e].text;
		});
		function vu({ serie: e, plot: t, type: n }) {
			if (!Ct(t.value)) return "";
			let r = fe({
				p: e.prefix || V.value.chart.labels.prefix,
				v: t.value,
				s: e.suffix || V.value.chart.labels.suffix,
				r: V.value[n].labels.rounding
			}), i = t.datasetXValue, o = K.value && a(i) ? `x: ${fe({
				v: i,
				r: V.value.chart.grid.labels.xAxis.rounding
			})}\ny: ${r}` : _(V.value[n].labels.formatter, t.value, r, {
				datapoint: t,
				serie: e
			});
			return c({
				content: o,
				fontSize: B.value.plotLabels,
				fill: V.value[n].labels.color,
				x: 0,
				y: 0
			});
		}
		function yu({ plot: e }) {
			let t = V.value.bar.labels.offsetY, n = Math.abs(Ns(e)), r = e.value < 0, i = {
				x: Ls(e) + V.value.bar.labels.offsetX,
				y: m(e.y) + (V.value.bar.labels.alwaysOnTop ? t - (r ? n : 0) : e.value >= 0 ? t : -t * 3)
			};
			return `translate(${i.x}, ${i.y}) rotate(${V.value.bar.labels.rotation})`;
		}
		function bu({ plot: e, type: t }) {
			let n = V.value[t].labels.offsetY, r = {
				x: e.x + V.value[t].labels.offsetX,
				y: m(e.y) + (V.value[t].labels.alwaysOnTop || e.value >= 0 ? n : -n * 3)
			};
			return `translate(${r.x}, ${r.y}) rotate(${V.value[t].labels.rotation})`;
		}
		function xu({ plot: e, type: t }) {
			if (V.value[t].labels.textAnchor != null) return V.value[t].labels.textAnchor;
			let n = e.value >= 0;
			return V.value[t].labels.rotation === 0 ? "middle" : V.value[t].labels.alwaysOnTop || n ? "start" : "end";
		}
		let Su = b(() => K.value ? a(ca.value) ? pl({ x: Number(ca.value) }) : a(Hs.value) ? Hs.value : null : _c(R.value ?? I.value ?? 0)), Cu = b(() => V.value.chart.highlighter.crosshairs.show && $o.value), wu = b(() => [
			...Q.value,
			...$.value,
			...Ol.value
		].flatMap((e) => (e.plots || []).map((e, t) => ({
			plot: e,
			index: t
		})).filter(({ plot: t, index: n }) => Du(e, t, n)).filter(({ plot: e }) => Ct(e.value)).map(({ plot: t, index: n }) => ({
			...t,
			index: t.index ?? n,
			serie: e,
			x: m(e.type === "bar" ? Ls(t) : t.x),
			y: m(t.y)
		})))), Tu = b(() => {
			if (!Cu.value) return null;
			if (K.value) {
				let e = Vs.value[0], t = e?.point?.raw?.x ?? e?.point?.x;
				return a(t) ? {
					x: Su.value,
					text: _(V.value.chart.grid.labels.xAxis.formatter, t, fe({
						v: t,
						r: V.value.chart.tooltip.roundingValue
					}))
				} : null;
			}
			let e = R.value === null ? I.value : R.value;
			return e == null ? null : {
				x: _c(e),
				text: X.value[e]?.text ?? ""
			};
		});
		function Eu(e) {
			return V.value.chart.grid.labels.yAxis.position === "right" ? Y.value.right : Y.value.left;
		}
		function Du(e, t, n) {
			return K.value ? Vs.value.some((n) => n.datapoint.id === e.id && n.index === t.index) : R.value !== null && R.value === n || I.value !== null && I.value === n;
		}
		function Ou({ serie: e, plot: t, index: n, type: r }) {
			return !t || !Ct(t.value) || !V.value[r].labels.show || !H.value.dataLabels.show ? !1 : !Object.hasOwn(e, "dataLabels") || e.dataLabels === !0 || Du(e, t, n);
		}
		function ku(e, t) {
			let n = [];
			return e.forEach((e, r) => {
				(Array.isArray(e.plots) ? e.plots : []).forEach((i, a) => {
					Ou({
						serie: e,
						plot: i,
						index: a,
						type: t
					}) && n.push({
						key: `data_label_${t}_${e.id}_${(i.index ?? a) + U.value.start}`,
						serie: e,
						serieIndex: r,
						plot: i,
						plotIndex: a
					});
				});
			}), n;
		}
		let Au = b(() => ku(Ol.value, "bar")), ju = b(() => ku($.value, "plot")), Mu = b(() => ku(Q.value, "line"));
		bt(() => M.dataset, (e) => {
			if (Array.isArray(e) && e.length > 0 && (Ha.value = !1), V.value.chart.zoom.keepState) {
				let e = Ja.value;
				if (e > 0) {
					let t = U.value.start, n = U.value.end;
					if (!is.value || t === 0 && n === 0) ss();
					else {
						let r = Math.max(0, Math.min(t, e - 1)), i = Math.max(r + 1, Math.min(n, e));
						U.value = {
							start: r,
							end: i
						}, W.value = {
							start: r,
							end: i
						}, as.value = 0, os.value = e;
					}
				}
			} else U.value = {
				start: 0,
				end: Ja.value
			};
			Ri.value += 1, ea.value += 1, wa(), $a();
		}, { deep: !0 }), bt(() => M.config, (e) => {
			Ba.value || (V.value = ja()), au(), Qi.value += 1, Zi.value += 1, Ia(), V.value.chart.zoom.keepState && Ja.value > 0 && (!is.value || U.value.start === 0 && U.value.end === 0) && ss(), $a();
		}, { deep: !0 });
		let Nu = D(!1), Pu = D(!1);
		function Fu() {
			let e = wi.value?.parentNode;
			if (!e) {
				Pu.value = Nu.value, Nu.value = !1;
				return;
			}
			let t = e.getBoundingClientRect();
			Pu.value = Nu.value, Nu.value = t.width > 0 && t.height > 0;
		}
		function Iu() {
			return !Nu.value || Pu.value || is.value ? !1 : U.value.start === 0 && U.value.end === 0;
		}
		pt(() => {
			Fu(), da.value = new ResizeObserver(() => {
				Fu(), Nu.value && (au(), $a(), Iu() && ss());
			}), wi.value?.parentNode && da.value.observe(wi.value.parentNode);
		}), bt(V, () => {
			Ia();
		}, { immediate: !0 });
		let Lu = b(() => {
			let e = V.value.table.useDialog && !V.value.showTable, t = H.value.showTable;
			return {
				component: e ? _i : Qe,
				title: `${V.value.chart.title.text}${V.value.chart.title.subtitle.text ? `: ${V.value.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: V.value.table.th.backgroundColor,
					color: V.value.table.th.color,
					headerColor: V.value.table.th.color,
					headerBg: V.value.table.th.backgroundColor,
					isFullscreen: Gi.value,
					fullscreenParent: wi.value,
					forcedWidth: Math.min(800, window.innerWidth * .8)
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: V.value.chart.backgroundColor,
							color: V.value.chart.color
						},
						head: {
							backgroundColor: V.value.chart.backgroundColor,
							color: V.value.chart.color
						}
					}
				}
			};
		});
		bt(() => H.value.showTable, (e) => {
			V.value.showTable || (e && V.value.table.useDialog && sa.value ? sa.value.open() : sa.value && sa.value.close && sa.value.close());
		});
		function Ru() {
			H.value.showTable = !1, Fi.value && Fi.value.setTableIconState(!1);
		}
		let zu = b(() => So.value.map((e) => ({
			shape: e.type === "bar" ? "square" : e.shape ?? "circle",
			color: e.color,
			name: e.name
		}))), Bu = b(() => V.value.chart.backgroundColor), Vu = b(() => V.value.chart.legend), Hu = b(() => V.value.chart.title), { isCallbackImaging: Uu, isCallbackSvg: Wu, generateSvg: Gu, onGenerateImage: Ku } = Ve({
			svg: L,
			title: Hu,
			legend: Vu,
			legendItems: zu,
			backgroundColor: Bu,
			getSvgCallback: () => V.value.chart.userOptions.callbacks.svg,
			generateImage: io
		});
		async function qu() {
			if (vi("copyAlt", {
				config: {
					...V.value,
					formattedDates: X.value
				},
				dataset: {
					lines: Q.value,
					bars: Ol.value,
					plots: $.value
				}
			}), !V.value.chart.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(V.value.chart.userOptions.callbacks.altCopy({
				config: {
					...V.value,
					formattedDates: X.value
				},
				dataset: {
					lines: Q.value,
					bars: Ol.value,
					plots: $.value
				}
			}));
		}
		let Ju = D(!1);
		function Yu() {
			U.value.end > U.value.start && (la.value = null), zo.value = !0, Ju.value = !0;
		}
		function Xu() {
			la.value = null, Xs(), Ju.value = !1, zo.value = !1;
		}
		function Zu(e) {
			if (!L.value || Wi.value || document.activeElement !== L.value) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight";
			if (!t && !n || !U.value.end && U.value.end !== 0) return;
			let r = U.value.end - U.value.start;
			if (r <= 0) return;
			if (e.preventDefault(), e.stopPropagation(), K.value) {
				let e = Gs.value.length;
				if (!e) return;
				let r = la.value;
				r !== null && r >= 0 && r < e ? n ? (r += 1, r >= e && (r = 0)) : t && (--r, r < 0 && (r = e - 1)) : r = n ? 0 : e - 1, Ks(r);
				return;
			}
			let i = la.value, a = Bs.value;
			i !== null && i >= 0 && i < r ? n ? (i += 1, i >= r && (i = 0)) : t && (--i, i < 0 && (i = r - 1)) : a !== null && a >= 0 && a < r ? (i = n ? a + 1 : a - 1, i >= r && (i = 0), i < 0 && (i = r - 1)) : i = n ? 0 : r - 1, la.value = i, Qu(i), $l(!0, i);
		}
		function Qu(e) {
			let t = U.value.end - U.value.start;
			if (t <= 0) return;
			let n = Y.value.width / t, r = Y.value.left + n * e + n / 2, i = Y.value.top + Y.value.height / 2, a = l(r, i, L.value);
			a && (ua.value = a);
		}
		let $u = b(() => {
			if (!Zl.value) return null;
			let e = V.value.table.showSum, t = [V.value.table.columnNames.period].concat(q.value.map((e) => e.name));
			return e && (t = t.concat(V.value.table.columnNames.total)), {
				headers: t,
				rows: Zl.value.body
			};
		});
		return Ye({
			getData: $s,
			getImage: ec,
			generatePdf: ro,
			generateImage: io,
			generateSvg: Gu,
			generateCsv: Ql,
			hideSeries: sc,
			showSeries: oc,
			toggleStack: es,
			toggleTable: eu,
			toggleLabels: tu,
			toggleTooltip: nu,
			toggleAnnotator: Uo,
			toggleFullscreen: ru,
			copyAlt: qu,
			resetZoom: bs
		}), (t, n) => (E(), S("div", {
			id: `vue-ui-xy_${F.value}`,
			class: w(`vue-data-ui-component vue-ui-xy ${Gi.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${V.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			ref_key: "chart",
			ref: wi,
			style: T(`background:${V.value.chart.backgroundColor}; color:${V.value.chart.color};width:100%;font-family:${V.value.chart.fontFamily};${V.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: Vo,
			onMouseleave: Ho,
			onClick: Zs
		}, [
			C("div", {
				id: `chart-instructions-${F.value}`,
				class: "sr-only"
			}, [C("p", null, gt(V.value.a11y.translations.keyboardNavigation), 1)], 8, Pt),
			$u.value?.rows?.length ? (E(), nt(et, {
				key: 0,
				uid: F.value,
				head: $u.value.headers,
				body: $u.value.rows,
				notice: V.value.a11y.translations.tableAvailable,
				caption: V.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : x("", !0),
			V.value.chart.userOptions.buttons.annotator && L.value ? (E(), nt(A(gi), {
				key: 1,
				svgRef: L.value,
				backgroundColor: V.value.chart.backgroundColor,
				color: V.value.chart.color,
				active: Wi.value,
				isCursorPointer: Fa.value,
				onClose: Uo
			}, {
				"annotator-action-close": j(() => [k(t.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": j(({ color: e }) => [k(t.$slots, "annotator-action-color", dt(ct({ color: e })), void 0, !0)]),
				"annotator-action-draw": j(({ mode: e }) => [k(t.$slots, "annotator-action-draw", dt(ct({ mode: e })), void 0, !0)]),
				"annotator-action-undo": j(({ disabled: e }) => [k(t.$slots, "annotator-action-undo", dt(ct({ disabled: e })), void 0, !0)]),
				"annotator-action-redo": j(({ disabled: e }) => [k(t.$slots, "annotator-action-redo", dt(ct({ disabled: e })), void 0, !0)]),
				"annotator-action-delete": j(({ disabled: e }) => [k(t.$slots, "annotator-action-delete", dt(ct({ disabled: e })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : x("", !0),
			uc.value ? (E(), S("div", {
				key: 2,
				ref_key: "noTitle",
				ref: ki,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%; background:transparent"
			}, null, 512)) : x("", !0),
			V.value.chart.title.show ? (E(), S("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Ti,
				class: "vue-ui-xy-title",
				style: T(`font-family:${V.value.chart.fontFamily}`)
			}, [(E(), nt(Ke, {
				key: `title_${Qi.value}`,
				config: {
					title: {
						cy: "xy-div-title",
						...V.value.chart.title
					},
					subtitle: {
						cy: "xy-div-subtitle",
						...V.value.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 4)) : x("", !0),
			C("div", { id: `legend-top-${F.value}` }, null, 8, Ft),
			V.value.chart.userOptions.show && (na.value || ra.value) ? (E(), nt(A(fi), {
				ref_key: "userOptionsRef",
				ref: Fi,
				key: `user_options_${Xi.value}`,
				backgroundColor: V.value.chart.backgroundColor,
				color: V.value.chart.color,
				isPrinting: A(to),
				isImaging: A(no),
				uid: F.value,
				hasTooltip: V.value.chart.userOptions.buttons.tooltip && V.value.chart.tooltip.show,
				hasPdf: V.value.chart.userOptions.buttons.pdf,
				hasXls: V.value.chart.userOptions.buttons.csv,
				hasImg: V.value.chart.userOptions.buttons.img,
				hasSvg: V.value.chart.userOptions.buttons.svg,
				hasLabel: V.value.chart.userOptions.buttons.labels,
				hasTable: V.value.chart.userOptions.buttons.table,
				hasStack: e.dataset.length > 1 && V.value.chart.userOptions.buttons.stack,
				hasFullscreen: V.value.chart.userOptions.buttons.fullscreen,
				hasAltCopy: V.value.chart.userOptions.buttons.altCopy,
				isStacked: H.value.isStacked,
				isFullscreen: Gi.value,
				chartElement: t.$refs.chart,
				position: V.value.chart.userOptions.position,
				isTooltip: H.value.showTooltip,
				titles: { ...V.value.chart.userOptions.buttonTitles },
				hasAnnotator: V.value.chart.userOptions.buttons.annotator,
				isAnnotation: Wi.value,
				callbacks: V.value.chart.userOptions.callbacks,
				tableDialog: V.value.table.useDialog,
				printScale: V.value.chart.userOptions.print.scale,
				isCursorPointer: Fa.value,
				onToggleFullscreen: ru,
				onGeneratePdf: A(ro),
				onGenerateCsv: Ql,
				onGenerateImage: A(Ku),
				onGenerateSvg: A(Gu),
				onToggleTable: eu,
				onToggleLabels: tu,
				onToggleStack: es,
				onToggleTooltip: nu,
				onToggleAnnotator: Uo,
				onCopyAlt: qu,
				style: T({ visibility: na.value ? ra.value ? "visible" : "hidden" : "visible" })
			}, rt({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: j(({ isOpen: e, color: n }) => [k(t.$slots, "menuIcon", dt(ct({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: j(() => [k(t.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: j(() => [k(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: j(() => [k(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: j(() => [k(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionSvg ? {
					name: "optionSvg",
					fn: j(() => [k(t.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionTable ? {
					name: "optionTable",
					fn: j(() => [k(t.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionLabels ? {
					name: "optionLabels",
					fn: j(() => [k(t.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots.optionStack ? {
					name: "optionStack",
					fn: j(({ isStack: e }) => [k(t.$slots, "optionStack", dt(ct({ isStack: e })), void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: j(({ toggleFullscreen: e, isFullscreen: n }) => [k(t.$slots, "optionFullscreen", dt(ct({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				t.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: j(({ toggleAnnotator: e, isAnnotator: n }) => [k(t.$slots, "optionAnnotator", dt(ct({
						toggleAnnotator: e,
						isAnnotator: n
					})), void 0, !0)]),
					key: "10"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: j(({ altCopy: e }) => [k(t.$slots, "optionAltCopy", dt(ct({ altCopy: e })), void 0, !0)]),
					key: "11"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: j(() => [k(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "12"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: j(() => [k(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "13"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasLabel.hasTable.hasStack.hasFullscreen.hasAltCopy.isStacked.isFullscreen.chartElement.position.isTooltip.titles.hasAnnotator.isAnnotation.callbacks.tableDialog.printScale.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : x("", !0),
			C("div", It, [(E(), S("svg", {
				ref_key: "svgRef",
				ref: L,
				xmlns: "http://www.w3.org/2000/svg",
				class: w([{
					"vue-data-ui-fullscreen--on": Gi.value,
					"vue-data-ui-fulscreen--off": !Gi.value,
					"vue-data-ui-no-transition": !A(Pa)
				}, "vue-ui-xy-svg vue-data-ui-svg"]),
				width: "100%",
				viewBox: Vi.value,
				style: T({
					background: "transparent",
					color: V.value.chart.color,
					fontFamily: V.value.chart.fontFamily
				}),
				"aria-label": cc.value,
				"aria-describedby": `chart-instructions-${F.value}`,
				"aria-live": "polite",
				role: "img",
				tabindex: "0",
				preserveAspectRatio: "xMidYMid",
				onMousemove: Ys,
				onMouseleave: Xs,
				onClick: Zs,
				onFocus: Yu,
				onBlur: Xu,
				onKeydown: Zu
			}, [C("g", {
				ref_key: "G",
				ref: Ai,
				class: "vue-data-ui-g"
			}, [
				at(A(hi)),
				t.$slots["chart-background"] ? (E(), S("foreignObject", {
					key: 0,
					x: Math.max(0, Y.value?.left),
					y: Y.value?.top,
					width: Math.max(0, Y.value?.width),
					height: Math.max(0, Y.value?.height),
					style: { pointerEvents: "none" }
				}, [k(t.$slots, "chart-background", {}, void 0, !0)], 8, Rt)) : x("", !0),
				Z.value > 0 ? (E(), S("g", zt, [
					C("g", Bt, [
						V.value.chart.grid.labels.xAxis.showBaseline ? (E(), S("line", {
							key: 0,
							stroke: V.value.chart.grid.stroke,
							"stroke-width": "1",
							x1: Y.value?.left,
							x2: Y.value?.right,
							y1: A(v)(Y.value?.bottom),
							y2: A(v)(Y.value?.bottom),
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Vt)) : x("", !0),
						H.value.useIndividualScale ? V.value.chart.grid.showHorizontalLines ? (E(!0), S(y, { key: 2 }, O(Al.value, (e) => (E(), S("g", { key: `individual_grid_${e.groupId || e.id}` }, [e.id === N.value && e.yLabels.length ? (E(!0), S(y, { key: 0 }, O(e.yLabels, (t) => (E(), S("line", {
							key: `selected_individual_grid_line_${e.groupId || e.id}_${t.value}_${t.y}`,
							x1: Y.value?.left,
							x2: Y.value?.right,
							y1: A(v)(t.y),
							y2: A(v)(t.y),
							stroke: e.color,
							"stroke-width": .5,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Gt))), 128)) : e.yLabels.length ? (E(!0), S(y, { key: 1 }, O(e.yLabels, (t) => (E(), S("line", {
							key: `individual_grid_line_${e.groupId || e.id}_${t.value}_${t.y}`,
							x1: Y.value?.left,
							x2: Y.value?.right,
							y1: A(v)(t.y),
							y2: A(v)(t.y),
							stroke: V.value.chart.grid.stroke,
							"stroke-width": .5,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Kt))), 128)) : x("", !0)]))), 128)) : x("", !0) : (E(), S(y, { key: 1 }, [V.value.chart.grid.labels.yAxis.showBaseline ? (E(), S("line", {
							key: 0,
							class: "vue-ui-xy-y-axis",
							stroke: V.value.chart.grid.stroke,
							"stroke-width": "1",
							x1: J.value ? Y.value?.right : Y.value?.left,
							x2: J.value ? Y.value?.right : Y.value?.left,
							y1: A(v)(Y.value?.top),
							y2: A(v)(Y.value?.bottom),
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Ht)) : x("", !0), V.value.chart.grid.showHorizontalLines ? (E(), S("g", Ut, [(E(!0), S(y, null, O(el.value, (e) => (E(), S("line", {
							key: `horizontal_grid_line_${e.value}_${e.y}`,
							x1: Y.value?.left,
							x2: Y.value?.right,
							y1: A(v)(e.y),
							y2: A(v)(e.y),
							stroke: V.value.chart.grid.stroke,
							"stroke-width": .5,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Wt))), 128))])) : x("", !0)], 64)),
						V.value.chart.grid.showVerticalLines ? (E(), S("g", qt, [C("path", {
							d: No.value,
							"stroke-width": .5,
							stroke: V.value.chart.grid.stroke,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Jt)])) : x("", !0),
						V.value.chart.grid.labels.xAxisLabels.show ? (E(), S("g", Yt, [C("path", {
							d: Po.value,
							stroke: V.value.chart.grid.stroke,
							"stroke-width": 1,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Xt)])) : x("", !0)
					]),
					(E(!0), S(y, null, O(Ol.value, (e, n) => (E(), S("defs", { key: `def_rect_${n}` }, [t.$slots["bar-gradient"] ? k(t.$slots, "bar-gradient", lt({ ref_for: !0 }, {
						series: e,
						positiveId: `rectGradient_pos_${n}_${F.value}`,
						negativeId: `rectGradient_neg_${n}_${F.value}`
					}), void 0, !0, 0) : (E(), S(y, { key: 1 }, [at(Xe, {
						t: "linear",
						id: `rectGradient_pos_${n}_${F.value}`,
						x2: "0%",
						y2: "100%",
						stops: [
							[
								"0%",
								e.color,
								1
							],
							[
								"62%",
								A(ue)(e.color, .02),
								1
							],
							[
								"100%",
								A(ue)(e.color, .05),
								1
							]
						]
					}, null, 8, ["id", "stops"]), at(Xe, {
						t: "linear",
						id: `rectGradient_neg_${n}_${F.value}`,
						x2: "0%",
						y2: "100%",
						stops: [
							[
								"0%",
								A(ue)(e.color, .05),
								1
							],
							[
								"38%",
								A(ue)(e.color, .02),
								1
							],
							[
								"100%",
								e.color,
								1
							]
						]
					}, null, 8, ["id", "stops"])], 64))]))), 128)),
					(E(!0), S(y, null, O($.value, (e, t) => (E(), S("defs", { key: `def_plot_${t}` }, [at(Xe, {
						t: "radial",
						id: `plotGradient_${t}_${F.value}`,
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						stops: [[
							"0%",
							A(ue)(e.color, .05),
							1
						], [
							"100%",
							e.color,
							1
						]]
					}, null, 8, ["id", "stops"])]))), 128)),
					(E(!0), S(y, null, O(Q.value, (e, n) => (E(), S(y, { key: `def_line_${e.id}` }, [C("defs", null, [at(Xe, {
						t: "radial",
						id: `lineGradient_${n}_${F.value}`,
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						stops: [[
							"0%",
							A(ue)(e.color, .05),
							1
						], [
							"100%",
							e.color,
							1
						]]
					}, null, 8, ["id", "stops"]), t.$slots["area-gradient"] ? k(t.$slots, "area-gradient", lt({ ref_for: !0 }, {
						series: e,
						id: `areaGradient_${n}_${F.value}`
					}), void 0, !0, 0) : (E(), nt(Xe, {
						key: 1,
						t: "linear",
						id: `areaGradient_${n}_${F.value}`,
						x1: "0%",
						x2: "100%",
						y1: "0%",
						y2: "0%",
						stops: [[
							"0%",
							A(i)(A(ue)(e.color, .03), V.value.line.area.opacity),
							1
						], [
							"100%",
							A(i)(e.color, V.value.line.area.opacity),
							1
						]]
					}, null, 8, ["id", "stops"]))]), e.temperatureColors && !e.isFlatTemperatureLine ? (E(), S("defs", Zt, [C("linearGradient", {
						id: `temperature_grad_line_${n}_${F.value}`,
						gradientTransform: "rotate(90)"
					}, [(E(!0), S(y, null, O(e.temperatureColors, (t, r) => (E(), S("stop", {
						key: `temperature_grad_stop_${n}_${r}_${F.value}`,
						"stop-color": t,
						offset: A(ke)(r, e.temperatureColors.length)
					}, null, 8, $t))), 128))], 8, Qt)])) : x("", !0)], 64))), 128)),
					(E(!0), S(y, null, O(dc.value, (e) => (E(), S("g", { key: `highlight_area_${e.from}_${e.span}_${e.color}` }, [e.show ? (E(), S(y, { key: 0 }, [(E(!0), S(y, null, O(e.span, (t, n) => (E(), S("g", { key: `highlight_area_rect_${e.from}_${n}` }, [C("rect", {
						style: T({
							transition: "none",
							opacity: +(e.from + n >= U.value.start && e.from + n <= U.value.end - 1)
						}),
						x: yc(e.from + n - U.value.start),
						y: Y.value?.top,
						height: Y.value.height < 0 ? 10 : Y.value.height,
						width: bc(e.from + n - U.value.start),
						fill: A(i)(e.color, e.opacity)
					}, null, 12, en)]))), 128)), (E(!0), S(y, null, O(e.span, (t, n) => (E(), S("g", { key: `highlight_area_caption_${e.from}_${n}` }, [e.caption.text && n === 0 ? (E(), S("foreignObject", {
						key: 0,
						x: yc(e.from + n - U.value.start) - (e.caption.width === "auto" ? 0 : e.caption.width / 2 - Cc(e.span) / 2),
						y: Y.value?.top + e.caption.offsetY,
						style: T({
							overflow: "visible",
							opacity: +(e.to >= U.value.start && e.from < U.value.end)
						}),
						height: "1",
						width: e.caption.width === "auto" ? Cc(e.span) : e.caption.width
					}, [C("div", { style: T(`padding:${e.caption.padding}px;text-align:${e.caption.textAlign};font-size:${e.caption.fontSize}px;color:${e.caption.color};font-weight:${e.caption.bold ? "bold" : "normal"}`) }, gt(e.caption.text), 5)], 12, tn)) : x("", !0)]))), 128))], 64)) : x("", !0)]))), 128)),
					zo.value && !K.value ? (E(), S("g", nn, [(E(!0), S(y, null, O(Z.value, (e, t) => (E(), S("g", { key: `tooltip_trap_highlighter_${t}` }, [C("rect", {
						x: yc(t),
						y: Y.value?.top,
						height: Y.value.height < 0 ? 10 : Y.value.height,
						width: Sc(t),
						fill: [
							I.value,
							R.value,
							qi.value
						].includes(t) ? A(i)(V.value.chart.highlighter.color, V.value.chart.highlighter.crosshairs.show ? 0 : V.value.chart.highlighter.opacity) : "transparent",
						style: {
							transition: "none !important",
							animation: "none !important"
						}
					}, null, 8, rn)]))), 128))])) : x("", !0),
					Ol.value.length ? (E(!0), S(y, { key: 1 }, O(Ol.value, (e, n) => (E(), S("g", {
						key: `serie_bar_${e.id}`,
						class: w(`serie_bar_${n}`),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [(E(!0), S(y, null, O(e.plots, (r, i) => (E(), S("g", { key: `bar_plot_${n}_${i}` }, [
						A(Ct)(r.value) ? (E(), S("rect", {
							key: 0,
							x: Is(r) + Bc.value / 2,
							y: H.value.useIndividualScale ? zs(r) : Rs(r),
							height: H.value.useIndividualScale ? Math.abs(Ns(r)) : Math.abs(Ms(r)),
							width: zc.value - Bc.value,
							rx: V.value.bar.borderRadius,
							fill: V.value.bar.useGradient ? r.value >= 0 ? `url(#rectGradient_pos_${n}_${F.value})` : `url(#rectGradient_neg_${n}_${F.value})` : e.color,
							stroke: V.value.bar.border.useSerieColor ? e.color : V.value.bar.border.stroke,
							"stroke-width": V.value.bar.border.strokeWidth,
							style: T({ transition: A(Ba) || !V.value.bar.showTransition ? void 0 : `all ${V.value.bar.transitionDurationMs}ms ease-in-out` })
						}, null, 12, an)) : x("", !0),
						A(Ct)(r.value) && t.$slots.pattern ? (E(), S("rect", {
							key: 1,
							x: Is(r) - Bc.value / 2,
							y: H.value.useIndividualScale ? zs(r) : Rs(r),
							height: H.value.useIndividualScale ? Math.abs(Ns(r)) : Math.abs(Ms(r)),
							width: zc.value - Bc.value,
							rx: V.value.bar.borderRadius,
							fill: `url(#pattern_${F.value}_${e.slotAbsoluteIndex})`,
							stroke: V.value.bar.border.useSerieColor ? e.color : V.value.bar.border.stroke,
							"stroke-width": V.value.bar.border.strokeWidth,
							style: T({ transition: A(Ba) || !V.value.bar.showTransition ? void 0 : `all ${V.value.bar.transitionDurationMs}ms ease-in-out` })
						}, null, 12, on)) : x("", !0),
						r.comment && V.value.chart.comments.show ? (E(), S("foreignObject", {
							key: 2,
							style: { overflow: "visible" },
							height: "12",
							width: zc.value + V.value.chart.comments.width,
							x: Is(r) - V.value.chart.comments.width / 2 + V.value.chart.comments.offsetX,
							y: A(m)(r.y) + V.value.chart.comments.offsetY + 6
						}, [k(t.$slots, "plot-comment", { plot: {
							...r,
							color: e.color,
							seriesIndex: n,
							datapointIndex: i
						} }, void 0, !0)], 8, sn)) : x("", !0)
					]))), 128))], 6))), 128)) : x("", !0),
					!H.value.useIndividualScale && V.value.chart.grid.labels.zeroLine.show ? (E(), S("line", {
						key: 2,
						stroke: V.value.chart.grid.stroke,
						"stroke-width": "1",
						x1: Y.value?.left,
						x2: Y.value?.right,
						y1: A(v)(js.value),
						y2: A(v)(js.value),
						"stroke-linecap": "round",
						style: { animation: "none !important" }
					}, null, 8, cn)) : x("", !0),
					!V.value.chart.highlighter.crosshairs.show && (V.value.chart.highlighter.useLine || K.value) && $o.value ? (E(), S("g", ln, [C("line", {
						x1: Su.value,
						x2: Su.value,
						y1: A(v)(Y.value?.top),
						y2: A(v)(Y.value?.bottom),
						stroke: V.value.chart.highlighter.color,
						"stroke-width": V.value.chart.highlighter.lineWidth,
						"stroke-dasharray": V.value.chart.highlighter.lineDasharray,
						"stroke-linecap": "round",
						style: {
							transition: "none !important",
							animation: "none !important",
							"pointer-events": "none"
						}
					}, null, 8, un)])) : x("", !0),
					Cu.value ? (E(), S("g", dn, [(E(!0), S(y, null, O(wu.value, (e) => (E(), S("g", { key: `crosshair_${e.serie.id}_${e.index}` }, [
						C("line", {
							x1: V.value.chart.highlighter.crosshairs.stopOnPoint ? A(v)(e.x) : Y.value.left,
							x2: V.value.chart.highlighter.crosshairs.stopOnPoint ? A(v)(Eu(e.x)) : Y.value.right,
							y1: A(v)(e.y),
							y2: A(v)(e.y),
							stroke: V.value.chart.highlighter.crosshairs.stroke,
							"stroke-width": V.value.chart.highlighter.crosshairs.strokeWidth,
							"stroke-dasharray": V.value.chart.highlighter.crosshairs.strokeDasharray,
							"stroke-linecap": "round",
							style: { "pointer-events": "none" }
						}, null, 8, fn),
						C("line", {
							x1: A(v)(e.x),
							x2: A(v)(e.x),
							y1: V.value.chart.highlighter.crosshairs.stopOnPoint ? A(v)(e.y) : Y.value.top,
							y2: V.value.chart.highlighter.crosshairs.stopOnPoint ? A(v)(Y.value.bottom) : Y.value.bottom,
							stroke: V.value.chart.highlighter.crosshairs.stroke,
							"stroke-width": V.value.chart.highlighter.crosshairs.strokeWidth,
							"stroke-dasharray": V.value.chart.highlighter.crosshairs.strokeDasharray,
							"stroke-linecap": "round",
							style: { "pointer-events": "none" }
						}, null, 8, pn),
						C("circle", {
							cx: A(v)(Eu(e.x)),
							cy: A(v)(e.y),
							r: V.value.chart.highlighter.crosshairs.dot.radius,
							fill: V.value.chart.highlighter.crosshairs.dot.fill,
							stroke: V.value.chart.highlighter.crosshairs.dot.stroke,
							"stroke-width": V.value.chart.highlighter.crosshairs.dot.strokeWidth,
							style: { "pointer-events": "none" }
						}, null, 8, mn)
					]))), 128))])) : x("", !0),
					V.value.chart.grid.frame.show ? (E(), S("rect", {
						key: 5,
						style: {
							pointerEvents: "none",
							transition: "none",
							animation: "none !important"
						},
						x: Math.max(0, Y.value?.left),
						y: Y.value?.top,
						width: Math.max(0, Y.value?.width),
						height: Y.value.height < 0 ? 0 : Y.value.height,
						fill: "transparent",
						stroke: V.value.chart.grid.frame.stroke,
						"stroke-width": V.value.chart.grid.frame.strokeWidth,
						"stroke-linecap": V.value.chart.grid.frame.strokeLinecap,
						"stroke-linejoin": V.value.chart.grid.frame.strokeLinejoin,
						"stroke-dasharray": V.value.chart.grid.frame.strokeDasharray
					}, null, 8, hn)) : x("", !0),
					V.value.chart.grid.labels.show ? (E(), S("g", {
						key: 6,
						ref_key: "scaleLabels",
						ref: Pi,
						opacity: Cu.value ? .2 : 1,
						style: { transition: "opacity 0.2s" }
					}, [H.value.useIndividualScale ? (E(), S(y, { key: 0 }, [(E(!0), S(y, null, O(Al.value, (e) => (E(), S("g", { key: `individual_scale_axis_${e.groupId || e.id}` }, [C("line", {
						x1: H.value.isStacked ? J.value ? Y.value.right : Y.value.left : J.value ? e.x : e.x - Y.value.individualOffsetX,
						x2: H.value.isStacked ? J.value ? Y.value.right : Y.value.left : J.value ? e.x : e.x - Y.value.individualOffsetX,
						y1: H.value.isStacked ? A(v)(Y.value?.bottom - e.yOffset - e.individualHeight) : A(v)(Y.value?.top),
						y2: H.value.isStacked ? A(v)(Y.value?.bottom - e.yOffset) : A(v)(Y.value?.bottom),
						stroke: e.color,
						"stroke-width": V.value.chart.grid.stroke,
						"stroke-linecap": "round",
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .3 : 1};transition:opacity 0.2s ease-in-out; animation: none !important`)
					}, null, 12, _n)]))), 128)), (E(!0), S(y, null, O(Al.value, (e) => (E(), S("g", {
						key: `individual_scale_label_${e.groupId || e.id}`,
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .3 : 1};transition:opacity 0.2s ease-in-out`)
					}, [
						C("text", {
							class: w({ "vue-data-ui-transition": A(Pa) }),
							fill: e.color,
							"font-size": B.value.dataLabels * .8,
							"text-anchor": "middle",
							transform: `translate(${H.value.isStacked ? J.value ? Y.value.right + V.value.chart.grid.labels.yAxis.crosshairSize + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + 5 + V.value.chart.grid.labels.yAxis.labelWidth + Y.value.individualOffsetX + V.value.chart.grid.labels.axis.yLabelOffsetX : Y.value.left - V.value.chart.grid.labels.yAxis.crosshairSize - V.value.chart.grid.labels.yAxis.scaleValueOffsetX - V.value.chart.grid.labels.yAxis.labelWidth - V.value.chart.grid.labels.axis.yLabelOffsetX : J.value ? e.x + V.value.chart.grid.labels.yAxis.crosshairSize + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + 5 + V.value.chart.grid.labels.yAxis.labelWidth + V.value.chart.grid.labels.axis.yLabelOffsetX : e.x - B.value.dataLabels * .8 / 2}, ${H.value.isStacked ? Y.value?.bottom - e.yOffset - e.individualHeight / 2 : Y.value?.top + Y.value.height / 2}) rotate(-90)`
						}, gt(e.name) + " " + gt(e.scaleLabel && e.unique && e.scaleLabel !== e.id ? `- ${e.scaleLabel}` : ""), 11, vn),
						(E(!0), S(y, null, O(e.yLabels, (t, n) => (E(), S(y, { key: `individual_scale_y_crosshair_${e.groupId || e.id}_${t.value}_${n}` }, [V.value.chart.grid.labels.yAxis.showCrosshairs ? (E(), S("line", {
							key: 0,
							x1: H.value.isStacked ? J.value ? Y.value.right : Y.value.left : J.value ? e.x : e.x + 3 - V.value.chart.grid.labels.yAxis.crosshairSize - Y.value.individualOffsetX,
							x2: H.value.isStacked ? J.value ? Y.value.right + V.value.chart.grid.labels.yAxis.crosshairSize : Y.value.left - V.value.chart.grid.labels.yAxis.crosshairSize : J.value ? e.x + V.value.chart.grid.labels.yAxis.crosshairSize : e.x - Y.value.individualOffsetX,
							y1: A(v)(t.y),
							y2: A(v)(t.y),
							stroke: e.color,
							"stroke-width": 1,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, yn)) : x("", !0)], 64))), 128)),
						(E(!0), S(y, null, O(e.yLabels, (t, n) => (E(), S("text", {
							class: w({ "vue-data-ui-transition": A(Pa) }),
							key: `individual_scale_y_label_${e.groupId || e.id}_${t.value}_${n}`,
							transform: `translate(${H.value.isStacked ? J.value ? Y.value.right + V.value.chart.grid.labels.yAxis.crosshairSize + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + 5 : Y.value.left - V.value.chart.grid.labels.yAxis.crosshairSize - V.value.chart.grid.labels.yAxis.scaleValueOffsetX - 5 : J.value ? e.x + V.value.chart.grid.labels.yAxis.crosshairSize + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + 5 : e.x - 5 - Y.value.individualOffsetX}, ${A(v)(t.y) + B.value.dataLabels / 3})`,
							"text-anchor": J.value ? "start" : "end",
							"font-size": B.value.dataLabels,
							fill: e.color
						}, gt(A(_)(V.value.chart.grid.labels.yAxis.formatter, t.value, A(fe)({
							p: t.prefix,
							v: t.value,
							s: t.suffix,
							r: V.value.chart.grid.labels.yAxis.rounding
						}), {
							datapoint: t.datapoint,
							seriesIndex: n
						})), 11, bn))), 128))
					], 4))), 128))], 64)) : (E(!0), S(y, { key: 1 }, O(el.value, (e, t) => (E(), S("g", { key: `yLabel_${t}` }, [A(Ct)(e) && e.value >= G.value.min && e.value <= G.value.max && V.value.chart.grid.labels.yAxis.showCrosshairs ? (E(), S("line", {
						key: 0,
						x1: J.value ? Y.value?.right : Y.value?.left,
						x2: J.value ? Y.value?.right + V.value.chart.grid.labels.yAxis.crosshairSize : Y.value?.left - V.value.chart.grid.labels.yAxis.crosshairSize,
						y1: A(v)(e.y),
						y2: A(v)(e.y),
						stroke: V.value.chart.grid.stroke,
						"stroke-width": "1",
						"stroke-linecap": "round",
						style: { animation: "none !important" }
					}, null, 8, xn)) : x("", !0), e.value >= G.value.min && e.value <= G.value.max ? (E(), S("text", {
						key: 1,
						class: w({ "vue-data-ui-transition": A(Pa) }),
						transform: `translate(${J.value ? Y.value.right + V.value.chart.grid.labels.yAxis.crosshairSize + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + 5 : Y.value.scaleLabelX - V.value.chart.grid.labels.yAxis.crosshairSize}, ${A(m)(e.y + B.value.dataLabels / 3)})`,
						"font-size": B.value.dataLabels,
						"text-anchor": J.value ? "start" : "end",
						fill: V.value.chart.grid.labels.color
					}, gt(A(Ct)(e.value) ? A(_)(V.value.chart.grid.labels.yAxis.formatter, e.value, A(fe)({
						p: e.prefix,
						v: e.value,
						s: e.suffix,
						r: V.value.chart.grid.labels.yAxis.rounding
					})) : ""), 11, Sn)) : x("", !0)]))), 128))], 8, gn)) : x("", !0),
					Cu.value ? (E(), S("g", Cn, [(E(!0), S(y, null, O(wu.value, (e) => (E(), S("text", {
						class: w({ "vue-data-ui-transition": A(Pa) }),
						key: `crosshair_y_label_${e.serie.id}_${e.index}`,
						transform: `translate(${J.value ? Y.value.right + V.value.chart.grid.labels.yAxis.crosshairSize + V.value.chart.grid.labels.yAxis.scaleValueOffsetX + 5 : Y.value.scaleLabelX - V.value.chart.grid.labels.yAxis.crosshairSize}, ${A(v)(e.y) + B.value.dataLabels / 3})`,
						"font-size": B.value.dataLabels,
						"text-anchor": J.value ? "start" : "end",
						fill: V.value.chart.grid.labels.color,
						style: {
							transition: "none !important",
							animation: "none !important",
							"pointer-events": "none"
						}
					}, gt(A(_)(V.value.chart.grid.labels.yAxis.formatter, e.value, A(fe)({
						p: e.serie.prefix || V.value.chart.labels.prefix,
						v: e.value,
						s: e.serie.suffix || V.value.chart.labels.suffix,
						r: V.value.chart.grid.labels.yAxis.rounding
					}), {
						datapoint: e.serie,
						seriesIndex: e.index
					})), 11, wn))), 128))])) : x("", !0),
					Tu.value ? (E(), S("g", Tn, [C("circle", {
						cx: Su.value,
						cy: A(v)(Y.value?.bottom),
						r: V.value.chart.highlighter.crosshairs.dot.radius,
						fill: V.value.chart.highlighter.crosshairs.dot.fill,
						stroke: V.value.chart.highlighter.crosshairs.dot.stroke,
						"stroke-width": V.value.chart.highlighter.crosshairs.dot.strokeWidth,
						style: {
							transition: "none !important",
							animation: "none !important",
							"pointer-events": "none"
						}
					}, null, 8, En)])) : x("", !0),
					(E(!0), S(y, null, O($.value, (e, n) => (E(), S("g", {
						key: `serie_plot_${e.id}`,
						class: w(`serie_plot_${n}`),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [(E(!0), S(y, null, O(e.plots, (r, i) => (E(), S("g", { key: `circle_plot_${e.id}_${i}` }, [r && A(Ct)(r.value) ? (E(), nt(Je, {
						key: 0,
						shape: [
							"triangle",
							"square",
							"diamond",
							"pentagon",
							"hexagon",
							"star"
						].includes(e.shape) ? e.shape : "circle",
						color: V.value.plot.useGradient ? `url(#plotGradient_${n}_${F.value})` : V.value.plot.dot.useSerieColor ? e.color : V.value.plot.dot.fill,
						plot: {
							x: A(m)(r.x),
							y: A(m)(r.y)
						},
						radius: Du(e, r, i) ? (ka.value.plot || 6) * 1.5 : (nl(e.plots, i), ka.value.plot || 6),
						stroke: V.value.plot.dot.useSerieColor ? V.value.chart.backgroundColor : e.color,
						strokeWidth: V.value.plot.dot.strokeWidth,
						transition: A(Ba) || ls.value || !V.value.plot.showTransition ? void 0 : `all ${V.value.plot.transitionDurationMs}ms ease-in-out`,
						still: ls.value
					}, null, 8, [
						"shape",
						"color",
						"plot",
						"radius",
						"stroke",
						"strokeWidth",
						"transition",
						"still"
					])) : x("", !0), r.comment && V.value.chart.comments.show ? (E(), S("foreignObject", {
						key: 1,
						style: { overflow: "visible" },
						height: "12",
						width: V.value.chart.comments.width,
						x: r.x - V.value.chart.comments.width / 2 + V.value.chart.comments.offsetX,
						y: r.y + V.value.chart.comments.offsetY + 6
					}, [C("div", On, [k(t.$slots, "plot-comment", { plot: {
						...r,
						color: e.color,
						seriesIndex: n,
						datapointIndex: i
					} }, void 0, !0)])], 8, Dn)) : x("", !0)]))), 128))], 6))), 128)),
					(E(!0), S(y, null, O(Q.value, (e, t) => (E(), S("g", {
						key: `serie_line_${e.id}`,
						class: w(`serie_line_${t}`),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [e.hasDashedSegments ? (E(), S(y, { key: 0 }, [e.smooth ? (E(!0), S(y, { key: 0 }, O(e.dashedSmooth, (t, n) => (E(), S("path", {
						key: `line_coating_smooth_segment_${e.id}_${n}`,
						fill: "none",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						d: `M ${t.path}`,
						stroke: V.value.chart.backgroundColor,
						"stroke-width": V.value.line.strokeWidth + 1,
						"stroke-dasharray": t.dashed ? V.value.line.strokeWidth * 2 : 0,
						style: T({ transition: cl() })
					}, null, 12, kn))), 128)) : (E(!0), S(y, { key: 1 }, O(e.dashedStraight, (t, n) => (E(), S("path", {
						key: `line_coating_straight_segment_${e.id}_${n}`,
						fill: "none",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						d: `M ${t.path}`,
						stroke: V.value.chart.backgroundColor,
						"stroke-width": V.value.line.strokeWidth + 1,
						"stroke-dasharray": t.dashed ? V.value.line.strokeWidth * 2 : 0,
						style: T({ transition: cl() })
					}, null, 12, An))), 128))], 64)) : e.smooth && e.plots.length > 1 && e.curve ? (E(), S("path", {
						key: 1,
						d: `M${e.curve}`,
						stroke: V.value.chart.backgroundColor,
						"stroke-width": V.value.line.strokeWidth + 1,
						"stroke-dasharray": e.dashed ? V.value.line.strokeWidth * 2 : 0,
						fill: "none",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						style: T({ transition: cl() })
					}, null, 12, jn)) : e.plots.length > 1 && e.straight ? (E(), S("path", {
						key: 2,
						d: `M${e.straight}`,
						stroke: V.value.chart.backgroundColor,
						"stroke-width": V.value.line.strokeWidth + 1,
						"stroke-dasharray": e.dashed ? V.value.line.strokeWidth * 2 : 0,
						fill: "none",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						style: T({ transition: cl() })
					}, null, 12, Mn)) : x("", !0)], 6))), 128)),
					t.$slots.pattern ? (E(), S("defs", Nn, [(E(!0), S(y, null, O(xo.value, (e, n) => k(t.$slots, "pattern", lt({ key: `serie_pattern_slot_${e.id}` }, { ref_for: !0 }, {
						...e,
						seriesIndex: e.slotAbsoluteIndex,
						patternId: `pattern_${F.value}_${n}`
					}), void 0, !0)), 128))])) : x("", !0),
					jl.value.length && !H.value.isStacked ? (E(), S("g", Pn, [(E(!0), S(y, null, O(jl.value, (e) => (E(), S("path", {
						key: e.key,
						d: e.d,
						fill: e.color,
						"fill-opacity": V.value.line.interLine.fillOpacity,
						stroke: "none",
						"pointer-events": "none",
						style: T({ transition: cl() })
					}, null, 12, Fn))), 128))])) : x("", !0),
					(E(!0), S(y, null, O(Q.value, (e, n) => (E(), S("g", {
						key: `serie_line_above_${e.id}`,
						class: w(`serie_line_${n}`),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [
						e.useArea && e.plots.length > 1 ? (E(), S("g", In, [e.smooth ? (E(!0), S(y, { key: 0 }, O(e.curveAreas, (r, a) => (E(), S(y, { key: a }, [r ? (E(), S("path", {
							key: 0,
							d: r,
							fill: V.value.line.area.useGradient ? `url(#areaGradient_${n}_${F.value})` : A(i)(e.color, V.value.line.area.opacity),
							style: T({ transition: cl() })
						}, null, 12, Ln)) : x("", !0), t.$slots.pattern && r ? (E(), S("path", {
							key: 1,
							d: r,
							fill: `url(#pattern_${F.value}_${e.slotAbsoluteIndex})`,
							style: T({ transition: cl() })
						}, null, 12, Rn)) : x("", !0)], 64))), 128)) : (E(!0), S(y, { key: 1 }, O(e.area.split(";"), (r, a) => (E(), S(y, { key: a }, [r ? (E(), S("path", {
							key: 0,
							d: `M${r}Z`,
							fill: V.value.line.area.useGradient ? `url(#areaGradient_${n}_${F.value})` : A(i)(e.color, V.value.line.area.opacity),
							style: T({ transition: cl() })
						}, null, 12, zn)) : x("", !0), t.$slots.pattern && r ? (E(), S("path", {
							key: 1,
							d: `M${r}Z`,
							fill: `url(#pattern_${F.value}_${e.slotAbsoluteIndex})`,
							style: T({ transition: cl() })
						}, null, 12, Bn)) : x("", !0)], 64))), 128))])) : x("", !0),
						!e.hasDashedSegments && e.smooth && e.plots.length > 1 && e.curve ? (E(), S("path", {
							key: 1,
							d: `M${e.curve}`,
							stroke: e.temperatureColors && !e.isFlatTemperatureLine ? `url(#temperature_grad_line_${n}_${F.value})` : e.color,
							"stroke-width": V.value.line.strokeWidth,
							"stroke-dasharray": e.dashed ? V.value.line.strokeWidth * 2 : 0,
							fill: "none",
							"stroke-linecap": "round",
							style: T({ transition: cl() })
						}, null, 12, Vn)) : e.hasDashedSegments ? (E(), S(y, { key: 2 }, [e.smooth ? (E(!0), S(y, { key: 0 }, O(e.dashedSmooth, (t, r) => (E(), S("path", {
							key: `line_smooth_segment_${e.id}_${r}`,
							fill: "none",
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							d: `M ${t.path}`,
							stroke: e.temperatureColors && !e.isFlatTemperatureLine ? `url(#temperature_grad_line_${n}_${F.value})` : e.color,
							"stroke-width": V.value.line.strokeWidth,
							"stroke-dasharray": t.dashed ? V.value.line.strokeWidth * 2 : 0,
							style: T({ transition: cl() })
						}, null, 12, Hn))), 128)) : (E(!0), S(y, { key: 1 }, O(e.dashedStraight, (t, r) => (E(), S("path", {
							key: `line_straight_segment_${e.id}_${r}`,
							fill: "none",
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							d: `M ${t.path}`,
							stroke: e.temperatureColors && !e.isFlatTemperatureLine ? `url(#temperature_grad_line_${n}_${F.value})` : e.color,
							"stroke-width": V.value.line.strokeWidth,
							"stroke-dasharray": t.dashed ? V.value.line.strokeWidth * 2 : 0,
							style: T({ transition: cl() })
						}, null, 12, Un))), 128))], 64)) : e.plots.length > 1 && e.straight ? (E(), S("path", {
							key: 3,
							d: `M${e.straight}`,
							stroke: e.temperatureColors && !e.isFlatTemperatureLine ? `url(#temperature_grad_line_${n}_${F.value})` : e.color,
							"stroke-width": V.value.line.strokeWidth,
							"stroke-dasharray": e.dashed ? V.value.line.strokeWidth * 2 : 0,
							fill: "none",
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							style: T({ transition: cl() })
						}, null, 12, Wn)) : x("", !0),
						(E(!0), S(y, null, O(e.plots, (r, i) => (E(), S(y, { key: `circle_line_${e.id}_${i}` }, [!lc.value.linePlot && r && A(Ct)(r.value) || lc.value.linePlot && r && A(Ct)(r.value) && (R.value !== null && R.value === i || I.value !== null && I.value === i) || nl(e.plots, i) ? (E(), nt(Je, {
							key: 0,
							shape: [
								"triangle",
								"square",
								"diamond",
								"pentagon",
								"hexagon",
								"star"
							].includes(e.shape) ? e.shape : "circle",
							color: V.value.line.useGradient ? `url(#lineGradient_${n}_${F.value})` : V.value.line.dot.useSerieColor ? e.color : V.value.line.dot.fill,
							plot: {
								x: A(m)(r.x),
								y: A(m)(r.y)
							},
							radius: Du(e, r, i) ? Aa.value || 0 : (nl(e.plots, i), ka.value.line || 0),
							stroke: V.value.line.dot.useSerieColor ? V.value.chart.backgroundColor : e.color,
							strokeWidth: V.value.line.dot.strokeWidth,
							transition: A(Ba) || ls.value || !V.value.line.showTransition ? void 0 : `all ${V.value.line.transitionDurationMs}ms ease-in-out`,
							still: ls.value
						}, null, 8, [
							"shape",
							"color",
							"plot",
							"radius",
							"stroke",
							"strokeWidth",
							"transition",
							"still"
						])) : x("", !0), r.comment && V.value.chart.comments.show ? (E(), S("foreignObject", {
							key: 1,
							style: { overflow: "visible" },
							height: "12",
							width: V.value.chart.comments.width,
							x: r.x - V.value.chart.comments.width / 2 + V.value.chart.comments.offsetX,
							y: r.y + V.value.chart.comments.offsetY + 6
						}, [C("div", Kn, [k(t.$slots, "plot-comment", { plot: {
							...r,
							color: e.color,
							seriesIndex: n,
							datapointIndex: i
						} }, void 0, !0)])], 8, Gn)) : x("", !0)], 64))), 128))
					], 6))), 128)),
					(V.value.bar.labels.show || V.value.bar.serieName.show) && H.value.dataLabels.show ? (E(), S("g", qn, [(E(!0), S(y, null, O(Au.value, (e) => (E(), S("text", {
						key: e.key,
						class: w({ "vue-data-ui-transition": A(Pa) }),
						"text-anchor": xu({
							plot: e.plot,
							type: "bar"
						}),
						"font-size": B.value.plotLabels,
						transform: yu({ plot: e.plot }),
						fill: V.value.bar.labels.color,
						stroke: V.value.chart.backgroundColor,
						"paint-order": "stroke",
						style: T(`opacity:${N.value ? N.value === e.serie.groupId ? 1 : .2 : 1};`),
						innerHTML: vu({
							serie: e.serie,
							plot: e.plot,
							type: "bar"
						})
					}, null, 14, Jn))), 128)), (E(!0), S(y, null, O(Ol.value, (e, t) => (E(), S(y, { key: `xLabel_bar_${e.id}` }, [(E(!0), S(y, null, O(e.plots, (n, r) => (E(), S(y, { key: `xLabel_bar_${t}_${r}` }, [n && V.value.bar.serieName.show ? (E(), S("text", {
						key: 0,
						class: w({ "vue-data-ui-transition": A(Pa) }),
						transform: `translate(${H.value.useIndividualScale && H.value.isStacked ? n.x + Ps.value.line / 2 : n.x + Fs() * 1.1}, ${n.y + (n.value > 0 ? V.value.bar.serieName.offsetY : -V.value.bar.serieName.offsetY * 3)})`,
						"text-anchor": "middle",
						"font-size": B.value.plotLabels,
						fill: V.value.bar.serieName.useSerieColor ? e.color : V.value.bar.serieName.color,
						"font-weight": V.value.bar.serieName.bold ? "bold" : "normal",
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, gt(V.value.bar.serieName.useAbbreviation ? A(ve)({
						source: e.name,
						length: V.value.bar.serieName.abbreviationSize
					}) : e.name), 15, Yn)) : x("", !0)], 64))), 128))], 64))), 128))])) : x("", !0),
					V.value.plot.labels.show && H.value.dataLabels.show ? (E(), S("g", Xn, [(E(!0), S(y, null, O(ju.value, (e) => (E(), S("text", {
						key: e.key,
						class: w({ "vue-data-ui-transition": A(Pa) }),
						transform: bu({
							plot: e.plot,
							type: "plot"
						}),
						"text-anchor": xu({
							plot: e.plot,
							type: "plot"
						}),
						"font-size": B.value.plotLabels,
						fill: V.value.plot.labels.color,
						stroke: V.value.chart.backgroundColor,
						"paint-order": "stroke",
						style: T(`opacity:${N.value ? N.value === e.serie.groupId ? 1 : .2 : 1}`),
						innerHTML: vu({
							serie: e.serie,
							plot: e.plot,
							type: "plot"
						})
					}, null, 14, Zn))), 128))])) : (E(), S("g", Qn, [(E(!0), S(y, null, O($.value, (e, t) => (E(), S(y, { key: `xLabel_plot_${e.id}` }, [(E(!0), S(y, null, O(e.plots, (n, r) => (E(), S(y, { key: `xLabel_plot_${t}_${r}` }, [V.value.plot.tag.followValue ? (E(), S(y, { key: 1 }, [[
						I.value,
						R.value,
						qi.value
					].includes(r) && e.useTag ? (E(), S("line", {
						key: 0,
						class: "vue-ui-xy-tag-plot",
						x1: Y.value?.left,
						x2: Y.value?.right,
						y1: n.y,
						y2: n.y,
						"stroke-width": 1,
						"stroke-linecap": "round",
						"stroke-dasharray": "2",
						stroke: e.color
					}, null, 8, rr)) : x("", !0)], 64)) : (E(), S(y, { key: 0 }, [n && r === 0 && e.useTag && e.useTag === "start" ? (E(), S("foreignObject", {
						key: 0,
						x: n.x,
						y: n.y - 20,
						height: 24,
						width: "150",
						style: T(`overflow: visible; opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [C("div", {
						style: T(`padding: 3px; background:${A(i)(e.color, 80)};color:${A(Se)(e.color)};width:fit-content;font-size:${B.value.plotLabels}px;border-radius: 2px;`),
						innerHTML: A(_)(V.value.plot.tag.formatter, n.value, e.name, {
							datapoint: n,
							seriesIndex: r,
							serieName: e.name
						})
					}, null, 12, er)], 12, $n)) : x("", !0), n && r === e.plots.length - 1 && e.useTag && e.useTag === "end" ? (E(), S("foreignObject", {
						key: 1,
						x: n.x - e.name.length * (B.value.plotLabels / 2),
						y: n.y - 20,
						height: 24,
						width: "150",
						style: T(`overflow: visible; opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [C("div", {
						style: T(`padding: 3px; background:${A(i)(e.color, 80)};color:${A(Se)(e.color)};width:fit-content;font-size:${B.value.plotLabels}px;border-radius: 2px;`),
						innerHTML: A(_)(V.value.plot.tag.formatter, n.value, e.name, {
							datapoint: n,
							seriesIndex: r,
							serieName: e.name
						})
					}, null, 12, nr)], 12, tr)) : x("", !0)], 64))], 64))), 128))], 64))), 128))])),
					V.value.line.labels.show && H.value.dataLabels.show ? (E(), S("g", ir, [(E(!0), S(y, null, O(Mu.value, (e) => (E(), S("text", {
						key: e.key,
						class: w({ "vue-data-ui-transition": A(Pa) }),
						transform: bu({
							plot: e.plot,
							type: "line"
						}),
						"text-anchor": xu({
							plot: e.plot,
							type: "line"
						}),
						"font-size": B.value.plotLabels,
						fill: V.value.line.labels.color,
						stroke: V.value.chart.backgroundColor,
						"paint-order": "stroke",
						style: T(`opacity:${N.value ? N.value === e.serie.groupId ? 1 : .2 : 1};`),
						innerHTML: vu({
							serie: e.serie,
							plot: e.plot,
							type: "line"
						})
					}, null, 14, ar))), 128))])) : (E(), S("g", or, [(E(!0), S(y, null, O(Q.value, (e, t) => (E(), S(y, { key: `xLabel_line_${e.id}` }, [(E(!0), S(y, null, O(e.plots, (n, r) => (E(), S(y, { key: `xLabel_line_${t}_${r}` }, [V.value.line.tag.followValue ? (E(), S(y, { key: 1 }, [[
						I.value,
						R.value,
						qi.value
					].includes(r) && e.useTag ? (E(), S("line", {
						key: 0,
						class: "vue-ui-xy-tag-line",
						x1: Y.value?.left,
						x2: Y.value?.right,
						y1: n.y,
						y2: n.y,
						"stroke-width": 1,
						"stroke-linecap": "round",
						"stroke-dasharray": "2",
						stroke: e.color
					}, null, 8, dr)) : x("", !0)], 64)) : (E(), S(y, { key: 0 }, [n && r === 0 && e.useTag && e.useTag === "start" ? (E(), S("foreignObject", {
						key: 0,
						x: n.x,
						y: n.y - 20,
						height: 24,
						width: "150",
						style: T(`overflow: visible; opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [C("div", {
						style: T(`padding: 3px; background:${A(i)(e.color, 80)};color:${A(Se)(e.color)};width:fit-content;font-size:${B.value.plotLabels}px;border-radius: 2px;`),
						innerHTML: A(_)(V.value.line.tag.formatter, n.value, e.name, {
							datapoint: n,
							seriesIndex: r,
							serieName: e.name
						})
					}, null, 12, cr)], 12, sr)) : x("", !0), n && r === e.plots.length - 1 && e.useTag && e.useTag === "end" ? (E(), S("foreignObject", {
						key: 1,
						x: n.x,
						y: n.y - 20,
						height: 24,
						width: "150",
						style: T(`overflow: visible; opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, [C("div", {
						style: T(`padding: 3px; background:${A(i)(e.color, 80)};color:${A(Se)(e.color)};width:fit-content;font-size:${B.value.plotLabels}px;border-radius: 2px;`),
						innerHTML: A(_)(V.value.line.tag.formatter, n.value, e.name, {
							datapoint: n,
							seriesIndex: r,
							serieName: e.name
						})
					}, null, 12, ur)], 12, lr)) : x("", !0)], 64))], 64))), 128))], 64))), 128))])),
					(E(!0), S(y, null, O(Q.value, (e, t) => (E(), S(y, { key: `xLabel_line_${e.id}` }, [(E(!0), S(y, null, O(e.plots, (n, r) => (E(), S(y, { key: `xLabel_line_${t}_${r}` }, [n && r === 0 && e.showSerieName && e.showSerieName === "start" ? (E(), S("text", {
						key: 0,
						x: n.x - B.value.plotLabels,
						y: n.y,
						"font-size": B.value.plotLabels,
						"text-anchor": "end",
						fill: e.color,
						innerHTML: A(de)({
							content: e.name,
							fontSize: B.value.plotLabels,
							fill: e.color,
							x: n.x - B.value.plotLabels,
							y: n.y,
							maxWords: 2
						}),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, null, 12, fr)) : x("", !0), n && r === e.plots.length - 1 && e.showSerieName && e.showSerieName === "end" ? (E(), S("text", {
						key: 1,
						x: n.x + B.value.plotLabels,
						y: n.y,
						"font-size": B.value.plotLabels,
						"text-anchor": "start",
						fill: e.color,
						innerHTML: A(de)({
							content: e.name,
							fontSize: B.value.plotLabels,
							fill: e.color,
							x: n.x + B.value.plotLabels,
							y: n.y,
							maxWords: 2
						}),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, null, 12, pr)) : x("", !0)], 64))), 128))], 64))), 128)),
					(E(!0), S(y, null, O($.value, (e, t) => (E(), S(y, { key: `xLabel_plot_${e.id}` }, [(E(!0), S(y, null, O(e.plots, (n, r) => (E(), S(y, { key: `xLabel_plot_${t}_${r}` }, [n && r === 0 && e.showSerieName && e.showSerieName === "start" ? (E(), S("text", {
						key: 0,
						x: n.x - B.value.plotLabels,
						y: n.y,
						"font-size": B.value.plotLabels,
						"text-anchor": "end",
						fill: e.color,
						innerHTML: A(de)({
							content: e.name,
							fontSize: B.value.plotLabels,
							fill: e.color,
							x: n.x - B.value.plotLabels,
							y: n.y,
							maxWords: 2
						}),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, null, 12, mr)) : x("", !0), n && r === e.plots.length - 1 && e.showSerieName && e.showSerieName === "end" ? (E(), S("text", {
						key: 1,
						x: n.x + B.value.plotLabels,
						y: n.y,
						"font-size": B.value.plotLabels,
						"text-anchor": "start",
						fill: e.color,
						innerHTML: A(de)({
							content: e.name,
							fontSize: B.value.plotLabels,
							fill: e.color,
							x: n.x + B.value.plotLabels,
							y: n.y,
							maxWords: 2
						}),
						style: T(`opacity:${N.value ? N.value === e.groupId ? 1 : .2 : 1};transition:opacity 0.2s ease-in-out`)
					}, null, 12, hr)) : x("", !0)], 64))), 128))], 64))), 128)),
					(E(!0), S(y, null, O([
						...$.value,
						...Q.value,
						...Ol.value
					], (e, t) => (E(), S(y, { key: `progression-${t}` }, [Object.hasOwn(e, "useProgression") && e.useProgression === !0 && !isNaN(A(g)(e.plots).trend) ? (E(), S("g", gr, [
						C("defs", null, [C("marker", {
							id: `progression_arrow_${t}`,
							markerWidth: "9",
							markerHeight: "9",
							viewBox: "-1 -1 9 9",
							markerUnits: "userSpaceOnUse",
							refX: "7",
							refY: 7 / 2,
							orient: "auto",
							overflow: "visible"
						}, [C("polygon", {
							points: "0,0 7,3.5 0,7",
							fill: e.color,
							stroke: V.value.chart.backgroundColor,
							"stroke-width": "1",
							"stroke-linejoin": "round"
						}, null, 8, vr)], 8, _r)]),
						e.plots.length > 1 ? (E(), S("line", {
							key: 0,
							x1: A(g)(e.plots).x1 + (e.type === "bar" ? Fs() : 0),
							x2: A(g)(e.plots).x2 + (e.type === "bar" ? Fs() : 0),
							y1: A(v)(A(g)(e.plots).y1),
							y2: A(v)(A(g)(e.plots).y2),
							"stroke-width": 1,
							stroke: V.value.chart.backgroundColor,
							"stroke-dasharray": 2,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							"marker-end": `url(#progression_arrow_${t})`
						}, null, 8, yr)) : x("", !0),
						e.plots.length > 1 ? (E(), S("line", {
							key: 1,
							x1: A(g)(e.plots).x1 + (e.type === "bar" ? Fs() : 0),
							x2: A(g)(e.plots).x2 + (e.type === "bar" ? Fs() : 0),
							y1: A(v)(A(g)(e.plots).y1),
							y2: A(v)(A(g)(e.plots).y2),
							"stroke-width": 1,
							stroke: e.color,
							"stroke-dasharray": 2,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							"marker-end": `url(#progression_arrow_${t})`
						}, null, 8, br)) : x("", !0),
						e.plots.length > 1 ? (E(), S("text", {
							key: 2,
							class: w({ "vue-data-ui-transition": A(Pa) }),
							"text-anchor": "middle",
							transform: `translate(${A(g)(e.plots).x2 + (e.type === "bar" ? Fs() : 0)}, ${A(g)(e.plots).y2 - 12})`,
							"font-size": B.value.plotLabels,
							fill: e.color,
							stroke: V.value.chart.backgroundColor,
							"stroke-width": 4,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							"paint-order": "stroke fill"
						}, gt(A(fe)({
							v: A(g)(e.plots).trend * 100,
							s: "%",
							r: 2
						})), 11, xr)) : x("", !0)
					])) : x("", !0)], 64))), 128)),
					H.value.useIndividualScale && !H.value.isStacked ? (E(), S(y, { key: 16 }, [C("defs", null, [(E(!0), S(y, null, O(Al.value, (e, t) => (E(), nt(Xe, {
						t: "linear",
						key: `individual_scale_gradient_${e.groupId || e.id || t}`,
						id: `individual_scale_gradient_${F.value}_${t}`,
						x1: J.value ? "100%" : "0%",
						x2: J.value ? "0%" : "100%",
						y1: "0%",
						y2: "0%",
						stops: [[
							"0%",
							V.value.chart.backgroundColor,
							0
						], [
							"100%",
							e.color,
							.2
						]]
					}, null, 8, [
						"id",
						"x1",
						"x2",
						"stops"
					]))), 128))]), (E(!0), S(y, null, O(Al.value, (e, t) => (E(), S("rect", {
						key: `individual_scale_background_${e.groupId || e.id || t}`,
						x: J.value ? e.x : e.x - V.value.chart.grid.labels.yAxis.labelWidth - Y.value.individualOffsetX,
						y: Y.value?.top,
						width: (J.value, V.value.chart.grid.labels.yAxis.labelWidth + Y.value.individualOffsetX),
						height: Y.value.height < 0 ? 10 : Y.value.height,
						fill: N.value === e.groupId ? `url(#individual_scale_gradient_${F.value}_${t})` : "transparent",
						onMouseenter: (t) => N.value = e.groupId,
						onMouseleave: n[0] ||= (e) => N.value = null
					}, null, 40, Sr))), 128))], 64)) : x("", !0),
					C("g", null, [V.value.chart.grid.labels.axis.yLabel && !H.value.useIndividualScale ? (E(), S("text", {
						key: 0,
						ref_key: "yAxisLabel",
						ref: Mi,
						"font-size": B.value.yAxis,
						fill: V.value.chart.grid.labels.color,
						transform: `translate(${J.value ? Y.value.right + Y.value.scaleLabelsOffset + V.value.chart.grid.labels.axis.yLabelOffsetX + B.value.yAxis : V.value.chart.grid.labels.axis.fontSize + V.value.chart.grid.labels.axis.yLabelOffsetX}, ${Y.value?.top + Y.value.height / 2}) rotate(-90)`,
						"text-anchor": "middle",
						style: { transition: "none" }
					}, gt(V.value.chart.grid.labels.axis.yLabel), 9, Cr)) : x("", !0), V.value.chart.grid.labels.axis.xLabel ? (E(), S("text", {
						key: 1,
						ref_key: "xAxisLabel",
						ref: ji,
						"text-anchor": "middle",
						x: P.value / 2,
						y: Bi.value - 3,
						"font-size": B.value.yAxis,
						fill: V.value.chart.grid.labels.color
					}, gt(V.value.chart.grid.labels.axis.xLabel), 9, wr)) : x("", !0)]),
					V.value.chart.grid.labels.xAxisLabels.show ? (E(), S("g", {
						key: 17,
						ref_key: "timeLabelsEls",
						ref: Ni,
						opacity: Cu.value ? .1 : 1,
						style: { transition: "opacity 0.2s" }
					}, [t.$slots["time-label"] ? (E(!0), S(y, { key: 0 }, O(dl.value, (e, n) => k(t.$slots, "time-label", lt({ ref_for: !0 }, {
						x: fl(e, n),
						y: Y.value?.bottom,
						fontSize: B.value.xAxis,
						fill: V.value.chart.grid.labels.xAxisLabels.color,
						transform: `translate(${fl(e, n)}, ${Y.value?.bottom + B.value.xAxis * 1.3 + V.value.chart.grid.labels.xAxisLabels.yOffset}), rotate(${V.value.chart.grid.labels.xAxisLabels.rotation})`,
						absoluteIndex: e.absoluteIndex,
						content: e.text,
						textAnchor: V.value.chart.grid.labels.xAxisLabels.rotation > 0 ? "start" : V.value.chart.grid.labels.xAxisLabels.rotation < 0 ? "end" : "middle",
						show: e && e.text
					}), void 0, !0, `time_label_${e.id}`)), 128)) : (E(!0), S(y, { key: 1 }, O(dl.value, (e, t) => (E(), S("g", { key: `time_label_${t}` }, [e && e.text ? (E(), S(y, { key: 0 }, [String(e.text).includes("\n") ? (E(), S("text", {
						key: 1,
						class: "vue-data-ui-time-label",
						"text-anchor": V.value.chart.grid.labels.xAxisLabels.rotation > 0 ? "start" : V.value.chart.grid.labels.xAxisLabels.rotation < 0 ? "end" : "middle",
						"font-size": B.value.xAxis,
						fill: V.value.chart.grid.labels.xAxisLabels.color,
						transform: `translate(${fl(e, t)}, ${Y.value?.bottom + B.value.xAxis * 1.5}), rotate(${V.value.chart.grid.labels.xAxisLabels.rotation})`,
						style: T({ cursor: Fo() && Fa.value ? "pointer" : "default" }),
						innerHTML: A(c)({
							content: String(e.text),
							fontSize: B.value.xAxis,
							fill: V.value.chart.grid.labels.xAxisLabels.color,
							x: 0,
							y: 0
						}),
						onClick: (n) => Xo(e, t)
					}, null, 12, Dr)) : (E(), S("text", {
						key: 0,
						class: "vue-data-ui-time-label",
						"text-anchor": V.value.chart.grid.labels.xAxisLabels.rotation > 0 ? "start" : V.value.chart.grid.labels.xAxisLabels.rotation < 0 ? "end" : "middle",
						"font-size": B.value.xAxis,
						fill: V.value.chart.grid.labels.xAxisLabels.color,
						transform: `translate(${fl(e, t)}, ${Y.value?.bottom + B.value.xAxis * 1.5}), rotate(${V.value.chart.grid.labels.xAxisLabels.rotation})`,
						style: T({ cursor: Fo() && Fa.value ? "pointer" : "default" }),
						onClick: (n) => Xo(e, t)
					}, gt(e.text || ""), 13, Er))], 64)) : x("", !0)]))), 128))], 8, Tr)) : x("", !0),
					Tu.value ? (E(), S("g", Or, [C("text", {
						class: "vue-data-ui-time-label",
						"text-anchor": V.value.chart.grid.labels.xAxisLabels.rotation > 0 ? "start" : V.value.chart.grid.labels.xAxisLabels.rotation < 0 ? "end" : "middle",
						"font-size": B.value.xAxis,
						fill: V.value.chart.grid.labels.xAxisLabels.color,
						transform: `translate(${Tu.value.x}, ${Y.value?.bottom + B.value.xAxis * 1.5}), rotate(${V.value.chart.grid.labels.xAxisLabels.rotation})`,
						"font-weight": "bold",
						style: {
							transition: "none !important",
							animation: "none !important",
							"pointer-events": "none"
						},
						innerHTML: A(c)({
							content: String(Tu.value.text),
							fontSize: B.value.xAxis,
							fill: V.value.chart.grid.labels.xAxisLabels.color,
							x: 0,
							y: 0
						})
					}, null, 8, kr)])) : x("", !0),
					tl.value.length && !H.value.isStacked ? (E(), S("g", Ar, [(E(!0), S(y, null, O(tl.value, (e) => (E(), S("g", { key: e.id }, [
						e.yTop && e.show && isFinite(e.yTop) ? (E(), S("line", {
							key: 0,
							x1: e.x1,
							y1: e.yTop,
							x2: e.x2,
							y2: e.yTop,
							stroke: e.config.line.stroke,
							"stroke-width": e.config.line.strokeWidth,
							"stroke-dasharray": e.config.line.strokeDasharray,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, jr)) : x("", !0),
						e.yBottom && e.show && isFinite(e.yBottom) ? (E(), S("line", {
							key: 1,
							x1: e.x1,
							y1: e.yBottom,
							x2: e.x2,
							y2: e.yBottom,
							stroke: e.config.line.stroke,
							"stroke-width": e.config.line.strokeWidth,
							"stroke-dasharray": e.config.line.strokeDasharray,
							"stroke-linecap": "round",
							style: { animation: "none !important" }
						}, null, 8, Mr)) : x("", !0),
						e.hasArea && e.show && isFinite(e.yTop) && isFinite(e.yBottom) ? (E(), S("rect", {
							key: 2,
							y: Math.min(e.yTop, e.yBottom),
							x: e.x1,
							width: Y.value.width,
							height: A(m)(e.areaHeight, 0),
							fill: A(i)(e.config.area.fill, e.config.area.opacity),
							style: { animation: "none !important" }
						}, null, 8, Nr)) : x("", !0),
						e.config.label.text && e.show && isFinite(e._box.y) ? (E(), S("rect", lt({
							key: 3,
							class: "vue-ui-xy-annotation-label-box"
						}, { ref_for: !0 }, e._box, { style: {
							animation: "none !important",
							transition: "none !important"
						} }), null, 16)) : x("", !0),
						e.config.label.text && e.show && isFinite(e._text.y) ? (E(), S("text", {
							key: 4,
							id: e.id,
							class: w(["vue-ui-xy-annotation-label", { "vue-data-ui-transition": A(Pa) }]),
							transform: `translate(${e._text.x}, ${e._text.y})`,
							"font-size": e.config.label.fontSize,
							fill: e.config.label.color,
							"text-anchor": e.config.label.textAnchor
						}, gt(e.config.label.text), 11, Pr)) : x("", !0)
					]))), 128))])) : x("", !0),
					!Cu.value && V.value.chart.timeTag.show && (K.value ? A(a)(du.value) : ![null, void 0].includes(R.value) || ![null, void 0].includes(I.value)) ? (E(), S("g", Fr, [(E(), S("foreignObject", {
						x: pu(),
						y: Y.value?.bottom,
						width: "200",
						height: "40",
						style: { overflow: "visible !important" }
					}, [C("div", {
						ref_key: "timeTagEl",
						ref: cu,
						class: "vue-ui-xy-time-tag",
						style: T(`width: fit-content;margin: 0 auto;text-align:center;padding:3px 12px;background:${V.value.chart.timeTag.backgroundColor};color:${V.value.chart.timeTag.color};font-size:${V.value.chart.timeTag.fontSize}px`),
						innerHTML: _u.value
					}, null, 12, Lr)], 8, Ir)), C("circle", {
						cx: K.value && A(a)(du.value) ? pl({ x: du.value }) : _c((R.value === null ? 0 : R.value) || (I.value === null ? 0 : I.value)),
						cy: Y.value?.bottom,
						r: V.value.chart.timeTag.circleMarker.radius,
						fill: V.value.chart.timeTag.circleMarker.color
					}, null, 8, Rr)])) : x("", !0)
				])) : x("", !0),
				Ya.value ? (E(), S("rect", lt({ key: 2 }, eo.value, {
					"data-start": U.value.start,
					"data-end": U.value.end
				}), null, 16, zr)) : x("", !0),
				k(t.$slots, "svg", { svg: {
					...Ta.value,
					slicer: U.value,
					isPrintingImg: A(to) || A(no) || A(Uu),
					isPrintingSvg: A(Wu),
					data: [
						...Q.value,
						...Ol.value,
						...$.value
					],
					drawingArea: Y.value
				} }, void 0, !0)
			], 512)], 46, Lt)), t.$slots.hint ? (E(), S("div", Br, [k(t.$slots, "hint", dt(ct({
				hint: V.value.a11y.translations.keyboardNavigation,
				isVisible: Ju.value
			})), void 0, !0)])) : x("", !0)]),
			t.$slots.watermark ? (E(), S("div", Vr, [k(t.$slots, "watermark", dt(ct({ isPrinting: A(to) || A(no) || A(Uu) || A(Wu) })), void 0, !0)])) : x("", !0),
			(E(!0), S(y, null, O(Q.value, (e, t) => (E(), S(y, { key: `tag_line_${e.id}` }, [(E(!0), S(y, null, O(e.plots, (n, r) => (E(), S(y, { key: `tag_line_${t}_${r}` }, [[
				I.value,
				R.value,
				qi.value
			].includes(r) && e.useTag && e.useTag === "end" && V.value.line.tag.followValue ? (E(), S("div", {
				key: 0,
				ref_for: !0,
				ref: (e) => Ro(t, r, e, "right", "line"),
				class: "vue-ui-xy-tag",
				"data-tag": "right",
				style: T({
					position: "fixed",
					top: A(f)({
						svgElement: L.value,
						x: Y.value?.right + V.value.line.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_right_line`],
						position: "right"
					})?.top + "px",
					left: A(f)({
						svgElement: L.value,
						x: Y.value?.right + V.value.line.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_right_line`],
						position: "right"
					})?.left + "px",
					height: "fit-content",
					width: "fit-content",
					background: e.color,
					color: A(Se)(e.color),
					padding: "0 6px",
					fontSize: V.value.line.tag.fontSize + "px",
					opacity: 1
				})
			}, [(E(), S("svg", Hr, [C("path", {
				d: "M 0,10 10,0 10,20 Z",
				fill: e.color,
				stroke: "none"
			}, null, 8, Ur)])), C("div", {
				class: "vue-ui-xy-tag-content",
				innerHTML: A(_)(V.value.line.tag.formatter, n.value, e.name, {
					datapoint: n,
					seriesIndex: r,
					serieName: e.name
				})
			}, null, 8, Wr)], 4)) : x("", !0), [
				I.value,
				R.value,
				qi.value
			].includes(r) && e.useTag && e.useTag === "start" && V.value.line.tag.followValue ? (E(), S("div", {
				key: 1,
				ref_for: !0,
				ref: (e) => Ro(t, r, e, "left", "line"),
				class: "vue-ui-xy-tag",
				"data-tag": "left",
				style: T({
					position: "fixed",
					top: A(f)({
						svgElement: L.value,
						x: Y.value?.left - V.value.line.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_left_line`],
						position: "left"
					})?.top + "px",
					left: A(f)({
						svgElement: L.value,
						x: Y.value?.left - V.value.line.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_left_line`],
						position: "left"
					})?.left + "px",
					height: "fit-content",
					width: "fit-content",
					background: e.color,
					color: A(Se)(e.color),
					padding: "0 6px",
					fontSize: V.value.line.tag.fontSize + "px",
					opacity: 1
				})
			}, [(E(), S("svg", Gr, [C("path", {
				d: "M 0,0 10,10 0,20 Z",
				fill: e.color,
				stroke: "none"
			}, null, 8, Kr)])), C("div", {
				class: "vue-ui-xy-tag-content",
				innerHTML: A(_)(V.value.line.tag.formatter, n.value, e.name, {
					datapoint: n,
					seriesIndex: r,
					serieName: e.name
				})
			}, null, 8, qr)], 4)) : x("", !0)], 64))), 128))], 64))), 128)),
			(E(!0), S(y, null, O($.value, (e, t) => (E(), S(y, { key: `tag_plot_${e.id}` }, [(E(!0), S(y, null, O(e.plots, (n, r) => (E(), S(y, { key: `tag_plot_${t}_${r}` }, [[
				I.value,
				R.value,
				qi.value
			].includes(r) && e.useTag && e.useTag === "end" && V.value.plot.tag.followValue ? (E(), S("div", {
				key: 0,
				ref_for: !0,
				ref: (e) => Ro(t, r, e, "right", "plot"),
				class: "vue-ui-xy-tag",
				"data-tag": "right",
				style: T({
					position: "fixed",
					top: A(f)({
						svgElement: L.value,
						x: Y.value?.right + V.value.plot.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_right_plot`],
						position: "right"
					})?.top + "px",
					left: A(f)({
						svgElement: L.value,
						x: Y.value?.right + V.value.plot.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_right_plot`],
						position: "right"
					})?.left + "px",
					height: "fit-content",
					width: "fit-content",
					background: e.color,
					color: A(Se)(e.color),
					padding: "0 6px",
					fontSize: V.value.plot.tag.fontSize + "px",
					opacity: 1
				})
			}, [(E(), S("svg", Jr, [C("path", {
				d: "M 0,10 10,0 10,20 Z",
				fill: e.color,
				stroke: "none"
			}, null, 8, Yr)])), C("div", {
				class: "vue-ui-xy-tag-content",
				innerHTML: A(_)(V.value.plot.tag.formatter, n.value, e.name, {
					datapoint: n,
					seriesIndex: r,
					serieName: e.name
				})
			}, null, 8, Xr)], 4)) : x("", !0), [
				I.value,
				R.value,
				qi.value
			].includes(r) && e.useTag && e.useTag === "start" && V.value.plot.tag.followValue ? (E(), S("div", {
				key: 1,
				ref_for: !0,
				ref: (e) => Ro(t, r, e, "left", "plot"),
				class: "vue-ui-xy-tag",
				"data-tag": "left",
				style: T({
					position: "fixed",
					top: A(f)({
						svgElement: L.value,
						x: Y.value?.left - V.value.plot.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_left_plot`],
						position: "left"
					})?.top + "px",
					left: A(f)({
						svgElement: L.value,
						x: Y.value?.left - V.value.plot.tag.fontSize / 1.5,
						y: n.y,
						element: ia.value[`${t}_${r}_left_plot`],
						position: "left"
					})?.left + "px",
					height: "fit-content",
					width: "fit-content",
					background: e.color,
					color: A(Se)(e.color),
					padding: "0 6px",
					fontSize: V.value.plot.tag.fontSize + "px",
					opacity: 1
				})
			}, [(E(), S("svg", Zr, [C("path", {
				d: "M 0,0 10,10 0,20 Z",
				fill: e.color,
				stroke: "none"
			}, null, 8, Qr)])), C("div", {
				class: "vue-ui-xy-tag-content",
				innerHTML: A(_)(V.value.plot.tag.formatter, n.value, e.name, {
					datapoint: n,
					seriesIndex: r,
					serieName: e.name
				})
			}, null, 8, $r)], 4)) : x("", !0)], 64))), 128))], 64))), 128)),
			V.value.chart.zoom.show && Ja.value > 6 && Na.value && is.value ? (E(), nt(Ze, {
				key: 6,
				ref_key: "chartSlicer",
				ref: Ei,
				uuid: F.value,
				allMinimaps: Uc.value,
				background: V.value.chart.zoom.color,
				borderColor: V.value.chart.backgroundColor,
				customFormat: V.value.chart.zoom.customFormat,
				cutNullValues: V.value.line.cutNullValues,
				enableRangeHandles: V.value.chart.zoom.enableRangeHandles,
				enableSelectionDrag: V.value.chart.zoom.enableSelectionDrag,
				end: qc.value,
				focusOnDrag: V.value.chart.zoom.focusOnDrag,
				focusRangeRatio: V.value.chart.zoom.focusRangeRatio,
				fontSize: V.value.chart.zoom.fontSize,
				useResetSlot: V.value.chart.zoom.useResetSlot,
				immediate: !V.value.chart.zoom.preview.enable,
				inputColor: V.value.chart.zoom.color,
				isPreview: Ya.value,
				labelLeft: Zc.value,
				labelRight: Qc.value,
				max: Gc.value,
				min: Wc.value,
				precision: K.value ? V.value.chart.grid.labels.xAxis.rounding : 0,
				useValueRange: K.value,
				minimap: Vc.value,
				minimapCompact: V.value.chart.zoom.minimap.compact,
				minimapFrameColor: V.value.chart.zoom.minimap.frameColor,
				minimapIndicatorColor: V.value.chart.zoom.minimap.indicatorColor,
				minimapLineColor: V.value.chart.zoom.minimap.lineColor,
				minimapMerged: V.value.chart.zoom.minimap.merged,
				minimapSelectedColor: V.value.chart.zoom.minimap.selectedColor,
				minimapSelectedColorOpacity: V.value.chart.zoom.minimap.selectedColorOpacity,
				minimapSelectedIndex: K.value ? Jc.value : R.value ?? I.value,
				minimapSelectionRadius: V.value.chart.zoom.minimap.selectionRadius,
				preciseLabels: Il.value.length ? Il.value : Wo.value,
				refreshStartPoint: K.value ? Wc.value : V.value.chart.zoom.startIndex === null ? 0 : V.value.chart.zoom.startIndex,
				refreshEndPoint: K.value ? Gc.value : V.value.chart.zoom.endIndex === null ? Math.max(...e.dataset.map((e) => Wa(e.series).length)) : V.value.chart.zoom.endIndex + 1,
				selectColor: V.value.chart.zoom.highlightColor,
				selectedSeries: $c.value,
				smoothMinimap: V.value.chart.zoom.minimap.smooth,
				start: Kc.value,
				textColor: V.value.chart.color,
				timeLabels: Wo.value,
				usePreciseLabels: V.value.chart.grid.labels.xAxisLabels.datetimeFormatter.enable && !V.value.chart.zoom.useDefaultFormat,
				valueEnd: qc.value,
				valueStart: Kc.value,
				verticalHandles: V.value.chart.zoom.minimap.verticalHandles,
				minScale: Yc.value,
				maxScale: Xc.value,
				maxWidth: V.value.chart.zoom.maxWidth,
				minimapLeftInsetRatio: P.value > 0 && V.value.chart.zoom.autoFit ? Y.value.left / P.value : null,
				minimapRightInsetRatio: P.value > 0 && V.value.chart.zoom.autoFit ? (P.value - Y.value.right) / P.value : null,
				additionalMinimapHeight: V.value.chart.zoom.minimap.additionalHeight,
				handleType: V.value.chart.zoom.minimap.handleType,
				handleIconColor: V.value.chart.zoom.minimap.handleIconColor,
				handleBorderWidth: V.value.chart.zoom.minimap.handleBorderWidth,
				handleBorderColor: V.value.chart.zoom.minimap.handleBorderColor,
				handleFill: V.value.chart.zoom.minimap.handleFill,
				handleWidth: V.value.chart.zoom.minimap.handleWidth,
				isCursorPointer: Fa.value,
				onFutureEnd: n[1] ||= (e) => Qa("end", e),
				onFutureStart: n[2] ||= (e) => Qa("start", e),
				onReset: bs,
				onTrapMouse: Zo,
				onTrapMouseValue: Qo,
				"onUpdate:end": _s,
				"onUpdate:start": gs
			}, {
				"reset-action": j(({ reset: e }) => [k(t.$slots, "reset-action", dt(ct({ reset: e })), void 0, !0)]),
				_: 3
			}, 8, /* @__PURE__ */ "uuid.allMinimaps.background.borderColor.customFormat.cutNullValues.enableRangeHandles.enableSelectionDrag.end.focusOnDrag.focusRangeRatio.fontSize.useResetSlot.immediate.inputColor.isPreview.labelLeft.labelRight.max.min.precision.useValueRange.minimap.minimapCompact.minimapFrameColor.minimapIndicatorColor.minimapLineColor.minimapMerged.minimapSelectedColor.minimapSelectedColorOpacity.minimapSelectedIndex.minimapSelectionRadius.preciseLabels.refreshStartPoint.refreshEndPoint.selectColor.selectedSeries.smoothMinimap.start.textColor.timeLabels.usePreciseLabels.valueEnd.valueStart.verticalHandles.minScale.maxScale.maxWidth.minimapLeftInsetRatio.minimapRightInsetRatio.additionalMinimapHeight.handleType.handleIconColor.handleBorderWidth.handleBorderColor.handleFill.handleWidth.isCursorPointer".split("."))) : x("", !0),
			C("div", { id: `legend-bottom-${F.value}` }, null, 8, ei),
			oa.value && (V.value.chart.legend.show || t.$slots.legend) ? (E(), nt(tt, {
				key: 7,
				to: V.value.chart.legend.position === "top" ? `#legend-top-${F.value}` : `#legend-bottom-${F.value}`
			}, [C("div", {
				ref_key: "chartLegend",
				ref: Di
			}, [k(t.$slots, "legend", { legend: So.value }, () => [V.value.chart.legend.show ? (E(), S("div", {
				key: 0,
				class: "vue-ui-xy-legend",
				style: T({ fontSize: `var(--legend-font-size, ${V.value.chart.legend.fontSize ?? 14}px)` })
			}, [V.value.chart.legend.selectAllToggle.show && So.value.length > 2 && !A(Ba) ? (E(), nt($e, {
				key: 0,
				backgroundColor: V.value.chart.legend.selectAllToggle.backgroundColor,
				color: V.value.chart.legend.selectAllToggle.color,
				fontSize: V.value.chart.legend.fontSize,
				checked: Ji.value.length > 0,
				onToggle: tc
			}, null, 8, [
				"backgroundColor",
				"color",
				"fontSize",
				"checked"
			])) : x("", !0), (E(!0), S(y, null, O(So.value, (e, n) => (E(), S("div", {
				key: `div_legend_item_${n}`,
				onClick: (t) => ic(e),
				onKeydown: (t) => nc(t, e),
				role: "button",
				tabindex: "0",
				class: w({
					"vue-ui-xy-legend-item-alone": So.value.length === 1,
					"vue-ui-xy-legend-item": !0,
					"vue-ui-xy-legend-item-segregated": Yi.value.has(e.id)
				}),
				style: T({ cursor: Fa.value ? "pointer" : "default" })
			}, [Ui.value[e.type] === "line" ? (E(), S("svg", ni, [C("rect", {
				x: "0",
				y: "7.5",
				rx: "1.5",
				stroke: V.value.chart.backgroundColor,
				"stroke-width": .5,
				height: "3",
				width: "20",
				fill: e.color
			}, null, 8, ri), at(Je, {
				plot: {
					x: 10,
					y: 9
				},
				radius: 4,
				color: e.color,
				shape: [
					"triangle",
					"square",
					"diamond",
					"pentagon",
					"hexagon",
					"star"
				].includes(e.shape) ? e.shape : "circle",
				stroke: V.value.chart.backgroundColor,
				strokeWidth: .5
			}, null, 8, [
				"color",
				"shape",
				"stroke"
			])])) : Ui.value[e.type] === "bar" ? (E(), S("svg", ii, [Ui.value[e.type] === "bar" && t.$slots.pattern ? (E(), S("rect", {
				key: 0,
				x: "4",
				y: "4",
				rx: "1",
				height: "32",
				width: "32",
				stroke: "none",
				fill: e.color
			}, null, 8, ai)) : x("", !0), Ui.value[e.type] === "bar" ? (E(), S("rect", {
				key: 1,
				x: "4",
				y: "4",
				rx: "1",
				height: "32",
				width: "32",
				stroke: "none",
				fill: t.$slots.pattern ? `url(#pattern_${F.value}_${e.slotAbsoluteIndex})` : e.color
			}, null, 8, oi)) : x("", !0)])) : (E(), S("svg", si, [at(Je, {
				plot: {
					x: 6,
					y: 6
				},
				radius: 5,
				color: e.color,
				shape: [
					"triangle",
					"square",
					"diamond",
					"pentagon",
					"hexagon",
					"star"
				].includes(e.shape) ? e.shape : "circle"
			}, null, 8, ["color", "shape"])])), C("span", { style: T(`color:${V.value.chart.legend.color}`) }, gt(e.name), 5)], 46, ti))), 128))], 4)) : x("", !0)], !0)], 512)], 8, ["to"])) : x("", !0),
			t.$slots.source ? (E(), S("div", {
				key: 8,
				ref_key: "source",
				ref: Oi,
				dir: "auto"
			}, [k(t.$slots, "source", {}, void 0, !0)], 512)) : x("", !0),
			at(A(di), {
				teleportTo: V.value.chart.tooltip.teleportTo,
				show: H.value.showTooltip && Ki.value,
				backgroundColor: V.value.chart.tooltip.backgroundColor,
				color: V.value.chart.tooltip.color,
				fontSize: V.value.chart.tooltip.fontSize,
				borderRadius: V.value.chart.tooltip.borderRadius,
				borderColor: V.value.chart.tooltip.borderColor,
				borderWidth: V.value.chart.tooltip.borderWidth,
				backgroundOpacity: V.value.chart.tooltip.backgroundOpacity,
				position: V.value.chart.tooltip.position,
				offsetX: V.value.chart.tooltip.offsetX,
				offsetY: V.value.chart.tooltip.offsetY,
				parent: t.$refs.chart,
				content: Vl.value,
				isFullscreen: Gi.value,
				isCustom: V.value.chart.tooltip.customFormat && typeof V.value.chart.tooltip.customFormat == "function",
				smooth: V.value.chart.tooltip.smooth,
				backdropFilter: V.value.chart.tooltip.backdropFilter,
				smoothForce: V.value.chart.tooltip.smoothForce,
				smoothSnapThreshold: V.value.chart.tooltip.smoothSnapThreshold,
				isA11yMode: la.value != null,
				a11yPosition: ua.value
			}, {
				"tooltip-before": j(() => [k(t.$slots, "tooltip-before", dt(ct({ ...Ml.value })), void 0, !0)]),
				tooltip: j(() => [k(t.$slots, "tooltip", dt(ct({ ...Ml.value })), void 0, !0)]),
				"tooltip-after": j(() => [k(t.$slots, "tooltip-after", dt(ct({ ...Ml.value })), void 0, !0)]),
				_: 3
			}, 8, [
				"teleportTo",
				"show",
				"backgroundColor",
				"color",
				"fontSize",
				"borderRadius",
				"borderColor",
				"borderWidth",
				"backgroundOpacity",
				"position",
				"offsetX",
				"offsetY",
				"parent",
				"content",
				"isFullscreen",
				"isCustom",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			Na.value && V.value.chart.userOptions.buttons.table ? (E(), nt(mt(Lu.value.component), lt({ key: 9 }, Lu.value.props, {
				ref_key: "tableUnit",
				ref: sa,
				onClose: Ru
			}), rt({
				content: j(() => [C("div", { style: T(`${A(to) || V.value.table.useDialog ? "" : "max-height:400px"};${V.value.table.useDialog ? "height: fit-content; " : ""};overflow:auto;width:100%;${V.value.table.useDialog ? "" : "margin-top:48px"}`) }, [C("div", ci, [St(C("input", {
					type: "checkbox",
					"onUpdate:modelValue": n[4] ||= (e) => $i.value = e
				}, null, 512), [[yt, $i.value]]), C("div", {
					onClick: n[5] ||= (e) => $i.value = !$i.value,
					style: T({ cursor: Fa.value ? "pointer" : "default" })
				}, [at(A(pi), {
					name: "chartLine",
					size: 20,
					stroke: V.value.chart.color
				}, null, 8, ["stroke"])], 4)]), $i.value ? (E(), nt(A(mi), {
					key: `sparkline_${ea.value}`,
					dataset: pc.value,
					config: mc.value
				}, null, 8, ["dataset", "config"])) : (E(), nt(A(ui), {
					key: `table_${Zi.value}`,
					colNames: Zl.value.colNames,
					head: Zl.value.head,
					body: Zl.value.body,
					config: Zl.value.config,
					title: V.value.table.useDialog ? "" : Lu.value.title,
					withCloseButton: !V.value.table.useDialog,
					onClose: Ru
				}, {
					th: j(({ th: e }) => [C("div", { innerHTML: e }, null, 8, li)]),
					td: j(({ td: e }) => [it(gt(isNaN(Number(e)) ? e : A(fe)({
						p: V.value.chart.labels.prefix,
						v: e,
						s: V.value.chart.labels.suffix,
						r: V.value.table.rounding
					})), 1)]),
					_: 1
				}, 8, [
					"colNames",
					"head",
					"body",
					"config",
					"title",
					"withCloseButton"
				]))], 4)]),
				_: 2
			}, [V.value.table.useDialog ? {
				name: "title",
				fn: j(() => [it(gt(Lu.value.title), 1)]),
				key: "0"
			} : void 0, V.value.table.useDialog ? {
				name: "actions",
				fn: j(() => [C("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: n[3] ||= (e) => Ql(V.value.chart.userOptions.callbacks.csv)
				}, [at(A(pi), {
					name: "fileCsv",
					stroke: Lu.value.props.color
				}, null, 8, ["stroke"])])]),
				key: "1"
			} : void 0]), 1040)) : x("", !0),
			k(t.$slots, "skeleton", {}, () => [A(Ba) ? (E(), nt(Re, { key: 0 })) : x("", !0)], !0)
		], 46, Nt));
	}
}, [["__scopeId", "data-v-4c984fee"]]);
//#endregion
export { Mt as n, M as t };
