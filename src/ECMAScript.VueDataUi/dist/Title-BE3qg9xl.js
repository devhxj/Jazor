import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { t } from "./useNestedProp-vPNvh7rV.js";
import { Fragment as n, computed as r, createCommentVNode as i, createElementBlock as a, createElementVNode as o, normalizeStyle as s, openBlock as c, renderSlot as l, toDisplayString as u } from "vue";
//#region src/atoms/Title.vue
var d = /* @__PURE__ */ e({ default: () => f }), f = {
	__name: "Title",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		lineHeight: {
			type: [String, Boolean],
			default: !1
		}
	},
	setup(e) {
		let d = e, f = r(() => t({
			userConfig: d.config,
			defaultConfig: {
				title: {
					cy: "",
					text: "",
					color: "",
					fontSize: 20,
					bold: !0,
					textAlign: "center",
					paddingLeft: 0,
					paddingRight: 0
				},
				subtitle: {
					cy: "",
					text: "",
					color: "",
					fontSize: 14,
					bold: !1
				}
			}
		}));
		return (t, r) => (c(), a(n, null, [
			o("div", {
				class: "atom-title",
				style: s({
					width: `calc(100% - ${f.value.title.paddingLeft + f.value.title.paddingRight}px)`,
					textAlign: f.value.title.textAlign,
					color: f.value.title.color,
					fontSize: `var(--title-font-size, ${f.value.title.fontSize}px)`,
					fontWeight: f.value.title.bold ? "bold" : "",
					paddingLeft: f.value.title.paddingLeft + "px",
					paddingRight: f.value.title.paddingRight + "px",
					lineHeight: e.lineHeight ? e.lineHeight : void 0
				})
			}, u(f.value.title.text), 5),
			f.value.subtitle.text ? (c(), a("div", {
				key: 0,
				class: "atom-subtitle",
				style: s({
					width: `calc(100% - ${f.value.title.paddingLeft + f.value.title.paddingRight}px)`,
					textAlign: f.value.title.textAlign,
					color: f.value.subtitle.color,
					fontSize: `var(--subtitle-font-size, ${f.value.subtitle.fontSize}px)`,
					fontWeight: f.value.subtitle.bold ? "bold" : "",
					paddingLeft: f.value.title.paddingLeft + "px",
					paddingRight: f.value.title.paddingRight + "px",
					lineHeight: e.lineHeight ? e.lineHeight : void 0
				})
			}, u(f.value.subtitle.text), 5)) : i("", !0),
			f.value.subtitle.text ? (c(), a("div", {
				key: 1,
				style: s({
					width: `calc(100% - ${f.value.title.paddingLeft + f.value.title.paddingRight}px)`,
					textAlign: f.value.title.textAlign,
					color: f.value.subtitle.color,
					fontSize: `var(--subtitle-font-size, ${f.value.subtitle.fontSize}px)`,
					fontWeight: f.value.subtitle.bold ? "bold" : "",
					paddingLeft: f.value.title.paddingLeft + "px",
					paddingRight: f.value.title.paddingRight + "px",
					lineHeight: e.lineHeight ? e.lineHeight : void 0
				})
			}, [l(t.$slots, "default")], 4)) : i("", !0)
		], 64));
	}
};
//#endregion
export { f as n, d as t };
