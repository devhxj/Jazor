import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Jt as r, Kt as i, Ot as a, Pt as o, S as s, X as c, Z as l, ct as u, i as d, jt as f, q as ee, r as te, t as ne, tt as re, w as ie, xt as ae } from "./lib-Bttd6u5E.js";
import { n as oe, t as se } from "./useHints-Dq_w2E8B.js";
import { t as ce } from "./useConfig-DlNpz6P8.js";
import { t as le } from "./usePrinter-DN5bYhTG.js";
import { n as ue, t as de } from "./BaseScanner-DZvpgOjM.js";
import { t as fe } from "./useNestedProp-vPNvh7rV.js";
import { t as pe } from "./useThemeCheck-C43Tcqmk.js";
import { t as me } from "./useChartExport-DNiwdPmb.js";
import { t as he } from "./useTransitions-g_zBREk2.js";
import { t as ge } from "./img-Bnokohej.js";
import { n as _e } from "./Title-BE3qg9xl.js";
import { t as ve } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ye, t as be } from "./useResponsive-ZtArZtUf.js";
import { t as xe } from "./BaseIcon-BfndwIWE.js";
import { t as Se } from "./DefGrad-DVBqDjhO.js";
import { t as Ce } from "./BaseLegendToggle-DZVucLnv.js";
import { t as we } from "./A11yDataTable-DdRsVULz.js";
import { t as Te } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ee } from "./useChartAccessibility-DYqac8yF.js";
import { t as De } from "./Legend-CQxUgOd-.js";
import { t as Oe } from "./usePanZoom-CYU3B4T3.js";
import { t as ke } from "./vue_ui_treemap-DoEtkRN6.js";
import { Fragment as p, Teleport as Ae, computed as m, createBlock as h, createCommentVNode as g, createElementBlock as _, createElementVNode as v, createSlots as je, createTextVNode as Me, createVNode as Ne, defineAsyncComponent as Pe, guardReactiveProps as y, mergeProps as Fe, nextTick as Ie, normalizeClass as Le, normalizeProps as b, normalizeStyle as x, onBeforeUnmount as Re, onMounted as ze, openBlock as S, ref as C, renderList as Be, renderSlot as w, resolveDynamicComponent as Ve, shallowRef as T, toDisplayString as He, toRefs as Ue, unref as E, useCssVars as We, useSlots as Ge, watch as D, withCtx as O, withKeys as Ke, withModifiers as qe } from "vue";
//#region src/treemap.js
function Je(e, t) {
	let n = e.length;
	if (n === 0) throw Error(`Max aspect ratio cannot be computed: ${e} is an empty array`);
	{
		let r = Infinity, i = -Infinity, a = 0;
		for (let t = 0; t < n; t += 1) {
			let n = e[t].normalizedValue;
			n < r && (r = n), n > i && (i = n), a += n;
		}
		return Math.max(t ** 2 * i / a ** 2, a ** 2 / (t ** 2 * r));
	}
}
function Ye(e) {
	let { xOffset: t, yOffset: n, width: r, height: i } = e;
	return {
		x0: t,
		y0: n,
		x1: t + r,
		y1: n + i
	};
}
function Xe(e, t, n) {
	if (e.length === 0) return !0;
	{
		let r = e.concat(t);
		return Je(e, n) >= Je(r, n);
	}
}
function Ze(e) {
	let t = [], n = e.length;
	for (let r = 0; r < n; r += 1) {
		let n = e[r], i = n.length;
		for (let e = 0; e < i; e += 1) t.push(n[e]);
	}
	return t;
}
function Qe(e, t) {
	return at({
		...t,
		children: e
	});
}
function $e(e) {
	return (e.x1 - e.x0) * (e.y1 - e.y0);
}
function et(e, t) {
	let { width: n, height: r, xOffset: i, yOffset: a } = rt(t), o = e.length, s = e.map((e) => e.normalizedValue || 0).reduce((e, t) => e + t, 0), c = s / r, l = s / n, u = i, d = a, f = [];
	if (n >= r) {
		for (let t = 0; t < o; t += 1) {
			let n = e[t], r = d + n.normalizedValue / c, i = {
				x0: u,
				y0: d,
				x1: u + c,
				y1: r
			}, a = Object.assign({}, n, i);
			d = r, f.push(a);
		}
		return f;
	}
	for (let t = 0; t < o; t += 1) {
		let n = e[t], r = u + n.normalizedValue / l, i = {
			x0: u,
			y0: d,
			x1: r,
			y1: d + l
		}, a = Object.assign({}, n, i);
		u = r, f.push(a);
	}
	return f;
}
function tt(e) {
	let t = rt(e), n = t.width, r = t.height;
	return Math.min(n, r);
}
function nt(e, t) {
	let n = e.length, r = t / e.map((e) => e.value ?? 0).reduce((e, t) => e + t, 0), i = [], a, o;
	for (let t = 0; t < n; t += 1) o = e[t], a = Object.assign({}, o, { normalizedValue: o.value * (r || 0) }), i.push(a);
	return i;
}
function rt(e) {
	let { x0: t, y0: n, x1: r, y1: i } = e;
	return {
		xOffset: t,
		yOffset: n,
		width: r - t,
		height: i - n
	};
}
function it(e, t, n, r) {
	let i = e, a = t, o = n, s = r;
	for (;;) {
		let e = i.length;
		if (e === 0) {
			let e = et(a, o);
			return s.concat(e);
		}
		let t = tt(o), n = i[0], r = i.slice(1, e);
		if (Xe(a, n, t)) {
			let e = a.concat(n);
			i = r, a = e, o = o, s = s;
		} else {
			let e = a.length, t = 0;
			for (let n = 0; n < e; n += 1) t += a[n].normalizedValue;
			let n = k(o, t), r = et(a, o), c = s.concat(r);
			i = i, a = [], o = n, s = c;
		}
	}
}
function at(e) {
	if (e.children === void 0 || !e.children.length) return [e];
	{
		let t = it(nt(e.children, $e(e)), [], e, []), n = t.length, r = [];
		for (let e = 0; e < n; e += 1) r.push(at(t[e]));
		return Ze(r);
	}
}
function k(e, t) {
	let { width: n, height: r, xOffset: i, yOffset: a } = rt(e);
	if (n >= r) {
		let e = t / r, o = n - e;
		return Ye({
			xOffset: i + e,
			yOffset: a,
			width: o,
			height: r
		});
	}
	{
		let e = t / n, o = r - e;
		return Ye({
			xOffset: i,
			yOffset: a + e,
			width: n,
			height: o
		});
	}
}
//#endregion
//#region src/components/vue-ui-treemap.vue
var ot = /* @__PURE__ */ e({ default: () => Ft }), st = ["id"], ct = ["id"], lt = ["id"], ut = [
	"tabindex",
	"onClick",
	"onKeydown",
	"data-last-crumb",
	"onMouseenter",
	"onFocus"
], dt = { class: "vue-ui-treemap-crumb-unit" }, ft = { class: "vue-ui-treemap-crumb-unit-label" }, pt = {
	key: 0,
	style: {
		width: "24px",
		display: "flex",
		"align-items": "center"
	}
}, mt = {
	key: 0,
	class: "vue-ui-treemap-crumb-unit-arrow"
}, ht = { style: { position: "relative" } }, gt = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], _t = { key: 0 }, vt = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"rx",
	"stroke",
	"stroke-width",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], yt = ["id"], bt = [
	"x",
	"y",
	"width",
	"height"
], xt = ["clip-path"], St = ["transform"], Ct = ["transform"], wt = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-family",
	"font-weight"
], Tt = { key: 0 }, Et = [
	"x",
	"y",
	"height",
	"width"
], Dt = {
	style: {
		width: "100%",
		height: "100%",
		overflow: "hidden"
	},
	class: "vue-ui-treemap-cell"
}, Ot = {
	key: 0,
	"data-dom-to-png-ignore": "",
	class: "reset-wrapper"
}, kt = {
	key: 1,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, At = {
	key: 6,
	class: "vue-data-ui-watermark"
}, jt = ["id"], Mt = ["onClick"], Nt = ["innerHTML"], Pt = 4, Ft = /*#__PURE__*/ ve({
	__name: "vue-ui-treemap",
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
	setup(e, { expose: ve, emit: Je }) {
		We((e) => ({ v5ebbeef2: vi.value }));
		let Ye = Pe(() => import("./DataTable-BbKgJ5UI.js")), Xe = Pe(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Ze = Pe(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), $e = Pe(() => import("./Tooltip-DhjyfHwz.js")), et = Pe(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), tt = Pe(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), nt = Pe(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_treemap: rt } = ce(), { isThemeValid: it, warnInvalidTheme: at } = pe(), k = e, ot = Je, Ft = Ge(), It = m(() => !!k.dataset && k.dataset.length), Lt = m(() => /^((?!chrome|android).)*safari/i.test(navigator.userAgent)), A = C(ee()), Rt = C(!1), zt = C(""), j = C(!1), Bt = C(0), M = C([]), N = T(null), Vt = T(null), Ht = T(null), Ut = T(null), Wt = T(null), Gt = C(0), Kt = C(0), qt = C(0), P = C([]), Jt = C(null), Yt = C(!1), Xt = C(null), Zt = C(null), Qt = C(null), $t = C(/* @__PURE__ */ new Map()), F = C(null), en = C({
			x: 0,
			y: 0
		}), tn = C("pointer"), nn = C(!1), I = C(dn());
		oe({
			config: () => I.value,
			dataset: () => k.dataset,
			component: "VueUiTreemap",
			rules: [
				se.emptyArray,
				{
					test: (e) => e.length === 1 && e[0]?.children && e[0].children.length < 6 && e[0].children.length > 0 || e.flatMap((e) => e?.children?.length ?? 0).reduce((e, t) => e + t, 0) < 6,
					message: [
						"👀 The number of data points is < 6. Consider:",
						"",
						"▶️ Using another type of chart instead to better show proportions: VueUiDonut, VueUiWaffle"
					]
				},
				{
					test: (e) => e.length === 1 && e[0]?.children && e[0].children.length === 0,
					message: [
						"👀 There is a series defined in the dataset but it has no children items.",
						"",
						"▶️ Add children items to your series."
					]
				}
			]
		});
		let { transitionEnabled: rn } = he({
			config: () => I.value.transitions,
			dataset: () => k.dataset
		}), L = m(() => I.value.userOptions.useCursorPointer), an = m(() => r({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#999999",
					layout: {
						labels: { showDefaultLabels: !1 },
						rects: { stroke: "#6A6A6A" }
					},
					legend: { backgroundColor: "transparent" }
				} }
			},
			userConfig: I.value.skeletonConfig ?? {}
		})), { loading: R, FINAL_DATASET: on, manualLoading: sn } = ue({
			...Ue(k),
			FINAL_CONFIG: I,
			prepareConfig: dn,
			skeletonDataset: k.config?.skeletonDataset ?? [{
				name: "_",
				value: 53,
				color: "#CACACA90",
				children: [
					{
						name: "_",
						value: 21
					},
					{
						name: "_",
						value: 13
					},
					{
						name: "_",
						value: 8
					},
					{
						name: "_",
						value: 5
					},
					{
						name: "_",
						value: 3
					},
					{
						name: "_",
						value: 2
					},
					{
						name: "_",
						value: 1
					}
				]
			}],
			skeletonConfig: r({
				defaultConfig: I.value,
				userConfig: an.value
			})
		}), { userOptionsVisible: cn, setUserOptionsVisibility: ln, keepUserOptionState: un } = Te({ config: I.value }), { svgRef: z } = Ee({ config: I.value.style.chart.title });
		function dn() {
			let e = fe({
				userConfig: k.config,
				defaultConfig: rt
			}), t = e.theme;
			if (!t) return e;
			if (!it.value(e)) return at(e), e;
			let n = fe({
				userConfig: ke[t] || k.config,
				defaultConfig: e
			}), r = fe({
				userConfig: k.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : i[t] || o
			};
		}
		D(() => k.config, (e) => {
			R.value || (I.value = dn()), cn.value = !I.value.userOptions.showOnChartHover, Gt.value += 1, Kt.value += 1, qt.value += 1, B.value.showTable = I.value.table.show, B.value.showTooltip = I.value.style.chart.tooltip.show, B.value.showZoom = I.value.style.chart.zoom?.show ?? !1;
		}, { deep: !0 });
		let { isPrinting: fn, isImaging: pn, generatePdf: mn, generateImage: hn } = le({
			elementId: `treemap_${A.value}`,
			fileName: I.value.style.chart.title.text || "vue-ui-treemap",
			options: I.value.userOptions.print
		}), gn = m(() => I.value.userOptions.show && !I.value.style.chart.title.text), _n = m(() => ie(I.value.customPalette)), B = C({
			showTable: I.value.table.show,
			showTooltip: I.value.style.chart.tooltip.show,
			showZoom: I.value.style.chart.zoom?.show ?? !1
		});
		D(I, () => {
			B.value = {
				showTable: I.value.table.show,
				showTooltip: I.value.style.chart.tooltip.show,
				showZoom: I.value.style.chart.zoom?.show ?? !1
			};
		}, { immediate: !0 });
		let vn = C({
			height: I.value.style.chart.height,
			width: I.value.style.chart.width
		});
		function yn(e) {
			let t = N.value, n = vn.value.height;
			return !t || !n ? e : e * (n / t.getBoundingClientRect().height);
		}
		function bn() {
			if (!Jt.value || X.value.length <= 1) return 0;
			let e = Jt.value, t = getComputedStyle(e);
			return yn(e.offsetHeight + parseFloat(t.marginTop || "0") + parseFloat(t.marginBottom || "0") + parseFloat(t.paddingTop || "0") + parseFloat(t.paddingBottom || "0"));
		}
		let V = m(() => {
			let e = I.value.style.chart.padding, t = vn.value.width, n = vn.value.height, r = bn(), i = e.left, a = t - e.right, o = e.top, s = n - e.bottom - r;
			return {
				left: i,
				top: o,
				right: a,
				bottom: s,
				width: a - i,
				height: Math.max(0, s - o),
				vbWidth: t,
				vbHeight: n - r,
				offsetY: r
			};
		}), H = C(l(on.value)), U = C(H.value), xn = T(/* @__PURE__ */ new Map());
		D([H, () => I.value], () => {
			Qt.value = null, $t.value = /* @__PURE__ */ new Map();
		}, { deep: !0 }), D(() => M.value, () => {
			Qt.value = null, $t.value = /* @__PURE__ */ new Map(), Tn(), qt.value += 1, Kt.value += 1;
		}, { deep: !0 }), D([() => vn.value.width, () => vn.value.height], () => {
			$t.value = /* @__PURE__ */ new Map();
		});
		function Sn(e) {
			Array.isArray(e) && e.forEach((e, t) => {
				e.id ||= ee(), e.sourceColor === void 0 && (e.sourceColor = s(e.color) || null);
				let n = e.sourceColor || xn.value.get(e.id) || _n.value[t] || o[t] || o[t % o.length];
				n = s(n), xn.value.set(e.id, n), e.color = n, Cn(e, n);
			});
		}
		function Cn(e, t) {
			Array.isArray(e.children) && e.children.forEach((n) => {
				n.id ||= ee(), n.parentId = e.id, n.sourceColor === void 0 && (n.sourceColor = s(n.color) || null);
				let r = n.sourceColor || t;
				n.color = r, Cn(n, r);
			});
		}
		function wn(e) {
			let t = Y(e);
			for (; t?.parentId;) t = Y(t.parentId);
			return t?.id ?? null;
		}
		function Tn() {
			let e = H.value.filter((e) => !M.value.includes(e.id));
			if (!P.value.length) {
				U.value = e;
				return;
			}
			let t = P.value[P.value.length - 1], n = Y(t);
			if (!n) {
				P.value = [], U.value = e;
				return;
			}
			let r = wn(n.id);
			if (r && M.value.includes(r)) {
				P.value = [], U.value = e;
				return;
			}
			U.value = [n];
		}
		D(() => on.value, () => {
			H.value = l(on.value), Sn(H.value), Tn(), qt.value += 1, Kt.value += 1;
		}, {
			deep: !0,
			immediate: !0,
			flush: "post"
		});
		let W = T(null), En = T(null);
		ze(() => {
			Yt.value = !0, On();
		});
		let Dn = m(() => I.value.debug);
		ze(() => {
			Ft["chart-background"] && Dn.value && console.warn("VueUiTreemap does not support the #chart-background slot.");
		});
		function On() {
			if (f(k.dataset) && re({
				componentName: "VueUiTreemap",
				type: "dataset",
				debug: Dn.value
			}), Sn(H.value), f(k.dataset) || (sn.value = I.value.loading), I.value.responsive) {
				let e = ye(() => {
					let { width: e, height: t } = be({
						chart: N.value,
						title: I.value.style.chart.title.text ? Vt.value : null,
						legend: I.value.style.chart.legend.show ? Ht.value : null,
						source: Ut.value,
						noTitle: Wt.value
					});
					requestAnimationFrame(() => {
						vn.value.width = e, vn.value.height = t - 12;
					});
				});
				W.value && (En.value && W.value.unobserve(En.value), W.value.disconnect()), W.value = new ResizeObserver(e), En.value = N.value.parentNode, W.value.observe(En.value);
			}
		}
		Re(() => {
			W.value && (En.value && W.value.unobserve(En.value), W.value.disconnect());
		});
		let kn = m(() => U.value.map((e, t) => ({
			...e,
			color: s(e.color) || _n.value[t] || o[t] || o[t % o.length]
		})).filter((e) => !M.value.includes(e.id))), An = m(() => H.value.filter((e) => !M.value.includes(e.id)).map((e) => e.value || 0).reduce((e, t) => e + t, 0)), jn = m({
			get() {
				let e = [...kn.value];
				return I.value.style.chart.layout.sorted && (e = [...kn.value].sort((e, t) => t.value - e.value)), e.map((e) => ({ ...e }));
			},
			set(e) {
				return e;
			}
		});
		function Mn(e, t) {
			return e.value / t;
		}
		function Nn(e, t, n) {
			let r = I.value.style.chart.layout.rects.colorRatio - Mn(t, n);
			return a(e, r < 0 ? 0 : r);
		}
		function Pn(e) {
			return Array.isArray(e.children) && e.children.length > 0;
		}
		function Fn(e) {
			return [...e].sort((e, t) => {
				let n = Pn(e), r = Pn(t);
				if (n !== r) return n - r;
				let i = Number(e.value) || 0;
				return (Number(t.value) || 0) - i;
			});
		}
		function In(e, t, n, r, i) {
			return Fn(e).map((e, a) => {
				let c = e.sourceColor || s(e.color) || s(t) || _n.value[a] || o[a] || o[a % o.length], l = e.sourceColor ? e.sourceColor : Nn(c, e, r), u = Mn(e, r), d = i ?? e.parentId ?? e.id, f = Array.isArray(e.children) && e.children.length && e.children.reduce((e, t) => e + (Number(t.value) || 0), 0) || 1;
				return {
					...e,
					color: l,
					proportion: u,
					parentName: n,
					rootId: d,
					children: Array.isArray(e.children) && e.children.length ? In(e.children, c, e.name, f, d) : void 0
				};
			});
		}
		function Ln(e) {
			let t = Xn(e), n = Rn(e), r = t * .55, i = t * .55, a = t * 1.05, o = t * 1.05, s = t * .35, c = n ? a + o + s : t * .45, l = e.x0 + r, u = e.x1 - r, d = e.y0 + c, f = e.y1 - i;
			return u <= l || f <= d ? null : {
				x0: l,
				y0: d,
				x1: u,
				y1: f
			};
		}
		function Rn(e) {
			return I.value.style.chart.layout.labels.hideUnderProportion === 0 || zn(e);
		}
		function zn(e) {
			let t = Number(I.value.style.chart.layout.labels.hideUnderProportion);
			return !Number.isFinite(t) || t <= 0 || e.proportion > t;
		}
		function Bn(e, t, n = 0, r = null) {
			if (!Array.isArray(e) || !e.length) return [];
			let i = e.map((e) => {
				let { children: t, ...n } = e;
				return { ...n };
			}), a = new Map(e.map((e) => [e.id, Array.isArray(e.children) ? e.children : []])), o = new Map(e.map((e) => [e.id, e])), s = Qe(i, t), c = [];
			for (let e of s) {
				let t = a.get(e.id) || [], i = o.get(e.id), s = {
					...e,
					depth: n,
					parentId: r ?? e.parentId ?? null,
					color: i?.color ?? e.color ?? null,
					children: t,
					isVisibleNode: !0,
					showLabel: Rn(e)
				};
				if (c.push(s), t.length) {
					let r = Ln(s);
					r && c.push(...Bn(t, r, n + 1, e.id));
				}
			}
			return c;
		}
		let Vn = m(() => {
			let e = U.value.length ? U.value : jn.value, t = e.map((e) => Number(e.value) || 0).reduce((e, t) => e + t, 0) || 1;
			return Bn(e.map((e, n) => {
				let r = e.children ? e.children.reduce((e, t) => e + (Number(t.value) || 0), 0) : e.value, { children: i, ...a } = e, c = e.sourceColor || s(e.color) || xn.value.get(e.id) || _n.value[n] || o[n] || o[n % o.length], l = e.sourceColor ? e.sourceColor : Nn(c, e, t);
				return {
					...a,
					id: e.id || ee(),
					parentId: e.parentId ?? null,
					name: e.name,
					value: e.value,
					color: l,
					proportion: (Number(e.value) || 0) / t,
					children: Array.isArray(e.children) && e.children.length ? In(e.children, c, e.name, r || 1) : void 0
				};
			}), {
				x0: V.value.left,
				y0: V.value.top,
				x1: V.value.left + V.value.width,
				y1: V.value.top + V.value.height
			});
		}), G = m(() => Vn.value);
		function Hn(e) {
			return (Z.value && Z.value.id === e.id ? I.value.style.chart.layout.rects.selected.strokeWidth : I.value.style.chart.layout.rects.strokeWidth) * Mr.value;
		}
		function K({ y0: e, y1: t }) {
			return t - e <= 0 ? 1e-4 : Math.round((t - e) * 1e4) / 1e4;
		}
		function q({ x0: e, x1: t }) {
			return t - e <= 0 ? 1e-4 : Math.round((t - e) * 1e4) / 1e4;
		}
		function Un() {
			let e = I.value.style.chart.layout.labels, t = Number(e.minFontSize), n = Number(e.fontSize), r = Math.max(0, Math.min(Number.isFinite(t) ? t : 0, Number.isFinite(n) ? n : 0));
			return {
				lowerBound: r,
				upperBound: Math.max(r, Number.isFinite(n) ? n : r)
			};
		}
		function Wn(e) {
			let t = Math.min(4, Math.max(e * .25, .5));
			return {
				paddingX: t,
				paddingTop: t
			};
		}
		function Gn(e, t) {
			let { paddingX: n, paddingTop: r } = Wn(t), i = t * 1.1;
			return {
				x: e.x0 + n,
				y: e.y0 + r,
				width: Math.max(q(e) - n * 2, 0),
				height: Math.max(K(e) - r * 2, 0),
				lineHeight: i
			};
		}
		function Kn(e, t) {
			return d(I.value.style.chart.layout.labels.formatter, e.value, c({
				p: I.value.style.chart.layout.labels.prefix,
				v: e.value,
				s: I.value.style.chart.layout.labels.suffix,
				r: I.value.style.chart.tooltip.roundingValue
			}), {
				datapoint: e,
				seriesIndex: t
			});
		}
		function qn(e, t) {
			let n = Kn(e, t);
			return Array.isArray(e.children) && e.children.length ? [`${e.name} (${n})`] : [`${e.name}`, n].filter((e) => `${e}`.length);
		}
		function Jn(e, t, n) {
			let r = qn(e, n), i = Gn(e, t);
			return !r.length || i.width <= 0 || i.height <= 0 || r.length * i.lineHeight > i.height ? !1 : r.every((e, n) => ui(e, t, n === 0 ? 500 : 400) <= i.width);
		}
		function Yn(e, t, n, r) {
			let i = qn(e, t), a = Math.max(1, ...i.map((e) => String(e).length)), o = Math.min(18, Math.max(10, Math.ceil(a * .7))), s = Math.max(1, i.length), c = Math.max(q(e) - 1, 1e-4), l = Math.max(K(e) - 1, 1e-4), u = c / (o * .58), d = l / (s * 1.35);
			return Math.max(n, Math.min(r, u, d));
		}
		function Xn(e, t) {
			let { lowerBound: n, upperBound: r } = Un(), i = Math.max(1e-4, Math.min(q(e), K(e))), a = Math.max(n, Math.min(r, i * .9)), o = Math.min(a, Yn(e, t, n, r));
			if (Jn(e, o, t) || !Jn(e, n, t)) return o;
			let s = n, c = o, l = n;
			for (let n = 0; n < 8; n += 1) {
				let n = (s + c) / 2;
				Jn(e, n, t) ? (l = n, s = n) : c = n;
			}
			let u = Math.max(n, o * .7);
			return l >= u ? l : o;
		}
		function Zn(e) {
			j.value = e, Bt.value += 1;
		}
		let J = m(() => ({
			x: 0,
			y: 0,
			width: V.value.vbWidth,
			height: V.value.vbHeight
		}));
		function Y(e, t = H.value) {
			for (let n of t) {
				if (n.id === e) return n;
				if (n.children) {
					let t = Y(e, n.children);
					if (t) return t;
				}
			}
			return null;
		}
		function Qn(e) {
			let t = [], n = Y(e);
			for (; n && n.parentId;) {
				let e = Y(n.parentId);
				if (!e) break;
				t.unshift(e.id), n = e;
			}
			return t;
		}
		let $n = m(() => P.value.length > 0);
		function er() {
			U.value = H.value.slice(), P.value = [], ot("selectDatapoint", void 0);
		}
		function tr() {
			return P.value[P.value.length - 1] ?? null;
		}
		function nr(e, t, n) {
			P.value = [...Qn(e.id), e.id], U.value = [e], I.value.events.datapointClick && I.value.events.datapointClick({
				datapoint: t ?? e,
				seriesIndex: n
			}), ot("selectDatapoint", t ?? e);
		}
		function rr(e, t) {
			let n = tr();
			if (!n) {
				er();
				return;
			}
			let r = Y(n);
			if (!r?.parentId) {
				I.value.events.datapointClick && I.value.events.datapointClick({
					datapoint: e,
					seriesIndex: t
				}), er();
				return;
			}
			let i = Y(r.parentId);
			if (!i) {
				er();
				return;
			}
			P.value = [...Qn(i.id), i.id], U.value = [i], I.value.events.datapointClick && I.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			}), ot("selectDatapoint", e);
		}
		function ir(e, t) {
			if (!e) {
				er();
				return;
			}
			let n = Y(e.id);
			if (n) {
				if (tr() === n.id) {
					rr(e, t);
					return;
				}
				nr(n, e, t);
			}
		}
		function ar(e) {
			return e ? tr() === e.id : !1;
		}
		let X = m(() => {
			let e = [{
				id: null,
				label: "All"
			}];
			if (P.value.length > 0) {
				let t = Y(P.value[P.value.length - 1]), n = [];
				for (; t;) n.unshift(t), t = t.parentId ? Y(t.parentId) : null;
				for (let t of n) e.push({
					id: t.id,
					label: t.name,
					node: t
				});
			}
			return e;
		}), Z = T(null), or = m(() => H.value.map((e, t) => ({
			...e,
			color: s(e.color) || _n.value[t] || o[t] || o[t % o.length],
			shape: "square"
		})).sort((e, t) => t.value - e.value).map((e, t) => {
			let n = e.value / H.value.map((e) => e.value).reduce((e, t) => e + t, 0);
			return {
				...e,
				proportion: n,
				isSegregated: M.value.includes(e.id),
				segregate: () => ur(e),
				opacity: M.value.includes(e.id) ? .5 : 1,
				display: `${e.name}${I.value.style.chart.legend.showPercentage || I.value.style.chart.legend.showValue ? ": " : ""}${I.value.style.chart.legend.showValue ? d(I.value.style.chart.layout.labels.formatter, e.value, c({
					p: I.value.style.chart.layout.labels.prefix,
					v: e.value,
					s: I.value.style.chart.layout.labels.suffix,
					r: I.value.style.chart.legend.roundingValue
				}), { datapoint: e }) : ""}${I.value.style.chart.legend.showPercentage ? M.value.includes(e.id) ? `${I.value.style.chart.legend.showValue ? " (" : ""}- %${I.value.style.chart.legend.showValue ? ")" : ""}` : `${I.value.style.chart.legend.showValue ? " (" : ""}${isNaN(e.value / An.value) ? "-" : (e.value / An.value * 100).toFixed(I.value.style.chart.legend.roundingPercentage)}%${I.value.style.chart.legend.showValue ? ")" : ""}` : ""}`
			};
		})), sr = m(() => ({
			cy: "treemap-div-legend",
			backgroundColor: I.value.style.chart.legend.backgroundColor,
			color: I.value.style.chart.legend.color,
			fontSize: I.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: I.value.style.chart.legend.bold ? "bold" : ""
		})), cr = m(() => M.value.length === or.value.length);
		function lr() {
			M.value.length ? M.value = [] : or.value.forEach((e) => {
				M.value.push(e.id);
			}), ot("selectLegend", H.value.filter((e) => !M.value.includes(e.id)));
		}
		function ur(e) {
			Z.value = null, M.value.includes(e.id) ? M.value = M.value.filter((t) => t !== e.id) : M.value.length < k.dataset.length - 1 && M.value.push(e.id), ot("selectLegend", H.value.filter((e) => !M.value.includes(e.id)));
		}
		function dr(e) {
			return H.value.length ? H.value.find((t) => t.name === e) || (Dn.value && console.warn(`VueUiTreemap - Series name not found "${e}"`), null) : (Dn.value && console.warn("VueUiTreemap - There are no series to show."), null);
		}
		function fr(e) {
			let t = dr(e);
			t !== null && M.value.includes(t.id) && ur({ id: t.id });
		}
		function pr(e) {
			let t = dr(e);
			t !== null && (M.value.includes(t.id) || ur({ id: t.id }));
		}
		function mr({ datapoint: e, seriesIndex: t }) {
			Z.value = null, Rt.value = !1, F.value = null, tn.value = "pointer", I.value.events.datapointLeave && I.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		let hr = C(null);
		function gr({ datapoint: e, seriesIndex: t, triggerMode: n = "pointer" }) {
			if (cr.value || n === "pointer" && Er.value) return;
			I.value.events.datapointEnter && I.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), F.value = t, tn.value = n, Z.value = e, hr.value = {
				datapoint: e,
				seriesIndex: t,
				config: I.value,
				series: kn.value
			};
			let r = I.value.style.chart.tooltip.customFormat;
			if (ae(r) && u(() => r({
				seriesIndex: t,
				datapoint: e,
				series: kn.value,
				config: I.value
			}))) zt.value = r({
				seriesIndex: t,
				datapoint: e,
				series: kn.value,
				config: I.value
			});
			else {
				let n = "";
				n += `<div style="width:100%;text-align:center;border-bottom:1px solid ${I.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${e.name}</div>`, n += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 12 12" height="14" width="14"><rect x="0" y="0" height="12" width="12" stroke="none" fill="${e.color}"/></svg>`, n += `<b>${d(I.value.style.chart.layout.labels.formatter, e.value, c({
					p: I.value.style.chart.layout.labels.prefix,
					v: e.value,
					s: I.value.style.chart.layout.labels.suffix,
					r: I.value.style.chart.tooltip.roundingValue
				}), {
					datapoint: e,
					seriesIndex: t
				})}</b>`, zt.value = `<div>${n}</div>`;
			}
			Rt.value = !0;
		}
		let Q = m(() => ({
			head: G.value.map((e) => ({
				name: e.name,
				color: e.color
			})),
			body: G.value.map((e) => e.value)
		}));
		function _r(e = null) {
			Ie(() => {
				let r = Q.value.head.map((e, t) => [
					[e.name],
					[Q.value.body[t]],
					[isNaN(Q.value.body[t] / An.value) ? "-" : Q.value.body[t] / An.value * 100]
				]), i = [
					[I.value.style.chart.title.text],
					[I.value.style.chart.title.subtitle.text],
					[
						[""],
						["val"],
						["%"]
					]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: I.value.style.chart.title.text || "vue-ui-treemap"
				});
			});
		}
		let vr = m(() => {
			let e = [
				I.value.table.columnNames.series,
				I.value.table.columnNames.value,
				I.value.table.columnNames.percentage
			], t = Q.value.head.map((e, t) => {
				let n = d(I.value.style.chart.layout.labels.formatter, Q.value.body[t], c({
					p: I.value.style.chart.layout.labels.prefix,
					v: Q.value.body[t],
					s: I.value.style.chart.layout.labels.suffix,
					r: I.value.table.td.roundingValue
				}));
				return [
					{
						color: e.color,
						name: e.name,
						shape: "square"
					},
					n,
					isNaN(Q.value.body[t] / An.value) ? "-" : c({
						v: Q.value.body[t] / An.value * 100,
						s: "%",
						r: I.value.table.td.roundingPercentage
					})
				];
			}), n = {
				th: {
					backgroundColor: I.value.table.th.backgroundColor,
					color: I.value.table.th.color,
					outline: I.value.table.th.outline
				},
				td: {
					backgroundColor: I.value.table.td.backgroundColor,
					color: I.value.table.td.color,
					outline: I.value.table.td.outline
				},
				breakpoint: I.value.table.responsiveBreakpoint
			};
			return {
				colNames: [I.value.table.columnNames.series, I.value.table.columnNames.value],
				head: e,
				body: t,
				config: n
			};
		});
		function yr() {
			return G.value;
		}
		function br() {
			B.value.showTable = !B.value.showTable;
		}
		function xr() {
			B.value.showTooltip = !B.value.showTooltip;
		}
		function Sr() {
			B.value.showZoom = !B.value.showZoom;
		}
		let Cr = C(!1);
		function wr() {
			Cr.value = !Cr.value;
		}
		let Tr = m(() => !Cr.value && B.value.showZoom), Er = C(null), Dr = C(!1), { viewBox: $, resetZoom: Or, isZoom: kr, setInitialViewBox: Ar, scale: jr } = Oe(z, {
			x: J.value.x,
			y: J.value.y,
			width: Math.max(10, J.value.width),
			height: Math.max(10, J.value.height)
		}, 1, Tr), Mr = m(() => 1 / jr.value), Nr = m(() => Math.min(1, I.value.style.chart.layout.labels.fontSizeZoomFactor / Math.max(jr.value, 1e-4))), Pr = m(() => `scale(${Nr.value})`), Fr = m(() => B.value.showZoom || kr.value);
		D([
			() => J.value.x,
			() => J.value.y,
			() => J.value.width,
			() => J.value.height
		], () => {
			Ie(() => {
				Ar({
					x: J.value.x,
					y: J.value.y,
					width: Math.max(10, J.value.width),
					height: Math.max(10, J.value.height)
				});
			});
		}, { immediate: !0 });
		function Ir(e) {
			Tr.value && (e.button === void 0 || e.button === 0) && (Er.value = {
				x: e.clientX,
				y: e.clientY
			}, Dr.value = !1);
		}
		function Lr(e) {
			if (!Tr.value || !Er.value) return;
			let t = e.clientX - Er.value.x, n = e.clientY - Er.value.y;
			Math.hypot(t, n) > Pt && (Dr.value = !0);
		}
		function Rr() {
			Er.value = null, Dr.value && window.setTimeout(() => {
				Dr.value = !1;
			}, 0);
		}
		function zr(e, t) {
			Dr.value || ir(e, t);
		}
		let Br = C(null);
		function Vr(e) {
			Br.value = e;
		}
		function Hr() {
			Br.value = null;
		}
		async function Ur({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { width: t, height: n } = N.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ge({
				domElement: N.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: I.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Wr = m(() => {
			let e = I.value.table.useDialog && !I.value.table.show, t = B.value.showTable;
			return {
				component: e ? nt : Ze,
				title: `${I.value.style.chart.title.text}${I.value.style.chart.title.subtitle.text ? `: ${I.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: I.value.table.th.backgroundColor,
					color: I.value.table.th.color,
					headerColor: I.value.table.th.color,
					headerBg: I.value.table.th.backgroundColor,
					isFullscreen: j.value,
					fullscreenParent: N.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: L.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: I.value.style.chart.backgroundColor,
							color: I.value.style.chart.color
						},
						head: {
							backgroundColor: I.value.style.chart.backgroundColor,
							color: I.value.style.chart.color
						}
					}
				}
			};
		});
		D(() => B.value.showTable, (e) => {
			I.value.table.show || (e && I.value.table.useDialog && Xt.value ? Xt.value.open() : "close" in Xt.value && Xt.value.close());
		});
		function Gr() {
			B.value.showTable = !1, Zt.value && Zt.value.setTableIconState(!1);
		}
		let Kr = m(() => or.value.map((e) => ({
			...e,
			name: e.display
		}))), qr = m(() => I.value.style.chart.backgroundColor), Jr = m(() => I.value.style.chart.legend), Yr = m(() => I.value.style.chart.title), { isCallbackImaging: Xr, isCallbackSvg: Zr, generateSvg: Qr, onGenerateImage: $r } = me({
			svg: z,
			title: Yr,
			legend: Jr,
			legendItems: Kr,
			backgroundColor: qr,
			getSvgCallback: () => I.value.userOptions.callbacks.svg,
			generateImage: hn
		});
		function ei(e, t) {
			let n = Xn(e, t), { paddingX: r, paddingTop: i } = Wn(n);
			return {
				fontSize: n,
				paddingX: r,
				paddingTop: i,
				lineHeight: n * 1.1
			};
		}
		function ti(e) {
			return te(e.color ?? I.value.style.chart.backgroundColor);
		}
		function ni({ availableHeight: e, lineHeight: t, lineIndex: n }) {
			if (n === 0) return Infinity;
			let r = e / (n + 1);
			return r < 1e-4 ? -Infinity : t <= 0 ? Infinity : r / t;
		}
		function ri() {
			return Number(I.value.style.chart.layout.labels.hideUnderProportion) === 0;
		}
		function ii(e) {
			let t = Nr.value;
			return e.alwaysShowAllLines ? `scale(${Math.min(t, e.allLinesScaleCap)})` : Pr.value;
		}
		function ai({ rect: e, seriesIndex: t }) {
			if (!e || !I.value.style.chart.layout.labels.showDefaultLabels || !Rn(e)) return null;
			let { fontSize: n, paddingX: r, paddingTop: i, lineHeight: a } = ei(e, t), o = ti(e), s = qn(e, t).map((e) => String(e ?? "")), c = Gn(e, n);
			if (!s.some(Boolean) || c.width <= 0 || c.height <= 0) return null;
			let l = ri(), u = l ? Math.max(1e-4, Math.min(1, c.height / Math.max(s.length * a, 1e-4))) : 1, d = `treemap_clip_${A.value}_${e.id}`, f = e.x0, ee = e.y0, te = Math.max(q(e), 0), ne = Math.max(K(e), 0);
			return {
				key: oi(e),
				clipId: d,
				clipX: f,
				clipY: ee,
				clipWidth: te,
				clipHeight: ne,
				translate: `translate(${e.x0}, ${e.y0})`,
				alwaysShowAllLines: l,
				allLinesScaleCap: u,
				lines: s.map((t, s) => ({
					key: `${e.id}_${s}`,
					text: t,
					x: r,
					y: i + a * s,
					maxZoomFactor: ni({
						availableHeight: c.height,
						lineHeight: a,
						lineIndex: s
					}),
					fill: o,
					fontSize: n,
					fontFamily: I.value.style.fontFamily,
					fontWeight: s === 0 ? 500 : 400
				}))
			};
		}
		function oi(e) {
			return `${e.id}`;
		}
		let si = m(() => Ft.rect || R.value || !I.value.style.chart.layout.labels.showDefaultLabels || cr.value ? [] : G.value.map((e, t) => e.showLabel ? ai({
			rect: e,
			seriesIndex: t
		}) : null).filter(Boolean)), ci;
		function li() {
			return typeof document > "u" ? null : (ci ||= document.createElement("canvas"), ci.getContext("2d"));
		}
		function ui(e, t, n = 400) {
			let r = li();
			return r ? (r.font = `${n} ${t}px ${I.value.style.fontFamily}`, r.measureText(String(e)).width) : e.length * t * .6;
		}
		function di(e) {
			let t = I.value.style.chart.layout.rects.borderRadius, n = q(e), r = K(e);
			return Math.min(t, Math.min(n, r) / 6);
		}
		async function fi() {
			if (ot("copyAlt", {
				config: I.value,
				dataset: G.value
			}), !I.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(I.value.userOptions.callbacks.altCopy({
				config: I.value,
				dataset: G.value
			}));
		}
		function pi() {
			F.value = null, nn.value = !0;
		}
		function mi() {
			F.value = null, tn.value = "pointer", Rt.value = !1, Z.value = null, nn.value = !1;
		}
		function hi(e) {
			if (!z.value || Cr.value || document.activeElement !== z.value || !G.value.length || cr.value) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				F.value = null, tn.value = "pointer", Rt.value = !1, Z.value = null;
				return;
			}
			if (r) {
				if (F.value === null) return;
				let e = G.value[F.value];
				if (!e) return;
				ir(e, F.value);
				return;
			}
			let a = F.value, o = Z.value ? G.value.findIndex((e) => e.id === Z.value.id) : null, s = a !== null && a >= 0 && a < G.value.length, c = o !== null && o >= 0 && o < G.value.length;
			s ? n ? (a += 1, a >= G.value.length && (a = 0)) : t && (--a, a < 0 && (a = G.value.length - 1)) : c ? (a = n ? o + 1 : o - 1, a >= G.value.length && (a = 0), a < 0 && (a = G.value.length - 1)) : a = n ? 0 : G.value.length - 1;
			let l = G.value[a];
			l && (gi(a), gr({
				datapoint: l,
				seriesIndex: a,
				triggerMode: "keyboard"
			}));
		}
		function gi(e) {
			if (!Number.isFinite(e) || !z.value) return;
			let t = G.value[e];
			if (!t) return;
			let n = t.x0 + q(t) / 2, r = t.y0 + K(t) / 2, i = z.value.getBoundingClientRect();
			en.value = {
				x: i.left + (n - $.value.x) / $.value.width * i.width,
				y: i.top + (r - $.value.y) / $.value.height * i.height
			};
		}
		let _i = m(() => ({
			headers: vr.value?.colNames ?? [],
			rows: vr.value?.body ?? []
		})), vi = m(() => I.value.style.chart.color);
		return ve({
			getData: yr,
			getImage: Ur,
			generateCsv: _r,
			generateImage: hn,
			generateSvg: Qr,
			generatePdf: mn,
			hideSeries: pr,
			showSeries: fr,
			toggleTable: br,
			toggleTooltip: xr,
			toggleZoom: Sr,
			toggleAnnotator: wr,
			toggleFullscreen: Zn,
			resetZoom: Or,
			copyAlt: fi
		}), (e, t) => (S(), _("div", {
			ref_key: "treemapChart",
			ref: N,
			class: Le(`vue-data-ui-component vue-ui-treemap ${j.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${I.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: x(`font-family:${I.value.style.fontFamily};width:100%; ${I.value.responsive ? "height: 100%;" : ""} text-align:center;background:${I.value.style.chart.backgroundColor}`),
			id: `treemap_${A.value}`,
			onMouseenter: t[3] ||= () => E(ln)(!0),
			onMouseleave: t[4] ||= () => E(ln)(!1)
		}, [
			v("div", {
				id: `chart-instructions-${A.value}`,
				class: "sr-only"
			}, [v("p", null, He(I.value.a11y.translations.keyboardNavigation), 1)], 8, ct),
			_i.value?.rows?.length ? (S(), h(we, {
				key: 0,
				uid: A.value,
				head: _i.value.headers,
				body: _i.value.rows,
				notice: I.value.a11y.translations.tableAvailable,
				caption: I.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : g("", !0),
			I.value.userOptions.buttons.annotator ? (S(), h(E(Xe), {
				key: 1,
				svgRef: E(z),
				backgroundColor: I.value.style.chart.backgroundColor,
				color: I.value.style.chart.color,
				active: Cr.value,
				isCursorPointer: L.value,
				onClose: wr
			}, {
				"annotator-action-close": O(() => [w(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": O(({ color: t }) => [w(e.$slots, "annotator-action-color", b(y({ color: t })), void 0, !0)]),
				"annotator-action-draw": O(({ mode: t }) => [w(e.$slots, "annotator-action-draw", b(y({ mode: t })), void 0, !0)]),
				"annotator-action-undo": O(({ disabled: t }) => [w(e.$slots, "annotator-action-undo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": O(({ disabled: t }) => [w(e.$slots, "annotator-action-redo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": O(({ disabled: t }) => [w(e.$slots, "annotator-action-delete", b(y({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : g("", !0),
			gn.value ? (S(), _("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Wt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : g("", !0),
			I.value.style.chart.title.text ? (S(), _("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Vt,
				style: x(`width:100%;background:${I.value.style.chart.backgroundColor};padding-bottom:6px`)
			}, [(S(), h(_e, {
				key: `title_${Gt.value}`,
				config: {
					title: {
						cy: "treemap-div-title",
						...I.value.style.chart.title
					},
					subtitle: {
						cy: "treemap-div-subtitle",
						...I.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 4)) : g("", !0),
			v("div", { id: `legend-top-${A.value}` }, null, 8, lt),
			I.value.userOptions.show && It.value && (E(un) || E(cn)) ? (S(), h(E(et), {
				ref_key: "userOptionsRef",
				ref: Zt,
				key: `user_option_${Bt.value}`,
				backgroundColor: I.value.style.chart.backgroundColor,
				color: I.value.style.chart.color,
				isPrinting: E(fn),
				isImaging: E(pn),
				uid: A.value,
				hasTooltip: I.value.userOptions.buttons.tooltip && I.value.style.chart.tooltip.show,
				hasPdf: I.value.userOptions.buttons.pdf,
				hasXls: I.value.userOptions.buttons.csv,
				hasImg: I.value.userOptions.buttons.img,
				hasSvg: I.value.userOptions.buttons.svg,
				hasTable: I.value.userOptions.buttons.table,
				hasFullscreen: I.value.userOptions.buttons.fullscreen,
				hasAltCopy: I.value.userOptions.buttons.altCopy,
				isFullscreen: j.value,
				isTooltip: B.value.showTooltip,
				titles: { ...I.value.userOptions.buttonTitles },
				chartElement: N.value,
				position: I.value.userOptions.position,
				hasAnnotator: I.value.userOptions.buttons.annotator,
				isAnnotation: Cr.value,
				callbacks: I.value.userOptions.callbacks,
				printScale: I.value.userOptions.print.scale,
				tableDialog: I.value.table.useDialog,
				isCursorPointer: L.value,
				hasZoom: I.value.userOptions.buttons.zoom,
				isZoom: B.value.showZoom,
				onToggleFullscreen: Zn,
				onGeneratePdf: E(mn),
				onGenerateCsv: _r,
				onGenerateImage: E($r),
				onGenerateSvg: E(Qr),
				onToggleTable: br,
				onToggleTooltip: xr,
				onToggleAnnotator: wr,
				onToggleZoom: Sr,
				onCopyAlt: fi,
				style: x({ visibility: E(un) ? E(cn) ? "visible" : "hidden" : "visible" })
			}, je({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: O(({ isOpen: t, color: n }) => [w(e.$slots, "menuIcon", b(y({
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
					fn: O(({ toggleFullscreen: t, isFullscreen: n }) => [w(e.$slots, "optionFullscreen", b(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: O(({ toggleAnnotator: t, isAnnotator: n }) => [w(e.$slots, "optionAnnotator", b(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionZoom ? {
					name: "optionZoom",
					fn: O(({ toggleZoom: t, isZoomLocked: n }) => [w(e.$slots, "optionZoom", b(y({
						toggleZoom: t,
						isZoomLocked: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: O(({ altCopy: t }) => [w(e.$slots, "optionAltCopy", b(y({ altCopy: t })), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: O(() => [w(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: O(() => [w(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.hasZoom.isZoom.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : g("", !0),
			X.value.length > 1 ? (S(), _("nav", {
				key: 5,
				class: "vue-ui-treemap-breadcrumbs",
				"data-dom-to-png-ignore": "",
				ref_key: "breadcrumbsNav",
				ref: Jt
			}, [(S(!0), _(p, null, Be(X.value, (n, r) => (S(), _("span", {
				role: "button",
				tabindex: r < X.value.length - 1 ? 0 : void 0,
				key: n.id || "root",
				onClick: (e) => r === X.value.length - 1 ? () => {} : ir(n.node),
				onKeydown: [Ke(qe((e) => r === X.value.length - 1 ? void 0 : ir(n.node), ["prevent"]), ["enter"]), Ke(qe((e) => r === X.value.length - 1 ? void 0 : ir(n.node), ["prevent"]), ["space"])],
				class: "vue-ui-treemap-crumb",
				"data-last-crumb": r === X.value.length - 1,
				style: x({
					color: I.value.style.chart.color,
					cursor: L.value ? "pointer" : "default"
				}),
				onMouseenter: (e) => Vr(r),
				onMouseleave: Hr,
				onFocus: (e) => Vr(r),
				onBlur: Hr
			}, [v("span", dt, [v("span", ft, [w(e.$slots, "breadcrumb-label", Fe({ ref_for: !0 }, {
				crumb: n,
				isRoot: r === 0,
				isFocus: Br.value === r
			}), () => [r === 0 ? (S(), _("div", pt, [Ne(xe, {
				name: Br.value === 0 ? "homeFilled" : "home",
				stroke: I.value.style.chart.color
			}, null, 8, ["name", "stroke"])])) : (S(), _(p, { key: 1 }, [Me(He(n.label), 1)], 64))], !0)]), r < X.value.length - 1 ? (S(), _("span", mt, [w(e.$slots, "breadcrumb-arrow", {}, () => [t[5] ||= Me(" › ", -1)], !0)])) : g("", !0)])], 44, ut))), 128))], 512)) : g("", !0),
			v("div", ht, [
				(S(), _("svg", {
					ref_key: "svgRef",
					ref: z,
					xmlns: E(ne),
					"aria-describedby": `chart-instructions-${A.value}`,
					class: Le({
						"vue-data-ui-fullscreen--on": j.value,
						"vue-data-ui-fulscreen--off": !j.value,
						"vue-data-ui-zoom-plus": !$n.value,
						"vue-data-ui-zoom-minus": $n.value,
						"vue-data-ui-no-transition": !E(rn),
						loading: E(R)
					}),
					viewBox: `${E($).x} ${E($).y} ${E($).width <= 0 ? 10 : E($).width} ${E($).height <= 0 ? 10 : E($).height}`,
					style: x(`max-width:100%; overflow:${Fr.value ? "hidden" : "visible"}; background:transparent;color:${I.value.style.chart.color}`),
					tabindex: "0",
					onFocus: pi,
					onBlur: mi,
					onKeydown: hi,
					onPointerdown: Ir,
					onPointermove: Lr,
					onPointerup: Rr,
					onPointercancel: Rr,
					onPointerleave: Rr
				}, [
					Ne(E(tt)),
					(S(!0), _(p, null, Be(G.value, (e, t) => (S(), _("g", { key: `tgrad_${e.id}` }, [I.value.style.chart.layout.rects.gradient.show ? (S(), _("defs", _t, [Ne(Se, {
						t: "radial",
						id: `tgrad_${e.id}`,
						gradientTransform: "translate(-1, -1.000001) scale(2, 2)",
						stops: [[
							"18%",
							e.color,
							1
						], [
							"100%",
							E(a)(e.color, I.value.style.chart.layout.rects.gradient.intensity / 100),
							1
						]]
					}, null, 8, ["id", "stops"])])) : g("", !0)]))), 128)),
					v("g", null, [
						(S(!0), _(p, null, Be(G.value, (e, t) => (S(), _("g", { key: `rect_${e.id}_${e.depth}` }, [v("rect", {
							x: e.x0,
							y: e.y0,
							height: K(e),
							width: q(e),
							fill: Lt.value ? e.color ?? I.value.style.chart.backgroundColor : I.value.style.chart.layout.rects.gradient.show ? cr.value ? I.value.style.chart.backgroundColor : `url(#tgrad_${e.id})` : e.color ?? I.value.style.chart.backgroundColor,
							rx: E(jr) > 1 ? di(e) * Mr.value : di(e),
							stroke: Z.value && Z.value.id === e.id ? I.value.style.chart.layout.rects.selected.stroke : I.value.style.chart.layout.rects.stroke,
							"stroke-width": Hn(e),
							onClick: qe((n) => zr(e, t), ["stop"]),
							onMouseenter: () => gr({
								datapoint: e,
								seriesIndex: t,
								triggerMode: "pointer"
							}),
							onMouseleave: (n) => mr({
								datapoint: e,
								seriesIndex: t
							}),
							style: x(`opacity:${Z.value ? Z.value.id === e.id ? 1 : I.value.style.chart.layout.rects.selected.unselectedOpacity : 1}`),
							class: Le([
								"vue-ui-treemap-rect",
								E(rn) ? "vue-data-ui-transition" : "",
								ar(e) ? "vue-data-ui-zoom-minus" : "vue-data-ui-zoom-plus"
							])
						}, null, 46, vt)]))), 128)),
						(S(!0), _(p, null, Be(si.value, (e) => (S(), _("g", {
							key: e.key,
							style: { "pointer-events": "none" }
						}, [v("defs", null, [v("clipPath", {
							id: e.clipId,
							clipPathUnits: "userSpaceOnUse"
						}, [v("rect", {
							class: Le({ "vue-data-ui-transition": E(rn) }),
							x: e.clipX,
							y: e.clipY,
							width: e.clipWidth,
							height: e.clipHeight
						}, null, 10, bt)], 8, yt)]), v("g", { "clip-path": `url(#${e.clipId})` }, [v("g", { transform: e.translate }, [v("g", {
							class: Le({ "vue-data-ui-transition": E(rn) }),
							transform: ii(e)
						}, [(S(!0), _(p, null, Be(e.lines, (t) => (S(), _(p, { key: t.key }, [t.text && (e.alwaysShowAllLines || Nr.value <= t.maxZoomFactor) ? (S(), _("text", {
							key: 0,
							x: t.x,
							y: t.y,
							fill: t.fill,
							"font-size": t.fontSize,
							"font-family": t.fontFamily,
							"font-weight": t.fontWeight,
							class: Le({ "vue-data-ui-transition": E(rn) }),
							"text-anchor": "start",
							"dominant-baseline": "text-before-edge"
						}, He(t.text), 11, wt)) : g("", !0)], 64))), 128))], 10, Ct)], 8, St)], 8, xt)]))), 128)),
						e.$slots.rect ? (S(), _("g", Tt, [(S(!0), _(p, null, Be(G.value, (t, n) => (S(), _("g", { key: `slot_${t.id}_${t.depth}` }, [(S(), _("foreignObject", {
							x: t.x0,
							y: t.y0,
							height: K(t),
							width: q(t),
							class: "vue-ui-treemap-cell-foreignObject",
							style: {
								"pointer-events": "none",
								overflow: "hidden"
							}
						}, [v("div", Dt, [E(R) ? g("", !0) : w(e.$slots, "rect", Fe({ ref_for: !0 }, {
							rect: {
								...t,
								height: K(t),
								width: q(t),
								isSelected: !Z.value || Z.value.id === t.id
							},
							shouldShow: zn(t) || $n.value,
							fontSize: Xn(t, n),
							isZoom: $n.value,
							textColor: E(te)(t.color)
						}), void 0, !0, 0)])], 8, Et))]))), 128))])) : g("", !0)
					]),
					w(e.$slots, "svg", b(y({
						svg: V.value,
						isZoom: $n.value,
						rect: Z.value,
						config: I.value,
						isPrintingImg: E(fn) || E(pn) || E(Xr),
						isPrintingSvg: E(Zr)
					})), void 0, !0)
				], 46, gt)),
				E(kr) ? (S(), _("div", Ot, [w(e.$slots, "reset-action", { reset: E(Or) }, () => [v("button", {
					"data-cy-reset": "",
					tabindex: "0",
					role: "button",
					class: "vue-data-ui-refresh-button",
					style: x({
						background: I.value.style.chart.backgroundColor,
						cursor: L.value ? "pointer" : "default"
					}),
					onClick: t[0] ||= (e) => E(Or)(!0)
				}, [Ne(xe, {
					name: "refresh",
					stroke: I.value.style.chart.color
				}, null, 8, ["stroke"])], 4)], !0)])) : g("", !0),
				e.$slots.hint ? (S(), _("div", kt, [w(e.$slots, "hint", b(y({
					hint: I.value.a11y.translations.keyboardNavigation,
					isVisible: nn.value
				})), void 0, !0)])) : g("", !0)
			]),
			e.$slots.watermark ? (S(), _("div", At, [w(e.$slots, "watermark", b(y({ isPrinting: E(fn) || E(pn) || E(Xr) || E(Zr) })), void 0, !0)])) : g("", !0),
			v("div", { id: `legend-bottom-${A.value}` }, null, 8, jt),
			Yt.value && (I.value.style.chart.legend.show || e.$slots.legend) ? (S(), h(Ae, {
				key: 7,
				to: I.value.style.chart.legend.position === "top" ? `#legend-top-${A.value}` : `#legend-bottom-${A.value}`
			}, [v("div", {
				ref_key: "chartLegend",
				ref: Ht
			}, [w(e.$slots, "legend", { legend: or.value }, () => [I.value.style.chart.legend.show ? (S(), h(De, {
				key: `legend_${qt.value}`,
				legendSet: or.value,
				config: sr.value,
				id: `treemap_legend_${A.value}`,
				isCursorPointer: L.value,
				onClickMarker: t[1] ||= ({ legend: e }) => ur(e)
			}, {
				item: O(({ legend: e, index: t }) => [E(R) ? g("", !0) : (S(), _("div", {
					key: 0,
					onClick: (t) => ur(e),
					style: x(`opacity:${M.value.includes(e.id) ? .5 : 1}`)
				}, He(e.display), 13, Mt))]),
				legendToggle: O(() => [or.value.length > 2 && I.value.style.chart.legend.selectAllToggle.show && !E(R) ? (S(), h(Ce, {
					key: 0,
					backgroundColor: I.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: I.value.style.chart.legend.selectAllToggle.color,
					fontSize: I.value.style.chart.legend.fontSize,
					checked: M.value.length > 0,
					isCursorPointer: L.value,
					onToggle: lr
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
				"id",
				"isCursorPointer"
			])) : g("", !0)], !0)], 512)], 8, ["to"])) : g("", !0),
			e.$slots.source ? (S(), _("div", {
				key: 8,
				ref_key: "source",
				ref: Ut,
				dir: "auto"
			}, [w(e.$slots, "source", {}, void 0, !0)], 512)) : g("", !0),
			Ne(E($e), {
				teleportTo: I.value.style.chart.tooltip.teleportTo,
				show: B.value.showTooltip && Rt.value,
				backgroundColor: I.value.style.chart.tooltip.backgroundColor,
				color: I.value.style.chart.tooltip.color,
				fontSize: I.value.style.chart.tooltip.fontSize,
				borderRadius: I.value.style.chart.tooltip.borderRadius,
				borderColor: I.value.style.chart.tooltip.borderColor,
				borderWidth: I.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: I.value.style.chart.tooltip.backgroundOpacity,
				position: I.value.style.chart.tooltip.position,
				offsetX: I.value.style.chart.tooltip.offsetX,
				offsetY: I.value.style.chart.tooltip.offsetY,
				parent: N.value,
				content: zt.value,
				isFullscreen: j.value,
				isCustom: E(ae)(I.value.style.chart.tooltip.customFormat),
				smooth: I.value.style.chart.tooltip.smooth,
				backdropFilter: I.value.style.chart.tooltip.backdropFilter,
				smoothForce: I.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: I.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: tn.value === "keyboard",
				a11yPosition: en.value
			}, {
				"tooltip-before": O(() => [w(e.$slots, "tooltip-before", b(y({ ...hr.value })), void 0, !0)]),
				tooltip: O(() => [w(e.$slots, "tooltip", b(y({ ...hr.value })), void 0, !0)]),
				"tooltip-after": O(() => [w(e.$slots, "tooltip-after", b(y({ ...hr.value })), void 0, !0)]),
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
			It.value && I.value.userOptions.buttons.table ? (S(), h(Ve(Wr.value.component), Fe({ key: 9 }, Wr.value.props, {
				ref_key: "tableUnit",
				ref: Xt,
				onClose: Gr
			}), je({
				content: O(() => [(S(), h(E(Ye), {
					key: `table_${Kt.value}`,
					colNames: vr.value.colNames,
					head: vr.value.head,
					body: vr.value.body,
					config: vr.value.config,
					title: I.value.table.useDialog ? "" : Wr.value.title,
					withCloseButton: !I.value.table.useDialog,
					isCursorPointer: L.value,
					onClose: Gr
				}, {
					th: O(({ th: e }) => [v("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, Nt)]),
					td: O(({ td: e }) => [Me(He(e.name || e), 1)]),
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
			}, [I.value.table.useDialog ? {
				name: "title",
				fn: O(() => [Me(He(Wr.value.title), 1)]),
				key: "0"
			} : void 0, I.value.table.useDialog ? {
				name: "actions",
				fn: O(() => [v("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[2] ||= (e) => _r(I.value.userOptions.callbacks.csv),
					style: x({ cursor: L.value ? "pointer" : "default" })
				}, [Ne(xe, {
					name: "fileCsv",
					stroke: Wr.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : g("", !0),
			w(e.$slots, "skeleton", {}, () => [E(R) ? (S(), h(de, { key: 0 })) : g("", !0)], !0)
		], 46, st));
	}
}, [["__scopeId", "data-v-da6e1542"]]);
//#endregion
export { ot as n, Ft as t };
