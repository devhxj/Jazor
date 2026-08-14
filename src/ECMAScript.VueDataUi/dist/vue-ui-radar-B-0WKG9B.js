import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, At as n, Bt as r, D as i, Jt as a, Kt as o, M as s, Pt as c, S as ee, Vt as te, X as l, b as ne, ct as re, i as ie, jt as ae, pt as oe, q as se, t as ce, tt as u, w as le, xt as ue } from "./lib-Bttd6u5E.js";
import { n as de } from "./useHints-Dq_w2E8B.js";
import { t as fe } from "./useConfig-DlNpz6P8.js";
import { t as pe } from "./usePrinter-DN5bYhTG.js";
import { n as me, t as he } from "./BaseScanner-DZvpgOjM.js";
import { t as ge } from "./useNestedProp-vPNvh7rV.js";
import { t as _e } from "./useThemeCheck-C43Tcqmk.js";
import { t as ve } from "./useChartExport-DNiwdPmb.js";
import { t as ye } from "./img-Bnokohej.js";
import { n as be } from "./Title-BE3qg9xl.js";
import { t as xe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Se, t as Ce } from "./useResponsive-ZtArZtUf.js";
import { t as we } from "./DefGrad-DVBqDjhO.js";
import { t as Te } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Ee } from "./A11yDataTable-DdRsVULz.js";
import { t as De } from "./useUserOptionState-DK-_1ddE.js";
import { t as Oe } from "./useChartAccessibility-DYqac8yF.js";
import { t as ke } from "./Legend-CQxUgOd-.js";
import { t as Ae } from "./useAutoSizeLabelsInsideViewbox-DvDwcwi_.js";
import { t as je } from "./vue_ui_radar-jafTED5j.js";
import { Fragment as d, Teleport as Me, computed as f, createBlock as p, createCommentVNode as m, createElementBlock as h, createElementVNode as g, createSlots as Ne, createTextVNode as Pe, createVNode as Fe, defineAsyncComponent as _, guardReactiveProps as v, mergeProps as Ie, nextTick as Le, normalizeClass as y, normalizeProps as b, normalizeStyle as x, onBeforeUnmount as Re, onMounted as ze, openBlock as S, ref as C, renderList as w, renderSlot as T, resolveDynamicComponent as Be, shallowRef as Ve, toDisplayString as E, toRefs as He, unref as D, watch as Ue, withCtx as O } from "vue";
//#region src/components/vue-ui-radar.vue
var We = /* @__PURE__ */ e({ default: () => yt }), Ge = ["id"], Ke = ["id"], qe = ["id"], Je = { style: { position: "relative" } }, Ye = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Xe = ["width", "height"], Ze = { key: 1 }, Qe = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], $e = { key: 0 }, et = [
	"d",
	"stroke",
	"stroke-width"
], tt = [
	"d",
	"stroke",
	"stroke-width"
], nt = { key: 2 }, rt = [
	"x",
	"y",
	"text-anchor",
	"font-size",
	"fill",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], it = [
	"points",
	"stroke",
	"stroke-width"
], at = [
	"points",
	"stroke",
	"stroke-width",
	"fill"
], ot = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], st = { key: 4 }, ct = [
	"cx",
	"cy",
	"fill",
	"r",
	"stroke"
], lt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, ut = {
	key: 5,
	class: "vue-data-ui-watermark"
}, dt = ["id"], ft = ["onClick"], pt = { class: "vue-data-ui-legend-item" }, mt = [
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], ht = {
	key: 0,
	style: {
		"max-width": "200px",
		margin: "0 auto"
	}
}, gt = {
	class: "vue-ui-radar-tooltip-datalabel",
	style: { width: "100%" }
}, _t = { class: "vue-ui-radar-tooltip-datalabel-name" }, vt = { key: 0 }, yt = /*#__PURE__*/ xe({
	__name: "vue-ui-radar",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	emits: ["selectLegend", "copyAlt"],
	setup(e, { expose: xe, emit: We }) {
		let yt = _(() => import("./Tooltip-DhjyfHwz.js")), bt = _(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), xt = _(() => import("./vue-ui-sparkbar-iyq8Toli.js").then((e) => e.n)), St = _(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Ct = _(() => import("./DataTable-BbKgJ5UI.js")), wt = _(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Tt = _(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Et = _(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Dt = _(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_radar: Ot } = fe(), { isThemeValid: kt, warnInvalidTheme: At } = _e(), k = e, jt = f(() => !!k.dataset && Object.keys(k.dataset).length), A = C(se()), j = C(!1), Mt = C(""), Nt = C(0), M = C(null), Pt = C(null), Ft = C(null), It = C(null), Lt = C(null), Rt = C(0), zt = C(0), Bt = C(0), Vt = C(!1), N = C(null), Ht = C(null), P = C(null), Ut = C({
			x: 0,
			y: 0
		}), Wt = C("pointer"), Gt = C(!1), F = C(Zt());
		de({
			config: () => F.value,
			dataset: () => k.dataset,
			component: "VueUiRadar",
			rules: [
				{
					test: (e) => e?.categories && e.categories.length === 0,
					message: [
						"👀 There are no categories in your dataset. Consider:",
						"",
						"▶️ Adding categories..."
					]
				},
				{
					test: (e) => e?.series && e.series.length === 0,
					message: [
						"👀 There are no series in your dataset. Consider:",
						"",
						"▶️ Adding series..."
					]
				},
				{
					test: (e) => e?.categories && e.categories.length > 3,
					message: [
						"👀 The number of categories > 3, the chart might become hard to read. Consider:",
						"",
						"▶️ Using several instances of the component to display related categories.",
						"",
						"▶️ Adding filters to allow users to choose a maximum number of categories."
					]
				},
				{
					test: (e) => e?.series && e.series.length > 12,
					message: [
						"👀 The number of series is > 12. Consider:",
						"",
						"▶️ Using several instances of the component to distribute the series.",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display.",
						""
					]
				}
			]
		});
		let I = f(() => F.value.userOptions.useCursorPointer), Kt = f(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				useCssAnimation: !1,
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						grid: { stroke: "#6A6A6A90" },
						labels: { dataLabels: { show: !1 } },
						outerPolygon: { stroke: "#6A6A6A" }
					},
					legend: { backgroundColor: "transparent" }
				} }
			},
			userConfig: F.value.skeletonConfig ?? {}
		})), { loading: qt, FINAL_DATASET: L } = me({
			...He(k),
			FINAL_CONFIG: F,
			prepareConfig: Zt,
			callback: () => {
				Promise.resolve().then(async () => {
					await Le(), H.value.showTable = F.value.table.show;
				});
			},
			skeletonDataset: k.config?.skeletonDataset ?? {
				categories: [{
					name: "_",
					color: "#6A6A6A"
				}],
				series: [
					{
						name: "_",
						values: [.6],
						target: 1
					},
					{
						name: "_",
						values: [.6],
						target: 1
					},
					{
						name: "_",
						values: [.6],
						target: 1
					},
					{
						name: "_",
						values: [.6],
						target: 1
					},
					{
						name: "_",
						values: [.6],
						target: 1
					},
					{
						name: "_",
						values: [.6],
						target: 1
					}
				]
			},
			skeletonConfig: a({
				defaultConfig: F.value,
				userConfig: Kt.value
			})
		}), { userOptionsVisible: Jt, setUserOptionsVisibility: Yt, keepUserOptionState: Xt } = De({ config: F.value }), { svgRef: R } = Oe({ config: F.value.style.chart.title });
		function Zt() {
			let e = ge({
				userConfig: k.config,
				defaultConfig: Ot
			}), t = e.theme;
			if (!t) return e;
			if (!kt.value(e)) return At(e), e;
			let n = ge({
				userConfig: je[t] || k.config,
				defaultConfig: e
			}), r = ge({
				userConfig: k.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : o[t] || c
			};
		}
		Ue(() => k.config, (e) => {
			F.value = Zt(), Jt.value = !F.value.userOptions.showOnChartHover, Qt(), Rt.value += 1, zt.value += 1, Bt.value += 1, H.value.dataLabels.show = F.value.style.chart.layout.labels.dataLabels.show, H.value.showTable = F.value.table.show, H.value.showTooltip = F.value.style.chart.tooltip ? F.value.style.chart.tooltip.show : !1;
		}, { deep: !0 });
		let z = Ve(null), B = Ve(null), V = f(() => F.value.debug);
		function Qt() {
			if (ae(k.dataset) && u({
				componentName: "VueUiRadar",
				type: "dataset",
				debug: V.value
			}), F.value.responsive) {
				let e = Se(() => {
					let { width: e, height: t } = Ce({
						chart: M.value,
						title: F.value.style.chart.title.text ? Pt.value : null,
						legend: F.value.style.chart.legend.show ? Ft.value : null,
						source: It.value,
						noTitle: Lt.value
					});
					requestAnimationFrame(() => {
						U.value.width = e, U.value.height = t, Tn();
					});
				});
				z.value && (B.value && z.value.unobserve(B.value), z.value.disconnect()), z.value = new ResizeObserver(e), B.value = M.value.parentNode, z.value.observe(B.value);
			}
			Tn();
		}
		ze(() => {
			Vt.value = !0, Qt();
		}), Re(() => {
			z.value && (B.value && z.value.unobserve(B.value), z.value.disconnect());
		});
		let { isPrinting: $t, isImaging: en, generatePdf: tn, generateImage: nn } = pe({
			elementId: `vue-ui-radar_${A.value}`,
			fileName: F.value.style.chart.title.text || "vue-ui-radar",
			options: F.value.userOptions.print
		}), rn = f(() => F.value.userOptions.show && !F.value.style.chart.title.text), an = f(() => le(F.value.customPalette)), H = C({
			dataLabels: { show: F.value.style.chart.layout.labels.dataLabels.show },
			showTable: F.value.table.show,
			showTooltip: F.value.style.chart.tooltip.show
		}), on = f(() => ({ style: {
			backgroundColor: "#FFFFFF00",
			animation: {
				show: F.value.style.chart.tooltip.animation.show,
				animationFrames: F.value.style.chart.tooltip.animation.animationFrames
			},
			labels: {
				fontSize: F.value.style.chart.tooltip.fontSize,
				name: { color: F.value.style.chart.tooltip.color }
			},
			gutter: {
				backgroundColor: "#CCCCCC",
				opacity: 30
			}
		} })), U = C({
			height: 312,
			width: 512
		}), sn = We, W = C([]), G = C(null), K = C(!1);
		function cn() {
			W.value.length ? W.value = [] : X.value.forEach((e, t) => {
				W.value.push(t);
			}), ln();
		}
		function ln() {
			sn("selectLegend", X.value.filter((e, t) => !W.value.includes(t)).map((e) => ({
				name: e.name,
				color: e.color,
				proportion: e.totalProportion
			})));
		}
		function un(e) {
			K.value = !0, W.value.includes(e) ? (G.value = e, W.value = W.value.filter((t) => t !== e), setTimeout(() => {
				K.value = !1, G.value = null;
			}, 500)) : (W.value.push(e), setTimeout(() => {
				K.value = !1;
			}, 500)), ln();
		}
		function dn() {
			return X.value.map((e) => ({
				name: e.name,
				color: e.color,
				proportion: e.totalProportion
			}));
		}
		function fn() {
			let e = L.value;
			if ([null, void 0].includes(e?.categories)) {
				u({
					componentName: "VueUiRadar",
					type: "dataset",
					debug: V.value
				}), u({
					componentName: "VueUiRadar",
					type: "datasetAttribute",
					property: "categories ({ name: string; prefix?: string; suffix?: string}[])",
					debug: V.value
				});
				return;
			}
			e.categories.length === 0 ? u({
				componentName: "VueUiRadar",
				type: "datasetAttributeEmpty",
				property: "categories",
				debug: V.value
			}) : V.value && e.categories.forEach((e, t) => {
				oe({
					datasetObject: e,
					requiredAttributes: ["name"]
				}).forEach((e) => {
					u({
						componentName: "VueUiRadar",
						type: "datasetAttribute",
						property: `category.${e} at index ${t}`,
						index: t
					});
				});
			}), [null, void 0].includes(e?.series) ? u({
				componentName: "VueUiRadar",
				type: "datasetAttribute",
				property: "series ({ name: string; values: number[]; color?: string; target: number}[])",
				debug: V.value
			}) : V.value && e.series.forEach((e, t) => {
				oe({
					datasetObject: e,
					requiredAttributes: [
						"name",
						"values",
						"target"
					]
				}).forEach((e) => {
					u({
						componentName: "VueUiRadar",
						type: "datasetSerieAttribute",
						key: "series",
						property: e,
						index: t
					});
				});
			});
		}
		Ue(() => L.value, () => fn(), {
			deep: !0,
			immediate: !0
		});
		let q = f(() => {
			let e = Array.isArray(L.value?.categories) ? L.value.categories : [], t = an.value ?? c;
			return e.map((e, n) => ({
				name: e?.name ?? "",
				categoryId: `radar_category_${A.value}_${n}`,
				color: ee(e?.color) || t[n] || c[n % c.length],
				prefix: e?.prefix ?? "",
				suffix: e?.suffix ?? ""
			}));
		}), J = f(() => L.value.series.map((e, t) => ({
			...e,
			color: ee(e.color) || an.value[t] || c[t] || c[t % c.length],
			serieId: `radar_serie_${A.value}_${t}`,
			formatter: e.formatter || null,
			absoluteIndex: t
		})));
		function pn(e) {
			return J.value.length ? X.value.find((t) => t.name === e) || (V.value && console.warn(`VueUiRadar - Series name not found "${e}"`), null) : (V.value && console.warn("VueUiRadar - There are no series to show."), null);
		}
		function mn(e) {
			let t = pn(e);
			t !== null && W.value.includes(t.absoluteIndex) && un(t.absoluteIndex);
		}
		function hn(e) {
			let t = pn(e);
			t !== null && (W.value.includes(t.absoluteIndex) || un(t.absoluteIndex));
		}
		let gn = f(() => Math.max(0, ...J.value.flatMap((e) => e.values)));
		function _n(e) {
			return F.value.style.chart.layout.scaleToAxisMax ? Math.max(1, e.target || 0, ...e.values) : e.target || gn.value || 1;
		}
		let vn = f(() => J.value.map((e, t) => {
			let n = xn.value.coordinates[t];
			return En({
				centerX: U.value.width / 2,
				centerY: U.value.height / 2,
				apexX: n.x,
				apexY: n.y,
				proportion: (e.target || 0) / _n(e)
			});
		})), yn = f(() => J.value.length), bn = f(() => Math.min(U.value.width, U.value.height) / 3), xn = f(() => s({
			plot: {
				x: U.value.width / 2,
				y: U.value.height / 2
			},
			radius: bn.value,
			sides: yn.value,
			rotation: 0
		})), Sn = f(() => {
			let e = [];
			for (let t = 0; t < bn.value; t += bn.value / F.value.style.chart.layout.grid.graduations) e.push(t);
			return e;
		}), Y = f(() => xn.value.coordinates.map((e, t) => {
			let n = J.value[t], r = J.value[t].values.map((t) => En({
				centerX: U.value.width / 2,
				centerY: U.value.height / 2,
				apexX: e.x,
				apexY: e.y,
				proportion: t / _n(n)
			}));
			return {
				...e,
				...n,
				plots: r
			};
		}).map((e) => ({
			...e,
			labelX: Cn(e).x,
			labelY: Cn(e).y,
			labelAnchor: Cn(e).anchor
		})));
		function Cn({ x: e, y: t }) {
			let n = "middle";
			return e = Math.round(e), t = Math.round(t), e > U.value.width / 2 && (e += 12, n = "start"), e < U.value.width / 2 && (e -= 12, n = "end"), t > U.value.height / 2 + 1 && (t += 20), t < U.value.height / 2 - 1 && (t -= 12), t === U.value.height / 2 && (t += 4), {
				x: e,
				y: t,
				anchor: n
			};
		}
		let wn = f({
			get: () => F.value.style.chart.layout.labels.dataLabels.fontSize,
			set: (e) => e
		}), { autoSizeLabels: Tn } = Ae({
			svgRef: R,
			fontSize: F.value.style.chart.layout.labels.dataLabels.fontSize,
			minFontSize: 6,
			sizeRef: wn,
			labelClass: ".vue-ui-radar-apex-label"
		});
		function En({ centerX: e, centerY: t, apexX: n, apexY: r, proportion: i }) {
			return {
				x: e + (n - e) * i,
				y: t + (r - t) * i
			};
		}
		let X = f(() => {
			let e = J.value.map((e, t) => e.values.map((t) => t / (e.target || gn.value)));
			return q.value.map((t, n) => {
				let r = ne(e.map((e) => e[n]).reduce((e, t) => e + t, 0) / J.value.length);
				return {
					...t,
					absoluteIndex: n,
					totalProportion: r,
					shape: "circle",
					opacity: W.value.includes(n) ? .5 : 1,
					segregate: () => un(n),
					isSegregated: W.value.includes(n),
					display: `${t.name}: ${l({
						v: (r ?? 0) * 100,
						s: "%",
						r: F.value.style.chart.legend.roundingPercentage
					})}`
				};
			});
		}), Dn = f(() => ({
			cy: "radar-div-legend",
			backgroundColor: F.value.style.chart.legend.backgroundColor,
			color: F.value.style.chart.legend.color,
			fontSize: F.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: F.value.style.chart.legend.bold ? "bold" : ""
		})), Z = f(() => {
			let e = [
				{
					name: F.value.translations.datapoint,
					color: ""
				},
				{
					name: F.value.translations.target,
					color: ""
				},
				...X.value
			];
			return {
				head: e,
				body: L.value.series.map((e) => [
					e.name,
					ie(e.formatter, e.target, l({
						p: e.prefix,
						v: e.target,
						s: e.suffix,
						r: F.value.table.td.roundingValue
					})),
					...e.values.map((t, n) => `${ie(e.formatter, t, l({
						p: q.value[n]?.prefix ?? "",
						v: t,
						s: q.value[n]?.suffix ?? "",
						r: F.value.table.td.roundingValue
					}))} (${isNaN(t / e.target) ? "" : l({
						v: t / e.target * 100,
						s: "%",
						r: F.value.table.td.roundingPercentage
					})})`)
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
				},
				colNames: e
			};
		}), Q = C(null), On = C([]), kn = C(null);
		function An(e, t) {
			j.value = !1, Q.value = null, P.value = null, Wt.value = "pointer", F.value.events.datapointLeave && F.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		function jn(e, t) {
			F.value.events.datapointClick && F.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Mn(e, t, n = "pointer") {
			F.value.events.datapointEnter && F.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), Wt.value = n, P.value = t;
			let r = q.value.slice();
			On.value = [], Q.value = t, j.value = !0, kn.value = {
				datapoint: e,
				seriesIndex: t,
				series: {
					categories: r,
					datapoints: J.value,
					radar: Y.value
				},
				config: F.value
			};
			let i = F.value.style.chart.tooltip.customFormat;
			if (ue(i) && re(() => i({
				seriesIndex: t,
				datapoint: e,
				series: {
					categories: r,
					datapoints: J.value,
					radar: Y.value
				},
				config: F.value
			}))) {
				Mt.value = i({
					seriesIndex: t,
					datapoint: e,
					series: {
						categories: r,
						datapoints: J.value,
						radar: Y.value
					},
					config: F.value
				});
				return;
			}
			Mt.value = `<div style="width:100%;text-align:center;border-bottom:1px solid ${F.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${e.name}</div>`;
			for (let t = 0; t < e.values.length; t += 1) if (!W.value.includes(t)) {
				let n = r[t], i = e.values[t], a = isNaN(i / e.target) ? 0 : i / e.target * 100, o = ie(e.formatter, i, l({
					p: q.value[t].prefix,
					v: i,
					s: q.value[t].suffix,
					r: F.value.style.chart.tooltip.roundingValue
				}), { datapoint: e }), s = l({
					v: a,
					s: "%",
					r: F.value.style.chart.tooltip.roundingPercentage
				}), c = F.value.style.chart.tooltip.showValue && F.value.style.chart.tooltip.showPercentage ? `${o} (${s})` : F.value.style.chart.tooltip.showValue && !F.value.style.chart.tooltip.showPercentage ? o : !F.value.style.chart.tooltip.showValue && F.value.style.chart.tooltip.showPercentage ? `${s}` : "";
				On.value.push({
					name: n?.name ?? `#${t + 1}`,
					value: e.values[t] / e.target * 100,
					color: n?.color,
					suffix: c,
					prefix: "",
					rounding: F.value.style.chart.tooltip.roundingPercentage,
					formatter: e.formatter
				});
			}
		}
		function Nn(e = null) {
			Le(() => {
				let n = [
					[F.value.style.chart.title.text],
					[F.value.style.chart.title.subtitle.text],
					[""]
				], r = [
					[""],
					[F.value.translations.target],
					...X.value.flatMap((e) => [[e.name], ["%"]])
				], a = L.value.series.map((e, t) => [
					e.name,
					e.target,
					...e.values.flatMap((t) => [t, isNaN(t / e.target) ? "" : t / e.target * 100])
				]), o = n.concat([r]).concat(a), s = i(o);
				e ? e(s) : t({
					csvContent: s,
					title: F.value.style.chart.title.text || "vue-ui-radar"
				});
			});
		}
		let $ = C(!1);
		function Pn(e) {
			$.value = e, Nt.value += 1;
		}
		function Fn() {
			H.value.showTable = !H.value.showTable;
		}
		function In() {
			H.value.showTooltip = !H.value.showTooltip;
		}
		let Ln = C(!1);
		function Rn() {
			Ln.value = !Ln.value;
		}
		async function zn({ scale: e = 2 } = {}) {
			if (!M.value) return;
			let { width: t, height: n } = M.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ye({
				domElement: M.value,
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
		let Bn = f(() => {
			let e = F.value.table.useDialog && !F.value.table.show, t = H.value.showTable;
			return {
				component: e ? Dt : St,
				title: `${F.value.style.chart.title.text}${F.value.style.chart.title.subtitle.text ? `: ${F.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					headerColor: F.value.table.th.color,
					headerBg: F.value.table.th.backgroundColor,
					isFullscreen: $.value,
					fullscreenParent: M.value,
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
		Ue(() => H.value.showTable, (e) => {
			F.value.table.show || (e && F.value.table.useDialog && N.value ? N.value.open() : "close" in N.value && N.value.close());
		});
		function Vn() {
			H.value.showTable = !1, Ht.value && Ht.value.setTableIconState(!1);
		}
		let Hn = f(() => X.value.map((e) => ({
			...e,
			name: e.display
		}))), Un = f(() => F.value.style.chart.backgroundColor), Wn = f(() => F.value.style.chart.legend), Gn = f(() => F.value.style.chart.title), { isCallbackImaging: Kn, isCallbackSvg: qn, generateSvg: Jn, onGenerateImage: Yn } = ve({
			svg: R,
			title: Gn,
			legend: Wn,
			legendItems: Hn,
			backgroundColor: Un,
			getSvgCallback: () => F.value.userOptions.callbacks.svg,
			generateImage: nn
		});
		async function Xn() {
			if (sn("copyAlt", {
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
		function Zn() {
			P.value = null, Gt.value = !0;
		}
		function Qn() {
			P.value = null, Wt.value = "pointer", j.value = !1, Q.value = null, Gt.value = !1;
		}
		function $n(e) {
			if (!Number.isFinite(e) || !R.value) return;
			let t = Y.value[e];
			if (!t) return;
			let n = R.value.getBoundingClientRect();
			Ut.value = {
				x: n.left + t.labelX / U.value.width * n.width,
				y: n.top + t.labelY / U.value.height * n.height
			};
		}
		function er(e) {
			if (!R.value || Ln.value || document.activeElement !== R.value || !Y.value.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				P.value = null, Wt.value = "pointer", j.value = !1, Q.value = null;
				return;
			}
			if (r) {
				if (P.value === null) return;
				let e = Y.value[P.value];
				if (!e) return;
				jn(e, P.value);
				return;
			}
			let a = P.value;
			a === null || a < 0 || a >= Y.value.length ? a = n ? 0 : Y.value.length - 1 : n ? (a += 1, a >= Y.value.length && (a = 0)) : t && (--a, a < 0 && (a = Y.value.length - 1));
			let o = Y.value[a];
			o && (P.value = a, $n(a), Mn(o, a, "keyboard"));
		}
		let tr = f(() => ({
			headers: Z.value?.colNames?.map((e) => e.name ?? e) ?? [],
			rows: Z.value?.body ?? []
		}));
		return xe({
			getData: dn,
			getImage: zn,
			generatePdf: tn,
			generateCsv: Nn,
			generateImage: nn,
			generateSvg: Jn,
			hideSeries: hn,
			showSeries: mn,
			toggleTable: Fn,
			toggleTooltip: In,
			toggleAnnotator: Rn,
			toggleFullscreen: Pn,
			copyAlt: Xn
		}), (e, t) => (S(), h("div", {
			class: y(`vue-data-ui-component vue-ui-radar ${$.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${F.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			ref_key: "radarChart",
			ref: M,
			id: `vue-ui-radar_${A.value}`,
			style: x(`font-family:${F.value.style.fontFamily};width:100%; ${F.value.responsive ? "height: 100%;" : ""} text-align:center;background:${F.value.style.chart.backgroundColor}`),
			onMouseenter: t[2] ||= () => D(Yt)(!0),
			onMouseleave: t[3] ||= () => D(Yt)(!1)
		}, [
			g("div", {
				id: `chart-instructions-${A.value}`,
				class: "sr-only"
			}, [g("p", null, E(F.value.a11y.translations.keyboardNavigation), 1)], 8, Ke),
			tr.value?.rows?.length ? (S(), p(Ee, {
				key: 0,
				uid: A.value,
				head: tr.value.headers,
				body: tr.value.rows,
				notice: F.value.a11y.translations.tableAvailable,
				caption: F.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : m("", !0),
			F.value.userOptions.buttons.annotator ? (S(), p(D(wt), {
				key: 1,
				svgRef: D(R),
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				active: Ln.value,
				isCursorPointer: I.value,
				onClose: Rn
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
			rn.value ? (S(), h("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Lt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : m("", !0),
			F.value.style.chart.title.text ? (S(), h("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Pt,
				style: "width:100%;background:transparent;padding-bottom:12px"
			}, [(S(), p(be, {
				key: `title_${Rt.value}`,
				config: {
					title: {
						cy: "radar-div-title",
						...F.value.style.chart.title
					},
					subtitle: {
						cy: "radar-div-subtitle",
						...F.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : m("", !0),
			g("div", { id: `legend-top-${A.value}` }, null, 8, qe),
			F.value.userOptions.show && jt.value && (D(Xt) || D(Jt)) ? (S(), p(D(Tt), {
				ref_key: "userOptionsRef",
				ref: Ht,
				key: `user_options_${Nt.value}`,
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				isImaging: D(en),
				isPrinting: D($t),
				uid: A.value,
				hasTooltip: F.value.userOptions.buttons.tooltip && F.value.style.chart.tooltip.show,
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
				chartElement: M.value,
				position: F.value.userOptions.position,
				hasAnnotator: F.value.userOptions.buttons.annotator,
				isAnnotation: Ln.value,
				callbacks: F.value.userOptions.callbacks,
				printScale: F.value.userOptions.print.scale,
				tableDialog: F.value.table.useDialog,
				isCursorPointer: I.value,
				onToggleFullscreen: Pn,
				onGeneratePdf: D(tn),
				onGenerateCsv: Nn,
				onGenerateImage: D(Yn),
				onGenerateSvg: D(Jn),
				onToggleTable: Fn,
				onToggleTooltip: In,
				onToggleAnnotator: Rn,
				onCopyAlt: Xn,
				style: x({ visibility: D(Xt) ? D(Jt) ? "visible" : "hidden" : "visible" })
			}, Ne({ _: 2 }, [
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
			g("div", Je, [(S(), h("svg", {
				ref_key: "svgRef",
				ref: R,
				xmlns: D(ce),
				"aria-describedby": `chart-instructions-${A.value}`,
				class: y({
					"vue-data-ui-fullscreen--on": $.value,
					"vue-data-ui-fulscreen--off": !$.value
				}),
				viewBox: `0 0 ${U.value.width <= 0 ? 10 : U.value.width} ${U.value.height <= 0 ? 10 : U.value.height}`,
				style: x(`max-width:100%;overflow:visible;background:transparent;color:${F.value.style.chart.color}`),
				tabindex: "0",
				onFocus: Zn,
				onBlur: Qn,
				onKeydown: er
			}, [
				Fe(D(Et)),
				e.$slots["chart-background"] ? (S(), h("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: U.value.width <= 0 ? 10 : U.value.width,
					height: U.value.height <= 0 ? 10 : U.value.height,
					style: { pointerEvents: "none" }
				}, [T(e.$slots, "chart-background", {}, void 0, !0)], 8, Xe)) : m("", !0),
				g("defs", null, [(S(!0), h(d, null, w(q.value, (e, t) => (S(), p(we, {
					t: "radial",
					cx: "50%",
					cy: "50%",
					r: "50%",
					fx: "50%",
					fy: "50%",
					id: `radar_gradient_${A.value}_${t}`,
					key: `radar_gradient_${A.value}_${t}`,
					stops: [[
						"0%",
						D(r)(D(te)(e.color, .05), F.value.style.chart.layout.dataPolygon.opacity),
						1
					], [
						"100%",
						D(r)(e.color, F.value.style.chart.layout.dataPolygon.opacity),
						1
					]]
				}, null, 8, ["id", "stops"]))), 128))]),
				F.value.style.chart.layout.grid.show ? (S(), h("g", Ze, [(S(!0), h(d, null, w(Y.value, (e) => (S(), h("line", {
					x1: U.value.width / 2,
					y1: U.value.height / 2,
					x2: e.x,
					y2: e.y,
					stroke: F.value.style.chart.layout.grid.stroke,
					"stroke-width": F.value.style.chart.layout.grid.strokeWidth
				}, null, 8, Qe))), 256)), F.value.style.chart.layout.grid.graduations > 0 ? (S(), h("g", $e, [(S(!0), h(d, null, w(Sn.value, (e) => (S(), h("path", {
					d: D(s)({
						plot: {
							x: U.value.width / 2,
							y: U.value.height / 2
						},
						radius: e,
						sides: yn.value,
						rotation: 0
					}).path,
					fill: "none",
					stroke: F.value.style.chart.layout.grid.stroke,
					"stroke-width": F.value.style.chart.layout.grid.strokeWidth
				}, null, 8, et))), 256))])) : m("", !0)])) : m("", !0),
				g("path", {
					d: xn.value.path,
					fill: "none",
					stroke: F.value.style.chart.layout.outerPolygon.stroke,
					"stroke-width": F.value.style.chart.layout.outerPolygon.strokeWidth,
					"stroke-linejoin": "round",
					"stroke-linecap": "round"
				}, null, 8, tt),
				F.value.style.chart.layout.labels.dataLabels.show ? (S(), h("g", nt, [(S(!0), h(d, null, w(Y.value, (e, t) => (S(), h("text", {
					class: "vue-ui-radar-apex-label",
					x: e.labelX,
					y: e.labelY,
					"text-anchor": e.labelAnchor,
					"font-size": F.value.style.chart.layout.labels.dataLabels.fontSize,
					fill: F.value.style.chart.layout.labels.dataLabels.color,
					onMouseenter: (n) => Mn(e, t, "pointer"),
					onMouseleave: (n) => An(e, t),
					onClick: (n) => jn(e, t)
				}, E(e.name), 41, rt))), 256))])) : m("", !0),
				(S(!0), h(d, null, w(q.value, (e, t) => (S(), h("g", null, [g("g", null, [F.value.useCssAnimation || !F.value.useCssAnimation && !W.value.includes(t) ? (S(), h("polygon", {
					key: 0,
					points: D(n)(Y.value.map((e) => e.plots[t]), !1, !0),
					stroke: F.value.style.chart.backgroundColor,
					"stroke-width": F.value.style.chart.layout.dataPolygon.strokeWidth + 1,
					fill: "none",
					class: y({
						"animated-out": W.value.includes(t) && F.value.useCssAnimation,
						"animated-in": K.value && G.value === t && F.value.useCssAnimation
					})
				}, null, 10, it)) : m("", !0), F.value.useCssAnimation || !F.value.useCssAnimation && !W.value.includes(t) ? (S(), h("polygon", {
					key: 1,
					points: D(n)(Y.value.map((e) => e.plots[t]), !1, !0),
					stroke: e.color,
					"stroke-width": F.value.style.chart.layout.dataPolygon.strokeWidth,
					fill: F.value.style.chart.layout.dataPolygon.transparent ? "transparent" : F.value.style.chart.layout.dataPolygon.useGradient ? `url(#radar_gradient_${A.value}_${t})` : D(r)(e.color, F.value.style.chart.layout.dataPolygon.opacity),
					class: y({
						"animated-out": W.value.includes(t) && F.value.useCssAnimation,
						"animated-in": K.value && G.value === t && F.value.useCssAnimation
					})
				}, null, 10, at)) : m("", !0)])]))), 256)),
				F.value.style.chart.layout.targetReference.show ? (S(), h("path", {
					key: 3,
					class: "vue-ui-radar-target-polygon",
					d: `M${D(n)(vn.value, !1, !0)}Z`,
					fill: "none",
					stroke: F.value.style.chart.layout.targetReference.stroke,
					"stroke-width": F.value.style.chart.layout.targetReference.strokeWidth,
					"stroke-linejoin": "round",
					"stroke-linecap": "round",
					"stroke-dasharray": F.value.style.chart.layout.targetReference.strokeDasharray
				}, null, 8, ot)) : m("", !0),
				F.value.style.chart.layout.plots.show ? (S(), h("g", st, [(S(!0), h(d, null, w(Y.value, (e, t) => (S(), h("g", null, [(S(!0), h(d, null, w(e.plots, (e, n) => (S(), h("circle", {
					cx: e.x,
					cy: e.y,
					fill: W.value.includes(n) ? "transparent" : q.value[n] ? q.value[n].color : "transparent",
					r: Q.value !== null && Q.value === t ? F.value.style.chart.layout.plots.radius * 1.6 : F.value.style.chart.layout.plots.radius,
					stroke: W.value.includes(n) ? "transparent" : F.value.style.chart.backgroundColor,
					"stroke-width": .5,
					class: y({
						"animated-out": W.value.includes(n) && F.value.useCssAnimation,
						"animated-in": K.value && G.value === n && F.value.useCssAnimation
					})
				}, null, 10, ct))), 256))]))), 256))])) : m("", !0),
				T(e.$slots, "svg", { svg: {
					...U.value,
					outerPolygon: xn.value,
					isPrintingImg: D($t) || D(en) || D(Kn),
					isPrintingSvg: D(qn)
				} }, void 0, !0)
			], 46, Ye)), e.$slots.hint ? (S(), h("div", lt, [T(e.$slots, "hint", b(v({
				hint: F.value.a11y.translations.keyboardNavigation,
				isVisible: Gt.value
			})), void 0, !0)])) : m("", !0)]),
			e.$slots.watermark ? (S(), h("div", ut, [T(e.$slots, "watermark", b(v({ isPrinting: D($t) || D(en) || D(Kn) || D(qn) })), void 0, !0)])) : m("", !0),
			g("div", { id: `legend-bottom-${A.value}` }, null, 8, dt),
			Vt.value && (F.value.style.chart.legend.show || e.$slots.legend) ? (S(), p(Me, {
				key: 6,
				to: F.value.style.chart.legend.position === "top" ? `#legend-top-${A.value}` : `#legend-bottom-${A.value}`
			}, [g("div", {
				ref_key: "chartLegend",
				ref: Ft
			}, [T(e.$slots, "legend", { legend: X.value }, () => [F.value.style.chart.legend.show ? (S(), p(ke, {
				key: `legend_${Bt.value}`,
				legendSet: X.value,
				config: Dn.value,
				isCursorPointer: I.value,
				onClickMarker: t[0] ||= ({ i: e }) => un(e)
			}, Ne({
				item: O(({ legend: e, index: t }) => [D(qt) ? m("", !0) : (S(), h("div", {
					key: 0,
					onClick: (t) => e.segregate(),
					style: x(`opacity:${W.value.includes(t) ? .5 : 1}`)
				}, E(e.display), 13, ft))]),
				legendToggle: O(() => [X.value.length > 2 && F.value.style.chart.legend.selectAllToggle.show && !D(qt) ? (S(), p(Te, {
					key: 0,
					backgroundColor: F.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: F.value.style.chart.legend.selectAllToggle.color,
					fontSize: F.value.style.chart.legend.fontSize,
					checked: W.value.length > 0,
					isCursorPointer: I.value,
					onToggle: cn
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : m("", !0)]),
				_: 2
			}, [F.value.style.chart.layout.targetReference.show && F.value.style.chart.layout.targetReference.showInLegend ? {
				name: "after",
				fn: O(() => [g("div", pt, [g("div", { style: x({
					display: "flex",
					height: F.value.style.chart.legend.fontSize
				}) }, [(S(), h("svg", {
					style: x({ width: F.value.style.chart.legend.fontSize * 2 }),
					viewBox: "0 0 16 8"
				}, [g("line", {
					x1: "0",
					x2: "16",
					y1: "4",
					y2: "4",
					stroke: F.value.style.chart.layout.targetReference.stroke,
					"stroke-width": F.value.style.chart.layout.targetReference.strokeWidth * .7,
					"stroke-dasharray": F.value.style.chart.layout.targetReference.strokeDasharray * .7,
					"stroke-linecap": "round"
				}, null, 8, mt)], 4))], 4), g("span", null, E(F.value.style.chart.layout.targetReference.legendLabel), 1)])]),
				key: "0"
			} : void 0]), 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : m("", !0)], !0)], 512)], 8, ["to"])) : m("", !0),
			e.$slots.source ? (S(), h("div", {
				key: 7,
				ref_key: "source",
				ref: It,
				dir: "auto"
			}, [T(e.$slots, "source", {}, void 0, !0)], 512)) : m("", !0),
			Fe(D(yt), {
				teleportTo: F.value.style.chart.tooltip.teleportTo,
				show: H.value.showTooltip && j.value,
				backgroundColor: F.value.style.chart.tooltip.backgroundColor,
				color: F.value.style.chart.tooltip.color,
				borderRadius: F.value.style.chart.tooltip.borderRadius,
				borderColor: F.value.style.chart.tooltip.borderColor,
				borderWidth: F.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: F.value.style.chart.tooltip.backgroundOpacity,
				fontSize: F.value.style.chart.tooltip.fontSize,
				position: F.value.style.chart.tooltip.position,
				offsetX: F.value.style.chart.tooltip.offsetX,
				offsetY: F.value.style.chart.tooltip.offsetY,
				parent: M.value,
				content: Mt.value,
				isFullscreen: $.value,
				isCustom: F.value.style.chart.tooltip.customFormat && typeof F.value.style.chart.tooltip.customFormat == "function",
				smooth: F.value.style.chart.tooltip.smooth,
				backdropFilter: F.value.style.chart.tooltip.backdropFilter,
				smoothForce: F.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: F.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: Wt.value === "keyboard",
				a11yPosition: Ut.value
			}, {
				"tooltip-before": O(() => [T(e.$slots, "tooltip-before", b(v({ ...kn.value })), void 0, !0)]),
				tooltip: O(() => [T(e.$slots, "tooltip", b(v({ ...kn.value })), void 0, !0)]),
				"tooltip-after": O(() => [!["function"].includes(typeof F.value.style.chart.tooltip.customFormat) && !e.$slots.tooltip ? (S(), h("div", ht, [Fe(D(xt), {
					dataset: On.value,
					config: on.value,
					backgroundOpacity: 0
				}, {
					"data-label": O(({ bar: e }) => [g("div", gt, [g("span", _t, E(e.name + (F.value.style.chart.tooltip.showValue || F.value.style.chart.tooltip.showPercentage ? ":" : "")), 1), F.value.style.chart.tooltip.showValue || F.value.style.chart.tooltip.showPercentage ? (S(), h("span", vt, E(e.suffix), 1)) : m("", !0)])]),
					_: 1
				}, 8, ["dataset", "config"])])) : m("", !0), T(e.$slots, "tooltip-after", b(v({ ...kn.value })), void 0, !0)]),
				_: 3
			}, 8, [
				"teleportTo",
				"show",
				"backgroundColor",
				"color",
				"borderRadius",
				"borderColor",
				"borderWidth",
				"backgroundOpacity",
				"fontSize",
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
			jt.value && F.value.userOptions.buttons.table ? (S(), p(Be(Bn.value.component), Ie({ key: 8 }, Bn.value.props, {
				ref_key: "tableUnit",
				ref: N,
				onClose: Vn
			}), Ne({
				content: O(() => [(S(), p(D(Ct), {
					key: `table_${zt.value}`,
					colNames: Z.value.colNames,
					head: Z.value.head,
					body: Z.value.body,
					config: Z.value.config,
					title: F.value.table.useDialog ? "" : Bn.value.title,
					withCloseButton: !F.value.table.useDialog,
					isCursorPointer: I.value,
					onClose: Vn
				}, {
					th: O(({ th: e }) => [Pe(E(e.name), 1)]),
					td: O(({ td: e }) => [Pe(E(e), 1)]),
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
				fn: O(() => [Pe(E(Bn.value.title), 1)]),
				key: "0"
			} : void 0, F.value.table.useDialog ? {
				name: "actions",
				fn: O(() => [g("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Nn(F.value.userOptions.callbacks.csv),
					style: x({ cursor: I.value ? "pointer" : "default" })
				}, [Fe(D(bt), {
					name: "fileCsv",
					stroke: Bn.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : m("", !0),
			T(e.$slots, "skeleton", {}, () => [D(qt) ? (S(), p(he, { key: 0 })) : m("", !0)], !0)
		], 46, Ge));
	}
}, [["__scopeId", "data-v-0a3f041f"]]);
//#endregion
export { We as n, yt as t };
