import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Jt as r, Kt as i, Pt as a, S as o, X as s, ct as ee, i as te, jt as ne, q as re, t as ie, tt as ae, w as oe, xt as se } from "./lib-Bttd6u5E.js";
import { n as ce, t as le } from "./useHints-Dq_w2E8B.js";
import { t as ue } from "./useConfig-DlNpz6P8.js";
import { t as de } from "./usePrinter-DN5bYhTG.js";
import { n as fe, t as pe } from "./BaseScanner-DZvpgOjM.js";
import { t as me } from "./useNestedProp-vPNvh7rV.js";
import { t as he } from "./useThemeCheck-C43Tcqmk.js";
import { t as ge } from "./useChartExport-DNiwdPmb.js";
import { t as _e } from "./useTransitions-g_zBREk2.js";
import { t as ve } from "./img-Bnokohej.js";
import { n as ye } from "./Title-BE3qg9xl.js";
import { t as be } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as xe, t as Se } from "./useResponsive-ZtArZtUf.js";
import { t as Ce } from "./BaseLegendToggle-DZVucLnv.js";
import { t as we } from "./A11yDataTable-DdRsVULz.js";
import { t as Te } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ee } from "./useChartAccessibility-DYqac8yF.js";
import { t as De } from "./labelUtils-BeVpDvTJ.js";
import { t as Oe } from "./Legend-CQxUgOd-.js";
import { t as ke } from "./useAutoSizeLabelsInsideViewbox-DvDwcwi_.js";
import { t as Ae } from "./vue_ui_onion-1FTFFS46.js";
import { Fragment as je, Teleport as Me, computed as c, createBlock as l, createCommentVNode as u, createElementBlock as d, createElementVNode as f, createSlots as Ne, createTextVNode as Pe, createVNode as Fe, defineAsyncComponent as p, guardReactiveProps as m, mergeProps as Ie, nextTick as Le, normalizeClass as h, normalizeProps as g, normalizeStyle as _, onBeforeUnmount as Re, onMounted as ze, openBlock as v, ref as y, renderList as b, renderSlot as x, resolveDynamicComponent as Be, shallowRef as Ve, toDisplayString as He, toRefs as Ue, unref as S, watch as C, withCtx as w } from "vue";
//#region src/components/vue-ui-onion.vue
var We = /* @__PURE__ */ e({ default: () => ft }), Ge = ["id"], Ke = ["id"], qe = ["id"], Je = { style: { position: "relative" } }, Ye = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Xe = ["width", "height"], Ze = [
	"cx",
	"cy",
	"r",
	"stroke",
	"stroke-width",
	"stroke-dasharray",
	"stroke-dashoffset"
], Qe = [
	"cx",
	"cy",
	"r",
	"stroke",
	"stroke-width",
	"stroke-dasharray",
	"stroke-dashoffset"
], $e = ["id"], et = ["stdDeviation"], tt = ["filter"], nt = [
	"cx",
	"cy",
	"r",
	"stroke-width",
	"stroke-dasharray",
	"stroke-dashoffset"
], rt = [
	"cx",
	"cy",
	"r",
	"stroke-width",
	"stroke-dasharray",
	"stroke-dashoffset",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], it = { key: 2 }, at = [
	"onMouseenter",
	"onMouseleave",
	"onClick"
], ot = [
	"transform",
	"font-size",
	"fill",
	"font-weight"
], st = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, ct = {
	key: 5,
	class: "vue-data-ui-watermark"
}, lt = ["id"], ut = ["onClick"], dt = ["innerHTML"], ft = /*#__PURE__*/ be({
	__name: "vue-ui-onion",
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
	emits: ["selectLegend", "copyAlt"],
	setup(e, { expose: be, emit: We }) {
		let ft = p(() => import("./Tooltip-DhjyfHwz.js")), pt = p(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), mt = p(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), ht = p(() => import("./DataTable-BbKgJ5UI.js")), gt = p(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), _t = p(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), vt = p(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), yt = p(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_onion: bt } = ue(), { isThemeValid: xt, warnInvalidTheme: St } = he(), T = e, Ct = c(() => !!T.dataset && T.dataset.length), E = y(re()), wt = y(null), Tt = y(0), D = y(!1), Et = y(""), O = y([]), k = y(null), Dt = y(null), Ot = y(null), kt = y(null), At = y(null), jt = y(0), Mt = y(0), Nt = y(0), Pt = y(!1), Ft = y(!1), It = y(null), A = y(null), Lt = y({
			x: 0,
			y: 0
		}), j = y("pointer"), Rt = y(!1), M = y(Gt());
		ce({
			config: () => M.value,
			dataset: () => T.dataset,
			component: "VueUiOnion",
			rules: [le.emptyArray, {
				test: (e) => e.length > 8,
				message: [
					"👀 The number of series is > 8. Consider:",
					"",
					"▶️ Grouping small values dynamically into a single \"Other\" series.",
					"",
					"▶️ Using filters to let users choose a maximum number of series to display.",
					"",
					"▶️ Using VueUiHorizontalBar instead to make the dataset breakdown easier to read."
				]
			}]
		});
		let { transitionEnabled: N } = _e({
			config: () => M.value.transitions,
			dataset: () => T.dataset
		}), P = c(() => M.value.userOptions.useCursorPointer), zt = c(() => r({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						gutter: { color: "#99999950" },
						labels: { show: !1 }
					},
					legend: { backgroundColor: "transparent" }
				} }
			},
			userConfig: M.value.skeletonConfig ?? {}
		})), { loading: F, FINAL_DATASET: Bt, manualLoading: Vt } = fe({
			...Ue(T),
			FINAL_CONFIG: M,
			prepareConfig: Gt,
			callback: () => {
				Promise.resolve().then(async () => {
					await Le(), dn();
				});
			},
			skeletonDataset: T.config?.skeletonDataset ?? [
				{
					name: "_",
					percentage: 50,
					value: 1,
					color: "#DBDBDB"
				},
				{
					name: "_",
					percentage: 50,
					value: 1,
					color: "#C4C4C4"
				},
				{
					name: "_",
					percentage: 50,
					value: 1,
					color: "#ADADAD"
				},
				{
					name: "_",
					percentage: 50,
					value: 1,
					color: "#969696"
				}
			],
			skeletonConfig: r({
				defaultConfig: M.value,
				userConfig: zt.value
			})
		}), { userOptionsVisible: Ht, setUserOptionsVisibility: Ut, keepUserOptionState: Wt } = Te({ config: M.value }), { svgRef: I } = Ee({ config: M.value.style.chart.title });
		function Gt() {
			let e = me({
				userConfig: T.config,
				defaultConfig: bt
			}), t = e.theme;
			if (!t) return e;
			if (!xt.value(e)) return St(e), e;
			let n = me({
				userConfig: Ae[t] || T.config,
				defaultConfig: e
			}), r = me({
				userConfig: T.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : i[t] || a
			};
		}
		C(() => T.config, (e) => {
			F.value || (M.value = Gt()), Ht.value = !M.value.userOptions.showOnChartHover, on(), jt.value += 1, Mt.value += 1, Nt.value += 1, L.value.showTable = M.value.table.show, L.value.showTooltip = M.value.style.chart.tooltip.show;
		}, { deep: !0 });
		let { isPrinting: Kt, isImaging: qt, generatePdf: Jt, generateImage: Yt } = de({
			elementId: `vue-ui-onion_${E.value}`,
			fileName: M.value.style.chart.title.text || "vue-ui-onion",
			options: M.value.userOptions.print
		}), Xt = c(() => M.value.userOptions.show && !M.value.style.chart.title.text), Zt = c(() => oe(M.value.customPalette)), L = y({
			showTable: M.value.table.show,
			showTooltip: M.value.style.chart.tooltip.show
		});
		C(M, () => {
			L.value = {
				showTable: M.value.table.show,
				showTooltip: M.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let R = y({
			height: 512,
			width: 512,
			padding: {
				top: 64,
				left: 64,
				right: 64,
				bottom: 64
			},
			minRadius: 64
		}), z = Ve(null), B = Ve(null);
		ze(() => {
			Ft.value = !0, on();
		});
		let { autoSizeLabels: Qt } = ke({
			svgRef: I,
			fontSize: () => M.value.style.chart.layout.labels.fontSize,
			minFontSize: () => M.value.style.chart.layout.labels.minFontSize,
			labelClass: ".vue-ui-onion-label"
		}), V = null, H = null, U = 0;
		function $t() {
			V !== null && (cancelAnimationFrame(V), V = null), H !== null && (clearTimeout(H), H = null);
		}
		function en(e) {
			return String(e).split(",").map((e) => {
				let t = e.trim(), n = Number.parseFloat(t);
				return Number.isFinite(n) ? t.endsWith("ms") ? n : n * 1e3 : 0;
			});
		}
		function tn() {
			return typeof window > "u" || !N.value || Pt.value || F.value || !I.value ? 0 : Array.from(I.value.querySelectorAll(".vue-ui-onion-label.vue-data-ui-transition")).reduce((e, t) => {
				let n = window.getComputedStyle(t);
				if (n.transitionProperty === "none") return e;
				let r = en(n.transitionDuration), i = en(n.transitionDelay), a = Math.max(r.length, i.length), o = Array.from({ length: a }, (e, t) => (r[t % r.length] ?? 0) + (i[t % i.length] ?? 0)).reduce((e, t) => Math.max(e, t), 0);
				return Math.max(e, o);
			}, 0);
		}
		function nn() {
			typeof window > "u" || (V = requestAnimationFrame(() => {
				V = null, Qt();
			}));
		}
		function W() {
			if (typeof window > "u") return;
			let e = ++U;
			$t(), Le(() => {
				e === U && (V = requestAnimationFrame(() => {
					if (V = null, e !== U) return;
					let t = tn();
					if (t <= 0) {
						nn();
						return;
					}
					H = setTimeout(() => {
						H = null, e === U && nn();
					}, t + 34);
				}));
			});
		}
		let rn = c(() => M.value.debug), an = null;
		function on() {
			if (ne(T.dataset) && (ae({
				componentName: "VueUiOnion",
				type: "dataset",
				debug: rn.value
			}), Vt.value = !0), ne(T.dataset) || (Vt.value = M.value.loading), M.value.responsive) {
				let e = 64 / 512, t = xe(() => {
					an && clearTimeout(an), Pt.value = !0;
					let { width: t, height: n } = Se({
						chart: k.value,
						title: M.value.style.chart.title.text ? Dt.value : null,
						legend: M.value.style.chart.legend.show ? Ot.value : null,
						source: kt.value,
						noTitle: At.value
					});
					n -= 12, requestAnimationFrame(async () => {
						R.value.width = t, R.value.height = n, R.value.padding.top = Math.max(t, n) * e, R.value.padding.right = Math.max(t, n) * e, R.value.padding.bottom = Math.max(t, n) * e, R.value.padding.left = Math.max(t, n) * e, R.value.minRadius = Math.min(t, n) * e, an = setTimeout(() => {
							Pt.value = !1, W();
						}, 0);
					});
				});
				z.value && (B.value && z.value.unobserve(B.value), z.value.disconnect()), z.value = new ResizeObserver(t), B.value = k.value.parentNode, z.value.observe(B.value);
			}
			W();
		}
		Re(() => {
			U += 1, $t(), z.value && (B.value && z.value.unobserve(B.value), z.value.disconnect());
		});
		let G = c(() => ({
			top: R.value.padding.top,
			left: R.value.padding.left,
			right: R.value.width - R.value.padding.right,
			bottom: R.value.height - R.value.padding.bottom,
			centerX: R.value.width / 2,
			centerY: R.value.height / 2,
			width: R.value.width - R.value.padding.right - R.value.padding.left,
			height: R.value.height - R.value.padding.bottom - R.value.padding.top,
			minRadius: R.value.minRadius,
			maxRadius: Math.min(R.value.width, R.value.height) - R.value.padding.top * 2
		})), K = c(() => (rn.value && Bt.value.forEach((e, t) => {
			[null, void 0].includes(e.name) && ae({
				componentName: "VueUiOnion",
				type: "datasetSerieAttribute",
				property: "name",
				index: t
			}), [void 0].includes(e.percentage) && ae({
				componentName: "VueUiOnion",
				type: "datasetSerieAttribute",
				property: "percentage",
				index: t
			});
		}), Bt.value.map((e, t) => {
			let n = `onion_serie_${t}_${E.value}`;
			return {
				...e,
				percentage: e.percentage || 0,
				targetPercentage: e.percentage || 0,
				color: o(e.color) || Zt.value[t] || a[t],
				id: n,
				shape: "circle",
				opacity: O.value.includes(n) ? .5 : 1,
				absoluteIndex: t,
				segregate: () => _n(n),
				isSegregated: O.value.includes(n)
			};
		}))), q = c(() => K.value.map((e, t) => {
			let n = M.value.style.chart.legend.showValue, r = M.value.style.chart.legend.showPercentage, i = Dn({
				showVal: n,
				showPercentage: r,
				val: te(M.value.style.chart.layout.labels.value.formatter, e.value, s({
					p: e.prefix || "",
					v: e.value,
					s: e.suffix || "",
					r: M.value.style.chart.legend.roundingValue
				})),
				percentage: s({
					v: e.percentage ?? 0,
					s: "%",
					r: M.value.style.chart.legend.roundingPercentage
				}),
				config: M.value.style.chart.legend
			});
			return {
				...e,
				display: `${e.name}${n || r ? ": " : ""}${i}`
			};
		})), J = y(K.value), sn = c(() => M.value.useStartAnimation), cn = y(null), ln = c(() => Math.max(...K.value.map((e) => e.percentage))), un = y(!1);
		C(() => K.value, dn, {
			immediate: !0,
			deep: !0
		}), C(() => T.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Vt.value = !1), W();
		}, {
			deep: !0,
			flush: "post"
		});
		function dn() {
			if (sn.value && !un.value && !F.value) {
				J.value = K.value.map((e) => ({
					...e,
					percentage: 0
				}));
				let e = 0;
				function t() {
					e >= ln.value ? (cancelAnimationFrame(cn.value), J.value = K.value, un.value = !0) : (J.value = K.value.map((t) => ({
						...t,
						percentage: e < t.targetPercentage ? e : t.targetPercentage
					})), e += 1, requestAnimationFrame(t), un.value = !0);
				}
				t();
			} else J.value = K.value;
		}
		let fn = c(() => ({
			cy: "onion-div-legend",
			backgroundColor: M.value.style.chart.legend.backgroundColor,
			color: M.value.style.chart.legend.color,
			fontSize: M.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: M.value.style.chart.legend.bold ? "bold" : ""
		})), pn = c(() => K.value.filter((e) => !O.value.includes(e.id)).length), Y = c(() => {
			let e = Math.min(G.value.width, G.value.height) / 2 / K.value.length;
			return {
				gutter: (e > M.value.style.chart.layout.maxThickness ? M.value.style.chart.layout.maxThickness : e) * M.value.style.chart.layout.gutter.width,
				track: (e > M.value.style.chart.layout.maxThickness ? M.value.style.chart.layout.maxThickness : e) * M.value.style.chart.layout.track.width
			};
		}), X = c(() => J.value.filter((e) => !O.value.includes(e.id)).map((e, t) => {
			let n = (G.value.maxRadius - Y.value.track) / pn.value / 2 * (1 + t), r = G.value.centerY - n;
			return {
				percentage: e.percentage || 0,
				...e,
				labelY: r,
				radius: n,
				path: mn(n, e.percentage || 0)
			};
		}));
		function mn(e, t) {
			let n = 2 * Math.PI * e, r = n * .75, i = `${r} ${n}`, a = r * (1 - t / 100);
			return {
				bgDashArray: `${r} ${n}`,
				bgDashOffset: 0,
				dashArray: i,
				dashOffset: a,
				fullOffset: 0,
				active: `
            M ${G.value.centerX},${G.value.centerY - e} 
            A ${e},${e} 0 1 1 
            ${G.value.centerX + e * Math.cos(Math.PI * 3 / 4)},${G.value.centerY + e * Math.sin(Math.PI * 3 / 4)}
        `.trim()
			};
		}
		let hn = We;
		function gn() {
			O.value.length ? O.value = [] : q.value.forEach((e) => {
				O.value.push(e.id);
			}), W(), hn("selectLegend", X.value);
		}
		function _n(e) {
			if (O.value.includes(e)) O.value = O.value.filter((t) => t !== e);
			else {
				if (O.value.length === K.value.length - 1) return;
				O.value.push(e);
			}
			W(), hn("selectLegend", X.value);
		}
		function vn(e) {
			return K.value.length ? K.value.find((t) => t.name === e) || (rn.value && console.warn(`VueUiOnion - Series name not found "${e}"`), null) : (rn.value && console.warn("VueUiOnion - There are no series to show."), null);
		}
		function yn(e) {
			let t = vn(e);
			t !== null && O.value.includes(t.id) && _n(t.id);
		}
		function bn(e) {
			let t = vn(e);
			t !== null && (O.value.includes(t.id) || _n(t.id));
		}
		function xn() {
			return X.value;
		}
		let Sn = c(() => ({
			head: [
				M.value.table.translations.serie,
				M.value.table.translations.percentage,
				M.value.table.translations.value
			],
			body: X.value.map((e) => [
				e.name,
				e.percentage,
				e.value
			])
		})), Z = c(() => {
			let e = Sn.value.head;
			return {
				head: e,
				body: X.value.map((e) => [
					`<span style="color:${e.color}" aria-hidden="true">⬤</span> ${e.name}`,
					`${Number(e.percentage ?? 0).toFixed(M.value.table.td.roundingPercentage).toLocaleString()}%`,
					`${e.prefix || ""}${[
						null,
						void 0,
						NaN,
						"NaN"
					].includes(e.value) ? "-" : e.value.toFixed(M.value.table.td.roundingValue).toLocaleString()}${e.suffix || ""}`
				]),
				config: {
					th: {
						backgroundColor: M.value.table.th.backgroundColor,
						color: M.value.table.th.color,
						outline: M.value.table.th.outline
					},
					td: {
						backgroundColor: M.value.table.td.backgroundColor,
						color: M.value.table.td.color,
						outline: M.value.table.td.outline
					},
					breakpoint: M.value.table.responsiveBreakpoint
				},
				colNames: e
			};
		});
		function Cn(e = null) {
			Le(() => {
				let r = [
					[M.value.style.chart.title.text],
					[M.value.style.chart.title.subtitle.text],
					[""]
				], i = Sn.value.head, a = Sn.value.body, o = r.concat([i]).concat(a), s = n(o);
				e ? e(s) : t({
					csvContent: s,
					title: M.value.style.chart.title.text || "vue-ui-onion"
				});
			});
		}
		let Q = y(void 0), $ = y(!1);
		function wn(e) {
			$.value = e, Tt.value += 1;
		}
		function Tn({ datapoint: e }) {
			M.value.events.datapointClick && M.value.events.datapointClick({
				datapoint: e,
				seriesIndex: e.absoluteIndex
			});
		}
		function En({ datapoint: e }) {
			Q.value = void 0, D.value = !1, A.value = null, j.value = "pointer", M.value.events.datapointLeave && M.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: e.absoluteIndex
			});
		}
		function Dn({ val: e, percentage: t, showVal: n, showPercentage: r, config: i }) {
			return De({
				config: i,
				val: e,
				percentage: t,
				showVal: n,
				showPercentage: r
			});
		}
		function On(e, t) {
			let n = M.value.style.chart.layout.labels.value.show, r = M.value.style.chart.layout.labels.percentage.show, i = Dn({
				config: M.value.style.chart.layout.labels,
				showVal: n,
				showPercentage: r,
				val: te(M.value.style.chart.layout.labels.value.formatter, e.value, s({
					p: e.prefix || "",
					v: e.value || 0,
					s: e.suffix || "",
					r: M.value.style.chart.layout.labels.roundingValue
				}), {
					datapoint: e,
					seriesIndex: t
				}),
				percentage: s({
					v: e.percentage,
					s: "%",
					r: M.value.style.chart.layout.labels.roundingPercentage
				})
			});
			return `${e.name}${n || r ? ": " : ""}${i}`;
		}
		let kn = y(null);
		function An({ datapoint: e, seriesIndex: t, show: n = !0, triggerMode: r = "pointer" }) {
			M.value.events.datapointEnter && M.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: e.absoluteIndex
			}), j.value = r, A.value = t;
			let i = e.absoluteIndex;
			Q.value = t, kn.value = {
				datapoint: e,
				seriesIndex: i,
				series: K.value,
				config: M.value
			}, D.value = n;
			let a = "", o = M.value.style.chart.tooltip.customFormat;
			if (se(o) && ee(() => o({
				seriesIndex: i,
				datapoint: e,
				series: K.value,
				config: M.value
			}))) Et.value = o({
				seriesIndex: i,
				datapoint: e,
				series: K.value,
				config: M.value
			});
			else {
				let n = M.value.style.chart.tooltip.showPercentage, r = M.value.style.chart.tooltip.showValue;
				a += `<div style="width:100%;text-align:center;border-bottom:1px solid ${M.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${e.name}</div>`, a += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 60 60" height="14" width="14"><circle cx="30" cy="30" r="30" stroke="none" fill="${e.color}"/></svg>`, a += `<b>${Dn({
					config: M.value.style.chart.tooltip,
					showVal: r,
					showPercentage: n,
					val: `<span>${te(M.value.style.chart.layout.labels.value.formatter, e.value, s({
						p: e.prefix || "",
						v: e.value,
						s: e.suffix || "",
						r: M.value.style.chart.tooltip.roundingValue
					}), {
						datapoint: e,
						seriesIndex: t
					})}</span>`,
					percentage: s({
						v: e.percentage,
						s: "%",
						r: M.value.style.chart.tooltip.roundingPercentage
					})
				})}</b></div>`, Et.value = `<div>${a}</div>`;
			}
		}
		function jn() {
			L.value.showTable = !L.value.showTable;
		}
		function Mn() {
			L.value.showTooltip = !L.value.showTooltip;
		}
		let Nn = y(!1);
		function Pn() {
			Nn.value = !Nn.value;
		}
		async function Fn({ scale: e = 2 } = {}) {
			if (!k.value) return;
			let { width: t, height: n } = k.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ve({
				domElement: k.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: M.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let In = c(() => {
			let e = M.value.table.useDialog && !M.value.table.show, t = L.value.showTable;
			return {
				component: e ? yt : mt,
				title: `${M.value.style.chart.title.text}${M.value.style.chart.title.subtitle.text ? `: ${M.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: M.value.table.th.backgroundColor,
					color: M.value.table.th.color,
					headerColor: M.value.table.th.color,
					headerBg: M.value.table.th.backgroundColor,
					isFullscreen: $.value,
					fullscreenParent: k.value,
					forcedWidth: Math.min(600, window.innerWidth * .8),
					isCursorPointer: P.value
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
		C(() => L.value.showTable, (e) => {
			M.value.table.show || (e && M.value.table.useDialog && It.value ? It.value.open() : "close" in It.value && It.value.close());
		});
		let Ln = c(() => q.value.map((e) => ({
			...e,
			name: e.display
		}))), Rn = c(() => M.value.style.chart.backgroundColor), zn = c(() => M.value.style.chart.legend), Bn = c(() => M.value.style.chart.title), { isCallbackImaging: Vn, isCallbackSvg: Hn, generateSvg: Un, onGenerateImage: Wn } = ge({
			svg: I,
			title: Bn,
			legend: zn,
			legendItems: Ln,
			backgroundColor: Rn,
			getSvgCallback: () => M.value.userOptions.callbacks.svg,
			generateImage: Yt
		});
		async function Gn() {
			if (hn("copyAlt", {
				config: M.value,
				dataset: X.value
			}), !M.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(M.value.userOptions.callbacks.altCopy({
				config: M.value,
				dataset: X.value
			}));
		}
		function Kn() {
			A.value = null, Rt.value = !0;
		}
		function qn() {
			A.value = null, j.value = "pointer", D.value = !1, Q.value = void 0, Rt.value = !1;
		}
		function Jn(e) {
			if (!I.value || Nn.value || document.activeElement !== I.value || !X.value.length) return;
			let t = ["ArrowLeft", "ArrowDown"].includes(e.key), n = ["ArrowRight", "ArrowUp"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				A.value = null, j.value = "pointer", D.value = !1, Q.value = void 0;
				return;
			}
			if (r) {
				if (A.value === null) return;
				let e = X.value[A.value];
				if (!e) return;
				Tn({ datapoint: e });
				return;
			}
			let a = A.value;
			a === null || a < 0 || a >= X.value.length ? a = n ? 0 : X.value.length - 1 : n ? (a += 1, a >= X.value.length && (a = 0)) : t && (--a, a < 0 && (a = X.value.length - 1));
			let o = X.value[a];
			o && (Yn(a), An({
				datapoint: o,
				seriesIndex: a,
				show: !0,
				triggerMode: "keyboard"
			}));
		}
		function Yn(e) {
			if (!Number.isFinite(e) || !I.value) return;
			let t = X.value[e];
			if (!t) return;
			let n = t.radius, r = Math.PI * 7 / 4, i = G.value.centerX + n * Math.cos(r), a = G.value.centerY + n * Math.sin(r), o = I.value.getBoundingClientRect();
			Lt.value = {
				x: o.left + i / R.value.width * o.width,
				y: o.top + a / R.value.height * o.height
			};
		}
		let Xn = c(() => ({
			headers: Z.value?.colNames ?? [],
			rows: Z.value?.body ?? []
		}));
		return be({
			getData: xn,
			getImage: Fn,
			generatePdf: Jt,
			generateCsv: Cn,
			generateImage: Yt,
			generateSvg: Un,
			hideSeries: bn,
			showSeries: yn,
			toggleTable: jn,
			toggleTooltip: Mn,
			toggleAnnotator: Pn,
			toggleFullscreen: wn,
			copyAlt: Gn
		}), (e, t) => (v(), d("div", {
			class: h(`vue-data-ui-component vue-ui-onion ${$.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${M.value.useCssAnimation ? "" : "vue-ui-dna"}`),
			ref_key: "onionChart",
			ref: k,
			id: `vue-ui-onion_${E.value}`,
			style: _(`font-family:${M.value.style.fontFamily};width:100%; ${M.value.responsive ? "height: 100%;" : ""} text-align:center;background:${M.value.style.chart.backgroundColor}`),
			onMouseenter: t[4] ||= () => S(Ut)(!0),
			onMouseleave: t[5] ||= () => S(Ut)(!1)
		}, [
			f("div", {
				id: `chart-instructions-${E.value}`,
				class: "sr-only"
			}, [f("p", null, He(M.value.a11y.translations.keyboardNavigation), 1)], 8, Ke),
			Xn.value?.rows?.length ? (v(), l(we, {
				key: 0,
				uid: E.value,
				head: Xn.value.headers,
				body: Xn.value.rows,
				notice: M.value.a11y.translations.tableAvailable,
				caption: M.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : u("", !0),
			M.value.userOptions.buttons.annotator ? (v(), l(S(_t), {
				key: 1,
				svgRef: S(I),
				backgroundColor: M.value.style.chart.backgroundColor,
				color: M.value.style.chart.color,
				active: Nn.value,
				isCursorPointer: P.value,
				onClose: Pn
			}, {
				"annotator-action-close": w(() => [x(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": w(({ color: t }) => [x(e.$slots, "annotator-action-color", g(m({ color: t })), void 0, !0)]),
				"annotator-action-draw": w(({ mode: t }) => [x(e.$slots, "annotator-action-draw", g(m({ mode: t })), void 0, !0)]),
				"annotator-action-undo": w(({ disabled: t }) => [x(e.$slots, "annotator-action-undo", g(m({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": w(({ disabled: t }) => [x(e.$slots, "annotator-action-redo", g(m({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": w(({ disabled: t }) => [x(e.$slots, "annotator-action-delete", g(m({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : u("", !0),
			Xt.value ? (v(), d("div", {
				key: 2,
				ref_key: "noTitle",
				ref: At,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : u("", !0),
			M.value.style.chart.title.text ? (v(), d("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: Dt,
				style: "width:100%;background:transparent"
			}, [(v(), l(ye, {
				key: `title_${jt.value}`,
				config: {
					title: {
						cy: "onion-div-title",
						...M.value.style.chart.title
					},
					subtitle: {
						cy: "onion-div-subtitle",
						...M.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : u("", !0),
			f("div", { id: `legend-top-${E.value}` }, null, 8, qe),
			M.value.userOptions.show && Ct.value && (S(Wt) || S(Ht)) ? (v(), l(S(gt), {
				ref_key: "details",
				ref: wt,
				key: `user_options${Tt.value}`,
				backgroundColor: M.value.style.chart.backgroundColor,
				color: M.value.style.chart.color,
				isImaging: S(qt),
				isPrinting: S(Kt),
				uid: E.value,
				hasTooltip: M.value.userOptions.buttons.tooltip && M.value.style.chart.tooltip.show,
				hasPdf: M.value.userOptions.buttons.pdf,
				hasImg: M.value.userOptions.buttons.img,
				hasSvg: M.value.userOptions.buttons.svg,
				hasXls: M.value.userOptions.buttons.csv,
				hasTable: M.value.userOptions.buttons.table,
				hasFullscreen: M.value.userOptions.buttons.fullscreen,
				hasAltCopy: M.value.userOptions.buttons.altCopy,
				isFullscreen: $.value,
				isTooltip: L.value.showTooltip,
				titles: { ...M.value.userOptions.buttonTitles },
				chartElement: k.value,
				position: M.value.userOptions.position,
				hasAnnotator: M.value.userOptions.buttons.annotator,
				isAnnotation: Nn.value,
				callbacks: M.value.userOptions.callbacks,
				printScale: M.value.userOptions.print.scale,
				tableDialog: M.value.table.useDialog,
				isCursorPointer: P.value,
				onToggleFullscreen: wn,
				onGeneratePdf: S(Jt),
				onGenerateCsv: Cn,
				onGenerateImage: S(Wn),
				onGenerateSvg: S(Un),
				onToggleTable: jn,
				onToggleTooltip: Mn,
				onToggleAnnotator: Pn,
				onCopyAlt: Gn,
				style: _({ visibility: S(Wt) ? S(Ht) ? "visible" : "hidden" : "visible" })
			}, Ne({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: w(({ isOpen: t, color: n }) => [x(e.$slots, "menuIcon", g(m({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: w(() => [x(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: w(() => [x(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: w(() => [x(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: w(() => [x(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: w(() => [x(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: w(() => [x(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: w(({ toggleFullscreen: t, isFullscreen: n }) => [x(e.$slots, "optionFullscreen", g(m({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: w(({ toggleAnnotator: t, isAnnotator: n }) => [x(e.$slots, "optionAnnotator", g(m({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: w(({ altCopy: t }) => [x(e.$slots, "optionAltCopy", g(m({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: w(() => [x(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: w(() => [x(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isImaging.isPrinting.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : u("", !0),
			f("div", Je, [(v(), d("svg", {
				ref_key: "svgRef",
				ref: I,
				xmlns: S(ie),
				"aria-describedby": `chart-instructions-${E.value}`,
				class: h({
					"vue-data-ui-fullscreen--on": $.value,
					"vue-data-ui-fulscreen--off": !$.value,
					resizing: Pt.value,
					"vue-data-ui-no-transition": !S(N) || Pt.value || S(F)
				}),
				viewBox: `0 0 ${R.value.width <= 0 ? 10 : R.value.width} ${R.value.height <= 0 ? 10 : R.value.height}`,
				style: _(`max-width:100%;overflow:visible;background:transparent;color:${M.value.style.chart.color}`),
				tabindex: "0",
				onFocus: Kn,
				onBlur: qn,
				onKeydown: Jn
			}, [
				Fe(S(vt)),
				e.$slots["chart-background"] ? (v(), d("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: R.value.width <= 0 ? 10 : R.value.width,
					height: R.value.height <= 0 ? 10 : R.value.height,
					style: { pointerEvents: "none" }
				}, [x(e.$slots, "chart-background", {}, void 0, !0)], 8, Xe)) : u("", !0),
				(v(!0), d(je, null, b(X.value, (e, t) => (v(), d("circle", {
					cx: G.value.centerX,
					cy: G.value.centerY,
					r: e.radius <= 0 ? 1e-4 : e.radius,
					stroke: M.value.style.chart.layout.gutter.color,
					"stroke-width": Y.value.gutter,
					fill: "none",
					"stroke-dasharray": e.path.bgDashArray,
					"stroke-dashoffset": e.path.fullOffset,
					"stroke-linecap": "round",
					class: h({
						"vue-ui-onion-path": !0,
						"vue-ui-onion-blur": M.value.useBlurOnHover && ![null, void 0].includes(Q.value) && Q.value !== t,
						"vue-data-ui-transition": S(N)
					}),
					style: {
						transform: "rotate(-90deg)",
						transformOrigin: "50% 50%"
					}
				}, null, 10, Ze))), 256)),
				(v(!0), d(je, null, b(X.value, (e, t) => (v(), d("circle", {
					cx: G.value.centerX,
					cy: G.value.centerY,
					r: e.radius < 0 ? 1e-4 : e.radius,
					stroke: `${e.color}`,
					"stroke-width": Y.value.track,
					fill: "none",
					"stroke-dasharray": e.path.dashArray,
					"stroke-dashoffset": e.path.dashOffset,
					class: h({
						"vue-ui-onion-path": !0,
						"vue-ui-onion-blur": M.value.useBlurOnHover && ![null, void 0].includes(Q.value) && Q.value !== t,
						"vue-data-ui-transition": S(N)
					}),
					"stroke-linecap": "round",
					style: {
						transform: "rotate(-90deg)",
						transformOrigin: "50% 50%"
					}
				}, null, 10, Qe))), 256)),
				f("defs", null, [f("filter", {
					id: `blur_${E.value}`,
					x: "-50%",
					y: "-50%",
					width: "200%",
					height: "200%"
				}, [f("feGaussianBlur", {
					in: "SourceGraphic",
					stdDeviation: 100 / M.value.style.chart.gradientIntensity
				}, null, 8, et)], 8, $e)]),
				M.value.style.chart.useGradient ? (v(), d("g", {
					key: 1,
					filter: `url(#blur_${E.value})`
				}, [(v(!0), d(je, null, b(X.value, (e, t) => (v(), d("circle", {
					cx: G.value.centerX,
					cy: G.value.centerY,
					r: e.radius <= 0 ? 1e-4 : e.radius,
					stroke: "white",
					"stroke-width": Y.value.track / 3,
					fill: "none",
					"stroke-linecap": "round",
					"stroke-dasharray": e.path.dashArray,
					"stroke-dashoffset": e.path.dashOffset,
					style: {
						transform: "rotate(-90deg)",
						transformOrigin: "50% 50%"
					},
					class: h({ "vue-data-ui-transition": S(N) })
				}, null, 10, nt))), 256))], 8, tt)) : u("", !0),
				(v(!0), d(je, null, b(X.value, (e, t) => (v(), d("circle", {
					cx: G.value.centerX,
					cy: G.value.centerY,
					r: e.radius <= 0 ? 1e-4 : e.radius,
					stroke: "transparent",
					"stroke-width": Math.max(Y.value.track, Y.value.gutter),
					fill: "none",
					"stroke-dasharray": e.path.bgDashArray,
					"stroke-dashoffset": e.path.fullOffset,
					"stroke-linecap": "round",
					class: "vue-ui-onion-path",
					style: {
						transform: "rotate(-90deg)",
						transformOrigin: "50% 50%"
					},
					onMouseenter: (n) => An({
						datapoint: e,
						show: !0,
						seriesIndex: t,
						triggerMode: "pointer"
					}),
					onMouseleave: (t) => En({ datapoint: e }),
					onClick: (t) => Tn({ datapoint: e })
				}, null, 40, rt))), 256)),
				M.value.style.chart.layout.labels.show ? (v(), d("g", it, [(v(!0), d(je, null, b(X.value, (e, t) => (v(), d("g", {
					key: `dl_${e.id}`,
					onMouseenter: (n) => An({
						datapoint: e,
						show: !0,
						seriesIndex: t,
						triggerMode: "pointer"
					}),
					onMouseleave: (t) => En({ datapoint: e }),
					onClick: (t) => Tn({ datapoint: e })
				}, [O.value.includes(e.id) ? u("", !0) : (v(), d("text", {
					key: 0,
					class: h(["vue-ui-onion-label", { "vue-data-ui-transition": S(N) }]),
					transform: `translate(${R.value.width / 2 - Y.value.gutter * .8 + M.value.style.chart.layout.labels.offsetX},${e.labelY + M.value.style.chart.layout.labels.offsetY})`,
					"text-anchor": "end",
					"font-size": M.value.style.chart.layout.labels.fontSize,
					fill: M.value.useBlurOnHover && ![null, void 0].includes(Q.value) && Q.value === t ? e.color : M.value.style.chart.layout.labels.color,
					"font-weight": M.value.style.chart.layout.labels.bold ? "bold" : "normal"
				}, He(On(e, t)), 11, ot))], 40, at))), 128))])) : u("", !0),
				x(e.$slots, "svg", { svg: {
					...R.value,
					drawingArea: G.value,
					isPrintingImg: S(Kt) || S(qt) || S(Vn),
					isPrintingSvg: S(Hn)
				} }, void 0, !0)
			], 46, Ye)), e.$slots.hint ? (v(), d("div", st, [x(e.$slots, "hint", g(m({
				hint: M.value.a11y.translations.keyboardNavigation,
				isVisible: Rt.value
			})), void 0, !0)])) : u("", !0)]),
			e.$slots.watermark ? (v(), d("div", ct, [x(e.$slots, "watermark", g(m({ isPrinting: S(Kt) || S(qt) || S(Vn) || S(Hn) })), void 0, !0)])) : u("", !0),
			f("div", { id: `legend-bottom-${E.value}` }, null, 8, lt),
			Ft.value && (M.value.style.chart.legend.show || e.$slots.legend) ? (v(), l(Me, {
				key: 6,
				to: M.value.style.chart.legend.position === "top" ? `#legend-top-${E.value}` : `#legend-bottom-${E.value}`
			}, [f("div", {
				ref_key: "chartLegend",
				ref: Ot
			}, [x(e.$slots, "legend", { legend: q.value }, () => [M.value.style.chart.legend.show ? (v(), l(Oe, {
				key: `legend_${Nt.value}`,
				legendSet: q.value,
				config: fn.value,
				isCursorPointer: P.value,
				onClickMarker: t[0] ||= ({ legend: e }) => _n(e.id)
			}, {
				item: w(({ legend: e }) => [S(F) ? u("", !0) : (v(), d("div", {
					key: 0,
					"data-cy-legend-item": "",
					onClick: (t) => e.segregate(),
					style: _(`opacity:${O.value.includes(e.id) ? .5 : 1}`)
				}, He(e.display), 13, ut))]),
				legendToggle: w(() => [q.value.length > 2 && M.value.style.chart.legend.selectAllToggle.show && !S(F) ? (v(), l(Ce, {
					key: 0,
					backgroundColor: M.value.style.chart.legend.selectAllToggle.backgroundColor,
					color: M.value.style.chart.legend.selectAllToggle.color,
					fontSize: M.value.style.chart.legend.fontSize,
					checked: O.value.length > 0,
					isCursorPointer: P.value,
					onToggle: gn
				}, null, 8, [
					"backgroundColor",
					"color",
					"fontSize",
					"checked",
					"isCursorPointer"
				])) : u("", !0)]),
				_: 1
			}, 8, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : u("", !0)], !0)], 512)], 8, ["to"])) : u("", !0),
			e.$slots.source ? (v(), d("div", {
				key: 7,
				ref_key: "source",
				ref: kt,
				dir: "auto"
			}, [x(e.$slots, "source", {}, void 0, !0)], 512)) : u("", !0),
			Fe(S(ft), {
				teleportTo: M.value.style.chart.tooltip.teleportTo,
				show: L.value.showTooltip && D.value,
				backgroundColor: M.value.style.chart.tooltip.backgroundColor,
				color: M.value.style.chart.tooltip.color,
				borderRadius: M.value.style.chart.tooltip.borderRadius,
				borderColor: M.value.style.chart.tooltip.borderColor,
				borderWidth: M.value.style.chart.tooltip.borderWidth,
				fontSize: M.value.style.chart.tooltip.fontSize,
				backgroundOpacity: M.value.style.chart.tooltip.backgroundOpacity,
				position: M.value.style.chart.tooltip.position,
				offsetX: M.value.style.chart.tooltip.offsetX,
				offsetY: M.value.style.chart.tooltip.offsetY,
				parent: k.value,
				content: Et.value,
				isFullscreen: $.value,
				isCustom: S(se)(M.value.style.chart.tooltip.customFormat),
				smooth: M.value.style.chart.tooltip.smooth,
				backdropFilter: M.value.style.chart.tooltip.backdropFilter,
				smoothForce: M.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: M.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: j.value === "keyboard",
				a11yPosition: Lt.value
			}, {
				"tooltip-before": w(() => [x(e.$slots, "tooltip-before", g(m({ ...kn.value })), void 0, !0)]),
				tooltip: w(() => [x(e.$slots, "tooltip", g(m({ ...kn.value })), void 0, !0)]),
				"tooltip-after": w(() => [x(e.$slots, "tooltip-after", g(m({ ...kn.value })), void 0, !0)]),
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
			Ct.value && M.value.userOptions.buttons.table ? (v(), l(Be(In.value.component), Ie({ key: 8 }, In.value.props, {
				ref_key: "tableUnit",
				ref: It,
				onClose: t[3] ||= (e) => L.value.showTable = !1
			}), Ne({
				content: w(() => [(v(), l(S(ht), {
					key: `table_${Mt.value}`,
					colNames: Z.value.colNames,
					head: Z.value.head,
					body: Z.value.body,
					config: Z.value.config,
					title: M.value.table.useDialog ? "" : In.value.title,
					withCloseButton: !M.value.table.useDialog,
					isCursorPointer: P.value,
					onClose: t[2] ||= (e) => L.value.showTable = !1
				}, {
					th: w(({ th: e }) => [Pe(He(e), 1)]),
					td: w(({ td: e }) => [f("div", { innerHTML: e }, null, 8, dt)]),
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
			}, [M.value.table.useDialog ? {
				name: "title",
				fn: w(() => [Pe(He(In.value.title), 1)]),
				key: "0"
			} : void 0, M.value.table.useDialog ? {
				name: "actions",
				fn: w(() => [f("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Cn(M.value.userOptions.callbacks.csv),
					style: _({ cursor: P.value ? "pointer" : "default" })
				}, [Fe(S(pt), {
					name: "fileCsv",
					stroke: In.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : u("", !0),
			x(e.$slots, "skeleton", {}, () => [S(F) ? (v(), l(pe, { key: 0 })) : u("", !0)], !0)
		], 46, Ge));
	}
}, [["__scopeId", "data-v-e202bda3"]]);
//#endregion
export { We as n, ft as t };
