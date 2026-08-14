import { t as e } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t } from "./BaseIcon-BfndwIWE.js";
import { createElementBlock as n, createElementVNode as r, createVNode as i, normalizeClass as a, normalizeStyle as o, openBlock as s, withKeys as c } from "vue";
var l = /*#__PURE__*/ e({
	__name: "BaseLegendToggle",
	props: {
		backgroundColor: {
			type: String,
			default: "#CCCCCC"
		},
		color: {
			type: String,
			default: "#2D353C"
		},
		fontSize: {
			type: Number,
			default: 14
		},
		checked: { type: Boolean },
		isCursorPointer: { type: Boolean }
	},
	emits: ["toggle"],
	setup(e, { emit: l }) {
		let u = l;
		return (l, d) => (s(), n("div", {
			class: a({ "vue-ui-legend-toggle-wrapper": e.isCursorPointer }),
			"data-dom-to-png-ignore": ""
		}, [r("div", {
			class: "vue-ui-legend-toggle",
			role: "button",
			tabindex: "0",
			onClick: d[0] ||= (e) => u("toggle"),
			onKeydown: d[1] ||= c((e) => u("toggle"), ["enter"]),
			style: o({
				position: "relative",
				display: "flex",
				alignItems: "center",
				justifyContent: "center",
				backgroundColor: e.backgroundColor,
				padding: e.fontSize / 4 + "px"
			})
		}, [i(t, {
			name: e.checked ? "minus" : "check",
			stroke: e.color,
			size: e.fontSize * .6,
			"stroke-width": e.fontSize / 4
		}, null, 8, [
			"name",
			"stroke",
			"size",
			"stroke-width"
		])], 36)], 2));
	}
}, [["__scopeId", "data-v-f05513e8"]]);
//#endregion
export { l as t };
