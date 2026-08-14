import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Bt as t, q as n, t as ee } from "./lib-Bttd6u5E.js";
import { t as r } from "./useConfig-DlNpz6P8.js";
import { t as i } from "./useNestedProp-vPNvh7rV.js";
import { t as a } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { n as te } from "./PackageVersion-CtRPcMPr.js";
import { Teleport as o, computed as s, createBlock as c, createCommentVNode as l, createElementBlock as u, createElementVNode as d, createVNode as f, nextTick as ne, normalizeClass as p, normalizeStyle as m, onBeforeUnmount as h, onMounted as g, openBlock as _, ref as v, toDisplayString as y, unref as b } from "vue";
//#region src/components/vue-ui-cursor.vue
var x = /* @__PURE__ */ e({ default: () => R }), re = [
	"xmlns",
	"height",
	"width"
], ie = {
	id: "follower",
	fy: "30%",
	fx: "30%"
}, ae = ["stop-color", "stop-opacity"], oe = [
	"r",
	"fill",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], se = ["r"], ce = {
	key: 2,
	class: "wave"
}, S = ["id"], C = ["id"], w = ["filter", "stroke"], T = {
	key: 3,
	class: "crosshair"
}, E = [
	"x1",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], D = [
	"x1",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], O = [
	"y1",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], k = [
	"y1",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], A = [
	"cy",
	"r",
	"fill"
], j = [
	"cy",
	"r",
	"fill"
], M = [
	"cx",
	"r",
	"fill"
], N = [
	"cx",
	"r",
	"fill"
], P = {
	key: 5,
	class: "coordinates"
}, F = [
	"x",
	"y",
	"font-size",
	"fill"
], I = ["transform"], L = ["font-size", "fill"], R = /*#__PURE__*/ a({
	__name: "vue-ui-cursor",
	props: { config: {
		type: Object,
		default: {}
	} },
	setup(e) {
		let { vue_ui_cursor: a } = r(), x = e, R = v(n()), z = s(() => i({
			userConfig: x.config,
			defaultConfig: a
		})), B = v({
			x: -100,
			y: -100
		}), V = s(() => Math.round(z.value.centerCircleRadius)), H = s(() => Math.round(z.value.coordinatesOffset));
		function U({ clientX: e, clientY: t, ...n }) {
			B.value.x = e - V.value, B.value.y = t - V.value;
		}
		function W(e) {
			B.value.x = e.targetTouches[0].clientX - V.value, B.value.y = e.targetTouches[0].clientY - V.value;
		}
		let G = v(!0);
		function K(e) {
			G.value = e;
		}
		let q = v(!1), J = v(null);
		function Y() {
			q.value = !1, J.value && clearTimeout(J.value), ne(() => {
				q.value = !0, J.value = setTimeout(() => {
					q.value = !1;
				}, 1e3);
			});
		}
		let X = v(null), Z = v(null);
		function Q() {
			return z.value.parentId ? document.getElementById(z.value.parentId) : document.getElementsByTagName("div")[0] || null;
		}
		function $(e) {
			e && (e.addEventListener("mousemove", U), e.addEventListener("touchmove", W), e.addEventListener("mouseleave", () => K(!1)), e.addEventListener("mouseenter", () => K(!0)), e.addEventListener("click", Y));
		}
		function le(e) {
			e && (e.removeEventListener("mousemove", U), e.removeEventListener("touchmove", W), e.removeEventListener("mouseleave", () => K(!1)), e.removeEventListener("mouseenter", () => K(!0)), e.removeEventListener("click", Y));
		}
		return g(() => {
			let e = Q();
			if (e) {
				X.value = e, $(e);
				return;
			}
			if (!z.value.parentId) return;
			let t = new MutationObserver(() => {
				let e = Q();
				e && (X.value = e, $(e), t.disconnect(), Z.value = null);
			});
			t.observe(document.body, {
				childList: !0,
				subtree: !0
			}), Z.value = t;
		}), h(() => {
			Z.value &&= (Z.value.disconnect(), null), X.value &&= (le(X.value), null);
		}), (e, n) => (_(), c(o, { to: "body" }, [G.value ? (_(), u("svg", {
			key: 0,
			"data-dom-to-png-ignore": "",
			xmlns: b(ee),
			style: m(`z-index: 2147483647; overflow: visible; pointer-events: none;background: transparent; position:fixed; top:${B.value.y}px; left:${B.value.x}px;`),
			viewBox: "0 0 100 100",
			height: V.value * 2,
			width: V.value * 2
		}, [
			f(te),
			d("defs", null, [d("radialGradient", ie, [d("stop", {
				offset: "10%",
				"stop-color": z.value.bubbleEffectColor,
				"stop-opacity": z.value.bubbleEffectOpacity
			}, null, 8, ae), n[0] ||= d("stop", {
				offset: "95%",
				"stop-color": "transparent"
			}, null, -1)])]),
			z.value.showCenterCircle ? (_(), u("circle", {
				key: 0,
				cx: 50,
				cy: 50,
				r: V.value,
				fill: b(t)(z.value.centerCircleColor, z.value.centerCircleOpacity * 100),
				stroke: z.value.centerCircleStroke,
				"stroke-width": z.value.centerCircleStrokeWidth,
				"stroke-dasharray": z.value.centerCircleDasharray
			}, null, 8, oe)) : l("", !0),
			z.value.bubbleEffect ? (_(), u("circle", {
				key: 1,
				cx: 50,
				cy: 50,
				r: V.value,
				fill: "url(#follower)",
				stroke: "none"
			}, null, 8, se)) : l("", !0),
			z.value.useWaveOnClick ? (_(), u("g", ce, [d("defs", null, [d("filter", {
				id: `blur_${R.value}`,
				x: "-50%",
				y: "-50%",
				width: "200%",
				height: "200%"
			}, [d("feGaussianBlur", {
				in: "SourceGraphic",
				stdDeviation: 4,
				id: `blur_std_${R.value}`
			}, null, 8, C), n[1] ||= d("feColorMatrix", {
				type: "saturate",
				values: "0"
			}, null, -1)], 8, S)]), q.value ? (_(), u("circle", {
				key: 0,
				class: p({ "circle-wave": q.value }),
				cx: 50,
				cy: 50,
				r: 50,
				filter: `url(#blur_${R.value})`,
				stroke: z.value.centerCircleStroke,
				fill: "none",
				"stroke-width": "3"
			}, null, 10, w)) : l("", !0)])) : l("", !0),
			z.value.showCrosshair ? (_(), u("g", T, [
				d("line", {
					x1: -V.value + 50,
					x2: -5e3,
					y1: 50,
					y2: 50,
					stroke: z.value.crosshairStroke,
					"stroke-width": z.value.crosshairStrokeWidth,
					"stroke-dasharray": z.value.crosshairDasharray,
					"stroke-linecap": "round"
				}, null, 8, E),
				d("line", {
					x1: 50 + V.value,
					x2: 5e3,
					y1: 50,
					y2: 50,
					stroke: z.value.crosshairStroke,
					"stroke-width": z.value.crosshairStrokeWidth,
					"stroke-dasharray": z.value.crosshairDasharray,
					"stroke-linecap": "round"
				}, null, 8, D),
				d("line", {
					x1: 50,
					x2: 50,
					y1: -V.value + 50,
					y2: -5e3,
					stroke: z.value.crosshairStroke,
					"stroke-width": z.value.crosshairStrokeWidth,
					"stroke-dasharray": z.value.crosshairDasharray,
					"stroke-linecap": "round"
				}, null, 8, O),
				d("line", {
					x1: 50,
					x2: 50,
					y1: V.value + 50,
					y2: 5e3,
					stroke: z.value.crosshairStroke,
					"stroke-width": z.value.crosshairStrokeWidth,
					"stroke-dasharray": z.value.crosshairDasharray,
					"stroke-linecap": "round"
				}, null, 8, k)
			])) : l("", !0),
			z.value.showIntersectCircles ? (_(), u("g", {
				key: 4,
				class: p({ "rotating-circles": z.value.isLoading })
			}, [
				d("circle", {
					cx: 50,
					cy: V.value + 50,
					r: z.value.intersectCirclesRadius,
					fill: z.value.intersectCirclesFill
				}, null, 8, A),
				d("circle", {
					cx: 50,
					cy: -V.value + 50,
					r: z.value.intersectCirclesRadius,
					fill: z.value.intersectCirclesFill
				}, null, 8, j),
				d("circle", {
					cx: -V.value + 50,
					cy: 50,
					r: z.value.intersectCirclesRadius,
					fill: z.value.intersectCirclesFill
				}, null, 8, M),
				d("circle", {
					cx: V.value + 50,
					cy: 50,
					r: z.value.intersectCirclesRadius,
					fill: z.value.intersectCirclesFill
				}, null, 8, N)
			], 2)) : l("", !0),
			z.value.showCoordinates ? (_(), u("g", P, [d("text", {
				"text-anchor": "end",
				x: -V.value + 50 - z.value.coordinatesFontSize / 2 + H.value,
				y: 50 - z.value.coordinatesFontSize / 2 + H.value,
				"font-size": z.value.coordinatesFontSize,
				fill: z.value.coordinatesColor,
				style: { "font-variant-numeric": "tabular-nums" },
				"font-family": "Arial"
			}, y(B.value.x.toFixed(0)), 9, F), d("g", { transform: `translate(${50 - z.value.coordinatesFontSize / 2 + H.value}, ${-V.value + 50 - z.value.coordinatesFontSize / 2 + H.value})` }, [d("text", {
				"text-anchor": "start",
				"font-size": z.value.coordinatesFontSize,
				fill: z.value.coordinatesColor,
				style: { "font-variant-numeric": "tabular-nums" },
				transform: "rotate(-90)",
				"font-family": "Arial"
			}, y(B.value.y.toFixed(0)), 9, L)], 8, I)])) : l("", !0)
		], 12, re)) : l("", !0)]));
	}
}, [["__scopeId", "data-v-4b1f566e"]]);
//#endregion
export { x as n, R as t };
