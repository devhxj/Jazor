import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Vt as t, X as n, i as r, jt as i, q as a, t as o, tt as s, z as c } from "./lib-Bttd6u5E.js";
import { n as ee, t as l } from "./useHints-Dq_w2E8B.js";
import { t as u } from "./useConfig-DlNpz6P8.js";
import { t as d } from "./useNestedProp-vPNvh7rV.js";
import { t as f } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as p } from "./DefGrad-DVBqDjhO.js";
import { Fragment as m, computed as h, createCommentVNode as g, createElementBlock as _, createElementVNode as v, createVNode as y, mergeProps as b, normalizeStyle as x, onMounted as S, openBlock as C, ref as w, renderList as T, renderSlot as E, toDisplayString as D, unref as O, useSlots as k, watch as A, withKeys as j } from "vue";
//#region src/components/vue-ui-rating.vue
var M = /* @__PURE__ */ e({ default: () => L }), te = {
	key: 0,
	class: "vue-ui-rating-title",
	style: { width: "100%" }
}, ne = {
	key: 0,
	style: { position: "relative" }
}, re = {
	key: 0,
	style: {
		position: "absolute",
		top: "0",
		left: "0",
		width: "100%",
		height: "100%"
	}
}, ie = {
	key: 1,
	style: {
		position: "absolute",
		top: "0",
		left: "0",
		width: "100%",
		height: "100%"
	}
}, ae = [
	"src",
	"height",
	"width"
], oe = [
	"xmlns",
	"height",
	"width"
], se = [
	"points",
	"fill",
	"stroke",
	"stroke-width"
], ce = [
	"src",
	"alt",
	"height",
	"width",
	"id"
], le = [
	"xmlns",
	"viewBox",
	"height",
	"id"
], N = [
	"points",
	"fill",
	"stroke"
], P = ["xmlns", "height"], F = [
	"onClick",
	"onMouseenter",
	"onFocus",
	"onKeyup"
], I = ["onMouseenter", "onFocus"], L = /*#__PURE__*/ f({
	__name: "vue-ui-rating",
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
	setup(e, { expose: f, emit: M }) {
		let { vue_ui_rating: L } = u(), R = e, z = w(a()), B = w(!1), V = w(void 0), H = w(void 0), U = w([]), ue = k(), de = M, W = h({
			get: () => G(),
			set: (e) => e
		});
		S(() => {
			ue["chart-background"] && W.value.debug && console.warn("VueUiRating does not support the #chart-background slot.");
		}), ee({
			config: () => W.value,
			dataset: () => [],
			component: "VueUiRating",
			rules: [l.noHint]
		});
		let fe = h(() => W.value.useCursorPointer);
		function G() {
			return d({
				userConfig: R.config,
				defaultConfig: L
			});
		}
		A(() => R.config, (e) => {
			W.value = G(), X();
		}, { deep: !0 });
		let K = h(() => typeof R.dataset.rating == "object" && !Array.isArray(R.dataset.rating) ? me(R.dataset.rating) : R.dataset.rating), pe = h(() => typeof R.dataset.rating == "object" && !Array.isArray(R.dataset.rating)), q = w(K.value), J = h(() => W.value.type === "image"), Y = h(() => W.value.readonly);
		function me(e) {
			let t = 0, n = 0;
			for (let r in e) {
				let i = parseInt(r), a = e[r];
				t += i * a, n += a;
			}
			if (n === 0) return 0;
			let r = t / n;
			return Math.min(W.value.to, Math.max(W.value.from, r));
		}
		S(() => {
			X();
		});
		function X() {
			(!Object.hasOwn(R.dataset, "rating") || i(R.dataset)) && s({
				componentName: "VueUiRating",
				type: "datasetAttribute",
				property: "rating",
				debug: W.value.debug
			}), U.value = [];
			for (let e = W.value.from; e <= W.value.to; e += 1) U.value.push(e);
		}
		function Z(e, t = !1) {
			return e > V.value || Y.value ? t ? W.value.style.image.inactiveOpacity : W.value.style.star.inactiveColor : t ? 1 : W.value.style.star.useGradient ? `url(#star_gradient_under_${z.value})` : W.value.style.star.activeColor;
		}
		function Q(e, t = !1) {
			let n = q.value - e, r = t ? 1 : 100;
			switch (!0) {
				case n <= 0: return .001;
				case n > 1: return 1 * r;
				default: return n * r;
			}
		}
		function $(e) {
			Y.value || (q.value = e, de("rate", e));
		}
		function he() {
			return q.value;
		}
		function ge(e = !0) {
			Y.value = e;
		}
		return f({
			getData: he,
			toggleReadonly: ge
		}), (e, i) => (C(), _("div", {
			style: x(`background:${W.value.style.backgroundColor};font-family:${W.value.style.fontFamily};width:100%`),
			class: "vue-data-ui-component vue-ui-rating",
			onMouseover: i[4] ||= (e) => B.value = !0,
			onMouseleave: i[5] ||= (e) => {
				B.value = !1, V.value = void 0;
			}
		}, [
			W.value.style.title.text ? (C(), _("div", te, [v("div", { style: x(`color:${W.value.style.title.color};font-weight:${W.value.style.title.bold ? "bold" : "normal"};text-align:${W.value.style.title.textAlign};margin-bottom:${W.value.style.title.offsetY}px;font-size:${W.value.style.title.fontSize}px`) }, D(W.value.style.title.text), 5), W.value.style.title.subtitle.text ? (C(), _("div", {
				key: 0,
				style: x(`color:${W.value.style.title.subtitle.color};font-size:${W.value.style.title.subtitle.fontSize}px;text-align:${W.value.style.title.textAlign};margin-bottom:${W.value.style.title.subtitle.offsetY}px;font-weight:${W.value.style.title.subtitle.bold ? "bold" : "normal"}`)
			}, D(W.value.style.title.subtitle.text), 5)) : g("", !0)])) : g("", !0),
			W.value.style.rating.show && W.value.style.rating.position === "top" ? (C(), _("div", {
				key: 1,
				style: x(`width:100%;text-align:center;margin-bottom:${W.value.style.rating.offsetY}px;font-size:${W.value.style.rating.fontSize}px;font-weight:${W.value.style.rating.bold ? "bold" : "normal"};margin-left:${W.value.style.rating.offsetX}px`)
			}, D(O(r)(W.value.style.rating.formatter, q.value, O(n)({
				v: q.value,
				r: W.value.style.rating.roundingValue
			}), W.value)), 5)) : g("", !0),
			v("div", {
				class: "vue-ui-rating-wrapper",
				style: x(`height:${W.value.style.itemSize}px;width:100%;display:flex;align-items:center;justify-content:center`)
			}, [
				W.value.style.rating.show && W.value.style.rating.position === "left" ? (C(), _("div", {
					key: 0,
					style: x(`width:fit-content;text-align:center;margin-bottom:${W.value.style.rating.offsetY}px;font-size:${W.value.style.rating.fontSize}px;font-weight:${W.value.style.rating.bold ? "bold" : "normal"};padding-right:${W.value.style.rating.offsetX}px`)
				}, D(O(r)(W.value.style.rating.formatter, q.value, O(n)({
					v: q.value,
					r: W.value.style.rating.roundingValue
				}), W.value)), 5)) : g("", !0),
				(C(!0), _(m, null, T(U.value, (a, s) => (C(), _("div", {
					class: "vue-ui-rating-unit-container",
					style: x(`position:relative;height:${W.value.style.itemSize}px;width:${W.value.style.itemSize}px`)
				}, [
					e.$slots["layer-under"] || e.$slots["layer-above"] ? (C(), _("div", ne, [e.$slots["layer-under"] ? (C(), _("div", re, [E(e.$slots, "layer-under", b({ ref_for: !0 }, {
						value: a,
						size: W.value.style.itemSize,
						hoveredValue: V.value,
						focusedValue: H.value
					}), void 0, !0)])) : g("", !0), e.$slots["layer-above"] ? (C(), _("div", ie, [E(e.$slots, "layer-above", b({ ref_for: !0 }, {
						value: a,
						size: W.value.style.itemSize,
						hoveredValue: V.value,
						focusedValue: H.value
					}), void 0, !0)])) : g("", !0)])) : (C(), _(m, { key: 1 }, [J.value ? (C(), _("img", {
						key: 0,
						src: W.value.style.image.src,
						height: W.value.style.itemSize,
						width: W.value.style.itemSize,
						class: "vue-ui-rating-unit",
						style: x(`position:absolute;top:0;left:0;opacity:${isNaN(V.value) ? W.value.style.image.inactiveOpacity : Z(a, !0)}`)
					}, null, 12, ae)) : (C(), _("svg", {
						key: 1,
						xmlns: O(o),
						viewBox: "0 0 100 100",
						height: W.value.style.itemSize,
						width: W.value.style.itemSize,
						class: "vue-ui-rating-unit"
					}, [v("defs", null, [y(p, {
						t: "radial",
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						id: `star_gradient_under_${z.value}`,
						stops: [[
							"0%",
							O(t)(W.value.style.star.activeColor, .05),
							1
						], [
							"100%",
							W.value.style.star.activeColor,
							1
						]]
					}, null, 8, ["id", "stops"])]), v("polygon", {
						points: O(c)({
							plot: {
								x: 50,
								y: 50
							},
							radius: 30,
							apexes: W.value.style.star.apexes
						}),
						fill: isNaN(V.value) ? W.value.style.star.inactiveColor : Z(a),
						stroke: W.value.style.star.borderColor ? W.value.style.star.borderColor : V.value ? Z(a) : W.value.style.star.inactiveColor,
						"stroke-width": W.value.style.star.borderWidth,
						"stroke-linecap": "round",
						"stroke-linejoin": "round"
					}, null, 8, se)], 8, oe)), J.value ? (C(), _("img", {
						key: 2,
						src: W.value.style.image.src,
						alt: `${W.value.style.image.alt} ${a}`,
						height: W.value.style.itemSize,
						width: W.value.style.itemSize,
						id: `active_${z.value}_${a}`,
						class: "vue-ui-rating-unit",
						style: x(`position:absolute;top:0;left:0;clip:rect(0px,${Q(s, !0) * W.value.style.itemSize}px,${W.value.style.itemSize}px,0px`)
					}, null, 12, ce)) : (C(), _("svg", {
						key: 3,
						xmlns: O(o),
						viewBox: `0 0 ${Q(s)} 100`,
						height: W.value.style.itemSize,
						class: "vue-ui-rating-unit",
						id: `active_${z.value}_${a}`,
						style: {
							position: "absolute",
							top: "0",
							left: "0"
						}
					}, [v("defs", null, [y(p, {
						t: "radial",
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						id: `star_gradient_over_${z.value}`,
						stops: [[
							"0%",
							O(t)(W.value.style.star.activeColor, .05),
							1
						], [
							"100%",
							W.value.style.star.activeColor,
							1
						]]
					}, null, 8, ["id", "stops"])]), v("polygon", {
						points: O(c)({
							plot: {
								x: 50,
								y: 50
							},
							radius: 30,
							apexes: W.value.style.star.apexes
						}),
						fill: W.value.style.star.useGradient ? `url(#star_gradient_over_${z.value})` : W.value.style.star.activeColor,
						stroke: W.value.style.star.activeColor
					}, null, 8, N)], 8, le))], 64)),
					(C(), _("svg", {
						xmlns: O(o),
						viewBox: "0 0 100 100",
						height: W.value.style.itemSize,
						class: "vue-ui-rating-unit",
						style: x(`position:absolute;top:0;left:0;${Y.value ? "" : fe.value ? "cursor:pointer" : ""}`)
					}, [Y.value ? g("", !0) : (C(), _("rect", {
						key: 0,
						class: "vue-ui-rating-mouse-trap",
						x: 0,
						y: 0,
						width: 100,
						height: 100,
						fill: "transparent",
						onClick: (e) => $(a),
						onMouseenter: (e) => V.value = a,
						onMouseleave: i[0] ||= (e) => V.value = void 0,
						onFocus: (e) => H.value = a,
						onBlur: i[1] ||= (e) => H.value = void 0,
						tabindex: "0",
						onKeyup: j((e) => $(a), ["enter"])
					}, null, 40, F)), Y.value ? (C(), _("rect", {
						key: 1,
						class: "vue-ui-rating-mouse-trap",
						x: 0,
						y: 0,
						width: 100,
						height: 100,
						fill: "transparent",
						onMouseenter: (e) => V.value = a,
						onMouseleave: i[2] ||= (e) => V.value = void 0,
						onFocus: (e) => H.value = a,
						onBlur: i[3] ||= (e) => H.value = void 0
					}, null, 40, I)) : g("", !0)], 12, P)),
					W.value.style.tooltip.show && pe.value && Y.value ? (C(), _("div", {
						key: 2,
						class: "vue-ui-rating-tooltip",
						style: x(`border:1px solid ${W.value.style.tooltip.borderColor};position:absolute;top:${-48 + W.value.style.tooltip.offsetY}px;left:50%;transform:translateX(-50%);width:fit-content;text-align:center;background:${W.value.style.tooltip.backgroundColor};display:${V.value === a ? "block" : "none"};padding:2px 12px;border-radius:${W.value.style.tooltip.borderRadius}px;box-shadow:${W.value.style.tooltip.boxShadow}`)
					}, [v("div", { style: x(`width:100%;display:flex;flex-direction:row;gap:6px;position:relative;text-align:center;color:${W.value.style.tooltip.color}`) }, [
						v("span", { style: x(`font-size:${W.value.style.tooltip.fontSize}px`) }, D(a) + ":", 5),
						v("span", { style: x(`font-weight:${W.value.style.tooltip.bold ? "bold" : "normal"};font-size:${W.value.style.tooltip.fontSize}px`) }, D(O(r)(W.value.style.tooltip.formatter, R.dataset.rating[a], O(n)({
							v: R.dataset.rating[a],
							r: W.value.style.tooltip.roundingValue
						}), W.value)), 5),
						v("div", { style: x(`font-family:Arial !important;position:absolute;top:calc(100% - 4px);left:50%;transform:translateX(-50%);color:${W.value.style.tooltip.borderColor}`) }, " ▼ ", 4)
					], 4)], 4)) : g("", !0)
				], 4))), 256)),
				W.value.style.rating.show && W.value.style.rating.position === "right" ? (C(), _("div", {
					key: 1,
					style: x(`width:fit-content;text-align:center;margin-bottom:${W.value.style.rating.offsetY}px;font-size:${W.value.style.rating.fontSize}px;font-weight:${W.value.style.rating.bold ? "bold" : "normal"};padding-left:${W.value.style.rating.offsetX}px`)
				}, D(O(r)(W.value.style.rating.formatter, q.value, O(n)({
					v: q.value,
					r: W.value.style.rating.roundingValue
				}), W.value)), 5)) : g("", !0)
			], 4),
			W.value.style.rating.show && W.value.style.rating.position === "bottom" ? (C(), _("div", {
				key: 2,
				style: x(`width:100%;text-align:center;margin-top:${W.value.style.rating.offsetY}px;font-size:${W.value.style.rating.fontSize}px;font-weight:${W.value.style.rating.bold ? "bold" : "normal"};margin-left:${W.value.style.rating.offsetX}px`)
			}, D(O(r)(W.value.style.rating.formatter, q.value, O(n)({
				v: q.value,
				r: W.value.style.rating.roundingValue
			}), W.value)), 5)) : g("", !0)
		], 36));
	}
}, [["__scopeId", "data-v-d863563b"]]);
//#endregion
export { M as n, L as t };
