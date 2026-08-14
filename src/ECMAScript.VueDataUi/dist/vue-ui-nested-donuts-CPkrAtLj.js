import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Ft as i, Jt as a, Kt as o, Pt as s, Rt as c, S as l, X as u, b as ee, ct as te, et as ne, f as re, i as d, jt as ie, kt as ae, n as oe, p as se, q as ce, t as le, tt as ue, w as de, xt as fe } from "./lib-Bttd6u5E.js";
import { n as pe, t as me } from "./useHints-Dq_w2E8B.js";
import { t as he } from "./useConfig-DlNpz6P8.js";
import { t as ge } from "./usePrinter-DN5bYhTG.js";
import { n as _e, t as ve } from "./BaseScanner-DZvpgOjM.js";
import { t as ye } from "./useNestedProp-vPNvh7rV.js";
import { t as be } from "./useThemeCheck-C43Tcqmk.js";
import { t as xe } from "./useChartExport-DNiwdPmb.js";
import { t as Se } from "./img-Bnokohej.js";
import { n as Ce } from "./Title-BE3qg9xl.js";
import { t as we } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Te, t as Ee } from "./useResponsive-ZtArZtUf.js";
import { t as De } from "./DefGrad-DVBqDjhO.js";
import { t as Oe } from "./BaseLegendToggle-DZVucLnv.js";
import { t as ke } from "./A11yDataTable-DdRsVULz.js";
import { t as Ae } from "./useUserOptionState-DK-_1ddE.js";
import { t as je } from "./useChartAccessibility-DYqac8yF.js";
import { t as Me } from "./labelUtils-BeVpDvTJ.js";
import { t as Ne } from "./Legend-CQxUgOd-.js";
import { t as Pe } from "./vue_ui_nested_donuts-B8csIoVO.js";
import { Fragment as f, Teleport as Fe, computed as p, createBlock as m, createCommentVNode as h, createElementBlock as g, createElementVNode as _, createSlots as Ie, createTextVNode as Le, createVNode as Re, defineAsyncComponent as v, guardReactiveProps as y, mergeProps as ze, nextTick as Be, normalizeClass as Ve, normalizeProps as b, normalizeStyle as He, onBeforeUnmount as Ue, onMounted as We, openBlock as x, ref as S, renderList as C, renderSlot as w, resolveDynamicComponent as Ge, shallowRef as Ke, toDisplayString as T, toRefs as qe, unref as E, vShow as Je, watch as Ye, withCtx as D, withDirectives as Xe } from "vue";
//#region src/components/vue-ui-nested-donuts.vue
var Ze = /* @__PURE__ */ e({ default: () => Ot }), Qe = ["id"], $e = ["id"], et = ["id"], tt = { style: { position: "relative" } }, nt = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], rt = ["width", "height"], it = ["id"], at = ["id"], ot = ["id"], st = ["flood-color"], ct = ["id", "d"], lt = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], ut = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], dt = { key: 1 }, ft = ["d", "fill"], pt = { key: 2 }, mt = [
	"font-size",
	"font-weight",
	"fill",
	"dy"
], ht = ["href"], gt = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], _t = { key: 3 }, vt = ["filter"], yt = [
	"opacity",
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], bt = [
	"opacity",
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], xt = [
	"d",
	"fill",
	"onMouseenter",
	"onClick",
	"onMouseleave"
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
}, wt = ["id"], Tt = {
	key: 0,
	class: "vue-ui-nested-donuts-legend-title"
}, Et = ["onClick"], Dt = ["innerHTML"], Ot = /*#__PURE__*/ we({
	__name: "vue-ui-nested-donuts",
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
	setup(e, { expose: we, emit: Ze }) {
		let Ot = v(() => import("./Tooltip-DhjyfHwz.js")), kt = v(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), At = v(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), jt = v(() => import("./DataTable-BbKgJ5UI.js")), Mt = v(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Nt = v(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Pt = v(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ft = v(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_nested_donuts: It } = he(), { isThemeValid: Lt, warnInvalidTheme: Rt } = be(), O = e, zt = p({
			get() {
				return !!O.dataset && O.dataset.length;
			},
			set(e) {
				return e;
			}
		}), k = S(ce()), Bt = S(!1), Vt = S(""), Ht = S(null), Ut = S(0), A = S(null), Wt = S(null), Gt = S(null), Kt = S(null), qt = S(null), Jt = S(0), Yt = S(0), Xt = S(0), j = S(!0), M = S([]), Zt = S([]), Qt = S(!1), $t = S(null), en = S(null), N = S(null), tn = S({
			x: 0,
			y: 0
		}), nn = S("pointer"), rn = S(!1), P = S(!1);
		function an(e) {
			P.value = e, Ut.value += 1;
		}
		let F = S(pn());
		pe({
			config: () => F.value,
			dataset: () => O.dataset,
			component: "VueUiNestedDonuts",
			rules: [
				me.emptyArray,
				{
					test: (e) => e.some((e) => e.series.length > 6),
					message: [
						"👀 Some series have > 6 data points. Consider:",
						"",
						"▶️ Grouping small values dynamically into a single \"Other\" data point.",
						"",
						"▶️ Using filters to let users choose a maximum number of data points to display.",
						"",
						"▶️ Using VueUiHorizontalBar instead to make the dataset breakdown easier to read."
					]
				},
				{
					test: (e) => e.length === 1,
					message: [
						"👀 There is only one series in your dataset. Consider:",
						"",
						"▶️ Using VueUiDonut instead."
					]
				},
				{
					test: (e) => e.length > 6,
					message: [
						"👀 The number of series in your dataset > 6. Consider:",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display",
						"",
						"▶️ Using VueUiHorizontalBar instead to make the dataset breakdown easier to read."
					]
				}
			]
		});
		let I = p(() => F.value.userOptions.useCursorPointer), on = {
			name: "",
			series: [
				{
					name: "",
					values: [3],
					color: "#BABABA"
				},
				{
					name: "",
					values: [2],
					color: "#AAAAAA"
				},
				{
					name: "",
					values: [1],
					color: "#CACACA"
				}
			]
		}, sn = p(() => a({
			defaultConfig: {
				useCssAnimation: !1,
				table: { show: !1 },
				startAnimation: { show: !1 },
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: { labels: { dataLabels: { show: !1 } } },
					legend: {
						backgroundColor: "transparent",
						showValue: !1,
						showPercentage: !1
					},
					title: {
						color: "#1A1A1A",
						subtitle: { color: "#5A5A5A" }
					}
				} }
			},
			userConfig: F.value.skeletonConfig ?? {}
		})), { loading: cn, FINAL_DATASET: L, manualLoading: ln } = _e({
			...qe(O),
			FINAL_CONFIG: F,
			prepareConfig: pn,
			callback: () => {
				Promise.resolve().then(async () => {
					await gn();
				});
			},
			skeletonDataset: O.config?.skeletonDataset ?? [on, on],
			skeletonConfig: a({
				defaultConfig: F.value,
				userConfig: sn.value
			})
		}), { userOptionsVisible: un, setUserOptionsVisibility: dn, keepUserOptionState: fn } = Ae({ config: F.value }), { svgRef: R } = je({ config: F.value.style.chart.title });
		function pn() {
			let e = ye({
				userConfig: O.config,
				defaultConfig: It
			}), t = {}, n = e.theme;
			if (n) if (!Lt.value(e)) Rt(e), t = e;
			else {
				let r = ye({
					userConfig: Pe[n] || O.config,
					defaultConfig: e
				});
				t = {
					...ye({
						userConfig: O.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : o[n] || s
				};
			}
			else t = e;
			return t;
		}
		Ye(() => O.config, (e) => {
			cn.value || (F.value = pn()), un.value = !F.value.userOptions.showOnChartHover, vn(), Jt.value += 1, Yt.value += 1, Xt.value += 1, V.value.dataLabels.show = F.value.style.chart.layout.labels.dataLabels.show, V.value.showTable = F.value.table.show, V.value.showTooltip = F.value.style.chart.tooltip.show, H.value.width = F.value.style.chart.width, H.value.height = F.value.style.chart.height;
		}, { deep: !0 });
		let mn = p(() => {
			let { top: e, right: t, bottom: n, left: r } = F.value.style.chart.padding;
			return {
				css: `padding:${e}px ${t}px ${n}px ${r}px`,
				top: e,
				right: t,
				bottom: n,
				left: r
			};
		});
		function hn(e, t = 1e3, n = 50) {
			return new Promise((r) => {
				let i = e.length;
				M.value = Array(i).fill(0), Zt.value = [];
				let a = 0;
				e.forEach((e, o) => {
					setTimeout(() => {
						let n = performance.now();
						function s(l) {
							let u = Math.min((l - n) / t, 1), te = e * ne(u);
							M.value[o] = te, M.value = [...M.value];
							let re = [], d = 0;
							O.dataset.forEach((e, t) => {
								let n = ee(e.series.reduce((e, t) => e + ee(c(t.values).reduce((e, t) => e + t, 0)), 0)) - ee(M.value.slice(d, d + e.series.length).reduce((e, t) => e + t, 0));
								n > Number.MIN_VALUE && re.push({
									name: "__ghost__",
									arcOf: e.name,
									arcOfId: `${k.value}_${t}`,
									id: `ghost_${k.value}_${t}`,
									seriesIndex: -1,
									datasetIndex: t,
									color: "transparent",
									value: n,
									fullValue: n,
									absoluteValues: [],
									ghost: !0
								}), d += e.series.length;
							}), Zt.value = re, u < 1 ? requestAnimationFrame(s) : (a += 1, a === i && r());
						}
						requestAnimationFrame(s);
					}, o * n);
				});
			});
		}
		async function gn() {
			if (F.value.startAnimation?.show) {
				let e = L.value.flatMap((e) => e.series).map((e) => c(e.values).reduce((e, t) => e + t, 0));
				M.value = e.map(() => 0), j.value = !0, Zt.value = L.value.map((e, t) => {
					let n = e.series.reduce((e, t) => e + c(t.values).reduce((e, t) => e + t, 0), 0);
					return {
						name: "__ghost__",
						arcOf: e.name,
						arcOfId: `${k.value}_${t}`,
						id: `ghost_${k.value}_${t}`,
						seriesIndex: -1,
						datasetIndex: t,
						color: "transparent",
						value: n,
						fullValue: n,
						absoluteValues: [],
						ghost: !0
					};
				}), await Be(), hn(e, F.value.startAnimation.durationMs, F.value.startAnimation.staggerMs).then(() => {
					j.value = !1, Zt.value = [];
				});
			} else j.value = !1;
		}
		We(async () => {
			Qt.value = !0, vn(), await gn();
		});
		let z = Ke(null), _n = Ke(null), B = p(() => F.value.debug);
		function vn() {
			if (ie(O.dataset) ? (ue({
				componentName: "VueUiNestedDonuts",
				type: "dataset",
				debug: B.value
			}), zt.value = !1, ln.value = !0) : ln.value = F.value.loading, F.value.responsive) {
				let e = Te(() => {
					let { width: e, height: t } = Ee({
						chart: A.value,
						title: F.value.style.chart.title.text ? Wt.value : null,
						legend: F.value.style.chart.legend.show ? Gt.value : null,
						source: Kt.value,
						noTitle: qt.value,
						padding: mn.value
					});
					requestAnimationFrame(() => {
						H.value.width = e, H.value.height = t;
					});
				});
				z.value && (_n.value && z.value.unobserve(_n.value), z.value.disconnect()), z.value = new ResizeObserver(e), _n.value = A.value.parentNode, z.value.observe(_n.value);
			}
		}
		Ue(() => {
			z.value && (_n.value && z.value.unobserve(_n.value), z.value.disconnect());
		});
		let { isPrinting: yn, isImaging: bn, generatePdf: xn, generateImage: Sn } = ge({
			elementId: `nested_donuts_${k.value}`,
			fileName: F.value.style.chart.title.text || "vue-ui-nested-donuts",
			options: F.value.userOptions.print
		}), Cn = p(() => F.value.userOptions.show && !F.value.style.chart.title.text), wn = p(() => de(F.value.customPalette)), V = S({
			dataLabels: { show: F.value.style.chart.layout.labels.dataLabels.show },
			showTable: F.value.table.show,
			showTooltip: F.value.style.chart.tooltip.show
		});
		Ye(F, () => {
			V.value = {
				dataLabels: { show: F.value.style.chart.layout.labels.dataLabels.show },
				showTable: F.value.table.show,
				showTooltip: F.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let H = S({
			width: F.value.style.chart.width,
			height: F.value.style.chart.height
		}), U = Ze;
		function Tn({ datapoint: e, index: t, seriesIndex: n }) {
			F.value.events.datapointClick && F.value.events.datapointClick({
				datapoint: e,
				seriesIndex: n
			}), U("selectDatapoint", {
				datapoint: e,
				index: t
			});
		}
		function En({ from: e, to: t, duration: n, onUpdate: r, onDone: i, easing: a = ne }) {
			let o = performance.now();
			function s(c) {
				let l = Math.min((c - o) / n, 1), u = a(l);
				r(e + (t - e) * u, l), l < 1 ? requestAnimationFrame(s) : (r(t, 1), i && i());
			}
			requestAnimationFrame(s);
		}
		let W = S([]), G = p(() => {
			cn.value, L.value.forEach((e, t) => {
				[null, void 0].includes(e.name) && ue({
					componentName: "VueUiNestedDonuts",
					type: "datasetSerieAttribute",
					property: "name",
					index: t,
					debug: B.value
				}), [null, void 0].includes(e.series) ? ue({
					componentName: "VueUiNestedDonuts",
					type: "datasetSerieAttribute",
					property: "series",
					index: t,
					debug: B.value
				}) : e.series.length === 0 ? ue({
					componentName: "VueUiNestedDonuts",
					type: "datasetAttributeEmpty",
					property: `series at index ${t}`,
					debug: B.value
				}) : e.series.forEach((e, t) => {
					[null, void 0].includes(e.name) && ue({
						componentName: "VueUiNestedDonuts",
						type: "datasetSerieAttribute",
						property: "name",
						index: t,
						key: "serie",
						debug: B.value
					}), [null, void 0].includes(e.values) && ue({
						componentName: "VueUiNestedDonuts",
						type: "datasetSerieAttribute",
						property: "values",
						index: t,
						key: "serie",
						debug: B.value
					});
				});
			});
			let e = 0;
			return L.value.map((t, n) => ({
				...t,
				total: t.series.filter((e) => !W.value.includes(e.id)).map((e) => c(e.values).reduce((e, t) => e + t, 0)).reduce((e, t) => e + t, 0),
				datasetIndex: n,
				id: `${k.value}_${n}`,
				series: t.series.map((r, i) => {
					let a = c(r.values).reduce((e, t) => e + t, 0);
					return {
						name: r.name,
						arcOf: t.name,
						arcOfId: `${k.value}_${n}`,
						id: `${k.value}_${n}_${i}`,
						seriesIndex: i,
						datasetIndex: n,
						color: l(r.color) || wn.value[i] || s[i % s.length],
						value: j.value ? M.value[e++] ?? 0 : a,
						absoluteValues: r.values || []
					};
				})
			}));
		});
		Ye(() => O.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (ln.value = !1);
		}, { immediate: !0 });
		let K = p(() => Math.min(H.value.height, H.value.width) * (F.value.style.chart.layout.donut.strokeWidth / 512)), q = p(() => [...G.value].map((e, t) => {
			let n = e.series.filter((e) => !W.value.includes(e.id)).map((e) => e.value).reduce((e, t) => e + t, 0);
			return {
				...e,
				total: n,
				series: e.series.filter((e) => !W.value.includes(e.id)).map((e) => ({
					...e,
					proportion: e.value / n
				}))
			};
		}));
		function Dn(e, t, n) {
			let r = 0;
			for (let t = 0; t < e.length; t += 1) n.includes(e[t]) && (r += 1);
			return r < t;
		}
		let J = S(q.value);
		Ye(() => q.value, (e) => J.value = e);
		function On(e) {
			let t = e.map((e) => e.id);
			if (e.some((e) => W.value.includes(e.id))) {
				let e = new Set(t);
				W.value = W.value.filter((t) => !e.has(t));
			} else W.value.push(...t);
			U("selectLegend", q.value);
		}
		function Y(e) {
			let t = O.dataset.flatMap((e, t) => e.series.map((e, n) => ({
				value: c(e.values).reduce((e, t) => e + t, 0),
				id: `${k.value}_${t}_${n}`,
				arcOfId: `${k.value}_${t}`
			}))).find((t) => t.id === e.id);
			if (!t) return;
			let n = G.value.flatMap((e) => e.series).find((t) => t.id === e.id)?.value ?? 0, r = J.value.flatMap((e) => e.series).find((t) => t.id === e.id), i = r ? r.value : 0, a = G.value.find((e) => e.id === t.arcOfId);
			if (!a) return;
			let o = a.series.map((e) => e.id), s = Dn(o, o.length - 1, W.value);
			W.value.includes(e.id) ? (W.value = W.value.filter((t) => t !== e.id), F.value.serieToggleAnimation.show ? En({
				from: i,
				to: n,
				duration: F.value.serieToggleAnimation.durationMs,
				onUpdate: (t) => {
					J.value = J.value.map((n) => ({
						...n,
						series: n.series.map((n) => n.id === e.id ? {
							...n,
							value: t
						} : n)
					}));
				},
				onDone: () => {
					U("selectLegend", q.value);
				}
			}) : (J.value = J.value.map((t) => ({
				...t,
				series: t.series.map((t) => t.id === e.id ? {
					...t,
					value: n
				} : t)
			})), U("selectLegend", q.value))) : s && (F.value.serieToggleAnimation.show ? En({
				from: i,
				to: 0,
				duration: F.value.serieToggleAnimation.durationMs,
				onUpdate: (t) => {
					J.value = J.value.map((n) => ({
						...n,
						series: n.series.map((n) => n.id === e.id ? {
							...n,
							value: t
						} : n)
					}));
				},
				onDone: () => {
					W.value.push(e.id), U("selectLegend", q.value);
				}
			}) : (J.value = J.value.map((t) => ({
				...t,
				series: t.series.map((t) => t.id === e.id ? {
					...t,
					value: 0
				} : t)
			})), W.value.push(e.id), U("selectLegend", q.value)));
		}
		function kn(e) {
			return G.value.length ? G.value.flatMap((e) => e.series).filter((t) => t.name === e) || (B.value && console.warn(`VueUiNestedDonuts - Series name not found "${e}"`), null) : (B.value && console.warn("VueUiNestedDonuts - There are no series to show."), null);
		}
		function An(e) {
			let t = kn(e);
			t !== null && (Array.isArray(t) ? t.forEach((e) => {
				W.value.includes(e.id) && Y({ id: e.id });
			}) : W.value.includes(t.id) && Y({ id: t.id }));
		}
		function jn(e) {
			let t = kn(e);
			t !== null && (Array.isArray(t) ? t.forEach((e) => {
				W.value.includes(e.id) || Y({ id: e.id });
			}) : W.value.includes(t.id) || Y({ id: t.id }));
		}
		let Mn = p(() => K.value / G.value.length * F.value.style.chart.layout.donut.spacingRatio), Nn = p(() => J.value.map((e, t) => K.value - t * K.value / G.value.length)), X = p(() => J.value.map((e, t) => {
			let n = Math.abs(e.series.map((e) => e.value).reduce((e, t) => e + t, 0)) > 0, r = K.value - t * K.value / J.value.length, i = j.value ? Zt.value.find((e) => e.datasetIndex === t) : null, a = [...e.series, ...i ? [i] : []].map((e) => ({
				...e,
				value: e.value < 1e-11 ? Number.MIN_VALUE : e.value
			})), o = ae({ series: [{
				name: "_",
				color: F.value.style.chart.layout.donut.emptyFill,
				value: 1
			}] }, H.value.width / 2, H.value.height / 2, r, r, 1.99999, 2, 1, 360, 105.25, Mn.value), s = `M ${H.value.width / 2},${H.value.height / 2 + r}
            a ${r},${r} 0 1,1 0,${-2 * r}
            a ${r},${r} 0 1,1 0,${2 * r}`;
			return {
				...e,
				hasData: n,
				radius: r,
				skeleton: o,
				fullCirclePath: s,
				donut: ae({ series: a }, H.value.width / 2, H.value.height / 2, r, r, 1.99999, 2, 1, 360, 105.25, Mn.value)
			};
		})), Pn = p(() => [...G.value].map((e, t) => {
			let n = t * K.value / G.value.length;
			return {
				sizeRatio: n,
				donut: ae({ series: [{ value: 1 }] }, H.value.width / 2, H.value.height / 2, K.value - n, K.value - n, 1.99999, 2, 1, 360, 105.25, K.value / G.value.length * F.value.style.chart.layout.donut.spacingRatio)[0]
			};
		})), Fn = S(null), In = S(null), Z = S(null), Ln = S(null);
		function Rn({ datapoint: e, seriesIndex: t }) {
			F.value.events.datapointLeave && F.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}), Bt.value = !1, Fn.value = null, Ht.value = null, In.value = null, Z.value = null, N.value = null, nn.value = "pointer";
		}
		function zn({ val: e, percentage: t, showVal: n, showPercentage: r, config: i }) {
			return Me({
				config: i,
				val: e,
				percentage: t,
				showVal: n,
				showPercentage: r
			});
		}
		function Bn({ datapoint: e, _relativeIndex: t, seriesIndex: n, triggerMode: r = "pointer" }) {
			F.value.events.datapointEnter && F.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: n
			}), nn.value = r, N.value = n, Fn.value = e.arcOfId, In.value = e.id, Z.value = n, Ht.value = e.id, Ln.value = {
				datapoint: e,
				seriesIndex: n,
				series: J.value,
				config: F.value
			};
			let i = F.value.style.chart.tooltip.customFormat;
			if (fe(i) && te(() => i({
				seriesIndex: n,
				datapoint: e,
				series: J.value,
				config: F.value
			}))) Vt.value = i({
				seriesIndex: n,
				datapoint: e,
				series: J.value,
				config: F.value
			});
			else {
				let t = "";
				if (F.value.style.chart.tooltip.showAllItemsAtIndex && W.value.length === 0) {
					let r = J.value.map((e) => e.series.find((e) => e.seriesIndex === n));
					r.forEach((i, a) => {
						if (!i) return "";
						t += `
                    <div style="display:flex; flex-direction: column; justify-content:flex-start; align-items:flex-start;padding:6px 0; ${a < r.length - 1 ? `border-bottom:1px solid ${F.value.style.chart.tooltip.borderColor}` : ""}">
                        <div style="display:flex; flex-direction: row; gap: 3px; justify-content:flex-start; align-items:center;">
                            <svg viewBox="0 0 20 20" height="${F.value.style.chart.tooltip.fontSize}" width="${F.value.style.chart.tooltip.fontSize}">
                                <circle cx="10" cy="10" r="10" fill="${i.color}"/>
                            </svg>
                            <span>
                                ${i.arcOf ?? ""} - ${i.name}
                            </span>
                        </div>
                        <span>
                            <b>
                                ${zn({
							config: F.value.style.chart.tooltip,
							showVal: F.value.style.chart.tooltip.showValue,
							showPercentage: F.value.style.chart.tooltip.showPercentage,
							val: d(F.value.style.chart.layout.labels.dataLabels.formatter, e.value, u({
								p: F.value.style.chart.layout.labels.dataLabels.prefix,
								v: e.value,
								s: F.value.style.chart.layout.labels.dataLabels.suffix,
								r: F.value.style.chart.tooltip.roundingValue
							}), {
								datapoint: e,
								seriesIndex: n
							}),
							percentage: u({
								v: i.proportion * 100,
								s: "%",
								r: F.value.style.chart.tooltip.roundingPercentage
							})
						})}
                            </b>
                        </span>
                    </div>
                `;
					});
				} else t += `<div style="width:100%;text-align:center;border-bottom:1px solid ${F.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${e.arcOf ?? ""} - ${e.name}</div>`, t += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="none" fill="${e.color}"/></svg>`, F.value.style.chart.tooltip.showValue && (t += `<b>${d(F.value.style.chart.layout.labels.dataLabels.formatter, e.value, u({
					p: F.value.style.chart.layout.labels.dataLabels.prefix,
					v: e.value,
					s: F.value.style.chart.layout.labels.dataLabels.suffix,
					r: F.value.style.chart.tooltip.roundingValue
				}), {
					datapoint: e,
					seriesIndex: n
				})}</b>`), F.value.style.chart.tooltip.showPercentage && (F.value.style.chart.tooltip.showValue ? t += `<span>(${u({
					v: e.proportion * 100,
					s: "%",
					r: F.value.style.chart.tooltip.roundingPercentage
				})})</span></div>` : t += `<b>${u({
					v: e.proportion * 100,
					s: "%",
					r: F.value.style.chart.tooltip.roundingPercentage
				})}</b></div>`);
				Vt.value = `<div style="font-size:${F.value.style.chart.tooltip.fontSize}px">${t}</div>`;
			}
			Bt.value = !0;
		}
		function Vn(e) {
			return e.proportion * 100 > F.value.style.chart.layout.labels.dataLabels.hideUnderValue;
		}
		function Hn(e, t) {
			if (!F.value.useBlurOnHover) return "";
			if (F.value.style.chart.tooltip.showAllItemsAtIndex && W.value.length === 0) return [null, void 0].includes(Z.value) || Z.value === t ? "" : `url(#blur_${k.value})`;
			if (!F.value.style.chart.tooltip.showAllItemsAtIndex || W.value.length) return [null, void 0].includes(In.value) || In.value === e.id ? "" : `url(#blur_${k.value})`;
		}
		let Un = p(() => G.value.map((e, t) => {
			let n = e.series.filter((e) => !W.value.includes(e.id)), r = j.value ? n.map((e) => {
				let n = L.value[t].series.findIndex((t) => t.name === e.name);
				return c(L.value[t].series[n].values).reduce((e, t) => e + t, 0);
			}).reduce((e, t) => e + t, 0) : n.map((e) => e.value).reduce((e, t) => e + t, 0);
			return e.series.map((e, n) => {
				let i = c(L.value[t].series[n].values).reduce((e, t) => e + t, 0), a = j.value ? i : e.value, o = zn({
					val: d(F.value.style.chart.layout.labels.dataLabels.formatter, a, u({
						p: F.value.style.chart.layout.labels.dataLabels.prefix,
						v: a,
						s: F.value.style.chart.layout.labels.dataLabels.suffix,
						r: F.value.style.chart.legend.roundingValue
					}), {
						datapoint: e,
						seriesIndex: n
					}),
					percentage: isNaN(a / r) || W.value.includes(e.id) ? "-" : u({
						v: a / r * 100,
						s: "%",
						r: F.value.style.chart.legend.roundingPercentage
					}),
					showVal: F.value.style.chart.legend.showValue,
					showPercentage: F.value.style.chart.legend.showPercentage,
					config: F.value.style.chart.legend
				}), s = `${e.name}${F.value.style.chart.legend.showPercentage || F.value.style.chart.legend.showValue ? ": " : ""}${o}`;
				return {
					name: e.name,
					color: e.color,
					value: a,
					display: s,
					svgDisplay: `${e.arcOf ? `${e.arcOf} - ` : ""}${s}`,
					shape: "circle",
					arcOf: e.arcOf,
					id: e.id,
					seriesIndex: n,
					datasetIndex: t,
					total: r,
					opacity: W.value.includes(e.id) ? .5 : 1,
					segregate: () => Y(e),
					isSegregated: W.value.includes(e.id)
				};
			});
		})), Wn = p(() => ({
			cy: "nested-donuts-legend",
			backgroundColor: F.value.style.chart.legend.backgroundColor,
			color: F.value.style.chart.legend.color,
			fontSize: F.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: F.value.style.chart.legend.bold ? "bold" : ""
		})), Q = p(() => ({
			head: J.value.flatMap((e) => e.series.map((t) => ({
				name: `${e.name} - ${t.name}`,
				color: t.color,
				total: e.total
			}))),
			body: J.value.flatMap((e) => e.series.map((e) => e.value))
		}));
		function Gn(e = null) {
			Be(() => {
				let n = Q.value.head.map((e, t) => [
					[e.name],
					[Q.value.body[t]],
					[isNaN(Q.value.body[t] / e.total) ? "-" : Q.value.body[t] / e.total * 100]
				]), i = [
					[F.value.style.chart.title.text],
					[F.value.style.chart.title.subtitle.text],
					[
						[""],
						["val"],
						["%"]
					]
				].concat(n), a = r(i);
				e ? e(a) : t({
					csvContent: a,
					title: F.value.style.chart.title.text || "vue-ui-nested-donuts"
				});
			});
		}
		let Kn = p(() => {
			let e = [
				F.value.table.columnNames.series,
				F.value.table.columnNames.value,
				F.value.table.columnNames.percentage
			], t = Q.value.head.map((e, t) => {
				let n = u({
					p: F.value.style.chart.layout.labels.dataLabels.prefix,
					v: Q.value.body[t],
					s: F.value.style.chart.layout.labels.dataLabels.suffix,
					r: F.value.table.td.roundingValue
				});
				return [
					{
						color: e.color,
						name: e.name
					},
					n,
					isNaN(Q.value.body[t] / e.total) ? "-" : u({
						v: Q.value.body[t] / e.total * 100,
						s: "%",
						r: F.value.table.td.roundingPercentage
					})
				];
			}), n = t.map((e) => e.map((e, t) => t === 0 ? e.name : e)), r = {
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
				colNames: [
					F.value.table.columnNames.series,
					F.value.table.columnNames.value,
					F.value.table.columnNames.percentage
				],
				head: e,
				body: t,
				a11yBody: n,
				config: r
			};
		});
		function qn() {
			return G.value;
		}
		function Jn() {
			V.value.showTable = !V.value.showTable;
		}
		function Yn() {
			V.value.dataLabels.show = !V.value.dataLabels.show;
		}
		function Xn() {
			V.value.showTooltip = !V.value.showTooltip;
		}
		let Zn = S(!1);
		function Qn() {
			Zn.value = !Zn.value;
		}
		async function $n({ scale: e = 2 } = {}) {
			if (!A.value) return;
			let { width: t, height: n } = A.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await Se({
				domElement: A.value,
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
		let er = S(null);
		function tr() {
			if (!er.value) return;
			let { x: e, y: t, width: n, height: r } = er.value.getBBox();
			R.value && R.value.setAttribute("viewBox", `${e} ${t} ${n + Math.min(0, e)} ${r + Math.min(0, t)}`);
		}
		let nr = p(() => {
			let e = F.value.table.useDialog && !F.value.table.show, t = V.value.showTable;
			return {
				component: e ? Ft : At,
				title: `${F.value.style.chart.title.text}${F.value.style.chart.title.subtitle.text ? `: ${F.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					headerColor: F.value.table.th.color,
					headerBg: F.value.table.th.backgroundColor,
					isFullscreen: P.value,
					fullscreenParent: A.value,
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
		Ye(() => V.value.showTable, (e) => {
			F.value.table.show || (e && F.value.table.useDialog && $t.value ? $t.value.open() : "close" in $t.value && $t.value.close());
		});
		function rr() {
			V.value.showTable = !1, en.value && en.value.setTableIconState(!1);
		}
		let ir = p(() => F.value.style.chart.backgroundColor), ar = p(() => F.value.style.chart.legend), or = p(() => F.value.style.chart.title), sr = p(() => Un.value.flat().map((e) => ({
			...e,
			name: e.svgDisplay
		}))), { isCallbackImaging: cr, isCallbackSvg: lr, generateSvg: ur, onGenerateImage: dr } = xe({
			svg: R,
			title: or,
			legend: ar,
			legendItems: sr,
			backgroundColor: ir,
			getSvgCallback: () => F.value.userOptions.callbacks.svg,
			generateImage: Sn
		});
		async function fr() {
			if (U("copyAlt", {
				config: F.value,
				dataset: J.value
			}), !F.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(F.value.userOptions.callbacks.altCopy({
				config: F.value,
				dataset: J.value
			}));
		}
		let $ = p(() => X.value.flatMap((e) => e.donut.filter((e) => !e.ghost)));
		function pr() {
			N.value = null, rn.value = !0;
		}
		function mr() {
			N.value = null, nn.value = "pointer", Bt.value = !1, Fn.value = null, Ht.value = null, In.value = null, Z.value = null, rn.value = !1;
		}
		function hr(e) {
			if (!R.value || Zn.value || document.activeElement !== R.value || !$.value.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				N.value = null, nn.value = "pointer", Bt.value = !1, Fn.value = null, Ht.value = null, In.value = null, Z.value = null;
				return;
			}
			if (r) {
				if (N.value === null) return;
				let e = $.value[N.value];
				if (!e) return;
				Tn({
					datapoint: e,
					index: N.value,
					seriesIndex: e.seriesIndex
				});
				return;
			}
			let a = N.value, o = Z.value, s = a !== null && a >= 0 && a < $.value.length, c = o !== null && o >= 0 && o < $.value.length;
			s ? n ? (a += 1, a >= $.value.length && (a = 0)) : t && (--a, a < 0 && (a = $.value.length - 1)) : c ? (a = n ? o + 1 : o - 1, a >= $.value.length && (a = 0), a < 0 && (a = $.value.length - 1)) : a = n ? 0 : $.value.length - 1;
			let l = $.value[a];
			l && (N.value = a, gr(), Bn({
				datapoint: l,
				relativeIndex: a,
				seriesIndex: l.seriesIndex,
				show: !0,
				triggerMode: "keyboard"
			}));
		}
		function gr() {
			if (!R.value) return;
			let e = R.value.getBoundingClientRect();
			tn.value = {
				x: e.left + e.width / 2,
				y: e.top + e.height / 2
			};
		}
		let _r = p(() => ({
			headers: Kn.value?.colNames ?? [],
			rows: Kn.value?.a11yBody ?? []
		}));
		return we({
			autoSize: tr,
			getData: qn,
			getImage: $n,
			generatePdf: xn,
			generateCsv: Gn,
			generateImage: Sn,
			generateSvg: ur,
			hideSeries: jn,
			showSeries: An,
			toggleTable: Jn,
			toggleLabels: Yn,
			toggleTooltip: Xn,
			toggleAnnotator: Qn,
			toggleFullscreen: an,
			copyAlt: fr
		}), (e, t) => (x(), g("div", {
			ref_key: "nestedDonutsChart",
			ref: A,
			class: Ve(`vue-data-ui-component vue-ui-nested-donuts ${P.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${F.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: He(`font-family:${F.value.style.fontFamily};width:100%; text-align:center;background:${F.value.style.chart.backgroundColor}`),
			id: `nested_donuts_${k.value}`,
			onMouseenter: t[2] ||= () => E(dn)(!0),
			onMouseleave: t[3] ||= () => E(dn)(!1)
		}, [
			_("div", {
				id: `chart-instructions-${k.value}`,
				class: "sr-only"
			}, [_("p", null, T(F.value.a11y.translations.keyboardNavigation), 1)], 8, $e),
			_r.value?.rows?.length ? (x(), m(ke, {
				key: 0,
				uid: k.value,
				head: _r.value.headers,
				body: _r.value.rows,
				notice: F.value.a11y.translations.tableAvailable,
				caption: F.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : h("", !0),
			F.value.userOptions.buttons.annotator ? (x(), m(E(Nt), {
				key: 1,
				svgRef: E(R),
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				active: Zn.value,
				isCursorPointer: I.value,
				onClose: Qn
			}, {
				"annotator-action-close": D(() => [w(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": D(({ color: t }) => [w(e.$slots, "annotator-action-color", b(y({ color: t })), void 0, !0)]),
				"annotator-action-draw": D(({ mode: t }) => [w(e.$slots, "annotator-action-draw", b(y({ mode: t })), void 0, !0)]),
				"annotator-action-undo": D(({ disabled: t }) => [w(e.$slots, "annotator-action-undo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": D(({ disabled: t }) => [w(e.$slots, "annotator-action-redo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": D(({ disabled: t }) => [w(e.$slots, "annotator-action-delete", b(y({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : h("", !0),
			Cn.value ? (x(), g("div", {
				key: 2,
				ref_key: "noTitle",
				ref: qt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : h("", !0),
			F.value.style.chart.title.text ? (x(), g("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Wt
			}, [(x(), m(Ce, {
				key: `title_${Jt.value}`,
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
			_("div", { id: `legend-top-${k.value}` }, null, 8, et),
			F.value.userOptions.show && zt.value && (E(fn) || E(un)) ? (x(), m(E(Mt), {
				ref_key: "userOptionsRef",
				ref: en,
				key: `user_option_${Ut.value}`,
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				isPrinting: E(yn),
				isImaging: E(bn),
				uid: k.value,
				hasTooltip: F.value.userOptions.buttons.tooltip && F.value.style.chart.tooltip.show,
				hasPdf: F.value.userOptions.buttons.pdf,
				hasXls: F.value.userOptions.buttons.csv,
				hasImg: F.value.userOptions.buttons.img,
				hasSvg: F.value.userOptions.buttons.svg,
				hasTable: F.value.userOptions.buttons.table,
				hasLabel: F.value.userOptions.buttons.labels,
				hasFullscreen: F.value.userOptions.buttons.fullscreen,
				hasAltCopy: F.value.userOptions.buttons.altCopy,
				isFullscreen: P.value,
				isTooltip: V.value.showTooltip,
				titles: { ...F.value.userOptions.buttonTitles },
				chartElement: A.value,
				position: F.value.userOptions.position,
				hasAnnotator: F.value.userOptions.buttons.annotator,
				isAnnotation: Zn.value,
				callbacks: F.value.userOptions.callbacks,
				printScale: F.value.userOptions.print.scale,
				tableDialog: F.value.table.useDialog,
				isCursorPointer: I.value,
				onToggleFullscreen: an,
				onGeneratePdf: E(xn),
				onGenerateCsv: Gn,
				onGenerateImage: E(dr),
				onGenerateSvg: E(ur),
				onToggleTable: Jn,
				onToggleLabels: Yn,
				onToggleTooltip: Xn,
				onToggleAnnotator: Qn,
				onCopyAlt: fr,
				style: He({ visibility: E(fn) ? E(un) ? "visible" : "hidden" : "visible" })
			}, Ie({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: D(({ isOpen: t, color: n }) => [w(e.$slots, "menuIcon", b(y({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: D(() => [w(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: D(() => [w(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: D(() => [w(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: D(() => [w(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: D(() => [w(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: D(() => [w(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionLabels ? {
					name: "optionLabels",
					fn: D(() => [w(e.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: D(({ toggleFullscreen: t, isFullscreen: n }) => [w(e.$slots, "optionFullscreen", b(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: D(({ toggleAnnotator: t, isAnnotator: n }) => [w(e.$slots, "optionAnnotator", b(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: D(({ altCopy: t }) => [w(e.$slots, "optionAltCopy", b(y({ altCopy: t })), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: D(() => [w(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: D(() => [w(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasLabel.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : h("", !0),
			_("div", tt, [(x(), g("svg", {
				ref_key: "svgRef",
				ref: R,
				xmlns: E(le),
				"aria-describedby": `chart-instructions-${k.value}`,
				class: Ve({
					"vue-data-ui-fullscreen--on": P.value,
					"vue-data-ui-fulscreen--off": !P.value,
					"vue-data-ui-svg": !0
				}),
				viewBox: `0 0 ${H.value.width <= 0 ? .001 : H.value.width} ${H.value.height < 0 ? .001 : H.value.height}`,
				style: He(`max-width:100%; overflow: visible; background:transparent;color:${F.value.style.chart.color};${mn.value.css}`),
				tabindex: "0",
				onFocus: pr,
				onBlur: mr,
				onKeydown: hr
			}, [_("g", {
				ref_key: "G",
				ref: er,
				class: "vue-data-ui-g"
			}, [
				Re(E(Pt)),
				e.$slots["chart-background"] ? (x(), g("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: H.value.width <= 0 ? .001 : H.value.width,
					height: H.value.height < 0 ? .001 : H.value.height,
					style: { pointerEvents: "none" }
				}, [w(e.$slots, "chart-background", {}, void 0, !0)], 8, rt)) : h("", !0),
				_("defs", null, [(x(!0), g(f, null, C(Pn.value, (e, t) => (x(), m(De, {
					t: "radial",
					id: `radial_${k.value}_${t}`,
					key: `radial_${k.value}_${t}`,
					stops: [
						[
							"0%",
							"#FFFFFF",
							0
						],
						[
							`${(1 - Mn.value / Nn.value[t]) * 100}%`,
							E(n)("#FFFFFF", 0),
							0
						],
						[
							`${(1 - Mn.value / Nn.value[t] / 2) * 100}%`,
							"#FFFFFF",
							F.value.style.chart.gradientIntensity / 100
						],
						[
							"100%",
							"#FFFFFF",
							0
						]
					]
				}, null, 8, ["id", "stops"]))), 128))]),
				_("defs", null, [_("filter", {
					id: `blur_${k.value}`,
					x: "-50%",
					y: "-50%",
					width: "200%",
					height: "200%"
				}, [_("feGaussianBlur", {
					in: "SourceGraphic",
					stdDeviation: 2,
					id: `blur_std_${k.value}`
				}, null, 8, at), t[4] ||= _("feColorMatrix", {
					type: "saturate",
					values: "0"
				}, null, -1)], 8, it), _("filter", {
					id: `shadow_${k.value}`,
					"color-interpolation-filters": "sRGB"
				}, [_("feDropShadow", {
					dx: "0",
					dy: "0",
					stdDeviation: "10",
					"flood-opacity": "0.5",
					"flood-color": F.value.style.chart.layout.donut.shadowColor
				}, null, 8, st)], 8, ot)]),
				_("defs", null, [(x(!0), g(f, null, C(X.value, (e, t) => (x(), g("path", {
					key: `path-full-${t}`,
					id: `path-full-${t}-${k.value}`,
					d: e.fullCirclePath,
					fill: "none"
				}, null, 8, ct))), 128))]),
				(x(!0), g(f, null, C(X.value, (e, t) => (x(), g("g", null, [e.hasData ? (x(!0), g(f, { key: 0 }, C(e.donut.filter((e) => !e.ghost), (e, t) => (x(), g("g", null, [_("path", {
					class: "vue-ui-donut-arc-path",
					d: e.arcSlice,
					fill: e.color,
					stroke: F.value.style.chart.layout.donut.borderColorAuto ? F.value.style.chart.backgroundColor : F.value.style.chart.layout.donut.borderColor,
					"stroke-width": F.value.style.chart.layout.donut.borderWidth,
					filter: Hn(e, t)
				}, null, 8, lt)]))), 256)) : (x(!0), g(f, { key: 1 }, C(e.skeleton, (e, t) => (x(), g("g", null, [_("path", {
					class: "vue-ui-donut-arc-path",
					d: e.arcSlice,
					fill: e.color,
					stroke: F.value.style.chart.layout.donut.borderColorAuto ? F.value.style.chart.backgroundColor : F.value.style.chart.layout.donut.borderColor,
					"stroke-width": F.value.style.chart.layout.donut.borderWidth
				}, null, 8, ut)]))), 256))]))), 256)),
				F.value.style.chart.useGradient ? (x(), g("g", dt, [(x(!0), g(f, null, C(Pn.value, (e, t) => (x(), g("g", null, [_("path", {
					d: e.donut.arcSlice,
					fill: `url(#radial_${k.value}_${t})`,
					stroke: "transparent",
					"stroke-width": "0"
				}, null, 8, ft)]))), 256))])) : h("", !0),
				F.value.style.chart.layout.labels.dataLabels.showDonutName ? (x(), g("g", pt, [F.value.style.chart.layout.labels.dataLabels.curvedDonutName ? (x(!0), g(f, { key: 0 }, C(X.value, (e, t) => (x(), g("g", null, [(x(!0), g(f, null, C(e.donut, (n, r) => (x(), g("g", null, [r === 0 && H.value.width && H.value.height ? (x(), g("text", {
					key: 0,
					"text-anchor": "middle",
					"font-size": F.value.style.chart.layout.labels.dataLabels.fontSize,
					"font-weight": F.value.style.chart.layout.labels.dataLabels.boldDonutName ? "bold" : "normal",
					fill: F.value.style.chart.layout.labels.dataLabels.color,
					dy: F.value.style.chart.layout.labels.dataLabels.donutNameOffsetY
				}, [_("textPath", {
					href: `#path-full-${t}-${k.value}`,
					startOffset: "50%",
					"text-anchor": "middle",
					method: "align",
					spacing: "auto"
				}, T(F.value.style.chart.layout.labels.dataLabels.donutNameAbbreviation ? E(oe)({
					source: e.name,
					length: F.value.style.chart.layout.labels.dataLabels.donutNameMaxAbbreviationSize
				}) : e.name), 9, ht)], 8, mt)) : h("", !0)]))), 256))]))), 256)) : (x(!0), g(f, { key: 1 }, C(X.value, (e, t) => (x(), g("g", null, [(x(!0), g(f, null, C(e.donut, (t, n) => (x(), g("g", null, [n === 0 && H.value.width && H.value.height ? (x(), g("text", {
					key: 0,
					x: H.value.width / 2,
					y: t.startY - F.value.style.chart.layout.labels.dataLabels.fontSize + F.value.style.chart.layout.labels.dataLabels.donutNameOffsetY,
					"text-anchor": "middle",
					"font-size": F.value.style.chart.layout.labels.dataLabels.fontSize,
					"font-weight": F.value.style.chart.layout.labels.dataLabels.boldDonutName ? "bold" : "normal",
					fill: F.value.style.chart.layout.labels.dataLabels.color
				}, T(F.value.style.chart.layout.labels.dataLabels.donutNameAbbreviation ? E(oe)({
					source: e.name,
					length: F.value.style.chart.layout.labels.dataLabels.donutNameMaxAbbreviationSize
				}) : e.name), 9, gt)) : h("", !0)]))), 256))]))), 256))])) : h("", !0),
				F.value.style.chart.layout.labels.dataLabels.show ? (x(), g("g", _t, [(x(!0), g(f, null, C(X.value, (e, t) => (x(), g("g", null, [(x(!0), g(f, null, C(e.donut.filter((e) => !e.ghost), (e, n) => (x(), g("g", { filter: Hn(e, n) }, [Xe(_("text", {
					opacity: +!!Vn(e),
					"text-anchor": E(re)(e, !0).anchor,
					x: E(re)(e, !1, F.value.style.chart.layout.labels.dataLabels.offsetX).x || 0,
					y: E(se)(e, F.value.style.chart.layout.labels.dataLabels.offsetY, F.value.style.chart.layout.labels.dataLabels.offsetY) + (F.value.style.chart.layout.labels.dataLabels.showValueFirst && F.value.style.chart.layout.labels.dataLabels.showValue ? F.value.style.chart.layout.labels.dataLabels.fontSize : 0),
					fill: F.value.style.chart.layout.labels.dataLabels.useSerieColor ? e.color : F.value.style.chart.layout.labels.dataLabels.color,
					"font-size": F.value.style.chart.layout.labels.dataLabels.fontSize,
					"font-weight": F.value.style.chart.layout.labels.dataLabels.boldPercentage ? "bold" : "normal"
				}, T(E(i)(E(u)({
					v: e.proportion * 100,
					s: "%",
					r: F.value.style.chart.layout.labels.dataLabels.roundingPercentage
				}), F.value.style.chart.layout.labels.dataLabels.usePercentageParens ? "(" : "", F.value.style.chart.layout.labels.dataLabels.usePercentageParens ? ")" : "")), 9, yt), [[Je, V.value.dataLabels.show && F.value.style.chart.layout.labels.dataLabels.showPercentage]]), Xe(_("text", {
					opacity: +!!Vn(e),
					"text-anchor": E(re)(e, !0).anchor,
					x: E(re)(e, !1, F.value.style.chart.layout.labels.dataLabels.offsetX).x || 0,
					y: E(se)(e, F.value.style.chart.layout.labels.dataLabels.offsetY, F.value.style.chart.layout.labels.dataLabels.offsetY) + (F.value.style.chart.layout.labels.dataLabels.showValueFirst || !F.value.style.chart.layout.labels.dataLabels.showPercentage ? 0 : F.value.style.chart.layout.labels.dataLabels.fontSize),
					fill: F.value.style.chart.layout.labels.dataLabels.useSerieColor ? e.color : F.value.style.chart.layout.labels.dataLabels.color,
					"font-size": F.value.style.chart.layout.labels.dataLabels.fontSize,
					"font-weight": F.value.style.chart.layout.labels.dataLabels.boldValue ? "bold" : "normal"
				}, T(E(i)(E(d)(F.value.style.chart.layout.labels.dataLabels.formatter, e.value, E(u)({
					p: F.value.style.chart.layout.labels.dataLabels.prefix,
					v: e.value,
					s: F.value.style.chart.layout.labels.dataLabels.suffix,
					r: F.value.style.chart.layout.labels.dataLabels.roundingValue
				}), {
					datapoint: e,
					seriesIndex: t,
					datapointIndex: n
				}), F.value.style.chart.layout.labels.dataLabels.useValueParens ? "(" : "", F.value.style.chart.layout.labels.dataLabels.useValueParens ? ")" : "")), 9, bt), [[Je, V.value.dataLabels.show && F.value.style.chart.layout.labels.dataLabels.showValue]])], 8, vt))), 256))]))), 256))])) : h("", !0),
				(x(!0), g(f, null, C(X.value, (e, t) => (x(), g("g", null, [(x(!0), g(f, null, C(e.donut, (e, t) => (x(), g("g", null, [_("path", {
					d: e.arcSlice,
					fill: Ht.value === e.id ? F.value.style.chart.layout.donut.selectedColor : "transparent",
					onMouseenter: (t) => Bn({
						datapoint: e,
						relativeIndex: $.value.findIndex((t) => t.id === e.id),
						seriesIndex: e.seriesIndex
					}),
					onClick: (n) => Tn({
						datapoint: e,
						index: t,
						seriesIndex: e.seriesIndex
					}),
					onMouseleave: (t) => Rn({
						datapoint: e,
						seriesIndex: e.seriesIndex
					})
				}, null, 40, xt)]))), 256))]))), 256)),
				w(e.$slots, "svg", { svg: {
					...H.value,
					isPrintingImg: E(yn) || E(bn) || E(cr),
					isPrintingSvg: E(lr)
				} }, void 0, !0)
			], 512)], 46, nt)), e.$slots.hint ? (x(), g("div", St, [w(e.$slots, "hint", b(y({
				hint: F.value.a11y.translations.keyboardNavigation,
				isVisible: rn.value
			})), void 0, !0)])) : h("", !0)]),
			e.$slots.watermark ? (x(), g("div", Ct, [w(e.$slots, "watermark", b(y({ isPrinting: E(yn) || E(bn) || E(cr) || E(lr) })), void 0, !0)])) : h("", !0),
			Re(E(Ot), {
				teleportTo: F.value.style.chart.tooltip.teleportTo,
				show: V.value.showTooltip && Bt.value,
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
				parent: A.value,
				content: Vt.value,
				isFullscreen: P.value,
				isCustom: E(fe)(F.value.style.chart.tooltip.customFormat),
				smooth: F.value.style.chart.tooltip.smooth,
				backdropFilter: F.value.style.chart.tooltip.backdropFilter,
				smoothForce: F.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: F.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: nn.value === "keyboard",
				a11yPosition: tn.value
			}, {
				"tooltip-before": D(() => [w(e.$slots, "tooltip-before", b(y({ ...Ln.value })), void 0, !0)]),
				tooltip: D(() => [w(e.$slots, "tooltip", b(y({ ...Ln.value })), void 0, !0)]),
				"tooltip-after": D(() => [w(e.$slots, "tooltip-after", b(y({ ...Ln.value })), void 0, !0)]),
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
			_("div", { id: `legend-bottom-${k.value}` }, null, 8, wt),
			Qt.value && (F.value.style.chart.legend.show || e.$slots.legend) ? (x(), m(Fe, {
				key: 6,
				to: F.value.style.chart.legend.position === "top" ? `#legend-top-${k.value}` : `#legend-bottom-${k.value}`
			}, [F.value.style.chart.legend.show ? (x(), g("div", {
				key: 0,
				ref_key: "chartLegend",
				ref: Gt,
				class: Ve({ "vue-ui-nested-donuts-legend": Un.value.length > 1 })
			}, [w(e.$slots, "legend", { legend: Un.value }, () => [(x(!0), g(f, null, C(Un.value, (e, n) => (x(), m(Ne, {
				key: `legend_${n}_${Xt.value}`,
				legendSet: e,
				config: Wn.value,
				isCursorPointer: I.value,
				onClickMarker: t[0] ||= ({ legend: e }) => Y(e)
			}, {
				legendTitle: D(({ titleSet: e }) => [e[0] && e[0].arcOf ? (x(), g("div", Tt, T(e[0].arcOf), 1)) : h("", !0)]),
				item: D(({ legend: e, index: t }) => [_("div", {
					onClick: (t) => Y(e),
					style: He(`opacity:${W.value.includes(e.id) ? .5 : 1}`)
				}, T(e.display), 13, Et)]),
				legendToggle: D(() => [e.length > 2 && F.value.style.chart.legend.selectAllToggle.show && !E(cn) ? (x(), m(Oe, {
					key: `toggle-${n}`,
					backgroundColor: F.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: F.value.style.chart.legend.selectAllToggle.color,
					fontSize: F.value.style.chart.legend.fontSize,
					checked: e.some((e) => W.value.includes(e.id)),
					isCursorPointer: I.value,
					onToggle: (t) => On(e)
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer",
					"onToggle"
				])) : h("", !0)]),
				_: 2
			}, 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			]))), 128))], !0)], 2)) : h("", !0)], 8, ["to"])) : h("", !0),
			e.$slots.source ? (x(), g("div", {
				key: 7,
				ref_key: "source",
				ref: Kt,
				dir: "auto"
			}, [w(e.$slots, "source", {}, void 0, !0)], 512)) : h("", !0),
			zt.value && F.value.userOptions.buttons.table ? (x(), m(Ge(nr.value.component), ze({ key: 8 }, nr.value.props, {
				ref_key: "tableUnit",
				ref: $t,
				onClose: rr
			}), Ie({
				content: D(() => [(x(), m(E(jt), {
					key: `table_${Yt.value}`,
					colNames: Kn.value.colNames,
					head: Kn.value.head,
					body: Kn.value.body,
					config: Kn.value.config,
					title: F.value.table.useDialog ? "" : nr.value.title,
					withCloseButton: !F.value.table.useDialog,
					isCursorPointer: I.value,
					onClose: rr
				}, {
					th: D(({ th: e }) => [_("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, Dt)]),
					td: D(({ td: e }) => [Le(T(e.name || e), 1)]),
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
				fn: D(() => [Le(T(nr.value.title), 1)]),
				key: "0"
			} : void 0, F.value.table.useDialog ? {
				name: "actions",
				fn: D(() => [_("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Gn(F.value.userOptions.callbacks.csv),
					style: He({ cursor: I.value ? "pointer" : "default" })
				}, [Re(E(kt), {
					name: "fileCsv",
					stroke: nr.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : h("", !0),
			w(e.$slots, "skeleton", {}, () => [E(cn) ? (x(), m(ve, { key: 0 })) : h("", !0)], !0)
		], 46, Qe));
	}
}, [["__scopeId", "data-v-d80ff98e"]]);
//#endregion
export { Ze as n, Ot as t };
