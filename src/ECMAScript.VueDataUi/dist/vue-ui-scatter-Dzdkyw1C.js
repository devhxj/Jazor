import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, F as i, Gt as a, Jt as o, Kt as s, P as c, Pt as l, S as u, Tt as d, X as f, _ as p, _t as m, b as h, ct as ee, i as g, jt as te, pt as ne, q as re, t as ie, tt as ae, v as oe, w as se, xt as ce } from "./lib-Bttd6u5E.js";
import { n as le, t as ue } from "./useHints-Dq_w2E8B.js";
import { t as de } from "./useConfig-DlNpz6P8.js";
import { t as fe } from "./usePrinter-DN5bYhTG.js";
import { n as pe, t as me } from "./BaseScanner-DZvpgOjM.js";
import { t as he } from "./useNestedProp-vPNvh7rV.js";
import { t as ge } from "./useThemeCheck-C43Tcqmk.js";
import { t as _e } from "./useChartExport-DNiwdPmb.js";
import { t as ve } from "./useTransitions-g_zBREk2.js";
import { t as ye } from "./img-Bnokohej.js";
import { n as be } from "./Title-BE3qg9xl.js";
import { t as xe } from "./Shape-C21CMlWS.js";
import { t as Se } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Ce, t as we } from "./useResponsive-ZtArZtUf.js";
import { t as Te } from "./DefGrad-DVBqDjhO.js";
import { t as Ee } from "./BaseLegendToggle-DZVucLnv.js";
import { t as De } from "./A11yDataTable-DdRsVULz.js";
import { t as Oe } from "./useUserOptionState-DK-_1ddE.js";
import { t as ke } from "./useChartAccessibility-DYqac8yF.js";
import { t as Ae } from "./Legend-CQxUgOd-.js";
import { t as je } from "./vue_ui_scatter-I0POnicu.js";
import { Fragment as _, Teleport as Me, computed as v, createBlock as y, createCommentVNode as b, createElementBlock as x, createElementVNode as S, createSlots as Ne, createTextVNode as Pe, createVNode as Fe, defineAsyncComponent as Ie, guardReactiveProps as C, mergeProps as Le, nextTick as Re, normalizeClass as ze, normalizeProps as w, normalizeStyle as T, onBeforeUnmount as Be, onMounted as Ve, openBlock as E, ref as D, renderList as O, renderSlot as k, resolveDynamicComponent as He, shallowRef as Ue, toDisplayString as A, toRefs as We, unref as j, watch as Ge, withCtx as M } from "vue";
//#region src/components/vue-ui-scatter.vue
var Ke = /* @__PURE__ */ e({ default: () => _n }), qe = ["id"], Je = ["id"], Ye = {
	key: 0,
	class: "sr-only",
	"aria-live": "polite",
	"aria-atomic": "true"
}, Xe = ["id"], Ze = { style: { position: "relative" } }, Qe = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], $e = ["width", "height"], et = { key: 1 }, tt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], nt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], rt = { key: 2 }, it = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], at = ["opacity"], ot = [
	"stroke",
	"stroke-width",
	"d"
], st = [
	"transform",
	"font-size",
	"fill",
	"stroke"
], ct = { key: 4 }, lt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], ut = ["opacity"], dt = [
	"d",
	"stroke",
	"stroke-width"
], ft = [
	"transform",
	"font-size",
	"fill",
	"stroke"
], pt = { key: 6 }, mt = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width",
	"rx"
], ht = [
	"x",
	"y",
	"width",
	"height",
	"onMouseenter"
], gt = {
	key: 2,
	style: { "pointer-events": "none" }
}, _t = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"fill-opacity"
], vt = [
	"x1",
	"x2",
	"y2",
	"stroke",
	"stroke-dasharray",
	"stroke-width"
], yt = [
	"x1",
	"x2",
	"y2",
	"stroke",
	"stroke-dasharray",
	"stroke-width"
], bt = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke",
	"stroke-width",
	"rx"
], xt = [
	"x",
	"y",
	"width",
	"height",
	"onMouseenter"
], St = {
	key: 2,
	style: { "pointer-events": "none" }
}, Ct = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"fill-opacity"
], wt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-dasharray",
	"stroke-width"
], Tt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-dasharray",
	"stroke-width"
], Et = {
	key: 0,
	style: { "pointer-events": "none" }
}, Dt = [
	"d",
	"stroke",
	"stroke-width"
], Ot = [
	"d",
	"stroke",
	"stroke-width"
], kt = [
	"d",
	"stroke",
	"stroke-width"
], At = [
	"d",
	"stroke",
	"stroke-width"
], jt = { key: 7 }, Mt = [
	"points",
	"fill",
	"stroke-width",
	"stroke-dasharray",
	"stroke"
], Nt = {
	key: 0,
	class: "vue-ui-scatter-datapoint"
}, Pt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width",
	"onMouseover",
	"onMouseleave",
	"onClick"
], Ft = { key: 1 }, It = [
	"transform",
	"font-size",
	"fill",
	"onMouseover",
	"onMouseleave",
	"onClick"
], Lt = ["clip-path"], Rt = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"stroke-opacity"
], zt = {
	key: 0,
	style: { "pointer-events": "none" }
}, Bt = [
	"x",
	"y",
	"width",
	"height"
], Vt = {
	key: 0,
	style: { "pointer-events": "none !important" }
}, Ht = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Ut = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Wt = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight",
	"text-anchor"
], Gt = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], Kt = [
	"x",
	"y",
	"font-size",
	"fill"
], qt = [
	"x",
	"y",
	"font-size",
	"fill"
], Jt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Yt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Xt = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight",
	"text-anchor"
], Zt = [
	"id",
	"transform",
	"font-size",
	"font-weight",
	"fill"
], Qt = [
	"font-size",
	"fill",
	"transform"
], $t = [
	"transform",
	"font-size",
	"fill"
], en = [
	"x",
	"y",
	"font-size",
	"fill"
], tn = [
	"x",
	"y",
	"font-size",
	"fill"
], nn = [
	"font-size",
	"font-weight",
	"fill",
	"x",
	"y"
], rn = ["id"], an = [
	"x",
	"y",
	"width",
	"height"
], on = {
	key: 14,
	style: { pointerEvents: "none" }
}, sn = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-dasharray",
	"stroke",
	"stroke-width",
	"clip-path"
], cn = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], ln = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, un = {
	key: 6,
	class: "vue-data-ui-watermark"
}, dn = ["id"], fn = ["onClick"], pn = {
	key: 0,
	style: {
		width: "100%",
		display: "flex",
		"align-items": "center",
		"justify-content": "center"
	}
}, mn = {
	viewBox: "0 0 20 20",
	height: "20",
	width: "20",
	style: {
		overflow: "hidden",
		background: "transparent"
	}
}, hn = { key: 0 }, gn = ["innerHTML"], _n = /*#__PURE__*/ Se({
	__name: "vue-ui-scatter",
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
		}
	},
	emits: ["copyAlt", "selectLegend"],
	setup(e, { expose: Se, emit: Ke }) {
		let _n = Ie(() => import("./Tooltip-DhjyfHwz.js")), vn = Ie(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), yn = Ie(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), bn = Ie(() => import("./DataTable-BbKgJ5UI.js")), xn = Ie(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Sn = Ie(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Cn = Ie(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), wn = Ie(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_scatter: Tn } = de(), { isThemeValid: En, warnInvalidTheme: Dn } = ge(), N = e, On = Ke, kn = v(() => !!N.dataset && N.dataset.length), P = D(re()), An = D(!1), jn = D(""), Mn = D(0), F = D(null), Nn = D(null), Pn = D(null), Fn = D(null), In = D(null), Ln = D(0), Rn = D(0), zn = D(0), I = D([]), Bn = D(!1), Vn = D(null), Hn = D(null), Un = D(null), Wn = D(null), Gn = D(null), Kn = D(null), qn = D(null), Jn = D(null), Yn = D(null), Xn = D(null), Zn = D(null), Qn = D({
			x: 0,
			y: 0
		}), $n = D("pointer"), er = D(!1), tr = D(null), L = D(fr());
		le({
			config: () => L.value,
			dataset: () => N.dataset,
			component: "VueUiScatter",
			rules: [ue.emptyArray, {
				test: (e) => e.length > 6,
				message: [
					"👀 The number of series (clusters) is > 6. Consider:",
					"",
					"",
					"▶️ Using filters to let users choose a maximum number of series (clusters) to display."
				]
			}]
		});
		let { transitionEnabled: nr } = ve({
			config: () => L.value.transitions,
			dataset: () => N.dataset
		}), rr = v(() => L.value.userOptions.useCursorPointer);
		function ir(e = 100, t = .8, n = {}) {
			let { meanX: r = 0, sdX: i = 1, meanY: a = 0, sdY: o = 1, seed: s } = n, c = (s ?? Math.floor(Math.random() * 2 ** 31)) >>> 0, l = () => (c = c * 1664525 + 1013904223 >>> 0, c / 2 ** 32), u = () => {
				let e = 0, t = 0;
				for (; e === 0;) e = l();
				for (; t === 0;) t = l();
				return Math.sqrt(-2 * Math.log(e)) * Math.cos(2 * Math.PI * t);
			}, d = e / 2, f = Array.from({ length: d }, u), p = Array.from({ length: d }, u), m = (e) => e.reduce((e, t) => e + t, 0) / e.length, h = m(f), ee = m(p);
			for (let e = 0; e < d; e += 1) f[e] -= h, p[e] -= ee;
			let g = (e, t) => e.reduce((e, n, r) => e + n * t[r], 0), te = (e) => g(e, e), ne = g(p, f) / te(f), re = p.map((e, t) => e - ne * f[t]), ie = te(f) / d, ae = te(re) / d, oe = Math.sqrt((1 - t * t) * ie / ae), se = f.map((e, n) => t * e + oe * re[n]), ce = f.concat(f.map((e) => -e)), le = se.concat(se.map((e) => -e)), ue = (e) => Math.sqrt(te(e) / e.length), de = (e, t, n) => {
				let r = ue(e);
				return e.map((e) => n + (r ? e / r * t : 0));
			}, fe = de(ce, i, r), pe = de(le, o, a);
			for (let e = fe.length - 1; e > 0; --e) {
				let t = Math.floor(l() * (e + 1));
				[fe[e], fe[t]] = [fe[t], fe[e]], [pe[e], pe[t]] = [pe[t], pe[e]];
			}
			return fe.map((e, t) => ({
				x: e,
				y: pe[t]
			}));
		}
		let ar = v(() => o({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				useCssAnimation: !1,
				style: {
					backgroundColor: "#99999930",
					layout: {
						axis: { stroke: "#6A6A6A" },
						correlation: { label: { show: !1 } },
						dataLabels: {
							xAxis: { show: !1 },
							yAxis: { show: !1 }
						},
						marginalBars: { fill: "#99999960" },
						padding: {
							top: 12,
							right: 12,
							bottom: 12,
							left: 12
						},
						plots: { stroke: "#6A6A6A" }
					},
					legend: { backgroundColor: "transparent" }
				}
			},
			userConfig: L.value.skeletonConfig ?? {}
		})), { loading: or, FINAL_DATASET: sr, manualLoading: cr } = pe({
			...We(N),
			FINAL_CONFIG: L,
			prepareConfig: fr,
			skeletonDataset: N.config?.skeletonDataset ?? [{
				name: "",
				color: "#999999",
				values: ir(100, .5, { seed: 42 })
			}],
			skeletonConfig: o({
				defaultConfig: L.value,
				userConfig: ar.value
			})
		}), { userOptionsVisible: lr, setUserOptionsVisibility: ur, keepUserOptionState: dr } = Oe({ config: L.value }), { svgRef: R } = ke({ config: L.value.style.title });
		function fr() {
			let e = he({
				userConfig: N.config,
				defaultConfig: Tn
			}), t = e.theme;
			if (!t) return e;
			if (!En.value(e)) return Dn(e), e;
			let n = he({
				userConfig: je[t] || N.config,
				defaultConfig: e
			}), r = he({
				userConfig: N.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : s[t] || l
			};
		}
		Ge(() => N.config, (e) => {
			or.value || (L.value = fr()), lr.value = !L.value.userOptions.showOnChartHover, hr(), Ln.value += 1, Rn.value += 1, zn.value += 1, B.value.showTable = L.value.table.show, B.value.showTooltip = L.value.style.tooltip.show, mr.value && L.value.usePerformanceMode && console.warn("VueUiScatter : You are using performance mode\n\n- downsampling is disabled in this mode, all plots are rendered\n\n- plot significance is not active in this mode (all plots have the same opacity) \n\n- Depending on plot density, shapes might not display a border (stroke) to avoid fuzziness \n\nℹ️ To remove this warning, set config.debug to false.");
		}, { deep: !0 }), Ge(() => N.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (cr.value = !1);
		}, { deep: !0 });
		let z = Ue(null), pr = Ue(null);
		Ve(async () => {
			hr(), await Re(), Bn.value = !0;
		});
		let mr = v(() => L.value.debug);
		function hr() {
			if (te(N.dataset) && (ae({
				componentName: "VueUiScatter",
				type: "dataset",
				debug: mr.value
			}), cr.value = !0), te(N.dataset) || (cr.value = L.value.loading), L.value.responsive) {
				let e = Ce(() => {
					let { width: e, height: t } = we({
						chart: F.value,
						title: L.value.style.title.text ? Nn.value : null,
						legend: L.value.style.legend.show ? Pn.value : null,
						source: Fn.value,
						noTitle: In.value
					});
					requestAnimationFrame(() => {
						V.value.width = e, V.value.height = t;
					});
				});
				z.value && (pr.value && z.value.unobserve(pr.value), z.value.disconnect()), z.value = new ResizeObserver(e), pr.value = F.value.parentNode, z.value.observe(pr.value);
			}
		}
		Be(() => {
			z.value && (pr.value && z.value.unobserve(pr.value), z.value.disconnect());
		});
		let { isPrinting: gr, isImaging: _r, generatePdf: vr, generateImage: yr } = fe({
			elementId: `vue-ui-scatter_${P.value}`,
			fileName: L.value.style.title.text || "vue-ui-scatter",
			options: L.value.userOptions.print
		}), br = v(() => L.value.userOptions.show && !L.value.style.title.text), xr = v(() => se(L.value.customPalette)), B = D({
			showTable: L.value.table.show,
			showTooltip: L.value.style.tooltip.show
		});
		Ge(L, () => {
			B.value = {
				showTable: L.value.table.show,
				showTooltip: L.value.style.tooltip.show
			};
		}, { immediate: !0 });
		let V = D({
			height: L.value.style.layout.height,
			width: L.value.style.layout.width
		}), Sr = v(() => L.value.style.layout.marginalBars.show ? L.value.style.layout.marginalBars.size + L.value.style.layout.marginalBars.offset : 0);
		function Cr() {
			let e = 0;
			if (L.value.style.layout.dataLabels.yAxis.scales.show && Yn.value) try {
				e = Array.from(Yn.value.querySelectorAll("text")).reduce((e, t) => {
					let n = t.getComputedTextLength();
					return n > e ? n : e;
				}, 0);
			} catch {
				e = 0;
			}
			let t = 0;
			if (Gn.value) try {
				t = Gn.value.getBBox().width;
			} catch {
				t = 0;
			}
			return Math.max(t, e ? e + 12 : 0);
		}
		function wr() {
			let e = 0;
			if (Kn.value) try {
				let t = Kn.value.getBBox(), n = t.x + t.width - V.value.width;
				e = n > 0 ? n + 6 : 0;
			} catch {
				e = 0;
			}
			let t = 0;
			if (L.value.style.layout.dataLabels.xAxis.scales.show && Xn.value) try {
				t = Array.from(Xn.value.querySelectorAll("text")).reduce((e, t) => {
					let n = t.getBBox(), r = n.x + n.width - V.value.width, i = r > 0 ? r + 6 : 0;
					return i > e ? i : e;
				}, 0);
			} catch {
				t = 0;
			}
			return e > t ? e : t;
		}
		let H = v(() => {
			let e = 0, t = 0, n = 0;
			if (e = Cr(), t = wr(), Jn.value) try {
				n = Jn.value.getBBox().height + 6;
			} catch {
				n = 0;
			}
			return {
				top: L.value.style.layout.padding.top + Sr.value + L.value.style.layout.dataLabels.yAxis.fontSize * 2,
				right: V.value.width - L.value.style.layout.padding.right - Sr.value - 6 - t,
				bottom: V.value.height - L.value.style.layout.padding.bottom - n,
				left: L.value.style.layout.padding.left + e,
				height: V.value.height - L.value.style.layout.padding.top - L.value.style.layout.padding.bottom - Sr.value - n - L.value.style.layout.dataLabels.yAxis.fontSize * 2,
				width: V.value.width - L.value.style.layout.padding.left - L.value.style.layout.padding.right - Sr.value - e - t - 6
			};
		}), Tr = v(() => {
			mr.value && sr.value.forEach((e, t) => {
				ne({
					datasetObject: e,
					requiredAttributes: ["values"]
				}).forEach((e) => {
					ae({
						componentName: "VueUiScatter",
						type: "datasetSerieAttribute",
						property: e,
						index: t
					});
				}), e.values && e.values.forEach((e, n) => {
					ne({
						datasetObject: e,
						requiredAttributes: ["x", "y"]
					}).forEach((e) => {
						ae({
							componentName: "VueUiScatter",
							type: "datasetSerieAttribute",
							property: `values.${e}`,
							index: `${t} - ${n}`
						});
					});
				});
			});
			let e = Math.min(...G.value.filter((e) => !I.value.includes(e.id)).flatMap((e) => e.values.map((e) => e.x))), t = Math.max(...G.value.filter((e) => !I.value.includes(e.id)).flatMap((e) => e.values.map((e) => e.x))), n = Math.min(...G.value.filter((e) => !I.value.includes(e.id)).flatMap((e) => e.values.map((e) => e.y))), r = Math.max(...G.value.filter((e) => !I.value.includes(e.id)).flatMap((e) => e.values.map((e) => e.y)));
			return {
				xMin: e < 0 ? e : 0,
				xMax: t,
				yMin: n < 0 ? n : 0,
				yMax: r
			};
		}), Er = v(() => {
			let e = L.value.style.layout.axis;
			return {
				xMin: e.xMin !== null && e.xMin !== void 0 ? e.xMin : Tr.value.xMin,
				xMax: e.xMax !== null && e.xMax !== void 0 ? e.xMax : Tr.value.xMax,
				yMin: e.yMin !== null && e.yMin !== void 0 ? e.yMin : Tr.value.yMin,
				yMax: e.yMax !== null && e.yMax !== void 0 ? e.yMax : Tr.value.yMax
			};
		});
		function Dr(e) {
			return Number.isFinite(e) ? e < 2 ? 2 : Math.trunc(e) : 5;
		}
		function Or(e) {
			return e.roundingValue !== void 0 && e.roundingValue !== null ? e.roundingValue : e.rounding !== void 0 && e.rounding !== null ? e.rounding : 0;
		}
		function kr({ minimum: e, maximum: t, stepCount: n, useNiceScale: r }) {
			let i = Dr(n);
			if (e === t) {
				let n = t + (e === 0 ? 1 : Math.abs(e) * .01);
				return (r ? p : oe)(e, n, i);
			}
			return (r ? p : oe)(e, t, i);
		}
		let Ar = v(() => {
			let e = L.value.style.layout.dataLabels.xAxis;
			return kr({
				minimum: Er.value.xMin,
				maximum: Er.value.xMax,
				stepCount: e.scales.steps,
				useNiceScale: e.scales.useNiceScale
			});
		}), jr = v(() => {
			let e = L.value.style.layout.dataLabels.yAxis;
			return kr({
				minimum: Er.value.yMin,
				maximum: Er.value.yMax,
				stepCount: e.scales.steps,
				useNiceScale: e.scales.useNiceScale
			});
		}), U = v(() => ({
			xMin: Ar.value.min,
			xMax: Ar.value.max,
			yMin: jr.value.min,
			yMax: jr.value.max
		})), Mr = v(() => {
			let e = L.value.style.layout.dataLabels.xAxis, t = e.scales.labels;
			return Ar.value.ticks.map((n) => ({
				value: n,
				x: Ir(n),
				label: g(t.formatter, h(n), f({
					p: L.value.style.layout.plots.selectors.labels.prefix,
					v: h(n),
					s: L.value.style.layout.plots.selectors.labels.suffix,
					r: Or(e)
				}))
			}));
		}), Nr = v(() => {
			let e = L.value.style.layout.dataLabels.yAxis, t = e.scales.labels;
			return jr.value.ticks.map((n) => ({
				value: n,
				y: Lr(n),
				label: g(t.formatter, h(n), f({
					p: L.value.style.layout.plots.selectors.labels.prefix,
					v: h(n),
					s: L.value.style.layout.plots.selectors.labels.suffix,
					r: Or(e)
				}))
			}));
		}), Pr = v(() => Xr.value.length > 0 && L.value.style.layout.dataLabels.xAxis.scales.show), Fr = v(() => Xr.value.length > 0 && L.value.style.layout.dataLabels.yAxis.scales.show);
		function Ir(e) {
			let t = U.value.xMax - U.value.xMin;
			return t === 0 ? H.value.left : H.value.left + (e - U.value.xMin) / t * H.value.width;
		}
		function Lr(e) {
			let t = U.value.yMax - U.value.yMin;
			return t === 0 ? H.value.bottom : H.value.bottom - (e - U.value.yMin) / t * H.value.height;
		}
		let W = v(() => {
			let { xMin: e, xMax: t, yMin: n, yMax: r } = U.value, i = e > 0 ? e : t < 0 ? t : 0, a = n > 0 ? n : r < 0 ? r : 0;
			return {
				x: Ir(i),
				y: Lr(a)
			};
		}), G = v(() => sr.value.map((e, t) => {
			let n = `cluster_${P.value}_${t}`;
			return {
				...e,
				values: d({
					data: e.values,
					threshold: L.value.usePerformanceMode ? e.values.length + 1 : L.value.downsample.threshold
				}),
				id: n,
				color: e.color ? e.color : xr.value[t] || l[t] || l[t % l.length],
				opacity: I.value.includes(n) ? .5 : 1,
				shape: e.shape ?? "circle",
				segregate: () => ri(n),
				isSegregated: I.value.includes(n),
				hasGroupSelection: !!Gr.value,
				isGroupSelected: Gr.value === n,
				onEnter: () => qr(n),
				onLeave: () => Jr()
			};
		})), Rr = v(() => ({
			cy: "scatter-div-legend",
			backgroundColor: L.value.style.legend.backgroundColor,
			color: L.value.style.legend.color,
			fontSize: L.value.style.legend.fontSize,
			paddingBottom: 12,
			fontWeight: L.value.style.legend.bold ? "bold" : ""
		})), zr = v(() => G.value.map((e, t) => ({
			...e,
			plots: e.values.map((n, r) => ({
				x: Ir(n.x),
				y: Lr(n.y),
				v: {
					...n,
					name: n.name || ""
				},
				clusterName: e.name,
				clusterId: e.id,
				color: e.color ? e.color : xr.value[t] || l[t] || l[t % l.length],
				id: `plot_${P.value}_${t}_${r}`,
				weight: n.weight ?? L.value.style.layout.plots.radius
			}))
		})).filter((e) => !I.value.includes(e.id))), K = v(() => {
			let e = 1e-9, t = ({ m: t, b: n, rect: r, verticalX: i = null }) => {
				let { left: a, right: o, top: s, bottom: c } = r, l = [], u = (e, t) => {
					Number.isFinite(e) && Number.isFinite(t) && l.push({
						x: e,
						y: t
					});
				}, d = ({ x: t, y: n }) => t >= a - e && t <= o + e && n >= s - e && n <= c + e;
				i === null ? Number.isFinite(t) && (u(a, t * a + n), u(o, t * o + n), Math.abs(t) > e ? (u((s - n) / t, s), u((c - n) / t, c)) : n >= s - e && n <= c + e && (u(a, n), u(o, n))) : i >= a - e && i <= o + e && (u(i, s), u(i, c));
				let f = l.filter(d), p = [];
				for (let e of f) p.some((t) => Math.abs(t.x - e.x) < 1e-6 && Math.abs(t.y - e.y) < 1e-6) || p.push(e);
				if (p.length < 2) return null;
				let m = p[0], h = p[1], ee = 0;
				for (let e = 0; e < p.length; e += 1) for (let t = e + 1; t < p.length; t += 1) {
					let n = p[e].x - p[t].x, r = p[e].y - p[t].y, i = n * n + r * r;
					i > ee && (ee = i, m = p[e], h = p[t]);
				}
				return {
					x1: m.x,
					y1: m.y,
					x2: h.x,
					y2: h.y
				};
			};
			return zr.value.map((n) => {
				let r = n.plots.length, i = n.plots.reduce((e, t) => e + t.x, 0) / r, a = n.plots.reduce((e, t) => e + t.y, 0) / r, o = 0, s = 0, c = 0;
				for (let e of n.plots) {
					let t = e.x - i, n = e.y - a;
					o += t * n, s += t * t, c += n * n;
				}
				let l, d, f = null;
				s < e ? (f = i, l = Infinity, d = null) : (l = o / s, d = a - l * i);
				let p, m;
				f === null ? (p = l, m = d) : (p = Infinity, m = null);
				let h = n.plots.every((e) => e.v && typeof e.v.x == "number" && typeof e.v.y == "number"), ee = NaN;
				if (r >= 2) {
					let t = 0, o = 0;
					h ? (t = n.plots.reduce((e, t) => e + t.v.x, 0) / r, o = n.plots.reduce((e, t) => e + t.v.y, 0) / r) : (t = i, o = -a);
					let s = 0, c = 0, l = 0;
					for (let e of n.plots) {
						let n = h ? e.v.x : e.x, r = h ? e.v.y : -e.y, i = n - t, a = r - o;
						s += i * a, c += i * i, l += a * a;
					}
					if (c >= e && l >= e) {
						let e = s / Math.sqrt(c * l);
						ee = Math.max(-1, Math.min(1, e));
					}
				}
				let g = t({
					m: p,
					b: m,
					rect: H.value,
					verticalX: f
				});
				if (!g) return {
					...n,
					correlation: null,
					label: null,
					plots: n.plots.map((e) => ({
						...e,
						deviation: 0,
						shape: n.shape,
						color: u(n.color)
					}))
				};
				let te = {
					x: (g.x1 + g.x2) / 2,
					y: (g.y1 + g.y2) / 2
				};
				return {
					...n,
					color: u(n.color),
					correlation: {
						...g,
						coefficient: ee
					},
					label: te,
					plots: n.plots.map((t) => {
						let r, i;
						f === null ? Math.abs(p) < e ? (r = t.x, i = m) : (r = (t.x + p * t.y - p * m) / (1 + p * p), i = (p * t.x + p * p * t.y + m) / (1 + p * p)) : (r = f, i = t.y);
						let a = t.x - r, o = t.y - i, s = Math.sqrt(a * a + o * o);
						return {
							...t,
							deviation: s,
							shape: n.shape,
							color: u(n.color)
						};
					})
				};
			});
		}), Br = v(() => Math.max(...K.value.flatMap((e) => e.plots.map((e) => Math.abs(e.deviation)))));
		function Vr() {
			return K.value;
		}
		function Hr(e, t) {
			let n = Array.isArray(e) ? e.flatMap((e) => e.plots.map((e) => ({
				x: e.x,
				y: e.y
			}))) : e.plots.map((e) => ({
				x: e.x,
				y: e.y
			})), r = Infinity, i = -Infinity, a = Infinity, o = -Infinity;
			n.forEach(({ x: e, y: t }) => {
				r = Math.min(r, e), i = Math.max(i, e), a = Math.min(a, t), o = Math.max(o, t);
			});
			let s = i - r, c = o - a, l = s / t, u = c / t, d = Array(t).fill(0), f = Array(t).fill(0);
			n.forEach(({ x: e, y: t }) => {
				let n = Math.floor((e - r) / l), i = Math.floor((t - a) / u);
				d[n] || (d[n] = 0), f[i] || (f[i] = 0), d[n] += 1, f[i] += 1;
			});
			let p = [], m = [];
			for (let e = 0; e < t; e += 1) p.push(r + (e + .5) * l), m.push(a + (e + .5) * u);
			return {
				x: d,
				y: f,
				avgX: p,
				avgY: m,
				maxX: Math.max(...d),
				maxY: Math.max(...f)
			};
		}
		let q = v(() => L.value.style.layout.marginalBars.tranches), J = v(() => Hr(zr.value, q.value)), Ur = v(() => {
			let e = H.value.top - L.value.style.layout.marginalBars.offset, t = H.value.right + L.value.style.layout.marginalBars.offset;
			return zr.value.map((n) => {
				let r = Hr(n, q.value);
				return {
					coords: r,
					dX: c(r.avgX.map((t, n) => ({
						x: t,
						y: e - r.x[n] / r.maxX * L.value.style.layout.marginalBars.size
					}))),
					dY: i(r.avgY.map((e, n) => ({
						y: e,
						x: t + L.value.style.layout.marginalBars.size * r.y[n] / r.maxY
					}))),
					color: n.color,
					id: n.id
				};
			});
		}), Y = D(void 0), X = D(null), Z = D([]), Wr = D(null), Gr = D(void 0);
		function Kr() {
			An.value = !1, Y.value = void 0, Z.value = [], X.value = null, tr.value = null, Zn.value = null, Gr.value = void 0;
		}
		function qr(e) {
			let t = K.value.find((t) => t.id === e);
			if (!t) {
				Z.value = [];
				return;
			}
			Z.value = t.plots.map((e) => e.id), Gr.value = e;
		}
		function Jr() {
			Z.value = [], Gr.value = void 0;
		}
		function Yr(e) {
			if (!R.value || !e) return;
			let t = a(e.x, e.y, R.value);
			t && (Qn.value = t);
		}
		let Xr = v(() => X.value ? [X.value] : Z.value.length ? K.value.flatMap((e) => e.plots).filter((e) => Z.value.includes(e.id)) : []);
		function Zr(e) {
			let t = L.value.style.layout.dataLabels.xAxis, n = t.scales.labels;
			return g(n.formatter, h(e.v.x), f({
				p: L.value.style.layout.plots.selectors.labels.prefix,
				v: h(e.v.x),
				s: L.value.style.layout.plots.selectors.labels.suffix,
				r: Or(t)
			}), { datapoint: e });
		}
		function Qr(e) {
			let t = L.value.style.layout.dataLabels.yAxis, n = t.scales.labels;
			return g(n.formatter, h(e.v.y), f({
				p: L.value.style.layout.plots.selectors.labels.prefix,
				v: h(e.v.y),
				s: L.value.style.layout.plots.selectors.labels.suffix,
				r: Or(t)
			}), { datapoint: e });
		}
		function $r(e, t, n = "pointer", r = null) {
			Y.value = e.id, X.value = e, $n.value = n, tr.value = r, Zn.value = e.id;
			let i = "";
			L.value.events.datapointEnter && L.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), Wr.value = {
				datapoint: e,
				seriesIndex: t,
				series: K.value,
				config: L.value
			};
			let a = L.value.style.tooltip.customFormat;
			ce(a) && ee(() => a({
				datapoint: e,
				seriesIndex: t,
				series: K.value,
				config: L.value
			})) ? jn.value = a({
				datapoint: e,
				seriesIndex: t,
				series: K.value,
				config: L.value
			}) : (e.clusterName && (i += `<div style="display:flex;gap:3px;align-items:center">${e.clusterName}</div>`), e.v.name && (i += `<div>${e.v.name}</div>`), i += `<div style="text-align:left;margin-top:6px;padding-top:6px;border-top:1px solid ${L.value.style.tooltip.borderColor}">`, i += `<div>${L.value.style.layout.dataLabels.xAxis.name}: <b>${isNaN(e.v.x) ? "-" : g(L.value.style.layout.plots.selectors.labels.x.formatter, e.v.x, f({
				p: L.value.style.tooltip.prefix,
				v: e.v.x,
				s: L.value.style.tooltip.suffix,
				r: L.value.style.tooltip.roundingValue
			}), {
				datapoint: e,
				seriesIndex: t
			})}</b></div>`, i += `<div>${L.value.style.layout.dataLabels.yAxis.name}: <b>${isNaN(e.v.y) ? "-" : g(L.value.style.layout.plots.selectors.labels.y.formatter, e.v.y, f({
				p: L.value.style.tooltip.prefix,
				v: e.v.y,
				s: L.value.style.tooltip.suffix,
				r: L.value.style.tooltip.roundingValue
			}), {
				datapoint: e,
				seriesIndex: t
			})}</b></div>`, i += `${L.value.style.layout.plots.deviation.translation}: <b>${f({
				v: e.deviation,
				r: L.value.style.layout.plots.deviation.roundingValue
			})}</b>`, i += "</div>", jn.value = `<div>${i}</div>`), An.value = !0, n === "keyboard" && Re(() => {
				Yr(e);
			});
		}
		function ei(e, t) {
			L.value.events.datapointLeave && L.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}), ($n.value !== "keyboard" || Zn.value !== e?.id) && (An.value = !1, Y.value = void 0, X.value = null);
		}
		function ti(e, t) {
			L.value.events.datapointClick && L.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function ni() {
			I.value.length ? I.value = [] : G.value.forEach((e) => {
				I.value.push(e.id);
			}), On("selectLegend", zr.value);
		}
		function ri(e) {
			I.value.includes(e) ? I.value = I.value.filter((t) => t !== e) : I.value.length < sr.value.length - 1 && I.value.push(e), On("selectLegend", zr.value);
		}
		function ii(e) {
			return G.value.length ? G.value.find((t) => t.name === e) || (mr.value && console.warn(`VueUiScatter - Series name not found "${e}"`), null) : (mr.value && console.warn("VueUiScatter - There are no series to show."), null);
		}
		function ai(e) {
			let t = ii(e);
			t !== null && I.value.includes(t.id) && ri(t.id);
		}
		function oi(e) {
			let t = ii(e);
			t !== null && (I.value.includes(t.id) || ri(t.id));
		}
		function si(e = null) {
			Re(() => {
				let n = [
					"",
					L.value.table.translations.correlationCoefficient,
					L.value.table.translations.nbrPlots,
					`${L.value.style.layout.dataLabels.xAxis.name} ${L.value.table.translations.average}`,
					`${L.value.style.layout.dataLabels.yAxis.name} ${L.value.table.translations.average}`
				], i = K.value.map((e) => [
					e.name,
					e.correlation.coefficient,
					e.plots.length,
					e.plots.map((e) => e.v.x).reduce((e, t) => e + t, 0) / e.plots.length,
					e.plots.map((e) => e.v.y).reduce((e, t) => e + t, 0) / e.plots.length
				]), a = [
					[L.value.style.title.text],
					[L.value.style.title.subtitle.text],
					[
						[""],
						[""],
						[""]
					],
					n
				].concat(i), o = r(a);
				e ? e(o) : t({
					csvContent: o,
					title: L.value.style.title.text || "vue-ui-heatmap"
				});
			});
		}
		let ci = v(() => {
			let e = [
				L.value.table.translations.series,
				L.value.table.translations.correlationCoefficient,
				L.value.table.translations.nbrPlots,
				`${L.value.style.layout.dataLabels.xAxis.name} ${L.value.table.translations.average}`,
				`${L.value.style.layout.dataLabels.yAxis.name} ${L.value.table.translations.average}`
			], t = K.value.map((e) => [
				{
					shape: e.shape,
					content: e.name ?? "-",
					color: e.color
				},
				Number((e.correlation?.coefficient ?? 0).toFixed(L.value.table.td.roundingValue)).toLocaleString(),
				e.plots.length.toLocaleString(),
				Number((e.plots.map((e) => e.v.x ?? 0).reduce((e, t) => e + t, 0) / e.plots.length).toFixed(L.value.table.td.roundingAverage)).toLocaleString(),
				Number((e.plots.map((e) => e.v.y ?? 0).reduce((e, t) => e + t, 0) / e.plots.length).toFixed(L.value.table.td.roundingAverage)).toLocaleString()
			]);
			return {
				head: e,
				body: t,
				a11yBody: t.map((e) => e.map((e, t) => t === 0 ? e.content : e)),
				config: {
					th: {
						backgroundColor: L.value.table.th.backgroundColor,
						color: L.value.table.th.color,
						outline: L.value.table.th.outline
					},
					td: {
						backgroundColor: L.value.table.td.backgroundColor,
						color: L.value.table.td.color,
						outline: L.value.table.td.outline
					},
					breakpoint: L.value.table.responsiveBreakpoint
				},
				colNames: e
			};
		}), li = D(!1);
		function ui(e) {
			li.value = e, Mn.value += 1;
		}
		function di() {
			B.value.showTable = !B.value.showTable;
		}
		function fi() {
			B.value.showTooltip = !B.value.showTooltip;
		}
		let pi = D(!1);
		function mi() {
			pi.value = !pi.value;
		}
		async function hi({ scale: e = 2 } = {}) {
			if (!F.value) return;
			let { width: t, height: n } = F.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ye({
				domElement: F.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: L.value.style.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		function gi(e) {
			Vn.value = e, L.value.style.layout.marginalBars.highlighter.highlightBothAxes && (Hn.value = J.value.y.length - 2 - e);
		}
		function _i(e) {
			Hn.value = e, L.value.style.layout.marginalBars.highlighter.highlightBothAxes && (Vn.value = e);
		}
		function vi() {
			Vn.value = null, Hn.value = null;
		}
		let yi = {
			circle: 1,
			square: 1,
			diamond: 1,
			triangle: 1.2,
			star: 1.3,
			pentagon: 1.3,
			hexagon: 1.3
		}, Q = (e) => e.toFixed(3);
		function bi({ shape: e = "circle", cx: t, cy: n, r }) {
			if (!L.value.usePerformanceMode) return "";
			let i = yi[e] * r;
			switch (e) {
				case "circle": {
					let e = Q(t - i), r = Q(n), a = Q(t + i), o = Q(i);
					return `M ${e} ${r} A ${o} ${o} 0 1 0 ${a} ${r} A ${o} ${o} 0 1 0 ${e} ${r} Z`;
				}
				case "square": {
					let e = Q(t - i), r = Q(n - i), a = Q(t + i), o = Q(n + i);
					return `M ${e} ${r} L ${a} ${r} L ${a} ${o} L ${e} ${o} Z`;
				}
				case "diamond": {
					let e = Q(t), r = Q(n);
					return `M ${e} ${Q(n - i)} L ${Q(t + i)} ${r} L ${e} ${Q(n + i)} L ${Q(t - i)} ${r} Z`;
				}
				case "triangle": {
					let e = i * Math.sqrt(3), r = t, a = n - 2 / 3 * e, o = t - i, s = n + 1 / 3 * e, c = t + i, l = s;
					return `M ${Q(r)} ${Q(a)} L ${Q(o)} ${Q(s)} L ${Q(c)} ${Q(l)} Z`;
				}
				case "star": {
					let e = i, r = i * .5, a = [];
					for (let i = 0; i < 10; i += 1) {
						let o = (-90 + i * 36) * Math.PI / 180, s = i % 2 == 0 ? e : r;
						a.push([t + s * Math.cos(o), n + s * Math.sin(o)]);
					}
					let o = `M ${Q(a[0][0])} ${Q(a[0][1])}`;
					for (let e = 1; e < a.length; e += 1) o += ` L ${Q(a[e][0])} ${Q(a[e][1])}`;
					return o + " Z";
				}
				case "pentagon": {
					let e = [];
					for (let r = 0; r < 5; r += 1) {
						let a = (-90 + 360 / 5 * r) * Math.PI / 180;
						e.push([t + i * Math.cos(a), n + i * Math.sin(a)]);
					}
					let r = `M ${Q(e[0][0])} ${Q(e[0][1])}`;
					for (let t = 1; t < 5; t += 1) r += ` L ${Q(e[t][0])} ${Q(e[t][1])}`;
					return r + " Z";
				}
				case "hexagon": {
					let e = [];
					for (let r = 0; r < 6; r += 1) {
						let a = (-60 + 360 / 6 * r) * Math.PI / 180;
						e.push([t + i * Math.cos(a), n + i * Math.sin(a)]);
					}
					let r = `M ${Q(e[0][0])} ${Q(e[0][1])}`;
					for (let t = 1; t < 6; t += 1) r += ` L ${Q(e[t][0])} ${Q(e[t][1])}`;
					return r + " Z";
				}
				default: {
					let e = Q(t - i), r = Q(n), a = Q(t + i), o = Q(i);
					return `M ${e} ${r} A ${o} ${o} 0 1 0 ${a} ${r} A ${o} ${o} 0 1 0 ${e} ${r} Z`;
				}
			}
		}
		let xi = v(() => {
			if (!L.value.usePerformanceMode) return [""];
			let { left: e, right: t, top: r, bottom: i } = H.value, a = Math.max(1, (t - e) * (i - r)), o = (e) => e / a * 1e4, s = L.value.style.layout.plots.stroke, c = L.value.style.layout.plots.strokeWidth, l = L.value.style.layout.plots.opacity;
			return K.value.map((a) => {
				let u = [];
				for (let n of a.plots) {
					let o = n.x, s = n.y;
					if (o < e || o > t || s < r || s > i) continue;
					let c = Math.max(L.value.style.layout.plots.radius, n.weight);
					u.push(bi({
						shape: a.shape || "circle",
						cx: o,
						cy: s,
						r: c
					}));
				}
				if (!u.length) return null;
				let d = o(a.plots.length) > 2.5 || a.plots.length > 1e3;
				return {
					id: a.id,
					d: u.join(""),
					fill: n(a.color, l * 100),
					stroke: d ? "none" : s,
					strokeWidth: d ? 0 : c,
					strokeOpacity: 1
				};
			}).filter(Boolean);
		});
		function Si() {
			return L.value.usePerformanceMode ? (e) => {
				let t = R.value;
				if (!t) return;
				let n = t.createSVGPoint();
				n.x = e.clientX, n.y = e.clientY;
				let r = t.getScreenCTM();
				if (!r) return;
				let i = r.inverse(), a = n.matrixTransform(i), o = null, s = Infinity, c = -1;
				if (K.value.forEach((e, t) => {
					e.plots.forEach((e) => {
						let n = e.x - a.x, r = e.y - a.y, i = n * n + r * r;
						i <= 64 && i < s && (s = i, o = e, c = t);
					});
				}), o) Y.value !== o.id && (Y.value = o.id, $r(o, c, "pointer", $.value.get(o.id) ?? null));
				else if (Y.value) {
					let e = X.value;
					Y.value = void 0, ei(e, c);
				}
			} : () => null;
		}
		let Ci = Si();
		function wi() {
			if (Y.value) {
				let e = X.value;
				Y.value = void 0, ei(e, null);
			}
		}
		function Ti(e) {
			let t = X.value;
			if (t) {
				let e = K.value.findIndex((e) => e.id === t.clusterId);
				ti(t, e >= 0 ? e : 0);
			}
		}
		let Ei = v(() => {
			let e = L.value.table.useDialog && !L.value.table.show, t = B.value.showTable;
			return {
				component: e ? wn : yn,
				title: `${L.value.style.title.text}${L.value.style.title.subtitle.text ? `: ${L.value.style.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: L.value.table.th.backgroundColor,
					color: L.value.table.th.color,
					headerColor: L.value.table.th.color,
					headerBg: L.value.table.th.backgroundColor,
					isFullscreen: li.value,
					fullscreenParent: F.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: rr.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: L.value.style.backgroundColor,
							color: L.value.style.color
						},
						head: {
							backgroundColor: L.value.style.backgroundColor,
							color: L.value.style.color
						}
					}
				}
			};
		});
		Ge(() => B.value.showTable, (e) => {
			L.value.table.show || (e && L.value.table.useDialog && Un.value ? Un.value.open() : "close" in Un.value && Un.value.close());
		});
		function Di() {
			B.value.showTable = !1, Wn.value && Wn.value.setTableIconState(!1);
		}
		let Oi = v(() => L.value.style.backgroundColor), ki = v(() => L.value.style.legend), Ai = v(() => L.value.style.title), { isCallbackImaging: ji, isCallbackSvg: Mi, generateSvg: Ni, onGenerateImage: Pi } = _e({
			svg: R,
			title: Ai,
			legend: ki,
			legendItems: G,
			backgroundColor: Oi,
			getSvgCallback: () => L.value.userOptions.callbacks.svg,
			generateImage: yr
		});
		async function Fi() {
			if (On("copyAlt", {
				config: L.value,
				dataset: zr.value
			}), !L.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(L.value.userOptions.callbacks.altCopy({
				config: L.value,
				dataset: zr.value
			}));
		}
		function Ii(e, t) {
			let n = Vi.value.filter((t) => t.id !== e.id);
			if (!n.length) return null;
			let r = ((n) => {
				if (!n.length) return null;
				let r = null, i = Infinity;
				return n.forEach((n) => {
					let a = n.x - e.x, o = n.y - e.y, s = 0, c = 0;
					t === "right" || t === "left" ? (s = Math.abs(a), c = Math.abs(o)) : (s = Math.abs(o), c = Math.abs(a));
					let l = s * 1e3 + c;
					l < i && (r = n, i = l);
				}), r;
			})(n.filter((n) => t === "right" ? n.x > e.x : t === "left" ? n.x < e.x : t === "down" ? n.y > e.y : t === "up" && n.y < e.y));
			if (r) return r;
			let i = [];
			if (t === "right") {
				let e = Math.min(...n.map((e) => e.x));
				i = n.filter((t) => t.x === e);
			}
			if (t === "left") {
				let e = Math.max(...n.map((e) => e.x));
				i = n.filter((t) => t.x === e);
			}
			if (t === "down") {
				let e = Math.min(...n.map((e) => e.y));
				i = n.filter((t) => t.y === e);
			}
			if (t === "up") {
				let e = Math.max(...n.map((e) => e.y));
				i = n.filter((t) => t.y === e);
			}
			return i.length ? i.reduce((n, r) => {
				if (!n) return r;
				let i = Math.abs(t === "right" || t === "left" ? n.y - e.y : n.x - e.x);
				return Math.abs(t === "right" || t === "left" ? r.y - e.y : r.x - e.x) < i ? r : n;
			}, null) : null;
		}
		function Li() {
			tr.value = null, Zn.value = null, er.value = !0;
		}
		function Ri() {
			Kr(), $n.value = "pointer", er.value = !1;
		}
		function zi(e) {
			if (!R.value || pi.value || document.activeElement !== R.value || !Vi.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "ArrowUp", i = e.key === "ArrowDown", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				Kr(), $n.value = "pointer";
				return;
			}
			if (a) {
				if (tr.value === null) return;
				let e = Vi.value[tr.value];
				if (!e) return;
				ti(e, e.seriesIndex);
				return;
			}
			let s = null;
			if (n && (s = "right"), t && (s = "left"), i && (s = "down"), r && (s = "up"), Zn.value === null) {
				let e = X.value;
				if (e && e.id) {
					let t = Ii(e, s);
					if (!t) {
						let t = $.value.get(e.id);
						if (t === void 0) return;
						$r(e, e.seriesIndex, "keyboard", t);
						return;
					}
					let n = $.value.get(t.id);
					if (n === void 0) return;
					$r(t, t.seriesIndex, "keyboard", n);
					return;
				}
				let t = Vi.value[0];
				if (!t) return;
				let n = $.value.get(t.id) ?? 0;
				$r(t, t.seriesIndex, "keyboard", n);
				return;
			}
			let c = $.value.get(Zn.value);
			if (c === void 0) return;
			let l = Vi.value[c];
			if (!l) return;
			let u = Ii(l, s);
			if (!u) return;
			let d = $.value.get(u.id);
			d !== void 0 && $r(u, u.seriesIndex, "keyboard", d);
		}
		let Bi = v(() => {
			if (!X.value) return "";
			let e = X.value, t = isNaN(e.v.x) ? "-" : g(L.value.style.layout.plots.selectors.labels.x.formatter, e.v.x, f({
				p: L.value.style.tooltip.prefix,
				v: e.v.x,
				s: L.value.style.tooltip.suffix,
				r: L.value.style.tooltip.roundingValue
			}), {
				datapoint: e,
				seriesIndex: e.seriesIndex
			}), n = isNaN(e.v.y) ? "-" : g(L.value.style.layout.plots.selectors.labels.y.formatter, e.v.y, f({
				p: L.value.style.tooltip.prefix,
				v: e.v.y,
				s: L.value.style.tooltip.suffix,
				r: L.value.style.tooltip.roundingValue
			}), {
				datapoint: e,
				seriesIndex: e.seriesIndex
			});
			return [
				e.clusterName || "",
				e.v.name || "",
				`${L.value.style.layout.dataLabels.xAxis.name}: ${t}`,
				`${L.value.style.layout.dataLabels.yAxis.name}: ${n}`
			].filter(Boolean).join(". ");
		}), Vi = v(() => K.value.flatMap((e, t) => e.plots.map((n) => ({
			...n,
			seriesIndex: t,
			seriesId: e.id,
			seriesName: e.name,
			shape: e.shape || "circle",
			color: e.color
		})))), $ = v(() => {
			let e = /* @__PURE__ */ new Map();
			return Vi.value.forEach((t, n) => {
				e.set(t.id, n);
			}), e;
		}), Hi = v(() => ({
			headers: ci.value?.colNames ?? [],
			rows: ci.value?.a11yBody ?? []
		}));
		return Se({
			getData: Vr,
			getImage: hi,
			generatePdf: vr,
			generateCsv: si,
			generateImage: yr,
			generateSvg: Ni,
			hideSeries: oi,
			showSeries: ai,
			toggleTable: di,
			toggleTooltip: fi,
			toggleAnnotator: mi,
			toggleFullscreen: ui,
			copyAlt: Fi
		}), (e, t) => (E(), x("div", {
			class: ze(`vue-data-ui-component vue-ui-scatter ${li.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${L.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			ref_key: "scatterChart",
			ref: F,
			id: `vue-ui-scatter_${P.value}`,
			style: T(`font-family:${L.value.style.fontFamily};width:100%; text-align:center;background:${L.value.style.backgroundColor};${L.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: t[5] ||= () => j(ur)(!0),
			onMouseleave: t[6] ||= () => {
				j(ur)(!1), er.value || Kr();
			}
		}, [
			S("div", {
				id: `chart-instructions-${P.value}`,
				class: "sr-only"
			}, [S("p", null, A(L.value.a11y.translations.keyboardNavigation), 1)], 8, Je),
			B.value.showTooltip ? b("", !0) : (E(), x("div", Ye, A(Bi.value), 1)),
			Hi.value?.rows?.length && kn.value ? (E(), y(De, {
				key: 1,
				uid: P.value,
				head: Hi.value.headers,
				body: Hi.value.rows,
				notice: L.value.a11y.translations.tableAvailable,
				caption: L.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : b("", !0),
			L.value.userOptions.buttons.annotator ? (E(), y(j(xn), {
				key: 2,
				svgRef: j(R),
				backgroundColor: L.value.style.backgroundColor,
				color: L.value.style.color,
				active: pi.value,
				isCursorPointer: rr.value,
				onClose: mi
			}, {
				"annotator-action-close": M(() => [k(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": M(({ color: t }) => [k(e.$slots, "annotator-action-color", w(C({ color: t })), void 0, !0)]),
				"annotator-action-draw": M(({ mode: t }) => [k(e.$slots, "annotator-action-draw", w(C({ mode: t })), void 0, !0)]),
				"annotator-action-undo": M(({ disabled: t }) => [k(e.$slots, "annotator-action-undo", w(C({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": M(({ disabled: t }) => [k(e.$slots, "annotator-action-redo", w(C({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": M(({ disabled: t }) => [k(e.$slots, "annotator-action-delete", w(C({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : b("", !0),
			br.value ? (E(), x("div", {
				key: 3,
				ref_key: "noTitle",
				ref: In,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : b("", !0),
			L.value.style.title.text ? (E(), x("div", {
				key: 4,
				ref_key: "chartTitle",
				ref: Nn,
				style: "width:100%;background:transparent"
			}, [(E(), y(be, {
				key: `title_${Ln.value}`,
				config: {
					title: {
						cy: "scatter-div-title",
						...L.value.style.title
					},
					subtitle: {
						cy: "scatter-div-subtitle",
						...L.value.style.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : b("", !0),
			S("div", { id: `legend-top-${P.value}` }, null, 8, Xe),
			L.value.userOptions.show && kn.value && (j(dr) || j(lr)) ? (E(), y(j(Sn), {
				ref_key: "userOptionsRef",
				ref: Wn,
				key: `user_options_${Mn.value}`,
				backgroundColor: L.value.style.backgroundColor,
				color: L.value.style.color,
				isImaging: j(_r),
				isPrinting: j(gr),
				uid: P.value,
				hasTooltip: L.value.userOptions.buttons.tooltip && L.value.style.tooltip.show,
				hasPdf: L.value.userOptions.buttons.pdf,
				hasImg: L.value.userOptions.buttons.img,
				hasSvg: L.value.userOptions.buttons.svg,
				hasXls: L.value.userOptions.buttons.csv,
				hasTable: L.value.userOptions.buttons.table,
				hasFullscreen: L.value.userOptions.buttons.fullscreen,
				hasAltCopy: L.value.userOptions.buttons.altCopy,
				isTooltip: B.value.showTooltip,
				isFullscreen: li.value,
				titles: { ...L.value.userOptions.buttonTitles },
				chartElement: F.value,
				position: L.value.userOptions.position,
				hasAnnotator: L.value.userOptions.buttons.annotator,
				isAnnotation: pi.value,
				callbacks: L.value.userOptions.callbacks,
				printScale: L.value.userOptions.print.scale,
				tableDialog: L.value.table.useDialog,
				isCursorPointer: rr.value,
				onToggleFullscreen: ui,
				onGeneratePdf: j(vr),
				onGenerateCsv: si,
				onGenerateImage: j(Pi),
				onGenerateSvg: j(Ni),
				onToggleTable: di,
				onToggleTooltip: fi,
				onToggleAnnotator: mi,
				onCopyAlt: Fi,
				style: T({ visibility: j(dr) ? j(lr) ? "visible" : "hidden" : "visible" })
			}, Ne({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: M(({ isOpen: t, color: n }) => [k(e.$slots, "menuIcon", w(C({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: M(() => [k(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: M(() => [k(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: M(() => [k(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: M(() => [k(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: M(() => [k(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: M(() => [k(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: M(({ toggleFullscreen: t, isFullscreen: n }) => [k(e.$slots, "optionFullscreen", w(C({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: M(({ toggleAnnotator: t, isAnnotator: n }) => [k(e.$slots, "optionAnnotator", w(C({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: M(({ altCopy: t }) => [k(e.$slots, "optionAltCopy", w(C({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: M(() => [k(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: M(() => [k(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isImaging.isPrinting.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isTooltip.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : b("", !0),
			S("div", Ze, [(E(), x("svg", {
				ref_key: "svgRef",
				ref: R,
				xmlns: j(ie),
				"aria-describedby": `chart-instructions-${P.value}`,
				class: ze({
					"vue-data-ui-fullscreen--on": li.value,
					"vue-data-ui-fulscreen--off": !li.value,
					"vue-data-ui-no-transition": !j(nr)
				}),
				viewBox: `0 0 ${V.value.width <= 0 ? 10 : V.value.width} ${V.value.height <= 0 ? 10 : V.value.height}`,
				style: T(`max-width:100%;overflow:visible;background:transparent;color:${L.value.style.color}`),
				onMouseleave: vi,
				tabindex: "0",
				onFocus: Li,
				onBlur: Ri,
				onKeydown: zi
			}, [
				Fe(j(Cn)),
				e.$slots["chart-background"] ? (E(), x("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: V.value.width <= 0 ? 10 : V.value.width,
					height: V.value.height <= 0 ? 10 : V.value.height,
					style: { pointerEvents: "none" }
				}, [k(e.$slots, "chart-background", {}, void 0, !0)], 8, $e)) : b("", !0),
				L.value.style.layout.axis.show ? (E(), x("g", et, [S("line", {
					x1: W.value.x,
					x2: W.value.x,
					y1: H.value.top,
					y2: H.value.bottom,
					stroke: L.value.style.layout.axis.stroke,
					"stroke-width": L.value.style.layout.axis.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, tt), S("line", {
					x1: H.value.left,
					x2: H.value.right,
					y1: W.value.y,
					y2: W.value.y,
					stroke: L.value.style.layout.axis.stroke,
					"stroke-width": L.value.style.layout.axis.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, nt)])) : b("", !0),
				L.value.style.layout.dataLabels.xAxis.scales.verticalLines.show ? (E(), x("g", rt, [(E(!0), x(_, null, O(Mr.value, (e, t) => (E(), x("line", {
					key: `scatter-x-scale-line-${P.value}-${t}`,
					x1: e.x,
					x2: e.x,
					y1: H.value.top,
					y2: H.value.bottom,
					stroke: L.value.style.layout.dataLabels.xAxis.scales.verticalLines.stroke,
					"stroke-width": L.value.style.layout.dataLabels.xAxis.scales.verticalLines.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, it))), 128))])) : b("", !0),
				L.value.style.layout.dataLabels.xAxis.scales.show ? (E(), x("g", {
					key: 3,
					ref_key: "xAxisScaleLabels",
					ref: Xn
				}, [(E(!0), x(_, null, O(Mr.value, (e, t) => (E(), x("g", {
					class: "vue-ui-scatter-scale-group",
					key: `scatter-x-scale-${P.value}-${t}`,
					opacity: +!Pr.value
				}, [S("path", {
					class: ze({ "vue-data-ui-transition": j(nr) }),
					stroke: L.value.style.layout.axis.stroke,
					"stroke-width": L.value.style.layout.axis.strokeWidth,
					d: `M${e.x},${W.value.y - 4} ${e.x},${W.value.y + 4}`,
					"stroke-linecap": "round"
				}, null, 10, ot), S("text", {
					class: ze({ "vue-data-ui-transition": j(nr) }),
					transform: `translate(${e.x}, ${W.value.y + L.value.style.layout.dataLabels.xAxis.scales.labels.fontSize + 6 + L.value.style.layout.dataLabels.xAxis.scales.labels.offsetY})`,
					"text-anchor": "middle",
					"font-size": L.value.style.layout.dataLabels.xAxis.scales.labels.fontSize,
					fill: L.value.style.layout.dataLabels.xAxis.scales.labels.color,
					stroke: L.value.style.backgroundColor,
					"stroke-width": "2",
					"paint-order": "stroke"
				}, A(e.label), 11, st)], 8, at))), 128))], 512)) : b("", !0),
				L.value.style.layout.dataLabels.yAxis.scales.horizontalLines.show ? (E(), x("g", ct, [(E(!0), x(_, null, O(Nr.value, (e, t) => (E(), x("line", {
					key: `scatter-y-scale-line-${P.value}-${t}`,
					x1: H.value.left,
					x2: H.value.right,
					y1: e.y,
					y2: e.y,
					stroke: L.value.style.layout.dataLabels.yAxis.scales.horizontalLines.stroke,
					"stroke-width": L.value.style.layout.dataLabels.yAxis.scales.horizontalLines.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, lt))), 128))])) : b("", !0),
				L.value.style.layout.dataLabels.yAxis.scales.show ? (E(), x("g", {
					key: 5,
					ref_key: "yAxisScaleLabels",
					ref: Yn,
					opacity: +!Fr.value,
					style: { transition: "opacity 0.1s ease-in-out" },
					class: "vue-ui-scatter-scale-group"
				}, [(E(!0), x(_, null, O(Nr.value, (e, t) => (E(), x("g", { key: `scatter-y-scale-${P.value}-${t}` }, [S("path", {
					class: ze({ "vue-data-ui-transition": j(nr) }),
					d: `M${W.value.x - 4},${e.y} ${W.value.x + 4},${e.y}`,
					stroke: L.value.style.layout.axis.stroke,
					"stroke-width": L.value.style.layout.axis.strokeWidth,
					"stroke-linecap": "round"
				}, null, 10, dt), S("text", {
					class: ze({ "vue-data-ui-transition": j(nr) }),
					transform: `translate(${W.value.x - L.value.style.layout.dataLabels.yAxis.scales.labels.fontSize / 2 - 8 + L.value.style.layout.dataLabels.yAxis.scales.labels.offsetX}, ${e.y + L.value.style.layout.dataLabels.yAxis.scales.labels.fontSize / 3})`,
					"text-anchor": "end",
					"font-size": L.value.style.layout.dataLabels.yAxis.scales.labels.fontSize,
					fill: L.value.style.layout.dataLabels.yAxis.scales.labels.color,
					stroke: L.value.style.backgroundColor,
					"stroke-width": "2",
					"paint-order": "stroke"
				}, A(e.label), 11, ft)]))), 128))], 8, ut)) : b("", !0),
				L.value.style.layout.marginalBars.show ? (E(), x("g", pt, [
					S("defs", null, [Fe(Te, {
						t: "linear",
						id: `marginal_x_${P.value}`,
						x1: "0%",
						y1: "0%",
						x2: "0%",
						y2: "100%",
						stops: [[
							"0%",
							L.value.style.layout.marginalBars.fill,
							1
						], [
							"100%",
							L.value.style.backgroundColor,
							1
						]]
					}, null, 8, ["id", "stops"]), Fe(Te, {
						t: "linear",
						id: `marginal_y_${P.value}`,
						x1: "0%",
						x2: "100%",
						y1: "0%",
						y2: "0%",
						stops: [[
							"0%",
							L.value.style.backgroundColor,
							1
						], [
							"100%",
							L.value.style.layout.marginalBars.fill,
							1
						]]
					}, null, 8, ["id", "stops"])]),
					(E(!0), x(_, null, O(J.value.x, (e, n) => (E(), x("g", null, [
						e && J.value.avgX[n] ? (E(), x("rect", {
							key: 0,
							x: J.value.avgX[n] - H.value.width / q.value / 2,
							y: H.value.top - L.value.style.layout.marginalBars.offset - e / J.value.maxX * L.value.style.layout.marginalBars.size,
							width: H.value.width / q.value <= 0 ? 1e-4 : H.value.width / q.value,
							height: e / J.value.maxX * L.value.style.layout.marginalBars.size <= 0 ? 1e-4 : e / J.value.maxX * L.value.style.layout.marginalBars.size,
							fill: L.value.style.layout.marginalBars.useGradient ? `url(#marginal_x_${P.value})` : L.value.style.layout.marginalBars.fill,
							style: T([`opacity:${L.value.style.layout.marginalBars.opacity}`, { "pointer-events": "none" }]),
							stroke: L.value.style.backgroundColor,
							"stroke-width": L.value.style.layout.marginalBars.strokeWidth,
							rx: L.value.style.layout.marginalBars.borderRadius
						}, null, 12, mt)) : b("", !0),
						J.value.avgX[n] ? (E(), x("rect", {
							key: 1,
							x: J.value.avgX[n] - H.value.width / q.value / 2,
							y: H.value.top - L.value.style.layout.marginalBars.offset - L.value.style.layout.marginalBars.size,
							width: H.value.width / q.value <= 0 ? 1e-4 : H.value.width / q.value,
							height: Math.max(.1, L.value.style.layout.marginalBars.size),
							fill: "transparent",
							onMouseenter: (e) => gi(n),
							onMouseleave: t[0] ||= (e) => vi()
						}, null, 40, ht)) : b("", !0),
						J.value.avgX[n] && Vn.value != null && Vn.value === n ? (E(), x("g", gt, [
							S("rect", {
								x: J.value.avgX[n] - H.value.width / q.value / 2,
								y: H.value.top,
								width: H.value.width / q.value <= 0 ? 1e-4 : H.value.width / q.value,
								height: H.value.height,
								fill: L.value.style.layout.marginalBars.highlighter.color,
								"fill-opacity": L.value.style.layout.marginalBars.highlighter.opacity
							}, null, 8, _t),
							S("line", {
								x1: J.value.avgX[n] - H.value.width / q.value / 2,
								x2: J.value.avgX[n] - H.value.width / q.value / 2,
								y1: 0,
								y2: H.value.top + H.value.height,
								stroke: L.value.style.layout.marginalBars.highlighter.stroke,
								"stroke-dasharray": L.value.style.layout.marginalBars.highlighter.strokeDasharray,
								"stroke-width": L.value.style.layout.marginalBars.highlighter.strokeWidth,
								style: {
									transition: "none !important",
									animation: "none !important"
								}
							}, null, 8, vt),
							S("line", {
								x1: J.value.avgX[n] - H.value.width / q.value / 2 + (H.value.width / q.value <= 0 ? 1e-4 : H.value.width / q.value),
								x2: J.value.avgX[n] - H.value.width / q.value / 2 + (H.value.width / q.value <= 0 ? 1e-4 : H.value.width / q.value),
								y1: 0,
								y2: H.value.top + H.value.height,
								stroke: L.value.style.layout.marginalBars.highlighter.stroke,
								"stroke-dasharray": L.value.style.layout.marginalBars.highlighter.strokeDasharray,
								"stroke-width": L.value.style.layout.marginalBars.highlighter.strokeWidth,
								style: {
									transition: "none !important",
									animation: "none !important"
								}
							}, null, 8, yt)
						])) : b("", !0)
					]))), 256)),
					(E(!0), x(_, null, O(J.value.y, (e, n) => (E(), x("g", null, [
						e && J.value.avgY[n] ? (E(), x("rect", {
							key: 0,
							x: H.value.right + L.value.style.layout.marginalBars.offset,
							y: J.value.avgY[n] - H.value.height / q.value / 2,
							height: H.value.height / q.value <= 0 ? 1e-4 : H.value.height / q.value,
							width: e / J.value.maxY * L.value.style.layout.marginalBars.size <= 0 ? 1e-4 : e / J.value.maxY * L.value.style.layout.marginalBars.size,
							fill: L.value.style.layout.marginalBars.useGradient ? `url(#marginal_y_${P.value})` : L.value.style.layout.marginalBars.fill,
							style: T([`opacity:${L.value.style.layout.marginalBars.opacity}`, { "pointer-events": "none" }]),
							stroke: L.value.style.backgroundColor,
							"stroke-width": L.value.style.layout.marginalBars.strokeWidth,
							rx: L.value.style.layout.marginalBars.borderRadius
						}, null, 12, bt)) : b("", !0),
						J.value.avgY[n] ? (E(), x("rect", {
							key: 1,
							x: H.value.right + L.value.style.layout.marginalBars.offset,
							y: J.value.avgY[n] - H.value.height / q.value / 2,
							width: Math.max(.1, L.value.style.layout.marginalBars.size),
							height: H.value.height / q.value <= 0 ? 1e-4 : H.value.height / q.value,
							fill: "transparent",
							onMouseenter: (e) => _i(n),
							onMouseleave: t[1] ||= (e) => vi()
						}, null, 40, xt)) : b("", !0),
						J.value.avgY[n] && Hn.value != null && Hn.value === n ? (E(), x("g", St, [
							S("rect", {
								x: H.value.left,
								y: J.value.avgY[n] - H.value.height / q.value / 2,
								width: H.value.width,
								height: H.value.height / q.value <= 0 ? 1e-4 : H.value.height / q.value,
								fill: L.value.style.layout.marginalBars.highlighter.color,
								"fill-opacity": L.value.style.layout.marginalBars.highlighter.opacity
							}, null, 8, Ct),
							S("line", {
								x1: H.value.left,
								x2: V.value.width,
								y1: J.value.avgY[n] - H.value.height / q.value / 2,
								y2: J.value.avgY[n] - H.value.height / q.value / 2,
								stroke: L.value.style.layout.marginalBars.highlighter.stroke,
								"stroke-dasharray": L.value.style.layout.marginalBars.highlighter.strokeDasharray,
								"stroke-width": L.value.style.layout.marginalBars.highlighter.strokeWidth,
								style: {
									transition: "none !important",
									animation: "none !important"
								}
							}, null, 8, wt),
							S("line", {
								x1: H.value.left,
								x2: V.value.width,
								y1: J.value.avgY[n] - H.value.height / q.value / 2 + (H.value.height / q.value <= 0 ? 1e-4 : H.value.height / q.value),
								y2: J.value.avgY[n] - H.value.height / q.value / 2 + (H.value.height / q.value <= 0 ? 1e-4 : H.value.height / q.value),
								stroke: L.value.style.layout.marginalBars.highlighter.stroke,
								"stroke-dasharray": L.value.style.layout.marginalBars.highlighter.strokeDasharray,
								"stroke-width": L.value.style.layout.marginalBars.highlighter.strokeWidth,
								style: {
									transition: "none !important",
									animation: "none !important"
								}
							}, null, 8, Tt)
						])) : b("", !0)
					]))), 256)),
					L.value.style.layout.marginalBars.showLines ? (E(), x("g", Et, [(E(!0), x(_, null, O(Ur.value, (e) => (E(), x(_, null, [
						I.value.includes(e.id) ? b("", !0) : (E(), x("path", {
							key: 0,
							d: `M ${e.dX}`,
							stroke: L.value.style.backgroundColor,
							"stroke-width": L.value.style.layout.marginalBars.linesStrokeWidth + 1,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							fill: "none"
						}, null, 8, Dt)),
						I.value.includes(e.id) ? b("", !0) : (E(), x("path", {
							key: 1,
							d: `M ${e.dX}`,
							stroke: e.color,
							"stroke-width": L.value.style.layout.marginalBars.linesStrokeWidth,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							fill: "none"
						}, null, 8, Ot)),
						I.value.includes(e.id) ? b("", !0) : (E(), x("path", {
							key: 2,
							d: `M ${e.dY}`,
							stroke: L.value.style.backgroundColor,
							"stroke-width": L.value.style.layout.marginalBars.linesStrokeWidth + 1,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							fill: "none"
						}, null, 8, kt)),
						I.value.includes(e.id) ? b("", !0) : (E(), x("path", {
							key: 3,
							d: `M ${e.dY}`,
							stroke: e.color,
							"stroke-width": L.value.style.layout.marginalBars.linesStrokeWidth,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							fill: "none"
						}, null, 8, At))
					], 64))), 256))])) : b("", !0)
				])) : b("", !0),
				L.value.style.layout.plots.giftWrap.show ? (E(), x("g", jt, [(E(!0), x(_, null, O(K.value, (e, t) => (E(), x("g", null, [e.plots.length > 2 ? (E(), x("polygon", {
					key: 0,
					points: j(m)({ series: e.plots }),
					fill: j(n)(e.color, L.value.style.layout.plots.giftWrap.fillOpacity * 100),
					"stroke-width": L.value.style.layout.plots.giftWrap.strokeWidth,
					"stroke-dasharray": L.value.style.layout.plots.giftWrap.strokeDasharray,
					stroke: e.color,
					"stroke-linejoin": "round",
					"stroke-linecap": "round"
				}, null, 8, Mt)) : b("", !0)]))), 256))])) : b("", !0),
				L.value.usePerformanceMode ? b("", !0) : (E(!0), x(_, { key: 8 }, O(K.value, (e, t) => (E(), x("g", { key: e.id }, [!e.shape || e.shape === "circle" ? (E(), x("g", Nt, [(E(!0), x(_, null, O(e.plots, (r, i) => (E(), x("circle", {
					key: r.id,
					cx: r.x,
					cy: r.y,
					r: Y.value && Y.value === r.id || Z.value.includes(r.id) ? r.weight * L.value.style.layout.plots.hoverRadiusRatio : r.weight,
					fill: j(n)(e.color, Y.value && Y.value !== r.id || Z.value.length && !Z.value.includes(r.id) ? L.value.style.layout.plots.opacityNotSelected * 100 : L.value.style.layout.plots.opacity * 100),
					stroke: L.value.style.layout.plots.stroke,
					"stroke-width": L.value.style.layout.plots.strokeWidth,
					style: T(`opacity:${Y.value && Y.value === r.id || Z.value.includes(r.id) ? 1 : L.value.style.layout.plots.significance.useDistanceOpacity ? 1 - Math.abs(r.deviation) / Br.value : L.value.style.layout.plots.significance.show && Math.abs(r.deviation) > L.value.style.layout.plots.significance.deviationThreshold ? L.value.style.layout.plots.significance.opacity : 1}`),
					onMouseover: (e) => $r(r, t, "pointer", $.value.get(r.id)),
					onMouseleave: (e) => ei(r, t),
					onClick: (e) => ti(r, t)
				}, null, 44, Pt))), 128))])) : (E(), x("g", Ft, [(E(!0), x(_, null, O(e.plots, (r, i) => (E(), y(xe, {
					class: "vue-ui-scatter-datapoint",
					plot: {
						x: r.x,
						y: r.y
					},
					radius: Y.value && Y.value === r.id || Z.value.includes(r.id) ? r.weight * L.value.style.layout.plots.hoverRadiusRatio : r.weight,
					shape: e.shape,
					color: j(n)(e.color, Y.value && Y.value !== r.id || Z.value.length && !Z.value.includes(r.id) ? L.value.style.layout.plots.opacityNotSelected * 100 : L.value.style.layout.plots.opacity * 100),
					stroke: L.value.style.layout.plots.stroke,
					strokeWidth: L.value.style.layout.plots.strokeWidth,
					style: T(`opacity:${Y.value && Y.value === r.id || Z.value.includes(r.id) ? 1 : L.value.style.layout.plots.significance.useDistanceOpacity ? 1 - Math.abs(r.deviation) / Br.value : L.value.style.layout.plots.significance.show && Math.abs(r.deviation) > L.value.style.layout.plots.significance.deviationThreshold ? L.value.style.layout.plots.significance.opacity : 1}`),
					onMouseover: (e) => $r(r, t, "pointer", $.value.get(r.id)),
					onMouseleave: (e) => ei(r, t),
					onClick: (e) => ti(r, t)
				}, null, 8, [
					"plot",
					"radius",
					"shape",
					"color",
					"stroke",
					"strokeWidth",
					"style",
					"onMouseover",
					"onMouseleave",
					"onClick"
				]))), 256))])), L.value.style.layout.plots.name.show ? (E(!0), x(_, { key: 2 }, O(e.plots, (e, r) => (E(), x("text", {
					class: "vue-ui-scatter-datapoint-label",
					key: `datalabel-${e.id}`,
					transform: `translate(${e.x}, ${e.y - e.weight - L.value.style.layout.plots.name.fontSize + L.value.style.layout.plots.name.offsetY})`,
					"text-anchor": "middle",
					"font-size": L.value.style.layout.plots.name.fontSize,
					fill: j(n)(L.value.style.layout.plots.name.color, Y.value && Y.value !== e.id || Z.value.length && !Z.value.includes(e.id) ? L.value.style.layout.plots.opacityNotSelected * 100 : 100),
					onMouseover: (n) => $r(e, t, "pointer", $.value.get(e.id)),
					onMouseleave: (n) => ei(e, t),
					onClick: (n) => ti(e, t)
				}, A(e.clusterName), 41, It))), 128)) : b("", !0)]))), 128)),
				L.value.usePerformanceMode ? (E(), x(_, { key: 9 }, [
					S("g", { "clip-path": `url(#clip_path_${P.value})` }, [(E(!0), x(_, null, O(xi.value, (e) => (E(), x("path", {
						key: e.id,
						d: e.d,
						fill: e.fill,
						stroke: e.stroke,
						"stroke-width": e.strokeWidth,
						"stroke-opacity": e.strokeOpacity,
						"vector-effect": "non-scaling-stroke",
						"paint-order": "fill"
					}, null, 8, Rt))), 128))], 8, Lt),
					X.value && L.value.style.layout.plots.selectors.show ? (E(), x("g", zt, [Fe(xe, {
						shape: X.value.shape || "circle",
						color: X.value.color,
						plot: {
							x: X.value.x,
							y: X.value.y
						},
						radius: Math.max(4 * yi[X.value.shape || "circle"], X.value.weight * L.value.style.layout.plots.hoverRadiusRatio),
						stroke: L.value.style.layout.plots.stroke,
						strokeWidth: L.value.style.layout.plots.strokeWidth
					}, null, 8, [
						"shape",
						"color",
						"plot",
						"radius",
						"stroke",
						"strokeWidth"
					])])) : b("", !0),
					S("rect", {
						x: H.value.left,
						y: H.value.top,
						width: Math.max(1e-4, H.value.width),
						height: Math.max(1e-4, H.value.height),
						fill: "transparent",
						onMousemove: t[2] ||= (...e) => j(Ci) && j(Ci)(...e),
						onMouseleave: wi,
						onClick: Ti
					}, null, 40, Bt)
				], 64)) : b("", !0),
				(E(!0), x(_, null, O(Xr.value, (e) => (E(), x(_, { key: `selector_${e.id}` }, [L.value.style.layout.plots.selectors.show ? (E(), x("g", Vt, [
					S("line", {
						x1: W.value.x,
						x2: e.x,
						y1: e.y,
						y2: e.y,
						stroke: L.value.style.layout.plots.selectors.stroke,
						"stroke-width": L.value.style.layout.plots.selectors.strokeWidth,
						"stroke-dasharray": L.value.style.layout.plots.selectors.strokeDasharray,
						"stroke-linecap": "round",
						class: "line-pointer"
					}, null, 8, Ht),
					S("line", {
						x1: e.x,
						x2: e.x,
						y1: W.value.y,
						y2: e.y,
						stroke: L.value.style.layout.plots.selectors.stroke,
						"stroke-width": L.value.style.layout.plots.selectors.strokeWidth,
						"stroke-dasharray": L.value.style.layout.plots.selectors.strokeDasharray,
						"stroke-linecap": "round",
						class: "line-pointer"
					}, null, 8, Ut),
					L.value.style.layout.dataLabels.yAxis.scales.show ? b("", !0) : (E(), x("text", {
						key: 0,
						x: W.value.x + (e.x > W.value.x ? -6 : 6),
						y: e.y + L.value.style.layout.plots.selectors.labels.fontSize / 3,
						"font-size": L.value.style.layout.plots.selectors.labels.fontSize,
						fill: L.value.style.layout.plots.selectors.labels.color,
						"font-weight": L.value.style.layout.plots.selectors.labels.bold ? "bold" : "normal",
						"text-anchor": e.x > W.value.x ? "end" : "start"
					}, A(j(g)(L.value.style.layout.plots.selectors.labels.y.formatter, j(h)(e.v.y), j(f)({
						p: L.value.style.layout.plots.selectors.labels.prefix,
						v: j(h)(e.v.y),
						s: L.value.style.layout.plots.selectors.labels.suffix,
						r: L.value.style.layout.plots.selectors.labels.rounding
					}), { datapoint: e })), 9, Wt)),
					L.value.style.layout.dataLabels.xAxis.scales.show ? b("", !0) : (E(), x("text", {
						key: 1,
						x: e.x,
						y: W.value.y + (e.y > W.value.y ? -6 : L.value.style.layout.plots.selectors.labels.fontSize + 6),
						"font-size": L.value.style.layout.plots.selectors.labels.fontSize,
						fill: L.value.style.layout.plots.selectors.labels.color,
						"font-weight": L.value.style.layout.plots.selectors.labels.bold ? "bold" : "normal",
						"text-anchor": "middle"
					}, A(j(g)(L.value.style.layout.plots.selectors.labels.y.formatter, j(h)(e.v.x), j(f)({
						p: L.value.style.layout.plots.selectors.labels.prefix,
						v: j(h)(e.v.x),
						s: L.value.style.layout.plots.selectors.labels.suffix,
						r: L.value.style.layout.plots.selectors.labels.rounding
					}), { datapoint: e })), 9, Gt)),
					L.value.style.layout.dataLabels.xAxis.scales.show ? (E(), x("text", {
						key: 2,
						x: e.x,
						y: W.value.y + L.value.style.layout.dataLabels.xAxis.scales.labels.fontSize + 6 + L.value.style.layout.dataLabels.xAxis.scales.labels.offsetY,
						"text-anchor": "middle",
						"font-size": L.value.style.layout.dataLabels.xAxis.scales.labels.fontSize,
						fill: L.value.style.layout.dataLabels.xAxis.scales.labels.color
					}, A(Zr(e)), 9, Kt)) : b("", !0),
					L.value.style.layout.dataLabels.yAxis.scales.show ? (E(), x("text", {
						key: 3,
						x: W.value.x - L.value.style.layout.dataLabels.yAxis.scales.labels.fontSize / 2 - 8 + L.value.style.layout.dataLabels.yAxis.scales.labels.offsetX,
						y: e.y + L.value.style.layout.dataLabels.yAxis.scales.labels.fontSize / 3,
						"text-anchor": "end",
						"font-size": L.value.style.layout.dataLabels.yAxis.scales.labels.fontSize,
						fill: L.value.style.layout.dataLabels.yAxis.scales.labels.color
					}, A(Qr(e)), 9, qt)) : b("", !0),
					S("circle", {
						cx: W.value.x,
						cy: e.y,
						r: L.value.style.layout.plots.selectors.markers.radius,
						fill: L.value.style.layout.plots.selectors.markers.fill,
						stroke: L.value.style.layout.plots.selectors.markers.stroke,
						"stroke-width": L.value.style.layout.plots.selectors.markers.strokeWidth,
						class: "line-pointer"
					}, null, 8, Jt),
					S("circle", {
						cx: e.x,
						cy: W.value.y,
						r: L.value.style.layout.plots.selectors.markers.radius,
						fill: L.value.style.layout.plots.selectors.markers.fill,
						stroke: L.value.style.layout.plots.selectors.markers.stroke,
						"stroke-width": L.value.style.layout.plots.selectors.markers.strokeWidth,
						class: "line-pointer"
					}, null, 8, Yt),
					L.value.style.layout.plots.selectors.labels.showName && !L.value.style.layout.plots.name.show ? (E(), x("text", {
						key: 4,
						x: e.x,
						y: e.y + (e.y < W.value.y ? -L.value.style.layout.plots.selectors.labels.fontSize / 2 : L.value.style.layout.plots.selectors.labels.fontSize),
						"font-size": L.value.style.layout.plots.selectors.labels.fontSize,
						fill: L.value.style.layout.plots.selectors.labels.color,
						"font-weight": L.value.style.layout.plots.selectors.labels.bold ? "bold" : "normal",
						"text-anchor": e.x < H.value.left + 100 ? "start" : e.x > H.value.right - 100 ? "end" : e.x > W.value.x ? "start" : "end"
					}, A(e.v.name), 9, Xt)) : b("", !0)
				])) : b("", !0)], 64))), 128)),
				L.value.style.layout.dataLabels.xAxis.show ? (E(), x("g", {
					key: 10,
					ref_key: "xAxisLabelLeft",
					ref: Gn
				}, [S("text", {
					id: `vue-ui-scatter-xAxis-label-${P.value}`,
					transform: `translate(${L.value.style.layout.dataLabels.xAxis.fontSize + (L.value.style.layout.dataLabels.reverseAxisLabels ? L.value.style.layout.dataLabels.yAxis.offsetX : L.value.style.layout.dataLabels.xAxis.offsetX)}, ${H.value.top + H.value.height / 2 + (L.value.style.layout.dataLabels.reverseAxisLabels ? L.value.style.layout.dataLabels.yAxis.offsetY : L.value.style.layout.dataLabels.xAxis.offsetY)}), rotate(-90)`,
					"text-anchor": "middle",
					"font-size": L.value.style.layout.dataLabels.xAxis.fontSize,
					"font-weight": L.value.style.layout.dataLabels.xAxis.bold ? "bold" : "normal",
					fill: L.value.style.layout.dataLabels.xAxis.color
				}, A(L.value.style.layout.dataLabels.reverseAxisLabels ? L.value.style.layout.dataLabels.yAxis.name : L.value.style.layout.dataLabels.xAxis.name), 9, Zt), L.value.style.layout.dataLabels.xAxis.showValue ? (E(), x("text", {
					key: 0,
					"text-anchor": "middle",
					"font-size": L.value.style.layout.dataLabels.xAxis.fontSize,
					fill: L.value.style.layout.dataLabels.xAxis.color,
					transform: `translate(${L.value.style.layout.dataLabels.xAxis.name ? L.value.style.layout.dataLabels.xAxis.fontSize * 3 : 0}, ${W.value.y + L.value.style.layout.dataLabels.xAxis.fontSize / 3}), rotate(-90)`
				}, A(j(g)(L.value.style.layout.plots.selectors.labels.x.formatter, j(h)(U.value.xMin), j(f)({
					p: L.value.style.layout.plots.selectors.labels.prefix,
					v: j(h)(U.value.xMin),
					s: L.value.style.layout.plots.selectors.labels.suffix,
					r: L.value.style.layout.dataLabels.xAxis.rounding
				}))), 9, Qt)) : b("", !0)], 512)) : b("", !0),
				L.value.style.layout.dataLabels.xAxis.show && L.value.style.layout.dataLabels.xAxis.showValue ? (E(), x("text", {
					key: 11,
					ref_key: "xAxisLabelRight",
					ref: Kn,
					"text-anchor": "middle",
					transform: `translate(${H.value.right + L.value.style.layout.padding.right + 6}, ${W.value.y + L.value.style.layout.dataLabels.xAxis.fontSize / 3}), rotate(-90)`,
					"font-size": L.value.style.layout.dataLabels.xAxis.fontSize,
					fill: L.value.style.layout.dataLabels.xAxis.color
				}, A(j(g)(L.value.style.layout.plots.selectors.labels.x.formatter, j(h)(U.value.xMax), j(f)({
					p: L.value.style.layout.plots.selectors.labels.prefix,
					v: j(h)(U.value.xMax),
					s: L.value.style.layout.plots.selectors.labels.suffix,
					r: L.value.style.layout.dataLabels.xAxis.rounding
				}))), 9, $t)) : b("", !0),
				L.value.style.layout.dataLabels.yAxis.show && L.value.style.layout.dataLabels.yAxis.showValue ? (E(), x("text", {
					key: 12,
					ref_key: "yAxisLabelTop",
					ref: qn,
					x: W.value.x,
					y: H.value.top - L.value.style.layout.dataLabels.yAxis.fontSize,
					"text-anchor": "middle",
					"font-size": L.value.style.layout.dataLabels.yAxis.fontSize,
					fill: L.value.style.layout.dataLabels.yAxis.color
				}, A(j(g)(L.value.style.layout.plots.selectors.labels.y.formatter, j(h)(U.value.yMax), j(f)({
					p: L.value.style.layout.plots.selectors.labels.prefix,
					v: j(h)(U.value.yMax),
					s: L.value.style.layout.plots.selectors.labels.suffix,
					r: L.value.style.layout.dataLabels.yAxis.rounding
				}))), 9, en)) : b("", !0),
				L.value.style.layout.dataLabels.yAxis.show ? (E(), x("g", {
					key: 13,
					ref_key: "yAxisLabelBottom",
					ref: Jn
				}, [L.value.style.layout.dataLabels.yAxis.showValue ? (E(), x("text", {
					key: 0,
					x: W.value.x,
					y: V.value.height - L.value.style.layout.dataLabels.yAxis.fontSize * 2,
					"text-anchor": "middle",
					"font-size": L.value.style.layout.dataLabels.yAxis.fontSize,
					fill: L.value.style.layout.dataLabels.yAxis.color
				}, A(j(g)(L.value.style.layout.plots.selectors.labels.y.formatter, j(h)(U.value.yMin), j(f)({
					p: L.value.style.layout.plots.selectors.labels.prefix,
					v: j(h)(U.value.yMin),
					s: L.value.style.layout.plots.selectors.labels.suffix,
					r: L.value.style.layout.dataLabels.yAxis.rounding
				}))), 9, tn)) : b("", !0), S("text", {
					"text-anchor": "middle",
					"font-size": L.value.style.layout.dataLabels.yAxis.fontSize,
					"font-weight": L.value.style.layout.dataLabels.yAxis.bold ? "bold" : "normal",
					fill: L.value.style.layout.dataLabels.yAxis.color,
					x: H.value.left + H.value.width / 2 + (L.value.style.layout.dataLabels.reverseAxisLabels ? L.value.style.layout.dataLabels.xAxis.offsetX : L.value.style.layout.dataLabels.yAxis.offsetX),
					y: V.value.height + (L.value.style.layout.dataLabels.reverseAxisLabels ? L.value.style.layout.dataLabels.xAxis.offsetY : L.value.style.layout.dataLabels.yAxis.offsetY)
				}, A(L.value.style.layout.dataLabels.reverseAxisLabels ? L.value.style.layout.dataLabels.xAxis.name : L.value.style.layout.dataLabels.yAxis.name), 9, nn)], 512)) : b("", !0),
				S("clipPath", { id: `clip_path_${P.value}` }, [S("rect", {
					x: H.value.left,
					y: H.value.top,
					width: H.value.width <= 0 ? 1e-4 : H.value.width,
					height: H.value.height <= 0 ? 1e-4 : H.value.height
				}, null, 8, an)], 8, rn),
				L.value.style.layout.correlation.show ? (E(), x("g", on, [(E(!0), x(_, null, O(K.value.filter((e) => e.correlation), (e, t) => (E(), x("line", {
					x1: e.correlation.x1,
					x2: e.correlation.x2,
					y1: e.correlation.y1,
					y2: e.correlation.y2,
					"stroke-dasharray": L.value.style.layout.correlation.strokeDasharray,
					stroke: e.color,
					"stroke-width": L.value.style.layout.correlation.strokeWidth,
					"clip-path": `url(#clip_path_${P.value})`
				}, null, 8, sn))), 256)), (E(!0), x(_, null, O(K.value.filter((e) => e.correlation), (e, t) => (E(), x("g", null, [L.value.style.layout.correlation.label.show ? (E(), x("text", {
					key: 0,
					x: e.correlation.x2,
					y: e.correlation.y2,
					fill: L.value.style.layout.correlation.label.useSerieColor ? e.color : L.value.style.layout.correlation.label.color,
					"text-anchor": "end",
					"font-size": L.value.style.layout.correlation.label.fontSize,
					"font-weight": L.value.style.layout.correlation.label.bold ? "bold" : "normal"
				}, A(j(f)({
					v: j(h)(e.correlation?.coefficient ?? 0),
					r: L.value.style.layout.correlation.label.roundingValue
				})), 9, cn)) : b("", !0)]))), 256))])) : b("", !0),
				k(e.$slots, "svg", { svg: {
					...V.value,
					drawingArea: {
						...H.value,
						zero: W.value
					},
					data: zr.value,
					isPrintingImg: j(gr) || j(_r) || j(ji),
					isPrintingSvg: j(Mi)
				} }, void 0, !0)
			], 46, Qe)), e.$slots.hint ? (E(), x("div", ln, [k(e.$slots, "hint", w(C({
				hint: L.value.a11y.translations.keyboardNavigation,
				isVisible: er.value
			})), void 0, !0)])) : b("", !0)]),
			e.$slots.watermark ? (E(), x("div", un, [k(e.$slots, "watermark", w(C({ isPrinting: j(gr) || j(_r) || j(ji) || j(Mi) })), void 0, !0)])) : b("", !0),
			S("div", { id: `legend-bottom-${P.value}` }, null, 8, dn),
			kn.value && Bn.value && (L.value.style.legend.show || e.$slots.legend) ? (E(), y(Me, {
				key: 7,
				to: L.value.style.legend.position === "top" ? `#legend-top-${P.value}` : `#legend-bottom-${P.value}`
			}, [S("div", {
				ref_key: "chartLegend",
				ref: Pn
			}, [k(e.$slots, "legend", { legend: G.value }, () => [L.value.style.legend.show ? (E(), y(Ae, {
				key: `legend_${zn.value}`,
				legendSet: G.value,
				config: Rr.value,
				isCursorPointer: rr.value,
				onClickMarker: t[3] ||= ({ legend: e }) => ri(e.id)
			}, {
				item: M(({ legend: e }) => [S("div", {
					onClick: (t) => e.segregate(),
					style: T(`opacity:${I.value.includes(e.id) ? .5 : 1}`)
				}, A(e.name), 13, fn)]),
				legendToggle: M(() => [G.value.length > 2 && L.value.style.legend.selectAllToggle.show && !j(or) ? (E(), y(Ee, {
					key: 0,
					backgroundColor: L.value.style.legend.selectAllToggle.backgroundColor,
					color: L.value.style.legend.selectAllToggle.color,
					fontSize: L.value.style.legend.fontSize,
					checked: I.value.length > 0,
					isCursorPointer: rr.value,
					onToggle: ni
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : b("", !0)]),
				_: 1
			}, 8, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : b("", !0)], !0)], 512)], 8, ["to"])) : b("", !0),
			e.$slots.source ? (E(), x("div", {
				key: 8,
				ref_key: "source",
				ref: Fn,
				dir: "auto"
			}, [k(e.$slots, "source", {}, void 0, !0)], 512)) : b("", !0),
			Fe(j(_n), {
				teleportTo: L.value.style.tooltip.teleportTo,
				show: B.value.showTooltip && An.value,
				backgroundColor: L.value.style.tooltip.backgroundColor,
				color: L.value.style.tooltip.color,
				borderRadius: L.value.style.tooltip.borderRadius,
				borderColor: L.value.style.tooltip.borderColor,
				borderWidth: L.value.style.tooltip.borderWidth,
				fontSize: L.value.style.tooltip.fontSize,
				backgroundOpacity: L.value.style.tooltip.backgroundOpacity,
				position: L.value.style.tooltip.position,
				offsetX: L.value.style.tooltip.offsetX,
				offsetY: L.value.style.tooltip.offsetY,
				parent: F.value,
				content: jn.value,
				isFullscreen: li.value,
				isCustom: L.value.style.tooltip.customFormat && typeof L.value.style.tooltip.customFormat == "function",
				smooth: L.value.style.tooltip.smooth,
				backdropFilter: L.value.style.tooltip.backdropFilter,
				smoothForce: L.value.style.tooltip.smoothForce,
				smoothSnapThreshold: L.value.style.tooltip.smoothSnapThreshold,
				isA11yMode: $n.value === "keyboard",
				a11yPosition: Qn.value
			}, {
				"tooltip-before": M(() => [k(e.$slots, "tooltip-before", w(C({ ...Wr.value })), void 0, !0)]),
				tooltip: M(() => [k(e.$slots, "tooltip", w(C({ ...Wr.value })), void 0, !0)]),
				"tooltip-after": M(() => [k(e.$slots, "tooltip-after", w(C({ ...Wr.value })), void 0, !0)]),
				default: M(() => [L.value.style.tooltip.showShape ? (E(), x("div", pn, [(E(), x("svg", mn, [Fe(xe, {
					shape: X.value.shape,
					color: X.value.color,
					plot: {
						x: 10,
						y: 10
					},
					radius: 7
				}, null, 8, ["shape", "color"])]))])) : b("", !0)]),
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
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			kn.value && L.value.userOptions.buttons.table ? (E(), y(He(Ei.value.component), Le({ key: 9 }, Ei.value.props, {
				ref_key: "tableUnit",
				ref: Un,
				onClose: Di
			}), Ne({
				content: M(() => [(E(), y(j(bn), {
					key: `table_${Rn.value}`,
					colNames: ci.value.colNames,
					head: ci.value.head,
					body: ci.value.body,
					config: ci.value.config,
					title: L.value.table.useDialog ? "" : Ei.value.title,
					withCloseButton: !L.value.table.useDialog,
					isCursorPointer: rr.value,
					onClose: Di
				}, {
					th: M(({ th: e }) => [Pe(A(e), 1)]),
					td: M(({ td: e }) => [e.shape ? (E(), x("div", hn, [S("span", null, A(e.content), 1)])) : (E(), x("div", {
						key: 1,
						innerHTML: e
					}, null, 8, gn))]),
					_: 1
				}, 8, [
					"colNames",
					"head",
					"body",
					"config",
					"title",
					"withCloseButton",
					"isCursorPointer"
				]))]),
				_: 2
			}, [L.value.table.useDialog ? {
				name: "title",
				fn: M(() => [Pe(A(Ei.value.title), 1)]),
				key: "0"
			} : void 0, L.value.table.useDialog ? {
				name: "actions",
				fn: M(() => [S("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[4] ||= (e) => si(L.value.userOptions.callbacks.csv),
					style: T({ cursor: rr.value ? "pointer" : "default" })
				}, [Fe(j(vn), {
					name: "fileCsv",
					stroke: Ei.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : b("", !0),
			k(e.$slots, "skeleton", {}, () => [j(or) ? (E(), y(me, { key: 0 })) : b("", !0)], !0)
		], 46, qe));
	}
}, [["__scopeId", "data-v-c9028944"]]);
//#endregion
export { Ke as n, _n as t };
