import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Jt as t, Ot as n, S as r, X as i, _ as a, i as o, jt as s, pt as ee, q as te, t as ne, tt as re } from "./lib-Bttd6u5E.js";
import { n as ie } from "./useHints-Dq_w2E8B.js";
import { t as ae } from "./useConfig-DlNpz6P8.js";
import { t as oe } from "./usePrinter-DN5bYhTG.js";
import { n as se, t as ce } from "./BaseScanner-DZvpgOjM.js";
import { t as c } from "./useNestedProp-vPNvh7rV.js";
import { t as le } from "./useThemeCheck-C43Tcqmk.js";
import { t as ue } from "./useChartExport-DNiwdPmb.js";
import { t as de } from "./img-Bnokohej.js";
import { n as fe } from "./Title-BE3qg9xl.js";
import { t as pe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as me, t as he } from "./useResponsive-ZtArZtUf.js";
import { t as ge } from "./useUserOptionState-DK-_1ddE.js";
import { t as _e } from "./useChartAccessibility-DYqac8yF.js";
import { t as ve } from "./Legend-CQxUgOd-.js";
import { t as ye } from "./vue_ui_bullet-ClzdLoOv.js";
import { Fragment as l, Teleport as be, computed as u, createBlock as d, createCommentVNode as f, createElementBlock as p, createElementVNode as m, createSlots as xe, createVNode as Se, defineAsyncComponent as h, guardReactiveProps as g, normalizeClass as Ce, normalizeProps as _, normalizeStyle as v, onBeforeUnmount as we, onMounted as Te, openBlock as y, ref as b, renderList as x, renderSlot as S, toDisplayString as C, toRefs as Ee, unref as w, watch as De, withCtx as T } from "vue";
//#region src/components/vue-ui-bullet.vue
var Oe = /* @__PURE__ */ e({ default: () => E }), ke = ["id"], Ae = {
	key: 1,
	ref: "noTitle",
	class: "vue-data-ui-no-title-space",
	style: "height:36px; width: 100%;background:transparent"
}, je = ["id"], Me = [
	"xmlns",
	"viewBox",
	"aria-labelledby",
	"aria-describedby"
], Ne = ["id"], Pe = ["id"], Fe = ["width", "height"], Ie = { key: 1 }, Le = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke"
], Re = [
	"x",
	"y",
	"height",
	"width",
	"rx",
	"fill",
	"stroke",
	"stroke-width"
], ze = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke",
	"stroke-width"
], Be = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], Ve = [
	"x",
	"y",
	"height",
	"width",
	"rx",
	"fill",
	"stroke",
	"stroke-width"
], He = { key: 3 }, Ue = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], We = { key: 4 }, Ge = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke"
], Ke = {
	key: 4,
	class: "vue-data-ui-watermark"
}, qe = ["id"], Je = {
	key: 0,
	class: "vue-ui-bullet-legend-item",
	dir: "auto"
}, Ye = { style: { "margin-right": "2px" } }, E = /*#__PURE__*/ pe({
	__name: "vue-ui-bullet",
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
	setup(e, { expose: pe, emit: Oe }) {
		let E = h(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Xe = h(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Ze = h(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), { vue_ui_bullet: Qe } = ae(), { isThemeValid: $e, warnInvalidTheme: et } = le(), D = e, tt = Oe, O = b(null), nt = b(null), rt = b(0), it = b(null), k = b(null), at = b(0), A = b(null), j = b(null), ot = b(!1), M = u({
			get: () => z.value.hasOwnProperty("value"),
			set: (e) => e
		}), N = b(L()), P = u(() => N.value.debug), F = u(() => z.value.segments ? Array.isArray(z.value.segments) ? z.value.segments.length ? !0 : (P.value && console.warn("VueUiBullet: dataset segments is empty. Provide segments with this datastructure:\n\n    segments: [\n        {\n            name: string;\n            from: number;\n            to: number;\n            color?: string;\n        },\n        {...}\n    ]\n            "), M.value = !1, !1) : (P.value && console.warn("VueUiBullet: dataset segments must be an array of objects with this datastructure:\n\n    segments: [\n        {\n            name: string;\n            from: number;\n            to: number;\n            color?: string;\n        },\n        {...}\n    ] \n            "), M.value = !1, !1) : (P.value && console.warn("VueUiBullet: dataset segments is empty. Provide segments with this datastructure:\n\n    segments: [\n        {\n            name: string;\n            from: number;\n            to: number;\n            color?: string;\n        },\n        {...}\n    ]\n            "), M.value = !1, !1)), st = u(() => {
			let { top: e, right: t, bottom: n, left: r } = N.value.style.chart.padding;
			return {
				top: e,
				right: t,
				bottom: n,
				left: r
			};
		});
		function ct() {
			if (s(z.value) ? (re({
				componentName: "VueUiBullet",
				type: "dataset",
				debug: P.value
			}), B.value = !0) : F.value ? z.value.segments.forEach((e, t) => {
				ee({
					datasetObject: e,
					requiredAttributes: [
						"name",
						"from",
						"to"
					]
				}).forEach((e) => {
					M.value = !1, re({
						componentName: "VueUiBullet segment",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: P.value
					});
				});
			}) : (M.value = !1, B.value = !0), s(z.value) || (B.value = N.value.loading), N.value.responsive) {
				let e = me(() => {
					let { width: e, height: t } = he({
						chart: O.value,
						title: N.value.style.chart.title.text ? nt.value : null,
						legend: N.value.style.chart.legend.show ? it.value : null,
						source: k.value,
						padding: st.value
					}), n = (N.value.style.chart.legend.show ? 24 : 0) || 12;
					requestAnimationFrame(() => {
						U.value.width = e, U.value.height = t - n;
					});
				});
				A.value && (j.value && A.value.unobserve(j.value), A.value.disconnect()), A.value = new ResizeObserver(e), j.value = O.value.parentNode, A.value.observe(j.value);
			}
			N.value.style.chart.animation.show && !R.value && vt(z.value.value || 0);
		}
		Te(() => {
			ot.value = !0, ct();
		});
		let I = b(te());
		function L() {
			let e = c({
				userConfig: D.config,
				defaultConfig: Qe
			}), t = e.theme;
			if (!t) return e;
			if (!$e.value(e)) return et(e), e;
			let n = c({
				userConfig: ye[t] || D.config,
				defaultConfig: e
			});
			return c({
				userConfig: D.config,
				defaultConfig: n
			});
		}
		ie({
			config: () => N.value,
			dataset: () => D.dataset,
			component: "VueUiBullet",
			rules: [{
				test: (e) => e?.segments && e?.segments.length > 8,
				message: [
					"👀 The number of target segments is > 8, which can make the chart hard to read and a large legend. Consider:",
					"",
					"▶️ Using broader segments."
				]
			}]
		});
		let lt = u(() => N.value.userOptions.useCursorPointer), ut = u(() => t({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					segments: {
						dataLabels: { show: !1 },
						ticks: { stroke: "#8A8A8A" }
					},
					valueBar: { label: { show: !1 } }
				} }
			},
			userConfig: N.value.skeletonConfig ?? {}
		})), { loading: R, FINAL_DATASET: z, manualLoading: B } = se({
			...Ee(D),
			FINAL_CONFIG: N,
			prepareConfig: L,
			skeletonDataset: D.config?.skeletonDataset ?? {
				value: 100,
				target: 100,
				segments: [
					{
						name: "",
						from: 0,
						to: 33,
						color: "#AAAAAA"
					},
					{
						name: "",
						from: 33,
						to: 66,
						color: "#BABABA"
					},
					{
						name: "",
						from: 66,
						to: 100,
						color: "#CACACA"
					}
				]
			},
			skeletonConfig: t({
				defaultConfig: N.value,
				userConfig: ut.value
			})
		}), { userOptionsVisible: V, setUserOptionsVisibility: dt, keepUserOptionState: ft } = ge({ config: N.value }), { svgRef: H } = _e({ config: N.value.style.chart.title }), U = b({
			width: N.value.style.chart.width,
			height: N.value.style.chart.height
		}), pt = u(() => U.value.width), mt = u(() => U.value.height);
		De(() => D.config, (e) => {
			R.value || (N.value = L()), V.value = !N.value.userOptions.showOnChartHover, U.value.width = N.value.style.chart.width, U.value.height = N.value.style.chart.height, ct(), rt.value += 1;
		}, { deep: !0 });
		let W = u(() => {
			let e = mt.value, t = pt.value, n = N.value.style.chart.padding.left, r = t - N.value.style.chart.padding.right, i = N.value.style.chart.padding.top, a = e - N.value.style.chart.padding.bottom;
			return {
				height: Math.max(.001, e),
				width: Math.max(.001, t),
				left: n,
				right: r,
				top: i,
				bottom: a,
				chartWidth: Math.max(.001, r - n),
				chartHeight: Math.max(.001, a - i)
			};
		}), ht = u(() => {
			if (!F.value) return [];
			let e = [];
			for (let t = 0; t < z.value.segments.length; t += 1) e.push(n(N.value.style.chart.segments.baseColor, t / z.value.segments.length));
			return e;
		}), G = u(() => F.value ? {
			min: Math.min(...z.value.segments.map((e) => e.from)),
			max: Math.max(...z.value.segments.map((e) => e.to))
		} : {
			min: 0,
			max: 1
		}), K = b(gt());
		De(() => z.value, (e) => {
			e.hasOwnProperty("value") && (B.value = !1), N.value.style.chart.animation.show && !R.value ? vt(e.value || 0) : K.value = e.value || 0;
		}, { deep: !0 });
		function gt() {
			return N.value.style.chart.animation.show && !R.value ? G.value.min : z.value.value || 0;
		}
		let _t = b(null);
		function vt(e) {
			let t = Math.abs(e - K.value) / N.value.style.chart.animation.animationFrames;
			function n() {
				K.value < e ? K.value = Math.min(K.value + t, e) : K.value > e && (K.value = Math.max(K.value - t, e)), K.value !== e && (_t.value = requestAnimationFrame(n));
			}
			n();
		}
		we(() => {
			cancelAnimationFrame(_t.value);
		});
		let q = u(() => {
			if (!F.value) return [];
			let e = a(G.value.min, G.value.max, N.value.style.chart.segments.ticks.divisions), t = e.min >= 0 ? 0 : Math.abs(e.min);
			return {
				scale: e,
				target: { x: W.value.left + (z.value.target + t) / (e.max + t) * W.value.chartWidth - N.value.style.chart.target.width / 2 },
				value: { width: (K.value + t) / (e.max + t) * W.value.chartWidth },
				ticks: e.ticks.map((n) => ({
					value: n,
					y: W.value.bottom + N.value.style.chart.segments.dataLabels.fontSize + 3 + N.value.style.chart.segments.dataLabels.offsetY,
					x: W.value.left + (n + t) / (e.max + t) * W.value.chartWidth
				})),
				chunks: z.value.segments.map((n, i) => ({
					...n,
					color: n.color ? r(n.color) : ht.value[i],
					x: W.value.left + W.value.chartWidth * ((n.from + t) / (e.max + t)),
					y: W.value.top,
					height: W.value.chartHeight,
					width: W.value.chartWidth * (Math.abs(n.to - n.from) / (e.max + t))
				}))
			};
		}), J = u(() => !q.value || !q.value.chunks || !q.value.chunks.length ? [] : q.value.chunks.map((e) => {
			let t = `${o(N.value.style.chart.segments.dataLabels.formatter, e.from, i({
				p: N.value.style.chart.segments.dataLabels.prefix,
				v: e.from,
				s: N.value.style.chart.segments.dataLabels.suffix,
				r: N.value.style.chart.segments.dataLabels.rounding
			}))} — ${o(N.value.style.chart.segments.dataLabels.formatter, e.to, i({
				p: N.value.style.chart.segments.dataLabels.prefix,
				v: e.to,
				s: N.value.style.chart.segments.dataLabels.suffix,
				r: N.value.style.chart.segments.dataLabels.rounding
			}))}`;
			return {
				...e,
				shape: "square",
				value: t,
				display: `${e.name}: ${t}`
			};
		})), yt = u(() => ({
			cy: "bullet-div-legend",
			backgroundColor: "transparent",
			color: N.value.style.chart.legend.color,
			fontSize: N.value.style.chart.legend.fontSize,
			paddingBottom: 6,
			fontWeight: N.value.style.chart.legend.bold ? "bold" : ""
		})), { isPrinting: Y, isImaging: X, generatePdf: bt, generateImage: xt } = oe({
			elementId: `bullet_${I.value}`,
			fileName: N.value.style.chart.title.text || "vue-ui-bullet",
			options: N.value.userOptions.print
		}), St = u(() => N.value.userOptions.show && !N.value.style.chart.title.text), Z = b(!1);
		function Ct(e) {
			Z.value = e, at.value += 1;
		}
		function wt() {
			return q.value;
		}
		let Q = b(!1);
		function $() {
			Q.value = !Q.value;
		}
		async function Tt({ scale: e = 2 } = {}) {
			if (!O.value) return;
			let { width: t, height: n } = O.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await de({
				domElement: O.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: N.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Et = u(() => J.value.map((e) => ({
			...e,
			name: e.display
		}))), Dt = u(() => N.value.style.chart.backgroundColor), Ot = u(() => N.value.style.chart.legend), kt = u(() => N.value.style.chart.title), { isCallbackImaging: At, isCallbackSvg: jt, generateSvg: Mt, onGenerateImage: Nt } = ue({
			svg: H,
			title: kt,
			legend: Ot,
			legendItems: Et,
			backgroundColor: Dt,
			getSvgCallback: () => N.value.userOptions.callbacks.svg,
			generateImage: xt
		});
		async function Pt() {
			if (tt("copyAlt", {
				config: N.value,
				dataset: z.value
			}), !N.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(N.value.userOptions.callbacks.altCopy({
				config: N.value,
				dataset: z.value
			}));
		}
		return pe({
			getData: wt,
			getImage: Tt,
			generatePdf: bt,
			generateImage: xt,
			generateSvg: Mt,
			toggleAnnotator: $,
			toggleFullscreen: Ct,
			copyAlt: Pt
		}), (e, t) => (y(), p("div", {
			ref_key: "bulletChart",
			ref: O,
			class: Ce(`vue-data-ui-component vue-ui-bullet ${Z.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			style: v(`font-family:${N.value.style.fontFamily};width:100%;background:${N.value.style.chart.backgroundColor};${N.value.responsive ? "height:100%" : ""}`),
			id: `bullet_${I.value}`,
			onMouseenter: t[0] ||= () => w(dt)(!0),
			onMouseleave: t[1] ||= () => w(dt)(!1)
		}, [
			N.value.userOptions.buttons.annotator ? (y(), d(w(Xe), {
				key: 0,
				svgRef: w(H),
				backgroundColor: N.value.style.chart.backgroundColor,
				color: N.value.style.chart.color,
				active: Q.value,
				isCursorPointer: lt.value,
				onClose: $
			}, {
				"annotator-action-close": T(() => [S(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": T(({ color: t }) => [S(e.$slots, "annotator-action-color", _(g({ color: t })), void 0, !0)]),
				"annotator-action-draw": T(({ mode: t }) => [S(e.$slots, "annotator-action-draw", _(g({ mode: t })), void 0, !0)]),
				"annotator-action-undo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-undo", _(g({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-redo", _(g({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": T(({ disabled: t }) => [S(e.$slots, "annotator-action-delete", _(g({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : f("", !0),
			St.value ? (y(), p("div", Ae, null, 512)) : f("", !0),
			N.value.style.chart.title.text ? (y(), p("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: nt,
				style: "width:100%;background:transparent;"
			}, [(y(), d(fe, {
				lineHeight: "1.3rem",
				key: `title_${rt.value}`,
				config: {
					title: {
						cy: "bullet-div-title",
						...N.value.style.chart.title
					},
					subtitle: {
						cy: "bullet-div-subtitle",
						...N.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : f("", !0),
			m("div", { id: `legend-top-${I.value}` }, null, 8, je),
			N.value.userOptions.show && M.value && (w(ft) || w(V)) ? (y(), d(w(Ze), {
				key: 3,
				ref: "details",
				backgroundColor: N.value.style.chart.backgroundColor,
				color: N.value.style.chart.color,
				isPrinting: w(Y),
				isImaging: w(X),
				uid: I.value,
				hasTooltip: !1,
				hasPdf: N.value.userOptions.buttons.pdf,
				hasImg: N.value.userOptions.buttons.img,
				hasSvg: N.value.userOptions.buttons.svg,
				hasXls: !1,
				hasTable: !1,
				hasLabel: !1,
				hasFullscreen: N.value.userOptions.buttons.fullscreen,
				hasAltCopy: N.value.userOptions.buttons.altCopy,
				isFullscreen: Z.value,
				chartElement: O.value,
				position: N.value.userOptions.position,
				titles: { ...N.value.userOptions.buttonTitles },
				hasAnnotator: N.value.userOptions.buttons.annotator,
				isAnnotation: Q.value,
				callbacks: N.value.userOptions.callbacks,
				printScale: N.value.userOptions.print.scale,
				isCursorPointer: lt.value,
				onToggleFullscreen: Ct,
				onGeneratePdf: w(bt),
				onGenerateImage: w(Nt),
				onGenerateSvg: w(Mt),
				onToggleAnnotator: $,
				onCopyAlt: Pt,
				style: v({ visibility: w(ft) ? w(V) ? "visible" : "hidden" : "visible" })
			}, xe({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: T(({ isOpen: t, color: n }) => [S(e.$slots, "menuIcon", _(g({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: T(() => [S(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: T(() => [S(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: T(() => [S(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: T(({ toggleFullscreen: t, isFullscreen: n }) => [S(e.$slots, "optionFullscreen", _(g({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: T(({ toggleAnnotator: t, isAnnotator: n }) => [S(e.$slots, "optionAnnotator", _(g({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: T(({ altCopy: t }) => [S(e.$slots, "optionAltCopy", _(g({ altCopy: t })), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: T(() => [S(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: T(() => [S(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "8"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"hasPdf",
				"hasImg",
				"hasSvg",
				"hasFullscreen",
				"hasAltCopy",
				"isFullscreen",
				"chartElement",
				"position",
				"titles",
				"hasAnnotator",
				"isAnnotation",
				"callbacks",
				"printScale",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : f("", !0),
			(y(), p("svg", {
				ref_key: "svgRef",
				ref: H,
				xmlns: w(ne),
				class: Ce({
					"vue-data-ui-fullscreen--on": Z.value,
					"vue-data-ui-fulscreen--off": !Z.value,
					"vue-ui-bullet-svg": !0
				}),
				viewBox: `0 0 ${W.value.width} ${W.value.height}`,
				style: v(`width: 100%; overflow: visible; background:transparent;color:${N.value.style.chart.color}`),
				"aria-labelledby": `bullet-svg-title-${I.value}`,
				"aria-describedby": `bullet-svg-desc-${I.value}`
			}, [
				Se(w(E)),
				m("title", { id: `bullet-svg-title-${I.value}` }, C(N.value.style.chart.title.text || "Bullet chart"), 9, Ne),
				m("desc", { id: `bullet-svg-desc-${I.value}` }, " Value: " + C(K.value) + ", Target: " + C(q.value.target?.value), 9, Pe),
				e.$slots["chart-background"] ? (y(), p("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: W.value.width,
					height: W.value.height,
					style: { pointerEvents: "none" }
				}, [S(e.$slots, "chart-background", {}, void 0, !0)], 8, Fe)) : f("", !0),
				F.value ? (y(), p("g", Ie, [
					(y(!0), p(l, null, x(q.value.chunks, (e) => (y(), p("rect", {
						x: e.x,
						y: e.y,
						height: e.height,
						width: e.width,
						fill: e.color,
						"stroke-width": 1,
						stroke: N.value.style.chart.backgroundColor,
						style: { transition: "x 0.3s ease-in-out, width 0.3s ease-in-out" }
					}, null, 8, Le))), 256)),
					!N.value.style.chart.target.onTop && N.value.style.chart.target.show ? (y(), p("rect", {
						key: 0,
						x: q.value.target.x,
						y: W.value.top + (W.value.chartHeight - W.value.chartHeight * N.value.style.chart.target.heightRatio) / 2,
						height: W.value.chartHeight * N.value.style.chart.target.heightRatio,
						width: N.value.style.chart.target.width,
						rx: N.value.style.chart.target.rounded ? N.value.style.chart.target.width / 2 : 0,
						fill: N.value.style.chart.target.color,
						stroke: N.value.style.chart.target.stroke,
						"stroke-width": N.value.style.chart.target.strokeWidth
					}, null, 8, Re)) : f("", !0),
					m("rect", {
						x: W.value.left,
						y: W.value.top + (W.value.chartHeight - W.value.chartHeight * N.value.style.chart.valueBar.heightRatio) / 2,
						height: W.value.chartHeight * N.value.style.chart.valueBar.heightRatio,
						width: q.value.value.width,
						fill: N.value.style.chart.valueBar.color,
						stroke: N.value.style.chart.valueBar.stroke,
						"stroke-width": N.value.style.chart.valueBar.strokeWidth
					}, null, 8, ze),
					N.value.style.chart.valueBar.label.show ? (y(), p("text", {
						key: 1,
						x: W.value.left + q.value.value.width,
						y: W.value.top - 6 + N.value.style.chart.valueBar.label.offsetY,
						"font-size": N.value.style.chart.valueBar.label.fontSize,
						"font-weight": N.value.style.chart.valueBar.label.bold ? "bold" : "normal",
						fill: N.value.style.chart.valueBar.label.color,
						"text-anchor": "middle"
					}, C(w(o)(N.value.style.chart.segments.dataLabels.formatter, K.value, w(i)({
						p: N.value.style.chart.segments.dataLabels.prefix,
						v: K.value,
						s: N.value.style.chart.segments.dataLabels.suffix,
						r: N.value.style.chart.segments.dataLabels.rounding
					}))), 9, Be)) : f("", !0),
					N.value.style.chart.target.onTop && N.value.style.chart.target.show ? (y(), p("rect", {
						key: 2,
						x: q.value.target.x,
						y: W.value.top + (W.value.chartHeight - W.value.chartHeight * N.value.style.chart.target.heightRatio) / 2,
						height: W.value.chartHeight * N.value.style.chart.target.heightRatio,
						width: N.value.style.chart.target.width,
						rx: N.value.style.chart.target.rounded ? N.value.style.chart.target.width / 2 : 0,
						fill: N.value.style.chart.target.color,
						stroke: N.value.style.chart.target.stroke,
						"stroke-width": N.value.style.chart.target.strokeWidth,
						style: { transition: "x 0.3s ease-in-out" }
					}, null, 8, Ve)) : f("", !0),
					N.value.style.chart.segments.dataLabels.show ? (y(), p("g", He, [(y(!0), p(l, null, x(q.value.ticks, (e) => (y(), p("text", {
						x: e.x,
						y: e.y,
						"text-anchor": "middle",
						fill: N.value.style.chart.segments.dataLabels.color,
						"font-size": N.value.style.chart.segments.dataLabels.fontSize + "px",
						"font-weight": N.value.style.chart.segments.dataLabels.bold ? "bold" : "normal"
					}, C(w(o)(N.value.style.chart.segments.dataLabels.formatter, e.value, w(i)({
						p: N.value.style.chart.segments.dataLabels.prefix,
						v: e.value,
						s: N.value.style.chart.segments.dataLabels.suffix,
						r: N.value.style.chart.segments.dataLabels.rounding
					}))), 9, Ue))), 256))])) : f("", !0),
					N.value.style.chart.segments.dataLabels.show && N.value.style.chart.segments.ticks.show ? (y(), p("g", We, [(y(!0), p(l, null, x(q.value.ticks, (e) => (y(), p("line", {
						x1: e.x,
						x2: e.x,
						y1: W.value.bottom,
						y2: W.value.bottom + 3,
						stroke: N.value.style.chart.segments.ticks.stroke,
						"stroke-width": 1,
						"stroke-linecap": "round"
					}, null, 8, Ge))), 256))])) : f("", !0)
				])) : f("", !0),
				S(e.$slots, "svg", { svg: {
					...W.value,
					isPrintingImg: w(Y) || w(X) || w(At),
					isPrintingSvg: w(jt)
				} }, void 0, !0)
			], 14, Me)),
			e.$slots.watermark ? (y(), p("div", Ke, [S(e.$slots, "watermark", _(g({ isPrinting: w(Y) || w(X) || w(At) || w(jt) })), void 0, !0)])) : f("", !0),
			m("div", { id: `legend-bottom-${I.value}` }, null, 8, qe),
			ot.value && (N.value.style.chart.legend.show || e.$slots.legend) ? (y(), d(be, {
				key: 5,
				to: N.value.style.chart.legend.position === "top" ? `#legend-top-${I.value}` : `#legend-bottom-${I.value}`
			}, [m("div", {
				ref_key: "chartLegend",
				ref: it
			}, [S(e.$slots, "legend", { legend: J.value }, () => [N.value.style.chart.legend.show ? (y(), d(ve, {
				key: 0,
				clickable: !1,
				legendSet: J.value,
				config: yt.value
			}, {
				item: T(({ legend: e }) => [w(R) ? f("", !0) : (y(), p("div", Je, [m("span", Ye, C(e.name) + ":", 1), m("span", null, C(e.value), 1)]))]),
				_: 1
			}, 8, ["legendSet", "config"])) : f("", !0)], !0)], 512)], 8, ["to"])) : f("", !0),
			e.$slots.source ? (y(), p("div", {
				key: 6,
				ref_key: "source",
				ref: k,
				dir: "auto"
			}, [S(e.$slots, "source", {}, void 0, !0)], 512)) : f("", !0),
			S(e.$slots, "skeleton", {}, () => [w(R) ? (y(), d(ce, { key: 0 })) : f("", !0)], !0)
		], 46, ke));
	}
}, [["__scopeId", "data-v-5912378f"]]);
//#endregion
export { Oe as n, E as t };
