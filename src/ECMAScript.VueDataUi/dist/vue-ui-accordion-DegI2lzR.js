import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { q as t } from "./lib-Bttd6u5E.js";
import { t as n } from "./useConfig-DlNpz6P8.js";
import { t as r } from "./useNestedProp-vPNvh7rV.js";
import { t as i } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as a } from "./BaseIcon-BfndwIWE.js";
import { computed as o, createBlock as s, createCommentVNode as c, createElementBlock as l, createElementVNode as u, guardReactiveProps as d, normalizeClass as f, normalizeProps as p, normalizeStyle as m, onMounted as h, openBlock as g, ref as _, renderSlot as v, useCssVars as y, watch as b } from "vue";
//#region src/components/vue-ui-accordion.vue
var x = /* @__PURE__ */ e({ default: () => T }), S = { class: "vue-data-ui-component" }, C = ["id"], w = {
	key: 0,
	class: "vue-ui-accordion-arrow"
}, T = /*#__PURE__*/ i({
	__name: "vue-ui-accordion",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		hideDetails: {
			type: Boolean,
			default: !1
		}
	},
	emits: ["toggle"],
	setup(e, { emit: i }) {
		y((e) => ({ fc620ee6: P.value }));
		let { vue_ui_accordion: x } = n(), T = e, E = i, D = o(() => r({
			userConfig: T.config,
			defaultConfig: x
		})), O = o(() => D.value.useCursorPointer), k = _(D.value.open), A = _(t()), j = _(null), M = _(0);
		h(() => {
			j.value.open = D.value.open;
		}), b(() => D.value.open, (e) => {
			j.value.open = e;
		});
		function N() {
			(M.value > 0 || !D.value.open) && (k.value = !k.value), M.value += 1, E("toggle");
		}
		let P = o(() => `${D.value.maxHeight}px`);
		return (t, n) => (g(), l("div", S, [u("details", {
			id: `details_${A.value}`,
			ref_key: "details",
			ref: j,
			onToggle: N
		}, [u("summary", {
			class: f({ "vue-ui-accordion-headless": e.hideDetails }),
			style: m({ cursor: O.value ? "pointer" : "default" })
		}, [u("div", {
			class: "vue-ui-accordion-head",
			style: m(`background:${D.value.head.backgroundColor};padding:${D.value.head.padding}; ${e.hideDetails ? "height: 0px !important; padding: 0 !important;" : ""}`)
		}, [e.hideDetails ? c("", !0) : (g(), l("div", w, [D.value.head.useArrowSlot ? v(t.$slots, "arrow", p(d({
			backgroundColor: D.value.head.backgroundColor,
			color: D.value.head.color,
			iconColor: D.value.head.iconColor,
			isOpen: k.value
		})), void 0, !0, 0) : (g(), s(a, {
			key: 1,
			name: D.value.head.icon,
			stroke: D.value.head.iconColor,
			size: D.value.head.iconSize
		}, null, 8, [
			"name",
			"stroke",
			"size"
		]))])), v(t.$slots, "title", p(d({
			color: D.value.head.color,
			isOpen: k.value
		})), void 0, !0)], 4)], 6)], 40, C), u("div", {
			class: "vue-ui-accordion-content",
			style: m(`background:${D.value.body.backgroundColor};color:${D.value.body.color}`)
		}, [v(t.$slots, "content", p(d({
			backgroundColor: D.value.body.backgroundColor,
			color: D.value.body.color,
			isOpen: k.value
		})), void 0, !0)], 4)]));
	}
}, [["__scopeId", "data-v-2782a9e6"]]);
//#endregion
export { x as n, T as t };
