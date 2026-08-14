import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Gt as r, Jt as i, Kt as a, Ot as o, Pt as s, S as c, W as l, X as u, Y as d, i as f, jt as p, q as m, r as h, t as g, tt as _, w as ee, xt as te } from "./lib-Bttd6u5E.js";
import { n as ne, t as re } from "./useHints-Dq_w2E8B.js";
import { t as ie } from "./useConfig-DlNpz6P8.js";
import { t as ae } from "./usePrinter-DN5bYhTG.js";
import { n as oe, t as se } from "./BaseScanner-DZvpgOjM.js";
import { t as ce } from "./useNestedProp-vPNvh7rV.js";
import { t as le } from "./useThemeCheck-C43Tcqmk.js";
import { t as ue } from "./useChartExport-DNiwdPmb.js";
import { t as de } from "./useTransitions-g_zBREk2.js";
import { t as fe } from "./img-Bnokohej.js";
import { n as pe } from "./Title-BE3qg9xl.js";
import { t as me } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as he, t as ge } from "./useResponsive-ZtArZtUf.js";
import { t as _e } from "./DefGrad-DVBqDjhO.js";
import { t as ve } from "./A11yDataTable-DdRsVULz.js";
import { t as ye } from "./useUserOptionState-DK-_1ddE.js";
import { t as be } from "./useChartAccessibility-DYqac8yF.js";
import { t as xe } from "./usePanZoom-CYU3B4T3.js";
import { t as Se } from "./vue_ui_circle_pack-DZC_rdfn.js";
import { Fragment as v, computed as y, createBlock as b, createCommentVNode as x, createElementBlock as S, createElementVNode as C, createSlots as Ce, createTextVNode as we, createVNode as Te, defineAsyncComponent as w, guardReactiveProps as T, mergeProps as Ee, nextTick as De, normalizeClass as E, normalizeProps as D, normalizeStyle as O, onBeforeUnmount as Oe, onMounted as ke, openBlock as k, ref as A, renderList as Ae, renderSlot as j, resolveDynamicComponent as je, toDisplayString as M, toRefs as Me, unref as N, useCssVars as Ne, watch as Pe, withCtx as P } from "vue";
//#region src/packCircles.js
function Fe(e, t = 0) {
	let n = e.reduce((e, { x: n, r }) => Math.min(e, n - r - t), Infinity), r = e.reduce((e, { x: n, r }) => Math.max(e, n + r + t), -Infinity), i = e.reduce((e, { y: n, r }) => Math.min(e, n - r - t), Infinity), a = e.reduce((e, { y: n, r }) => Math.max(e, n + r + t), -Infinity);
	return [
		n,
		i,
		r - n,
		a - i
	];
}
function Ie(e, t, n) {
	let r = e._, i = e.next._, a = r.r + i.r, o = (r.x * i.r + i.x * r.r) / a, s = (r.y * i.r + i.y * r.r) / a;
	return Math.max(Math.abs(o * n), Math.abs(s * t));
}
function Le(e, t, n) {
	let r = Ie, i = e.length;
	if (!i) return e;
	e.sort((e, t) => t.r - e.r);
	let a, o, s;
	if (a = e[0], a.x = 0, a.y = 0, !(i > 1)) return e;
	if (o = e[1], a.x = -o.r, o.x = a.r, o.y = 0, !(i > 2)) return ze(e, n, t), e;
	Re(o, a, s = e[2]), a = new Ke(a), o = new Ke(o), s = new Ke(s), a.next = s.previous = o, o.next = a.previous = s, s.next = o.previous = a;
	packLoop: for (let c = 3; c < i; ++c) {
		Re(a._, o._, s = e[c]), s = new Ke(s);
		let i = o.next, l = a.previous, u = o._.r, d = a._.r;
		do
			if (u <= d) {
				if (F(i._, s._)) {
					o = i, a.next = o, o.previous = a, --c;
					continue packLoop;
				}
				u += i._.r, i = i.next;
			} else {
				if (F(l._, s._)) {
					a = l, a.next = o, o.previous = a, --c;
					continue packLoop;
				}
				d += l._.r, l = l.previous;
			}
		while (i !== l.next);
		s.previous = a, s.next = o, a.next = o.previous = o = s;
		let f = a, p = r(a, n, t), m = a.next;
		for (; m !== o;) {
			let e = r(m, n, t);
			e < p && (f = m, p = e), m = m.next;
		}
		a = f, o = a.next;
	}
	return ze(e, n, t), He(e, n, t), e;
}
function Re(e, t, n) {
	let r = e.x - t.x, i = e.y - t.y, a = r * r + i * i;
	if (a) {
		let o = (t.r + n.r) ** 2, s = (e.r + n.r) ** 2;
		if (o > s) {
			let t = (a + s - o) / (2 * a), c = Math.sqrt(Math.max(0, s / a - t * t));
			n.x = e.x - t * r - c * i, n.y = e.y - t * i + c * r;
		} else {
			let e = (a + o - s) / (2 * a), c = Math.sqrt(Math.max(0, o / a - e * e));
			n.x = t.x + e * r - c * i, n.y = t.y + e * i + c * r;
		}
	} else n.x = t.x + n.r, n.y = t.y;
}
function F(e, t) {
	let n = e.r + t.r - 1e-6, r = t.x - e.x, i = t.y - e.y;
	return n > 0 && n * n > r * r + i * i;
}
function ze(e, t, n) {
	if (e.length < 4) return;
	let r = e[0].r, i = e.findIndex((e) => e.r < r * .6), a = i === -1 ? Math.min(e.length, 12) : i, o = e.slice(0, Math.max(3, a));
	for (let t = 0; t < 80; t += 1) {
		let t = !1;
		for (let n = 0; n < o.length; n += 1) {
			let r = o[n], i = .06 * (1 - n / Math.max(1, o.length)), a = r.x * (1 - i), s = r.y * (1 - i);
			Be(r, a, s, e, n) && (r.x = a, r.y = s, t = !0);
		}
		if (Ve(o), !t) break;
	}
	let s = o.reduce((e, t) => e + t.x, 0) / o.length, c = o.reduce((e, t) => e + t.y, 0) / o.length;
	for (let t of e) t.x -= s, t.y -= c;
}
function Be(e, t, n, r) {
	let i = e.x, a = e.y;
	e.x = t, e.y = n;
	for (let t of r) if (t !== e && F(e, t)) return e.x = i, e.y = a, !1;
	return e.x = i, e.y = a, !0;
}
function Ve(e) {
	for (let t = 0; t < 6; t += 1) {
		let t = !1;
		for (let n = 0; n < e.length - 1; n += 1) for (let r = n + 1; r < e.length; r += 1) {
			let i = e[n], a = e[r], o = a.x - i.x, s = a.y - i.y, c = Math.sqrt(o * o + s * s) || 1e-6, l = i.r + a.r;
			if (c < l) {
				let e = (l - c) / 2, n = o / c, r = s / c;
				i.x -= n * e, i.y -= r * e, a.x += n * e, a.y += r * e, t = !0;
			}
		}
		if (!t) break;
	}
}
function He(e, t, n) {
	if (e.length < 6) return;
	let r = e[0].r, i = e.findIndex((e) => e.r <= r * .45);
	if (i !== -1) for (let r = i; r < e.length; r += 1) {
		let i = e[r], a = Ue(i, e.slice(0, r), t, n);
		a && (i.x = a.x, i.y = a.y);
	}
}
function Ue(e, t, n, r) {
	if (t.length < 2) return null;
	let i = null, a = Infinity;
	for (let o = 0; o < t.length - 1; o += 1) for (let s = o + 1; s < t.length; s += 1) {
		let c = t[o], l = t[s], u = We(c, l, e.r);
		for (let o of u) {
			if (!o) continue;
			let s = {
				...e,
				x: o.x,
				y: o.y
			};
			if (!I(s, t)) continue;
			let c = Ge(s, t, n, r);
			c < a && (a = c, i = o);
		}
	}
	return i;
}
function We(e, t, n) {
	let r = e.r + n, i = t.r + n, a = t.x - e.x, o = t.y - e.y, s = a * a + o * o, c = Math.sqrt(s);
	if (!c || c > r + i || c < Math.abs(r - i)) return [];
	let l = (r * r - i * i + s) / (2 * c), u = r * r - l * l;
	if (u < 0) return [];
	let d = Math.sqrt(u), f = a / c, p = o / c, m = e.x + l * f, h = e.y + l * p, g = -p * d, _ = f * d;
	return [{
		x: m + g,
		y: h + _
	}, {
		x: m - g,
		y: h - _
	}];
}
function I(e, t) {
	for (let n of t) if (F(n, e)) return !1;
	return !0;
}
function Ge(e, t, n, r) {
	let i = e.x / Math.max(1, n), a = e.y / Math.max(1, r), o = i * i + a * a, s = [];
	for (let n of t) {
		let t = e.x - n.x, r = e.y - n.y, i = Math.sqrt(t * t + r * r) - (e.r + n.r);
		s.push(i);
	}
	s.sort((e, t) => e - t);
	let c = s[0] ?? 0, l = s[1] ?? c, u = s[2] ?? l;
	return c * 8 + l * 4 + u * 2 + o * e.r * 12;
}
var Ke = class {
	constructor(e) {
		this._ = e, this.next = null, this.previous = null;
	}
}, qe = /* @__PURE__ */ e({ default: () => mt }), Je = ["id"], Ye = ["id"], Xe = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Ze = [
	"x",
	"y",
	"width",
	"height"
], Qe = { key: 0 }, $e = [
	"x",
	"y",
	"width",
	"height",
	"stroke",
	"stroke-width",
	"fill",
	"rx",
	"onMouseenter",
	"onMouseout",
	"onClick"
], et = [
	"x",
	"y",
	"width",
	"height",
	"stroke",
	"stroke-width",
	"fill",
	"rx"
], tt = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], nt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], rt = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], it = {
	key: 1,
	style: { pointerEvents: "none" }
}, at = [
	"stroke",
	"stroke-width",
	"stroke-dasharray",
	"opacity",
	"d"
], ot = [
	"x",
	"y",
	"width",
	"height",
	"rx",
	"ry",
	"fill",
	"stroke",
	"stroke-width",
	"filter"
], st = [
	"fill",
	"x",
	"y",
	"rx",
	"width",
	"height"
], ct = [
	"transform",
	"font-size",
	"fill",
	"font-family"
], lt = ["dy"], ut = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, dt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, ft = {
	key: 6,
	"data-dom-to-png-ignore": "",
	class: "reset-wrapper"
}, pt = ["innerHTML"], mt = /*#__PURE__*/ me({
	__name: "vue-ui-circle-pack",
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
	setup(e, { expose: me, emit: Ie }) {
		Ne((e) => ({ v0ca658e8: or.value }));
		let Re = w(() => import("./Tooltip-DhjyfHwz.js")), F = w(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), ze = w(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Be = w(() => import("./DataTable-BbKgJ5UI.js")), Ve = w(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), He = w(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Ue = w(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), We = w(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), I = e, Ge = Ie, { vue_ui_circle_pack: Ke } = ie(), { isThemeValid: qe, warnInvalidTheme: mt } = le(), ht = y(() => !!I.dataset && I.dataset.length), L = A(m()), R = A(null), gt = A(null), _t = A(null), vt = A(0), yt = A(0), bt = A(0), xt = A(null), z = A(null), St = A(null), Ct = A(!1), wt = A(""), B = A(null), V = A(null), Tt = A({
			x: 0,
			y: 0
		}), Et = A("pointer"), Dt = A(!1), H = A(Pt());
		ne({
			config: () => H.value,
			dataset: () => I.dataset,
			component: "VueUiCirclePack",
			rules: [
				re.emptyArray,
				{
					test: (e) => e.length === 1 && (!e[0].children || e[0]?.children.length === 0),
					message: [
						"👀 The dataset only has a single series. Consider:",
						"",
						"▶️ Using a value display instead of a chart component, or using VueUiKpi."
					]
				},
				{
					test: (e) => e.length === 1 && e[0]?.children && e[0]?.children.length < 6,
					message: [
						"👀 The number of datapoints is probably too low for this type of chart. Consider:",
						"",
						"▶️ Using VueUiDonut or VueUiWaffle instead."
					]
				}
			]
		});
		let { transitionEnabled: U } = de({
			config: () => H.value.transitions,
			dataset: () => I.dataset
		}), W = y(() => H.value.userOptions.useCursorPointer), Ot = y(() => i({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					circles: {
						stroke: "#6A6A6A",
						labels: {
							name: { show: !1 },
							value: { show: !1 }
						}
					}
				} }
			},
			userConfig: H.value.skeletonConfig ?? {}
		})), { loading: kt, FINAL_DATASET: At } = oe({
			...Me(I),
			FINAL_CONFIG: H,
			prepareConfig: Pt,
			skeletonDataset: I.config?.skeletonDataset ?? [
				{
					name: "_",
					value: 13,
					color: "#F2F2F2"
				},
				{
					name: "_",
					value: 8,
					color: "#DBDBDB"
				},
				{
					name: "_",
					value: 5,
					color: "#ADADAD"
				},
				{
					name: "_",
					value: 3,
					color: "#969696"
				},
				{
					name: "_",
					value: 2,
					color: "#808080"
				},
				{
					name: "_",
					value: 1,
					color: "#696969"
				}
			],
			skeletonConfig: i({
				defaultConfig: H.value,
				userConfig: Ot.value
			})
		}), { svgRef: G } = be({ config: H.value.style.chart.title }), { userOptionsVisible: jt, setUserOptionsVisibility: Mt, keepUserOptionState: Nt } = ye({ config: H.value });
		function Pt() {
			let e = ce({
				userConfig: I.config,
				defaultConfig: Ke
			}), t = e.theme;
			if (!t) return e;
			if (!qe.value(e)) return mt(e), e;
			let n = ce({
				userConfig: Se[t] || I.config,
				defaultConfig: e
			}), r = ce({
				userConfig: I.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : a[t] || s
			};
		}
		Pe(() => I.config, (e) => {
			H.value = Pt(), jt.value = !H.value.userOptions.showOnChartHover, rn(), vt.value += 1, yt.value += 1, K.value.showTable = H.value.table.show, K.value.showTooltip = H.value.style.chart.tooltip.show, K.value.showZoom = H.value.style.chart.zoom?.show ?? !1;
		}, { deep: !0 });
		let { isPrinting: Ft, isImaging: It, generatePdf: Lt, generateImage: Rt } = ae({
			elementId: `vue-ui-circle-pack_${L.value}`,
			fileName: H.value.style.chart.title.text || "vue-ui-circle-pack",
			options: H.value.userOptions.print
		}), zt = y(() => H.value.userOptions.show && !H.value.style.chart.title.text), K = A({
			showTable: H.value.table.show,
			showTooltip: H.value.style.chart.tooltip.show,
			showZoom: H.value.style.chart.zoom?.show ?? !1
		});
		Pe(H, () => {
			K.value = {
				showTable: H.value.table.show,
				showTooltip: H.value.style.chart.tooltip.show,
				showZoom: H.value.style.chart.zoom?.show ?? !1
			};
		}, { immediate: !0 });
		let q = A({
			h: 10,
			w: 10
		}), Bt = A([
			0,
			0,
			100,
			100
		]), Vt = A({
			x: 0,
			y: 0,
			width: 100,
			height: 100
		}), Ht = y(() => H.value.debug), Ut = y(() => {
			let e = H.value.style?.chart || {};
			return e.dimensions && typeof e.dimensions.width == "number" ? e.dimensions.width : typeof e.width == "number" ? e.width : 300;
		}), Wt = y(() => {
			let e = H.value.style?.chart || {};
			return e.dimensions && typeof e.dimensions.height == "number" ? e.dimensions.height : typeof e.height == "number" ? e.height : 300;
		}), J = A([]), Y = A([]), X = A(null), Z = A(null);
		function Gt(e, t) {
			q.value = {
				w: e,
				h: t
			};
			let n = on.value.map(Kt);
			if (!n.length) {
				let n = {
					x: 0,
					y: 0,
					width: e,
					height: t
				};
				J.value = [], Y.value = [], Bt.value = [
					n.x,
					n.y,
					n.width,
					n.height
				], Vt.value = { ...n }, Sn(n);
				return;
			}
			let r = Le(n, t, e), [i, a, o, s] = Fe(r, 1), c = Math.min(o ? e / o : 1, s ? t / s : 1), l = (e - o * c) / 2, u = (t - s * c) / 2, d = r.map((e) => ({
				...e,
				x: (e.x - i) * c + l,
				y: (e.y - a) * c + u,
				r: e.r * c
			}));
			J.value = Qt(d), Y.value = J.value;
			let f = Nn({
				x: 0,
				y: 0,
				width: e,
				height: t
			});
			Bt.value = [
				f.x,
				f.y,
				f.width,
				f.height
			], Vt.value = { ...f }, Sn(f);
		}
		function Kt(e) {
			return {
				...e,
				children: Array.isArray(e.children) ? e.children.map(Kt) : []
			};
		}
		function qt(e) {
			let t = Number(e?.value);
			return Number.isFinite(t) && t > 0 ? t : Array.isArray(e?.children) ? e.children.reduce((e, t) => e + qt(t), 0) : 0;
		}
		function Jt(e, t) {
			let n = a[H.value.theme || "default"] || s, r = an.value.length ? an.value : n.length ? n : s;
			return c(e.color) || r[t % r.length] || s[t % s.length];
		}
		function Yt(e) {
			return !Array.isArray(e.children) || !e.children.length ? 1 : e.children.reduce((e, t) => e + Yt(t), 0);
		}
		function Xt(e, t = null, n = 0, r = []) {
			return Array.isArray(e) ? e.map((e, i) => {
				let a = qt(e);
				if (!Number.isFinite(a) || a <= 0) return null;
				let o = m(), s = [...r, i], l = s.reduce((e, t, n) => e + t + n, 0), u = e.color || t?.color ? c(e.color || t?.color) : Jt(e, l), d = Xt(e.children, {
					id: o,
					name: e.name,
					color: u,
					rootId: t?.rootId ?? o
				}, n + 1, s);
				return {
					...e,
					value: a,
					r: a,
					id: o,
					color: u,
					depth: n,
					parentId: t?.id ?? null,
					parentName: t?.name ?? null,
					rootId: t?.rootId ?? o,
					hasChildren: d.length > 0,
					childCount: d.length,
					leafCount: d.length ? Yt({ children: d }) : 1,
					hierarchyPath: s,
					children: d
				};
			}).filter(Boolean) : [];
		}
		function Zt(e) {
			if (!Array.isArray(e.children) || !e.children.length) return [];
			let t = e.r * 2;
			if (!t) return [];
			let n = Le(e.children.map(Kt), t, t), [r, i, a, o] = Fe(n, 1), s = Math.min(a ? t / a : 1, o ? t / o : 1) * .9, c = a * s, l = o * s, u = e.x - c / 2, d = e.y - l / 2;
			return n.map((e) => ({
				...e,
				x: (e.x - r) * s + u,
				y: (e.y - i) * s + d,
				r: e.r * s
			}));
		}
		function Qt(e) {
			let t = [];
			return e.forEach((e) => {
				let n = Zt(e);
				t.push({
					...e,
					children: n
				}), t.push(...Qt(n));
			}), t;
		}
		function $t(e, t) {
			return !e || !t || !Array.isArray(e.hierarchyPath) || !Array.isArray(t.hierarchyPath) || e.hierarchyPath.length <= t.hierarchyPath.length ? !1 : t.hierarchyPath.every((t, n) => e.hierarchyPath[n] === t);
		}
		function en(e) {
			return e.hasChildren ? H.value.style.chart.circles.labels.parents.show : H.value.style.chart.circles.labels.children.show;
		}
		function tn() {
			if (!H.value.responsive || !R.value) return;
			let e = he(() => {
				let { width: e, height: t } = ge({
					chart: R.value,
					title: H.value.style.chart.title.text ? gt.value : null,
					legend: null,
					source: xt.value,
					noTitle: _t.value
				});
				requestAnimationFrame(() => {
					!e || !t || (G.value && (G.value.setAttribute("width", e), G.value.setAttribute("height", t)), Gt(e, t));
				});
			});
			X.value && (Z.value && X.value.unobserve(Z.value), X.value.disconnect()), X.value = new ResizeObserver(e), Z.value = R.value.parentNode || R.value, Z.value && X.value.observe(Z.value), e();
		}
		function nn() {
			X.value && (Z.value && X.value.unobserve(Z.value), X.value.disconnect(), X.value = null, Z.value = null);
		}
		async function rn() {
			p(I.dataset) && _({
				componentName: "VueUiCirclePack",
				type: "dataset",
				debug: Ht.value
			});
			let e = Ut.value, t = Wt.value;
			Gt(e, t), H.value.responsive ? tn() : nn();
		}
		ke(rn), Oe(() => {
			nn();
		}), Pe(() => At.value, async (e) => {
			await rn();
		}, { deep: !0 });
		let an = y(() => ee(H.value.customPalette)), on = y(() => Xt(At.value)), sn = y(() => J.value.length ? Math.max(...J.value.map((e) => e.r)) : 0);
		function cn(e, t) {
			return sn.value ? t / sn.value * e : 0;
		}
		function ln(e, t) {
			H.value.events.datapointLeave && H.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}), (Et.value !== "keyboard" || B.value?.id !== e?.id) && (Ct.value = !1, B.value = null, V.value = null, Et.value = "pointer");
		}
		function un(e, t) {
			Ge("selectDatapoint", e), H.value.events.datapointClick && H.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		let dn = A(null), fn = A(!1);
		function pn(e, t, n = "pointer") {
			B.value = e, V.value = t, Et.value = n, H.value.events.datapointEnter && H.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), dn.value = {
				datapoint: e,
				seriesIndex: t,
				config: H.value,
				series: Y.value
			}, Ct.value = !0;
			let r = H.value.style.chart.tooltip.customFormat;
			if (fn.value = !1, te(r)) try {
				let n = r({
					seriesIndex: t,
					datapoint: e,
					series: Y.value,
					config: H.value
				});
				typeof n == "string" && (wt.value = n, fn.value = !0);
			} catch {
				console.warn("Custom format cannot be applied."), fn.value = !1;
			}
			if (!fn.value) {
				let t = "";
				t += `
            <div style="display:flex;align-items:center;gap:4px;">
                <svg viewBox="0 0 10 10" height="${H.value.style.chart.tooltip.fontSize}" width="${H.value.style.chart.tooltip.fontSize}">
                    <circle
                        cx="5"
                        cy="5"
                        r="5"
                        fill="${H.value.style.chart.circles.gradient.show ? `url(#${e.id})` : e.color}"
                    />
                </svg>
                <span>${e.name}: <b>${mn(e)}</b></span>
            </div>
        `, wt.value = t;
			}
		}
		function mn(e) {
			return f(H.value.style.chart.circles.labels.value.formatter, e.value, u({
				p: H.value.style.chart.circles.labels.value.prefix,
				v: e.value,
				s: H.value.style.chart.circles.labels.value.suffix,
				r: H.value.style.chart.circles.labels.value.rounding
			}));
		}
		function hn(e) {
			if (!e) return 0;
			let t = mn(e), n = e.r / (t.length || 1) * (t.length === 1 ? 1 : 2);
			return Math.min(e.r / 2.5, n);
		}
		let Q = A(!1);
		function gn(e) {
			Q.value = e, bt.value += 1;
		}
		let _n = A(!1);
		function vn() {
			_n.value = !_n.value;
		}
		let yn = y(() => !_n.value && K.value.showZoom), { viewBox: $, resetZoom: bn, isZoom: xn, setInitialViewBox: Sn } = xe(G, {
			x: 0,
			y: 0,
			width: Math.max(10, q.value.w),
			height: Math.max(10, q.value.h)
		}, H.value.style.chart.zoom?.speed ?? 1, yn);
		function Cn(e, t, n = 0) {
			return !(e.x + e.width + n < t.x || t.x + t.width + n < e.x || e.y + e.height + n < t.y || t.y + t.height + n < e.y);
		}
		function wn(e, t, n = 0) {
			let r = Math.min(Math.max(t.x, e.x - n), e.x + e.width + n), i = Math.min(Math.max(t.y, e.y - n), e.y + e.height + n), a = t.x - r, o = t.y - i;
			return a * a + o * o <= t.r * t.r;
		}
		function Tn(e, t, n = 1) {
			let r = Math.max(4 * n, Math.min(t.width, t.height) * .01);
			return {
				...e,
				x: Math.min(Math.max(e.x, t.x + r), t.x + t.width - e.width - r),
				y: Math.min(Math.max(e.y, t.y + r), t.y + t.height - e.height - r)
			};
		}
		function En(e) {
			let t = [e.name];
			return H.value.style.chart.circles.labels.value.show && t.push(mn(e)), t.filter(Boolean);
		}
		function Dn(e, t, n, r, i) {
			let a = e.x, o = e.y;
			return {
				right: {
					x: a + e.r + i,
					y: o - n / 2,
					anchorX: a + e.r,
					anchorY: o
				},
				left: {
					x: a - e.r - i - t,
					y: o - n / 2,
					anchorX: a - e.r,
					anchorY: o
				},
				top: {
					x: a - t / 2,
					y: o - e.r - i - n,
					anchorX: a,
					anchorY: o - e.r
				},
				bottom: {
					x: a - t / 2,
					y: o + e.r + i,
					anchorX: a,
					anchorY: o + e.r
				},
				topRight: {
					x: a + e.r * .7 + i,
					y: o - e.r * .7 - i - n,
					anchorX: a + e.r * .7,
					anchorY: o - e.r * .7
				},
				topLeft: {
					x: a - e.r * .7 - i - t,
					y: o - e.r * .7 - i - n,
					anchorX: a - e.r * .7,
					anchorY: o - e.r * .7
				},
				bottomRight: {
					x: a + e.r * .7 + i,
					y: o + e.r * .7 + i,
					anchorX: a + e.r * .7,
					anchorY: o + e.r * .7
				},
				bottomLeft: {
					x: a - e.r * .7 - i - t,
					y: o + e.r * .7 + i,
					anchorX: a - e.r * .7,
					anchorY: o + e.r * .7
				}
			}[r];
		}
		function On(e, t) {
			let n = [], r = t.get(e.parentId);
			for (; r;) n.push(r), r = t.get(r.parentId);
			return n;
		}
		function kn(e, t, n, r, i, a) {
			let o = e.x, s = e.y, c = t || e, l = a * .6, u = {
				right: {
					x: c.x + c.r + a,
					y: s - r / 2
				},
				left: {
					x: c.x - c.r - a - n,
					y: s - r / 2
				},
				top: {
					x: o - n / 2,
					y: c.y - c.r - a - r
				},
				bottom: {
					x: o - n / 2,
					y: c.y + c.r + a
				},
				topRight: {
					x: c.x + c.r + a,
					y: c.y - c.r - a - r - l
				},
				topLeft: {
					x: c.x - c.r - a - n,
					y: c.y - c.r - a - r - l
				},
				bottomRight: {
					x: c.x + c.r + a,
					y: c.y + c.r + a + l
				},
				bottomLeft: {
					x: c.x - c.r - a - n,
					y: c.y + c.r + a + l
				}
			}[i], d = u.x + n / 2, f = u.y + r / 2, p = Math.atan2(f - s, d - o);
			return {
				...u,
				anchorX: o + Math.cos(p) * e.r,
				anchorY: s + Math.sin(p) * e.r
			};
		}
		function An(e, t = {}) {
			let { clamp: n = !0, scale: r = 1 } = t;
			if (!J.value.length || !q.value.w || !q.value.h || !H.value.style.chart.parentTooltips.show) return [];
			let i = Math.max(8, H.value.style.chart.parentTooltips.fontSizeRatio * 10) * r, a = i * 1.25, o = i * .75, s = i * .55, c = Math.max(8 * r, Math.min(e.width, e.height) * .025), l = Math.max(2 * r, Math.min(e.width, e.height) * .006), u = [
				"right",
				"left",
				"top",
				"bottom",
				"topRight",
				"topLeft",
				"bottomRight",
				"bottomLeft"
			], d = [], f = new Map(J.value.map((e) => [e.id, e]));
			return J.value.filter((e) => e.hasChildren && e.name).sort((e, t) => t.r - e.r).forEach((t) => {
				let p = On(t, f).at(-1), m = En(t), h = m.reduce((e, t) => Math.max(e, String(t).length), 0), g = Math.max(i * 5, h * i * .62 + o * 2), _ = m.length * a + s * 2, ee = p ? u.map((e) => kn(t, p, g, _, e, c)) : [], te = u.map((e) => Dn(t, g, _, e, c)), ne = [...ee, ...te].map((t) => {
					let i = n ? Tn({
						x: t.x,
						y: t.y,
						width: g,
						height: _
					}, e, r) : {
						x: t.x,
						y: t.y,
						width: g,
						height: _
					};
					return {
						...i,
						anchorX: t.anchorX,
						anchorY: t.anchorY,
						lineX: i.x + i.width / 2,
						lineY: i.y + i.height / 2
					};
				}).find((e) => !d.some((t) => Cn(e, t, l)) && !J.value.some((t) => wn(e, t, l)));
				ne && d.push({
					...ne,
					datapoint: t,
					id: `parent_tooltip_${t.id}`,
					color: t.color,
					lines: m,
					fontSize: i,
					lineHeight: a,
					paddingX: o,
					paddingY: s,
					scale: r
				});
			}), d;
		}
		function jn(e) {
			let t = Vt.value;
			if (!t.width || !t.height || !e.width || !e.height) return 1;
			let n = Math.min(e.width / t.width, e.height / t.height);
			return Number.isFinite(n) && n > 0 ? n : 1;
		}
		let Mn = y(() => An($.value, {
			clamp: !0,
			scale: jn($.value)
		}));
		function Nn(e) {
			let t = An(e, { clamp: !1 });
			if (!t.length) return e;
			let n = Math.min(e.x, ...t.map((e) => e.x)), r = Math.min(e.y, ...t.map((e) => e.y)), i = Math.max(e.x + e.width, ...t.map((e) => e.x + e.width)), a = Math.max(e.y + e.height, ...t.map((e) => e.y + e.height)), o = Math.max(8, Math.min(e.width, e.height) * .025);
			return {
				x: n - o,
				y: r - o,
				width: i - n + o * 2,
				height: a - r + o * 2
			};
		}
		function Pn(e) {
			let t = [];
			function n(e, r = 0, i = "") {
				e.forEach((e) => {
					t.push({
						name: e.name,
						value: e.value,
						color: e.color,
						parentName: i,
						depth: r
					}), Array.isArray(e.children) && e.children.length && n(e.children, r + 1, e.name);
				});
			}
			return n(e), t;
		}
		let Fn = y(() => {
			let e = Pn(on.value);
			return {
				head: Y.value.map((e) => ({
					name: e.name,
					value: e.value,
					color: e.color
				})).toSorted((e, t) => t.value - e.value),
				body: e.map((e) => e.value),
				hierarchy: e
			};
		}), In = y(() => Pn(on.value).map((e) => ({
			...e,
			name: `${"- ".repeat(e.depth)}${e.name}`
		})));
		function Ln(e = null) {
			De(() => {
				let r = In.value.map((e) => [
					[e.name],
					[e.parentName || ""],
					[e.depth],
					[e.value]
				]), i = [
					[H.value.style.chart.title.text],
					[H.value.style.chart.title.subtitle.text],
					[
						[H.value.table.columnNames.datapoint],
						["Parent"],
						["Depth"],
						[H.value.table.columnNames.value]
					]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: H.value.style.chart.title.text || "vue-ui-circle-pack"
				});
			});
		}
		let Rn = y(() => {
			let e = [
				H.value.table.columnNames.datapoint,
				H.value.table.columnNames.parent,
				H.value.table.columnNames.depth,
				H.value.table.columnNames.value
			], t = Fn.value.hierarchy.map((e) => {
				let t = u({
					p: H.value.style.chart.circles.labels.value.prefix,
					v: e.value,
					s: H.value.style.chart.circles.labels.value.suffix,
					r: H.value.style.chart.circles.labels.value.rounding
				});
				return [
					{
						color: e.color,
						name: `${"  ".repeat(e.depth)}${e.name}`
					},
					e.parentName ?? "",
					e.depth ?? 0,
					t
				];
			}), n = {
				th: {
					backgroundColor: H.value.table.th.backgroundColor,
					color: H.value.table.th.color,
					outline: H.value.table.th.outline
				},
				td: {
					backgroundColor: H.value.table.td.backgroundColor,
					color: H.value.table.td.color,
					outline: H.value.table.td.outline
				},
				breakpoint: H.value.table.responsiveBreakpoint
			};
			return {
				colNames: [
					H.value.table.columnNames.datapoint,
					H.value.table.columnNames.parent,
					H.value.table.columnNames.depth,
					H.value.table.columnNames.value
				],
				head: e,
				body: t,
				config: n
			};
		});
		function zn() {
			K.value.showTable = !K.value.showTable;
		}
		function Bn() {
			K.value.showTooltip = !K.value.showTooltip;
		}
		function Vn() {
			K.value.showZoom = !K.value.showZoom;
		}
		function Hn() {
			return on.value;
		}
		async function Un({ scale: e = 2 } = {}) {
			if (!R.value) return;
			let { width: t, height: n } = R.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await fe({
				domElement: R.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: H.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Wn = y(() => {
			let e = H.value.table.useDialog && !H.value.table.show, t = K.value.showTable;
			return {
				component: e ? We : ze,
				title: `${H.value.style.chart.title.text}${H.value.style.chart.title.subtitle.text ? `: ${H.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: H.value.table.th.backgroundColor,
					color: H.value.table.th.color,
					headerColor: H.value.table.th.color,
					headerBg: H.value.table.th.backgroundColor,
					isFullscreen: Q.value,
					fullscreenParent: R.value,
					forcedWidth: Math.min(500, window.innerWidth * .8),
					isCursorPointer: W.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: H.value.style.chart.backgroundColor,
							color: H.value.style.chart.color
						},
						head: {
							backgroundColor: H.value.style.chart.backgroundColor,
							color: H.value.style.chart.color
						}
					}
				}
			};
		});
		Pe(() => K.value.showTable, (e) => {
			H.value.table.show || (e && H.value.table.useDialog && z.value ? z.value.open() : z.value && "close" in z.value && z.value.close());
		});
		function Gn() {
			K.value.showTable = !1, St.value && St.value.setTableIconState(!1);
		}
		let Kn = y(() => H.value.style.chart.backgroundColor), qn = y(() => H.value.style.chart.title), { isCallbackImaging: Jn, isCallbackSvg: Yn, generateSvg: Xn, onGenerateImage: Zn } = ue({
			svg: G,
			title: qn,
			legend: null,
			legendItems: null,
			backgroundColor: Kn,
			getSvgCallback: () => H.value.userOptions.callbacks.svg,
			generateImage: Rt
		});
		async function Qn() {
			if (Ge("copyAlt", {
				config: H.value,
				dataset: on.value,
				flattenedDataset: Y.value
			}), !H.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(H.value.userOptions.callbacks.altCopy({
				config: H.value,
				dataset: on.value,
				flattenedDataset: Y.value
			}));
		}
		function $n(e) {
			if (!Number.isFinite(e) || !G.value) return;
			let t = J.value[e];
			if (!t) return;
			let n = r(t.x, t.y, G.value);
			n && (Tt.value = n);
		}
		function er() {
			if (V.value !== null) {
				let e = J.value[V.value];
				e && ln(e, V.value);
			}
			V.value = null, Et.value = "pointer", Ct.value = !1, B.value = null;
		}
		function tr() {
			V.value = null, Dt.value = !0;
		}
		function nr() {
			er(), Dt.value = !1;
		}
		function rr(e, t) {
			let n = J.value.length;
			return n ? e === null || e < 0 || e >= n ? t === "next" ? 0 : n - 1 : t === "previous" ? (e - 1 + n) % n : (e + 1) % n : null;
		}
		function ir(e) {
			if (!G.value || _n.value || document.activeElement !== G.value || !J.value.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				er();
				return;
			}
			if (r) {
				if (V.value === null) return;
				let e = J.value[V.value];
				if (!e) return;
				un(e, V.value);
				return;
			}
			let a = V.value;
			a === null || a < 0 || a >= J.value.length ? a = n ? 0 : J.value.length - 1 : n ? a = rr(a, "next") : t && (a = rr(a, "previous"));
			let o = J.value[a];
			o && (V.value = a, $n(a), pn(o, a, "keyboard"));
		}
		let ar = y(() => ({
			headers: Rn.value?.colNames ?? [],
			rows: Rn.value?.body?.map((e) => [e[0]?.name ?? "", e[1]]) ?? []
		})), or = y(() => H.value.style.chart.color);
		return me({
			getData: Hn,
			getImage: Un,
			generateCsv: Ln,
			generatePdf: Lt,
			generateImage: Rt,
			generateSvg: Xn,
			toggleTable: zn,
			toggleAnnotator: vn,
			toggleFullscreen: gn,
			copyAlt: Qn,
			toggleZoom: Vn,
			resetZoom: bn
		}), (e, t) => (k(), S("div", {
			id: `vue-ui-circle-pack_${L.value}`,
			class: E(`vue-data-ui-component vue-ui-circle-pack ${Q.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${N(kt) ? "loading" : ""}`),
			ref_key: "circlePackChart",
			ref: R,
			style: O(`font-family:${H.value.style.fontFamily};text-align:center;background:${H.value.style.chart.backgroundColor};`),
			onMouseenter: t[2] ||= () => N(Mt)(!0),
			onMouseleave: t[3] ||= () => N(Mt)(!1)
		}, [
			C("div", {
				id: `chart-instructions-${L.value}`,
				class: "sr-only"
			}, [C("p", null, M(H.value.a11y.translations.keyboardNavigation), 1)], 8, Ye),
			ar.value?.rows?.length ? (k(), b(ve, {
				key: 0,
				uid: L.value,
				head: ar.value.headers,
				body: ar.value.rows,
				notice: H.value.a11y.translations.tableAvailable,
				caption: H.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : x("", !0),
			H.value.userOptions.buttons.annotator ? (k(), b(N(He), {
				key: 1,
				svgRef: N(G),
				backgroundColor: H.value.style.chart.backgroundColor,
				color: H.value.style.chart.color,
				active: _n.value,
				scale: sn.value / 100,
				isCursorPointer: W.value,
				onClose: vn
			}, {
				"annotator-action-close": P(() => [j(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": P(({ color: t }) => [j(e.$slots, "annotator-action-color", D(T({ color: t })), void 0, !0)]),
				"annotator-action-draw": P(({ mode: t }) => [j(e.$slots, "annotator-action-draw", D(T({ mode: t })), void 0, !0)]),
				"annotator-action-undo": P(({ disabled: t }) => [j(e.$slots, "annotator-action-undo", D(T({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": P(({ disabled: t }) => [j(e.$slots, "annotator-action-redo", D(T({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": P(({ disabled: t }) => [j(e.$slots, "annotator-action-delete", D(T({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"scale",
				"isCursorPointer"
			])) : x("", !0),
			j(e.$slots, "userConfig", {}, void 0, !0),
			zt.value ? (k(), S("div", {
				key: 2,
				ref_key: "noTitle",
				ref: _t,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : x("", !0),
			H.value.style.chart.title.text ? (k(), S("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: gt,
				style: "width:100%;background:transparent;padding-bottom:12px"
			}, [(k(), b(pe, {
				key: `title_${vt.value}`,
				config: {
					title: {
						cy: "donut-div-title",
						...H.value.style.chart.title
					},
					subtitle: {
						cy: "donut-div-subtitle",
						...H.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : x("", !0),
			H.value.userOptions.show && ht.value && (N(Nt) || N(jt)) ? (k(), b(N(Ve), {
				ref_key: "userOptionsRef",
				ref: St,
				key: `user_option_${bt.value}`,
				backgroundColor: H.value.style.chart.backgroundColor,
				color: H.value.style.chart.color,
				isPrinting: N(Ft),
				isImaging: N(It),
				uid: L.value,
				hasTooltip: H.value.userOptions.buttons.tooltip,
				isTooltip: K.value.showTooltip,
				hasLabel: !1,
				hasPdf: H.value.userOptions.buttons.pdf,
				hasImg: H.value.userOptions.buttons.img,
				hasSvg: H.value.userOptions.buttons.svg,
				hasXls: H.value.userOptions.buttons.csv,
				hasTable: H.value.userOptions.buttons.table,
				hasFullscreen: H.value.userOptions.buttons.fullscreen,
				hasAltCopy: H.value.userOptions.buttons.altCopy,
				isFullscreen: Q.value,
				chartElement: R.value,
				position: H.value.userOptions.position,
				callbacks: H.value.userOptions.callbacks,
				printScale: H.value.userOptions.print.scale,
				titles: { ...H.value.userOptions.buttonTitles },
				hasAnnotator: H.value.userOptions.buttons.annotator,
				isAnnotation: _n.value,
				tableDialog: H.value.table.useDialog,
				isCursorPointer: W.value,
				hasZoom: H.value.userOptions.buttons.zoom,
				isZoom: K.value.showZoom,
				onToggleFullscreen: gn,
				onGeneratePdf: N(Lt),
				onGenerateCsv: Ln,
				onGenerateImage: N(Zn),
				onGenerateSvg: N(Xn),
				onToggleTable: zn,
				onToggleTooltip: Bn,
				onToggleAnnotator: vn,
				onToggleZoom: Vn,
				onCopyAlt: Qn,
				style: O({ visibility: N(Nt) ? N(jt) ? "visible" : "hidden" : "visible" })
			}, Ce({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: P(({ isOpen: t, color: n }) => [j(e.$slots, "menuIcon", D(T({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: P(() => [j(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: P(() => [j(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: P(() => [j(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: P(() => [j(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: P(() => [j(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: P(() => [j(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: P(({ toggleFullscreen: t, isFullscreen: n }) => [j(e.$slots, "optionFullscreen", D(T({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: P(({ toggleAnnotator: t, isAnnotator: n }) => [j(e.$slots, "optionAnnotator", D(T({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionZoom ? {
					name: "optionZoom",
					fn: P(({ toggleZoom: t, isZoomLocked: n }) => [j(e.$slots, "optionZoom", D(T({
						toggleZoom: t,
						isZoomLocked: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: P(({ altCopy: t }) => [j(e.$slots, "optionAltCopy", D(T({ altCopy: t })), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: P(() => [j(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: P(() => [j(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.isTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.chartElement.position.callbacks.printScale.titles.hasAnnotator.isAnnotation.tableDialog.isCursorPointer.hasZoom.isZoom.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : x("", !0),
			C("div", { class: E({
				"vue-ui-circle-pack-svg-container": !0,
				"not-responsive": !H.value.responsive
			}) }, [(k(), S("svg", {
				ref_key: "svgRef",
				ref: G,
				xmlns: N(g),
				"aria-describedby": `chart-instructions-${L.value}`,
				viewBox: `${N($).x} ${N($).y} ${N($).width} ${N($).height}`,
				preserveAspectRatio: "xMidYMid meet",
				class: E({
					"vue-data-ui-fullscreen--on": Q.value,
					"vue-data-ui-fulscreen--off": !Q.value,
					"not-responsive": !H.value.responsive,
					"vue-data-ui-no-transition": !N(U)
				}),
				style: O(`display:block;${H.value.responsive ? "width:100%;height:auto" : "height:100%;"};overflow:${K.value.showZoom ? "hidden" : "visible"};background:transparent;color:${H.value.style.chart.color};background:${H.value.style.chart.backgroundColor};`),
				tabindex: "0",
				onFocus: tr,
				onBlur: nr,
				onKeydown: ir
			}, [
				Te(N(Ue)),
				e.$slots["chart-background"] ? (k(), S("foreignObject", {
					key: 0,
					x: N($).x,
					y: N($).y,
					width: q.value.w,
					height: q.value.h,
					style: { pointerEvents: "none" }
				}, [j(e.$slots, "chart-background", {}, void 0, !0)], 8, Ze)) : x("", !0),
				(k(!0), S(v, null, Ae(J.value, (t, n) => (k(), S(v, { key: t.id }, [
					C("defs", null, [Te(_e, {
						t: "radial",
						id: t.id,
						fy: "30%",
						stops: [
							[
								"10%",
								N(o)(t.color, H.value.style.chart.circles.gradient.intensity / 100),
								1
							],
							[
								"90%",
								N(d)(t.color, .1),
								1
							],
							[
								"100%",
								t.color,
								1
							]
						]
					}, null, 8, ["id", "stops"])]),
					e.$slots.pattern ? (k(), S("g", Qe, [C("defs", null, [j(e.$slots, "pattern", Ee({ ref_for: !0 }, {
						...t,
						patternId: `pattern_${L.value}_${t.id}`
					}), void 0, !0)])])) : x("", !0),
					C("rect", {
						class: E({ "vue-data-ui-transition": N(U) }),
						x: t.x - t.r,
						y: t.y - t.r,
						width: t.r * 2,
						height: t.r * 2,
						stroke: H.value.style.chart.circles.stroke,
						"vector-effect": "non-scaling-stroke",
						"stroke-width": H.value.style.chart.circles.strokeWidth * (sn.value || 1) / 100,
						fill: H.value.style.chart.circles.gradient.show ? `url(#${t.id})` : t.color,
						rx: t.r,
						onMouseenter: (e) => pn(t, n, "pointer"),
						onMouseout: (e) => ln(t, n),
						onClick: (e) => un(t, n)
					}, null, 42, $e),
					e.$slots.pattern ? (k(), S("rect", {
						key: 1,
						class: E({ "vue-data-ui-transition": N(U) }),
						x: t.x - t.r,
						y: t.y - t.r,
						width: t.r * 2,
						height: t.r * 2,
						stroke: H.value.style.chart.circles.stroke,
						"vector-effect": "non-scaling-stroke",
						"stroke-width": H.value.style.chart.circles.strokeWidth * (sn.value || 1) / 100,
						fill: `url(#pattern_${L.value}_${t.id})`,
						rx: t.r,
						style: { pointerEvents: "none" }
					}, null, 10, et)) : x("", !0)
				], 64))), 128)),
				(k(!0), S(v, null, Ae(J.value, (e, t) => (k(), S(v, { key: e.id }, [e.hasChildren ? x("", !0) : (k(), S("rect", {
					key: 0,
					class: E({ "vue-data-ui-transition": N(U) }),
					x: e.x - e.r,
					y: e.y - e.r,
					width: e.r * 2,
					height: e.r * 2,
					stroke: "none",
					fill: B.value && B.value.id === e.id ? H.value.style.chart.circles.gradient.show ? `url(#${e.id})` : e.color : "transparent",
					rx: e.r,
					style: O({
						filter: B.value && B.value.id === e.id ? `drop-shadow(0px 0px 6px ${H.value.style.chart.circles.selectedShadowColor})` : "none",
						opacity: +!!B.value,
						pointerEvents: "none"
					})
				}, null, 14, tt))], 64))), 128)),
				(k(!0), S(v, null, Ae(J.value, (t, n) => (k(), S(v, { key: `cl_${t.id}` }, [e.$slots["data-label"] && en(t) ? j(e.$slots, "data-label", Ee({ ref_for: !0 }, {
					...t,
					createTSpans: N(l),
					fontSize: {
						name: t.r / 3 * H.value.style.chart.circles.labels.name.fontSizeRatio,
						value: hn(t) * H.value.style.chart.circles.labels.value.fontSizeRatio
					},
					color: H.value.style.chart.circles.labels.name.color ? H.value.style.chart.circles.labels.name.color : N(h)(t.color)
				}), void 0, !0, 0) : (k(), S(v, { key: 1 }, [H.value.style.chart.circles.labels.name.show && t.name && en(t) ? (k(), S("text", {
					key: 0,
					style: { pointerEvents: "none" },
					class: E({ "vue-data-ui-transition": N(U) }),
					transform: `translate(${t.x}, ${t.y + cn(t.r, H.value.style.chart.circles.labels.name.offsetY) - t.r / 10})`,
					"font-size": t.r / 3 * H.value.style.chart.circles.labels.name.fontSizeRatio,
					fill: H.value.style.chart.circles.labels.name.color === "auto" ? N(h)(t.color) : H.value.style.chart.circles.labels.name.color,
					"font-weight": H.value.style.chart.circles.labels.name.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, M(t.name), 11, nt)) : x("", !0), H.value.style.chart.circles.labels.value.show && en(t) ? (k(), S("text", {
					key: 1,
					class: E({ "vue-data-ui-transition": N(U) }),
					style: {
						pointerEvents: "none",
						transition: "opacity 0.2s ease-in-out"
					},
					transform: `translate(${t.x}, ${t.y + cn(t.r, H.value.style.chart.circles.labels.value.offsetY) + t.r / 2.5})`,
					"font-size": hn(t) * H.value.style.chart.circles.labels.value.fontSizeRatio,
					fill: H.value.style.chart.circles.labels.value.color === "auto" ? N(h)(t.color) : H.value.style.chart.circles.labels.value.color,
					"font-weight": H.value.style.chart.circles.labels.value.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, M(mn(t)), 11, rt)) : x("", !0)], 64)), j(e.$slots, "circle", Ee({ ref_for: !0 }, {
					...t,
					showLabel: en(t),
					isSelected: B.value?.id === t.id,
					isDescendantOfSelected: $t(t, B.value),
					uid: `${n}_${L.value}`
				}), void 0, !0)], 64))), 128)),
				Mn.value.length && H.value.style.chart.parentTooltips.show ? (k(), S("g", it, [(k(!0), S(v, null, Ae(Mn.value, (t) => (k(), S("g", { key: `tltp_${t.id}` }, [
					C("path", {
						class: E({ "vue-data-ui-transition": N(U) }),
						stroke: H.value.style.chart.parentTooltips.link.useSerieColor ? t.color : H.value.style.chart.parentTooltips.link.stroke,
						"vector-effect": "non-scaling-stroke",
						"stroke-width": H.value.style.chart.parentTooltips.link.strokeWidth,
						"stroke-linecap": "round",
						"stroke-dasharray": H.value.style.chart.parentTooltips.link.strokeDasharray,
						opacity: H.value.style.chart.parentTooltips.link.opacity,
						d: `M${t.anchorX}, ${t.anchorY} ${t.lineX}, ${t.lineY}`
					}, null, 10, at),
					C("rect", {
						class: E({ "vue-data-ui-transition": N(U) }),
						x: t.x,
						y: t.y,
						width: t.width,
						height: t.height,
						rx: Math.max(3 * t.scale, t.fontSize / 2.5) * H.value.style.chart.parentTooltips.borderRadiusRatio,
						ry: Math.max(3 * t.scale, t.fontSize / 2.5) * H.value.style.chart.parentTooltips.borderRadiusRatio,
						fill: H.value.style.chart.parentTooltips.backgroundColor,
						stroke: H.value.style.chart.parentTooltips.useSerieColor ? t.color : H.value.style.chart.parentTooltips.stroke,
						"vector-effect": "non-scaling-stroke",
						"stroke-width": H.value.style.chart.parentTooltips.strokeWidth,
						filter: H.value.style.chart.parentTooltips.filter
					}, null, 10, ot),
					j(e.$slots, "parent-tooltip", Ee({ ref_for: !0 }, { ...t }), () => [C("rect", {
						class: E({ "vue-data-ui-transition": N(U) }),
						fill: t.color,
						x: t.x + t.paddingX * 1.3 - t.fontSize * .35,
						y: t.y + t.paddingY + t.lineHeight / 2 - t.fontSize * .35,
						rx: t.fontSize * .35,
						width: t.fontSize * .7,
						height: t.fontSize * .7
					}, null, 10, st), C("text", {
						class: E({ "vue-data-ui-transition": N(U) }),
						transform: `translate(${t.x + t.paddingX + t.fontSize}, ${t.y + t.paddingY + t.fontSize})`,
						"font-size": t.fontSize,
						fill: H.value.style.chart.parentTooltips.color,
						"font-family": H.value.style.fontFamily,
						"text-anchor": "start"
					}, [(k(!0), S(v, null, Ae(t.lines, (e, n) => (k(), S("tspan", {
						key: `${t.id}_${n}`,
						x: 0,
						dy: n === 0 ? 0 : t.lineHeight
					}, M(e), 9, lt))), 128))], 10, ct)], !0)
				]))), 128))])) : x("", !0),
				j(e.$slots, "svg", { svg: {
					drawingArea: { ...N($) },
					width: q.value.w,
					height: q.value.h,
					isPrintingImg: N(Ft) || N(It) || N(Jn),
					isPrintingSvg: N(Yn)
				} }, void 0, !0)
			], 46, Xe)), e.$slots.hint ? (k(), S("div", ut, [j(e.$slots, "hint", D(T({
				hint: H.value.a11y.translations.keyboardNavigation,
				isVisible: Dt.value
			})), void 0, !0)])) : x("", !0)], 2),
			e.$slots.watermark ? (k(), S("div", dt, [j(e.$slots, "watermark", D(T({ isPrinting: N(Ft) || N(It) || N(Jn) || N(Yn) })), void 0, !0)])) : x("", !0),
			N(xn) ? (k(), S("div", ft, [j(e.$slots, "reset-action", { reset: N(bn) }, () => [C("button", {
				"data-cy-reset": "",
				tabindex: "0",
				role: "button",
				class: "vue-data-ui-refresh-button",
				style: O({
					background: H.value.style.chart.backgroundColor,
					cursor: W.value ? "pointer" : "default"
				}),
				onClick: t[0] ||= (e) => N(bn)(!0)
			}, [Te(N(F), {
				name: "refresh",
				stroke: H.value.style.chart.color
			}, null, 8, ["stroke"])], 4)], !0)])) : x("", !0),
			e.$slots.source ? (k(), S("div", {
				key: 7,
				ref_key: "source",
				ref: xt,
				dir: "auto"
			}, [j(e.$slots, "source", {}, void 0, !0)], 512)) : x("", !0),
			Te(N(Re), {
				teleportTo: H.value.style.chart.tooltip.teleportTo,
				show: K.value.showTooltip && Ct.value,
				backgroundColor: H.value.style.chart.tooltip.backgroundColor,
				color: H.value.style.chart.tooltip.color,
				fontSize: H.value.style.chart.tooltip.fontSize,
				borderRadius: H.value.style.chart.tooltip.borderRadius,
				borderColor: H.value.style.chart.tooltip.borderColor,
				borderWidth: H.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: H.value.style.chart.tooltip.backgroundOpacity,
				position: H.value.style.chart.tooltip.position,
				offsetX: H.value.style.chart.tooltip.offsetX,
				offsetY: H.value.style.chart.tooltip.offsetY,
				parent: R.value,
				content: wt.value,
				isCustom: fn.value,
				isFullscreen: Q.value,
				smooth: H.value.style.chart.tooltip.smooth,
				backdropFilter: H.value.style.chart.tooltip.backdropFilter,
				smoothForce: H.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: H.value.style.chart.tooltip.smoothSnapThrehsold,
				isA11yMode: Et.value === "keyboard",
				a11yPosition: Tt.value
			}, {
				"tooltip-before": P(() => [j(e.$slots, "tooltip-before", D(T({ ...dn.value })), void 0, !0)]),
				tooltip: P(() => [j(e.$slots, "tooltip", D(T({ ...dn.value })), void 0, !0)]),
				"tooltip-after": P(() => [j(e.$slots, "tooltip-after", D(T({ ...dn.value })), void 0, !0)]),
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
			ht.value && H.value.userOptions.buttons.table ? (k(), b(je(Wn.value.component), Ee({ key: 8 }, Wn.value.props, {
				ref_key: "tableUnit",
				ref: z,
				onClose: Gn
			}), Ce({
				content: P(() => [(k(), b(N(Be), {
					key: `table_${yt.value}`,
					colNames: Rn.value.colNames,
					head: Rn.value.head,
					body: Rn.value.body,
					config: Rn.value.config,
					title: H.value.table.useDialog ? "" : Wn.value.title,
					withCloseButton: !H.value.table.useDialog,
					isCursorPointer: W.value,
					onClose: Gn
				}, {
					th: P(({ th: e }) => [C("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, pt)]),
					td: P(({ td: e }) => [we(M(e.name || e), 1)]),
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
			}, [H.value.table.useDialog ? {
				name: "title",
				fn: P(() => [we(M(Wn.value.title), 1)]),
				key: "0"
			} : void 0, H.value.table.useDialog ? {
				name: "actions",
				fn: P(() => [C("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Ln(H.value.userOptions.callbacks.csv),
					style: O({ cursor: W.value ? "pointer" : "default" })
				}, [Te(N(F), {
					name: "fileCsv",
					stroke: Wn.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : x("", !0),
			j(e.$slots, "skeleton", {}, () => [N(kt) ? (k(), b(se, { key: 0 })) : x("", !0)], !0)
		], 46, Je));
	}
}, [["__scopeId", "data-v-66dd561d"]]);
//#endregion
export { qe as n, mt as t };
