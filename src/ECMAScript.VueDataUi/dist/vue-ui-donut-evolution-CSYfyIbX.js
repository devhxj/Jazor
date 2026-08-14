import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, G as i, Jt as a, Kt as o, Pt as s, Q as c, Rt as ee, S as te, Ut as ne, X as re, _ as ie, f as ae, h as oe, i as se, jt as ce, kt as le, p as ue, pt as de, q as fe, t as pe, tt as me, w as he, wt as ge, y as _e } from "./lib-Bttd6u5E.js";
import { n as ve, t as ye } from "./useHints-Dq_w2E8B.js";
import { t as be } from "./useTimeLabels-d2f-W1L4.js";
import { t as xe } from "./useConfig-DlNpz6P8.js";
import { t as Se } from "./usePrinter-DN5bYhTG.js";
import { n as Ce, t as we } from "./BaseScanner-DZvpgOjM.js";
import { t as Te } from "./useNestedProp-vPNvh7rV.js";
import { t as Ee } from "./useThemeCheck-C43Tcqmk.js";
import { t as De } from "./useChartExport-DNiwdPmb.js";
import { t as Oe } from "./useTransitions-g_zBREk2.js";
import { t as ke } from "./useStableElementSize-C7KADDKj.js";
import { t as Ae } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as je } from "./img-Bnokohej.js";
import { n as Me } from "./Title-BE3qg9xl.js";
import { t as Ne } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Pe, t as Fe } from "./useResponsive-ZtArZtUf.js";
import { t as Ie } from "./DefGrad-DVBqDjhO.js";
import { t as Le } from "./SlicerPreview-wUw1hFwe.js";
import { t as Re } from "./BaseLegendToggle-DZVucLnv.js";
import { t as ze } from "./A11yDataTable-DdRsVULz.js";
import { t as Be } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ve } from "./useChartAccessibility-DYqac8yF.js";
import { t as He } from "./Legend-CQxUgOd-.js";
import { t as Ue } from "./vue_ui_donut_evolution-D1yAAIHr.js";
import { Fragment as l, Teleport as We, computed as u, createBlock as d, createCommentVNode as f, createElementBlock as p, createElementVNode as m, createSlots as Ge, createTextVNode as Ke, createVNode as qe, defineAsyncComponent as h, guardReactiveProps as g, mergeProps as Je, nextTick as Ye, normalizeClass as _, normalizeProps as v, normalizeStyle as y, onBeforeUnmount as Xe, onMounted as Ze, openBlock as b, ref as x, renderList as S, renderSlot as C, resolveDynamicComponent as Qe, shallowRef as $e, toDisplayString as w, toRefs as et, unref as T, watch as tt, watchEffect as nt, withCtx as E } from "vue";
//#region src/components/vue-ui-donut-evolution.vue
var rt = /* @__PURE__ */ e({ default: () => Gt }), it = ["id"], at = ["id"], ot = ["id"], st = { style: { position: "relative" } }, ct = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], lt = [
	"x",
	"y",
	"width",
	"height"
], ut = { key: 1 }, dt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], ft = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], pt = { key: 0 }, mt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], ht = [
	"font-size",
	"fill",
	"transform"
], gt = [
	"x",
	"y",
	"font-size",
	"fill"
], _t = [
	"d",
	"stroke",
	"stroke-width"
], vt = [
	"transform",
	"font-size",
	"text-anchor",
	"fill",
	"font-weight"
], yt = { key: 0 }, bt = [
	"text-anchor",
	"font-size",
	"fill",
	"transform"
], xt = [
	"text-anchor",
	"font-size",
	"fill",
	"transform",
	"innerHTML"
], St = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], Ct = { key: 1 }, wt = [
	"cx",
	"cy",
	"r",
	"fill"
], Tt = { key: 0 }, Et = { key: 0 }, Dt = ["d", "stroke"], Ot = [
	"text-anchor",
	"x",
	"y",
	"fill"
], kt = [
	"cx",
	"cy",
	"r",
	"fill"
], At = { key: 0 }, jt = [
	"cx",
	"cy",
	"fill"
], Mt = { key: 1 }, Nt = [
	"d",
	"fill",
	"stroke"
], Pt = { key: 2 }, Ft = [
	"d",
	"fill",
	"stroke"
], It = [
	"x",
	"y",
	"font-size",
	"fill"
], Lt = [
	"x",
	"y",
	"width",
	"height",
	"fill"
], Rt = [
	"x",
	"y",
	"width",
	"height",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], zt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, Bt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Vt = ["id"], Ht = ["onClick"], Ut = { key: 0 }, Wt = { key: 1 }, Gt = /*#__PURE__*/ Ne({
	__name: "vue-ui-donut-evolution",
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
	emits: ["selectLegend", "copyAlt"],
	setup(e, { expose: Ne, emit: rt }) {
		let Gt = h(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Kt = h(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), qt = h(() => import("./DataTable-BbKgJ5UI.js")), Jt = h(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Yt = h(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Xt = h(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Zt = h(() => import("./vue-ui-donut-8RB-gL2J.js").then((e) => e.n)), Qt = h(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_donut_evolution: $t } = xe(), { isThemeValid: en, warnInvalidTheme: tn } = Ee(), D = e, O = x(fe()), k = x([]), A = x(null), j = x(null), nn = x(!1), M = x(null), N = x(null), rn = x(null), an = x(0), on = x(0), sn = x(0), cn = x(0), ln = x(0), un = x(null), dn = x(null), fn = x(null), pn = x(null), mn = x(!1), hn = x(null), gn = x(null), _n = $e(null), vn = x(0), yn = x(0), bn = x(!1);
		function xn() {
			_n.value = N.value?.parentNode ?? null;
		}
		function Sn() {
			return new Promise((e) => {
				requestAnimationFrame(() => {
					requestAnimationFrame(e);
				});
			});
		}
		async function Cn() {
			let e = ++yn.value;
			await Ye(), await Sn(), await Sn(), e === yn.value && (vn.value += 1);
		}
		function wn() {
			bn.value || (bn.value = !0, Ye(() => {
				bn.value = !1, xn(), Cn();
			}));
		}
		let Tn = ke({
			elementRef: _n,
			minimumWidth: 2,
			minimumHeight: 2,
			stableFramesRequired: 2,
			once: !1,
			onSizeAccepted: () => {
				Cn();
			}
		}), En = x(null), Dn = x(null), On = x(null), kn = x(null), An = x(null), jn = x(!1), P = $e(null), F = $e(null), Mn = x(null), I = x(null), Nn = x(!1), Pn = rt, Fn = u(() => !!D.dataset && D.dataset.length);
		Ze(() => {
			mn.value = !0, xn(), Tn.start(), Ln(), Cn();
		});
		let In = u(() => L.value.debug);
		function Ln() {
			if (ce(D.dataset) ? (me({
				componentName: "VueUiDonutEvolution",
				type: "dataset",
				debug: In.value
			}), Hn.value = !0) : D.dataset.length && D.dataset.forEach((e, t) => {
				de({
					datasetObject: e,
					requiredAttributes: ["name", "values"]
				}).forEach((e) => {
					me({
						componentName: "VueUiDonutEvolution",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: In.value
					}), Hn.value = !0;
				});
			}), ce(D.dataset) || (Hn.value = L.value.loading), setTimeout(() => {
				jn.value = !0;
			}, 10), Gn(), L.value.responsive) {
				let e = Pe(() => {
					jn.value = !1;
					let { width: e, height: t } = Fe({
						chart: N.value,
						title: L.value.style.chart.title.text ? En.value : null,
						legend: L.value.style.chart.legend.show ? Dn.value : null,
						slicer: L.value.style.chart.zoom.show && Un.value > 1 ? An.value : null,
						source: On.value
					});
					requestAnimationFrame(() => {
						z.value.width = e, z.value.height = t - 12, wn(), clearTimeout(Mn.value), Mn.value = setTimeout(() => {
							jn.value = !0;
						}, 10);
					});
				});
				P.value && (F.value && P.value.unobserve(F.value), P.value.disconnect()), P.value = new ResizeObserver(e), F.value = N.value.parentNode, P.value.observe(F.value);
			}
			wn();
		}
		let L = x(Xn());
		ve({
			config: () => L.value,
			dataset: () => D.dataset,
			component: "VueUiDonutEvolution",
			rules: [
				ye.emptyArray,
				{
					test: (e) => e.length === 1,
					message: [
						"👀 There is only a single series in your dataset. Consider:",
						"",
						"▶️ Using VueUiXy instead, for a regular line chart."
					]
				},
				{
					test: (e) => e.length > 6,
					message: [
						"👀 The number of series is > 6, which makes donuts hard to read. Consider:",
						"",
						"▶️ Grouping small values dynamically into a single \"Other\" series.",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display.",
						"",
						"▶️ Using VueUiHorizontalBar instead to make the dataset breakdown easier to read."
					]
				},
				{
					test: (e) => e.some((e) => e.values.length > 24),
					message: [
						"👀 Some series have a number of data points > 24, which can make the chart and donuts hard to read. Consider:",
						"",
						"▶️ Using larger time scales, or aggregated values.",
						"",
						"▶️ Filtering the time range by adding date intputs in your UI.",
						"",
						"▶️ Using VueUiXy to show longer series in a more comfortable display."
					]
				}
			]
		});
		let { transitionEnabled: Rn } = Oe({
			config: () => L.value.transitions,
			dataset: () => D.dataset
		}), R = u(() => L.value.userOptions.useCursorPointer), zn = u(() => a({
			defaultConfig: {
				useCssAnimation: !1,
				table: { show: !1 },
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						dataLabels: { show: !1 },
						grid: {
							stroke: "#6A6A6A",
							axis: {
								yLabel: "",
								xLabel: ""
							},
							yAxis: {
								scaleMin: null,
								scaleMax: null,
								autoScale: !1,
								dataLabels: { show: !1 }
							},
							xAxis: { dataLabels: { show: !1 } }
						},
						line: { stroke: "#CACACA60" }
					},
					legend: {
						backgroundColor: "transparent",
						showValue: !1,
						showPercentage: !1
					},
					zoom: {
						show: !1,
						startIndex: null,
						endIndex: null
					}
				} }
			},
			userConfig: L.value.skeletonConfig ?? {}
		})), { loading: Bn, FINAL_DATASET: Vn, manualLoading: Hn } = Ce({
			...et(D),
			FINAL_CONFIG: L,
			prepareConfig: Xn,
			callback: () => {
				Promise.resolve().then(async () => {
					await Gn();
				});
			},
			skeletonDataset: D.config?.skeletonDataset ?? [
				{
					name: "",
					values: [
						1,
						2,
						3,
						5,
						8,
						13
					],
					color: "#AAAAAA"
				},
				{
					name: "",
					values: [
						1,
						2,
						3,
						5,
						8,
						13
					],
					color: "#BABABA"
				},
				{
					name: "",
					values: [
						1,
						2,
						3,
						5,
						8,
						13
					],
					color: "#CACACA"
				}
			],
			skeletonConfig: a({
				defaultConfig: L.value,
				userConfig: zn.value
			})
		}), z = x({
			width: L.value.style.chart.layout.width,
			height: L.value.style.chart.layout.height
		}), Un = u(() => Math.max(...Vn.value.map((e) => e.values.length))), B = x({
			start: 0,
			end: Un.value
		});
		function Wn() {
			Gn();
		}
		async function Gn() {
			await Ye(), await Ye();
			let { startIndex: e, endIndex: t } = L.value.style.chart.zoom, n = kn.value;
			B.value = {
				start: 0,
				end: q.value
			}, (e != null || t != null) && n ? (e == null ? (B.value.start = 0, n.setStartValue(0)) : n.setStartValue(e), t == null ? (B.value.end = q.value, n.setEndValue(q.value)) : n.setEndValue(Kn(t + 1))) : (B.value = {
				start: 0,
				end: q.value
			}, on.value += 1), wn();
		}
		function Kn(e) {
			let t = q.value;
			return e > t ? t : e < 0 || L.value.style.chart.zoom.startIndex !== null && e < L.value.style.chart.zoom.startIndex ? L.value.style.chart.zoom.startIndex === null ? 1 : L.value.style.chart.zoom.startIndex + 1 : e;
		}
		let { userOptionsVisible: qn, setUserOptionsVisibility: Jn, keepUserOptionState: Yn } = Be({ config: L.value }), { svgRef: V } = Ve({ config: L.value.style.chart.title });
		function Xn() {
			let e = Te({
				userConfig: D.config,
				defaultConfig: $t
			}), t = {}, n = e.theme;
			if (n) if (!en.value(e)) tn(e), t = e;
			else {
				let r = Te({
					userConfig: Ue[n] || D.config,
					defaultConfig: e
				});
				t = {
					...Te({
						userConfig: D.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette ? e.customPalette : o[n] || s
				};
			}
			else t = e;
			return t;
		}
		tt(() => D.config, (e) => {
			Bn.value || (L.value = Xn()), qn.value = !L.value.userOptions.showOnChartHover, Ln(), sn.value += 1, cn.value += 1, ln.value += 1, H.value.showTable = L.value.table.show;
		}, { deep: !0 }), tt(() => D.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Hn.value = !1), Wn();
		}, { deep: !0 });
		let { isPrinting: Zn, isImaging: Qn, generatePdf: $n, generateImage: er } = Se({
			elementId: O.value,
			fileName: L.value.style.chart.title.text || "vue-ui-donut-evolution",
			options: L.value.userOptions.print
		}), tr = u(() => L.value.userOptions.show && !L.value.style.chart.title.text), nr = u(() => he(L.value.customPalette)), H = x({ showTable: L.value.table.show });
		tt(L, () => {
			H.value = { showTable: L.value.table.show };
		}, { immediate: !0 });
		let U = u(() => ({
			top: L.value.style.chart.layout.padding.top,
			right: L.value.style.chart.layout.padding.right,
			bottom: L.value.style.chart.layout.padding.bottom,
			left: L.value.style.chart.layout.padding.left
		})), W = u(() => L.value.style.chart.layout.grid.yAxis.position === "right");
		function rr() {
			let e = 0;
			un.value && (e = Array.from(un.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = pn.value ? pn.value.getBoundingClientRect().width + L.value.style.chart.layout.grid.axis.yLabelOffsetX + L.value.style.chart.layout.grid.axis.fontSize : 0, n = e + t + 5;
			return {
				left: W.value ? 0 : n,
				right: W.value ? n : 0,
				scaleLabelsWidth: e,
				yAxisLabelWidth: t,
				crosshair: 5
			};
		}
		let ir = x(0), ar = Pe((e) => {
			ir.value = e;
		}, 100);
		nt((e) => {
			let t = dn.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				ar(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), Xe(() => {
			ir.value = 0, Tn.stop(), clearTimeout(Mn.value), P.value && (F.value && P.value.unobserve(F.value), P.value.disconnect(), P.value = null, F.value = null);
		});
		let or = u(() => {
			let e = 0;
			fn.value && (e = fn.value.getBBox().height);
			let t = 0;
			return dn.value && (t = ir.value), e + t + L.value.style.chart.layout.grid.axis.fontSize + L.value.style.chart.layout.grid.xAxis.dataLabels.offsetY;
		}), G = u(() => {
			vn.value;
			let e = rr(), t = L.value.style.chart.layout.dataLabels.fontSize * 3, n = z.value.width, r = z.value.height, i = U.value.left + e.left, a = n - U.value.right - e.right, o = n - i - U.value.right - e.right, s = r - U.value.top - U.value.bottom - t - or.value, c = U.value.top + t;
			return {
				top: c,
				left: i,
				right: a,
				bottom: r - U.value.bottom - or.value,
				absoluteHeight: r,
				absoluteWidth: n,
				centerX: i + Math.max(10, o) / 2,
				centerY: c + Math.max(10, s) / 2,
				width: Math.max(10, o),
				height: Math.max(10, s),
				scaleLabelsWidth: e.scaleLabelsWidth,
				yAxisLabelWidth: e.yAxisLabelWidth,
				crosshair: e.crosshair
			};
		}), K = u(() => (In.value && Vn.value.forEach((e, t) => {
			[null, void 0].includes(e.name) && me({
				componentName: "VueUiDonutEvolution",
				type: "datasetSerieAttribute",
				property: "name",
				index: t
			}), [null, void 0].includes(e.values) && me({
				componentName: "VueUiDonutEvolution",
				type: "datasetSerieAttribute",
				property: "values",
				index: t
			});
		}), Vn.value.map((e, t) => ({
			...e,
			values: ee(e.values),
			color: te(e.color) || nr.value[t] || s[t] || s[t % s.length],
			length: (e.values || []).length,
			uid: fe()
		})))), sr = u(() => K.value.filter((e) => !k.value.includes(e.uid)).map((e) => ({
			...e,
			values: e.values.filter((e, t) => t >= B.value.start && t <= B.value.end)
		}))), q = u(() => Math.max(...K.value.map((e) => e.length))), J = x([]), cr = 0;
		nt(() => {
			let e = ++cr;
			(async () => {
				let t = await be({
					values: L.value.style.chart.layout.grid.xAxis.dataLabels.values,
					maxDatapoints: q.value,
					formatter: L.value.style.chart.layout.grid.xAxis.dataLabels.datetimeFormatter,
					start: B.value.start,
					end: B.value.end
				});
				e === cr && (J.value = t);
			})();
		});
		let Y = u(() => G.value.width / (B.value.end - B.value.start)), lr = u(() => {
			let e = [];
			for (let t = 0; t < B.value.end - B.value.start; t += 1) {
				let n = sr.value.map((e) => e.values[t] ?? null), r = n.filter((e) => [void 0, null].includes(e)).length === n.length, i = n.reduce((e, t) => e + t, 0), a = n.map((e) => e / i), o = G.value.left + Y.value * t + Y.value / 2;
				e.push({
					index: t,
					percentages: a,
					subtotal: r || i < 0 ? null : i,
					values: n,
					x: o
				});
			}
			return e;
		}), ur = u(() => {
			let e = Math.max(...lr.value.map((e) => e.subtotal).filter((e) => ge(e))) ?? 1, t = Math.min(...lr.value.map((e) => e.subtotal).filter((e) => ge(e))) ?? 0, n = Math.max(L.value.style.chart.layout.grid.yAxis.scaleMax ?? 0, e), r;
			return r = L.value.style.chart.layout.grid.yAxis.scaleMin == null ? 0 : Math.min(t, L.value.style.chart.layout.grid.yAxis.scaleMin), L.value.style.chart.layout.grid.yAxis.autoScale && (r = t, n = e), r === n && (r = n / 2, n *= 1.5), {
				max: n,
				min: r
			};
		}), X = u(() => {
			let e = lr.value.length === 1 ? ur.value.max * 2 : ur.value.max;
			return ie(ur.value.min, e, L.value.style.chart.layout.grid.yAxis.dataLabels.steps);
		});
		tt(() => X.value.ticks.join("|"), () => {
			wn();
		}, { flush: "post" });
		function dr(e) {
			return (e - X.value.min) / (X.value.max - X.value.min);
		}
		let fr = u(() => X.value.ticks.map((e) => ({
			y: G.value.bottom - G.value.height * dr(e),
			value: e
		}))), Z = u(() => {
			let e = lr.value, t = Math.max(...e.map((e) => e.subtotal));
			return e.length === 1 && t * 2, e.map((t, n) => {
				let r = Math.min(G.value.width / 24, Y.value / 2 * .7), i = r > G.value.width / 16 ? G.value.width / 16 : r, a = A.value === t.index ? G.value.width / 16 : i, o = e.length > 4 ? r * 2 : r * 2 > Y.value / 2 * .7 ? Y.value / 2 * .7 : r * 2, s = G.value.bottom - G.value.height * dr(t.subtotal), c = sr.value.map((e) => ({
					color: e.color,
					name: e.name,
					value: e.values[n] ?? 0
				})).toSorted((e, t) => t.value - e.value);
				return {
					...t,
					y: s,
					radius: i,
					activeRadius: a,
					hoverRadius: o,
					donut: le({ series: c }, t.x, s, i, i, 1.99999, 2, 1, 360, 105.25, i / 2),
					donutHover: le({ series: c }, t.x, s, o, o, 1.99999, 2, 1, 360, 105.25, o / 2),
					donutFocus: le({ series: c }, G.value.centerX, G.value.centerY, G.value.height / 3.6, G.value.height / 3.6, 1.99999, 2, 1, 360, 105.25, G.value.height / 6)
				};
			});
		});
		function pr(e, t, n) {
			return se(L.value.style.chart.layout.dataLabels.formatter, e, re({
				p: L.value.style.chart.layout.dataLabels.prefix,
				v: e,
				s: L.value.style.chart.layout.dataLabels.suffix,
				r: L.value.style.chart.layout.dataLabels.rounding
			}), {
				datapoint: t,
				index: n
			});
		}
		function mr(e, t) {
			return isNaN(e.value / ne(t, "value")) ? 0 : (e.value / ne(t, "value") * 100).toFixed(0) + "%";
		}
		function hr(e) {
			A.value = null, j.value = null, I.value = null, L.value.events.datapointLeave && L.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: e.seriesIndex + B.value.start
			});
		}
		function gr(e) {
			L.value.events.datapointEnter && L.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: e.index + B.value.start
			}), A.value = e.index, j.value = e, I.value = e.index;
		}
		let _r = x(null);
		function vr(e, t) {
			L.value.events.datapointClick && L.value.events.datapointClick({
				datapoint: e,
				seriesIndex: e.index + B.value.start
			}), !(!e.subtotal || !L.value.style.chart.dialog.show) && (j.value = null, A.value = null, nn.value = !0, M.value = e, Lr(e), [null, void 0].includes(t) || (_r.value = t));
		}
		let yr = u(() => K.value.map((e, t) => ({
			name: e.name,
			value: e.values.slice(B.value.start, B.value.end).reduce((e, t) => e + t, 0),
			shape: "circle",
			uid: e.uid,
			color: e.color
		})).sort((e, t) => t.value - e.value).map((e, t) => ({
			...e,
			opacity: k.value.includes(e.uid) ? .5 : 1,
			segregate: () => Cr(e.uid),
			isSegregated: k.value.includes(e.uid),
			display: `${e.name}${L.value.style.chart.legend.showPercentage || L.value.style.chart.legend.showValue ? ": " : ""}${L.value.style.chart.legend.showValue ? se(L.value.style.chart.layout.dataLabels.formatter, e.value, re({
				p: L.value.style.chart.layout.dataLabels.prefix,
				v: e.value,
				s: L.value.style.chart.layout.dataLabels.suffix,
				r: L.value.style.chart.legend.roundingValue
			}), {
				datapoint: e,
				seriesIndex: t
			}) : ""}${L.value.style.chart.legend.showPercentage ? k.value.includes(e.uid) ? `${L.value.style.chart.legend.showValue ? " (" : ""}- %${L.value.style.chart.legend.showValue ? ")" : ""}` : `${L.value.style.chart.legend.showValue ? " (" : ""}${isNaN(e.value / br.value) ? "-" : re({
				v: e.value / br.value * 100,
				s: "%",
				r: L.value.style.chart.legend.roundingPercentage
			})}${L.value.style.chart.legend.showValue ? ")" : ""}` : ""}`
		}))), br = u(() => Z.value.map((e) => e.subtotal).reduce((e, t) => e + t, 0)), xr = u(() => ({
			cy: "donut-div-legend",
			backgroundColor: L.value.style.chart.legend.backgroundColor,
			color: L.value.style.chart.legend.color,
			fontSize: L.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: L.value.style.chart.legend.bold ? "bold" : ""
		}));
		function Sr() {
			k.value.length ? k.value = [] : yr.value.forEach((e) => {
				k.value.push(e.uid);
			}), Pn("selectLegend", sr.value);
		}
		function Cr(e) {
			if (k.value.includes(e)) k.value = k.value.filter((t) => t !== e), Pn("selectLegend", sr.value);
			else {
				if (k.value.length === K.value.length - 1) return;
				k.value.push(e), Pn("selectLegend", sr.value);
			}
			M.value && vr(Z.value.find((e, t) => t === _r.value));
		}
		function wr(e) {
			return K.value.length ? K.value.find((t) => t.name === e) || (In.value && console.warn(`VueUiDonutEvolution - Series name not found "${e}"`), null) : (In.value && console.warn("VueUiDonutEvolution - There are no series to show."), null);
		}
		function Tr(e) {
			let t = wr(e);
			t !== null && k.value.includes(t.uid) && Cr(t.uid);
		}
		function Er(e) {
			let t = wr(e);
			t !== null && (k.value.includes(t.uid) || Cr(t.uid));
		}
		let Q = u(() => {
			let e = [""].concat(K.value.filter((e) => !k.value.includes(e.uid)).map((e) => ({
				name: e.name,
				color: e.color
			})), ["Σ"]), t = [];
			for (let e = 0; e < q.value; e += 1) {
				let n = K.value.filter((e) => !k.value.includes(e.uid)).map((t) => t.values[e] ?? 0).reduce((e, t) => e + t, 0);
				t.push([J.value[e] ? J.value[e].text : "-"].concat(K.value.filter((e) => !k.value.includes(e.uid)).map((t) => ({
					value: t.values[e] ?? 0,
					percentage: t.values[e] ? t.values[e] / n * 100 : 0
				})), [`${L.value.style.chart.layout.dataLabels.prefix}${Number(n.toFixed(L.value.table.td.roundingValue))}${L.value.style.chart.layout.dataLabels.suffix}`]));
			}
			return {
				head: e,
				body: t,
				bodyA11y: t.map((e) => e.map((t, n) => n === 0 || n === e.length - 1 ? t : `${pr(t.value ?? 0, null, n)} (${t.percentage.toFixed(1)}%)`)),
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
				colNames: [L.value.table.columnNames.period].concat(K.value.filter((e) => !k.value.includes(e.uid)).map((e) => e.name), L.value.table.columnNames.total)
			};
		});
		function Dr() {
			return K.value;
		}
		function Or(e = null) {
			Ye(() => {
				let n = [
					[L.value.style.chart.title.text],
					[L.value.style.chart.title.subtitle.text],
					[""]
				], i = [...Q.value.head.map((e) => e.name ?? e)], a = [...Q.value.body.map((e) => e.map((e) => e.value ?? e))], o = n.concat([i]).concat(a), s = r(o);
				e ? e(s) : t({
					csvContent: s,
					title: L.value.style.chart.title.text || "vue-ui-donut-evolution"
				});
			});
		}
		let $ = x(!1);
		function kr(e) {
			$.value = e, an.value += 1;
		}
		function Ar() {
			H.value.showTable = !H.value.showTable;
		}
		let jr = x(!1);
		function Mr() {
			jr.value = !jr.value;
		}
		function Nr(e) {
			return e.proportion * 100 > L.value.style.chart.donuts.hover.hideLabelsUnderValue;
		}
		let Pr = x([]), Fr = x({}), Ir = x(null);
		function Lr(e) {
			Pr.value = e.donut.map((e) => ({
				name: e.name,
				values: [e.value],
				color: e.color
			})), Fr.value = c({
				...L.value.style.chart.dialog.donutChart,
				responsive: !0,
				theme: L.value.theme
			}), Ir.value && Ir.value.open();
		}
		async function Rr({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { width: t, height: n } = N.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await je({
				domElement: N.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: L.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let zr = u(() => z.value.width), Br = u(() => z.value.height);
		Ae({
			timeLabelsEls: dn,
			timeLabels: J,
			slicer: B,
			configRef: L,
			rotationPath: [
				"style",
				"chart",
				"layout",
				"grid",
				"xAxis",
				"dataLabels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"layout",
				"grid",
				"xAxis",
				"dataLabels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: zr,
			height: Br,
			rotation: L.value.style.chart.layout.grid.xAxis.dataLabels.autoRotate.angle
		});
		let Vr = u(() => {
			let e = L.value.table.useDialog && !L.value.table.show, t = H.value.showTable;
			return {
				component: e ? Qt : Gt,
				title: `${L.value.style.chart.title.text}${L.value.style.chart.title.subtitle.text ? `: ${L.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: L.value.table.th.backgroundColor,
					color: L.value.table.th.color,
					headerColor: L.value.table.th.color,
					headerBg: L.value.table.th.backgroundColor,
					isFullscreen: $.value,
					fullscreenParent: N.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: R.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: L.value.style.chart.backgroundColor,
							color: L.value.style.chart.color
						},
						head: {
							backgroundColor: L.value.style.chart.backgroundColor,
							color: L.value.style.chart.color
						}
					}
				}
			};
		});
		tt(() => H.value.showTable, (e) => {
			L.value.table.show || (e && L.value.table.useDialog && hn.value ? hn.value.open() : "close" in hn.value && hn.value.close());
		});
		function Hr() {
			H.value.showTable = !1, gn.value && gn.value.setTableIconState(!1);
		}
		let Ur = u(() => yr.value.map((e) => ({
			...e,
			name: e.display
		}))), Wr = u(() => L.value.style.chart.backgroundColor), Gr = u(() => L.value.style.chart.legend), Kr = u(() => L.value.style.chart.title), { isCallbackImaging: qr, isCallbackSvg: Jr, generateSvg: Yr, onGenerateImage: Xr } = De({
			svg: V,
			title: Kr,
			legend: Gr,
			legendItems: Ur,
			backgroundColor: Wr,
			getSvgCallback: () => L.value.userOptions.callbacks.svg,
			generateImage: er
		});
		async function Zr() {
			if (Pn("copyAlt", {
				config: L.value,
				dataset: Z.value
			}), !L.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(L.value.userOptions.callbacks.altCopy({
				config: L.value,
				dataset: Z.value
			}));
		}
		function Qr() {
			I.value = null, Nn.value = !0;
		}
		function $r() {
			I.value = null, A.value = null, j.value = null, Nn.value = !1;
		}
		function ei(e) {
			if (!V.value || jr.value || document.activeElement !== V.value || !Z.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				I.value = null, A.value = null, j.value = null;
				return;
			}
			if (r) {
				if (I.value === null) return;
				let e = Z.value[I.value];
				if (!e) return;
				vr(e, I.value);
				return;
			}
			let a = I.value, o = A.value, s = a !== null && a >= 0 && a < Z.value.length, c = o !== null && o >= 0 && o < Z.value.length;
			s ? n ? (a += 1, a >= Z.value.length && (a = 0)) : t && (--a, a < 0 && (a = Z.value.length - 1)) : c ? (a = n ? o + 1 : o - 1, a >= Z.value.length && (a = 0), a < 0 && (a = Z.value.length - 1)) : a = n ? 0 : Z.value.length - 1;
			let ee = Z.value[a];
			ee && gr(ee);
		}
		let ti = u(() => ({
			headers: Q.value?.colNames ?? [],
			rows: Q.value?.bodyA11y ?? []
		}));
		return Ne({
			getData: Dr,
			getImage: Rr,
			generatePdf: $n,
			generateCsv: Or,
			generateImage: er,
			generateSvg: Yr,
			hideSeries: Er,
			showSeries: Tr,
			toggleTable: Ar,
			toggleAnnotator: Mr,
			toggleFullscreen: kr,
			copyAlt: Zr
		}), (e, t) => (b(), p("div", {
			ref_key: "donutEvolutionChart",
			ref: N,
			class: _(`vue-data-ui-component vue-ui-donut-evolution ${$.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${L.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: y(`font-family:${L.value.style.fontFamily};width:100%; text-align:center;background:${L.value.style.chart.backgroundColor}`),
			id: O.value,
			onMouseenter: t[5] ||= () => T(Jn)(!0),
			onMouseleave: t[6] ||= () => T(Jn)(!1)
		}, [
			m("div", {
				id: `chart-instructions-${O.value}`,
				class: "sr-only"
			}, [m("p", null, w(L.value.a11y.translations.keyboardNavigation), 1)], 8, at),
			ti.value?.rows?.length ? (b(), d(ze, {
				key: 0,
				uid: O.value,
				head: ti.value.headers,
				body: ti.value.rows,
				notice: L.value.a11y.translations.tableAvailable,
				caption: L.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : f("", !0),
			L.value.userOptions.buttons.annotator ? (b(), d(T(Yt), {
				key: 1,
				svgRef: T(V),
				backgroundColor: L.value.style.chart.backgroundColor,
				color: L.value.style.chart.color,
				active: jr.value,
				isCursorPointer: R.value,
				onClose: Mr
			}, {
				"annotator-action-close": E(() => [C(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": E(({ color: t }) => [C(e.$slots, "annotator-action-color", v(g({ color: t })), void 0, !0)]),
				"annotator-action-draw": E(({ mode: t }) => [C(e.$slots, "annotator-action-draw", v(g({ mode: t })), void 0, !0)]),
				"annotator-action-undo": E(({ disabled: t }) => [C(e.$slots, "annotator-action-undo", v(g({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": E(({ disabled: t }) => [C(e.$slots, "annotator-action-redo", v(g({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": E(({ disabled: t }) => [C(e.$slots, "annotator-action-delete", v(g({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : f("", !0),
			tr.value ? (b(), p("div", {
				key: 2,
				ref_key: "noTitle",
				ref: rn,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : f("", !0),
			L.value.style.chart.title.text ? (b(), p("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: En,
				style: "width:100%;background:transparent;padding-bottom:24px",
				onMouseleave: hr
			}, [(b(), d(Me, {
				key: `title_${sn.value}`,
				config: {
					title: {
						cy: "donut-evolution-div-title",
						...L.value.style.chart.title
					},
					subtitle: {
						cy: "donut-evolution-div-subtitle",
						...L.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 544)) : f("", !0),
			m("div", { id: `legend-top-${O.value}` }, null, 8, ot),
			L.value.userOptions.show && Fn.value && (T(Yn) || T(qn)) ? (b(), d(T(Xt), {
				ref_key: "userOptionsRef",
				ref: gn,
				key: `user_options_${an.value}`,
				backgroundColor: L.value.style.chart.backgroundColor,
				color: L.value.style.chart.color,
				isPrinting: T(Zn),
				isImaging: T(Qn),
				uid: O.value,
				hasPdf: L.value.userOptions.buttons.pdf,
				hasImg: L.value.userOptions.buttons.img,
				hasSvg: L.value.userOptions.buttons.svg,
				hasXls: L.value.userOptions.buttons.csv,
				hasTable: L.value.userOptions.buttons.table,
				hasFullscreen: L.value.userOptions.buttons.fullscreen,
				hasAltCopy: L.value.userOptions.buttons.altCopy,
				isFullscreen: $.value,
				titles: { ...L.value.userOptions.buttonTitles },
				chartElement: N.value,
				position: L.value.userOptions.position,
				hasAnnotator: L.value.userOptions.buttons.annotator,
				isAnnotation: jr.value,
				callbacks: L.value.userOptions.callbacks,
				printScale: L.value.userOptions.print.scale,
				tableDialog: L.value.table.useDialog,
				isCursorPointer: R.value,
				onToggleFullscreen: kr,
				onGeneratePdf: T($n),
				onGenerateCsv: Or,
				onGenerateImage: T(Xr),
				onGenerateSvg: T(Yr),
				onToggleTable: Ar,
				onToggleAnnotator: Mr,
				onCopyAlt: Zr,
				style: y({ visibility: T(Yn) ? T(qn) ? "visible" : "hidden" : "visible" })
			}, Ge({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: E(({ isOpen: t, color: n }) => [C(e.$slots, "menuIcon", v(g({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: E(() => [C(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: E(() => [C(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: E(() => [C(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: E(() => [C(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: E(() => [C(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: E(({ toggleFullscreen: t, isFullscreen: n }) => [C(e.$slots, "optionFullscreen", v(g({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: E(({ toggleAnnotator: t, isAnnotator: n }) => [C(e.$slots, "optionAnnotator", v(g({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: E(({ altCopy: t }) => [C(e.$slots, "optionAltCopy", v(g({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: E(() => [C(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: E(() => [C(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : f("", !0),
			m("div", st, [(b(), p("svg", {
				ref_key: "svgRef",
				ref: V,
				xmlns: T(pe),
				"aria-describedby": `chart-instructions-${O.value}`,
				class: _({
					"vue-data-ui-fullscreen--on": $.value,
					"vue-data-ui-fulscreen--off": !$.value,
					"vue-data-ui-no-transition": !T(Rn)
				}),
				viewBox: `0 0 ${G.value.absoluteWidth} ${G.value.absoluteHeight}`,
				style: y(`max-width:100%; overflow: visible; background:transparent;color:${L.value.style.chart.color};`),
				tabindex: "0",
				onFocus: Qr,
				onBlur: $r,
				onKeydown: ei
			}, [
				qe(T(Jt)),
				e.$slots["chart-background"] ? (b(), p("foreignObject", {
					key: 0,
					x: G.value.left,
					y: G.value.top,
					width: G.value.width,
					height: G.value.height,
					style: { pointerEvents: "none" }
				}, [C(e.$slots, "chart-background", {}, void 0, !0)], 8, lt)) : f("", !0),
				m("defs", null, [qe(Ie, {
					t: "linear",
					id: `hover_${O.value}`,
					x1: "0%",
					y1: "0%",
					x2: "0%",
					y2: "100%",
					stops: [[
						"0%",
						T(n)(L.value.style.chart.backgroundColor, L.value.style.chart.layout.highlighter.opacity),
						1
					], [
						"100%",
						T(n)(L.value.style.chart.layout.highlighter.color, L.value.style.chart.layout.highlighter.opacity),
						1
					]]
				}, null, 8, ["id", "stops"]), qe(Ie, {
					t: "radial",
					id: `focus_${O.value}`,
					stops: [
						[
							"0%",
							T(n)(T(te)(L.value.style.chart.backgroundColor), 0),
							1
						],
						[
							"77%",
							T(n)("#FFFFFF", 30),
							1
						],
						[
							"100%",
							T(n)(T(te)(L.value.style.chart.backgroundColor), 0),
							1
						]
					]
				}, null, 8, ["id", "stops"])]),
				L.value.style.chart.layout.grid.show ? (b(), p("g", ut, [
					m("line", {
						x1: W.value ? G.value.right : G.value.left,
						x2: W.value ? G.value.right : G.value.left,
						y1: G.value.top,
						y2: G.value.top + G.value.height,
						stroke: L.value.style.chart.layout.grid.stroke,
						"stroke-width": L.value.style.chart.layout.grid.strokeWidth,
						"stroke-linecap": "round"
					}, null, 8, dt),
					m("line", {
						x1: G.value.left,
						x2: G.value.right,
						y1: G.value.bottom,
						y2: G.value.bottom,
						stroke: L.value.style.chart.layout.grid.stroke,
						"stroke-width": L.value.style.chart.layout.grid.strokeWidth,
						"stroke-linecap": "round"
					}, null, 8, ft),
					L.value.style.chart.layout.grid.showVerticalLines ? (b(), p("g", pt, [(b(!0), p(l, null, S(B.value.end - B.value.start, (e, t) => (b(), p("line", {
						x1: G.value.left + (t + 1) * Y.value,
						x2: G.value.left + (t + 1) * Y.value,
						y1: G.value.top,
						y2: G.value.bottom,
						stroke: L.value.style.chart.layout.grid.stroke,
						"stroke-width": L.value.style.chart.layout.grid.strokeWidth,
						"stroke-linecap": "round"
					}, null, 8, mt))), 256))])) : f("", !0)
				])) : f("", !0),
				m("g", null, [L.value.style.chart.layout.grid.axis.yLabel ? (b(), p("text", {
					key: 0,
					ref_key: "yAxisLabel",
					ref: pn,
					"font-size": L.value.style.chart.layout.grid.axis.fontSize,
					fill: L.value.style.chart.layout.grid.axis.color,
					transform: `translate(${W.value ? G.value.absoluteWidth - L.value.style.chart.layout.grid.axis.fontSize / 2 - L.value.style.chart.layout.grid.axis.yLabelOffsetX : L.value.style.chart.layout.grid.axis.fontSize}, ${G.value.top + G.value.height / 2}) rotate(-90)`,
					"text-anchor": "middle",
					style: { transition: "none" }
				}, w(L.value.style.chart.layout.grid.axis.yLabel), 9, ht)) : f("", !0), L.value.style.chart.layout.grid.axis.xLabel ? (b(), p("text", {
					key: 1,
					ref_key: "xAxisLabel",
					ref: fn,
					"text-anchor": "middle",
					x: G.value.absoluteWidth / 2,
					y: G.value.absoluteHeight - 3,
					"font-size": L.value.style.chart.layout.grid.axis.fontSize,
					fill: L.value.style.chart.layout.grid.axis.color
				}, w(L.value.style.chart.layout.grid.axis.xLabel), 9, gt)) : f("", !0)]),
				L.value.style.chart.layout.grid.yAxis.dataLabels.show ? (b(), p("g", {
					key: 2,
					ref_key: "scaleLabels",
					ref: un,
					class: _({
						"donut-opacity": !0,
						"donut-behind": A.value !== null
					})
				}, [(b(!0), p(l, null, S(fr.value, (e, t) => (b(), p("g", { key: `sl_${t}` }, [e.value >= X.value.min && e.value <= X.value.max ? (b(), p("path", {
					key: 0,
					class: _({ "vue-data-ui-transition": T(Rn) }),
					d: `M${W.value ? G.value.right : G.value.left},${e.y} ${W.value ? G.value.right + 5 : G.value.left - 5},${e.y}`,
					stroke: L.value.style.chart.layout.grid.stroke,
					"stroke-width": L.value.style.chart.layout.grid.strokeWidth,
					"stroke-linecap": "round"
				}, null, 10, _t)) : f("", !0), e.value >= X.value.min && e.value <= X.value.max ? (b(), p("text", {
					key: 1,
					class: _({ "vue-data-ui-transition": T(Rn) }),
					transform: `translate(${W.value ? G.value.right - L.value.style.chart.layout.grid.yAxis.dataLabels.offsetX + 7 : G.value.left + L.value.style.chart.layout.grid.yAxis.dataLabels.offsetX - 7}, ${e.y + L.value.style.chart.layout.grid.yAxis.dataLabels.fontSize / 3})`,
					"font-size": L.value.style.chart.layout.grid.yAxis.dataLabels.fontSize,
					"text-anchor": W.value ? "start" : "end",
					fill: L.value.style.chart.layout.grid.yAxis.dataLabels.color,
					"font-weight": L.value.style.chart.layout.grid.yAxis.dataLabels.bold ? "bold" : "normal"
				}, w(T(_e)(e.value) ? T(se)(L.value.style.chart.layout.dataLabels.formatter, e.value, T(re)({
					p: L.value.style.chart.layout.dataLabels.prefix,
					v: e.value,
					s: L.value.style.chart.layout.dataLabels.suffix,
					r: L.value.style.chart.layout.grid.yAxis.dataLabels.roundingValue
				}), {
					datapoint: e,
					seriesIndex: t
				}) : ""), 11, vt)) : f("", !0)]))), 128))], 2)) : f("", !0),
				L.value.style.chart.layout.grid.xAxis.dataLabels.show ? (b(), p("g", {
					key: 3,
					ref_key: "timeLabelsEls",
					ref: dn,
					class: _({ "donut-opacity": !0 })
				}, [(b(!0), p(l, null, S(B.value.end - B.value.start, (e, t) => (b(), p("g", null, [(L.value.style.chart.layout.grid.xAxis.dataLabels.showOnlyFirstAndLast && (t === 0 || t === q.value - 1) || !L.value.style.chart.layout.grid.xAxis.dataLabels.showOnlyFirstAndLast) && J.value[t] && J.value[t].text ? (b(), p("g", yt, [String(J.value[t].text).includes("\n") ? (b(), p("text", {
					key: 1,
					class: "vue-data-ui-time-label",
					"text-anchor": L.value.style.chart.layout.grid.xAxis.dataLabels.rotation > 0 ? "start" : L.value.style.chart.layout.grid.xAxis.dataLabels.rotation < 0 ? "end" : "middle",
					"font-size": L.value.style.chart.layout.grid.xAxis.dataLabels.fontSize,
					fill: L.value.style.chart.layout.grid.xAxis.dataLabels.color,
					transform: `translate(${G.value.left + Y.value * t + Y.value / 2}, ${G.value.bottom + L.value.style.chart.layout.grid.xAxis.dataLabels.fontSize + L.value.style.chart.layout.grid.xAxis.dataLabels.offsetY}), rotate(${L.value.style.chart.layout.grid.xAxis.dataLabels.rotation})`,
					innerHTML: T(i)({
						content: String(J.value[t].text),
						fontSize: L.value.style.chart.layout.grid.xAxis.dataLabels.fontSize,
						fill: L.value.style.chart.layout.grid.xAxis.dataLabels.color,
						x: 0,
						y: 0
					})
				}, null, 8, xt)) : (b(), p("text", {
					key: 0,
					class: "vue-data-ui-time-label",
					"text-anchor": L.value.style.chart.layout.grid.xAxis.dataLabels.rotation > 0 ? "start" : L.value.style.chart.layout.grid.xAxis.dataLabels.rotation < 0 ? "end" : "middle",
					"font-size": L.value.style.chart.layout.grid.xAxis.dataLabels.fontSize,
					fill: L.value.style.chart.layout.grid.xAxis.dataLabels.color,
					transform: `translate(${G.value.left + Y.value * t + Y.value / 2}, ${G.value.bottom + L.value.style.chart.layout.grid.xAxis.dataLabels.fontSize + L.value.style.chart.layout.grid.xAxis.dataLabels.offsetY}), rotate(${L.value.style.chart.layout.grid.xAxis.dataLabels.rotation})`
				}, w(J.value[t].text || ""), 9, bt))])) : f("", !0)]))), 256))], 512)) : f("", !0),
				(b(!0), p(l, null, S(Z.value, (e, t) => (b(), p("g", null, [L.value.style.chart.layout.line.show && t < Z.value.length - 1 && ![e.subtotal, Z.value[t + 1].subtotal].includes(null) ? (b(), p("line", {
					key: 0,
					class: _({
						"donut-opacity": !0,
						"donut-behind": A.value !== null
					}),
					x1: e.x,
					y1: e.y,
					x2: Z.value[t + 1].x,
					y2: Z.value[t + 1].y,
					stroke: L.value.style.chart.layout.line.stroke,
					"stroke-width": L.value.style.chart.layout.line.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 10, St)) : f("", !0), e.subtotal === null ? f("", !0) : (b(), p("g", Ct, [e.subtotal ? (b(), p("circle", {
					key: 0,
					cx: e.x,
					cy: e.y,
					r: e.activeRadius,
					fill: L.value.style.chart.backgroundColor
				}, null, 8, wt)) : f("", !0)]))]))), 256)),
				(b(!0), p(l, null, S(Z.value, (e, t) => (b(), p("g", { class: _({
					"donut-opacity": !0,
					"donut-behind": t !== A.value && A.value !== null
				}) }, [e.subtotal ? (b(), p("g", Tt, [A.value !== null && A.value === t ? (b(), p("g", Et, [
					(b(!0), p(l, null, S(e.donutHover, (t) => (b(), p("g", null, [Nr(t) ? (b(), p("path", {
						key: 0,
						d: T(oe)(t, {
							x: t.center.endX,
							y: t.center.endY
						}, 12, 12, {
							x: e.x,
							y: e.y
						}, !1, 20),
						stroke: t.color,
						"stroke-width": "1",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						fill: "none"
					}, null, 8, Dt)) : f("", !0)]))), 256)),
					(b(!0), p(l, null, S(e.donutHover, (t, n) => (b(), p("g", null, [Nr(t) ? (b(), p("text", {
						key: 0,
						"data-cy-hover-label": "",
						"text-anchor": T(ae)(t, !0, 0).anchor,
						x: T(ae)(t, !0, 9).x,
						y: T(ue)(t, 14, 10),
						fill: L.value.style.chart.layout.grid.yAxis.dataLabels.color,
						"font-size": 8,
						"font-weight": "bold"
					}, w(t.name) + ": " + w(mr(t, e.donut)) + " (" + w(t.value === null ? "-" : pr(t.value, t, n)) + ") ", 9, Ot)) : f("", !0)]))), 256)),
					m("g", null, [m("circle", {
						cx: e.x,
						cy: e.y,
						r: e.hoverRadius,
						fill: L.value.style.chart.backgroundColor
					}, null, 8, kt)])
				])) : f("", !0)])) : f("", !0)], 2))), 256)),
				(b(!0), p(l, null, S(Z.value, (e, t) => (b(), p("g", { class: _({
					"donut-opacity": !0,
					"donut-behind": t !== A.value && A.value !== null
				}) }, [e.subtotal === null ? f("", !0) : (b(), p("g", At, [e.subtotal === 0 ? (b(), p("circle", {
					key: 0,
					cx: e.x,
					cy: e.y,
					r: 3,
					fill: L.value.style.chart.color
				}, null, 8, jt)) : A.value !== null && A.value === t ? (b(), p("g", Mt, [(b(!0), p(l, null, S(e.donutHover, (e, t) => (b(), p("path", {
					d: e.arcSlice,
					fill: `${e.color}`,
					"stroke-width": 1,
					stroke: L.value.style.chart.backgroundColor
				}, null, 8, Nt))), 256))])) : (b(), p("g", Pt, [(b(!0), p(l, null, S(e.donut, (e, t) => (b(), p("path", {
					d: e.arcSlice,
					fill: `${e.color}`,
					"stroke-width": .5,
					stroke: L.value.style.chart.backgroundColor
				}, null, 8, Ft))), 256))]))]))], 2))), 256)),
				(b(!0), p(l, null, S(Z.value, (e, t) => (b(), p("g", { class: _({
					"donut-opacity": !0,
					"donut-behind": t !== A.value && A.value !== null || nn.value && t !== M.value.index
				}) }, [e.subtotal !== null && L.value.style.chart.layout.dataLabels.show ? (b(), p("text", {
					key: 0,
					"text-anchor": "middle",
					x: e.x,
					y: A.value === e.index && e.subtotal ? e.y + L.value.style.chart.layout.dataLabels.fontSize / 3 : e.y - e.radius - L.value.style.chart.layout.dataLabels.fontSize + L.value.style.chart.layout.dataLabels.offsetY,
					"font-size": L.value.style.chart.layout.dataLabels.fontSize,
					"font-weight": "bold",
					fill: L.value.style.chart.layout.dataLabels.color
				}, w(pr(e.subtotal, e, t)), 9, It)) : f("", !0)], 2))), 256)),
				(b(!0), p(l, null, S(Z.value, (e, t) => (b(), p("rect", {
					x: G.value.left + t * Y.value,
					y: G.value.top,
					width: Y.value,
					height: G.value.height,
					fill: [A.value, _r.value].includes(e.index) ? `url(#hover_${O.value})` : "transparent",
					class: _({ "donut-hover": R.value && e.subtotal && [A.value, _r.value].includes(e.index) }),
					style: { pointerEvents: "none" }
				}, null, 10, Lt))), 256)),
				(b(!0), p(l, null, S(Z.value, (e, t) => (b(), p("rect", {
					"data-cy-trap": "",
					x: G.value.left + t * Y.value,
					y: G.value.top,
					width: Y.value,
					height: G.value.height,
					fill: "transparent",
					onMouseenter: (t) => gr(e),
					onMouseleave: (t) => hr(e),
					onClick: (n) => vr(e, t),
					class: _({ "donut-hover": R.value && A.value === e.index && e.subtotal })
				}, null, 42, Rt))), 256)),
				C(e.$slots, "svg", { svg: {
					...G.value,
					isPrintingImg: T(Zn) || T(Qn) || T(qr),
					isPrintingSvg: T(Jr)
				} }, void 0, !0)
			], 46, ct)), e.$slots.hint ? (b(), p("div", zt, [C(e.$slots, "hint", v(g({
				hint: L.value.a11y.translations.keyboardNavigation,
				isVisible: Nn.value
			})), void 0, !0)])) : f("", !0)]),
			e.$slots.watermark ? (b(), p("div", Bt, [C(e.$slots, "watermark", v(g({ isPrinting: T(Zn) || T(Qn) || T(qr) || T(Jr) })), void 0, !0)])) : f("", !0),
			m("div", {
				ref_key: "chartSlicer",
				ref: An,
				style: y(`width:100%;background:${L.value.style.chart.backgroundColor}`),
				"data-dom-to-png-ignore": ""
			}, [q.value > 1 && L.value.style.chart.zoom.show ? (b(), d(Le, {
				ref_key: "slicerComponent",
				ref: kn,
				key: `slicer_${on.value}`,
				background: L.value.style.chart.zoom.color,
				borderColor: L.value.style.chart.backgroundColor,
				fontSize: L.value.style.chart.zoom.fontSize,
				useResetSlot: L.value.style.chart.zoom.useResetSlot,
				timeLabels: J.value,
				textColor: L.value.style.chart.color,
				inputColor: L.value.style.chart.zoom.color,
				selectColor: L.value.style.chart.zoom.highlightColor,
				max: q.value,
				min: 0,
				valueStart: B.value.start,
				valueEnd: B.value.end,
				start: B.value.start,
				"onUpdate:start": t[0] ||= (e) => B.value.start = e,
				end: B.value.end,
				"onUpdate:end": t[1] ||= (e) => B.value.end = e,
				refreshStartPoint: L.value.style.chart.zoom.startIndex === null ? 0 : L.value.style.chart.zoom.startIndex,
				refreshEndPoint: L.value.style.chart.zoom.endIndex === null ? q.value : L.value.style.chart.zoom.endIndex + 1,
				enableRangeHandles: L.value.style.chart.zoom.enableRangeHandles,
				enableSelectionDrag: L.value.style.chart.zoom.enableSelectionDrag,
				focusOnDrag: L.value.style.chart.zoom.focusOnDrag,
				focusRangeRatio: L.value.style.chart.zoom.focusRangeRatio,
				isCursorPointer: R.value,
				maxWidth: L.value.style.chart.zoom.maxWidth,
				minimapLeftInsetRatio: G.value.absoluteWidth > 0 && L.value.style.chart.zoom.autoFit ? G.value.left / G.value.absoluteWidth : null,
				minimapRightInsetRatio: G.value.absoluteWidth > 0 && L.value.style.chart.zoom.autoFit ? (G.value.absoluteWidth - G.value.right) / G.value.absoluteWidth : null,
				onReset: Wn
			}, {
				"reset-action": E(({ reset: t }) => [C(e.$slots, "reset-action", v(g({ reset: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"background",
				"borderColor",
				"fontSize",
				"useResetSlot",
				"timeLabels",
				"textColor",
				"inputColor",
				"selectColor",
				"max",
				"valueStart",
				"valueEnd",
				"start",
				"end",
				"refreshStartPoint",
				"refreshEndPoint",
				"enableRangeHandles",
				"enableSelectionDrag",
				"focusOnDrag",
				"focusRangeRatio",
				"isCursorPointer",
				"maxWidth",
				"minimapLeftInsetRatio",
				"minimapRightInsetRatio"
			])) : f("", !0)], 4),
			m("div", { id: `legend-bottom-${O.value}` }, null, 8, Vt),
			mn.value && (L.value.style.chart.legend.show || e.$slots.legend) ? (b(), d(We, {
				key: 6,
				to: L.value.style.chart.legend.position === "top" ? `#legend-top-${O.value}` : `#legend-bottom-${O.value}`
			}, [m("div", {
				ref_key: "chartLegend",
				ref: Dn
			}, [C(e.$slots, "legend", { legend: yr.value }, () => [L.value.style.chart.legend.show ? (b(), d(He, {
				key: `legend_${ln.value}`,
				legendSet: yr.value,
				config: xr.value,
				onClickMarker: t[2] ||= ({ legend: e }) => Cr(e.uid)
			}, {
				item: E(({ legend: e, index: t }) => [m("div", {
					onClick: (t) => Cr(e.uid),
					style: y(`opacity:${k.value.includes(e.uid) ? .5 : 1}`)
				}, w(e.display), 13, Ht)]),
				legendToggle: E(() => [yr.value.length > 2 && L.value.style.chart.legend.selectAllToggle.show && !T(Bn) ? (b(), d(Re, {
					key: 0,
					backgroundColor: L.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: L.value.style.chart.legend.selectAllToggle.color,
					fontSize: L.value.style.chart.legend.fontSize,
					checked: k.value.length > 0,
					onToggle: Sr
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked"
				])) : f("", !0)]),
				_: 1
			}, 8, ["legendSet", "config"])) : f("", !0)], !0)], 512)], 8, ["to"])) : f("", !0),
			e.$slots.source ? (b(), p("div", {
				key: 7,
				ref_key: "source",
				ref: On,
				dir: "auto"
			}, [C(e.$slots, "source", {}, void 0, !0)], 512)) : f("", !0),
			Fn.value && L.value.userOptions.buttons.table ? (b(), d(Qe(Vr.value.component), Je({ key: 8 }, Vr.value.props, {
				ref_key: "tableUnit",
				ref: hn,
				onClose: Hr
			}), Ge({
				content: E(() => [(b(), d(T(qt), {
					key: `table_${cn.value}`,
					colNames: Q.value.colNames,
					head: Q.value.head,
					body: Q.value.body,
					config: Q.value.config,
					title: L.value.table.useDialog ? "" : Vr.value.title,
					withCloseButton: !L.value.table.useDialog,
					isCursorPointer: R.value,
					onClose: Hr
				}, {
					th: E(({ th: e }) => [Ke(w(e.name ?? e), 1)]),
					td: E(({ td: e }) => [e.value === null ? (b(), p("span", Ut, "-")) : (b(), p("b", Wt, w(isNaN(e.value) ? "" : L.value.style.chart.layout.dataLabels.prefix) + w(!isNaN(e.value) && e.value !== null ? Number(e.value.toFixed(L.value.table.td.roundingValue)).toLocaleString() : e) + w(isNaN(e.value) ? "" : L.value.style.chart.layout.dataLabels.suffix), 1)), m("span", null, w(e.percentage && !isNaN(e.percentage) ? `(${Number(e.percentage.toFixed(L.value.table.td.roundingPercentage)).toLocaleString()}%)` : ""), 1)]),
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
				fn: E(() => [Ke(w(Vr.value.title), 1)]),
				key: "0"
			} : void 0, L.value.table.useDialog ? {
				name: "actions",
				fn: E(() => [m("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[3] ||= (e) => Or(L.value.userOptions.callbacks.csv),
					style: y({ cursor: R.value ? "cursor" : "default" })
				}, [qe(T(Kt), {
					name: "fileCsv",
					stroke: Vr.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : f("", !0),
			L.value.style.chart.dialog.show ? (b(), d(T(Qt), {
				key: 9,
				ref_key: "dialog",
				ref: Ir,
				onClose: t[4] ||= (e) => {
					M.value = null, nn.value = !1, j.value = null, A.value = null, _r.value = null;
				},
				backgroundColor: L.value.style.chart.dialog.backgroundColor,
				color: L.value.style.chart.dialog.color,
				headerBg: L.value.style.chart.dialog.header.backgroundColor,
				headerColor: L.value.style.chart.dialog.header.color,
				isFullscreen: $.value,
				fullscreenParent: N.value
			}, {
				title: E(() => [Ke(w(J.value[Number(M.value.index)] ? J.value[Number(M.value.index)].text : ""), 1)]),
				content: E(() => [M.value ? (b(), d(T(Zt), {
					key: 0,
					config: Fr.value,
					dataset: Pr.value
				}, null, 8, ["config", "dataset"])) : f("", !0)]),
				_: 1
			}, 8, [
				"backgroundColor",
				"color",
				"headerBg",
				"headerColor",
				"isFullscreen",
				"fullscreenParent"
			])) : f("", !0),
			T(Bn) ? (b(), d(we, { key: 10 })) : f("", !0)
		], 46, it));
	}
}, [["__scopeId", "data-v-9711e76c"]]);
//#endregion
export { rt as n, Gt as t };
