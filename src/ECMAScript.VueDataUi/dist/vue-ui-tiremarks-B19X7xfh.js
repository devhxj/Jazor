import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Jt as t, Vt as n, X as r, b as i, i as a, jt as o, q as s, t as c, tt as l } from "./lib-Bttd6u5E.js";
import { n as ee, t as te } from "./useHints-Dq_w2E8B.js";
import { t as ne } from "./useConfig-DlNpz6P8.js";
import { t as re } from "./usePrinter-DN5bYhTG.js";
import { n as ie, t as ae } from "./BaseScanner-DZvpgOjM.js";
import { t as u } from "./useNestedProp-vPNvh7rV.js";
import { t as oe } from "./useThemeCheck-C43Tcqmk.js";
import { t as se } from "./useChartExport-DNiwdPmb.js";
import { t as ce } from "./img-Bnokohej.js";
import { n as le } from "./Title-BE3qg9xl.js";
import { t as ue } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as de, t as fe } from "./useResponsive-ZtArZtUf.js";
import { t as pe } from "./useUserOptionState-DK-_1ddE.js";
import { t as me } from "./useChartAccessibility-DYqac8yF.js";
import { t as he } from "./usePrefersMotion-BC-CsqR1.js";
import { t as ge } from "./vue_ui_tiremarks-CdEPieWV.js";
import { Fragment as _e, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as ve, createSlots as ye, createVNode as be, defineAsyncComponent as h, guardReactiveProps as g, normalizeClass as _, normalizeProps as v, normalizeStyle as y, onMounted as xe, openBlock as b, ref as x, renderList as Se, renderSlot as S, toDisplayString as C, toRefs as Ce, unref as w, watch as we, withCtx as T } from "vue";
//#region src/components/vue-ui-tiremarks.vue
var Te = /* @__PURE__ */ e({ default: () => E }), Ee = ["id"], De = [
	"xmlns",
	"viewBox",
	"aria-labelledby",
	"aria-describedby"
], Oe = ["id"], ke = ["id"], Ae = ["width", "height"], je = { key: 1 }, Me = [
	"d",
	"stroke-width",
	"stroke"
], Ne = { key: 2 }, Pe = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke-width",
	"stroke"
], Fe = ["aria-label"], Ie = [
	"x",
	"y",
	"height"
], Le = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight",
	"text-anchor"
], Re = {
	key: 4,
	class: "vue-data-ui-watermark"
}, E = /*#__PURE__*/ ue({
	__name: "vue-ui-tiremarks",
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
	setup(e, { expose: ue, emit: Te }) {
		let E = h(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), ze = h(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Be = h(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_tiremarks: Ve } = ne(), { isThemeValid: He, warnInvalidTheme: Ue } = oe(), D = he(), O = e, We = Te, Ge = d(() => !!O.dataset && Object.keys(O.dataset).length), k = x(s()), A = x(null), Ke = x(null), qe = x(null), Je = x(null), Ye = x(0), Xe = x(0), j = x(null), M = x(null), N = x(R());
		ee({
			config: () => N.value,
			dataset: () => O.dataset,
			component: "VueUiTiremarks",
			rules: [te.noHint]
		});
		let Ze = d(() => N.value.userOptions.useCursorPointer), Qe = d(() => t({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					animation: { use: !1 },
					layout: {
						activeColor: "#6A6A6A80",
						inactiveColor: "#CACACA80",
						ticks: { gradient: { show: !1 } }
					}
				} }
			},
			userConfig: N.value.skeletonConfig ?? {}
		})), { loading: P, FINAL_DATASET: F } = ie({
			...Ce(O),
			FINAL_CONFIG: N,
			prepareConfig: R,
			skeletonDataset: O.config?.skeletonDataset ?? { percentage: 50 },
			skeletonConfig: t({
				defaultConfig: N.value,
				userConfig: Qe.value
			})
		}), { userOptionsVisible: I, setUserOptionsVisibility: $e, keepUserOptionState: et } = pe({ config: N.value }), { svgRef: L } = me({ config: N.value.style.chart.title });
		function R() {
			let e = u({
				userConfig: O.config,
				defaultConfig: Ve
			}), t = e.theme;
			if (!t) return e;
			if (!He.value(e)) return Ue(e), e;
			let n = u({
				userConfig: ge[t] || O.config,
				defaultConfig: e
			});
			return u({
				userConfig: O.config,
				defaultConfig: n
			});
		}
		we(() => O.config, (e) => {
			N.value = R(), I.value = !N.value.userOptions.showOnChartHover, G.value = N.value.style.chart.width, K.value = N.value.style.chart.height, ot(), Xe.value += 1;
		}, { deep: !0 });
		let { isPrinting: z, isImaging: B, generatePdf: tt, generateImage: nt } = re({
			elementId: k.value,
			fileName: N.value.style.chart.title.text || "vue-ui-tiremarks",
			options: N.value.userOptions.print
		}), rt = d(() => N.value.userOptions.show && !N.value.style.chart.title.text), V = x(N.value.style.chart.animation.use && !D.value ? 0 : i(F.value.percentage));
		we(() => F.value, (e) => {
			N.value.style.chart.animation.use && !D.value ? it(e.percentage) : V.value = e.percentage || 0;
		}, { deep: !0 }), xe(() => {
			ot();
		});
		function it(e) {
			let t = N.value.style.chart.animation.speed, n = Math.abs(e - V.value) / (t * 120);
			function r() {
				V.value < e ? V.value = Math.min(V.value + n, e) : V.value > e && (V.value = Math.max(V.value - n, e)), V.value !== e && requestAnimationFrame(r);
			}
			r();
		}
		let at = d(() => N.value.debug);
		function ot() {
			if (o(O.dataset) && l({
				componentName: "VueUiTiremarks",
				type: "dataset",
				debug: at.value
			}), N.value.responsive) {
				let e = de(() => {
					let { width: e, height: t } = fe({
						chart: A.value,
						title: N.value.style.chart.title.text ? qe.value : null,
						source: Je.value
					});
					requestAnimationFrame(() => {
						G.value = Math.max(.1, e), K.value = Math.max(.1, t - 12);
					});
				});
				j.value && (M.value && j.value.unobserve(M.value), j.value.disconnect()), j.value = new ResizeObserver(e), M.value = A.value.parentNode, j.value.observe(M.value);
			}
			it(F.value.percentage || 0);
		}
		let H = d(() => N.value.style.chart.layout.display === "vertical"), U = d(() => {
			let e = N.value.style.chart.percentage.show, t = {
				top: e ? 48 : 12,
				left: e ? 64 : 16,
				right: e ? 64 : 16,
				bottom: e ? 48 : 12
			};
			return H.value ? {
				top: N.value.style.chart.percentage.verticalPosition === "top" ? t.top : 3,
				left: 3,
				right: 3,
				bottom: N.value.style.chart.percentage.verticalPosition === "bottom" ? t.bottom : 3
			} : {
				top: 0,
				bottom: 0,
				left: N.value.style.chart.percentage.horizontalPosition === "left" ? t.left : 16,
				right: N.value.style.chart.percentage.horizontalPosition === "right" ? t.right : 10
			};
		}), W = d(() => Object.values(U.value).reduce((e, t) => e + t, 0)), G = x(N.value.style.chart.width), K = x(N.value.style.chart.height), q = d(() => ({
			height: K.value,
			width: G.value
		})), st = d(() => ({
			horizontal: {
				x: Y.value.x + (N.value.style.chart.percentage.horizontalPosition === "left" ? 6 : 3),
				y: q.value.height / 2 - Y.value.fontSize / 2
			},
			vertical: {
				x: q.value.width / 2 - 20,
				y: Y.value.y - Y.value.fontSize / 2
			}
		})[N.value.style.chart.layout.display]), J = d(() => H.value ? {
			mark: (q.value.height - W.value) / 100 * .5,
			space: (q.value.height - W.value) / 100 * .5
		} : {
			mark: (q.value.width - W.value) / 100 * .5,
			space: (q.value.width - W.value) / 100 * .5
		}), ct = d(() => {
			let e = [];
			for (let t = 0; t < 100; t += 1) {
				let r = N.value.style.chart.layout.ticks.gradient.show ? n(N.value.style.chart.layout.activeColor, t / 100 * (N.value.style.chart.layout.ticks.gradient.shiftHueIntensity / 100)) : N.value.style.chart.layout.activeColor;
				if (H.value) {
					let n = N.value.style.chart.layout.crescendo ? (100 - t) * (q.value.width - U.value.left - U.value.right) / 100 / 3 : 0, i = U.value.left + 4 + n, a = q.value.width - U.value.right - 4 - n, o = q.value.height - U.value.bottom - t * J.value.mark - t * J.value.space - J.value.mark, s = q.value.height - U.value.bottom - t * J.value.mark - t * J.value.space - J.value.mark, c = (a - i) / N.value.style.chart.layout.curveAngleX, l = N.value.style.chart.layout.curveAngleY * ((1 + t) / 100);
					e.push({
						x1: i,
						x2: a,
						y1: o,
						y2: s,
						curve: `M ${i} ${o} C ${i + c} ${o - l}, ${a - c} ${s - l}, ${a} ${s}`,
						color: r
					});
				} else {
					let n = N.value.style.chart.layout.crescendo ? (100 - t) * (q.value.height - U.value.top - U.value.bottom) / 100 / 3 : 0, i = U.value.left + t * J.value.mark + t * J.value.space - J.value.mark, a = i, o = U.value.top + 4 + n, s = q.value.height - U.value.bottom - 4 - n, c = N.value.style.chart.layout.curveAngleY * ((1 + t) / 100), l = (s - o) / N.value.style.chart.layout.curveAngleX;
					e.push({
						x1: i,
						x2: a,
						y1: o,
						y2: s,
						curve: `M ${i} ${o} C ${i + c} ${o + l}, ${a + c} ${s - l}, ${a} ${s}`,
						color: r
					});
				}
			}
			return e;
		}), Y = d(() => {
			let e, t, n, r = N.value.style.chart.percentage.fontSize / 3;
			return H.value ? N.value.style.chart.percentage.verticalPosition === "top" ? (e = q.value.width / 2, t = U.value.top / 2, n = "middle") : N.value.style.chart.percentage.verticalPosition === "bottom" && (e = q.value.width / 2, t = q.value.height - U.value.bottom / 2 + r, n = "middle") : N.value.style.chart.percentage.horizontalPosition === "left" ? (e = 4, t = q.value.height / 2 + r, n = "start") : N.value.style.chart.percentage.horizontalPosition === "right" && (e = q.value.width - U.value.right + 8, t = q.value.height / 2 + r, n = "start"), {
				x: e,
				y: t,
				textAnchor: n,
				bold: N.value.style.chart.percentage.bold,
				fontSize: N.value.style.chart.percentage.fontSize,
				fill: N.value.style.chart.percentage.color
			};
		}), X = x(!1);
		function lt(e) {
			X.value = e, Ye.value += 1;
		}
		let Z = x(!1);
		function Q() {
			Z.value = !Z.value;
		}
		async function ut({ scale: e = 2 } = {}) {
			if (!A.value) return;
			let { width: t, height: n } = A.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ce({
				domElement: A.value,
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
		let dt = d(() => N.value.style.chart.backgroundColor), ft = d(() => N.value.style.chart.title), { isCallbackImaging: pt, isCallbackSvg: mt, generateSvg: ht, onGenerateImage: gt } = se({
			svg: L,
			title: ft,
			legend: null,
			legendItems: null,
			backgroundColor: dt,
			getSvgCallback: () => N.value.userOptions.callbacks.svg,
			generateImage: nt
		});
		async function _t() {
			if (We("copyAlt", {
				config: N.value,
				dataset: F.value
			}), !N.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(N.value.userOptions.callbacks.altCopy({
				config: N.value,
				dataset: F.value
			}));
		}
		let vt = d(() => `${k.value}-title`), yt = d(() => `${k.value}-desc`), $ = d(() => a(N.value.style.chart.percentage.formatter, V.value, r({
			v: V.value,
			s: "%",
			r: N.value.style.chart.percentage.rounding
		}))), bt = d(() => N.value.style.chart.title.text || ""), xt = d(() => P.value ? "Loading data" : `Value: ${$.value}`);
		return ue({
			getImage: ut,
			generatePdf: tt,
			generateImage: nt,
			generateSvg: ht,
			toggleAnnotator: Q,
			toggleFullscreen: lt,
			copyAlt: _t
		}), (e, t) => (b(), m("div", {
			ref_key: "tiremarksChart",
			ref: A,
			class: _(`vue-data-ui-component vue-ui-tiremarks ${N.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: y(`font-family:${N.value.style.fontFamily};width:100%; text-align:center;background:${N.value.style.chart.backgroundColor}`),
			id: k.value,
			onMouseenter: t[0] ||= () => w($e)(!0),
			onMouseleave: t[1] ||= () => w($e)(!1)
		}, [
			N.value.userOptions.buttons.annotator ? (b(), f(w(E), {
				key: 0,
				svgRef: w(L),
				backgroundColor: N.value.style.chart.backgroundColor,
				color: N.value.style.chart.color,
				active: Z.value,
				isCursorPointer: Ze.value,
				onClose: Q
			}, {
				"annotator-action-close": T(() => [S(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": T(({ color: t }) => [S(e.$slots, "annotator-action-color", v(g({ color: t })), void 0, !0)]),
				"annotator-action-draw": T(({ mode: t }) => [S(e.$slots, "annotator-action-draw", v(g({ mode: t })), void 0, !0)]),
				"annotator-action-undo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-undo", v(g({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-redo", v(g({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": T(({ disabled: t }) => [S(e.$slots, "annotator-action-delete", v(g({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : p("", !0),
			rt.value ? (b(), m("div", {
				key: 1,
				ref_key: "noTitle",
				ref: Ke,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : p("", !0),
			N.value.style.chart.title.text ? (b(), m("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: qe,
				style: "width:100%;background:transparent;padding-bottom:12px"
			}, [(b(), f(le, {
				key: `title_${Xe.value}`,
				config: {
					title: {
						cy: "wheel-title",
						...N.value.style.chart.title
					},
					subtitle: {
						cy: "wheel-subtitle",
						...N.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : p("", !0),
			N.value.userOptions.show && Ge.value && (w(et) || w(I)) ? (b(), f(w(ze), {
				ref: "details",
				key: `user_options_${Ye.value}`,
				backgroundColor: N.value.style.chart.backgroundColor,
				color: N.value.style.chart.color,
				isPrinting: w(z),
				isImaging: w(B),
				uid: k.value,
				hasPdf: N.value.userOptions.buttons.pdf,
				hasImg: N.value.userOptions.buttons.img,
				hasSvg: N.value.userOptions.buttons.svg,
				hasFullscreen: N.value.userOptions.buttons.fullscreen,
				hasAltCopy: N.value.userOptions.buttons.altCopy,
				hasXls: !1,
				isFullscreen: X.value,
				titles: { ...N.value.userOptions.buttonTitles },
				chartElement: A.value,
				position: N.value.userOptions.position,
				hasAnnotator: N.value.userOptions.buttons.annotator,
				isAnnotation: Z.value,
				callbacks: N.value.userOptions.callbacks,
				printScale: N.value.userOptions.print.scale,
				isCursorPointer: Ze.value,
				onToggleFullscreen: lt,
				onGeneratePdf: w(tt),
				onGenerateImage: w(gt),
				onGenerateSvg: w(ht),
				onToggleAnnotator: Q,
				onCopyAlt: _t,
				style: y({ visibility: w(et) ? w(I) ? "visible" : "hidden" : "visible" })
			}, ye({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: T(({ isOpen: t, color: n }) => [S(e.$slots, "menuIcon", v(g({
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
					fn: T(({ toggleFullscreen: t, isFullscreen: n }) => [S(e.$slots, "optionFullscreen", v(g({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: T(({ toggleAnnotator: t, isAnnotator: n }) => [S(e.$slots, "optionAnnotator", v(g({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: T(({ altCopy: t }) => [S(e.$slots, "optionAltCopy", v(g({ altCopy: t })), void 0, !0)]),
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
				"titles",
				"chartElement",
				"position",
				"hasAnnotator",
				"isAnnotation",
				"callbacks",
				"printScale",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : p("", !0),
			(b(), m("svg", {
				ref_key: "svgRef",
				ref: L,
				xmlns: w(c),
				class: _({
					"vue-data-ui-fullscreen--on": X.value,
					"vue-data-ui-fulscreen--off": !X.value
				}),
				viewBox: `0 0 ${G.value} ${K.value}`,
				style: y(`max-width:100%; overflow: visible; background:transparent;color:${N.value.style.chart.color}`),
				role: "img",
				"aria-labelledby": vt.value,
				"aria-describedby": yt.value
			}, [
				ve("title", { id: vt.value }, C(bt.value), 9, Oe),
				ve("desc", { id: yt.value }, C(xt.value), 9, ke),
				be(w(Be)),
				e.$slots["chart-background"] ? (b(), m("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: q.value.width,
					height: q.value.height,
					style: { pointerEvents: "none" }
				}, [S(e.$slots, "chart-background", {}, void 0, !0)], 8, Ae)) : p("", !0),
				N.value.style.chart.layout.curved ? (b(), m("g", je, [(b(!0), m(_e, null, Se(ct.value, (e, t) => (b(), m("path", {
					d: e.curve,
					"stroke-width": J.value.mark,
					stroke: V.value >= t ? e.color : N.value.style.chart.layout.inactiveColor,
					"stroke-linecap": "round",
					fill: "none",
					class: _({ "vue-ui-tick-animated": N.value.style.chart.animation.use && !w(D) && t <= V.value })
				}, null, 10, Me))), 256))])) : (b(), m("g", Ne, [(b(!0), m(_e, null, Se(ct.value, (e, t) => (b(), m("line", {
					x1: e.x1,
					y1: e.y1,
					x2: e.x2,
					y2: e.y2,
					"stroke-width": J.value.mark,
					stroke: V.value >= t ? e.color : N.value.style.chart.layout.inactiveColor,
					"stroke-linecap": "round"
				}, null, 8, Pe))), 256))])),
				N.value.style.chart.percentage.show ? (b(), m("g", {
					key: 3,
					role: "status",
					"aria-live": "polite",
					"aria-label": w(P) ? "..." : `${$.value}`
				}, [w(P) ? (b(), m("rect", {
					key: 0,
					x: st.value.x,
					y: st.value.y,
					width: 40,
					height: Y.value.fontSize,
					fill: "#6A6A6A80",
					rx: 3
				}, null, 8, Ie)) : (b(), m("text", {
					key: 1,
					"aria-hidden": "true",
					x: Y.value.x,
					y: Y.value.y,
					"font-size": Y.value.fontSize,
					fill: N.value.style.chart.layout.ticks.gradient.show && N.value.style.chart.percentage.useGradientColor ? w(n)(N.value.style.chart.layout.activeColor, V.value / 100 * (N.value.style.chart.layout.ticks.gradient.shiftHueIntensity / 100)) : N.value.style.chart.percentage.color,
					"font-weight": Y.value.bold ? "bold" : "normal",
					"text-anchor": Y.value.textAnchor
				}, C($.value), 9, Le))], 8, Fe)) : p("", !0),
				S(e.$slots, "svg", { svg: {
					...q.value,
					isPrintingImg: w(z) || w(B) || w(pt),
					isPrintingSvg: w(mt)
				} }, void 0, !0)
			], 14, De)),
			e.$slots.watermark ? (b(), m("div", Re, [S(e.$slots, "watermark", v(g({ isPrinting: w(z) || w(B) || w(pt) || w(mt) })), void 0, !0)])) : p("", !0),
			e.$slots.source ? (b(), m("div", {
				key: 5,
				ref_key: "source",
				ref: Je,
				dir: "auto"
			}, [S(e.$slots, "source", {}, void 0, !0)], 512)) : p("", !0),
			S(e.$slots, "skeleton", {}, () => [w(P) ? (b(), f(ae, { key: 0 })) : p("", !0)], !0)
		], 46, Ee));
	}
}, [["__scopeId", "data-v-2c6b8285"]]);
//#endregion
export { Te as n, E as t };
