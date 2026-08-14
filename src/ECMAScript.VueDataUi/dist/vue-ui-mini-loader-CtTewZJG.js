import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { t } from "./lib-Bttd6u5E.js";
import { n, t as r } from "./useHints-Dq_w2E8B.js";
import { t as i } from "./useConfig-DlNpz6P8.js";
import { t as a } from "./useNestedProp-vPNvh7rV.js";
import { t as o } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { computed as s, createCommentVNode as c, createElementBlock as l, createElementVNode as u, normalizeStyle as d, openBlock as f, ref as p, unref as m, useCssVars as h } from "vue";
//#region src/components/vue-ui-mini-loader.vue
var g = /* @__PURE__ */ e({ default: () => E }), _ = ["xmlns", "viewBox"], v = { key: 0 }, y = ["stroke"], b = ["stroke"], x = ["stroke"], S = { key: 1 }, C = ["stroke"], w = { key: 2 }, T = ["stroke"], E = /*#__PURE__*/ o({
	__name: "vue-ui-mini-loader",
	props: { config: {
		type: Object,
		default() {
			return {};
		}
	} },
	setup(e) {
		h((e) => ({
			v5fadbcf2: j.value,
			v488061db: M.value,
			v2e2d4bba: N.value
		}));
		let { vue_ui_mini_loader: o } = i(), g = e, E = s(() => a({
			userConfig: g.config,
			defaultConfig: o
		}));
		n({
			config: () => E.value,
			dataset: () => [],
			component: "VueUiMiniLoader",
			rules: [r.noHint]
		});
		let D = p({
			onion: "-10 -10 84 84",
			line: "-10 -10 112 84",
			bar: "-10 -10 106 84"
		}), O = s(() => ({
			gutter: `stroke:${E.value.line.gutterColor};opacity:${E.value.line.gutterOpacity};`,
			gutterBlur: `filter:blur(${E.value.line.gutterBlur}px);`
		})), k = s(() => ({
			gutter: `stroke:${E.value.bar.gutterColor};opacity:${E.value.bar.gutterOpacity};`,
			gutterBlur: `filter:blur(${E.value.bar.gutterBlur}px);`
		})), A = s(() => ({
			gutter: `stroke:${E.value.onion.gutterColor};opacity:${E.value.onion.gutterOpacity};`,
			gutterBlur: `filter:blur(${E.value.onion.gutterBlur}px);`
		})), j = s(() => `blur(${E.value.onion.trackBlur}px) hue-rotate(${E.value.onion.trackHueRotate}deg)`), M = s(() => `blur(${E.value.line.trackBlur}px) hue-rotate(${E.value.line.trackHueRotate}deg)`), N = s(() => `blur(${E.value.bar.trackBlur}px) hue-rotate(${E.value.bar.trackHueRotate}deg)`);
		return (e, n) => (f(), l("svg", {
			class: "vue-data-ui-component vue-ui-mini-loader",
			xmlns: m(t),
			viewBox: D.value[E.value.type],
			style: { background: "transparent" },
			width: "100%"
		}, [
			E.value.type === "onion" ? (f(), l("g", v, [
				u("path", {
					d: "M 3 32 C 3 45 12 62 32 62 A 1 1 0 0 0 32 3",
					"stroke-width": "4",
					fill: "none",
					"stroke-linecap": "round",
					style: d(A.value.gutter + A.value.gutterBlur)
				}, null, 4),
				u("path", {
					d: "M 13 32 C 13 39 19 52 32 52 A 1 1 0 0 0 32 13",
					"stroke-width": "4",
					fill: "none",
					"stroke-linecap": "round",
					style: d(A.value.gutter + A.value.gutterBlur)
				}, null, 4),
				u("path", {
					d: "M 23 32 C 23 37 26.5 41 32 41 A 1 1 0 0 0 32 25",
					"stroke-width": "4",
					fill: "none",
					"stroke-linecap": "round",
					style: d(A.value.gutter + A.value.gutterBlur)
				}, null, 4),
				u("path", {
					d: "M 3 32 C 3 45 12 62 32 62 A 1 1 0 0 0 32 3",
					stroke: E.value.onion.trackColor,
					"stroke-width": "4",
					fill: "none",
					"stroke-linecap": "round",
					class: "onion-animated"
				}, null, 8, y),
				u("path", {
					d: "M 13 32 C 13 39 19 52 32 52 A 1 1 0 0 0 32 13",
					stroke: E.value.onion.trackColor,
					"stroke-width": "4",
					fill: "none",
					"stroke-linecap": "round",
					class: "onion-animated"
				}, null, 8, b),
				u("path", {
					d: "M 23 32 C 23 37 26.5 41 32 41 A 1 1 0 0 0 32 25",
					stroke: E.value.onion.trackColor,
					"stroke-width": "4",
					fill: "none",
					"stroke-linecap": "round",
					class: "onion-animated"
				}, null, 8, x)
			])) : c("", !0),
			E.value.type === "line" ? (f(), l("g", S, [u("path", {
				d: "M 3 62 C 6 57 6 48 11 45 C 16 44 17 53 22 52 C 27 49 25 32 30 31 C 34 29 37 47 42 47 C 46 47 45 38 49 36 C 53 34 56 45 61 45 C 66 45 65 24 69 24 C 73 22 75 35 79 34 C 84 34 83 11 91 5",
				"stroke-width": "4",
				fill: "none",
				"stroke-linecap": "round",
				style: d(O.value.gutter + O.value.gutterBlur)
			}, null, 4), u("path", {
				d: "M 3 62 C 6 57 6 48 11 45 C 16 44 17 53 22 52 C 27 49 25 32 30 31 C 34 29 37 47 42 47 C 46 47 45 38 49 36 C 53 34 56 45 61 45 C 66 45 65 24 69 24 C 73 22 75 35 79 34 C 84 34 83 11 91 5",
				stroke: E.value.line.trackColor,
				"stroke-width": "4",
				fill: "none",
				"stroke-linecap": "round",
				class: "line-animated"
			}, null, 8, C)])) : c("", !0),
			E.value.type === "bar" ? (f(), l("g", w, [u("path", {
				d: "M 3 62 L 3 44 M 12 62 L 12 49 M 21 62 L 21 37 M 30 62 L 30 29 M 39 62 L 39 43 M 48 62 L 48 16 M 57 62 L 57 24 M 66 62 L 66 35 M 75 62 L 75 20 M 84 62 L 84 5",
				"stroke-width": "4",
				fill: "none",
				"stroke-linecap": "round",
				style: d(k.value.gutter + k.value.gutterBlur)
			}, null, 4), u("path", {
				d: "M 3 62 L 3 44 M 12 62 L 12 49 M 21 62 L 21 37 M 30 62 L 30 29 M 39 62 L 39 43 M 48 62 L 48 16 M 57 62 L 57 24 M 66 62 L 66 35 M 75 62 L 75 20 M 84 62 L 84 5",
				stroke: E.value.bar.trackColor,
				"stroke-width": "4",
				fill: "none",
				"stroke-linecap": "round",
				class: "bar-animated"
			}, null, 8, T)])) : c("", !0)
		], 8, _));
	}
}, [["__scopeId", "data-v-5266752a"]]);
//#endregion
export { g as n, E as t };
