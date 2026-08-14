import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Et as i, Jt as a, Kt as o, Ot as s, Pt as c, Rt as l, S as u, X as d, _ as ee, ct as te, dt as ne, i as re, jt as ie, o as ae, q as oe, tt as se, vt as ce, w as le, xt as ue } from "./lib-Bttd6u5E.js";
import { n as de, t as fe } from "./useHints-Dq_w2E8B.js";
import { n as pe, r as me, t as he } from "./useTimeLabels-d2f-W1L4.js";
import { t as ge } from "./useConfig-DlNpz6P8.js";
import { t as _e } from "./usePrinter-DN5bYhTG.js";
import { n as ve, t as ye } from "./BaseScanner-DZvpgOjM.js";
import { t as be } from "./useNestedProp-vPNvh7rV.js";
import { t as xe } from "./useThemeCheck-C43Tcqmk.js";
import { t as Se } from "./img-Bnokohej.js";
import { n as Ce } from "./Title-BE3qg9xl.js";
import { t as we } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { a as f, c as p, i as Te, l as Ee, n as De, o as Oe, r as ke, s as Ae, t as je } from "./useResponsive-ZtArZtUf.js";
import { t as Me } from "./BaseIcon-BfndwIWE.js";
import { t as Ne } from "./SlicerPreview-wUw1hFwe.js";
import { t as Pe } from "./vue-ui-accordion-DegI2lzR.js";
import { t as Fe } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Ie } from "./useUserOptionState-DK-_1ddE.js";
import { t as Le } from "./useChartAccessibility-DYqac8yF.js";
import { t as Re } from "./Legend-CQxUgOd-.js";
import { t as ze } from "./vue_ui_xy_canvas-Cb6dg3eK.js";
import { Teleport as Be, computed as m, createBlock as h, createCommentVNode as g, createElementBlock as Ve, createElementVNode as _, createSlots as He, createTextVNode as Ue, createVNode as We, defineAsyncComponent as Ge, guardReactiveProps as v, mergeProps as Ke, nextTick as qe, normalizeClass as Je, normalizeProps as y, normalizeStyle as Ye, onBeforeUnmount as Xe, onMounted as Ze, openBlock as b, ref as x, renderSlot as S, resolveDynamicComponent as Qe, shallowRef as $e, toDisplayString as et, toRefs as tt, unref as C, useSlots as nt, watch as w, watchEffect as rt, withCtx as T } from "vue";
//#region src/components/vue-ui-xy-canvas.vue
var it = /* @__PURE__ */ e({ default: () => ht }), at = ["id"], ot = ["id"], st = {
	class: "sr-only",
	"aria-live": "polite",
	"aria-atomic": "true"
}, ct = ["id"], lt = ["aria-label", "aria-describedby"], ut = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, dt = ["id"], ft = ["onClick"], pt = {
	key: 4,
	class: "vue-data-ui-watermark"
}, mt = ["innerHTML"], ht = /*#__PURE__*/ we({
	__name: "vue-ui-xy-canvas",
	props: {
		dataset: {
			type: Array,
			default() {
				return [];
			}
		},
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		selectedXIndex: {
			type: Number,
			default: void 0
		}
	},
	emits: [
		"selectLegend",
		"selectX",
		"copyAlt"
	],
	setup(e, { expose: we, emit: it }) {
		let ht = Ge(() => import("./Tooltip-DhjyfHwz.js")), gt = Ge(() => import("./DataTable-BbKgJ5UI.js")), _t = Ge(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), vt = Ge(() => import("./NonSvgPenAndPaper-4ypecDg1.js")), yt = Ge(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_xy_canvas: bt } = ge(), { isThemeValid: xt, warnInvalidTheme: St } = xe(), E = e, D = x(oe()), O = x(null), Ct = x(null), k = x(null), A = x(1), j = x(1), wt = x(!1), M = x(null), Tt = x(""), Et = x(null), N = x([]), Dt = x(1), P = x(!0), F = x(!0), Ot = x(null), kt = x(0), At = x(!1), jt = x(null), Mt = x(null), I = x(null), Nt = x(null), Pt = x(0), Ft = x(0), It = x(0), L = x(null), Lt = x(!1), Rt = x(null), zt = x(null), Bt = x(!1), Vt = x(!1), Ht = x(!1), R = x(null), Ut = x(!1), Wt = x(!1), Gt = x(!1), z = x(null), Kt = x({
			x: 0,
			y: 0
		}), B = m(() => Array.isArray(tn.value) && tn.value.length > 0), qt = it, Jt = nt(), V = x(cn());
		de({
			config: () => V.value,
			dataset: () => E.dataset,
			component: "VueUiXyCanvas",
			rules: [
				fe.emptyArray,
				{
					test: (e) => e.every((e) => e.series.length < 300),
					message: [
						"👀 Series have < 300 datapoints. Consider:",
						"",
						"▶️ Using VueUiXy instead, if you need more options, or more customization possibilities."
					]
				},
				{
					test: (e) => e.some((e) => e.series.length > 1e4),
					message: [
						"👀 The dataset has > 10_000 datapoints. Above this threshold, the dataset is computed through an LTTB algorithm, to preserve the shape of the data without increasing the number of datapoints.",
						"",
						"▶️ If you need this level of detail, you can change config.downsample.threshold and set a higher value. Note that performance can be impacted. JS runtime might crash with > 100_000 datapoints."
					]
				}
			]
		});
		let Yt = m(() => {
			let e = V.value.style.chart.title.text || "XY chart", t = Y.value.end - Y.value.start;
			return `${e}. ${J.value.filter((e) => !N.value.includes(e.absoluteIndex)).length} series. ${t} visible data points.`;
		}), Xt = m(() => {
			if (z.value === null) return "";
			let e = z.value + Y.value.start;
			return `${V.value.style.chart.grid.x.timeLabels.values.slice(Y.value.start, Y.value.end)[z.value] ? V.value.style.chart.tooltip.useDefaultTimeFormat ? Q.value.slice(Y.value.start, Y.value.end)[z.value]?.text : Zn.value[z.value]?.text : Q.value[e]?.text ?? `Point ${e + 1}`}. ${J.value.filter((e) => !N.value.includes(e.absoluteIndex)).map((e) => {
				let t = e.series[z.value], n = t?.value ?? t ?? null;
				return `${e.name}: ${n}`;
			}).join(". ")}.`;
		}), Zt = m(() => V.value.userOptions.useCursorPointer), Qt = m(() => V.value.debug);
		Ze(() => {
			Jt["chart-background"] && Qt.value && console.warn("VueUiXyCanvas does not support the #chart-background slot.");
		});
		let $t = m(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					grid: {
						x: {
							axisColor: "#6A6A6A",
							timeLabels: { show: !1 },
							axisName: "",
							horizontalLines: { color: "#6A6A6A" }
						},
						y: {
							axisColor: "#6A6A6A",
							axisLabels: { show: !1 },
							axisName: "",
							verticalLines: { color: "#6A6A6A" }
						},
						zeroLine: { color: "#6A6A6A" }
					},
					legend: { backgroundColor: "#99999930" },
					paddingProportions: { left: .05 },
					scale: {
						max: null,
						min: null
					},
					zoom: {
						endIndex: null,
						startIndex: null
					}
				} }
			},
			userConfig: V.value.skeletonConfig ?? {}
		})), { loading: en, FINAL_DATASET: tn } = ve({
			...tt(E),
			FINAL_CONFIG: V,
			prepareConfig: cn,
			skeletonDataset: E.config?.skeletonDataset ?? [{
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
				color: "#AAAAAA"
			}],
			skeletonConfig: a({
				defaultConfig: V.value,
				userConfig: $t.value
			})
		}), { userOptionsVisible: nn, setUserOptionsVisibility: rn, keepUserOptionState: an } = Ie({ config: V.value }), { svgRef: H } = Le({ config: V.value.style.chart.title });
		function on() {
			rn(!0);
		}
		function sn() {
			rn(!1), qt("selectX", {
				dataset: null,
				index: null,
				indexLabel: null
			}), M.value = null;
		}
		function cn() {
			let e = be({
				userConfig: E.config,
				defaultConfig: bt
			}), t = {}, n = e.theme;
			if (n) if (!xt.value(e)) St(e), t = e;
			else {
				let r = be({
					userConfig: ze[n] || E.config,
					defaultConfig: e
				});
				t = {
					...be({
						userConfig: E.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : o[n] || c
				};
			}
			else t = e;
			return E.config && ce(E.config, "style.chart.grid.y.timeLabels") && (console.warn("VueUiXyCanvas: you are using the deprecated config.style.chart.grid.y.timeLabels. It is recommended to move this configuration to config.style.chart.grid.x.timeLabels."), t.style.chart.grid.x.timeLabels = be({
				defaultConfig: t.style.chart.grid.x.timeLabels,
				userConfig: E.config.style.chart.grid.y.timeLabels
			})), t;
		}
		w(() => E.config, (e) => {
			en.value || (V.value = cn()), nn.value = !V.value.userOptions.showOnChartHover, wr(), Pt.value += 1, Ft.value += 1, It.value += 1, U.value.showTable = V.value.table.show, U.value.showDataLabels = V.value.style.chart.dataLabels.show, U.value.stacked = V.value.style.chart.stacked, U.value.showTooltip = V.value.style.chart.tooltip.show;
		}, { deep: !0 }), w(() => E.dataset, async (e) => {
			!Array.isArray(e) || e.length === 0 || (await qe(), H.value && !k.value && (k.value = H.value.getContext("2d", { willReadFrequently: !0 })), P.value = !0, F.value = !0, await jn(), Bn(), Pt.value += 1, Ft.value += 1, It.value += 1);
		}, { deep: !0 });
		let ln = x(V.value.style.chart.aspectRatio), { isPrinting: un, isImaging: dn, generatePdf: fn, generateImage: pn } = _e({
			elementId: `xy_canvas_${D.value}`,
			fileName: V.value.style.chart.title.text || "vue-ui-xy-canvas",
			options: V.value.userOptions.print
		}), U = x({
			showTable: V.value.table.show,
			showDataLabels: V.value.style.chart.dataLabels.show,
			stacked: V.value.style.chart.stacked,
			showTooltip: V.value.style.chart.tooltip.show
		});
		function mn(e) {
			At.value = e, kt.value += 1;
		}
		let hn = m(() => le(V.value.customPalette)), gn = m(() => N.value.length === q.value.length), W = m(() => q.value ? Math.max(...q.value.filter((e, t) => gn.value ? !0 : !N.value.includes(e.absoluteIndex)).map((e) => e.series.length)) : 0);
		function _n(e) {
			R.value = e, $();
		}
		let vn = x(0), yn = x(0);
		function bn() {
			if (!k.value || !V.value.style.chart.grid.x.timeLabels.show) return 0;
			let e = Q.value || [], t = Y.value.start ?? 0, n = Y.value.end ?? 0;
			if (!Math.max(0, n - t)) return 0;
			let r = Math.round(A.value / 40 * V.value.style.chart.grid.x.timeLabels.fontSizeRatio), i = `${V.value.style.chart.grid.x.timeLabels.bold ? "bold " : ""}${r}px ${V.value.style.fontFamily}`;
			k.value.save(), k.value.font = i;
			let a = 0;
			for (let r = t; r < n; r += 1) {
				let t = e[r]?.text ?? `${r + 1}`, n = k.value.measureText(String(t));
				n.width > a && (a = n.width);
			}
			k.value.restore();
			let o = (V.value.style.chart.grid.x.timeLabels.rotation || 0) * Math.PI / 180, s = r, c = Math.abs(Math.sin(o)) * a + Math.abs(Math.cos(o)) * s, l = V.value.style.chart.grid.x.timeLabels.offsetY || 1, u = A.value / l;
			return Math.max(0, u + c + 4);
		}
		function xn() {
			vn.value = bn();
		}
		function Sn() {
			yn.value && cancelAnimationFrame(yn.value), yn.value = requestAnimationFrame(() => {
				requestAnimationFrame(() => {
					xn();
				});
			});
		}
		Xe(() => {
			yn.value && cancelAnimationFrame(yn.value);
		});
		let G = m(() => {
			let e = A.value - A.value * (V.value.style.chart.paddingProportions.left + V.value.style.chart.paddingProportions.right), t = j.value * V.value.style.chart.paddingProportions.top, n = j.value * V.value.style.chart.paddingProportions.bottom, r = vn.value, i = j.value - n - r, a = j.value - (t + n) - r;
			return {
				canvasWidth: A.value,
				canvasHeight: j.value,
				left: A.value * V.value.style.chart.paddingProportions.left,
				top: t,
				right: A.value - A.value * V.value.style.chart.paddingProportions.right,
				bottom: i,
				width: e,
				height: a,
				slot: e / (Y.value.end - Y.value.start)
			};
		});
		function Cn(e, t) {
			return e / t;
		}
		function wn({ hasAutoScale: e, series: t, min: n, max: r, scale: i, yOffset: a, individualHeight: o, stackIndex: s = null }) {
			return t.map((t, n) => {
				let r = i.min < 0 ? Math.abs(i.min) : 0, c = Cn(t + r, r + i.max), l, u;
				e && (l = i.min, u = Cn(t - l, i.max - l));
				let d = 0;
				return d = s === null ? G.value.bottom - G.value.height * (e ? u : c) : G.value.bottom - a - o * (e ? u : c), {
					x: G.value.left + G.value.slot * n + G.value.slot / 2,
					y: d,
					value: t
				};
			});
		}
		let K = m(() => {
			let e = V.value.style.chart.scale.min === null ? Math.min(...q.value.filter((e, t) => !N.value.includes(e.absoluteIndex)).flatMap((e) => e.series.slice(Y.value.start, Y.value.end))) : V.value.style.chart.scale.min, t = V.value.style.chart.scale.max === null ? Math.max(...q.value.filter((e, t) => !N.value.includes(e.absoluteIndex)).flatMap((e) => e.series.slice(Y.value.start, Y.value.end))) : V.value.style.chart.scale.max, n = ee(e < 0 ? e : 0, t === e ? e + 1 < 0 ? 0 : e + 1 : t < 0 ? 0 : t, V.value.style.chart.scale.ticks), r = n.min < 0 ? Math.abs(n.min) : 0, i = G.value.bottom - G.value.height * (r / (n.max + r));
			return {
				absoluteMin: r,
				max: t,
				min: e,
				scale: n,
				yLabels: n.ticks.map((e) => ({
					y: G.value.bottom - G.value.height * ((e + r) / (n.max + r)),
					x: G.value.left - 8,
					value: e
				})),
				zero: i
			};
		}), Tn = m(() => J.value.map((e) => `
            <div style="display:flex;flex-direction:row;gap:6px;align-items:center;">
                <svg viewBox="0 0 10 10" height="12" width="12">
                    <circle cx="5" cy="5" r="5" fill="${e.color}"/>
                </svg>
                <span>${e.name ? e.name + ": " : ""}</span>
                <span>${re(V.value.style.chart.dataLabels.formatter, e.series[M.value] ?? "-", d({
			p: e.prefix || "",
			v: e.series[M.value] ?? "-",
			s: e.suffix || "",
			r: e.rounding || 0
		}), {
			datapoint: e,
			seriesIndex: M.value
		})}</span>
            </div>
        `)), En = m(() => V.value.style.chart.line.cutNullValues), q = m(() => tn.value.map((e, t) => ({
			...e,
			series: i({
				data: l(e.series, [], En.value),
				threshold: V.value.downsample.threshold
			}),
			absoluteIndex: t,
			color: u(e.color || hn.value[t] || c[t] || c[t % c.length])
		}))), Dn = m(() => {
			if (!V.value.style.chart.zoom.minimap.show) return [];
			let e = q.value.filter((e) => !N.value.includes(e.absoluteIndex)), t = Math.max(...e.map((e) => e.series.length)), n = [];
			for (let r = 0; r < t; r += 1) n.push(e.map((e) => e.series[r] || 0).reduce((e, t) => (e || 0) + (t || 0), 0));
			let r = Math.min(...n);
			return n.map((e) => e + (r < 0 ? Math.abs(r) : 0));
		}), On = m(() => V.value.style.chart.zoom.minimap.show ? q.value.map((e) => ({
			...e,
			isVisible: !N.value.includes(e.absoluteIndex)
		})) : []);
		w(W, (e) => {
			e && jn();
		});
		let J = m(() => ae(q.value.filter((e, t) => !N.value.includes(e.absoluteIndex))).map((e, t) => ({
			...e,
			series: e.series.slice(Y.value.start, Y.value.end)
		})).map((e, t) => {
			let n = [null, void 0].includes(e.scaleMin) ? Math.min(...e.series) || 0 : e.scaleMin, r = [null, void 0].includes(e.scaleMax) ? Math.max(...e.series) || 1 : e.scaleMax;
			n === r && (n = n >= 0 ? r - 1 : n, r = r >= 0 ? r : n + 1);
			let i = {
				ratios: e.series.filter((e) => ![null, void 0].includes(e)).map((e) => (e - n) / (r - n)),
				valueMin: n,
				valueMax: r
			}, a = e.scaleSteps || V.value.style.chart.scale.ticks, o;
			o = e.autoScaling ? ee(i.valueMin, i.valueMax, a) : ee(i.valueMin < 0 ? i.valueMin : 0, i.valueMax <= 0 ? 0 : i.valueMax, a);
			let s = U.value.stacked ? G.value.height * (1 - e.cumulatedStackRatio) : 0, c = U.value.stacked ? G.value.height / V.value.style.chart.stackGap : 0, l = U.value.stacked ? G.value.height * e.stackRatio - c : G.value.height, u = o.min < 0 ? Math.abs(o.min) : 0, d;
			d = e.autoScaling && U.value.stacked && r <= 0 ? G.value.bottom - s - l : G.value.bottom - s - l * (u / (o.max + u));
			let te = o.ticks.map((e, t) => ({
				y: G.value.bottom - s - l * (t / (o.ticks.length - 1)),
				x: G.value.left - 8,
				value: e
			})), ne = wn({
				hasAutoScale: U.value.stacked && e.autoScaling,
				series: e.series,
				min: U.value.stacked ? n : K.value.min,
				max: U.value.stacked ? r : K.value.max,
				scale: U.value.stacked ? o : K.value.scale,
				yOffset: s,
				individualHeight: l,
				stackIndex: U.value.stacked ? t : null
			});
			return {
				...e,
				coordinatesLine: ne,
				min: n,
				max: r,
				localScale: o,
				localZero: d,
				localMin: u,
				localYLabels: te,
				yOffset: s,
				individualHeight: l
			};
		})), Y = x({
			start: 0,
			end: W.value
		}), X = x({
			start: 0,
			end: W.value
		}), kn = x(null);
		function An() {
			return new Promise((e) => requestAnimationFrame(() => requestAnimationFrame(() => e())));
		}
		Xe(() => {
			kn.value && cancelAnimationFrame(kn.value);
		});
		async function jn() {
			Pn(), await qe(), kn.value && cancelAnimationFrame(kn.value), kn.value = requestAnimationFrame(async () => {
				await An(), Pn();
			});
		}
		let Mn = m(() => V.value.style.chart.zoom.preview.enable && (X.value.start !== Y.value.start || X.value.end !== Y.value.end));
		function Nn(e, t) {
			X.value[e] = t;
		}
		async function Pn() {
			if (!Bt.value) {
				Bt.value = !0;
				try {
					let { startIndex: e, endIndex: t } = V.value.style.chart.zoom, n = Math.max(...q.value.map((e) => e.series.length)), r = e ?? 0, i = t == null ? n : Math.min(Ln(t + 1), n);
					Ht.value = !0, Y.value.start = r, Y.value.end = i, X.value.start = r, X.value.end = i, Rn(), Vt.value = !0, await qe(), I.value && (I.value.setStartValue(Y.value.start), I.value.setEndValue(Y.value.end));
				} finally {
					queueMicrotask(() => {
						Ht.value = !1;
					}), Bt.value = !1;
				}
			}
		}
		function Fn(e) {
			Bt.value || Ht.value || e !== Y.value.start && (Y.value.start = e, X.value.start = e, Rn());
		}
		function In(e) {
			if (Bt.value || Ht.value) return;
			let t = Ln(e);
			t !== Y.value.end && (Y.value.end = t, X.value.end = t, Rn());
		}
		function Ln(e) {
			let t = W.value;
			return e > t ? t : e < 0 || e < Y.value.start ? V.value.style.chart.zoom.startIndex === null ? 1 : V.value.style.chart.zoom.startIndex + 1 : e;
		}
		function Rn() {
			let e = Math.max(1, Math.max(...q.value.map((e) => e.series.length))), t = Math.max(0, Math.min(Y.value.start ?? 0, e - 1)), n = Math.max(t + 1, Math.min(Y.value.end ?? e, e));
			(!Number.isFinite(t) || !Number.isFinite(n) || n <= t) && (t = 0, n = e), Y.value = {
				start: t,
				end: n
			}, X.value.start = t, X.value.end = n, I.value && (I.value.setStartValue(t), I.value.setEndValue(n));
		}
		let zn = m(() => J.value.filter((e) => [
			"line",
			"plot",
			void 0
		].includes(e.type))), Z = m(() => J.value.filter((e) => e.type === "bar"));
		function Bn() {
			if (!H.value || !Ct.value) return;
			let e = Ct.value.offsetWidth, t = Ct.value.offsetHeight;
			H.value.width = e * Dt.value * 2, H.value.height = t * Dt.value * 2, A.value = e * Dt.value * 2, j.value = t * Dt.value * 2, k.value?.scale(Dt.value, Dt.value), $();
		}
		w(B, async (e) => {
			if (!e) {
				Ot.value = null, wt.value = !1, M.value = null, L.value = null;
				return;
			}
			await qe(), H.value && !k.value && (k.value = H.value.getContext("2d", { willReadFrequently: !0 })), P.value = !0, F.value = !0, await jn(), Bn(), $();
		});
		function Vn() {
			if (k.value && (k.value.clearRect(0, 0, 1e4, 1e4), k.value.fillStyle = V.value.style.chart.backgroundColor, k.value.fillRect(0, 0, G.value.canvasWidth, G.value.canvasHeight), B.value)) {
				if (U.value.stacked) V.value.style.chart.grid.y.verticalLines.show && Y.value.end - Y.value.start < V.value.style.chart.grid.y.verticalLines.hideUnderXLength ? J.value.forEach((e) => {
					for (let t = 0; t < Y.value.end - Y.value.start + 1; t += 1) f(k.value, [{
						x: G.value.left + G.value.slot * t,
						y: G.value.bottom - e.yOffset - e.individualHeight
					}, {
						x: G.value.left + G.value.slot * t,
						y: G.value.bottom - e.yOffset
					}], { color: V.value.style.chart.grid.y.verticalLines.color });
				}) : V.value.style.chart.grid.y.verticalLines.show && Y.value.end - Y.value.start >= V.value.style.chart.grid.y.verticalLines.hideUnderXLength && J.value.forEach((e) => {
					for (let t = Y.value.start; t < Y.value.end; t += 1) t % Math.floor((Y.value.end - Y.value.start) / V.value.style.chart.grid.x.timeLabels.modulo) === 0 && f(k.value, [{
						x: G.value.left + G.value.slot * (t - Y.value.start) + G.value.slot / 2,
						y: G.value.bottom - e.yOffset - e.individualHeight
					}, {
						x: G.value.left + G.value.slot * (t - Y.value.start) + G.value.slot / 2,
						y: G.value.bottom - e.yOffset
					}], { color: V.value.style.chart.grid.y.verticalLines.color });
				}), V.value.style.chart.grid.x.horizontalLines.show && (V.value.style.chart.grid.x.horizontalLines.alternate ? J.value.forEach((e) => {
					e.localYLabels.forEach((t, r) => {
						r < e.localYLabels.length - 1 && Ae(k.value, [
							{
								x: G.value.left,
								y: t.y
							},
							{
								x: G.value.right,
								y: t.y
							},
							{
								x: G.value.right,
								y: e.localYLabels[r + 1].y
							},
							{
								x: G.value.left,
								y: e.localYLabels[r + 1].y
							}
						], {
							fillColor: r % 2 == 0 ? "transparent" : n(V.value.style.chart.grid.x.horizontalLines.color, V.value.style.chart.grid.x.horizontalLines.opacity),
							strokeColor: "transparent"
						});
					});
				}) : J.value.forEach((e) => {
					e.localYLabels.slice(Y.value.start, Y.value.end).forEach((e) => {
						f(k.value, [{
							x: G.value.left,
							y: e.y
						}, {
							x: G.value.right,
							y: e.y
						}], { color: V.value.style.chart.grid.x.horizontalLines.color });
					});
				})), V.value.style.chart.grid.zeroLine.show && J.value.forEach((e) => {
					f(k.value, [{
						x: G.value.left,
						y: e.localZero
					}, {
						x: G.value.right,
						y: e.localZero
					}], {
						color: V.value.style.chart.grid.zeroLine.color,
						lineDash: V.value.style.chart.grid.zeroLine.dashed ? [10, 10] : [0, 0]
					});
				}), V.value.style.chart.grid.y.axisLabels.show && J.value.forEach((e) => {
					f(k.value, [{
						x: G.value.left,
						y: G.value.bottom - e.yOffset
					}, {
						x: G.value.left,
						y: G.value.bottom - e.yOffset - e.individualHeight
					}], { color: e.color }), f(k.value, [{
						x: G.value.right,
						y: G.value.bottom - e.yOffset
					}, {
						x: G.value.right,
						y: G.value.bottom - e.yOffset - e.individualHeight
					}], { color: e.color });
				}), J.value.forEach((e) => {
					p(k.value, e.name, A.value / 35, G.value.bottom - e.yOffset - e.individualHeight / 2, {
						align: "center",
						rotation: -90,
						color: e.color,
						font: `${Math.round(A.value / 40 * V.value.style.chart.grid.y.axisLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`
					});
				});
				else {
					if (V.value.style.chart.grid.y.verticalLines.show && Y.value.end - Y.value.start < V.value.style.chart.grid.y.verticalLines.hideUnderXLength) for (let e = 0; e < Y.value.end - Y.value.start + 1; e += 1) f(k.value, [{
						x: G.value.left + G.value.slot * e,
						y: G.value.top
					}, {
						x: G.value.left + G.value.slot * e,
						y: G.value.bottom
					}], { color: V.value.style.chart.grid.y.verticalLines.color });
					else if (V.value.style.chart.grid.y.verticalLines.show && Y.value.end - Y.value.start >= V.value.style.chart.grid.y.verticalLines.hideUnderXLength) for (let e = Y.value.start; e < Y.value.end; e += 1) e % Math.floor((Y.value.end - Y.value.start) / V.value.style.chart.grid.x.timeLabels.modulo) === 0 && f(k.value, [{
						x: G.value.left + G.value.slot * (e - Y.value.start) + G.value.slot / 2,
						y: G.value.top
					}, {
						x: G.value.left + G.value.slot * (e - Y.value.start) + G.value.slot / 2,
						y: G.value.bottom
					}], { color: V.value.style.chart.grid.y.verticalLines.color });
					V.value.style.chart.grid.x.horizontalLines.show && (V.value.style.chart.grid.x.horizontalLines.alternate ? K.value.yLabels.forEach((e, t) => {
						t < K.value.yLabels.length - 1 && Ae(k.value, [
							{
								x: G.value.left,
								y: e.y
							},
							{
								x: G.value.right,
								y: e.y
							},
							{
								x: G.value.right,
								y: K.value.yLabels[t + 1].y
							},
							{
								x: G.value.left,
								y: K.value.yLabels[t + 1].y
							}
						], {
							fillColor: t % 2 == 0 ? "transparent" : n(V.value.style.chart.grid.x.horizontalLines.color, V.value.style.chart.grid.x.horizontalLines.opacity),
							strokeColor: "transparent"
						});
					}) : K.value.yLabels.forEach((e) => {
						f(k.value, [{
							x: G.value.left,
							y: e.y
						}, {
							x: G.value.right,
							y: e.y
						}], { color: V.value.style.chart.grid.x.horizontalLines.color });
					})), V.value.style.chart.grid.y.showAxis && f(k.value, [{
						x: G.value.left,
						y: G.value.top
					}, {
						x: G.value.left,
						y: G.value.bottom
					}], {
						color: V.value.style.chart.grid.y.axisColor,
						lineWidth: V.value.style.chart.grid.y.axisThickness
					}), V.value.style.chart.grid.x.showAxis && f(k.value, [{
						x: G.value.left,
						y: G.value.bottom
					}, {
						x: G.value.right,
						y: G.value.bottom
					}], {
						color: V.value.style.chart.grid.x.axisColor,
						lineWidth: V.value.style.chart.grid.x.axisThickness
					}), V.value.style.chart.grid.zeroLine.show && f(k.value, [{
						x: G.value.left,
						y: K.value.zero
					}, {
						x: G.value.right,
						y: K.value.zero
					}], {
						color: V.value.style.chart.grid.zeroLine.color,
						lineDash: V.value.style.chart.grid.zeroLine.dashed ? [10, 10] : [0, 0]
					});
				}
				V.value.style.chart.grid.y.axisName && p(k.value, V.value.style.chart.grid.y.axisName, A.value - A.value / 40 * V.value.style.chart.grid.y.axisLabels.fontSizeRatio * 1.2, G.value.bottom - G.value.height / 2, {
					font: `${V.value.style.chart.grid.y.axisLabels.bold ? "bold " : ""}${Math.round(A.value / 40 * V.value.style.chart.grid.y.axisLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
					color: V.value.style.chart.color,
					align: "center",
					rotation: 90
				}), V.value.style.chart.grid.x.axisName && p(k.value, V.value.style.chart.grid.x.axisName, A.value / 2, j.value, {
					font: `${V.value.style.chart.grid.y.axisLabels.bold ? "bold " : ""}${Math.round(A.value / 40 * V.value.style.chart.grid.y.axisLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
					color: V.value.style.chart.color,
					align: "center"
				});
			}
		}
		function Hn(e) {
			for (let t = 0; t < e.coordinatesLine.length; t += 1) {
				let n = (M.value === t || R.value === t ? A.value / 150 : V.value.style.chart.line.plots.show || e.type === "plot" ? A.value / 200 : 0) * V.value.style.chart.line.plots.radiusRatio;
				De(k.value, {
					x: e.coordinatesLine[t].x,
					y: e.coordinatesLine[t].y
				}, n, {
					color: V.value.style.chart.backgroundColor,
					fillStyle: e.color,
					strokeColor: "transparent"
				});
			}
		}
		function Un() {
			J.value.forEach((e) => {
				e.showYMarker && lr(e) && p(k.value, re(V.value.style.chart.dataLabels.formatter, lr(e).value, d({
					p: e.prefix || V.value.style.chart.grid.y.axisLabels.prefix || "",
					v: lr(e).value,
					s: e.suffix || V.value.style.chart.grid.y.axisLabels.suffix || "",
					r: e.rounding || V.value.style.chart.grid.y.axisLabels.rounding || 0
				}), {
					datapoint: lr(e),
					seriesIndex: null
				}), G.value.left - 8 + V.value.style.chart.grid.y.axisLabels.offsetX, lr(e).y, {
					align: "right",
					font: `${V.value.style.chart.grid.y.axisLabels.bold ? "bold " : ""}${Math.round(A.value / 40 * V.value.style.chart.grid.y.axisLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
					color: e.color
				});
			});
		}
		function Wn() {
			V.value.style.chart.grid.y.axisLabels.show && (U.value.stacked ? J.value.forEach((e) => {
				e.localYLabels.forEach((t, n) => {
					p(k.value, re(V.value.style.chart.dataLabels.formatter, t.value, d({
						p: e.prefix || V.value.style.chart.grid.y.axisLabels.prefix || "",
						v: t.value,
						s: e.suffix || V.value.style.chart.grid.y.axisLabels.suffix || "",
						r: e.rounding || V.value.style.chart.grid.y.axisLabels.rounding || 0
					}), {
						datapoint: t,
						seriesIndex: n
					}), t.x + V.value.style.chart.grid.y.axisLabels.offsetX, t.y, {
						align: "right",
						font: `${V.value.style.chart.grid.y.axisLabels.bold ? "bold " : ""}${Math.round(A.value / 40 * V.value.style.chart.grid.y.axisLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
						color: e.color,
						globalAlpha: e.showYMarker && ![null, void 0].includes(M.value ?? R.value) ? .2 : 1
					});
				});
			}) : K.value.yLabels.forEach((e, t) => {
				p(k.value, re(V.value.style.chart.dataLabels.formatter, e.value, d({
					p: V.value.style.chart.grid.y.axisLabels.prefix || "",
					v: e.value,
					s: V.value.style.chart.grid.y.axisLabels.suffix || "",
					r: V.value.style.chart.grid.y.axisLabels.rounding || 0
				}), {
					datapoint: e,
					seriesIndex: t
				}), e.x + V.value.style.chart.grid.y.axisLabels.offsetX, e.y, {
					align: "right",
					font: `${V.value.style.chart.grid.y.axisLabels.bold ? "bold " : ""}${Math.round(A.value / 40 * V.value.style.chart.grid.y.axisLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
					color: V.value.style.chart.grid.y.axisLabels.color,
					globalAlpha: J.value.some((e) => e.showYMarker) && ![null, void 0].includes(M.value ?? R.value) ? .2 : 1
				});
			}));
		}
		function Gn(e) {
			for (let t = 0; t < e.coordinatesLine.length; t += 1) p(k.value, re(V.value.style.chart.dataLabels.formatter, e.coordinatesLine[t].value, d({
				p: e.prefix || "",
				v: e.coordinatesLine[t].value,
				s: e.suffix || "",
				r: e.rounding || 0
			}), {
				datapoint: e.coordinatesLine[t],
				seriesIndex: t
			}), e.coordinatesLine[t].x, e.coordinatesLine[t].y + V.value.style.chart.dataLabels.offsetY, {
				align: "center",
				font: `${V.value.style.chart.dataLabels.bold ? "bold " : ""}${Math.round(A.value / 40 * V.value.style.chart.dataLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
				color: V.value.style.chart.dataLabels.useSerieColor ? e.color : V.value.style.chart.dataLabels.color,
				strokeColor: V.value.style.chart.backgroundColor,
				lineWidth: .5
			});
		}
		let Q = x([]), Kn = 0;
		rt(() => {
			let e = ++Kn;
			(async () => {
				let t = await he({
					values: V.value.style.chart.grid.x.timeLabels.values,
					maxDatapoints: W.value,
					formatter: V.value.style.chart.grid.x.timeLabels.datetimeFormatter,
					start: 0,
					end: W.value
				});
				e === Kn && (Q.value = t);
			})();
		}), rt(() => {
			V.value.style.chart.grid.x.timeLabels.show, V.value.style.chart.grid.x.timeLabels.rotation, V.value.style.chart.grid.x.timeLabels.offsetY, V.value.style.chart.grid.x.timeLabels.fontSizeRatio, V.value.style.chart.grid.x.timeLabels.bold, Y.value.start, Y.value.end, A.value, j.value, (Q.value || []).map((e) => e?.text ?? "").join("|"), Sn();
		}, { flush: "post" });
		let qn = x({
			months: [],
			shortMonths: [],
			days: [],
			shortDays: []
		}), Jn = 0;
		rt(() => {
			let e = ++Jn, t = V.value.style.chart.grid.x.timeLabels.datetimeFormatter;
			(async () => {
				let n = await me(t.locale).catch(() => me("en"));
				e === Jn && (qn.value = n.data);
			})();
		});
		let Yn = m(() => {
			let e = V.value.style.chart.grid.x.timeLabels.datetimeFormatter, t = pe({
				useUTC: e.useUTC,
				locale: qn.value,
				januaryAsYear: e.januaryAsYear
			});
			return (e, n) => {
				let r = V.value.style.chart.grid.x.timeLabels.values?.[e];
				return r == null ? "" : t.formatDate(new Date(r), n);
			};
		}), Xn = m(() => (V.value.style.chart.grid.x.timeLabels.values || []).map((e, t) => ({
			text: Yn.value(t, V.value.style.chart.zoom.timeFormat),
			absoluteIndex: t
		}))), Zn = m(() => (V.value.style.chart.grid.x.timeLabels.values || []).map((e, t) => ({
			text: Yn.value(t, V.value.style.chart.tooltip.timeFormat),
			absoluteIndex: t
		})));
		function Qn() {
			for (let e = Y.value.start; e < Y.value.end; e += 1) (Y.value.end - Y.value.start < V.value.style.chart.grid.x.timeLabels.modulo || Y.value.end - Y.value.start >= V.value.style.chart.grid.x.timeLabels.modulo && (e % Math.floor((Y.value.end - Y.value.start) / V.value.style.chart.grid.x.timeLabels.modulo) === 0 || (e === M.value + Y.value.start || e === R.value) && V.value.style.chart.grid.x.timeLabels.showMarker)) && p(k.value, Q.value[e] ? Q.value[e].text : e + 1, G.value.left + G.value.slot * (e - Y.value.start) + G.value.slot / 2, G.value.bottom + A.value / V.value.style.chart.grid.x.timeLabels.offsetY, {
				align: V.value.style.chart.grid.x.timeLabels.rotation === 0 ? "center" : V.value.style.chart.grid.x.timeLabels.rotation > 0 ? "left" : "right",
				font: `${V.value.style.chart.grid.x.timeLabels.bold ? "bold " : ""}${Math.round(A.value / 40 * V.value.style.chart.grid.x.timeLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
				color: V.value.style.chart.grid.x.timeLabels.showMarker ? n(V.value.style.chart.grid.x.timeLabels.color, M.value !== null || R.value !== null ? M.value + Y.value.start === e || R.value === e ? 100 : 20 : 100) : V.value.style.chart.grid.x.timeLabels.color,
				rotation: V.value.style.chart.grid.x.timeLabels.rotation
			});
		}
		function $n() {
			f(k.value, [{
				x: G.value.left + G.value.slot * (M.value ?? R.value) + G.value.slot / 2,
				y: G.value.top
			}, {
				x: G.value.left + G.value.slot * (M.value ?? R.value) + G.value.slot / 2,
				y: G.value.bottom
			}], {
				color: V.value.style.chart.selector.color,
				lineDash: V.value.style.chart.selector.dashed ? [8, 8] : [0, 0],
				lineWidth: 2,
				linceCap: "round"
			});
		}
		function er() {
			L.value && f(k.value, [{
				x: G.value.left,
				y: L.value
			}, {
				x: G.value.right,
				y: L.value
			}], {
				color: V.value.style.chart.selector.color,
				lineDash: V.value.style.chart.selector.dashed ? [8, 8] : [0, 0],
				lineWidth: 2,
				linceCap: "round"
			});
		}
		function tr() {
			Z.value.forEach((e, t) => {
				for (let n = 0; n < e.coordinatesLine.length; n += 1) Ae(k.value, [
					{
						x: G.value.left + G.value.slot * n + G.value.slot / 10 + (U.value.stacked ? 0 : G.value.slot / Z.value.length * t - (t === 0 ? 0 : G.value.slot / (5 * Z.value.length) * t)),
						y: U.value.stacked ? e.localZero : K.value.zero
					},
					{
						x: G.value.left + G.value.slot * n + G.value.slot / 10 + (U.value.stacked ? 0 : G.value.slot / Z.value.length * t - (t === 0 ? 0 : G.value.slot / (5 * Z.value.length) * t)) + G.value.slot * .8 / (U.value.stacked ? 1 : Z.value.length),
						y: U.value.stacked ? e.localZero : K.value.zero
					},
					{
						x: G.value.left + G.value.slot * n + G.value.slot / 10 + (U.value.stacked ? 0 : G.value.slot / Z.value.length * t - (t === 0 ? 0 : G.value.slot / (5 * Z.value.length) * t)) + G.value.slot * .8 / (U.value.stacked ? 1 : Z.value.length),
						y: e.coordinatesLine[n].y
					},
					{
						x: G.value.left + G.value.slot * n + G.value.slot / 10 + (U.value.stacked ? 0 : G.value.slot / Z.value.length * t - (t === 0 ? 0 : G.value.slot / (5 * Z.value.length) * t)),
						y: e.coordinatesLine[n].y
					}
				], {
					strokeColor: V.value.style.chart.backgroundColor,
					gradient: {
						type: "linear",
						start: {
							x: e.coordinatesLine[n].x,
							y: e.coordinatesLine[n].y
						},
						end: {
							x: e.coordinatesLine[n].x,
							y: U.value.stacked ? e.localZero : K.value.zero
						},
						stops: [{
							offset: 0,
							color: e.color
						}, {
							offset: 1,
							color: V.value.style.chart.bar.gradient.show ? s(e.color, .5) : e.color
						}]
					}
				}), U.value.showDataLabels && [!0, void 0].includes(e.dataLabels) && p(k.value, re(V.value.style.chart.dataLabels.formatter, e.coordinatesLine[n].value, d({
					p: e.prefix || "",
					v: e.coordinatesLine[n].value,
					s: e.suffix || "",
					r: e.rounding || 0
				}), {
					datapoint: e.coordinatesLine[n],
					seriesIndex: n
				}), G.value.left + G.value.slot * n + G.value.slot / 10 + (U.value.stacked ? 0 : G.value.slot / Z.value.length * t - (t === 0 ? 0 : G.value.slot / (5 * Z.value.length) * t)) + G.value.slot * .4 / (U.value.stacked ? 1 : Z.value.length), (e.coordinatesLine[n].value < 0 ? U.value.stacked ? e.localZero : K.value.zero : e.coordinatesLine[n].y) + V.value.style.chart.dataLabels.offsetY, {
					align: "center",
					font: `${Math.round(A.value / 40 * V.value.style.chart.dataLabels.fontSizeRatio)}px ${V.value.style.fontFamily}`,
					color: V.value.style.chart.dataLabels.useSerieColor ? e.color : V.value.style.chart.dataLabels.color,
					strokeColor: V.value.style.chart.backgroundColor,
					lineWidth: .8
				});
			});
		}
		function nr(e, t) {
			let n = e.coordinatesLine.map((t, n) => e.series[n] != null && Number.isFinite(t?.y) ? t : null), r = [], i = [], a = [];
			for (let e = 0; e < n.length; e += 1) {
				let o = n[e];
				if (o) a.push(o);
				else {
					if (a.length >= 2) {
						let e = a[0], n = a[a.length - 1];
						r.push([
							{
								x: e.x,
								y: t
							},
							...a,
							{
								x: n.x,
								y: t
							}
						]);
					} else a.length === 1 && i.push(a[0]);
					a = [];
				}
			}
			if (a.length >= 2) {
				let e = a[0], n = a[a.length - 1];
				r.push([
					{
						x: e.x,
						y: t
					},
					...a,
					{
						x: n.x,
						y: t
					}
				]);
			} else a.length === 1 && i.push(a[0]);
			return {
				polygons: r,
				singles: i
			};
		}
		function rr(e) {
			let t = e.coordinatesLine.map((t, n) => e.series[n] != null && Number.isFinite(t?.y) ? t : null), n = [], r = [];
			for (let e = 0; e < t.length; e += 1) {
				let i = t[e];
				i ? r.push(i) : (r.length >= 2 && n.push(r), r = []);
			}
			return r.length >= 2 && n.push(r), n;
		}
		function ir(e) {
			if (En.value) {
				let t = rr(e);
				for (let n of t) f(k.value, n, {
					color: e.color,
					lineWidth: 3
				});
			} else f(k.value, e.coordinatesLine, {
				color: e.color,
				lineWidth: 3
			});
		}
		function ar(e) {
			let t = !!En.value;
			if (e.useArea) {
				let r = U.value.stacked ? e.localZero : K.value.zero;
				if (t) {
					let { polygons: t, singles: i } = nr(e, r);
					for (let r of t) Oe(k.value, r, {
						fillColor: n(e.color, V.value.style.chart.area.opacity),
						strokeColor: "transparent"
					});
					let a = A.value / 200 * V.value.style.chart.line.plots.radiusRatio;
					for (let t of i) De(k.value, {
						x: t.x,
						y: t.y
					}, a, {
						color: V.value.style.chart.backgroundColor,
						fillStyle: e.color,
						strokeColor: "transparent"
					});
				} else {
					let t = {
						x: e.coordinatesLine[0].x,
						y: r
					}, i = {
						x: e.coordinatesLine.at(-1).x,
						y: r
					};
					Oe(k.value, [
						t,
						...e.coordinatesLine,
						i
					], {
						fillColor: n(e.color, V.value.style.chart.area.opacity),
						strokeColor: "transparent"
					});
				}
				ir(e);
				return;
			}
			ir(e);
		}
		function or() {
			J.value.forEach((e, t) => {
				f(k.value, [{
					x: G.value.left,
					y: G.value.bottom - e.yOffset
				}, {
					x: G.value.right,
					y: G.value.bottom - e.yOffset
				}], {
					color: V.value.style.chart.grid.x.horizontalLines.color,
					lineWidth: 1
				});
			});
		}
		function sr() {
			let { left: e, top: t, width: n, height: r } = G.value, i = Y.value.start, a = Y.value.end - i, o = n / a, s = X.value.start - i, c = X.value.end - i, l = Math.max(0, Math.min(a, s)), u = Math.max(0, Math.min(a, c));
			Ae(k.value, [
				{
					x: e + l * o,
					y: t
				},
				{
					x: e + l * o + (u - l) * o,
					y: t
				},
				{
					x: e + l * o + (u - l) * o,
					y: t + r
				},
				{
					x: e + l * o,
					y: t + r
				}
			], {
				fillColor: V.value.style.chart.zoom.preview.fill,
				strokeColor: V.value.style.chart.zoom.preview.stroke,
				lineDash: [
					,
					,
					,
					,
				].fill(V.value.style.chart.zoom.preview.strokeDasharray),
				lineWidth: V.value.style.chart.zoom.preview.strokeWidth
			});
		}
		function $() {
			!B.value || !H.value || !k.value || (Vn(), P.value ? ((M.value !== null || R.value !== null) && V.value.style.chart.selector.show && $n(), tr(), U.value.stacked && V.value.style.chart.grid.x.showAxis && or(), zn.value.forEach((e) => {
				(e.type === "line" || !e.type) && ar(e), F.value && (Hn(e), U.value.showDataLabels && [!0, void 0].includes(e.dataLabels) && Gn(e));
			}), H.value && (Ot.value = ke(H.value))) : (Ot.value && (k.value.clearRect(0, 0, 1e4, 1e4), k.value.drawImage(Ot.value, 0, 0)), (M.value !== null || R.value !== null) && V.value.style.chart.selector.show && $n(), (M.value !== null || R.value !== null) && J.value.forEach((e) => {
				let t = M.value ?? R.value, n = e.coordinatesLine[t];
				(e.type === "line" || !e.type || e.type === "plot") && n && Number.isFinite(n.x) && Number.isFinite(n.y) && De(k.value, {
					x: n.x,
					y: n.y
				}, A.value / 150 * V.value.style.chart.line.plots.radiusRatio, {
					color: V.value.style.chart.backgroundColor,
					fillStyle: e.color,
					strokeColor: "transparent"
				});
			})), V.value.style.chart.grid.x.timeLabels.show && Qn(), V.value.style.chart.selector.show && V.value.style.chart.selector.showHorizontalSelector && er(), Wn(), Un(), V.value.style.chart.zoom.preview.enable && (Y.value.start !== X.value.start || Y.value.end !== X.value.end) && sr(), P.value = !1);
		}
		let cr = Te(() => {
			F.value = !0, Bn();
		}, W.value > 200 ? 10 : 1, !F.value);
		function lr(e) {
			if ([null, void 0].includes(M.value ?? R.value) || !e.coordinatesLine[M.value ?? R.value]) return !1;
			let { y: t, value: n } = e.coordinatesLine[M.value ?? R.value];
			return {
				y: t,
				value: n
			};
		}
		function ur(e) {
			if (!B.value || !H.value || gn.value) return;
			let { left: t, top: n } = H.value.getBoundingClientRect(), r = e.clientX - t;
			if (L.value = (e.clientY - n) * 2, (L.value < G.value.top || L.value > G.value.bottom) && (L.value = null), r * 2 < G.value.left || r * 2 > G.value.right) {
				hr();
				return;
			}
			let i = r * 2 - G.value.left, a = Math.floor(i / G.value.slot);
			if (M.value = a, z.value = a, wt.value = !0, Wt.value = !1, !F.value) return;
			let o = "", s = V.value.style.chart.tooltip.customFormat, c = J.value.map((e) => ({
				shape: e.shape || null,
				name: e.name,
				color: e.color,
				type: e.type || "line",
				value: e.series.find((e, t) => t === M.value)
			}));
			Et.value = {
				timeLabel: V.value.style.chart.grid.x.timeLabels.values.slice(Y.value.start, Y.value.end)[M.value] ? V.value.style.chart.tooltip.useDefaultTimeFormat ? Q.value.slice(Y.value.start, Y.value.end)[M.value]?.text : Zn.value[M.value]?.text : "",
				datapoint: c,
				seriesIndex: M.value,
				series: J.value,
				config: V.value
			}, yr({
				seriesIndex: M.value,
				datapoint: c
			}), ue(s) && te(() => s({
				seriesIndex: M.value,
				datapoint: c,
				series: J.value,
				config: V.value
			})) ? Tt.value = s({
				seriesIndex: M.value,
				datapoint: c,
				series: J.value,
				config: V.value
			}) : (V.value.style.chart.grid.x.timeLabels.values.slice(Y.value.start, Y.value.end)[M.value] ? o += `<div style="padding-bottom: 6px; margin-bottom: 4px; border-bottom: 1px solid ${V.value.style.chart.tooltip.borderColor}; width:100%">${V.value.style.chart.tooltip.useDefaultTimeFormat ? Q.value.slice(Y.value.start, Y.value.end)[M.value]?.text : Zn.value[M.value]?.text}</div>` : o += `<div style="padding-bottom: 6px; margin-bottom: 4px; border-bottom: 1px solid ${V.value.style.chart.tooltip.borderColor}; width:100%">${Q.value[M.value + Y.value.start]?.text ?? ""}</div>`, o += Tn.value.join(""), Tt.value = o), F.value = !1;
		}
		function dr(e) {
			return J.value.map((t) => ({
				shape: t.shape || null,
				name: t.name,
				color: t.color,
				type: t.type || "line",
				value: t.series.find((t, n) => n === e)
			}));
		}
		function fr(e) {
			let t = "", n = V.value.style.chart.tooltip.customFormat, r = dr(e);
			return yr({
				seriesIndex: e,
				datapoint: r
			}), ue(n) && te(() => n({
				seriesIndex: e,
				datapoint: r,
				series: J.value,
				config: V.value
			})) ? n({
				seriesIndex: e,
				datapoint: r,
				series: J.value,
				config: V.value
			}) : (V.value.style.chart.grid.x.timeLabels.values.slice(Y.value.start, Y.value.end)[e] ? t += `<div style="padding-bottom: 6px; margin-bottom: 4px; border-bottom: 1px solid ${V.value.style.chart.tooltip.borderColor}; width:100%">${V.value.style.chart.tooltip.useDefaultTimeFormat ? Q.value.slice(Y.value.start, Y.value.end)[e]?.text : Zn.value[e]?.text}</div>` : t += `<div style="padding-bottom: 6px; margin-bottom: 4px; border-bottom: 1px solid ${V.value.style.chart.tooltip.borderColor}; width:100%">${Q.value[e + Y.value.start]?.text ?? ""}</div>`, t += Tn.value.join(""), t);
		}
		function pr(e) {
			if (!H.value || !G.value?.slot) return;
			let t = H.value.getBoundingClientRect(), n = t.width / G.value.canvasWidth, r = t.height / G.value.canvasHeight, i = G.value.left + G.value.slot * e + G.value.slot / 2, a = G.value.top + G.value.height / 2;
			Kt.value = {
				x: t.left + i * n,
				y: t.top + a * r
			};
		}
		function mr(e, { fromKeyboard: t = !1 } = {}) {
			!B.value || gn.value || e != null && (e < 0 || e >= Y.value.end - Y.value.start || (M.value = e, z.value = e, wt.value = !0, Wt.value = t, Tt.value = fr(e), t && pr(e), F.value = !1, $()));
		}
		function hr() {
			wt.value = !1, M.value = null, z.value = null, Tt.value = "", L.value = null, Wt.value = !1, $();
		}
		function gr() {
			Gt.value = !0;
		}
		function _r() {
			Gt.value = !1, hr();
		}
		function vr(e) {
			if (!H.value || Br.value || document.activeElement !== H.value) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight";
			if (!t && !n) return;
			let r = Y.value.end - Y.value.start;
			if (r <= 0) return;
			e.preventDefault(), e.stopPropagation();
			let i = z.value;
			i !== null && i >= 0 && i < r ? n ? (i += 1, i >= r && (i = 0)) : (--i, i < 0 && (i = r - 1)) : i = n ? 0 : r - 1, mr(i, { fromKeyboard: !0 });
		}
		function yr({ seriesIndex: e, datapoint: t }) {
			let n = Y.value.start + e;
			qt("selectX", {
				dataset: t,
				index: n,
				indexLabel: ""
			});
		}
		w(() => E.selectedXIndex, (e) => {
			if ([null, void 0].includes(E.selectedXIndex)) {
				hr();
				return;
			}
			let t = e - Y.value.start;
			t < 0 || e >= Y.value.end ? hr() : mr(t, { fromKeyboard: !1 });
		}, { immediate: !0 }), w(() => M.value, (e) => {
			cr();
		}), w(() => Y.value, (e) => {
			P.value = !0, $();
		}, { deep: !0 }), w(() => X.value, (e) => {
			$();
		}, { deep: !0 }), w(() => U.value.showDataLabels, (e) => {
			P.value = !0, $();
		}), w(() => L.value, (e) => {
			e && $();
		}), w(() => U.value.stacked, (e) => {
			P.value = !0, F.value = !0, cr();
		});
		function br() {
			hr();
		}
		let xr = $e(null), Sr = $e(null), Cr = $e(null);
		Ze(() => {
			Lt.value = !0, wr();
		});
		function wr() {
			if (ie(E.dataset) && se({
				componentName: "VueUiXyCanvas",
				type: "dataset",
				debug: Qt.value
			}), qe(() => {
				H.value && !k.value && (k.value = H.value.getContext("2d", { willReadFrequently: !0 })), k.value && B.value && (P.value = !0, F.value = !0, Bn());
			}), V.value.responsive) {
				let e = Ee(() => {
					let { width: e, height: t } = je({
						chart: O.value,
						title: V.value.style.chart.title.text ? jt.value : null,
						legend: V.value.style.chart.legend.show ? Mt.value : null,
						slicer: V.value.style.chart.zoom.show && W.value > 6 ? I.value?.$el : null,
						source: Nt.value
					});
					requestAnimationFrame(() => {
						ln.value = `${e} / ${t}`;
					});
				});
				xr.value && (Sr.value && xr.value.unobserve(Sr.value), xr.value.disconnect()), xr.value = new ResizeObserver(e), Sr.value = O.value.parentNode, xr.value.observe(Sr.value);
			}
			Cr.value && Cr.value.disconnect(), Cr.value = new ResizeObserver(async (e) => {
				for (let t of e) t.contentBoxSize && Ct.value && (P.value = !0, cr());
			}), Cr.value.observe(Ct.value), jn();
		}
		Xe(() => {
			Cr.value && Cr.value.disconnect(), xr.value && (Sr.value && xr.value.unobserve(Sr.value), xr.value.disconnect());
		});
		function Tr() {
			N.value.length ? N.value = [] : Ar.value.forEach((e, t) => {
				N.value.push(t);
			}), P.value = !0, cr(), qt("selectLegend", J.value);
		}
		function Er(e) {
			N.value.includes(e) ? N.value = N.value.filter((t) => t !== e) : N.value.push(e), P.value = !0, cr(), qt("selectLegend", J.value);
		}
		function Dr(e) {
			return q.value.length ? q.value.find((t) => t.name === e) || (Qt.value && console.warn(`VueUiXyCanvas - Series name not found "${e}"`), null) : (Qt.value && console.warn("VueUiXyCanvas - There are no series to show."), null);
		}
		function Or(e) {
			let t = Dr(e);
			t !== null && N.value.includes(t.absoluteIndex) && Er(t.absoluteIndex);
		}
		function kr(e) {
			let t = Dr(e);
			t !== null && (N.value.includes(t.absoluteIndex) || Er(t.absoluteIndex));
		}
		let Ar = m(() => q.value.map((e, t) => ({
			...e,
			name: e.name,
			color: u(e.color) || hn.value[t] || c[t] || c[t % c.length],
			shape: e.shape || "circle",
			prefix: e.prefix || "",
			suffix: e.suffix || "",
			rounding: e.rounding || 0
		})).map((e) => ({
			...e,
			opacity: N.value.includes(e.absoluteIndex) ? .5 : 1,
			segregate: () => Er(e.absoluteIndex),
			isSegregated: N.value.includes(e.absoluteIndex)
		}))), jr = m(() => ({
			cy: "donut-div-legend",
			backgroundColor: V.value.style.chart.legend.backgroundColor,
			color: V.value.style.chart.legend.color,
			fontSize: V.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: V.value.style.chart.legend.bold ? "bold" : ""
		})), Mr = m(() => {
			let e = [""].concat(J.value.map((e) => e.name), " <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>"), t = [];
			for (let e = 0; e < W.value; e += 1) {
				let n = J.value.map((t) => t.series[e] ?? 0).reduce((e, t) => e + t, 0);
				t.push([V.value.style.chart.grid.x.timeLabels.values.slice(Y.value.start, Y.value.end)[e] ? Q?.value?.slice(Y.value.start, Y.value.end)?.[e]?.text ?? e + 1 : e + 1].concat(J.value.map((t) => (t.series[e] ?? 0).toFixed(V.value.table.rounding)), (n ?? 0).toFixed(V.value.table.rounding)));
			}
			let n = {
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
			}, r = [V.value.table.columnNames.period].concat(J.value.map((e) => e.name), V.value.table.columnNames.total);
			return {
				head: e,
				body: t.slice(0, Y.value.end - Y.value.start),
				config: n,
				colNames: r
			};
		}), Nr = m(() => {
			if (J.value.length === 0) return {
				head: [],
				body: [],
				config: {},
				columnNames: []
			};
			let e = J.value.map((e) => ({
				label: e.name,
				color: e.color,
				type: e.type
			})), t = [];
			for (let e = Y.value.start; e < Y.value.end; e += 1) {
				let n = [V.value.style.chart.grid.x.timeLabels.values[e] ? Q.value[e].text : e + 1];
				J.value.forEach((t) => {
					n.push(Number((t.series[e] || 0).toFixed(V.value.table.rounding)));
				}), t.push(n);
			}
			return {
				head: e,
				body: t
			};
		});
		function Pr(e = null) {
			let n = [
				[V.value.style.chart.title.text],
				[V.value.style.chart.title.subtitle.text],
				[""]
			], i = ["", ...Nr.value.head.map((e) => e.label)], a = Nr.value.body, o = n.concat([i]).concat(a), s = r(o);
			e ? e(s) : t({
				csvContent: s,
				title: V.value.style.chart.title.text || "vue-ui-xy-canvas"
			});
		}
		function Fr() {
			return J.value;
		}
		function Ir() {
			U.value.showTable = !U.value.showTable;
		}
		function Lr() {
			U.value.showDataLabels = !U.value.showDataLabels;
		}
		function Rr() {
			U.value.stacked = !U.value.stacked;
		}
		function zr() {
			U.value.showTooltip = !U.value.showTooltip;
		}
		let Br = x(!1);
		function Vr() {
			Br.value = !Br.value;
		}
		async function Hr({ scale: e = 2 } = {}) {
			if (!O.value) return;
			let { imageUri: t, base64: n } = await Se({
				domElement: O.value,
				base64: !0,
				img: !0,
				scale: e
			}), r = O.value.getBoundingClientRect(), i = {
				width: r.width,
				height: r.height,
				aspectRatio: r.height ? r.width / r.height : 0
			}, a = await ne(t, e) ?? i;
			return {
				imageUri: t,
				base64: n,
				title: V.value.style.chart.title.text,
				...a
			};
		}
		let Ur = m(() => {
			let e = V.value.table.useDialog && !V.value.table.show, t = U.value.showTable;
			return {
				component: e ? yt : Pe,
				title: `${V.value.style.chart.title.text}${V.value.style.chart.title.subtitle.text ? `: ${V.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: V.value.table.th.backgroundColor,
					color: V.value.table.th.color,
					headerColor: V.value.table.th.color,
					headerBg: V.value.table.th.backgroundColor,
					isFullscreen: At.value,
					fullscreenParent: O.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: Zt.value
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
		w(() => U.value.showTable, (e) => {
			V.value.table.show || (e && V.value.table.useDialog && Rt.value ? Rt.value.open() : "close" in Rt.value && Rt.value.close());
		});
		function Wr() {
			U.value.showTable = !1, zt.value && zt.value.setTableIconState(!1);
		}
		function Gr(e) {
			if (e?.stage === "start") {
				Ut.value = !0;
				return;
			}
			if (e?.stage === "end") {
				Ut.value = !1;
				return;
			}
			pn();
		}
		async function Kr() {
			if (qt("copyAlt", {
				config: V.value,
				dataset: J.value
			}), !V.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(V.value.userOptions.callbacks.altCopy({
				config: V.value,
				dataset: J.value
			}));
		}
		return we({
			getData: Fr,
			getImage: Hr,
			generateCsv: Pr,
			generatePdf: fn,
			generateImage: pn,
			hideSeries: kr,
			showSeries: Or,
			toggleTable: Ir,
			toggleLabels: Lr,
			toggleStack: Rr,
			toggleTooltip: zr,
			toggleAnnotator: Vr,
			toggleFullscreen: mn,
			copyAlt: Kr
		}), (t, n) => (b(), Ve("div", {
			style: Ye(`width:100%; position:relative; ${V.value.responsive ? "height: 100%" : ""}; background:${V.value.style.chart.backgroundColor};`),
			ref_key: "xy",
			ref: O,
			id: `xy_canvas_${D.value}`,
			class: Je(`vue-data-ui-component vue-ui-xy-canvas ${At.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			onMouseenter: on,
			onMouseleave: sn
		}, [
			_("div", {
				id: `chart-instructions-${D.value}`,
				class: "sr-only"
			}, [_("p", null, et(V.value.a11y.translations.keyboardNavigation), 1)], 8, ot),
			_("div", st, et(Xt.value), 1),
			V.value.style.chart.title.text ? (b(), Ve("div", {
				key: 0,
				ref_key: "chartTitle",
				ref: jt,
				style: Ye(`width:100%;background:${V.value.style.chart.backgroundColor};`)
			}, [(b(), h(Ce, {
				key: `title_${Pt.value}`,
				config: {
					title: {
						cy: "xy-canvas-title",
						...V.value.style.chart.title
					},
					subtitle: {
						cy: "xy-canvas-subtitle",
						...V.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 4)) : g("", !0),
			_("div", { id: `legend-top-${D.value}` }, null, 8, ct),
			V.value.userOptions.show && B.value && (C(an) || C(nn)) ? (b(), h(C(_t), {
				ref_key: "userOptionsRef",
				ref: zt,
				key: `user_option_${kt.value}`,
				backgroundColor: V.value.style.chart.backgroundColor,
				color: V.value.style.chart.color,
				isPrinting: C(un),
				isImaging: C(dn),
				uid: D.value,
				hasTooltip: V.value.userOptions.buttons.tooltip && V.value.style.chart.tooltip.show,
				hasPdf: V.value.userOptions.buttons.pdf,
				hasImg: V.value.userOptions.buttons.img,
				hasXls: V.value.userOptions.buttons.csv,
				hasLabel: V.value.userOptions.buttons.labels,
				hasStack: e.dataset.length > 1 && V.value.userOptions.buttons.stack,
				hasFullscreen: V.value.userOptions.buttons.fullscreen,
				hasAltCopy: V.value.userOptions.buttons.altCopy,
				hasTable: Y.value.end - Y.value.start <= 730 && V.value.userOptions.buttons.table,
				isFullscreen: At.value,
				isTooltip: U.value.showTooltip,
				isStacked: U.value.stacked,
				titles: { ...V.value.userOptions.buttonTitles },
				chartElement: O.value,
				position: V.value.userOptions.position,
				hasAnnotator: V.value.userOptions.buttons.annotator,
				isAnnotation: Br.value,
				callbacks: V.value.userOptions.callbacks,
				printScale: V.value.userOptions.print.scale,
				tableDialog: V.value.table.useDialog,
				isCursorPointer: Zt.value,
				onToggleFullscreen: mn,
				onGeneratePdf: C(fn),
				onGenerateCsv: Pr,
				onGenerateImage: Gr,
				onToggleTable: Ir,
				onToggleLabels: Lr,
				onToggleStack: Rr,
				onToggleTooltip: zr,
				onToggleAnnotator: Vr,
				onCopyAlt: Kr,
				style: Ye({ visibility: C(an) ? C(nn) ? "visible" : "hidden" : "visible" })
			}, He({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: T(({ isOpen: e, color: n }) => [S(t.$slots, "menuIcon", y(v({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: T(() => [S(t.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: T(() => [S(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: T(() => [S(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: T(() => [S(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionTable ? {
					name: "optionTable",
					fn: T(() => [S(t.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionLabels ? {
					name: "optionLabels",
					fn: T(() => [S(t.$slots, "optionLabels", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionStack ? {
					name: "optionStack",
					fn: T(() => [S(t.$slots, "optionStack", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: T(({ toggleFullscreen: e, isFullscreen: n }) => [S(t.$slots, "optionFullscreen", y(v({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: T(({ toggleAnnotator: e, isAnnotator: n }) => [S(t.$slots, "optionAnnotator", y(v({
						toggleAnnotator: e,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: T(({ altCopy: e }) => [S(t.$slots, "optionAltCopy", y(v({ altCopy: e })), void 0, !0)]),
					key: "10"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: T(() => [S(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: T(() => [S(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasImg.hasXls.hasLabel.hasStack.hasFullscreen.hasAltCopy.hasTable.isFullscreen.isTooltip.isStacked.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.style".split("."))) : g("", !0),
			_("div", {
				class: "vue-ui-xy-canvas",
				style: Ye(`position: relative; aspect-ratio: ${ln.value}`),
				ref_key: "container",
				ref: Ct
			}, [
				_("canvas", {
					ref_key: "canvas",
					ref: H,
					"aria-label": Yt.value,
					"aria-describedby": `chart-instructions-${D.value}`,
					role: "img",
					"aria-live": "polite",
					tabindex: "0",
					style: {
						width: "100%",
						height: "100%"
					},
					onMousemove: n[0] ||= (e) => ur(e),
					onMouseleave: br,
					onFocus: gr,
					onBlur: _r,
					onKeydown: vr
				}, null, 40, lt),
				t.$slots.hint ? (b(), Ve("div", ut, [S(t.$slots, "hint", y(v({
					hint: V.value.a11y.translations.keyboardNavigation,
					isVisible: Gt.value
				})), void 0, !0)])) : g("", !0),
				S(t.$slots, "skeleton", {}, () => [C(en) ? (b(), h(ye, { key: 0 })) : g("", !0)], !0),
				We(C(ht), {
					teleportTo: V.value.style.chart.tooltip.teleportTo,
					show: U.value.showTooltip && wt.value,
					backgroundColor: V.value.style.chart.tooltip.backgroundColor,
					color: V.value.style.chart.tooltip.color,
					fontSize: V.value.style.chart.tooltip.fontSize,
					borderRadius: V.value.style.chart.tooltip.borderRadius,
					borderColor: V.value.style.chart.tooltip.borderColor,
					borderWidth: V.value.style.chart.tooltip.borderWidth,
					position: V.value.style.chart.tooltip.position,
					offsetX: V.value.style.chart.tooltip.offsetX,
					offsetY: V.value.style.chart.tooltip.offsetY,
					parent: t.$refs.xy,
					content: Tt.value,
					isFullscreen: At.value,
					backgroundOpacity: V.value.style.chart.tooltip.backgroundOpacity,
					isCustom: C(ue)(V.value.style.chart.tooltip.customFormat),
					smooth: V.value.style.chart.tooltip.smooth,
					backdropFilter: V.value.style.chart.tooltip.backdropFilter,
					smoothForce: V.value.style.chart.tooltip.smoothForce,
					smoothSnapThreshold: V.value.style.chart.tooltip.smoothSnapThreshold,
					isA11yMode: Wt.value,
					a11yPosition: Kt.value
				}, {
					"tooltip-before": T(() => [S(t.$slots, "tooltip-before", y(v({ ...Et.value })), void 0, !0)]),
					tooltip: T(() => [S(t.$slots, "tooltip", y(v({ ...Et.value })), void 0, !0)]),
					"tooltip-after": T(() => [S(t.$slots, "tooltip-after", y(v({ ...Et.value })), void 0, !0)]),
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
					"position",
					"offsetX",
					"offsetY",
					"parent",
					"content",
					"isFullscreen",
					"backgroundOpacity",
					"isCustom",
					"smooth",
					"backdropFilter",
					"smoothForce",
					"smoothSnapThreshold",
					"isA11yMode",
					"a11yPosition"
				])
			], 4),
			V.value.style.chart.zoom.show && W.value > 6 && B.value && Vt.value && !C(en) ? (b(), h(Ne, {
				key: 2,
				ref_key: "chartSlicer",
				ref: I,
				"data-dom-to-png-ignore-layout": "",
				allMinimaps: On.value,
				background: V.value.style.chart.zoom.color,
				borderColor: V.value.style.chart.backgroundColor,
				customFormat: V.value.style.chart.zoom.customFormat,
				cutNullValues: En.value,
				enableRangeHandles: V.value.style.chart.zoom.enableRangeHandles,
				enableSelectionDrag: V.value.style.chart.zoom.enableSelectionDrag,
				end: Y.value.end,
				focusOnDrag: V.value.style.chart.zoom.focusOnDrag,
				focusRangeRatio: V.value.style.chart.zoom.focusRangeRatio,
				fontSize: V.value.style.chart.zoom.fontSize,
				immediate: !V.value.style.chart.zoom.preview.enable,
				inputColor: V.value.style.chart.zoom.color,
				isPreview: Mn.value,
				labelLeft: V.value.style.chart.grid.x.timeLabels.values[Y.value.start] ? Q.value[Y.value.start]?.text : "",
				labelRight: V.value.style.chart.grid.x.timeLabels.values[Y.value.end - 1] ? Q.value[Y.value.end - 1]?.text : "",
				max: W.value,
				min: 0,
				minimap: Dn.value,
				minimapCompact: V.value.style.chart.zoom.minimap.compact,
				minimapFrameColor: V.value.style.chart.zoom.minimap.frameColor,
				minimapIndicatorColor: V.value.style.chart.zoom.minimap.indicatorColor,
				minimapLineColor: V.value.style.chart.zoom.minimap.lineColor,
				minimapMerged: V.value.style.chart.zoom.minimap.merged,
				minimapSelectedColor: V.value.style.chart.zoom.minimap.selectedColor,
				minimapSelectedColorOpacity: V.value.style.chart.zoom.minimap.selectedColorOpacity,
				minimapSelectedIndex: M.value,
				minimapSelectionRadius: V.value.style.chart.zoom.minimap.selectionRadius,
				preciseLabels: Xn.value?.length ? Xn.value : Q.value,
				refreshEndPoint: V.value.style.chart.zoom.endIndex === null ? W.value : V.value.style.chart.zoom.endIndex + 1,
				refreshStartPoint: V.value.style.chart.zoom.startIndex === null ? 0 : V.value.style.chart.zoom.startIndex,
				selectColor: V.value.style.chart.zoom.highlightColor,
				selectedSeries: q.value,
				smoothMinimap: V.value.style.chart.zoom.minimap.smooth,
				start: Y.value.start,
				timeLabels: Q.value,
				usePreciseLabels: V.value.style.chart.grid.x.timeLabels.datetimeFormatter.enable && !V.value.style.chart.zoom.useDefaultFormat,
				textColor: V.value.style.chart.color,
				useResetSlot: V.value.style.chart.zoom.useResetSlot,
				valueEnd: Y.value.end,
				valueStart: Y.value.start,
				verticalHandles: V.value.style.chart.zoom.minimap.verticalHandles,
				maxWidth: V.value.style.chart.zoom.maxWidth,
				minimapLeftInsetRatio: G.value.canvasWidth > 0 && V.value.style.chart.zoom.autoFit ? G.value.left / G.value.canvasWidth : null,
				minimapRightInsetRatio: G.value.canvasWidth > 0 && V.value.style.chart.zoom.autoFit ? (G.value.canvasWidth - G.value.right) / G.value.canvasWidth : null,
				additionalMinimapHeight: V.value.style.chart.zoom.minimap.additionalHeight,
				handleType: V.value.style.chart.zoom.minimap.handleType,
				handleIconColor: V.value.style.chart.zoom.minimap.handleIconColor,
				handleBorderWidth: V.value.style.chart.zoom.minimap.handleBorderWidth,
				handleBorderColor: V.value.style.chart.zoom.minimap.handleBorderColor,
				handleFill: V.value.style.chart.zoom.minimap.handleFill,
				handleWidth: V.value.style.chart.zoom.minimap.handleWidth,
				onFutureEnd: n[1] ||= (e) => Nn("end", e),
				onFutureStart: n[2] ||= (e) => Nn("start", e),
				onReset: jn,
				onTrapMouse: _n,
				"onUpdate:end": In,
				"onUpdate:start": Fn
			}, {
				"reset-action": T(({ reset: e }) => [S(t.$slots, "reset-action", y(v({ reset: e })), void 0, !0)]),
				_: 3
			}, 8, /* @__PURE__ */ "allMinimaps.background.borderColor.customFormat.cutNullValues.enableRangeHandles.enableSelectionDrag.end.focusOnDrag.focusRangeRatio.fontSize.immediate.inputColor.isPreview.labelLeft.labelRight.max.minimap.minimapCompact.minimapFrameColor.minimapIndicatorColor.minimapLineColor.minimapMerged.minimapSelectedColor.minimapSelectedColorOpacity.minimapSelectedIndex.minimapSelectionRadius.preciseLabels.refreshEndPoint.refreshStartPoint.selectColor.selectedSeries.smoothMinimap.start.timeLabels.usePreciseLabels.textColor.useResetSlot.valueEnd.valueStart.verticalHandles.maxWidth.minimapLeftInsetRatio.minimapRightInsetRatio.additionalMinimapHeight.handleType.handleIconColor.handleBorderWidth.handleBorderColor.handleFill.handleWidth".split("."))) : g("", !0),
			_("div", { id: `legend-bottom-${D.value}` }, null, 8, dt),
			Lt.value && (V.value.style.chart.legend.show || t.$slots.legend) ? (b(), h(Be, {
				key: 3,
				to: V.value.style.chart.legend.position === "top" ? `#legend-top-${D.value}` : `#legend-bottom-${D.value}`
			}, [_("div", {
				ref_key: "chartLegend",
				ref: Mt
			}, [S(t.$slots, "legend", { legend: Ar.value }, () => [V.value.style.chart.legend.show && B.value ? (b(), h(Re, {
				legendSet: Ar.value,
				config: jr.value,
				key: `legend_${It.value}`,
				isCursorPointer: Zt.value,
				onClickMarker: n[3] ||= ({ i: e }) => Er(e)
			}, {
				item: T(({ legend: e, index: t }) => [_("div", {
					onClick: (t) => e.segregate(),
					style: Ye(`opacity:${N.value.includes(t) ? .5 : 1}`)
				}, et(e.name), 13, ft)]),
				legendToggle: T(() => [Ar.value.length > 2 && V.value.style.chart.legend.selectAllToggle.show && !C(en) ? (b(), h(Fe, {
					key: 0,
					backgroundColor: V.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: V.value.style.chart.legend.selectAllToggle.color,
					fontSize: V.value.style.chart.legend.fontSize,
					checked: N.value.length > 0,
					isCursorPointer: Zt.value,
					onToggle: Tr
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : g("", !0)]),
				_: 1
			}, 8, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : g("", !0)], !0)], 512)], 8, ["to"])) : g("", !0),
			t.$slots.watermark ? (b(), Ve("div", pt, [S(t.$slots, "watermark", y(v({ isPrinting: C(un) || C(dn) || Ut.value })), void 0, !0)])) : g("", !0),
			t.$slots.source ? (b(), Ve("div", {
				key: 5,
				ref_key: "source",
				ref: Nt,
				dir: "auto"
			}, [S(t.$slots, "source", {}, void 0, !0)], 512)) : g("", !0),
			B.value && V.value.userOptions.buttons.table ? (b(), h(Qe(Ur.value.component), Ke({ key: 6 }, Ur.value.props, {
				ref_key: "tableUnit",
				ref: Rt,
				onClose: Wr
			}), He({
				content: T(() => [(b(), h(C(gt), {
					key: `table_${Ft.value}`,
					colNames: Mr.value.colNames,
					head: Mr.value.head,
					body: Mr.value.body,
					config: Mr.value.config,
					title: V.value.table.useDialog ? "" : Ur.value.title,
					withCloseButton: !V.value.table.useDialog,
					isCursorPointer: Zt.value,
					onClose: Wr
				}, {
					th: T(({ th: e }) => [_("div", { innerHTML: e }, null, 8, mt)]),
					td: T(({ td: e }) => [Ue(et(e), 1)]),
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
				fn: T(() => [Ue(et(Ur.value.title), 1)]),
				key: "0"
			} : void 0, V.value.table.useDialog ? {
				name: "actions",
				fn: T(() => [_("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: n[4] ||= (e) => Pr(V.value.userOptions.callbacks.csv),
					style: Ye({ cursor: Zt.value ? "pointer" : "default" })
				}, [We(Me, {
					name: "fileCsv",
					stroke: Ur.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : g("", !0),
			V.value.userOptions.buttons.annotator && J.value.length ? (b(), h(C(vt), {
				key: 7,
				parent: O.value,
				backgroundColor: V.value.style.chart.backgroundColor,
				color: V.value.style.chart.color,
				active: Br.value,
				isCursorPointer: Zt.value,
				onClose: Vr
			}, {
				"annotator-action-close": T(() => [S(t.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": T(({ color: e }) => [S(t.$slots, "annotator-action-color", y(v({ color: e })), void 0, !0)]),
				"annotator-action-draw": T(({ mode: e }) => [S(t.$slots, "annotator-action-draw", y(v({ mode: e })), void 0, !0)]),
				"annotator-action-undo": T(({ disabled: e }) => [S(t.$slots, "annotator-action-undo", y(v({ disabled: e })), void 0, !0)]),
				"annotator-action-redo": T(({ disabled: e }) => [S(t.$slots, "annotator-action-redo", y(v({ disabled: e })), void 0, !0)]),
				"annotator-action-delete": T(({ disabled: e }) => [S(t.$slots, "annotator-action-delete", y(v({ disabled: e })), void 0, !0)]),
				_: 3
			}, 8, [
				"parent",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : g("", !0)
		], 46, at));
	}
}, [["__scopeId", "data-v-366a5f0a"]]);
//#endregion
export { it as n, ht as t };
