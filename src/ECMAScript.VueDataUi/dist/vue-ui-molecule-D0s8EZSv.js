import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Jt as r, Kt as i, M as a, Ot as o, Pt as ee, S as te, Z as s, ct as c, jt as ne, q as re, t as l, tt as u, w as ie, xt as ae } from "./lib-Bttd6u5E.js";
import { n as oe, t as se } from "./useHints-Dq_w2E8B.js";
import { t as ce } from "./useConfig-DlNpz6P8.js";
import { t as le } from "./usePrinter-DN5bYhTG.js";
import { n as ue, t as de } from "./BaseScanner-DZvpgOjM.js";
import { t as d } from "./useNestedProp-vPNvh7rV.js";
import { t as fe } from "./useThemeCheck-C43Tcqmk.js";
import { t as pe } from "./useChartExport-DNiwdPmb.js";
import { t as me } from "./img-Bnokohej.js";
import { t as he } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as ge } from "./DefGrad-DVBqDjhO.js";
import { t as _e } from "./useUserOptionState-DK-_1ddE.js";
import { t as ve } from "./useChartAccessibility-DYqac8yF.js";
import { t as ye } from "./vue_ui_molecule-CO9L59SF.js";
import { t as be } from "./usePanZoom-CYU3B4T3.js";
import { Fragment as xe, computed as f, createBlock as p, createCommentVNode as m, createElementBlock as h, createElementVNode as g, createSlots as Se, createTextVNode as Ce, createVNode as _, defineAsyncComponent as v, guardReactiveProps as y, mergeProps as we, nextTick as b, normalizeClass as Te, normalizeProps as x, normalizeStyle as S, onMounted as Ee, openBlock as C, ref as w, renderList as De, renderSlot as T, resolveDynamicComponent as Oe, toDisplayString as ke, toRefs as Ae, unref as E, useCssVars as je, watch as D, withCtx as O } from "vue";
//#region src/components/vue-ui-molecule.vue
var Me = /* @__PURE__ */ e({ default: () => k }), Ne = ["id"], Pe = {
	key: 1,
	ref: "noTitle",
	class: "vue-data-ui-no-title-space",
	style: "height:36px; width: 100%;background:transparent"
}, Fe = {
	key: 2,
	style: "width:100%;background:transparent;"
}, Ie = ["xmlns", "viewBox"], Le = ["width", "height"], Re = {
	key: 4,
	class: "vue-data-ui-watermark"
}, ze = {
	key: 5,
	"data-dom-to-png-ignore": "",
	class: "reset-wrapper"
}, Be = {
	key: 6,
	ref: "source",
	dir: "auto"
}, Ve = ["innerHTML"], k = /*#__PURE__*/ he({
	__name: "vue-ui-molecule",
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
	emits: ["selectNode", "copyAlt"],
	setup(e, { expose: he, emit: Me }) {
		je((e) => ({ v2c5beca2: $t.value }));
		let k = v(() => import("./Title-BE3qg9xl.js").then((e) => e.t)), He = v(() => import("./Tooltip-DhjyfHwz.js")), Ue = v(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), We = v(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Ge = v(() => import("./DataTable-BbKgJ5UI.js")), Ke = v(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), qe = v(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Je = v(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), Ye = v(() => import("./RecursiveLinks-BygcpDiT.js")), Xe = v(() => import("./RecursiveLabels-D9rfID6q.js")), Ze = v(() => import("./RecursiveCircles-D6aDPmoe.js")), Qe = v(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_molecule: $e } = ce(), { isThemeValid: et, warnInvalidTheme: tt } = fe(), A = e, nt = Me, rt = f(() => !!A.dataset && A.dataset.length);
		Ee(() => {
			at();
		});
		let it = f(() => P.value.debug);
		function at() {
			ne(A.dataset) && u({
				componentName: "VueUiMolecule",
				type: "dataset",
				debug: it.value
			});
		}
		let ot = w(re()), st = w(!1), j = w(""), M = w(null), ct = w(0), lt = w(0), ut = w(0), N = w(null), dt = w(null), P = w(B());
		oe({
			config: () => P.value,
			dataset: () => A.dataset,
			component: "VueUiMolecule",
			rules: [
				se.emptyArray,
				{
					test: (e) => e[0] && e[0].nodes && e[0].nodes.length === 0,
					message: [
						"👀 There are no children nodes attached to the root node. Consider:",
						"",
						"▶️ Adding children nodes to the root node."
					]
				},
				{
					test: (e) => e[0] && e[0].nodes && e[0].nodes.length > 8,
					message: [
						"👀 The number of children nodes > 8, some nodes might overlap. Consider:",
						"",
						"▶️ Grouping data into broader categories."
					]
				}
			]
		});
		let F = f(() => P.value.userOptions.useCursorPointer), ft = f(() => r({
			defaultConfig: {
				userOptions: { show: !1 },
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					nodes: { stroke: "#6A6A6A" },
					links: { stroke: "#6A6A6A80" }
				} }
			},
			userConfig: P.value.skeletonConfig ?? {}
		})), { loading: I, FINAL_DATASET: L } = ue({
			...Ae(A),
			FINAL_CONFIG: P,
			prepareConfig: B,
			skeletonDataset: A.config?.skeletonDataset ?? [{
				name: "_",
				color: "#CACACA",
				nodes: [
					{
						name: "_",
						color: "#CACACA",
						nodes: [
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							}
						]
					},
					{
						name: "_",
						color: "#CACACA",
						nodes: [
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							}
						]
					},
					{
						name: "_",
						color: "#CACACA",
						nodes: [
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							}
						]
					},
					{
						name: "_",
						color: "#CACACA",
						nodes: [
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							}
						]
					},
					{
						name: "_",
						color: "#CACACA",
						nodes: [
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							}
						]
					},
					{
						name: "_",
						color: "#CACACA",
						nodes: [
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							},
							{
								name: "_",
								color: "#CACACA"
							}
						]
					}
				]
			}],
			skeletonConfig: r({
				defaultConfig: P.value,
				userConfig: ft.value
			})
		}), { userOptionsVisible: R, setUserOptionsVisibility: pt, keepUserOptionState: mt } = _e({ config: P.value }), { svgRef: z } = ve({ config: P.value.style.chart.title });
		function B() {
			let e = d({
				userConfig: A.config,
				defaultConfig: $e
			}), t = e.theme;
			if (!t) return e;
			if (!et.value(e)) return tt(e), e;
			let n = d({
				userConfig: ye[t] || A.config,
				defaultConfig: e
			}), r = d({
				userConfig: A.config,
				defaultConfig: n
			});
			return {
				...r,
				customPalette: r.customPalette.length ? r.customPalette : i[t] || ee
			};
		}
		D(() => A.config, (e) => {
			P.value = B(), R.value = !P.value.userOptions.showOnChartHover, at(), lt.value += 1, ut.value += 1, U.value.showTable = P.value.table.show, U.value.showTooltip = P.value.style.chart.tooltip.show, U.value.showZoom = P.value.style.chart.zoom.show;
		}, { deep: !0 });
		let ht = w(0), gt = () => {
			ht.value += 1;
		};
		D([() => I.value, () => L.value], async ([e]) => {
			e || (await b(), W.value = St(), await b(), gt(), await b(), Ht({
				x: 0,
				y: 0,
				width: 400,
				height: 400
			}), Bt(!1));
		}, {
			flush: "post",
			deep: !1
		});
		let { isPrinting: V, isImaging: H, generatePdf: _t, generateImage: vt } = le({
			elementId: `cluster_${ot.value}`,
			fileName: P.value.style.chart.title.text || "vue-ui-molecule",
			options: P.value.userOptions.print
		}), yt = f(() => P.value.userOptions.show && !P.value.style.chart.title.text), bt = f(() => ie(P.value.customPalette)), U = w({
			showTable: P.value.table.show,
			showDataLabels: !0,
			showTooltip: P.value.style.chart.tooltip.show,
			showZoom: P.value.style.chart.zoom.show
		});
		D(P, () => {
			U.value = {
				showTable: P.value.table.show,
				showDataLabels: !0,
				showTooltip: P.value.style.chart.tooltip.show,
				showZoom: P.value.style.chart.zoom.show
			};
		}, { immediate: !0 });
		function xt(e, t = 0) {
			return Array.isArray(e) && e.length > 0 && e[0].nodes ? xt(e[0].nodes, t + 1) : t;
		}
		function St() {
			let e = xt(L.value), t = 100, n = t;
			for (let r = 0; r < e; r += 1) t /= 1, n += t;
			return {
				height: n,
				width: n
			};
		}
		let W = w(St());
		function Ct(e, t = {
			x: -W.value.width / 2.43,
			y: W.value.height / 2
		}, n = W.value.width / 1.1, r = 24, i = 0, o = 0, s = "#BBBBBB", c = 0) {
			if (!Array.isArray(e) || e.length === 0) return e;
			let ne = a({
				plot: t,
				radius: n,
				sides: e.length,
				rotation: i
			});
			return e.forEach((e, t) => {
				let a = ne.coordinates[t], l = e.color ? (() => {
					let t = te(e.color);
					return t.startsWith("#") ? t : `#${t}`;
				})() : null, u;
				l ? u = l : c === 0 ? u = s : c === 1 ? (u = bt.value[o] || ee[o] || s, o += 1) : u = s, e.polygonPath = { coordinates: [a] }, e.circleRadius = r, e.color = u, e.strokeWidth = Math.min(P.value.style.chart.links.strokeWidth / (c + 1), r / 2), e.uid = re(), Array.isArray(e.nodes) && e.nodes.length && (e.nodes = Ct(e.nodes, a, n / 2.9, r / 2.2, i + Math.PI * t / e.nodes.length, o, u, c + 1));
			}), e;
		}
		function wt(e) {
			let t = /* @__PURE__ */ new Set();
			function n(e) {
				e.forEach((e) => {
					if (!e.color) return;
					let r = e.color;
					/^#?[0-9A-F]{6}$/i.test(r) || (r = te(r)), r.startsWith("#") || (r = `#${r}`), t.add(r), Array.isArray(e.nodes) && e.nodes.length && n(e.nodes);
				});
			}
			n(e);
			let r = {};
			return Array.from(t).forEach((e) => {
				let t = e.slice(1);
				r[e] = `gradient_${t}`;
			}), r;
		}
		let Tt = f(() => wt(G.value)), G = f(() => Ct(s(L.value))), K = w(null);
		function Et(e) {
			K.value = {
				datapoint: e,
				seriesIndex: -1,
				series: G.value,
				config: P.value
			};
			let t = P.value.style.chart.tooltip.customFormat;
			if (ae(t) && c(() => t({
				seriesIndex: -1,
				datapoint: e,
				series: G.value,
				config: P.value
			}))) j.value = t({
				seriesIndex: -1,
				datapoint: e,
				series: G.value,
				config: P.value
			});
			else {
				let t = "";
				t += `<div style="display:flex;align-items:center;gap:3px"><div style="color:${e.color}">⬤</div><div>${e.name}</div></div>`, e.details && (t += `<div style="width:100%;border-top:1px solid ${P.value.style.chart.tooltip.borderColor};margin-top: 2px">${e.details}</div>`), j.value = `<div style="font-family:inherit">${t}</div>`;
			}
		}
		let q = w(null), J = w(null), Dt = w(null);
		function Ot(e) {
			Dt.value = q.value, q.value = e, e ? P.value.events.datapointEnter && P.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: -1
			}) : P.value.events.datapointLeave && P.value.events.datapointLeave({
				datapoint: Dt.value || q.value,
				seriesIndex: -1
			}), e ? (st.value = !0, Et(e), J.value = e.uid) : (st.value = !1, j.value = "", J.value = null);
		}
		function kt(e) {
			let t = [];
			function n(e) {
				e.forEach((e) => {
					let r = {
						name: e.name,
						details: e.details || "-",
						ancestor: e.ancestor && e.ancestor.name || "-",
						color: e.color || ""
					};
					t.push(r), e.nodes && e.nodes.length > 0 && n(e.nodes, e.name);
				});
			}
			return n(e), t;
		}
		let At = f(() => kt(G.value)), Y = f(() => ({
			head: [
				P.value.table.translations.nodeName,
				P.value.table.translations.details,
				P.value.table.translations.ancestor
			],
			body: At.value.map((e, t) => [
				{
					color: e.color,
					name: e.name
				},
				e.details,
				e.ancestor || ""
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
				P.value.table.translations.nodeName,
				P.value.table.translations.details,
				P.value.table.translations.ancestor
			]
		}));
		function jt(e = null) {
			b(() => {
				let r = Y.value.body.map((e, t) => [
					[e[0].name],
					[e[1]],
					[e[2]]
				]), i = [
					[P.value.style.chart.title.text],
					[P.value.style.chart.title.subtitle.text],
					[[...Y.value.head]]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: P.value.style.chart.title.text || "vue-ui-molecule"
				});
			});
		}
		function Mt() {
			return G.value;
		}
		let X = w(!1);
		function Nt(e) {
			X.value = e, ct.value += 1;
		}
		function Pt() {
			U.value.showTable = !U.value.showTable;
		}
		function Ft() {
			U.value.showDataLabels = !U.value.showDataLabels;
		}
		function It() {
			U.value.showTooltip = !U.value.showTooltip;
		}
		function Lt() {
			U.value.showZoom = !U.value.showZoom;
		}
		let Z = w(!1);
		function Rt() {
			Z.value = !Z.value;
		}
		let zt = f(() => !Z.value && U.value.showZoom), { viewBox: Q, resetZoom: Bt, isZoom: Vt, setInitialViewBox: Ht } = be(z, {
			x: 0,
			y: 0,
			width: Math.max(10, W.value.width),
			height: Math.max(10, W.value.height)
		}, P.value.style.chart.zoom.speed, zt);
		function Ut(e) {
			P.value.events.datapointClick && P.value.events.datapointClick({
				datapoint: e,
				seriesIndex: -1
			}), nt("selectNode", e);
		}
		async function Wt({ scale: e = 2 } = {}) {
			if (!M.value) return;
			let { width: t, height: n } = M.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await me({
				domElement: M.value,
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
		let $ = f(() => {
			let e = P.value.table.useDialog && !P.value.table.show, t = U.value.showTable;
			return {
				component: e ? Qe : We,
				title: `${P.value.style.chart.title.text}${P.value.style.chart.title.subtitle.text ? `: ${P.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: P.value.table.th.backgroundColor,
					color: P.value.table.th.color,
					headerColor: P.value.table.th.color,
					headerBg: P.value.table.th.backgroundColor,
					isFullscreen: X.value,
					fullscreenParent: M.value,
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
		D(() => U.value.showTable, (e) => {
			P.value.table.show || (e && P.value.table.useDialog && N.value ? N.value.open() : "close" in N.value && N.value.close());
		});
		function Gt() {
			U.value.showTable = !1, dt.value && dt.value.setTableIconState(!1);
		}
		let Kt = f(() => P.value.style.chart.backgroundColor), qt = f(() => P.value.style.chart.title), { isCallbackImaging: Jt, isCallbackSvg: Yt, generateSvg: Xt, onGenerateImage: Zt } = pe({
			svg: z,
			title: qt,
			legend: null,
			legendItems: null,
			backgroundColor: Kt,
			getSvgCallback: () => P.value.userOptions.callbacks.svg,
			generateImage: vt
		});
		async function Qt() {
			if (nt("copyAlt", {
				config: P.value,
				dataset: G.value
			}), !P.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(P.value.userOptions.callbacks.altCopy({
				config: P.value,
				dataset: G.value
			}));
		}
		let $t = f(() => P.value.style.chart.color);
		return he({
			getData: Mt,
			getImage: Wt,
			generatePdf: _t,
			generateCsv: jt,
			generateImage: vt,
			generateSvg: Xt,
			toggleTable: Pt,
			toggleLabels: Ft,
			toggleTooltip: It,
			toggleAnnotator: Rt,
			toggleFullscreen: Nt,
			toggleZoom: Lt,
			copyAlt: Qt
		}), (e, t) => (C(), h("div", {
			ref_key: "moleculeChart",
			ref: M,
			class: Te(`vue-data-ui-component vue-ui-molecule ${X.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			style: S(`font-family:${P.value.style.fontFamily};width:100%; text-align:center;background:${P.value.style.chart.backgroundColor}`),
			id: `cluster_${ot.value}`,
			onMouseleave: t[2] ||= (e) => {
				q.value = null, J.value = null, E(pt)(!1);
			},
			onMouseenter: t[3] ||= () => E(pt)(!0)
		}, [
			P.value.userOptions.buttons.annotator && E(z) ? (C(), p(E(Ke), {
				key: 0,
				svgRef: E(z),
				backgroundColor: P.value.style.chart.backgroundColor,
				color: P.value.style.chart.color,
				active: Z.value,
				isCursorPointer: F.value,
				onClose: Rt
			}, {
				"annotator-action-close": O(() => [T(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": O(({ color: t }) => [T(e.$slots, "annotator-action-color", x(y({ color: t })), void 0, !0)]),
				"annotator-action-draw": O(({ mode: t }) => [T(e.$slots, "annotator-action-draw", x(y({ mode: t })), void 0, !0)]),
				"annotator-action-undo": O(({ disabled: t }) => [T(e.$slots, "annotator-action-undo", x(y({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": O(({ disabled: t }) => [T(e.$slots, "annotator-action-redo", x(y({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": O(({ disabled: t }) => [T(e.$slots, "annotator-action-delete", x(y({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : m("", !0),
			yt.value ? (C(), h("div", Pe, null, 512)) : m("", !0),
			P.value.style.chart.title.text ? (C(), h("div", Fe, [(C(), p(E(k), {
				key: `title_${lt.value}`,
				config: {
					title: {
						cy: "molecule-div-title",
						...P.value.style.chart.title
					},
					subtitle: {
						cy: "molecule-div-subtitle",
						...P.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))])) : m("", !0),
			P.value.userOptions.show && rt.value && (E(mt) || E(R)) ? (C(), p(E(qe), {
				ref_key: "userOptionsRef",
				ref: dt,
				key: `user_options_${ct.value}`,
				backgroundColor: P.value.style.chart.backgroundColor,
				color: P.value.style.chart.color,
				isPrinting: E(V),
				isImaging: E(H),
				uid: ot.value,
				hasTooltip: P.value.userOptions.buttons.tooltip && P.value.style.chart.tooltip.show,
				hasPdf: P.value.userOptions.buttons.pdf,
				hasXls: P.value.userOptions.buttons.csv,
				hasImg: P.value.userOptions.buttons.img,
				hasSvg: P.value.userOptions.buttons.svg,
				hasTable: P.value.userOptions.buttons.table,
				hasLabel: P.value.userOptions.buttons.labels,
				hasFullscreen: P.value.userOptions.buttons.fullscreen,
				hasAltCopy: P.value.userOptions.buttons.altCopy,
				isTooltip: U.value.showTooltip,
				titles: { ...P.value.userOptions.buttonTitles },
				chartElement: M.value,
				position: P.value.userOptions.position,
				hasAnnotator: P.value.userOptions.buttons.annotator,
				isAnnotation: Z.value,
				callbacks: P.value.userOptions.callbacks,
				printScale: P.value.userOptions.print.scale,
				tableDialog: P.value.table.useDialog,
				hasZoom: P.value.userOptions.buttons.zoom,
				isZoom: U.value.showZoom,
				isCursorPointer: F.value,
				onToggleFullscreen: Nt,
				onGeneratePdf: E(_t),
				onGenerateCsv: jt,
				onGenerateImage: E(Zt),
				onGenerateSvg: E(Xt),
				onToggleTable: Pt,
				onToggleLabels: Ft,
				onToggleTooltip: It,
				onToggleAnnotator: Rt,
				onToggleZoom: Lt,
				onCopyAlt: Qt,
				style: S({ visibility: E(mt) ? E(R) ? "visible" : "hidden" : "visible" })
			}, Se({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: O(({ isOpen: t, color: n }) => [T(e.$slots, "menuIcon", x(y({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: O(() => [T(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: O(() => [T(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: O(() => [T(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: O(() => [T(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: O(() => [T(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: O(() => [T(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionLabels ? {
					name: "optionLabels",
					fn: O(() => [T(e.$slots, "optionLabels", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: O(({ toggleFullscreen: t, isFullscreen: n }) => [T(e.$slots, "optionFullscreen", x(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: O(({ toggleAnnotator: t, isAnnotator: n }) => [T(e.$slots, "optionAnnotator", x(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots.optionZoom ? {
					name: "optionZoom",
					fn: O(({ toggleZoom: t, isZoomLocked: n }) => [T(e.$slots, "optionZoom", x(y({
						toggleZoom: t,
						isZoomLocked: n
					})), void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: O(({ altCopy: t }) => [T(e.$slots, "optionAltCopy", x(y({ altCopy: t })), void 0, !0)]),
					key: "11"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: O(() => [T(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "12"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: O(() => [T(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "13"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasXls.hasImg.hasSvg.hasTable.hasLabel.hasFullscreen.hasAltCopy.isTooltip.titles.chartElement.position.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.hasZoom.isZoom.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : m("", !0),
			(C(), h("svg", {
				ref_key: "svgRef",
				ref: z,
				key: `svg_${ht.value}`,
				xmlns: E(l),
				viewBox: `${E(Q).x} ${E(Q).y} ${E(Q).width} ${E(Q).height}`,
				class: Te({
					"vue-data-ui-fullscreen--on": X.value,
					"vue-data-ui-fulscreen--off": !X.value
				}),
				style: S(`overflow: hidden; background:transparent;color:${P.value.style.chart.color}`)
			}, [
				_(E(Je)),
				e.$slots["chart-background"] ? (C(), h("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: W.value.width <= 0 ? 10 : W.value.width,
					height: W.value.height <= 0 ? 10 : W.value.height,
					style: { pointerEvents: "none" }
				}, [T(e.$slots, "chart-background", {}, void 0, !0)], 8, Le)) : m("", !0),
				g("defs", null, [(C(!0), h(xe, null, De(Object.keys(Tt.value), (e, t) => (C(), p(ge, {
					t: "radial",
					id: `gradient_${e}`,
					key: `gradient_${e}_${t}`,
					cx: "50%",
					cy: "30%",
					r: "50%",
					fx: "50%",
					fy: "50%",
					stops: [[
						"0%",
						E(o)(e, .5),
						1
					], [
						"100%",
						e,
						1
					]]
				}, null, 8, ["id", "stops"]))), 128))]),
				_(E(Ye), {
					dataset: G.value,
					color: P.value.style.chart.links.stroke,
					backgroundColor: P.value.style.chart.backgroundColor,
					useChildColor: P.value.style.chart.links.useChildColor
				}, null, 8, [
					"dataset",
					"color",
					"backgroundColor",
					"useChildColor"
				]),
				_(E(Ze), {
					dataset: G.value,
					hoveredUid: J.value,
					stroke: P.value.style.chart.nodes.stroke,
					strokeHovered: P.value.style.chart.nodes.strokeHovered,
					onClick: Ut,
					onHover: Ot
				}, {
					node: O(({ node: t }) => [T(e.$slots, "node", x(y({ node: t })), void 0, !0)]),
					"node-svg": O(({ nodeSvg: t }) => [T(e.$slots, "node-svg", x(y({ nodeSvg: t })), void 0, !0)]),
					_: 3
				}, 8, [
					"dataset",
					"hoveredUid",
					"stroke",
					"strokeHovered"
				]),
				U.value.showDataLabels && !E(I) ? (C(), p(E(Xe), {
					key: 1,
					dataset: G.value,
					color: P.value.style.chart.color,
					hoveredUid: J.value
				}, null, 8, [
					"dataset",
					"color",
					"hoveredUid"
				])) : m("", !0),
				T(e.$slots, "svg", { svg: {
					...W.value,
					drawingArea: { ...E(Q) },
					isPrintingImg: E(V) || E(H) || E(Jt),
					isPrintingSvg: E(Yt)
				} }, void 0, !0)
			], 14, Ie)),
			e.$slots.watermark ? (C(), h("div", Re, [T(e.$slots, "watermark", x(y({ isPrinting: E(V) || E(H) || E(Jt) || E(Yt) })), void 0, !0)])) : m("", !0),
			E(Vt) ? (C(), h("div", ze, [T(e.$slots, "reset-action", { reset: E(Bt) }, () => [g("button", {
				"data-cy-reset": "",
				tabindex: "0",
				role: "button",
				class: "vue-data-ui-refresh-button",
				style: S({
					background: P.value.style.chart.backgroundColor,
					cursor: F.value ? "pointer" : "default"
				}),
				onClick: t[0] ||= (e) => E(Bt)(!0)
			}, [_(E(Ue), {
				name: "refresh",
				stroke: P.value.style.chart.color
			}, null, 8, ["stroke"])], 4)], !0)])) : m("", !0),
			e.$slots.source ? (C(), h("div", Be, [T(e.$slots, "source", {}, void 0, !0)], 512)) : m("", !0),
			_(E(He), {
				teleportTo: P.value.style.chart.tooltip.teleportTo,
				show: U.value.showTooltip && st.value,
				backgroundColor: P.value.style.chart.tooltip.backgroundColor,
				color: P.value.style.chart.tooltip.color,
				borderRadius: P.value.style.chart.tooltip.borderRadius,
				borderColor: P.value.style.chart.tooltip.borderColor,
				borderWidth: P.value.style.chart.tooltip.borderWidth,
				fontSize: P.value.style.chart.tooltip.fontSize,
				backgroundOpacity: P.value.style.chart.tooltip.backgroundOpacity,
				position: P.value.style.chart.tooltip.position,
				offsetX: P.value.style.chart.tooltip.offsetX,
				offsetY: P.value.style.chart.tooltip.offsetY,
				parent: M.value,
				content: j.value,
				isFullscreen: X.value,
				isCustom: P.value.style.chart.tooltip.customFormat && typeof P.value.style.chart.tooltip.customFormat == "function",
				smooth: P.value.style.chart.tooltip.smooth,
				backdropFilter: P.value.style.chart.tooltip.backdropFilter,
				smoothForce: P.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: P.value.style.chart.tooltip.smoothSnapThreshold
			}, {
				"tooltip-before": O(() => [T(e.$slots, "tooltip-before", x(y({ ...K.value })), void 0, !0)]),
				tooltip: O(() => [T(e.$slots, "tooltip", x(y({ ...K.value })), void 0, !0)]),
				"tooltip-after": O(() => [T(e.$slots, "tooltip-after", x(y({ ...K.value })), void 0, !0)]),
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
				"smoothSnapThreshold"
			]),
			rt.value && P.value.userOptions.buttons.table ? (C(), p(Oe($.value.component), we({ key: 7 }, $.value.props, {
				ref_key: "tableUnit",
				ref: N,
				onClose: Gt
			}), Se({
				content: O(() => [(C(), p(E(Ge), {
					key: `table_${ut.value}`,
					colNames: Y.value.colNames,
					head: Y.value.head,
					body: Y.value.body,
					config: Y.value.config,
					title: P.value.table.useDialog ? "" : $.value.title,
					withCloseButton: !P.value.table.useDialog,
					isCursorPointer: F.value,
					onClose: Gt
				}, {
					th: O(({ th: e }) => [g("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, Ve)]),
					td: O(({ td: e }) => [Ce(ke(e.name || e), 1)]),
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
			}, [P.value.table.useDialog ? {
				name: "title",
				fn: O(() => [Ce(ke($.value.title), 1)]),
				key: "0"
			} : void 0, P.value.table.useDialog ? {
				name: "actions",
				fn: O(() => [g("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => jt(P.value.userOptions.callbacks.csv),
					style: S({ cursor: F.value ? "pointer" : "default" })
				}, [_(E(Ue), {
					name: "fileCsv",
					stroke: $.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : m("", !0),
			T(e.$slots, "skeleton", {}, () => [E(I) ? (C(), p(de, { key: 0 })) : m("", !0)], !0)
		], 46, Ne));
	}
}, [["__scopeId", "data-v-65b09aea"]]);
//#endregion
export { Me as n, k as t };
