import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, G as r, Gt as i, Jt as a, Kt as o, Ot as s, Pt as c, S as ee, T as te, X as l, Y as ne, _ as re, bt as ie, i as u, jt as ae, pt as oe, q as se, qt as d, r as ce, t as le, tt as ue, w as de, xt as fe, zt as pe } from "./lib-Bttd6u5E.js";
import { n as me, t as he } from "./useHints-Dq_w2E8B.js";
import { t as ge } from "./useConfig-DlNpz6P8.js";
import { t as _e } from "./usePrinter-DN5bYhTG.js";
import { n as ve, t as ye } from "./BaseScanner-DZvpgOjM.js";
import { t as be } from "./useNestedProp-vPNvh7rV.js";
import { t as xe } from "./useThemeCheck-C43Tcqmk.js";
import { t as Se } from "./useChartExport-DNiwdPmb.js";
import { t as Ce } from "./useTransitions-g_zBREk2.js";
import { t as we } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as Te } from "./img-Bnokohej.js";
import { n as Ee } from "./Title-BE3qg9xl.js";
import { t as De } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Oe, t as ke } from "./useResponsive-ZtArZtUf.js";
import { t as Ae } from "./DefGrad-DVBqDjhO.js";
import { t as je } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Me } from "./A11yDataTable-DdRsVULz.js";
import { t as Ne } from "./useUserOptionState-DK-_1ddE.js";
import { t as Pe } from "./useChartAccessibility-DYqac8yF.js";
import { t as Fe } from "./Legend-CQxUgOd-.js";
import { t as Ie } from "./vue_ui_history_plot-CuN63VEc.js";
import { Fragment as f, Teleport as Le, computed as p, createBlock as m, createCommentVNode as h, createElementBlock as g, createElementVNode as _, createSlots as Re, createTextVNode as ze, createVNode as Be, defineAsyncComponent as v, guardReactiveProps as y, mergeProps as Ve, nextTick as He, normalizeClass as b, normalizeProps as x, normalizeStyle as S, onBeforeUnmount as Ue, onMounted as We, openBlock as C, ref as w, renderList as T, renderSlot as E, resolveDynamicComponent as Ge, shallowRef as Ke, toDisplayString as D, toRefs as qe, unref as O, watch as Je, watchEffect as Ye, withCtx as k } from "vue";
//#region src/components/vue-ui-history-plot.vue
var Xe = /* @__PURE__ */ e({ default: () => Ft }), Ze = ["id"], Qe = ["id"], $e = ["id"], et = { style: { position: "relative" } }, tt = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], nt = [
	"x",
	"y",
	"width",
	"height"
], rt = { key: 1 }, it = [
	"id",
	"x1",
	"y1",
	"x2",
	"y2"
], at = ["stop-color", "offset"], ot = { key: 2 }, st = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], ct = { key: 3 }, lt = [
	"stroke",
	"stroke-width",
	"x1",
	"x2",
	"y1",
	"y2"
], ut = [
	"x",
	"y",
	"fill",
	"font-size"
], dt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], ft = { key: 6 }, pt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], mt = { key: 7 }, ht = [
	"stroke",
	"stroke-width",
	"x1",
	"x2",
	"y1",
	"y2"
], gt = [
	"transform",
	"fill",
	"font-size",
	"text-anchor"
], _t = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], vt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], yt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], bt = [
	"d",
	"stroke",
	"stroke-width"
], xt = [
	"d",
	"stroke",
	"stroke-width"
], St = [
	"cx",
	"cy",
	"fill",
	"r"
], Ct = [
	"cx",
	"cy",
	"fill",
	"r",
	"stroke",
	"stroke-width"
], wt = { key: 1 }, Tt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], Et = [
	"transform",
	"font-size",
	"fill",
	"font-weight",
	"innerHTML"
], Dt = { key: 2 }, Ot = [
	"transform",
	"font-size",
	"font-weight",
	"fill"
], kt = [
	"cx",
	"cy",
	"r",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], At = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, jt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Mt = ["id"], Nt = ["onClick"], Pt = ["innerHTML"], Ft = /*#__PURE__*/ De({
	__name: "vue-ui-history-plot",
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
	emits: [
		"selectLegend",
		"selectDatapoint",
		"copyAlt"
	],
	setup(e, { expose: De, emit: Xe }) {
		let Ft = v(() => import("./Tooltip-DhjyfHwz.js")), It = v(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Lt = v(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Rt = v(() => import("./DataTable-BbKgJ5UI.js")), zt = v(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Bt = v(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Vt = v(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ht = v(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_history_plot: Ut } = ge(), { isThemeValid: Wt, warnInvalidTheme: Gt } = xe(), A = e, j = w(null), Kt = w(null), qt = w(0), Jt = w(null), Yt = w(0), Xt = w(0), Zt = w(0), M = Ke(null), N = Ke(null), Qt = w(null), P = w(se()), $t = w(!1), en = w(""), F = w([]), I = w(!1), L = w(null), R = w(!1), tn = w(null), nn = w(!1), rn = w(null), an = w(null), on = w(null), sn = w(null), cn = w(null), ln = w(null), z = w(null), B = w(null), un = w({
			x: 0,
			y: 0
		}), dn = w("pointer"), fn = w(!1), pn = p({
			get: () => !!A.dataset && A.dataset.length,
			set: (e) => e
		}), mn = Xe;
		We(() => {
			nn.value = !0, gn();
		});
		let hn = p(() => V.value.debug);
		function gn() {
			if (ae(A.dataset) ? (ue({
				componentName: "VueUiHistoryPlot",
				type: "dataset",
				debug: hn.value
			}), xn.value = !0) : A.dataset.forEach((e, t) => {
				oe({
					datasetObject: e,
					requiredAttributes: ["name", "values"]
				}).forEach((e) => {
					pn.value = !1, ue({
						componentName: "VueUiHistoryPlot",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: hn.value
					});
				});
			}), ae(A.dataset) || (xn.value = V.value.loading), V.value.responsive) {
				let e = Oe(() => {
					let { width: e, height: t } = ke({
						chart: j.value,
						title: V.value.style.chart.title.text ? Kt.value : null,
						noTitle: Qt.value,
						legend: V.value.style.chart.legend.show ? Jt.value : null,
						source: tn.value
					});
					requestAnimationFrame(() => {
						K.value.width = e, K.value.height = t, V.value.responsiveProportionalSizing ? (J.value.plots = d({
							relator: Math.min(e, t),
							adjuster: 600,
							source: V.value.style.chart.plots.radius,
							threshold: 3,
							fallback: 3
						}), J.value.indexLabels = d({
							relator: Math.min(e, t),
							adjuster: 600,
							source: V.value.style.chart.plots.indexLabels.fontSize,
							threshold: 6,
							fallback: 6
						}), J.value.labels = d({
							relator: Math.min(e, t),
							adjuster: 600,
							source: V.value.style.chart.plots.labels.fontSize,
							threshold: 6,
							fallback: 6
						}), J.value.xAxisLabels = d({
							relator: Math.min(e, t),
							adjuster: 600,
							source: V.value.style.chart.axes.x.labels.fontSize,
							threshold: 6,
							fallback: 6
						}), J.value.xAxisName = d({
							relator: Math.min(e, t),
							adjuster: 600,
							source: V.value.style.chart.axes.x.name.fontSize,
							threshold: 6,
							fallback: 6
						}), J.value.yAxisLabels = d({
							relator: Math.min(e, t),
							adjuster: 600,
							source: V.value.style.chart.axes.y.labels.fontSize,
							threshold: 6,
							fallback: 6
						}), J.value.yAxisName = d({
							relator: Math.min(e, t),
							adjuster: 600,
							source: V.value.style.chart.axes.y.name.fontSize,
							threshold: 6,
							fallback: 6
						})) : (J.value.plots = V.value.style.chart.plots.radius, J.value.indexLabels = V.value.style.chart.plots.indexLabels.fontSize, J.value.labels = V.value.style.chart.plots.labels.fontSize, J.value.xAxisLabels = V.value.style.chart.axes.x.labels.fontSize, J.value.xAxisName = V.value.style.chart.axes.x.name.fontSize, J.value.yAxisLabels = V.value.style.chart.axes.y.labels.fontSize, J.value.yAxisName = V.value.style.chart.axes.y.name.fontSize);
					});
				});
				M.value && (N.value && M.value.unobserve(N.value), M.value.disconnect()), M.value = new ResizeObserver(e), N.value = j.value.parentNode, M.value.observe(N.value);
			}
		}
		Ue(() => {
			M.value && (N.value && M.value.unobserve(N.value), M.value.disconnect());
		});
		function _n() {
			let e = be({
				userConfig: A.config,
				defaultConfig: Ut
			}), t = {}, n = e.theme;
			if (n) if (!Wt.value(e)) Gt(e), t = e;
			else {
				let r = be({
					userConfig: Ie[n] || A.config,
					defaultConfig: e
				});
				t = {
					...be({
						userConfig: A.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : o[n] || c
				};
			}
			else t = e;
			return t;
		}
		let V = w(_n());
		me({
			config: () => V.value,
			dataset: () => A.dataset,
			component: "VueUiHistoryPlot",
			rules: [he.emptyArray, {
				test: (e) => e.length > 6,
				message: [
					"👀 The number of series is > 6. Consider:",
					"",
					"▶️ Using filters to let users choose a maximum number of series to display."
				]
			}]
		});
		let { transitionEnabled: H } = Ce({
			config: () => V.value.transitions,
			dataset: () => A.dataset
		}), U = p(() => V.value.userOptions.useCursorPointer), vn = p(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					axes: {
						x: {
							scaleMin: 0,
							scaleMax: 10,
							labels: { show: !1 },
							name: { text: "" }
						},
						y: {
							scaleMin: 0,
							scaleMax: 10,
							labels: { show: !1 },
							name: { text: "" }
						}
					},
					grid: {
						xAxis: { stroke: "#6A6A6A" },
						horizontalLines: { stroke: "#6A6A6A50" },
						yAxis: { stroke: "#6A6A6A" },
						verticalLines: { stroke: "#6A6A6A50" }
					},
					legend: { backgroundColor: "transparent" },
					paths: {
						useSerieColor: !1,
						stroke: "#6A6A6A"
					},
					plots: {
						stroke: "#6A6A6A",
						indexLabels: { show: !1 },
						labels: { show: !1 }
					}
				} }
			},
			userConfig: V.value.skeletonConfig ?? {}
		})), { loading: yn, FINAL_DATASET: bn, manualLoading: xn } = ve({
			...qe(A),
			FINAL_CONFIG: V,
			prepareConfig: _n,
			skeletonDataset: A.config?.skeletonDataset ?? [{
				name: "",
				color: "#CACACA",
				values: [
					{
						label: "",
						x: 1,
						y: 9
					},
					{
						label: "",
						x: 4,
						y: 1
					},
					{
						label: "",
						x: 7,
						y: 9
					},
					{
						label: "",
						x: 9,
						y: 4
					}
				]
			}],
			skeletonConfig: a({
				defaultConfig: V.value,
				userConfig: vn.value
			})
		}), { userOptionsVisible: Sn, setUserOptionsVisibility: Cn, keepUserOptionState: wn } = Ne({ config: V.value }), { svgRef: W } = Pe({ config: V.value.style.chart.title });
		Je(() => A.config, (e) => {
			yn.value || (V.value = _n()), Sn.value = !V.value.userOptions.showOnChartHover, gn(), qt.value += 1, Xt.value += 1, Yt.value += 1, K.value.height = V.value.style.chart.height, K.value.width = V.value.style.chart.width, J.value.plots = V.value.style.chart.plots.radius, J.value.indexLabels = V.value.style.chart.plots.indexLabels.fontSize, J.value.labels = V.value.style.chart.plots.labels.fontSize, J.value.xAxisLabels = V.value.style.chart.axes.x.labels.fontSize, J.value.xAxisName = V.value.style.chart.axes.x.name.fontSize, J.value.yAxisLabels = V.value.style.chart.axes.y.labels.fontSize, J.value.yAxisName = V.value.style.chart.axes.y.name.fontSize, G.value.showTable = V.value.table.show, G.value.showTooltip = V.value.style.chart.tooltip.show;
		}, { deep: !0 }), Je(() => A.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (xn.value = !1);
		}, { deep: !0 });
		let { isPrinting: Tn, isImaging: En, generatePdf: Dn, generateImage: On } = _e({
			elementId: `history_plot_${P.value}`,
			fileName: V.value.style.chart.title.text || "vue-ui-history-plot",
			options: V.value.userOptions.print
		}), kn = p(() => V.value.userOptions.show && !V.value.style.chart.title.text), An = p(() => de(V.value.customPalette)), G = w({
			showTable: V.value.table.show,
			showTooltip: V.value.style.chart.tooltip.show
		});
		Je(V, () => {
			G.value = {
				showTable: V.value.table.show,
				showTooltip: V.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let K = w({
			height: V.value.style.chart.height,
			width: V.value.style.chart.width
		}), jn = p(() => K.value.width), Mn = p(() => K.value.height);
		function Nn() {
			let e = 0;
			ln.value && (e = Array.from(ln.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = sn.value ? sn.value.getBoundingClientRect().width : 0;
			return e + t + (t ? 24 : 0);
		}
		let Pn = w(0), Fn = Oe((e) => {
			Pn.value = e;
		});
		Ye((e) => {
			let t = cn.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				Fn(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		});
		let In = p(() => {
			let e = 0;
			on.value && (e = on.value.getBBox().height + J.value.xAxisName / 2);
			let t = 0;
			return cn.value && (t = Pn.value), e + t;
		}), q = p(() => {
			let e = V.value.style.chart.padding.left, t = V.value.style.chart.padding.top, n = Nn(), r = V.value.style.chart.plots.radius;
			return {
				left: e + n + V.value.style.chart.axes.y.name.offsetX,
				top: t + r,
				right: K.value.width - V.value.style.chart.padding.right - r - V.value.style.chart.axes.y.name.offsetX,
				bottom: K.value.height - V.value.style.chart.padding.bottom - In.value - r - V.value.style.chart.axes.x.name.offsetY,
				width: K.value.width - e - V.value.style.chart.padding.right - n - r - V.value.style.chart.axes.y.name.offsetX,
				height: K.value.height - t - V.value.style.chart.padding.bottom - In.value - r * 2 - V.value.style.chart.axes.x.name.offsetY
			};
		}), J = w({
			plots: V.value.style.chart.plots.radius,
			indexLabels: V.value.style.chart.plots.indexLabels.fontSize,
			labels: V.value.style.chart.plots.labels.fontSize,
			xAxisLabels: V.value.style.chart.axes.x.labels.fontSize,
			xAxisName: V.value.style.chart.axes.x.name.fontSize,
			yAxisLabels: V.value.style.chart.axes.y.labels.fontSize,
			yAxisName: V.value.style.chart.axes.y.name.fontSize
		}), Y = p(() => bn.value.map((e, t) => {
			let n = Array.isArray(e.temperatureColors) && e.temperatureColors.length ? e.temperatureColors.map((e) => ee(e)) : void 0;
			return {
				...e,
				color: e.color ? ee(e.color) : An.value[t] || c[t] || c[t % c.length],
				temperatureColors: n,
				temperatureAngle: Ln(e.temperatureAngle),
				temperatureIndependant: e.temperatureIndependant === !0,
				usePlotTemperatureColors: e.usePlotTemperatureColors !== !1,
				seriesIndex: t
			};
		}));
		function Ln(e = 0) {
			let t = Number(e);
			return Number.isFinite(t) ? (t % 360 + 360) % 360 : 0;
		}
		let Rn = p(() => Math.max(...Y.value.filter((e) => !F.value.includes(e.seriesIndex)).flatMap((e) => e.values.map((e) => e.x)))), zn = p(() => {
			let e = Math.min(...Y.value.filter((e) => !F.value.includes(e.seriesIndex)).flatMap((e) => e.values.map((e) => e.x)));
			return e < 0 ? e : 0;
		}), Bn = p(() => Math.max(...Y.value.filter((e) => !F.value.includes(e.seriesIndex)).flatMap((e) => e.values.map((e) => e.y)))), Vn = p(() => {
			let e = Math.min(...Y.value.filter((e) => !F.value.includes(e.seriesIndex)).flatMap((e) => e.values.map((e) => e.y)));
			return e < 0 ? e : 0;
		}), X = p(() => {
			let e = re(V.value.style.chart.axes.x.scaleMin ?? zn.value, V.value.style.chart.axes.x.scaleMax ?? Rn.value, V.value.style.chart.axes.x.ticks), t = re(V.value.style.chart.axes.y.scaleMin ?? Vn.value, V.value.style.chart.axes.y.scaleMax ?? Bn.value, V.value.style.chart.axes.y.ticks);
			return {
				x: e,
				y: t,
				tickX: e.ticks.map((t) => ({
					x: q.value.left + (t - e.min) / (e.max - e.min) * q.value.width,
					y1: q.value.top,
					y2: q.value.bottom,
					value: t
				})),
				tickY: t.ticks.map((e) => ({
					y: q.value.bottom - (e - t.min) / (t.max - t.min) * q.value.height,
					x1: q.value.left,
					x2: q.value.right,
					value: e
				}))
			};
		});
		function Hn(e) {
			let t = X.value.x.min < 0 ? Math.abs(X.value.x.min) : X.value.x.min > 0 ? -X.value.x.min : 0;
			return q.value.left + ((e || 0) + t) / (X.value.x.max + t) * q.value.width;
		}
		function Un(e) {
			let t = X.value.y.min < 0 ? Math.abs(X.value.y.min) : X.value.y.min > 0 ? -X.value.y.min : 0;
			return q.value.bottom - ((e || 0) + t) / (X.value.y.max + t) * q.value.height;
		}
		let Z = p(() => Y.value.filter((e) => !F.value.includes(e.seriesIndex)).map((e, t) => {
			let n = e.values.map((t, n) => ({
				valueX: t.x,
				valueY: t.y,
				label: t.label,
				x: Hn(t.x),
				y: Un(t.y),
				color: e.color,
				seriesName: e.name,
				id: se()
			})), r = e.smooth ? te(n) : n.map((e) => `${e.x},${e.y} `).join("").trim();
			return {
				...e,
				gradientIndex: t,
				plots: n,
				path: `M${r}`
			};
		})), Wn = p(() => {
			if (L.value === null || z.value === null) return Z.value;
			let e = Z.value.findIndex((e) => e.seriesIndex === z.value);
			if (e === -1) return Z.value;
			let t = Z.value[e];
			return [
				...Z.value.slice(0, e),
				...Z.value.slice(e + 1),
				t
			];
		}), Gn = p(() => Z.value.some((e) => Kn(e)));
		function Kn(e) {
			return Array.isArray(e?.temperatureColors) && e.temperatureColors.length;
		}
		function qn(e) {
			return Kn(e) && e.usePlotTemperatureColors;
		}
		function Jn(e) {
			let t = Array.isArray(e?.plots) ? e.plots : [];
			if (!t.length) return {
				minX: 0,
				maxX: 0,
				minY: 0,
				maxY: 0,
				width: 0,
				height: 0
			};
			let n = t.map((e) => e.x), r = t.map((e) => e.y), i = Math.min(...n), a = Math.max(...n), o = Math.min(...r), s = Math.max(...r);
			return {
				minX: i,
				maxX: a,
				minY: o,
				maxY: s,
				width: a - i,
				height: s - o
			};
		}
		function Yn() {
			let e = q.value;
			return {
				minX: e.left,
				maxX: e.right,
				minY: e.top,
				maxY: e.bottom,
				width: e.width,
				height: e.height
			};
		}
		function Xn(e) {
			return e?.temperatureIndependant ? Jn(e) : Yn();
		}
		function Zn(e) {
			let t = Ln(e?.temperatureAngle) * Math.PI / 180, n = Math.sin(t), r = Math.cos(t), i = Xn(e), a = i.minX + i.width / 2, o = i.minY + i.height / 2, s = (Math.abs(n) * i.width + Math.abs(r) * i.height) / 2 || 1;
			return {
				x1: a - n * s,
				y1: o - r * s,
				x2: a + n * s,
				y2: o + r * s
			};
		}
		function Qn(e, t) {
			let { x1: n, y1: r, x2: i, y2: a } = Zn(e), o = i - n, s = a - r, c = o * o + s * s;
			if (!c) return 0;
			let ee = ((t.x - n) * o + (t.y - r) * s) / c;
			return Math.min(1, Math.max(0, ee));
		}
		function Q(e, t) {
			return ie({
				colors: e.temperatureColors,
				ratio: Qn(e, t)
			});
		}
		function $n(e, t) {
			return `temperature_plot_grad_history_${e.seriesIndex}_${t.id}_${P.value}`;
		}
		function er(e, t, n) {
			return qn(e) ? V.value.style.chart.plots.gradient.show ? `url(#${$n(e, t)})` : Q(e, t) : V.value.style.chart.plots.gradient.show ? `url(#gradient_${n}_${P.value})` : t.color;
		}
		function tr(e, t) {
			return qn(e) ? Q(e, t) : e.color;
		}
		function nr(e, t) {
			return V.value.style.chart.plots.indexLabels.adaptColorToBackground ? ce(tr(e, t)) : V.value.style.chart.plots.indexLabels.color;
		}
		function rr(e, t) {
			let n = Z.value.find((t) => t.seriesIndex === e);
			return n && qn(n) ? Q(n, t) : t.color;
		}
		function ir(e) {
			return Kn(e) ? `url(#temperature_grad_history_${e.seriesIndex}_${P.value})` : V.value.style.chart.paths.useSerieColor ? e.color : V.value.style.chart.paths.stroke;
		}
		function ar(e) {
			return L.value === null || z.value === e ? 1 : Math.min(1, V.value.style.chart.plots.unselectedOpacity);
		}
		p(() => L.value !== null && z.value !== null);
		let or = w(!1);
		function sr() {
			F.value.length ? F.value = [] : fr.value.forEach((e) => {
				F.value.push(e.seriesIndex);
			}), mn("selectLegend", Z.value);
		}
		function cr(e) {
			F.value.includes(e) ? F.value = F.value.filter((t) => t !== e) : F.value.push(e), mn("selectLegend", Z.value);
		}
		function lr(e) {
			return Y.value.length ? Y.value.find((t) => t.name === e) || (hn.value && console.warn(`VueUiHistoryPlot - Series name not found "${e}"`), null) : (hn.value && console.warn("VueUiHistoryPlot - There are no series to show."), null);
		}
		function ur(e) {
			let t = lr(e);
			t !== null && F.value.includes(t.seriesIndex) && cr(t.seriesIndex);
		}
		function dr(e) {
			let t = lr(e);
			t !== null && (F.value.includes(t.seriesIndex) || cr(t.seriesIndex));
		}
		let fr = p(() => Y.value.map((e) => {
			let t = Kn(e) ? e.temperatureColors : null;
			return {
				...e,
				gradientColors: t,
				opacity: F.value.includes(e.seriesIndex) ? .5 : 1,
				segregate: () => cr(e.seriesIndex),
				isSegregated: F.value.includes(e.seriesIndex),
				shape: t ? "gradient" : "circle"
			};
		})), pr = p(() => ({
			cy: "history-plot-div-legend",
			backgroundColor: V.value.style.chart.legend.backgroundColor,
			color: V.value.style.chart.legend.color,
			fontSize: V.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: V.value.style.chart.legend.bold ? "bold" : ""
		}));
		function mr({ datapoint: e, plotIndex: t, seriesIndex: n }) {
			V.value.events.datapointClick && V.value.events.datapointClick({
				datapoint: {
					...e,
					plotIndex: t,
					seriesIndex: n
				},
				seriesIndex: n
			}), mn("selectDatapoint", e);
		}
		function hr(e) {
			I.value = e, Zt.value += 1;
		}
		let gr = w(null);
		function _r({ datapoint: e, plotIndex: t, seriesIndex: n }) {
			V.value.events.datapointLeave && V.value.events.datapointLeave({
				datapoint: {
					...e,
					plotIndex: t,
					seriesIndex: n
				},
				seriesIndex: n
			}), (dn.value !== "keyboard" || z.value !== n || B.value !== t) && ($t.value = !1, L.value = null);
		}
		function vr(e, t) {
			if (!W.value) return;
			let n = Z.value.find((t) => t.seriesIndex === e);
			if (!n) return;
			let r = n.plots[t];
			if (!r) return;
			let a = i(r.x, r.y, W.value);
			a && (un.value = a);
		}
		function yr({ datapoint: e, plotIndex: t, seriesIndex: n, triggerMode: r = "pointer" }) {
			V.value.events.datapointEnter && V.value.events.datapointEnter({
				datapoint: {
					...e,
					plotIndex: t,
					seriesIndex: n
				},
				seriesIndex: n
			}), z.value = n, B.value = t, dn.value = r, or.value = !0;
			let i = rr(n, e), a = {
				...e,
				color: i
			};
			gr.value = {
				datapoint: a,
				color: i,
				seriesIndex: n,
				plotIndex: t,
				config: V.value,
				series: Y.value
			}, L.value = e;
			let o = V.value.style.chart.tooltip.customFormat;
			if (R.value = !1, fe(o)) try {
				let e = o({
					seriesIndex: n,
					datapoint: a,
					color: i,
					plotIndex: t,
					series: Y.value,
					config: V.value
				});
				typeof e == "string" && (en.value = e, R.value = !0);
			} catch {
				console.warn("Custom format cannot be applied."), R.value = !1;
			}
			if (!R.value) {
				let t = "";
				t += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;border-bottom:1px solid ${V.value.style.chart.tooltip.borderColor};margin-bottom:3px;padding-bottom:6px;"><svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="none" fill="${i}"/></svg><span>${e.seriesName}</span></div>`, t += `<div>${e.label}</div>`, t += `<div>${V.value.style.chart.axes.x.name.text || "x"}: ${u(V.value.style.chart.axes.x.labels.formatter, e.valueX, l({
					p: V.value.style.chart.axes.x.labels.prefix,
					v: e.valueX,
					s: V.value.style.chart.axes.x.labels.suffix,
					r: V.value.style.chart.axes.x.labels.rounding
				}))}</div>`, t += `<div>${V.value.style.chart.axes.y.name.text || "y"}: ${u(V.value.style.chart.axes.y.labels.formatter, e.valueY, l({
					p: V.value.style.chart.axes.y.labels.prefix,
					v: e.valueY,
					s: V.value.style.chart.axes.y.labels.suffix,
					r: V.value.style.chart.axes.y.labels.rounding
				}))}</div>`, en.value = `<div>${t}</div>`;
			}
			$t.value = !0, r === "keyboard" && He(() => {
				vr(n, t);
			});
		}
		p(() => ({ head: Z.value.map((e) => ({
			name: e.name,
			color: e.color
		})) }));
		let $ = p(() => {
			let e = [
				V.value.table.columnNames.series,
				V.value.table.columnNames.datapoint,
				V.value.table.columnNames.x,
				V.value.table.columnNames.y
			];
			return {
				head: e,
				body: Z.value.flatMap((e) => e.plots.map((e) => [
					{
						color: e.color,
						name: e.seriesName
					},
					e.label,
					u(V.value.style.chart.axes.x.labels.formatter, e.valueX, l({
						p: V.value.style.chart.axes.x.labels.prefix,
						v: e.valueX,
						s: V.value.style.chart.axes.x.labels.suffix,
						r: V.value.style.chart.axes.x.labels.rounding
					})),
					u(V.value.style.chart.axes.y.labels.formatter, e.valueY, l({
						p: V.value.style.chart.axes.y.labels.prefix,
						v: e.valueY,
						s: V.value.style.chart.axes.y.labels.suffix,
						r: V.value.style.chart.axes.y.labels.rounding
					}))
				])),
				config: {
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
				},
				colNames: e
			};
		});
		function br(e = null) {
			He(() => {
				let r = [
					[V.value.table.columnNames.series],
					[V.value.table.columnNames.datapoint],
					[V.value.table.columnNames.x],
					[V.value.table.columnNames.y]
				], i = Z.value.flatMap((e) => e.plots.map((e) => [
					[e.seriesName],
					[e.label],
					[e.valueX],
					[e.valueY]
				])), a = [
					[V.value.style.chart.title.text],
					[V.value.style.chart.title.subtitle.text],
					r
				].concat(i), o = n(a);
				e ? e(o) : t({
					csvContent: o,
					title: V.value.style.chart.title.text || "vue-ui-history-plot"
				});
			});
		}
		let xr = w(!1);
		function Sr() {
			xr.value = !xr.value;
		}
		function Cr() {
			G.value.showTable = !G.value.showTable;
		}
		function wr() {
			G.value.showTooltip = !G.value.showTooltip;
		}
		function Tr() {
			return Z.value;
		}
		async function Er({ scale: e = 2 } = {}) {
			if (!j.value) return;
			let { width: t, height: n } = j.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await Te({
				domElement: j.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: V.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Dr = p(() => X.value.tickX), Or = p(() => ({
			start: 0,
			end: X.value.tickX.length
		}));
		we({
			timeLabelsEls: cn,
			timeLabels: Dr,
			slicer: Or,
			configRef: V,
			rotationPath: [
				"style",
				"chart",
				"axes",
				"x",
				"labels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"axes",
				"x",
				"labels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: jn,
			height: Mn,
			targetClass: ".vue-ui-history-plot-x-axis-scale",
			rotation: V.value.style.chart.axes.x.labels.autoRotate.angle
		});
		let kr = p(() => {
			let e = V.value.table.useDialog && !V.value.table.show, t = G.value.showTable;
			return {
				component: e ? Ht : Lt,
				title: `${V.value.style.chart.title.text}${V.value.style.chart.title.subtitle.text ? `: ${V.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: V.value.table.th.backgroundColor,
					color: V.value.table.th.color,
					headerColor: V.value.table.th.color,
					headerBg: V.value.table.th.backgroundColor,
					isFullscreen: I.value,
					fullscreenParent: j.value,
					forcedWidth: Math.min(800, window.innerWidth * .8)
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: V.value.style.chart.backgroundColor,
							color: V.value.style.chart.color
						},
						head: {
							backgroundColor: V.value.style.chart.backgroundColor,
							color: V.value.style.chart.color
						}
					}
				}
			};
		});
		Je(() => G.value.showTable, (e) => {
			V.value.table.show || (e && V.value.table.useDialog && rn.value ? rn.value.open() : "close" in rn.value && rn.value.close());
		});
		function Ar() {
			G.value.showTable = !1, an.value && an.value.setTableIconState(!1);
		}
		let jr = p(() => V.value.style.chart.backgroundColor), Mr = p(() => V.value.style.chart.legend), Nr = p(() => V.value.style.chart.title), { isCallbackImaging: Pr, isCallbackSvg: Fr, generateSvg: Ir, onGenerateImage: Lr } = Se({
			svg: W,
			title: Nr,
			legend: Mr,
			legendItems: fr,
			backgroundColor: jr,
			getSvgCallback: () => V.value.userOptions.callbacks.svg,
			generateImage: On
		});
		async function Rr() {
			if (mn("copyAlt", {
				config: V.value,
				dataset: Z.value
			}), !V.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(V.value.userOptions.callbacks.altCopy({
				config: V.value,
				dataset: Z.value
			}));
		}
		let zr = p(() => ({
			head: $.value.head,
			body: $.value.body.map((e) => [
				e[0]?.name || "",
				e[1],
				e[2],
				e[3]
			]),
			caption: V.value.a11y.translations.tableCaption,
			notice: V.value.a11y.translations.tableAvailable
		}));
		function Br(e) {
			return Z.value.findIndex((t) => t.seriesIndex === e);
		}
		function Vr() {
			if (z.value !== null && B.value !== null) {
				let e = Z.value.find((e) => e.seriesIndex === z.value)?.plots?.[B.value];
				e && _r({
					datapoint: e,
					plotIndex: B.value,
					seriesIndex: z.value
				});
			}
			z.value = null, B.value = null, dn.value = "pointer", $t.value = !1, L.value = null;
		}
		function Hr() {
			z.value = null, B.value = null, fn.value = !0;
		}
		function Ur() {
			Vr(), fn.value = !1;
		}
		function Wr(e, t) {
			if (!e?.plots?.length) return null;
			let n = e.plots.length;
			return (t % n + n) % n;
		}
		function Gr({ seriesIndex: e, plotIndex: t, direction: n }) {
			if (!Z.value.length) return null;
			let r = Br(e);
			if (r === -1) return null;
			if (n === "up" || n === "down") {
				let e = Z.value[r];
				if (!e?.plots?.length) return null;
				let i = Wr(e, t + (n === "up" ? -1 : 1));
				return {
					seriesIndex: e.seriesIndex,
					plotIndex: i
				};
			}
			if (n === "left" || n === "right") {
				let e = ((r + (n === "left" ? -1 : 1)) % Z.value.length + Z.value.length) % Z.value.length, i = Z.value[e];
				if (!i?.plots?.length) return null;
				let a = Math.min(t, i.plots.length - 1);
				return {
					seriesIndex: i.seriesIndex,
					plotIndex: a
				};
			}
			return null;
		}
		function Kr(e) {
			if (!W.value || xr.value || document.activeElement !== W.value || !Z.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "ArrowUp", i = e.key === "ArrowDown", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				Vr();
				return;
			}
			if (a) {
				if (z.value === null || B.value === null) return;
				let e = Z.value.find((e) => e.seriesIndex === z.value)?.plots?.[B.value];
				if (!e) return;
				mr({
					datapoint: e,
					plotIndex: B.value,
					seriesIndex: z.value
				});
				return;
			}
			let s = null;
			if (z.value === null || B.value === null) {
				let e = Z.value[0];
				if (!e?.plots?.length) return;
				s = {
					seriesIndex: e.seriesIndex,
					plotIndex: 0
				};
			} else r ? s = Gr({
				seriesIndex: z.value,
				plotIndex: B.value,
				direction: "up"
			}) : i ? s = Gr({
				seriesIndex: z.value,
				plotIndex: B.value,
				direction: "down"
			}) : t ? s = Gr({
				seriesIndex: z.value,
				plotIndex: B.value,
				direction: "left"
			}) : n && (s = Gr({
				seriesIndex: z.value,
				plotIndex: B.value,
				direction: "right"
			}));
			if (!s) return;
			let c = Z.value.find((e) => e.seriesIndex === s.seriesIndex)?.plots?.[s.plotIndex];
			c && yr({
				datapoint: c,
				plotIndex: s.plotIndex,
				seriesIndex: s.seriesIndex,
				triggerMode: "keyboard"
			});
		}
		return De({
			getData: Tr,
			getImage: Er,
			generatePdf: Dn,
			generateCsv: br,
			generateImage: On,
			generateSvg: Ir,
			hideSeries: dr,
			showSeries: ur,
			toggleTable: Cr,
			toggleTooltip: wr,
			toggleAnnotator: Sr,
			toggleFullscreen: hr,
			copyAlt: Rr
		}), (e, t) => (C(), g("div", {
			id: `history_plot_${P.value}`,
			ref_key: "historyPlotChart",
			ref: j,
			class: b({
				"vue-data-ui-component": !0,
				"vue-ui-history-plot": !0,
				"vue-data-ui-wrapper-fullscreen": I.value
			}),
			style: S(`background:${V.value.style.chart.backgroundColor};color:${V.value.style.chart.color};font-family:${V.value.style.fontFamily}; position: relative; ${V.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: t[2] ||= () => O(Cn)(!0),
			onMouseleave: t[3] ||= () => O(Cn)(!1)
		}, [
			_("div", {
				id: `chart-instructions-${P.value}`,
				class: "sr-only"
			}, [_("p", null, D(V.value.a11y.translations.keyboardNavigation), 1)], 8, Qe),
			zr.value.body.length ? (C(), m(Me, {
				key: 0,
				uid: P.value,
				head: zr.value.head,
				body: zr.value.body,
				caption: zr.value.caption,
				notice: zr.value.notice
			}, null, 8, [
				"uid",
				"head",
				"body",
				"caption",
				"notice"
			])) : h("", !0),
			E(e.$slots, "userConfig", {}, void 0, !0),
			V.value.userOptions.buttons.annotator ? (C(), m(O(zt), {
				key: 1,
				svgRef: O(W),
				backgroundColor: V.value.style.chart.backgroundColor,
				color: V.value.style.chart.color,
				active: xr.value,
				isCursorPointer: U.value,
				onClose: Sr
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
			])) : h("", !0),
			kn.value ? (C(), g("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Qt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : h("", !0),
			V.value.style.chart.title.text ? (C(), g("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Kt,
				class: "vue-ui-xy-title",
				style: S(`font-family:${V.value.style.fontFamily}`)
			}, [(C(), m(Ee, {
				key: `title_${qt.value}`,
				config: {
					title: {
						cy: "history-plot-div-title",
						...V.value.style.chart.title
					},
					subtitle: {
						cy: "history-plot-div-subtitle",
						...V.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 4)) : h("", !0),
			_("div", { id: `legend-top-${P.value}` }, null, 8, $e),
			V.value.userOptions.show && pn.value && (O(wn) || O(Sn)) ? (C(), m(O(Bt), {
				ref_key: "userOptionsRef",
				ref: an,
				key: `user_option_${Zt.value}`,
				backgroundColor: V.value.style.chart.backgroundColor,
				color: V.value.style.chart.color,
				isPrinting: O(Tn),
				isImaging: O(En),
				uid: P.value,
				hasTooltip: V.value.style.chart.tooltip.show && V.value.userOptions.buttons.tooltip,
				hasPdf: V.value.userOptions.buttons.pdf,
				hasImg: V.value.userOptions.buttons.img,
				hasSvg: V.value.userOptions.buttons.svg,
				hasXls: V.value.userOptions.buttons.csv,
				hasTable: V.value.userOptions.buttons.table,
				hasLabel: !1,
				hasFullscreen: V.value.userOptions.buttons.fullscreen,
				hasAltCopy: V.value.userOptions.buttons.altCopy,
				isFullscreen: I.value,
				chartElement: j.value,
				position: V.value.userOptions.position,
				isTooltip: G.value.showTooltip,
				titles: { ...V.value.userOptions.buttonTitles },
				hasAnnotator: V.value.userOptions.buttons.annotator,
				isAnnotation: xr.value,
				callbacks: V.value.userOptions.callbacks,
				printScale: V.value.userOptions.print.scale,
				tableDialog: V.value.table.useDialog,
				isCursorPointer: U.value,
				onToggleFullscreen: hr,
				onGeneratePdf: O(Dn),
				onGenerateCsv: br,
				onGenerateImage: O(Lr),
				onGenerateSvg: O(Ir),
				onToggleTable: Cr,
				onToggleTooltip: wr,
				onToggleAnnotator: Sr,
				onCopyAlt: Rr,
				style: S({ visibility: O(wn) ? O(Sn) ? "visible" : "hidden" : "visible" })
			}, Re({ _: 2 }, [
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
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: k(() => [E(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: k(() => [E(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: k(() => [E(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: k(() => [E(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: k(({ toggleFullscreen: t, isFullscreen: n }) => [E(e.$slots, "optionFullscreen", x(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: k(({ toggleAnnotator: t, isAnnotator: n }) => [E(e.$slots, "optionAnnotator", x(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: k(({ altCopy: t }) => [E(e.$slots, "optionAltCopy", x(y({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: k(() => [E(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: k(() => [E(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.chartElement.position.isTooltip.titles.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : h("", !0),
			_("div", et, [(C(), g("svg", {
				ref_key: "svgRef",
				ref: W,
				xmlns: O(le),
				viewBox: `0 0 ${K.value.width < 0 ? .1 : K.value.width} ${K.value.height < 0 ? .1 : K.value.height}`,
				style: S(`max-width:100%;overflow:visible;background:transparent;color:${V.value.style.chart.color}`),
				"aria-describedby": `chart-instructions-${P.value}`,
				tabindex: "0",
				onFocus: Hr,
				onBlur: Ur,
				onKeydown: Kr
			}, [
				Be(O(Vt)),
				e.$slots["chart-background"] ? (C(), g("foreignObject", {
					key: 0,
					x: q.value.left,
					y: q.value.top,
					width: q.value.width <= 0 ? 10 : q.value.width,
					height: q.value.height <= 0 ? 10 : q.value.height,
					style: { pointerEvents: "none" }
				}, [E(e.$slots, "chart-background", {}, void 0, !0)], 8, nt)) : h("", !0),
				V.value.style.chart.plots.gradient.show || Gn.value ? (C(), g("defs", rt, [V.value.style.chart.plots.gradient.show ? (C(!0), g(f, { key: 0 }, T(Z.value, (e, t) => (C(), m(Ae, {
					t: "radial",
					key: `gradient_${t}_${P.value}`,
					id: `gradient_${t}_${P.value}`,
					fy: "30%",
					stops: [
						[
							"10%",
							O(s)(e.color, V.value.style.chart.plots.gradient.intensity / 100),
							1
						],
						[
							"90%",
							O(ne)(e.color, .1),
							1
						],
						[
							"100%",
							e.color,
							1
						]
					]
				}, null, 8, ["id", "stops"]))), 128)) : h("", !0), (C(!0), g(f, null, T(Z.value, (e) => (C(), g(f, { key: `temperature_grad_history_template_${e.seriesIndex}_${P.value}` }, [V.value.style.chart.plots.gradient.show && qn(e) ? (C(!0), g(f, { key: 0 }, T(e.plots, (t) => (C(), m(Ae, {
					t: "radial",
					key: `temperature_plot_grad_history_${e.seriesIndex}_${t.id}_${P.value}`,
					id: $n(e, t),
					fy: "30%",
					stops: [
						[
							"10%",
							O(s)(Q(e, t), V.value.style.chart.plots.gradient.intensity / 100),
							1
						],
						[
							"90%",
							O(ne)(Q(e, t), .1),
							1
						],
						[
							"100%",
							Q(e, t),
							1
						]
					]
				}, null, 8, ["id", "stops"]))), 128)) : h("", !0), Kn(e) ? (C(), g("linearGradient", {
					key: 1,
					id: `temperature_grad_history_${e.seriesIndex}_${P.value}`,
					gradientUnits: "userSpaceOnUse",
					x1: Zn(e).x1,
					y1: Zn(e).y1,
					x2: Zn(e).x2,
					y2: Zn(e).y2
				}, [(C(!0), g(f, null, T(e.temperatureColors, (t, n) => (C(), g("stop", {
					key: `temperature_grad_history_stop_${e.seriesIndex}_${n}_${P.value}`,
					"stop-color": t,
					offset: e.temperatureColors.length === 1 ? "0%" : O(pe)(n, e.temperatureColors.length)
				}, null, 8, at))), 128))], 8, it)) : h("", !0)], 64))), 128))])) : h("", !0),
				V.value.style.chart.grid.verticalLines.show ? (C(), g("g", ot, [(C(!0), g(f, null, T(X.value.tickX, (e) => (C(), g("line", {
					x1: e.x,
					x2: e.x,
					y1: e.y1,
					y2: e.y2,
					stroke: V.value.style.chart.grid.verticalLines.stroke,
					"stroke-width": V.value.style.chart.grid.verticalLines.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, st))), 256))])) : h("", !0),
				V.value.style.chart.axes.y.labels.show ? (C(), g("g", ct, [(C(!0), g(f, null, T(X.value.tickY, (e) => (C(), g("line", {
					stroke: V.value.style.chart.grid.verticalLines.stroke,
					"stroke-width": V.value.style.chart.grid.verticalLines.strokeWidth,
					"stroke-linecap": "round",
					x1: q.value.left - 5,
					x2: q.value.left,
					y1: e.y,
					y2: e.y
				}, null, 8, lt))), 256))])) : h("", !0),
				V.value.style.chart.axes.y.labels.show ? (C(), g("g", {
					key: 4,
					ref_key: "yAxisScales",
					ref: ln
				}, [(C(!0), g(f, null, T(X.value.tickY, (e) => (C(), g("text", {
					x: q.value.left + V.value.style.chart.axes.y.labels.offsetX - 4 - V.value.style.chart.plots.radius,
					y: e.y + J.value.yAxisLabels / 3,
					fill: V.value.style.chart.axes.y.labels.color,
					"font-size": J.value.yAxisLabels,
					"text-anchor": "end"
				}, D(O(u)(V.value.style.chart.axes.y.labels.formatter, e.value, O(l)({
					p: V.value.style.chart.axes.y.labels.prefix,
					v: e.value,
					s: V.value.style.chart.axes.y.labels.suffix,
					r: V.value.style.chart.axes.y.labels.rounding
				}))), 9, ut))), 256))], 512)) : h("", !0),
				V.value.style.chart.axes.y.name.text ? (C(), g("text", {
					key: 5,
					ref_key: "yAxisLabel",
					ref: sn,
					transform: `translate(${J.value.yAxisName}, ${K.value.height / 2 + V.value.style.chart.axes.y.name.offsetY}), rotate(-90)`,
					"font-size": J.value.yAxisName,
					fill: V.value.style.chart.axes.y.name.color,
					"font-weight": V.value.style.chart.axes.y.name.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, D(V.value.style.chart.axes.y.name.text), 9, dt)) : h("", !0),
				V.value.style.chart.grid.horizontalLines.show ? (C(), g("g", ft, [(C(!0), g(f, null, T(X.value.tickY, (e) => (C(), g("line", {
					x1: e.x1,
					x2: e.x2,
					y1: e.y,
					y2: e.y,
					stroke: V.value.style.chart.grid.horizontalLines.stroke,
					"stroke-width": V.value.style.chart.grid.horizontalLines.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, pt))), 256))])) : h("", !0),
				V.value.style.chart.axes.x.labels.show ? (C(), g("g", mt, [(C(!0), g(f, null, T(X.value.tickX, (e) => (C(), g("line", {
					stroke: V.value.style.chart.grid.verticalLines.stroke,
					"stroke-width": V.value.style.chart.grid.verticalLines.strokeWidth,
					"stroke-linecap": "round",
					x1: e.x,
					x2: e.x,
					y1: q.value.bottom,
					y2: q.value.bottom + 5
				}, null, 8, ht))), 256))])) : h("", !0),
				V.value.style.chart.axes.x.labels.show ? (C(), g("g", {
					key: 8,
					ref_key: "xAxisScales",
					ref: cn
				}, [(C(!0), g(f, null, T(X.value.tickX, (e) => (C(), g("text", {
					class: "vue-ui-history-plot-x-axis-scale",
					transform: `translate(${e.x}, ${q.value.bottom + V.value.style.chart.axes.x.labels.offsetY + J.value.xAxisLabels + V.value.style.chart.plots.radius}), rotate(${V.value.style.chart.axes.x.labels.rotation})`,
					fill: V.value.style.chart.axes.x.labels.color,
					"font-size": J.value.xAxisLabels,
					"text-anchor": V.value.style.chart.axes.x.labels.rotation > 0 ? "start" : V.value.style.chart.axes.x.labels.rotation < 0 ? "end" : "middle"
				}, D(O(u)(V.value.style.chart.axes.x.labels.formatter, e.value, O(l)({
					p: V.value.style.chart.axes.x.labels.prefix,
					v: e.value,
					s: V.value.style.chart.axes.x.labels.suffix,
					r: V.value.style.chart.axes.x.labels.rounding
				}))), 9, gt))), 256))], 512)) : h("", !0),
				V.value.style.chart.axes.x.name.text ? (C(), g("text", {
					key: 9,
					ref_key: "xAxisLabel",
					ref: on,
					x: K.value.width / 2 + V.value.style.chart.axes.x.name.offsetX,
					y: K.value.height,
					"font-size": J.value.xAxisName,
					fill: V.value.style.chart.axes.x.name.color,
					"font-weight": V.value.style.chart.axes.x.name.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, D(V.value.style.chart.axes.x.name.text), 9, _t)) : h("", !0),
				V.value.style.chart.grid.xAxis.show ? (C(), g("line", {
					key: 10,
					x1: q.value.left,
					x2: q.value.left + q.value.width,
					y1: q.value.bottom,
					y2: q.value.bottom,
					stroke: V.value.style.chart.grid.xAxis.stroke,
					"stroke-width": V.value.style.chart.grid.xAxis.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, vt)) : h("", !0),
				V.value.style.chart.grid.yAxis.show ? (C(), g("line", {
					key: 11,
					x1: q.value.left,
					x2: q.value.left,
					y1: q.value.top,
					y2: q.value.bottom,
					stroke: V.value.style.chart.grid.yAxis.stroke,
					"stroke-width": V.value.style.chart.grid.yAxis.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, yt)) : h("", !0),
				(C(!0), g(f, null, T(Wn.value, (e) => (C(), g("g", { key: `history_plot_serie_${e.seriesIndex}_${P.value}` }, [
					V.value.style.chart.paths.show ? (C(), g("g", {
						key: 0,
						style: S({ opacity: ar(e.seriesIndex) })
					}, [_("path", {
						class: b({ "vue-data-ui-transition": O(H) }),
						d: e.path,
						stroke: V.value.style.chart.backgroundColor,
						"stroke-width": V.value.style.chart.paths.strokeWidth + 4,
						fill: "none",
						"stroke-linecap": "round",
						"stroke-linejoin": "round"
					}, null, 10, bt), _("path", {
						class: b({ "vue-data-ui-transition": O(H) }),
						d: e.path,
						stroke: ir(e),
						"stroke-width": V.value.style.chart.paths.strokeWidth,
						fill: "none",
						"stroke-linecap": "round",
						"stroke-linejoin": "round"
					}, null, 10, xt)], 4)) : h("", !0),
					(C(!0), g(f, null, T(e.plots, (e) => (C(), g("circle", {
						cx: e.x,
						cy: e.y,
						fill: V.value.style.chart.backgroundColor,
						r: J.value.plots,
						stroke: "none",
						class: b({ "vue-data-ui-transition": O(H) })
					}, null, 10, St))), 256)),
					(C(!0), g(f, null, T(e.plots, (t) => (C(), g("circle", {
						cx: t.x,
						cy: t.y,
						fill: er(e, t, e.gradientIndex),
						r: J.value.plots,
						stroke: V.value.style.chart.plots.stroke,
						"stroke-width": V.value.style.chart.plots.strokeWidth,
						class: b({ "vue-data-ui-transition": O(H) }),
						style: S({ opacity: ar(e.seriesIndex) })
					}, null, 14, Ct))), 256)),
					V.value.style.chart.plots.labels.show ? (C(), g("g", wt, [(C(!0), g(f, null, T(e.plots, (t, n) => (C(), g("g", { key: `plab_${n}` }, [String(t.label).includes("\n") ? (C(), g("text", {
						key: 1,
						class: b({ "vue-data-ui-transition": O(H) }),
						transform: `translate(${t.x + V.value.style.chart.plots.labels.offsetX}, ${t.y + V.value.style.chart.plots.labels.offsetY + J.value.plots + J.value.labels})`,
						"font-size": J.value.labels,
						fill: V.value.style.chart.plots.labels.color,
						"font-weight": V.value.style.chart.plots.labels.bold ? "bold" : "normal",
						"text-anchor": "middle",
						style: S({ opacity: ar(e.seriesIndex) }),
						innerHTML: O(r)({
							content: String(t.label),
							fontSize: J.value.labels,
							fill: V.value.style.chart.plots.labels.color,
							x: 0,
							y: 0
						})
					}, null, 14, Et)) : (C(), g("text", {
						key: 0,
						class: b({ "vue-data-ui-transition": O(H) }),
						transform: `translate(${t.x + V.value.style.chart.plots.labels.offsetX}, ${t.y + V.value.style.chart.plots.labels.offsetY + J.value.plots + J.value.labels})`,
						"font-size": J.value.labels,
						fill: V.value.style.chart.plots.labels.color,
						"font-weight": V.value.style.chart.plots.labels.bold ? "bold" : "normal",
						"text-anchor": "middle",
						style: S({ opacity: ar(e.seriesIndex) })
					}, D(t.label), 15, Tt))]))), 128))])) : h("", !0),
					V.value.style.chart.plots.indexLabels.show ? (C(), g("g", Dt, [(C(!0), g(f, null, T(e.plots, (t, n) => (C(), g("text", {
						key: `lab_${n}`,
						class: b({ "vue-data-ui-transition": O(H) }),
						transform: `translate(${t.x + V.value.style.chart.plots.indexLabels.offsetX}, ${t.y + V.value.style.chart.plots.indexLabels.offsetY + J.value.indexLabels / 3})`,
						"font-size": J.value.indexLabels,
						"font-weight": V.value.style.chart.plots.indexLabels.bold ? "bold" : "normal",
						fill: nr(e, t),
						"text-anchor": "middle",
						style: S({ opacity: ar(e.seriesIndex) })
					}, D(V.value.style.chart.plots.indexLabels.startAtZero ? n : n + 1), 15, Ot))), 128))])) : h("", !0)
				]))), 128)),
				(C(!0), g(f, null, T(Wn.value, (e) => (C(), g("g", { key: `history_plot_trap_serie_${e.seriesIndex}_${P.value}` }, [(C(!0), g(f, null, T(e.plots, (t, n) => (C(), g("circle", {
					cx: t.x,
					cy: t.y,
					fill: "transparent",
					r: J.value.plots,
					stroke: "none",
					onMouseenter: (r) => yr({
						datapoint: t,
						plotIndex: n,
						seriesIndex: e.seriesIndex,
						triggerMode: "pointer"
					}),
					onMouseleave: (r) => _r({
						datapoint: t,
						plotIndex: n,
						seriesIndex: e.seriesIndex
					}),
					onClick: (r) => mr({
						datapoint: t,
						plotIndex: n,
						seriesIndex: e.seriesIndex
					})
				}, null, 40, kt))), 256))]))), 128)),
				E(e.$slots, "svg", { svg: {
					...K.value,
					drawingArea: q.value,
					isPrintingImg: O(Tn) || O(En) || O(Pr),
					isPrintingSvg: O(Fr)
				} }, void 0, !0)
			], 44, tt)), e.$slots.hint ? (C(), g("div", At, [E(e.$slots, "hint", x(y({
				hint: V.value.a11y.translations.keyboardNavigation,
				isVisible: fn.value
			})), void 0, !0)])) : h("", !0)]),
			e.$slots.watermark ? (C(), g("div", jt, [E(e.$slots, "watermark", x(y({ isPrinting: O(Tn) || O(En) || O(Pr) || O(Fr) })), void 0, !0)])) : h("", !0),
			_("div", { id: `legend-bottom-${P.value}` }, null, 8, Mt),
			nn.value && (V.value.style.chart.legend.show || e.$slots.legend) ? (C(), m(Le, {
				key: 6,
				to: V.value.style.chart.legend.position === "top" ? `#legend-top-${P.value}` : `#legend-bottom-${P.value}`
			}, [_("div", {
				ref_key: "chartLegend",
				ref: Jt
			}, [E(e.$slots, "legend", { legend: fr.value }, () => [V.value.style.chart.legend.show && pn.value ? (C(), m(Fe, {
				key: `legend_${Yt.value}`,
				legendSet: fr.value,
				config: pr.value,
				isCursorPointer: U.value,
				onClickMarker: t[0] ||= ({ legend: e }) => {
					cr(e.seriesIndex);
				}
			}, {
				item: k(({ legend: e, index: t }) => [_("div", {
					onClick: (t) => e.segregate(),
					style: S(`opacity:${F.value.includes(e.seriesIndex) ? .5 : 1}`)
				}, D(e.name), 13, Nt)]),
				legendToggle: k(() => [fr.value.length > 2 && V.value.style.chart.legend.selectAllToggle.show && !O(yn) ? (C(), m(je, {
					key: 0,
					backgroundColor: V.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: V.value.style.chart.legend.selectAllToggle.color,
					fontSize: V.value.style.chart.legend.fontSize,
					checked: F.value.length > 0,
					isCursorPointer: U.value,
					onToggle: sr
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : h("", !0)]),
				_: 1
			}, 8, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : h("", !0)], !0)], 512)], 8, ["to"])) : h("", !0),
			e.$slots.source ? (C(), g("div", {
				key: 7,
				ref_key: "source",
				ref: tn,
				dir: "auto"
			}, [E(e.$slots, "source", {}, void 0, !0)], 512)) : h("", !0),
			Be(O(Ft), {
				teleportTo: V.value.style.chart.tooltip.teleportTo,
				show: G.value.showTooltip && $t.value,
				backgroundColor: V.value.style.chart.tooltip.backgroundColor,
				color: V.value.style.chart.tooltip.color,
				fontSize: V.value.style.chart.tooltip.fontSize,
				borderRadius: V.value.style.chart.tooltip.borderRadius,
				borderColor: V.value.style.chart.tooltip.borderColor,
				borderWidth: V.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: V.value.style.chart.tooltip.backgroundOpacity,
				position: V.value.style.chart.tooltip.position,
				offsetX: V.value.style.chart.tooltip.offsetX,
				offsetY: V.value.style.chart.tooltip.offsetY,
				parent: j.value,
				content: en.value,
				isCustom: R.value,
				isFullscreen: I.value,
				smooth: V.value.style.chart.tooltip.smooth,
				backdropFilter: V.value.style.chart.tooltip.backdropFilter,
				smoothForce: V.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: V.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: dn.value === "keyboard",
				a11yPosition: un.value
			}, {
				"tooltip-before": k(() => [E(e.$slots, "tooltip-before", x(y({ ...gr.value })), void 0, !0)]),
				tooltip: k(() => [E(e.$slots, "tooltip", x(y({ ...gr.value })), void 0, !0)]),
				"tooltip-after": k(() => [E(e.$slots, "tooltip-after", x(y({ ...gr.value })), void 0, !0)]),
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
				"isCustom",
				"isFullscreen",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			pn.value && V.value.userOptions.buttons.table ? (C(), m(Ge(kr.value.component), Ve({ key: 8 }, kr.value.props, {
				ref_key: "tableUnit",
				ref: rn,
				onClose: Ar
			}), Re({
				content: k(() => [(C(), m(O(Rt), {
					key: `table_${Xt.value}`,
					colNames: $.value.colNames,
					head: $.value.head,
					body: $.value.body,
					config: $.value.config,
					title: V.value.table.useDialog ? "" : kr.value.title,
					withCloseButton: !V.value.table.useDialog,
					isCursorPointer: U.value,
					onClose: Ar
				}, {
					th: k(({ th: e }) => [_("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, Pt)]),
					td: k(({ td: e }) => [ze(D(e.name || e), 1)]),
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
			}, [V.value.table.useDialog ? {
				name: "title",
				fn: k(() => [ze(D(kr.value.title), 1)]),
				key: "0"
			} : void 0, V.value.table.useDialog ? {
				name: "actions",
				fn: k(() => [_("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => br(V.value.userOptions.callbacks.csv),
					style: S({ cursor: U.value ? "pointer" : "default" })
				}, [Be(O(It), {
					name: "fileCsv",
					stroke: kr.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : h("", !0),
			E(e.$slots, "skeleton", {}, () => [O(yn) ? (C(), m(ye, { key: 0 })) : h("", !0)], !0)
		], 46, Ze));
	}
}, [["__scopeId", "data-v-03e4e523"]]);
//#endregion
export { Xe as n, Ft as t };
