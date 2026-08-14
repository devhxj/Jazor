import { B as e, Ct as t, H as n, I as r, P as i, V as a, Yt as o, q as ee, r as te, t as ne, xt as re } from "./lib-Bttd6u5E.js";
import { t as s } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { i as ie, l as c, t as ae } from "./useResponsive-ZtArZtUf.js";
import { t as oe } from "./BaseIcon-BfndwIWE.js";
import { t as se } from "./DefGrad-DVBqDjhO.js";
import { Fragment as l, computed as u, createCommentVNode as d, createElementBlock as f, createElementVNode as p, createVNode as ce, guardReactiveProps as le, mergeProps as ue, nextTick as de, normalizeClass as m, normalizeProps as fe, normalizeStyle as h, onBeforeUnmount as pe, onMounted as me, onUpdated as he, openBlock as g, ref as _, renderList as v, renderSlot as ge, toDisplayString as _e, unref as ve, useCssVars as ye, vModelText as be, watch as xe, withDirectives as Se, withKeys as Ce, withModifiers as we } from "vue";
//#region src/atoms/SlicerPreview.vue
var Te = ["data-minimap"], Ee = {
	class: "vue-data-ui-slicer-labels",
	style: {
		position: "relative",
		"z-index": "1",
		"pointer-events": "none"
	}
}, De = {
	key: 0,
	style: {
		width: "100%",
		position: "relative",
		"pointer-events": "all"
	}
}, Oe = {
	key: 0,
	class: "minimap",
	style: { width: "100%" }
}, ke = ["xmlns", "viewBox"], Ae = ["id"], je = [
	"x",
	"width",
	"height"
], Me = [
	"width",
	"height",
	"stroke"
], Ne = ["d", "stroke"], Pe = ["d", "fill"], Fe = [
	"x",
	"y",
	"width",
	"height",
	"fill"
], Ie = [
	"d",
	"stroke",
	"stroke-dasharray"
], Le = [
	"cx",
	"cy",
	"fill",
	"stroke"
], Re = [
	"cx",
	"cy",
	"fill",
	"stroke"
], ze = [
	"x",
	"width",
	"height",
	"fill",
	"rx"
], Be = [
	"x",
	"width",
	"height",
	"rx",
	"fill"
], Ve = [
	"x",
	"width",
	"height",
	"fill",
	"rx",
	"aria-valuemin",
	"aria-valuemax",
	"aria-valuenow",
	"aria-valuetext"
], He = [
	"x2",
	"y1",
	"y2",
	"stroke"
], Ue = { key: "merged-tree" }, We = ["d", "stroke"], Ge = [
	"cx",
	"cy",
	"stroke",
	"fill"
], Ke = [
	"cx",
	"cy",
	"stroke",
	"fill"
], qe = { key: "split-tree" }, Je = [
	"x",
	"y",
	"width",
	"height",
	"fill"
], Ye = [
	"d",
	"stroke",
	"stroke-dasharray"
], Xe = [
	"cx",
	"cy",
	"fill",
	"stroke"
], Ze = ["width", "height"], Qe = [
	"x",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], $e = ["transform"], et = ["width", "height"], tt = ["fill", "stroke"], nt = ["stroke"], rt = ["fill"], it = [
	"x",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], at = ["transform"], ot = ["width", "height"], st = ["fill", "stroke"], ct = ["stroke"], lt = ["fill"], ut = {
	key: "merged-tree",
	style: { "pointer-events": "none" }
}, dt = [
	"cx",
	"cy",
	"stroke",
	"fill"
], ft = [
	"cx",
	"cy",
	"fill"
], pt = [
	"cx",
	"cy",
	"stroke",
	"fill"
], mt = [
	"cx",
	"cy",
	"fill"
], ht = {
	key: 1,
	style: { "pointer-events": "none" }
}, gt = [
	"d",
	"stroke",
	"stroke-dasharray"
], _t = [
	"cx",
	"cy",
	"fill",
	"stroke"
], vt = [
	"cx",
	"cy",
	"stroke",
	"fill"
], yt = [
	"cx",
	"cy",
	"fill"
], bt = [
	"cx",
	"cy",
	"stroke",
	"fill"
], xt = [
	"cx",
	"cy",
	"fill"
], St = [
	"cx",
	"cy",
	"fill",
	"stroke"
], Ct = [
	"step",
	"min",
	"max",
	"tabindex"
], wt = [
	"step",
	"min",
	"max",
	"tabindex"
], Tt = {
	key: 3,
	class: "minimap-handle-overlay"
}, Et = ["xmlns", "viewBox"], Dt = [
	"x",
	"width",
	"height"
], Ot = [
	"x",
	"width",
	"height"
], kt = 48, At = /*#__PURE__*/ s({
	__name: "SlicerPreview",
	props: {
		uuid: {
			type: String,
			default: ""
		},
		immediate: {
			type: Boolean,
			default: !0
		},
		background: {
			type: String,
			default: "#FFFFFF"
		},
		borderColor: {
			type: String,
			default: "#FFFFFF"
		},
		fontSize: {
			type: Number,
			default: 14
		},
		labelLeft: {
			type: [String, Number],
			default: ""
		},
		labelRight: {
			type: [String, Number],
			default: ""
		},
		textColor: {
			type: String,
			default: "#1A1A1A"
		},
		inputColor: {
			type: String,
			default: "#1A1A1A"
		},
		max: {
			type: Number,
			default: 0
		},
		min: {
			type: Number,
			default: 0
		},
		selectColor: {
			type: String,
			default: "#4A4A4A"
		},
		useResetSlot: {
			type: Boolean,
			default: !1
		},
		valueStart: {
			type: [Number, String],
			default: 0
		},
		valueEnd: {
			type: [Number, String],
			default: 0
		},
		minimap: {
			type: Array,
			default: []
		},
		smoothMinimap: {
			type: Boolean,
			default: !1
		},
		minimapSelectedColor: {
			type: String,
			default: "#1f77b4"
		},
		minimapSelectionRadius: {
			type: Number,
			default: 12
		},
		minimapLineColor: {
			type: String,
			default: "#2D353C"
		},
		minimapSelectedColorOpacity: {
			type: Number,
			default: .2
		},
		minimapSelectedIndex: {
			type: Number,
			default: null
		},
		minimapIndicatorColor: {
			type: String,
			default: "#2D353C"
		},
		refreshStartPoint: {
			type: Number,
			default: 0
		},
		refreshEndPoint: {
			type: Number,
			default: null
		},
		enableRangeHandles: {
			type: Boolean,
			default: !1
		},
		enableSelectionDrag: {
			type: Boolean,
			default: !0
		},
		verticalHandles: {
			type: Boolean,
			default: !1
		},
		timeLabels: { type: Array },
		isPreview: {
			type: Boolean,
			default: !1
		},
		preciseLabels: {
			type: Array,
			default() {
				return [];
			}
		},
		usePreciseLabels: {
			type: Boolean,
			default: !1
		},
		selectedSeries: { type: Object },
		customFormat: { type: [Function, null] },
		minimapCompact: {
			type: Boolean,
			default: !1
		},
		allMinimaps: {
			type: Array,
			default() {
				return [];
			}
		},
		minimapMerged: {
			type: Boolean,
			default: !1
		},
		minimapFrameColor: {
			type: String,
			default: "#e1e5e8"
		},
		cutNullValues: {
			type: Boolean,
			default: !1
		},
		focusOnDrag: {
			type: Boolean,
			default: !1
		},
		focusRangeRatio: {
			type: Number,
			default: .1
		},
		minScale: {
			type: Number,
			default: null
		},
		maxScale: {
			type: Number,
			default: null
		},
		forceZeroCenter: {
			type: Boolean,
			default: !1
		},
		maxWidth: {
			type: Number,
			default: null
		},
		minimapLeftInsetRatio: {
			type: Number,
			default: null
		},
		minimapRightInsetRatio: {
			type: Number,
			default: null
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		},
		additionalMinimapHeight: {
			type: Number,
			default: 0
		},
		handleType: {
			type: String,
			default: ""
		},
		handleWidth: {
			type: Number,
			default: 20
		},
		handleBorderWidth: {
			type: Number,
			default: 1
		},
		handleIconColor: {
			type: String,
			default: null
		},
		handleBorderColor: {
			type: String,
			default: null
		},
		handleFill: {
			type: String,
			default: null
		},
		precision: {
			type: Number,
			default: 0
		},
		useValueRange: {
			type: Boolean,
			default: !1
		}
	},
	emits: [
		"futureStart",
		"futureEnd",
		"update:start",
		"update:end",
		"reset",
		"trapMouse",
		"trapMouseValue"
	],
	setup(s, { expose: At, emit: jt }) {
		ye((e) => ({
			v13222322: sn.value,
			a9d0a8d4: N.value,
			cfb69ef2: ln.value,
			e9662674: s.selectColor,
			v05118454: cn.value,
			de2dd9f4: y.additionalMinimapHeight
		}));
		let y = s;
		function Mt(e) {
			let t = Number(e) || 0;
			return y.precision <= 0 ? Math.round(t) : Number(t.toFixed(y.precision));
		}
		let b = _(null), x = _(y.min), S = _(y.max), C = u(() => !!y.allMinimaps.length), Nt = u(() => t(y.minimapLeftInsetRatio) && t(y.minimapRightInsetRatio)), Pt = u(() => {
			let e = Math.min(1, Math.max(0, y.minimapLeftInsetRatio)) * 100, t = Math.min(1, Math.max(0, y.minimapRightInsetRatio)) * 100;
			return {
				padding: Nt.value ? `0 ${t}% 0 ${e}%` : "0 48px",
				maxWidth: y.maxWidth && !Nt.value ? `${y.maxWidth}px` : void 0,
				margin: y.maxWidth && !Nt.value ? "0 auto" : void 0
			};
		}), Ft = _(ee()), w = _(!1), T = u(() => C.value && y.minimapCompact), E = _(null), It = _(!1), Lt = _(0), Rt = _(0), zt = _(null), Bt = _(!1), Vt = u(() => E.value !== null), Ht = u(() => y.useValueRange ? S.value : S.value - 1), Ut = u(() => Math.max(1, Number(y.max) - Number(y.min)));
		function Wt(e) {
			return (Number(e) - Number(y.min)) / Ut.value * 100;
		}
		function Gt(e) {
			return F.value.width * (Wt(e) / 100);
		}
		function Kt(e) {
			let t = Math.min(1, Math.max(0, e / Math.max(1, F.value.width)));
			return Mt(Number(y.min) + Ut.value * t);
		}
		function qt(e) {
			let n = y.useValueRange ? e?.y : e;
			return t(n) ? Number(n) : null;
		}
		function Jt(e, t) {
			return y.useValueRange ? Number(e?.x) : t + Number(y.min);
		}
		function Yt(e, t) {
			return y.useValueRange ? Gt(Jt(e, t)) : B.value * t + (y.minimapCompact ? 0 : B.value / 2);
		}
		let Xt = u(() => Math.min(40, Math.max(20, y.handleWidth))), Zt = _(0), Qt = c((e) => j("futureStart", e), 0), $t = c((e) => j("futureEnd", e), 0), D = u(() => y.precision > 0 ? 1 / 10 ** y.precision : 1), O = u({
			get: () => x.value,
			set(e) {
				let t = Math.min(Number(e), Number(S.value) - D.value);
				t !== x.value && (x.value = t, Un.value && (Un.value.value = String(t)), y.immediate ? j("update:start", Number(t)) : w.value && Qt(t));
			}
		}), k = u({
			get: () => S.value,
			set(e) {
				let t = Math.max(Number(e), Number(x.value) + D.value);
				t !== S.value && (S.value = t, Wn.value && (Wn.value.value = String(t)), y.immediate ? j("update:end", Number(t)) : w.value && $t(t));
			}
		});
		function A() {
			clearTimeout(null), y.immediate || (j("update:start", Number(x.value)), j("update:end", Number(S.value))), w.value = !1;
		}
		let en = u(() => y.refreshEndPoint === null ? y.max : y.refreshEndPoint), j = jt, tn = u(() => {
			if (y.useValueRange) return Wt(x.value);
			if (T.value) {
				let e = Math.max(1, L.value - 1);
				return Pn.value / e * 100;
			}
			let e = Math.max(1, y.max - y.min);
			return (x.value - y.min) / e * 100;
		}), nn = u(() => {
			if (y.useValueRange) return Wt(S.value);
			if (T.value) {
				let e = Math.max(1, L.value - 1);
				return Fn.value / e * 100;
			}
			let e = Math.max(1, y.max - y.min);
			return (S.value - y.min) / e * 100;
		}), rn = u(() => (tn.value + nn.value) / 2), an = u(() => {
			if (!b.value) return !1;
			let e = Math.max(1, y.max - y.min);
			return b.value.getBoundingClientRect().width * ((x.value - y.min) / e) - vr.value / 2 < 0;
		}), on = u(() => {
			if (!b.value) return !1;
			let e = Math.max(1, y.max - y.min), t = b.value.getBoundingClientRect().width;
			return t * ((S.value - y.min) / e) + yr.value / 2 > t;
		}), M = u(() => {
			let e = on.value ? `calc(${rn.value}% - ${Tr.value.width}px)` : an.value ? `calc(${rn.value}% - 8px)` : `calc(${rn.value}% - ${Tr.value.width / 2}px)`;
			return {
				left: `${tn.value}%`,
				width: `${Math.max(0, nn.value - tn.value)}%`,
				background: y.selectColor,
				tooltipLeft: `calc(${tn.value}% - ${an.value ? 9 : vr.value / 2}px)`,
				tooltipRight: `calc(${nn.value}% - ${on.value ? yr.value - 9 : yr.value / 2}px)`,
				tooltipCenter: e,
				arrowLeft: !an.value,
				arrowRight: !on.value
			};
		}), sn = u(() => y.inputColor), cn = u(() => y.background), ln = u(() => `${y.selectColor}33`), N = u(() => y.borderColor), un = u(() => Number(y.max) > Number(y.min));
		function dn() {
			j("reset");
		}
		xe(() => y.min, (e) => {
			Number(x.value) < Number(e) && (x.value = Number(e)), Number(S.value) < Number(e) && (S.value = Number(e));
		}), xe(() => y.max, (e) => {
			Number(x.value) > Number(e) && (x.value = Number(e)), Number(S.value) > Number(e) && (S.value = Number(e));
		});
		let P = _(null), F = _({
			width: 1,
			height: 1
		}), I = _(null);
		me(() => {
			if (C.value) {
				let e = c(() => {
					if (!P.value) return;
					let { width: e, height: t } = ae({ chart: P.value }), n = Math.max(0, Math.round(e)), r = Math.max(0, Math.round(t - 47));
					(n !== F.value.width || r !== F.value.height) && (F.value.width = n, F.value.height = r + y.additionalMinimapHeight);
				}, 0);
				I.value = new ResizeObserver(e), I.value.observe(P.value);
			}
		}), pe(() => {
			I.value && I.value.disconnect();
		});
		let L = u(() => Math.max(1, y.max - y.min));
		function fn(e) {
			let t = Math.floor(e - y.min);
			return Math.min(Math.max(0, t), L.value);
		}
		function pn(e) {
			let t = Math.ceil(e - y.min);
			return Math.min(Math.max(0, t), L.value);
		}
		let R = u(() => fn(x.value)), z = u(() => pn(S.value)), mn = u(() => Math.max(...y.allMinimaps.map((e) => e.series.length))), hn = u(() => C.value && y.minimapCompact ? 40 : 0), gn = u(() => hn.value / 2);
		u(() => Math.max(1, F.value.width - gn.value * 2));
		let B = u(() => {
			if (y.minimapCompact && !y.useValueRange) return F.value.width / Math.max(1, L.value - 1);
			let e = Math.max(1, mn.value - +!!y.minimapCompact);
			return F.value.width / e;
		}), _n = u(() => y.allMinimaps.length ? y.allMinimaps.filter((e) => e.type === "bar" && e.isVisible).length : 0), vn = u(() => B.value / (_n.value || 1) * .8);
		function yn(e, t, n) {
			let r = vn.value, i = Math.max(1, _n.value), a = mn.value - 1;
			return n === 0 ? e + r / 2 * t : n === a ? e - r / 2 * (i - t) : e - i * r / 2 + t * r;
		}
		function bn(e, t) {
			return [0, mn.value - 1].includes(t) ? vn.value / 2 : vn.value;
		}
		let xn = u(() => {
			let e = [];
			if (Array.isArray(y.minimap) && y.minimap.length && y.minimapMerged && e.push(...y.minimap.map(qt).filter(Number.isFinite)), Array.isArray(y.allMinimaps) && y.allMinimaps.length) for (let t of y.allMinimaps) t?.isVisible && Array.isArray(t?.series) && e.push(...t.series.map(qt).filter(Number.isFinite));
			return e.length ? {
				min: Math.min(...e),
				max: Math.max(...e)
			} : {
				min: 0,
				max: 1
			};
		}), Sn = u(() => {
			if (y.minScale == null && y.forceZeroCenter) return null;
			let e = Number(y.minScale);
			return Number.isFinite(e) ? e : null;
		}), Cn = u(() => {
			if (y.maxScale == null && y.forceZeroCenter) return null;
			let e = Number(y.maxScale);
			return Number.isFinite(e) ? e : null;
		}), wn = u(() => Sn.value !== null && Cn.value !== null), Tn = u(() => {
			let { min: e, max: t } = xn.value, n = Sn.value, r = Cn.value, i, a;
			return n !== null && r !== null ? (i = Math.min(n, e), a = Math.max(r, t)) : (i = n === null ? e : n, a = r === null ? t : r), Number.isFinite(i) || (i = 0), Number.isFinite(a) || (a = 1), i === a ? a = i + 1 : i > a && ([i, a] = [a, i]), {
				min: i,
				max: a
			};
		}), En = u(() => Tn.value.min), Dn = u(() => Tn.value.max), On = u(() => En.value < 0 && Dn.value > 0 || Dn.value <= 0 ? En.value : 0), kn = (e) => {
			let t = Math.max(1, F.value.height);
			return jn(En.value, Dn.value, t, wn.value)(e);
		}, An = u(() => kn(0));
		function jn(e, t, n, r = !1) {
			let i = (e, t, n) => Math.max(t, Math.min(n, e)), a = 1e-9, o = Math.max(a, t - e);
			if (r) return (t) => n - (t - e) / o * n;
			if (t <= 0) {
				let t = Math.max(a, 0 - e);
				return (r) => n - (r - e) / t * n;
			}
			if (e >= 0) {
				let e = Math.max(a, t - 0);
				return (t) => n - (t - 0) / e * n;
			}
			{
				let r = Math.max(a, Math.max(Math.abs(e), Math.abs(t)));
				return (e) => (1 - (i(e / r, -1, 1) + 1) / 2) * n;
			}
		}
		function Mn(t, o = !1, ee = !1) {
			if (!t || !t.length) return {
				fullSet: "",
				points: [],
				selectionSet: "",
				sliced: [],
				firstPlot: null,
				lastPlot: null,
				hasFull: !1,
				hasSelection: !1,
				fullMarkers: [],
				selectionMarkers: [],
				dashed: !1
			};
			let te = Math.max(1, F.value.height), ne = jn(En.value, Dn.value, te, wn.value), re = t.length, s = Math.min(Math.max(0, R.value), Math.max(0, re - 1)), ie = Math.min(re, Math.max(s + 1, z.value)), c = t.map((e, t) => {
				let n = qt(e), r = Number.isFinite(n), i = Yt(e, t), a = wn.value ? ((e, t, n) => Math.max(t, Math.min(n, e)))(0, En.value, Dn.value) : 0, o = ne(a);
				return {
					x: i,
					y: r ? ne(n) : NaN,
					v: n,
					value: r ? n : null,
					y0: o,
					i: t,
					xValue: Jt(e, t)
				};
			}), ae = (e) => e >= 0 && e < c.length && Number.isFinite(c[e]?.value), oe = c.filter((e) => Number.isFinite(e.value) && !ae(e.i - 1) && !ae(e.i + 1)), se = y.useValueRange ? oe.filter((e) => e.xValue >= Number(x.value) && e.xValue <= Number(S.value)) : oe.filter((e) => e.i >= s && e.i < ie), l = y.useValueRange ? c.filter((e) => e.xValue >= Number(x.value) && e.xValue <= Number(S.value)) : c.slice(s, ie);
			return {
				fullSet: c.length >= 2 ? ee ? e(y.cutNullValues ? c : c.filter((e) => e.value != null)) : y.smoothMinimap || o ? y.cutNullValues ? r(c) : i(c.filter((e) => e.value != null)) : y.cutNullValues ? n(c) : a(c.filter((e) => e.value != null)) : "",
				points: c,
				selectionSet: l.length >= 2 ? ee ? e(y.cutNullValues ? l : l.filter((e) => e.value != null)) : y.smoothMinimap || o ? y.cutNullValues ? r(l) : i(l.filter((e) => e.value != null)) : y.cutNullValues ? n(l) : a(l.filter((e) => e.value != null)) : "",
				sliced: l,
				firstPlot: y.useValueRange ? l[0] || null : c[s] || null,
				lastPlot: y.useValueRange ? l[l.length - 1] || null : c[Math.max(0, ie - 1)] || null,
				hasFull: c.length >= 2,
				hasSelection: l.length >= 2,
				fullMarkers: oe,
				selectionMarkers: se
			};
		}
		let V = u(() => y.minimap.length ? Mn(y.minimap) : []), H = u(() => y.allMinimaps.length ? y.allMinimaps.map((e, t) => {
			let n = Mn(e?.series || [], !!e.smooth, !!e.useStepper), r = e?.id ?? e?.name ?? t;
			return {
				key: typeof r == "object" ? JSON.stringify(r) : String(r),
				color: e?.color,
				...n,
				temperatureColors: e?.temperatureColors ?? null,
				isVisible: e.isVisible,
				type: e.type || void 0,
				dashed: e.dashed ?? !1,
				useStepper: !!e.useStepper
			};
		}) : []), U = u(() => {
			if (y.useValueRange) {
				let e = Gt(x.value), t = Gt(S.value);
				return {
					x: e,
					width: Math.max(0, t - e)
				};
			}
			let e = R.value, t = Math.max(e + 1, z.value);
			return {
				x: B.value * e + (y.minimapCompact ? 0 : B.value / 2),
				width: B.value * (t - e) - B.value
			};
		}), W = _(y.minimapSelectedIndex), Nn = (e) => Math.round(y.min + e), Pn = u({
			get() {
				return T.value ? R.value : Number(O.value);
			},
			set(e) {
				if (T.value) {
					if (y.useValueRange) {
						let t = Mt(e);
						qn(Math.min(Math.max(Number(y.min), t), Number(S.value) - D.value));
						return;
					}
					let t = Math.round(+e || 0);
					qn(Nn(t));
				} else {
					let t = Mt(e), n = Number(S.value) - D.value, r = Math.min(Math.max(y.min, t), n);
					Un.value && (Un.value.valueAsNumber = r), qn(r);
				}
			}
		}), Fn = u({
			get() {
				return T.value ? Math.max(R.value, z.value - 1) : Number(k.value);
			},
			set(e) {
				if (T.value) {
					if (y.useValueRange) {
						let t = Mt(e);
						Yn(Math.max(Number(x.value) + D.value, Math.min(t, Number(y.max))));
						return;
					}
					let t = Math.round(+e || 0);
					Yn(Nn(t + 1));
				} else {
					let t = Mt(e), n = Number(x.value) + D.value, r = Math.max(n, Math.min(t, y.max));
					Wn.value && (Wn.value.valueAsNumber = r), Yn(r);
				}
			}
		}), In = 0;
		function Ln(e, t) {
			if (t === In) {
				if (y.useValueRange) {
					W.value = Number.isFinite(Number(e)) ? Gt(Number(e)) : null;
					return;
				}
				W.value = fn(y.valueStart) + e;
			}
		}
		let Rn = ie(Ln, 60);
		xe(() => y.minimapSelectedIndex, (e, t) => {
			if (In += 1, [null, void 0].includes(e)) {
				W.value = null;
				return;
			}
			e !== t && Rn(e, In);
		}, { immediate: !0 });
		function zn(e) {
			if (Vt.value) return;
			W.value = e;
			let t = R.value, n = z.value;
			e >= t && e < n && !X.value && j("trapMouse", e - t);
		}
		function Bn(e) {
			let t = Kt(e), n = y.allMinimaps.flatMap((e) => (e.series || []).map((e, n) => ({
				point: e,
				index: n,
				x: Number(e?.x),
				distance: Math.abs(Number(e?.x) - t)
			}))).filter((e) => Number.isFinite(e.x));
			return n.length ? n.reduce((e, t) => t.distance < e.distance ? t : e, n[0]) : null;
		}
		function Vn(e) {
			if (!un.value || Vt.value) return;
			let t = Vr(e.clientX);
			if (y.useValueRange) {
				let e = Bn(t);
				if (!e) {
					Hn();
					return;
				}
				W.value = Gt(e.x), j("trapMouse", e.index), j("trapMouseValue", e.x);
				return;
			}
			zn(Hr(t));
		}
		function Hn() {
			W.value = null, !Vt.value && (j("trapMouse", null), j("trapMouseValue", null));
		}
		let Un = _(null), Wn = _(null);
		function Gn(e) {
			if (typeof e == "object" && e && "target" in e) {
				let t = e.target, n = "valueAsNumber" in t ? t.valueAsNumber : +t.value;
				return Number.isFinite(n) ? n : NaN;
			}
			let t = +e;
			return Number.isFinite(t) ? t : NaN;
		}
		let Kn = 0;
		function qn(e) {
			w.value = !0;
			let t = Gn(e);
			Number.isFinite(t) && (cancelAnimationFrame(Kn), Kn = requestAnimationFrame(() => {
				O.value = t;
			}));
		}
		let Jn = 0;
		function Yn(e) {
			w.value = !0;
			let t = Gn(e);
			Number.isFinite(t) && (cancelAnimationFrame(Jn), Jn = requestAnimationFrame(() => {
				k.value = t;
			}));
		}
		pe(() => {
			cancelAnimationFrame(Kn), cancelAnimationFrame(Jn);
		});
		let Xn = u(() => y.valueEnd - y.valueStart), Zn = u(() => Xn.value < y.max - y.min), G = _(!1), Qn = _(null), $n = u(() => (Zt.value - 48) / (y.max - y.min) * Xn.value), er = u(() => Math.max(1, Zt.value - kt - $n.value)), tr = u(() => Math.max(1, y.max - y.min - Xn.value)), nr = u(() => tr.value / er.value), rr = _(0), ir = _(0), ar = _(0), or = _(0), K = null, q = null, J = null, Y = null, sr = _(y.min);
		function cr(e) {
			if (!b.value) return y.min;
			let t = b.value.getBoundingClientRect(), n = t.left + kt / 2, r = t.right - kt / 2, i = Math.max(1, r - n), a = (Math.max(n, Math.min(e, r)) - n) / i, o = Math.max(1, y.max - y.min);
			return Math.round(y.min + a * o);
		}
		let lr = async (e) => {
			if (w.value = !0, Z.value = !0, !y.enableSelectionDrag) return;
			let t = e.type === "touchstart";
			t || e.stopPropagation();
			let n = t && e.targetTouches && e.targetTouches[0] ? e.targetTouches[0] : null, r = t ? n ? n.target : null : e.target;
			if (!r || !(r instanceof Element) || r.classList?.contains("range-handle") || r.classList?.contains("vue-ui-zoom-compact-minimap-handle") || r.closest(".vue-ui-zoom-compact-minimap-handle")) return;
			G.value = !0;
			let i = t ? n ? n.clientX : 0 : e.clientX;
			if (Qn.value = i, rr.value = i, y.focusOnDrag && !Zn.value && b.value && !Bt.value) {
				Bt.value = !0;
				try {
					sr.value = cr(i);
					let e = Math.min(.95, Math.max(.05, y.focusRangeRatio)), t = Number(y.max) - Number(y.min), n = Math.max(1, Math.round(t * e)), r = Math.floor(n / 2), a = sr.value - r;
					a = Math.max(Number(y.min), Math.min(a, Number(y.max) - n));
					let ee = Math.min(Number(y.max), a + n);
					O.value = a, k.value = ee, Qt(a), $t(ee), o(b.value, "mouseup"), await de(), o(b.value, "mousedown", { clientX: i });
				} finally {
					Bt.value = !1;
				}
				return;
			}
			ir.value = x.value, ar.value = S.value, or.value = nr.value, K = t ? "touchmove" : "mousemove", q = t ? "touchend" : "mouseup", J = t ? dr : ur, Y = t ? mr : pr, window.addEventListener(K, J, { passive: !1 }), window.addEventListener(q, Y);
		};
		function ur(e) {
			G.value && fr(e.clientX);
		}
		function dr(e) {
			if (!G.value || !b.value) return;
			let t = e.target;
			if (!(t instanceof Element) || !b.value.contains(t) || t.classList && t.classList.contains("range-handle")) return;
			e.preventDefault();
			let n = e.targetTouches && e.targetTouches[0] ? e.targetTouches[0] : null;
			n && fr(n.clientX);
		}
		function fr(e) {
			if (!G.value) return;
			let t = cr(rr.value), n = cr(e) - t, r = Math.round(ir.value + n);
			r = Math.max(y.min, Math.min(r, y.max - Xn.value));
			let i = r + Xn.value;
			O.value = r, k.value = i, Qt(r), $t(i);
		}
		function pr() {
			hr();
		}
		function mr() {
			hr();
		}
		function hr() {
			G.value = !1, K && J && window.removeEventListener(K, J), q && Y && window.removeEventListener(q, Y), K = q = null, J = Y = null, A();
		}
		let X = _(!1), gr = _(null), _r = _(null), vr = _(1), yr = _(1), Z = _(!1);
		function br() {
			if (gr.value) {
				let e = Math.round(gr.value.getBoundingClientRect().width);
				e !== vr.value && (vr.value = e);
			}
		}
		function xr() {
			if (_r.value) {
				let e = Math.round(_r.value.getBoundingClientRect().width);
				e !== yr.value && (yr.value = e);
			}
		}
		he(() => {
			br(), xr();
		});
		let Sr = _(0);
		function Cr(e) {
			Sr.value = +(e === "start");
		}
		let wr = _(!1), Tr = _({
			width: 0,
			left: 0
		});
		xe([x, S], async () => {
			if (await de(), !gr.value || !_r.value) {
				wr.value = !1, Tr.value = {
					width: 0,
					left: 0
				};
				return;
			}
			let e = gr.value.getBoundingClientRect(), t = _r.value.getBoundingClientRect();
			wr.value = e.x + e.width > t.x;
			let n = e.x + e.width / 2, r = t.x + t.width / 2, i = e.width + t.width, a = (n + r) / 2;
			Tr.value = {
				width: i,
				left: a - i / 2
			};
		}), he(() => {
			br(), xr();
		}), xe(() => y.labelLeft, () => {
			de(br);
		}, { deep: !0 }), xe(() => y.labelRight, () => {
			de(xr);
		}, { deep: !0 });
		let Q = u(() => {
			let e = "", t = "", n = !1;
			if (re(y.customFormat)) try {
				let r = y.customFormat({
					absoluteIndex: x.value,
					seriesIndex: x.value,
					datapoint: y.selectedSeries,
					timeLabel: y.preciseLabels[x.value],
					side: "left"
				}), i = y.customFormat({
					absoluteIndex: Ht.value,
					seriesIndex: -1,
					datapoint: y.selectedSeries,
					timeLabel: y.useValueRange ? null : y.preciseLabels[Ht.value],
					side: "right"
				});
				typeof r == "string" && typeof i == "string" && (e = r, t = i, n = !0);
			} catch {
				n = !1;
			}
			if (!n) {
				let n = y.useValueRange ? { text: String(Math.round(x.value * 1e3) / 1e3) } : y.usePreciseLabels ? y.preciseLabels.find((e) => e.absoluteIndex === x.value) : y.timeLabels.find((e) => e.absoluteIndex === x.value), r = y.useValueRange ? { text: String(Math.round(S.value * 1e3) / 1e3) } : y.usePreciseLabels ? y.preciseLabels.find((e) => e.absoluteIndex === Ht.value) : y.timeLabels.find((e) => e.absoluteIndex === Ht.value);
				e = n ? n.text : "", t = r ? r.text : "";
			}
			return {
				left: e,
				right: t
			};
		});
		pe(() => {
			I.value && I.value.disconnect(), K && J && window.removeEventListener(K, J), q && Y && window.removeEventListener(q, Y), K = q = null, J = Y = null, clearTimeout(null);
		});
		let Er = u(() => {
			if (!un.value || W.value === null) return null;
			if (y.useValueRange) {
				let e = W.value;
				return e < U.value.x || e > U.value.x + U.value.width ? null : {
					x1: e,
					x2: e,
					y1: 0,
					y2: Math.max(F.value.height, 0),
					stroke: y.minimapIndicatorColor,
					"stroke-linecap": "round",
					"stroke-dasharray": 2,
					"stroke-width": 1
				};
			}
			if (W.value >= R.value && W.value < z.value) {
				let e = W.value, t = B.value * e + (y.minimapCompact ? 0 : B.value / 2);
				return {
					x1: t,
					x2: t,
					y1: 0,
					y2: Math.max(F.value.height, 0),
					stroke: y.minimapIndicatorColor,
					"stroke-linecap": "round",
					"stroke-dasharray": 2,
					"stroke-width": 1
				};
			}
			return null;
		}), Dr = u(() => Number(x.value) < Number(S.value) - 1), Or = u(() => Number(x.value) > Number(y.min)), kr = u(() => Number(S.value) < Number(y.max)), Ar = u(() => Number(S.value) > Number(x.value) + 1);
		function jr(e) {
			let t = Number(x.value) + e;
			e > 0 && !Dr.value || e < 0 && !Or.value || (w.value = !0, O.value = t, A());
		}
		function Mr(e) {
			let t = Number(S.value) + e;
			e > 0 && !kr.value || e < 0 && !Ar.value || (w.value = !0, k.value = t, A());
		}
		let Nr = {
			plus: () => jr(1),
			minus: () => jr(-1),
			canPlus: Dr,
			canMinus: Or
		}, Pr = {
			plus: () => Mr(1),
			minus: () => Mr(-1),
			canPlus: kr,
			canMinus: Ar
		};
		function Fr(e) {
			if (!e || Ir(e.target)) return;
			let t = e.key, n = t === "ArrowLeft" || t === "ArrowDown" || t === "-" || t === "Subtract", r = t === "ArrowRight" || t === "ArrowUp" || t === "+" || t === "Add";
			!n && !r || (e.preventDefault(), e.stopPropagation(), r ? (Nr.plus(), Pr.plus()) : (Nr.minus(), Pr.minus()));
		}
		function Ir(e) {
			return e instanceof HTMLElement && (e.isContentEditable || e.tagName === "INPUT" || e.tagName === "TEXTAREA" || e.tagName === "SELECT");
		}
		function Lr(e) {
			if (!C.value || !y.minimapCompact) return;
			let t = b.value;
			if (!t) return;
			let n = e === "start" ? "[data-cy=\"slicer-compact-handle-left\"]" : "[data-cy=\"slicer-compact-handle-right\"]", r = t.querySelector(n);
			r && r instanceof SVGElement && typeof r.focus == "function" && r.focus();
		}
		function Rr(e, t) {
			if (!t || Ir(t.target)) return;
			let n = t.key, r = n === "ArrowLeft" || n === "ArrowDown" || n === "-" || n === "Subtract", i = n === "ArrowRight" || n === "ArrowUp" || n === "+" || n === "Add";
			!r && !i || (t.preventDefault(), t.stopPropagation(), e === "start" ? i ? Nr.plus() : Nr.minus() : i ? Pr.plus() : Pr.minus(), de(() => Lr(e)));
		}
		let zr = u(() => ({
			tabindex: 0,
			role: "slider",
			"aria-label": "Range start",
			"aria-valuemin": Number(y.min),
			"aria-valuemax": Math.max(Number(y.min), Number(S.value) - 1),
			"aria-valuenow": Number(x.value)
		})), Br = u(() => ({
			tabindex: 0,
			role: "slider",
			"aria-label": "Range end",
			"aria-valuemin": Math.min(Number(y.max), Number(x.value) + 1),
			"aria-valuemax": Number(y.max),
			"aria-valuenow": Number(S.value)
		}));
		function Vr(e) {
			if (!zt.value) return 0;
			let t = zt.value.getBoundingClientRect();
			if (!t.width) return 0;
			let n = (e - t.left) / t.width;
			return Math.min(1, Math.max(0, n)) * F.value.width;
		}
		function Hr(e) {
			let t = Math.max(0, L.value - 1), n = Math.max(1, F.value.width), r = Math.min(1, Math.max(0, e / n));
			return Math.min(t, Math.max(0, Math.round(r * t)));
		}
		function Ur(e) {
			let t = Math.min(Math.max(0, e), Math.max(1, F.value.width));
			if (y.useValueRange) {
				let e = Kt(t);
				if (E.value === "start") {
					let t = Math.min(e, Number(S.value) - D.value);
					O.value = t, Qt(t);
					return;
				}
				if (E.value === "end") {
					let t = Math.max(Number(x.value) + D.value, e), n = Math.min(Number(y.max), t);
					k.value = n, $t(n);
					return;
				}
			}
			let n = Hr(t);
			if (E.value === "start") {
				let e = Math.min(y.min + n, Number(S.value) - 1);
				O.value = e, Qt(e);
				return;
			}
			if (E.value === "end") {
				let e = Math.max(Number(x.value) + 1, y.min + n + 1), t = Math.min(y.max, e);
				k.value = t, $t(t);
			}
		}
		function Wr(e) {
			if (!E.value || !P.value) return;
			let t = e.clientX - Lt.value;
			if (!It.value) {
				if (Math.abs(t) < 4) return;
				It.value = !0, w.value = !0, Z.value = !0;
			}
			let n = Vr(e.clientX) - Rt.value;
			Ur(E.value === "start" ? n + Xt.value : n);
		}
		function Gr() {
			if (!E.value) return;
			window.removeEventListener("mousemove", Wr, !0), window.removeEventListener("mouseup", Gr, !0);
			let e = It.value;
			E.value = null, It.value = !1, Lt.value = 0, Rt.value = 0, e && A();
		}
		function Kr(e, t) {
			if (!C.value || !y.minimapCompact || !P.value) return;
			t.preventDefault(), t.stopPropagation(), W.value = null;
			let n = Vr(t.clientX), r = e === "start" ? U.value.x - Xt.value : U.value.x + U.value.width;
			E.value = e, It.value = !1, Lt.value = t.clientX, Rt.value = n - r, window.addEventListener("mousemove", Wr, !0), window.addEventListener("mouseup", Gr, !0);
		}
		let qr = u(() => R.value), Jr = u(() => Math.max(R.value, z.value - 1));
		u(() => {
			if (!C.value || !y.minimapCompact || !y.allMinimaps.length) return [];
			let e = qr.value, t = Jr.value;
			return H.value.flatMap((n) => {
				if (!n?.isVisible || !["line", "plot"].includes(n.type) || !n.type) return [];
				let r = [], i = n.points?.find((t) => t.i === e && t.value !== null), a = n.points?.find((e) => e.i === t && e.value !== null);
				return i && r.push({
					key: `${n.key}-left-${i.i}`,
					x: i.x,
					y: i.y,
					color: n.color
				}), a && t !== e && r.push({
					key: `${n.key}-right-${a.i}`,
					x: a.x,
					y: a.y,
					color: n.color
				}), r;
			});
		});
		let $ = u(() => Math.min(40, Math.max(20, y.handleWidth))), Yr = u(() => U.value.x - $.value), Xr = u(() => U.value.x + U.value.width);
		return At({
			setStartValue: qn,
			setEndValue: Yn
		}), (e, t) => (g(), f("div", {
			"data-minimap": C.value,
			"data-dom-to-png-ignore": "",
			"data-dom-to-png-ignore-layout": "",
			class: "vue-data-ui-zoom",
			ref_key: "zoomWrapper",
			ref: b,
			onMousedown: lr,
			onTouchstart: lr,
			style: h(Pt.value)
		}, [p("div", Ee, [s.valueStart !== s.refreshStartPoint || s.valueEnd !== en.value ? (g(), f("div", De, [ge(e.$slots, "reset-action", { reset: dn }, () => [p("button", {
			tabindex: "0",
			role: "button",
			class: "vue-data-ui-refresh-button",
			style: h({
				top: C.value ? "36px" : "-16px",
				cursor: s.isCursorPointer ? "pointer" : "default"
			}),
			onClick: dn
		}, [ce(oe, {
			name: "refresh",
			stroke: s.textColor
		}, null, 8, ["stroke"])], 4)], !0)])) : d("", !0)]), p("div", {
			class: "double-range-slider",
			ref_key: "minimapWrapper",
			ref: P,
			style: h([{ "z-index": "0" }, C.value ? {
				"--minimap-unit-px": B.value + "px",
				"--minimap-offset-px": (s.minimapCompact ? 0 : B.value / 2) + "px"
			} : void 0]),
			onMouseenter: t[18] ||= (e) => Z.value = !0,
			onMouseleave: t[19] ||= (e) => Z.value = !1
		}, [
			C.value ? (g(), f("div", Oe, [(g(), f("svg", {
				ref_key: "minimapSvg",
				ref: zt,
				key: `mm-${s.minimapMerged ? "merged" : "split"}-${s.minimapCompact ? "compact" : "normal"}`,
				xmlns: ve(ne),
				viewBox: `0 0 ${Math.max(0, F.value.width)} ${Math.max(0, F.value.height)}`,
				preserveAspectRatio: "none"
			}, [
				p("defs", null, [ce(se, {
					t: "linear",
					id: Ft.value,
					x1: "0%",
					y1: "0%",
					x2: "0%",
					y2: "100%",
					stops: [[
						"0%",
						s.minimapLineColor,
						.31
					], [
						"100%",
						"transparent",
						0
					]]
				}, null, 8, ["id", "stops"]), p("clipPath", { id: `selection_clip_${Ft.value}` }, [p("rect", {
					x: U.value.x,
					y: "0",
					width: Math.max(0, U.value.width),
					height: Math.max(F.value.height, 0)
				}, null, 8, je)], 8, Ae)]),
				s.minimapCompact ? (g(), f("rect", {
					key: 0,
					class: "vue-ui-zoom-minimap-frame",
					x: 0,
					y: 0,
					width: F.value.width,
					height: F.value.height,
					fill: "none",
					stroke: s.minimapFrameColor,
					rx: 3
				}, null, 8, Me)) : d("", !0),
				e.$slots.slotMap ? d("", !0) : (g(), f(l, { key: 1 }, [s.minimapMerged ? (g(), f("path", {
					key: 0,
					d: `M${V.value.fullSet}`,
					stroke: `${s.minimapLineColor}`,
					fill: "none",
					"stroke-width": "1",
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					style: { opacity: "0.6" }
				}, null, 8, Ne)) : d("", !0), s.minimapMerged && !s.minimapCompact ? (g(), f("path", {
					key: 1,
					d: `M${B.value / 2},${Math.max(F.value.height, 0)} ${V.value.fullSet} L${F.value.width - B.value / 2},${Math.max(F.value.height, 0)}Z`,
					fill: `url(#${Ft.value})`,
					stroke: "none",
					style: { opacity: "0.6" }
				}, null, 8, Pe)) : s.minimapMerged ? d("", !0) : (g(), f(l, { key: 2 }, [
					(g(!0), f(l, null, v(H.value.filter((e) => e.type === "bar" && e.isVisible), (e, t) => (g(), f("g", null, [(g(!0), f(l, null, v(e.points, (n, r) => (g(), f(l, null, [e && !isNaN(n.y) ? (g(), f("rect", {
						key: 0,
						x: yn(n.x, t, r),
						y: n.v >= 0 ? n.y : n.y0,
						width: bn(t, r),
						height: n.v >= 0 ? n.y0 - n.y : n.y - n.y0,
						fill: e.color,
						style: { opacity: .6 }
					}, null, 8, Fe)) : d("", !0)], 64))), 256))]))), 256)),
					(g(!0), f(l, null, v(H.value.filter((e) => e.type === "line"), (e) => (g(), f("g", null, [e.isVisible ? (g(), f("path", {
						key: 0,
						d: `M ${e.fullSet}`,
						fill: "none",
						stroke: e.color,
						style: { opacity: "0.6" },
						"stroke-dasharray": e.dashed ? "2 4" : 0
					}, null, 8, Ie)) : d("", !0), e.isVisible && s.cutNullValues ? (g(!0), f(l, { key: 1 }, v(e.fullMarkers, (t) => (g(), f("circle", {
						key: `sel-dot-under-${e.key}-${t.i}`,
						cx: t.x,
						cy: t.y,
						r: "2",
						fill: e.color,
						stroke: N.value,
						"stroke-width": "0.5",
						style: { opacity: "0.6" }
					}, null, 8, Le))), 128)) : d("", !0)]))), 256)),
					(g(!0), f(l, null, v(H.value.filter((e) => e.type === "plot"), (e) => (g(), f("g", null, [(g(!0), f(l, null, v(e.points, (t) => (g(), f("g", null, [e.isVisible && t.value !== null ? (g(), f("circle", {
						key: `sel-plot-under-${e.key}-${t.i}`,
						cx: t.x,
						cy: t.y,
						r: "2",
						fill: e.color,
						stroke: N.value,
						"stroke-width": "0.5",
						style: { opacity: "0.6" }
					}, null, 8, Re)) : d("", !0)]))), 256))]))), 256))
				], 64))], 64)),
				p("rect", {
					x: U.value.x,
					y: 0,
					width: Math.max(0, U.value.width),
					height: Math.max(F.value.height, 0),
					fill: N.value,
					rx: s.minimapSelectionRadius,
					stroke: "none"
				}, null, 8, ze),
				p("rect", {
					x: U.value.x,
					y: 0,
					width: U.value.width < 0 ? 0 : U.value.width,
					height: Math.max(F.value.height, 0),
					rx: s.minimapSelectionRadius,
					fill: N.value,
					style: h({ opacity: G.value || w.value ? 0 : 1 })
				}, null, 12, Be),
				p("rect", {
					x: U.value.x,
					y: 0,
					width: U.value.width < 0 ? 0 : U.value.width,
					height: Math.max(F.value.height, 0),
					fill: s.minimapSelectedColor,
					rx: s.minimapSelectionRadius,
					style: h({ opacity: s.minimapSelectedColorOpacity }),
					tabindex: "0",
					role: "slider",
					"aria-label": "Selected range",
					"aria-valuemin": Number(y.min),
					"aria-valuemax": Number(y.max),
					"aria-valuenow": Number(x.value),
					"aria-valuetext": Q.value.left && Q.value.right ? `${Q.value.left} – ${Q.value.right}` : void 0,
					onKeydown: Fr
				}, null, 44, Ve),
				!s.minimapMerged && On.value < 0 ? (g(), f("line", {
					key: 2,
					class: "slicer-minimap-zero-line",
					x1: 0,
					x2: F.value.width,
					y1: An.value,
					y2: An.value,
					stroke: s.minimapFrameColor,
					"stroke-width": "0.5"
				}, null, 8, He)) : d("", !0),
				e.$slots.slotMap ? d("", !0) : (g(), f(l, { key: 3 }, [s.minimapMerged ? (g(), f("g", Ue, [
					V.value && V.value.sliced && V.value.sliced.length ? (g(), f(l, { key: 0 }, [V.value.selectionSet ? (g(), f("path", {
						key: 0,
						d: `M ${V.value.selectionSet}`,
						stroke: `${s.minimapLineColor}`,
						fill: "transparent",
						"stroke-width": "2",
						"stroke-linecap": "round",
						"stroke-linejoin": "round"
					}, null, 8, We)) : d("", !0)], 64)) : d("", !0),
					V.value && V.value.firstPlot ? (g(), f("circle", {
						key: 1,
						cx: V.value.firstPlot.x,
						cy: V.value.firstPlot.y,
						"stroke-width": "0.5",
						stroke: N.value,
						r: "3",
						fill: s.minimapLineColor
					}, null, 8, Ge)) : d("", !0),
					V.value && V.value.lastPlot ? (g(), f("circle", {
						key: 2,
						cx: V.value.lastPlot.x,
						cy: V.value.lastPlot.y,
						"stroke-width": "0.5",
						stroke: N.value,
						r: "3",
						fill: s.minimapLineColor
					}, null, 8, Ke)) : d("", !0)
				])) : (g(), f("g", qe, [
					(g(!0), f(l, null, v(H.value.filter((e) => e.type === "bar" && e.isVisible), (e, t) => (g(), f("g", null, [(g(!0), f(l, null, v(e.points, (n, r) => (g(), f(l, null, [e && !isNaN(n.y) ? (g(), f("rect", {
						key: 0,
						x: yn(n.x, t, r),
						y: n.v >= 0 ? n.y : n.y0,
						width: bn(t, r),
						height: n.v >= 0 ? n.y0 - n.y : n.y - n.y0,
						fill: e.color,
						style: h({ opacity: +(r >= O.value && r < k.value) })
					}, null, 12, Je)) : d("", !0)], 64))), 256))]))), 256)),
					(g(!0), f(l, null, v(H.value.filter((e) => e.type === "line"), (e, t) => (g(), f("g", { key: String(e.key) }, [e && e.hasSelection && e.selectionSet && e.isVisible ? (g(), f("path", {
						key: 0,
						d: `M ${e.selectionSet}`,
						stroke: e.temperatureColors ? `url(#temperature_grad_line_${t}_${s.uuid})` : e.color,
						fill: "transparent",
						"stroke-width": "2",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						"stroke-dasharray": e.dashed ? "2 4" : 0
					}, null, 8, Ye)) : d("", !0)]))), 128)),
					(g(!0), f(l, null, v(H.value.filter((e) => e.type === "plot"), (e) => (g(), f("g", { key: String(e.key) }, [(g(!0), f(l, null, v(e.sliced, (t) => (g(), f("g", null, [e.isVisible && t.value !== null ? (g(), f("circle", {
						key: 0,
						cx: t.x,
						cy: t.y,
						r: "2",
						fill: e.color,
						stroke: N.value,
						"stroke-width": "0.5",
						style: { opacity: "0.6" }
					}, null, 8, Xe)) : d("", !0)]))), 256))]))), 128))
				]))], 64)),
				e.$slots.slotMap ? ge(e.$slots, "slotMap", fe(le({
					width: Math.max(0, F.value.width),
					height: Math.max(0, F.value.height),
					zeroY: An.value,
					unitW: Math.max(0, B.value)
				})), void 0, !0, 4) : d("", !0),
				W.value !== null && !X.value ? (g(), f("line", fe(ue({ key: 5 }, Er.value)), null, 16)) : d("", !0),
				p("rect", {
					x: 0,
					y: 0,
					width: Math.max(0, F.value.width),
					height: Math.max(0, F.value.height),
					fill: "transparent",
					style: h([{ "pointer-events": "all !important" }, { cursor: s.enableSelectionDrag ? X.value ? "grabbing" : "grab" : "default" }]),
					onMousedown: t[0] ||= (e) => X.value = !0,
					onMouseup: t[1] ||= (e) => X.value = !1,
					onMousemove: Vn,
					onMouseleave: Hn
				}, null, 44, Ze),
				C.value && s.minimapCompact ? (g(), f(l, { key: 6 }, [
					p("rect", {
						class: "vue-ui-zoom-compact-minimap-handle",
						x: Yr.value,
						y: 0,
						width: $.value,
						height: F.value.height,
						fill: s.handleFill || N.value,
						stroke: s.handleBorderColor || s.textColor,
						"stroke-width": s.handleBorderWidth,
						rx: 3
					}, null, 8, Qe),
					s.handleType && s.handleType !== "empty" ? (g(), f("g", {
						key: 0,
						class: "compact-handle-icon",
						transform: `translate(${Yr.value}, 0)`,
						style: { "pointer-events": "none" }
					}, [(g(), f("svg", {
						width: $.value,
						height: F.value.height,
						viewBox: "0 0 20 20",
						preserveAspectRatio: "xMidYMid meet"
					}, [s.handleType === "arrow" ? (g(), f("path", {
						key: 0,
						d: "M 7 7 L 3 10 L 7 13 L 7 7 M 13 7 L 17 10 L 13 13 L 13 7",
						fill: N.value,
						stroke: s.handleIconColor || s.textColor,
						"stroke-width": .618,
						"stroke-linejoin": "round",
						"stroke-linecap": "round"
					}, null, 8, tt)) : s.handleType === "chevron" ? (g(), f("path", {
						key: 1,
						d: "M 6 7 L 4 10 L 6 13 M 14 7 L 16 10 L 14 13",
						fill: "none",
						stroke: s.handleIconColor || s.textColor,
						"stroke-width": .618,
						"stroke-linejoin": "round",
						"stroke-linecap": "round"
					}, null, 8, nt)) : s.handleType === "grab" ? (g(), f("path", {
						key: 2,
						d: "M 8 5 A 1 1 0 0 0 8 7 A 1 1 0 0 0 8 5 M 8 9 A 1 1 0 0 0 8 11 A 1 1 0 0 0 8 9 M 8 13 A 1 1 0 0 0 8 15 A 1 1 0 0 0 8 13 M 12 5 A 1 1 0 0 0 12 7 A 1 1 0 0 0 12 5 M 12 9 A 1 1 0 0 0 12 11 A 1 1 0 0 0 12 9 M 12 13 A 1 1 0 0 0 12 15 A 1 1 0 0 0 12 13",
						fill: s.handleIconColor || s.textColor,
						stroke: "none",
						opacity: "0.6"
					}, null, 8, rt)) : d("", !0)], 8, et))], 8, $e)) : d("", !0),
					p("rect", {
						class: "vue-ui-zoom-compact-minimap-handle",
						x: Xr.value,
						y: 0,
						width: $.value,
						height: F.value.height,
						fill: s.handleFill || N.value,
						stroke: s.handleBorderColor || s.textColor,
						"stroke-width": s.handleBorderWidth,
						rx: 3
					}, null, 8, it),
					s.handleType && s.handleType !== "empty" ? (g(), f("g", {
						key: 1,
						class: "compact-handle-icon",
						transform: `translate(${Xr.value}, 0)`,
						style: { "pointer-events": "none" }
					}, [(g(), f("svg", {
						width: $.value,
						height: F.value.height,
						viewBox: "0 0 20 20",
						preserveAspectRatio: "xMidYMid meet"
					}, [s.handleType === "arrow" ? (g(), f("path", {
						key: 0,
						d: "M 7 7 L 3 10 L 7 13 L 7 7 M 13 7 L 17 10 L 13 13 L 13 7",
						fill: N.value,
						stroke: s.handleIconColor || s.textColor,
						"stroke-width": .618,
						"stroke-linejoin": "round",
						"stroke-linecap": "round"
					}, null, 8, st)) : s.handleType === "chevron" ? (g(), f("path", {
						key: 1,
						d: "M 6 7 L 4 10 L 6 13 M 14 7 L 16 10 L 14 13",
						fill: "none",
						stroke: s.handleIconColor || s.textColor,
						"stroke-width": .618,
						"stroke-linejoin": "round",
						"stroke-linecap": "round"
					}, null, 8, ct)) : s.handleType === "grab" ? (g(), f("path", {
						key: 2,
						d: "M 8 5 A 1 1 0 0 0 8 7 A 1 1 0 0 0 8 5 M 8 9 A 1 1 0 0 0 8 11 A 1 1 0 0 0 8 9 M 8 13 A 1 1 0 0 0 8 15 A 1 1 0 0 0 8 13 M 12 5 A 1 1 0 0 0 12 7 A 1 1 0 0 0 12 5 M 12 9 A 1 1 0 0 0 12 11 A 1 1 0 0 0 12 9 M 12 13 A 1 1 0 0 0 12 15 A 1 1 0 0 0 12 13",
						fill: s.handleIconColor || s.textColor,
						stroke: "none",
						opacity: "0.6"
					}, null, 8, lt)) : d("", !0)], 8, ot))], 8, at)) : d("", !0)
				], 64)) : d("", !0),
				e.$slots.slotMap ? d("", !0) : (g(), f(l, { key: 7 }, [s.minimapMerged ? (g(), f("g", ut, [
					V.value && V.value.firstPlot && V.value.firstPlot.value !== null ? (g(), f("circle", {
						key: 0,
						cx: V.value.firstPlot.x,
						cy: V.value.firstPlot.y,
						"stroke-width": "0.5",
						stroke: N.value,
						r: "4",
						fill: s.minimapLineColor
					}, null, 8, dt)) : d("", !0),
					V.value && V.value.firstPlot && V.value.firstPlot.value !== null ? (g(), f("circle", {
						key: 1,
						cx: V.value.firstPlot.x,
						cy: V.value.firstPlot.y,
						r: 2,
						fill: N.value
					}, null, 8, ft)) : d("", !0),
					V.value && V.value.lastPlot && V.value.lastPlot.value !== null ? (g(), f("circle", {
						key: 2,
						cx: V.value.lastPlot.x,
						cy: V.value.lastPlot.y,
						"stroke-width": "0.5",
						stroke: N.value,
						r: "4",
						fill: s.minimapLineColor
					}, null, 8, pt)) : d("", !0),
					V.value && V.value.lastPlot && V.value.lastPlot.value !== null ? (g(), f("circle", {
						key: 3,
						cx: V.value.lastPlot.x,
						cy: V.value.lastPlot.y,
						r: "2",
						fill: N.value
					}, null, 8, mt)) : d("", !0)
				])) : (g(), f("g", ht, [(g(!0), f(l, null, v(H.value.filter((e) => e.type === "line"), (e, t) => (g(), f("g", { key: String(e.key) }, [
					e && e.hasSelection && e.selectionSet && e.isVisible ? (g(), f("path", {
						key: 0,
						d: `M ${e.selectionSet}`,
						stroke: e.temperatureColors ? `url(#temperature_grad_line_${t}_${s.uuid})` : e.color,
						fill: "transparent",
						"stroke-width": "2",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						"stroke-dasharray": e.dashed ? "2 4" : 0
					}, null, 8, gt)) : d("", !0),
					e.isVisible && s.cutNullValues ? (g(!0), f(l, { key: 1 }, v(e.selectionMarkers, (t) => (g(), f("circle", {
						key: `sel-dot-${e.key}-${t.i}`,
						cx: t.x,
						cy: t.y,
						r: "2.5",
						fill: e.color,
						stroke: N.value,
						"stroke-width": "0.5"
					}, null, 8, _t))), 128)) : d("", !0),
					e && e.firstPlot && e.isVisible && e.firstPlot.value !== null ? (g(), f("circle", {
						key: 2,
						cx: e.firstPlot.x,
						cy: e.firstPlot.y,
						"stroke-width": "0.5",
						stroke: N.value,
						r: "4",
						fill: e.color
					}, null, 8, vt)) : d("", !0),
					e && e.firstPlot && e.isVisible && e.firstPlot.value !== null ? (g(), f("circle", {
						key: 3,
						cx: e.firstPlot.x,
						cy: e.firstPlot.y,
						r: "2",
						fill: N.value
					}, null, 8, yt)) : d("", !0),
					e && e.lastPlot && e.isVisible && e.lastPlot.value !== null ? (g(), f("circle", {
						key: 4,
						cx: e.lastPlot.x,
						cy: e.lastPlot.y,
						"stroke-width": "0.5",
						stroke: N.value,
						r: "4",
						fill: e.color
					}, null, 8, bt)) : d("", !0),
					e && e.lastPlot && e.isVisible && e.lastPlot.value !== null ? (g(), f("circle", {
						key: 5,
						cx: e.lastPlot.x,
						cy: e.lastPlot.y,
						r: "2",
						fill: N.value
					}, null, 8, xt)) : d("", !0)
				]))), 128)), (g(!0), f(l, null, v(H.value.filter((e) => e.type === "plot"), (e) => (g(), f("g", { key: String(e.key) }, [(g(!0), f(l, null, v(e.points, (t) => (g(), f("g", null, [e.isVisible && s.cutNullValues && t.value !== null ? (g(), f("circle", {
					key: `sel-plot-${e.key}-${t.i}`,
					cx: t.x,
					cy: t.y,
					r: "2.5",
					fill: e.color,
					stroke: N.value,
					"stroke-width": "0.5"
				}, null, 8, St)) : d("", !0)]))), 256))]))), 128))]))], 64))
			], 8, ke))])) : d("", !0),
			p("div", {
				class: "slider-track",
				style: h({ visibility: C.value && s.minimapCompact ? "hidden" : "visible" })
			}, null, 4),
			p("div", {
				class: m({
					"range-highlight": !0,
					move: s.enableSelectionDrag
				}),
				onMousedown: t[2] ||= (e) => X.value = !0,
				onMouseup: t[3] ||= (e) => X.value = !1,
				style: h({
					...M.value,
					cursor: X.value ? "grabbing" : "grab",
					visibility: C.value && s.minimapCompact ? "hidden" : "visible"
				})
			}, null, 38),
			s.enableRangeHandles ? Se((g(), f("input", {
				key: 1,
				"aria-label": "range-handle-left",
				ref_key: "rangeStart",
				ref: Un,
				type: "range",
				step: s.precision > 0 ? 1 / 10 ** s.precision : 1,
				class: m({
					"range-left": !0,
					"range-handle": !0,
					"range-minimap": C.value && s.verticalHandles,
					"range-invisible": C.value && s.minimapCompact
				}),
				min: s.min,
				max: s.minimapCompact && C.value && !s.useValueRange ? Math.max(0, L.value - 1) : s.max,
				tabindex: C.value ? -1 : 0,
				"onUpdate:modelValue": t[4] ||= (e) => Pn.value = e,
				onFocus: t[5] ||= (e) => C.value && e.target.blur(),
				onInput: t[6] ||= (e) => Pn.value = e.target.valueAsNumber,
				onChange: A,
				onKeyup: Ce(A, ["enter"]),
				onBlur: A,
				onMouseenter: t[7] ||= (e) => Cr("start"),
				onPointerup: A
			}, null, 42, Ct)), [[
				be,
				Pn.value,
				void 0,
				{ number: !0 }
			]]) : d("", !0),
			s.enableRangeHandles ? Se((g(), f("input", {
				key: 2,
				"aria-label": "range-handle-right",
				ref_key: "rangeEnd",
				ref: Wn,
				type: "range",
				step: s.precision > 0 ? 1 / 10 ** s.precision : 1,
				class: m({
					"range-right": !0,
					"range-handle": !0,
					"range-minimap": C.value && s.verticalHandles,
					"range-invisible": C.value && s.minimapCompact
				}),
				min: s.min,
				max: s.minimapCompact && C.value && !s.useValueRange ? Math.max(0, L.value - 1) : s.max,
				tabindex: C.value ? -1 : 0,
				onFocus: t[8] ||= (e) => C.value && e.target.blur(),
				"onUpdate:modelValue": t[9] ||= (e) => Fn.value = e,
				onInput: t[10] ||= (e) => Fn.value = e.target.valueAsNumber,
				onChange: A,
				onKeyup: Ce(A, ["enter"]),
				onBlur: A,
				onMouseenter: t[11] ||= (e) => Cr("end"),
				onPointerup: A
			}, null, 42, wt)), [[
				be,
				Fn.value,
				void 0,
				{ number: !0 }
			]]) : d("", !0),
			C.value && s.minimapCompact ? (g(), f("div", Tt, [(g(), f("svg", {
				xmlns: ve(ne),
				viewBox: `0 0 ${Math.max(0, F.value.width)} ${Math.max(0, F.value.height + 1)}`
			}, [p("rect", ue({
				class: "vue-ui-zoom-compact-minimap-handle",
				x: Yr.value,
				y: 0,
				width: $.value,
				height: F.value.height,
				fill: "transparent",
				stroke: "none"
			}, zr.value, {
				onKeydown: t[12] ||= (e) => Rr("start", e),
				onMousedown: t[13] ||= we((e) => Kr("start", e), ["stop", "prevent"]),
				onClick: t[14] ||= we(() => {}, ["stop", "prevent"])
			}), null, 16, Dt), p("rect", ue({
				class: "vue-ui-zoom-compact-minimap-handle",
				x: Xr.value,
				y: 0,
				width: $.value,
				height: F.value.height,
				fill: "transparent",
				stroke: "none",
				rx: 3
			}, Br.value, {
				onKeydown: t[15] ||= (e) => Rr("end", e),
				onMousedown: t[16] ||= we((e) => Kr("end", e), ["stop", "prevent"]),
				onClick: t[17] ||= we(() => {}, ["stop", "prevent"])
			}), null, 16, Ot)], 8, Et))])) : d("", !0),
			Q.value.left ? (g(), f("div", {
				key: 4,
				ref_key: "tooltipLeft",
				ref: gr,
				class: m({
					"range-tooltip": !0,
					"range-tooltip-visible": Z.value,
					"range-tooltip-arrow": M.value.arrowLeft && !s.verticalHandles,
					"range-tooltip-arrow-left": !M.value.arrowLeft && !s.verticalHandles
				}),
				style: h({
					left: M.value.tooltipLeft,
					color: ve(te)(s.selectColor),
					backgroundColor: s.selectColor,
					border: `1px solid ${N.value}`,
					zIndex: `${Sr.value + 4}`,
					visibility: wr.value || Q.value.left === Q.value.right ? "hidden" : "visible",
					top: C.value && s.minimapCompact ? "calc(-100% - 12px)" : "-100%"
				})
			}, _e(Q.value.left), 7)) : d("", !0),
			(wr.value || Q.value.left === Q.value.right) && (Q.value.left || Q.value.right) ? (g(), f("div", {
				key: 5,
				ref: "tooltipMerge",
				class: m({
					"range-tooltip": !0,
					"range-tooltip-visible": Z.value,
					"range-tooltip-arrow": !0,
					"range-tooltip-arrow-left": !M.value.arrowLeft && !s.verticalHandles,
					"range-tooltip-arrow-right": !M.value.arrowRight && !s.verticalHandles
				}),
				style: h({
					left: M.value.tooltipCenter,
					width: Tr.value.width + "px",
					color: ve(te)(s.selectColor),
					backgroundColor: s.selectColor,
					border: `1px solid ${N.value}`,
					zIndex: "4",
					top: C.value && s.minimapCompact ? "calc(-100% - 12px)" : "-100%"
				})
			}, _e(Q.value.left === Q.value.right ? Q.value.left : `${Q.value.left} - ${Q.value.right}`), 7)) : d("", !0),
			Q.value.right ? (g(), f("div", {
				key: 6,
				ref_key: "tooltipRight",
				ref: _r,
				class: m({
					"range-tooltip": !0,
					"range-tooltip-visible": Z.value,
					"range-tooltip-arrow": M.value.arrowRight && !s.verticalHandles,
					"range-tooltip-arrow-right": !M.value.arrowRight && !s.verticalHandles
				}),
				style: h({
					left: M.value.tooltipRight,
					color: ve(te)(s.selectColor),
					backgroundColor: s.selectColor,
					border: `1px solid ${N.value}`,
					zIndex: "4",
					visibility: wr.value || Q.value.left === Q.value.right ? "hidden" : "visible",
					top: C.value && s.minimapCompact ? "calc(-100% - 12px)" : "-100%"
				})
			}, _e(Q.value.right), 7)) : d("", !0)
		], 36)], 44, Te));
	}
}, [["__scopeId", "data-v-023703bd"]]);
//#endregion
export { At as t };
