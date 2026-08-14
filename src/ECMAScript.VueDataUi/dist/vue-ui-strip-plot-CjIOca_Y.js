import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, G as r, Jt as i, Kt as a, Ot as o, Pt as s, S as c, X as l, Y as u, Zt as ee, _ as te, b as ne, ct as re, i as ie, jt as ae, pt as oe, q as se, qt as ce, t as le, tt as ue, w as de, x as d, xt as fe } from "./lib-Bttd6u5E.js";
import { n as pe, t as me } from "./useHints-Dq_w2E8B.js";
import { t as he } from "./useConfig-DlNpz6P8.js";
import { t as ge } from "./usePrinter-DN5bYhTG.js";
import { n as _e, t as ve } from "./BaseScanner-DZvpgOjM.js";
import { t as ye } from "./useNestedProp-vPNvh7rV.js";
import { t as be } from "./useThemeCheck-C43Tcqmk.js";
import { t as xe } from "./useChartExport-DNiwdPmb.js";
import { t as Se } from "./useTransitions-g_zBREk2.js";
import { t as Ce } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as we } from "./img-Bnokohej.js";
import { n as Te } from "./Title-BE3qg9xl.js";
import { t as Ee } from "./Shape-C21CMlWS.js";
import { t as De } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Oe, t as ke } from "./useResponsive-ZtArZtUf.js";
import { t as Ae } from "./DefGrad-DVBqDjhO.js";
import { t as je } from "./A11yDataTable-DdRsVULz.js";
import { t as Me } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ne } from "./useChartAccessibility-DYqac8yF.js";
import { t as Pe } from "./vue_ui_strip_plot-BaHbnnwN.js";
import { Fragment as f, computed as p, createBlock as m, createCommentVNode as h, createElementBlock as g, createElementVNode as _, createSlots as Fe, createTextVNode as Ie, createVNode as Le, defineAsyncComponent as v, guardReactiveProps as y, mergeProps as Re, nextTick as ze, normalizeClass as b, normalizeProps as x, normalizeStyle as S, onBeforeUnmount as Be, onMounted as Ve, openBlock as C, ref as w, renderList as T, renderSlot as E, resolveDynamicComponent as He, shallowRef as Ue, toDisplayString as D, toRefs as We, unref as O, watch as Ge, watchEffect as Ke, withCtx as k } from "vue";
//#region src/components/vue-ui-strip-plot.vue
var qe = /* @__PURE__ */ e({ default: () => Tt }), Je = ["id"], Ye = ["id"], Xe = { style: { position: "relative" } }, Ze = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], Qe = [
	"x",
	"y",
	"width",
	"height"
], $e = { key: 1 }, et = { key: 0 }, tt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], nt = { key: 1 }, rt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], it = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], at = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], ot = [
	"transform",
	"fill",
	"font-size"
], st = [
	"transform",
	"font-size",
	"fill",
	"text-anchor"
], ct = [
	"transform",
	"font-size",
	"fill",
	"text-anchor",
	"innerHTML"
], lt = [
	"fill",
	"font-size",
	"transform"
], ut = [
	"fill",
	"font-size",
	"x",
	"y"
], dt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], ft = [
	"cx",
	"cy",
	"fill"
], pt = [
	"cx",
	"cy",
	"fill"
], mt = [
	"d",
	"fill",
	"stroke",
	"fill-opacity",
	"stroke-opacity",
	"stroke-width",
	"onMouseenter"
], ht = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-opacity",
	"stroke-width"
], gt = ["onMouseenter"], _t = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], vt = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width",
	"rx"
], yt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke"
], bt = { key: 1 }, xt = [
	"x",
	"y",
	"font-size",
	"fill"
], St = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, Ct = {
	key: 5,
	class: "vue-data-ui-watermark"
}, wt = ["innerHTML"], Tt = /*#__PURE__*/ De({
	__name: "vue-ui-strip-plot",
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
	emits: ["selectDatapoint", "copyAlt"],
	setup(e, { expose: De, emit: qe }) {
		let Tt = v(() => import("./Tooltip-DhjyfHwz.js")), Et = v(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Dt = v(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Ot = v(() => import("./DataTable-BbKgJ5UI.js")), kt = v(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), At = v(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), jt = v(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Mt = v(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), Nt = [
			"classic",
			"scatter",
			"violin"
		], { vue_ui_strip_plot: Pt } = he(), { isThemeValid: Ft, warnInvalidTheme: It } = be(), A = e, Lt = qe, Rt = p({
			get() {
				return !!A.dataset && A.dataset.length;
			},
			set(e) {
				return e;
			}
		}), j = w(se()), zt = w(0), M = w(!1), Bt = w(""), N = w(null), Vt = w(null), Ht = w(null), Ut = w(null), Wt = w(!1), Gt = w(0), Kt = w(0), qt = w(null), Jt = w(null), Yt = w(null), Xt = w(null), Zt = w(null), Qt = w(null), $t = w(null), en = w(null), P = w("pointer"), tn = w({
			x: 0,
			y: 0
		}), nn = w(!1), F = w(pn());
		pe({
			config: () => F.value,
			dataset: () => A.dataset,
			component: "VueUiStripPlot",
			rules: [me.emptyArray, {
				test: (e) => e.length > 12,
				message: [
					"👀 The number of series is > 12. Consider:",
					"",
					"▶️ Using filters to let users choose a maximum number of series to display.",
					"",
					"▶️ Using multiple instances of the chart to display related series."
				]
			}]
		});
		let { transitionEnabled: rn } = Se({
			config: () => F.value.transitions,
			dataset: () => A.dataset
		}), an = p(() => mn(F.value.type)), I = p(() => F.value.userOptions.useCursorPointer), on = p(() => i({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					padding: {
						top: 24,
						left: 24,
						right: 24,
						bottom: 24
					},
					grid: {
						stroke: "#6A6A6A",
						horizontalGrid: { stroke: "#6A6A6A" },
						verticalGrid: { stroke: "#6A6A6A" }
					},
					plots: { stroke: "#6A6A6A" },
					labels: {
						bestPlotLabel: { show: !1 },
						axis: {
							xLabel: "",
							yLabel: ""
						},
						xAxisLabels: { show: !1 },
						yAxisLabels: { show: !1 }
					}
				} }
			},
			userConfig: F.value.skeletonConfig ?? {}
		})), { loading: sn, FINAL_DATASET: cn, manualLoading: ln } = _e({
			...We(A),
			FINAL_CONFIG: F,
			prepareConfig: pn,
			skeletonDataset: A.config?.skeletonDataset ?? [
				{
					name: "_",
					color: "#DBDBDB",
					plots: [
						{
							name: "_",
							value: 1
						},
						{
							name: "_",
							value: 2
						},
						{
							name: "_",
							value: 3
						}
					]
				},
				{
					name: "_",
					color: "#C4C4C4",
					plots: [
						{
							name: "_",
							value: 3
						},
						{
							name: "_",
							value: 5
						},
						{
							name: "_",
							value: 8
						}
					]
				},
				{
					name: "_",
					color: "#ADADAD",
					plots: [
						{
							name: "_",
							value: 8
						},
						{
							name: "_",
							value: 13
						},
						{
							name: "_",
							value: 21
						}
					]
				},
				{
					name: "_",
					color: "#969696",
					plots: [
						{
							name: "_",
							value: 21
						},
						{
							name: "_",
							value: 34
						},
						{
							name: "_",
							value: 55
						}
					]
				},
				{
					name: "_",
					color: "#808080",
					plots: [
						{
							name: "_",
							value: 55
						},
						{
							name: "_",
							value: 89
						},
						{
							name: "_",
							value: 144
						}
					]
				}
			],
			skeletonConfig: i({
				defaultConfig: F.value,
				userConfig: on.value
			})
		}), { userOptionsVisible: un, setUserOptionsVisibility: dn, keepUserOptionState: fn } = Me({ config: F.value }), { svgRef: L } = Ne({ config: F.value.style.chart.title });
		function pn() {
			let e = ye({
				userConfig: A.config,
				defaultConfig: Pt
			}), t = e.theme;
			if (!t) return e;
			if (!Ft.value(e)) return It(e), e;
			let n = ye({
				userConfig: Pe[t] || A.config,
				defaultConfig: e
			}), r = ye({
				userConfig: A.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : a[t] || s
			};
		}
		function mn(e) {
			return Nt.includes(e) ? e : "classic";
		}
		Ge(() => A.config, async (e) => {
			sn.value || (F.value = pn()), un.value = !F.value.userOptions.showOnChartHover, gn(), Gt.value += 1, Kt.value += 1, H.value.dataLabels.show = F.value.style.chart.labels.bestPlotLabel.show, H.value.showTable = F.value.table.show, H.value.showTooltip = F.value.style.chart.tooltip.show, B.value = F.value.style.chart.width, V.value = F.value.style.chart.height, En.value = F.value.style.chart.plots.radius;
		}, { deep: !0 }), Ge(() => A.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (ln.value = !1);
		}, { deep: !0 });
		let R = Ue(null), z = Ue(null);
		Ve(() => {
			gn();
		});
		let hn = p(() => F.value.debug);
		function gn() {
			if (ae(A.dataset) ? (ue({
				componentName: "VueUiStripPlot",
				type: "dataset",
				debug: hn.value
			}), ln.value = !0) : A.dataset.forEach((e, t) => {
				oe({
					datasetObject: e,
					requiredAttributes: ["name", "plots"]
				}).forEach((e) => {
					Rt.value = !1, ue({
						componentName: "VueUiStripPlot",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: hn.value
					});
				}), e.plots && e.plots.forEach((e, n) => {
					oe({
						datasetObject: e,
						requiredAttributes: ["name", "value"]
					}).forEach((e) => {
						Rt.value = !1, ue({
							componentName: "VueUiStripPlot",
							type: "datasetSerieAttribute",
							property: e,
							index: `${t},${n}`,
							debug: hn.value
						});
					});
				});
			}), ae(A.dataset) || (ln.value = F.value.loading), F.value.responsive) {
				let e = Oe(() => {
					let { width: e, height: t } = ke({
						chart: N.value,
						title: F.value.style.chart.title.text ? Vt.value : null,
						source: Ht.value,
						noTitle: Ut.value
					});
					requestAnimationFrame(() => {
						Tn.value = t, B.value = Math.max(.1, e), V.value = Math.max(.1, t - 12), F.value.responsiveProportionalSizing ? En.value = ce({
							relator: Math.min(t, e),
							adjuster: 600,
							source: F.value.style.chart.plots.radius,
							threshold: 6,
							fallback: 6
						}) : En.value = F.value.style.chart.plots.radius;
					});
				});
				R.value && (z.value && R.value.unobserve(z.value), R.value.disconnect()), R.value = new ResizeObserver(e), z.value = N.value.parentNode, R.value.observe(z.value);
			}
			Wt.value = !0, setTimeout(() => {
				Cn.value = !1;
			}, Nn.value * 50);
		}
		Be(() => {
			R.value && (z.value && R.value.unobserve(z.value), R.value.disconnect());
		});
		let { isPrinting: _n, isImaging: vn, generatePdf: yn, generateImage: bn } = ge({
			elementId: `strip-plot_${j.value}`,
			fileName: F.value.style.chart.title.text || "vue-ui-strip-plot",
			options: F.value.userOptions.print
		}), xn = p(() => F.value.userOptions.show && !F.value.style.chart.title.text), Sn = p(() => de(F.value.customPalette)), Cn = w(F.value.useCssAnimation), wn = w({
			top: F.value.style.chart.padding.top,
			bottom: F.value.style.chart.padding.bottom,
			left: F.value.style.chart.padding.left,
			right: F.value.style.chart.padding.right
		}), B = w(F.value.style.chart.width), V = w(F.value.style.chart.height), Tn = w(F.value.style.chart.height), En = w(F.value.style.chart.plots.radius), H = w({
			showTable: F.value.table.show,
			dataLabels: { show: F.value.style.chart.labels.bestPlotLabel.show },
			showTooltip: F.value.style.chart.tooltip.show
		});
		Ge(F, () => {
			H.value = {
				showTable: F.value.table.show,
				dataLabels: { show: F.value.style.chart.labels.bestPlotLabel.show },
				showTooltip: F.value.style.chart.tooltip.show
			}, B.value = F.value.style.chart.width, V.value = F.value.style.chart.height, En.value = F.value.style.chart.plots.radius;
		}, { immediate: !0 });
		let U = p(() => Math.min(En.value, G.value.stripWidth / 2 * .9));
		function W(e) {
			return Number.isFinite(e) ? Number(e.toFixed(2)) : 0;
		}
		function Dn() {
			return Math.max(0, (G.value.stripWidth / 2 - U.value * 1.8) * .86) * Math.min(1, F.value.style.chart.violin.widthRatio);
		}
		function On() {
			let e = 0;
			Qt.value && (e = Array.from(Qt.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = Xt.value ? Xt.value.getBoundingClientRect().width : 0;
			return e + t + (t ? 24 : 0);
		}
		let kn = w(0), An = Oe((e) => {
			kn.value = e;
		}, 100);
		Ke((e) => {
			let t = Zt.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				An(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), Be(() => {
			kn.value = 0;
		});
		let jn = p(() => {
			let e = 0;
			Yt.value && (e = Yt.value.getBBox().height + F.value.style.chart.labels.axis.fontSize / 3 + 12 + F.value.style.chart.labels.axis.xLabelOffsetY);
			let t = 0;
			return Zt.value && (t = kn.value + 12), e + t;
		}), G = p(() => {
			let e = On(), t = wn.value.left + e + F.value.style.chart.labels.axis.yLabelOffsetX + 5, n = B.value - wn.value.right, r = Math.max(0, n - t), i = wn.value.top + F.value.style.chart.plots.radius + F.value.style.chart.labels.bestPlotLabel.fontSize, a = V.value - wn.value.bottom - jn.value, o = Math.max(0, a - i), s = Array.isArray(cn.value) ? cn.value.length : 0;
			return {
				left: t,
				right: n,
				top: i,
				bottom: a,
				width: r,
				height: o,
				stripWidth: s > 0 ? r / s : 0,
				absoluteHeight: V.value
			};
		});
		function Mn(e) {
			return G.value.left + (e + 1) * G.value.stripWidth - G.value.stripWidth / 2;
		}
		let K = p(() => cn.value.map((e, t) => {
			let n = se();
			return {
				...e,
				id: n,
				color: e.color ? c(e.color) : Sn.value[t] || s[t] || s[t % s.length],
				plots: e.plots.map((r, i) => ({
					...r,
					value: ne(r.value),
					parentId: n,
					parentName: e.name,
					parentIndex: t,
					plotIndex: i,
					color: e.color ? c(e.color) : Sn.value[t] || s[t] || s[t % s.length],
					id: se()
				})).sort((e, t) => e.value - t.value)
			};
		})), q = p(() => (K.value || []).map((e, t) => ({
			...e,
			plots: e.plots.map((e) => ({
				...e,
				x: Mn(t)
			}))
		}))), Nn = p(() => Math.max(...q.value.map((e) => e.plots.length))), Pn = p(() => {
			let e = q.value.flatMap((e) => e.plots.map((e) => e.value));
			return {
				max: Math.max(...e),
				min: Math.min(...e)
			};
		}), J = p(() => te(Pn.value.min < 0 ? Pn.value.min : 0, Pn.value.max, F.value.style.chart.grid.scaleSteps)), Y = p(() => (q.value || []).map((e, t) => {
			let n = e.plots.map((e) => ({
				...e,
				y: G.value.bottom - (e.value + Math.abs(J.value.min)) / (J.value.max + Math.abs(J.value.min)) * G.value.height
			})), r = an.value === "classic" ? n.map(() => 0) : Bn(n);
			return {
				...e,
				plots: n.map((e, t) => ({
					...e,
					x: e.x + r[t]
				}))
			};
		})), X = p(() => an.value === "violin" && !!F.value.style.chart.violin?.boxPlot?.show);
		function Fn(e, t) {
			if (!e.length) return 0;
			if (e.length === 1) return e[0];
			let n = (e.length - 1) * t, r = Math.floor(n), i = n - r, a = e[r + 1];
			return a === void 0 ? e[r] : e[r] + i * (a - e[r]);
		}
		function In(e) {
			let t = J.value.max + Math.abs(J.value.min);
			return t ? G.value.bottom - (e + Math.abs(J.value.min)) / t * G.value.height : G.value.bottom;
		}
		let Ln = p(() => X.value ? Y.value.map((e, t) => {
			let n = e.plots.map((e) => e.value).sort((e, t) => e - t), r = Fn(n, .25), i = Fn(n, .5), a = Fn(n, .75), o = a - r, s = r - o * 1.5, c = a + o * 1.5, l = n.find((e) => e >= s), u = n.slice().reverse().find((e) => e <= c), ee = Mn(t), te = Math.max(U.value * 2.8, Math.min(Dn() * .32, G.value.stripWidth * .12)) * Math.min(1.5, F.value.style.chart.violin.boxPlot.widthRatio);
			return {
				id: e.id,
				boxPlotColor: F.value.style.chart.violin.boxPlot.useSerieColor ? e.color : F.value.style.chart.violin.boxPlot.color,
				color: e.color,
				name: e.name,
				count: n.length,
				lowerAdjacent: l ?? n[0],
				q1: r,
				median: i,
				q3: a,
				upperAdjacent: u ?? n[n.length - 1],
				iqr: o,
				lowerFence: s,
				upperFence: c,
				centerX: ee,
				boxLeft: ee - te / 2,
				boxWidth: te,
				q1Y: In(r),
				medianY: In(i),
				q3Y: In(a),
				lowerY: In(l ?? n[0]),
				upperY: In(u ?? n[n.length - 1])
			};
		}) : []), Rn = p(() => [
			F.value.table.columnNames.series,
			F.value.style.chart.violin.tooltipLabels.lowerAdjacent,
			F.value.style.chart.violin.tooltipLabels.q1,
			F.value.style.chart.violin.tooltipLabels.median,
			F.value.style.chart.violin.tooltipLabels.q3,
			F.value.style.chart.violin.tooltipLabels.upperAdjacent,
			F.value.style.chart.violin.tooltipLabels.iqr,
			F.value.style.chart.violin.tooltipLabels.count
		]);
		function zn(e) {
			return l({
				p: F.value.style.chart.labels.prefix,
				v: e,
				s: F.value.style.chart.labels.suffix,
				r: F.value.table.td.roundingValue
			});
		}
		function Bn(e) {
			let t = an.value === "violin" ? Math.max(0, Dn() - U.value * 1.15) : Math.max(0, G.value.stripWidth / 2 - U.value * 1.5);
			if (!e.length || !t) return e.map(() => 0);
			let n = e.map(() => 0), r = U.value * 2.1, i = Math.min(t, r), a = [0];
			for (let e = i; e <= t + i / 2; e += i) a.push(-Math.min(e, t)), a.push(Math.min(e, t));
			let o = [];
			return e.forEach((e, t) => {
				let i = a.find((t) => o.every((n) => {
					let i = t - n.offset, a = e.y - n.y;
					return Math.hypot(i, a) >= r;
				})) ?? 0;
				n[t] = i, o.push({
					y: e.y,
					offset: i
				});
			}), n;
		}
		let Vn = p(() => an.value === "violin" ? Y.value.map((e, t) => ({
			id: e.id,
			color: F.value.style.chart.violin.useSerieColor ? e.color : F.value.style.chart.violin.stroke,
			fill: F.value.style.chart.violin.useSerieColor ? e.color : F.value.style.chart.violin.fill,
			path: Hn(e.plots, t),
			connectors: Un(e.plots, t)
		})) : []);
		function Hn(e, t) {
			let n = Dn();
			if (!e.length || !G.value.height || !n) return "";
			let r = Mn(t), i = e.map((e) => e.y), a = Z(i), o = Gn({
				yValues: i,
				bandwidth: a
			});
			return o.length > 1 ? o.map((e) => qn({
				yValues: e,
				centerX: r,
				maxWidth: Wn({
					clusterLength: e.length,
					totalLength: i.length,
					maxWidth: n
				}),
				bandwidth: Z(e)
			})).join(" ") : qn({
				yValues: i,
				centerX: r,
				maxWidth: n,
				bandwidth: a
			});
		}
		function Un(e, t) {
			if (!e.length) return [];
			let n = Mn(t), r = e.map((e) => e.y), i = Gn({
				yValues: r,
				bandwidth: Z(r)
			});
			return i.length < 2 ? [] : i.slice(1).map((e, t) => {
				let r = i[t], a = Z(r), o = Z(e), s = d(Math.max(...r) + a * 2.35, G.value.top, G.value.bottom), c = d(Math.min(...e) - o * 2.35, G.value.top, G.value.bottom);
				return {
					x: W(n),
					y1: W(s),
					y2: W(c)
				};
			});
		}
		function Wn({ clusterLength: e, totalLength: t, maxWidth: n }) {
			let r = (e / t) ** .6, i = e <= 2 ? .72 : 1;
			return Math.max(U.value * 2.2, n * d(r * i, .18, 1));
		}
		function Z(e) {
			let t = e.reduce((e, t) => e + t, 0) / Math.max(1, e.length), n = e.reduce((e, n) => e + (n - t) ** 2, 0) / Math.max(1, e.length), r = Math.sqrt(n), i = e.length < 2 ? Math.max(U.value * 2.6, G.value.height / 48) : U.value * 2.1;
			return d(1.06 * r * e.length ** -.2, i, G.value.height / 5);
		}
		function Gn({ yValues: e, bandwidth: t }) {
			if (e.length < 2) return [e];
			let n = [...e].sort((e, t) => e - t), r = Math.max(t * 1.45, U.value * 5), i = [], a = [n[0]];
			for (let e = 1; e < n.length; e += 1) Math.abs(n[e] - n[e - 1]) > r ? (i.push(a), a = [n[e]]) : a.push(n[e]);
			return i.push(a), Kn(i);
		}
		function Kn(e) {
			return e.reduce((e, t) => {
				let n = e[e.length - 1];
				if (!n) return e.push(t), e;
				let r = Z(n), i = Z(t);
				return Math.max(...n) + r * 2.35 >= Math.min(...t) - i * 2.35 ? n.push(...t) : e.push(t), e;
			}, []);
		}
		function qn({ yValues: e, centerX: t, maxWidth: n, bandwidth: r }) {
			let i = d(Math.min(...e) - r * 2.35, G.value.top, G.value.bottom), a = d(Math.max(...e) + r * 2.35, G.value.top, G.value.bottom), o = Math.max(48, Math.min(120, Math.round((a - i) / Math.max(1, r / 4)))), s = Array.from({ length: o }, (t, n) => {
				let s = i + (a - i) * n / Math.max(1, o - 1);
				return {
					y: s,
					density: e.reduce((e, t) => {
						let n = (s - t) / r;
						return e + Math.exp(-.5 * n * n);
					}, 0)
				};
			}), c = Math.max(...s.map((e) => e.density));
			if (!c) return "";
			let l = s.map((e, t) => {
				let r = t === 0 || t === s.length - 1 ? 0 : e.density / c * n;
				return {
					y: W(e.y),
					width: W(r)
				};
			}), u = l.map((e) => ({
				x: W(t - e.width),
				y: e.y
			})), ee = l.slice(0, -1).reverse().map((e) => ({
				x: W(t + e.width),
				y: e.y
			}));
			return `${Jn([...u, ...ee])} Z`;
		}
		function Jn(e) {
			if (!e.length) return "";
			if (e.length === 1) return `M ${e[0].x} ${e[0].y}`;
			let t = `M ${e[0].x} ${e[0].y}`;
			for (let n = 1; n < e.length - 1; n += 1) {
				let r = e[n], i = e[n + 1], a = W((r.x + i.x) / 2), o = W((r.y + i.y) / 2);
				t += ` Q ${r.x} ${r.y} ${a} ${o}`;
			}
			let n = e[e.length - 1];
			return `${t} L ${n.x} ${n.y}`;
		}
		let Yn = p(() => J.value.ticks.map((e) => ({
			y: G.value.bottom - G.value.height * ((e + Math.abs(J.value.min)) / (J.value.max + Math.abs(J.value.min))),
			x1: G.value.left,
			x2: G.value.right,
			value: e
		}))), Xn = w(null), Q = w(null);
		function Zn({ datapoint: e, seriesIndex: t }) {
			P.value !== "keyboard" && (M.value = !1, Q.value = null, F.value.events.datapointLeave && F.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}));
		}
		function Qn({ datapoint: e, seriesIndex: t }) {
			Lt("selectDatapoint", e), F.value.events.datapointClick && F.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function $n(e, t = {}) {
			return ie(F.value.style.chart.labels.formatter, e, l({
				p: F.value.style.chart.labels.prefix,
				v: e,
				s: F.value.style.chart.labels.suffix,
				r: F.value.style.chart.tooltip.roundingValue
			}), t);
		}
		function er({ datapoint: e, seriesIndex: t, triggerMode: n = "pointer" }) {
			F.value.events.datapointEnter && F.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), P.value = n, Xn.value = {
				datapoint: e,
				seriesIndex: t,
				config: F.value,
				series: K.value
			}, M.value = !0, Q.value = e;
			let r = F.value.style.chart.tooltip.customFormat;
			if (fe(r) && re(() => r({
				seriesIndex: t,
				datapoint: e,
				series: K.value,
				config: F.value
			}))) Bt.value = r({
				seriesIndex: t,
				datapoint: e,
				series: K.value,
				config: F.value
			});
			else {
				let n = "";
				n += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="none" fill="${F.value.style.chart.plots.gradient.show ? `url(#${e.parentId})` : e.color}"/></svg>${e.name}</div>`, n += `<div>${$n(e.value, {
					datapoint: e,
					seriesIndex: t
				})}</div>`, Bt.value = `<div>${n}</div>`;
			}
		}
		function tr() {
			P.value !== "keyboard" && (M.value = !1, Q.value = null);
		}
		function nr({ boxPlot: e, seriesIndex: t, triggerMode: n = "pointer" }) {
			P.value = n, Q.value = null, M.value = !0, Xn.value = {
				boxPlot: e,
				seriesIndex: t,
				config: F.value,
				series: K.value
			};
			let r = F.value.style.chart.tooltip.customFormat;
			if (fe(r)) try {
				let n = {
					boxPlot: e,
					seriesIndex: t,
					series: K.value,
					config: F.value
				};
				if (re(() => r(n))) {
					Bt.value = r(n);
					return;
				}
			} catch {}
			let i = [
				[F.value.style.chart.violin.tooltipLabels.upperAdjacent, e.upperAdjacent],
				[F.value.style.chart.violin.tooltipLabels.q3, e.q3],
				[F.value.style.chart.violin.tooltipLabels.median, e.median],
				[F.value.style.chart.violin.tooltipLabels.q1, e.q1],
				[F.value.style.chart.violin.tooltipLabels.lowerAdjacent, e.lowerAdjacent],
				[F.value.style.chart.violin.tooltipLabels.iqr, e.iqr]
			], a = `<svg viewBox="0 0 12 12" height="14" width="14"><rect x="1" y="1" width="10" height="10" rx="2" stroke="none" fill="${e.color}"/></svg>`, o = i.map(([n, r]) => `<div style="display:flex;flex-direction:row;gap:12px;align-items:center;justify-content:space-between;"><span>${n}</span><b>${$n(r, {
				boxPlot: e,
				seriesIndex: t
			})}</b></div>`).join("");
			Bt.value = `<div style="min-width:160px;"><div style="display:flex;flex-direction:row;gap:6px;align-items:center;margin-bottom:6px;">${a}${e.name}</div>` + o + `<div style="display:flex;flex-direction:row;gap:12px;align-items:center;justify-content:space-between;margin-top:6px;"><span>${F.value.style.chart.violin.tooltipLabels.count}</span><b>${e.count}</b></div></div>`;
		}
		let rr = p(() => ({
			head: q.value.flatMap((e) => JSON.parse(JSON.stringify(e.plots)).sort((e, t) => t.value - e.value).map((t) => ({
				name: `${e.name} - ${t.name}`,
				color: t.color
			}))),
			body: q.value.flatMap((e) => JSON.parse(JSON.stringify(e.plots)).sort((e, t) => t.value - e.value).map((e) => e.value))
		}));
		function ir(e = null) {
			ze(() => {
				let r = [], i = [F.value.table.columnNames.series, F.value.table.columnNames.value];
				X.value ? (i = Rn.value, r = Ln.value.map((e) => [
					[e.name],
					[e.lowerAdjacent],
					[e.q1],
					[e.median],
					[e.q3],
					[e.upperAdjacent],
					[e.iqr],
					[e.count]
				])) : r = rr.value.head.map((e, t) => [[e.name], [rr.value.body[t]]]);
				let a = [
					[F.value.style.chart.title.text],
					[F.value.style.chart.title.subtitle.text],
					i.map((e) => [e])
				].concat(r), o = n(a);
				e ? e(o) : t({
					csvContent: o,
					title: F.value.style.chart.title.text || "vue-ui-strip-plot"
				});
			});
		}
		let ar = p(() => {
			if (X.value) {
				let e = Rn.value;
				return {
					colNames: e,
					head: e,
					body: Ln.value.map((e) => [
						{
							color: e.color,
							name: e.name
						},
						zn(e.lowerAdjacent),
						zn(e.q1),
						zn(e.median),
						zn(e.q3),
						zn(e.upperAdjacent),
						zn(e.iqr),
						e.count
					]),
					config: {
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
					}
				};
			}
			let e = [F.value.table.columnNames.series, F.value.table.columnNames.value], t = rr.value.head.map((e, t) => {
				let n = l({
					p: F.value.style.chart.labels.prefix,
					v: rr.value.body[t],
					s: F.value.style.chart.labels.suffix,
					r: F.value.table.td.roundingValue
				});
				return [{
					color: e.color,
					name: e.name
				}, n];
			}), n = {
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
			};
			return {
				colNames: [F.value.table.columnNames.series, F.value.table.columnNames.value],
				head: e,
				body: t,
				config: n
			};
		}), $ = w(!1);
		function or(e) {
			$.value = e, zt.value += 1;
		}
		function sr() {
			return q.value;
		}
		function cr() {
			H.value.showTable = !H.value.showTable;
		}
		function lr() {
			H.value.dataLabels.show = !H.value.dataLabels.show;
		}
		function ur() {
			H.value.showTooltip = !H.value.showTooltip;
		}
		let dr = w(!1);
		function fr() {
			dr.value = !dr.value;
		}
		async function pr({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { width: t, height: n } = N.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await we({
				domElement: N.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: F.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let mr = p(() => q.value.map((e) => e.name)), hr = w({
			start: 0,
			end: q.value.length
		});
		Ce({
			timeLabelsEls: Zt,
			timeLabels: mr,
			slicer: hr,
			configRef: F,
			rotationPath: [
				"style",
				"chart",
				"labels",
				"xAxisLabels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"labels",
				"xAxisLabels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: B,
			height: V,
			targetClass: ".vue-ui-strip-plot-category-name",
			rotation: F.value.style.chart.labels.xAxisLabels.autoRotate.angle
		});
		let gr = p(() => {
			let e = F.value.table.useDialog && !F.value.table.show, t = H.value.showTable;
			return {
				component: e ? Mt : Dt,
				title: `${F.value.style.chart.title.text}${F.value.style.chart.title.subtitle.text ? `: ${F.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					headerColor: F.value.table.th.color,
					headerBg: F.value.table.th.backgroundColor,
					isFullscreen: $.value,
					fullscreenParent: N.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: I.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: F.value.style.chart.backgroundColor,
							color: F.value.style.chart.color
						},
						head: {
							backgroundColor: F.value.style.chart.backgroundColor,
							color: F.value.style.chart.color
						}
					}
				}
			};
		});
		Ge(() => H.value.showTable, (e) => {
			F.value.table.show || (e && F.value.table.useDialog && qt.value ? qt.value.open() : "close" in qt.value && qt.value.close());
		});
		function _r() {
			H.value.showTable = !1, Jt.value && Jt.value.setTableIconState(!1);
		}
		let vr = p(() => F.value.style.chart.backgroundColor), yr = p(() => F.value.style.chart.title), { isCallbackImaging: br, isCallbackSvg: xr, generateSvg: Sr, onGenerateImage: Cr } = xe({
			svg: L,
			title: yr,
			legend: null,
			legendItems: null,
			backgroundColor: vr,
			getSvgCallback: () => F.value.userOptions.callbacks.svg,
			generateImage: bn
		});
		async function wr() {
			if (Lt("copyAlt", {
				config: F.value,
				dataset: Y.value
			}), !F.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(F.value.userOptions.callbacks.altCopy({
				config: F.value,
				dataset: Y.value
			}));
		}
		function Tr() {
			return Y.value.length;
		}
		function Er(e) {
			return Y.value[e]?.plots ?? [];
		}
		function Dr(e = $t.value, t = en.value) {
			return !Number.isInteger(e) || !Number.isInteger(t) ? null : Er(e)[t] ?? null;
		}
		function Or() {
			$t.value = null, en.value = null, P.value = "pointer", M.value = !1, Q.value = null;
		}
		function kr(e) {
			if (!e || !L.value) return;
			let t = L.value.getBoundingClientRect();
			tn.value = {
				x: t.left + e.x / B.value * t.width,
				y: t.top + e.y / V.value * t.height
			};
		}
		function Ar(e, t) {
			let n = Dr(e, t);
			n && ($t.value = e, en.value = t, P.value = "keyboard", kr(n), er({
				datapoint: n,
				seriesIndex: e,
				triggerMode: "keyboard"
			}));
		}
		function jr() {
			nn.value = !0, $t.value = null, en.value = null;
		}
		function Mr() {
			nn.value = !1, Or();
		}
		function Nr(e) {
			if (!L.value || dr.value || document.activeElement !== L.value || !Tr()) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "ArrowUp", i = e.key === "ArrowDown", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				Or();
				return;
			}
			let s = $t.value, c = en.value;
			if (s === null || c === null) s = 0, c = 0;
			else if (t || n) {
				let e = Tr();
				s = n ? (s + 1) % e : (s - 1 + e) % e;
				let t = Er(s);
				if (!t.length) return;
				c = Math.min(c, t.length - 1);
			} else if (r || i) {
				let e = Er(s);
				if (!e.length) return;
				c = r ? (c + 1) % e.length : (c - 1 + e.length) % e.length;
			}
			if (a) {
				let e = Dr();
				if (!e) return;
				Qn({
					datapoint: e,
					seriesIndex: $t.value
				});
				return;
			}
			Ar(s, c);
		}
		let Pr = p(() => ({
			headers: ar.value?.colNames ?? [],
			rows: ar.value?.body?.map((e) => e.map((e) => e && typeof e == "object" && "name" in e ? e.name : e)) ?? []
		}));
		return De({
			getData: sr,
			getImage: pr,
			generatePdf: yn,
			generateCsv: ir,
			generateImage: bn,
			generateSvg: Sr,
			toggleTable: cr,
			toggleLabels: lr,
			toggleTooltip: ur,
			toggleAnnotator: fr,
			toggleFullscreen: or,
			copyAlt: wr
		}), (e, t) => (C(), g("div", {
			ref_key: "stripPlotChart",
			ref: N,
			class: b(`vue-data-ui-component vue-ui-strip-plot ${$.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${F.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: S(`font-family:${F.value.style.fontFamily};width:100%; text-align:center;background:${F.value.style.chart.backgroundColor};${F.value.responsive ? "height:100%" : ""}`),
			id: `strip-plot_${j.value}`,
			onMouseenter: t[2] ||= () => O(dn)(!0),
			onMouseleave: t[3] ||= () => O(dn)(!1)
		}, [
			_("div", {
				id: `chart-instructions-${j.value}`,
				class: "sr-only"
			}, [_("p", null, D(F.value.a11y.translations.keyboardNavigation), 1)], 8, Ye),
			Pr.value?.rows?.length ? (C(), m(je, {
				key: 0,
				uid: j.value,
				head: Pr.value.headers,
				body: Pr.value.rows,
				notice: F.value.a11y.translations.tableAvailable,
				caption: F.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : h("", !0),
			F.value.userOptions.buttons.annotator ? (C(), m(O(At), {
				key: 1,
				svgRef: O(L),
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				active: dr.value,
				isCursorPointer: I.value,
				onClose: fr
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
			xn.value ? (C(), g("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Ut,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : h("", !0),
			F.value.style.chart.title.text ? (C(), g("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Vt,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(C(), m(Te, {
				key: `title_${Gt.value}`,
				config: {
					title: {
						cy: "donut-div-title",
						...F.value.style.chart.title
					},
					subtitle: {
						cy: "donut-div-subtitle",
						...F.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : h("", !0),
			F.value.userOptions.show && Rt.value && (O(fn) || O(un)) ? (C(), m(O(kt), {
				ref_key: "userOptionsRef",
				ref: Jt,
				key: `user_option_${zt.value}`,
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				isPrinting: O(_n),
				isImaging: O(vn),
				uid: j.value,
				hasTooltip: F.value.userOptions.buttons.tooltip && F.value.style.chart.tooltip.show,
				hasPdf: F.value.userOptions.buttons.pdf,
				hasXls: F.value.userOptions.buttons.csv,
				hasImg: F.value.userOptions.buttons.img,
				hasSvg: F.value.userOptions.buttons.svg,
				hasTable: F.value.userOptions.buttons.table,
				hasLabel: F.value.userOptions.buttons.labels && F.value.type !== "violin",
				hasFullscreen: F.value.userOptions.buttons.fullscreen,
				hasAltCopy: F.value.userOptions.buttons.altCopy,
				isTooltip: H.value.showTooltip,
				isFullscreen: $.value,
				titles: { ...F.value.userOptions.buttonTitles },
				chartElement: N.value,
				position: F.value.userOptions.position,
				hasAnnotator: F.value.userOptions.buttons.annotator,
				isAnnotation: dr.value,
				callbacks: F.value.userOptions.callbacks,
				printScale: F.value.userOptions.print.scale,
				tableDialog: F.value.table.useDialog,
				isCursorPointer: I.value,
				onToggleFullscreen: or,
				onGeneratePdf: O(yn),
				onGenerateCsv: ir,
				onGenerateImage: O(Cr),
				onGenerateSvg: O(Sr),
				onToggleTable: cr,
				onToggleLabels: lr,
				onToggleTooltip: ur,
				onToggleAnnotator: fr,
				onCopyAlt: wr,
				style: S({ visibility: O(fn) ? O(un) ? "visible" : "hidden" : "visible" })
			}, Fe({ _: 2 }, [
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
				e.$slots.optionLabels ? {
					name: "optionLabels",
					fn: k(() => [E(e.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: k(({ toggleFullscreen: t, isFullscreen: n }) => [E(e.$slots, "optionFullscreen", x(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: k(({ toggleAnnotator: t, isAnnotator: n }) => [E(e.$slots, "optionAnnotator", x(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: k(({ altCopy: t }) => [E(e.$slots, "optionAltCopy", x(y({ altCopy: t })), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: k(() => [E(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: k(() => [E(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasLabel.hasFullscreen.hasAltCopy.isTooltip.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : h("", !0),
			_("div", Xe, [(C(), g("svg", {
				ref_key: "svgRef",
				ref: L,
				xmlns: O(le),
				class: b({
					"vue-data-ui-fullscreen--on": $.value,
					"vue-data-ui-fulscreen--off": !$.value,
					"vue-data-ui-no-transition": !O(rn)
				}),
				viewBox: `0 0 ${B.value} ${V.value}`,
				style: S(`max-width:100%; overflow: visible; background:transparent;color:${F.value.style.chart.color};`),
				"aria-describedby": `chart-instructions-${j.value}`,
				tabindex: "0",
				onFocus: jr,
				onBlur: Mr,
				onKeydown: Nr
			}, [
				Le(O(jt)),
				e.$slots["chart-background"] ? (C(), g("foreignObject", {
					key: 0,
					x: G.value.left,
					y: G.value.top,
					width: Math.max(.1, G.value.width),
					height: Math.max(.1, G.value.height),
					style: { pointerEvents: "none" }
				}, [E(e.$slots, "chart-background", {}, void 0, !0)], 8, Qe)) : h("", !0),
				F.value.style.chart.grid.show ? (C(), g("g", $e, [
					F.value.style.chart.grid.horizontalGrid.show ? (C(), g("g", et, [(C(!0), g(f, null, T(Yn.value, (e) => (C(), g("line", {
						x1: e.x1,
						x2: e.x2,
						y1: e.y,
						y2: e.y,
						stroke: F.value.style.chart.grid.horizontalGrid.stroke,
						"stroke-width": F.value.style.chart.grid.horizontalGrid.strokeWidth,
						"stroke-dasharray": F.value.style.chart.grid.horizontalGrid.strokeDasharray,
						"stroke-linecap": "round"
					}, null, 8, tt))), 256))])) : h("", !0),
					F.value.style.chart.grid.verticalGrid.show ? (C(), g("g", nt, [(C(!0), g(f, null, T(q.value, (e, t) => (C(), g("line", {
						x1: G.value.left + (t + 1) * G.value.stripWidth,
						x2: G.value.left + (t + 1) * G.value.stripWidth,
						y1: G.value.top,
						y2: G.value.bottom,
						stroke: F.value.style.chart.grid.verticalGrid.stroke,
						"stroke-width": F.value.style.chart.grid.verticalGrid.strokeWidth,
						"stroke-dasharray": F.value.style.chart.grid.verticalGrid.strokeDasharray,
						"stroke-linecap": "round"
					}, null, 8, rt))), 256))])) : h("", !0),
					_("line", {
						x1: G.value.left,
						x2: G.value.left,
						y1: G.value.top,
						y2: G.value.bottom,
						stroke: F.value.style.chart.grid.stroke,
						"stroke-width": F.value.style.chart.grid.strokeWidth,
						"stroke-linecap": "round"
					}, null, 8, it),
					_("line", {
						x1: G.value.left,
						x2: G.value.right,
						y1: G.value.bottom,
						y2: G.value.bottom,
						stroke: F.value.style.chart.grid.stroke,
						"stroke-width": F.value.style.chart.grid.strokeWidth,
						"stroke-linecap": "round"
					}, null, 8, at)
				])) : h("", !0),
				F.value.style.chart.labels.yAxisLabels.show ? (C(), g("g", {
					key: 2,
					ref_key: "scaleLabels",
					ref: Qt
				}, [(C(!0), g(f, null, T(Yn.value, (e, t) => (C(), g("text", {
					class: b({ "vue-data-ui-transition": O(rn) }),
					key: `sl_${t}`,
					transform: `translate(${e.x1 + F.value.style.chart.labels.yAxisLabels.offsetX - 5}, ${e.y + F.value.style.chart.labels.yAxisLabels.fontSize / 3})`,
					fill: F.value.style.chart.labels.yAxisLabels.color,
					"font-size": F.value.style.chart.labels.yAxisLabels.fontSize,
					"text-anchor": "end"
				}, D(O(ie)(F.value.style.chart.labels.formatter, e.value, O(l)({
					p: F.value.style.chart.labels.prefix,
					v: e.value,
					s: F.value.style.chart.labels.suffix,
					r: F.value.style.chart.labels.yAxisLabels.rounding
				}), {
					datapoint: e,
					seriesIndex: t
				})), 11, ot))), 128))], 512)) : h("", !0),
				F.value.style.chart.labels.xAxisLabels.show ? (C(), g("g", {
					key: 3,
					ref_key: "timeLabelsEls",
					ref: Zt
				}, [(C(!0), g(f, null, T(mr.value, (e, t) => (C(), g("g", null, [String(e).includes("\n") ? (C(), g("text", {
					key: 1,
					class: "vue-ui-strip-plot-category-name",
					transform: `translate(${G.value.left + (t + 1) * G.value.stripWidth - G.value.stripWidth / 2}, ${G.value.bottom + F.value.style.chart.labels.xAxisLabels.fontSize * 2 + F.value.style.chart.labels.xAxisLabels.offsetY}), rotate(${F.value.style.chart.labels.xAxisLabels.rotation})`,
					"font-size": F.value.style.chart.labels.xAxisLabels.fontSize,
					fill: F.value.style.chart.labels.xAxisLabels.color,
					"text-anchor": F.value.style.chart.labels.xAxisLabels.rotation > 0 ? "start" : F.value.style.chart.labels.xAxisLabels.rotation < 0 ? "end" : "middle",
					innerHTML: O(r)({
						content: O(ee)(String(e)),
						fontSize: F.value.style.chart.labels.xAxisLabels.fontSize,
						fill: F.value.style.chart.labels.xAxisLabels.color,
						x: 0,
						y: 0
					})
				}, null, 8, ct)) : (C(), g("text", {
					key: 0,
					class: "vue-ui-strip-plot-category-name",
					transform: `translate(${G.value.left + (t + 1) * G.value.stripWidth - G.value.stripWidth / 2}, ${G.value.bottom + F.value.style.chart.labels.xAxisLabels.fontSize * 2 + F.value.style.chart.labels.xAxisLabels.offsetY}), rotate(${F.value.style.chart.labels.xAxisLabels.rotation})`,
					"font-size": F.value.style.chart.labels.xAxisLabels.fontSize,
					fill: F.value.style.chart.labels.xAxisLabels.color,
					"text-anchor": F.value.style.chart.labels.xAxisLabels.rotation > 0 ? "start" : F.value.style.chart.labels.xAxisLabels.rotation < 0 ? "end" : "middle"
				}, D(String(e)), 9, st))]))), 256))], 512)) : h("", !0),
				F.value.style.chart.labels.axis.yLabel ? (C(), g("text", {
					key: 4,
					ref_key: "yAxisLabel",
					ref: Xt,
					fill: F.value.style.chart.labels.axis.color,
					"font-size": F.value.style.chart.labels.axis.fontSize,
					transform: `translate(${F.value.style.chart.labels.axis.fontSize}, ${G.value.top + G.value.height / 2}) rotate(-90)`,
					"text-anchor": "middle"
				}, D(F.value.style.chart.labels.axis.yLabel), 9, lt)) : h("", !0),
				F.value.style.chart.labels.axis.xLabel ? (C(), g("text", {
					key: 5,
					ref_key: "xAxisLabel",
					ref: Yt,
					fill: F.value.style.chart.labels.axis.color,
					"font-size": F.value.style.chart.labels.axis.fontSize,
					x: G.value.left + G.value.width / 2,
					y: V.value - F.value.style.chart.labels.axis.fontSize / 3,
					"text-anchor": "middle"
				}, D(F.value.style.chart.labels.axis.xLabel), 9, ut)) : h("", !0),
				Q.value ? (C(), g(f, { key: 6 }, [
					_("line", {
						x1: G.value.left,
						x2: G.value.right,
						y1: Q.value.y,
						y2: Q.value.y,
						stroke: Q.value.color,
						"stroke-width": 1,
						class: b({ "select-circle": F.value.useCssAnimation })
					}, null, 10, dt),
					_("circle", {
						cx: G.value.left,
						cy: Q.value.y,
						r: 3,
						fill: Q.value.color,
						class: b({ "select-circle": F.value.useCssAnimation })
					}, null, 10, ft),
					_("circle", {
						cx: G.value.right,
						cy: Q.value.y,
						r: 3,
						fill: Q.value.color,
						class: b({ "select-circle": F.value.useCssAnimation })
					}, null, 10, pt)
				], 64)) : h("", !0),
				_("defs", null, [(C(!0), g(f, null, T(q.value, (e) => (C(), m(Ae, {
					t: "radial",
					id: e.id,
					key: `r_${e.id}`,
					fy: "30%",
					stops: [
						[
							"10%",
							O(o)(e.color, F.value.style.chart.plots.gradient.intensity / 100),
							1
						],
						[
							"90%",
							O(u)(e.color, .1),
							1
						],
						[
							"100%",
							e.color,
							1
						]
					]
				}, null, 8, ["id", "stops"]))), 128))]),
				(C(!0), g(f, null, T(Vn.value, (e, n) => (C(), g(f, { key: e.id }, [e.path ? (C(), g("path", {
					key: 0,
					d: e.path,
					fill: e.fill,
					stroke: e.color,
					"fill-opacity": F.value.style.chart.violin.opacity,
					"stroke-opacity": F.value.style.chart.violin.strokeOpacity,
					"stroke-width": F.value.style.chart.violin.strokeWidth,
					style: S({
						pointerEvents: X.value ? "auto" : "none",
						cursor: X.value && I.value ? "pointer" : "default"
					}),
					onMouseenter: (e) => X.value && Ln.value[n] && nr({
						boxPlot: Ln.value[n],
						seriesIndex: n,
						triggerMode: "pointer"
					}),
					onMouseleave: t[0] ||= (e) => X.value && tr()
				}, null, 44, mt)) : h("", !0), (C(!0), g(f, null, T(e.connectors, (t, n) => (C(), g("line", {
					key: `violin_connector_${e.id}_${n}`,
					x1: t.x,
					x2: t.x,
					y1: t.y1,
					y2: t.y2,
					stroke: e.color,
					"stroke-opacity": F.value.style.chart.violin.strokeOpacity,
					"stroke-width": F.value.style.chart.violin.strokeWidth,
					"stroke-linecap": "round",
					style: { "pointer-events": "none" }
				}, null, 8, ht))), 128))], 64))), 128)),
				(C(!0), g(f, null, T(Ln.value, (e, t) => (C(), g("g", {
					key: `boxplot_${e.id}`,
					style: S({ cursor: I.value ? "pointer" : "default" }),
					onMouseenter: (n) => nr({
						boxPlot: e,
						seriesIndex: t,
						triggerMode: "pointer"
					}),
					onMouseleave: tr
				}, [
					_("line", {
						x1: e.centerX,
						x2: e.centerX,
						y1: e.upperY,
						y2: e.lowerY,
						stroke: O(u)(e.boxPlotColor, .2),
						"stroke-width": Math.max(1, F.value.style.chart.violin.strokeWidth),
						"stroke-linecap": "round"
					}, null, 8, _t),
					_("rect", {
						x: e.boxLeft,
						y: Math.min(e.q1Y, e.q3Y),
						width: e.boxWidth,
						height: Math.max(1, Math.abs(e.q3Y - e.q1Y)),
						fill: e.boxPlotColor,
						stroke: O(u)(e.boxPlotColor, .2),
						"stroke-width": Math.max(1, F.value.style.chart.violin.strokeWidth),
						rx: e.boxWidth / 20
					}, null, 8, vt),
					_("circle", {
						cx: e.boxLeft + e.boxWidth / 2,
						cy: e.medianY,
						r: e.boxWidth / 3 * Math.min(1, F.value.style.chart.violin.boxPlot.medianCircleRadiusRatio),
						fill: F.value.style.chart.violin.boxPlot.medianCircleFill,
						stroke: O(u)(e.boxPlotColor, .2)
					}, null, 8, yt)
				], 44, gt))), 128)),
				(C(!0), g(f, null, T(Y.value, (t, n) => (C(), g(f, null, [X.value ? h("", !0) : (C(!0), g(f, { key: 0 }, T(t.plots, (n, r) => (C(), m(Ee, Re({ ref_for: !0 }, e.$attrs, {
					plot: {
						x: n.x,
						y: Wt.value ? n.y : G.value.top
					},
					radius: Q.value && Q.value.id === n.id ? U.value * 1.5 : U.value,
					shape: F.value.style.chart.plots.shape,
					stroke: F.value.style.chart.plots.stroke,
					strokeWidth: F.value.style.chart.plots.strokeWidth,
					color: F.value.style.chart.plots.gradient.show ? `url(#${t.id})` : t.color,
					style: `transition: all 0.2s ease-in-out; opacity:${Q.value ? Q.value.id === n.id ? 1 : .2 : F.value.style.chart.plots.opacity};${Cn.value ? `transition-delay:${r * 50}ms` : ""}`,
					class: {
						"vue-ui-strip-plot-animated": F.value.useCssAnimation && Cn.value && !O(sn),
						"vue-ui-strip-plot-select-circle": F.value.useCssAnimation && !Cn.value
					},
					onMouseenter: (e) => er({
						datapoint: n,
						seriesIndex: r,
						triggerMode: "pointer"
					}),
					onMouseleave: (e) => Zn({
						datapoint: n,
						seriesIndex: r
					}),
					onClick: (e) => Qn({
						datapoint: n,
						seriesIndex: r
					})
				}), null, 16, [
					"plot",
					"radius",
					"shape",
					"stroke",
					"strokeWidth",
					"color",
					"style",
					"class",
					"onMouseenter",
					"onMouseleave",
					"onClick"
				]))), 256)), H.value.dataLabels.show && !X.value ? (C(), g("g", bt, [(C(!0), g(f, null, T(t.plots, (e, n) => (C(), g(f, null, [n === t.plots.length - 1 || Q.value && Q.value.id === e.id && !H.value.showTooltip ? (C(), g("text", {
					key: 0,
					x: e.x,
					y: e.y + F.value.style.chart.labels.bestPlotLabel.offsetY - U.value * (Q.value && Q.value.id === e.id && !H.value.showTooltip ? 2 : 1.5),
					"font-size": F.value.style.chart.labels.bestPlotLabel.fontSize,
					fill: F.value.style.chart.labels.bestPlotLabel.color,
					"text-anchor": "middle",
					style: S(`opacity:${F.value.useCssAnimation ? +!Cn.value : 1};transition:opacity 0.2s ease-in;`)
				}, D(e.name) + " " + D(F.value.style.chart.labels.bestPlotLabel.showValue ? O(ie)(F.value.style.chart.labels.formatter, e.value, O(l)({
					p: `(${F.value.style.chart.labels.prefix}`,
					v: e.value,
					s: `${F.value.style.chart.labels.suffix})`,
					r: F.value.style.chart.labels.bestPlotLabel.rounding
				}), {
					datapoint: e,
					seriesIndex: n
				}) : ""), 13, xt)) : h("", !0)], 64))), 256))])) : h("", !0)], 64))), 256)),
				E(e.$slots, "svg", { svg: {
					...G.value,
					isPrintingImg: O(_n) || O(vn) || O(br),
					isPrintingSvg: O(xr),
					boxPlotSummaries: Ln.value,
					series: q.value
				} }, void 0, !0)
			], 46, Ze)), e.$slots.hint ? (C(), g("div", St, [E(e.$slots, "hint", x(y({
				hint: F.value.a11y.translations.keyboardNavigation,
				isVisible: nn.value
			})), void 0, !0)])) : h("", !0)]),
			e.$slots.watermark ? (C(), g("div", Ct, [E(e.$slots, "watermark", x(y({ isPrinting: O(_n) || O(vn) || O(br) || O(xr) })), void 0, !0)])) : h("", !0),
			e.$slots.source ? (C(), g("div", {
				key: 6,
				ref_key: "source",
				ref: Ht,
				dir: "auto"
			}, [E(e.$slots, "source", {}, void 0, !0)], 512)) : h("", !0),
			Le(O(Tt), {
				teleportTo: F.value.style.chart.tooltip.teleportTo,
				show: H.value.showTooltip && M.value,
				backgroundColor: F.value.style.chart.tooltip.backgroundColor,
				color: F.value.style.chart.tooltip.color,
				borderRadius: F.value.style.chart.tooltip.borderRadius,
				borderColor: F.value.style.chart.tooltip.borderColor,
				borderWidth: F.value.style.chart.tooltip.borderWidth,
				fontSize: F.value.style.chart.tooltip.fontSize,
				backgroundOpacity: F.value.style.chart.tooltip.backgroundOpacity,
				position: F.value.style.chart.tooltip.position,
				offsetX: F.value.style.chart.tooltip.offsetX,
				offsetY: F.value.style.chart.tooltip.offsetY,
				parent: N.value,
				content: Bt.value,
				isFullscreen: $.value,
				isCustom: O(fe)(F.value.style.chart.tooltip.customFormat),
				smooth: F.value.style.chart.tooltip.smooth,
				backdropFilter: F.value.style.chart.tooltip.backdropFilter,
				smoothForce: F.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: F.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: P.value === "keyboard",
				a11yPosition: tn.value
			}, {
				"tooltip-before": k(() => [E(e.$slots, "tooltip-before", x(y({ ...Xn.value })), void 0, !0)]),
				tooltip: k(() => [E(e.$slots, "tooltip", x(y({ ...Xn.value })), void 0, !0)]),
				"tooltip-after": k(() => [E(e.$slots, "tooltip-after", x(y({ ...Xn.value })), void 0, !0)]),
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
			Rt.value && F.value.userOptions.buttons.table ? (C(), m(He(gr.value.component), Re({ key: 7 }, gr.value.props, {
				ref_key: "tableUnit",
				ref: qt,
				onClose: _r
			}), Fe({
				content: k(() => [(C(), m(O(Ot), {
					key: `table_${Kt.value}`,
					colNames: ar.value.colNames,
					head: ar.value.head,
					body: ar.value.body,
					config: ar.value.config,
					title: F.value.table.useDialog ? "" : gr.value.title,
					withCloseButton: !F.value.table.useDialog,
					isCursorPointer: I.value,
					onClose: _r
				}, {
					th: k(({ th: e }) => [_("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, wt)]),
					td: k(({ td: e }) => [Ie(D(e.name || e), 1)]),
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
				fn: k(() => [Ie(D(gr.value.title), 1)]),
				key: "0"
			} : void 0, F.value.table.useDialog ? {
				name: "actions",
				fn: k(() => [_("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => ir(F.value.userOptions.callbacks.csv),
					style: S({ cursor: I.value ? "pointer" : "default" })
				}, [Le(O(Et), {
					name: "fileCsv",
					stroke: gr.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : h("", !0),
			E(e.$slots, "skeleton", {}, () => [O(sn) ? (C(), m(ve, { key: 0 })) : h("", !0)], !0)
		], 46, Je));
	}
}, [["__scopeId", "data-v-17198a70"]]);
//#endregion
export { qe as n, Tt as t };
