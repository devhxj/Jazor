import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Bt as t, Jt as n, Kt as r, Pt as i, S as a, Vt as ee, X as o, i as s, jt as te, pt as ne, q as re, t as c, tt as l, w as ie } from "./lib-Bttd6u5E.js";
import { n as ae, t as u } from "./useHints-Dq_w2E8B.js";
import { t as d } from "./useConfig-DlNpz6P8.js";
import { n as f, t as oe } from "./BaseScanner-DZvpgOjM.js";
import { t as p } from "./useNestedProp-vPNvh7rV.js";
import { t as se } from "./useThemeCheck-C43Tcqmk.js";
import { t as ce } from "./DefGrad-DVBqDjhO.js";
import { t as le } from "./usePrefersMotion-BC-CsqR1.js";
import { t as ue } from "./vue_ui_sparkbar-z6qO--Kf.js";
import { Fragment as m, computed as h, createBlock as de, createCommentVNode as g, createElementBlock as _, createElementVNode as v, createVNode as y, defineAsyncComponent as fe, guardReactiveProps as pe, mergeProps as b, nextTick as x, normalizeProps as me, normalizeStyle as S, onMounted as C, openBlock as w, ref as T, renderList as he, renderSlot as E, toDisplayString as D, toRefs as O, unref as k, useSlots as A, watch as j } from "vue";
//#region src/components/vue-ui-sparkbar.vue
var M = /* @__PURE__ */ e({ default: () => z }), N = [
	"onClick",
	"onMouseenter",
	"onMouseleave"
], P = {
	key: 1,
	class: "vue-ui-sparkbar-datapoint-value"
}, F = ["xmlns", "viewBox"], I = [
	"height",
	"width",
	"fill",
	"rx"
], L = [
	"height",
	"width",
	"fill",
	"rx"
], R = [
	"height",
	"width",
	"fill",
	"rx"
], ge = {
	key: 2,
	ref: "source",
	dir: "auto"
}, z = {
	__name: "vue-ui-sparkbar",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: Array,
			default() {
				return [];
			}
		},
		backgroundOpacity: {
			type: Number,
			default: null
		}
	},
	emits: ["selectDatapoint"],
	setup(e, { emit: M }) {
		let z = fe(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_sparkbar: _e } = d(), { isThemeValid: ve, warnInvalidTheme: ye } = se(), B = le(), V = e, be = A(), H = T(re()), U = T(q()), W = h(() => U.value.debug);
		C(() => {
			be["chart-background"] && W.value && console.warn("VueUiSparkbar does not support the #chart-background slot.");
		}), ae({
			config: () => U.value,
			dataset: () => V.dataset,
			component: "VueUiSparkbar",
			rules: [u.noHint]
		});
		let xe = h(() => n({
			defaultConfig: { style: {
				backgroundColor: "#99999930",
				animation: { show: !1 },
				layout: { independant: !0 },
				gutter: {
					backgroundColor: "#6A6A6A",
					opacity: 50
				},
				bar: { gradient: { underlayerColor: "#6A6A6A" } }
			} },
			userConfig: U.value.skeletonConfig ?? {}
		})), { loading: G, FINAL_DATASET: K, manualLoading: Se } = f({
			...O(V),
			FINAL_CONFIG: U,
			prepareConfig: q,
			skeletonDataset: V.config?.skeletonDataset ?? [
				{
					name: "_",
					value: 21,
					target: 25,
					color: "#808080"
				},
				{
					name: "_",
					value: 13,
					target: 25,
					color: "#ADADAD"
				},
				{
					name: "_",
					value: 8,
					target: 25,
					color: "#DBDBDB"
				}
			],
			skeletonConfig: n({
				defaultConfig: U.value,
				userConfig: xe.value
			})
		});
		function q() {
			let e = p({
				userConfig: V.config,
				defaultConfig: _e
			}), t = e.theme;
			if (!t) return e;
			if (!ve.value(e)) return ye(e), e;
			let n = p({
				userConfig: ue[t] || V.config,
				defaultConfig: e
			}), a = p({
				userConfig: V.config,
				defaultConfig: n
			});
			return {
				...a,
				customPalette: a.customPalette.length ? a.customPalette : r[t] || i
			};
		}
		j(() => V.config, (e) => {
			U.value = q();
		}, { deep: !0 });
		let Ce = h(() => ie(U.value.customPalette)), J = T(K.value.map((e) => ({
			...e,
			value: U.value.style.animation.show ? 0 : e.value || 0,
			formatter: e.formatter || null
		}))), Y = T(null);
		C(async () => {
			te(V.dataset) && l({
				componentName: "VueUiSparkbar",
				type: "dataset",
				debug: W.value
			}), X();
		});
		function X() {
			if (U.value.style.animation.show && !B.value) {
				let e = U.value.style.animation.animationFrames, t = K.value.map((t, n) => t.value / e), n = K.value.map((e) => e.value || 0).reduce((e, t) => e + t, 0), r = 0;
				function i() {
					r += n / e, r < n ? (J.value = J.value.map((e, n) => ({
						...e,
						value: e.value += t[n]
					})), Y.value = requestAnimationFrame(i)) : J.value = K.value.map((e) => ({
						...e,
						value: e.value || 0,
						formatter: e.formatter || null
					}));
				}
				i();
			}
		}
		j(() => K.value, async (e) => {
			cancelAnimationFrame(Y.value), J.value = K.value.map((e) => ({
				...e,
				value: U.value.style.animation.show && !B.value ? 0 : e.value || 0,
				formatter: e.formatter || null
			})), x(X);
		}, { deep: !0 });
		let Z = T({
			width: 500,
			height: 16
		}), we = h(() => Math.max(...K.value.map((e) => e.value))), Te = h(() => (W.value && K.value.forEach((e, t) => {
			ne({
				datasetObject: e,
				requiredAttributes: ["name", "value"]
			}).forEach((e) => {
				l({
					componentName: "VueUiSparkbar",
					type: "datasetSerieAttribute",
					property: e,
					index: t
				});
			});
		}), J.value.map((e, t) => ({
			...e,
			value: e.value || 0,
			color: a(e.color) || Ce.value[t] || i[t] || i[t % i.length]
		}))));
		function Ee(e) {
			return e / we.value;
		}
		function Q(e) {
			return U.value.style.layout.independant ? e.target ? e.value / e.target : U.value.style.layout.percentage ? e.value > 100 ? 1 : e.value / 100 : U.value.style.layout.target === 0 ? 1 : e.value / U.value.style.layout.target : Ee(e.value);
		}
		function $(e) {
			return U.value.style.layout.independant && e.target || U.value.style.layout.target;
		}
		let De = M;
		function Oe(e, t) {
			De("selectDatapoint", {
				datapoint: e,
				index: t
			}), U.value.events.datapointClick && U.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function ke(e, t) {
			U.value.events.datapointEnter && U.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Ae(e, t) {
			U.value.events.datapointLeave && U.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		return (e, n) => (w(), _("div", {
			class: "vue-data-ui-component vue-ui-sparkbar",
			style: S({
				width: "100%",
				position: "relative",
				fontFamily: U.value.style.fontFamily,
				background: V.backgroundOpacity === null ? U.value.style.backgroundColor : k(t)(U.value.style.backgroundColor, V.backgroundOpacity)
			})
		}, [
			e.$slots.title ? E(e.$slots, "title", me(pe({ title: {
				...e.title,
				title: U.value.style.title.text,
				subtitle: U.value.style.title.subtitle.text
			} })), void 0, void 0, 0) : g("", !0),
			!e.$slots.title && U.value.style.title.text ? (w(), _("div", {
				key: 1,
				class: "vue-ui-sparkbar-title-container",
				style: S({
					background: U.value.style.title.backgroundColor,
					margin: U.value.style.title.margin,
					textAlign: U.value.style.title.textAlign
				})
			}, [v("div", {
				class: "vue-ui-sparkbar-title",
				style: S({
					fontSize: U.value.style.title.fontSize + "px",
					color: U.value.style.title.color,
					fontWeight: U.value.style.title.bold ? "bold" : "normal"
				})
			}, D(U.value.style.title.text), 5), U.value.style.title.subtitle.text ? (w(), _("div", {
				key: 0,
				class: "vue-ui-sparkbar-subtitle",
				style: S({
					fontSize: U.value.style.title.subtitle.fontSize + "px",
					color: U.value.style.title.subtitle.color,
					fontWeight: U.value.style.title.subtitle.bold ? "bold" : "normal"
				})
			}, D(U.value.style.title.subtitle.text), 5)) : g("", !0)], 4)) : g("", !0),
			(w(!0), _(m, null, he(Te.value, (n, r) => (w(), _("div", {
				style: S(`display:flex !important;${["left", "right"].includes(U.value.style.labels.name.position) ? `flex-direction: ${U.value.style.labels.name.position === "right" ? "row-reverse" : "row"} !important` : "flex-direction:column !important"};gap:${U.value.style.gap}px !important;align-items:center;${k(K).length > 0 && r !== k(K).length - 1 ? "margin-bottom:6px" : ""}`),
				onClick: (e) => Oe(n, r),
				onMouseenter: (e) => ke(n, r),
				onMouseleave: (e) => Ae(n, r)
			}, [
				k(G) ? g("", !0) : E(e.$slots, "data-label", b({ ref_for: !0 }, { bar: {
					...n,
					target: $(n),
					valueLabel: k(s)(n.formatter, n.value, k(o)({
						p: n.prefix || "",
						v: n.value,
						s: n.suffix || "",
						r: n.rounding || 0
					}), {
						datapoint: n,
						seriesIndex: r
					}),
					targetLabel: k(s)(n.formatter, $(n), k(o)({
						p: n.prefix || "",
						v: $(n),
						s: n.suffix || "",
						r: n.rounding || 0
					}), {
						datapoint: n,
						seriesIndex: r
					})
				} }), void 0, void 0, 0),
				!e.$slots["data-label"] || k(G) ? (w(), _("div", {
					key: 1,
					style: S({
						display: "flex",
						justifyContent: ["right", "top-right"].includes(U.value.style.labels.name.position) ? "flex-end" : ["top-center"].includes(U.value.style.labels.name.position) ? "center" : "flex-start",
						alignItems: "center",
						width: U.value.style.labels.name.width,
						color: U.value.style.labels.name.color,
						fontSize: U.value.style.labels.fontSize + "px",
						fontWeight: U.value.style.labels.name.bold ? "bold" : "normal",
						flexWrap: "wrap"
					})
				}, [k(G) ? (w(), _("div", {
					key: 0,
					class: "vue-ui-sparkbar-skeleton-name",
					style: S({
						width: "60px",
						height: U.value.style.labels.fontSize + "px",
						borderRadius: U.value.style.labels.fontSize / 4 + "px",
						display: "flex",
						alignItems: "center",
						justifyContent: "space-between",
						marginTop: "5px"
					})
				}, [v("div", { style: S({
					height: "100%",
					width: "40px",
					borderRadius: U.value.style.labels.fontSize / 4 + "px",
					backgroundColor: "#6A6A6A80"
				}) }, null, 4), v("div", { style: S({
					height: "100%",
					width: "15px",
					borderRadius: U.value.style.labels.fontSize / 4 + "px",
					backgroundColor: "#6A6A6A80"
				}) }, null, 4)], 4)) : (w(), _(m, { key: 1 }, [
					v("span", null, D(n.name), 1),
					U.value.style.labels.value.show ? (w(), _("span", {
						key: 0,
						style: S(`font-weight:${U.value.style.labels.value.bold ? "bold" : "normal"}`),
						class: "vue-ui-sparkbar-datapoint-name"
					}, ": " + D(k(s)(n.formatter, n.value, k(o)({
						p: n.prefix || "",
						v: n.value,
						s: n.suffix || "",
						r: n.rounding || 0
					}), {
						datapoint: n,
						seriesIndex: r
					})), 5)) : g("", !0),
					U.value.style.layout.showTargetValue ? (w(), _("span", P, D(" " + U.value.style.layout.targetValueText) + " " + D(k(s)(n.formatter, $(n), k(o)({
						p: n.prefix || "",
						v: $(n),
						s: n.suffix || "",
						r: n.rounding || 0
					}), {
						datapoint: n,
						seriesIndex: r
					})), 1)) : g("", !0)
				], 64))], 4)) : g("", !0),
				(w(), _("svg", {
					role: "img",
					xmlns: k(c),
					viewBox: `0 0 ${Z.value.width} ${Z.value.height}`,
					width: "100%"
				}, [
					y(k(z)),
					v("defs", null, [y(ce, {
						t: "linear",
						x1: "0%",
						y1: "0%",
						x2: "100%",
						y2: "0%",
						id: `sparkbar_gradient_${r}_${H.value}`,
						stops: [[
							"0%",
							k(t)(k(ee)(n.color, .03), 100 - U.value.style.bar.gradient.intensity),
							1
						], [
							"100%",
							n.color,
							1
						]]
					}, null, 8, ["id", "stops"])]),
					v("rect", {
						height: Z.value.height,
						width: Z.value.width,
						x: 0,
						y: 0,
						fill: k(t)(U.value.style.gutter.backgroundColor, U.value.style.gutter.opacity),
						rx: Z.value.height / 2
					}, null, 8, I),
					v("rect", {
						height: Z.value.height,
						width: Z.value.width * Q(n),
						x: 0,
						y: 0,
						fill: U.value.style.bar.gradient.underlayerColor,
						rx: Z.value.height / 2
					}, null, 8, L),
					v("rect", {
						height: Z.value.height,
						width: Z.value.width * Q(n),
						x: 0,
						y: 0,
						fill: U.value.style.bar.gradient.show ? `url(#sparkbar_gradient_${r}_${H.value})` : n.color,
						rx: Z.value.height / 2
					}, null, 8, R)
				], 8, F))
			], 44, N))), 256)),
			e.$slots.source ? (w(), _("div", ge, [E(e.$slots, "source")], 512)) : g("", !0),
			k(G) ? (w(), de(oe, { key: 3 })) : g("", !0)
		], 4));
	}
};
//#endregion
export { M as n, z as t };
