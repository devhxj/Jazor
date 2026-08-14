import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, At as n, Bt as r, D as i, Jt as a, M as ee, Vt as te, X as o, i as s, jt as ne, q as re, t as ie, tt as ae } from "./lib-Bttd6u5E.js";
import { n as oe, t as se } from "./useHints-Dq_w2E8B.js";
import { t as ce } from "./useConfig-DlNpz6P8.js";
import { t as le } from "./usePrinter-DN5bYhTG.js";
import { n as ue, t as de } from "./BaseScanner-DZvpgOjM.js";
import { t as fe } from "./useNestedProp-vPNvh7rV.js";
import { t as pe } from "./useThemeCheck-C43Tcqmk.js";
import { t as me } from "./useChartExport-DNiwdPmb.js";
import { t as he } from "./img-Bnokohej.js";
import { n as ge } from "./Title-BE3qg9xl.js";
import { t as _e } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ve, t as ye } from "./useResponsive-ZtArZtUf.js";
import { t as be } from "./DefGrad-DVBqDjhO.js";
import { t as xe } from "./A11yDataTable-DdRsVULz.js";
import { t as Se } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ce } from "./useChartAccessibility-DYqac8yF.js";
import { t as we } from "./Legend-CQxUgOd-.js";
import { t as Te } from "./vue_ui_mood_radar-BA6LAKhk.js";
import { Fragment as Ee, Teleport as De, computed as c, createBlock as l, createCommentVNode as u, createElementBlock as d, createElementVNode as f, createSlots as Oe, createTextVNode as ke, createVNode as p, defineAsyncComponent as m, guardReactiveProps as h, mergeProps as Ae, nextTick as je, normalizeClass as Me, normalizeProps as g, normalizeStyle as _, onMounted as Ne, openBlock as v, ref as y, renderList as Pe, renderSlot as b, resolveDynamicComponent as Fe, toDisplayString as x, toRefs as Ie, unref as S, watch as Le, withCtx as C } from "vue";
//#region src/components/vue-ui-mood-radar.vue
var Re = /* @__PURE__ */ e({ default: () => w }), ze = ["id"], Be = ["id"], Ve = ["id"], He = { style: { position: "relative" } }, Ue = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], We = ["width", "height"], Ge = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], Ke = [
	"d",
	"stroke",
	"stroke-width"
], qe = ["transform"], Je = ["stroke"], Ye = ["aria-label", "fill"], Xe = ["transform"], Ze = ["stroke"], Qe = ["aria-label", "fill"], $e = ["transform"], et = ["stroke"], tt = ["aria-label", "fill"], nt = ["transform"], rt = ["stroke"], it = ["aria-label", "fill"], at = ["transform"], ot = ["stroke"], st = ["aria-label", "fill"], ct = [
	"d",
	"stroke",
	"stroke-width",
	"fill"
], lt = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke"
], ut = [
	"cx",
	"cy",
	"fill",
	"stroke"
], dt = [
	"cx",
	"cy",
	"fill",
	"stroke"
], ft = [
	"x",
	"y",
	"fill",
	"font-weight"
], pt = [
	"x",
	"y",
	"fill"
], mt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, ht = {
	key: 5,
	class: "vue-data-ui-watermark"
}, gt = ["id"], _t = ["onClick", "onKeydown"], vt = ["innerHTML"], w = /*#__PURE__*/ _e({
	__name: "vue-ui-mood-radar",
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
	setup(e, { expose: _e, emit: Re }) {
		let w = m(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), yt = m(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), bt = m(() => import("./DataTable-BbKgJ5UI.js")), xt = m(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), St = m(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Ct = m(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), wt = m(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_mood_radar: Tt } = ce(), { isThemeValid: Et, warnInvalidTheme: Dt } = pe(), T = e, Ot = Re, E = y(re()), D = y(null), O = y(null), kt = y(null), At = y(0), jt = y(0), Mt = y(0), Nt = y(null), Pt = y(null), k = y(null), A = y(null), Ft = y(null), It = y(!1), j = y(null), Lt = y(null), M = y(null), N = y(!1), Rt = c(() => !!T.dataset && Object.keys(T.dataset).length), P = y(Kt());
		oe({
			config: () => P.value,
			dataset: () => T.dataset,
			component: "VueUiMoodRadar",
			rules: [se.noHint]
		});
		let F = c(() => P.value.userOptions.useCursorPointer), zt = c(() => a({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						grid: { stroke: "#6A6A6A" },
						outerPolygon: { stroke: "#6A6A6A" },
						dataPolygon: {
							color: "#6A6A6A",
							opacity: 30,
							stroke: "#6A6A6A"
						},
						smileys: { colors: {
							1: "#DBDBDB",
							2: "#C4C4C4",
							3: "#ADADAD",
							4: "#969696",
							5: "#808080"
						} },
						dataLabel: { color: "transparent" }
					},
					legend: { backgroundColor: "transparent" }
				} }
			},
			userConfig: P.value.skeletonConfig ?? {}
		})), { loading: Bt, FINAL_DATASET: I } = ue({
			...Ie(T),
			FINAL_CONFIG: P,
			prepareConfig: Kt,
			callback: () => {
				Promise.resolve().then(async () => {
					await je(), R.value.showTable = P.value.table.show;
				});
			},
			skeletonDataset: T.config?.skeletonDataset ?? {
				1: 1,
				2: 1,
				3: 1,
				4: 1,
				5: 1
			},
			skeletonConfig: a({
				defaultConfig: P.value,
				userConfig: zt.value
			})
		});
		Ne(() => {
			It.value = !0, Ht();
		});
		let Vt = c(() => P.value.debug);
		function Ht() {
			if (ne(T.dataset) && ae({
				componentName: "VueUiMoodRadar",
				type: "dataset",
				debug: Vt.value
			}), P.value.responsive) {
				let e = ve(() => {
					let { width: e, height: t } = ye({
						chart: D.value,
						title: P.value.style.chart.title.text ? Nt.value : null,
						legend: P.value.style.chart.legend.show ? Pt.value : null,
						noTitle: kt.value,
						source: Ft.value
					});
					requestAnimationFrame(() => {
						z.value.width = e, z.value.height = t - 12;
					});
				});
				k.value && (A.value && k.value.unobserve(A.value), k.value.disconnect()), k.value = new ResizeObserver(e), A.value = D.value.parentNode, k.value.observe(A.value);
			}
		}
		let { userOptionsVisible: Ut, setUserOptionsVisibility: Wt, keepUserOptionState: Gt } = Se({ config: P.value }), { svgRef: L } = Ce({ config: P.value.style.chart.title });
		function Kt() {
			let e = fe({
				userConfig: T.config,
				defaultConfig: Tt
			}), t = e.theme;
			if (!t) return e;
			if (!Et.value(e)) return Dt(e), e;
			let n = fe({
				userConfig: Te[t] || T.config,
				defaultConfig: e
			});
			return fe({
				userConfig: T.config,
				defaultConfig: n
			});
		}
		Le(() => T.config, (e) => {
			P.value = Kt(), Ut.value = !P.value.userOptions.showOnChartHover, Ht(), At.value += 1, jt.value += 1, Mt.value += 1, R.value.showTable = P.value.table.show;
		}, { deep: !0 });
		let { isPrinting: qt, isImaging: Jt, generatePdf: Yt, generateImage: Xt } = le({
			elementId: E.value,
			fileName: P.value.style.chart.title.text || "vue-ui-mood-radar",
			options: P.value.userOptions.print
		}), Zt = c(() => P.value.userOptions.show && !P.value.style.chart.title.text), R = y({ showTable: P.value.table.show }), z = y({
			height: 256,
			width: 256
		}), Qt = {
			5: {
				x: 128,
				y: 35
			},
			4: {
				x: 218,
				y: 98.5
			},
			3: {
				x: 185,
				y: 204
			},
			2: {
				x: 70,
				y: 204
			},
			1: {
				x: 38.5,
				y: 98.5
			}
		}, $t = c(() => {
			let e = {};
			return an.value.forEach((t) => {
				e[t.key] = {
					x: t.x,
					y: t.y
				};
			}), e;
		});
		function B(e) {
			let t = Qt[e], n = $t.value[e] || t;
			return `translate(${n.x - t.x}, ${n.y - t.y})`;
		}
		let en = c(() => ee({
			plot: {
				x: z.value.width / 2,
				y: z.value.height / 2
			},
			radius: Math.min(z.value.height, z.value.width) * .35,
			sides: 5,
			rotation: 11
		}));
		function tn({ centerX: e, centerY: t, apexX: n, apexY: r, proportion: i, key: a, value: ee }) {
			return {
				x: e + (n - e) * i,
				y: t + (r - t) * i,
				key: a,
				value: ee
			};
		}
		let nn = c(() => Math.max(...Object.values(I.value).map((e) => isNaN(e) ? 0 : e))), V = c(() => Object.values(I.value).reduce((e, t) => (isNaN(e) ? 0 : e) + (isNaN(t) ? 0 : t), 0)), H = c(() => Object.keys(I.value).map((e, t) => {
			let n = typeof I.value[e] != "number" || isNaN(I.value[e]) ? 0 : I.value[e];
			return {
				index: t,
				key: e,
				value: n,
				proportion: n / V.value,
				color: P.value.style.chart.layout.smileys.colors[e]
			};
		}).map((e) => ({
			...e,
			onSelect: () => U(e.key)
		})).sort((e, t) => t.key - e.key)), rn = c(() => H.value.map((e, t) => ({
			...e,
			display: `${s(P.value.style.chart.layout.dataLabel.formatter, e.value, o({
				p: P.value.style.chart.layout.dataLabel.prefix,
				v: e.value,
				s: P.value.style.chart.layout.dataLabel.suffix,
				r: P.value.style.chart.layout.dataLabel.roundingValue
			}))}${Bt.value ? "" : ` (${o({
				v: e.proportion * 100,
				s: "%",
				r: P.value.style.chart.legend.roundingPercentage
			})})`}`
		}))), an = c(() => ([
			"1",
			"2",
			"3",
			"4",
			"5"
		].forEach((e) => {
			[null, void 0].includes(I.value[e]) && ae({
				componentName: "VueUiMoodRadar",
				type: "datasetAttribute",
				property: e,
				debug: Vt.value
			});
		}), en.value.coordinates.map((e, t) => {
			let n = tn({
				centerX: z.value.width / 2,
				centerY: z.value.height / 2,
				apexX: e.x,
				apexY: e.y,
				proportion: H.value[t].value / nn.value,
				key: H.value[t].key,
				value: H.value[t].value
			});
			return {
				...e,
				plots: n,
				key: H.value[t].key
			};
		}))), on = c(() => ({
			cy: "mood-radar-legend",
			backgroundColor: P.value.style.chart.legend.backgroundColor,
			color: P.value.style.chart.legend.color,
			fontSize: P.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: P.value.style.chart.legend.bold ? "bold" : ""
		}));
		function U(e) {
			e === O.value ? O.value = null : (O.value = e, K(e));
		}
		function W(e) {
			O.value = e;
			let t = H.value.find((t) => t.key === e);
			P.value.events.datapointEnter && P.value.events.datapointEnter({
				datapoint: t,
				seriesIndex: t.index
			});
		}
		function G(e) {
			O.value = null;
			let t = H.value.find((t) => t.key === e);
			P.value.events.datapointLeave && P.value.events.datapointLeave({
				datapoint: t,
				seriesIndex: t.index
			});
		}
		function K(e) {
			let t = H.value.find((t) => t.key === e);
			P.value.events.datapointClick && P.value.events.datapointClick({
				datapoint: t,
				seriesIndex: t.index
			});
		}
		let q = c(() => ({
			head: H.value.map((e) => ({
				name: e.key,
				color: e.color
			})),
			body: H.value.map((e) => isNaN(e.value) ? 0 : e.value)
		}));
		function sn(e = null) {
			je(() => {
				let n = q.value.head.map((e, t) => [
					[e.name],
					[q.value.body[t]],
					[isNaN(q.value.body[t] / V.value) ? "-" : q.value.body[t] / V.value * 100]
				]), r = [
					[P.value.style.chart.title.text],
					[P.value.style.chart.title.subtitle.text],
					[
						[""],
						["val"],
						["%"]
					]
				].concat(n), a = i(r);
				e ? e(a) : t({
					csvContent: a,
					title: P.value.style.chart.title.text || "vue-ui-mood-radar"
				});
			});
		}
		let J = c(() => ({
			head: [
				" <svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2\" /></svg>",
				Number(V.value.toFixed(P.value.table.td.roundingValue)).toLocaleString(),
				"100%"
			],
			body: q.value.head.map((e, t) => [
				{
					color: e.color,
					name: e.name
				},
				q.value.body[t].toFixed(P.value.table.td.roundingValue),
				isNaN(q.value.body[t] / V.value) ? "-" : (q.value.body[t] / V.value * 100).toFixed(P.value.table.td.roundingPercentage) + "%"
			]),
			config: {
				th: {
					backgroundColor: P.value.table.th.backgroundColor,
					color: P.value.table.th.color,
					outline: P.value.table.th.outline
				},
				td: {
					backgroundColor: P.value.table.td.backgroundColor,
					color: P.value.table.td.color,
					outline: P.value.table.td.outline
				},
				breakpoint: P.value.table.responsiveBreakpoint
			},
			colNames: [
				P.value.table.columnNames.series,
				P.value.table.columnNames.value,
				P.value.table.columnNames.percentage
			]
		}));
		function cn() {
			return H.value;
		}
		function ln() {
			R.value.showTable = !R.value.showTable;
		}
		let Y = y(!1);
		function un(e) {
			Y.value = e;
		}
		let X = y(!1);
		function dn() {
			X.value = !X.value;
		}
		async function fn({ scale: e = 2 } = {}) {
			if (!D.value) return;
			let { width: t, height: n } = D.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await he({
				domElement: D.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: P.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Z = c(() => {
			let e = P.value.table.useDialog && !P.value.table.show, t = R.value.showTable;
			return {
				component: e ? wt : yt,
				title: `${P.value.style.chart.title.text}${P.value.style.chart.title.subtitle.text ? `: ${P.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: P.value.table.th.backgroundColor,
					color: P.value.table.th.color,
					headerColor: P.value.table.th.color,
					headerBg: P.value.table.th.backgroundColor,
					isFullscreen: Y.value,
					fullscreenParent: D.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: F.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: P.value.style.chart.backgroundColor,
							color: P.value.style.chart.color
						},
						head: {
							backgroundColor: P.value.style.chart.backgroundColor,
							color: P.value.style.chart.color
						}
					}
				}
			};
		});
		Le(() => R.value.showTable, (e) => {
			P.value.table.show || (e && P.value.table.useDialog && j.value ? j.value.open() : "close" in j.value && j.value.close());
		});
		function pn() {
			R.value.showTable = !1, Lt.value && Lt.value.setTableIconState(!1);
		}
		let mn = c(() => rn.value.map((e) => ({
			...e,
			name: e.display,
			shape: "circle"
		}))), hn = c(() => P.value.style.chart.backgroundColor), gn = c(() => P.value.style.chart.legend), _n = c(() => P.value.style.chart.title), { isCallbackImaging: vn, isCallbackSvg: yn, generateSvg: bn, onGenerateImage: xn } = me({
			svg: L,
			title: _n,
			legend: gn,
			legendItems: mn,
			backgroundColor: hn,
			getSvgCallback: () => P.value.userOptions.callbacks.svg,
			generateImage: Xt
		});
		async function Sn() {
			if (Ot("copyAlt", {
				config: P.value,
				dataset: H.value
			}), !P.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(P.value.userOptions.callbacks.altCopy({
				config: P.value,
				dataset: H.value
			}));
		}
		let Q = c(() => H.value.map((e) => String(e.key)));
		function Cn() {
			Wt(!0);
		}
		function wn() {
			Wt(!1);
		}
		function Tn(e) {
			return H.value.find((t) => String(t.key) === String(e));
		}
		function En(e, t, n) {
			return s(P.value.style.chart.layout.dataLabel.formatter, e, o({
				p: P.value.style.chart.layout.dataLabel.prefix,
				v: e,
				s: P.value.style.chart.layout.dataLabel.suffix,
				r: P.value.style.chart.layout.dataLabel.roundingValue
			}), {
				datapoint: n,
				seriesIndex: t
			});
		}
		function Dn(e) {
			return o({
				v: V.value ? e / V.value * 100 : 0,
				s: "%",
				r: P.value.style.chart.layout.dataLabel.roundingPercentage
			});
		}
		function $(e) {
			let t = Tn(e);
			return t ? `${e}, ${En(t.value, t.index, t)}, ${Dn(t.value)}` : `${e}`;
		}
		function On(e) {
			let t = Q.value[e];
			t && (M.value = e, O.value = t);
		}
		function kn() {
			N.value = !0;
		}
		function An() {
			M.value = null, O.value = null, N.value = !1;
		}
		function jn(e) {
			if (!L.value || !Q.value.length || X.value || document.activeElement !== L.value) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				M.value = null, O.value = null;
				return;
			}
			if (r) {
				if (M.value === null) return;
				let e = Q.value[M.value];
				if (!e) return;
				U(e);
				return;
			}
			let a = M.value;
			a === null || a < 0 || a >= Q.value.length ? a = n ? 0 : Q.value.length - 1 : n ? (a += 1, a >= Q.value.length && (a = 0)) : t && (--a, a < 0 && (a = Q.value.length - 1)), On(a);
		}
		function Mn(e, t) {
			let n = e.key === "Enter" || e.key === " ", r = e.key === "Escape";
			if (!(!n && !r)) {
				if (e.preventDefault(), e.stopPropagation(), r) {
					M.value = null, O.value = null;
					return;
				}
				U(String(t));
			}
		}
		let Nn = c(() => ({
			headers: J.value?.colNames ?? [],
			rows: H.value.map((e) => [
				String(e.key),
				En(e.value, e.index, e),
				Dn(e.value)
			])
		}));
		return _e({
			getData: cn,
			getImage: fn,
			generatePdf: Yt,
			generateCsv: sn,
			generateImage: Xt,
			generateSvg: bn,
			toggleTable: ln,
			toggleAnnotator: dn,
			toggleFullscreen: un,
			copyAlt: Sn
		}), (e, t) => (v(), d("div", {
			class: Me(`vue-data-ui-component vue-ui-mood-radar ${Y.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${P.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			ref_key: "moodRadarChart",
			ref: D,
			id: `${E.value}`,
			style: _(`font-family:${P.value.style.fontFamily};width:100%; text-align:center;background:${P.value.style.chart.backgroundColor}`),
			onMouseenter: Cn,
			onMouseleave: wn
		}, [
			f("div", {
				id: `chart-instructions-${E.value}`,
				class: "sr-only"
			}, [f("p", null, x(P.value.a11y.translations.keyboardNavigation), 1)], 8, Be),
			Nn.value?.rows?.length ? (v(), l(xe, {
				key: 0,
				uid: E.value,
				head: Nn.value.headers,
				body: Nn.value.rows,
				notice: P.value.a11y.translations.tableAvailable,
				caption: P.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : u("", !0),
			P.value.userOptions.buttons.annotator ? (v(), l(S(xt), {
				key: 1,
				svgRef: S(L),
				backgroundColor: P.value.style.chart.backgroundColor,
				color: P.value.style.chart.color,
				active: X.value,
				isCursorPointer: F.value,
				onClose: dn
			}, {
				"annotator-action-close": C(() => [b(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": C(({ color: t }) => [b(e.$slots, "annotator-action-color", g(h({ color: t })), void 0, !0)]),
				"annotator-action-draw": C(({ mode: t }) => [b(e.$slots, "annotator-action-draw", g(h({ mode: t })), void 0, !0)]),
				"annotator-action-undo": C(({ disabled: t }) => [b(e.$slots, "annotator-action-undo", g(h({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": C(({ disabled: t }) => [b(e.$slots, "annotator-action-redo", g(h({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": C(({ disabled: t }) => [b(e.$slots, "annotator-action-delete", g(h({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : u("", !0),
			Zt.value ? (v(), d("div", {
				key: 2,
				ref_key: "noTitle",
				ref: kt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : u("", !0),
			P.value.style.chart.title.text ? (v(), d("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Nt,
				style: "width:100%;background:transparent"
			}, [p(ge, { config: {
				title: {
					cy: "mood-radar-title",
					...P.value.style.chart.title
				},
				subtitle: {
					cy: "mood-radar-subtitle",
					...P.value.style.chart.title.subtitle
				}
			} }, null, 8, ["config"])], 512)) : u("", !0),
			f("div", { id: `legend-top-${E.value}` }, null, 8, Ve),
			P.value.userOptions.show && Rt.value && (S(Gt) || S(Ut)) ? (v(), l(S(St), {
				key: 4,
				ref_key: "userOptionsRef",
				ref: Lt,
				backgroundColor: P.value.style.chart.backgroundColor,
				color: P.value.style.chart.color,
				isPrinting: S(qt),
				isImaging: S(Jt),
				uid: E.value,
				hasPdf: P.value.userOptions.buttons.pdf,
				hasXls: P.value.userOptions.buttons.csv,
				hasImg: P.value.userOptions.buttons.img,
				hasSvg: P.value.userOptions.buttons.svg,
				hasTable: P.value.userOptions.buttons.table,
				hasFullscreen: P.value.userOptions.buttons.fullscreen,
				hasAltCopy: P.value.userOptions.buttons.altCopy,
				isFullscreen: Y.value,
				titles: { ...P.value.userOptions.buttonTitles },
				chartElement: D.value,
				position: P.value.userOptions.position,
				hasAnnotator: P.value.userOptions.buttons.annotator,
				isAnnotation: X.value,
				callbacks: P.value.userOptions.callbacks,
				printScale: P.value.userOptions.print.scale,
				tableDialog: P.value.table.useDialog,
				isCursorPointer: F.value,
				onToggleFullscreen: un,
				onGeneratePdf: S(Yt),
				onGenerateCsv: sn,
				onGenerateImage: S(xn),
				onGenerateSvg: S(bn),
				onToggleTable: ln,
				onToggleAnnotator: dn,
				onCopyAlt: Sn,
				style: _({ visibility: S(Gt) ? S(Ut) ? "visible" : "hidden" : "visible" })
			}, Oe({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: C(({ isOpen: t, color: n }) => [b(e.$slots, "menuIcon", g(h({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: C(() => [b(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: C(() => [b(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: C(() => [b(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: C(() => [b(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: C(() => [b(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: C(({ toggleFullscreen: t, isFullscreen: n }) => [b(e.$slots, "optionFullscreen", g(h({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: C(({ toggleAnnotator: t, isAnnotator: n }) => [b(e.$slots, "optionAnnotator", g(h({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: C(({ altCopy: t }) => [b(e.$slots, "optionAltCopy", g(h({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: C(() => [b(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: C(() => [b(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasFullscreen.hasAltCopy.isFullscreen.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : u("", !0),
			f("div", He, [(v(), d("svg", {
				ref_key: "svgRef",
				ref: L,
				xmlns: S(ie),
				"aria-describedby": `chart-instructions-${E.value}`,
				viewBox: `0 0 ${z.value.width} ${z.value.height}`,
				class: Me({
					"vue-data-ui-fullscreen--on": Y.value,
					"vue-data-ui-fulscreen--off": !Y.value
				}),
				style: _(`overflow:visible;background:transparent;color:${P.value.style.chart.color}`),
				tabindex: "0",
				onFocus: kn,
				onBlur: An,
				onKeydown: jn
			}, [
				p(S(Ct)),
				e.$slots["chart-background"] ? (v(), d("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: z.value.width,
					height: z.value.height,
					style: { pointerEvents: "none" }
				}, [b(e.$slots, "chart-background", {}, void 0, !0)], 8, We)) : u("", !0),
				f("defs", null, [p(be, {
					t: "radial",
					cx: "50%",
					cy: "50%",
					r: "50%",
					fx: "50%",
					fy: "50%",
					id: `mood_radar_gradient_${E.value}`,
					stops: [[
						"0%",
						S(r)(P.value.style.chart.layout.dataPolygon.color, P.value.style.chart.layout.dataPolygon.opacity),
						1
					], [
						"100%",
						S(r)(S(te)(P.value.style.chart.layout.dataPolygon.color, P.value.style.chart.layout.dataPolygon.gradient.intensity / 100), P.value.style.chart.layout.dataPolygon.opacity),
						1
					]]
				}, null, 8, ["id", "stops"])]),
				(v(!0), d(Ee, null, Pe(en.value.coordinates, (e) => (v(), d("line", {
					x1: z.value.width / 2,
					y1: z.value.height / 2,
					x2: e.x,
					y2: e.y,
					stroke: P.value.style.chart.layout.grid.stroke,
					"stroke-width": P.value.style.chart.layout.grid.strokeWidth
				}, null, 8, Ge))), 256)),
				f("path", {
					d: en.value.path,
					fill: "none",
					stroke: P.value.style.chart.layout.outerPolygon.stroke,
					"stroke-width": P.value.style.chart.layout.outerPolygon.strokeWidth,
					"stroke-linejoin": "round",
					"stroke-linecap": "round"
				}, null, 8, Ke),
				f("g", { transform: B("5") }, [f("path", {
					fill: "none",
					stroke: P.value.style.chart.layout.smileys.colors[5],
					"stroke-width": "1",
					"stroke-linecap": "round",
					d: "M119 25A1 1 0 00137 25 1 1 0 00119 25M123 26C124 33 132 33 133 26L123 26M123 22A1 1 0 00126 22 1 1 0 00123 22M130 22A1 1 0 00133 22 1 1 0 00130 22"
				}, null, 8, Je), f("circle", {
					role: "button",
					class: "vue-ui-mood-radar-trap",
					cx: "128",
					cy: "25",
					r: "20",
					"aria-label": $("5"),
					fill: O.value === "5" ? S(r)(P.value.style.chart.layout.smileys.colors[5], 20) : "transparent",
					onMouseenter: t[0] ||= (e) => W("5"),
					onMouseleave: t[1] ||= (e) => G("5"),
					onClick: t[2] ||= (e) => K("5")
				}, null, 40, Ye)], 8, qe),
				f("g", { transform: B("4") }, [f("path", {
					fill: "none",
					stroke: P.value.style.chart.layout.smileys.colors[4],
					"stroke-width": "1",
					"stroke-linecap": "round",
					d: "M218 95A1 1 0 00236 95 1 1 0 00218 95M222 97C225 99 229 99 232 97M222 92A1 1 0 00225 92 1 1 0 00222 92M229 92A1 1 0 00232 92 1 1 0 00229 92"
				}, null, 8, Ze), f("circle", {
					class: "vue-ui-mood-radar-trap",
					cx: "227",
					cy: "95.5",
					r: "20",
					"aria-label": $("4"),
					fill: O.value === "4" ? S(r)(P.value.style.chart.layout.smileys.colors[4], 20) : "transparent",
					onMouseenter: t[3] ||= (e) => W("4"),
					onMouseleave: t[4] ||= (e) => G("4"),
					onClick: t[5] ||= (e) => K("4")
				}, null, 40, Qe)], 8, Xe),
				f("g", { transform: B("3") }, [f("path", {
					fill: "none",
					stroke: P.value.style.chart.layout.smileys.colors[3],
					"stroke-width": "1",
					"stroke-linecap": "round",
					d: "M181 213A1 1 0 00199 213 1 1 0 00181 213M185 210A1 1 0 00188 210 1 1 0 00185 210M192 210A1 1 0 00195 210 1 1 0 00192 210M185 215 195 215"
				}, null, 8, et), f("circle", {
					class: "vue-ui-mood-radar-trap",
					cx: "190",
					cy: "213.5",
					r: "20",
					"aria-label": $("3"),
					fill: O.value === "3" ? S(r)(P.value.style.chart.layout.smileys.colors[3], 20) : "transparent",
					onMouseenter: t[6] ||= (e) => W("3"),
					onMouseleave: t[7] ||= (e) => G("3"),
					onClick: t[8] ||= (e) => K("3")
				}, null, 40, tt)], 8, $e),
				f("g", { transform: B("2") }, [f("path", {
					fill: "none",
					stroke: P.value.style.chart.layout.smileys.colors[2],
					"stroke-width": "1",
					"stroke-linecap": "round",
					d: "M56 213A1 1 0 0074 213 1 1 0 0056 213M60 216C63 214 67 214 70 216M60 210A1 1 0 0063 210 1 1 0 0060 210M67 210A1 1 0 0070 210 1 1 0 0067 210"
				}, null, 8, rt), f("circle", {
					class: "vue-ui-mood-radar-trap",
					cx: "65",
					cy: "213.5",
					r: "20",
					"aria-label": $("2"),
					fill: O.value === "2" ? S(r)(P.value.style.chart.layout.smileys.colors[2], 20) : "transparent",
					onMouseenter: t[9] ||= (e) => W("2"),
					onMouseleave: t[10] ||= (e) => G("2"),
					onClick: t[11] ||= (e) => K("2")
				}, null, 40, it)], 8, nt),
				f("g", { transform: B("1") }, [f("path", {
					fill: "none",
					stroke: P.value.style.chart.layout.smileys.colors[1],
					"stroke-width": "1",
					"stroke-linecap": "round",
					d: "M20 96A1 1 0 0038 96 1 1 0 0020 96M24 100C25 95 33 95 34 100L24 100M24 93A1 1 0 0027 93 1 1 0 0024 93M31 93A1 1 0 0034 93 1 1 0 0031 93"
				}, null, 8, ot), f("circle", {
					class: "vue-ui-mood-radar-trap",
					cx: "29",
					cy: "95.5",
					r: "20",
					"aria-label": $("1"),
					fill: O.value === "1" ? S(r)(P.value.style.chart.layout.smileys.colors[1], 20) : "transparent",
					onMouseenter: t[12] ||= (e) => W("1"),
					onMouseleave: t[13] ||= (e) => G("1"),
					onClick: t[14] ||= (e) => K("1")
				}, null, 40, st)], 8, at),
				f("path", {
					d: S(n)(an.value.map((e) => e.plots)),
					stroke: P.value.style.chart.layout.dataPolygon.stroke,
					"stroke-width": P.value.style.chart.layout.dataPolygon.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					fill: P.value.style.chart.layout.dataPolygon.gradient.show ? `url(#mood_radar_gradient_${E.value})` : S(r)(P.value.style.chart.layout.dataPolygon.color, P.value.style.chart.layout.dataPolygon.opacity)
				}, null, 8, ct),
				(v(!0), d(Ee, null, Pe(an.value.map((e) => e.plots), (e, t) => (v(), d("g", {
					class: "vue-ui-mood-radar-trap",
					style: _(`opacity:${O.value == e.key ? "1" : "0"}`)
				}, [
					f("line", {
						x1: e.x,
						y1: e.y,
						x2: z.value.width / 2,
						y2: z.value.height / 2,
						stroke: P.value.style.chart.layout.smileys.colors[e.key]
					}, null, 8, lt),
					f("circle", {
						cx: e.x,
						cy: e.y,
						fill: P.value.style.chart.layout.smileys.colors[e.key],
						r: "3",
						stroke: P.value.style.chart.backgroundColor,
						"stroke-width": .5
					}, null, 8, ut),
					f("circle", {
						cx: z.value.width / 2,
						cy: z.value.height / 2,
						fill: P.value.style.chart.layout.smileys.colors[e.key],
						r: "3",
						stroke: P.value.style.chart.backgroundColor,
						"stroke-width": .5
					}, null, 8, dt),
					f("text", {
						x: z.value.width / 2,
						y: ["5", 5].includes(e.key) ? z.value.height / 2 * 1.13 : z.value.height / 2 * .9375,
						fill: P.value.style.chart.layout.dataLabel.color,
						"font-size": "12",
						"text-anchor": "middle",
						"font-weight": P.value.style.chart.layout.dataLabel.bold ? "bold" : "normal"
					}, x(S(s)(P.value.style.chart.layout.dataLabel.formatter, e.value, S(o)({
						p: P.value.style.chart.layout.dataLabel.prefix,
						v: e.value,
						s: P.value.style.chart.layout.dataLabel.suffix,
						r: P.value.style.chart.layout.dataLabel.roundingValue
					}), {
						datapoint: e,
						seriesIndex: t
					})), 9, ft),
					f("text", {
						x: z.value.width / 2,
						y: ["5", 5].includes(e.key) ? z.value.height / 2 * 1.273 : z.value.height / 2 * .7968,
						fill: P.value.style.chart.layout.dataLabel.color,
						"font-size": "12",
						"text-anchor": "middle"
					}, " (" + x(S(o)({
						v: e.value / V.value * 100,
						s: "%",
						r: P.value.style.chart.layout.dataLabel.roundingPercentage
					})) + ") ", 9, pt)
				], 4))), 256)),
				b(e.$slots, "svg", { svg: {
					...z.value,
					isPrintingImg: S(qt) || S(Jt) || S(vn),
					isPrintingSvg: S(yn)
				} }, void 0, !0)
			], 46, Ue)), e.$slots.hint ? (v(), d("div", mt, [b(e.$slots, "hint", g(h({
				hint: P.value.a11y.translations.keyboardNavigation,
				isVisible: N.value
			})), void 0, !0)])) : u("", !0)]),
			e.$slots.watermark ? (v(), d("div", ht, [b(e.$slots, "watermark", g(h({ isPrinting: S(qt) || S(Jt) || S(vn) || S(yn) })), void 0, !0)])) : u("", !0),
			f("div", { id: `legend-bottom-${E.value}` }, null, 8, gt),
			It.value && (P.value.style.chart.legend.show || e.$slots.legend) ? (v(), l(De, {
				key: 6,
				to: P.value.style.chart.legend.position === "top" ? `#legend-top-${E.value}` : `#legend-bottom-${E.value}`
			}, [f("div", {
				ref_key: "chartLegend",
				ref: Pt
			}, [b(e.$slots, "legend", { legend: H.value }, () => [P.value.style.chart.legend.show ? (v(), l(we, {
				legendSet: rn.value,
				config: on.value,
				key: `legend_${Mt.value}`,
				isCursorPointer: F.value,
				style: {
					display: "flex",
					"row-gap": "6px"
				},
				onFocusMarker: t[15] ||= ({ legend: e }) => U(e.key)
			}, {
				item: C(({ legend: e, index: t }) => [f("div", {
					role: "button",
					onClick: () => U(e.key),
					style: {
						display: "flex",
						"flex-direction": "row",
						gap: "3px",
						"align-items": "center",
						margin: "3px 0"
					},
					onKeydown: (t) => Mn(t, String(e.key))
				}, [
					e.key == 1 ? (v(), l(S(w), {
						key: 0,
						strokeWidth: 1,
						name: "moodSad",
						stroke: P.value.style.chart.layout.smileys.colors[e.key]
					}, null, 8, ["stroke"])) : u("", !0),
					e.key == 2 ? (v(), l(S(w), {
						key: 1,
						strokeWidth: 1,
						name: "moodFlat",
						stroke: P.value.style.chart.layout.smileys.colors[e.key]
					}, null, 8, ["stroke"])) : u("", !0),
					e.key == 3 ? (v(), l(S(w), {
						key: 2,
						strokeWidth: 1,
						name: "moodNeutral",
						stroke: P.value.style.chart.layout.smileys.colors[e.key]
					}, null, 8, ["stroke"])) : u("", !0),
					e.key == 4 ? (v(), l(S(w), {
						key: 3,
						strokeWidth: 1,
						name: "smiley",
						stroke: P.value.style.chart.layout.smileys.colors[e.key]
					}, null, 8, ["stroke"])) : u("", !0),
					e.key == 5 ? (v(), l(S(w), {
						key: 4,
						strokeWidth: 1,
						name: "moodHappy",
						stroke: P.value.style.chart.layout.smileys.colors[e.key]
					}, null, 8, ["stroke"])) : u("", !0),
					S(Bt) ? u("", !0) : (v(), d("span", {
						key: 5,
						style: _({ fontWeight: P.value.style.chart.legend.bold ? "bold" : "normal" })
					}, x(e.display), 5))
				], 40, _t)]),
				_: 1
			}, 8, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : u("", !0)], !0)], 512)], 8, ["to"])) : u("", !0),
			e.$slots.source ? (v(), d("div", {
				key: 7,
				ref_key: "source",
				ref: Ft,
				dir: "auto"
			}, [b(e.$slots, "source", {}, void 0, !0)], 512)) : u("", !0),
			Rt.value && P.value.userOptions.buttons.table ? (v(), l(Fe(Z.value.component), Ae({ key: 8 }, Z.value.props, {
				ref_key: "tableUnit",
				ref: j,
				onClose: pn
			}), Oe({
				content: C(() => [p(S(bt), {
					colNames: J.value.colNames,
					head: J.value.head,
					body: J.value.body,
					config: J.value.config,
					title: P.value.table.useDialog ? "" : Z.value.title,
					withCloseButton: !P.value.table.useDialog,
					isCursorPointer: F.value,
					onClose: pn
				}, {
					th: C(({ th: e }) => [f("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, vt)]),
					td: C(({ td: e }) => [ke(x(e.name || e), 1)]),
					_: 1
				}, 8, [
					"colNames",
					"head",
					"body",
					"config",
					"title",
					"withCloseButton",
					"isCursorPointer"
				])]),
				_: 2
			}, [P.value.table.useDialog ? {
				name: "title",
				fn: C(() => [ke(x(Z.value.title), 1)]),
				key: "0"
			} : void 0, P.value.table.useDialog ? {
				name: "actions",
				fn: C(() => [f("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[16] ||= (e) => sn(P.value.userOptions.callbacks.csv),
					style: _({ cursor: F.value ? "pointer" : "default" })
				}, [p(S(w), {
					name: "fileCsv",
					stroke: Z.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : u("", !0),
			b(e.$slots, "skeleton", {}, () => [S(Bt) ? (v(), l(de, { key: 0 })) : u("", !0)], !0)
		], 46, ze));
	}
}, [["__scopeId", "data-v-099dbe8f"]]);
//#endregion
export { Re as n, w as t };
