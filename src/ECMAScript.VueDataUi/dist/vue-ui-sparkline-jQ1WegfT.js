import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { A as t, Bt as n, Dt as r, H as i, I as a, Jt as o, L as s, N as c, P as l, S as u, U as d, V as ee, Vt as f, X as p, bt as te, d as ne, i as re, it as ie, jt as ae, k as oe, m as se, ot as ce, pt as le, q as ue, t as de, tt as fe, zt as pe } from "./lib-Bttd6u5E.js";
import { n as me, t as he } from "./useHints-Dq_w2E8B.js";
import { t as m } from "./useTimeLabels-d2f-W1L4.js";
import { t as ge } from "./useConfig-DlNpz6P8.js";
import { n as _e, t as ve } from "./BaseScanner-DZvpgOjM.js";
import { t as ye } from "./useNestedProp-vPNvh7rV.js";
import { t as be } from "./useThemeCheck-C43Tcqmk.js";
import { t as xe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Se, t as Ce } from "./useResponsive-ZtArZtUf.js";
import { t as we } from "./DefGrad-DVBqDjhO.js";
import { t as Te } from "./A11yDataTable-DdRsVULz.js";
import { t as Ee } from "./useChartAccessibility-DYqac8yF.js";
import { t as De } from "./usePrefersMotion-BC-CsqR1.js";
import { t as Oe } from "./vue_ui_sparkline-auZhap6Y.js";
import { Fragment as h, computed as g, createBlock as ke, createCommentVNode as _, createElementBlock as v, createElementVNode as y, createTextVNode as Ae, createVNode as je, defineAsyncComponent as Me, guardReactiveProps as Ne, nextTick as b, normalizeProps as Pe, normalizeStyle as x, onBeforeUnmount as Fe, onMounted as Ie, openBlock as S, ref as C, renderList as w, renderSlot as T, shallowRef as Le, toDisplayString as Re, toRefs as ze, unref as E, watch as D, watchEffect as Be, withCtx as Ve } from "vue";
//#region src/atoms/SparklinePulse.vue
var He = {
	key: 0,
	style: { "pointer-events": "none" }
}, Ue = [
	"r",
	"fill",
	"filter"
], We = [
	"begin",
	"dur",
	"repeatCount",
	"fill",
	"calcMode",
	"keySplines",
	"keyTimes",
	"keyPoints"
], Ge = ["href"], Ke = [
	"dur",
	"repeatCount",
	"fill",
	"values"
], qe = ["r", "fill"], O = [
	"begin",
	"dur",
	"repeatCount",
	"calcMode",
	"keySplines",
	"keyTimes",
	"keyPoints"
], Je = ["href"], k = ["dur", "repeatCount"], Ye = {
	__name: "SparklinePulse",
	props: {
		uid: {
			type: String,
			required: !0
		},
		svgRef: {
			type: Object,
			default: null
		},
		pulsePathId: {
			type: String,
			required: !0
		},
		pulsePathLength: {
			type: Number,
			required: !0
		},
		pulseDur: {
			type: String,
			required: !0
		},
		pulseBegin: {
			type: String,
			required: !0
		},
		pulseRepeatCount: {
			type: String,
			required: !0
		},
		pulseFillMode: {
			type: String,
			default: void 0
		},
		pulseKeyPoints: {
			type: String,
			required: !0
		},
		pulseMotion: {
			type: Object,
			required: !0
		},
		pulse: {
			type: Object,
			required: !0
		},
		pulseTrail: {
			type: Object,
			required: !0
		},
		pulseTrailLength: {
			type: Number,
			required: !0
		},
		prefersReducedMotion: {
			type: Boolean,
			required: !0
		},
		loading: {
			type: Boolean,
			required: !0
		},
		isBar: {
			type: Boolean,
			required: !0
		}
	},
	setup(e) {
		let t = e, n = g(() => Math.min(t.pulseTrailLength, 45)), r = g(() => !!t.pulse?.show && !t.isBar && !t.prefersReducedMotion && !t.loading && Number(t.pulsePathLength) > 0);
		function i() {
			let e = t.svgRef?.value;
			e && (typeof e.pauseAnimations == "function" && e.pauseAnimations(), typeof e.setCurrentTime == "function" && e.setCurrentTime(0), typeof e.unpauseAnimations == "function" && e.unpauseAnimations());
		}
		function a(e, t) {
			if (typeof e != "string") return e;
			let n = e.trim().match(/^([\d.]+)\s*(ms|s)$/);
			if (!n) return e;
			let r = Number(n[1]), i = n[2];
			return Number.isNaN(r) ? e : `${r + t}${i}`;
		}
		function o(e) {
			return (n.value - e) / n.value * t.pulse.radius;
		}
		function s(e) {
			let r = t.pulse.trail.opacity, i = (n.value - e) / n.value, a = e === 0 ? 1 : i * r;
			return `0;${a};${a};0`;
		}
		return D(() => t.loading, async (e) => {
			if (e) {
				i();
				return;
			}
			await b(), i();
		}), D(() => t.pulsePathId, async () => {
			await b(), i();
		}), Fe(() => {
			i();
		}), (t, i) => r.value ? (S(), v("g", He, [(S(!0), v(h, null, w(n.value, (t, n) => (S(), v(h, null, [n % 3 == 0 ? (S(), v("circle", {
			key: `sparkline_dot_${n}_${e.pulsePathId}`,
			r: o(n),
			fill: e.pulse.color,
			filter: `url(#sparkline_pulse_glow_${e.uid})`,
			opacity: "0"
		}, [y("animateMotion", {
			begin: a(e.pulseBegin, n * 10),
			dur: e.pulseDur,
			repeatCount: e.pulseRepeatCount,
			fill: e.pulseFillMode,
			calcMode: e.pulseMotion.calcMode,
			keySplines: e.pulseMotion.keySplines || void 0,
			keyTimes: e.pulseMotion.keyTimes || void 0,
			keyPoints: e.pulseKeyPoints,
			rotate: "auto"
		}, [y("mpath", { href: `#${e.pulsePathId}` }, null, 8, Ge)], 8, We), y("animate", {
			attributeName: "opacity",
			dur: e.pulseDur,
			repeatCount: e.pulseRepeatCount,
			fill: e.pulseFillMode,
			values: s(n),
			keyTimes: "0;0.1;0.9;1"
		}, null, 8, Ke)], 8, Ue)) : _("", !0)], 64))), 256)), (S(), v("circle", {
			key: `sparkline_halo_${e.pulsePathId}`,
			r: Math.max(e.pulse.radius * 1.3),
			fill: e.pulse.color,
			opacity: "0"
		}, [y("animateMotion", {
			begin: e.pulseBegin,
			dur: e.pulseDur,
			repeatCount: e.pulseRepeatCount,
			calcMode: e.pulseMotion.calcMode,
			keySplines: e.pulseMotion.keySplines || void 0,
			keyTimes: e.pulseMotion.keyTimes || void 0,
			keyPoints: e.pulseKeyPoints,
			rotate: "auto"
		}, [y("mpath", { href: `#${e.pulsePathId}` }, null, 8, Je)], 8, O), y("animate", {
			attributeName: "opacity",
			values: "0;0.35;0.35;0",
			keyTimes: "0;0.15;0.85;1",
			dur: e.pulseDur,
			repeatCount: e.pulseRepeatCount
		}, null, 8, k)], 8, qe))])) : _("", !0);
	}
}, A = {
	canvas: null,
	ctx: null
};
function Xe() {
	if (typeof document > "u") throw Error("color-utils: document is not available (browser-only).");
	if (!A.canvas) {
		let e = document.createElement("canvas");
		e.width = 1, e.height = 1;
		let t = e.getContext("2d", { willReadFrequently: !0 });
		if (!t) throw Error("color-utils: unable to get 2D canvas context.");
		A.canvas = e, A.ctx = t;
	}
	return A.ctx;
}
function Ze(e) {
	return e < 0 ? 0 : e > 255 ? 255 : Math.round(e);
}
function j(e) {
	return e < 0 ? 0 : e > 1 ? 1 : e;
}
function M(e, t = null) {
	if (typeof e != "string" || e.trim().length === 0) throw Error("colorToRgba: inputColor must be a non-empty string.");
	let n = e.trim();
	if (n.toLowerCase() === "transparent") return {
		red: 0,
		green: 0,
		blue: 0,
		alpha: 0
	};
	let r = Qe(n, t), i = Xe();
	i.clearRect(0, 0, 1, 1), i.fillStyle = "#000";
	let a = i.fillStyle;
	if (i.fillStyle = r, i.fillStyle === a && !$e(r)) throw Error(`colorToRgba: unsupported or invalid color "${e}".`);
	i.fillRect(0, 0, 1, 1);
	let o = i.getImageData(0, 0, 1, 1).data;
	return {
		red: o[0],
		green: o[1],
		blue: o[2],
		alpha: o[3] / 255
	};
}
function N(e, t = null) {
	if (!Array.isArray(e) || e.length === 0) throw Error("colorsToRgba: colors must be a non-empty array.");
	let n = Array(e.length);
	for (let r = 0; r < e.length; r += 1) n[r] = M(e[r], t);
	return n;
}
function Qe(e, t = null) {
	return e !== "currentColor" || typeof window > "u" || !t ? e : window.getComputedStyle(t).color || e;
}
function $e(e) {
	let t = e.trim().toLowerCase();
	return t === "black" || t === "#000" || t === "#000000" || t === "rgb(0, 0, 0)" || t === "rgba(0, 0, 0, 1)" || t === "rgba(0, 0, 0, 0)";
}
function P(e) {
	let t = j(e / 255);
	return t <= .04045 ? t / 12.92 : ((t + .055) / 1.055) ** 2.4;
}
function F(e) {
	let t = j(e), n;
	return n = t <= .0031308 ? t * 12.92 : 1.055 * t ** (1 / 2.4) - .055, Ze(n * 255);
}
function et(e, t, n, r) {
	let i = j(n);
	if (r === "linearRGB") {
		let n = P(e.red), r = P(e.green), a = P(e.blue), o = P(t.red), s = P(t.green), c = P(t.blue), l = n + (o - n) * i, u = r + (s - r) * i, d = a + (c - a) * i;
		return {
			red: F(l),
			green: F(u),
			blue: F(d),
			alpha: e.alpha + (t.alpha - e.alpha) * i
		};
	}
	return {
		red: e.red + (t.red - e.red) * i,
		green: e.green + (t.green - e.green) * i,
		blue: e.blue + (t.blue - e.blue) * i,
		alpha: e.alpha + (t.alpha - e.alpha) * i
	};
}
//#endregion
//#region src/SPG/number-utils.js
function I(e, t, n) {
	let r = e;
	return r < t && (r = t), r > n && (r = n), r;
}
function L(e, t) {
	let n = 10 ** t;
	return Math.round(e * n) / n;
}
function R(e) {
	return Number.isFinite(e) ? e.toString() : "0";
}
function tt(e) {
	let t = I(e, 0, 255).toString(16);
	return t.length === 1 ? "0" + t : t;
}
function nt(e) {
	if (typeof e != "string") return;
	let t = Number.parseFloat(e);
	return Number.isFinite(t) ? t : void 0;
}
function rt(e, t, n, r = !0) {
	let i = n - t;
	if (i === 0) return 0;
	let a = (e - t) / i;
	return r ? I(a, 0, 1) : a;
}
//#endregion
//#region src/SPG/index.js
var it = "http://www.w3.org/2000/svg";
function at(e) {
	if (typeof document > "u") throw Error("SvgPathGradientAsync: document is not available (browser-only implementation).");
	let t = document.createElementNS(it, "svg");
	t.setAttribute("width", "0"), t.setAttribute("height", "0"), t.setAttribute("viewBox", "0 0 0 0"), t.style.position = "absolute", t.style.left = "-10000px", t.style.top = "-10000px", t.style.visibility = "hidden", t.style.pointerEvents = "none";
	let n = document.createElementNS(it, "path");
	return e === "M " && (e = "M 0,0"), n.setAttribute("d", e), t.appendChild(n), document.body.appendChild(t), {
		svgElement: t,
		pathElement: n
	};
}
function ot(e) {
	if (e) try {
		e.svgElement?.remove?.();
	} catch {}
}
function st(e, t, n) {
	if (typeof n == "number" && n > 0) return n;
	let r = t * .75;
	r < .5 && (r = .5), r > 8 && (r = 8);
	let i = e / 2;
	return r > i && i > 0 && (r = i), r <= 0 && (r = 1), r;
}
function ct(e, t, n) {
	if (typeof t == "number" && t >= 1) return Math.floor(t);
	let r = Math.ceil(e / (typeof n == "number" && n > 0 ? n : 2));
	return r < 1 ? 1 : r;
}
function lt(e, t, n, r, i, a) {
	let o = n - t, s = Math.ceil(o / r) + 1;
	s < 2 && (s = 2), s > a && (s = a);
	let c = s > 1 ? o / (s - 1) : 0, l = e.getPointAtLength(t), u = `M ${R(L(l.x, i))} ${R(L(l.y, i))}`;
	for (let n = 1; n < s; n += 1) {
		let r = t + n * c, a = e.getPointAtLength(r);
		u += ` L ${R(L(a.x, i))} ${R(L(a.y, i))}`;
	}
	return u;
}
function ut(e, t, n, r, i, a) {
	let o = n - t, s = Math.ceil(o / r) + 1;
	s < 2 && (s = 2), s > a && (s = a);
	let c = s > 1 ? o / (s - 1) : 0, l = e.getPointAtLength(t), u = l.x, d = l.x, ee = l.y, f = l.y, p = `M ${R(L(l.x, i))} ${R(L(l.y, i))}`;
	for (let n = 1; n < s; n += 1) {
		let r = t + n * c, a = e.getPointAtLength(r);
		a.x < u && (u = a.x), a.x > d && (d = a.x), a.y < ee && (ee = a.y), a.y > f && (f = a.y), p += ` L ${R(L(a.x, i))} ${R(L(a.y, i))}`;
	}
	return {
		pathData: p,
		bounds: {
			minX: u,
			maxX: d,
			minY: ee,
			maxY: f
		}
	};
}
function dt(e) {
	if (e.length === 1) return [{
		position: 0,
		rgba: e[0]
	}];
	let t = Array(e.length), n = e.length - 1;
	for (let r = 0; r < e.length; r += 1) t[r] = {
		position: r / n,
		rgba: e[r]
	};
	return t;
}
function z(e, t, n, r) {
	let i = I(t, 0, 1), a = r;
	for (a < 0 && (a = 0), a > e.length - 2 && (a = e.length - 2); a > 0 && i < e[a].position;) --a;
	for (; a < e.length - 2 && i > e[a + 1].position;) a += 1;
	let o = e[a], s = e[a + 1], c = s.position - o.position, l = c > 0 ? (i - o.position) / c : 0;
	return U(B(o.rgba, s.rgba, l, n));
}
function B(e, t, n, r) {
	let i = I(n, 0, 1);
	if (r === "linearRGB") {
		let n = V(e.red), r = V(e.green), a = V(e.blue), o = V(t.red), s = V(t.green), c = V(t.blue), l = n + (o - n) * i, u = r + (s - r) * i, d = a + (c - a) * i;
		return {
			red: H(l),
			green: H(u),
			blue: H(d),
			alpha: e.alpha + (t.alpha - e.alpha) * i
		};
	}
	return {
		red: e.red + (t.red - e.red) * i,
		green: e.green + (t.green - e.green) * i,
		blue: e.blue + (t.blue - e.blue) * i,
		alpha: e.alpha + (t.alpha - e.alpha) * i
	};
}
function V(e) {
	let t = I(e / 255, 0, 1);
	return t <= .04045 ? t / 12.92 : ((t + .055) / 1.055) ** 2.4;
}
function H(e) {
	let t = I(e, 0, 1), n;
	return n = t <= .0031308 ? t * 12.92 : 1.055 * t ** (1 / 2.4) - .055, I(Math.round(n * 255), 0, 255);
}
function U(e) {
	let t = I(e.alpha, 0, 1), n = I(Math.round(e.red), 0, 255), r = I(Math.round(e.green), 0, 255), i = I(Math.round(e.blue), 0, 255);
	return t >= 1 ? `#${tt(n)}${tt(r)}${tt(i)}` : `rgba(${n}, ${r}, ${i}, ${L(t, 4).toString()})`;
}
function ft(e) {
	let t = `fill="${W(e.fill ?? "none")}"`, n = Object.keys(e);
	for (let r = 0; r < n.length; r += 1) {
		let i = n[r];
		if (i === "stroke" || i === "d") continue;
		let a = e[i];
		t += ` ${i}="${W(a)}"`;
	}
	return t;
}
function pt(e, t, n) {
	return `<path d="${W(e)}" stroke="${W(t)}" ${n} />`;
}
function mt(e) {
	let t = [], n = e.segmentAttributeMap.fill ?? "none";
	t.push(`fill="${W(n)}"`), t.push(`stroke="${W(e.stroke)}"`);
	for (let n of Object.keys(e.segmentAttributeMap)) {
		if (n === "stroke" || n === "d") continue;
		let r = e.segmentAttributeMap[n];
		t.push(`${n}="${W(r)}"`);
	}
	return `<path d="${W(e.pathData)}" ${t.join(" ")} />`;
}
function ht(e, t) {
	let n = [];
	for (let e of Object.keys(t)) n.push(`${e}="${W(t[e])}"`);
	return `<g data-svg-path-gradient="true"${n.length ? " " + n.join(" ") : ""}>${e.join("")}</g>`;
}
function W(e) {
	return String(e).replaceAll("&", "&amp;").replaceAll("\"", "&quot;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
}
function gt(e, t = null, n = {}) {
	return new Promise((r, i) => {
		let a = null;
		try {
			if (!e || typeof e != "string") throw Error("SvgPathGradientAsync: pathData must be a non-empty string.");
			let i = n.temperatureMode ?? null;
			if (i === null && (!Array.isArray(t) || t.length === 0)) throw Error("SvgPathGradientAsync: colors must be a non-empty array when temperatureMode is not enabled.");
			if ((n.returnMode ?? "string") !== "string") throw Error("SvgPathGradientAsync: only returnMode=\"string\" is supported for the async version.");
			let o = typeof n.decimalPlaces == "number" ? n.decimalPlaces : 3, s = typeof n.flattenTolerance == "number" && n.flattenTolerance > 0 ? n.flattenTolerance : .25, c = n.attrs ? { ...n.attrs } : {}, l = n.groupAttrs ? { ...n.groupAttrs } : {}, u = nt(c["stroke-width"]), d = typeof n.strokeWidth == "number" && n.strokeWidth > 0 ? n.strokeWidth : typeof u == "number" && u > 0 ? u : 1;
			a = at(e);
			let ee = a.pathElement, f = ee.getTotalLength(), p = i === null ? N(t, n.colorReferenceElement) : [];
			if (i === null && (!(f > 0) || p.length === 1)) {
				let t = ht([mt({
					pathData: e,
					stroke: U(p[0]),
					segmentAttributeMap: c
				})], l);
				ot(a), r(t);
				return;
			}
			let te = n.colorSpace ?? "linearRGB";
			if (i !== null) {
				let e = n.temperatureColors;
				if (!Array.isArray(e) || e.length !== 2) throw Error("SvgPathGradientAsync: temperatureColors must be a tuple of exactly 2 colors when temperatureMode is enabled.");
			}
			let ne = i === null ? null : N(n.temperatureColors, n.colorReferenceElement), re = i === null ? (() => {
				let e = dt(p), t = Array(e.length);
				for (let n = 0; n < e.length; n += 1) t[n] = {
					position: e[n].position,
					rgba: e[n].rgba
				};
				return t;
			})() : [], ie = st(f, d, n.maxSegmentLength), ae = ct(f, n.segments, ie), oe = f / ae, se = typeof n.overlap == "number" && n.overlap >= 0 ? n.overlap : d * .5, ce = oe * .45;
			se > ce && (se = ce);
			let le = typeof n.samplePointLimitPerSegment == "number" && n.samplePointLimitPerSegment > 10 ? n.samplePointLimitPerSegment : 250, ue = ft(c), de = [], fe = i === null ? null : [], pe = i === null ? null : [], me = 0, he = i === null ? re.length - 2 : 0, m = 0, ge = () => {
				let e = performance.now(), t = typeof n.frameBudgetMs == "number" ? n.frameBudgetMs : 8;
				for (; m < ae && performance.now() - e < t;) {
					let e = m * oe, t = (m + 1) * oe, n = e, r = t;
					if (m !== 0 && (n = e - se), m !== ae - 1 && (r = t + se), n = I(n, 0, f), r = I(r, 0, f), r > n) if (i !== null) {
						let e = ut(ee, n, r, s, o, le);
						fe.push(e.pathData), pe.push(e.bounds);
					} else {
						let e = (n + r) * .5 / f;
						for (; me < he && e > re[me + 1].position;) me += 1;
						let t = z(re, e, te, me), i = lt(ee, n, r, s, o, le);
						de.push(pt(i, t, ue));
					}
					m += 1;
				}
				if (m < ae) {
					requestAnimationFrame(ge);
					return;
				}
				if (i !== null) {
					let e = Infinity, t = -Infinity;
					for (let n = 0; n < pe.length; n += 1) {
						let r = pe[n], a = i === "vertical" ? r.minY : r.minX, o = i === "vertical" ? r.maxY : r.maxX;
						a < e && (e = a), o > t && (t = o);
					}
					let n = ne[0], r = ne[1];
					de.length = 0;
					for (let a = 0; a < pe.length; a += 1) {
						let o = pe[a], s = rt(((i === "vertical" ? o.minY : o.minX) + (i === "vertical" ? o.maxY : o.maxX)) * .5, e, t, !0), c = U(i === "vertical" ? et(n, r, s, te) : et(r, n, s, te)), l = fe[a];
						de.push(pt(l, c, ue));
					}
				}
				let c = ht(de, l);
				ot(a), r(c);
			};
			requestAnimationFrame(ge);
		} catch (e) {
			ot(a), i(e);
		}
	});
}
//#endregion
//#region src/atoms/SparklineGradientPath.vue
var _t = ["innerHTML"], vt = {
	__name: "SparklineGradientPath",
	props: {
		svgPathData: {
			type: String,
			required: !0
		},
		enabled: {
			type: Boolean,
			required: !0
		},
		strokeWidth: {
			type: Number,
			required: !0
		},
		highColor: {
			type: String,
			required: !0
		},
		lowColor: {
			type: String,
			required: !0
		},
		segments: {
			type: Number,
			required: !0
		}
	},
	setup(e) {
		let t = e, n = C(""), r = 0;
		return D(() => [
			t.enabled,
			t.svgPathData,
			t.strokeWidth,
			t.highColor,
			t.lowColor,
			t.segments
		], async ([e]) => {
			let i = ++r;
			if (!e) {
				n.value = "";
				return;
			}
			let a = await gt(t.svgPathData, null, {
				segments: t.segments,
				temperatureMode: "vertical",
				temperatureColors: [t.highColor, t.lowColor],
				attrs: {
					"stroke-width": t.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				},
				groupAttrs: { class: "vue-ui-sparkline-gradient" }
			});
			i === r && (n.value = a);
		}, { immediate: !0 }), (e, t) => (S(), v("g", { innerHTML: n.value }, null, 8, _t));
	}
}, yt = /* @__PURE__ */ e({ default: () => G }), bt = ["id"], xt = ["id"], St = { style: { position: "relative" } }, Ct = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], wt = ["width", "height"], Tt = ["id"], Et = [
	"id",
	"y1",
	"y2"
], Dt = ["stop-color", "offset"], Ot = { key: 1 }, kt = [
	"data-cy",
	"d",
	"fill"
], At = [
	"id",
	"d",
	"stroke-width"
], jt = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Mt = [
	"d",
	"stroke",
	"stroke-width"
], Nt = [
	"id",
	"d",
	"stroke-width"
], Pt = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Ft = [
	"d",
	"stroke",
	"stroke-width"
], It = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], Lt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Rt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-dasharray",
	"stroke-width"
], zt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], Bt = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], Vt = [
	"x",
	"y",
	"height",
	"width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Ht = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, G = /*#__PURE__*/ xe({
	__name: "vue-ui-sparkline",
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
		},
		showInfo: {
			type: Boolean,
			default: !0
		},
		selectedIndex: {
			type: Number,
			default: void 0
		},
		heightRatio: {
			type: Number,
			default: 1
		},
		forcedPadding: {
			type: Number,
			default: 30
		}
	},
	emits: ["hoverIndex", "selectDatapoint"],
	setup(e, { emit: xe }) {
		let He = Me(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ue = Me(() => import("./SparkTooltip-CjsbIhzi.js")), { vue_ui_sparkline: We } = ge(), { isThemeValid: Ge, warnInvalidTheme: Ke } = be(), qe = De(), O = e, Je = g(() => Array.isArray(O.dataset) && O.dataset.length > 0), k = C(ue()), A = C(null), Xe = C(null), Ze = C(null), j = C(null), M = C(null), N = C(L());
		me({
			config: () => N.value,
			dataset: () => O.dataset,
			component: "VueUiSparkline",
			rules: [
				he.emptyArray,
				{
					test: (e) => e.length > 500,
					message: [
						"👀 The dataset has > 500 datapoints. Consider if you really need this level of detail.",
						"",
						"▶️ Use larger time scales, or aggregated values"
					]
				},
				{
					test: (e) => e.length > 1095,
					message: [
						"👀 The dataset has > 1095 datapoints. Above this threshold, the dataset is computed through an LTTB algorithm, to preserve the shape of the data without increasing the number of datapoints.",
						"",
						"▶️ If you need this level of detail, you can change config.downsample.threshold and set a higher value. Note that performance will be impacted."
					]
				}
			]
		});
		function Qe(e) {
			return O.config?.skeletonDataset && Array.isArray(O.config.skeletonDataset) ? O.config.skeletonDataset.map((e) => ({
				period: "-",
				value: e
			})) : ie(e).map((e) => ({
				period: "-",
				value: e
			}));
		}
		let $e = g(() => o({
			defaultConfig: {
				gradientPath: { show: !1 },
				temperatureColors: { show: !1 },
				style: {
					backgroundColor: "#99999930",
					scaleMin: 0,
					scaleMax: null,
					animation: { show: !1 },
					line: {
						color: "#AAAAAA",
						pulse: { show: !1 },
						dashIndices: []
					},
					bar: { color: "#AAAAAA" },
					area: { color: "#CACACA" },
					zeroLine: { color: "#6A6A6A" },
					dataLabel: { show: !1 },
					tooltip: { show: !1 }
				}
			},
			userConfig: N.value.skeletonConfig ?? {}
		})), { loading: P, FINAL_DATASET: F, manualLoading: et } = _e({
			...ze(O),
			FINAL_CONFIG: N,
			prepareConfig: L,
			callback: () => {
				Promise.resolve().then(async () => {
					await b(), _t();
				});
			},
			skeletonDataset: Qe(12),
			skeletonConfig: o({
				defaultConfig: N.value,
				userConfig: $e.value
			})
		}), { svgRef: I } = Ee({ config: N.value.style.title });
		function L() {
			let e = ye({
				userConfig: O.config,
				defaultConfig: We
			}), t = {}, n = e.theme;
			if (n) if (!Ge.value(e)) Ke(e), t = e;
			else {
				let r = ye({
					userConfig: Oe[n] || O.config,
					defaultConfig: e
				});
				t = { ...ye({
					userConfig: O.config,
					defaultConfig: r
				}) };
			}
			else t = e;
			return t;
		}
		let R = g(() => N.value?.style?.line.pulse || {}), tt = g(() => `${Math.max(200, Number(R.value.durationMs) || 4e3) / 1e3}s`), nt = C(0), rt = g(() => R.value?.begin || "0ms"), it = C("0;1"), at = g(() => R.value?.loop === !1 ? "1" : "indefinite"), ot = g(() => R.value?.loop === !1 ? "freeze" : void 0), st = g(() => R.value.trail.show && dt.value.lengthPx || 1), ct = g(() => !!R.value?.show && !Q.value && !qe.value && !P.value && (J.value?.length || 0) > 1);
		function lt() {
			if (!ct.value) {
				nt.value = 0;
				return;
			}
			let e = I.value;
			if (!e) return;
			let t = `#${G.value}`, n = e.querySelector?.(t);
			if (n && typeof n.getTotalLength == "function") {
				let e = n.getTotalLength();
				Number.isFinite(e) && e > 0 && (nt.value = e);
				return;
			}
			requestAnimationFrame(() => {
				let n = e.querySelector?.(t);
				if (n && typeof n.getTotalLength == "function") {
					let e = n.getTotalLength();
					Number.isFinite(e) && e > 0 && (nt.value = e);
				}
			});
		}
		let ut = g(() => {
			let e = R.value?.easing || "ease-in-out", t = {
				ease: [
					.25,
					.1,
					.25,
					1
				],
				"ease-in": [
					.42,
					0,
					1,
					1
				],
				"ease-out": [
					0,
					0,
					.58,
					1
				],
				"ease-in-out": [
					.42,
					0,
					.58,
					1
				]
			};
			return e === "linear" ? {
				calcMode: "linear",
				keySplines: null,
				keyTimes: "0;1"
			} : e === "steps" ? {
				calcMode: "discrete",
				keySplines: null,
				keyTimes: "0;1"
			} : {
				calcMode: "spline",
				keySplines: (e === "cubic-bezier" ? Array.isArray(R.value?.cubicBezier) && R.value.cubicBezier.length === 4 ? R.value.cubicBezier : [
					.4,
					0,
					.2,
					1
				] : t[e] || t["ease-in-out"]).join(" "),
				keyTimes: "0;1"
			};
		}), dt = g(() => {
			let e = R.value?.trail || {}, t = N.value.style.line.strokeWidth || 1;
			return {
				show: e.show !== !1,
				lengthPx: e.length,
				width: Math.max(1, Number(e.strokeWidth) || t * 2.2),
				opacity: Math.min(1, Math.max(0, Number(e.opacity) ?? .6)),
				fadeIn: .5,
				fadeOut: .2
			};
		}), z = g(() => r({
			data: F.value,
			threshold: N.value.downsample.threshold
		}));
		g(() => {
			if (!N.value.temperatureColors.show) return !1;
			let e = F.value.map(({ value: e }) => e).filter((e) => Number.isFinite(e));
			return new Set(e).size <= 1;
		}), D(() => O.config, (e) => {
			P.value || (N.value = L()), Kt(), K.value.chartWidth = N.value.style.chartWidth;
		}, { deep: !0 }), D(() => O.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (et.value = !1), B.value = r({
				data: F.value.map((e) => ({
					...e,
					value: [void 0].includes(e.value) ? null : e.value
				})),
				threshold: N.value.downsample.threshold
			});
		}, { deep: !0 });
		let B = C(V());
		function V() {
			return r({
				data: F.value.map((e) => N.value.style.animation.show && F.value.length > 1 ? {
					...e,
					value: null
				} : {
					...e,
					value: [void 0].includes(e.value) ? null : e.value
				}),
				threshold: N.value.downsample.threshold
			});
		}
		let H = Le(null), U = Le(null), ft = C(!1), pt = C(0), mt = C([]), ht = C(""), W = g(() => {
			let e = (z.value || []).map((e) => `${e.period}::${Number.isFinite(e.value) ? e.value : 0}`).join("|"), t = N.value?.style?.animation || {}, n = !!N.value?.gradientPath?.show && !N.value.temperatureColors.show;
			return `${e}#${!!t.show}#${t.animationFrames || 0}#${n}`;
		});
		function gt() {
			pt.value &&= (cancelAnimationFrame(pt.value), 0), mt.value.forEach((e) => clearTimeout(e)), mt.value = [], ft.value = !1;
		}
		function _t() {
			let e = N.value?.style?.animation || {}, t = z.value || [], n = W.value, r = !!N.value.gradientPath.show && !N.value.temperatureColors.show;
			if (n && n === ht.value && (ft.value || B.value.length === t.length)) return;
			if (gt(), r || !e.show || P.value || t.length <= 1 || qe.value) {
				B.value = t, ht.value = n;
				return;
			}
			ft.value = !0, ht.value = n, B.value = [];
			let i = Math.max(1, Number(e.animationFrames) || 1), a = Math.max(1, Math.floor(i / t.length)), o = 0, s = () => {
				if (n !== W.value) {
					gt();
					return;
				}
				if (o < t.length) {
					B.value.push(t[o]);
					let e = setTimeout(() => {
						pt.value = requestAnimationFrame(s);
					}, a);
					mt.value.push(e), o += 1;
				} else B.value = t, gt();
			};
			pt.value = requestAnimationFrame(s);
		}
		D(W, () => {
			_t();
		}), Ie(() => {
			Kt(), _t();
		}), Fe(() => {
			gt();
		});
		let yt = C(0);
		D(() => P.value, async (e) => {
			e || (await b(), yt.value += 1);
		}), D(() => ft.value, async (e) => {
			e || P.value || (await b(), yt.value += 1);
		});
		let G = g(() => `sparkline_line_path_${k.value}`), Ut = C(!0);
		async function Wt() {
			Ut.value = !1, await b(), Ut.value = !0, await b(), lt();
		}
		let Gt = g(() => N.value.debug);
		function Kt() {
			if (ae(O.dataset) ? (fe({
				componentName: "VueUiSparkline",
				type: "dataset",
				debug: Gt.value
			}), et.value = !0) : Gt.value && O.dataset.forEach((e, t) => {
				le({
					datasetObject: e,
					requiredAttributes: ["period", "value"]
				}).forEach((e) => {
					fe({
						componentName: "VueUiSparkline",
						type: "datasetSerieAttribute",
						property: e,
						index: t
					});
				});
			}), ae(O.dataset) || (et.value = N.value.loading), N.value.responsive) {
				let e = Se(() => {
					let { width: e, height: t } = Ce({
						chart: A.value,
						title: N.value.style.title.show && O.showInfo ? Xe.value : null,
						source: Ze.value
					});
					requestAnimationFrame(() => {
						K.value.width = e, K.value.height = t, K.value.chartWidth = N.value.style.chartWidth / 500 * e, K.value.padding = O.forcedPadding / 500 * e;
					});
				});
				H.value && (U.value && H.value.unobserve(U.value), H.value.disconnect()), H.value = new ResizeObserver(e), U.value = A.value.parentNode, H.value.observe(U.value);
			}
		}
		Fe(() => {
			H.value && (U.value && H.value.unobserve(U.value), H.value.disconnect());
		});
		let K = C({
			height: 80 * O.heightRatio,
			width: 500,
			chartWidth: N.value.style.chartWidth,
			padding: O.forcedPadding
		}), qt = xe, q = g(() => {
			let { top: e, right: t, bottom: n, left: r } = N.value.style.padding;
			return {
				top: e,
				left: r,
				right: K.value.width - t,
				bottom: K.value.height - n,
				start: O.showInfo && N.value.style.dataLabel.show && N.value.style.dataLabel.position === "left" ? K.value.width - K.value.chartWidth + r : K.value.padding + r,
				width: O.showInfo && N.value.style.dataLabel.show ? K.value.chartWidth - r - t : K.value.width - K.value.padding - r - t,
				height: K.value.height - e - n
			};
		}), Jt = g(() => [null, void 0].includes(N.value.style.scaleMin) ? Math.min(...B.value.map((e) => isNaN(e.value) || [
			void 0,
			null,
			"NaN",
			NaN,
			Infinity,
			-Infinity
		].includes(e.value) ? 0 : e.value || 0)) : N.value.style.scaleMin), Yt = g(() => [null, void 0].includes(N.value.style.scaleMax) ? Math.max(...B.value.map((e) => isNaN(e.value) || [
			void 0,
			null,
			"NaN",
			NaN,
			Infinity,
			-Infinity
		].includes(e.value) ? 0 : e.value || 0)) : N.value.style.scaleMax), Xt = g(() => {
			let e = Jt.value >= 0 ? 0 : Jt.value;
			return Math.abs(e);
		}), Zt = g(() => Yt.value + Xt.value), Qt = g(() => q.value.bottom - q.value.height * $t(Xt.value));
		function $t(e) {
			return isNaN(e / Zt.value) ? 0 : e / Zt.value;
		}
		let en = g(() => z.value.length - 1 || 1), tn = C([]), nn = 0;
		Be(() => {
			let e = ++nn;
			(async () => {
				let t = await m({
					values: z.value.map((e) => e.period),
					maxDatapoints: z.value.length,
					formatter: N.value.style.dataLabel.datetimeFormatter,
					start: 0,
					end: z.value.length
				});
				e === nn && (tn.value = t);
			})();
		});
		let J = g(() => B.value.map((e, t) => {
			let n = isNaN(e.value) || [
				void 0,
				"NaN",
				NaN,
				Infinity,
				-Infinity
			].includes(e.value) ? 0 : e.value, r = q.value.width / en.value;
			return {
				value: e.value,
				absoluteValue: n,
				period: tn.value && tn.value[t] && tn.value[t].text ? tn.value[t].text : e.period,
				plotValue: n + Xt.value,
				toMax: $t(n + Xt.value),
				x: q.value.start + t * r,
				y: q.value.bottom - q.value.height * $t(n + Xt.value),
				id: `plot_${k.value}_${t}`,
				color: Q.value ? N.value.style.bar.color : N.value.style.area.useGradient ? f(N.value.style.line.color, .05 * (1 - t / en.value)) : N.value.style.line.color,
				width: r
			};
		})), rn = g(() => O.selectedIndex !== void 0 && O.selectedIndex !== null && O.selectedIndex >= 0 && O.selectedIndex < J.value.length ? O.selectedIndex : M.value !== void 0 && M.value !== null && M.value >= 0 && M.value < J.value.length ? M.value : null), Y = C(void 0), X = C(void 0);
		function an(e, t) {
			N.value.events.datapointEnter && N.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), M.value = t, j.value = t, Y.value = e, X.value ||= e, qt("hoverIndex", { index: t });
		}
		function on(e, t) {
			N.value.events.datapointLeave && N.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}), X.value = Y.value, Y.value = void 0, M.value = null, j.value = null, qt("hoverIndex", { index: void 0 });
		}
		let Z = g(() => {
			if (Je.value) {
				let e = J.value.map((e) => e.absoluteValue), t = e.reduce((e, t) => e + t, 0);
				return {
					latest: J.value[J.value.length - 1] ? J.value[J.value.length - 1].absoluteValue : 0,
					sum: t,
					average: t / J.value.length,
					median: se(e),
					trend: ne(J.value.map(({ x: e, y: t, absoluteValue: n }) => ({
						x: e,
						y: t,
						value: n
					}))).trend
				};
			}
			return {
				latest: null,
				sum: null,
				average: null,
				median: null,
				trend: null
			};
		}), sn = g(() => Je.value ? N.value.style.dataLabel.valueType === "latest" ? Z.value.latest : N.value.style.dataLabel.valueType === "sum" ? Z.value.sum : N.value.style.dataLabel.valueType === "average" ? Z.value.average : 0 : 0), Q = g(() => N.value.type && N.value.type === "bar");
		function cn(e, t) {
			N.value.events.datapointClick && N.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			}), qt("selectDatapoint", {
				datapoint: e,
				index: t
			});
		}
		let ln = g(() => Array.isArray(N.value.style.line.dashIndices) && N.value.style.line.dashIndices.length > 0), un = g(() => N.value.style.line.cutNullValues);
		function dn(e) {
			return ![
				null,
				void 0,
				"NaN",
				NaN,
				Infinity,
				-Infinity
			].includes(e);
		}
		function fn(e, t) {
			let n = e[t - 1], r = e[t + 1], i = !!n && !!r && n.value == null && r.value == null || !n && !!r && r.value == null || !!n && !r && n.value == null;
			return dn(e[t]?.value) && i && un.value;
		}
		function pn(e, t) {
			return Y.value && e.id === Y.value.id || rn.value === t || F.value.length === 1;
		}
		function mn(e, t) {
			return Q.value || !dn(e?.value) ? !1 : N.value.style.plot.show && pn(e, t) || fn(J.value, t);
		}
		function hn(e, t) {
			return Math.max(1, pn(e, t) ? N.value.style.plot.radius : N.value.style.plot.radius * .7);
		}
		function gn(e, t) {
			return pn(e, t) ? N.value.style.plot.stroke : N.value.style.backgroundColor;
		}
		let _n = g(() => un.value ? J.value : J.value.filter((e) => e.value !== null)), vn = g(() => J.value.filter((e) => e.value !== null)), yn = g(() => vn.value.length > 1), bn = g(() => Q.value || !yn.value ? "" : un.value ? a(J.value) : l(_n.value)), xn = g(() => Q.value || !yn.value ? "" : un.value ? i(J.value) : ee(_n.value)), Sn = g(() => Q.value || !ln.value || !yn.value ? [] : s(_n.value, N.value.style.line.dashIndices)), Cn = g(() => Q.value || !ln.value || !yn.value ? [] : d(_n.value, N.value.style.line.dashIndices)), wn = g(() => Q.value || !N.value.style.area.show || !yn.value ? [] : N.value.style.line.smooth ? c(_n.value, q.value.bottom, un.value).filter(Boolean) : (un.value ? t(J.value, q.value.bottom) : oe(_n.value, q.value.bottom)).split(";").filter(Boolean).map((e) => `M${e}Z`)), Tn = g(() => Q.value || !N.value.gradientPath.show || N.value.temperatureColors.show ? "" : `M ${(N.value.style.line.smooth ? bn.value : xn.value) || "0,0"}`), En = g(() => {
			if (!N.value.temperatureColors.show) return null;
			let e = N.value.temperatureColors.colors;
			return !Array.isArray(e) || !e.length ? null : e.map((e) => u(e));
		}), $ = g(() => {
			if (!N.value.temperatureColors.show) return null;
			let e = N.value.temperatureColors.colors;
			return !Array.isArray(e) || !e.length ? null : e.map((e) => u(e));
		});
		function Dn(e) {
			if (!Number.isFinite(e?.y)) return 0;
			let t = q.value.height || 1, n = (e.y - q.value.top) / t;
			return Math.min(1, Math.max(0, n));
		}
		function On(e) {
			let t = En.value;
			return !Array.isArray(t) || !t.length ? e.color : te({
				colors: t,
				ratio: Dn(e)
			});
		}
		D(() => [
			ct.value,
			G.value,
			J.value.length,
			K.value.width,
			K.value.height,
			N.value?.style?.line?.smooth
		], async () => {
			await b(), lt();
		}, { immediate: !0 }), Ie(async () => {
			await b(), lt();
		}), D(() => P.value, async (e) => {
			e || await Wt();
		}), D(() => ft.value, async (e) => {
			e || P.value || await Wt();
		}), D(() => O.selectedIndex, (e) => {
			if (e == null) {
				M.value = null, j.value = null, Y.value = void 0;
				return;
			}
			if (e < 0 || e >= J.value.length) return;
			let t = J.value[e];
			t && (M.value = e, j.value = e, Y.value = t);
		});
		let kn = C(!1);
		function An() {
			let e = rn.value;
			if (e !== null && e >= 0 && e < J.value.length) {
				let t = J.value[e];
				if (t) {
					j.value = e, Y.value = t, M.value = e, kn.value = !0;
					return;
				}
			}
			j.value = null, !Y.value && J.value.length && an(J.value.at(-1), J.value.length - 1), kn.value = !0;
		}
		function jn() {
			X.value = Y.value, (O.selectedIndex === void 0 || O.selectedIndex === null) && (j.value = null, Y.value = void 0, M.value = null, qt("hoverIndex", { index: void 0 })), kn.value = !1;
		}
		function Mn(e) {
			if (!I.value || document.activeElement !== I.value) return;
			X.value = Y.value;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight";
			if (!t && !n) return;
			let r = J.value.length;
			if (!r) return;
			e.preventDefault(), e.stopPropagation();
			let i = j.value, a = i !== null && i >= 0 && i < r, o = Y.value ? J.value.findIndex((e) => e.id === Y.value.id) : -1;
			a ? n ? (i += 1, i >= r && (i = 0)) : t && (--i, i < 0 && (i = r - 1)) : o !== null && o >= 0 && o < r ? (i = n ? o + 1 : o - 1, i >= r && (i = 0), i < 0 && (i = r - 1)) : i = n ? 0 : r - 1;
			let s = J.value[i];
			s && (j.value = i, an(s, i));
		}
		let Nn = g(() => ({
			headers: [N.value.translations.period, N.value.translations.value],
			rows: J.value.map((e) => [e.period, e.absoluteValue])
		}));
		return (t, r) => (S(), v("div", {
			ref_key: "sparklineChart",
			ref: A,
			class: "vue-data-ui-component vue-ui-sparkline",
			id: k.value,
			style: x(`width:100%;font-family:${N.value.style.fontFamily};`)
		}, [
			y("p", {
				id: `chart-instructions-${k.value}`,
				class: "sr-only"
			}, Re(N.value.a11y.translations.keyboardNavigation), 9, xt),
			Nn.value?.rows?.length ? (S(), ke(Te, {
				key: 0,
				uid: k.value,
				head: Nn.value.headers,
				body: Nn.value.rows,
				notice: N.value.a11y.translations.tableAvailable,
				caption: N.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : _("", !0),
			T(t.$slots, "before", Pe(Ne({
				selected: Y.value,
				latest: Z.value.latest,
				sum: Z.value.sum,
				average: Z.value.average,
				median: Z.value.median,
				trend: Z.value.trend
			})), void 0, !0),
			N.value.style.title.show && e.showInfo ? (S(), v("div", {
				key: 1,
				ref_key: "chartTitle",
				ref: Xe,
				class: "vue-ui-sparkline-title",
				style: x(`display:flex;align-items:center;width:100%;color:${N.value.style.title.color};background:${N.value.style.backgroundColor};justify-content:${N.value.style.title.textAlign === "left" ? "flex-start" : N.value.style.title.textAlign === "right" ? "flex-end" : "center"};height:${N.value.style.title.fontSize * 2}px;font-size:${N.value.style.title.fontSize}px;font-weight:${N.value.style.title.bold ? "bold" : "normal"};`)
			}, [y("span", { style: x(`padding:${N.value.style.title.textAlign === "left" ? "0 0 0 12px" : N.value.style.title.textAlign === "right" ? "0 12px 0 0" : "0"}`) }, Re(Y.value ? Y.value.period : N.value.style.title.text), 5)], 4)) : _("", !0),
			y("div", St, [(S(), v("svg", {
				ref_key: "svgRef",
				ref: I,
				xmlns: E(de),
				viewBox: `0 0 ${K.value.width} ${K.value.height}`,
				style: x(`background:${N.value.style.backgroundColor};overflow:visible;direction:ltr`),
				tabindex: "0",
				"aria-describedby": `chart-instructions-${k.value}`,
				onMouseleave: r[0] ||= (e) => X.value = void 0,
				onFocus: An,
				onBlur: jn,
				onKeydown: Mn
			}, [
				je(E(He)),
				t.$slots["chart-background"] ? (S(), v("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: K.value.width <= 0 ? 10 : K.value.width,
					height: K.value.height <= 0 ? 10 : K.value.height,
					style: { pointerEvents: "none" }
				}, [T(t.$slots, "chart-background", {}, void 0, !0)], 8, wt)) : _("", !0),
				y("defs", null, [
					je(we, {
						t: "linear",
						x1: "0%",
						y1: "0%",
						x2: "100%",
						y2: "0%",
						id: `sparkline_gradient_${k.value}`,
						stops: [[
							"0%",
							E(n)(E(f)(N.value.style.area.color, .05), N.value.style.area.opacity),
							1
						], [
							"100%",
							E(n)(N.value.style.area.color, N.value.style.area.opacity),
							1
						]]
					}, null, 8, ["id", "stops"]),
					je(we, {
						t: "linear",
						x2: "0%",
						y2: "100%",
						id: `sparkline_bar_gradient_pos_${k.value}`,
						stops: [[
							"0%",
							N.value.style.bar.color,
							1
						], [
							"100%",
							E(f)(N.value.style.bar.color, .05),
							1
						]]
					}, null, 8, ["id", "stops"]),
					je(we, {
						t: "linear",
						x2: "0%",
						y2: "100%",
						id: `sparkline_bar_gradient_neg_${k.value}`,
						stops: [[
							"0%",
							E(f)(N.value.style.bar.color, .05),
							1
						], [
							"100%",
							N.value.style.bar.color,
							1
						]]
					}, null, 8, ["id", "stops"]),
					y("filter", {
						id: `sparkline_pulse_glow_${k.value}`,
						filterUnits: "userSpaceOnUse",
						x: "-50",
						y: "-50",
						width: "100",
						height: "100"
					}, [...r[1] ||= [y("feGaussianBlur", {
						in: "SourceGraphic",
						stdDeviation: "3",
						result: "blur"
					}, null, -1), y("feMerge", null, [y("feMergeNode", { in: "blur" }), y("feMergeNode", { in: "SourceGraphic" })], -1)]], 8, Tt),
					N.value.temperatureColors.show && $.value ? (S(), v("linearGradient", {
						key: 0,
						id: `temperature_grad_sparkline_${k.value}`,
						gradientUnits: "userSpaceOnUse",
						x1: "0",
						x2: "0",
						y1: q.value.top,
						y2: q.value.bottom
					}, [(S(!0), v(h, null, w($.value, (e, t) => (S(), v("stop", {
						key: `temperature_grad_stop_${t}_${k.value}`,
						"stop-color": e,
						offset: $.value.length === 1 ? "0%" : E(pe)(t, $.value.length)
					}, null, 8, Dt))), 128))], 8, Et)) : _("", !0)
				]),
				N.value.style.area.show && !Q.value && wn.value.length ? (S(), v("g", Ot, [(S(!0), v(h, null, w(wn.value, (e, t) => (S(), v("path", {
					key: `sparkline_area_${t}_${k.value}`,
					class: "vue-ui-sparkline-area",
					"data-cy": N.value.style.line.smooth ? "sparkline-smooth-area" : "sparkline-angle-area",
					d: e,
					fill: N.value.style.area.useGradient ? `url(#sparkline_gradient_${k.value})` : E(n)(N.value.style.area.color, N.value.style.area.opacity),
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					style: x({ transition: E(P) ? void 0 : "all 0.2s" })
				}, null, 12, kt))), 128))])) : _("", !0),
				N.value.style.line.smooth && !Q.value ? (S(), v(h, { key: 2 }, [y("path", {
					id: G.value,
					d: `M ${bn.value || "0,0"}`,
					fill: "none",
					stroke: "transparent",
					"stroke-width": N.value.style.line.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 8, At), ln.value ? (S(!0), v(h, { key: 0 }, w(Sn.value, (e) => (S(), v("path", {
					key: e.path,
					class: "vue-ui-sparkline-path",
					d: `M ${e.path}`,
					stroke: $.value ? `url(#temperature_grad_sparkline_${k.value})` : N.value.style.line.color,
					fill: "none",
					"stroke-width": N.value.style.line.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					"stroke-dasharray": e.dashed ? N.value.style.line.dashArray : 0,
					style: x({ transition: E(P) ? void 0 : "all 0.2s" })
				}, null, 12, jt))), 128)) : (S(), v("path", {
					key: 1,
					class: "vue-ui-sparkline-path",
					d: `M ${bn.value || "0,0"}`,
					stroke: $.value ? `url(#temperature_grad_sparkline_${k.value})` : N.value.style.line.color,
					fill: "none",
					"stroke-width": N.value.style.line.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					style: x({ transition: E(P) ? void 0 : "all 0.2s" })
				}, null, 12, Mt))], 64)) : _("", !0),
				!N.value.style.line.smooth && !Q.value ? (S(), v(h, { key: 3 }, [y("path", {
					id: G.value,
					d: `M ${xn.value || "0,0"}`,
					fill: "none",
					stroke: "transparent",
					"stroke-width": N.value.style.line.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 8, Nt), ln.value ? (S(!0), v(h, { key: 0 }, w(Cn.value, (e) => (S(), v("path", {
					key: e.path,
					class: "vue-ui-sparkline-path",
					d: `M ${e.path}`,
					stroke: $.value ? `url(#temperature_grad_sparkline_${k.value})` : N.value.style.line.color,
					fill: "none",
					"stroke-width": N.value.style.line.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					"stroke-dasharray": e.dashed ? N.value.style.line.dashArray * 2 : 0,
					style: x({ transition: E(P) ? void 0 : "all 0.2s" })
				}, null, 12, Pt))), 128)) : (S(), v("path", {
					key: 1,
					class: "vue-ui-sparkline-path",
					d: `M ${xn.value || "0,0"}`,
					stroke: $.value ? `url(#temperature_grad_sparkline_${k.value})` : N.value.style.line.color,
					fill: "none",
					"stroke-width": N.value.style.line.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					style: x({ transition: E(P) ? void 0 : "all 0.2s" })
				}, null, 12, Ft))], 64)) : _("", !0),
				Tn.value && !$.value ? (S(), ke(vt, {
					key: 4,
					svgPathData: Tn.value,
					enabled: N.value.gradientPath.show && !Q.value && !$.value,
					strokeWidth: N.value.style.line.strokeWidth,
					highColor: N.value.gradientPath.colors.high,
					lowColor: N.value.gradientPath.colors.low,
					segments: N.value.gradientPath.segments
				}, null, 8, [
					"svgPathData",
					"enabled",
					"strokeWidth",
					"highColor",
					"lowColor",
					"segments"
				])) : _("", !0),
				Ut.value && ct.value ? (S(), ke(Ye, {
					key: 5,
					uid: k.value,
					svgRef: E(I),
					pulsePathId: G.value,
					pulsePathLength: nt.value,
					pulseDur: tt.value,
					pulseBegin: rt.value,
					pulseRepeatCount: at.value,
					pulseFillMode: ot.value,
					pulseKeyPoints: it.value,
					pulseMotion: ut.value,
					pulse: R.value,
					pulseTrail: dt.value,
					pulseTrailLength: st.value,
					prefersReducedMotion: E(qe),
					loading: E(P),
					isBar: Q.value
				}, null, 8, [
					"uid",
					"svgRef",
					"pulsePathId",
					"pulsePathLength",
					"pulseDur",
					"pulseBegin",
					"pulseRepeatCount",
					"pulseFillMode",
					"pulseKeyPoints",
					"pulseMotion",
					"pulse",
					"pulseTrail",
					"pulseTrailLength",
					"prefersReducedMotion",
					"loading",
					"isBar"
				])) : _("", !0),
				(S(!0), v(h, null, w(J.value, (e, t) => (S(), v("g", null, [Q.value ? (S(), v("rect", {
					key: 0,
					x: e.x - e.width / 2,
					y: isNaN(e.absoluteValue > 0 ? e.y : Qt.value) ? 0 : e.absoluteValue > 0 ? e.y : Qt.value,
					width: e.width,
					height: isNaN(Math.abs(e.y - Qt.value)) ? 0 : Math.abs(e.y - Qt.value),
					fill: e.absoluteValue > 0 ? `url(#sparkline_bar_gradient_pos_${k.value})` : `url(#sparkline_bar_gradient_neg_${k.value})`,
					rx: N.value.style.bar.borderRadius
				}, null, 8, It)) : _("", !0), N.value.style.verticalIndicator.show && (Y.value && e.id === Y.value.id || rn.value === t) ? (S(), v("line", {
					key: 1,
					x1: e.x,
					x2: e.x,
					y1: q.value.top - 6,
					y2: q.value.bottom,
					stroke: N.value.style.verticalIndicator.color || e.color,
					"stroke-width": N.value.style.verticalIndicator.strokeWidth,
					"stroke-linecap": "round",
					"stroke-dasharray": N.value.style.verticalIndicator.strokeDasharray || 0
				}, null, 8, Lt)) : _("", !0)]))), 256)),
				Jt.value < 0 ? (S(), v("line", {
					key: 6,
					x1: q.value.start,
					x2: q.value.start + q.value.width,
					y1: E(ce)(Qt.value, q.value.bottom),
					y2: E(ce)(Qt.value, q.value.bottom),
					stroke: N.value.style.zeroLine.color,
					"stroke-dasharray": N.value.style.zeroLine.strokeWidth * 2,
					"stroke-width": N.value.style.zeroLine.strokeWidth,
					"stroke-linecap": "round"
				}, null, 8, Rt)) : _("", !0),
				(S(!0), v(h, null, w(J.value, (e, t) => (S(), v("g", { key: `sparkline_plot_${e.id}` }, [mn(e, t) ? (S(), v("circle", {
					key: 0,
					cx: e.x,
					cy: e.y,
					r: hn(e, t),
					fill: On(e),
					stroke: gn(e, t),
					"stroke-width": N.value.style.plot.strokeWidth
				}, null, 8, zt)) : _("", !0)]))), 128)),
				e.showInfo && N.value.style.dataLabel.show ? (S(), v("text", {
					key: 7,
					x: N.value.style.dataLabel.position === "left" ? 12 + N.value.style.dataLabel.offsetX : q.value.width + 12 + N.value.style.dataLabel.offsetX,
					y: K.value.height / 2 + N.value.style.dataLabel.fontSize / 2.5 + N.value.style.dataLabel.offsetY,
					"font-size": N.value.style.dataLabel.fontSize,
					"font-weight": N.value.style.dataLabel.bold ? "bold" : "normal",
					fill: N.value.style.dataLabel.color
				}, Re(Y.value ? E(re)(N.value.style.dataLabel.formatter, Y.value.absoluteValue, E(p)({
					p: N.value.style.dataLabel.prefix,
					v: Y.value.absoluteValue,
					s: N.value.style.dataLabel.suffix,
					r: N.value.style.dataLabel.roundingValue
				}), { datapoint: Y.value }) : E(re)(N.value.style.dataLabel.formatter, sn.value, E(p)({
					p: N.value.style.dataLabel.prefix,
					v: sn.value,
					s: N.value.style.dataLabel.suffix,
					r: N.value.style.dataLabel.roundingValue
				}))), 9, Bt)) : _("", !0),
				(S(!0), v(h, null, w(J.value, (e, t) => (S(), v("rect", {
					x: e.x - (q.value.width / (en.value + 1) > K.value.padding ? K.value.padding : q.value.width / (en.value + 1)) / 2,
					y: q.value.top - 6,
					height: q.value.height + 6,
					width: q.value.width / (en.value + 1) > K.value.padding ? K.value.padding : q.value.width / (en.value + 1),
					fill: "transparent",
					onMouseenter: () => an(e, t),
					onMouseleave: () => on(e, t),
					onClick: () => cn(e, t)
				}, null, 40, Vt))), 256)),
				T(t.$slots, "svg", { svg: {
					...K.value,
					drawingArea: q.value,
					timeLabels: tn.value,
					series: J.value,
					hoveredIndex: j.value
				} }, void 0, !0)
			], 44, Ct)), t.$slots.hint ? (S(), v("div", Ht, [T(t.$slots, "hint", Pe(Ne({
				hint: N.value.a11y.translations.keyboardNavigation,
				isVisible: kn.value
			})), void 0, !0)])) : _("", !0)]),
			Y.value && N.value.style.tooltip.show ? (S(), ke(E(Ue), {
				key: 2,
				x: Y.value.x,
				y: Y.value.y,
				prevX: X.value.x,
				prevY: X.value.y,
				offsetY: N.value.style.plot.radius * 3 + N.value.style.tooltip.offsetY,
				svgRef: E(I),
				background: N.value.style.tooltip.backgroundColor,
				color: N.value.style.tooltip.color,
				fontSize: N.value.style.tooltip.fontSize,
				borderWidth: N.value.style.tooltip.borderWidth,
				borderColor: N.value.style.tooltip.borderColor,
				borderRadius: N.value.style.tooltip.borderRadius,
				backgroundOpacity: N.value.style.tooltip.backgroundOpacity
			}, {
				default: Ve(() => [T(t.$slots, "tooltip", Pe(Ne({ ...Y.value })), () => [Ae(Re(Y.value.period) + ": " + Re(E(re)(N.value.style.dataLabel.formatter, Y.value.absoluteValue, E(p)({
					p: N.value.style.dataLabel.prefix,
					v: Y.value.absoluteValue,
					s: N.value.style.dataLabel.suffix,
					r: N.value.style.dataLabel.roundingValue
				}), { datapoint: Y.value })), 1)], !0)]),
				_: 3
			}, 8, [
				"x",
				"y",
				"prevX",
				"prevY",
				"offsetY",
				"svgRef",
				"background",
				"color",
				"fontSize",
				"borderWidth",
				"borderColor",
				"borderRadius",
				"backgroundOpacity"
			])) : _("", !0),
			t.$slots.source ? (S(), v("div", {
				key: 3,
				ref_key: "source",
				ref: Ze,
				dir: "auto"
			}, [T(t.$slots, "source", {}, void 0, !0)], 512)) : _("", !0),
			T(t.$slots, "skeleton", {}, () => [E(P) ? (S(), ke(ve, { key: 0 })) : _("", !0)], !0)
		], 12, bt));
	}
}, [["__scopeId", "data-v-4d09c790"]]);
//#endregion
export { yt as n, G as t };
