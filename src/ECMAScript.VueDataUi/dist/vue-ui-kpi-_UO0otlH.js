import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { X as t, i as n, q as r } from "./lib-Bttd6u5E.js";
import { n as i, t as a } from "./useHints-Dq_w2E8B.js";
import { t as o } from "./useConfig-DlNpz6P8.js";
import { t as s } from "./useNestedProp-vPNvh7rV.js";
import { t as c } from "./usePrefersMotion-BC-CsqR1.js";
import { Fragment as l, computed as u, createCommentVNode as d, createElementBlock as f, createElementVNode as p, createTextVNode as m, createVNode as h, defineAsyncComponent as g, normalizeClass as _, normalizeStyle as v, onMounted as y, openBlock as b, ref as x, renderSlot as S, toDisplayString as C, unref as w, useSlots as T, watch as E } from "vue";
//#region src/components/vue-ui-kpi.vue
var D = /* @__PURE__ */ e({ default: () => j }), O = ["aria-labelledby"], k = ["id"], A = ["aria-label"], j = {
	__name: "vue-ui-kpi",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: Number,
			default: 0
		}
	},
	setup(e) {
		let D = g(() => import("./vue-ui-digits-Uy498lu-.js").then((e) => e.n)), { vue_ui_kpi: j } = o(), M = c(), N = e, P = u({
			get: () => R(),
			set: (e) => e
		});
		i({
			config: () => P.value,
			dataset: () => N.dataset,
			component: "VueUiKpi",
			rules: [a.noHint]
		});
		let F = T(), I = u(() => P.value.debug), L = x(r());
		y(() => {
			F["chart-background"] && I.value && console.warn("VueUiKpi does not support the #chart-background slot.");
		});
		function R() {
			return s({
				userConfig: N.config,
				defaultConfig: j
			});
		}
		E(() => N.config, (e) => {
			P.value = R(), H();
		}, { deep: !0 });
		let z = x((N.dataset, N.dataset)), B = x(P.value.useAnimation ? P.value.animationValueStart : z.value), V = (e) => {
			let t = P.value.animationFrames, n = Math.abs(e - B.value) / t;
			function r() {
				B.value < e ? B.value = Math.min(B.value + n, e) : B.value > e && (B.value = Math.max(B.value - n, e)), B.value !== e && requestAnimationFrame(r);
			}
			r();
		};
		y(() => {
			H();
		});
		function H() {
			P.value.useAnimation && !M.value ? (B.value = P.value.animationValueStart, V(N.dataset)) : B.value = N.dataset;
		}
		return E(() => N.dataset, (e) => {
			P.value.useAnimation && !M.value ? V(e) : B.value = e;
		}), (r, i) => (b(), f("div", {
			class: _(`vue-data-ui-component vue-ui-kpi ${P.value.layoutClass}`),
			style: v(`background:${P.value.backgroundColor}; ${P.value.layoutCss}`),
			"aria-labelledby": `kpi-title-${L.value}`
		}, [
			P.value.title || r.$slots.title ? (b(), f("div", {
				key: 0,
				class: _(`vue-ui-kpi-title ${P.value.titleClass}`),
				style: v(`font-family: ${P.value.fontFamily}; font-size:${P.value.titleFontSize}px; color:${P.value.titleColor}; font-weight:${P.value.titleBold ? "bold" : "normal"}; ${P.value.titleCss}`),
				id: `kpi-title-${L.value}`
			}, [S(r.$slots, "title", { comment: e.dataset }), m(" " + C(P.value.title), 1)], 14, k)) : d("", !0),
			S(r.$slots, "comment-before", { comment: e.dataset }),
			p("div", {
				class: _(`vue-ui-kpi-value ${P.value.valueClass}`),
				style: v(`font-family: ${P.value.fontFamily}; font-size:${P.value.valueFontSize}px; color:${P.value.valueColor}; font-weight:${P.value.valueBold ? "bold" : "normal"}; ${P.value.valueCss}`),
				role: "status",
				"aria-live": "polite",
				"aria-atomic": "true",
				"aria-label": B.value.toFixed(P.value.valueRounding)
			}, [S(r.$slots, "value", { comment: e.dataset }), P.value.analogDigits.show ? (b(), f("div", {
				key: 0,
				style: v({ height: P.value.analogDigits.height + "px" })
			}, [h(w(D), {
				dataset: Number(B.value.toFixed(P.value.valueRounding)),
				config: {
					backgroundColor: P.value.backgroundColor,
					digits: {
						color: P.value.analogDigits.color,
						skeletonColor: P.value.analogDigits.skeletonColor
					}
				}
			}, null, 8, ["dataset", "config"])], 4)) : (b(), f(l, { key: 1 }, [m(C(w(n)(P.value.formatter, B.value, w(t)({
				p: P.value.prefix,
				v: B.value,
				s: P.value.suffix,
				r: P.value.valueRounding
			}))), 1)], 64))], 14, A),
			S(r.$slots, "comment-after", { comment: e.dataset })
		], 14, O));
	}
};
//#endregion
export { D as n, j as t };
