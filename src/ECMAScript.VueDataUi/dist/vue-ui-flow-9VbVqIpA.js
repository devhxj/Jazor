import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, Jt as r, Kt as i, Pt as a, S as o, X as s, b as c, i as l, jt as u, n as d, q as ee, r as f, t as p, tt as m, w as h, xt as te } from "./lib-Bttd6u5E.js";
import { n as ne, t as g } from "./useHints-Dq_w2E8B.js";
import { t as _ } from "./useConfig-DlNpz6P8.js";
import { t as v } from "./usePrinter-DN5bYhTG.js";
import { n as y, t as b } from "./BaseScanner-DZvpgOjM.js";
import { t as x } from "./useNestedProp-vPNvh7rV.js";
import { t as re } from "./useThemeCheck-C43Tcqmk.js";
import { t as S } from "./useChartExport-DNiwdPmb.js";
import { t as ie } from "./img-Bnokohej.js";
import { n as ae } from "./Title-BE3qg9xl.js";
import { t as oe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as se, t as ce } from "./useResponsive-ZtArZtUf.js";
import { t as le } from "./A11yDataTable-DdRsVULz.js";
import { t as ue } from "./useUserOptionState-DK-_1ddE.js";
import { t as de } from "./useChartAccessibility-DYqac8yF.js";
import { t as fe } from "./Legend-CQxUgOd-.js";
import { t as pe } from "./vue_ui_flow-BewZjjKG.js";
import { Fragment as me, Teleport as he, computed as C, createBlock as w, createCommentVNode as T, createElementBlock as E, createElementVNode as D, createSlots as ge, createTextVNode as _e, createVNode as ve, defineAsyncComponent as O, guardReactiveProps as k, mergeProps as ye, nextTick as be, normalizeClass as xe, normalizeProps as A, normalizeStyle as j, onMounted as Se, openBlock as M, ref as N, renderList as Ce, renderSlot as P, resolveDynamicComponent as we, toDisplayString as F, toRefs as Te, unref as I, watch as Ee, withCtx as L } from "vue";
//#region src/components/vue-ui-flow.vue
var De = /* @__PURE__ */ e({ default: () => Je }), Oe = ["id"], ke = ["id"], Ae = {
	key: 2,
	ref: "noTitle",
	class: "vue-data-ui-no-title-space",
	style: "height:36px; width: 100%;background:transparent"
}, je = ["id"], Me = { style: { position: "relative" } }, Ne = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Pe = ["width", "height"], Fe = ["id"], Ie = ["stop-color"], Le = ["stop-color"], Re = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], ze = [
	"data-a11y-node-id",
	"x",
	"y",
	"height",
	"width",
	"fill",
	"stroke",
	"stroke-width",
	"rx",
	"aria-label",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Be = { key: 1 }, Ve = [
	"x",
	"y",
	"font-size",
	"fill"
], He = [
	"x",
	"y",
	"font-size",
	"fill"
], Ue = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, We = {
	key: 5,
	class: "vue-data-ui-watermark"
}, Ge = ["id"], Ke = ["onClick"], qe = ["innerHTML"], Je = /*#__PURE__*/ oe({
	__name: "vue-ui-flow",
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
	setup(e, { expose: oe, emit: De }) {
		let Je = O(() => import("./Tooltip-DhjyfHwz.js")), Ye = O(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), Xe = O(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), Ze = O(() => import("./DataTable-BbKgJ5UI.js")), Qe = O(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), $e = O(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), et = O(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), tt = O(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), { vue_ui_flow: nt } = _(), { isThemeValid: rt, warnInvalidTheme: it } = re(), R = e, at = De, z = N(ee()), B = N(null), ot = N(0), st = N(0), ct = N(!1), lt = N(""), ut = N(null), dt = N(null), ft = N(null), pt = N(null), mt = N(null), ht = N(!1), gt = N(null), _t = N(null), vt = N(null), yt = N(null), V = N(null), bt = N({
			x: 0,
			y: 0
		}), xt = N("pointer"), St = N(!1), Ct = C(() => !!R.dataset && R.dataset.length);
		Se(() => {
			ht.value = !0, At();
		});
		let H = N(!1);
		function wt(e) {
			H.value = e, ot.value += 1;
		}
		let U = N(Pt());
		ne({
			config: () => U.value,
			dataset: () => R.dataset,
			component: "VueUiFlow",
			rules: [g.emptyArray, g.noHint]
		});
		let W = C(() => U.value.userOptions.useCursorPointer), Tt = C(() => r({
			defaultConfig: {
				userOptions: { show: !1 },
				nodeCategories: {
					B: "A",
					C: "B"
				},
				nodeCategoryColors: {
					A: "#CACACA",
					B: "#AAAAAA"
				},
				table: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					legend: { backgroundColor: "transparent" },
					nodes: {
						labels: { show: !1 },
						stroke: "#666666"
					},
					links: { stroke: "#666666" }
				} }
			},
			userConfig: U.value.skeletonConfig ?? {}
		})), { loading: Et, FINAL_DATASET: Dt, manualLoading: Ot } = y({
			...Te(R),
			FINAL_CONFIG: U,
			prepareConfig: Pt,
			skeletonDataset: R.config?.skeletonDataset ?? [
				[
					"A",
					"B",
					2,
					"#CACACA"
				],
				[
					"B",
					"C",
					1,
					"#CACACA"
				],
				[
					"C",
					"D",
					.5,
					"#CACACA"
				],
				[
					"E",
					"F",
					1,
					"#AAAAAA"
				],
				[
					"F",
					"G",
					.5,
					"#AAAAAA"
				],
				[
					"G",
					"H",
					.25,
					"#AAAAAA"
				]
			],
			skeletonConfig: r({
				defaultConfig: U.value,
				userConfig: Tt.value
			})
		}), kt = C(() => U.value.debug);
		function At() {
			if (u(R.dataset) && (m({
				componentName: "VueUiFlow",
				type: "dataset",
				debug: kt.value
			}), Ot.value = !0), u(R.dataset) || (Ot.value = U.value.loading), U.value.responsive) {
				let e = se(() => {
					let { width: e, height: t } = ce({
						chart: B.value,
						title: U.value.style.chart.title.text ? dt.value : null,
						legend: U.value.style.chart.legend.show ? ut.value : null,
						source: ft.value
					});
					requestAnimationFrame(() => {
						Ft.value = e, It.value = t;
					});
				});
				pt.value && (mt.value && pt.value.unobserve(mt.value), pt.value.disconnect()), pt.value = new ResizeObserver(e), mt.value = B.value.parentNode, pt.value.observe(mt.value);
			}
		}
		let { userOptionsVisible: jt, setUserOptionsVisibility: Mt, keepUserOptionState: Nt } = ue({ config: U.value }), { svgRef: G } = de({ config: U.value.style.chart.title });
		function Pt() {
			let e = x({
				userConfig: R.config,
				defaultConfig: nt
			}), t = e, n = e.theme;
			if (n) if (!rt.value(e)) it(e), t = e;
			else {
				let r = x({
					userConfig: pe[n] || R.config,
					defaultConfig: e
				});
				t = {
					...x({
						userConfig: R.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : i[n] || a
				};
			}
			else t = e;
			return t.nodeCategories = R.config.nodeCategories || {}, t.nodeCategoryColors = R.config.nodeCategoryColors || {}, t;
		}
		let Ft = N(U.value.style.chart.width), It = N(U.value.style.chart.height);
		Ee(() => R.config, (e) => {
			Et.value || (U.value = Pt()), jt.value = !U.value.userOptions.showOnChartHover, At(), st.value += 1, K.value.showTable = U.value.table.show;
		}, { deep: !0 }), Ee(() => R.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (Ot.value = !1);
		}, { deep: !0 });
		let { isPrinting: Lt, isImaging: Rt, generatePdf: zt, generateImage: Bt } = v({
			elementId: `flow_${z.value}`,
			fileName: U.value.style.chart.title.text || "vue-ui-flow",
			options: U.value.userOptions.print
		}), Vt = C(() => U.value.userOptions.show && !U.value.style.chart.title.text), Ht = C(() => h(U.value.customPalette)), Ut = C(() => U.value.style.chart.nodes.width), K = N({
			showTable: U.value.table.show,
			showTooltip: U.value.style.chart.tooltip.show
		});
		Ee(U, () => {
			K.value = {
				showTable: U.value.table.show,
				showTooltip: U.value.style.chart.tooltip.show
			};
		}, { immediate: !0 });
		let Wt = C(() => !Dt.value || !Dt.value.length ? [] : Dt.value.map((e, t) => [
			e[0],
			e[1],
			c(e[2]),
			e[3] ? o(e[3]) : Ht.value[t] || Ht.value[t % Ht.value.length] || a[t] || a[t % a.length]
		]));
		function Gt(e) {
			let t = {}, n = {};
			function r(e, r) {
				t[e] || (t[e] = {
					level: null,
					inflow: 0,
					outflow: 0,
					children: [],
					color: null,
					uid: ee()
				}), t[e].level === null && (t[e].level = r), n[r] || (n[r] = []), n[r].includes(e) || n[r].push(e);
			}
			e.forEach(([e, n, i]) => {
				let a = t[e]?.level ?? 0, o = a + 1;
				r(e, a), r(n, o), t[e].children.push({
					target: n,
					value: i
				}), t[e].outflow += i, t[n].inflow += i;
			});
			let i = new Set(e.map(([e]) => e)), o = new Set(e.map(([, e]) => e)), s = Array.from(i).filter((e) => !o.has(e)), l = {};
			s.forEach((e, t) => {
				l[e] = Ht.value[t] || a[t % a.length];
			});
			let u = {};
			e.forEach(([e, t, n, r]) => {
				r && (u[e] = r, u[t] = r);
			}), Object.keys(t).forEach((e, n) => {
				let r = U.value.nodeCategories?.[e], i = r ? U.value.nodeCategoryColors?.[r] : null;
				t[e].color = u[e] || i || (s.includes(e) ? l[e] : null) || a[n % a.length];
			}), Object.keys(t).forEach((e) => {
				t[e].value = Math.max(t[e].inflow, t[e].outflow);
			});
			let d = Jt.value, f = Yt.value.width, p = Yt.value.height, m = Object.keys(n).map(Number).sort((e, t) => e - t), h = m.length || 1, te = h > 1 ? f / (h - 1) : 0, ne = Number(Ut.value), g = Number(U.value.style.chart.nodes.gapPx ?? U.value.style.chart.nodes.gap ?? 8), _ = Number(U.value.style.chart.nodes.minHeight || 0);
			function v(e) {
				let r = n[e], i = r.length;
				if (!i) return Infinity;
				let a = Math.max(0, (i - 1) * g), o = Math.max(0, p - a), s = Math.min(_, i ? o / i : 0), c = 0, l = r.map((e) => t[e].value || 0), u = l.reduce((e, t) => e + t, 0);
				for (let e = 0; e < 12; e += 1) {
					let e = u > 0 ? (o - c) / u : 0, t = [];
					for (let n = 0; n < l.length; n += 1) {
						let r = l[n];
						r < 0 || r * e < s && t.push(n);
					}
					if (!t.length) return Math.max(0, e);
					for (let e of t) c += s, u -= l[e], l[e] = -1;
					if (u <= 0) return 0;
				}
				return u > 0 ? Math.max(0, (o - c) / u) : 0;
			}
			let y = m.map(v), b = y.length ? Math.min(...y) : 0, x = {};
			m.forEach((e) => {
				let r = n[e], i = r.length, a = Math.max(0, (i - 1) * g), o = Math.max(0, p - a), s = Math.min(_, i ? o / i : 0), c = r.map((e) => Math.max(s, (t[e].value || 0) * b)), l = c.reduce((e, t) => e + t, 0) + a, u = Math.max(0, (p - l) / 2);
				r.forEach((n, r) => {
					let a = c[r], o = d.left + e * te, s = u;
					x[n] = {
						x: o,
						y: s,
						absoluteY: s,
						height: a,
						i: r,
						color: t[n].color,
						value: t[n].value,
						id: ee()
					}, u += a, r < i - 1 && (u += g);
				});
			});
			let re = [], S = d.top, ie = {}, ae = {};
			Object.keys(t).forEach((e) => {
				ie[e] = x[e]?.y ?? 0, ae[e] = 0;
			});
			let oe = !!U.value.style.chart.links.smooth, se = .5;
			return m.forEach((e) => {
				n[e].forEach((e) => {
					let n = t[e], r = x[e];
					if (!n.children || !n.children.length) return;
					let i = r.y;
					n.children.forEach(({ target: a, value: o }) => {
						let s = x[a], l = t[a], u = n.outflow > 0 ? o / n.outflow : 0, d = l.inflow > 0 ? o / l.inflow : 0, f = c(i + S), p = c(i + u * r.height + S), m = ie[a], h = m + d * s.height;
						ae[a] += o;
						let te = l.inflow > 0 && ae[a] >= l.inflow - 1e-6, g = s.y + s.height;
						(te || h > g - .25) && (h = g);
						let _ = c(m + S), v = c(h + S), y = c(r.x + ne), b = c(s.x), ce;
						if (!oe || b <= y) ce = `M ${y} ${f} L ${y} ${p} L ${b} ${v} L ${b} ${_} Z`;
						else {
							let e = b - y, t = c(y + e * se), n = c(b - e * se), r = f, i = _, a = p, o = v;
							ce = `M ${y} ${r} C ${t} ${r}, ${n} ${i}, ${b} ${i} L ${b} ${o} C ${n} ${o}, ${t} ${a}, ${y} ${a} Z`;
						}
						re.push({
							id: ee(),
							source: e,
							target: a,
							path: ce,
							value: o,
							sourceColor: n.color,
							targetColor: t[a].color
						}), i = p - S, ie[a] = h;
					});
				});
			}), {
				nodeCoordinates: x,
				links: re
			};
		}
		let q = C(() => {
			let e = Gt(Dt.value);
			return {
				nodes: Object.keys(e.nodeCoordinates).map((t, n) => ({
					...e.nodeCoordinates[t],
					name: t
				})),
				links: e.links
			};
		}), Kt = C(() => Ft.value), qt = C(() => It.value), Jt = C(() => U.value.style.chart.padding), Yt = C(() => ({
			width: Math.max(0, Kt.value - 40 - Jt.value.right - Jt.value.left),
			height: Math.max(0, qt.value - Jt.value.top - Jt.value.bottom)
		})), Xt = C(() => ({
			width: Kt.value,
			height: qt.value
		}));
		function Zt(e) {
			let t = {}, n = {}, r = /* @__PURE__ */ new Set();
			return Wt.value.forEach(([e, r, i]) => {
				t[e] || (t[e] = []), n[r] || (n[r] = []), t[e].push(r), n[r].push(e);
			}), t[e] && t[e].forEach((e) => r.add(e)), n[e] && n[e].forEach((e) => r.add(e)), Array.from(r).concat(e);
		}
		let J = N(null), Y = N(null), X = N(null), Qt = N(!1), $t = N(null);
		function en() {
			$t.value = null, J.value = null, Y.value = null, ct.value = !1, yt.value = null, V.value = null;
		}
		function tn(e) {
			if (!G.value || !e) return;
			let t = G.value.querySelector(`[data-a11y-node-id="${e}"]`);
			if (!t) return;
			let n = t.getBoundingClientRect();
			bt.value = {
				x: n.left + n.width / 2,
				y: n.top + n.height / 2
			};
		}
		function nn(e, t, n = "pointer", r = null) {
			$.value = [], J.value = Zt(e.name), Y.value = e.name, $t.value = e.id, xt.value = n, yt.value = r, V.value = e.id;
			let i = e.name, a = Wt.value, o = 0, c = 0, u = [], d = [], ee = new Set(a.map(([e]) => e)), f = new Set(a.map(([, e]) => e)), p = Array.from(ee).filter((e) => !f.has(e)), m = a.filter(([e]) => p.includes(e)).reduce((e, [t, n, r]) => e + r, 0), h = {};
			q.value.nodes.forEach((e) => {
				h[e.name] = e.color;
			}), a.forEach(([e, t, n]) => {
				t === i && (o += n, u.push({
					source: e,
					value: n,
					color: h[e]
				})), e === i && (c += n, d.push({
					target: t,
					value: n,
					color: h[t]
				}));
			});
			let ne = m > 0 ? Math.max(o, c) / m * 100 : 0, g = {
				name: i,
				inflow: o,
				outflow: c,
				from: u,
				to: d,
				percentOfTotal: ne,
				color: h[i] || "#000000"
			};
			U.value.events.datapointEnter && U.value.events.datapointEnter({
				datapoint: g,
				seriesIndex: t
			}), X.value = {
				datapoint: g,
				config: U.value,
				seriesIndex: t,
				series: q.value
			}, ct.value = !0;
			let _ = "", v = U.value.style.chart.tooltip.customFormat;
			if (Qt.value = !1, te(v)) try {
				let e = v({
					datapoint: g,
					series: q.value,
					config: U.value
				});
				typeof e == "string" && (lt.value = e, Qt.value = !0);
			} catch {
				console.warn("Custom format cannot be applied.");
			}
			if (!Qt.value) {
				let e = U.value.style.chart.tooltip.showPercentage ? `<div>${s({
					p: U.value.style.chart.tooltip.translations.percentOfTotal,
					v: g.percentOfTotal,
					s: "%",
					r: U.value.style.chart.tooltip.roundingPercentage
				})}</div>` : "";
				_ += `<div style="width:100%;text-align:center;border-bottom:1px solid ${U.value.style.chart.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;"><span style="margin-right:4px; color:${g.color}">⏹</span>${g.name}${e}</div>`, g.from.length && (_ += `<div>${U.value.style.chart.tooltip.translations.from}</div>`, g.from.forEach((e) => {
					_ += `<div><span style="color:${e.color}">⏹←</span> ${e.source}: ${l(U.value.style.chart.nodes.labels.formatter, e.value, s({
						p: U.value.style.chart.nodes.labels.prefix,
						v: e.value,
						s: U.value.style.chart.nodes.labels.suffix,
						r: U.value.style.chart.nodes.labels.rounding
					}))}</div>`;
				})), g.to.length && (_ += `<div style="margin-top:6px;">${U.value.style.chart.tooltip.translations.to}</div>`, g.to.forEach((e) => {
					_ += `<div><span style="color:${e.color}">⏹→</span> ${e.target}: ${l(U.value.style.chart.nodes.labels.formatter, e.value, s({
						p: U.value.style.chart.nodes.labels.prefix,
						v: e.value,
						s: U.value.style.chart.nodes.labels.suffix,
						r: U.value.style.chart.nodes.labels.rounding
					}))}</div>`;
				})), lt.value = _;
			}
			n === "keyboard" && be(() => {
				tn(e.id);
			});
		}
		function rn(e) {
			let t = X.value;
			U.value.events.datapointLeave && U.value.events.datapointLeave({
				datapoint: t,
				seriesIndex: e
			}), !(xt.value === "keyboard" && V.value) && ($t.value = null, J.value = null, Y.value = null, ct.value = !1);
		}
		function an(e) {
			let t = X.value;
			U.value.events.datapointClick && U.value.events.datapointClick({
				datapoint: t,
				seriesIndex: e
			});
		}
		let on = C(() => q.value.links.map(({ source: e, target: t, sourceColor: n, targetColor: r, value: i }) => ({
			source: e,
			target: t,
			sourceColor: n,
			targetColor: r,
			value: i
		})));
		function sn(e = null) {
			be(() => {
				let r = on.value.map((e, t) => [
					[e.source],
					[e.target],
					[e.value]
				]), i = [
					[U.value.style.chart.title.text],
					[U.value.style.chart.title.subtitle.text],
					[
						[U.value.table.columnNames.source],
						[U.value.table.columnNames.target],
						[U.value.table.columnNames.value]
					]
				].concat(r), a = n(i);
				e ? e(a) : t({
					csvContent: a,
					title: U.value.style.chart.title.text || "vue-ui-flow"
				});
			});
		}
		let Z = C(() => {
			let e = [
				U.value.table.columnNames.source,
				U.value.table.columnNames.target,
				U.value.table.columnNames.value
			], t = on.value.map((e, t) => [
				{
					color: e.sourceColor,
					name: e.source,
					shape: "square"
				},
				{
					color: e.targetColor,
					name: e.target,
					shape: "square"
				},
				s({
					p: U.value.style.chart.nodes.labels.prefix,
					v: e.value,
					s: U.value.style.chart.nodes.labels.suffix,
					r: U.value.style.chart.nodes.labels.rounding
				})
			]), n = {
				th: {
					backgroundColor: U.value.table.th.backgroundColor,
					color: U.value.table.th.color,
					outline: U.value.table.th.outline
				},
				td: {
					backgroundColor: U.value.table.td.backgroundColor,
					color: U.value.table.td.color,
					outline: U.value.table.td.outline
				},
				breakpoint: U.value.table.responsiveBreakpoint
			};
			return {
				colNames: [
					U.value.table.columnNames.source,
					U.value.table.columnNames.target,
					U.value.table.columnNames.value
				],
				head: e,
				body: t,
				config: n
			};
		}), cn = C(() => ({
			headers: Z.value?.colNames ?? [],
			rows: Z.value?.body ?? []
		})), ln = C(() => [...q.value.nodes].map((e, t) => ({
			...e,
			index: t,
			centerX: e.x + Ut.value / 2,
			centerY: c(e.absoluteY) + U.value.style.chart.padding.top + e.height / 2
		})).sort((e, t) => e.x === t.x ? e.absoluteY - t.absoluteY : e.x - t.x)), Q = C(() => {
			let e = [];
			return ln.value.forEach((t) => {
				let n = e.find((e) => Math.abs(e.x - t.x) <= 1);
				n ? n.nodes.push(t) : e.push({
					x: t.x,
					nodes: [t]
				});
			}), e.forEach((e) => {
				e.nodes.sort((e, t) => e.absoluteY - t.absoluteY);
			}), e.sort((e, t) => e.x - t.x);
		}), un = C(() => {
			let e = /* @__PURE__ */ new Map();
			return ln.value.forEach((t, n) => {
				e.set(t.id, n);
			}), e;
		});
		function dn() {
			return q.value;
		}
		function fn() {
			K.value.showTable = !K.value.showTable;
		}
		let pn = N(!1);
		function mn() {
			pn.value = !pn.value;
		}
		function hn() {
			K.value.showTooltip = !K.value.showTooltip;
		}
		let gn = C(() => {
			let e = new Set(q.value.nodes.map((e) => U.value.nodeCategories[e.name] || "__uncategorized__"));
			return Array.from(e).map((e) => ({
				name: e,
				color: U.value.nodeCategoryColors[e] || a[0],
				shape: "square",
				count: q.value.nodes.filter((t) => (U.value.nodeCategories[t.name] || "__uncategorized__") === e).length
			})).map((e, t) => {
				let n = $.value.includes(t);
				return {
					...e,
					segregate: () => vn({
						legend: e,
						i: t
					}),
					isSegregated: n,
					opacity: $.value.length ? n ? 1 : .5 : 1,
					display: `${e.name} (${e.count})`
				};
			});
		}), _n = C(() => gn.value.filter((e) => e.name !== "__uncategorized__")), $ = N([]);
		function vn({ legend: e, i: t }) {
			let n = e.name;
			if (J.value?.every((e) => U.value.nodeCategories[e] === n)) {
				J.value = null, Y.value = null, $.value = [];
				return;
			}
			$.value = [t], J.value = q.value.nodes.filter((e) => U.value.nodeCategories[e.name] === n).map((e) => e.name), Y.value = null;
		}
		let yn = C(() => ({
			cy: "flow-legend",
			backgroundColor: U.value.style.chart.legend.backgroundColor,
			color: U.value.style.chart.legend.color,
			fontSize: U.value.style.chart.legend.fontSize,
			paddingBottom: U.value.style.chart.legend.paddingBottom,
			fontWeight: U.value.style.chart.legend.bold ? "bold" : "normal"
		}));
		async function bn({ scale: e = 2 } = {}) {
			if (!B.value) return;
			let { width: t, height: n } = B.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await ie({
				domElement: B.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: U.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let xn = C(() => {
			let e = U.value.table.useDialog && !U.value.table.show, t = K.value.showTable;
			return {
				component: e ? tt : Xe,
				title: `${U.value.style.chart.title.text}${U.value.style.chart.title.subtitle.text ? `: ${U.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: U.value.table.th.backgroundColor,
					color: U.value.table.th.color,
					headerColor: U.value.table.th.color,
					headerBg: U.value.table.th.backgroundColor,
					isFullscreen: H.value,
					fullscreenParent: B.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
					isCursorPointer: W.value
				} : {
					hideDetails: !0,
					config: {
						open: t,
						maxHeight: 1e4,
						body: {
							backgroundColor: U.value.style.chart.backgroundColor,
							color: U.value.style.chart.color
						},
						head: {
							backgroundColor: U.value.style.chart.backgroundColor,
							color: U.value.style.chart.color
						}
					}
				}
			};
		});
		Ee(() => K.value.showTable, (e) => {
			U.value.table.show || (e && U.value.table.useDialog && gt.value ? gt.value.open() : "close" in gt.value && gt.value.close());
		});
		function Sn() {
			K.value.showTable = !1, _t.value && _t.value.setTableIconState(!1);
		}
		let Cn = C(() => _n.value.map((e) => ({
			...e,
			name: e.display
		}))), wn = C(() => U.value.style.chart.backgroundColor), Tn = C(() => U.value.style.chart.legend), En = C(() => U.value.style.chart.title), { isCallbackImaging: Dn, isCallbackSvg: On, generateSvg: kn, onGenerateImage: An } = S({
			svg: G,
			title: En,
			legend: Tn,
			legendItems: Cn,
			backgroundColor: wn,
			getSvgCallback: () => U.value.userOptions.callbacks.svg,
			generateImage: Bt
		});
		async function jn() {
			if (at("copyAlt", {
				config: U.value,
				dataset: q.value
			}), !U.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(U.value.userOptions.callbacks.altCopy({
				config: U.value,
				dataset: q.value
			}));
		}
		function Mn(e) {
			return Q.value.findIndex((t) => t.nodes.some((t) => t.id === e));
		}
		function Nn(e, t) {
			let n = Q.value[e];
			return n ? n.nodes.findIndex((e) => e.id === t) : -1;
		}
		function Pn(e, t) {
			let n = Q.value[e];
			if (!n || !n.nodes.length) return null;
			let r = n.nodes[0], i = Math.abs(r.centerY - t);
			for (let e = 1; e < n.nodes.length; e += 1) {
				let a = n.nodes[e], o = Math.abs(a.centerY - t);
				o < i && (r = a, i = o);
			}
			return r;
		}
		function Fn() {
			yt.value = null, V.value = null, St.value = !0;
		}
		function In() {
			en(), St.value = !1;
		}
		function Ln(e) {
			if (!G.value || pn.value || document.activeElement !== G.value || !ln.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "ArrowUp", i = e.key === "ArrowDown", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				en();
				return;
			}
			if (a) {
				if (yt.value === null) return;
				an(yt.value);
				return;
			}
			if (V.value === null) {
				let e = Q.value[0]?.nodes?.[0];
				if (!e) return;
				let t = un.value.get(e.id) ?? 0;
				nn(e, t, "keyboard", t);
				return;
			}
			let s = un.value.get(V.value);
			if (s === void 0) return;
			let c = ln.value[s];
			if (!c) return;
			let l = Mn(c.id);
			if (l < 0) return;
			let u = null;
			if (r || i) {
				let e = Nn(l, c.id);
				if (e < 0) return;
				let t = Q.value[l];
				if (!t?.nodes?.length) return;
				let n = e + (i ? 1 : -1);
				n < 0 && (n = t.nodes.length - 1), n >= t.nodes.length && (n = 0), u = t.nodes[n];
			}
			if (t || n) {
				let e = l + (n ? 1 : -1);
				e < 0 && (e = Q.value.length - 1), e >= Q.value.length && (e = 0), u = Pn(e, c.centerY);
			}
			if (!u) return;
			let d = un.value.get(u.id);
			d !== void 0 && nn(u, d, "keyboard", d);
		}
		return oe({
			getData: dn,
			getImage: bn,
			generateCsv: sn,
			generateImage: Bt,
			generateSvg: kn,
			generatePdf: zt,
			toggleTable: fn,
			toggleAnnotator: mn,
			toggleTooltip: hn,
			drillCategory: vn,
			unselectNode: rn,
			toggleFullscreen: wt,
			copyAlt: jn
		}), (e, t) => (M(), E("div", {
			ref_key: "flowChart",
			ref: B,
			class: xe(`vue-data-ui-component vue-ui-flow ${H.value ? "vue-data-ui-wrapper-fullscreen" : ""}`),
			style: j(`font-family:${U.value.style.fontFamily};width:100%; text-align:center;background:${U.value.style.chart.backgroundColor}`),
			id: `flow_${z.value}`,
			onMouseenter: t[2] ||= () => I(Mt)(!0),
			onMouseleave: t[3] ||= () => {
				I(Mt)(!1), St.value || en();
			}
		}, [
			D("div", {
				id: `chart-instructions-${z.value}`,
				class: "sr-only"
			}, [D("p", null, F(U.value.a11y.translations.keyboardNavigation), 1)], 8, ke),
			cn.value?.rows?.length ? (M(), w(le, {
				key: 0,
				uid: z.value,
				head: cn.value.headers,
				body: cn.value.rows,
				notice: U.value.a11y.translations.tableAvailable,
				caption: U.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : T("", !0),
			U.value.userOptions.buttons.annotator ? (M(), w(I(Qe), {
				key: 1,
				svgRef: I(G),
				backgroundColor: U.value.style.chart.backgroundColor,
				color: U.value.style.chart.color,
				active: pn.value,
				onClose: mn,
				isCursorPointer: W.value
			}, {
				"annotator-action-close": L(() => [P(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": L(({ color: t }) => [P(e.$slots, "annotator-action-color", A(k({ color: t })), void 0, !0)]),
				"annotator-action-draw": L(({ mode: t }) => [P(e.$slots, "annotator-action-draw", A(k({ mode: t })), void 0, !0)]),
				"annotator-action-undo": L(({ disabled: t }) => [P(e.$slots, "annotator-action-undo", A(k({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": L(({ disabled: t }) => [P(e.$slots, "annotator-action-redo", A(k({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": L(({ disabled: t }) => [P(e.$slots, "annotator-action-delete", A(k({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : T("", !0),
			Vt.value ? (M(), E("div", Ae, null, 512)) : T("", !0),
			U.value.style.chart.title.text ? (M(), E("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: dt,
				style: "width:100%;background:transparent;padding-bottom:24px"
			}, [(M(), w(ae, {
				key: `title_${st.value}`,
				config: {
					title: {
						cy: "flow-title",
						...U.value.style.chart.title
					},
					subtitle: {
						cy: "flow-subtitle",
						...U.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : T("", !0),
			D("div", { id: `legend-top-${z.value}` }, null, 8, je),
			U.value.userOptions.show && Ct.value && (I(Nt) || I(jt)) ? (M(), w(I($e), {
				ref_key: "userOptionsRef",
				ref: _t,
				key: `user_option_${ot.value}`,
				backgroundColor: U.value.style.chart.backgroundColor,
				color: U.value.style.chart.color,
				isPrinting: I(Lt),
				isImaging: I(Rt),
				uid: z.value,
				hasPdf: U.value.userOptions.buttons.pdf,
				hasXls: U.value.userOptions.buttons.csv,
				hasImg: U.value.userOptions.buttons.img,
				hasSvg: U.value.userOptions.buttons.svg,
				hasTable: U.value.userOptions.buttons.table,
				callbacks: U.value.userOptions.callbacks,
				hasFullscreen: U.value.userOptions.buttons.fullscreen,
				hasAltCopy: U.value.userOptions.buttons.altCopy,
				isFullscreen: H.value,
				titles: { ...U.value.userOptions.buttonTitles },
				chartElement: B.value,
				position: U.value.userOptions.position,
				hasAnnotator: U.value.userOptions.buttons.annotator,
				printScale: U.value.userOptions.print.scale,
				isAnnotation: pn.value,
				hasTooltip: U.value.style.chart.tooltip.show && U.value.userOptions.buttons.tooltip,
				isTooltip: K.value.showTooltip,
				tableDialog: U.value.table.useDialog,
				isCursorPointer: W.value,
				onToggleTooltip: hn,
				onToggleFullscreen: wt,
				onGeneratePdf: I(zt),
				onGenerateCsv: sn,
				onGenerateImage: I(An),
				onGenerateSvg: I(kn),
				onToggleTable: fn,
				onToggleAnnotator: mn,
				onCopyAlt: jn,
				style: j({ visibility: I(Nt) ? I(jt) ? "visible" : "hidden" : "visible" })
			}, ge({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: L(({ isOpen: t, color: n }) => [P(e.$slots, "menuIcon", A(k({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: L(() => [P(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: L(() => [P(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: L(() => [P(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: L(() => [P(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: L(() => [P(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: L(() => [P(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: L(({ toggleFullscreen: t, isFullscreen: n }) => [P(e.$slots, "optionFullscreen", A(k({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: L(({ toggleAnnotator: t, isAnnotator: n }) => [P(e.$slots, "optionAnnotator", A(k({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: L(({ altCopy: t }) => [P(e.$slots, "optionAltCopy", A(k({ altCopy: t })), void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: L(() => [P(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "10"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: L(() => [P(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "11"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasXls.hasImg.hasSvg.hasTable.callbacks.hasFullscreen.hasAltCopy.isFullscreen.titles.chartElement.position.hasAnnotator.printScale.isAnnotation.hasTooltip.isTooltip.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : T("", !0),
			D("div", Me, [(M(), E("svg", {
				ref_key: "svgRef",
				ref: G,
				xmlns: I(p),
				"aria-describedby": `chart-instructions-${z.value}`,
				viewBox: `0 0 ${Xt.value.width} ${Xt.value.height}`,
				class: xe({
					"vue-data-ui-fullscreen--on": H.value,
					"vue-data-ui-fulscreen--off": !H.value
				}),
				style: j({
					maxWidth: "100%",
					overflow: "visible",
					background: "transparent",
					color: U.value.style.chart.color
				}),
				tabindex: "0",
				onFocus: Fn,
				onBlur: In,
				onKeydown: Ln
			}, [
				ve(I(et)),
				e.$slots["chart-background"] ? (M(), E("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: Xt.value.width,
					height: Xt.value.height,
					style: { pointerEvents: "none" }
				}, [P(e.$slots, "chart-background", {}, void 0, !0)], 8, Pe)) : T("", !0),
				D("defs", null, [(M(!0), E(me, null, Ce(q.value.links, (e, t) => (M(), E("linearGradient", {
					id: e.id,
					x1: "0%",
					y1: "0%",
					x2: "100%",
					y2: "0%"
				}, [D("stop", {
					offset: "0%",
					"stop-color": e.sourceColor
				}, null, 8, Ie), D("stop", {
					offset: "100%",
					"stop-color": e.targetColor
				}, null, 8, Le)], 8, Fe))), 256))]),
				(M(!0), E(me, null, Ce(q.value.links, (e) => (M(), E("path", {
					class: "vue-ui-flow-link",
					d: e.path,
					"stroke-linejoin": "round",
					"stroke-miterlimit": "1",
					fill: `url(#${e.id})`,
					stroke: U.value.style.chart.links.stroke,
					"stroke-width": U.value.style.chart.links.strokeWidth,
					style: j(`
                        opacity:${J.value ? J.value.includes(e.source) && J.value.includes(e.target) ? 1 : .3 : Y.value ? [e.target, e.source].includes(Y.value) ? 1 : .3 : U.value.style.chart.links.opacity}
                    `)
				}, null, 12, Re))), 256)),
				(M(!0), E(me, null, Ce(q.value.nodes, (e, t) => (M(), E("rect", {
					"data-a11y-node-id": e.id,
					class: "vue-ui-flow-node",
					x: e.x,
					y: I(c)(e.absoluteY) + U.value.style.chart.padding.top,
					height: I(c)(e.height),
					width: Ut.value,
					fill: e.color,
					stroke: U.value.style.chart.nodes.stroke,
					"stroke-width": U.value.style.chart.nodes.strokeWidth,
					rx: U.value.style.chart.nodes.borderRadius,
					style: j({
						opacity: J.value ? J.value.includes(e.name) ? 1 : .3 : 1,
						outline: $t.value !== null && $t.value === e.id ? "2px solid currentColor" : void 0
					}),
					"aria-label": `${e.name}: ${I(l)(U.value.style.chart.nodes.labels.formatter, e.value, I(s)({
						p: U.value.style.chart.nodes.labels.prefix,
						v: e.value,
						s: U.value.style.chart.nodes.labels.suffix,
						r: U.value.style.chart.nodes.labels.rounding
					}))}`,
					onMouseenter: (n) => nn(e, t, "pointer", un.value.get(e.id)),
					onMouseleave: (e) => rn(t),
					onClick: (e) => an(t)
				}, null, 44, ze))), 256)),
				U.value.style.chart.nodes.labels.show ? (M(), E("g", Be, [(M(!0), E(me, null, Ce(q.value.nodes, (e, t) => (M(), E("text", {
					x: e.x + Ut.value / 2,
					y: (U.value.style.chart.nodes.labels.showValue ? I(c)(e.absoluteY + e.height / 2 - U.value.style.chart.nodes.labels.fontSize / 4) : e.absoluteY + e.height / 2 + U.value.style.chart.nodes.labels.fontSize / 3) + U.value.style.chart.padding.top,
					"font-size": U.value.style.chart.nodes.labels.fontSize,
					fill: I(f)(e.color),
					"text-anchor": "middle",
					style: j(`pointer-events: none; opacity:${J.value ? +!!J.value.includes(e.name) : 1}`)
				}, F(U.value.style.chart.nodes.labels.abbreviation.use ? I(d)({
					source: e.name,
					length: U.value.style.chart.nodes.labels.abbreviation.length
				}) : e.name), 13, Ve))), 256)), U.value.style.chart.nodes.labels.showValue ? (M(!0), E(me, { key: 0 }, Ce(q.value.nodes, (e, t) => (M(), E("text", {
					x: e.x + Ut.value / 2,
					y: I(c)(e.absoluteY + e.height / 2 + U.value.style.chart.nodes.labels.fontSize / 1.3) + U.value.style.chart.padding.top,
					"font-size": U.value.style.chart.nodes.labels.fontSize,
					fill: I(f)(e.color),
					"text-anchor": "middle",
					style: j(`pointer-events: none; opacity:${J.value ? +!!J.value.includes(e.name) : 1}`)
				}, F(I(l)(U.value.style.chart.nodes.labels.formatter, e.value, I(s)({
					p: U.value.style.chart.nodes.labels.prefix,
					v: e.value,
					s: U.value.style.chart.nodes.labels.suffix,
					r: U.value.style.chart.nodes.labels.rounding
				}), {
					datapoint: e,
					seriesIndex: t
				})), 13, He))), 256)) : T("", !0)])) : T("", !0),
				P(e.$slots, "svg", { svg: {
					...Xt.value,
					isPrintingImg: I(Lt) || I(Rt) || I(Dn),
					isPrintingSvg: I(On)
				} }, void 0, !0)
			], 46, Ne)), e.$slots.hint ? (M(), E("div", Ue, [P(e.$slots, "hint", A(k({
				hint: U.value.a11y.translations.keyboardNavigation,
				isVisible: St.value
			})), void 0, !0)])) : T("", !0)]),
			e.$slots.watermark ? (M(), E("div", We, [P(e.$slots, "watermark", A(k({ isPrinting: I(Lt) || I(Rt) || I(Dn) || I(On) })), void 0, !0)])) : T("", !0),
			D("div", { id: `legend-bottom-${z.value}` }, null, 8, Ge),
			ht.value && (U.value.style.chart.legend.show || e.$slots.legend) ? (M(), w(he, {
				key: 6,
				to: U.value.style.chart.legend.position === "top" ? `#legend-top-${z.value}` : `#legend-bottom-${z.value}`
			}, [D("div", {
				ref_key: "chartLegend",
				ref: ut
			}, [P(e.$slots, "legend", { legend: gn.value }, () => [U.value.style.chart.legend.show && _n.value.length ? (M(), w(fe, {
				key: 0,
				legendSet: _n.value,
				config: yn.value,
				isCursorPointer: W.value,
				onClickMarker: t[0] ||= (e) => vn(e)
			}, {
				item: L(({ legend: e, index: t }) => [I(Et) ? T("", !0) : (M(), E("div", {
					key: 0,
					onClick: (t) => e.segregate(),
					style: j(`opacity:${$.value.length ? $.value.includes(t) ? 1 : .5 : 1}`)
				}, F(e.display), 13, Ke))]),
				_: 1
			}, 8, [
				"legendSet",
				"config",
				"isCursorPointer"
			])) : T("", !0)], !0)], 512)], 8, ["to"])) : T("", !0),
			e.$slots.source ? (M(), E("div", {
				key: 7,
				ref_key: "source",
				ref: ft,
				dir: "auto"
			}, [P(e.$slots, "source", {}, void 0, !0)], 512)) : T("", !0),
			ve(I(Je), {
				ref_key: "tooltip",
				ref: vt,
				teleportTo: U.value.style.chart.tooltip.teleportTo,
				show: K.value.showTooltip && ct.value,
				backgroundColor: U.value.style.chart.tooltip.backgroundColor,
				color: U.value.style.chart.tooltip.color,
				fontSize: U.value.style.chart.tooltip.fontSize,
				borderRadius: U.value.style.chart.tooltip.borderRadius,
				borderColor: U.value.style.chart.tooltip.borderColor,
				borderWidth: U.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: U.value.style.chart.tooltip.backgroundOpacity,
				position: U.value.style.chart.tooltip.position,
				offsetX: U.value.style.chart.tooltip.offsetX,
				offsetY: U.value.style.chart.tooltip.offsetY,
				parent: B.value,
				content: lt.value,
				isCustom: Qt.value,
				isFullscreen: H.value,
				smooth: U.value.style.chart.tooltip.smooth,
				backdropFilter: U.value.style.chart.tooltip.backdropFilter,
				smoothForce: U.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: U.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: xt.value === "keyboard",
				a11yPosition: bt.value
			}, {
				"tooltip-before": L(() => [P(e.$slots, "tooltip-before", A(k({ ...X.value })), void 0, !0)]),
				tooltip: L(() => [P(e.$slots, "tooltip", A(k({ ...X.value })), void 0, !0)]),
				"tooltip-after": L(() => [P(e.$slots, "tooltip-after", A(k({ ...X.value })), void 0, !0)]),
				_: 3
			}, 8, [
				"teleportTo",
				"show",
				"backgroundColor",
				"color",
				"fontSize",
				"borderRadius",
				"borderColor",
				"borderWidth",
				"backgroundOpacity",
				"position",
				"offsetX",
				"offsetY",
				"parent",
				"content",
				"isCustom",
				"isFullscreen",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			Ct.value && U.value.userOptions.buttons.table ? (M(), w(we(xn.value.component), ye({ key: 8 }, xn.value.props, {
				ref_key: "tableUnit",
				ref: gt,
				onClose: Sn
			}), ge({
				content: L(() => [ve(I(Ze), {
					colNames: Z.value.colNames,
					head: Z.value.head,
					body: Z.value.body,
					config: Z.value.config,
					title: U.value.table.useDialog ? "" : xn.value.title,
					withCloseButton: !U.value.table.useDialog,
					isCursorPointer: W.value,
					onClose: Sn
				}, {
					th: L(({ th: e }) => [D("div", {
						innerHTML: e,
						style: {
							display: "flex",
							"align-items": "center"
						}
					}, null, 8, qe)]),
					td: L(({ td: e }) => [_e(F(e.name || e), 1)]),
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
			}, [U.value.table.useDialog ? {
				name: "title",
				fn: L(() => [_e(F(xn.value.title), 1)]),
				key: "0"
			} : void 0, U.value.table.useDialog ? {
				name: "actions",
				fn: L(() => [D("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => sn(U.value.userOptions.callbacks.csv),
					style: j({ cursor: W.value ? "pointer" : "default" })
				}, [ve(I(Ye), {
					name: "fileCsv",
					stroke: xn.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : T("", !0),
			P(e.$slots, "skeleton", {}, () => [I(Et) ? (M(), w(b, { key: 0 })) : T("", !0)], !0)
		], 46, Oe));
	}
}, [["__scopeId", "data-v-c203fc92"]]);
//#endregion
export { De as n, Je as t };
