import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, G as r, Gt as i, Jt as a, Kt as o, P as s, Pt as c, S as ee, V as te, X as ne, _ as re, ct as ie, ht as ae, i as oe, jt as se, pt as ce, q as le, qt as ue, t as de, tt as fe, w as pe, xt as me } from "./lib-Bttd6u5E.js";
import { n as he, t as ge } from "./useHints-Dq_w2E8B.js";
import { t as _e } from "./useConfig-DlNpz6P8.js";
import { t as ve } from "./usePrinter-DN5bYhTG.js";
import { n as ye, t as be } from "./BaseScanner-DZvpgOjM.js";
import { t as xe } from "./useNestedProp-vPNvh7rV.js";
import { t as Se } from "./useThemeCheck-C43Tcqmk.js";
import { t as Ce } from "./useChartExport-DNiwdPmb.js";
import { t as we } from "./useTransitions-g_zBREk2.js";
import { t as Te } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as Ee } from "./img-Bnokohej.js";
import { n as De } from "./Title-BE3qg9xl.js";
import { t as Oe } from "./Shape-C21CMlWS.js";
import { t as ke } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Ae, t as je } from "./useResponsive-ZtArZtUf.js";
import { t as Me } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Ne } from "./A11yDataTable-DdRsVULz.js";
import { t as Pe } from "./useUserOptionState-DK-_1ddE.js";
import { t as Fe } from "./useChartAccessibility-DYqac8yF.js";
import { t as Ie } from "./Legend-CQxUgOd-.js";
import { t as Le } from "./vue_ui_parallel_coordinate_plot-CBiOBira.js";
import { Fragment as l, Teleport as Re, computed as u, createBlock as d, createCommentVNode as f, createElementBlock as p, createElementVNode as m, createSlots as ze, createTextVNode as Be, createVNode as Ve, defineAsyncComponent as h, guardReactiveProps as g, mergeProps as He, nextTick as Ue, normalizeClass as _, normalizeProps as v, normalizeStyle as y, onBeforeUnmount as We, onMounted as Ge, openBlock as b, ref as x, renderList as S, renderSlot as C, resolveDynamicComponent as Ke, shallowRef as qe, toDisplayString as w, toRefs as Je, unref as T, watch as Ye, watchEffect as Xe, withCtx as E } from "vue";
//#region src/components/vue-ui-parallel-coordinate-plot.vue
var Ze = /* @__PURE__ */ e({ default: () => Ct }), Qe = ["id"], $e = ["id"], et = ["id"], tt = { style: { position: "relative" } }, nt = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], rt = [
	"x",
	"y",
	"width",
	"height"
], it = { style: { "pointer-events": "none" } }, at = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], ot = [
	"d",
	"stroke",
	"stroke-width"
], st = { key: 0 }, ct = [
	"transform",
	"fill",
	"font-size",
	"font-weight"
], lt = [
	"fill",
	"font-size",
	"font-weight",
	"text-anchor",
	"transform"
], ut = [
	"fill",
	"font-size",
	"font-weight",
	"text-anchor",
	"transform",
	"innerHTML"
], dt = { key: 0 }, ft = [
	"x",
	"y",
	"font-size",
	"fill"
], pt = [
	"width",
	"x",
	"y"
], mt = { style: { width: "100%" } }, ht = [
	"d",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], gt = [
	"d",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], _t = [
	"transform",
	"fill",
	"font-weight",
	"font-size",
	"stroke",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], vt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, yt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, bt = ["id"], xt = ["onClick"], St = ["innerHTML"], Ct = /*#__PURE__*/ ke({
	__name: "vue-ui-parallel-coordinate-plot",
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
	setup(e, { expose: ke, emit: Ze }) {
		let Ct = h(() => import("./Tooltip-DhjyfHwz.js")), wt = h(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Tt = h(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Et = h(() => import("./DataTable-BbKgJ5UI.js")), Dt = h(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Ot = h(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), kt = h(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), At = h(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_parallel_coordinate_plot: jt } = _e(), { isThemeValid: Mt, warnInvalidTheme: Nt } = Se(), D = e, Pt = u({
			get() {
				return !!D.dataset && D.dataset.length;
			},
			set(e) {
				return e;
			}
		}), Ft = x(0), O = x(null), It = x(null), Lt = x(null), Rt = x(null), zt = x(null), Bt = x(0), Vt = x(0), Ht = x(0), Ut = x(!1), k = x(null), Wt = x(null), Gt = x(null), A = x(null), Kt = x({
			x: 0,
			y: 0
		}), qt = x("pointer"), Jt = x(!1), j = x(le()), M = x(!1);
		function Yt(e) {
			M.value = e, Ft.value += 1;
		}
		let N = x(rn()), { transitionEnabled: P } = we({
			config: () => N.value.transitions,
			dataset: () => D.dataset
		});
		he({
			config: () => N.value,
			dataset: () => D.dataset,
			component: "VueUiParallelCoordinatePlot",
			rules: [
				ge.emptyArray,
				{
					test: (e) => e.length > 10,
					message: [
						"👀 There are > 10 series. Consider:",
						"",
						"▶️ Using filters, to show less series at the same time and make the chart more readable."
					]
				},
				{
					test: (e) => e.some((e) => (e?.series ?? []).some((e) => (e?.values ?? []).length > 15)),
					message: [
						"👀 There are > 15 axes, which can make the chart hard to read. Consider:",
						"",
						"▶️ Using filters, to allow users to select a maximum set of metrics."
					]
				},
				{
					test: (e) => e.some((e) => e?.series?.length > 5),
					message: [
						"👀 Some series have > 5 datapoints. Consider:",
						"",
						"▶️ Using filters, to allow users to select a maximum set of metrics."
					]
				}
			]
		});
		let F = u(() => N.value.userOptions.useCursorPointer), Xt = u(() => a({
			defaultConfig: {
				useCssAnimation: !1,
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					legend: {
						show: !0,
						backgroundColor: "transparent"
					},
					yAxis: {
						stroke: "#6A6A6A",
						labels: {
							showAxisNames: !1,
							axisNames: [],
							ticks: { color: "#6A6A6A" }
						}
					}
				} }
			},
			userConfig: N.value.skeletonConfig ?? {}
		})), { loading: Zt, FINAL_DATASET: Qt, manualLoading: $t } = ye({
			...Je(D),
			FINAL_CONFIG: N,
			prepareConfig: rn,
			callback: () => {
				Promise.resolve().then(async () => {
					await Ue(), H.value.showTable = N.value.table.show;
				});
			},
			skeletonDataset: D.config?.skeletonDataset ?? [
				{
					name: "",
					shape: "circle",
					color: "transparent",
					series: [{
						name: "",
						values: [
							1,
							10,
							100,
							1e3
						]
					}]
				},
				{
					name: "",
					shape: "circle",
					color: "#CACACA",
					series: [{
						name: "",
						values: [
							.2,
							3,
							50,
							800
						]
					}]
				},
				{
					name: "",
					shape: "circle",
					color: "transparent",
					series: [{
						name: "",
						values: [
							0,
							0,
							0,
							0
						]
					}]
				}
			],
			skeletonConfig: a({
				defaultConfig: N.value,
				userConfig: Xt.value
			})
		}), { userOptionsVisible: en, setUserOptionsVisibility: tn, keepUserOptionState: nn } = Pe({ config: N.value }), { svgRef: I } = Fe({ config: N.value.style.chart.title });
		function rn() {
			let e = xe({
				userConfig: D.config,
				defaultConfig: jt
			}), t = e.theme;
			if (!t) return e;
			if (!Mt.value(e)) return Nt(e), e;
			let n = xe({
				userConfig: Le[t] || D.config,
				defaultConfig: e
			}), r = xe({
				userConfig: D.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : o[t] || c
			};
		}
		Ye(() => D.config, (e) => {
			Zt.value || (N.value = rn()), en.value = !N.value.userOptions.showOnChartHover, on(), Bt.value += 1, Ht.value += 1, Vt.value += 1, H.value.dataLabels.show = N.value.style.chart.yAxis.labels.datapoints.show, H.value.showTable = N.value.table.show, H.value.showTooltip = N.value.style.chart.tooltip.show;
		}, { deep: !0 }), Ye(() => D.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && ($t.value = !1);
		}, { deep: !0 });
		let L = qe(null), R = qe(null);
		Ge(() => {
			Ut.value = !0, on();
		});
		let an = u(() => N.value.debug);
		function on() {
			if (se(D.dataset) ? (fe({
				componentName: "VueUiParallelCoordinatePlot",
				type: "dataset",
				debug: an.value
			}), $t.value = !0) : D.dataset.forEach((e, t) => {
				ce({
					datasetObject: e,
					requiredAttributes: ["name", "series"]
				}).forEach((e) => {
					Pt.value = !1, fe({
						componentName: "VueUiParallelCoordinatePlot",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: an.value
					});
				});
			}), se(D.dataset) || ($t.value = N.value.loading), N.value.responsive) {
				let e = Ae(() => {
					let { width: e, height: t } = je({
						chart: O.value,
						title: N.value.style.chart.title.text ? It.value : null,
						legend: N.value.style.chart.legend.show ? Lt.value : null,
						source: Rt.value,
						noTitle: zt.value
					});
					requestAnimationFrame(() => {
						z.value.width = e, z.value.height = t - 12, N.value.responsiveProportionalSizing ? (z.value.plotSize = ue({
							relator: Math.min(e, t),
							adjuster: 600,
							source: N.value.style.chart.plots.radius,
							threshold: 2,
							fallback: 2
						}), z.value.ticksFontSize = ue({
							relator: Math.min(e, t),
							adjuster: 600,
							source: N.value.style.chart.yAxis.labels.ticks.fontSize,
							threshold: 10,
							fallback: 10
						}), z.value.datapointFontSize = ue({
							relator: Math.min(e, t),
							adjuster: 600,
							source: N.value.style.chart.yAxis.labels.datapoints.fontSize,
							threshold: 10,
							fallback: 10
						}), z.value.axisNameFontSize = ue({
							relator: Math.min(e, t),
							adjuster: 600,
							source: N.value.style.chart.yAxis.labels.axisNamesFontSize,
							threshold: 12,
							fallback: 12
						})) : (z.value.plotSize = N.value.style.chart.plots.radius, z.value.ticksFontSize = N.value.style.chart.yAxis.labels.ticks.fontSize, z.value.datapointFontSize = N.value.style.chart.yAxis.labels.datapoints.fontSize, z.value.axisNameFontSize = N.value.style.chart.yAxis.labels.axisNamesFontSize);
					});
				});
				L.value && (R.value && L.value.unobserve(R.value), L.value.disconnect()), L.value = new ResizeObserver(e), R.value = O.value.parentNode, L.value.observe(R.value);
			}
		}
		We(() => {
			L.value && (R.value && L.value.unobserve(R.value), L.value.disconnect());
		});
		let { isPrinting: sn, isImaging: cn, generatePdf: ln, generateImage: un } = ve({
			elementId: `pcp_${j.value}`,
			fileName: N.value.style.chart.title.text || "vue-ui-parallel-coordinate-plot",
			options: N.value.userOptions.print
		}), dn = u(() => N.value.userOptions.show && !N.value.style.chart.title.text), z = x({
			height: N.value.style.chart.height,
			width: N.value.style.chart.width,
			plotSize: N.value.style.chart.plots.radius,
			ticksFontSize: N.value.style.chart.yAxis.labels.ticks.fontSize,
			datapointFontSize: N.value.style.chart.yAxis.labels.datapoints.fontSize,
			axisNameFontSize: N.value.style.chart.yAxis.labels.axisNamesFontSize
		}), fn = u(() => z.value.width), pn = u(() => z.value.height), B = x(0), mn = Ae((e) => {
			B.value = e;
		}, 100);
		Xe((e) => {
			let t = Gt.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				mn(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		});
		let V = u(() => {
			let { top: e, right: t, bottom: n, left: r } = N.value.style.chart.padding, i = pn.value, a = fn.value;
			return {
				chartHeight: Math.max(.001, i),
				chartWidth: Math.max(.001, a),
				height: Math.max(.001, i - e - n - B.value - z.value.datapointFontSize * 2),
				width: Math.max(.001, a - r - t),
				top: e + B.value + z.value.datapointFontSize,
				left: r,
				right: a - t,
				bottom: i - n - z.value.datapointFontSize
			};
		}), hn = u(() => pe(N.value.customPalette)), H = x({
			dataLabels: { show: N.value.style.chart.yAxis.labels.datapoints.show },
			showTable: N.value.table.show,
			showTooltip: N.value.style.chart.tooltip.show
		});
		Ye(N, () => {
			H.value = {
				dataLabels: { show: N.value.style.chart.yAxis.labels.datapoints.show },
				showTable: N.value.table.show,
				showTooltip: N.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let U = x([]);
		function gn() {
			U.value.length ? U.value = [] : G.value.forEach((e) => {
				U.value.push(e.id);
			}), Nn("selectLegend", Cn.value);
		}
		function _n(e) {
			U.value.includes(e) ? U.value = U.value.filter((t) => t !== e) : U.value.push(e), Nn("selectLegend", Cn.value);
		}
		let W = u(() => Qt.value.map((e, t) => {
			let n = ee(e.color) || hn.value[t] || c[t] || c[t % c.length];
			return {
				...e,
				series: e.series.map((e) => ({
					...e,
					id: le(),
					color: n
				})),
				seriesIndex: t,
				color: n,
				id: le(),
				shape: e.shape || "circle"
			};
		}));
		function vn(e) {
			return W.value.length ? W.value.find((t) => t.name === e) || (an.value && console.warn(`VueUiParallelCoordinatePlot - Series name not found "${e}"`), null) : (an.value && console.warn("VueUiParallelCoordinatePlot - There are no series to show."), null);
		}
		function yn(e) {
			let t = vn(e);
			t !== null && U.value.includes(t.id) && _n(t.id);
		}
		function bn(e) {
			let t = vn(e);
			t !== null && (U.value.includes(t.id) || _n(t.id));
		}
		let G = u(() => W.value.map((e) => ({
			...e,
			opacity: U.value.includes(e.id) ? .5 : 1,
			segregate: () => _n(e.id),
			isSegregated: U.value.includes(e.id),
			shape: e.shape || "circle"
		}))), xn = u(() => ({
			cy: "pcp-div-legend",
			backgroundColor: N.value.style.chart.legend.backgroundColor,
			color: N.value.style.chart.legend.color,
			fontSize: N.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: N.value.style.chart.legend.bold ? "bold" : ""
		})), Sn = u(() => Math.max(...W.value.filter((e) => !U.value.includes(e.id)).map((e) => Math.max(...e.series.flatMap((e) => e.values.length))))), K = u(() => V.value.width / Sn.value), Cn = u(() => W.value.filter((e) => !U.value.includes(e.id))), q = u(() => {
			let e = [];
			for (let t = 0; t < Sn.value; t += 1) {
				let n = Math.min(...Cn.value.flatMap((e) => e.series.map((e) => e.values[t] || 0) || 0)), r = Math.max(...Cn.value.flatMap((e) => e.series.map((e) => e.values[t] || 0) || 0)), i = r === n ? n / 4 : n, a = r === n ? r * 2 : r, o = re(i, a, N.value.style.chart.yAxis.scaleTicks), s = o.ticks.map((e, n) => {
					let r = o.min < 0 ? e + Math.abs(o.min) : e - Math.abs(o.min), i = o.min < 0 ? o.max + Math.abs(o.min) : o.max - Math.abs(o.min);
					return {
						y: V.value.bottom - V.value.height * (r / i),
						x: V.value.left + K.value * t + K.value / 2,
						value: e
					};
				});
				e.push({
					scale: o,
					ticks: s,
					name: N.value.style.chart.yAxis.labels.axisNames[t] || `Y-${t + 1}`
				});
			}
			return e;
		}), J = u(() => Cn.value.map((e, t) => ({
			...e,
			series: e.series.map((n, r) => ({
				...n,
				datapoints: n.values.map((i, a) => {
					let o = q.value[a].scale.min < 0 ? (i || 0) + Math.abs(q.value[a].scale.min) : (i || 0) - Math.abs(q.value[a].scale.min), s = q.value[a].scale.min < 0 ? q.value[a].scale.max + Math.abs(q.value[a].scale.min) : q.value[a].scale.max - Math.abs(q.value[a].scale.min);
					return {
						name: n.name,
						seriesName: e.name,
						axisIndex: a,
						datapointIndex: r,
						seriesIndex: t,
						value: i || 0,
						x: V.value.left + K.value * a + K.value / 2,
						y: V.value.bottom - V.value.height * (o / s),
						comment: n.comments && n.comments[a] || ""
					};
				})
			}))
		})).map((e) => ({
			...e,
			series: e.series.map((e) => {
				let t = te(e.datapoints), n = s(e.datapoints, .12), r = ae(N.value.style.chart.lines.smooth ? `M ${n}` : `M ${t}`);
				return {
					...e,
					smoothPath: n,
					straightPath: t,
					pathLength: r
				};
			})
		}))), Y = u(() => J.value.flatMap((e, t) => e.series.map((n, r) => ({
			shape: e.shape,
			serieName: e.name,
			serie: n,
			relativeIndex: r,
			seriesIndex: n.seriesIndex,
			S: t,
			key: `${t}_${r}`
		}))));
		function wn({ value: e, index: t, datapoint: n }) {
			return oe(N.value.style.chart.yAxis.labels.formatters[t] || null, e, ne({
				p: N.value.style.chart.yAxis.labels.prefixes[t] || "",
				v: e,
				s: N.value.style.chart.yAxis.labels.suffixes[t] || "",
				r: N.value.style.chart.yAxis.labels.roundings[t] || 0
			}), {
				datapoint: n,
				seriesIndex: t
			});
		}
		let X = x(null), Tn = x(null), En = x(!1), Dn = x("");
		function Z({ shape: e, serie: t, S: n }) {
			N.value.events.datapointLeave && N.value.events.datapointLeave({
				datapoint: {
					...t,
					shape: e
				},
				seriesIndex: n
			});
			let r = A.value === null ? null : Y.value[A.value];
			qt.value === "keyboard" && r && r.serie.id === t.id || (X.value = null, En.value = !1);
		}
		function On(e) {
			if (!I.value || !e?.datapoints?.length) return;
			let t = e.datapoints[Math.floor(e.datapoints.length / 2)];
			if (!t) return;
			let n = i(t.x, t.y, I.value);
			n && (Kt.value = n);
		}
		function kn({ shape: e, serieName: t, serie: n, relativeIndex: r, seriesIndex: i, S: a, triggerMode: o = "pointer" }) {
			N.value.events.datapointEnter && N.value.events.datapointEnter({
				datapoint: {
					...n,
					shape: e
				},
				seriesIndex: a
			}), qt.value = o, Tn.value = {
				config: N.value,
				datapoint: n,
				serie: n,
				relativeIndex: r,
				seriesIndex: i,
				series: W.value,
				scales: q.value
			}, En.value = !0, X.value = n.id;
			let s = "", c = N.value.style.chart.tooltip.customFormat;
			me(c) && ie(() => c({
				serie: n,
				seriesIndex: n.seriesIndex,
				series: W.value,
				config: N.value,
				scales: q.value
			})) ? Dn.value = c({
				serie: n,
				seriesIndex: n.seriesIndex,
				series: W.value,
				config: N.value,
				scales: q.value
			}) : (s += `<div style="width:100%;text-align:center;border-bottom:1px solid ${N.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${t ? t + " - " : ""}${n.name}</div>`, q.value.map((e) => e.name).forEach((e, t) => {
				s += `
                <div class="vue-ui-tooltip-item" style="text-align:left">
                    <span>${e}: </span>
                    <span>
                        ${oe(N.value.style.chart.yAxis.labels.formatters[t] || null, n.datapoints[t].value, ne({
					p: N.value.style.chart.yAxis.labels.prefixes[t] || "",
					v: n.datapoints[t].value,
					s: N.value.style.chart.yAxis.labels.suffixes[t] || "",
					r: N.value.style.chart.yAxis.labels.roundings[t] || ""
				}), {
					datapoint: n.datapoints[t],
					seriesIndex: t
				})}    
                    </span>
                </div>
            `, N.value.style.chart.comments.showInTooltip && n.datapoints[t].comment && (s += `<div class="vue-data-ui-tooltip-comment" style="background:${n.color}20; padding: 6px; margin-bottom: 6px; border-left: 1px solid ${n.color}">${n.datapoints[t].comment}</div>`);
			}), Dn.value = `<div>${s}</div>`), o === "keyboard" && Ue(() => {
				On(n);
			});
		}
		function An() {
			return W.value;
		}
		let Q = u(() => {
			let e = [N.value.table.columnNames.series, N.value.table.columnNames.item].concat(q.value.map((e) => e.name));
			return {
				body: J.value.flatMap((e, t) => e.series.map((t) => [e.name, t.name].concat(t.values))),
				head: e,
				config: {
					th: {
						backgroundColor: N.value.table.th.backgroundColor,
						color: N.value.table.th.color,
						outline: N.value.table.th.outline
					},
					td: {
						backgroundColor: N.value.table.td.backgroundColor,
						color: N.value.table.td.color,
						outline: N.value.table.td.outline
					},
					breakpoint: N.value.table.responsiveBreakpoint
				},
				colNames: [N.value.table.columnNames.series, N.value.table.columnNames.item].concat(q.value.map((e) => e.name))
			};
		}), jn = u(() => J.value.length === 0 ? {
			head: [],
			body: [],
			config: {},
			columnNames: []
		} : {
			head: Q.value.head,
			body: Q.value.body
		});
		function Mn(e = null) {
			let r = [
				[N.value.style.chart.title.text],
				[N.value.style.chart.title.subtitle.text],
				[""]
			], i = jn.value.head, a = jn.value.body, o = r.concat([i]).concat(a), s = n(o);
			e ? e(s) : t({
				csvContent: s,
				title: N.value.style.chart.title.text || "vue-ui-parallel-coordinate-plot"
			});
		}
		let Nn = Ze;
		function Pn({ serie: e, shape: t, S: n }) {
			N.value.events.datapointClick && N.value.events.datapointClick({
				datapoint: {
					...e,
					shape: t
				},
				seriesIndex: n
			}), Nn("selectDatapoint", e);
		}
		function Fn() {
			H.value.showTable = !H.value.showTable;
		}
		function In() {
			H.value.dataLabels.show = !H.value.dataLabels.show;
		}
		function Ln() {
			H.value.showTooltip = !H.value.showTooltip;
		}
		let Rn = x(!1);
		function zn() {
			Rn.value = !Rn.value;
		}
		async function Bn({ scale: e = 2 } = {}) {
			if (!O.value) return;
			let { width: t, height: n } = O.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await Ee({
				domElement: O.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: N.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Vn = x({
			start: 0,
			end: 1
		}), Hn = u(() => q.value.map((e) => e.name));
		Te({
			timeLabelsEls: Gt,
			timeLabels: Hn,
			slicer: Vn,
			configRef: N,
			rotationPath: [
				"style",
				"chart",
				"yAxis",
				"labels",
				"axisNamesRotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"yAxis",
				"labels",
				"axisNamesAutoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: fn,
			height: pn,
			targetClass: ".vue-ui-parallel-coordinate-plot-x-label",
			rotation: N.value.style.chart.yAxis.labels.axisNamesAutoRotate.angle
		});
		let Un = u(() => {
			let e = N.value.table.useDialog && !N.value.table.show, t = H.value.showTable;
			return {
				component: e ? At : Tt,
				title: `${N.value.style.chart.title.text}${N.value.style.chart.title.subtitle.text ? `: ${N.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: N.value.table.th.backgroundColor,
					color: N.value.table.th.color,
					headerColor: N.value.table.th.color,
					headerBg: N.value.table.th.backgroundColor,
					isFullscreen: M.value,
					fullscreenParent: O.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: F.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: N.value.style.chart.backgroundColor,
							color: N.value.style.chart.color
						},
						head: {
							backgroundColor: N.value.style.chart.backgroundColor,
							color: N.value.style.chart.color
						}
					}
				}
			};
		});
		Ye(() => H.value.showTable, (e) => {
			N.value.table.show || (e && N.value.table.useDialog && k.value ? k.value.open() : "close" in k.value && k.value.close());
		});
		function Wn() {
			H.value.showTable = !1, Wt.value && Wt.value.setTableIconState(!1);
		}
		let Gn = u(() => N.value.style.chart.backgroundColor), Kn = u(() => N.value.style.chart.legend), qn = u(() => N.value.style.chart.title), { isCallbackImaging: Jn, isCallbackSvg: Yn, generateSvg: Xn, onGenerateImage: Zn } = Ce({
			svg: I,
			title: qn,
			legend: Kn,
			legendItems: G,
			backgroundColor: Gn,
			getSvgCallback: () => N.value.userOptions.callbacks.svg,
			generateImage: un
		});
		async function Qn() {
			if (Nn("copyAlt", {
				config: N.value,
				dataset: J.value
			}), !N.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(N.value.userOptions.callbacks.altCopy({
				config: N.value,
				dataset: J.value
			}));
		}
		function $n(e) {
			let t = Y.value.length;
			return t ? (e % t + t) % t : null;
		}
		function er() {
			if (A.value !== null) {
				let e = Y.value[A.value];
				e && Z({
					shape: e.shape,
					serie: e.serie,
					S: e.S
				});
			}
			A.value = null, qt.value = "pointer", X.value = null, En.value = !1;
		}
		function tr() {
			A.value = null, Jt.value = !0;
		}
		function nr() {
			er(), Jt.value = !1;
		}
		function rr(e) {
			if (!I.value || Rn.value || document.activeElement !== I.value || !Y.value.length) return;
			let t = ["ArrowUp", "ArrowLeft"].includes(e.key), n = ["ArrowDown", "ArrowRight"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				er();
				return;
			}
			if (r) {
				if (A.value === null) return;
				let e = Y.value[A.value];
				if (!e) return;
				Pn({
					serie: e.serie,
					shape: e.shape,
					S: e.S
				});
				return;
			}
			let a = A.value;
			a = a === null ? n ? 0 : Y.value.length - 1 : $n(a + (n ? 1 : -1));
			let o = Y.value[a];
			o && (A.value = a, kn({
				shape: o.shape,
				serieName: o.serieName,
				serie: o.serie,
				relativeIndex: o.relativeIndex,
				seriesIndex: o.seriesIndex,
				S: o.S,
				triggerMode: "keyboard"
			}));
		}
		let $ = u(() => ({
			head: Q.value.head,
			body: Q.value.body.map((e) => [
				e[0] ?? "",
				e[1] ?? "",
				...e.slice(2)
			]),
			caption: N.value.a11y.translations.tableCaption,
			notice: N.value.a11y.translations.tableAvailable
		}));
		return ke({
			getData: An,
			getImage: Bn,
			generateCsv: Mn,
			generatePdf: ln,
			generateImage: un,
			generateSvg: Xn,
			hideSeries: bn,
			showSeries: yn,
			toggleTable: Fn,
			toggleLabels: In,
			toggleTooltip: Ln,
			toggleAnnotator: zn,
			toggleFullscreen: Yt,
			copyAlt: Qn
		}), (e, t) => (b(), p("div", {
			ref_key: "pcpChart",
			ref: O,
			class: _(`vue-data-ui-component vue-ui-pcp ${M.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${N.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: y(`font-family:${N.value.style.fontFamily};width:100%; text-align:center;background:${N.value.style.chart.backgroundColor};${N.value.responsive ? "height:100%" : ""}`),
			id: `pcp_${j.value}`,
			onMouseenter: t[2] ||= () => T(tn)(!0),
			onMouseleave: t[3] ||= () => T(tn)(!1)
		}, [
			m("div", {
				id: `chart-instructions-${j.value}`,
				class: "sr-only"
			}, [m("p", null, w(N.value.a11y.translations.keyboardNavigation), 1)], 8, $e),
			$.value.body.length ? (b(), d(Ne, {
				key: 0,
				uid: j.value,
				head: $.value.head,
				body: $.value.body,
				caption: $.value.caption,
				notice: $.value.notice
			}, null, 8, [
				"uid",
				"head",
				"body",
				"caption",
				"notice"
			])) : f("", !0),
			N.value.userOptions.buttons.annotator ? (b(), d(T(Dt), {
				key: 1,
				svgRef: T(I),
				backgroundColor: N.value.style.chart.backgroundColor,
				color: N.value.style.chart.color,
				active: Rn.value,
				isCursorPointer: F.value,
				onClose: zn
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
			dn.value ? (b(), p("div", {
				key: 2,
				ref_key: "noTitle",
				ref: zt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : f("", !0),
			N.value.style.chart.title.text ? (b(), p("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: It,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(b(), d(De, {
				key: `title_${Bt.value}`,
				config: {
					title: {
						cy: "pcp-div-title",
						...N.value.style.chart.title
					},
					subtitle: {
						cy: "pcp-div-subtitle",
						...N.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : f("", !0),
			m("div", { id: `legend-top-${j.value}` }, null, 8, et),
			N.value.userOptions.show && Pt.value && (T(nn) || T(en)) ? (b(), d(T(Ot), {
				ref_key: "userOptionsRef",
				ref: Wt,
				key: `user_options_${Ft.value}`,
				backgroundColor: N.value.style.chart.backgroundColor,
				color: N.value.style.chart.color,
				isPrinting: T(sn),
				isImaging: T(cn),
				uid: j.value,
				hasTooltip: N.value.userOptions.buttons.tooltip && N.value.style.chart.tooltip.show,
				hasPdf: N.value.userOptions.buttons.pdf,
				hasXls: N.value.userOptions.buttons.csv,
				hasImg: N.value.userOptions.buttons.img,
				hasSvg: N.value.userOptions.buttons.svg,
				hasTable: N.value.userOptions.buttons.table,
				hasLabel: N.value.userOptions.buttons.labels,
				hasFullscreen: N.value.userOptions.buttons.fullscreen,
				hasAltCopy: N.value.userOptions.buttons.altCopy,
				isFullscreen: M.value,
				isTooltip: H.value.showTooltip,
				titles: { ...N.value.userOptions.buttonTitles },
				chartElement: O.value,
				position: N.value.userOptions.position,
				hasAnnotator: N.value.userOptions.buttons.annotator,
				isAnnotation: Rn.value,
				callbacks: N.value.userOptions.callbacks,
				printScale: N.value.userOptions.print.scale,
				tableDialog: N.value.table.useDialog,
				isCursorPointer: F.value,
				onToggleFullscreen: Yt,
				onGeneratePdf: T(ln),
				onGenerateCsv: Mn,
				onGenerateImage: T(Zn),
				onGenerateSvg: T(Xn),
				onToggleTable: Fn,
				onToggleLabels: In,
				onToggleTooltip: Ln,
				onToggleAnnotator: zn,
				onCopyAlt: Qn,
				style: y({ visibility: T(nn) ? T(en) ? "visible" : "hidden" : "visible" })
			}, ze({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: E(({ isOpen: t, color: n }) => [C(e.$slots, "menuIcon", v(g({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: E(() => [C(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: E(() => [C(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: E(() => [C(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: E(() => [C(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: E(() => [C(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: E(() => [C(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionLabels ? {
					name: "optionLabels",
					fn: E(() => [C(e.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: E(({ toggleFullscreen: t, isFullscreen: n }) => [C(e.$slots, "optionFullscreen", v(g({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: E(({ toggleAnnotator: t, isAnnotator: n }) => [C(e.$slots, "optionAnnotator", v(g({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: E(({ altCopy: t }) => [C(e.$slots, "optionAltCopy", v(g({ altCopy: t })), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: E(() => [C(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: E(() => [C(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasLabel.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : f("", !0),
			m("div", tt, [(b(), p("svg", {
				ref_key: "svgRef",
				ref: I,
				xmlns: T(de),
				"aria-describedby": `chart-instructions-${j.value}`,
				class: _({
					"vue-data-ui-fullscreen--on": M.value,
					"vue-data-ui-fulscreen--off": !M.value,
					"vue-data-ui-no-transition": !T(P)
				}),
				viewBox: `0 0 ${V.value.chartWidth <= 0 ? 10 : V.value.chartWidth} ${V.value.chartHeight <= 0 ? 10 : V.value.chartHeight}`,
				style: y(`max-width:100%; overflow: visible; background:transparent;color:${N.value.style.chart.color}`),
				tabindex: "0",
				onFocus: tr,
				onBlur: nr,
				onKeydown: rr
			}, [
				Ve(T(kt)),
				e.$slots["chart-background"] ? (b(), p("foreignObject", {
					key: 0,
					x: V.value.left,
					y: V.value.top,
					width: V.value.width,
					height: V.value.height,
					style: { pointerEvents: "none" }
				}, [C(e.$slots, "chart-background", {}, void 0, !0)], 8, rt)) : f("", !0),
				(b(!0), p(l, null, S(q.value, (e, t) => (b(), p("g", it, [m("line", {
					x1: V.value.left + K.value * t + K.value / 2,
					x2: V.value.left + K.value * t + K.value / 2,
					y1: V.value.top,
					y2: V.value.bottom,
					stroke: N.value.style.chart.yAxis.stroke,
					"stroke-width": N.value.style.chart.yAxis.strokeWidth
				}, null, 8, at), N.value.style.chart.yAxis.labels.ticks.show ? (b(), p(l, { key: 0 }, [(b(!0), p(l, null, S(e.ticks, (t, n) => (b(), p("path", {
					class: _({ "vue-data-ui-transition": T(P) }),
					key: `tick_${e.name}_${n}`,
					d: `M${t.x},${t.y} ${t.x - 10},${t.y}`,
					stroke: N.value.style.chart.yAxis.stroke,
					"stroke-width": N.value.style.chart.yAxis.strokeWidth,
					style: y(`opacity:${X.value && !H.value.showTooltip ? .2 : 1}`)
				}, null, 14, ot))), 128)), T(Zt) ? f("", !0) : (b(), p("g", st, [(b(!0), p(l, null, S(e.ticks, (n, r) => (b(), p("text", {
					key: `tl_${e.name}_${r}`,
					class: _({ "vue-data-ui-transition": T(P) }),
					transform: `translate(${n.x - 12 + N.value.style.chart.yAxis.labels.ticks.offsetX}, ${n.y + N.value.style.chart.yAxis.labels.ticks.offsetY + z.value.ticksFontSize / 3})`,
					fill: N.value.style.chart.yAxis.labels.ticks.color,
					"text-anchor": "end",
					"font-size": z.value.ticksFontSize,
					"font-weight": N.value.style.chart.yAxis.labels.ticks.bold ? "bold" : "normal",
					style: y(`opacity:${X.value && !H.value.showTooltip ? .2 : 1}`)
				}, w(wn({
					value: n.value,
					index: t,
					datapoint: n
				})), 15, ct))), 128))]))], 64)) : f("", !0)]))), 256)),
				N.value.style.chart.yAxis.labels.showAxisNames ? (b(), p("g", {
					key: 1,
					ref_key: "xAxisLabels",
					ref: Gt
				}, [(b(!0), p(l, null, S(q.value, (e, t) => (b(), p(l, null, [String(e.name).includes("\n") ? (b(), p("text", {
					key: 1,
					class: "vue-ui-parallel-coordinate-plot-x-label",
					fill: N.value.style.chart.yAxis.labels.axisNamesColor,
					"font-size": z.value.axisNameFontSize,
					"font-weight": N.value.style.chart.yAxis.labels.axisNamesBold ? "bold" : "",
					"text-anchor": N.value.style.chart.yAxis.labels.axisNamesRotation === 0 ? "middle" : N.value.style.chart.yAxis.labels.axisNamesRotation < 0 ? "start" : "end",
					transform: `translate(${V.value.left + K.value * t + K.value / 2}, ${B.value - z.value.axisNameFontSize}), rotate(${N.value.style.chart.yAxis.labels.axisNamesRotation})`,
					innerHTML: T(r)({
						content: String(e.name),
						fontSize: z.value.axisNameFontSize,
						fill: N.value.style.chart.yAxis.labels.axisNamesColor,
						x: 0,
						y: 0
					})
				}, null, 8, ut)) : (b(), p("text", {
					key: 0,
					class: "vue-ui-parallel-coordinate-plot-x-label",
					fill: N.value.style.chart.yAxis.labels.axisNamesColor,
					"font-size": z.value.axisNameFontSize,
					"font-weight": N.value.style.chart.yAxis.labels.axisNamesBold ? "bold" : "",
					"text-anchor": N.value.style.chart.yAxis.labels.axisNamesRotation === 0 ? "middle" : N.value.style.chart.yAxis.labels.axisNamesRotation < 0 ? "start" : "end",
					transform: `translate(${V.value.left + K.value * t + K.value / 2}, ${B.value - z.value.axisNameFontSize}), rotate(${N.value.style.chart.yAxis.labels.axisNamesRotation})`
				}, w(e.name), 9, lt))], 64))), 256))], 512)) : f("", !0),
				(b(!0), p(l, null, S(J.value, (t, n) => (b(), p("g", null, [(b(!0), p(l, null, S(t.series, (r, i) => (b(), p("g", null, [
					N.value.style.chart.plots.show ? (b(), p("g", dt, [
						(b(!0), p(l, null, S(r.datapoints, (e, a) => (b(), d(Oe, {
							plot: {
								x: e.x,
								y: e.y
							},
							color: t.color,
							shape: t.shape,
							radius: t.shape === "triangle" ? z.value.plotSize * 1.2 : z.value.plotSize,
							stroke: N.value.style.chart.backgroundColor,
							strokeWidth: .5,
							onMouseenter: (e) => kn({
								shape: t.shape,
								serieName: t.name,
								serie: r,
								relativeIndex: i,
								seriesIndex: r.seriesIndex,
								S: n,
								triggerMode: "pointer"
							}),
							onMouseleave: (e) => Z({
								serie: r,
								shape: t.shape,
								S: n
							}),
							style: y(`opacity:${X.value ? X.value === r.id ? N.value.style.chart.plots.opacity : .2 : N.value.style.chart.plots.opacity}`),
							onClick: () => Pn({
								serie: r,
								shape: t.shape,
								S: n
							})
						}, null, 8, [
							"plot",
							"color",
							"shape",
							"radius",
							"stroke",
							"onMouseenter",
							"onMouseleave",
							"style",
							"onClick"
						]))), 256)),
						H.value.showTooltip ? f("", !0) : (b(), p(l, { key: 0 }, [X.value && X.value === r.id && r.datapoints.length ? (b(), p("text", {
							key: 0,
							x: r.datapoints[0].x - z.value.ticksFontSize,
							y: r.datapoints[0].y + z.value.ticksFontSize / 3,
							"text-anchor": "end",
							"font-size": z.value.ticksFontSize,
							fill: t.color,
							"font-weight": "bold"
						}, w(r.name), 9, ft)) : f("", !0)], 64)),
						N.value.style.chart.comments.show ? (b(!0), p(l, { key: 1 }, S(r.datapoints, (n) => (b(), p("g", null, [n.comment ? (b(), p("foreignObject", {
							key: 0,
							style: { overflow: "visible" },
							height: "12",
							width: N.value.style.chart.comments.width,
							x: n.x - N.value.style.chart.comments.width / 2 + N.value.style.chart.comments.offsetX,
							y: n.y + N.value.style.chart.comments.offsetY + 6
						}, [m("div", mt, [C(e.$slots, "plot-comment", { plot: {
							...n,
							color: t.color
						} }, void 0, !0)])], 8, pt)) : f("", !0)]))), 256)) : f("", !0)
					])) : f("", !0),
					m("path", {
						d: `M${N.value.style.chart.lines.smooth ? r.smoothPath : r.straightPath}`,
						stroke: t.color,
						"stroke-width": N.value.style.chart.lines.strokeWidth,
						fill: "none",
						class: _({
							"vue-ui-pcp-animated vue-data-ui-line-animated": N.value.useCssAnimation,
							"vue-data-ui-transition": T(P)
						}),
						onMouseenter: (e) => kn({
							shape: t.shape,
							serieName: t.name,
							serie: r,
							relativeIndex: i,
							seriesIndex: r.seriesIndex,
							S: n,
							triggerMode: "pointer"
						}),
						onMouseleave: (e) => Z({
							serie: r,
							shape: t.shape,
							S: n
						}),
						onClick: () => Pn({
							serie: r,
							shape: t.shape,
							S: n
						}),
						style: y(`opacity:${X.value ? X.value === r.id ? N.value.style.chart.lines.opacity : .2 : N.value.style.chart.lines.opacity}; stroke-dasharray:${r.pathLength}; stroke-dashoffset: ${N.value.useCssAnimation ? r.pathLength : 0}`)
					}, null, 46, ht),
					H.value.showTooltip ? (b(), p("path", {
						key: 1,
						d: `M${N.value.style.chart.lines.smooth ? r.smoothPath : r.straightPath}`,
						stroke: "transparent",
						"stroke-width": 12,
						fill: "none",
						class: _({
							"vue-ui-pcp-animated vue-data-ui-line-animated": N.value.useCssAnimation,
							"vue-data-ui-transition": T(P)
						}),
						onMouseenter: (e) => kn({
							shape: t.shape,
							serieName: t.name,
							serie: r,
							relativeIndex: i,
							seriesIndex: r.seriesIndex,
							S: n,
							triggerMode: "pointer"
						}),
						onMouseleave: (e) => Z({
							serie: r,
							shape: t.shape,
							S: n
						}),
						onClick: () => Pn({
							serie: r,
							shape: t.shape,
							S: n
						}),
						style: { opacity: "0" }
					}, null, 42, gt)) : f("", !0)
				]))), 256))]))), 256)),
				(b(!0), p(l, null, S(J.value, (e, t) => (b(), p("g", null, [(b(!0), p(l, null, S(e.series, (n, r) => (b(), p("g", null, [!T(Zt) && (H.value.dataLabels.show || X.value && X.value === n.id) ? (b(!0), p(l, { key: 0 }, S(n.datapoints, (i, a) => (b(), p("text", {
					key: `pl_${n.id}_${a}`,
					transform: `translate(${i.x + 12 + N.value.style.chart.yAxis.labels.datapoints.offsetX}, ${i.y + N.value.style.chart.yAxis.labels.datapoints.offsetY + z.value.datapointFontSize / 3})`,
					fill: N.value.style.chart.yAxis.labels.datapoints.useSerieColor ? e.color : N.value.style.chart.yAxis.labels.datapoints.color,
					"text-anchor": "start",
					"font-weight": N.value.style.chart.yAxis.labels.datapoints.bold ? "bold" : "normal",
					class: _({
						"vue-ui-pcp-plot-label": !0,
						"vue-data-ui-transition": T(P)
					}),
					"font-size": z.value.datapointFontSize,
					stroke: N.value.style.chart.backgroundColor,
					"stroke-width": 3,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					"paint-order": "stroke fill",
					onMouseenter: (i) => kn({
						shape: e.shape,
						serieName: e.name,
						serie: n,
						relativeIndex: r,
						seriesIndex: n.seriesIndex,
						S: t,
						triggerMode: "pointer"
					}),
					onMouseleave: (r) => Z({
						serie: n,
						shape: e.shape,
						S: t
					}),
					onClick: () => Pn({
						serie: n,
						shape: e.shape,
						S: t
					}),
					style: y(`opacity:${X.value ? X.value === n.id ? 1 : .2 : 1}`)
				}, w(wn({
					value: i.value,
					index: a,
					datapoint: i
				})), 47, _t))), 128)) : f("", !0)]))), 256))]))), 256)),
				C(e.$slots, "svg", { svg: {
					...V.value,
					isPrintingImg: T(sn) || T(cn) || T(Jn),
					isPrintingSvg: T(Yn)
				} }, void 0, !0)
			], 46, nt)), e.$slots.hint ? (b(), p("div", vt, [C(e.$slots, "hint", v(g({
				hint: N.value.a11y.translations.keyboardNavigation,
				isVisible: Jt.value
			})), void 0, !0)])) : f("", !0)]),
			e.$slots.watermark ? (b(), p("div", yt, [C(e.$slots, "watermark", v(g({ isPrinting: T(sn) || T(cn) || T(Jn) || T(Yn) })), void 0, !0)])) : f("", !0),
			m("div", { id: `legend-bottom-${j.value}` }, null, 8, bt),
			Ut.value && (N.value.style.chart.legend.show || e.$slots.legend) ? (b(), d(Re, {
				key: 6,
				to: N.value.style.chart.legend.position === "top" ? `#legend-top-${j.value}` : `#legend-bottom-${j.value}`
			}, [m("div", {
				ref_key: "chartLegend",
				ref: Lt
			}, [C(e.$slots, "legend", { legend: G.value }, () => [N.value.style.chart.legend.show && Pt.value ? (b(), d(Ie, {
				key: `legend_${Vt.value}`,
				legendSet: G.value,
				config: xn.value,
				isCursorPointer: F.value,
				onClickMarker: t[0] ||= ({ legend: e }) => {
					_n(e.id);
				}
			}, {
				item: E(({ legend: e, index: t }) => [m("div", {
					onClick: (t) => e.segregate(),
					style: y(`opacity:${U.value.includes(e.id) ? .5 : 1}`)
				}, w(e.name), 13, xt)]),
				legendToggle: E(() => [G.value.length > 2 && N.value.style.chart.legend.selectAllToggle.show && !T(Zt) ? (b(), d(Me, {
					key: 0,
					backgroundColor: N.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: N.value.style.chart.legend.selectAllToggle.color,
					fontSize: N.value.style.chart.legend.fontSize,
					checked: U.value.length > 0,
					isCursorPointer: F.value,
					onToggle: gn
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : f("", !0)]),
				_: 1
			}, 8, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : f("", !0)], !0)], 512)], 8, ["to"])) : f("", !0),
			e.$slots.source ? (b(), p("div", {
				key: 7,
				ref_key: "source",
				ref: Rt,
				dir: "auto"
			}, [C(e.$slots, "source", {}, void 0, !0)], 512)) : f("", !0),
			Ve(T(Ct), {
				teleportTo: N.value.style.chart.tooltip.teleportTo,
				show: H.value.showTooltip && En.value,
				backgroundColor: N.value.style.chart.tooltip.backgroundColor,
				color: N.value.style.chart.tooltip.color,
				fontSize: N.value.style.chart.tooltip.fontSize,
				borderRadius: N.value.style.chart.tooltip.borderRadius,
				borderColor: N.value.style.chart.tooltip.borderColor,
				borderWidth: N.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: N.value.style.chart.tooltip.backgroundOpacity,
				position: N.value.style.chart.tooltip.position,
				offsetX: N.value.style.chart.tooltip.offsetX,
				offsetY: N.value.style.chart.tooltip.offsetY,
				parent: O.value,
				content: Dn.value,
				isFullscreen: M.value,
				isCustom: T(me)(N.value.style.chart.tooltip.customFormat),
				smooth: N.value.style.chart.tooltip.smooth,
				backdropFilter: N.value.style.chart.tooltip.backdropFilter,
				smoothForce: N.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: N.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: qt.value === "keyboard",
				a11yPosition: Kt.value
			}, {
				"tooltip-before": E(() => [C(e.$slots, "tooltip-before", v(g({ ...Tn.value })), void 0, !0)]),
				tooltip: E(() => [C(e.$slots, "tooltip", v(g({ ...Tn.value })), void 0, !0)]),
				"tooltip-after": E(() => [C(e.$slots, "tooltip-after", v(g({ ...Tn.value })), void 0, !0)]),
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
			Pt.value && N.value.userOptions.buttons.table ? (b(), d(Ke(Un.value.component), He({ key: 8 }, Un.value.props, {
				ref_key: "tableUnit",
				ref: k,
				onClose: Wn
			}), ze({
				content: E(() => [(b(), d(T(Et), {
					key: `table_${Ht.value}`,
					colNames: Q.value.colNames,
					head: Q.value.head,
					body: Q.value.body,
					config: Q.value.config,
					title: N.value.table.useDialog ? "" : Un.value.title,
					withCloseButton: !N.value.table.useDialog,
					isCursorPointer: F.value,
					onClose: Wn
				}, {
					th: E(({ th: e }) => [m("div", { innerHTML: e }, null, 8, St)]),
					td: E(({ td: e }) => [Be(w(e), 1)]),
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
			}, [N.value.table.useDialog ? {
				name: "title",
				fn: E(() => [Be(w(Un.value.title), 1)]),
				key: "0"
			} : void 0, N.value.table.useDialog ? {
				name: "actions",
				fn: E(() => [m("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Mn(N.value.userOptions.callbacks.csv),
					style: y({ cursor: F.value ? "pointer" : "default" })
				}, [Ve(T(wt), {
					name: "fileCsv",
					stroke: Un.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : f("", !0),
			C(e.$slots, "skeleton", {}, () => [T(Zt) ? (b(), d(be, { key: 0 })) : f("", !0)], !0)
		], 46, Qe));
	}
}, [["__scopeId", "data-v-519f368b"]]);
//#endregion
export { Ze as n, Ct as t };
