import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { G as t, Jt as n, Kt as r, P as i, Pt as a, Rt as o, S as ee, X as s, _ as c, b as l, ct as te, f as u, h as d, i as f, jt as ne, kt as re, p, q as ie, t as ae, tt as oe, w as se, xt as ce } from "./lib-Bttd6u5E.js";
import { n as le, t as ue } from "./useHints-Dq_w2E8B.js";
import { t as de } from "./useTimeLabels-d2f-W1L4.js";
import { t as fe } from "./useConfig-DlNpz6P8.js";
import { t as pe } from "./usePrinter-DN5bYhTG.js";
import { n as me, t as he } from "./BaseScanner-DZvpgOjM.js";
import { t as ge } from "./useNestedProp-vPNvh7rV.js";
import { t as _e } from "./useThemeCheck-C43Tcqmk.js";
import { t as ve } from "./useChartExport-DNiwdPmb.js";
import { t as ye } from "./useTransitions-g_zBREk2.js";
import { t as be } from "./useStableElementSize-C7KADDKj.js";
import { t as xe } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as Se } from "./img-Bnokohej.js";
import { t as Ce } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as we, t as Te } from "./useResponsive-ZtArZtUf.js";
import { t as Ee } from "./SlicerPreview-wUw1hFwe.js";
import { t as De } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Oe } from "./A11yDataTable-DdRsVULz.js";
import { t as ke } from "./useChartAccessibility-DYqac8yF.js";
import { t as Ae } from "./vue_ui_quick_chart-mZBdml3Z.js";
import { Fragment as m, Teleport as je, computed as h, createBlock as Me, createCommentVNode as g, createElementBlock as _, createElementVNode as v, createSlots as Ne, createVNode as Pe, defineAsyncComponent as Fe, guardReactiveProps as y, mergeProps as Ie, nextTick as Le, normalizeClass as b, normalizeProps as x, normalizeStyle as S, onBeforeUnmount as Re, onMounted as ze, openBlock as C, ref as w, renderList as T, renderSlot as E, shallowRef as Be, toDisplayString as D, toRefs as Ve, unref as O, watch as He, watchEffect as Ue, withCtx as k } from "vue";
//#region src/chartDetector.js
var A = {
	LINE: "LINE",
	BAR: "BAR",
	DONUT: "DONUT"
}, We = [
	"SERIE",
	"SERIES",
	"DATA",
	"VALUE",
	"VALUES",
	"NUM"
];
function Ge({ dataset: e, barLineSwitch: t = 6, debug: n = !0 }) {
	let r = null, i = null, a = 0;
	if ((typeof e == "number" || typeof e == "string") && n && console.warn(`The provided dataset (${e}) is not sufficient to build a chart`), qe(e) && (j(e) && (r = e.length < t ? A.BAR : A.LINE, i = e, a = e.length), Je(e))) {
		if (!Xe(e)) return n && console.warn("The objects in the dataset array have a different data structure. Either keys or value types are different."), !1;
		let o = Object.keys(e[0]), ee = Object.values(e[0]);
		if (!o.some((e) => Ze(e))) return n && console.warn("The data type of the dataset objects in the array must contain one of the following keys: DATA, SERIES, VALUE, VALUES, NUM. Casing is not important."), !1;
		Qe(ee, (e) => typeof e == "number") && (r = A.DONUT, i = e), Qe(ee, (e) => Array.isArray(e) && j(e)) && (r = $e(e) > t ? A.LINE : A.BAR, a = $e(e), i = e.map((e) => ({
			...e,
			data: et(e, (e) => j(e))
		}))), e = e.map((e) => M(e)), i = i.map((e) => M(e));
	}
	return {
		dataset: e,
		type: r,
		usableDataset: i,
		maxSeriesLength: a
	};
}
function Ke(e) {
	return !e || qe(e) && !e.length;
}
function qe(e) {
	return Array.isArray(e);
}
function j(e) {
	if (!qe(e) || Ke(e)) return !1;
	let t = e.map((e) => Number(e));
	return ![...new Set(t.flatMap((e) => typeof e == "number" && !isNaN(e)))].includes(!1);
}
function Je(e) {
	return !qe(e) || Ke(e) || [...new Set(e.flatMap((e) => typeof e == "object" && !Array.isArray(e)))].includes(!1) ? !1 : !e.map((e) => Object.keys(e).length > 0).includes(!1);
}
function Ye(e, t) {
	let n = Object.keys(e).sort(), r = Object.keys(t).sort();
	if (n.length !== r.length) return !1;
	for (let i = 0; i < n.length; i += 1) {
		let a = n[i], o = r[i];
		if (a !== o || typeof e[a] != typeof t[o]) return !1;
	}
	return !0;
}
function Xe(e) {
	if (e.length <= 1) return !0;
	for (let t = 0; t < e.length; t += 1) for (let n = t + 1; n < e.length; n += 1) if (!Ye(e[t], e[n])) return !1;
	return !0;
}
function Ze(e) {
	return We.includes(e.toUpperCase());
}
function Qe(e, t) {
	let n = [];
	for (let r = 0; r < e.length; r += 1) n.push(t(e[r]));
	return n.includes(!0);
}
function $e(e) {
	return Math.max(...[...e].flatMap((e) => Object.values(e).filter((e) => j(e)).map((e) => e.length)));
}
function et(e, t) {
	return Object.values(e).filter((e) => t(e))[0];
}
function M(e) {
	let t = {};
	for (let n in e) e.hasOwnProperty(n) && (t[n.toUpperCase()] = e[n]);
	return t;
}
//#endregion
//#region src/components/vue-ui-quick-chart.vue
var N = /* @__PURE__ */ e({ default: () => Dn }), tt = ["id"], nt = ["id"], rt = ["id"], it = { style: { position: "relative" } }, at = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], ot = [
	"x",
	"y",
	"width",
	"height"
], st = [
	"x",
	"y",
	"width",
	"height"
], ct = ["width", "height"], lt = ["id"], ut = ["id"], dt = ["id"], ft = ["flood-color"], pt = {
	key: 0,
	class: "donut-label-connectors"
}, mt = [
	"d",
	"stroke",
	"stroke-width",
	"filter"
], ht = [
	"cx",
	"cy",
	"r",
	"fill",
	"filter"
], gt = { class: "donut" }, _t = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], vt = [
	"d",
	"onMouseenter",
	"onMouseout",
	"onClick"
], yt = {
	key: 1,
	class: "donut-labels"
}, bt = [
	"cx",
	"cy",
	"fill",
	"stroke",
	"filter"
], xt = [
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"filter"
], St = [
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"filter"
], Ct = {
	key: 2,
	class: "donut-hollow"
}, wt = [
	"x",
	"y",
	"font-size",
	"fill"
], Tt = [
	"x",
	"y",
	"font-size",
	"fill"
], Et = {
	key: 0,
	class: "line-grid"
}, Dt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], Ot = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], kt = {
	key: 1,
	class: "line-axis"
}, At = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], jt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], Mt = [
	"d",
	"stroke",
	"stroke-width"
], Nt = [
	"transform",
	"font-size",
	"fill"
], Pt = {
	key: 3,
	class: "periodLabels"
}, Ft = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], It = { key: 0 }, Lt = [
	"font-size",
	"text-anchor",
	"fill",
	"transform"
], Rt = [
	"font-size",
	"text-anchor",
	"fill",
	"transform",
	"innerHTML"
], zt = { class: "plots" }, Bt = [
	"d",
	"stroke",
	"stroke-width"
], Vt = [
	"d",
	"stroke",
	"stroke-width"
], Ht = [
	"d",
	"stroke",
	"stroke-width"
], Ut = [
	"d",
	"stroke",
	"stroke-width"
], Wt = [
	"cx",
	"cy",
	"fill",
	"stroke"
], Gt = {
	key: 4,
	class: "dataLabels"
}, Kt = [
	"font-size",
	"fill",
	"transform"
], qt = {
	key: 5,
	class: "tooltip-traps"
}, Jt = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Yt = {
	key: 0,
	class: "line-grid"
}, Xt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], Zt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], Qt = {
	key: 1,
	class: "line-axis"
}, $t = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], en = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], tn = [
	"d",
	"stroke",
	"stroke-width"
], nn = [
	"transform",
	"font-size",
	"fill"
], rn = {
	key: 3,
	class: "periodLabels"
}, an = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], on = { key: 0 }, sn = [
	"font-size",
	"text-anchor",
	"fill",
	"transform"
], cn = [
	"font-size",
	"text-anchor",
	"fill",
	"transform",
	"innerHTML"
], ln = { class: "plots" }, un = [
	"x",
	"width",
	"height",
	"y",
	"fill",
	"stroke",
	"stroke-width"
], dn = {
	key: 4,
	class: "dataLabels"
}, fn = [
	"transform",
	"font-size",
	"fill"
], pn = {
	key: 5,
	class: "tooltip-traps"
}, mn = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], hn = {
	key: 6,
	class: "axis-labels"
}, gn = [
	"font-size",
	"fill",
	"x",
	"y"
], _n = [
	"font-size",
	"fill",
	"x",
	"y"
], vn = [
	"font-size",
	"fill",
	"transform"
], yn = [
	"font-size",
	"fill",
	"transform"
], bn = {
	key: 1,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, xn = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Sn = ["id"], Cn = ["onClick", "onKeydown"], wn = ["onClick", "onKeydown"], Tn = ["onClick", "onKeydown"], En = {
	key: 1,
	class: "vue-ui-quick-chart-not-processable"
}, Dn = /*#__PURE__*/ Ce({
	__name: "vue-ui-quick-chart",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: [
				Array,
				Object,
				String,
				Number
			],
			default() {
				return null;
			}
		}
	},
	emits: [
		"selectDatapoint",
		"selectLegend",
		"copyAlt"
	],
	setup(e, { expose: Ce, emit: We }) {
		let Ke = Fe(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), qe = Fe(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ye = Fe(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Xe = Fe(() => import("./Tooltip-DhjyfHwz.js")), Ze = Fe(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), { vue_ui_quick_chart: Qe } = fe(), { isThemeValid: $e, warnInvalidTheme: et } = _e(), M = e, N = w(null), Dn = w(null), On = w(null), kn = w(null), P = w(ie()), F = w(!1), An = w(null), jn = w(""), I = w(null), Mn = w(null), Nn = w(null), L = w([]), Pn = w(0), Fn = w(0), In = w(!1), Ln = w(null), Rn = w(null), R = w(null), zn = w(null), Bn = w(null), Vn = w(null), Hn = Be(null), Un = w(!1), Wn = w(0), Gn = w(0), Kn = w(!1);
		function qn() {
			Hn.value = N.value?.parentNode ?? null;
		}
		function Jn() {
			return new Promise((e) => {
				requestAnimationFrame(() => {
					requestAnimationFrame(e);
				});
			});
		}
		async function Yn() {
			let e = ++Gn.value;
			Un.value = !1, await Le(), await Jn(), await Jn(), e === Gn.value && (Wn.value += 1, Un.value = !0);
		}
		function Xn() {
			Kn.value || (Kn.value = !0, Le(() => {
				Kn.value = !1, qn(), Yn();
			}));
		}
		let Zn = be({
			elementRef: Hn,
			minimumWidth: 2,
			minimumHeight: 2,
			stableFramesRequired: 2,
			once: !1,
			onSizeAccepted: () => {
				Yn();
			}
		});
		w("#FFFFFF");
		let z = w(fr());
		le({
			config: () => z.value,
			dataset: () => M.dataset,
			component: "VueUiQuickChart",
			rules: [ue.emptyArray, {
				test: () => !0,
				message: [
					"👀 This is a swiss-knife component. If you need more control, consider using dedicated components:",
					"",
					"▶️ VueUiXy for time series line and/or bars",
					"",
					"▶️ VueUiDonut, VueUiWaffle, VueUiRings for proportions"
				]
			}]
		});
		let { transitionEnabled: B } = ye({
			config: () => z.value.transitions,
			dataset: () => M.dataset
		}), Qn = h(() => z.value.debug), $n = h(() => z.value.useCursorPointer), V = w(null), er = w({
			x: 0,
			y: 0
		}), H = w("pointer"), tr = w(!1), nr = h(() => n({
			dafaultConfig: {
				backgroundColor: "#99999930",
				customPalette: ["#BABABA"],
				showDataLabels: !1,
				paletteStartIndex: 0,
				showUserOptions: !1,
				showTooltip: !1,
				xAxisLabel: "",
				yAxisLabel: "",
				xyAxisStroke: "#999999",
				xyGridStroke: "#99999950",
				xyPeriods: [],
				xyShowScale: !1,
				xyPaddingLeft: 6,
				xyPaddingBottom: 12,
				zoomXy: !1,
				zoomStartIndex: null,
				zoomEndIndex: null
			},
			userConfig: z.value.skeletonConfig ?? {}
		})), { loading: rr, FINAL_DATASET: ir, manualLoading: ar } = me({
			...Ve(M),
			FINAL_CONFIG: z,
			prepareConfig: fr,
			skeletonDataset: M.config?.skeletonDataset ?? [
				1,
				2,
				3,
				5,
				8,
				13,
				21,
				34,
				55,
				89
			],
			skeletonConfig: n({
				defaultConfig: z.value,
				userConfig: nr.value
			})
		}), { svgRef: or } = ke({ config: { text: z.value.title } }), sr = h(() => z.value.showUserOptionsOnChartHover), cr = h(() => z.value.keepUserOptionsStateOnChartLeave), lr = w(!z.value.showUserOptionsOnChartHover), ur = w(!1);
		function dr(e = !1) {
			ur.value = e, sr.value && (lr.value = e);
		}
		function fr() {
			let e = ge({
				userConfig: M.config,
				defaultConfig: Qe
			}), t = {}, n = e.theme;
			if (n) if (!$e.value(e)) et(e), t = e;
			else {
				let i = ge({
					userConfig: Ae[n] || M.config,
					defaultConfig: e
				});
				t = {
					...ge({
						userConfig: M.config,
						defaultConfig: i
					}),
					customPalette: e.customPalette.length ? e.customPalette : r[n] || a
				};
			}
			else t = e;
			return t;
		}
		He(() => M.config, (e) => {
			rr.value || (z.value = fr()), K.value.width = z.value.width, K.value.height = z.value.height, lr.value = !z.value.showUserOptionsOnChartHover, Cr(), xr.value.showTooltip = z.value.showTooltip;
		}, { deep: !0 }), He(() => M.dataset, (e) => {
			W.value = mr.value, X.value.start = 0, X.value.end = W.value.maxSeriesLength, Fn.value += 1, Xn();
		}, { deep: !0 }), He(() => M.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (ar.value = !1);
		}, { immediate: !0 });
		let pr = h(() => se(z.value.customPalette)), U = We, mr = h(() => {
			let e = Ge({
				debug: Qn.value,
				dataset: o(ir.value, [
					"serie",
					"series",
					"data",
					"value",
					"values",
					"num"
				]),
				barLineSwitch: z.value.chartIsBarUnderDatasetLength
			});
			return !e && Qn.value && console.error("VueUiQuickChart : Dataset is not processable"), e;
		}), W = w(mr.value), hr = h(() => !!W.value), G = h(() => W.value ? W.value.type : null);
		He(() => G.value, (e) => {
			e || oe({
				componentName: "VueUiQuickChart",
				type: "dataset",
				debug: Qn.value
			});
		}, { immediate: !0 });
		let { isPrinting: gr, isImaging: _r, generatePdf: vr, generateImage: yr } = pe({
			elementId: `${G.value}_${P.value}`,
			fileName: z.value.title || G.value,
			options: z.value.userOptionsPrint
		}), br = h(() => z.value.showUserOptions && !z.value.title), K = w({
			width: z.value.width,
			height: z.value.height
		}), xr = w({ showTooltip: z.value.showTooltip });
		He(z, () => {
			xr.value = { showTooltip: z.value.showTooltip };
		}, { immediate: !0 });
		let q = Be(null), Sr = Be(null);
		ze(() => {
			In.value = !0, qn(), Zn.start(), Cr(), Yn();
		});
		function Cr() {
			if (ne(M.dataset) || (ar.value = z.value.loading), z.value.responsive) {
				let e = we(() => {
					let { width: e, height: t } = Te({
						chart: N.value,
						title: z.value.title ? Dn.value : null,
						legend: z.value.showLegend ? On.value : null,
						slicer: [A.BAR, A.LINE].includes(G.value) && z.value.zoomXy && W.value.maxSeriesLength > 1 ? kn.value : null,
						source: Mn.value,
						noTitle: Nn.value
					});
					requestAnimationFrame(() => {
						K.value.width = e, K.value.height = t, Xn();
					});
				});
				q.value && (Sr.value && q.value.unobserve(Sr.value), q.value.disconnect()), q.value = new ResizeObserver(e), Sr.value = N.value.parentNode, q.value.observe(Sr.value);
			}
			Lr(), Xn();
		}
		Re(() => {
			Zn.stop(), q.value && (Sr.value && q.value.unobserve(Sr.value), q.value.disconnect(), q.value = null, Sr.value = null);
		});
		let wr = h(() => {
			switch (G.value) {
				case A.LINE: return `0 0 ${K.value.width <= 0 ? 10 : K.value.width} ${K.value.height <= 0 ? 10 : K.value.height}`;
				case A.BAR: return `0 0 ${K.value.width <= 0 ? 10 : K.value.width} ${K.value.height <= 0 ? 10 : K.value.height}`;
				case A.DONUT: return `0 0 ${K.value.width <= 0 ? 10 : K.value.width} ${K.value.height <= 0 ? 10 : K.value.height}`;
				default: return `0 0 ${K.value.width <= 0 ? 10 : K.value.width} ${K.value.height <= 0 ? 10 : K.value.height}`;
			}
		});
		function Tr(e) {
			return [...e].map((e) => e.value).reduce((e, t) => e + t, 0);
		}
		function Er(e) {
			return z.value.blurOnHover && ![null, void 0].includes(I.value) && I.value !== e ? `url(#blur_${P.value})` : "";
		}
		function Dr() {
			L.value.length ? L.value = [] : G.value === A.DONUT ? Y.value.legend.forEach((e) => {
				L.value.push(e.id);
			}) : G.value === A.LINE ? Z.value.legend.forEach((e) => {
				L.value.push(e.id);
			}) : G.value === A.BAR && Q.value.legend.forEach((e) => {
				L.value.push(e.id);
			}), G.value === A.DONUT ? U("selectLegend", Y.value.dataset) : G.value === A.LINE ? U("selectLegend", Z.value.dataset) : G.value === A.BAR && U("selectLegend", Q.value.dataset);
		}
		function Or(e, t) {
			L.value.includes(e) ? L.value = L.value.filter((t) => t !== e) : L.value.length < t && L.value.push(e);
		}
		let kr = w(null), Ar = w(null), jr = w(!1);
		function Mr(e, t) {
			jr.value = !0;
			let n = e.value, r = mr.value.dataset.find((t, n) => e.id === `donut_${n}`).VALUE;
			if (L.value.includes(e.id)) {
				L.value = L.value.filter((t) => t !== e.id);
				function t() {
					n > r ? (jr.value = !1, cancelAnimationFrame(Ar.value), W.value = {
						...W.value,
						dataset: W.value.dataset.map((t, n) => e.id === `donut_${n}` ? {
							...t,
							value: r,
							VALUE: r
						} : t)
					}, U("selectLegend", Y.value.dataset)) : (n += r * .025, W.value = {
						...W.value,
						dataset: W.value.dataset.map((t, r) => e.id === `donut_${r}` ? {
							...t,
							value: n,
							VALUE: n
						} : t)
					}, Ar.value = requestAnimationFrame(t));
				}
				t();
			} else if (t.length > 1) {
				function t() {
					n < r / 100 ? (jr.value = !1, cancelAnimationFrame(kr.value), L.value.push(e.id), W.value = {
						...W.value,
						dataset: W.value.dataset.map((t, n) => e.id === `donut_${n}` ? {
							...t,
							value: 0,
							VALUE: 0
						} : t)
					}, U("selectLegend", Y.value.dataset)) : (n /= 1.1, W.value = {
						...W.value,
						dataset: W.value.dataset.map((t, r) => e.id === `donut_${r}` ? {
							...t,
							value: n,
							VALUE: n
						} : t)
					}, kr.value = requestAnimationFrame(t));
				}
				t();
			} else U("selectLegend", Y.value.dataset);
		}
		let J = w(null);
		function Nr(e) {
			J.value = e;
		}
		let Pr = h(() => z.value.donutThicknessRatio < .01 ? .01 : z.value.donutThicknessRatio > .4 ? .4 : z.value.donutThicknessRatio), Y = h(() => {
			if (G.value !== A.DONUT) return null;
			let e = W.value.dataset.map((e, t) => ({
				...e,
				value: e.VALUE || e.DATA || e.SERIE || e.VALUES || e.NUM || 0,
				name: e.NAME || e.DESCRIPTION || e.TITLE || e.LABEL || `Serie ${t}`,
				id: `donut_${t}`
			})).map((e, t) => ({
				...e,
				color: e.COLOR ? ee(e.COLOR) : pr.value[t + z.value.paletteStartIndex] || a[t + z.value.paletteStartIndex] || a[(t + z.value.paletteStartIndex) % a.length],
				immutableValue: e.value
			}));
			function t(e, t) {
				return s({
					v: isNaN(e.value / Tr(t)) ? 0 : e.value / Tr(t) * 100,
					s: "%",
					r: z.value.dataLabelRoundingPercentage
				});
			}
			function n(e) {
				return e.proportion * 100 > z.value.donutHideLabelUnderPercentage;
			}
			function r(e, t) {
				let n = mr.value.dataset.find((t, n) => `donut_${n}` === e).VALUE;
				return Math.abs(String(Number(n.toFixed(0))).length - String(Number(t.toFixed(0))).length);
			}
			function i({ datapoint: t, seriesIndex: n, triggerMode: r = "pointer" }) {
				An.value = {
					datapoint: t,
					seriesIndex: n,
					config: z.value,
					dataset: e
				}, I.value = t.id, V.value = n, H.value = r;
				let i = z.value.tooltipCustomFormat;
				if (z.value.events.datapointEnter && z.value.events.datapointEnter({
					datapoint: t,
					seriesIndex: n
				}), ce(i) && te(() => i({
					datapoint: t,
					seriesIndex: n,
					series: e,
					config: z.value
				}))) jn.value = i({
					datapoint: t,
					seriesIndex: n,
					series: e,
					config: z.value
				});
				else {
					let e = "";
					e += `<div style="width:100%;text-align:center;border-bottom:1px solid ${z.value.tooltipBorderColor};padding-bottom:6px;margin-bottom:3px;">${t.name}</div>`, e += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="none" fill="${t.color}"/></svg>`, e += `<b>${f(z.value.formatter, t.value, s({
						p: z.value.valuePrefix,
						v: t.value,
						s: z.value.valueSuffix,
						r: z.value.dataLabelRoundingValue
					}), {
						datapoint: t,
						seriesIndex: n
					})}</b>`, e += `<span>(${s({
						v: t.proportion * 100,
						s: "%",
						r: z.value.dataLabelRoundingPercentage
					})})</span></div>`, jn.value = `<div>${e}</div>`;
				}
				F.value = !0;
			}
			function o({ datapoint: e, seriesIndex: t }) {
				z.value.events.datapointLeave && z.value.events.datapointLeave({
					datapoint: e,
					seriesIndex: t
				}), F.value = !1, I.value = null, J.value = null, V.value = null, H.value = "pointer";
			}
			function c({ datapoint: e, seriesIndex: t }) {
				z.value.events.datapointClick && z.value.events.datapointClick({
					datapoint: e,
					seriesIndex: t
				}), U("selectDatapoint", e);
			}
			let l = {
				centerX: K.value.width / 2,
				centerY: K.value.height / 2
			}, u = e.filter((e) => !L.value.includes(e.id)).map((e) => e.value || 0).reduce((e, t) => e + t, 0), d = e.map((e, t) => ({
				...e,
				proportion: (e.value || 0) / u,
				value: e.value || 0,
				absoluteValue: mr.value.dataset.find((t, n) => `donut_${n}` === e.id).VALUE,
				shape: "circle"
			})), ne = K.value.width / 2, p = K.value.height / 2, ie = K.value.height * z.value.donutRadiusRatio;
			return {
				dataset: d.filter((e) => !L.value.includes(e.id)),
				legend: d,
				drawingArea: l,
				displayArcPercentage: t,
				isArcBigEnough: n,
				useTooltip: i,
				killTooltip: o,
				selectDatapoint: c,
				getSpaces: r,
				total: u,
				cx: ne,
				cy: p,
				radius: ie,
				chart: re({ series: e.filter((e) => !L.value.includes(e.id)) }, ne, p, ie, ie, 1.99999, 2, 1, 360, 105.25, K.value.height * Pr.value)
			};
		}), X = w({
			start: 0,
			end: W.value.maxSeriesLength
		});
		function Fr() {
			Lr();
		}
		let Ir = w(null);
		async function Lr() {
			await Le(), await Le();
			let { zoomStartIndex: e, zoomEndIndex: t } = z.value, n = Ir.value;
			(e != null || t != null) && n ? (e != null && n.setStartValue(e), t != null && n.setEndValue(Rr(t + 1))) : (X.value = {
				start: 0,
				end: W.value.maxSeriesLength
			}, Fn.value += 1), Xn();
		}
		function Rr(e) {
			let t = W.value.maxSeriesLength;
			return e > t ? t : e < 0 || z.value.zoomStartIndex !== null && e < z.value.zoomStartIndex ? z.value.zoomStartIndex === null ? 1 : z.value.zoomStartIndex + 1 : e;
		}
		let zr = h(() => {
			if (!z.value.zoomMinimap.show || G.value === A.DONUT) return [];
			let e = [];
			j(W.value.dataset) && (e = W.value.dataset), Je(W.value.dataset) && (e = W.value.dataset.map((e, t) => ({
				values: e.VALUE || e.DATA || e.SERIE || e.SERIES || e.VALUES || e.NUM || 0,
				id: G.value === A.LINE ? `line_${t}` : `bar_${t}`
			})).filter((e) => !L.value.includes(e.id)));
			let t = j(e) ? e.length : Math.max(...e.map((e) => e.values.length)), n = [];
			if (j(e)) n = e;
			else for (let r = 0; r < t; r += 1) n.push(e.map((e) => e.values[r] || 0).reduce((e, t) => (e || 0) + (t || 0), 0));
			let r = Math.min(...n);
			return n.map((e) => e + (r < 0 ? Math.abs(r) : 0));
		});
		function Br() {
			let e = 0;
			return zn.value && (e = Array.from(zn.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0)), e + 4;
		}
		let Vr = w(0), Hr = we((e) => {
			Vr.value = e;
		}, 100);
		Ue((e) => {
			let t = R.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				Hr(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), Re(() => {
			Vr.value = 0;
		});
		let Ur = h(() => {
			let e = 0;
			return R.value && (e = Vr.value), 0 + e;
		}), Z = h(() => {
			if (G.value !== A.LINE) return null;
			Wn.value;
			let e = {
				height: K.value.height,
				width: K.value.width
			}, t = Br();
			if (R.value) {
				let e = R.value.getBBox().x;
				e < 0 && (t += Math.abs(e));
			}
			let n = {
				left: t + z.value.xyPaddingLeft,
				top: z.value.xyPaddingTop,
				right: e.width - z.value.xyPaddingRight,
				bottom: e.height - z.value.xyPaddingBottom - Ur.value,
				width: Math.max(10, e.width - z.value.xyPaddingLeft - z.value.xyPaddingRight - t),
				height: Math.max(10, e.height - z.value.xyPaddingTop - z.value.xyPaddingBottom - Ur.value)
			}, r = [];
			j(W.value.dataset) && (r = [{
				values: W.value.dataset.slice(X.value.start, X.value.end),
				absoluteValues: W.value.dataset,
				absoluteIndices: W.value.dataset.map((e, t) => t).slice(X.value.start, X.value.end),
				name: z.value.title,
				color: pr.value[z.value.paletteStartIndex] || a[z.value.paletteStartIndex],
				id: "line_0"
			}]), Je(W.value.dataset) && (r = W.value.dataset.map((e, t) => ({
				...e,
				values: e.VALUE || e.DATA || e.SERIE || e.SERIES || e.VALUES || e.NUM || 0,
				name: e.NAME || e.DESCRIPTION || e.TITLE || e.LABEL || `Serie ${t}`,
				id: `line_${t}`
			})).map((e, t) => ({
				...e,
				color: e.COLOR ? ee(e.COLOR) : pr.value[t + z.value.paletteStartIndex] || a[t + z.value.paletteStartIndex] || a[(t + z.value.paletteStartIndex) % a.length],
				values: e.values.slice(X.value.start, X.value.end),
				absoluteValues: e.values,
				absoluteIndices: e.values.map((e, t) => t).slice(X.value.start, X.value.end)
			})));
			let i = {
				max: Math.max(...r.filter((e) => !L.value.includes(e.id)).flatMap((e) => e.values)),
				min: Math.min(...r.filter((e) => !L.value.includes(e.id)).flatMap((e) => e.values)),
				maxSeries: Math.max(...r.map((e) => e.values.length))
			}, o = i.max === i.min ? c(Math.min(i.min, 0), i.min === 0 ? 1 : Math.max(i.min, 0), z.value.xyScaleSegments) : c(i.min < 0 ? i.min : 0, i.max < 0 ? 0 : i.max, z.value.xyScaleSegments), l = i.min < 0 ? Math.abs(i.min) : 0, u = i.max < 0 ? n.top : n.bottom - l / (o.max + l) * n.height, d = n.width / i.maxSeries, ne = o.ticks.map((e) => ({
				y: n.bottom - n.height * ((e + l) / (o.max + l)),
				x: n.left - 8,
				value: e
			})), re = r.map((e, t) => ({
				...e,
				shape: "circle",
				coordinates: e.values.map((e, t) => ({
					x: n.left + d * (t + 1) - d / 2,
					y: n.bottom - (e + l) / (o.max + l) * n.height,
					value: e
				}))
			})).map((e) => {
				let t = [];
				return e.coordinates.forEach((e) => {
					t.push(`${e.x},${e.y} `);
				}), {
					...e,
					linePath: t.join(" ")
				};
			});
			function p(e) {
				return r.map((t) => ({
					...t,
					value: t.values[e],
					absoluteIndex: t.absoluteIndices[e]
				})).filter((e) => !L.value.includes(e.id));
			}
			function ie(e, t = "pointer") {
				I.value = e, J.value = e, V.value = e, H.value = t;
				let n = p(e);
				An.value = {
					datapoint: n,
					seriesIndex: e,
					config: z.value,
					dataset: r
				};
				let i = z.value.tooltipCustomFormat;
				if (z.value.events.datapointEnter && z.value.events.datapointEnter({
					datapoint: n,
					seriesIndex: e + X.value.start
				}), ce(i) && te(() => i({
					datapoint: n,
					seriesIndex: e,
					series: r,
					config: z.value
				}))) jn.value = i({
					datapoint: n,
					seriesIndex: e,
					series: r,
					config: z.value
				});
				else {
					let e = "";
					$.value[n[0].absoluteIndex] && (e += `<div style="border-bottom:1px solid ${z.value.tooltipBorderColor};padding-bottom:6px;margin-bottom:3px;">${$.value[n[0].absoluteIndex].text}</div>`), n.forEach((t, n) => {
						e += `
                    <div style="display:flex; flex-wrap: wrap; align-items:center; gap:3px;">
                        <svg viewBox="0 0 12 12" height="14" width="12"><circle cx="6" cy="6" r="6" stroke="none" fill="${t.color}"/></svg>
                        <span>${t.name}:</span>
                        <b>${f(z.value.formatter, t.value, s({
							p: z.value.valuePrefix,
							v: t.value,
							s: z.value.valueSuffix,
							r: z.value.dataLabelRoundingValue
						}), {
							datapoint: t,
							seriesIndex: n
						})}
                        </b>
                    </div>
                `;
					}), jn.value = e;
				}
				F.value = !0;
			}
			function ae(e) {
				let t = p(e);
				z.value.events.datapointLeave && z.value.events.datapointLeave({
					datapoint: t,
					seriesIndex: e + X.value.start
				}), I.value = null, J.value = null, F.value = !1, V.value = null, H.value = "pointer";
			}
			function oe(e) {
				let t = p(e);
				z.value.events.datapointClick && z.value.events.datapointClick({
					datapoint: t,
					seriesIndex: e + X.value.start
				}), U("selectDatapoint", t);
			}
			return {
				absoluteZero: u,
				dataset: re.filter((e) => !L.value.includes(e.id)),
				legend: re,
				drawingArea: n,
				extremes: i,
				slotSize: d,
				yLabels: ne,
				useTooltip: ie,
				killTooltip: ae,
				selectDatapoint: oe
			};
		}), Q = h(() => {
			if (G.value !== A.BAR) return null;
			Wn.value;
			let e = {
				height: K.value.height,
				width: K.value.width
			}, t = Br();
			if (R.value) {
				let e = R.value.getBBox().x;
				e < 0 && (t += Math.abs(e));
			}
			let n = {
				left: t + z.value.xyPaddingLeft,
				top: z.value.xyPaddingTop,
				right: e.width - z.value.xyPaddingRight,
				bottom: e.height - z.value.xyPaddingBottom - Ur.value,
				width: Math.max(10, e.width - z.value.xyPaddingLeft - z.value.xyPaddingRight - t),
				height: Math.max(10, e.height - z.value.xyPaddingTop - z.value.xyPaddingBottom - Ur.value)
			}, r = [];
			j(W.value.dataset) && (r = [{
				values: W.value.dataset.slice(X.value.start, X.value.end),
				absoluteValues: W.value.dataset,
				absoluteIndices: W.value.dataset.map((e, t) => t).slice(X.value.start, X.value.end),
				name: z.value.title,
				color: pr.value[z.value.paletteStartIndex] || a[z.value.paletteStartIndex],
				id: "bar_0"
			}]), Je(W.value.dataset) && (r = W.value.dataset.map((e, t) => ({
				...e,
				values: e.VALUE || e.DATA || e.SERIE || e.SERIES || e.VALUES || e.NUM || 0,
				name: e.NAME || e.DESCRIPTION || e.TITLE || e.LABEL || `Serie ${t}`,
				id: `bar_${t}`
			})).map((e, t) => ({
				...e,
				color: e.COLOR ? ee(e.COLOR) : pr.value[t + z.value.paletteStartIndex] || a[t + z.value.paletteStartIndex] || a[(t + z.value.paletteStartIndex) % a.length],
				values: e.values.slice(X.value.start, X.value.end),
				absoluteValues: e.values,
				absoluteIndices: e.values.map((e, t) => t).slice(X.value.start, X.value.end)
			})));
			let i = {
				max: Math.max(...r.filter((e) => !L.value.includes(e.id)).flatMap((e) => e.values)) < 0 ? 0 : Math.max(...r.filter((e) => !L.value.includes(e.id)).flatMap((e) => e.values)) ?? 1,
				min: Math.min(...r.filter((e) => !L.value.includes(e.id)).flatMap((e) => e.values)) ?? 0,
				maxSeries: Math.max(...r.filter((e) => !L.value.includes(e.id)).map((e) => e.values.length)) ?? 0
			}, o = i.min === i.max ? c(Math.min(i.min, 0), i.min === 0 ? 1 : Math.max(i.min, 0), z.value.xyScaleSegments) : c(i.min < 0 ? i.min : 0, i.max, z.value.xyScaleSegments), l = o.min < 0 ? Math.abs(o.min) : 0, u = n.bottom - l / (o.max + l) * n.height, d = n.width / i.maxSeries, ne = o.ticks.map((e) => ({
				y: n.bottom - n.height * ((e + l) / (o.max + l)),
				x: n.left - 8,
				value: e
			})), re = r.map((e, t) => ({
				...e,
				shape: "square",
				coordinates: e.values.map((e, a) => {
					let o = (e + l) / (i.max + l) * n.height, ee = Math.abs(e) / Math.abs(i.min) * (n.height - u), s = l / (i.max + l) * n.height, c = d / r.filter((e) => !L.value.includes(e.id)).length - z.value.barGap / r.filter((e) => !L.value.includes(e.id)).length;
					return {
						x: n.left + d * a + c * t + z.value.barGap / 2,
						y: e > 0 ? n.bottom - o : u,
						height: e > 0 ? o - s : ee,
						value: e,
						width: c
					};
				})
			})), p = r.filter((e) => !L.value.includes(e.id)).map((e, t) => ({
				...e,
				coordinates: e.values.map((e, a) => {
					let o = (e + l) / (i.max + l) * n.height, ee = Math.abs(e) / (i.max + l) * n.height, s = l / (i.max + l) * n.height, c = d / r.filter((e) => !L.value.includes(e.id)).length - z.value.barGap / r.filter((e) => !L.value.includes(e.id)).length;
					return {
						x: n.left + d * a + c * t + z.value.barGap / 2,
						y: e > 0 ? n.bottom - o : u,
						height: e > 0 ? o - s : ee,
						value: e,
						width: c
					};
				})
			}));
			function ie(e) {
				return r.map((t) => ({
					...t,
					value: t.values[e],
					absoluteIndex: t.absoluteIndices[e]
				})).filter((e) => !L.value.includes(e.id));
			}
			function ae(e, t = "pointer") {
				I.value = e, J.value = e, V.value = e, H.value = t;
				let n = ie(e);
				An.value = {
					datapoint: n,
					seriesIndex: e,
					config: z.value,
					dataset: r
				};
				let i = z.value.tooltipCustomFormat;
				if (z.value.events.datapointEnter && z.value.events.datapointEnter({
					datapoint: n,
					seriesIndex: e + X.value.start
				}), ce(i) && te(() => i({
					datapoint: n,
					seriesIndex: e,
					series: r,
					config: z.value
				}))) jn.value = i({
					point: n,
					seriesIndex: e,
					series: r,
					config: z.value
				});
				else {
					let e = "";
					$.value[n[0].absoluteIndex] && (e += `<div style="border-bottom:1px solid ${z.value.tooltipBorderColor};padding-bottom:6px;margin-bottom:3px;">${$.value[n[0].absoluteIndex].text}</div>`), n.forEach((t, n) => {
						e += `
                    <div style="display:flex; flex-wrap: wrap; align-items:center; gap:3px;">
                        <svg viewBox="0 0 12 12" height="14" width="12"><rect x=0 y="0" width="12" height="12" rx="1" stroke="none" fill="${t.color}"/></svg>
                        <span>${t.name}:</span>
                        <b>${f(z.value.formatter, t.value, s({
							p: z.value.valuePrefix,
							v: t.value,
							s: z.value.valueSuffix,
							r: z.value.dataLabelRoundingValue
						}), {
							datapoint: t,
							seriesIndex: n
						})}
                        </b>
                    </div>
                `;
					}), jn.value = e;
				}
				F.value = !0;
			}
			function oe(e) {
				let t = ie(e);
				z.value.events.datapointLeave && z.value.events.datapointLeave({
					datapoint: t,
					seriesIndex: e + X.value.start
				}), F.value = !1, I.value = null, J.value = null, V.value = null, H.value = "pointer";
			}
			function se(e) {
				let t = ie(e);
				z.value.events.datapointClick && z.value.events.datapointClick({
					datapoint: t,
					seriesIndex: e + X.value.start
				}), U("selectDatapoint", t);
			}
			return {
				absoluteZero: u,
				dataset: p.filter((e) => !L.value.includes(e.id)),
				absoluteDataset: p,
				legend: re,
				drawingArea: n,
				extremes: i,
				slotSize: d,
				yLabels: ne,
				useTooltip: ae,
				killTooltip: oe,
				selectDatapoint: se
			};
		}), Wr = h(() => G.value === A.LINE ? Z.value?.drawingArea ?? null : G.value === A.BAR ? Q.value?.drawingArea ?? null : null), Gr = h(() => {
			let e = Wr.value, t = K.value.width;
			return !e || t <= 0 || !z.value.zoomXyAutoFit ? null : e.left / t;
		}), Kr = h(() => {
			let e = Wr.value, t = K.value.width;
			return !e || t <= 0 || !z.value.zoomXyAutoFit ? null : (t - e.right) / t;
		}), qr = h(() => {
			if (G.value === A.LINE) return Z.value.legend.map((e) => (Math.min(...e.absoluteValues.map((e) => e ?? 0)), {
				...e,
				isVisible: !L.value.includes(e.id),
				type: "line",
				series: e.absoluteValues
			}));
			if (G.value === A.BAR) return Q.value.legend.map((e) => (Math.min(...e.absoluteValues.map((e) => e ?? 0)), {
				...e,
				isVisible: !L.value.includes(e.id),
				type: "bar",
				series: e.absoluteValues
			}));
		}), $ = w([]), Jr = 0;
		Ue(() => {
			let e = ++Jr;
			(async () => {
				let t = await de({
					values: z.value.xyPeriods,
					maxDatapoints: W.value.maxSeriesLength,
					formatter: z.value.datetimeFormatter,
					start: X.value.start,
					end: X.value.end
				});
				e === Jr && ($.value = t);
			})();
		});
		let Yr = h(() => {
			let e = z.value.xyPeriodsModulo;
			return z.value.xyPeriods.length ? Math.min(e, [...new Set($.value.map((e) => e.text))].length) : e;
		}), Xr = w(!1);
		function Zr(e) {
			Xr.value = e, Pn.value += 1;
		}
		function Qr() {
			xr.value.showTooltip = !xr.value.showTooltip;
		}
		let $r = w(!1);
		function ei() {
			$r.value = !$r.value;
		}
		async function ti({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { width: t, height: n } = N.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await Se({
				domElement: N.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: z.value.title,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let ni = h(() => K.value.width), ri = h(() => K.value.height);
		xe({
			timeLabelsEls: R,
			timeLabels: $,
			slicer: X,
			configRef: z,
			rotationPath: ["xyPeriodLabelsRotation"],
			autoRotatePath: ["xyPeriodLabelsAutoRotate", "enable"],
			isAutoSize: !1,
			rotation: z.value.xyPeriodLabelsAutoRotate.angle,
			height: ri.value,
			width: ni.value
		});
		let ii = h(() => z.value.backgroundColor), ai = h(() => G.value === A.DONUT ? Y.value.legend : G.value === A.LINE ? Z.value.legend : Q.value.legend), oi = h(() => ({
			show: z.value.showLegend,
			bold: !1,
			backgroundColor: z.value.backgroundColor,
			color: z.value.color,
			fontSize: z.value.legendFontSize,
			position: z.value.legendPosition
		})), si = h(() => ({
			text: z.value.title,
			color: z.value.color,
			fontSize: z.value.titleFontSize,
			bold: z.value.titleBold,
			textAlign: z.value.titleTextAlign,
			subtitle: { text: "" }
		})), { generateSvg: ci, onGenerateImage: li } = ve({
			svg: or,
			title: si,
			legend: oi,
			legendItems: ai,
			backgroundColor: ii,
			getSvgCallback: () => z.value.userOptionsCallbacks.svg,
			generateImage: yr
		});
		async function ui() {
			if (U("copyAlt", {
				config: z.value,
				dataset: {
					line: Z.value,
					bar: Q.value,
					donut: Y.value
				}
			}), !z.value.userOptionsCallbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(z.value.userOptionsCallbacks.altCopy({
				config: z.value,
				dataset: {
					line: Z.value,
					bar: Q.value,
					donut: Y.value
				}
			}));
		}
		function di(e, t) {
			(e.key === "Enter" || e.key === " ") && (e.preventDefault(), t());
		}
		let fi = h(() => G.value === A.DONUT ? Y.value?.chart?.length ?? 0 : G.value === A.LINE ? Z.value?.extremes?.maxSeries ?? 0 : G.value === A.BAR ? Q.value?.extremes?.maxSeries ?? 0 : 0);
		function pi() {
			V.value = null, tr.value = !0;
		}
		function mi() {
			V.value = null, H.value = "pointer", F.value = !1, I.value = null, J.value = null, tr.value = !1;
		}
		function hi(e) {
			if (!or.value || $r.value || document.activeElement !== or.value || !fi.value) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				V.value = null, H.value = "pointer", F.value = !1, I.value = null, J.value = null;
				return;
			}
			if (r) {
				if (V.value === null) return;
				if (G.value === A.DONUT) {
					let e = Y.value?.chart?.[V.value];
					if (!e) return;
					Y.value.selectDatapoint({
						datapoint: e,
						seriesIndex: V.value
					});
					return;
				}
				if (G.value === A.LINE) {
					Z.value?.selectDatapoint(V.value);
					return;
				}
				if (G.value === A.BAR) {
					Q.value?.selectDatapoint(V.value);
					return;
				}
				return;
			}
			let a = V.value, o = J.value, ee = a !== null && a >= 0 && a < fi.value, s = o !== null && o >= 0 && o < fi.value;
			if (ee ? n ? (a += 1, a >= fi.value && (a = 0)) : t && (--a, a < 0 && (a = fi.value - 1)) : s ? (a = n ? o + 1 : o - 1, a >= fi.value && (a = 0), a < 0 && (a = fi.value - 1)) : a = n ? 0 : fi.value - 1, G.value === A.DONUT) {
				let e = Y.value?.chart?.[a];
				if (!e) return;
				gi(a), Y.value.useTooltip({
					datapoint: e,
					seriesIndex: a,
					triggerMode: "keyboard"
				});
				return;
			}
			if (G.value === A.LINE) {
				gi(a), Z.value?.useTooltip(a, "keyboard");
				return;
			}
			G.value === A.BAR && (gi(a), Q.value?.useTooltip(a, "keyboard"));
		}
		function gi(e) {
			if (!Number.isFinite(e) || !or.value) return;
			let t = 0, n = 0;
			if (G.value === A.DONUT) {
				let r = Y.value?.chart?.[e];
				if (!r) return;
				t = u(r, !0).x, n = p(r);
			}
			if (G.value === A.LINE) {
				let r = Z.value?.drawingArea, i = Z.value?.slotSize;
				if (!r || !i) return;
				t = r.left + i * (e + 1) - i / 2, n = r.top + r.height / 2;
			}
			if (G.value === A.BAR) {
				let r = Q.value?.drawingArea, i = Q.value?.slotSize;
				if (!r || !i) return;
				t = r.left + i * (e + 1) - i / 2, n = r.top + r.height / 2;
			}
			let r = or.value.getBoundingClientRect();
			er.value = {
				x: r.left + t / K.value.width * r.width,
				y: r.top + n / K.value.height * r.height
			};
		}
		let _i = h(() => {
			if (G.value === A.DONUT) return {
				headers: [
					"Series",
					"Value",
					"Percentage"
				],
				rows: (Y.value?.dataset ?? []).map((e) => {
					let t = Y.value?.total ? s({
						v: e.value / Y.value.total * 100,
						s: "%",
						r: z.value.dataLabelRoundingPercentage
					}) : "0%";
					return [
						e.name,
						f(z.value.formatter, e.value, s({
							p: z.value.valuePrefix,
							v: e.value,
							s: z.value.valueSuffix,
							r: z.value.dataLabelRoundingValue
						})),
						t
					];
				})
			};
			if (G.value === A.LINE || G.value === A.BAR) {
				let e = G.value === A.LINE ? Z.value?.dataset ?? [] : Q.value?.dataset ?? [], t = G.value === A.LINE ? Z.value?.extremes?.maxSeries ?? 0 : Q.value?.extremes?.maxSeries ?? 0;
				return {
					headers: ["Index", ...e.map((e) => e.name)],
					rows: Array.from({ length: t }, (t, n) => [$.value?.[n + X.value.start]?.text ?? String(n + X.value.start), ...e.map((e) => {
						let t = e.values?.[n];
						return f(z.value.formatter, t, s({
							p: z.value.valuePrefix,
							v: t,
							s: z.value.valueSuffix,
							r: z.value.dataLabelRoundingValue
						}));
					})])
				};
			}
			return {
				headers: [],
				rows: []
			};
		});
		return Ce({
			getImage: ti,
			generatePdf: vr,
			generateImage: yr,
			generateSvg: ci,
			toggleTooltip: Qr,
			toggleAnnotator: ei,
			toggleFullscreen: Zr,
			copyAlt: ui
		}), (e, n) => hr.value ? (C(), _("div", {
			key: 0,
			id: `${G.value}_${P.value}`,
			ref_key: "quickChart",
			ref: N,
			class: b({
				"vue-data-ui-component": !0,
				"vue-ui-quick-chart": !0,
				"vue-data-ui-wrapper-fullscreen": Xr.value
			}),
			style: S(`background:${z.value.backgroundColor};color:${z.value.color};font-family:${z.value.fontFamily}; position: relative; ${z.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: n[2] ||= () => dr(!0),
			onMouseleave: n[3] ||= () => dr(!1)
		}, [
			v("div", {
				id: `chart-instructions-${P.value}`,
				class: "sr-only"
			}, [v("p", null, D(z.value.a11y.translations.keyboardNavigation), 1)], 8, nt),
			_i.value.rows.length ? (C(), Me(Oe, {
				key: 0,
				uid: P.value,
				head: _i.value.headers,
				body: _i.value.rows,
				notice: z.value.a11y.translations.tableAvailable,
				caption: z.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : g("", !0),
			z.value.userOptionsButtons.annotator ? (C(), Me(O(Ye), {
				key: 1,
				svgRef: O(or),
				backgroundColor: z.value.backgroundColor,
				color: z.value.color,
				active: $r.value,
				isCursorPointer: $n.value,
				onClose: ei
			}, {
				"annotator-action-close": k(() => [E(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": k(({ color: t }) => [E(e.$slots, "annotator-action-color", x(y({ color: t })), void 0, !0)]),
				"annotator-action-draw": k(({ mode: t }) => [E(e.$slots, "annotator-action-draw", x(y({ mode: t })), void 0, !0)]),
				"annotator-action-undo": k(({ disabled: t }) => [E(e.$slots, "annotator-action-undo", x(y({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": k(({ disabled: t }) => [E(e.$slots, "annotator-action-redo", x(y({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": k(({ disabled: t }) => [E(e.$slots, "annotator-action-delete", x(y({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : g("", !0),
			br.value ? (C(), _("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Nn,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : g("", !0),
			z.value.showUserOptions && (cr.value || lr.value) ? (C(), Me(O(Ze), {
				ref: "details",
				key: `user_option_${Pn.value}`,
				backgroundColor: z.value.backgroundColor,
				color: z.value.color,
				isPrinting: O(gr),
				isImaging: O(_r),
				uid: P.value,
				hasTooltip: z.value.userOptionsButtons.tooltip && z.value.showTooltip,
				hasPdf: z.value.userOptionsButtons.pdf,
				hasImg: z.value.userOptionsButtons.img,
				hasSvg: z.value.userOptionsButtons.svg,
				hasFullscreen: z.value.userOptionsButtons.fullscreen,
				hasAltCopy: z.value.userOptionsButtons.altCopy,
				hasXls: !1,
				isTooltip: xr.value.showTooltip,
				isFullscreen: Xr.value,
				titles: { ...z.value.userOptionsButtonTitles },
				chartElement: N.value,
				position: z.value.userOptionsPosition,
				hasAnnotator: z.value.userOptionsButtons.annotator,
				isAnnotation: $r.value,
				callbacks: z.value.userOptionsCallbacks,
				printScale: z.value.userOptionsPrint.scale,
				isCursorPointer: $n.value,
				onToggleFullscreen: Zr,
				onGeneratePdf: O(vr),
				onGenerateImage: O(li),
				onGenerateSvg: O(ci),
				onToggleTooltip: Qr,
				onToggleAnnotator: ei,
				onCopyAlt: ui,
				style: S({ visibility: cr.value ? lr.value ? "visible" : "hidden" : "visible" })
			}, Ne({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: k(({ isOpen: t, color: n }) => [E(e.$slots, "menuIcon", x(y({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: k(() => [E(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: k(() => [E(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: k(() => [E(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: k(() => [E(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: k(({ toggleFullscreen: t, isFullscreen: n }) => [E(e.$slots, "optionFullscreen", x(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: k(({ toggleAnnotator: t, isAnnotator: n }) => [E(e.$slots, "optionAnnotator", x(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: k(({ altCopy: t }) => [E(e.$slots, "optionAltCopy", x(y({ altCopy: t })), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: k(() => [E(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: k(() => [E(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "9"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"hasTooltip",
				"hasPdf",
				"hasImg",
				"hasSvg",
				"hasFullscreen",
				"hasAltCopy",
				"isTooltip",
				"isFullscreen",
				"titles",
				"chartElement",
				"position",
				"hasAnnotator",
				"isAnnotation",
				"callbacks",
				"printScale",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : g("", !0),
			z.value.title ? (C(), _("div", {
				key: 4,
				ref_key: "quickChartTitle",
				ref: Dn,
				class: "vue-ui-quick-chart-title",
				style: S(`background:transparent;color:${z.value.color};font-size:${z.value.titleFontSize}px;font-weight:${z.value.titleBold ? "bold" : "normal"};text-align:${z.value.titleTextAlign}`)
			}, D(z.value.title), 5)) : g("", !0),
			v("div", { id: `legend-top-${P.value}` }, null, 8, rt),
			v("div", it, [G.value ? (C(), _("svg", {
				key: 0,
				ref_key: "svgRef",
				ref: or,
				xmlns: O(ae),
				"aria-describedby": `chart-instructions-${P.value}`,
				viewBox: wr.value,
				style: S(`max-width:100%;overflow:visible;background:transparent;color:${z.value.color}`),
				class: b({ "vue-data-ui-no-transition": !O(B) }),
				tabindex: "0",
				onFocus: pi,
				onBlur: mi,
				onKeydown: hi
			}, [
				Pe(O(qe)),
				e.$slots["chart-background"] && G.value === A.BAR ? (C(), _("foreignObject", {
					key: 0,
					x: Q.value.drawingArea.left,
					y: Q.value.drawingArea.top,
					width: Q.value.drawingArea.width,
					height: Q.value.drawingArea.height,
					style: { pointerEvents: "none" }
				}, [E(e.$slots, "chart-background", {}, void 0, !0)], 8, ot)) : g("", !0),
				e.$slots["chart-background"] && G.value === A.LINE ? (C(), _("foreignObject", {
					key: 1,
					x: Z.value.drawingArea.left,
					y: Z.value.drawingArea.top,
					width: Z.value.drawingArea.width,
					height: Z.value.drawingArea.height,
					style: { pointerEvents: "none" }
				}, [E(e.$slots, "chart-background", {}, void 0, !0)], 8, st)) : g("", !0),
				e.$slots["chart-background"] && G.value === A.DONUT ? (C(), _("foreignObject", {
					key: 2,
					x: 0,
					y: 0,
					width: K.value.width,
					height: K.value.height,
					style: { pointerEvents: "none" }
				}, [E(e.$slots, "chart-background", {}, void 0, !0)], 8, ct)) : g("", !0),
				v("defs", null, [v("filter", {
					id: `blur_${P.value}`,
					x: "-50%",
					y: "-50%",
					width: "200%",
					height: "200%"
				}, [v("feGaussianBlur", {
					in: "SourceGraphic",
					stdDeviation: 2,
					id: `blur_std_${P.value}`
				}, null, 8, ut), n[4] ||= v("feColorMatrix", {
					type: "saturate",
					values: "0"
				}, null, -1)], 8, lt), v("filter", {
					id: `shadow_${P.value}`,
					"color-interpolation-filters": "sRGB"
				}, [v("feDropShadow", {
					dx: "0",
					dy: "0",
					stdDeviation: "10",
					"flood-opacity": "0.5",
					"flood-color": z.value.donutShadowColor
				}, null, 8, ft)], 8, dt)]),
				G.value === A.DONUT ? (C(), _(m, { key: 3 }, [
					z.value.showDataLabels ? (C(), _("g", pt, [(C(!0), _(m, null, T(Y.value.chart, (e, t) => (C(), _(m, null, [Y.value.isArcBigEnough(e) ? (C(), _("path", {
						key: 0,
						d: O(d)(e, {
							x: K.value.width / 2,
							y: K.value.height / 2
						}, 16, 16, !1, !1, K.value.height * Pr.value, 12, z.value.donutCurvedMarkers),
						stroke: e.color,
						"stroke-width": z.value.donutLabelMarkerStrokeWidth,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						fill: "none",
						filter: Er(e.id)
					}, null, 8, mt)) : g("", !0)], 64))), 256))])) : g("", !0),
					v("circle", {
						cx: Y.value.cx,
						cy: Y.value.cy,
						r: Y.value.radius,
						fill: z.value.backgroundColor,
						filter: z.value.donutUseShadow ? `url(#shadow_${P.value})` : ""
					}, null, 8, ht),
					v("g", gt, [(C(!0), _(m, null, T(Y.value.chart, (e, t) => (C(), _("path", {
						d: e.arcSlice,
						fill: e.color,
						stroke: z.value.donutStroke || z.value.backgroundColor,
						"stroke-width": z.value.donutStrokeWidth,
						filter: Er(e.id)
					}, null, 8, _t))), 256)), (C(!0), _(m, null, T(Y.value.chart, (e, t) => (C(), _("path", {
						d: e.arcSlice,
						fill: "transparent",
						onMouseenter: (n) => Y.value.useTooltip({
							datapoint: e,
							seriesIndex: t,
							triggerMode: "pointer"
						}),
						onMouseout: (n) => Y.value.killTooltip({
							datapoint: e,
							seriesIndex: t
						}),
						onClick: (n) => Y.value.selectDatapoint({
							datapoint: e,
							seriesIndex: t
						})
					}, null, 40, vt))), 256))]),
					z.value.showDataLabels ? (C(), _("g", yt, [(C(!0), _(m, null, T(Y.value.chart, (e, t) => (C(), _(m, null, [
						Y.value.isArcBigEnough(e) ? (C(), _("circle", {
							key: 0,
							cx: O(u)(e).x,
							cy: O(p)(e) - 3.7,
							fill: e.color,
							stroke: z.value.backgroundColor,
							"stroke-width": 1,
							r: 3,
							filter: Er(e.id)
						}, null, 8, bt)) : g("", !0),
						Y.value.isArcBigEnough(e) ? (C(), _("text", {
							key: 1,
							"text-anchor": O(u)(e, !0, 20).anchor,
							x: O(u)(e, !0).x,
							y: O(p)(e),
							fill: z.value.color,
							"font-size": z.value.dataLabelFontSize,
							filter: Er(e.id)
						}, D(Y.value.displayArcPercentage(e, Y.value.chart)) + " (" + D(O(f)(z.value.formatter, e.value, O(s)({
							p: z.value.valuePrefix,
							v: e.value,
							s: z.value.valueSuffix,
							r: z.value.dataLabelRoundingValue
						}), {
							datapoint: e,
							seriesIndex: t
						})) + ") ", 9, xt)) : g("", !0),
						Y.value.isArcBigEnough(e, !0, 20) ? (C(), _("text", {
							key: 2,
							"text-anchor": O(u)(e).anchor,
							x: O(u)(e, !0).x,
							y: O(p)(e) + z.value.dataLabelFontSize,
							fill: z.value.color,
							"font-size": z.value.dataLabelFontSize,
							filter: Er(e.id)
						}, D(e.name), 9, St)) : g("", !0)
					], 64))), 256))])) : g("", !0),
					z.value.donutShowTotal ? (C(), _("g", Ct, [v("text", {
						"text-anchor": "middle",
						x: Y.value.drawingArea.centerX,
						y: Y.value.drawingArea.centerY - z.value.donutTotalLabelFontSize / 2,
						"font-size": z.value.donutTotalLabelFontSize,
						fill: z.value.color
					}, D(z.value.donutTotalLabelText), 9, wt), v("text", {
						"text-anchor": "middle",
						x: Y.value.drawingArea.centerX,
						y: Y.value.drawingArea.centerY + z.value.donutTotalLabelFontSize,
						"font-size": z.value.donutTotalLabelFontSize,
						fill: z.value.color
					}, D(O(s)({
						p: z.value.valuePrefix,
						v: Y.value.total,
						s: z.value.valueSuffix,
						r: z.value.dataLabelRoundingValue
					})), 9, Tt)])) : g("", !0)
				], 64)) : g("", !0),
				G.value === A.LINE ? (C(), _(m, { key: 4 }, [
					z.value.xyShowGrid ? (C(), _("g", Et, [(C(!0), _(m, null, T(Z.value.yLabels, (e) => (C(), _(m, null, [e.y <= Z.value.drawingArea.bottom ? (C(), _("line", {
						key: 0,
						x1: Z.value.drawingArea.left,
						x2: Z.value.drawingArea.right,
						y1: e.y,
						y2: e.y,
						stroke: z.value.xyGridStroke,
						"stroke-width": z.value.xyGridStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, Dt)) : g("", !0)], 64))), 256)), (C(!0), _(m, null, T(Z.value.extremes.maxSeries + 1, (e, t) => (C(), _("line", {
						x1: Z.value.drawingArea.left + Z.value.slotSize * t,
						x2: Z.value.drawingArea.left + Z.value.slotSize * t,
						y1: Z.value.drawingArea.top,
						y2: Z.value.drawingArea.bottom,
						stroke: z.value.xyGridStroke,
						"stroke-width": z.value.xyGridStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, Ot))), 256))])) : g("", !0),
					z.value.xyShowAxis ? (C(), _("g", kt, [v("line", {
						x1: Z.value.drawingArea.left,
						x2: Z.value.drawingArea.left,
						y1: Z.value.drawingArea.top,
						y2: Z.value.drawingArea.bottom,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, At), v("line", {
						x1: Z.value.drawingArea.left,
						x2: Z.value.drawingArea.right,
						y1: isNaN(Z.value.absoluteZero) ? Z.value.drawingArea.bottom : Z.value.absoluteZero,
						y2: isNaN(Z.value.absoluteZero) ? Z.value.drawingArea.bottom : Z.value.absoluteZero,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, jt)])) : g("", !0),
					z.value.xyShowScale ? (C(), _("g", {
						key: 2,
						class: "yLabels",
						ref_key: "scaleLabels",
						ref: zn
					}, [(C(!0), _(m, null, T(Z.value.yLabels, (e, t) => (C(), _(m, { key: `sl_${t}` }, [e.y <= Z.value.drawingArea.bottom ? (C(), _("path", {
						key: 0,
						class: b({ "vue-data-ui-transition": O(B) }),
						d: `M${e.x + 4},${e.y} ${Z.value.drawingArea.left},${e.y}`,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 10, Mt)) : g("", !0), e.y <= Z.value.drawingArea.bottom ? (C(), _("text", {
						key: 1,
						class: b({ "vue-data-ui-transition": O(B) }),
						transform: `translate(${e.x}, ${e.y + z.value.xyLabelsYFontSize / 3})`,
						"text-anchor": "end",
						"font-size": z.value.xyLabelsYFontSize,
						fill: z.value.color
					}, D(O(f)(z.value.formatter, e.value, O(s)({
						p: z.value.valuePrefix,
						v: e.value,
						s: z.value.valueSuffix,
						r: z.value.dataLabelRoundingValue
					}), {
						datapoint: e,
						seriesIndex: t
					})), 11, Nt)) : g("", !0)], 64))), 128))], 512)) : g("", !0),
					z.value.xyShowScale && z.value.xyPeriods.length ? (C(), _("g", Pt, [(C(!0), _(m, null, T($.value.map((e) => e.text), (e, t) => (C(), _(m, null, [!z.value.xyPeriodsShowOnlyAtModulo || z.value.xyPeriodsShowOnlyAtModulo && t % Math.floor((X.value.end - X.value.start) / Yr.value) === 0 || X.value.end - X.value.start <= Yr.value ? (C(), _("line", {
						key: 0,
						x1: Z.value.drawingArea.left + Z.value.slotSize * (t + 1) - Z.value.slotSize / 2,
						x2: Z.value.drawingArea.left + Z.value.slotSize * (t + 1) - Z.value.slotSize / 2,
						y1: Z.value.drawingArea.bottom,
						y2: Z.value.drawingArea.bottom + 4,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, Ft)) : g("", !0)], 64))), 256)), v("g", {
						ref_key: "timeLabelsEls",
						ref: R
					}, [(C(!0), _(m, null, T($.value.map((e) => e.text), (e, n) => (C(), _(m, null, [!z.value.xyPeriodsShowOnlyAtModulo || z.value.xyPeriodsShowOnlyAtModulo && n % Math.floor((X.value.end - X.value.start) / Yr.value) === 0 || X.value.end - X.value.start <= Yr.value ? (C(), _("g", It, [String(e).includes("\n") ? (C(), _("text", {
						key: 1,
						class: "vue-data-ui-time-label",
						"font-size": z.value.xyLabelsXFontSize,
						"text-anchor": z.value.xyPeriodLabelsRotation > 0 ? "start" : z.value.xyPeriodLabelsRotation < 0 ? "end" : "middle",
						fill: z.value.color,
						transform: `translate(${Z.value.drawingArea.left + Z.value.slotSize * (n + 1) - Z.value.slotSize / 2}, ${Z.value.drawingArea.bottom + z.value.xyLabelsXFontSize + 6}), rotate(${z.value.xyPeriodLabelsRotation})`,
						innerHTML: O(t)({
							content: String(e),
							fontSize: z.value.xyLabelsXFontSize,
							fill: z.value.color,
							x: 0,
							y: 0
						})
					}, null, 8, Rt)) : (C(), _("text", {
						key: 0,
						class: "vue-data-ui-time-label",
						"font-size": z.value.xyLabelsXFontSize,
						"text-anchor": z.value.xyPeriodLabelsRotation > 0 ? "start" : z.value.xyPeriodLabelsRotation < 0 ? "end" : "middle",
						fill: z.value.color,
						transform: `translate(${Z.value.drawingArea.left + Z.value.slotSize * (n + 1) - Z.value.slotSize / 2}, ${Z.value.drawingArea.bottom + z.value.xyLabelsXFontSize + 6}), rotate(${z.value.xyPeriodLabelsRotation})`
					}, D(e), 9, Lt))])) : g("", !0)], 64))), 256))], 512)])) : g("", !0),
					v("g", zt, [(C(!0), _(m, null, T(Z.value.dataset, (e, t) => (C(), _("g", {
						key: `serie_${e.id}`,
						class: "line-plot-series"
					}, [z.value.lineSmooth ? (C(), _(m, { key: 0 }, [v("path", {
						ref_for: !0,
						ref_key: "pathWrapper",
						ref: Ln,
						d: `M ${O(i)(e.coordinates)}`,
						stroke: z.value.backgroundColor,
						"stroke-width": z.value.lineStrokeWidth + 1,
						"stroke-linecap": "round",
						fill: "none",
						class: b({ "vue-data-ui-transition": O(B) })
					}, null, 10, Bt), v("path", {
						ref_for: !0,
						ref_key: "pathTop",
						ref: Rn,
						d: `M ${O(i)(e.coordinates)}`,
						stroke: e.color,
						"stroke-width": z.value.lineStrokeWidth,
						"stroke-linecap": "round",
						fill: "none",
						class: b({ "vue-data-ui-transition": O(B) }),
						style: S({ transition: O(rr) ? void 0 : "all 0.2s ease-in-out" })
					}, null, 14, Vt)], 64)) : (C(), _(m, { key: 1 }, [v("path", {
						ref_for: !0,
						ref_key: "pathWrapper",
						ref: Ln,
						d: `M ${e.linePath}`,
						stroke: z.value.backgroundColor,
						"stroke-width": z.value.lineStrokeWidth + 1,
						"stroke-linecap": "round",
						fill: "none",
						class: b({ "vue-data-ui-transition": O(B) })
					}, null, 10, Ht), v("path", {
						ref_for: !0,
						ref_key: "pathTop",
						ref: Rn,
						d: `M ${e.linePath}`,
						stroke: e.color,
						"stroke-width": z.value.lineStrokeWidth,
						"stroke-linecap": "round",
						fill: "none",
						class: b({ "vue-data-ui-transition": O(B) })
					}, null, 10, Ut)], 64)), (C(!0), _(m, null, T(e.coordinates, (t, n) => (C(), _("circle", {
						key: `dp_${e.id}_${n + X.value.start}`,
						cx: t.x,
						cy: O(l)(t.y),
						r: 3,
						fill: e.color,
						stroke: z.value.backgroundColor,
						"stroke-width": "0.5",
						class: b({
							"vue-ui-quick-chart-plot": !0,
							"vue-data-ui-transition": O(B)
						})
					}, null, 10, Wt))), 128))]))), 128))]),
					z.value.showDataLabels ? (C(), _("g", Gt, [(C(!0), _(m, null, T(Z.value.dataset, (e, t) => (C(), _(m, { key: `ds_${e.id}` }, [(C(!0), _(m, null, T(e.coordinates, (t, n) => (C(), _("text", {
						class: b(["vue-ui-quick-chart-label", { "vue-data-ui-transition": O(B) }]),
						key: `plot_${e.id}_${n + X.value.start}`,
						"text-anchor": "middle",
						"font-size": z.value.dataLabelFontSize,
						fill: e.color,
						transform: `translate(${t.x}, ${O(l)(t.y) - z.value.dataLabelFontSize / 2})`
					}, D(O(f)(z.value.formatter, O(l)(t.value), O(s)({
						p: z.value.valuePrefix,
						v: O(l)(t.value),
						s: z.value.valueSuffix,
						r: z.value.dataLabelRoundingValue
					}), {
						datapoint: t,
						seriesIndex: n
					})), 11, Kt))), 128))], 64))), 128))])) : g("", !0),
					ur.value ? (C(), _("g", qt, [(C(!0), _(m, null, T(Z.value.extremes.maxSeries, (e, t) => (C(), _("rect", {
						x: Z.value.drawingArea.left + t * Z.value.slotSize,
						y: Z.value.drawingArea.top,
						height: Z.value.drawingArea.height <= 0 ? 1e-5 : Z.value.drawingArea.height,
						width: Z.value.slotSize <= 0 ? 1e-5 : Z.value.slotSize,
						fill: [I.value, J.value].includes(t) ? z.value.xyHighlighterColor : "transparent",
						style: S(`opacity:${z.value.xyHighlighterOpacity}`),
						onMouseenter: (e) => Z.value.useTooltip(t, "pointer"),
						onMouseleave: (e) => Z.value.killTooltip(t),
						onClick: (e) => Z.value.selectDatapoint(t)
					}, null, 44, Jt))), 256))])) : g("", !0)
				], 64)) : g("", !0),
				G.value === A.BAR ? (C(), _(m, { key: 5 }, [
					z.value.xyShowGrid ? (C(), _("g", Yt, [(C(!0), _(m, null, T(Q.value.yLabels, (e) => (C(), _(m, null, [e.y <= Q.value.drawingArea.bottom ? (C(), _("line", {
						key: 0,
						x1: Q.value.drawingArea.left,
						x2: Q.value.drawingArea.right,
						y1: e.y,
						y2: e.y,
						stroke: z.value.xyGridStroke,
						"stroke-width": z.value.xyGridStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, Xt)) : g("", !0)], 64))), 256)), L.value.length < Q.value.legend.length ? (C(!0), _(m, { key: 0 }, T(Q.value.extremes.maxSeries + 1, (e, t) => (C(), _("line", {
						x1: Q.value.drawingArea.left + Q.value.slotSize * t,
						x2: Q.value.drawingArea.left + Q.value.slotSize * t,
						y1: Q.value.drawingArea.top,
						y2: Q.value.drawingArea.bottom,
						stroke: z.value.xyGridStroke,
						"stroke-width": z.value.xyGridStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, Zt))), 256)) : g("", !0)])) : g("", !0),
					z.value.xyShowAxis ? (C(), _("g", Qt, [v("line", {
						x1: Q.value.drawingArea.left,
						x2: Q.value.drawingArea.left,
						y1: Q.value.drawingArea.top,
						y2: Q.value.drawingArea.bottom,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, $t), v("line", {
						x1: Q.value.drawingArea.left,
						x2: Q.value.drawingArea.right,
						y1: isNaN(Q.value.absoluteZero) ? Q.value.drawingArea.bottom : Q.value.absoluteZero,
						y2: isNaN(Q.value.absoluteZero) ? Q.value.drawingArea.bottom : Q.value.absoluteZero,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, en)])) : g("", !0),
					z.value.xyShowScale ? (C(), _("g", {
						key: 2,
						class: "yLabels",
						ref_key: "scaleLabels",
						ref: zn
					}, [(C(!0), _(m, null, T(Q.value.yLabels, (e, t) => (C(), _(m, { key: `sl_${t}` }, [e.y <= Q.value.drawingArea.bottom ? (C(), _("path", {
						key: 0,
						class: b({ "vue-data-ui-transition": O(B) }),
						d: `M${e.x + 4},${e.y} ${Q.value.drawingArea.left},${e.y}`,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 10, tn)) : g("", !0), e.y <= Q.value.drawingArea.bottom ? (C(), _("text", {
						key: 1,
						class: b({ "vue-data-ui-transition": O(B) }),
						transform: `translate(${e.x}, ${e.y + z.value.xyLabelsYFontSize / 3})`,
						"text-anchor": "end",
						"font-size": z.value.xyLabelsYFontSize,
						fill: z.value.color
					}, D(O(f)(z.value.formatter, e.value, O(s)({
						p: z.value.valuePrefix,
						v: e.value,
						s: z.value.valueSuffix,
						r: z.value.dataLabelRoundingValue
					}), {
						datapoint: e,
						seriesIndex: t
					})), 11, nn)) : g("", !0)], 64))), 128))], 512)) : g("", !0),
					z.value.xyShowScale && z.value.xyPeriods.length ? (C(), _("g", rn, [(C(!0), _(m, null, T(z.value.xyPeriods.slice(X.value.start, X.value.end), (e, t) => (C(), _("line", {
						x1: Q.value.drawingArea.left + Q.value.slotSize * (t + 1) - Q.value.slotSize / 2,
						x2: Q.value.drawingArea.left + Q.value.slotSize * (t + 1) - Q.value.slotSize / 2,
						y1: Q.value.drawingArea.bottom,
						y2: Q.value.drawingArea.bottom + 4,
						stroke: z.value.xyAxisStroke,
						"stroke-width": z.value.xyAxisStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, an))), 256)), v("g", {
						ref_key: "timeLabelsEls",
						ref: R
					}, [(C(!0), _(m, null, T($.value.map((e) => e.text), (e, n) => (C(), _(m, null, [!z.value.xyPeriodsShowOnlyAtModulo || z.value.xyPeriodsShowOnlyAtModulo && n % Math.floor((X.value.end - X.value.start) / Yr.value) === 0 || X.value.end - X.value.start <= Yr.value ? (C(), _("g", on, [String(e).includes("\n") ? (C(), _("text", {
						key: 1,
						class: "vue-data-ui-time-label",
						"font-size": z.value.xyLabelsXFontSize,
						"text-anchor": z.value.xyPeriodLabelsRotation > 0 ? "start" : z.value.xyPeriodLabelsRotation < 0 ? "end" : "middle",
						fill: z.value.color,
						transform: `translate(${Q.value.drawingArea.left + Q.value.slotSize * (n + 1) - Q.value.slotSize / 2}, ${Q.value.drawingArea.bottom + z.value.xyLabelsXFontSize + 6}), rotate(${z.value.xyPeriodLabelsRotation})`,
						innerHTML: O(t)({
							content: String(e),
							fontSize: z.value.xyLabelsXFontSize,
							fill: z.value.color,
							x: 0,
							y: 0
						})
					}, null, 8, cn)) : (C(), _("text", {
						key: 0,
						class: "vue-data-ui-time-label",
						"font-size": z.value.xyLabelsXFontSize,
						"text-anchor": z.value.xyPeriodLabelsRotation > 0 ? "start" : z.value.xyPeriodLabelsRotation < 0 ? "end" : "middle",
						fill: z.value.color,
						transform: `translate(${Q.value.drawingArea.left + Q.value.slotSize * (n + 1) - Q.value.slotSize / 2}, ${Q.value.drawingArea.bottom + z.value.xyLabelsXFontSize + 6}), rotate(${z.value.xyPeriodLabelsRotation})`
					}, D(e), 9, sn))])) : g("", !0)], 64))), 256))], 512)])) : g("", !0),
					v("g", ln, [(C(!0), _(m, null, T(Q.value.dataset, (e, t) => (C(), _(m, null, [(C(!0), _(m, null, T(e.coordinates, (t, n) => (C(), _("rect", {
						x: t.x,
						width: t.width <= 0 ? 1e-5 : t.width,
						height: O(l)(t.height <= 0 ? 1e-5 : t.height),
						y: O(l)(t.y),
						fill: e.color,
						stroke: z.value.backgroundColor,
						"stroke-width": z.value.barStrokeWidth,
						"stroke-linecap": "round",
						class: b({ "vue-data-ui-transition": O(B) })
					}, null, 10, un))), 256))], 64))), 256))]),
					z.value.showDataLabels ? (C(), _("g", dn, [(C(!0), _(m, null, T(Q.value.dataset, (e, t) => (C(), _(m, { key: `ds_${e.id}` }, [(C(!0), _(m, null, T(e.coordinates, (t, n) => (C(), _("text", {
						class: b({ "vue-data-ui-transition": O(B) }),
						key: `plot_${n + X.value.start}`,
						transform: `translate(${t.x + t.width / 2}, ${O(l)(t.y) - z.value.dataLabelFontSize / 2})`,
						"text-anchor": "middle",
						"font-size": z.value.dataLabelFontSize,
						fill: e.color
					}, D(O(f)(z.value.formatter, O(l)(t.value), O(s)({
						p: z.value.valuePrefix,
						v: O(l)(t.value),
						s: z.value.valueSuffix,
						r: z.value.dataLabelRoundingValue
					}), {
						datapoint: t,
						seriesIndex: n
					})), 11, fn))), 128))], 64))), 128))])) : g("", !0),
					ur.value && L.value.length < Q.value.legend.length ? (C(), _("g", pn, [(C(!0), _(m, null, T(Q.value.extremes.maxSeries, (e, t) => (C(), _("rect", {
						x: Q.value.drawingArea.left + t * Q.value.slotSize,
						y: Q.value.drawingArea.top,
						height: Q.value.drawingArea.height <= 0 ? 1e-5 : Q.value.drawingArea.height,
						width: Q.value.slotSize <= 0 ? 1e-5 : Q.value.slotSize,
						fill: [I.value, J.value].includes(t) ? z.value.xyHighlighterColor : "transparent",
						style: S(`opacity:${z.value.xyHighlighterOpacity}`),
						onMouseenter: (e) => Q.value.useTooltip(t, "pointer"),
						onMouseleave: (e) => Q.value.killTooltip(t),
						onClick: (e) => Q.value.selectDatapoint(t)
					}, null, 44, mn))), 256))])) : g("", !0)
				], 64)) : g("", !0),
				[A.LINE, A.BAR].includes(G.value) ? (C(), _("g", hn, [
					z.value.xAxisLabel && G.value === A.LINE ? (C(), _("g", {
						key: 0,
						ref_key: "xAxisLabel",
						ref: Bn
					}, [v("text", {
						"font-size": z.value.axisLabelsFontSize,
						fill: z.value.color,
						"text-anchor": "middle",
						x: Z.value.drawingArea.left + Z.value.drawingArea.width / 2,
						y: K.value.height - z.value.axisLabelsFontSize / 3
					}, D(z.value.xAxisLabel), 9, gn)], 512)) : g("", !0),
					z.value.xAxisLabel && G.value === A.BAR ? (C(), _("g", {
						key: 1,
						ref_key: "xAxisLabel",
						ref: Bn
					}, [v("text", {
						"font-size": z.value.axisLabelsFontSize,
						fill: z.value.color,
						"text-anchor": "middle",
						x: Q.value.drawingArea.left + Q.value.drawingArea.width / 2,
						y: K.value.height - z.value.axisLabelsFontSize / 3
					}, D(z.value.xAxisLabel), 9, _n)], 512)) : g("", !0),
					z.value.yAxisLabel && G.value === A.LINE ? (C(), _("g", {
						key: 2,
						ref_key: "yAxisLabel",
						ref: Vn
					}, [v("text", {
						"font-size": z.value.axisLabelsFontSize,
						fill: z.value.color,
						transform: `translate(${z.value.axisLabelsFontSize}, ${Z.value.drawingArea.top + Z.value.drawingArea.height / 2}) rotate(-90)`,
						"text-anchor": "middle"
					}, D(z.value.yAxisLabel), 9, vn)], 512)) : g("", !0),
					z.value.yAxisLabel && G.value === A.BAR ? (C(), _("g", {
						key: 3,
						ref_key: "yAxisLabel",
						ref: Vn
					}, [v("text", {
						"font-size": z.value.axisLabelsFontSize,
						fill: z.value.color,
						transform: `translate(${z.value.axisLabelsFontSize}, ${Q.value.drawingArea.top + Q.value.drawingArea.height / 2}) rotate(-90)`,
						"text-anchor": "middle"
					}, D(z.value.yAxisLabel), 9, yn)], 512)) : g("", !0)
				])) : g("", !0)
			], 46, at)) : g("", !0), e.$slots.hint ? (C(), _("div", bn, [E(e.$slots, "hint", x(y({
				hint: z.value.a11y.translations.keyboardNavigation,
				isVisible: tr.value
			})), void 0, !0)])) : g("", !0)]),
			e.$slots.watermark ? (C(), _("div", xn, [E(e.$slots, "watermark", x(y({ isPrinting: O(gr) || O(_r) })), void 0, !0)])) : g("", !0),
			[A.BAR, A.LINE].includes(G.value) && z.value.zoomXy && W.value.maxSeriesLength > 1 ? (C(), _("div", {
				key: `slicer_${Fn.value}`,
				ref_key: "quickChartSlicer",
				ref: kn
			}, [(C(), Me(Ee, {
				ref_key: "slicerComponent",
				ref: Ir,
				key: `slicer_${Fn.value}`,
				timeLabels: $.value,
				background: z.value.zoomColor,
				borderColor: z.value.backgroundColor,
				fontSize: z.value.zoomFontSize,
				useResetSlot: z.value.zoomUseResetSlot,
				textColor: z.value.color,
				inputColor: z.value.zoomColor,
				selectColor: z.value.zoomHighlightColor,
				max: W.value.maxSeriesLength,
				min: 0,
				valueStart: X.value.start,
				valueEnd: X.value.end,
				smoothMinimap: z.value.zoomMinimap.smooth,
				minimapSelectedColor: z.value.zoomMinimap.selectedColor,
				minimapSelectedColorOpacity: z.value.zoomMinimap.selectedColorOpacity,
				minimapSelectionRadius: z.value.zoomMinimap.selectionRadius,
				minimapLineColor: z.value.zoomMinimap.lineColor,
				minimap: zr.value,
				minimapIndicatorColor: z.value.zoomMinimap.indicatorColor,
				verticalHandles: z.value.zoomMinimap.verticalHandles,
				minimapSelectedIndex: J.value,
				start: X.value.start,
				"onUpdate:start": n[0] ||= (e) => X.value.start = e,
				end: X.value.end,
				"onUpdate:end": n[1] ||= (e) => X.value.end = e,
				refreshStartPoint: z.value.zoomStartIndex === null ? 0 : z.value.zoomStartIndex,
				refreshEndPoint: z.value.zoomEndIndex === null ? W.value.maxSeriesLength : z.value.zoomEndIndex + 1,
				enableRangeHandles: z.value.zoomEnableRangeHandles,
				enableSelectionDrag: z.value.zoomEnableSelectionDrag,
				minimapCompact: z.value.zoomMinimap.compact,
				minimapMerged: z.value.zoomMinimap.merged,
				allMinimaps: qr.value,
				minimapFrameColor: z.value.zoomMinimap.frameColor,
				additionalMinimapHeight: z.value.zoomMinimap.additionalHeight,
				handleType: z.value.zoomMinimap.handleType,
				handleWidth: z.value.zoomMinimap.handleWidth,
				handleBorderWidth: z.value.zoomMinimap.handleBorderWidth,
				handleIconColor: z.value.zoomMinimap.handleIconColor,
				handleBorderColor: z.value.zoomMinimap.handleBorderColor,
				handleFill: z.value.zoomMinimap.handleFill,
				focusOnDrag: z.value.zoomFocusOnDrag,
				focusRangeRatio: z.value.zoomFocusRangeRatio,
				maxWidth: z.value.zoomMaxWidth,
				minimapLeftInsetRatio: Gr.value,
				minimapRightInsetRatio: Kr.value,
				onReset: Fr,
				onTrapMouse: Nr
			}, {
				"reset-action": k(({ reset: t }) => [E(e.$slots, "reset-action", x(y({ reset: t })), void 0, !0)]),
				_: 3
			}, 8, /* @__PURE__ */ "timeLabels.background.borderColor.fontSize.useResetSlot.textColor.inputColor.selectColor.max.valueStart.valueEnd.smoothMinimap.minimapSelectedColor.minimapSelectedColorOpacity.minimapSelectionRadius.minimapLineColor.minimap.minimapIndicatorColor.verticalHandles.minimapSelectedIndex.start.end.refreshStartPoint.refreshEndPoint.enableRangeHandles.enableSelectionDrag.minimapCompact.minimapMerged.allMinimaps.minimapFrameColor.additionalMinimapHeight.handleType.handleWidth.handleBorderWidth.handleIconColor.handleBorderColor.handleFill.focusOnDrag.focusRangeRatio.maxWidth.minimapLeftInsetRatio.minimapRightInsetRatio".split(".")))])) : g("", !0),
			v("div", { id: `legend-bottom-${P.value}` }, null, 8, Sn),
			In.value && z.value.showLegend ? (C(), Me(je, {
				key: 7,
				to: z.value.legendPosition === "top" ? `#legend-top-${P.value}` : `#legend-bottom-${P.value}`
			}, [z.value.showLegend ? (C(), _("div", {
				key: 0,
				ref_key: "quickChartLegend",
				ref: On,
				class: "vue-ui-quick-chart-legend",
				style: S(`background:transparent;color:${z.value.color}`)
			}, [
				(Y.value?.legend?.length > 2 || Z.value?.legend?.length > 2 || Q.value?.legend?.length > 2) && z.value.showLegendSelectAllToggle && !O(rr) ? (C(), Me(De, {
					key: 0,
					backgroundColor: z.value.legendSelectAllToggleBackgroundColor,
					color: z.value.legendSelectAllToggleColor,
					fontSize: z.value.legendFontSize,
					checked: L.value.length > 0,
					isCursorPointer: $n.value,
					onToggle: Dr
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : g("", !0),
				G.value === A.DONUT ? (C(!0), _(m, { key: 1 }, T(Y.value.legend, (t, n) => (C(), _("div", {
					class: "vue-ui-quick-chart-legend-item",
					onClick: (e) => Mr(t, Y.value.dataset),
					style: S(`cursor: ${Y.value.legend.length > 1 && $n.value ? "pointer" : "default"}; opacity:${L.value.includes(t.id) ? "0.5" : "1"}`),
					role: "button",
					tabindex: "0",
					onKeydown: (e) => {
						di(e, () => {
							Mr(t, Y.value.dataset);
						});
					}
				}, [z.value.useCustomLegend ? E(e.$slots, "legend", Ie({ ref_for: !0 }, { legend: {
					...t,
					isSegregated: L.value.includes(t.id),
					segregate: () => {
						Mr(t, Y.value.dataset);
					}
				} }), void 0, !0, 0) : (C(), _(m, { key: 1 }, [
					Pe(O(Ke), {
						name: z.value.legendIcon,
						stroke: t.color,
						size: z.value.legendIconSize
					}, null, 8, [
						"name",
						"stroke",
						"size"
					]),
					v("span", { style: S(`font-size:${z.value.legendFontSize}px`) }, D(t.name), 5),
					v("span", { style: S(`font-size:${z.value.legendFontSize}px;font-variant-numeric:tabular-nums`) }, D(L.value.includes(t.id) ? "-" : O(f)(z.value.formatter, t.absoluteValue, O(s)({
						p: z.value.valuePrefix,
						v: t.absoluteValue,
						s: z.value.valueSuffix,
						r: z.value.dataLabelRoundingValue
					}), {
						datapoint: t,
						seriesIndex: n
					})), 5),
					L.value.includes(t.id) ? (C(), _("span", {
						key: 0,
						style: S(`font-size:${z.value.legendFontSize}px`)
					}, " ( - % ) ", 4)) : jr.value ? (C(), _("span", {
						key: 1,
						style: S(`font-size:${z.value.legendFontSize}px; font-variant-numeric: tabular-nums;`)
					}, " ( - % ) ", 4)) : (C(), _("span", {
						key: 2,
						style: S(`font-size:${z.value.legendFontSize}px; font-variant-numeric: tabular-nums;`)
					}, " (" + D(O(s)({
						v: t.value / Y.value.total * 100,
						s: "%",
						r: z.value.dataLabelRoundingPercentage
					})) + ") ", 5))
				], 64))], 44, Cn))), 256)) : g("", !0),
				G.value === A.LINE ? (C(!0), _(m, { key: 2 }, T(Z.value.legend, (t, n) => (C(), _("div", {
					class: "vue-ui-quick-chart-legend-item",
					onClick: (e) => {
						Or(t.id, Z.value.legend.length - 1), U("selectLegend", Z.value.dataset);
					},
					style: S(`cursor: ${Z.value.legend.length > 1 && $n.value ? "pointer" : "default"}; opacity:${L.value.includes(t.id) ? "0.5" : "1"}`),
					role: "button",
					tabindex: "0",
					onKeydown: (e) => {
						di(e, () => {
							Or(t.id, Z.value.legend.length - 1), U("selectLegend", Z.value.dataset);
						});
					}
				}, [z.value.useCustomLegend ? E(e.$slots, "legend", Ie({ ref_for: !0 }, { legend: {
					...t,
					isSegregated: L.value.includes(t.id),
					segregate: () => {
						Or(t.id, Z.value.legend.length - 1), U("selectLegend", Z.value.dataset);
					}
				} }), void 0, !0, 0) : (C(), _(m, { key: 1 }, [Pe(O(Ke), {
					name: z.value.legendIcon,
					stroke: t.color,
					size: z.value.legendIconSize
				}, null, 8, [
					"name",
					"stroke",
					"size"
				]), v("span", { style: S(`font-size:${z.value.legendFontSize}px`) }, D(t.name), 5)], 64))], 44, wn))), 256)) : g("", !0),
				G.value === A.BAR ? (C(!0), _(m, { key: 3 }, T(Q.value.legend, (t, n) => (C(), _("div", {
					class: "vue-ui-quick-chart-legend-item",
					onClick: (e) => {
						Or(t.id, Q.value.legend.length - 1), U("selectLegend", Q.value.dataset);
					},
					style: S(`cursor: ${Q.value.legend.length > 1 && $n.value ? "pointer" : "default"}; opacity:${L.value.includes(t.id) ? "0.5" : "1"}`),
					role: "button",
					tabindex: "0",
					onKeydown: (e) => {
						di(e, () => {
							Or(t.id, Q.value.legend.length - 1), U("selectLegend", Q.value.dataset);
						});
					}
				}, [z.value.useCustomLegend ? E(e.$slots, "legend", Ie({ ref_for: !0 }, { legend: {
					...t,
					isSegregated: L.value.includes(t.id),
					segregate: () => {
						Or(t.id, Q.value.legend.length - 1), U("selectLegend", Q.value.dataset);
					}
				} }), void 0, !0, 0) : (C(), _(m, { key: 1 }, [Pe(O(Ke), {
					name: z.value.legendIcon,
					stroke: t.color,
					size: z.value.legendIconSize
				}, null, 8, [
					"name",
					"stroke",
					"size"
				]), v("span", { style: S(`font-size:${z.value.legendFontSize}px`) }, D(t.name), 5)], 64))], 44, Tn))), 256)) : g("", !0)
			], 4)) : g("", !0)], 8, ["to"])) : g("", !0),
			e.$slots.source ? (C(), _("div", {
				key: 8,
				ref_key: "source",
				ref: Mn,
				dir: "auto"
			}, [E(e.$slots, "source", {}, void 0, !0)], 512)) : g("", !0),
			Pe(O(Xe), {
				teleportTo: z.value.tooltipTeleportTo,
				show: xr.value.showTooltip && F.value,
				backgroundColor: z.value.backgroundColor,
				color: z.value.color,
				borderRadius: z.value.tooltipBorderRadius,
				borderColor: z.value.tooltipBorderColor,
				borderWidth: z.value.tooltipBorderWidth,
				fontSize: z.value.tooltipFontSize,
				backgroundOpacity: z.value.tooltipBackgroundOpacity,
				position: z.value.tooltipPosition,
				offsetX: z.value.tooltipOffsetX,
				offsetY: z.value.tooltipOffsetY,
				parent: N.value,
				content: jn.value,
				isFullscreen: Xr.value,
				isCustom: O(ce)(z.value.tooltipCustomFormat),
				smooth: z.value.tooltipSmooth,
				smoothForce: z.value.tooltipSmoothForce,
				smoothSnapThreshold: z.value.tooltipSmoothSnapThreshold,
				backdropFilter: z.value.tooltipBackdropFilter,
				isA11yMode: H.value === "keyboard",
				a11yPosition: er.value
			}, {
				"tooltip-before": k(() => [E(e.$slots, "tooltip-before", x(y({ ...An.value })), void 0, !0)]),
				tooltip: k(() => [E(e.$slots, "tooltip", x(y({ ...An.value })), void 0, !0)]),
				"tooltip-after": k(() => [E(e.$slots, "tooltip-after", x(y({ ...An.value })), void 0, !0)]),
				_: 3
			}, 8, [
				"teleportTo",
				"show",
				"backgroundColor",
				"color",
				"borderRadius",
				"borderColor",
				"borderWidth",
				"fontSize",
				"backgroundOpacity",
				"position",
				"offsetX",
				"offsetY",
				"parent",
				"content",
				"isFullscreen",
				"isCustom",
				"smooth",
				"smoothForce",
				"smoothSnapThreshold",
				"backdropFilter",
				"isA11yMode",
				"a11yPosition"
			]),
			E(e.$slots, "skeleton", {}, () => [O(rr) ? (C(), Me(he, { key: 0 })) : g("", !0)], !0)
		], 46, tt)) : (C(), _("div", En, [Pe(O(Ke), {
			name: "circleCancel",
			stroke: "red"
		}), n[5] ||= v("span", null, "Dataset is not processable", -1)]));
	}
}, [["__scopeId", "data-v-4bec8cdd"]]);
//#endregion
export { N as n, Dn as t };
