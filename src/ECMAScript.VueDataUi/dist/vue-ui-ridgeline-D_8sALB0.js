import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, G as r, Ht as i, Jt as a, Kt as o, N as s, P as ee, Pt as c, Q as te, S as ne, V as re, X as l, ht as ie, i as u, jt as ae, pt as oe, q as se, r as ce, t as le, tt as ue, w as de } from "./lib-Bttd6u5E.js";
import { n as fe, t as pe } from "./useHints-Dq_w2E8B.js";
import { t as me } from "./useTimeLabels-d2f-W1L4.js";
import { t as he } from "./useConfig-DlNpz6P8.js";
import { t as ge } from "./usePrinter-DN5bYhTG.js";
import { n as _e, t as ve } from "./BaseScanner-DZvpgOjM.js";
import { t as ye } from "./useNestedProp-vPNvh7rV.js";
import { t as be } from "./useThemeCheck-C43Tcqmk.js";
import { t as xe } from "./useChartExport-DNiwdPmb.js";
import { t as Se } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as Ce } from "./img-Bnokohej.js";
import { n as we } from "./Title-BE3qg9xl.js";
import { t as Te } from "./Shape-C21CMlWS.js";
import { t as Ee } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as De, t as Oe } from "./useResponsive-ZtArZtUf.js";
import { t as ke } from "./DefGrad-DVBqDjhO.js";
import { t as Ae } from "./BaseLegendToggle-DZVucLnv.js";
import { t as je } from "./A11yDataTable-DdRsVULz.js";
import { t as Me } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ne } from "./useChartAccessibility-DYqac8yF.js";
import { t as Pe } from "./Legend-CQxUgOd-.js";
import { t as Fe } from "./vue_ui_ridgeline-VM8_mx4J.js";
import { Fragment as d, computed as f, createBlock as p, createCommentVNode as m, createElementBlock as h, createElementVNode as g, createSlots as Ie, createTextVNode as Le, createVNode as Re, defineAsyncComponent as _, guardReactiveProps as v, mergeProps as ze, nextTick as Be, normalizeClass as Ve, normalizeProps as y, normalizeStyle as b, onBeforeUnmount as He, openBlock as x, ref as S, renderList as C, renderSlot as w, resolveDynamicComponent as Ue, shallowRef as We, toDisplayString as T, toRefs as Ge, unref as E, watch as D, watchEffect as Ke, withCtx as O } from "vue";
//#region src/components/vue-ui-ridgeline.vue
var qe = /* @__PURE__ */ e({ default: () => _t }), Je = ["id"], Ye = ["id"], Xe = { style: { position: "relative" } }, Ze = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Qe = { key: 0 }, $e = ["fill", "d"], et = [
	"stroke",
	"stroke-width",
	"d"
], tt = ["fill", "d"], nt = [
	"stroke",
	"stroke-dasharray",
	"stroke-width",
	"d"
], rt = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], it = [
	"cx",
	"cy",
	"stroke",
	"stroke-width",
	"r",
	"fill"
], at = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill",
	"text-decoration",
	"onMouseenter",
	"onClick"
], ot = { key: 0 }, st = [
	"font-size",
	"fill",
	"font-weight",
	"transform",
	"text-anchor"
], ct = [
	"font-size",
	"fill",
	"font-weight",
	"transform",
	"text-anchor",
	"innerHTML"
], lt = [
	"x",
	"y",
	"width",
	"height",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], ut = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], dt = [
	"cx",
	"cy",
	"stroke",
	"stroke-width",
	"r",
	"fill"
], ft = [
	"x",
	"y",
	"text-anchor",
	"font-size",
	"fill"
], pt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, mt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, ht = ["onClick"], gt = ["innerHTML"], _t = /*#__PURE__*/ Ee({
	__name: "vue-ui-ridgeline",
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
		"selectX",
		"copyAlt"
	],
	setup(e, { expose: Ee, emit: qe }) {
		let _t = _(() => import("./vue-ui-xy-ChUQgqEu.js").then((e) => e.n)), vt = _(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), yt = _(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), bt = _(() => import("./DataTable-BbKgJ5UI.js")), xt = _(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), St = _(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Ct = _(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), wt = _(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_ridgeline: Tt } = he(), { isThemeValid: Et, warnInvalidTheme: Dt } = be(), k = e, Ot = f({
			get() {
				return Array.isArray(B.value) && B.value.length > 0;
			},
			set(e) {
				return e;
			}
		}), kt = qe, A = S(null), At = S(null), jt = S(null), j = We(null), M = We(null), Mt = S(null), Nt = S(null), Pt = S(0), Ft = S(0), It = S(0), N = S(se()), Lt = S(0), Rt = S(0), zt = S(512), P = S(null), F = S(null), Bt = S(null), Vt = S(0), Ht = S(null), I = S(null), Ut = S(null), L = S(null), Wt = S(!1);
		function Gt() {
			let e = ye({
				userConfig: k.config,
				defaultConfig: Tt
			}), t = e.theme;
			if (!t) return e;
			if (!Et.value(e)) return Dt(e), e;
			let n = ye({
				userConfig: Fe[t] || k.config,
				defaultConfig: e
			}), r = ye({
				userConfig: k.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : o[t] || c
			};
		}
		let R = S(Gt());
		fe({
			config: () => R.value,
			dataset: () => k.dataset,
			component: "VueUiRidgeline",
			rules: [
				pe.emptyArray,
				{
					test: (e) => e.length > 31,
					message: [
						"👀 The number of series > 31. Consider:",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display",
						"",
						"▶️ Use several instances of the component to show related series"
					]
				},
				{
					test: (e) => e.some((e) => e.datapoints.some((e) => e.values.length > 200)),
					message: [
						"👀 Some series contain > 200 data points, which can affect performance. Consider if you really need this level of detail.",
						"",
						"▶️ Use larger time scales, or aggregated values.",
						"",
						"▶️ Filter the time range by adding date inputs in your UI."
					]
				},
				{
					test: (e) => e.length < 6 && e.length > 0,
					message: [
						"👀 The number of series < 6. Consider:",
						"",
						"▶️ Using VueUiXy instead"
					]
				}
			]
		});
		let z = f(() => R.value.userOptions.useCursorPointer), Kt = f(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					areas: {
						maxPoint: { show: !1 },
						opacity: .9,
						stroke: { useSerieColor: !0 }
					},
					legend: { backgroundColor: "transparent" },
					padding: {
						right: -24,
						left: 0
					},
					xAxis: { labels: { values: [] } },
					yAxis: { labels: { fontSize: 0 } },
					zeroLine: { show: !1 }
				} }
			},
			userConfig: R.value.skeletonConfig ?? {}
		})), { loading: qt, FINAL_DATASET: B, manualLoading: Jt } = _e({
			...Ge(k),
			FINAL_CONFIG: R,
			prepareConfig: Gt,
			skeletonDataset: k.config?.skeletonDataset ?? [
				{
					name: "_",
					datapoints: [{
						name: "__",
						color: "#999999",
						values: [
							28.639,
							32.04,
							41.134,
							44.525,
							21.151,
							2.436,
							.218,
							.024,
							.002,
							0,
							0,
							0
						]
					}, {
						name: "_",
						color: "#CACACA",
						values: [
							13.253,
							15.621,
							23.36,
							33.698,
							29.935,
							10.874,
							2.364,
							.561,
							.107,
							.02,
							.006,
							.004
						]
					}]
				},
				{
					name: "_",
					datapoints: [{
						name: "_",
						color: "#999999",
						values: [
							10.851,
							13.195,
							21.617,
							36.556,
							42.292,
							21.006,
							3.398,
							.223,
							.013,
							.001,
							0,
							0
						]
					}, {
						name: "_",
						color: "#CACACA",
						values: [
							3.171,
							4.115,
							8.108,
							18.248,
							31.641,
							29.063,
							12.031,
							2.742,
							.504,
							.102,
							.032,
							.021
						]
					}]
				},
				{
					name: "_",
					datapoints: [{
						name: "_",
						color: "#999999",
						values: [
							1.731,
							2.334,
							5.125,
							13.626,
							29.911,
							38.524,
							24.168,
							7.646,
							1.575,
							.317,
							.097,
							.063
						]
					}, {
						name: "_",
						color: "#CACACA",
						values: [
							.25,
							.367,
							1.026,
							3.944,
							13.635,
							28.891,
							30.149,
							15.419,
							4.714,
							1.246,
							.442,
							.299
						]
					}]
				},
				{
					name: "_",
					datapoints: [{
						name: "_",
						color: "#999999",
						values: [
							.034,
							.054,
							.194,
							1.065,
							5.747,
							20.735,
							38.306,
							32.899,
							15.318,
							5.566,
							2.422,
							1.76
						]
					}, {
						name: "_",
						color: "#CACACA",
						values: [
							.001,
							.002,
							.009,
							.095,
							1.124,
							8.342,
							27.115,
							35.08,
							21.449,
							9.093,
							4.243,
							3.143
						]
					}]
				},
				{
					name: "_",
					datapoints: [{
						name: "_",
						color: "#999999",
						values: [
							0,
							.001,
							.004,
							.051,
							.567,
							3.322,
							14.215,
							44.783,
							40.351,
							20.377,
							9.866,
							7.378
						]
					}, {
						name: "_",
						color: "#CACACA",
						values: [
							0,
							0,
							0,
							0,
							.001,
							.11,
							4.136,
							27.498,
							43.24,
							29.807,
							17.345,
							13.678
						]
					}]
				},
				{
					name: "_",
					datapoints: [{
						name: "_",
						color: "#999999",
						values: [
							0,
							0,
							0,
							0,
							.025,
							.598,
							3.886,
							10.645,
							54.479,
							45.953,
							30.814,
							24.55
						]
					}, {
						name: "_",
						color: "#CACACA",
						values: [
							0,
							0,
							0,
							0,
							0,
							0,
							.007,
							1.655,
							26.63,
							52.017,
							45.192,
							39.651
						]
					}]
				}
			],
			skeletonConfig: a({
				defaultConfig: R.value,
				userConfig: Kt.value
			})
		}), V = S(Math.min(R.value.style.chart.areas.height, R.value.style.chart.areas.rowHeight)), { userOptionsVisible: Yt, setUserOptionsVisibility: Xt, keepUserOptionState: Zt } = Me({ config: R.value }), { svgRef: H } = Ne({ config: R.value.style.chart.title }), Qt = f(() => R.value.debug);
		function $t() {
			let e = B.value || [];
			if (!Array.isArray(e) || e.length === 0) {
				ue({
					componentName: "VueUiRidgeline",
					type: "dataset",
					debug: Qt.value
				}), Jt.value = !0;
				return;
			}
			if (e.forEach((e, t) => {
				oe({
					datasetObject: e,
					requiredAttributes: ["name", "datapoints"]
				}).forEach((e) => {
					Ot.value = !1, ue({
						componentName: "VueUiRidgeline",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: Qt.value
					});
				}), Array.isArray(e.datapoints) && e.datapoints.length && e.datapoints.forEach((e, n) => {
					oe({
						datasetObject: e,
						requiredAttributes: ["name", "values"]
					}).forEach((e) => {
						Ot.value = !1, ue({
							componentName: "VueUiRidgeline",
							type: "datasetSerieAttribute",
							property: `datapoint.${e}`,
							index: `${t}-${n}`,
							debug: Qt.value
						});
					});
				});
			}), Rt.value = e.length, V.value = Math.min(R.value.style.chart.areas.height, R.value.style.chart.areas.rowHeight), ae(k.dataset) || (Jt.value = R.value.loading), R.value.responsive) {
				let t = De(() => {
					let { width: t, height: n } = Oe({
						chart: A.value,
						title: R.value.style.chart.title.text ? At.value : null,
						legend: R.value.style.chart.legend.show ? jt.value : null,
						source: Mt.value,
						noTitle: Nt.value,
						padding: R.value.style.chart.padding
					});
					requestAnimationFrame(() => {
						zt.value = t, V.value = e.length ? n / e.length : 0, Vt.value = n - 12;
					});
				});
				j.value && (M.value && j.value.unobserve(M.value), j.value.disconnect()), j.value = new ResizeObserver(t), M.value = A.value?.parentNode || null, M.value && j.value.observe(M.value);
			}
		}
		He(() => {
			j.value && (M.value && j.value.unobserve(M.value), j.value.disconnect());
		});
		let { isPrinting: en, isImaging: tn, generatePdf: nn, generateImage: rn } = ge({
			elementId: `vue-ui-ridgeline_${N.value}`,
			fileName: R.value.style.chart.title.text || "vue-ui-ridgeline",
			options: R.value.userOptions.print
		}), an = f(() => R.value.userOptions.show && !R.value.style.chart.title.text), on = f(() => de(R.value.customPalette)), U = S({ showTable: R.value.table.show });
		D(() => B.value, async (e) => {
			Array.isArray(e) && e.length && (Yt.value = !R.value.userOptions.showOnChartHover, await Be(), $t(), Pt.value += 1, It.value += 1, U.value.showTable = R.value.table.show);
		}, {
			deep: !0,
			immediate: !0
		}), D(() => k.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Jt.value = !1);
		}, { deep: !0 }), D(() => qt.value, async (e) => {
			e || (await Be(), $t());
		}, { immediate: !0 }), D(() => k.config, () => {
			R.value = Gt(B.value || null), Yt.value = !R.value.userOptions.showOnChartHover, V.value = Math.min(R.value.style.chart.areas.height, R.value.style.chart.areas.rowHeight), U.value.showTable = R.value.table.show, Pt.value += 1, It.value += 1;
		}, { deep: !0 });
		let sn = f(() => R.value.style.chart.areas.height / R.value.style.chart.areas.rowHeight);
		function cn() {
			X.value.length ? X.value = [] : Z.value.forEach((e) => {
				X.value.push(e.id);
			}), kt("selectLegend", Y.value);
		}
		function ln(e) {
			X.value.includes(e) ? X.value = X.value.filter((t) => t !== e) : X.value.push(e), kt("selectLegend", Y.value);
		}
		function un(e) {
			return Z.value.length ? Z.value.find((t) => t.name === e) || (Qt.value && console.warn(`VueUiRidgeline - Series name not found "${e}"`), null) : (Qt.value && console.warn("VueUiRidgeline - There are no series to show."), null);
		}
		function dn(e) {
			let t = un(e);
			t !== null && X.value.includes(t.id) && ln(t.id);
		}
		function fn(e) {
			let t = un(e);
			t !== null && (X.value.includes(t.id) || ln(t.id));
		}
		let W = f(() => Ot.value ? (B.value || []).map((e) => ({
			...e,
			labelLen: En(e.name, R.value.style.chart.yAxis.labels.fontSize),
			uid: se(),
			datapoints: e.datapoints.map((e, t) => {
				let n = e.color ? ne(e.color) : on.value[t] || c[t] || c[t % c.length], r = i(e.name);
				return {
					...e,
					color: n,
					id: r
				};
			})
		})) : []), pn = f(() => R.value.style.chart.padding.top + V.value * (B.value || []).length + V.value * sn.value + R.value.style.chart.padding.bottom), G = f(() => {
			let e = R.value.style.chart.padding;
			return {
				width: zt.value,
				height: pn.value,
				padding: e
			};
		}), mn = f(() => G.value.width), hn = f(() => G.value.height), gn = S(0), _n = De((e) => {
			gn.value = e;
		}, 100);
		Ke((e) => {
			let t = Ht.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				_n(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), He(() => {
			gn.value = 0;
		});
		let K = f(() => ({
			fullHeight: G.value.height + gn.value,
			top: G.value.padding.top,
			left: G.value.padding.left,
			right: G.value.width - G.value.padding.right,
			bottom: G.value.height - R.value.style.chart.padding.bottom,
			width: G.value.width - (G.value.padding.left + G.value.padding.right)
		})), vn = f(() => Math.max(...W.value.flatMap((e) => e.datapoints.map((e) => e.values.length)))), q = S([]), yn = 0;
		Ke(() => {
			let e = ++yn;
			(async () => {
				let t = await me({
					values: R.value.style.chart.xAxis.labels.values,
					maxDatapoints: vn.value,
					formatter: R.value.style.chart.xAxis.labels.datetimeFormatter,
					start: 0,
					end: R.value.style.chart.xAxis.labels.values.length
				});
				e === yn && (q.value = t);
			})();
		});
		let J = f(() => {
			let e = Math.max(...W.value.map((e) => e.labelLen)), t = G.value.padding.left + e + 16 + R.value.style.chart.yAxis.labels.offsetX, n = (K.value.width - t) / vn.value, r = [];
			for (let e = 0; e < vn.value; e += 1) r.push({
				selectorX: t + n * e,
				x: t + n * e - n / 2,
				y: K.value.top,
				label: R.value.style.chart.xAxis.labels.values[e] ? q.value[e]?.text ?? "" : "",
				index: e,
				width: n,
				height: pn.value
			});
			return r;
		});
		function bn(e) {
			return W.value.map((t) => t.datapoints.map((t) => ({
				dp: t,
				selected: t.values[e.index]
			})));
		}
		function xn(e) {
			P.value = e;
			let t = bn(e);
			R.value.events.datapointEnter && R.value.events.datapointEnter({
				datapoint: t,
				seriesIndex: e.index
			});
		}
		function Sn(e) {
			P.value = null;
			let t = bn(e);
			R.value.events.datapointLeave && R.value.events.datapointLeave({
				datapoint: t,
				seriesIndex: e.index
			});
		}
		function Cn(e) {
			let t = bn(e);
			R.value.events.datapointClick && R.value.events.datapointClick({
				datapoint: t,
				seriesIndex: e.index
			}), kt("selectX", t);
		}
		function wn(e, t) {
			return e.length * t / 2 + t;
		}
		function Tn(e, t, n) {
			return e + wn(t, n) > K.value.right;
		}
		function En(e, t, n = "sans-serif") {
			let r = document.createElement("canvas").getContext("2d");
			return r.font = `${typeof t == "number" ? `${t}px` : t} ${n}`, r.measureText(e).width;
		}
		let Y = f(() => {
			V.value;
			let e = Math.max(...W.value.map((e) => e.labelLen)), t = Math.max(...W.value.flatMap((e) => e.datapoints.flatMap((e) => e.values))), n = Math.min(...W.value.flatMap((e) => e.datapoints.flatMap((e) => e.values))), r = G.value.padding.left + e + 16 + R.value.style.chart.yAxis.labels.offsetX, i = (K.value.width - r) / vn.value, a = Math.abs(Math.min(n, 0)), o = t + a;
			function c(e) {
				return isNaN(e / o) ? 0 : e / o;
			}
			return W.value.map((e, t) => {
				let n = K.value.top + V.value * t, o = K.value.top + n + V.value * sn.value * (1 - c(a));
				return {
					...e,
					label: {
						x: r - R.value.style.chart.yAxis.labels.fontSize,
						y: o
					},
					datapoints: e.datapoints.map((e) => {
						let t = e.values.map((t, s) => {
							let ee = isNaN(t) || [
								void 0,
								null,
								"NaN",
								NaN,
								Infinity,
								-Infinity
							].includes(t) ? 0 : t || 0;
							return {
								x: r + i * s,
								y: K.value.top + n + V.value * sn.value * (1 - c(ee + a)),
								value: t,
								isMaxPoint: t === Math.max(...e.values),
								zero: o
							};
						}), te = `${s(t, o, !1, !1)}`, ne = `M ${r},${o} ${re(t)} ${t.at(-1).x},${o}`, l = `M ${r},${o} ${t.at(-1).x},${o}`, u = `M ${ee(t)}`, ae = `M ${re(t)}`, oe = ie(R.value.style.chart.areas.smooth ? u : ae);
						return {
							...e,
							uid: se(),
							plots: t,
							smoothPath: te,
							straightPath: ne,
							zeroPath: l,
							pathLength: oe,
							smoothPathRidge: u,
							straightPathRidge: ae
						};
					}).filter((e) => !X.value.includes(e.id))
				};
			});
		}), X = S([]);
		function Dn(e) {
			let t = /* @__PURE__ */ new Map();
			return e.forEach((e) => {
				e.datapoints.forEach((e, n) => {
					let r = i(e.name);
					t.has(r) || t.set(r, {
						id: r,
						name: e.name,
						color: e.color,
						shape: "circle",
						segregate: () => ln(r),
						isSegregated: X.value.includes(r),
						opacity: X.value.includes(r) ? .5 : 1
					});
				});
			}), Array.from(t.values());
		}
		let Z = f(() => Dn(W.value)), On = f(() => ({
			cy: "donut-div-legend",
			backgroundColor: R.value.style.chart.legend.backgroundColor,
			color: R.value.style.chart.legend.color,
			fontSize: R.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: R.value.style.chart.legend.bold ? "bold" : ""
		})), Q = S(!1);
		function kn(e) {
			Q.value = e, Lt.value += 1;
		}
		let An = S({}), jn = S([]);
		function Mn(e) {
			kt("selectDatapoint", e), R.value.style.chart.dialog.show && (jn.value = e.datapoints.map((e) => ({
				name: e.name,
				color: e.color,
				type: "line",
				useArea: !0,
				smooth: R.value.style.chart.areas.smooth,
				series: e.values
			})), F.value = e, An.value = te({
				...R.value.style.chart.dialog.xyChart,
				responsive: !0,
				chart: {
					...R.value.style.chart.dialog.xyChart.chart,
					grid: {
						...R.value.style.chart.dialog.xyChart.chart.grid,
						labels: {
							...R.value.style.chart.dialog.xyChart.chart.grid.labels,
							xAxisLabels: {
								...R.value.style.chart.dialog.xyChart.chart.grid.labels.xAxisLabels,
								values: R.value.style.chart.xAxis.labels.values,
								autoRotate: {
									enable: !0,
									angle: R.value.style.chart.dialog.xyChart.chart.grid.labels.xAxisLabels.autoRotate.angle
								},
								datetimeFormatter: R.value.style.chart.xAxis.labels.datetimeFormatter
							}
						}
					},
					tooltip: {
						...R.value.style.chart.dialog.xyChart.chart.tooltip,
						showTimeLabel: R.value.style.chart.xAxis.labels.values.length > 0
					},
					userOptions: {
						...R.value.style.chart.dialog.xyChart.chart.userOptions,
						buttons: {
							...R.value.style.chart.dialog.xyChart.chart.userOptions.buttons,
							altCopy: R.value.userOptions.buttons.altCopy
						},
						callbacks: {
							...R.value.style.chart.dialog.xyChart.chart.userOptions.callbacks,
							altCopy: () => {}
						},
						useCursorPointer: R.value.userOptions.useCursorPointer
					}
				}
			}), Bt.value && Bt.value.open());
		}
		let Nn = S(null);
		function Pn(e) {
			Nn.value = e;
		}
		function Fn() {
			Nn.value = null;
		}
		let In = S(!1);
		function Ln() {
			In.value = !In.value;
		}
		function Rn() {
			U.value.showTable = !U.value.showTable;
		}
		let zn = f(() => ({ body: Y.value.flatMap((e) => e.datapoints.flatMap((t) => ({
			...t,
			rowName: `${e.name}: ${t.name}`
		}))).map((e) => [{
			name: e.rowName,
			color: e.color
		}, ...e.values]) })), $ = f(() => {
			let e = [R.value.table.columnNames.series, ...q.value.map((e) => e.text)], t = {
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
			};
			return {
				colNames: [R.value.table.columnNames.series, ...q.value.map((e) => e.text)],
				head: e,
				body: zn.value.body,
				config: t
			};
		});
		function Bn(e = null) {
			Be(() => {
				let r = [[R.value.table.columnNames.series, ...q.map((e) => [e.text])], ...zn.value.body.map((e, t) => [e[0].name, ...e.slice(1)])], i = [[R.value.style.chart.title.text], [R.value.style.chart.title.subtitle.text]].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: R.value.style.chart.title.text || "vue-ui-ridgeline"
				});
			});
		}
		function Vn() {
			return Y.value;
		}
		async function Hn({ scale: e = 2 } = {}) {
			if (!A.value) return;
			let { width: t, height: n } = A.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await Ce({
				domElement: A.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: R.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Un = f(() => ({
			min: 0,
			max: vn.value
		}));
		Se({
			timeLabelsEls: Ht,
			timeLabels: q,
			slicer: Un,
			configRef: R,
			rotationPath: [
				"style",
				"chart",
				"xAxis",
				"labels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"xAxis",
				"labels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: mn,
			height: hn,
			targetClass: ".vue-ui-ridgeline-x-axis-label",
			angle: R.value.style.chart.xAxis.labels.autoRotate.angle
		});
		let Wn = f(() => {
			let e = R.value.table.useDialog && !R.value.table.show, t = U.value.showTable;
			return {
				component: e ? wt : yt,
				title: `${R.value.style.chart.title.text}${R.value.style.chart.title.subtitle.text ? `: ${R.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: R.value.table.th.backgroundColor,
					color: R.value.table.th.color,
					headerColor: R.value.table.th.color,
					headerBg: R.value.table.th.backgroundColor,
					isFullscreen: Q.value,
					fullscreenParent: A.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: z.value
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
		D(() => U.value.showTable, (e) => {
			R.value.table.show || (e && R.value.table.useDialog && I.value ? I.value.open() : "close" in I.value && I.value.close());
		});
		function Gn() {
			U.value.showTable = !1, Ut.value && Ut.value.setTableIconState(!1);
		}
		let Kn = f(() => R.value.style.chart.backgroundColor), qn = f(() => ({
			...R.value.style.chart.legend,
			position: "bottom"
		})), Jn = f(() => R.value.style.chart.title), { isCallbackImaging: Yn, isCallbackSvg: Xn, generateSvg: Zn, onGenerateImage: Qn } = xe({
			svg: H,
			title: Jn,
			legend: qn,
			legendItems: Z,
			backgroundColor: Kn,
			getSvgCallback: () => R.value.userOptions.callbacks.svg,
			generateImage: rn
		});
		function $n(e) {
			er("xy-zoom", e.dataset, e.config);
		}
		async function er(e = "main-chart", t = null, n = null) {
			if (kt("copyAlt", {
				source: e,
				config: e === "main-chart" ? R.value : An.value,
				dataset: e === "main-chart" ? Y.value : jn.value
			}), !R.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(R.value.userOptions.callbacks.altCopy({
				source: e,
				config: e === "main-chart" ? R.value : An.value,
				dataset: e === "main-chart" ? Y.value : jn.value
			}));
		}
		function tr() {
			L.value = null, Wt.value = !0;
		}
		function nr() {
			or(), Wt.value = !1;
		}
		function rr(e) {
			if (!H.value || In.value || document.activeElement !== H.value || !Y.value.length) return;
			let t = e.key === "ArrowUp", n = e.key === "ArrowDown", r = e.key === "ArrowLeft", i = e.key === "ArrowRight", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				or();
				return;
			}
			if (a) {
				if (L.value !== null) {
					let e = Y.value[L.value];
					e && Mn(e);
					return;
				}
				P.value && Cn(P.value);
				return;
			}
			if (t || n) {
				let e = L.value;
				e = e === null ? n ? 0 : Y.value.length - 1 : ir(e + (n ? 1 : -1)), L.value = e, Nn.value = e;
				return;
			}
			if (!J.value.length) return;
			let s = P.value?.index ?? null;
			s = s === null ? i ? 0 : J.value.length - 1 : ar(s + (i ? 1 : -1));
			let ee = J.value[s];
			ee && xn(ee);
		}
		function ir(e) {
			let t = Y.value.length;
			return t ? (e % t + t) % t : null;
		}
		function ar(e) {
			let t = J.value.length;
			return t ? (e % t + t) % t : null;
		}
		function or() {
			L.value = null, Nn.value = null, P.value ? Sn(P.value) : P.value = null;
		}
		let sr = f(() => ({
			head: $.value.head,
			body: $.value.body.map((e) => [e[0]?.name ?? "", ...e.slice(1)]),
			caption: R.value.a11y.translations.tableCaption,
			notice: R.value.a11y.translations.tableAvailable
		}));
		return Ee({
			getData: Vn,
			getImage: Hn,
			generateImage: rn,
			generateSvg: Zn,
			generatePdf: nn,
			generateCsv: Bn,
			hideSeries: fn,
			showSeries: dn,
			toggleAnnotator: Ln,
			toggleTable: Rn,
			toggleFullscreen: kn,
			copyAlt: er
		}), (e, t) => (x(), h("div", {
			ref_key: "ridgelineChart",
			ref: A,
			class: Ve(`vue-data-ui-component vue-ui-ridgeline ${Q.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			id: `vue-ui-ridgeline_${N.value}`,
			style: b({
				fontFamily: R.value.style.fontFamily,
				width: "100%",
				textAlign: "center",
				background: R.value.style.chart.backgroundColor,
				height: R.value.responsive ? "100%" : void 0
			}),
			onMouseenter: t[3] ||= () => E(Xt)(!0),
			onMouseleave: t[4] ||= () => E(Xt)(!1)
		}, [
			g("div", {
				id: `chart-instructions-${N.value}`,
				class: "sr-only"
			}, [g("p", null, T(R.value.a11y.translations.keyboardNavigation), 1)], 8, Ye),
			sr.value.body.length ? (x(), p(je, {
				key: 0,
				uid: N.value,
				head: sr.value.head,
				body: sr.value.body,
				caption: sr.value.caption,
				notice: sr.value.notice
			}, null, 8, [
				"uid",
				"head",
				"body",
				"caption",
				"notice"
			])) : m("", !0),
			R.value.userOptions.buttons.annotator && E(H) ? (x(), p(E(xt), {
				key: 1,
				color: R.value.style.chart.color,
				backgroundColor: R.value.style.chart.backgroundColor,
				active: In.value,
				svgRef: E(H),
				isCursorPointer: z.value,
				onClose: Ln
			}, {
				"annotator-action-close": O(() => [w(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": O(({ color: t }) => [w(e.$slots, "annotator-action-color", y(v({ color: t })), void 0, !0)]),
				"annotator-action-draw": O(({ mode: t }) => [w(e.$slots, "annotator-action-draw", y(v({ mode: t })), void 0, !0)]),
				"annotator-action-undo": O(({ disabled: t }) => [w(e.$slots, "annotator-action-undo", y(v({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": O(({ disabled: t }) => [w(e.$slots, "annotator-action-redo", y(v({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": O(({ disabled: t }) => [w(e.$slots, "annotator-action-delete", y(v({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"color",
				"backgroundColor",
				"active",
				"svgRef",
				"isCursorPointer"
			])) : m("", !0),
			w(e.$slots, "userConfig", {}, void 0, !0),
			an.value ? (x(), h("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Nt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : m("", !0),
			R.value.style.chart.title.text ? (x(), h("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: At,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(x(), p(we, {
				key: `title_${Pt.value}`,
				config: {
					title: {
						cy: "ridgeline-div-title",
						...R.value.style.chart.title
					},
					subtitle: {
						cy: "ridgeline-div-subtitle",
						...R.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : m("", !0),
			R.value.userOptions.show && Ot.value && (E(Zt) || E(Yt)) ? (x(), p(E(St), {
				ref_key: "userOptionsRef",
				ref: Ut,
				key: `user_option_${Lt.value}`,
				backgroundColor: R.value.style.chart.backgroundColor,
				color: R.value.style.chart.color,
				isPrinting: E(en),
				isImaging: E(tn),
				uid: N.value,
				hasTooltip: !1,
				callbacks: R.value.userOptions.callbacks,
				hasPdf: R.value.userOptions.buttons.pdf,
				hasImg: R.value.userOptions.buttons.img,
				hasSvg: R.value.userOptions.buttons.svg,
				hasXls: R.value.userOptions.buttons.csv,
				hasTable: R.value.userOptions.buttons.table,
				hasLabel: !1,
				hasFullscreen: R.value.userOptions.buttons.fullscreen,
				hasAltCopy: R.value.userOptions.buttons.altCopy,
				isFullscreen: Q.value,
				printScale: R.value.userOptions.print.scale,
				chartElement: A.value,
				position: R.value.userOptions.position,
				isTooltip: !1,
				titles: { ...R.value.userOptions.buttonTitles },
				hasAnnotator: R.value.userOptions.buttons.annotator,
				isAnnotation: In.value,
				tableDialog: R.value.table.useDialog,
				style: b({ visibility: E(Zt) ? E(Yt) ? "visible" : "hidden" : "visible" }),
				isCursorPointer: z.value,
				onToggleFullscreen: kn,
				onGeneratePdf: E(nn),
				onGenerateCsv: Bn,
				onGenerateImage: E(Qn),
				onGenerateSvg: E(Zn),
				onToggleTable: Rn,
				onToggleAnnotator: Ln,
				onCopyAlt: er
			}, Ie({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: O(({ isOpen: t, color: n }) => [w(e.$slots, "menuIcon", y(v({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: O(() => [w(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: O(() => [w(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: O(() => [w(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: O(() => [w(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: O(() => [w(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: O(() => [w(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: O(({ toggleFullscreen: t, isFullscreen: n }) => [w(e.$slots, "optionFullscreen", y(v({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: O(({ toggleAnnotator: t, isAnnotator: n }) => [w(e.$slots, "optionAnnotator", y(v({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: O(({ altCopy: t }) => [w(e.$slots, "optionAltCopy", y(v({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: O(() => [w(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: O(() => [w(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.callbacks.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.printScale.chartElement.position.titles.hasAnnotator.isAnnotation.tableDialog.style.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg".split("."))) : m("", !0),
			g("div", Xe, [(x(), h("svg", {
				ref_key: "svgRef",
				ref: H,
				xmlns: E(le),
				"aria-describedby": `chart-instructions-${N.value}`,
				class: Ve({
					"vue-data-ui-fullscreen--on": Q.value,
					"vue-data-ui-fulscreen--off": !Q.value
				}),
				viewBox: `0 0 ${G.value.width <= 0 ? 10 : G.value.width} ${K.value.fullHeight <= 0 ? 10 : K.value.fullHeight}`,
				style: b(`max-width:100%;overflow:visible;background:transparent;color:${R.value.style.chart.color};${R.value.responsive ? `height: ${Vt.value}px; width: 100%;` : ""}`),
				tabindex: "0",
				onFocus: tr,
				onBlur: nr,
				onKeydown: rr
			}, [
				Re(E(Ct)),
				g("defs", null, [(x(!0), h(d, null, C(Z.value, (e, t) => (x(), p(ke, {
					t: "linear",
					id: `gradient-${e.id}-${N.value}`,
					key: `gradient-${e.id}-${N.value}`,
					x1: "50%",
					y1: "0%",
					x2: "50%",
					y2: "100%",
					stops: [
						[
							"0%",
							e.color,
							1
						],
						[
							"30%",
							e.color,
							.7
						],
						[
							"70%",
							e.color,
							.3
						],
						[
							"100%",
							e.color,
							.1
						]
					]
				}, null, 8, ["id", "stops"]))), 128)), (x(!0), h(d, null, C(Y.value, (e, t) => (x(), h(d, null, [(x(!0), h(d, null, C(e.datapoints, (e, t) => (x(), p(ke, {
					t: "linear",
					key: `grad${e.id}`,
					id: `gradient-single-${N.value}-${e.uid}`,
					x1: "50%",
					y1: "0%",
					x2: "50%",
					y2: "100%",
					stops: [
						[
							"0%",
							e.color,
							1
						],
						[
							"30%",
							e.color,
							.7
						],
						[
							"70%",
							e.color,
							.3
						],
						[
							"100%",
							e.color,
							.1
						]
					]
				}, null, 8, ["id", "stops"]))), 128))], 64))), 256))]),
				(x(!0), h(d, null, C(Y.value, (t, n) => (x(), h("g", { key: `ds-${n}` }, [(x(!0), h(d, null, C(t.datapoints, (t, r) => (x(), h("g", { key: t.id }, [
					e.$slots.pattern ? (x(), h("g", Qe, [g("defs", null, [w(e.$slots, "pattern", ze({ ref_for: !0 }, {
						datapointIndex: n,
						seriesIndex: r,
						patternId: `pattern_${N.value}_${t.uid}`
					}), void 0, !0)])])) : m("", !0),
					g("path", {
						fill: e.$slots.pattern ? `url(#pattern_${N.value}_${t.uid})` : R.value.style.chart.backgroundColor,
						stroke: "none",
						"stroke-linecap": "round",
						d: R.value.style.chart.areas.smooth ? t.smoothPath : t.straightPath,
						style: b({ opacity: R.value.style.chart.areas.opacity })
					}, null, 12, $e),
					g("path", {
						fill: "none",
						stroke: R.value.style.chart.areas.stroke.useSerieColor ? t.color : R.value.style.chart.areas.stroke.color,
						"stroke-width": R.value.style.chart.areas.strokeWidth,
						d: R.value.style.chart.areas.smooth ? t.smoothPathRidge : t.straightPathRidge,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						style: b({
							strokeDasharray: t.pathLength,
							strokeDashoffset: R.value.useCssAnimation ? t.pathLength : 0
						})
					}, null, 12, et),
					g("path", {
						fill: R.value.style.chart.areas.useGradient ? R.value.style.chart.areas.useCommonColor ? `url(#gradient-${t.id}-${N.value})` : `url(#gradient-single-${N.value}-${t.uid})` : t.color,
						stroke: "none",
						d: R.value.style.chart.areas.smooth ? t.smoothPath : t.straightPath,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						style: b({
							strokeDasharray: t.pathLength,
							strokeDashoffset: R.value.useCssAnimation ? t.pathLength : 0
						})
					}, null, 12, tt),
					R.value.style.chart.zeroLine.show ? (x(), h("path", {
						key: 1,
						stroke: R.value.style.chart.zeroLine.useSerieColor ? t.color : R.value.style.chart.zeroLine.stroke,
						"stroke-dasharray": R.value.style.chart.zeroLine.strokeDasharray,
						"stroke-width": R.value.style.chart.zeroLine.strokeWidth,
						d: t.zeroPath,
						"stroke-linecap": "round"
					}, null, 8, nt)) : m("", !0),
					R.value.style.chart.areas.maxPoint.show && t.plots.length > 1 ? (x(!0), h(d, { key: 2 }, C(t.plots, (e) => (x(), h(d, null, [e.isMaxPoint ? (x(), h("line", {
						key: 0,
						x1: e.x,
						y1: e.y,
						x2: e.x,
						y2: e.zero,
						stroke: R.value.style.chart.areas.maxPoint.adaptStrokeToBackground ? E(ce)(t.color) : R.value.style.chart.areas.maxPoint.stroke,
						"stroke-width": R.value.style.chart.areas.maxPoint.strokeWidth,
						"stroke-linecap": "round",
						"stroke-dasharray": R.value.style.chart.areas.maxPoint.strokeDasharray
					}, null, 8, rt)) : m("", !0)], 64))), 256)) : m("", !0),
					t.plots.length === 1 ? (x(), h("circle", {
						key: 3,
						cx: t.plots[0].x,
						cy: t.plots[0].y,
						stroke: R.value.style.chart.selector.dot.stroke,
						"stroke-width": R.value.style.chart.selector.dot.strokeWidth,
						r: R.value.style.chart.selector.dot.radius,
						fill: R.value.style.chart.selector.dot.useDatapointColor ? t.color : R.value.style.chart.selector.dot.fill,
						style: { pointerEvents: "none" }
					}, null, 8, it)) : m("", !0)
				]))), 128)), g("text", {
					x: t.label.x,
					y: t.label.y,
					"text-anchor": "end",
					"font-size": R.value.style.chart.yAxis.labels.fontSize,
					"font-weight": R.value.style.chart.yAxis.labels.bold ? "bold" : "normal",
					fill: R.value.style.chart.yAxis.labels.color,
					style: b({ cursor: R.value.style.chart.dialog.show && z.value ? "pointer" : "default" }),
					"text-decoration": R.value.style.chart.dialog.show && (Nn.value === n || F.value && t.uid === F.value.uid || L.value === n) ? "underline" : "",
					onMouseenter: (e) => Pn(n),
					onMouseleave: Fn,
					onClick: (e) => Mn(t)
				}, T(t.name), 45, at)]))), 128)),
				R.value.style.chart.xAxis.labels.values.length ? (x(), h("g", {
					key: 0,
					ref_key: "timeLabelsEls",
					ref: Ht
				}, [(x(!0), h(d, null, C(J.value, (t, n) => w(e.$slots, "time-label", ze({ ref_for: !0 }, {
					show: t && !R.value.style.chart.xAxis.labels.showOnlyFirstAndLast && !R.value.style.chart.xAxis.labels.showOnlyAtModulo || t && R.value.style.chart.xAxis.labels.showOnlyFirstAndLast && (n === 0 || n === J.value.length - 1) || t && P.value && P.value.index === n || t && !R.value.style.chart.xAxis.labels.showOnlyFirstAndLast && R.value.style.chart.xAxis.labels.showOnlyAtModulo && n % Math.floor(J.value.length / R.value.style.chart.xAxis.labels.modulo) === 0,
					fontSize: R.value.style.chart.xAxis.labels.fontSize,
					content: t.label,
					textAnchor: R.value.style.chart.xAxis.labels.rotation > 0 ? "start" : R.value.style.chart.xAxis.labels.rotation < 0 ? "end" : "middle",
					fill: R.value.style.chart.xAxis.labels.color,
					transform: `translate(${t.selectorX}, ${K.value.top + t.height + R.value.style.chart.xAxis.labels.offsetY}), rotate(${R.value.style.chart.xAxis.labels.rotation})`,
					x: t.selectorX,
					y: K.value.bottom + R.value.style.chart.xAxis.labels.offsetY
				}), () => [t && !R.value.style.chart.xAxis.labels.showOnlyFirstAndLast && !R.value.style.chart.xAxis.labels.showOnlyAtModulo || t && R.value.style.chart.xAxis.labels.showOnlyFirstAndLast && (n === 0 || n === J.value.length - 1) || t && P.value && P.value.index === n || t && !R.value.style.chart.xAxis.labels.showOnlyFirstAndLast && R.value.style.chart.xAxis.labels.showOnlyAtModulo && n % Math.floor(J.value.length / R.value.style.chart.xAxis.labels.modulo) === 0 ? (x(), h("g", ot, [String(t.label).includes("\n") ? (x(), h("text", {
					key: 1,
					class: "vue-ui-ridgeline-x-axis-label",
					"font-size": R.value.style.chart.xAxis.labels.fontSize,
					fill: R.value.style.chart.xAxis.labels.color,
					"font-weight": R.value.style.chart.xAxis.labels.bold ? "bold" : "normal",
					transform: `translate(${t.selectorX}, ${K.value.bottom + R.value.style.chart.xAxis.labels.offsetY}), rotate(${R.value.style.chart.xAxis.labels.rotation})`,
					"text-anchor": R.value.style.chart.xAxis.labels.rotation > 0 ? "start" : R.value.style.chart.xAxis.labels.rotation < 0 ? "end" : "middle",
					style: b({ opacity: P.value ? P.value.index === n ? 1 : .2 : 1 }),
					innerHTML: E(r)({
						content: String(t.label),
						fontSize: R.value.style.chart.xAxis.labels.fontSize,
						fill: R.value.style.chart.xAxis.labels.color,
						x: 0,
						y: 0
					})
				}, null, 12, ct)) : (x(), h("text", {
					key: 0,
					class: "vue-ui-ridgeline-x-axis-label",
					"font-size": R.value.style.chart.xAxis.labels.fontSize,
					fill: R.value.style.chart.xAxis.labels.color,
					"font-weight": R.value.style.chart.xAxis.labels.bold ? "bold" : "normal",
					transform: `translate(${t.selectorX}, ${K.value.bottom + R.value.style.chart.xAxis.labels.offsetY}), rotate(${R.value.style.chart.xAxis.labels.rotation})`,
					"text-anchor": R.value.style.chart.xAxis.labels.rotation > 0 ? "start" : R.value.style.chart.xAxis.labels.rotation < 0 ? "end" : "middle",
					style: b({ opacity: P.value ? P.value.index === n ? 1 : .2 : 1 })
				}, T(t.label), 13, st))])) : m("", !0)], !0)), 256))], 512)) : m("", !0),
				g("g", null, [
					(x(!0), h(d, null, C(J.value, (e, t) => (x(), h("rect", {
						x: e.x,
						y: e.y,
						width: e.width < 0 ? .1 : e.width,
						height: e.height < 0 ? .1 : e.height,
						fill: "transparent",
						onMouseenter: (t) => xn(e),
						onMouseleave: (t) => Sn(e),
						onClick: () => Cn(e)
					}, null, 40, lt))), 256)),
					R.value.style.chart.selector.show && P.value ? (x(), h("line", {
						key: 0,
						x1: P.value.selectorX,
						x2: P.value.selectorX,
						y1: P.value.y,
						y2: P.value.y + P.value.height - V.value / 2,
						stroke: R.value.style.chart.selector.stroke,
						"stroke-width": R.value.style.chart.selector.strokeWidth,
						"stroke-dasharray": R.value.style.chart.selector.strokeDasharray,
						"stroke-linecap": "round",
						style: { pointerEvents: "none" }
					}, null, 8, ut)) : m("", !0),
					P.value ? (x(!0), h(d, { key: 1 }, C(Y.value, (e) => (x(), h(d, null, [(x(!0), h(d, null, C(e.datapoints, (e) => (x(), h(d, null, [(x(!0), h(d, null, C(e.plots, (t, n) => (x(), h(d, null, [P.value && P.value.index === n ? (x(), h("circle", {
						key: 0,
						cx: t.x,
						cy: t.y,
						stroke: R.value.style.chart.selector.dot.stroke,
						"stroke-width": R.value.style.chart.selector.dot.strokeWidth,
						r: R.value.style.chart.selector.dot.radius,
						fill: R.value.style.chart.selector.dot.useDatapointColor ? e.color : R.value.style.chart.selector.dot.fill,
						style: { pointerEvents: "none" }
					}, null, 8, dt)) : m("", !0), P.value && P.value.index === n ? (x(), h("text", {
						key: 1,
						x: Tn(t.x, E(u)(R.value.style.chart.selector.labels.formatter, t.value, E(l)({
							p: R.value.style.chart.xAxis.labels.prefix,
							v: t.value,
							s: R.value.style.chart.xAxis.labels.suffix,
							r: R.value.style.chart.selector.labels.rounding
						})), R.value.style.chart.selector.labels.fontSize) ? t.x - R.value.style.chart.selector.labels.fontSize / 2 : t.x + R.value.style.chart.selector.labels.fontSize / 2,
						y: t.y + R.value.style.chart.selector.labels.fontSize / 3,
						"text-anchor": Tn(t.x, E(u)(R.value.style.chart.selector.labels.formatter, t.value, E(l)({
							p: R.value.style.chart.xAxis.labels.prefix,
							v: t.value,
							s: R.value.style.chart.xAxis.labels.suffix,
							r: R.value.style.chart.selector.labels.rounding
						})), R.value.style.chart.selector.labels.fontSize) ? "end" : "start",
						"font-size": R.value.style.chart.selector.labels.fontSize,
						fill: R.value.style.chart.selector.labels.color,
						style: { pointerEvents: "none" }
					}, T(E(u)(R.value.style.chart.selector.labels.formatter, t.value, E(l)({
						p: R.value.style.chart.xAxis.labels.prefix,
						v: t.value,
						s: R.value.style.chart.xAxis.labels.suffix,
						r: R.value.style.chart.selector.labels.rounding
					}))), 9, ft)) : m("", !0)], 64))), 256))], 64))), 256))], 64))), 256)) : m("", !0)
				]),
				w(e.$slots, "svg", { svg: {
					...G.value,
					drawingArea: K.value,
					isPrintingImg: E(en) || E(tn) || E(Yn),
					isPrintingSvg: E(Xn)
				} }, void 0, !0)
			], 46, Ze)), e.$slots.hint ? (x(), h("div", pt, [w(e.$slots, "hint", y(v({
				hint: R.value.a11y.translations.keyboardNavigation,
				isVisible: Wt.value
			})), void 0, !0)])) : m("", !0)]),
			e.$slots.watermark ? (x(), h("div", mt, [w(e.$slots, "watermark", y(v({ isPrinting: E(en) || E(tn) || E(Yn) || E(Xn) })), void 0, !0)])) : m("", !0),
			g("div", {
				ref_key: "chartLegend",
				ref: jt
			}, [w(e.$slots, "legend", { legend: Z.value }, () => [R.value.style.chart.legend.show ? (x(), p(Pe, {
				key: `legend_${It.value}`,
				legendSet: Z.value,
				config: On.value,
				isCursorPointer: z.value,
				onClickMarker: t[0] ||= ({ legend: e }) => ln(e.id)
			}, Ie({
				item: O(({ legend: e }) => [E(qt) ? m("", !0) : (x(), h("div", {
					key: 0,
					style: b(`opacity:${X.value.includes(e.id) ? .5 : 1}`),
					onClick: (t) => e.segregate()
				}, T(e.name), 13, ht))]),
				legendToggle: O(() => [Z.value.length > 2 && R.value.style.chart.legend.selectAllToggle.show && !E(qt) ? (x(), p(Ae, {
					key: 0,
					backgroundColor: R.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: R.value.style.chart.legend.selectAllToggle.color,
					fontSize: R.value.style.chart.legend.fontSize,
					checked: X.value.length > 0,
					isCursorPointer: z.value,
					onToggle: cn
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : m("", !0)]),
				_: 2
			}, [e.$slots.pattern ? {
				name: "legend-pattern",
				fn: O(({ legend: e, index: t }) => [Re(Te, {
					shape: e.shape,
					radius: 30,
					stroke: "none",
					plot: {
						x: 30,
						y: 30
					},
					fill: `url(#pattern_${N.value}_${t})`
				}, null, 8, ["shape", "fill"])]),
				key: "0"
			} : void 0]), 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : m("", !0)], !0)], 512),
			e.$slots.source ? (x(), h("div", {
				key: 6,
				ref_key: "source",
				ref: Mt,
				dir: "auto"
			}, [w(e.$slots, "source", {}, void 0, !0)], 512)) : m("", !0),
			Ot.value && R.value.userOptions.buttons.table ? (x(), p(Ue(Wn.value.component), ze({ key: 7 }, Wn.value.props, {
				ref_key: "tableUnit",
				ref: I,
				onClose: Gn
			}), Ie({
				content: O(() => [(x(), p(E(bt), {
					key: `table_${Ft.value}`,
					colNames: $.value.colNames,
					head: $.value.head,
					body: $.value.body,
					config: $.value.config,
					title: R.value.table.useDialog ? "" : Wn.value.title,
					withCloseButton: !R.value.table.useDialog,
					isCursorPointer: z.value,
					onClose: Gn
				}, {
					th: O(({ th: e }) => [g("div", { innerHTML: e }, null, 8, gt)]),
					td: O(({ td: e }) => [Le(T(e.name ? e.name : E(u)(R.value.style.chart.selector.labels.formatter, e, E(l)({
						p: R.value.style.chart.xAxis.labels.prefix,
						v: e,
						s: R.value.style.chart.xAxis.labels.suffix,
						r: R.value.table.td.roundingValue
					}))), 1)]),
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
			}, [R.value.table.useDialog ? {
				name: "title",
				fn: O(() => [Le(T(Wn.value.title), 1)]),
				key: "0"
			} : void 0, R.value.table.useDialog ? {
				name: "actions",
				fn: O(() => [g("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Bn(R.value.userOptions.callbacks.csv),
					style: b({ cursor: z.value ? "pointer" : "default" })
				}, [Re(E(vt), {
					name: "fileCsv",
					stroke: Wn.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : m("", !0),
			R.value.style.chart.dialog.show ? (x(), p(E(wt), {
				key: 8,
				ref_key: "dialog",
				ref: Bt,
				onClose: t[2] ||= (e) => F.value = null,
				backgroundColor: R.value.style.chart.dialog.backgroundColor,
				color: R.value.style.chart.dialog.color,
				headerBg: R.value.style.chart.dialog.header.backgroundColor,
				headerColor: R.value.style.chart.dialog.header.color,
				isFullscreen: Q.value,
				fullscreenParent: A.value,
				isCursorPointer: z.value,
				withPadding: ""
			}, {
				title: O(() => [Le(T(F.value.name), 1)]),
				content: O(() => [F.value ? (x(), p(E(_t), {
					key: 0,
					config: An.value,
					dataset: jn.value,
					onCopyAlt: $n
				}, null, 8, ["config", "dataset"])) : m("", !0)]),
				_: 1
			}, 8, [
				"backgroundColor",
				"color",
				"headerBg",
				"headerColor",
				"isFullscreen",
				"fullscreenParent",
				"isCursorPointer"
			])) : m("", !0),
			w(e.$slots, "skeleton", {}, () => [E(qt) ? (x(), p(ve, { key: 0 })) : m("", !0)], !0)
		], 46, Je));
	}
}, [["__scopeId", "data-v-861273fd"]]);
//#endregion
export { qe as n, _t as t };
