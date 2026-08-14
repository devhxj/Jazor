import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Jt as r, K as i, Kt as a, Pt as o, S as s, X as c, Zt as l, i as ee, jt as te, pt as ne, q as re, r as ie, t as ae, tt as oe, w as se } from "./lib-Bttd6u5E.js";
import { n as ce } from "./useHints-Dq_w2E8B.js";
import { t as le } from "./useConfig-DlNpz6P8.js";
import { t as ue } from "./usePrinter-DN5bYhTG.js";
import { n as de, t as fe } from "./BaseScanner-DZvpgOjM.js";
import { t as pe } from "./useNestedProp-vPNvh7rV.js";
import { t as me } from "./useThemeCheck-C43Tcqmk.js";
import { t as he } from "./useChartExport-DNiwdPmb.js";
import { t as ge } from "./img-Bnokohej.js";
import { n as _e } from "./Title-BE3qg9xl.js";
import { t as ve } from "./Shape-C21CMlWS.js";
import { t as ye } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as be, t as xe } from "./useResponsive-ZtArZtUf.js";
import { t as Se } from "./A11yDataTable-DdRsVULz.js";
import { t as Ce } from "./useUserOptionState-DK-_1ddE.js";
import { t as we } from "./useChartAccessibility-DYqac8yF.js";
import { t as Te } from "./Legend-CQxUgOd-.js";
import { t as Ee } from "./vue_ui_chord-DPfS1Umc.js";
import { Fragment as u, Teleport as De, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createSlots as Oe, createTextVNode as ke, createVNode as Ae, defineAsyncComponent as g, guardReactiveProps as _, mergeProps as je, nextTick as Me, normalizeClass as v, normalizeProps as y, normalizeStyle as b, onBeforeUnmount as Ne, onMounted as Pe, openBlock as x, ref as S, renderList as C, renderSlot as w, resolveDynamicComponent as Fe, shallowRef as Ie, toDisplayString as T, toRefs as Le, unref as E, useCssVars as Re, watch as ze, withCtx as D, withModifiers as Be } from "vue";
//#region src/components/vue-ui-chord.vue
var Ve = /* @__PURE__ */ e({ default: () => yt }), He = ["id"], Ue = ["id"], We = ["id"], Ge = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], Ke = ["width", "height"], qe = { key: 1 }, Je = ["id", "d"], Ye = { key: 2 }, Xe = ["transform"], Ze = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Qe = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], $e = ["d", "fill"], et = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onClick",
	"onMouseleave"
], tt = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], nt = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], rt = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], it = { key: 0 }, at = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], ot = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], st = [
	"cx",
	"cy",
	"r",
	"stroke",
	"stroke-width",
	"fill"
], ct = [
	"transform",
	"fill",
	"text-anchor",
	"font-size",
	"font-weight"
], lt = { key: 1 }, ut = [
	"font-size",
	"font-weight",
	"fill"
], dt = ["href"], ft = [
	"transform",
	"text-anchor",
	"font-size",
	"font-weight",
	"fill",
	"innerHTML"
], pt = {
	key: 5,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, mt = {
	key: 6,
	class: "vue-data-ui-watermark"
}, ht = ["id"], gt = ["onClick"], _t = {
	key: 9,
	"data-dom-to-png-ignore": "",
	class: "reset-wrapper"
}, vt = { style: {
	"text-align": "right",
	width: "100%"
} }, yt = /*#__PURE__*/ ye({
	__name: "vue-ui-chord",
	props: {
		dataset: {
			type: Object,
			default() {
				return {};
			}
		},
		config: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	emits: [
		"selectLegend",
		"selectGroup",
		"selectRibbon",
		"copyAlt"
	],
	setup(e, { expose: ye, emit: Ve }) {
		Re((e) => ({ bdd35a12: e.slicerColor }));
		let yt = g(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), bt = g(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), xt = g(() => import("./DataTable-BbKgJ5UI.js")), St = g(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ct = g(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), wt = g(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Tt = g(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_chord: Et } = le(), { isThemeValid: Dt, warnInvalidTheme: Ot } = me(), O = e, kt = Ve, k = S(!!O.dataset && Object.hasOwn(O.dataset, "matrix")), A = S(re()), j = S(null), M = S(null), N = S(null), At = S(0), P = S(null), jt = S(null), Mt = S(null), Nt = S(null), Pt = S(null), Ft = S(0), It = S(0), Lt = S(0), Rt = S(!1), F = Ie(null), I = Ie(null), zt = S(!1), Bt = S(null), Vt = S(null), L = S("group"), R = S(null), Ht = S(!1), z = S($t());
		ce({
			config: () => z.value,
			dataset: () => O.dataset,
			component: "VueUiChord",
			rules: [{
				test: (e) => e.matrix && e.matrix.length > 12,
				message: [
					"👀 The number of groups > 12, the chart might become hard to read. Consider:",
					"",
					"▶️ Using broader groups to reduce their number."
				]
			}]
		});
		let B = d(() => z.value.userOptions.useCursorPointer), Ut = d(() => r({
			defaultConfig: {
				useCssAnimation: !1,
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					legend: { backgroundColor: "transparent" },
					arcs: {
						stroke: "#6A6A6A",
						labels: { show: !1 }
					},
					ribbons: {
						stroke: "#6A6A6A",
						underlayerOpacity: 0,
						labels: { show: !1 }
					}
				} }
			},
			userConfig: z.value.skeletonConfig ?? {}
		})), { loading: Wt, FINAL_DATASET: V, manualLoading: Gt } = de({
			...Le(O),
			FINAL_CONFIG: z,
			prepareConfig: $t,
			callback: () => {
				Promise.resolve().then(async () => {
					await Me(), U.value.showTable = z.value.table.show;
				});
			},
			skeletonDataset: O.config?.skeletonDataset ?? {
				matrix: [
					[
						12e3,
						6e3,
						9e3,
						3e3
					],
					[
						2e3,
						1e4,
						2e3,
						6001
					],
					[
						8e3,
						1600,
						8e3,
						8001
					],
					[
						1e3,
						1e3,
						1e3,
						7001
					]
				],
				labels: [],
				colors: [
					"#DBDBDB",
					"#C4C4C4",
					"#ADADAD",
					"#969696"
				]
			},
			skeletonConfig: r({
				defaultConfig: z.value,
				userConfig: Ut.value
			})
		}), { userOptionsVisible: Kt, setUserOptionsVisibility: qt, keepUserOptionState: Jt } = Ce({ config: z.value }), { svgRef: H } = we({ config: z.value.style.chart.title }), { isPrinting: Yt, isImaging: Xt, generatePdf: Zt, generateImage: Qt } = ue({
			elementId: `chord_${A.value}`,
			fileName: z.value.style.chart.title.text || "vue-ui-chord",
			options: z.value.userOptions.print
		}), U = S({ showTable: z.value.table.show });
		function $t() {
			let e = pe({
				userConfig: O.config,
				defaultConfig: Et
			}), t = e.theme;
			if (!t) return e;
			if (!Dt.value(e)) return Ot(e), e;
			let n = pe({
				userConfig: Ee[t] || O.config,
				defaultConfig: e
			}), r = pe({
				userConfig: O.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : a[t] || o
			};
		}
		ze(() => O.config, (e) => {
			Wt.value || (z.value = $t()), Kt.value = !z.value.userOptions.showOnChartHover, tn(), Ft.value += 1, It.value += 1, Lt.value += 1, Y.value = z.value.initialRotation, U.value.showTable = z.value.table.show;
		}, { deep: !0 });
		let W = d(() => z.value.debug);
		ze(() => O.dataset, () => {
			en(), tn(), Ft.value += 1, It.value += 1, Lt.value += 1;
		});
		function en() {
			if (te(O.dataset)) {
				oe({
					componentName: "VueUiChord",
					type: "dataset",
					debug: W.value
				}), k.value = !1, Gt.value = !0;
				return;
			}
			let e = ne({
				datasetObject: O.dataset,
				requiredAttributes: ["matrix"]
			});
			if (e.length) {
				W.value && e.forEach((e) => {
					oe({
						componentName: "VueUiChord",
						type: "datasetAttribute",
						property: e
					});
				}), k.value = !1, Gt.value = !0;
				return;
			}
			let t = O.dataset.matrix;
			if (!Array.isArray(t) || t.length < 2) {
				W.value && console.warn("VueUiChord: dataset.matrix requires a minimum of 2 datapoints, for example:\n        \nmatrix:[\n    [1, 1],\n    [1, 1]\n]"), k.value = !1, Gt.value = !0;
				return;
			}
			let n = t.length, r = t.findIndex((e) => !Array.isArray(e) || e.length !== n);
			if (r !== -1) {
				W.value && console.warn(`VueUiChord - Invalid matrix: dataset.matrix at index ${r} has ${Array.isArray(t[r]) ? t[r].length : "NaN"} elements instead of the required ${n}

dataset.matrix[${r}] = [${Array.isArray(t[r]) ? t[r].toString() : "invalid"}]`), k.value = !1, Gt.value = !0;
				return;
			}
			k.value = !0, Gt.value = !1;
		}
		function tn() {
			if (en(), z.value.responsive) {
				let e = be(() => {
					let { width: e, height: t, heightNoTitle: n, heightSource: r, heightTitle: i, heightLegend: a } = xe({
						chart: P.value,
						title: z.value.style.chart.title.text ? jt.value : null,
						legend: z.value.style.chart.legend.show ? Mt.value : null,
						source: Nt.value,
						noTitle: Pt.value
					}), o = i + a + r + n;
					e < t ? P.value.style.width = "100%" : (P.value.style.height = "100%", H.value.style.height = `calc(100% - ${o}px)`);
				}, 100);
				F.value && (I.value && F.value.unobserve(I.value), F.value.disconnect()), F.value = new ResizeObserver(e), I.value = P.value.parentNode, F.value.observe(I.value), e();
			}
			rn.value = setTimeout(() => {
				Rt.value = !0;
			}, 500);
		}
		function nn() {
			if (!V.value || !Object.hasOwn(V.value, "matrix") || V.value.matrix.length < 2) {
				W.value && console.warn("VueUiChord: dataset.matrix requires a minimum of 2 datapoints, for example:\n\nmatrix:[\n  [1, 1],\n  [1, 1]\n]"), k.value = !1;
				return;
			}
			V.value.matrix.forEach((e, t) => {
				e.length !== V.value.matrix.length && (W.value && console.warn(`VueUiChord - Invalid matrix: dataset.matrix at index ${t} has ${e.length} elements instead of the required ${V.value.matrix.length}\n\ndataset.matrix[${t}] = [${e.toString()}] has a length of ${e.length} but should have the same length as the matrix itself (${V.value.matrix.length})`), k.value = !1);
			});
		}
		let rn = S(null);
		Pe(() => {
			zt.value = !0, tn();
		});
		let G = S({
			height: 600,
			width: 600
		}), an = d(() => z.value.userOptions.show && !z.value.style.chart.title.text), on = d(() => se(z.value.customPalette)), K = d(() => ({
			inner: G.value.width * .3 * z.value.style.chart.arcs.innerRadiusRatio,
			outer: G.value.width * .34 * z.value.style.chart.arcs.outerRadiusRatio
		})), sn = d(() => z.value.style.chart.arcs.padAngle / 100), q = d(() => ({
			matrix: V.value.matrix ?? [[0]],
			labels: V.value.labels ?? [""],
			colors: V.value.colors && Array.isArray(V.value.colors) && V.value.colors.length ? V.value.colors.map((e) => s(e)) : V.value.matrix.map((e, t) => on.value[t] || o[t] || o[t % o.length])
		}));
		function cn(e, t) {
			let n = e.length, r = K.value.inner, i = Array(n).fill(0), a = 0;
			for (let t = 0; t < n; t += 1) for (let r = 0; r < n; r += 1) i[t] += e[t][r], a += e[t][r];
			let o = (2 * Math.PI - t * n) / a, s = [], c = 0;
			for (let e = 0; e < n; e += 1) {
				let n = c, r = n + i[e] * o;
				s.push({
					index: e,
					pattern: `pattern_${A.value}_${e}`,
					startAngle: n,
					endAngle: r,
					name: q.value.labels[e],
					id: re(),
					color: q.value.colors[e],
					proportion: i[e] / a
				}), c = r + t;
			}
			let l = [];
			for (let t = 0; t < n; t += 1) {
				let n = e[t].map((e, t) => ({
					j: t,
					v: e
				}));
				n.sort((e, t) => t.v - e.v);
				let i = s[t].startAngle;
				for (let { j: e, v: a } of n) {
					let n = i, c = n + a * o;
					l.push({
						index: t,
						subIndex: e,
						pattern: `pattern_${A.value}_${t}`,
						startAngle: n,
						endAngle: c,
						value: a,
						groupName: q.value.labels[t],
						groupId: s[t].id,
						groupColor: q.value.colors[t],
						midAngle: (n + c) / 2,
						midBaseX: Math.cos((n + c) / 2 - Math.PI / 2) * r,
						midBaseY: Math.sin((n + c) / 2 - Math.PI / 2) * r
					}), i = c;
				}
			}
			let ee = [];
			for (let e of l) {
				let t = l.find((t) => t.index === e.subIndex && t.subIndex === e.index);
				ee.push({
					source: e,
					target: t,
					id: re()
				});
			}
			return {
				groups: s,
				chords: ee
			};
		}
		let J = d(() => {
			let e = cn(q.value.matrix, sn.value);
			return nn(), e.chords.sort((e, t) => Math.max(t.source.value, t.target.value) - Math.max(e.source.value, e.target.value)), e;
		}), ln = d(() => {
			let e = K.value.outer + z.value.style.chart.ribbons.labels.offset + 12, t = z.value.style.chart.ribbons.labels.fontSize * .6, n = [];
			if (M.value) {
				let r = M.value;
				if (r.source.value) {
					let i = String(r.source.value);
					n.push({
						id: r.id + "-src",
						theta: r.source.midAngle,
						w: i.length * t / e,
						midBaseX: r.source.midBaseX,
						midBaseY: r.source.midBaseY,
						groupColor: r.source.groupColor,
						value: r.source.value
					});
				}
				if (r.target && r.target.value && r.target.value !== r.source.value) {
					let i = String(r.target.value);
					n.push({
						id: r.id + "-tgt",
						theta: r.target.midAngle,
						w: i.length * t / e,
						midBaseX: r.target.midBaseX,
						midBaseY: r.target.midBaseY,
						groupColor: r.target.groupColor,
						value: r.target.value
					});
				}
				return n;
			}
			function r(r) {
				J.value.chords.filter((e) => e.source.groupId === r && e.source.value).forEach((r) => {
					{
						let i = String(r.source.value);
						n.push({
							id: r.id + "-src",
							theta: r.source.midAngle,
							w: i.length * t / e,
							midBaseX: r.source.midBaseX,
							midBaseY: r.source.midBaseY,
							groupColor: r.source.groupColor,
							value: r.source.value
						});
					}
					if (r.target && r.target.value && r.target.value !== r.source.value) {
						let i = String(r.target.value);
						n.push({
							id: r.id + "-tgt",
							theta: r.target.midAngle,
							w: i.length * t / e,
							midBaseX: r.target.midBaseX,
							midBaseY: r.target.midBaseY,
							groupColor: r.target.groupColor,
							value: r.target.value
						});
					}
				});
			}
			return N.value && r(N.value), j.value && r(j.value.id), n;
		}), un = d(() => {
			let e = ln.value.map((e) => ({ ...e })).sort((e, t) => e.theta - t.theta), t = z.value.style.chart.ribbons.labels.minSeparationDeg * Math.PI / 180, n = !0, r = 0;
			for (; n && r++ < 10;) {
				n = !1;
				for (let r = 1; r < e.length; r += 1) {
					let i = e[r - 1], a = e[r], o = i.theta + i.w + t;
					a.theta < o && (a.theta = o, n = !0);
				}
				let r = e[0], i = e[e.length - 1], a = i.theta + i.w + t - 2 * Math.PI;
				r.theta < a && (r.theta = a, n = !0);
			}
			return e;
		});
		function dn(e, t, n, r) {
			let i = e - Math.PI / 2, a = t - Math.PI / 2, o = Math.cos(i) * n, s = Math.sin(i) * n, c = Math.cos(a) * n, l = Math.sin(a) * n, ee = Math.cos(a) * r, te = Math.sin(a) * r, ne = Math.cos(i) * r, re = Math.sin(i) * r, ie = +(t - e > Math.PI);
			return `M${o},${s} A${n},${n} 0 ${ie} 1 ${c},${l} L${ee},${te} A${r},${r} 0 ${ie} 0 ${ne},${re} Z`;
		}
		function fn(e, t) {
			let n = K.value.inner, r = e.startAngle - Math.PI / 2, i = e.endAngle - Math.PI / 2, a = t.startAngle - Math.PI / 2, o = t.endAngle - Math.PI / 2, s = Math.cos(r) * n, c = Math.sin(r) * n, l = Math.cos(i) * n, ee = Math.sin(i) * n, te = Math.cos(a) * n, ne = Math.sin(a) * n, re = Math.cos(o) * n, ie = Math.sin(o) * n;
			return `M${s},${c}A${n},${n} 0 ${+(e.endAngle - e.startAngle > Math.PI)} 1 ${l},${ee}Q0,0 ${te},${ne}A${n},${n} 0 ${+(t.endAngle - t.startAngle > Math.PI)} 1 ${re},${ie}Q0,0 ${s},${c}Z`;
		}
		let pn = d(() => Y.value * Math.PI / 180);
		function mn(e) {
			return (e.startAngle + e.endAngle) / 2;
		}
		function hn(e) {
			return ((mn(e) + pn.value) % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI);
		}
		function gn(e) {
			let t = e + pn.value;
			return t = (t % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI), t > Math.PI ? "end" : "start";
		}
		function _n(e) {
			let t = e - Math.PI / 2;
			return `translate(${Math.cos(t) * (K.value.outer + z.value.style.chart.arcs.labels.offset + 24)},${Math.sin(t) * (K.value.outer + z.value.style.chart.arcs.labels.offset + 24)})`;
		}
		function vn(e, t, n) {
			let r = e - Math.PI / 2, i = t - Math.PI / 2, a = Math.cos(r) * n, o = Math.sin(r) * n, s = Math.cos(i) * n, c = Math.sin(i) * n;
			return `M${a},${o} A${n},${n} 0 ${+(t - e > Math.PI)} 1 ${s},${c}`;
		}
		let Y = S(z.value.initialRotation), yn = S(!1), bn = 0, xn = 0;
		function Sn(e) {
			let t = H.value.getBoundingClientRect(), n = t.left + t.width / 2, r = t.top + t.height / 2, i = e.clientX ?? e.touches[0].clientX, a = e.clientY ?? e.touches[0].clientY;
			return Math.atan2(a - r, i - n);
		}
		function Cn(e) {
			!z.value.enableRotation || X.value || (e.preventDefault(), yn.value = !0, bn = Y.value, xn = Sn(e));
		}
		function wn(e) {
			if (!yn.value) return;
			let t = Sn(e);
			Y.value = bn + (t - xn) * 180 / Math.PI;
		}
		function Tn() {
			yn.value = !1;
		}
		function En(e) {
			return ee(z.value.style.chart.ribbons.labels.formatter, e, c({
				p: z.value.style.chart.ribbons.labels.prefix,
				v: e,
				s: z.value.style.chart.ribbons.labels.suffix,
				r: z.value.style.chart.ribbons.labels.rounding
			}));
		}
		Pe(() => {
			window.addEventListener("mousemove", wn), window.addEventListener("mouseup", Tn), window.addEventListener("touchmove", wn, { passive: !1 }), window.addEventListener("touchend", Tn);
		}), Ne(() => {
			window.removeEventListener("mousemove", wn), window.removeEventListener("mouseup", Tn), window.removeEventListener("touchmove", wn), window.removeEventListener("touchend", Tn), clearTimeout(rn.value), F.value && (I.value && F.value.unobserve(I.value), F.value.disconnect());
		});
		let Dn = S(!1);
		function On(e) {
			Dn.value = e, At.value += 1;
		}
		function kn(e, t) {
			z.value.events.datapointEnter && z.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), !N.value && (j.value = e);
		}
		function An(e, t) {
			j.value = null, z.value.events.datapointLeave && z.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		function jn(e, t) {
			kt("selectGroup", e), z.value.events.datapointClick && z.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Mn(e, t) {
			z.value.events.datapointEnter && z.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			}), !N.value && (M.value = e);
		}
		function Nn(e, t) {
			M.value = null, z.value.events.datapointLeave && z.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Pn(e, t) {
			kt("selectRibbon", e), z.value.events.datapointClick && z.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Fn(e) {
			return !N.value && !j.value && !M.value ? .8 : (N.value ? N.value === e.source.groupId : j.value ? j.value.id === e.source.groupId : M.value?.id === e.id) ? 1 : .1;
		}
		function In(e) {
			return !N.value && !j.value && !M.value || (N.value ? N.value === e.id : j.value ? j.value.id === e.id : [M.value?.source.groupId, M.value?.target.groupId].includes(e.id)) ? 1 : .3;
		}
		let X = S(!1);
		function Ln() {
			X.value = !X.value;
		}
		function Rn() {
			U.value.showTable = !U.value.showTable;
		}
		function zn() {
			return J.value;
		}
		let Z = S(null);
		function Bn() {
			let e = z.value.initialRotation;
			Z.value !== null && cancelAnimationFrame(Z.value);
			let t = () => {
				Y.value += (e - Y.value) * .05, Math.abs(Y.value - e) < .1 ? (Y.value = e, Z.value = null) : Z.value = requestAnimationFrame(t);
			};
			Z.value = requestAnimationFrame(t);
		}
		Ne(() => {
			Z.value !== null && cancelAnimationFrame(Z.value);
		});
		function Vn(e) {
			e === N.value ? (N.value = null, kt("selectLegend", null)) : (N.value = e, kt("selectLegend", J.value.groups.find((t) => t.id === e)));
		}
		let Hn = d(() => J.value.groups.map((e, t) => ({
			name: e.name,
			color: e.color,
			shape: "circle",
			patternIndex: t,
			pattern: `pattern_${A.value}_${t}`,
			id: e.id,
			select: () => Vn(e.id),
			opacity: N.value ? N.value === e.id ? 1 : .3 : 1
		}))), Un = d(() => ({
			cy: "chord-div-legend",
			backgroundColor: z.value.style.chart.legend.backgroundColor,
			color: z.value.style.chart.legend.color,
			fontSize: z.value.style.chart.legend.fontSize,
			paddingBottom: 12,
			fontWeight: z.value.style.chart.legend.bold ? "bold" : ""
		})), Q = d(() => ({
			head: J.value.groups.map((e, t) => ({
				name: e.name || t,
				color: e.color
			})),
			body: q.value.matrix
		})), Wn = d(() => {
			let e = [{
				name: "",
				color: null
			}, ...Q.value.head];
			return {
				colNames: e,
				head: e,
				body: Q.value.body.map((e, t) => [Q.value.head[t], ...Q.value.body[t]]),
				config: {
					th: {
						backgroundColor: z.value.table.th.backgroundColor,
						color: z.value.table.th.color,
						outline: z.value.table.th.outline
					},
					td: {
						backgroundColor: z.value.table.td.backgroundColor,
						color: z.value.table.td.color,
						outline: z.value.table.td.outline
					},
					breakpoint: z.value.table.responsiveBreakpoint
				}
			};
		});
		function Gn(e = null) {
			Me(() => {
				let r = q.value.matrix.map((e, t) => [[q.value.labels[t] || t], e]), i = [
					[z.value.style.chart.title.text],
					[z.value.style.chart.title.subtitle.text],
					[[""], ...q.value.labels.map((e, t) => [e || t])]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: z.value.style.chart.title.text || "vue-ui-chord"
				});
			});
		}
		async function Kn({ scale: e = 2 } = {}) {
			if (!P.value) return;
			let { width: t, height: n } = P.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ge({
				domElement: P.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: z.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		function qn(e, t) {
			return `${q.value.labels[t]}${z.value.style.chart.arcs.labels.showPercentage ? c({
				p: " (",
				v: isNaN(e.proportion) ? 0 : e.proportion * 100,
				s: "%)",
				r: z.value.style.chart.arcs.labels.roundingPercentage
			}) : ""}`;
		}
		let Jn = d(() => {
			let e = z.value.table.useDialog && !z.value.table.show, t = U.value.showTable;
			return {
				component: e ? Tt : yt,
				title: `${z.value.style.chart.title.text}${z.value.style.chart.title.subtitle.text ? `: ${z.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: z.value.table.th.backgroundColor,
					color: z.value.table.th.color,
					headerColor: z.value.table.th.color,
					headerBg: z.value.table.th.backgroundColor,
					isFullscreen: Dn.value,
					fullscreenParent: P.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: B.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: z.value.style.chart.backgroundColor,
							color: z.value.style.chart.color
						},
						head: {
							backgroundColor: z.value.style.chart.backgroundColor,
							color: z.value.style.chart.color
						}
					}
				}
			};
		});
		ze(() => U.value.showTable, (e) => {
			z.value.table.show || (e && z.value.table.useDialog && Bt.value ? Bt.value.open() : "close" in Bt.value && Bt.value.close());
		});
		function Yn() {
			U.value.showTable = !1, Vt.value && Vt.value.setTableIconState(!1);
		}
		let Xn = d(() => z.value.style.chart.backgroundColor), Zn = d(() => z.value.style.chart.legend), Qn = d(() => z.value.style.chart.title), { isCallbackImaging: $n, isCallbackSvg: er, generateSvg: tr, onGenerateImage: nr } = he({
			svg: H,
			title: Qn,
			legend: Zn,
			legendItems: Hn,
			backgroundColor: Xn,
			getSvgCallback: () => z.value.userOptions.callbacks.svg,
			generateImage: Qt
		});
		async function rr() {
			if (kt("copyAlt", {
				config: z.value,
				dataset: q.value
			}), !z.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(z.value.userOptions.callbacks.altCopy({
				config: z.value,
				dataset: q.value
			}));
		}
		let ir = d(() => (j.value ? J.value.chords.filter((e) => e.source.groupId === j.value.id) : N.value ? J.value.chords.filter((e) => e.source.groupId === N.value) : J.value.chords).filter((e) => e.source.value)), $ = d(() => ({
			groups: J.value.groups.map((e, t) => ({
				type: "group",
				index: t,
				item: e
			})),
			ribbons: ir.value.map((e, t) => ({
				type: "ribbon",
				index: t,
				item: {
					...e,
					path: fn(e.source, e.target),
					color: q.value.colors[e.source.index]
				}
			}))
		})), ar = d(() => ({
			headers: ["", ...Q.value.head.map((e) => e.name)],
			rows: Q.value.body.map((e, t) => [Q.value.head[t]?.name ?? `${t}`, ...e.map((e) => En(e))])
		}));
		function or(e) {
			let t = $.value.groups[e];
			t && (L.value = "group", R.value = e, M.value = null, j.value = t.item);
		}
		function sr(e) {
			let t = $.value.ribbons[e];
			t && (L.value = "ribbon", R.value = e, j.value = null, M.value = t.item);
		}
		function cr() {
			R.value = null, L.value = "group", j.value = null, M.value = null;
		}
		function lr() {
			Ht.value = !0, L.value = "group", R.value = null;
		}
		function ur() {
			Ht.value = !1, cr();
		}
		function dr(e) {
			if (!H.value || X.value || document.activeElement !== H.value) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "ArrowUp", i = e.key === "ArrowDown", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				cr();
				return;
			}
			if (r) {
				L.value = "group", $.value.groups.length && or(R.value === null ? 0 : Math.min(R.value, $.value.groups.length - 1));
				return;
			}
			if (i) {
				if (!$.value.ribbons.length) return;
				L.value = "ribbon", sr(R.value === null ? 0 : Math.min(R.value, $.value.ribbons.length - 1));
				return;
			}
			if (a) {
				if (L.value === "group") {
					let e = $.value.groups[R.value];
					e && jn(e.item, e.index);
				} else {
					let e = $.value.ribbons[R.value];
					e && Pn(e.item, e.index);
				}
				return;
			}
			let s = L.value === "group" ? $.value.groups : $.value.ribbons;
			if (!s.length) return;
			let c = R.value;
			c === null || c < 0 || c >= s.length ? c = n ? 0 : s.length - 1 : n ? (c += 1, c >= s.length && (c = 0)) : t && (--c, c < 0 && (c = s.length - 1)), L.value === "group" ? or(c) : sr(c);
		}
		return ye({
			getData: zn,
			getImage: Kn,
			generateCsv: Gn,
			generateImage: Qt,
			generateSvg: tr,
			generatePdf: Zt,
			toggleAnnotator: Ln,
			toggleTable: Rn,
			toggleFullscreen: On,
			copyAlt: rr
		}), (e, t) => (x(), m("div", {
			ref_key: "chordChart",
			ref: P,
			class: v({
				"vue-data-ui-component": !0,
				"vue-ui-chord": !0,
				"vue-data-ui-wrapper-fullscreen": Dn.value,
				"vue-data-ui-responsive": z.value.responsive
			}),
			style: b(`font-family:${z.value.style.fontFamily};width:100%; text-align:center;background:${z.value.style.chart.backgroundColor}`),
			id: `chord_${A.value}`,
			onMouseenter: t[2] ||= () => E(qt)(!0),
			onMouseleave: t[3] ||= () => E(qt)(!1)
		}, [
			h("div", {
				id: `chart-instructions-${A.value}`,
				class: "sr-only"
			}, [h("p", null, T(z.value.a11y.translations.keyboardNavigation), 1)], 8, Ue),
			ar.value?.rows?.length ? (x(), f(Se, {
				key: 0,
				uid: A.value,
				head: ar.value.headers,
				body: ar.value.rows,
				notice: z.value.a11y.translations.tableAvailable,
				caption: z.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : p("", !0),
			z.value.userOptions.buttons.annotator && E(H) ? (x(), f(E(Ct), {
				key: 1,
				color: z.value.style.chart.color,
				backgroundColor: z.value.style.chart.backgroundColor,
				active: X.value,
				svgRef: E(H),
				isCursorPointer: B.value,
				onClose: Ln
			}, {
				"annotator-action-close": D(() => [w(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": D(({ color: t }) => [w(e.$slots, "annotator-action-color", y(_({ color: t })), void 0, !0)]),
				"annotator-action-draw": D(({ mode: t }) => [w(e.$slots, "annotator-action-draw", y(_({ mode: t })), void 0, !0)]),
				"annotator-action-undo": D(({ disabled: t }) => [w(e.$slots, "annotator-action-undo", y(_({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": D(({ disabled: t }) => [w(e.$slots, "annotator-action-redo", y(_({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": D(({ disabled: t }) => [w(e.$slots, "annotator-action-delete", y(_({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"color",
				"backgroundColor",
				"active",
				"svgRef",
				"isCursorPointer"
			])) : p("", !0),
			w(e.$slots, "userConfig", {}, void 0, !0),
			an.value ? (x(), m("div", {
				key: 2,
				ref_key: "noTitle",
				ref: Pt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : p("", !0),
			z.value.style.chart.title.text ? (x(), m("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: jt,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(x(), f(_e, {
				key: `title_${Ft.value}`,
				config: {
					title: {
						cy: "chord-div-title",
						...z.value.style.chart.title
					},
					subtitle: {
						cy: "chord-div-subtitle",
						...z.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : p("", !0),
			h("div", { id: `legend-top-${A.value}` }, null, 8, We),
			z.value.userOptions.show && k.value && (E(Jt) || E(Kt)) ? (x(), f(E(wt), {
				ref_key: "userOptionsRef",
				ref: Vt,
				key: `user_option_${At.value}`,
				backgroundColor: z.value.style.chart.backgroundColor,
				color: z.value.style.chart.color,
				isPrinting: E(Yt),
				isImaging: E(Xt),
				uid: A.value,
				hasTooltip: !1,
				hasPdf: z.value.userOptions.buttons.pdf,
				hasImg: z.value.userOptions.buttons.img,
				hasSvg: z.value.userOptions.buttons.svg,
				hasXls: z.value.userOptions.buttons.csv,
				hasTable: z.value.userOptions.buttons.table,
				hasLabel: !1,
				hasFullscreen: z.value.userOptions.buttons.fullscreen,
				hasAltCopy: z.value.userOptions.buttons.altCopy,
				isFullscreen: Dn.value,
				chartElement: P.value,
				position: z.value.userOptions.position,
				titles: { ...z.value.userOptions.buttonTitles },
				hasAnnotator: z.value.userOptions.buttons.annotator,
				isAnnotation: X.value,
				callbacks: z.value.userOptions.callbacks,
				printScale: z.value.userOptions.print.scale,
				tableDialog: z.value.table.useDialog,
				isCursorPointer: B.value,
				onToggleFullscreen: On,
				onGeneratePdf: E(Zt),
				onGenerateCsv: Gn,
				onGenerateImage: E(nr),
				onGenerateSvg: E(tr),
				onToggleTable: Rn,
				onToggleAnnotator: Ln,
				onCopyAlt: rr,
				style: b({ visibility: E(Jt) ? E(Kt) ? "visible" : "hidden" : "visible" })
			}, Oe({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: D(({ isOpen: t, color: n }) => [w(e.$slots, "menuIcon", y(_({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: D(() => [w(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: D(() => [w(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: D(() => [w(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: D(() => [w(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: D(() => [w(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: D(({ toggleFullscreen: t, isFullscreen: n }) => [w(e.$slots, "optionFullscreen", y(_({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: D(({ toggleAnnotator: t, isAnnotator: n }) => [w(e.$slots, "optionAnnotator", y(_({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: D(({ altCopy: t }) => [w(e.$slots, "optionAltCopy", y(_({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: D(() => [w(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: D(() => [w(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasFullscreen.hasAltCopy.isFullscreen.chartElement.position.titles.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : p("", !0),
			(x(), m("svg", {
				xmlns: E(ae),
				ref_key: "svgRef",
				ref: H,
				viewBox: `0 0 ${G.value.width} ${G.value.height}`,
				"aria-describedby": `chart-instructions-${A.value}`,
				tabindex: "0",
				preserveAspectRatio: "xMidYMid meet",
				style: { overflow: "visible" },
				class: v({
					"vue-ui-chord-rotating": yn.value,
					"vue-ui-chord-idle": !yn.value
				}),
				onMousedown: Be(Cn, ["prevent"]),
				onTouchstart: Be(Cn, ["prevent"]),
				onFocus: lr,
				onBlur: ur,
				onKeydown: dr
			}, [
				Ae(E(St)),
				e.$slots["chart-background"] ? (x(), m("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: G.value.width <= 0 ? 10 : G.value.width,
					height: G.value.height <= 0 ? 10 : G.value.height,
					style: { pointerEvents: "none" }
				}, [w(e.$slots, "chart-background", {}, void 0, !0)], 8, Ke)) : p("", !0),
				z.value.style.chart.arcs.labels.curved ? (x(), m("defs", qe, [(x(!0), m(u, null, C(J.value.groups, (e, t) => (x(), m("path", {
					key: `labelPath-${t}`,
					id: `labelPath-${t}_${A.value}`,
					d: vn(e.startAngle, e.endAngle, (K.value.inner + K.value.outer) / 2 + z.value.style.chart.arcs.labels.offset),
					fill: "none"
				}, null, 8, Je))), 128))])) : p("", !0),
				e.$slots.pattern ? (x(), m("g", Ye, [(x(!0), m(u, null, C(J.value.groups, (t, n) => (x(), m("defs", null, [w(e.$slots, "pattern", je({ ref_for: !0 }, {
					seriesIndex: t.index,
					patternId: `pattern_${A.value}_${n}`
				}), void 0, !0)]))), 256))])) : p("", !0),
				h("g", { transform: `translate(${G.value.width / 2}, ${G.value.height / 2}) rotate(${Y.value})` }, [
					(x(!0), m(u, null, C(J.value.groups, (t, n) => (x(), m("g", null, [(x(), m("path", {
						class: v({
							"vue-ui-chord-arc": !0,
							"vue-ui-chord-arc-animated": z.value.useCssAnimation && !Rt.value
						}),
						key: `arc-${n}`,
						d: dn(t.startAngle, t.endAngle, K.value.outer, K.value.inner),
						fill: q.value.colors[n],
						stroke: z.value.style.chart.arcs.stroke,
						"stroke-width": z.value.style.chart.arcs.strokeWidth,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						style: b({ opacity: In(t) }),
						onMouseenter: (e) => kn(t, n),
						onMouseleave: (e) => An(t, n),
						onClick: (e) => jn(t, n)
					}, null, 46, Ze)), e.$slots.pattern ? (x(), m("path", {
						class: v({
							"vue-ui-chord-arc": !0,
							"vue-ui-chord-arc-animated": z.value.useCssAnimation && !Rt.value
						}),
						key: `arc-${n}`,
						d: dn(t.startAngle, t.endAngle, K.value.outer, K.value.inner),
						fill: `url(#${t.pattern})`,
						stroke: z.value.style.chart.arcs.stroke,
						"stroke-width": z.value.style.chart.arcs.strokeWidth,
						"stroke-linecap": "round",
						"stroke-linejoin": "round",
						style: b({
							opacity: In(t),
							pointerEvents: "none"
						})
					}, null, 14, Qe)) : p("", !0)]))), 256)),
					h("g", null, [
						(x(!0), m(u, null, C(j.value ? J.value.chords.filter((e) => e.source.groupId === j.value.id) : N.value ? J.value.chords.filter((e) => e.source.groupId === N.value) : J.value.chords, (t, n) => (x(), m(u, { key: `ribbon-${t.id}` }, [
							t.source.value ? (x(), m("path", {
								key: 0,
								class: v({ "vue-ui-chord-ribbon": !0 }),
								d: fn(t.source, t.target),
								fill: z.value.style.chart.backgroundColor,
								style: b({ opacity: z.value.style.chart.ribbons.underlayerOpacity })
							}, null, 12, $e)) : p("", !0),
							t.source.value ? (x(), m("path", {
								key: 1,
								class: v({ "vue-ui-chord-ribbon": !0 }),
								d: fn(t.source, t.target),
								fill: q.value.colors[t.source.index],
								stroke: z.value.style.chart.ribbons.stroke,
								"stroke-width": z.value.style.chart.ribbons.strokeWidth,
								"stroke-linecap": "round",
								"stroke-linejoin": "round",
								style: b({ opacity: Fn(t) }),
								onMouseenter: (e) => Mn({
									...t,
									path: fn(t.source, t.target),
									color: q.value.colors[t.source.index]
								}, n),
								onClick: (e) => Pn({
									...t,
									color: q.value.colors[t.source.index]
								}, n),
								onMouseleave: (e) => Nn({
									...t,
									color: q.value.colors[t.source.index]
								}, n)
							}, null, 44, et)) : p("", !0),
							t.source.value && e.$slots.pattern ? (x(), m("path", {
								key: 2,
								class: v({ "vue-ui-chord-ribbon": !0 }),
								d: fn(t.source, t.target),
								fill: `url(#pattern_${A.value}_${t.source.index})`,
								stroke: z.value.style.chart.ribbons.stroke,
								"stroke-width": z.value.style.chart.ribbons.strokeWidth,
								"stroke-linecap": "round",
								"stroke-linejoin": "round",
								style: b({
									opacity: Fn(t),
									pointerEvents: "none"
								})
							}, null, 12, tt)) : p("", !0)
						], 64))), 128)),
						M.value ? (x(), m("path", {
							key: 0,
							d: M.value.path,
							fill: M.value.color,
							stroke: z.value.style.chart.ribbons.stroke,
							"stroke-width": z.value.style.chart.ribbons.strokeWidth,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							class: v({ "vue-ui-chord-ribbon": !0 }),
							style: { pointerEvents: "none" }
						}, null, 8, nt)) : p("", !0),
						M.value && e.$slots.pattern ? (x(), m("path", {
							key: 1,
							d: M.value.path,
							fill: `url(#${M.value.source.pattern})`,
							stroke: z.value.style.chart.ribbons.stroke,
							"stroke-width": z.value.style.chart.ribbons.strokeWidth,
							"stroke-linecap": "round",
							"stroke-linejoin": "round",
							class: v({ "vue-ui-chord-ribbon": !0 }),
							style: { pointerEvents: "none" }
						}, null, 8, rt)) : p("", !0)
					]),
					(j.value || M.value || N.value) && z.value.style.chart.ribbons.labels.show ? (x(), m("g", it, [(x(!0), m(u, null, C(un.value, (e) => (x(), m(u, { key: e.id }, [
						h("line", {
							x1: e.midBaseX,
							y1: e.midBaseY,
							x2: Math.cos(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 12),
							y2: Math.sin(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 12),
							stroke: z.value.style.chart.backgroundColor,
							"stroke-width": z.value.style.chart.ribbons.labels.connector.strokeWidth * 3,
							"pointer-events": "none"
						}, null, 8, at),
						h("line", {
							x1: e.midBaseX,
							y1: e.midBaseY,
							x2: Math.cos(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 12),
							y2: Math.sin(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 12),
							stroke: z.value.style.chart.ribbons.labels.connector.stroke,
							"stroke-width": z.value.style.chart.ribbons.labels.connector.strokeWidth,
							"pointer-events": "none"
						}, null, 8, ot),
						z.value.style.chart.ribbons.labels.marker.show ? (x(), m("circle", {
							key: 0,
							cx: Math.cos(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 12),
							cy: Math.sin(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 12),
							r: z.value.style.chart.ribbons.labels.marker.radius,
							stroke: z.value.style.chart.ribbons.labels.marker.stroke,
							"stroke-width": z.value.style.chart.ribbons.labels.marker.strokeWidth,
							fill: e.groupColor,
							"pointer-events": "none"
						}, null, 8, st)) : p("", !0),
						h("text", {
							transform: `
                            translate(
                                ${Math.cos(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 24)},
                                ${Math.sin(e.theta - Math.PI / 2) * (K.value.outer + z.value.style.chart.ribbons.labels.offset + 24)}
                            ) rotate(${-Y.value})
                            `,
							fill: z.value.style.chart.ribbons.labels.useSerieColor ? e.groupColor : z.value.style.chart.ribbons.labels.color,
							"text-anchor": gn(e.theta),
							"font-size": z.value.style.chart.ribbons.labels.fontSize,
							"font-weight": z.value.style.chart.ribbons.labels.bold ? "bold" : "normal",
							dy: ".35em",
							"pointer-events": "none"
						}, T(En(e.value)), 9, ct)
					], 64))), 128))])) : p("", !0),
					z.value.style.chart.arcs.labels.show ? (x(), m("g", lt, [z.value.style.chart.arcs.labels.curved ? (x(!0), m(u, { key: 0 }, C(J.value.groups, (e, t) => (x(), m("text", {
						class: "vue-ui-chord-label-curved",
						key: `curved-label-${t}`,
						"font-size": z.value.style.chart.arcs.labels.fontSize,
						"font-weight": z.value.style.chart.arcs.labels.bold ? "bold" : "normal",
						fill: z.value.style.chart.arcs.labels.adaptColorToBackground ? E(ie)(q.value.colors[t]) : z.value.style.chart.arcs.labels.color
					}, [h("textPath", {
						href: `#labelPath-${t}_${A.value}`,
						startOffset: "50%",
						"text-anchor": "middle"
					}, T(q.value.labels[t]) + T(z.value.style.chart.arcs.labels.showPercentage ? E(c)({
						p: " (",
						v: isNaN(e.proportion) ? 0 : e.proportion * 100,
						s: "%)",
						r: z.value.style.chart.arcs.labels.roundingPercentage
					}) : ""), 9, dt)], 8, ut))), 128)) : !j.value && !M.value && !N.value ? (x(!0), m(u, { key: 1 }, C(J.value.groups, (e, t) => (x(), m("text", {
						class: "vue-ui-chord-label-straight",
						key: `label-${t}`,
						transform: `
                                ${_n((e.startAngle + e.endAngle) / 2)}
                                rotate(${-Y.value})
                            `,
						dy: ".35em",
						"text-anchor": hn(e) > Math.PI ? "end" : "start",
						"font-size": z.value.style.chart.arcs.labels.fontSize,
						"font-weight": z.value.style.chart.arcs.labels.bold ? "bold" : "normal",
						fill: z.value.style.chart.arcs.labels.color,
						innerHTML: E(i)({
							content: E(l)(qn(e, t)),
							fontSize: z.value.style.chart.arcs.labels.fontSize,
							fill: z.value.style.chart.arcs.labels.color,
							x: 0,
							y: 0
						})
					}, null, 8, ft))), 128)) : p("", !0)])) : p("", !0)
				], 8, Xe),
				w(e.$slots, "svg", { svg: {
					height: 600,
					width: 600,
					isPrintingImg: E(Yt) || E(Xt) || E($n),
					isPrintingSvg: E(er)
				} }, void 0, !0)
			], 42, Ge)),
			e.$slots.hint ? (x(), m("div", pt, [w(e.$slots, "hint", y(_({
				hint: z.value.a11y.translations.keyboardNavigation,
				isVisible: Ht.value
			})), void 0, !0)])) : p("", !0),
			e.$slots.watermark ? (x(), m("div", mt, [w(e.$slots, "watermark", y(_({ isPrinting: E(Yt) || E(Xt) || E($n) || E(er) })), void 0, !0)])) : p("", !0),
			h("div", { id: `legend-bottom-${A.value}` }, null, 8, ht),
			zt.value && (z.value.style.chart.legend.show || e.$slots.legend) ? (x(), f(De, {
				key: 7,
				to: z.value.style.chart.legend.position === "top" ? `#legend-top-${A.value}` : `#legend-bottom-${A.value}`
			}, [h("div", {
				ref_key: "chartLegend",
				ref: Mt
			}, [w(e.$slots, "legend", { legend: Hn.value }, () => [z.value.style.chart.legend.show ? (x(), f(Te, {
				key: `legend_${Lt.value}`,
				legendSet: Hn.value,
				config: Un.value,
				isCursorPointer: B.value,
				onClickMarker: t[0] ||= ({ legend: e }) => Vn(e.id)
			}, Oe({
				item: D(({ legend: e }) => [h("div", {
					style: b({ opacity: N.value ? N.value === e.id ? 1 : .3 : 1 }),
					onClick: (t) => e.select()
				}, T(e.name), 13, gt)]),
				_: 2
			}, [e.$slots.pattern ? {
				name: "legend-pattern",
				fn: D(({ legend: e, index: t }) => [Ae(ve, {
					shape: e.shape,
					radius: 30,
					stroke: "none",
					plot: {
						x: 30,
						y: 30
					},
					fill: `url(#pattern_${A.value}_${t})`
				}, null, 8, ["shape", "fill"])]),
				key: "0"
			} : void 0]), 1032, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : p("", !0)], !0)], 512)], 8, ["to"])) : p("", !0),
			e.$slots.source ? (x(), m("div", {
				key: 8,
				ref_key: "source",
				ref: Nt,
				dir: "auto"
			}, [w(e.$slots, "source", {}, void 0, !0)], 512)) : p("", !0),
			Y.value === z.value.initialRotation ? p("", !0) : (x(), m("div", _t, [w(e.$slots, "reset-action", { reset: Bn }, () => [h("button", {
				"data-cy-reset": "",
				tabindex: "0",
				role: "button",
				class: "vue-data-ui-refresh-button",
				style: b({
					background: z.value.style.chart.backgroundColor,
					cursor: B.value ? "pointer" : "default"
				}),
				onClick: Bn
			}, [Ae(E(bt), {
				name: "refresh",
				stroke: z.value.style.chart.color
			}, null, 8, ["stroke"])], 4)], !0)])),
			k.value && z.value.userOptions.buttons.table ? (x(), f(Fe(Jn.value.component), je({ key: 10 }, Jn.value.props, {
				ref_key: "tableUnit",
				ref: Bt,
				onClose: Yn
			}), Oe({
				content: D(() => [(x(), f(E(xt), {
					key: `table_${It.value}`,
					colNames: Wn.value.colNames,
					head: Wn.value.head,
					body: Wn.value.body,
					config: Wn.value.config,
					title: z.value.table.useDialog ? "" : Jn.value.title,
					withCloseButton: !z.value.table.useDialog,
					isCursorPointer: B.value,
					onClose: Yn
				}, {
					th: D(({ th: e }) => [ke(T(e.name), 1)]),
					td: D(({ td: e }) => [h("div", vt, T(e.name ? e.name : En(e)), 1)]),
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
			}, [z.value.table.useDialog ? {
				name: "title",
				fn: D(() => [ke(T(Jn.value.title), 1)]),
				key: "0"
			} : void 0, z.value.table.useDialog ? {
				name: "actions",
				fn: D(() => [h("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => Gn(z.value.userOptions.callbacks.csv),
					style: b({ cursor: B.value ? "pointer" : "default" })
				}, [Ae(E(bt), {
					name: "fileCsv",
					stroke: Jn.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : p("", !0),
			w(e.$slots, "skeleton", {}, () => [E(Wt) ? (x(), f(fe, { key: 0 })) : p("", !0)], !0)
		], 46, He));
	}
}, [["__scopeId", "data-v-b9aaa15d"]]);
//#endregion
export { Ve as n, yt as t };
