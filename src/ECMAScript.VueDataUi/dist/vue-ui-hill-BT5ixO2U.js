import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Kt as r, Pt as i, X as a, a as o, i as s, q as c, r as l, x as u } from "./lib-Bttd6u5E.js";
import { n as d, t as f } from "./useHints-Dq_w2E8B.js";
import { t as p } from "./useConfig-DlNpz6P8.js";
import { t as m } from "./usePrinter-DN5bYhTG.js";
import { t as h } from "./useNestedProp-vPNvh7rV.js";
import { t as ee } from "./useThemeCheck-C43Tcqmk.js";
import { t as te } from "./useChartExport-DNiwdPmb.js";
import { t as ne } from "./useTransitions-g_zBREk2.js";
import { t as re } from "./img-Bnokohej.js";
import { n as ie } from "./Title-BE3qg9xl.js";
import { t as ae } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as oe } from "./useUserOptionState-DK-_1ddE.js";
import { t as se } from "./vue_ui_hill-dnltu0L-.js";
import { Fragment as g, computed as _, createBlock as ce, createCommentVNode as v, createElementBlock as y, createElementVNode as b, createSlots as le, createVNode as ue, defineAsyncComponent as de, guardReactiveProps as x, nextTick as fe, normalizeClass as pe, normalizeProps as S, normalizeStyle as C, onBeforeUnmount as me, onMounted as he, openBlock as w, ref as T, renderList as ge, renderSlot as E, shallowRef as _e, toDisplayString as D, unref as O, useTemplateRef as ve, watch as ye, withCtx as k, withKeys as be, withModifiers as A } from "vue";
//#region src/components/vue-ui-hill.vue
var xe = /* @__PURE__ */ e({ default: () => ht }), Se = ["id", "data-editing"], Ce = {
	key: 0,
	ref: "chartTitle",
	style: "width:100%;background:transparent;padding-bottom:24px"
}, we = {
	key: 3,
	class: "vue-ui-hill__toolbar",
	"data-dom-to-png-ignore": ""
}, Te = ["viewBox"], Ee = ["width", "height"], De = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Oe = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], ke = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Ae = {
	key: 0,
	"pointer-events": "none"
}, je = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Me = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Ne = [
	"transform",
	"font-size",
	"fill",
	"stroke"
], Pe = [
	"transform",
	"data-datapoint-index",
	"data-datapoint-id",
	"tabindex",
	"aria-valuenow",
	"aria-valuetext",
	"aria-label",
	"onFocus",
	"onBlur",
	"onKeydown",
	"onPointerenter",
	"onPointerleave",
	"onClick"
], Fe = ["r", "onPointerdown"], Ie = [
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Le = ["stroke", "stroke-width"], Re = ["d"], ze = [
	"x",
	"y",
	"text-anchor",
	"fill",
	"font-size",
	"font-weight",
	"stroke",
	"stroke-width"
], Be = [
	"transform",
	"tabindex",
	"aria-label",
	"aria-expanded",
	"onClick",
	"onFocus",
	"onKeydown"
], Ve = {
	key: 0,
	class: "vue-ui-hill__stack-overflow-collapse-ghosts",
	"pointer-events": "none",
	"aria-hidden": "true"
}, He = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Ue = ["from", "dur"], We = ["from", "dur"], Ge = [
	"from",
	"to",
	"dur"
], Ke = ["dur"], qe = ["r"], Je = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], Ye = ["stroke", "stroke-width"], Xe = ["d"], Ze = [
	"y",
	"fill",
	"font-size",
	"font-weight",
	"stroke"
], Qe = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight",
	"letter-spacing"
], $e = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight",
	"letter-spacing"
], et = [
	"x",
	"y",
	"rx",
	"width",
	"height",
	"fill"
], tt = ["id"], nt = [
	"x",
	"y",
	"width",
	"height",
	"rx",
	"ry"
], rt = ["clip-path"], it = [
	"width",
	"x",
	"y",
	"fill",
	"height",
	"onPointerenter",
	"onPointerleave"
], at = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], ot = [
	"x",
	"y",
	"width",
	"height",
	"rx",
	"ry",
	"stroke",
	"stroke-width"
], st = [
	"x",
	"y",
	"fill",
	"font-size"
], ct = ["aria-label", "onKeydown"], lt = {
	key: 0,
	class: "vue-ui-hill__stack-overflow-menu-title"
}, ut = [
	"disabled",
	"aria-label",
	"onClick"
], dt = { class: "vue-ui-hill__stack-overflow-menu-label" }, ft = { class: "vue-ui-hill__stack-overflow-menu-value" }, pt = {
	key: 6,
	class: "vue-data-ui-watermark"
}, mt = {
	key: 7,
	class: "vue-ui-hill__loading",
	"data-dom-to-png-ignore": ""
}, ht = /*#__PURE__*/ ae({
	__name: "vue-ui-hill",
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
		"edit",
		"save",
		"cancel",
		"copyAlt",
		"change",
		"dragStart",
		"dragEnd",
		"datapointEnter",
		"datapointLeave",
		"selectDatapoint"
	],
	setup(e, { expose: ae, emit: xe }) {
		let ht = de(() => import("./HillActions-BD9j_sOU.js")), gt = de(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), _t = de(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), vt = de(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), yt = de(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), { vue_ui_hill: bt } = p(), { isThemeValid: xt, warnInvalidTheme: St } = ee(), Ct = e, wt = T(c()), j = _e(null), M = ve("hillChartRef"), N = ve("svgRef"), Tt = ve("overflowMenuRef"), Et = ve("userOptionsRef"), Dt = T(0), Ot = xe;
		function kt() {
			let e = h({
				userConfig: Ct.config,
				defaultConfig: bt
			}), t = {}, n = e.theme;
			if (!n) t = e;
			else if (!xt.value(e)) St(e), t = e;
			else {
				let i = h({
					userConfig: se[n] || Ct.config,
					defaultConfig: e
				});
				t = {
					...h({
						userConfig: Ct.config,
						defaultConfig: i
					}),
					customPalette: e.customPalette.length ? e.customPalette : r[n] || Kt
				};
			}
			return t;
		}
		let P = T(kt());
		d({
			config: () => P.value,
			dataset: () => Ct.dataset,
			component: "VueUiHill",
			rules: [f.emptyArray, f.noHint]
		});
		let { transitionEnabled: F } = ne({
			config: () => P.value.transitions,
			dataset: () => Ct.dataset
		}), { userOptionsVisible: At, setUserOptionsVisibility: jt, keepUserOptionState: Mt } = oe({ config: P.value });
		function Nt() {
			jt(!0);
		}
		function Pt() {
			jt(!1), Gn.value = null, $.value = null, Wn.value = null;
		}
		let { isPrinting: Ft, isImaging: It, generatePdf: Lt, generateImage: Rt } = m({
			elementId: `hill_${wt.value}`,
			fileName: P.value.style.chart.title.text || "vue-ui-hill",
			options: P.value.userOptions.print
		}), zt = _(() => P.value.userOptions.useCursorPointer), Bt = T(0), Vt = T(!1);
		function Ht(e) {
			Vt.value = e, Bt.value += 1;
		}
		let Ut = T(!1);
		function Wt() {
			Ut.value = !Ut.value;
		}
		let Gt = _(() => P.value.userOptions.position === "left" ? "right" : "left");
		ye(() => Ct.config, () => {
			j.value = null, P.value = kt(), Bt.value += 1, I.value !== P.value.editing && (I.value = P.value.editing && !P.value.readonly);
		}, { deep: !0 });
		let Kt = _(() => {
			let e = P.value.customPalette;
			return Array.isArray(e) && e.length ? e : i;
		});
		function qt(e) {
			let t = Number(e);
			return Number.isFinite(t) ? Math.min(1, Math.max(0, t)) : 0;
		}
		function Jt(e) {
			if (e <= 0) return "todo";
			if (e >= 1) return "done";
			let t = .5, n = Math.min(t, Math.max(0, Number(P.value.interaction.peakTolerance) || 0)), r = t - n, i = t + n;
			return e >= r && e <= i ? "top" : e < t ? "left" : "right";
		}
		function Yt(e, t) {
			let n = e && typeof e == "object" ? e : {}, r = `Item ${t + 1}`, i = String(n.name ?? n.label ?? r), a = qt(n.position ?? n.value ?? 0);
			return {
				...n,
				id: String(n.id ?? `${i}-${t}`),
				name: i,
				label: String(n.label ?? i),
				position: a,
				color: n.color || Kt.value[t % Kt.value.length],
				muted: !!n.muted,
				disabled: !!n.disabled,
				status: Jt(e.position),
				labelSide: [
					"left",
					"right",
					"auto"
				].includes(n.labelSide) ? n.labelSide : "auto",
				__index: t
			};
		}
		let Xt = _(() => Array.isArray(Ct.dataset) ? Ct.dataset.map(Yt) : []);
		function Zt(e) {
			return e.map((e) => ({ ...e }));
		}
		function Qt(e) {
			let { __index: t, ...n } = e;
			return Object.prototype.hasOwnProperty.call(n, "value") && (n.value = n.position), n;
		}
		function $t(e = L.value) {
			return e.map(Qt);
		}
		let en = _(() => P.value.editing && !P.value.readonly), I = T(en.value), L = T(Zt(Xt.value)), R = T(null);
		ye(Xt, (e) => {
			I.value || (j.value = null, L.value = Zt(e));
		}, { deep: !0 });
		let z = _(() => !P.value.readonly);
		ye(z, (e) => {
			e || (R.value = null, j.value = null, I.value = !1, L.value = Zt(Xt.value));
		});
		function B(e, t) {
			Ot(e, t);
			let n = P.value.events?.[e];
			typeof n == "function" && n(t);
		}
		let V = _(() => P.value.style.chart), tn = _(() => V.value.layout), H = _(() => tn.value.hill), U = _(() => tn.value.plots), W = _(() => tn.value.labels.item), G = _(() => tn.value.labels.phases), K = _(() => V.value.toolbar), q = _(() => tn.value.stackbar), J = _(() => {
			let e = U.value.stacking?.overflow || {}, t = Math.max(0, Number(U.value.radius) || 0);
			return {
				show: e.show !== !1,
				marker: {
					radius: Math.max(4, Number(e.marker?.radius) || t),
					fill: e.marker.fill,
					stroke: e.marker?.stroke || U.value.stroke,
					strokeWidth: e.marker.strokeWidth,
					color: e.marker.labelColor,
					fontSize: e.marker.fontSize,
					bold: e.marker?.bold !== !1,
					offsetY: e.marker.labelOffsetY
				},
				hysteresis: t * 1.5,
				transitionDuration: F.value ? e.transitionDuration : 0,
				menu: {
					width: Math.max(160, Number(e.menu?.width) || 220),
					maxHeight: Math.max(96, Number(e.menu?.maxHeight) || 220),
					backgroundColor: e.menu?.backgroundColor || V.value.backgroundColor,
					color: e.menu?.color || V.value.color,
					borderColor: e.menu?.borderColor || U.value.stroke,
					borderRadius: Math.max(0, Number(e.menu?.borderRadius) || 6)
				}
			};
		});
		function nn(e, t, n = 1) {
			let r = Number(e);
			return Number.isFinite(r) ? Math.min(n, Math.max(0, r)) : t;
		}
		let Y = _(() => {
			let e = H.value.geometry || {}, t = Math.max(0, Number(V.value.width) || 0), n = Math.max(0, Number(V.value.height) || 0), r = nn(e.horizontalPaddingRatio, .02, .49), i = nn(e.topPaddingRatio, .12, .49), a = nn(e.bottomPaddingRatio, .2, .49), o = nn(e.curvature, .65), s = t * r, c = t / 2, l = t * (1 - r), u = n * i, d = n * (1 - a), f = c - s, p = l - c, m = s + f * o, h = c + p * (1 - o);
			return {
				startX: s,
				centerX: c,
				endX: l,
				baseY: d,
				peakY: u,
				width: f + p,
				left: [
					{
						x: s,
						y: d
					},
					{
						x: m,
						y: d
					},
					{
						x: m,
						y: u
					},
					{
						x: c,
						y: u
					}
				],
				right: [
					{
						x: c,
						y: u
					},
					{
						x: h,
						y: u
					},
					{
						x: h,
						y: d
					},
					{
						x: l,
						y: d
					}
				]
			};
		}), rn = _(() => {
			let { left: e, right: t } = Y.value;
			return [
				`M${e[0].x} ${e[0].y}`,
				`C${e[1].x} ${e[1].y} ${e[2].x} ${e[2].y} ${e[3].x} ${e[3].y}`,
				`C${t[1].x} ${t[1].y} ${t[2].x} ${t[2].y} ${t[3].x} ${t[3].y}`
			].join(" ");
		});
		function an(e, t, n, r, i) {
			let a = 1 - i;
			return a ** 3 * e + 3 * a ** 2 * i * t + 3 * a * i ** 2 * n + i ** 3 * r;
		}
		function on(e, t, n, r, i) {
			let a = 1 - i;
			return 3 * a ** 2 * (t - e) + 6 * a * i * (n - t) + 3 * i ** 2 * (r - n);
		}
		function sn(e) {
			let t = qt(e), { startX: n, centerX: r, endX: i, left: a, right: o } = Y.value, s = n + (t - 0) / 1 * (i - n), c = s <= r ? a : o, l = 0, u = 1;
			for (let e = 0; e < 28; e += 1) {
				let e = (l + u) / 2;
				an(c[0].x, c[1].x, c[2].x, c[3].x, e) < s ? l = e : u = e;
			}
			let d = (l + u) / 2, f = an(c[0].y, c[1].y, c[2].y, c[3].y, d), p = on(c[0].x, c[1].x, c[2].x, c[3].x, d), m = on(c[0].y, c[1].y, c[2].y, c[3].y, d), h = Math.hypot(p, m) || 1;
			return {
				x: s,
				y: f,
				normalX: m / h,
				normalY: -p / h
			};
		}
		function cn(e) {
			let t = Number(U.value.stacking.overlapThresholdRatio);
			return e * (2 - (Number.isFinite(t) ? Math.min(2, Math.max(0, t)) : .5));
		}
		function ln(e, t) {
			let n = /* @__PURE__ */ new Map(), r = U.value.stacking;
			if (e.forEach((e) => {
				n.set(e.id, {
					position: e.position,
					index: 0,
					size: 1,
					stackId: `stack:${e.id}`
				});
			}), !r.show || e.length < 2 || t <= 0) return n;
			let i = cn(t), a = e.map((e, t) => ({
				datapoint: e,
				index: t,
				point: sn(e.position)
			})).sort((e, t) => e.point.x - t.point.x || e.datapoint.position - t.datapoint.position || e.index - t.index), o = 0;
			for (; o < a.length;) {
				let e = [a[o]], t = o + 1;
				for (; t < a.length;) {
					let n = e[e.length - 1], r = a[t];
					if (Math.hypot(r.point.x - n.point.x, r.point.y - n.point.y) >= i) break;
					e.push(r), t += 1;
				}
				if (e.length > 1) {
					let t = e[0].datapoint.position, r = `stack:${e.map(({ datapoint: e }) => e.id).sort().join("|")}`;
					e.forEach(({ datapoint: i }, a) => {
						n.set(i.id, {
							position: t,
							index: a,
							size: e.length,
							stackId: r
						});
					});
				}
				o = t;
			}
			return n;
		}
		function un(e) {
			let t = e + Math.max(0, Number(U.value.strokeWidth) || 0) / 2, n = U.value.stacking.gap ?? e / 2;
			return t * 2 + n;
		}
		let dn = T(null), X = T(null), Z = T(null), fn = T(null), pn = T(null), mn = /* @__PURE__ */ new Set(), hn = null, gn = null, Q = null;
		function _n(e) {
			let t = Math.max(0, Number(U.value.strokeWidth) || 0), n = U.value.shadow.show ? Math.max(0, Number(U.value.shadow.blur) || 0) + Math.max(0, -(Number(U.value.shadow.offsetY) || 0)) : 0;
			return e + t / 2 + n;
		}
		function vn(e, t, n) {
			if (t <= 1) return e - _n(n);
			let r = un(n);
			return e - (t - 1) * r - _n(n);
		}
		function yn(e, t, n, r) {
			if (n <= 1) return mn.delete(e), !1;
			let i = vn(t, n, r), a = mn.has(e), o = J.value.hysteresis, s = a ? i < o : i < 0;
			return s ? mn.add(e) : mn.delete(e), s;
		}
		let bn = _(() => {
			let e = Math.max(0, Number(U.value.radius) || 0), t = un(e), n = ln(L.value, e), r = /* @__PURE__ */ new Map(), i = /* @__PURE__ */ new Map(), a = [], o = /* @__PURE__ */ new Set(), s = R.value?.id ?? null;
			J.value.show || mn.clear(), L.value.forEach((e) => {
				let t = n.get(e.id) || {
					position: e.position,
					index: 0,
					size: 1,
					stackId: `stack:${e.id}`
				};
				r.has(t.stackId) || r.set(t.stackId, []), r.get(t.stackId).push({
					datapoint: e,
					stack: t
				});
			});
			let c = new Set(r.keys());
			return mn.forEach((e) => {
				c.has(e) || mn.delete(e);
			}), r.forEach((n, r) => {
				let c = [...n].sort((e, t) => e.stack.index - t.stack.index || e.datapoint.__index - t.datapoint.__index), l = c.reduce((e, t) => e + (Number(t.datapoint.position) || 0), 0) / c.length, u = sn(l);
				if (!(J.value.show && yn(r, u.y, c.length, e))) {
					c.forEach((e) => {
						i.set(e.datapoint.id, {
							hidden: !1,
							displayIndex: e.stack.index,
							stackId: r,
							stackSize: c.length,
							promotedFromOverflow: !1,
							renderPosition: e.stack.position
						});
					});
					return;
				}
				c.forEach(({ datapoint: e }) => {
					o.add(e.id);
				});
				let d = s ? c.find(({ datapoint: e }) => e.id === s) : null, f = c.find(({ datapoint: e }) => e.id === dn.value), p = d || f || null, m = p ? c.filter(({ datapoint: e }) => e.id !== p.datapoint.id) : c, h = X.value, ee = !!(f && h?.datapointId === f.datapoint.id && !h.moved), te = m.length ? ee ? l : m.reduce((e, t) => e + (Number(t.datapoint.position) || 0), 0) / m.length : l, ne = sn(te);
				c.forEach((e) => {
					let t = p?.datapoint.id === e.datapoint.id, n = t && f?.datapoint.id === e.datapoint.id;
					i.set(e.datapoint.id, {
						hidden: !t,
						displayIndex: +!!t,
						stackId: r,
						stackSize: c.length,
						promotedFromOverflow: n,
						renderPosition: te
					});
				}), m.length && a.push({
					stackId: r,
					anchorPosition: te,
					x: ne.x,
					y: ne.y,
					hiddenCount: m.length,
					hiddenDatapoints: m.map(({ datapoint: e }) => e),
					memberIds: c.map(({ datapoint: e }) => e.id),
					collapseDatapoints: m.map(({ datapoint: e, stack: n }) => {
						let r = sn(n.position);
						return {
							id: e.id,
							color: e.color,
							relativeX: r.x - ne.x,
							relativeY: r.y - n.index * t - ne.y
						};
					})
				});
			}), {
				layoutMap: n,
				displayMap: i,
				markers: a,
				overflowMemberIds: o
			};
		});
		function xn(e) {
			let t = Cn.value.filter((e) => !e.isStackOverflowHidden), n = t.find((t) => t.id === e), r = new Map(t.filter((t) => t.id !== e).map((e) => [e.id, {
				x: e.x,
				y: e.y
			}]));
			if (!n || !U.value.stacking.show) return r;
			let i = t.filter((e) => Math.abs(e.x - n.x) < .001).sort((e, t) => t.y - e.y || e.datasetIndex - t.datasetIndex);
			if (i.length < 2) return r;
			let a = un(Math.max(0, Number(U.value.radius) || 0)), o = Math.max(...i.map((e) => e.y));
			return i.filter((t) => t.id !== e).forEach((e, t) => {
				r.set(e.id, {
					x: n.x,
					y: o - t * a
				});
			}), r;
		}
		function Sn() {
			return new Map(Cn.value.filter((e) => !e.isStackOverflowHidden).map((e) => [e.id, {
				x: e.x,
				y: e.y
			}]));
		}
		let Cn = _(() => {
			let e = Math.max(0, Number(U.value.radius) || 0), { layoutMap: t, displayMap: n, markers: r } = bn.value, i = un(e), a = R.value, o = a?.stationaryLayout, s = o instanceof Map ? o : j.value instanceof Map ? j.value : null, c = null;
			if (a && o instanceof Map && U.value.stacking.show && e > 0) {
				let t = L.value.find((e) => e.id === a.id);
				if (t) {
					let n = r.find((e) => e.memberIds.includes(t.id));
					if (n) c = {
						x: n.x,
						y: n.y - i
					};
					else {
						let n = sn(t.position), r = cn(e), s = L.value.filter((e) => e.id !== a.id && o.has(e.id)).map((e) => {
							let t = sn(e.position);
							return {
								datapoint: e,
								distance: Math.hypot(t.x - n.x, t.y - n.y)
							};
						}).filter(({ distance: e }) => e < r).sort((e, t) => e.distance - t.distance)[0];
						if (s) {
							let e = o.get(s.datapoint.id), t = [...o.values()].filter((t) => Math.abs(t.x - e.x) < .001), n = Math.min(e.y, ...t.map((e) => e.y));
							c = {
								x: e.x,
								y: n - i
							};
						} else c = n;
					}
				}
			}
			return L.value.map((e, r) => {
				let o = t.get(e.id) || {
					position: e.position,
					index: 0,
					size: 1,
					stackId: `stack:${e.id}`
				}, l = n.get(e.id) || {
					hidden: !1,
					displayIndex: o.index,
					stackId: o.stackId,
					stackSize: o.size,
					renderPosition: o.position
				}, d = sn(l.renderPosition ?? o.position), f = e.id !== a?.id && !l.promotedFromOverflow && s instanceof Map ? s.get(e.id) : null, p = e.id === a?.id && c ? c : f || {
					x: d.x,
					y: d.y - l.displayIndex * i
				}, m = e.labelSide === "auto" ? null : e.labelSide, h = u(W.value.autoSideThreshold, .5, 1), ee = m || (e.position > h ? "left" : "right"), te = Math.abs(Number(W.value.offsetX) || 0) + U.value.radius + W.value.fontSize / 2;
				return {
					...e,
					datasetIndex: r,
					x: p.x,
					y: p.y,
					stackId: l.stackId,
					stackSize: l.stackSize,
					stackDisplayIndex: l.displayIndex,
					isStackOverflowHidden: l.hidden,
					isPromotedFromStackOverflow: l.promotedFromOverflow,
					labelSide: ee,
					labelX: ee === "left" ? -te : te,
					textAnchor: ee === "left" ? "end" : "start"
				};
			});
		});
		function wn(e) {
			return L.value.find((t) => t.id === e);
		}
		function Tn(e, t) {
			let n = qt(t), r;
			L.value = L.value.map((t) => t.id === e ? (r = {
				...t,
				position: n
			}, r) : t), r && (dr(r.id, r.position), B("change", {
				datapoint: Qt(r),
				dataset: $t()
			}));
		}
		function En(e) {
			let t = N.value;
			if (!t) return null;
			let n = t.getScreenCTM();
			if (!n) return null;
			let r = t.createSVGPoint();
			r.x = e, r.y = 0;
			let i = r.matrixTransform(n.inverse()), { startX: a, endX: o } = Y.value;
			return qt(0 + (i.x - a) / (o - a) * 1);
		}
		function Dn() {
			z.value && (Z.value = null, dn.value = null, X.value = null, pn.value = null, ir(), j.value = null, L.value = Zt(Xt.value), I.value = !0, B("edit", $t()));
		}
		function On() {
			let e = $t();
			Z.value = null, dn.value = null, X.value = null, pn.value = null, ir(), R.value = null, I.value = !1, B("save", e);
		}
		function kn() {
			Z.value = null, dn.value = null, X.value = null, pn.value = null, ir(), R.value = null, j.value = null, L.value = Zt(Xt.value), I.value = !1, B("cancel", $t());
		}
		function An(e, t) {
			if (!z.value || !I.value || t.disabled) return;
			Z.value = null;
			let n = e.currentTarget.closest(".vue-ui-hill__datapoint");
			n && typeof n.focus == "function" && (n.focus(), $.value = t.id), e.preventDefault();
			let r = xn(t.id), i = X.value?.datapointId === t.id && !X.value?.moved;
			if (R.value = {
				id: t.id,
				pointerId: e.pointerId,
				stationaryLayout: r,
				startClientX: e.clientX,
				hasMoved: !1,
				deferPositionUpdate: i
			}, N.value?.setPointerCapture(e.pointerId), !i) {
				let n = En(e.clientX);
				n !== null && Tn(t.id, n);
			}
			let a = wn(t.id);
			a && B("dragStart", Qt(a));
		}
		function jn(e) {
			if (!R.value || R.value.pointerId !== e.pointerId) return;
			e.preventDefault();
			let t = Math.abs(e.clientX - R.value.startClientX);
			if (R.value.deferPositionUpdate && !R.value.hasMoved && t < 3) return;
			R.value.hasMoved || (R.value = {
				...R.value,
				hasMoved: !0
			});
			let n = En(e.clientX);
			n !== null && Tn(R.value.id, n);
		}
		function Mn(e) {
			let t = R.value;
			if (!t || t.pointerId !== e.pointerId) return;
			N.value?.hasPointerCapture(e.pointerId) && N.value.releasePointerCapture(e.pointerId);
			let n = e.type === "pointerup" && !t.hasMoved, r = wn(t.id);
			j.value = Sn(), R.value = null, r && (n && B("selectDatapoint", Fn(r)), B("dragEnd", Qt(r)));
		}
		function Nn(e, t) {
			if (!z.value || !I.value || t.disabled) return;
			let n = Math.max(0, Number(P.value.interaction.keyboardStep) || 0), r = null;
			switch (e.key) {
				case "ArrowLeft":
				case "ArrowDown":
					r = t.position - n;
					break;
				case "ArrowRight":
				case "ArrowUp":
					r = t.position + n;
					break;
				case "Home":
					r = 0;
					break;
				case "End":
					r = 1;
					break;
				default: return;
			}
			e.preventDefault();
			let i = e.currentTarget;
			$.value = t.id, R.value = {
				id: t.id,
				pointerId: null,
				stationaryLayout: xn(t.id)
			}, Tn(t.id, r), j.value = Sn(), R.value = null, fe(() => {
				i?.isConnected && i.focus({ preventScroll: !0 });
			});
		}
		function Pn(e) {
			let t = .5, n = Math.max(0, Number(P.value.interaction.peakTolerance) || 0);
			return Math.abs(e - t) <= n ? P.value.a11y.translations.topOfHill : e < t ? G.value.left.text : G.value.right.text;
		}
		function Fn(e) {
			return {
				datapoint: Qt(e),
				index: e.__index
			};
		}
		function In(e) {
			$.value = null, Gn.value = e.id, ![0, 1].includes(e.position) && $.value !== e.id && Jn(e), B("datapointEnter", Fn(e));
		}
		function Ln(e) {
			Gn.value === e.id && (Gn.value = null), Wn.value === e.id && $.value !== e.id && R.value?.id !== e.id && (Wn.value = null), B("datapointLeave", Fn(e));
		}
		function Rn(e) {
			I.value && z.value || B("selectDatapoint", Fn(e));
		}
		let zn = _(() => {
			let e = V.value;
			return {
				width: "100%",
				maxWidth: "100%",
				color: e.color,
				backgroundColor: e.backgroundColor,
				fontFamily: P.value.style.fontFamily,
				"--vue-ui-hill-button-hover-border": K.value.buttons.hoverBorderColor,
				"--vue-ui-hill-button-active-offset": `${K.value.buttons.activeTranslateY}px`
			};
		}), Bn = _(() => ({
			color: K.value.status.color,
			fontSize: `${K.value.status.fontSize}px`,
			fontWeight: K.value.status.bold ? "bold" : "normal",
			lineHeight: K.value.status.lineHeight
		}));
		_(() => ({
			color: V.value.title.color,
			fontSize: `${V.value.title.fontSize}px`,
			fontWeight: V.value.title.bold ? "bold" : "normal",
			textAlign: V.value.title.textAlign
		})), _(() => ({
			color: V.value.title.subtitle.color,
			fontSize: `${V.value.title.subtitle.fontSize}px`,
			fontWeight: V.value.title.subtitle.bold ? "bold" : "normal"
		}));
		let Vn = _(() => {
			let e = Math.max(0, Number(V.value.height) || 0);
			return Math.min(e, Y.value.baseY + e) + G.value.offsetY;
		});
		function Hn(e) {
			return G.value[e].text;
		}
		function Un(e) {
			return e.position === 0 || e.position === 1;
		}
		let Wn = T(null), Gn = T(null), $ = T(null);
		function Kn(e) {
			$.value = e.id;
		}
		function qn(e) {
			$.value === e.id && ($.value = null), Wn.value === e.id && Gn.value !== e.id && R.value?.id !== e.id && (Wn.value = null);
		}
		function Jn(e) {
			e?.id && (Wn.value = e.id);
		}
		let Yn = _(() => {
			let e = [], t = [], n = null;
			return Cn.value.forEach((r) => {
				if (!r.isStackOverflowHidden) {
					if (r.id === Wn.value) {
						n = r;
						return;
					}
					Un(r) ? e.push(r) : t.push(r);
				}
			}), [
				...e,
				...t,
				...n ? [n] : []
			];
		}), Xn = _(() => bn.value.markers), Zn = _(() => Xn.value.find((e) => e.stackId === Z.value) || null), Qn = _(() => {
			let e = Math.max(160, Number(J.value.menu.width) || 220), t = Math.max(80, Number(J.value.menu.maxHeight) || 220);
			return {
				visibility: fn.value ? "visible" : "hidden",
				width: "max-content",
				minWidth: `${e}px`,
				maxWidth: "calc(100% - 16px)",
				maxHeight: `${t}px`,
				backgroundColor: J.value.menu.backgroundColor,
				color: J.value.menu.color,
				borderColor: J.value.menu.borderColor,
				borderRadius: `${J.value.menu.borderRadius}px`
			};
		});
		function $n(e, t) {
			let n = N.value, r = M.value;
			if (!n || !r) return null;
			let i = n.getScreenCTM();
			if (!i) return null;
			let a = n.createSVGPoint();
			a.x = e, a.y = t;
			let o = a.matrixTransform(i), s = r.getBoundingClientRect();
			return {
				x: o.x - s.left,
				y: o.y - s.top
			};
		}
		function er() {
			let e = Zn.value, t = Tt.value, n = M.value;
			if (!e || !t || !n) {
				fn.value = null;
				return;
			}
			let r = $n(e.x, e.y), i = $n(e.x, e.y + J.value.marker.radius);
			if (!r || !i) {
				fn.value = null;
				return;
			}
			let a = n.getBoundingClientRect(), o = t.offsetWidth, s = t.offsetHeight, c = Math.abs(i.y - r.y), l = Math.max(0, a.width - 16), u = Math.min(o, l), d = r.x;
			if (u >= l) d = a.width / 2;
			else {
				let e = u / 2;
				d = Math.min(a.width - 8 - e, Math.max(8 + e, r.x));
			}
			let f = r.y + c + 8, p = r.y - c - 8 - s, m = f + s <= a.height - 8, h = p >= 8, ee = f;
			!m && h ? ee = p : m || (ee = Math.min(Math.max(8, f), Math.max(8, a.height - s - 8))), fn.value = {
				left: `${d}px`,
				top: `${ee}px`,
				transform: "translateX(-50%)",
				visibility: "visible"
			};
		}
		function tr() {
			typeof window > "u" || (Q !== null && window.cancelAnimationFrame(Q), Q = window.requestAnimationFrame(() => {
				Q = null, er();
			}));
		}
		ye(Xn, (e) => {
			Z.value && !e.some((e) => e.stackId === Z.value) && rr();
		}), ye(() => {
			let e = Zn.value;
			return e ? [
				e.stackId,
				e.x,
				e.y,
				e.hiddenDatapoints.length,
				e.hiddenDatapoints.map((e) => `${e.id}:${e.label}:${e.position}`).join("|"),
				J.value.marker.radius,
				J.value.menu.width,
				J.value.menu.maxHeight,
				J.value.menu.backgroundColor,
				J.value.menu.color,
				J.value.menu.borderColor,
				J.value.menu.borderRadius,
				Vt.value
			] : null;
		}, (e) => {
			if (!e) {
				fn.value = null;
				return;
			}
			fe(tr);
		}, { flush: "post" }), ye(Tt, (e, t) => {
			t && gn && gn.unobserve(t), e && gn && (gn.observe(e), fe(tr));
		}, { flush: "post" }), ye(() => {
			let e = dn.value;
			return !e || bn.value.overflowMemberIds.has(e);
		}, (e) => {
			e || (dn.value = null, X.value = null);
		});
		function nr(e) {
			if (e) {
				if (Z.value === e.stackId) {
					tr();
					return;
				}
				fn.value = null, Z.value = e.stackId, fe(tr);
			}
		}
		function rr() {
			Z.value = null, fn.value = null, typeof window < "u" && Q !== null && (window.cancelAnimationFrame(Q), Q = null);
		}
		function ir() {
			hn !== null && (clearTimeout(hn), hn = null);
		}
		function ar() {
			let e = X.value;
			!e || e.moved || (ir(), pn.value = e.stackId, dn.value = null, X.value = null, rr(), hn = setTimeout(() => {
				pn.value = null, hn = null;
			}, J.value.transitionDuration + 80));
		}
		function or(e) {
			if (!(e instanceof Element)) return {
				isMenu: !1,
				isMarker: !1,
				isSelectedDatapoint: !1
			};
			let t = X.value, n = e.closest("[data-datapoint-id]");
			return {
				isMenu: !!e.closest("[data-stack-overflow-menu]"),
				isMarker: !!e.closest("[data-stack-overflow-marker]"),
				isSelectedDatapoint: !!(t && n?.getAttribute("data-datapoint-id") === t.datapointId)
			};
		}
		function sr(e) {
			let t = or(e.target);
			Z.value && !t.isMenu && !t.isMarker && rr(), X.value && !X.value.moved && !t.isMenu && !t.isMarker && !t.isSelectedDatapoint && ar();
		}
		function cr(e) {
			let t = Cn.value.find((t) => t.id === e);
			if (!t || t.isStackOverflowHidden) return;
			let n = N.value?.querySelector(`[data-datapoint-index="${t.__index}"]`);
			n && typeof n.focus == "function" && n.focus({ preventScroll: !0 });
		}
		function lr(e) {
			if (!e || e.disabled || P.value.readonly) return;
			let t = Zn.value?.stackId;
			t && (ir(), pn.value = null, dn.value = e.id, X.value = {
				datapointId: e.id,
				stackId: t,
				initialPosition: e.position,
				moved: !1
			}, rr(), fe(() => {
				cr(e.id);
				let t = Cn.value.find((t) => t.id === e.id);
				t && B("selectDatapoint", Fn(t));
			}));
		}
		function ur() {
			let e = Math.max(1, Number(Y.value.width) || 1);
			return Math.max(1e-6, 1 / e * 2);
		}
		function dr(e, t) {
			let n = X.value;
			!n || n.datapointId !== e || n.moved || Math.abs(t - n.initialPosition) >= ur() && (X.value = {
				...n,
				moved: !0
			});
		}
		he(() => {
			document.addEventListener("pointerdown", sr), window.addEventListener("resize", tr, { passive: !0 }), typeof ResizeObserver < "u" && (gn = new ResizeObserver(() => {
				Zn.value && tr();
			}), M.value && gn.observe(M.value), N.value && gn.observe(N.value));
		}), me(() => {
			document.removeEventListener("pointerdown", sr), window.removeEventListener("resize", tr), gn?.disconnect(), gn = null, Q !== null && (window.cancelAnimationFrame(Q), Q = null), ir();
		});
		function fr(e) {
			return R.value ? R.value.id === e.id : Gn.value === e.id || $.value === e.id;
		}
		function pr(e) {
			return s(P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.formatter, e.position, a({
				p: "",
				v: e.position * 100,
				s: "%",
				r: P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.rounding
			}));
		}
		let mr = _(() => P.value.style.chart.backgroundColor), hr = _(() => P.value.style.chart.legend), gr = _(() => P.value.style.chart.title), { isCallbackImaging: _r, isCallbackSvg: vr, generateSvg: yr, onGenerateImage: br } = te({
			svg: N,
			title: gr,
			legend: hr,
			legendItems: null,
			backgroundColor: mr,
			getSvgCallback: () => P.value.userOptions.callbacks.svg,
			generateImage: Rt
		});
		function xr(e = null) {
			fe(() => {
				let r = $t().map((e) => [
					[e.name],
					[e.position],
					[pr(e)],
					[Pn(e.position)]
				]), i = [
					[P.value.style.chart.title.text],
					[P.value.style.chart.title.subtitle.text],
					[
						["Name"],
						["Position"],
						["Percentage"],
						["Phase"]
					]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: P.value.style.chart.title.text || "vue-ui-hill"
				});
			});
		}
		let Sr = _(() => P.value.style.chart.layout.stackbar.show), Cr = _(() => {
			let e = Cn.value.length;
			Cn.value.reduce((e, t) => e + (t?.position ?? 0), 0);
			let t = 0, n = Cn.value.every((e) => e.position === 1);
			return Cn.value.toSorted((e, t) => {
				if (n) return e.__index - t.__index;
				let r = (e?.position ?? 0) - (t?.position ?? 0);
				return r === 0 ? e.__index - t.__index : r;
			}).map((n) => {
				let r = n?.position ?? 0, i = e > 0 ? r / e : 0, a = {
					...n,
					proportion: i,
					proportionStart: t
				};
				return t += i, a;
			});
		}), wr = _(() => Math.min(1, Math.max(0, Cr.value.reduce((e, t) => e + t.proportion, 0)))), Tr = _(() => wr.value * 100), Er = _(() => Y.value.left[0].x + Y.value.width * wr.value), Dr = _(() => V.value.height + 32 + q.value.paddingTop);
		function Or() {
			return s(q.value.label.formatter, Tr.value, a({
				p: "",
				v: Tr.value,
				s: "%",
				r: 0
			}));
		}
		let kr = _(() => `${wt.value}-stackbar-clip`);
		async function Ar() {
			let e = $t();
			if (Ot("copyAlt", {
				config: P.value,
				dataset: e
			}), !P.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(P.value.userOptions.callbacks.altCopy({
				config: P.value,
				dataset: e
			}));
		}
		async function jr({ scale: e = 2 } = {}) {
			if (!M.value) return;
			let { width: t, height: n } = M.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await re({
				domElement: M.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: P.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		function Mr() {
			return $t();
		}
		return ae({
			isEditing: I,
			beginEditing: Dn,
			save: On,
			cancel: kn,
			getData: Mr,
			copyAlt: Ar,
			toggleFullscreen: Ht,
			toggleAnnotator: Wt,
			generateImage: Rt,
			generateSvg: yr,
			generatePdf: Lt,
			generateCsv: xr,
			getImage: jr
		}), (e, t) => (w(), y("div", {
			ref_key: "hillChartRef",
			ref: M,
			class: pe("vue-data-ui-component vue-ui-hill"),
			style: C(zn.value),
			id: `hill_${wt.value}`,
			onMouseenter: Nt,
			onMouseleave: Pt,
			"data-editing": I.value
		}, [
			P.value.style.chart.title.text ? (w(), y("div", Ce, [(w(), ce(ie, {
				key: `title_${Dt.value}`,
				config: {
					title: {
						cy: "hill-div-title",
						...P.value.style.chart.title
					},
					subtitle: {
						cy: "hill-div-subtitle",
						...P.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : v("", !0),
			P.value.userOptions.buttons.annotator && N.value ? (w(), ce(O(vt), {
				key: 1,
				color: P.value.style.chart.color,
				backgroundColor: P.value.style.chart.backgroundColor,
				active: Ut.value,
				svgRef: N.value,
				isCursorPointer: zt.value,
				onClose: Wt
			}, {
				"annotator-action-close": k(() => [E(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": k(({ color: t }) => [E(e.$slots, "annotator-action-color", S(x({ color: t })), void 0, !0)]),
				"annotator-action-draw": k(({ mode: t }) => [E(e.$slots, "annotator-action-draw", S(x({ mode: t })), void 0, !0)]),
				"annotator-action-undo": k(({ disabled: t }) => [E(e.$slots, "annotator-action-undo", S(x({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": k(({ disabled: t }) => [E(e.$slots, "annotator-action-redo", S(x({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": k(({ disabled: t }) => [E(e.$slots, "annotator-action-delete", S(x({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"color",
				"backgroundColor",
				"active",
				"svgRef",
				"isCursorPointer"
			])) : v("", !0),
			K.value.show ? (w(), ce(O(ht), {
				key: 2,
				"is-editing": I.value,
				"is-editable": z.value,
				"is-fullscreen": Vt.value,
				position: Gt.value,
				color: P.value.style.chart.color,
				"background-color": P.value.style.chart.backgroundColor,
				translations: K.value.buttons.translations,
				"is-cursor-pointer": zt.value,
				onUpdate: Dn,
				onCancel: kn,
				onSave: On
			}, {
				"hill-edit": k(() => [E(e.$slots, "hill-edit", {}, void 0, !0)]),
				"hill-cancel": k(() => [E(e.$slots, "hill-cancel", {}, void 0, !0)]),
				"hill-save": k(() => [E(e.$slots, "hill-save", {}, void 0, !0)]),
				_: 3
			}, 8, [
				"is-editing",
				"is-editable",
				"is-fullscreen",
				"position",
				"color",
				"background-color",
				"translations",
				"is-cursor-pointer"
			])) : v("", !0),
			K.value.show ? (w(), y("div", we, [b("p", {
				class: "vue-ui-hill__status",
				style: C(Bn.value),
				"aria-live": "polite"
			}, D(I.value ? K.value.status.editInstruction : K.value.status.lastUpdated), 5)])) : v("", !0),
			P.value.userOptions.show && (O(Mt) || O(At)) ? (w(), ce(O(gt), {
				ref_key: "userOptionsRef",
				ref: Et,
				key: `uo_${Bt.value}`,
				backgroundColor: P.value.style.chart.backgroundColor,
				color: P.value.style.chart.color,
				isPrinting: O(Ft),
				isImaging: O(It),
				uid: wt.value,
				hasTooltip: !1,
				hasPdf: P.value.userOptions.buttons.pdf,
				hasImg: P.value.userOptions.buttons.img,
				hasSvg: P.value.userOptions.buttons.svg,
				hasXls: P.value.userOptions.buttons.csv,
				hasTable: P.value.userOptions.buttons.table,
				hasLabel: P.value.userOptions.buttons.labels,
				hasFullscreen: P.value.userOptions.buttons.fullscreen,
				hasAltCopy: P.value.userOptions.buttons.altCopy,
				chartElement: M.value,
				position: P.value.userOptions.position,
				callbacks: P.value.userOptions.callbacks,
				titles: { ...P.value.userOptions.buttonTitles },
				hasAnnotator: P.value.userOptions.buttons.annotator,
				isAnnotation: Ut.value,
				printScale: P.value.userOptions.print.scale,
				isCursorPointer: zt.value,
				onToggleFullscreen: Ht,
				onGeneratePdf: O(Lt),
				onGenerateCsv: xr,
				onGenerateImage: O(br),
				onGenerateSvg: O(yr),
				onToggleAnnotator: Wt,
				onCopyAlt: Ar,
				style: C({ visibility: O(Mt) ? O(At) ? "visible" : "hidden" : "visible" })
			}, le({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: k(({ isOpen: t, color: n }) => [E(e.$slots, "menuIcon", S(x({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: k(() => [E(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: k(() => [E(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: k(() => [E(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: k(() => [E(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: k(({ toggleFullscreen: t, isFullscreen: n }) => [E(e.$slots, "optionFullscreen", S(x({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: k(({ toggleAnnotator: t, isAnnotator: n }) => [E(e.$slots, "optionAnnotator", S(x({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: k(({ altCopy: t }) => [E(e.$slots, "optionAltCopy", S(x({ altCopy: t })), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: k(() => [E(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: k(() => [E(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "9"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"hasPdf",
				"hasImg",
				"hasSvg",
				"hasXls",
				"hasTable",
				"hasLabel",
				"hasFullscreen",
				"hasAltCopy",
				"chartElement",
				"position",
				"callbacks",
				"titles",
				"hasAnnotator",
				"isAnnotation",
				"printScale",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : v("", !0),
			(w(), y("svg", {
				ref_key: "svgRef",
				ref: N,
				style: C({
					background: "transparent",
					color: P.value.style.chart.color,
					fontFamily: P.value.style.fontFamily
				}),
				class: pe({ "vue-data-ui-no-transition": !O(F) }),
				viewBox: `0 0 ${V.value.width} ${V.value.height + (Sr.value ? 32 + q.value.paddingTop + q.value.paddingBottom + q.value.height : 0)}`,
				role: "group",
				"aria-live": "polite",
				preserveAspectRatio: "xMidYMid meet",
				onPointermove: jn,
				onPointerup: Mn,
				onPointercancel: Mn
			}, [
				ue(O(_t)),
				e.$slots["chart-background"] ? (w(), y("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: Math.max(.1, V.value.width),
					height: Math.max(.1, V.value.height + (Sr.value ? 32 + q.value.paddingTop + q.value.paddingBottom + q.value.height : 0)),
					style: { pointerEvents: "none" }
				}, [E(e.$slots, "chart-background", {}, void 0, !0)], 8, Ee)) : v("", !0),
				H.value.baseline.show ? (w(), y("line", {
					key: 1,
					x1: Y.value.startX,
					x2: Y.value.endX,
					y1: Y.value.baseY,
					y2: Y.value.baseY,
					stroke: H.value.baseline.stroke,
					"stroke-width": H.value.baseline.strokeWidth,
					"stroke-dasharray": H.value.baseline.strokeDasharray,
					"vector-effect": "non-scaling-stroke"
				}, null, 8, De)) : v("", !0),
				H.value.midline.show ? (w(), y("line", {
					key: 2,
					x1: Y.value.centerX,
					x2: Y.value.centerX,
					y1: Y.value.peakY,
					y2: Y.value.baseY,
					stroke: H.value.midline.stroke,
					"stroke-width": H.value.midline.strokeWidth,
					"stroke-dasharray": H.value.midline.strokeDasharray,
					"vector-effect": "non-scaling-stroke"
				}, null, 8, Oe)) : v("", !0),
				b("path", {
					d: rn.value,
					fill: "none",
					stroke: H.value.curve.stroke,
					"stroke-width": H.value.curve.strokeWidth,
					"stroke-dasharray": H.value.curve.strokeDasharray,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					"vector-effect": "non-scaling-stroke"
				}, null, 8, ke),
				P.value.style.chart.layout.plots.dragMarker.positionIndicator.show ? (w(!0), y(g, { key: 3 }, ge(Yn.value, (e) => (w(), y("g", { key: `position-indicator-${e.id}` }, [fr(e) && U.value.dragMarker.show ? (w(), y("g", Ae, [
					b("path", {
						d: `M${e.x},${e.y} ${e.x},${Y.value.baseY}`,
						stroke: P.value.style.chart.layout.plots.dragMarker.positionIndicator.useSerieColor ? e.color : P.value.style.chart.layout.plots.dragMarker.positionIndicator.color,
						"stroke-width": P.value.style.chart.layout.plots.dragMarker.positionIndicator.strokeWidth,
						"stroke-dasharray": P.value.style.chart.layout.plots.dragMarker.positionIndicator.strokeDasharray,
						"stroke-linecap": "round",
						"vector-effect": "non-scaling-stroke",
						class: pe({ "vue-data-ui-transition": O(F) && R.value?.id !== e.id })
					}, null, 10, je),
					P.value.style.chart.layout.plots.dragMarker.positionIndicator.circle.show ? (w(), y("circle", {
						key: 0,
						cx: e.x,
						cy: Y.value.baseY,
						r: P.value.style.chart.layout.plots.dragMarker.positionIndicator.circle.radius,
						fill: P.value.style.chart.layout.plots.dragMarker.positionIndicator.useSerieColor ? e.color : P.value.style.chart.layout.plots.dragMarker.positionIndicator.color,
						stroke: P.value.style.chart.layout.plots.dragMarker.positionIndicator.circle.stroke,
						"stroke-width": P.value.style.chart.layout.plots.dragMarker.positionIndicator.circle.strokeWidth,
						"vector-effect": "non-scaling-stroke",
						"paint-order": "stroke fill",
						class: pe({ "vue-data-ui-transition": O(F) && R.value?.id !== e.id })
					}, null, 10, Me)) : v("", !0),
					P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.show ? (w(), y("text", {
						key: 1,
						"paint-order": "stroke fill",
						"vector-effect": "non-scaling-stroke",
						"text-anchor": "middle",
						transform: `translate(${e.x}, ${Y.value.baseY + P.value.style.chart.layout.plots.radius + P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.offsetY + P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.fontSize})`,
						"font-size": P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.fontSize,
						fill: P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.useSerieColor ? e.color : P.value.style.chart.layout.plots.dragMarker.positionIndicator.value.color,
						stroke: P.value.style.chart.backgroundColor,
						"stroke-width": "1",
						class: pe({ "vue-data-ui-transition": O(F) && R.value?.id !== e.id })
					}, D(pr(e)), 11, Ne)) : v("", !0)
				])) : v("", !0)]))), 128)) : v("", !0),
				(w(!0), y(g, null, ge(Yn.value, (e) => (w(), y("g", {
					key: e.id,
					"data-cy-datapoint": "",
					class: pe({
						"vue-ui-hill__datapoint": !0,
						"vue-data-ui-transition": O(F) && R.value?.id !== e.id,
						"vue-ui-hill__datapoint--promoted": O(F) && e.isPromotedFromStackOverflow
					}),
					style: C({ opacity: [0, 1].includes(e.position) || e?.muted ? U.value.mutedOpacity : e?.disabled ? U.value.disabledOpacity : 1 }),
					transform: `translate(${e.x} ${e.y})`,
					"data-datapoint-index": e.__index,
					"data-datapoint-id": e.id,
					role: "slider",
					tabindex: I.value && z.value && !e.disabled ? 0 : -1,
					"aria-orientation": "horizontal",
					"aria-valuemin": "0",
					"aria-valuemax": "100",
					"aria-valuenow": pr(e),
					"aria-valuetext": `${e.label}: ${Pn(e.position)}, ${pr(e)}`,
					"aria-label": e.label,
					onFocus: (t) => Kn(e),
					onBlur: (t) => qn(e),
					onKeydown: (t) => Nn(t, e),
					onPointerenter: (t) => In(e),
					onPointerleave: (t) => Ln(e),
					onClick: (t) => Rn(e)
				}, [
					b("circle", {
						r: U.value.hitRadius,
						fill: "transparent",
						class: pe({
							"vue-ui-hill__hit-area--active": I.value && z.value && !e.disabled,
							"vue-ui-hill__hit-area--dragging": R.value?.id === e.id
						}),
						onPointerdown: (t) => An(t, e)
					}, null, 42, Fe),
					b("circle", {
						class: "vue-ui-hill-circle",
						r: U.value.radius,
						fill: e.color,
						stroke: U.value.stroke,
						"stroke-width": U.value.strokeWidth,
						style: C({ filter: U.value.shadow.show ? `drop-shadow(${U.value.shadow.offsetX}px ${U.value.shadow.offsetY}px ${U.value.shadow.blur}px ${U.value.shadow.color})` : "none" }),
						"vector-effect": "non-scaling-stroke",
						"pointer-events": "none"
					}, null, 12, Ie),
					I.value && (R.value?.id === e.id && U.value.dragMarker.show || fr(e)) && !e?.disabled ? (w(), y("g", {
						key: 0,
						fill: "none",
						stroke: O(l)(e.color),
						"stroke-width": U.value.dragMarker.strokeWidth,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						"pointer-events": "none",
						"vector-effect": "non-scaling-stroke",
						"aria-hidden": "true"
					}, [b("path", { d: U.value.dragMarker.crossPath }, null, 8, Re)], 8, Le)) : v("", !0),
					W.value.show ? (w(), y("text", {
						key: 1,
						x: e.labelX,
						y: W.value.offsetY,
						"text-anchor": e.textAnchor,
						fill: W.value.useSerieColor ? e.color : W.value.color,
						"font-size": W.value.fontSize,
						"font-weight": W.value.bold ? "bold" : "normal",
						"paint-order": "stroke fill",
						stroke: W.value.stroke,
						"stroke-width": W.value.strokeWidth,
						"stroke-linejoin": "round",
						"stroke-linecap": "round",
						"pointer-events": "none",
						style: { "user-select": "none" },
						"dominant-baseline": "central"
					}, D(O(o)(e.label, W.value.ellipsisThresholdChars)), 9, ze)) : v("", !0)
				], 46, Pe))), 128)),
				(w(!0), y(g, null, ge(Xn.value, (e) => (w(), y("g", {
					key: `stack-overflow-${e.stackId}`,
					class: pe(["vue-ui-hill__stack-overflow-marker", {
						"vue-ui-hill__stack-overflow-marker--animated": O(F),
						"vue-ui-hill__stack-overflow-marker--restored": pn.value === e.stackId
					}]),
					style: C({
						"--vue-ui-hill-overflow-marker-duration": `${J.value.transitionDuration * .7}ms`,
						"--vue-ui-hill-overflow-marker-delay": `${J.value.transitionDuration * .3}ms`,
						"--vue-ui-hill-overflow-label-duration": `${J.value.transitionDuration * .55}ms`,
						"--vue-ui-hill-overflow-label-delay": `${J.value.transitionDuration * .45}ms`,
						cursor: zt.value && !P.value.readonly ? "pointer" : "default"
					}),
					transform: `translate(${e.x} ${e.y})`,
					"data-stack-overflow-marker": "",
					role: "button",
					tabindex: I.value && z.value ? 0 : -1,
					"aria-label": `${e.hiddenCount} stacked datapoints. Activate to choose one.`,
					"aria-expanded": Z.value === e.stackId,
					onPointerdown: t[0] ||= A(() => {}, ["stop"]),
					onClick: A((t) => nr(e), ["stop"]),
					onFocus: (t) => nr(e),
					onKeydown: [
						be(A((t) => nr(e), ["prevent", "stop"]), ["enter"]),
						be(A((t) => nr(e), ["prevent", "stop"]), ["space"]),
						be(A(rr, ["prevent", "stop"]), ["esc"])
					]
				}, [
					O(F) && pn.value !== e.stackId ? (w(), y("g", Ve, [(w(!0), y(g, null, ge(e.collapseDatapoints, (t) => (w(), y("circle", {
						key: `overflow-ghost-${e.stackId}-${t.id}`,
						cx: t.relativeX,
						cy: t.relativeY,
						r: U.value.radius,
						fill: t.color,
						stroke: U.value.stroke,
						"stroke-width": U.value.strokeWidth,
						opacity: "0.8",
						"vector-effect": "non-scaling-stroke"
					}, [
						b("animate", {
							attributeName: "cx",
							from: t.relativeX,
							to: "0",
							dur: `${J.value.transitionDuration}ms`,
							fill: "freeze",
							calcMode: "spline",
							keyTimes: "0;1",
							keySplines: "0.22 1 0.36 1"
						}, null, 8, Ue),
						b("animate", {
							attributeName: "cy",
							from: t.relativeY,
							to: "0",
							dur: `${J.value.transitionDuration}ms`,
							fill: "freeze",
							calcMode: "spline",
							keyTimes: "0;1",
							keySplines: "0.22 1 0.36 1"
						}, null, 8, We),
						b("animate", {
							attributeName: "r",
							from: U.value.radius,
							to: J.value.marker.radius * .7,
							dur: `${J.value.transitionDuration}ms`,
							fill: "freeze"
						}, null, 8, Ge),
						b("animate", {
							attributeName: "opacity",
							from: "0.8",
							to: "0",
							dur: `${J.value.transitionDuration * .85}ms`,
							fill: "freeze"
						}, null, 8, Ke)
					], 8, He))), 128))])) : v("", !0),
					b("circle", {
						r: Math.max(U.value.hitRadius, J.value.marker.radius),
						fill: "transparent",
						"pointer-events": "all"
					}, null, 8, qe),
					b("rect", {
						class: "vue-ui-hill__stack-overflow-marker-rect",
						x: -J.value.marker.radius,
						y: -J.value.marker.radius,
						width: J.value.marker.radius * 2,
						height: J.value.marker.radius * 2,
						fill: J.value.marker.fill,
						stroke: J.value.marker.stroke,
						"stroke-width": J.value.marker.strokeWidth,
						"vector-effect": "non-scaling-stroke",
						"pointer-events": "none"
					}, null, 8, Je),
					b("g", {
						class: "vue-ui-hill__stack-overflow-focus-cross",
						fill: "none",
						stroke: O(l)(H.value.curve.stroke),
						"stroke-width": U.value.dragMarker.strokeWidth,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						"pointer-events": "none",
						"vector-effect": "non-scaling-stroke",
						"aria-hidden": "true"
					}, [b("path", { d: U.value.dragMarker.crossPath }, null, 8, Xe)], 8, Ye),
					Zn.value ? v("", !0) : (w(), y("text", {
						key: 1,
						class: "vue-ui-hill__stack-overflow-marker-label",
						"text-anchor": "middle",
						y: J.value.marker.radius + J.value.marker.offsetY + J.value.marker.fontSize,
						fill: J.value.marker.color,
						"font-size": J.value.marker.fontSize,
						"font-weight": J.value.marker.bold ? "bold" : "normal",
						"pointer-events": "none",
						style: { "user-select": "none" },
						"paint-order": "stroke fill",
						"vector-effect": "non-scaling-stroke",
						stroke: P.value.style.chart.backgroundColor,
						"stroke-width": "3"
					}, " +" + D(e.hiddenCount), 9, Ze))
				], 46, Be))), 128)),
				G.value.show ? (w(), y(g, { key: 4 }, [b("text", {
					x: (Y.value.startX + Y.value.centerX) / 2,
					y: Vn.value,
					"text-anchor": "middle",
					fill: G.value.color,
					"font-size": G.value.fontSize,
					"font-weight": G.value.bold ? "bold" : "normal",
					"letter-spacing": G.value.letterSpacing
				}, D(Hn("left")), 9, Qe), b("text", {
					x: (Y.value.centerX + Y.value.endX) / 2,
					y: Vn.value,
					"text-anchor": "middle",
					fill: G.value.color,
					"font-size": G.value.fontSize,
					"font-weight": G.value.bold ? "bold" : "normal",
					"letter-spacing": G.value.letterSpacing
				}, D(Hn("right")), 9, $e)], 64)) : v("", !0),
				Sr.value ? (w(), y(g, { key: 5 }, [
					b("rect", {
						x: Y.value.left[0].x,
						y: Dr.value,
						rx: q.value.height / 2,
						width: Y.value.width,
						height: q.value.height,
						fill: q.value.gutterColor
					}, null, 8, et),
					b("defs", null, [b("clipPath", { id: kr.value }, [b("rect", {
						x: Y.value.left[0].x,
						y: Dr.value,
						width: Y.value.width * wr.value,
						height: q.value.height,
						rx: Math.min(q.value.height / 2, Y.value.width * wr.value / 2),
						ry: Math.min(q.value.height / 2, Y.value.width * wr.value / 2)
					}, null, 8, nt)], 8, tt)]),
					b("g", { "clip-path": `url(#${kr.value})` }, [(w(!0), y(g, null, ge(Cr.value, (e) => (w(), y("rect", {
						key: `bar_fill_${e.id}`,
						width: Y.value.width * e.proportion,
						x: Y.value.left[0].x + Y.value.width * e.proportionStart,
						y: V.value.height + 32 + q.value.paddingTop,
						fill: e.color,
						height: q.value.height,
						onPointerenter: (t) => In(e),
						onPointerleave: (t) => Ln(e)
					}, null, 40, it))), 128))], 8, rt),
					(w(!0), y(g, null, ge(Cr.value.slice(1), (e) => (w(), y("line", {
						key: `bar_separator_${e.id}`,
						x1: Y.value.left[0].x + Y.value.width * e.proportionStart,
						x2: Y.value.left[0].x + Y.value.width * e.proportionStart,
						y1: V.value.height + 32 + q.value.paddingTop,
						y2: V.value.height + 32 + q.value.paddingTop + q.value.height,
						stroke: q.value.stroke,
						"stroke-width": q.value.strokeWidth,
						"vector-effect": "non-scaling-stroke"
					}, null, 8, at))), 128)),
					b("rect", {
						x: Y.value.left[0].x,
						y: V.value.height + 32 + q.value.paddingTop,
						width: Y.value.width * wr.value,
						height: q.value.height,
						rx: Math.min(q.value.height / 2, Y.value.width * wr.value / 2),
						ry: Math.min(q.value.height / 2, Y.value.width * wr.value / 2),
						fill: "none",
						stroke: q.value.stroke,
						"stroke-width": q.value.strokeWidth,
						"vector-effect": "non-scaling-stroke"
					}, null, 8, ot),
					q.value.label.show && wr.value > 0 ? (w(), y("text", {
						key: 0,
						x: Er.value,
						y: Dr.value - 6,
						fill: q.value.label.color,
						"font-size": q.value.label.fontSize,
						"text-anchor": "end",
						"dominant-baseline": "auto",
						"pointer-events": "none",
						style: { "user-select": "none" }
					}, D(Or()), 9, st)) : v("", !0)
				], 64)) : v("", !0),
				E(e.$slots, "svg", { svg: {
					drawingArea: Y.value,
					isEditing: I.value,
					datapoints: Yn.value,
					isPrintingImg: O(Ft) || O(It) || O(_r),
					isPrintingSvg: O(vr)
				} }, void 0, !0)
			], 46, Te)),
			Zn.value ? (w(), y("div", {
				ref_key: "overflowMenuRef",
				ref: Tt,
				key: `stack-overflow-menu-${Zn.value.stackId}`,
				class: "vue-ui-hill__stack-overflow-menu",
				"data-stack-overflow-menu": "",
				"data-dom-to-png-ignore": "",
				style: C([Qn.value, fn.value]),
				role: "listbox",
				"aria-label": `Choose one of ${Zn.value.hiddenCount} stacked datapoints`,
				onPointerdown: t[1] ||= A(() => {}, ["stop"]),
				onClick: t[2] ||= A(() => {}, ["stop"]),
				onKeydown: be(A(rr, ["prevent", "stop"]), ["esc"])
			}, [P.value.style.chart.layout.plots.stacking.overflow.menu.title ? (w(), y("div", lt, D(P.value.style.chart.layout.plots.stacking.overflow.menu.title), 1)) : v("", !0), (w(!0), y(g, null, ge(Zn.value.hiddenDatapoints, (e) => (w(), y("button", {
				key: `hidden-${e.id}`,
				type: "button",
				class: pe(["vue-ui-hill__stack-overflow-menu-item", { readonly: P.value.readonly }]),
				role: "option",
				disabled: e.disabled,
				"aria-label": `${e.label}, ${pr(e)}`,
				onClick: A((t) => lr(e), ["stop"]),
				style: C({ cursor: zt.value && !P.value.readonly ? "pointer" : "default" })
			}, [
				b("span", {
					class: "vue-ui-hill__stack-overflow-menu-swatch",
					style: C({ backgroundColor: e.color }),
					"aria-hidden": "true"
				}, null, 4),
				b("span", dt, D(O(o)(e.label, W.value.ellipsisThresholdChars)), 1),
				b("span", ft, D(pr(e)), 1)
			], 14, ut))), 128))], 44, ct)) : v("", !0),
			E(e.$slots, "analysis", { data: { ...Cr.value } }, void 0, !0),
			e.$slots.watermark ? (w(), y("div", pt, [E(e.$slots, "watermark", S(x({ isPrinting: O(Ft) || O(It) || O(_r) || O(vr) })), void 0, !0)])) : v("", !0),
			P.value.loading ? (w(), y("div", mt, [P.value.loading ? E(e.$slots, "loading", {}, () => [ue(O(yt), {
				name: "spinner2",
				stroke: P.value.style.chart.color,
				"is-spin": !0
			}, null, 8, ["stroke"])], !0, 0) : v("", !0)])) : v("", !0)
		], 44, Se));
	}
}, [["__scopeId", "data-v-3c476fa7"]]);
//#endregion
export { xe as n, ht as t };
