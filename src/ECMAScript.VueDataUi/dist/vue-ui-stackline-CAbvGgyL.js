import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Gt as i, Jt as a, Kt as o, M as s, Ot as c, P as l, Pt as u, S as d, St as f, V as p, Wt as m, X as h, _ as g, b as _, c as ee, ct as te, dt as ne, i as v, jt as re, ot as ie, pt as ae, q as oe, t as se, tt as ce, v as le, w as ue, xt as de, z as fe } from "./lib-Bttd6u5E.js";
import { n as pe, t as me } from "./useHints-Dq_w2E8B.js";
import { n as he, r as ge, t as _e } from "./useTimeLabels-d2f-W1L4.js";
import { t as ve } from "./useConfig-DlNpz6P8.js";
import { t as ye } from "./usePrinter-DN5bYhTG.js";
import { n as be, t as xe } from "./BaseScanner-DZvpgOjM.js";
import { t as Se } from "./useNestedProp-vPNvh7rV.js";
import { t as Ce } from "./useThemeCheck-C43Tcqmk.js";
import { t as we } from "./useChartExport-DNiwdPmb.js";
import { t as Te } from "./useTransitions-g_zBREk2.js";
import { t as Ee } from "./useStableElementSize-C7KADDKj.js";
import { t as De } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as Oe } from "./img-Bnokohej.js";
import { n as ke } from "./Title-BE3qg9xl.js";
import { t as Ae } from "./Shape-C21CMlWS.js";
import { t as je } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Me, t as Ne } from "./useResponsive-ZtArZtUf.js";
import { t as Pe } from "./DefGrad-DVBqDjhO.js";
import { t as Fe } from "./SlicerPreview-wUw1hFwe.js";
import { t as Ie } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Le } from "./A11yDataTable-DdRsVULz.js";
import { t as Re } from "./useUserOptionState-DK-_1ddE.js";
import { t as ze } from "./useChartAccessibility-DYqac8yF.js";
import { t as Be } from "./Legend-CQxUgOd-.js";
import { t as Ve } from "./vue_ui_stackline-DQqKPA9z.js";
import { Fragment as y, Teleport as He, computed as b, createBlock as x, createCommentVNode as S, createElementBlock as C, createElementVNode as w, createSlots as Ue, createTextVNode as We, createVNode as Ge, defineAsyncComponent as Ke, guardReactiveProps as T, mergeProps as qe, nextTick as Je, normalizeClass as E, normalizeProps as D, normalizeStyle as Ye, onBeforeUnmount as Xe, onMounted as Ze, openBlock as O, ref as k, renderList as A, renderSlot as j, resolveDynamicComponent as Qe, shallowRef as $e, toDisplayString as M, toRefs as et, unref as N, useSlots as tt, watch as nt, watchEffect as rt, withCtx as P } from "vue";
//#region src/components/vue-ui-stackline.vue
var it = /* @__PURE__ */ e({ default: () => Vt }), at = ["id"], ot = ["id"], st = ["id"], ct = { style: { position: "relative" } }, lt = [
	"aria-describedby",
	"xmlns",
	"viewBox"
], ut = { key: 0 }, dt = [
	"x",
	"y",
	"width",
	"height"
], ft = { key: 1 }, pt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
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
	"stroke-width"
], gt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], _t = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], vt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], yt = [
	"x",
	"y",
	"width",
	"height",
	"stroke",
	"stroke-width",
	"stroke-linecap",
	"stroke-linejoin",
	"stroke-dasharray"
], bt = [
	"d",
	"fill",
	"opacity"
], xt = [
	"d",
	"stroke",
	"stroke-width"
], St = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], Ct = [
	"transform",
	"font-size",
	"font-weight",
	"fill",
	"text-anchor"
], wt = { key: 0 }, Tt = { key: 1 }, Et = [
	"text-anchor",
	"font-size",
	"font-weight",
	"fill",
	"transform",
	"onClick"
], Dt = [
	"text-anchor",
	"font-size",
	"fill",
	"transform",
	"innerHTML",
	"onClick"
], Ot = [
	"x",
	"y",
	"height",
	"width",
	"fill"
], kt = { key: 0 }, At = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], jt = { key: 0 }, Mt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], Nt = [
	"transform",
	"font-size",
	"font-weight",
	"fill"
], Pt = ["data-start", "data-end"], Ft = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, It = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Lt = ["onClick"], Rt = ["innerHTML"], zt = ["innerHTML"], Bt = ["id"], Vt = /*#__PURE__*/ je({
	__name: "vue-ui-stackline",
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
	setup(e, { expose: je, emit: it }) {
		let Vt = Ke(() => import("./Tooltip-DhjyfHwz.js")), Ht = Ke(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Ut = Ke(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Wt = Ke(() => import("./DataTable-BbKgJ5UI.js")), Gt = Ke(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Kt = Ke(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), qt = Ke(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Jt = Ke(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_stackline: Yt } = ve(), { isThemeValid: Xt, warnInvalidTheme: Zt } = Ce(), Qt = tt(), F = e, $t = it, en = b({
			get() {
				return !!F.dataset && F.dataset.length;
			},
			set(e) {
				return e;
			}
		}), tn = k(null), I = k(oe()), nn = k(!1), L = k([]), rn = k(0), an = k(null), on = k(null), sn = k(null), cn = k(null), ln = k(null), un = k(!1), dn = k(!1), fn = k(0), pn = k(0), mn = k(0), hn = k(!1), gn = k(null), _n = k(null), vn = k(!1), yn = k(null), bn = k(null), xn = k(null), Sn = k(null), Cn = k(null), wn = $e(null), Tn = k(!1), En = k(0), Dn = k(0), R = k(null), On = k({
			x: 0,
			y: 0
		}), kn = k("pointer"), An = Ee({
			elementRef: wn,
			minimumWidth: 2,
			minimumHeight: 2,
			stableFramesRequired: 2,
			once: !1,
			onSizeAccepted: () => {
				Nn();
			}
		});
		function jn() {
			wn.value = tn.value?.parentNode ?? null;
		}
		function Mn() {
			return new Promise((e) => {
				requestAnimationFrame(() => {
					requestAnimationFrame(e);
				});
			});
		}
		async function Nn() {
			let e = ++Dn.value;
			Tn.value = !1, await Je(), await Mn(), await Mn(), e === Dn.value && (En.value += 1, Tn.value = !0);
		}
		let Pn = k(null), Fn = k(!1);
		function In() {
			Fn.value = !Fn.value;
		}
		Ze(() => {
			jn(), An.start(), hn.value = !0, or(), Nn();
		});
		let z = k(qn());
		pe({
			config: () => z.value,
			dataset: () => F.dataset,
			component: "VueUiStackline",
			rules: [
				me.emptyArray,
				{
					test: (e) => e.some((e) => e.series.length > 200),
					message: [
						"👀 Some series have > 200 datapoints, which can impact performance. Consider:",
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
		let { transitionEnabled: Ln } = Te({
			config: () => z.value.transitions,
			dataset: () => F.dataset
		}), B = b(() => z.value.userOptions.useCursorPointer), Rn = b(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				useCssAnimation: !1,
				table: { show: !1 },
				tooltip: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					lines: {
						totalValues: { show: !1 },
						dataLabels: { show: !1 }
					},
					grid: {
						frame: { stroke: "#6A6A6A" },
						scale: {
							scaleMin: 0,
							scaleMax: 144
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
						endIndex: null,
						keepState: !1
					}
				} }
			},
			userConfig: z.value.skeletonConfig ?? {}
		})), { loading: zn, FINAL_DATASET: Bn, manualLoading: Vn } = be({
			...et(F),
			FINAL_CONFIG: z,
			prepareConfig: qn,
			callback: () => {
				Promise.resolve().then(async () => {
					(!z.value.style.chart.zoom.keepState || !Qr.value || q.value.start === 0 && q.value.end === 0) && await ei();
				});
			},
			skeletonDataset: F.config?.skeletonDataset ?? [{
				name: "",
				series: [
					3,
					2,
					1,
					5,
					13,
					21,
					8,
					89,
					34,
					55
				],
				color: "#8A8A8A"
			}, {
				name: "",
				series: [
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
				color: "#CACACA"
			}],
			skeletonConfig: a({
				defaultConfig: z.value,
				userConfig: Rn.value
			})
		}), { userOptionsVisible: Hn, setUserOptionsVisibility: Un, keepUserOptionState: Wn } = Re({ config: z.value }), { svgRef: V } = ze({ config: z.value.style.chart.title });
		function Gn() {
			vn.value = !0, Un(!0);
		}
		function Kn() {
			Un(!1), vn.value = !1, R.value = null, kn.value = "pointer", nn.value = !1, $.value = null, $t("selectX", {
				index: null,
				indexLabel: null,
				dataset: null
			});
		}
		function qn() {
			let e = Se({
				userConfig: F.config,
				defaultConfig: Yt
			}), t = {}, n = e.theme;
			if (n) if (!Xt.value(e)) Zt(e), t = e;
			else {
				let r = Se({
					userConfig: Ve[n] || F.config,
					defaultConfig: e
				});
				t = {
					...Se({
						userConfig: F.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : o[n] || u
				};
			}
			else t = e;
			return t;
		}
		let Jn = b(() => z.value.style.chart.lines.dataLabels.hideUnderValue !== null), Yn = b(() => z.value.style.chart.lines.dataLabels.hideUnderPercentage !== null);
		nt(() => F.config, (e) => {
			zn.value || (z.value = qn()), Hn.value = !z.value.userOptions.showOnChartHover, or({ resetSlicer: !z.value.style.chart.zoom.keepState }), fn.value += 1, pn.value += 1, mn.value += 1, H.value.dataLabels.show = z.value.style.chart.lines.dataLabels.show, H.value.showTable = z.value.table.show, H.value.showTooltip = z.value.style.chart.tooltip.show, U.value.width = z.value.style.chart.width, U.value.height = z.value.style.chart.height, U.value.paddingRatio = {
				top: z.value.style.chart.padding.top / z.value.style.chart.height,
				right: z.value.style.chart.padding.right / z.value.style.chart.width,
				bottom: z.value.style.chart.padding.bottom / z.value.style.chart.height,
				left: z.value.style.chart.padding.left / z.value.style.chart.width
			}, jn(), Nn(), !z.value.style.chart.zoom.keepState || !Qr.value || q.value.start === 0 && q.value.end === 0 ? ei() : Sr();
		}, { deep: !0 }), nt(() => F.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Vn.value = !1), z.value.style.chart.zoom.keepState ? Sr() : Tr(), jn(), Nn();
		}, { deep: !0 });
		let H = k({
			dataLabels: { show: z.value.style.chart.lines.dataLabels.show },
			showTable: z.value.table.show,
			showTooltip: z.value.style.chart.tooltip.show
		});
		nt(z, () => {
			H.value = {
				dataLabels: { show: z.value.style.chart.lines.dataLabels.show },
				showTable: z.value.table.show,
				showTooltip: z.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let { isPrinting: Xn, isImaging: Zn, generatePdf: Qn, generateImage: $n } = ye({
			elementId: `stackline_${I.value}`,
			fileName: z.value.style.chart.title.text || "vue-ui-stackline",
			options: z.value.userOptions.print
		}), er = b(() => z.value.userOptions.show && !z.value.style.chart.title.text), U = k({
			width: z.value.style.chart.width,
			height: z.value.style.chart.height,
			paddingRatio: {
				top: z.value.style.chart.padding.top / z.value.style.chart.height,
				right: z.value.style.chart.padding.right / z.value.style.chart.width,
				bottom: z.value.style.chart.padding.bottom / z.value.style.chart.height,
				left: z.value.style.chart.padding.left / z.value.style.chart.width
			}
		}), tr = b(() => ue(z.value.customPalette)), nr = $e(null), rr = $e(null), ir = k(null), ar = b(() => z.value.debug);
		function or({ resetSlicer: e = !0 } = {}) {
			if (re(F.dataset) ? (ce({
				componentName: "VueUiStackline",
				type: "dataset",
				debug: ar.value
			}), Vn.value = !0) : F.dataset.forEach((e, t) => {
				ae({
					datasetObject: e,
					requiredAttributes: ["name", "series"]
				}).forEach((e) => {
					en.value = !1, ce({
						componentName: "VueUiStackline",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: ar.value
					}), Vn.value = !0;
				});
			}), re(F.dataset) || (Vn.value = z.value.loading), setTimeout(() => {
				dn.value = !0;
			}, 10), z.value.responsive) {
				let e = Me(() => {
					dn.value = !1;
					let { width: e, height: t } = Ne({
						chart: tn.value,
						noTitle: cn.value,
						title: z.value.style.chart.title.text ? an.value : null,
						legend: z.value.style.chart.legend.show ? on.value : null,
						slicer: z.value.style.chart.zoom.show && K.value > 6 ? sn.value.$el : null,
						source: ln.value
					});
					requestAnimationFrame(() => {
						U.value.width = e, U.value.height = t - 12, clearTimeout(ir.value), ir.value = setTimeout(() => {
							dn.value = !0;
						}, 10);
					});
				});
				nr.value && (rr.value && nr.value.unobserve(rr.value), nr.value.disconnect()), nr.value = new ResizeObserver(e), rr.value = tn.value.parentNode, nr.value.observe(rr.value);
			}
			e && ei();
		}
		Xe(() => {
			An.stop(), nr.value && (rr.value && nr.value.unobserve(rr.value), nr.value.disconnect());
		});
		let sr = b(() => z.value.style.chart.grid.y.position === "right");
		function cr() {
			let e = 0;
			xn.value && (e = Array.from(xn.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = bn.value ? bn.value.getBoundingClientRect().width + z.value.style.chart.grid.y.axisName.fontSize + z.value.style.chart.grid.y.axisName.offsetX : 0;
			return {
				left: sr.value ? 0 : e + t,
				right: sr.value ? e + t : 0,
				scaleLabelsWidth: e,
				yAxisLabelWidth: t
			};
		}
		let lr = k(0), ur = k(0), dr = k(0);
		function fr() {
			let e = Sn.value;
			if (!e) {
				ur.value = 0;
				return;
			}
			try {
				let t = e.getBBox();
				ur.value = Number.isFinite(t?.height) ? t.height : 0;
			} catch {
				ur.value = 0;
			}
		}
		function pr() {
			dr.value && cancelAnimationFrame(dr.value), dr.value = requestAnimationFrame(() => {
				requestAnimationFrame(() => {
					fr();
				});
			});
		}
		Xe(() => {
			dr.value && cancelAnimationFrame(dr.value), ur.value = 0, lr.value = 0;
		});
		let mr = b(() => {
			let e = 0;
			if (yn.value) try {
				e = yn.value.getBBox().height;
			} catch {
				e = 0;
			}
			let t = z.value.style.chart.grid.x.timeLabels.show ? ur.value : 0;
			return e + t;
		}), W = b(() => {
			En.value;
			let { height: e, width: t } = U.value, { right: n } = U.value.paddingRatio, r = z.value.style.chart.lines.totalValues.show && F.dataset && F.dataset.length > 1 ? z.value.style.chart.lines.totalValues.fontSize * 1.3 : 0, i = 0, a = 0, o = 0, s = 0;
			if (z.value.style.chart.grid.y.axisLabels.show) {
				let e = cr();
				i = e.left, a = e.right, o = e.scaleLabelsWidth, s = e.yAxisLabelWidth;
			}
			let c = z.value.style.chart.padding.top + r, l = t - t * n - lr.value - a, u = e - z.value.style.chart.padding.bottom - mr.value - r, d = z.value.style.chart.padding.left + i, f = t - d - t * n - lr.value - a, p = e - c - z.value.style.chart.padding.bottom - mr.value - r;
			return {
				chartHeight: Math.max(0, e),
				chartWidth: Math.max(0, t),
				top: c,
				right: Math.max(0, l),
				bottom: Math.max(0, u),
				left: Math.max(0, d),
				width: Math.max(0, f),
				height: Math.max(0, p),
				offsetLeftAxis: i,
				offsetRightAxis: a,
				scaleLabelsWidth: o,
				yAxisLabelWidth: s
			};
		}), hr = b(() => {
			let { left: e, top: t, width: n, height: r } = W.value, i = q.value.start, a = q.value.end, o = Math.max(1, a - i), s = Math.max(0, Math.min(o, (J.value.start ?? i) - i)), c = Math.max(0, Math.min(o, (J.value.end ?? a) - i)), l = Math.max(0, c - s), u = {
				fill: z.value.style.chart.zoom.preview.fill,
				stroke: z.value.style.chart.zoom.preview.stroke,
				"stroke-width": z.value.style.chart.zoom.preview.strokeWidth,
				"stroke-dasharray": z.value.style.chart.zoom.preview.strokeDasharray,
				"stroke-linecap": "round",
				"stroke-linejoin": "round",
				style: {
					pointerEvents: "none",
					transition: "none !important",
					animation: "none !important"
				}
			}, d = n / o;
			return {
				x: e + s * d,
				y: t,
				width: l * d,
				height: r,
				...u
			};
		}), G = b(() => Bn.value.map((e, t) => {
			let n = oe(), r = d(e.color) || tr.value[t] || u[t] || u[t % u.length];
			return {
				...e,
				shape: e.shape || "circle",
				standalone: !!e.standalone,
				series: JSON.parse(JSON.stringify(e.series)).map((e) => z.value.style.chart.lines.distributed ? Math.abs(e) : e),
				seriesSource: e.series,
				signedSeries: e.series.map((e) => e >= 0 ? 1 : -1),
				absoluteIndex: t,
				id: n,
				color: r
			};
		})), gr = b(() => G.value.filter((e) => !L.value.includes(e.id) && !e.standalone)), _r = b(() => G.value.filter((e) => !L.value.includes(e.id) && e.standalone)), K = b(() => {
			let e = Math.max(...G.value.filter((e) => !L.value.includes(e.id)).map((e) => e.series.length));
			return isFinite(e) ? e : Math.max(...G.value.map((e) => e.series.length));
		});
		function vr(e) {
			Pn.value = e;
		}
		let q = k({
			start: 0,
			end: Math.max(...Bn.value.map((e) => e.series.length))
		}), J = k({
			start: 0,
			end: Math.max(...Bn.value.map((e) => e.series.length))
		});
		function yr(e) {
			return (q.value.start ?? 0) + (e ?? 0);
		}
		let br = b(() => z.value.style.chart.zoom.preview.enable && (J.value.start !== q.value.start || J.value.end !== q.value.end));
		function xr(e, t) {
			J.value[e] = t;
		}
		function Sr() {
			let e = K.value, t = Math.max(0, Math.min(q.value.start ?? 0, e - 1)), n = Math.max(t + 1, Math.min(q.value.end ?? e, e));
			(!Number.isFinite(t) || !Number.isFinite(n) || n <= t) && (t = 0, n = e), q.value.start = t, q.value.end = n, J.value.start = t, J.value.end = n, sn.value && (sn.value.setStartValue(t), sn.value.setEndValue(n));
		}
		let Cr = k(null);
		function wr() {
			return new Promise((e) => requestAnimationFrame(() => requestAnimationFrame(() => e())));
		}
		Xe(() => {
			Cr.value && cancelAnimationFrame(Cr.value);
		});
		async function Tr({ force: e = !1 } = {}) {
			if (z.value.style.chart.zoom.keepState && !e && Qr.value && (q.value.start !== 0 || q.value.end !== 0)) {
				Sr();
				return;
			}
			ei(), await Je(), Cr.value && cancelAnimationFrame(Cr.value), Cr.value = requestAnimationFrame(async () => {
				await wr(), ei();
			});
		}
		let Er = b(() => Math.max(0, W.value.width / (q.value.end - q.value.start))), Dr = b(() => m(gr.value.map((e) => ({
			...e,
			series: e.series.map((e) => e ?? 0)
		}))).slice(q.value.start, q.value.end)), Or = b(() => L.value.length === G.value.length), kr = b(() => {
			if (!z.value.style.chart.zoom.minimap.show) return [];
			let e = G.value.filter((e) => Or.value ? !0 : !L.value.includes(e.id) && !e.standalone);
			if (e.length) return m(e.map((e) => ({
				...e,
				series: (e.series || []).map((e) => e ?? 0)
			})));
			let t = _r.value;
			if (!t.length) return [];
			let n = Math.max(...t.map((e) => e.series.length || 0));
			return Array.from({ length: n }, (e, n) => t.reduce((e, t) => e + Math.abs(t.series[n] ?? 0), 0));
		}), Ar = b(() => {
			if (!z.value.style.chart.zoom.minimap.show) return [];
			let e = G.value.filter((e) => Or.value ? !0 : !L.value.includes(e.id) && !e.standalone), t = _r.value, n = e.length ? [{
				name: "",
				series: kr.value,
				color: "#000000",
				isVisible: !0
			}] : [], r = t.map((e) => ({
				name: e.name || "",
				series: (e.series || []).map((e) => e ?? 0),
				color: e.color,
				isVisible: !0
			}));
			return n.concat(r);
		}), jr = b(() => m(gr.value.filter((e) => !L.value.includes(e.id)).map((e) => ({
			...e,
			series: e.series.map((t, n) => {
				let r = t ?? 0;
				return e.signedSeries[n] === -1 && r >= 0 ? -r : r;
			})
		}))).slice(q.value.start, q.value.end)), Mr = b(() => {
			let e = gr.value.filter((e) => !L.value.includes(e.id));
			return {
				positive: m(e.map((e) => ({
					...e,
					series: e.series.slice(q.value.start, q.value.end).map((e) => (e ?? 0) >= 0 ? e ?? 0 : 0)
				}))),
				negative: m(e.map((e) => ({
					...e,
					series: e.series.slice(q.value.start, q.value.end).map((e) => (e ?? 0) < 0 ? e ?? 0 : 0)
				})))
			};
		}), Nr = b(() => {
			let e = Math.max(0, q.value.end - q.value.start), t = Array(e).fill(0), n = Array(e).fill(0);
			return _r.value.forEach((r) => {
				for (let i = 0; i < e; i += 1) {
					let e = r.series[q.value.start + i] ?? 0;
					e > 0 && (t[i] = Math.max(t[i], e)), e < 0 && (n[i] = Math.min(n[i], e));
				}
			}), {
				positive: t,
				negative: n
			};
		}), Pr = b(() => jr.value.map((e, t) => ({
			value: e,
			sign: e >= 0 ? 1 : -1
		})));
		function Fr() {
			return { y0: Lr.value?.[0]?.zero ?? W.value.bottom };
		}
		function Ir(e) {
			let { y0: t } = Fr(), n = z.value.style.chart.lines.totalValues, r = Math.max(2, n.fontSize * 1.3 + n.offsetY), i = (e) => Math.min(Math.max(e, W.value.top - z.value.style.chart.lines.totalValues.fontSize * 1.3), W.value.bottom + z.value.style.chart.lines.totalValues.fontSize * 2);
			if ((Pr.value?.[e]?.value ?? 0) >= 0 || z.value.style.chart.lines.distributed) {
				let n = Infinity;
				for (let t of Y.value || []) {
					let r = t?.series?.[e], i = t?.topY?.[e];
					(r ?? 0) > 0 && Number.isFinite(i) && i < n && (n = i);
				}
				return i((Number.isFinite(n) ? n : t) - r - z.value.style.chart.lines.totalValues.offsetY);
			}
			{
				let a = -Infinity;
				for (let t of Y.value || []) {
					let n = t?.series?.[e], r = t?.topY?.[e];
					(n ?? 0) < 0 && Number.isFinite(r) && r > a && (a = r);
				}
				return i((Number.isFinite(a) ? a : t) + r + n.fontSize * .7 + z.value.style.chart.lines.totalValues.offsetY);
			}
		}
		let Lr = b(() => {
			let e = Math.max(...Mr.value.positive, 0), t = Math.min(...Mr.value.negative, 0), n = Math.max(...Nr.value.positive, 0), r = Math.min(...Nr.value.negative, 0), i = Math.max(e, n), a = Math.min(t, r), o = [
				-Infinity,
				Infinity,
				NaN,
				void 0,
				null
			].includes(a) ? 0 : a, s = z.value.style.chart.grid.scale.scaleMin, c = z.value.style.chart.grid.scale.scaleMax, l = !z.value.style.chart.lines.distributed && (s !== null || c !== null), u = s !== null && !z.value.style.chart.lines.distributed ? s : o > 0 ? 0 : o, d = c !== null && !z.value.style.chart.lines.distributed ? c : i < 0 ? 0 : i, f = l ? le(u, d, z.value.style.chart.grid.scale.ticks) : g(u, d, z.value.style.chart.grid.scale.ticks), p = Math.abs(Number(f.min) || 0), m = (Number(f.max) || 0) + p, h = m === 0 || !Number.isFinite(m) ? 1 : m, _ = Array.isArray(f.ticks) && f.ticks.length ? f.ticks : [0], ee = W.value.bottom - W.value.height * (p / h);
			return _.map((e) => {
				let t = Number(e) || 0, n = W.value.bottom - W.value.height * ((t + p) / h);
				return {
					zero: ee,
					y: n,
					x: sr.value ? X.value.right + 8 : X.value.left - 8,
					value: t
				};
			});
		}), Rr = k([]), zr = k([]), Br = 0;
		rt(() => {
			let e = ++Br;
			(async () => {
				let t = await _e({
					values: z.value.style.chart.grid.x.timeLabels.values,
					maxDatapoints: K.value,
					formatter: z.value.style.chart.grid.x.timeLabels.datetimeFormatter,
					start: q.value.start,
					end: q.value.end
				});
				e === Br && (Rr.value = t);
			})();
		});
		let Vr = 0;
		rt(() => {
			let e = ++Vr;
			(async () => {
				let t = await _e({
					values: z.value.style.chart.grid.x.timeLabels.values,
					maxDatapoints: K.value,
					formatter: z.value.style.chart.grid.x.timeLabels.datetimeFormatter,
					start: 0,
					end: K.value
				});
				e === Vr && (zr.value = t);
			})();
		});
		let Hr = b(() => {
			let e = z.value.style.chart.grid.x.timeLabels.modulo;
			return Rr.value.length ? Math.min(e, [...new Set(Rr.value.map((e) => e.text))].length) : e;
		}), Ur = b(() => {
			let e = z.value.style.chart.grid.x.timeLabels, t = Rr.value || [], n = zr.value || [], r = q.value.start ?? 0, i = $.value, a = K.value, o = t.map((e) => e?.text ?? ""), s = n.map((e) => e?.text ?? "");
			return ee(!!e.showOnlyFirstAndLast, !!e.showOnlyAtModulo, Math.max(1, Hr.value || 1), o, s, r, i, a);
		});
		rt(() => {
			z.value.style.chart.grid.x.timeLabels.show, z.value.style.chart.grid.x.timeLabels.fontSize, z.value.style.chart.grid.x.timeLabels.rotation, z.value.style.chart.grid.x.timeLabels.offsetY, (Ur.value || []).map((e) => e?.text ?? "").join("|"), U.value.width, U.value.height, Sn.value, yn.value, pr();
		}, { flush: "post" });
		let Wr = k({
			months: [],
			shortMonths: [],
			days: [],
			shortDays: []
		}), Gr = 0;
		rt(() => {
			let e = ++Gr, t = z.value.style.chart.grid.x.timeLabels.datetimeFormatter;
			(async () => {
				let n = await ge(t.locale).catch(() => ge("en"));
				e === Gr && (Wr.value = n.data);
			})();
		});
		let Kr = b(() => {
			let e = z.value.style.chart.grid.x.timeLabels.datetimeFormatter, t = he({
				useUTC: e.useUTC,
				locale: Wr.value,
				januaryAsYear: e.januaryAsYear
			});
			return (e, n) => {
				let r = z.value.style.chart.grid.x.timeLabels.values?.[e];
				return r == null ? "" : t.formatDate(new Date(r), n);
			};
		}), qr = b(() => (z.value.style.chart.grid.x.timeLabels.values || []).map((e, t) => ({
			text: Kr.value(t, z.value.style.chart.zoom.timeFormat),
			absoluteIndex: t
		}))), Jr = b(() => (z.value.style.chart.grid.x.timeLabels.values || []).map((e, t) => ({
			text: Kr.value(t, z.value.style.chart.tooltip.timeFormat),
			absoluteIndex: t
		}))), Yr = b(() => {
			if (!en.value && !zn.value) return [];
			let e = W.value.height, t = Lr.value[0] ? Lr.value[0].zero : W.value.bottom, n = q.value.start ?? 0, r = q.value.end ?? 0, i = Math.max(1, r - n), a = Math.max(...Mr.value.positive, 0), o = Math.min(...Mr.value.negative, 0), s = Math.max(...Nr.value.positive, 0), c = Math.min(...Nr.value.negative, 0), l = Math.max(a, s), u = Math.min(o, c), { min: d, max: f } = !z.value.style.chart.lines.distributed && (z.value.style.chart.grid.scale.scaleMax !== null || z.value.style.chart.grid.scale.scaleMin !== null) ? le(z.value.style.chart.grid.scale.scaleMin === null ? u > 0 ? 0 : u : z.value.style.chart.grid.scale.scaleMin, z.value.style.chart.grid.scale.scaleMax === null ? l < 0 ? 0 : l : z.value.style.chart.grid.scale.scaleMax, z.value.style.chart.grid.scale.ticks) : g(z.value.style.chart.grid.scale.scaleMin === null ? u > 0 ? 0 : u : z.value.style.chart.grid.scale.scaleMin, z.value.style.chart.grid.scale.scaleMax === null ? l < 0 ? 0 : l : z.value.style.chart.grid.scale.scaleMax, z.value.style.chart.grid.scale.ticks), p = f + (d >= 0 ? 0 : Math.abs(d)) || 1, m = Array(i).fill(0), h = Array(i).fill(0), _ = (e) => i <= 1 ? W.value.left + W.value.width / 2 : W.value.left + e / (i - 1) * W.value.width;
			return G.value.filter((e) => !L.value.includes(e.id)).map((r) => {
				let a = [], o = [], s = [], c = Array(i).fill(null), l = Array(i).fill(null);
				for (let u = 0; u < i; u += 1) {
					let i = n + u, d = r.series?.[i], f = r.signedSeries?.[i], g = d == null || Number.isNaN(d) ? 0 : d, _ = r.standalone ? g / p : z.value.style.chart.lines.distributed ? g / (Dr.value[u] || 1) : g / p;
					if (g >= 0) {
						let n = e * Math.abs(_);
						if (r.standalone) c[u] = t, l[u] = t - n;
						else {
							let e = m[u], r = e + n;
							c[u] = t - e, l[u] = t - r, m[u] = r;
						}
					} else {
						let n = e * Math.abs(_);
						if (r.standalone) c[u] = t, l[u] = t + n;
						else {
							let e = h[u], r = e + n;
							c[u] = t + e, l[u] = t + r, h[u] = r;
						}
					}
					a.push(u), o.push(g), s.push(f ?? (g >= 0 ? 1 : -1));
				}
				let u = a.map((e) => ({
					x: _(e),
					y: l[e]
				})), d = u.map((e) => e.x), f = o.reduce((e, t) => e + Math.abs(t || 0), 0), g = f === 0 ? 1 : f, ee = o.map((e, t) => {
					if (z.value.style.chart.lines.distributed && !r.standalone) {
						let n = a[t], r = Dr.value[n] || 1;
						return (e || 0) / r;
					}
					return (e || 0) / g;
				});
				return {
					...r,
					x: d,
					points: u,
					baseY: a.map((e) => c[e]),
					topY: a.map((e) => l[e]),
					series: o,
					signedSeries: s,
					proportions: ee,
					rel: a,
					fullSeries: Array.isArray(r.fullSeries) ? r.fullSeries : r.series
				};
			});
		}), Y = b(() => {
			let e = (e) => typeof e == "string" ? e.replace(/^M\s*[-+]?[\d.]+(?:e[-+]?\d+)?\s*,?\s*[-+]?[\d.]+(?:e[-+]?\d+)?\s*/i, "").trim() : "", t = Math.max(...Mr.value.positive, 0), n = Math.min(...Mr.value.negative, 0), r = Math.max(...Nr.value.positive, 0), i = Math.min(...Nr.value.negative, 0), a = Math.max(t, r), o = Math.min(n, i), s = !z.value.style.chart.lines.distributed && (z.value.style.chart.grid.scale.scaleMax !== null || z.value.style.chart.grid.scale.scaleMin !== null) ? le(z.value.style.chart.grid.scale.scaleMin === null ? o > 0 ? 0 : o : z.value.style.chart.grid.scale.scaleMin, z.value.style.chart.grid.scale.scaleMax === null ? a < 0 ? 0 : a : z.value.style.chart.grid.scale.scaleMax, z.value.style.chart.grid.scale.ticks) : g(z.value.style.chart.grid.scale.scaleMin === null ? o > 0 ? 0 : o : z.value.style.chart.grid.scale.scaleMin, z.value.style.chart.grid.scale.scaleMax === null ? a < 0 ? 0 : a : z.value.style.chart.grid.scale.scaleMax, z.value.style.chart.grid.scale.ticks), c = Number(s.min) || 0, u = Number(s.max) || 0, d = Math.abs(c), f = u + d || 1, m = (e) => {
				if (z.value.style.chart.lines.distributed) {
					let t = Math.max(0, Math.min(1, e));
					return minimapH - minimapH * t;
				}
				return clampY(minimapH - minimapH * (((e ?? 0) + d) / f));
			};
			return Yr.value.map((t) => {
				let n = t.x.length, r = Array.isArray(t.fullSeries) ? t.fullSeries : t.series, i = ({ left: e = 0, unitW: t }) => {
					let n = r.length;
					return !Number.isFinite(t) || t <= 0 || n <= 0 ? [] : n === 1 ? [e + t * .5] : r.map((n, r) => e + r * t);
				}, a = ({ minimapH: e }) => !Number.isFinite(e) || e <= 0 ? [] : r.map((t) => m(t || 0, e)), o = ({ minimapH: e }) => {
					if (!Number.isFinite(e) || e <= 0) return [];
					let t = m(0, e);
					return r.map(() => t);
				};
				if (n === 0) return {
					...t,
					points: [],
					smoothPath: "",
					straightPath: "",
					smoothArea: "",
					straightArea: "",
					xMinimap: i,
					yMinimap: a,
					yMinimapBase: o
				};
				let s = t.x.map((e, n) => ({
					x: e,
					y: t.topY[n]
				})), c = t.x.map((e, n) => ({
					x: e,
					y: t.baseY[n]
				})), u = n >= 2 ? l(s) : `M${s[0].x},${s[0].y}`, d = n >= 2 ? p(s) : `M${s[0].x},${s[0].y}`, f = n >= 2 ? `M${e(u)}` : u, h = n >= 2 ? `M${e(d)}` : d, g = "", _ = "";
				if (z.value.style.chart.lines.useArea && n >= 2) {
					let t = l([...c].reverse()), n = p([...c].reverse()), r = c[c.length - 1];
					g = `M${s[0].x},${s[0].y} ${e(u)} L${r.x},${r.y} ${e(t)} Z`, _ = `M${s[0].x},${s[0].y} ${e(d)} L${r.x},${r.y} ${e(n)} Z`;
				}
				return {
					...t,
					points: s,
					smoothPath: f,
					straightPath: h,
					smoothArea: g,
					straightArea: _,
					xMinimap: i,
					yMinimap: a,
					yMinimapBase: o
				};
			});
		}), Xr = b(() => $.value === null || $.value === void 0 ? null : {
			timeLabel: _i($.value),
			absoluteIndex: $.value + q.value.start,
			seriesIndex: $.value,
			datapoint: gi.value,
			series: Y.value,
			config: z.value
		}), Zr = k(!1), Qr = k(!1);
		function $r(e) {
			let t = K.value;
			return e > t ? t : e < 0 || e < q.value.start ? z.value.style.chart.zoom.startIndex === null ? 1 : q.value.start + 1 : e;
		}
		function ei() {
			if (!Zr.value) {
				Zr.value = !0;
				try {
					let { startIndex: e, endIndex: t, keepState: n } = z.value.style.chart.zoom, r = n ? Math.max(0, K.value) : K.value;
					if (n && r <= 0) return;
					let i = e ?? 0, a = t == null ? r : Math.min($r(t + 1), r);
					ii.value = !0, q.value.start = i, q.value.end = a, J.value.start = i, J.value.end = a, Sr(), Qr.value = !0;
				} finally {
					queueMicrotask(() => {
						ii.value = !1;
					}), Zr.value = !1;
				}
			}
		}
		function ti(e) {
			Zr.value || ii.value || e !== q.value.start && (q.value.start = e, J.value.start = e, Sr());
		}
		function ni(e) {
			if (Zr.value || ii.value) return;
			let t = $r(e);
			t !== q.value.end && (q.value.end = t, J.value.end = t, Sr());
		}
		let X = b(() => ({
			left: W.value.left,
			right: W.value.right,
			width: W.value.width
		})), ri = b(() => {
			let e = z.value.style.chart.grid.y.axisLabels, { prefix: t, suffix: n } = z.value.style.chart.lines.dataLabels;
			return Lr.value.map((r) => String(v(e.formatter, r.value, h({
				p: t,
				v: r.value,
				s: n,
				r: e.rounding
			}), { datapoint: r }) ?? "")).join("|");
		});
		nt([
			ri,
			() => z.value.style.chart.grid.y.axisLabels.show,
			() => z.value.style.chart.grid.y.axisLabels.fontSize,
			() => z.value.style.chart.grid.y.axisLabels.bold,
			() => z.value.style.chart.grid.y.position
		], () => {
			z.value.style.chart.grid.y.axisLabels.show && Nn();
		}, { flush: "post" });
		let ii = k(!1), ai = b(() => U.value.width), oi = b(() => U.value.height);
		De({
			timeLabelsEls: Sn,
			timeLabels: Rr,
			slicer: q,
			configRef: z,
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
			width: ai,
			height: oi,
			rotation: z.value.style.chart.grid.x.timeLabels.autoRotate.angle
		});
		let si = k(null);
		function ci(e) {
			let t = V.value;
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
				x: (e.clientX - n.left - s) / i + r.x,
				y: (e.clientY - n.top - c) / i + r.y,
				ok: !0
			};
		}
		let li = 0, Z = b(() => Math.max(1, q.value.end - q.value.start)), ui = b(() => Z.value > 1 ? W.value.width / (Z.value - 1) : 0);
		function Q(e) {
			return Z.value <= 1 ? W.value.left + W.value.width / 2 : W.value.left + e * ui.value;
		}
		function di(e) {
			if (Z.value <= 1) return {
				x: W.value.left,
				width: W.value.width
			};
			let t = e === 0 ? W.value.left : (Q(e - 1) + Q(e)) / 2, n = e === Z.value - 1 ? W.value.left + W.value.width : (Q(e) + Q(e + 1)) / 2;
			return {
				x: t,
				width: Math.max(0, n - t)
			};
		}
		function fi(e) {
			Fn.value || (li && cancelAnimationFrame(li), li = requestAnimationFrame(() => {
				li = 0;
				let t = ci(e);
				if (!t || !V.value) {
					hi();
					return;
				}
				let { left: n, right: r, top: i, bottom: a } = W.value;
				if (t.x < n || t.x > r || t.y < i || t.y > a) {
					hi();
					return;
				}
				let o = 0;
				if (Z.value > 1) {
					let e = (t.x - n) / ui.value;
					o = Math.round(e);
				} else o = 0;
				o < 0 && (o = 0), o > Z.value - 1 && (o = Z.value - 1), si.value !== o && (si.value = o, yi(!0, o));
			}));
		}
		function pi(e) {
			let t = ci(e);
			if (!t || !V.value) return;
			let { left: n, right: r, top: i, bottom: a } = W.value;
			if (t.x < n || t.x > r || t.y < i || t.y > a) return;
			let o = 0;
			if (Z.value > 1) {
				let e = (t.x - n) / ui.value;
				o = Math.round(e);
			} else o = 0;
			o < 0 && (o = 0), o > Z.value - 1 && (o = Z.value - 1), ea({
				seriesIndex: o,
				datapoint: Ki(o)
			}), mi(o);
		}
		function mi(e) {
			let t = JSON.parse(JSON.stringify(Y.value)).map((t) => ({
				name: t.name,
				value: t.series[e] === 0 ? 0 : t.series[e] || null,
				proportion: t.proportions[e] || null,
				color: t.color,
				id: t.id
			}));
			z.value.events.datapointClick && z.value.events.datapointClick({
				datapoint: t,
				seriesIndex: e + q.value.start
			}), $t("selectDatapoint", {
				datapoint: t,
				period: Rr.value[e]
			});
		}
		nt(() => [
			q.value.start,
			q.value.end,
			Y.value.length
		], () => {
			let e = Z.value;
			if ($.value != null) {
				if (e <= 0) {
					$.value = null;
					return;
				}
				$.value < 0 && ($.value = 0), $.value > e - 1 && ($.value = e - 1);
			}
		});
		function hi() {
			li &&= (cancelAnimationFrame(li), 0), si.value = null, yi(!1, null);
		}
		let $ = k(null), gi = b(() => {
			let e = $.value, t = e == null ? null : yr(e);
			return Y.value.map((n) => ({
				slotAbsoluteIndex: n.absoluteIndex,
				shape: n.shape || "circle",
				name: n.name,
				color: n.color,
				value: e == null ? null : n.series.find((t, n) => n === e),
				sourceValue: t == null ? null : n.seriesSource?.[t],
				comments: n.comments || [],
				id: n.id,
				standalone: !!n.standalone
			}));
		});
		function _i(e) {
			if (e == null || !z.value.style.chart.tooltip.showTimeLabel) return null;
			let t = Rr.value?.[e]?.text || null, n = Jr.value?.[e]?.text || null, r = zr.value?.[e]?.text || null;
			return z.value.style.chart.tooltip.useDefaultTimeFormat ? t : n || r;
		}
		let vi = b(() => {
			let e = z.value.style.chart.tooltip.customFormat, t = [...gi.value].reverse(), n = t.filter((e) => !e.standalone), r = t.filter((e) => e.standalone), i = n.map((e) => e.value).filter((e) => f(e) && e !== null).reduce((e, t) => Math.abs(e) + Math.abs(t), 0);
			if (de(e) && te(() => e({
				absoluteIndex: $.value + q.value.start,
				seriesIndex: $.value,
				datapoint: gi.value,
				series: G.value,
				config: z.value
			}))) return e({
				absoluteIndex: $.value + q.value.start,
				seriesIndex: $.value,
				datapoint: gi.value,
				series: G.value,
				config: z.value
			});
			let { showValue: a, showTotal: o, totalTranslation: c, showPercentage: l, borderColor: u, roundingValue: d, roundingPercentage: p } = z.value.style.chart.tooltip, m = (e) => !e.shape || ![
				"star",
				"triangle",
				"square",
				"diamond",
				"pentagon",
				"hexagon"
			].includes(e.shape) ? `<svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="${z.value.style.chart.tooltip.backgroundColor}" stroke-width="1" fill="${e.color}" /></svg>` : e.shape === "star" ? `<svg viewBox="0 0 12 12" height="14" width="14" style="overflow:visible"><polygon stroke="${z.value.style.chart.tooltip.backgroundColor}" stroke-width="1" fill="${e.color}" points="${fe({
				plot: {
					x: 6,
					y: 6
				},
				radius: 5
			})}" /></svg>` : e.shape === "triangle" ? `<svg viewBox="0 0 12 12" height="14" width="14" style="overflow:visible"><path d="${s({
				plot: {
					x: 6,
					y: 6
				},
				radius: 6,
				sides: 3,
				rotation: .52
			}).path}" fill="${e.color}" stroke="${z.value.style.chart.tooltip.backgroundColor}" stroke-width="1" /></svg>` : e.shape === "square" ? `<svg viewBox="0 0 12 12" height="14" width="14"><path d="${s({
				plot: {
					x: 6,
					y: 6
				},
				radius: 6,
				sides: 4,
				rotation: .8
			}).path}" fill="${e.color}" stroke="${z.value.style.chart.tooltip.backgroundColor}" stroke-width="1" /></svg>` : e.shape === "diamond" ? `<svg viewBox="0 0 12 12" height="14" width="14" style="overflow:visible"><path d="${s({
				plot: {
					x: 6,
					y: 6
				},
				radius: 5,
				sides: 4,
				rotation: 0
			}).path}" fill="${e.color}" stroke="${z.value.style.chart.tooltip.backgroundColor}" stroke-width="1" /></svg>` : e.shape === "pentagon" ? `<svg viewBox="0 0 12 12" height="14" width="14" style="overflow:visible"><path d="${s({
				plot: {
					x: 6,
					y: 6
				},
				radius: 5,
				sides: 5,
				rotation: .95
			}).path}" fill="${e.color}" stroke="${z.value.style.chart.tooltip.backgroundColor}" stroke-width="1" /></svg>` : `<svg viewBox="0 0 12 12" height="14" width="14" style="overflow:visible"><path d="${s({
				plot: {
					x: 6,
					y: 6
				},
				radius: 5,
				sides: 6,
				rotation: 0
			}).path}" fill="${e.color}" stroke="${z.value.style.chart.tooltip.backgroundColor}" stroke-width="1" /></svg>`, g = (e, t) => {
				let n = t ? h({
					v: isNaN((e.value ?? 0) / (i || 1)) ? 0 : Math.abs(e.value ?? 0) / (i || 1) * 100,
					s: "%",
					r: p
				}) : "", r = a && t ? "(" : "", o = a && t ? ")" : "";
				return `
        <div style="display:flex;flex-direction:row;align-items:center;gap:4px">
            <div style="width:20px;height:20px;display:flex;align-items:center;justify-content:center;">${m(e)}</div>
            ${e.name}${a || t ? ":" : ""} 
            ${a ? v(z.value.style.chart.lines.dataLabels.formatter, e.sourceValue, h({
					p: z.value.style.chart.lines.dataLabels.prefix,
					v: e.sourceValue,
					s: z.value.style.chart.lines.dataLabels.suffix,
					r: d
				}, { datapoint: e })) : ""} ${r}${n}${o}
        </div>
        `;
			}, _ = "", ee = _i($.value);
			return ee && (_ += `<div style="width:100%;text-align:center;border-bottom:1px solid ${u};padding-bottom:6px;margin-bottom:3px;">${ee}</div>`), o && n.length > 1 && (_ += `<div class="vue-data-ui-tooltip-total" style="display:flex;flex-direction:row;align-items:center;gap:4px">
        <span>${c}:</span>
        <span>${v(z.value.style.chart.lines.dataLabels.formatter, ie(i), h({
				p: z.value.style.chart.lines.dataLabels.prefix,
				v: ie(i),
				s: z.value.style.chart.lines.dataLabels.suffix,
				r: d
			}), { datapoint: {
				name: c,
				value: ie(i)
			} })}</span>
        </div>`), n.forEach((e) => {
				_ += g(e, l);
			}), r.length && (_ += `<div style="border-top:1px solid ${gr.value.length ? u : "transparent"}; margin:${gr.value.length ? "6px 0" : "0"};"></div>`, r.forEach((e) => {
				_ += g(e, !1);
			})), `<div>${_}</div>`;
		});
		function yi(e, t = null, n = "pointer") {
			if (Or.value) return;
			kn.value = n, nn.value = e;
			let r = Y.value.map((e) => ({
				name: e.name,
				value: [
					null,
					void 0,
					NaN
				].includes(e.series[t]) ? null : e.series[t],
				color: e.color
			}));
			e ? (R.value = t, $.value = t, ea({
				seriesIndex: t,
				datapoint: r
			}), z.value.events.datapointEnter && z.value.events.datapointEnter({
				datapoint: r,
				seriesIndex: t + q.value.start
			})) : (R.value = null, $.value = null, $t("selectX", {
				index: null,
				indexLabel: null,
				dataset: null
			}), z.value.events.datapointLeave && z.value.events.datapointLeave({
				datapoint: r,
				seriesIndex: t + q.value.start
			}));
		}
		function bi(e, t) {
			let n = JSON.parse(JSON.stringify(Y.value)).map((e) => ({
				name: e.name,
				value: e.series[t] === 0 ? 0 : (e.signedSeries[t] === -1 && e.series[t] >= 0 ? -e.series[t] : e.series[t]) || null,
				proportion: e.proportions[t] || null,
				color: e.color,
				id: e.id
			}));
			$t("selectTimeLabel", {
				datapoint: n,
				absoluteIndex: e.absoluteIndex,
				label: e.text
			});
		}
		function xi() {
			L.value.length ? L.value = [] : Ci.value.forEach((e) => {
				L.value.push(e.id);
			}), $t("selectLegend", Y.value);
		}
		function Si(e) {
			if (L.value.includes(e.id)) L.value = L.value.filter((t) => t !== e.id);
			else {
				if (L.value.length === G.value.length - 1) return;
				L.value.push(e.id);
			}
			$t("selectLegend", Y.value);
		}
		let Ci = b(() => G.value.map((e) => ({
			...e,
			opacity: L.value.includes(e.id) ? .5 : 1,
			segregate: () => Si(e),
			isSegregated: L.value.includes(e.id)
		}))), wi = b(() => ({
			cy: "stackline-legend",
			backgroundColor: z.value.style.chart.legend.backgroundColor,
			color: z.value.style.chart.legend.color,
			fontSize: z.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: z.value.style.chart.legend.bold ? "bold" : ""
		}));
		function Ti(e) {
			return G.value.length ? G.value.find((t) => t.name === e) || (ar.value && console.warn(`VueUiStackline - Series name not found "${e}"`), null) : (ar.value && console.warn("VueUiStackline - There are no series to show."), null);
		}
		function Ei(e) {
			let t = Ti(e);
			t !== null && L.value.includes(t.id) && Si({ id: t.id });
		}
		function Di(e) {
			let t = Ti(e);
			t !== null && (L.value.includes(t.id) || Si({ id: t.id }));
		}
		let Oi = b(() => {
			if (Y.value.length === 0) return {
				head: [],
				body: [],
				config: {},
				columnNames: []
			};
			let e = Y.value.map(({ name: e, color: t }) => ({
				label: e,
				color: t
			})), t = [];
			return Ur.value.forEach((e) => {
				let n = e.absoluteIndex, r = [z.value.style.chart.grid.x.timeLabels.values?.[n] ? e.text : n + 1];
				G.value.forEach((e) => {
					let t = e.series?.[n], i = Number((t ?? 0).toFixed(z.value.table.td.roundingValue));
					r.push(i);
				}), t.push(r);
			}), {
				head: e,
				body: t
			};
		});
		function ki(e = null) {
			let n = [
				[z.value.style.chart.title.text],
				[z.value.style.chart.title.subtitle.text],
				[""]
			], i = ["", ...Oi.value.head.map((e) => e.label)], a = Oi.value.body, o = n.concat([i]).concat(a), s = r(o);
			e ? e(s) : t({
				csvContent: s,
				title: z.value.style.chart.title.text || "vue-ui-stackline"
			});
		}
		let Ai = b(() => {
			let e = [""].concat(Y.value.map((e) => e.name), " <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>"), t = [], n = Math.max(0, q.value.end - q.value.start);
			for (let e = 0; e < n; e += 1) {
				let n = yr(e), r = z.value.style.chart.grid.x.timeLabels.values?.[n] ? Rr.value?.[e]?.text ?? n + 1 : n + 1, i = Y.value.map((t) => {
					let n = t.series?.[e] ?? 0;
					return Number(n.toFixed(z.value.table.td.roundingValue));
				}), a = Y.value.filter((e) => !e.standalone).map((t) => t.series?.[e] ?? 0).reduce((e, t) => e + t, 0);
				t.push([r].concat(i, Number(a.toFixed(z.value.table.td.roundingValue))));
			}
			return {
				head: e,
				body: t,
				config: {
					th: {
						backgroundColor: z.value.table.th.backgroundColor,
						color: z.value.table.th.color,
						outline: z.value.table.th.outline
					},
					td: {
						backgroundColor: z.value.table.td.backgroundColor,
						color: z.value.table.td.color,
						outline: z.value.table.td.outline
					},
					breakpoint: z.value.table.responsiveBreakpoint
				},
				colNames: [z.value.table.columnNames.period].concat(Y.value.map((e) => e.name), z.value.table.columnNames.total)
			};
		}), ji = b(() => z.value.style.chart.backgroundColor), Mi = b(() => z.value.style.chart.legend), Ni = b(() => z.value.style.chart.title), { isCallbackImaging: Pi, isCallbackSvg: Fi, generateSvg: Ii, onGenerateImage: Li } = we({
			svg: V,
			title: Ni,
			legend: Mi,
			legendItems: Ci,
			backgroundColor: ji,
			getSvgCallback: () => z.value.userOptions.callbacks.svg,
			generateImage: $n
		});
		async function Ri({ scale: e = 2 } = {}) {
			if (!tn.value) return;
			let { imageUri: t, base64: n } = await Oe({
				domElement: tn.value,
				base64: !0,
				img: !0,
				scale: e
			}), r = tn.value.getBoundingClientRect(), i = {
				width: r.width,
				height: r.height,
				aspectRatio: r.height ? r.width / r.height : 0
			}, a = await ne(t, e) ?? i;
			return {
				imageUri: t,
				base64: n,
				title: z.value.style.chart.title.text,
				...a
			};
		}
		let zi = b(() => {
			let e = z.value.table.useDialog && !z.value.table.show, t = H.value.showTable;
			return {
				component: e ? Jt : Ut,
				title: `${z.value.style.chart.title.text}${z.value.style.chart.title.subtitle.text ? `: ${z.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: z.value.table.th.backgroundColor,
					color: z.value.table.th.color,
					headerColor: z.value.table.th.color,
					headerBg: z.value.table.th.backgroundColor,
					isFullscreen: un.value,
					fullscreenParent: tn.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: B.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: z.value.style.chart.backgroundColor,
							color: z.value.style.chart.color
						},
						head: {
							backgroundColor: z.value.style.chart.backgroundColor,
							color: z.value.style.chart.color
						}
					}
				}
			};
		});
		nt(() => H.value.showTable, (e) => {
			z.value.table.show || (e && z.value.table.useDialog && gn.value ? gn.value.open() : "close" in gn.value && gn.value.close());
		});
		function Bi() {
			H.value.showTable = !1, _n.value && _n.value.setTableIconState(!1);
		}
		function Vi(e) {
			un.value = e, rn.value += 1;
		}
		function Hi() {
			H.value.showTable = !H.value.showTable;
		}
		function Ui() {
			H.value.dataLabels.show = !H.value.dataLabels.show;
		}
		function Wi() {
			H.value.showTooltip = !H.value.showTooltip;
		}
		function Gi() {
			return Y.value;
		}
		function Ki(e) {
			let t = e ?? 0, n = yr(t);
			return JSON.parse(JSON.stringify(Y.value)).map((e) => {
				let r = e.series[t], i = e.signedSeries?.[t], a = r === 0 ? 0 : (i === -1 && r >= 0 ? -r : r) || null;
				return {
					name: e.name,
					absoluteIndex: e.absoluteIndex,
					value: a,
					proportion: e.proportions?.[t] || null,
					color: e.color,
					id: e.id,
					timeLabel: zr.value?.[n] || null
				};
			});
		}
		let qi = b(() => Array(K.value).fill(0).map((e, t) => Ki(t)));
		nt(() => F.selectedXIndex, (e) => {
			if ([null, void 0].includes(F.selectedXIndex)) {
				$.value = null;
				return;
			}
			let t = e - q.value.start;
			t < 0 || e >= q.value.end ? $.value = null : $.value = t ?? null;
		}, { immediate: !0 });
		let Ji = b(() => {
			if (z.value.style.chart.lines.distributed) return {
				min: -1,
				max: 1
			};
			let e = gr.value, t = _r.value, n = Math.max(1, ...[...e, ...t].map((e) => e.series?.length || 0)), r = Array(n).fill(0), i = Array(n).fill(0);
			e.forEach((e) => {
				for (let t = 0; t < n; t += 1) {
					let n = e.series[t] ?? 0;
					n >= 0 ? r[t] += n : i[t] += n;
				}
			});
			let a = Array(n).fill(0), o = Array(n).fill(0);
			t.forEach((e) => {
				for (let t = 0; t < n; t += 1) {
					let n = e.series[t] ?? 0;
					n > 0 && (a[t] = Math.max(a[t], n)), n < 0 && (o[t] = Math.min(o[t], n));
				}
			});
			let s = Math.max(0, ...r, ...a), c = Math.min(0, ...i, ...o), l = z.value.style.chart.grid.scale.scaleMin, u = z.value.style.chart.grid.scale.scaleMax, d = z.value.style.chart.grid.scale.ticks, f = !z.value.style.chart.lines.distributed && (l !== null || u !== null) ? le(l === null ? c > 0 ? 0 : c : l, u === null ? s < 0 ? 0 : s : u, d) : g(l === null ? c > 0 ? 0 : c : l, u === null ? s < 0 ? 0 : s : u, d);
			return {
				min: Number(f.min) || 0,
				max: Number(f.max) || 0
			};
		});
		function Yi({ minimapH: e, unitW: t }) {
			let n = (e) => typeof e == "string" ? e.replace(/^M\s*[-+]?[\d.]+(?:e[-+]?\d+)?\s*,?\s*[-+]?[\d.]+(?:e[-+]?\d+)?\s*/i, "").trim() : "";
			if (!Number.isFinite(e) || e <= 0 || !Number.isFinite(t) || t <= 0) return "";
			let r = Y.value.filter((e) => !e.standalone), i = Y.value.filter((e) => e.standalone), a = Math.max(0, ...Y.value.map((e) => Array.isArray(e.fullSeries) ? e.fullSeries.length : e.series.length));
			if (a <= 0) return "";
			let o = a === 1 ? [t * .5] : Array.from({ length: a }, (e, n) => n * t);
			if (z.value.style.chart.lines.distributed) {
				let t = Array.from({ length: a }, (e, t) => {
					let n = 0;
					return r.forEach((e) => {
						let r = (Array.isArray(e.fullSeries) ? e.fullSeries : e.series)?.[t];
						r != null && !Number.isNaN(r) && (n += Math.abs(r));
					}), n || 1;
				}), s = (t) => e - e * Math.max(0, Math.min(1, t || 0)), c = [], u = Array(a).fill(0);
				return r.forEach((e) => {
					let r = Array.isArray(e.fullSeries) ? e.fullSeries : e.series, i = Array(a), d = Array(a);
					for (let e = 0; e < a; e += 1) {
						let n = r?.[e], a = n == null || Number.isNaN(n) ? 0 : Math.abs(n) / t[e];
						d[e] = u[e], u[e] += a, i[e] = u[e];
					}
					let f = i.map(s), m = d.map(s), h = o.map((e, t) => ({
						x: e,
						y: f[t]
					})), g = o.map((e, t) => ({
						x: e,
						y: m[t]
					})), _ = l(h), ee = l([...g].reverse()), te = p(h), ne = p([...g].reverse()), v = g[g.length - 1];
					if (z.value.style.chart.lines.useArea) {
						let t = z.value.style.chart.lines.smooth ? `M${h[0].x},${h[0].y} ${n(_)} L${v.x},${v.y} ${n(ee)} Z` : `M${h[0].x},${h[0].y} ${n(te)} L${v.x},${v.y} ${n(ne)} Z`;
						c.push(`<path d="${t}"
                    fill="${Qt.pattern ? `url(#pattern_${I.value}_${e.absoluteIndex})` : (z.value.style.chart.lines.gradient.show, e.color)}"
                    opacity="${z.value.style.chart.lines.areaOpacity}"
                    stroke="none" />`);
					}
					let re = z.value.style.chart.lines.smooth ? `M${h[0].x},${h[0].y} ${n(_)}` : `M${h[0].x},${h[0].y} ${n(te)}`;
					c.push(`<path d="${re}"
                fill="none"
                stroke="${e.color}"
                stroke-width="${z.value.style.chart.lines.strokeWidth}"
                stroke-linecap="round" />`);
				}), i.length && i.forEach((e) => {
					let r = Array.isArray(e.fullSeries) ? e.fullSeries : e.series, i = Array.from({ length: a }, (e, n) => s(Math.abs(r?.[n] ?? 0) / t[n])), u = o.map((e, t) => ({
						x: e,
						y: i[t]
					})), d = l(u), f = p(u), m = z.value.style.chart.lines.smooth ? `M${u[0].x},${u[0].y} ${n(d)}` : `M${u[0].x},${u[0].y} ${n(f)}`;
					c.push(`<path d="${m}"
                    fill="none"
                    stroke="${e.color}"
                    stroke-width="${z.value.style.chart.lines.strokeWidth}"
                    stroke-linecap="round" />`);
				}), c.join("");
			}
			let s = Ji.value.min, c = Ji.value.max, u = Math.abs(s), d = c + u || 1, f = (t) => Math.max(0, Math.min(e, t)), m = (t) => f(e - e * (((t ?? 0) + u) / d)), h = [];
			if (r.length) {
				let e = Array(a).fill(0), t = Array(a).fill(0);
				r.forEach((r) => {
					let i = Array.isArray(r.fullSeries) ? r.fullSeries : r.series, s = Array(a), c = Array(a);
					for (let n = 0; n < a; n += 1) {
						let r = i?.[n] ?? 0;
						r >= 0 ? (c[n] = e[n], e[n] += r, s[n] = e[n]) : (c[n] = t[n], t[n] += r, s[n] = t[n]);
					}
					let u = s.map(m), d = c.map(m), f = o.map((e, t) => ({
						x: e,
						y: u[t]
					})), g = o.map((e, t) => ({
						x: e,
						y: d[t]
					})), _ = l(f), ee = l([...g].reverse()), te = p(f), ne = p([...g].reverse()), v = g[g.length - 1];
					if (z.value.style.chart.lines.useArea) {
						let e = z.value.style.chart.lines.smooth ? `M${f[0].x},${f[0].y} ${n(_)} L${v.x},${v.y} ${n(ee)} Z` : `M${f[0].x},${f[0].y} ${n(te)} L${v.x},${v.y} ${n(ne)} Z`;
						h.push(`<path d="${e}"
                    fill="${Qt.pattern ? `url(#pattern_${I.value}_${r.absoluteIndex})` : (z.value.style.chart.lines.gradient.show, r.color)}"
                    opacity="${z.value.style.chart.lines.areaOpacity}"
                    stroke="none" />`);
					}
					let re = z.value.style.chart.lines.smooth ? `M${f[0].x},${f[0].y} ${n(_)}` : `M${f[0].x},${f[0].y} ${n(te)}`;
					h.push(`<path d="${re}"
                fill="none"
                stroke="${r.color}"
                stroke-width="${z.value.style.chart.lines.strokeWidth}"
                stroke-linecap="round" />`);
				});
			}
			return i.length && i.forEach((e) => {
				let t = (Array.isArray(e.fullSeries) ? e.fullSeries : e.series).map(m), r = o.map((e, n) => ({
					x: e,
					y: t[n]
				})), i = l(r), a = p(r), s = z.value.style.chart.lines.smooth ? `M${r[0].x},${r[0].y} ${n(i)}` : `M${r[0].x},${r[0].y} ${n(a)}`;
				h.push(`<path d="${s}"
                fill="none"
                stroke="${e.color}"
                stroke-width="${z.value.style.chart.lines.strokeWidth}"
                stroke-linecap="round" />`);
			}), h.join("");
		}
		function Xi(e, t) {
			let n = z.value.style.chart.lines, r = n.dataLabels;
			return n.showDistributedPercentage && n.distributed ? Yn.value ? t === 0 ? !r.hideEmptyPercentages : Math.abs(t) * 100 >= r.hideUnderPercentage : !r.hideEmptyPercentages || Math.abs(t) > 0 : Yn.value ? (Jn.value && ar.value && console.warn("Vue Data UI - VueUiStackline - You cannot set both dataLabels.hideUnderPercentage and dataLabels.hideUnderValue. Note that dataLabels.hideUnderPercentage takes precedence in this case."), Math.abs(e) > Zi.value * r.hideUnderPercentage / 100) : Jn.value ? Math.abs(e) >= r.hideUnderValue : !r.hideEmptyValues || e !== 0;
		}
		let Zi = b(() => Math.max(...Y.value.flatMap((e) => e.series)));
		function Qi(e, t, n, r, i) {
			let a = i === -1 && e >= 0 ? -e : e;
			return v(z.value.style.chart.lines.dataLabels.formatter, a, h({
				p: z.value.style.chart.lines.dataLabels.prefix,
				v: a,
				s: z.value.style.chart.lines.dataLabels.suffix,
				r: z.value.style.chart.lines.dataLabels.rounding
			}), {
				datapoint: t,
				seriesIndex: n,
				datapointIndex: r
			});
		}
		function $i(e, t, n, r) {
			return v(z.value.style.chart.lines.dataLabels.formatter, e, h({
				v: isNaN(e) ? 0 : e,
				s: "%",
				r: z.value.style.chart.lines.dataLabels.rounding
			}), {
				datapoint: t,
				seriesIndex: n,
				datapointIndex: r
			});
		}
		function ea({ seriesIndex: e, datapoint: t }) {
			let n = q.value.start + e;
			$t("selectX", {
				dataset: t,
				index: n,
				indexLabel: z.value.style.chart.grid.x.timeLabels.values[n]
			});
		}
		async function ta() {
			if ($t("copyAlt", {
				config: z.value,
				dataset: Y.value
			}), !z.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(z.value.userOptions.callbacks.altCopy({
				config: z.value,
				dataset: Y.value
			}));
		}
		let na = k(!1);
		function ra() {
			R.value = null, na.value = !0;
		}
		function ia() {
			R.value = null, kn.value = "pointer", nn.value = !1, $.value = null, $t("selectX", {
				index: null,
				indexLabel: null,
				dataset: null
			}), na.value = !1;
		}
		function aa(e) {
			if (!V.value || Fn.value || document.activeElement !== V.value || Or.value || !Z.value) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				R.value = null, kn.value = "pointer", nn.value = !1, $.value = null, $t("selectX", {
					index: null,
					indexLabel: null,
					dataset: null
				});
				return;
			}
			if (r) {
				if (R.value === null) return;
				mi(R.value);
				return;
			}
			let a = R.value, o = si.value, s = a !== null && a >= 0 && a < Z.value, c = o !== null && o >= 0 && o < Z.value;
			s ? n ? (a += 1, a >= Z.value && (a = 0)) : t && (--a, a < 0 && (a = Z.value - 1)) : c ? (a = n ? o + 1 : o - 1, a >= Z.value && (a = 0), a < 0 && (a = Z.value - 1)) : a = n ? 0 : Z.value - 1, R.value = a, oa(a), yi(!0, a, "keyboard");
		}
		function oa(e) {
			if (!Number.isFinite(e) || !V.value) return;
			let t = W.value.left + e * ui.value, n = W.value.top + W.value.height / 2, r = i(t, n, V.value);
			r && (On.value = r);
		}
		let sa = b(() => ({
			headers: Ai.value?.colNames ?? [],
			rows: Ai.value?.body ?? []
		}));
		return je({
			getData: Gi,
			getImage: Ri,
			generatePdf: Qn,
			generateCsv: ki,
			generateImage: $n,
			generateSvg: Ii,
			hideSeries: Di,
			showSeries: Ei,
			toggleTable: Hi,
			toggleLabels: Ui,
			toggleTooltip: Wi,
			toggleAnnotator: In,
			toggleFullscreen: Vi,
			copyAlt: ta
		}), (t, r) => (O(), C("div", {
			id: `stackline_${I.value}`,
			ref_key: "stacklineChart",
			ref: tn,
			class: E({
				"vue-data-ui-component": !0,
				"vue-ui-stackline": !0,
				"vue-data-ui-wrapper-fullscreen": un.value
			}),
			style: Ye(`background:${z.value.style.chart.backgroundColor};color:${z.value.style.chart.color};font-family:${z.value.style.fontFamily}; position: relative; ${z.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: Gn,
			onMouseleave: Kn
		}, [
			w("div", {
				id: `chart-instructions-${I.value}`,
				class: "sr-only"
			}, [w("p", null, M(z.value.a11y.translations.keyboardNavigation), 1)], 8, ot),
			sa.value?.rows?.length ? (O(), x(Le, {
				key: 0,
				uid: I.value,
				head: sa.value.headers,
				body: sa.value.rows,
				notice: z.value.a11y.translations.tableAvailable,
				caption: z.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : S("", !0),
			z.value.userOptions.buttons.annotator ? (O(), x(N(Gt), {
				key: 1,
				svgRef: N(V),
				backgroundColor: z.value.style.chart.backgroundColor,
				color: z.value.style.chart.color,
				active: Fn.value,
				isCursorPointer: B.value,
				onClose: In
			}, {
				"annotator-action-close": P(() => [j(t.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": P(({ color: e }) => [j(t.$slots, "annotator-action-color", D(T({ color: e })), void 0, !0)]),
				"annotator-action-draw": P(({ mode: e }) => [j(t.$slots, "annotator-action-draw", D(T({ mode: e })), void 0, !0)]),
				"annotator-action-undo": P(({ disabled: e }) => [j(t.$slots, "annotator-action-undo", D(T({ disabled: e })), void 0, !0)]),
				"annotator-action-redo": P(({ disabled: e }) => [j(t.$slots, "annotator-action-redo", D(T({ disabled: e })), void 0, !0)]),
				"annotator-action-delete": P(({ disabled: e }) => [j(t.$slots, "annotator-action-delete", D(T({ disabled: e })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : S("", !0),
			j(t.$slots, "userConfig", {}, void 0, !0),
			er.value ? (O(), C("div", {
				key: 2,
				ref_key: "noTitle",
				ref: cn,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : S("", !0),
			z.value.style.chart.title.text ? (O(), C("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: an,
				style: "width:100%;background:transparent;"
			}, [(O(), x(ke, {
				key: `title_${fn.value}`,
				config: {
					title: {
						cy: "stackline-title",
						...z.value.style.chart.title
					},
					subtitle: {
						cy: "stackline-subtitle",
						...z.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : S("", !0),
			w("div", { id: `legend-top-${I.value}` }, null, 8, st),
			z.value.userOptions.show && en.value && (N(Wn) || N(Hn)) ? (O(), x(N(Kt), {
				ref_key: "userOptionsRef",
				ref: _n,
				key: `user_option_${rn.value}`,
				backgroundColor: z.value.style.chart.backgroundColor,
				color: z.value.style.chart.color,
				isPrinting: N(Xn),
				isImaging: N(Zn),
				uid: I.value,
				hasTooltip: z.value.style.chart.tooltip.show && z.value.userOptions.buttons.tooltip,
				hasPdf: z.value.userOptions.buttons.pdf,
				hasImg: z.value.userOptions.buttons.img,
				hasSvg: z.value.userOptions.buttons.svg,
				hasXls: z.value.userOptions.buttons.csv,
				hasTable: z.value.userOptions.buttons.table,
				hasLabel: z.value.userOptions.buttons.labels,
				hasFullscreen: z.value.userOptions.buttons.fullscreen,
				hasAltCopy: z.value.userOptions.buttons.altCopy,
				isFullscreen: un.value,
				chartElement: tn.value,
				position: z.value.userOptions.position,
				isTooltip: H.value.showTooltip,
				titles: { ...z.value.userOptions.buttonTitles },
				hasAnnotator: z.value.userOptions.buttons.annotator,
				isAnnotation: Fn.value,
				callbacks: z.value.userOptions.callbacks,
				printScale: z.value.userOptions.print.scale,
				tableDialog: z.value.table.useDialog,
				isCursorPointer: B.value,
				onToggleFullscreen: Vi,
				onGeneratePdf: N(Qn),
				onGenerateCsv: ki,
				onGenerateImage: N(Li),
				onGenerateSvg: N(Ii),
				onToggleTable: Hi,
				onToggleLabels: Ui,
				onToggleTooltip: Wi,
				onToggleAnnotator: In,
				onCopyAlt: ta,
				style: Ye({ visibility: N(Wn) ? N(Hn) ? "visible" : "hidden" : "visible" })
			}, Ue({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: P(({ isOpen: e, color: n }) => [j(t.$slots, "menuIcon", D(T({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: P(() => [j(t.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: P(() => [j(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: P(() => [j(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: P(() => [j(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionSvg ? {
					name: "optionSvg",
					fn: P(() => [j(t.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionTable ? {
					name: "optionTable",
					fn: P(() => [j(t.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionLabels ? {
					name: "optionLabels",
					fn: P(() => [j(t.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: P(({ toggleFullscreen: e, isFullscreen: n }) => [j(t.$slots, "optionFullscreen", D(T({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: P(({ toggleAnnotator: e, isAnnotator: n }) => [j(t.$slots, "optionAnnotator", D(T({
						toggleAnnotator: e,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: P(({ altCopy: e }) => [j(t.$slots, "optionAltCopy", D(T({ altCopy: e })), void 0, !0)]),
					key: "10"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: P(() => [j(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: P(() => [j(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasLabel.hasFullscreen.hasAltCopy.isFullscreen.chartElement.position.isTooltip.titles.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : S("", !0),
			w("div", ct, [(O(), C("svg", {
				ref_key: "svgRef",
				ref: V,
				"aria-describedby": `chart-instructions-${I.value}`,
				xmlns: N(se),
				viewBox: `0 0 ${W.value.chartWidth <= 0 ? 10 : W.value.chartWidth} ${W.value.chartHeight <= 0 ? 10 : W.value.chartHeight}`,
				class: E({
					"vue-data-ui-loading": N(zn),
					"vue-data-ui-fullscreen--on": un.value,
					"vue-data-ui-fulscreen--off": !un.value,
					"vue-data-ui-no-transition": !N(Ln)
				}),
				style: Ye(`max-width:100%;overflow:visible;background:transparent;color:${z.value.style.chart.color}`),
				role: "img",
				"aria-live": "polite",
				tabindex: "0",
				preserveAspectRatio: "xMidYMid",
				onMousemove: fi,
				onMouseleave: hi,
				onClick: pi,
				onFocus: ra,
				onBlur: ia,
				onKeydown: aa
			}, [
				Ge(N(qt)),
				(O(!0), C(y, null, A(gr.value, (e) => (O(), C(y, null, [t.$slots.pattern ? (O(), C("defs", ut, [j(t.$slots, "pattern", qe({ ref_for: !0 }, {
					seriesIndex: e.absoluteIndex,
					patternId: `pattern_${I.value}_${e.absoluteIndex}`
				}), void 0, !0)])) : S("", !0)], 64))), 256)),
				t.$slots["chart-background"] ? (O(), C("foreignObject", {
					key: 0,
					x: X.value.left,
					y: W.value.top,
					width: X.value.width,
					height: W.value.height,
					style: { pointerEvents: "none" }
				}, [j(t.$slots, "chart-background", {}, void 0, !0)], 8, dt)) : S("", !0),
				z.value.style.chart.lines.gradient.show ? (O(), C("defs", ft, [(O(!0), C(y, null, A(Y.value, (e, t) => (O(), x(Pe, {
					t: "linear",
					id: `gradient_${e.id}`,
					key: `gradient_${e.id}_${t}`,
					x1: "0%",
					y1: "0%",
					x2: "0%",
					y2: "100%",
					stops: [[
						"0%",
						e.color,
						1
					], [
						"100%",
						N(c)(e.color, z.value.style.chart.lines.gradient.intensity / 100),
						1
					]]
				}, null, 8, ["id", "stops"]))), 128))])) : S("", !0),
				z.value.style.chart.grid.x.showHorizontalLines ? (O(!0), C(y, { key: 2 }, A(Lr.value, (e, t) => (O(), C("line", {
					x1: X.value.left,
					x2: X.value.right,
					y1: e.y,
					y2: e.y,
					stroke: z.value.style.chart.grid.x.linesColor,
					"stroke-width": z.value.style.chart.grid.x.linesThickness,
					"stroke-dasharray": z.value.style.chart.grid.x.linesStrokeDasharray,
					"stroke-linecap": "round"
				}, null, 8, pt))), 256)) : S("", !0),
				z.value.style.chart.grid.y.showVerticalLines ? (O(!0), C(y, { key: 3 }, A(q.value.end - q.value.start, (e, t) => (O(), C("line", {
					x1: Q(t),
					x2: Q(t),
					y1: W.value.top,
					y2: W.value.bottom,
					stroke: z.value.style.chart.grid.y.linesColor,
					"stroke-width": z.value.style.chart.grid.y.linesThickness,
					"stroke-dasharray": z.value.style.chart.grid.y.linesStrokeDasharray,
					"stroke-linecap": "round"
				}, null, 8, mt))), 256)) : S("", !0),
				z.value.style.chart.grid.x.showAxis ? (O(), C("line", {
					key: 4,
					x1: X.value.left,
					x2: X.value.right,
					y1: W.value.bottom,
					y2: W.value.bottom,
					stroke: z.value.style.chart.grid.x.axisColor,
					"stroke-width": z.value.style.chart.grid.x.axisThickness,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 8, ht)) : S("", !0),
				z.value.style.chart.grid.y.showAxis && !z.value.style.chart.lines.distributed ? (O(), C("line", {
					key: 5,
					x1: sr.value ? X.value.right : X.value.left,
					x2: sr.value ? X.value.right : X.value.left,
					y1: W.value.top,
					y2: W.value.bottom,
					stroke: z.value.style.chart.grid.y.axisColor,
					"stroke-width": z.value.style.chart.grid.y.axisThickness,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 8, gt)) : S("", !0),
				z.value.style.chart.grid.x.axisName.show && z.value.style.chart.grid.x.axisName.text ? (O(), C("text", {
					key: 6,
					ref_key: "xAxisLabel",
					ref: yn,
					x: W.value.left + W.value.width / 2,
					y: W.value.chartHeight - 3,
					"font-size": z.value.style.chart.grid.x.axisName.fontSize,
					fill: z.value.style.chart.grid.x.axisName.color,
					"font-weight": z.value.style.chart.grid.x.axisName.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, M(z.value.style.chart.grid.x.axisName.text), 9, _t)) : S("", !0),
				z.value.style.chart.grid.y.axisName.show && z.value.style.chart.grid.y.axisName.text ? (O(), C("text", {
					key: 7,
					ref_key: "yAxisLabel",
					ref: bn,
					transform: `translate(${sr.value ? W.value.chartWidth - z.value.style.chart.grid.y.axisName.fontSize / 2 - z.value.style.chart.grid.y.axisName.offsetX : z.value.style.chart.grid.y.axisName.fontSize + z.value.style.chart.grid.y.axisName.offsetX}, ${W.value.top + W.value.height / 2}) rotate(-90)`,
					"font-size": z.value.style.chart.grid.y.axisName.fontSize,
					fill: z.value.style.chart.grid.y.axisName.color,
					"font-weight": z.value.style.chart.grid.y.axisName.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, M(z.value.style.chart.grid.y.axisName.text), 9, vt)) : S("", !0),
				z.value.style.chart.grid.frame.show ? (O(), C("rect", {
					key: 8,
					style: {
						pointerEvents: "none",
						transition: "none",
						animation: "none !important"
					},
					x: X.value.left,
					y: W.value.top,
					width: X.value.width,
					height: W.value.height,
					fill: "transparent",
					stroke: z.value.style.chart.grid.frame.stroke,
					"stroke-width": z.value.style.chart.grid.frame.strokeWidth,
					"stroke-linecap": z.value.style.chart.grid.frame.strokeLinecap,
					"stroke-linejoin": z.value.style.chart.grid.frame.strokeLinejoin,
					"stroke-dasharray": z.value.style.chart.grid.frame.strokeDasharray
				}, null, 8, yt)) : S("", !0),
				(O(!0), C(y, null, A(Y.value, (e) => (O(), C(y, null, [z.value.style.chart.lines.useArea && !e.standalone ? (O(), C("path", {
					key: 0,
					d: z.value.style.chart.lines.smooth ? e.smoothArea : e.straightArea,
					fill: t.$slots.pattern ? `url(#pattern_${I.value}_${e.absoluteIndex})` : z.value.style.chart.lines.gradient.show ? `url(#gradient_${e.id})` : e.color,
					opacity: z.value.style.chart.lines.areaOpacity,
					class: E({ "vue-data-ui-transition": N(Ln) })
				}, null, 10, bt)) : S("", !0)], 64))), 256)),
				(O(!0), C(y, null, A(Y.value, (e) => (O(), C("path", {
					d: z.value.style.chart.lines.smooth ? e.smoothPath : e.straightPath,
					stroke: z.value.style.chart.lines.path.useSerieColor ? e.color : z.value.style.chart.lines.path.stroke,
					"stroke-width": z.value.style.chart.lines.strokeWidth,
					fill: "none",
					"stroke-linecap": "round",
					class: E({ "vue-data-ui-transition": N(Ln) })
				}, null, 10, xt))), 256)),
				z.value.style.chart.grid.y.axisLabels.show && !z.value.style.chart.lines.distributed ? (O(), C("g", {
					key: 9,
					ref_key: "scaleLabels",
					ref: xn
				}, [(O(!0), C(y, null, A(Lr.value, (e, t) => (O(), C("line", {
					key: `ytick_${t}`,
					class: E({ "vue-data-ui-transition": N(Ln) }),
					x1: sr.value ? X.value.right : X.value.left,
					x2: sr.value ? X.value.right + 6 : X.value.left - 6,
					y1: e.y,
					y2: e.y,
					stroke: z.value.style.chart.grid.x.axisColor,
					"stroke-width": 1
				}, null, 10, St))), 128)), (O(!0), C(y, null, A(Lr.value, (e, t) => (O(), C("text", {
					class: E({ "vue-data-ui-transition": N(Ln) }),
					transform: `translate(${e.x}, ${e.y + z.value.style.chart.grid.y.axisLabels.fontSize / 3})`,
					"font-size": z.value.style.chart.grid.y.axisLabels.fontSize,
					"font-weight": z.value.style.chart.grid.y.axisLabels.bold ? "bold" : "normal",
					fill: z.value.style.chart.grid.y.axisLabels.color,
					"text-anchor": sr.value ? "start" : "end"
				}, M(N(v)(z.value.style.chart.grid.y.axisLabels.formatter, e.value, N(h)({
					p: z.value.style.chart.lines.dataLabels.prefix,
					v: e.value,
					s: z.value.style.chart.lines.dataLabels.suffix,
					r: z.value.style.chart.grid.y.axisLabels.rounding
				}), { datapoint: e })), 11, Ct))), 256))], 512)) : S("", !0),
				z.value.style.chart.grid.x.timeLabels.show ? (O(), C("g", {
					key: 10,
					ref_key: "timeLabelsEls",
					ref: Sn
				}, [t.$slots["time-label"] ? (O(), C("g", wt, [(O(!0), C(y, null, A(Ur.value, (e, n) => (O(), C("g", null, [j(t.$slots, "time-label", qe({ ref_for: !0 }, {
					x: W.value.left + Er.value * n + Er.value / 2,
					y: W.value.bottom + z.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + z.value.style.chart.grid.x.timeLabels.offsetY,
					fontSize: z.value.style.chart.grid.x.timeLabels.fontSize,
					fill: z.value.style.chart.grid.x.timeLabels.color,
					transform: `translate(${W.value.left + Er.value * n + Er.value / 2}, ${W.value.bottom + z.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + z.value.style.chart.grid.x.timeLabels.offsetY}), rotate(${z.value.style.chart.grid.x.timeLabels.rotation})`,
					absoluteIndex: e.absoluteIndex,
					content: e.text,
					textAnchor: z.value.style.chart.grid.x.timeLabels.rotation > 0 ? "start" : z.value.style.chart.grid.x.timeLabels.rotation < 0 ? "end" : "middle",
					show: !0
				}), void 0, !0)]))), 256))])) : (O(), C("g", Tt, [(O(!0), C(y, null, A(Ur.value, (e, n) => (O(), C("g", null, [String(e.text).includes("\n") ? (O(), C("text", {
					key: n + "-multi",
					"text-anchor": z.value.style.chart.grid.x.timeLabels.rotation > 0 ? "start" : z.value.style.chart.grid.x.timeLabels.rotation < 0 ? "end" : "middle",
					"font-size": z.value.style.chart.grid.x.timeLabels.fontSize,
					fill: z.value.style.chart.grid.x.timeLabels.color,
					transform: `
                                        translate(
                                        ${Q(n)},
                                        ${W.value.bottom + z.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + z.value.style.chart.grid.x.timeLabels.offsetY}
                                        ),
                                        rotate(${z.value.style.chart.grid.x.timeLabels.rotation})
                                    `,
					style: Ye({ cursor: B.value ? "pointer" : "default" }),
					innerHTML: t.createTSpansFromLineBreaksOnX({
						content: String(e.text),
						fontSize: z.value.style.chart.grid.x.timeLabels.fontSize,
						fill: z.value.style.chart.grid.x.timeLabels.color,
						x: 0,
						y: 0
					}),
					onClick: () => bi(e, n)
				}, null, 12, Dt)) : (O(), C("text", {
					class: "vue-data-ui-time-label",
					key: n,
					"text-anchor": z.value.style.chart.grid.x.timeLabels.rotation > 0 ? "start" : z.value.style.chart.grid.x.timeLabels.rotation < 0 ? "end" : "middle",
					"font-size": z.value.style.chart.grid.x.timeLabels.fontSize,
					"font-weight": z.value.style.chart.grid.x.timeLabels.bold ? "bold" : "normal",
					fill: z.value.style.chart.grid.x.timeLabels.color,
					transform: `translate(${Q(n)}, ${W.value.bottom + z.value.style.chart.grid.x.timeLabels.fontSize * 1.3 + z.value.style.chart.grid.x.timeLabels.offsetY}), rotate(${z.value.style.chart.grid.x.timeLabels.rotation})`,
					style: Ye({ cursor: B.value ? "pointer" : "default" }),
					onClick: () => bi(e, n)
				}, M(e.text), 13, Et))]))), 256))]))], 512)) : S("", !0),
				(vn.value || ![null, void 0].includes($.value)) && !z.value.style.chart.highlighter.useLine ? (O(!0), C(y, { key: 11 }, A(q.value.end - q.value.start, (e, t) => (O(), C("g", { key: `tooltip_trap_highlighter_${t}` }, [w("rect", {
					x: di(t).x,
					y: W.value.top,
					height: W.value.height,
					width: di(t).width,
					fill: [Pn.value, $.value].includes(t) ? N(n)(z.value.style.chart.highlighter.color, z.value.style.chart.highlighter.opacity) : "transparent",
					style: {
						transition: "none !important",
						animation: "none !important"
					}
				}, null, 8, Ot)]))), 128)) : S("", !0),
				(vn.value || ![null, void 0].includes($.value)) && z.value.style.chart.highlighter.useLine ? (O(), C(y, { key: 12 }, [![null, void 0].includes($.value) || ![null, void 0].includes(Pn.value) ? (O(), C("g", kt, [w("line", {
					x1: Q(($.value ?? Pn.value) || 0),
					x2: Q(($.value ?? Pn.value) || 0),
					y1: N(ie)(W.value.top),
					y2: N(ie)(W.value.bottom),
					stroke: z.value.style.chart.highlighter.color,
					"stroke-width": z.value.style.chart.highlighter.lineWidth,
					"stroke-dasharray": z.value.style.chart.highlighter.lineDasharray,
					"stroke-linecap": "round",
					style: {
						transition: "none !important",
						animation: "none !important",
						"pointer-events": "none"
					}
				}, null, 8, At)])) : S("", !0)], 64)) : S("", !0),
				(O(!0), C(y, null, A(Y.value, (e) => (O(), C(y, { key: `shp_sel_${e.id}` }, [vn.value && q.value.end - q.value.start > z.value.style.chart.lines.dot.hideAboveMaxSerieLength ? (O(), C("g", jt, [$.value == null ? S("", !0) : (O(), C(y, { key: 0 }, [e.rel.includes($.value) && e.fullSeries?.[q.value.start + $.value] != null && !Number.isNaN(e.fullSeries?.[q.value.start + $.value]) ? (O(), x(Ae, {
					key: 0,
					shape: [
						"triangle",
						"square",
						"diamond",
						"pentagon",
						"hexagon",
						"star"
					].includes(e.shape) ? e.shape : "circle",
					color: z.value.style.chart.lines.dot.useSerieColor ? e.color : z.value.style.chart.lines.dot.fill,
					plot: {
						x: N(_)(e.points[e.rel.indexOf($.value)].x),
						y: N(_)(e.points[e.rel.indexOf($.value)].y)
					},
					radius: z.value.style.chart.lines.dot.radius * 1.3,
					stroke: z.value.style.chart.lines.dot.useSerieColor ? z.value.style.chart.lines.dot.stroke : e.color,
					strokeWidth: z.value.style.chart.lines.dot.strokeWidth,
					still: N(zn),
					class: E({ "vue-data-ui-transition": N(Ln) })
				}, null, 8, [
					"shape",
					"color",
					"plot",
					"radius",
					"stroke",
					"strokeWidth",
					"still",
					"class"
				])) : S("", !0)], 64))])) : S("", !0)], 64))), 128)),
				(O(!0), C(y, null, A(Y.value, (e) => (O(), C(y, { key: `shp_${e.id}` }, [q.value.end - q.value.start < z.value.style.chart.lines.dot.hideAboveMaxSerieLength ? (O(!0), C(y, { key: 0 }, A(e.points, (t, n) => (O(), C("g", { key: `shp_${e.id}_${q.value.start + n}` }, [e.fullSeries?.[q.value.start + e.rel[n]] != null && !Number.isNaN(e.fullSeries?.[q.value.start + e.rel[n]]) ? (O(), x(Ae, {
					key: 0,
					shape: [
						"triangle",
						"square",
						"diamond",
						"pentagon",
						"hexagon",
						"star"
					].includes(e.shape) ? e.shape : "circle",
					color: z.value.style.chart.lines.dot.useSerieColor ? e.color : z.value.style.chart.lines.dot.fill,
					plot: {
						x: t.x,
						y: t.y
					},
					radius: vn.value && $.value === e.rel[n] ? z.value.style.chart.lines.dot.radius * 1.3 : z.value.style.chart.lines.dot.radius,
					stroke: z.value.style.chart.lines.dot.useSerieColor ? z.value.style.chart.lines.dot.stroke : e.color,
					strokeWidth: z.value.style.chart.lines.dot.strokeWidth,
					still: N(zn),
					class: E({ "vue-data-ui-transition": N(Ln) })
				}, null, 8, [
					"shape",
					"color",
					"plot",
					"radius",
					"stroke",
					"strokeWidth",
					"still",
					"class"
				])) : S("", !0)]))), 128)) : S("", !0)], 64))), 128)),
				H.value.dataLabels.show && z.value.style.chart.lines.dataLabels.hideAboveMaxSerieLength > q.value.end - q.value.start ? (O(), C(y, { key: 13 }, [(O(!0), C(y, null, A(Y.value, (e, t) => (O(), C("g", { key: `dl_${e.id}` }, [(O(!0), C(y, null, A(e.points, (n, r) => (O(), C(y, { key: `dp_${e.id}_${q.value.start + r}` }, [Xi(e.series[r], e.proportions[r]) ? (O(), C("text", {
					key: 0,
					class: E({ "vue-data-ui-transition": N(Ln) }),
					transform: `translate(${n.x}, ${n.y + (e.series[r] >= 0 ? -z.value.style.chart.lines.dataLabels.fontSize / 2 + z.value.style.chart.lines.dataLabels.offsetY : z.value.style.chart.lines.dataLabels.fontSize * 1.2 - z.value.style.chart.lines.dataLabels.offsetY)})`,
					"font-size": z.value.style.chart.lines.dataLabels.fontSize,
					fill: z.value.style.chart.lines.dataLabels.color,
					"font-weight": z.value.style.chart.lines.dataLabels.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, M(z.value.style.chart.lines.showDistributedPercentage && z.value.style.chart.lines.distributed ? $i(e.proportions[r] * 100, e, t, e.rel[r]) : Qi(e.series[r], e, t, e.rel[r], e.signedSeries[r])), 11, Mt)) : S("", !0)], 64))), 128))]))), 128)), z.value.style.chart.lines.totalValues.show && Y.value.length > 1 ? (O(), C("g", {
					key: 0,
					ref_key: "sumTop",
					ref: Cn
				}, [(O(!0), C(y, null, A(Pr.value, (e, t) => (O(), C(y, { key: `total_l_${t + q.value.start}` }, [!z.value.style.chart.lines.dataLabels.hideEmptyValues || e.value !== 0 ? (O(), C("text", {
					key: 0,
					class: E({ "vue-data-ui-transition": N(Ln) }),
					transform: `translate(${Q(t)}, ${Ir(t)})`,
					"text-anchor": "middle",
					"font-size": z.value.style.chart.lines.totalValues.fontSize,
					"font-weight": z.value.style.chart.lines.totalValues.bold ? "bold" : "normal",
					fill: z.value.style.chart.lines.totalValues.color
				}, M(Qi(e.value, e, t, t, e.sign)), 11, Nt)) : S("", !0)], 64))), 128))], 512)) : S("", !0)], 64)) : S("", !0),
				br.value ? (O(), C("rect", qe({ key: 14 }, hr.value, {
					"data-start": q.value.start,
					"data-end": q.value.end
				}), null, 16, Pt)) : S("", !0),
				j(t.$slots, "svg", { svg: {
					drawingArea: W.value,
					data: Y.value,
					isPrintingImg: N(Xn) || N(Zn) || N(Pi),
					isPrintingSvg: N(Fi)
				} }, void 0, !0)
			], 46, lt)), t.$slots.hint ? (O(), C("div", Ft, [j(t.$slots, "hint", D(T({
				hint: z.value.a11y.translations.keyboardNavigation,
				isVisible: na.value
			})), void 0, !0)])) : S("", !0)]),
			t.$slots.watermark ? (O(), C("div", It, [j(t.$slots, "watermark", D(T({ isPrinting: N(Xn) || N(Zn) || N(Pi) || N(Fi) })), void 0, !0)])) : S("", !0),
			hn.value && (z.value.style.chart.legend.show || t.$slots.legend) ? (O(), x(He, {
				key: 6,
				to: z.value.style.chart.legend.position === "top" ? `#legend-top-${I.value}` : `#legend-bottom-${I.value}`
			}, [w("div", {
				ref_key: "chartLegend",
				ref: on
			}, [j(t.$slots, "legend", { legend: Ci.value }, () => [z.value.style.chart.legend.show ? (O(), x(Be, {
				key: 0,
				legendSet: Ci.value,
				config: wi.value,
				isCursorPointer: B.value,
				onClickMarker: r[0] ||= ({ legend: e }) => e.segregate()
			}, Ue({
				item: P(({ legend: e }) => [N(zn) ? S("", !0) : (O(), C("div", {
					key: 0,
					onClick: (t) => e.segregate(),
					style: Ye(`opacity:${L.value.includes(e.id) ? .5 : 1}`)
				}, M(e.name), 13, Lt))]),
				legendToggle: P(() => [Ci.value.length > 2 && z.value.style.chart.legend.selectAllToggle.show && !N(zn) ? (O(), x(Ie, {
					key: 0,
					backgroundColor: z.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: z.value.style.chart.legend.selectAllToggle.color,
					fontSize: z.value.style.chart.legend.fontSize,
					checked: L.value.length > 0,
					isCursorPointer: B.value,
					onToggle: xi
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : S("", !0)]),
				_: 2
			}, [t.$slots.pattern ? {
				name: "legend-pattern",
				fn: P(({ legend: e, index: t }) => [Ge(Ae, {
					shape: e.shape,
					radius: 30,
					stroke: "none",
					plot: {
						x: 30,
						y: 30
					},
					fill: `url(#pattern_${I.value}_${t})`
				}, null, 8, ["shape", "fill"])]),
				key: "0"
			} : void 0]), 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : S("", !0)], !0)], 512)], 8, ["to"])) : S("", !0),
			en.value && z.value.userOptions.buttons.table ? (O(), x(Qe(zi.value.component), qe({ key: 7 }, zi.value.props, {
				ref_key: "tableUnit",
				ref: gn,
				onClose: Bi
			}), Ue({
				content: P(() => [Ge(N(Wt), {
					colNames: Ai.value.colNames,
					head: Ai.value.head,
					body: Ai.value.body,
					config: Ai.value.config,
					title: z.value.table.useDialog ? "" : zi.value.title,
					withCloseButton: !z.value.table.useDialog,
					isCursorPointer: B.value,
					onClose: Bi
				}, {
					th: P(({ th: e }) => [w("div", { innerHTML: e }, null, 8, Rt)]),
					td: P(({ td: e }) => [We(M(isNaN(Number(e)) ? e : N(h)({
						p: z.value.style.chart.lines.dataLabels.prefix,
						v: e,
						s: z.value.style.chart.lines.dataLabels.suffix,
						r: z.value.table.td.roundingValue
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
			}, [z.value.table.useDialog ? {
				name: "title",
				fn: P(() => [We(M(zi.value.title), 1)]),
				key: "0"
			} : void 0, z.value.table.useDialog ? {
				name: "actions",
				fn: P(() => [w("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: r[1] ||= (e) => ki(z.value.userOptions.callbacks.csv),
					style: Ye({ cursor: B.value ? "pointer" : "default" })
				}, [Ge(N(Ht), {
					name: "fileCsv",
					stroke: zi.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : S("", !0),
			z.value.style.chart.zoom.show && en.value && Qr.value && K.value > 6 ? (O(), x(Fe, {
				key: 8,
				ref_key: "chartSlicer",
				ref: sn,
				"data-dom-to-png-ignore-layout": "",
				allMinimaps: Ar.value,
				background: z.value.style.chart.zoom.color,
				borderColor: z.value.style.chart.backgroundColor,
				customFormat: z.value.style.chart.zoom.customFormat,
				cutNullValues: !1,
				enableRangeHandles: z.value.style.chart.zoom.enableRangeHandles,
				enableSelectionDrag: z.value.style.chart.zoom.enableSelectionDrag,
				end: q.value.end,
				focusOnDrag: z.value.style.chart.zoom.focusOnDrag,
				focusRangeRatio: z.value.style.chart.zoom.focusRangeRatio,
				fontSize: z.value.style.chart.zoom.fontSize,
				immediate: !z.value.style.chart.zoom.preview.enable,
				inputColor: z.value.style.chart.zoom.color,
				isPreview: br.value,
				labelLeft: z.value.style.chart.grid.x.timeLabels.values[q.value.start] ? Rr.value?.[0]?.text ?? "" : "",
				labelRight: z.value.style.chart.grid.x.timeLabels.values[q.value.end - 1] ? Rr.value?.at(-1)?.text ?? "" : "",
				max: Math.max(...e.dataset.map((e) => e.series.length)),
				min: 0,
				minimap: kr.value,
				minimapCompact: z.value.style.chart.zoom.minimap.compact,
				minimapFrameColor: z.value.style.chart.zoom.minimap.frameColor,
				minimapIndicatorColor: z.value.style.chart.zoom.minimap.indicatorColor,
				minimapMerged: !1,
				minimapSelectedColor: z.value.style.chart.zoom.minimap.selectedColor,
				minimapSelectedColorOpacity: z.value.style.chart.zoom.minimap.selectedColorOpacity,
				minimapSelectedIndex: $.value,
				minimapSelectionRadius: 1,
				preciseLabels: qr.value.length ? qr.value : zr.value,
				refreshEndPoint: z.value.style.chart.zoom.endIndex === null ? Math.max(...e.dataset.map((e) => e.series.length)) : z.value.style.chart.zoom.endIndex + 1,
				refreshStartPoint: z.value.style.chart.zoom.startIndex === null ? 0 : z.value.style.chart.zoom.startIndex,
				selectColor: z.value.style.chart.zoom.highlightColor,
				selectedSeries: qi.value,
				smoothMinimap: !1,
				start: q.value.start,
				textColor: z.value.style.chart.color,
				timeLabels: zr.value,
				usePreciseLabels: z.value.style.chart.grid.x.timeLabels.datetimeFormatter.enable && !z.value.style.chart.zoom.useDefaultFormat,
				valueEnd: q.value.end,
				valueStart: q.value.start,
				verticalHandles: z.value.style.chart.zoom.minimap.verticalHandles,
				maxWidth: z.value.style.chart.zoom.maxWidth,
				minimapLeftInsetRatio: W.value.chartWidth > 0 && z.value.style.chart.zoom.autoFit ? W.value.left / W.value.chartWidth : null,
				minimapRightInsetRatio: W.value.chartWidth > 0 && z.value.style.chart.zoom.autoFit ? (W.value.chartWidth - W.value.right) / W.value.chartWidth : null,
				isCursorPointer: B.value,
				additionalMinimapHeight: z.value.style.chart.zoom.minimap.additionalHeight,
				handleType: z.value.style.chart.zoom.minimap.handleType,
				handleIconColor: z.value.style.chart.zoom.minimap.handleIconColor,
				handleBorderWidth: z.value.style.chart.zoom.minimap.handleBorderWidth,
				handleBorderColor: z.value.style.chart.zoom.minimap.handleBorderColor,
				handleFill: z.value.style.chart.zoom.minimap.handleFill,
				handleWidth: z.value.style.chart.zoom.minimap.handleWidth,
				"onUpdate:end": ni,
				"onUpdate:start": ti,
				onTrapMouse: vr,
				onReset: r[2] ||= () => Tr({ force: !0 }),
				onFutureEnd: r[3] ||= (e) => xr("end", e),
				onFutureStart: r[4] ||= (e) => xr("start", e)
			}, {
				"reset-action": P(({ reset: e }) => [j(t.$slots, "reset-action", D(T({ reset: e })), void 0, !0)]),
				slotMap: P(({ width: e, height: t, unitW: n }) => [w("g", { innerHTML: Yi({
					minimapW: e,
					minimapH: t,
					unitW: n
				}) }, null, 8, zt)]),
				_: 3
			}, 8, /* @__PURE__ */ "allMinimaps.background.borderColor.customFormat.enableRangeHandles.enableSelectionDrag.end.focusOnDrag.focusRangeRatio.fontSize.immediate.inputColor.isPreview.labelLeft.labelRight.max.minimap.minimapCompact.minimapFrameColor.minimapIndicatorColor.minimapSelectedColor.minimapSelectedColorOpacity.minimapSelectedIndex.preciseLabels.refreshEndPoint.refreshStartPoint.selectColor.selectedSeries.start.textColor.timeLabels.usePreciseLabels.valueEnd.valueStart.verticalHandles.maxWidth.minimapLeftInsetRatio.minimapRightInsetRatio.isCursorPointer.additionalMinimapHeight.handleType.handleIconColor.handleBorderWidth.handleBorderColor.handleFill.handleWidth".split("."))) : S("", !0),
			w("div", { id: `legend-bottom-${I.value}` }, null, 8, Bt),
			Ge(N(Vt), {
				teleportTo: z.value.style.chart.tooltip.teleportTo,
				show: H.value.showTooltip && nn.value,
				backgroundColor: z.value.style.chart.tooltip.backgroundColor,
				color: z.value.style.chart.tooltip.color,
				fontSize: z.value.style.chart.tooltip.fontSize,
				borderRadius: z.value.style.chart.tooltip.borderRadius,
				borderColor: z.value.style.chart.tooltip.borderColor,
				borderWidth: z.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: z.value.style.chart.tooltip.backgroundOpacity,
				position: z.value.style.chart.tooltip.position,
				offsetX: z.value.style.chart.tooltip.offsetX,
				offsetY: z.value.style.chart.tooltip.offsetY,
				parent: tn.value,
				content: vi.value,
				isFullscreen: un.value,
				isCustom: z.value.style.chart.tooltip.customFormat && typeof z.value.style.chart.tooltip.customFormat == "function",
				smooth: z.value.style.chart.tooltip.smooth,
				backdropFilter: z.value.style.chart.tooltip.backdropFilter,
				smoothForce: z.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: z.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: kn.value === "keyboard",
				a11yPosition: On.value
			}, {
				"tooltip-before": P(() => [j(t.$slots, "tooltip-before", D(T({ ...Xr.value })), void 0, !0)]),
				tooltip: P(() => [j(t.$slots, "tooltip", D(T({ ...Xr.value })), void 0, !0)]),
				"tooltip-after": P(() => [j(t.$slots, "tooltip-after", D(T({ ...Xr.value })), void 0, !0)]),
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
			t.$slots.source ? (O(), C("div", {
				key: 9,
				ref_key: "source",
				ref: ln,
				dir: "auto"
			}, [j(t.$slots, "source", {}, void 0, !0)], 512)) : S("", !0),
			j(t.$slots, "skeleton", {}, () => [N(zn) ? (O(), x(xe, { key: 0 })) : S("", !0)], !0)
		], 46, at));
	}
}, [["__scopeId", "data-v-deca57cf"]]);
//#endregion
export { it as n, Vt as t };
