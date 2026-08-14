import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, J as r, Jt as i, Kt as a, Pt as o, S as s, X as c, b as l, jt as u, pt as d, q as f, t as p, tt as m, xt as h } from "./lib-Bttd6u5E.js";
import { n as g, t as _ } from "./useHints-Dq_w2E8B.js";
import { t as v } from "./useConfig-DlNpz6P8.js";
import { t as y } from "./usePrinter-DN5bYhTG.js";
import { n as ee, t as te } from "./BaseScanner-DZvpgOjM.js";
import { t as b } from "./useNestedProp-vPNvh7rV.js";
import { t as x } from "./useThemeCheck-C43Tcqmk.js";
import { t as S } from "./useChartExport-DNiwdPmb.js";
import { t as C } from "./img-Bnokohej.js";
import { n as w } from "./Title-BE3qg9xl.js";
import { t as T } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { i as ne, t as re } from "./useResponsive-ZtArZtUf.js";
import { t as ie } from "./A11yDataTable-DdRsVULz.js";
import { t as E } from "./useUserOptionState-DK-_1ddE.js";
import { t as ae } from "./useChartAccessibility-DYqac8yF.js";
import { t as oe } from "./usePanZoom-CYU3B4T3.js";
import { t as se } from "./vue_ui_word_cloud-C-qIMNLu.js";
import { t as ce } from "./BaseZoomControls-BZvCnZEi.js";
import { Fragment as le, computed as D, createBlock as O, createCommentVNode as k, createElementBlock as A, createElementVNode as j, createSlots as ue, createTextVNode as de, createVNode as fe, defineAsyncComponent as M, guardReactiveProps as N, mergeProps as pe, nextTick as me, normalizeClass as he, normalizeProps as P, normalizeStyle as ge, onBeforeUnmount as _e, onMounted as ve, openBlock as F, ref as I, renderList as ye, renderSlot as L, resolveDynamicComponent as be, shallowRef as xe, toDisplayString as Se, toRefs as Ce, unref as R, watch as we, withCtx as z } from "vue";
//#region src/wordcloud.js
var Te = {
	fast: {
		spiralStep: 6,
		fallbackSpiralStep: 2,
		spiralRadiusStep: 2,
		fallbackSpiralRadiusStep: 1,
		primaryAttempts: 1e4,
		fallbackAttempts: 25e3,
		minimumVisualPadding: 2,
		scaleStep: .9
	},
	balanced: {
		spiralStep: 4,
		fallbackSpiralStep: 1,
		spiralRadiusStep: 1.5,
		fallbackSpiralRadiusStep: 1,
		primaryAttempts: 2e4,
		fallbackAttempts: 5e4,
		minimumVisualPadding: 3,
		scaleStep: .92
	},
	precise: {
		spiralStep: 2,
		fallbackSpiralStep: 1,
		spiralRadiusStep: 1,
		fallbackSpiralRadiusStep: .75,
		primaryAttempts: 5e4,
		fallbackAttempts: 1e5,
		minimumVisualPadding: 4,
		scaleStep: .95
	}
}, Ee = "fast";
function De(e) {
	return Te[e] || Te[Ee];
}
var Oe = /* @__PURE__ */ new Map();
function ke(e) {
	let t = String(e), n = Oe.get(t);
	if (n) return n;
	let r = Math.PI / 180, i = [], a = [];
	for (let t = 0; t < 360; t += e) {
		let e = t * r;
		i.push(Math.cos(e)), a.push(Math.sin(e));
	}
	let o = {
		cosineArray: i,
		sineArray: a
	};
	return Oe.set(t, o), o;
}
function Ae({ word: e, fontSize: t, pad: n, canvas: r, ctx: i, svg: a }) {
	let o = a.style && a.style.bold, s = a.style && a.style.fontFamily || "Arial, \"Noto Sans Arabic\", Tahoma, sans-serif", c = `${o ? "bold " : ""}${t}px ${s}`, l = String(e.name || ""), u = /[\u0600-\u06FF]/.test(l);
	i.font = c, i.direction = u ? "rtl" : "ltr", i.textAlign = "left", i.textBaseline = "alphabetic";
	let d = i.measureText(l), f = Math.ceil(Math.abs(d.actualBoundingBoxLeft || 0)), p = Math.ceil(Math.abs(d.actualBoundingBoxRight || d.width)), m = Math.ceil(d.actualBoundingBoxAscent || t), h = Math.ceil(d.actualBoundingBoxDescent || t * .25), g = Math.max(0, Math.round(n || 0)) + Math.ceil(t * (u ? .22 : .12)), _ = Math.max(1, f + p + g * 2), v = Math.max(1, m + h + g * 2);
	r.width = _, r.height = v, i.font = c, i.direction = u ? "rtl" : "ltr", i.textAlign = "left", i.textBaseline = "alphabetic", i.fillStyle = "black";
	let y = g + f, ee = g + m;
	i.fillText(l, y, ee);
	let te = i.getImageData(0, 0, _, v).data, b = [], x = [], S = _, C = v, w = 0, T = 0, ne = !1;
	for (let e = 0; e < v; e += 1) {
		let t = e * _ * 4, n = -1, r = !1;
		for (let i = 0; i < _; i += 1) te[t + i * 4 + 3] > 1 ? (b.push([i, e]), ne = !0, i < S && (S = i), i > w && (w = i), e < C && (C = e), e > T && (T = e), r || (r = !0, n = i)) : r && (x.push([
			e,
			n,
			i - 1
		]), r = !1, n = -1);
		r && x.push([
			e,
			n,
			_ - 1
		]);
	}
	return ne || (S = 0, C = 0, w = 0, T = 0), {
		w: _,
		h: v,
		wordMask: b,
		runs: x,
		minX: S,
		minY: C,
		maxX: w,
		maxY: T
	};
}
function je(e) {
	let t = [];
	if (!e.length) return t;
	let n = e[0][1], r = e[0][0], i = r;
	for (let a = 1; a < e.length; a += 1) {
		let o = e[a][0], s = e[a][1];
		s === n ? o === i + 1 ? i = o : (t.push([
			n,
			r,
			i
		]), r = o, i = o) : (t.push([
			n,
			r,
			i
		]), n = s, r = o, i = o);
	}
	return t.push([
		n,
		r,
		i
	]), t;
}
function Me(e, t) {
	return 4294967295 >>> 32 - (t - e + 1) << e >>> 0;
}
function Ne(e) {
	return 4294967295 << e >>> 0;
}
function Pe(e) {
	return 4294967295 >>> 31 - e >>> 0;
}
function Fe({ maskBits: e, maskRowStride: t, maskW: n, maskH: r, wx: i, wy: a, runs: o }) {
	for (let s = 0; s < o.length; s += 1) {
		let c = o[s][0], l = o[s][1], u = o[s][2], d = a + c;
		if (d < 0 || d >= r) return !1;
		let f = i + l, p = i + u;
		if (f < 0 || p >= n) return !1;
		let m = d * t, h = f >>> 5, g = p >>> 5, _ = f & 31, v = p & 31;
		if (h === g) {
			if (e[m + h] & Me(_, v)) return !1;
		} else {
			if (e[m + h] & Ne(_)) return !1;
			for (let t = h + 1; t < g; t += 1) if (e[m + t]) return !1;
			if (e[m + g] & Pe(v)) return !1;
		}
	}
	return !0;
}
function Ie({ maskBits: e, maskRowStride: t, maskW: n, maskH: r, wx: i, wy: a, runs: o }) {
	for (let s = 0; s < o.length; s += 1) {
		let c = o[s][0], l = o[s][1], u = o[s][2], d = a + c;
		if (d < 0 || d >= r) continue;
		let f = i + l, p = i + u;
		if (p < 0 || f >= n) continue;
		let m = d * t, h = f >>> 5, g = p >>> 5, _ = f & 31, v = p & 31;
		if (h === g) {
			let t = Me(_, v);
			e[m + h] |= t;
		} else {
			{
				let t = Ne(_);
				e[m + h] |= t;
			}
			for (let t = h + 1; t < g; t += 1) e[m + t] = 4294967295;
			{
				let t = Pe(v);
				e[m + g] |= t;
			}
		}
	}
}
function B({ wordMask: e, w: t, h: n, dilation: r }) {
	let i = new Uint8Array(t * n), a = [];
	for (let n = 0; n < e.length; n += 1) {
		let r = e[n][0], o = e[n][1] * t + r;
		i[o] || (i[o] = 1, a.push(o));
	}
	for (let e = 0; e < a.length; e += 1) {
		let o = a[e], s = o / t | 0, c = o - s * t;
		for (let e = -r; e <= r; e += 1) {
			let a = s + e;
			if (a < 0 || a >= n) continue;
			let o = a * t;
			for (let n = -r; n <= r; n += 1) {
				if (n === 0 && e === 0) continue;
				let r = c + n;
				r < 0 || r >= t || (i[o + r] = 1);
			}
		}
	}
	let o = [];
	for (let e = 0; e < i.length; e += 1) if (i[e]) {
		let n = e / t | 0, r = e - n * t;
		o.push([r, n]);
	}
	return o;
}
function Le(e, t) {
	let n = [];
	for (let r = 0; r < t; r += 1) {
		let t = e[r];
		if (!t || t.length === 0) continue;
		t.sort((e, t) => e[0] - t[0]);
		let i = t[0][0], a = t[0][1];
		for (let e = 1; e < t.length; e += 1) {
			let o = t[e][0], s = t[e][1];
			o <= a + 1 ? s > a && (a = s) : (n.push([
				r,
				i,
				a
			]), i = o, a = s);
		}
		n.push([
			r,
			i,
			a
		]);
	}
	return n;
}
function Re({ runs: e, w: t, h: n, dilation: r }) {
	if (!e.length || r <= 0) return e;
	let i = Array(n);
	for (let a = 0; a < e.length; a += 1) {
		let o = e[a], s = o[0], c = Math.max(0, o[1] - r), l = Math.min(t - 1, o[2] + r), u = Math.max(0, s - r), d = Math.min(n - 1, s + r);
		for (let e = u; e <= d; e += 1) {
			let t = i[e];
			t || (t = [], i[e] = t), t.push([c, l]);
		}
	}
	return Le(i, n);
}
function V(e, t) {
	let n = e.runs, r = e.w, i = e.h, a = Math.max(1, Math.round(r * t)), o = Math.max(1, Math.round(i * t)), s = Array(o), c = a, l = o, u = 0, d = 0, f = !1;
	for (let e = 0; e < n.length; e += 1) {
		let r = n[e], i = r[0], a = r[1], p = r[2], m = Math.round(i * t);
		if (m < 0 || m >= o) continue;
		let h = Math.round(a * t), g = Math.round((p + 1) * t) - 1;
		if (g < h) continue;
		let _ = s[m];
		_ || (_ = [], s[m] = _), _.push([h, g]), f = !0, h < c && (c = h), g > u && (u = g), m < l && (l = m), m > d && (d = m);
	}
	if (!f) return {
		w: a,
		h: o,
		runs: [],
		minX: 0,
		minY: 0,
		maxX: 0,
		maxY: 0
	};
	let p = [];
	for (let e = 0; e < o; e += 1) {
		let t = s[e];
		if (!t || t.length === 0) continue;
		t.sort((e, t) => e[0] - t[0]);
		let n = t[0][0], r = t[0][1];
		for (let i = 1; i < t.length; i += 1) {
			let a = t[i][0], o = t[i][1];
			a <= r + 1 ? o > r && (r = o) : (p.push([
				e,
				n,
				r
			]), n = a, r = o);
		}
		p.push([
			e,
			n,
			r
		]);
	}
	return {
		w: a,
		h: o,
		runs: p,
		minX: c,
		minY: l,
		maxX: u,
		maxY: d
	};
}
var ze = /* @__PURE__ */ new Map(), H = /* @__PURE__ */ new Map();
function Be({ word: e, fontSize: t, pad: n, svg: r }) {
	let i = r.style && r.style.bold ? 1 : 0, a = n || 0;
	return `${e.name}::${t}::${a}::${i}`;
}
function Ve({ word: e, fontSize: t, pad: n, canvas: r, ctx: i, svg: a }) {
	let o = Be({
		word: e,
		fontSize: t,
		pad: n,
		svg: a
	}), s = ze.get(o);
	if (s) return {
		key: o,
		bitmap: s
	};
	let c = Ae({
		word: e,
		fontSize: t,
		pad: n,
		canvas: r,
		ctx: i,
		svg: a
	});
	return ze.set(o, c), {
		key: o,
		bitmap: c
	};
}
function He({ bitmapKey: e, wordMask: t, w: n, h: r, dilation: i }) {
	let a = `${e}::d${i}`, o = H.get(a);
	if (o) return o;
	let s = B({
		wordMask: t,
		w: n,
		h: r,
		dilation: i
	}), c = {
		wordMask: s,
		runs: je(s)
	};
	return H.set(a, c), c;
}
function Ue(e, t, n) {
	if (!e.length) return;
	let r = Infinity, i = -Infinity, a = Infinity, o = -Infinity;
	for (let t = 0; t < e.length; t += 1) {
		let n = e[t], s = n.x + n.minX, c = n.x + n.maxX, l = n.y + n.minY, u = n.y + n.maxY;
		s < r && (r = s), c > i && (i = c), l < a && (a = l), u > o && (o = u);
	}
	if (!isFinite(r) || !isFinite(i) || !isFinite(a) || !isFinite(o)) return;
	let s = Math.max(Math.abs(r), Math.abs(i)), c = Math.max(Math.abs(a), Math.abs(o));
	if (s === 0 || c === 0) return;
	let l = .9, u = t * .5 * l / s, d = n * .5 * l / c, f = Math.min(u, d);
	if (!(f <= 1)) {
		f > 4 && (f = 4);
		for (let t = 0; t < e.length; t += 1) {
			let n = e[t];
			n.x *= f, n.y *= f, n.width *= f, n.height *= f, n.fontSize *= f, n.minX *= f, n.maxX *= f, n.minY *= f, n.maxY *= f;
		}
	}
}
function We() {
	return typeof performance < "u" && typeof performance.now == "function" ? () => performance.now() : () => Date.now();
}
function Ge({ value: e, minimumValue: t, maximumValue: n, configuredMinimumFontSize: r, maximumFontSize: i }) {
	if (n === t) return i;
	let a = (e - t) / (n - t) * (i - r) + r;
	return Math.max(r, Math.min(i, a));
}
function Ke({ bitmapHeight: e, proximity: t, strictPixelPadding: n, minimumVisualPadding: r }) {
	let i = Math.max(0, Math.round(t || 0)), a = Math.ceil(e * .035) + 2;
	return Math.max(i, a, n ? 3 : 0, r);
}
function qe({ currentBitmap: e, strictPixelPadding: t, scaleFactor: n, baseBitmap: r, bitmapKey: i, proximity: a, minimumVisualPadding: o }) {
	let s = e.runs, c = e.w, l = e.h, u = e.minX, d = e.minY, f = e.maxX, p = e.maxY;
	if (!s.length) return {
		runs: s,
		bitmapWidth: c,
		bitmapHeight: l,
		bitmapMinimumX: u,
		bitmapMinimumY: d,
		bitmapMaximumX: f,
		bitmapMaximumY: p
	};
	let m = Ke({
		bitmapHeight: l,
		proximity: a,
		strictPixelPadding: t,
		minimumVisualPadding: o
	});
	return m <= 0 ? {
		runs: s,
		bitmapWidth: c,
		bitmapHeight: l,
		bitmapMinimumX: u,
		bitmapMinimumY: d,
		bitmapMaximumX: f,
		bitmapMaximumY: p
	} : n === 1 && m === 2 && t ? {
		runs: He({
			bitmapKey: i,
			wordMask: r.wordMask,
			w: r.w,
			h: r.h,
			dilation: m
		}).runs,
		bitmapWidth: c,
		bitmapHeight: l,
		bitmapMinimumX: u,
		bitmapMinimumY: d,
		bitmapMaximumX: f,
		bitmapMaximumY: p
	} : {
		runs: Re({
			runs: s,
			w: c,
			h: l,
			dilation: m
		}),
		bitmapWidth: c,
		bitmapHeight: l,
		bitmapMinimumX: u,
		bitmapMinimumY: d,
		bitmapMaximumX: f,
		bitmapMaximumY: p
	};
}
async function Je({ baseBitmap: e, baseFontSize: t, minimumScaleFactor: n, maskBits: r, maskRowStride: i, maskWidth: a, maskHeight: o, centerX: s, centerY: c, maximumRadius: l, scaleStep: u, strictPixelPadding: d, bitmapKey: f, minimumFontSize: p, rawWord: m, cosineArray: h, sineArray: g, radiusStep: _, maximumAttempts: v, maybeYield: y, proximity: ee, minimumVisualPadding: te }) {
	let b = 1;
	for (; b >= n;) {
		let { runs: n, bitmapWidth: x, bitmapHeight: S, bitmapMinimumX: C, bitmapMinimumY: w, bitmapMaximumX: T, bitmapMaximumY: ne } = qe({
			currentBitmap: b === 1 ? e : V(e, b),
			strictPixelPadding: d,
			scaleFactor: b,
			baseBitmap: e,
			bitmapKey: f,
			proximity: ee,
			minimumVisualPadding: te
		}), re = 0, ie = 0;
		for (; re < l && ie < v;) {
			for (let e = 0; e < h.length; e += 1) {
				ie += 1;
				let l = Math.round(s + re * h[e] - x / 2), u = Math.round(c + re * g[e] - S / 2);
				if (!(l < 0 || u < 0 || l + x > a || u + S > o) && Fe({
					maskBits: r,
					maskRowStride: i,
					maskW: a,
					maskH: o,
					wx: l,
					wy: u,
					runs: n
				})) {
					let { __wcIndex: e, ...s } = m, c = Math.max(p, Math.round(t * b)), d = {
						...s,
						x: l - a / 2,
						y: u - o / 2,
						fontSize: c,
						width: x,
						height: S,
						angle: 0,
						minX: C,
						minY: w,
						maxX: T,
						maxY: ne
					};
					return Ie({
						maskBits: r,
						maskRowStride: i,
						maskW: a,
						maskH: o,
						wx: l,
						wy: u,
						runs: n
					}), d;
				}
			}
			re += _, ie & 1023 || await y();
		}
		b *= u, await y();
	}
	return null;
}
async function Ye({ baseBitmap: e, baseFontSize: t, minimumScaleFactor: n, maskBits: r, maskRowStride: i, maskWidth: a, maskHeight: o, centerX: s, centerY: c, maximumRadius: l, scaleStep: u, strictPixelPadding: d, bitmapKey: f, minimumFontSize: p, rawWord: m, maybeYield: h, proximity: g, qualityPreset: _, primarySpiral: v, fallbackSpiral: y }) {
	return await Je({
		baseBitmap: e,
		baseFontSize: t,
		minimumScaleFactor: n,
		maskBits: r,
		maskRowStride: i,
		maskWidth: a,
		maskHeight: o,
		centerX: s,
		centerY: c,
		maximumRadius: l,
		scaleStep: u,
		strictPixelPadding: d,
		bitmapKey: f,
		minimumFontSize: p,
		rawWord: m,
		cosineArray: v.cosineArray,
		sineArray: v.sineArray,
		radiusStep: _.spiralRadiusStep,
		maximumAttempts: _.primaryAttempts,
		maybeYield: h,
		proximity: g,
		minimumVisualPadding: _.minimumVisualPadding
	}) || await Je({
		baseBitmap: e,
		baseFontSize: t,
		minimumScaleFactor: n,
		maskBits: r,
		maskRowStride: i,
		maskWidth: a,
		maskHeight: o,
		centerX: s,
		centerY: c,
		maximumRadius: l,
		scaleStep: u,
		strictPixelPadding: d,
		bitmapKey: f,
		minimumFontSize: p,
		rawWord: m,
		cosineArray: y.cosineArray,
		sineArray: y.sineArray,
		radiusStep: _.fallbackSpiralRadiusStep,
		maximumAttempts: _.fallbackAttempts,
		maybeYield: h,
		proximity: g,
		minimumVisualPadding: _.minimumVisualPadding
	});
}
async function Xe({ words: e, proximity: t = 0, svg: n, strictPixelPadding: r, quality: i = Ee, onProgress: a, debugTiming: o = !1 }) {
	let s = We(), c = s(), l = c;
	async function u() {
		s() - l >= 12 && (await new Promise((e) => setTimeout(e, 0)), l = s());
	}
	let d = n.width, f = n.height, p = Math.round(d), m = Math.round(f), h = De(i), g = ke(h.spiralStep), _ = ke(h.fallbackSpiralStep), v = n.minFontSize, y = Math.min(n.maxFontSize, 100), ee = e.map((e) => e.value), te = Math.min(...ee), b = Math.max(...ee);
	if (p <= 0 || m <= 0) return [];
	let x = p + 31 >>> 5, S = new Uint32Array(x * m), C = document.createElement("canvas"), w = C.getContext("2d", { willReadFrequently: !0 });
	C.width = p, C.height = m;
	let T = Math.max(p, m), ne = Math.floor(p / 2), re = Math.floor(m / 2), ie = [...e.map((e, t) => ({
		...e,
		__wcIndex: t,
		id: e.id == null ? `${e.name}__${t}` : e.id
	}))].sort((e, t) => t.value - e.value), E = [], ae = h.scaleStep;
	for (let e = 0; e < ie.length; e += 1) {
		let i = ie[e], o = E.length, s = Ge({
			value: i.value,
			minimumValue: te,
			maximumValue: b,
			configuredMinimumFontSize: v,
			maximumFontSize: y
		}), c = Ve({
			word: i,
			fontSize: s,
			pad: t,
			canvas: C,
			ctx: w,
			svg: n
		}), l = c.key, d = c.bitmap, f = d.w, ee = d.h;
		if (f <= 0 || ee <= 0) {
			await u();
			continue;
		}
		let oe = await Ye({
			baseBitmap: d,
			baseFontSize: s,
			minimumScaleFactor: Math.max(1 / s, .1),
			maskBits: S,
			maskRowStride: x,
			maskWidth: p,
			maskHeight: m,
			centerX: ne,
			centerY: re,
			maximumRadius: T,
			scaleStep: ae,
			strictPixelPadding: r,
			bitmapKey: l,
			minimumFontSize: 1,
			rawWord: i,
			maybeYield: u,
			proximity: t,
			qualityPreset: h,
			primarySpiral: g,
			fallbackSpiral: _
		});
		if (oe && (E.push(oe), a && E.length > o)) {
			let e = E[E.length - 1];
			a({
				word: e,
				all: E
			});
		}
		await u();
	}
	if (!E.length) return [];
	Ue(E, p, m);
	let oe = s() - c;
	return o && typeof console < "u" && console.log && console.log("[vue-data-ui][word-cloud] positionWordsAsync:", `${oe.toFixed(2)} ms for ${e.length} words`), E.sort((e, t) => t.fontSize - e.fontSize);
}
//#endregion
//#region src/components/vue-ui-word-cloud.vue
var U = /* @__PURE__ */ e({ default: () => ut }), Ze = [
	"id",
	"data-resizing",
	"data-relayout"
], Qe = ["id"], $e = { style: { position: "relative" } }, et = [
	"aria-describedby",
	"xmlns",
	"viewBox"
], tt = ["width", "height"], nt = ["transform"], rt = ["transform"], it = [
	"data-a11y-word-index",
	"x",
	"y",
	"width",
	"height",
	"aria-label",
	"onMouseover",
	"onMouseleave",
	"onClick"
], at = [
	"fill",
	"font-weight",
	"font-size",
	"transform",
	"stroke",
	"stroke-width"
], ot = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, st = {
	key: 5,
	class: "vue-data-ui-watermark"
}, ct = {
	key: 6,
	"data-dom-to-png-ignore": "",
	class: "reset-wrapper"
}, lt = ["innerHTML"], ut = /*#__PURE__*/ T({
	__name: "vue-ui-word-cloud",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: [Array, String],
			default() {
				return [];
			}
		}
	},
	emits: ["copyAlt"],
	setup(e, { expose: T, emit: Te }) {
		let Ee = M(() => import("./Tooltip-DhjyfHwz.js")), De = M(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Oe = M(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), ke = M(() => import("./DataTable-BbKgJ5UI.js")), Ae = M(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), je = M(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Me = M(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ne = M(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_word_cloud: Pe } = v(), { isThemeValid: Fe, warnInvalidTheme: Ie } = x(), B = e, Le = Te, Re = D({
			get() {
				return !!B.dataset && B.dataset.length;
			},
			set(e) {
				return e;
			}
		}), V = I(f()), ze = I(0), H = I(null), Be = I(null), Ve = I(null), He = I(0), Ue = I(0), We = I(!1), Ge = I(null), Ke = I(null), qe = I(!1), Je = I(!1), Ye = I(null), U = I(null), ut = I({
			x: 0,
			y: 0
		}), dt = I("pointer"), ft = I(!1), W = I(St());
		g({
			config: () => W.value,
			dataset: () => B.dataset,
			component: "VueUiWordCloud",
			rules: [_.noHint]
		});
		let G = D(() => W.value.userOptions.useCursorPointer), pt = D(() => i({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				useCssAnimation: !1,
				animationDelayMs: 0,
				nodeCategories: {},
				nodeCategoryColors: {},
				style: { chart: {
					backgroundColor: "#99999930",
					words: {
						color: "#6A6A6A",
						usePalette: !1,
						selectedStroke: "#CCCCCC"
					}
				} }
			},
			userConfig: W.value.skeletonConfig ?? {}
		})), { loading: mt, FINAL_DATASET: ht, manualLoading: gt } = ee({
			...Ce(B),
			FINAL_CONFIG: W,
			prepareConfig: St,
			callback: () => {
				Promise.resolve().then(() => {
					Y.value.showTable = W.value.table.show, Y.value.showTooltip = W.value.style.chart.tooltip.show, Y.value.showZoom = W.value.style.chart.zoom.show;
				});
			},
			skeletonDataset: B.config?.skeletonDataset ?? [
				{
					name: "Lorem",
					value: 6
				},
				{
					name: "ipsum",
					value: 3
				},
				{
					name: "dolor",
					value: 1
				},
				{
					name: "sit",
					value: 3
				},
				{
					name: "amet",
					value: 3
				},
				{
					name: "consectetur",
					value: 2
				},
				{
					name: "adipiscing",
					value: 1
				},
				{
					name: "elit",
					value: 2
				},
				{
					name: "Vivamus",
					value: 2
				},
				{
					name: "pulvinar",
					value: 1
				},
				{
					name: "pretium",
					value: 1
				},
				{
					name: "venenatis",
					value: 2
				},
				{
					name: "Donec",
					value: 1
				},
				{
					name: "imperdiet",
					value: 3
				},
				{
					name: "id",
					value: 1
				},
				{
					name: "porttitor",
					value: 2
				},
				{
					name: "tristique",
					value: 1
				},
				{
					name: "Aenean",
					value: 2
				},
				{
					name: "ac",
					value: 5
				},
				{
					name: "commodo",
					value: 2
				},
				{
					name: "justo",
					value: 2
				},
				{
					name: "Vestibulum",
					value: 2
				},
				{
					name: "placerat",
					value: 1
				},
				{
					name: "molestie",
					value: 1
				},
				{
					name: "nisl",
					value: 1
				},
				{
					name: "lacinia",
					value: 2
				},
				{
					name: "nulla",
					value: 1
				},
				{
					name: "posuere",
					value: 2
				},
				{
					name: "quis",
					value: 3
				},
				{
					name: "ullamcorper",
					value: 1
				},
				{
					name: "eu",
					value: 1
				},
				{
					name: "ex",
					value: 1
				},
				{
					name: "vitae",
					value: 3
				},
				{
					name: "facilisis",
					value: 1
				},
				{
					name: "Aliquam",
					value: 1
				},
				{
					name: "erat",
					value: 1
				},
				{
					name: "volutpat",
					value: 1
				},
				{
					name: "Proin",
					value: 1
				},
				{
					name: "nunc",
					value: 1
				},
				{
					name: "felis",
					value: 1
				},
				{
					name: "gravida",
					value: 3
				},
				{
					name: "sed",
					value: 1
				},
				{
					name: "orci",
					value: 1
				},
				{
					name: "Interdum",
					value: 1
				},
				{
					name: "et",
					value: 1
				},
				{
					name: "malesuada",
					value: 1
				},
				{
					name: "fames",
					value: 1
				},
				{
					name: "ante",
					value: 1
				}
			],
			skeletonConfig: i({
				defaultConfig: W.value,
				userConfig: pt.value
			})
		}), _t = I(vt());
		function vt() {
			return typeof ht.value == "string" ? r(ht.value) : ht.value.map((e, t) => ({
				...e,
				value: l(e.value)
			}));
		}
		let { userOptionsVisible: yt, setUserOptionsVisibility: bt, keepUserOptionState: xt } = E({ config: W.value }), { svgRef: K } = ae({ config: W.value.style.chart.title });
		function St() {
			let e = b({
				userConfig: B.config,
				defaultConfig: Pe
			}), t = e.theme;
			if (!t) return e;
			if (!Fe.value(e)) return Ie(e), e;
			let n = b({
				userConfig: se[t] || B.config,
				defaultConfig: e
			}), r = b({
				userConfig: B.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : a[t] || o
			};
		}
		let Ct = I({
			x: 0,
			y: 0
		});
		function wt() {
			let e = {
				x: 0,
				y: 0,
				width: Math.max(10, q.value.width),
				height: Math.max(10, q.value.height)
			};
			Qt(e), Ct.value = {
				x: e.x + e.width / 2,
				y: e.y + e.height / 2
			}, Xt();
		}
		let Tt = ne(() => {
			Rt();
		}, 100), Et = I(!1);
		we(() => Et.value, (e) => {
			e === !1 && (Je.value = !0, Tt(), wt());
		}), we(() => B.config, (e) => {
			W.value = St(), yt.value = !W.value.userOptions.showOnChartHover, jt(), He.value += 1, Ue.value += 1, Y.value.showTable = W.value.table.show, Y.value.showTooltip = W.value.style.chart.tooltip.show, Y.value.showZoom = W.value.style.chart.zoom.show;
		}, { deep: !0 });
		let q = I({
			width: W.value.style.chart.width,
			height: W.value.style.chart.height,
			maxFontSize: W.value.style.chart.words.maxFontSize,
			minFontSize: W.value.style.chart.words.minFontSize,
			bold: W.value.style.chart.words.bold
		}), Dt = ne(() => {
			let { width: e, height: t } = re({
				chart: H.value,
				title: W.value.style.chart.title.text ? Be.value : null,
				legend: W.value.style.chart.controls.show ? Ye.value?.$el : null,
				source: Ve.value
			});
			requestAnimationFrame(async () => {
				q.value.width = Math.max(10, e), q.value.height = Math.max(10, t - 12), await me(), Et.value = !1;
			});
		}, 100), Ot = () => {
			Et.value = !0, Dt();
		}, J = xe(null), kt = xe(null);
		ve(jt);
		let At = D(() => W.value.debug);
		function jt() {
			u(B.dataset) ? m({
				componentName: "VueUiWordCloud",
				type: "dataset",
				debug: At.value
			}) : _t.value.forEach((e, t) => {
				d({
					datasetObject: e,
					requiredAttributes: ["name", "value"]
				}).forEach((e) => {
					Re.value = !1, m({
						componentName: "VueUiWordCloud",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: At.value
					});
				});
			}), u(B.dataset) || (gt.value = W.value.loading), W.value.responsive && (J.value && (kt.value && J.value.unobserve(kt.value), J.value.disconnect()), J.value = new ResizeObserver(Ot), kt.value = H.value.parentNode, J.value.observe(kt.value)), wt();
		}
		_e(() => {
			J.value && (kt.value && J.value.unobserve(kt.value), J.value.disconnect());
		});
		let { isPrinting: Mt, isImaging: Nt, generatePdf: Pt, generateImage: Ft } = y({
			elementId: `wordCloud_${V.value}`,
			fileName: W.value.style.chart.title.text || "vue-ui-word-cloud",
			options: W.value.userOptions.print
		}), Y = I({
			showTable: W.value.table.show,
			showTooltip: W.value.style.chart.tooltip.show,
			showZoom: W.value.style.chart.zoom.show
		});
		we(W, () => {
			Y.value.showTable = W.value.table.show, Y.value.showTooltip = W.value.style.chart.tooltip.show, Y.value.showZoom = W.value.style.chart.zoom.show;
		}, { immediate: !0 });
		function It(e, t, n = "Arial") {
			let r = document.createElement("canvas").getContext("2d");
			return r.font = `${t}px ${W.value.style.chart.words.bold ? "bold" : "normal"} ${n}`, {
				width: r.measureText(e).width + W.value.style.chart.words.proximity,
				height: t
			};
		}
		let X = I([]), Lt = /* @__PURE__ */ new Map();
		function Rt() {
			let e = [..._t.value].map((e) => e.value), t = Math.max(...e), n = Math.min(...e), r = [..._t.value].map((e, r) => {
				let i = (e.value - n) / (t - n) * (q.value.maxFontSize - q.value.minFontSize) + q.value.minFontSize;
				i = isNaN(i) ? q.value.minFontSize : i;
				let a = It(e.name, i);
				return {
					...e,
					id: e.id ?? `${e.name}__${r}`,
					fontSize: i,
					width: a.width,
					height: a.height,
					color: e.color ? s(e.color) : W.value.style.chart.words.usePalette ? W.value.customPalette[r] || W.value.customPalette[r % W.value.customPalette.length] || o[r] || o[r % o.length] : W.value.style.chart.words.color
				};
			});
			X.value.length = 0, Lt.clear(), Xe({
				debugTiming: At.value,
				words: r,
				svg: q.value,
				proximity: W.value.style.chart.words.proximity,
				strictPixelPadding: W.value.strictPixelPadding,
				quality: W.value.quality,
				onProgress: ({ all: e }) => {
					for (let t of e) {
						let e = t.id, n = Lt.get(e);
						if (n === void 0) n = X.value.length, Lt.set(e, n), X.value.push({ ...t });
						else {
							let e = X.value[n];
							e.x = t.x, e.y = t.y, e.width = t.width, e.height = t.height, e.fontSize = t.fontSize, e.minX = t.minX, e.minY = t.minY, e.maxX = t.maxX, e.maxY = t.maxY;
						}
					}
				}
			}), X.value.sort((e, t) => t.fontSize - e.fontSize), Lt.clear(), X.value.forEach((e, t) => {
				Lt.set(e.id, t);
			}), qe.value = !0, Je.value = !1;
		}
		let zt = D(() => ({
			head: X.value.map((e) => ({
				name: e.name,
				color: e.color
			})),
			body: X.value.map((e) => e.value)
		}));
		function Bt(e = null) {
			me(() => {
				let r = zt.value.head.map((e, t) => [[e.name], [zt.value.body[t]]]), i = [
					[W.value.style.chart.title.text],
					[W.value.style.chart.title.subtitle.text],
					[[""], [W.value.table.columnNames.value]]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: W.value.style.chart.title.text || "vue-ui-word-cloud"
				});
			});
		}
		let Z = D(() => {
			let e = [W.value.table.columnNames.series, W.value.table.columnNames.value], t = zt.value.head.map((e, t) => {
				let n = c({
					p: W.value.table.td.prefix,
					v: zt.value.body[t],
					s: W.value.table.td.suffix,
					r: W.value.table.td.roundingValue
				});
				return [{
					color: e.color,
					name: e.name
				}, n];
			}), n = t.map((e) => e.map((e, t) => t === 0 ? e.name : e)), r = {
				th: {
					backgroundColor: W.value.table.th.backgroundColor,
					color: W.value.table.th.color,
					outline: W.value.table.th.outline
				},
				td: {
					backgroundColor: W.value.table.td.backgroundColor,
					color: W.value.table.td.color,
					outline: W.value.table.td.outline
				},
				breakpoint: W.value.table.responsiveBreakpoint
			};
			return {
				colNames: [W.value.table.columnNames.series, W.value.table.columnNames.value],
				head: e,
				body: t,
				a11yBody: n,
				config: r
			};
		}), Q = I(!1);
		function Vt(e) {
			Q.value = e, ze.value += 1;
		}
		function Ht() {
			return X.value;
		}
		function Ut() {
			Y.value.showTable = !Y.value.showTable;
		}
		function Wt() {
			Y.value.showTooltip = !Y.value.showTooltip;
		}
		let Gt = I(!1);
		function Kt() {
			Gt.value = !Gt.value;
		}
		function qt() {
			Y.value.showZoom = !Y.value.showZoom;
		}
		let Jt = D(() => !Gt.value && Y.value.showZoom), { viewBox: Yt, resetZoom: Xt, isZoom: Zt, setInitialViewBox: Qt, zoomByFactor: $t, scale: en } = oe(K, {
			x: 0,
			y: 0,
			width: q.value.width <= 0 ? 10 : q.value.width,
			height: q.value.height <= 0 ? 10 : q.value.height
		}, 1, Jt);
		we(() => B.dataset, () => {
			_t.value = vt(), W.value.responsive || (Rt(), wt());
		}, { immediate: !0 });
		async function tn({ scale: e = 2 } = {}) {
			if (!H.value) return;
			let { width: t, height: n } = H.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await C({
				domElement: H.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: W.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let nn = D(() => {
			let e = W.value.table.useDialog && !W.value.table.show, t = Y.value.showTable;
			return {
				component: e ? Ne : Oe,
				title: `${W.value.style.chart.title.text}${W.value.style.chart.title.subtitle.text ? `: ${W.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: W.value.table.th.backgroundColor,
					color: W.value.table.th.color,
					headerColor: W.value.table.th.color,
					headerBg: W.value.table.th.backgroundColor,
					isFullscreen: Q.value,
					fullscreenParent: H.value,
					forcedWidth: Math.min(500, window.innerWidth * .8),
					isCursorPointer: G.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: W.value.style.chart.backgroundColor,
							color: W.value.style.chart.color
						},
						head: {
							backgroundColor: W.value.style.chart.backgroundColor,
							color: W.value.style.chart.color
						}
					}
				}
			};
		});
		we(() => Y.value.showTable, (e) => {
			W.value.table.show || (e && W.value.table.useDialog && Ge.value ? Ge.value.open() : "close" in Ge.value && Ge.value.close());
		});
		function rn() {
			Y.value.showTable = !1, Ke.value && Ke.value.setTableIconState(!1);
		}
		let an = D(() => W.value.style.chart.backgroundColor), on = D(() => W.value.style.chart.title), { isCallbackImaging: sn, isCallbackSvg: cn, generateSvg: ln, onGenerateImage: un } = S({
			svg: K,
			title: on,
			legend: null,
			legendItems: null,
			backgroundColor: an,
			getSvgCallback: () => W.value.userOptions.callbacks.svg,
			generateImage: Ft
		});
		function dn() {
			$t(1.5, !0);
		}
		function fn() {
			$t(1 / 1.5, !0);
		}
		let $ = I(null), pn = I(!1), mn = I(""), hn = I(null);
		function gn() {
			$.value = null, U.value = null, We.value = !1;
		}
		function _n(e, t) {
			W.value.events.datapointLeave && W.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}), (U.value !== t || dt.value !== "keyboard") && ($.value = null, We.value = !1);
		}
		function vn(e, t) {
			W.value.events.datapointClick && W.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function yn(e) {
			if (!K.value || e == null) return;
			let t = K.value.querySelector(`[data-a11y-word-index="${e}-${V.value}"]`);
			if (!t) return;
			let n = t.getBoundingClientRect();
			ut.value = {
				x: n.left + n.width / 2,
				y: n.top + n.height / 2
			};
		}
		function bn(e, t, n = "pointer") {
			if (W.value.events.datapointEnter && W.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), !Y.value.showTooltip) return;
			dt.value = n, U.value = t, $.value = e.id, hn.value = {
				datapoint: e,
				config: W.value,
				seriesIndex: t
			};
			let r = W.value.style.chart.tooltip.customFormat;
			if (pn.value = !1, h(r)) try {
				let t = r({
					datapoint: e,
					config: W.value
				});
				typeof t == "string" && (mn.value = t, pn.value = !0);
			} catch {
				console.warn("Custom format cannot be applied."), pn.value = !1;
			}
			if (!pn.value) {
				let t = `<svg viewBox="0 0 10 10" height="${W.value.style.chart.tooltip.fontSize}"><circle cx="5" cy="5" r="5" fill="${e.color}"/></svg><span>${e.name}:</span><b>${(e.value || 0).toFixed(W.value.style.chart.tooltip.roundingValue)}</b>`;
				mn.value = `<div dir="auto" style="display:flex; gap:4px; align-items:center; jsutify-content:center;">${t}</div>`;
			}
			n === "keyboard" && me(() => {
				yn(t);
			}), We.value = !0;
		}
		async function xn() {
			if (Le("copyAlt", {
				config: W.value,
				dataset: X.value
			}), !W.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(W.value.userOptions.callbacks.altCopy({
				config: W.value,
				dataset: X.value
			}));
		}
		function Sn() {
			U.value = null, ft.value = !0;
		}
		function Cn() {
			gn(), ft.value = !1;
		}
		function wn(e) {
			if (!K.value || Gt.value || document.activeElement !== K.value || !X.value.length) return;
			let t = e.key === "ArrowLeft" || e.key === "ArrowUp", n = e.key === "ArrowRight" || e.key === "ArrowDown", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				gn();
				return;
			}
			if (r) {
				if (U.value === null) return;
				let e = X.value[U.value];
				if (!e) return;
				vn(e, U.value);
				return;
			}
			let a = U.value;
			a === null || a < 0 || a >= X.value.length ? a = n ? 0 : X.value.length - 1 : (a += n ? 1 : -1, a < 0 && (a = X.value.length - 1), a >= X.value.length && (a = 0));
			let o = X.value[a];
			o && bn(o, a, "keyboard");
		}
		let Tn = D(() => ({
			headers: Z.value?.colNames ?? [],
			rows: Z.value?.a11yBody ?? []
		}));
		return T({
			getData: Ht,
			getImage: tn,
			generateCsv: Bt,
			generatePdf: Pt,
			generateImage: Ft,
			generateSvg: ln,
			resetZoom: Xt,
			toggleTable: Ut,
			toggleTooltip: Wt,
			toggleAnnotator: Kt,
			toggleFullscreen: Vt,
			toggleZoom: qt,
			copyAlt: xn
		}), (e, t) => (F(), A("div", {
			class: "vue-data-ui-component vue-ui-word-cloud",
			ref_key: "wordCloudChart",
			ref: H,
			id: `wordCloud_${V.value}`,
			"data-resizing": Et.value,
			"data-relayout": Je.value,
			style: ge(`width: 100%; font-family:${W.value.style.fontFamily};background:${W.value.style.chart.backgroundColor};${W.value.responsive ? "height:100%" : ""}`),
			onMouseenter: t[3] ||= () => R(bt)(!0),
			onMouseleave: t[4] ||= () => {
				R(bt)(!1), ft.value || gn();
			}
		}, [
			j("div", {
				id: `chart-instructions-${V.value}`,
				class: "sr-only"
			}, [j("p", null, Se(W.value.a11y.translations.keyboardNavigation), 1)], 8, Qe),
			Tn.value?.rows?.length ? (F(), O(ie, {
				key: 0,
				uid: V.value,
				head: Tn.value.headers,
				body: Tn.value.rows,
				notice: W.value.a11y.translations.tableAvailable,
				caption: W.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : k("", !0),
			W.value.userOptions.buttons.annotator ? (F(), O(R(Ae), {
				key: 1,
				svgRef: R(K),
				backgroundColor: W.value.style.chart.backgroundColor,
				color: W.value.style.chart.color,
				active: Gt.value,
				isCursorPointer: G.value,
				onClose: Kt
			}, {
				"annotator-action-close": z(() => [L(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": z(({ color: t }) => [L(e.$slots, "annotator-action-color", P(N({ color: t })), void 0, !0)]),
				"annotator-action-draw": z(({ mode: t }) => [L(e.$slots, "annotator-action-draw", P(N({ mode: t })), void 0, !0)]),
				"annotator-action-undo": z(({ disabled: t }) => [L(e.$slots, "annotator-action-undo", P(N({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": z(({ disabled: t }) => [L(e.$slots, "annotator-action-redo", P(N({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": z(({ disabled: t }) => [L(e.$slots, "annotator-action-delete", P(N({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : k("", !0),
			W.value.style.chart.title.text ? (F(), A("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: Be,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(F(), O(w, {
				key: `title_${He.value}`,
				config: {
					title: { ...W.value.style.chart.title },
					subtitle: { ...W.value.style.chart.title.subtitle }
				}
			}, null, 8, ["config"]))], 512)) : k("", !0),
			W.value.userOptions.show && Re.value && (R(xt) || R(yt)) ? (F(), O(R(je), {
				ref_key: "userOptionsRef",
				ref: Ke,
				key: `user_option_${ze.value}`,
				backgroundColor: W.value.style.chart.backgroundColor,
				color: W.value.style.chart.color,
				isPrinting: R(Mt),
				isImaging: R(Nt),
				uid: V.value,
				hasPdf: W.value.userOptions.buttons.pdf,
				hasXls: W.value.userOptions.buttons.csv,
				hasImg: W.value.userOptions.buttons.img,
				hasSvg: W.value.userOptions.buttons.svg,
				hasTable: W.value.userOptions.buttons.table,
				hasFullscreen: W.value.userOptions.buttons.fullscreen,
				hasAltCopy: W.value.userOptions.buttons.altCopy,
				isFullscreen: Q.value,
				titles: { ...W.value.userOptions.buttonTitles },
				chartElement: H.value,
				position: W.value.userOptions.position,
				hasTooltip: W.value.style.chart.tooltip.show && W.value.userOptions.buttons.tooltip,
				isTooltip: Y.value.showTooltip,
				hasAnnotator: W.value.userOptions.buttons.annotator,
				isAnnotation: Gt.value,
				callbacks: W.value.userOptions.callbacks,
				printScale: W.value.userOptions.print.scale,
				tableDialog: W.value.table.useDialog,
				hasZoom: W.value.userOptions.buttons.zoom,
				isZoom: Y.value.showZoom,
				isCursorPointer: G.value,
				onToggleFullscreen: Vt,
				onGeneratePdf: R(Pt),
				onGenerateCsv: Bt,
				onGenerateImage: R(un),
				onGenerateSvg: R(ln),
				onToggleTable: Ut,
				onToggleTooltip: Wt,
				onToggleAnnotator: Kt,
				onToggleZoom: qt,
				onCopyAlt: xn,
				style: ge({ visibility: R(xt) ? R(yt) ? "visible" : "hidden" : "visible" })
			}, ue({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: z(({ isOpen: t, color: n }) => [L(e.$slots, "menuIcon", P(N({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: z(() => [L(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: z(() => [L(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: z(() => [L(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: z(() => [L(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: z(() => [L(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: z(() => [L(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: z(({ toggleFullscreen: t, isFullscreen: n }) => [L(e.$slots, "optionFullscreen", P(N({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: z(({ toggleAnnotator: t, isAnnotator: n }) => [L(e.$slots, "optionAnnotator", P(N({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionZoom ? {
					name: "optionZoom",
					fn: z(({ toggleZoom: t, isZoomLocked: n }) => [L(e.$slots, "optionZoom", P(N({
						toggleZoom: t,
						isZoomLocked: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: z(({ altCopy: t }) => [L(e.$slots, "optionAltCopy", P(N({ altCopy: t })), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: z(() => [L(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: z(() => [L(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "12"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasFullscreen.hasAltCopy.isFullscreen.titles.chartElement.position.hasTooltip.isTooltip.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.hasZoom.isZoom.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : k("", !0),
			W.value.style.chart.controls.position === "top" && W.value.style.chart.controls.show && !R(mt) ? (F(), O(ce, {
				key: 4,
				ref_key: "zoomControls",
				ref: Ye,
				config: W.value,
				scale: R(en),
				isFullscreen: Q.value,
				isCursorPointer: G.value,
				onZoomIn: dn,
				onZoomOut: fn,
				onResetZoom: t[0] ||= (e) => R(Xt)(!0)
			}, null, 8, [
				"config",
				"scale",
				"isFullscreen",
				"isCursorPointer"
			])) : k("", !0),
			j("div", $e, [(F(), A("svg", {
				ref_key: "svgRef",
				ref: K,
				class: he({
					"vue-data-ui-fullscreen--on": Q.value,
					"vue-data-ui-fulscreen--off": !Q.value
				}),
				"aria-describedby": `chart-instructions-${V.value}`,
				xmlns: R(p),
				viewBox: `${R(Yt).x} ${R(Yt).y} ${R(Yt).width} ${R(Yt).height}`,
				style: "overflow:hidden;background:transparent;display:block",
				tabindex: "0",
				onFocus: Sn,
				onBlur: Cn,
				onKeydown: wn
			}, [
				fe(R(Me)),
				e.$slots["chart-background"] ? (F(), A("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: q.value.width <= 0 ? 10 : q.value.width,
					height: q.value.height <= 0 ? 10 : q.value.height,
					style: { pointerEvents: "none" }
				}, [L(e.$slots, "chart-background", {}, void 0, !0)], 8, tt)) : k("", !0),
				j("g", {
					transform: `translate(${Ct.value.x}, ${Ct.value.y})`,
					class: he({ "wc-finalized": qe.value })
				}, [(F(!0), A(le, null, ye(X.value, (e, t) => (F(), A("g", {
					key: e.id,
					class: "vue-ui-word-cloud-word",
					transform: `translate(${e.x}, ${e.y})`
				}, [e.minX === void 0 ? k("", !0) : (F(), A("rect", {
					key: 0,
					"data-a11y-word-index": `${t}-${V.value}`,
					x: e.minX,
					y: e.minY * 1.25,
					width: e.maxX - e.minX,
					height: e.maxY - e.minY,
					fill: "transparent",
					"pointer-events": "visiblePainted",
					"aria-label": `${e.name}: ${(e.value || 0).toFixed(W.value.style.chart.tooltip.roundingValue)}`,
					onMouseover: (n) => bn(e, t),
					onMouseleave: (n) => _n(e, t),
					onClick: (n) => vn(e, t)
				}, null, 40, it)), j("text", {
					fill: e.color,
					"font-weight": W.value.style.chart.words.bold ? "bold" : "normal",
					x: 0,
					y: 0,
					"font-size": e.fontSize,
					transform: `translate(${e.width / 2}, ${e.height / 2})`,
					"text-anchor": "middle",
					"dominant-baseline": "central",
					"paint-order": "stroke fill",
					stroke: !$.value || $.value === e.id ? W.value.style.chart.words.selectedStroke : void 0,
					"stroke-width": e.height * .05,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					style: ge(`
                                pointer-events:none;
                                fill-opacity:${!$.value || $.value === e.id || !qe.value ? 1 : W.value.style.chart.words.hoverOpacity} !important;
                            `)
				}, Se(e.name), 13, at)], 8, rt))), 128))], 10, nt),
				L(e.$slots, "svg", { svg: {
					...q.value,
					isPrintingImg: R(Mt) || R(Nt) || R(sn),
					isPrintingSvg: R(cn)
				} }, void 0, !0)
			], 42, et)), e.$slots.hint ? (F(), A("div", ot, [L(e.$slots, "hint", P(N({
				hint: W.value.a11y.translations.keyboardNavigation,
				isVisible: ft.value
			})), void 0, !0)])) : k("", !0)]),
			e.$slots.watermark ? (F(), A("div", st, [L(e.$slots, "watermark", P(N({ isPrinting: R(Mt) || R(Nt) || R(sn) || R(cn) })), void 0, !0)])) : k("", !0),
			R(Zt) ? (F(), A("div", ct, [L(e.$slots, "reset-action", { reset: R(Xt) }, void 0, !0)])) : k("", !0),
			W.value.style.chart.controls.position === "bottom" && W.value.style.chart.controls.show && !R(mt) ? (F(), O(ce, {
				key: 7,
				ref_key: "zoomControls",
				ref: Ye,
				config: W.value,
				scale: R(en),
				isFullscreen: Q.value,
				isCursorPointer: G.value,
				onZoomIn: dn,
				onZoomOut: fn,
				onResetZoom: t[1] ||= (e) => R(Xt)(!0)
			}, null, 8, [
				"config",
				"scale",
				"isFullscreen",
				"isCursorPointer"
			])) : k("", !0),
			fe(R(Ee), {
				teleportTo: W.value.style.chart.tooltip.teleportTo,
				show: Y.value.showTooltip && We.value,
				backgroundColor: W.value.style.chart.tooltip.backgroundColor,
				color: W.value.style.chart.tooltip.color,
				fontSize: W.value.style.chart.tooltip.fontSize,
				borderRadius: W.value.style.chart.tooltip.borderRadius,
				borderColor: W.value.style.chart.tooltip.borderColor,
				borderWidth: W.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: W.value.style.chart.tooltip.backgroundOpacity,
				position: W.value.style.chart.tooltip.position,
				offsetX: W.value.style.chart.tooltip.offsetX,
				offsetY: W.value.style.chart.tooltip.offsetY,
				parent: H.value,
				content: mn.value,
				isCustom: pn.value,
				isFullscreen: Q.value,
				smooth: W.value.style.chart.tooltip.smooth,
				backdropFilter: W.value.style.chart.tooltip.backdropFilter,
				smoothForce: W.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: W.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: dt.value === "keyboard",
				a11yPosition: ut.value
			}, {
				"tooltip-before": z(() => [L(e.$slots, "tooltip-before", P(N({ ...hn.value })), void 0, !0)]),
				tooltip: z(() => [L(e.$slots, "tooltip", P(N({ ...hn.value })), void 0, !0)]),
				"tooltip-after": z(() => [L(e.$slots, "tooltip-after", P(N({ ...hn.value })), void 0, !0)]),
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
			e.$slots.source ? (F(), A("div", {
				key: 8,
				ref_key: "source",
				ref: Ve,
				dir: "auto"
			}, [L(e.$slots, "source", {}, void 0, !0)], 512)) : k("", !0),
			Re.value && W.value.userOptions.buttons.table ? (F(), O(be(nn.value.component), pe({ key: 9 }, nn.value.props, {
				ref_key: "tableUnit",
				ref: Ge,
				onClose: rn
			}), ue({
				content: z(() => [(F(), O(R(ke), {
					key: `table_${Ue.value}`,
					colNames: Z.value.colNames,
					head: Z.value.head,
					body: Z.value.body,
					config: Z.value.config,
					title: W.value.table.useDialog ? "" : nn.value.title,
					withCloseButton: !W.value.table.useDialog,
					isCursorPointer: G.value,
					onClose: rn
				}, {
					th: z(({ th: e }) => [j("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, lt)]),
					td: z(({ td: e }) => [de(Se(e.name || e), 1)]),
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
			}, [W.value.table.useDialog ? {
				name: "title",
				fn: z(() => [de(Se(nn.value.title), 1)]),
				key: "0"
			} : void 0, W.value.table.useDialog ? {
				name: "actions",
				fn: z(() => [j("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[2] ||= (e) => Bt(W.value.userOptions.callbacks.csv),
					style: ge({ cursor: G.value ? "pointer" : "default" })
				}, [fe(R(De), {
					name: "fileCsv",
					stroke: nn.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : k("", !0),
			L(e.$slots, "skeleton", {}, () => [R(mt) ? (F(), O(te, { key: 0 })) : k("", !0)], !0)
		], 44, Ze));
	}
}, [["__scopeId", "data-v-d55cdbfc"]]);
//#endregion
export { U as n, ut as t };
