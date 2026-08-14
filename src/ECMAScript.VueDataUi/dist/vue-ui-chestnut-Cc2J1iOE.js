import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Jt as i, Kt as a, Pt as o, S as ee, Vt as te, X as s, f as c, h as ne, i as l, jt as re, kt as ie, p as ae, pt as oe, q as se, r as ce, t as le, tt as ue, w as de } from "./lib-Bttd6u5E.js";
import { n as fe, t as pe } from "./useHints-Dq_w2E8B.js";
import { t as me } from "./useConfig-DlNpz6P8.js";
import { t as he } from "./usePrinter-DN5bYhTG.js";
import { n as ge, t as _e } from "./BaseScanner-DZvpgOjM.js";
import { t as ve } from "./useNestedProp-vPNvh7rV.js";
import { t as ye } from "./useThemeCheck-C43Tcqmk.js";
import { t as be } from "./useChartExport-DNiwdPmb.js";
import { t as xe } from "./img-Bnokohej.js";
import { t as Se } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as Ce } from "./DefGrad-DVBqDjhO.js";
import { t as we } from "./A11yDataTable-DdRsVULz.js";
import { t as Te } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ee } from "./useChartAccessibility-DYqac8yF.js";
import { t as De } from "./useTableResponsive-BAqJPR68.js";
import { t as Oe } from "./vue_ui_chestnut-D2oUhad6.js";
import { Fragment as u, computed as d, createBlock as ke, createCommentVNode as f, createElementBlock as p, createElementVNode as m, createSlots as Ae, createTextVNode as je, createVNode as h, defineAsyncComponent as Me, guardReactiveProps as g, mergeProps as Ne, nextTick as Pe, normalizeClass as _, normalizeProps as v, normalizeStyle as y, onMounted as Fe, openBlock as b, ref as x, renderList as S, renderSlot as C, resolveDynamicComponent as Ie, toDisplayString as w, toRefs as Le, unref as T, useCssVars as Re, watch as ze, withCtx as E, withKeys as Be } from "vue";
//#region src/components/vue-ui-chestnut.vue
var Ve = /* @__PURE__ */ e({ default: () => Nn }), He = ["id"], Ue = ["id"], We = {
	"aria-live": "polite",
	class: "sr-only"
}, Ge = {
	key: 2,
	ref: "noTitle",
	class: "vue-data-ui-no-title-space",
	style: "height:36px; width: 100%;background:transparent"
}, Ke = { style: { position: "relative" } }, qe = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Je = ["width", "height"], Ye = { key: 1 }, Xe = [
	"fill",
	"font-weight",
	"font-size",
	"x",
	"y"
], Ze = [
	"fill",
	"font-weight",
	"font-size",
	"x",
	"y"
], Qe = { key: 2 }, $e = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], et = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], tt = [
	"d",
	"stroke",
	"fill"
], nt = [
	"cx",
	"cy",
	"r",
	"fill"
], rt = [
	"aria-label",
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width",
	"onClick"
], it = { key: 3 }, at = [
	"x",
	"y",
	"font-size",
	"fill",
	"onClick"
], ot = { key: 0 }, st = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], ct = [
	"x",
	"y",
	"height",
	"width",
	"fill",
	"rx",
	"onClick"
], lt = [
	"aria-label",
	"x",
	"y",
	"height",
	"width",
	"fill",
	"rx",
	"stroke",
	"stroke-width",
	"onClick"
], ut = { key: 4 }, dt = [
	"x",
	"y",
	"fill",
	"font-size",
	"onClick"
], ft = ["d", "stroke"], pt = [
	"aria-label",
	"fill",
	"cx",
	"cy",
	"r",
	"onClick"
], mt = { key: 5 }, ht = [
	"x",
	"y",
	"font-size",
	"font-weight",
	"fill"
], gt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], _t = [
	"y",
	"height",
	"width"
], vt = { style: {
	width: "100%",
	height: "100%",
	display: "flex",
	"align-items": "center",
	"justify-content": "center",
	"flex-direction": "column"
} }, yt = { style: {
	display: "flex",
	"align-items": "center",
	"justify-content": "center",
	gap: "12px",
	"flex-wrap": "wrap",
	"flex-direction": "row"
} }, bt = {
	viewBox: "0 0 20 20",
	height: "16",
	width: "16"
}, xt = ["fill"], St = { key: 7 }, Ct = [
	"y",
	"height",
	"width"
], wt = { style: {
	width: "100%",
	height: "100%",
	display: "flex",
	"align-items": "center",
	"justify-content": "center",
	"flex-direction": "column"
} }, Tt = { style: {
	display: "flex",
	"align-items": "center",
	"justify-content": "center",
	gap: "12px",
	"flex-wrap": "wrap",
	"flex-direction": "row"
} }, Et = {
	viewBox: "0 0 20 20",
	height: "16",
	width: "16"
}, Dt = ["fill"], Ot = [
	"cx",
	"cy",
	"fill"
], kt = ["d", "stroke"], At = [
	"cx",
	"cy",
	"fill"
], jt = ["d", "stroke"], Mt = [
	"cx",
	"cy",
	"fill"
], Nt = [
	"cx",
	"cy",
	"fill"
], Pt = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], Ft = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], It = [
	"x",
	"text-anchor",
	"y",
	"fill",
	"font-size"
], Lt = [
	"x",
	"text-anchor",
	"y",
	"fill",
	"font-size"
], Rt = [
	"x",
	"text-anchor",
	"y",
	"fill",
	"font-size"
], zt = [
	"x",
	"text-anchor",
	"y",
	"fill",
	"font-size"
], Bt = [
	"x",
	"text-anchor",
	"y",
	"fill",
	"font-size"
], Vt = [
	"x",
	"y",
	"font-size",
	"fill"
], Ht = [
	"x",
	"y",
	"font-size",
	"fill"
], Ut = [
	"x",
	"y",
	"font-size",
	"fill"
], Wt = {
	key: 4,
	class: "vue-data-ui-watermark"
}, Gt = {
	key: 5,
	ref: "source",
	dir: "auto"
}, Kt = { class: "vue-ui-data-table" }, qt = { key: 0 }, Jt = ["data-cell"], Yt = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, Xt = { key: 0 }, Zt = { key: 1 }, Qt = ["data-cell"], $t = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, en = { key: 0 }, tn = { key: 1 }, nn = ["data-cell"], rn = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, an = { key: 0 }, on = { key: 1 }, sn = ["data-cell"], cn = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, ln = { key: 0 }, un = { key: 1 }, dn = ["data-cell"], fn = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, pn = { key: 0 }, mn = { key: 1 }, hn = ["data-cell"], gn = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, _n = { key: 0 }, vn = { key: 1 }, yn = ["data-cell"], bn = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, xn = { key: 0 }, Sn = { key: 1 }, Cn = ["data-cell"], wn = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, Tn = ["data-cell"], En = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, Dn = ["data-cell"], On = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, kn = ["data-cell"], An = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, jn = ["data-cell"], Mn = { style: {
	display: "flex",
	"align-items": "center",
	gap: "5px",
	"justify-content": "flex-end",
	width: "100%",
	"padding-right": "3px"
} }, Nn = /*#__PURE__*/ Se({
	__name: "vue-ui-chestnut",
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
	emits: [
		"selectRoot",
		"selectBranch",
		"selectNut",
		"copyAlt"
	],
	setup(e, { expose: Se, emit: Ve }) {
		Re((e) => ({ v442dd34a: e.tdo }));
		let Nn = Me(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Pn = Me(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Fn = Me(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), In = Me(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Ln = Me(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Rn = Me(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_chestnut: zn } = me(), { isThemeValid: Bn, warnInvalidTheme: Vn } = ye(), D = e, Hn = d(() => !!D.dataset && D.dataset.length), O = x(se()), Un = x(null), Wn = x(0), Gn = x(null), Kn = x(null), qn = x(!1), k = x(!1), A = x(""), j = x({
			level: "root",
			rootIndex: 0,
			branchIndex: 0,
			nutIndex: 0,
			locked: !1
		}), M = x(tr());
		fe({
			config: () => M.value,
			dataset: () => D.dataset,
			component: "VueUiChestnut",
			rules: [
				pe.emptyArray,
				{
					test: (e) => e.some((e) => e?.branches && e.branches.length > 10),
					message: [
						"👀 Some root has > 10 branches, which can make the chart heavy. Consider:",
						"",
						"▶️ Grouping some branches in broader categories."
					]
				},
				{
					test: (e) => e.some((e) => e?.branches && e.branches.some((e) => e?.breakdown && e?.breakdown.length > 6)),
					message: [
						"👀 Some donuts have > 6 series. Consider:",
						"",
						"▶️ Grouping small values into an \"Other\" category."
					]
				}
			]
		});
		let N = d(() => M.value.userOptions.useCursorPointer), Jn = d(() => i({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						grandTotal: { show: !1 },
						roots: {
							stroke: "#6A6A6A",
							labels: { show: !1 }
						},
						verticalSeparator: { stroke: "transparent" },
						branches: {
							stroke: "#6A6A6A",
							underlayerColor: "#6A6A6A90",
							labels: {
								show: !1,
								dataLabels: { show: !1 }
							}
						}
					}
				} }
			},
			userConfig: M.value.skeletonConfig ?? {}
		})), { loading: Yn, FINAL_DATASET: Xn } = ge({
			...Le(D),
			FINAL_CONFIG: M,
			prepareConfig: tr,
			skeletonDataset: D.config?.skeletonDataset ?? [{
				name: "_",
				color: "#969696",
				branches: [
					{
						name: "_",
						value: 32,
						breakdown: [{
							name: "_",
							value: 16,
							color: "#CACACA"
						}, {
							name: "_",
							value: 16,
							color: "#6A6A6A"
						}]
					},
					{
						name: "_",
						value: 16,
						breakdown: [{
							name: "_",
							value: 8,
							color: "#CACACA"
						}, {
							name: "_",
							value: 8,
							color: "#6A6A6A"
						}]
					},
					{
						name: "_",
						value: 8,
						breakdown: [{
							name: "_",
							value: 4,
							color: "#CACACA"
						}, {
							name: "_",
							value: 4,
							color: "#6A6A6A"
						}]
					},
					{
						name: "_",
						value: 4,
						breakdown: [{
							name: "_",
							value: 2,
							color: "#CACACA"
						}, {
							name: "_",
							value: 2,
							color: "#6A6A6A"
						}]
					}
				]
			}, {
				name: "_",
				color: "#C4C4C4",
				branches: [
					{
						name: "_",
						value: 24,
						breakdown: [{
							name: "_",
							value: 12,
							color: "#CACACA"
						}, {
							name: "_",
							value: 12,
							color: "#6A6A6A"
						}]
					},
					{
						name: "_",
						value: 12,
						breakdown: [{
							name: "_",
							value: 6,
							color: "#CACACA"
						}, {
							name: "_",
							value: 6,
							color: "#6A6A6A"
						}]
					},
					{
						name: "_",
						value: 6,
						breakdown: [{
							name: "_",
							value: 3,
							color: "#CACACA"
						}, {
							name: "_",
							value: 3,
							color: "#6A6A6A"
						}]
					},
					{
						name: "_",
						value: 2,
						breakdown: [{
							name: "_",
							value: 1,
							color: "#CACACA"
						}, {
							name: "_",
							value: 1,
							color: "#6A6A6A"
						}]
					}
				]
			}],
			skeletonConfig: i({
				defaultConfig: M.value,
				userConfig: Jn.value
			})
		}), { userOptionsVisible: Zn, setUserOptionsVisibility: Qn, keepUserOptionState: $n } = Te({ config: M.value }), { svgRef: er } = Ee({ config: M.value.style.chart.layout.title });
		function tr() {
			let e = ve({
				userConfig: D.config,
				defaultConfig: zn
			}), t = e.theme;
			if (!t) return e;
			if (!Bn.value(e)) return Vn(e), e;
			let n = ve({
				userConfig: Oe[t] || D.config,
				defaultConfig: e
			}), r = ve({
				userConfig: D.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : a[t] || o
			};
		}
		ze(() => D.config, (e) => {
			M.value = tr(), Zn.value = !M.value.userOptions.showOnChartHover, P.value.showTable = M.value.table.show, Dr();
		}, { deep: !0 });
		let { isPrinting: nr, isImaging: rr, generatePdf: ir, generateImage: ar } = he({
			elementId: `vue-ui-chestnut_${O.value}`,
			fileName: M.value.style.chart.layout.title.text || "vue-ui-chestnut",
			options: M.value.userOptions.print
		}), or = d(() => M.value.userOptions.show), sr = d(() => de(M.value.customPalette)), P = x({ showTable: M.value.table.show });
		ze(M, () => {
			P.value = { showTable: M.value.table.show };
		}, { immediate: !0 });
		let cr = x(null), lr = d(() => M.value.table.responsiveBreakpoint), F = x({
			gap: 6,
			padding: {
				top: 102,
				left: 12,
				right: 12,
				bottom: 96
			},
			width: 1024,
			height: 0,
			branchSize: 32,
			branchStart: 128
		}), I = d(() => ({
			left: F.value.padding.left,
			top: F.value.padding.top,
			right: F.value.width - F.value.padding.right,
			bottom: F.value.height - F.value.padding.bottom,
			width: F.value.width - (F.value.padding.left + F.value.padding.right),
			height: F.value.height - (F.value.padding.top + F.value.padding.bottom),
			seedX: F.value.padding.left + 64
		})), L = d(() => Xn.value.flatMap((e) => (e.branches || []).map((e) => e.value || 0)).reduce((e, t) => e + t, 0)), ur = d(() => M.value.debug), R = d(() => (ur.value && Xn.value.forEach((e, t) => {
			oe({
				datasetObject: e,
				requiredAttributes: ["name", "branches"]
			}).forEach((e) => {
				ue({
					componentName: "VueUiChestnut",
					type: "datasetSerieAttribute",
					property: e,
					index: t
				});
			}), e.branches && e.branches.forEach((e, n) => {
				oe({
					datasetObject: e,
					requiredAttributes: ["name", "value"]
				}).forEach((e) => {
					ue({
						componentName: "VueUiChestnut",
						type: "datasetSerieAttribute",
						property: e,
						index: `${t} - ${n}`
					});
				}), e.breakdown && e.breakdown.forEach((e, r) => {
					oe({
						datasetObject: e,
						requiredAttributes: ["name", "value"]
					}).forEach((e) => {
						ue({
							componentName: "VueUiChestnut",
							type: "datasetSerieAttribute",
							property: e,
							index: `${t} - ${n} - ${r}`
						});
					});
				});
			});
		}), Xn.value.map((e, t) => {
			let n = (e.branches || []).map((e) => e.value || 0).reduce((e, t) => e + t, 0);
			return {
				...e,
				color: ee(e.color) || sr.value[t] || o[t] || o[t % o.length],
				id: e.id || `root_${t}_${O.value}`,
				type: "root",
				total: n,
				rootIndex: t,
				branches: (e.branches || []).map((r, i) => ({
					...r,
					rootName: e.name,
					rootIndex: t,
					color: ee(e.color) || sr.value[t] || o[t] || o[t % o.length],
					value: r.value >= 0 ? r.value : 0,
					id: r.id || `branch_${t}_${i}_${O.value}`,
					proportionToRoot: r.value / n,
					type: "branch",
					breakdown: (r.breakdown || []).map((a, te) => ({
						table: {
							rootName: e.name,
							rootValue: n,
							rootToTotal: n / L.value,
							branchName: r.name,
							branchValue: r.value,
							branchToTotal: r.value / L.value,
							branchToRoot: r.value / n,
							nutName: a.name,
							nutValue: a.value,
							nutToTotal: a.value / L.value,
							nutToRoot: a.value / n,
							nutToBranch: a.value / r.value
						},
						...a,
						type: "nut",
						branchName: r.name,
						rootName: e.name,
						branchTotal: r.value >= 0 ? r.value : 0,
						proportionToBranch: a.value / r.value,
						proportionToRoot: a.value / n,
						proportionToTree: a.value / L.value,
						rootIndex: t,
						id: a.id || `nut_${t}_${i}_${te}_${O.value}`,
						color: ee(a.color) || sr.value[te] || o[te] || o[te % o.length],
						value: a.value >= 0 ? a.value : 0
					}))
				}))
			};
		})));
		function dr() {
			return R.value;
		}
		let fr = Ve, pr = d(() => R.value.flatMap((e) => e.branches).length), mr = d(() => Math.max(...R.value.map((e) => e.branches.map((e) => e.value).reduce((e, t) => e + t, 0)))), hr = d(() => Math.max(...R.value.flatMap((e) => e.branches.map((e) => e.value)))), gr = d(() => 256 + F.value.padding.left), z = d(() => R.value.sort((e, t) => t.total - e.total).map((e, t) => {
			let n = I.value.height / R.value.length / 2, r = e.total / mr.value * (n > 64 ? 64 : n);
			return {
				...e,
				x: I.value.seedX,
				y: I.value.top + I.value.height / R.value.length * (t + 1) - (I.value.height / R.value.length / 2 + F.value.gap / 2),
				r: r < F.value.branchSize / 2 ? F.value.branchSize / 2 : r
			};
		})), _r = d(() => M.value.style.chart.layout.branches.widthRatio <= 0 ? .1 : M.value.style.chart.layout.branches.widthRatio > 1.8 ? 1.8 : M.value.style.chart.layout.branches.widthRatio), vr = d(() => z.value.flatMap((e) => e.branches)), B = d(() => vr.value.sort((e, t) => t.value - e.value).map((e, t) => ({
			...e,
			y1: t * F.value.branchSize + I.value.top + t * F.value.gap,
			y2: t * F.value.branchSize + F.value.branchSize,
			x1: gr.value,
			x2: 384 * e.value / hr.value * _r.value + gr.value
		}))), V = d(() => z.value), yr = d(() => V.value.map((e) => B.value.filter((t) => t.rootIndex === e.rootIndex).sort((e, t) => e.y1 - t.y1))), br = d(() => V.value.map((e, t) => yr.value[t].map((e) => (e.breakdown || []).map((t, n) => ({
			...t,
			nutIndex: n,
			rootIndex: e.rootIndex,
			branchId: e.id,
			branchName: e.name,
			branchValue: e.value,
			rootName: e.rootName
		})))));
		function xr(e) {
			let t = z.value.find((t) => t.rootIndex === e.rootIndex);
			return {
				x: t.x,
				y: t.y,
				r: t.r
			};
		}
		let H = x(null), U = x(null), W = x(null), G = x(null);
		function K() {
			H.value = null, W.value = null, G.value = null;
		}
		function q(e) {
			if (H.value) return e.type === "root" ? e.rootIndex === H.value.rootIndex : e.type === "branch" ? e.id === H.value.id : e.type === "nut" && e.branchName === H.value.name && e.rootIndex === H.value.rootIndex;
			if (W.value) return e.type === "root" ? e.rootIndex === W.value.rootIndex : e.type === "branch" ? e.id === W.value.id : e.type === "nut" && e.branchName === W.value.name && e.rootIndex === W.value.rootIndex;
			if (G.value) return e.type === "root" ? e.id === G.value.id : e.type === "branch" || e.type === "nut" ? e.rootIndex === G.value.rootIndex : !1;
			if (ni.value && k.value) {
				if (j.value.level === "root") return e.type === "root" ? Z.value && e.id === Z.value.id : e.type === "branch" || e.type === "nut" ? Z.value && e.rootIndex === Z.value.rootIndex : !1;
				if (j.value.level === "branch") return e.type === "root" ? Z.value && e.rootIndex === Z.value.rootIndex : e.type === "branch" ? ei.value && e.id === ei.value.id : (e.type, !1);
				if (j.value.level === "nut") return e.type === "root" ? Z.value && e.rootIndex === Z.value.rootIndex : e.type === "branch" ? ti.value && e.id === ti.value.id : e.type === "nut" && ti.value && e.branchName === ti.value.name && e.rootIndex === ti.value.rootIndex;
			}
			return !0;
		}
		function Sr(e) {
			K();
			let t = V.value.findIndex((t) => t.rootIndex === e.rootIndex), n = (yr.value[t] || []).findIndex((t) => t.id === e.id);
			t !== -1 && (j.value.rootIndex = t), n !== -1 && (j.value.branchIndex = n), j.value.level = "nut", j.value.nutIndex = 0, Pe(() => {
				H.value = e, W.value = e, U.value = ie({
					series: e.breakdown,
					base: 1
				}, e.x2 + 24 + M.value.style.chart.layout.nuts.offsetX, e.y1 + F.value.branchSize / 2, 80, 80), fr("selectNut", e.breakdown), Q();
			});
		}
		function J() {
			H.value = null, U.value = null, fr("selectNut", null);
		}
		function Cr(e) {
			let t = V.value.findIndex((t) => t.rootIndex === e.rootIndex), n = (yr.value[t] || []).findIndex((t) => t.id === e.id);
			t !== -1 && (j.value.rootIndex = t), n !== -1 && (j.value.branchIndex = n), j.value.level = "branch", j.value.nutIndex = 0, W.value && W.value.id === e.id ? (W.value = null, K(), fr("selectBranch", null), Q()) : (K(), W.value = e, fr("selectBranch", e), Q());
		}
		function wr(e) {
			let t = V.value.findIndex((t) => t.id === e.id);
			t !== -1 && (j.value.rootIndex = t), j.value.level = "root", j.value.branchIndex = 0, j.value.nutIndex = 0, G.value && G.value.id === e.id ? (K(), fr("selectRoot", null), Q()) : (K(), G.value = e, fr("selectRoot", e), Q());
		}
		function Tr() {
			return I.value.bottom - (H.value.y1 + 180) < 0 ? 0 : I.value.bottom;
		}
		function Er(e) {
			return e.proportion * 100 > M.value.style.chart.layout.nuts.selected.labels.dataLabels.hideUnderValue;
		}
		Fe(() => {
			Dr();
		});
		function Dr() {
			re(D.dataset) && ue({
				componentName: "VueUiChestnut",
				type: "dataset",
				debug: ur.value
			});
			let e = pr.value * (F.value.branchSize + F.value.gap) + F.value.padding.top + F.value.padding.bottom;
			F.value.height = e;
		}
		let Y = d(() => ({
			head: [
				M.value.table.th.translations.rootName,
				M.value.table.th.translations.rootValue,
				M.value.table.th.translations.rootToTotal,
				M.value.table.th.translations.branchName,
				M.value.table.th.translations.branchValue,
				M.value.table.th.translations.branchToRoot,
				M.value.table.th.translations.branchToTotal,
				M.value.table.th.translations.nutName,
				M.value.table.th.translations.nutValue,
				M.value.table.th.translations.nutToBranch,
				M.value.table.th.translations.nutToRoot,
				M.value.table.th.translations.nutToTotal
			],
			body: R.value.flatMap((e, t) => e.branches.flatMap((e, t) => e.breakdown.flatMap((e, t) => e.table)))
		}));
		function Or(e = null) {
			Pe(() => {
				let n = [
					[M.value.style.chart.layout.title.text],
					[M.value.style.chart.layout.title.subtitle.text],
					[""],
					["Grand total", L.value],
					[""]
				], i = Y.value.head, a = Y.value.body.map((e, t) => [
					Y.value.body[t - 1] && Y.value.body[t - 1].rootName === e.rootName ? "" : e.rootName,
					Y.value.body[t - 1] && Y.value.body[t - 1].rootName === e.rootName ? "" : e.rootValue,
					Y.value.body[t - 1] && Y.value.body[t - 1].rootName === e.rootName ? "" : e.rootToTotal,
					Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? "" : e.branchName,
					Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? "" : e.branchValue,
					Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? "" : e.branchToRoot,
					Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? "" : e.branchToTotal,
					e.nutName,
					e.nutValue,
					e.nutToBranch,
					e.nutToRoot,
					e.nutToTotal
				]), o = n.concat([i]).concat(a), ee = r(o);
				e ? e(ee) : t({
					csvContent: ee,
					title: M.value.style.chart.layout.title.text || "vue-ui-chestnut"
				});
			});
		}
		let kr = x(!1);
		function Ar(e) {
			kr.value = e, Wn.value += 1;
		}
		function jr() {
			P.value.showTable = !P.value.showTable;
		}
		let Mr = x(!1);
		function Nr() {
			Mr.value = !Mr.value;
		}
		async function Pr({ scale: e = 2 } = {}) {
			if (!Un.value) return;
			let { width: t, height: n } = Un.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await xe({
				domElement: Un.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: M.value.style.chart.layout.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		function Fr(e) {
			let t = xr(e), n = e.x1, r = e.y1, i = F.value.branchSize, a = t.x + t.r / 2, o = t.y;
			return [
				`M ${n},${r}`,
				`C ${n - 20},${r} ${n - 20},${r} ${a},${o}`,
				`C ${a},${o} ${n - 20},${r + i} ${n},${r + i}`,
				"Z"
			].join(" ");
		}
		let Ir = d(() => {
			let e = M.value.table.useDialog && !M.value.table.show, t = P.value.showTable;
			return {
				component: e ? Rn : Pn,
				title: `${M.value.style.chart.layout.title.text}${M.value.style.chart.layout.title.subtitle.text ? `: ${M.value.style.chart.layout.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: M.value.table.th.backgroundColor,
					color: M.value.table.th.color,
					headerColor: M.value.table.th.color,
					headerBg: M.value.table.th.backgroundColor,
					isFullscreen: kr.value,
					fullscreenParent: Un.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: N.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: M.value.style.chart.backgroundColor,
							color: M.value.style.chart.color
						},
						head: {
							backgroundColor: M.value.style.chart.backgroundColor,
							color: M.value.style.chart.color
						}
					}
				}
			};
		});
		ze(() => P.value.showTable, async (e) => {
			M.value.table.show || (e && M.value.table.useDialog && Gn.value ? (await Pe(), Gn.value.open()) : "close" in Gn.value && Gn.value.close());
		});
		let { isResponsive: Lr } = De(cr, lr);
		function Rr() {
			P.value.showTable = !1, Kn.value && Kn.value.setTableIconState(!1);
		}
		let zr = d(() => R.value.map((e, t) => ({
			...e,
			display: `${e.name}: ${l(M.value.style.chart.layout.roots.labels.formatter, e.total, s({
				p: M.value.style.chart.layout.legend.prefix,
				v: e.total,
				s: M.value.style.chart.layout.legend.suffix,
				r: M.value.style.chart.layout.legend.roundingValue
			}), { datapoint: e })} (${s({
				v: e.total / L.value * 100,
				s: "%",
				r: M.value.style.chart.layout.legend.roundingPercentage
			})})`
		}))), Br = d(() => zr.value.map((e) => ({
			color: e.color,
			name: e.display,
			shape: "circle"
		}))), Vr = d(() => M.value.style.chart.backgroundColor), Hr = d(() => ({
			...M.value.style.chart.layout.legend,
			textAlign: "center",
			show: !0,
			position: "bottom"
		})), Ur = d(() => M.value.style.chart.layout.title), { isCallbackImaging: Wr, isCallbackSvg: Gr, generateSvg: Kr, onGenerateImage: qr } = be({
			svg: er,
			title: Ur,
			legend: Hr,
			legendItems: Br,
			backgroundColor: Vr,
			titleEmbedded: !0,
			getSvgCallback: () => M.value.userOptions.callbacks.svg,
			generateImage: ar
		});
		async function Jr() {
			if (fr("copyAlt", {
				config: M.value,
				dataset: R.value
			}), !M.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(M.value.userOptions.callbacks.altCopy({
				config: M.value,
				dataset: R.value
			}));
		}
		function Yr(e) {
			if (!e) return "";
			let t = s({
				p: M.value.style.chart.layout.legend.prefix,
				v: e.total,
				s: M.value.style.chart.layout.legend.suffix,
				r: M.value.style.chart.layout.legend.roundingValue
			}), n = s({
				v: e.total / L.value * 100,
				s: "%",
				r: M.value.style.chart.layout.legend.roundingPercentage
			});
			return `Root ${e.name}. Value ${t}. ${n} of total.`;
		}
		function Xr(e) {
			if (!e) return "";
			let t = s({
				p: M.value.style.chart.layout.branches.labels.dataLabels.prefix,
				v: e.value,
				s: M.value.style.chart.layout.branches.labels.dataLabels.suffix,
				r: M.value.style.chart.layout.branches.labels.dataLabels.roundingValue
			}), n = s({
				v: e.proportionToRoot * 100,
				s: "%",
				r: M.value.style.chart.layout.legend.roundingPercentage
			});
			return `Branch ${e.name}. Root ${e.rootName}. Value ${t}. ${n} of root ${e.rootName}.`;
		}
		function Zr(e) {
			if (!e) return "";
			let t = s({
				p: M.value.style.chart.layout.legend.prefix,
				v: e.value,
				s: M.value.style.chart.layout.legend.suffix,
				r: M.value.style.chart.layout.nuts.selected.labels.roundingValue
			}), n = s({
				v: e.proportionToBranch * 100,
				s: "%",
				r: M.value.style.chart.layout.nuts.selected.labels.roundingPercentage
			});
			return `Nut ${e.name}. Branch ${e.branchName}. Root ${e.rootName}. Value ${t}. ${n} of branch ${e.branchName}.`;
		}
		let Qr = d(() => V.value[j.value.rootIndex] || null), X = d(() => (yr.value[j.value.rootIndex] || [])[j.value.branchIndex] || null), $r = d(() => ((br.value[j.value.rootIndex] || [])[j.value.branchIndex] || [])[j.value.nutIndex] || null), Z = d(() => Qr.value), ei = d(() => X.value), ti = d(() => j.value.level === "nut" ? X.value : null), ni = d(() => qn.value);
		function Q() {
			if (j.value.level === "root") {
				A.value = Yr(Qr.value);
				return;
			}
			if (j.value.level === "branch") {
				A.value = Xr(X.value);
				return;
			}
			j.value.level === "nut" && (A.value = Zr($r.value));
		}
		function $() {
			j.value = {
				level: "root",
				rootIndex: 0,
				branchIndex: 0,
				nutIndex: 0,
				locked: !1
			}, A.value = "";
		}
		function ri() {
			let e = yr.value[j.value.rootIndex] || [];
			if (!e.length) {
				j.value.branchIndex = 0;
				return;
			}
			j.value.branchIndex >= e.length && (j.value.branchIndex = 0), j.value.branchIndex < 0 && (j.value.branchIndex = e.length - 1);
		}
		function ii() {
			let e = (br.value[j.value.rootIndex] || [])[j.value.branchIndex] || [];
			if (!e.length) {
				j.value.nutIndex = 0;
				return;
			}
			j.value.nutIndex >= e.length && (j.value.nutIndex = 0), j.value.nutIndex < 0 && (j.value.nutIndex = e.length - 1);
		}
		function ai(e) {
			if (j.value.level === "root") {
				let t = V.value.length;
				if (!t) return;
				j.value.rootIndex += e, j.value.rootIndex >= t && (j.value.rootIndex = 0), j.value.rootIndex < 0 && (j.value.rootIndex = t - 1), j.value.branchIndex = 0, j.value.nutIndex = 0, Q();
				return;
			}
			if (j.value.level === "branch") {
				j.value.branchIndex += e, ri(), j.value.nutIndex = 0, Q();
				return;
			}
			j.value.level === "nut" && (j.value.nutIndex += e, ii(), Q());
		}
		function oi(e) {
			if (e > 0) {
				if (j.value.level === "root") {
					if (!(yr.value[j.value.rootIndex] || []).length) return;
					j.value.level = "branch", j.value.branchIndex = 0, j.value.nutIndex = 0, Q();
					return;
				}
				if (j.value.level === "branch") {
					if (!((br.value[j.value.rootIndex] || [])[j.value.branchIndex] || []).length) return;
					j.value.level = "nut", j.value.nutIndex = 0, X.value ? Sr(X.value) : Q();
					return;
				}
				return;
			}
			if (j.value.level === "nut") {
				J(), W.value = null, j.value.level = "branch", j.value.nutIndex = 0, Q();
				return;
			}
			j.value.level === "branch" && (W.value = null, G.value = null, j.value.level = "root", j.value.nutIndex = 0, Q());
		}
		function si() {
			if (j.value.level === "root" && Qr.value) {
				wr(Qr.value), A.value = `${Yr(Qr.value)} selected.`;
				return;
			}
			if (j.value.level === "branch" && X.value) {
				Cr(X.value), A.value = `${Xr(X.value)} selected.`;
				return;
			}
			j.value.level === "nut" && X.value && $r.value && (Sr(X.value), A.value = `${Zr($r.value)} details opened.`);
		}
		function ci() {
			qn.value = !0, k.value = !1;
		}
		function li() {
			qn.value = !1, k.value = !1, A.value = "", !G.value && !W.value && !H.value && $();
		}
		function ui(e) {
			let t = e.key === "ArrowUp", n = e.key === "ArrowDown", r = e.key === "ArrowLeft", i = e.key === "ArrowRight", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!(!t && !n && !r && !i && !a && !o)) {
				if (e.preventDefault(), e.stopPropagation(), qn.value ||= !0, o) {
					K(), $(), k.value = !1;
					return;
				}
				if ((t || n || r || i) && (k.value = !0), t) {
					ai(-1);
					return;
				}
				if (n) {
					ai(1);
					return;
				}
				if (r) {
					oi(-1);
					return;
				}
				if (i) {
					oi(1);
					return;
				}
				a && si();
			}
		}
		let di = d(() => ({
			headers: Y.value.head,
			rows: Y.value.body.map((e) => [
				e.rootName,
				e.rootValue,
				s({
					v: e.rootToTotal * 100,
					s: "%",
					r: M.value.table.td.roundingPercentage
				}),
				e.branchName,
				e.branchValue,
				s({
					v: e.branchToRoot * 100,
					s: "%",
					r: M.value.table.td.roundingPercentage
				}),
				s({
					v: e.branchToTotal * 100,
					s: "%",
					r: M.value.table.td.roundingPercentage
				}),
				e.nutName,
				e.nutValue,
				s({
					v: e.nutToBranch * 100,
					s: "%",
					r: M.value.table.td.roundingPercentage
				}),
				s({
					v: e.nutToRoot * 100,
					s: "%",
					r: M.value.table.td.roundingPercentage
				}),
				s({
					v: e.nutToTotal * 100,
					s: "%",
					r: M.value.table.td.roundingPercentage
				})
			])
		}));
		return Se({
			getData: dr,
			getImage: Pr,
			generatePdf: ir,
			generateCsv: Or,
			generateImage: ar,
			generateSvg: Kr,
			toggleTable: jr,
			toggleAnnotator: Nr,
			toggleFullscreen: Ar,
			copyAlt: Jr
		}), (e, t) => (b(), p("div", {
			class: _(`vue-data-ui-component vue-ui-chestnut ${kr.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			ref_key: "chestnutChart",
			ref: Un,
			id: `vue-ui-chestnut_${O.value}`,
			style: y(`font-family:${M.value.style.fontFamily};width:100%; text-align:center;background:${M.value.style.chart.backgroundColor}`),
			onMouseenter: t[12] ||= () => T(Qn)(!0),
			onMouseleave: t[13] ||= () => T(Qn)(!1)
		}, [
			m("div", {
				id: `chart-instructions-${O.value}`,
				class: "sr-only"
			}, [m("p", null, w(M.value.a11y.translations.keyboardNavigation), 1)], 8, Ue),
			m("div", We, w(A.value), 1),
			di.value?.rows?.length ? (b(), ke(we, {
				key: 0,
				uid: O.value,
				head: di.value.headers,
				body: di.value.rows,
				notice: M.value.a11y.translations.tableAvailable,
				caption: M.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : f("", !0),
			M.value.userOptions.buttons.annotator ? (b(), ke(T(Fn), {
				key: 1,
				svgRef: T(er),
				backgroundColor: M.value.style.chart.backgroundColor,
				color: M.value.style.chart.color,
				active: Mr.value,
				isCursorPointer: N.value,
				onClose: Nr
			}, {
				"annotator-action-close": E(() => [C(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": E(({ color: t }) => [C(e.$slots, "annotator-action-color", v(g({ color: t })), void 0, !0)]),
				"annotator-action-draw": E(({ mode: t }) => [C(e.$slots, "annotator-action-draw", v(g({ mode: t })), void 0, !0)]),
				"annotator-action-undo": E(({ disabled: t }) => [C(e.$slots, "annotator-action-undo", v(g({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": E(({ disabled: t }) => [C(e.$slots, "annotator-action-redo", v(g({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": E(({ disabled: t }) => [C(e.$slots, "annotator-action-delete", v(g({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : f("", !0),
			or.value ? (b(), p("div", Ge, null, 512)) : f("", !0),
			M.value.userOptions.show && Hn.value && (T($n) || T(Zn)) ? (b(), ke(T(In), {
				ref_key: "userOptionsRef",
				ref: Kn,
				key: `user_options_${Wn.value}`,
				backgroundColor: M.value.style.chart.backgroundColor,
				color: M.value.style.chart.color,
				isImaging: T(rr),
				isPrinting: T(nr),
				uid: O.value,
				hasPdf: M.value.userOptions.buttons.pdf,
				hasImg: M.value.userOptions.buttons.img,
				hasSvg: M.value.userOptions.buttons.svg,
				hasXls: M.value.userOptions.buttons.csv,
				hasTable: M.value.userOptions.buttons.table,
				hasFullscreen: M.value.userOptions.buttons.fullscreen,
				hasAltCopy: M.value.userOptions.buttons.altCopy,
				isFullscreen: kr.value,
				titles: { ...M.value.userOptions.buttonTitles },
				chartElement: Un.value,
				position: M.value.userOptions.position,
				hasAnnotator: M.value.userOptions.buttons.annotator,
				isAnnotation: Mr.value,
				callbacks: M.value.userOptions.callbacks,
				printScale: M.value.userOptions.print.scale,
				tableDialog: M.value.table.useDialog,
				isCursorPointer: N.value,
				onToggleFullscreen: Ar,
				onGeneratePdf: T(ir),
				onGenerateCsv: Or,
				onGenerateImage: T(qr),
				onGenerateSvg: T(Kr),
				onToggleTable: jr,
				onToggleAnnotator: Nr,
				onCopyAlt: Jr,
				style: y({ visibility: T($n) ? T(Zn) ? "visible" : "hidden" : "visible" })
			}, Ae({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: E(({ isOpen: t, color: n }) => [C(e.$slots, "menuIcon", v(g({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: E(() => [C(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: E(() => [C(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: E(() => [C(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: E(() => [C(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: E(() => [C(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: E(({ toggleFullscreen: t, isFullscreen: n }) => [C(e.$slots, "optionFullscreen", v(g({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: E(({ toggleAnnotator: t, isAnnotator: n }) => [C(e.$slots, "optionAnnotator", v(g({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: E(({ altCopy: t }) => [C(e.$slots, "optionAltCopy", v(g({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: E(() => [C(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: E(() => [C(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isImaging.isPrinting.uid.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : f("", !0),
			m("div", Ke, [F.value.height > 0 ? (b(), p("svg", {
				key: 0,
				ref_key: "svgRef",
				ref: er,
				xmlns: T(le),
				"aria-describedby": `chart-instructions-${O.value}`,
				class: _({
					"vue-data-ui-fullscreen--on": kr.value,
					"vue-data-ui-fulscreen--off": !kr.value
				}),
				viewBox: `0 0 ${F.value.width <= 0 ? 10 : F.value.width} ${F.value.height <= 0 ? 10 : F.value.height}`,
				style: y(`overflow:visible;background:transparent;color:${M.value.style.chart.color}`),
				tabindex: "0",
				onFocus: ci,
				onBlur: li,
				onKeydown: ui
			}, [
				h(T(Ln)),
				e.$slots["chart-background"] ? (b(), p("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: F.value.width <= 0 ? 10 : F.value.width,
					height: F.value.height <= 0 ? 10 : F.value.height,
					style: { pointerEvents: "none" }
				}, [C(e.$slots, "chart-background", {}, void 0, !0)], 8, Je)) : f("", !0),
				H.value ? f("", !0) : (b(), p("g", Ye, [M.value.style.chart.layout.title.text ? (b(), p("text", {
					key: 0,
					"text-anchor": "middle",
					fill: M.value.style.chart.layout.title.color,
					"font-weight": M.value.style.chart.layout.title.bold ? "bold" : "normal",
					"font-size": M.value.style.chart.layout.title.fontSize,
					x: F.value.width / 2,
					y: 12 + M.value.style.chart.layout.title.fontSize + M.value.style.chart.layout.title.offsetY,
					onClick: t[0] ||= () => {
						K(), $(), k.value = !1;
					}
				}, w(M.value.style.chart.layout.title.text), 9, Xe)) : f("", !0), M.value.style.chart.layout.title.subtitle.text ? (b(), p("text", {
					key: 1,
					"text-anchor": "middle",
					fill: M.value.style.chart.layout.title.subtitle.color,
					"font-weight": M.value.style.chart.layout.title.subtitle.bold ? "bold" : "normal",
					"font-size": M.value.style.chart.layout.title.subtitle.fontSize,
					x: F.value.width / 2,
					y: 48 + M.value.style.chart.layout.title.subtitle.fontSize + M.value.style.chart.layout.title.subtitle.offsetY,
					onClick: t[1] ||= () => {
						K(), $(), k.value = !1;
					}
				}, w(M.value.style.chart.layout.title.subtitle.text), 9, Ze)) : f("", !0)])),
				m("defs", null, [
					(b(!0), p(u, null, S(R.value, (e, t) => (b(), ke(Ce, {
						t: "radial",
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						id: `root_gradient_${O.value}_${e.rootIndex}`,
						key: `root_gradient_${O.value}_${e.rootIndex}`,
						stops: [[
							"0%",
							T(n)(T(te)(e.color, .05), 100 - M.value.style.chart.layout.roots.gradientIntensity),
							1
						], [
							"100%",
							e.color,
							1
						]]
					}, null, 8, ["id", "stops"]))), 128)),
					(b(!0), p(u, null, S(R.value, (e) => (b(), ke(Ce, {
						t: "linear",
						x1: "0%",
						y1: "0%",
						x2: "100%",
						y2: "0%",
						id: `branch_gradient_${O.value}_${e.rootIndex}`,
						key: `branch_gradient_${O.value}_${e.rootIndex}`,
						stops: [[
							"0%",
							e.color,
							1
						], [
							"100%",
							T(n)(T(te)(e.color, .02), 100 - M.value.style.chart.layout.branches.gradientIntensity),
							1
						]]
					}, null, 8, ["id", "stops"]))), 128)),
					h(Ce, {
						t: "radial",
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						id: `nutpick_${O.value}`,
						stops: [
							[
								"0%",
								T(n)("#FFFFFF", 0),
								0
							],
							[
								"80%",
								T(n)("#FFFFFF", M.value.style.chart.layout.nuts.selected.gradientIntensity),
								1
							],
							[
								"100%",
								T(n)("#FFFFFF", 0),
								0
							]
						]
					}, null, 8, ["id", "stops"]),
					h(Ce, {
						t: "radial",
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						id: `nut_${O.value}`,
						stops: [
							[
								"0%",
								T(n)("#FFFFFF", 0),
								0
							],
							[
								"80%",
								T(n)("#FFFFFF", M.value.style.chart.layout.nuts.gradientIntensity),
								1
							],
							[
								"100%",
								T(n)("#FFFFFF", 0),
								0
							]
						]
					}, null, 8, ["id", "stops"]),
					h(Ce, {
						t: "radial",
						cx: "50%",
						cy: "50%",
						r: "50%",
						fx: "50%",
						fy: "50%",
						id: `nut_underlayer_${O.value}`,
						stops: [
							[
								"0%",
								T(n)(M.value.style.chart.backgroundColor, 100),
								1
							],
							[
								"80%",
								T(n)(M.value.style.chart.backgroundColor, 60),
								1
							],
							[
								"100%",
								T(n)(M.value.style.chart.backgroundColor, 0),
								1
							]
						]
					}, null, 8, ["id", "stops"])
				]),
				M.value.style.chart.layout.grandTotal.show ? (b(), p("g", Qe, [m("text", {
					x: I.value.seedX,
					y: 32 + M.value.style.chart.layout.grandTotal.offsetY,
					"font-size": M.value.style.chart.layout.grandTotal.fontSize,
					"font-weight": M.value.style.chart.layout.grandTotal.bold ? "bold" : "normal",
					fill: M.value.style.chart.layout.grandTotal.color,
					"text-anchor": "middle",
					onClick: t[2] ||= () => {
						K(), $(), k.value = !1;
					}
				}, w(M.value.style.chart.layout.grandTotal.text), 9, $e), m("text", {
					x: I.value.seedX,
					y: 38 + M.value.style.chart.layout.grandTotal.fontSize + M.value.style.chart.layout.grandTotal.offsetY,
					"font-size": M.value.style.chart.layout.grandTotal.fontSize,
					"font-weight": M.value.style.chart.layout.grandTotal.bold ? "bold" : "normal",
					fill: M.value.style.chart.layout.grandTotal.color,
					"text-anchor": "middle",
					onClick: t[3] ||= () => {
						K(), $(), k.value = !1;
					}
				}, w(T(l)(M.value.style.chart.layout.grandTotal.formatter, L.value, T(s)({
					p: M.value.style.chart.layout.grandTotal.prefix,
					v: L.value,
					s: M.value.style.chart.layout.grandTotal.suffix,
					r: M.value.style.chart.layout.grandTotal.roundingValue
				}))), 9, et)])) : f("", !0),
				(b(!0), p(u, null, S(B.value, (e) => (b(), p("g", null, [m("defs", null, [h(Ce, {
					t: "linear",
					id: `link_grad_${e.id}`,
					stops: [[
						"0%",
						e.color,
						1
					], [
						"100%",
						T(n)(e.color, M.value.style.chart.layout.links.opacity),
						1
					]]
				}, null, 8, ["id", "stops"])]), m("path", {
					d: Fr(e),
					stroke: T(n)(e.color, M.value.style.chart.layout.links.opacity),
					fill: `url(#link_grad_${e.id})`,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					style: y(`opacity:${+!!q(e)}`),
					onClick: t[4] ||= () => {
						K(), $(), k.value = !1;
					}
				}, null, 12, tt)]))), 256)),
				(b(!0), p(u, null, S(z.value, (e) => (b(), p("circle", {
					cx: e.x,
					cy: e.y,
					r: e.r,
					fill: M.value.style.chart.layout.roots.underlayerColor,
					stroke: "none",
					style: y(`cursor:${N.value ? "pointer" : "default"}; opacity:${q(e) ? 1 : .05}`)
				}, null, 12, nt))), 256)),
				(b(!0), p(u, null, S(z.value, (e, t) => (b(), p("circle", {
					"aria-label": Yr(e),
					cx: e.x,
					cy: e.y,
					r: e.r,
					fill: M.value.style.chart.layout.roots.useGradient ? `url(#root_gradient_${O.value}_${e.rootIndex})` : e.color,
					stroke: M.value.style.chart.layout.roots.stroke,
					"stroke-width": M.value.style.chart.layout.roots.strokeWidth,
					style: y(`cursor:${N.value ? "pointer" : "default"}; opacity:${q(e) ? 1 : .05}`),
					onClick: (t) => wr(e)
				}, null, 12, rt))), 256)),
				M.value.style.chart.layout.roots.labels.show ? (b(), p("g", it, [(b(!0), p(u, null, S(z.value, (e, t) => (b(), p("text", {
					x: e.x,
					y: e.y + M.value.style.chart.layout.roots.labels.fontSize / 2.6,
					"text-anchor": "middle",
					"font-size": M.value.style.chart.layout.roots.labels.fontSize,
					fill: M.value.style.chart.layout.roots.labels.adaptColorToBackground ? T(ce)(e.color) : M.value.style.chart.layout.roots.labels.color,
					"font-weight": "bold",
					style: y(`cursor:${N.value ? "pointer" : "default"}; opacity:${q(e) ? 1 : .05}`),
					onClick: (t) => wr(e)
				}, w(T(l)(M.value.style.chart.layout.roots.labels.formatter, e.total, T(s)({
					p: M.value.style.chart.layout.roots.labels.prefix,
					v: e.total,
					s: M.value.style.chart.layout.roots.labels.suffix,
					r: M.value.style.chart.layout.roots.labels.roundingValue
				}), { datapoint: e })), 13, at))), 256)), (b(!0), p(u, null, S(z.value, (e) => (b(), p("g", null, [H.value && e.rootIndex === H.value.rootIndex || W.value && e.rootIndex === W.value.rootIndex || G.value && e.rootIndex === G.value.rootIndex ? (b(), p("g", ot, [m("text", {
					x: e.x,
					y: e.y + e.r + 24,
					"text-anchor": "middle",
					fill: M.value.style.chart.layout.roots.labels.name.color,
					"font-size": M.value.style.chart.layout.roots.labels.name.fontSize,
					"font-weight": M.value.style.chart.layout.roots.labels.name.bold ? "bold" : "normal",
					onClick: t[5] ||= () => {
						K(), $(), k.value = !1;
					}
				}, w(e.name), 9, st)])) : f("", !0)]))), 256))])) : f("", !0),
				(b(!0), p(u, null, S(B.value, (e) => (b(), p("rect", {
					x: e.x1,
					y: e.y1,
					height: F.value.branchSize,
					width: e.x2 - e.x1,
					fill: M.value.style.chart.layout.branches.underlayerColor,
					rx: M.value.style.chart.layout.branches.borderRadius,
					stroke: "none",
					style: y(`opacity:${q(e) ? 1 : .05}`),
					onClick: (t) => Cr(e)
				}, null, 12, ct))), 256)),
				(b(!0), p(u, null, S(B.value, (e, t) => (b(), p("rect", {
					"aria-label": Xr(e),
					x: e.x1,
					y: e.y1,
					height: F.value.branchSize,
					width: e.x2 - e.x1,
					fill: M.value.style.chart.layout.branches.useGradient ? `url(#branch_gradient_${O.value}_${e.rootIndex})` : e.color,
					rx: M.value.style.chart.layout.branches.borderRadius,
					stroke: M.value.style.chart.layout.branches.stroke,
					"stroke-width": M.value.style.chart.layout.branches.strokeWidth,
					style: y(`cursor:${N.value ? "pointer" : "default"}; opacity:${q(e) ? 1 : .05}`),
					onClick: (t) => Cr(e)
				}, null, 12, lt))), 256)),
				M.value.style.chart.layout.branches.labels.dataLabels.show ? (b(), p("g", ut, [(b(!0), p(u, null, S(B.value, (e) => (b(), p("g", null, [e.proportionToRoot * 100 > M.value.style.chart.layout.branches.labels.dataLabels.hideUnderValue ? (b(), p("text", {
					key: 0,
					x: e.x1 + 6,
					y: e.y1 + F.value.branchSize / 1.5,
					"text-anchor": "start",
					fill: T(ce)(e.color),
					"font-size": M.value.style.chart.layout.branches.labels.dataLabels.fontSize,
					"font-weight": "bold",
					style: y(`cursor:${N.value ? "pointer" : "default"}; opacity:${q(e) ? 1 : .05}`),
					onClick: (t) => Cr(e)
				}, w(T(l)(M.value.style.chart.layout.branches.labels.dataLabels.formatter, e.value, T(s)({
					p: M.value.style.chart.layout.branches.labels.dataLabels.prefix,
					v: e.value,
					s: M.value.style.chart.layout.branches.labels.dataLabels.suffix,
					r: M.value.style.chart.layout.branches.labels.dataLabels.roundingValue
				}), { datapoint: e })), 13, dt)) : f("", !0)]))), 256))])) : f("", !0),
				(b(!0), p(u, null, S(B.value, (e, t) => (b(), p("g", null, [(b(!0), p(u, null, S(T(ie)({
					series: e.breakdown,
					base: 1
				}, e.x2 + 24 + M.value.style.chart.layout.nuts.offsetX, e.y1 + F.value.branchSize / 2, F.value.branchSize / 3, F.value.branchSize / 3), (t, n) => (b(), p("path", {
					d: t.path,
					stroke: t.color,
					"stroke-width": 10,
					fill: "none",
					style: y(`opacity:${q(e) ? 1 : .1}`)
				}, null, 12, ft))), 256)), m("circle", {
					"aria-label": `Open details for branch ${e.name} in root ${e.rootName}`,
					fill: M.value.style.chart.layout.nuts.useGradient ? `url(#nut_${O.value})` : "transparent",
					cx: e.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
					cy: e.y1 + F.value.branchSize / 2,
					r: F.value.branchSize / 2 + 2,
					onClick: (t) => Sr(e),
					style: y(`cursor:${N.value ? "pointer" : "default"};opacity:${q(e) ? 1 : .1}`)
				}, null, 12, pt)]))), 256)),
				M.value.style.chart.layout.branches.labels.show && !W.value ? (b(), p("g", mt, [(b(!0), p(u, null, S(B.value, (e) => (b(), p("text", {
					x: e.x2 + F.value.branchSize + 24 + M.value.style.chart.layout.nuts.offsetX,
					y: e.y1 + F.value.branchSize / 2 + 5,
					"font-size": M.value.style.chart.layout.branches.labels.fontSize,
					"font-weight": M.value.style.chart.layout.branches.labels.bold ? "bold" : "normal",
					fill: M.value.style.chart.layout.branches.labels.color,
					"text-anchor": "start",
					style: y(`opacity:${q(e) ? 1 : .1}`)
				}, w(e.name), 13, ht))), 256))])) : f("", !0),
				m("line", {
					x1: 256 + F.value.padding.left,
					x2: 256 + F.value.padding.left,
					y1: I.value.top,
					y2: I.value.bottom,
					stroke: M.value.style.chart.layout.verticalSeparator.stroke,
					"stroke-width": M.value.style.chart.layout.verticalSeparator.strokeWidth
				}, null, 8, gt),
				!H.value && !W.value ? (b(), p("foreignObject", {
					key: 6,
					x: 0,
					y: I.value.bottom,
					height: F.value.height - I.value.bottom,
					width: F.value.width,
					style: { overflow: "visible" },
					"data-no-svg-export": "",
					onClick: t[6] ||= () => {
						K(), $(), k.value = !1;
					}
				}, [m("div", vt, [m("div", yt, [(b(!0), p(u, null, S(z.value, (e) => (b(), p("div", { style: y(`display:flex;align-items:center;gap:3px;flex-direction:row;font-size:${M.value.style.chart.layout.legend.fontSize}px;`) }, [(b(), p("svg", bt, [m("circle", {
					cx: "10",
					cy: "10",
					r: "10",
					fill: e.color,
					stroke: "none"
				}, null, 8, xt)])), T(Yn) ? f("", !0) : (b(), p(u, { key: 0 }, [
					m("span", null, w(e.name) + ":", 1),
					m("b", null, w(T(l)(M.value.style.chart.layout.roots.labels.formatter, e.total, T(s)({
						p: M.value.style.chart.layout.legend.prefix,
						v: e.total,
						s: M.value.style.chart.layout.legend.suffix,
						r: M.value.style.chart.layout.legend.roundingValue
					}), { datapoint: e })), 1),
					je(" (" + w(T(s)({
						v: e.total / L.value * 100,
						s: "%",
						r: M.value.style.chart.layout.legend.roundingPercentage
					})) + ") ", 1)
				], 64))], 4))), 256))])])], 8, _t)) : f("", !0),
				H.value && U.value ? (b(), p("g", St, [
					(b(), p("foreignObject", {
						x: 0,
						y: Tr(),
						height: F.value.height - I.value.bottom,
						width: F.value.width,
						style: { overflow: "visible" },
						onClick: t[7] ||= () => {
							K(), $(), k.value = !1;
						}
					}, [m("div", wt, [m("b", null, w(H.value.name), 1), m("div", Tt, [(b(!0), p(u, null, S(H.value.breakdown, (e, t) => (b(), p("div", { style: y(`display:flex;align-items:center;gap:6px;flex-direction:row;font-size:${M.value.style.chart.layout.legend.fontSize}px;`) }, [(b(), p("svg", Et, [m("circle", {
						cx: "10",
						cy: "10",
						r: "10",
						fill: e.color,
						stroke: "none"
					}, null, 8, Dt)])), m("span", null, [
						je(w(e.name) + ": ", 1),
						m("b", null, w(M.value.style.chart.layout.legend.prefix) + " " + w(e.value.toFixed(M.value.style.chart.layout.nuts.selected.labels.roundingValue)) + " " + w(M.value.style.chart.layout.legend.suffix), 1),
						je(" (" + w((e.proportionToBranch * 100).toFixed(M.value.style.chart.layout.nuts.selected.labels.roundingPercentage)) + "%)", 1)
					])], 4))), 256))])])], 8, Ct)),
					m("circle", {
						cx: H.value.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
						cy: H.value.y1 + F.value.branchSize / 2,
						r: 256,
						fill: `url(#nut_underlayer_${O.value})`,
						onClick: J,
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, null, 10, Ot),
					(b(!0), p(u, null, S(U.value, (e) => (b(), p("g", null, [Er(e) ? (b(), p("path", {
						key: 0,
						d: T(ne)(e, {
							x: H.value.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
							y: H.value.y1 + F.value.branchSize / 2
						}, 16, 16, !1, !1, 64),
						stroke: e.color,
						"stroke-width": "1",
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						fill: "none",
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, null, 10, kt)) : f("", !0)]))), 256)),
					m("circle", {
						cx: H.value.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
						cy: H.value.y1 + F.value.branchSize / 2,
						r: 118,
						fill: M.value.style.chart.backgroundColor,
						onClick: J,
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, null, 10, At),
					(b(!0), p(u, null, S(U.value, (e) => (b(), p("path", {
						d: e.path,
						stroke: e.color,
						"stroke-width": 64,
						fill: "none",
						onClick: J,
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, null, 10, jt))), 256)),
					m("circle", {
						cx: H.value.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
						cy: H.value.y1 + F.value.branchSize / 2,
						r: 110,
						fill: `url(#nutpick_${O.value})`,
						onClick: J,
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, null, 10, Mt),
					m("circle", {
						cx: H.value.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
						cy: H.value.y1 + F.value.branchSize / 2,
						r: 64,
						fill: M.value.style.chart.backgroundColor,
						onClick: J,
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, null, 10, Nt),
					m("text", {
						x: H.value.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
						y: H.value.y1 + 8,
						fill: M.value.style.chart.layout.nuts.selected.labels.core.total.color,
						"font-size": M.value.style.chart.layout.nuts.selected.labels.core.total.fontSize,
						"font-weight": M.value.style.chart.layout.nuts.selected.labels.core.total.bold ? "bold" : "normal",
						"text-anchor": "middle",
						onClick: J,
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, w(M.value.translations.total), 11, Pt),
					m("text", {
						x: H.value.x2 + 24 + M.value.style.chart.layout.nuts.offsetX,
						y: H.value.y1 + 36,
						fill: M.value.style.chart.layout.nuts.selected.labels.core.value.color,
						"font-size": M.value.style.chart.layout.nuts.selected.labels.core.value.fontSize,
						"font-weight": M.value.style.chart.layout.nuts.selected.labels.core.value.bold ? "bold" : "normal",
						"text-anchor": "middle",
						onClick: J,
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, w(T(l)(M.value.style.chart.layout.nuts.selected.labels.dataLabels.formatter, H.value.value, T(s)({
						p: M.value.style.chart.layout.nuts.selected.labels.core.value.prefix,
						v: H.value.value,
						s: M.value.style.chart.layout.nuts.selected.labels.core.value.suffix,
						r: M.value.style.chart.layout.nuts.selected.roundingValue
					}), { datapoint: H.value })), 11, Ft),
					(b(!0), p(u, null, S(U.value, (e, t) => (b(), p("g", null, [Er(e) ? (b(), p("text", {
						key: 0,
						x: T(c)(e).x,
						"text-anchor": T(c)(e).anchor,
						y: T(ae)(e) - M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize / 6,
						fill: e.color,
						"font-size": M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize / 2,
						style: y(`font-weight:${M.value.style.chart.layout.nuts.selected.labels.dataLabels.bold ? "bold" : ""}`),
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, " ⬤ ", 14, It)) : f("", !0), Er(e) ? (b(), p("text", {
						key: 1,
						x: T(c)(e, !0).x,
						"text-anchor": T(c)(e, !0).anchor,
						y: T(ae)(e),
						fill: M.value.style.chart.layout.nuts.selected.labels.dataLabels.color,
						"font-size": M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize,
						style: y(`font-weight:${M.value.style.chart.layout.nuts.selected.labels.dataLabels.bold ? "bold" : ""}`),
						class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
					}, w(H.value.breakdown[t].name), 15, Lt)) : f("", !0)]))), 256)),
					(b(!0), p(u, null, S(U.value, (e, t) => (b(), p("g", null, [
						Er(e) ? (b(), p("text", {
							key: 0,
							x: T(c)(e, !0).x,
							"text-anchor": T(c)(e).anchor,
							y: T(ae)(e) + M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize,
							fill: M.value.style.chart.layout.nuts.selected.labels.dataLabels.color,
							"font-size": M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize,
							style: y(`font-weight:${M.value.style.chart.layout.nuts.selected.labels.dataLabels.bold ? "bold" : ""}`),
							class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
						}, w(T(s)({
							v: H.value.breakdown[t].value / H.value.value * 100,
							s: "%",
							r: M.value.style.chart.layout.nuts.selected.labels.roundingPercentage
						})) + " " + w(M.value.translations.of) + " " + w(H.value.breakdown[t].branchName) + " " + w(T(l)(M.value.style.chart.layout.nuts.selected.labels.dataLabels.formatter, H.value.breakdown[t].value, T(s)({
							p: M.value.style.chart.layout.nuts.selected.labels.dataLabels.prefix,
							v: H.value.breakdown[t].value,
							s: M.value.style.chart.layout.nuts.selected.labels.dataLabels.suffix,
							r: M.value.style.chart.layout.nuts.selected.roundingValue
						}), {
							datapoint: U.value,
							seriesIndex: t
						})), 15, Rt)) : f("", !0),
						Er(e) ? (b(), p("text", {
							key: 1,
							x: T(c)(e, !0).x,
							"text-anchor": T(c)(e).anchor,
							y: T(ae)(e) + M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize * 2,
							fill: M.value.style.chart.layout.nuts.selected.labels.dataLabels.color,
							"font-size": M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize,
							style: y(`font-weight:${M.value.style.chart.layout.nuts.selected.labels.dataLabels.bold ? "bold" : ""}`),
							class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
						}, w(T(s)({
							v: H.value.breakdown[t].proportionToRoot * 100,
							s: "%",
							r: M.value.style.chart.layout.nuts.selected.labels.roundingPercentage
						})) + " " + w(M.value.translations.of) + " " + w(H.value.breakdown[t].rootName), 15, zt)) : f("", !0),
						Er(e) ? (b(), p("text", {
							key: 2,
							x: T(c)(e, !0).x,
							"text-anchor": T(c)(e).anchor,
							y: T(ae)(e) + M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize * 3,
							fill: M.value.style.chart.layout.nuts.selected.labels.dataLabels.color,
							"font-size": M.value.style.chart.layout.nuts.selected.labels.dataLabels.fontSize,
							style: y(`font-weight:${M.value.style.chart.layout.nuts.selected.labels.dataLabels.bold ? "bold" : ""}`),
							class: _(M.value.style.chart.layout.nuts.selected.useMotion ? "vue-ui-chestnut-animated" : "")
						}, w(T(s)({
							v: H.value.breakdown[t].proportionToTree * 100,
							s: "%",
							r: M.value.style.chart.layout.nuts.selected.labels.roundingPercentage
						})) + " " + w(M.value.translations.proportionToTree), 15, Bt)) : f("", !0)
					]))), 256))
				])) : f("", !0),
				(b(!0), p(u, null, S(B.value, (e) => (b(), p("g", null, [
					W.value && W.value.id === e.id && !H.value ? (b(), p("text", {
						key: 0,
						x: e.x1 + 6,
						y: e.y1 + F.value.branchSize + 24,
						"font-weight": "bold",
						"text-anchor": "start",
						"font-size": M.value.style.chart.layout.branches.labels.dataLabels.fontSize,
						fill: M.value.style.chart.layout.branches.labels.color,
						onClick: t[8] ||= () => {
							K(), $(), k.value = !1;
						}
					}, w(e.name) + ": " + w(T(l)(M.value.style.chart.layout.branches.labels.dataLabels.formatter, e.value, T(s)({
						p: M.value.style.chart.layout.branches.labels.dataLabels.prefix,
						v: e.value,
						s: M.value.style.chart.layout.branches.labels.dataLabels.suffix,
						r: M.value.style.chart.layout.branches.labels.dataLabels.roundingValue
					}), { datapoint: e })), 9, Vt)) : f("", !0),
					W.value && W.value.id === e.id && !H.value ? (b(), p("text", {
						key: 1,
						x: e.x1 + 6,
						y: e.y1 + F.value.branchSize + 48,
						"text-anchor": "start",
						"font-size": M.value.style.chart.layout.branches.labels.dataLabels.fontSize,
						fill: M.value.style.chart.layout.branches.labels.color,
						onClick: t[9] ||= () => {
							K(), $(), k.value = !1;
						}
					}, w(T(s)({
						v: e.proportionToRoot * 100,
						s: "%",
						r: M.value.style.chart.layout.branches.labels.dataLabels.roundingPercentage
					})) + " " + w(M.value.translations.of) + " " + w(e.rootName), 9, Ht)) : f("", !0),
					W.value && W.value.id === e.id && !H.value ? (b(), p("text", {
						key: 2,
						x: e.x1 + 6,
						y: e.y1 + F.value.branchSize + 72,
						"text-anchor": "start",
						"font-size": M.value.style.chart.layout.branches.labels.dataLabels.fontSize,
						fill: M.value.style.chart.layout.branches.labels.color,
						onClick: t[10] ||= () => {
							K(), $(), k.value = !1;
						}
					}, w(T(s)({
						v: e.value / L.value * 100,
						s: "%",
						r: M.value.style.chart.layout.branches.labels.dataLabels.roundingPercentage
					})) + " " + w(M.value.translations.proportionToTree), 9, Ut)) : f("", !0)
				]))), 256)),
				C(e.$slots, "svg", { svg: {
					...F.value,
					isPrintingImg: T(nr) || T(rr) || T(Wr),
					isPrintingSvg: T(Gr)
				} }, void 0, !0)
			], 46, qe)) : f("", !0)]),
			e.$slots.watermark ? (b(), p("div", Wt, [C(e.$slots, "watermark", v(g({ isPrinting: T(nr) || T(rr) || T(Wr) || T(Gr) })), void 0, !0)])) : f("", !0),
			C(e.$slots, "legend", { legend: R.value }, void 0, !0),
			e.$slots.source ? (b(), p("div", Gt, [C(e.$slots, "source", {}, void 0, !0)], 512)) : f("", !0),
			Hn.value && M.value.userOptions.buttons.table ? (b(), ke(Ie(Ir.value.component), Ne({ key: 6 }, Ir.value.props, {
				ref_key: "tableUnit",
				ref: Gn,
				onClose: Rr
			}), Ae({
				content: E(() => [m("div", {
					ref_key: "tableContainer",
					ref: cr,
					class: "vue-ui-chestnut-table",
					style: y(`${M.value.table.useDialog ? "" : "max-height: 300px;margin-top:24px"}`)
				}, [m("div", { style: y(`${M.value.table.useDialog ? "" : "padding-top:36px;"}position: relative`) }, [M.value.table.useDialog ? f("", !0) : (b(), p("div", {
					key: 0,
					role: "button",
					tabindex: "0",
					style: y(`width:32px; position: absolute; top: 0; left:4px; padding: 0 0px; display: flex; align-items:center;justify-content:center;height: 36px; width: 32px; cursor:${N.value ? "pointer" : "default"}; background:${M.value.table.th.backgroundColor};`),
					onClick: Rr,
					onKeypress: Be(Rr, ["enter"])
				}, [h(T(Nn), {
					name: "close",
					stroke: M.value.table.th.color,
					"stroke-width": 2
				}, null, 8, ["stroke"])], 36)), m("div", {
					style: { width: "100%" },
					class: _({ "vue-ui-responsive": T(Lr) })
				}, [m("table", Kt, [
					M.value.table.useDialog ? f("", !0) : (b(), p("caption", {
						key: 0,
						style: y({
							backgroundColor: M.value.table.th.backgroundColor,
							color: M.value.table.th.color,
							outline: M.value.table.th.outline
						}),
						class: "vue-ui-data-table__caption"
					}, [je(w(M.value.style.chart.layout.title.text) + " ", 1), M.value.style.chart.layout.title.subtitle.text ? (b(), p("span", qt, w(M.value.style.chart.layout.title.subtitle.text), 1)) : f("", !0)], 4)),
					m("thead", null, [m("tr", {
						role: "row",
						style: y(`background:${M.value.table.th.backgroundColor};color:${M.value.table.th.color}`)
					}, [(b(!0), p(u, null, S(Y.value.head, (e) => (b(), p("th", { style: y(`outline:${M.value.table.th.outline}`) }, w(e), 5))), 256))], 4)]),
					m("tbody", null, [(b(!0), p(u, null, S(Y.value.body, (e, t) => (b(), p("tr", {
						class: _({
							"vue-ui-data-table__tbody__row": !0,
							"vue-ui-data-table__tbody__row-even": t % 2 == 0,
							"vue-ui-data-table__tbody__row-odd": t % 2 != 0
						}),
						style: y(`background:${M.value.table.td.backgroundColor};color:${M.value.table.td.color}`)
					}, [
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[0]
						}, [m("div", Yt, [Y.value.body[t - 1] && Y.value.body[t - 1].rootName === e.rootName ? (b(), p("span", Xt)) : (b(), p("span", Zt, w(e.rootName), 1))])], 12, Jt),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[1]
						}, [m("div", $t, [Y.value.body[t - 1] && Y.value.body[t - 1].rootName === e.rootName ? (b(), p("span", en)) : (b(), p("span", tn, w(e.rootValue.toFixed(M.value.table.td.roundingValue)), 1))])], 12, Qt),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[2]
						}, [m("div", rn, [Y.value.body[t - 1] && Y.value.body[t - 1].rootName === e.rootName ? (b(), p("span", an)) : (b(), p("span", on, w((e.rootToTotal * 100).toFixed(M.value.table.td.roundingPercentage)) + "% ", 1))])], 12, nn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[3]
						}, [m("div", cn, [Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? (b(), p("span", ln)) : (b(), p("span", un, w(e.branchName), 1))])], 12, sn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[4]
						}, [m("div", fn, [Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? (b(), p("span", pn)) : (b(), p("span", mn, w(e.branchValue.toFixed(M.value.table.td.roundingValue)), 1))])], 12, dn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[5]
						}, [m("div", gn, [Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? (b(), p("span", _n)) : (b(), p("span", vn, w((e.branchToRoot * 100).toFixed(M.value.table.td.roundingPercentage)) + "% ", 1))])], 12, hn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[6]
						}, [m("div", bn, [Y.value.body[t - 1] && Y.value.body[t - 1].branchName === e.branchName ? (b(), p("span", xn)) : (b(), p("span", Sn, w((e.branchToTotal * 100).toFixed(M.value.table.td.roundingPercentage)) + "% ", 1))])], 12, yn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[7]
						}, [m("div", wn, w(e.nutName), 1)], 12, Cn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[8]
						}, [m("div", En, w(e.nutValue.toFixed(M.value.table.td.roundingValue)), 1)], 12, Tn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[9]
						}, [m("div", On, w((e.nutToBranch * 100).toFixed(M.value.table.td.roundingPercentage)) + "% ", 1)], 12, Dn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[10]
						}, [m("div", An, w((e.nutToRoot * 100).toFixed(M.value.table.td.roundingPercentage)) + "% ", 1)], 12, kn),
						m("td", {
							class: "vue-ui-data-table__tbody__td",
							style: y(`outline:${M.value.table.td.outline}`),
							"data-cell": Y.value.head[11]
						}, [m("div", Mn, w((e.nutToTotal * 100).toFixed(M.value.table.td.roundingPercentage)) + "% ", 1)], 12, jn)
					], 6))), 256))])
				])], 2)], 4)], 4)]),
				_: 2
			}, [M.value.table.useDialog ? {
				name: "title",
				fn: E(() => [je(w(Ir.value.title), 1)]),
				key: "0"
			} : void 0, M.value.table.useDialog ? {
				name: "actions",
				fn: E(() => [m("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[11] ||= (e) => Or(M.value.userOptions.callbacks.csv)
				}, [h(T(Nn), {
					name: "fileCsv",
					stroke: Ir.value.props.color
				}, null, 8, ["stroke"])])]),
				key: "1"
			} : void 0]), 1040)) : f("", !0),
			C(e.$slots, "skeleton", {}, () => [T(Yn) ? (b(), ke(_e, { key: 0 })) : f("", !0)], !0)
		], 46, He));
	}
}, [["__scopeId", "data-v-92e64679"]]);
//#endregion
export { Ve as n, Nn as t };
