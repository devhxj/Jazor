import { isRef as e, toRaw as t, unref as n } from "vue";
var r = {
	dataset: "#COMP# dataset prop is either missing, undefined or empty.",
	datasetAttribute: "#COMP# dataset is missing the '#ATTR#' attribute.",
	datasetAttributeEmpty: "#COMP# dataset '#ATTR#' attribute cannot be empty.",
	datasetSerieAttribute: "#COMP# dataset #KEY# item at index #INDX# is missing the '#ATTR#' attribute.",
	notBuildable: "#COMP# : Chart could not be built. Dataset is not formatted correctly",
	attributeWrongValue: "#COMP# : A wrong value was provided to the #ATTR# attribute (#KEY# is not a valid value)."
};
//#endregion
//#region src/lib.js
function i(e, t, n, r, i, a = 1.99999, o = 2, s = 1.45, l = 360, u = 105.25, d = 0) {
	let { series: f } = e;
	if (!f || f.length === 0) return [];
	let p = f.reduce((e, t) => e + t.value, 0), m = [], g = 0;
	for (let e = 0; e < f.length; e++) {
		let _ = f[e].value, v = f.length === 1, y = v ? 1 : p > 0 ? _ / p : 0, b = Math.PI * a * y, x = (v ? .5 : p > 0 ? _ / 2 / p : .5) * (Math.PI * o), { startX: S, startY: C, endX: w, endY: T, path: E } = c([t, n], [r, i], [g, b], u, l, o), D = c([t, n], [r - d, i - d], [g, b], u, l, o, !0), O = c([t, n], [r * s, i * s], [g, x], u, l, o);
		m.push({
			arcSlice: `${E} L ${D.startX} ${D.startY} ${D.path} L ${S} ${C}`,
			cx: h(t),
			cy: h(n),
			...f[e],
			proportion: h(y),
			ratio: h(b),
			path: E.replaceAll("NaN", "0"),
			startX: h(S),
			startY: h(C),
			endX: h(w),
			endY: h(T),
			separator: {
				x: D.startX,
				y: D.startY
			},
			firstSeparator: {
				x: Number(D.path.split(" ").at(-2)),
				y: Number(D.path.split(" ").at(-1))
			},
			center: O
		}), g += b;
	}
	return m;
}
function a([e, t], [n, r]) {
	return [e + n, t + r];
}
function o([[e, t], [n, r]], [i, a]) {
	return [e * i + t * a, n * i + r * a];
}
function s(e) {
	return [[Math.cos(e), -Math.sin(e)], [Math.sin(e), Math.cos(e)]];
}
function c([e, t], [n, r], [i, c], l, u = 360, d = 2, f = !1) {
	c %= d * Math.PI;
	let p = s(l), [m, g] = a(o(p, [n * Math.cos(i), r * Math.sin(i)]), [e, t]), [_, v] = a(o(p, [n * Math.cos(i + c), r * Math.sin(i + c)]), [e, t]), y = +(c > Math.PI), b = c > 0 ? +!f : +!!f;
	return {
		startX: h(f ? _ : m),
		startY: h(f ? v : g),
		endX: h(f ? m : _),
		endY: h(f ? g : v),
		path: `M${h(f ? _ : m)} ${h(f ? v : g)} A ${[
			h(n),
			h(r),
			h(l / (d * Math.PI) * u),
			h(y),
			h(b),
			h(f ? m : _),
			h(f ? g : v)
		].join(" ")}`
	};
}
function l({ defaultConfig: e, userConfig: t }) {
	let n = { ...e };
	return Object.keys(n).forEach((e) => {
		if (Object.hasOwn(t, e)) {
			let r = t[e];
			r === null ? n[e] = null : ["boolean", "function"].includes(typeof r) ? n[e] = r : ["string", "number"].includes(typeof r) ? f(r) && (n[e] = r) : Array.isArray(n[e]) ? u({
				userConfig: t,
				key: e
			}) && (n[e] = r) : d({
				userConfig: t,
				key: e
			}) && (n[e] = l({
				defaultConfig: n[e],
				userConfig: r
			}));
		}
	}), Object.keys(t).forEach((e) => {
		if (!Object.hasOwn(n, e)) {
			let r = t[e];
			n[e] = r && typeof r == "object" && !Array.isArray(r) ? { ...r } : r;
		}
	}), n;
}
function u({ userConfig: e, key: t }) {
	return Object.hasOwn(e, t) && Array.isArray(e[t]) && e[t].length >= 0;
}
function d({ userConfig: e, key: t }) {
	return Object.hasOwn(e, t) && !Array.isArray(e[t]) && typeof e[t] == "object";
}
function f(e) {
	return ![
		null,
		void 0,
		NaN,
		Infinity,
		-Infinity
	].includes(e);
}
function p(e) {
	return ![
		void 0,
		NaN,
		Infinity,
		-Infinity
	].includes(e);
}
function m(e, t = 0) {
	return f(e) ? e : t;
}
function h(e, t = 0) {
	return isNaN(e) ? t : e;
}
var g = /* @__PURE__ */ "#1f77b4.#aec7e8.#ff7f0e.#ffbb78.#2ca02c.#98df8a.#d62728.#ff9896.#9467bd.#c5b0d5.#8c564b.#c49c94.#e377c2.#f7b6d2.#7f7f7f.#c7c7c7.#bcbd22.#dbdb8d.#17becf.#9edae5.#393b79.#5254a3.#6b6ecf.#9c9ede.#637939.#8ca252.#b5cf6b.#cedb9c.#8c6d31.#bd9e39.#e7ba52.#e7cb94.#843c39.#ad494a.#d6616b.#e7969c.#7b4173.#a55194.#ce6dbd.#de9ed6".split(".");
function _(e = "default") {
	switch (e) {
		case "hack": return v.hack;
		case "zen": return v.zen;
		case "concrete": return v.concrete;
		case "celebration": return v.celebration;
		case "celebrationNight": return v.celebrationNight;
		case "minimal": return v.minimal;
		case "minimalDark": return v.minimalDark;
		default: return v.default;
	}
}
var v = {
	default: g,
	dark: g,
	minimal: [
		"#2A2929",
		"#454862",
		"#65698E",
		"#8D99AE",
		"#678681",
		"#7FA09B",
		"#9CBCA8",
		"#76645D",
		"#877675",
		"#A9998C",
		"#C6B7AB",
		"#906C70",
		"#B08C91",
		"#C9ACB0",
		"#9F816B",
		"#B39783",
		"#D8C3B3",
		"#825E76",
		"#9D7D92",
		"#C2A6B9"
	],
	minimalDark: [
		"#524f4f",
		"#454862",
		"#65698E",
		"#8D99AE",
		"#678681",
		"#7FA09B",
		"#9CBCA8",
		"#76645D",
		"#877675",
		"#A9998C",
		"#C6B7AB",
		"#906C70",
		"#B08C91",
		"#C9ACB0",
		"#9F816B",
		"#B39783",
		"#D8C3B3",
		"#825E76",
		"#9D7D92",
		"#C2A6B9"
	],
	celebration: [
		"#D32F2F",
		"#E64A19",
		"#F57C00",
		"#FF9800",
		"#FF5722",
		"#FFC107",
		"#FFEB3B",
		"#FFD54F",
		"#FF6F00",
		"#D84315",
		"#BF360C",
		"#C62828",
		"#B71C1C",
		"#FF7043",
		"#FF8A65",
		"#FFB74D",
		"#FFA726",
		"#FFCC80",
		"#FFE082",
		"#FFECB3"
	],
	celebrationNight: [
		"#D32F2F",
		"#E64A19",
		"#F57C00",
		"#FF9800",
		"#FF5722",
		"#FFC947",
		"#FFEB3B",
		"#FFD95B",
		"#FF8800",
		"#FF5722",
		"#DD2C00",
		"#F44336",
		"#C62828",
		"#FF6E6E",
		"#FF867C",
		"#FFB547",
		"#FFA837",
		"#FFD180",
		"#FFE57F",
		"#FFF59D"
	],
	concrete: /* @__PURE__ */ "#4A6A75.#6C94A0.#7DA9B5.#8EBFCA.#9FD4E0.#B0E9F5.#C1FFFF.#5C6B5B.#6D7D6D.#7E8F7E.#8FA290.#A1B5A3.#B2C7B5.#C3DAC8.#D4ECDA.#E6FFF0.#8A9CA5.#9AA7B0.#ABB1BC.#BBCBC7.#CCD6D3.#DEE1DE.#EFECEC.#404C4D.#50605F.#617472.#718885.#829C98.#92B0AB.#A3C4BE.#B3D8D2.#C4EDE5.#D4F1E8.#404C5A.#50606C.#61747E.#718890.#829CA2.#92B0B5".split("."),
	hack: /* @__PURE__ */ "#004C00.#006600.#008000.#009900.#00B300.#00CC00.#00E600.#00FF00.#33FF33.#33E633.#33CC33.#33B333.#339933.#338033.#336633.#334C33.#333333.#00AF19.#19E619.#19CC19.#19B319.#199919.#198019.#196619.#194C19.#193319.#191919.#66FF66.#66E666.#66CC66.#66B366.#669966.#668066.#666666.#4CFF4C.#4CE64C.#4CCC4C.#4CB34C".split("."),
	zen: /* @__PURE__ */ "#B9B99D.#E0CFC3.#DFCA99.#DCB482.#C09E85.#8F837A.#858480.#B0B9A8.#606C5A.#5E5E5E.#4F5B75.#647393.#818EA9.#9FA9BE.#BBC4D3.#DCDFE7.#928A98.#8A9892.#B1A7AD.#C5B8A7.#EBD6CC.#D7E0D2.#E0D2D7.#E0DBD2.#D2E0DB.#DBD2E0.#C1B7A5.#A5AFC1.#E0DBD2.#D2D7E0.#F7EDE2.#97ACB7.#C4CBBC.#C3C5C5.#A0AC94".split(".")
}, y = /* @__PURE__ */ "00.03.05.08.0A.0D.0F.12.14.17.1A.1C.1F.21.24.26.29.2B.2E.30.33.36.38.3B.3D.40.42.45.47.4A.4D.4F.52.54.57.59.5C.5E.61.63.66.69.6B.6E.70.73.75.78.7A.7D.80.82.85.87.8A.8C.8F.91.94.96.99.9C.9E.A1.A3.A6.A8.AB.AD.B0.B3.B5.B8.BA.BD.BF.C2.C4.C7.C9.CC.CF.D1.D4.D6.D9.DB.DE.E0.E3.E6.E8.EB.ED.F0.F2.F5.F7.FA.FC.FF".split(".");
function b(t) {
	let r = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})?$/i, i = /^#?([a-f\d])([a-f\d])([a-f\d])([a-f\d])?$/i, a = /^rgba?\(\s*([+\-]?\d*\.?\d+%?)(?:\s*,\s*|\s+)([+\-]?\d*\.?\d+%?)(?:\s*,\s*|\s+)([+\-]?\d*\.?\d+%?)(?:\s*(?:,|\/)\s*([+\-]?\d*\.?\d+%?))?\s*\)$/i, o = /^hsla?\(\s*([+\-]?\d*\.?\d+(?:deg)?)(?:\s*,\s*|\s+)([\d.]+)%(?:\s*,\s*|\s+)([\d.]+)%(?:\s*(?:,|\/)\s*([\d.]+%?))?\s*\)$/i, s = /^oklch\(\s*([\d.]+)%?\s*[, ]\s*([\d.]+)%?\s*[, ]\s*([\d.]+)(?:deg)?\s*(?:\/\s*([\d.]+%?)\s*)?\)$/i, c = /^lch\(/i;
	if (t == null || typeof t == "number" && isNaN(t)) return null;
	if (t = e?.(t) ? n(t) : t, t = We(t), Array.isArray(t)) {
		let [e, n, r, i = 1] = t;
		t = `rgba(${e},${n},${r},${i})`;
	} else if (typeof t == "object") if (Number.isFinite(t.r) && Number.isFinite(t.g) && Number.isFinite(t.b)) {
		let e = Number.isFinite(t.a) ? t.a : 1;
		t = `rgba(${t.r},${t.g},${t.b},${e})`;
	} else return null;
	else if (typeof t == "number") {
		let e = t >>> 0, n = e.toString(16).padStart(e <= 16777215 ? 6 : 8, "0");
		return `#${n.length === 6 ? n + "ff" : n}`;
	} else if (typeof t != "string") return null;
	if (t = t.trim(), c.test(t)) return console.warn("[convertColorToHex] lch() colors are not supported. Use oklch() instead."), null;
	if (t.toLowerCase() === "transparent") return "#FFFFFF00";
	t = t.replace(i, (e, t, n, r, i) => `#${t}${t}${n}${n}${r}${r}${i ? i + i : ""}`);
	let l;
	if (l = t.match(r)) {
		let [, e, t, n, r] = l, i = r ? parseInt(r, 16) / 255 : 1;
		return `#${e}${t}${n}${k(Math.round(S(i) * 255))}`;
	}
	if (l = t.match(a)) {
		let [, e, t, n, r] = l, i = x(e), a = x(t), o = x(n), s = r ? D(r) : 1;
		return i === null || a === null || o === null || s === null ? null : `#${k(i)}${k(a)}${k(o)}${k(Math.round(S(s) * 255))}`;
	}
	if (l = t.match(o)) {
		let [, e, t, n, r] = l, i = parseFloat(e), a = Number(t), o = Number(n), s = r ? D(r) : 1;
		if (!Number.isFinite(i) || !Number.isFinite(a) || !Number.isFinite(o) || s === null) return null;
		let [c, u, d] = ne(i, a, o);
		return `#${k(c)}${k(u)}${k(d)}${k(Math.round(S(s) * 255))}`;
	}
	if (l = t.match(s)) {
		let [, e, t, n, r] = l, i = O(e);
		if (i === null) return null;
		let a = ee(t);
		if (a === null) return null;
		let o = te(n);
		if (o === null) return null;
		let s = D(r);
		if (s === null) return null;
		let [c, u, d] = T(i, a, o);
		return `#${k(c)}${k(u)}${k(d)}${k(Math.round(S(s) * 255))}`;
	}
	return null;
}
function x(e) {
	if (typeof e != "string") return null;
	if (e.endsWith("%")) {
		let t = parseFloat(e);
		return Number.isFinite(t) ? Math.round(S(t / 100) * 255) : null;
	}
	let t = parseFloat(e);
	return Number.isFinite(t) ? t < 0 ? 0 : t > 255 ? 255 : Math.round(t) : null;
}
function S(e) {
	return Number.isFinite(e) ? e < 0 ? 0 : e > 1 ? 1 : e : 0;
}
function C(e) {
	return !Number.isFinite(e) || e < 0 ? 0 : e > 255 ? 255 : Math.round(e);
}
function w(e) {
	return e <= .0031308 ? 12.92 * e : 1.055 * e ** (1 / 2.4) - .055;
}
function T(e, t, n) {
	let r = (n % 360 + 360) % 360 * (Math.PI / 180);
	return E(e, t * Math.cos(r), t * Math.sin(r));
}
function E(e, t, n) {
	let r = e + .3963377774 * t + .2158037573 * n, i = e - .1055613458 * t - .0638541728 * n, a = e - .0894841775 * t - 1.291485548 * n, o = r * r * r, s = i * i * i, c = a * a * a, l = 4.0767416621 * o - 3.3077115913 * s + .2309699292 * c, u = -1.2684380046 * o + 2.6097574011 * s - .3413193965 * c, d = -.0041960863 * o - .7034186147 * s + 1.707614701 * c;
	l = S(l), u = S(u), d = S(d);
	let f = w(l) * 255, p = w(u) * 255, m = w(d) * 255;
	return [
		C(f),
		C(p),
		C(m)
	];
}
function D(e) {
	if (e === void 0) return 1;
	if (typeof e == "string" && e.endsWith("%")) {
		let t = parseFloat(e);
		return Number.isFinite(t) ? S(t / 100) : null;
	}
	let t = parseFloat(e);
	return Number.isFinite(t) ? S(t) : null;
}
function O(e) {
	let t = Number(e);
	return Number.isFinite(t) ? (t > 1 && (t /= 100), S(t)) : null;
}
function ee(e) {
	let t = Number(e);
	return Number.isFinite(t) ? (t > 1 && (t /= 100), t < 0 ? 0 : t) : null;
}
function te(e) {
	let t = Number(e);
	return Number.isFinite(t) ? t : null;
}
function k(e) {
	let t = Number(e).toString(16);
	return t.length === 1 ? "0" + t : t;
}
function ne(e, t, n, r = 1) {
	e /= 360, t /= 100, n /= 100;
	let i, a, o;
	if (t === 0) i = a = o = n;
	else {
		let r = (e, t, n) => (n < 0 && (n += 1), n > 1 && --n, n < 1 / 6 ? e + (t - e) * 6 * n : n < 1 / 2 ? t : n < 2 / 3 ? e + (t - e) * (2 / 3 - n) * 6 : e), s = n < .5 ? n * (1 + t) : n + t - n * t, c = 2 * n - s;
		i = r(c, s, e + 1 / 3), a = r(c, s, e), o = r(c, s, e - 1 / 3);
	}
	return [
		Math.round(i * 255),
		Math.round(a * 255),
		Math.round(o * 255),
		r
	];
}
function A(e, t) {
	let n = e.length === 9 ? e.substring(0, 7) : e, r = e.length === 9 ? e.substring(7, 9) : null, i = (e) => ({
		r: parseInt(e.substring(1, 3), 16),
		g: parseInt(e.substring(3, 5), 16),
		b: parseInt(e.substring(5, 7), 16)
	}), a = ({ r: e, g: t, b: n }) => {
		e /= 255, t /= 255, n /= 255;
		let r = Math.max(e, t, n), i = Math.min(e, t, n), a, o, s = (r + i) / 2;
		if (r === i) a = o = 0;
		else {
			let c = r - i;
			switch (o = s > .5 ? c / (2 - r - i) : c / (r + i), r) {
				case e:
					a = (t - n) / c + (t < n ? 6 : 0);
					break;
				case t:
					a = (n - e) / c + 2;
					break;
				case n: a = (e - t) / c + 4;
			}
			a /= 6;
		}
		return {
			h: a,
			s: o,
			l: s
		};
	}, o = ({ h: e, s: t, l: n }) => {
		let r, i, a;
		if (t === 0) r = i = a = n;
		else {
			let o = (e, t, n) => (n < 0 && (n += 1), n > 1 && --n, n < 1 / 6 ? e + (t - e) * 6 * n : n < 1 / 2 ? t : n < 2 / 3 ? e + (t - e) * (2 / 3 - n) * 6 : e), s = n < .5 ? n * (1 + t) : n + t - n * t, c = 2 * n - s;
			r = o(c, s, e + 1 / 3), i = o(c, s, e), a = o(c, s, e - 1 / 3);
		}
		return {
			r: Math.round(r * 255),
			g: Math.round(i * 255),
			b: Math.round(a * 255)
		};
	}, s = a(i(n || "#000000"));
	s.h += t, s.h = (s.h + 1) % 1;
	let c = o(s);
	return `#${(c.r << 16 | c.g << 8 | c.b).toString(16).padStart(6, "0")}` + (r || "");
}
function re({ centerX: e, centerY: t, outerPoints: n, radius: r, rotation: i }) {
	let a = Math.PI / n, o = i, s = "", c = [];
	for (let i = 0; i < n * 2; i += 1) {
		let n = e + Math.cos(i * a + o) * r, l = t + Math.sin(i * a + o) * r;
		s += `${n},${l} `, c.push({
			x: n,
			y: l
		});
	}
	return {
		path: `M${s}Z`,
		coordinates: c
	};
}
function ie({ plot: e, radius: t, sides: n, rotation: r = 0 }) {
	let i = e.x, a = e.y;
	return re({
		centerX: i,
		centerY: a,
		outerPoints: n / 2,
		radius: t + 1,
		rotation: r
	});
}
function ae({ centerX: e, centerY: t, innerCirclePoints: n, innerRadius: r, outerRadius: i }) {
	let a = Math.PI / n, o = n * 2, s = "";
	for (let n = 0; n < o; n += 1) {
		let o = n % 2 == 0 ? i : r, c = e + Math.cos(n * a + 60) * o, l = t + Math.sin(n * a + 60) * o;
		s += `${c},${l} `;
	}
	return s;
}
function oe({ plot: e, radius: t, apexes: n = 5 }) {
	let r = e.x, i = e.y, a = n, o = t * 3.5 / a;
	return ae({
		centerX: r,
		centerY: i,
		innerCirclePoints: a,
		innerRadius: o,
		outerRadius: o * 2
	});
}
function se({ series: e }) {
	if (!Array.isArray(e) || e.length === 0) return "";
	let t = Array.from(new Map(e.filter((e) => e && Number.isFinite(e.x) && Number.isFinite(e.y)).map((e) => [`${e.x},${e.y}`, {
		x: +e.x,
		y: +e.y
	}])).values());
	if (t.length === 0) return "";
	if (t.length === 1) return `${Math.round(t[0].x)},${Math.round(t[0].y)} `;
	let n = (e, t) => {
		let n = e.x - t.x, r = e.y - t.y;
		return n * n + r * r;
	}, r = (e, t, n) => (t.x - e.x) * (n.y - e.y) - (t.y - e.y) * (n.x - e.x), i = t[0];
	for (let e of t) (e.x < i.x || e.x === i.x && e.y < i.y) && (i = e);
	let a = [i], o = i, s = t.length + 2, c = 0;
	for (; !(++c > s);) {
		let e = t[0] === o ? t[1] : t[0];
		for (let i of t) {
			if (i === o || i === e) continue;
			let t = r(o, e, i);
			t < 0 || (t > 0 || n(o, i) > n(o, e)) && (e = i);
		}
		if (e === i) break;
		a.push(e), o = e;
	}
	let l = "";
	for (let e of a) l += `${Math.round(e.x)},${Math.round(e.y)} `;
	return l;
}
function j(e) {
	return e * Math.PI / 180;
}
function ce(e, t, n) {
	return Math.min(Math.max(e, t), n);
}
function le(e) {
	if (typeof e != "string") return null;
	let t = e.trim();
	if (!t.startsWith("#")) return null;
	let n = t.slice(1);
	return n.length === 3 ? {
		red: Number.parseInt(n[0] + n[0], 16),
		green: Number.parseInt(n[1] + n[1], 16),
		blue: Number.parseInt(n[2] + n[2], 16),
		alpha: 1
	} : n.length === 6 ? {
		red: Number.parseInt(n.slice(0, 2), 16),
		green: Number.parseInt(n.slice(2, 4), 16),
		blue: Number.parseInt(n.slice(4, 6), 16),
		alpha: 1
	} : n.length === 8 ? {
		red: Number.parseInt(n.slice(0, 2), 16),
		green: Number.parseInt(n.slice(2, 4), 16),
		blue: Number.parseInt(n.slice(4, 6), 16),
		alpha: Number.parseInt(n.slice(6, 8), 16) / 255
	} : null;
}
function ue(e, t, n) {
	let r = e / 255, i = t / 255, a = n / 255, o = (e) => e <= .03928 ? e / 12.92 : ((e + .055) / 1.055) ** 2.4, s = o(r), c = o(i), l = o(a);
	return .2126 * s + .7152 * c + .0722 * l;
}
function de(e, t = {
	dark: "#000000",
	light: "#FFFFFF"
}) {
	if (!e) return t?.dark ?? "#000000";
	let n = le(b(e));
	if (!n) return t?.dark ?? "#000000";
	let r = ue(n.red, n.green, n.blue);
	return (n.alpha < 1 ? n.alpha * r + (1 - n.alpha) * 1 : r) > .3 ? t?.dark ?? "#000000" : t?.light ?? "#FFFFFF";
}
function M(e) {
	return typeof e == "object" && !!e && Object.prototype.toString.call(e) === "[object Object]" && (e.constructor === Object || e.constructor == null);
}
function N(e) {
	return !!e && (e.__v_isRef || e.__v_isReactive || e.__v_isReadonly || e.effect || e.dep || e.deps || e.subs);
}
function P(e) {
	return e === "" ? "#000000" : e === "transparent" ? "#FFFFFF00" : b(e) ?? e;
}
function F(r, i = /* @__PURE__ */ new WeakSet()) {
	let a = t(r);
	if (!M(a) || i.has(a)) return a;
	i.add(a);
	for (let t in a) {
		let r = e(a[t]) ? n(a[t]) : a[t];
		if (t === "color" || t === "backgroundColor") {
			typeof r == "string" && (a[t] = P(r));
			continue;
		}
		if (t === "stroke") {
			typeof r == "string" ? a[t] = P(r) : M(r) && !N(r) && F(r, i);
			continue;
		}
		if (Array.isArray(r)) {
			for (let e of r) M(e) && !N(e) && F(e, i);
			continue;
		}
		M(r) && !N(r) && F(r, i);
	}
	return a;
}
function fe(e) {
	let t = e?.length ?? 0;
	if (t < 2) return {
		x1: 0,
		y1: 0,
		x2: 0,
		y2: 0,
		slope: 0,
		trend: 0
	};
	let n = 0, r = 0, i = 0, a = 0;
	for (let { x: t, y: o } of e) n += t, r += o, i += t * o, a += t * t;
	let o = t * a - n * n || 1, s = (t * i - n * r) / o, c = (r - s * n) / t, l = e[0].x, u = e[t - 1].x, d = s * l + c, f = s * u + c, p = 0, m = 0, h = 0, g = 0;
	for (let n = 0; n < t; n += 1) p += n, m += e[n].value, h += n * e[n].value, g += n * n;
	let _ = t * g - p * p || 1, v = (t * h - p * m) / _, y = (m - v * p) / t, b = y, x = v * (t - 1) + y, S = Math.max(Math.abs(b), Math.abs(m / t), Math.abs(x), 1e-9);
	return {
		x1: l,
		y1: d,
		x2: u,
		y2: f,
		slope: s,
		trend: (x - b) / S
	};
}
function pe(e) {
	if (!Array.isArray(e) || e.length === 0) return null;
	let t = e.filter(Number.isFinite);
	if (t.length === 0) return null;
	if (t.length === 1) return t[0];
	t.sort((e, t) => e - t);
	let n = Math.floor(t.length / 2);
	return t.length % 2 == 1 ? t[n] : t[n - 1] / 2 + t[n] / 2;
}
function me(e) {
	if (!Array.isArray(e) || e.length === 0) return null;
	let t = 0, n = 0;
	for (let r of e) Number.isFinite(r) && (n += 1, t = t * ((n - 1) / n) + r / n);
	return n === 0 ? null : t;
}
function he(e) {
	let t = [];
	for (let n = 0; n < e.length; n += 1) t.push(`${h(e[n].x)},${h(e[n].y)} `);
	return t.join(" ").trim();
}
function ge(e, t = null) {
	let n = (e) => e && e.value !== null && e.value !== void 0 && Number.isFinite(e.x) && Number.isFinite(e.y), r = [], i = [];
	for (let t of e) n(t) ? i.push(t) : (i.length && r.push(i), i = []);
	return i.length && r.push(i), r.map((e) => {
		if (!e.length) return "";
		let n = [`${h(e[0].x)},${h(e[0].y)}`];
		for (let t = 1; t < e.length; t += 1) {
			let r = e[t - 1], i = e[t];
			n.push(`L${h(i.x)},${h(r.y)}`, `L${h(i.x)},${h(i.y)}`);
		}
		return t != null && (n.unshift(`${h(e[0].x)},${h(t)}`), n.push(`${h(e.at(-1).x)},${h(t)}`)), n.join(" ");
	}).filter(Boolean).join(";");
}
function I(e) {
	if (e.length < 2) return "0,0";
	let t = e.length - 1, n = [`${h(e[0].x)},${h(e[0].y)}`], r = [], i = [], a = [], o = [];
	for (let n = 0; n < t; n += 1) r[n] = e[n + 1].x - e[n].x, i[n] = e[n + 1].y - e[n].y, a[n] = i[n] / r[n];
	o[0] = a[0], o[t] = a[t - 1];
	for (let e = 1; e < t; e += 1) if (a[e - 1] * a[e] <= 0) o[e] = 0;
	else {
		let t = 2 * a[e - 1] * a[e] / (a[e - 1] + a[e]);
		o[e] = t;
	}
	for (let r = 0; r < t; r += 1) {
		let t = e[r].x, i = e[r].y, a = e[r + 1].x, s = e[r + 1].y, c = o[r], l = o[r + 1], u = t + (a - t) / 3, d = i + c * (a - t) / 3, f = a - (a - t) / 3, p = s - l * (a - t) / 3;
		n.push(`C ${h(u)},${h(d)} ${h(f)},${h(p)} ${h(a)},${h(s)}`);
	}
	return n.join(" ");
}
function L(e, t = .2) {
	function n(e, t) {
		let n = t.x - e.x, r = t.y - e.y;
		return {
			length: Math.sqrt(n ** 2 + r ** 2),
			angle: Math.atan2(r, n)
		};
	}
	function r(e, r, i, a) {
		let o = n(r || e, i || e), s = o.angle + (a ? Math.PI : 0), c = o.length * t;
		return {
			x: e.x + Math.cos(s) * c,
			y: e.y + Math.sin(s) * c
		};
	}
	function i(e, t, n) {
		let i = r(n[t - 1], n[t - 2], e), a = r(e, n[t - 1], n[t + 1], !0);
		return `C ${h(i.x)},${h(i.y)} ${h(a.x)},${h(a.y)} ${h(e.x)},${h(e.y)}`;
	}
	return e.filter((e) => !!e).reduce((e, t, n, r) => n === 0 ? `${h(t.x)},${h(t.y)} ` : `${e} ${i(t, n, r)} `, "");
}
function _e() {
	return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(e) {
		let t = Math.random() * 16 | 0;
		return (e == "x" ? t : t & 3 | 8).toString(16);
	});
}
function R(e, t = !1, n = 16, r = !1) {
	let i = 0, a = t ? n : 0, o = r ? "center" : "middle";
	return e.center.endX > e.cx ? (i = e.center.endX + n + a, o = r ? "left" : "start") : e.center.endX < e.cx ? (i = e.center.endX - n - a, o = r ? "right" : "end") : (i = e.centerX + a, o = r ? "center" : "middle"), {
		x: i,
		anchor: o
	};
}
function z(e, t = 16, n = 16) {
	return e.center.endY > e.cy ? e.center.endY + n : e.center.endY < e.cy ? e.center.endY - t : e.center.endY;
}
function ve({ initX: e, initY: t, offset: n, centerX: r, centerY: i }) {
	let a = Math.atan2(t - i, e - r);
	return {
		x: e + n * Math.cos(a),
		y: t + n * Math.sin(a)
	};
}
function B(e) {
	let t = document.createElementNS("http://www.w3.org/2000/svg", "path");
	t.setAttribute("d", e);
	let n = t.getTotalLength(), r = 0, i = n, a = n / 2, o = .01;
	for (; i - r > o;) {
		let e = (r + i) / 2, n = t.getPointAtLength(e).x;
		if (Math.abs(n - a) < o) {
			a = e;
			break;
		}
		n < a ? r = e : i = e;
	}
	let { x: s, y: c } = t.getPointAtLength(a);
	return {
		x: s,
		y: c
	};
}
function ye(e, t = !1, n = 16, r = 16, i = !1, a = !1, o = 0, s = 12, c = !1) {
	let { x: l } = B(e.path), u = `${R(e).x},${z(e, n, r) - 4} `, d = "", f, p;
	l > e.cx ? (f = R(e).x - s, p = z(e, n, r) - 4, d = `${f},${p}`) : (e.cx, f = R(e).x + s, p = z(e, n, r) - 4, d = `${f},${p}`);
	let m = ` ${e.center.endX},${e.center.endY}`;
	return c ? a ? `M${d} Q${d} ${m}` : `M${u} Q${d} ${m}` : `M${a ? "" : u}${d}${m}`;
}
function be(e) {
	return ![
		null,
		void 0,
		NaN
	].includes(e);
}
function xe(e, t) {
	return [...e].map((e) => e[t]).reduce((e, t) => e + t, 0);
}
function Se(e, t = !0, n = !1) {
	if (!e.length) return "M0,0";
	let r = "";
	return e.forEach((e) => {
		if (!e) return "";
		r += `${e.x},${e.y} `;
	}), n ? r.trim() : `M${r}${t ? "Z" : ""}`;
}
function Ce({ csvContent: e, title: t = "vue-data-ui" }) {
	let n = encodeURI(e), r = document.createElement("a");
	r.setAttribute("href", n), r.setAttribute("download", `${t}.csv`), document.body.appendChild(r), r.click(), r.remove(), window.URL.revokeObjectURL(n);
}
function we(e) {
	return `data:text/csv;charset=utf-8,${e.map((e) => e.join(",")).join("\n")}`;
}
function Te(e, t) {
	if (!/^#([0-9A-F]{3}){1,2}([0-9A-F]{2})?$/i.test(e)) return console.warn("lightenHexColor : Invalid hex color format"), "#000000";
	let n = e.replace("#", "");
	n.length === 3 && (n = n.split("").map((e) => e + e).join(""));
	let r = parseInt(n.substring(0, 2), 16), i = parseInt(n.substring(2, 4), 16), a = parseInt(n.substring(4, 6), 16), o = Math.min(255, r + (255 - r) * t), s = Math.min(255, i + (255 - i) * t), c = Math.min(255, a + (255 - a) * t), l = `#${Math.round(o).toString(16).padStart(2, "0")}${Math.round(s).toString(16).padStart(2, "0")}${Math.round(c).toString(16).padStart(2, "0")}`;
	return n.length === 8 ? l + n.substring(6, 8) : l;
}
function Ee(e, t) {
	if (!/^#([0-9A-F]{3}){1,2}([0-9A-F]{2})?$/i.test(e)) return console.warn("darkenHexColor: Invalid hex color format"), "#000000";
	let n = e.replace("#", "");
	n.length === 3 && (n = n.split("").map((e) => e + e).join(""));
	let r = parseInt(n.substring(0, 2), 16), i = parseInt(n.substring(2, 4), 16), a = parseInt(n.substring(4, 6), 16), o = Math.max(0, r - r * t), s = Math.max(0, i - i * t), c = Math.max(0, a - a * t), l = `#${Math.round(o).toString(16).padStart(2, "0")}${Math.round(s).toString(16).padStart(2, "0")}${Math.round(c).toString(16).padStart(2, "0")}`;
	return n.length === 8 ? l + n.substring(6, 8) : l;
}
function V(e, t) {
	let n = Math.floor(Math.log10(e)), r = e / 10 ** n, i;
	return i = t ? r < 1.5 ? 1 : r < 3 ? 2 : r < 7 ? 5 : 10 : r <= 1 ? 1 : r <= 2 ? 2 : r <= 5 ? 5 : 10, i * 10 ** n;
}
function De(e, t, n, r = !1) {
	let i = r ? t - e : V(t - e, !1), a = r ? i / (n - 1) : V(i / (n - 1), !0), o = Math.floor(e / a) * a, s = Math.ceil(t / a) * a, c = [];
	for (let e = o; e <= s; e += a) c.push(e);
	return {
		min: o,
		max: s,
		tickSize: a,
		ticks: c
	};
}
function Oe(e, t, n, r = !1) {
	let i = r ? t - e : V(t - e, !1), a = r ? i / (n - 1) : V(i / (n - 1), !0), o = Math.floor(e / a) * a, s = Math.ceil(t / a) * a, c = [], l = o;
	for (; l <= s;) l >= e && l <= t && c.push(l), l += a;
	return c[0] !== e && (c[0] = e), c[c.length - 1] !== t && (c[c.length - 1] = t), {
		min: e,
		max: t,
		tickSize: a,
		ticks: c
	};
}
function ke(e) {
	if (typeof e != "string") return console.error("hexColor must be a string"), "#000000ff";
	let t = e.trim();
	if (!t.startsWith("#")) return console.error("hexColor must start with #"), "#000000ff";
	let n = t.slice(1);
	return [
		3,
		4,
		6,
		8
	].includes(n.length) ? /^[0-9a-fA-F]+$/.test(n) ? n.length === 3 || n.length === 4 ? `#${n.split("").map((e) => e + e).join("")}` : t : (console.error("hexColor contains invalid characters"), "#000000ff") : (console.error("hexColor must be #RGB, #RGBA, #RRGGBB, or #RRGGBBAA"), "#000000ff");
}
var H = (e) => {
	let t = ke(e);
	return {
		r: parseInt(t.substring(1, 3), 16),
		g: parseInt(t.substring(3, 5), 16),
		b: parseInt(t.substring(5, 7), 16),
		a: t.length === 9 ? parseInt(t.substring(7, 9), 16) / 255 : 1
	};
}, Ae = ({ r: e, g: t, b: n, a: r = 1 }) => {
	let i = U(r, 0, 1), a = `#${k(e)}${k(t)}${k(n)}`;
	return i < 1 ? `${a}${k(i * 255)}` : a;
};
function U(e, t, n) {
	return Math.min(Math.max(e, t), n);
}
function je(e, t, n, r, i) {
	let a = H(e), o = H(t);
	i = Math.min(Math.max(i, n), r);
	let s = (i - n) / (r - n), c = {
		r: Math.round(a.r + (o.r - a.r) * s),
		g: Math.round(a.g + (o.g - a.g) * s),
		b: Math.round(a.b + (o.b - a.b) * s)
	}, l = a.a + (o.a - a.a) * s;
	return Ae({
		...c,
		a: l
	});
}
function Me({ colors: e, ratio: t }) {
	if (!Array.isArray(e)) throw Error("colors must be an array");
	if (!e.length) throw Error("colors must contain at least 1 hex color");
	let n = (e) => Math.round(U(e, 0, 255)).toString(16).padStart(2, "0"), r = ({ r: e, g: t, b: r, a: i = 1 }) => `#${n(e)}${n(t)}${n(r)}${n(U(i, 0, 1) * 255)}`;
	if (e.length === 1) return r(H(e[0]));
	let i = U(t, 0, 1), a = e.map(H), o = a.length - 1, s = i * o, c = Math.min(Math.floor(s), o - 1), l = s - c, u = a[c], d = a[c + 1];
	return r({
		r: u.r + (d.r - u.r) * l,
		g: u.g + (d.g - u.g) * l,
		b: u.b + (d.b - u.b) * l,
		a: u.a + (d.a - u.a) * l
	});
}
function Ne({ p: e = "", v: t, s: n = "", r = 0, space: i = !1, isAnimating: a = !1, regex: o = /[^%]/g, replacement: s = "-", locale: c = null }) {
	let l = c ? Number(Number(t).toFixed(r)).toLocaleString(c) : Number(Number(t).toFixed(r)).toLocaleString(), u = l === Infinity ? "∞" : l === -Infinity ? "-∞" : l, d = `${e ?? ""}${i ? " " : ""}${[void 0, null].includes(t) ? "-" : u}${i ? " " : ""}${n ?? ""}`;
	return a ? d.replace(o, s) : d;
}
function Pe({ source: e, length: t = 3 }) {
	if (!e && e !== 0) return "";
	e = String(e);
	let n = e.length > 1 ? e.split(" ") : [e];
	if (n.length === 1 && n[0].length === 1) return String(e).toUpperCase();
	if (n.length === 1) return e.slice(0, t).toUpperCase();
	{
		let e = [];
		return n.forEach((n, r) => {
			r < t && e.push(n.slice(0, 1));
		}), e.join().replaceAll(",", "").toUpperCase();
	}
}
function Fe(e) {
	return !!e && typeof e == "function";
}
function Ie(e) {
	return typeof e.apply(null, arguments) == "string";
}
function Le(e) {
	return Array.isArray(e) ? e.length === 0 : !e || Object.keys(e).length === 0;
}
function Re({ componentName: e, type: t, property: n = "", index: i = "", key: a = "", warn: o = !0, debug: s = !0 }) {
	if (!s) return;
	let c = `\n> ${r[t].replace("#COMP#", e).replace("#ATTR#", n).replace("#INDX#", i).replace("#KEY#", a)}\n`;
	if (o) console.warn(c);
	else throw Error(c);
}
function ze({ points: e, a: t, b: n, angleStep: r, startX: i, startY: a }) {
	let o = [];
	for (let s = 0; s < e; s++) {
		let e = r * s, c = t + n * e, l = c * Math.cos(e) + i, u = c * Math.sin(e) + a;
		o.push({
			x: l,
			y: u
		});
	}
	return o;
}
function Be(e) {
	let t = Infinity, n = Infinity, r = -Infinity, i = -Infinity;
	for (let a of e) a.x < t && (t = a.x), a.y < n && (n = a.y), a.x > r && (r = a.x), a.y > i && (i = a.y);
	return {
		minX: t,
		minY: n,
		maxX: r,
		maxY: i,
		width: r - t || 1,
		height: i - n || 1
	};
}
function Ve(e) {
	if (!e.length) return "";
	let t = `M${e[0].x} ${e[0].y}`;
	for (let n = 1; n < e.length - 2; n += 2) {
		let r = e[n - 1], i = e[n], a = e[n + 1], o = e[n + 2], s = (r.x + i.x) / 2, c = (r.y + i.y) / 2, l = (i.x + a.x) / 2, u = (i.y + a.y) / 2, d = (a.x + o.x) / 2, f = (a.y + o.y) / 2;
		t += ` C${s} ${c}, ${l} ${u}, ${d} ${f}`;
	}
	return t;
}
function He({ maxPoints: e, a: t = 6, b: n = 6, angleStep: r = .07, startX: i, startY: a, boxWidth: o, boxHeight: s, padding: c = 12 }) {
	let l = ze({
		points: e,
		a: t,
		b: n,
		angleStep: r,
		startX: 0,
		startY: 0
	}), { minX: u, minY: d, maxX: f, maxY: p, width: m, height: h } = Be(l), g = (u + f) / 2, _ = (d + p) / 2, v = Math.max(1, o - 2 * c), y = Math.max(1, s - 2 * c), b = Math.min(v / m, y / h), x = i - g * b, S = a - _ * b;
	return function(e) {
		let t = Math.max(2, Math.min(Math.round(e), l.length));
		return Ve(l.slice(0, t).map((e) => ({
			x: e.x * b + x,
			y: e.y * b + S
		})));
	};
}
function Ue({ datasetObject: e, requiredAttributes: t }) {
	let n = [];
	return t.forEach((t) => {
		Object.hasOwn(e, t) || n.push(t);
	}), n;
}
var W = {
	ALICEBLUE: "#F0F8FF",
	ANTIQUEWHITE: "#FAEBD7",
	AQUA: "#00FFFF",
	AQUAMARINE: "#7FFFD4",
	AZURE: "#F0FFFF",
	BEIGE: "#F5F5DC",
	BISQUE: "#FFE4C4",
	BLACK: "#000000",
	BLANCHEDALMOND: "#FFEBCD",
	BLUE: "#0000FF",
	BLUEVIOLET: "#8A2BE2",
	BROWN: "#A52A2A",
	BURLYWOOD: "#DEB887",
	CADETBLUE: "#5F9EA0",
	CHARTREUSE: "#7FFF00",
	CHOCOLATE: "#D2691E",
	CORAL: "#FF7F50",
	CORNFLOWERBLUE: "#6495ED",
	CORNSILK: "#FFF8DC",
	CRIMSON: "#DC143C",
	CYAN: "#00FFFF",
	DARKBLUE: "#00008B",
	DARKCYAN: "#008B8B",
	DARKGOLDENROD: "#B8860B",
	DARKGREY: "#A9A9A9",
	DARKGREEN: "#006400",
	DARKKHAKI: "#BDB76B",
	DARKMAGENTA: "#8B008B",
	DARKOLIVEGREEN: "#556B2F",
	DARKORANGE: "#FF8C00",
	DARKORCHID: "#9932CC",
	DARKRED: "#8B0000",
	DARKSALMON: "#E9967A",
	DARKSEAGREEN: "#8FBC8F",
	DARKSLATEBLUE: "#483D8B",
	DARKSLATEGREY: "#2F4F4F",
	DARKTURQUOISE: "#00CED1",
	DARKVIOLET: "#9400D3",
	DEEPPINK: "#FF1493",
	DEEPSKYBLUE: "#00BFFF",
	DIMGRAY: "#696969",
	DODGERBLUE: "#1E90FF",
	FIREBRICK: "#B22222",
	FLORALWHITE: "#FFFAF0",
	FORESTGREEN: "#228B22",
	FUCHSIA: "#FF00FF",
	GAINSBORO: "#DCDCDC",
	GHOSTWHITE: "#F8F8FF",
	GOLD: "#FFD700",
	GOLDENROD: "#DAA520",
	GREY: "#808080",
	GREEN: "#008000",
	GREENYELLOW: "#ADFF2F",
	HONEYDEW: "#F0FFF0",
	HOTPINK: "#FF69B4",
	INDIANRED: "#CD5C5C",
	INDIGO: "#4B0082",
	IVORY: "#FFFFF0",
	KHAKI: "#F0E68C",
	LAVENDER: "#E6E6FA",
	LAVENDERBLUSH: "#FFF0F5",
	LAWNGREEN: "#7CFC00",
	LEMONCHIFFON: "#FFFACD",
	LIGHTBLUE: "#ADD8E6",
	LIGHTCORAL: "#F08080",
	LIGHTCYAN: "#E0FFFF",
	LIGHTGOLDENRODYELLOW: "#FAFAD2",
	LIGHTGREY: "#D3D3D3",
	LIGHTGREEN: "#90EE90",
	LIGHTPINK: "#FFB6C1",
	LIGHTSALMON: "#FFA07A",
	LIGHTSEAGREEN: "#20B2AA",
	LIGHTSKYBLUE: "#87CEFA",
	LIGHTSLATEGREY: "#778899",
	LIGHTSTEELBLUE: "#B0C4DE",
	LIGHTYELLOW: "#FFFFE0",
	LIME: "#00FF00",
	LIMEGREEN: "#32CD32",
	LINEN: "#FAF0E6",
	MAGENTA: "#FF00FF",
	MAROON: "#800000",
	MEDIUMAQUAMARINE: "#66CDAA",
	MEDIUMBLUE: "#0000CD",
	MEDIUMORCHID: "#BA55D3",
	MEDIUMPURPLE: "#9370D8",
	MEDIUMSEAGREEN: "#3CB371",
	MEDIUMSLATEBLUE: "#7B68EE",
	MEDIUMSPRINGGREEN: "#00FA9A",
	MEDIUMTURQUOISE: "#48D1CC",
	MEDIUMVIOLETRED: "#C71585",
	MIDNIGHTBLUE: "#191970",
	MINTCREAM: "#F5FFFA",
	MISTYROSE: "#FFE4E1",
	MOCCASIN: "#FFE4B5",
	NAVAJOWHITE: "#FFDEAD",
	NAVY: "#000080",
	OLDLACE: "#FDF5E6",
	OLIVE: "#808000",
	OLIVEDRAB: "#6B8E23",
	ORANGE: "#FFA500",
	ORANGERED: "#FF4500",
	ORCHID: "#DA70D6",
	PALEGOLDENROD: "#EEE8AA",
	PALEGREEN: "#98FB98",
	PALETURQUOISE: "#AFEEEE",
	PALEVIOLETRED: "#D87093",
	PAPAYAWHIP: "#FFEFD5",
	PEACHPUFF: "#FFDAB9",
	PERU: "#CD853F",
	PINK: "#FFC0CB",
	PLUM: "#DDA0DD",
	POWDERBLUE: "#B0E0E6",
	PURPLE: "#800080",
	RED: "#FF0000",
	ROSYBROWN: "#BC8F8F",
	ROYALBLUE: "#4169E1",
	SADDLEBROWN: "#8B4513",
	SALMON: "#FA8072",
	SANDYBROWN: "#F4A460",
	SEAGREEN: "#2E8B57",
	SEASHELL: "#FFF5EE",
	SIENNA: "#A0522D",
	SILVER: "#C0C0C0",
	SKYBLUE: "#87CEEB",
	SLATEBLUE: "#6A5ACD",
	SLATEGREY: "#708090",
	SNOW: "#FFFAFA",
	SPRINGGREEN: "#00FF7F",
	STEELBLUE: "#4682B4",
	TAN: "#D2B48C",
	TEAL: "#008080",
	THISTLE: "#D8BFD8",
	TOMATO: "#FF6347",
	TURQUOISE: "#40E0D0",
	VIOLET: "#EE82EE",
	WHEAT: "#F5DEB3",
	WHITE: "#FFFFFF",
	WHITESMOKE: "#F5F5F5",
	YELLOW: "#FFFF00",
	YELLOWGREEN: "#9ACD32",
	REBECCAPURPLE: "#663399"
};
function We(t) {
	let r = e?.(t) ? n(t) : t;
	if (typeof r != "string") return r;
	let i = r.trim();
	if (i === "" || i[0] === "#") return i;
	if (i.toLowerCase() === "transparent") return "#FFFFFF00";
	let a = i.toUpperCase(), o = a.replace(/GRAY/g, "GREY");
	return W[a] || W[o] || i;
}
var G = "http://www.w3.org/2000/svg";
function Ge(e) {
	if (e.length < 2) return 0;
	let t = 0, n = 0;
	for (let r = 1; r < e.length; r++) {
		let i = e[r - 1], a = e[r];
		if ([
			null,
			void 0,
			0,
			Infinity,
			-Infinity
		].includes(i)) continue;
		let o = (a - i) / Math.abs(i) * 100;
		t += o, n++;
	}
	return n === 0 ? 0 : t / n;
}
function K({ content: e, fontSize: t, fill: n, x: r, y: i, translateY: a = !1 }) {
	let o = e.split("\n"), s = o.length * t, c = a ? (s - t) / 2 : 0;
	return o.map((e, a) => `<tspan x="${r}" y="${i - c + a * t}" fill="${n}">${e}</tspan>`).join("");
}
function Ke(e) {
	return f(e) ? (K({
		content: e,
		fontSize: 1,
		fill: "",
		x: 0,
		y: 0
	}).match(/<tspan\b/g) || []).length : 1;
}
function qe({ content: e, fontSize: t, fill: n, x: r, autoOffset: i = !1 }) {
	let a = e.split("\n"), o = i ? (a.length - 1) * t / 2 : 0;
	return a.map((e, i) => `<tspan x="${r}" dy="${i === 0 ? -o : t}" fill="${n}">${e}</tspan>`).join("");
}
function Je({ content: e, fontSize: t, fill: n, maxWords: r, x: i, y: a }) {
	function o(e, t) {
		let n = e.split(" "), r = [];
		for (let e = 0; e < n.length; e += t) r.push(n.slice(e, e + t).join(" "));
		return r;
	}
	let s = "";
	return o(e, r).forEach((e, r) => {
		let o = `<tspan x="${i}" y="${a + r * t}" fill="${n}">${e}</tspan>`;
		s += o;
	}), s;
}
function Ye(e) {
	return e.length ? e.map((e) => b(e)) : [];
}
function Xe(e, t = null) {
	let n = e.replace(/[\p{P}\p{S}]+/gu, " ").trim(), r = (/[\p{Script=Han}\p{Script=Hiragana}\p{Script=Katakana}\p{Script=Hangul}\p{Script=Thai}\p{Script=Lao}\p{Script=Khmer}\p{Script=Tibetan}\p{Script=Myanmar}\p{Script=Devanagari}]/u.test(e) ? [...n] : n.split(/\s+/)).filter((e) => e.trim().length > 0).reduce((e, t) => (e[t] ? e[t] += 1 : e[t] = 1, e), {});
	return Object.keys(r).map((e) => {
		let n = e;
		return typeof t == "function" && typeof t(e) == "string" && (n = t(e)), {
			name: n,
			value: r[e]
		};
	});
}
function Ze(e) {
	let t = e.reduce((e, t) => e + (t.stackRatio || 0), 0), n = e.filter((e) => e.stackRatio === void 0).length, r = 1 - t, i = n > 0 ? r / n : 0, a = e.map((e) => ({
		...e,
		stackRatio: e.stackRatio === void 0 ? i : e.stackRatio
	})), o = 0;
	return a = a.map((e, t) => (o += e.stackRatio, {
		...e,
		stackIndex: t,
		cumulatedStackRatio: o
	})), a;
}
function Qe(e) {
	function t(e, t, n, r) {
		let i = n - e, a = r - t;
		return Math.sqrt(i * i + a * a);
	}
	function n(e, n, r, i) {
		let a = 0, o = e.x, s = e.y;
		for (let c = 1; c <= 100; c += 1) {
			let l = c / 100, u = 1 - l, d = u * u, f = l * l, p = d * u * e.x + 3 * d * l * n.x + 3 * u * f * r.x + f * l * i.x, m = d * u * e.y + 3 * d * l * n.y + 3 * u * f * r.y + f * l * i.y;
			a += t(o, s, p, m), o = p, s = m;
		}
		return a;
	}
	let r = e.match(/[a-zA-Z][^a-zA-Z]*/g), i = 0, a = 0, o = 0, s = 0, c = 0;
	return r.forEach((e) => {
		let r = e[0], l = e.slice(1).trim().split(/[\s,]+/).map(Number), u = 0;
		switch (r) {
			case "M":
				for (i = l[u++], a = l[u++], o = i, s = a; u < l.length;) c += t(i, a, l[u], l[u + 1]), i = l[u++], a = l[u++];
				break;
			case "L":
				for (; u < l.length;) c += t(i, a, l[u], l[u + 1]), i = l[u++], a = l[u++];
				break;
			case "H":
				for (; u < l.length;) c += t(i, a, l[u], a), i = l[u++];
				break;
			case "V":
				for (; u < l.length;) c += t(i, a, i, l[u]), a = l[u++];
				break;
			case "C":
				for (; u < l.length;) c += n({
					x: i,
					y: a
				}, {
					x: l[u],
					y: l[u + 1]
				}, {
					x: l[u + 2],
					y: l[u + 3]
				}, {
					x: l[u + 4],
					y: l[u + 5]
				}), i = l[u + 4], a = l[u + 5], u += 6;
				break;
			case "Z": c += t(i, a, o, s), i = o, a = s;
		}
	}), c;
}
function $e({ relator: e, adjuster: t, source: n, threshold: r = 0, fallback: i, max: a = 24 }) {
	let o = e / (t / n);
	return o > a ? a : o < r ? i : o;
}
function et(e) {
	return e.reduce((e, t) => (t.series.forEach((t, n) => {
		![
			void 0,
			null,
			Infinity,
			-Infinity
		].includes(t) && !isNaN(t) && (e[n] = (e[n] || 0) + t);
	}), e), []);
}
function tt(e, { value: t, config: n }) {
	let r = !1, i = t;
	if (typeof e == "function") try {
		i = e({
			value: t,
			config: n
		}), ["number", "string"].includes(typeof i) ? r = !0 : Array.isArray(i) ? (i = i.map(String).join("\n"), r = !0) : i = t;
	} catch (e) {
		console.warn("Formatter could not be applied:", e), r = !1;
	}
	return {
		isValid: r,
		value: i
	};
}
function nt(e, t, n, r) {
	let { isValid: i, value: a } = tt(e, {
		value: t,
		config: r
	});
	return i ? a : n;
}
function rt(e, t) {
	return t.split(".").every((t) => typeof e == "object" && e && t in e ? (e = e[t], !0) : !1);
}
function it(e, t = [], n = !1) {
	function r(e) {
		return n && e === null ? null : typeof e == "string" && isNaN(Number(e)) || typeof e == "number" && isFinite(e) ? e : 0;
	}
	function i(e) {
		if (Array.isArray(e)) return e.map((e) => i(e));
		if (typeof e == "object" && e) {
			let n = { ...e };
			return t.forEach((e) => {
				n.hasOwnProperty(e) && !(/* @__PURE__ */ "NAME.name.TITLE.title.DESCRIPTION.description.LABEL.label.TIME.time.PERIOD.period.MONTH.month.YEAR.year.MONTHS.months.YEARS.years.DAY.day.DAYS.days.HOUR.hour.HOURS.hours".split(".")).includes(e) && Array.isArray(n[e]) && (n[e] = i(n[e]));
			}), Object.fromEntries(Object.entries(n).map(([e, t]) => [e, i(t)]));
		}
		return r(e);
	}
	return i(e);
}
function at(e, t = 100) {
	return e.length === 9 ? e.substring(0, 7) + y[t] : e + y[t];
}
function ot({ series: e, center: t, maxRadius: n, hasGhost: r = !1 }) {
	let i = 360 / (e.length - +!!r);
	return e.map((e, r) => {
		let a = e * n, o = r * i, s = o + i, c = o + i / 2, l = j(o) - j(90), u = j(s) - j(90), d = j(c) - j(90), f = t.x + a * Math.cos(l), p = t.y + a * Math.sin(l), m = t.x + a * Math.cos(u), h = t.y + a * Math.sin(u), g = t.x + a * Math.cos(d), _ = t.y + a * Math.sin(d);
		return {
			path: `
            M ${t.x} ${t.y} 
            L ${f} ${p} 
            A ${a} ${a} 0 0 1 ${m} ${h} 
            Z
        `.trim(),
			middlePoint: {
				x: g,
				y: _
			},
			radius: a
		};
	});
}
function st({ data: e, threshold: t }) {
	if (t >= e.length || t < 3) return e;
	let n = [], r = (e.length - 2) / (t - 2), i = 0;
	n.push(e[i]);
	for (let a = 0; a < t - 2; a += 1) {
		let t = Math.floor((a + 1) * r) + 1, o = Math.min(Math.floor((a + 2) * r) + 1, e.length), s = e.slice(t, o), c = 0, l = 0;
		for (let e of s) c += e.x, l += e.y;
		c /= s.length, l /= s.length;
		let u = -1, d = i;
		for (let n = t; n < o; n += 1) {
			let t = Math.abs((e[i].x - c) * (e[n].y - e[i].y) - (e[i].x - e[n].x) * (l - e[i].y));
			t > u && (u = t, d = n);
		}
		n.push(e[d]), i = d;
	}
	return n.push(e[e.length - 1]), n;
}
function ct({ data: e, threshold: t }) {
	if (t >= e.length || t < 3) return e;
	let n = [], r = (e.length - 2) / (t - 2), i = 0;
	n.push(e[i]);
	for (let a = 0; a < t - 2; a += 1) {
		let t = Math.floor((a + 1) * r) + 1, o = Math.min(Math.floor((a + 2) * r) + 1, e.length), s = e.slice(t, o), c = s.reduce((e, t) => e + t, 0) / s.length, l = -1, u = i;
		for (let n = t; n < o; n += 1) {
			let t = Math.abs((e[i] - c) * (n - i));
			t > l && (l = t, u = n);
		}
		n.push(e[u]), i = u;
	}
	return n.push(e[e.length - 1]), n;
}
function lt({ data: e, threshold: t, key: n = "value" }) {
	if (t >= e.length || t < 3) return e;
	let r = [], i = (e.length - 2) / (t - 2), a = 0;
	r.push(e[a]);
	for (let o = 0; o < t - 2; o += 1) {
		let t = Math.floor((o + 1) * i) + 1, s = Math.min(Math.floor((o + 2) * i) + 1, e.length), c = e.slice(t, s), l = c.reduce((e, t) => e + t[n], 0) / c.length, u = -1, d = a;
		for (let r = t; r < s; r += 1) {
			let t = Math.abs((e[a][n] - l) * (r - a));
			t > u && (u = t, d = r);
		}
		r.push(e[d]), a = d;
	}
	return r.push(e[e.length - 1]), r;
}
function ut({ radius: e, centerX: t, centerY: n, percentage: r }) {
	r = Math.max(0, Math.min(1, r));
	let i = r * Math.PI;
	return `M ${t},${n} L ${t - e},${n} A ${e},${e} 0 0 1 ${t - e * Math.cos(i)},${n - e * Math.sin(i)} Z`.trim();
}
function dt({ svgElement: e, x: t, y: n, offsetY: r = 0, element: i }) {
	if (!e || !i) return {
		top: 0,
		left: 0
	};
	let a = e.createSVGPoint();
	a.x = t, a.y = n;
	let o = a.matrixTransform(e.getScreenCTM()), s = e.getBoundingClientRect(), c = i.getBoundingClientRect(), l = 0, u = 0;
	return l = o.x - c.width / 2 < s.left ? 0 : o.x + c.width > s.right ? -c.width : -c.width / 2, u = o.y - r - c.height < s.top ? r : -c.height - r, {
		top: o.y + u,
		left: o.x + l
	};
}
function ft({ svgElement: e, x: t, y: n, element: r, position: i }) {
	if (!e || !r) return {
		top: 0,
		left: 0
	};
	let a = e.createSVGPoint();
	a.x = t, a.y = n;
	let o = a.matrixTransform(e.getScreenCTM()), { height: s, width: c } = r.getBoundingClientRect(), l = i === "right" ? 0 : -c, u = -(s / 2);
	return {
		top: o.y + u,
		left: o.x + l
	};
}
function q(e) {
	if (typeof e != "object" || !e) return e;
	if (e instanceof Date) return new Date(e.getTime());
	if (e instanceof RegExp) return new RegExp(e.source, e.flags);
	if (e instanceof Map) {
		let t = /* @__PURE__ */ new Map();
		for (let [n, r] of e.entries()) t.set(n, q(r));
		return t;
	}
	if (e instanceof Set) {
		let t = /* @__PURE__ */ new Set();
		for (let n of e.values()) t.add(q(n));
		return t;
	}
	if (Array.isArray(e)) return e.map((e) => q(e));
	let t = {};
	for (let n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = q(e[n]));
	return t;
}
function pt(e) {
	let t = [], n = [];
	for (let r of e) !r || r.value == null || Number.isNaN(r.x) || Number.isNaN(r.y) ? (n.length && t.push(n), n = []) : n.push(r);
	return n.length && t.push(n), t;
}
function mt(e, t) {
	let n = e.filter((e) => !!e);
	if (!n[0]) return [
		-10,
		-10,
		"",
		-10,
		-10
	].toString();
	let r = {
		x: n[0].x,
		y: t
	}, i = {
		x: n.at(-1).x,
		y: t
	}, a = [];
	return n.forEach((e) => {
		a.push(`${e.x},${e.y} `);
	}), [
		r.x,
		r.y,
		...a,
		i.x,
		i.y
	].toString();
}
function ht(e, t) {
	if (!e[0]) return [
		-10,
		-10,
		"",
		-10,
		-10
	].toString();
	let n = pt(e);
	return n.length ? n.map((e) => {
		let n = {
			x: e[0].x,
			y: t
		}, r = {
			x: e.at(-1).x,
			y: t
		}, i = [];
		return e.forEach((e) => {
			i.push(`${e.x},${e.y} `);
		}), [
			n.x,
			n.y,
			...i,
			r.x,
			r.y
		].toString();
	}).join(";") : "";
}
function gt(e) {
	let t = [], n = [];
	for (let r of e) r.value == null || Number.isNaN(r.x) || Number.isNaN(r.y) ? (n.length > 1 && t.push(n), n = []) : n.push(r);
	return n.length > 1 && t.push(n), t;
}
function _t(e) {
	let t = "", n = !1;
	for (let r = 0; r < e.length; r++) {
		let i = e[r];
		if (!X(i)) continue;
		let a = `${h(i.x)},${h(i.y)}`;
		if (!n) t += a, n = !0;
		else {
			let n = e[r - 1], i = X(n) ? "L" : "M";
			t += `${i}${a}`;
		}
		t += " ";
	}
	return t.trim();
}
function J(e) {
	let t = Number(e);
	return Number.isFinite(t) ? Math.trunc(t) : null;
}
function Y(e) {
	return `${h(e.x)},${h(e.y)}`;
}
function vt(e, t = []) {
	let n = Array.isArray(e) ? e : [], r = n.length, i = /* @__PURE__ */ new Set(), a = r - 1;
	for (let e of Array.isArray(t) ? t : []) {
		let t = J(e);
		t !== null && (t > 0 && i.add(t - 1), t < a && i.add(t));
	}
	let o = [], s = [], c = !1, l = !1, u = () => {
		s.length >= 2 && o.push({
			path: s.map((e, t) => t === 0 ? e : `L${e}`).join(" "),
			dashed: c
		}), s = [], l = !1;
	};
	for (let e = 0; e < r; e += 1) {
		let t = n[e];
		if (!X(t)) {
			u();
			continue;
		}
		let r = Y(t);
		if (!l) {
			s = [r], l = !0;
			continue;
		}
		let a = n[e - 1];
		if (!X(a)) {
			u(), s = [r], l = !0;
			continue;
		}
		let o = i.has(e - 1);
		if (s.length === 1) {
			c = o, s.push(r);
			continue;
		}
		if (o !== c) {
			let e = Y(a);
			u(), c = o, s = [e, r], l = !0;
		} else s.push(r);
	}
	return u(), o;
}
function yt(e) {
	let t = gt(e);
	if (!t.length) return "";
	let n = "";
	for (let [e, r] of t.entries()) {
		if (r.length < 2) continue;
		let t = r.length - 1, i = [], a = [], o = [], s = [];
		for (let e = 0; e < t; e += 1) i[e] = r[e + 1].x - r[e].x, a[e] = r[e + 1].y - r[e].y, o[e] = a[e] / i[e];
		s[0] = o[0], s[t] = o[t - 1];
		for (let e = 1; e < t; e += 1) if (o[e - 1] * o[e] <= 0) s[e] = 0;
		else {
			let t = 2 * o[e - 1] * o[e] / (o[e - 1] + o[e]);
			s[e] = t;
		}
		n += `${e === 0 ? "" : "M"}${h(r[0].x)},${h(r[0].y)} `;
		for (let e = 0; e < t; e += 1) {
			let t = r[e].x, i = r[e].y, a = r[e + 1].x, o = r[e + 1].y, c = s[e], l = s[e + 1], u = t + (a - t) / 3, d = i + c * (a - t) / 3, f = a - (a - t) / 3, p = o - l * (a - t) / 3;
			n += `C${h(u)},${h(d)} ${h(f)},${h(p)} ${h(a)},${h(o)} `;
		}
	}
	return n.trim();
}
function X(e) {
	return e != null && e.value != null && Number.isFinite(e.x) && Number.isFinite(e.y);
}
function bt(e, t) {
	let n = e - 1, r = /* @__PURE__ */ new Set(), i = Array.isArray(t) ? t : [];
	for (let e of i) {
		let t = J(e);
		t !== null && (t > 0 && r.add(t - 1), t < n && r.add(t));
	}
	return r;
}
function xt(e) {
	let t = [], n = [];
	for (let r = 0; r < e.length; r += 1) {
		let i = e[r];
		X(i) ? n.push(r) : (n.length > 1 && t.push(n), n = []);
	}
	return n.length > 1 && t.push(n), t;
}
function St(e, t) {
	let n = t.map((t) => e[t]), r = n.length - 1, i = [], a = [], o = [], s = [];
	for (let e = 0; e < r; e += 1) i[e] = n[e + 1].x - n[e].x, a[e] = n[e + 1].y - n[e].y, o[e] = a[e] / i[e];
	s[0] = o[0], s[r] = o[r - 1];
	for (let e = 1; e < r; e += 1) o[e - 1] * o[e] <= 0 ? s[e] = 0 : s[e] = 2 * o[e - 1] * o[e] / (o[e - 1] + o[e]);
	let c = Array(r);
	for (let e = 0; e < r; e += 1) {
		let t = n[e].x, r = n[e].y, i = n[e + 1].x, a = n[e + 1].y, o = s[e], l = s[e + 1], u = t + (i - t) / 3, d = r + o * (i - t) / 3, f = i - (i - t) / 3, p = a - l * (i - t) / 3;
		c[e] = `C${h(u)},${h(d)} ${h(f)},${h(p)} ${h(i)},${h(a)}`;
	}
	return {
		startCoord: Y(n[0]),
		commands: c
	};
}
function Ct(e) {
	let t = [];
	if (!e.length) return t;
	let n = 0, r = e[0];
	for (let i = 1; i < e.length; i += 1) e[i] !== r && (t.push({
		startEdge: n,
		endEdge: i - 1,
		dashed: r
	}), n = i, r = e[i]);
	return t.push({
		startEdge: n,
		endEdge: e.length - 1,
		dashed: r
	}), t;
}
function wt(e, t = []) {
	let n = Array.isArray(e) ? e : [];
	if (n.length < 2) return [];
	let r = bt(n.length, t), i = xt(n);
	if (!i.length) return [];
	let a = [];
	for (let e of i) {
		let { startCoord: t, commands: i } = St(n, e), o = Array(i.length);
		for (let t = 0; t < i.length; t += 1) {
			let n = e[t];
			o[t] = r.has(n);
		}
		let s = Ct(o);
		for (let r of s) {
			let o = r.startEdge, s = `${o === 0 ? t : Y(n[e[o]])} ${i.slice(r.startEdge, r.endEdge + 1).join(" ")}`.trim();
			a.push({
				path: s,
				dashed: r.dashed
			});
		}
	}
	return a;
}
function Tt(e, t, n = !1, r = !0) {
	function i(e) {
		let t = [], n = [];
		for (let r of e) !r || r.value == null || Number.isNaN(r.x) || Number.isNaN(r.y) ? (n.length > 1 && t.push(n), n = []) : n.push(r);
		return n.length > 1 && t.push(n), t;
	}
	return (n ? i(e) : [e]).map((e) => {
		if (e.length < 2) return "";
		let n = e.length - 1, i = [], a = [], o = [], s = [];
		for (let t = 0; t < n; t += 1) i[t] = e[t + 1].x - e[t].x, a[t] = e[t + 1].y - e[t].y, o[t] = a[t] / i[t];
		s[0] = o[0], s[n] = o[n - 1];
		for (let e = 1; e < n; e += 1) if (o[e - 1] * o[e] <= 0) s[e] = 0;
		else {
			let t = 2 * o[e - 1] * o[e] / (o[e - 1] + o[e]);
			s[e] = t;
		}
		let c = `M${e[0].x},${t}`;
		c += ` L${e[0].x},${e[0].y}`;
		for (let t = 0; t < n; t += 1) {
			let n = e[t].x, r = e[t].y, i = e[t + 1].x, a = e[t + 1].y, o = s[t], l = s[t + 1], u = n + (i - n) / 3, d = r + o * (i - n) / 3, f = i - (i - n) / 3, p = a - l * (i - n) / 3;
			c += ` C${u},${d} ${f},${p} ${i},${a}`;
		}
		return c += ` L${e[n].x},${t} ${r ? "Z" : ""}`, c;
	}).filter(Boolean);
}
function Et(e) {
	return e.toString().toLowerCase().replace(/\s+/g, "-").replace(/[^\p{L}\p{N}_-]+/gu, "").replace(/\-\-+/g, "-").replace(/^-+/, "").replace(/-+$/, "");
}
function Dt(e) {
	return e && typeof e == "object" && !Array.isArray(e) && Object.keys(e).length === 0 ? null : e;
}
function Z(e) {
	if (Array.isArray(e)) return e.map(Z);
	if (e && typeof e == "object" && !Array.isArray(e)) {
		let t = {};
		for (let n in e) Object.hasOwn(e, n) && (t[n] = Z(e[n]));
		return Dt(t);
	}
	return e;
}
function Ot(e) {
	return 1 - (1 - e) ** 3;
}
function kt({ values: e, config: t = {} }) {
	let { keepInvalid: n = !0, convertInvalidToZero: r = !1 } = t, i = [], a = 0, o = 0;
	function s(e) {
		return typeof e != "number" || !Number.isFinite(e);
	}
	function c(e) {
		a += e, o += 1, i.push(a / o);
	}
	for (let t of e) s(t) ? r && n ? c(0) : !r && n && i.push(t) : c(t);
	return i;
}
function At({ values: e, config: t = {} }) {
	let { keepInvalid: n = !0, convertInvalidToZero: r = !1 } = t, i = [], a = [];
	function o(e) {
		return typeof e != "number" || !Number.isFinite(e);
	}
	function s(e) {
		a.push(e), a.sort((e, t) => e - t);
		let t = a.length, n = Math.floor(t / 2);
		t % 2 == 1 ? i.push(a[n]) : i.push((a[n - 1] + a[n]) / 2);
	}
	for (let t of e) o(t) ? r && n ? s(0) : !r && n && i.push(t) : s(t);
	return i;
}
function jt({ el: e, bounds: t, currentFontSize: n, minFontSize: r = 6, attempts: i = 200, padding: a = 1 }) {
	if (!e || !n) return 0;
	let o = n;
	e.setAttribute("font-size", o);
	let { x: s, y: c, width: l, height: u } = t, d = s + a, f = c + a, p = s + l - a, m = c + u - a, h = e.getBBox();
	if (h.x >= d + a && h.y >= f + a && h.x + h.width <= p - a && h.y + h.height <= m - a) return o;
	let g = i;
	for (; g-- > 0 && o > r && (o--, e.setAttribute("font-size", o), h = e.getBBox(), !(h.x >= d + a && h.y >= f + a && h.x + h.width <= p - a && h.y + h.height <= m - a)););
	return o < r && (o = 0, e.setAttribute("font-size", o)), o;
}
function Mt({ value: e, maxDecimals: t = 4, fallbackFormatter: n, removeTrailingZero: r = !0 }) {
	if (e === 0) return "0";
	let i = Math.abs(e);
	if (i >= 1 && typeof n == "function") {
		let t = n(e);
		return String(t);
	}
	let a;
	a = i < 1 ? Math.min(Math.max(1 - Math.floor(Math.log10(i)), 1), t) : t;
	let o = e.toFixed(a);
	return r && (o = o.replace(/(\.\d*?[1-9])0+$/, "$1").replace(/\.0+$/, "")), o;
}
function Nt(e) {
	let t = [];
	for (let n = 0; n < e; n += 1) t.push(n === 0 ? 0 : n === 1 ? 1 : t[n - 1] + t[n - 2]);
	return t;
}
function Pt(e, t = 20) {
	e = e.replace(/[\r\n]+/g, " ");
	let n = e.split(" "), r = "", i = "";
	for (let e of n) (r + (r ? " " : "") + e).length <= t ? r += (r ? " " : "") + e : (r && (i += (i ? "\n" : "") + r), r = e);
	return r && (i += (i ? "\n" : "") + r), i;
}
function Ft(e) {
	return e && ![null, void 0].includes(e.value) && Number.isFinite(e.x) && Number.isFinite(e.y);
}
function Q(e, t) {
	if (!Array.isArray(e) || !e.length) return [];
	let n = [], r = [], i = () => {
		r.length && (n.push(r), r = []);
	};
	for (let n of e) Ft(n) ? r.push(n) : t && i();
	if (i(), !t) {
		let e = n.flat();
		n.length = 0, e.length && n.push(e);
	}
	let a = [];
	return n.forEach((e, t) => {
		if (t > 0 && a.push({
			x: null,
			y: null,
			value: null
		}), e.length) {
			a.push({ ...e[0] });
			for (let t = 1; t < e.length; t += 1) {
				let n = e[t - 1], r = e[t], i = r.x - n.x;
				if (!Number.isFinite(i) || i === 0) {
					a.push({ ...r });
					continue;
				}
				let o = Math.sign(i), s = Math.min(Math.abs(i) * 1e-6, 1e-4), c = r.x - o * s;
				(o > 0 ? c > n.x : c < n.x) && a.push({
					...n,
					x: c,
					y: n.y,
					value: n.value
				}), a.push({ ...r });
			}
		}
	}), a;
}
function It(e) {
	let { lineA: t, lineB: n, colorLineA: r, colorLineB: i, smoothA: a = !1, smoothB: o = !1, stepperA: s = !1, stepperB: c = !1, sampleStepPx: l = 2, cutNullValues: u = !0, merge: d = !0 } = e || {};
	if (!Array.isArray(t) || !Array.isArray(n) || !t.length || !n.length) return [];
	let f = s ? Q(t, u) : t, p = c ? Q(n, u) : n, m = (e) => Number.isFinite(e);
	function h(e) {
		if (!u) return [e.filter((e) => e && m(e.x) && m(e.y))];
		let t = [], n = [];
		for (let r of e) r && m(r.x) && m(r.y) && r.value != null ? n.push({
			x: r.x,
			y: r.y
		}) : (n.length > 1 && t.push(n), n = []);
		return n.length > 1 && t.push(n), t;
	}
	function g(e) {
		let t = e.length - 1, n = Array(t), r = Array(t), i = Array(t), a = Array(e.length);
		for (let a = 0; a < t; a += 1) n[a] = e[a + 1].x - e[a].x, r[a] = e[a + 1].y - e[a].y, i[a] = r[a] / n[a];
		a[0] = i[0], a[t] = i[t - 1];
		for (let e = 1; e < t; e += 1) i[e - 1] * i[e] <= 0 ? a[e] = 0 : a[e] = 2 * i[e - 1] * i[e] / (i[e - 1] + i[e]);
		return a;
	}
	function _(e, t, n, r, i) {
		let a = e.x, o = t.x, s = e.y, c = t.y, l = o - a;
		if (l === 0) return s;
		let u = (i - a) / l, d = u * u, f = d * u, p = 2 * f - 3 * d + 1, m = f - 2 * d + u, h = -2 * f + 3 * d, g = f - d;
		return p * s + n * l * m + h * c + r * l * g;
	}
	function v(e, t) {
		let n = h(e);
		if (!n.length) return [];
		let r = Infinity, i = -Infinity;
		for (let e of n) r = Math.min(r, e[0].x), i = Math.max(i, e[e.length - 1].x);
		if (!m(r) || !m(i) || i <= r) return [];
		let a = Math.max(1, l), o = [];
		for (let e = r; e <= i; e += a) o.push(e);
		o[o.length - 1] < i && o.push(i);
		let s = [];
		for (let e of o) {
			let r = null, i = !1;
			for (let a of n) {
				let n = a.length - 1;
				if (!(e < a[0].x - 1e-9 || e > a[n].x + 1e-9)) {
					for (let o = 0; o < n; o += 1) {
						let n = a[o], s = a[o + 1];
						if (!(e + 1e-9 < n.x || e - 1e-9 > s.x)) {
							if (t) {
								let t = a.__tangents ||= g(a);
								r = _(n, s, t[o], t[o + 1], e);
							} else {
								let t = (e - n.x) / (s.x - n.x || 1);
								r = n.y + t * (s.y - n.y);
							}
							i = !0;
							break;
						}
					}
					if (i) break;
				}
			}
			r == null ? s.push({
				x: e,
				y: null,
				hole: !0
			}) : s.push({
				x: e,
				y: r,
				hole: !1
			});
		}
		return s;
	}
	function y(e, t, n) {
		return e + n * (t - e);
	}
	function b(e, t) {
		let n = [], r = [], i = Math.min(e.length, t.length);
		for (let a = 0; a < i - 1; a += 1) {
			let i = e[a], o = e[a + 1], s = t[a], c = t[a + 1];
			if (n.push(i), r.push(s), i.hole || o.hole || s.hole || c.hole || i.y == null || o.y == null || s.y == null || c.y == null) continue;
			let l = i.y - s.y, u = o.y - c.y;
			if (l > 0 && u < 0 || l < 0 && u > 0) {
				let e = l / (l - u), t = y(i.x, o.x, e), a = y(i.y, o.y, e), s = {
					x: t,
					y: a,
					hole: !1
				}, c = {
					x: t,
					y: a,
					hole: !1
				};
				n.push(s), r.push(c);
			}
		}
		return i > 0 && (n.push(e[i - 1]), r.push(t[i - 1])), {
			A: n,
			B: r
		};
	}
	function x(e, t) {
		let n = [], a = Math.min(e.length, t.length);
		for (let o = 0; o < a - 1; o += 1) {
			let a = e[o], s = e[o + 1], c = t[o], l = t[o + 1];
			if (a.hole || s.hole || c.hole || l.hole || a.y == null || s.y == null || c.y == null || l.y == null) continue;
			let u = a.y - c.y, d = s.y - l.y, f = u <= 0 ? a : c, p = d <= 0 ? s : l, m = d <= 0 ? l : s, h = u <= 0 ? c : a, g = u <= 0 ? r : i, _ = [
				`M${f.x},${f.y}`,
				`L${p.x},${p.y}`,
				`L${m.x},${m.y}`,
				`L${h.x},${h.y}`,
				"Z"
			].join(" ");
			n.push({
				d: _,
				color: g
			});
		}
		return n;
	}
	function S(e, t) {
		let n = [], a = Math.min(e.length, t.length);
		if (a < 2) return n;
		let o = 0;
		for (; o < a - 1;) {
			for (; o < a - 1;) {
				let n = e[o], r = t[o], i = e[o + 1], a = t[o + 1];
				if (!n.hole && !r.hole && !i.hole && !a.hole && n.y != null && r.y != null && i.y != null && a.y != null) break;
				o += 1;
			}
			if (o >= a - 1) break;
			let s = o, c = Math.sign(t[o].y - e[o].y || 0) || 1;
			for (o += 1; o < a - 1;) {
				let n = e[o], r = t[o], i = e[o + 1], a = t[o + 1];
				if (n.hole || r.hole || i.hole || a.hole || n.y == null || r.y == null || i.y == null || a.y == null || (Math.sign(r.y - n.y || 0) || 1) !== c) break;
				o += 1;
			}
			let l = o + 0, u = c >= 0 ? e : t, d = c >= 0 ? t : e, f = c >= 0 ? r : i, p = [];
			for (let e = s; e <= l; e += 1) p.push(`${u[e].x},${u[e].y}`);
			let m = [];
			for (let e = l; e >= s; --e) m.push(`${d[e].x},${d[e].y}`);
			let h = `M${p[0]} L${p.slice(1).join(" L")} L${m.join(" L")} Z`;
			n.push({
				d: h,
				color: f
			});
		}
		return n;
	}
	let { A: C, B: w } = b(v(f, !s && a), v(p, !c && o));
	return d ? S(C, w) : x(C, w);
}
function Lt(e, t, n = {}) {
	let r = {
		bubbles: !0,
		cancelable: !0,
		composed: !0,
		...n
	}, i = /* @__PURE__ */ new Set([
		"click",
		"mousedown",
		"mouseup",
		"mousemove",
		"mouseover",
		"mouseout",
		"mouseenter",
		"mouseleave",
		"dblclick",
		"contextmenu"
	]), a = /* @__PURE__ */ new Set([
		"keydown",
		"keyup",
		"keypress"
	]), o;
	if (i.has(t)) o = new MouseEvent(t, r);
	else if (a.has(t)) o = new KeyboardEvent(t, r);
	else if (t === "input") try {
		o = new InputEvent(t, r);
	} catch {
		o = new Event(t, r);
	}
	else o = t.startsWith("custom:") ? new CustomEvent(t, {
		...r,
		detail: r.detail
	}) : new Event(t, r);
	return e.dispatchEvent(o), o;
}
function Rt(e, { delta: t = 1, delay: n = 20, disableTransitions: r = !0 } = {}) {
	if (!e || !(e instanceof HTMLElement)) return;
	let i = e.style, a = {
		width: i.width,
		height: i.height,
		transition: i.transition
	}, o = e.getBoundingClientRect(), s = o.width, c = o.height;
	r && (i.transition = "none");
	let l = (e) => /%|em|rem/.test(e);
	i.width = a.width && l(a.width) ? `calc(${a.width} + ${t}px)` : `${Math.max(0, s + t)}px`, i.height = a.height && l(a.height) ? `calc(${a.height} + ${t}px)` : `${Math.max(0, c + t)}px`, e.offsetWidth, setTimeout(() => {
		i.width = a.width, i.height = a.height, e.offsetWidth, requestAnimationFrame(() => {
			r && (i.transition = a.transition);
		});
	}, n);
}
function zt(e) {
	let t = null, n = null;
	return (...r) => {
		let i = JSON.stringify(r);
		return i === t ? n : (t = i, n = e(...r), n);
	};
}
var $ = zt((e, t, n, r, i, a, o, s) => {
	if (e) return r.length <= 2 ? r.map((e, t) => ({
		text: e,
		absoluteIndex: t
	})) : r.map((e, t) => ({
		text: t === 0 || t === r.length - 1 || o != null && t === o ? e : "",
		absoluteIndex: t
	}));
	if (!t) return r.map((e, t) => ({
		text: e,
		absoluteIndex: t
	}));
	let c = Math.max(1, n || 1);
	if (s <= c) return r.map((e, t) => ({
		text: e,
		absoluteIndex: t
	}));
	let l = [];
	for (let e = 0; e < r.length; e += 1) {
		let t = r[e] ?? "";
		t && t !== (a + e - 1 >= 0 ? i[a + e - 1] ?? "" : null) && l.push(e);
	}
	if (!l.length) return r.map((e, t) => ({
		text: "",
		absoluteIndex: t
	}));
	let u = l.length, d = c, f = Math.max(2, Math.min(d - 3, u)), p = Math.min(u, d + 3), m = Math.min(d, u), h = Infinity;
	for (let e = f; e <= p; e += 1) {
		let t = (u - 1) % (e - 1), n = Math.abs(e - d), r = t * 10 + n;
		r < h && (h = r, m = e);
	}
	let g = /* @__PURE__ */ new Set();
	if (m <= 1) g.add(l[Math.round((u - 1) / 2)]);
	else {
		let e = (u - 1) / (m - 1);
		for (let t = 0; t < m; t += 1) g.add(l[Math.round(t * e)]);
	}
	return r.map((e, t) => ({
		text: g.has(t) ? e : "",
		absoluteIndex: t
	}));
});
function Bt(e) {
	return String(e).replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
}
function Vt(e) {
	return String(e).replaceAll("&", "&amp;").replaceAll("\"", "&quot;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
}
function Ht(e) {
	if (!e || typeof e != "string") return null;
	let t = document.createElementNS(G, "svg");
	t.setAttribute("xmlns", G);
	let n = document.createElementNS(G, "path");
	n.setAttribute("d", e), t.appendChild(n);
	let r = n.getTotalLength(), i = n.getPointAtLength(r / 2);
	return {
		x: i.x,
		y: i.y
	};
}
function Ut(e, t = "(", n = ")") {
	return `${t}${e}${n}`;
}
function Wt(e, t) {
	return t <= 1 ? "0%" : `${e * 100 / (t - 1)}%`;
}
function Gt(e, t, n) {
	if (!n || !Number.isFinite(e) || !Number.isFinite(t)) return null;
	if (n.createSVGPoint && n.getScreenCTM) {
		let r = n.createSVGPoint();
		r.x = e, r.y = t;
		let i = n.getScreenCTM();
		if (i) {
			let e = r.matrixTransform(i);
			return !Number.isFinite(e.x) || !Number.isFinite(e.y) ? null : {
				x: e.x,
				y: e.y
			};
		}
	}
	let r = n.getBoundingClientRect();
	return {
		x: r.left + e,
		y: r.top + t
	};
}
function Kt(e) {
	return e != null && Number.isFinite(Number(e));
}
function qt(e, t = 1) {
	if (!e.length) return "";
	if (e.length === 1) return `${e[0].x} ${e[0].y}`;
	let n = [`${e[0].x} ${e[0].y}`];
	for (let r = 0; r < e.length - 1; r++) {
		let i = e[r - 1] || e[r], a = e[r], o = e[r + 1], s = e[r + 2] || o, c = {
			x: a.x + (o.x - i.x) / 6 * t,
			y: a.y + (o.y - i.y) / 6 * t
		}, l = {
			x: o.x - (s.x - a.x) / 6 * t,
			y: o.y - (s.y - a.y) / 6 * t
		};
		n.push(`C ${c.x} ${c.y}, ${l.x} ${l.y}, ${o.x} ${o.y}`);
	}
	return n.join(" ");
}
function Jt(e, t = 1) {
	return !e || typeof window > "u" ? Promise.resolve(null) : new Promise((n) => {
		let r = new window.Image();
		r.onload = () => {
			let e = r.naturalWidth / t, i = r.naturalHeight / t;
			n({
				width: e,
				height: i,
				aspectRatio: i ? e / i : 0
			});
		}, r.onerror = () => n(null), r.src = e;
	});
}
function Yt(e, t) {
	let n = Math.floor(Number(t));
	if (!Number.isFinite(n) || n <= 0) return [];
	let r = b(e);
	return r ? Array.from({ length: n }, (e, t) => A(r, t / n)) : (console.error(`Vue Data Ui - createColorWheel - Invalid starting color: ${r}`), []);
}
function Xt(e, t) {
	if (!t || !e) return e;
	let n = String(e ?? ""), r = Math.max(0, Number(t) || 0);
	return n.length > r ? `${n.slice(0, r)}...` : n;
}
//#endregion
export { Ce as $, ht as A, Se as At, ge as B, at as Bt, F as C, Kt as Ct, we as D, lt as Dt, Yt as E, ct as Et, L as F, Ut as Ft, K as G, Gt, _t as H, Et as Ht, yt as I, dt as It, Xe as J, l as Jt, qe as K, v as Kt, wt as L, ft as Lt, ie as M, ve as Mt, Tt as N, y as Nt, ut as O, Te as Ot, I as P, g as Pt, Z as Q, He as R, it as Rt, b as S, p as St, qt as T, st as Tt, vt as U, xe as Ut, he as V, A as Vt, Je as W, et as Wt, Ne as X, Rt as Xt, Ee as Y, Lt as Yt, q as Z, Pt as Zt, De as _, se as _t, Xt as a, B as at, h as b, Me as bt, $ as c, Ie as ct, fe as d, Jt as dt, Ot as et, R as f, Ke as ft, Ge as g, Ht as gt, ye as h, Qe as ht, nt as i, Nt as it, ot as j, Le as jt, mt as k, i as kt, It as l, kt as lt, pe as m, _ as mt, Pe as n, Bt as nt, Ze as o, m as ot, z as p, Ue as pt, _e as q, $e as qt, de as r, Vt as rt, jt as s, Mt as st, G as t, Re as tt, me as u, At as ut, Oe as v, rt as vt, Ye as w, f as wt, ce as x, Fe as xt, be as y, je as yt, oe as z, Wt as zt };
