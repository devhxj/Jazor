import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { q as t, t as n } from "./lib-Bttd6u5E.js";
import { t as r } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as i } from "./BaseIcon-BfndwIWE.js";
import { Teleport as a, computed as o, createBlock as s, createCommentVNode as c, createElementBlock as l, createElementVNode as u, createVNode as d, nextTick as f, normalizeClass as p, normalizeStyle as m, onMounted as h, onUnmounted as g, openBlock as _, reactive as v, ref as y, renderSlot as b, unref as x, withModifiers as S } from "vue";
//#region src/atoms/BaseDraggableDialog.vue
var C = /* @__PURE__ */ e({ default: () => E }), w = ["xmlns"], T = { class: "draggable-dialog-actions" }, E = /*#__PURE__*/ r({
	__name: "BaseDraggableDialog",
	props: {
		backgroundColor: { type: String },
		color: { type: String },
		headerBg: { type: String },
		headerColor: { type: String },
		fullscreenParent: { type: HTMLElement },
		isFullscreen: {
			type: Boolean,
			default: !1
		},
		withPadding: {
			type: Boolean,
			default: !1
		},
		forcedWidth: {
			type: Number,
			default: 400
		},
		forcedHeight: {
			type: Number,
			default: 400
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		},
		forceAspectRatio: {
			type: Boolean,
			default: !1
		},
		withFullWidth: {
			type: Boolean,
			default: !1
		},
		noLayerUpdate: {
			type: Boolean,
			default: !1
		}
	},
	emits: ["close", "size"],
	setup(e, { expose: r, emit: C }) {
		let E = e, D = C, O = y(!1), k = y(!1), A = y(0), j = y(null), M = y(null), N = y(null), P = `vue-ui-draggable-dialog-${t()}`, F = `${P}-title`, I = `${P}-body`;
		function L() {
			E.noLayerUpdate || (A.value += 1);
		}
		let R = v({
			left: window.innerWidth / 2 - 200,
			top: window.innerHeight / 2 - 120,
			width: E.forcedWidth,
			height: E.forcedHeight,
			dragging: !1,
			resizing: !1,
			dragOffsetX: 0,
			dragOffsetY: 0,
			pointerStartX: 0,
			pointerStartY: 0,
			resizeStartW: 0,
			resizeStartH: 0
		});
		function z() {
			N.value = document.activeElement instanceof HTMLElement ? document.activeElement : null, O.value = !0, f(() => {
				k.value ||= (R.left = Math.max(0, window.innerWidth / 2 - R.width / 2), R.top = Math.max(0, window.innerHeight / 2 - R.height / 2), !0);
				let e = M.value || j.value;
				e && typeof e.focus == "function" && e.focus();
			});
		}
		function B() {
			O.value = !1, D("close"), N.value && typeof N.value.focus == "function" && N.value.focus();
		}
		r({
			open: z,
			close: B
		});
		let V = o(() => ({
			position: "fixed",
			left: `${R.left}px`,
			top: `${R.top}px`,
			width: `${R.width}px`,
			height: `${R.height}px`,
			padding: 0,
			border: "none",
			background: E.backgroundColor,
			boxShadow: "0 4px 24px rgba(0,0,0,0.15)",
			zIndex: 9999,
			overflow: "visible",
			borderRadius: "2px",
			"--dlg-color": E.color
		}));
		function H(e) {
			return e.touches && e.touches.length ? {
				x: e.touches[0].clientX,
				y: e.touches[0].clientY
			} : {
				x: e.clientX,
				y: e.clientY
			};
		}
		function U(e) {
			e.preventDefault?.(), L(), R.dragging = !0;
			let t = H(e);
			R.dragOffsetX = t.x - R.left, R.dragOffsetY = t.y - R.top, document.addEventListener("mousemove", W), document.addEventListener("mouseup", G), document.addEventListener("touchmove", W, { passive: !1 }), document.addEventListener("touchend", G);
		}
		function W(e) {
			if (!R.dragging) return;
			e.preventDefault?.();
			let t = H(e), n = t.x - R.dragOffsetX, r = t.y - R.dragOffsetY;
			n = Math.max(0, Math.min(n, window.innerWidth - R.width)), r = Math.max(0, Math.min(r, window.innerHeight - R.height)), R.left = n, R.top = r;
		}
		function G() {
			R.dragging = !1, document.removeEventListener("mousemove", W), document.removeEventListener("mouseup", G), document.removeEventListener("touchmove", W), document.removeEventListener("touchend", G);
		}
		function K(e) {
			e.preventDefault?.(), L(), R.resizing = !0;
			let t = H(e);
			R.pointerStartX = t.x, R.pointerStartY = t.y, R.resizeStartW = R.width, R.resizeStartH = R.height, document.addEventListener("mousemove", q), document.addEventListener("mouseup", J), document.addEventListener("touchmove", q, { passive: !1 }), document.addEventListener("touchend", J);
		}
		function q(e) {
			if (!R.resizing) return;
			e.preventDefault?.();
			let t = H(e), n = t.x - R.pointerStartX, r = t.y - R.pointerStartY;
			R.width = Math.max(240, R.resizeStartW + n), R.height = Math.max(400, R.resizeStartH + r), D("size", {
				width: R.width,
				height: R.height
			});
		}
		function J() {
			R.resizing = !1, document.removeEventListener("mousemove", q), document.removeEventListener("mouseup", J), document.removeEventListener("touchmove", q), document.removeEventListener("touchend", J), D("size", {
				width: R.width,
				height: R.height
			});
		}
		h(() => {
			D("size", {
				width: R.width,
				height: R.height
			});
		});
		function Y(e) {
			e.preventDefault?.(), L(), R.resizing = !0;
			let t = H(e);
			R.pointerStartX = t.x, R.pointerStartY = t.y, R.resizeStartW = R.width, R.resizeStartH = R.height, R.resizeStartLeft = R.left, R.resizeStartTop = R.top, document.addEventListener("mousemove", X), document.addEventListener("mouseup", Z), document.addEventListener("touchmove", X, { passive: !1 }), document.addEventListener("touchend", Z);
		}
		function X(e) {
			if (!R.resizing) return;
			e.preventDefault?.();
			let t = H(e), n = t.x - R.pointerStartX, r = Math.min(Math.max(0, R.resizeStartLeft + n), R.resizeStartLeft + R.resizeStartW - 240), i = R.resizeStartW - (r - R.resizeStartLeft), a = t.y - R.pointerStartY, o = Math.max(400, R.resizeStartH + a);
			R.left = r, R.width = i, R.height = o;
		}
		function Z() {
			R.resizing = !1, document.removeEventListener("mousemove", X), document.removeEventListener("mouseup", Z), document.removeEventListener("touchmove", X), document.removeEventListener("touchend", Z);
		}
		h(() => {
			document.addEventListener("keydown", Q);
		});
		function Q(e) {
			e.key && e.key === "Escape" && B();
		}
		g(() => {
			G(), J(), Z(), document.removeEventListener("keydown", Q);
		});
		function $(e) {
			e.key === "Tab" && ee(e);
		}
		function ee(e) {
			if (!j.value) return;
			let t = j.value.querySelectorAll("a[href], area[href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), iframe, object, embed, [tabindex]:not([tabindex=\"-1\"]), [contenteditable=\"true\"]");
			if (!t.length) return;
			let n = t[0], r = t[t.length - 1];
			e.shiftKey ? document.activeElement === n && (e.preventDefault(), r.focus()) : document.activeElement === r && (e.preventDefault(), n.focus());
		}
		return (t, r) => (_(), s(a, {
			to: e.isFullscreen ? e.fullscreenParent : "body",
			key: A.value
		}, [O.value ? (_(), l("div", {
			key: 0,
			ref_key: "draggableDialog",
			ref: j,
			class: "vue-ui-draggable-dialog",
			style: m(V.value),
			role: "dialog",
			"aria-modal": !0,
			"aria-labelledby": F,
			"aria-describedby": I,
			tabindex: "-1",
			onClick: r[0] ||= S(() => {}, ["stop"]),
			onKeydown: $
		}, [
			u("div", {
				class: "vue-ui-draggable-dialog-header",
				style: m({
					backgroundColor: e.headerBg,
					color: e.headerColor
				})
			}, [
				u("span", {
					class: "drag-handle",
					"aria-hidden": "true",
					onMousedown: S(U, ["stop", "prevent"]),
					onTouchstart: S(U, ["stop", "prevent"])
				}, [(_(), l("svg", {
					xmlns: x(n),
					width: "20",
					height: "20",
					viewBox: "0 0 24 24",
					fill: "none",
					stroke: "currentColor",
					"stroke-width": "1",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [...r[1] ||= [
					u("path", { d: "M5 9m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" }, null, -1),
					u("path", { d: "M5 15m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" }, null, -1),
					u("path", { d: "M12 9m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" }, null, -1),
					u("path", { d: "M12 15m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" }, null, -1),
					u("path", { d: "M19 9m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" }, null, -1),
					u("path", { d: "M19 15m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" }, null, -1)
				]], 8, w))], 32),
				u("span", {
					class: "vue-ui-draggable-dialog-title",
					id: F
				}, [b(t.$slots, "title", {}, void 0, !0)]),
				u("div", T, [b(t.$slots, "actions", {}, void 0, !0), u("button", {
					ref_key: "closeButtonElement",
					ref: M,
					class: "close",
					type: "button",
					"aria-label": "Close dialog",
					onClick: B,
					style: m({ cursor: e.isCursorPointer ? "pointer" : "default" })
				}, [d(i, {
					name: "close",
					stroke: e.headerColor
				}, null, 8, ["stroke"])], 4)])
			], 4),
			u("div", {
				id: I,
				role: "document",
				class: p({
					"vue-ui-draggable-dialog-body": !e.withPadding,
					"vue-ui-draggable-dialog-body-pad": e.withPadding,
					"vue-ui-draggable-dialog-body-full-width": e.withFullWidth
				})
			}, [
				b(t.$slots, "before", {}, void 0, !0),
				b(t.$slots, "content", {}, void 0, !0),
				b(t.$slots, "after", {}, void 0, !0)
			], 2),
			u("div", {
				class: "resize-handle",
				"aria-hidden": "true",
				onMousedown: S(K, ["stop", "prevent"]),
				onTouchstart: S(K, ["stop", "prevent"])
			}, null, 32),
			u("div", {
				class: "resize-handle resize-handle-left",
				"aria-hidden": "true",
				onMousedown: S(Y, ["stop", "prevent"]),
				onTouchstart: S(Y, ["stop", "prevent"])
			}, null, 32)
		], 36)) : c("", !0)], 8, ["to"]));
	}
}, [["__scopeId", "data-v-41359ac7"]]);
//#endregion
export { C as n, E as t };
