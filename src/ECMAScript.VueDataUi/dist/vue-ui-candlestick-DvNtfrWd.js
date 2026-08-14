import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Jt as i, Vt as a, X as o, _ as s, b as c, c as l, ct as ee, dt as te, jt as ne, q as re, qt as ie, t as ae, tt as u, xt as oe } from "./lib-Bttd6u5E.js";
import { n as se, t as ce } from "./useHints-Dq_w2E8B.js";
import { n as le, r as ue, t as de } from "./useTimeLabels-d2f-W1L4.js";
import { t as fe } from "./useConfig-DlNpz6P8.js";
import { t as pe } from "./usePrinter-DN5bYhTG.js";
import { n as me, t as he } from "./BaseScanner-DZvpgOjM.js";
import { t as ge } from "./useNestedProp-vPNvh7rV.js";
import { t as _e } from "./useThemeCheck-C43Tcqmk.js";
import { t as ve } from "./useChartExport-DNiwdPmb.js";
import { t as ye } from "./useTransitions-g_zBREk2.js";
import { t as be } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as xe } from "./img-Bnokohej.js";
import { n as Se } from "./Title-BE3qg9xl.js";
import { t as Ce } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as we, t as Te } from "./useResponsive-ZtArZtUf.js";
import { t as Ee } from "./DefGrad-DVBqDjhO.js";
import { t as De } from "./SlicerPreview-wUw1hFwe.js";
import { t as Oe } from "./A11yDataTable-DdRsVULz.js";
import { t as ke } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ae } from "./useChartAccessibility-DYqac8yF.js";
import { t as je } from "./vue_ui_candlestick-J8jmJvxP.js";
import { Fragment as d, computed as f, createBlock as p, createCommentVNode as m, createElementBlock as h, createElementVNode as g, createSlots as Me, createTextVNode as Ne, createVNode as Pe, defineAsyncComponent as _, guardReactiveProps as v, mergeProps as Fe, nextTick as Ie, normalizeClass as y, normalizeProps as b, normalizeStyle as x, onBeforeUnmount as Le, onMounted as Re, openBlock as S, ref as C, renderList as w, renderSlot as T, resolveDynamicComponent as ze, shallowRef as Be, toDisplayString as E, toRefs as Ve, unref as D, watch as He, watchEffect as Ue, withCtx as O } from "vue";
//#region src/components/vue-ui-candlestick.vue
var We = /* @__PURE__ */ e({ default: () => wt }), Ge = ["id"], Ke = ["id"], qe = { style: { position: "relative" } }, Je = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Ye = [
	"x",
	"y",
	"width",
	"height"
], Xe = { key: 1 }, Ze = { key: 0 }, Qe = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], $e = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], et = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], tt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], nt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], rt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], it = [
	"transform",
	"text-anchor",
	"font-size",
	"fill",
	"font-weight"
], at = [
	"transform",
	"text-anchor",
	"font-size",
	"fill",
	"font-weight"
], ot = [
	"transform",
	"text-anchor",
	"font-size",
	"fill",
	"font-weight"
], st = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], ct = { key: 0 }, lt = [
	"cx",
	"cy",
	"r",
	"fill"
], ut = [
	"cx",
	"cy",
	"r",
	"fill"
], dt = { key: 1 }, ft = [
	"x",
	"y",
	"width",
	"height",
	"rx",
	"fill"
], pt = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], mt = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"rx"
], ht = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"rx",
	"stroke",
	"stroke-width"
], gt = ["d", "stroke"], _t = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"onMouseover",
	"onMouseleave",
	"onClick"
], vt = ["data-start", "data-end"], yt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, bt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, xt = ["d", "stroke"], St = [
	"d",
	"stroke",
	"stroke-width"
], Ct = ["innerHTML"], wt = /*#__PURE__*/ Ce({
	__name: "vue-ui-candlestick",
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
	emits: ["selectX", "copyAlt"],
	setup(e, { expose: Ce, emit: We }) {
		let wt = _(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Tt = _(() => import("./Tooltip-DhjyfHwz.js")), Et = _(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Dt = _(() => import("./DataTable-BbKgJ5UI.js")), Ot = _(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), kt = _(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), At = _(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), jt = _(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_candlestick: Mt } = fe(), { isThemeValid: Nt, warnInvalidTheme: Pt } = _e(), k = e, Ft = We, It = f(() => !!k.dataset && k.dataset.length), A = C(!1), Lt = C(null), j = C(re()), Rt = C(!1), zt = C(""), M = C(void 0), Bt = C(0), N = C(null), Vt = C(null), Ht = C(null), Ut = C(null), Wt = C(null), Gt = C(null), Kt = C(0), qt = C(0), Jt = C(0), Yt = C(null), Xt = C(null), Zt = C(null), Qt = C(null), $t = C(null), P = C(null), en = C({
			x: 0,
			y: 0
		}), tn = C("pointer"), nn = C(!1), F = C(pn());
		se({
			config: () => F.value,
			dataset: () => k.dataset,
			component: "VueUiCandlestick",
			rules: [ce.emptyArray, {
				test: (e) => e.length > 200,
				message: [
					"👀 Dataset has > 200 points, which can cause performance issues. Consider:",
					"",
					"▶️ Adding date inputs to constraint the dataset into a specific timeframe",
					"",
					"▶️ Aggregate the data to larger time series to reduce the number of datapoints."
				]
			}]
		});
		let { transitionEnabled: rn } = ye({
			config: () => F.value.transitions,
			dataset: () => k.dataset
		}), an = f(() => F.value.userOptions.useCursorPointer), on = f(() => i({
			defaultConfig: {
				useCssAnimation: !1,
				userOptions: { show: !1 },
				table: { show: !1 },
				style: {
					backgroundColor: "#99999930",
					layout: {
						candle: { colors: {
							bearish: "#BABABA",
							bullish: "#CACACA"
						} },
						grid: {
							stroke: "#6A6A6A",
							verticalLines: { stroke: "#6A6A6A" },
							horizontalLines: { stroke: "#6A6A6A" },
							yAxis: {
								dataLabels: { show: !1 },
								scale: {
									min: null,
									max: null
								}
							}
						},
						wick: {
							stroke: "#6A6A6A",
							extremity: { color: "#6A6A6A" }
						}
					},
					tooltip: { show: !1 },
					zoom: {
						show: !1,
						startIndex: null,
						endIndex: null
					}
				}
			},
			userConfig: F.value.skeletonConfig ?? {}
		})), { loading: I, FINAL_DATASET: L, manualLoading: sn } = me({
			...Ve(k),
			FINAL_CONFIG: F,
			prepareConfig: pn,
			callback: () => {
				Promise.resolve().then(async () => {
					await sr();
				});
			},
			skeletonDataset: k.config?.skeletonDataset ?? [
				[
					17040672e5,
					10,
					20,
					2,
					10,
					30
				],
				[
					17067456e5,
					10,
					30,
					5,
					20,
					50
				],
				[
					17092512e5,
					20,
					50,
					10,
					30,
					80
				],
				[
					17119296e5,
					30,
					80,
					20,
					50,
					130
				],
				[
					17145216e5,
					50,
					130,
					30,
					100,
					210
				],
				[
					17172e8,
					80,
					210,
					50,
					150,
					340
				],
				[
					1719792e6,
					130,
					340,
					80,
					280,
					550
				],
				[
					17224704e5,
					210,
					550,
					130,
					450,
					890
				],
				[
					17251488e5,
					340,
					890,
					210,
					750,
					1440
				],
				[
					17277408e5,
					550,
					1440,
					340,
					1230,
					2330
				],
				[
					17304192e5,
					890,
					2330,
					550,
					1950,
					3770
				],
				[
					17330112e5,
					1440,
					3770,
					890,
					3200,
					5100
				]
			],
			skeletonConfig: i({
				defaultConfig: F.value,
				userConfig: on.value
			})
		}), { userOptionsVisible: cn, setUserOptionsVisibility: ln, keepUserOptionState: un } = ke({ config: F.value }), { svgRef: R } = Ae({ config: F.value.style.title });
		function dn() {
			ln(!0);
		}
		function fn() {
			ln(!1), Ft("selectX", {
				seriesIndex: null,
				datapoint: null
			}), M.value = null;
		}
		function pn() {
			let e = ge({
				userConfig: k.config,
				defaultConfig: Mt
			}), t = {}, n = e.theme;
			if (n) if (!Nt.value(e)) Pt(e), t = e;
			else {
				let r = ge({
					userConfig: je[n] || k.config,
					defaultConfig: e
				});
				t = { ...ge({
					userConfig: k.config,
					defaultConfig: r
				}) };
			}
			else t = e;
			return t;
		}
		He(() => k.config, (e) => {
			I.value || (F.value = pn()), cn.value = !F.value.userOptions.showOnChartHover, hn(), Kt.value += 1, Jt.value += 1, qt.value += 1, H.value.showTable = F.value.table.show, H.value.showTooltip = F.value.style.tooltip.show, On();
		}, { deep: !0 }), He(() => k.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (sn.value = !1), rr();
		}, { deep: !0 });
		let z = C({
			height: F.value.style.height,
			width: F.value.style.width,
			xAxisFontSize: F.value.style.layout.grid.xAxis.dataLabels.fontSize,
			yAxisFontSize: F.value.style.layout.grid.yAxis.dataLabels.fontSize
		}), B = Be(null), V = Be(null);
		Re(() => {
			hn();
		});
		let mn = f(() => F.value.debug);
		function hn() {
			if (ne(k.dataset) && (u({
				componentName: "VueUiCandlestick",
				type: "dataset",
				debug: mn.value
			}), sn.value = !0), ne(k.dataset) || (sn.value = F.value.loading), setTimeout(() => {
				A.value = !0;
			}, 10), F.value.responsive) {
				let e = we(() => {
					A.value = !1;
					let { width: e, height: t } = Te({
						chart: N.value,
						title: F.value.style.title.text ? Vt.value : null,
						slicer: F.value.style.zoom.show && G.value > 6 ? Ut.value.$el : null,
						legend: Ht.value,
						source: Wt.value,
						noTitle: Gt.value
					});
					requestAnimationFrame(() => {
						z.value.width = e, z.value.height = t - 12, F.value.responsiveProportionalSizing ? (z.value.xAxisFontSize = ie({
							relator: Math.min(e, t),
							adjuster: F.value.style.width,
							source: F.value.style.layout.grid.xAxis.dataLabels.fontSize,
							threshold: 6,
							fallback: 6
						}), z.value.yAxisFontSize = ie({
							relator: Math.min(e, t),
							adjuster: F.value.style.width,
							source: F.value.style.layout.grid.yAxis.dataLabels.fontSize,
							threshold: 6,
							fallback: 6
						})) : (z.value.xAxisFontSize = F.value.style.layout.grid.xAxis.dataLabels.fontSize, z.value.yAxisFontSize = F.value.style.layout.grid.yAxis.dataLabels.fontSize), Lt.value && clearTimeout(Lt.value), Lt.value = setTimeout(() => {
							A.value = !0;
						}, 10);
					});
				});
				B.value && (V.value && B.value.unobserve(V.value), B.value.disconnect()), B.value = new ResizeObserver(e), V.value = N.value.parentNode, B.value.observe(V.value);
			}
			sr();
		}
		Le(() => {
			B.value && (V.value && B.value.unobserve(V.value), B.value.disconnect());
		});
		let { isPrinting: gn, isImaging: _n, generatePdf: vn, generateImage: yn } = pe({
			elementId: `vue-ui-candlestick_${j.value}`,
			fileName: F.value.style.title.text || "vue-ui-candlestick",
			options: F.value.userOptions.print
		}), bn = f(() => F.value.userOptions.show && !F.value.style.title.text), H = C({
			showTable: F.value.table.show,
			showTooltip: F.value.style.tooltip.show
		}), xn = C(0), Sn = we((e) => {
			xn.value = e;
		}, 100);
		Ue((e) => {
			let t = Xt.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				Sn(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), Le(() => {
			xn.value = 0;
		});
		let Cn = f(() => {
			let e = 0;
			return Xt.value && (e = xn.value + z.value.xAxisFontSize), e;
		}), U = f(() => F.value.style.layout.grid.yAxis.position === "right");
		function wn() {
			let e = F.value.style.layout.grid.yAxis.dataLabels.offsetX;
			Yt.value && (e = Array.from(Yt.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = F.value.style.layout.grid.yAxis.axisName?.show ? F.value.style.layout.grid.yAxis.axisName.fontSize + F.value.style.layout.grid.yAxis.axisName.offsetX : 0, n = e + 13 + t;
			return {
				left: U.value ? 0 : n,
				right: U.value ? n : 0,
				scaleLabelsWidth: e,
				yAxisNameWidth: t,
				crosshair: 13
			};
		}
		let W = f(() => {
			let { top: e, right: t, bottom: n, left: r } = F.value.style.layout.padding, i = wn();
			return {
				top: e + 12,
				right: z.value.width - t - i.right,
				left: r + i.left,
				bottom: z.value.height - n - Cn.value,
				width: z.value.width - r - t - i.left - i.right,
				height: z.value.height - e - n - Cn.value - 12,
				scaleLabelsWidth: i.scaleLabelsWidth,
				yAxisNameWidth: i.yAxisNameWidth,
				crosshair: i.crosshair
			};
		}), Tn = f(() => {
			let { left: e, top: t, width: n, height: r } = W.value, i = K.value.start, a = K.value.end - i, o = n / a, s = q.value.start - i, c = q.value.end - i, l = Math.max(0, Math.min(a, s)), ee = Math.max(0, Math.min(a, c));
			return {
				x: e + l * o,
				y: t,
				width: (ee - l) * o,
				height: r,
				fill: F.value.style.zoom.preview.fill,
				stroke: F.value.style.zoom.preview.stroke,
				"stroke-width": F.value.style.zoom.preview.strokeWidth,
				"stroke-dasharray": F.value.style.zoom.preview.strokeDasharray,
				"stroke-linecap": "round",
				"stroke-linejoin": "round",
				style: {
					pointerEvents: "none",
					transition: "none !important",
					animation: "none !important"
				}
			};
		}), G = f(() => L.value.length), K = C({
			start: 0,
			end: G.value
		}), q = C({
			start: 0,
			end: G.value
		}), En = f(() => F.value.style.zoom.preview.enable && (q.value.start !== K.value.start || q.value.end !== K.value.end));
		function Dn(e, t) {
			q.value[e] = t;
		}
		function On() {
			let e = Math.max(0, Math.min(K.value.start ?? 0, G.value - 1)), t = Math.max(e + 1, Math.min(K.value.end ?? G.value, G.value));
			(!Number.isFinite(e) || !Number.isFinite(t) || t <= e) && (e = 0, t = G.value), K.value.start = e, K.value.end = t, q.value.start = e, q.value.end = t, Ut.value && (Ut.value.setStartValue(e), Ut.value.setEndValue(t));
		}
		let kn = f(() => L.value.map((e, t) => ({
			...e,
			absoluteIndex: t
		}))), An = f(() => kn.value.slice(K.value.start, K.value.end)), J = f(() => (mn.value && L.value.forEach((e, t) => {
			[null, void 0].includes(e[0]) && u({
				componentName: "VueUiCandlestick",
				type: "datasetAttribute",
				property: "period (index 0)",
				index: t
			}), [null, void 0].includes(e[1]) && u({
				componentName: "VueUiCandlestick",
				type: "datasetAttribute",
				property: "open (index 1)",
				index: t
			}), [null, void 0].includes(e[2]) && u({
				componentName: "VueUiCandlestick",
				type: "datasetAttribute",
				property: "high (index 2)",
				index: t
			}), [null, void 0].includes(e[3]) && u({
				componentName: "VueUiCandlestick",
				type: "datasetAttribute",
				property: "low (index 3)",
				index: t
			}), [null, void 0].includes(e[4]) && u({
				componentName: "VueUiCandlestick",
				type: "datasetAttribute",
				property: "close (index 4)",
				index: t
			}), [null, void 0].includes(e[5]) && u({
				componentName: "VueUiCandlestick",
				type: "datasetAttribute",
				property: "volume (index 5)",
				index: t
			});
		}), An.value.map((e) => ({
			absoluteIndex: e.absoluteIndex,
			period: e[0],
			open: e[1],
			high: e[2],
			low: e[3],
			close: e[4],
			volume: e[5]
		})))), jn = f(() => kn.value.map((e) => ({
			absoluteIndex: e.absoluteIndex,
			period: e[0],
			open: e[1],
			high: e[2],
			low: e[3],
			close: e[4],
			volume: e[5]
		}))), Y = f(() => W.value.width / An.value.length), Mn = f(() => ({
			max: F.value.style.layout.grid.yAxis.scale.max === null ? Math.max(...J.value.map((e) => e.high)) : F.value.style.layout.grid.yAxis.scale.max,
			min: F.value.style.layout.grid.yAxis.scale.min === null ? 0 : F.value.style.layout.grid.yAxis.scale.min
		})), X = f(() => s(Mn.value.min, Mn.value.max, F.value.style.layout.grid.yAxis.dataLabels.steps));
		function Nn(e, t, n = null, r = null) {
			return {
				...e,
				x: c(W.value.left + t * Y.value + Y.value / 2),
				y: c(W.value.top + (1 - (e - X.value.min) / (X.value.max - X.value.min)) * W.value.height),
				value: c(e),
				isMax: e === n,
				isMin: e === r
			};
		}
		let Z = f(() => {
			let e = {
				o: Math.max(...J.value.map((e) => e.open)),
				h: Math.max(...J.value.map((e) => e.high)),
				l: Math.max(...J.value.map((e) => e.low)),
				c: Math.max(...J.value.map((e) => e.low))
			}, t = {
				o: Math.min(...J.value.map((e) => e.open)),
				h: Math.min(...J.value.map((e) => e.high)),
				l: Math.min(...J.value.map((e) => e.low)),
				c: Math.min(...J.value.map((e) => e.low))
			}, n = Math.max(...J.value.map((e) => e.volume)), r = Math.min(...J.value.map((e) => e.volume));
			return J.value.map((i, a) => {
				let o = Nn(i.open, a, e.o, t.o), s = Nn(i.high, a, e.h, t.h), c = Nn(i.low, a, e.l, t.l), l = Nn(i.close, a, e.c, t.c), ee = i.close > i.open, te = i.volume === n, ne = i.volume === r;
				return {
					period: i.period,
					open: o,
					high: s,
					low: c,
					close: l,
					volume: i.volume,
					isBullish: ee,
					absoluteIndex: i.absoluteIndex,
					isMaxVolume: te,
					isMinVolume: ne
				};
			});
		});
		function Pn({ item: e, index: t, minimapH: n, unitW: r }) {
			let i = F.value.style.layout.grid.yAxis.scale.min ?? 0, a = F.value.style.layout.grid.yAxis.scale.max ?? Math.max(...L.value.map((e) => e[2]));
			return {
				...e,
				x: c(t * r),
				y: c((1 - (e - i) / (a - i)) * n),
				value: c(e)
			};
		}
		let Fn = f(() => ({ minimapH: e, unitW: t }) => jn.value.map((n, r) => {
			let i = Pn({
				item: n.open,
				index: r,
				minimapH: e,
				unitW: t
			}), a = Pn({
				item: n.high,
				index: r,
				minimapH: e,
				unitW: t
			}), o = Pn({
				item: n.low,
				index: r,
				minimapH: e,
				unitW: t
			}), s = Pn({
				item: n.close,
				index: r,
				minimapH: e,
				unitW: t
			}), c = n.close > n.open;
			return {
				period: n.period,
				open: i,
				high: a,
				low: o,
				close: s,
				volume: n.volume,
				isBullish: c,
				absoluteIndex: n.absoluteIndex
			};
		})), In = f(() => F.value.style.zoom.minimap.show ? [{
			name: "",
			series: L.value.map((e) => e[2]),
			color: "#000000",
			isVisible: !0
		}] : []);
		function Ln(e) {
			return c((e - X.value.min) / (X.value.max - X.value.min));
		}
		let Rn = f(() => X.value.ticks.map((e) => ({
			y: W.value.bottom - W.value.height * Ln(e),
			value: c(e)
		}))), zn = f(() => J.value.map((e) => e.period)), Q = C([]), Bn = C([]), Vn = 0;
		Ue(() => {
			let e = ++Vn;
			(async () => {
				let t = await de({
					values: L.value.map((e) => e[0]),
					maxDatapoints: L.value.length,
					formatter: F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter,
					start: K.value.start,
					end: K.value.end
				});
				e === Vn && (Q.value = t);
			})();
		});
		let Hn = 0;
		Ue(() => {
			let e = ++Hn;
			(async () => {
				let t = await de({
					values: L.value.map((e) => e[0]),
					maxDatapoints: L.value.length,
					formatter: F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter,
					start: 0,
					end: G.value
				});
				e === Hn && (Bn.value = t);
			})();
		});
		let Un = f(() => {
			let e = F.value.style.layout.grid.xAxis.dataLabels.modulo;
			return Q.value.length ? Math.min(e, [...new Set(Q.value.map((e) => e.text))].length) : e;
		}), Wn = C({
			months: [],
			shortMonths: [],
			days: [],
			shortDays: []
		}), Gn = 0;
		Ue(() => {
			let e = ++Gn, t = F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter;
			(async () => {
				let n = await ue(t.locale).catch(() => ue("en"));
				e === Gn && (Wn.value = n.data);
			})();
		});
		let Kn = f(() => {
			let e = F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter, t = le({
				useUTC: e.useUTC,
				locale: Wn.value,
				januaryAsYear: e.januaryAsYear
			});
			return (e, n) => {
				let r = L.value.map((e) => e[0])?.[e];
				return r == null ? "" : t.formatDate(new Date(r), n);
			};
		}), qn = f(() => (L.value.map((e) => e[0]) || []).map((e, t) => ({
			text: Kn.value(t, F.value.style.tooltip.timeFormat),
			absoluteIndex: t
		}))), Jn = f(() => (L.value.map((e) => e[0]) || []).map((e, t) => ({
			text: Kn.value(t, F.value.style.zoom.timeFormat),
			absoluteIndex: t
		}))), Yn = f(() => {
			let e = F.value.style.layout.grid.xAxis.dataLabels, t = Q.value || [], n = Bn.value || [], r = K.value.start ?? 0, i = M.value, a = G.value, o = t.map((e) => e?.text ?? ""), s = n.map((e) => e?.text ?? "");
			return l(!!e.showOnlyFirstAndLast, !!e.showOnlyAtModulo, Math.max(1, Un.value || 1), o, s, r, i, a);
		}), Xn = f(() => F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable ? {
			start: Q.value.find((e) => e.absoluteIndex === K.value.start)?.text ?? "",
			end: Q.value.find((e) => e.absoluteIndex === K.value.end - 1)?.text ?? ""
		} : {
			start: L.value[K.value.start] ? L.value[K.value.start][0] : L.value[0][0],
			end: L.value[K.value.end - 1] ? L.value[K.value.end - 1][0] : L.value.at(-1)[0]
		}), Zn = C(null);
		function Qn(e, t) {
			F.value.events.datapointClick && F.value.events.datapointClick({
				datapoint: t,
				seriesIndex: e + K.value.start
			});
		}
		function $n(e, t) {
			F.value.events.datapointLeave && F.value.events.datapointLeave({
				datapoint: t,
				seriesIndex: e + K.value.start
			}), M.value = void 0, Rt.value = !1, P.value = null, tn.value = "pointer";
		}
		He(() => k.selectedXIndex, (e) => {
			if ([null, void 0].includes(k.selectedXIndex)) {
				M.value = null;
				return;
			}
			let t = e - K.value.start;
			t < 0 || e >= K.value.end ? M.value = null : M.value = t ?? null;
		}, { immediate: !0 });
		function er(e, t, n = "pointer") {
			F.value.events.datapointEnter && F.value.events.datapointEnter({
				datapoint: t,
				seriesIndex: e + K.value.start
			}), M.value = e, P.value = e, tn.value = n, Zn.value = {
				datapoint: t,
				seriesIndex: e,
				series: Z.value,
				config: F.value
			}, Ar({
				seriesIndex: e,
				datapoint: t
			});
			let r = F.value.style.tooltip.customFormat;
			if (oe(r) && ee(() => r({
				seriesIndex: e,
				datapoint: t,
				series: Z.value,
				config: F.value
			}))) zt.value = r({
				seriesIndex: e,
				datapoint: t,
				series: Z.value,
				config: F.value
			});
			else if (F.value.style.tooltip.show) {
				let n = "", { period: r, open: i, high: a, low: s, close: c, volume: l, isBullish: ee } = Z.value[e], { period: te, open: ne, high: re, low: ie, close: ae, volume: u } = F.value.translations, oe = F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable ? F.value.style.tooltip.useDefaultTimeFormat ? Q.value[e].text : qn.value[e].text : r;
				n += `<div><svg style="margin-right:6px" viewBox="0 0 12 12" height="12" width="12"><rect x="0" y="0" height="12" width="12" rx="${F.value.style.layout.candle.borderRadius * 3}" stroke="${F.value.style.layout.candle.stroke}" stroke-width="${F.value.style.layout.candle.strokeWidth}" 
                fill="${F.value.style.layout.candle.gradient.show ? ee ? `url(#bullish_gradient_${j.value})` : `url(#bearish_gradient_${j.value})` : ee ? F.value.style.layout.candle.colors.bullish : F.value.style.layout.candle.colors.bearish}"/></svg>${oe}</div>`, n += `${u} : <b>${isNaN(l) ? "-" : Number(l.toFixed(F.value.style.tooltip.roundingValue)).toLocaleString()}</b>`, n += `<div style="margin-top:6px;padding-top:6px;border-top:1px solid ${F.value.style.tooltip.borderColor}">`;
				let se = o({
					p: F.value.style.tooltip.prefix,
					v: i.value,
					s: F.value.style.tooltip.suffix,
					r: F.value.style.tooltip.roundingValue
				}), ce = o({
					p: F.value.style.tooltip.prefix,
					v: a.value,
					s: F.value.style.tooltip.suffix,
					r: F.value.style.tooltip.roundingValue
				}), le = o({
					p: F.value.style.tooltip.prefix,
					v: s.value,
					s: F.value.style.tooltip.suffix,
					r: F.value.style.tooltip.roundingValue
				}), ue = o({
					p: F.value.style.tooltip.prefix,
					v: c.value,
					s: F.value.style.tooltip.suffix,
					r: F.value.style.tooltip.roundingValue
				});
				F.value.style.tooltip.showChart ? n += `<div style="width:100%;display:flex;align-items:center;justify-content:center;">
                    <svg viewBox="0 0 100 100" width="100px" style="background: transparent; overflow: visible">
                        <g>
                            <line x1="50" x2="50" y1="20" y2="80" stroke="${t.isBullish ? F.value.style.layout.candle.colors.bullish : F.value.style.layout.candle.colors.bearish}" stroke-width="2" stroke-linecap="round" />
                            ${t.isBullish ? `
                                <line x1="45" x2="50" y1="65" y2="65" stroke="${F.value.style.layout.candle.colors.bullish}" stroke-width="1.5" stroke-linecap="round" />
                                <line x1="50" x2="55" y1="35" y2="35" stroke="${F.value.style.layout.candle.colors.bullish}" stroke-width="1.5" stroke-linecap="round" />
                                <text x="38" y="70" text-anchor="end" fill="${F.value.style.tooltip.color}">${se}</text>
                                <text x="62" y="40" text-anchor="start" fill="${F.value.style.tooltip.color}">${ue}</text>
                            ` : `
                                <line x1="45" x2="50" y1="35" y2="35" stroke="${F.value.style.layout.candle.colors.bearish}" stroke-width="1.5" stroke-linecap="round" />
                                <line x1="50" x2="55" y1="65" y2="65" stroke="${F.value.style.layout.candle.colors.bearish}" stroke-width="1.5" stroke-linecap="round" />
                                <text x="40" y="40" text-anchor="end" fill="${F.value.style.tooltip.color}">${se}</text>
                                <text x="60" y="70" text-anchor="start" fill="${F.value.style.tooltip.color}">${ue}</text>
                            `}
                            <text x="50" y="13" text-anchor="middle" fill="${F.value.style.tooltip.color}">${ce}</text>
                            <text x="50" y="97" text-anchor="middle" fill="${F.value.style.tooltip.color}">${le}</text>
                        <g>
                    </svg>
                    <div>
                ` : (n += `<div>${ne}: <b>${se}</b></div>`, n += `<div>${re}: <b>${ce}</b></div>`, n += `<div>${ie}: <b>${le}</b></div>`, n += `<div>${ae}: <b>${ue}</b></div>`), n += "</div>", zt.value = `<div style="text-align:right">${n}</div>`;
			}
			Rt.value = !0;
		}
		let tr = C(null);
		function nr() {
			return new Promise((e) => requestAnimationFrame(() => requestAnimationFrame(() => e())));
		}
		Le(() => {
			tr.value && cancelAnimationFrame(tr.value);
		});
		async function rr() {
			sr(), await Ie(), tr.value && cancelAnimationFrame(tr.value), tr.value = requestAnimationFrame(async () => {
				await nr(), sr();
			});
		}
		let ir = C(!1), ar = C(!1), or = C(!1);
		function sr() {
			if (!ir.value) {
				ir.value = !0;
				try {
					let { startIndex: e, endIndex: t } = F.value.style.zoom, n = G.value, r = e ?? 0, i = t == null ? n : Math.min(cr(t + 1), n);
					or.value = !0, K.value.start = r, K.value.end = i, q.value.start = r, q.value.end = i, On(), ar.value = !0;
				} finally {
					queueMicrotask(() => {
						or.value = !1;
					}), ir.value = !1;
				}
			}
		}
		function cr(e) {
			let t = G.value;
			return e > t ? t : e < 0 || e < K.value.start ? F.value.style.zoom.startIndex === null ? 1 : F.value.style.zoom.startIndex + 1 : e;
		}
		function lr(e = null) {
			Ie(() => {
				let n = [
					F.value.translations.period,
					F.value.translations.open,
					F.value.translations.high,
					F.value.translations.low,
					F.value.translations.close,
					F.value.translations.volume
				], i = Z.value.map((e, t) => [
					F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable ? Q.value[t].text : e.period,
					e.open.value,
					e.high.value,
					e.low.value,
					e.close.value,
					e.volume
				]), a = [
					[F.value.style.title.text],
					[F.value.style.title.subtitle.text],
					[
						[""],
						[""],
						[""]
					],
					n
				].concat(i), o = r(a);
				e ? e(o) : t({
					csvContent: o,
					title: F.value.style.title.text || "vue-ui-candlestick"
				});
			});
		}
		let ur = f(() => {
			let e = Z.value.map((e, t) => {
				let n = F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable ? Q.value?.[t]?.text ?? "" : e.period, r = o({
					p: F.value.table.td.prefix,
					v: e.open.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				}), i = o({
					p: F.value.table.td.prefix,
					v: e.high.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				}), a = o({
					p: F.value.table.td.prefix,
					v: e.low.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				}), s = o({
					p: F.value.table.td.prefix,
					v: e.close.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				});
				return [
					`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 12 12" height="12" width="12" style="margin-right: 6px"><rect x="0" y="0" height="12" width="12" rx="${F.value.style.layout.candle.borderRadius * 3}" fill="${F.value.style.layout.candle.gradient.show ? e.isBullish ? `url(#bullish_gradient_${j.value})` : `url(#bearish_gradient_${j.value})` : e.isBullish ? F.value.style.layout.candle.colors.bullish : F.value.style.layout.candle.colors.bearish}"/></svg> ${n}`,
					r,
					i,
					a,
					s,
					`${isNaN(e.volume) ? "-" : e.volume.toLocaleString()}`
				];
			}), t = Z.value.map((e, t) => [
				F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable ? Q.value?.[t]?.text ?? "" : e.period,
				o({
					p: F.value.table.td.prefix,
					v: e.open.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				}),
				o({
					p: F.value.table.td.prefix,
					v: e.high.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				}),
				o({
					p: F.value.table.td.prefix,
					v: e.low.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				}),
				o({
					p: F.value.table.td.prefix,
					v: e.close.value,
					s: F.value.table.td.suffix,
					r: F.value.table.td.roundingValue
				}),
				`${isNaN(e.volume) ? "-" : e.volume}`
			]), n = {
				th: {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					outline: F.value.table.th.outline
				},
				td: {
					backgroundColor: F.value.table.td.backgroundColor,
					color: F.value.table.td.color,
					outline: F.value.table.td.outline
				},
				breakpoint: F.value.table.responsiveBreakpoint
			}, r = [
				F.value.translations.period,
				F.value.translations.open,
				F.value.translations.high,
				F.value.translations.low,
				F.value.translations.last,
				F.value.translations.volume
			];
			return {
				head: r,
				body: e,
				bodyA11y: t,
				config: n,
				colNames: r
			};
		}), $ = C(!1);
		function dr(e) {
			$.value = e, Bt.value += 1;
		}
		function fr() {
			H.value.showTable = !H.value.showTable;
		}
		function pr() {
			H.value.showTooltip = !H.value.showTooltip;
		}
		let mr = C(!1);
		function hr() {
			mr.value = !mr.value;
		}
		async function gr({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { imageUri: t, base64: n } = await xe({
				domElement: N.value,
				base64: !0,
				img: !0,
				scale: e
			}), r = N.value.getBoundingClientRect(), i = {
				width: r.width,
				height: r.height,
				aspectRatio: r.height ? r.width / r.height : 0
			}, a = await te(t, e) ?? i;
			return {
				imageUri: t,
				base64: n,
				title: F.value.style.title.text,
				...a
			};
		}
		let _r = f(() => z.value.width), vr = f(() => z.value.height);
		be({
			timeLabelsEls: Xt,
			timeLabels: Q,
			slicer: K,
			configRef: F,
			rotationPath: [
				"style",
				"layout",
				"grid",
				"xAxis",
				"dataLabels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"layout",
				"grid",
				"xAxis",
				"dataLabels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			rotation: F.value.style.layout.grid.xAxis.dataLabels.autoRotate.angle,
			width: _r,
			height: vr
		}), He(F, () => {
			H.value = {
				showTable: F.value.table.show,
				showTooltip: F.value.style.tooltip.show
			};
		}, { immediate: !0 });
		let yr = f(() => {
			let e = F.value.table.useDialog && !F.value.table.show, t = H.value.showTable;
			return {
				component: e ? jt : Et,
				title: `${F.value.style.title.text}${F.value.style.title.subtitle.text ? `: ${F.value.style.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					headerColor: F.value.table.th.color,
					headerBg: F.value.table.th.backgroundColor,
					isFullscreen: $.value,
					fullscreenParent: N.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: an.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: F.value.style.backgroundColor,
							color: F.value.style.color
						},
						head: {
							backgroundColor: F.value.style.backgroundColor,
							color: F.value.style.color
						}
					}
				}
			};
		});
		He(() => H.value.showTable, (e) => {
			F.value.table.show || (e && F.value.table.useDialog && Zt.value ? Zt.value.open() : "close" in Zt.value && Zt.value.close());
		});
		function br() {
			H.value.showTable = !1, Qt.value && Qt.value.setTableIconState(!1);
		}
		let xr = f(() => F.value.style.backgroundColor), Sr = f(() => F.value.style.title), { isCallbackImaging: Cr, isCallbackSvg: wr, generateSvg: Tr, onGenerateImage: Er } = ve({
			svg: R,
			title: Sr,
			legend: null,
			legendItems: null,
			backgroundColor: xr,
			getSvgCallback: () => F.value.userOptions.callbacks.svg,
			generateImage: yn
		});
		function Dr(e) {
			$t.value = e;
		}
		function Or(e) {
			ir.value || or.value || e !== K.value.start && (K.value.start = e, q.value.start = e, On());
		}
		function kr(e) {
			if (ir.value || or.value) return;
			let t = cr(e);
			t !== K.value.end && (K.value.end = t, q.value.end = t, On());
		}
		function Ar({ seriesIndex: e, datapoint: t }) {
			let n = K.value.start + e;
			Ft("selectX", {
				dataset: t,
				index: n,
				indexLabel: ""
			});
		}
		async function jr() {
			if (Ft("copyAlt", {
				config: F.value,
				dataset: Z.value
			}), !F.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(F.value.userOptions.callbacks.altCopy({
				config: F.value,
				dataset: Z.value
			}));
		}
		function Mr() {
			P.value = null, nn.value = !0;
		}
		function Nr() {
			P.value = null, tn.value = "pointer", Rt.value = !1, M.value = void 0, nn.value = !1;
		}
		function Pr(e) {
			if (!R.value || mr.value || document.activeElement !== R.value || !Z.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				P.value = null, tn.value = "pointer", Rt.value = !1, M.value = void 0;
				return;
			}
			if (r) {
				if (P.value === null) return;
				let e = Z.value[P.value];
				if (!e) return;
				Qn(P.value, e);
				return;
			}
			let a = P.value, o = M.value, s = a !== null && a >= 0 && a < Z.value.length, c = o != null && o >= 0 && o < Z.value.length;
			s ? n ? (a += 1, a >= Z.value.length && (a = 0)) : t && (--a, a < 0 && (a = Z.value.length - 1)) : c ? (a = n ? o + 1 : o - 1, a >= Z.value.length && (a = 0), a < 0 && (a = Z.value.length - 1)) : a = n ? 0 : Z.value.length - 1;
			let l = Z.value[a];
			l && (Fr(a), er(a, l, "keyboard"));
		}
		function Fr(e) {
			if (!Number.isFinite(e) || !R.value) return;
			let t = W.value.left + Y.value * e + Y.value / 2, n = W.value.top + W.value.height / 2, r = R.value.getBoundingClientRect();
			en.value = {
				x: r.left + t / z.value.width * r.width,
				y: r.top + n / z.value.height * r.height
			};
		}
		let Ir = f(() => ({
			headers: ur.value?.colNames ?? [],
			rows: ur.value?.bodyA11y ?? []
		}));
		return Ce({
			getImage: gr,
			generatePdf: vn,
			generateCsv: lr,
			generateImage: yn,
			generateSvg: Tr,
			toggleTable: fr,
			toggleTooltip: pr,
			toggleAnnotator: hr,
			toggleFullscreen: dr,
			copyAlt: jr
		}), (e, t) => (S(), h("div", {
			ref_key: "candlestickChart",
			ref: N,
			class: y(`vue-data-ui-component vue-ui-candlestick ${$.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${F.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: x(`position:relative;font-family:${F.value.style.fontFamily}; text-align:center;background:${F.value.style.backgroundColor}; ${F.value.responsive ? "height: 100%" : ""}`),
			id: `vue-ui-candlestick_${j.value}`,
			onMouseenter: dn,
			onMouseleave: fn
		}, [
			g("div", {
				id: `chart-instructions-${j.value}`,
				class: "sr-only"
			}, [g("p", null, E(F.value.a11y.translations.keyboardNavigation), 1)], 8, Ke),
			Ir.value?.rows?.length ? (S(), p(Oe, {
				key: 0,
				uid: j.value,
				head: Ir.value.headers,
				body: Ir.value.rows,
				notice: F.value.a11y.translations.tableAvailable,
				caption: F.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : m("", !0),
			F.value.userOptions.buttons.annotator ? (S(), p(D(Ot), {
				key: 1,
				svgRef: D(R),
				backgroundColor: F.value.style.backgroundColor,
				color: F.value.style.color,
				active: mr.value,
				isCursorPointer: an.value,
				onClose: hr
			}, {
				"annotator-action-close": O(() => [T(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": O(({ color: t }) => [T(e.$slots, "annotator-action-color", b(v({ color: t })), void 0, !0)]),
				"annotator-action-draw": O(({ mode: t }) => [T(e.$slots, "annotator-action-draw", b(v({ mode: t })), void 0, !0)]),
				"annotator-action-undo": O(({ disabled: t }) => [T(e.$slots, "annotator-action-undo", b(v({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": O(({ disabled: t }) => [T(e.$slots, "annotator-action-redo", b(v({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": O(({ disabled: t }) => [T(e.$slots, "annotator-action-delete", b(v({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : m("", !0),
			bn.value ? (S(), h("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Gt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : m("", !0),
			F.value.style.title.text ? (S(), h("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Vt,
				style: "width:100%;background:transparent"
			}, [(S(), p(Se, {
				key: `title_${Jt.value}`,
				config: {
					title: {
						cy: "candlestick-div-title",
						...F.value.style.title
					},
					subtitle: {
						cy: "candlestick-div-subtitle",
						...F.value.style.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : m("", !0),
			F.value.userOptions.show && It.value && (D(un) || D(cn)) ? (S(), p(D(kt), {
				ref_key: "userOptionsRef",
				ref: Qt,
				key: `user_options_${Bt.value}`,
				backgroundColor: F.value.style.backgroundColor,
				color: F.value.style.color,
				isImaging: D(_n),
				isPrinting: D(gn),
				uid: j.value,
				hasTooltip: F.value.userOptions.buttons.tooltip && F.value.style.tooltip.show,
				hasPdf: F.value.userOptions.buttons.pdf,
				hasImg: F.value.userOptions.buttons.img,
				hasSvg: F.value.userOptions.buttons.svg,
				hasXls: F.value.userOptions.buttons.csv,
				hasTable: F.value.userOptions.buttons.table,
				hasFullscreen: F.value.userOptions.buttons.fullscreen,
				hasAltCopy: F.value.userOptions.buttons.altCopy,
				isFullscreen: $.value,
				isTooltip: H.value.showTooltip,
				titles: { ...F.value.userOptions.buttonTitles },
				chartElement: N.value,
				position: F.value.userOptions.position,
				hasAnnotator: F.value.userOptions.buttons.annotator,
				isAnnotation: mr.value,
				callbacks: F.value.userOptions.callbacks,
				printScale: F.value.userOptions.print.scale,
				tableDialog: F.value.table.useDialog,
				isCursorPointer: an.value,
				onToggleFullscreen: dr,
				onGeneratePdf: D(vn),
				onGenerateCsv: lr,
				onGenerateImage: D(Er),
				onGenerateSvg: D(Tr),
				onToggleTable: fr,
				onToggleTooltip: pr,
				onToggleAnnotator: hr,
				onCopyAlt: jr,
				style: x({ visibility: D(un) ? D(cn) ? "visible" : "hidden" : "visible" })
			}, Me({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: O(({ isOpen: t, color: n }) => [T(e.$slots, "menuIcon", b(v({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: O(() => [T(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: O(() => [T(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: O(() => [T(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: O(() => [T(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: O(() => [T(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: O(() => [T(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: O(({ toggleFullscreen: t, isFullscreen: n }) => [T(e.$slots, "optionFullscreen", b(v({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: O(({ toggleAnnotator: t, isAnnotator: n }) => [T(e.$slots, "optionAnnotator", b(v({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: O(({ altCopy: t }) => [T(e.$slots, "optionAltCopy", b(v({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: O(() => [T(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: O(() => [T(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isImaging.isPrinting.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : m("", !0),
			g("div", qe, [(S(), h("svg", {
				ref_key: "svgRef",
				ref: R,
				xmlns: D(ae),
				"aria-describedby": `chart-instructions-${j.value}`,
				class: y({
					"vue-data-ui-fullscreen--on": $.value,
					"vue-data-ui-fulscreen--off": !$.value,
					"vue-data-ui-no-transition": !D(rn)
				}),
				viewBox: `0 0 ${z.value.width <= 0 ? 10 : z.value.width} ${z.value.height <= 0 ? 10 : z.value.height}`,
				style: x(`max-width:100%;overflow:visible;background:transparent;color:${F.value.style.color}`),
				tabindex: "0",
				onFocus: Mr,
				onBlur: Nr,
				onKeydown: Pr
			}, [
				Pe(D(At)),
				e.$slots["chart-background"] ? (S(), h("foreignObject", {
					key: 0,
					x: W.value.left,
					y: W.value.top,
					width: Math.max(.1, W.value.width),
					height: Math.max(.1, W.value.height),
					style: { pointerEvents: "none" }
				}, [T(e.$slots, "chart-background", {}, void 0, !0)], 8, Ye)) : m("", !0),
				Z.value.length > 0 ? (S(), h("g", Xe, [
					g("defs", null, [Pe(Ee, {
						t: "linear",
						id: `bearish_gradient_${j.value}`,
						x2: "0%",
						y2: "100%",
						stops: [
							[
								"0%",
								F.value.style.layout.candle.colors.bearish,
								1
							],
							[
								"50%",
								D(a)(F.value.style.layout.candle.colors.bearish, .02),
								.87
							],
							[
								"100%",
								D(a)(F.value.style.layout.candle.colors.bearish, .05),
								.4
							]
						]
					}, null, 8, ["id", "stops"]), Pe(Ee, {
						t: "linear",
						id: `bullish_gradient_${j.value}`,
						x2: "0%",
						y2: "100%",
						stops: [
							[
								"0%",
								F.value.style.layout.candle.colors.bullish,
								1
							],
							[
								"50%",
								D(a)(F.value.style.layout.candle.colors.bullish, .02),
								.87
							],
							[
								"100%",
								D(a)(F.value.style.layout.candle.colors.bullish, .05),
								.4
							]
						]
					}, null, 8, ["id", "stops"])]),
					F.value.style.layout.grid.show ? (S(), h("g", Ze, [
						g("line", {
							x1: U.value ? W.value.right : W.value.left,
							x2: U.value ? W.value.right : W.value.left,
							y1: W.value.top,
							y2: W.value.bottom,
							stroke: F.value.style.layout.grid.stroke,
							"stroke-width": F.value.style.layout.grid.strokeWidth,
							"stroke-linecap": "round"
						}, null, 8, Qe),
						g("line", {
							x1: W.value.left,
							x2: W.value.right,
							y1: W.value.bottom,
							y2: W.value.bottom,
							stroke: F.value.style.layout.grid.stroke,
							"stroke-width": F.value.style.layout.grid.strokeWidth,
							"stroke-linecap": "round"
						}, null, 8, $e),
						F.value.style.layout.grid.horizontalLines.show ? (S(!0), h(d, { key: 0 }, w(Rn.value, (e) => (S(), h("line", {
							x1: W.value.left,
							x2: W.value.right,
							y1: e.y,
							y2: e.y,
							stroke: F.value.style.layout.grid.horizontalLines.stroke,
							"stroke-width": F.value.style.layout.grid.horizontalLines.strokeWidth,
							"stroke-dasharray": F.value.style.layout.grid.horizontalLines.strokeDasharray,
							"stroke-linecap": "round"
						}, null, 8, et))), 256)) : m("", !0),
						F.value.style.layout.grid.verticalLines.show ? (S(!0), h(d, { key: 1 }, w(Yn.value, (e, t) => (S(), h("g", null, [e.text ? (S(), h("line", {
							key: 0,
							x1: W.value.left + Y.value * t + Y.value / 2,
							x2: W.value.left + Y.value * t + Y.value / 2,
							y1: W.value.top,
							y2: W.value.bottom,
							stroke: F.value.style.layout.grid.verticalLines.stroke,
							"stroke-width": F.value.style.layout.grid.verticalLines.strokeWidth,
							"stroke-dasharray": F.value.style.layout.grid.verticalLines.strokeDasharray,
							"stroke-linecap": "round"
						}, null, 8, tt)) : m("", !0)]))), 256)) : m("", !0),
						F.value.style.layout.grid.xAxis.ticks.show ? (S(!0), h(d, { key: 2 }, w(Yn.value, (e, t) => (S(), h("g", null, [e.text ? (S(), h("line", {
							key: 0,
							x1: W.value.left + Y.value * t + Y.value / 2,
							x2: W.value.left + Y.value * t + Y.value / 2,
							y1: W.value.bottom,
							y2: W.value.bottom + 3,
							stroke: F.value.style.layout.grid.stroke,
							"stroke-width": F.value.style.layout.grid.strokeWidth,
							"stroke-linecap": "round"
						}, null, 8, nt)) : m("", !0)]))), 256)) : m("", !0)
					])) : m("", !0),
					F.value.style.layout.grid.yAxis.dataLabels.show ? (S(), h("g", {
						key: 1,
						ref_key: "scaleLabels",
						ref: Yt
					}, [(S(!0), h(d, null, w(Rn.value, (e, t) => (S(), h("g", { key: `sl_${t}` }, [e.value >= X.value.min && e.value <= X.value.max ? (S(), h("line", {
						key: 0,
						x1: U.value ? W.value.right : W.value.left,
						x2: U.value ? W.value.right + 5 : W.value.left - 5,
						y1: e.y,
						y2: e.y,
						stroke: F.value.style.layout.grid.stroke,
						"stroke-width": F.value.style.layout.grid.strokeWidth,
						"stroke-linecap": "round"
					}, null, 8, rt)) : m("", !0), e.value >= X.value.min && e.value <= X.value.max ? (S(), h("text", {
						key: 1,
						class: y({ "vue-data-ui-transition": D(rn) }),
						transform: `translate(${U.value ? W.value.right + 8 + F.value.style.layout.grid.yAxis.dataLabels.offsetX : W.value.left - 8 + F.value.style.layout.grid.yAxis.dataLabels.offsetX}, ${e.y + z.value.yAxisFontSize / 3})`,
						"text-anchor": U.value ? "start" : "end",
						"font-size": z.value.yAxisFontSize,
						fill: F.value.style.layout.grid.yAxis.dataLabels.color,
						"font-weight": F.value.style.layout.grid.yAxis.dataLabels.bold ? "bold" : "normal"
					}, E(D(o)({
						p: F.value.style.layout.grid.yAxis.dataLabels.prefix,
						v: e.value,
						s: F.value.style.layout.grid.yAxis.dataLabels.suffix,
						r: F.value.style.layout.grid.yAxis.dataLabels.roundingValue
					})), 11, it)) : m("", !0)]))), 128))], 512)) : m("", !0),
					F.value.style.layout.grid.xAxis.dataLabels.show && !F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable ? (S(), h("g", {
						key: 2,
						ref_key: "timeLabelsEls",
						ref: Xt
					}, [(S(!0), h(d, null, w(zn.value, (e, t) => (S(), h("g", null, [g("text", {
						class: "vue-data-ui-time-label",
						transform: `translate(${W.value.left + Y.value * t + Y.value / 2}, ${W.value.bottom + z.value.xAxisFontSize * 1.5}), rotate(${F.value.style.layout.grid.xAxis.dataLabels.rotation})`,
						"text-anchor": F.value.style.layout.grid.xAxis.dataLabels.rotation > 0 ? "start" : F.value.style.layout.grid.xAxis.dataLabels.rotation < 0 ? "end" : "middle",
						"font-size": z.value.xAxisFontSize,
						fill: F.value.style.layout.grid.xAxis.dataLabels.color,
						"font-weight": F.value.style.layout.grid.xAxis.dataLabels.bold ? "bold" : "normal"
					}, E(e), 9, at)]))), 256))], 512)) : m("", !0),
					F.value.style.layout.grid.xAxis.dataLabels.show && F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable ? (S(), h("g", {
						key: 3,
						ref_key: "timeLabelsEls",
						ref: Xt
					}, [(S(!0), h(d, null, w(Yn.value, (e, t) => (S(), h("g", null, [g("text", {
						class: "vue-data-ui-time-label",
						transform: `translate(${W.value.left + Y.value * t + Y.value / 2}, ${W.value.bottom + z.value.xAxisFontSize * 1.5}), rotate(${F.value.style.layout.grid.xAxis.dataLabels.rotation})`,
						"text-anchor": F.value.style.layout.grid.xAxis.dataLabels.rotation > 0 ? "start" : F.value.style.layout.grid.xAxis.dataLabels.rotation < 0 ? "end" : "middle",
						"font-size": z.value.xAxisFontSize,
						fill: F.value.style.layout.grid.xAxis.dataLabels.color,
						"font-weight": F.value.style.layout.grid.xAxis.dataLabels.bold ? "bold" : "normal"
					}, E(e?.text ?? ""), 9, ot)]))), 256))], 512)) : m("", !0),
					F.value.type === "candlestick" ? (S(), h(d, { key: 4 }, [g("g", null, [(S(!0), h(d, null, w(Z.value, (e, t) => (S(), h("g", null, [
						g("rect", {
							x: e.open.x - F.value.style.layout.wick.strokeWidth / 2,
							y: e.high.y,
							width: F.value.style.layout.wick.strokeWidth,
							height: Math.abs(e.high.y - e.low.y),
							fill: F.value.style.layout.wick.stroke,
							stroke: "none",
							rx: F.value.style.layout.wick.strokeWidth / 2,
							class: y({ "vue-data-ui-transition": A.value && !D(I) })
						}, null, 10, st),
						F.value.style.layout.wick.extremity.shape === "circle" ? (S(), h("g", ct, [g("circle", {
							cx: e.high.x,
							cy: e.high.y,
							r: F.value.style.layout.wick.extremity.size === "auto" ? Y.value / 20 : F.value.style.layout.wick.extremity.size,
							fill: F.value.style.layout.wick.extremity.color,
							class: y({ "vue-data-ui-transition": A.value && !D(I) })
						}, null, 10, lt), g("circle", {
							cx: e.low.x,
							cy: e.low.y,
							r: F.value.style.layout.wick.extremity.size === "auto" ? Y.value / 20 : F.value.style.layout.wick.extremity.size,
							fill: F.value.style.layout.wick.extremity.color,
							class: y({ "vue-data-ui-transition": A.value && !D(I) })
						}, null, 10, ut)])) : m("", !0),
						F.value.style.layout.wick.extremity.shape === "line" ? (S(), h("g", dt, [g("rect", {
							x: e.high.x - (F.value.style.layout.wick.extremity.size === "auto" ? Y.value * F.value.style.layout.candle.widthRatio : F.value.style.layout.wick.extremity.size) / 2,
							y: e.high.y - F.value.style.layout.wick.strokeWidth / 2,
							width: Math.abs(e.high.x - (F.value.style.layout.wick.extremity.size === "auto" ? Y.value * F.value.style.layout.candle.widthRatio : F.value.style.layout.wick.extremity.size) / 2 - (e.high.x + (F.value.style.layout.wick.extremity.size === "auto" ? Y.value * F.value.style.layout.candle.widthRatio : F.value.style.layout.wick.extremity.size) / 2)),
							height: F.value.style.layout.wick.strokeWidth,
							rx: F.value.style.layout.wick.strokeWidth / 2,
							fill: F.value.style.layout.wick.extremity.color,
							stroke: "none",
							class: y({ "vue-data-ui-transition": A.value && !D(I) })
						}, null, 10, ft), g("rect", {
							x: e.low.x - (F.value.style.layout.wick.extremity.size === "auto" ? Y.value * F.value.style.layout.candle.widthRatio : F.value.style.layout.wick.extremity.size) / 2,
							y: e.low.y - F.value.style.layout.wick.strokeWidth / 2,
							width: Math.abs(e.low.x - (F.value.style.layout.wick.extremity.size === "auto" ? Y.value * F.value.style.layout.candle.widthRatio : F.value.style.layout.wick.extremity.size) / 2 - (e.low.x + (F.value.style.layout.wick.extremity.size === "auto" ? Y.value * F.value.style.layout.candle.widthRatio : F.value.style.layout.wick.extremity.size) / 2)),
							height: F.value.style.layout.wick.strokeWidth,
							fill: F.value.style.layout.wick.extremity.color,
							stroke: "none",
							rx: F.value.style.layout.wick.strokeWidth / 2,
							class: y({ "vue-data-ui-transition": A.value && !D(I) })
						}, null, 10, pt)])) : m("", !0)
					]))), 256))]), g("g", null, [(S(!0), h(d, null, w(Z.value, (e, t) => (S(), h("rect", {
						x: e.open.x - Y.value / 2 + Y.value * (1 - F.value.style.layout.candle.widthRatio) / 2,
						y: e.isBullish ? e.close.y : e.open.y,
						height: Math.abs(e.close.y - e.open.y) <= 0 ? 1e-4 : Math.abs(e.close.y - e.open.y),
						width: Y.value * F.value.style.layout.candle.widthRatio <= 0 ? 1e-4 : Y.value * F.value.style.layout.candle.widthRatio,
						fill: F.value.style.layout.candle.gradient.underlayer,
						rx: F.value.style.layout.candle.borderRadius,
						stroke: "none",
						class: y({ "vue-data-ui-transition": A.value && !D(I) })
					}, null, 10, mt))), 256)), (S(!0), h(d, null, w(Z.value, (e, t) => (S(), h("rect", {
						x: e.open.x - Y.value / 2 + Y.value * (1 - F.value.style.layout.candle.widthRatio) / 2,
						y: e.isBullish ? e.close.y : e.open.y,
						height: Math.abs(e.close.y - e.open.y) <= 0 ? 1e-4 : Math.abs(e.close.y - e.open.y),
						width: Y.value * F.value.style.layout.candle.widthRatio <= 0 ? 1e-4 : Y.value * F.value.style.layout.candle.widthRatio,
						fill: e.isBullish ? F.value.style.layout.candle.gradient.show ? `url(#bullish_gradient_${j.value})` : F.value.style.layout.candle.colors.bullish : F.value.style.layout.candle.gradient.show ? `url(#bearish_gradient_${j.value})` : F.value.style.layout.candle.colors.bearish,
						rx: F.value.style.layout.candle.borderRadius,
						stroke: F.value.style.layout.candle.stroke,
						"stroke-width": F.value.style.layout.candle.strokeWidth,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						class: y({ "vue-data-ui-transition": A.value && !D(I) })
					}, null, 10, ht))), 256))])], 64)) : m("", !0),
					F.value.type === "ohlc" ? (S(!0), h(d, { key: 5 }, w(Z.value, (e, t) => (S(), h("g", { key: `ohlc_${e.absoluteIndex}` }, [g("path", {
						d: `M ${e.high.x},${e.high.y} ${e.low.x},${e.low.y} M${e.open.x - Math.min(6, Y.value / 3)},${e.open.y} ${e.open.x},${e.open.y} M${e.close.x},${e.close.y} ${e.close.x + Math.min(6, Y.value / 3)},${e.close.y}`,
						stroke: e.isBullish ? F.value.style.layout.candle.colors.bullish : F.value.style.layout.candle.colors.bearish,
						"stroke-width": 1,
						class: y({ "vue-data-ui-transition": D(rn) }),
						"stroke-linecap": "round",
						"stroke-linejoin": "round"
					}, null, 10, gt)]))), 128)) : m("", !0),
					g("g", null, [(S(!0), h(d, null, w(Z.value, (e, t) => (S(), h("rect", {
						x: W.value.left + t * Y.value,
						y: W.value.top,
						height: W.value.height <= 0 ? 1e-4 : W.value.height,
						width: Y.value <= 0 ? 1e-4 : Y.value,
						fill: M.value === t || $t.value === t ? D(n)(F.value.style.layout.selector.color, F.value.style.layout.selector.opacity) : "transparent",
						onMouseover: () => er(t, e, "pointer"),
						onMouseleave: () => $n(t, e),
						onClick: () => Qn(t, e)
					}, null, 40, _t))), 256))])
				])) : m("", !0),
				En.value ? (S(), h("rect", Fe({ key: 2 }, Tn.value, {
					"data-start": K.value.start,
					"data-end": K.value.end
				}), null, 16, vt)) : m("", !0),
				T(e.$slots, "svg", { svg: {
					...z.value,
					data: Z.value,
					drawingArea: W.value,
					isPrintingImg: D(gn) || D(_n) || D(Cr),
					isPrintingSvg: D(wr)
				} }, void 0, !0)
			], 46, Je)), e.$slots.hint ? (S(), h("div", yt, [T(e.$slots, "hint", b(v({
				hint: F.value.a11y.translations.keyboardNavigation,
				isVisible: nn.value
			})), void 0, !0)])) : m("", !0)]),
			e.$slots.watermark ? (S(), h("div", bt, [T(e.$slots, "watermark", b(v({ isPrinting: D(gn) || D(_n) || D(Cr) || D(wr) })), void 0, !0)])) : m("", !0),
			F.value.style.zoom.show && G.value > 6 && It.value && ar.value ? (S(), p(De, {
				key: 6,
				ref_key: "chartSlicer",
				ref: Ut,
				"data-dom-to-png-ignore-layout": "",
				allMinimaps: In.value,
				background: F.value.style.zoom.color,
				borderColor: F.value.style.backgroundColor,
				customFormat: F.value.style.zoom.customFormat,
				cutNullValues: !1,
				enableRangeHandles: F.value.style.zoom.enableRangeHandles,
				enableSelectionDrag: F.value.style.zoom.enableSelectionDrag,
				end: K.value.end,
				focusOnDrag: F.value.style.zoom.focusOnDrag,
				focusRangeRatio: F.value.style.zoom.focusRangeRatio,
				fontSize: F.value.style.zoom.fontSize,
				immediate: !F.value.style.zoom.preview.enable,
				inputColor: F.value.style.zoom.color,
				isPreview: En.value,
				labelLeft: Xn.value.start || "",
				labelRight: Xn.value.end || "",
				max: G.value,
				min: 0,
				minimap: F.value.style.zoom.minimap.show ? D(L).map((e) => e[2]) : [],
				minimapCompact: F.value.style.zoom.minimap.compact,
				minimapFrameColor: F.value.style.zoom.minimap.frameColor,
				minimapIndicatorColor: F.value.style.zoom.minimap.indicatorColor,
				minimapMerged: !1,
				minimapSelectedColor: F.value.style.zoom.minimap.selectedColor,
				minimapSelectedColorOpacity: F.value.style.zoom.minimap.selectedColorOpacity,
				minimapSelectedIndex: M.value,
				minimapSelectionRadius: 1,
				preciseLabels: Jn.value,
				refreshEndPoint: F.value.style.zoom.endIndex === null ? G.value : F.value.style.zoom.endIndex + 1,
				refreshStartPoint: F.value.style.zoom.startIndex === null ? 0 : F.value.style.zoom.startIndex,
				selectColor: F.value.style.zoom.highlightColor,
				selectedSeries: D(L),
				smoothMinimap: !1,
				start: K.value.start,
				textColor: F.value.style.color,
				timeLabels: Bn.value,
				usePreciseLabels: F.value.style.layout.grid.xAxis.dataLabels.datetimeFormatter.enable && !F.value.style.zoom.useDefaultFormat,
				useResetSlot: F.value.style.zoom.useResetSlot,
				valueEnd: K.value.end,
				valueStart: K.value.start,
				verticalHandles: F.value.style.zoom.minimap.verticalHandles,
				minScale: F.value.style.layout.grid.yAxis.scale.min,
				maxScale: F.value.style.layout.grid.yAxis.scale.max,
				maxWidth: F.value.style.zoom.maxWidth,
				minimapLeftInsetRatio: z.value.width > 0 && F.value.style.zoom.autoFit ? W.value.left / z.value.width : null,
				minimapRightInsetRatio: z.value.width > 0 && F.value.style.zoom.autoFit ? (z.value.width - W.value.right) / z.value.width : null,
				additionalMinimapHeight: F.value.style.zoom.minimap.additionalHeight,
				handleType: F.value.style.zoom.minimap.handleType,
				handleIconColor: F.value.style.zoom.minimap.handleIconColor,
				handleBorderWidth: F.value.style.zoom.minimap.handleBorderWidth,
				handleBorderColor: F.value.style.zoom.minimap.handleBorderColor,
				handleFill: F.value.style.zoom.minimap.handleFill,
				handleWidth: F.value.style.zoom.minimap.handleWidth,
				"onUpdate:end": kr,
				"onUpdate:start": Or,
				onTrapMouse: Dr,
				onReset: rr,
				onFutureEnd: t[0] ||= (e) => Dn("end", e),
				onFutureStart: t[1] ||= (e) => Dn("start", e)
			}, {
				"reset-action": O(({ reset: t }) => [T(e.$slots, "reset-action", b(v({ reset: t })), void 0, !0)]),
				slotMap: O(({ height: e, unitW: t }) => [(S(!0), h(d, null, w(Fn.value({
					minimapH: e,
					unitW: t
				}), (e, n) => (S(), h("g", null, [g("path", {
					d: `M ${e.high.x},${e.high.y} ${e.low.x},${e.low.y}`,
					stroke: e.isBullish ? F.value.style.layout.candle.colors.bullish : F.value.style.layout.candle.colors.bearish,
					"stroke-width": 1,
					style: x({ opacity: n >= q.value.start && n <= q.value.end ? 1 : .6 })
				}, null, 12, xt), g("path", {
					d: `M ${e.open.x},${e.open.y} ${e.close.x},${e.close.y}`,
					stroke: e.isBullish ? F.value.style.layout.candle.colors.bullish : F.value.style.layout.candle.colors.bearish,
					"stroke-width": Math.min(6, t / 1.5),
					style: x({ opacity: n >= q.value.start && n <= q.value.end ? 1 : .6 })
				}, null, 12, St)]))), 256))]),
				_: 3
			}, 8, /* @__PURE__ */ "allMinimaps.background.borderColor.customFormat.enableRangeHandles.enableSelectionDrag.end.focusOnDrag.focusRangeRatio.fontSize.immediate.inputColor.isPreview.labelLeft.labelRight.max.minimap.minimapCompact.minimapFrameColor.minimapIndicatorColor.minimapSelectedColor.minimapSelectedColorOpacity.minimapSelectedIndex.preciseLabels.refreshEndPoint.refreshStartPoint.selectColor.selectedSeries.start.textColor.timeLabels.usePreciseLabels.useResetSlot.valueEnd.valueStart.verticalHandles.minScale.maxScale.maxWidth.minimapLeftInsetRatio.minimapRightInsetRatio.additionalMinimapHeight.handleType.handleIconColor.handleBorderWidth.handleBorderColor.handleFill.handleWidth".split("."))) : m("", !0),
			g("div", {
				ref_key: "chartLegend",
				ref: Ht
			}, [T(e.$slots, "legend", { legend: Z.value }, void 0, !0)], 512),
			e.$slots.source ? (S(), h("div", {
				key: 7,
				ref_key: "source",
				ref: Wt,
				dir: "auto"
			}, [T(e.$slots, "source", {}, void 0, !0)], 512)) : m("", !0),
			Pe(D(Tt), {
				teleportTo: F.value.style.tooltip.teleportTo,
				show: H.value.showTooltip && Rt.value,
				backgroundColor: F.value.style.tooltip.backgroundColor,
				color: F.value.style.tooltip.color,
				borderRadius: F.value.style.tooltip.borderRadius,
				borderColor: F.value.style.tooltip.borderColor,
				borderWidth: F.value.style.tooltip.borderWidth,
				fontSize: F.value.style.tooltip.fontSize,
				backgroundOpacity: F.value.style.tooltip.backgroundOpacity,
				position: F.value.style.tooltip.position,
				offsetX: F.value.style.tooltip.offsetX,
				offsetY: F.value.style.tooltip.offsetY,
				parent: N.value,
				content: zt.value,
				isFullscreen: $.value,
				isCustom: F.value.style.tooltip.customFormat && typeof F.value.style.tooltip.customFormat == "function",
				smooth: F.value.style.tooltip.smooth,
				backdropFilter: F.value.style.tooltip.backdropFilter,
				smoothForce: F.value.style.tooltip.smoothForce,
				smoothSnapThreshold: F.value.style.tooltip.smoothSnapThreshold,
				isA11yMode: tn.value === "keyboard",
				a11yPosition: en.value
			}, {
				"tooltip-before": O(() => [T(e.$slots, "tooltip-before", b(v({ ...Zn.value })), void 0, !0)]),
				tooltip: O(() => [T(e.$slots, "tooltip", b(v({ ...Zn.value })), void 0, !0)]),
				"tooltip-after": O(() => [T(e.$slots, "tooltip-after", b(v({ ...Zn.value })), void 0, !0)]),
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
			It.value && F.value.userOptions.buttons.table ? (S(), p(ze(yr.value.component), Fe({ key: 8 }, yr.value.props, {
				ref_key: "tableUnit",
				ref: Zt,
				onClose: br
			}), Me({
				content: O(() => [(S(), p(D(Dt), {
					key: `table_${qt.value}`,
					colNames: ur.value.colNames,
					head: ur.value.head,
					body: ur.value.body,
					config: ur.value.config,
					title: F.value.table.useDialog ? "" : yr.value.title,
					withCloseButton: !F.value.table.useDialog,
					isCursorPointer: an.value,
					onClose: br
				}, {
					th: O(({ th: e }) => [Ne(E(e), 1)]),
					td: O(({ td: e }) => [g("div", { innerHTML: e }, null, 8, Ct)]),
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
			}, [F.value.table.useDialog ? {
				name: "title",
				fn: O(() => [Ne(E(yr.value.title), 1)]),
				key: "0"
			} : void 0, F.value.table.useDialog ? {
				name: "actions",
				fn: O(() => [g("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[2] ||= (e) => lr(F.value.userOptions.callbacks.csv),
					style: x({ cursor: an.value ? "pointer" : "default" })
				}, [Pe(D(wt), {
					name: "fileCsv",
					stroke: yr.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : m("", !0),
			T(e.$slots, "skeleton", {}, () => [D(I) ? (S(), p(he, { key: 0 })) : m("", !0)], !0)
		], 46, Ge));
	}
}, [["__scopeId", "data-v-3777144d"]]);
//#endregion
export { We as n, wt as t };
