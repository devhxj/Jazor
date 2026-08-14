import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { wt as t } from "./lib-Bttd6u5E.js";
import { t as n } from "./Shape-C21CMlWS.js";
import { t as r } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { Fragment as i, createBlock as a, createCommentVNode as o, createElementBlock as s, createElementVNode as c, mergeProps as l, normalizeClass as u, normalizeStyle as d, openBlock as f, renderList as p, renderSlot as m, unref as h } from "vue";
//#region src/atoms/Legend.vue
var g = /* @__PURE__ */ e({ default: () => w }), _ = ["id"], v = [
	"role",
	"tabindex",
	"onKeydown",
	"onFocus"
], y = [
	"onClick",
	"width",
	"viewBox"
], b = { key: 0 }, x = ["id"], S = ["offset", "stop-color"], C = ["fill"], w = /*#__PURE__*/ r({
	__name: "Legend",
	props: {
		legendSet: {
			type: Array,
			default() {
				return [];
			}
		},
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		id: {
			type: String,
			default: ""
		},
		clickable: {
			type: Boolean,
			default: !0
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		}
	},
	emits: ["clickMarker", "focusMarker"],
	setup(e, { emit: r }) {
		let g = e, w = r;
		function T(e, t) {
			g.clickable && w("clickMarker", {
				legend: e,
				i: t
			});
		}
		function E(e, t, n) {
			g.clickable && (e.key === "Enter" || e.key === " ") && (e.preventDefault(), T(t, n));
		}
		function D(e, t, n) {
			e.preventDefault(), w("focusMarker", {
				legend: t,
				i: n
			});
		}
		function O(e) {
			return Array.isArray(e.gradientColors) && e.gradientColors.length;
		}
		function k(e, t) {
			let n = g.id || g.config.cy || "legend", r = O(e) ? e.gradientColors.join("_") : "";
			return `${n}_${e.seriesIndex ?? t}_${t}_${r}_gradient`.replace(/[^a-zA-Z0-9_-]/g, "_").slice(0, 160);
		}
		function A(e, t) {
			return t === 1 ? "0%" : `${e / (t - 1) * 100}%`;
		}
		function j(e) {
			return O(e) ? "0 0 90 60" : e.shape && e.shape === "star" ? "-10 -10 80 80" : "0 0 60 60";
		}
		return (r, g) => (f(), s("div", {
			id: e.id,
			class: "vue-data-ui-legend",
			style: d({
				background: e.config.backgroundColor,
				color: e.config.color,
				paddingBottom: (e.config.paddingBottom ?? 0) + "px",
				paddingTop: (e.config.paddingTop ?? 12) + "px",
				fontWeight: e.config.fontWeight,
				fontSize: `var(--legend-font-size, ${e.config.fontSize ?? 14}px)`
			})
		}, [
			m(r.$slots, "legendTitle", { titleSet: e.legendSet }, void 0, !0),
			m(r.$slots, "legendToggle", {}, void 0, !0),
			(f(!0), s(i, null, p(e.legendSet, (g, _) => (f(), s("div", {
				key: `legend_${_}`,
				class: u({
					"vue-data-ui-legend-item": !0,
					active: e.clickable && e.isCursorPointer
				}),
				role: e.clickable ? "button" : void 0,
				tabindex: e.clickable ? 0 : void 0,
				onKeydown: (e) => E(e, g, _),
				onFocus: (e) => D(e, g, _)
			}, [g.shape ? (f(), s("svg", {
				key: 0,
				onClick: (e) => T(g, _),
				height: "1em",
				width: O(g) ? "1.4em" : "1em",
				viewBox: j(g),
				style: d(`overflow: visible; opacity:${g.opacity}`),
				"aria-hidden": "true"
			}, [
				O(g) ? (f(), s("defs", b, [c("linearGradient", {
					id: k(g, _),
					x1: "0%",
					y1: "0%",
					x2: "100%",
					y2: "0%"
				}, [(f(!0), s(i, null, p(g.gradientColors, (e, t) => (f(), s("stop", {
					key: `legend_gradient_${_}_${t}`,
					offset: A(t, g.gradientColors.length),
					"stop-color": e
				}, null, 8, S))), 128))], 8, x)])) : o("", !0),
				O(g) ? (f(), s("rect", {
					key: 1,
					x: "3",
					y: "12",
					width: "84",
					height: "38",
					rx: "21",
					stroke: "none",
					fill: `url(#${k(g, _)})`
				}, null, 8, C)) : (f(), a(n, {
					key: 2,
					stroke: "none",
					shape: g.shape,
					radius: 30,
					plot: {
						x: 30,
						y: g.shape === "triangle" ? 36 : 30
					},
					fill: g.color
				}, null, 8, [
					"shape",
					"plot",
					"fill"
				])),
				m(r.$slots, "legend-pattern", l({ ref_for: !0 }, {
					legend: g,
					index: h(t)(g.absoluteIndex) ? g.absoluteIndex : _
				}), void 0, !0)
			], 12, y)) : o("", !0), m(r.$slots, "item", {
				legend: g,
				index: _
			}, void 0, !0)], 42, v))), 128)),
			m(r.$slots, "after", {}, void 0, !0)
		], 12, _));
	}
}, [["__scopeId", "data-v-a5e2fb14"]]);
//#endregion
export { g as n, w as t };
