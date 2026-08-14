import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, G as i, Jt as a, Kt as ee, Pt as o, Rt as te, S as ne, Vt as re, X as s, b as ie, ct as ae, i as oe, jt as se, pt as ce, q as le, t as ue, tt as de, w as fe, xt as pe } from "./lib-Bttd6u5E.js";
import { n as me, t as he } from "./useHints-Dq_w2E8B.js";
import { t as ge } from "./useConfig-DlNpz6P8.js";
import { t as _e } from "./usePrinter-DN5bYhTG.js";
import { n as ve, t as ye } from "./BaseScanner-DZvpgOjM.js";
import { t as be } from "./useNestedProp-vPNvh7rV.js";
import { t as xe } from "./useThemeCheck-C43Tcqmk.js";
import { t as Se } from "./useChartExport-DNiwdPmb.js";
import { t as Ce } from "./useTransitions-g_zBREk2.js";
import { t as we } from "./img-Bnokohej.js";
import { n as Te } from "./Title-BE3qg9xl.js";
import { t as Ee } from "./Shape-C21CMlWS.js";
import { t as De } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Oe, t as ke } from "./useResponsive-ZtArZtUf.js";
import { t as Ae } from "./DefGrad-DVBqDjhO.js";
import { t as je } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Me } from "./A11yDataTable-DdRsVULz.js";
import { t as Ne } from "./useUserOptionState-DK-_1ddE.js";
import { t as Pe } from "./useChartAccessibility-DYqac8yF.js";
import { t as Fe } from "./labelUtils-BeVpDvTJ.js";
import { t as Ie } from "./Legend-CQxUgOd-.js";
import { t as Le } from "./vue_ui_rings-BVgD2aMn.js";
import { Fragment as Re, Teleport as ze, computed as c, createBlock as l, createCommentVNode as u, createElementBlock as d, createElementVNode as f, createSlots as Be, createTextVNode as Ve, createVNode as He, defineAsyncComponent as p, guardReactiveProps as m, mergeProps as Ue, nextTick as We, normalizeClass as h, normalizeProps as g, normalizeStyle as _, onBeforeUnmount as Ge, onMounted as Ke, openBlock as v, ref as y, renderList as qe, renderSlot as b, resolveDynamicComponent as Je, shallowRef as Ye, toDisplayString as Xe, toRefs as Ze, unref as x, useSlots as Qe, watch as $e, withCtx as S } from "vue";
//#region src/components/vue-ui-rings.vue
var et = /* @__PURE__ */ e({ default: () => bt }), tt = ["id"], nt = ["id"], rt = ["id"], it = { style: { position: "relative" } }, at = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], ot = ["width", "height"], st = { key: 1 }, ct = [
	"stroke",
	"cx",
	"cy",
	"r",
	"fill"
], lt = [
	"stroke",
	"stroke-width",
	"cx",
	"cy",
	"r",
	"fill"
], ut = [
	"stroke",
	"stroke-width",
	"cx",
	"cy",
	"r",
	"fill"
], dt = [
	"cx",
	"cy",
	"r",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], ft = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], pt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke"
], mt = [
	"text-anchor",
	"font-size",
	"fill",
	"font-weight",
	"transform",
	"onMouseenter",
	"onMouseleave",
	"onClick",
	"innerHTML"
], ht = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, gt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, _t = ["id"], vt = ["onClick"], yt = ["innerHTML"], bt = /*#__PURE__*/ De({
	__name: "vue-ui-rings",
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
	setup(e, { expose: De, emit: et }) {
		let bt = p(() => import("./Tooltip-DhjyfHwz.js")), xt = p(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), St = p(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Ct = p(() => import("./DataTable-BbKgJ5UI.js")), wt = p(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Tt = p(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Et = p(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Dt = p(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_rings: Ot } = ge(), { isThemeValid: kt, warnInvalidTheme: At } = xe(), jt = Qe(), C = e, w = y(!1), Mt = c(() => !!C.dataset && C.dataset.length), T = y(le()), E = y(!1), Nt = y(""), D = y(null), Pt = y(0), O = y(null), Ft = y(null), It = y(null), Lt = y(null), Rt = y(null), zt = y(0), Bt = y(0), Vt = y(0), Ht = y(!1), Ut = y(!1), k = y(null), Wt = y(null), A = y(null), Gt = y({
			x: 0,
			y: 0
		}), Kt = y("pointer"), qt = y(!1), j = y(en());
		me({
			config: () => j.value,
			dataset: () => C.dataset,
			component: "VueUiRings",
			rules: [
				he.singleSeries,
				he.emptyArray,
				{
					test: (e) => e.length > 6,
					message: [
						"👀 The number of series is > 6. Consider:",
						"",
						"▶️ Grouping small values dynamically into a single \"Other\" series.",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display.",
						"",
						"▶️ Using VueUiHorizontalBar instead to make the dataset breakdown easier to read."
					]
				}
			]
		});
		let { transitionEnabled: M } = Ce({
			config: () => j.value.transitions,
			dataset: () => C.dataset
		}), N = c(() => j.value.userOptions.useCursorPointer), Jt = c(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						rings: {
							stroke: "#6A6A6A",
							gradient: { underlayerColor: "#FFFFFF" }
						},
						labels: { dataLabels: { show: !1 } }
					},
					legend: { backgroundColor: "transparent" }
				} }
			},
			userConfig: j.value.skeletonConfig ?? {}
		})), { loading: P, FINAL_DATASET: Yt, manualLoading: Xt } = ve({
			...Ze(C),
			FINAL_CONFIG: j,
			prepareConfig: en,
			skeletonDataset: C.config?.skeletonDataset ?? [
				{
					name: "_",
					values: [13],
					color: "#808080"
				},
				{
					name: "_",
					values: [8],
					color: "#969696"
				},
				{
					name: "_",
					values: [5],
					color: "#ADADAD"
				},
				{
					name: "_",
					values: [3],
					color: "#C4C4C4"
				},
				{
					name: "_",
					values: [2],
					color: "#DBDBDB"
				}
			],
			skeletonConfig: a({
				defaultConfig: j.value,
				userConfig: Jt.value
			})
		}), { userOptionsVisible: Zt, setUserOptionsVisibility: Qt, keepUserOptionState: $t } = Ne({ config: j.value }), { svgRef: F } = Pe({ config: j.value.style.chart.title });
		function en() {
			let e = be({
				userConfig: C.config,
				defaultConfig: Ot
			}), t = e.theme;
			if (!t) return e;
			if (!kt.value(e)) return At(e), e;
			let n = be({
				userConfig: Le[t] || C.config,
				defaultConfig: e
			});
			return {
				...be({
					userConfig: C.config,
					defaultConfig: n
				}),
				customPalette: e.customPalette.length ? e.customPalette : ee[e.theme] || o
			};
		}
		$e(() => C.config, (e) => {
			P.value || (j.value = en()), Zt.value = !j.value.userOptions.showOnChartHover, nn(), zt.value += 1, Bt.value += 1, Vt.value += 1, B.value.showTable = j.value.table.show, B.value.showTooltip = j.value.style.chart.tooltip.show, B.value.showLabels = j.value.style.chart.layout.labels.dataLabels.show, V.value.width = j.value.style.chart.size, V.value.height = j.value.style.chart.size;
		}, { deep: !0 });
		let tn = c(() => {
			let { markers: e } = j.value.style.chart.layout.labels.dataLabels, t = J.value / 2, n = B.value.showLabels ? e.position === "left" ? t : -t : 0, r = B.value.showLabels ? V.value.width / 2 - t : 0, i = e.position === "left" ? r : -r;
			return {
				x: j.value.responsive ? i : n / j.value.style.chart.size * V.value.width,
				y: 0 / j.value.style.chart.size * V.value.height
			};
		}), I = c(() => ({
			x: V.value.width / 2 + tn.value.x,
			y: V.value.height / 2 + tn.value.y
		}));
		$e(() => C.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Xt.value = !1);
		}, { deep: !0 });
		let L = Ye(null), R = Ye(null);
		Ke(() => {
			Ut.value = !0, nn();
		});
		let z = c(() => j.value.debug);
		function nn() {
			if (se(C.dataset) ? (de({
				componentName: "VueUiRings",
				type: "dataset",
				debug: z.value
			}), Xt.value = !0) : C.dataset.forEach((e, t) => {
				e.values.length || (de({
					componentName: "VueUiRings",
					type: "dataset",
					debug: z.value
				}), Xt.value = !0), ce({
					datasetObject: e,
					requiredAttributes: ["name", "values"]
				}).forEach((e) => {
					de({
						componentName: "VueUiRings",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: z.value
					});
				});
			}), se(C.dataset) || (Xt.value = j.value.loading), j.value.responsive) {
				let e = Oe(() => {
					Ht.value = !0;
					let { width: e, height: t } = ke({
						chart: O.value,
						title: j.value.style.chart.title.text ? Ft.value : null,
						legend: j.value.style.chart.legend.show ? It.value : null,
						source: Lt.value,
						noTitle: Rt.value
					});
					requestAnimationFrame(() => {
						V.value.width = e, V.value.height = t - 12, Ht.value = !1;
					});
				});
				L.value && (R.value && L.value.unobserve(R.value), L.value.disconnect()), L.value = new ResizeObserver(e), R.value = O.value.parentNode, L.value.observe(R.value);
			}
			setTimeout(() => {
				w.value = !0;
			}, 600);
		}
		Ge(() => {
			L.value && (R.value && L.value.unobserve(R.value), L.value.disconnect());
		});
		let { isPrinting: rn, isImaging: an, generatePdf: on, generateImage: sn } = _e({
			elementId: `rings_${T.value}`,
			fileName: j.value.style.chart.title.text || "vue-ui-rings",
			options: j.value.userOptions.print
		}), cn = c(() => j.value.userOptions.show && !j.value.style.chart.title.text), ln = c(() => fe(j.value.customPalette)), B = y({
			showTable: j.value.table.show,
			showTooltip: j.value.style.chart.tooltip.show,
			showLabels: j.value.style.chart.layout.labels.dataLabels.show
		});
		$e(j, () => {
			B.value = {
				showTable: j.value.table.show,
				showTooltip: j.value.style.chart.tooltip.show,
				showLabels: j.value.style.chart.layout.labels.dataLabels.show
			};
		}, { immediate: !0 });
		let V = y({
			height: j.value.style.chart.size,
			width: j.value.style.chart.size
		}), un = c(() => Math.min(V.value.height, V.value.width)), dn = et, H = y([]);
		function fn() {
			H.value.length ? H.value = [] : G.value.forEach((e) => {
				H.value.push(e.uid);
			}), pn();
		}
		function pn() {
			dn("selectLegend", q.value.map((e) => ({
				name: e.name,
				color: e.color,
				value: e.value
			})));
		}
		function U(e) {
			if (H.value.includes(e)) H.value = H.value.filter((t) => t !== e);
			else {
				if (H.value.length === W.value.length - 1) return;
				H.value.push(e);
			}
			pn();
		}
		function mn(e) {
			return W.value.length ? W.value.find((t) => t.name === e) || (z.value && console.warn(`VueUiRings - Series name not found "${e}"`), null) : (z.value && console.warn("VueUiRings - There are no series to show."), null);
		}
		function hn(e) {
			let t = mn(e);
			t !== null && H.value.includes(t.uid) && U(t.uid);
		}
		function gn(e) {
			let t = mn(e);
			t !== null && (H.value.includes(t.uid) || U(t.uid));
		}
		let _n = c(() => Math.max(...W.value.filter((e) => !H.value.includes(e.uid)).map(({ value: e }) => e)));
		function vn(e) {
			return e / _n.value;
		}
		let W = c(() => Yt.value.map(({ values: e, name: t, color: n = null }, r) => {
			let i = te(e).reduce((e, t) => e + t, 0);
			return {
				name: t,
				color: n || ne(n) || ln.value[r] || o[r] || o[r % o.length],
				value: i,
				proportion: i / Yt.value.map((e) => (e.values || []).reduce((e, t) => e + t, 0)).reduce((e, t) => e + t, 0),
				uid: le(),
				absoluteIndex: r
			};
		})), G = c(() => W.value.map((e, t) => {
			let n = oe(j.value.style.chart.layout.labels.dataLabels.formatter, e.value, s({
				p: j.value.style.chart.layout.labels.dataLabels.prefix,
				v: e.value,
				s: j.value.style.chart.layout.labels.dataLabels.suffix,
				r: j.value.style.chart.legend.roundingValue
			}), {
				datapoint: e,
				index: t
			}), r = isNaN(e.value / K.value) ? "-" : s({
				v: e.value / K.value * 100,
				s: "%",
				r: j.value.style.chart.legend.roundingPercentage
			}), i = Cn({
				showVal: j.value.style.chart.legend.showValue,
				showPercentage: j.value.style.chart.legend.showPercentage,
				val: n,
				percentage: H.value.includes(e.uid) ? "-%" : r
			});
			return {
				...e,
				shape: "circle",
				opacity: H.value.includes(e.uid) ? .5 : 1,
				segregate: () => U(e.uid),
				isSegregated: H.value.includes(e.uid),
				display: `${e.name}${j.value.style.chart.legend.showPercentage || j.value.style.chart.legend.showValue ? ": " : ""}${i}`
			};
		}).toSorted((e, t) => t.value - e.value)), yn = c(() => ({
			cy: "rings-div-legend",
			backgroundColor: j.value.style.chart.legend.backgroundColor,
			color: j.value.style.chart.legend.color,
			fontSize: j.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: j.value.style.chart.legend.bold ? "bold" : ""
		})), K = c(() => W.value.filter((e) => !H.value.includes(e.uid)).map(({ value: e }) => e).reduce((e, t) => e + t, 0)), q = c(() => W.value.filter((e) => !H.value.includes(e.uid)).map(({ name: e, value: t, color: n = null, uid: r, absoluteIndex: i }, a) => ({
			absoluteIndex: i,
			uid: r,
			name: e,
			color: n || ne(n) || ln.value[a] || o[a] || o[a % o.length],
			value: t,
			proportion: vn(t),
			percentage: t / K.value * 100,
			strokeWidth: j.value.style.chart.layout.rings.strokeWidth * vn(t)
		})).toSorted((e, t) => t.value - e.value));
		function bn() {
			return q.value.map(({ name: e, color: t, value: n, absoluteValues: r, percentage: i }) => ({
				name: e,
				color: t,
				value: n,
				absoluteValues: r,
				percentage: i
			}));
		}
		let J = c(() => un.value - j.value.style.chart.layout.rings.strokeWidth * 2);
		function xn(e, t) {
			j.value.events.datapointClick && j.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Sn(e, t) {
			D.value = null, E.value = !1, A.value = null, Kt.value = "pointer", j.value.events.datapointLeave && j.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Cn({ val: e, percentage: t, showVal: n, showPercentage: r }) {
			let i = j.value.style.chart.layout.labels.dataLabels;
			return Fe({
				config: i,
				val: e,
				percentage: t,
				showVal: n,
				showPercentage: r
			});
		}
		function wn(e) {
			let t = j.value.style.chart.layout.labels.dataLabels, n = oe(t.formatter, e.value, s({
				p: t.prefix,
				v: e.value,
				s: t.suffix,
				r: t.roundingValue
			})), r = s({
				v: e.percentage,
				s: "%",
				r: t.roundingPercentage
			});
			return `${e.name}\n${Cn({
				val: n,
				percentage: r,
				showVal: t.showValue,
				showPercentage: t.showPercentage
			})}`;
		}
		function Tn(e) {
			return ie(J.value * e.proportion / 2 * .9 <= 0 ? 1e-4 : J.value * e.proportion / 2 * .9);
		}
		function En(e, t) {
			return t === 0 ? I.value.y : I.value.y + J.value * q.value[0].proportion / 2 - J.value * e.proportion / 2 - 2 * (t + 1);
		}
		function Y(e, t) {
			let n = j.value.style.chart.layout.labels.dataLabels.markers.position === "left" ? -(J.value / 2) : J.value / 2;
			return {
				x: I.value.x + n,
				y: En(e, t) - Tn(e)
			};
		}
		let Dn = y(null);
		function On(e, t, n = "pointer") {
			if (j.value.events.datapointEnter && j.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), H.value.length === C.dataset.length) return;
			Dn.value = {
				datapoint: e,
				seriesIndex: t,
				series: q.value,
				config: j.value
			}, D.value = t, A.value = t, Kt.value = n;
			let r = q.value[t], i = j.value.style.chart.tooltip.customFormat;
			if (pe(i) && ae(() => i({
				seriesIndex: t,
				datapoint: e,
				series: q.value,
				config: j.value
			}))) Nt.value = i({
				seriesIndex: t,
				datapoint: e,
				series: q.value,
				config: j.value
			});
			else {
				let n = "";
				n += `<div style="width:100%;text-align:center;border-bottom:1px solid ${j.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${r.name}</div>`, n += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 60 60" height="14" width="14"><circle cx="30" cy="30" r="30" stroke="none" fill="${r.color}" />${jt.pattern ? `<circle cx="30" cy="30" r="30" stroke="none" fill="url(#pattern_${T.value}_${e.absoluteIndex})" />` : ""}</svg>`, n += `<b>${Cn({
					showVal: j.value.style.chart.tooltip.showValue,
					showPercentage: j.value.style.chart.tooltip.showPercentage,
					val: `<span>${oe(j.value.style.chart.layout.labels.dataLabels.formatter, r.value, s({
						p: j.value.style.chart.layout.labels.dataLabels.prefix,
						v: r.value,
						s: j.value.style.chart.layout.labels.dataLabels.suffix,
						r: j.value.style.chart.tooltip.roundingValue
					}), {
						datapoint: e,
						seriesIndex: t
					})}</span>`,
					percentage: s({
						v: r.value / K.value * 100,
						s: "%",
						r: j.value.style.chart.tooltip.roundingPercentage
					})
				})}</b></div>`, Nt.value = n;
			}
			E.value = !0;
		}
		let X = c(() => ({
			head: q.value.map((e) => ({
				name: e.name,
				color: e.color
			})),
			body: q.value.map((e) => e.value)
		})), Z = c(() => {
			let e = [
				" <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>",
				s({
					p: j.value.style.chart.layout.labels.dataLabels.prefix,
					v: K.value,
					s: j.value.style.chart.layout.labels.dataLabels.suffix,
					r: j.value.table.td.roundingValue
				}),
				"100%"
			], t = X.value.head.map((e, t) => [
				{
					color: e.color,
					name: e.name
				},
				s({
					p: j.value.style.chart.layout.labels.dataLabels.prefix,
					v: X.value.body[t],
					s: j.value.style.chart.layout.labels.dataLabels.suffix,
					r: j.value.table.td.roundingValue
				}),
				isNaN(X.value.body[t] / K.value) ? "-" : (X.value.body[t] / K.value * 100).toFixed(j.value.table.td.roundingPercentage) + "%"
			]);
			return {
				head: e,
				body: t,
				a11yBody: t.map((e) => e.map((e, t) => t === 0 ? e.name : e)),
				config: {
					th: {
						backgroundColor: j.value.table.th.backgroundColor,
						color: j.value.table.th.color,
						outline: j.value.table.th.outline
					},
					td: {
						backgroundColor: j.value.table.td.backgroundColor,
						color: j.value.table.td.color,
						outline: j.value.table.td.outline
					},
					breakpoint: j.value.table.responsiveBreakpoint
				},
				colNames: [
					j.value.table.columnNames.series,
					j.value.table.columnNames.value,
					j.value.table.columnNames.percentage
				]
			};
		});
		function kn(e = null) {
			We(() => {
				let n = X.value.head.map((e, t) => [
					[e.name],
					[X.value.body[t]],
					[isNaN(X.value.body[t] / K.value) ? "-" : X.value.body[t] / K.value * 100]
				]), i = [
					[j.value.style.chart.title.text],
					[j.value.style.chart.title.subtitle.text],
					[
						[""],
						["val"],
						["%"]
					]
				].concat(n), a = r(i);
				e ? e(a) : t({
					csvContent: a,
					title: j.value.style.chart.title.text || "vue-ui-rings"
				});
			});
		}
		let Q = y(!1);
		function An(e) {
			Q.value = e, Pt.value += 1;
		}
		function jn() {
			B.value.showTable = !B.value.showTable;
		}
		function Mn() {
			B.value.showTooltip = !B.value.showTooltip;
		}
		function Nn() {
			B.value.showLabels = !B.value.showLabels;
		}
		let Pn = y(!1);
		function Fn() {
			Pn.value = !Pn.value;
		}
		async function In({ scale: e = 2 } = {}) {
			if (!O.value) return;
			let { width: t, height: n } = O.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await we({
				domElement: O.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: j.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let $ = c(() => {
			let e = j.value.table.useDialog && !j.value.table.show, t = B.value.showTable;
			return {
				component: e ? Dt : St,
				title: `${j.value.style.chart.title.text}${j.value.style.chart.title.subtitle.text ? `: ${j.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: j.value.table.th.backgroundColor,
					color: j.value.table.th.color,
					headerColor: j.value.table.th.color,
					headerBg: j.value.table.th.backgroundColor,
					isFullscreen: Q.value,
					fullscreenParent: O.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: N.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: j.value.style.chart.backgroundColor,
							color: j.value.style.chart.color
						},
						head: {
							backgroundColor: j.value.style.chart.backgroundColor,
							color: j.value.style.chart.color
						}
					}
				}
			};
		});
		$e(() => B.value.showTable, (e) => {
			j.value.table.show || (e && j.value.table.useDialog && k.value ? k.value.open() : "close" in k.value && k.value.close());
		});
		function Ln() {
			B.value.showTable = !1, Wt.value && Wt.value.setTableIconState(!1);
		}
		let Rn = c(() => G.value.map((e) => ({
			...e,
			name: e.display
		}))), zn = c(() => j.value.style.chart.backgroundColor), Bn = c(() => j.value.style.chart.legend), Vn = c(() => j.value.style.chart.title), { isCallbackImaging: Hn, isCallbackSvg: Un, generateSvg: Wn, onGenerateImage: Gn } = Se({
			svg: F,
			title: Vn,
			legend: Bn,
			legendItems: Rn,
			backgroundColor: zn,
			getSvgCallback: () => j.value.userOptions.callbacks.svg,
			generateImage: sn
		});
		async function Kn() {
			if (dn("copyAlt", {
				config: j.value,
				dataset: q.value
			}), !j.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(j.value.userOptions.callbacks.altCopy({
				config: j.value,
				dataset: q.value
			}));
		}
		function qn() {
			A.value = null, qt.value = !0;
		}
		function Jn() {
			A.value = null, Kt.value = "pointer", E.value = !1, D.value = null, qt.value = !1;
		}
		function Yn(e) {
			if (!F.value || Pn.value || document.activeElement !== F.value || !q.value.length || H.value.length === C.dataset.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				A.value = null, Kt.value = "pointer", E.value = !1, D.value = null;
				return;
			}
			if (r) {
				if (A.value === null) return;
				let e = q.value[A.value];
				if (!e) return;
				xn(e, A.value);
				return;
			}
			let a = A.value;
			a === null || a < 0 || a >= q.value.length ? a = n ? 0 : q.value.length - 1 : n ? (a += 1, a >= q.value.length && (a = 0)) : t && (--a, a < 0 && (a = q.value.length - 1));
			let ee = q.value[a];
			ee && (Xn(a), On(ee, a, "keyboard"));
		}
		function Xn(e) {
			if (!Number.isFinite(e) || !F.value) return;
			let t = q.value[e];
			if (!t) return;
			let n = Y(t, e), r = F.value.getBoundingClientRect();
			Gt.value = {
				x: r.left + n.x / V.value.width * r.width,
				y: r.top + n.y / V.value.height * r.height
			};
		}
		let Zn = c(() => ({
			headers: Z.value?.colNames ?? [],
			rows: Z.value?.a11yBody ?? []
		}));
		return De({
			getData: bn,
			getImage: In,
			generatePdf: on,
			generateCsv: kn,
			generateImage: sn,
			generateSvg: Wn,
			hideSeries: gn,
			showSeries: hn,
			toggleTable: jn,
			toggleTooltip: Mn,
			toggleAnnotator: Fn,
			toggleFullscreen: An,
			toggleLabels: Nn,
			copyAlt: Kn
		}), (e, t) => (v(), d("div", {
			ref_key: "ringsChart",
			ref: O,
			class: h(`vue-data-ui-component vue-ui-rings ${Q.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${j.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: _(`font-family:${j.value.style.fontFamily};text-align:center;width:100%;background:${j.value.style.chart.backgroundColor};${j.value.responsive ? "height: 100%" : ""}`),
			id: `rings_${T.value}`,
			onMouseleave: t[2] ||= (e) => {
				D.value = null, E.value = !1, x(Qt)(!1);
			},
			onMouseenter: t[3] ||= () => x(Qt)(!0)
		}, [
			f("div", {
				id: `chart-instructions-${T.value}`,
				class: "sr-only"
			}, [f("p", null, Xe(j.value.a11y.translations.keyboardNavigation), 1)], 8, nt),
			Zn.value?.rows?.length ? (v(), l(Me, {
				key: 0,
				uid: T.value,
				head: Zn.value.headers,
				body: Zn.value.rows,
				notice: j.value.a11y.translations.tableAvailable,
				caption: j.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : u("", !0),
			j.value.userOptions.buttons.annotator ? (v(), l(x(wt), {
				key: 1,
				svgRef: x(F),
				backgroundColor: j.value.style.chart.backgroundColor,
				color: j.value.style.chart.color,
				active: Pn.value,
				isCursorPointer: N.value,
				onClose: Fn
			}, {
				"annotator-action-close": S(() => [b(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": S(({ color: t }) => [b(e.$slots, "annotator-action-color", g(m({ color: t })), void 0, !0)]),
				"annotator-action-draw": S(({ mode: t }) => [b(e.$slots, "annotator-action-draw", g(m({ mode: t })), void 0, !0)]),
				"annotator-action-undo": S(({ disabled: t }) => [b(e.$slots, "annotator-action-undo", g(m({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": S(({ disabled: t }) => [b(e.$slots, "annotator-action-redo", g(m({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": S(({ disabled: t }) => [b(e.$slots, "annotator-action-delete", g(m({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : u("", !0),
			cn.value ? (v(), d("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Rt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : u("", !0),
			j.value.style.chart.title.text ? (v(), d("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Ft,
				style: "width:100%;background:transparent"
			}, [(v(), l(Te, {
				key: `title_${zt.value}`,
				config: {
					title: {
						cy: "rings-div-title",
						...j.value.style.chart.title
					},
					subtitle: {
						cy: "rings-div-subtitle",
						...j.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : u("", !0),
			f("div", { id: `legend-top-${T.value}` }, null, 8, rt),
			j.value.userOptions.show && Mt.value && (x($t) || x(Zt)) ? (v(), l(x(Tt), {
				ref_key: "userOptionsRef",
				ref: Wt,
				key: `user_options_${Pt.value}`,
				backgroundColor: j.value.style.chart.backgroundColor,
				color: j.value.style.chart.color,
				isPrinting: x(rn),
				isImaging: x(an),
				uid: T.value,
				hasTooltip: j.value.userOptions.buttons.tooltip && j.value.style.chart.tooltip.show,
				hasPdf: j.value.userOptions.buttons.pdf,
				hasXls: j.value.userOptions.buttons.csv,
				hasImg: j.value.userOptions.buttons.img,
				hasSvg: j.value.userOptions.buttons.svg,
				hasTable: j.value.userOptions.buttons.table,
				hasFullscreen: j.value.userOptions.buttons.fullscreen,
				hasAltCopy: j.value.userOptions.buttons.altCopy,
				hasLabel: j.value.userOptions.buttons.labels,
				isTooltip: B.value.showTooltip,
				isFullscreen: Q.value,
				titles: { ...j.value.userOptions.buttonTitles },
				chartElement: O.value,
				position: j.value.userOptions.position,
				hasAnnotator: j.value.userOptions.buttons.annotator,
				isAnnotation: Pn.value,
				callbacks: j.value.userOptions.callbacks,
				printScale: j.value.userOptions.print.scale,
				tableDialog: j.value.table.useDialog,
				isCursorPointer: N.value,
				onToggleFullscreen: An,
				onGeneratePdf: x(on),
				onGenerateCsv: kn,
				onGenerateImage: x(Gn),
				onGenerateSvg: x(Wn),
				onToggleTable: jn,
				onToggleTooltip: Mn,
				onToggleAnnotator: Fn,
				onToggleLabels: Nn,
				onCopyAlt: Kn,
				style: _({ visibility: x($t) ? x(Zt) ? "visible" : "hidden" : "visible" })
			}, Be({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: S(({ isOpen: t, color: n }) => [b(e.$slots, "menuIcon", g(m({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: S(() => [b(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: S(() => [b(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: S(() => [b(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: S(() => [b(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: S(() => [b(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: S(() => [b(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionLabels ? {
					name: "optionLabels",
					fn: S(() => [b(e.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: S(({ toggleFullscreen: t, isFullscreen: n }) => [b(e.$slots, "optionFullscreen", g(m({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: S(({ toggleAnnotator: t, isAnnotator: n }) => [b(e.$slots, "optionAnnotator", g(m({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: S(({ altCopy: t }) => [b(e.$slots, "optionAltCopy", g(m({ altCopy: t })), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: S(() => [b(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: S(() => [b(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasFullscreen.hasAltCopy.hasLabel.isTooltip.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : u("", !0),
			f("div", it, [(v(), d("svg", {
				ref_key: "svgRef",
				ref: F,
				xmlns: x(ue),
				class: h({
					"vue-data-ui-fullscreen--on": Q.value,
					"vue-data-ui-fulscreen--off": !Q.value,
					resizing: Ht.value || x(P),
					"vue-data-ui-no-transition": !x(M)
				}),
				viewBox: `0 0 ${V.value.width <= 0 ? 10 : V.value.width} ${V.value.height <= 0 ? 10 : V.value.height}`,
				style: _(`max-width:100%;overflow:hidden;background:transparent;color:${j.value.style.chart.color}`),
				"aria-describedby": `chart-instructions-${T.value}`,
				tabindex: "0",
				onFocus: qn,
				onBlur: Jn,
				onKeydown: Yn
			}, [
				He(x(Et)),
				e.$slots["chart-background"] ? (v(), d("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: V.value.width <= 0 ? 10 : V.value.width,
					height: V.value.height <= 0 ? 10 : V.value.height,
					style: { pointerEvents: "none" }
				}, [b(e.$slots, "chart-background", {}, void 0, !0)], 8, ot)) : u("", !0),
				f("defs", null, [(v(!0), d(Re, null, qe(q.value, (e, t) => (v(), l(Ae, {
					t: "radial",
					cx: "50%",
					cy: "30%",
					r: "50%",
					fx: "50%",
					fy: "50%",
					id: `gradient_${T.value}_${t}`,
					key: `gradient_${T.value}_${t}`,
					stops: [[
						"0%",
						x(n)(x(re)(e.color, .05), 100 - j.value.style.chart.layout.rings.gradient.intensity),
						1
					], [
						"100%",
						e.color,
						1
					]]
				}, null, 8, ["id", "stops"]))), 128))]),
				e.$slots.pattern ? (v(), d("g", st, [(v(!0), d(Re, null, qe(q.value, (t) => (v(), d("defs", null, [b(e.$slots, "pattern", Ue({ ref_for: !0 }, {
					seriesIndex: t.absoluteIndex,
					patternId: `pattern_${T.value}_${t.absoluteIndex}`
				}), void 0, !0)]))), 256))])) : u("", !0),
				(v(!0), d(Re, null, qe(q.value, (t, n) => (v(), d("g", { key: `r_${t.uid}` }, [
					f("circle", {
						class: h({
							"vue-data-ui-transition": w.value && x(M),
							"vue-rings-item-onload": !w.value && j.value.useCssAnimation && !x(P),
							"vue-ui-rings-opacity": D.value !== null && D.value !== n
						}),
						style: _(`animation-delay:${n * 100}ms`),
						stroke: j.value.style.chart.layout.rings.stroke,
						cx: I.value.x,
						cy: En(t, n),
						r: Tn(t),
						fill: j.value.style.chart.layout.rings.gradient.underlayerColor
					}, null, 14, ct),
					f("circle", {
						class: h({
							"vue-data-ui-transition": w.value && x(M),
							"vue-rings-item-onload": !w.value && j.value.useCssAnimation && !x(P),
							"vue-ui-rings-shadow": j.value.style.chart.layout.rings.useShadow,
							"vue-ui-rings-blur": D.value !== null && D.value !== n
						}),
						style: _(`animation-delay:${n * 100}ms`),
						stroke: j.value.style.chart.layout.rings.stroke,
						"stroke-width": t.strokeWidth < .5 ? .5 : t.strokeWidth,
						cx: I.value.x,
						cy: En(t, n),
						r: Tn(t),
						fill: j.value.style.chart.layout.rings.gradient.show ? `url(#gradient_${T.value}_${n})` : t.color
					}, null, 14, lt),
					e.$slots.pattern ? (v(), d("circle", {
						key: 0,
						class: h({
							"vue-data-ui-transition": w.value && x(M),
							"vue-rings-item-onload": !w.value && j.value.useCssAnimation && !x(P),
							"vue-ui-rings-shadow": j.value.style.chart.layout.rings.useShadow,
							"vue-ui-rings-blur": D.value !== null && D.value !== n
						}),
						style: _(`animation-delay:${n * 100}ms`),
						stroke: j.value.style.chart.layout.rings.stroke,
						"stroke-width": t.strokeWidth < .5 ? .5 : t.strokeWidth,
						cx: I.value.x,
						cy: En(t, n),
						r: Tn(t),
						fill: `url(#pattern_${T.value}_${t.absoluteIndex})`
					}, null, 14, ut)) : u("", !0),
					f("circle", {
						stroke: "none",
						cx: I.value.x,
						cy: En(t, n),
						r: Tn(t),
						fill: "transparent",
						onMouseenter: (e) => On(t, n, "pointer"),
						onMouseleave: (e) => Sn(t, n),
						onClick: (e) => xn(t, n)
					}, null, 40, dt),
					B.value.showLabels ? (v(), d(Re, { key: 1 }, [
						f("rect", {
							x: j.value.style.chart.layout.labels.dataLabels.markers.position === "left" ? Y(t, n).x : I.value.x,
							y: Y(t, n).y - j.value.style.chart.layout.labels.dataLabels.markers.strokeWidth / 2,
							width: Math.abs(I.value.x - Y(t, n).x),
							height: j.value.style.chart.layout.labels.dataLabels.markers.strokeWidth,
							fill: j.value.style.chart.layout.labels.dataLabels.markers.stroke,
							rx: j.value.style.chart.layout.labels.dataLabels.markers.strokeWidth,
							class: h({
								"vue-data-ui-transition": w.value && x(M),
								"vue-rings-item-onload": !w.value && j.value.useCssAnimation && !x(P),
								"vue-ui-rings-shadow": j.value.style.chart.layout.rings.useShadow,
								"vue-ui-rings-blur": D.value !== null && D.value !== n
							})
						}, null, 10, ft),
						f("circle", {
							cx: Y(t, n).x,
							cy: Y(t, n).y,
							r: j.value.style.chart.layout.labels.dataLabels.markers.radius,
							fill: t.color,
							stroke: j.value.style.chart.backgroundColor,
							class: h({
								"vue-data-ui-transition": w.value && x(M),
								"vue-rings-item-onload": !w.value && j.value.useCssAnimation && !x(P),
								"vue-ui-rings-shadow": j.value.style.chart.layout.rings.useShadow,
								"vue-ui-rings-blur": D.value !== null && D.value !== n
							})
						}, null, 10, pt),
						f("text", {
							"text-anchor": j.value.style.chart.layout.labels.dataLabels.markers.position === "left" ? "end" : "start",
							"font-size": j.value.style.chart.layout.labels.dataLabels.fontSize,
							fill: j.value.style.chart.layout.labels.dataLabels.color,
							"font-weight": j.value.style.chart.layout.labels.dataLabels.bold ? "bold" : "normal",
							class: h({
								"vue-data-ui-transition": w.value && x(M),
								"vue-rings-item-onload": !w.value && j.value.useCssAnimation && !x(P),
								"vue-ui-rings-shadow": j.value.style.chart.layout.rings.useShadow,
								"vue-ui-rings-blur": D.value !== null && D.value !== n
							}),
							transform: `translate(${Y(t, n).x + (j.value.style.chart.layout.labels.dataLabels.markers.position === "left" ? -j.value.style.chart.layout.labels.dataLabels.offsetX - 6 : j.value.style.chart.layout.labels.dataLabels.offsetX) + 6}, ${Y(t, n).y + j.value.style.chart.layout.labels.dataLabels.fontSize / 3})`,
							onMouseenter: (e) => On(t, n, "pointer"),
							onMouseleave: (e) => Sn(t, n),
							onClick: (e) => xn(t, n),
							innerHTML: x(i)({
								content: wn(t),
								fontSize: j.value.style.chart.layout.labels.dataLabels.fontSize,
								fill: j.value.style.chart.layout.labels.dataLabels.color,
								x: 0,
								y: 0,
								translateY: !0
							})
						}, null, 42, mt)
					], 64)) : u("", !0)
				]))), 128)),
				b(e.$slots, "svg", { svg: {
					...V.value,
					isPrintingImg: x(rn) || x(an) || x(Hn),
					isPrintingSvg: x(Un)
				} }, void 0, !0)
			], 46, at)), e.$slots.hint ? (v(), d("div", ht, [b(e.$slots, "hint", g(m({
				hint: j.value.a11y.translations.keyboardNavigation,
				isVisible: qt.value
			})), void 0, !0)])) : u("", !0)]),
			e.$slots.watermark ? (v(), d("div", gt, [b(e.$slots, "watermark", g(m({ isPrinting: x(rn) || x(an) || x(Hn) || x(Un) })), void 0, !0)])) : u("", !0),
			f("div", { id: `legend-bottom-${T.value}` }, null, 8, _t),
			Ut.value && (j.value.style.chart.legend.show || e.$slots.legend) ? (v(), l(ze, {
				key: 6,
				to: j.value.style.chart.legend.position === "top" ? `#legend-top-${T.value}` : `#legend-bottom-${T.value}`
			}, [f("div", {
				ref_key: "chartLegend",
				ref: It
			}, [b(e.$slots, "legend", { legend: G.value }, () => [j.value.style.chart.legend.show ? (v(), l(Ie, {
				key: `legend_${Vt.value}`,
				legendSet: G.value,
				config: yn.value,
				isCursorPointer: N.value,
				onClickMarker: t[0] ||= ({ legend: e }) => U(e.uid)
			}, Be({
				item: S(({ legend: e, index: t }) => [x(P) ? u("", !0) : (v(), d("div", {
					key: 0,
					onClick: (t) => U(e.uid),
					style: _(`opacity:${H.value.includes(e.uid) ? .5 : 1}`)
				}, Xe(e.display), 13, vt))]),
				legendToggle: S(() => [G.value.length > 2 && j.value.style.chart.legend.selectAllToggle.show && !x(P) ? (v(), l(je, {
					key: 0,
					backgroundColor: j.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: j.value.style.chart.legend.selectAllToggle.color,
					fontSize: j.value.style.chart.legend.fontSize,
					checked: H.value.length > 0,
					isCursorPointer: N.value,
					onToggle: fn
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : u("", !0)]),
				_: 2
			}, [e.$slots.pattern ? {
				name: "legend-pattern",
				fn: S(({ legend: e, index: t }) => [He(Ee, {
					shape: e.shape,
					radius: 30,
					stroke: "none",
					plot: {
						x: 30,
						y: 30
					},
					fill: `url(#pattern_${T.value}_${t})`
				}, null, 8, ["shape", "fill"])]),
				key: "0"
			} : void 0]), 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : u("", !0)], !0)], 512)], 8, ["to"])) : u("", !0),
			e.$slots.source ? (v(), d("div", {
				key: 7,
				ref_key: "source",
				ref: Lt,
				dir: "auto"
			}, [b(e.$slots, "source", {}, void 0, !0)], 512)) : u("", !0),
			He(x(bt), {
				teleportTo: j.value.style.chart.tooltip.teleportTo,
				show: B.value.showTooltip && E.value && H.value.length < C.dataset.length,
				backgroundColor: j.value.style.chart.tooltip.backgroundColor,
				color: j.value.style.chart.tooltip.color,
				borderRadius: j.value.style.chart.tooltip.borderRadius,
				borderColor: j.value.style.chart.tooltip.borderColor,
				borderWidth: j.value.style.chart.tooltip.borderWidth,
				fontSize: j.value.style.chart.tooltip.fontSize,
				backgroundOpacity: j.value.style.chart.tooltip.backgroundOpacity,
				position: j.value.style.chart.tooltip.position,
				offsetX: j.value.style.chart.tooltip.offsetX,
				offsetY: j.value.style.chart.tooltip.offsetY,
				parent: O.value,
				content: Nt.value,
				isFullscreen: Q.value,
				isCustom: j.value.style.chart.tooltip.customFormat && typeof j.value.style.chart.tooltip.customFormat == "function",
				smooth: j.value.style.chart.tooltip.smooth,
				backdropFilter: j.value.style.chart.tooltip.backdropFilter,
				smoothForce: j.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: j.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: Kt.value === "keyboard",
				a11yPosition: Gt.value
			}, {
				"tooltip-before": S(() => [b(e.$slots, "tooltip-before", g(m({ ...Dn.value })), void 0, !0)]),
				tooltip: S(() => [b(e.$slots, "tooltip", g(m({ ...Dn.value })), void 0, !0)]),
				"tooltip-after": S(() => [b(e.$slots, "tooltip-after", g(m({ ...Dn.value })), void 0, !0)]),
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
			Mt.value && j.value.userOptions.buttons.table ? (v(), l(Je($.value.component), Ue({ key: 8 }, $.value.props, {
				ref_key: "tableUnit",
				ref: k,
				onClose: Ln
			}), Be({
				content: S(() => [(v(), l(x(Ct), {
					key: `table_${Bt.value}`,
					colNames: Z.value.colNames,
					head: Z.value.head,
					body: Z.value.body,
					config: Z.value.config,
					title: j.value.table.useDialog ? "" : $.value.title,
					withCloseButton: !j.value.table.useDialog,
					isCursorPointer: N.value,
					onClose: Ln
				}, {
					th: S(({ th: e }) => [f("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, yt)]),
					td: S(({ td: e }) => [Ve(Xe(e.name || e), 1)]),
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
			}, [j.value.table.useDialog ? {
				name: "title",
				fn: S(() => [Ve(Xe($.value.title), 1)]),
				key: "0"
			} : void 0, j.value.table.useDialog ? {
				name: "actions",
				fn: S(() => [f("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => kn(j.value.userOptions.callbacks.csv),
					style: _({ cursor: N.value ? "pointer" : "default" })
				}, [He(x(xt), {
					name: "fileCsv",
					stroke: $.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : u("", !0),
			b(e.$slots, "skeleton", {}, () => [x(P) ? (v(), l(ye, { key: 0 })) : u("", !0)], !0)
		], 46, tt));
	}
}, [["__scopeId", "data-v-93e9c460"]]);
//#endregion
export { et as n, bt as t };
