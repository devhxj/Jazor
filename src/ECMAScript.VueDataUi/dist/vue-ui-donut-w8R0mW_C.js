import { Bt as e, Jt as t, K as n, Kt as r, Mt as i, Pt as a, S as o, Vt as s, X as c, b as l, f as u, h as d, i as f, j as p, kt as m, nt as h, p as g, q as _, t as v } from "./lib-Bttd6u5E.js";
import { t as y } from "./useConfig-DlNpz6P8.js";
import { t as b } from "./vue_ui_donut-BDGqG07h.js";
import { _ as x, a as S, c as C, d as w, g as T, i as E, l as D, n as O, o as k, p as A, r as j, s as ee, t as te, u as M, v as N } from "./render-common-BOIjYVtz.js";
//#region src/svg/vue-ui-donut/create.js
var { vue_ui_donut: ne } = y();
function re({ config: e = {} } = {}) {
	let n = e ?? {}, i = t({
		userConfig: n,
		defaultConfig: ne
	}), o = i.theme;
	if (!o) return {
		...i,
		customPalette: i.customPalette?.length ? i.customPalette : a
	};
	let s = t({
		userConfig: b[o] || {},
		defaultConfig: i
	}), c = t({
		userConfig: n,
		defaultConfig: s
	});
	return {
		...c,
		customPalette: c.customPalette?.length ? c.customPalette : r[o] || a
	};
}
function P(e, t = 0) {
	let n = Number(e);
	return Number.isFinite(n) ? n : t;
}
function F(e, t, n) {
	return String(t).split(".").reduce((e, t) => e && e[t] !== void 0 ? e[t] : void 0, e) ?? n;
}
function ie(e) {
	return typeof e != "object" || !e ? 0 : Array.isArray(e.values) ? e.values.reduce((e, t) => e + P(t, 0), 0) : Array.isArray(e.series) ? e.series.reduce((e, t) => e + P(t, 0), 0) : P(e.value ?? e.y ?? 0, 0);
}
function ae(e, t) {
	return String(e?.name ?? e?.label ?? `Serie ${t + 1}`);
}
function oe(e = [], t) {
	let n = Array.isArray(t.customPalette) ? t.customPalette : a;
	return (Array.isArray(e) ? e : []).map((e, t) => ({
		source: e,
		sourceIndex: t,
		value: ie(e)
	})).filter((e) => Number.isFinite(e.value)).sort((e, t) => e.source?.ghost && !t.source?.ghost ? 1 : t.source?.ghost && !e.source?.ghost ? -1 : t.value - e.value).map((e, t) => {
		let r = e.source, i = e.value, a = (r?.color ? o(r.color) : "") || n[t % n.length];
		return {
			...r,
			id: r?.id ?? `donut_serie_${t}`,
			name: ae(r, t),
			value: i,
			absoluteValue: Math.abs(i),
			absoluteValues: Array.isArray(r?.values) ? r.values : [i],
			color: a,
			index: t,
			sourceIndex: e.sourceIndex,
			patternIndex: r?.patternIndex ?? t,
			seriesIndex: r?.seriesIndex ?? t,
			ghost: !!r?.ghost
		};
	}).filter((e) => Number.isFinite(e.absoluteValue));
}
function se(e) {
	return e.reduce((e, t) => e + t.absoluteValue, 0);
}
function ce({ series: e, total: t, startAngle: n = -90 }) {
	let r = n;
	return e.map((e) => {
		let n = t ? e.absoluteValue / t : 0, i = n * 360, a = r, o = r + i;
		return r = o, {
			...e,
			ratio: n,
			percentage: n * 100,
			startAngle: a,
			endAngle: o,
			midAngle: a + i / 2
		};
	});
}
function le({ dataset: e = [], config: t = {}, width: n = 512, height: r = 512, additionalSvgContent: i = "", svgTitle: a = "" } = {}) {
	let o = re({ config: t }), s = oe(e, o), c = se(s), l = {
		top: P(F(o, "chart.padding.top", 24), 24),
		right: P(F(o, "chart.padding.right", 24), 24),
		bottom: P(F(o, "chart.padding.bottom", 24), 24),
		left: P(F(o, "chart.padding.left", 24), 24)
	}, u = {
		top: l.top,
		left: l.left,
		right: n - l.right,
		bottom: r - l.bottom
	};
	u.width = Math.max(1, u.right - u.left), u.height = Math.max(1, u.bottom - u.top);
	let d = {
		x: u.left + u.width / 2,
		y: u.top + u.height / 2
	}, f = Math.max(1, Math.min(u.width, u.height) / 2), p = P(F(o, "chart.donut.radius", f), f), m = P(F(o, "chart.donut.holeRatio", .5), .5);
	return {
		width: n,
		height: r,
		dataset: e,
		config: o,
		drawingArea: u,
		center: d,
		radius: p,
		innerRadius: Math.max(0, Math.min(p - 1, p * m)),
		total: c,
		series: ce({
			series: s,
			total: c,
			startAngle: P(F(o, "chart.donut.startAngle", -90), -90)
		}),
		additionalSvgContent: i,
		svgTitle: a
	};
}
//#endregion
//#region src/svg/vue-ui-donut/render.js
var ue = _();
function I() {
	return ue;
}
function L(e) {
	return k(S(e, "backgroundColor", "#FFFFFF"), "#FFFFFF");
}
function de(e) {
	return x(e?.value ?? e?.absoluteValue ?? 0, 0);
}
function R(e) {
	return T(e.series).filter((e) => !e?.ghost).map((e, t) => ({
		...e,
		value: Math.abs(de(e)),
		color: k(e.color, "#000000"),
		name: String(e.name ?? `Serie ${t + 1}`),
		seriesIndex: e.seriesIndex ?? e.index ?? t,
		patternIndex: e.patternIndex ?? e.index ?? t
	})).filter((e) => Number.isFinite(e.value));
}
function z(e) {
	return e.reduce((e, t) => e + Math.abs(x(t.value, 0)), 0);
}
function fe(e) {
	let t = S(e, "title", {}), n = A(t, "subtitle", {});
	if (!t.text) return 0;
	let r = x(t.fontSize, 20), i = n.show !== !1 && n.text ? x(n.fontSize, 14) : 0;
	return x(t.paddingTop, 12) + r + (i ? i + 12 : 0) + x(t.paddingBottom, 36);
}
function pe({ state: e, series: t }) {
	let { config: n, width: r } = e, i = S(n, "legend", {}), a = x(i.fontSize, 12), o = x(i.markerSize, 10), s = x(i.itemGap, 24), c = x(i.rowGap, 8), l = x(i.padding, 8), u = x(i.labelGap, 8), d = Math.max(1, r - l * 2), f = [], p = [], m = 0;
	return t.forEach((t) => {
		let n = Ve(e, t), r = O(n, a), i = o + u + r, c = m + i + (p.length ? s : 0);
		p.length && c > d && (f.push({
			items: p,
			width: m
		}), p = [], m = 0), p.push({
			serie: t,
			label: n,
			itemWidth: i
		}), m += i + (p.length > 1 ? s : 0);
	}), p.length && f.push({
		items: p,
		width: m
	}), {
		rows: f,
		fontSize: a,
		markerSize: o,
		itemGap: s,
		labelGap: u,
		rowGap: c,
		padding: l,
		height: f.length * Math.max(o, a) + Math.max(0, f.length - 1) * c + l * 2
	};
}
function B(e, t) {
	return !S(e.config, "legend.show", !0) || !t.length ? {
		rows: [],
		height: 0,
		padding: 0,
		fontSize: 12,
		markerSize: 10,
		rowGap: 0
	} : pe({
		state: e,
		series: t
	});
}
function V(e) {
	return !!E(e, "pie", S(e, "pie", !1));
}
function me(e, t) {
	let { config: n, width: r, height: i } = e, a = fe(n), o = B(e, t), s = S(n, "legend.show", !0), c = S(n, "legend.position", "bottom"), l = s && c === "top" ? o.height : 0, u = s && c !== "top" ? o.height : 0, d = x(S(n, "padding.top", 24), 24), f = x(S(n, "padding.right", 24), 24), p = x(S(n, "padding.bottom", 24), 24), m = x(S(n, "padding.left", 24), 24), h = {
		top: d + a + l,
		right: r - f,
		bottom: i - p - u,
		left: m
	};
	h.width = Math.max(1, h.right - h.left), h.height = Math.max(1, h.bottom - h.top);
	let g = a ? x(E(n, "title.chartOffsetY", 24), 24) : 0, _ = Math.max(1, h.height - g), v = {
		x: h.left + h.width / 2,
		y: h.top + g / 2 + _ / 2
	}, y = x(S(n, "layout.donut.radiusRatio", .35), .35), b = Math.max(.1, Math.min(.50001, y)), C = Math.max(12, Math.min(h.width, _) * b), w = S(n, "layout.donut.radius", null), T = w == null ? C : Math.min(C, x(w, C)), D = x(S(n, "layout.donut.strokeWidth", 64), 64) / 300, O = Math.max(Math.min(T * D * 2, T), 12 * (1 + D)), k = S(n, "layout.donut.thickness", S(n, "layout.donut.size", O)), A = Math.max(0, Math.min(T - 1, k == null ? O : x(k, O)));
	return {
		...e,
		drawingArea: h,
		center: v,
		radius: T,
		thickness: A,
		innerRadius: V(n) ? 0 : Math.max(0, T - A)
	};
}
function he(e, t) {
	let n = x(S(e.config, "layout.donut.rotation", 105.25), 105.25);
	return m({ series: t }, e.center.x, e.center.y, e.radius, e.radius, 1.99999, 2, 1, 360, n, V(e.config) ? e.radius : e.thickness).map((e, n) => ({
		...e,
		index: n,
		color: k(e.color, t[n]?.color),
		value: x(e.value, 0),
		name: String(e.name ?? t[n]?.name ?? `Serie ${n + 1}`),
		patternIndex: e.patternIndex ?? t[n]?.patternIndex ?? n,
		seriesIndex: e.seriesIndex ?? t[n]?.seriesIndex ?? n
	}));
}
function ge(e, t) {
	let n = Math.max(1, ...t.map((e) => Math.abs(x(e.value, 0)))), r = t.map((e) => Math.abs(x(e.value, 0)) / n);
	return p({
		series: r,
		center: e.center,
		maxRadius: Math.min(e.drawingArea.width, e.drawingArea.height) / 3,
		hasGhost: !1
	});
}
function H(e) {
	return `gradient_${I()}`;
}
function U(e, t) {
	return `polar_gradient_${t}_${I()}`;
}
function _e(e) {
	return E(e.config, "type", "classic") !== "classic" || !S(e.config, "useGradient", !1) ? "" : w("circle", {
		"data-cy": "donut-gradient-hollow",
		cx: e.center.x,
		cy: e.center.y,
		r: e.radius <= 0 ? 10 : e.radius,
		fill: `url(#${H(e)})`
	});
}
function W(e) {
	return `drop_shadow_${I()}`;
}
function ve(e) {
	let { config: t, width: n } = e, r = S(t, "title", {}), i = A(r, "subtitle", {});
	if (!r.text) return "";
	let a = r.textAlign || "center", o = a === "left" ? x(r.paddingLeft, 12) : a === "right" ? n - x(r.paddingRight, 12) : n / 2, s = a === "left" ? "start" : a === "right" ? "end" : "middle", c = x(r.fontSize, 20), l = x(i.fontSize, 14), u = x(r.paddingTop, 12) + c, d = [N(r.text, {
		"data-cy": "donut-div-title",
		x: o,
		y: u,
		"font-size": c,
		"font-weight": r.bold ? "700" : "400",
		"text-anchor": s,
		fill: k(r.color, j(t))
	})];
	return i.text && d.push(N(i.text, {
		"data-cy": "donut-div-subtitle",
		x: o,
		y: u + l + 4,
		"font-size": l,
		"font-weight": i.bold ? "700" : "400",
		"text-anchor": s,
		fill: k(i.color || r.color, j(t))
	})), M("g", { "data-layer": "title" }, d.join(""));
}
function ye(t, n) {
	let { config: r } = t, i = [], a = !!S(r, "useGradient", !1), o = !!S(r, "layout.donut.useShadow", !1);
	if (a) {
		let a = E(r, "type", "classic"), o = x(S(r, "gradientIntensity", 30), 30);
		if (a === "classic" && Number.isFinite(t.thickness / t.radius)) {
			let n = V(r) ? 1 : t.thickness / t.radius;
			i.push(M("radialGradient", { id: H(t) }, [
				w("stop", {
					offset: "0%",
					"stop-color": e(L(r), 0),
					"stop-opacity": 0
				}),
				w("stop", {
					offset: `${(1 - n) * 100}%`,
					"stop-color": e("#FFFFFF", 0),
					"stop-opacity": 0
				}),
				w("stop", {
					offset: `${(1 - n / 2) * 100}%`,
					"stop-color": e("#FFFFFF", o)
				}),
				w("stop", {
					offset: "100%",
					"stop-color": e(L(r), 0),
					"stop-opacity": 0
				})
			].join("")));
		}
		a === "polar" && n.forEach((e, n) => {
			let r = t.polarAreas?.[n];
			r?.middlePoint && i.push(M("radialGradient", {
				id: U(t, n),
				cx: `${l(r.middlePoint.x / t.width * 100)}%`,
				cy: `${l(r.middlePoint.y / t.height * 100)}%`,
				r: "62%"
			}, [w("stop", {
				offset: "0%",
				"stop-color": s(e.color, .05),
				"stop-opacity": o / 100
			}), w("stop", {
				offset: "100%",
				"stop-color": e.color
			})].join("")));
		});
	}
	if (o) {
		let e = k(S(r, "layout.donut.shadowColor", "#000000"), "#000000"), n = x(S(r, "layout.donut.shadowOpacity", .2), .2), a = x(S(r, "layout.donut.shadowBlur", 6), 6), o = x(S(r, "layout.donut.shadowOffsetY", 3), 3);
		i.push(M("filter", {
			id: W(t),
			x: "-50%",
			y: "-50%",
			width: "200%",
			height: "200%"
		}, [w("feDropShadow", {
			dx: 0,
			dy: o,
			stdDeviation: a,
			"flood-color": e,
			"flood-opacity": n
		})].join("")));
	}
	return i.length ? M("defs", { "data-layer": "definitions" }, i.join("")) : "";
}
function be(e, t) {
	return t.color;
}
function xe(e, t, n) {
	return S(e.config, "useGradient", !1) ? `url(#${U(e, n)})` : t.color;
}
function G(e) {
	let { config: t } = e;
	return S(t, "layout.donut.borderColorAuto", !0) ? L(t) : k(S(t, "layout.donut.borderColor", L(t)), L(t));
}
function K(e) {
	return S(e.config, "layout.donut.useShadow", !1) ? `url(#${W(e)})` : void 0;
}
function Se(e, t) {
	if (!t.length) return "";
	let n = G(e), r = x(S(e.config, "layout.donut.borderWidth", 1), 1), i = [];
	return t.forEach((t, a) => {
		x(t.proportion, 0) <= 0 || (i.push(w("path", {
			"data-cy": `donut-arc-underlay-${a}`,
			d: t.arcSlice,
			fill: "#FFFFFF",
			stroke: L(e.config)
		})), i.push(w("path", {
			class: "vue-ui-donut-arc-path",
			"data-cy": `donut-arc-${a}`,
			d: t.arcSlice,
			fill: be(e, t, a),
			stroke: n,
			"stroke-width": r,
			filter: K(e)
		})));
	}), M("g", { "data-layer": "classic-slices" }, i.join(""));
}
function Ce(e, t, n) {
	if (!t.length) return "";
	let r = G(e), i = x(S(e.config, "layout.donut.borderWidth", 1), 1), a = [];
	return t.forEach((t, o) => {
		let s = n[o];
		s?.path && (a.push(w("path", {
			"data-cy": `polar-arc-underlay-${o}`,
			d: s.path,
			fill: "#FFFFFF",
			stroke: r
		})), a.push(w("path", {
			class: "vue-ui-donut-arc-path",
			"data-cy": `donut-arc-${o}`,
			d: s.path,
			fill: xe(e, t, o),
			stroke: r,
			"stroke-width": i,
			filter: K(e)
		})));
	}), M("g", { "data-layer": "polar-slices" }, a.join(""));
}
function q(e, t, n = "layout.labels.value.rounding") {
	let { config: r } = e, i = S(r, "layout.labels", {}), a = i.dataLabels ?? {}, o = i.value ?? {}, s = c({
		p: a.prefix ?? "",
		v: t.value,
		s: a.suffix ?? "",
		r: x(S(r, n, o.rounding ?? 0), o.rounding ?? 0)
	});
	return f(o.formatter, t.value, s, { datapoint: t });
}
function J(e, t) {
	let { config: n } = e, r = S(n, "layout.labels.percentage", {}), i = x(t.proportion, 0) * 100, a = c({
		v: i,
		s: "%",
		r: x(r.rounding, 0)
	});
	return f(r.formatter, i, a, { datapoint: t });
}
function Y(e, t) {
	let n = S(e.config, "layout.labels", {}), r = !!n.name?.show, i = !!(n.value?.show ?? !0), a = !!(n.percentage?.show ?? !0);
	return {
		name: r ? String(t.name ?? "") : "",
		value: i ? q(e, t) : "",
		percentage: a ? J(e, t) : ""
	};
}
function we(e, t) {
	let { value: n, percentage: r } = Y(e, t);
	return r && n ? `${r} (${n})` : r || n || "";
}
function X(e, t) {
	let n = S(e.config, "layout.labels", {}), r = n.dataLabels ?? {}, i = !!(n.value?.show ?? !0), a = !!(n.percentage?.show ?? !0), o = i ? q(e, t) : "", s = a ? J(e, t) : "";
	return o && s ? r.valuePercentageRatio === "percentageFirst" ? `${s}\n${o}` : `${o}\n${s}` : o || s || "";
}
function Z(e, t) {
	let n = S(e.config, "layout.labels", {}).dataLabels ?? {};
	if (!E(e.config, "dataLabels.show", n.show ?? !0) || !n.show) return !1;
	let r = x(n.hideUnderValue, 0);
	return x(t.proportion, 0) * 100 > r;
}
function Te(e, t) {
	let n = S(e.config, "layout.labels.dataLabels", {}), r = x(n.smallArcClusterThreshold, 0), i = x(n.hideUnderValue, 0), a = x(t.proportion, 0) * 100;
	return a > i && r > 0 && a <= r;
}
function Q(e) {
	return !!S(e.config, "layout.curvedMarkers", S(e.config, "layout.donut.curvedMarkers", !1));
}
function Ee(e, t) {
	let n = S(e.config, "layout.labels.dataLabels", {}), r = {}, i = e.center.x, a = e.center.y, o = x(n.smallArcClusterFontSize ?? n.fontSize, 10), s = o / 3, c = o * 1.5, d = e.drawingArea.top + 16, f = e.drawingArea.bottom - 16, p = i - (e.radius + 6), m = i + (e.radius + 6), h = Q(e);
	function _(e) {
		return {
			x: l(e.center?.endX ?? e.endX),
			y: l(e.center?.endY ?? e.endY)
		};
	}
	function v({ midX: e, midY: t, bandX: n, bandY: r }) {
		if (!h) return `M ${e} ${t} L ${e} ${r} L ${n} ${r}`;
		let o = n < i ? -1 : 1, s = n - e, c = r - t, l = Math.sqrt(s * s + c * c) || 1, u = e - i, d = t - a, f = Math.sqrt(u * u + d * d) || 1, p = u / f, m = d / f, g = f + 9;
		function _({ x: e, y: t }) {
			let n = e - i, r = t - a, o = Math.sqrt(n * n + r * r) || 1;
			if (o >= g) return {
				x: e,
				y: t
			};
			let s = g / o;
			return {
				x: i + n * s,
				y: a + r * s
			};
		}
		if (l < 56) {
			let o = s / l, u = -(c / l), d = o, f = (e + n) * .5, h = (t + r) * .5, g = (f + u - i) ** 2 + (h + d - a) ** 2;
			(f - u - i) ** 2 + (h - d - a) ** 2 > g && (u = -u, d = -d);
			let v = Math.max(0, Math.min(1, (l - 18) / 44)), y = v * v * (3 - 2 * v), b = e + s * .78, x = t + c * .78, S = _({
				x: b + u * (2.5 + y * 4) * .9 + p * (1 + y * 2.5),
				y: x + d * (2.5 + y * 4) * .9 + m * (1 + y * 2.5)
			});
			return `M ${e} ${t} Q ${S.x} ${S.y} ${n} ${r}`;
		}
		let v = l * .34;
		v < 20 && (v = 20), v > 46 && (v = 46);
		let y = l * .46;
		y < 22 && (y = 22), y > 70 && (y = 70);
		let b = _({
			x: e + p * v,
			y: t + m * v
		}), x = i + o * Math.max(Math.abs(n - i), g), S = _({
			x: n - o * Math.min(y, Math.abs(x - n) * .75),
			y: r
		});
		return `M ${e} ${t} C ${b.x} ${b.y} ${S.x} ${S.y} ${n} ${r}`;
	}
	function y(e) {
		let t = String(e ?? "").split(/\n/g);
		return c + Math.max(0, t.length - 1) * o * 1.2;
	}
	function b({ arc: e, index: t }) {
		let n = _(e);
		return {
			arc: e,
			index: t,
			midX: n.x,
			midY: n.y,
			inlineMarkerX: u(e).x,
			inlineMarkerY: g(e) - 3.5,
			labelHeight: y(e.name)
		};
	}
	function C(e) {
		let t = e.inlineMarkerY < a, n = e.inlineMarkerX < i;
		return t && n ? "TL" : t && !n ? "TR" : !t && n ? "BL" : "BR";
	}
	function w(e, t) {
		let n = t.startsWith("T");
		e.sort((e, t) => n ? e.inlineMarkerY - t.inlineMarkerY || e.index - t.index : t.inlineMarkerY - e.inlineMarkerY || e.index - t.index);
	}
	function T({ side: e, markerX: t, markerY: n, labelY: r, connectorPath: i }) {
		return {
			side: e,
			labelX: e === "left" ? t - 8 : t + 8,
			labelY: r + s,
			textAnchor: e === "left" ? "end" : "start",
			markerX: t,
			markerY: n,
			connectorPath: i
		};
	}
	function E({ candidateList: e, side: t, bandMarkerX: n, startY: i, direction: a }) {
		let o = i;
		e.forEach((e) => {
			let { index: i, midX: s, midY: c, labelHeight: l } = e, u;
			a === "down" ? (u = o, o += l) : (o -= l, u = o);
			let d = u, f = v({
				midX: s,
				midY: c,
				bandX: n,
				bandY: d
			});
			r[i] = T({
				side: t,
				markerX: n,
				markerY: d,
				labelY: u,
				connectorPath: f
			});
		});
	}
	let D = {
		TL: [],
		TR: [],
		BL: [],
		BR: []
	};
	return t.map((e, t) => b({
		arc: e,
		index: t
	})).filter(({ arc: t }) => Z(e, t) && Te(e, t)).forEach((e) => {
		D[C(e)].push(e);
	}), Object.keys(D).forEach((e) => {
		w(D[e], e);
	}), E({
		candidateList: D.TL,
		side: "left",
		bandMarkerX: p,
		startY: d,
		direction: "down"
	}), E({
		candidateList: D.TR,
		side: "right",
		bandMarkerX: m,
		startY: d,
		direction: "down"
	}), D.BL.length > 1 && E({
		candidateList: D.BL,
		side: "left",
		bandMarkerX: p,
		startY: f,
		direction: "up"
	}), D.BR.length > 1 && E({
		candidateList: D.BR,
		side: "right",
		bandMarkerX: m,
		startY: f,
		direction: "up"
	}), r;
}
function De({ state: e, arc: t, arcIndex: n, smallArcLayout: r, fontSize: i }) {
	let a = S(e.config, "layout.labels", {}), o = a.percentage ?? {}, s = a.name ?? {}, c = r?.labelX ?? u(t, !0, 12).x, l = r?.labelY ?? g(t), d = r?.textAnchor ?? u(t, !0, 12).anchor, { name: f } = Y(e, t), p = we(e, t), m = String(f).split(/\n/g), _ = f ? m.map((t, n) => M("tspan", {
		class: "vue-data-ui-datalabel-name",
		x: n === 0 ? void 0 : c,
		dy: n === 0 ? void 0 : i * 1.2,
		fill: k(s.color, j(e.config)),
		"font-size": i,
		"font-weight": s.bold ? "700" : "400"
	}, h(t + (n === m.length - 1 ? " " : "")))).join("") : "", v = p ? M("tspan", {
		class: "vue-data-ui-datalabel-value",
		fill: k(o.color, j(e.config)),
		"font-size": i,
		"font-weight": o.bold ? "700" : "400"
	}, h(p)) : "";
	return M("text", {
		"data-cy": `donut-label-inline-${n}`,
		class: "vue-data-ui-datalabel-inline",
		x: c,
		y: l,
		"text-anchor": d
	}, `${_}${v}`);
}
function Oe(e, t) {
	let n = S(e.config, "layout.labels", {}), r = n.dataLabels ?? {};
	if (!E(e.config, "dataLabels.show", r.show ?? !0) || !r.show) return "";
	let i = x(r.fontSize ?? n.value?.fontSize ?? 12, 12);
	k(r.color ?? n.value?.color ?? j(e.config), j(e.config)), r.bold ?? n.value?.bold;
	let a = x(r.offset, 16), o = Ee(e, t), s = [];
	return t.forEach((t, n) => {
		if (!Z(e, t)) return;
		let c = o[n];
		if (X(e, t), c) {
			s.push(De({
				state: e,
				arc: t,
				arcIndex: n,
				smallArcLayout: c,
				fontSize: x(r.smallArcClusterFontSize, i)
			}));
			return;
		}
		s.push(De({
			state: e,
			arc: t,
			arcIndex: n,
			smallArcLayout: {
				labelX: u(t, !1, a + i / 2).x,
				labelY: g(t, a, a),
				textAnchor: u(t).anchor
			},
			fontSize: i
		}));
	}), s.length ? M("g", { "data-layer": "data-labels" }, s.join("")) : "";
}
function ke(e, t, n = 42) {
	return i({
		initX: t.middlePoint.x,
		initY: t.middlePoint.y,
		offset: n,
		centerX: e.center.x,
		centerY: e.center.y
	});
}
function Ae(e, t) {
	return t.x < e.center.x ? "end" : t.x > e.center.x ? "start" : "middle";
}
function je(e, t, n, r) {
	let i = S(e.config, "layout.labels", {}), a = i.percentage ?? {}, o = i.name ?? {}, s = x(a.fontSize, i.dataLabels?.fontSize ?? 12), c = x(o.fontSize, i.dataLabels?.fontSize ?? 12), l = M("tspan", {
		class: "vue-data-ui-datalabel-value",
		fill: k(a.color, j(e.config)),
		"font-size": s,
		"font-weight": a.bold ? "700" : "400"
	}, h(String(X(e, t).replace(/\n/g, " / ")))), u = o.show ? String(t.name ?? "").split(/\n/g).map((t, r) => M("tspan", {
		class: "vue-data-ui-datalabel-name",
		x: r === 0 ? void 0 : n,
		dy: r === 0 ? void 0 : c * 1.2,
		fill: k(o.color, j(e.config)),
		"font-size": c,
		"font-weight": o.bold ? "700" : "400"
	}, h(t))).join("") : "";
	return r === "end" ? `${u}${l}` : `${l}${u}`;
}
function Me(e, t, r) {
	let i = S(e.config, "layout.labels", {}), a = i.dataLabels ?? {}, o = i.value ?? {}, s = i.percentage ?? {}, c = i.name ?? {};
	if (!E(e.config, "dataLabels.show", a.show ?? !0) || !a.show) return "";
	let l = !!a.oneLine, u = x(o.fontSize, a.fontSize ?? 12), d = x(c.fontSize, a.fontSize ?? 12), f = [];
	return t.forEach((t, i) => {
		let a = r[i];
		if (!a || !Z(e, t)) return;
		let o = ke(e, a, 42), p = Ae(e, o);
		if (l) {
			f.push(M("text", {
				"data-cy": "polar-label-inline",
				class: "vue-data-ui-datalabel-inline",
				x: o.x,
				y: o.y,
				"text-anchor": p
			}, je(e, t, o.x, p)));
			return;
		}
		f.push(M("text", {
			"data-cy": "polar-label-value",
			class: "vue-data-ui-datalabel-value",
			x: o.x,
			y: o.y,
			"font-size": u,
			"font-weight": s.bold ? "700" : "400",
			"text-anchor": p,
			fill: valueFill
		}, te({
			content: X(e, t),
			x: o.x,
			fontSize: u,
			fill: valueFill,
			textAnchor: p
		}))), c.show && f.push(M("text", {
			"data-cy": "polar-label-name",
			class: "vue-data-ui-datalabel-name",
			x: o.x,
			y: o.y + d * 1.2,
			"font-size": d,
			"font-weight": c.bold ? "700" : "400",
			"text-anchor": p,
			fill: k(c.color, j(e.config))
		}, n({
			content: h(String(t.name ?? "")),
			fontSize: d,
			fill: k(c.color, j(e.config)),
			x: o.x
		})));
	}), f.length ? M("g", { "data-layer": "polar-data-labels" }, f.join("")) : "";
}
function Ne(e, t, n = null) {
	if (n?.connectorPath) return n.connectorPath;
	let r = S(e.config, "layout.labels.dataLabels", {}), i = x(r.offset, 16), a = x(r.markerFlatLength, 12), o = Q(e);
	return d(t, !1, i, i, !1, !1, 0, a, o);
}
function Pe(e, t) {
	let n = S(e.config, "layout.labels.dataLabels", {});
	if (!E(e.config, "dataLabels.show", n.show ?? !0) || !n.show) return "";
	let r = Ee(e, t), i = [];
	return t.forEach((t, a) => {
		if (!Z(e, t)) return;
		let o = r[a], s = Ne(e, t, o);
		s && (i.push(w("path", {
			"data-cy": `donut-marker-${a}`,
			d: s,
			stroke: t.color,
			"stroke-width": x(n.markerStrokeWidth, 1),
			"stroke-linecap": "round",
			"stroke-linejoin": "round",
			fill: "none"
		})), i.push(w("circle", {
			"data-cy": `donut-label-marker-${a}`,
			cx: o?.markerX ?? u(t).x,
			cy: o?.markerY ?? g(t) - 3.5,
			r: x(n.markerRadius, 3),
			fill: t.color,
			stroke: L(e.config),
			"stroke-width": 1
		})));
	}), i.length ? M("g", { "data-layer": "label-markers" }, i.join("")) : "";
}
function Fe(e, t, n) {
	let r = S(e.config, "layout.labels.dataLabels", {});
	if (!E(e.config, "dataLabels.show", r.show ?? !0) || !r.show) return "";
	let a = [];
	return t.forEach((t, o) => {
		let s = n[o];
		if (!s?.middlePoint || !Z(e, t)) return;
		let c = i({
			initX: s.middlePoint.x,
			initY: s.middlePoint.y,
			offset: 24,
			centerX: e.center.x,
			centerY: e.center.y
		});
		a.push(w("path", {
			"data-cy": `polar-marker-${o}`,
			d: `M ${c.x},${c.y} ${s.middlePoint.x},${s.middlePoint.y}`,
			stroke: t.color,
			"stroke-width": x(r.markerStrokeWidth, 1),
			"stroke-linecap": "round",
			"stroke-linejoin": "round",
			fill: "none"
		})), a.push(w("circle", {
			"data-cy": `polar-label-marker-${o}`,
			cx: c.x,
			cy: c.y,
			r: x(r.markerRadius, 3),
			fill: t.color,
			stroke: L(e.config),
			"stroke-width": 1
		}));
	}), a.length ? M("g", { "data-layer": "polar-label-markers" }, a.join("")) : "";
}
function $(e, t) {
	return e == null || e === "" ? "" : N(e, {
		"text-anchor": "middle",
		...t
	});
}
function Ie(e, t, n) {
	if (E(e.config, "type", "classic") !== "classic" || V(e.config) || !t) return "";
	let r = S(e.config, "layout.labels.hollow", {}), i = r.total ?? {}, a = r.average ?? {}, o = [];
	if (i.show) {
		let n = x(i.fontSize, 14), r = !!a.show, s = e.center.y - (r ? n : 0) + x(i.offsetY, 0), l = i.value ?? {}, u = x(l.fontSize, n), d = f(l.formatter, t, c({
			p: l.prefix ?? "",
			v: t,
			s: l.suffix ?? "",
			r: x(l.rounding, 0)
		}), { total: t });
		o.push($(i.text ?? "", {
			"data-cy": "hollow-total-name",
			x: e.center.x,
			y: s,
			fill: k(i.color, j(e.config)),
			"font-size": n,
			"font-weight": i.bold ? "700" : "400"
		})), o.push($(d, {
			"data-cy": "hollow-total-value",
			x: e.center.x,
			y: e.center.y + n - (r ? n : 0) + x(l.offsetY, 0),
			fill: k(l.color, k(i.color, j(e.config))),
			"font-size": u,
			"font-weight": l.bold ? "700" : "400"
		}));
	}
	if (a.show) {
		let r = x(a.fontSize, 14), s = !!i.show, l = t / Math.max(1, n.length), u = a.value ?? {}, d = x(u.fontSize, r), p = f(u.formatter, l, c({
			p: u.prefix ?? "",
			v: l,
			s: u.suffix ?? "",
			r: x(u.rounding, 0)
		}), { average: l });
		o.push($(a.text ?? "", {
			"data-cy": "hollow-average-name",
			x: e.center.x,
			y: e.center.y + (s ? r : 0) + x(a.offsetY, 0),
			fill: k(a.color, j(e.config)),
			"font-size": r,
			"font-weight": a.bold ? "700" : "400"
		})), o.push($(p, {
			"data-cy": "hollow-average-value",
			x: e.center.x,
			y: e.center.y + (s ? r : 0) + r + x(u.offsetY, 0),
			fill: k(u.color, k(a.color, j(e.config))),
			"font-size": d,
			"font-weight": u.bold ? "700" : "400"
		}));
	}
	return o.filter(Boolean).length ? M("g", {
		class: "vue-data-ui-donut-hollow-labels",
		"data-layer": "hollow-labels"
	}, o.join("")) : "";
}
function Le(e, t) {
	let { config: n, center: r } = e, i = S(n, "layout.donut.labels.total", S(n, "layout.labels.total", {}));
	if (!i.show || !Number.isFinite(t) || t <= 0) return "";
	let a = S(n, "layout.labels.dataLabels", {}), o = x(i.fontSize, 20), s = k(i.color, j(n)), l = f(i.formatter, t, c({
		p: a.prefix ?? "",
		v: t,
		s: a.suffix ?? "",
		r: x(i.rounding, 0)
	}), { total: t });
	return N(l, {
		"data-cy": "donut-total",
		x: r.x,
		y: r.y + o / 3,
		"font-size": o,
		"font-weight": i.bold ? "700" : "400",
		"text-anchor": "middle",
		fill: s
	});
}
function Re(e, t) {
	let n = z(R(e));
	return n ? x(t.value, 0) / n * 100 : 0;
}
function ze(e, t) {
	let n = S(e.config, "legend", {}), r = S(e.config, "layout.labels", {}), i = r.dataLabels ?? {}, a = r.value ?? {}, o = c({
		p: i.prefix ?? "",
		v: t.value,
		s: i.suffix ?? "",
		r: x(n.roundingValue, a.rounding ?? 0)
	});
	return f(a.formatter, t.value, o, {
		datapoint: t,
		serie: t
	});
}
function Be(e, t) {
	let n = S(e.config, "legend", {}), r = S(e.config, "layout.labels", {}).percentage ?? {}, i = Re(e, t), a = c({
		v: i,
		s: "%",
		r: x(n.roundingPercentage, r.rounding ?? 0)
	});
	return f(r.formatter, i, a, {
		datapoint: t,
		serie: t
	});
}
function Ve(e, t) {
	let n = S(e.config, "legend", {}), r = [], i = n.showPercentage ? Be(e, t) : "", a = n.showValue ? ze(e, t) : "", o = i && n.usePercentageParens ? `(${i})` : i, s = a && n.useValueParens ? `(${a})` : a;
	return n.showValueFirst ? (s && r.push(s), o && r.push(o)) : (o && r.push(o), s && r.push(s)), `${t.name}${r.length ? `: ${r.join(" ")}` : ""}`;
}
function He(e, t) {
	let n = S(e.config, "legend", {});
	if (!n.show || !t.length) return "";
	let r = B(e, t), i = k(n.color, j(e.config)), a = n.position || "bottom", o = Math.max(r.markerSize, r.fontSize), s = a === "top" ? r.padding + o : e.height - r.height + r.padding + o, c = [];
	return r.rows.forEach((t, a) => {
		let l = r.padding + (e.width - r.padding * 2) / 2 - t.width / 2, u = s + a * (o + r.rowGap);
		t.items.forEach(({ serie: e, label: t, itemWidth: a }) => {
			c.push(w("circle", {
				cx: l + r.markerSize / 2,
				cy: u,
				r: r.markerSize / 2,
				fill: e.color,
				opacity: e.opacity ?? 1
			})), c.push(N(t, {
				x: l + r.markerSize + r.labelGap,
				y: u + 2,
				"font-size": r.fontSize,
				"font-weight": n.bold ? "700" : "400",
				"dominant-baseline": "middle",
				fill: i,
				opacity: e.opacity ?? 1
			})), l += a + r.itemGap;
		});
	}), M("g", {
		"data-layer": "legend",
		"data-cy": "donut-div-legend"
	}, c.join(""));
}
function Ue(e) {
	return M("g", { "data-layer": "no-data" }, [w("circle", {
		cx: e.center.x,
		cy: e.center.y,
		r: Math.max(1, e.radius),
		fill: k(S(e.config, "layout.donut.emptyFill", "#E1E5E8"), "#E1E5E8"),
		opacity: .4
	})].join(""));
}
function We(e, t) {
	let n = e.additionalSvgContent;
	return typeof n == "function" ? n({
		width: e.width,
		height: e.height,
		drawingArea: e.drawingArea,
		center: e.center,
		radius: e.radius,
		thickness: e.thickness,
		innerRadius: e.innerRadius,
		config: e.config,
		series: t.map((e) => ({ ...e }))
	}) ?? "" : n ?? "";
}
function Ge(e, t, n) {
	let r = `${E(e.config, "title.text", "Donut chart") || e.svgTitle || "Donut chart"}. ${t.length} series. Total ${l(n)}.`;
	return M("title", {}, h(r));
}
async function Ke(e) {
	let t = e.config, n = x(e.width, S(t, "width", 512)), r = x(e.height, S(t, "height", 512)), i = R(e), a = z(i), o = me({
		...e,
		width: n,
		height: r
	}, i), s = he(o, i), c = ge(o, i), l = E(t, "type", "classic"), u = a > 0 && s.some((e) => x(e.proportion, 0) > 0);
	return `
<svg
    xmlns="${v}"
    width="100%"
    viewBox="0 0 ${n} ${r}"
    role="img"
>
    ${D()}
    ${Ge(o, i, a)}
    ${C(o)}
    ${ye({
		...o,
		polarAreas: c
	}, s)}
    ${ee(o)}
    ${ve(o)}
    ${u ? l === "polar" ? Ce(o, s, c) : Se(o, s) : Ue(o)}
    ${u && l === "classic" ? _e(o) : ""}
    ${u ? l === "polar" ? Fe(o, s, c) : Pe(o, s) : ""}
    ${u ? Ie(o, a, i) : ""}
    ${u ? l === "polar" ? Me(o, s, c) : Oe(o, s) : ""}
    ${u ? Le(o, a) : ""}
    ${He(o, i)}
    ${We(o, s)}
</svg>`.trim();
}
//#endregion
//#region src/svg/vue-ui-donut/index.js
async function qe(e = {}) {
	return await Ke(le(e));
}
//#endregion
export { qe as t };
