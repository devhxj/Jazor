import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Kt as r, Pt as i, S as a, X as o, i as ee, jt as te, m as ne, pt as re, q as ie, tt as ae, w as oe } from "./lib-Bttd6u5E.js";
import { n as se, t as ce } from "./useHints-Dq_w2E8B.js";
import { t as le } from "./useConfig-DlNpz6P8.js";
import { t as ue } from "./usePrinter-DN5bYhTG.js";
import { t as s } from "./useNestedProp-vPNvh7rV.js";
import { t as de } from "./useThemeCheck-C43Tcqmk.js";
import { t as fe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as pe } from "./useUserOptionState-DK-_1ddE.js";
import { t as me } from "./vClickOutside-DUrZWttG.js";
import { t as he } from "./vue_ui_table_sparkline-DAbkUrNz.js";
import { Fragment as c, computed as l, createBlock as ge, createCommentVNode as u, createElementBlock as d, createElementVNode as f, createSlots as _e, createVNode as p, defineAsyncComponent as m, mergeProps as h, nextTick as ve, normalizeClass as g, normalizeStyle as _, onBeforeUnmount as ye, onMounted as be, openBlock as v, ref as y, renderList as b, renderSlot as x, shallowRef as xe, toDisplayString as S, unref as C, useCssVars as Se, useSlots as Ce, watch as we, withCtx as w, withDirectives as Te } from "vue";
//#region src/components/vue-ui-table-sparkline.vue
var Ee = /* @__PURE__ */ e({ default: () => T }), De = ["id"], Oe = { style: {
	"z-index": "1",
	"padding-right": "24px"
} }, ke = {
	key: 0,
	style: {
		display: "flex",
		flexDirection: "row",
		alignItems: "center"
	}
}, Ae = {
	key: 0,
	style: {
		display: "flex",
		flexDirection: "row",
		alignItems: "center"
	}
}, je = ["onClick"], Me = ["onClick"], Ne = ["data-cell"], Pe = ["data-cell", "onPointerenter"], Fe = ["data-cell"], Ie = ["data-cell"], Le = ["data-cell"], Re = ["data-cell"], ze = {
	key: 1,
	ref: "source",
	dir: "auto"
}, T = /*#__PURE__*/ fe({
	__name: "vue-ui-table-sparkline",
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
	setup(e, { expose: fe, emit: Ee }) {
		Se((e) => ({ v4821b9e9: e.tdo }));
		let T = m(() => import("./vue-ui-sparkline-jQ1WegfT.js").then((e) => e.n)), E = m(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Be = m(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), { vue_ui_table_sparkline: Ve } = le(), { isThemeValid: He, warnInvalidTheme: Ue } = de(), D = e, We = Ee, O = y(ie()), Ge = y(0), k = y(0), A = y(null), Ke = Ce(), qe = y(!1), j = l({
			get: () => Ze(),
			set: (e) => e
		});
		be(() => {
			Ke["chart-background"] && j.value.debug && console.warn("VueUiTableSparkline does not support the #chart-background slot.");
		}), se({
			config: () => j.value,
			dataset: () => D.dataset,
			component: "VueUiTableSparkline",
			rules: [
				ce.emptyArray,
				{
					test: (e) => e.some((e) => e?.values && e.values.length > 500),
					message: [
						"👀 One or more series have > 500 datapoints. Consider if you really need this level of detail.",
						"",
						"▶️ Use larger time scales, or aggregated values"
					]
				},
				{
					test: (e) => e.some((e) => e?.values && e.values.length > 1095),
					message: [
						"👀 One or more series have > 1095 datapoints. Above this threshold, the dataset is computed through an LTTB algorithm, to preserve the shape of the data without increasing the number of datapoints.",
						"",
						"▶️ If you need this level of detail, you can change config.downsample.threshold and set a higher value. Note that performance will be impacted."
					]
				}
			]
		});
		let M = l(() => j.value.userOptions.useCursorPointer), { userOptionsVisible: Je, setUserOptionsVisibility: Ye, keepUserOptionState: Xe } = pe({ config: j.value });
		function Ze() {
			let e = s({
				userConfig: D.config,
				defaultConfig: Ve
			}), t = e.theme;
			if (!t) return e;
			if (!He.value(e)) return Ue(e), e;
			let n = s({
				userConfig: he[t] || D.config,
				defaultConfig: e
			}), a = s({
				userConfig: D.config,
				defaultConfig: n
			});
			return {
				...a,
				customPalette: a.customPalette.length ? a.customPalette : r[t] || i
			};
		}
		we(() => D.config, (e) => {
			j.value = Ze(), Je.value = !j.value.userOptions.showOnChartHover, it(), k.value += 1;
		}, { deep: !0 }), we(() => D.dataset, (e) => {
			U.value = H.value, k.value += 1;
		}, { deep: !0 });
		let { isPrinting: Qe, isImaging: $e, generatePdf: et, generateImage: tt } = ue({
			elementId: `table_${O.value}`,
			fileName: j.value.title.text || "vue-ui-table-sparkline",
			options: j.value.userOptions.print
		}), nt = l(() => oe(j.value.customPalette)), N = y(null), P = y(!1), rt = l(() => j.value.responsiveBreakpoint);
		be(() => {
			it();
		});
		let F = y(j.value.colNames), I = xe(null);
		function it() {
			te(D.dataset) && ae({
				componentName: "VueUiTableSparkline",
				type: "dataset"
			}), I.value && I.value.disconnect(), I.value = new ResizeObserver((e) => {
				e.forEach((e) => {
					P.value = e.contentRect.width < rt.value;
				});
			}), N.value && I.value.observe(N.value), F.value = [];
			for (let e = 0; e < lt.value; e += 1) F.value.push(j.value.colNames[e] || `col ${e}`);
		}
		ye(() => {
			I.value && I.value.disconnect();
		});
		function L(e) {
			if ([null, void 0].includes(e)) return null;
			let t = Number(e);
			return Number.isFinite(t) ? t : null;
		}
		function R(e) {
			return typeof e == "number" && Number.isFinite(e);
		}
		function at(e = []) {
			return e.map(L).filter(R);
		}
		function z(e, t, n) {
			let r = L(e), i = L(t), a = R(r), o = R(i);
			return a && o ? n === -1 ? r - i : i - r : a ? -1 : +!!o;
		}
		function B(e, t, n = {}) {
			let r = L(e);
			return r === null ? "-" : ee(j.value.formatter, r, o({
				p: j.value.prefix,
				v: r,
				s: j.value.suffix,
				r: t
			}), n);
		}
		let V = l(() => (D.dataset.forEach((e, t) => {
			re({
				datasetObject: e,
				requiredAttributes: ["name", "values"]
			}).forEach((e) => {
				ae({
					componentName: "VueUiTableSparkline",
					type: "datasetSerieAttribute",
					property: e,
					index: t
				});
			});
		}), D.dataset.map((e, t) => {
			let n = e.values || [], r = at(n), o = r.length ? r.reduce((e, t) => e + t, 0) : null, ee = r.length ? o / r.length : null, te = r.length ? ne(r) : null;
			return {
				...e,
				values: n,
				color: a(e.color) || nt.value[t] || i[t] || i[t % i.length],
				sum: o,
				average: ee,
				median: te,
				sparklineDataset: n.map((e, t) => ({
					period: j.value.colNames[t] || `col ${t}`,
					value: L(e)
				}))
			};
		})));
		function ot(e) {
			let t = (e[0]?.values || []).map((t, n) => e.map((e) => e?.values?.[n] ?? null)).map((e) => e.map((e, t) => [e, t]).sort((e, t) => z(e[0], t[0], 1)).map((e) => e[1]));
			return e.map((e, n) => ({
				...e,
				values: e.values || [],
				orders: t[n]
			}));
		}
		let H = l(() => ot(V.value)), U = y(H.value), st = l(() => Math.max(...U.value.map((e) => (e.values || []).length))), W = y(void 0), G = y(!1), K = y(void 0), ct = y(1);
		function q() {
			G.value = !1, K.value = void 0, Z.value = void 0, ct.value = 1, $.value.forEach((e) => e.state = 1), X.value = {
				name: 1,
				sum: 1,
				average: 1,
				median: 1
			}, U.value = H.value;
		}
		function J(e, t, n) {
			if ([
				"name",
				"sum",
				"average",
				"median"
			].includes(e.type)) {
				ut(e.type, t, n);
				return;
			}
			if (!gt(t)) return;
			if (Q.value = t, Z.value = void 0, ![null, void 0].includes(K.value) && t !== K.value && (K.value = void 0, q()), $.value[t].state === n && K.value === t) {
				K.value = void 0, q();
				return;
			}
			G.value = !0, K.value = t;
			let r = H.value.map((e) => e.values?.[t] ?? null), i = n;
			$.value[t].state = i, ct.value = i, t === W.value ? W.value = void 0 : W.value = t;
			let a = r.map((e, t) => [t, e]).sort((e, t) => z(e[1], t[1], i)).map((e) => e[0]).map((e) => H.value[e]);
			U.value = a, k.value += 1;
		}
		let lt = l(() => Math.max(...D.dataset.map((e) => (e.values || []).length))), Y = l(() => {
			let e = F.value.map((e) => ({
				type: "reg",
				value: e
			}));
			if (!e.length) for (let t = 0; t < lt.value; t += 1) e.push({
				type: "reg",
				value: `col ${t + 1}`
			});
			j.value.showTotal && (e = [...e, {
				type: "sum",
				value: j.value.translations.total
			}]);
			let t;
			return t = j.value.showAverage && j.value.showMedian ? [
				...e,
				{
					type: "average",
					value: j.value.translations.average
				},
				{
					type: "median",
					value: j.value.translations.median
				}
			] : j.value.showAverage && !j.value.showMedian ? [...e, {
				type: "average",
				value: j.value.translations.average
			}] : !j.value.showAverage && j.value.showMedian ? [...e, {
				type: "median",
				value: j.value.translations.median
			}] : e, j.value.showSparklines ? [...t, {
				type: "chart",
				value: j.value.translations.chart
			}] : t;
		}), X = y({
			name: 1,
			sum: 1,
			average: 1,
			median: 1
		}), Z = y(void 0);
		function ut(e, t, n) {
			if (!U.value || U.value.length === 0 || !_t(e)) return;
			if (Z.value !== e && (Z.value = void 0), ![null, void 0].includes(K.value) && t !== K.value && q(), K.value = void 0, X.value[e] === n && Z.value) {
				Z.value = void 0, q();
				return;
			}
			Z.value = e, G.value = !0, X.value[e] = n, [null, void 0].includes(t) || ($.value[t].state = X.value[e]);
			let r = X.value[e], i = [...U.value].sort((t, n) => {
				let i = t[e], a = n[e];
				return typeof i == "string" && typeof a == "string" ? r === -1 ? i.localeCompare(a) : a.localeCompare(i) : z(i, a, r);
			});
			U.value = i;
		}
		let Q = y(void 0), dt = y(void 0);
		function ft({ dataIndex: e, serieIndex: t }) {
			Q.value = e, dt.value = t, A.value[e] && !P.value && A.value[e].scrollIntoView({
				behavior: "smooth",
				block: "nearest",
				inline: "center"
			});
		}
		let pt = y(!1);
		function mt(e) {
			pt.value = e, Ge.value += 1;
		}
		function ht(e = null) {
			ve(() => {
				let r = [j.value.translations.serie].concat(Y.value), i = V.value.map((e, t) => [
					[e.name],
					e.values,
					[e.sum],
					[e.average],
					[e.median]
				]), a = [r].concat(i), o = n(a);
				e ? e(o) : t({
					csvContent: o,
					title: j.value.title.text || "vue-ui-table-sparkline"
				});
			});
		}
		function gt(e) {
			return j.value.sortedDataColumnIndices.includes(e);
		}
		function _t(e) {
			return e.type === "name" || e === "name" ? j.value.sortedSeriesName : e.type === "sum" || e === "sum" ? j.value.sortedSum : e.type === "average" || e === "average" ? j.value.sortedAverage : e.type === "median" || e === "median" ? j.value.sortedMedian : !1;
		}
		function vt(e, t, n) {
			return [
				"sum",
				"average",
				"median"
			].includes(t.type) ? Z.value === t.type && X.value[t.type] === n ? 1 : .3 : e === K.value && $.value[e].state === n ? 1 : .3;
		}
		function yt() {
			j.value.resetSortOnClickOutside && q();
		}
		let $ = l({
			get: () => Y.value.map((e) => ({ state: 1 })),
			set: (e) => e
		});
		function bt(e) {
			if (e?.stage === "start") {
				qe.value = !0;
				return;
			}
			if (e?.stage === "end") {
				qe.value = !1;
				return;
			}
			tt();
		}
		async function xt() {
			if (We("copyAlt", {
				config: j.value,
				dataset: V.value
			}), !j.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(j.value.userOptions.callbacks.altCopy({
				config: j.value,
				dataset: V.value
			}));
		}
		return fe({
			generatePdf: et,
			generateImage: tt,
			generateCsv: ht,
			restoreOrder: q,
			copyAlt: xt
		}), (e, t) => (v(), d("div", {
			ref_key: "tableContainer",
			ref: N,
			class: g({
				"vue-data-ui-component": !0,
				"vue-ui-table-sparkline": !0,
				"vue-ui-responsive": P.value
			}),
			style: { overflow: "hidden" },
			id: `table_${O.value}`,
			onMouseenter: t[3] ||= () => C(Ye)(!0),
			onMouseleave: t[4] ||= () => C(Ye)(!1)
		}, [
			j.value.title.text ? (v(), d("div", {
				key: 0,
				class: "vue-ui-table-sparkline-caption",
				style: _({ backgroundColor: j.value.title.backgroundColor })
			}, [f("div", {
				class: "atom-title",
				style: _({
					fontSize: `${j.value.title.fontSize}px`,
					fontWeight: j.value.title.bold ? "bold" : "normal",
					color: j.value.title.color,
					textAlign: j.value.title.textAlign
				})
			}, S(j.value.title.text), 5), j.value.title.subtitle.text ? (v(), d("div", {
				key: 0,
				class: "atom-subtitle",
				style: _({
					fontSize: `${j.value.title.subtitle.fontSize}px`,
					fontWeight: j.value.title.subtitle.bold ? "bold" : "normal",
					color: j.value.title.subtitle.color,
					textAlign: j.value.title.textAlign
				})
			}, S(j.value.title.subtitle.text), 5)) : u("", !0)], 4)) : u("", !0),
			f("div", {
				style: { overflow: "auto" },
				onPointerleave: t[2] ||= (e) => {
					dt.value = void 0, Q.value = void 0;
				}
			}, [f("table", {
				class: "vue-ui-data-table",
				style: _({
					fontFamily: j.value.fontFamily,
					position: "relative"
				})
			}, [f("thead", Oe, [Te((v(), d("tr", {
				role: "row",
				class: "vue-ui-data-table__thead-row",
				style: _({
					backgroundColor: j.value.thead.backgroundColor,
					color: j.value.thead.color
				})
			}, [f("th", {
				role: "cell",
				style: _({
					backgroundColor: j.value.thead.backgroundColor,
					border: j.value.thead.outline,
					textAlign: j.value.thead.textAlign,
					fontWeight: j.value.thead.bold ? "bold" : "normal"
				}),
				class: "sticky-col-first"
			}, [f("div", { style: _({
				display: "flex",
				flexDirection: "row",
				alignItems: "center",
				gap: "3px",
				justifyContent: j.value.thead.textAlign
			}) }, [f("span", null, S(j.value.translations.serie), 1), U.value.length > 1 && j.value.sortedSeriesName ? (v(), d("div", ke, [f("button", {
				class: "vue-ui-table-sparkline-sorting-button vue-ui-table-sparkline-sorting-button-down",
				onClick: t[0] ||= (e) => J({ type: "name" }, null, -1),
				style: _({ cursor: M.value ? "pointer" : "default" })
			}, [p(C(E), {
				size: 12,
				name: "arrowBottom",
				stroke: j.value.thead.color,
				style: _({ opacity: Z.value === "name" && X.value.name === -1 ? 1 : .3 })
			}, null, 8, ["stroke", "style"])], 4), f("button", {
				class: "vue-ui-table-sparkline-sorting-button vue-ui-table-sparkline-sorting-button-up",
				onClick: t[1] ||= (e) => J({ type: "name" }, null, 1),
				style: _({ cursor: M.value ? "pointer" : "default" })
			}, [p(C(E), {
				size: 12,
				name: "arrowTop",
				stroke: j.value.thead.color,
				style: _({ opacity: Z.value === "name" && X.value.name === 1 ? 1 : .3 })
			}, null, 8, ["stroke", "style"])], 4)])) : u("", !0)], 4)], 4), (v(!0), d(c, null, b(Y.value, (t, n) => (v(), d("th", {
				role: "cell",
				style: _({
					background: j.value.thead.backgroundColor,
					border: j.value.thead.outline,
					textAlign: j.value.thead.textAlign,
					fontWeight: j.value.thead.bold ? "bold" : "normal",
					minWidth: n === Y.value.length - 1 ? `${j.value.sparkline.dimensions.width}px` : "48px",
					paddingRight: n === Y.value.length - 1 && j.value.userOptions.show ? "36px" : ""
				}),
				class: g({ "sticky-col": n === Y.value.length - 1 && j.value.showSparklines })
			}, [f("div", { style: _({
				display: "flex",
				flexDirection: "row",
				alignItems: "center",
				gap: "3px",
				justifyContent: j.value.thead.textAlign
			}) }, [f("span", null, S(t.value), 1), U.value.length > 1 && (gt(n) || _t(t)) ? (v(), d("div", Ae, [f("button", {
				class: "vue-ui-table-sparkline-sorting-button vue-ui-table-sparkline-sorting-button-down",
				onClick: () => J(t, n, -1),
				style: _({ cursor: M.value ? "pointer" : "default" })
			}, [p(C(E), {
				size: 12,
				name: "arrowBottom",
				stroke: j.value.thead.color,
				style: _({ opacity: vt(n, t, -1) })
			}, null, 8, ["stroke", "style"])], 12, je), f("button", {
				class: "vue-ui-table-sparkline-sorting-button vue-ui-table-sparkline-sorting-button-up",
				onClick: () => J(t, n, 1),
				style: _({ cursor: M.value ? "pointer" : "default" })
			}, [p(C(E), {
				size: 12,
				name: "arrowTop",
				stroke: j.value.thead.color,
				style: _({ opacity: vt(n, t, 1) })
			}, null, 8, ["stroke", "style"])], 12, Me)])) : u("", !0)], 4), j.value.userOptions.show && n === Y.value.length - 1 && (C(Xe) || C(Je)) ? (v(), ge(C(Be), {
				ref_for: !0,
				ref: "details",
				key: `user_option_${Ge.value}`,
				backgroundColor: j.value.thead.backgroundColor,
				color: j.value.thead.color,
				isPrinting: C(Qe),
				isImaging: C($e),
				uid: O.value,
				hasPdf: j.value.userOptions.buttons.pdf,
				hasXls: j.value.userOptions.buttons.csv,
				hasImg: j.value.userOptions.buttons.img,
				hasFullscreen: j.value.userOptions.buttons.fullscreen,
				hasAltCopy: j.value.userOptions.buttons.altCopy,
				isFullscreen: pt.value,
				titles: { ...j.value.userOptions.buttonTitles },
				chartElement: N.value,
				position: j.value.userOptions.position,
				callbacks: j.value.userOptions.callbacks,
				printScale: j.value.userOptions.print.scale,
				isCursorPointer: M.value,
				onToggleFullscreen: mt,
				onGeneratePdf: C(et),
				onGenerateImage: bt,
				onGenerateCsv: ht,
				onCopyAlt: xt,
				style: _({ visibility: C(Xe) ? C(Je) ? "visible" : "hidden" : "visible" })
			}, _e({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: w(({ isOpen: t, color: n }) => [x(e.$slots, "menuIcon", h({ ref_for: !0 }, {
						isOpen: t,
						color: n
					}), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: w(() => [x(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: w(() => [x(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: w(() => [x(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: w(({ toggleFullscreen: t, isFullscreen: n }) => [x(e.$slots, "optionFullscreen", h({ ref_for: !0 }, {
						toggleFullscreen: t,
						isFullscreen: n
					}), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: w(({ altCopy: t }) => [x(e.$slots, "optionAltCopy", h({ ref_for: !0 }, { altCopy: t }), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: w(() => [x(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: w(() => [x(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "7"
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
				"hasAltCopy",
				"isFullscreen",
				"titles",
				"chartElement",
				"position",
				"callbacks",
				"printScale",
				"isCursorPointer",
				"onGeneratePdf",
				"style"
			])) : u("", !0)], 6))), 256))], 4)), [[C(me), yt]])]), f("tbody", null, [(v(!0), d(c, null, b(U.value, (e, t) => (v(), d("tr", {
				role: "row",
				style: _({
					backgroundColor: j.value.tbody.backgroundColor,
					color: j.value.tbody.color
				}),
				class: g({
					"vue-ui-data-table__tbody__row": !0,
					"vue-ui-data-table__tbody__row-even": t % 2 == 0,
					"vue-ui-data-table__tbody__row-odd": t % 2 != 0
				})
			}, [
				f("td", {
					role: "cell",
					style: _({
						backgroundColor: j.value.tbody.backgroundColor,
						border: j.value.tbody.outline,
						fontSize: `${j.value.tbody.fontSize}px`,
						fontWeight: j.value.tbody.bold ? "bold" : "normal",
						textAlign: j.value.tbody.textAlign
					}),
					"data-cell": j.value.translations.serie,
					class: "vue-ui-data-table__tbody__td sticky-col-first"
				}, [f("div", {
					dir: "auto",
					style: _({
						display: "flex",
						flexDirection: "row",
						alignItems: "center",
						gap: "6px",
						justifyContent: j.value.tbody.textAlign
					})
				}, [j.value.tbody.showColorMarker ? (v(), d("span", {
					key: 0,
					style: _({ color: e.color })
				}, "⬤", 4)) : u("", !0), f("span", null, S(e.name ?? "-"), 1)], 4)], 12, Ne),
				(v(!0), d(c, null, b(st.value, (n, r) => (v(), d("td", {
					dir: "auto",
					role: "cell",
					ref_for: !0,
					ref_key: "TD",
					ref: A,
					style: _({
						border: j.value.tbody.outline,
						fontSize: `${j.value.tbody.fontSize}px`,
						fontWeight: j.value.tbody.bold ? "bold" : "normal",
						textAlign: j.value.tbody.textAlign,
						background: Q.value !== void 0 && r === Q.value ? j.value.tbody.selectedColor.useSerieColor ? `${e.color.length > 7 ? e.color.slice(0, -2) : e.color}33` : j.value.tbody.selectedColor.fallback : ""
					}),
					"data-cell": Y.value[r] ? Y.value[r].value : "",
					class: "vue-ui-data-table__tbody__td",
					onPointerenter: (e) => {
						dt.value = t, Q.value = r;
					}
				}, S(B(e.values[r], j.value.roundingValues, {
					datapoint: e,
					seriesIndex: t,
					datapointIndex: r
				})), 45, Pe))), 256)),
				j.value.showTotal ? (v(), d("td", {
					key: 0,
					dir: "auto",
					role: "cell",
					style: _({
						border: j.value.tbody.outline,
						fontSize: `${j.value.tbody.fontSize}px`,
						fontWeight: j.value.tbody.bold ? "bold" : "normal",
						textAlign: j.value.tbody.textAlign
					}),
					"data-cell": j.value.translations.total,
					class: "vue-ui-data-table__tbody__td"
				}, S(B(e.sum, j.value.roundingTotal, {
					datapoint: e.sum,
					seriesIndex: t
				})), 13, Fe)) : u("", !0),
				j.value.showAverage ? (v(), d("td", {
					key: 1,
					dir: "auto",
					role: "cell",
					style: _({
						border: j.value.tbody.outline,
						fontSize: `${j.value.tbody.fontSize}px`,
						fontWeight: j.value.tbody.bold ? "bold" : "normal",
						textAlign: j.value.tbody.textAlign
					}),
					"data-cell": j.value.translations.average,
					class: "vue-ui-data-table__tbody__td"
				}, S(B(e.average, j.value.roundingAverage, {
					datapoint: e.average,
					seriesIndex: t
				})), 13, Ie)) : u("", !0),
				j.value.showMedian ? (v(), d("td", {
					key: 2,
					dir: "auto",
					role: "cell",
					style: _({
						border: j.value.tbody.outline,
						fontSize: `${j.value.tbody.fontSize}px`,
						fontWeight: j.value.tbody.bold ? "bold" : "normal",
						textAlign: j.value.tbody.textAlign
					}),
					"data-cell": j.value.translations.median,
					class: "vue-ui-data-table__tbody__td"
				}, S(B(e.median, j.value.roundingMedian, {
					datapoint: e.median,
					seriesIndex: t
				})), 13, Le)) : u("", !0),
				j.value.showSparklines ? (v(), d("td", {
					key: 3,
					role: "cell",
					"data-cell": j.value.translations.chart,
					style: _({
						border: j.value.tbody.outline,
						fontSize: `${j.value.tbody.fontSize}px`,
						fontWeight: j.value.tbody.bold ? "bold" : "normal",
						textAlign: j.value.tbody.textAlign,
						backgroundColor: j.value.tbody.backgroundColor,
						padding: "0"
					}),
					class: "vue-ui-data-table__tbody__td sticky-col"
				}, [p(C(T), {
					onHoverIndex: ({ index: e }) => ft({
						dataIndex: e,
						serieIndex: t
					}),
					"height-ratio": j.value.sparkline.dimensions.heightRatio,
					"forced-padding": 30,
					dataset: e.sparklineDataset,
					showInfo: !1,
					selectedIndex: Q.value,
					config: {
						type: j.value.sparkline.type,
						style: {
							backgroundColor: "transparent",
							animation: {
								show: j.value.sparkline.animation.show && !C(Qe) && !C($e),
								animationFrames: j.value.sparkline.animation.animationFrames
							},
							padding: { right: 12 },
							line: {
								color: e.color,
								smooth: j.value.sparkline.smooth,
								cutNullValues: j.value.sparkline.cutNullValues,
								strokeWidth: j.value.sparkline.strokeWidth
							},
							bar: { color: e.color },
							area: {
								color: e.color,
								opacity: j.value.sparkline.showArea ? 16 : 0,
								useGradient: j.value.sparkline.useGradient
							},
							verticalIndicator: { color: e.color },
							plot: {
								radius: 9,
								stroke: j.value.tbody.backgroundColor,
								strokeWidth: 3
							}
						}
					}
				}, null, 8, [
					"onHoverIndex",
					"height-ratio",
					"dataset",
					"selectedIndex",
					"config"
				])], 12, Re)) : u("", !0)
			], 6))), 256))])], 4)], 32),
			e.$slots.source ? (v(), d("div", ze, [x(e.$slots, "source", {}, void 0, !0)], 512)) : u("", !0)
		], 42, De));
	}
}, [["__scopeId", "data-v-004f0a63"]]);
//#endregion
export { Ee as n, T as t };
