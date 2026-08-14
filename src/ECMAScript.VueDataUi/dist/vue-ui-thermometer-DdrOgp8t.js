import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Bt as t, Jt as n, Kt as r, Pt as i, S as a, X as o, b as s, i as c, jt as ee, pt as te, q as ne, t as re, tt as ie, w as ae } from "./lib-Bttd6u5E.js";
import { n as oe, t as se } from "./useHints-Dq_w2E8B.js";
import { t as ce } from "./useConfig-DlNpz6P8.js";
import { t as le } from "./usePrinter-DN5bYhTG.js";
import { n as ue, t as de } from "./BaseScanner-DZvpgOjM.js";
import { t as l } from "./useNestedProp-vPNvh7rV.js";
import { t as fe } from "./useThemeCheck-C43Tcqmk.js";
import { t as pe } from "./useChartExport-DNiwdPmb.js";
import { t as me } from "./img-Bnokohej.js";
import { n as he } from "./Title-BE3qg9xl.js";
import { t as ge } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as _e, t as ve } from "./useResponsive-ZtArZtUf.js";
import { t as ye } from "./DefGrad-DVBqDjhO.js";
import { t as be } from "./useUserOptionState-DK-_1ddE.js";
import { t as xe } from "./useChartAccessibility-DYqac8yF.js";
import { t as Se } from "./usePrefersMotion-BC-CsqR1.js";
import { t as Ce } from "./useAutoSizeLabelsInsideViewbox-DvDwcwi_.js";
import { t as we } from "./vue_ui_thermometer-DxgqWKlE.js";
import { Fragment as u, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createSlots as Te, createVNode as Ee, defineAsyncComponent as g, guardReactiveProps as _, normalizeClass as v, normalizeProps as y, normalizeStyle as De, onMounted as Oe, openBlock as b, ref as x, renderList as ke, renderSlot as S, toDisplayString as C, toRefs as Ae, unref as w, useCssVars as je, watch as Me, withCtx as T } from "vue";
//#region src/components/vue-ui-thermometer.vue
var Ne = /* @__PURE__ */ e({ default: () => E }), Pe = ["id"], Fe = {
	key: 1,
	ref: "noTitle",
	class: "vue-data-ui-no-title-space",
	style: "height:36px; width: 100%;background:transparent"
}, Ie = [
	"xmlns",
	"viewBox",
	"aria-labelledby",
	"aria-describedby"
], Le = ["id"], Re = ["id"], ze = ["width", "height"], Be = ["id"], Ve = [
	"x",
	"y",
	"width",
	"height",
	"rx",
	"ry"
], He = ["clip-path"], Ue = [
	"x",
	"y",
	"height",
	"width"
], We = [
	"x",
	"y",
	"height",
	"width",
	"fill"
], Ge = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], Ke = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], qe = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], Je = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], Ye = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], Xe = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], Ze = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], Qe = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke-width",
	"stroke"
], $e = [
	"x",
	"y",
	"height",
	"width"
], et = ["aria-label"], tt = [
	"x",
	"y",
	"height"
], nt = [
	"y",
	"x",
	"fill",
	"font-size",
	"font-weight"
], rt = {
	key: 4,
	class: "vue-data-ui-watermark"
}, E = /*#__PURE__*/ ge({
	__name: "vue-ui-thermometer",
	props: {
		dataset: {
			type: Object,
			default() {
				return {};
			}
		},
		config: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	emits: ["copyAlt"],
	setup(e, { expose: ge, emit: Ne }) {
		je((e) => ({
			v7d874252: At.value,
			v52f10497: kt.value,
			v6954d344: Ot.value
		}));
		let E = g(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), it = g(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), at = g(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), { vue_ui_thermometer: ot } = ce(), { isThemeValid: st, warnInvalidTheme: ct } = fe(), lt = Se(), D = e, ut = Ne, O = x(ne()), k = x(null), dt = x(0), ft = x(0), pt = x(null), mt = x(null), A = x(null), j = x(null), ht = d(() => !!D.dataset && Object.keys(D.dataset).length);
		Oe(() => {
			bt();
		});
		let M = x(z());
		oe({
			config: () => M.value,
			dataset: () => D.dataset,
			component: "VueUiThermometer",
			rules: [se.noHint]
		});
		let gt = x(M.value.style.chart.thermometer.width), N = x(M.value.style.chart.height), P = x(M.value.style.chart.width), _t = d(() => M.value.userOptions.useCursorPointer), vt = d(() => n({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					animation: { use: !1 },
					backgroundColor: "#99999930",
					graduations: { stroke: "#6A6A6A" }
				} }
			},
			userConfig: M.value.skeletonConfig ?? {}
		})), { loading: F, FINAL_DATASET: I } = ue({
			...Ae(D),
			FINAL_CONFIG: M,
			prepareConfig: z,
			skeletonDataset: D.config?.skeletonDataset ?? {
				value: 0,
				from: -100,
				to: 100,
				steps: 20,
				colors: {
					from: "#A1A1A1",
					to: "#CACACA"
				}
			},
			skeletonConfig: n({
				defaultConfig: M.value,
				userConfig: vt.value
			})
		}), yt = d(() => M.value.debug);
		function bt() {
			if (ee(D.dataset) ? ie({
				componentName: "VueUiThermometer",
				type: "dataset",
				debug: yt.value
			}) : te({
				datasetObject: D.dataset,
				requiredAttributes: [
					"value",
					"from",
					"to"
				]
			}).forEach((e) => {
				ie({
					componentName: "VueUiThermometer",
					type: "datasetAttribute",
					property: e,
					debug: yt.value
				});
			}), M.value.responsive) {
				let e = _e(() => {
					let { width: e, height: t } = ve({
						chart: k.value,
						title: M.value.style.title.text ? pt.value : null,
						source: mt.value
					});
					requestAnimationFrame(() => {
						N.value = Math.max(.1, t - 12), P.value = e, Ft();
					});
				});
				A.value && (j.value && A.value.unobserve(j.value), A.value.disconnect()), A.value = new ResizeObserver(e), j.value = k.value, A.value.observe(j.value);
			}
			Ft();
		}
		let { userOptionsVisible: L, setUserOptionsVisibility: xt, keepUserOptionState: St } = be({ config: M.value }), { svgRef: R } = xe({ config: M.value.style.title });
		function z() {
			let e = l({
				userConfig: D.config,
				defaultConfig: ot
			}), t = e.theme;
			if (!t) return e;
			if (!st.value(e)) return ct(e), e;
			let n = l({
				userConfig: we[t] || D.config,
				defaultConfig: e
			}), a = l({
				userConfig: D.config,
				defaultConfig: n
			});
			return {
				...a,
				customPalette: a.customPalette.length ? a.customPalette : r[t] || i
			};
		}
		Me(() => D.config, (e) => {
			M.value = z(), L.value = !M.value.userOptions.showOnChartHover, gt.value = M.value.style.chart.thermometer.width, N.value = M.value.style.chart.height, P.value = M.value.style.chart.width, bt(), ft.value += 1;
		}, { deep: !0 });
		let { isPrinting: B, isImaging: V, generatePdf: Ct, generateImage: wt } = le({
			elementId: `thermometer__${O.value}`,
			fileName: M.value.style.title.text || "vue-ui-thermometer",
			options: M.value.userOptions.print
		}), Tt = d(() => M.value.userOptions.show && !M.value.style.title.text), H = d(() => ae(M.value.customPalette)), U = d(() => I.value.steps || 10);
		function W(e, t, n) {
			let r = [], i = Et(e), a = Et(t);
			for (let e = 0; e < n; e += 1) {
				let t = G(i.red, a.red, e, n), o = G(i.green, a.green, e, n), s = G(i.blue, a.blue, e, n), c = `#${K(t)}${K(o)}${K(s)}`;
				r.push(c);
			}
			return r;
		}
		function Et(e) {
			let t = e.slice(1);
			return {
				red: parseInt(t.slice(0, 2), 16),
				green: parseInt(t.slice(2, 4), 16),
				blue: parseInt(t.slice(4, 6), 16)
			};
		}
		function G(e, t, n, r) {
			return Math.round(e + (t - e) * n / r);
		}
		function K(e) {
			return e.toString(16).padStart(2, "0");
		}
		let q = d(() => {
			let e = Math.max(.1, P.value), t = Math.max(.1, N.value), n = M.value.style.chart.padding;
			return {
				width: e,
				left: e / 2 - M.value.style.chart.thermometer.width / 2,
				right: e / 2 + M.value.style.chart.thermometer.width / 2,
				top: n.top,
				bottom: t - n.bottom - n.top,
				height: t,
				thermoHeight: t - n.top - n.bottom,
				thermoWidth: M.value.style.chart.thermometer.width
			};
		}), Dt = d(() => q.value), J = d(() => {
			let e = s(I.value.from) < 0 ? Math.abs(s(I.value.from)) : s(I.value.from), t = s(I.value.to) < 0 ? Math.abs(s(I.value.to)) : s(I.value.to), n = 0;
			return n = s(I.value.to) > 0 ? e + t : e > t ? e - t : t - e, (1 - (Math.abs(s(I.value.from)) + s(I.value.value)) / n) * q.value.thermoHeight;
		}), Ot = d(() => `${J.value}px`), kt = d(() => `${q.value.thermoHeight}px`), At = d(() => lt.value ? "0ms" : `${M.value.style.chart.animation.speedMs}ms`), jt = d(() => {
			if (I.value.colors) {
				if (!I.value.colors.from) return W(H.value[0] || i[0], a(I.value.colors.to), U.value || 10);
				if (!I.value.colors.to) return W(a(I.value.colors.from), H.value[1] || i[1], U.value || 10);
			} else return W(H.value[1] || i[1], H.value[0] || i[0], U.value || 10);
			return W(a(I.value.colors.from), a(I.value.colors.to), U.value || 10);
		}), Mt = d(() => {
			let e = [], t = 0, n = q.value.thermoHeight;
			for (let r = 0; r < n - 1; r += n / U.value) e.push({
				x: q.value.left,
				y: q.value.top + r,
				qYLess: q.value.top + r + n / U.value / 4,
				halfY: q.value.top + r + n / U.value / 2,
				qYMore: q.value.top + r + n / U.value / 4 * 3,
				color: jt.value[t],
				height: Math.max(.1, n / U.value)
			}), t += 1;
			return e;
		}), Y = x(!1);
		function Nt(e) {
			Y.value = e, dt.value += 1;
		}
		let X = x(!1);
		function Z() {
			X.value = !X.value;
		}
		async function Pt({ scale: e = 2 } = {}) {
			if (!k.value) return;
			let { width: t, height: n } = k.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await me({
				domElement: k.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: M.value.style.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Q = d({
			get: () => M.value.style.chart.label.fontSize,
			set: (e) => e
		}), { autoSizeLabels: Ft } = Ce({
			svgRef: R,
			fontSize: M.value.style.chart.label.fontSize,
			minFontSize: M.value.style.chart.label.minFontSize,
			sizeRef: Q,
			labelClass: ".vue-ui-thermometer-label"
		}), It = d(() => M.value.style.chart.backgroundColor), Lt = d(() => M.value.style.title), { isCallbackImaging: Rt, isCallbackSvg: zt, generateSvg: Bt, onGenerateImage: Vt } = pe({
			svg: R,
			title: Lt,
			legend: null,
			legendItems: null,
			backgroundColor: It,
			getSvgCallback: () => M.value.userOptions.callbacks.svg,
			generateImage: wt
		});
		async function Ht() {
			if (ut("copyAlt", {
				config: M.value,
				dataset: I.value
			}), !M.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(M.value.userOptions.callbacks.altCopy({
				config: M.value,
				dataset: I.value
			}));
		}
		let Ut = d(() => `${O.value}-title`), Wt = d(() => `${O.value}-desc`), $ = d(() => c(M.value.style.chart.label.formatter, I.value.value, o({
			p: M.value.style.chart.label.prefix,
			v: I.value.value,
			s: M.value.style.chart.label.suffix,
			r: M.value.style.chart.label.rounding
		}), { datapoint: I.value })), Gt = d(() => M.value.style.title.text || ""), Kt = d(() => {
			if (F.value) return "...";
			let e = s(I.value.from), t = s(I.value.to);
			return `Thermometer value: ${$.value}. Range: ${e} to ${t}.`;
		});
		return ge({
			getImage: Pt,
			generatePdf: Ct,
			generateImage: wt,
			generateSvg: Bt,
			toggleAnnotator: Z,
			toggleFullscreen: Nt,
			copyAlt: Ht
		}), (e, n) => (b(), m("div", {
			ref_key: "thermoChart",
			ref: k,
			class: v(`vue-data-ui-component vue-ui-thermometer ${Y.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			style: De(`width:100%;background:${M.value.style.chart.backgroundColor};color:${M.value.style.chart.color};font-family:${M.value.style.fontFamily}`),
			id: `thermometer__${O.value}`,
			onMouseenter: n[0] ||= () => w(xt)(!0),
			onMouseleave: n[1] ||= () => w(xt)(!1)
		}, [
			M.value.userOptions.buttons.annotator ? (b(), f(w(it), {
				key: 0,
				svgRef: w(R),
				backgroundColor: M.value.style.chart.backgroundColor,
				color: M.value.style.chart.color,
				active: X.value,
				isCursorPointer: _t.value,
				onClose: Z
			}, {
				"annotator-action-close": T(() => [S(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": T(({ color: t }) => [S(e.$slots, "annotator-action-color", y(_({ color: t })), void 0, !0)]),
				"annotator-action-draw": T(({ mode: t }) => [S(e.$slots, "annotator-action-draw", y(_({ mode: t })), void 0, !0)]),
				"annotator-action-undo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-undo", y(_({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-redo", y(_({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": T(({ disabled: t }) => [S(e.$slots, "annotator-action-delete", y(_({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : p("", !0),
			Tt.value ? (b(), m("div", Fe, null, 512)) : p("", !0),
			M.value.style.title.text ? (b(), m("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: pt,
				style: "width:100%"
			}, [(b(), f(he, {
				key: `title_${ft.value}`,
				config: {
					title: {
						cy: "thermometer-div-title",
						...M.value.style.title
					},
					subtitle: {
						cy: "thermometer-div-subtitle",
						...M.value.style.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : p("", !0),
			M.value.userOptions.show && ht.value && (w(St) || w(L)) ? (b(), f(w(at), {
				ref: "details",
				key: `user_options_${dt.value}`,
				backgroundColor: M.value.style.chart.backgroundColor,
				color: M.value.style.chart.color,
				isImaging: w(V),
				isPrinting: w(B),
				uid: O.value,
				hasPdf: M.value.userOptions.buttons.pdf,
				hasImg: M.value.userOptions.buttons.img,
				hasSvg: M.value.userOptions.buttons.svg,
				hasFullscreen: M.value.userOptions.buttons.fullscreen,
				hasAltCopy: M.value.userOptions.buttons.altCopy,
				hasXls: !1,
				isFullscreen: Y.value,
				titles: { ...M.value.userOptions.buttonTitles },
				chartElement: k.value,
				position: M.value.userOptions.position,
				hasAnnotator: M.value.userOptions.buttons.annotator,
				isAnnotation: X.value,
				callbacks: M.value.userOptions.callbacks,
				printScale: M.value.userOptions.print.scale,
				isCursorPointer: _t.value,
				onToggleFullscreen: Nt,
				onGeneratePdf: w(Ct),
				onGenerateImage: w(Vt),
				onGenerateSvg: w(Bt),
				onToggleAnnotator: Z,
				onCopyAlt: Ht,
				style: De({ visibility: w(St) ? w(L) ? "visible" : "hidden" : "visible" })
			}, Te({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: T(({ isOpen: t, color: n }) => [S(e.$slots, "menuIcon", y(_({
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
					fn: T(({ toggleFullscreen: t, isFullscreen: n }) => [S(e.$slots, "optionFullscreen", y(_({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: T(({ toggleAnnotator: t, isAnnotator: n }) => [S(e.$slots, "optionAnnotator", y(_({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: T(({ altCopy: t }) => [S(e.$slots, "optionAltCopy", y(_({ altCopy: t })), void 0, !0)]),
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
				ref: R,
				xmlns: w(re),
				class: v({
					"vue-data-ui-fullscreen--on": Y.value,
					"vue-data-ui-fulscreen--off": !Y.value
				}),
				width: "100%",
				viewBox: `0 0 ${q.value.width} ${q.value.height}`,
				style: "background:transparent",
				"aria-labelledby": Ut.value,
				"aria-describedby": Wt.value
			}, [
				h("title", { id: Ut.value }, C(Gt.value), 9, Le),
				h("desc", { id: Wt.value }, C(Kt.value), 9, Re),
				Ee(w(E)),
				e.$slots["chart-background"] ? (b(), m("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: q.value.width,
					height: q.value.height,
					style: { pointerEvents: "none" }
				}, [S(e.$slots, "chart-background", {}, void 0, !0)], 8, ze)) : p("", !0),
				h("defs", null, [h("clipPath", {
					id: `vueUiPill-${O.value}`,
					clipPathUnits: "userSpaceOnUse"
				}, [h("rect", {
					x: q.value.left,
					y: q.value.top,
					width: q.value.thermoWidth,
					height: q.value.thermoHeight,
					rx: q.value.thermoWidth / 2,
					ry: q.value.thermoWidth / 2
				}, null, 8, Ve)], 8, Be), (b(!0), m(u, null, ke(Mt.value, (e, n) => (b(), f(ye, {
					t: "linear",
					id: `vueUiThermometerGradient_${n}_${O.value}`,
					key: `t_${n}_${O.value}`,
					x1: "0%",
					y1: "0%",
					x2: "100%",
					y2: "0%",
					stops: [
						[
							"0%",
							e.color,
							1
						],
						[
							"50%",
							w(t)(e.color, 100 - M.value.style.chart.graduations.gradient.intensity),
							1
						],
						[
							"100%",
							e.color,
							1
						]
					]
				}, null, 8, ["id", "stops"]))), 128))]),
				h("g", { "clip-path": `url(#vueUiPill-${O.value})` }, [
					h("rect", {
						x: q.value.left,
						y: q.value.top,
						height: q.value.thermoHeight,
						width: q.value.thermoWidth,
						fill: "#FFFFFF"
					}, null, 8, Ue),
					(b(!0), m(u, null, ke(Mt.value, (e, t) => (b(), m("g", { key: `graduation_${t}` }, [
						h("rect", {
							x: e.x,
							y: e.y,
							height: e.height,
							width: q.value.thermoWidth,
							fill: M.value.style.chart.graduations.gradient.show ? `url(#vueUiThermometerGradient_${t}_${O.value})` : e.color,
							"shape-rendering": "crispEdges"
						}, null, 8, We),
						M.value.style.chart.graduations.show && ["both", "left"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
							key: 0,
							x1: e.x,
							x2: e.x + 10,
							y1: e.y,
							y2: e.y,
							"stroke-width": M.value.style.chart.graduations.strokeWidth,
							stroke: M.value.style.chart.graduations.stroke,
							"stroke-linecap": "round"
						}, null, 8, Ge)) : p("", !0),
						M.value.style.chart.graduations.showIntermediate ? (b(), m(u, { key: 1 }, [
							M.value.style.chart.graduations.show && ["both", "left"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
								key: 0,
								x1: e.x,
								x2: e.x + 5,
								y1: e.halfY,
								y2: e.halfY,
								"stroke-width": M.value.style.chart.graduations.strokeWidth / 2,
								stroke: M.value.style.chart.graduations.stroke,
								"stroke-linecap": "round"
							}, null, 8, Ke)) : p("", !0),
							M.value.style.chart.graduations.show && ["both", "left"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
								key: 1,
								x1: e.x,
								x2: e.x + 2.5,
								y1: e.qYLess,
								y2: e.qYLess,
								"stroke-width": M.value.style.chart.graduations.strokeWidth / 2,
								stroke: M.value.style.chart.graduations.stroke,
								"stroke-linecap": "round"
							}, null, 8, qe)) : p("", !0),
							M.value.style.chart.graduations.show && ["both", "left"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
								key: 2,
								x1: e.x,
								x2: e.x + 2.5,
								y1: e.qYMore,
								y2: e.qYMore,
								"stroke-width": M.value.style.chart.graduations.strokeWidth / 2,
								stroke: M.value.style.chart.graduations.stroke,
								"stroke-linecap": "round"
							}, null, 8, Je)) : p("", !0)
						], 64)) : p("", !0),
						M.value.style.chart.graduations.show && ["both", "right"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
							key: 2,
							x1: q.value.right,
							x2: q.value.right - 10,
							y1: e.y,
							y2: e.y,
							"stroke-width": M.value.style.chart.graduations.strokeWidth,
							stroke: M.value.style.chart.graduations.stroke,
							"stroke-linecap": "round"
						}, null, 8, Ye)) : p("", !0),
						M.value.style.chart.graduations.showIntermediate ? (b(), m(u, { key: 3 }, [
							M.value.style.chart.graduations.show && ["both", "right"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
								key: 0,
								x1: q.value.right,
								x2: q.value.right - 5,
								y1: e.halfY,
								y2: e.halfY,
								"stroke-width": M.value.style.chart.graduations.strokeWidth / 2,
								stroke: M.value.style.chart.graduations.stroke,
								"stroke-linecap": "round"
							}, null, 8, Xe)) : p("", !0),
							M.value.style.chart.graduations.show && ["both", "right"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
								key: 1,
								x1: q.value.right,
								x2: q.value.right - 2.5,
								y1: e.qYLess,
								y2: e.qYLess,
								"stroke-width": M.value.style.chart.graduations.strokeWidth / 2,
								stroke: M.value.style.chart.graduations.stroke,
								"stroke-linecap": "round"
							}, null, 8, Ze)) : p("", !0),
							M.value.style.chart.graduations.show && ["both", "right"].includes(M.value.style.chart.graduations.sides) ? (b(), m("line", {
								key: 2,
								x1: q.value.right,
								x2: q.value.right - 2.5,
								y1: e.qYMore,
								y2: e.qYMore,
								"stroke-width": M.value.style.chart.graduations.strokeWidth / 2,
								stroke: M.value.style.chart.graduations.stroke,
								"stroke-linecap": "round"
							}, null, 8, Qe)) : p("", !0)
						], 64)) : p("", !0)
					]))), 128)),
					h("rect", {
						class: v({ "vue-ui-thermometer-temperature": M.value.style.chart.animation.use }),
						x: q.value.left,
						y: q.value.top,
						height: J.value,
						width: q.value.thermoWidth,
						fill: "#FFFFFF66"
					}, null, 10, $e)
				], 8, He),
				M.value.style.chart.label.show ? (b(), m("g", {
					key: 1,
					role: "status",
					"aria-live": "polite",
					"aria-label": w(F) ? "Loading data" : $.value
				}, [w(F) ? (b(), m("rect", {
					key: 0,
					x: q.value.left - 60,
					y: J.value + q.value.top - Q.value / 2,
					width: 50,
					height: Q.value,
					fill: "#6A6A6A40",
					rx: "3"
				}, null, 8, tt)) : (b(), m("text", {
					key: 1,
					"aria-hidden": "true",
					class: v({
						"vue-ui-thermometer-temperature-value": M.value.style.chart.animation.use,
						"vue-ui-thermometer-label": !0
					}),
					y: J.value + q.value.top + Q.value / 3,
					x: q.value.left - 10,
					"text-anchor": "end",
					fill: M.value.style.chart.label.color,
					"font-size": Q.value,
					"font-weight": M.value.style.chart.label.bold ? "bold" : "normal"
				}, C($.value), 11, nt))], 8, et)) : p("", !0),
				S(e.$slots, "svg", { svg: {
					...Dt.value,
					isPrintingImg: w(B) || w(V) || w(Rt),
					isPrintingSvg: w(zt)
				} }, void 0, !0)
			], 10, Ie)),
			e.$slots.watermark ? (b(), m("div", rt, [S(e.$slots, "watermark", y(_({ isPrinting: w(B) || w(V) || w(Rt) || w(zt) })), void 0, !0)])) : p("", !0),
			e.$slots.source ? (b(), m("div", {
				key: 5,
				ref_key: "source",
				ref: mt,
				dir: "auto"
			}, [S(e.$slots, "source", {}, void 0, !0)], 512)) : p("", !0),
			S(e.$slots, "skeleton", {}, () => [w(F) ? (b(), f(de, { key: 0 })) : p("", !0)], !0)
		], 46, Pe));
	}
}, [["__scopeId", "data-v-3ce3d8f1"]]);
//#endregion
export { Ne as n, E as t };
