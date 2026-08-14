import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Jt as t, X as n, b as r, i, jt as ee, pt as te, q as ne, t as re, tt as a, yt as ie } from "./lib-Bttd6u5E.js";
import { n as ae, t as oe } from "./useHints-Dq_w2E8B.js";
import { t as se } from "./useConfig-DlNpz6P8.js";
import { n as ce, t as o } from "./BaseScanner-DZvpgOjM.js";
import { t as s } from "./useNestedProp-vPNvh7rV.js";
import { t as c } from "./useThemeCheck-C43Tcqmk.js";
import { t as le } from "./DefGrad-DVBqDjhO.js";
import { t as l } from "./useChartAccessibility-DYqac8yF.js";
import { t as ue } from "./usePrefersMotion-BC-CsqR1.js";
import { t as de } from "./vue_ui_sparkgauge-BX1MS3bA.js";
import { computed as u, createBlock as fe, createCommentVNode as d, createElementBlock as f, createElementVNode as p, createVNode as m, defineAsyncComponent as pe, normalizeClass as h, normalizeStyle as g, onBeforeUnmount as me, onMounted as _, openBlock as v, ref as y, renderSlot as b, toDisplayString as x, toRefs as he, unref as S, watch as C } from "vue";
//#region src/components/vue-ui-sparkgauge.vue
var w = /* @__PURE__ */ e({ default: () => j }), ge = ["xmlns", "viewBox"], T = ["width", "height"], E = [
	"d",
	"stroke",
	"stroke-linecap"
], D = [
	"d",
	"stroke",
	"stroke-linecap",
	"stroke-dashoffset"
], O = [
	"x",
	"y",
	"width",
	"height"
], k = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], A = {
	key: 2,
	ref: "source",
	dir: "auto"
}, j = {
	__name: "vue-ui-sparkgauge",
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
	setup(e) {
		let w = pe(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_sparkgauge: j } = se(), { isThemeValid: M, warnInvalidTheme: N } = c(), P = ue(), F = y(null), I = e, L = y(ne()), R = y(W());
		ae({
			config: () => R.value,
			dataset: () => I.dataset,
			component: "VueUiSparkgauge",
			rules: [oe.noHint]
		});
		let z = u(() => R.value.style.animation.show && !P.value), _e = u(() => t({
			defaultConfig: { style: {
				animation: { show: !1 },
				background: "#99999930",
				colors: {
					min: "#CACACA",
					max: "#6A6A6A"
				},
				track: { autoColor: !0 },
				gutter: { color: "#6A6A6A80" }
			} },
			userConfig: R.value.skeletonConfig ?? {}
		})), { loading: B, FINAL_DATASET: V } = ce({
			...he(I),
			FINAL_CONFIG: R,
			prepareConfig: W,
			skeletonDataset: I.config?.skeletonDataset ?? {
				value: 0,
				min: -1,
				max: 1,
				title: ""
			},
			skeletonConfig: t({
				defaultConfig: R.value,
				userConfig: _e.value
			})
		}), H = u(() => {
			let e = V.value.min ?? 0, t = V.value.max ?? 0;
			return {
				min: e,
				max: t,
				diff: t - e
			};
		}), U = u(() => H.value.diff / R.value.style.animation.speedMs), { svgRef: ve } = l({ config: { text: I.dataset?.title || "" } });
		function W() {
			let e = s({
				userConfig: I.config,
				defaultConfig: j
			}), t = e.theme;
			if (!t) return e;
			if (!M.value(e)) return N(e), e;
			let n = s({
				userConfig: de[t] || I.config,
				defaultConfig: e
			});
			return s({
				userConfig: I.config,
				defaultConfig: n
			});
		}
		_(() => {
			K();
		});
		let G = u(() => R.value.debug);
		function K() {
			ee(I.dataset) ? a({
				componentName: "VueUiSparkgauge",
				type: "dataset",
				debug: G.value
			}) : te({
				datasetObject: I.dataset,
				requiredAttributes: [
					"value",
					"min",
					"max"
				]
			}).forEach((e) => {
				a({
					componentName: "VueUiSparkgauge",
					type: "datasetAttribute",
					property: e,
					debug: G.value
				});
			});
		}
		C(() => I.config, () => {
			R.value = W(), J.value = z.value ? H.value.min : V.value.value, K();
		}, { deep: !0 });
		let q = u(() => ({
			height: R.value.style.height,
			width: 128,
			base: R.value.style.basePosition
		})), J = y(z.value ? H.value.min : V.value.value);
		C([() => V.value.value, z], ([e, t]) => {
			Y(e || 0, t);
		}, { immediate: !0 });
		let ye = u(() => J.value > H.value.max ? H.value.max : J.value < H.value.min ? H.value.min : J.value);
		_(() => {
			Y(V.value.value || 0);
		});
		function Y(e, t = z.value) {
			if (F.value &&= (cancelAnimationFrame(F.value), null), !t) {
				J.value = e;
				return;
			}
			function n() {
				J.value < e ? J.value = Math.min(J.value + U.value, e) : J.value > e && (J.value = Math.max(J.value - U.value, e)), J.value === e ? F.value = null : F.value = requestAnimationFrame(n);
			}
			n();
		}
		let X = u(() => V.value.title ?? ""), Z = u(() => {
			let e = H.value.diff;
			if (!isFinite(e) || e === 0) return 0;
			let t = H.value.min, n = ye.value;
			return n >= 0 ? (n - t) / e : (Math.abs(t) - Math.abs(n)) / e;
		}), Q = u(() => ie(R.value.style.colors.min, R.value.style.colors.max, H.value.min, H.value.max, J.value)), $ = u(() => R.value.style.dataLabel.autoColor ? Q.value : R.value.style.dataLabel.color), be = u(() => R.value.style.track.autoColor ? Q.value : R.value.style.track.color);
		return me(() => {
			F.value && cancelAnimationFrame(F.value);
		}), (e, t) => (v(), f("div", {
			class: "vue-data-ui-component vue-ui-sparkgauge",
			style: g(`font-family:${R.value.style.fontFamily};width: 100%; background:${R.value.style.background}`)
		}, [
			R.value.style.title.show && X.value && R.value.style.title.position === "top" ? (v(), f("div", {
				key: 0,
				class: "vue-data-ui-sparkgauge-label",
				style: g(`font-size:${R.value.style.title.fontSize}px;text-align:${R.value.style.title.textAlign};font-weight:${R.value.style.title.bold ? "bold" : "normal"};color:${R.value.style.title.color}`)
			}, x(X.value), 5)) : d("", !0),
			(v(), f("svg", {
				ref_key: "svgRef",
				ref: ve,
				xmlns: S(re),
				viewBox: `0 0 ${q.value.width} ${q.value.height}`,
				style: "overflow: visible; background:transparent; width:100%;"
			}, [
				m(S(w)),
				e.$slots["chart-background"] && !S(B) ? (v(), f("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: q.value.width,
					height: q.value.height,
					style: { pointerEvents: "none" }
				}, [b(e.$slots, "chart-background")], 8, T)) : d("", !0),
				p("defs", null, [m(le, {
					t: "linear",
					id: `gradient_${L.value}`,
					x1: "-10%",
					y1: "100%",
					x2: "110%",
					y2: "100%",
					stops: [[
						"0%",
						R.value.style.colors.min,
						1
					], [
						"100%",
						R.value.style.colors.max,
						1
					]]
				}, null, 8, ["id", "stops"])]),
				p("path", {
					d: `M10 ${q.value.base} A 1 1 0 1 1 118 ${q.value.base}`,
					stroke: R.value.style.gutter.color,
					"stroke-width": 8,
					"stroke-linecap": R.value.style.gutter.strokeLinecap,
					fill: "none"
				}, null, 8, E),
				Z.value === 0 ? d("", !0) : (v(), f("path", {
					key: 1,
					d: `M10 ${q.value.base} A 1 1 0 1 1 118 ${q.value.base}`,
					stroke: R.value.style.colors.showGradient ? `url(#gradient_${L.value})` : be.value,
					"stroke-width": 8,
					"stroke-linecap": R.value.style.track.strokeLinecap,
					fill: "none",
					"stroke-dasharray": 169.5,
					"stroke-dashoffset": 169.5 - 169.5 * Z.value,
					class: h({ "vue-ui-sparkgauge-track": R.value.style.animation.show }),
					style: g(R.value.style.animation.show ? `animation: vue-ui-sparkgauge-animation ${R.value.style.animation.speedMs}ms ease-in;` : "")
				}, null, 14, D)),
				S(B) ? (v(), f("rect", {
					key: 2,
					x: q.value.width / 2 - R.value.style.dataLabel.fontSize / 2,
					y: q.value.base + 6 + R.value.style.dataLabel.offsetY - R.value.style.dataLabel.fontSize,
					width: R.value.style.dataLabel.fontSize,
					height: R.value.style.dataLabel.fontSize,
					fill: "#6A6A6A50",
					rx: 3
				}, null, 8, O)) : (v(), f("text", {
					key: 3,
					"text-anchor": "middle",
					x: q.value.width / 2,
					y: q.value.base + 6 + R.value.style.dataLabel.offsetY,
					"font-size": R.value.style.dataLabel.fontSize,
					fill: $.value,
					"font-weight": R.value.style.dataLabel.bold ? "bold" : "normal"
				}, x(S(i)(R.value.style.dataLabel.formatter, S(r)(J.value), S(n)({
					p: R.value.style.dataLabel.prefix,
					v: S(r)(J.value),
					s: R.value.style.dataLabel.suffix,
					r: R.value.style.dataLabel.rounding
				}), {
					datapoint: S(r)(J.value),
					color: $.value
				})), 9, k))
			], 8, ge)),
			R.value.style.title.show && X.value && R.value.style.title.position === "bottom" ? (v(), f("div", {
				key: 1,
				class: "vue-data-ui-sparkgauge-label",
				style: g(`font-size:${R.value.style.title.fontSize}px;text-align:${R.value.style.title.textAlign};font-weight:${R.value.style.title.bold ? "bold" : "normal"};font-weight:${R.value.style.title.bold ? "bold" : "normal"};color:${R.value.style.title.color}`)
			}, x(X.value), 5)) : d("", !0),
			e.$slots.source ? (v(), f("div", A, [b(e.$slots, "source")], 512)) : d("", !0),
			b(e.$slots, "skeleton", {}, () => [S(B) ? (v(), fe(o, { key: 0 })) : d("", !0)])
		], 4));
	}
};
//#endregion
export { w as n, j as t };
