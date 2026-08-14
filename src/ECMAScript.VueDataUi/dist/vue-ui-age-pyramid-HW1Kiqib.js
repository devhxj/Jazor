import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Gt as i, Jt as a, Vt as o, X as s, b as c, ct as ee, i as l, jt as te, q as ne, t as re, tt as ie, xt as ae } from "./lib-Bttd6u5E.js";
import { n as oe, t as se } from "./useHints-Dq_w2E8B.js";
import { t as ce } from "./useConfig-DlNpz6P8.js";
import { t as le } from "./usePrinter-DN5bYhTG.js";
import { n as ue, t as de } from "./BaseScanner-DZvpgOjM.js";
import { t as fe } from "./useNestedProp-vPNvh7rV.js";
import { t as pe } from "./useThemeCheck-C43Tcqmk.js";
import { t as me } from "./useChartExport-DNiwdPmb.js";
import { t as he } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as ge } from "./img-Bnokohej.js";
import { n as _e } from "./Title-BE3qg9xl.js";
import { t as ve } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ye, t as be } from "./useResponsive-ZtArZtUf.js";
import { t as xe } from "./DefGrad-DVBqDjhO.js";
import { t as Se } from "./A11yDataTable-DdRsVULz.js";
import { t as Ce } from "./useUserOptionState-DK-_1ddE.js";
import { t as we } from "./useChartAccessibility-DYqac8yF.js";
import { t as Te } from "./vue_ui_age_pyramid-BY6c-oX_.js";
import { Fragment as u, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createSlots as Ee, createTextVNode as De, createVNode as g, defineAsyncComponent as _, guardReactiveProps as v, mergeProps as Oe, nextTick as ke, normalizeClass as Ae, normalizeProps as y, normalizeStyle as b, onBeforeUnmount as je, onMounted as Me, openBlock as x, ref as S, renderList as C, renderSlot as w, resolveDynamicComponent as Ne, shallowRef as Pe, toDisplayString as T, toRefs as Fe, unref as E, watch as Ie, watchEffect as Le, withCtx as D } from "vue";
//#region src/components/vue-ui-age-pyramid.vue
var Re = /* @__PURE__ */ e({ default: () => dt }), ze = ["id"], Be = ["id"], Ve = { style: { position: "relative" } }, He = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], Ue = [
	"x",
	"y",
	"width",
	"height"
], We = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], Ge = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], Ke = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], qe = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"rx"
], Je = { key: 0 }, Ye = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], Xe = [
	"x",
	"y",
	"fill",
	"font-size",
	"font-weight"
], Ze = { key: 1 }, Qe = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], $e = { key: 2 }, et = { key: 0 }, tt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], nt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], rt = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], it = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width"
], at = [
	"font-size",
	"fill",
	"text-anchor",
	"font-weight",
	"transform"
], ot = [
	"font-size",
	"fill",
	"text-anchor",
	"font-weight",
	"transform"
], st = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], ct = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"onMouseover",
	"onMouseleave",
	"onClick"
], lt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, ut = {
	key: 5,
	class: "vue-data-ui-watermark"
}, dt = /*#__PURE__*/ ve({
	__name: "vue-ui-age-pyramid",
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
	setup(e, { expose: ve, emit: Re }) {
		let dt = _(() => import("./Tooltip-DhjyfHwz.js")), ft = _(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), pt = _(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), mt = _(() => import("./DataTable-BbKgJ5UI.js")), ht = _(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), gt = _(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), _t = _(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), vt = _(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_age_pyramid: yt } = ce(), { isThemeValid: bt, warnInvalidTheme: xt } = pe(), O = e, St = Re, Ct = d(() => !!O.dataset && O.dataset.length), k = S(ne()), A = S(!1), wt = S(""), j = S(null), Tt = S(0), M = S(null), Et = S(null), Dt = S(null), Ot = S(null), kt = S(0), At = S(0), jt = S(null), N = S(null), Mt = S(null), P = S(null), Nt = S({
			x: 0,
			y: 0
		}), F = S("pointer"), Pt = S(!1), I = S(Ht());
		oe({
			config: () => I.value,
			dataset: () => O.dataset,
			component: "VueUiAgePyramid",
			rules: [se.emptyArray, {
				test: (e) => e.length > 130,
				message: [
					"👀 Dataset has > 130 points. Consider:",
					"",
					"▶️ Grouping small values into aggregated categories.",
					"",
					"▶️ Using VueUiStackline to display data in a more compact way."
				]
			}]
		});
		let L = d(() => I.value.userOptions.useCursorPointer), Ft = d(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				translations: {
					male: "",
					female: ""
				},
				style: {
					backgroundColor: "#99999930",
					layout: {
						bars: {
							left: { color: "#CACACA" },
							right: { color: "#999999" }
						},
						dataLabels: {
							xAxis: {
								fontSize: 0,
								scale: 1e3,
								translation: ""
							},
							yAxis: { show: !1 }
						},
						grid: { stroke: "#6A6A6A" }
					}
				}
			},
			userConfig: I.value.skeletonConfig ?? {}
		})), { loading: It, FINAL_DATASET: Lt, manualLoading: Rt } = ue({
			...Fe(O),
			FINAL_CONFIG: I,
			prepareConfig: Ht,
			skeletonDataset: O.config?.skeletonDataset ?? [
				[
					"_",
					9,
					2,
					2
				],
				[
					"_",
					8,
					3,
					3
				],
				[
					"_",
					7,
					5,
					5
				],
				[
					"_",
					6,
					8,
					8
				],
				[
					"_",
					5,
					13,
					13
				],
				[
					"_",
					4,
					21,
					21
				],
				[
					"_",
					3,
					34,
					34
				],
				[
					"_",
					2,
					55,
					55
				],
				[
					"_",
					1,
					89,
					89
				],
				[
					"_",
					0,
					144,
					144
				]
			],
			skeletonConfig: a({
				defaultConfig: I.value,
				userConfig: Ft.value
			})
		}), { userOptionsVisible: zt, setUserOptionsVisibility: Bt, keepUserOptionState: Vt } = Ce({ config: I.value }), { svgRef: R } = we({ config: I.value.style.title });
		function Ht() {
			let e = fe({
				userConfig: O.config,
				defaultConfig: yt
			}), t = e.theme;
			if (!t) return e;
			if (!bt.value(e)) return xt(e), e;
			let n = fe({
				userConfig: Te[t] || O.config,
				defaultConfig: e
			});
			return fe({
				userConfig: O.config,
				defaultConfig: n
			});
		}
		Ie(() => O.config, (e) => {
			It.value || (I.value = Ht()), zt.value = !I.value.userOptions.showOnChartHover, Wt(), kt.value += 1, At.value += 1, V.value.showTable = I.value.table.show, V.value.showTooltip = I.value.style.tooltip.show;
		}, { deep: !0 }), Ie(() => O.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Rt.value = !1);
		}, { deep: !0 });
		let z = Pe(null), B = Pe(null);
		Me(() => {
			Wt();
		});
		let Ut = d(() => I.value.debug);
		function Wt() {
			if (te(O.dataset) && (ie({
				componentName: "VueUiAgePyramid",
				type: "dataset",
				debug: Ut.value
			}), Rt.value = !0), te(O.dataset) || (Rt.value = I.value.loading), I.value.responsive) {
				let e = ye(() => {
					let { width: e, height: t } = be({
						chart: M.value,
						title: I.value.style.title.text ? Et.value : null,
						source: Dt.value,
						noTitle: Ot.value
					});
					requestAnimationFrame(() => {
						H.value.width = e, H.value.height = t;
					});
				});
				z.value && (B.value && z.value.unobserve(B.value), z.value.disconnect()), z.value = new ResizeObserver(e), B.value = M.value.parentNode, z.value.observe(B.value);
			}
		}
		je(() => {
			z.value && (B.value && z.value.unobserve(B.value), z.value.disconnect());
		});
		let { isPrinting: Gt, isImaging: Kt, generatePdf: qt, generateImage: Jt } = le({
			elementId: `vue-ui-age-pyramid_${k.value}`,
			fileName: I.value.style.title.text || "vue-ui-age-pyramid",
			options: I.value.userOptions.print
		}), Yt = d(() => I.value.userOptions.show && !I.value.style.title.text), V = S({
			showTable: I.value.table.show,
			showTooltip: I.value.style.tooltip.show
		});
		Ie(I, () => {
			V.value = {
				showTable: I.value.table.show,
				showTooltip: I.value.style.tooltip.show
			};
		}, { immediate: !0 });
		let H = S({
			height: I.value.style.height,
			width: I.value.style.width
		}), Xt = d(() => H.value.width), Zt = d(() => H.value.height), U = S(0), Qt = ye((e) => {
			U.value = e;
		}, 100);
		Le((e) => {
			let t = jt.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				Qt(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), je(() => {
			U.value = 0;
		});
		let W = d(() => {
			let e = H.value.width - I.value.style.layout.padding.right - I.value.style.layout.padding.left, t = I.value.style.layout.padding.left, n = H.value.width - I.value.style.layout.padding.right;
			return {
				top: I.value.style.layout.padding.top + I.value.style.layout.dataLabels.sideTitles.fontSize + I.value.style.layout.dataLabels.sideTitles.offsetY + 12,
				left: t,
				right: n,
				bottom: H.value.height - I.value.style.layout.padding.bottom - U.value,
				width: e,
				height: H.value.height - I.value.style.layout.padding.top - I.value.style.layout.padding.bottom - U.value - I.value.style.layout.dataLabels.sideTitles.fontSize - I.value.style.layout.dataLabels.sideTitles.offsetY - 12,
				centerX: I.value.style.layout.padding.left + e / 2,
				leftChart: {
					width: e / 2 - I.value.style.layout.centerSlit.width,
					right: t + e / 2 - I.value.style.layout.centerSlit.width
				},
				rightChart: {
					width: e / 2 - I.value.style.layout.centerSlit.width,
					left: t + e / 2 + I.value.style.layout.centerSlit.width
				}
			};
		}), $t = d(() => Lt.value.map((e) => I.value.style.layout.dataLabels.yAxis.display === "age" ? e[1] : e[0])), G = d(() => {
			let e = en(K.value / 5), t = [], n = [];
			for (let r = 0; r <= 5; r += 1) {
				let i = e * r, a = e * (r - 5);
				t.push({
					value: i,
					x: W.value.left + W.value.width / 2 + I.value.style.layout.centerSlit.width + i / K.value * W.value.leftChart.width
				}), n.push({
					value: Math.abs(a),
					x: W.value.left + W.value.width / 2 + a / K.value * W.value.leftChart.width - I.value.style.layout.centerSlit.width
				});
			}
			return {
				right: t,
				left: n
			};
		});
		function en(e) {
			if (e === 0) return 0;
			let t = 10 ** Math.floor(Math.log10(Math.abs(e))), n;
			return n = Math.round(e / t) * t, n;
		}
		let K = d(() => Math.max(...Lt.value.flatMap((e) => e.slice(-2).map((e) => c(e))))), q = d(() => Lt.value.length), tn = d(() => Lt.value.map((e) => ({
			segment: e[0],
			age: e[1],
			left: {
				value: e[2],
				proportionToMax: e[2] / K.value
			},
			right: {
				value: e[3],
				proportionToMax: e[3] / K.value
			}
		}))), J = d(() => tn.value.map((e, t) => {
			let n = W.value.top + W.value.height / q.value * t, r = W.value.height / q.value - I.value.style.layout.bars.gap;
			return {
				segment: e.segment,
				age: e.age,
				left: {
					...e.left,
					y: n,
					color: I.value.style.layout.bars.left.color,
					x: W.value.leftChart.right - e.left.proportionToMax * W.value.leftChart.width,
					width: c(e.left.proportionToMax * W.value.leftChart.width),
					height: r
				},
				right: {
					...e.right,
					y: n,
					color: I.value.style.layout.bars.right.color,
					x: W.value.rightChart.left,
					width: c(e.right.proportionToMax * W.value.rightChart.width),
					height: r
				}
			};
		})), Y = S(null);
		function nn(e) {
			let [t, n, r, i] = e;
			return {
				segment: t,
				index: n,
				left: r,
				right: i
			};
		}
		function rn(e, t) {
			I.value.events.datapointClick && I.value.events.datapointClick({
				datapoint: nn(t),
				seriesIndex: e
			});
		}
		function an(e, t) {
			I.value.events.datapointLeave && I.value.events.datapointLeave({
				datapoint: nn(t),
				seriesIndex: e
			}), (F.value !== "keyboard" || P.value !== e) && (j.value = null, A.value = !1);
		}
		function on(e) {
			if (!R.value) return;
			let t = J.value[e];
			if (!t) return;
			let n = i(W.value.left + W.value.width / 2, t.left.y + t.left.height / 2, R.value);
			n && (Nt.value = n);
		}
		function sn(e, t, n = "pointer") {
			I.value.events.datapointEnter && I.value.events.datapointEnter({
				datapoint: nn(t),
				seriesIndex: e
			}), j.value = e, P.value = e, F.value = n, Y.value = {
				datapoint: t,
				seriesIndex: e,
				series: J.value,
				config: I.value
			};
			let r = I.value.style.tooltip.customFormat;
			if (ae(r) && ee(() => r({
				seriesIndex: e,
				datapoint: {
					segment: t[0],
					index: t[1],
					left: t[2],
					right: t[3]
				},
				series: J.value,
				config: I.value
			}))) wt.value = r({
				seriesIndex: e,
				datapoint: {
					segment: t[0],
					index: t[1],
					left: t[2],
					right: t[3]
				},
				series: J.value,
				config: I.value
			});
			else {
				let n = "", r = J.value[e];
				n += `<div><b>${r.segment}</b></div>`, n += `<div>${I.value.translations.age}: ${l(I.value.style.layout.dataLabels.yAxis.formatter, c(r.age), s({ v: c(r.age) }), {
					datapoint: t,
					seriesIndex: e
				})}</div>`, n += `<div style="margin-top:6px;padding-top:6px;border-top:1px solid ${I.value.style.tooltip.borderColor}">`, n += "<div style=\"display:flex; flex-direction:row;gap:12px\">", n += `<div style="display:flex;flex-direction:column;align-items:center;justify-content:center"><svg viewBox="0 0 12 12" height="12" width="12"><rect stroke="none" x="0" y="0" height="12" width="12" rx="2" fill="${I.value.style.layout.bars.gradient.underlayer}"/><rect stroke="none" x="0" y="0" height="12" width="12" rx="2" fill="${I.value.style.layout.bars.gradient.show ? `url(#age_pyramid_left_${k.value})` : I.value.style.layout.bars.left.color}"/></svg><div>${I.value.translations.female}</div><div><b>${l(I.value.style.layout.dataLabels.xAxis.formatter, c(r.left.value), s({ v: c(r.left.value) }), {
					datapoint: t,
					seriesIndex: e
				})}</b></div></div>`, n += `<div style="display:flex;flex-direction:column;align-items:center;justify-content:center"><svg viewBox="0 0 12 12" height="12" width="12"><rect stroke="none" x="0" y="0" height="12" width="12" rx="2" fill="${I.value.style.layout.bars.gradient.underlayer}"/><rect stroke="none" x="0" y="0" height="12" width="12" rx="2" fill="${I.value.style.layout.bars.gradient.show ? `url(#age_pyramid_right_${k.value})` : I.value.style.layout.bars.right.color}"/></svg><div>${I.value.translations.male}</div><div><b>${l(I.value.style.layout.dataLabels.xAxis.formatter, c(r.right.value), s({ v: c(r.right.value) }), {
					datapoint: t,
					seriesIndex: e
				})}</b></div></div>`, n += "</div>", n += `<div style="margin-top:6px;padding-top:6px;border-top:1px solid ${I.value.style.tooltip.borderColor}"><div>${I.value.translations.total}</div><div><b>${l(I.value.style.layout.dataLabels.xAxis.formatter, c(r.right.value) + c(r.left.value), s({ v: c(r.right.value) + c(r.left.value) }), {
					datapoint: t,
					seriesIndex: e
				})}</b></div></div>`, n += "</div>", wt.value = `<div>${n}</div>`;
			}
			A.value = !0, n === "keyboard" && ke(() => {
				on(e);
			});
		}
		function cn(e = null) {
			ke(() => {
				let n = [
					I.value.translations.year,
					I.value.translations.age,
					I.value.translations.female,
					I.value.translations.male,
					I.value.translations.total
				], i = O.dataset.map((e) => [
					e[0],
					e[1],
					e[2],
					e[3],
					e[2] ?? 0 + e[3] ?? 0
				]), a = [
					[I.value.style.title.text],
					[I.value.style.title.subtitle.text],
					[
						[""],
						[""],
						[""]
					],
					n
				].concat(i), o = r(a);
				e ? e(o) : t({
					csvContent: o,
					title: I.value.style.title.text || "vue-ui-heatmap"
				});
			});
		}
		let X = d(() => {
			let e = [
				I.value.translations.year,
				I.value.translations.age,
				I.value.translations.female,
				I.value.translations.male,
				I.value.translations.total
			];
			return {
				head: e,
				body: O.dataset.map((e) => [
					e[0],
					e[1],
					e[2] == null ? "" : e[2].toLocaleString(),
					e[3] == null ? "" : e[3].toLocaleString(),
					(e[2] ?? 0 + e[3] ?? 0).toLocaleString()
				]),
				config: {
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
				},
				colNames: e
			};
		}), Z = S(!1);
		function ln(e) {
			Z.value = e, Tt.value += 1;
		}
		function un() {
			V.value.showTable = !V.value.showTable;
		}
		function dn() {
			V.value.showTooltip = !V.value.showTooltip;
		}
		let Q = S(!1);
		function fn() {
			Q.value = !Q.value;
		}
		async function pn({ scale: e = 2 } = {}) {
			if (!M.value) return;
			let { width: t, height: n } = M.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ge({
				domElement: M.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: I.value.style.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let mn = S([]), hn = S({
			start: 0,
			end: J.value.length
		});
		he({
			timeLabelsEls: jt,
			timeLabels: mn,
			slicer: hn,
			configRef: I,
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
			width: Xt,
			height: Zt,
			targetClass: ".vue-ui-age-pyramid-x-axis-label",
			rotation: I.value.style.layout.dataLabels.xAxis.autoRotate.angle
		});
		let $ = d(() => {
			let e = I.value.table.useDialog && !I.value.table.show, t = V.value.showTable;
			return {
				component: e ? vt : pt,
				title: `${I.value.style.title.text}${I.value.style.title.subtitle.text ? `: ${I.value.style.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: I.value.table.th.backgroundColor,
					color: I.value.table.th.color,
					headerColor: I.value.table.th.color,
					headerBg: I.value.table.th.backgroundColor,
					isFullscreen: Z.value,
					fullscreenParent: M.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: L.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: I.value.style.backgroundColor,
							color: I.value.style.color
						},
						head: {
							backgroundColor: I.value.style.backgroundColor,
							color: I.value.style.color
						}
					}
				}
			};
		});
		Ie(() => V.value.showTable, (e) => {
			I.value.table.show || (e && I.value.table.useDialog && N.value ? N.value.open() : "close" in N.value && N.value.close());
		});
		function gn() {
			V.value.showTable = !1, Mt.value && Mt.value.setTableIconState(!1);
		}
		let _n = d(() => I.value.style.backgroundColor), vn = d(() => I.value.style.title), { isCallbackImaging: yn, isCallbackSvg: bn, generateSvg: xn, onGenerateImage: Sn } = me({
			svg: R,
			title: vn,
			legend: null,
			legendItems: null,
			backgroundColor: _n,
			getSvgCallback: () => I.value.userOptions.callbacks.svg,
			generateImage: Jt
		});
		async function Cn() {
			if (St("copyAlt", {
				config: I.value,
				dataset: J.value
			}), !I.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(I.value.userOptions.callbacks.altCopy({
				config: I.value,
				dataset: J.value
			}));
		}
		function wn() {
			Pt.value = !0;
		}
		function Tn() {
			j.value = null, P.value = null, F.value = "pointer", A.value = !1, Pt.value = !1;
		}
		function En(e) {
			if (!R.value || Q.value || document.activeElement !== R.value || !J.value.length) return;
			let t = ["ArrowUp", "ArrowLeft"].includes(e.key), n = ["ArrowDown", "ArrowRight"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				j.value = null, P.value = null, F.value = "pointer", A.value = !1;
				return;
			}
			if (r) {
				if (P.value === null) return;
				let e = O.dataset[P.value];
				if (!e) return;
				rn(P.value, e);
				return;
			}
			let a = P.value, o = j.value, s = a !== null && a >= 0 && a < J.value.length, c = o !== null && o >= 0 && o < J.value.length;
			s ? t ? a = a - 1 < 0 ? J.value.length - 1 : a - 1 : n && (a = a + 1 > J.value.length - 1 ? 0 : a + 1) : c ? (a = n ? o + 1 : o - 1, a >= J.value.length && (a = 0), a < 0 && (a = J.value.length - 1)) : a = n ? 0 : J.value.length - 1;
			let ee = O.dataset[a];
			ee && sn(a, ee, "keyboard");
		}
		return ve({
			getImage: pn,
			generatePdf: qt,
			generateCsv: cn,
			generateImage: Jt,
			generateSvg: xn,
			toggleTable: un,
			toggleTooltip: dn,
			toggleAnnotator: fn,
			toggleFullscreen: ln,
			copyAlt: Cn
		}), (t, r) => (x(), m("div", {
			class: Ae(`vue-data-ui-component vue-ui-age-pyramid ${Z.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			ref_key: "agePyramid",
			ref: M,
			id: `vue-ui-age-pyramid_${k.value}`,
			style: b(`font-family:${I.value.style.fontFamily};width:100%; text-align:center;background:${I.value.style.backgroundColor};${I.value.responsive ? "height:100%" : ""}`),
			onMouseenter: r[1] ||= () => E(Bt)(!0),
			onMouseleave: r[2] ||= () => E(Bt)(!1)
		}, [
			h("div", {
				id: `chart-instructions-${k.value}`,
				class: "sr-only"
			}, [h("p", null, T(I.value.a11y.translations.keyboardNavigation), 1)], 8, Be),
			X.value?.body?.length ? (x(), f(Se, {
				key: 0,
				uid: k.value,
				head: X.value.head,
				body: X.value.body,
				notice: I.value.a11y.translations.tableAvailable,
				caption: I.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : p("", !0),
			I.value.userOptions.buttons.annotator ? (x(), f(E(ht), {
				key: 1,
				svgRef: E(R),
				backgroundColor: I.value.style.backgroundColor,
				color: I.value.style.color,
				active: Q.value,
				isCursorPointer: L.value,
				onClose: fn
			}, {
				"annotator-action-close": D(() => [w(t.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": D(({ color: e }) => [w(t.$slots, "annotator-action-color", y(v({ color: e })), void 0, !0)]),
				"annotator-action-draw": D(({ mode: e }) => [w(t.$slots, "annotator-action-draw", y(v({ mode: e })), void 0, !0)]),
				"annotator-action-undo": D(({ disabled: e }) => [w(t.$slots, "annotator-action-undo", y(v({ disabled: e })), void 0, !0)]),
				"annotator-action-redo": D(({ disabled: e }) => [w(t.$slots, "annotator-action-redo", y(v({ disabled: e })), void 0, !0)]),
				"annotator-action-delete": D(({ disabled: e }) => [w(t.$slots, "annotator-action-delete", y(v({ disabled: e })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : p("", !0),
			Yt.value ? (x(), m("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Ot,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : p("", !0),
			I.value.style.title.text ? (x(), m("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Et,
				style: "width:100%;background:transparent"
			}, [(x(), f(_e, {
				key: `title_${kt.value}`,
				config: {
					title: {
						cy: "pyramid-div-title",
						...I.value.style.title
					},
					subtitle: {
						cy: "pyramid-div-subtitle",
						...I.value.style.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : p("", !0),
			I.value.userOptions.show && Ct.value && (E(Vt) || E(zt)) ? (x(), f(E(gt), {
				ref_key: "userOptionsRef",
				ref: Mt,
				key: `user_options_${Tt.value}`,
				backgroundColor: I.value.style.backgroundColor,
				color: I.value.style.color,
				isImaging: E(Kt),
				isPrinting: E(Gt),
				uid: k.value,
				hasTooltip: I.value.userOptions.buttons.tooltip && I.value.style.tooltip.show,
				hasPdf: I.value.userOptions.buttons.pdf,
				hasXls: I.value.userOptions.buttons.csv,
				hasImg: I.value.userOptions.buttons.img,
				hasSvg: I.value.userOptions.buttons.svg,
				hasTable: I.value.userOptions.buttons.table,
				hasFullscreen: I.value.userOptions.buttons.fullscreen,
				hasAltCopy: I.value.userOptions.buttons.altCopy,
				isFullscreen: Z.value,
				isTooltip: V.value.showTooltip,
				titles: { ...I.value.userOptions.buttonTitles },
				chartElement: M.value,
				position: I.value.userOptions.position,
				hasAnnotator: I.value.userOptions.buttons.annotator,
				isAnnotation: Q.value,
				callbacks: I.value.userOptions.callbacks,
				printScale: I.value.userOptions.print.scale,
				tableDialog: I.value.table.useDialog,
				isCursorPointer: L.value,
				onToggleFullscreen: ln,
				onGeneratePdf: E(qt),
				onGenerateCsv: cn,
				onGenerateImage: E(Sn),
				onGenerateSvg: E(xn),
				onToggleTable: un,
				onToggleTooltip: dn,
				onToggleAnnotator: fn,
				onCopyAlt: Cn,
				style: b({ visibility: E(Vt) ? E(zt) ? "visible" : "hidden" : "visible" })
			}, Ee({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: D(({ isOpen: e, color: n }) => [w(t.$slots, "menuIcon", y(v({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: D(() => [w(t.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: D(() => [w(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: D(() => [w(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: D(() => [w(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionSvg ? {
					name: "optionSvg",
					fn: D(() => [w(t.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionTable ? {
					name: "optionTable",
					fn: D(() => [w(t.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: D(({ toggleFullscreen: e, isFullscreen: n }) => [w(t.$slots, "optionFullscreen", y(v({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: D(({ toggleAnnotator: e, isAnnotator: n }) => [w(t.$slots, "optionAnnotator", y(v({
						toggleAnnotator: e,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: D(({ altCopy: e }) => [w(t.$slots, "optionAltCopy", y(v({ altCopy: e })), void 0, !0)]),
					key: "9"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: D(() => [w(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: D(() => [w(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isImaging.isPrinting.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : p("", !0),
			h("div", Ve, [(x(), m("svg", {
				ref_key: "svgRef",
				ref: R,
				xmlns: E(re),
				class: Ae({
					"vue-data-ui-fullscreen--on": Z.value,
					"vue-data-ui-fulscreen--off": !Z.value
				}),
				viewBox: `0 0 ${H.value.width <= 0 ? 10 : H.value.width} ${H.value.height <= 0 ? 10 : H.value.height}`,
				style: b(`max-width:100%;overflow:visible;background:transparent;color:${I.value.style.color}`),
				tabindex: "0",
				"aria-describedby": `chart-instructions-${k.value}`,
				onFocus: wn,
				onBlur: Tn,
				onKeydown: En
			}, [
				g(E(_t)),
				t.$slots["chart-background"] ? (x(), m("foreignObject", {
					key: 0,
					x: W.value.left,
					y: W.value.top,
					width: Math.max(.1, W.value.width),
					height: Math.max(.1, W.value.height),
					style: { pointerEvents: "none" }
				}, [w(t.$slots, "chart-background", {}, void 0, !0)], 8, Ue)) : p("", !0),
				h("defs", null, [g(xe, {
					t: "linear",
					id: `age_pyramid_left_${k.value}`,
					x1: "0%",
					y1: "0%",
					x2: "100%",
					y2: "0%",
					stops: [[
						"0%",
						I.value.style.layout.bars.left.color,
						1
					], [
						"100%",
						E(n)(E(o)(I.value.style.layout.bars.left.color, I.value.style.layout.bars.gradient.shiftHue), 100 - I.value.style.layout.bars.gradient.intensity),
						1
					]]
				}, null, 8, ["id", "stops"]), g(xe, {
					t: "linear",
					id: `age_pyramid_right_${k.value}`,
					x1: "0%",
					y1: "0%",
					x2: "100%",
					y2: "0%",
					stops: [[
						"0%",
						E(n)(E(o)(I.value.style.layout.bars.right.color, I.value.style.layout.bars.gradient.shiftHue), 100 - I.value.style.layout.bars.gradient.intensity),
						1
					], [
						"100%",
						I.value.style.layout.bars.right.color,
						1
					]]
				}, null, 8, ["id", "stops"])]),
				(x(!0), m(u, null, C(J.value, (e, t) => (x(), m("g", null, [
					h("rect", {
						x: e.left.x,
						y: e.left.y,
						width: E(c)(e.left.width <= 0 ? 1e-4 : e.left.width),
						height: e.left.height <= 0 ? 1e-4 : e.left.height,
						fill: I.value.style.layout.bars.gradient.underlayer,
						rx: I.value.style.layout.bars.borderRadius
					}, null, 8, We),
					h("rect", {
						x: e.left.x,
						y: e.left.y,
						width: e.left.width <= 0 ? 1e-4 : e.left.width,
						height: e.left.height <= 0 ? 1e-4 : e.left.height,
						fill: I.value.style.layout.bars.gradient.show ? `url(#age_pyramid_left_${k.value})` : e.left.color,
						rx: I.value.style.layout.bars.borderRadius
					}, null, 8, Ge),
					h("rect", {
						x: e.right.x,
						y: e.right.y,
						width: e.right.width <= 0 ? 1e-4 : e.right.width,
						height: e.right.height <= 0 ? 1e-4 : e.right.height,
						fill: I.value.style.layout.bars.gradient.underlayer,
						rx: I.value.style.layout.bars.borderRadius
					}, null, 8, Ke),
					h("rect", {
						x: e.right.x,
						y: e.right.y,
						width: e.right.width <= 0 ? 1e-4 : e.right.width,
						height: e.right.height <= 0 ? 1e-4 : e.right.height,
						fill: I.value.style.layout.bars.gradient.show ? `url(#age_pyramid_right_${k.value})` : e.right.color,
						rx: I.value.style.layout.bars.borderRadius
					}, null, 8, qe)
				]))), 256)),
				h("g", null, [
					I.value.style.layout.dataLabels.sideTitles.show ? (x(), m("g", Je, [h("text", {
						x: W.value.left,
						y: I.value.style.layout.dataLabels.sideTitles.fontSize,
						fill: I.value.style.layout.dataLabels.sideTitles.useSideColor ? I.value.style.layout.bars.left.color : I.value.style.layout.dataLabels.sideTitles.color,
						"font-size": I.value.style.layout.dataLabels.sideTitles.fontSize,
						"text-anchor": "start",
						"font-weight": I.value.style.layout.dataLabels.sideTitles.bold ? "bold" : "normal"
					}, T(I.value.translations.female), 9, Ye), h("text", {
						x: W.value.right,
						y: I.value.style.layout.dataLabels.sideTitles.fontSize,
						fill: I.value.style.layout.dataLabels.sideTitles.useSideColor ? I.value.style.layout.bars.right.color : I.value.style.layout.dataLabels.sideTitles.color,
						"font-size": I.value.style.layout.dataLabels.sideTitles.fontSize,
						"text-anchor": "end",
						"font-weight": I.value.style.layout.dataLabels.sideTitles.bold ? "bold" : "normal"
					}, T(I.value.translations.male), 9, Xe)])) : p("", !0),
					I.value.style.layout.dataLabels.yAxis.show ? (x(), m("g", Ze, [(x(!0), m(u, null, C($t.value, (e, t) => (x(), m(u, null, [t % I.value.style.layout.dataLabels.yAxis.showEvery === 0 ? (x(), m("text", {
						key: 0,
						x: W.value.centerX,
						y: W.value.top + W.value.height / q.value * t + I.value.style.layout.dataLabels.yAxis.fontSize / 3,
						"text-anchor": "middle",
						"font-size": I.value.style.layout.dataLabels.yAxis.fontSize,
						fill: I.value.style.layout.dataLabels.yAxis.color,
						"font-weight": I.value.style.layout.dataLabels.yAxis.bold ? "bold" : "normal"
					}, T(E(l)(I.value.style.layout.dataLabels.yAxis.formatter, e, E(s)({ v: e }), {
						datapoint: e,
						seriesIndex: t
					})), 9, Qe)) : p("", !0)], 64))), 256))])) : p("", !0),
					I.value.style.layout.dataLabels.xAxis.show ? (x(), m("g", $e, [
						I.value.style.layout.grid.show ? (x(), m("g", et, [h("line", {
							x1: G.value.right[0].x,
							x2: G.value.right.at(-1).x,
							y1: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2,
							y2: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2,
							stroke: I.value.style.layout.grid.stroke,
							"stroke-width": I.value.style.layout.grid.strokeWidth,
							"stroke-linecap": "round"
						}, null, 8, tt), h("line", {
							x1: G.value.left[0].x,
							x2: G.value.left.at(-1).x,
							y1: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2,
							y2: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2,
							stroke: I.value.style.layout.grid.stroke,
							"stroke-width": I.value.style.layout.grid.strokeWidth,
							"stroke-linecap": "round"
						}, null, 8, nt)])) : p("", !0),
						(x(!0), m(u, null, C(G.value.right, (e, t) => (x(), m("g", null, [I.value.style.layout.grid.show ? (x(), m("line", {
							key: 0,
							x1: e.x,
							x2: e.x,
							y1: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2,
							y2: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2 + 4,
							stroke: I.value.style.layout.grid.stroke,
							"stroke-width": I.value.style.layout.grid.strokeWidth,
							"stroke-linecap": "round"
						}, null, 8, rt)) : p("", !0)]))), 256)),
						(x(!0), m(u, null, C(G.value.left, (e, t) => (x(), m("g", null, [I.value.style.layout.grid.show ? (x(), m("line", {
							key: 0,
							x1: e.x,
							x2: e.x,
							y1: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2,
							y2: W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize / 2 + 4,
							stroke: I.value.style.layout.grid.stroke,
							"stroke-width": I.value.style.layout.grid.strokeWidth,
							"stroke-linecap": "round"
						}, null, 8, it)) : p("", !0)]))), 256)),
						h("g", {
							ref_key: "xAxisLabels",
							ref: jt
						}, [(x(!0), m(u, null, C(G.value.right, (e, t) => (x(), m("text", {
							class: "vue-ui-age-pyramid-x-axis-label",
							"font-size": I.value.style.layout.dataLabels.xAxis.fontSize,
							fill: I.value.style.layout.dataLabels.xAxis.color,
							"text-anchor": I.value.style.layout.dataLabels.xAxis.rotation > 0 ? "start" : I.value.style.layout.dataLabels.xAxis.rotation < 0 ? "end" : "middle",
							"font-weight": I.value.style.layout.dataLabels.xAxis.bold ? "bold" : "normal",
							transform: `translate(${e.x}, ${W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize * 2}) rotate(${I.value.style.layout.dataLabels.xAxis.rotation})`
						}, T(E(l)(I.value.style.layout.dataLabels.xAxis.formatter, e.value / I.value.style.layout.dataLabels.xAxis.scale, E(s)({ v: e.value / I.value.style.layout.dataLabels.xAxis.scale }), {
							datapoint: e,
							seriesIndex: t
						})), 9, at))), 256)), (x(!0), m(u, null, C(G.value.left, (e, t) => (x(), m("text", {
							class: "vue-ui-age-pyramid-x-axis-label",
							"font-size": I.value.style.layout.dataLabels.xAxis.fontSize,
							fill: I.value.style.layout.dataLabels.xAxis.color,
							"text-anchor": I.value.style.layout.dataLabels.xAxis.rotation > 0 ? "start" : I.value.style.layout.dataLabels.xAxis.rotation < 0 ? "end" : "middle",
							"font-weight": I.value.style.layout.dataLabels.xAxis.bold ? "bold" : "normal",
							transform: `translate(${e.x}, ${W.value.bottom + I.value.style.layout.dataLabels.xAxis.fontSize * 2}) rotate(${I.value.style.layout.dataLabels.xAxis.rotation})`
						}, T(E(l)(I.value.style.layout.dataLabels.xAxis.formatter, e.value / I.value.style.layout.dataLabels.xAxis.scale, E(s)({ v: e.value / I.value.style.layout.dataLabels.xAxis.scale }), {
							datapoint: e,
							seriesIndex: t
						})), 9, ot))), 256))], 512),
						h("text", {
							x: W.value.right,
							y: H.value.height,
							"text-anchor": "end",
							"font-size": I.value.style.layout.dataLabels.xAxis.fontSize,
							fill: I.value.style.layout.dataLabels.xAxis.color,
							"font-weight": I.value.style.layout.dataLabels.xAxis.bold ? "bold" : "normal"
						}, T(I.value.style.layout.dataLabels.xAxis.translation), 9, st)
					])) : p("", !0)
				]),
				(x(!0), m(u, null, C(e.dataset, (e, t) => (x(), m("g", null, [h("rect", {
					x: W.value.left,
					y: W.value.top + W.value.height / q.value * t - I.value.style.layout.bars.gap / 2,
					width: W.value.width <= 0 ? 1e-4 : W.value.width,
					height: W.value.height / q.value <= 0 ? 1e-4 : W.value.height / q.value,
					fill: j.value !== null && j.value === t ? E(n)(I.value.style.highlighter.color, I.value.style.highlighter.opacity) : "transparent",
					onMouseover: (n) => sn(t, e, "pointer"),
					onMouseleave: (n) => an(t, e),
					onClick: (n) => rn(t, e)
				}, null, 40, ct)]))), 256)),
				w(t.$slots, "svg", { svg: {
					...H.value,
					drawingArea: W.value,
					isPrintingImg: E(Gt) || E(Kt) || E(yn),
					isPrintingSvg: E(bn)
				} }, void 0, !0)
			], 46, He)), t.$slots.hint ? (x(), m("div", lt, [w(t.$slots, "hint", y(v({
				hint: I.value.a11y.translations.keyboardNavigation,
				isVisible: Pt.value
			})), void 0, !0)])) : p("", !0)]),
			t.$slots.watermark ? (x(), m("div", ut, [w(t.$slots, "watermark", y(v({ isPrinting: E(Gt) || E(Kt) || E(yn) || E(bn) })), void 0, !0)])) : p("", !0),
			w(t.$slots, "legend", { legend: J.value }, void 0, !0),
			t.$slots.source ? (x(), m("div", {
				key: 6,
				ref_key: "source",
				ref: Dt,
				dir: "auto"
			}, [w(t.$slots, "source", {}, void 0, !0)], 512)) : p("", !0),
			g(E(dt), {
				teleportTo: I.value.style.tooltip.teleportTo,
				show: V.value.showTooltip && A.value,
				backgroundColor: I.value.style.tooltip.backgroundColor,
				color: I.value.style.tooltip.color,
				borderRadius: I.value.style.tooltip.borderRadius,
				borderColor: I.value.style.tooltip.borderColor,
				borderWidth: I.value.style.tooltip.borderWidth,
				fontSize: I.value.style.tooltip.fontSize,
				backgroundOpacity: I.value.style.tooltip.backgroundOpacity,
				position: I.value.style.tooltip.position,
				offsetX: I.value.style.tooltip.offsetX,
				offsetY: I.value.style.tooltip.offsetY,
				parent: M.value,
				content: wt.value,
				isFullscreen: Z.value,
				isCustom: I.value.style.tooltip.customFormat && typeof I.value.style.tooltip.customFormat == "function",
				smooth: I.value.style.tooltip.smooth,
				backdropFilter: I.value.style.tooltip.backdropFilter,
				smoothForce: I.value.style.tooltip.smoothForce,
				smoothSnapThreshold: I.value.style.tooltip.smoothSnapThreshold,
				isA11yMode: F.value === "keyboard",
				a11yPosition: Nt.value
			}, {
				"tooltip-before": D(() => [w(t.$slots, "tooltip-before", y(v({ ...Y.value })), void 0, !0)]),
				tooltip: D(() => [w(t.$slots, "tooltip", y(v({ ...Y.value })), void 0, !0)]),
				"tooltip-after": D(() => [w(t.$slots, "tooltip-after", y(v({ ...Y.value })), void 0, !0)]),
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
			Ct.value && I.value.userOptions.buttons.table ? (x(), f(Ne($.value.component), Oe({ key: 7 }, $.value.props, {
				ref_key: "tableUnit",
				ref: N,
				onClose: gn
			}), Ee({
				content: D(() => [(x(), f(E(mt), {
					key: `table_${At.value}`,
					colNames: X.value.colNames,
					head: X.value.head,
					body: X.value.body,
					config: X.value.config,
					title: I.value.table.useDialog ? "" : $.value.title,
					withCloseButton: !I.value.table.useDialog,
					isCursorPointer: L.value,
					onClose: gn
				}, {
					th: D(({ th: e }) => [De(T(e), 1)]),
					td: D(({ td: e }) => [De(T(e), 1)]),
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
			}, [I.value.table.useDialog ? {
				name: "title",
				fn: D(() => [De(T($.value.title), 1)]),
				key: "0"
			} : void 0, I.value.table.useDialog ? {
				name: "actions",
				fn: D(() => [h("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: r[0] ||= (e) => cn(I.value.userOptions.callbacks.csv),
					style: b({ cursor: L.value ? "pointer" : "default" })
				}, [g(E(ft), {
					name: "fileCsv",
					stroke: $.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : p("", !0),
			w(t.$slots, "skeleton", {}, () => [E(It) ? (x(), f(de, { key: 0 })) : p("", !0)], !0)
		], 46, ze));
	}
}, [["__scopeId", "data-v-96895e9d"]]);
//#endregion
export { Re as n, dt as t };
