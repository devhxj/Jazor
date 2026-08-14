import { t as e } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t } from "./BaseIcon-BfndwIWE.js";
import { Fragment as n, computed as r, createCommentVNode as i, createElementBlock as a, createElementVNode as o, createVNode as s, normalizeClass as c, normalizeStyle as l, onBeforeUnmount as u, onMounted as d, openBlock as f, ref as p, renderSlot as m, toDisplayString as h } from "vue";
//#region src/atoms/HillActions.vue
var g = ["aria-label"], _ = ["aria-label"], v = ["aria-label"], y = /*#__PURE__*/ e(/* @__PURE__ */ Object.assign({ name: "HillActions" }, {
	__name: "HillActions",
	props: {
		isEditing: {
			type: Boolean,
			default: !1
		},
		isEditable: {
			type: Boolean,
			default: !0
		},
		isFullscreen: {
			type: Boolean,
			default: !1
		},
		position: {
			type: String,
			default: "left",
			validator: (e) => ["left", "right"].includes(e)
		},
		color: {
			type: String,
			default: "#2D353C"
		},
		backgroundColor: {
			type: String,
			default: "#FFFFFF"
		},
		translations: {
			type: Object,
			default() {
				return {
					edit: "Edit",
					cancel: "Cancel",
					save: "Save"
				};
			}
		},
		showTooltips: {
			type: Boolean,
			default: !0
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		},
		zIndex: {
			type: Number,
			default: 1
		}
	},
	emits: [
		"update",
		"cancel",
		"save"
	],
	setup(e, { emit: y }) {
		let b = e, x = y, S = p(!0), C = p(null), w = r(() => ({
			zIndex: b.zIndex,
			position: b.isFullscreen ? "fixed" : "absolute",
			top: "0",
			left: b.position === "left" ? b.isFullscreen ? "12px" : "0" : "auto",
			right: b.position === "right" ? b.isFullscreen ? "12px" : "0" : "auto",
			height: "36px",
			padding: "4px",
			background: "transparent",
			display: "flex",
			alignItems: "center",
			gap: "4px",
			overflow: "visible"
		})), T = r(() => ({
			cursor: b.isCursorPointer ? "pointer" : "default",
			background: b.backgroundColor,
			color: b.color
		}));
		function E() {
			if (typeof window > "u") {
				S.value = !0;
				return;
			}
			S.value = window.innerWidth > 600;
		}
		function D(e) {
			E(), C.value = e;
		}
		function O(e) {
			C.value === e && (C.value = null);
		}
		function k(e) {
			return {
				"button-info-left": b.position === "left",
				"button-info-right": b.position === "right",
				"button-info-left-visible": b.position === "left" && C.value === e,
				"button-info-right-visible": b.position === "right" && C.value === e
			};
		}
		function A(e) {
			b.showTooltips && D(e);
		}
		function j(e) {
			O(e), x(e);
		}
		return d(() => {
			E(), window.addEventListener("resize", E, { passive: !0 });
		}), u(() => {
			window.removeEventListener("resize", E);
		}), (r, u) => e.isEditable ? (f(), a("div", {
			key: 0,
			class: "vue-ui-hill-actions",
			style: l(w.value),
			"data-dom-to-png-ignore": ""
		}, [e.isEditing ? (f(), a(n, { key: 1 }, [o("button", {
			type: "button",
			class: "vue-ui-hill-actions__button",
			style: l(T.value),
			"aria-label": e.translations.cancel,
			onMouseenter: u[5] ||= (e) => A("cancel"),
			onMouseout: u[6] ||= (e) => O("cancel"),
			onFocus: u[7] ||= (e) => A("cancel"),
			onBlur: u[8] ||= (e) => O("cancel"),
			onClick: u[9] ||= (e) => j("cancel")
		}, [m(r.$slots, "hill-cancel", {}, () => [s(t, {
			name: "circleCancel",
			stroke: e.color,
			style: { "pointer-events": "none" }
		}, null, 8, ["stroke"])], !0), e.showTooltips && S.value && e.translations.cancel ? (f(), a("div", {
			key: 0,
			dir: "auto",
			class: c(k("cancel")),
			style: l({
				background: e.backgroundColor,
				color: e.color
			})
		}, h(e.translations.cancel), 7)) : i("", !0)], 44, _), o("button", {
			type: "button",
			class: "vue-ui-hill-actions__button",
			style: l(T.value),
			"aria-label": e.translations.save,
			onMouseenter: u[10] ||= (e) => A("save"),
			onMouseout: u[11] ||= (e) => O("save"),
			onFocus: u[12] ||= (e) => A("save"),
			onBlur: u[13] ||= (e) => O("save"),
			onClick: u[14] ||= (e) => j("save")
		}, [m(r.$slots, "hill-save", {}, () => [s(t, {
			name: "save",
			stroke: e.color,
			style: { "pointer-events": "none" }
		}, null, 8, ["stroke"])], !0), e.showTooltips && S.value && e.translations.save ? (f(), a("div", {
			key: 0,
			dir: "auto",
			class: c(k("save")),
			style: l({
				background: e.backgroundColor,
				color: e.color
			})
		}, h(e.translations.save), 7)) : i("", !0)], 44, v)], 64)) : (f(), a("button", {
			key: 0,
			type: "button",
			class: "vue-ui-hill-actions__button",
			style: l(T.value),
			"aria-label": e.translations.edit,
			onMouseenter: u[0] ||= (e) => A("update"),
			onMouseout: u[1] ||= (e) => O("update"),
			onFocus: u[2] ||= (e) => A("update"),
			onBlur: u[3] ||= (e) => O("update"),
			onClick: u[4] ||= (e) => j("update")
		}, [m(r.$slots, "hill-edit", {}, () => [s(t, {
			name: "move",
			stroke: e.color,
			style: { "pointer-events": "none" }
		}, null, 8, ["stroke"])], !0), e.showTooltips && S.value && e.translations.edit ? (f(), a("div", {
			key: 0,
			dir: "auto",
			class: c(k("update")),
			style: l({
				background: e.backgroundColor,
				color: e.color
			})
		}, h(e.translations.edit), 7)) : i("", !0)], 44, g))], 4)) : i("", !0);
	}
}), [["__scopeId", "data-v-f3e67d0c"]]);
//#endregion
export { y as default };
