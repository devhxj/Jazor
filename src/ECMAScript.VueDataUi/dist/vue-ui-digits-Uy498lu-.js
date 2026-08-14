import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { q as t, t as n } from "./lib-Bttd6u5E.js";
import { n as r, t as i } from "./useHints-Dq_w2E8B.js";
import { t as a } from "./useConfig-DlNpz6P8.js";
import { t as o } from "./useNestedProp-vPNvh7rV.js";
import { t as s } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { n as c } from "./PackageVersion-CtRPcMPr.js";
import { Fragment as l, computed as u, createBlock as d, createCommentVNode as f, createElementBlock as p, createElementVNode as m, createVNode as h, normalizeStyle as g, openBlock as _, ref as v, renderList as y, toDisplayString as b, unref as x } from "vue";
//#region src/atoms/Digit.vue
var S = { key: 0 }, C = ["d", "fill"], w = ["d", "fill"], T = ["d", "fill"], E = ["d", "fill"], D = ["d", "fill"], O = ["d", "fill"], k = ["d", "fill"], A = { key: 1 }, j = [
	"cx",
	"cy",
	"r",
	"fill"
], M = {
	__name: "Digit",
	props: {
		quanta: {
			type: String,
			default: null
		},
		backgroundColor: {
			type: String,
			default: "#e1e5e8"
		},
		color: {
			type: String,
			default: "#000000"
		},
		x: {
			type: Number,
			default: 0
		},
		y: {
			type: Number,
			default: 0
		},
		thickness: {
			type: Number,
			default: 1
		}
	},
	setup(e) {
		let t = e, n = v({
			0: "1111110",
			1: "0110000",
			2: "1101101",
			3: "1111001",
			4: "0110011",
			5: "1011011",
			6: "1011111",
			7: "1110000",
			8: "1111111",
			9: "1111011",
			"-": "0000001",
			X: "0000000"
		}), r = u(() => 2 * (t.thickness || 1)), i = u(() => [void 0, null].includes(t.quanta) ? n.value.X : n.value[t.quanta]);
		return (t, n) => (_(), p(l, null, [[
			void 0,
			null,
			"."
		].includes(e.quanta) ? f("", !0) : (_(), p("g", S, [
			m("path", {
				d: `M ${e.x} ${e.y}
                L ${e.x + r.value} ${e.y - r.value}
                L ${e.x + 26 - r.value} ${e.y - r.value}
                L ${e.x + 26} ${e.y}
                L ${e.x + 26 - r.value} ${e.y + r.value}
                L ${e.x + r.value} ${e.y + r.value} Z`,
				fill: i.value[0] == 1 ? e.color : e.backgroundColor,
				stroke: "none"
			}, null, 8, C),
			m("path", {
				d: `M ${e.x + 28 - r.value} ${e.y + 28 - r.value}
                L ${e.x + 28 - r.value} ${e.y + 2 + r.value}
                L ${e.x + 28} ${e.y + 2}
                L ${e.x + 28 + r.value} ${e.y + 2 + r.value}
                L ${e.x + 28 + r.value} ${e.y + 28 - r.value}
                L ${e.x + 28} ${e.y + 28}
                L ${e.x + 28 - r.value} ${e.y + 28 - r.value}`,
				fill: i.value[1] == 1 ? e.color : e.backgroundColor,
				stroke: "none"
			}, null, 8, w),
			m("path", {
				d: `M ${e.x + 28 - r.value} ${e.y + 58 - r.value}
                L ${e.x + 28 - r.value} ${e.y + 32 + r.value}
                L ${e.x + 28} ${e.y + 32}
                L ${e.x + 28 + r.value} ${e.y + 32 + r.value}
                L ${e.x + 28 + r.value} ${e.y + 58 - r.value}
                L ${e.x + 28} ${e.y + 58}
                L ${e.x + 28 - r.value} ${e.y + 58 - r.value}`,
				fill: i.value[2] == 1 ? e.color : e.backgroundColor,
				stroke: "none"
			}, null, 8, T),
			m("path", {
				d: `M ${e.x + r.value} ${e.y + 60 - r.value}
                L ${e.x} ${e.y + 60}
                L ${e.x + r.value} ${e.y + 60 + r.value}
                L ${e.x + 26 - r.value} ${e.y + 60 + r.value}
                L ${e.x + 26} ${e.y + 60}
                L ${e.x + 26 - r.value} ${e.y + 60 - r.value}
                L ${e.x + r.value} ${e.y + 60 - r.value}`,
				fill: i.value[3] == 1 ? e.color : e.backgroundColor,
				stroke: "none"
			}, null, 8, E),
			m("path", {
				d: `M ${e.x - 2 + r.value} ${e.y + 58 - r.value}
                L ${e.x - 2 + r.value} ${e.y + 32 + r.value}
                L ${e.x - 2} ${e.y + 32}
                L ${e.x - 2 - r.value} ${e.y + 32 + r.value}
                L ${e.x - 2 - r.value} ${e.y + 58 - r.value}
                L ${e.x - 2} ${e.y + 58}
                L ${e.x - 2 + r.value} ${e.y + 58 - r.value}`,
				fill: i.value[4] == 1 ? e.color : e.backgroundColor,
				stroke: "none"
			}, null, 8, D),
			m("path", {
				d: `M ${e.x - 2 + r.value} ${e.y + 28 - r.value}
                L ${e.x - 2 + r.value} ${e.y + 2 + r.value}
                L ${e.x - 2} ${e.y + 2}
                L ${e.x - 2 - r.value} ${e.y + 2 + r.value}
                L ${e.x - 2 - r.value} ${e.y + 28 - r.value}
                L ${e.x - 2} ${e.y + 28}
                L ${e.x - 2 + r.value} ${e.y + 28 - r.value}`,
				fill: i.value[5] == 1 ? e.color : e.backgroundColor,
				stroke: "none"
			}, null, 8, O),
			m("path", {
				d: `M ${e.x + r.value} ${e.y + 30 - r.value}
                L ${e.x} ${e.y + 30}
                L ${e.x + r.value} ${e.y + 30 + r.value}
                L ${e.x + 26 - r.value} ${e.y + 30 + r.value}
                L ${e.x + 26} ${e.y + 30}
                L ${e.x + 26 - r.value} ${e.y + 30 - r.value}
                L ${e.x + r.value} ${e.y + 30 - r.value}`,
				fill: i.value[6] == 1 ? e.color : e.backgroundColor,
				stroke: "none"
			}, null, 8, k)
		])), e.quanta == "." ? (_(), p("g", A, [m("circle", {
			cx: e.x - 8,
			cy: e.y + 60,
			r: 2 + r.value / 2,
			fill: e.color
		}, null, 8, j)])) : f("", !0)], 64));
	}
}, N = /* @__PURE__ */ e({ default: () => I }), P = ["id"], F = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], I = /*#__PURE__*/ s({
	__name: "vue-ui-digits",
	props: {
		dataset: {
			type: Number,
			default: 0
		},
		config: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	setup(e) {
		let { vue_ui_digits: s } = a(), f = e, v = u(() => o({
			userConfig: f.config,
			defaultConfig: s
		}));
		r({
			config: () => v.value,
			dataset: () => f.dataset,
			component: "VueUiDigits",
			rules: [i.noHint]
		});
		let S = u(() => {
			let e = (f.dataset || 0).toString().split(""), t = [], n = {
				x: 10,
				y: 10
			}, r = 0;
			for (let i = 0; i < e.length; i += 1) {
				let a = e[i];
				t.push({
					x: n.x + r,
					y: n.y,
					quanta: a
				}), r += a == "." ? 2 : 44;
			}
			return t;
		}), C = u(() => Math.max(...S.value.map((e) => e.x)) + 36), w = t();
		return (t, r) => (_(), p(l, null, [m("div", {
			class: "sr-only",
			id: `digit-${x(w)}`
		}, b(e.dataset), 9, P), (_(), p("svg", {
			class: "vue-data-ui-component vue-ui-digits",
			xmlns: x(n),
			viewBox: `0 0 ${C.value} 80`,
			style: g(`background:${v.value.backgroundColor};${v.value.height ? `height:${v.value.height};` : ""}${v.value.width ? `width:${v.value.width}` : ""}`),
			"aria-describedby": `digit-${x(w)}`
		}, [h(c), (_(!0), p(l, null, y(S.value, (e) => (_(), d(M, {
			x: e.x,
			y: e.y,
			quanta: e.quanta,
			color: v.value.digits.color,
			backgroundColor: v.value.digits.skeletonColor,
			thickness: v.value.digits.thickness
		}, null, 8, [
			"x",
			"y",
			"quanta",
			"color",
			"backgroundColor",
			"thickness"
		]))), 256))], 12, F))], 64));
	}
}, [["__scopeId", "data-v-63bfbc12"]]);
//#endregion
export { N as n, I as t };
