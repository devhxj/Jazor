import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Jt as i, Kt as a, Pt as o, S as ee, Vt as te, X as s, ct as ne, i as re, jt as ie, n as ae, pt as oe, q as se, r as ce, t as le, tt as ue, w as de, xt as fe } from "./lib-Bttd6u5E.js";
import { n as pe, t as me } from "./useHints-Dq_w2E8B.js";
import { t as he } from "./useConfig-DlNpz6P8.js";
import { t as ge } from "./usePrinter-DN5bYhTG.js";
import { n as _e, t as ve } from "./BaseScanner-DZvpgOjM.js";
import { t as ye } from "./useNestedProp-vPNvh7rV.js";
import { t as be } from "./useThemeCheck-C43Tcqmk.js";
import { t as xe } from "./useChartExport-DNiwdPmb.js";
import { t as Se } from "./img-Bnokohej.js";
import { n as Ce } from "./Title-BE3qg9xl.js";
import { t as we } from "./Shape-C21CMlWS.js";
import { t as Te } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Ee, t as De } from "./useResponsive-ZtArZtUf.js";
import { t as Oe } from "./DefGrad-DVBqDjhO.js";
import { t as ke } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Ae } from "./A11yDataTable-DdRsVULz.js";
import { t as je } from "./useUserOptionState-DK-_1ddE.js";
import { t as Me } from "./useChartAccessibility-DYqac8yF.js";
import { t as Ne } from "./labelUtils-BeVpDvTJ.js";
import { t as Pe } from "./Legend-CQxUgOd-.js";
import { t as Fe } from "./usePrefersMotion-BC-CsqR1.js";
import { t as Ie } from "./vue_ui_waffle-DIARFc7g.js";
import { Fragment as c, Teleport as Le, computed as l, createBlock as u, createCommentVNode as d, createElementBlock as f, createElementVNode as p, createSlots as Re, createTextVNode as ze, createVNode as Be, defineAsyncComponent as m, guardReactiveProps as h, mergeProps as Ve, nextTick as He, normalizeClass as Ue, normalizeProps as g, normalizeStyle as We, onBeforeUnmount as Ge, onMounted as Ke, openBlock as _, ref as v, renderList as y, renderSlot as b, resolveDynamicComponent as qe, shallowRef as Je, toDisplayString as Ye, toRefs as Xe, unref as x, useSlots as Ze, watch as S, withCtx as C } from "vue";
//#region src/components/vue-ui-waffle.vue
var Qe = /* @__PURE__ */ e({ default: () => yt }), $e = ["id"], et = ["id"], tt = ["id"], nt = { style: { position: "relative" } }, rt = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], it = ["id"], at = [
	"x",
	"y",
	"height",
	"width"
], ot = ["height", "width"], st = { key: 0 }, ct = [
	"rx",
	"x",
	"y",
	"height",
	"width",
	"stroke",
	"stroke-width",
	"filter"
], lt = [
	"rx",
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], ut = { key: 1 }, dt = [
	"rx",
	"x",
	"y",
	"height",
	"width",
	"fill",
	"filter"
], ft = [
	"textContent",
	"x",
	"y",
	"font-size",
	"fill",
	"filter"
], pt = [
	"x",
	"y",
	"height",
	"width",
	"onMouseover",
	"onMouseleave",
	"onClick"
], mt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, ht = {
	key: 5,
	class: "vue-data-ui-watermark"
}, gt = ["id"], _t = ["onClick"], vt = ["innerHTML"], yt = /*#__PURE__*/ Te({
	__name: "vue-ui-waffle",
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
	setup(e, { expose: Te, emit: Qe }) {
		let yt = m(() => import("./Tooltip-DhjyfHwz.js")), bt = m(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), xt = m(() => import("./DataTable-BbKgJ5UI.js")), St = m(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Ct = m(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), wt = m(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Tt = m(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Et = m(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_waffle: Dt } = he(), { isThemeValid: Ot, warnInvalidTheme: kt } = be(), At = Fe(), w = e, jt = Ze(), Mt = l({
			get() {
				return !!w.dataset && w.dataset.length;
			},
			set(e) {
				return e;
			}
		}), T = v(se()), Nt = v(!1), Pt = v(""), E = v(null), Ft = v(0), D = v(null), It = v(null), Lt = v(null), Rt = v(null), zt = v(null), Bt = v(0), Vt = v(0), Ht = v(0), Ut = v(!1), Wt = v(null), Gt = v(null), O = v(null), Kt = v({
			x: 0,
			y: 0
		}), qt = v("pointer"), Jt = v(!1), k = v(nn());
		pe({
			config: () => k.value,
			dataset: () => w.dataset,
			component: "VueUiWaffle",
			rules: [
				me.singleSeries,
				me.emptyArray,
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
		let A = l(() => k.value.userOptions.useCursorPointer), Yt = l(() => i({
			defaultConfig: {
				useCustomCells: !1,
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						labels: { captions: { show: !1 } },
						rect: { stroke: "#999999" }
					},
					legend: {
						backgroundColor: "transparent",
						showValue: !1,
						showPercentage: !1
					}
				} }
			},
			userConfig: k.value.skeletonConfig ?? {}
		})), { loading: Xt, FINAL_DATASET: Zt, manualLoading: Qt } = _e({
			...Xe(w),
			FINAL_CONFIG: k,
			prepareConfig: nn,
			callback: () => {
				Promise.resolve().then(async () => {
					z.value = gn();
				});
			},
			skeletonDataset: w.config?.skeletonDataset ?? [
				{
					name: "",
					values: [1],
					color: "#AAAAAA"
				},
				{
					name: "",
					values: [1],
					color: "#BABABA"
				},
				{
					name: "",
					values: [1],
					color: "#CACACA"
				}
			],
			skeletonConfig: i({
				defaultConfig: k.value,
				userConfig: Yt.value
			})
		}), { userOptionsVisible: $t, setUserOptionsVisibility: en, keepUserOptionState: tn } = je({ config: k.value }), { svgRef: j } = Me({ config: k.value.style.chart.title });
		function nn() {
			let e = ye({
				userConfig: w.config,
				defaultConfig: Dt
			}), t = e.theme;
			if (!t) return e;
			if (!Ot.value(e)) return kt(e), e;
			let n = ye({
				userConfig: Ie[t] || w.config,
				defaultConfig: e
			}), r = ye({
				userConfig: w.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : a[t] || o
			};
		}
		S(() => w.config, (e) => {
			Xt.value || (k.value = nn()), $t.value = !k.value.userOptions.showOnChartHover, an(), Bt.value += 1, Vt.value += 1, Ht.value += 1, P.value.showTable = k.value.table.show, P.value.showTooltip = k.value.style.chart.tooltip.show;
		}, { deep: !0 });
		let M = Je(null), N = Je(null), rn = l(() => k.value.debug);
		function an() {
			if (ie(w.dataset) ? (ue({
				componentName: "VueUiWaffle",
				type: "dataset",
				debug: rn.value
			}), Mt.value = !1, Qt.value = !0) : rn.value && w.dataset.forEach((e, t) => {
				oe({
					datasetObject: e,
					requiredAttributes: ["name", "values"]
				}).forEach((e) => {
					ue({
						componentName: "VueUiWaffle",
						type: "datasetSerieAttribute",
						property: e,
						index: t
					});
				});
			}), ie(w.dataset) || (Qt.value = k.value.loading), k.value.responsive) {
				let e = Ee(() => {
					let { width: e, height: t } = De({
						chart: D.value,
						title: k.value.style.chart.title.text ? It.value : null,
						legend: k.value.style.chart.legend.show ? Lt.value : null,
						source: Rt.value,
						noTitle: zt.value
					});
					requestAnimationFrame(() => {
						F.value.width = e, F.value.height = t, I.value.width = e, I.value.height = t;
					});
				});
				M.value && (N.value && M.value.unobserve(N.value), M.value.disconnect()), M.value = new ResizeObserver(e), N.value = D.value.parentNode, M.value.observe(N.value);
			}
		}
		Ke(() => {
			Ut.value = !0, an();
		}), Ge(() => {
			M.value && (N.value && M.value.unobserve(N.value), M.value.disconnect());
		});
		let { isPrinting: on, isImaging: sn, generatePdf: cn, generateImage: ln } = ge({
			elementId: `vue-ui-waffle_${T.value}`,
			fileName: k.value.style.chart.title.text || "vue-ui-waffle",
			options: k.value.userOptions.print
		}), un = l(() => k.value.userOptions.show && !k.value.style.chart.title.text), dn = l(() => de(k.value.customPalette)), P = v({
			showTable: k.value.table.show,
			showTooltip: k.value.style.chart.tooltip.show
		});
		S(k, () => {
			P.value = {
				showTable: k.value.table.show,
				showTooltip: k.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let F = v({
			height: 512,
			width: 512
		}), I = v({
			top: 0,
			left: 0,
			height: 512,
			width: 512
		}), L = l(() => (I.value.width - k.value.style.chart.layout.grid.size * k.value.style.chart.layout.grid.spaceBetween) / k.value.style.chart.layout.grid.size), R = l(() => (I.value.height - k.value.style.chart.layout.grid.size * k.value.style.chart.layout.grid.spaceBetween) / k.value.style.chart.layout.grid.size), fn = l(() => Math.max(1e-4, I.value.width / k.value.style.chart.layout.grid.size)), pn = l(() => Math.max(1e-4, I.value.height / k.value.style.chart.layout.grid.size));
		function mn(e) {
			let t = k.value.style.chart.layout.grid.size * k.value.style.chart.layout.grid.size, n = e.reduce((e, t) => e + t, 0), r = e.map((e) => e / n * t), i = r.map(Math.floor), a = r.map((e) => e % 1), o = t - i.reduce((e, t) => e + t, 0);
			for (; o > 0;) {
				let e = a.indexOf(Math.max(...a));
				i[e] += 1, a[e] = 0, --o;
			}
			return i;
		}
		let hn = v(!1);
		function gn() {
			return hn.value = Zt.value.flatMap((e) => e.values.reduce((e, t) => e + t, 0)).reduce((e, t) => e + t, 0) === 0, Zt.value.map((e, t) => ({
				...e,
				color: ee(e.color) || dn.value[t] || o[t] || o[t % o.length],
				uid: `serie_${t}`,
				absoluteIndex: t
			}));
		}
		let _n = l(() => gn()), z = v(_n.value);
		S(() => w.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Qt.value = !1);
		}, { immediate: !0 }), S(() => w.dataset, (e) => {
			z.value = gn();
		}, { deep: !0 });
		let vn = l(() => mn(z.value.filter((e, t) => !U.value.includes(e.uid)).map((e, t) => hn.value ? 1 : (e.values || []).reduce((e, t) => e + t, 0)))), yn = l(() => mn(z.value.map((e, t) => hn.value ? 1 : (e.values || []).reduce((e, t) => e + t)))), B = l(() => (rn.value && Zt.value.forEach((e, t) => {
			[null, void 0].includes(e.values) && ue({
				componentName: "VueUiWaffle",
				type: "datasetSerieAttribute",
				property: "values (number[])",
				index: t
			});
		}), z.value.filter((e, t) => !U.value.includes(e.uid)).map((e, t) => ({
			absoluteIndex: e.absoluteIndex,
			uid: e.uid,
			name: e.name,
			color: e.color,
			value: (e.values || []).reduce((e, t) => e + t, 0),
			absoluteValues: e.values || [],
			proportion: vn.value[t]
		})))), bn = l(() => z.value.map((e, t) => ({
			absoluteIndex: e.absoluteIndex,
			uid: e.uid,
			name: e.name,
			color: e.color,
			value: (e.values || []).reduce((e, t) => e + t, 0),
			absoluteValues: e.values || [],
			proportion: yn.value[t]
		})));
		function xn() {
			return bn.value.map((e) => ({
				name: e.name,
				color: e.color,
				value: e.value,
				proportion: e.proportion
			}));
		}
		let Sn = l(() => {
			let e = 0;
			return B.value.map((t, n) => {
				let r = e, i = r + t.proportion, a = [];
				for (let e = Math.floor(r); e < Math.floor(i); e += 1) a.push(e);
				return e = i, {
					...t,
					start: r,
					rects: a
				};
			});
		}), V = l(() => Sn.value.flatMap((e, t) => e.rects.map((n, r) => ({
			isFirst: r === 0,
			isLongEnough: n.length > 2,
			name: e.name,
			color: e.color,
			value: e.value,
			serieIndex: t,
			absoluteStartIndex: r < 3,
			serieId: e.uid,
			...e
		}))).map((e, t) => ({
			...e,
			isAbsoluteFirst: t % k.value.style.chart.layout.grid.size === 0
		}))), H = l(() => {
			let e = [];
			for (let t = 0; t < k.value.style.chart.layout.grid.size; t += 1) for (let n = 0; n < k.value.style.chart.layout.grid.size; n += 1) e.push({
				isStartOfLine: n === 0,
				position: k.value.style.chart.layout.grid.vertical ? t : n,
				x: (k.value.style.chart.layout.grid.vertical ? t : n) * (L.value + k.value.style.chart.layout.grid.spaceBetween),
				y: (k.value.style.chart.layout.grid.vertical ? n : t) * (R.value + k.value.style.chart.layout.grid.spaceBetween) + I.value.top
			});
			return e;
		}), U = v([]), Cn = v(!1), wn = v(Object.create(null)), Tn = v(Object.create(null)), W = v(0);
		function G(e, t) {
			z.value = z.value.map((n) => n.uid === e ? {
				...n,
				values: [t]
			} : n);
		}
		function En(e, t) {
			let n = t.find((t) => t.uid === e);
			return n ? (n.values || []).reduce((e, t) => e + t, 0) : 0;
		}
		function Dn(e) {
			let t = !1;
			wn.value[e] && (cancelAnimationFrame(wn.value[e]), delete wn.value[e], t = !0), Tn.value[e] && (cancelAnimationFrame(Tn.value[e]), delete Tn.value[e], t = !0), t && (W.value = Math.max(0, W.value - 1), Cn.value = W.value > 0);
		}
		function On({ seriesId: e, fromValue: t, toValue: n, mode: r }) {
			Dn(e), W.value += 1, Cn.value = !0;
			let i = t;
			return new Promise((a) => {
				function o() {
					if (r === "increase") {
						if (i >= n) {
							G(e, n), --W.value, W.value <= 0 && (Cn.value = !1), delete wn.value[e], a();
							return;
						}
						i += n * .025, i > n && (i = n), G(e, i), wn.value[e] = requestAnimationFrame(o);
					} else {
						if (i <= t / 100) {
							G(e, 0), --W.value, W.value <= 0 && (Cn.value = !1), delete Tn.value[e], a();
							return;
						}
						i /= 1.15, G(e, i), Tn.value[e] = requestAnimationFrame(o);
					}
				}
				o();
			});
		}
		function kn() {
			z.value = _n.value;
			let e = U.value.length > 0;
			if (!k.value.useAnimation) {
				e ? U.value = [] : U.value = q.value.map((e) => e.uid);
				return;
			}
			e ? [...U.value].forEach((e) => {
				K(e, !0);
			}) : q.value.map((e) => e.uid).forEach((e) => {
				K(e, !0);
			});
		}
		function K(e, t = !1) {
			if (hn.value && !t) return;
			let n = t || U.value.length < q.value.length - 1 && q.value.length > 1;
			if (!k.value.useAnimation) {
				U.value.includes(e) ? U.value = U.value.filter((t) => t !== e) : n && U.value.push(e);
				return;
			}
			let r = En(e, _n.value), i = En(e, z.value);
			if (i !== 0 || r !== 0) {
				if (U.value.includes(e)) {
					if (U.value = U.value.filter((t) => t !== e), At.value) {
						Dn(e), G(e, r), Hn("selectLegend", B.value.map((e) => ({
							name: e.name,
							color: e.color,
							value: e.value,
							proportion: e.proportion / k.value.style.chart.layout.grid.size ** 2
						})));
						return;
					}
					On({
						seriesId: e,
						fromValue: i,
						toValue: r,
						mode: "increase"
					}).then(() => {
						Hn("selectLegend", B.value.map((e) => ({
							name: e.name,
							color: e.color,
							value: e.value,
							proportion: e.proportion / k.value.style.chart.layout.grid.size ** 2
						})));
					});
					return;
				}
				if (n) {
					if (At.value) {
						Dn(e), U.value.includes(e) || U.value.push(e), G(e, 0), Hn("selectLegend", B.value.map((e) => ({
							name: e.name,
							color: e.color,
							value: e.value,
							proportion: e.proportion / k.value.style.chart.layout.grid.size ** 2
						})));
						return;
					}
					On({
						seriesId: e,
						fromValue: i,
						toValue: 0,
						mode: "decrease"
					}).then(() => {
						U.value.includes(e) || U.value.push(e), G(e, 0), Hn("selectLegend", B.value.map((e) => ({
							name: e.name,
							color: e.color,
							value: e.value,
							proportion: e.proportion / k.value.style.chart.layout.grid.size ** 2
						})));
					});
				}
			}
		}
		function An(e) {
			return bn.value.length ? bn.value.find((t) => t.name === e) || (rn.value && console.warn(`VueUiWaffle - Series name not found "${e}"`), null) : (rn.value && console.warn("VueUiWaffle - There are no series to show."), null);
		}
		function jn(e) {
			let t = An(e);
			t !== null && U.value.includes(t.uid) && K(t.uid);
		}
		function Mn(e) {
			let t = An(e);
			t !== null && (U.value.includes(t.uid) || K(t.uid));
		}
		function Nn({ val: e, percentage: t, showVal: n, showPercentage: r, config: i }) {
			return Ne({
				config: i,
				val: e,
				percentage: t,
				showVal: n,
				showPercentage: r
			});
		}
		let q = l(() => z.value.map((e, t) => {
			let n = (e.values || []).reduce((e, t) => e + t, 0), r = n / z.value.map((e) => (e.values || []).reduce((e, t) => e + t, 0)).reduce((e, t) => e + t, 0);
			return {
				name: e.name,
				color: e.color || dn[t] || o[t] || o[t % o.length],
				value: n,
				proportion: r,
				uid: e.uid,
				shape: "square"
			};
		}).map((e, t) => {
			let n = Nn({
				val: re(k.value.style.chart.layout.labels.dataLabels.formatter, e.value, s({
					p: k.value.style.chart.layout.labels.dataLabels.prefix,
					v: e.value,
					s: k.value.style.chart.layout.labels.dataLabels.suffix,
					r: k.value.style.chart.legend.roundingValue
				}), {
					datapoint: e,
					index: t
				}),
				percentage: isNaN(e.value / J.value) ? "-" : s({
					v: e.value / J.value * 100,
					s: "%",
					r: k.value.style.chart.legend.roundingPercentage
				}),
				showVal: k.value.style.chart.legend.showValue,
				showPercentage: k.value.style.chart.legend.showPercentage,
				config: k.value.style.chart.legend
			});
			return {
				...e,
				opacity: U.value.includes(e.uid) ? .5 : 1,
				segregate: () => K(e.uid),
				isSegregated: U.value.includes(e.uid),
				display: `${e.name}${k.value.style.chart.legend.showPercentage || k.value.style.chart.legend.showValue ? ": " : ""}${n}`
			};
		})), Pn = l(() => ({
			cy: "waffle-div-legend",
			backgroundColor: k.value.style.chart.legend.backgroundColor,
			color: k.value.style.chart.legend.color,
			fontSize: k.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: k.value.style.chart.legend.bold ? "bold" : ""
		})), J = l(() => B.value.map((e) => e.value).reduce((e, t) => e + t, 0)), Fn = v(null), In = v(null), Y = l(() => {
			let e = /* @__PURE__ */ new Map();
			return V.value.forEach((t, n) => {
				e.has(t.serieIndex) ? e.get(t.serieIndex).rectIndexes.push(n) : e.set(t.serieIndex, {
					serieIndex: t.serieIndex,
					absoluteIndex: t.absoluteIndex,
					serieId: t.serieId,
					name: t.name,
					color: t.color,
					value: t.value,
					proportion: t.proportion,
					rectIndexes: [n]
				});
			}), Array.from(e.values()).sort((e, t) => e.serieIndex - t.serieIndex);
		});
		function Ln(e) {
			return Y.value.find((t) => t.serieIndex === e) || null;
		}
		function Rn(e) {
			let t = Ln(e);
			return !t || !t.rectIndexes.length ? null : t.rectIndexes[0];
		}
		function zn(e) {
			let t = V.value[e];
			k.value.events.datapointClick && k.value.events.datapointClick({
				datapoint: t,
				seriesIndex: t.serieIndex
			});
		}
		function Bn(e) {
			let t = V.value[e];
			k.value.events.datapointLeave && k.value.events.datapointLeave({
				datapoint: t,
				seriesIndex: t.serieIndex
			}), In.value = null, Nt.value = !1, E.value = null, O.value = null, qt.value = "pointer";
		}
		function Vn(e, t = "pointer") {
			if (U.value.length === w.dataset.length) return;
			let n = V.value[e];
			if (!n) return;
			qt.value = t, O.value = n.serieIndex, Fn.value = {
				datapoint: n,
				seriesIndex: n.absoluteIndex,
				series: z.value,
				config: k.value
			}, k.value.events.datapointEnter && In.value !== n.serieIndex && k.value.events.datapointEnter({
				datapoint: n,
				seriesIndex: n.serieIndex
			}), In.value = n.serieIndex, Nt.value = !0, E.value = n.serieIndex;
			let r = k.value.style.chart.tooltip.customFormat;
			if (fe(r) && ne(() => r({
				seriesIndex: n.absoluteIndex,
				datapoint: n,
				series: z.value,
				config: k.value
			}))) Pt.value = r({
				seriesIndex: n.absoluteIndex,
				datapoint: n,
				series: z.value,
				config: k.value
			});
			else {
				let e = "";
				e += `<div style="width:100%;text-align:center;border-bottom:1px solid ${k.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${n.name}</div>`, e += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 60 60" height="14" width="14"><rect x="0" y="0" height="60" width="60" stroke="none" rx="1" fill="${n.color}" />${jt.pattern ? `<rect x="0" y="0" height="60" width="60" stroke="none" rx="1" stroke="none" fill="url(#pattern_${T.value}_${n.absoluteIndex})"/>` : ""}</svg>`, e += `<b>${Nn({
					config: k.value.style.chart.tooltip,
					showVal: k.value.style.chart.tooltip.showValue,
					showPercentage: k.value.style.chart.tooltip.showPercentage,
					val: `<span>${re(k.value.style.chart.layout.labels.dataLabels.formatter, n.value, s({
						p: k.value.style.chart.layout.labels.dataLabels.prefix,
						v: n.value,
						s: k.value.style.chart.layout.labels.dataLabels.suffix,
						r: k.value.style.chart.tooltip.roundingValue
					}), {
						datapoint: n,
						seriesIndex: n.absoluteIndex,
						series: z.value
					})}</span>`,
					percentage: `<span>${s({
						v: hn.value ? 1 / Zt.value.length * 100 : n.value / J.value * 100,
						s: "%",
						r: k.value.style.chart.tooltip.roundingPercentage
					})}</span>`
				})}</b></div>`, Pt.value = e;
			}
		}
		let Hn = Qe, X = l(() => ({
			head: B.value.map((e) => ({
				name: e.name,
				color: e.color
			})),
			body: B.value.map((e) => e.value)
		}));
		function Un(e) {
			return k.value.useBlurOnHover && ![null, void 0].includes(E.value) && E.value !== e ? `url(#blur_${T.value})` : "";
		}
		function Wn(e, t) {
			return k.value.style.chart.layout.labels.captions.show ? V.value.length && !Cn.value && !k.value.style.chart.layout.grid.vertical && (V.value[e].isFirst && t.position < k.value.style.chart.layout.grid.size - 2 || V.value[e].isAbsoluteFirst && e % k.value.style.chart.layout.grid.size === 0 && V.value[e].absoluteStartIndex) : !1;
		}
		function Gn(e, t = null) {
			let n = re(k.value.style.chart.layout.labels.dataLabels.formatter, V.value[e].value, s({
				p: k.value.style.chart.layout.labels.dataLabels.prefix,
				v: V.value[e].value,
				s: k.value.style.chart.layout.labels.dataLabels.suffix,
				r: k.value.style.chart.layout.labels.captions.roundingValue
			}), {
				datapoint: V.value[e],
				position: t
			}), r = s({
				v: V.value[e].proportion,
				s: "%",
				r: k.value.style.chart.layout.labels.captions.roundingPercentage
			}), i = (k.value.style.chart.layout.labels.captions.serieNameAbbreviation ? ae({
				source: V.value[e].name,
				length: k.value.style.chart.layout.labels.captions.serieNameMaxAbbreviationSize
			}) : V.value[e].name) + (k.value.style.chart.layout.labels.captions.showPercentage || k.value.style.chart.layout.labels.captions.showValue ? ":" : "");
			return `${k.value.style.chart.layout.labels.captions.showSerieName ? i : ""} ${Nn({
				val: n,
				percentage: r,
				showVal: k.value.style.chart.layout.labels.captions.showValue,
				showPercentage: k.value.style.chart.layout.labels.captions.showPercentage,
				config: k.value.style.chart.layout.labels.dataLabels
			})}`;
		}
		function Kn(e = null) {
			He(() => {
				let n = X.value.head.map((e, t) => [
					[e.name],
					[X.value.body[t]],
					[isNaN(X.value.body[t] / J.value) ? "-" : X.value.body[t] / J.value * 100]
				]), i = [
					[k.value.style.chart.title.text],
					[k.value.style.chart.title.subtitle.text],
					[
						[""],
						["val"],
						["%"]
					]
				].concat(n), a = r(i);
				e ? e(a) : t({
					csvContent: a,
					title: k.value.style.chart.title.text || "vue-ui-waffle"
				});
			});
		}
		let Z = l(() => ({
			head: [
				" <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>",
				re(k.value.style.chart.layout.labels.dataLabels.formatter, J.value, s({
					p: k.value.style.chart.layout.labels.dataLabels.prefix,
					v: J.value,
					s: k.value.style.chart.layout.labels.dataLabels.suffix,
					r: k.value.table.td.roundingValue
				})),
				"100%"
			],
			body: X.value.head.map((e, t) => [
				{
					color: e.color,
					name: e.name
				},
				re(k.value.style.chart.layout.labels.dataLabels.formatter, X.value.body[t], s({
					p: k.value.style.chart.layout.labels.dataLabels.prefix,
					v: X.value.body[t],
					s: k.value.style.chart.layout.labels.dataLabels.suffix,
					r: k.value.table.td.roundingValue
				})),
				isNaN(X.value.body[t] / J.value) ? "-" : s({
					v: X.value.body[t] / J.value * 100,
					s: "%",
					r: k.value.table.td.roundingPercentage
				})
			]),
			config: {
				th: {
					backgroundColor: k.value.table.th.backgroundColor,
					color: k.value.table.th.color,
					outline: k.value.table.th.outline
				},
				td: {
					backgroundColor: k.value.table.td.backgroundColor,
					color: k.value.table.td.color,
					outline: k.value.table.td.outline
				},
				shape: "square",
				breakpoint: k.value.table.responsiveBreakpoint
			},
			colNames: [
				k.value.table.columnNames.series,
				k.value.table.columnNames.value,
				k.value.table.columnNames.percentage
			]
		})), Q = v(!1);
		function qn(e) {
			Q.value = e, Ft.value += 1;
		}
		function Jn() {
			P.value.showTable = !P.value.showTable;
		}
		function Yn() {
			P.value.showTooltip = !P.value.showTooltip;
		}
		let Xn = v(!1);
		function Zn() {
			Xn.value = !Xn.value;
		}
		async function Qn({ scale: e = 2 } = {}) {
			if (!D.value) return;
			let { width: t, height: n } = D.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await Se({
				domElement: D.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: k.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let $ = l(() => {
			let e = k.value.table.useDialog && !k.value.table.show, t = P.value.showTable;
			return {
				component: e ? Et : St,
				title: `${k.value.style.chart.title.text}${k.value.style.chart.title.subtitle.text ? `: ${k.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: k.value.table.th.backgroundColor,
					color: k.value.table.th.color,
					headerColor: k.value.table.th.color,
					headerBg: k.value.table.th.backgroundColor,
					isFullscreen: Q.value,
					fullscreenParent: D.value,
					forcedWidth: Math.min(500, window.innerWidth * .8),
					isCursorPointer: A.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: k.value.style.chart.backgroundColor,
							color: k.value.style.chart.color
						},
						head: {
							backgroundColor: k.value.style.chart.backgroundColor,
							color: k.value.style.chart.color
						}
					}
				}
			};
		});
		S(() => P.value.showTable, (e) => {
			k.value.table.show || (e && k.value.table.useDialog && Wt.value ? Wt.value.open() : "close" in Wt.value && Wt.value.close());
		});
		function $n() {
			P.value.showTable = !1, Gt.value && Gt.value.setTableIconState(!1);
		}
		let er = l(() => q.value.map((e) => ({
			...e,
			name: e.display
		}))), tr = l(() => k.value.style.chart.backgroundColor), nr = l(() => k.value.style.chart.legend), rr = l(() => k.value.style.chart.title), { isCallbackImaging: ir, isCallbackSvg: ar, generateSvg: or, onGenerateImage: sr } = xe({
			svg: j,
			title: rr,
			legend: nr,
			legendItems: er,
			backgroundColor: tr,
			getSvgCallback: () => k.value.userOptions.callbacks.svg,
			generateImage: ln
		});
		async function cr() {
			if (Hn("copyAlt", {
				config: k.value,
				dataset: bn.value
			}), !k.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(k.value.userOptions.callbacks.altCopy({
				config: k.value,
				dataset: bn.value
			}));
		}
		function lr() {
			O.value = null, Jt.value = !0;
		}
		function ur() {
			O.value = null, qt.value = "pointer", Nt.value = !1, E.value = null, In.value = null, Jt.value = !1;
		}
		function dr(e) {
			if (!j.value || Xn.value || document.activeElement !== j.value || !Y.value.length || U.value.length === w.dataset.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				O.value = null, qt.value = "pointer", Nt.value = !1, E.value = null, In.value = null;
				return;
			}
			if (r) {
				if (O.value === null) return;
				let e = Rn(O.value);
				if (e === null) return;
				zn(e);
				return;
			}
			let a = Y.value.findIndex((e) => e.serieIndex === O.value), o = null;
			if (o = a === -1 ? n ? Y.value[0] : Y.value[Y.value.length - 1] : n ? Y.value[(a + 1) % Y.value.length] : Y.value[(a - 1 + Y.value.length) % Y.value.length], !o) return;
			let ee = Rn(o.serieIndex);
			ee !== null && (fr(o.serieIndex), Vn(ee, "keyboard"));
		}
		function fr(e) {
			if (!Number.isFinite(e) || !j.value) return;
			let t = Ln(e);
			if (!t || !t.rectIndexes.length) return;
			let n = t.rectIndexes.map((e) => {
				let t = H.value[e];
				return t ? {
					x: t.x + k.value.style.chart.layout.grid.spaceBetween / 2 + fn.value / 2,
					y: t.y + k.value.style.chart.layout.grid.spaceBetween / 2 + pn.value / 2
				} : null;
			}).filter(Boolean);
			if (!n.length) return;
			let r = Math.min(...n.map((e) => e.x)), i = Math.max(...n.map((e) => e.x)), a = Math.min(...n.map((e) => e.y)), o = Math.max(...n.map((e) => e.y)), ee = (r + i) / 2, te = (a + o) / 2, s = j.value.getBoundingClientRect();
			Kt.value = {
				x: s.left + ee / F.value.width * s.width,
				y: s.top + te / F.value.height * s.height
			};
		}
		let pr = l(() => ({
			headers: Z.value?.colNames ?? [],
			rows: Z.value?.body ?? []
		}));
		return Te({
			getData: xn,
			getImage: Qn,
			generatePdf: cn,
			generateCsv: Kn,
			generateImage: ln,
			generateSvg: or,
			hideSeries: Mn,
			showSeries: jn,
			toggleTable: Jn,
			toggleTooltip: Yn,
			toggleAnnotator: Zn,
			toggleFullscreen: qn,
			copyAlt: cr
		}), (e, t) => (_(), f("div", {
			class: Ue(`vue-data-ui-component vue-ui-waffle ${Q.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			ref_key: "waffleChart",
			ref: D,
			id: `vue-ui-waffle_${T.value}`,
			style: We(`font-family:${k.value.style.fontFamily};width:100%; text-align:center;background:${k.value.style.chart.backgroundColor};${k.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: t[2] ||= () => x(en)(!0),
			onMouseleave: t[3] ||= () => x(en)(!1)
		}, [
			p("div", {
				id: `chart-instructions-${T.value}`,
				class: "sr-only"
			}, [p("p", null, Ye(k.value.a11y.translations.keyboardNavigation), 1)], 8, et),
			pr.value?.rows?.length ? (_(), u(Ae, {
				key: 0,
				uid: T.value,
				head: pr.value.headers,
				body: pr.value.rows,
				notice: k.value.a11y.translations.tableAvailable,
				caption: k.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : d("", !0),
			k.value.userOptions.buttons.annotator ? (_(), u(x(wt), {
				key: 1,
				svgRef: x(j),
				backgroundColor: k.value.style.chart.backgroundColor,
				color: k.value.style.chart.color,
				active: Xn.value,
				isCursorPointer: A.value,
				onClose: Zn
			}, {
				"annotator-action-close": C(() => [b(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": C(({ color: t }) => [b(e.$slots, "annotator-action-color", g(h({ color: t })), void 0, !0)]),
				"annotator-action-draw": C(({ mode: t }) => [b(e.$slots, "annotator-action-draw", g(h({ mode: t })), void 0, !0)]),
				"annotator-action-undo": C(({ disabled: t }) => [b(e.$slots, "annotator-action-undo", g(h({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": C(({ disabled: t }) => [b(e.$slots, "annotator-action-redo", g(h({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": C(({ disabled: t }) => [b(e.$slots, "annotator-action-delete", g(h({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : d("", !0),
			un.value ? (_(), f("div", {
				key: 2,
				ref_key: "noTitle",
				ref: zt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : d("", !0),
			k.value.style.chart.title.text ? (_(), f("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: It,
				style: "width:100%;background:transparent;padding-bottom:12px"
			}, [(_(), u(Ce, {
				key: `title_${Bt.value}`,
				config: {
					title: {
						cy: "waffle-title",
						...k.value.style.chart.title
					},
					subtitle: {
						cy: "waffle-subtitle",
						...k.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : d("", !0),
			p("div", { id: `legend-top-${T.value}` }, null, 8, tt),
			k.value.userOptions.show && Mt.value && (x(tn) || x($t)) ? (_(), u(x(Ct), {
				ref_key: "userOptionsRef",
				ref: Gt,
				key: `user_options_${Ft.value}`,
				backgroundColor: k.value.style.chart.backgroundColor,
				color: k.value.style.chart.color,
				isPrinting: x(on),
				isImaging: x(sn),
				uid: T.value,
				hasTooltip: k.value.userOptions.buttons.tooltip && k.value.style.chart.tooltip.show,
				hasPdf: k.value.userOptions.buttons.pdf,
				hasImg: k.value.userOptions.buttons.img,
				hasSvg: k.value.userOptions.buttons.svg,
				hasXls: k.value.userOptions.buttons.csv,
				hasTable: k.value.userOptions.buttons.table,
				hasFullscreen: k.value.userOptions.buttons.fullscreen,
				hasAltCopy: k.value.userOptions.buttons.altCopy,
				isFullscreen: Q.value,
				isTooltip: P.value.showTooltip,
				titles: { ...k.value.userOptions.buttonTitles },
				chartElement: D.value,
				position: k.value.userOptions.position,
				hasAnnotator: k.value.userOptions.buttons.annotator,
				isAnnotation: Xn.value,
				callbacks: k.value.userOptions.callbacks,
				printScale: k.value.userOptions.print.scale,
				tableDialog: k.value.table.useDialog,
				isCursorPointer: A.value,
				onToggleFullscreen: qn,
				onGeneratePdf: x(cn),
				onGenerateCsv: Kn,
				onGenerateImage: x(sr),
				onGenerateSvg: x(or),
				onToggleTable: Jn,
				onToggleTooltip: Yn,
				onToggleAnnotator: Zn,
				onCopyAlt: cr,
				style: We({ visibility: x(tn) ? x($t) ? "visible" : "hidden" : "visible" })
			}, Re({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: C(({ isOpen: t, color: n }) => [b(e.$slots, "menuIcon", g(h({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: C(() => [b(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: C(() => [b(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: C(() => [b(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: C(() => [b(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: C(() => [b(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: C(() => [b(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: C(({ toggleFullscreen: t, isFullscreen: n }) => [b(e.$slots, "optionFullscreen", g(h({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: C(({ toggleAnnotator: t, isAnnotator: n }) => [b(e.$slots, "optionAnnotator", g(h({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: C(({ altCopy: t }) => [b(e.$slots, "optionAltCopy", g(h({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: C(() => [b(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: C(() => [b(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : d("", !0),
			p("div", nt, [(_(), f("svg", {
				ref_key: "svgRef",
				ref: j,
				xmlns: x(le),
				"aria-describedby": `chart-instructions-${T.value}`,
				class: Ue({
					"vue-data-ui-fullscreen--on": Q.value,
					"vue-data-ui-fulscreen--off": !Q.value
				}),
				tabindex: "0",
				viewBox: `0 0 ${F.value.width <= 0 ? 10 : F.value.width} ${F.value.height <= 0 ? 10 : F.value.height}`,
				style: We(`max-width:100%;overflow:visible;background:transparent;color:${k.value.style.chart.color}`),
				onFocus: lr,
				onBlur: ur,
				onKeydown: dr
			}, [
				Be(x(Tt)),
				p("defs", null, [(_(!0), f(c, null, y(V.value, (e, t) => (_(), u(Oe, {
					t: "radial",
					cx: "50%",
					cy: "50%",
					r: "50%",
					fx: "50%",
					fy: "50%",
					id: `gradient_${T.value}_${t}`,
					key: `gradient_${T.value}_${t}`,
					stops: [[
						"0%",
						x(n)(x(te)(e.color, .05), 100 - k.value.style.chart.layout.rect.gradientIntensity),
						1
					], [
						"100%",
						e.color,
						1
					]]
				}, null, 8, ["id", "stops"]))), 128))]),
				p("defs", null, [p("filter", {
					id: `blur_${T.value}`,
					x: "-50%",
					y: "-50%",
					width: "200%",
					height: "200%"
				}, [...t[4] ||= [p("feGaussianBlur", {
					in: "SourceGraphic",
					stdDeviation: 2
				}, null, -1), p("feColorMatrix", {
					type: "saturate",
					values: "0"
				}, null, -1)]], 8, it)]),
				k.value.useCustomCells && V.value.length && e.$slots.cell ? (_(!0), f(c, { key: 0 }, y(H.value, (t, n) => (_(), f("foreignObject", {
					x: t.x,
					y: t.y,
					height: R.value <= 0 ? 1e-4 : R.value,
					width: L.value <= 0 ? 1e-4 : L.value,
					class: "vue-ui-waffle-custom-cell-foreignObject"
				}, [b(e.$slots, "cell", Ve({ ref_for: !0 }, {
					cell: {
						...t,
						color: V.value[n].color,
						height: Math.max(0, R.value),
						width: Math.max(0, L.value),
						...V.value[n]
					},
					isSelected: [null, void 0].includes(E.value) ? !0 : V.value[n].serieIndex === E.value
				}), void 0, !0)], 8, at))), 256)) : d("", !0),
				!V.value.length && !k.value.useCustomCells ? (_(), f("rect", {
					key: 1,
					x: 12,
					y: 12,
					height: I.value.height - 24,
					width: I.value.width - 24,
					rx: 3,
					fill: "none",
					stroke: "black"
				}, null, 8, ot)) : V.value.length && !k.value.useCustomCells ? (_(), f(c, { key: 2 }, [
					e.$slots.pattern ? (_(), f("g", st, [(_(!0), f(c, null, y(_n.value, (t) => (_(), f("defs", null, [b(e.$slots, "pattern", Ve({ ref_for: !0 }, {
						seriesIndex: t.absoluteIndex,
						patternId: `pattern_${T.value}_${t.absoluteIndex}`
					}), void 0, !0)]))), 256))])) : d("", !0),
					(_(!0), f(c, null, y(H.value, (e, t) => (_(), f("rect", {
						rx: k.value.style.chart.layout.rect.rounded ? k.value.style.chart.layout.rect.rounding : 0,
						x: e.x + k.value.style.chart.layout.grid.spaceBetween / 2,
						y: e.y + k.value.style.chart.layout.grid.spaceBetween / 2,
						height: R.value <= 0 ? 1e-4 : R.value,
						width: L.value <= 0 ? 1e-4 : L.value,
						fill: "white",
						stroke: k.value.style.chart.layout.rect.stroke,
						"stroke-width": k.value.style.chart.layout.rect.strokeWidth,
						filter: Un(V.value[t].serieIndex)
					}, null, 8, ct))), 256)),
					(_(!0), f(c, null, y(H.value, (e, t) => (_(), f("rect", {
						rx: k.value.style.chart.layout.rect.rounded ? k.value.style.chart.layout.rect.rounding : 0,
						x: e.x + k.value.style.chart.layout.grid.spaceBetween / 2,
						y: e.y + k.value.style.chart.layout.grid.spaceBetween / 2,
						height: R.value <= 0 ? 1e-4 : R.value,
						width: L.value <= 0 ? 1e-4 : L.value,
						fill: k.value.style.chart.layout.rect.useGradient && k.value.style.chart.layout.rect.gradientIntensity > 0 ? `url(#gradient_${T.value}_${t})` : V.value[t].color,
						stroke: k.value.style.chart.layout.rect.stroke,
						"stroke-width": k.value.style.chart.layout.rect.strokeWidth,
						filter: Un(V.value[t].serieIndex)
					}, null, 8, lt))), 256)),
					e.$slots.pattern ? (_(), f("g", ut, [(_(!0), f(c, null, y(H.value, (e, t) => (_(), f("rect", {
						rx: k.value.style.chart.layout.rect.rounded ? k.value.style.chart.layout.rect.rounding : 0,
						x: e.x + k.value.style.chart.layout.grid.spaceBetween / 2,
						y: e.y + k.value.style.chart.layout.grid.spaceBetween / 2,
						height: R.value <= 0 ? 1e-4 : R.value,
						width: L.value <= 0 ? 1e-4 : L.value,
						fill: `url(#pattern_${T.value}_${V.value[t].absoluteIndex})`,
						stroke: "none",
						filter: Un(V.value[t].serieIndex)
					}, null, 8, dt))), 256))])) : d("", !0)
				], 64)) : d("", !0),
				e.$slots.cellSvg ? (_(!0), f(c, { key: 3 }, y(H.value, (t, n) => (_(), f("g", null, [b(e.$slots, "cellSvg", Ve({ ref_for: !0 }, {
					cell: {
						...t,
						color: V.value[n].color,
						height: Math.max(0, R.value),
						width: Math.max(0, L.value),
						...V.value[n]
					},
					isSelected: [null, void 0].includes(E.value) ? !0 : V.value[n].serieIndex === E.value
				}), void 0, !0)]))), 256)) : d("", !0),
				(_(!0), f(c, null, y(H.value, (e, t) => (_(), f(c, null, [Wn(t, e) ? (_(), f("text", {
					key: `datalabel_${t}`,
					textContent: Ye(Gn(t, e)),
					x: e.x + k.value.style.chart.layout.labels.captions.offsetX + k.value.style.chart.layout.grid.spaceBetween / 2 + 6,
					y: e.y + k.value.style.chart.layout.labels.captions.offsetY + k.value.style.chart.layout.grid.spaceBetween / 2 + pn.value / 2 + k.value.style.chart.layout.labels.captions.fontSize / 3,
					"font-size": k.value.style.chart.layout.labels.captions.fontSize,
					fill: x(ce)(V.value[t].color),
					filter: Un(V.value[t].serieIndex)
				}, null, 8, ft)) : d("", !0)], 64))), 256)),
				(_(!0), f(c, null, y(H.value, (e, t) => (_(), f("rect", {
					x: e.x + k.value.style.chart.layout.grid.spaceBetween / 2,
					y: e.y + k.value.style.chart.layout.grid.spaceBetween / 2,
					height: pn.value,
					width: fn.value,
					fill: "transparent",
					stroke: "none",
					onMouseover: (e) => Vn(t),
					onMouseleave: (e) => Bn(t),
					onClick: (e) => zn(t)
				}, null, 40, pt))), 256)),
				b(e.$slots, "svg", { svg: {
					...F.value,
					isPrintingImg: x(on) || x(sn) || x(ir),
					isPrintingSvg: x(ar)
				} }, void 0, !0)
			], 46, rt)), e.$slots.hint ? (_(), f("div", mt, [b(e.$slots, "hint", g(h({
				hint: k.value.a11y.translations.keyboardNavigation,
				isVisible: Jt.value
			})), void 0, !0)])) : d("", !0)]),
			e.$slots.watermark ? (_(), f("div", ht, [b(e.$slots, "watermark", g(h({ isPrinting: x(on) || x(sn) || x(ir) || x(ar) })), void 0, !0)])) : d("", !0),
			p("div", { id: `legend-bottom-${T.value}` }, null, 8, gt),
			Ut.value && (k.value.style.chart.legend.show || e.$slots.legend) ? (_(), u(Le, {
				key: 6,
				to: k.value.style.chart.legend.position === "top" ? `#legend-top-${T.value}` : `#legend-bottom-${T.value}`
			}, [p("div", {
				ref_key: "chartLegend",
				ref: Lt
			}, [b(e.$slots, "legend", { legend: q.value }, () => [k.value.style.chart.legend.show ? (_(), u(Pe, {
				key: `legend_${Ht.value}`,
				legendSet: q.value,
				config: Pn.value,
				isCursorPointer: A.value,
				onClickMarker: t[0] ||= ({ legend: e }) => K(e.uid)
			}, Re({
				item: C(({ legend: e }) => [p("div", {
					onClick: (t) => e.segregate(),
					style: We(`opacity:${U.value.includes(e.uid) ? .5 : 1}`)
				}, Ye(e.display), 13, _t)]),
				legendToggle: C(() => [q.value.length > 2 && k.value.style.chart.legend.selectAllToggle.show && !x(Xt) ? (_(), u(ke, {
					key: 0,
					backgroundColor: k.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: k.value.style.chart.legend.selectAllToggle.color,
					fontSize: k.value.style.chart.legend.fontSize,
					checked: U.value.length > 0,
					isCursorPointer: A.value,
					onToggle: kn
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : d("", !0)]),
				_: 2
			}, [e.$slots.pattern ? {
				name: "legend-pattern",
				fn: C(({ legend: e, index: t }) => [Be(we, {
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
			])) : d("", !0)], !0)], 512)], 8, ["to"])) : d("", !0),
			e.$slots.source ? (_(), f("div", {
				key: 7,
				ref_key: "source",
				ref: Rt,
				dir: "auto"
			}, [b(e.$slots, "source", {}, void 0, !0)], 512)) : d("", !0),
			Be(x(yt), {
				teleportTo: k.value.style.chart.tooltip.teleportTo,
				show: P.value.showTooltip && Nt.value && U.value.length < w.dataset.length,
				backgroundColor: k.value.style.chart.tooltip.backgroundColor,
				color: k.value.style.chart.tooltip.color,
				borderRadius: k.value.style.chart.tooltip.borderRadius,
				borderColor: k.value.style.chart.tooltip.borderColor,
				borderWidth: k.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: k.value.style.chart.tooltip.backgroundOpacity,
				position: k.value.style.chart.tooltip.position,
				offsetX: k.value.style.chart.tooltip.offsetX,
				offsetY: k.value.style.chart.tooltip.offsetY,
				parent: D.value,
				content: Pt.value,
				isCustom: k.value.style.chart.tooltip.customFormat && typeof k.value.style.chart.tooltip.customFormat == "function",
				fontSize: k.value.style.chart.tooltip.fontSize,
				isFullscreen: Q.value,
				smooth: k.value.style.chart.tooltip.smooth,
				backdropFilter: k.value.style.chart.tooltip.backdropFilter,
				smoothForce: k.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: k.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: qt.value === "keyboard",
				a11yPosition: Kt.value
			}, {
				"tooltip-before": C(() => [b(e.$slots, "tooltip-before", g(h({ ...Fn.value })), void 0, !0)]),
				tooltip: C(() => [b(e.$slots, "tooltip", g(h({ ...Fn.value })), void 0, !0)]),
				"tooltip-after": C(() => [b(e.$slots, "tooltip-after", g(h({ ...Fn.value })), void 0, !0)]),
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
				"position",
				"offsetX",
				"offsetY",
				"parent",
				"content",
				"isCustom",
				"fontSize",
				"isFullscreen",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			Mt.value && k.value.userOptions.buttons.table ? (_(), u(qe($.value.component), Ve({ key: 8 }, $.value.props, {
				ref_key: "tableUnit",
				ref: Wt,
				onClose: $n
			}), Re({
				content: C(() => [(_(), u(x(xt), {
					key: `table_${Vt.value}`,
					colNames: Z.value.colNames,
					head: Z.value.head,
					body: Z.value.body,
					config: Z.value.config,
					title: k.value.table.useDialog ? "" : $.value.title,
					withCloseButton: !k.value.table.useDialog,
					isCursorPointer: A.value,
					onClose: $n
				}, {
					th: C(({ th: e }) => [p("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, vt)]),
					td: C(({ td: e }) => [ze(Ye(e.name || e), 1)]),
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
			}, [k.value.table.useDialog ? {
				name: "title",
				fn: C(() => [ze(Ye($.value.title), 1)]),
				key: "0"
			} : void 0, k.value.table.useDialog ? {
				name: "actions",
				fn: C(() => [p("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Kn(k.value.userOptions.callbacks.csv),
					style: We({ cursor: A.value ? "pointer" : "default" })
				}, [Be(x(bt), {
					name: "fileCsv",
					stroke: $.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : d("", !0),
			b(e.$slots, "skeleton", {}, () => [x(Xt) ? (_(), u(ve, { key: 0 })) : d("", !0)], !0)
		], 46, $e));
	}
}, [["__scopeId", "data-v-1d448c79"]]);
//#endregion
export { Qe as n, yt as t };
