import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Bt as t, Jt as n, Vt as r, X as i, i as a, jt as o, pt as s, q as ee, t as c, tt as l } from "./lib-Bttd6u5E.js";
import { n as u, t as d } from "./useHints-Dq_w2E8B.js";
import { t as f } from "./useConfig-DlNpz6P8.js";
import { n as p, t as te } from "./BaseScanner-DZvpgOjM.js";
import { t as m } from "./useNestedProp-vPNvh7rV.js";
import { t as ne } from "./useThemeCheck-C43Tcqmk.js";
import { t as re } from "./Shape-C21CMlWS.js";
import { t as ie } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ae, t as oe } from "./useResponsive-ZtArZtUf.js";
import { t as h } from "./DefGrad-DVBqDjhO.js";
import { t as se } from "./A11yDataTable-DdRsVULz.js";
import { t as ce } from "./useChartAccessibility-DYqac8yF.js";
import { t as le } from "./useFitSvgText-CXTzBplU.js";
import { t as ue } from "./vue_ui_sparkhistogram-BRgvKUH6.js";
import { Fragment as g, computed as _, createBlock as v, createCommentVNode as y, createElementBlock as b, createElementVNode as x, createTextVNode as de, createVNode as fe, defineAsyncComponent as pe, guardReactiveProps as me, nextTick as he, normalizeClass as ge, normalizeProps as _e, normalizeStyle as S, onMounted as ve, openBlock as C, ref as w, renderList as T, renderSlot as E, toDisplayString as D, toRefs as ye, unref as O, useCssVars as be, watch as k } from "vue";
//#region src/components/vue-ui-sparkhistogram.vue
var A = /* @__PURE__ */ e({ default: () => j }), xe = ["id"], Se = { key: 0 }, Ce = { key: 1 }, we = { style: { position: "relative" } }, Te = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], Ee = ["width", "height"], De = [
	"height",
	"width",
	"fill",
	"x",
	"stroke",
	"stroke-width",
	"rx",
	"stroke-dasharray"
], Oe = { key: 1 }, ke = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke",
	"stroke-width",
	"rx"
], Ae = { key: 2 }, je = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], Me = [
	"x",
	"y",
	"font-size",
	"fill"
], Ne = [
	"x",
	"y",
	"font-size",
	"fill"
], Pe = [
	"height",
	"width",
	"x",
	"onMouseover",
	"onMouseleave",
	"onClick"
], Fe = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, j = /*#__PURE__*/ ie({
	__name: "vue-ui-sparkhistogram",
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
		}
	},
	emits: ["selectDatapoint"],
	setup(e, { emit: ie }) {
		be((e) => ({ v5034d9c8: Xe.value }));
		let A = pe(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_sparkhistogram: j } = f(), { isThemeValid: Ie, warnInvalidTheme: Le } = ne(), M = e, N = w(ee()), P = w(null), F = w(null), I = w(null), L = w(null), R = w(null), z = w(!1), B = w(null), V = w("pointer"), H = w(q());
		u({
			config: () => H.value,
			dataset: () => M.dataset,
			component: "VueUiSparkHistogram",
			rules: [d.singleSeries, {
				test: (e) => e.length > 31,
				message: [
					"👀 The number of datapoints is > 31. For a more readable chart, consider:",
					"",
					"▶️ Using VueUiXy with a line series"
				]
			}]
		});
		let Re = _(() => n({
			defaultConfig: { style: {
				animation: { show: !1 },
				backgroundColor: "#99999930"
			} },
			userConfig: H.value.skeletonConfig ?? {}
		})), { loading: ze, FINAL_DATASET: U, manualLoading: Be } = p({
			...ye(M),
			FINAL_CONFIG: H,
			prepareConfig: q,
			skeletonDataset: M.config?.skeletonDataset ?? [
				{
					value: 1,
					intensity: .2,
					color: "#CACACA"
				},
				{
					value: 2,
					intensity: .3,
					color: "#CACACA"
				},
				{
					value: 3,
					intensity: .5,
					color: "#CACACA"
				},
				{
					value: 5,
					intensity: .7,
					color: "#CACACA"
				},
				{
					value: 8,
					intensity: .9,
					color: "#CACACA"
				},
				{
					value: 13,
					intensity: .95,
					color: "#CACACA"
				},
				{
					value: 21,
					intensity: 1,
					color: "#CACACA"
				},
				{
					value: 13,
					intensity: .95,
					color: "#CACACA"
				},
				{
					value: 8,
					intensity: .9,
					color: "#CACACA"
				},
				{
					value: 5,
					intensity: .7,
					color: "#CACACA"
				},
				{
					value: 3,
					intensity: .5,
					color: "#CACACA"
				},
				{
					value: 2,
					intensity: .3,
					color: "#CACACA"
				},
				{
					value: 1,
					intensity: .2,
					color: "#CACACA"
				}
			],
			skeletonConfig: n({
				defaultConfig: H.value,
				userConfig: Re.value
			})
		}), W = w(H.value.style.layout.width), G = w(H.value.style.layout.height), { svgRef: K } = ce({ config: H.value.style.title });
		function q() {
			let e = m({
				userConfig: M.config,
				defaultConfig: j
			}), t = e.theme;
			if (!t) return e;
			if (!Ie.value(e)) return Le(e), e;
			let n = m({
				userConfig: ue[t] || M.config,
				defaultConfig: e
			});
			return m({
				userConfig: M.config,
				defaultConfig: n
			});
		}
		ve(() => {
			He();
		});
		let Ve = _(() => H.value.debug);
		function He() {
			if (o(M.dataset) ? l({
				componentName: "VueUiSparkHistogram",
				type: "dataset",
				debug: Ve.value
			}) : Ve.value && M.dataset.forEach((e, t) => {
				s({
					datasetObject: e,
					requiredAttributes: ["value"]
				}).forEach((e) => {
					l({
						componentName: "VueUiSparkHistogram",
						type: "datasetSerieAttribute",
						property: e,
						index: t
					});
				});
			}), H.value.responsive) {
				let e = ae(() => {
					let { width: e, height: t } = oe({
						chart: I.value,
						title: H.value.style.title.text ? F.value : null,
						source: P.value
					}), n = H.value.style.labels.timeLabel.show ? H.value.style.labels.timeLabel.fontSize * 2 : 0, r = H.value.style.labels.valueLabel.show ? H.value.style.labels.valueLabel.fontSize * 2 : 0;
					requestAnimationFrame(() => {
						W.value = Math.max(10, e), G.value = Math.max(10, t - 12 - n - r);
					});
				});
				L.value && (R.value && L.value.unobserve(R.value), L.value.disconnect()), L.value = new ResizeObserver(e), R.value = I.value.parentNode, L.value.observe(R.value);
			}
		}
		k(() => M.config, (e) => {
			H.value = q(), He();
		}, { deep: !0 });
		let J = _(() => {
			let e = H.value.style.labels.timeLabel.show ? H.value.style.labels.timeLabel.fontSize * 2 : 0, t = H.value.style.labels.valueLabel.show ? H.value.style.labels.valueLabel.fontSize * 2 : 0, n = G.value + e + t, r = W.value, i = H.value.style.layout.padding.top, a = n - H.value.style.layout.padding.bottom, o = H.value.style.layout.padding.left, s = r - H.value.style.layout.padding.right;
			return {
				bottom: a,
				centerY: i + (n - i - H.value.style.layout.padding.bottom) / 2,
				drawingHeight: n - H.value.style.layout.padding.top - H.value.style.layout.padding.bottom - e - t,
				drawingWidth: r - H.value.style.layout.padding.left - H.value.style.layout.padding.right,
				height: n,
				left: o,
				right: s,
				top: i,
				width: r
			};
		}), Ue = _(() => Math.max(...U.value.map((e) => Math.abs(e.value || 0))));
		function We(e) {
			return Math.abs(e) / Ue.value;
		}
		let Y = _(() => U.value.map((e, n) => {
			let r = We(e.value || 0), i = J.value.drawingHeight * r, a = J.value.drawingWidth / U.value.length, o = a * (H.value.style.bars.gap / 100), s = a - o, ee = J.value.centerY - i / 2, c = J.value.left + (o / 2 + n * a), l = J.value.left + n * a, u = e.intensity === void 0 ? 100 : Math.round(e.intensity * 100), d = e.color ? e.color : e.value >= 0 ? t(H.value.style.bars.colors.positive, u) : t(H.value.style.bars.colors.negative, u), f = e.color ? e.color : e.value >= 0 ? H.value.style.bars.colors.positive : H.value.style.bars.colors.negative, p = e.color ? `url(#gradient_datapoint_${n}_${N.value})` : e.value >= 0 ? `url(#gradient_positive_${n}_${N.value})` : `url(#gradient_negative_${n}_${N.value})`, te = c + s / 2;
			return {
				...e,
				color: d,
				gradient: p,
				height: i,
				intensity: u,
				proportion: r,
				stroke: f,
				textAnchor: te,
				trapX: l,
				unitWidth: a,
				width: s,
				x: c,
				y: ee
			};
		}));
		function Ge(e, t) {
			return a(H.value.style.labels.value.formatter, e.value, i({
				p: H.value.style.labels.value.prefix,
				v: e.value,
				s: H.value.style.labels.value.suffix,
				r: H.value.style.labels.value.rounding
			}), {
				datapoint: e,
				seriesIndex: t
			});
		}
		let X = w(null), Ke = ie;
		function Z(e, t) {
			Ke("selectDatapoint", {
				datapoint: e,
				index: t
			}), H.value.events.datapointClick && H.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function qe(e, t) {
			V.value = "pointer", B.value = t, X.value = t, H.value.events.datapointEnter && H.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Je(e, t) {
			V.value !== "keyboard" && (X.value = null, B.value = null, H.value.events.datapointLeave && H.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}));
		}
		function Ye() {
			V.value !== "keyboard" && (X.value = null, B.value = null);
		}
		let Xe = _(() => `${H.value.style.animation.speedMs}ms`), Ze = _(() => J.value.drawingWidth / U.value.length * .9), { fitText: Q } = le({
			svgRef: K,
			unitWidth: Ze
		});
		ve(async () => {
			await he(), Q(".vue-ui-sparkhistogram-top-label", H.value.style.labels.value.minFontSize), Q(".vue-ui-sparkhistogram-bottom-label", H.value.style.labels.valueLabel.minFontSize), Q(".vue-ui-sparkhistogram-time-label", H.value.style.labels.timeLabel.minFontSize);
		}), k([
			W,
			G,
			() => U.value
		], async () => {
			await he(), Q(".vue-ui-sparkhistogram-top-label", H.value.style.labels.value.minFontSize), Q(".vue-ui-sparkhistogram-bottom-label", H.value.style.labels.valueLabel.minFontSize), Q(".vue-ui-sparkhistogram-time-label", H.value.style.labels.timeLabel.minFontSize);
		});
		function Qe() {
			z.value = !0;
		}
		function $e() {
			z.value = !1, B.value = null, V.value = "pointer", X.value = null;
		}
		function et(e) {
			if (!K.value || document.activeElement !== K.value || !Y.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				B.value = null, X.value = null, V.value = "pointer";
				return;
			}
			if (r) {
				if (X.value === null) return;
				let e = Y.value[X.value];
				if (!e) return;
				Z(e, X.value);
				return;
			}
			V.value = "keyboard";
			let a = B.value;
			a === null || a < 0 || a >= Y.value.length ? a = n ? 0 : Y.value.length - 1 : n ? (a += 1, a >= Y.value.length && (a = 0)) : t && (--a, a < 0 && (a = Y.value.length - 1)), B.value = a, X.value = a;
			let o = Y.value[a];
			H.value.events.datapointEnter && H.value.events.datapointEnter({
				datapoint: o,
				seriesIndex: a
			});
		}
		let $ = _(() => ({
			headers: [
				H.value.a11y?.translations?.series ?? "Series",
				H.value.a11y?.translations?.time ?? "Time",
				H.value.a11y?.translations?.value ?? "Value",
				H.value.a11y?.translations?.valueLabel ?? "Label"
			],
			rows: Y.value.map((e, t) => [
				t + 1,
				e.timeLabel ?? "",
				e.value ?? "",
				e.valueLabel ?? ""
			])
		}));
		return (e, n) => (C(), b("div", {
			class: "vue-data-ui-component vue-ui-spark-histogram",
			ref_key: "histogramChart",
			ref: I,
			style: S(`width:100%;background:${H.value.style.backgroundColor};font-family:${H.value.style.fontFamily}`),
			onMouseleave: Ye
		}, [
			x("div", {
				id: `chart-instructions-${N.value}`,
				class: "sr-only"
			}, [x("p", null, D(H.value.a11y.translations.keyboardNavigation), 1)], 8, xe),
			$.value?.rows?.length ? (C(), v(se, {
				key: 0,
				uid: N.value,
				head: $.value.headers,
				body: $.value.rows,
				notice: H.value.a11y.translations.tableAvailable,
				caption: H.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : y("", !0),
			H.value.style.title.text ? (C(), b("div", {
				key: 1,
				ref_key: "chartTitle",
				ref: F,
				style: S(`width:calc(100% - 12px);background:transparent;margin:0 auto;margin:${H.value.style.title.margin};padding: 0 6px;text-align:${H.value.style.title.textAlign}`)
			}, [x("div", { style: S(`font-size:${H.value.style.title.fontSize}px;color:${H.value.style.title.color};font-weight:${H.value.style.title.bold ? "bold" : "normal"}`) }, [
				de(D(H.value.style.title.text) + " ", 1),
				X.value === null ? y("", !0) : (C(), b("span", Se, "- " + D(Y.value[X.value].timeLabel || "") + " " + D(O(a)(H.value.style.labels.value.formatter, Y.value[X.value].value, O(i)({
					p: H.value.style.labels.value.prefix,
					v: Y.value[X.value].value,
					s: H.value.style.labels.value.suffix,
					r: H.value.style.labels.value.rounding
				}), {
					datapoint: Y.value[X.value],
					seriesIndex: X.value
				})), 1)),
				![void 0, null].includes(X.value) && ![null, void 0].includes(Y.value[X.value].valueLabel) ? (C(), b("span", Ce, D(` (${Y.value[X.value].valueLabel || 0})`), 1)) : y("", !0)
			], 4), H.value.style.title.subtitle.text ? (C(), b("div", {
				key: 0,
				style: S(`font-size:${H.value.style.title.subtitle.fontSize}px;color:${H.value.style.title.subtitle.color};font-weight:${H.value.style.title.subtitle.bold ? "bold" : "normal"}`)
			}, D(H.value.style.title.subtitle.text), 5)) : y("", !0)], 4)) : y("", !0),
			x("div", we, [(C(), b("svg", {
				ref_key: "svgRef",
				ref: K,
				xmlns: O(c),
				viewBox: `0 0 ${J.value.width} ${J.value.height}`,
				style: { overflow: "visible" },
				"aria-describedby": `chart-instructions-${N.value}`,
				tabindex: "0",
				onFocus: Qe,
				onBlur: $e,
				onKeydown: et
			}, [
				fe(O(A)),
				e.$slots["chart-background"] ? (C(), b("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: J.value.width,
					height: J.value.height,
					style: { pointerEvents: "none" }
				}, [E(e.$slots, "chart-background", {}, void 0, !0)], 8, Ee)) : y("", !0),
				x("defs", null, [
					(C(!0), b(g, null, T(Y.value, (e, n) => (C(), v(h, {
						t: "radial",
						id: `gradient_positive_${n}_${N.value}`,
						key: `gradient_positive_${n}_${N.value}`,
						cy: "50%",
						cx: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						stops: [[
							"0%",
							O(t)(O(r)(H.value.style.bars.colors.positive, .05), e.intensity),
							1
						], [
							"100%",
							O(t)(H.value.style.bars.colors.positive, e.intensity),
							1
						]]
					}, null, 8, ["id", "stops"]))), 128)),
					(C(!0), b(g, null, T(Y.value, (e, n) => (C(), v(h, {
						t: "radial",
						id: `gradient_negative_${n}_${N.value}`,
						key: `gradient_negative_${n}_${N.value}`,
						cy: "50%",
						cx: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						stops: [[
							"0%",
							O(t)(O(r)(H.value.style.bars.colors.negative, .05), e.intensity),
							1
						], [
							"100%",
							O(t)(H.value.style.bars.colors.negative, e.intensity),
							1
						]]
					}, null, 8, ["id", "stops"]))), 128)),
					(C(!0), b(g, null, T(Y.value, (e, n) => (C(), v(h, {
						t: "radial",
						id: `gradient_datapoint_${n}_${N.value}`,
						key: `gradient_datapoint_${n}_${N.value}`,
						cy: "50%",
						cx: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						stops: [[
							"0%",
							O(t)(O(r)(e.color, .05), e.intensity),
							1
						], [
							"100%",
							O(t)(e.color, e.intensity),
							1
						]]
					}, null, 8, ["id", "stops"]))), 128))
				]),
				(C(!0), b(g, null, T(Y.value, (e, t) => (C(), b("g", null, [X.value !== null && X.value === t ? (C(), b("rect", {
					key: 0,
					height: J.value.height,
					width: e.unitWidth,
					fill: H.value.style.selector.fill,
					x: e.trapX,
					y: 0,
					stroke: H.value.style.selector.stroke,
					"stroke-width": H.value.style.selector.strokeWidth,
					rx: H.value.style.selector.borderRadius,
					"stroke-dasharray": H.value.style.selector.strokeDasharray
				}, null, 8, De)) : y("", !0)]))), 256)),
				!H.value.style.bars.shape || H.value.style.bars.shape === "square" ? (C(), b("g", Oe, [(C(!0), b(g, null, T(Y.value, (e, t) => (C(), b("rect", {
					x: e.x,
					y: e.y,
					height: e.height,
					width: e.width,
					fill: H.value.style.bars.colors.gradient.show ? e.gradient : e.color,
					stroke: e.stroke,
					"stroke-width": H.value.style.bars.strokeWidth,
					rx: `${H.value.style.bars.borderRadius * e.proportion / 12}%`,
					class: ge({ "vue-ui-sparkhistogram-shape": H.value.style.animation.show })
				}, null, 10, ke))), 256))])) : (C(), b("g", Ae, [(C(!0), b(g, null, T(Y.value, (e, t) => (C(), v(re, {
					plot: {
						x: e.x + e.width / 2,
						y: e.y + e.height / 2
					},
					color: H.value.style.bars.colors.gradient.show ? e.gradient : e.color,
					shape: H.value.style.bars.shape,
					radius: Math.min(e.height * .4, e.width * .4),
					class: ge({ "vue-ui-sparkhistogram-shape": H.value.style.animation.show })
				}, null, 8, [
					"plot",
					"color",
					"shape",
					"radius",
					"class"
				]))), 256))])),
				O(ze) ? y("", !0) : (C(), b(g, { key: 3 }, [
					(C(!0), b(g, null, T(Y.value, (e, t) => (C(), b("g", null, [H.value.style.labels.value.show ? (C(), b("text", {
						key: 0,
						class: "vue-ui-sparkhistogram-top-label",
						"text-anchor": "middle",
						x: e.textAnchor,
						y: e.y - H.value.style.labels.value.fontSize / 3 + H.value.style.labels.value.offsetY,
						"font-size": H.value.style.labels.value.fontSize,
						"font-weight": H.value.style.labels.value.bold ? "bold" : "normal",
						fill: H.value.style.labels.value.color
					}, D(Ge(e, t)), 9, je)) : y("", !0)]))), 256)),
					(C(!0), b(g, null, T(Y.value, (e, t) => (C(), b("g", null, [e.valueLabel && H.value.style.labels.valueLabel.show ? (C(), b("text", {
						key: 0,
						class: "vue-ui-sparkhistogram-bottom-label",
						x: e.textAnchor,
						y: e.y + e.height + H.value.style.labels.valueLabel.fontSize,
						"font-size": H.value.style.labels.valueLabel.fontSize,
						"text-anchor": "middle",
						fill: H.value.style.labels.valueLabel.color
					}, D(e.valueLabel), 9, Me)) : y("", !0)]))), 256)),
					(C(!0), b(g, null, T(Y.value, (e, t) => (C(), b("g", null, [e.timeLabel && H.value.style.labels.timeLabel.show ? (C(), b("text", {
						key: 0,
						class: "vue-ui-sparkhistogram-time-label",
						x: e.textAnchor,
						y: J.value.height,
						"font-size": H.value.style.labels.timeLabel.fontSize,
						fill: H.value.style.labels.timeLabel.color,
						"text-anchor": "middle"
					}, D(e.timeLabel), 9, Ne)) : y("", !0)]))), 256))
				], 64)),
				(C(!0), b(g, null, T(Y.value, (e, t) => (C(), b("g", null, [x("rect", {
					height: J.value.height,
					width: e.unitWidth,
					fill: "transparent",
					x: e.trapX,
					y: 0,
					onMouseover: (n) => qe(e, t),
					onMouseleave: (n) => Je(e, t),
					onClick: () => Z(e, t)
				}, null, 40, Pe)]))), 256))
			], 40, Te)), e.$slots.hint ? (C(), b("div", Fe, [E(e.$slots, "hint", _e(me({
				hint: H.value.a11y.translations.keyboardNavigation,
				isVisible: z.value
			})), void 0, !0)])) : y("", !0)]),
			e.$slots.source ? (C(), b("div", {
				key: 2,
				ref_key: "source",
				ref: P,
				dir: "auto"
			}, [E(e.$slots, "source", {}, void 0, !0)], 512)) : y("", !0),
			E(e.$slots, "skeleton", {}, () => [O(ze) ? (C(), v(te, { key: 0 })) : y("", !0)], !0)
		], 36));
	}
}, [["__scopeId", "data-v-9193580a"]]);
//#endregion
export { A as n, j as t };
