import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, G as r, Gt as a, Jt as o, K as s, Kt as c, Ot as l, Pt as u, S as d, Wt as f, X as p, _ as ee, c as te, ct as ne, dt as re, i as ie, jt as ae, ot as oe, pt as se, q as ce, r as le, t as ue, tt as de, v as fe, w as pe, xt as me } from "./lib-Bttd6u5E.js";
import { n as he, t as ge } from "./useHints-Dq_w2E8B.js";
import { n as _e, r as ve, t as ye } from "./useTimeLabels-d2f-W1L4.js";
import { t as be } from "./useConfig-DlNpz6P8.js";
import { t as xe } from "./usePrinter-DN5bYhTG.js";
import { n as Se, t as Ce } from "./BaseScanner-DZvpgOjM.js";
import { t as we } from "./useNestedProp-vPNvh7rV.js";
import { t as Te } from "./useThemeCheck-C43Tcqmk.js";
import { t as Ee } from "./useChartExport-DNiwdPmb.js";
import { t as De } from "./useTransitions-g_zBREk2.js";
import { t as Oe } from "./useStableElementSize-C7KADDKj.js";
import { t as ke } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as Ae } from "./img-Bnokohej.js";
import { n as je } from "./Title-BE3qg9xl.js";
import { t as Me } from "./Shape-C21CMlWS.js";
import { t as Ne } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Pe, t as Fe } from "./useResponsive-ZtArZtUf.js";
import { t as Ie } from "./DefGrad-DVBqDjhO.js";
import { t as Le } from "./SlicerPreview-wUw1hFwe.js";
import { t as Re } from "./BaseLegendToggle-DZVucLnv.js";
import { t as ze } from "./A11yDataTable-DdRsVULz.js";
import { t as Be } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ve } from "./useChartAccessibility-DYqac8yF.js";
import { t as He } from "./Legend-CQxUgOd-.js";
import { t as Ue } from "./vue_ui_stackbar-COOrQQdK.js";
import { Fragment as m, Teleport as We, computed as h, createBlock as g, createCommentVNode as _, createElementBlock as v, createElementVNode as y, createSlots as Ge, createTextVNode as Ke, createVNode as qe, defineAsyncComponent as Je, guardReactiveProps as b, mergeProps as Ye, nextTick as Xe, normalizeClass as x, normalizeProps as S, normalizeStyle as C, onBeforeUnmount as Ze, onMounted as Qe, openBlock as w, ref as T, renderList as E, renderSlot as D, resolveDynamicComponent as $e, shallowRef as et, toDisplayString as O, toRefs as tt, unref as k, useSlots as nt, watch as rt, watchEffect as it, withCtx as A } from "vue";
//#region src/components/vue-ui-stackbar.vue
var at = /* @__PURE__ */ e({ default: () => en }), ot = ["id"], st = ["id"], ct = ["id"], lt = { style: { position: "relative" } }, ut = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], dt = [
	"x",
	"y",
	"width",
	"height"
], ft = { key: 1 }, pt = [
	"x",
	"y",
	"width",
	"height",
	"stroke",
	"stroke-width",
	"stroke-linecap",
	"stroke-linejoin",
	"stroke-dasharray"
], mt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], ht = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], gt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], _t = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], vt = { key: 0 }, yt = [
	"x",
	"y",
	"height",
	"rx",
	"width",
	"fill",
	"stroke",
	"stroke-width"
], bt = { key: 0 }, xt = [
	"x",
	"y",
	"height",
	"rx",
	"width",
	"fill",
	"stroke",
	"stroke-width"
], St = [
	"x",
	"y",
	"width",
	"rx",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], Ct = { key: 0 }, wt = [
	"x",
	"y",
	"width",
	"rx",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], Tt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], Et = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], Dt = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], Ot = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], kt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], At = [
	"transform",
	"font-size",
	"font-weight",
	"fill"
], jt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], Mt = [
	"transform",
	"font-size",
	"font-weight",
	"fill"
], Nt = ["stroke", "d"], Pt = [
	"transform",
	"font-size",
	"font-weight",
	"fill",
	"text-anchor"
], Ft = ["d", "stroke"], It = [
	"font-size",
	"font-weight",
	"fill",
	"text-anchor",
	"transform"
], Lt = { key: 0 }, Rt = { key: 1 }, zt = [
	"text-anchor",
	"font-size",
	"font-weight",
	"fill",
	"transform",
	"onClick"
], Bt = [
	"text-anchor",
	"font-size",
	"fill",
	"transform",
	"innerHTML",
	"onClick"
], Vt = { key: 0 }, Ht = { key: 1 }, Ut = [
	"font-size",
	"font-weight",
	"fill",
	"transform",
	"onClick"
], Wt = [
	"font-size",
	"font-weight",
	"fill",
	"transform",
	"onClick",
	"innerHTML"
], Gt = [
	"x",
	"y",
	"width",
	"height",
	"onClick",
	"onMouseenter",
	"onMouseleave",
	"fill"
], Kt = [
	"x",
	"y",
	"width",
	"height",
	"onClick",
	"onMouseenter",
	"onMouseleave",
	"fill"
], qt = ["data-start", "data-end"], Jt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, Yt = {
	key: 4,
	class: "vue-data-ui-watermark"
}, Xt = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke"
], Zt = ["id"], Qt = ["onClick"], $t = ["innerHTML"], en = /*#__PURE__*/ Ne({
	__name: "vue-ui-stackbar",
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
		"selectDatapoint",
		"selectLegend",
		"selectTimeLabel",
		"selectX",
		"copyAlt"
	],
	setup(e, { expose: Ne, emit: at }) {
		let en = Je(() => import("./Tooltip-DhjyfHwz.js")), tn = Je(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), nn = Je(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), rn = Je(() => import("./DataTable-BbKgJ5UI.js")), an = Je(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), on = Je(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), sn = Je(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), cn = Je(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_stackbar: ln } = be(), { isThemeValid: un, warnInvalidTheme: dn } = Te(), fn = nt(), j = e, M = at, pn = h({
			get() {
				return !!j.dataset && j.dataset.length;
			},
			set(e) {
				return e;
			}
		}), N = T(null), P = T(ce()), mn = T(!1), hn = T(null), gn = T(""), F = T([]), _n = T(0), vn = T(null), yn = T(null), bn = T(null), xn = T(null), Sn = T(!1), I = T(null), Cn = T(!1), wn = T(0), Tn = T(0), En = T(0), Dn = T(!1), On = T(null), kn = T(null), An = T(null), jn = T(null), Mn = T(null), Nn = T(null), Pn = T(null), Fn = T(null), In = et(null), Ln = T(!1), Rn = T(0), zn = T(0), L = T(null), Bn = T({
			x: 0,
			y: 0
		}), Vn = T("pointer"), Hn = Oe({
			elementRef: In,
			minimumWidth: 2,
			minimumHeight: 2,
			stableFramesRequired: 2,
			once: !1,
			onSizeAccepted: () => {
				Gn();
			}
		});
		function Un() {
			In.value = N.value?.parentNode ?? null;
		}
		function Wn() {
			return new Promise((e) => {
				requestAnimationFrame(() => {
					requestAnimationFrame(e);
				});
			});
		}
		async function Gn() {
			let e = ++zn.value;
			Ln.value = !1, await Xe(), await Wn(), await Wn(), e === zn.value && (Rn.value += 1, Ln.value = !0);
		}
		let Kn = T(null);
		Qe(() => {
			Dn.value = !0, Un(), Hn.start(), mr();
		});
		let R = T(rr());
		he({
			config: () => R.value,
			dataset: () => j.dataset,
			component: "VueUiStackbar",
			rules: [
				ge.emptyArray,
				{
					test: (e) => e.some((e) => e.series.length > 60),
					message: [
						"👀 Some series have > 60 datapoints. Consider",
						"",
						"▶️ Using VueUiStackline instead, for better readability.",
						"",
						"▶️ Aggregating data to larger time units, to reduce the number of datapoints.",
						"",
						"▶️ Using filters to constraint the data to a specific time range."
					]
				},
				{
					test: (e) => e.length > 6,
					message: [
						"👀 The number of series > 6, which can make the chart hard to read. Consider:",
						"",
						"▶️ Grouping small values into an \"Other\" category.",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display."
					]
				}
			]
		});
		let { transitionEnabled: z } = De({
			config: () => R.value.transitions,
			dataset: () => j.dataset
		}), B = h(() => R.value.userOptions.useCursorPointer), qn = h(() => o({
			defaultConfig: {
				userOptions: { show: !1 },
				useCssAnimation: !1,
				table: { show: !1 },
				tooltip: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					bars: {
						totalValues: { show: !1 },
						dataLabels: { show: !1 }
					},
					grid: {
						frame: { stroke: "#6A6A6A" },
						scale: {
							scaleMin: 0,
							scaleMax: 40
						},
						x: {
							axisColor: "#6A6A6A",
							linesColor: "#6A6A6A",
							axisName: { show: !1 },
							timeLabels: { show: !1 }
						},
						y: {
							axisColor: "#6A6A6A",
							linesColor: "#6A6A6A",
							axisName: { show: !1 },
							axisLabels: { show: !1 }
						}
					},
					legend: { backgroundColor: "transparent" },
					padding: {
						left: 24,
						right: 24,
						bottom: 12
					},
					zoom: {
						show: !1,
						startIndex: null,
						endIndex: null
					}
				} }
			},
			userConfig: R.value.skeletonConfig ?? {}
		})), { loading: Jn, FINAL_DATASET: Yn, manualLoading: Xn } = Se({
			...tt(j),
			FINAL_CONFIG: R,
			prepareConfig: rr,
			callback: () => {
				Promise.resolve().then(async () => {
					(!R.value.style.chart.zoom.keepState || !Gi.value || J.value.start === 0 && J.value.end === 0) && await Ki();
				});
			},
			skeletonDataset: j.config?.skeletonDataset ?? [{
				name: "",
				series: [
					2,
					3,
					5,
					8,
					13,
					21
				],
				color: "#BABABA"
			}, {
				name: "",
				series: [
					1,
					2,
					3,
					5,
					8,
					13
				],
				color: "#CACACA"
			}],
			skeletonConfig: o({
				defaultConfig: R.value,
				userConfig: qn.value
			})
		}), { userOptionsVisible: Zn, setUserOptionsVisibility: Qn, keepUserOptionState: $n } = Be({ config: R.value }), { svgRef: er } = Ve({ config: R.value.style.chart.title });
		function tr() {
			Qn(!0);
		}
		function nr() {
			Qn(!1), M("selectX", {
				index: null,
				indexLabel: null,
				dataset: null
			}), I.value = null, L.value = null, mn.value = !1;
		}
		function rr() {
			let e = we({
				userConfig: j.config,
				defaultConfig: ln
			}), t = {}, n = e.theme;
			if (n) if (!un.value(e)) dn(e), t = e;
			else {
				let r = we({
					userConfig: Ue[n] || j.config,
					defaultConfig: e
				});
				t = {
					...we({
						userConfig: j.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : c[n] || u
				};
			}
			else t = e;
			return t;
		}
		let ir = h(() => R.value.style.chart.bars.dataLabels.hideUnderValue != null), ar = h(() => R.value.style.chart.bars.dataLabels.hideUnderPercentage != null);
		rt(() => j.config, (e) => {
			Jn.value || (R.value = rr()), Zn.value = !R.value.userOptions.showOnChartHover, mr({ resetSlicer: !R.value.style.chart.zoom.keepState }), wn.value += 1, Tn.value += 1, En.value += 1, V.value.dataLabels.show = R.value.style.chart.bars.dataLabels.show, V.value.showTable = R.value.table.show, V.value.showTooltip = R.value.style.chart.tooltip.show, Un(), !R.value.style.chart.zoom.keepState || !Gi.value || J.value.start === 0 && J.value.end === 0 ? Ki() : Ar();
		}, { deep: !0 }), rt(() => j.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Xn.value = !1), Un(), R.value.style.chart.zoom.keepState ? Ar() : Nr();
		}, { deep: !0 });
		let V = T({
			dataLabels: { show: R.value.style.chart.bars.dataLabels.show },
			showTable: R.value.table.show,
			showTooltip: R.value.style.chart.tooltip.show
		});
		rt(R, () => {
			V.value = {
				dataLabels: { show: R.value.style.chart.bars.dataLabels.show },
				showTable: R.value.table.show,
				showTooltip: R.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let { isPrinting: or, isImaging: sr, generatePdf: cr, generateImage: lr } = xe({
			elementId: `stackbar_${P.value}`,
			fileName: R.value.style.chart.title.text || "vue-ui-stackbar",
			options: R.value.userOptions.print
		}), H = T({
			width: R.value.style.chart.width,
			height: R.value.style.chart.height,
			paddingRatio: {
				top: R.value.style.chart.padding.top / R.value.style.chart.height,
				right: R.value.style.chart.padding.right / R.value.style.chart.width,
				bottom: R.value.style.chart.padding.bottom / R.value.style.chart.height,
				left: R.value.style.chart.padding.left / R.value.style.chart.width
			}
		}), ur = h(() => pe(R.value.customPalette)), U = et(null), dr = et(null), fr = T(null), pr = h(() => R.value.debug);
		function mr({ resetSlicer: e = !0 } = {}) {
			if (ae(j.dataset) ? (de({
				componentName: "VueUiStackbar",
				type: "dataset",
				debug: pr.value
			}), Xn.value = !0) : j.dataset.forEach((e, t) => {
				se({
					datasetObject: e,
					requiredAttributes: ["name", "series"]
				}).forEach((e) => {
					pn.value = !1, de({
						componentName: "VueUiStackbar",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: pr.value
					}), Xn.value = !0;
				});
			}), ae(j.dataset) || (Xn.value = R.value.loading), setTimeout(() => {
				Cn.value = !0;
			}, 10), R.value.responsive) {
				let e = Pe(() => {
					Cn.value = !1;
					let { width: e, height: t } = Fe({
						chart: N.value,
						title: R.value.style.chart.title.text ? vn.value : null,
						legend: R.value.style.chart.legend.show ? yn.value : null,
						slicer: R.value.style.chart.zoom.show && q.value > 6 ? bn.value.$el : null,
						source: xn.value
					});
					requestAnimationFrame(() => {
						H.value.width = e, H.value.height = t - 12, clearTimeout(fr.value), fr.value = setTimeout(() => {
							Cn.value = !0;
						}, 10);
					});
				});
				U.value && (dr.value && U.value.unobserve(dr.value), U.value.disconnect()), U.value = new ResizeObserver(e), dr.value = N.value.parentNode, U.value.observe(dr.value);
			}
			Gn(), e && Ki();
		}
		Ze(() => {
			Hn.stop(), U.value && (dr.value && U.value.unobserve(dr.value), U.value.disconnect());
		});
		let W = h(() => R.value.style.chart.grid.y.position === "right");
		function hr() {
			let e = 0;
			R.value.orientation === "vertical" && Mn.value && (e = Array.from(Mn.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0)), R.value.orientation === "horizontal" && Nn.value && (e = Array.from(Nn.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = jn.value ? jn.value.getBoundingClientRect().width + R.value.style.chart.grid.y.axisName.fontSize + R.value.style.chart.grid.y.axisName.offsetX : 0;
			return {
				left: W.value ? 0 : e + t,
				right: W.value ? e + t : 0,
				scaleLabelsWidth: e,
				yAxisLabelWidth: t
			};
		}
		let gr = T(0), _r = T(0), vr = Pe((e) => {
			_r.value = e + R.value.style.chart.bars.totalValues.fontSize;
		}, 100);
		function yr() {
			if (R.value.orientation !== "horizontal") return 0;
			let e = Fn.value;
			if (!e) return 0;
			let t = Array.from(e.querySelectorAll("text"));
			if (!t.length) return 0;
			let n = -Infinity;
			for (let e of t) try {
				let t = e.getBBox(), r = t.x + t.width;
				r > n && (n = r);
			} catch {}
			return Math.max(0, n - (G.value?.right ?? 0));
		}
		it((e) => {
			if (R.value.orientation !== "horizontal") return;
			let t = Fn.value;
			if (!t) return;
			let n = () => {
				let e = yr();
				vr(e);
			};
			n();
			let r = new ResizeObserver(n);
			r.observe(t);
			let a = new MutationObserver(n);
			a.observe(t, {
				childList: !0,
				subtree: !0,
				characterData: !0
			}), e(() => {
				r.disconnect(), a.disconnect();
			});
		}), Ze(() => {
			gr.value = 0, _r.value = 0;
		});
		let br = T(0), xr = T(0);
		function Sr(e) {
			let t = Number.isFinite(e) ? e : 0;
			br.value !== t && (br.value = t);
		}
		function Cr() {
			let e = R.value.orientation === "vertical" ? Nn.value : Mn.value;
			if (!e) {
				Sr(0);
				return;
			}
			try {
				Sr(e.getBBox()?.height);
			} catch {
				Sr(0);
			}
		}
		function wr() {
			xr.value && cancelAnimationFrame(xr.value), xr.value = requestAnimationFrame(() => {
				requestAnimationFrame(() => {
					Cr();
				});
			});
		}
		Ze(() => {
			xr.value && cancelAnimationFrame(xr.value);
		});
		let Tr = h(() => {
			let e = 0;
			if (An.value) try {
				e = An.value.getBBox().height;
			} catch {
				e = 0;
			}
			let t = R.value.style.chart.grid.x.timeLabels.show ? br.value : 0;
			return e + t;
		}), G = h(() => {
			Rn.value;
			let { height: e, width: t } = H.value, { right: n } = H.value.paddingRatio, r = R.value.style.chart.bars.totalValues.show && j.dataset && j.dataset.length > 1 ? R.value.style.chart.bars.totalValues.fontSize * 1.3 : 0, a = 0, o = 0, s = 0, c = 0;
			if (R.value.style.chart.grid.y.axisLabels.show) {
				let e = hr();
				a = e.left, o = e.right, s = e.scaleLabelsWidth, c = e.yAxisLabelWidth;
			}
			let l = R.value.style.chart.padding.top + r, u = t - t * n - _r.value - o, d = e - R.value.style.chart.padding.bottom - Tr.value - r, f = R.value.style.chart.padding.left + a, p = t - f - t * n - _r.value - o, ee = e - l - R.value.style.chart.padding.bottom - Tr.value - r;
			return {
				chartHeight: e,
				chartWidth: t,
				top: l,
				right: u,
				bottom: d,
				left: f,
				width: Math.max(0, p),
				height: Math.max(0, ee),
				offsetLeft: a,
				offsetRightAxis: o,
				scaleLabelsWidth: s,
				yAxisLabelWidth: c
			};
		}), Er = h(() => {
			let { left: e, top: t, width: n, height: r } = G.value, a = J.value.start, o = J.value.end, s = Math.max(1, o - a), c = Math.max(0, Math.min(s, (Y.value.start ?? a) - a)), l = Math.max(0, Math.min(s, (Y.value.end ?? o) - a)), u = Math.max(0, l - c), d = {
				fill: R.value.style.chart.zoom.preview.fill,
				stroke: R.value.style.chart.zoom.preview.stroke,
				"stroke-width": R.value.style.chart.zoom.preview.strokeWidth,
				"stroke-dasharray": R.value.style.chart.zoom.preview.strokeDasharray,
				"stroke-linecap": "round",
				"stroke-linejoin": "round",
				style: {
					pointerEvents: "none",
					transition: "none !important",
					animation: "none !important"
				}
			};
			if (R.value.orientation === "horizontal") {
				let a = r / s;
				return {
					x: e,
					y: t + c * a,
					width: n,
					height: u * a,
					...d
				};
			}
			{
				let a = n / s;
				return {
					x: e + c * a,
					y: t,
					width: u * a,
					height: r,
					...d
				};
			}
		}), K = h(() => Yn.value.map((e, t) => {
			let n = ce(), r = String(e.key ?? e.id ?? e.name ?? n), a = d(e.color) || ur.value[t] || u[t] || u[t % u.length];
			return {
				...e,
				datasetKey: r,
				series: JSON.parse(JSON.stringify(e.series)).map((e) => R.value.style.chart.bars.distributed ? Math.abs(e) : e),
				signedSeries: e.series.map((e) => e >= 0 ? 1 : -1),
				absoluteIndex: t,
				id: n,
				color: a
			};
		})), q = h(() => {
			let e = Math.max(...K.value.filter((e) => !F.value.includes(e.id)).map((e) => e.series.length));
			return isFinite(e) ? e : Math.max(...K.value.map((e) => e.series.length));
		});
		function Dr(e) {
			Kn.value = e;
		}
		let J = T({
			start: 0,
			end: Math.max(...Yn.value.map((e) => e.series.length))
		}), Y = T({
			start: 0,
			end: Math.max(...Yn.value.map((e) => e.series.length))
		}), Or = h(() => R.value.style.chart.zoom.preview.enable && (Y.value.start !== J.value.start || Y.value.end !== J.value.end));
		function kr(e, t) {
			Y.value[e] = t;
		}
		function Ar() {
			let e = q.value, t = Math.max(0, Math.min(J.value.start ?? 0, e - 1)), n = Math.max(t + 1, Math.min(J.value.end ?? e, e));
			(!Number.isFinite(t) || !Number.isFinite(n) || n <= t) && (t = 0, n = e), J.value.start = t, J.value.end = n, Y.value.start = t, Y.value.end = n, bn.value && (bn.value.setStartValue(t), bn.value.setEndValue(n));
		}
		let jr = T(null);
		function Mr() {
			return new Promise((e) => requestAnimationFrame(() => requestAnimationFrame(() => e())));
		}
		Ze(() => {
			jr.value && cancelAnimationFrame(jr.value);
		});
		async function Nr({ force: e = !1 } = {}) {
			if (R.value.style.chart.zoom.keepState && !e && Gi.value && (J.value.start !== 0 || J.value.end !== 0)) {
				Ar();
				return;
			}
			Ki(), await Xe(), jr.value && cancelAnimationFrame(jr.value), jr.value = requestAnimationFrame(async () => {
				await Mr(), Ki();
			});
		}
		let X = h(() => {
			let e;
			return e = R.value.orientation === "vertical" ? G.value.width / (J.value.end - J.value.start) : G.value.height / (J.value.end - J.value.start), e <= 0 ? 0 : e;
		}), Pr = h(() => f(K.value.filter((e) => !F.value.includes(e.id))).slice(J.value.start, J.value.end)), Fr = h(() => F.value.length === K.value.length), Ir = h(() => R.value.style.chart.zoom.minimap.show ? f(K.value.map((e) => ({
			...e,
			series: e.series.map((e) => e ?? 0)
		})).filter((e) => Fr.value ? !0 : !F.value.includes(e.id))) : []), Lr = h(() => R.value.style.chart.zoom.minimap.show ? [{
			name: "",
			series: Ir.value,
			color: "#000000",
			isVisible: !0
		}] : []), Rr = h(() => f(K.value.filter((e) => !F.value.includes(e.id)).map((e) => ({
			...e,
			series: e.series.map((t, n) => e.signedSeries[n] === -1 && t >= 0 ? -t : t)
		}))).slice(J.value.start, J.value.end)), zr = h(() => {
			let e = K.value.filter((e) => !F.value.includes(e.id));
			return {
				positive: f(e.map((e) => ({
					...e,
					series: e.series.slice(J.value.start, J.value.end).map((e) => e >= 0 ? e : 0)
				}))),
				negative: f(e.map((e) => ({
					...e,
					series: e.series.slice(J.value.start, J.value.end).map((e) => e < 0 ? e : 0)
				})))
			};
		}), Br = h(() => {
			let e = R.value.style.chart.grid.scale.scaleMax !== null && !R.value.style.chart.bars.distributed ? R.value.style.chart.grid.scale.scaleMax : Math.max(...zr.value.positive), t = Math.min(...zr.value.negative), n = R.value.style.chart.grid.scale.scaleMin !== null && !R.value.style.chart.bars.distributed ? R.value.style.chart.grid.scale.scaleMin : [
				-Infinity,
				Infinity,
				NaN,
				void 0,
				null
			].includes(t) ? 0 : t;
			return !R.value.style.chart.bars.distributed && (R.value.style.chart.grid.scale.scaleMax !== null || R.value.style.chart.grid.scale.scaleMin !== null) ? fe(n > 0 ? 0 : n, e < 0 ? 0 : e, R.value.style.chart.grid.scale.ticks) : ee(n > 0 ? 0 : n, e < 0 ? 0 : e, R.value.style.chart.grid.scale.ticks);
		}), Vr = h(() => Br.value.ticks), Z = h(() => {
			let e = Br.value, t = e.max + Math.abs(e.min);
			return Vr.value.map((n) => ({
				zero: G.value.bottom - G.value.height * (Math.abs(e.min) / t),
				y: G.value.bottom - G.value.height * ((n + Math.abs(e.min)) / t),
				x: W.value ? G.value.right + 8 : G.value.left - 8,
				horizontal_zero: G.value.left + G.value.width * (Math.abs(e.min) / t),
				horizontal_x: G.value.left + G.value.width * ((n + Math.abs(e.min)) / t),
				horizontal_y: G.value.bottom - 8,
				value: n
			}));
		}), Hr = h(() => {
			let e = R.value.style.chart.grid.y.axisLabels, { prefix: t, suffix: n } = R.value.style.chart.bars.dataLabels;
			return Vr.value.map((r) => String(ie(e.formatter, r, p({
				p: t,
				v: r,
				s: n,
				r: e.rounding
			}), { datapoint: { value: r } }) ?? "")).join("|");
		});
		rt(() => [
			Hr.value,
			R.value.orientation,
			R.value.style.chart.grid.y.axisLabels.show,
			R.value.style.chart.grid.y.axisLabels.fontSize,
			R.value.style.chart.grid.y.axisLabels.bold,
			R.value.style.chart.grid.y.position
		], () => {
			R.value.style.chart.grid.y.axisLabels.show && Gn();
		}, { flush: "post" });
		let Q = T([]), Ur = T([]), Wr = 0;
		it(() => {
			let e = ++Wr;
			(async () => {
				let t = await ye({
					values: R.value.style.chart.grid.x.timeLabels.values,
					maxDatapoints: q.value,
					formatter: R.value.style.chart.grid.x.timeLabels.datetimeFormatter,
					start: J.value.start,
					end: J.value.end
				});
				e === Wr && (Q.value = t);
			})();
		});
		let Gr = 0;
		it(() => {
			let e = ++Gr;
			(async () => {
				let t = await ye({
					values: R.value.style.chart.grid.x.timeLabels.values,
					maxDatapoints: q.value,
					formatter: R.value.style.chart.grid.x.timeLabels.datetimeFormatter,
					start: 0,
					end: q.value
				});
				e === Gr && (Ur.value = t);
			})();
		});
		let Kr = h(() => {
			let e = R.value.style.chart.grid.x.timeLabels.modulo;
			return Q.value.length ? Math.min(e, [...new Set(Q.value.map((e) => e.text))].length) : e;
		}), qr = h(() => {
			let e = R.value.style.chart.grid.x.timeLabels, t = Q.value || [], n = Ur.value || [], r = J.value.start ?? 0, a = I.value, o = q.value, s = t.map((e) => e?.text ?? ""), c = n.map((e) => e?.text ?? "");
			return te(!!e.showOnlyFirstAndLast, !!e.showOnlyAtModulo, Math.max(1, Kr.value || 1), s, c, r, a, o);
		});
		it(() => {
			R.value.orientation, R.value.style.chart.grid.x.timeLabels.show, R.value.style.chart.grid.x.timeLabels.fontSize, R.value.style.chart.grid.x.timeLabels.rotation, R.value.style.chart.grid.x.timeLabels.offsetY, qr.value?.map((e) => e?.text ?? "").join("|"), Hr.value, H.value.width, H.value.height, Nn.value, Mn.value, An.value, wr();
		}, { flush: "post" });
		let Jr = T({
			months: [],
			shortMonths: [],
			days: [],
			shortDays: []
		}), Yr = 0;
		it(() => {
			let e = ++Yr, t = R.value.style.chart.grid.x.timeLabels.datetimeFormatter;
			(async () => {
				let n = await ve(t.locale).catch(() => ve("en"));
				e === Yr && (Jr.value = n.data);
			})();
		});
		let Xr = h(() => {
			let e = R.value.style.chart.grid.x.timeLabels.datetimeFormatter, t = _e({
				useUTC: e.useUTC,
				locale: Jr.value,
				januaryAsYear: e.januaryAsYear
			});
			return (e, n) => {
				let r = R.value.style.chart.grid.x.timeLabels.values?.[e];
				return r == null ? "" : t.formatDate(new Date(r), n);
			};
		}), Zr = h(() => (R.value.style.chart.grid.x.timeLabels.values || []).map((e, t) => ({
			text: Xr.value(t, R.value.style.chart.zoom.timeFormat),
			absoluteIndex: t
		}))), Qr = h(() => (R.value.style.chart.grid.x.timeLabels.values || []).map((e, t) => ({
			text: Xr.value(t, R.value.style.chart.tooltip.timeFormat),
			absoluteIndex: t
		}))), $r = h(() => {
			let e = /* @__PURE__ */ new Map();
			return (R.value.style.chart.grid.x.timeLabels.values || []).forEach((t) => {
				if ([
					null,
					void 0,
					""
				].includes(t)) return;
				let n = String(t);
				e.set(n, (e.get(n) || 0) + 1);
			}), e;
		});
		function ei(e) {
			let t = (R.value.style.chart.grid.x.timeLabels.values || [])[e];
			if ([
				null,
				void 0,
				""
			].includes(t)) return `fallback-${e}`;
			let n = String(t);
			return ($r.value.get(n) || 0) > 1 ? `${n}-${e}` : n;
		}
		function ti(e, t) {
			return `${e.datasetKey}:${ei(t)}`;
		}
		let $ = h(() => {
			if (!pn.value && !Jn.value) return [];
			let e = Array(q.value).fill(0), t = Array(q.value).fill(0), n = Array(q.value).fill(0), r = Array(q.value).fill(0), a = Math.max(...zr.value.positive) || 0, o = Math.min(...zr.value.negative), s = [
				-Infinity,
				Infinity,
				NaN,
				void 0,
				null
			].includes(o) ? 0 : o, { min: c, max: l } = !R.value.style.chart.bars.distributed && (R.value.style.chart.grid.scale.scaleMax !== null || R.value.style.chart.grid.scale.scaleMin !== null) ? fe(R.value.style.chart.grid.scale.scaleMin === null ? s > 0 ? 0 : s : R.value.style.chart.grid.scale.scaleMin, R.value.style.chart.grid.scale.scaleMax === null ? a < 0 ? 0 : a : R.value.style.chart.grid.scale.scaleMax, R.value.style.chart.grid.scale.ticks) : ee(R.value.style.chart.grid.scale.scaleMin === null ? s > 0 ? 0 : s : R.value.style.chart.grid.scale.scaleMin, R.value.style.chart.grid.scale.scaleMax === null ? a < 0 ? 0 : a : R.value.style.chart.grid.scale.scaleMax, R.value.style.chart.grid.scale.ticks), u = l + (c >= 0 ? 0 : Math.abs(c)) || 1, d = G.value.height, f = G.value.width, p = Z.value[0] ? Z.value[0].zero : G.value.bottom, te = Z.value[0] ? Z.value[0].horizontal_zero : G.value.left, ne = Array(Ir.value.length).fill(0), re = Array(Ir.value.length).fill(0);
			return K.value.filter((e) => !F.value.includes(e.id)).map((a) => {
				let o = a.series.slice(), s = o.map((e, t) => ne[t]), c = o.map((e, t) => re[t]);
				o.forEach((e, t) => {
					(Number(e) || 0) >= 0 ? ne[t] += Number(e) || 0 : re[t] += Math.abs(Number(e) || 0);
				});
				let l = a.series.slice(J.value.start, J.value.end), ee = a.signedSeries.slice(J.value.start, J.value.end), ie = l.map((e, t) => ti(a, J.value.start + t)), ae = o.map((e, t) => ti(a, t)), oe = l.map((e, t) => G.value.left + X.value * t + X.value * R.value.style.chart.bars.gapRatio / 4), se = (Ir.value || []).filter(Number.isFinite), ce = se.length ? Math.max(...se) : 0, le = se.length ? Math.min(...se) : 0;
				function ue({ minimapH: e }) {
					let t = 1e-9, n = ce > 0;
					if (n && le < 0) {
						let n = Math.max(ce, Math.abs(le)) || t;
						return {
							pxPerUnit: e / 2 / n,
							zero: e / 2
						};
					}
					return n ? {
						pxPerUnit: e / Math.max(t, ce),
						zero: e
					} : {
						pxPerUnit: e / Math.max(t, Math.abs(le)),
						zero: 0
					};
				}
				let de = ({ left: e, unitW: t }) => {
					let n = t * (R.value.style.chart.bars.gapRatio / 4);
					return o.map((r, a) => e + t * a + n);
				}, fe = l.map((e, t) => G.value.top + X.value * t + X.value * R.value.style.chart.bars.gapRatio / 4), pe = l.map((t, r) => {
					let a = R.value.style.chart.bars.distributed ? (t || 0) / Pr.value[r] : (t || 0) / u, o, s;
					return t > 0 ? (s = d * a, o = p - s - e[r], e[r] += s) : (s = d * a, o = p + n[r], n[r] += Math.abs(s)), o;
				}), me = ({ minimapH: e }) => {
					if (R.value.style.chart.bars.distributed) return o.map((t, n) => {
						let r = Math.abs(Number(t) || 0), a = Math.abs(Ir.value?.[n] || 0) || 1e-9;
						return e - (s[n] + r) / a * e;
					});
					let { pxPerUnit: t, zero: n } = ue({ minimapH: e });
					return o.map((e, r) => {
						let a = Number(e) || 0;
						return a >= 0 ? n - (s[r] + a) * t : n + c[r] * t;
					});
				}, he = l.map((e, n) => {
					let a = R.value.style.chart.bars.distributed ? (e || 0) / Pr.value[n] : (e || 0) / u, o, s;
					return e > 0 ? (s = f * a, o = te + t[n], t[n] += s) : (s = f * a, o = te - Math.abs(s) - r[n], r[n] += Math.abs(s)), o;
				}), ge = l.map((e, t) => {
					let n = R.value.style.chart.bars.distributed ? (e || 0) / Pr.value[t] : (e || 0) / u;
					return e > 0 ? d * n : d * Math.abs(n);
				}), _e = ({ minimapH: e }) => {
					if (R.value.style.chart.bars.distributed) return o.map((t, n) => Math.abs(Number(t) || 0) / (Math.abs(Ir.value?.[n] || 0) || 1e-9) * e);
					let { pxPerUnit: t } = ue({ minimapH: e });
					return o.map((e) => Math.abs(Number(e) || 0) * t);
				}, ve = l.map((e, t) => {
					let n = R.value.style.chart.bars.distributed ? (e || 0) / Pr.value[t] : (e || 0) / u;
					return e > 0 ? f * n : f * Math.abs(n);
				}), ye = l.map((e) => Math.abs(e)).reduce((e, t) => e + t, 0);
				return {
					...a,
					proportions: l.map((e, t) => R.value.style.chart.bars.distributed ? (e || 0) / Pr.value[t] : (e || 0) / ye),
					series: l,
					signedSeries: ee,
					rectKeys: ie,
					minimapRectKeys: ae,
					x: oe,
					y: pe,
					height: ge,
					horizontal_width: ve,
					horizontal_y: fe,
					horizontal_x: he,
					xMinimap: de,
					yMinimap: me,
					heightMinimap: _e
				};
			});
		}), ni = h(() => Rr.value.map((e, t) => ({
			value: e,
			sign: e >= 0 ? 1 : -1
		})));
		function ri(e, t, n, r, a) {
			let o = a === -1 && e >= 0 ? -e : e;
			return ie(R.value.style.chart.bars.dataLabels.formatter, o, p({
				p: R.value.style.chart.bars.dataLabels.prefix,
				v: o,
				s: R.value.style.chart.bars.dataLabels.suffix,
				r: R.value.style.chart.bars.dataLabels.rounding
			}), {
				datapoint: t,
				seriesIndex: n,
				datapointIndex: r
			});
		}
		function ii(e, t, n, r) {
			return ie(R.value.style.chart.bars.dataLabels.formatter, e, p({
				v: isNaN(e) ? 0 : e,
				s: "%",
				r: R.value.style.chart.bars.dataLabels.rounding
			}), {
				datapoint: t,
				seriesIndex: n,
				datapointIndex: r
			});
		}
		function ai(e) {
			let t = JSON.parse(JSON.stringify($.value)).map((t) => ({
				name: t.name,
				value: t.series[e] === 0 ? 0 : t.series[e] || null,
				proportion: t.proportions[e] || null,
				color: t.color,
				id: t.id
			}));
			R.value.events.datapointClick && R.value.events.datapointClick({
				datapoint: t,
				seriesIndex: e + J.value.start
			}), M("selectDatapoint", {
				datapoint: t,
				period: Q.value[e]
			});
		}
		function oi(e) {
			return JSON.parse(JSON.stringify($.value)).map((t) => ({
				name: t.name,
				absoluteIndex: t.absoluteIndex,
				value: t.series[e] === 0 ? 0 : (t.signedSeries[e] === -1 && t.series[e] >= 0 ? -t.series[e] : t.series[e]) || null,
				proportion: t.proportions[e] || null,
				color: t.color,
				id: t.id,
				timeLabel: Ur.value[e]
			}));
		}
		function si(e) {
			if (R.value.events.datapointLeave) {
				let t = oi(e);
				R.value.events.datapointLeave({
					datapoint: t,
					seriesIndex: e + J.value.start
				});
			}
			mn.value = !1, I.value = null, L.value === e && (L.value = null);
		}
		let ci = h(() => Array(q.value).fill(0).map((e, t) => oi(t)));
		rt(() => j.selectedXIndex, (e) => {
			if ([null, void 0].includes(j.selectedXIndex)) {
				I.value = null;
				return;
			}
			let t = e - J.value.start;
			t < 0 || e >= J.value.end ? I.value = null : I.value = t ?? null;
		}, { immediate: !0 });
		function li(e) {
			if (!R.value.style.chart.tooltip.showTimeLabel) return null;
			let t = Q.value?.[e]?.text || null, n = Qr.value?.[e]?.text || null, r = Ur.value?.[e]?.text || null;
			return R.value.style.chart.tooltip.useDefaultTimeFormat ? t : n || r;
		}
		function ui(e, t = "pointer") {
			if (Fr.value) return;
			Vn.value = t, L.value = e, I.value = e, mn.value = !0;
			let n = R.value.style.chart.tooltip.customFormat, r = oi(e);
			Xi({
				seriesIndex: e,
				datapoint: r
			}), R.value.events.datapointEnter && R.value.events.datapointEnter({
				datapoint: r,
				seriesIndex: e + J.value.start
			}), hn.value = {
				timeLabel: li(e),
				datapoint: r,
				seriesIndex: e,
				config: R.value,
				series: $.value
			};
			let a = r.map((e) => Math.abs(e.value)).reduce((e, t) => e + t, 0), o = r.map((e) => oe(e.value)).reduce((e, t) => e + t, 0);
			if (me(n) && ne(() => n({
				seriesIndex: e,
				datapoint: r,
				series: $.value,
				config: R.value
			}))) gn.value = n({
				seriesIndex: e,
				datapoint: r,
				series: $.value,
				config: R.value
			});
			else {
				let { showValue: t, showTotal: n, totalTranslation: s, showPercentage: c, borderColor: l, roundingValue: u, roundingPercentage: d } = R.value.style.chart.tooltip, f = "", ee = li(e);
				ee && (f += `<div style="width:100%;text-align:center;border-bottom:1px solid ${l};padding-bottom:6px;margin-bottom:3px;">${ee}</div>`), n && (f += `<div class="vue-data-ui-tooltip-total" style="display:flex;flex-direction:row;align-items:center;gap:4px">
                <span>${s}:</span>
                <span>
                    ${ie(R.value.style.chart.bars.dataLabels.formatter, o, p({
					p: R.value.style.chart.bars.dataLabels.prefix,
					v: o,
					s: R.value.style.chart.bars.dataLabels.suffix,
					r: u
				}), { datapoint: {
					name: s,
					value: o
				} })}
                </span>
            </div>`);
				let te = [t && c ? "(" : "", t && c ? ")" : ""];
				r.reverse().forEach((e) => {
					f += `
                <div style="display:flex;flex-direction:row;align-items:center;gap:4px">
                    <svg viewBox="0 0 60 60" height="14" width="14"><rect rx="5" x="0" y="0" height="60" width="60" stroke="none" fill="${R.value.style.chart.bars.gradient.show ? `url(#gradient_${e.id})` : e.color}"/>${fn.pattern ? `<rect rx="5" x="0" y="0" height="60" width="60" stroke="none" fill="url(#pattern_${P.value}_${e.absoluteIndex})"/>` : ""}</svg>
                    ${e.name}${t || c ? ":" : ""} ${t ? ie(R.value.style.chart.bars.dataLabels.formatter, e.value, p({
						p: R.value.style.chart.bars.dataLabels.prefix,
						v: e.value,
						s: R.value.style.chart.bars.dataLabels.suffix,
						r: u
					}, { datapoint: e })) : ""} ${te[0]}${c ? p({
						v: isNaN(e.value / a) ? 0 : Math.abs(e.value) / a * 100,
						s: "%",
						r: d
					}) : ""}${te[1]}
                </div>
            `;
				}), gn.value = `<div>${f}</div>`;
			}
		}
		let di = h(() => H.value.width), fi = h(() => H.value.height);
		ke({
			timeLabelsEls: R.value.orientation === "vertical" ? Nn : Mn,
			timeLabels: Q,
			slicer: J,
			configRef: R,
			rotationPath: [
				"style",
				"chart",
				"grid",
				"x",
				"timeLabels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"grid",
				"x",
				"timeLabels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: di,
			height: fi,
			rotation: R.value.style.chart.grid.x.timeLabels.autoRotate.angle
		});
		function pi(e) {
			Sn.value = e, _n.value += 1;
		}
		function mi() {
			V.value.showTable = !V.value.showTable;
		}
		function hi() {
			V.value.dataLabels.show = !V.value.dataLabels.show;
		}
		function gi() {
			V.value.showTooltip = !V.value.showTooltip;
		}
		function _i() {
			return $.value;
		}
		let vi = h(() => {
			if ($.value.length === 0) return {
				head: [],
				body: [],
				config: {},
				columnNames: []
			};
			let e = $.value.map(({ name: e, color: t }) => ({
				label: e,
				color: t
			})), t = [];
			return Q.value.forEach((e) => {
				let n = [R.value.style.chart.grid.x.timeLabels.values[e.absoluteIndex] ? e.text : i + 1];
				K.value.forEach((t) => {
					n.push(Number((t.series[e.absoluteIndex] || 0).toFixed(R.value.table.td.roundingValue)));
				}), t.push(n);
			}), {
				head: e,
				body: t
			};
		});
		function yi(e = null) {
			let r = [
				[R.value.style.chart.title.text],
				[R.value.style.chart.title.subtitle.text],
				[""]
			], a = ["", ...vi.value.head.map((e) => e.label)], o = vi.value.body, s = r.concat([a]).concat(o), c = n(s);
			e ? e(c) : t({
				csvContent: c,
				title: R.value.style.chart.title.text || "vue-ui-stackbar"
			});
		}
		let bi = h(() => {
			let e = [""].concat($.value.map((e) => e.name), " <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>"), t = [], n = J.value.end - J.value.start;
			for (let e = 0; e < n; e += 1) {
				let n = J.value.start + e, r = $.value.map((t) => t.series[e] ?? 0).reduce((e, t) => e + t, 0);
				t.push([R.value.style.chart.grid.x.timeLabels.values[n] && Q.value[e]?.text || n + 1].concat($.value.map((t) => (t.series[e] ?? 0).toFixed(R.value.table.td.roundingValue)), (r ?? 0).toFixed(R.value.table.td.roundingValue)));
			}
			let r = {
				th: {
					backgroundColor: R.value.table.th.backgroundColor,
					color: R.value.table.th.color,
					outline: R.value.table.th.outline
				},
				td: {
					backgroundColor: R.value.table.td.backgroundColor,
					color: R.value.table.td.color,
					outline: R.value.table.td.outline
				},
				breakpoint: R.value.table.responsiveBreakpoint
			}, a = [R.value.table.columnNames.period].concat($.value.map((e) => e.name), R.value.table.columnNames.total);
			return {
				head: e,
				body: t.slice(0, J.value.end - J.value.start),
				config: r,
				colNames: a
			};
		});
		function xi() {
			F.value.length ? F.value = [] : Ei.value.forEach((e) => {
				F.value.push(e.id);
			}), M("selectLegend", $.value);
		}
		function Si(e) {
			if (F.value.includes(e.id)) F.value = F.value.filter((t) => t !== e.id);
			else {
				if (F.value.length === K.value.length - 1) return;
				F.value.push(e.id);
			}
			M("selectLegend", $.value);
		}
		function Ci(e) {
			return K.value.length ? K.value.find((t) => t.name === e) || (pr.value && console.warn(`VueUiStackbar - Series name not found "${e}"`), null) : (pr.value && console.warn("VueUiStackbar - There are no series to show."), null);
		}
		function wi(e) {
			let t = Ci(e);
			t !== null && F.value.includes(t.id) && Si({ id: t.id });
		}
		function Ti(e) {
			let t = Ci(e);
			t !== null && (F.value.includes(t.id) || Si({ id: t.id }));
		}
		let Ei = h(() => K.value.map((e, t) => ({
			...e,
			shape: "square"
		})).map((e) => ({
			...e,
			opacity: F.value.includes(e.id) ? .5 : 1,
			segregate: () => Si(e),
			isSegregated: F.value.includes(e.id)
		}))), Di = h(() => ({
			cy: "stackbar-legend",
			backgroundColor: R.value.style.chart.legend.backgroundColor,
			color: R.value.style.chart.legend.color,
			fontSize: R.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: R.value.style.chart.legend.bold ? "bold" : ""
		})), Oi = T(!1);
		function ki() {
			Oi.value = !Oi.value;
		}
		function Ai(e, t) {
			let n = JSON.parse(JSON.stringify($.value)).map((e) => ({
				name: e.name,
				value: e.series[t] === 0 ? 0 : (e.signedSeries[t] === -1 && e.series[t] >= 0 ? -e.series[t] : e.series[t]) || null,
				proportion: e.proportions[t] || null,
				color: e.color,
				id: e.id
			}));
			M("selectTimeLabel", {
				datapoint: n,
				absoluteIndex: e.absoluteIndex,
				label: e.text
			});
		}
		let ji = h(() => Math.max(...$.value.flatMap((e) => e.series)));
		function Mi(e, t) {
			return R.value.style.chart.bars.showDistributedPercentage && R.value.style.chart.bars.distributed ? ar.value ? t * 100 >= R.value.style.chart.bars.dataLabels.hideUnderPercentage : !R.value.style.chart.bars.dataLabels.hideEmptyPercentages || t > 0 : ar.value ? (ir.value && pr.value && console.warn("Vue Data UI - VueUiStackbar - You cannot set both dataLabels.hideUnderPercentage and dataLabels.hideUnderValue. Note that dataLabels.hideUnderPercentage takes precedence in this case."), e > ji.value * R.value.style.chart.bars.dataLabels.hideUnderPercentage / 100) : ir.value ? Math.abs(e) >= R.value.style.chart.bars.dataLabels.hideUnderValue : !R.value.style.chart.bars.dataLabels.hideEmptyValues || e !== 0;
		}
		async function Ni({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { imageUri: t, base64: n } = await Ae({
				domElement: N.value,
				base64: !0,
				img: !0,
				scale: e
			}), r = N.value.getBoundingClientRect(), a = {
				width: r.width,
				height: r.height,
				aspectRatio: r.height ? r.width / r.height : 0
			}, o = await re(t, e) ?? a;
			return {
				imageUri: t,
				base64: n,
				title: R.value.style.chart.title.text,
				...o
			};
		}
		let Pi = h(() => {
			let e = R.value.table.useDialog && !R.value.table.show, t = V.value.showTable;
			return {
				component: e ? cn : nn,
				title: `${R.value.style.chart.title.text}${R.value.style.chart.title.subtitle.text ? `: ${R.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: R.value.table.th.backgroundColor,
					color: R.value.table.th.color,
					headerColor: R.value.table.th.color,
					headerBg: R.value.table.th.backgroundColor,
					isFullscreen: Sn.value,
					fullscreenParent: N.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: B.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: R.value.style.chart.backgroundColor,
							color: R.value.style.chart.color
						},
						head: {
							backgroundColor: R.value.style.chart.backgroundColor,
							color: R.value.style.chart.color
						}
					}
				}
			};
		});
		rt(() => V.value.showTable, (e) => {
			R.value.table.show || (e && R.value.table.useDialog && On.value ? On.value.open() : "close" in On.value && On.value.close());
		});
		function Fi() {
			V.value.showTable = !1, kn.value && kn.value.setTableIconState(!1);
		}
		let Ii = h(() => R.value.style.chart.backgroundColor), Li = h(() => R.value.style.chart.legend), Ri = h(() => R.value.style.chart.title), { isCallbackImaging: zi, isCallbackSvg: Bi, generateSvg: Vi, onGenerateImage: Hi } = Ee({
			svg: er,
			title: Ri,
			legend: Li,
			legendItems: Ei,
			backgroundColor: Ii,
			getSvgCallback: () => R.value.userOptions.callbacks.svg,
			generateImage: lr
		});
		function Ui(e) {
			let t = q.value;
			return e > t ? t : e < 0 || e < J.value.start ? R.value.style.chart.zoom.startIndex === null ? 1 : J.value.start + 1 : e;
		}
		let Wi = T(!1), Gi = T(!1);
		function Ki() {
			if (!Wi.value) {
				Wi.value = !0;
				try {
					let { startIndex: e, endIndex: t, keepState: n } = R.value.style.chart.zoom, r = n ? Math.max(0, q.value) : q.value;
					if (n && r <= 0) return;
					let a = e ?? 0, o = t == null ? r : Math.min(Ui(t + 1), r);
					qi.value = !0, J.value.start = a, J.value.end = o, Y.value.start = a, Y.value.end = o, Ar(), Gi.value = !0;
				} finally {
					queueMicrotask(() => {
						qi.value = !1;
					}), Wi.value = !1;
				}
			}
		}
		let qi = T(!1);
		function Ji(e) {
			Wi.value || qi.value || e !== J.value.start && (J.value.start = e, Y.value.start = e, Ar());
		}
		function Yi(e) {
			if (Wi.value || qi.value) return;
			let t = Ui(e);
			t !== J.value.end && (J.value.end = t, Y.value.end = t, Ar());
		}
		function Xi({ seriesIndex: e, datapoint: t }) {
			let n = J.value.start + e;
			M("selectX", {
				dataset: t,
				index: n,
				indexLabel: R.value.style.chart.grid.x.timeLabels.values[n]
			});
		}
		function Zi() {
			return { y0: Z.value?.[0]?.zero ?? G.value.bottom };
		}
		function Qi(e) {
			let { y0: t } = Zi(), n = R.value.style.chart.bars.totalValues, r = Math.max(2, n.fontSize * .3 + n.offsetY), a = Infinity, o = !1;
			for (let t of $.value || []) {
				let n = t?.series?.[e] ?? 0, r = t?.height?.[e] ?? 0, s = t?.y?.[e];
				n > 0 && r > 0 && Number.isFinite(s) && (o = !0, s < a && (a = s));
			}
			let s = (o && Number.isFinite(a) ? a : t) - r;
			return Math.min(Math.max(s, 0), G.value.bottom);
		}
		function $i(e) {
			let { x0: t } = Zi(), n = Math.max(2, R.value.style.chart.bars.totalValues.fontSize * .3 + R.value.style.chart.bars.totalValues.offsetX), r = -Infinity, a = !1;
			for (let t of $.value || []) {
				let n = t?.series?.[e] ?? 0, o = t?.horizontal_x?.[e], s = t?.horizontal_width?.[e], c = Number.isFinite(s) ? Math.max(0, s) : 0;
				Number.isFinite(o) && n > 0 && c > 0 && (a = !0, r = Math.max(r, o + c));
			}
			return (a && Number.isFinite(r) ? r : t) + n;
		}
		async function ea() {
			if (M("copyAlt", {
				config: R.value,
				dataset: $.value
			}), !R.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(R.value.userOptions.callbacks.altCopy({
				config: R.value,
				dataset: $.value
			}));
		}
		let ta = T(!1);
		function na() {
			L.value = null, ta.value = !0;
		}
		function ra() {
			L.value = null, mn.value = !1, I.value = null, M("selectX", {
				index: null,
				indexLabel: null,
				dataset: null
			}), ta.value = !1;
		}
		function ia(e) {
			if (!er.value || Oi.value || document.activeElement !== er.value || Fr.value || !J.value.end && J.value.end !== 0) return;
			let t = R.value.orientation === "vertical", n = R.value.orientation === "horizontal", r = t && e.key === "ArrowLeft" || n && e.key === "ArrowUp", a = t && e.key === "ArrowRight" || n && e.key === "ArrowDown", o = e.key === "Enter" || e.key === " ", s = e.key === "Escape";
			if (!r && !a && !o && !s) return;
			let c = J.value.end - J.value.start;
			if (c <= 0) return;
			if (e.preventDefault(), e.stopPropagation(), s) {
				L.value = null, mn.value = !1, I.value = null, M("selectX", {
					index: null,
					indexLabel: null,
					dataset: null
				});
				return;
			}
			if (o) {
				if (L.value === null) return;
				ai(L.value);
				return;
			}
			let l = L.value, u = I.value;
			l !== null && l >= 0 && l < c ? a ? (l += 1, l >= c && (l = 0)) : r && (--l, l < 0 && (l = c - 1)) : u !== null && u >= 0 && u < c ? (l = a ? u + 1 : u - 1, l >= c && (l = 0), l < 0 && (l = c - 1)) : l = a ? 0 : c - 1, L.value = l, aa(l), ui(l, "keyboard");
		}
		function aa(e) {
			if (!Number.isFinite(e)) return;
			let t, n;
			R.value.orientation === "vertical" ? (t = G.value.left + e * X.value + X.value / 2, n = G.value.top + G.value.height / 2) : (t = G.value.left + G.value.width / 2, n = G.value.top + e * X.value + X.value / 2);
			let r = a(t, n, er.value);
			r && (Bn.value = r);
		}
		let oa = h(() => ({
			headers: bi.value?.colNames ?? [],
			rows: bi.value?.body ?? []
		}));
		return Ne({
			getData: _i,
			getImage: Ni,
			generatePdf: cr,
			generateCsv: yi,
			generateImage: lr,
			generateSvg: Vi,
			hideSeries: Ti,
			showSeries: wi,
			toggleTable: mi,
			toggleLabels: hi,
			toggleTooltip: gi,
			toggleAnnotator: ki,
			toggleFullscreen: pi,
			copyAlt: ea
		}), (t, n) => (w(), v("div", {
			id: `stackbar_${P.value}`,
			ref_key: "stackbarChart",
			ref: N,
			class: x({
				"vue-data-ui-component": !0,
				"vue-ui-stackbar": !0,
				"vue-data-ui-wrapper-fullscreen": Sn.value
			}),
			style: C(`background:${R.value.style.chart.backgroundColor};color:${R.value.style.chart.color};font-family:${R.value.style.fontFamily}; position: relative; ${R.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: tr,
			onMouseleave: nr
		}, [
			y("div", {
				id: `chart-instructions-${P.value}`,
				class: "sr-only"
			}, [y("p", null, O(R.value.a11y.translations.keyboardNavigation), 1)], 8, st),
			oa.value?.rows?.length ? (w(), g(ze, {
				key: 0,
				uid: P.value,
				head: oa.value.headers,
				body: oa.value.rows,
				notice: R.value.a11y.translations.tableAvailable,
				caption: R.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : _("", !0),
			R.value.userOptions.buttons.annotator ? (w(), g(k(an), {
				key: 1,
				svgRef: k(er),
				backgroundColor: R.value.style.chart.backgroundColor,
				color: R.value.style.chart.color,
				active: Oi.value,
				isCursorPointer: B.value,
				onClose: ki
			}, {
				"annotator-action-close": A(() => [D(t.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": A(({ color: e }) => [D(t.$slots, "annotator-action-color", S(b({ color: e })), void 0, !0)]),
				"annotator-action-draw": A(({ mode: e }) => [D(t.$slots, "annotator-action-draw", S(b({ mode: e })), void 0, !0)]),
				"annotator-action-undo": A(({ disabled: e }) => [D(t.$slots, "annotator-action-undo", S(b({ disabled: e })), void 0, !0)]),
				"annotator-action-redo": A(({ disabled: e }) => [D(t.$slots, "annotator-action-redo", S(b({ disabled: e })), void 0, !0)]),
				"annotator-action-delete": A(({ disabled: e }) => [D(t.$slots, "annotator-action-delete", S(b({ disabled: e })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : _("", !0),
			D(t.$slots, "userConfig", {}, void 0, !0),
			R.value.style.chart.title.text ? (w(), v("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: vn,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(w(), g(je, {
				key: `title_${wn.value}`,
				config: {
					title: {
						cy: "stackbar-title",
						...R.value.style.chart.title
					},
					subtitle: {
						cy: "stackbar-subtitle",
						...R.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : _("", !0),
			y("div", { id: `legend-top-${P.value}` }, null, 8, ct),
			R.value.userOptions.show && pn.value && (k($n) || k(Zn)) ? (w(), g(k(on), {
				ref_key: "userOptionsRef",
				ref: kn,
				key: `user_option_${_n.value}`,
				backgroundColor: R.value.style.chart.backgroundColor,
				color: R.value.style.chart.color,
				isPrinting: k(or),
				isImaging: k(sr),
				uid: P.value,
				hasTooltip: R.value.style.chart.tooltip.show && R.value.userOptions.buttons.tooltip,
				hasPdf: R.value.userOptions.buttons.pdf,
				hasImg: R.value.userOptions.buttons.img,
				hasSvg: R.value.userOptions.buttons.svg,
				hasXls: R.value.userOptions.buttons.csv,
				hasTable: R.value.userOptions.buttons.table,
				hasLabel: R.value.userOptions.buttons.labels,
				hasFullscreen: R.value.userOptions.buttons.fullscreen,
				hasAltCopy: R.value.userOptions.buttons.altCopy,
				isFullscreen: Sn.value,
				chartElement: N.value,
				position: R.value.userOptions.position,
				isTooltip: V.value.showTooltip,
				titles: { ...R.value.userOptions.buttonTitles },
				hasAnnotator: R.value.userOptions.buttons.annotator,
				isAnnotation: Oi.value,
				callbacks: R.value.userOptions.callbacks,
				printScale: R.value.userOptions.print.scale,
				tableDialog: R.value.table.useDialog,
				isCursorPointer: B.value,
				onToggleFullscreen: pi,
				onGeneratePdf: k(cr),
				onGenerateCsv: yi,
				onGenerateImage: k(Hi),
				onGenerateSvg: k(Vi),
				onToggleTable: mi,
				onToggleLabels: hi,
				onToggleTooltip: gi,
				onToggleAnnotator: ki,
				onCopyAlt: ea,
				style: C({ visibility: k($n) ? k(Zn) ? "visible" : "hidden" : "visible" })
			}, Ge({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: A(({ isOpen: e, color: n }) => [D(t.$slots, "menuIcon", S(b({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: A(() => [D(t.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: A(() => [D(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: A(() => [D(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: A(() => [D(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionSvg ? {
					name: "optionSvg",
					fn: A(() => [D(t.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionTable ? {
					name: "optionTable",
					fn: A(() => [D(t.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionLabels ? {
					name: "optionLabels",
					fn: A(() => [D(t.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: A(({ toggleFullscreen: e, isFullscreen: n }) => [D(t.$slots, "optionFullscreen", S(b({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: A(({ toggleAnnotator: e, isAnnotator: n }) => [D(t.$slots, "optionAnnotator", S(b({
						toggleAnnotator: e,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: A(({ altCopy: e }) => [D(t.$slots, "optionAltCopy", S(b({ altCopy: e })), void 0, !0)]),
					key: "10"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: A(() => [D(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: A(() => [D(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasLabel.hasFullscreen.hasAltCopy.isFullscreen.chartElement.position.isTooltip.titles.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : _("", !0),
			y("div", lt, [(w(), v("svg", {
				ref_key: "svgRef",
				ref: er,
				xmlns: k(ue),
				"aria-describedby": `chart-instructions-${P.value}`,
				viewBox: `0 0 ${G.value.chartWidth <= 0 ? 10 : G.value.chartWidth} ${G.value.chartHeight <= 0 ? 10 : G.value.chartHeight}`,
				class: x({
					"vue-data-ui-loading": k(Jn),
					"vue-data-ui-fullscreen--on": Sn.value,
					"vue-data-ui-fulscreen--off": !Sn.value,
					"vue-data-ui-no-transition": !k(z)
				}),
				style: C(`max-width:100%;overflow:visible;background:transparent;color:${R.value.style.chart.color}`),
				tabindex: "0",
				onFocus: na,
				onBlur: ra,
				onKeydown: ia
			}, [
				qe(k(sn)),
				t.$slots["chart-background"] ? (w(), v("foreignObject", {
					key: 0,
					x: G.value.left,
					y: G.value.top,
					width: G.value.width <= 0 ? 10 : G.value.width,
					height: G.value.height <= 0 ? 10 : G.value.height,
					style: { pointerEvents: "none" }
				}, [D(t.$slots, "chart-background", {}, void 0, !0)], 8, dt)) : _("", !0),
				R.value.style.chart.bars.gradient.show ? (w(), v("defs", ft, [(w(!0), v(m, null, E($.value, (e, t) => (w(), g(Ie, {
					t: "linear",
					id: `gradient_${e.id}`,
					key: `gradient_${e.id}_${t}`,
					x1: "0%",
					y1: "0%",
					x2: "0%",
					y2: "100%",
					stops: [
						[
							"0%",
							e.color,
							1
						],
						[
							"61.8%",
							k(l)(e.color, R.value.style.chart.bars.gradient.intensity / 100),
							1
						],
						[
							"100%",
							e.color,
							1
						]
					]
				}, null, 8, ["id", "stops"]))), 128))])) : _("", !0),
				R.value.style.chart.grid.frame.show ? (w(), v("rect", {
					key: 2,
					style: {
						pointerEvents: "none",
						transition: "none",
						animation: "none !important"
					},
					x: Math.max(0, G.value.left),
					y: Math.max(0, G.value.top),
					width: Math.max(0, G.value.width),
					height: Math.max(0, G.value.height),
					fill: "transparent",
					stroke: R.value.style.chart.grid.frame.stroke,
					"stroke-width": R.value.style.chart.grid.frame.strokeWidth,
					"stroke-linecap": R.value.style.chart.grid.frame.strokeLinecap,
					"stroke-linejoin": R.value.style.chart.grid.frame.strokeLinejoin,
					"stroke-dasharray": R.value.style.chart.grid.frame.strokeDasharray
				}, null, 8, pt)) : _("", !0),
				R.value.style.chart.grid.x.showHorizontalLines && R.value.orientation === "vertical" ? (w(!0), v(m, { key: 3 }, E(Z.value, (e, t) => (w(), v("line", {
					x1: G.value.left,
					x2: G.value.right,
					y1: e.y,
					y2: e.y,
					stroke: R.value.style.chart.grid.x.linesColor,
					"stroke-width": R.value.style.chart.grid.x.linesThickness,
					"stroke-dasharray": R.value.style.chart.grid.x.linesStrokeDasharray,
					"stroke-linecap": "round"
				}, null, 8, mt))), 256)) : _("", !0),
				R.value.style.chart.grid.x.showHorizontalLines && R.value.orientation === "horizontal" ? (w(!0), v(m, { key: 4 }, E(J.value.end - J.value.start + 1, (e, t) => (w(), v("line", {
					x1: G.value.left,
					x2: G.value.right,
					y1: G.value.top + X.value * t,
					y2: G.value.top + X.value * t,
					stroke: R.value.style.chart.grid.x.linesColor,
					"stroke-width": R.value.style.chart.grid.x.linesThickness,
					"stroke-dasharray": R.value.style.chart.grid.x.linesStrokeDasharray,
					"stroke-linecap": "round"
				}, null, 8, ht))), 256)) : _("", !0),
				R.value.style.chart.grid.y.showVerticalLines && R.value.orientation === "vertical" ? (w(!0), v(m, { key: 5 }, E(J.value.end - J.value.start + 1, (e, t) => (w(), v("line", {
					x1: G.value.left + X.value * t,
					x2: G.value.left + X.value * t,
					y1: G.value.top,
					y2: G.value.bottom,
					stroke: R.value.style.chart.grid.y.linesColor,
					"stroke-width": R.value.style.chart.grid.y.linesThickness,
					"stroke-dasharray": R.value.style.chart.grid.y.linesStrokeDasharray,
					"stroke-linecap": "round"
				}, null, 8, gt))), 256)) : _("", !0),
				R.value.style.chart.grid.y.showVerticalLines && R.value.orientation === "horizontal" ? (w(!0), v(m, { key: 6 }, E(Z.value, (e, t) => (w(), v("line", {
					x1: e.horizontal_x,
					x2: e.horizontal_x,
					y1: G.value.top,
					y2: G.value.bottom,
					stroke: R.value.style.chart.grid.y.linesColor,
					"stroke-width": R.value.style.chart.grid.y.linesThickness,
					"stroke-dasharray": R.value.style.chart.grid.y.linesStrokeDasharray,
					"stroke-linecap": "round"
				}, null, 8, _t))), 256)) : _("", !0),
				(w(!0), v(m, null, E($.value, (e, n) => (w(), v("g", { key: `stackbar-group-${e.datasetKey}-${n}-${P.value}` }, [t.$slots.pattern ? (w(), v("defs", vt, [D(t.$slots, "pattern", Ye({ ref_for: !0 }, {
					seriesIndex: e.absoluteIndex,
					patternId: `pattern_${P.value}_${e.absoluteIndex}`
				}), void 0, !0)])) : _("", !0), R.value.orientation === "vertical" ? (w(), v(m, { key: 1 }, [(w(!0), v(m, null, E(e.x, (t, n) => (w(), v("rect", {
					key: `stackbar-rect-${e.rectKeys[n]}`,
					x: t,
					y: k(oe)(e.y[n]),
					height: e.height[n] < 0 ? 1e-4 : e.height[n] || 0,
					rx: R.value.style.chart.bars.borderRadius > e.height[n] / 2 ? (e.height[n] < 0 ? 0 : e.height[n]) / 2 : R.value.style.chart.bars.borderRadius,
					width: X.value * (1 - R.value.style.chart.bars.gapRatio / 2),
					fill: R.value.style.chart.bars.gradient.show ? `url(#gradient_${e.id})` : e.color,
					stroke: R.value.style.chart.backgroundColor,
					"stroke-width": R.value.style.chart.bars.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					class: x({ "vue-data-ui-bar-transition": Cn.value })
				}, null, 10, yt))), 128)), t.$slots.pattern ? (w(), v("g", bt, [(w(!0), v(m, null, E(e.x, (t, n) => (w(), v("rect", {
					key: `stackbar-pattern-rect-${e.rectKeys[n]}`,
					x: t,
					y: k(oe)(e.y[n]),
					height: e.height[n] < 0 ? 1e-4 : e.height[n] || 0,
					rx: R.value.style.chart.bars.borderRadius > e.height[n] / 2 ? (e.height[n] < 0 ? 0 : e.height[n]) / 2 : R.value.style.chart.bars.borderRadius,
					width: X.value * (1 - R.value.style.chart.bars.gapRatio / 2),
					fill: `url(#pattern_${P.value}_${e.absoluteIndex})`,
					stroke: R.value.style.chart.backgroundColor,
					"stroke-width": R.value.style.chart.bars.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					class: x({ "vue-data-ui-bar-transition": Cn.value })
				}, null, 10, xt))), 128))])) : _("", !0)], 64)) : (w(), v(m, { key: 2 }, [(w(!0), v(m, null, E(e.horizontal_x, (t, n) => (w(), v("rect", {
					key: `stackbar-rect-${e.rectKeys[n]}`,
					x: k(oe)(t, G.value.left),
					y: e.horizontal_y[n] < 0 ? 0 : e.horizontal_y[n],
					width: k(oe)(e.horizontal_width[n] < 0 ? 1e-4 : e.horizontal_width[n]),
					rx: R.value.style.chart.bars.borderRadius > e.height[n] / 2 ? (e.height[n] < 0 ? 0 : e.height[n]) / 2 : R.value.style.chart.bars.borderRadius,
					height: X.value * (1 - R.value.style.chart.bars.gapRatio / 2),
					fill: R.value.style.chart.bars.gradient.show ? `url(#gradient_${e.id})` : e.color,
					stroke: R.value.style.chart.backgroundColor,
					"stroke-width": R.value.style.chart.bars.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					class: x({ "vue-data-ui-bar-transition": Cn.value })
				}, null, 10, St))), 128)), t.$slots.pattern ? (w(), v("g", Ct, [(w(!0), v(m, null, E(e.horizontal_x, (t, n) => (w(), v("rect", {
					key: `stackbar-pattern-rect-${e.rectKeys[n]}`,
					x: k(oe)(t, G.value.left),
					y: e.horizontal_y[n] < 0 ? 0 : e.horizontal_y[n],
					width: k(oe)(e.horizontal_width[n] < 0 ? 1e-4 : e.horizontal_width[n]),
					rx: R.value.style.chart.bars.borderRadius > e.height[n] / 2 ? (e.height[n] < 0 ? 0 : e.height[n]) / 2 : R.value.style.chart.bars.borderRadius,
					height: X.value * (1 - R.value.style.chart.bars.gapRatio / 2),
					fill: `url(#pattern_${P.value}_${e.absoluteIndex})`,
					stroke: R.value.style.chart.backgroundColor,
					"stroke-width": R.value.style.chart.bars.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					class: x({ "vue-data-ui-bar-transition": Cn.value })
				}, null, 10, wt))), 128))])) : _("", !0)], 64))]))), 128)),
				R.value.style.chart.grid.x.showAxis ? (w(), v("line", {
					key: 7,
					x1: G.value.left,
					x2: G.value.right,
					y1: G.value.bottom,
					y2: G.value.bottom,
					stroke: R.value.style.chart.grid.x.axisColor,
					"stroke-width": R.value.style.chart.grid.x.axisThickness,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 8, Tt)) : _("", !0),
				R.value.style.chart.grid.y.showAxis && !R.value.style.chart.bars.distributed ? (w(), v("line", {
					key: 8,
					x1: W.value ? G.value.right : G.value.left,
					x2: W.value ? G.value.right : G.value.left,
					y1: G.value.top,
					y2: G.value.bottom,
					stroke: R.value.style.chart.grid.y.axisColor,
					"stroke-width": R.value.style.chart.grid.y.axisThickness,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 8, Et)) : _("", !0),
				R.value.style.chart.grid.x.axisName.show && R.value.style.chart.grid.x.axisName.text ? (w(), v("text", {
					key: 9,
					ref_key: "xAxisLabel",
					ref: An,
					x: G.value.left + G.value.width / 2,
					y: G.value.chartHeight - 3,
					"font-size": R.value.style.chart.grid.x.axisName.fontSize,
					fill: R.value.style.chart.grid.x.axisName.color,
					"font-weight": R.value.style.chart.grid.x.axisName.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, O(R.value.style.chart.grid.x.axisName.text), 9, Dt)) : _("", !0),
				R.value.style.chart.grid.y.axisName.show && R.value.style.chart.grid.y.axisName.text ? (w(), v("text", {
					key: 10,
					ref_key: "yAxisLabel",
					ref: jn,
					transform: `translate(${W.value ? H.value.width - R.value.style.chart.grid.y.axisName.fontSize / 2 - R.value.style.chart.grid.y.axisName.offsetX : R.value.style.chart.grid.y.axisName.fontSize + R.value.style.chart.grid.y.axisName.offsetX}, ${G.value.top + G.value.height / 2}) rotate(-90)`,
					"font-size": R.value.style.chart.grid.y.axisName.fontSize,
					fill: R.value.style.chart.grid.y.axisName.color,
					"font-weight": R.value.style.chart.grid.y.axisName.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, O(R.value.style.chart.grid.y.axisName.text), 9, Ot)) : _("", !0),
				V.value.dataLabels.show && R.value.orientation === "vertical" ? (w(), v(m, { key: 11 }, [(w(!0), v(m, null, E($.value, (e, t) => (w(), v("g", { key: `dp_${e.id}` }, [(w(!0), v(m, null, E(e.x, (n, r) => (w(), v(m, { key: `rect_${e.id}_${J.value.start + r}` }, [Mi(e.series[r], e.proportions[r]) ? (w(), v("text", {
					key: 0,
					class: x({ "vue-data-ui-transition": k(z) }),
					transform: `translate(${n + X.value * (1 - R.value.style.chart.bars.gapRatio / 2) / 2}, ${e.y[r] + e.height[r] / 2 + R.value.style.chart.bars.dataLabels.fontSize / 3})`,
					"font-size": R.value.style.chart.bars.dataLabels.fontSize,
					fill: R.value.style.chart.bars.dataLabels.adaptColorToBackground ? k(le)(e.color) : R.value.style.chart.bars.dataLabels.color,
					"font-weight": R.value.style.chart.bars.dataLabels.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, O(R.value.style.chart.bars.showDistributedPercentage && R.value.style.chart.bars.distributed ? ii(e.proportions[r] * 100, e, t, r) : ri(e.series[r], e, t, r, e.signedSeries[r])), 11, kt)) : _("", !0)], 64))), 128))]))), 128)), R.value.style.chart.bars.totalValues.show && $.value.length > 1 ? (w(), v("g", {
					key: 0,
					ref_key: "sumTop",
					ref: Pn
				}, [(w(!0), v(m, null, E(ni.value, (e, t) => (w(), v(m, { key: `tl_${t + J.value.start}` }, [!R.value.style.chart.bars.dataLabels.hideEmptyValues || e.value !== 0 ? (w(), v("text", {
					key: 0,
					class: x({ "vue-data-ui-transition": k(z) }),
					transform: `translate(${G.value.left + X.value * t + X.value / 2}, ${Qi(t)})`,
					"text-anchor": "middle",
					"font-size": R.value.style.chart.bars.totalValues.fontSize,
					"font-weight": R.value.style.chart.bars.totalValues.bold ? "bold" : "normal",
					fill: R.value.style.chart.bars.totalValues.color
				}, O(ri(e.value, e, t, e.sign)), 11, At)) : _("", !0)], 64))), 128))], 512)) : _("", !0)], 64)) : _("", !0),
				V.value.dataLabels.show && R.value.orientation === "horizontal" ? (w(), v(m, { key: 12 }, [(w(!0), v(m, null, E($.value, (e, t) => (w(), v("g", null, [(w(!0), v(m, null, E(e.horizontal_x, (n, r) => (w(), v(m, null, [Mi(e.series[r], e.proportions[r]) ? (w(), v("text", {
					key: 0,
					class: x({ "vue-data-ui-transition": k(z) }),
					transform: `translate(${n + (e.horizontal_width[r] < 0 ? 1e-4 : e.horizontal_width[r]) / 2}, ${e.horizontal_y[r] + X.value * (1 - R.value.style.chart.bars.gapRatio / 2) / 2 + R.value.style.chart.bars.dataLabels.fontSize / 3})`,
					"font-size": R.value.style.chart.bars.dataLabels.fontSize,
					fill: R.value.style.chart.bars.dataLabels.adaptColorToBackground ? k(le)(e.color) : R.value.style.chart.bars.dataLabels.color,
					"font-weight": R.value.style.chart.bars.dataLabels.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, O(R.value.style.chart.bars.showDistributedPercentage && R.value.style.chart.bars.distributed ? ii(e.proportions[r] * 100, e, t, r) : ri(e.series[r], e, t, r, e.signedSeries[r])), 11, jt)) : _("", !0)], 64))), 256))]))), 256)), R.value.style.chart.bars.totalValues.show && $.value.length > 1 ? (w(), v("g", {
					key: 0,
					ref_key: "sumRight",
					ref: Fn
				}, [(w(!0), v(m, null, E(ni.value, (e, t) => (w(), v(m, null, [!R.value.style.chart.bars.dataLabels.hideEmptyValues || e.value !== 0 ? (w(), v("text", {
					key: 0,
					class: x({ "vue-data-ui-transition": k(z) }),
					transform: `translate(${$i(t)}, ${G.value.top + X.value * t + X.value / 2 + R.value.style.chart.bars.totalValues.fontSize / 3})`,
					"text-anchor": "start",
					"font-size": R.value.style.chart.bars.totalValues.fontSize,
					"font-weight": R.value.style.chart.bars.totalValues.bold ? "bold" : "normal",
					fill: R.value.style.chart.bars.totalValues.color
				}, O(ri(e.value, e, t, e.sign)), 11, Mt)) : _("", !0)], 64))), 256))], 512)) : _("", !0)], 64)) : _("", !0),
				R.value.style.chart.grid.y.axisLabels.show && !R.value.style.chart.bars.distributed && R.value.orientation === "vertical" ? (w(), v("g", {
					key: 13,
					ref_key: "scaleLabels",
					ref: Mn
				}, [(w(!0), v(m, null, E(Z.value, (e, t) => (w(), v("path", {
					key: `ty_${t}`,
					stroke: R.value.style.chart.grid.x.axisColor,
					class: x({ "vue-data-ui-transition": k(z) }),
					d: `M${W.value ? G.value.right : G.value.left},${e.y} ${W.value ? G.value.right + 6 : G.value.left - 6},${e.y}`,
					"stroke-width": 1,
					"stroke-linecap": "round"
				}, null, 10, Nt))), 128)), (w(!0), v(m, null, E(Z.value, (e, t) => (w(), v("text", {
					class: x({ "vue-data-ui-transition": k(z) }),
					key: `tl_${t}`,
					transform: `translate(${e.x}, ${e.y + R.value.style.chart.grid.y.axisLabels.fontSize / 3})`,
					"font-size": R.value.style.chart.grid.y.axisLabels.fontSize,
					"font-weight": R.value.style.chart.grid.y.axisLabels.bold ? "bold" : "normal",
					fill: R.value.style.chart.grid.y.axisLabels.color,
					"text-anchor": W.value ? "start" : "end"
				}, O(k(ie)(R.value.style.chart.grid.y.axisLabels.formatter, e.value, k(p)({
					p: R.value.style.chart.bars.dataLabels.prefix,
					v: e.value,
					s: R.value.style.chart.bars.dataLabels.suffix,
					r: R.value.style.chart.grid.y.axisLabels.rounding
				}), { datapoint: e })), 11, Pt))), 128))], 512)) : _("", !0),
				R.value.style.chart.grid.y.axisLabels.show && !R.value.style.chart.bars.distributed && R.value.orientation === "horizontal" ? (w(), v("g", {
					key: 14,
					ref_key: "scaleLabels",
					ref: Mn
				}, [(w(!0), v(m, null, E(Z.value, (e, t) => (w(), v("path", {
					key: `scy_${t}`,
					d: `M${e.horizontal_x},${G.value.bottom} ${e.horizontal_x},${G.value.bottom + 6}`,
					class: x({ "vue-data-ui-transition": k(z) }),
					stroke: R.value.style.chart.grid.x.axisColor,
					"stroke-width": 1,
					"stroke-linecap": "round"
				}, null, 10, Ft))), 128)), (w(!0), v(m, null, E(Z.value, (e, t) => (w(), v("text", {
					class: x(["vue-data-ui-time-label", { "vue-data-ui-transition": k(z) }]),
					key: `tly_${t}`,
					"font-size": R.value.style.chart.grid.x.timeLabels.fontSize,
					"font-weight": R.value.style.chart.grid.y.axisLabels.bold ? "bold" : "normal",
					fill: R.value.style.chart.grid.y.axisLabels.color,
					"text-anchor": R.value.style.chart.grid.x.timeLabels.rotation > 0 ? "start" : R.value.style.chart.grid.x.timeLabels.rotation < 0 ? "end" : "middle",
					transform: `translate(${e.horizontal_x}, ${G.value.bottom + R.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + R.value.style.chart.grid.x.timeLabels.offsetY}), rotate(${R.value.style.chart.grid.x.timeLabels.rotation})`
				}, O(k(ie)(R.value.style.chart.grid.y.axisLabels.formatter, e.value, k(p)({
					p: R.value.style.chart.bars.dataLabels.prefix,
					v: e.value,
					s: R.value.style.chart.bars.dataLabels.suffix,
					r: R.value.style.chart.grid.y.axisLabels.rounding
				}), { datapoint: e })), 11, It))), 128))], 512)) : _("", !0),
				R.value.style.chart.grid.x.timeLabels.show && R.value.orientation === "vertical" ? (w(), v("g", {
					key: 15,
					ref_key: "timeLabelsEls",
					ref: Nn
				}, [t.$slots["time-label"] ? (w(), v("g", Lt, [(w(!0), v(m, null, E(qr.value, (e, n) => (w(), v("g", null, [D(t.$slots, "time-label", Ye({ ref_for: !0 }, {
					x: G.value.left + X.value * n + X.value / 2,
					y: G.value.bottom + R.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + R.value.style.chart.grid.x.timeLabels.offsetY,
					fontSize: R.value.style.chart.grid.x.timeLabels.fontSize,
					fill: R.value.style.chart.grid.x.timeLabels.color,
					transform: `translate(${G.value.left + X.value * n + X.value / 2}, ${G.value.bottom + R.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + R.value.style.chart.grid.x.timeLabels.offsetY}), rotate(${R.value.style.chart.grid.x.timeLabels.rotation})`,
					absoluteIndex: e.absoluteIndex,
					content: e.text,
					textAnchor: R.value.style.chart.grid.x.timeLabels.rotation > 0 ? "start" : R.value.style.chart.grid.x.timeLabels.rotation < 0 ? "end" : "middle",
					show: !0
				}), void 0, !0)]))), 256))])) : (w(), v("g", Rt, [(w(!0), v(m, null, E(qr.value, (e, t) => (w(), v("g", null, [String(e.text).includes("\n") ? (w(), v("text", {
					class: x({ "vue-data-ui-transition": k(z) }),
					key: t + "-multi",
					"text-anchor": R.value.style.chart.grid.x.timeLabels.rotation > 0 ? "start" : R.value.style.chart.grid.x.timeLabels.rotation < 0 ? "end" : "middle",
					"font-size": R.value.style.chart.grid.x.timeLabels.fontSize,
					fill: R.value.style.chart.grid.x.timeLabels.color,
					transform: `
                                        translate(
                                        ${G.value.left + X.value * t + X.value / 2},
                                        ${G.value.bottom + R.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + R.value.style.chart.grid.x.timeLabels.offsetY}
                                        ),
                                        rotate(${R.value.style.chart.grid.x.timeLabels.rotation})
                                    `,
					style: C({ cursor: B.value ? "pointer" : "default" }),
					innerHTML: k(r)({
						content: String(e.text),
						fontSize: R.value.style.chart.grid.x.timeLabels.fontSize,
						fill: R.value.style.chart.grid.x.timeLabels.color,
						x: 0,
						y: 0
					}),
					onClick: () => Ai(e, t)
				}, null, 14, Bt)) : (w(), v("text", {
					class: "vue-data-ui-time-label",
					key: t,
					"text-anchor": R.value.style.chart.grid.x.timeLabels.rotation > 0 ? "start" : R.value.style.chart.grid.x.timeLabels.rotation < 0 ? "end" : "middle",
					"font-size": R.value.style.chart.grid.x.timeLabels.fontSize,
					"font-weight": R.value.style.chart.grid.x.timeLabels.bold ? "bold" : "normal",
					fill: R.value.style.chart.grid.x.timeLabels.color,
					transform: `translate(${G.value.left + X.value * t + X.value / 2}, ${G.value.bottom + R.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + R.value.style.chart.grid.x.timeLabels.offsetY}), rotate(${R.value.style.chart.grid.x.timeLabels.rotation})`,
					style: C({ cursor: B.value ? "pointer" : "default" }),
					onClick: () => Ai(e, t)
				}, O(e.text), 13, zt))]))), 256))]))], 512)) : _("", !0),
				R.value.style.chart.grid.x.timeLabels.show && R.value.orientation === "horizontal" ? (w(), v("g", {
					key: 16,
					ref_key: "timeLabelsEls",
					ref: Nn
				}, [t.$slots["time-label"] ? (w(), v("g", Vt, [(w(!0), v(m, null, E(Q.value, (e, n) => (w(), v("g", null, [D(t.$slots, "time-label", Ye({ ref_for: !0 }, {
					x: G.value.left - 8,
					y: G.value.top + X.value * n + X.value / 2 + R.value.style.chart.grid.y.axisLabels.fontSize / 3,
					fontSize: R.value.style.chart.grid.x.timeLabels.fontSize,
					fill: R.value.style.chart.grid.x.timeLabels.color,
					transform: null,
					absoluteIndex: e.absoluteIndex,
					content: e.text,
					textAnchor: "end",
					show: !0
				}), void 0, !0)]))), 256))])) : (w(), v("g", Ht, [(w(!0), v(m, null, E(Q.value, (e, t) => (w(), v("g", null, [String(e.text).includes("\n") ? (w(), v("text", {
					key: 1,
					"text-anchor": "end",
					"font-size": R.value.style.chart.grid.y.axisLabels.fontSize,
					"font-weight": R.value.style.chart.grid.y.axisLabels.bold ? "bold" : "normal",
					fill: R.value.style.chart.grid.y.axisLabels.color,
					transform: `translate(${G.value.left - 8}, ${G.value.top + X.value * t + X.value / 2 + R.value.style.chart.grid.y.axisLabels.fontSize / 3})`,
					style: C({ cursor: B.value ? "pointer" : "default" }),
					onClick: () => Ai(e, t),
					innerHTML: k(s)({
						content: String(e.text),
						fontSize: R.value.style.chart.grid.y.axisLabels.fontSize,
						fill: R.value.style.chart.grid.y.axisLabels.color,
						x: G.value.left - 8,
						y: 0
					})
				}, null, 12, Wt)) : (w(), v("text", {
					key: 0,
					"text-anchor": "end",
					"font-size": R.value.style.chart.grid.y.axisLabels.fontSize,
					"font-weight": R.value.style.chart.grid.y.axisLabels.bold ? "bold" : "normal",
					fill: R.value.style.chart.grid.y.axisLabels.color,
					transform: `translate(${G.value.left - 8}, ${G.value.top + X.value * t + X.value / 2 + R.value.style.chart.grid.y.axisLabels.fontSize / 3})`,
					style: C({ cursor: B.value ? "pointer" : "default" }),
					onClick: () => Ai(e, t)
				}, O(e.text), 13, Ut))]))), 256))]))], 512)) : _("", !0),
				V.value.showTooltip && R.value.orientation === "vertical" ? (w(!0), v(m, { key: 17 }, E(J.value.end - J.value.start, (e, t) => (w(), v("rect", {
					x: G.value.left + t * X.value,
					y: G.value.top,
					width: X.value,
					height: G.value.height < 0 ? 0 : G.value.height,
					onClick: () => ai(t),
					onMouseenter: () => ui(t, "pointer"),
					onMouseleave: () => si(t),
					fill: t === I.value || t === Kn.value ? R.value.style.chart.highlighter.color : "transparent",
					style: C({ opacity: R.value.style.chart.highlighter.opacity / 100 })
				}, null, 44, Gt))), 256)) : _("", !0),
				V.value.showTooltip && R.value.orientation === "horizontal" ? (w(!0), v(m, { key: 18 }, E(J.value.end - J.value.start, (e, t) => (w(), v("rect", {
					x: G.value.left,
					y: G.value.top + t * X.value,
					width: G.value.width < 0 ? 0 : G.value.width,
					height: X.value,
					onClick: () => ai(t),
					onMouseenter: () => ui(t, "pointer"),
					onMouseleave: () => si(t),
					fill: t === I.value || t === Kn.value ? R.value.style.chart.highlighter.color : "transparent",
					style: C({ opacity: R.value.style.chart.highlighter.opacity / 100 })
				}, null, 44, Kt))), 256)) : _("", !0),
				Or.value ? (w(), v("rect", Ye({ key: 19 }, Er.value, {
					"data-start": J.value.start,
					"data-end": J.value.end
				}), null, 16, qt)) : _("", !0),
				D(t.$slots, "svg", { svg: {
					drawingArea: G.value,
					slicer: J.value,
					data: $.value,
					isPrintingImg: k(or) || k(sr) || k(zi),
					isPrintingSvg: k(Bi),
					barWidth: X.value
				} }, void 0, !0)
			], 46, ut)), t.$slots.hint ? (w(), v("div", Jt, [D(t.$slots, "hint", S(b({
				hint: R.value.a11y.translations.keyboardNavigation,
				isVisible: ta.value
			})), void 0, !0)])) : _("", !0)]),
			t.$slots.watermark ? (w(), v("div", Yt, [D(t.$slots, "watermark", S(b({ isPrinting: k(or) || k(sr) || k(zi) || k(Bi) })), void 0, !0)])) : _("", !0),
			R.value.style.chart.zoom.show && pn.value && Gi.value && q.value > 6 ? (w(), g(Le, {
				key: 5,
				ref_key: "chartSlicer",
				ref: bn,
				"data-dom-to-png-ignore-layout": "",
				allMinimaps: Lr.value,
				background: R.value.style.chart.zoom.color,
				borderColor: R.value.style.chart.backgroundColor,
				customFormat: R.value.style.chart.zoom.customFormat,
				cutNullValues: !1,
				forceZeroCenter: !0,
				enableRangeHandles: R.value.style.chart.zoom.enableRangeHandles,
				enableSelectionDrag: R.value.style.chart.zoom.enableSelectionDrag,
				end: J.value.end,
				focusOnDrag: R.value.style.chart.zoom.focusOnDrag,
				focusRangeRatio: R.value.style.chart.zoom.focusRangeRatio,
				fontSize: R.value.style.chart.zoom.fontSize,
				immediate: !R.value.style.chart.zoom.preview.enable,
				inputColor: R.value.style.chart.zoom.color,
				isPreview: Or.value,
				labelLeft: R.value.style.chart.grid.x.timeLabels.values[J.value.start] ? Q.value?.[0]?.text ?? "" : "",
				labelRight: R.value.style.chart.grid.x.timeLabels.values[J.value.end - 1] ? Q.value?.at(-1)?.text ?? "" : "",
				max: Math.max(...e.dataset.map((e) => e.series.length)),
				min: 0,
				minimap: Ir.value,
				minimapCompact: R.value.style.chart.zoom.minimap.compact,
				minimapFrameColor: R.value.style.chart.zoom.minimap.frameColor,
				minimapIndicatorColor: R.value.style.chart.zoom.minimap.indicatorColor,
				minimapMerged: !1,
				minimapSelectedColor: R.value.style.chart.zoom.minimap.selectedColor,
				minimapSelectedColorOpacity: R.value.style.chart.zoom.minimap.selectedColorOpacity,
				minimapSelectedIndex: I.value,
				minimapSelectionRadius: 1,
				preciseLabels: Zr.value.length ? Zr.value : Ur.value,
				refreshEndPoint: R.value.style.chart.zoom.endIndex === null ? Math.max(...e.dataset.map((e) => e.series.length)) : R.value.style.chart.zoom.endIndex + 1,
				refreshStartPoint: R.value.style.chart.zoom.startIndex === null ? 0 : R.value.style.chart.zoom.startIndex,
				selectColor: R.value.style.chart.zoom.highlightColor,
				selectedSeries: ci.value,
				smoothMinimap: !1,
				start: J.value.start,
				textColor: R.value.style.chart.color,
				timeLabels: Ur.value,
				usePreciseLabels: R.value.style.chart.grid.x.timeLabels.datetimeFormatter.enable && !R.value.style.chart.zoom.useDefaultFormat,
				valueEnd: J.value.end,
				valueStart: J.value.start,
				verticalHandles: R.value.style.chart.zoom.minimap.verticalHandles,
				maxWidth: R.value.style.chart.zoom.maxWidth,
				minimapLeftInsetRatio: G.value.chartWidth > 0 && R.value.style.chart.zoom.autoFit ? G.value.left / G.value.chartWidth : null,
				minimapRightInsetRatio: G.value.chartWidth > 0 && R.value.style.chart.zoom.autoFit ? (G.value.chartWidth - G.value.right) / G.value.chartWidth : null,
				isCursorPointer: B.value,
				additionalMinimapHeight: R.value.style.chart.zoom.minimap.additionalHeight,
				handleType: R.value.style.chart.zoom.minimap.handleType,
				handleIconColor: R.value.style.chart.zoom.minimap.handleIconColor,
				handleBorderWidth: R.value.style.chart.zoom.minimap.handleBorderWidth,
				handleBorderColor: R.value.style.chart.zoom.minimap.handleBorderColor,
				handleFill: R.value.style.chart.zoom.minimap.handleFill,
				handleWidth: R.value.style.chart.zoom.minimap.handleWidth,
				"onUpdate:end": Yi,
				"onUpdate:start": Ji,
				onTrapMouse: Dr,
				onReset: n[0] ||= () => Nr({ force: !0 }),
				onFutureEnd: n[1] ||= (e) => kr("end", e),
				onFutureStart: n[2] ||= (e) => kr("start", e)
			}, {
				"reset-action": A(({ reset: e }) => [D(t.$slots, "reset-action", S(b({ reset: e })), void 0, !0)]),
				slotMap: A(({ width: e, height: t, unitW: n }) => [(w(!0), v(m, null, E($.value, (e) => (w(), v("g", { key: e.id }, [(w(!0), v(m, null, E(e.xMinimap({
					left: 0,
					unitW: n
				}), (r, a) => (w(), v("rect", {
					key: `minimap-rect-${e.minimapRectKeys[a]}`,
					x: a === 0 ? r - n * (R.value.style.chart.bars.gapRatio / 4) : r - n / 2,
					y: Math.max(0, Math.min(t, e.yMinimap({ minimapH: t })[a])),
					height: e.heightMinimap({ minimapH: t })[a],
					width: [0, q.value - 1].includes(a) ? n * (1 - R.value.style.chart.bars.gapRatio / 2) / 2 : n * (1 - R.value.style.chart.bars.gapRatio / 2),
					fill: R.value.style.chart.bars.gradient.show ? `url(#gradient_${e.id})` : e.color,
					stroke: R.value.style.chart.backgroundColor,
					"stroke-width": .5,
					rx: "0",
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					style: C({ opacity: a >= Y.value.start && a <= Y.value.end ? 1 : .62 })
				}, null, 12, Xt))), 128))]))), 128))]),
				_: 3
			}, 8, /* @__PURE__ */ "allMinimaps.background.borderColor.customFormat.enableRangeHandles.enableSelectionDrag.end.focusOnDrag.focusRangeRatio.fontSize.immediate.inputColor.isPreview.labelLeft.labelRight.max.minimap.minimapCompact.minimapFrameColor.minimapIndicatorColor.minimapSelectedColor.minimapSelectedColorOpacity.minimapSelectedIndex.preciseLabels.refreshEndPoint.refreshStartPoint.selectColor.selectedSeries.start.textColor.timeLabels.usePreciseLabels.valueEnd.valueStart.verticalHandles.maxWidth.minimapLeftInsetRatio.minimapRightInsetRatio.isCursorPointer.additionalMinimapHeight.handleType.handleIconColor.handleBorderWidth.handleBorderColor.handleFill.handleWidth".split("."))) : _("", !0),
			qe(k(en), {
				teleportTo: R.value.style.chart.tooltip.teleportTo,
				show: V.value.showTooltip && mn.value,
				backgroundColor: R.value.style.chart.tooltip.backgroundColor,
				color: R.value.style.chart.tooltip.color,
				fontSize: R.value.style.chart.tooltip.fontSize,
				borderRadius: R.value.style.chart.tooltip.borderRadius,
				borderColor: R.value.style.chart.tooltip.borderColor,
				borderWidth: R.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: R.value.style.chart.tooltip.backgroundOpacity,
				position: R.value.style.chart.tooltip.position,
				offsetX: R.value.style.chart.tooltip.offsetX,
				offsetY: R.value.style.chart.tooltip.offsetY,
				parent: N.value,
				content: gn.value,
				isFullscreen: Sn.value,
				isCustom: k(me)(R.value.style.chart.tooltip.customFormat),
				smooth: R.value.style.chart.tooltip.smooth,
				backdropFilter: R.value.style.chart.tooltip.backdropFilter,
				smoothForce: R.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: R.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: Vn.value === "keyboard",
				a11yPosition: Bn.value
			}, {
				"tooltip-before": A(() => [D(t.$slots, "tooltip-before", S(b({ ...hn.value })), void 0, !0)]),
				tooltip: A(() => [D(t.$slots, "tooltip", S(b({ ...hn.value })), void 0, !0)]),
				"tooltip-after": A(() => [D(t.$slots, "tooltip-after", S(b({ ...hn.value })), void 0, !0)]),
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
			y("div", { id: `legend-bottom-${P.value}` }, null, 8, Zt),
			Dn.value && (R.value.style.chart.legend.show || t.$slots.legend) ? (w(), g(We, {
				key: 6,
				to: R.value.style.chart.legend.position === "top" ? `#legend-top-${P.value}` : `#legend-bottom-${P.value}`
			}, [y("div", {
				ref_key: "chartLegend",
				ref: yn
			}, [D(t.$slots, "legend", { legend: Ei.value }, () => [R.value.style.chart.legend.show ? (w(), g(He, {
				key: 0,
				legendSet: Ei.value,
				config: Di.value,
				isCursorPointer: B.value,
				onClickMarker: n[3] ||= ({ legend: e }) => e.segregate()
			}, Ge({
				item: A(({ legend: e }) => [k(Jn) ? _("", !0) : (w(), v("div", {
					key: 0,
					onClick: (t) => e.segregate(),
					style: C(`opacity:${F.value.includes(e.id) ? .5 : 1}`)
				}, O(e.name), 13, Qt))]),
				legendToggle: A(() => [Ei.value.length > 2 && R.value.style.chart.legend.selectAllToggle.show && !k(Jn) ? (w(), g(Re, {
					key: 0,
					backgroundColor: R.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: R.value.style.chart.legend.selectAllToggle.color,
					fontSize: R.value.style.chart.legend.fontSize,
					checked: F.value.length > 0,
					isCursorPointer: B.value,
					onToggle: xi
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : _("", !0)]),
				_: 2
			}, [t.$slots.pattern ? {
				name: "legend-pattern",
				fn: A(({ legend: e, index: t }) => [qe(Me, {
					shape: e.shape,
					radius: 30,
					stroke: "none",
					plot: {
						x: 30,
						y: 30
					},
					fill: `url(#pattern_${P.value}_${t})`
				}, null, 8, ["shape", "fill"])]),
				key: "0"
			} : void 0]), 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : _("", !0)], !0)], 512)], 8, ["to"])) : _("", !0),
			t.$slots.source ? (w(), v("div", {
				key: 7,
				ref_key: "source",
				ref: xn,
				dir: "auto"
			}, [D(t.$slots, "source", {}, void 0, !0)], 512)) : _("", !0),
			pn.value && R.value.userOptions.buttons.table ? (w(), g($e(Pi.value.component), Ye({ key: 8 }, Pi.value.props, {
				ref_key: "tableUnit",
				ref: On,
				onClose: Fi
			}), Ge({
				content: A(() => [qe(k(rn), {
					colNames: bi.value.colNames,
					head: bi.value.head,
					body: bi.value.body,
					config: bi.value.config,
					title: R.value.table.useDialog ? "" : Pi.value.title,
					withCloseButton: !R.value.table.useDialog,
					isCursorPointer: B.value,
					onClose: Fi
				}, {
					th: A(({ th: e }) => [y("div", { innerHTML: e }, null, 8, $t)]),
					td: A(({ td: e }) => [Ke(O(isNaN(Number(e)) ? e : k(p)({
						p: R.value.style.chart.bars.dataLabels.prefix,
						v: e,
						s: R.value.style.chart.bars.dataLabels.suffix,
						r: R.value.table.td.roundingValue
					})), 1)]),
					_: 1
				}, 8, [
					"colNames",
					"head",
					"body",
					"config",
					"title",
					"withCloseButton",
					"isCursorPointer"
				])]),
				_: 2
			}, [R.value.table.useDialog ? {
				name: "title",
				fn: A(() => [Ke(O(Pi.value.title), 1)]),
				key: "0"
			} : void 0, R.value.table.useDialog ? {
				name: "actions",
				fn: A(() => [y("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: n[4] ||= (e) => yi(R.value.userOptions.callbacks.csv),
					style: C({ cursor: B.value ? "pointer" : "default" })
				}, [qe(k(tn), {
					name: "fileCsv",
					stroke: Pi.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : _("", !0),
			D(t.$slots, "skeleton", {}, () => [k(Jn) ? (w(), g(Ce, { key: 0 })) : _("", !0)], !0)
		], 46, ot));
	}
}, [["__scopeId", "data-v-342b4392"]]);
//#endregion
export { at as n, en as t };
