import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Jt as r, X as i, Xt as a, b as o, ct as s, i as c, jt as l, pt as u, q as d, r as f, t as p, tt as m, xt as ee, yt as te } from "./lib-Bttd6u5E.js";
import { n as ne, t as re } from "./useHints-Dq_w2E8B.js";
import { t as ie } from "./useTimeLabels-d2f-W1L4.js";
import { t as ae } from "./useConfig-DlNpz6P8.js";
import { t as oe } from "./usePrinter-DN5bYhTG.js";
import { n as se, t as ce } from "./BaseScanner-DZvpgOjM.js";
import { t as le } from "./useNestedProp-vPNvh7rV.js";
import { t as ue } from "./useThemeCheck-C43Tcqmk.js";
import { t as de } from "./useChartExport-DNiwdPmb.js";
import { t as fe } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as pe } from "./img-Bnokohej.js";
import { n as me } from "./Title-BE3qg9xl.js";
import { t as he } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ge, t as _e } from "./useResponsive-ZtArZtUf.js";
import { t as ve } from "./vue-ui-accordion-DegI2lzR.js";
import { t as ye } from "./A11yDataTable-DdRsVULz.js";
import { t as be } from "./useUserOptionState-DK-_1ddE.js";
import { t as xe } from "./useChartAccessibility-DYqac8yF.js";
import { t as Se } from "./useTableResponsive-BAqJPR68.js";
import { t as Ce } from "./vue_ui_heatmap-B2BBBSWG.js";
import { Fragment as h, computed as g, createBlock as we, createCommentVNode as _, createElementBlock as v, createElementVNode as y, createSlots as Te, createTextVNode as Ee, createVNode as De, defineAsyncComponent as b, guardReactiveProps as x, mergeProps as Oe, nextTick as ke, normalizeClass as Ae, normalizeProps as S, normalizeStyle as C, onBeforeUnmount as je, onMounted as Me, openBlock as w, reactive as Ne, ref as T, renderList as E, renderSlot as D, resolveDynamicComponent as Pe, shallowRef as Fe, toDisplayString as O, toRefs as Ie, unref as k, useCssVars as Le, vShow as Re, watch as ze, watchEffect as Be, withCtx as A, withDirectives as Ve, withKeys as He } from "vue";
//#region src/useLabelObserverEfffect.js
function Ue({ elementRef: e, callback: t, attr: n, earlyReturn: r = !1, retryFrames: i = 12, alsoAfterFontsReady: a = !0 }) {
	if (r) return;
	let o = (e) => {
		if (!e) return;
		let r;
		if (typeof e.getBBox == "function") try {
			let t = e.getBBox();
			r = n === "width" ? t.width : t.height;
		} catch {}
		if (typeof r != "number" || Number.isNaN(r)) try {
			let t = e.getBoundingClientRect();
			r = n === "width" ? t.width : t.height;
		} catch {
			r = void 0;
		}
		typeof r == "number" && !Number.isNaN(r) && t(r);
	};
	Be((t) => {
		let n = e.value;
		if (!n) return;
		let r = !1;
		(async () => {
			await ke();
			for (let t = 0; t < i; t += 1) {
				if (r) return;
				await new Promise((e) => requestAnimationFrame(e));
				let t = e.value;
				if (!t) return;
				o(t);
			}
			if (a && typeof document < "u" && document.fonts && document.fonts.ready) {
				try {
					await document.fonts.ready;
				} catch {}
				!r && e.value && o(e.value);
			}
		})();
		let s = new MutationObserver(() => {
			e.value && requestAnimationFrame(() => {
				e.value && o(e.value);
			});
		});
		s.observe(n, {
			childList: !0,
			subtree: !0,
			characterData: !0,
			attributes: !0,
			attributeFilter: [
				"transform",
				"style",
				"class"
			]
		});
		let c;
		if (typeof ResizeObserver < "u") {
			let t = n.ownerSVGElement ? n.ownerSVGElement : n;
			c = new ResizeObserver(() => {
				let t = e.value;
				t && o(t);
			}), c.observe(t);
		}
		t(() => {
			r = !0, s.disconnect(), c && c.disconnect();
		});
	}, { flush: "post" });
}
//#endregion
//#region src/directives/vFitText.js
var We = {
	mounted(e, t) {
		Ge(e, t.value);
	},
	updated(e, t) {
		Ge(e, t.value);
	}
};
function Ge(e, { cellWidth: t, cellHeight: n, maxFontSize: r, minFontSize: i, index: a, reportRotation: o, reportHide: s, rotateAll: c, hideAll: l }) {
	e.removeAttribute("transform"), e.removeAttribute("visibility");
	let u = !1, d = !1;
	e.setAttribute("font-size", r);
	let f = e.getComputedTextLength();
	if (f <= t) o(a, !1), s(a, !1);
	else {
		let n = Math.floor(r * t / f);
		n >= i ? (e.setAttribute("font-size", n), o(a, !1), s(a, !1)) : (o(a, !0), s(a, !1), u = !0);
	}
	if (c && (u = !0), u) {
		e.setAttribute("font-size", r);
		let t = e.getBBox(), o = t.x + t.width / 2, c = t.y + t.height / 2;
		e.setAttribute("transform", `rotate(-90 ${o} ${c})`);
		let l = e.getBBox().width;
		if (l <= n) s(a, !1);
		else {
			let t = Math.floor(r * n / l);
			t >= i ? (e.setAttribute("font-size", t), s(a, !1)) : (d = !0, s(a, !0));
		}
	}
	(l || d) && e.setAttribute("visibility", "hidden");
}
//#endregion
//#region src/components/vue-ui-heatmap.vue
var Ke = /* @__PURE__ */ e({ default: () => wt }), qe = ["id"], Je = ["id"], Ye = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Xe = [
	"x",
	"y",
	"width",
	"height"
], Ze = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke",
	"stroke-width"
], Qe = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke",
	"stroke-width"
], $e = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], et = [
	"data-a11y-cell-id",
	"x",
	"y",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width",
	"aria-label",
	"onMouseover",
	"onMouseout",
	"onClick"
], tt = [
	"font-size",
	"font-weight",
	"fill",
	"x",
	"y"
], nt = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], rt = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"stroke",
	"stroke-width"
], it = [
	"font-size",
	"fill",
	"x",
	"y",
	"font-weight"
], at = [
	"font-size",
	"fill",
	"x",
	"y",
	"font-weight"
], ot = [
	"text-anchor",
	"font-size",
	"fill",
	"font-weight",
	"transform"
], st = [
	"text-anchor",
	"font-size",
	"fill",
	"font-weight",
	"transform"
], ct = { key: 6 }, lt = [
	"x",
	"y",
	"width",
	"height",
	"stroke",
	"stroke-width"
], ut = { key: 7 }, dt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], ft = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], pt = { class: "vue-ui-heatmap-legend-gauge-right" }, mt = ["data-value"], ht = { class: "vue-ui-heatmap-gauge-indicator-value" }, gt = {
	key: 1,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, _t = {
	key: 5,
	class: "vue-data-ui-watermark"
}, vt = { class: "vue-ui-data-table" }, yt = { key: 0 }, bt = ["data-cell"], xt = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, St = ["data-cell"], Ct = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, wt = /*#__PURE__*/ he({
	__name: "vue-ui-heatmap",
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
	emits: ["selectDatapoint", "copyAlt"],
	setup(e, { expose: he, emit: Ge }) {
		Le((e) => ({ v3fc9460c: e.tdo }));
		let Ke = b(() => import("./Tooltip-DhjyfHwz.js")), wt = b(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Tt = b(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Et = b(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Dt = b(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ot = b(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_heatmap: kt } = ae(), { isThemeValid: At, warnInvalidTheme: jt } = ue(), j = e, Mt = Ge, Nt = g({
			get() {
				return !!j.dataset && j.dataset.length;
			},
			set(e) {
				return e;
			}
		}), M = T(d()), N = T(null), Pt = T(!1), Ft = T(""), It = T(void 0), P = T(null), Lt = T(0), Rt = T(null), zt = T(0), Bt = T(null), Vt = T(null), Ht = T(null), Ut = T(null), Wt = T(null), Gt = T(null), Kt = T(null), qt = T(null), Jt = T(null), Yt = T(null), Xt = T(null), Zt = T(null), F = Fe(null), I = T(null), Qt = T(null), $t = T({
			x: 0,
			y: 0
		}), en = T("pointer"), tn = T(!1), L = T(fn());
		ne({
			config: () => L.value,
			dataset: () => j.dataset,
			component: "VueUiHeatmap",
			rules: [re.emptyArray, re.noHint]
		});
		let nn = g(() => L.value.userOptions.useCursorPointer);
		function rn() {
			let e = Array(7).fill("_"), t = [], n = e.length;
			for (let r = 0; r < n; r += 1) {
				let n = [];
				for (let e = 0; e < 14; e += 1) n.push(r + e * 2);
				t.push({
					name: `${e[r]}`,
					values: n
				});
			}
			return t;
		}
		let an = g(() => r({
			defaultConfig: {
				table: { show: !1 },
				userOptions: { show: !1 },
				style: {
					backgroundColor: "#99999930",
					layout: {
						cells: {
							colors: {
								hot: "#999999",
								cold: "#CACACA"
							},
							columnTotal: { value: { show: !1 } },
							rowTotal: { value: { show: !1 } },
							value: { show: !1 }
						},
						dataLabels: {
							xAxis: { show: !1 },
							yAxis: { show: !1 }
						}
					}
				}
			},
			userConfig: L.value.skeletonConfig ?? {}
		})), { loading: on, FINAL_DATASET: R, manualLoading: sn } = se({
			...Ie(j),
			FINAL_CONFIG: L,
			prepareConfig: fn,
			callback: () => {
				Promise.resolve().then(async () => {
					await ke(), N.value && a(N.value, {
						delta: .1,
						delay: 250
					});
				});
			},
			skeletonDataset: j.config?.skeletonDataset ?? rn(),
			skeletonConfig: r({
				defaultConfig: L.value,
				userConfig: an.value
			})
		}), { userOptionsVisible: cn, setUserOptionsVisibility: ln, keepUserOptionState: un } = be({ config: L.value }), { svgRef: z } = xe({ config: L.value.style.title });
		function dn(e) {
			ln(e);
		}
		function fn() {
			let e = le({
				userConfig: j.config,
				defaultConfig: kt
			}), t = {}, n = e.theme;
			if (n) if (!At.value(e)) jt(e), t = e;
			else {
				let r = le({
					userConfig: Ce[n] || j.config,
					defaultConfig: e
				});
				t = { ...le({
					userConfig: j.config,
					defaultConfig: r
				}) };
			}
			else t = e;
			return t;
		}
		ze(() => j.config, (e) => {
			on.value || (L.value = fn()), cn.value = !L.value.userOptions.showOnChartHover, bn(), zt.value += 1, B.value.showTable = L.value.table.show, B.value.showTooltip = L.value.style.tooltip.show, xn.value = L.value.style.layout.width, Sn.value = L.value.style.layout.height;
		}, { deep: !0 }), ze(() => j.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (sn.value = !1), bn();
		}, { deep: !0 });
		let { isPrinting: pn, isImaging: mn, generatePdf: hn, generateImage: gn } = oe({
			elementId: `heatmap__${M.value}`,
			fileName: L.value.style.title.text || "vue-ui-heatmap",
			options: L.value.userOptions.print
		}), _n = g(() => L.value.userOptions.show && !L.value.style.title.text), B = T({
			showTable: L.value.table.show,
			showTooltip: L.value.style.tooltip.show
		});
		ze(L, () => {
			B.value = {
				showTable: L.value.table.show,
				showTooltip: L.value.style.tooltip.show
			};
		}, { immediate: !0 });
		let vn = g(() => L.value.table.responsiveBreakpoint), V = T(null), yn = g(() => L.value.debug);
		function bn() {
			if (l(j.dataset) && (m({
				componentName: "VueUiHeatmap",
				type: "dataset",
				debug: yn.value
			}), Nt.value = !1, sn.value = !0), l(j.dataset) || (sn.value = L.value.loading), L.value.responsive) {
				let e = ge(() => {
					let { width: e, height: t } = _e({
						chart: N.value,
						title: L.value.style.title.text ? Ut.value : null,
						source: Zt.value,
						noTitle: Wt.value
					});
					requestAnimationFrame(() => {
						xn.value = e, Sn.value = t;
					});
				});
				V.value && (F.value && V.value.unobserve(F.value), V.value.disconnect()), V.value = new ResizeObserver(e), F.value = N.value.parentNode, V.value.observe(F.value);
			}
		}
		je(() => {
			V.value && (F.value && V.value.unobserve(F.value), V.value.disconnect());
		}), Me(() => {
			bn();
		});
		let H = g(() => Math.max(...R.value.flatMap((e) => (e.values || []).length))), xn = T(L.value.style.layout.width), Sn = T(L.value.style.layout.height), U = g(() => ({
			width: Math.max(10, xn.value),
			height: Math.max(10, Sn.value)
		})), Cn = T(0);
		Ue({
			elementRef: Jt,
			callback: ge((e) => {
				e !== Cn.value && (Cn.value = e);
			}, 100),
			attr: "height"
		});
		let W = T(0);
		Ue({
			elementRef: Kt,
			callback: ge((e) => {
				e !== W.value && (W.value = e);
			}, 100),
			attr: "width"
		});
		let wn = T(0);
		Ue({
			elementRef: Yt,
			callback: ge((e) => {
				e !== wn.value && (wn.value = e);
			}, 100),
			attr: "height"
		}), je(() => {
			Cn.value = 0, W.value = 0, wn.value = 0;
		});
		let G = g(() => Math.min(U.value.height, U.value.width) / 1e3 * L.value.style.layout.cells.spacing), K = g(() => {
			let e = 0;
			L.value.style.legend.show && (e = L.value.style.legend.width);
			let t = L.value.style.layout.padding, n = L.value.style.layout.dataLabels.xAxis.fontSize / 3, r = L.value.style.layout.dataLabels.xAxis.fontSize / 2, i = U.value.height - t.top - t.bottom - Cn.value - n, a = U.value.width / 60, o = {
				x: G.value * H.value,
				y: G.value * ((R.value || []).length + 1)
			}, s = U.value.width - t.left - t.right - o.x - a * 2 - 2 - e - W.value, c = i - a - o.y - wn.value - r - L.value.style.layout.cells.columnTotal.value.offsetY, l = {
				width: Math.max(3, s / H.value),
				height: Math.max(3, c / (R.value.length ?? 1))
			};
			return {
				top: t.top + Cn.value + a + n,
				topLabelsHeight: Cn.value,
				sumCellXHeight: a,
				height: c,
				left: t.left + W.value + a / 2,
				right: t.right - e,
				bottom: U.value.height - t.bottom - wn.value,
				width: s,
				cellSize: l
			};
		}), q = g(() => Math.max(...R.value.flatMap((e) => e.values))), Tn = g(() => Math.min(...R.value.flatMap((e) => e.values))), J = g(() => {
			let e = R.value.flatMap((e) => e.values);
			return e.reduce((e, t) => e + t, 0) / e.length;
		}), En = T([]), Dn = T([]), On = 0;
		Be(() => {
			let e = ++On;
			(async () => {
				let t = L.value.style.layout.dataLabels.yAxis, n = await ie({
					values: t.values.length ? t.values : R.value.map((e) => e.name),
					maxDatapoints: R.value.length,
					formatter: t.datetimeFormatter,
					start: 0,
					end: R.value.length
				});
				e === On && (En.value = n);
			})();
		});
		let kn = 0;
		Be(() => {
			let e = ++kn;
			(async () => {
				let t = L.value.style.layout.dataLabels.xAxis, n = await ie({
					values: t.values,
					maxDatapoints: H.value,
					formatter: t.datetimeFormatter,
					start: 0,
					end: H.value
				});
				e === kn && (Dn.value = n);
			})();
		});
		let Y = g(() => {
			let e = En.value.map((e) => e.text), t = Dn.value.map((e) => e.text), n = R.value.map((e) => e.values.reduce((e, t) => e + t, 0)), r = Math.max(...n), i = Math.min(...n), a = [];
			for (let e = 0; e < H.value; e += 1) a.push(R.value.map((t) => t.values[e] || 0).reduce((e, t) => e + t, 0));
			let o = Math.max(...a), s = Math.min(...a);
			return {
				yTotals: n.map((e) => ({
					total: e,
					proportion: isNaN(e / r) ? 0 : e / r,
					color: te(L.value.style.layout.cells.colors.cold, L.value.style.layout.cells.colors.hot, i, r, e)
				})),
				xTotals: a.map((e) => ({
					total: e,
					proportion: isNaN(e / o) ? 0 : e / o,
					color: te(L.value.style.layout.cells.colors.cold, L.value.style.layout.cells.colors.hot, s, o, e)
				})),
				yLabels: e,
				xLabels: t.slice(0, H.value)
			};
		}), X = g(() => (R.value.forEach((e, t) => {
			u({
				datasetObject: e,
				requiredAttributes: ["values"]
			}).forEach((e) => {
				m({
					componentName: "VueUiHeatmap",
					type: "datasetSerieAttribute",
					property: "values",
					index: t,
					debug: yn.value
				});
			});
		}), R.value.map((e, t) => ({
			...e,
			temperatures: (e.values || []).map((e, n) => e >= J.value ? {
				side: "up",
				color: te(L.value.style.layout.cells.colors.cold, L.value.style.layout.cells.colors.hot, Tn.value, q.value, e),
				ratio: Math.abs(Math.abs(e - J.value) / Math.abs(q.value - J.value)) > 1 ? 1 : Math.abs(Math.abs(e - J.value) / Math.abs(q.value - J.value)),
				value: e,
				yAxisName: Y.value.yLabels[t],
				xAxisName: Y.value.xLabels[n],
				id: `vue-data-ui-heatmap-cell-${d()}`
			} : {
				side: "down",
				ratio: Math.abs(1 - Math.abs(e) / Math.abs(J.value)) > 1 ? 1 : Math.abs(1 - Math.abs(e) / Math.abs(J.value)),
				color: te(L.value.style.layout.cells.colors.cold, L.value.style.layout.cells.colors.hot, Tn.value, q.value, e),
				value: e,
				yAxisName: Y.value.yLabels[t],
				xAxisName: Y.value.xLabels[n],
				id: `vue-data-ui-heatmap-cell-${d()}`
			})
		})))), An = g(() => R.value.length), jn = Ne(Array(An.value * H.value || 1).fill(!1)), Mn = g(() => jn.some((e) => e));
		function Nn(e, t) {
			jn[e] = t;
		}
		let Pn = Ne(Array(An.value * H.value || 1).fill(!1)), Fn = g(() => Pn.some((e) => e));
		function In(e, t) {
			Pn[e] = t;
		}
		let Z = T(null), Ln = T(null);
		function Rn() {
			Pt.value = !1, It.value = void 0, Z.value = null, P.value = null, I.value = null, Qt.value = null;
		}
		function zn(e) {
			if (!z.value || !e) return;
			let t = z.value.querySelector(`[data-a11y-cell-id="${e}"]`);
			if (!t) return;
			let n = t.getBoundingClientRect();
			$t.value = {
				x: n.left + n.width / 2,
				y: n.top + n.height / 2
			};
		}
		function Bn(e, t, n, r, a = "pointer", o = null) {
			if (L.value.events.datapointEnter && L.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), !B.value.showTooltip) return;
			en.value = a, I.value = o, Qt.value = e.id, P.value = {
				x: n,
				y: r
			};
			let { value: l, yAxisName: u, xAxisName: d, id: f } = e;
			It.value = f, Z.value = l, Ln.value = {
				datapoint: e,
				seriesIndex: t,
				series: X.value,
				config: L.value
			}, Pt.value = !0;
			let p = "", m = L.value.style.tooltip.customFormat;
			ee(m) && s(() => m({
				datapoint: e,
				seriesIndex: t,
				series: X.value,
				config: L.value
			})) ? Ft.value = m({
				datapoint: e,
				seriesIndex: t,
				series: X.value,
				config: L.value
			}) : (p += `<div>${u} ${d ? u ? ` - ${d}` : `${d}` : ""}</div>`, p += `<div style="margin-top:6px;padding-top:6px;border-top:1px solid ${L.value.style.tooltip.borderColor};font-weight:bold;display:flex;flex-direction:row;gap:12px;align-items:center;justify-content:center"><span style="color:${te(L.value.style.layout.cells.colors.cold, L.value.style.layout.cells.colors.hot, Tn.value, q.value, l)}">⬤</span><span>${isNaN(l) ? "-" : c(L.value.style.layout.cells.value.formatter, l, i({
				p: L.value.style.layout.dataLabels.prefix,
				v: l,
				s: L.value.style.layout.dataLabels.suffix,
				r: L.value.style.tooltip.roundingValue
			}), {
				datapoint: e,
				seriesIndex: t
			})}</span></div>`, Ft.value = `<div style="font-size:${L.value.style.tooltip.fontSize}px">${p}</div>`), a === "keyboard" && ke(() => {
				zn(e.id);
			});
		}
		function Vn({ datapoint: e, seriesIndex: t }) {
			L.value.events.datapointLeave && L.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			}), (Qt.value !== e.id || en.value !== "keyboard") && (Pt.value = !1, It.value = void 0, Z.value = null, P.value = null);
		}
		function Hn(e) {
			return c(L.value.style.layout.cells.value.formatter, Y.value.yTotals[e].total, i({
				p: L.value.style.layout.dataLabels.prefix,
				v: Y.value.yTotals[e].total,
				s: L.value.style.layout.dataLabels.suffix,
				r: L.value.style.layout.cells.value.roundingValue
			}), {
				datapoint: Y.value.yTotals[e],
				rowIndex: e
			});
		}
		function Un(e) {
			return c(L.value.style.layout.cells.value.formatter, Y.value.xTotals[e].total, i({
				p: L.value.style.layout.dataLabels.prefix,
				v: Y.value.xTotals[e].total,
				s: L.value.style.layout.dataLabels.suffix,
				r: L.value.style.layout.cells.value.roundingValue
			}), {
				datapoint: Y.value.xTotals[e],
				colIndex: e
			});
		}
		g(() => ({
			head: R.value.map((e) => ({ name: e.name })),
			body: R.value.map((e) => e.values)
		}));
		function Wn(e = null) {
			ke(() => {
				let r = ["", ...R.value.map((e, t) => e.name)], i = [];
				for (let e = 0; e < Y.value.xLabels.length; e += 1) {
					let t = [Y.value.xLabels[e]];
					for (let n = 0; n < R.value.length; n += 1) t.push([R.value[n].values[e]]);
					i.push(t);
				}
				let a = [
					[L.value.style.title.text],
					[L.value.style.title.subtitle.text],
					[
						[""],
						[""],
						[""]
					],
					r
				].concat(i), o = n(a);
				e ? e(o) : t({
					csvContent: o,
					title: L.value.style.title.text || "vue-ui-heatmap"
				});
			});
		}
		let Q = T(!1);
		function Gn(e) {
			Q.value = e, Lt.value += 1;
		}
		function Kn() {
			B.value.showTable = !B.value.showTable;
		}
		function qn() {
			B.value.showTooltip = !B.value.showTooltip;
		}
		let Jn = T(!1);
		function Yn() {
			Jn.value = !Jn.value;
		}
		function Xn(e, t) {
			L.value.events.datapointClick && L.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			}), Mt("selectDatapoint", e);
		}
		function Zn() {
			return X.value;
		}
		async function Qn({ scale: e = 2 } = {}) {
			if (!N.value) return;
			let { width: t, height: n } = N.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await pe({
				domElement: N.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: L.value.style.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let $n = g(() => ({
			start: 0,
			end: H.value
		})), er = g(() => Y.value.xLabels);
		fe({
			timeLabelsEls: Jt,
			timeLabels: er,
			slicer: $n,
			configRef: L,
			rotationPath: [
				"style",
				"layout",
				"dataLabels",
				"xAxis",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"layout",
				"dataLabels",
				"xAxis",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			targetClass: ".vue-ui-heatmap-col-name",
			rotation: L.value.style.layout.dataLabels.xAxis.autoRotate.angle,
			width: xn,
			height: Sn
		}), fe({
			timeLabelsEls: Yt,
			timeLabels: er,
			slicer: $n,
			configRef: L,
			rotationPath: [
				"style",
				"layout",
				"cells",
				"columnTotal",
				"value",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"layout",
				"cells",
				"columnTotal",
				"value",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			targetClass: ".vue-ui-heatmap-col-total",
			rotation: L.value.style.layout.cells.columnTotal.value.autoRotate.angle,
			width: xn,
			height: Sn
		});
		let tr = g(() => {
			let e = L.value.table.useDialog && !L.value.table.show, t = B.value.showTable;
			return {
				component: e ? Ot : ve,
				title: `${L.value.style.title.text}${L.value.style.title.subtitle.text ? `: ${L.value.style.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: L.value.table.th.backgroundColor,
					color: L.value.table.th.color,
					headerColor: L.value.table.th.color,
					headerBg: L.value.table.th.backgroundColor,
					isFullscreen: Q.value,
					fullscreenParent: N.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: nn.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: L.value.style.backgroundColor,
							color: L.value.style.color
						},
						head: {
							backgroundColor: L.value.style.backgroundColor,
							color: L.value.style.color
						}
					}
				}
			};
		});
		ze(() => B.value.showTable, async (e) => {
			L.value.table.show || (e && L.value.table.useDialog && Vt.value ? (await ke(), Vt.value.open()) : "close" in Vt.value && Vt.value.close());
		});
		let { isResponsive: nr } = Se(Rt, vn);
		function rr() {
			B.value.showTable = !1, Ht.value && Ht.value.setTableIconState(!1);
		}
		let ir = g(() => L.value.style.backgroundColor), ar = g(() => L.value.style.title), { isCallbackImaging: or, isCallbackSvg: sr, generateSvg: cr, onGenerateImage: lr } = de({
			svg: z,
			title: ar,
			legend: null,
			legendItems: null,
			backgroundColor: ir,
			getSvgCallback: () => L.value.userOptions.callbacks.svg,
			generateImage: gn
		});
		async function ur() {
			if (Mt("copyAlt", {
				config: L.value,
				dataset: X.value
			}), !L.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(L.value.userOptions.callbacks.altCopy({
				config: L.value,
				dataset: X.value
			}));
		}
		function dr(e, t) {
			let n = X.value[e];
			if (!n) return null;
			let r = n.temperatures[t];
			return r ? {
				cell: r,
				rowIndex: e,
				columnIndex: t
			} : null;
		}
		function fr(e, t) {
			let n = 0;
			for (let t = 0; t < e; t += 1) n += X.value[t]?.temperatures?.length || 0;
			return n + t;
		}
		function pr() {
			I.value = null, Qt.value = null, tn.value = !0;
		}
		function mr() {
			Rn(), tn.value = !1;
		}
		function hr(e) {
			if (!z.value || Jn.value || document.activeElement !== z.value || !$.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "ArrowUp", i = e.key === "ArrowDown", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				Rn();
				return;
			}
			if (a) {
				if (I.value === null) return;
				let e = $.value[I.value];
				if (!e) return;
				Xn(e.cell, e.rowIndex);
				return;
			}
			let s = I.value === null ? null : $.value[I.value];
			if (!s) {
				let e = It.value ? $.value.findIndex((e) => e.cell.id === It.value) : null;
				if (e !== null && e >= 0 && e < $.value.length) {
					let a = $.value[e];
					if (!a) return;
					let o = a.rowIndex, s = a.columnIndex;
					n ? s += 1 : t ? --s : i ? o += 1 : r && --o;
					let c = _r.value.rowCount;
					if (c <= 0) return;
					o < 0 && (o = c - 1), o >= c && (o = 0);
					let l = X.value[o];
					if (!l || !l.temperatures.length) return;
					let u = l.temperatures.length;
					s < 0 && (s = u - 1), s >= u && (s = 0);
					let d = dr(o, s);
					if (!d) return;
					let f = fr(o, s), p = K.value.left + K.value.cellSize.width * d.columnIndex, m = K.value.top + K.value.cellSize.height * d.rowIndex;
					Bn(d.cell, d.rowIndex, p, m, "keyboard", f);
					return;
				}
				let a = $.value[0];
				if (!a) return;
				let o = K.value.left + K.value.cellSize.width * a.columnIndex, s = K.value.top + K.value.cellSize.height * a.rowIndex;
				Bn(a.cell, a.rowIndex, o, s, "keyboard", 0);
				return;
			}
			let c = s.rowIndex, l = s.columnIndex;
			n ? l += 1 : t ? --l : i ? c += 1 : r && --c;
			let u = _r.value.rowCount;
			if (u <= 0) return;
			c < 0 && (c = u - 1), c >= u && (c = 0);
			let d = X.value[c];
			if (!d || !d.temperatures.length) return;
			let f = d.temperatures.length;
			l < 0 && (l = f - 1), l >= f && (l = 0);
			let p = dr(c, l);
			if (!p) return;
			let m = fr(c, l), ee = K.value.left + K.value.cellSize.width * p.columnIndex, te = K.value.top + K.value.cellSize.height * p.rowIndex;
			Bn(p.cell, p.rowIndex, ee, te, "keyboard", m);
		}
		let gr = g(() => ({
			headers: [L.value.table.colNames.xAxis, ...R.value.map((e) => e.name)],
			rows: Y.value.xLabels.map((e, t) => [e, ...R.value.map((e) => {
				let n = e.values?.[t];
				return isNaN(n) ? "-" : i({
					p: L.value.style.layout.dataLabels.prefix,
					v: n,
					s: L.value.style.layout.dataLabels.suffix,
					r: L.value.table.td.roundingValue
				});
			})])
		})), _r = g(() => ({
			rowCount: X.value.length,
			columnCount: Math.max(0, ...X.value.map((e) => e.temperatures.length))
		})), $ = g(() => X.value.flatMap((e, t) => e.temperatures.map((e, n) => ({
			cell: e,
			rowIndex: t,
			columnIndex: n
		}))));
		return he({
			getData: Zn,
			getImage: Qn,
			generatePdf: hn,
			generateCsv: Wn,
			generateImage: gn,
			generateSvg: cr,
			toggleTable: Kn,
			toggleTooltip: qn,
			toggleAnnotator: Yn,
			toggleFullscreen: Gn,
			copyAlt: ur
		}), (t, n) => (w(), v("div", {
			ref_key: "heatmapChart",
			ref: N,
			class: Ae(`vue-data-ui-component vue-ui-heatmap ${Q.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			style: C(`font-family:${L.value.style.fontFamily};width:100%;${L.value.responsive ? "height: 100%;" : ""} text-align:center;background:${L.value.style.backgroundColor}`),
			id: `heatmap__${M.value}`,
			onMouseenter: n[1] ||= () => dn(!0),
			onMouseleave: n[2] ||= () => {
				dn(!1), tn.value || Rn();
			}
		}, [
			y("div", {
				id: `chart-instructions-${M.value}`,
				class: "sr-only"
			}, [y("p", null, O(L.value.a11y.translations.keyboardNavigation), 1)], 8, Je),
			gr.value?.rows?.length ? (w(), we(ye, {
				key: 0,
				uid: M.value,
				head: gr.value.headers,
				body: gr.value.rows,
				notice: L.value.a11y.translations.tableAvailable,
				caption: L.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : _("", !0),
			L.value.userOptions.buttons.annotator ? (w(), we(k(Tt), {
				key: 1,
				svgRef: k(z),
				backgroundColor: L.value.style.backgroundColor,
				color: L.value.style.color,
				active: Jn.value,
				isCursorPointer: nn.value,
				onClose: Yn
			}, {
				"annotator-action-close": A(() => [D(t.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": A(({ color: e }) => [D(t.$slots, "annotator-action-color", S(x({ color: e })), void 0, !0)]),
				"annotator-action-draw": A(({ mode: e }) => [D(t.$slots, "annotator-action-draw", S(x({ mode: e })), void 0, !0)]),
				"annotator-action-undo": A(({ disabled: e }) => [D(t.$slots, "annotator-action-undo", S(x({ disabled: e })), void 0, !0)]),
				"annotator-action-redo": A(({ disabled: e }) => [D(t.$slots, "annotator-action-redo", S(x({ disabled: e })), void 0, !0)]),
				"annotator-action-delete": A(({ disabled: e }) => [D(t.$slots, "annotator-action-delete", S(x({ disabled: e })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : _("", !0),
			_n.value ? (w(), v("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Wt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : _("", !0),
			L.value.style.title.text ? (w(), v("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Ut,
				style: "width:100%;background:transparent"
			}, [(w(), we(me, {
				key: `title_${zt.value}`,
				config: {
					title: {
						cy: "heatmap-div-title",
						...L.value.style.title
					},
					subtitle: {
						cy: "heatmap-div-subtitle",
						...L.value.style.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : _("", !0),
			L.value.userOptions.show && Nt.value && (k(un) || k(cn)) ? (w(), we(k(Et), {
				ref_key: "userOptionsRef",
				ref: Ht,
				key: `user_options_${Lt.value}`,
				backgroundColor: L.value.style.backgroundColor,
				color: L.value.style.color,
				isImaging: k(mn),
				isPrinting: k(pn),
				uid: M.value,
				hasTooltip: L.value.userOptions.buttons.tooltip && L.value.style.tooltip.show,
				hasPdf: L.value.userOptions.buttons.pdf,
				hasImg: L.value.userOptions.buttons.img,
				hasSvg: L.value.userOptions.buttons.svg,
				hasXls: L.value.userOptions.buttons.csv,
				hasTable: L.value.userOptions.buttons.table,
				hasFullscreen: L.value.userOptions.buttons.fullscreen,
				hasAltCopy: L.value.userOptions.buttons.altCopy,
				isFullscreen: Q.value,
				isTooltip: B.value.showTooltip,
				titles: { ...L.value.userOptions.buttonTitles },
				chartElement: N.value,
				position: L.value.userOptions.position,
				hasAnnotator: L.value.userOptions.buttons.annotator,
				isAnnotation: Jn.value,
				callbacks: L.value.userOptions.callbacks,
				printScale: L.value.userOptions.print.scale,
				tableDialog: L.value.table.useDialog,
				isCursorPointer: nn.value,
				onToggleFullscreen: Gn,
				onGeneratePdf: k(hn),
				onGenerateCsv: Wn,
				onGenerateImage: k(lr),
				onGenerateSvg: k(cr),
				onToggleTable: Kn,
				onToggleTooltip: qn,
				onToggleAnnotator: Yn,
				onCopyAlt: ur,
				style: C({ visibility: k(un) ? k(cn) ? "visible" : "hidden" : "visible" })
			}, Te({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: A(({ isOpen: e, color: n }) => [D(t.$slots, "menuIcon", S(x({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: A(() => [D(t.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: A(() => [D(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: A(() => [D(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: A(() => [D(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionSvg ? {
					name: "optionSvg",
					fn: A(() => [D(t.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionTable ? {
					name: "optionTable",
					fn: A(() => [D(t.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: A(({ toggleFullscreen: e, isFullscreen: n }) => [D(t.$slots, "optionFullscreen", S(x({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: A(({ toggleAnnotator: e, isAnnotator: n }) => [D(t.$slots, "optionAnnotator", S(x({
						toggleAnnotator: e,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: A(({ altCopy: e }) => [D(t.$slots, "optionAltCopy", S(x({ altCopy: e })), void 0, !0)]),
					key: "9"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: A(() => [D(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: A(() => [D(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isImaging.isPrinting.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : _("", !0),
			y("div", { class: Ae({
				"vue-ui-heatmap-chart-wrapper": !0,
				"vue-ui-heatmap-chart-wrapper-legend-right": L.value.style.legend.show
			}) }, [
				(w(), v("svg", {
					ref_key: "svgRef",
					ref: z,
					xmlns: k(p),
					"aria-describedby": `chart-instructions-${M.value}`,
					class: Ae({
						"vue-data-ui-fullscreen--on": Q.value,
						"vue-data-ui-fulscreen--off": !Q.value
					}),
					viewBox: `0 0 ${U.value.width} ${U.value.height}`,
					width: "100%",
					style: C(`overflow: visible; background:transparent;color:${L.value.style.color}`),
					"aria-live": "polite",
					role: "img",
					preserveAspectRatio: "xMidYMid",
					tabindex: "0",
					onFocus: pr,
					onBlur: mr,
					onKeydown: hr
				}, [
					De(k(Dt)),
					t.$slots["chart-background"] ? (w(), v("foreignObject", {
						key: 0,
						x: K.value.left,
						y: K.value.top,
						width: K.value.width,
						height: K.value.height,
						style: { pointerEvents: "none" }
					}, [D(t.$slots, "chart-background", {}, void 0, !0)], 8, Xe)) : _("", !0),
					L.value.style.layout.cells.columnTotal.color.show ? (w(), v("g", {
						key: 1,
						ref_key: "xAxisSumRects",
						ref: Xt
					}, [(w(!0), v(h, null, E(Y.value.xTotals, (e, t) => (w(), v("rect", {
						x: K.value.left + K.value.cellSize.width * t + G.value / 2 + K.value.sumCellXHeight,
						y: K.value.top - K.value.sumCellXHeight + G.value * (U.value.height / U.value.width),
						height: K.value.sumCellXHeight,
						width: K.value.cellSize.width - G.value,
						fill: L.value.style.layout.cells.colors.underlayer,
						stroke: L.value.style.backgroundColor,
						"stroke-width": G.value
					}, null, 8, Ze))), 256)), (w(!0), v(h, null, E(Y.value.xTotals, (e, t) => (w(), v("rect", {
						x: K.value.left + K.value.cellSize.width * t + G.value / 2 + K.value.sumCellXHeight,
						y: K.value.top - K.value.sumCellXHeight + G.value * (U.value.height / U.value.width),
						height: K.value.sumCellXHeight,
						width: K.value.cellSize.width - G.value,
						fill: e.color,
						stroke: L.value.style.backgroundColor,
						"stroke-width": G.value
					}, null, 8, Qe))), 256))], 512)) : _("", !0),
					y("g", {
						ref_key: "datapoints",
						ref: Bt
					}, [(w(!0), v(h, null, E(X.value, (e, t) => (w(), v(h, null, [(w(!0), v(h, null, E(e.temperatures, (e, n) => (w(), v("g", null, [
						y("rect", {
							x: K.value.left + K.value.cellSize.width * n + G.value / 2 + K.value.sumCellXHeight,
							y: K.value.top + K.value.cellSize.height * t + G.value / 2,
							width: K.value.cellSize.width - G.value,
							height: K.value.cellSize.height - G.value,
							fill: L.value.style.layout.cells.colors.underlayer,
							stroke: L.value.style.backgroundColor,
							"stroke-width": G.value
						}, null, 8, $e),
						y("rect", {
							"data-a11y-cell-id": e.id,
							x: K.value.left + K.value.cellSize.width * n + G.value / 2 + K.value.sumCellXHeight,
							y: K.value.top + K.value.cellSize.height * t + G.value / 2,
							width: K.value.cellSize.width - G.value,
							height: K.value.cellSize.height - G.value,
							fill: e.color,
							stroke: L.value.style.backgroundColor,
							"stroke-width": G.value,
							"aria-label": `${e.yAxisName}${e.xAxisName ? ` - ${e.xAxisName}` : ""}: ${isNaN(e.value) ? "-" : e.value}`,
							onMouseover: (r) => Bn(e, t, K.value.left + K.value.cellSize.width * n, K.value.top + K.value.cellSize.height * t, "pointer", fr(t, n)),
							onMouseout: () => Vn({
								datapoint: e,
								seriesIndex: t
							}),
							onClick: () => Xn(e, t)
						}, null, 40, et),
						L.value.style.layout.cells.value.show ? Ve((w(), v("text", {
							key: 0,
							"text-anchor": "middle",
							"font-size": L.value.style.layout.cells.value.fontSize,
							"font-weight": L.value.style.layout.cells.value.bold ? "bold" : "normal",
							fill: k(f)(e.color),
							x: K.value.left + K.value.cellSize.width * n + K.value.cellSize.width / 2 + K.value.sumCellXHeight,
							y: K.value.top + K.value.cellSize.height * t + K.value.cellSize.height / 2 + L.value.style.layout.cells.value.fontSize / 3,
							style: {
								pointerEvents: "none",
								userSelect: "none"
							}
						}, [Ee(O(k(c)(L.value.style.layout.cells.value.formatter, e.value, k(i)({
							p: L.value.style.layout.dataLabels.prefix,
							v: e.value,
							s: L.value.style.layout.dataLabels.suffix,
							r: L.value.style.layout.cells.value.roundingValue
						}), { datapoint: e })), 1)], 8, tt)), [[k(We), {
							cellWidth: K.value.cellSize.width - G.value,
							cellHeight: K.value.cellSize.height - G.value,
							maxFontSize: L.value.style.layout.cells.value.fontSize,
							minFontSize: 10,
							index: t * H.value + n,
							reportHide: In,
							reportRotation: Nn,
							hideAll: Fn.value,
							rotateAll: Mn.value
						}]]) : _("", !0)
					]))), 256))], 64))), 256))], 512),
					L.value.style.layout.cells.rowTotal.color.show ? (w(), v("g", {
						key: 2,
						ref_key: "yAxisSumRects",
						ref: qt
					}, [(w(!0), v(h, null, E(X.value, (e, t) => (w(), v(h, null, [y("rect", {
						x: K.value.left,
						y: K.value.top + K.value.cellSize.height * t,
						width: K.value.sumCellXHeight,
						height: K.value.cellSize.height - G.value,
						fill: L.value.style.layout.cells.colors.underlayer,
						stroke: L.value.style.backgroundColor,
						"stroke-width": G.value
					}, null, 8, nt), y("rect", {
						x: K.value.left,
						y: K.value.top + K.value.cellSize.height * t + G.value / 2,
						width: K.value.sumCellXHeight,
						height: K.value.cellSize.height - G.value,
						fill: Y.value.yTotals[t].color,
						stroke: L.value.style.backgroundColor,
						"stroke-width": G.value
					}, null, 8, rt)], 64))), 256))], 512)) : _("", !0),
					L.value.style.layout.dataLabels.yAxis.show ? (w(), v("g", {
						key: 3,
						ref_key: "yAxisLabels",
						ref: Kt
					}, [(w(!0), v(h, null, E(X.value, (e, t) => (w(), v(h, null, [y("text", {
						class: "vue-ui-heatmap-row-name",
						"font-size": L.value.style.layout.dataLabels.yAxis.fontSize,
						fill: L.value.style.layout.dataLabels.yAxis.color,
						x: W.value,
						y: K.value.top + K.value.cellSize.height * t + K.value.cellSize.height / 2 + L.value.style.layout.dataLabels.yAxis.fontSize / 3 + L.value.style.layout.dataLabels.yAxis.offsetY - (L.value.style.layout.cells.rowTotal.value.show ? L.value.style.layout.dataLabels.yAxis.fontSize / 1.5 : 0),
						"text-anchor": "end",
						"font-weight": L.value.style.layout.dataLabels.yAxis.bold ? "bold" : "normal"
					}, O(Y.value.yLabels[t]), 9, it), L.value.style.layout.cells.rowTotal.value.show ? (w(), v("text", {
						key: 0,
						class: "vue-ui-heatmap-row-total",
						"font-size": L.value.style.layout.dataLabels.yAxis.fontSize,
						fill: L.value.style.layout.dataLabels.yAxis.color,
						x: W.value,
						y: K.value.top + K.value.cellSize.height * t + K.value.cellSize.height / 2 + L.value.style.layout.dataLabels.yAxis.fontSize + L.value.style.layout.dataLabels.yAxis.offsetY,
						"text-anchor": "end",
						"font-weight": L.value.style.layout.dataLabels.yAxis.bold ? "bold" : "normal"
					}, O(Hn(t)), 9, at)) : _("", !0)], 64))), 256))], 512)) : _("", !0),
					L.value.style.layout.dataLabels.xAxis.show ? (w(), v("g", {
						key: 4,
						ref_key: "xAxisLabels",
						ref: Jt
					}, [(w(!0), v(h, null, E(Y.value.xLabels, (e, t) => (w(), v(h, null, [!L.value.style.layout.dataLabels.xAxis.showOnlyAtModulo || L.value.style.layout.dataLabels.xAxis.showOnlyAtModulo && t % L.value.style.layout.dataLabels.xAxis.showOnlyAtModulo === 0 ? (w(), v("text", {
						key: 0,
						class: "vue-ui-heatmap-col-name",
						"text-anchor": L.value.style.layout.dataLabels.xAxis.rotation === 0 ? "middle" : L.value.style.layout.dataLabels.xAxis.rotation < 0 ? "start" : "end",
						"font-size": L.value.style.layout.dataLabels.xAxis.fontSize,
						fill: L.value.style.layout.dataLabels.xAxis.color,
						"font-weight": L.value.style.layout.dataLabels.xAxis.bold ? "bold" : "normal",
						transform: `translate(${K.value.left + K.value.cellSize.width / 2 + K.value.width / Y.value.xLabels.length * t + L.value.style.layout.dataLabels.xAxis.offsetX + K.value.sumCellXHeight}, ${K.value.topLabelsHeight}), rotate(${L.value.style.layout.dataLabels.xAxis.rotation})`
					}, O(e), 9, ot)) : _("", !0)], 64))), 256))], 512)) : _("", !0),
					L.value.style.layout.cells.columnTotal.value.show ? (w(), v("g", {
						key: 5,
						ref_key: "xAxisSums",
						ref: Yt
					}, [(w(!0), v(h, null, E(Y.value.xLabels, (e, t) => (w(), v("text", {
						class: "vue-ui-heatmap-col-total",
						"text-anchor": L.value.style.layout.cells.columnTotal.value.rotation === 0 ? "middle" : L.value.style.layout.cells.columnTotal.value.rotation < 0 ? "end" : "start",
						"font-size": L.value.style.layout.dataLabels.xAxis.fontSize,
						fill: L.value.style.layout.dataLabels.xAxis.color,
						"font-weight": L.value.style.layout.dataLabels.xAxis.bold ? "bold" : "normal",
						transform: `translate(${K.value.left + K.value.cellSize.width / 2 + K.value.width / Y.value.xLabels.length * t + L.value.style.layout.dataLabels.xAxis.offsetX + L.value.style.layout.cells.columnTotal.value.offsetX + K.value.sumCellXHeight}, ${K.value.bottom + L.value.style.layout.dataLabels.xAxis.fontSize / 2}), rotate(${L.value.style.layout.cells.columnTotal.value.rotation})`
					}, O(Un(t)), 9, st))), 256))], 512)) : _("", !0),
					P.value ? (w(), v("g", ct, [y("rect", {
						style: { "pointer-events": "none" },
						x: P.value.x - L.value.style.layout.cells.selected.border / 2 + G.value + K.value.sumCellXHeight,
						y: P.value.y - L.value.style.layout.cells.selected.border / 2 + G.value,
						width: K.value.cellSize.width - G.value + L.value.style.layout.cells.selected.border - G.value,
						height: K.value.cellSize.height - G.value + L.value.style.layout.cells.selected.border - G.value,
						fill: "transparent",
						stroke: L.value.style.layout.cells.selected.color,
						"stroke-width": L.value.style.layout.cells.selected.border,
						rx: 1
					}, null, 8, lt)])) : _("", !0),
					L.value.style.layout.crosshairs.show && P.value ? (w(), v("g", ut, [y("line", {
						x1: K.value.left + K.value.sumCellXHeight,
						x2: P.value.x + K.value.sumCellXHeight,
						y1: P.value.y + (K.value.cellSize.height - G.value) / 2,
						y2: P.value.y + (K.value.cellSize.height - G.value) / 2,
						stroke: L.value.style.layout.crosshairs.stroke,
						"stroke-width": L.value.style.layout.crosshairs.strokeWidth,
						"stroke-dasharray": L.value.style.layout.crosshairs.strokeDasharray,
						"stroke-linecap": "round"
					}, null, 8, dt), y("line", {
						x1: P.value.x + K.value.sumCellXHeight + (K.value.cellSize.width - G.value) / 2,
						x2: P.value.x + K.value.sumCellXHeight + (K.value.cellSize.width - G.value) / 2,
						y1: P.value.y,
						y2: K.value.top,
						stroke: L.value.style.layout.crosshairs.stroke,
						"stroke-width": L.value.style.layout.crosshairs.strokeWidth,
						"stroke-dasharray": L.value.style.layout.crosshairs.strokeDasharray,
						"stroke-linecap": "round"
					}, null, 8, ft)])) : _("", !0),
					D(t.$slots, "svg", { svg: {
						...U.value,
						drawingArea: K.value,
						isPrintingImg: k(pn) || k(mn) || k(or),
						isPrintingSvg: k(sr)
					} }, void 0, !0)
				], 46, Ye)),
				L.value.style.legend.show ? (w(), v("div", {
					key: 0,
					ref_key: "legendRight",
					ref: Gt,
					class: "vue-ui-heatmap-legend-right",
					style: C({ "--legend-width": L.value.style.legend.width + "px" })
				}, [
					k(on) ? _("", !0) : (w(), v("div", {
						key: 0,
						class: "vue-ui-heatmap-legend-label-max",
						style: C({
							fontSize: L.value.style.legend.fontSize + "px",
							color: L.value.style.legend.color
						})
					}, O(k(c)(L.value.style.layout.cells.value.formatter, k(o)(q.value), k(i)({
						p: L.value.style.layout.dataLabels.prefix,
						v: k(o)(q.value),
						s: L.value.style.layout.dataLabels.suffix,
						r: L.value.style.legend.roundingValue
					}))), 5)),
					y("div", pt, [y("div", {
						class: "vue-ui-heatmap-gauge",
						style: C({ background: `linear-gradient(to bottom, ${L.value.style.layout.cells.colors.hot}, ${L.value.style.layout.cells.colors.cold})` })
					}, [Ve(y("div", {
						class: "vue-ui-heatmap-gauge-indicator",
						"data-value": k(c)(L.value.style.layout.cells.value.formatter, k(o)(Z.value), k(i)({
							p: L.value.style.layout.dataLabels.prefix,
							v: k(o)(Z.value),
							s: L.value.style.layout.dataLabels.suffix,
							r: L.value.style.legend.roundingValue
						})),
						style: C({
							position: "absolute",
							width: "100%",
							height: "2px",
							background: [void 0, null].includes(Z.value) ? "transparent" : k(f)(Ln.value.datapoint.color),
							top: `${[void 0, null].includes(Z.value) ? 0 : (1 - Z.value / q.value) * 100}%`,
							transition: "all 0.2s ease-in-out",
							"--background-color": L.value.style.backgroundColor,
							"--gauge-arrow-color": k(f)(L.value.style.backgroundColor),
							"--gauge-arrow-text-color": k(f)(L.value.style.backgroundColor),
							"--gauge-arrow-value": Z.value,
							"--gauge-arrow-font-size": L.value.style.legend.fontSize + "px"
						})
					}, [y("div", ht, O(k(c)(L.value.style.layout.cells.value.formatter, k(o)(Z.value), k(i)({
						p: L.value.style.layout.dataLabels.prefix,
						v: k(o)(Z.value),
						s: L.value.style.layout.dataLabels.suffix,
						r: L.value.style.legend.roundingValue
					}))), 1)], 12, mt), [[Re, ![void 0, null].includes(Z.value)]])], 4)]),
					k(on) ? _("", !0) : (w(), v("div", {
						key: 1,
						class: "vue-ui-heatmap-legend-label-min",
						style: C({
							fontSize: L.value.style.legend.fontSize + "px",
							color: L.value.style.legend.color
						})
					}, O(k(c)(L.value.style.layout.cells.value.formatter, k(o)(Tn.value), k(i)({
						p: L.value.style.layout.dataLabels.prefix,
						v: k(o)(Tn.value),
						s: L.value.style.layout.dataLabels.suffix,
						r: L.value.style.legend.roundingValue
					}))), 5))
				], 4)) : _("", !0),
				t.$slots.hint ? (w(), v("div", gt, [D(t.$slots, "hint", S(x({
					hint: L.value.a11y.translations.keyboardNavigation,
					isVisible: tn.value
				})), void 0, !0)])) : _("", !0)
			], 2),
			t.$slots.watermark ? (w(), v("div", _t, [D(t.$slots, "watermark", S(x({ isPrinting: k(pn) || k(mn) || k(or) || k(sr) })), void 0, !0)])) : _("", !0),
			t.$slots.source ? (w(), v("div", {
				key: 6,
				ref_key: "source",
				ref: Zt,
				dir: "auto"
			}, [D(t.$slots, "source", {}, void 0, !0)], 512)) : _("", !0),
			De(k(Ke), {
				teleportTo: L.value.style.tooltip.teleportTo,
				show: B.value.showTooltip && Pt.value,
				backgroundColor: L.value.style.tooltip.backgroundColor,
				color: L.value.style.tooltip.color,
				borderRadius: L.value.style.tooltip.borderRadius,
				borderColor: L.value.style.tooltip.borderColor,
				borderWidth: L.value.style.tooltip.borderWidth,
				fontSize: L.value.style.tooltip.fontSize,
				backgroundOpacity: L.value.style.tooltip.backgroundOpacity,
				position: L.value.style.tooltip.position,
				offsetX: L.value.style.tooltip.offsetX,
				offsetY: L.value.style.tooltip.offsetY,
				parent: N.value,
				content: Ft.value,
				isFullscreen: Q.value,
				isCustom: L.value.style.tooltip.customFormat && typeof L.value.style.tooltip.customFormat == "function",
				smooth: L.value.style.tooltip.smooth,
				backdropFilter: L.value.style.tooltip.backdropFilter,
				smoothForce: L.value.style.tooltip.smoothForce,
				smoothSnapThreshold: L.value.style.tooltip.smoothSnapThreshold,
				isA11yMode: en.value === "keyboard",
				a11yPosition: $t.value
			}, {
				"tooltip-before": A(() => [D(t.$slots, "tooltip-before", S(x({ ...Ln.value })), void 0, !0)]),
				tooltip: A(() => [D(t.$slots, "tooltip", S(x({ ...Ln.value })), void 0, !0)]),
				"tooltip-after": A(() => [D(t.$slots, "tooltip-after", S(x({ ...Ln.value })), void 0, !0)]),
				_: 3
			}, 8, [
				"teleportTo",
				"show",
				"backgroundColor",
				"color",
				"borderRadius",
				"borderColor",
				"borderWidth",
				"fontSize",
				"backgroundOpacity",
				"position",
				"offsetX",
				"offsetY",
				"parent",
				"content",
				"isFullscreen",
				"isCustom",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			Nt.value && L.value.userOptions.buttons.table ? (w(), we(Pe(tr.value.component), Oe({ key: 7 }, tr.value.props, {
				ref_key: "tableUnit",
				ref: Vt,
				onClose: rr
			}), Te({
				content: A(() => [y("div", {
					ref_key: "tableContainer",
					ref: Rt,
					class: "vue-ui-heatmap-table atom-data-table",
					style: C(`${L.value.table.useDialog ? "" : "max-height: 300px; margin-top: 24px;"}`)
				}, [y("div", {
					style: C(`width:100%;overflow-x:auto;position:relative;${L.value.table.useDialog ? "" : "padding-top:36px"};`),
					class: Ae({ "vue-ui-responsive": k(nr) })
				}, [L.value.table.useDialog ? _("", !0) : (w(), v("div", {
					key: 0,
					role: "button",
					tabindex: "0",
					style: C(`width:32px; position: absolute; top: 0; left:4px; padding: 0 0px; display: flex; align-items:center;justify-content:center;height: 36px; width: 32px; cursor:pointer; background:${L.value.table.th.backgroundColor};`),
					onClick: rr,
					onKeypress: He(rr, ["enter"])
				}, [De(k(wt), {
					name: "close",
					stroke: L.value.table.th.color,
					"stroke-width": 2
				}, null, 8, ["stroke"])], 36)), y("table", vt, [
					L.value.table.useDialog ? _("", !0) : (w(), v("caption", {
						key: 0,
						style: C(`backgroundColor:${L.value.table.th.backgroundColor};color:${L.value.table.th.color};outline:${L.value.table.th.outline}`)
					}, [Ee(O(L.value.style.title.text) + " ", 1), L.value.style.title.subtitle.text ? (w(), v("span", yt, O(L.value.style.title.subtitle.text), 1)) : _("", !0)], 4)),
					y("thead", null, [y("tr", {
						role: "row",
						style: C(`background:${L.value.table.th.backgroundColor};color:${L.value.table.th.color}`)
					}, [y("th", { style: C(`outline:${L.value.table.th.outline};padding-right:6px`) }, null, 4), (w(!0), v(h, null, E(e.dataset, (e, t) => (w(), v("th", {
						align: "right",
						style: C(`outline:${L.value.table.th.outline};padding-right:6px`)
					}, O(e.name), 5))), 256))], 4)]),
					y("tbody", null, [(w(!0), v(h, null, E(Y.value.xLabels, (t, n) => (w(), v("tr", {
						role: "row",
						class: Ae({
							"vue-ui-data-table__tbody__row": !0,
							"vue-ui-data-table__tbody__row-even": n % 2 == 0,
							"vue-ui-data-table__tbody__row-odd": n % 2 != 0
						}),
						style: C(`background:${L.value.table.td.backgroundColor};color:${L.value.table.td.color}`)
					}, [y("td", {
						"data-cell": L.value.table.colNames.xAxis,
						class: "vue-ui-data-table__tbody__td",
						style: C(`outline:${L.value.table.td.outline}`)
					}, [y("div", xt, O(t), 1)], 12, bt), (w(!0), v(h, null, E(e.dataset, (t, r) => (w(), v("td", {
						class: "vue-ui-data-table__tbody__td",
						"data-cell": e.dataset[r].name,
						style: C(`outline:${L.value.table.td.outline}`)
					}, [y("div", Ct, O(isNaN(t.values[n]) ? "-" : k(i)({
						p: L.value.style.layout.dataLabels.prefix,
						v: t.values[n],
						s: L.value.style.layout.dataLabels.suffix,
						r: L.value.table.td.roundingValue
					})), 1)], 12, St))), 256))], 6))), 256))])
				])], 6)], 4)]),
				_: 2
			}, [L.value.table.useDialog ? {
				name: "title",
				fn: A(() => [Ee(O(tr.value.title), 1)]),
				key: "0"
			} : void 0, L.value.table.useDialog ? {
				name: "actions",
				fn: A(() => [y("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: n[0] ||= (e) => Wn(L.value.userOptions.callbacks.csv),
					style: C({ cursor: nn.value ? "pointer" : "default" })
				}, [De(k(wt), {
					name: "fileCsv",
					stroke: tr.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : _("", !0),
			D(t.$slots, "skeleton", {}, () => [k(on) ? (w(), we(ce, { key: 0 })) : _("", !0)], !0)
		], 46, qe));
	}
}, [["__scopeId", "data-v-9e3f53ee"]]);
//#endregion
export { Ke as n, wt as t };
