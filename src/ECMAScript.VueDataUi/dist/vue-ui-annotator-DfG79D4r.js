import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { C as t, Jt as n, Nt as r, q as i } from "./lib-Bttd6u5E.js";
import { n as a, t as o } from "./useHints-Dq_w2E8B.js";
import { t as s } from "./useConfig-DlNpz6P8.js";
import { t as ee } from "./usePrinter-DN5bYhTG.js";
import { t as c } from "./dom-to-png-BYIvdTe8.js";
import { t as l } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as u } from "./BaseIcon-BfndwIWE.js";
import { t as te } from "./vue-ui-accordion-DegI2lzR.js";
import { t as ne } from "./ColorPicker--ayG2YHf.js";
import { Fragment as re, Teleport as ie, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createTextVNode as g, createVNode as _, nextTick as ae, normalizeClass as v, normalizeStyle as y, onBeforeUnmount as oe, onMounted as se, openBlock as b, ref as x, renderList as ce, renderSlot as le, toDisplayString as S, unref as ue, vModelCheckbox as de, vModelText as fe, watch as pe, withCtx as C, withDirectives as me, withModifiers as he } from "vue";
//#region src/registerAnnotatorShortcuts.js
function ge(e) {
	let t = (t) => e.isMacLike.value ? t.metaKey : t.ctrlKey, n = (e) => {
		let t = e;
		if (!t) return !1;
		let n = (t.tagName || "").toLowerCase();
		return t.isContentEditable || n === "input" || n === "textarea" || n === "select";
	}, r = (t) => !!(!e.isSummaryOpen.value || n(t.target) || e.isWriting.value), i = () => {
		e.isDeleteMode.value = !1, e.isMoveMode.value = !1, e.isResizeMode.value = !1, e.isSelectMode.value = !1, e.isDrawMode.value = !1, e.isTextMode.value = !1, e.activeShape.value = void 0, e.showCaret.value = !1;
	}, a = (t) => {
		switch (i(), t) {
			case "m":
				e.isMoveMode.value = !0;
				break;
			case "r":
				e.isResizeMode.value = !0;
				break;
			case "d":
				e.isDeleteMode.value = !0;
				break;
			case "g":
				e.isSelectMode.value = !0, e.setShapeTo("group"), e.activeShape.value = "group";
				break;
			case "t": e.isTextMode.value = !0, e.isWriting.value = !1, e.showCaret.value = !1;
		}
	}, o = (t) => {
		switch (t) {
			case "c":
				e.setShapeTo("circle");
				break;
			case "s":
				e.setShapeTo("rect");
				break;
			case "a":
				e.setShapeTo("arrow");
				break;
			case "l": e.setShapeTo("line");
		}
	}, s = (t, n) => {
		let r = e.lastSelectedShape.value;
		if (!r) return;
		let i = (e, t) => {
			typeof r[e] == "number" && (r[e] += t);
		};
		switch (r.type) {
			case "rect":
			case "circle":
			case "text":
				i("x", t), i("y", n);
				break;
			case "arrow": i("x", t), i("y", n), i("endX", t), i("endY", n);
		}
	}, ee = () => {
		let t = e.lastSelectedShape.value;
		t && (e.shapes.value = e.shapes.value.filter((e) => e.id !== t.id), e.lastSelectedShape.value = void 0);
	}, c = !1, l = null, u = () => {
		c || (c = !0, e.history?.value?.begin?.("nudge"));
	}, te = () => {
		c && (clearTimeout(l), l = setTimeout(() => {
			c = !1, e.history?.value?.end?.();
		}, 160));
	}, ne = () => {
		clearTimeout(l), c && e.history?.value?.end?.(), c = !1;
	}, re = (n) => {
		if (t(n) && !n.shiftKey && n.key.toLowerCase() === "z") {
			if (r(n)) return;
			n.preventDefault(), e.undoLastShape?.();
			return;
		}
		if (t(n) && n.shiftKey && n.key.toLowerCase() === "z" || t(n) && n.key.toLowerCase() === "y") {
			if (r(n)) return;
			n.preventDefault(), typeof e.redoLastShape == "function" ? e.redoLastShape() : e.history?.value?.redo?.();
			return;
		}
		if (r(n)) return;
		let c = n.key.toLowerCase();
		if (c === "escape") {
			n.preventDefault(), i();
			return;
		}
		if (c === "delete" || c === "backspace") {
			n.preventDefault(), ee();
			return;
		}
		if ([
			"m",
			"r",
			"d",
			"g",
			"t"
		].includes(c)) {
			n.preventDefault(), a(c);
			return;
		}
		if ([
			"c",
			"a",
			"l",
			"s"
		].includes(c)) {
			n.preventDefault(), o(c);
			return;
		}
		if (n.key === "ArrowUp" || n.key === "ArrowDown" || n.key === "ArrowLeft" || n.key === "ArrowRight") {
			n.preventDefault(), u();
			let e = n.shiftKey ? 10 : 1;
			n.key === "ArrowUp" && s(0, -e), n.key === "ArrowDown" && s(0, e), n.key === "ArrowLeft" && s(-e, 0), n.key === "ArrowRight" && s(e, 0), te();
		}
	}, ie = (e) => {
		e.key.startsWith("Arrow") && te();
	};
	return window.addEventListener("keydown", re), window.addEventListener("keyup", ie), function() {
		window.removeEventListener("keydown", re), window.removeEventListener("keyup", ie), ne();
	};
}
//#endregion
//#region src/atoms/TeleportedTooltip.vue
var _e = { class: "teleport-tooltip__inner" }, w = /*#__PURE__*/ l({
	__name: "TeleportedTooltip",
	props: {
		show: {
			type: Boolean,
			default: !1
		},
		x: {
			type: Number,
			required: !0
		},
		y: {
			type: Number,
			required: !0
		},
		placement: {
			type: String,
			default: "top"
		},
		styleObject: {
			type: Object,
			default() {
				return {};
			}
		},
		delay: {
			type: Number,
			default: 0
		},
		delayIn: {
			type: Number,
			default: 300
		},
		delayOut: {
			type: Number,
			default: 0
		}
	},
	setup(e) {
		let t = e, n = d(() => t.delayIn ?? t.delay), r = d(() => t.delayOut ?? t.delay), i = x(!1), a = null, o = null;
		function s() {
			a &&= (clearTimeout(a), null), o &&= (clearTimeout(o), null);
		}
		function ee() {
			s();
			let e = Math.max(0, n.value || 0);
			e === 0 ? i.value = !0 : a = setTimeout(() => {
				i.value = !0, a = null;
			}, e);
		}
		function c() {
			s();
			let e = Math.max(0, r.value || 0);
			e === 0 ? i.value = !1 : o = setTimeout(() => {
				i.value = !1, o = null;
			}, e);
		}
		pe(() => t.show, (e) => {
			e ? ee() : c();
		}, { immediate: !0 }), se(() => {
			t.show && ee();
		}), oe(() => {
			s();
		});
		let l = d(() => ({
			position: "fixed",
			zIndex: 2147483647,
			top: `${t.y}px`,
			left: `${t.x}px`,
			transform: t.placement === "bottom" ? "translate(-50%, 8px)" : "translate(-50%, -100%)",
			pointerEvents: "none",
			...t.styleObject
		}));
		return (t, n) => (b(), f(ie, { to: "body" }, [i.value ? (b(), m("div", {
			key: 0,
			class: v(["teleport-tooltip", e.placement]),
			style: y(l.value),
			role: "tooltip",
			"aria-hidden": "false"
		}, [h("div", _e, [le(t.$slots, "default", {}, void 0, !0)])], 6)) : p("", !0)]));
	}
}, [["__scopeId", "data-v-c292996f"]]), ve = /* @__PURE__ */ e({ default: () => yt }), ye = { class: "vue-data-ui-component vue-ui-annotator" }, be = { "data-dom-to-png-ignore": "" }, xe = ["disabled"], Se = ["disabled"], Ce = ["disabled"], we = ["disabled"], Te = ["disabled"], Ee = ["disabled"], De = ["disabled"], Oe = ["disabled"], ke = ["disabled"], Ae = {
	class: "tool-selection",
	style: { "margin-top": "6px" }
}, je = {
	viewBox: "0 0 12 12",
	style: { width: "100%" }
}, Me = ["fill"], Ne = { key: 0 }, Pe = { class: "tool-input" }, Fe = ["checked"], Ie = {
	viewBox: "0 0 12 12",
	style: { width: "100%" }
}, Le = ["fill"], Re = { key: 1 }, ze = { class: "tool-input" }, Be = ["checked"], Ve = {
	viewBox: "0 0 24 24",
	style: { width: "100%" }
}, He = ["stroke"], Ue = { key: 2 }, We = { style: {
	display: "flex",
	"flex-direction": "column",
	"align-items": "center",
	"justify-content": "center"
} }, Ge = { class: "tool-input" }, Ke = { key: 3 }, qe = { style: {
	display: "flex",
	"flex-direction": "column",
	"align-items": "center",
	"justify-content": "center"
} }, Je = { class: "tool-input" }, Ye = {
	viewBox: "0 0 24 24",
	height: "24",
	width: "24",
	style: {
		"margin-bottom": "-5px",
		"margin-top": "-10px"
	}
}, Xe = ["checked"], Ze = { key: 4 }, Qe = { style: {
	display: "flex",
	"flex-direction": "column",
	"align-items": "center",
	"justify-content": "center"
} }, $e = { class: "tool-input" }, et = { key: 5 }, tt = { key: 6 }, nt = ["disabled"], rt = { key: 7 }, it = ["disabled"], at = { key: 8 }, ot = { key: 9 }, st = { key: 10 }, ct = { key: 11 }, lt = {
	style: {
		display: "flex",
		"flex-direction": "column",
		"align-items": "center",
		"justify-content": "center"
	},
	class: "tooltip"
}, ut = { style: {
	display: "flex",
	"flex-direction": "column",
	"align-items": "start",
	"justify-content": "center"
} }, dt = {
	class: "tool-input",
	style: { "font-variant-numeric": "tabular-nums" }
}, ft = ["id"], pt = [
	"viewBox",
	"width",
	"height"
], mt = [
	"width",
	"height",
	"pointer-events"
], ht = ["innerHTML"], gt = [
	"height",
	"viewBox",
	"width"
], _t = ["cx", "cy"], vt = "annotations", yt = /*#__PURE__*/ l({
	__name: "vue-ui-annotator",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: Object,
			default() {
				return {
					shapes: [],
					lastSelectedShape: void 0
				};
			}
		}
	},
	emits: ["toggleOpenState", "saveAnnotations"],
	setup(e, { emit: l }) {
		let ie = e, _e = l, ve = x(i()), { isImaging: yt, generateImage: bt } = ee({
			elementId: ve.value,
			fileName: vt
		}), T = x(void 0), xt = x(1), E = x({
			start: {
				x: 0,
				y: 0
			},
			end: {
				x: 0,
				y: 0
			}
		}), D = x(void 0), O = x(void 0), St = x(!1), k = x(!1), Ct = x(!1), A = x(!1), wt = x(!1), Tt = x(!0), j = x(!1), Et = x(!1), Dt = x(!1), M = x(!1), Ot = x(!1), N = x(!1), P = x(!1), kt = x(!1), F = x(!1), At = x(!1), I = x(!1), L = x(ie.dataset?.lastSelectedShape ?? void 0), R = x({
			x: 0,
			y: 0
		}), jt = x(!0), z = x([]), B = x(ie.dataset?.shapes || []);
		x([]);
		let Mt = x(Math.round(Math.random()) * 1e5), Nt = x(1e3), Pt = x(1e3), V = x({
			arrow: {
				color: "grey",
				filled: !0
			},
			circle: {
				color: "grey",
				filled: !1,
				radius: 3,
				strokeWidth: 2
			},
			rect: {
				color: "grey",
				filled: !1,
				strokeWidth: 2,
				height: 12,
				width: 12
			}
		}), Ft = x("#1A1A1A"), H = x(!1), It = x(1), Lt = x(void 0), Rt = x(1), zt = x(1), U = x("start"), Bt = x(20), Vt = x(100), Ht = r, Ut = x(null), W = x(!1), G = x(null), K = x({
			x: 0,
			y: 0
		}), Wt = x(null), q = x([]), Gt = x(!1), Kt = x(null), qt = x({
			undo: 0,
			redo: 0
		}), Jt = x(null);
		x(null), x(null);
		let Yt = x(null), J = d(() => {
			let e = s().vue_ui_annotator;
			if (!Object.keys(ie.config || {}).length) return e;
			let r = n({
				defaultConfig: e,
				userConfig: ie.config
			});
			return t(r);
		});
		a({
			config: () => J.value,
			dataset: () => [],
			component: "VueUiAnnotator",
			rules: [o.noHint]
		});
		let Y = d(() => J.value.useCursorPointer), X = d(() => {
			let e = J.value.style.tooltips;
			return {
				backgroundColor: e.backgroundColor,
				color: e.color,
				border: e.border,
				borderRadius: `${e.borderRadius}px`,
				boxShadow: e.boxShadow
			};
		}), Xt = d(() => B.value.filter((e) => !["line", "group"].includes(e.type)).length > 1), Zt = d(() => Ht[Vt.value > 98 ? 98 : Vt.value]), Qt = d(() => {
			switch (!0) {
				case A.value: return "default";
				case M.value: return "move";
				case F.value: return "text";
				case N.value: return "se-resize";
				default: return "";
			}
		}), $t = d(() => B.value), en = x(null);
		function tn(e) {
			if (e) switch (!0) {
				case e.type === "rect": return `
                <rect
                id="${e.id}" 
                style="stroke-dasharray: 10; display:${O.value && O.value === e.id ? "initial" : "none"}"
                x="${e.x - 20}"
                y="${e.y - 20}"
                height="${e.rectHeight + 40}"
                width="${e.rectWidth + 40}"
                fill="transparent"
                stroke="grey"
                />
            `;
				case e.type === "circle": return `
                <rect
                id="${e.id}" 
                style="stroke-dasharray: 10; display:${O.value && O.value === e.id ? "initial" : "none"}"
                x="${e.x - e.circleRadius - 20}"
                y="${e.y - e.circleRadius - 20}"
                height="${e.circleRadius * 2 + 40}"
                width="${e.circleRadius * 2 + 40}"
                fill="transparent"
                stroke="grey"
                />
            `;
				case e.type === "arrow":
					let t = e.endX - e.x > 0, n = e.endY - e.y > 0;
					return `
                <rect
                id="${e.id}" 
                style="stroke-dasharray: 10; display:${O.value && O.value === e.id ? "initial" : "none"}"
                x="${t ? e.x - 20 : e.endX - 20}"
                y="${n ? e.y - 20 : e.endY - 20}"
                height="${n ? e.endY - e.y + 40 : e.y - e.endY + 40}"
                width="${t ? e.endX - e.x + 40 : e.x - e.endX + 40}"
                fill="transparent"
                stroke="grey"
                />
            `;
				case e.type === "text":
					let r = en.value ? Array.from(en.value.getElementsByTagName("text")).find((t) => t.id === e.id) : null;
					if (!r) return;
					let { x: i, y: a, width: o, height: s } = r.getBBox();
					return `
                <rect
                id="${e.id}" 
                style="stroke-dasharray: 10; display:${O.value && O.value === e.id ? "initial" : "none"}"
                x="${i - 20}"
                y="${a - 20}"
                height="${s + 40}"
                width="${o + 40}"
                fill="transparent"
                stroke="grey"
                />
            `;
				default: return "";
			}
		}
		function nn(e, t = !1) {
			switch (!0) {
				case e.type === "circle": return `
                <g id="${e.id}" style="display:${A.value ? "initial" : "none"};">
                    <circle id="${e.id}" cx="${e.x}" cy="${e.y}" r="12" fill="red"/>
                    <line stroke="white" stroke-width="2" id="${e.id}" x1="${e.x - 4}" y1="${e.y - 4}" x2="${e.x + 4}" y2="${e.y + 4}"/>
                    <line stroke="white" stroke-width="2" id="${e.id}" x1="${e.x + 4}" y1="${e.y - 4}" x2="${e.x - 4}" y2="${e.y + 4}"/>
                </g>
            `;
				case e.type === "text":
					let n, r = [
						-8,
						-12,
						-4,
						-12,
						-4
					];
					switch (!0) {
						case e.textAlign === "start":
							n = t ? [
								-20,
								-24,
								-16,
								-16,
								-24
							] : [
								-16,
								-20,
								-12,
								-12,
								-20
							];
							break;
						case e.textAlign === "middle":
							n = [
								0,
								-4,
								4,
								4,
								-4
							], r = [
								-32,
								-36,
								-28,
								-36,
								-28
							];
							break;
						case e.textAlign === "end":
							n = [
								16,
								20,
								12,
								12,
								20
							];
							break;
						default: n = [
							0,
							0,
							0
						];
					}
					return `
                <g id="${e.id}" style="display:${A.value ? "initial" : "none"};">
                    <circle id="${e.id}" cx="${e.x + n[0]}" cy="${e.y + r[0]}" r="12" fill="red"/>
                    <line stroke="white" stroke-width="2" id="${e.id}" x1="${e.x + n[1]}" y1="${e.y + r[1]}" x2="${e.x + n[2]}" y2="${e.y + r[2]}"/>
                    <line stroke="white" stroke-width="2" id="${e.id}" x1="${e.x + n[3]}" y1="${e.y + r[3]}" x2="${e.x + n[4]}" y2="${e.y + r[4]}"/>
                </g>
            `;
				default: return `
                <g id="${e.id}" style="display:${A.value ? "initial" : "none"};">
                    <circle id="${e.id}" cx="${e.x - 4}" cy="${e.y - 4}" r="12" fill="red"/>
                    <line stroke="white" stroke-width="2" id="${e.id}" x1="${e.x - 8}" y1="${e.y - 8}" x2="${e.x}" y2="${e.y}"/>
                    <line stroke="white" stroke-width="2" id="${e.id}" x1="${e.x}" y1="${e.y - 8}" x2="${e.x - 8}" y2="${e.y}"/>
                </g>
            `;
			}
		}
		function rn(e) {
			switch (!0) {
				case e.textAlign === "middle": return `<path class="vue-ui-annotator-caret" stroke="black" stroke-width="2" d="M${e.x},${e.y - e.fontSize} ${e.x},${e.y - e.fontSize - 15}" /> <path class="vue-ui-annotator-caret" stroke="black" stroke-width="2" d="M${e.x - 3},${e.y - e.fontSize - 5} ${e.x},${e.y - e.fontSize} ${e.x + 3},${e.y - e.fontSize - 5}"/>`;
				case e.textAlign === "start":
					let t = e.isBulletTextMode ? e.fontSize : 0;
					return `<path class="vue-ui-annotator-caret" d="M${e.x - 20 - t},${e.y - e.fontSize / 6} ${e.x - 5 - t},${e.y - e.fontSize / 6}" stroke="black" stroke-width="2" />
                    <path class="vue-ui-annotator-caret" d="M${e.x - 10 - t},${e.y - e.fontSize / 3} ${e.x - 5 - t},${e.y - e.fontSize / 6} ${e.x - 10 - t},${e.y}" stroke="black" stroke-width="2">`;
				case e.textAlign === "end": return `<path class="vue-ui-annotator-caret" d="M${e.x + 20},${e.y - e.fontSize / 6} ${e.x + 5},${e.y - e.fontSize / 6}" stroke="black" stroke-width="2" />
                    <path class="vue-ui-annotator-caret" d="M${e.x + 10},${e.y - e.fontSize / 3} ${e.x + 5},${e.y - e.fontSize / 6} ${e.x + 10},${e.y}" stroke="black" stroke-width="2">`;
				default: return "";
			}
		}
		function an(e, t, n = !1) {
			switch (!0) {
				case e.textAlign === "start": return `
            <g id="${e.id}">
                <rect 
                    id="${e.id}" 
                    style="display:${L.value && L.value.id === e.id ? "initial" : "none"};" 
                    x="${e.x}" 
                    y="${e.y - 50}" 
                    height="${e.lines === 0 || e.lines === 1 ? e.fontSize * 4 : e.fontSize * 2 * e.lines}"
                    width="100" 
                    fill="rgba(0,0,0,0)"
                />
                <text
                style="user-select:none; height:100px;"
                id="${e.id}"
                x="${e.x}"
                y="${e.y}"
                text-anchor="${e.textAlign}"
                font-size="${e.fontSize}"
                fill="${e.color}"
                font-weight="${e.isBold ? "bold" : "normal"}"
                font-style="${e.isItalic ? "italic" : "normal"}"
                text-decoration="${e.isUnderline ? "underline" : "none"}"
                >
                    ${t.join("")}
                </text>
                ${H.value && L.value && L.value.id === e.id ? rn(e) : ""}
                ${nn(e, n)}
            </g> 
            `;
				case e.textAlign === "middle": return `
                <g id="${e.id}">
                <rect 
                    id="${e.id}" 
                    style="display:${L.value && L.value.id === e.id ? "initial" : "none"};" 
                    x="${e.x - 50}" 
                    y="${e.y - 50}" 
                    height="${e.lines === 0 || e.lines === 1 ? e.fontSize * 4 : e.fontSize * 2 * e.lines}"
                    width="100" 
                    fill="rgba(0,0,0,0)"
                />
                <text
                style="user-select:none; height:100px;"
                id="${e.id}"
                x="${e.x}"
                y="${e.y}"
                text-anchor="${e.textAlign}"
                font-size="${e.fontSize}"
                fill="${e.color}"
                font-weight="${e.isBold ? "bold" : "normal"}"
                font-style="${e.isItalic ? "italic" : "normal"}"
                text-decoration="${e.isUnderline ? "underline" : "none"}"
                >
                    ${t.join("")}
                </text>
                ${H.value && L.value && L.value.id === e.id ? rn(e) : ""}
                ${nn(e)}
                </g>
            `;
				case e.textAlign === "end": return `
            <g id="${e.id}">
                <rect 
                    id="${e.id}" 
                    style="display:${L.value && L.value.id === e.id ? "initial" : "none"};" 
                    x="${e.x - 100}" 
                    y="${e.y - 50}" 
                    height="${e.lines === 0 || e.lines === 1 ? e.fontSize * 4 : e.fontSize * 2 * e.lines}"
                    width="100" 
                    fill="rgba(0,0,0,0)"
                />
                <text
                style="user-select:none; height:100px;"
                id="${e.id}"
                x="${e.x}"
                y="${e.y}"
                text-anchor="${e.textAlign}"
                font-size="${e.fontSize}"
                fill="${e.color}"
                font-weight="${e.isBold ? "bold" : "normal"}"
                font-style="${e.isItalic ? "italic" : "normal"}"
                text-decoration="${e.isUnderline ? "underline" : "none"}"
                >
                    ${t.join("")}
                </text>
                ${H.value && L.value && L.value.id === e.id ? rn(e) : ""}
                ${nn(e)}
            </g> 
            `;
				default: return "";
			}
		}
		let on = d(() => $t.value.map((e) => {
			switch (!0) {
				case e && e.type === "arrow":
					let t = e.strokeWidth > 3 ? 5 : 10, n = e.strokeWidth > 3 ? 2.5 : 5;
					return {
						html: `
          <defs>
          <marker 
              id="${e.id}" 
              markerWidth="${t}" 
              markerHeight="${t}" 
              refX="0" 
              refY="${n}" 
              orient="auto"
          >
              <polygon 
              points="0 0,${t} ${n}, 0 ${t}" 
              fill="${e.color}"
              />
          </marker>
          </defs>
          ${tn(e)}
          <g id="${e.id}">
              <path 
              style="stroke-linecap: round !important; ${e.isDash ? `stroke-dasharray: ${e.strokeWidth * 3}` : ""}" 
              stroke="${e.color}" 
              id="${e.id}" 
              d="M${e.x},${e.y} ${e.endX},${e.endY}" 
              stroke-width="${e.strokeWidth}" 
              marker-end="url(#${e.id})"
              />
          </g>
          <g id="${e.id}">
          <rect 
              id="${e.id}"
              x="${e.x - 10}"
              y="${e.y - 10}"
              height="20"
              width="20"
              fill="rgba(0,0,0,0.3)"
              style="display:${N.value || M.value ? "initial" : "none"}; rx:1 !important; ry:1 !important;"
          />
          </g>
          ${nn(e)}
          </g>
          `,
						id: e.id
					};
				case e && e.type === "circle": return {
					html: `
          <g id="${e.id}">
              ${tn(e)}
              <circle 
              id="${e.id}" 
              cx="${e.x}" 
              cy="${e.y}" 
              r="${e.circleRadius ? e.circleRadius : Number.MIN_VALUE}"
              fill="${e.isFilled ? e.color + e.alpha : "rgba(255,255,255,0.001)"}" 
              stroke="${e.color + e.alpha}" 
              stroke-width="${e.strokeWidth}"
              style="${e.isDash ? `stroke-dasharray: ${e.strokeWidth * 3}` : ""}"
              >
              </circle>
          </g>
          
          ${nn(e)}`,
					id: e.id
				};
				case e && e.type === "group": return {
					html: `<g id="${e.id}">
            <rect
                id="${N.value ? "" : e.id}"
                x="${e.x}"
                y="${e.y}"
                fill="transparent"
                height="${e.rectHeight}"
                width="${e.rectWidth}"
                stroke="grey"
                stroke-width="1"
                style="rx:1 !important; ry:1 !important; ${e.isDash ? `stroke-dasharray: ${e.strokeWidth * 3}` : ""}; display:${P.value || A.value || O.value && O.value === e.id ? "initial" : "none"};"
                        />
            <g id="${e.id}">
            ${e.content ? e.content : ""}
            </g>
            ${nn(e)}
            </g> `,
					id: e.id
				};
				case e && e.type === "rect": return {
					html: `<g id="${e.id}">
            ${tn(e)}
            <rect
                id="${N.value ? "" : e.id}"
                x="${e.x}"
                y="${e.y}"
                fill="${e.isFilled ? e.color + e.alpha : "rgba(255,255,255,0.001)"}"
                height="${e.rectHeight}"
                width="${e.rectWidth}"
                stroke="${e.color + e.alpha}"
                stroke-width="${e.strokeWidth}"
                style="rx:1 !important; ry:1 !important; ${e.isDash ? `stroke-dasharray: ${e.strokeWidth * 3}` : ""}"
            />
            <rect id="${e.id}"
                x="${e.x + e.rectWidth}"
                y="${e.y + e.rectHeight}"
                height="20"
                width="20"
                fill="rgba(0,0,0,0.3)"
                style="display:${N.value ? "initial" : "none"}; rx:1 !important; ry:1 !important;"
            />
            ${nn(e)}
            </g> `,
					id: e.id
				};
				case e && e.type === "line": return {
					html: `
                <g id="${e.id}">
                    <path 
                    id="${e.id}" 
                    d="M${e.path ? e.path : ""}" 
                    style="stroke:${e.color + e.alpha} !important; fill:none; stroke-width:${e.strokeWidth} !important; stroke-linecap: round !important; stroke-linejoin: round !important;"        
                    />
            ${nn(e)}
                </g>
                `,
					id: e.id
				};
				case e && e.type === "text":
					let r = e.textContent.split("‎"), i = [];
					for (let t = 0; t < r.length; t += 1) i.push(`
        ${e.isBulletTextMode ? `<tspan x="${e.x - e.fontSize}" y="${e.y + e.fontSize * t}" id="${e.id}" font-size="${e.fontSize / 2}">⬤</tspan>` : ""}
                <tspan id="${e.id}" x="${e.x}" y="${e.y + e.fontSize * t}">
                    ${r[t]}
                </tspan>`);
					return {
						html: `
            ${tn(e)}
            ${an(e, i, e.isBulletTextMode)}
            `,
						id: e.id
					};
			}
		}));
		function Z(e) {
			if (e == null) return e;
			try {
				return typeof structuredClone == "function" ? structuredClone(e) : JSON.parse(JSON.stringify(e));
			} catch {
				return Array.isArray(e) ? e.map((e) => Z(e)) : typeof e == "object" ? Object.fromEntries(Object.entries(e).map(([e, t]) => [e, Z(t)])) : e;
			}
		}
		function sn(e = !1) {
			if (!T.value || !wt.value) return;
			E.value.end = {
				x: R.value.x,
				y: R.value.y
			};
			let t;
			B.value.length > 0 && D.value && (t = [...B.value].find((e) => e.id === D.value.id));
			let n, r, i;
			t && (n = t.x - E.value.end.x, r = t.y - E.value.end.y, i = Math.sqrt(n * n + r * r));
			let a, o;
			switch (e ? (a = Math.max(E.value.end.x, t.x), o = Math.min(E.value.end.x, t.x), Math.max(E.value.end.y, t.y), Math.min(E.value.end.y, t.y)) : (a = Math.max(E.value.end.x, E.value.start.x), o = Math.min(E.value.end.x, E.value.start.x), Math.max(E.value.end.y, E.value.start.y), Math.min(E.value.end.y, E.value.start.y)), !0) {
				case T.value === "arrow":
					B.value.at(-1).endX = E.value.end.x, B.value.at(-1).endY = E.value.end.y;
					break;
				case T.value === "circle":
					B.value.at(-1).circleRadius = Tt.value ? Z(a - o) + 20 : i + 20;
					break;
				case T.value === "line":
					B.value.at(-1).path += ` ${R.value.x} ${R.value.y} `;
					break;
				case ["rect", "group"].includes(T.value): B.value.at(-1).rectWidth = Z(E.value.end.x - B.value.at(-1).x) > 0 ? Z(E.value.end.x - B.value.at(-1).x) : 20, B.value.at(-1).rectHeight = Z(E.value.end.y - B.value.at(-1).y) > 0 ? Z(E.value.end.y - B.value.at(-1).y) : 20;
			}
		}
		function cn() {
			if (!wt.value) {
				Jt.value = null;
				return;
			}
			sn(), Jt.value = requestAnimationFrame(cn);
		}
		function ln(e) {
			let t = B.value.findIndex((t) => t.id === e);
			if (t > -1 && t !== B.value.length - 1) {
				let [e] = B.value.splice(t, 1);
				B.value.push(e);
			}
		}
		function un(e) {
			let t = (e) => !!e && B.value.some((t) => t.id === e);
			if (t(e?.target?.id)) return e.target.id;
			let n = en.value?.querySelector(".annotator__glass");
			if (!n) return null;
			let r = n.style.pointerEvents;
			n.style.pointerEvents = "none";
			let i = document.elementFromPoint(e.clientX, e.clientY);
			return n.style.pointerEvents = r || "all", t(i?.id) ? i.id : null;
		}
		function dn(e = {}) {
			let { maxEntries: t = 200, maxBytes: n = 2e6 } = e, r = {
				open: !1,
				before: null,
				undo: [],
				redo: []
			}, i = {
				undo: 0,
				redo: 0
			}, a = (e) => typeof e == "string" ? e.length * 2 : 0, o = () => JSON.stringify({
				shapes: Z(B.value),
				lastSelectedShape: Z(L.value)
			}), s = (e) => {
				let t = JSON.parse(e);
				B.value = t.shapes, L.value = t.lastSelectedShape;
			}, ee = () => {
				qt.value.undo = r.undo.length, qt.value.redo = r.redo.length;
			}, c = (e, o) => {
				let s = r[e];
				for (s.push(o), i[e] += a(o); s.length > t || i[e] > n;) {
					let t = s.shift();
					i[e] -= a(t);
				}
			};
			return {
				begin() {
					r.open || (r.open = !0, r.before = o());
				},
				end() {
					r.open && (r.open = !1, o() !== r.before && (c("undo", r.before), r.redo.length = 0, i.redo = 0), r.before = null, ee());
				},
				undo() {
					let e = r.undo.pop();
					if (!e) return;
					i.undo -= a(e);
					let t = o();
					c("redo", t), s(e), ee();
				},
				redo() {
					let e = r.redo.pop();
					if (!e) return;
					i.redo -= a(e);
					let t = o();
					c("undo", t), s(e), ee();
				},
				size() {
					return {
						undo: r.undo.length,
						redo: r.redo.length,
						approxBytes: {
							undo: i.undo,
							redo: i.redo
						}
					};
				}
			};
		}
		function fn() {
			if (!Wt.value) return;
			let e = Wt.value.getBoundingClientRect();
			K.value = {
				x: e.left + e.width / 2,
				y: e.top
			};
		}
		function Q(e, t, n = "top") {
			G.value = e, Wt.value = t.currentTarget || t.target, fn(), W.value = !0, window.addEventListener("scroll", fn, !0), window.addEventListener("resize", fn, { passive: !0 });
		}
		function $() {
			W.value = !1, G.value = null, Wt.value = null, window.removeEventListener("scroll", fn, !0), window.removeEventListener("resize", fn);
		}
		function pn(e) {
			let t = e?.target && e.target.id || D.value && D.value.id, n = B.value.find((e) => e.id === t);
			if (!n) {
				Ut.value = null;
				return;
			}
			L.value = n;
			let r = R.value.x, i = R.value.y;
			switch (n.type) {
				case "rect":
				case "circle":
				case "text":
					Ut.value = {
						dx: r - n.x,
						dy: i - n.y
					};
					break;
				case "arrow":
					Ut.value = {
						dx: r - n.x,
						dy: i - n.y,
						endDx: r - n.endX,
						endDy: i - n.endY
					};
					break;
				case "group":
					Ut.value = {
						dx: r - (n.x || 0),
						dy: i - (n.y || 0)
					};
					break;
				default: Ut.value = {
					dx: 0,
					dy: 0
				};
			}
		}
		function mn(e) {
			let t = B.value.find((e) => e.id === L.value.id);
			switch (!0) {
				case e === "front":
					B.value = B.value.filter((e) => e.id !== t.id), B.value.push(t);
					break;
				case e === "back":
					B.value = B.value.filter((e) => e.id !== t.id), B.value = [t, ...B.value];
					break;
				default: return;
			}
		}
		function hn() {
			if (!L.value?.id) return;
			let e = {
				...L.value,
				id: `${L.value.id}_copy_${i()}`,
				x: L.value.x - 100 < 0 ? 1 : L.value.x - 100,
				y: L.value.y - 100 < 0 ? 1 : L.value.y - 100
			};
			B.value.push(e);
		}
		function gn(e) {
			e.preventDefault(), jt.value = !1, e.target && e.target.id && (O.value = e.target.id);
		}
		function _n() {
			!L.value || !L.value.id.includes("text") || L.value.textContent === "" && (B.value = B.value.filter((e) => e.id !== L.value.id), L.value = B.value.at(-1));
		}
		function vn(e) {
			if (A.value) return;
			e.preventDefault(), e.stopPropagation(), _n(), F.value ? (I.value = !0, H.value = !0) : (I.value = !1, H.value = !1, F.value = !1);
			let t = `text_${i()}`;
			if (I.value) {
				q.value?.begin(), B.value.push({
					id: t,
					type: "text",
					lines: 0,
					x: R.value.x,
					y: R.value.y,
					textContent: "",
					fontSize: Z(Bt.value),
					textAlign: Z(U.value),
					isBold: Z(St.value),
					isItalic: Z(Et.value),
					isUnderline: Z(At.value),
					color: Z(Ft.value),
					isBulletTextMode: Z(k.value)
				}), D.value = B.value.at(-1), L.value = B.value.at(-1), q.value?.end();
				return;
			}
			let n = () => {
				Ct.value = B.value.find((t) => t.id === e.target.id).isDash;
			}, r = () => {
				xt.value = B.value.find((t) => t.id === e.target.id).strokeWidth;
			};
			if (P.value = !1, e.target.id.includes("arrow")) {
				T.value = "arrow", n(), r();
				return;
			}
			if (e.target.id.includes("circle")) {
				T.value = "circle", V.value.circle.filled = B.value.find((t) => t.id === e.target.id).isFilled, n(), r();
				return;
			}
			if (e.target.id.includes("rect")) {
				T.value = "rect", V.value.rect.filled = B.value.find((t) => t.id === e.target.id).isFilled, n(), r();
				return;
			}
			if (e.target.id.includes("line")) {
				T.value = "line", r();
				return;
			}
			if (e.target.id.includes("text")) {
				F.value = !0, I.value = !0, H.value = !0;
				let t = B.value.find((t) => t.id === e.target.id);
				t && t.textAlign && (U.value = B.value.find((t) => t.id === e.target.id).textAlign), t && (k.value = B.value.find((t) => t.id === e.target.id).isBulletTextMode);
				return;
			}
		}
		function yn(e) {
			!L.value || L.value.type !== "text" || (L.value.textAlign = e);
		}
		function bn() {
			q.value?.undo?.();
		}
		function xn() {
			q.value?.redo?.();
		}
		let Sn = [
			16,
			17,
			18,
			20,
			27,
			33,
			34,
			35,
			36,
			37,
			38,
			39,
			40,
			45,
			91,
			112,
			113,
			114,
			115,
			116,
			117,
			118,
			119,
			120,
			121,
			122,
			123,
			221,
			255,
			"Unidentified"
		];
		function Cn(e) {
			if (jt.value) return;
			e.preventDefault();
			let t = e.keyCode;
			if (!I.value) return;
			H.value = !0;
			let n;
			if (n = L.value.type === "text" ? B.value.find((e) => e.id === L.value.id) : B.value.at(-1), D.value = n, n.type === "text") switch (D.value.isBold = Z(St.value), D.value.isItalic = Z(Et.value), D.value.isUnderline = Z(At.value), !0) {
				case [8, 46].includes(t):
					n.textContent = n.textContent.slice(0, -1);
					break;
				case t === 9:
					n.textContent += "&nbsp; &nbsp; &nbsp; &nbsp;";
					break;
				case t === 13:
					n.lines += 1, n.textContent += "‎";
					return;
				case Sn.includes(t): return;
				default: n.textContent += e.key;
			}
		}
		function wn() {
			if (z.value = [], T.value !== "group") {
				P.value = !1, B.value = B.value.filter((e) => e.type !== "group");
				return;
			}
			let e = B.value.at(-1);
			if (B.value.forEach((t) => {
				if (t.type !== "group") switch (!0) {
					case t.type === "arrow":
						let n = t.x <= t.endX && t.y <= t.endY && e.x <= t.x && e.y <= t.y && e.x + e.rectWidth >= t.endX && e.y + e.rectHeight >= t.endY, r = t.endY < t.y && t.x < t.endX && e.x <= t.x && e.y <= t.y && e.x + e.rectWidth >= t.endX && e.y + e.rectHeight >= t.y, i = t.x > t.endX && t.y < t.endY && e.x <= t.endX && e.y <= t.endY && e.x + e.rectWidth >= t.x && e.y + e.rectHeight >= t.endY, a = t.x > t.endX && t.y > t.endY && e.x <= t.endX && e.y <= t.endY && e.x + e.rectWidth >= t.x && e.y + e.rectHeight >= t.y;
						(n || r || i || a) && z.value.push(t);
						break;
					case t.type === "circle":
						e.x <= t.x + t.circleRadius && e.y <= t.y + t.circleRadius && t.x + t.circleRadius <= e.x + e.rectWidth && t.y + t.circleRadius <= e.y + e.rectHeight && z.value.push(t);
						break;
					case t.type === "rect":
						e.x <= t.x && e.y <= t.y && t.x <= e.x + e.rectWidth && t.y <= e.y + e.rectHeight && t.x + t.rectWidth <= e.x + e.rectWidth && t.y + t.rectHeight <= e.y + e.rectHeight && t.rectWidth <= e.rectWidth && t.rectHeight <= e.rectHeight && z.value.push(t);
						break;
					case t.type === "text": e.x <= t.x && e.y <= t.y && z.value.push(t);
				}
			}), z.value = z.value.map((t) => ({
				...t,
				id: e.id,
				oldId: t.id,
				diffX: t.x - e.x,
				diffY: t.y - e.y,
				diffEndX: t.endX ? t.endX - e.x : 0,
				diffEndY: t.endY ? t.endY - e.y : 0
			})), e.source = z.value, z.value.length > 1) {
				let t = Z(z.value).map((e) => e.oldId);
				B.value = B.value.filter((e) => !t.includes(e.id)), z.value.forEach((t) => {
					switch (!0) {
						case t.type === "circle":
							e.content += `
            <circle
            id="${t.id}"
            cx="${t.x}"
            cy="${t.y}"
            r="${t.circleRadius ? t.circleRadius : Number.MIN_VALUE}"
            fill="${t.isFilled ? t.color + t.alpha : "rgba(255,255,255,0.001)"}"
            stroke="${t.color + t.alpha}" 
            stroke-width="${t.strokeWidth}"
            style="${t.isDash ? `stroke-dasharray: ${t.strokeWidth * 3}` : ""}"
            />
        `;
							break;
						case t.type === "rect":
							e.content += `
            <rect
            id="${N.value ? "" : t.id}"
            x="${t.x}"
            y="${t.y}"
            fill="${t.isFilled ? t.color + t.alpha : "rgba(255,255,255,0.001)"}"
            height="${t.rectHeight}"
            width="${t.rectWidth}"
            stroke="${t.color + t.alpha}"
            stroke-width="${t.strokeWidth}"
            style="rx:1 !important; ry:1 !important; ${t.isDash ? `stroke-dasharray: ${t.strokeWidth * 3}` : ""}"
                        />
        `;
							break;
						case t.type === "arrow":
							let n = t.strokeWidth > 3 ? 5 : 10, r = t.strokeWidth > 3 ? 2.5 : 5, a = i();
							e.content += `
            <g id="${t.id}">
            <defs>
                <marker 
                id="${a}" 
                markerWidth="${n}" 
                markerHeight="${n}" 
                refX="0" 
                refY="${r}" 
                orient="auto"
                >
                <polygon 
                points="0 0,${n} ${r}, 0 ${n}" 
                fill="${t.color}"
                />
                </marker>
            </defs>

            <path 
                style="stroke-linecap: round !important; ${t.isDash ? `stroke-dasharray: ${t.strokeWidth * 3}` : ""}" 
                stroke="${t.color}" 
                id="${t.id}" 
                d="M${t.x},${t.y} ${t.endX},${t.endY}" 
                stroke-width="${t.strokeWidth}" 
                marker-end="url(#${a})"
            />
            </g>
        `;
							break;
						case t.type === "text":
							let o = t.textContent.split("‎"), s = [];
							for (let e = 0; e < o.length; e += 1) s.push(`
            ${t.isBulletTextMode ? `<tspan x="${t.x - t.fontSize}" y="${t.y + t.fontSize * e}" id="${t.id}" font-size="${t.fontSize / 2}">⬤</tspan>` : ""}
            <tspan id="${t.id}" x="${t.x}" y="${t.y + t.fontSize * e}">
                ${o[e]}
            </tspan>`);
							e.content += `
            ${an(t, s, t.isBulletTextMode)}
            `;
					}
				});
			} else B.value = B.value.filter((t) => t.id !== e.id);
		}
		function Tn(e) {
			e.content = "";
			let t = e.x || 0, n = e.y || 0;
			(e.source || []).forEach((r) => {
				switch (r.type) {
					case "circle": {
						let i = t + r.diffX, a = n + r.diffY;
						e.content += `
          <circle
            id="${r.id}"
            cx="${i}"
            cy="${a}"
            r="${r.circleRadius ? r.circleRadius : Number.MIN_VALUE}"
            fill="${r.isFilled ? r.color + r.alpha : "rgba(255,255,255,0.001)"}"
            stroke="${r.color + r.alpha}"
            stroke-width="${r.strokeWidth}"
            style="${r.isDash ? `stroke-dasharray: ${r.strokeWidth * 3}` : ""}"
          />
        `;
						break;
					}
					case "rect": {
						let i = t + r.diffX, a = n + r.diffY;
						e.content += `
          <rect
            id="${N.value ? "" : r.id}"
            x="${i}"
            y="${a}"
            fill="${r.isFilled ? r.color + r.alpha : "rgba(255,255,255,0.001)"}"
            height="${r.rectHeight}"
            width="${r.rectWidth}"
            stroke="${r.color + r.alpha}"
            stroke-width="${r.strokeWidth}"
            style="rx:1 !important; ry:1 !important; ${r.isDash ? `stroke-dasharray: ${r.strokeWidth * 3}` : ""}"
          />
        `;
						break;
					}
					case "arrow": {
						let a = t + r.diffX, o = n + r.diffY, s = t + r.diffEndX, ee = n + r.diffEndY, c = r.strokeWidth > 3 ? 5 : 10, l = r.strokeWidth > 3 ? 2.5 : 5, u = `m_${r.id}_${i()}`;
						e.content += `
          <g id="${r.id}">
            <defs>
              <marker 
                id="${u}" 
                markerWidth="${c}" 
                markerHeight="${c}" 
                refX="0" 
                refY="${l}" 
                orient="auto">
                <polygon points="0 0,${c} ${l}, 0 ${c}" fill="${r.color}" />
              </marker>
            </defs>
            <path
              style="stroke-linecap: round !important; ${r.isDash ? `stroke-dasharray: ${r.strokeWidth * 3}` : ""}"
              stroke="${r.color}"
              id="${r.id}"
              d="M${a},${o} ${s},${ee}"
              stroke-width="${r.strokeWidth}"
              marker-end="url(#${u})"
            />
          </g>
        `;
						break;
					}
					case "text": {
						let i = (r.textContent || "").split("‎").map((e, i) => `
          ${r.isBulletTextMode ? `<tspan x="${t + r.diffX - r.fontSize}" y="${n + r.diffY + r.fontSize * i}" id="${r.id}" font-size="${r.fontSize / 2}">⬤</tspan>` : ""}
          <tspan id="${r.id}" x="${t + r.diffX}" y="${n + r.diffY + r.fontSize * i}">
            ${e}
          </tspan>
        `).join("");
						e.content += `
          <g id="${r.id}">
            <text
              style="user-select:none; height:100px;"
              id="${r.id}"
              x="${t + r.diffX}"
              y="${n + r.diffY}"
              text-anchor="${r.textAlign}"
              font-size="${r.fontSize}"
              fill="${r.color}"
              font-weight="${r.isBold ? "bold" : "normal"}"
              font-style="${r.isItalic ? "italic" : "normal"}"
              text-decoration="${r.isUnderline ? "underline" : "none"}">
              ${i}
            </text>
          </g>
        `;
						break;
					}
				}
			});
		}
		function En(e) {
			e.relatedTarget && en.value && en.value.contains(e.relatedTarget) || (jt.value = !0, O.value = void 0);
		}
		function Dn() {
			if (wt.value = !0, !T.value && !P.value || !wt.value) return;
			Tt.value = !0, E.value.start = {
				x: R.value.x,
				y: R.value.y
			};
			let e = `${P.value ? "group" : T.value}_${i()}`;
			switch (!0) {
				case T.value === "arrow":
					B.value.push({
						id: e,
						x: R.value.x,
						y: R.value.y,
						endX: R.value.x,
						endY: R.value.y,
						type: T.value,
						color: Z(Ft.value),
						strokeWidth: Z(Math.abs(xt.value)),
						isDash: Z(Ct.value)
					}), L.value = B.value.at(-1);
					break;
				case T.value === "circle":
					B.value.push({
						alpha: V.value.circle.filled ? Zt.value : "",
						id: e,
						color: Z(Ft.value),
						isFilled: Z(V.value.circle.filled),
						circleRadius: Z(V.value.circle.radius),
						circleStrokeWidth: Z(V.value.circle.strokeWidth),
						type: T.value,
						x: R.value.x,
						y: R.value.y,
						strokeWidth: Z(Math.abs(xt.value)),
						isDash: Z(Ct.value)
					}), L.value = B.value.at(-1);
					break;
				case T.value === "line":
					B.value.push({
						alpha: Z(Zt.value),
						id: e,
						x: R.value.x,
						y: R.value.y,
						type: T.value,
						color: Z(Ft.value),
						strokeWidth: Z(Math.abs(xt.value)),
						isDash: Z(Ct.value),
						path: `${R.value.x} ${R.value.y}`
					}), L.value = B.value.at(-1);
					break;
				case T.value === "rect":
					B.value.push({
						alpha: V.value.rect.filled ? Zt.value : "",
						id: e,
						color: Z(Ft.value),
						isFilled: Z(V.value.rect.filled),
						rectStrokeWidth: Z(V.value.rect.strokeWidth),
						rectHeight: Z(V.value.rect.height),
						rectWidth: Z(V.value.rect.width),
						type: T.value,
						x: R.value.x,
						y: R.value.y,
						strokeWidth: Z(Math.abs(xt.value)),
						isDash: Z(Ct.value)
					}), L.value = B.value.at(-1);
					break;
				case T.value === "group": B.value.push({
					alpha: 1,
					id: `group_${i()}`,
					x: R.value.x,
					y: R.value.y,
					isFilled: !1,
					rectHeight: Z(V.value.rect.height),
					rectWidth: Z(V.value.rect.width),
					rectStrokeWidth: 1,
					type: "group",
					color: "grey",
					strokeWidth: 1,
					isDash: !0,
					content: ""
				});
			}
			Jt.value ||= requestAnimationFrame(cn);
		}
		function On(e) {
			if (A.value) {
				Dt.value = !1;
				return;
			}
			if (e.preventDefault(), e.stopPropagation(), Dt.value = !0, e.pointerId != null) {
				try {
					en.value?.setPointerCapture?.(e.pointerId);
				} catch {}
				Yt.value = e.pointerId;
			} else Yt.value = null;
			if ((j.value || M.value || N.value || P.value) && q.value?.begin?.(), j.value) {
				Dn();
				return;
			}
			if (M.value) {
				let t = un(e) || O.value || L.value?.id;
				t && (ln(t), D.value = { id: t }), pn(e);
			}
		}
		function kn(e) {
			if (!e || !e.id || e.type === "line" || !Ut.value && (pn({ target: { id: e.id } }), !Ut.value)) return;
			let { dx: t, dy: n, endDx: r, endDy: i } = Ut.value, a = R.value.x, o = R.value.y;
			switch (L.value = e, e.type) {
				case "arrow":
					e.x = a - t, e.y = o - n, e.endX = a - (r ?? t), e.endY = o - (i ?? n);
					break;
				case "circle":
					e.x = a - t, e.y = o - n;
					break;
				case "rect":
					e.x = a - t, e.y = o - n;
					break;
				case "text":
					e.x = a - t, e.y = o - n;
					break;
				case "group": e.x = a - t, e.y = o - n, Tn(e);
			}
		}
		function An() {
			let e = D.value?.id || O.value;
			if (!e) return;
			let t = B.value.find((t) => t.id === e);
			t && kn(t);
		}
		function jn() {
			Tt.value = !1;
			let e = D.value.id;
			if (!e) return;
			wt.value = !0;
			let t = B.value.find((t) => t.id === e);
			T.value = t.type, B.value = B.value.filter((t) => t.id !== e), B.value.push(t), sn(!0);
		}
		function Mn(e) {
			A.value || (e.preventDefault(), e.stopPropagation(), e.target.localName !== "svg" && (D.value = e.target), M.value && Dt.value ? An() : N.value && Dt.value && jn());
		}
		function Nn(e) {
			let t = e.target.id;
			switch (!0) {
				case A.value:
					q.value?.begin(), B.value = [...B.value].filter((e) => e.id !== t), L.value = void 0, q.value?.end();
					return;
				default: L.value = B.value.find((e) => e.id === t);
			}
		}
		let Pn = x(null);
		function Fn(e, t) {
			for (t(e), e = e.firstChild; e;) Fn(e, t), e = e.nextSibling;
		}
		function In() {
			Ot.value = !0, A.value = !1, M.value = !1, N.value = !1, F.value = !1, I.value = !1, P.value = !1, T.value = void 0, H.value = !1, ae(async () => {
				let e = Pn.value;
				if (e) {
					Fn(e, (e) => {
						e && e.nodeType === 1 && (e.setAttribute("font-family", "Helvetica"), e.style.fontFamily = "Helvetica");
					});
					try {
						let t;
						try {
							t = (await import("jspdf")).default;
						} catch {
							throw Error("jspdf is not installed. Run npm install jspdf");
						}
						let n = await c({
							container: e,
							scale: 2
						}), r = new Image();
						r.src = n, r.onload = () => {
							let e = {
								width: 595.28,
								height: 841.89
							}, i = r.width, a = r.height, o = i / e.width * e.height, s = e.width, ee = s / i * a, c = new t("", "pt", "a4"), l = 0, u = a;
							if (u < o) c.addImage(n, "PNG", 0, 0, s, ee, "", "FAST");
							else for (; u > 0;) c.addImage(n, "PNG", 0, l, s, ee, "", "FAST"), u -= o, l -= e.height, u > 0 && c.addPage();
							c.save(`${(/* @__PURE__ */ new Date()).toLocaleDateString()}_annotations.pdf`);
						};
					} catch (e) {
						console.error("Error generating image:", e);
					} finally {
						Ot.value = !1, Fn(e, (e) => {
							e && e.nodeType === 1 && (e.setAttribute("font-family", J.value.style.fontFamily), e.style.fontFamily = J.value.style.fontFamily);
						});
					}
				}
			});
		}
		function Ln(e) {
			if (!A.value && e && (e.preventDefault(), e.stopPropagation()), wt.value = !1, Dt.value = !1, Ut.value = null, Yt.value != null) {
				try {
					en.value?.releasePointerCapture?.(Yt.value);
				} catch {}
				Yt.value = null;
			}
			Jt.value &&= (cancelAnimationFrame(Jt.value), null), P.value && wn(), q.value?.end();
		}
		function Rn() {
			!L.value || !L.value.id.includes("rect") || (L.value.isFilled = !L.value.isFilled);
		}
		function zn() {
			!L.value || !L.value.id.includes("circle") || (L.value.isFilled = !L.value.isFilled);
		}
		function Bn() {
			!L.value || L.value.type === "text" || (L.value.isDash = Z(Ct.value));
		}
		function Vn() {
			!L.value || ["arrow", "text"].includes(L.value.id) || (L.value.alpha = Z(Zt.value));
		}
		function Hn() {
			!L.value || ![
				"arrow",
				"circle",
				"rect",
				"line"
			].includes(L.value.type) || (L.value.strokeWidth = Z(Math.abs(xt.value)));
		}
		function Un() {
			!L.value || L.value.type !== "text" || (L.value.isBold = Z(St.value), L.value.isItalic = Z(Et.value), L.value.isUnderline = Z(At.value), L.value.fontSize = Z(Bt.value), L.value.isBulletTextMode = Z(k.value));
		}
		function Wn(e) {
			if (e.preventDefault(), !en.value) return;
			let t = en.value.getBoundingClientRect(), n, r;
			e.touches && e.touches.length > 0 ? (n = e.touches[0].clientX, r = e.touches[0].clientY) : (n = e.clientX, r = e.clientY), R.value.x = (n - t.left) / t.width * Pt.value, R.value.y = (r - t.top) / t.height * Nt.value;
		}
		function Gn(e) {
			if (H.value = !1, _n(), e === T.value) {
				T.value = void 0, j.value = !1;
				return;
			}
			j.value = !0, A.value = !1, M.value = !1, N.value = !1, F.value = !1, T.value = e;
		}
		function Kn() {
			kt.value = !kt.value, kt.value || (M.value = !1, N.value = !1, F.value = !1, I.value = !1, T.value = void 0, H.value = !1, A.value = !1, I.value = !1), _e("toggleOpenState", { isOpen: kt.value });
		}
		function qn() {
			_e("saveAnnotations", {
				shapes: B.value,
				lastSelectedShape: L.value
			});
		}
		let Jn = null;
		return se(() => {
			if (Pn.value) {
				let e = !1;
				Fn(Pn.value, (t) => {
					if (!e && [
						"DIV",
						"svg",
						"section",
						"canvas"
					].includes(t.tagName)) {
						Lt.value = t, e = !0;
						return;
					}
				});
			}
			Gt.value = (() => {
				if (typeof navigator > "u") return !1;
				let e = navigator.userAgentData?.platform ?? "";
				if (e) return /mac|ios/i.test(e);
				let t = navigator.userAgent ?? "";
				return /(Mac|iPhone|iPad|iPod)/i.test(t);
			})();
			let e = Lt?.value.getBoundingClientRect();
			if (It.value = e.height / e.width, Pt.value = 1e3, Nt.value = It.value * 1e3, Rt.value = e.width, zt.value = e.height, new ResizeObserver((e) => {
				e.forEach((e) => {
					Rt.value = e.contentRect.width, zt.value = e.contentRect.height, It.value = e.contentRect.height / e.contentRect.width, Nt.value = It.value * 1e3;
				});
			}).observe(Lt.value), Kt.value = (e) => Cn(e), window.addEventListener("keydown", Kt.value), q.value = dn(), q.value.size) {
				let e = q.value.size();
				qt.value.undo = e.undo, qt.value.redo = e.redo;
			}
			Jn = ge({
				isMacLike: Gt,
				isSummaryOpen: kt,
				isWriting: I,
				isDeleteMode: A,
				isMoveMode: M,
				isResizeMode: N,
				isSelectMode: P,
				isDrawMode: j,
				isTextMode: F,
				activeShape: T,
				showCaret: H,
				lastSelectedShape: L,
				shapes: B,
				history: q,
				setShapeTo: Gn,
				undoLastShape: bn,
				redoLastShape: xn
			});
		}), oe(() => {
			$(), Jt.value && cancelAnimationFrame(Jt.value), Kt.value && window.removeEventListener("keydown", Kt.value), Jn && Jn(), window.removeEventListener("keydown", Cn);
		}), pe(B, (e) => {
			e.length === 0 && (L.value = void 0);
		}), pe(F, (e) => {
			H.value = e;
		}), (e, t) => (b(), m("div", ye, [h("div", be, [_(te, {
			config: {
				maxHeight: 1e3,
				useCursorPointer: Y.value,
				head: {
					backgroundColor: J.value.style.backgroundColor,
					color: J.value.style.color,
					iconColor: J.value.style.color,
					iconSize: 20,
					icon: kt.value ? "close" : "annotator",
					padding: "6px"
				},
				body: {
					backgroundColor: J.value.style.backgroundColor,
					color: J.value.style.color
				}
			},
			onToggle: Kn
		}, {
			title: C(({ color: e }) => [h("div", { style: y({ color: e }) }, S(J.value.translations.title), 5)]),
			content: C(({ backgroundColor: e }) => [h("div", {
				class: "tool-selection",
				style: y({ backgroundColor: e })
			}, [
				h("button", {
					disabled: B.value.length === 0,
					style: y({
						background: M.value ? J.value.style.buttons.controls.selected.backgroundColor : J.value.style.buttons.controls.backgroundColor,
						border: M.value ? J.value.style.buttons.controls.selected.border : J.value.style.buttons.controls.border,
						color: M.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						"button-tool--selected": M.value,
						tooltip: !0
					}),
					onClick: t[0] ||= (e) => {
						_n(), M.value = !M.value, T.value = void 0, A.value = !1, j.value = !1, N.value = !1, P.value = !1, F.value = !1, I.value = !1, H.value = !1;
					},
					onMouseenter: t[1] ||= (e) => J.value.style.showTooltips && Q("move", e, "top"),
					onMouseleave: $,
					onFocus: t[2] ||= (e) => J.value.style.showTooltips && Q("move", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "move",
					stroke: M.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "move",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipMove) + " ", 1), t[91] ||= h("kbd", null, "M", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 46, xe),
				h("button", {
					disabled: B.value.length === 0 || T.value === "line",
					style: y({
						background: N.value ? J.value.style.buttons.controls.selected.backgroundColor : J.value.style.buttons.controls.backgroundColor,
						border: N.value ? J.value.style.buttons.controls.selected.border : J.value.style.buttons.controls.border,
						color: N.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						"button-tool--selected": N.value,
						tooltip: !0
					}),
					onClick: t[3] ||= (e) => {
						_n(), N.value = !N.value, M.value = !1, A.value = !1, j.value = !1, P.value = !1, F.value = !1, I.value = !1, T.value = void 0, H.value = !1;
					},
					onMouseenter: t[4] ||= (e) => J.value.style.showTooltips && Q("resize", e, "top"),
					onMouseleave: $,
					onFocus: t[5] ||= (e) => J.value.style.showTooltips && Q("resize", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "resize",
					stroke: N.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "resize",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipResize) + " ", 1), t[92] ||= h("kbd", null, "R", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 46, Se),
				h("button", {
					disabled: B.value.length === 0,
					style: y({
						background: A.value ? J.value.style.buttons.controls.selected.backgroundColor : J.value.style.buttons.controls.backgroundColor,
						border: A.value ? J.value.style.buttons.controls.selected.border : J.value.style.buttons.controls.border,
						color: A.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						"button-tool--selected": A.value,
						tooltip: !0
					}),
					onClick: t[6] ||= (e) => {
						_n(), A.value = !A.value, M.value = !1, N.value = !1, P.value = !1, F.value = !1, I.value = !1, T.value = void 0, H.value = !1;
					},
					onMouseenter: t[7] ||= (e) => J.value.style.showTooltips && Q("delete", e, "top"),
					onMouseleave: $,
					onFocus: t[8] ||= (e) => J.value.style.showTooltips && Q("delete", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "trash",
					stroke: A.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "delete",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipDelete) + " ", 1), t[93] ||= h("kbd", null, "D", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 46, Ce),
				h("button", {
					disabled: !Xt.value,
					style: y({
						background: P.value ? J.value.style.buttons.controls.selected.backgroundColor : J.value.style.buttons.controls.backgroundColor,
						border: P.value ? J.value.style.buttons.controls.selected.border : J.value.style.buttons.controls.border,
						color: P.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						"button-tool--selected": P.value,
						tooltip: !0
					}),
					onClick: t[9] ||= (e) => {
						_n(), Gn("group"), P.value = !P.value, A.value = !1, M.value = !1, N.value = !1, F.value = !1, I.value = !1, T.value = "group", H.value = !1;
					},
					onMouseenter: t[10] ||= (e) => J.value.style.showTooltips && Q("selectAndGroup", e, "top"),
					onMouseleave: $,
					onFocus: t[11] ||= (e) => J.value.style.showTooltips && Q("selectAndGroup", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "selectAndGroup",
					stroke: P.value ? J.value.style.buttons.controls.selected.color : J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "selectAndGroup",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipGroup) + " ", 1), t[94] ||= h("kbd", null, "G", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 46, we),
				h("button", {
					disabled: B.value.length === 0,
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						tooltip: !0
					}),
					onClick: t[12] ||= (e) => {
						N.value = !1, M.value = !0, A.value = !1, j.value = !1, P.value = !1, F.value = !1, I.value = !1, H.value = !1, mn("front");
					},
					onMouseenter: t[13] ||= (e) => J.value.style.showTooltips && Q("bringToFront", e, "top"),
					onMouseleave: $,
					onFocus: t[14] ||= (e) => J.value.style.showTooltips && Q("bringToFront", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "bringToFront",
					stroke: J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "bringToFront",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipBringToFront), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 44, Te),
				h("button", {
					disabled: B.value.length === 0,
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						tooltip: !0
					}),
					onClick: t[15] ||= (e) => {
						N.value = !1, M.value = !0, A.value = !1, j.value = !1, P.value = !1, F.value = !1, I.value = !1, H.value = !1, mn("back");
					},
					onMouseenter: t[16] ||= (e) => J.value.style.showTooltips && Q("bringToBack", e, "top"),
					onMouseleave: $,
					onFocus: t[17] ||= (e) => J.value.style.showTooltips && Q("bringToBack", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "bringToBack",
					stroke: J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "bringToBack",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipBringToBack), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 44, Ee),
				h("button", {
					disabled: B.value.length === 0 || T.value === "line",
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						tooltip: !0
					}),
					onClick: t[18] ||= (e) => {
						_n(), N.value = !1, M.value = !0, A.value = !1, j.value = !1, P.value = !1, F.value = !1, I.value = !1, H.value = !1, hn();
					},
					onMouseenter: t[19] ||= (e) => J.value.style.showTooltips && Q("duplicate", e, "top"),
					onMouseleave: $,
					onFocus: t[20] ||= (e) => J.value.style.showTooltips && Q("duplicate", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "copy",
					stroke: J.value.style.buttons.controls.color,
					size: 18
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "duplicate",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipDuplicate), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 44, De),
				h("button", {
					disabled: qt.value.undo === 0,
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						"button-tool--one-shot": !0,
						tooltip: !0
					}),
					onClick: t[21] ||= (e) => {
						N.value = !1, M.value = !1, A.value = !1, j.value = !1, P.value = !1, F.value = !1, I.value = !1, T.value = void 0, H.value = !1, bn();
					},
					onMouseenter: t[22] ||= (e) => J.value.style.showTooltips && Q("undoLast", e, "top"),
					onMouseleave: $,
					onFocus: t[23] ||= (e) => J.value.style.showTooltips && Q("undoLast", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "refresh",
					stroke: J.value.style.buttons.controls.color,
					size: 20
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "undoLast",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [
						g(S(J.value.translations.tooltipUndo) + " ", 1),
						h("kbd", null, S(Gt.value ? "⌘" : "Ctrl"), 1),
						t[95] ||= h("kbd", null, "Z", -1)
					]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 44, Oe),
				h("button", {
					disabled: qt.value.redo === 0,
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						"button-tool--one-shot": !0,
						tooltip: !0
					}),
					onClick: t[24] ||= (e) => {
						N.value = !1, M.value = !1, A.value = !1, j.value = !1, P.value = !1, F.value = !1, I.value = !1, T.value = void 0, H.value = !1, xn();
					},
					onMouseenter: t[25] ||= (e) => J.value.style.showTooltips && Q("redoLast", e, "top"),
					onMouseleave: $,
					onFocus: t[26] ||= (e) => J.value.style.showTooltips && Q("redoLast", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "refresh",
					stroke: J.value.style.buttons.controls.color,
					size: 20,
					style: { transform: "rotateX(0deg) rotateY(180deg)" }
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "redoLast",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [
						g(S(J.value.translations.tooltipRedo) + " ", 1),
						h("kbd", null, S(Gt.value ? "⌘" : "Ctrl"), 1),
						t[96] ||= h("kbd", null, "Y", -1)
					]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 44, ke),
				J.value.style.showPrint ? (b(), m("button", {
					key: 0,
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						tooltip: !0
					}),
					onClick: In,
					onMouseenter: t[27] ||= (e) => J.value.style.showTooltips && Q("printPdf", e, "top"),
					onMouseleave: $,
					onFocus: t[28] ||= (e) => J.value.style.showTooltips && Q("printPdf", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "printer",
					stroke: J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "printPdf",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipPdf), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 36)) : p("", !0),
				J.value.style.showImage ? (b(), m("button", {
					key: 1,
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						tooltip: !0
					}),
					onClick: t[29] ||= (...e) => ue(bt) && ue(bt)(...e),
					onMouseenter: t[30] ||= (e) => J.value.style.showTooltips && Q("printImage", e, "top"),
					onMouseleave: $,
					onFocus: t[31] ||= (e) => J.value.style.showTooltips && Q("printImage", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "image",
					stroke: J.value.style.buttons.controls.color,
					size: 20
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "printImage",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipImage), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 36)) : p("", !0),
				J.value.style.showSave ? (b(), m("button", {
					key: 2,
					style: y({
						background: J.value.style.buttons.controls.backgroundColor,
						border: J.value.style.buttons.controls.border,
						color: J.value.style.buttons.controls.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					class: v({
						"button-tool": !0,
						tooltip: !0
					}),
					onClick: qn,
					onMouseenter: t[32] ||= (e) => J.value.style.showTooltips && Q("saveAction", e, "top"),
					onMouseleave: $,
					onFocus: t[33] ||= (e) => J.value.style.showTooltips && Q("saveAction", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "save",
					stroke: J.value.style.buttons.controls.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "saveAction",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipSave), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 36)) : p("", !0)
			], 4), h("div", Ae, [
				h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": T.value === "circle",
						tooltip: !0
					}),
					style: y({
						background: T.value === "circle" ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: T.value === "circle" ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: T.value === "circle" ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[34] ||= (e) => {
						Gn("circle"), P.value = !1;
					},
					onMouseenter: t[35] ||= (e) => J.value.style.showTooltips && Q("setCircle", e, "top"),
					onMouseleave: $,
					onFocus: t[36] ||= (e) => J.value.style.showTooltips && Q("setCircle", e, "top"),
					onBlur: $
				}, [(b(), m("svg", je, [h("circle", {
					cx: 6,
					cy: 6,
					r: "4",
					fill: V.value.circle.filled ? (T.value, Ft.value + Zt.value) : "none",
					stroke: "currentColor"
				}, null, 8, Me)])), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setCircle",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeCircle) + " ", 1), t[97] ||= h("kbd", null, "C", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38),
				T.value === "circle" ? (b(), m("div", Ne, [h("label", Pe, [g(S(J.value.translations.filled) + " ", 1), me(h("input", {
					type: "checkbox",
					"onUpdate:modelValue": t[37] ||= (e) => V.value.circle.filled = e,
					onChange: zn,
					checked: V.value.circle.filled,
					style: y({
						all: "revert",
						appearance: "auto",
						"-webkit-appearance": "auto",
						accentColor: J.value.style.color + " !important"
					})
				}, null, 44, Fe), [[de, V.value.circle.filled]])])])) : p("", !0),
				h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": T.value === "rect",
						tooltip: !0
					}),
					style: y({
						background: T.value === "rect" ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: T.value === "rect" ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: T.value === "rect" ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[38] ||= (e) => {
						Gn("rect"), P.value = !1;
					},
					onMouseenter: t[39] ||= (e) => J.value.style.showTooltips && Q("setRect", e, "top"),
					onMouseleave: $,
					onFocus: t[40] ||= (e) => J.value.style.showTooltips && Q("setRect", e, "top"),
					onBlur: $
				}, [(b(), m("svg", Ie, [h("rect", {
					x: "3",
					y: "3",
					style: {
						rx: "0 !important",
						ry: "0 !important"
					},
					height: "6",
					width: "6",
					fill: V.value.rect.filled ? (T.value, Ft.value + Zt.value) : "none",
					stroke: "currentColor"
				}, null, 8, Le)])), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setRect",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeRect) + " ", 1), t[98] ||= h("kbd", null, "S", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38),
				T.value === "rect" ? (b(), m("div", Re, [h("label", ze, [g(S(J.value.translations.filled) + " ", 1), me(h("input", {
					type: "checkbox",
					"onUpdate:modelValue": t[41] ||= (e) => V.value.rect.filled = e,
					onChange: Rn,
					checked: V.value.rect.filled,
					style: y({
						all: "revert",
						appearance: "auto",
						"-webkit-appearance": "auto",
						accentColor: J.value.style.color + " !important"
					})
				}, null, 44, Be), [[de, V.value.rect.filled]])])])) : p("", !0),
				h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": T.value === "arrow",
						tooltip: !0
					}),
					style: y({
						background: T.value === "arrow" ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: T.value === "arrow" ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: T.value === "arrow" ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[42] ||= (e) => {
						Gn("arrow"), P.value = !1;
					},
					onMouseenter: t[43] ||= (e) => J.value.style.showTooltips && Q("setArrow", e, "top"),
					onMouseleave: $,
					onFocus: t[44] ||= (e) => J.value.style.showTooltips && Q("setArrow", e, "top"),
					onBlur: $
				}, [(b(), m("svg", Ve, [h("path", {
					stroke: V.value.arrow.filled ? T.value === "arrow" ? "white" : "grey" : "none",
					"stroke-width": "2",
					d: "M5,19 19,5 14,5 19,10.5 19,5",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, null, 8, He)])), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setArrow",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeArrow) + " ", 1), t[99] ||= h("kbd", null, "A", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38),
				h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": T.value === "line",
						tooltip: !0
					}),
					style: y({
						background: T.value === "line" ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: T.value === "line" ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: T.value === "line" ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[45] ||= (e) => {
						Gn("line"), P.value = !1;
					},
					onMouseenter: t[46] ||= (e) => J.value.style.showTooltips && Q("setFreehand", e, "top"),
					onMouseleave: $,
					onFocus: t[47] ||= (e) => J.value.style.showTooltips && Q("setFreehand", e, "top"),
					onBlur: $
				}, [t[101] ||= h("svg", {
					width: "80%",
					viewBox: "0 0 24 24",
					"stroke-width": "2",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [h("path", {
					stroke: "none",
					d: "M0 0h24v24H0z",
					fill: "none"
				}), h("path", { d: "M3 15c2 3 4 4 7 4s7 -3 7 -7s-3 -7 -6 -7s-5 1.5 -5 4s2 5 6 5s8.408 -2.453 10 -5" })], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setFreehand",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeFreehand) + " ", 1), t[100] ||= h("kbd", null, "L", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38),
				[
					"arrow",
					"circle",
					"rect",
					"line"
				].includes(T.value) ? (b(), m("div", Ue, [h("div", We, [h("label", Ge, [g(S(J.value.translations.thickness) + " ", 1), me(h("input", {
					type: "number",
					"onUpdate:modelValue": t[48] ||= (e) => xt.value = e,
					onInput: Hn,
					min: 1,
					style: {
						padding: "0 4px",
						width: "40px",
						border: "1px solid #dadada",
						"border-radius": "3px"
					}
				}, null, 544), [[fe, xt.value]])])])])) : p("", !0),
				[
					"arrow",
					"circle",
					"rect"
				].includes(T.value) ? (b(), m("div", Ke, [h("div", qe, [h("label", Je, [
					g(S(J.value.translations.dashedLines) + " ", 1),
					(b(), m("svg", Ye, [...t[102] ||= [h("line", {
						x1: "0",
						x2: "24",
						y1: "12",
						y2: "12",
						"stroke-width": "2",
						stroke: "black",
						"stroke-dasharray": "3"
					}, null, -1)]])),
					me(h("input", {
						name: "dashStyle",
						type: "checkbox",
						"onUpdate:modelValue": t[49] ||= (e) => Ct.value = e,
						onChange: Bn,
						checked: Ct.value,
						style: y({
							all: "revert",
							appearance: "auto",
							"-webkit-appearance": "auto",
							accentColor: J.value.style.color + " !important"
						})
					}, null, 44, Xe), [[de, Ct.value]])
				])])])) : p("", !0),
				h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": F.value,
						tooltip: !0
					}),
					style: y({
						background: F.value ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: F.value ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: F.value ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[50] ||= (e) => {
						_n(), F.value = !F.value, A.value = !1, M.value = !1, N.value = !1, P.value = !1, j.value = !1, T.value = void 0;
					},
					onMouseenter: t[51] ||= (e) => J.value.style.showTooltips && Q("setText", e, "top"),
					onMouseleave: $,
					onFocus: t[52] ||= (e) => J.value.style.showTooltips && Q("setText", e, "top"),
					onBlur: $
				}, [_(u, {
					name: "text",
					stroke: F.value ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color
				}, null, 8, ["stroke"]), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setText",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeText) + " ", 1), t[103] ||= h("kbd", null, "T", -1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38),
				F.value ? (b(), m("div", Ze, [h("div", Qe, [h("label", $e, [g(S(J.value.translations.fontSize) + " ", 1), me(h("input", {
					type: "number",
					"onUpdate:modelValue": t[53] ||= (e) => Bt.value = e,
					onInput: Un,
					style: {
						padding: "0 4px",
						width: "40px",
						border: "1px solid #dadada",
						"border-radius": "3px"
					}
				}, null, 544), [[fe, Bt.value]])])])])) : p("", !0),
				F.value ? (b(), m("div", et, [h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": U.value === "start",
						tooltip: !0
					}),
					style: y({
						background: U.value === "start" ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: U.value === "start" ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: U.value === "start" ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[54] ||= (e) => {
						A.value = !1, M.value = !1, N.value = !1, j.value = !1, P.value = !1, T.value = void 0, U.value = "start", yn("start");
					},
					onMouseenter: t[55] ||= (e) => J.value.style.showTooltips && Q("setAlignStart", e, "top"),
					onMouseleave: $,
					onFocus: t[56] ||= (e) => J.value.style.showTooltips && Q("setAlignStart", e, "top"),
					onBlur: $
				}, [t[104] ||= h("svg", {
					width: "80%",
					viewBox: "0 0 24 24",
					"stroke-width": "2",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [
					h("path", {
						stroke: "none",
						d: "M0 0h24v24H0z",
						fill: "none"
					}),
					h("path", { d: "M4 6l16 0" }),
					h("path", { d: "M4 12l10 0" }),
					h("path", { d: "M4 18l14 0" })
				], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setAlignStart",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeTextLeft), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38)])) : p("", !0),
				F.value ? (b(), m("div", tt, [h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": U.value === "middle",
						tooltip: !0
					}),
					style: y({
						background: U.value === "middle" ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: U.value === "middle" ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: U.value === "middle" ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					disabled: k.value,
					onClick: t[57] ||= (e) => {
						A.value = !1, M.value = !1, N.value = !1, j.value = !1, P.value = !1, T.value = void 0, U.value = "middle", yn("middle");
					},
					onMouseenter: t[58] ||= (e) => J.value.style.showTooltips && Q("setAlignMiddle", e, "top"),
					onMouseleave: $,
					onFocus: t[59] ||= (e) => J.value.style.showTooltips && Q("setAlignMiddle", e, "top"),
					onBlur: $
				}, [t[105] ||= h("svg", {
					width: "80%",
					viewBox: "0 0 24 24",
					"stroke-width": "2",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [
					h("path", {
						stroke: "none",
						d: "M0 0h24v24H0z",
						fill: "none"
					}),
					h("path", { d: "M4 6l16 0" }),
					h("path", { d: "M8 12l8 0" }),
					h("path", { d: "M6 18l12 0" })
				], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setAlignMiddle",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeTextCenter), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 46, nt)])) : p("", !0),
				F.value ? (b(), m("div", rt, [h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": U.value === "end",
						tooltip: !0
					}),
					style: y({
						background: U.value === "end" ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: U.value === "end" ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: U.value === "end" ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					disabled: k.value,
					onClick: t[60] ||= (e) => {
						A.value = !1, M.value = !1, N.value = !1, j.value = !1, P.value = !1, T.value = void 0, U.value = "end", yn("end");
					},
					onMouseenter: t[61] ||= (e) => J.value.style.showTooltips && Q("setAlignEnd", e, "top"),
					onMouseleave: $,
					onFocus: t[62] ||= (e) => J.value.style.showTooltips && Q("setAlignEnd", e, "top"),
					onBlur: $
				}, [t[106] ||= h("svg", {
					width: "80%",
					viewBox: "0 0 24 24",
					"stroke-width": "2",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [
					h("path", {
						stroke: "none",
						d: "M0 0h24v24H0z",
						fill: "none"
					}),
					h("path", { d: "M4 6l16 0" }),
					h("path", { d: "M10 12l10 0" }),
					h("path", { d: "M6 18l14 0" })
				], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setAlignEnd",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeTextRight), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 46, it)])) : p("", !0),
				F.value ? (b(), m("div", at, [h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": k.value,
						tooltip: !0
					}),
					style: y({
						background: k.value ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: k.value ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: k.value ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[63] ||= (e) => {
						A.value = !1, M.value = !1, N.value = !1, j.value = !1, P.value = !1, T.value = void 0, k.value = !k.value, U.value = "start", yn("start"), Un();
					},
					onMouseenter: t[64] ||= (e) => J.value.style.showTooltips && Q("setBulletMode", e, "top"),
					onMouseleave: $,
					onFocus: t[65] ||= (e) => J.value.style.showTooltips && Q("setBulletMode", e, "top"),
					onBlur: $
				}, [t[107] ||= h("svg", {
					width: "100%",
					viewBox: "0 0 24 24",
					"stroke-width": "2",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [
					h("path", {
						stroke: "none",
						d: "M0 0h24v24H0z",
						fill: "none"
					}),
					h("path", { d: "M9 6l11 0" }),
					h("path", { d: "M9 12l11 0" }),
					h("path", { d: "M9 18l11 0" }),
					h("path", { d: "M5 6l0 .01" }),
					h("path", { d: "M5 12l0 .01" }),
					h("path", { d: "M5 18l0 .01" })
				], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setBulletMode",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeTextBullet), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38)])) : p("", !0),
				F.value ? (b(), m("div", ot, [h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": St.value,
						tooltip: !0
					}),
					style: y({
						background: St.value ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: St.value ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: St.value ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[66] ||= (e) => {
						A.value = !1, M.value = !1, N.value = !1, j.value = !1, P.value = !1, T.value = void 0, St.value = !St.value, Un();
					},
					onMouseenter: t[67] ||= (e) => J.value.style.showTooltips && Q("setBold", e, "top"),
					onMouseleave: $,
					onFocus: t[68] ||= (e) => J.value.style.showTooltips && Q("setBold", e, "top"),
					onBlur: $
				}, [t[108] ||= h("svg", {
					width: "100%",
					viewBox: "0 0 24 24",
					"stroke-width": "3",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [
					h("path", {
						stroke: "none",
						d: "M0 0h24v24H0z",
						fill: "none"
					}),
					h("path", { d: "M7 5h6a3.5 3.5 0 0 1 0 7h-6z" }),
					h("path", { d: "M13 12h1a3.5 3.5 0 0 1 0 7h-7v-7" })
				], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setBold",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeTextBold), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38)])) : p("", !0),
				F.value ? (b(), m("div", st, [h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": Et.value,
						tooltip: !0
					}),
					style: y({
						background: Et.value ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: Et.value ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: Et.value ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[69] ||= (e) => {
						A.value = !1, M.value = !1, N.value = !1, j.value = !1, P.value = !1, T.value = void 0, Et.value = !Et.value, Un();
					},
					onMouseenter: t[70] ||= (e) => J.value.style.showTooltips && Q("setItalic", e, "top"),
					onMouseleave: $,
					onFocus: t[71] ||= (e) => J.value.style.showTooltips && Q("setItalic", e, "top"),
					onBlur: $
				}, [t[109] ||= h("svg", {
					width: "100%",
					height: "44",
					viewBox: "0 0 24 24",
					"stroke-width": "2",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [
					h("path", {
						stroke: "none",
						d: "M0 0h24v24H0z",
						fill: "none"
					}),
					h("path", { d: "M11 5l6 0" }),
					h("path", { d: "M7 19l6 0" }),
					h("path", { d: "M14 5l-4 14" })
				], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setItalic",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeTextItalic), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38)])) : p("", !0),
				F.value ? (b(), m("div", ct, [h("button", {
					class: v({
						"button-tool": !0,
						"button-tool--selected": At.value,
						tooltip: !0
					}),
					style: y({
						background: At.value ? J.value.style.buttons.shapes.selected.backgroundColor : J.value.style.buttons.shapes.backgroundColor,
						border: At.value ? J.value.style.buttons.shapes.selected.border : J.value.style.buttons.shapes.border,
						color: At.value ? J.value.style.buttons.shapes.selected.color : J.value.style.buttons.shapes.color,
						borderRadius: `${J.value.style.buttons.borderRadius}px`,
						cursor: Y.value ? "pointer" : "default"
					}),
					onClick: t[72] ||= (e) => {
						A.value = !1, M.value = !1, N.value = !1, j.value = !1, P.value = !1, T.value = void 0, At.value = !At.value, Un();
					},
					onMouseenter: t[73] ||= (e) => J.value.style.showTooltips && Q("setUnderline", e, "top"),
					onMouseleave: $,
					onFocus: t[74] ||= (e) => J.value.style.showTooltips && Q("setUnderline", e, "top"),
					onBlur: $
				}, [t[110] ||= h("svg", {
					width: "100%",
					viewBox: "0 0 24 24",
					"stroke-width": "2",
					stroke: "currentColor",
					fill: "none",
					"stroke-linecap": "round",
					"stroke-linejoin": "round"
				}, [
					h("path", {
						stroke: "none",
						d: "M0 0h24v24H0z",
						fill: "none"
					}),
					h("path", { d: "M7 5v5a5 5 0 0 0 10 0v-5" }),
					h("path", { d: "M5 19h14" })
				], -1), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setUnderline",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeTextUnderline), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)], 38)])) : p("", !0),
				h("div", lt, [h("button", {
					class: v({
						"button-tool": !0,
						tooltip: !0
					}),
					style: { borderRadius: "6px" },
					onMouseenter: t[76] ||= (e) => J.value.style.showTooltips && Q("setColor", e, "top"),
					onMouseleave: $,
					onFocus: t[77] ||= (e) => J.value.style.showTooltips && Q("setColor", e, "top"),
					onBlur: $
				}, [_(ne, {
					value: Ft.value,
					"onUpdate:value": t[75] ||= (e) => Ft.value = e,
					backgroundColor: J.value.style.backgroundColor,
					buttonBorderColor: J.value.style.color,
					isCursorPointer: Y.value,
					teleported: ""
				}, null, 8, [
					"value",
					"backgroundColor",
					"buttonBorderColor",
					"isCursorPointer"
				])], 32), J.value.style.showTooltips ? (b(), f(w, {
					key: 0,
					show: W.value && G.value === "setColor",
					x: K.value.x,
					y: K.value.y - 6,
					placement: "top",
					styleObject: X.value
				}, {
					default: C(() => [g(S(J.value.translations.tooltipShapeColor), 1)]),
					_: 1
				}, 8, [
					"show",
					"x",
					"y",
					"styleObject"
				])) : p("", !0)]),
				h("div", ut, [h("label", dt, [g(S(J.value.translations.colorAlpha) + ": " + S(Vt.value > 98 ? 100 : Vt.value) + " % ", 1), me(h("input", {
					name: "colorTransparency",
					type: "range",
					"onUpdate:modelValue": t[78] ||= (e) => Vt.value = e,
					onInput: Vn,
					min: 0,
					max: 100,
					style: y({
						width: "100%",
						accentColor: J.value.style.color + " !important"
					})
				}, null, 36), [[fe, Vt.value]])])])
			])]),
			_: 1
		}, 8, ["config"])]), h("div", {
			class: "annotator annotator__wrapper",
			ref_key: "drawSvgContainer",
			ref: Pn,
			style: { position: "relative" },
			id: ve.value,
			"data-annotator-content": ""
		}, [
			h("div", {
				class: "annotator__content-layer",
				style: y(`${kt.value ? "pointer-events: none;" : ""}`)
			}, [le(e.$slots, "default", {}, void 0, !0)], 4),
			kt.value || J.value.alwaysVisible ? (b(), m("svg", {
				id: "annotatorSvg",
				key: Mt.value,
				ref_key: "mainSvg",
				ref: en,
				class: v({
					annotator__overlay: !0,
					draw: !0,
					"draw--free": T.value === "line"
				}),
				viewBox: `0 0 ${Pt.value} ${Nt.value}`,
				width: Rt.value,
				height: zt.value,
				onPointerdown: t[83] ||= (e) => On(e),
				onPointerup: t[84] ||= (e) => Ln(e),
				onTouchend: t[85] ||= (e) => Ln(e),
				onTouchstart: t[86] ||= (e) => {
					Wn(e), vn(e);
				},
				onPointermove: t[87] ||= (e) => {
					Wn(e), Mn(e);
				},
				onPointerout: t[88] ||= (e) => En(e),
				onPointerover: t[89] ||= (e) => gn(e),
				onClick: t[90] ||= (e) => vn(e),
				style: y({
					position: "absolute",
					top: 0,
					left: 0,
					cursor: Qt.value,
					fontFamily: "Helvetica",
					zIndex: 1e8,
					pointerEvents: kt.value ? "all" : "none"
				})
			}, [h("rect", {
				class: "annotator__glass",
				x: "0",
				y: "0",
				width: Pt.value,
				height: Nt.value,
				fill: "transparent",
				"pointer-events": kt.value ? "all" : "none",
				style: { cursor: "inherit" },
				onPointerdown: t[79] ||= he((e) => On(e), ["stop", "prevent"]),
				onPointermove: t[80] ||= he((e) => {
					Wn(e), Mn(e);
				}, ["stop", "prevent"]),
				onPointerup: he(Ln, ["stop", "prevent"]),
				onClick: t[81] ||= he(() => {}, ["stop", "prevent"])
			}, null, 40, mt), (b(!0), m(re, null, ce(on.value, (e) => (b(), m("g", {
				key: e.id,
				innerHTML: e.html,
				onClick: t[82] ||= (e) => {
					Nn(e), M.value = !1;
				}
			}, null, 8, ht))), 128))], 46, pt)) : p("", !0),
			Ot.value || ue(yt) ? (b(), m("svg", {
				key: 1,
				style: {
					position: "absolute",
					top: "0",
					left: "0"
				},
				height: zt.value,
				viewBox: `0 0 ${Pt.value} ${Nt.value}`,
				width: Rt.value,
				"data-dom-to-png-ignore": ""
			}, [h("circle", {
				class: "animated-circle-print",
				cx: Pt.value / 2,
				cy: Nt.value / 2,
				r: "50",
				stroke: "#6376DD",
				"stroke-width": "10",
				fill: "none"
			}, null, 8, _t)], 8, gt)) : p("", !0)
		], 8, ft)]));
	}
}, [["__scopeId", "data-v-16880eff"]]);
//#endregion
export { ve as n, yt as t };
