import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Bt as t, Et as n, Jt as r, P as i, X as a, b as o, g as ee, i as s, jt as c, q as te, t as ne, tt as re } from "./lib-Bttd6u5E.js";
import { n as ie, t as ae } from "./useHints-Dq_w2E8B.js";
import { t as oe } from "./useConfig-DlNpz6P8.js";
import { n as se, t as ce } from "./BaseScanner-DZvpgOjM.js";
import { t as l } from "./useNestedProp-vPNvh7rV.js";
import { t as le } from "./useThemeCheck-C43Tcqmk.js";
import { t as u } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ue, t as de } from "./useResponsive-ZtArZtUf.js";
import { t as fe } from "./DefGrad-DVBqDjhO.js";
import { t as pe } from "./usePrefersMotion-BC-CsqR1.js";
import { t as me } from "./useFitSvgText-CXTzBplU.js";
import { t as he } from "./vue_ui_spark_trend-DxVmpkmC.js";
import { computed as d, createBlock as ge, createCommentVNode as f, createElementBlock as p, createElementVNode as _e, createVNode as m, defineAsyncComponent as ve, nextTick as h, normalizeStyle as ye, onMounted as be, openBlock as g, ref as _, renderSlot as v, toDisplayString as y, toRefs as xe, unref as b, watch as x } from "vue";
//#region src/components/vue-ui-spark-trend.vue
var S = /* @__PURE__ */ e({ default: () => C }), Se = ["id", "aria-describedby"], Ce = ["id"], we = ["xmlns", "viewBox"], Te = ["width", "height"], Ee = { key: 1 }, De = ["d", "fill"], Oe = ["d", "fill"], ke = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-linecap",
	"stroke-linejoin"
], Ae = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-linecap",
	"stroke-linejoin"
], je = ["d"], Me = ["fill", "d"], Ne = [
	"x",
	"y",
	"width",
	"height"
], Pe = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], Fe = [
	"stroke",
	"cx",
	"cy",
	"fill"
], Ie = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], C = /*#__PURE__*/ u({
	__name: "vue-ui-spark-trend",
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
	setup(e) {
		let u = ve(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_spark_trend: S } = oe(), { isThemeValid: C, warnInvalidTheme: Le } = le(), w = pe(), T = e, E = _(null), Re = _(null), ze = _(null), D = _(null), O = _(null), k = _(!1), A = _(null), j = _(te()), M = _(I());
		ie({
			config: () => M.value,
			dataset: () => T.dataset,
			component: "VueUiSparkTrend",
			rules: [ae.emptyArray, {
				test: (e) => e.length > 1095,
				message: [
					"👀 The dataset has > 1095 datapoints. Above this threshold, the dataset is computed through an LTTB algorithm, to preserve the shape of the data without increasing the number of datapoints.",
					"",
					"▶️ If you need this level of detail, you can change config.downsample.threshold and set a higher value. Note that performance will be impacted."
				]
			}]
		});
		let Be = d(() => r({
			defaultConfig: { style: {
				animation: { show: !1 },
				backgroundColor: "#99999930",
				line: {
					stroke: "#6A6A6A",
					useColorTrend: !1
				},
				dataLabel: {
					show: !1,
					useColorTrend: !1,
					color: "#6A6A6A"
				}
			} },
			userConfig: M.value.skeletonConfig ?? {}
		})), { loading: N, FINAL_DATASET: P, manualLoading: F } = se({
			...xe(T),
			FINAL_CONFIG: M,
			prepareConfig: I,
			skeletonDataset: T.config?.skeletonDataset ?? [
				1,
				2,
				3,
				5,
				8,
				13,
				21,
				34,
				55,
				89,
				144,
				233
			],
			skeletonConfig: r({
				defaultConfig: M.value,
				userConfig: Be.value
			})
		});
		function I() {
			let e = l({
				userConfig: T.config,
				defaultConfig: S
			}), t = e.theme;
			if (!t) return e;
			if (!C.value(e)) return Le(e), e;
			let n = l({
				userConfig: he[t] || T.config,
				defaultConfig: e
			});
			return l({
				userConfig: T.config,
				defaultConfig: n
			});
		}
		let L = d(() => n({
			data: P.value,
			threshold: M.value.downsample.threshold
		}));
		x(() => T.config, (e) => {
			M.value = I(), H.value = M.value.style.width, U.value = M.value.style.height, V();
		}, { deep: !0 });
		function R(e) {
			return e.map((e) => o(e));
		}
		let z = _(n({
			data: P.value,
			threshold: M.value.downsample.threshold
		}).map((e) => M.value.style.animation.show && !w.value || [
			void 0,
			Infinity,
			-Infinity,
			null,
			NaN
		].includes(e) ? null : e));
		x(L, (e) => {
			A.value &&= (cancelAnimationFrame(A.value), null), M.value.style.animation.show && !w.value ? z.value = Array(e.length).fill(null) : z.value = e.map((e) => ![
				void 0,
				Infinity,
				-Infinity,
				null
			].includes(e) && !Number.isNaN(e) ? e : null), B(), h(() => $(".vue-ui-sparktrend-progress-label", 6));
		}, {
			deep: !0,
			immediate: !0
		}), x(() => JSON.stringify(T.dataset), () => {
			A.value &&= (cancelAnimationFrame(A.value), null), F.value = c(T.dataset);
			let e = L.value;
			z.value = M.value.style.animation.show && !w.value ? Array(e.length).fill(null) : e.map((e) => Number.isFinite(e) ? e : null), B(), h(() => $(".vue-ui-sparktrend-progress-label", 6));
		}, {
			deep: !1,
			immediate: !0
		});
		function B() {
			let e = 1e3 / M.value.style.animation.animationFrames, t = performance.now();
			if (!N.value && M.value.style.animation.show && !w.value && M.value.style.animation.animationFrames && P.value.length > 1) {
				z.value = [];
				let n = 0;
				function r() {
					k.value = !0;
					let i = performance.now(), a = i - t;
					a > e ? (t = i - a % e, n < L.value.length ? (z.value.push(L.value[n]), n += 1, A.value = requestAnimationFrame(r)) : (cancelAnimationFrame(A.value), z.value = R(L.value), k.value = !1, $(".vue-ui-sparktrend-progress-label", 6))) : A.value = requestAnimationFrame(r);
				}
				r();
			}
		}
		be(() => {
			V();
		});
		let Ve = d(() => M.value.debug);
		function V() {
			if (c(T.dataset) && (re({
				componentName: "VueUiSparkTrend",
				type: "dataset",
				debug: Ve.value
			}), F.value = !0), c(T.dataset) || (F.value = !1), M.value.responsive) {
				let e = ue(() => {
					let { width: e, height: t } = de({
						chart: E.value,
						source: ze.value
					});
					requestAnimationFrame(() => {
						H.value = e, U.value = t;
					});
				});
				D.value && (O.value && D.value.unobserve(O.value), D.value.disconnect()), D.value = new ResizeObserver(e), O.value = E.value.parentNode, D.value.observe(O.value);
			}
			B(), $(".vue-ui-sparktrend-progress-label", 6);
		}
		let H = _(M.value.style.width), U = _(M.value.style.height), W = d(() => ({
			height: U.value,
			width: H.value
		})), G = d(() => ({
			top: M.value.style.padding.top,
			left: M.value.style.padding.left,
			right: W.value.width - M.value.style.padding.right,
			bottom: W.value.height - M.value.style.padding.bottom,
			height: W.value.height - (M.value.style.padding.top + M.value.style.padding.bottom) - (M.value.style.dataLabel.show ? M.value.style.dataLabel.fontSize : 0),
			width: W.value.width - (M.value.style.padding.left + M.value.style.padding.right)
		})), K = d(() => {
			let e = R(L.value).filter(Number.isFinite);
			if (!e.length) return {
				max: 0,
				min: 0
			};
			let t = e[0], n = e[0];
			for (let r = 1; r < e.length; r++) {
				let i = e[r];
				i > t && (t = i), i < n && (n = i);
			}
			return {
				max: t,
				min: n
			};
		}), He = d(() => {
			let e = L.value, t = e.length ? e[e.length - 1] : "x";
			return [
				j.value,
				e.length,
				Number.isFinite(t) ? t : "x",
				M.value.downsample.threshold,
				M.value.style.line.smooth ? "s" : "l"
			].join("-");
		}), q = d(() => {
			let e = K.value.min >= 0 ? 0 : K.value.min;
			return Math.abs(e);
		}), Ue = d(() => K.value.max + q.value);
		function We(e) {
			return e / Ue.value;
		}
		let Ge = d(() => L.value.length), J = d(() => z.value.map((e, t) => {
			let n = isNaN(e) || [
				void 0,
				null,
				"NaN",
				NaN,
				Infinity,
				-Infinity
			].includes(e) ? 0 : e || 0;
			return {
				value: o(e),
				absoluteValue: o(n),
				plotValue: o(n + q.value),
				toMax: We(n + q.value),
				x: G.value.left + o(t * (G.value.width / (Ge.value - 1))) - M.value.style.padding.right,
				y: G.value.bottom - o(G.value.height * We(n + q.value))
			};
		})), Y = d(() => {
			let e = R(L.value);
			return M.value.style.trendLabel.trendType === "global" ? ee(e) : M.value.style.trendLabel.trendType === "n-1" && e.length > 1 ? (e.at(-1) / e.at(-2) - 1) * 100 : M.value.style.trendLabel.trendType === "lastToFirst" ? (e.at(-1) / e[0] - 1) * 100 : 0;
		}), X = d(() => k.value || Y.value === 0 ? "neutral" : Y.value > 0 ? "positive" : "negative"), Z = d(() => M.value.style.arrow.colors[X.value]), Ke = d(() => {
			let e = {
				x: J.value[0].x,
				y: W.value.height - 6
			}, t = {
				x: J.value[J.value.length - 1].x,
				y: W.value.height - 6
			}, n = [];
			return J.value.forEach((e) => {
				n.push(`${e.x},${e.y} `);
			}), [
				e.x,
				e.y,
				...n,
				t.x,
				t.y
			].toString();
		}), qe = d(() => {
			let e = [];
			return J.value.forEach((t) => {
				e.push(`${t.x},${t.y} `);
			}), `M ${e.toString()}`;
		}), Q = d(() => U.value / 2 - M.value.style.trendLabel.fontSize), Je = d(() => G.value.left * .8), { fitText: $ } = me({
			svgRef: Re,
			unitWidth: Je,
			fontSize: M.value.style.trendLabel.fontSize
		}), Ye = d(() => {
			if (N.value || k.value) return "Trend chart loading";
			let e = J.value?.at(-1), t = e?.value, n = a({
				p: Y.value > 0 ? "+" : "",
				v: Y.value,
				s: "%",
				r: M.value.style.trendLabel.rounding
			}), r = t == null ? "not available" : s(M.value.style.dataLabel.formatter, t, a({
				p: M.value.style.dataLabel.prefix,
				v: t,
				s: M.value.style.dataLabel.suffix,
				r: M.value.style.dataLabel.rounding
			}), { datapoint: e });
			return `Progression ${X.value === "positive" ? `up ${n}` : X.value === "negative" ? `down ${n}` : `stable at ${n}`}. Last value ${r}.`;
		}), Xe = d(() => `sparktrend-a11y-${j.value}`);
		return (e, n) => (g(), p("div", {
			ref_key: "sparkTrendChart",
			ref: E,
			class: "vue-data-ui-component vue-ui-spark-trend",
			id: j.value,
			style: ye(`width:100%;font-family:${M.value.style.fontFamily};background:${M.value.style.backgroundColor}`),
			role: "img",
			"aria-describedby": Xe.value
		}, [
			_e("p", {
				id: Xe.value,
				class: "sr-only",
				"aria-live": "polite"
			}, y(Ye.value), 9, Ce),
			(g(), p("svg", {
				key: He.value,
				ref_key: "svgRef",
				ref: Re,
				xmlns: b(ne),
				viewBox: `0 0 ${W.value.width} ${W.value.height}`,
				style: "width:100%;background:transparent;overflow:visible",
				"aria-hidden": "true"
			}, [
				m(b(u)),
				e.$slots["chart-background"] ? (g(), p("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: W.value.width <= 0 ? 10 : W.value.width,
					height: W.value.height <= 0 ? 10 : W.value.height,
					style: { pointerEvents: "none" }
				}, [v(e.$slots, "chart-background", {}, void 0, !0)], 8, Te)) : f("", !0),
				_e("defs", null, [m(fe, {
					t: "linear",
					x1: "0%",
					y1: "0%",
					x2: "0%",
					y2: "100%",
					id: `pill_gradient_${j.value}`,
					stops: [[
						"0%",
						b(t)(M.value.style.line.useColorTrend ? Z.value : M.value.style.line.stroke, M.value.style.area.opacity),
						1
					], [
						"100%",
						M.value.style.backgroundColor,
						1
					]]
				}, null, 8, ["id", "stops"])]),
				M.value.style.area.show && J.value[0] ? (g(), p("g", Ee, [M.value.style.line.smooth ? (g(), p("path", {
					key: 0,
					d: `M ${J.value[0].x},${G.value.bottom} ${b(i)(J.value)} L ${J.value.at(-1).x},${G.value.bottom} Z`,
					fill: M.value.style.area.useGradient ? `url(#pill_gradient_${j.value})` : b(t)(M.value.style.line.useColorTrend ? Z.value : M.value.style.line.stroke, M.value.style.area.opacity),
					stroke: "none"
				}, null, 8, De)) : (g(), p("path", {
					key: 1,
					d: `M${Ke.value}Z`,
					fill: M.value.style.area.useGradient ? `url(#pill_gradient_${j.value})` : b(t)(M.value.style.line.useColorTrend ? Z.value : M.value.style.line.stroke, M.value.style.area.opacity),
					stroke: "none"
				}, null, 8, Oe))])) : f("", !0),
				M.value.style.line.smooth && J.value.length ? (g(), p("path", {
					key: 2,
					d: `M ${b(i)(J.value)}`,
					stroke: M.value.style.line.useColorTrend ? Z.value : M.value.style.line.stroke,
					fill: "none",
					"stroke-width": M.value.style.line.strokeWidth,
					"stroke-linecap": M.value.style.line.strokeLinecap,
					"stroke-linejoin": M.value.style.line.strokeLinejoin
				}, null, 8, ke)) : f("", !0),
				!M.value.style.line.smooth && J.value.length ? (g(), p("path", {
					key: 3,
					d: qe.value,
					stroke: M.value.style.line.useColorTrend ? Z.value : M.value.style.line.stroke,
					fill: "none",
					"stroke-width": M.value.style.line.strokeWidth,
					"stroke-linecap": M.value.style.line.strokeLinecap,
					"stroke-linejoin": M.value.style.line.strokeLinejoin
				}, null, 8, Ae)) : f("", !0),
				b(N) ? (g(), p("path", {
					key: 4,
					fill: "#6A6A6A",
					d: `M ${G.value.left / 2 + 6}, ${Q.value + 7} ${G.value.left / 2 - 7}, ${Q.value} ${G.value.left / 2 - 7}, ${Q.value + 14} Z`
				}, null, 8, je)) : (g(), p("path", {
					key: 5,
					fill: Z.value,
					d: X.value === "positive" ? `M ${G.value.left / 2}, ${Q.value} ${G.value.left / 2 - 7}, ${Q.value + 12} ${G.value.left / 2 + 7}, ${Q.value + 12} Z` : X.value === "negative" ? `M ${G.value.left / 2}, ${Q.value + 12} ${G.value.left / 2 - 7}, ${Q.value} ${G.value.left / 2 + 7}, ${Q.value} Z` : `M ${G.value.left / 2 + 6}, ${Q.value + 7} ${G.value.left / 2 - 7}, ${Q.value} ${G.value.left / 2 - 7}, ${Q.value + 14} Z`
				}, null, 8, Me)),
				b(N) ? (g(), p("rect", {
					key: 6,
					x: G.value.left / 2 - M.value.style.trendLabel.fontSize - 2,
					y: U.value / 2 + M.value.style.trendLabel.fontSize - 2,
					width: M.value.style.trendLabel.fontSize * 2,
					height: M.value.style.trendLabel.fontSize,
					fill: "#6A6A6A80",
					rx: "3"
				}, null, 8, Ne)) : f("", !0),
				!k.value && !b(N) ? (g(), p("text", {
					key: 7,
					class: "vue-ui-sparktrend-progress-label",
					x: G.value.left / 2,
					y: U.value / 2 + M.value.style.trendLabel.fontSize * 2,
					"text-anchor": "middle",
					fill: M.value.style.trendLabel.useColorTrend ? Z.value : M.value.style.trendLabel.color,
					"font-size": M.value.style.trendLabel.fontSize,
					"font-weight": M.value.style.trendLabel.bold ? "bold" : "normal"
				}, y(b(a)({
					p: Y.value > 0 ? "+" : "",
					v: Y.value,
					s: "%",
					r: M.value.style.trendLabel.rounding
				})), 9, Pe)) : f("", !0),
				J.value.length && J.value.at(-1).x !== void 0 ? (g(), p("circle", {
					key: 8,
					stroke: M.value.style.backgroundColor,
					"stroke-width": 2,
					cx: J.value.at(-1).x,
					cy: J.value.at(-1).y,
					r: 4,
					fill: b(N) ? "#6A6A6A" : Z.value
				}, null, 8, Fe)) : f("", !0),
				J.value.length && J.value.at(-1).x !== void 0 && M.value.style.dataLabel.show ? (g(), p("text", {
					key: 9,
					"text-anchor": "middle",
					x: J.value.at(-1).x,
					y: J.value.at(-1).y - M.value.style.dataLabel.fontSize / 1.5,
					"font-size": M.value.style.dataLabel.fontSize,
					fill: M.value.style.dataLabel.useColorTrend ? Z.value : M.value.style.dataLabel.color,
					"font-weight": M.value.style.dataLabel.bold ? "bold" : "normal"
				}, y(b(s)(M.value.style.dataLabel.formatter, J.value.at(-1).value, b(a)({
					p: M.value.style.dataLabel.prefix,
					v: J.value.at(-1).value,
					s: M.value.style.dataLabel.suffix,
					r: M.value.style.dataLabel.rounding
				}), { datapoint: J.value.at(-1) })), 9, Ie)) : f("", !0)
			], 8, we)),
			e.$slots.source ? (g(), p("div", {
				key: 0,
				ref_key: "source",
				ref: ze,
				dir: "auto"
			}, [v(e.$slots, "source", {}, void 0, !0)], 512)) : f("", !0),
			v(e.$slots, "skeleton", {}, () => [b(N) ? (g(), ge(ce, { key: 0 })) : f("", !0)], !0)
		], 12, Se));
	}
}, [["__scopeId", "data-v-5bd4919c"]]);
//#endregion
export { S as n, C as t };
