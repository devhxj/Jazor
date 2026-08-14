import { A as e, B as t, Bt as n, G as r, H as i, I as a, Jt as o, Kt as s, L as c, N as l, P as u, Pt as d, S as f, U as p, V as m, Vt as h, X as g, _, c as v, i as y, k as b, l as x, nt as S, o as ee, q as C, t as w, v as te, vt as T, zt as E } from "./lib-Bttd6u5E.js";
import { t as D } from "./useTimeLabels-d2f-W1L4.js";
import { t as O } from "./useConfig-DlNpz6P8.js";
import { t as ne } from "./vue_ui_xy-BA3-_LCx.js";
import { _ as k, d as A, f as j, g as M, h as N, l as P, m as F, p as I, u as L, v as R } from "./render-common-BOIjYVtz.js";
//#region src/svg/vue-ui-xy/create.js
var { vue_ui_xy: z } = O();
function re({ config: e = {}, dataset: t = [] } = {}) {
	let n = e ?? {}, r = o({
		userConfig: n,
		defaultConfig: z
	});
	T(n, "chart.highlightArea") && (Array.isArray(n.chart.highlightArea) ? r.chart.highlightArea = n.chart.highlightArea.map((e) => o({
		defaultConfig: z.chart.highlightArea,
		userConfig: e
	})) : r.chart.highlightArea = [o({
		defaultConfig: z.chart.highlightArea,
		userConfig: n.chart.highlightArea
	})]), T(n, "chart.annotations") && Array.isArray(n.chart.annotations) && n.chart.annotations.length ? r.chart.annotations = n.chart.annotations.map((e) => o({
		defaultConfig: z.chart.annotations[0],
		userConfig: e
	})) : r.chart.annotations = [], T(n, "chart.grid.position") && n.chart.grid.position === "start" && t.some((e) => e.type === "bar") && (r.chart.grid.position = "middle");
	let i = r.theme;
	if (!i) return {
		...r,
		customPalette: r.customPalette?.length ? r.customPalette : d
	};
	let a = o({
		userConfig: ne[i] || {},
		defaultConfig: r
	}), c = o({
		userConfig: n,
		defaultConfig: a
	});
	return {
		...c,
		customPalette: c.customPalette?.length ? c.customPalette : s[i] || d
	};
}
function B(e) {
	return e && typeof e == "object" && Number.isFinite(Number(e.x)) && Number.isFinite(Number(e.y));
}
function ie(e) {
	return e != null && Number.isFinite(Number(e));
}
function ae(e) {
	return e.flatMap((e) => Array.isArray(e.series) ? e.series : []).filter(B).map((e) => Number(e.x));
}
function oe({ dataset: e, config: t }) {
	let n = ae(e);
	if (!n.length) return {
		min: 0,
		max: 1,
		ticks: [0, 1]
	};
	let r = t?.chart?.grid?.labels?.xAxis?.scaleMin, i = t?.chart?.grid?.labels?.xAxis?.scaleMax, a = ie(r) ? Number(r) : Math.min(...n), o = ie(i) ? Number(i) : Math.max(...n), s = a === o ? a - 1 : a, c = a === o ? o + 1 : o, l = Number.isFinite(Number(t?.chart?.grid?.labels?.xAxis?.scaleSteps)) ? Number(t.chart.grid.labels.xAxis.scaleSteps) : 6;
	return _(s, c, l);
}
function se({ xValue: e, xScale: t, drawingArea: n, config: r }) {
	let i = (Number(e) - t.min) / (t.max - t.min || 1), a = r?.chart?.grid?.labels?.xAxis?.reverse ? 1 - i : i;
	return n.left + n.width * a;
}
function ce({ dataset: e = [], config: t = {}, width: n = 1e3, height: r = 600, selectedXIndex: i, additionalSvgContent: a = "", svgTitle: o = "" } = {}) {
	let s = re({
		config: t,
		dataset: e
	}), c = {
		top: s.chart.padding.top,
		left: s.chart.padding.left,
		right: n - s.chart.padding.right,
		bottom: r - s.chart.padding.bottom
	};
	c.width = c.right - c.left, c.height = c.bottom - c.top;
	let l = e.flatMap((e) => Array.isArray(e.series) ? e.series.map((e) => Number(typeof e == "object" && e ? e.y : e)) : []).filter(Number.isFinite), u = Math.min(0, ...l), p = Math.max(1, ...l), m = _(u, p, 6), h = e.some((e) => Array.isArray(e.series) ? e.series.some(B) : !1), g = h ? oe({
		dataset: e,
		config: s
	}) : null, v = Math.max(0, ...e.map((e) => e.series?.length || 0)), y = (e) => v <= 1 ? c.left + c.width / 2 : c.left + e / (v - 1) * c.width, b = (e, t) => h ? B(e) ? se({
		xValue: e.x,
		xScale: g,
		drawingArea: c,
		config: s
	}) : null : y(t), x = (e) => {
		let t = (Number(e) - m.min) / (m.max - m.min || 1);
		return c.bottom - t * c.height;
	}, S = Array.isArray(s.customPalette) ? s.customPalette : [], ee = S.length ? S : d;
	return {
		width: n,
		height: r,
		selectedXIndex: i,
		dataset: e,
		config: s,
		drawingArea: c,
		scale: m,
		xScale: g,
		isContinuous: h,
		series: e.map((e, t) => {
			let n = (e.color ? f(e.color) : "") || ee[t % ee.length], r = (e.series || []).map((e, t) => {
				let n = e == null ? null : typeof e == "object" ? e.y ?? null : Number(e);
				return {
					x: b(e, t),
					y: n === null || !Number.isFinite(Number(n)) ? null : x(n),
					value: n,
					index: t,
					rawX: B(e) ? Number(e.x) : null,
					label: e?.name ?? e?.x ?? String(t)
				};
			});
			return {
				...e,
				color: n,
				type: e.type || "line",
				plots: r
			};
		}),
		additionalSvgContent: a,
		svgTitle: o
	};
}
//#endregion
//#region src/svg/vue-ui-xy/render.js
function V(e) {
	return !!I(e, "chart.grid.labels.yAxis.useIndividualScale", !1);
}
function le(e) {
	return String(e.scaleLabel ?? e.id ?? e.name ?? "");
}
function ue(e) {
	return k(I(e, "chart.grid.labels.yAxis.labelWidth", 40), 40) + 36;
}
function de(e) {
	return M(e).flatMap((e) => M(e.plots)).map((e) => e?.value === null || e?.value === void 0 ? null : Number(e.value)).filter(Number.isFinite);
}
function fe(e, t, n) {
	let r = de(n), i = I(e, "chart.grid.labels.yAxis.scaleMin", null), a = I(e, "chart.grid.labels.yAxis.scaleMax", null), o = X(i) ? Number(i) : Math.min(0, ...r), s = X(a) ? Number(a) : Math.max(1, ...r), c = k(I(e, "chart.grid.labels.yAxis.commonScaleSteps", 6), 6);
	return !r.length && !X(i) && !X(a) ? t : Oe(e, o, s === o ? o + 1 : s, c);
}
function pe(e, t) {
	let { drawingArea: n } = e, r = k(t.__yOffset, 0), i = k(t.__individualHeight, n.height);
	return {
		top: n.bottom - r - i,
		bottom: n.bottom - r
	};
}
function H(e, t, n, r) {
	let i = pe(e, t), a = Math.min(n.y, r), o = Math.max(n.y, r), s = Math.max(i.top, a), c = Math.min(i.bottom, o);
	return {
		y: s,
		height: Math.max(1e-5, c - s)
	};
}
function me(e, t) {
	let n = {};
	return M(t).forEach((t, r) => {
		let i = le(t) || `serie_${r}`, a = M(t.plots).map((e) => Number(e.value)).filter(Number.isFinite);
		n[i] || (n[i] = {
			scaleLabel: i,
			name: t.name ?? i,
			color: I(e, "chart.grid.labels.yAxis.groupColor", null) || t.color,
			series: [],
			values: []
		}), n[i].series.push(t), n[i].values.push(...a);
	}), Object.values(n).forEach((t) => {
		t.scale = ke(e, {
			...t.series[0],
			plots: t.series.flatMap((e) => M(e.plots))
		});
	}), n;
}
function he(e, t, n, r, i, a) {
	return !i && !a ? r : n[le(t)]?.scale ?? ke(e, t);
}
function ge(e) {
	return e === "bar" || e === "line" || e === "plot" ? e : "line";
}
function _e(e) {
	return M(e.series).map((e) => ({
		...e,
		type: ge(e.type),
		plots: M(e.plots).filter((e) => e && F(e.x))
	}));
}
function U(e) {
	let { drawingArea: t, scale: n } = e, r = (0 - n.min) / (n.max - n.min || 1);
	return t.bottom - r * t.height;
}
var ve = C();
function W(e) {
	return ve;
}
function G(e, t, n) {
	return `rectGradient_${n}_${t}_${W(e)}`;
}
function ye(e, t) {
	return `lineGradient_${t}_${W(e)}`;
}
function be(e, t) {
	return `temperature_grad_line_${t}_${W(e)}`;
}
function xe(e, t, n) {
	return t.temperatureColors && !t.isFlatTemperatureLine ? `url(#${be(e, n)})` : t.color;
}
function Se(e, t) {
	return `plotGradient_${t}_${W(e)}`;
}
function Ce(e, t, n) {
	let { config: r } = e;
	return I(r, "line.useGradient", !1) ? `url(#${ye(e, n)})` : I(r, "line.dot.useSerieColor", !0) ? t.color : I(r, "line.dot.fill", "#FFFFFF");
}
function we(e, t, r) {
	let { config: i } = e;
	return I(i, "line.area.useGradient", !1) ? `url(#${rt(e, r)})` : n(t.color, k(I(i, "line.area.opacity", 30), 30));
}
function K(e, t, n, r) {
	return I(e.config, "bar.useGradient", !1) ? r >= 0 ? `url(#${G(e, n, "pos")})` : `url(#${G(e, n, "neg")})` : t.color;
}
function Te(e, t) {
	let { config: n } = e, r = [];
	return I(n, "bar.useGradient", !1) && t.filter((e) => e.type === "bar").forEach((t, n) => {
		r.push(L("linearGradient", {
			id: G(e, n, "pos"),
			x2: "0%",
			y2: "100%"
		}, [
			A("stop", {
				offset: "0%",
				"stop-color": f(t.color)
			}),
			A("stop", {
				offset: "62%",
				"stop-color": h(f(t.color), .02)
			}),
			A("stop", {
				offset: "100%",
				"stop-color": h(f(t.color), .05)
			})
		].join(""))), r.push(L("linearGradient", {
			id: G(e, n, "neg"),
			x2: "0%",
			y2: "100%"
		}, [
			A("stop", {
				offset: "0%",
				"stop-color": h(f(t.color), .05)
			}),
			A("stop", {
				offset: "38%",
				"stop-color": h(f(t.color), .02)
			}),
			A("stop", {
				offset: "100%",
				"stop-color": f(t.color)
			})
		].join("")));
	}), t.filter((e) => e.type === "line").forEach((t, i) => {
		I(n, "line.useGradient", !1) && r.push(L("radialGradient", {
			id: ye(e, i),
			cx: "50%",
			cy: "50%",
			r: "50%",
			fx: "50%",
			fy: "50%"
		}, [A("stop", {
			offset: "0%",
			"stop-color": h(f(t.color), .05)
		}), A("stop", {
			offset: "100%",
			"stop-color": f(t.color)
		})].join(""))), t.temperatureColors && !t.isFlatTemperatureLine && r.push(L("linearGradient", {
			id: be(e, i),
			gradientTransform: "rotate(90)"
		}, t.temperatureColors.map((e, n) => A("stop", {
			offset: E(n, t.temperatureColors.length),
			"stop-color": f(e)
		})).join("")));
	}), t.filter((e) => e.type === "plot").forEach((t, i) => {
		I(n, "plot.useGradient", !1) && r.push(L("radialGradient", {
			id: Se(e, i),
			cx: "50%",
			cy: "50%",
			r: "50%",
			fx: "50%",
			fy: "50%"
		}, [A("stop", {
			offset: "0%",
			"stop-color": h(f(t.color), .05)
		}), A("stop", {
			offset: "100%",
			"stop-color": f(t.color)
		})].join("")));
	}), r.length ? L("defs", { "data-layer": "gradients" }, r.join("")) : "";
}
function Ee(e) {
	let t = I(e.config, "chart.backgroundColor", "#FFFFFF");
	return A("rect", {
		width: e.width,
		height: e.height,
		fill: t
	});
}
function q(e) {
	return {
		dataLabels: k(I(e, "chart.grid.labels.fontSize", 12), 12),
		xAxis: k(I(e, "chart.grid.labels.xAxisLabels.fontSize", 12), 12),
		yAxis: k(I(e, "chart.grid.labels.axis.fontSize", 12), 12)
	};
}
function J(e) {
	return I(e, "chart.grid.labels.yAxis.position", "left") === "right";
}
function De(e, t) {
	let n = g({
		p: I(t, "chart.labels.prefix", ""),
		v: e,
		s: I(t, "chart.labels.suffix", ""),
		r: I(t, "chart.grid.labels.yAxis.rounding", 0)
	});
	return y(I(t, "chart.grid.labels.yAxis.formatter", null), e, n, t);
}
function Y(e) {
	return !!I(e, "chart.grid.labels.yAxis.stacked", !1);
}
function X(e) {
	return e != null && Number.isFinite(Number(e));
}
function Oe(e, t, n, r) {
	return I(e, "chart.grid.labels.yAxis.useNiceScale", !0) ? _(t, n, r) : te(t, n, r);
}
function ke(e, t) {
	let n = M(t.plots).map((e) => Number(e.value)).filter(Number.isFinite), r = I(e, "chart.grid.labels.yAxis.scaleMin", null), i = I(e, "chart.grid.labels.yAxis.scaleMax", null), a = X(r) ? Number(r) : X(t.scaleMin) ? Number(t.scaleMin) : Math.min(0, ...n), o = X(i) ? Number(i) : X(t.scaleMax) ? Number(t.scaleMax) : Math.max(1, ...n);
	return Oe(e, a, o === a ? a + 1 : o, k(t.scaleSteps ?? I(e, "chart.grid.labels.yAxis.commonScaleSteps", 6), 6));
}
function Ae(e, t, n = []) {
	let r = q(e), i = Y(e), a = V(e), o = ue(e);
	if (a && !i) {
		let t = Object.keys(me(e, n)).length;
		return {
			left: J(e) ? 0 : t * o,
			right: J(e) ? t * o : 0,
			scaleLabelsOffset: t * o,
			yAxisLabelWidth: 0
		};
	}
	if (a && i) return {
		left: J(e) ? 0 : o,
		right: J(e) ? o : 0,
		scaleLabelsOffset: o,
		yAxisLabelWidth: 0
	};
	let s = (i ? n.flatMap((t) => ke(e, t).ticks) : M(t.ticks)).reduce((t, n) => Math.max(t, j(De(n, e), r.dataLabels)), 0), c = I(e, "chart.grid.labels.axis.yLabel", "") ? r.yAxis * 2 + 24 + k(I(e, "chart.grid.labels.axis.yLabelOffsetX", 0), 0) + r.yAxis : 0, l = s + k(I(e, "chart.grid.labels.yAxis.scaleValueOffsetX", 0), 0) + k(I(e, "chart.grid.labels.yAxis.crosshairSize", 0), 0);
	return {
		left: J(e) ? 0 : l + c,
		right: J(e) ? l + c : 0,
		scaleLabelsOffset: l,
		yAxisLabelWidth: c
	};
}
function je(e, t) {
	let { config: n, scale: r, width: i, height: a } = e, o = q(n);
	e.drawingArea;
	let s = Ae(n, r, t), c = I(n, "chart.legend.position", "bottom"), l = I(n, "chart.legend.show", !0), u = l ? tt({
		...e,
		width: i,
		height: a
	}, t) : { height: 0 }, d = l && c === "top" ? u.height : 0, f = l && c !== "top" ? u.height : 0, p = I(n, "chart.grid.labels.axis.xLabel", ""), m = I(n, "chart.grid.labels.xAxisLabels.show", !0) ? o.xAxis * 2 : 0, h = (p ? o.yAxis * 1.5 : 0) + m + o.xAxis, g = gt(n) + k(I(n, "chart.labels.fontSize", 12), 12) * 1.1, _ = k(I(n, "chart.padding.top", 12), 12), v = k(I(n, "chart.padding.right", 12), 12), y = k(I(n, "chart.padding.bottom", 12), 12), b = k(I(n, "chart.padding.left", 12), 12), x = k(I(n, "chart.grid.labels.axis.xLabelOffsetY", 0), 0), S = k(I(n, "chart.grid.labels.yAxis.crosshairSize", 0), 0), C = s.left, w = s.right, te = i - C - w - 6 - b - v, T = {
		top: _ + g + d,
		right: C + (J(n) ? 0 : S) + b + te,
		bottom: a - h - y - x - f,
		left: C + (J(n) ? 0 : S) + b,
		width: Math.max(1, te),
		height: 1,
		scaleLabelX: C,
		rightScaleLabelX: w,
		scaleLabelsOffset: s.scaleLabelsOffset,
		yAxisLabelWidth: s.yAxisLabelWidth,
		individualOffsetX: 36
	};
	T.height = Math.max(1, T.bottom - T.top);
	let E = Y(n), D = E ? ee(t) : t, O = Math.max(1, D.length), ne = E ? k(I(n, "chart.grid.labels.yAxis.gap", 12), 12) : 0, A = ne * Math.max(0, O - 1), j = Math.max(1, T.height - A), N = V(n), P = me(n, t), F = Math.max(1, ...t.map((e) => M(e.plots).length));
	function L(t, n) {
		if (Z(e)) {
			let n = Number(t.rawX);
			return Number.isFinite(n) ? We({
				...e,
				drawingArea: T
			}, n) : null;
		}
		return Q({
			...e,
			drawingArea: T
		}, t.index ?? n, F);
	}
	let R = t.map((e, t) => {
		let i = e.id !== void 0 && e.id !== null ? D.find((t) => t.id === e.id) ?? D[t] ?? e : D[t] ?? e, a = he(n, e, P, r, N, E), o = k(i.stackIndex, t), s = k(i.stackRatio, 1 / O), c = k(i.cumulatedStackRatio, s * (o + 1)), l = O - 1 - o, u = E ? 1 - c : 0, d = E ? j * u + ne * l : 0, f = E ? j * s : T.height;
		function p(e) {
			let t = (e - a.min) / (a.max - a.min || 1);
			return T.bottom - d - t * f;
		}
		let m = p(0);
		return {
			...e,
			__scale: a,
			__yOffset: d,
			__individualHeight: f,
			__zeroY: m,
			__scaleYLabels: M(a.ticks).map((t) => ({
				value: t,
				y: p(t),
				serie: e
			})),
			plots: e.plots.map((e, t) => {
				let n = e.value === null || e.value === void 0 ? null : Number(e.value);
				return {
					...e,
					value: n,
					x: L(e, t),
					y: n === null || !Number.isFinite(n) ? null : p(n)
				};
			})
		};
	});
	return {
		...e,
		drawingArea: T,
		series: R,
		__scaleGroups: P,
		__useIndividualScale: N
	};
}
function Me(e) {
	let { config: t, drawingArea: n, scale: r, series: i } = e;
	if (!I(t, "chart.grid.labels.show", !0) || !I(t, "chart.grid.labels.yAxis.show", !0)) return "";
	let a = V(t), o = Y(t);
	if (a) return Ne(e);
	let s = q(t), c = J(t), l = I(t, "chart.grid.labels.yAxis.showCrosshairs", !1), u = k(I(t, "chart.grid.labels.yAxis.crosshairSize", 0), 0), d = k(I(t, "chart.grid.labels.yAxis.scaleValueOffsetX", 0), 0), f = I(t, "chart.grid.labels.color", "#2A2A2A"), p = I(t, "chart.grid.stroke", "#CCCCCC"), m = o ? i.flatMap((e) => M(e.__scaleYLabels)) : M(r.ticks).map((e) => {
		let t = (e - r.min) / (r.max - r.min || 1);
		return {
			value: e,
			y: n.bottom - t * n.height
		};
	}), h = [];
	return m.forEach((e) => {
		Number.isFinite(e.y) && (l && h.push(A("line", {
			"data-cy": "axis-y-tick",
			x1: c ? n.right : n.left,
			x2: c ? n.right + u : n.left - u,
			y1: e.y,
			y2: e.y,
			stroke: p,
			"stroke-width": 1,
			"stroke-linecap": "round"
		})), h.push(R(De(e.value, t), {
			"data-cy": "axis-y-label",
			transform: `translate(${c ? n.right + u + d + 5 : n.scaleLabelX - u}, ${e.y + s.dataLabels / 3})`,
			"font-size": s.dataLabels,
			"text-anchor": c ? "start" : "end",
			fill: o && e.serie?.color ? I(t, "chart.grid.labels.yAxis.groupColor", null) || e.serie.color : f
		})));
	}), L("g", { "data-layer": "scale-labels" }, h.join(""));
}
function Ne(e) {
	let { config: t, drawingArea: n, series: r } = e, i = q(t), a = J(t), o = Y(t), s = e.__scaleGroups ?? me(t, r), c = I(t, "chart.grid.labels.yAxis.showCrosshairs", !1), l = k(I(t, "chart.grid.labels.yAxis.crosshairSize", 0), 0), u = k(I(t, "chart.grid.labels.yAxis.scaleValueOffsetX", 0), 0), d = k(I(t, "chart.grid.labels.yAxis.labelWidth", 40), 40), f = k(n.individualOffsetX, 36), p = [], m = Object.values(s);
	return m.forEach((e, s) => {
		let h = r.find((t) => le(t) === e.scaleLabel) ?? r[s];
		if (!h) return;
		let g = o ? a ? n.right : n.left : a ? n.right + ue(t) * s : n.left - ue(t) * (m.length - s - 1), _ = k(h.__yOffset, 0), v = k(h.__individualHeight, n.height), y = o ? n.bottom - _ - v : n.top, b = o ? n.bottom - _ : n.bottom;
		p.push(A("line", {
			x1: o || a ? g : g - f,
			x2: o || a ? g : g - f,
			y1: y,
			y2: b,
			stroke: e.color,
			"stroke-width": I(t, "chart.grid.strokeWidth", 1),
			"stroke-linecap": "round"
		}));
		let x = o ? a ? n.right + l + u + 5 + d + f : n.left - l - u - d : a ? g + l + u + 5 + d : g - i.dataLabels / 2;
		p.push(R(e.name, {
			transform: `translate(${x}, ${y + (b - y) / 2}) rotate(-90)`,
			"font-size": i.dataLabels * .8,
			"text-anchor": "middle",
			fill: e.color
		})), M(e.scale.ticks).forEach((r) => {
			let s = (r - e.scale.min) / (e.scale.max - e.scale.min || 1), d = b - s * (b - y);
			Number.isFinite(d) && (c && p.push(A("line", {
				"data-cy": "axis-y-tick",
				x1: o || a ? g : g + 3 - l - f,
				x2: o ? a ? g + l : g - l : a ? g + l : g - f,
				y1: d,
				y2: d,
				stroke: e.color,
				"stroke-width": 1,
				"stroke-linecap": "round"
			})), p.push(R(De(r, t), {
				"data-cy": "axis-y-label",
				transform: `translate(${o ? a ? n.right + l + u + 5 : n.left - l - u - 5 : a ? g + l + u + 5 : g - 5 - f}, ${d + i.dataLabels / 3})`,
				"font-size": i.dataLabels,
				"text-anchor": a ? "start" : "end",
				fill: e.color
			})));
		});
	}), L("g", { "data-layer": "individual-scale-labels" }, p.join(""));
}
function Pe(e) {
	return [...new Set(e.flatMap((e) => M(e.plots)).map((e) => Number(e.x)).filter(Number.isFinite))].sort((e, t) => e - t);
}
function Fe(e, t, n = 1) {
	let { drawingArea: r, config: i } = e, a = Pe(t), o = r.width * .05;
	if (a.length < 2) return Math.max(1e-5, o / Math.max(1, n));
	let s = a.slice(1).reduce((e, t, n) => {
		let r = a[n], i = Math.abs(t - r);
		return i > 0 ? Math.min(e, i) : e;
	}, Infinity), c = k(I(i, "bar.periodGap", .2), .2), l = Number.isFinite(s) && s > 0 ? s * (1 - c) : o;
	return Math.max(1e-5, l / Math.max(1, n));
}
function Ie(e, t, n) {
	let { config: r } = e, i = k(I(r, "bar.innerGap", .05), .05), a = I(r, "bar.borderRadius", 0), o = [];
	if (n) {
		let n = Fe(e, t, 1), r = n * Math.min(Math.abs(i), .95), s = Math.max(1e-5, n - r);
		return t.forEach((t, n) => {
			let r = Number.isFinite(Number(t.__zeroY)) ? Number(t.__zeroY) : U(e);
			M(t.plots).forEach((i) => {
				if (!i || i.value === null || i.value === void 0 || !Number.isFinite(Number(i.value)) || !Number.isFinite(Number(i.x)) || !Number.isFinite(Number(i.y))) return;
				let c = i.x - s / 2, { y: l, height: u } = H(e, t, i, r);
				i.__barLabelX = i.x, o.push(A("rect", {
					"data-cy": "datapoint-bar",
					x: c,
					y: l,
					width: s,
					height: u,
					rx: a,
					fill: K(e, t, n, Number(i.value)),
					...$(e, t)
				}));
			});
		}), L("g", { "data-layer": "bars" }, o.join(""));
	}
	let s = Math.max(1, t.length), c = Fe(e, t, s), l = c * Math.min(Math.abs(i), .95), u = Math.max(1e-5, c - l), d = c * s;
	return t.forEach((t, n) => {
		let r = Number.isFinite(Number(t.__zeroY)) ? Number(t.__zeroY) : U(e);
		M(t.plots).forEach((i) => {
			if (!i || i.value === null || i.value === void 0 || !Number.isFinite(Number(i.value)) || !Number.isFinite(Number(i.x)) || !Number.isFinite(Number(i.y))) return;
			let s = i.x - d / 2 + c * n + l / 2, { y: f, height: p } = H(e, t, i, r);
			i.__barLabelX = s + u / 2, o.push(A("rect", {
				"data-cy": "datapoint-bar",
				x: s,
				y: f,
				width: u,
				height: p,
				rx: a,
				fill: K(e, t, n, Number(i.value)),
				...$(e, t)
			}));
		});
	}), L("g", { "data-layer": "bars" }, o.join(""));
}
function Le(e, t) {
	let { config: n, drawingArea: r } = e, i = Y(n), a = t.filter((e) => e.type === "bar");
	if (!a.length) return "";
	if (Z(e)) return Ie(e, a, i);
	let o = Math.max(1, ...t.map((e) => M(e.plots).length)), s = r.width / o, c = k(I(n, "bar.periodGap", .2), .2), l = k(I(n, "bar.innerGap", .05), .05), u = I(n, "bar.borderRadius", 0), d = [];
	if (i) {
		let t = Math.max(1e-5, s * .9), n = t * Math.min(Math.abs(l), .95);
		return a.forEach((i, a) => {
			let o = Number.isFinite(Number(i.__zeroY)) ? Number(i.__zeroY) : U(e);
			M(i.plots).forEach((c, l) => {
				if (!c || c.value === null || c.value === void 0 || !Number.isFinite(Number(c.value)) || !Number.isFinite(Number(c.y))) return;
				let f = r.left + s * l + s * .05 + n / 2, { y: p, height: m } = H(e, i, c, o);
				c.__barLabelX = f + Math.max(1e-5, t - n) / 2, d.push(A("rect", {
					"data-cy": "datapoint-bar",
					x: f,
					y: p,
					width: Math.max(1e-5, t - n),
					height: m,
					rx: u,
					fill: K(e, i, a, Number(c.value)),
					...$(e, i)
				}));
			});
		}), L("g", { "data-layer": "bars" }, d.join(""));
	}
	let f = s * (1 - c), p = f / Math.max(1, a.length), m = p * Math.min(Math.abs(l), .95);
	return U(e), a.forEach((t, n) => {
		let i = Number.isFinite(Number(t.__zeroY)) ? Number(t.__zeroY) : U(e);
		M(t.plots).forEach((a, o) => {
			if (!a || a.value === null || a.value === void 0 || !Number.isFinite(Number(a.value)) || !Number.isFinite(Number(a.y))) return;
			let c = r.left + s * o + (s - f) / 2 + p * n + m / 2, { y: l, height: h } = H(e, t, a, i);
			a.__barLabelX = c + Math.max(1e-5, p - m) / 2, d.push(A("rect", {
				"data-cy": "datapoint-bar",
				x: c,
				y: l,
				width: Math.max(1e-5, p - m),
				height: h,
				rx: u,
				fill: K(e, t, n, Number(a.value)),
				...$(e, t)
			}));
		});
	}), L("g", { "data-layer": "bars" }, d.join(""));
}
function Re(e) {
	let { config: t, drawingArea: n, scale: r, series: i } = e;
	return V(t) || Y(t) ? i.flatMap((e) => M(e.__scaleYLabels)) : M(r.ticks).map((e) => {
		let t = (e - r.min) / (r.max - r.min || 1);
		return {
			value: e,
			y: n.bottom - t * n.height
		};
	});
}
function ze(e, t, n) {
	let { drawingArea: r } = e, i = r.width / Math.max(1, n);
	return r.left + i * t;
}
function Be(e, t) {
	let { config: n, drawingArea: r } = e, i = Math.max(1, ...t.map((e) => M(e.plots).length)), a = r.top, o = r.bottom;
	if (Z(e)) {
		let e = [...new Set(t.flatMap((e) => M(e.plots)).map((e) => Number(e.x)).filter(Number.isFinite))].sort((e, t) => e - t);
		return e.length ? I(n, "chart.grid.position", "middle") === "middle" ? e.map((e, t, n) => {
			if (t === 0) return null;
			let r = n[t - 1], i = r + (e - r) / 2;
			return `M${i},${a} L${i},${o}`;
		}).filter(Boolean).join(" ") : e.map((e) => `M${e},${a} L${e},${o}`).join(" ") : "";
	}
	let s = i + +(I(n, "chart.grid.position", "middle") === "middle");
	return Array.from({ length: s }).map((t, r) => {
		let s = I(n, "chart.grid.position", "middle") === "middle" ? ze(e, r, i) : Q(e, r, i);
		return `M${s},${a} L${s},${o}`;
	}).join(" ");
}
function Ve(e) {
	let { config: t, drawingArea: n, scale: r, series: i } = e, a = I(t, "chart.grid.show", !0), o = I(t, "chart.grid.showHorizontalLines", !0), s = I(t, "chart.grid.showVerticalLines", !1), c = I(t, "chart.grid.stroke", "#CCCCCC"), l = k(I(t, "chart.grid.strokeWidth", .5), .5), u = [];
	if (a && o && Re(e).forEach((e) => {
		Number.isFinite(Number(e.y)) && u.push(A("line", {
			"data-cy": "xy-grid-horizontal-line",
			x1: n.left,
			x2: n.right,
			y1: e.y,
			y2: e.y,
			stroke: c,
			"stroke-width": l,
			"stroke-linecap": "round"
		}));
	}), a && s) {
		let t = Be(e, i);
		t && u.push(A("path", {
			"data-cy": "xy-grid-vertical-line",
			d: t,
			stroke: c,
			"stroke-width": l,
			"stroke-linecap": "round"
		}));
	}
	u.push(A("line", {
		x1: n.left,
		y1: n.bottom,
		x2: n.right,
		y2: n.bottom,
		stroke: c,
		"stroke-width": l,
		"stroke-linecap": "round"
	}));
	let d = J(t) ? n.right : n.left;
	return u.push(A("line", {
		x1: d,
		y1: n.top,
		x2: d,
		y2: n.bottom,
		stroke: c,
		"stroke-width": l,
		"stroke-linecap": "round"
	})), r.min < 0 && r.max > 0 && u.push(A("line", {
		x1: n.left,
		y1: U(e),
		x2: n.right,
		y2: U(e),
		stroke: c,
		"stroke-width": l,
		"stroke-dasharray": 4,
		"stroke-linecap": "round"
	})), L("g", { "data-layer": "grid" }, u.join(""));
}
function He(e) {
	return e && typeof e == "object" && Number.isFinite(Number(e.x)) && Number.isFinite(Number(e.y));
}
function Z(e) {
	return e.isContinuous ? !0 : M(e.series).some((e) => M(e.series).some(He));
}
function Ue(e) {
	if (e.xScale && Number.isFinite(Number(e.xScale.min)) && Number.isFinite(Number(e.xScale.max))) return {
		min: Number(e.xScale.min),
		max: Number(e.xScale.max)
	};
	let t = M(e.series).flatMap((e) => M(e.series)).filter(He).map((e) => Number(e.x));
	if (!t.length) return {
		min: 0,
		max: 1
	};
	let n = Math.min(...t), r = Math.max(...t);
	return n === r ? {
		min: n - 1,
		max: r + 1
	} : {
		min: n,
		max: r
	};
}
function We(e, t) {
	let { drawingArea: n, config: r } = e, i = Ue(e), a = (Number(t) - i.min) / (i.max - i.min || 1), o = I(r, "chart.grid.labels.xAxis.reverse", !1) ? 1 - a : a;
	return n.left + n.width * o;
}
function Ge(e, t, n) {
	let { config: r } = e, i = g({
		v: t,
		p: I(r, "chart.labels.prefix", ""),
		s: I(r, "chart.labels.suffix", ""),
		r: I(r, "chart.grid.labels.xAxis.rounding", 0)
	});
	return y(I(r, "chart.grid.labels.xAxis.formatter", null), t, i, {
		datapoint: t,
		seriesIndex: null
	});
}
function Ke(e) {
	let { xScale: t, config: n } = e, r = M(t?.ticks);
	return (I(n, "chart.grid.labels.xAxis.reverse", !1) ? [...r].reverse() : r).map((t, n) => ({
		id: `continuous_x_label_${n}`,
		text: Ge(e, t, n),
		value: t,
		x: We(e, t),
		index: n,
		absoluteIndex: n
	}));
}
function Q(e, t, n) {
	let { drawingArea: r, config: i } = e, a = I(i, "chart.grid.position", "middle"), o = r.width, s = a === "middle" ? o / Math.max(1, n) : o / Math.max(1, n - 1);
	return a === "middle" ? r.left + s / 2 + s * t : r.left + s * t;
}
async function qe(e, t) {
	let { config: n } = e, r = Math.max(0, ...t.map((e) => M(e.plots).length));
	return await D({
		values: I(n, "chart.grid.labels.xAxisLabels.values", []),
		maxDatapoints: r,
		formatter: I(n, "chart.grid.labels.xAxisLabels.datetimeFormatter", null),
		start: k(e.slotStartIndex ?? e.startAbs, 0),
		end: k(e.slotEndIndex ?? e.endAbs, r)
	});
}
async function Je(e, t) {
	let { config: n } = e, r = I(n, "chart.grid.labels.xAxisLabels", {}), i = Math.max(0, ...t.map((e) => M(e.plots).length)), a = await qe(e, t), o = await D({
		values: I(n, "chart.grid.labels.xAxisLabels.values", []),
		maxDatapoints: i,
		formatter: I(n, "chart.grid.labels.xAxisLabels.datetimeFormatter", null),
		start: 0,
		end: i
	}), s = a.map((e) => e?.text ?? ""), c = o.map((e) => e?.text ?? ""), l = Math.min(k(r.modulo, 1), Math.max(1, new Set(s).size));
	return v(!!r.showOnlyFirstAndLast, !!r.showOnlyAtModulo, Math.max(1, l || 1), s, c, k(e.slotStartIndex ?? e.startAbs, 0), e.selectedXIndex ?? null, i);
}
async function Ye(e, t) {
	return Z(e) ? Ke(e) : await Je(e, t);
}
function Xe(e, t, n, r) {
	return Z(e) ? t.x : Q(e, n, r);
}
function Ze(e) {
	let t = e.userConfig ?? e.sourceConfig ?? e.props?.config ?? {};
	return T(t, "chart.grid.labels.xAxisLabels.rotation") ? k(I(e.config, "chart.grid.labels.xAxisLabels.rotation", 0), 0) : I(e.config, "chart.grid.labels.xAxisLabels.autoRotate.enable", !1) ? k(I(e.config, "chart.grid.labels.xAxisLabels.autoRotate.angle", 0), 0) : k(I(e.config, "chart.grid.labels.xAxisLabels.rotation", 0), 0);
}
async function Qe(e, t) {
	let { config: n, drawingArea: i } = e;
	if (!t[0] || !I(n, "chart.grid.labels.xAxisLabels.show", !0)) return "";
	I(n, "chart.grid.labels.xAxisLabels", {});
	let a = k(I(n, "chart.grid.labels.xAxisLabels.fontSize", 12), 12), o = I(n, "chart.grid.labels.xAxisLabels.color", I(n, "chart.grid.labels.color", "#2A2A2A")), s = Ze(e), c = Math.max(1, t[0].plots.length), l = await Ye(e, t), u = s > 0 ? "start" : s < 0 ? "end" : "middle", d = l.map((t, n) => {
		let l = typeof t == "string" ? t : t?.text;
		if (!l) return "";
		let d = `translate(${Xe(e, t, n, c)}, ${i.bottom + a * 1.5}), rotate(${s})`;
		return String(l).includes("\n") ? L("text", {
			class: "vue-data-ui-time-label",
			"data-cy": "time-label",
			"text-anchor": u,
			"font-size": a,
			fill: o,
			transform: d
		}, r({
			content: String(l),
			fontSize: a,
			fill: o,
			x: 0,
			y: 0
		})) : R(l, {
			class: "vue-data-ui-time-label",
			"data-cy": "time-label",
			"text-anchor": u,
			"font-size": a,
			fill: o,
			transform: d
		});
	}).join("");
	return L("g", { "data-layer": "x-axis-labels" }, d);
}
function $e(e) {
	let { config: t, drawingArea: n, height: r } = e, i = q(t), a = I(t, "chart.grid.labels.axis.xLabel", ""), o = I(t, "chart.grid.labels.axis.yLabel", ""), s = I(t, "chart.grid.labels.axis.color", I(t, "chart.grid.labels.color", "#2A2A2A")), c = k(I(t, "chart.grid.labels.axis.yLabelOffsetX", 0), 0), l = k(I(t, "chart.grid.labels.axis.xLabelOffsetY", 0), 0), u = k(I(t, "chart.grid.labels.yAxis.crosshairSize", 0), 0), d = k(I(t, "chart.grid.labels.yAxis.scaleValueOffsetX", 0), 0), f = k(I(t, "chart.grid.labels.yAxis.labelWidth", 0), 0), p = J(t), m = [];
	if (a && m.push(R(a, {
		x: n.left + n.width / 2,
		y: Math.min(r - 4, n.bottom + i.xAxis * 3 + l),
		"font-size": i.yAxis,
		"text-anchor": "middle",
		fill: s
	})), o) {
		let e = p ? n.right + u + d + 5 + f + c : n.scaleLabelX - u - d - f - c, t = n.top + n.height / 2;
		m.push(R(o, {
			x: e,
			y: t,
			transform: `rotate(-90 ${e} ${t})`,
			"font-size": i.yAxis,
			"text-anchor": "middle",
			fill: s
		}));
	}
	return L("g", { "data-layer": "axis-labels" }, m.join(""));
}
async function et(e, t) {
	let { config: n, drawingArea: r } = e, i = I(n, "chart.grid.labels.xAxisLabels.show", !0), a = I(n, "chart.grid.labels.xAxis.showCrosshairs", !1);
	if (!i || !a || !t[0]) return "";
	let o = k(I(n, "chart.grid.labels.xAxis.crosshairSize", 6), 6), s = !!I(n, "chart.grid.labels.xAxis.crosshairsAlwaysAtZero", !1), c = Math.max(1, t[0].plots.length), l = await Ye(e, t), u = U(e), d = l.map((t, n) => {
		if (!(typeof t == "string" ? t : t?.text)) return "";
		let i = Z(e) ? t.x : Q(e, n, c);
		return `M${i},${s ? u - (u === r.bottom ? 0 : o / 2) : r.bottom} L${i},${s ? u + o / (u === r.bottom ? 1 : 2) : r.bottom + o}`;
	}).filter(Boolean).join(" ");
	return d ? L("g", { "data-layer": "x-axis-ticks" }, A("path", {
		d,
		stroke: I(n, "chart.grid.stroke", "#CCCCCC"),
		"stroke-width": 1,
		"stroke-linecap": "round",
		"data-cy": "axis-x-tick"
	})) : "";
}
function tt(e, t) {
	let { config: n, width: r } = e, i = I(n, "chart.legend", {}), a = k(i.fontSize, 12), o = k(i.markerSize, 10), s = k(i.itemGap, 16), c = k(i.rowGap, 8), l = k(i.padding, 8), u = Math.max(1, r - l * 2), d = [], f = [], p = 0;
	return t.forEach((e) => {
		let t = String(e.name ?? ""), n = o + 6 + j(t, a) + s;
		f.length && p + n > u && (d.push({
			items: f,
			width: p
		}), f = [], p = 0), f.push({
			serie: e,
			label: t,
			itemWidth: n
		}), p += n;
	}), f.length && d.push({
		items: f,
		width: p
	}), {
		rows: d,
		fontSize: a,
		markerSize: o,
		itemGap: s,
		rowGap: c,
		padding: l,
		height: d.length * Math.max(o, a) + Math.max(0, d.length - 1) * c + l * 2
	};
}
function nt(e, t) {
	let { config: n, width: r, height: i } = e;
	if (!I(n, "chart.legend.show", !0) || !t.length) return "";
	let a = I(n, "chart.legend", {}), o = a.position || "bottom", s = a.color || I(n, "chart.color", "#2A2A2A"), c = tt(e, t), l = Math.max(c.markerSize, c.fontSize), u = o === "top" ? c.padding + l : i - c.height + c.padding + l, d = [];
	return c.rows.forEach((e, t) => {
		let n = Math.max(c.padding, r / 2 - e.width / 2), i = u + t * (l + c.rowGap);
		e.items.forEach(({ serie: e, label: t, itemWidth: r }) => {
			d.push(A("rect", {
				x: n,
				y: i - c.markerSize / 2,
				width: c.markerSize,
				height: c.markerSize,
				rx: 2,
				fill: e.color
			})), d.push(R(t, {
				x: n + c.markerSize + 6,
				y: i + 2,
				"font-size": c.fontSize,
				"dominant-baseline": "middle",
				fill: s
			})), n += r;
		});
	}), L("g", { "data-layer": "legend" }, d.join(""));
}
function rt(e, t) {
	return `areaGradient_${t}_${W()}`;
}
function it(e, t) {
	let { config: r } = e;
	if (!I(r, "line.area.useGradient", !1)) return "";
	let i = k(I(r, "line.area.opacity", 30), 30), a = t.filter((e) => e.type === "line" && e.useArea).map((t, r) => L("linearGradient", {
		id: rt(e, r),
		x1: "0%",
		x2: "0%",
		y1: "0%",
		y2: "100%"
	}, [A("stop", {
		offset: "0%",
		"stop-color": n(h(f(t.color), .03), i),
		"stop-opacity": 1
	}), A("stop", {
		offset: "100%",
		"stop-color": t.color,
		"stop-opacity": 0
	})].join(""))).join("");
	return a ? L("defs", {}, a) : "";
}
function at(n, r) {
	let { config: i } = n, a = !!I(i, "line.cutNullValues", !1), o = [];
	return r.filter((e) => e.type === "line" && e.useArea).forEach((r, i) => {
		let s = M(r.plots), c = s.filter((e) => e && e.value !== null);
		if (c.length < 2) return;
		let u = Number.isFinite(Number(r.__zeroY)) ? Number(r.__zeroY) : U(n), d = we(n, r, i);
		if (r.smooth && !r.useStepper) {
			l(a ? s : c, u, a).forEach((e) => {
				e && o.push(A("path", {
					d: e,
					fill: d,
					"data-cy": "datapoint-line-area-smooth"
				}));
			});
			return;
		}
		(r.useStepper ? t(a ? s : c, u) : a ? e(s, u) : b(c, u)).split(";").filter(Boolean).forEach((e) => {
			o.push(A("path", {
				d: `M${e}Z`,
				fill: d,
				"data-cy": "datapoint-line-area-straight"
			}));
		});
	}), L("g", { "data-layer": "line-areas" }, o.join(""));
}
function ot(e, t) {
	let n = k(t.slotStartIndex ?? t.startAbs, 0);
	return M(e.dashIndices).map((e) => Number(e) - n).filter(Number.isFinite);
}
function st(e, t, n, r, i) {
	let { config: a } = e, o = k(t.strokeWidth || I(a, "line.strokeWidth", 2), 2);
	return A("path", {
		d: `M${r}`,
		fill: "none",
		stroke: xe(e, t, n),
		"stroke-width": o,
		"stroke-linecap": "round",
		"stroke-linejoin": "round",
		"stroke-dasharray": i ? o * 2 : 0
	});
}
function ct(e, n) {
	let { config: r } = e, o = n.filter((e) => e.type === "line"), s = !!I(r, "line.cutNullValues", !1), l = [];
	return o.forEach((n, o) => {
		let d = s ? M(n.plots) : M(n.plots).filter((e) => e.value !== null), f = d.filter((e) => e.value !== null);
		if (!(f.length < 2)) {
			if (M(n.dashIndices).length > 0 && !n.useStepper) {
				let t = ot(n, e);
				(n.smooth || I(r, "line.smooth", !1) ? c(d, t) : p(d, t)).forEach((t) => {
					t?.path && l.push(st(e, n, o, t.path, !!t.dashed));
				});
				return;
			}
			(n.useStepper ? t(d).split(";").filter(Boolean) : [n.smooth || I(r, "line.smooth", !1) ? s ? a(d) : u(f) : s ? i(d) : m(f, !1, !0)]).forEach((t) => {
				t && l.push(st(e, n, o, t, !!n.dashed));
			});
		}
	}), L("g", { "data-layer": "lines" }, l.join(""));
}
function lt(e, t, n) {
	let { config: r } = e;
	return I(r, "plot.useGradient", !1) ? `url(#${Se(e, n)})` : I(r, "plot.dot.useSerieColor", !0) ? t.color : I(r, "plot.dot.fill", t.color);
}
function $(e, t) {
	let n = I(e.config, "bar.border", {});
	return {
		stroke: n.useSerieColor ? t.color : n.stroke,
		"stroke-width": k(n.strokeWidth, 0)
	};
}
function ut(e, t) {
	let { config: n } = e, r = t.filter((e) => e.type === "plot"), i = I(n, "plot.radius", 4), a = I(n, "plot.dot.strokeWidth", 2), o = I(n, "chart.backgroundColor", "#FFFFFF"), s = [];
	return r.forEach((t, r) => {
		t.plots.forEach((c, l) => {
			F(c.value) && s.push(N({
				dataCy: `xy-plot-${r}-${l}`,
				shape: t.shape,
				plot: c,
				radius: t.radius || i,
				fill: lt(e, t, r),
				stroke: I(n, "plot.dot.useSerieColor", !0) ? o : t.color,
				strokeWidth: a
			}));
		});
	}), L("g", { "data-layer": "plots" }, s.join(""));
}
function dt(e) {
	return ![
		null,
		void 0,
		NaN,
		Infinity,
		-Infinity
	].includes(e);
}
function ft(e, t, n) {
	let r = e[t - 1], i = e[t + 1], a = !!r && !!i && r.value == null && i.value == null || !r && !!i && i.value == null || !!r && !i && r.value == null;
	return dt(e[t]?.value) && a && !!I(n, "line.cutNullValues", !1);
}
function pt(e, t, n, r, i) {
	let { config: a } = e;
	if (!n || !dt(n.value)) return !1;
	if (!(i > k(I(a, "line.dot.hideAboveMaxSerieLength", Infinity), Infinity))) return !0;
	let o = e.selectedSerieIndex ?? null, s = e.selectedMinimapIndex ?? null;
	return o !== null && o === r || s !== null && s === r || ft(t.plots, r, a);
}
function mt(e, t) {
	let { config: n } = e, r = t.filter((e) => e.type === "line"), i = Math.max(0, ...r.map((e) => M(e.plots).length)), a = I(n, "line.radius", 4), o = I(n, "line.dot.strokeWidth", 2), s = I(n, "chart.backgroundColor", "#FFFFFF"), c = [];
	return r.forEach((t, r) => {
		t.plots.forEach((l, u) => {
			pt(e, t, l, u, i) && c.push(N({
				dataCy: "datapoint-line-plot",
				shape: t.shape,
				plot: l,
				radius: t.radius || a,
				fill: Ce(e, t, r),
				stroke: I(n, "line.dot.useSerieColor", !0) ? s : t.color,
				strokeWidth: o
			}));
		});
	}), L("g", { "data-layer": "line-dots" }, c.join(""));
}
function ht(e, t) {
	let { config: n } = e;
	if (Y(n)) return "";
	let r = I(n, "line.interLine", {}), i = M(r.pairs), a = M(r.colors);
	if (!i.length) return "";
	let o = t.filter((e) => e.type === "line"), s = k(r.fillOpacity, .2), c = !!I(n, "line.cutNullValues", !1), l = [];
	return i.forEach((e, t) => {
		let [n, r] = Array.isArray(e) ? e : [e?.a, e?.b];
		if (!n || !r) return;
		let i = o.find((e) => e.name === n), u = o.find((e) => e.name === r);
		if (!i || !u) return;
		let d = a?.[t]?.[0] ?? i.color, f = a?.[t]?.[1] ?? u.color;
		x({
			lineA: M(i.plots),
			lineB: M(u.plots),
			smoothA: !!i.smooth,
			smoothB: !!u.smooth,
			colorLineA: d,
			colorLineB: f,
			sampleStepPx: 2,
			cutNullValues: c
		}).forEach((e, i) => {
			l.push(A("path", {
				"data-cy": "interline-area",
				d: e.d,
				fill: e.color,
				"fill-opacity": s,
				stroke: "none",
				"pointer-events": "none",
				"data-key": `inter_${n}_${r}_${t}_${i}`
			}));
		});
	}), l.length ? L("g", { "data-layer": "interline-areas" }, l.join("")) : "";
}
function gt(e) {
	let t = I(e, "chart.title", {}), n = I(t, "subtitle", {});
	if (!t.show) return 0;
	let r = k(t.fontSize, 20), i = n.text ? k(n.fontSize, 14) : 0, a = k(t.paddingTop, 12), o = k(t.paddingBottom, 6);
	return a + r + (n.text ? i + 4 : 0) + o;
}
function _t(e) {
	let { config: t, width: n } = e, r = I(t, "chart.title", {}), i = I(r, "subtitle", {});
	if (!r.show || !r.text) return "";
	let a = r.textAlign || "center", o = a === "left" ? k(r.paddingLeft, 12) : a === "right" ? n - k(r.paddingRight, 12) : n / 2, s = a === "left" ? "start" : a === "right" ? "end" : "middle", c = k(r.fontSize, 20), l = k(i.fontSize, 14), u = k(r.paddingTop, 12) + c, d = [R(r.text, {
		"data-cy": "xy-div-title",
		x: o,
		y: u,
		"font-size": c,
		"font-weight": r.bold ? "700" : "400",
		"text-anchor": s,
		fill: r.color || I(t, "chart.color", "#2A2A2A")
	})];
	return i.text && d.push(R(i.text, {
		"data-cy": "xy-div-subtitle",
		x: o,
		y: u + l + 4,
		"font-size": l,
		"font-weight": i.bold ? "700" : "400",
		"text-anchor": s,
		fill: i.color || r.color || I(t, "chart.color", "#2A2A2A")
	})), L("g", { "data-layer": "title" }, d.join(""));
}
function vt(e) {
	if (!e.svgTitle) return "";
	let t = [e.svgTitle];
	return L("desc", { "aria-hidden": "true" }, t.join(""));
}
function yt(e, t, n, r, i) {
	return !n || n.value === null || n.value === void 0 || !Number.isFinite(Number(n.value)) || !I(e.config, "dataLabels.show", !0) || !I(e.config, `${i}.labels.show`, !1) ? !1 : !Object.hasOwn(t, "dataLabels") || t.dataLabels === !0 || e.selectedSerieIndex === r || e.selectedMinimapIndex === r;
}
function bt(e, t, n, i) {
	let { config: a } = e, o = k(I(a, `${i}.labels.fontSize`, I(a, "chart.labels.fontSize", 12)), 12), s = I(a, `${i}.labels.color`, I(a, "chart.color", "#2A2A2A")), c = g({
		p: t.prefix || I(a, "chart.labels.prefix", ""),
		v: n.value,
		s: t.suffix || I(a, "chart.labels.suffix", ""),
		r: I(a, `${i}.labels.rounding`, 0)
	}), l = Z(e) && Number.isFinite(Number(n.x)) ? `x: ${g({
		v: n.x,
		r: I(a, "chart.grid.labels.xAxis.rounding", 0)
	})}\ny: ${c}` : y(I(a, `${i}.labels.formatter`, null), n.value, c, {
		datapoint: n,
		serie: t
	});
	return r({
		content: l,
		fontSize: o,
		fill: s,
		x: 0,
		y: 0
	});
}
function xt(e, t, n) {
	let r = I(e.config, `${n}.labels.textAnchor`, null);
	return r === null ? k(I(e.config, `${n}.labels.rotation`, 0), 0) === 0 ? "middle" : I(e.config, `${n}.labels.alwaysOnTop`, !1) || t.value >= 0 ? "start" : "end" : r;
}
function St(e, t, n) {
	let r = k(I(e.config, `${n}.labels.offsetX`, 0), 0), i = k(I(e.config, `${n}.labels.offsetY`, -12), -12), a = k(I(e.config, `${n}.labels.rotation`, 0), 0), o = !!I(e.config, `${n}.labels.alwaysOnTop`, !1);
	return `translate(${t.x + r}, ${t.y + (o || t.value >= 0 ? i : -i * 3)}) rotate(${a})`;
}
function Ct(e, t, n) {
	let r = k(I(e.config, "bar.labels.offsetX", 0), 0), i = k(I(e.config, "bar.labels.offsetY", -6), -6), a = k(I(e.config, "bar.labels.rotation", 0), 0), o = !!I(e.config, "bar.labels.alwaysOnTop", !1), s = Number.isFinite(Number(t.__zeroY)) ? Number(t.__zeroY) : U(e), c = Math.abs(s - n.y), l = n.__barLabelX ?? n.x, u = n.y + (o ? i - (n.value < 0 ? c : 0) : n.value >= 0 ? i : -i * 3);
	return `translate(${l + r}, ${u}) rotate(${a})`;
}
function wt(e, t) {
	let { config: n } = e, r = k(I(n, "chart.labels.fontSize", 12), 12), i = I(n, "chart.backgroundColor", "#FFFFFF"), a = [];
	return t.forEach((t) => {
		M(t.plots).forEach((o, s) => {
			let c = t.type;
			[
				"line",
				"bar",
				"plot"
			].includes(c) && yt(e, t, o, s, c) && a.push(L("text", {
				"data-cy": `datapoint-${c}-label`,
				transform: c === "bar" ? Ct(e, t, o) : St(e, o, c),
				"text-anchor": xt(e, o, c),
				"font-size": I(n, `${c}.labels.fontSize`, r),
				fill: I(n, `${c}.labels.color`, I(n, "chart.color", "#2A2A2A")),
				stroke: i,
				"paint-order": "stroke"
			}, bt(e, t, o, c)));
		});
	}), L("g", { "data-layer": "datapoint-labels" }, a.join(""));
}
function Tt(e) {
	return k(e.slotStartIndex ?? e.startAbs, 0);
}
function Et(e, t) {
	let n = Math.max(0, ...t.map((e) => M(e.plots).length));
	return k(e.slotEndIndex ?? e.endAbs, n);
}
function Dt(e, t) {
	let { drawingArea: n, config: r } = e;
	return I(r, "chart.grid.position", "middle") === "middle" ? n.width / Math.max(1, t) : n.width / Math.max(1, t - 1);
}
function Ot(e, t, n) {
	let { drawingArea: r, config: i } = e, a = Dt(e, n);
	return I(i, "chart.grid.position", "middle") === "middle" ? r.left + a * t : r.left + a * t - a / 2;
}
function kt(e, t) {
	return Dt(e, t);
}
function At(e, t, n, r, i, a) {
	let { drawingArea: o } = e, s = t.caption ?? {};
	if (!s.text) return "";
	let c = kt(e, a) * i, l = s.width === "auto" ? c : k(s.width, c), u = Ot(e, r, a) - (s.width === "auto" ? 0 : l / 2 - c / 2), d = o.top + k(s.offsetY, 0);
	return L("foreignObject", {
		"data-key": `highlight_area_caption_${n}`,
		x: u,
		y: d,
		width: l,
		height: 1,
		overflow: "visible"
	}, L("div", {
		xmlns: "http://www.w3.org/1999/xhtml",
		"data-cy": "highlight-area-caption",
		style: [
			`padding:${k(s.padding, 0)}px`,
			`text-align:${s.textAlign || "center"}`,
			`font-size:${k(s.fontSize, 12)}px`,
			`color:${s.color || "#2A2A2A"}`,
			`font-weight:${s.bold ? "bold" : "normal"}`
		].join(";")
	}, S(s.text)));
}
function jt(e, t) {
	let { config: r, drawingArea: i } = e, a = M(I(r, "chart.highlightArea", []));
	if (!a.length) return "";
	let o = Math.max(1, ...t.map((e) => M(e.plots).length)), s = Tt(e), c = Et(e, t), l = [];
	return a.forEach((t, r) => {
		if (!t?.show) return;
		let a = k(t.from, 0), u = Math.min(k(t.to, a), o - 1);
		if (u < a) return;
		for (let d = a; d <= u; d += 1) {
			if (d < s || d > c - 1) continue;
			let a = d - s;
			l.push(A("rect", {
				"data-cy": "highlight-area",
				"data-key": `highlight_area_${r}_${d}`,
				x: Ot(e, a, o),
				y: i.top,
				width: kt(e, o),
				height: Math.max(1, i.height),
				fill: n(t.color, k(t.opacity, 20)),
				"pointer-events": "none"
			}));
		}
		let d = Math.max(a, s);
		d <= Math.min(u, c - 1) && l.push(At(e, t, r, d - s, u - a + 1, o));
	}), l.length ? L("g", { "data-layer": "highlight-areas" }, l.join("")) : "";
}
function Mt(e, t) {
	if (t == null) return null;
	let { drawingArea: n, scale: r } = e, i = U(e), a = r.max - r.min || 1;
	return i - (Number(t) - 0) / a * n.height;
}
function Nt(e) {
	return j(e.text, k(e.fontSize, 12));
}
function Pt(e, t, n) {
	let r = k(e.fontSize, 12), i = e.padding ?? {}, a = e.border ?? {}, o = Nt(e), s = r, c;
	return c = e.textAnchor === "middle" ? t - o / 2 - k(i.left, 0) : e.textAnchor === "end" ? t - o - k(i.right, 0) : t - k(i.left, 0), {
		x: c,
		y: n - s * .75 - k(i.top, 0),
		width: o + k(i.left, 0) + k(i.right, 0),
		height: s + k(i.top, 0) + k(i.bottom, 0),
		fill: e.backgroundColor,
		stroke: a.stroke,
		rx: a.rx,
		ry: a.ry,
		"stroke-width": a.strokeWidth
	};
}
function Ft(e) {
	let { config: t, drawingArea: r } = e;
	if (Y(t)) return "";
	let i = M(I(t, "chart.annotations", [])).filter((e) => e?.show && (e.yAxis?.yTop !== null || e.yAxis?.yBottom !== null) && (e.yAxis?.yTop !== void 0 || e.yAxis?.yBottom !== void 0));
	if (!i.length) return "";
	let a = [];
	return i.forEach((t, i) => {
		let o = t.yAxis ?? {}, s = o.label ?? {}, c = o.line ?? {}, l = o.area ?? {}, u = o.yTop, d = o.yBottom, f = u != null && d != null && u !== d, p = Mt(e, u), m = Mt(e, d), h = Number.isFinite(p), g = Number.isFinite(m);
		if (!h && !g) return;
		let _ = [];
		if (h && _.push(A("line", {
			x1: r.left,
			y1: p,
			x2: r.right,
			y2: p,
			stroke: c.stroke,
			"stroke-width": c.strokeWidth,
			"stroke-dasharray": c.strokeDasharray,
			"stroke-linecap": "round",
			"data-cy": "xy-annotation-y-top-line"
		})), g && _.push(A("line", {
			x1: r.left,
			y1: m,
			x2: r.right,
			y2: m,
			stroke: c.stroke,
			"stroke-width": c.strokeWidth,
			"stroke-dasharray": c.strokeDasharray,
			"stroke-linecap": "round",
			"data-cy": "xy-annotation-y-bottom-line"
		})), f && h && g && _.push(A("rect", {
			x: r.left,
			y: Math.min(p, m),
			width: r.width,
			height: Math.abs(p - m),
			fill: n(l.fill, k(l.opacity, 20)),
			"data-cy": "xy-annotation-y-area",
			"pointer-events": "none"
		})), s.text) {
			let e = s.padding ?? {}, t = h && g ? Math.min(p, m) : h ? p : m, n = (s.position === "start" ? r.left + k(e.left, 0) : r.right - k(e.right, 0)) + k(s.offsetX, 0), i = t - k(s.fontSize, 12) / 3 + k(s.offsetY, 0) - k(e.top, 0), a = Pt(s, n, i);
			Number.isFinite(a.y) && Number.isFinite(i) && (_.push(A("rect", {
				class: "vue-ui-xy-annotation-label-box",
				...a,
				"data-cy": "xy-annotation-label-box"
			})), _.push(R(s.text, {
				class: "vue-ui-xy-annotation-label",
				x: n,
				y: i,
				"font-size": s.fontSize,
				fill: s.color,
				"text-anchor": s.textAnchor,
				"data-cy": "xy-annotation-label"
			})));
		}
		a.push(L("g", {
			"data-layer": "annotation",
			"data-key": `annotation_y_${i}`
		}, _.join("")));
	}), a.length ? L("g", { "data-layer": "annotations" }, a.join("")) : "";
}
function It(e, t) {
	let n = e.additionalSvgContent;
	return typeof n == "function" ? n({
		width: e.width,
		height: e.height,
		drawingArea: e.drawingArea,
		scale: e.scale,
		config: e.config,
		series: t.map((e) => ({
			...e,
			plots: M(e.plots).map((e) => ({ ...e }))
		}))
	}) ?? "" : n ?? "";
}
async function Lt(e) {
	let t = e.config, n = k(e.width, t.chart.width ?? 1e3), r = k(e.height, t.chart.height ?? 600), i = _e({
		...e,
		config: t,
		width: n,
		height: r
	}), a = je({
		...e,
		config: t,
		width: n,
		height: r,
		scale: fe(t, e.scale, i)
	}, i), o = a.series;
	return `
<svg
    xmlns="${w}"
    width="100%"
    viewBox="0 0 ${n} ${r}"
    role="img"
>
    ${P()}
    ${vt(a)}
    ${it(a, o)}
    ${Te(a, o)}
    ${Ee(a)}
    ${_t(a)}
    ${jt(a, o)}
    ${Ve(a)}
    ${await et(a, o)}
    ${Me(a)}
    ${$e(a)}
    ${await Qe(a, o)}
    ${Le(a, o)}
    ${ht(a, o)}
    ${at(a, o)}
    ${ct(a, o)}
    ${ut(a, o)}
    ${mt(a, o)}
    ${wt(a, o)}
    ${nt(a, o)}
    ${Ft(a)}
    ${It(a, o)}
</svg>`.trim();
}
//#endregion
//#region src/svg/vue-ui-xy/index.js
async function Rt(e = {}) {
	return await Lt(ce(e));
}
//#endregion
export { Rt as t };
