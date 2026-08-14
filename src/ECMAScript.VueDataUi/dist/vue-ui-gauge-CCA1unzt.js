import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Bt as t, G as n, Jt as r, Kt as i, Mt as a, O as ee, Pt as o, S as te, X as s, b as ne, f as re, i as ie, jt as ae, kt as c, pt as oe, q as se, qt as ce, t as le, tt as l, w as ue } from "./lib-Bttd6u5E.js";
import { n as de } from "./useHints-Dq_w2E8B.js";
import { t as fe } from "./useConfig-DlNpz6P8.js";
import { t as pe } from "./usePrinter-DN5bYhTG.js";
import { n as me, t as he } from "./BaseScanner-DZvpgOjM.js";
import { t as ge } from "./useNestedProp-vPNvh7rV.js";
import { t as _e } from "./useThemeCheck-C43Tcqmk.js";
import { t as ve } from "./useChartExport-DNiwdPmb.js";
import { t as ye } from "./img-Bnokohej.js";
import { n as be } from "./Title-BE3qg9xl.js";
import { t as xe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Se, t as Ce } from "./useResponsive-ZtArZtUf.js";
import { t as we } from "./DefGrad-DVBqDjhO.js";
import { t as Te } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ee } from "./useChartAccessibility-DYqac8yF.js";
import { t as De } from "./usePrefersMotion-BC-CsqR1.js";
import { t as Oe } from "./useAutoSizeLabelsInsideViewbox-DvDwcwi_.js";
import { t as ke } from "./vue_ui_gauge-Cf1RZc9q.js";
import { Fragment as u, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createSlots as Ae, createVNode as je, defineAsyncComponent as g, guardReactiveProps as _, mergeProps as v, nextTick as Me, normalizeClass as Ne, normalizeProps as y, normalizeStyle as Pe, onBeforeUnmount as Fe, onMounted as Ie, openBlock as b, ref as x, renderList as S, renderSlot as C, shallowRef as Le, toDisplayString as w, toRefs as Re, unref as T, watch as E, withCtx as D } from "vue";
//#region src/components/vue-ui-gauge.vue
var ze = /* @__PURE__ */ e({ default: () => _t }), Be = ["id"], Ve = { key: 0 }, He = ["xmlns", "viewBox"], Ue = ["width", "height"], We = ["id"], Ge = ["stdDeviation"], Ke = { key: 1 }, qe = [
	"d",
	"fill",
	"stroke"
], Je = [
	"d",
	"fill",
	"stroke"
], Ye = ["d", "fill"], Xe = ["id", "d"], Ze = [
	"fill",
	"font-size",
	"font-weight"
], Qe = ["href", "startOffset"], $e = [
	"text-anchor",
	"fill",
	"font-size",
	"font-weight",
	"innerHTML"
], et = ["d", "filter"], tt = ["stroke", "stroke-width"], nt = ["stroke", "stroke-width"], rt = ["stroke", "stroke-width"], it = ["stroke", "stroke-width"], at = { key: 8 }, ot = [
	"x",
	"y",
	"text-anchor",
	"font-size",
	"font-weight",
	"fill"
], st = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], ct = { key: 0 }, lt = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], ut = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], dt = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke-width",
	"filter"
], ft = { key: 1 }, pt = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], mt = [
	"cx",
	"cy",
	"fill",
	"r",
	"stroke-width",
	"stroke"
], ht = [
	"x",
	"y",
	"font-size",
	"fill"
], gt = {
	key: 4,
	class: "vue-data-ui-watermark"
}, _t = /*#__PURE__*/ xe({
	__name: "vue-ui-gauge",
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
	emits: ["copyAlt"],
	setup(e, { expose: xe, emit: ze }) {
		let _t = g(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), vt = g(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), yt = g(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_gauge: bt } = fe(), { isThemeValid: xt, warnInvalidTheme: St } = _e(), Ct = De(), O = e, wt = ze, Tt = d(() => !!O.dataset && Object.keys(O.dataset).length > 0 && O.dataset.series && O.dataset.series.length), k = x(se()), Et = x(null), Dt = x(0), A = x(null), Ot = x(null), kt = x(null), At = x(null), jt = x(null), Mt = x(0), j = x(Rt());
		de({
			config: () => j.value,
			dataset: () => O.dataset,
			component: "VueUiGauge",
			rules: [{
				test: (e) => e?.series && e.series?.length > 6,
				message: [
					"👀 The number of steps is > 6, which can make the chart hard to read and labels overlap. Consider:",
					"",
					"▶️ Using broader steps."
				]
			}]
		});
		let Nt = d(() => j.value.userOptions.useCursorPointer), Pt = d(() => r({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					animation: { use: !1 },
					layout: {
						markers: { show: !1 },
						segmentSeparators: { stroke: "#6A6A6A" },
						segmentNames: { show: !1 },
						indicatorArc: { fill: "#6A6A6A50" },
						pointer: {
							stroke: "#6A6A6A",
							useRatingColor: !0,
							circle: {
								stroke: "#6A6A6A",
								color: "#6A6A6A"
							}
						}
					},
					legend: { show: !1 }
				} }
			},
			userConfig: j.value.skeletonConfig ?? {}
		})), { loading: Ft, FINAL_DATASET: M, manualLoading: N } = me({
			...Re(O),
			FINAL_CONFIG: j,
			prepareConfig: Rt,
			callback: () => {
				Promise.resolve().then(async () => {
					await Me();
				});
			},
			skeletonDataset: O.config?.skeletonDataset ?? {
				value: 0,
				series: [{
					from: -1,
					to: 0,
					name: "_",
					color: "#A1A1A1"
				}, {
					from: 0,
					to: 1,
					name: "__",
					color: "#CACACA"
				}]
			},
			skeletonConfig: r({
				defaultConfig: j.value,
				userConfig: Pt.value
			})
		}), { userOptionsVisible: P, setUserOptionsVisibility: It, keepUserOptionState: Lt } = Te({ config: j.value }), { svgRef: F } = Ee({ config: j.value.style.chart.title });
		E(() => O.config, (e) => {
			Ft.value || (j.value = Rt()), P.value = !j.value.userOptions.showOnChartHover, Yt(), Mt.value += 1;
		}, { deep: !0 }), E(() => O.dataset, (e) => {
			Yt(), e && Object.keys(e).length > 0 && (N.value = j.value?.loading ?? !1);
		}, {
			deep: !0,
			immediate: !1
		});
		function Rt() {
			let e = ge({
				userConfig: O.config,
				defaultConfig: bt
			}), t = e.theme;
			if (!t) return e;
			if (!xt.value(e)) return St(e), e;
			let n = ge({
				userConfig: ke[t] || O.config,
				defaultConfig: e
			}), r = ge({
				userConfig: O.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : i[t] || o
			};
		}
		let { isPrinting: zt, isImaging: Bt, generatePdf: Vt, generateImage: Ht } = pe({
			elementId: `vue-ui-gauge_${k.value}`,
			fileName: j.value.style.chart.title.text || "vue-ui-gauge",
			options: j.value.userOptions.print
		}), Ut = d(() => j.value.userOptions.show && !j.value.style.chart.title.text), Wt = d(() => ue(j.value.customPalette)), I = d(() => {
			if (ae(M.value.series || {})) return {
				value: 0,
				series: [{
					from: 0,
					to: 0
				}]
			};
			let e = [];
			(M.value.series || []).forEach((t) => {
				e.push(t.from || 1e-7), e.push(t.to || 1e-7);
			});
			let t = Math.max(...e);
			return {
				...M.value,
				series: (M.value.series || []).map((e, n) => ({
					...e,
					color: te(e.color) || Wt.value[n] || o[n],
					value: ((e.to || 0) - (e.from || 0)) / t * 100
				}))
			};
		}), L = x(512), R = x({
			height: 358.4,
			width: L.value,
			top: 0,
			bottom: 358.4,
			centerX: 179.2,
			centerY: L.value / 2,
			labelFontSize: 18,
			legendFontSize: j.value.style.chart.legend.fontSize,
			pointerRadius: j.value.style.chart.layout.pointer.circle.radius,
			trackSize: j.value.style.chart.layout.track.size,
			pointerSize: j.value.style.chart.layout.pointer.size,
			pointerStrokeWidth: j.value.style.chart.layout.pointer.strokeWidth,
			markerOffset: j.value.style.chart.layout.markers.offsetY + 3,
			segmentFontSize: j.value.style.chart.layout.segmentNames.fontSize
		}), z = x(0), B = x(0), V = d(() => j.value.style.chart.animation.use && !Ct.value), H = x(V.value ? Math.min(...M.value.series.map((e) => e.from)) : M.value.value), U = null;
		function Gt() {
			U !== null && (cancelAnimationFrame(U), U = null);
		}
		E(V, (e) => {
			e || (Gt(), H.value = M.value.value);
		}), E(() => M.value.value, () => {
			Xt(M.value.value);
		});
		let W = d(() => {
			let e = R.value.width / 2, t = Y.value.base, n = Math.PI * ((H.value + 0 - B.value) / (z.value - B.value)) + Math.PI;
			return {
				x1: e,
				y1: t,
				x2: e + Y.value.pointerSize * R.value.pointerSize * .9 * Math.cos(n),
				y2: t + Y.value.pointerSize * R.value.pointerSize * .9 * Math.sin(n)
			};
		}), Kt = d(() => {
			let e = R.value.width / 2, t = Y.value.base, n = Math.PI * ((H.value + 0 - B.value) / (z.value - B.value)) + Math.PI, r = e + Y.value.pointerSize * R.value.pointerSize * .9 * Math.cos(n), i = t + Y.value.pointerSize * R.value.pointerSize * .9 * Math.sin(n), a = R.value.pointerRadius, ee = e + a * Math.cos(n + Math.PI / 2), o = t + a * Math.sin(n + Math.PI / 2), te = e + a * Math.cos(n - Math.PI / 2), s = t + a * Math.sin(n - Math.PI / 2);
			return isNaN(r) ? null : `M ${r},${i} ${ee},${o} ${te},${s} Z`;
		}), qt = d(() => {
			for (let e = 0; e < I.value.series.length; e += 1) {
				let { color: t, from: n, to: r } = I.value.series[e];
				if (H.value >= n && H.value <= r) return t;
			}
			return "#2D353C";
		}), G = Le(null), K = Le(null), q = d({
			get: () => R.value.segmentFontSize,
			set: (e) => e
		}), { autoSizeLabels: Jt } = Oe({
			svgRef: F,
			fontSize: R.value.segmentFontSize,
			minFontSize: j.value.style.chart.layout.segmentNames.minFontSize,
			sizeRef: q,
			labelClass: ".vue-ui-gauge-label-flat"
		}), J = d(() => j.value.debug);
		function Yt() {
			let e = !1;
			if (ae(O.dataset) ? (l({
				componentName: "VueUiGauge",
				type: "dataset",
				debug: J.value
			}), N.value = !0, e = !0) : (oe({
				datasetObject: O.dataset,
				requiredAttributes: ["value", "series"]
			}).forEach((t) => {
				l({
					componentName: "VueUiGauge",
					type: "datasetAttribute",
					property: t,
					debug: J.value
				}), N.value = !0, e = !0;
			}), Object.hasOwn(O.dataset, "series") && (O.dataset.series.length ? O.dataset.series.forEach((t, n) => {
				oe({
					datasetObject: t,
					requiredAttributes: ["from", "to"]
				}).forEach((t) => {
					l({
						componentName: "VueUiGauge",
						type: "datasetSerieAttribute",
						property: t,
						index: n,
						debug: J.value
					}), N.value = !0, e = !0;
				});
			}) : (l({
				componentName: "VueUiGauge",
				type: "datasetAttributeEmpty",
				property: "series",
				debug: J.value
			}), N.value = !0, e = !0))), N.value = e, Xt(M.value.value || 0), j.value.responsive) {
				let e = Se(() => {
					let { width: e, height: t } = Ce({
						chart: A.value,
						title: j.value.style.chart.title.text ? Ot.value : null,
						legend: kt.value,
						source: At.value,
						noTitle: jt.value
					});
					t -= 12, requestAnimationFrame(() => {
						R.value.width = e, R.value.height = t, R.value.centerX = e / 2, R.value.centerY = L.value / 2 / 358.4 * t, R.value.bottom = t, R.value.labelFontSize = 18 / L.value * Math.min(t, e) < 10 ? 10 : 18 / L.value * Math.min(t, e), R.value.legendFontSize = j.value.style.chart.legend.fontSize / L.value * Math.min(t, e) < 14 ? 14 : j.value.style.chart.legend.fontSize / L.value * Math.min(t, e), R.value.pointerRadius = j.value.style.chart.layout.pointer.circle.radius / L.value * Math.min(t, e), R.value.trackSize = j.value.style.chart.layout.track.size / L.value * Math.min(t, e), R.value.pointerStrokeWidth = ce({
							relator: Math.min(e, t),
							adjuster: L.value,
							source: j.value.style.chart.layout.pointer.strokeWidth,
							threshold: 2,
							fallback: 2
						}), R.value.markerOffset = ce({
							relator: Math.max(e, t),
							adjuster: L.value,
							source: j.value.style.chart.layout.markers.offsetY + 3,
							threshold: 2,
							fallback: 2
						}), R.value.segmentFontSize = ce({
							relator: Math.min(e, t),
							adjuster: L.value,
							source: j.value.style.chart.layout.segmentNames.fontSize,
							threshold: 8,
							fallback: 8
						});
					}), Jt();
				});
				G.value && (K.value && G.value.unobserve(K.value), G.value.disconnect()), G.value = new ResizeObserver(e), K.value = A.value.parentNode, G.value.observe(K.value);
			}
			Jt();
		}
		Ie(() => {
			Yt();
		}), Fe(() => {
			Gt(), G.value && (K.value && G.value.unobserve(K.value), G.value.disconnect());
		});
		function Xt(e) {
			let t = [];
			if ((I.value.series || []).forEach((e) => {
				t.push(e.from || 0), t.push(e.to || 0);
			}), z.value = Math.max(...t), B.value = Math.min(...t), Gt(), !V.value) {
				H.value = e;
				return;
			}
			let n = j.value.style.chart.animation.speed, r = Math.abs(e - H.value) / (n * 60);
			function i() {
				if (!V.value) {
					H.value = e, U = null;
					return;
				}
				H.value < e ? H.value = Math.min(H.value + r, e) : H.value > e && (H.value = Math.max(H.value - r, e)), U = H.value === e ? null : requestAnimationFrame(i);
			}
			i();
		}
		let Y = d(() => {
			let e = j.value.responsive ? Math.min(R.value.width, R.value.height) : R.value.width, t = 2.5 / j.value.style.chart.layout.radiusRatio;
			return {
				arcs: e / t,
				gradients: e / (t * 1.1),
				base: j.value.responsive ? R.value.height / 1.618 : R.value.height * .7,
				ratingBase: j.value.responsive ? R.value.height / 2 + R.value.height / 4 : R.value.height * .9,
				pointerSize: j.value.responsive ? Math.min(R.value.width, R.value.height) / 3 : R.value.width / 3.2
			};
		}), X = d(() => c({ series: I.value.series }, R.value.width / 2, Y.value.base, Y.value.arcs, Y.value.arcs, 1, 1, 1, 180, 109.9495, 40 * R.value.trackSize)), Zt = d(() => c({ series: I.value.series }, R.value.width / 2, Y.value.base, Y.value.arcs * j.value.style.chart.layout.segmentNames.offsetRatio, Y.value.arcs * j.value.style.chart.layout.segmentNames.offsetRatio, 1, 1, 1, 180, 109.9495, 40 * R.value.trackSize)), Z = d(() => X.value.map((e) => Y.value.arcs * j.value.style.chart.layout.segmentNames.offsetRatio * (e.nameOffsetRatio || 1))), Qt = d(() => {
			let { x: e, y: t } = a({
				initX: X.value[0].firstSeparator.x,
				initY: X.value[0].firstSeparator.y,
				centerX: W.value.x1,
				centerY: W.value.y1,
				offset: -j.value.style.chart.layout.segmentSeparators.offsetIn
			}), { x: n, y: r } = a({
				initX: X.value[0].startX,
				initY: X.value[0].startY,
				centerX: W.value.x1,
				centerY: W.value.y1,
				offset: j.value.style.chart.layout.segmentSeparators.offsetOut
			});
			return {
				x1: e,
				y1: t,
				x2: n,
				y2: r
			};
		}), $t = d(() => X.value.map((e) => {
			let { x: t, y: n } = a({
				initX: e.separator.x,
				initY: e.separator.y,
				centerX: W.value.x1,
				centerY: W.value.y1,
				offset: -j.value.style.chart.layout.segmentSeparators.offsetIn
			}), { x: r, y: i } = a({
				initX: e.endX,
				initY: e.endY,
				centerX: W.value.x1,
				centerY: W.value.y1,
				offset: j.value.style.chart.layout.segmentSeparators.offsetOut
			});
			return {
				x1: t,
				y1: n,
				x2: r,
				y2: i
			};
		}));
		function en(e) {
			if (e.reduce((e, t) => e + t, 0) > 100) throw Error("Total % must not exceed 100");
			let t = 0;
			return e.map((e) => (t += e, `${t / 100 * 50 - e / 4}%`));
		}
		let tn = d(() => en(X.value.map((e) => e.proportion * 100))), nn = d(() => c({ series: I.value.series }, R.value.width / 2, Y.value.base, Y.value.gradients, Y.value.gradients, .95, 1, 1, 180, 110.02, 2 * R.value.trackSize)), rn = d(() => {
			let e = B.value >= 0 ? -B.value : Math.abs(B.value);
			return ee({
				radius: j.value.style.chart.layout.indicatorArc.radius * R.value.trackSize,
				centerX: R.value.width / 2,
				centerY: Y.value.base,
				percentage: ne((H.value + e) / (z.value + e))
			});
		}), Q = x(!1);
		function an(e) {
			Q.value = e, Dt.value += 1;
		}
		let $ = x(!1);
		function on() {
			$.value = !$.value;
		}
		async function sn({ scale: e = 2 } = {}) {
			if (!A.value) return;
			let { width: t, height: n } = A.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ye({
				domElement: A.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: j.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let cn = d(() => j.value.style.chart.backgroundColor), ln = d(() => j.value.style.chart.title), { isCallbackImaging: un, isCallbackSvg: dn, generateSvg: fn, onGenerateImage: pn } = ve({
			svg: F,
			title: ln,
			legend: null,
			legendItems: null,
			backgroundColor: cn,
			getSvgCallback: () => j.value.userOptions.callbacks.svg,
			generateImage: Ht
		});
		async function mn() {
			if (wt("copyAlt", {
				config: j.value,
				dataset: I.value
			}), !j.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(j.value.userOptions.callbacks.altCopy({
				config: j.value,
				dataset: I.value
			}));
		}
		return xe({
			getImage: sn,
			generatePdf: Vt,
			generateImage: Ht,
			generateSvg: fn,
			toggleAnnotator: on,
			toggleFullscreen: an,
			copyAlt: mn
		}), (e, r) => (b(), m("div", {
			class: Ne(`vue-data-ui-component vue-ui-gauge ${Q.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			ref_key: "gaugeChart",
			ref: A,
			id: `vue-ui-gauge_${k.value}`,
			style: Pe(`font-family:${j.value.style.fontFamily};width:100%; text-align:center;background:${j.value.style.chart.backgroundColor};${j.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: r[0] ||= () => T(It)(!0),
			onMouseleave: r[1] ||= () => T(It)(!1)
		}, [
			j.value.userOptions.buttons.annotator ? (b(), f(T(_t), {
				key: 0,
				svgRef: T(F),
				backgroundColor: j.value.style.chart.backgroundColor,
				color: j.value.style.chart.color,
				active: $.value,
				isCursorPointer: Nt.value,
				onClose: on
			}, {
				"annotator-action-close": D(() => [C(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": D(({ color: t }) => [C(e.$slots, "annotator-action-color", y(_({ color: t })), void 0, !0)]),
				"annotator-action-draw": D(({ mode: t }) => [C(e.$slots, "annotator-action-draw", y(_({ mode: t })), void 0, !0)]),
				"annotator-action-undo": D(({ disabled: t }) => [C(e.$slots, "annotator-action-undo", y(_({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": D(({ disabled: t }) => [C(e.$slots, "annotator-action-redo", y(_({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": D(({ disabled: t }) => [C(e.$slots, "annotator-action-delete", y(_({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : p("", !0),
			Ut.value ? (b(), m("div", {
				key: 1,
				ref_key: "noTitle",
				ref: jt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : p("", !0),
			j.value.style.chart.title.text ? (b(), m("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: Ot,
				style: "width:100%;background:transparent;padding-bottom:12px"
			}, [(b(), f(be, {
				key: `title_${Mt.value}`,
				config: {
					title: {
						cy: "gauge-div-title",
						...j.value.style.chart.title
					},
					subtitle: {
						cy: "gauge-div-subtitle",
						...j.value.style.chart.title.subtitle
					}
				}
			}, {
				default: D(() => [j.value.translations.base && T(M).base ? (b(), m("span", Ve, w(j.value.translations.base) + ": " + w(T(M).base), 1)) : p("", !0)]),
				_: 1
			}, 8, ["config"]))], 512)) : p("", !0),
			j.value.userOptions.show && Tt.value && (T(Lt) || T(P)) ? (b(), f(T(vt), {
				ref_key: "details",
				ref: Et,
				key: `user_options_${Dt.value}`,
				backgroundColor: j.value.style.chart.backgroundColor,
				color: j.value.style.chart.color,
				isImaging: T(Bt),
				isPrinting: T(zt),
				uid: k.value,
				hasXls: !1,
				hasPdf: j.value.userOptions.buttons.pdf,
				hasImg: j.value.userOptions.buttons.img,
				hasSvg: j.value.userOptions.buttons.svg,
				hasFullscreen: j.value.userOptions.buttons.fullscreen,
				hasAltCopy: j.value.userOptions.buttons.altCopy,
				isFullscreen: Q.value,
				titles: { ...j.value.userOptions.buttonTitles },
				chartElement: A.value,
				callbacks: j.value.userOptions.callbacks,
				printScale: j.value.userOptions.print.scale,
				position: j.value.userOptions.position,
				hasAnnotator: j.value.userOptions.buttons.annotator,
				isAnnotation: $.value,
				isCursorPointer: Nt.value,
				onToggleFullscreen: an,
				onGeneratePdf: T(Vt),
				onGenerateImage: T(pn),
				onGenerateSvg: T(fn),
				onToggleAnnotator: on,
				onCopyAlt: mn,
				style: Pe({ visibility: T(Lt) ? T(P) ? "visible" : "hidden" : "visible" })
			}, Ae({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: D(({ isOpen: t, color: n }) => [C(e.$slots, "menuIcon", y(_({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: D(() => [C(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: D(() => [C(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: D(() => [C(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: D(({ toggleFullscreen: t, isFullscreen: n }) => [C(e.$slots, "optionFullscreen", y(_({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: D(({ toggleAnnotator: t, isAnnotator: n }) => [C(e.$slots, "optionAnnotator", y(_({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: D(({ altCopy: t }) => [C(e.$slots, "optionAltCopy", y(_({ altCopy: t })), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: D(() => [C(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: D(() => [C(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "8"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isImaging",
				"isPrinting",
				"uid",
				"hasPdf",
				"hasImg",
				"hasSvg",
				"hasFullscreen",
				"hasAltCopy",
				"isFullscreen",
				"titles",
				"chartElement",
				"callbacks",
				"printScale",
				"position",
				"hasAnnotator",
				"isAnnotation",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : p("", !0),
			(b(), m("svg", {
				ref_key: "svgRef",
				ref: F,
				xmlns: T(le),
				class: Ne({
					"vue-data-ui-fullscreen--on": Q.value,
					"vue-data-ui-fulscreen--off": !Q.value
				}),
				viewBox: `0 0 ${R.value.width <= 0 ? 10 : R.value.width} ${R.value.height <= 0 ? 10 : R.value.height}`,
				style: Pe(`max-width:100%;overflow:hidden !important;background:transparent;color:${j.value.style.chart.color}`)
			}, [
				je(T(yt)),
				e.$slots["chart-background"] ? (b(), m("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: R.value.width <= 0 ? 10 : R.value.width,
					height: R.value.height <= 0 ? 10 : R.value.height,
					style: { pointerEvents: "none" }
				}, [C(e.$slots, "chart-background", {}, void 0, !0)], 8, Ue)) : p("", !0),
				h("defs", null, [je(we, {
					t: "radial",
					id: `gradient_${k.value}`,
					cx: "50%",
					cy: "50%",
					r: "50%",
					fx: "50%",
					fy: "50%",
					stops: [
						[
							"0%",
							T(t)("#FFFFFF", 1),
							1
						],
						[
							"80%",
							T(t)("#FFFFFF", j.value.style.chart.layout.track.gradientIntensity),
							1
						],
						[
							"100%",
							T(t)("#FFFFFF", 1),
							1
						]
					]
				}, null, 8, ["id", "stops"])]),
				h("defs", null, [h("filter", {
					id: `blur_${k.value}`,
					x: "-50%",
					y: "-50%",
					width: "200%",
					height: "200%"
				}, [h("feGaussianBlur", {
					in: "SourceGraphic",
					stdDeviation: 100 / j.value.style.chart.layout.track.gradientIntensity
				}, null, 8, Ge)], 8, We)]),
				e.$slots.pattern ? (b(), m("g", Ke, [(b(!0), m(u, null, S(X.value, (t, n) => (b(), m("defs", null, [C(e.$slots, "pattern", v({ ref_for: !0 }, {
					seriesIndex: n,
					patternId: `pattern_${k.value}_${n}`
				}), void 0, !0)]))), 256))])) : p("", !0),
				(b(!0), m(u, null, S(X.value, (e, t) => (b(), m("path", {
					key: `arc_${t}`,
					d: e.arcSlice,
					fill: e.color,
					stroke: j.value.style.chart.backgroundColor,
					"stroke-linecap": "round"
				}, null, 8, qe))), 128)),
				e.$slots.pattern ? (b(!0), m(u, { key: 2 }, S(X.value, (e, t) => (b(), m("path", {
					key: `arc_${t}`,
					d: e.arcSlice,
					fill: `url(#pattern_${k.value}_${t})`,
					stroke: j.value.style.chart.backgroundColor,
					"stroke-linecap": "round"
				}, null, 8, Je))), 128)) : p("", !0),
				j.value.style.chart.layout.indicatorArc.show ? (b(), m("path", {
					key: 3,
					d: rn.value,
					fill: j.value.style.chart.layout.indicatorArc.fill
				}, null, 8, Ye)) : p("", !0),
				j.value.style.chart.layout.segmentNames.show && j.value.style.chart.layout.segmentNames.curved ? (b(), m(u, { key: 4 }, [(b(!0), m(u, null, S(X.value, (e, t) => (b(), m("path", {
					id: `curve_${k.value}_${t}`,
					d: `M ${W.value.x1},${W.value.y1} m -${Z.value[t]},0 a ${Z.value[t]},${Z.value[t]} 0 1,1 ${2 * Z.value[t]},0 a ${Z.value[t]},${Z.value[t]} 0 1,1 -${2 * Z.value[t]},0`,
					fill: "transparent"
				}, null, 8, Xe))), 256)), (b(!0), m(u, null, S(X.value, (e, t) => (b(), m("text", {
					fill: j.value.style.chart.layout.segmentNames.useSerieColor ? e.color : j.value.style.chart.layout.segmentNames.color,
					"font-size": q.value,
					"font-weight": j.value.style.chart.layout.segmentNames.bold ? "bold" : "normal",
					"text-anchor": "middle"
				}, [h("textPath", {
					href: `#curve_${k.value}_${t}`,
					startOffset: tn.value[t]
				}, w(e.name || ""), 9, Qe)], 8, Ze))), 256))], 64)) : p("", !0),
				j.value.style.chart.layout.segmentNames.show && !j.value.style.chart.layout.segmentNames.curved ? (b(!0), m(u, { key: 5 }, S(Zt.value, (e, t) => (b(), m("text", {
					class: "vue-ui-gauge-label-flat",
					"text-anchor": T(re)(e, !1, 12).anchor,
					fill: j.value.style.chart.layout.segmentNames.useSerieColor ? e.color : j.value.style.chart.layout.segmentNames.color,
					"font-size": q.value,
					"font-weight": j.value.style.chart.layout.segmentNames.bold ? "bold" : "normal",
					innerHTML: T(n)({
						content: String(e.name ?? ""),
						fontSize: q.value,
						fill: j.value.style.chart.layout.segmentNames.useSerieColor ? e.color : j.value.style.chart.layout.segmentNames.color,
						x: e.center.endX,
						y: e.center.endY
					})
				}, null, 8, $e))), 256)) : p("", !0),
				j.value.style.chart.layout.track.useGradient ? (b(!0), m(u, { key: 6 }, S(nn.value, (e, t) => (b(), m("path", {
					key: `arc_${t}`,
					d: e.arcSlice,
					fill: "#FFFFFF",
					stroke: "none",
					"stroke-linecap": "round",
					filter: `url(#blur_${k.value})`
				}, null, 8, et))), 128)) : p("", !0),
				j.value.style.chart.layout.segmentSeparators.show ? (b(), m(u, { key: 7 }, [
					h("line", v(Qt.value, {
						stroke: j.value.style.chart.backgroundColor,
						"stroke-width": j.value.style.chart.layout.segmentSeparators.strokeWidth + 2,
						"stroke-linecap": "round"
					}), null, 16, tt),
					h("line", v(Qt.value, {
						stroke: j.value.style.chart.layout.segmentSeparators.stroke,
						"stroke-width": j.value.style.chart.layout.segmentSeparators.strokeWidth,
						"stroke-linecap": "round"
					}), null, 16, nt),
					(b(!0), m(u, null, S($t.value, (e) => (b(), m("line", v({ ref_for: !0 }, e, {
						stroke: j.value.style.chart.backgroundColor,
						"stroke-width": j.value.style.chart.layout.segmentSeparators.strokeWidth + 2,
						"stroke-linecap": "round"
					}), null, 16, rt))), 256)),
					(b(!0), m(u, null, S($t.value, (e) => (b(), m("line", v({ ref_for: !0 }, e, {
						stroke: j.value.style.chart.layout.segmentSeparators.stroke,
						"stroke-width": j.value.style.chart.layout.segmentSeparators.strokeWidth,
						"stroke-linecap": "round"
					}), null, 16, it))), 256))
				], 64)) : p("", !0),
				j.value.style.chart.layout.markers.show ? (b(), m("g", at, [(b(!0), m(u, null, S(X.value, (e, t) => (b(), m("text", {
					x: T(a)({
						centerX: W.value.x1,
						centerY: Y.value.base,
						initX: e.center.startX,
						initY: e.center.startY,
						offset: R.value.markerOffset
					}).x,
					y: T(a)({
						centerX: W.value.x1,
						centerY: Y.value.base,
						initX: e.center.startX,
						initY: e.center.startY,
						offset: R.value.markerOffset
					}).y,
					"text-anchor": e.center.startX < W.value.x1 - 5 ? "end" : e.center.startX > W.value.x1 + 5 ? "start" : "middle",
					"font-size": R.value.labelFontSize * j.value.style.chart.layout.markers.fontSizeRatio,
					"font-weight": `${j.value.style.chart.layout.markers.bold ? "bold" : "normal"}`,
					fill: j.value.style.chart.layout.markers.color
				}, w(T(ie)(j.value.style.chart.layout.markers.formatter, e.from, T(s)({
					p: j.value.style.chart.layout.markers.prefix,
					v: e.from,
					s: j.value.style.chart.layout.markers.suffix,
					r: j.value.style.chart.layout.markers.roundingValue
				}))), 9, ot))), 256))])) : p("", !0),
				j.value.style.chart.layout.markers.show ? (b(), m("text", {
					key: 9,
					x: T(a)({
						centerX: R.value.width / 2,
						centerY: Y.value.base,
						initX: X.value.at(-1).endX,
						initY: X.value.at(-1).endY,
						offset: R.value.markerOffset
					}).x,
					y: T(a)({
						centerX: R.value.width / 2,
						centerY: Y.value.base,
						initX: X.value.at(-1).endX,
						initY: X.value.at(-1).endY,
						offset: R.value.markerOffset
					}).y,
					"text-anchor": "start",
					"font-size": R.value.labelFontSize * j.value.style.chart.layout.markers.fontSizeRatio,
					"font-weight": `${j.value.style.chart.layout.markers.bold ? "bold" : "normal"}`,
					fill: j.value.style.chart.layout.markers.color
				}, w(T(ie)(j.value.style.chart.layout.markers.formatter, z.value, T(s)({
					p: j.value.style.chart.layout.markers.prefix,
					v: z.value,
					s: j.value.style.chart.layout.markers.suffix,
					r: j.value.style.chart.layout.markers.roundingValue
				}))), 9, st)) : p("", !0),
				j.value.style.chart.layout.pointer.show ? (b(), m(u, { key: 10 }, [j.value.style.chart.layout.pointer.type === "rounded" ? (b(), m("g", ct, [
					isNaN(W.value.x2) ? p("", !0) : (b(), m("line", {
						key: 0,
						x1: W.value.x1,
						y1: W.value.y1,
						x2: W.value.x2,
						y2: W.value.y2,
						stroke: j.value.style.chart.layout.pointer.stroke,
						"stroke-width": R.value.pointerStrokeWidth,
						"stroke-linecap": "round"
					}, null, 8, lt)),
					isNaN(W.value.x2) ? p("", !0) : (b(), m("line", {
						key: 1,
						x1: W.value.x1,
						y1: W.value.y1,
						x2: W.value.x2,
						y2: W.value.y2,
						stroke: j.value.style.chart.layout.pointer.useRatingColor ? qt.value : j.value.style.chart.layout.pointer.color,
						"stroke-linecap": "round",
						"stroke-width": R.value.pointerStrokeWidth * .7
					}, null, 8, ut)),
					!isNaN(W.value.x2) && j.value.style.chart.layout.track.useGradient ? (b(), m("line", {
						key: 2,
						x1: W.value.x1,
						y1: W.value.y1,
						x2: W.value.x2,
						y2: W.value.y2,
						stroke: "white",
						"stroke-linecap": "round",
						"stroke-width": R.value.pointerStrokeWidth * .3,
						filter: `url(#blur_${k.value})`
					}, null, 8, dt)) : p("", !0)
				])) : (b(), m("g", ft, [Kt.value ? (b(), m("path", {
					key: 0,
					d: Kt.value,
					fill: j.value.style.chart.layout.pointer.useRatingColor ? qt.value : j.value.style.chart.layout.pointer.color,
					stroke: j.value.style.chart.layout.pointer.stroke,
					"stroke-width": j.value.style.chart.layout.pointer.circle.strokeWidth,
					"stroke-linejoin": "round"
				}, null, 8, pt)) : p("", !0)])), h("circle", {
					cx: R.value.width / 2,
					cy: Y.value.base,
					fill: j.value.style.chart.layout.pointer.circle.color,
					r: R.value.pointerRadius <= 0 ? 1e-4 : R.value.pointerRadius,
					"stroke-width": j.value.style.chart.layout.pointer.circle.strokeWidth,
					stroke: j.value.style.chart.layout.pointer.circle.stroke
				}, null, 8, mt)], 64)) : p("", !0),
				j.value.style.chart.legend.show ? (b(), m("text", {
					key: 11,
					x: R.value.width / 2,
					y: Y.value.ratingBase,
					"text-anchor": "middle",
					"font-size": R.value.legendFontSize,
					"font-weight": "bold",
					fill: j.value.style.chart.legend.useRatingColor ? qt.value : j.value.style.chart.legend.color
				}, w(T(ie)(j.value.style.chart.legend.formatter, H.value, T(s)({
					p: j.value.style.chart.legend.prefix + (j.value.style.chart.legend.showPlusSymbol && H.value > 0 ? "+" : ""),
					v: H.value,
					s: j.value.style.chart.legend.suffix,
					r: j.value.style.chart.legend.roundingValue
				}))), 9, ht)) : p("", !0),
				C(e.$slots, "svg", { svg: {
					...R.value,
					isPrintingImg: T(zt) || T(Bt) || T(un),
					isPrintingSvg: T(dn)
				} }, void 0, !0)
			], 14, He)),
			e.$slots.watermark ? (b(), m("div", gt, [C(e.$slots, "watermark", y(_({ isPrinting: T(zt) || T(Bt) || T(un) || T(dn) })), void 0, !0)])) : p("", !0),
			h("div", {
				ref_key: "chartLegend",
				ref: kt
			}, [C(e.$slots, "legend", { legend: I.value }, void 0, !0)], 512),
			e.$slots.source ? (b(), m("div", {
				key: 5,
				ref_key: "source",
				ref: At,
				dir: "auto"
			}, [C(e.$slots, "source", {}, void 0, !0)], 512)) : p("", !0),
			C(e.$slots, "skeleton", {}, () => [T(Ft) ? (b(), f(he, { key: 0 })) : p("", !0)], !0)
		], 46, Be));
	}
}, [["__scopeId", "data-v-27f19797"]]);
//#endregion
export { ze as n, _t as t };
