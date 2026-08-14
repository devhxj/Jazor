import { r as e } from "./lib-Bttd6u5E.js";
import { t } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as n } from "./BaseIcon-BfndwIWE.js";
import { t as r } from "./vClickOutside-DUrZWttG.js";
import { Fragment as i, Teleport as a, computed as o, createBlock as s, createCommentVNode as c, createElementBlock as l, createElementVNode as u, createVNode as d, guardReactiveProps as f, nextTick as p, normalizeProps as m, normalizeStyle as h, onBeforeUnmount as g, onMounted as _, openBlock as v, ref as y, renderList as b, renderSlot as x, unref as S, useCssVars as C, watch as w, watchEffect as T, withDirectives as E, withKeys as D, withModifiers as O } from "vue";
//#region src/atoms/ColorPicker.vue
var k = ["aria-expanded", "aria-label"], A = [
	"aria-label",
	"aria-pressed",
	"onClick"
], ee = { style: {
	position: "absolute",
	top: "50%",
	left: "50%",
	transform: "translate(-50%, -46%)"
} }, te = ["value"], ne = ["onClick"], re = { style: {
	position: "absolute",
	top: "50%",
	left: "50%",
	transform: "translate(-50%, -46%)"
} }, ie = ["value"], j = /*#__PURE__*/ t({
	__name: "ColorPicker",
	props: {
		value: {
			type: String,
			default: "#ffffff"
		},
		size: {
			type: String,
			default: "50px"
		},
		backgroundColor: {
			type: String,
			default: "#FFFFFF"
		},
		buttonBorderColor: {
			type: String,
			default: "#FFFFFF"
		},
		teleported: {
			type: Boolean,
			default: !1
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		}
	},
	emits: ["update:value"],
	setup(t, { emit: j }) {
		C((e) => ({ v6ca7d17c: t.buttonBorderColor }));
		let M = t, N = j, P = y(null), F = y(null), I = y(null), L = y(!1), R = y(!1), z = y({
			top: 0,
			left: 0
		}), B = o(() => ({
			backgroundColor: M.value,
			width: "100%",
			height: "100%",
			cursor: M.isCursorPointer ? "pointer" : "default"
		})), V = o(() => e(M.value));
		function H(e) {
			N("update:value", e), L.value = !1;
		}
		function U(e) {
			N("update:value", e.target.value);
		}
		function W(e) {
			e?.stopPropagation?.(), R.value = !0, I.value?.click();
		}
		function G() {
			L.value = !1;
		}
		function K() {
			!R.value && L.value && G();
		}
		async function q() {
			L.value = !L.value, L.value && M.teleported && (await p(), J());
		}
		function J() {
			if (!F.value) return;
			let e = F.value.getBoundingClientRect();
			z.value = {
				top: e.top + 36,
				left: e.right - 48
			};
		}
		function Y() {
			L.value && M.teleported && J();
		}
		function X() {
			setTimeout(() => R.value = !1, 0);
		}
		T((e) => {
			let t = I.value;
			if (!t) return;
			let n = () => X(), r = () => X(), i = () => {};
			t.addEventListener("blur", n), t.addEventListener("change", r), t.addEventListener("input", i), e(() => {
				t.removeEventListener("blur", n), t.removeEventListener("change", r), t.removeEventListener("input", i);
			});
		});
		function Z() {
			X();
		}
		function Q() {
			document.visibilityState === "visible" && X();
		}
		_(() => {
			window.addEventListener("scroll", Y, { passive: !0 }), window.addEventListener("resize", Y, { passive: !0 }), window.addEventListener("focus", Z), document.addEventListener("visibilitychange", Q);
		}), g(() => {
			window.removeEventListener("scroll", Y), window.removeEventListener("resize", Y), window.removeEventListener("focus", Z), document.removeEventListener("visibilitychange", Q);
		}), w(() => M.value, (e) => {
			I.value && (I.value.value = e);
		});
		let $ = y([
			"#000000",
			"#FFFFFF",
			"#FF5733",
			"#33FF57",
			"#3357FF",
			"#FFC300",
			"#800080",
			"#FF1493",
			"#00CED1"
		]);
		return (e, o) => E((v(), l("div", {
			ref_key: "wrapperRef",
			ref: P,
			onKeydown: D(K, ["esc"]),
			style: {
				height: "100%",
				width: "100%",
				position: "relative"
			},
			"aria-expanded": L.value ? "true" : "false",
			"aria-haspopup": "dialog",
			"aria-label": `Choose color. Current color ${t.value}`
		}, [
			u("button", {
				ref_key: "buttonRef",
				ref: F,
				class: "icon",
				onClick: q,
				style: h(B.value),
				type: "button"
			}, [x(e.$slots, "annotator-action-color", m(f({ color: V.value })), () => [d(n, {
				name: "palette",
				stroke: V.value,
				size: 22
			}, null, 8, ["stroke"])], !0)], 4),
			L.value && !t.teleported ? (v(), l("div", {
				key: 0,
				class: "vue-ui-color-picker",
				role: "dialog",
				"aria-label": "Color picker",
				style: h({
					backgroundColor: t.backgroundColor,
					position: "absolute",
					left: "calc(100% + 30px)",
					top: "50%",
					transform: "translateY(-18%)"
				}),
				onMousedown: o[2] ||= O(() => {}, ["stop"]),
				onClick: o[3] ||= O(() => {}, ["stop"]),
				onTouchstart: o[4] ||= O(() => {}, ["stop"]),
				onKeydown: D(K, ["esc"])
			}, [(v(!0), l(i, null, b($.value, (e) => (v(), l("button", {
				key: e,
				"aria-label": `Select color ${e}`,
				"aria-pressed": t.value === e ? "true" : "false",
				class: "vue-ui-color-picker-option",
				type: "button",
				style: h({
					backgroundColor: e,
					outline: `1px solid ${t.buttonBorderColor}`,
					cursor: t.isCursorPointer ? "pointer" : "default"
				}),
				onClick: () => H(e)
			}, null, 12, A))), 128)), u("button", {
				class: "vue-ui-color-picker-option",
				type: "button",
				style: h({
					backgroundColor: t.value,
					outline: `1px solid ${t.buttonBorderColor}`,
					cursor: t.isCursorPointer ? "pointer" : "default"
				}),
				"aria-label": "Open native color picker",
				onClick: O(W, ["stop"]),
				onMousedown: o[0] ||= O(() => {}, ["stop"]),
				onTouchstart: o[1] ||= O(() => {}, ["stop"])
			}, [u("div", ee, [d(n, {
				name: "colorPicker",
				stroke: V.value,
				size: 22
			}, null, 8, ["stroke"])]), u("input", {
				ref_key: "colorInput",
				ref: I,
				type: "color",
				value: t.value,
				class: "hidden-input",
				onInput: U
			}, null, 40, te)], 36)], 36)) : c("", !0),
			L.value && t.teleported ? (v(), s(a, {
				key: 1,
				to: "body"
			}, [u("div", {
				tabindex: "0",
				class: "vue-ui-color-picker",
				style: h({
					backgroundColor: t.backgroundColor,
					position: "fixed",
					top: z.value.top + "px",
					left: z.value.left + "px",
					zIndex: 2147483647
				}),
				onMousedown: o[7] ||= O(() => {}, ["stop"]),
				onClick: o[8] ||= O(() => {}, ["stop"]),
				onTouchstart: o[9] ||= O(() => {}, ["stop"])
			}, [(v(!0), l(i, null, b($.value, (e) => (v(), l("button", {
				key: e,
				class: "vue-ui-color-picker-option",
				type: "button",
				style: h({
					backgroundColor: e,
					outline: `1px solid ${t.buttonBorderColor}`,
					cursor: t.isCursorPointer ? "pointer" : "default"
				}),
				onClick: () => H(e)
			}, null, 12, ne))), 128)), u("button", {
				class: "vue-ui-color-picker-option",
				type: "button",
				style: h({
					backgroundColor: t.value,
					outline: `1px solid ${t.buttonBorderColor}`,
					cursor: t.isCursorPointer ? "cursor" : "default"
				}),
				onClick: O(W, ["stop"]),
				onMousedown: o[5] ||= O(() => {}, ["stop"]),
				onTouchstart: o[6] ||= O(() => {}, ["stop"])
			}, [u("div", re, [d(n, {
				name: "colorPicker",
				stroke: V.value,
				size: 22
			}, null, 8, ["stroke"])]), u("input", {
				ref_key: "colorInput",
				ref: I,
				type: "color",
				value: t.value,
				class: "hidden-input",
				onInput: U
			}, null, 40, ie)], 36)], 36)])) : c("", !0)
		], 40, k)), [[S(r), K]]);
	}
}, [["__scopeId", "data-v-2965a72c"]]);
//#endregion
export { j as t };
