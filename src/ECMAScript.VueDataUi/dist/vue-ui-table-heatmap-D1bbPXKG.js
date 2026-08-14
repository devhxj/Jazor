import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, jt as r, m as i, q as a, r as ee, tt as te, yt as ne } from "./lib-Bttd6u5E.js";
import { n as re, t as ie } from "./useHints-Dq_w2E8B.js";
import { t as ae } from "./useConfig-DlNpz6P8.js";
import { t as oe } from "./usePrinter-DN5bYhTG.js";
import { t as o } from "./useNestedProp-vPNvh7rV.js";
import { t as se } from "./useThemeCheck-C43Tcqmk.js";
import { t as ce } from "./Shape-C21CMlWS.js";
import { t as s } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as le } from "./useUserOptionState-DK-_1ddE.js";
import { t as ue } from "./vue_ui_table_heatmap-w8vx5k6f.js";
import { Fragment as c, computed as l, createBlock as de, createCommentVNode as u, createElementBlock as d, createElementVNode as f, createSlots as fe, createVNode as pe, defineAsyncComponent as me, guardReactiveProps as p, mergeProps as m, nextTick as he, normalizeClass as h, normalizeProps as g, normalizeStyle as _, onMounted as v, openBlock as y, ref as b, renderList as x, renderSlot as S, unref as C, useCssVars as ge, useSlots as _e, watch as ve, withCtx as w } from "vue";
//#region src/components/vue-ui-table-heatmap.vue
var T = /* @__PURE__ */ e({ default: () => N }), ye = ["id"], be = { role: "cell" }, xe = { role: "row" }, E = ["data-cell"], D = {
	key: 0,
	style: {
		display: "flex",
		"flex-direction": "row",
		gap: "2px",
		"align-items": "center"
	}
}, O = ["height", "width"], k = {
	key: 0,
	role: "cell",
	"data-cell": "sum"
}, A = {
	key: 1,
	role: "cell",
	"data-cell": "average"
}, j = {
	key: 2,
	role: "cell",
	"data-cell": "median"
}, M = {
	key: 1,
	ref: "source",
	dir: "auto"
}, N = /*#__PURE__*/ s({
	__name: "vue-ui-table-heatmap",
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
	setup(e, { expose: s }) {
		ge((e) => ({
			v1ad1563c: Ae.value,
			v63a351d2: ke.value
		}));
		let T = me(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), { vue_ui_table_heatmap: N } = ae(), { isThemeValid: Se, warnInvalidTheme: P } = se(), F = e, I = b(a()), L = b(!1), R = b(null), z = b(!1), B = b(0), Ce = _e(), V = l({
			get: () => K(),
			set: (e) => e
		}), H = l(() => V.value.debug);
		v(() => {
			Ce["chart-background"] && H.value && console.warn("VueUiTableHeatmap does not support the #chart-background slot.");
		}), re({
			config: () => V.value,
			dataset: () => F.dataset,
			component: "VueUiTableHeatmap",
			rules: [
				ie.emptyArray,
				{
					test: (e) => e.length > 31,
					message: [
						"👀 The number of series is > 31. Consider:",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display."
					]
				},
				{
					test: (e) => e.some((e) => e?.values && e?.values.length > 31),
					message: [
						"👀 Some series have a number of data points > 31. Consider:",
						"",
						"▶️ Using filters to let users choose a maximum number of data points to display."
					]
				}
			]
		});
		let { userOptionsVisible: U, setUserOptionsVisibility: W, keepUserOptionState: G } = le({ config: V.value });
		function K() {
			let e = o({
				userConfig: F.config,
				defaultConfig: N
			}), t = e.theme;
			if (!t) return e;
			if (!Se.value(e)) return P(e), e;
			let n = o({
				userConfig: ue[t] || F.config,
				defaultConfig: e
			});
			return o({
				userConfig: F.config,
				defaultConfig: n
			});
		}
		ve(() => F.config, (e) => {
			V.value = K(), U.value = !V.value.userOptions.showOnChartHover, Y();
		}, { deep: !0 });
		let { isPrinting: we, isImaging: Te, generatePdf: q, generateImage: J } = oe({
			elementId: `table_heatmap_${I.value}`,
			fileName: "vue-ui-table-heatmap",
			options: V.value.userOptions.print
		}), Ee = l(() => V.value.table.responsiveBreakpoint), De = l(() => !!F.dataset && F.dataset.length);
		v(() => {
			Y();
		});
		function Y() {
			r(F.dataset) && te({
				componentName: "VueUiTableHeatmap",
				type: "dataset",
				debug: H.value
			});
			let e = new ResizeObserver((e) => {
				e.forEach((e) => {
					L.value = e.contentRect.width < Ee.value;
				});
			});
			R.value && e.observe(R.value);
		}
		let X = l(() => F.dataset.map((e) => {
			let t = e.values.map((e) => isNaN(e) ? 0 : e), n = t.reduce((e, t) => e + t, 0);
			return {
				...e,
				values: t,
				serieExtremes: {
					max: Math.max(...t),
					min: Math.min(...t)
				},
				sum: n,
				average: n / t.length,
				median: i(t),
				displayValues: [e.name, ...e.values],
				id: a()
			};
		})), Z = l(() => {
			let e = X.value.flatMap((e) => e.values);
			return {
				min: Math.min(...e),
				max: Math.max(...e)
			};
		});
		function Oe(e, t) {
			let n = V.value.style.heatmapColors.useIndividualScale;
			return ne(V.value.style.heatmapColors.min, V.value.style.heatmapColors.max, n ? t.min : Z.value.min, n ? t.max : Z.value.max, e);
		}
		let Q = l(() => X.value.map((e) => ({
			...e,
			colors: e.displayValues.map((t) => isNaN(t) ? V.value.style.backgroundColor : Oe(t, e.serieExtremes))
		}))), ke = l(() => V.value.style.backgroundColor), Ae = l(() => `${V.value.table.borderWidth}px`);
		function $(e = null) {
			he(() => {
				let r = Q.value.map((e) => [
					[e.name],
					e.displayValues,
					[e.sum],
					[e.average],
					[e.median]
				]), i = [[
					[""],
					V.value.table.head.values,
					["sum"],
					["average"],
					["median"]
				]].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: "vue-ui-table-heatmap"
				});
			});
		}
		function je(e) {
			z.value = e, B.value += 1;
		}
		return s({
			generatePdf: q,
			generateCsv: $,
			generateImage: J
		}), (e, t) => (y(), d("div", {
			ref_key: "tableContainer",
			ref: R,
			style: _(`width:100%; overflow-x:auto; container-type: inline-size;padding-top:${V.value.userOptions.show ? "36px" : ""}; ${z.value ? "vue-data-ui-wrapper-fullscreen" : ""}; position:relative;`),
			class: h({
				"vue-data-ui-component": !0,
				"vue-ui-responsive": L.value
			}),
			id: `table_heatmap_${I.value}`,
			onMouseenter: t[0] ||= () => C(W)(!0),
			onMouseleave: t[1] ||= () => C(W)(!1)
		}, [
			V.value.userOptions.show && De.value && (C(G) || C(U)) ? (y(), de(C(T), {
				ref: "details",
				key: `user_option_${B.value}`,
				backgroundColor: V.value.style.backgroundColor,
				color: V.value.style.color,
				isPrinting: C(we),
				isImaging: C(Te),
				uid: I.value,
				hasPdf: V.value.userOptions.buttons.pdf,
				hasXls: V.value.userOptions.buttons.csv,
				hasImg: V.value.userOptions.buttons.img,
				hasFullscreen: V.value.userOptions.buttons.fullscreen,
				isFullscreen: z.value,
				titles: { ...V.value.userOptions.buttonTitles },
				chartElement: R.value,
				position: V.value.userOptions.position,
				callbacks: V.value.userOptions.callbacks,
				printScale: V.value.userOptions.print.scale,
				onToggleFullscreen: je,
				onGeneratePdf: C(q),
				onGenerateCsv: $,
				onGenerateImage: C(J),
				style: _({ visibility: C(G) ? C(U) ? "visible" : "hidden" : "visible" })
			}, fe({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: w(({ isOpen: t, color: n }) => [S(e.$slots, "menuIcon", g(p({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: w(() => [S(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: w(() => [S(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: w(() => [S(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: w(({ toggleFullscreen: t, isFullscreen: n }) => [S(e.$slots, "optionFullscreen", g(p({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: w(() => [S(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: w(() => [S(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "6"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"hasPdf",
				"hasXls",
				"hasImg",
				"hasFullscreen",
				"isFullscreen",
				"titles",
				"chartElement",
				"position",
				"callbacks",
				"printScale",
				"onGeneratePdf",
				"onGenerateImage",
				"style"
			])) : u("", !0),
			f("table", {
				class: h({ "vue-ui-table-heatmap": !0 }),
				style: _(`width:100%;font-family:${V.value.style.fontFamily};background:${V.value.style.backgroundColor};`)
			}, [
				f("caption", null, [S(e.$slots, "caption", {}, void 0, !0)]),
				f("thead", null, [f("tr", {
					role: "row",
					style: _(`background:${V.value.table.head.backgroundColor};color:${V.value.table.head.color}`)
				}, [(y(!0), d(c, null, x(V.value.table.head.values, (t, n) => (y(), d("th", be, [S(e.$slots, "head", m({ ref_for: !0 }, {
					value: t,
					rowIndex: n,
					type: typeof t,
					isResponsive: L.value
				}), void 0, !0)]))), 256))], 4)]),
				f("tbody", null, [(y(!0), d(c, null, x(Q.value, (t, n) => (y(), d("tr", xe, [
					(y(!0), d(c, null, x(t.displayValues, (r, i) => (y(), d("td", {
						role: "cell",
						"data-cell": V.value.table.head.values[i]
					}, [t.color && i === 0 ? (y(), d("div", D, [t.color ? (y(), d("svg", {
						key: 0,
						height: V.value.style.shapeSize,
						width: V.value.style.shapeSize,
						viewBox: "0 0 20 20",
						style: {
							background: "none",
							overflow: "visible"
						}
					}, [pe(ce, {
						plot: {
							x: 10,
							y: 10
						},
						color: t.color,
						radius: 9,
						shape: t.shape || "circle"
					}, null, 8, ["color", "shape"])], 8, O)) : u("", !0), S(e.$slots, "rowTitle", m({ ref_for: !0 }, {
						value: r,
						rowIndex: n,
						colIndex: i,
						type: typeof r,
						isResponsive: L.value
					}), void 0, !0)])) : (y(), d(c, { key: 1 }, [i === 0 ? S(e.$slots, "rowTitle", m({ ref_for: !0 }, {
						value: r,
						rowIndex: n,
						colIndex: i,
						type: typeof r,
						isResponsive: L.value
					}), void 0, !0, 0) : u("", !0), i > 0 ? S(e.$slots, "cell", m({ ref_for: !0 }, {
						value: r,
						rowIndex: n,
						colIndex: i,
						type: typeof r,
						isResponsive: L.value,
						color: t.colors[i],
						textColor: C(ee)(t.colors[i])
					}), void 0, !0, 1) : u("", !0)], 64))], 8, E))), 256)),
					V.value.table.showSum ? (y(), d("td", k, [S(e.$slots, "sum", m({ ref_for: !0 }, {
						value: t.sum,
						rowIndex: n,
						isResponsive: L.value
					}), void 0, !0)])) : u("", !0),
					V.value.table.showAverage ? (y(), d("td", A, [S(e.$slots, "average", m({ ref_for: !0 }, {
						value: t.average,
						rowIndex: n,
						isResponsive: L.value
					}), void 0, !0)])) : u("", !0),
					V.value.table.showMedian ? (y(), d("td", j, [S(e.$slots, "median", m({ ref_for: !0 }, {
						value: t.median,
						rowIndex: n,
						isResponsive: L.value
					}), void 0, !0)])) : u("", !0)
				]))), 256))])
			], 4),
			e.$slots.source ? (y(), d("div", M, [S(e.$slots, "source", {}, void 0, !0)], 512)) : u("", !0)
		], 46, ye));
	}
}, [["__scopeId", "data-v-38352ed2"]]);
//#endregion
export { T as n, N as t };
