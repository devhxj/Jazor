import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Jt as i, K as a, Kt as o, Mt as s, Pt as c, S as l, Vt as u, X as d, at as ee, b as te, et as f, f as p, h as ne, i as m, j as h, jt as re, kt as ie, ot as ae, p as g, pt as oe, q as se, s as ce, t as le, tt as ue, w as de, xt as fe } from "./lib-Bttd6u5E.js";
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
import { n as Ne, t as Pe } from "./labelUtils-BeVpDvTJ.js";
import { t as Fe } from "./vue_ui_donut-BDGqG07h.js";
import { t as Ie } from "./Legend-CQxUgOd-.js";
import { Fragment as _, Teleport as Le, computed as v, createBlock as y, createCommentVNode as b, createElementBlock as x, createElementVNode as S, createSlots as Re, createTextVNode as ze, createVNode as Be, defineAsyncComponent as Ve, guardReactiveProps as C, mergeProps as He, nextTick as Ue, normalizeClass as We, normalizeProps as w, normalizeStyle as T, onBeforeUnmount as Ge, onMounted as Ke, openBlock as E, ref as D, renderList as O, renderSlot as k, resolveDynamicComponent as qe, shallowRef as Je, toDisplayString as Ye, toRefs as Xe, unref as A, useSlots as Ze, vShow as Qe, watch as $e, withCtx as j, withDirectives as et } from "vue";
//#region src/useSmallArcLayouts.js
function tt(e) {
	let { FINAL_CONFIG: t, noGhostDonut: n, svg: r, padding: i, labels_inline_fontSize: a, minSize: o, findArcMidpoint: s, calcMarkerOffsetX: c, calcMarkerOffsetY: l, animatingIndex: u, segregated: d, isSmallArc: ee } = e;
	return { smallArcLayoutsClassic: v(() => {
		if (t.value.type !== "classic") return {};
		let e = {}, te = n.value || [];
		if (!te.length) return e;
		let f = r.value.width / 2, p = r.value.height / 2, ne = i.value.top + 16, m = r.value.height - i.value.bottom - 16, h = a.value, re = h / 3, ie = h * 1.5, ae = f - (o.value + 6), g = f + (o.value + 6), oe = !!t.value.style.chart.layout.curvedMarkers;
		function se({ midX: e, midY: t, bandX: n, bandY: r }) {
			if (!oe) return `M ${e} ${t} L ${e} ${r} L ${n} ${r}`;
			let i = n < f ? -1 : 1, a = n - e, o = r - t, s = Math.sqrt(a * a + o * o) || 1, c = e - f, l = t - p, u = Math.sqrt(c * c + l * l) || 1, d = c / u, ee = l / u, te = u + 9;
			function ne({ x: e, y: t }) {
				let n = e - f, r = t - p, i = Math.sqrt(n * n + r * r) || 1;
				if (i >= te) return {
					x: e,
					y: t
				};
				let a = te / i;
				return {
					x: f + n * a,
					y: p + r * a
				};
			}
			if (s < 56) {
				let i = a / s, c = -(o / s), l = i, u = (e + n) * .5, te = (t + r) * .5, m = u + c, h = te + l, re = (m - f) * (m - f) + (h - p) * (h - p), ie = u - c, ae = te - l;
				(ie - f) * (ie - f) + (ae - p) * (ae - p) > re && (c = -c, l = -l);
				let g = .78, oe = e + a * g, se = t + o * g, ce = Math.max(0, Math.min(1, (s - 18) / 44)), le = ce * ce * (3 - 2 * ce), ue = 2.5 + le * 4, de = 1 + le * 2.5;
				ue *= .9;
				let fe = ne({
					x: oe + c * ue + d * de,
					y: se + l * ue + ee * de
				});
				return `M ${e} ${t} Q ${fe.x} ${fe.y} ${n} ${r}`;
			}
			let m = s * .34;
			m < 20 && (m = 20), m > 46 && (m = 46);
			let h = s * .46;
			h < 22 && (h = 22), h > 70 && (h = 70);
			let re = {
				x: e + d * m,
				y: t + ee * m
			}, ie = f + i * Math.max(Math.abs(n - f), te), ae = {
				x: n - i * Math.min(h, Math.abs(ie - n) * .75),
				y: r
			}, g = ne(re), se = ne(ae);
			return `M ${e} ${t} C ${g.x} ${g.y} ${se.x} ${se.y} ${n} ${r}`;
		}
		function ce(e) {
			let t = String(e ?? "").split(/\n/g), n = Math.max(0, t.length - 1) * (h * 1.2);
			return ie + n;
		}
		function le({ arc: e, index: t }) {
			let { x: n, y: r } = s(e.path);
			return {
				arc: e,
				index: t,
				midX: n,
				midY: r,
				inlineMarkerX: c(e).x,
				inlineMarkerY: l(e) - 3.5,
				labelHeight: ce(e.name)
			};
		}
		function ue(e) {
			let { arc: t } = e, n = t.seriesIndex ?? 0;
			return u.value === n || d.value.includes(n) ? !1 : ee(t, n);
		}
		function de(e) {
			let t = e.inlineMarkerY < p, n = e.inlineMarkerX < f;
			return t && n ? "TL" : t && !n ? "TR" : !t && n ? "BL" : "BR";
		}
		function fe(e, t) {
			if (t.startsWith("T")) {
				e.sort((e, t) => e.inlineMarkerY - t.inlineMarkerY || e.index - t.index);
				return;
			}
			e.sort((e, t) => t.inlineMarkerY - e.inlineMarkerY || e.index - t.index);
		}
		function pe({ side: e, markerX: t, markerY: n, labelY: r, connectorPath: i }) {
			return {
				side: e,
				labelX: e === "left" ? t - 8 : t + 8,
				labelY: r + re,
				textAnchor: e === "left" ? "end" : "start",
				markerX: t,
				markerY: n,
				connectorPath: i
			};
		}
		function me({ candidateList: t, side: n, bandMarkerX: r, startY: i, direction: a }) {
			let o = i;
			t.forEach((t) => {
				let { index: i, midX: s, midY: c, labelHeight: l } = t, u;
				a === "down" ? (u = o, o += l) : (o -= l, u = o);
				let d = u, ee = se({
					midX: s,
					midY: c,
					bandX: r,
					bandY: d
				});
				e[i] = pe({
					side: n,
					markerX: r,
					markerY: d,
					labelY: u,
					connectorPath: ee
				});
			});
		}
		let he = te.map((e, t) => le({
			arc: e,
			index: t
		})).filter(ue), ge = {
			TL: [],
			TR: [],
			BL: [],
			BR: []
		};
		return he.forEach((e) => {
			ge[de(e)].push(e);
		}), Object.keys(ge).forEach((e) => {
			fe(ge[e], e);
		}), me({
			candidateList: ge.TL,
			side: "left",
			bandMarkerX: ae,
			startY: ne,
			direction: "down"
		}), me({
			candidateList: ge.TR,
			side: "right",
			bandMarkerX: g,
			startY: ne,
			direction: "down"
		}), ge.BL.length > 1 && me({
			candidateList: ge.BL,
			side: "left",
			bandMarkerX: ae,
			startY: m,
			direction: "up"
		}), ge.BR.length > 1 && me({
			candidateList: ge.BR,
			side: "right",
			bandMarkerX: g,
			startY: m,
			direction: "up"
		}), e;
	}) };
}
//#endregion
//#region src/components/vue-ui-donut.vue
var nt = /* @__PURE__ */ e({ default: () => gn }), rt = ["id"], it = ["id"], at = ["id"], ot = { style: { position: "relative" } }, st = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], ct = ["width", "height"], lt = { key: 1 }, ut = { key: 2 }, dt = ["id"], ft = ["id"], pt = ["id"], mt = ["flood-color"], ht = ["id"], gt = ["flood-color"], _t = [
	"d",
	"stroke",
	"filter"
], vt = [
	"d",
	"stroke",
	"filter"
], yt = [
	"cx",
	"cy",
	"r",
	"fill",
	"filter"
], bt = { key: 6 }, xt = ["stroke", "d"], St = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], Ct = { key: 0 }, wt = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], Tt = { key: 0 }, Et = ["stroke", "d"], Dt = { key: 0 }, Ot = [
	"d",
	"stroke",
	"stroke-width",
	"filter"
], kt = { key: 1 }, At = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], jt = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], Mt = { key: 1 }, Nt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Pt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Ft = { key: 0 }, It = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], Lt = [
	"cx",
	"cy",
	"r",
	"stroke"
], Rt = [
	"cx",
	"cy",
	"r",
	"fill"
], zt = [
	"cx",
	"cy",
	"r"
], Bt = { key: 0 }, Vt = [
	"d",
	"stroke",
	"fill",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Ht = { key: 1 }, Ut = [
	"cx",
	"cy",
	"r"
], Wt = [
	"x",
	"y",
	"fill",
	"font-size"
], Gt = [
	"x",
	"y",
	"fill",
	"font-size"
], Kt = [
	"x",
	"y",
	"fill",
	"font-size"
], qt = [
	"x",
	"y",
	"fill",
	"font-size"
], Jt = ["filter", "opacity"], Yt = { key: 0 }, Xt = ["x", "y"], Zt = { key: 1 }, Qt = [
	"cx",
	"cy",
	"fill",
	"stroke",
	"filter",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], $t = [
	"text-anchor",
	"x",
	"y",
	"onClick",
	"onMouseenter",
	"onMouseleave",
	"innerHTML"
], en = [
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], tn = [
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"onClick",
	"onMouseenter",
	"onMouseleave",
	"innerHTML"
], nn = [
	"cx",
	"cy",
	"fill",
	"stroke",
	"filter",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], rn = [
	"text-anchor",
	"x",
	"y",
	"onClick",
	"onMouseenter",
	"onMouseleave",
	"innerHTML"
], an = [
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], on = [
	"text-anchor",
	"x",
	"y",
	"fill",
	"font-size",
	"onClick",
	"onMouseenter",
	"onMouseleave",
	"innerHTML"
], sn = { key: 2 }, cn = [
	"x",
	"y",
	"width"
], ln = [
	"x",
	"y",
	"width"
], un = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, dn = {
	key: 5,
	class: "vue-data-ui-watermark"
}, fn = ["id"], pn = ["onClick"], mn = {
	key: 8,
	class: "vue-ui-donut-hollow"
}, hn = ["innerHTML"], gn = /*#__PURE__*/ Te({
	__name: "vue-ui-donut",
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
	setup(e, { expose: Te, emit: nt }) {
		let gn = Ve(() => import("./Tooltip-DhjyfHwz.js")), _n = Ve(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), vn = Ve(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), yn = Ve(() => import("./DataTable-BbKgJ5UI.js")), bn = Ve(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), xn = Ve(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Sn = Ve(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Cn = Ve(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_donut: wn } = he(), { isThemeValid: Tn, warnInvalidTheme: En } = be(), Dn = Ze(), M = e, On = v({
			get() {
				return !!M.dataset && M.dataset.length;
			},
			set(e) {
				return e;
			}
		}), N = Je(null), kn = D(null), An = D(null), jn = D(null), Mn = Je(null), Nn = Je(null), Pn = Je(null), Fn = Je(null), In = Je(null), Ln = Je(null), Rn = D(0), zn = D(0), Bn = D(0), Vn = D(!1), Hn = D(null), Un = D(null), Wn = D(null), Gn = D({
			x: 0,
			y: 0
		}), Kn = D("pointer"), qn = D(!1), Jn = v({
			get: () => L.value.style.chart.layout.labels.percentage.fontSize,
			set: (e) => e
		}), P = v({
			get: () => L.value.style.chart.layout.labels.name.fontSize,
			set: (e) => e
		}), Yn = v({
			get: () => L.value.style.chart.layout.labels.dataLabels.smallArcClusterFontSize,
			set: (e) => e
		}), Xn = !1, Zn = () => {
			!L.value.autoSize || Xn || (Xn = !0, requestAnimationFrame(() => {
				Xn = !1;
				let e = L.value, t = kn.value, n = z.value;
				if (!e.autoSize || !t || !n) return;
				let [r, i, a, o] = n.getAttribute("viewBox").split(" ").map(Number), s = {
					x: r,
					y: i,
					width: a,
					height: o
				}, c = [
					{
						selector: ".vue-data-ui-datalabel-value",
						baseSize: e.style.chart.layout.labels.percentage.fontSize,
						minSize: e.style.chart.layout.labels.percentage.minFontSize,
						sizeRef: Jn
					},
					{
						selector: ".vue-data-ui-datalabel-name",
						baseSize: e.style.chart.layout.labels.name.fontSize,
						minSize: e.style.chart.layout.labels.name.minFontSize,
						sizeRef: P
					},
					{
						selector: ".vue-data-ui-datalabel-inline",
						baseSize: e.style.chart.layout.labels.dataLabels.smallArcClusterFontSize,
						minSize: e.style.chart.layout.labels.name.minFontSize,
						sizeRef: Yn
					}
				];
				c.map((e) => t.querySelectorAll(e.selector).length).reduce((e, t) => e + t, 0) !== 0 && c.forEach(({ selector: e, baseSize: n, minSize: r, sizeRef: i }) => {
					t.querySelectorAll(e).forEach((e) => {
						i.value = ce({
							el: e,
							bounds: s,
							currentFontSize: n,
							minFontSize: r,
							attempts: 200,
							padding: 1
						});
					});
				});
			}));
		};
		Ke(async () => {
			Vn.value = !0, er(), requestAnimationFrame(Zn);
		});
		let Qn;
		Ke(() => {
			N.value && (Qn = new ResizeObserver((e) => {
				for (let t of e) {
					let { width: e, height: n } = t.contentRect;
					if (e > 0 && n > 0) {
						Zn();
						break;
					}
				}
			}), Qn.observe(N.value.parentElement));
		}), Ge(() => {
			Qn?.disconnect();
		}), Ge(() => {
			Pn.value && (Fn.value && Pn.value.unobserve(Fn.value), Pn.value.disconnect());
		});
		let $n = v(() => L.value.debug);
		function er() {
			if (re(M.dataset) ? (ue({
				componentName: "VueUiDonut",
				type: "dataset",
				debug: $n.value
			}), On.value = !1, lr.value = !0) : (M.dataset.forEach((e, t) => {
				oe({
					datasetObject: e,
					requiredAttributes: ["name", "values"]
				}).forEach((e) => {
					ue({
						componentName: "VueUiDonut",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: $n.value
					}), On.value = !1, lr.value = !0;
				});
			}), $n.value && M.dataset.forEach((e, t) => {
				(!e.name || e.name === "") && ue({
					componentName: "VueUiDonut",
					type: "datasetAttributeEmpty",
					property: "name",
					index: t
				});
			})), re(M.dataset) || (lr.value = L.value.loading), L.value.responsive) {
				let e = Ee(() => {
					let { width: e, height: t } = De({
						chart: N.value,
						title: L.value.style.chart.title.text ? Mn.value : null,
						legend: L.value.style.chart.legend.show ? Nn.value : null,
						source: In.value,
						noTitle: Ln.value,
						padding: L.value.autoSize ? void 0 : br.value
					});
					requestAnimationFrame(() => {
						V.value.width = e, V.value.height = t, Zn();
					});
				});
				Pn.value && (Fn.value && Pn.value.unobserve(Fn.value), Pn.value.disconnect()), Pn.value = new ResizeObserver(e), Fn.value = N.value.parentNode, Pn.value.observe(Fn.value);
			}
		}
		let F = D(se()), tr = D(!1), nr = D(""), I = D(null), rr = D(0);
		function ir() {
			let e = ye({
				userConfig: M.config,
				defaultConfig: wn
			}), t = {}, n = e.theme;
			if (!n) t = e;
			else if (!Tn.value(e)) En(e), t = e;
			else {
				let r = ye({
					userConfig: Fe[n] || M.config,
					defaultConfig: e
				});
				t = {
					...ye({
						userConfig: M.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : o[n] || c
				};
			}
			return t;
		}
		let L = D(ir());
		pe({
			config: () => L.value,
			dataset: () => M.dataset,
			component: "VueUiDonut",
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
		let ar = v(() => L.value.userOptions.useCursorPointer), or = v(() => i({
			defaultConfig: {
				useCssAnimation: !1,
				table: { show: !1 },
				startAnimation: { show: !1 },
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: { labels: {
						dataLabels: { show: !1 },
						hollow: {
							average: { show: !1 },
							total: { show: !1 }
						},
						value: { show: !1 }
					} },
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
			userConfig: L.value.skeletonConfig ?? {}
		})), { loading: sr, FINAL_DATASET: cr, manualLoading: lr, skeletonDataset: ur } = _e({
			...Xe(M),
			FINAL_CONFIG: L,
			prepareConfig: ir,
			skeletonDataset: M.config?.skeletonDataset ?? [
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
			],
			skeletonConfig: i({
				defaultConfig: L.value,
				userConfig: or.value
			})
		}), R = D(!0), dr = D(0), fr = D(!1), pr = null;
		function mr(e = 1e3) {
			return new Promise((t) => {
				let n = performance.now();
				function r(i) {
					let a = i - n, o = Math.min(a / e, 1), s = f(o);
					dr.value = s, o < 1 ? pr = requestAnimationFrame(r) : (dr.value = 1, pr = null, t());
				}
				pr !== null && cancelAnimationFrame(pr), dr.value = 0, pr = requestAnimationFrame(r);
			});
		}
		$e(() => sr.value, async (e) => {
			if (e || fr.value) return;
			let t = cr.value === ur, n = L.value.startAnimation?.show;
			!t && n ? (fr.value = !0, await mr(L.value.startAnimation.durationMs || 1e3)) : dr.value = 1, R.value = !1;
		}, { immediate: !0 });
		let { userOptionsVisible: hr, setUserOptionsVisibility: gr, keepUserOptionState: _r } = je({ config: L.value }), { svgRef: z } = Me({ config: L.value.style.chart.title });
		function vr() {
			gr(!0);
		}
		function yr() {
			gr(!1);
		}
		$e(() => M.config, (e) => {
			sr.value || (L.value = ir()), hr.value = !L.value.userOptions.showOnChartHover, er(), Rn.value += 1, zn.value += 1, Bn.value += 1, B.value.dataLabels.show = L.value.style.chart.layout.labels.dataLabels.show, B.value.showTable = L.value.table.show, B.value.showTooltip = L.value.style.chart.tooltip.show, V.value.height = L.value.style.chart.height, V.value.width = L.value.style.chart.width;
		}, { deep: !0 });
		let br = v(() => {
			let { top: e, right: t, bottom: n, left: r } = L.value.style.chart.padding;
			return {
				css: `padding:${e}px ${t}px ${n}px ${r}px`,
				top: e,
				right: t,
				bottom: n,
				left: r
			};
		}), { isPrinting: xr, isImaging: Sr, generatePdf: Cr, generateImage: wr } = ge({
			elementId: `donut__${F.value}`,
			fileName: L.value.style.chart.title.text || "vue-ui-donut",
			options: L.value.userOptions.print
		}), Tr = v(() => L.value.userOptions.show && !L.value.style.chart.title.text), Er = v(() => de(L.value.customPalette)), B = D({
			dataLabels: { show: L.value.style.chart.layout.labels.dataLabels.show },
			showTable: L.value.table.show,
			showTooltip: L.value.style.chart.tooltip.show
		});
		$e(L, () => {
			B.value = {
				dataLabels: { show: L.value.style.chart.layout.labels.dataLabels.show },
				showTable: L.value.table.show,
				showTooltip: L.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let V = D({
			height: L.value.style.chart.height,
			width: L.value.style.chart.width
		}), Dr = v(() => {
			if (L.value.pie) return G.value;
			let e = L.value.style.chart.layout.donut.strokeWidth / 512, t = Math.min(V.value.width, V.value.height) * e, n = t > G.value ? G.value : t;
			return Math.max(n, 12 * (1 + e));
		}), Or = nt, H = v(() => cr.value.sort((e, t) => {
			let n = Array.isArray(e.values) ? e.values.reduce((e, t) => e + t, 0) : e.value ?? 0, r = Array.isArray(t.values) ? t.values.reduce((e, t) => e + t, 0) : t.value ?? 0;
			return e.ghost && !t.ghost ? 1 : t.ghost && !e.ghost ? -1 : r - n;
		}).map((e, t) => ({
			name: e.name,
			color: l(e.color) || Er.value[t] || c[t] || c[t % c.length],
			value: te(e.values.reduce((e, t) => e + t, 0)),
			absoluteValues: e.values,
			comment: e.comment || "",
			patternIndex: t,
			seriesIndex: t,
			ghost: !1,
			pattern: `pattern_${F.value}_${t}`
		})));
		$e(() => M.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (lr.value = !1);
		}, { immediate: !0 });
		let U = Je(H.value);
		$e(() => H.value, (e) => {
			U.value = e, requestAnimationFrame(Zn);
		});
		function kr() {
			return H.value.map((e) => ({
				name: e.name,
				color: e.color,
				value: e.value
			}));
		}
		let W = D([]), Ar = D(!1), jr = D(null);
		function Mr() {
			Or("selectLegend", Br.value.map((e) => ({
				name: e.name,
				color: e.color,
				value: e.value
			})));
		}
		function Nr({ from: e, to: t, duration: n, onUpdate: r, onDone: i, easing: a = f }) {
			let o = performance.now();
			function s(c) {
				let l = Math.min((c - o) / n, 1), u = a(l);
				r(e + (t - e) * u, l), l < 1 ? requestAnimationFrame(s) : (r(t, 1), i && i());
			}
			requestAnimationFrame(s);
		}
		function Pr() {
			W.value.length ? W.value = [] : Vr.value.forEach((e, t) => {
				W.value.push(t);
			}), Mr();
		}
		function Fr(e) {
			let t = H.value.find((t, n) => n === e), n = U.value.find((t, n) => n === e).value;
			if (W.value.includes(e)) {
				W.value = W.value.filter((t) => t !== e);
				let r = t.value;
				function i() {
					U.value = U.value.map((t, n) => e === n ? {
						...t,
						value: r
					} : t), Mr();
				}
				function a() {
					Ar.value = !0, jr.value = e, Nr({
						from: n,
						to: r,
						duration: L.value.serieToggleAnimation.durationMs,
						onUpdate: (t, n) => {
							U.value = U.value.map((n, r) => e === r ? {
								...n,
								value: t
							} : n), requestAnimationFrame(Zn);
						},
						onDone: () => {
							i(), Ar.value = !1, jr.value = null;
						}
					});
				}
				L.value.serieToggleAnimation.show && L.value.type === "classic" ? a() : (i(), requestAnimationFrame(Zn));
			} else if (W.value.length < H.value.length - 1) {
				function t() {
					W.value.push(e), U.value = U.value.map((t, n) => e === n ? {
						...t,
						value: 0
					} : t), Mr();
				}
				function r() {
					Ar.value = !0, jr.value = e, Nr({
						from: n,
						to: 0,
						duration: L.value.serieToggleAnimation.durationMs,
						onUpdate: (t, n) => {
							U.value = U.value.map((n, r) => e === r ? {
								...n,
								value: t
							} : n), requestAnimationFrame(Zn);
						},
						onDone: () => {
							t(), requestAnimationFrame(Zn), Ar.value = !1, jr.value = null;
						}
					});
				}
				L.value.serieToggleAnimation.show && L.value.type === "classic" ? r() : t();
			}
		}
		function Ir(e) {
			return H.value.length ? H.value.find((t) => t.name === e) || ($n.value && console.warn(`VueUiDonut - Series name not found "${e}"`), null) : ($n.value && console.warn("VueUiDonut - There are no series to show."), null);
		}
		function Lr(e) {
			let t = Ir(e);
			t !== null && W.value.includes(t.seriesIndex) && Fr(t.seriesIndex);
		}
		function Rr(e) {
			let t = Ir(e);
			t !== null && (W.value.includes(t.seriesIndex) || Fr(t.seriesIndex));
		}
		let zr = v(() => cr.value.reduce((e, t) => e + t.values.reduce((e, t) => e + t, 0), 0)), Br = v(() => {
			if (R.value && !sr.value) {
				let e = dr.value, t = H.value.map((t) => ({
					...t,
					value: t.value * e,
					color: t.color,
					ghost: !1
				})), n = zr.value * (1 - e);
				return n > 0 && t.push({
					name: "__ghost__",
					value: n,
					color: "transparent",
					ghost: !0
				}), t;
			}
			return U.value.forEach((e) => {
				if ([null, void 0].includes(e.values)) return {
					...e,
					values: []
				};
			}), U.value.map((e, t) => ({
				...e,
				seriesIndex: t
			})).filter((e, t) => !W.value.includes(t));
		}), Vr = v(() => cr.value.map((e, t) => {
			let n = (e.values || []).reduce((e, t) => e + t, 0), r = n / cr.value.map((e) => (e.values || []).reduce((e, t) => e + t, 0)).reduce((e, t) => e + t, 0);
			return {
				name: e.name,
				color: l(e.color) || Er.value[t] || c[t] || c[t % c.length],
				value: n,
				shape: "circle",
				patternIndex: t,
				proportion: r
			};
		}).map((e, t) => {
			let n = m(L.value.style.chart.layout.labels.value.formatter, e.value, d({
				p: L.value.style.chart.layout.labels.dataLabels.prefix,
				v: e.value,
				s: L.value.style.chart.layout.labels.dataLabels.suffix,
				r: L.value.style.chart.legend.roundingValue
			}), {
				datapoint: e,
				index: t
			}), r = m(L.value.style.chart.layout.labels.percentage.formatter, Qr(e), d({
				v: Qr(e),
				s: "%",
				r: L.value.style.chart.legend.roundingPercentage
			})), i = mi({
				val: n,
				percentage: W.value.includes(t) ? `${pi(e.proportion * 100)}%` : r,
				showVal: L.value.style.chart.legend.showValue,
				showPercentage: L.value.style.chart.legend.showPercentage,
				config: L.value.style.chart.legend
			});
			return {
				...e,
				opacity: W.value.includes(t) ? .5 : 1,
				segregate: () => !Ar.value && Fr(t),
				isSegregated: W.value.includes(t),
				display: `${e.name}${L.value.style.chart.legend.showPercentage || L.value.style.chart.legend.showValue ? ": " : ""}${i}`
			};
		})), Hr = v(() => ({
			cy: "donut-div-legend",
			backgroundColor: L.value.style.chart.legend.backgroundColor,
			color: L.value.style.chart.legend.color,
			fontSize: L.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: L.value.style.chart.legend.bold ? "bold" : ""
		})), G = v(() => {
			let e = L.value.style.chart.layout.donut.radiusRatio, t = Math.max(.1, Math.min(.50001, e)), n = Math.min(V.value.width * t, V.value.height * t);
			return Math.max(12, n);
		}), K = v(() => ie({ series: Br.value }, V.value.width / 2, V.value.height / 2, G.value, G.value, 1.99999, 2, 1, 360, 105.25, Dr.value)), Ur = v(() => Math.abs(Br.value.map((e) => e.value).reduce((e, t) => e + t, 0)) > 0), Wr = v(() => ie({ series: [{
			value: 1,
			color: L.value.style.chart.layout.donut.emptyFill,
			name: "_",
			seriesIndex: 0,
			patternIndex: -1,
			ghost: !1,
			absoluteValues: [1]
		}] }, V.value.width / 2, V.value.height / 2, G.value, G.value, 1.99999, 2, 1, 360, 105.25, Dr.value)), q = v(() => K.value.filter((e) => !e.ghost)), J = v(() => {
			let e = Math.max(...Br.value.map((e) => e.value)), t = Br.value.map((t) => t.value / e);
			return h({
				series: t,
				center: {
					x: V.value.width / 2,
					y: V.value.height / 2
				},
				maxRadius: Math.min(V.value.width, V.value.height) / 3,
				hasGhost: R.value
			});
		});
		function Gr(e) {
			return e.x > V.value.width / 2 + 6 ? "start" : e.x < V.value.width / 2 - 6 ? "end" : "middle";
		}
		function Kr(e) {
			return e.middlePoint.y > V.value.height / 2 ? s({
				initX: e.middlePoint.x,
				initY: e.middlePoint.y,
				offset: 100,
				centerX: V.value.width / 2,
				centerY: V.value.height / 2
			}).y : s({
				initX: e.middlePoint.x,
				initY: e.middlePoint.y,
				offset: 0,
				centerX: V.value.width / 2,
				centerY: V.value.height / 2
			}).y - 100;
		}
		function Y(e) {
			return e.proportion * 100 > L.value.style.chart.layout.labels.dataLabels.hideUnderValue;
		}
		function qr(e, t) {
			let n = L.value.style.chart.layout.labels.dataLabels.hideUnderValue, r = L.value.style.chart.layout.labels.dataLabels.smallArcClusterThreshold, i = (Xr.value[t] ?? e.proportion ?? 0) * 100;
			return i > n && i <= r;
		}
		let { smallArcLayoutsClassic: X } = tt({
			FINAL_CONFIG: L,
			noGhostDonut: q,
			svg: V,
			padding: br,
			labels_inline_fontSize: Yn,
			minSize: G,
			findArcMidpoint: ee,
			calcMarkerOffsetX: p,
			calcMarkerOffsetY: g,
			animatingIndex: jr,
			segregated: W,
			isSmallArc: qr
		});
		function Jr(e, t) {
			let n = e.value / Yr(t);
			return isNaN(n) ? 0 : m(L.value.style.chart.layout.labels.percentage.formatter, n * 100, d({
				v: n * 100,
				s: "%",
				r: L.value.style.chart.layout.labels.percentage.rounding
			}), { datapoint: e });
		}
		function Yr(e) {
			return [...e].map((e) => e.value).reduce((e, t) => e + t, 0);
		}
		let Z = v(() => Br.value.map((e) => e.value).reduce((e, t) => e + t, 0)), Xr = v(() => {
			let e = H.value.reduce((e, t) => e + t.value, 0);
			return e <= 0 ? [] : H.value.map((t) => t.value / e);
		}), Zr = v(() => Z.value / Br.value.length);
		function Qr(e) {
			return Ar.value ? e.proportion * 100 : e.value / Z.value * 100;
		}
		let $r = D(null), ei = D(!1);
		function ti({ datapoint: e, seriesIndex: t }) {
			L.value.events.datapointLeave && L.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}), tr.value = !1, I.value = null, Wn.value = null, Kn.value = "pointer";
		}
		function Q({ datapoint: e, relativeIndex: t, seriesIndex: n, show: r = !1, triggerMode: i = "pointer" }) {
			L.value.events.datapointEnter && L.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: n
			}), Kn.value = i, Wn.value = t, $r.value = {
				datapoint: e,
				seriesIndex: n,
				config: L.value,
				series: H.value
			}, tr.value = r, I.value = t;
			let a = "", o = L.value.style.chart.tooltip.customFormat;
			if (ei.value = !1, fe(o)) try {
				let t = o({
					seriesIndex: n,
					datapoint: e,
					series: H.value,
					config: L.value
				});
				typeof t == "string" && (nr.value = t, ei.value = !0);
			} catch {
				console.warn("Custom format cannot be applied."), ei.value = !1;
			}
			ei.value || (a += `<div style="width:100%;text-align:center;border-bottom:1px solid ${L.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${e.name}</div>`, a += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 60 60" height="14" width="14"><circle cx="30" cy="30" r="30" stroke="none" fill="${e.color}"/>${Dn.pattern ? `<circle cx="30" cy="30" r="30" stroke="none" fill="url(#pattern_${F.value}_${n})"/>` : ""}</svg>`, a += `<b>${mi({
				showVal: L.value.style.chart.tooltip.showValue,
				showPercentage: L.value.style.chart.tooltip.showPercentage,
				val: `<span>${m(L.value.style.chart.layout.labels.value.formatter, e.value, d({
					p: L.value.style.chart.layout.labels.dataLabels.prefix,
					v: e.value,
					s: L.value.style.chart.layout.labels.dataLabels.suffix,
					r: L.value.style.chart.tooltip.roundingValue
				}), {
					datapoint: e,
					relativeIndex: t,
					seriesIndex: n
				})}</span>`,
				percentage: m(L.value.style.chart.layout.labels.percentage.formatter, e.proportion * 100, d({
					v: e.proportion * 100,
					s: "%",
					r: L.value.style.chart.tooltip.roundingPercentage
				}), {
					datapoint: e,
					relativeIndex: t,
					seriesIndex: n
				}),
				config: L.value.style.chart.tooltip
			})}</b></div>`, L.value.style.chart.comments.showInTooltip && e.comment && (a += `<div class="vue-data-ui-tooltip-comment" style="background:${e.color}20; padding: 6px; margin-bottom: 6px; margin-top:6px; border-left: 1px solid ${e.color}">${e.comment}</div>`), nr.value = `<div>${a}</div>`);
		}
		function ni(e, t) {
			let n = X.value[t];
			if (n) return {
				textAnchor: n.textAnchor,
				x: n.labelX,
				y: n.labelY
			};
			let r = p(e, !0, 12);
			return {
				textAnchor: r.anchor,
				x: r.x,
				y: g(e)
			};
		}
		function ri(e, t, n) {
			let { textAnchor: r, x: i } = ni(e, t), a = hi(e), o = `
        <tspan
            class="vue-data-ui-datalabel-inline"
            fill="${L.value.style.chart.layout.labels.percentage.color}"
            font-size="${n ? Yn.value : Jn.value}px"
            style="font-weight:${L.value.style.chart.layout.labels.percentage.bold ? "bold" : ""}"
        >${a}</tspan>
    `, s = String(e.name ?? "").split(/\n/g), c = "";
			return s.forEach((e, t) => {
				c += t === 0 ? `
                <tspan
                    class="${n ? "vue-data-ui-datalabel-inline" : "vue-data-ui-datalabel-name"}"
                    fill="${L.value.style.chart.layout.labels.name.color}"
                    font-size="${n ? Yn.value : P.value}px"
                    style="font-weight:${L.value.style.chart.layout.labels.name.bold ? "bold" : ""}"
                >${e}</tspan>
            ` : `
                <tspan
                    class="${n ? "vue-data-ui-datalabel-inline" : "vue-data-ui-datalabel-name"}"
                    x="${i}"
                    dy="${(n ? Yn.value : P.value) * 1.2}"
                    fill="${L.value.style.chart.layout.labels.name.color}"
                    font-size="${n ? Yn.value : P.value}px"
                    style="font-weight:${L.value.style.chart.layout.labels.name.bold ? "bold" : ""}"
                >${e}</tspan>
            `;
			}), r === "end" ? `${L.value.style.chart.layout.labels.name.show ? c : ""}${o}` : `${o}${L.value.style.chart.layout.labels.name.show ? c : ""}`;
		}
		function ii(e, t) {
			let n = J.value[t].middlePoint, r = Gr(n), i = s({
				initX: n.x,
				initY: n.y,
				offset: 42,
				centerX: V.value.width / 2,
				centerY: V.value.height / 2
			}), a = i.x;
			i.y;
			let o = hi(e), c = `
        <tspan
            class="vue-data-ui-datalabel-value"
            fill="${L.value.style.chart.layout.labels.percentage.color}"
            font-size="${Jn.value}px"
            style="font-weight:${L.value.style.chart.layout.labels.percentage.bold ? "bold" : "normal"}"
        >${o}</tspan>
    `, l = String(e.name ?? "").split(/\n/g), u = "";
			return l.forEach((e, t) => {
				u += t === 0 ? `
                <tspan
                    class="vue-data-ui-datalabel-name"
                    fill="${L.value.style.chart.layout.labels.name.color}"
                    font-size="${P.value}px"
                    style="font-weight:${L.value.style.chart.layout.labels.name.bold ? "bold" : "normal"}"
                >${e}</tspan>
            ` : `
                <tspan
                    class="vue-data-ui-datalabel-name"
                    x="${a}"
                    dy="${P.value * 1.2}"
                    fill="${L.value.style.chart.layout.labels.name.color}"
                    font-size="${P.value}px"
                    style="font-weight:${L.value.style.chart.layout.labels.name.bold ? "bold" : "normal"}"
                >${e}</tspan>
            `;
			}), r === "end" ? `${L.value.style.chart.layout.labels.name.show ? u : ""}${c}` : `${c}${L.value.style.chart.layout.labels.name.show ? u : ""}`;
		}
		function ai(e) {
			return L.value.useBlurOnHover && ![null, void 0].includes(I.value) && I.value !== e ? `url(#blur_${F.value})` : "";
		}
		function oi(e) {
			if (!Ar.value || jr.value === null || e.seriesIndex !== jr.value) return 1;
			let t = (e.proportion ?? 0) * 100, n = L.value.style.chart.layout.labels.dataLabels.hideUnderValue, r = L.value.style.chart.layout.labels.dataLabels.smallArcClusterThreshold + 2, i = n;
			return t >= r ? 1 : t <= i ? 0 : (t - i) / (r - i);
		}
		let si = v(() => ({
			head: Br.value.map((e) => ({
				name: e.name,
				color: e.color
			})),
			body: Br.value.map((e) => e.value)
		}));
		function ci(e = null) {
			Ue(() => {
				let n = si.value.head.map((e, t) => [
					[e.name],
					[si.value.body[t]],
					[isNaN(si.value.body[t] / Z.value) ? "-" : si.value.body[t] / Z.value * 100]
				]), i = [
					[L.value.style.chart.title.text],
					[L.value.style.chart.title.subtitle.text],
					[
						[""],
						["val"],
						["%"]
					]
				].concat(n), a = r(i);
				e ? e(a) : t({
					csvContent: a,
					title: L.value.style.chart.title.text || "vue-ui-donut"
				});
			});
		}
		let li = v(() => {
			let e = [
				" <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>",
				d({
					p: L.value.style.chart.layout.labels.dataLabels.prefix,
					v: Z.value,
					s: L.value.style.chart.layout.labels.dataLabels.suffix,
					r: L.value.table.td.roundingValue
				}),
				"100%"
			], t = si.value.head.map((e, t) => [
				{
					color: e.color,
					name: e.name || "-"
				},
				si.value.body[t],
				isNaN(si.value.body[t] / Z.value) ? "-" : (si.value.body[t] / Z.value * 100).toFixed(L.value.table.td.roundingPercentage) + "%"
			]), n = t.map((e) => e.map((e, t) => t === 0 ? e.name : e)), r = {
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
			};
			return {
				colNames: [
					L.value.table.columnNames.series,
					L.value.table.columnNames.value,
					L.value.table.columnNames.percentage
				],
				head: e,
				body: t,
				a11yBody: n,
				config: r
			};
		}), ui = D(!1);
		function di(e) {
			ui.value = e, rr.value += 1;
		}
		let fi = v(() => /^((?!chrome|android).)*safari/i.test(navigator.userAgent));
		function pi(e) {
			return Ne({
				num: e,
				rounding: L.value.style.chart.legend.roundingPercentage
			});
		}
		function mi({ val: e, percentage: t, showVal: n, showPercentage: r, config: i }) {
			return Pe({
				config: i,
				val: e,
				percentage: t,
				showVal: n,
				showPercentage: r
			});
		}
		function hi(e) {
			return mi({
				val: m(L.value.style.chart.layout.labels.value.formatter, e.value, d({
					p: L.value.style.chart.layout.labels.dataLabels.prefix,
					v: e.value,
					s: L.value.style.chart.layout.labels.dataLabels.suffix,
					r: L.value.style.chart.layout.labels.value.rounding
				}), { datapoint: e }),
				percentage: Jr(e, q.value),
				showVal: L.value.style.chart.layout.labels.value.show,
				showPercentage: L.value.style.chart.layout.labels.percentage.show,
				config: L.value.style.chart.layout.labels.dataLabels
			});
		}
		function $(e, t) {
			L.value.events.datapointClick && L.value.events.datapointClick({
				datapoint: e,
				seriesIndex: e.seriesIndex
			}), Or("selectDatapoint", {
				datapoint: e,
				index: t
			});
		}
		function gi() {
			B.value.showTable = !B.value.showTable;
		}
		function _i() {
			B.value.dataLabels.show = !B.value.dataLabels.show;
		}
		function vi() {
			B.value.showTooltip = !B.value.showTooltip;
		}
		let yi = D(!1);
		function bi() {
			yi.value = !yi.value;
		}
		async function xi({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { width: t, height: n } = N.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await Se({
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
		function Si() {
			if (!kn.value) return;
			let { x: e, y: t, width: n, height: r } = kn.value.getBBox();
			z.value && z.value.setAttribute("viewBox", `${e} ${t} ${n + Math.min(0, e)} ${r + Math.min(0, t)}`);
		}
		let Ci = v(() => {
			let e = L.value.table.useDialog && !L.value.table.show, t = B.value.showTable;
			return {
				component: e ? Cn : vn,
				title: `${L.value.style.chart.title.text}${L.value.style.chart.title.subtitle.text ? `: ${L.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: L.value.table.th.backgroundColor,
					color: L.value.table.th.color,
					headerColor: L.value.table.th.color,
					headerBg: L.value.table.th.backgroundColor,
					isFullscreen: ui.value,
					fullscreenParent: N.value,
					forcedWidth: Math.min(500, window.innerWidth * .8),
					isCursorPointer: ar.value
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
		$e(() => B.value.showTable, (e) => {
			L.value.table.show || (e && L.value.table.useDialog && Hn.value ? Hn.value.open() : "close" in Hn.value && Hn.value.close());
		});
		function wi() {
			B.value.showTable = !1, Un.value && Un.value.setTableIconState(!1);
		}
		let Ti = v(() => Vr.value.map((e) => ({
			...e,
			name: e.display
		}))), Ei = v(() => L.value.style.chart.backgroundColor), Di = v(() => L.value.style.chart.legend), Oi = v(() => L.value.style.chart.title), { isCallbackImaging: ki, isCallbackSvg: Ai, generateSvg: ji, onGenerateImage: Mi } = xe({
			svg: z,
			title: Oi,
			legend: Di,
			legendItems: Ti,
			backgroundColor: Ei,
			getSvgCallback: () => L.value.userOptions.callbacks.svg,
			generateImage: wr
		});
		async function Ni() {
			if (Or("copyAlt", {
				config: L.value,
				dataset: U.value
			}), !L.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(L.value.userOptions.callbacks.altCopy({
				config: L.value,
				dataset: U.value
			}));
		}
		function Pi() {
			Wn.value = null, qn.value = !0;
		}
		function Fi() {
			Wn.value = null, Kn.value = "pointer", tr.value = !1, I.value = null, qn.value = !1;
		}
		function Ii(e) {
			if (!z.value || yi.value || document.activeElement !== z.value || !q.value.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				Wn.value = null, Kn.value = "pointer", tr.value = !1, I.value = null;
				return;
			}
			if (r) {
				if (Wn.value === null) return;
				let e = q.value[Wn.value];
				if (!e) return;
				$(e, Wn.value);
				return;
			}
			let a = Wn.value, o = I.value, s = a !== null && a >= 0 && a < q.value.length, c = o !== null && o >= 0 && o < q.value.length;
			s ? n ? (a += 1, a >= q.value.length && (a = 0)) : t && (--a, a < 0 && (a = q.value.length - 1)) : c ? (a = n ? o + 1 : o - 1, a >= q.value.length && (a = 0), a < 0 && (a = q.value.length - 1)) : a = n ? 0 : q.value.length - 1;
			let l = q.value[a];
			l && (Wn.value = a, Li(a), Q({
				datapoint: l,
				relativeIndex: a,
				seriesIndex: l.seriesIndex,
				show: !0,
				triggerMode: "keyboard"
			}));
		}
		function Li(e) {
			if (!Number.isFinite(e) || !z.value) return;
			let t = V.value.width / 2, n = V.value.height / 2;
			if (L.value.type === "classic") {
				let r = q.value[e];
				if (!r?.arcSlice) return;
				let i = ee(r.arcSlice);
				if (!i) return;
				t = i.x, n = i.y;
			} else {
				let r = J.value[e]?.middlePoint;
				if (!r) return;
				t = r.x, n = r.y;
			}
			let r = z.value.getBoundingClientRect();
			Gn.value = {
				x: r.left + t / V.value.width * r.width,
				y: r.top + n / V.value.height * r.height
			};
		}
		let Ri = v(() => ({
			headers: li.value?.colNames ?? [],
			rows: li.value?.a11yBody ?? []
		}));
		return Te({
			autoSize: Si,
			getData: kr,
			getImage: xi,
			generatePdf: Cr,
			generateCsv: ci,
			generateImage: wr,
			generateSvg: ji,
			hideSeries: Rr,
			showSeries: Lr,
			toggleTable: gi,
			toggleLabels: _i,
			toggleTooltip: vi,
			toggleAnnotator: bi,
			toggleFullscreen: di,
			copyAlt: Ni
		}), (t, r) => (E(), x("div", {
			ref_key: "donutChart",
			ref: N,
			class: We(`vue-data-ui-component vue-ui-donut ${ui.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${L.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: T(`font-family:${L.value.style.fontFamily};width:100%; ${L.value.responsive ? "height:100%;" : ""} text-align:center;background:${L.value.style.chart.backgroundColor}`),
			id: `donut__${F.value}`,
			onMouseenter: vr,
			onMouseleave: yr
		}, [
			S("div", {
				id: `chart-instructions-${F.value}`,
				class: "sr-only"
			}, [S("p", null, Ye(L.value.a11y.translations.keyboardNavigation), 1)], 8, it),
			Ri.value?.rows?.length ? (E(), y(Ae, {
				key: 0,
				uid: F.value,
				head: Ri.value.headers,
				body: Ri.value.rows,
				notice: L.value.a11y.translations.tableAvailable,
				caption: L.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : b("", !0),
			L.value.userOptions.buttons.annotator && A(z) ? (E(), y(A(bn), {
				key: 1,
				color: L.value.style.chart.color,
				backgroundColor: L.value.style.chart.backgroundColor,
				active: yi.value,
				svgRef: A(z),
				isCursorPointer: ar.value,
				onClose: bi
			}, {
				"annotator-action-close": j(() => [k(t.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": j(({ color: e }) => [k(t.$slots, "annotator-action-color", w(C({ color: e })), void 0, !0)]),
				"annotator-action-draw": j(({ mode: e }) => [k(t.$slots, "annotator-action-draw", w(C({ mode: e })), void 0, !0)]),
				"annotator-action-undo": j(({ disabled: e }) => [k(t.$slots, "annotator-action-undo", w(C({ disabled: e })), void 0, !0)]),
				"annotator-action-redo": j(({ disabled: e }) => [k(t.$slots, "annotator-action-redo", w(C({ disabled: e })), void 0, !0)]),
				"annotator-action-delete": j(({ disabled: e }) => [k(t.$slots, "annotator-action-delete", w(C({ disabled: e })), void 0, !0)]),
				_: 3
			}, 8, [
				"color",
				"backgroundColor",
				"active",
				"svgRef",
				"isCursorPointer"
			])) : b("", !0),
			k(t.$slots, "userConfig", {}, void 0, !0),
			Tr.value ? (E(), x("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Ln,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : b("", !0),
			L.value.style.chart.title.text ? (E(), x("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Mn,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(E(), y(Ce, {
				key: `title_${Rn.value}`,
				config: {
					title: {
						cy: "donut-div-title",
						...L.value.style.chart.title
					},
					subtitle: {
						cy: "donut-div-subtitle",
						...L.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : b("", !0),
			S("div", { id: `legend-top-${F.value}` }, null, 8, at),
			L.value.userOptions.show && On.value && (A(_r) || A(hr)) ? (E(), y(A(xn), {
				ref_key: "userOptionsRef",
				ref: Un,
				key: `user_option_${rr.value}`,
				backgroundColor: L.value.style.chart.backgroundColor,
				color: L.value.style.chart.color,
				isPrinting: A(xr),
				isImaging: A(Sr),
				uid: F.value,
				hasTooltip: L.value.style.chart.tooltip.show && L.value.userOptions.buttons.tooltip,
				hasPdf: L.value.userOptions.buttons.pdf,
				hasImg: L.value.userOptions.buttons.img,
				hasSvg: L.value.userOptions.buttons.svg,
				hasXls: L.value.userOptions.buttons.csv,
				hasTable: L.value.userOptions.buttons.table,
				hasLabel: L.value.userOptions.buttons.labels,
				hasFullscreen: L.value.userOptions.buttons.fullscreen,
				hasAltCopy: L.value.userOptions.buttons.altCopy,
				isFullscreen: ui.value,
				chartElement: N.value,
				position: L.value.userOptions.position,
				callbacks: L.value.userOptions.callbacks,
				isTooltip: B.value.showTooltip,
				titles: { ...L.value.userOptions.buttonTitles },
				hasAnnotator: L.value.userOptions.buttons.annotator,
				isAnnotation: yi.value,
				printScale: L.value.userOptions.print.scale,
				tableDialog: L.value.table.useDialog,
				isCursorPointer: ar.value,
				onToggleFullscreen: di,
				onGeneratePdf: A(Cr),
				onGenerateCsv: ci,
				onGenerateImage: A(Mi),
				onGenerateSvg: A(ji),
				onToggleTable: gi,
				onToggleLabels: _i,
				onToggleTooltip: vi,
				onToggleAnnotator: bi,
				onCopyAlt: Ni,
				style: T({ visibility: A(_r) ? A(hr) ? "visible" : "hidden" : "visible" })
			}, Re({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: j(({ isOpen: e, color: n }) => [k(t.$slots, "menuIcon", w(C({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: j(() => [k(t.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: j(() => [k(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: j(() => [k(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: j(() => [k(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionSvg ? {
					name: "optionSvg",
					fn: j(() => [k(t.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionTable ? {
					name: "optionTable",
					fn: j(() => [k(t.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionLabels ? {
					name: "optionLabels",
					fn: j(() => [k(t.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: j(({ toggleFullscreen: e, isFullscreen: n }) => [k(t.$slots, "optionFullscreen", w(C({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: j(({ toggleAnnotator: e, isAnnotator: n }) => [k(t.$slots, "optionAnnotator", w(C({
						toggleAnnotator: e,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: j(({ altCopy: e }) => [k(t.$slots, "optionAltCopy", w(C({ altCopy: e })), void 0, !0)]),
					key: "10"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: j(() => [k(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: j(() => [k(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasLabel.hasFullscreen.hasAltCopy.isFullscreen.chartElement.position.callbacks.isTooltip.titles.hasAnnotator.isAnnotation.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : b("", !0),
			S("div", ot, [(E(), x("svg", {
				ref_key: "svgRef",
				ref: z,
				xmlns: A(le),
				"aria-describedby": `chart-instructions-${F.value}`,
				class: We({
					"vue-data-ui-fullscreen--on": ui.value,
					"vue-data-ui-fulscreen--off": !ui.value,
					"vue-data-ui-svg": !0
				}),
				viewBox: `0 0 ${V.value.width <= 0 ? 10 : V.value.width} ${V.value.height <= 0 ? 10 : V.value.height}`,
				style: T(`max-width:100%; overflow: visible; background:transparent;color:${L.value.style.chart.color};${br.value.css}`),
				tabindex: "0",
				onFocus: Pi,
				onBlur: Fi,
				onKeydown: Ii
			}, [S("g", {
				ref_key: "G",
				ref: kn,
				class: "vue-data-ui-g"
			}, [
				Be(A(Sn)),
				t.$slots["chart-background"] ? (E(), x("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: V.value.width <= 0 ? 10 : V.value.width,
					height: V.value.height <= 0 ? 10 : V.value.height,
					style: { pointerEvents: "none" }
				}, [k(t.$slots, "chart-background", {}, void 0, !0)], 8, ct)) : b("", !0),
				L.value.type === "classic" && !isNaN(Dr.value / G.value) ? (E(), x("defs", lt, [L.value.style.chart.useGradient ? (E(), y(Oe, {
					key: 0,
					t: "radial",
					id: `gradient_${F.value}`,
					stops: [
						[
							"0%",
							A(n)(L.value.style.chart.backgroundColor, 0),
							0
						],
						[
							`${(1 - Dr.value / G.value) * 100}%`,
							A(n)("#FFFFFF", 0),
							0
						],
						[
							`${(1 - Dr.value / G.value / 2) * 100}%`,
							A(n)("#FFFFFF", L.value.style.chart.gradientIntensity),
							1
						],
						[
							"100%",
							A(n)(L.value.style.chart.backgroundColor, 0),
							0
						]
					]
				}, null, 8, ["id", "stops"])) : b("", !0)])) : b("", !0),
				L.value.type === "polar" ? (E(), x("defs", ut, [(E(!0), x(_, null, O(J.value, (e, t) => (E(), y(Oe, {
					t: "radial",
					id: `polar_gradient_${t}_${F.value}`,
					key: `pg_${t}_${F.value}`,
					cx: (isNaN(e.middlePoint.x / V.value.width * 100) ? 0 : e.middlePoint.x / V.value.width * 100) + "%",
					cy: (isNaN(e.middlePoint.y / V.value.height * 100) ? 0 : e.middlePoint.y / V.value.height * 100) + "%",
					r: "62%",
					stops: [[
						"0%",
						A(u)(K.value[t].color, .05),
						L.value.style.chart.gradientIntensity / 100
					], [
						"100%",
						K.value[t].color,
						1
					]]
				}, null, 8, [
					"id",
					"cx",
					"cy",
					"stops"
				]))), 128))])) : b("", !0),
				S("defs", null, [
					S("filter", {
						id: `blur_${F.value}`,
						x: "-50%",
						y: "-50%",
						width: "200%",
						height: "200%"
					}, [S("feGaussianBlur", {
						in: "SourceGraphic",
						stdDeviation: 2,
						id: `blur_std_${F.value}`
					}, null, 8, ft), r[5] ||= S("feColorMatrix", {
						type: "saturate",
						values: "0"
					}, null, -1)], 8, dt),
					S("filter", {
						id: `shadow_${F.value}`,
						"color-interpolation-filters": "sRGB"
					}, [S("feDropShadow", {
						dx: "0",
						dy: "0",
						stdDeviation: "10",
						"flood-opacity": "0.5",
						"flood-color": L.value.style.chart.layout.donut.shadowColor
					}, null, 8, mt)], 8, pt),
					S("filter", {
						id: `drop_shadow_${F.value}`,
						"color-interpolation-filters": "sRGB",
						x: "-50%",
						y: "-50%",
						width: "200%",
						height: "200%"
					}, [S("feDropShadow", {
						dx: "0",
						dy: "0",
						stdDeviation: "3",
						"flood-opacity": "1",
						"flood-color": L.value.style.chart.layout.donut.shadowColor
					}, null, 8, gt)], 8, ht)
				]),
				L.value.type === "classic" ? (E(!0), x(_, { key: 3 }, O(K.value.filter((e) => !e.ghost), (e, t) => (E(), x("g", null, [Y(e) && B.value.dataLabels.show ? (E(), x("path", {
					key: 0,
					d: A(X)[t]?.connectorPath || A(ne)(e, {
						x: V.value.width / 2,
						y: V.value.height / 2
					}, 16, 16, !1, !1, Dr.value, 12, L.value.style.chart.layout.curvedMarkers),
					stroke: e.color,
					"stroke-width": "1",
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					fill: "none",
					filter: ai(t)
				}, null, 8, _t)) : b("", !0)]))), 256)) : b("", !0),
				L.value.type === "polar" ? (E(!0), x(_, { key: 4 }, O(K.value.filter((e) => !e.ghost), (e, t) => (E(), x("g", null, [Y(e) && B.value.dataLabels.show ? (E(), x("path", {
					key: 0,
					d: `M ${A(s)({
						initX: J.value[t].middlePoint.x,
						initY: J.value[t].middlePoint.y,
						offset: 24,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x},${A(s)({
						initX: J.value[t].middlePoint.x,
						initY: J.value[t].middlePoint.y,
						offset: 24,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).y} ${J.value[t].middlePoint.x},${J.value[t].middlePoint.y}`,
					stroke: e.color,
					"stroke-width": "1",
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					fill: "none",
					filter: ai(t),
					style: T({ transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out` })
				}, null, 12, vt)) : b("", !0)]))), 256)) : b("", !0),
				L.value.type === "classic" && L.value.style.chart.layout.donut.useShadow ? (E(), x("circle", {
					key: 5,
					cx: V.value.width / 2,
					cy: V.value.height / 2,
					r: G.value <= 0 ? 10 : G.value,
					fill: L.value.style.chart.backgroundColor,
					filter: `url(#shadow_${F.value})`
				}, null, 8, yt)) : b("", !0),
				t.$slots.pattern ? (E(), x("g", bt, [(E(!0), x(_, null, O(e.dataset, (e, n) => (E(), x("defs", { key: `pattern-${e.patternIndex}` }, [k(t.$slots, "pattern", He({ ref_for: !0 }, {
					seriesIndex: n,
					patternId: `pattern_${F.value}_${n}`
				}), void 0, !0)]))), 128))])) : b("", !0),
				Z.value && L.value.type === "classic" ? (E(), x(_, { key: 7 }, [
					(E(!0), x(_, null, O(q.value, (e, t) => (E(), x("path", {
						stroke: L.value.style.chart.backgroundColor,
						d: e.arcSlice,
						fill: "#FFFFFF"
					}, null, 8, xt))), 256)),
					(E(!0), x(_, null, O(q.value, (e, t) => (E(), x("path", {
						class: "vue-ui-donut-arc-path",
						d: e.arcSlice,
						fill: e.color,
						stroke: L.value.style.chart.layout.donut.borderColorAuto ? L.value.style.chart.backgroundColor : L.value.style.chart.layout.donut.borderColor,
						"stroke-width": L.value.style.chart.layout.donut.borderWidth,
						filter: ai(t)
					}, null, 8, St))), 256)),
					t.$slots.pattern ? (E(), x("g", Ct, [(E(!0), x(_, null, O(q.value, (e, t) => (E(), x("path", {
						class: "vue-ui-donut-arc-path",
						d: e.arcSlice,
						fill: `url(#${e.pattern})`,
						stroke: L.value.style.chart.layout.donut.borderColorAuto ? L.value.style.chart.backgroundColor : L.value.style.chart.layout.donut.borderColor,
						"stroke-width": L.value.style.chart.layout.donut.borderWidth,
						filter: ai(t)
					}, null, 8, wt))), 256))])) : b("", !0)
				], 64)) : b("", !0),
				Z.value && L.value.type === "polar" ? (E(), x(_, { key: 8 }, [K.value.length > 1 ? (E(), x("g", Tt, [
					(E(!0), x(_, null, O(q.value, (e, t) => (E(), x("path", {
						stroke: L.value.style.chart.layout.donut.borderColorAuto ? L.value.style.chart.backgroundColor : L.value.style.chart.layout.donut.borderColor,
						d: J.value[t].path,
						fill: "#FFFFFF",
						style: T({ transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out` })
					}, null, 12, Et))), 256)),
					L.value.style.chart.layout.donut.useShadow ? (E(), x("g", Dt, [(E(!0), x(_, null, O(q.value, (e, t) => (E(), x("path", {
						class: "vue-ui-donut-arc-path",
						d: J.value[t].path,
						fill: "transparent",
						stroke: L.value.style.chart.layout.donut.borderColorAuto ? L.value.style.chart.backgroundColor : L.value.style.chart.layout.donut.borderColor,
						"stroke-width": L.value.style.chart.layout.donut.borderWidth,
						filter: `url(#drop_shadow_${F.value})`,
						style: T({ transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out` })
					}, null, 12, Ot))), 256))])) : b("", !0),
					t.$slots.pattern ? (E(), x("g", kt, [(E(!0), x(_, null, O(q.value, (e, t) => (E(), x("path", {
						class: "vue-ui-donut-arc-path",
						d: J.value[t].path,
						fill: `url(#${e.pattern})`,
						stroke: L.value.style.chart.layout.donut.borderColorAuto ? L.value.style.chart.backgroundColor : L.value.style.chart.layout.donut.borderColor,
						"stroke-width": L.value.style.chart.layout.donut.borderWidth,
						filter: ai(t),
						style: T({
							transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out`,
							transformOrigin: "center"
						})
					}, null, 12, At))), 256))])) : b("", !0),
					(E(!0), x(_, null, O(q.value, (e, t) => (E(), x("path", {
						class: "vue-ui-donut-arc-path",
						d: J.value[t].path,
						fill: L.value.style.chart.useGradient ? `url(#polar_gradient_${t}_${F.value})` : e.color,
						stroke: L.value.style.chart.layout.donut.borderColorAuto ? L.value.style.chart.backgroundColor : L.value.style.chart.layout.donut.borderColor,
						"stroke-width": L.value.style.chart.layout.donut.borderWidth,
						filter: ai(t),
						style: T({ transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out` })
					}, null, 12, jt))), 256))
				])) : (E(), x("g", Mt, [t.$slots.pattern ? (E(), x("circle", {
					key: 0,
					cx: V.value.width / 2,
					cy: V.value.height / 2,
					r: G.value,
					fill: `url(#pattern_${F.value}_${K.value[0].patternIndex})`,
					stroke: L.value.style.chart.backgroundColor,
					"stroke-width": L.value.style.chart.layout.donut.borderWidth
				}, null, 8, Nt)) : b("", !0), S("circle", {
					cx: V.value.width / 2,
					cy: V.value.height / 2,
					r: G.value,
					fill: L.value.style.chart.useGradient ? `url(#polar_gradient_0_${F.value})` : K.value[0].color,
					stroke: L.value.style.chart.backgroundColor,
					"stroke-width": L.value.style.chart.layout.donut.borderWidth
				}, null, 8, Pt)]))], 64)) : (E(), x(_, { key: 9 }, [L.value.type === "classic" && !Ur.value ? (E(), x("g", Ft, [(E(!0), x(_, null, O(Wr.value, (e, t) => (E(), x("path", {
					class: "vue-ui-donut-arc-path",
					d: e.arcSlice,
					fill: e.color,
					stroke: L.value.style.chart.backgroundColor,
					"stroke-width": L.value.style.chart.layout.donut.borderWidth
				}, null, 8, It))), 256))])) : b("", !0), S("circle", {
					cx: V.value.width / 2,
					cy: V.value.height / 2,
					r: G.value <= 0 ? 10 : G.value,
					fill: "transparent",
					stroke: L.value.style.chart.backgroundColor
				}, null, 8, Lt)], 64)),
				L.value.style.chart.useGradient && L.value.type === "classic" ? (E(), x("circle", {
					key: 10,
					cx: V.value.width / 2,
					cy: V.value.height / 2,
					r: G.value <= 0 ? 10 : G.value,
					fill: `url(#gradient_${F.value})`
				}, null, 8, Rt)) : b("", !0),
				S("circle", {
					ref_key: "circle_hollow",
					ref: jn,
					style: { pointerEvents: "none" },
					fill: "none",
					cx: V.value.width / 2,
					cy: V.value.height / 2,
					r: Math.max(.1, Dr.value * 1.7)
				}, null, 8, zt),
				Z.value ? (E(), x(_, { key: 11 }, [K.value.length > 1 || L.value.type === "classic" ? (E(), x("g", Bt, [(E(!0), x(_, null, O(K.value.filter((e) => !e.ghost), (e, t) => (E(), x("path", {
					d: L.value.type === "classic" ? e.arcSlice : J.value[t].path,
					stroke: L.value.style.chart.layout.donut.borderColorAuto ? L.value.style.chart.backgroundColor : L.value.style.chart.layout.donut.borderColor,
					fill: I.value === t ? L.value.style.chart.layout.donut.selectedColor : "transparent",
					onMouseenter: (n) => Q({
						datapoint: e,
						relativeIndex: t,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					}),
					onClick: (n) => $(e, t)
				}, null, 40, Vt))), 256))])) : (E(), x("g", Ht, [S("circle", {
					cx: V.value.width / 2,
					cy: V.value.height / 2,
					r: G.value,
					fill: "transparent",
					onMouseenter: r[0] ||= (e) => Q({
						datapoint: K.value[0],
						relativeIndex: 0,
						seriesIndex: K.value[0].seriesIndex,
						show: !0
					}),
					onMouseleave: r[1] ||= (e) => ti({
						datapoint: K.value[0],
						seriesIndex: K.value[0].seriesIndex
					}),
					onClick: r[2] ||= (e) => $(K.value[0], t.i)
				}, null, 40, Ut)]))], 64)) : b("", !0),
				L.value.type === "classic" ? (E(), x("g", {
					key: 12,
					ref_key: "G_hollow",
					ref: An,
					class: "vue-data-ui-donut-hollow-labels"
				}, [
					L.value.style.chart.layout.labels.hollow.total.show ? (E(), x("text", {
						key: 0,
						"text-anchor": "middle",
						x: V.value.width / 2,
						y: V.value.height / 2 - (L.value.style.chart.layout.labels.hollow.average.show ? L.value.style.chart.layout.labels.hollow.total.fontSize : 0) + L.value.style.chart.layout.labels.hollow.total.offsetY,
						fill: L.value.style.chart.layout.labels.hollow.total.color,
						"font-size": L.value.style.chart.layout.labels.hollow.total.fontSize,
						style: T(`font-weight:${L.value.style.chart.layout.labels.hollow.total.bold ? "bold" : ""}`)
					}, Ye(L.value.style.chart.layout.labels.hollow.total.text), 13, Wt)) : b("", !0),
					L.value.style.chart.layout.labels.hollow.total.show ? (E(), x("text", {
						key: 1,
						"text-anchor": "middle",
						x: V.value.width / 2,
						y: V.value.height / 2 + L.value.style.chart.layout.labels.hollow.total.fontSize - (L.value.style.chart.layout.labels.hollow.average.show ? L.value.style.chart.layout.labels.hollow.total.fontSize : 0) + L.value.style.chart.layout.labels.hollow.total.value.offsetY,
						fill: L.value.style.chart.layout.labels.hollow.total.value.color,
						"font-size": L.value.style.chart.layout.labels.hollow.total.value.fontSize,
						style: T(`font-weight:${L.value.style.chart.layout.labels.hollow.total.value.bold ? "bold" : ""}`)
					}, Ye(A(m)(L.value.style.chart.layout.labels.hollow.total.value.formatter, Z.value, A(d)({
						p: L.value.style.chart.layout.labels.hollow.total.value.prefix,
						v: Z.value,
						s: L.value.style.chart.layout.labels.hollow.total.value.suffix
					}))), 13, Gt)) : b("", !0),
					L.value.style.chart.layout.labels.hollow.average.show ? (E(), x("text", {
						key: 2,
						"text-anchor": "middle",
						x: V.value.width / 2,
						y: V.value.height / 2 + (L.value.style.chart.layout.labels.hollow.total.show ? L.value.style.chart.layout.labels.hollow.average.fontSize : 0) + L.value.style.chart.layout.labels.hollow.average.offsetY,
						fill: L.value.style.chart.layout.labels.hollow.average.color,
						"font-size": L.value.style.chart.layout.labels.hollow.average.fontSize,
						style: T(`font-weight:${L.value.style.chart.layout.labels.hollow.average.bold ? "bold" : ""}`)
					}, Ye(L.value.style.chart.layout.labels.hollow.average.text), 13, Kt)) : b("", !0),
					L.value.style.chart.layout.labels.hollow.average.show ? (E(), x("text", {
						key: 3,
						"text-anchor": "middle",
						x: V.value.width / 2,
						y: V.value.height / 2 + (L.value.style.chart.layout.labels.hollow.total.show ? L.value.style.chart.layout.labels.hollow.average.fontSize : 0) + L.value.style.chart.layout.labels.hollow.average.fontSize + L.value.style.chart.layout.labels.hollow.average.value.offsetY,
						fill: L.value.style.chart.layout.labels.hollow.average.value.color,
						"font-size": L.value.style.chart.layout.labels.hollow.average.value.fontSize,
						style: T(`font-weight:${L.value.style.chart.layout.labels.hollow.average.value.bold ? "bold" : ""}`)
					}, Ye(Ar.value || R.value ? "--" : A(m)(L.value.style.chart.layout.labels.hollow.average.value.formatter, A(ae)(Zr.value), A(d)({
						p: L.value.style.chart.layout.labels.hollow.average.value.prefix,
						v: A(ae)(Zr.value),
						s: L.value.style.chart.layout.labels.hollow.average.value.suffix,
						r: L.value.style.chart.layout.labels.hollow.average.value.rounding
					}))), 13, qt)) : b("", !0)
				], 512)) : b("", !0),
				(E(!0), x(_, null, O(q.value.filter((e) => !e.ghost), (e, n) => (E(), x("g", {
					filter: ai(n),
					key: e.seriesIndex,
					opacity: oi(e)
				}, [L.value.style.chart.layout.labels.dataLabels.useLabelSlots ? (E(), x("g", Yt, [(E(), x("foreignObject", {
					x: A(p)(e, !0).anchor === "end" ? A(p)(e).x - 120 : A(p)(e, !0).anchor === "middle" ? A(p)(e).x - 60 : A(p)(e).x,
					y: A(g)(e) - (fi.value ? 20 : 0),
					width: "120",
					height: "60",
					style: { overflow: "visible" }
				}, [S("div", null, [k(t.$slots, "dataLabel", He({ ref_for: !0 }, {
					datapoint: e,
					isBlur: !L.value.useBlurOnHover || [null, void 0].includes(I.value) || I.value === n,
					isSafari: fi.value,
					isVisible: Y(e) && B.value.dataLabels.show,
					textAlign: A(p)(e, !0, 16, !0).anchor,
					flexAlign: A(p)(e, !0, 16).anchor,
					percentage: Jr(e, q.value)
				}), void 0, !0)])], 8, Xt))])) : (E(), x("g", Zt, [L.value.type === "classic" ? (E(), x(_, { key: 0 }, [Y(e) && B.value.dataLabels.show ? (E(), x("circle", {
					key: 0,
					cx: A(X)[n]?.markerX ?? A(p)(e).x,
					cy: A(X)[n]?.markerY ?? A(g)(e) - 3.5,
					fill: e.color,
					stroke: L.value.style.chart.backgroundColor,
					"stroke-width": 1,
					r: 3,
					filter: !L.value.useBlurOnHover || [null, void 0].includes(I.value) || I.value === n ? "" : `url(#blur_${F.value})`,
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					})
				}, null, 40, Qt)) : b("", !0), L.value.style.chart.layout.labels.dataLabels.oneLine || A(X)[n] ? et((E(), x("text", {
					key: 1,
					class: "vue-data-ui-datalabel-inline",
					"text-anchor": A(X)[n]?.textAnchor || A(p)(e, !0, 12).anchor,
					x: A(X)[n]?.labelX ?? A(p)(e, !0, 12).x,
					y: A(X)[n]?.labelY ?? A(g)(e),
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					}),
					innerHTML: ri(e, n, !!A(X)[n])
				}, null, 40, $t)), [[Qe, Y(e) && B.value.dataLabels.show]]) : (E(), x(_, { key: 2 }, [et(S("text", {
					class: "vue-data-ui-datalabel-value",
					"text-anchor": A(X)[n]?.textAnchor || A(p)(e, !0, 12).anchor,
					x: A(X)[n]?.labelX ?? A(p)(e, !0, 12).x,
					y: A(X)[n]?.labelY ?? A(g)(e),
					fill: L.value.style.chart.layout.labels.percentage.color,
					"font-size": Jn.value + "px",
					style: T(`font-weight:${L.value.style.chart.layout.labels.percentage.bold ? "bold" : ""}`),
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					})
				}, Ye(hi(e)), 45, en), [[Qe, Y(e) && B.value.dataLabels.show]]), et(S("text", {
					class: "vue-data-ui-datalabel-name",
					"text-anchor": A(X)[n]?.textAnchor || A(p)(e).anchor,
					x: A(X)[n]?.labelX ?? A(p)(e, !0, 12).x,
					y: (A(X)[n]?.labelY ?? A(g)(e)) + P.value * 1.2,
					fill: L.value.style.chart.layout.labels.name.color,
					"font-size": P.value + "px",
					style: T(`font-weight:${L.value.style.chart.layout.labels.name.bold ? "bold" : ""}`),
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					}),
					innerHTML: A(a)({
						content: e.name,
						fontSize: P.value,
						fill: L.value.style.chart.layout.labels.name.color,
						x: A(X)[n]?.labelX ?? A(p)(e, !0, 12).x,
						y: (A(X)[n]?.labelY ?? A(g)(e)) + P.value
					})
				}, null, 44, tn), [[Qe, Y(e, !0, 12) && B.value.dataLabels.show && L.value.style.chart.layout.labels.name.show]])], 64))], 64)) : b("", !0), L.value.type === "polar" ? (E(), x(_, { key: 1 }, [Y(e) && B.value.dataLabels.show ? (E(), x("circle", {
					key: 0,
					cx: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 24,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x,
					cy: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 24,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).y,
					fill: e.color,
					stroke: L.value.style.chart.backgroundColor,
					"stroke-width": 1,
					r: 3,
					filter: !L.value.useBlurOnHover || [null, void 0].includes(I.value) || I.value === n ? "" : `url(#blur_${F.value})`,
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					}),
					style: T({ transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out` })
				}, null, 44, nn)) : b("", !0), L.value.style.chart.layout.labels.dataLabels.oneLine ? et((E(), x("text", {
					key: 1,
					class: "vue-data-ui-datalabel-inline",
					"text-anchor": Gr(J.value[n].middlePoint),
					x: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x,
					y: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).y,
					style: T({ transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out` }),
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					}),
					innerHTML: ii(e, n)
				}, null, 44, rn)), [[Qe, Y(e) && B.value.dataLabels.show]]) : (E(), x(_, { key: 2 }, [Y(e) && B.value.dataLabels.show ? (E(), x("text", {
					key: 0,
					class: "vue-data-ui-datalabel-value",
					"text-anchor": Gr(J.value[n].middlePoint),
					x: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x,
					y: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).y,
					fill: L.value.style.chart.layout.labels.percentage.color,
					"font-size": Jn.value,
					style: T({
						transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out`,
						fontWeight: L.value.style.chart.layout.labels.percentage.bold ? "bold" : "normal"
					}),
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					})
				}, Ye(hi(e)), 45, an)) : b("", !0), Y(e, !0, 12) && B.value.dataLabels.show && L.value.style.chart.layout.labels.name.show ? (E(), x("text", {
					key: 1,
					class: "vue-data-ui-datalabel-name",
					"text-anchor": Gr(J.value[n].middlePoint),
					x: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x,
					y: A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).y + P.value * 1.2,
					fill: L.value.style.chart.layout.labels.name.color,
					"font-size": P.value,
					style: T({
						transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out`,
						fontWeight: L.value.style.chart.layout.labels.name.bold ? "bold" : "normal"
					}),
					onClick: (t) => $(e, n),
					onMouseenter: (t) => Q({
						datapoint: e,
						relativeIndex: n,
						seriesIndex: e.seriesIndex,
						show: !0
					}),
					onMouseleave: (t) => ti({
						datapoint: e,
						seriesIndex: e.seriesIndex
					}),
					innerHTML: A(a)({
						content: e.name,
						fontSize: P.value,
						fill: L.value.style.chart.layout.labels.name.color,
						x: A(s)({
							initX: J.value[n].middlePoint.x,
							initY: J.value[n].middlePoint.y,
							offset: 42,
							centerX: V.value.width / 2,
							centerY: V.value.height / 2
						}).x,
						y: A(s)({
							initX: J.value[n].middlePoint.x,
							initY: J.value[n].middlePoint.y,
							offset: 42,
							centerX: V.value.width / 2,
							centerY: V.value.height / 2
						}).y + P.value * 1.2
					})
				}, null, 44, on)) : b("", !0)], 64))], 64)) : b("", !0)])), B.value.dataLabels.show && L.value.style.chart.comments.show && e.comment ? (E(), x("g", sn, [Y(e) && L.value.type === "classic" ? (E(), x("foreignObject", {
					key: 0,
					x: L.value.style.chart.comments.offsetX + (A(p)(e, !0).anchor === "end" ? A(p)(e).x - L.value.style.chart.comments.width : A(p)(e, !0).anchor === "middle" ? A(p)(e).x - L.value.style.chart.comments.width / 2 : A(p)(e).x),
					y: A(g)(e) + 24 + L.value.style.chart.comments.offsetY,
					width: L.value.style.chart.comments.width,
					height: "200",
					style: {
						overflow: "visible",
						"pointer-events": "none"
					}
				}, [S("div", null, [k(t.$slots, "plot-comment", { plot: {
					...e,
					textAlign: A(p)(e, !0, 16, !0).anchor,
					flexAlign: A(p)(e, !0, 16).anchor,
					isFirstLoad: R.value
				} }, void 0, !0)])], 8, cn)) : b("", !0), Y(e) && L.value.type === "polar" ? (E(), x("foreignObject", {
					key: 1,
					x: L.value.style.chart.comments.offsetX + (Gr(J.value[n].middlePoint) === "end" ? A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x - L.value.style.chart.comments.width : Gr(J.value[n].middlePoint) === "middle" ? A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x - L.value.style.chart.comments.width / 2 : A(s)({
						initX: J.value[n].middlePoint.x,
						initY: J.value[n].middlePoint.y,
						offset: 42,
						centerX: V.value.width / 2,
						centerY: V.value.height / 2
					}).x),
					y: Kr(J.value[n]) + L.value.style.chart.comments.offsetY,
					width: L.value.style.chart.comments.width,
					height: "200",
					style: T({
						transition: R.value || !L.value.serieToggleAnimation.show ? "none" : `all ${L.value.serieToggleAnimation.durationMs}ms ease-in-out`,
						overflow: "visible",
						pointerEvents: "none"
					})
				}, [S("div", null, [k(t.$slots, "plot-comment", { plot: {
					...e,
					textAlign: Gr(J.value[n].middlePoint),
					flexAlign: Gr(J.value[n].middlePoint),
					isFirstLoad: R.value
				} }, void 0, !0)])], 12, ln)) : b("", !0)])) : b("", !0)], 8, Jt))), 128)),
				k(t.$slots, "svg", { svg: {
					...V.value,
					datapoints: K.value,
					isPrintingImg: A(xr) || A(Sr) || A(ki),
					isPrintingSvg: A(Ai)
				} }, void 0, !0)
			], 512)], 46, st)), t.$slots.hint ? (E(), x("div", un, [k(t.$slots, "hint", w(C({
				hint: L.value.a11y.translations.keyboardNavigation,
				isVisible: qn.value
			})), void 0, !0)])) : b("", !0)]),
			t.$slots.watermark ? (E(), x("div", dn, [k(t.$slots, "watermark", w(C({ isPrinting: A(xr) || A(Sr) || A(ki) || A(Ai) })), void 0, !0)])) : b("", !0),
			S("div", { id: `legend-bottom-${F.value}` }, null, 8, fn),
			Vn.value && (L.value.style.chart.legend.show || t.$slots.legend) ? (E(), y(Le, {
				key: 6,
				to: L.value.style.chart.legend.position === "top" ? `#legend-top-${F.value}` : `#legend-bottom-${F.value}`
			}, [S("div", {
				ref_key: "chartLegend",
				ref: Nn
			}, [k(t.$slots, "legend", { legend: Vr.value }, () => [L.value.style.chart.legend.show ? (E(), y(Ie, {
				key: `legend_${Bn.value}`,
				legendSet: Vr.value,
				config: Hr.value,
				onClickMarker: r[3] ||= ({ i: e }) => Fr(e),
				isCursorPointer: ar.value
			}, Re({
				item: j(({ legend: e, index: t }) => [S("div", {
					style: T(`opacity:${W.value.includes(t) ? .5 : 1}`),
					onClick: (t) => e.segregate()
				}, Ye(e.display), 13, pn)]),
				legendToggle: j(() => [Vr.value.length > 2 && L.value.style.chart.legend.selectAllToggle.show && !A(sr) ? (E(), y(ke, {
					key: 0,
					backgroundColor: L.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: L.value.style.chart.legend.selectAllToggle.color,
					fontSize: L.value.style.chart.legend.fontSize,
					checked: W.value.length > 0,
					isCursorPointer: ar.value,
					onToggle: Pr
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : b("", !0)]),
				_: 2
			}, [t.$slots.pattern ? {
				name: "legend-pattern",
				fn: j(({ legend: e, index: t }) => [Be(we, {
					shape: e.shape,
					radius: 30,
					stroke: "none",
					plot: {
						x: 30,
						y: 30
					},
					fill: `url(#pattern_${F.value}_${t})`
				}, null, 8, ["shape", "fill"])]),
				key: "0"
			} : void 0]), 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : b("", !0)], !0)], 512)], 8, ["to"])) : b("", !0),
			t.$slots.source ? (E(), x("div", {
				key: 7,
				ref_key: "source",
				ref: In,
				dir: "auto"
			}, [k(t.$slots, "source", {}, void 0, !0)], 512)) : b("", !0),
			t.$slots.hollow ? (E(), x("div", mn, [k(t.$slots, "hollow", w(C({
				total: Z.value,
				average: Zr.value,
				dataset: H.value,
				...V.value
			})), void 0, !0)])) : b("", !0),
			Be(A(gn), {
				teleportTo: L.value.style.chart.tooltip.teleportTo,
				show: B.value.showTooltip && tr.value,
				backgroundColor: L.value.style.chart.tooltip.backgroundColor,
				color: L.value.style.chart.tooltip.color,
				fontSize: L.value.style.chart.tooltip.fontSize,
				borderRadius: L.value.style.chart.tooltip.borderRadius,
				borderColor: L.value.style.chart.tooltip.borderColor,
				borderWidth: L.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: L.value.style.chart.tooltip.backgroundOpacity,
				position: L.value.style.chart.tooltip.position,
				offsetX: L.value.style.chart.tooltip.offsetX,
				offsetY: L.value.style.chart.tooltip.offsetY,
				parent: N.value,
				content: nr.value,
				isCustom: ei.value,
				isFullscreen: ui.value,
				smooth: L.value.style.chart.tooltip.smooth,
				backdropFilter: L.value.style.chart.tooltip.backdropFilter,
				smoothForce: L.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: L.value.style.chart.tooltip.smoothSnapThrehsold,
				isA11yMode: Kn.value === "keyboard",
				a11yPosition: Gn.value
			}, {
				"tooltip-before": j(() => [k(t.$slots, "tooltip-before", w(C({ ...$r.value })), void 0, !0)]),
				tooltip: j(() => [k(t.$slots, "tooltip", w(C({ ...$r.value })), void 0, !0)]),
				"tooltip-after": j(() => [k(t.$slots, "tooltip-after", w(C({ ...$r.value })), void 0, !0)]),
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
				"isCustom",
				"isFullscreen",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			On.value && L.value.userOptions.buttons.table ? (E(), y(qe(Ci.value.component), He({ key: 9 }, Ci.value.props, {
				ref_key: "tableUnit",
				ref: Hn,
				onClose: wi
			}), Re({
				content: j(() => [(E(), y(A(yn), {
					key: `table_${zn.value}`,
					colNames: li.value.colNames,
					head: li.value.head,
					body: li.value.body,
					config: li.value.config,
					title: L.value.table.useDialog ? "" : Ci.value.title,
					withCloseButton: !L.value.table.useDialog,
					isCursorPointer: ar.value,
					onClose: wi
				}, {
					th: j(({ th: e }) => [S("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, hn)]),
					td: j(({ td: e }) => [ze(Ye(e.name ? e.name : isNaN(Number(e)) ? e.includes("%") ? e : A(m)(L.value.style.chart.layout.labels.percentage.formatter, e, A(d)({
						v: e,
						s: "%",
						r: L.value.style.chart.layout.labels.percentage.rounding
					})) : A(m)(L.value.style.chart.layout.labels.value.formatter, e, A(d)({
						p: L.value.style.chart.layout.labels.dataLabels.prefix,
						v: e,
						s: L.value.style.chart.layout.labels.dataLabels.suffix,
						r: L.value.style.chart.layout.labels.value.rounding
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
			}, [L.value.table.useDialog ? {
				name: "title",
				fn: j(() => [ze(Ye(Ci.value.title), 1)]),
				key: "0"
			} : void 0, L.value.table.useDialog ? {
				name: "actions",
				fn: j(() => [S("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: r[4] ||= (e) => ci(L.value.userOptions.callbacks.csv),
					style: T({ cursor: ar.value ? "pointer" : "default" })
				}, [Be(A(_n), {
					name: "fileCsv",
					stroke: Ci.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : b("", !0),
			k(t.$slots, "skeleton", {}, () => [A(sr) ? (E(), y(ve, { key: 0 })) : b("", !0)], !0)
		], 46, rt));
	}
}, [["__scopeId", "data-v-593004f0"]]);
//#endregion
export { nt as n, gn as t };
