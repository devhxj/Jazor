import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { t } from "./img-Bnokohej.js";
import { t as n } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as r } from "./BaseIcon-BfndwIWE.js";
import { t as i } from "./vClickOutside-DUrZWttG.js";
import { Fragment as a, computed as o, createBlock as s, createCommentVNode as c, createElementBlock as l, createElementVNode as u, createVNode as d, guardReactiveProps as f, nextTick as p, normalizeClass as m, normalizeProps as h, normalizeStyle as g, onBeforeUnmount as _, onMounted as ee, openBlock as v, ref as y, renderSlot as b, toDisplayString as x, unref as S, watch as te, withDirectives as ne, withKeys as C, withModifiers as w } from "vue";
//#region src/directives/vPopoverClickOutside.js
var re = {
	beforeMount(e, t) {
		let n = () => typeof t.value == "function" ? {
			handler: t.value,
			targets: [e],
			disabled: !1,
			scrollGuardMs: 250,
			moveThreshold: 10
		} : {
			handler: t.value?.handler,
			targets: t.value?.targets ?? [e],
			disabled: !!t.value?.disabled,
			scrollGuardMs: t.value?.scrollGuardMs ?? 250,
			moveThreshold: t.value?.moveThreshold ?? 10
		}, r = {
			lastScrollTs: 0,
			pointerId: null,
			startX: 0,
			startY: 0,
			moved: !1
		}, i = () => {
			r.lastScrollTs = performance.now();
		}, a = (e) => {
			r.pointerId = e.pointerId ?? null, r.startX = e.clientX ?? 0, r.startY = e.clientY ?? 0, r.moved = !1;
		}, o = (e) => {
			if (r.pointerId === null || (e.pointerId ?? null) !== r.pointerId) return;
			let t = (e.clientX ?? 0) - r.startX, i = (e.clientY ?? 0) - r.startY, { moveThreshold: a } = n();
			t * t + i * i >= a * a && (r.moved = !0);
		}, s = (e) => {
			r.pointerId !== null && (e.pointerId ?? null) === r.pointerId && (r.pointerId = null);
		}, c = (e, t) => {
			let n = e.composedPath ? e.composedPath() : [];
			for (let r of t) {
				let t = r && typeof r == "object" && "value" in r ? r.value : r;
				if (t && (t === e.target || t.contains && t.contains(e.target) || n.length && n.includes(t))) return !0;
			}
			return !1;
		}, l = (e) => {
			let { handler: t, targets: i, disabled: a, scrollGuardMs: o } = n();
			a || typeof t == "function" && (performance.now() - r.lastScrollTs < o || r.moved || c(e, i) || t(e));
		};
		e.__vPopoverClickOutside__ = {
			onScrollCapture: i,
			onPointerDownCapture: a,
			onPointerMoveCapture: o,
			onPointerUpCapture: s,
			onClickCapture: l
		}, window.addEventListener("scroll", i, !0), document.addEventListener("pointerdown", a, !0), document.addEventListener("pointermove", o, !0), document.addEventListener("pointerup", s, !0), document.addEventListener("click", l, !0);
	},
	unmounted(e) {
		let t = e.__vPopoverClickOutside__;
		t && (window.removeEventListener("scroll", t.onScrollCapture, !0), document.removeEventListener("pointerdown", t.onPointerDownCapture, !0), document.removeEventListener("pointermove", t.onPointerMoveCapture, !0), document.removeEventListener("pointerup", t.onPointerUpCapture, !0), document.removeEventListener("click", t.onClickCapture, !0), delete e.__vPopoverClickOutside__);
	}
}, T = /* @__PURE__ */ e({ default: () => E }), ie = ["title", "onKeydown"], ae = ["popover", "data-open"], oe = ["title"], se = ["data-open"], E = /*#__PURE__*/ n({
	__name: "UserOptions",
	props: {
		hasPdf: {
			type: Boolean,
			default: !0
		},
		hasXls: {
			type: Boolean,
			default: !0
		},
		hasImg: {
			type: Boolean,
			default: !1
		},
		hasSvg: {
			type: Boolean,
			default: !1
		},
		hasLabel: {
			type: Boolean,
			default: !1
		},
		isLabelActive: {
			type: Boolean,
			default: !1
		},
		hasTable: {
			type: Boolean,
			default: !1
		},
		hasSort: {
			type: Boolean,
			default: !1
		},
		hasStack: {
			type: Boolean,
			default: !1
		},
		hasTooltip: {
			type: Boolean,
			default: !1
		},
		color: { type: String },
		backgroundColor: { type: String },
		isPrinting: {
			type: Boolean,
			default: !1
		},
		isImaging: {
			type: Boolean,
			default: !1
		},
		title: { type: String },
		uid: { type: String },
		hasFullscreen: {
			type: Boolean,
			default: !1
		},
		chartElement: {
			type: HTMLElement,
			default: null
		},
		isFullscreen: {
			type: Boolean,
			default: !1
		},
		isStacked: {
			type: Boolean,
			default: !1
		},
		isTooltip: {
			type: Boolean,
			default: !1
		},
		hasAnimation: {
			type: Boolean,
			default: !1
		},
		isAnimation: {
			type: Boolean,
			default: !1
		},
		titles: {
			type: Object,
			default() {
				return {};
			}
		},
		showTooltips: {
			type: Boolean,
			default: !0
		},
		zIndex: {
			type: Number,
			default: 1
		},
		noOffset: {
			type: Boolean,
			default: !0
		},
		position: {
			type: String,
			default: "right"
		},
		offsetX: {
			type: Number,
			default: 0
		},
		hasAnnotator: {
			type: Boolean,
			default: !1
		},
		isAnnotation: {
			type: Boolean,
			default: !1
		},
		callbacks: {
			type: Object,
			default() {
				return {};
			}
		},
		printScale: {
			type: Number,
			default: 2
		},
		tableDialog: {
			type: Boolean,
			default: !1
		},
		hasZoom: {
			type: Boolean,
			default: !1
		},
		isZoom: {
			type: Boolean,
			default: !1
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		},
		hasAltCopy: {
			type: Boolean,
			default: !1
		}
	},
	emits: [
		"generatePdf",
		"generateCsv",
		"generateImage",
		"toggleTable",
		"toggleLabels",
		"toggleSort",
		"toggleFullscreen",
		"toggleStack",
		"toggleTooltip",
		"toggleAnimation",
		"toggleAnnotator",
		"generateSvg",
		"toggleZoom",
		"copyAlt"
	],
	setup(e, { expose: n, emit: T }) {
		let E = e, D = T, O = y(null), k = y(null), A = y(null), j = y(!1), M = o(() => typeof window < "u" && typeof HTMLElement < "u" && "popover" in HTMLElement.prototype);
		async function ce() {
			if (E.callbacks.pdf) {
				let { imageUri: e, base64: n } = await t({
					domElement: E.chartElement,
					base64: !0,
					img: !0,
					scale: E.printScale
				});
				E.callbacks.pdf({
					domElement: E.chartElement,
					base64: n,
					imageUri: e
				});
			} else D("generatePdf");
		}
		function le() {
			D("generateCsv", E.callbacks.csv);
		}
		async function ue() {
			if (E.callbacks.img) {
				D("generateImage", { stage: "start" });
				try {
					let { imageUri: e, base64: n } = await t({
						domElement: E.chartElement,
						base64: !0,
						img: !0,
						scale: E.printScale
					});
					await Promise.resolve(E.callbacks.img({
						domElement: E.chartElement,
						imageUri: e,
						base64: n
					}));
				} finally {
					D("generateImage", { stage: "end" });
				}
			} else D("generateImage");
		}
		function de() {
			D("generateSvg", { isCb: !!E.callbacks.svg });
		}
		let N = y(!1);
		function fe() {
			E.callbacks.table ? E.callbacks.table() : (N.value = !N.value, D("toggleTable"));
		}
		function pe(e) {
			N.value = e;
		}
		n({ setTableIconState: pe });
		let P = y(E.isLabelActive);
		function F() {
			E.callbacks.labels ? E.callbacks.labels() : (P.value = !P.value, D("toggleLabels"));
		}
		let I = y(E.isAnimation);
		function L() {
			E.callbacks.animation ? E.callbacks.animation() : (I.value = !I.value, D("toggleAnimation"));
		}
		let R = y(E.isZoom);
		function z() {
			E.callbacks.zoom ? E.callbacks.zoom() : (R.value = !R.value, D("toggleZoom"));
		}
		let B = o({
			get: () => E.isAnnotation,
			set: (e) => e
		});
		function V() {
			E.callbacks.annotator ? E.callbacks.annotator() : (B.value = !B.value, D("toggleAnnotator"));
		}
		function me() {
			E.callbacks.sort ? E.callbacks.sort() : D("toggleSort");
		}
		function H() {
			D("copyAlt");
		}
		let U = y(E.isStacked);
		function he() {
			E.callbacks.stack ? E.callbacks.stack() : (U.value = !U.value, D("toggleStack"));
		}
		let W = y(E.isTooltip);
		function G() {
			E.callbacks.tooltip ? E.callbacks.tooltip() : (W.value = !W.value, D("toggleTooltip"));
		}
		let ge = o({
			get: () => E.isFullscreen,
			set: (e) => D("toggleFullscreen", e)
		});
		function K() {
			if (!E.chartElement) return;
			let e = !E.isFullscreen;
			ge.value = e, e ? E.chartElement.requestFullscreen() : document.exitFullscreen();
		}
		function _e() {
			let e = !!document.fullscreenElement;
			D("toggleFullscreen", e);
		}
		ee(() => {
			document.addEventListener("fullscreenchange", _e);
		}), _(() => {
			document.removeEventListener("fullscreenchange", _e);
		});
		let q = y(window.innerWidth > 600), J = y({
			tooltip: !1,
			pdf: !1,
			csv: !1,
			img: !1,
			table: !1,
			labels: !1,
			sort: !1,
			stack: !1,
			fullscreen: !1,
			animation: !1,
			annotator: !1,
			svg: !1,
			zoom: !1,
			altCopy: !1
		}), ve = y(!0);
		function ye() {
			q.value = window.innerWidth > 600, j.value = !j.value, j.value && (ve.value = !1);
		}
		function be() {
			E.isPrinting || E.isImaging || (j.value = !1);
		}
		function xe() {
			j.value && be();
		}
		let Y = y(0);
		function Se() {
			Y.value = performance.now() + 50;
		}
		function X() {
			E.isPrinting || E.isImaging || (M.value && Q(), j.value = !1);
		}
		function Ce() {
			performance.now() < Y.value || X();
		}
		function we() {
			if (q.value = window.innerWidth > 600, !M.value) {
				j.value || Se(), j.value = !j.value;
				return;
			}
			let e = A.value;
			if (e && e.matches && e.matches(":popover-open") || j.value) {
				j.value = !1, Q();
				return;
			}
			Se(), j.value = !0;
		}
		function Te() {
			we();
		}
		function Ee(e, t, n, r) {
			let i = window.devicePixelRatio || 1, a = (e) => Math.round(e * i) / i;
			e.style.position = "fixed", e.style.top = `${a(t)}px`, e.style.left = `${a(n)}px`, e.style.right = "auto", e.style.width = `${a(r)}px`;
		}
		function De(e) {
			if (e.style.width = "", e.style.left = "", e.style.right = "", e.style.top = "", e.style.position = "absolute", e.style.top = "36px", E.position === "right") {
				let t = E.offsetX ? E.offsetX : E.noOffset ? 0 : 4;
				e.style.right = `${t}px`, e.style.left = "auto";
			} else {
				let t = E.noOffset ? 0 : 4;
				e.style.left = `${t}px`, e.style.right = "auto";
			}
			e.style.display = "flex", e.style.visibility = "hidden", e.style.pointerEvents = "none";
		}
		function Oe() {
			if (!M.value || !j.value || !O.value || !A.value) return;
			let e = O.value.getBoundingClientRect(), t = A.value.getBoundingClientRect(), n = e.top + 38;
			if (E.position === "right") {
				let r = E.offsetX ? E.offsetX : E.noOffset ? 0 : 4, i = e.right - r - t.width;
				Ee(A.value, n, i, t.width);
			} else {
				let r = E.noOffset ? 0 : 4, i = e.left + r;
				Ee(A.value, n, i, t.width);
			}
		}
		let ke = 0;
		function Ae() {
			cancelAnimationFrame(ke), ke = requestAnimationFrame(() => {
				Oe();
			});
		}
		function Z() {
			M.value && j.value && Ae();
		}
		function je(e, t) {
			let n = window.devicePixelRatio || 1, r = (e) => Math.round(e * n) / n;
			e.style.position = "fixed", e.style.top = `${r(t.top)}px`, e.style.left = `${r(t.left)}px`, e.style.right = "auto", e.style.width = `${r(t.width)}px`;
		}
		async function Me(e = !1) {
			if (!M.value) return;
			let t = A.value;
			if (!t) return;
			let n = t.getAttribute("style") || "", r = t.getAttribute("data-open");
			t.setAttribute("popover", "manual");
			try {
				t.setAttribute("data-open", "true"), De(t), await p();
				let i = t.getBoundingClientRect();
				t.setAttribute("style", n), r === null ? t.removeAttribute("data-open") : t.setAttribute("data-open", r), je(t, i), e || (typeof t.showPopover == "function" ? t.showPopover() : t.style.display = "flex");
			} catch {
				if (!e) try {
					typeof t.showPopover == "function" ? t.showPopover() : t.style.display = "flex";
				} catch {}
			}
		}
		function Q() {
			if (!M.value) return;
			let e = A.value;
			if (e) {
				e.setAttribute("popover", "manual");
				try {
					typeof e.hidePopover == "function" ? e.hidePopover() : e.style.display = "none";
				} catch {
					e.style.display = "none";
				}
				e.style.position = "", e.style.top = "", e.style.left = "", e.style.right = "", e.style.width = "", e.style.inset = "";
			}
		}
		function Ne() {
			if (!M.value) return;
			let e = A.value;
			e && (j.value &&= typeof e.matches == "function" && (() => {
				try {
					return e.matches(":popover-open");
				} catch {
					return !1;
				}
			})());
		}
		function Pe(e) {
			if (!M.value || !j.value || performance.now() < Y.value) return;
			let t = e.composedPath ? e.composedPath() : [], n = k.value ? t.includes(k.value) : !1, r = A.value ? t.includes(A.value) : !1;
			!n && !r && X();
		}
		te(() => E.isFullscreen, async () => {
			M.value && (await p(), j.value && Me());
		}), te(() => j.value, async (e) => {
			M.value && (await p(), e ? (await Me(!1), Oe()) : Q());
		});
		function $(e) {
			e.key === "Escape" && O.value && O.value.contains(document.activeElement) && (e.preventDefault(), e.stopPropagation(), X(), k.value?.focus());
		}
		return ee(() => {
			window.addEventListener("pointerdown", Pe, !0), window.addEventListener("resize", Z, { passive: !0 }), window.addEventListener("scroll", Z, {
				passive: !0,
				capture: !0
			}), A.value && A.value.addEventListener("toggle", Ne);
		}), _(() => {
			window.removeEventListener("pointerdown", Pe, !0), window.removeEventListener("resize", Z), window.removeEventListener("scroll", Z, !0), A.value && A.value.removeEventListener("toggle", Ne);
		}), (t, n) => M.value ? ne((v(), l("div", {
			key: 0,
			ref_key: "rootRef",
			ref: O,
			"data-dom-to-png-ignore": "",
			class: "vue-ui-user-options",
			style: g(`z-index: ${e.zIndex}; height: 34px; position: ${e.isFullscreen ? "fixed" : "absolute"}; top: 0; ${e.position === "right" ? `right:${e.isFullscreen ? "12px" : "0"}` : `left:${e.isFullscreen ? "12px" : "0"}`}; padding: 4px; background:transparent;`),
			onKeydown: $
		}, [u("div", {
			ref_key: "triggerRef",
			ref: k,
			tabindex: "0",
			role: "button",
			title: j.value ? e.titles.close || "" : e.titles.open || "",
			style: g(`width:32px; position: absolute; top: 0;${e.position === "right" ? `right: ${e.offsetX ? e.offsetX : e.noOffset ? 0 : 4}px` : `left: ${e.noOffset ? 0 : 4}px`}; padding: 0 0px; display: flex; align-items:center;justify-content:center;height: 36px;  cursor:${e.isCursorPointer ? "pointer" : "default"}; background:transparent`),
			onPointerdown: w(we, ["stop", "prevent"]),
			onKeydown: C(w(Te, ["stop", "prevent"]), ["enter"])
		}, [b(t.$slots, "menuIcon", h(f({
			isOpen: j.value,
			color: e.color
		})), () => [d(r, {
			name: j.value ? "close" : "menu",
			stroke: e.color,
			"stroke-width": 2
		}, null, 8, ["name", "stroke"])], !0)], 44, ie), u("div", {
			ref_key: "drawerRef",
			ref: A,
			popover: M.value ? "manual" : null,
			"data-open": M.value ? null : j.value,
			class: m({ "vue-ui-user-options-drawer": !0 }),
			style: g(M.value ? { background: e.backgroundColor } : `background:${e.backgroundColor}; ${e.position === "right" ? `right: ${e.offsetX ? e.offsetX : e.noOffset ? 0 : 4}px` : `left: ${e.noOffset ? 0 : 4}px`}`)
		}, [
			b(t.$slots, "custom-menu-before", {}, void 0, !0),
			e.hasPdf ? (v(), l("button", {
				key: 0,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: ce,
				onMouseenter: n[0] ||= (e) => J.value.pdf = !0,
				onMouseout: n[1] ||= (e) => J.value.pdf = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionPdf ? b(t.$slots, "optionPdf", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isPrinting ? (v(), s(r, {
				key: 0,
				name: "hourglass",
				isSpin: "",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "filePdf",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.pdf ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.pdf,
					"button-info-left-visible": e.position === "left" && J.value.pdf
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.pdf), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasXls ? (v(), l("button", {
				key: 1,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: le,
				onMouseenter: n[2] ||= (e) => J.value.csv = !0,
				onMouseout: n[3] ||= (e) => J.value.csv = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionCsv ? b(t.$slots, "optionCsv", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "fileCsv",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.csv ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.csv,
					"button-info-left-visible": e.position === "left" && J.value.csv
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.csv), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasImg ? (v(), l("button", {
				key: 2,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: ue,
				onMouseenter: n[4] ||= (e) => J.value.img = !0,
				onMouseout: n[5] ||= (e) => J.value.img = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionImg ? b(t.$slots, "optionImg", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isImaging ? (v(), s(r, {
				key: 0,
				name: "hourglass",
				isSpin: "",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "filePng",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.img ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.img,
					"button-info-left-visible": e.position === "left" && J.value.img
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.img), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasSvg ? (v(), l("button", {
				key: 3,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: de,
				onMouseenter: n[6] ||= (e) => J.value.svg = !0,
				onMouseout: n[7] ||= (e) => J.value.svg = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionSvg ? b(t.$slots, "optionSvg", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "fileSvg",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.svg ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.svg,
					"button-info-left-visible": e.position === "left" && J.value.svg
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.svg), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasTooltip ? (v(), l("button", {
				key: 4,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: G,
				onMouseenter: n[8] ||= (e) => J.value.tooltip = !0,
				onMouseout: n[9] ||= (e) => J.value.tooltip = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionTooltip ? b(t.$slots, "optionTooltip", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [W.value ? (v(), s(r, {
				key: 0,
				name: "tooltip",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "tooltipDisabled",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.tooltip ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-left-visible": e.position === "left" && J.value.tooltip,
					"button-info-right-visible": e.position === "right" && J.value.tooltip
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.tooltip), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasTable ? (v(), l("button", {
				key: 5,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: fe,
				onMouseenter: n[10] ||= (e) => J.value.table = !0,
				onMouseout: n[11] ||= (e) => J.value.table = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionTable ? b(t.$slots, "optionTable", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.tableDialog ? (v(), s(r, {
				key: 0,
				name: N.value ? "tableDialogClose" : "tableDialogOpen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["name", "stroke"])) : (v(), s(r, {
				key: 1,
				name: N.value ? "tableClose" : "tableOpen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["name", "stroke"]))], 64)), q.value && e.titles.table ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.table,
					"button-info-left-visible": e.position === "left" && J.value.table
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.table), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasLabel ? (v(), l("button", {
				key: 6,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: F,
				onMouseenter: n[12] ||= (e) => J.value.labels = !0,
				onMouseout: n[13] ||= (e) => J.value.labels = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionLabels ? b(t.$slots, "optionLabels", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: P.value ? "labelClose" : "labelOpen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["name", "stroke"])), q.value && e.titles.labels ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.labels,
					"button-info-left-visible": e.position === "left" && J.value.labels
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.labels), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasSort ? (v(), l("button", {
				key: 7,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: me,
				onMouseenter: n[14] ||= (e) => J.value.sort = !0,
				onMouseout: n[15] ||= (e) => J.value.sort = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionSort ? b(t.$slots, "optionSort", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "sort",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.sort ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.sort,
					"button-info-left-visible": e.position === "left" && J.value.sort
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.sort), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasStack ? (v(), l("button", {
				key: 8,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: he,
				onMouseenter: n[16] ||= (e) => J.value.stack = !0,
				onMouseout: n[17] ||= (e) => J.value.stack = !1,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionStack ? b(t.$slots, "optionStack", h(f({ isStack: U.value })), void 0, !0, 0) : (v(), l(a, { key: 1 }, [U.value ? (v(), s(r, {
				key: 0,
				name: "unstack",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "stack",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.stack ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.stack,
					"button-info-left-visible": e.position === "left" && J.value.stack
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.stack), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasFullscreen ? (v(), l("button", {
				key: 9,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[18] ||= (e) => J.value.fullscreen = !0,
				onMouseout: n[19] ||= (e) => J.value.fullscreen = !1,
				onClick: n[20] ||= (t) => K(e.isFullscreen ? "out" : "in"),
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionFullscreen ? b(t.$slots, "optionFullscreen", h(f({
				toggleFullscreen: K,
				isFullscreen: e.isFullscreen
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isFullscreen ? (v(), s(r, {
				key: 0,
				name: "exitFullscreen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "fullscreen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.fullscreen ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.fullscreen,
					"button-info-left-visible": e.position === "left" && J.value.fullscreen
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.fullscreen), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasZoom ? (v(), l("button", {
				key: 10,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[21] ||= (e) => J.value.zoom = !0,
				onMouseout: n[22] ||= (e) => J.value.zoom = !1,
				onClick: n[23] ||= (e) => z(),
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionZoom ? b(t.$slots, "optionZoom", h(f({
				toggleZoom: z,
				isZoomLocked: !e.isZoom
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isZoom ? (v(), s(r, {
				key: 0,
				name: "zoomUnlock",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "zoomLock",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.zoom ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.zoom,
					"button-info-left-visible": e.position === "left" && J.value.zoom
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.zoom), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasAnimation ? (v(), l("button", {
				key: 11,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[24] ||= (e) => J.value.animation = !0,
				onMouseout: n[25] ||= (e) => J.value.animation = !1,
				onClick: L,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionAnimation ? b(t.$slots, "optionAnimation", h(f({
				toggleAnimation: L,
				isAnimated: I.value
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [I.value ? (v(), s(r, {
				key: 0,
				name: "play",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "pause",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.fullscreen ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.animation,
					"button-info-left-visible": e.position === "left" && J.value.animation
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.animation), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasAnnotator ? (v(), l("button", {
				key: 12,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[26] ||= (e) => J.value.annotator = !0,
				onMouseout: n[27] ||= (e) => J.value.annotator = !1,
				onClick: V,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionAnnotator ? b(t.$slots, "optionAnnotator", h(f({
				toggleAnnotator: V,
				isAnnotator: B.value
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [B.value ? (v(), s(r, {
				key: 0,
				name: "annotatorDisabled",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "annotator",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.annotator ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.annotator,
					"button-info-left-visible": e.position === "left" && J.value.annotator
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.annotator), 7)) : c("", !0)], 36)) : c("", !0),
			e.hasAltCopy ? (v(), l("button", {
				key: 13,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[28] ||= (e) => J.value.altCopy = !0,
				onMouseout: n[29] ||= (e) => J.value.altCopy = !1,
				onClick: H,
				style: g({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [t.$slots.optionAltCopy ? b(t.$slots, "optionAltCopy", h(f({ copyAlt: H })), void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "accessibility",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.altCopy ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.altCopy,
					"button-info-left-visible": e.position === "left" && J.value.altCopy
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.altCopy), 7)) : c("", !0)], 36)) : c("", !0),
			b(t.$slots, "custom-menu-after", {}, void 0, !0)
		], 12, ae)], 36)), [[S(re), M.value ? null : {
			targets: [
				O.value,
				A.value,
				k.value
			],
			handler: Ce
		}]]) : ne((v(), l("div", {
			key: 1,
			ref_key: "rootRef",
			ref: O,
			"data-dom-to-png-ignore": "",
			class: "vue-ui-user-options",
			style: g(`z-index: ${e.zIndex}; height: 34px; position: ${e.isFullscreen ? "fixed" : "absolute"}; top: 0; ${e.position === "right" ? `right:${e.isFullscreen ? "12px" : "0"}` : `left:${e.isFullscreen ? "12px" : "0"}`}; padding: 4px; background:transparent;`),
			onKeydown: $
		}, [u("div", {
			ref_key: "triggerRef",
			ref: k,
			tabindex: "0",
			title: j.value ? e.titles.close || "" : e.titles.open || "",
			style: g(`width:32px; position: absolute; top: 0;${e.position === "right" ? `right: ${e.offsetX ? e.offsetX : e.noOffset ? 0 : 4}px` : `left: ${e.noOffset ? 0 : 4}px`}; padding: 0 0px; display: flex; align-items:center;justify-content:center;height: 36px;  cursor:pointer; background:transparent`),
			onClick: w(ye, ["stop"]),
			onKeypress: C(ye, ["enter"])
		}, [b(t.$slots, "menuIcon", h(f({
			isOpen: j.value,
			color: e.color
		})), () => [d(r, {
			name: j.value ? "close" : "menu",
			stroke: e.color,
			"stroke-width": 2
		}, null, 8, ["name", "stroke"])], !0)], 44, oe), u("div", {
			ref_key: "drawerRef",
			ref: A,
			"data-open": j.value,
			class: m({ "vue-ui-user-options-drawer": !0 }),
			style: g(`background:${e.backgroundColor}; ${e.position === "right" ? `right: ${e.offsetX ? e.offsetX : e.noOffset ? 0 : 4}px` : `left: ${e.noOffset ? 0 : 4}px`}`)
		}, [
			b(t.$slots, "custom-menu-before", {}, void 0, !0),
			e.hasPdf ? (v(), l("button", {
				key: 0,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: ce,
				onMouseenter: n[30] ||= (e) => J.value.pdf = !0,
				onMouseout: n[31] ||= (e) => J.value.pdf = !1
			}, [t.$slots.optionPdf ? b(t.$slots, "optionPdf", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isPrinting ? (v(), s(r, {
				key: 0,
				name: "hourglass",
				isSpin: "",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "filePdf",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.pdf ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.pdf,
					"button-info-left-visible": e.position === "left" && J.value.pdf
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.pdf), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasXls ? (v(), l("button", {
				key: 1,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: le,
				onMouseenter: n[32] ||= (e) => J.value.csv = !0,
				onMouseout: n[33] ||= (e) => J.value.csv = !1
			}, [t.$slots.optionCsv ? b(t.$slots, "optionCsv", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "fileCsv",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.csv ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.csv,
					"button-info-left-visible": e.position === "left" && J.value.csv
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.csv), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasImg ? (v(), l("button", {
				key: 2,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: ue,
				onMouseenter: n[34] ||= (e) => J.value.img = !0,
				onMouseout: n[35] ||= (e) => J.value.img = !1
			}, [t.$slots.optionImg ? b(t.$slots, "optionImg", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isImaging ? (v(), s(r, {
				key: 0,
				name: "hourglass",
				isSpin: "",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "filePng",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.img ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.img,
					"button-info-left-visible": e.position === "left" && J.value.img
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.img), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasSvg ? (v(), l("button", {
				key: 3,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: de,
				onMouseenter: n[36] ||= (e) => J.value.svg = !0,
				onMouseout: n[37] ||= (e) => J.value.svg = !1
			}, [t.$slots.optionSvg ? b(t.$slots, "optionSvg", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "fileSvg",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.svg ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.svg,
					"button-info-left-visible": e.position === "left" && J.value.svg
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.svg), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasTooltip ? (v(), l("button", {
				key: 4,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: G,
				onMouseenter: n[38] ||= (e) => J.value.tooltip = !0,
				onMouseout: n[39] ||= (e) => J.value.tooltip = !1
			}, [t.$slots.optionTooltip ? b(t.$slots, "optionTooltip", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [W.value ? (v(), s(r, {
				key: 0,
				name: "tooltip",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "tooltipDisabled",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.tooltip ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-left-visible": e.position === "left" && J.value.tooltip,
					"button-info-right-visible": e.position === "right" && J.value.tooltip
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.tooltip), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasTable ? (v(), l("button", {
				key: 5,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: fe,
				onMouseenter: n[40] ||= (e) => J.value.table = !0,
				onMouseout: n[41] ||= (e) => J.value.table = !1
			}, [t.$slots.optionTable ? b(t.$slots, "optionTable", {}, void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.tableDialog ? (v(), s(r, {
				key: 0,
				name: N.value ? "tableDialogClose" : "tableDialogOpen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["name", "stroke"])) : (v(), s(r, {
				key: 1,
				name: N.value ? "tableClose" : "tableOpen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["name", "stroke"]))], 64)), q.value && e.titles.table ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.table,
					"button-info-left-visible": e.position === "left" && J.value.table
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.table), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasLabel ? (v(), l("button", {
				key: 6,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: F,
				onMouseenter: n[42] ||= (e) => J.value.labels = !0,
				onMouseout: n[43] ||= (e) => J.value.labels = !1
			}, [t.$slots.optionLabels ? b(t.$slots, "optionLabels", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: P.value ? "labelClose" : "labelOpen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["name", "stroke"])), q.value && e.titles.labels ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.labels,
					"button-info-left-visible": e.position === "left" && J.value.labels
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.labels), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasSort ? (v(), l("button", {
				key: 7,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: me,
				onMouseenter: n[44] ||= (e) => J.value.sort = !0,
				onMouseout: n[45] ||= (e) => J.value.sort = !1
			}, [t.$slots.optionSort ? b(t.$slots, "optionSort", {}, void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "sort",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.sort ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.sort,
					"button-info-left-visible": e.position === "left" && J.value.sort
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.sort), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasStack ? (v(), l("button", {
				key: 8,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onClick: he,
				onMouseenter: n[46] ||= (e) => J.value.stack = !0,
				onMouseout: n[47] ||= (e) => J.value.stack = !1
			}, [t.$slots.optionStack ? b(t.$slots, "optionStack", h(f({ isStack: U.value })), void 0, !0, 0) : (v(), l(a, { key: 1 }, [U.value ? (v(), s(r, {
				key: 0,
				name: "unstack",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "stack",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.stack ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.stack,
					"button-info-left-visible": e.position === "left" && J.value.stack
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.stack), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasFullscreen ? (v(), l("button", {
				key: 9,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[48] ||= (e) => J.value.fullscreen = !0,
				onMouseout: n[49] ||= (e) => J.value.fullscreen = !1,
				onClick: n[50] ||= (t) => K(e.isFullscreen ? "out" : "in")
			}, [t.$slots.optionFullscreen ? b(t.$slots, "optionFullscreen", h(f({
				toggleFullscreen: K,
				isFullscreen: e.isFullscreen
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isFullscreen ? (v(), s(r, {
				key: 0,
				name: "exitFullscreen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "fullscreen",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.fullscreen ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.fullscreen,
					"button-info-left-visible": e.position === "left" && J.value.fullscreen
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.fullscreen), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasZoom ? (v(), l("button", {
				key: 10,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[51] ||= (e) => J.value.zoom = !0,
				onMouseout: n[52] ||= (e) => J.value.zoom = !1,
				onClick: n[53] ||= (e) => z()
			}, [t.$slots.optionZoom ? b(t.$slots, "optionZoom", h(f({
				toggleZoom: z,
				isZoomLocked: !e.isZoom
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [e.isZoom ? (v(), s(r, {
				key: 0,
				name: "zoomUnlock",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "zoomLock",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.zoom ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.zoom,
					"button-info-left-visible": e.position === "left" && J.value.zoom
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.zoom), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasAnimation ? (v(), l("button", {
				key: 11,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[54] ||= (e) => J.value.animation = !0,
				onMouseout: n[55] ||= (e) => J.value.animation = !1,
				onClick: L
			}, [t.$slots.optionAnimation ? b(t.$slots, "optionAnimation", h(f({
				toggleAnimation: L,
				isAnimated: I.value
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [I.value ? (v(), s(r, {
				key: 0,
				name: "play",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "pause",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.fullscreen ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.animation,
					"button-info-left-visible": e.position === "left" && J.value.animation
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.animation), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasAnnotator ? (v(), l("button", {
				key: 12,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[56] ||= (e) => J.value.annotator = !0,
				onMouseout: n[57] ||= (e) => J.value.annotator = !1,
				onClick: V
			}, [t.$slots.optionAnnotator ? b(t.$slots, "optionAnnotator", h(f({
				toggleAnnotator: V,
				isAnnotator: B.value
			})), void 0, !0, 0) : (v(), l(a, { key: 1 }, [B.value ? (v(), s(r, {
				key: 0,
				name: "annotatorDisabled",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])) : (v(), s(r, {
				key: 1,
				name: "annotator",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"]))], 64)), q.value && e.titles.annotator ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.annotator,
					"button-info-left-visible": e.position === "left" && J.value.annotator
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.annotator), 7)) : c("", !0)], 32)) : c("", !0),
			e.hasAltCopy ? (v(), l("button", {
				key: 13,
				tabindex: "0",
				class: "vue-ui-user-options-button",
				onMouseenter: n[58] ||= (e) => J.value.altCopy = !0,
				onMouseout: n[59] ||= (e) => J.value.altCopy = !1,
				onClick: V
			}, [t.$slots.optionAltCopy ? b(t.$slots, "optionAltCopy", h(f({ copyAlt: H })), void 0, !0, 0) : (v(), s(r, {
				key: 1,
				name: "accessibility",
				stroke: e.color,
				style: { "pointer-events": "none" }
			}, null, 8, ["stroke"])), q.value && e.titles.altCopy ? (v(), l("div", {
				key: 2,
				dir: "auto",
				class: m({
					"button-info-left": e.position === "left",
					"button-info-right": e.position === "right",
					"button-info-right-visible": e.position === "right" && J.value.altCopy,
					"button-info-left-visible": e.position === "left" && J.value.altCopy
				}),
				style: g({
					background: e.backgroundColor,
					color: e.color
				})
			}, x(e.titles.altCopy), 7)) : c("", !0)], 32)) : c("", !0),
			b(t.$slots, "custom-menu-actions", {}, void 0, !0)
		], 12, se)], 36)), [[S(i), xe]]);
	}
}, [["__scopeId", "data-v-4fbb340e"]]);
//#endregion
export { T as n, E as t };
