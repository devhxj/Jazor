import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Jt as t, Kt as n, Pt as r, X as i, i as a, jt as o, pt as ee, q as te, qt as s, r as ne, t as re, tt as ie, w as ae } from "./lib-Bttd6u5E.js";
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
import { t as be } from "./A11yDataTable-DdRsVULz.js";
import { t as xe } from "./useUserOptionState-DK-_1ddE.js";
import { t as Se } from "./useChartAccessibility-DYqac8yF.js";
import { t as Ce } from "./useAutoSizeLabelsInsideViewbox-DvDwcwi_.js";
import { t as we } from "./vue_ui_relation_circle-D0p4mXmv.js";
import { Fragment as c, computed as l, createBlock as u, createCommentVNode as d, createElementBlock as f, createElementVNode as p, createSlots as Te, createTextVNode as Ee, createVNode as De, defineAsyncComponent as m, guardReactiveProps as h, mergeProps as Oe, normalizeClass as ke, normalizeProps as g, normalizeStyle as _, onBeforeUnmount as Ae, onMounted as je, openBlock as v, ref as y, renderList as b, renderSlot as x, shallowRef as Me, toDisplayString as S, toRefs as Ne, unref as C, useCssVars as Pe, watch as Fe, withCtx as w } from "vue";
//#region src/components/vue-ui-relation-circle.vue
var Ie = /* @__PURE__ */ e({ default: () => T }), Le = ["id"], Re = ["id"], ze = { style: { position: "relative" } }, Be = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], Ve = ["width", "height"], He = [
	"cx",
	"cy",
	"r",
	"stroke",
	"stroke-width"
], Ue = { key: 1 }, We = [
	"stroke",
	"d",
	"stroke-width"
], Ge = { style: { "pointer-events": "none" } }, Ke = [
	"cx",
	"cy",
	"fill",
	"r",
	"stroke"
], qe = [
	"x",
	"y",
	"fill",
	"font-size"
], Je = { key: 2 }, Ye = [
	"stroke",
	"stroke-width",
	"x1",
	"x2",
	"y1",
	"y2"
], Xe = { style: { "pointer-events": "none" } }, Ze = [
	"cx",
	"cy",
	"fill",
	"r",
	"stroke"
], Qe = [
	"x",
	"y",
	"fill",
	"font-size"
], $e = [
	"text-anchor",
	"transform",
	"x",
	"y",
	"font-weight",
	"font-size",
	"fill",
	"text-decoration",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], et = [
	"cx",
	"cy",
	"fill",
	"stroke",
	"r",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], tt = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, nt = {
	key: 5,
	class: "vue-data-ui-watermark"
}, T = /*#__PURE__*/ _e({
	__name: "vue-ui-relation-circle",
	props: {
		dataset: {
			type: Array,
			default() {
				return [];
			}
		},
		config: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	emits: ["copyAlt"],
	setup(e, { expose: _e, emit: Ie }) {
		Pe((e) => ({
			ebbe27d4: jt.value,
			v6d2782f5: At.value,
			cec75fd2: Mt.value
		}));
		let T = m(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), rt = m(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), it = m(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_relation_circle: at } = ce(), { isThemeValid: ot, warnInvalidTheme: st } = pe(), E = e, ct = Ie, lt = l(() => !!E.dataset && Object.keys(E.dataset).length), D = y(te()), ut = y(0), O = y(null), dt = y(null), ft = y(null), pt = y(null), mt = y(0), k = y(null), A = y(!1), j = y(St());
		oe({
			config: () => j.value,
			dataset: () => E.dataset,
			component: "VueUiRelationCircle",
			rules: [se.noHint]
		});
		let M = l(() => j.value.userOptions.useCursorPointer), ht = l(() => t({
			defaultConfig: {
				userOptions: { show: !1 },
				customPalette: Array(7).fill("#CACACA"),
				style: {
					backgroundColor: "#99999930",
					labels: { color: "#6A6A6A" },
					circle: { stroke: "#6A6A6A" },
					plot: {
						color: "#6A6A6A",
						useSerieColor: !0
					},
					links: { maxWidth: 2 }
				}
			},
			userConfig: j.value.skeletonConfig ?? {}
		})), { loading: gt, FINAL_DATASET: _t, manualLoading: vt } = ue({
			...Ne(E),
			FINAL_CONFIG: j,
			prepareConfig: St,
			skeletonDataset: E.config?.skeletonDataset ?? [
				{
					id: "A",
					label: "_",
					relations: [
						"B",
						"C",
						"D",
						"E",
						"F",
						"G"
					]
				},
				{
					id: "B",
					label: "_",
					relations: ["A"]
				},
				{
					id: "C",
					label: "_",
					relations: ["A"]
				},
				{
					id: "D",
					label: "_",
					relations: ["A"]
				},
				{
					id: "E",
					label: "_",
					relations: ["A"]
				},
				{
					id: "F",
					label: "_",
					relations: ["A"]
				},
				{
					id: "G",
					label: "_",
					relations: ["A"]
				}
			],
			skeletonConfig: t({
				defaultConfig: j.value,
				userConfig: ht.value
			})
		}), { userOptionsVisible: yt, setUserOptionsVisibility: bt, keepUserOptionState: xt } = xe({ config: j.value }), { svgRef: N } = Se({ config: j.value.style.title });
		function St() {
			let e = fe({
				userConfig: E.config,
				defaultConfig: at
			}), t = e.theme;
			if (!t) return e;
			if (!ot.value(e)) return st(e), e;
			let i = fe({
				userConfig: we[t] || E.config,
				defaultConfig: e
			}), a = fe({
				userConfig: E.config,
				defaultConfig: i
			});
			return {
				...a,
				customPalette: a.customPalette.length ? a.customPalette : n[t] || r
			};
		}
		Fe(() => E.config, (e) => {
			j.value = St(), yt.value = !j.value.userOptions.showOnChartHover, B.value = j.value.style.size, V.value = j.value.style.weightLabels.size, H.value = j.value.style.plot.radius, U.value = j.value.style.labels.fontSize, W.value.height = j.value.style.size, W.value.width = j.value.style.size, Pt(), mt.value += 1;
		}, { deep: !0 }), Fe(() => E.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (vt.value = !1);
		}, { deep: !0 });
		let { isPrinting: Ct, isImaging: P, generatePdf: wt, generateImage: Tt } = le({
			elementId: `relation_circle_${D.value}`,
			fileName: j.value.style.title.text || "vue-ui-relation-circle",
			options: j.value.userOptions.print
		}), Et = l(() => j.value.userOptions.show && !j.value.style.title.text), Dt = l(() => ae(j.value.customPalette)), F = y([]), I = y([]), L = y({}), R = y([]), Ot = y(0), z = l(() => _t.value.slice(0, j.value.style.limit).map((e) => {
			let t = Array.isArray(e.relations) ? e.relations : [];
			return {
				...e,
				weights: Array.isArray(e.weights) ? e.weights : Array(t.length).fill(1),
				relations: t
			};
		}));
		Fe(z, () => {
			F.value = [], I.value = [], Lt(), zt();
		});
		let B = y(j.value.style.size), V = y(j.value.style.weightLabels.size), H = y(j.value.style.plot.radius), U = y(j.value.style.labels.fontSize), W = y({
			height: j.value.style.size,
			width: j.value.style.size
		}), G = l({
			get() {
				return B.value * j.value.style.circle.radiusProportion;
			},
			set(e) {
				return e;
			}
		}), kt = l(() => j.value.style.links.curved), At = l(() => `${j.value.style.animation.speedMs}ms`), jt = l(() => G.value * 2), Mt = l(() => G.value * 4), K = Me(null), q = Me(null);
		je(() => {
			Pt(), document.getElementById(`relation_circle_${D.value}`).addEventListener("click", It);
		});
		let Nt = l(() => j.value.debug);
		function Pt() {
			if (o(E.dataset) ? (ie({
				componentName: "VueUiRelationCircle",
				type: "dataset",
				debug: Nt.value
			}), vt.value = !0) : Nt.value && E.dataset.forEach((e, t) => {
				ee({
					datasetObject: e,
					requiredAttributes: [
						"id",
						"label",
						"relations",
						"weights"
					]
				}).forEach((e) => {
					ie({
						componentName: "VueUiRelationCircle",
						type: "datasetSerieAttribute",
						property: e,
						index: t
					});
				});
			}), o(E.dataset) || (vt.value = j.value.loading), j.value.responsive) {
				let e = ve(() => {
					let { width: e, height: t } = ye({
						chart: O.value,
						title: j.value.style.title.text ? dt.value : null,
						source: ft.value,
						noTitle: pt.value
					});
					requestAnimationFrame(() => {
						B.value = Math.min(e, t), W.value.width = Math.max(.1, e), W.value.height = Math.max(.1, t - 12), G.value = B.value * j.value.style.circle.radiusProportion, F.value = [], I.value = [], Lt(), zt(), Ft(), j.value.responsiveProportionalSizing ? (V.value = s({
							relator: B.value,
							adjuster: j.value.style.size,
							source: j.value.style.weightLabels.size,
							threshold: 6,
							fallback: 6
						}), H.value = s({
							relator: B.value,
							adjuster: j.value.style.size,
							source: j.value.style.plot.radius,
							threshold: 1,
							fallback: 1
						}), U.value = s({
							relator: B.value,
							adjuster: j.value.style.size,
							source: j.value.style.labels.fontSize,
							threshold: 6,
							fallback: 6
						})) : (V.value = j.value.style.weightLabels.size, H.value = j.value.style.plot.radius, U.value = j.value.style.labels.fontSize);
					});
				});
				K.value && (q.value && K.value.unobserve(q.value), K.value.disconnect()), K.value = new ResizeObserver(e), q.value = O.value.parentNode, K.value.observe(q.value);
			} else F.value = [], I.value = [], Lt(), zt();
			Ft();
		}
		Ae(() => {
			document.getElementById(`relation_circle_${D.value}`).removeEventListener("click", It), K.value && (q.value && K.value.unobserve(q.value), K.value.disconnect());
		});
		let { autoSizeLabels: Ft } = Ce({
			svgRef: N,
			fontSize: j.value.style.labels.fontSize,
			minFontSize: j.value.style.labels.minFontSize,
			sizeRef: U,
			labelClass: ".vue-ui-relation-circle-legend"
		});
		function It(e) {
			let t = e.target;
			t && Array.from(t.classList).includes("vue-ui-user-options") || t && Array.from(t.classList).includes("vue-ui-user-options-summary") || t && Array.from(t.classList).includes("vue-data-ui-button") || t && Array.from(t.classList).includes("vue-ui-relation-circle-legend") || (L.value = {}, R.value = []);
		}
		function Lt() {
			let e = 6.28319 / z.value.length, t = 360 / z.value.length, n = 0, i = 0;
			z.value.forEach((a, o) => {
				let ee = a.weights.reduce((e, t) => e + t, 0), te = G.value * Math.cos(n) + W.value.width / 2, s = G.value * Math.sin(n) + W.value.height / 2 + j.value.style.circle.offsetY;
				F.value.push({
					x: te,
					y: s,
					...a,
					color: a.color ? a.color : Dt.value[o] ? Dt.value[o] : r[o],
					regAngle: i,
					totalWeight: ee
				}), n += e, i += t;
			});
		}
		function Rt(e, t) {
			return {
				x: (e.x + t.x) / 2,
				y: (e.y + t.y) / 2
			};
		}
		function zt() {
			I.value = [], F.value.forEach((e) => {
				F.value.filter((t) => t.relations.includes(e.id)).forEach((t, n) => {
					let r = t.relations.indexOf(e.id);
					I.value.push({
						weight: t.weights[r] ? t.weights[r] : 0,
						relationId: `${e.id}_${t.id}`,
						x1: e.x,
						y1: e.y,
						x2: t.x,
						y2: t.y,
						colorSource: e.color,
						colorTarget: t.color,
						midPointLine: Rt({
							x: e.x,
							y: e.y
						}, {
							x: t.x,
							y: t.y
						}),
						midPointBezier: Bt({
							x1: e.x,
							x2: t.x,
							y1: e.y,
							y2: t.y
						}),
						...e
					});
				});
			});
		}
		function Bt(e) {
			let t = {
				x: e.x1,
				y: e.y1
			}, n = {
				x: e.x2,
				y: e.y2
			}, r = {
				x: e.x1,
				y: e.y1
			}, i = {
				x: W.value.width / 2,
				y: W.value.height / 2 + j.value.style.circle.offsetY
			}, a = .5;
			return {
				x: .5 ** 3 * t.x + 3 * .5 ** 2 * a * r.x + 1.5 * a ** 2 * i.x + a ** 3 * n.x,
				y: .5 ** 3 * t.y + 3 * .5 ** 2 * a * r.y + 1.5 * a ** 2 * i.y + a ** 3 * n.y
			};
		}
		let Vt = l(() => Math.max(...I.value.map((e) => e.weight)));
		function Ht(e) {
			return Object.hasOwn(L.value, "x") ? R.value.includes(e.id) ? "opacity:1" : "opacity:0.1" : "opacity:1";
		}
		function J(e) {
			return e.colorSource;
		}
		function Ut(e) {
			return Object.hasOwn(L.value, "x") ? R.value.includes(e.id) && e.relationId === `${e.id}_${L.value.id}` || e.relationId === `${L.value.id}_${e.id}` ? `opacity:1;stroke-width:${Z(e)}` : "opacity: 0" : "opacity: 1";
		}
		function Y(e) {
			return Object.hasOwn(L.value, "x") ? !!(R.value.includes(e.id) && e.relationId === `${e.id}_${L.value.id}` || e.relationId === `${L.value.id}_${e.id}`) : !1;
		}
		function Wt(e) {
			return e.regAngle > 90 && e.regAngle < 270 ? "end" : "start";
		}
		function Gt(e) {
			return e.regAngle > 90 && e.regAngle < 270 ? e.x - 5 : e.x + 5;
		}
		function Kt(e) {
			return Object.hasOwn(L.value, "x") ? L.value.id === e.id || R.value.includes(e.id) ? "opacity:1" : "opacity:0.2" : "opacity:1";
		}
		function qt(e) {
			return e.regAngle > 90 && e.regAngle < 270 ? `rotate(${e.regAngle + 180},${e.x},${e.y})` : `rotate(${e.regAngle},${e.x},${e.y})`;
		}
		let X = y(null);
		function Jt(e, t) {
			X.value = t, k.value = t, j.value.events.datapointEnter && j.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Yt(e, t) {
			X.value = null, A.value || (k.value = null), j.value.events.datapointLeave && j.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: t
			});
		}
		function Xt(e, t) {
			e && (j.value.events.datapointClick && j.value.events.datapointClick({
				datapoint: e,
				seriesIndex: t
			}), Ot.value = 360 - e.regAngle, L.value.id && e.id === L.value.id ? (L.value = {}, R.value = []) : (L.value = e, R.value = [...e.relations]));
		}
		function Zt(e, t) {
			k.value = t, Xt(e, t);
		}
		function Z(e) {
			let t = e.weight / Vt.value * j.value.style.links.maxWidth;
			return Math.max(.3, t);
		}
		let Q = y(!1);
		function Qt(e) {
			Q.value = e, ut.value += 1;
		}
		let $ = y(!1);
		function $t() {
			$.value = !$.value;
		}
		async function en({ scale: e = 2 } = {}) {
			if (!O.value) return;
			let { width: t, height: n } = O.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await he({
				domElement: O.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: j.value.style.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let tn = l(() => j.value.style.backgroundColor), nn = l(() => j.value.style.title), { isCallbackImaging: rn, isCallbackSvg: an, generateSvg: on, onGenerateImage: sn } = me({
			svg: N,
			title: nn,
			legend: null,
			legendItems: null,
			backgroundColor: tn,
			getSvgCallback: () => j.value.userOptions.callbacks.svg,
			generateImage: Tt
		});
		async function cn() {
			if (ct("copyAlt", {
				config: j.value,
				dataset: z.value
			}), !j.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(j.value.userOptions.callbacks.altCopy({
				config: j.value,
				dataset: z.value
			}));
		}
		function ln() {
			A.value = !0, F.value.length && k.value === null && (k.value = 0);
		}
		function un() {
			A.value = !1, k.value = null;
		}
		function dn(e) {
			if (!N.value || $.value || document.activeElement !== N.value || !F.value.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				k.value = null, L.value = {}, R.value = [];
				return;
			}
			if (r) {
				if (k.value === null) return;
				let e = F.value[k.value];
				if (!e) return;
				Xt(e, k.value);
				return;
			}
			let a = k.value;
			a === null || a < 0 || a >= F.value.length ? a = n ? 0 : F.value.length - 1 : n ? (a += 1, a >= F.value.length && (a = 0)) : t && (--a, a < 0 && (a = F.value.length - 1)), k.value = a, X.value = a;
			let o = F.value[a];
			o && j.value.events.datapointEnter && j.value.events.datapointEnter({
				datapoint: o,
				seriesIndex: a
			});
		}
		let fn = l(() => ({
			headers: [
				"ID",
				"Label",
				"Total weight",
				"Relations count",
				"Relations"
			],
			rows: z.value.map((e) => {
				let t = e.relations.map((e) => z.value.find((t) => t.id === e)?.label || e).join(", "), n = (e.weights || []).reduce((e, t) => e + Number(t || 0), 0);
				return [
					e.id,
					e.label,
					n,
					e.relations.length,
					t
				];
			})
		}));
		return _e({
			getImage: en,
			generatePdf: wt,
			generateSvg: on,
			generateImage: Tt,
			toggleAnnotator: $t,
			toggleFullscreen: Qt,
			copyAlt: cn
		}), (e, t) => (v(), f("div", {
			ref_key: "relationCircleChart",
			ref: O,
			class: "vue-data-ui-component vue-ui-relation-circle",
			style: _(`width:100%;background:${j.value.style.backgroundColor};text-align:center;${j.value.responsive ? "height: 100%" : ""}`),
			id: `relation_circle_${D.value}`,
			onMouseenter: t[0] ||= () => C(bt)(!0),
			onMouseleave: t[1] ||= () => C(bt)(!1)
		}, [
			p("div", {
				id: `chart-instructions-${D.value}`,
				class: "sr-only"
			}, [p("p", null, S(j.value.a11y.translations.keyboardNavigation), 1)], 8, Re),
			fn.value?.rows?.length ? (v(), u(be, {
				key: 0,
				uid: D.value,
				head: fn.value.headers,
				body: fn.value.rows,
				notice: j.value.a11y.translations.tableAvailable,
				caption: j.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : d("", !0),
			j.value.userOptions.buttons.annotator ? (v(), u(C(T), {
				key: 1,
				svgRef: C(N),
				backgroundColor: j.value.style.backgroundColor,
				color: j.value.style.color,
				active: $.value,
				isCursorPointer: M.value,
				onClose: $t
			}, {
				"annotator-action-close": w(() => [x(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": w(({ color: t }) => [x(e.$slots, "annotator-action-color", g(h({ color: t })), void 0, !0)]),
				"annotator-action-draw": w(({ mode: t }) => [x(e.$slots, "annotator-action-draw", g(h({ mode: t })), void 0, !0)]),
				"annotator-action-undo": w(({ disabled: t }) => [x(e.$slots, "annotator-action-undo", g(h({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": w(({ disabled: t }) => [x(e.$slots, "annotator-action-redo", g(h({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": w(({ disabled: t }) => [x(e.$slots, "annotator-action-delete", g(h({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : d("", !0),
			Et.value ? (v(), f("div", {
				key: 2,
				ref_key: "noTitle",
				ref: pt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : d("", !0),
			j.value.style.title.text ? (v(), f("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: dt,
				style: "width:100%;background:transparent"
			}, [(v(), u(ge, {
				key: `title_${mt.value}`,
				config: {
					title: {
						cy: "relation-div-title",
						...j.value.style.title
					},
					subtitle: {
						cy: "relation-div-subtitle",
						...j.value.style.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : d("", !0),
			j.value.userOptions.show && lt.value && (C(xt) || C(yt)) ? (v(), u(C(rt), {
				ref: "details",
				key: `user_options_${ut.value}`,
				backgroundColor: j.value.style.backgroundColor,
				color: j.value.style.color,
				isPrinting: C(Ct),
				isImaging: C(P),
				uid: D.value,
				hasPdf: j.value.userOptions.buttons.pdf,
				hasImg: j.value.userOptions.buttons.img,
				hasSvg: j.value.userOptions.buttons.svg,
				hasFullscreen: j.value.userOptions.buttons.fullscreen,
				hasAltCopy: j.value.userOptions.buttons.altCopy,
				hasXls: !1,
				isFullscreen: Q.value,
				titles: { ...j.value.userOptions.buttonTitles },
				chartElement: O.value,
				position: j.value.userOptions.position,
				hasAnnotator: j.value.userOptions.buttons.annotator,
				isAnnotation: $.value,
				callbacks: j.value.userOptions.callbacks,
				printScale: j.value.userOptions.print.scale,
				isCursorPointer: M.value,
				onToggleFullscreen: Qt,
				onGeneratePdf: C(wt),
				onGenerateImage: C(sn),
				onGenerateSvg: C(on),
				onToggleAnnotator: $t,
				onCopyAlt: cn,
				style: _({ visibility: C(xt) ? C(yt) ? "visible" : "hidden" : "visible" })
			}, Te({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: w(({ isOpen: t, color: n }) => [x(e.$slots, "menuIcon", g(h({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: w(() => [x(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: w(() => [x(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: w(() => [x(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: w(({ toggleFullscreen: t, isFullscreen: n }) => [x(e.$slots, "optionFullscreen", g(h({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: w(({ toggleAnnotator: t, isAnnotator: n }) => [x(e.$slots, "optionAnnotator", g(h({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: w(({ altCopy: t }) => [x(e.$slots, "optionAltCopy", g(h({ altCopy: t })), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: w(() => [x(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: w(() => [x(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "8"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"hasPdf",
				"hasImg",
				"hasSvg",
				"hasFullscreen",
				"hasAltCopy",
				"isFullscreen",
				"titles",
				"chartElement",
				"position",
				"hasAnnotator",
				"isAnnotation",
				"callbacks",
				"printScale",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : d("", !0),
			p("div", ze, [(v(), f("svg", {
				ref_key: "svgRef",
				ref: N,
				xmlns: C(re),
				class: ke([{
					"vue-data-ui-fullscreen--on": Q.value,
					"vue-data-ui-fulscreen--off": !Q.value
				}, "relation-circle"]),
				viewBox: `0 0 ${W.value.width <= 0 ? 10 : W.value.width} ${W.value.height <= 0 ? 10 : W.value.height}`,
				width: "100%",
				style: "user-select:none; background:transparent",
				"aria-describedby": `chart-instructions-${D.value}`,
				tabindex: "0",
				onFocus: ln,
				onBlur: un,
				onKeydown: dn
			}, [
				De(C(it)),
				e.$slots["chart-background"] ? (v(), f("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: W.value.width <= 0 ? 10 : W.value.width,
					height: W.value.height <= 0 ? 10 : W.value.height,
					style: { pointerEvents: "none" }
				}, [x(e.$slots, "chart-background", {}, void 0, !0)], 8, Ve)) : d("", !0),
				p("circle", {
					cx: (W.value.width <= 0 ? 1e-4 : W.value.width) / 2,
					cy: (W.value.height <= 0 ? 1e-4 : W.value.height) / 2 + j.value.style.circle.offsetY,
					r: G.value <= 0 ? 1e-4 : G.value,
					stroke: j.value.style.circle.stroke,
					"stroke-width": j.value.style.circle.strokeWidth,
					fill: "transparent",
					class: "main-circle"
				}, null, 8, He),
				kt.value ? (v(), f("g", Ue, [(v(!0), f(c, null, b(I.value, (e, t) => (v(), f("path", {
					key: `relation_${t}`,
					style: _(Ut(e)),
					stroke: J(e),
					class: ke(["relation", { "vue-ui-relation-circle-selected": L.value.hasOwnProperty("id") && R.value.includes(e.id) }]),
					d: `M${e.x1},${e.y1} C${e.x1},${e.y1} ${W.value.width / 2},${W.value.height / 2 + j.value.style.circle.offsetY} ${e.x2},${e.y2}`,
					fill: "none",
					"stroke-width": Z(e),
					"stroke-linecap": "round"
				}, null, 14, We))), 128)), (v(!0), f(c, null, b(I.value, (t, n) => (v(), f("g", Ge, [
					Y(t) ? x(e.$slots, "dataLabel", Oe({ ref_for: !0 }, {
						x: t.midPointBezier.x,
						y: t.midPointBezier.y,
						color: J(t),
						weight: t.weight,
						fontSize: V.value
					}), void 0, !0, 0) : d("", !0),
					Y(t) && !e.$slots.dataLabel ? (v(), f("circle", {
						key: 1,
						cx: t.midPointBezier.x,
						cy: t.midPointBezier.y,
						fill: J(t),
						r: V.value,
						stroke: j.value.style.backgroundColor,
						"stroke-width": "1"
					}, null, 8, Ke)) : d("", !0),
					Y(t) && !e.$slots.dataLabel ? (v(), f("text", {
						key: 2,
						x: t.midPointBezier.x,
						y: t.midPointBezier.y + V.value / 3,
						fill: C(ne)(J(t)),
						"text-anchor": "middle",
						"font-size": V.value
					}, S(C(a)(j.value.style.weightLabels.formatter, t.weight, C(i)({
						p: j.value.style.weightLabels.prefix,
						v: t.weight,
						s: j.value.style.weightLabels.suffix,
						r: j.value.style.weightLabels.rounding
					}), { ...t })), 9, qe)) : d("", !0)
				]))), 256))])) : (v(), f("g", Je, [(v(!0), f(c, null, b(I.value, (e, t) => (v(), f("line", {
					key: `relation_${t}`,
					stroke: J(e),
					"stroke-width": Z(e),
					style: _(Ut(e)),
					x1: e.x1,
					x2: e.x2,
					y1: e.y1,
					y2: e.y2,
					class: ke({ "vue-ui-relation-circle-selected": L.value.hasOwnProperty("id") && R.value.includes(e.id) }),
					"stroke-linecap": "round"
				}, null, 14, Ye))), 128)), (v(!0), f(c, null, b(I.value, (t, n) => (v(), f("g", Xe, [
					Y(t) ? x(e.$slots, "dataLabel", Oe({ ref_for: !0 }, {
						x: t.midPointLine.x,
						y: t.midPointLine.y,
						color: J(t),
						weight: t.weight,
						fontSize: V.value
					}), void 0, !0, 0) : d("", !0),
					Y(t) && !e.$slots.dataLabel && j.value.style.weightLabels.show ? (v(), f("circle", {
						key: 1,
						cx: t.midPointLine.x,
						cy: t.midPointLine.y,
						fill: J(t),
						r: V.value,
						stroke: j.value.style.backgroundColor,
						"stroke-width": "1"
					}, null, 8, Ze)) : d("", !0),
					Y(t) && !e.$slots.dataLabel && j.value.style.weightLabels.show ? (v(), f("text", {
						key: 2,
						x: t.midPointLine.x,
						y: t.midPointLine.y + V.value / 3,
						fill: C(ne)(J(t)),
						"text-anchor": "middle",
						"font-size": V.value
					}, S(C(a)(j.value.style.weightLabels.formatter, t.weight, C(i)({
						p: j.value.style.weightLabels.prefix,
						v: t.weight,
						s: j.value.style.weightLabels.suffix,
						r: j.value.style.weightLabels.rounding
					}), { ...t })), 9, Qe)) : d("", !0)
				]))), 256))])),
				(v(!0), f(c, null, b(F.value, (e, t) => (v(), f("text", {
					key: `plot_text_${t}`,
					"text-anchor": Wt(e),
					transform: qt(e),
					x: Gt(e),
					y: e.y + 5,
					class: "vue-ui-relation-circle-legend",
					"transform-origin": "start",
					"font-weight": L.value.id === e.id ? "900" : "400",
					style: _(`font-family:${j.value.style.fontFamily};${Kt(e)};cursor:${M.value ? "pointer" : "default"}`),
					"font-size": U.value,
					fill: j.value.style.labels.color,
					"text-decoration": t === X.value || t === k.value ? "underline" : void 0,
					onClick: (n) => Zt(e, t),
					onMouseenter: (n) => Jt(e, t),
					onMouseleave: (n) => Yt(e, t)
				}, [C(gt) ? (v(), f(c, { key: 0 }, [Ee("--------")], 64)) : (v(), f(c, { key: 1 }, [Ee(S(e.label) + " (" + S(C(a)(j.value.style.weightLabels.formatter, e.totalWeight, C(i)({
					p: j.value.style.weightLabels.prefix,
					v: e.totalWeight,
					s: j.value.style.weightLabels.suffix,
					r: j.value.style.weightLabels.rounding
				}), { ...e })) + ") ", 1)], 64))], 44, $e))), 128)),
				(v(!0), f(c, null, b(F.value, (e, t) => (v(), f("circle", {
					cx: e.x,
					cy: e.y,
					key: `plot_${t}`,
					style: _(`${Ht(e)}; transition: r 0.2s ease-in-out; cursor:${M.value ? "pointer" : "default"}`),
					class: "vue-ui-relation-circle-plot",
					fill: j.value.style.plot.useSerieColor ? e.color : j.value.style.plot.color,
					stroke: j.value.style.backgroundColor,
					"stroke-width": "1",
					r: H.value * (t === X.value || t === k.value ? 2 : 1),
					onClick: (n) => Zt(e, t),
					onMouseenter: (n) => Jt(e, t),
					onMouseleave: (n) => Yt(e, t)
				}, null, 44, et))), 128)),
				x(e.$slots, "svg", { svg: {
					...W.value,
					isPrintingImg: C(Ct) || C(P) || C(rn),
					isPrintingSvg: C(an)
				} }, void 0, !0)
			], 42, Be)), e.$slots.hint ? (v(), f("div", tt, [x(e.$slots, "hint", g(h({
				hint: j.value.a11y.translations.keyboardNavigation,
				isVisible: A.value
			})), void 0, !0)])) : d("", !0)]),
			e.$slots.watermark ? (v(), f("div", nt, [x(e.$slots, "watermark", g(h({ isPrinting: C(Ct) || C(P) || C(rn) || C(an) })), void 0, !0)])) : d("", !0),
			e.$slots.source ? (v(), f("div", {
				key: 6,
				ref_key: "source",
				ref: ft,
				dir: "auto"
			}, [x(e.$slots, "source", {}, void 0, !0)], 512)) : d("", !0),
			x(e.$slots, "skeleton", {}, () => [C(gt) ? (v(), u(de, { key: 0 })) : d("", !0)], !0)
		], 44, Le));
	}
}, [["__scopeId", "data-v-09b3c31c"]]);
//#endregion
export { Ie as n, T as t };
