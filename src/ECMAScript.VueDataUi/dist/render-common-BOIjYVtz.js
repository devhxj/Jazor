import { M as e, S as t, nt as n, rt as r, z as i } from "./lib-Bttd6u5E.js";
import { t as a } from "./package-MHNlcjAX.js";
//#region src/svg/utils/index.js
function o(e, t = 0) {
	let n = Number(e);
	return Number.isFinite(n) ? n : t;
}
function s(e) {
	return Array.isArray(e) ? e : [];
}
function c(e) {
	return Number.isFinite(Number(e));
}
function l(e, t = {}, n = "") {
	let i = Object.entries(t).filter(([, e]) => e != null && e !== !1).map(([e, t]) => `${e}="${r(t)}"`).join(" ");
	return `<${e}${i ? ` ${i}` : ""}>${n}</${e}>`;
}
function u(e, t = {}) {
	let n = Object.entries(t).filter(([, e]) => e != null && e !== !1).map(([e, t]) => `${e}="${r(t)}"`).join(" ");
	return `<${e}${n ? ` ${n}` : ""}/>`;
}
function d(e, t = {}) {
	return l("text", t, n(e ?? ""));
}
function f(e) {
	return [
		"triangle",
		"square",
		"diamond",
		"pentagon",
		"hexagon",
		"star"
	].includes(e) ? e : "circle";
}
function p({ dataCy: t, shape: n, plot: r, radius: a, fill: s, stroke: c, strokeWidth: l }) {
	let d = f(n), p = o(r.x, 0), m = o(r.y, 0), h = o(a, 4);
	if (d === "circle") return u("circle", {
		"data-cy": t,
		cx: p,
		cy: m,
		r: h,
		fill: s,
		stroke: c,
		"stroke-width": l
	});
	if (d === "star") return u("polygon", {
		"data-cy": t,
		points: i({
			plot: {
				x: p,
				y: m
			},
			radius: h
		}),
		fill: s,
		stroke: c,
		"stroke-width": l
	});
	let g = {
		triangle: {
			sides: 3,
			rotation: .52
		},
		square: {
			sides: 4,
			rotation: .8
		},
		diamond: {
			sides: 4,
			rotation: 0
		},
		pentagon: {
			sides: 5,
			rotation: .95
		},
		hexagon: {
			sides: 6,
			rotation: 0
		}
	}[d];
	return u("path", {
		"data-cy": t,
		d: e({
			plot: {
				x: p,
				y: m
			},
			radius: h,
			sides: g.sides,
			rotation: g.rotation
		}).path,
		fill: s,
		stroke: c,
		"stroke-width": l
	});
}
function m(e, t, n) {
	let r = e;
	for (let e of t.split(".")) {
		if (!r || typeof r != "object" || !(e in r)) return n;
		r = r[e];
	}
	return r ?? n;
}
function h(e, t) {
	return String(e ?? "").length * t * .58;
}
//#endregion
//#region src/svg/utils/render-common.js
function g(e, t, n) {
	let r = m(e, `style.chart.${t}`, void 0);
	return r === void 0 ? m(e, `chart.${t}`, n) : r;
}
function _(e, t, n) {
	let r = m(e, t, void 0);
	return r === void 0 ? g(e, t, n) : r;
}
function v(e, n = "#000000") {
	return t(e) || e || n;
}
function y(e, t = "#FFFFFF") {
	return v(g(e, "backgroundColor", t), t);
}
function b(e, t = "#2D353C") {
	return v(g(e, "color", t), t);
}
function x(e, t) {
	return u("rect", {
		width: e.width,
		height: e.height,
		fill: t ?? y(e.config)
	});
}
function S({ content: e, x: t, fontSize: r, fill: i, textAnchor: a }) {
	return String(e).split(/\n/g).map((e, o) => l("tspan", {
		x: t,
		dy: o === 0 ? 0 : r * 1.2,
		fill: i,
		"text-anchor": a
	}, n(e))).join("");
}
function C(e, t) {
	return String(e).split("").reduce((e, n) => n === " " ? e + t * .28 : "ilI.,:;|!".includes(n) ? e + t * .24 : "()[]{}".includes(n) ? e + t * .34 : "0123456789%/".includes(n) ? e + t * .5 : n === n.toUpperCase() && /[A-Z]/.test(n) ? e + t * .62 : e + t * .52, 0);
}
function w(e) {
	return e.svgTitle ? l("desc", { "aria-hidden": "true" }, n(e.svgTitle)) : "";
}
function T() {
	return `<desc aria-hidden="true">Composed with Vue Data UI ${a}</desc>`;
}
//#endregion
export { o as _, g as a, w as c, u as d, h as f, s as g, p as h, _ as i, T as l, c as m, C as n, v as o, m as p, b as r, x as s, S as t, l as u, d as v };
