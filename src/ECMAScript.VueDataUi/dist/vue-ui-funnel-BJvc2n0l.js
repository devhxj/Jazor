import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Ot as r, S as i, X as a, i as o, jt as ee, pt as te, q as ne, qt as re, r as ie, t as ae, tt as oe } from "./lib-Bttd6u5E.js";
import { n as se, t as ce } from "./useHints-Dq_w2E8B.js";
import { t as le } from "./useConfig-DlNpz6P8.js";
import { t as ue } from "./usePrinter-DN5bYhTG.js";
import { t as de } from "./useNestedProp-vPNvh7rV.js";
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
import { t as Ce } from "./vue_ui_funnel-_Og4EEkO.js";
import { Fragment as s, computed as c, createBlock as l, createCommentVNode as u, createElementBlock as d, createElementVNode as f, createSlots as we, createTextVNode as Te, createVNode as Ee, defineAsyncComponent as p, guardReactiveProps as m, mergeProps as h, nextTick as De, normalizeClass as g, normalizeProps as _, normalizeStyle as v, onBeforeUnmount as Oe, onMounted as ke, openBlock as y, ref as b, renderList as x, renderSlot as S, resolveDynamicComponent as Ae, shallowRef as je, toDisplayString as C, unref as w, useCssVars as Me, watch as Ne, withCtx as T } from "vue";
//#region src/components/vue-ui-funnel.vue
var Pe = /* @__PURE__ */ e({ default: () => qe }), Fe = ["id"], Ie = ["xmlns", "viewBox"], Le = ["width", "height"], Re = ["stroke", "stroke-width"], ze = ["stroke", "stroke-width"], Be = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], Ve = ["points", "fill"], He = [
	"stroke",
	"stroke-width",
	"rx"
], Ue = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], We = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], Ge = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Ke = ["innerHTML"], qe = /*#__PURE__*/ ge({
	__name: "vue-ui-funnel",
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
	emits: ["copyAlt"],
	setup(e, { expose: ge, emit: Pe }) {
		Me((e) => ({ b556dc08: Tt.value }));
		let qe = p(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Je = p(() => import("./vue-ui-skeleton-E6Hbh29Z.js").then((e) => e.n)), Ye = p(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Xe = p(() => import("./DataTable-BbKgJ5UI.js")), Ze = p(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Qe = p(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), $e = p(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), et = p(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_funnel: tt } = le(), { isThemeValid: nt, warnInvalidTheme: rt } = fe(), E = Se(), D = e, it = Pe, O = b(null), k = b(ne()), at = b(0), ot = b(0), st = b(0), ct = b(null), lt = b(null), ut = b(null), A = je(null), j = je(null), M = b(!1), N = b(null), P = b(null), F = c(() => !!D.dataset && D.dataset.length);
		ke(pt), Oe(() => {
			A.value && (j.value && A.value.unobserve(j.value), A.value.disconnect());
		});
		function dt() {
			let e = de({
				userConfig: D.config,
				defaultConfig: tt
			}), t = e.theme;
			if (!t) return e;
			if (!nt.value(e)) return rt(e), e;
			let n = de({
				userConfig: Ce[t] || D.config,
				defaultConfig: e
			});
			return de({
				userConfig: D.config,
				defaultConfig: n
			});
		}
		let I = c({
			get: () => dt(),
			set: (e) => e
		}), ft = c(() => I.value.debug);
		function pt() {
			if (ee(D.dataset) ? oe({
				componentName: "VueUiFunnel",
				type: "dataset",
				debug: ft.value
			}) : D.dataset.forEach((e, t) => {
				te({
					datasetObject: e,
					requiredAttributes: ["name", "value"]
				}).forEach((e) => {
					F.value = !1, oe({
						componentName: "VueUiFunnel",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: ft.value
					});
				});
			}), I.value.responsive) {
				let e = _e(() => {
					let { width: e, height: t } = ve({
						chart: O.value,
						title: I.value.style.chart.title.text ? ut.value : null,
						source: lt.value,
						noTitle: ct.value
					});
					requestAnimationFrame(() => {
						U.value.height = t, U.value.width = e, G.value = bt(), I.value.responsiveProportionalSizing ? (H.value.circles = re({
							relator: Math.min(e, t),
							adjuster: 600,
							source: I.value.style.chart.circles.dataLabels.fontSize,
							threshold: 10,
							fallback: 10
						}), H.value.names = re({
							relator: Math.min(e, t),
							adjuster: 600,
							source: I.value.style.chart.bars.dataLabels.name.fontSize,
							threshold: 10,
							fallback: 10
						}), H.value.values = re({
							relator: Math.min(e, t),
							adjuster: 600,
							source: I.value.style.chart.bars.dataLabels.value.fontSize,
							threshold: 10,
							fallback: 10
						})) : (H.value.circles = I.value.style.chart.circles.dataLabels.fontSize, H.value.names = I.value.style.chart.bars.dataLabels.name.fontSize, H.value.values = I.value.style.chart.bars.dataLabels.value.fontSize);
					});
				});
				A.value && (j.value && A.value.unobserve(j.value), A.value.disconnect()), A.value = new ResizeObserver(e), j.value = O.value.parentNode, A.value.observe(j.value);
			}
		}
		se({
			config: () => I.value,
			dataset: () => D.dataset,
			component: "VueUiFunnel",
			rules: [ce.emptyArray, {
				test: (e) => e.length > 10,
				message: [
					"👀 The dataset has a length > 10. Consider:",
					"",
					"▶️ Aggregating some datapoints into broader categories"
				]
			}]
		});
		let mt = c(() => I.value.userOptions.useCursorPointer), { userOptionsVisible: L, setUserOptionsVisibility: ht, keepUserOptionState: gt } = be({ config: I.value }), { svgRef: R } = xe({ config: I.value.style.chart.title });
		Ne(() => D.config, (e) => {
			I.value = dt(), L.value = !I.value.userOptions.showOnChartHover, pt(), ot.value += 1, st.value += 1, H.value.circles = I.value.style.chart.circles.dataLabels.fontSize, H.value.names = I.value.style.chart.bars.dataLabels.name.fontSize, H.value.values = I.value.style.chart.bars.dataLabels.value.fontSize, V.value.showTable = I.value.table.show;
		}, { deep: !0 });
		let { isPrinting: z, isImaging: B, generatePdf: _t, generateImage: vt } = ue({
			elementId: `funnel_${k.value}`,
			fileName: I.value.style.chart.title.text || "vue-ui-funnel",
			options: I.value.userOptions.print
		}), yt = c(() => I.value.userOptions.show && !I.value.style.chart.title.text), V = b({ showTable: I.value.table.show }), H = b({
			circles: I.value.style.chart.circles.dataLabels.fontSize,
			names: I.value.style.chart.bars.dataLabels.name.fontSize,
			values: I.value.style.chart.bars.dataLabels.value.fontSize
		}), U = c({
			get: () => ({
				height: I.value.style.chart.height,
				width: I.value.style.chart.width
			}),
			set: (e) => e
		}), W = c(() => F.value ? D.dataset.map((e, t) => ({
			...e,
			color: e.color ? i(e.color) : r(I.value.style.chart.bars.defaultColor, t / D.dataset.length)
		})) : []);
		setTimeout(() => {
			M.value = !0;
		}, W.value.length * 150);
		function bt() {
			let e = I.value.style.chart.padding.left, t = I.value.style.chart.padding.top;
			return {
				left: e,
				top: t,
				right: U.value.width - I.value.style.chart.padding.right,
				bottom: U.value.height - I.value.style.chart.padding.bottom,
				width: U.value.width - e - I.value.style.chart.padding.right,
				height: U.value.height - t - I.value.style.chart.padding.bottom
			};
		}
		let G = b(bt()), K = c(() => G.value.height / D.dataset.length), q = c(() => K.value * I.value.style.chart.bars.gapRatio), xt = c(() => G.value.width * I.value.style.chart.barCircleSpacingRatio), J = c(() => W.value.map((e, t) => {
			let n = K.value - q.value, r = G.value.top + q.value / 2 * t + (K.value - q.value / 2) * t + q.value / 2, i = e.value / W.value[0].value, a = (G.value.width - n - xt.value) * (e.value / W.value[0].value);
			return {
				...e,
				cx: G.value.left + n / 2,
				cy: r + n / 2,
				datapointIndex: t,
				fill: e.color,
				height: Math.max(n, 0),
				proportion: i,
				r: Math.max(n / 2, 0),
				width: Math.max(a, 0),
				x: G.value.left + n + xt.value,
				y: r
			};
		})), St = c(() => {
			let e = J.value.map((e) => `${e.x + e.width},${e.y + (K.value - q.value) / 2}`);
			return `${J.value[0].x},${J.value[0].y + (K.value - q.value) / 2} ${e.toString()} ${J.value.at(-1).x},${J.value.at(-1).y + (K.value - q.value) / 2}`;
		}), Ct = c(() => ({
			x1: J.value[0].cx,
			y1: J.value[0].cy,
			x2: J.value.at(-1).cx,
			y2: J.value.at(-1).cy
		})), Y = b(!1);
		function wt(e) {
			Y.value = e, at.value += 1;
		}
		let Tt = c(() => `${W.value.length * 150}ms`), X = b(!1);
		function Et() {
			X.value = !X.value;
		}
		function Dt() {
			V.value.showTable = !V.value.showTable;
		}
		let Z = c(() => ({
			head: W.value.map((e) => ({
				name: e.name,
				color: e.color
			})),
			body: W.value.map((e) => e.value)
		})), Q = c(() => {
			let e = [
				I.value.table.columnNames.series,
				I.value.table.columnNames.value,
				I.value.table.columnNames.percentage
			], t = Z.value.head.map((e, t) => {
				let n = o(I.value.style.chart.bars.dataLabels.value.formatter, Z.value.body[t], a({
					p: I.value.style.chart.bars.dataLabels.value.prefix,
					v: Z.value.body[t],
					s: I.value.style.chart.bars.dataLabels.value.suffix,
					r: I.value.table.td.roundingValue
				}), { datapoint: J.value[t] }), r = o(I.value.style.chart.circles.dataLabels.formatter, J.value[t].proportion * 100, a({
					v: J.value[t].proportion * 100,
					s: "%",
					r: I.value.table.td.roundingPercentage
				}), { datapoint: J.value[t] });
				return [
					{
						color: e.color,
						name: e.name
					},
					n,
					r
				];
			}), n = {
				th: {
					backgroundColor: I.value.table.th.backgroundColor,
					color: I.value.table.th.color,
					outline: I.value.table.th.outline
				},
				td: {
					backgroundColor: I.value.table.td.backgroundColor,
					color: I.value.table.td.color,
					outline: I.value.table.td.outline
				},
				breakpoint: I.value.table.responsiveBreakpoint
			};
			return {
				colNames: [
					I.value.table.columnNames.series,
					I.value.table.columnNames.value,
					I.value.table.columnNames.percentage
				],
				head: e,
				body: t,
				config: n
			};
		});
		function Ot(e = null) {
			De(() => {
				let r = Z.value.head.map((e, t) => [
					[e.name],
					[Z.value.body[t]],
					[J.value[t].proportion * 100]
				]), i = [
					[I.value.style.chart.title.text],
					[I.value.style.chart.title.subtitle.text],
					[
						[I.value.table.columnNames.series],
						[I.value.table.columnNames.value],
						["%"]
					]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: I.value.style.chart.title.text || "vue-ui-funnel"
				});
			});
		}
		function kt() {
			return W.value;
		}
		async function At({ scale: e = 2 } = {}) {
			if (!O.value) return;
			let { width: t, height: n } = O.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await me({
				domElement: O.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: I.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let $ = c(() => {
			let e = I.value.table.useDialog && !I.value.table.show, t = V.value.showTable;
			return {
				component: e ? et : Ye,
				title: `${I.value.style.chart.title.text}${I.value.style.chart.title.subtitle.text ? `: ${I.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: I.value.table.th.backgroundColor,
					color: I.value.table.th.color,
					headerColor: I.value.table.th.color,
					headerBg: I.value.table.th.backgroundColor,
					isFullscreen: Y.value,
					fullscreenParent: O.value,
					forcedWidth: Math.min(800, window.innerWidth * .8)
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: I.value.style.chart.backgroundColor,
							color: I.value.style.chart.color
						},
						head: {
							backgroundColor: I.value.style.chart.backgroundColor,
							color: I.value.style.chart.color
						}
					}
				}
			};
		});
		Ne(() => V.value.showTable, (e) => {
			I.value.table.show || (e && I.value.table.useDialog && N.value ? N.value.open() : "close" in N.value && N.value.close());
		});
		function jt() {
			V.value.showTable = !1, P.value && P.value.setTableIconState(!1);
		}
		let Mt = c(() => I.value.style.chart.backgroundColor), Nt = c(() => I.value.style.chart.title), { isCallbackImaging: Pt, isCallbackSvg: Ft, generateSvg: It, onGenerateImage: Lt } = pe({
			svg: R,
			title: Nt,
			legend: null,
			legendItems: null,
			backgroundColor: Mt,
			getSvgCallback: () => I.value.userOptions.callbacks.svg,
			generateImage: vt
		});
		async function Rt() {
			if (it("copyAlt", {
				config: I.value,
				dataset: W.value
			}), !I.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(I.value.userOptions.callbacks.altCopy({
				config: I.value,
				dataset: W.value
			}));
		}
		return ge({
			getData: kt,
			getImage: At,
			generatePdf: _t,
			generateCsv: Ot,
			generateImage: vt,
			generateSvg: It,
			toggleTable: Dt,
			toggleAnnotator: Et,
			toggleFullscreen: wt,
			copyAlt: Rt
		}), (e, t) => (y(), d("div", {
			ref_key: "funnelChart",
			ref: O,
			class: g(`vue-data-ui-component vue-ui-funnel ${Y.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${I.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			style: v(`font-family:${I.value.style.fontFamily};width:100%; ${I.value.responsive ? "height:100%;" : ""} text-align:center;background:${I.value.style.chart.backgroundColor}`),
			id: `funnel_${k.value}`,
			onMouseenter: t[1] ||= () => w(ht)(!0),
			onMouseleave: t[2] ||= () => w(ht)(!1)
		}, [
			I.value.userOptions.buttons.annotator ? (y(), l(w(Qe), {
				key: 0,
				svgRef: w(R),
				backgroundColor: I.value.style.chart.backgroundColor,
				color: I.value.style.chart.color,
				active: X.value,
				isCursorPointer: mt.value,
				onClose: Et
			}, {
				"annotator-action-close": T(() => [S(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": T(({ color: t }) => [S(e.$slots, "annotator-action-color", _(m({ color: t })), void 0, !0)]),
				"annotator-action-draw": T(({ mode: t }) => [S(e.$slots, "annotator-action-draw", _(m({ mode: t })), void 0, !0)]),
				"annotator-action-undo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-undo", _(m({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": T(({ disabled: t }) => [S(e.$slots, "annotator-action-redo", _(m({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": T(({ disabled: t }) => [S(e.$slots, "annotator-action-delete", _(m({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : u("", !0),
			yt.value ? (y(), d("div", {
				key: 1,
				ref_key: "noTitle",
				ref: ct,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : u("", !0),
			I.value.style.chart.title.text ? (y(), d("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: ut,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(y(), l(he, {
				key: `title_${ot.value}`,
				config: {
					title: {
						cy: "funnel-div-title",
						...I.value.style.chart.title
					},
					subtitle: {
						cy: "funnel-div-subtitle",
						...I.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : u("", !0),
			I.value.userOptions.show && F.value && (w(gt) || w(L)) ? (y(), l(w(Ze), {
				ref_key: "userOptionsRef",
				ref: P,
				key: `user_option_${at.value}`,
				backgroundColor: I.value.style.chart.backgroundColor,
				color: I.value.style.chart.color,
				isPrinting: w(z),
				isImaging: w(B),
				uid: k.value,
				hasTooltip: !1,
				hasPdf: I.value.userOptions.buttons.pdf,
				hasImg: I.value.userOptions.buttons.img,
				hasSvg: I.value.userOptions.buttons.svg,
				hasXls: I.value.userOptions.buttons.csv,
				hasTable: I.value.userOptions.buttons.table,
				hasLabel: !1,
				hasFullscreen: I.value.userOptions.buttons.fullscreen,
				hasAltCopy: I.value.userOptions.buttons.altCopy,
				isFullscreen: Y.value,
				chartElement: O.value,
				position: I.value.userOptions.position,
				titles: { ...I.value.userOptions.buttonTitles },
				hasAnnotator: I.value.userOptions.buttons.annotator,
				isAnnotation: X.value,
				callbacks: I.value.userOptions.callbacks,
				printScale: I.value.userOptions.print.scale,
				tableDialog: I.value.table.useDialog,
				isCursorPointer: mt.value,
				onToggleAnnotator: Et,
				onToggleFullscreen: wt,
				onGeneratePdf: w(_t),
				onGenerateImage: w(Lt),
				onGenerateSvg: w(It),
				onToggleTable: Dt,
				onGenerateCsv: Ot,
				onCopyAlt: Rt,
				style: v({ visibility: w(gt) ? w(L) ? "visible" : "hidden" : "visible" })
			}, we({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: T(({ isOpen: t, color: n }) => [S(e.$slots, "menuIcon", _(m({
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
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: T(() => [S(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: T(() => [S(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: T(() => [S(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: T(() => [S(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: T(({ toggleFullscreen: t, isFullscreen: n }) => [S(e.$slots, "optionFullscreen", _(m({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: T(({ toggleAnnotator: t, isAnnotator: n }) => [S(e.$slots, "optionAnnotator", _(m({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: T(({ altCopy: t }) => [S(e.$slots, "optionAltCopy", _(m({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: T(() => [S(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: T(() => [S(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.chartElement.position.titles.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : u("", !0),
			F.value ? (y(), d("svg", {
				key: 4,
				ref_key: "svgRef",
				ref: R,
				xmlns: w(ae),
				class: g({
					"vue-data-ui-fullscreen--on": Y.value,
					"vue-data-ui-fulscreen--off": !Y.value
				}),
				viewBox: `0 0 ${U.value.width <= 0 ? 10 : U.value.width} ${U.value.height <= 0 ? 10 : U.value.height}`,
				style: v(`max-width:100%; overflow: visible; background:transparent;color:${I.value.style.chart.color}`)
			}, [
				Ee(w($e)),
				e.$slots["chart-background"] ? (y(), d("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: U.value.width <= 0 ? 10 : U.value.width,
					height: U.value.height <= 0 ? 10 : U.value.height,
					style: { pointerEvents: "none" }
				}, [S(e.$slots, "chart-background", {}, void 0, !0)], 8, Le)) : u("", !0),
				f("defs", null, [Ee(ye, {
					t: "linear",
					id: `funnel_area_${k.value}`,
					x1: "0%",
					x2: "100%",
					y1: "0%",
					y2: "0%",
					stops: [
						[
							"0%",
							I.value.style.chart.backgroundColor,
							0
						],
						[
							"20%",
							I.value.style.chart.area.color,
							1
						],
						[
							"100%",
							I.value.style.chart.area.color,
							1
						]
					]
				}, null, 8, ["id", "stops"])]),
				I.value.style.chart.circleLinks.show ? (y(), d("line", h({ key: 1 }, Ct.value, {
					stroke: I.value.style.chart.circleLinks.color,
					"stroke-width": 12 * I.value.style.chart.circleLinks.widthRatio,
					"stroke-linecap": "round",
					class: { animated: I.value.useCssAnimation && !w(E) },
					style: {
						strokeDasharray: I.value.useCssAnimation && !w(E) ? G.value.height : 0,
						strokeDashoffset: I.value.useCssAnimation && !w(E) ? G.value.height : 0
					}
				}), null, 16, Re)) : u("", !0),
				(y(!0), d(s, null, x(J.value, ({ cx: e, cy: t, r: n, fill: r }, i) => (y(), d("circle", h({ ref_for: !0 }, {
					cx: e,
					cy: t,
					r: n,
					fill: r
				}, {
					stroke: I.value.style.chart.circles.stroke,
					"stroke-width": I.value.style.chart.circles.strokeWidth,
					class: { animated: I.value.useCssAnimation && !M.value && !w(E) },
					style: { animationDelay: `${150 * i}ms` }
				}), null, 16, ze))), 256)),
				(y(!0), d(s, null, x(J.value, (e, t) => (y(), d("text", {
					x: e.cx,
					y: e.cy + H.value.circles / 3 + I.value.style.chart.circles.dataLabels.offsetY,
					"text-anchor": "middle",
					"font-size": H.value.circles,
					fill: I.value.style.chart.circles.dataLabels.adaptColorToBackground ? w(ie)(e.color) : I.value.style.chart.circles.dataLabels.color,
					"font-weight": I.value.style.chart.circles.dataLabels.bold ? "bold" : "normal",
					class: g({ animated: I.value.useCssAnimation && !M.value && !w(E) }),
					style: v({ animationDelay: `${150 * t}ms` })
				}, C(w(o)(I.value.style.chart.circles.dataLabels.formatter, e.proportion * 100, w(a)({
					v: e.proportion * 100,
					s: "%",
					r: I.value.style.chart.circles.dataLabels.rounding
				}), { datapoint: e })), 15, Be))), 256)),
				I.value.style.chart.area.show ? (y(), d("polygon", {
					key: 2,
					points: St.value,
					fill: `url(#funnel_area_${k.value})`,
					class: g({ animated: I.value.useCssAnimation && !M.value && !w(E) }),
					style: v({ transition: I.value.useCssAnimation ? `all ${150 * W.value.length}ms ease-in` : "none" })
				}, null, 14, Ve)) : u("", !0),
				(y(!0), d(s, null, x(J.value, ({ x: e, y: t, height: n, width: r, fill: i }, a) => (y(), d("rect", h({ ref_for: !0 }, {
					x: e,
					y: t,
					height: n,
					width: r,
					fill: i
				}, {
					stroke: I.value.style.chart.bars.stroke,
					"stroke-width": I.value.style.chart.bars.strokeWidth,
					rx: I.value.style.chart.bars.borderRadius,
					class: { animated: I.value.useCssAnimation && !M.value && !w(E) },
					style: { animationDelay: `${150 * a}ms` }
				}), null, 16, He))), 256)),
				(y(!0), d(s, null, x(J.value, (e, t) => (y(), d("g", null, [f("text", {
					x: e.x + e.width + I.value.style.chart.bars.dataLabels.name.offsetX + 12,
					y: e.cy - H.value.names / 2 + I.value.style.chart.bars.dataLabels.name.offsetY,
					"text-anchor": "start",
					"font-size": H.value.names,
					fill: I.value.style.chart.bars.dataLabels.name.color,
					"font-weight": I.value.style.chart.bars.dataLabels.name.bold ? "bold" : "normal",
					class: g({ animated: I.value.useCssAnimation && !M.value && !w(E) }),
					style: v({ animationDelay: `${150 * t}ms` })
				}, C(e.name), 15, Ue), f("text", {
					x: e.x + e.width + I.value.style.chart.bars.dataLabels.value.offsetX + 12,
					y: e.cy + H.value.values + I.value.style.chart.bars.dataLabels.value.offsetY,
					"text-anchor": "start",
					"font-size": H.value.values,
					fill: I.value.style.chart.bars.dataLabels.value.color,
					"font-weight": I.value.style.chart.bars.dataLabels.value.bold ? "bold" : "normal",
					class: g({ animated: I.value.useCssAnimation && !M.value && !w(E) }),
					style: v({ animationDelay: `${150 * t}ms` })
				}, C(w(o)(I.value.style.chart.bars.dataLabels.value.formatter, e.value, w(a)({
					p: I.value.style.chart.bars.dataLabels.value.prefix,
					v: e.value,
					s: I.value.style.chart.bars.dataLabels.value.suffix,
					r: I.value.style.chart.bars.dataLabels.value.rounding
				}), { datapoint: e })), 15, We)]))), 256)),
				S(e.$slots, "svg", { svg: {
					...U.value,
					isPrintingImg: w(z) || w(B) || w(Pt),
					isPrintingSvg: w(Ft)
				} }, void 0, !0)
			], 14, Ie)) : u("", !0),
			e.$slots.watermark ? (y(), d("div", Ge, [S(e.$slots, "watermark", _(m({ isPrinting: w(z) || w(B) || w(Pt) || w(Ft) })), void 0, !0)])) : u("", !0),
			F.value ? u("", !0) : (y(), l(w(Je), {
				key: 6,
				config: {
					type: "verticalBar",
					style: {
						backgroundColor: I.value.style.chart.backgroundColor,
						verticalBar: {
							axis: { color: "#CCCCCC" },
							color: "#CCCCCC"
						}
					}
				}
			}, null, 8, ["config"])),
			e.$slots.source ? (y(), d("div", {
				key: 7,
				ref_key: "source",
				ref: lt,
				dir: "auto"
			}, [S(e.$slots, "source", {}, void 0, !0)], 512)) : u("", !0),
			F.value && I.value.userOptions.buttons.table ? (y(), l(Ae($.value.component), h({ key: 8 }, $.value.props, {
				ref_key: "tableUnit",
				ref: N,
				onClose: jt
			}), we({
				content: T(() => [(y(), l(w(Xe), {
					key: `table_${st.value}`,
					colNames: Q.value.colNames,
					head: Q.value.head,
					body: Q.value.body,
					config: Q.value.config,
					title: I.value.table.useDialog ? "" : $.value.title,
					withCloseButton: !I.value.table.useDialog,
					onClose: jt
				}, {
					th: T(({ th: e }) => [f("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, Ke)]),
					td: T(({ td: e }) => [Te(C(e.name ? e.name : e), 1)]),
					_: 1
				}, 8, [
					"colNames",
					"head",
					"body",
					"config",
					"title",
					"withCloseButton"
				]))]),
				_: 2
			}, [I.value.table.useDialog ? {
				name: "title",
				fn: T(() => [Te(C($.value.title), 1)]),
				key: "0"
			} : void 0, I.value.table.useDialog ? {
				name: "actions",
				fn: T(() => [f("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[0] ||= (e) => Ot(I.value.userOptions.callbacks.csv)
				}, [Ee(w(qe), {
					name: "fileCsv",
					stroke: $.value.props.color
				}, null, 8, ["stroke"])])]),
				key: "1"
			} : void 0]), 1040)) : u("", !0)
		], 46, Fe));
	}
}, [["__scopeId", "data-v-909ef499"]]);
//#endregion
export { Pe as n, qe as t };
