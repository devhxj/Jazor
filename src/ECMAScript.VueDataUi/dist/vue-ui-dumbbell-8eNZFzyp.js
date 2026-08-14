import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, Jt as i, Ot as a, X as o, Y as s, _ as ee, b as te, i as c, jt as ne, pt as re, q as ie, t as ae, tt as oe } from "./lib-Bttd6u5E.js";
import { n as se, t as ce } from "./useHints-Dq_w2E8B.js";
import { t as le } from "./useConfig-DlNpz6P8.js";
import { t as ue } from "./usePrinter-DN5bYhTG.js";
import { n as de, t as fe } from "./BaseScanner-DZvpgOjM.js";
import { t as pe } from "./useNestedProp-vPNvh7rV.js";
import { t as me } from "./useThemeCheck-C43Tcqmk.js";
import { t as he } from "./useChartExport-DNiwdPmb.js";
import { t as ge } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { t as _e } from "./img-Bnokohej.js";
import { n as ve } from "./Title-BE3qg9xl.js";
import { t as ye } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as be, t as xe } from "./useResponsive-ZtArZtUf.js";
import { t as l } from "./DefGrad-DVBqDjhO.js";
import { t as Se } from "./A11yDataTable-DdRsVULz.js";
import { t as Ce } from "./useUserOptionState-DK-_1ddE.js";
import { t as we } from "./useChartAccessibility-DYqac8yF.js";
import { t as Te } from "./Legend-CQxUgOd-.js";
import { t as Ee } from "./usePrefersMotion-BC-CsqR1.js";
import { t as De } from "./vue_ui_dumbbell-Bfe_jFyi.js";
import { Fragment as u, Teleport as Oe, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createSlots as ke, createTextVNode as Ae, createVNode as g, defineAsyncComponent as _, guardReactiveProps as v, mergeProps as je, nextTick as Me, normalizeClass as Ne, normalizeProps as y, normalizeStyle as Pe, onBeforeUnmount as Fe, onMounted as Ie, openBlock as b, ref as x, renderList as S, renderSlot as C, resolveDynamicComponent as Le, shallowRef as Re, toDisplayString as w, toRefs as ze, unref as T, vShow as E, watch as Be, watchEffect as Ve, withCtx as D, withDirectives as O } from "vue";
//#region src/components/vue-ui-dumbbell.vue
var He = /* @__PURE__ */ e({ default: () => Dt }), Ue = ["id"], We = ["id"], Ge = ["id"], Ke = { style: { position: "relative" } }, qe = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], Je = [
	"x",
	"y",
	"width",
	"height"
], Ye = { key: 1 }, Xe = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], Ze = { key: 2 }, Qe = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], $e = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], et = [
	"transform",
	"font-size",
	"fill"
], tt = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], nt = [
	"x",
	"y",
	"font-size",
	"fill",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], rt = [
	"x",
	"y",
	"font-size",
	"fill"
], it = [
	"transform",
	"font-size",
	"fill",
	"font-weight",
	"text-anchor"
], at = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], ot = [
	"d",
	"stroke",
	"stroke-width",
	"stroke-dasharray"
], st = [
	"x",
	"y",
	"height",
	"width",
	"fill"
], ct = [
	"transform",
	"fill",
	"font-size"
], lt = { key: 0 }, ut = { key: 0 }, dt = ["d", "fill"], ft = { key: 1 }, pt = [
	"x",
	"y",
	"height",
	"width",
	"fill"
], mt = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], ht = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], gt = { key: 7 }, _t = [
	"x",
	"y",
	"fill",
	"font-size"
], vt = { key: 8 }, yt = [
	"x",
	"y",
	"fill",
	"font-size"
], bt = [
	"x",
	"y",
	"width",
	"height",
	"fill",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], xt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, St = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Ct = ["id"], wt = [
	"xmlns",
	"height",
	"width"
], Tt = ["fill"], Et = ["innerHTML"], Dt = /*#__PURE__*/ ye({
	__name: "vue-ui-dumbbell",
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
	setup(e, { expose: ye, emit: He }) {
		let Dt = _(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Ot = _(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), kt = _(() => import("./DataTable-BbKgJ5UI.js")), At = _(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), jt = _(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Mt = _(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Nt = _(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_dumbbell: Pt } = le(), { isThemeValid: Ft, warnInvalidTheme: It } = me(), Lt = Ee(), k = e, Rt = He, zt = d({
			get() {
				return !!k.dataset && k.dataset.length;
			},
			set(e) {
				return e;
			}
		}), A = x(ie()), Bt = x(0), j = x(null), Vt = x(null), Ht = x(null), Ut = x(null), Wt = x(null), Gt = x(0), Kt = x(0), qt = x(0), Jt = x(!1), M = x(null), Yt = x(null), Xt = x(null), Zt = x(null), Qt = x(null), $t = x(null), N = x(null), P = x(null), F = x(null), en = x(!1), I = x(!1), L = x(ln());
		se({
			config: () => L.value,
			dataset: () => k.dataset,
			component: "VueUiDumbbell",
			rules: [ce.emptyArray, {
				test: (e) => e.length > 31,
				message: [
					"👀 The number of series is > 31. Consider:",
					"",
					"▶️ Using filters to let users choose a maximum number of series to display.",
					"",
					"▶️ Using multiple instances of the component to display related series."
				]
			}]
		});
		let R = d(() => L.value.userOptions.useCursorPointer), tn = d(() => i({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				useAnimation: !1,
				style: { chart: {
					backgroundColor: "#99999930",
					padding: {
						top: 12,
						right: 12,
						bottom: 12,
						left: 12
					},
					grid: {
						horizontalGrid: { stroke: "#6A6A6A" },
						verticalGrid: { stroke: "#6A6A6A" }
					},
					labels: {
						axis: {
							yLabel: "",
							xLabel: ""
						},
						xAxisLabels: { show: !1 },
						yAxisLabels: { show: !1 },
						endLabels: { show: !1 },
						startLabels: { show: !1 }
					},
					legend: { backgroundColor: "transparent" },
					plots: {
						endColor: "#969696",
						startColor: "#DBDBDB",
						stroke: "#6A6A6A",
						evaluationColors: { enable: !1 }
					}
				} }
			},
			userConfig: L.value.skeletonConfig ?? {}
		})), { loading: nn, FINAL_DATASET: rn, manualLoading: an } = de({
			...ze(k),
			FINAL_CONFIG: L,
			prepareConfig: ln,
			skeletonDataset: k.config?.skeletonDataset ?? [
				{
					name: "_",
					start: 21,
					end: 34
				},
				{
					name: "_",
					start: 13,
					end: 21
				},
				{
					name: "_",
					start: 8,
					end: 13
				},
				{
					name: "_",
					start: 5,
					end: 8
				},
				{
					name: "_",
					start: 3,
					end: 5
				}
			],
			skeletonConfig: i({
				defaultConfig: L.value,
				userConfig: tn.value
			})
		}), { userOptionsVisible: on, setUserOptionsVisibility: sn, keepUserOptionState: cn } = Ce({ config: L.value }), { svgRef: z } = we({ config: L.value.style.chart.title });
		function ln() {
			let e = pe({
				userConfig: k.config,
				defaultConfig: Pt
			}), t = e.theme;
			if (!t) return e;
			if (!Ft.value(e)) return It(e), e;
			let n = pe({
				userConfig: De[t] || k.config,
				defaultConfig: e
			});
			return pe({
				userConfig: k.config,
				defaultConfig: n
			});
		}
		Be(() => k.config, (e) => {
			nn.value || (L.value = ln()), on.value = !L.value.userOptions.showOnChartHover, dn(), Gt.value += 1, Kt.value += 1, qt.value += 1, G.value = L.value.style.chart.rowHeight, K.value = L.value.style.chart.width, H.value.showTable = L.value.table.show;
		}, { deep: !0 }), Be(() => rn.value, (e) => {
			Array.isArray(e) && e.length > 0 && (an.value = !1), Tn(), En();
		}, { deep: !0 });
		let B = Re(null), V = Re(null);
		Ie(() => {
			Jt.value = !0, dn();
		});
		let un = d(() => L.value.debug);
		function dn() {
			if (ne(k.dataset) ? oe({
				componentName: "VueUiDumbbell",
				type: "dataset",
				debug: un.value
			}) : k.dataset.forEach((e, t) => {
				re({
					datasetObject: e,
					requiredAttributes: [
						"name",
						"start",
						"end"
					]
				}).forEach((e) => {
					zt.value = !1, oe({
						componentName: "VueUiDumbbell",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: un.value
					});
				});
			}), L.value.responsive) {
				let e = be(() => {
					let { width: e, height: t } = xe({
						chart: j.value,
						title: L.value.style.chart.title.text ? Vt.value : null,
						legend: L.value.style.chart.legend.show ? Ht.value : null,
						source: Ut.value,
						noTitle: Wt.value
					}), n = L.value.style.chart.title.text ? 24 : 0, r = L.value.style.chart.legend.show ? 24 : 0;
					requestAnimationFrame(async () => {
						K.value = Math.max(.1, e), G.value = Math.max(.1, (Math.max(.1, t) - (n + r)) / rn.value.length), Bn();
					});
				});
				B.value && (V.value && B.value.unobserve(V.value), B.value.disconnect()), B.value = new ResizeObserver(e), V.value = j.value.parentNode, B.value.observe(V.value);
			}
			Bn();
		}
		Fe(() => {
			X.value != null && (cancelAnimationFrame(X.value), X.value = null), B.value && (V.value && B.value.unobserve(V.value), B.value.disconnect());
		});
		let { isPrinting: fn, isImaging: pn, generatePdf: mn, generateImage: hn } = ue({
			elementId: `dumbbell_${A.value}`,
			fileName: L.value.style.chart.title.text || "vue-ui-dumbbell",
			options: L.value.userOptions.print
		}), gn = d(() => L.value.userOptions.show && !L.value.style.chart.title.text), H = x({ showTable: L.value.table.show });
		Be(L, () => {
			H.value = { showTable: L.value.table.show };
		}, { immediate: !0 });
		let U = d(() => rn.value.map((e, t) => ({
			...e,
			start: te(e.start),
			end: te(e.end),
			id: e.id ?? `${String(e.name)}__${String(e.start)}__${String(e.end)}__${ie()}`
		}))), _n = d(() => {
			let e = L.value.style.chart.grid, t = U.value.flatMap((e) => [e.start, e.end]).map((e) => Number(e)).filter((e) => Number.isFinite(e)), n = t.length ? Math.min(...t) : 0, r = t.length ? Math.max(...t) : 0;
			return {
				min: e.scaleMin ?? Math.min(n, 0),
				max: e.scaleMax ?? r
			};
		}), W = d(() => ee(_n.value.min, _n.value.max, L.value.style.chart.grid.scaleSteps)), G = x(L.value.style.chart.rowHeight), K = x(L.value.style.chart.width);
		function vn() {
			let e = 0;
			Zt.value && (e = Array.from(Zt.value.querySelectorAll("text")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0));
			let t = Xt.value ? Xt.value.getBoundingClientRect().width : 0;
			return e + t + (t ? 24 + L.value.style.chart.labels.axis.yLabelOffsetX : 0);
		}
		let yn = x(0), bn = be((e) => {
			yn.value = e;
		}, 100);
		Ve((e) => {
			let t = Qt.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				bn(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), Fe(() => {
			yn.value = 0;
		});
		let xn = d(() => {
			I.value;
			let e = 0;
			$t.value && (e = $t.value.getBBox().height);
			let t = 0;
			return Qt.value && (t = yn.value), e + t;
		}), Sn = d(() => U.value.length);
		function Cn(e, t, n) {
			let r = Number(e), i = Number(t.min), a = Number(t.max), o = Number(n.width);
			if (!Number.isFinite(r) || !Number.isFinite(i) || !Number.isFinite(a) || !Number.isFinite(o)) return n.left;
			let s = a - i;
			return s <= 0 ? n.left : n.left + (r - i) / s * o;
		}
		let q = d(() => {
			I.value;
			let e = vn(), t = L.value.style.chart.padding, n = L.value.style.chart.labels.axis.xLabel ? L.value.style.chart.labels.axis.xLabelOffsetY : 0, r = G.value * Sn.value - xn.value - t.top - t.bottom - n, i = r / Sn.value, a = G.value * Sn.value, o = K.value - e - t.left - t.right, s = W.value.ticks.length * (o / W.value.ticks.length);
			return {
				left: L.value.style.chart.padding.left + e,
				right: K.value - L.value.style.chart.padding.right,
				top: L.value.style.chart.padding.top,
				bottom: a - L.value.style.chart.padding.bottom - xn.value - n,
				width: o,
				height: r,
				rowHeight: i,
				absoluteHeight: a,
				widthPlotReference: s
			};
		}), J = d(() => Math.min(G.value / 2 * .7, L.value.style.chart.plots.radius)), wn = x([]), Y = d({
			get() {
				let e = q.value, t = W.value;
				return wn.value.map((n, r) => {
					let i = Cn(n.start, t, e), a = Cn(n.endVal, t, e), o = i + (a - i) / 2, s = ![null, void 0].includes(n.start) && ![null, void 0].includes(n.end) && n.end > n.start, ee = ![null, void 0].includes(n.start) && ![null, void 0].includes(n.end) && n.end < n.start, te = ![null, void 0].includes(n.start) && ![null, void 0].includes(n.end) && n.end === n.start || [null, void 0].includes(n.start) || [null, void 0].includes(n.end), c = s ? L.value.style.chart.plots.evaluationColors.positive : ee ? L.value.style.chart.plots.evaluationColors.negative : L.value.style.chart.plots.evaluationColors.neutral;
					return {
						...n,
						isPositive: s,
						isNegative: ee,
						isNeutral: te,
						evaluationColor: c,
						evaluationGrad: `url(#${s ? "positive" : ee ? "negative" : "neutral"}_grad_${A.value})`,
						startX: i,
						endX: a,
						centerX: o,
						y: e.top + r * e.rowHeight + e.rowHeight / 2
					};
				});
			},
			set(e) {
				wn.value = e;
			}
		});
		function Tn() {
			wn.value = U.value.map((e) => {
				let t = Number(e.start);
				return {
					...e,
					endVal: Number.isFinite(t) ? t : 0
				};
			});
		}
		let X = x(null);
		Ie(() => {
			En();
		});
		function En() {
			if (X.value != null && (cancelAnimationFrame(X.value), X.value = null), Tn(), !L.value.useAnimation || Lt.value) {
				Y.value = wn.value.map((e) => {
					let t = Number(e.end);
					return {
						...e,
						endVal: Number.isFinite(t) ? t : e.endVal
					};
				});
				return;
			}
			let e = Math.max(1, Math.min(100, L.value.animationSpeed || 100)) / 100, t = U.value.map((e) => {
				let t = Number(e.start), n = Number(e.end), r = Number.isFinite(t) ? t : 0;
				return (Number.isFinite(n) ? n : r) - r;
			}), n = () => {
				let r = !0;
				Y.value = wn.value.map((n, i) => {
					let a = Number.isFinite(Number(n.end)) ? Number(n.end) : n.endVal, o = n.endVal + t[i] * e, s = t[i] >= 0 ? Math.min(o, a) : Math.max(o, a);
					return s !== a && (r = !1), {
						...n,
						endVal: s
					};
				}), r ? X.value = null : X.value = requestAnimationFrame(n);
			};
			X.value = requestAnimationFrame(n);
		}
		let Dn = d(() => L.value.style.chart.plots.evaluationColors.enable ? [
			{
				name: L.value.style.chart.legend.labelNegative,
				color: L.value.style.chart.plots.gradient.show ? `url(#negative_grad_${A.value})` : L.value.style.chart.plots.evaluationColors.negative
			},
			{
				name: L.value.style.chart.legend.labelNeutral,
				color: L.value.style.chart.plots.gradient.show ? `url(#neutral_grad_${A.value})` : L.value.style.chart.plots.evaluationColors.neutral
			},
			{
				name: L.value.style.chart.legend.labelPositive,
				color: L.value.style.chart.plots.gradient.show ? `url(#positive_grad_${A.value})` : L.value.style.chart.plots.evaluationColors.positive
			}
		] : [{
			name: L.value.style.chart.legend.labelStart,
			color: L.value.style.chart.plots.gradient.show ? `url(#start_grad_${A.value})` : L.value.style.chart.plots.startColor
		}, {
			name: L.value.style.chart.legend.labelEnd,
			color: L.value.style.chart.plots.gradient.show ? `url(#end_grad_${A.value})` : L.value.style.chart.plots.endColor
		}]), On = d(() => ({
			cy: "donut-div-legend",
			backgroundColor: L.value.style.chart.legend.backgroundColor,
			color: L.value.style.chart.legend.color,
			fontSize: L.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			paddingTop: 12,
			fontWeight: L.value.style.chart.legend.bold ? "bold" : ""
		})), Z = d(() => ({
			head: Y.value.map((e) => ({ name: e.name })),
			body: Y.value.map((e) => ({
				start: e.start,
				end: e.end
			}))
		})), Q = d(() => {
			let e = [
				L.value.table.columnNames.series,
				L.value.table.columnNames.start,
				L.value.table.columnNames.end,
				L.value.table.columnNames.progression
			], t = Z.value.head.map((e, t) => {
				let n = o({
					p: L.value.style.chart.labels.prefix,
					v: Z.value.body[t].start,
					s: L.value.style.chart.labels.suffix,
					r: L.value.table.td.roundingValue
				}), r = o({
					p: L.value.style.chart.labels.prefix,
					v: Z.value.body[t].end,
					s: L.value.style.chart.labels.suffix,
					r: L.value.table.td.roundingValue
				}), i = o({
					v: 100 * (Z.value.body[t].end / Z.value.body[t].start - 1),
					s: "%",
					r: L.value.table.td.roundingPercentage
				});
				return [
					{ name: e.name },
					n,
					r,
					i
				];
			}), n = {
				th: {
					backgroundColor: L.value.table.th.backgroundColor,
					color: L.value.table.th.color,
					outline: L.value.table.th.outline
				},
				td: {
					backgroundColor: L.value.table.td.backgroundColor,
					color: L.value.table.td.color,
					outline: L.value.table.td.outline
				},
				breakpoint: L.value.table.responsiveBreakpoint
			};
			return {
				colNames: [
					L.value.table.columnNames.series,
					L.value.table.columnNames.start,
					L.value.table.columnNames.end,
					L.value.table.columnNames.progression
				],
				head: e,
				body: t,
				config: n
			};
		});
		function kn(e = null) {
			Me(() => {
				let n = Z.value.head.map((e, t) => [
					[e.name],
					[Z.value.body[t].start],
					[Z.value.body[t].end]
				]), i = [
					[L.value.style.chart.title.text],
					[L.value.style.chart.title.subtitle.text],
					[
						[L.value.table.columnNames.series],
						[L.value.table.columnNames.start],
						[L.value.table.columnNames.end]
					]
				].concat(n), a = r(i);
				e ? e(a) : t({
					csvContent: a,
					title: L.value.style.chart.title.text || "vue-ui-dumbbell"
				});
			});
		}
		let $ = x(!1);
		function An(e) {
			$.value = e, Bt.value += 1;
		}
		function jn() {
			return U.value;
		}
		function Mn() {
			H.value.showTable = !H.value.showTable;
		}
		let Nn = x(!1);
		function Pn() {
			Nn.value = !Nn.value;
		}
		async function Fn({ scale: e = 2 } = {}) {
			if (!j.value) return;
			let { width: t, height: n } = j.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await _e({
				domElement: j.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: L.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let In = d(() => W.value.ticks), Ln = d(() => ({
			start: 0,
			end: In.value.length
		}));
		ge({
			timeLabelsEls: Qt,
			timeLabels: In,
			slicer: Ln,
			configRef: L,
			rotationPath: [
				"style",
				"chart",
				"labels",
				"xAxisLabels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"labels",
				"xAxisLabels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: K,
			height: G,
			targetClass: ".vue-ui-dumbbell-scale-label",
			rotation: L.value.style.chart.labels.xAxisLabels.autoRotate.angle
		});
		function Rn({ rowHeight: e, fontSize: t, showProgression: n }) {
			if (!n) return !1;
			let r = e / 3, i = e / 1.3;
			return Math.abs(i - r) < t * 1.2;
		}
		let zn = (() => {
			let e = null, t = 0;
			return (n) => {
				if (n === I.value) {
					e = null, t = 0;
					return;
				}
				e === null || e !== n ? (e = n, t = 1) : (t += 1, t >= 1 && (I.value = n, e = null, t = 0));
			};
		})(), Bn = be(() => {
			requestAnimationFrame(() => {
				requestAnimationFrame(() => {
					let e = Rn({
						rowHeight: q.value.rowHeight,
						fontSize: L.value.style.chart.labels.yAxisLabels.fontSize,
						showProgression: L.value.style.chart.labels.yAxisLabels.showProgression
					});
					zn(e);
				});
			});
		}, 100);
		function Vn({ datapoint: e, seriesIndex: t }) {
			N.value = t, P.value = e, L.value.events.datapointEnter && L.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Hn({ datapoint: e, seriesIndex: t }) {
			N.value = null, P.value = null, L.value.events.datapointLeave && L.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Un({ datapoint: e, seriesIndex: t }) {
			L.value.events.datapointClick && L.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			}), Rt("selectDatapoint", {
				...e,
				seriesIndex: t
			});
		}
		let Wn = d(() => {
			if (P.value === null) return 0;
			let e = ![null, void 0].includes(P.value.start), t = ![null, void 0].includes(P.value.end);
			return e && t ? Math.min(P.value.startX, P.value.endX) + Math.abs(P.value.startX - P.value.endX) / 2 : e && !t ? P.value.startX : t && !e ? P.value.endX : null;
		}), Gn = d(() => {
			if (P.value === null) return "";
			let e = ![null, void 0].includes(P.value.start), t = ![null, void 0].includes(P.value.end), n = "", r = "";
			return e && (n = c(L.value.style.chart.labels.formatter, P.value.start, o({
				p: L.value.style.chart.labels.prefix,
				v: P.value.start,
				s: L.value.style.chart.labels.suffix,
				r: L.value.style.chart.labels.startLabels.rounding
			}), {
				datapoint: P.value,
				seriesIndex: N.value
			})), t && (r = c(L.value.style.chart.labels.formatter, P.value.end, o({
				p: L.value.style.chart.labels.prefix,
				v: P.value.end,
				s: L.value.style.chart.labels.suffix,
				r: L.value.style.chart.labels.startLabels.rounding
			}), {
				datapoint: P.value,
				seriesIndex: N.value
			})), e && t ? `${n} → ${r}` : e && !t ? n : t && !e ? r : "";
		}), Kn = d(() => {
			let e = L.value.table.useDialog && !L.value.table.show, t = H.value.showTable;
			return {
				component: e ? Nt : Ot,
				title: `${L.value.style.chart.title.text}${L.value.style.chart.title.subtitle.text ? `: ${L.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: L.value.table.th.backgroundColor,
					color: L.value.table.th.color,
					headerColor: L.value.table.th.color,
					headerBg: L.value.table.th.backgroundColor,
					isFullscreen: $.value,
					fullscreenParent: j.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: R.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: L.value.style.chart.backgroundColor,
							color: L.value.style.chart.color
						},
						head: {
							backgroundColor: L.value.style.chart.backgroundColor,
							color: L.value.style.chart.color
						}
					}
				}
			};
		});
		Be(() => H.value.showTable, (e) => {
			L.value.table.show || (e && L.value.table.useDialog && M.value ? M.value.open() : "close" in M.value && M.value.close());
		});
		function qn() {
			H.value.showTable = !1, Yt.value && Yt.value.setTableIconState(!1);
		}
		let Jn = d(() => Dn.value.map((e) => ({
			...e,
			shape: "circle"
		}))), Yn = d(() => L.value.style.chart.backgroundColor), Xn = d(() => L.value.style.chart.legend), Zn = d(() => L.value.style.chart.title), { isCallbackImaging: Qn, isCallbackSvg: $n, generateSvg: er, onGenerateImage: tr } = he({
			svg: z,
			title: Zn,
			legend: Xn,
			legendItems: Jn,
			backgroundColor: Yn,
			getSvgCallback: () => L.value.userOptions.callbacks.svg,
			generateImage: hn
		});
		async function nr() {
			if (Rt("copyAlt", {
				config: L.value,
				dataset: Y.value
			}), !L.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(L.value.userOptions.callbacks.altCopy({
				config: L.value,
				dataset: Y.value
			}));
		}
		function rr(e) {
			let t = Y.value.length;
			return t ? (e % t + t) % t : null;
		}
		function ir() {
			if (F.value !== null) {
				let e = Y.value[F.value];
				e && Hn({
					datapoint: e,
					seriesIndex: F.value
				});
			}
			F.value = null, N.value = null, P.value = null;
		}
		function ar() {
			F.value = null, en.value = !0;
		}
		function or() {
			ir(), en.value = !1;
		}
		function sr(e) {
			if (!z.value || Nn.value || document.activeElement !== z.value || !Y.value.length) return;
			let t = ["ArrowUp", "ArrowLeft"].includes(e.key), n = ["ArrowDown", "ArrowRight"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				ir();
				return;
			}
			if (r) {
				if (F.value === null) return;
				let e = Y.value[F.value];
				if (!e) return;
				Un({
					datapoint: e,
					seriesIndex: F.value
				});
				return;
			}
			let a = F.value;
			a = a === null ? n ? 0 : Y.value.length - 1 : rr(a + (n ? 1 : -1));
			let o = Y.value[a];
			o && (F.value = a, Vn({
				datapoint: o,
				seriesIndex: a
			}));
		}
		let cr = d(() => ({
			head: Q.value.head,
			body: Q.value.body.map((e) => [
				e[0]?.name ?? "",
				e[1],
				e[2],
				e[3]
			]),
			caption: L.value.a11y.translations.tableCaption,
			notice: L.value.a11y.translations.tableAvailable
		}));
		return ye({
			getData: jn,
			getImage: Fn,
			generatePdf: mn,
			generateCsv: kn,
			generateImage: hn,
			generateSvg: er,
			toggleTable: Mn,
			toggleAnnotator: Pn,
			toggleFullscreen: An,
			copyAlt: nr
		}), (e, t) => (b(), m("div", {
			ref_key: "dumbbellChart",
			ref: j,
			class: Ne(`vue-data-ui-component vue-ui-dumbbell ${$.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			style: Pe(`font-family:${L.value.style.fontFamily};width:100%; text-align:center;background:${L.value.style.chart.backgroundColor};${L.value.responsive ? "height:100%" : ""}`),
			id: `dumbbell_${A.value}`,
			onMouseenter: t[1] ||= () => T(sn)(!0),
			onMouseleave: t[2] ||= () => T(sn)(!1)
		}, [
			h("div", {
				id: `chart-instructions-${A.value}`,
				class: "sr-only"
			}, [h("p", null, w(L.value.a11y.translations.keyboardNavigation), 1)], 8, We),
			cr.value.body.length ? (b(), f(Se, {
				key: 0,
				uid: A.value,
				head: cr.value.head,
				body: cr.value.body,
				caption: cr.value.caption,
				notice: cr.value.notice
			}, null, 8, [
				"uid",
				"head",
				"body",
				"caption",
				"notice"
			])) : p("", !0),
			L.value.userOptions.buttons.annotator ? (b(), f(T(At), {
				key: 1,
				svgRef: T(z),
				backgroundColor: L.value.style.chart.backgroundColor,
				color: L.value.style.chart.color,
				active: Nn.value,
				isCursorPointer: R.value,
				onClose: Pn
			}, {
				"annotator-action-close": D(() => [C(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": D(({ color: t }) => [C(e.$slots, "annotator-action-color", y(v({ color: t })), void 0, !0)]),
				"annotator-action-draw": D(({ mode: t }) => [C(e.$slots, "annotator-action-draw", y(v({ mode: t })), void 0, !0)]),
				"annotator-action-undo": D(({ disabled: t }) => [C(e.$slots, "annotator-action-undo", y(v({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": D(({ disabled: t }) => [C(e.$slots, "annotator-action-redo", y(v({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": D(({ disabled: t }) => [C(e.$slots, "annotator-action-delete", y(v({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : p("", !0),
			gn.value ? (b(), m("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Wt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : p("", !0),
			L.value.style.chart.title.text ? (b(), m("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Vt,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(b(), f(ve, {
				key: `title_${Gt.value}`,
				config: {
					title: {
						cy: "donut-div-title",
						...L.value.style.chart.title
					},
					subtitle: {
						cy: "donut-div-subtitle",
						...L.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : p("", !0),
			h("div", { id: `legend-top-${A.value}` }, null, 8, Ge),
			L.value.userOptions.show && zt.value && (T(cn) || T(on)) ? (b(), f(T(jt), {
				ref_key: "userOptionsRef",
				ref: Yt,
				key: `user_option_${Bt.value}`,
				backgroundColor: L.value.style.chart.backgroundColor,
				color: L.value.style.chart.color,
				isPrinting: T(fn),
				isImaging: T(pn),
				uid: A.value,
				hasPdf: L.value.userOptions.buttons.pdf,
				hasXls: L.value.userOptions.buttons.csv,
				hasImg: L.value.userOptions.buttons.img,
				hasSvg: L.value.userOptions.buttons.svg,
				hasTable: L.value.userOptions.buttons.table,
				hasFullscreen: L.value.userOptions.buttons.fullscreen,
				hasAltCopy: L.value.userOptions.buttons.altCopy,
				isFullscreen: $.value,
				titles: { ...L.value.userOptions.buttonTitles },
				chartElement: j.value,
				position: L.value.userOptions.position,
				hasAnnotator: L.value.userOptions.buttons.annotator,
				isAnnotation: Nn.value,
				callbacks: L.value.userOptions.callbacks,
				printScale: L.value.userOptions.print.scale,
				tableDialog: L.value.table.useDialog,
				isCursorPointer: R.value,
				onToggleFullscreen: An,
				onGeneratePdf: T(mn),
				onGenerateCsv: kn,
				onGenerateImage: T(tr),
				onGenerateSvg: T(er),
				onToggleTable: Mn,
				onToggleAnnotator: Pn,
				onCopyAlt: nr,
				style: Pe({ visibility: T(cn) ? T(on) ? "visible" : "hidden" : "visible" })
			}, ke({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: D(({ isOpen: t, color: n }) => [C(e.$slots, "menuIcon", y(v({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: D(() => [C(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: D(() => [C(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: D(() => [C(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: D(() => [C(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: D(() => [C(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: D(({ toggleFullscreen: t, isFullscreen: n }) => [C(e.$slots, "optionFullscreen", y(v({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: D(({ toggleAnnotator: t, isAnnotator: n }) => [C(e.$slots, "optionAnnotator", y(v({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: D(({ altCopy: t }) => [C(e.$slots, "optionAltCopy", y(v({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: D(() => [C(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: D(() => [C(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasFullscreen.hasAltCopy.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : p("", !0),
			h("div", Ke, [(b(), m("svg", {
				ref_key: "svgRef",
				ref: z,
				xmlns: T(ae),
				class: Ne({
					"vue-data-ui-fullscreen--on": $.value,
					"vue-data-ui-fulscreen--off": !$.value
				}),
				viewBox: `0 0 ${K.value} ${q.value.absoluteHeight <= 0 ? 10 : q.value.absoluteHeight}`,
				style: Pe(`max-width:100%; overflow: visible; background:transparent;color:${L.value.style.chart.color}`),
				"aria-describedby": `chart-instructions-${A.value}`,
				tabindex: "0",
				onFocus: ar,
				onBlur: or,
				onKeydown: sr
			}, [
				g(T(Mt)),
				e.$slots["chart-background"] ? (b(), m("foreignObject", {
					key: 0,
					x: q.value.left,
					y: q.value.top,
					width: Math.max(.1, q.value.width),
					height: Math.max(.1, q.value.height),
					style: { pointerEvents: "none" }
				}, [C(e.$slots, "chart-background", {}, void 0, !0)], 8, Je)) : p("", !0),
				L.value.style.chart.grid.verticalGrid.show ? (b(), m("g", Ye, [(b(!0), m(u, null, S(W.value.ticks, (e, t) => (b(), m("line", {
					x1: q.value.left + t * q.value.width / (W.value.ticks.length - 1),
					x2: q.value.left + t * q.value.width / (W.value.ticks.length - 1),
					y1: q.value.top,
					y2: q.value.bottom,
					stroke: L.value.style.chart.grid.verticalGrid.stroke,
					"stroke-width": L.value.style.chart.grid.verticalGrid.strokeWidth,
					"stroke-dasharray": L.value.style.chart.grid.verticalGrid.strokeDasharray
				}, null, 8, Xe))), 256))])) : p("", !0),
				L.value.style.chart.grid.horizontalGrid.show ? (b(), m("g", Ze, [(b(!0), m(u, null, S(U.value, (e, t) => (b(), m("line", {
					x1: q.value.left,
					x2: q.value.right,
					y1: q.value.top + t * q.value.rowHeight,
					y2: q.value.top + t * q.value.rowHeight,
					stroke: L.value.style.chart.grid.horizontalGrid.stroke,
					"stroke-width": L.value.style.chart.grid.horizontalGrid.strokeWidth,
					"stroke-dasharray": L.value.style.chart.grid.horizontalGrid.strokeDasharray
				}, null, 8, Qe))), 256)), h("line", {
					x1: q.value.left,
					x2: q.value.right,
					y1: q.value.bottom,
					y2: q.value.bottom,
					stroke: L.value.style.chart.grid.horizontalGrid.stroke,
					"stroke-width": L.value.style.chart.grid.horizontalGrid.strokeWidth,
					"stroke-dasharray": L.value.style.chart.grid.horizontalGrid.strokeDasharray
				}, null, 8, $e)])) : p("", !0),
				L.value.style.chart.labels.axis.yLabel ? (b(), m("text", {
					key: 3,
					ref_key: "yAxisLabel",
					ref: Xt,
					transform: `translate(${L.value.style.chart.labels.axis.fontSize}, ${q.value.absoluteHeight / 2}), rotate(-90)`,
					"font-size": L.value.style.chart.labels.axis.fontSize,
					fill: L.value.style.chart.labels.axis.color,
					"text-anchor": "middle"
				}, w(L.value.style.chart.labels.axis.yLabel), 9, et)) : p("", !0),
				L.value.style.chart.labels.yAxisLabels.show ? (b(), m("g", {
					key: 4,
					ref_key: "serieLabels",
					ref: Zt
				}, [(b(!0), m(u, null, S(Y.value, (e, t) => (b(), m("text", {
					class: "vue-ui-dumbbell-serie-name",
					key: `serieLabel_${e.id}_${t}`,
					x: q.value.left - 6 + L.value.style.chart.labels.yAxisLabels.offsetX,
					y: q.value.top + t * q.value.rowHeight + (!L.value.style.chart.labels.yAxisLabels.showProgression || I.value ? q.value.rowHeight / 2 : q.value.rowHeight / 3) + L.value.style.chart.labels.yAxisLabels.fontSize / 3,
					"font-size": L.value.style.chart.labels.yAxisLabels.fontSize,
					fill: L.value.style.chart.labels.yAxisLabels.color,
					"font-weight": L.value.style.chart.labels.yAxisLabels.bold ? "bold" : "normal",
					"text-anchor": "end",
					onMouseenter: (n) => Vn({
						datapoint: e,
						seriesIndex: t
					}),
					onMouseleave: (n) => Hn({
						datapoint: e,
						seriesIndex: t
					}),
					onClick: (n) => Un({
						datapoint: e,
						seriesIndex: t
					})
				}, w(e.name) + " " + w(I.value && L.value.style.chart.labels.yAxisLabels.showProgression ? [null, void 0].includes(e.start) || [null, void 0].includes(e.end) ? "" : `(${T(c)(L.value.style.chart.labels.yAxisLabels.formatter, 100 * (e.end / e.start - 1), T(o)({
					v: 100 * (e.end / e.start - 1),
					s: "%",
					r: L.value.style.chart.labels.yAxisLabels.rounding
				}), { datapoint: e })})` : ""), 41, tt))), 128)), L.value.style.chart.labels.yAxisLabels.showProgression && !I.value ? (b(!0), m(u, { key: 0 }, S(Y.value, (e, t) => (b(), m("text", {
					class: "vue-ui-dumbbell-serie-value",
					x: q.value.left - 6 + L.value.style.chart.labels.yAxisLabels.offsetX,
					y: q.value.top + t * q.value.rowHeight + q.value.rowHeight / 1.3 + L.value.style.chart.labels.yAxisLabels.fontSize / 3,
					"font-size": L.value.style.chart.labels.yAxisLabels.fontSize,
					fill: L.value.style.chart.labels.yAxisLabels.color,
					"text-anchor": "end",
					onMouseenter: (n) => Vn({
						datapoint: e,
						seriesIndex: t
					}),
					onMouseleave: (n) => Hn({
						datapoint: e,
						seriesIndex: t
					}),
					onClick: (n) => Un({
						datapoint: e,
						seriesIndex: t
					})
				}, w([null, void 0].includes(e.start) || [null, void 0].includes(e.end) ? "" : T(c)(L.value.style.chart.labels.yAxisLabels.formatter, 100 * (e.end / e.start - 1), T(o)({
					v: 100 * (e.end / e.start - 1),
					s: "%",
					r: L.value.style.chart.labels.yAxisLabels.rounding
				}), { datapoint: e })), 41, nt))), 256)) : p("", !0)], 512)) : p("", !0),
				L.value.style.chart.labels.axis.xLabel ? (b(), m("text", {
					key: 5,
					ref_key: "xAxisLabel",
					ref: $t,
					x: q.value.left + q.value.width / 2,
					y: q.value.absoluteHeight - L.value.style.chart.labels.axis.fontSize / 3,
					"font-size": L.value.style.chart.labels.axis.fontSize,
					fill: L.value.style.chart.labels.axis.color,
					"text-anchor": "middle"
				}, w(L.value.style.chart.labels.axis.xLabel), 9, rt)) : p("", !0),
				L.value.style.chart.labels.xAxisLabels.show ? (b(), m("g", {
					key: 6,
					ref_key: "scaleLabels",
					ref: Qt
				}, [(b(!0), m(u, null, S(W.value.ticks, (e, t) => (b(), m("text", {
					class: "vue-ui-dumbbell-scale-label",
					key: `tick_${t}`,
					transform: `translate(${q.value.left + t * (q.value.width / (W.value.ticks.length - 1))}, ${q.value.bottom + L.value.style.chart.labels.xAxisLabels.fontSize + L.value.style.chart.labels.xAxisLabels.offsetY}), rotate(${L.value.style.chart.labels.xAxisLabels.rotation})`,
					"font-size": L.value.style.chart.labels.xAxisLabels.fontSize,
					fill: L.value.style.chart.labels.xAxisLabels.color,
					"font-weight": L.value.style.chart.labels.xAxisLabels.bold ? "bold" : "normal",
					"text-anchor": L.value.style.chart.labels.xAxisLabels.rotation > 0 ? "start" : L.value.style.chart.labels.xAxisLabels.rotation < 0 ? "end" : "middle"
				}, w(T(c)(L.value.style.chart.labels.formatter, e, T(o)({
					p: L.value.style.chart.labels.prefix,
					v: e,
					s: L.value.style.chart.labels.suffix,
					r: L.value.style.chart.labels.xAxisLabels.rounding
				}), {
					datapoint: e,
					seriesIndex: t
				})), 9, it))), 128))], 512)) : p("", !0),
				O(h("g", null, [
					O(h("path", {
						d: `M ${P.value ? P.value.startX : q.value.left},${q.value.top} ${P.value ? P.value.startX : q.value.left},${q.value.bottom}`,
						stroke: P.value ? L.value.style.chart.plots.evaluationColors.enable ? P.value.evaluationColor : L.value.style.chart.plots.startColor : "transparent",
						"stroke-width": L.value.style.chart.comparisonLines.strokeWidth,
						"stroke-dasharray": L.value.style.chart.comparisonLines.strokeDasharray,
						style: { transition: "all 0.3s ease-in-out" }
					}, null, 8, at), [[E, P.value !== null && ![null, void 0].includes(P.value.start)]]),
					O(h("path", {
						d: `M ${P.value ? P.value.endX : q.value.left},${q.value.top} ${P.value ? P.value.endX : q.value.left},${q.value.bottom}`,
						stroke: P.value ? L.value.style.chart.plots.evaluationColors.enable ? P.value.evaluationColor : L.value.style.chart.plots.endColor : "transparent",
						"stroke-width": L.value.style.chart.comparisonLines.strokeWidth,
						"stroke-dasharray": L.value.style.chart.comparisonLines.strokeDasharray,
						style: { transition: "all 0.3s ease-in-out" }
					}, null, 8, ot), [[E, P.value !== null && ![null, void 0].includes(P.value.end)]]),
					O(h("rect", {
						x: P.value ? Math.min(P.value.startX, P.value.endX) : q.value.left,
						y: q.value.top,
						height: Math.max(.1, q.value.height),
						width: P.value ? Math.max(.1, Math.abs(P.value.endX - P.value.startX)) : 0,
						fill: P.value ? T(n)(L.value.style.chart.comparisonLines.rectColor, L.value.style.chart.comparisonLines.rectOpacity) : "transparent",
						style: { transition: "all 0.3s ease-in-out" }
					}, null, 8, st), [[E, L.value.style.chart.comparisonLines.showRect && P.value !== null && ![null, void 0].includes(P.value.start) && ![null, void 0].includes(P.value.end)]]),
					O(h("text", {
						transform: `translate(${Wn.value == null ? 0 : Wn.value}, ${q.value.top - 6})`,
						fill: L.value.style.chart.comparisonLines.labelColor,
						"font-size": L.value.style.chart.comparisonLines.labelFontSize,
						"text-anchor": "middle",
						style: { transition: "all 0.3s ease-in-out" }
					}, w(Gn.value), 9, ct), [[E, P.value !== null && Wn.value !== null && L.value.style.chart.comparisonLines.showLabel]])
				], 512), [[E, L.value.style.chart.comparisonLines.show && N.value !== null]]),
				h("defs", null, [
					g(l, {
						t: "radial",
						id: `start_grad_${A.value}`,
						fy: "30%",
						stops: [
							[
								"10%",
								T(a)(L.value.style.chart.plots.startColor, L.value.style.chart.plots.gradient.intensity / 100),
								1
							],
							[
								"90%",
								T(s)(L.value.style.chart.plots.startColor, .1),
								1
							],
							[
								"100%",
								L.value.style.chart.plots.startColor,
								1
							]
						]
					}, null, 8, ["id", "stops"]),
					g(l, {
						t: "radial",
						id: `end_grad_${A.value}`,
						fy: "30%",
						stops: [
							[
								"10%",
								T(a)(L.value.style.chart.plots.endColor, L.value.style.chart.plots.gradient.intensity / 100),
								1
							],
							[
								"90%",
								T(s)(L.value.style.chart.plots.endColor, .1),
								1
							],
							[
								"100%",
								L.value.style.chart.plots.endColor,
								1
							]
						]
					}, null, 8, ["id", "stops"]),
					g(l, {
						t: "radial",
						id: `positive_grad_${A.value}`,
						fy: "30%",
						stops: [
							[
								"10%",
								T(a)(L.value.style.chart.plots.evaluationColors.positive, L.value.style.chart.plots.gradient.intensity / 100),
								1
							],
							[
								"90%",
								T(s)(L.value.style.chart.plots.evaluationColors.positive, .1),
								1
							],
							[
								"100%",
								L.value.style.chart.plots.evaluationColors.positive,
								1
							]
						]
					}, null, 8, ["id", "stops"]),
					g(l, {
						t: "radial",
						id: `negative_grad_${A.value}`,
						fy: "30%",
						stops: [
							[
								"10%",
								T(a)(L.value.style.chart.plots.evaluationColors.negative, L.value.style.chart.plots.gradient.intensity / 100),
								1
							],
							[
								"90%",
								T(s)(L.value.style.chart.plots.evaluationColors.negative, .1),
								1
							],
							[
								"100%",
								L.value.style.chart.plots.evaluationColors.negative,
								1
							]
						]
					}, null, 8, ["id", "stops"]),
					g(l, {
						t: "radial",
						id: `neutral_grad_${A.value}`,
						fy: "30%",
						stops: [
							[
								"10%",
								T(a)(L.value.style.chart.plots.evaluationColors.neutral, L.value.style.chart.plots.gradient.intensity / 100),
								1
							],
							[
								"90%",
								T(s)(L.value.style.chart.plots.evaluationColors.neutral, .1),
								1
							],
							[
								"100%",
								L.value.style.chart.plots.evaluationColors.neutral,
								1
							]
						]
					}, null, 8, ["id", "stops"])
				]),
				(b(!0), m(u, null, S(Y.value, (e, t) => (b(), m("g", { key: `plot_${t}_${e.id}` }, [
					h("defs", null, [g(l, {
						t: "linear",
						id: `grad_pos_${A.value}`,
						x1: "0%",
						x2: "100%",
						y1: "0%",
						y2: "0%",
						stops: [[
							"0%",
							L.value.style.chart.plots.startColor,
							1
						], [
							"100%",
							L.value.style.chart.plots.endColor,
							1
						]]
					}, null, 8, ["id", "stops"]), g(l, {
						t: "linear",
						id: `grad_neg_${A.value}`,
						x1: "0%",
						x2: "100%",
						y1: "0%",
						y2: "0%",
						stops: [[
							"0%",
							L.value.style.chart.plots.endColor,
							1
						], [
							"100%",
							L.value.style.chart.plots.startColor,
							1
						]]
					}, null, 8, ["id", "stops"])]),
					![void 0, null].includes(e.end) && ![void 0, null].includes(e.start) ? (b(), m("g", lt, [L.value.style.chart.plots.link.type === "curved" ? (b(), m("g", ut, [h("path", {
						d: `M 
                                    ${e.startX},${e.y + J.value / 2} 
                                    C ${e.centerX},${e.y} ${e.centerX},${e.y} 
                                    ${e.endX},${e.y + J.value / 2}
                                    L ${e.endX},${e.y - J.value / 2}
                                    C ${e.centerX},${e.y} ${e.centerX},${e.y}
                                    ${e.startX},${e.y - J.value / 2}
                                    Z
                                `,
						fill: L.value.style.chart.plots.evaluationColors.enable ? e.evaluationColor : e.endX > e.startX ? `url(#grad_pos_${A.value})` : `url(#grad_neg_${A.value})`
					}, null, 8, dt)])) : (b(), m("g", ft, [h("rect", {
						x: e.endX > e.startX ? e.startX : e.endX,
						y: e.y - L.value.style.chart.plots.link.strokeWidth / 2,
						height: Math.max(.01, L.value.style.chart.plots.link.strokeWidth),
						width: Math.max(.01, Math.abs(e.endX - e.startX)),
						fill: L.value.style.chart.plots.evaluationColors.enable ? e.evaluationColor : e.endX > e.startX ? `url(#grad_pos_${A.value})` : `url(#grad_neg_${A.value})`
					}, null, 8, pt)]))])) : p("", !0),
					[null, void 0].includes(e.start) ? p("", !0) : (b(), m("circle", {
						key: 1,
						cx: e.startX,
						cy: e.y,
						r: J.value,
						fill: L.value.style.chart.plots.gradient.show ? L.value.style.chart.plots.evaluationColors.enable ? e.evaluationGrad : `url(#start_grad_${A.value})` : L.value.style.chart.plots.evaluationColors.enable ? e.evaluationColor : L.value.style.chart.plots.startColor,
						stroke: L.value.style.chart.plots.stroke,
						"stroke-width": L.value.style.chart.plots.strokeWidth
					}, null, 8, mt)),
					[null, void 0].includes(e.end) ? p("", !0) : (b(), m("circle", {
						key: 2,
						cx: e.endX,
						cy: e.y,
						r: J.value,
						fill: L.value.style.chart.plots.gradient.show ? L.value.style.chart.plots.evaluationColors.enable ? e.evaluationGrad : `url(#end_grad_${A.value})` : L.value.style.chart.plots.evaluationColors.enable ? e.evaluationColor : L.value.style.chart.plots.endColor,
						stroke: L.value.style.chart.plots.stroke,
						"stroke-width": L.value.style.chart.plots.strokeWidth
					}, null, 8, ht))
				]))), 128)),
				L.value.style.chart.labels.startLabels.show ? (b(), m("g", gt, [(b(!0), m(u, null, S(Y.value, (e, t) => (b(), m("g", { key: `start_label_${t}_${e.id}` }, [[null, void 0].includes(e.start) ? p("", !0) : (b(), m("text", {
					key: 0,
					x: e.startX,
					y: e.y + J.value * 2 + L.value.style.chart.labels.startLabels.fontSize / 2,
					fill: L.value.style.chart.plots.evaluationColors.enable && L.value.style.chart.labels.startLabels.useEvaluationColor ? e.evaluationColor : L.value.style.chart.labels.startLabels.useStartColor ? L.value.style.chart.plots.startColor : L.value.style.chart.labels.startLabels.color,
					"font-size": L.value.style.chart.labels.startLabels.fontSize,
					"text-anchor": "middle"
				}, w(T(c)(L.value.style.chart.labels.formatter, e.start, T(o)({
					p: L.value.style.chart.labels.prefix,
					v: e.start,
					s: L.value.style.chart.labels.suffix,
					r: L.value.style.chart.labels.startLabels.rounding
				}), {
					datapoint: e,
					seriesIndex: t
				})), 9, _t))]))), 128))])) : p("", !0),
				L.value.style.chart.labels.endLabels.show ? (b(), m("g", vt, [(b(!0), m(u, null, S(Y.value, (e, t) => (b(), m("g", { key: `end_label_${t}_${e.id}` }, [[null, void 0].includes(e.end) ? p("", !0) : (b(), m("text", {
					key: 0,
					x: e.endX,
					y: e.y - (J.value * 2 - L.value.style.chart.labels.startLabels.fontSize / 3),
					fill: L.value.style.chart.plots.evaluationColors.enable && L.value.style.chart.labels.endLabels.useEvaluationColor ? e.evaluationColor : L.value.style.chart.labels.endLabels.useEndColor ? L.value.style.chart.plots.endColor : L.value.style.chart.labels.endLabels.color,
					"font-size": L.value.style.chart.labels.endLabels.fontSize,
					"text-anchor": "middle"
				}, w(T(c)(L.value.style.chart.labels.formatter, e.end, T(o)({
					p: L.value.style.chart.labels.prefix,
					v: e.end,
					s: L.value.style.chart.labels.suffix,
					r: L.value.style.chart.labels.endLabels.rounding
				}), {
					datapoint: e,
					seriesIndex: t
				})), 9, yt))]))), 128))])) : p("", !0),
				h("g", null, [(b(!0), m(u, null, S(Y.value, (e, t) => (b(), m("rect", {
					x: q.value.left,
					y: q.value.top + t * Math.max(.1, q.value.rowHeight),
					width: Math.max(.1, q.value.width),
					height: Math.max(.1, q.value.rowHeight),
					fill: N.value === null ? "transparent" : N.value === t ? T(n)(L.value.style.chart.highlighter.color, L.value.style.chart.highlighter.opacity) : "transparent",
					onMouseenter: (n) => Vn({
						datapoint: e,
						seriesIndex: t
					}),
					onMouseleave: (n) => Hn({
						datapoint: e,
						seriesIndex: t
					}),
					onClick: (n) => Un({
						datapoint: e,
						seriesIndex: t
					})
				}, null, 40, bt))), 256))]),
				C(e.$slots, "svg", { svg: {
					...q.value,
					isPrintingImg: T(fn) || T(pn) || T(Qn),
					isPrintingSvg: T($n)
				} }, void 0, !0)
			], 46, qe)), e.$slots.hint ? (b(), m("div", xt, [C(e.$slots, "hint", y(v({
				hint: L.value.a11y.translations.keyboardNavigation,
				isVisible: en.value
			})), void 0, !0)])) : p("", !0)]),
			e.$slots.watermark ? (b(), m("div", St, [C(e.$slots, "watermark", y(v({ isPrinting: T(fn) || T(pn) || T(Qn) || T($n) })), void 0, !0)])) : p("", !0),
			h("div", { id: `legend-bottom-${A.value}` }, null, 8, Ct),
			Jt.value && (L.value.style.chart.legend.show || e.$slots.legend) ? (b(), f(Oe, {
				key: 6,
				to: L.value.style.chart.legend.position === "top" ? `#legend-top-${A.value}` : `#legend-bottom-${A.value}`
			}, [h("div", {
				ref_key: "chartLegend",
				ref: Ht
			}, [C(e.$slots, "legend", { legend: Dn.value }, () => [L.value.style.chart.legend.show && zt.value ? (b(), f(Te, {
				key: `legend_${qt.value}`,
				legendSet: Dn.value,
				config: On.value,
				clickable: !1
			}, {
				item: D(({ legend: e }) => [h("div", { style: Pe(`display:flex;align-items:center;gap:4px;font-size:${L.value.style.chart.legend.fontSize}px`) }, [(b(), m("svg", {
					xmlns: T(ae),
					viewBox: "0 0 20 20",
					height: L.value.style.chart.legend.fontSize,
					width: L.value.style.chart.legend.fontSize
				}, [h("circle", {
					cx: 10,
					cy: 10,
					r: 9,
					fill: e.color
				}, null, 8, Tt)], 8, wt)), T(nn) ? p("", !0) : (b(), m(u, { key: 0 }, [Ae(w(e.name), 1)], 64))], 4)]),
				_: 1
			}, 8, ["legendSet", "config"])) : p("", !0)], !0)], 512)], 8, ["to"])) : p("", !0),
			e.$slots.source ? (b(), m("div", {
				key: 7,
				ref_key: "source",
				ref: Ut,
				dir: "auto"
			}, [C(e.$slots, "source", {}, void 0, !0)], 512)) : p("", !0),
			zt.value && L.value.userOptions.buttons.table ? (b(), f(Le(Kn.value.component), je({ key: 8 }, Kn.value.props, {
				ref_key: "tableUnit",
				ref: M,
				onClose: qn
			}), ke({
				content: D(() => [(b(), f(T(kt), {
					key: `table_${Kt.value}`,
					colNames: Q.value.colNames,
					head: Q.value.head,
					body: Q.value.body,
					config: Q.value.config,
					title: L.value.table.useDialog ? "" : Kn.value.title,
					withCloseButton: !L.value.table.useDialog,
					isCursorPointer: R.value,
					onClose: qn
				}, {
					th: D(({ th: e }) => [h("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, Et)]),
					td: D(({ td: e }) => [Ae(w(e.name || e), 1)]),
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
			}, [L.value.table.useDialog ? {
				name: "title",
				fn: D(() => [Ae(w(Kn.value.title), 1)]),
				key: "0"
			} : void 0, L.value.table.useDialog ? {
				name: "actions",
				fn: D(() => [h("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[0] ||= (e) => kn(L.value.userOptions.callbacks.csv),
					style: Pe({ cursor: R.value ? "pointer" : "default" })
				}, [g(T(Dt), {
					name: "fileCsv",
					stroke: Kn.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : p("", !0),
			C(e.$slots, "skeleton", {}, () => [T(nn) ? (b(), f(fe, { key: 0 })) : p("", !0)], !0)
		], 46, Ue));
	}
}, [["__scopeId", "data-v-5ce0f3c4"]]);
//#endregion
export { He as n, Dt as t };
