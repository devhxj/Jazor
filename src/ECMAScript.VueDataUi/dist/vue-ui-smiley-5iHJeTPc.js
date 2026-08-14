import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Vt as t, X as n, i as r, jt as i, t as a, tt as o } from "./lib-Bttd6u5E.js";
import { n as s, t as c } from "./useHints-Dq_w2E8B.js";
import { t as l } from "./useConfig-DlNpz6P8.js";
import { t as u } from "./useNestedProp-vPNvh7rV.js";
import { t as d } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as f } from "./DefGrad-DVBqDjhO.js";
import { Fragment as p, computed as m, createCommentVNode as h, createElementBlock as g, createElementVNode as _, createTextVNode as v, createVNode as y, normalizeClass as b, normalizeStyle as x, onMounted as S, openBlock as C, ref as w, renderList as T, renderSlot as E, toDisplayString as D, unref as O, useSlots as k, watch as A, withCtx as j, withKeys as M } from "vue";
//#region src/atoms/BaseSmilingUnit.vue
var N = ["xmlns", "stroke"], P = ["stroke"], F = [
	"xmlns",
	"viewBox",
	"stroke"
], I = ["viewBox", "stroke"], L = /*#__PURE__*/ d({
	__name: "BaseSmilingUnit",
	props: {
		config: { type: Object },
		unit: { type: Number },
		currentRating: { type: Number },
		getActiveColor: { type: Function },
		calcShapeFill: { type: Function },
		isReadonly: { type: Boolean },
		hasBreakdown: { type: Boolean },
		hoveredValue: { type: Number },
		isCursorPointer: { type: Boolean }
	},
	emits: [
		"rate",
		"mouseenter",
		"mouseleave"
	],
	setup(e, { emit: n }) {
		let r = e, i = n;
		return (n, o) => (C(), g("div", {
			tabindex: "0",
			class: b({ "vue-ui-smiley-rated": !e.config.readonly && e.currentRating === e.unit }),
			style: x({
				cursor: e.config.readonly ? "default" : r.isCursorPointer ? "pointer" : "default",
				height: e.config.style.itemSize + "px",
				aspectRatio: "1/1",
				position: "relative"
			}),
			onMouseenter: o[0] ||= (e) => i("mouseenter"),
			onMouseleave: o[1] ||= (e) => i("mouseleave"),
			onClick: o[2] ||= (t) => i("rate", e.unit),
			onKeyup: o[3] ||= M((t) => i("rate", e.unit), ["enter"])
		}, [
			e.config.style.tooltip.show && e.hasBreakdown && e.isReadonly ? (C(), g("div", {
				key: 0,
				class: "vue-ui-rating-tooltip",
				style: x({
					border: `1px solid ${e.config.style.tooltip.borderColor}`,
					position: "absolute",
					top: `${e.config.style.tooltip.offsetY - 48}px`,
					left: "50%",
					transform: "translateX(-50%)",
					width: "fit-content",
					textAlign: "center",
					background: e.config.style.tooltip.backgroundColor,
					display: e.hoveredValue === e.unit - 1 ? "block" : "none",
					padding: "2px 12px",
					borderRadius: e.config.style.tooltip.borderRadius + "px",
					boxShadow: e.config.style.tooltip.boxShadow
				})
			}, [_("div", { style: x({
				width: "100%",
				display: "flex",
				flexDirection: "row",
				gap: "6px",
				position: "relative",
				textAlign: "center",
				color: e.config.style.tooltip.color
			}) }, [
				_("span", { style: x(`font-size:${e.config.style.tooltip.fontSize}px`) }, D(e.unit) + ":", 5),
				_("span", { style: x(`font-weight:${e.config.style.tooltip.bold ? "bold" : "normal"};font-size:${e.config.style.tooltip.fontSize}px`) }, [E(n.$slots, "rating", {}, void 0, !0)], 4),
				_("div", { style: x(`font-family:Arial !important;position:absolute;top:calc(100% - 4px);left:50%;transform:translateX(-50%);color:${e.config.style.tooltip.borderColor}`) }, " ▼ ", 4)
			], 4)], 4)) : h("", !0),
			e.config.style.icons.filled ? (C(), g("svg", {
				key: 1,
				xmlns: O(a),
				style: {
					transition: "all 0.1s ease-in-out",
					position: "absolute",
					top: "0",
					left: "0"
				},
				height: "100%",
				viewBox: "0 0 24 24",
				"stroke-width": "1.5",
				stroke: e.getActiveColor(e.unit - 1),
				fill: "none",
				"stroke-linecap": "round",
				"stroke-linejoin": "round"
			}, [_("defs", null, [y(f, {
				t: "radial",
				id: `vueUiSmiley${e.unit - 1}`,
				stops: [[
					"0%",
					O(t)(e.config.style.colors.active[e.unit - 1], .05),
					1
				], [
					"100%",
					e.config.style.colors.active[e.unit - 1],
					1
				]]
			}, null, 8, ["id", "stops"])]), E(n.$slots, "path-icon-filled", {}, void 0, !0)], 8, N)) : (C(), g("svg", {
				key: 2,
				style: {
					position: "absolute",
					top: "0",
					left: "0",
					transition: "all 0.1s ease-in-out"
				},
				height: "100%",
				viewBox: "0 0 24 24",
				"stroke-width": "1.5",
				stroke: e.getActiveColor(e.unit - 1),
				fill: "none",
				"stroke-linecap": "round",
				"stroke-linejoin": "round"
			}, [E(n.$slots, "path-icon", {}, void 0, !0)], 8, P)),
			e.config.style.icons.filled && e.isReadonly ? (C(), g("svg", {
				key: 3,
				xmlns: O(a),
				style: {
					transition: "all 0.1s ease-in-out",
					position: "absolute",
					top: "0",
					left: "0"
				},
				height: "100%",
				viewBox: `0 0 ${e.calcShapeFill(e.unit - 1)} 24`,
				"stroke-width": "1.5",
				stroke: e.config.style.colors.activeReadonly[e.unit - 1],
				fill: "none",
				"stroke-linecap": "round",
				"stroke-linejoin": "round"
			}, [_("defs", null, [y(f, {
				t: "radial",
				id: `vueUiSmiley${e.unit - 1}`,
				stops: [[
					"0%",
					O(t)(e.config.style.colors.activeReadonly[e.unit - 1], .05),
					1
				], [
					"100%",
					e.config.style.colors.activeReadonly[e.unit - 1],
					1
				]]
			}, null, 8, ["id", "stops"])]), E(n.$slots, "path-icon-filled-readonly", {}, void 0, !0)], 8, F)) : h("", !0),
			!e.config.style.icons.filled && e.isReadonly ? (C(), g("svg", {
				key: 4,
				style: {
					position: "absolute",
					top: "0",
					left: "0",
					transition: "all 0.1s ease-in-out"
				},
				height: "100%",
				viewBox: `0 0 ${e.calcShapeFill(e.unit - 1)} 24`,
				"stroke-width": "1.5",
				stroke: e.config.style.colors.activeReadonly[e.unit - 1],
				fill: "none",
				"stroke-linecap": "round",
				"stroke-linejoin": "round"
			}, [E(n.$slots, "path-icon-readonly", {}, void 0, !0)], 8, I)) : h("", !0)
		], 38));
	}
}, [["__scopeId", "data-v-9734e83c"]]), R = /* @__PURE__ */ e({ default: () => W }), z = {
	key: 0,
	class: "vue-ui-rating-title",
	style: { width: "100%" }
}, B = ["d", "fill"], V = ["d"], H = ["d", "fill"], U = ["d"], W = {
	__name: "vue-ui-smiley",
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
				return {};
			}
		}
	},
	emits: ["rate"],
	setup(e, { expose: t, emit: a }) {
		let { vue_ui_smiley: d } = l(), f = e, b = k(), E = a, O = w(null), M = w(I()), N = m(() => M.value.debug);
		S(() => {
			b["chart-background"] && N.value && console.warn("VueUiSmiley does not support the #chart-background slot.");
		});
		function P() {
			(!Object.hasOwn(f.dataset, "rating") || i(f.dataset)) && o({
				componentName: "VueUiSmiley",
				type: "datasetAttribute",
				property: "rating",
				debug: N.value
			});
		}
		S(P), s({
			config: () => M.value,
			dataset: () => [],
			component: "VueUiSmiley",
			rules: [c.noHint]
		});
		let F = m(() => M.value.useCursorPointer);
		function I() {
			return u({
				userConfig: f.config,
				defaultConfig: d
			});
		}
		A(() => f.config, (e) => {
			M.value = I(), K.value = M.value.readonly, P();
		}, { deep: !0 });
		let R = m(() => {
			let e = f.dataset.rating;
			return e && typeof e == "object" && !Array.isArray(e) ? J(e) : e ?? null;
		}), W = m(() => typeof f.dataset.rating == "object" && !Array.isArray(f.dataset.rating)), G = w(R.value);
		A(R, (e) => {
			G.value = e;
		});
		let K = w(M.value.readonly), q = [
			{
				key: "smiley_0",
				pathIconFilled: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-5 9.86a4.5 4.5 0 0 0 -3.214 1.35a1 1 0 1 0 1.428 1.4a2.5 2.5 0 0 1 3.572 0a1 1 0 0 0 1.428 -1.4a4.5 4.5 0 0 0 -3.214 -1.35zm-2.99 -4.2l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm6 0l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007z",
				pathIcon: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0 M9.5 15.25a3.5 3.5 0 0 1 5 0",
				pathIconFilledReadonly: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-5 9.86a4.5 4.5 0 0 0 -3.214 1.35a1 1 0 1 0 1.428 1.4a2.5 2.5 0 0 1 3.572 0a1 1 0 0 0 1.428 -1.4a4.5 4.5 0 0 0 -3.214 -1.35zm-2.99 -4.2l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm6 0l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007z",
				pathIconReadonly: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0 M9.5 15.25a3.5 3.5 0 0 1 5 0"
			},
			{
				key: "smiley_1",
				pathIconFilled: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-2 10.66h-6l-.117 .007a1 1 0 0 0 0 1.986l.117 .007h6l.117 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm-5.99 -5l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm6 0l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007z",
				pathIcon: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0 M9 15l6 0",
				pathIconFilledReadonly: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-2 10.66h-6l-.117 .007a1 1 0 0 0 0 1.986l.117 .007h6l.117 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm-5.99 -5l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm6 0l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007z",
				pathIconReadonly: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0 M9 15l6 0"
			},
			{
				key: "smiley_2",
				pathIconFilled: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-7.99 5.66l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm6 0l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007z",
				pathIcon: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0",
				pathIconFilledReadonly: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-7.99 5.66l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007zm6 0l-.127 .007a1 1 0 0 0 0 1.986l.117 .007l.127 -.007a1 1 0 0 0 0 -1.986l-.117 -.007z",
				pathIconReadonly: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0"
			},
			{
				key: "smiley_3",
				pathIconFilled: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-1.8 10.946a1 1 0 0 0 -1.414 .014a2.5 2.5 0 0 1 -3.572 0a1 1 0 0 0 -1.428 1.4a4.5 4.5 0 0 0 6.428 0a1 1 0 0 0 -.014 -1.414zm-6.19 -5.286l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993zm6 0l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993z",
				pathIcon: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0 M9.5 15a3.5 3.5 0 0 0 5 0",
				pathIconFilledReadonly: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-1.8 10.946a1 1 0 0 0 -1.414 .014a2.5 2.5 0 0 1 -3.572 0a1 1 0 0 0 -1.428 1.4a4.5 4.5 0 0 0 6.428 0a1 1 0 0 0 -.014 -1.414zm-6.19 -5.286l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993zm6 0l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993z",
				pathIconReadonly: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 10l.01 0 M15 10l.01 0 M9.5 15a3.5 3.5 0 0 0 5 0"
			},
			{
				key: "smiley_4",
				pathIconFilled: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-2 9.66h-6a1 1 0 0 0 -1 1v.05a3.975 3.975 0 0 0 3.777 3.97l.227 .005a4.026 4.026 0 0 0 3.99 -3.79l.006 -.206a1 1 0 0 0 -1 -1.029zm-5.99 -5l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993zm6 0l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993z",
				pathIcon: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 9l.01 0 M15 9l.01 0 M8 13a4 4 0 1 0 8 0h-8",
				pathIconFilledReadonly: "M17 3.34a10 10 0 1 1 -14.995 8.984l-.005 -.324l.005 -.324a10 10 0 0 1 14.995 -8.336zm-2 9.66h-6a1 1 0 0 0 -1 1v.05a3.975 3.975 0 0 0 3.777 3.97l.227 .005a4.026 4.026 0 0 0 3.99 -3.79l.006 -.206a1 1 0 0 0 -1 -1.029zm-5.99 -5l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993zm6 0l-.127 .007a1 1 0 0 0 .117 1.993l.127 -.007a1 1 0 0 0 -.117 -1.993z",
				pathIconReadonly: "M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0 M9 9l.01 0 M15 9l.01 0 M8 13a4 4 0 1 0 8 0h-8"
			}
		];
		function J(e) {
			if (e == null) return null;
			let t = 0, n = 0;
			for (let r in e) {
				let i = Number(r), a = e[r];
				Number.isFinite(i) && (t += i * a, n += a);
			}
			return n === 0 ? 0 : t / n;
		}
		function Y(e) {
			let t = G.value - e;
			switch (!0) {
				case t < 0: return 0;
				case t > 1: return 24;
				default: return t * 24;
			}
		}
		function X(e) {
			return M.value.readonly ? M.value.style.colors.inactive[e] : G.value === e + 1 || O.value !== null && O.value === e ? M.value.style.icons.useGradient && M.value.style.icons.filled ? `url(#vueUiSmiley${e})` : M.value.style.colors.active[e] : M.value.style.colors.inactive[e];
		}
		let Z = m(() => ({ value: e, tooltip: t = !1 }) => r(t ? M.value.style.tooltip.formatter : M.value.style.rating.formatter, e, n({
			v: e,
			r: t ? M.value.style.tooltip.roundingValue : M.value.style.rating.roundingValue
		}), M.value));
		function Q(e) {
			K.value || (G.value = e, E("rate", e));
		}
		function $() {
			return G.value;
		}
		function ee(e = !0) {
			K.value = e;
		}
		return t({
			getData: $,
			toggleReadonly: ee
		}), (e, t) => (C(), g("div", {
			class: "vue-data-ui-component vue-ui-smiley",
			style: x(`background:${M.value.style.backgroundColor};font-family:${M.value.style.fontFamily};width:100%;`),
			onMouseleave: t[1] ||= (e) => O.value = void 0
		}, [
			M.value.style.title.text ? (C(), g("div", z, [_("div", { style: x(`color:${M.value.style.title.color};font-weight:${M.value.style.title.bold ? "bold" : "normal"};text-align:${M.value.style.title.textAlign};margin-bottom:${M.value.style.title.offsetY}px;font-size:${M.value.style.title.fontSize}px`) }, D(M.value.style.title.text), 5), M.value.style.title.subtitle.text ? (C(), g("div", {
				key: 0,
				style: x(`color:${M.value.style.title.subtitle.color};font-size:${M.value.style.title.subtitle.fontSize}px;text-align:${M.value.style.title.textAlign};margin-bottom:${M.value.style.title.subtitle.offsetY}px;font-weight:${M.value.style.title.subtitle.bold ? "bold" : "normal"}`)
			}, D(M.value.style.title.subtitle.text), 5)) : h("", !0)])) : h("", !0),
			M.value.style.rating.show && M.value.style.rating.position === "top" ? (C(), g("div", {
				key: 1,
				style: x(`width:100%;text-align:center;margin-bottom:${M.value.style.rating.offsetY}px;font-size:${M.value.style.rating.fontSize}px;font-weight:${M.value.style.rating.bold ? "bold" : "normal"};margin-left:${M.value.style.rating.offsetX}px`)
			}, D(Z.value({ value: G.value })), 5)) : h("", !0),
			_("div", {
				class: "vue-ui-smiley-wrapper",
				style: x(`overflow:visible;height:${M.value.style.itemSize}px;width:fit-content;margin:0 auto;display:flex;align-items:center;justify-content:center;`)
			}, [
				M.value.style.rating.show && M.value.style.rating.position === "left" ? (C(), g("div", {
					key: 0,
					style: x(`width:fit-content;text-align:center;margin-bottom:${M.value.style.rating.offsetY}px;font-size:${M.value.style.rating.fontSize}px;font-weight:${M.value.style.rating.bold ? "bold" : "normal"};padding-right:${M.value.style.rating.offsetX}px`)
				}, D(Z.value({ value: G.value })), 5)) : h("", !0),
				(C(), g(p, null, T(q, (e, n) => y(L, {
					key: e.key,
					config: M.value,
					unit: n + 1,
					currentRating: G.value,
					isReadonly: K.value,
					hasBreakdown: W.value,
					hoveredValue: O.value,
					getActiveColor: X,
					calcShapeFill: Y,
					isCursorPointer: F.value,
					onMouseenter: (e) => O.value = n,
					onMouseleave: t[0] ||= (e) => O.value = null,
					onRate: (e) => Q(n + 1)
				}, {
					rating: j(() => [v(D(Z.value({
						value: f.dataset.rating[String(n + 1)],
						tooltip: !0
					})), 1)]),
					"path-icon-filled": j(() => [_("path", {
						d: e.pathIconFilled,
						"stroke-width": "0",
						fill: X(n)
					}, null, 8, B)]),
					"path-icon": j(() => [_("path", { d: e.pathIcon }, null, 8, V)]),
					"path-icon-filled-readonly": j(() => [_("path", {
						d: e.pathIconFilledReadonly,
						"stroke-width": "0",
						fill: M.value.style.icons.useGradient ? `url(#vueUiSmiley${n})` : M.value.style.colors.activeReadonly[n]
					}, null, 8, H)]),
					"path-icon-readonly": j(() => [_("path", { d: e.pathIconReadonly }, null, 8, U)]),
					_: 2
				}, 1032, [
					"config",
					"unit",
					"currentRating",
					"isReadonly",
					"hasBreakdown",
					"hoveredValue",
					"isCursorPointer",
					"onMouseenter",
					"onRate"
				])), 64)),
				M.value.style.rating.show && M.value.style.rating.position === "right" ? (C(), g("div", {
					key: 1,
					style: x(`width:fit-content;text-align:center;margin-bottom:${M.value.style.rating.offsetY}px;font-size:${M.value.style.rating.fontSize}px;font-weight:${M.value.style.rating.bold ? "bold" : "normal"};padding-left:${M.value.style.rating.offsetX}px`)
				}, D(Z.value({ value: G.value })), 5)) : h("", !0)
			], 4),
			M.value.style.rating.show && M.value.style.rating.position === "bottom" ? (C(), g("div", {
				key: 2,
				style: x(`width:100%;text-align:center;margin-top:${M.value.style.rating.offsetY}px;font-size:${M.value.style.rating.fontSize}px;font-weight:${M.value.style.rating.bold ? "bold" : "normal"};margin-left:${M.value.style.rating.offsetX}px`)
			}, D(Z.value({ value: G.value })), 5)) : h("", !0)
		], 36));
	}
};
//#endregion
export { R as n, W as t };
