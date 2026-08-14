import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Jt as r, Kt as i, Pt as a, R as o, Rt as s, S as ee, X as c, ct as te, i as ne, jt as re, pt as ie, q as ae, t as oe, tt as se, w as ce, xt as le } from "./lib-Bttd6u5E.js";
import { n as ue, t as de } from "./useHints-Dq_w2E8B.js";
import { t as fe } from "./useConfig-DlNpz6P8.js";
import { t as pe } from "./usePrinter-DN5bYhTG.js";
import { n as me, t as he } from "./BaseScanner-DZvpgOjM.js";
import { t as ge } from "./useNestedProp-vPNvh7rV.js";
import { t as _e } from "./useThemeCheck-C43Tcqmk.js";
import { t as ve } from "./useChartExport-DNiwdPmb.js";
import { t as ye } from "./useTransitions-g_zBREk2.js";
import { t as be } from "./img-Bnokohej.js";
import { n as xe } from "./Title-BE3qg9xl.js";
import { t as Se } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Ce, t as we } from "./useResponsive-ZtArZtUf.js";
import { t as Te } from "./BaseLegendToggle-DZVucLnv.js";
import { t as Ee } from "./A11yDataTable-DdRsVULz.js";
import { t as De } from "./useUserOptionState-DK-_1ddE.js";
import { t as Oe } from "./useChartAccessibility-DYqac8yF.js";
import { t as ke } from "./labelUtils-BeVpDvTJ.js";
import { t as Ae } from "./Legend-CQxUgOd-.js";
import { t as je } from "./vue_ui_galaxy-Ig0cc1_h.js";
import { Fragment as Me, Teleport as Ne, computed as l, createBlock as u, createCommentVNode as d, createElementBlock as f, createElementVNode as p, createSlots as Pe, createTextVNode as Fe, createVNode as Ie, defineAsyncComponent as m, guardReactiveProps as h, mergeProps as Le, nextTick as Re, normalizeClass as g, normalizeProps as _, normalizeStyle as v, onMounted as ze, openBlock as y, ref as b, renderList as Be, renderSlot as x, resolveDynamicComponent as Ve, toDisplayString as He, toRefs as Ue, unref as S, watch as We, withCtx as C } from "vue";
//#region src/components/vue-ui-galaxy.vue
var Ge = /* @__PURE__ */ e({ default: () => ut }), Ke = ["id"], qe = ["id"], Je = ["id"], Ye = { style: { position: "relative" } }, Xe = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Ze = ["width", "height"], Qe = ["id"], $e = ["stdDeviation"], et = [
	"d",
	"stroke",
	"stroke-width"
], tt = [
	"d",
	"stroke",
	"stroke-width"
], nt = ["filter"], rt = [
	"d",
	"stroke",
	"stroke-width"
], it = [
	"data-a11y-serie-id",
	"d",
	"stroke-width",
	"aria-label",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], at = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, ot = {
	key: 5,
	class: "vue-data-ui-watermark"
}, st = ["id"], ct = ["onClick"], lt = ["innerHTML"], ut = /*#__PURE__*/ Se({
	__name: "vue-ui-galaxy",
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
		"selectLegend",
		"selectDatapoint",
		"copyAlt"
	],
	setup(e, { expose: Se, emit: Ge }) {
		let ut = m(() => import("./Tooltip-DhjyfHwz.js")), dt = m(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), ft = m(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), pt = m(() => import("./DataTable-BbKgJ5UI.js")), mt = m(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), ht = m(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), gt = m(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), _t = m(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_galaxy: vt } = fe(), { isThemeValid: yt, warnInvalidTheme: bt } = _e(), w = e, T = b(ae()), E = b(null), D = b(!1), xt = b(""), O = b(null), St = b(0), Ct = b(0), wt = b(0), Tt = b(0), Et = b(null), Dt = b(null), Ot = b(null), kt = b(null), k = b(null), A = b(null), At = b(!1), j = b(null), jt = b(null), M = b(null), N = b(null), Mt = b({
			x: 0,
			y: 0
		}), Nt = b("pointer"), P = b(!1), Pt = l(() => !!w.dataset && w.dataset.length), F = b(Ht());
		ue({
			config: () => F.value,
			dataset: () => w.dataset,
			component: "VueUiGalaxy",
			rules: [
				de.singleSeries,
				de.emptyArray,
				{
					test: (e) => e.length > 6,
					message: [
						"👀 The number of series is > 6. Consider:",
						"",
						"▶️ Grouping small values dynamically into a single \"Other\" series.",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display.",
						"",
						"▶️ Using VueUiHorizontalBar instead to make the dataset breakdown easier to read."
					]
				}
			]
		});
		let { transitionEnabled: Ft } = ye({
			config: () => F.value.transitions,
			dataset: () => w.dataset
		}), I = l(() => F.value.userOptions.useCursorPointer), It = l(() => r({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				useCssAnimation: !1,
				style: { chart: {
					backgroundColor: "#99999930",
					legend: { backgroundColor: "transparent" }
				} }
			},
			userConfig: F.value.skeletonConfig ?? {}
		})), { loading: L, FINAL_DATASET: Lt } = me({
			...Ue(w),
			FINAL_CONFIG: F,
			prepareConfig: Ht,
			callback: () => {
				Promise.resolve().then(async () => {
					await Re(), B.value.showTable = F.value.table.show;
				});
			},
			skeletonDataset: w.config?.skeletonDataset ?? [
				{
					name: "_",
					values: [21],
					color: "#DBDBDB"
				},
				{
					name: "_",
					values: [13],
					color: "#C4C4C4"
				},
				{
					name: "_",
					values: [8],
					color: "#ADADAD"
				}
			],
			skeletonConfig: r({
				defaultConfig: F.value,
				userConfig: It.value
			})
		});
		ze(() => {
			At.value = !0, Rt();
		});
		let R = l(() => F.value.debug);
		function Rt() {
			if (re(w.dataset) ? se({
				componentName: "VueUiGalaxy",
				type: "dataset",
				debug: R.value
			}) : R.value && w.dataset.forEach((e, t) => {
				ie({
					datasetObject: e,
					requiredAttributes: ["name", "values"]
				}).forEach((e) => {
					se({
						componentName: "VueUiGalaxy",
						type: "datasetSerieAttribute",
						property: e,
						index: t
					});
				});
			}), F.value.responsive) {
				let e = Ce(() => {
					let { width: e, height: t } = we({
						chart: E.value,
						title: F.value.style.chart.title.text ? Et.value : null,
						legend: F.value.style.chart.legend.show ? Dt.value : null,
						noTitle: kt.value,
						source: Ot.value
					});
					requestAnimationFrame(() => {
						Yt.value = Math.max(.1, e), Xt.value = Math.max(.1, t - 12);
					});
				});
				k.value && (A.value && k.value.unobserve(A.value), k.value.disconnect()), k.value = new ResizeObserver(e), A.value = E.value.parentNode, k.value.observe(A.value);
			}
		}
		let { userOptionsVisible: zt, setUserOptionsVisibility: Bt, keepUserOptionState: Vt } = De({ config: F.value }), { svgRef: z } = Oe({ config: F.value.style.chart.title });
		function Ht() {
			let e = ge({
				userConfig: w.config,
				defaultConfig: vt
			}), t = e.theme;
			if (!t) return e;
			if (!yt.value(e)) return bt(e), e;
			let n = ge({
				userConfig: je[t] || w.config,
				defaultConfig: e
			}), r = ge({
				userConfig: w.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : i[t] || a
			};
		}
		We(() => w.config, (e) => {
			F.value = Ht(), zt.value = !F.value.userOptions.showOnChartHover, Rt(), Ct.value += 1, wt.value += 1, Tt.value += 1, B.value.dataLabels.show = F.value.style.chart.layout.labels.dataLabels.show, B.value.showTable = F.value.table.show, B.value.showTooltip = F.value.style.chart.tooltip.show;
		}, { deep: !0 });
		let { isPrinting: Ut, isImaging: Wt, generatePdf: Gt, generateImage: Kt } = pe({
			elementId: `galaxy_${T.value}`,
			fileName: F.value.style.chart.title.text || "vue-ui-galaxy",
			options: F.value.userOptions.print
		}), qt = l(() => F.value.userOptions.show && !F.value.style.chart.title.text), Jt = l(() => ce(F.value.customPalette)), B = b({
			dataLabels: { show: F.value.style.chart.layout.labels.dataLabels.show },
			showTable: F.value.table.show,
			showTooltip: F.value.style.chart.tooltip.show
		}), Yt = b(250), Xt = b(180), Zt = b(0), Qt = b(0), V = l(() => ({
			width: Yt.value,
			height: Xt.value,
			viewBox: `${Zt.value} ${Qt.value} ${Yt.value} ${Xt.value}`
		})), $t = Ge, H = b([]);
		function en() {
			$t("selectLegend", K.value.map((e) => ({
				name: e.name,
				color: e.color,
				value: e.value
			})));
		}
		function tn() {
			H.value.length ? H.value = [] : J.value.forEach((e) => {
				H.value.push(e.id);
			}), en();
		}
		function U(e) {
			H.value.includes(e.id) ? H.value = H.value.filter((t) => t !== e.id) : H.value.push(e.id), en();
		}
		function nn(e) {
			return W.value.length ? W.value.find((t) => t.name === e) || (R.value && console.warn(`VueUiGalaxy - Series name not found "${e}"`), null) : (R.value && console.warn("VueUiGalaxy - There are no series to show."), null);
		}
		function rn(e) {
			let t = nn(e);
			t !== null && H.value.includes(t.id) && U({ id: t.id });
		}
		function an(e) {
			let t = nn(e);
			t !== null && (H.value.includes(t.id) || U({ id: t.id }));
		}
		let W = l(() => Lt.value.map((e, t) => ({
			name: e.name,
			color: ee(e.color) || Jt.value[t] || a[t] || a[t % a.length],
			value: e.values ? s(e.values).reduce((e, t) => e + t, 0) : 0,
			absoluteValues: s(e.values),
			id: ae()
		})).sort((e, t) => t.value - e.value).map((e, t) => ({
			...e,
			absoluteIndex: t
		})));
		function on() {
			return W.value.map((e) => ({
				name: e.name,
				color: e.color,
				value: e.value
			}));
		}
		let G = l(() => W.value.filter((e) => !H.value.includes(e.id)).map((e) => e.value).reduce((e, t) => e + t, 0)), sn = b(190), cn = l(() => W.value.filter((e) => !H.value.includes(e.id))), ln = l(() => (F.value.style.chart.layout.arcs.strokeWidth + F.value.style.chart.layout.arcs.borderWidth) / 2 + (F.value.style.chart.layout.padding ?? 12)), un = l(() => o({
			maxPoints: sn.value,
			a: F.value.style.chart.layout.arcs.a ?? 6,
			b: F.value.style.chart.layout.arcs.b ?? 6,
			angleStep: F.value.style.chart.layout.arcs.angleStep ?? .07,
			startX: V.value.width / 2 + F.value.style.chart.layout.arcs.offsetX,
			startY: V.value.height / 2 + F.value.style.chart.layout.arcs.offsetY,
			boxWidth: V.value.width,
			boxHeight: V.value.height,
			padding: ln.value
		})), K = l(() => {
			let e = [];
			for (let t = 0; t < cn.value.length; t += 1) {
				let n = cn.value[t], r = n.value / G.value * sn.value + (t > 0 && e.length ? e[t - 1].points : 0);
				e.push({
					points: r,
					...n,
					seriesIndex: t,
					proportion: n.value / G.value,
					path: un.value(r)
				});
			}
			return e.filter((e) => !H.value.includes(e.id)).toSorted((e, t) => t.points - e.points);
		});
		function dn(e) {
			let t = Math.min(Yt.value, Xt.value), n = O.value === e.id && F.value.style.chart.layout.arcs.hoverEffect.show ? F.value.style.chart.layout.arcs.hoverEffect.multiplicator : 1, r = (F.value.style.chart.layout.arcs.strokeWidth + F.value.style.chart.layout.arcs.borderWidth) * n, i = F.value.style.chart.layout.arcs.strokeWidth * n, a = F.value.style.chart.layout.arcs.strokeWidth / 2 * n;
			return {
				border: r / 180 * t,
				path: i / 180 * t,
				blur: a / 180 * t
			};
		}
		let q = b(!1);
		function fn(e) {
			q.value = e, St.value += 1;
		}
		let pn = b(null);
		function mn() {
			D.value = !1, O.value = null, M.value = null, N.value = null;
		}
		function hn(e) {
			if (!z.value || !e) return;
			let t = z.value.querySelector(`[data-a11y-serie-id="${e}"]`);
			if (!t) return;
			let n = t.getBoundingClientRect();
			Mt.value = {
				x: n.left + n.width / 2,
				y: n.top + n.height / 2
			};
		}
		function gn(e) {
			F.value.events.datapointLeave && F.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: e.absoluteIndex
			}), (N.value !== e.id || Nt.value !== "keyboard") && (D.value = !1, O.value = null);
		}
		function _n(e) {
			$t("selectDatapoint", e), F.value.events.datapointClick && F.value.events.datapointClick({
				datapoint: e,
				seriesIndex: e.absoluteIndex
			});
		}
		function vn({ val: e, percentage: t, showVal: n, showPercentage: r }) {
			let i = F.value.style.chart.layout.labels.dataLabels;
			return ke({
				config: i,
				val: e,
				percentage: t,
				showVal: n,
				showPercentage: r
			});
		}
		function yn({ datapoint: e, _relativeIndex: t, seriesIndex: n, show: r = !1, triggerMode: i = "pointer", flatIndex: a = null }) {
			if (F.value.events.datapointEnter && F.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: e.absoluteIndex
			}), !B.value.showTooltip) return;
			Nt.value = i, M.value = a, N.value = e.id, pn.value = {
				datapoint: e,
				seriesIndex: n,
				series: W.value,
				config: F.value
			}, D.value = r, O.value = e.id;
			let o = "", s = F.value.style.chart.tooltip.customFormat;
			le(s) && te(() => s({
				seriesIndex: n,
				datapoint: e,
				series: W.value,
				config: F.value
			})) ? xt.value = s({
				seriesIndex: n,
				datapoint: e,
				series: W.value,
				config: F.value
			}) : (o += `<div style="width:100%;text-align:center;border-bottom:1px solid ${F.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${e.name}</div>`, o += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="none" fill="${e.color}"/></svg>`, o += `<b>${vn({
				showVal: F.value.style.chart.tooltip.showValue,
				showPercentage: F.value.style.chart.tooltip.showPercentage,
				val: ne(F.value.style.chart.layout.labels.dataLabels.formatter, e.value, c({
					p: F.value.style.chart.layout.labels.dataLabels.prefix,
					v: e.value,
					s: F.value.style.chart.layout.labels.dataLabels.suffix,
					r: F.value.style.chart.tooltip.roundingValue
				}), {
					datapoint: e,
					seriesIndex: n
				}),
				percentage: c({
					v: e.proportion * 100,
					s: "%",
					r: F.value.style.chart.tooltip.roundingPercentage
				})
			})}</b></div>`, xt.value = `<div>${o}</div>`), i === "keyboard" && Re(() => {
				hn(e.id);
			});
		}
		let J = l(() => W.value.map((e, t) => {
			let n = ne(F.value.style.chart.layout.labels.dataLabels.formatter, e.value, c({
				p: F.value.style.chart.layout.labels.dataLabels.prefix,
				v: e.value,
				s: F.value.style.chart.layout.labels.dataLabels.suffix,
				r: F.value.style.chart.legend.roundingValue
			}), {
				datapoint: e,
				index: t
			}), r = isNaN(e.value / G.value) || H.value.includes(e.id) ? "-" : c({
				v: e.value / G.value * 100,
				s: "%",
				r: F.value.style.chart.legend.roundingPercentage
			}), i = vn({
				showVal: F.value.style.chart.legend.showValue,
				showPercentage: F.value.style.chart.legend.showPercentage,
				val: n,
				percentage: r
			});
			return {
				...e,
				proportion: (e.value || 0) / Lt.value.map((e) => (e.values || []).reduce((e, t) => e + t, 0)).reduce((e, t) => e + t, 0),
				opacity: H.value.includes(e.id) ? .5 : 1,
				shape: e.shape || "circle",
				segregate: () => U(e),
				isSegregated: H.value.includes(e.id),
				display: `${e.name}${F.value.style.chart.legend.showPercentage || F.value.style.chart.legend.showValue ? ": " : ""}${i}`
			};
		})), bn = l(() => ({
			cy: "galaxy-div-legend",
			backgroundColor: F.value.style.chart.legend.backgroundColor,
			color: F.value.style.chart.legend.color,
			fontSize: F.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: F.value.style.chart.legend.bold ? "bold" : ""
		})), Y = l(() => ({
			head: K.value.map((e) => ({
				name: e.name,
				color: e.color
			})),
			body: K.value.map((e) => e.value)
		}));
		function xn(e = null) {
			Re(() => {
				let r = Y.value.head.map((e, t) => [
					[e.name],
					[Y.value.body[t]],
					[isNaN(Y.value.body[t] / G.value) ? "-" : Y.value.body[t] / G.value * 100]
				]), i = [
					[F.value.style.chart.title.text],
					[F.value.style.chart.title.subtitle.text],
					[
						[""],
						["val"],
						["%"]
					]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: F.value.style.chart.title.text || "vue-ui-galaxy"
				});
			});
		}
		let X = l(() => {
			let e = [
				" <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>",
				c({
					p: F.value.style.chart.layout.labels.dataLabels.prefix,
					v: G.value,
					s: F.value.style.chart.layout.labels.dataLabels.suffix,
					r: F.value.table.td.roundingValue
				}),
				"100%"
			], t = Y.value.head.map((e, t) => {
				let n = c({
					p: F.value.style.chart.layout.labels.dataLabels.prefix,
					v: Y.value.body[t],
					s: F.value.style.chart.layout.labels.dataLabels.suffix,
					r: F.value.table.td.roundingValue
				});
				return [
					{
						color: e.color,
						name: e.name
					},
					n,
					isNaN(Y.value.body[t] / G.value) ? "-" : c({
						v: Y.value.body[t] / G.value * 100,
						s: "%",
						r: F.value.table.td.roundingPercentage
					})
				];
			}), n = {
				th: {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					outline: F.value.table.th.outline
				},
				td: {
					backgroundColor: F.value.table.td.backgroundColor,
					color: F.value.table.td.color,
					outline: F.value.table.td.outline
				},
				breakpoint: F.value.table.responsiveBreakpoint
			};
			return {
				colNames: [
					F.value.table.columnNames.series,
					F.value.table.columnNames.value,
					F.value.table.columnNames.percentage
				],
				head: e,
				body: t,
				config: n
			};
		});
		function Sn() {
			B.value.showTable = !B.value.showTable;
		}
		function Cn() {
			B.value.showTooltip = !B.value.showTooltip;
		}
		let Z = b(!1);
		function wn() {
			Z.value = !Z.value;
		}
		async function Tn({ scale: e = 2 } = {}) {
			if (!E.value) return;
			let { width: t, height: n } = E.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await be({
				domElement: E.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: F.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Q = l(() => {
			let e = F.value.table.useDialog && !F.value.table.show, t = B.value.showTable;
			return {
				component: e ? _t : ft,
				title: `${F.value.style.chart.title.text}${F.value.style.chart.title.subtitle.text ? `: ${F.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					headerColor: F.value.table.th.color,
					headerBg: F.value.table.th.backgroundColor,
					isFullscreen: q.value,
					fullscreenParent: E.value,
					forcedWidth: Math.min(500, window.innerWidth * .8),
					isCursorPointer: I.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: F.value.style.chart.backgroundColor,
							color: F.value.style.chart.color
						},
						head: {
							backgroundColor: F.value.style.chart.backgroundColor,
							color: F.value.style.chart.color
						}
					}
				}
			};
		});
		We(() => B.value.showTable, (e) => {
			F.value.table.show || (e && F.value.table.useDialog && j.value ? j.value.open() : "close" in j.value && j.value.close());
		});
		function En() {
			B.value.showTable = !1, jt.value && jt.value.setTableIconState(!1);
		}
		let Dn = l(() => J.value.map((e) => ({
			...e,
			name: e.display
		}))), On = l(() => F.value.style.chart.backgroundColor), kn = l(() => F.value.style.chart.legend), An = l(() => F.value.style.chart.title), { isCallbackImaging: jn, isCallbackSvg: Mn, generateSvg: Nn, onGenerateImage: Pn } = ve({
			svg: z,
			title: An,
			legend: kn,
			legendItems: Dn,
			backgroundColor: On,
			getSvgCallback: () => F.value.userOptions.callbacks.svg,
			generateImage: Kt
		});
		async function Fn() {
			if ($t("copyAlt", {
				config: F.value,
				dataset: W.value
			}), !F.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(F.value.userOptions.callbacks.altCopy({
				config: F.value,
				dataset: W.value
			}));
		}
		function In() {
			M.value = null, N.value = null, P.value = !0;
		}
		function Ln() {
			mn(), P.value = !1;
		}
		function Rn(e) {
			if (!z.value || Z.value || document.activeElement !== z.value || !$.value.length) return;
			let t = e.key === "ArrowLeft" || e.key === "ArrowUp", n = e.key === "ArrowRight" || e.key === "ArrowDown", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				mn();
				return;
			}
			if (r) {
				if (M.value === null) return;
				let e = $.value[M.value];
				if (!e) return;
				_n(e.datapoint);
				return;
			}
			let a = M.value;
			a === null || a < 0 || a >= $.value.length ? a = n ? 0 : $.value.length - 1 : (a += n ? 1 : -1, a < 0 && (a = $.value.length - 1), a >= $.value.length && (a = 0));
			let o = $.value[a];
			o && yn({
				datapoint: o.datapoint,
				seriesIndex: o.datapoint.seriesIndex,
				show: !0,
				triggerMode: "keyboard",
				flatIndex: a
			});
		}
		let zn = l(() => ({
			headers: X.value?.colNames ?? [],
			rows: X.value?.body ?? []
		})), $ = l(() => K.value.map((e, t) => ({
			datapoint: e,
			index: t
		})));
		return Se({
			getData: on,
			getImage: Tn,
			generatePdf: Gt,
			generateCsv: xn,
			generateImage: Kt,
			generateSvg: Nn,
			hideSeries: an,
			showSeries: rn,
			toggleTable: Sn,
			toggleTooltip: Cn,
			toggleAnnotator: wn,
			toggleFullscreen: fn,
			copyAlt: Fn
		}), (e, t) => (y(), f("div", {
			ref_key: "galaxyChart",
			ref: E,
			class: g(`vue-data-ui-component vue-ui-galaxy ${q.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${F.value.useCssAnimation ? "" : "vue-ui-dna"} ${S(L) ? "loading" : ""}`),
			style: v(`font-family:${F.value.style.fontFamily};width:100%; text-align:center;${F.value.style.chart.title.text ? "" : "padding-top:36px"};background:${F.value.style.chart.backgroundColor}`),
			id: `galaxy_${T.value}`,
			onMouseenter: t[2] ||= () => S(Bt)(!0),
			onMouseleave: t[3] ||= () => {
				S(Bt)(!1), P.value || mn();
			}
		}, [
			p("div", {
				id: `chart-instructions-${T.value}`,
				class: "sr-only"
			}, [p("p", null, He(F.value.a11y.translations.keyboardNavigation), 1)], 8, qe),
			zn.value?.rows?.length ? (y(), u(Ee, {
				key: 0,
				uid: T.value,
				head: zn.value.headers,
				body: zn.value.rows,
				notice: F.value.a11y.translations.tableAvailable,
				caption: F.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : d("", !0),
			F.value.userOptions.buttons.annotator ? (y(), u(S(mt), {
				key: 1,
				svgRef: S(z),
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				active: Z.value,
				isCursorPointer: I.value,
				onClose: wn
			}, {
				"annotator-action-close": C(() => [x(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": C(({ color: t }) => [x(e.$slots, "annotator-action-color", _(h({ color: t })), void 0, !0)]),
				"annotator-action-draw": C(({ mode: t }) => [x(e.$slots, "annotator-action-draw", _(h({ mode: t })), void 0, !0)]),
				"annotator-action-undo": C(({ disabled: t }) => [x(e.$slots, "annotator-action-undo", _(h({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": C(({ disabled: t }) => [x(e.$slots, "annotator-action-redo", _(h({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": C(({ disabled: t }) => [x(e.$slots, "annotator-action-delete", _(h({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : d("", !0),
			qt.value ? (y(), f("div", {
				key: 2,
				ref_key: "noTitle",
				ref: kt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : d("", !0),
			F.value.style.chart.title.text ? (y(), f("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Et,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(y(), u(xe, {
				key: `title_${Ct.value}`,
				config: {
					title: {
						cy: "galaxy-div-title",
						...F.value.style.chart.title
					},
					subtitle: {
						cy: "galaxy-div-subtitle",
						...F.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : d("", !0),
			p("div", { id: `legend-top-${T.value}` }, null, 8, Je),
			F.value.userOptions.show && Pt.value && (S(Vt) || S(zt)) ? (y(), u(S(ht), {
				ref_key: "userOptionsRef",
				ref: jt,
				key: `user_option_${St.value}`,
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				isPrinting: S(Ut),
				isImaging: S(Wt),
				uid: T.value,
				hasTooltip: F.value.userOptions.buttons.tooltip && F.value.style.chart.tooltip.show,
				hasPdf: F.value.userOptions.buttons.pdf,
				hasXls: F.value.userOptions.buttons.csv,
				hasImg: F.value.userOptions.buttons.img,
				hasSvg: F.value.userOptions.buttons.svg,
				hasTable: F.value.userOptions.buttons.table,
				hasFullscreen: F.value.userOptions.buttons.fullscreen,
				hasAltCopy: F.value.userOptions.buttons.altCopy,
				isTooltip: B.value.showTooltip,
				isFullscreen: q.value,
				titles: { ...F.value.userOptions.buttonTitles },
				chartElement: E.value,
				position: F.value.userOptions.position,
				hasAnnotator: F.value.userOptions.buttons.annotator,
				isAnnotation: Z.value,
				callbacks: F.value.userOptions.callbacks,
				printScale: F.value.userOptions.print.scale,
				tableDialog: F.value.table.useDialog,
				isCursorPointer: I.value,
				onToggleFullscreen: fn,
				onGeneratePdf: S(Gt),
				onGenerateCsv: xn,
				onGenerateImage: S(Pn),
				onGenerateSvg: S(Nn),
				onToggleTable: Sn,
				onToggleTooltip: Cn,
				onToggleAnnotator: wn,
				onCopyAlt: Fn,
				style: v({ visibility: S(Vt) ? S(zt) ? "visible" : "hidden" : "visible" })
			}, Pe({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: C(({ isOpen: t, color: n }) => [x(e.$slots, "menuIcon", _(h({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: C(() => [x(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: C(() => [x(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: C(() => [x(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: C(() => [x(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: C(() => [x(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: C(() => [x(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: C(({ toggleFullscreen: t, isFullscreen: n }) => [x(e.$slots, "optionFullscreen", _(h({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: C(({ toggleAnnotator: t, isAnnotator: n }) => [x(e.$slots, "optionAnnotator", _(h({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: C(({ altCopy: t }) => [x(e.$slots, "optionAltCopy", _(h({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: C(() => [x(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: C(() => [x(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasFullscreen.hasAltCopy.isTooltip.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : d("", !0),
			p("div", Ye, [(y(), f("svg", {
				ref_key: "svgRef",
				ref: z,
				xmlns: S(oe),
				"aria-describedby": `chart-instructions-${T.value}`,
				class: g({
					"vue-data-ui-fullscreen--on": q.value,
					"vue-data-ui-fulscreen--off": !q.value
				}),
				viewBox: V.value.viewBox,
				style: v(`max-width:100%; overflow: visible; background:transparent;color:${F.value.style.chart.color}`),
				tabindex: "0",
				onFocus: In,
				onBlur: Ln,
				onKeydown: Rn
			}, [
				Ie(S(gt)),
				e.$slots["chart-background"] ? (y(), f("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: V.value.width,
					height: V.value.height,
					style: { pointerEvents: "none" }
				}, [x(e.$slots, "chart-background", {}, void 0, !0)], 8, Ze)) : d("", !0),
				p("defs", null, [p("filter", {
					id: `blur_${T.value}`,
					x: "-50%",
					y: "-50%",
					width: "200%",
					height: "200%"
				}, [p("feGaussianBlur", {
					in: "SourceGraphic",
					stdDeviation: 100 / F.value.style.chart.layout.arcs.gradient.intensity
				}, null, 8, $e)], 8, Qe)]),
				(y(!0), f(Me, null, Be(K.value, (e) => (y(), f("g", { key: `dp_${e.id}` }, [
					e.value ? (y(), f("path", {
						key: 0,
						d: e.path,
						fill: "none",
						stroke: F.value.style.chart.backgroundColor,
						"stroke-width": dn(e).border,
						class: g({ "vue-data-ui-transition": S(Ft) }),
						"stroke-linecap": "round"
					}, null, 10, et)) : d("", !0),
					e.value ? (y(), f("path", {
						key: 1,
						d: e.path,
						fill: "none",
						stroke: e.color,
						"stroke-width": dn(e).path,
						"stroke-linecap": "round",
						class: g({
							"vue-data-ui-transition": S(Ft),
							"vue-ui-galaxy-blur": O.value && O.value !== e.id && F.value.useBlurOnHover
						})
					}, null, 10, tt)) : d("", !0),
					e.value && F.value.style.chart.layout.arcs.gradient.show ? (y(), f("g", {
						key: 2,
						filter: `url(#blur_${T.value})`
					}, [p("path", {
						d: e.path,
						fill: "none",
						stroke: F.value.style.chart.layout.arcs.gradient.color,
						"stroke-width": dn(e).blur,
						"stroke-linecap": "round",
						class: g({
							"vue-ui-galaxy-gradient": !0,
							"vue-data-ui-transition": S(Ft),
							"vue-ui-galaxy-blur": O.value && O.value !== e.id && F.value.useBlurOnHover
						})
					}, null, 10, rt)], 8, nt)) : d("", !0)
				]))), 128)),
				(y(!0), f(Me, null, Be(K.value, (e, t) => (y(), f("g", null, [e.value ? (y(), f("path", {
					key: 0,
					"data-a11y-serie-id": e.id,
					d: e.path,
					fill: "none",
					stroke: "transparent",
					"stroke-width": F.value.style.chart.layout.arcs.strokeWidth + F.value.style.chart.layout.arcs.borderWidth,
					"stroke-linecap": "round",
					"aria-label": `${e.name}: ${S(c)({
						p: F.value.style.chart.layout.labels.dataLabels.prefix,
						v: e.value,
						s: F.value.style.chart.layout.labels.dataLabels.suffix,
						r: F.value.style.chart.tooltip.roundingValue
					})}`,
					onMouseenter: (n) => yn({
						datapoint: e,
						relativeIndex: t,
						seriesIndex: e.seriesIndex,
						show: !0,
						triggerMode: "pointer",
						flatIndex: t
					}),
					onMouseleave: (t) => gn(e),
					onClick: (t) => _n(e)
				}, null, 40, it)) : d("", !0)]))), 256)),
				x(e.$slots, "svg", { svg: {
					...V.value,
					isPrintingImg: S(Ut) || S(Wt) || S(jn),
					isPrintingSvg: S(Mn)
				} }, void 0, !0)
			], 46, Xe)), e.$slots.hint ? (y(), f("div", at, [x(e.$slots, "hint", _(h({
				hint: F.value.a11y.translations.keyboardNavigation,
				isVisible: P.value
			})), void 0, !0)])) : d("", !0)]),
			e.$slots.watermark ? (y(), f("div", ot, [x(e.$slots, "watermark", _(h({ isPrinting: S(Ut) || S(Wt) || S(jn) || S(Mn) })), void 0, !0)])) : d("", !0),
			p("div", { id: `legend-bottom-${T.value}` }, null, 8, st),
			At.value && (F.value.style.chart.legend.show || e.$slots.legend) ? (y(), u(Ne, {
				key: 6,
				to: F.value.style.chart.legend.position === "top" ? `#legend-top-${T.value}` : `#legend-bottom-${T.value}`
			}, [p("div", {
				ref_key: "chartLegend",
				ref: Dt
			}, [x(e.$slots, "legend", { legend: J.value }, () => [F.value.style.chart.legend.show ? (y(), u(Ae, {
				key: `legend_${Tt.value}`,
				legendSet: J.value,
				config: bn.value,
				onClickMarker: t[0] ||= ({ legend: e }) => U(e)
			}, {
				item: C(({ legend: e, index: t }) => [S(L) ? d("", !0) : (y(), f("div", {
					key: 0,
					onClick: (t) => U(e),
					style: v(`opacity:${H.value.includes(e.id) ? .5 : 1}`)
				}, He(e.display), 13, ct))]),
				legendToggle: C(() => [J.value.length > 2 && F.value.style.chart.legend.selectAllToggle.show && !S(L) ? (y(), u(Te, {
					key: 0,
					backgroundColor: F.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: F.value.style.chart.legend.selectAllToggle.color,
					fontSize: F.value.style.chart.legend.fontSize,
					checked: H.value.length > 0,
					onToggle: tn
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked"
				])) : d("", !0)]),
				_: 1
			}, 8, ["legendSet", "config"])) : d("", !0)], !0)], 512)], 8, ["to"])) : d("", !0),
			e.$slots.source ? (y(), f("div", {
				key: 7,
				ref_key: "source",
				ref: Ot,
				dir: "auto"
			}, [x(e.$slots, "source", {}, void 0, !0)], 512)) : d("", !0),
			Ie(S(ut), {
				teleportTo: F.value.style.chart.tooltip.teleportTo,
				show: B.value.showTooltip && D.value,
				backgroundColor: F.value.style.chart.tooltip.backgroundColor,
				color: F.value.style.chart.tooltip.color,
				borderRadius: F.value.style.chart.tooltip.borderRadius,
				borderColor: F.value.style.chart.tooltip.borderColor,
				borderWidth: F.value.style.chart.tooltip.borderWidth,
				fontSize: F.value.style.chart.tooltip.fontSize,
				backgroundOpacity: F.value.style.chart.tooltip.backgroundOpacity,
				position: F.value.style.chart.tooltip.position,
				offsetX: F.value.style.chart.tooltip.offsetX,
				offsetY: F.value.style.chart.tooltip.offsetY,
				parent: E.value,
				content: xt.value,
				isFullscreen: q.value,
				isCustom: S(le)(F.value.style.chart.tooltip.customFormat),
				smooth: F.value.style.chart.tooltip.smooth,
				backdropFilter: F.value.style.chart.tooltip.backdropFilter,
				smoothForce: F.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: F.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: Nt.value === "keyboard",
				a11yPosition: Mt.value
			}, {
				"tooltip-before": C(() => [x(e.$slots, "tooltip-before", _(h({ ...pn.value })), void 0, !0)]),
				tooltip: C(() => [x(e.$slots, "tooltip", _(h({ ...pn.value })), void 0, !0)]),
				"tooltip-after": C(() => [x(e.$slots, "tooltip-after", _(h({ ...pn.value })), void 0, !0)]),
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
			Pt.value && F.value.userOptions.buttons.table ? (y(), u(Ve(Q.value.component), Le({ key: 8 }, Q.value.props, {
				ref_key: "tableUnit",
				ref: j,
				onClose: En
			}), Pe({
				content: C(() => [(y(), u(S(pt), {
					key: `table_${wt.value}`,
					colNames: X.value.colNames,
					head: X.value.head,
					body: X.value.body,
					config: X.value.config,
					title: F.value.table.useDialog ? "" : Q.value.title,
					withCloseButton: !F.value.table.useDialog,
					isCursorPointer: I.value,
					onClose: En
				}, {
					th: C(({ th: e }) => [p("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, lt)]),
					td: C(({ td: e }) => [Fe(He(e.name || e), 1)]),
					_: 1
				}, 8, [
					"colNames",
					"head",
					"body",
					"config",
					"title",
					"withCloseButton",
					"isCursorPointer"
				]))]),
				_: 2
			}, [F.value.table.useDialog ? {
				name: "title",
				fn: C(() => [Fe(He(Q.value.title), 1)]),
				key: "0"
			} : void 0, F.value.table.useDialog ? {
				name: "actions",
				fn: C(() => [p("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => xn(F.value.userOptions.callbacks.csv),
					style: v({ cursor: I.value ? "pointer" : "default" })
				}, [Ie(S(dt), {
					name: "fileCsv",
					stroke: Q.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : d("", !0),
			x(e.$slots, "skeleton", {}, () => [S(L) ? (y(), u(he, { key: 0 })) : d("", !0)], !0)
		], 46, Ke));
	}
}, [["__scopeId", "data-v-29ce5588"]]);
//#endregion
export { Ge as n, ut as t };
