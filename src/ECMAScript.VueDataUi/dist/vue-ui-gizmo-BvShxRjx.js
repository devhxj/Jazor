import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Jt as t, X as n, i as r, q as ee, tt as te } from "./lib-Bttd6u5E.js";
import { n as ne, t as i } from "./useHints-Dq_w2E8B.js";
import { t as a } from "./useConfig-DlNpz6P8.js";
import { n as o, t as s } from "./BaseScanner-DZvpgOjM.js";
import { t as re } from "./useNestedProp-vPNvh7rV.js";
import { t as c } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as ie } from "./DefGrad-DVBqDjhO.js";
import { Fragment as l, computed as u, createBlock as d, createCommentVNode as f, createElementBlock as p, createElementVNode as m, createVNode as h, defineAsyncComponent as g, normalizeStyle as _, onMounted as v, openBlock as y, ref as b, renderSlot as ae, toDisplayString as x, toRefs as oe, unref as S, useSlots as C, watch as w } from "vue";
//#region src/components/vue-ui-gizmo.vue
var T = /* @__PURE__ */ e({ default: () => B }), E = [
	"aria-label",
	"aria-valuenow",
	"aria-valuetext",
	"aria-busy"
], D = ["viewBox", "width"], O = { key: 0 }, k = ["stroke"], A = ["stroke"], j = ["width", "fill"], M = ["fill"], N = { key: 0 }, P = ["id"], F = ["stroke"], I = [
	"stroke",
	"stroke-dasharray",
	"stroke-dashoffset"
], L = ["filter"], R = [
	"stroke",
	"stroke-dasharray",
	"stroke-dashoffset"
], z = ["fill"], B = /*#__PURE__*/ c({
	__name: "vue-ui-gizmo",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: { type: Number }
	},
	setup(e) {
		let c = g(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_gizmo: T } = a(), B = e, V = b(ee()), H = C();
		v(() => {
			W();
		}), v(() => {
			H["chart-background"] && U.value && console.warn("VueUiGizmo does not support the #chart-background slot.");
		});
		let U = u(() => G.value.debug);
		function W() {
			!B.dataset && B.dataset !== 0 && te({
				componentName: "VueUiGizmo",
				type: "dataset",
				debug: U.value
			}), B.dataset < 0 && U.value && console.warn("VueUiGizmo: dataset cannot be negative."), B.dataset > 100 && U.value && console.warn(`VueUiGizmo: gauge is maxed out, as dataset exceeds 100% (props.dataset = ${B.dataset})`);
		}
		let G = b(Y());
		ne({
			config: () => G.value,
			dataset: () => B.dataset,
			component: "VueUiGizmo",
			rules: [i.noHint]
		});
		let K = u(() => t({
			defaultConfig: {
				stroke: "#6A6A6A80",
				color: "#6A6A6A",
				gradientColor: "#CACACA",
				textColor: "transparent"
			},
			userConfig: G.value.skeletonConfig ?? {}
		})), { loading: q, FINAL_DATASET: J, manualLoading: se } = o({
			...oe(B),
			FINAL_CONFIG: G,
			prepareConfig: Y,
			dsIsNumber: !0,
			skeletonDataset: B.config?.skeletonDataset ?? 50,
			skeletonConfig: t({
				defaultConfig: G.value,
				userConfig: K.value
			})
		});
		function Y() {
			return re({
				userConfig: B.config,
				defaultConfig: T
			});
		}
		w(() => B.config, (e) => {
			G.value = Y(), W();
		}, { deep: !0 });
		let X = u(() => ({
			battery: {
				width: G.value.showPercentage ? 150 : 80,
				height: 50
			},
			gauge: {
				width: 80,
				height: 80
			}
		})[G.value.type]), Z = u(() => Math.min(Math.max(0, J.value), 100)), Q = u(() => Math.max(0, J.value)), $ = u(() => {
			let e = 2 * Math.PI * 35, t = e - Z.value / 100 * e;
			return {
				dasharray: `${e} ${e}`,
				dashoffset: t
			};
		});
		return (e, t) => (y(), p("div", {
			class: "vue-data-ui-component vue-ui-gizmo-wrapper",
			role: "progressbar",
			"aria-label": G.value.a11y.translations.label,
			"aria-valuemin": "0",
			"aria-valuemax": "100",
			"aria-valuenow": S(q) ? void 0 : Z.value,
			"aria-valuetext": Z.value + "%",
			"aria-busy": S(q) ? "true" : "false"
		}, [(y(), p("svg", {
			class: "vue-ui-gizmo",
			viewBox: `0 0 ${X.value.width} ${X.value.height}`,
			width: G.value.size,
			style: _({
				background: "transparent",
				fontFamily: G.value.fontFamily
			})
		}, [
			h(S(c)),
			G.value.useGradient ? (y(), p("defs", O, [h(ie, {
				t: "linear",
				id: `gizmo_gradient_${V.value}`,
				x1: "0%",
				x2: "100%",
				y1: "0%",
				y2: "0%",
				stops: [[
					"0%",
					G.value.gradientColor,
					1
				], [
					"100%",
					G.value.color,
					1
				]]
			}, null, 8, ["id", "stops"])])) : f("", !0),
			G.value.type === "battery" ? (y(), p(l, { key: 1 }, [
				m("path", {
					d: "M 5 10 L 5 40 C 5 43 7 45 9 45 L 65 45 C 68 45 70 43 70 40 L 70 38 C 73 38 75 38 75 33 L 75 17 C 75 12 73 12 70 12 L 70 10 C 70 7 68 5 65 5 L 10 5 C 7 5 5 7 5 10",
					stroke: G.value.stroke,
					"stroke-width": 2,
					fill: "none"
				}, null, 8, k),
				m("path", {
					d: "M 70 16 L 70 34",
					stroke: G.value.stroke,
					"stroke-width": 2,
					fill: "none",
					style: { opacity: "0.5" }
				}, null, 8, A),
				m("rect", {
					x: 10,
					y: 10,
					height: 30,
					width: 55 * (Z.value / 100),
					fill: G.value.useGradient ? `url(#gizmo_gradient_${V.value})` : G.value.color,
					rx: 2
				}, null, 8, j),
				G.value.showPercentage ? (y(), p("text", {
					key: 0,
					x: 85,
					y: 32,
					"text-anchor": "start",
					"font-size": "20",
					fill: G.value.textColor
				}, x(S(r)(G.value.formatter, Q.value, S(n)({
					v: Q.value,
					s: "%"
				}))), 9, M)) : f("", !0)
			], 64)) : f("", !0),
			G.value.type === "gauge" ? (y(), p(l, { key: 2 }, [
				G.value.useGradient ? (y(), p("defs", N, [m("filter", {
					id: `blur_${V.value}`,
					x: "-50%",
					y: "-50%",
					width: "200%",
					height: "200%"
				}, [...t[0] ||= [m("feGaussianBlur", {
					in: "SourceGraphic",
					stdDeviation: 1
				}, null, -1)]], 8, P)])) : f("", !0),
				m("circle", {
					cx: 40,
					cy: 40,
					r: 35,
					stroke: G.value.stroke,
					"stroke-width": 8,
					fill: "none"
				}, null, 8, F),
				m("circle", {
					cx: 40,
					cy: 40,
					r: 35,
					stroke: G.value.color,
					"stroke-width": 8,
					"stroke-dasharray": $.value.dasharray,
					"stroke-dashoffset": $.value.dashoffset,
					"stroke-linecap": "round",
					fill: "none",
					style: {
						transform: "rotate(-90deg)",
						"transform-origin": "50% 50%"
					}
				}, null, 8, I),
				G.value.useGradient ? (y(), p("g", {
					key: 1,
					filter: `url(#blur_${V.value})`
				}, [m("circle", {
					cx: 40,
					cy: 40,
					r: 35,
					stroke: G.value.gradientColor,
					"stroke-width": 1,
					fill: "none",
					"stroke-dasharray": $.value.dasharray,
					"stroke-dashoffset": $.value.dashoffset,
					style: {
						transform: "rotate(-90deg)",
						"transform-origin": "50% 50%"
					}
				}, null, 8, R)], 8, L)) : f("", !0),
				G.value.showPercentage ? (y(), p("text", {
					key: 2,
					x: 40,
					y: 47,
					"text-anchor": "middle",
					"font-size": "20",
					fill: G.value.textColor
				}, x(S(r)(G.value.formatter, Q.value, S(n)({
					v: Q.value,
					s: "%"
				}))), 9, z)) : f("", !0)
			], 64)) : f("", !0)
		], 12, D)), ae(e.$slots, "skeleton", {}, () => [S(q) ? (y(), d(s, { key: 0 })) : f("", !0)], !0)], 8, E));
	}
}, [["__scopeId", "data-v-f296c0f3"]]);
//#endregion
export { T as n, B as t };
