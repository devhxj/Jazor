import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, D as n, G as r, Jt as i, Kt as a, P as o, Pt as s, S as c, V as l, X as u, c as ee, i as te, jt as ne, pt as re, q as ie, r as ae, t as oe, tt as se, w as ce, wt as d } from "./lib-Bttd6u5E.js";
import { n as le, t as ue } from "./useHints-Dq_w2E8B.js";
import { t as de } from "./useTimeLabels-d2f-W1L4.js";
import { t as fe } from "./useConfig-DlNpz6P8.js";
import { t as pe } from "./usePrinter-DN5bYhTG.js";
import { n as me, t as he } from "./BaseScanner-DZvpgOjM.js";
import { t as ge } from "./useNestedProp-vPNvh7rV.js";
import { t as _e } from "./useThemeCheck-C43Tcqmk.js";
import { t as ve } from "./useChartExport-DNiwdPmb.js";
import { t as ye } from "./useTimeLabelCollider-AEcY4Ioe.js";
import { n as be } from "./Title-BE3qg9xl.js";
import { t as xe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as Se, t as Ce } from "./useResponsive-ZtArZtUf.js";
import { t as we } from "./A11yDataTable-DdRsVULz.js";
import { t as Te } from "./useUserOptionState-DK-_1ddE.js";
import { t as Ee } from "./useChartAccessibility-DYqac8yF.js";
import { t as De } from "./vue_ui_bump-Vl-zYAtG.js";
import { t as Oe } from "./BaseDraggableDialog-LoqqwRtV.js";
import { Fragment as f, computed as p, createBlock as m, createCommentVNode as h, createElementBlock as g, createElementVNode as _, createSlots as ke, createTextVNode as Ae, createVNode as je, defineAsyncComponent as v, guardReactiveProps as y, mergeProps as Me, normalizeClass as Ne, normalizeProps as b, normalizeStyle as x, onBeforeUnmount as Pe, onMounted as Fe, openBlock as S, ref as C, renderList as w, renderSlot as T, resolveDynamicComponent as Ie, shallowRef as Le, toDisplayString as E, toRefs as Re, unref as D, watch as ze, watchEffect as Be, withCtx as O } from "vue";
//#region src/components/vue-ui-bump.vue
var Ve = /* @__PURE__ */ e({ default: () => ct }), He = ["id"], Ue = ["id"], We = { style: { position: "relative" } }, Ge = [
	"xmlns",
	"aria-describedby",
	"viewBox"
], Ke = [
	"x",
	"y",
	"width",
	"height"
], qe = [
	"d",
	"stroke",
	"stroke-width",
	"onMouseenter"
], Je = [
	"d",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Ye = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Xe = [
	"x",
	"y",
	"fill",
	"font-size"
], Ze = [
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Qe = [
	"x",
	"y",
	"fill",
	"font-size"
], $e = [
	"x",
	"y",
	"fill",
	"font-size",
	"font--weight",
	"innerHTML",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], et = [
	"x",
	"y",
	"fill",
	"font-size",
	"font--weight",
	"innerHTML",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], tt = { key: 0 }, nt = { key: 1 }, rt = [
	"text-anchor",
	"font-size",
	"font-weight",
	"fill",
	"transform"
], it = [
	"text-anchor",
	"font-size",
	"fill",
	"transform",
	"innerHTML"
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
}, st = ["innerHTML"], ct = /*#__PURE__*/ xe({
	__name: "vue-ui-bump",
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
	setup(e, { expose: xe, emit: Ve }) {
		let ct = v(() => import("./BaseIcon-BfndwIWE.js").then((e) => e.n)), lt = v(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)), ut = v(() => import("./DataTable-BbKgJ5UI.js")), dt = v(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), ft = v(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), pt = v(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_bump: mt } = fe(), { isThemeValid: ht, warnInvalidTheme: gt } = _e(), k = e, _t = Ve, vt = p({
			get() {
				return !!k.dataset && k.dataset.length;
			},
			set(e) {
				return e;
			}
		}), A = C(null), j = C(ie()), yt = C(0), bt = C(null), xt = C(null), St = C(null), Ct = C(!1), wt = C(!1), Tt = C(0), Et = C(0), M = C(null), Dt = C(null), Ot = C(!1), N = C(null), kt = C(null), At = C(null), jt = C(null), P = C(null), Mt = C(!1), F = C(Lt());
		le({
			config: () => F.value,
			dataset: () => k.dataset,
			component: "VueUiBump",
			rules: [
				ue.emptyArray,
				{
					test: (e) => e.length > 31,
					message: [
						"👀 The number of series > 31. Consider:",
						"",
						"▶️ Using filters to let users choose a maximum number of series to display",
						"",
						"▶️ Use several instances of the component to show related series"
					]
				},
				{
					test: (e) => e.length === 1,
					message: [
						"👀 There is only 1 series in your dataset. Consider:",
						"",
						"▶️ Using a line chart, with VueUiXy."
					]
				},
				{
					test: (e) => e.some((e) => e.values.length > 31),
					message: [
						"👀 Some series contain > 31 data points, which can make the chart hard to read. Consider:",
						"",
						"▶️ Use filters to show less data points at a time.",
						"",
						"▶️ Use larger time scales, or aggregated values."
					]
				}
			]
		});
		let I = p(() => F.value.userOptions.useCursorPointer), Nt = p(() => i({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					layout: {
						timeLabels: { show: !1 },
						lines: { coatingColor: "#4A4A4A" },
						plots: {
							stroke: "#4A4A4A",
							labels: {
								show: !1,
								displayedValue: "rank"
							}
						},
						nameLabels: { useSerieColor: !0 }
					}
				} }
			},
			userConfig: F.value.skeletonConfig ?? {}
		})), { loading: Pt, FINAL_DATASET: Ft, manualLoading: It } = me({
			...Re(k),
			FINAL_CONFIG: F,
			prepareConfig: Lt,
			skeletonDataset: k.config?.skeletonDataset ?? [
				{
					name: "————",
					values: [
						1,
						1,
						1,
						2,
						2,
						2,
						3,
						3,
						2,
						2
					],
					color: "#4A4A4A"
				},
				{
					name: "————",
					values: [
						2,
						2,
						2,
						1,
						3,
						3,
						2,
						2,
						3,
						3
					],
					color: "#6A6A6A"
				},
				{
					name: "————",
					values: [
						3,
						3,
						3,
						3,
						1,
						1,
						1,
						1,
						1,
						1
					],
					color: "#8A8A8A"
				}
			],
			skeletonConfig: i({
				defaultConfig: F.value,
				userConfig: Nt.value
			})
		});
		Fe(Zt);
		function Lt() {
			let e = ge({
				userConfig: k.config,
				defaultConfig: mt
			}), t = {}, n = e.theme;
			if (n) if (!ht.value(e)) gt(e), t = e;
			else {
				let r = ge({
					userConfig: De[n] || k.config,
					defaultConfig: e
				});
				t = {
					...ge({
						userConfig: k.config,
						defaultConfig: r
					}),
					customPalette: e.customPalette.length ? e.customPalette : a[n] || s
				};
			}
			else t = e;
			return t;
		}
		let { userOptionsVisible: Rt, setUserOptionsVisibility: zt, keepUserOptionState: Bt } = Te({ config: F.value }), { svgRef: L } = Ee({ config: F.value.style.chart.title });
		function Vt() {
			Ot.value = !0, zt(!0);
		}
		function Ht() {
			zt(!1), Ot.value = !1;
		}
		let R = C({ showTable: F.value.table.show });
		ze(F, () => {
			R.value = { showTable: F.value.table.show };
		}, { immediate: !0 }), ze(() => k.config, (e) => {
			Pt.value || (F.value = Lt()), Rt.value = !F.value.userOptions.showOnChartHover, Zt(), Tt.value += 1, Et.value += 1, R.value.showTable = F.value.table.show, z.value.width = F.value.style.chart.width, z.value.height = F.value.style.chart.height, z.value.paddingRatio = {
				top: F.value.style.chart.padding.top / F.value.style.chart.height,
				right: F.value.style.chart.padding.right / F.value.style.chart.width,
				bottom: F.value.style.chart.padding.bottom / F.value.style.chart.height,
				left: F.value.style.chart.padding.left / F.value.style.chart.width
			};
		}), ze(() => k.dataset, (e) => {
			Array.isArray(e) && e.length > 0 && (It.value = !1);
		}, { deep: !0 });
		let { isPrinting: Ut, isImaging: Wt, generatePdf: Gt, generateImage: Kt } = pe({
			elementId: `bump_${j.value}`,
			fileName: F.value.style.chart.title.text || "vue-ui-bump",
			options: F.value.userOptions.print
		}), qt = p(() => F.value.userOptions.show && !F.value.style.chart.title.text), z = C({
			width: F.value.style.chart.width,
			height: F.value.style.chart.height,
			paddingRatio: {
				top: F.value.style.chart.padding.top / F.value.style.chart.height,
				right: F.value.style.chart.padding.right / F.value.style.chart.width,
				bottom: F.value.style.chart.padding.bottom / F.value.style.chart.height,
				left: F.value.style.chart.padding.left / F.value.style.chart.width
			}
		}), Jt = p(() => ce(F.value.customPalette)), B = Le(null), V = Le(null), Yt = C(null), Xt = p(() => F.value.debug);
		function Zt() {
			if (ne(k.dataset) ? (se({
				componentName: "VueUiBump",
				type: "dataset",
				debug: Xt.value
			}), It.value = !0) : k.dataset.forEach((e, t) => {
				re({
					datasetObject: e,
					requiredAttributes: ["name", "values"]
				}).forEach((e) => {
					vt.value = !1, se({
						componentName: "VueUiBump",
						type: "datasetSerieAttribute",
						property: e,
						index: t,
						debug: Xt.value
					}), It.value = !0;
				});
			}), ne(k.dataset) || (It.value = F.value.loading), setTimeout(() => {
				wt.value = !0;
			}, 10), F.value.responsive) {
				let e = Se(() => {
					wt.value = !1;
					let { width: e, height: t } = Ce({
						chart: A.value,
						noTitle: xt.value,
						title: F.value.style.chart.title.text ? bt.value : null,
						legend: null,
						slicer: null,
						source: St.value
					});
					requestAnimationFrame(() => {
						z.value.width = e, z.value.height = t - 12, clearTimeout(Yt.value), Yt.value = setTimeout(() => {
							wt.value = !0;
						}, 10);
					});
				});
				B.value && (V.value && B.value.unobserve(V.value), B.value.disconnect()), B.value = new ResizeObserver(e), V.value = A.value.parentNode, B.value.observe(V.value);
			}
		}
		Pe(() => {
			B.value && (V.value && B.value.unobserve(V.value), B.value.disconnect());
		});
		let Qt = p(() => Ft.value.map((e, t) => {
			let n = c(e.color) || Jt.value[t] || s[t] || s[t % s.length];
			return {
				...e,
				absoluteIndex: t,
				id: ie(),
				color: n
			};
		})), H = p(() => Math.max(...Qt.value.map((e) => e.values.length)));
		function $t(e) {
			let t = e.map(() => Array(H.value).fill(null));
			for (let n = 0; n < H.value; n += 1) {
				let r = e.map((e, r) => ({
					seriesIndex: r,
					value: e.values?.[n],
					previousPosition: n > 0 ? t[r][n - 1] : r
				})).filter((e) => Number.isFinite(e.value));
				r.sort((e, t) => t.value === e.value ? e.previousPosition - t.previousPosition : t.value - e.value);
				for (let e = 0; e < r.length; e += 1) {
					let i = r[e];
					t[i.seriesIndex][n] = e;
				}
			}
			return t;
		}
		function en() {
			let e = 0;
			return kt.value && (e = Array.from(kt.value.querySelectorAll("tspan")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0)), e;
		}
		function tn() {
			let e = 0;
			return At.value && (e = Array.from(At.value.querySelectorAll("tspan")).reduce((e, t) => {
				let n = t.getComputedTextLength();
				return n > e ? n : e;
			}, 0)), e;
		}
		let nn = C(0), rn = Se((e) => {
			nn.value = e;
		}, 100);
		Be((e) => {
			let t = jt.value;
			if (!t) return;
			let n = new ResizeObserver((e) => {
				rn(e[0].contentRect.height);
			});
			n.observe(t), e(() => n.disconnect());
		}), Pe(() => {
			nn.value = 0;
		});
		let an = p(() => {
			let e = F.value.style.chart.layout.timeLabels;
			if (!e.show) return 0;
			let t = e.fontSize, n = e.rotation, r = Math.PI / 180 * n, i = K.value || [];
			if (!i.length) return 0;
			let a = i.reduce((e, t) => {
				let n = String(t?.text ?? "").split("\n").length;
				return Math.max(e, n);
			}, 1), o = i.reduce((e, t) => {
				let n = String(t?.text ?? "").split("\n").reduce((e, t) => Math.max(e, t.length), 0);
				return Math.max(e, n);
			}, 0), s = t * 1.3 * a, c = o * t * (o === 1 ? 1 : .6), l = Math.abs(Math.sin(r)) * c + Math.abs(Math.cos(r)) * s, u = t * .3;
			return Math.max(0, l + Math.abs(e.offsetY) + u);
		}), U = p(() => {
			let { height: e, width: t } = z.value, { right: n, left: r } = z.value.paddingRatio, i = en(), a = tn(), o = F.value.style.chart.padding.top, s = t - t * n - a, c = t * r + i, l = t - t * n - t * r - i - a, u = e - o - F.value.style.chart.padding.bottom - an.value, ee = o + u, te = o + u, ne = Math.max(0, e);
			return {
				chartHeight: te,
				chartWidth: Math.max(0, t),
				top: o,
				right: Math.max(0, s),
				bottom: Math.max(0, ee),
				left: Math.max(0, c),
				width: Math.max(0, l),
				height: Math.max(0, u),
				unitH: Math.max(0, u) / Qt.value.length,
				unitW: Math.max(0, l) / H.value,
				svgHeight: ne
			};
		}), W = p(() => {
			if (!vt.value && !Pt.value) return [];
			let e = $t(Qt.value);
			return Qt.value.map((t, n) => ({
				...t,
				positions: e[n]
			})).map((e) => {
				let t = e.positions.map((t, n) => {
					let r = te(F.value.style.chart.layout.plots.labels.formatter, e.values[n], u({
						p: F.value.style.chart.layout.plots.labels.prefix,
						v: e.values[n],
						s: F.value.style.chart.layout.plots.labels.suffix,
						r: F.value.style.chart.layout.plots.labels.rounding
					}));
					return {
						name: e.name,
						id: e.id,
						x: U.value.left + n * U.value.unitW + U.value.unitW / 2,
						y: U.value.top + t * U.value.unitH + U.value.unitH / 2,
						value: e.values[n],
						displayValue: r,
						rank: e.positions[n] + 1,
						color: e.color,
						labelColor: F.value.style.chart.layout.plots.labels.color === "auto" ? ae(e.color) : c(F.value.style.chart.layout.plots.labels.color) ?? ae(e.color)
					};
				}), n = t.filter((e) => d(e.value)), r = F.value.style.chart.layout.lines.smooth ? o(n) : l(n);
				return {
					...e,
					coordinates: t,
					path: r
				};
			});
		});
		p(() => W.value.flatMap((e, t) => e.coordinates.map((n, r) => ({
			...n,
			pointIndex: r,
			seriesIndex: t,
			seriesId: e.id,
			seriesName: e.name,
			pointId: `${e.id}_${r}`
		})).filter((e) => d(e.value))));
		function on(e, t, n, r) {
			let i = e.toSorted((e, t) => e.y - t.y);
			for (let e = 1; e < i.length; e += 1) {
				let n = i[e - 1], r = i[e];
				r.y < n.y + t && (r.y = n.y + t);
			}
			let a = i.length - 1;
			if (a >= 0 && i[a].y > r) {
				i[a].y = r;
				for (let e = a - 1; e >= 0; --e) {
					let n = i[e + 1], r = i[e];
					r.y > n.y - t && (r.y = n.y - t);
				}
			}
			if (i.length && i[0].y < n) {
				let e = n - i[0].y;
				for (let t = 0; t < i.length; t += 1) i[t].y += e;
			}
			return i;
		}
		let sn = p(() => {
			let e = [], t = [];
			W.value.forEach((n) => {
				let r = n.coordinates.filter((e) => Number.isFinite(e.rank));
				if (!r.length) return;
				e.push({ ...r[0] });
				let i = r[r.length - 1];
				t.push({ ...i });
			});
			let n = F.value.style.chart.layout.nameLabels.fontSize, r = n * 1.4, i = U.value.top + n, a = U.value.bottom - n, o = on(t, r, i, a);
			return {
				left: on(e, r, i, a),
				right: o
			};
		});
		function cn(e) {
			let t = F.value.style.chart.layout.plots.labels.fontSize * .4, n = e.displayValue, r = n.length * (F.value.style.chart.layout.plots.labels.fontSize * (n.length === 1 ? 1 : .6)), i = F.value.style.chart.layout.plots.labels.fontSize, a = r + t * 2, o = i + t * 2, s = e.x - a / 2, c = e.y - o / 2, l = F.value.style.chart.layout.plots.strokeWidth;
			return {
				x: s - l / 2,
				y: c - l / 2,
				width: a + l,
				height: o + l,
				fill: e.color,
				stroke: F.value.style.chart.layout.plots.stroke,
				"stroke-width": l,
				rx: o / 2
			};
		}
		let G = C([]), ln = 0;
		Be(() => {
			let e = ++ln, t = F.value.style.chart.layout.timeLabels, n = t.values, r = t.datetimeFormatter, i = H.value;
			if (!i || !Array.isArray(n) || n.length === 0) {
				G.value = [];
				return;
			}
			(async () => {
				let t = await de({
					values: n,
					maxDatapoints: i,
					formatter: r,
					start: 0,
					end: i
				});
				e === ln && (G.value = t);
			})();
		});
		let un = p(() => {
			let e = F.value.style.chart.layout.timeLabels.modulo;
			return G.value.length ? Math.min(e, [...new Set(G.value.map((e) => e.text))].length) : e;
		}), K = p(() => {
			let e = F.value.style.chart.layout.timeLabels, t = G.value || [], n = G.value || [], r = H.value, i = t.map((e) => e?.text ?? ""), a = n.map((e) => e?.text ?? "");
			return ee(!!e.showOnlyFirstAndLast, !!e.showOnlyAtModulo, Math.max(1, un.value || 1), i, a, 0, null, r);
		}), dn = p(() => z.value.width), fn = p(() => z.value.height), pn = p(() => ({
			start: 0,
			end: H.value
		})), q = p(() => [...W.value].map((e) => {
			let t = [...e.coordinates].filter((e) => d(e.value)).at(-1);
			return {
				...e,
				navigationRank: t?.rank ?? Infinity
			};
		}).sort((e, t) => e.navigationRank - t.navigationRank));
		function mn() {
			P.value = null, Mt.value = !0;
		}
		function hn() {
			_n(), Mt.value = !1;
		}
		function gn(e, t) {
			let n = q.value.length;
			return n ? e === null || e < 0 || e >= n ? 0 : t === "previous" ? (e - 1 + n) % n : (e + 1) % n : null;
		}
		function _n() {
			if (P.value !== null) {
				let e = q.value[P.value];
				e && $(e, e);
			}
			P.value = null, N.value = null;
		}
		function vn(e) {
			if (!L.value || Y.value || document.activeElement !== L.value || !q.value.length) return;
			let t = ["ArrowLeft", "ArrowUp"].includes(e.key), n = ["ArrowRight", "ArrowDown"].includes(e.key), r = e.key === "Enter" || e.key === " ", i = e.key === "Escape";
			if (!t && !n && !r && !i) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				_n();
				return;
			}
			if (r) {
				if (P.value === null) return;
				let e = q.value[P.value];
				if (!e) return;
				Mn(e, e);
				return;
			}
			let a = P.value;
			a === null ? a = 0 : t ? a = gn(a, "previous") : n && (a = gn(a, "next"));
			let o = q.value[a];
			o && (P.value = a, Q(o, o));
		}
		let J = p(() => ({
			head: Z.value.head,
			body: W.value.map((e) => [e.name, ...e.coordinates.map((e) => d(e.value) ? `${e.displayValue} (${e.rank})` : "-")]),
			caption: F.value.a11y.translations.tableCaption,
			notice: F.value.a11y.translations.tableAvailable
		}));
		ye({
			timeLabelsEls: jt,
			timeLabels: G,
			slicer: pn,
			configRef: F,
			rotationPath: [
				"style",
				"chart",
				"layout",
				"timeLabels",
				"rotation"
			],
			autoRotatePath: [
				"style",
				"chart",
				"layout",
				"timeLabels",
				"autoRotate",
				"enable"
			],
			isAutoSize: !1,
			width: dn,
			height: fn,
			rotation: F.value.style.chart.layout.timeLabels.autoRotate.angle
		});
		let Y = C(!1);
		function yn() {
			Y.value = !Y.value;
		}
		function bn(e) {
			Ct.value = e, yt.value += 1;
		}
		function xn() {
			R.value.showTable = !R.value.showTable;
		}
		ze(() => R.value.showTable, (e) => {
			F.value.table.show || (e && F.value.table.useDialog && M.value ? M.value.open() : "close" in M.value && M.value.close());
		});
		function Sn() {
			R.value.showTable = !1, Dt.value && Dt.value.setTableIconState(!1);
		}
		function Cn() {
			return W.value;
		}
		function wn(e = null) {
			let r = [
				[F.value.style.chart.title.text],
				[F.value.style.chart.title.subtitle.text],
				[""]
			], i = ["", ...K.value.map((e) => e?.text ?? "")], a = W.value.map((e) => [e.name, ...e.coordinates.map((e) => `${e.displayValue}`)]), o = W.value.map((e) => [e.name, ...e.coordinates.map((e) => `${e.rank}`)]), s = r.concat([[F.value.table.columnNames.values]]).concat([i]).concat(a).concat([
				[""],
				[F.value.table.columnNames.ranking],
				[i]
			]).concat(o), c = n(s);
			e ? e(c) : t({
				csvContent: c,
				title: F.value.style.chart.title.text || "vue-ui-bump"
			});
		}
		let X = p(() => {
			let e = F.value.table.useDialog && !F.value.table.show, t = R.value.showTable;
			return {
				component: e ? Oe : lt,
				title: `${F.value.style.chart.title.text}${F.value.style.chart.title.subtitle.text ? `: ${F.value.style.chart.title.subtitle.text}` : ""}`,
				props: e ? {
					backgroundColor: F.value.table.th.backgroundColor,
					color: F.value.table.th.color,
					headerColor: F.value.table.th.color,
					headerBg: F.value.table.th.backgroundColor,
					isFullscreen: Ct.value,
					fullscreenParent: A.value,
					forcedWidth: Math.min(800, window.innerWidth * .8),
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
		}), Z = p(() => ({
			head: [""].concat(K.value.map((e) => e.text)),
			body: W.value.map((e, t) => [e.name, ...e.coordinates.map((e) => `${e.displayValue} (${e.rank})`)]),
			config: {
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
			},
			colNames: [F.value.table.columnNames.series]
		})), Tn = p(() => F.value.style.chart.backgroundColor), En = p(() => F.value.style.chart.title), { isCallbackImaging: Dn, isCallbackSvg: On, generateSvg: kn, onGenerateImage: An } = ve({
			svg: L,
			title: En,
			legend: null,
			legendItems: null,
			backgroundColor: Tn,
			getSvgCallback: () => F.value.userOptions.callbacks.svg,
			generateImage: Kt
		});
		async function jn({ scale: e = 2 } = {}) {
			if (!A.value) return;
			let { width: t, height: n } = A.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await img({
				domElement: A.value,
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
		function Q(e, t) {
			N.value = t.id, F.value.events.datapointEnter && F.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: e?.pointIndex ?? null
			});
		}
		function $(e, t) {
			N.value = null, F.value.events.datapointLeave && F.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: e?.pointIndex ?? null
			});
		}
		function Mn(e, t) {
			F.value.events.datapointClick && F.value.events.datapointClick({
				datapoint: e,
				seriesIndex: e?.pointIndex ?? null
			});
		}
		async function Nn() {
			if (_t("copyAlt", {
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
		return xe({
			getData: Cn,
			getImage: jn,
			generatePdf: Gt,
			generateCsv: wn,
			generateImage: Kt,
			generateSvg: kn,
			toggleTable: xn,
			toggleAnnotator: yn,
			toggleFullscreen: bn,
			copyAlt: Nn
		}), (e, t) => (S(), g("div", {
			id: `bump_${j.value}`,
			ref_key: "bumpChart",
			ref: A,
			class: Ne({
				"vue-data-ui-component": !0,
				"vue-ui-bump": !0,
				"vue-data-ui-wrapper-fullscreen": Ct.value
			}),
			style: x(`background:${F.value.style.chart.backgroundColor};color:${F.value.style.chart.color};font-family:${F.value.style.fontFamily}; position: relative; ${F.value.responsive ? "height: 100%" : ""}`),
			onMouseenter: Vt,
			onMouseleave: Ht
		}, [
			_("p", {
				id: `chart-instructions-${j.value}`,
				class: "sr-only"
			}, E(F.value.a11y.translations.keyboardNavigation), 9, Ue),
			J.value.body.length ? (S(), m(we, {
				key: 0,
				uid: j.value,
				head: J.value.head,
				body: J.value.body,
				caption: J.value.caption,
				notice: J.value.notice
			}, null, 8, [
				"uid",
				"head",
				"body",
				"caption",
				"notice"
			])) : h("", !0),
			F.value.userOptions.buttons.annotator ? (S(), m(D(dt), {
				key: 1,
				svgRef: D(L),
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				active: Y.value,
				isCursorPointer: I.value,
				onClose: yn
			}, {
				"annotator-action-close": O(() => [T(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": O(({ color: t }) => [T(e.$slots, "annotator-action-color", b(y({ color: t })), void 0, !0)]),
				"annotator-action-draw": O(({ mode: t }) => [T(e.$slots, "annotator-action-draw", b(y({ mode: t })), void 0, !0)]),
				"annotator-action-undo": O(({ disabled: t }) => [T(e.$slots, "annotator-action-undo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": O(({ disabled: t }) => [T(e.$slots, "annotator-action-redo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": O(({ disabled: t }) => [T(e.$slots, "annotator-action-delete", b(y({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : h("", !0),
			T(e.$slots, "userConfig", {}, void 0, !0),
			qt.value ? (S(), g("div", {
				key: 2,
				ref_key: "noTitle",
				ref: xt,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : h("", !0),
			F.value.style.chart.title.text ? (S(), g("div", {
				key: 3,
				ref_key: "chartTitle",
				ref: bt,
				style: "width:100%;background:transparent;"
			}, [(S(), m(be, {
				key: `title_${Tt.value}`,
				config: {
					title: {
						cy: "bump-title",
						...F.value.style.chart.title
					},
					subtitle: {
						cy: "bump-subtitle",
						...F.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : h("", !0),
			F.value.userOptions.show && vt.value && (D(Bt) || D(Rt)) ? (S(), m(D(ft), {
				ref_key: "userOptionsRef",
				ref: Dt,
				key: `user_option_${yt.value}`,
				backgroundColor: F.value.style.chart.backgroundColor,
				color: F.value.style.chart.color,
				isPrinting: D(Ut),
				isImaging: D(Wt),
				uid: j.value,
				hasTooltip: !1,
				hasPdf: F.value.userOptions.buttons.pdf,
				hasImg: F.value.userOptions.buttons.img,
				hasSvg: F.value.userOptions.buttons.svg,
				hasXls: F.value.userOptions.buttons.csv,
				hasTable: F.value.userOptions.buttons.table,
				hasLabel: !1,
				hasAltCopy: F.value.userOptions.buttons.altCopy,
				hasFullscreen: F.value.userOptions.buttons.fullscreen,
				isFullcreen: Ct.value,
				chartElement: A.value,
				position: F.value.userOptions.position,
				titles: { ...F.value.userOptions.buttonTitles },
				hasAnnotator: F.value.userOptions.buttons.annotator,
				isAnnotation: Y.value,
				callbacks: F.value.userOptions.callbacks,
				printScale: F.value.userOptions.print.scale,
				tableDialog: F.value.table.useDialog,
				isCursorPointer: I.value,
				onToggleFullscreen: bn,
				onGeneratePdf: D(Gt),
				onGenerateCsv: wn,
				onGenerateImage: D(An),
				onGenerateSvg: D(kn),
				onToggleTable: xn,
				onToggleAnnotator: yn,
				onCopyAlt: Nn,
				style: x({ visibility: D(Bt) ? D(Rt) ? "visible" : "hidden" : "visible" })
			}, ke({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: O(({ isOpen: t, color: n }) => [T(e.$slots, "menuIcon", b(y({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: O(() => [T(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionCsv ? {
					name: "optionCsv",
					fn: O(() => [T(e.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: O(() => [T(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: O(() => [T(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionTable ? {
					name: "optionTable",
					fn: O(() => [T(e.$slots, "optionTable", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: O(({ toggleFullscreen: t, isFullscreen: n }) => [T(e.$slots, "optionFullscreen", b(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: O(({ toggleAnnotator: t, isAnnotator: n }) => [T(e.$slots, "optionAnnotator", b(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: O(({ altCopy: t }) => [T(e.$slots, "optionAltCopy", b(y({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: O(() => [T(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: O(() => [T(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasPdf.hasImg.hasSvg.hasXls.hasTable.hasAltCopy.hasFullscreen.isFullcreen.chartElement.position.titles.hasAnnotator.isAnnotation.callbacks.printScale.tableDialog.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : h("", !0),
			_("div", We, [(S(), g("svg", {
				ref_key: "svgRef",
				ref: L,
				xmlns: D(oe),
				"aria-describedby": `chart-instructions-${j.value}`,
				viewBox: `0 0 ${U.value.chartWidth <= 0 ? 10 : U.value.chartWidth} ${U.value.svgHeight <= 0 ? 10 : U.value.svgHeight}`,
				class: Ne({
					"vue-data-ui-loading": D(Pt),
					"no-transition": !F.value.useCssAnimation
				}),
				style: x(`max-width:100%;overflow:visible;background:transparent;color:${F.value.style.chart.color}`),
				role: "img",
				"aria-live": "polite",
				preserveAspectRatio: "xMidYMid",
				tabindex: "0",
				onFocus: mn,
				onBlur: hn,
				onKeydown: vn
			}, [
				je(D(pt)),
				e.$slots["chart-background"] ? (S(), g("foreignObject", {
					key: 0,
					x: U.value.left,
					y: U.value.top,
					width: U.value.width <= 0 ? 10 : U.value.width,
					height: U.value.height <= 0 ? 10 : U.value.height,
					style: { pointerEvents: "none" }
				}, [T(e.$slots, "chart-background", {}, void 0, !0)], 8, Ke)) : h("", !0),
				(S(!0), g(f, null, w(W.value, (e) => (S(), g(f, null, [_("path", {
					class: "transition-opacity",
					d: `M${e.path}`,
					stroke: F.value.style.chart.layout.lines.coatingColor,
					"stroke-width": F.value.style.chart.layout.lines.strokeWidth + 2,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					fill: "none",
					style: x({ opacity: N.value == null || N.value === e.id ? 1 : .1 }),
					onMouseenter: (t) => N.value = e.id,
					onMouseleave: t[0] ||= (e) => N.value = null
				}, null, 44, qe), _("path", {
					class: "transition-opacity",
					d: `M${e.path}`,
					stroke: e.color,
					"stroke-width": F.value.style.chart.layout.lines.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					fill: "none",
					style: x({ opacity: N.value == null || N.value === e.id ? 1 : .1 }),
					onMouseenter: (t) => Q(e, e),
					onMouseleave: (t) => $(e, e),
					onClick: (t) => Mn(e, e)
				}, null, 44, Je)], 64))), 256)),
				(S(!0), g(f, null, w(W.value, (t) => (S(), g(f, null, [
					F.value.style.chart.layout.plots.labels.displayedValue === "rank" ? (S(!0), g(f, { key: 0 }, w(t.coordinates, (e, n) => (S(), g("circle", {
						class: "transition-opacity",
						cx: e.x,
						cy: e.y,
						r: F.value.style.chart.layout.plots.radius,
						fill: t.color,
						stroke: F.value.style.chart.layout.plots.stroke,
						"stroke-width": F.value.style.chart.layout.plots.strokeWidth,
						style: x({ opacity: N.value == null || N.value === t.id ? 1 : .1 }),
						onMouseenter: (r) => Q({
							...e,
							pointIndex: n
						}, t),
						onMouseleave: (r) => $({
							...e,
							pointIndex: n
						}, t),
						onClick: (r) => Q({
							...e,
							pointIndex: n
						}, t)
					}, null, 44, Ye))), 256)) : h("", !0),
					F.value.style.chart.layout.plots.labels.show && F.value.style.chart.layout.plots.labels.displayedValue === "rank" ? (S(!0), g(f, { key: 1 }, w(t.coordinates, (e) => (S(), g("text", {
						class: "transition-opacity",
						x: e.x,
						y: e.y + F.value.style.chart.layout.plots.labels.fontSize / 3,
						fill: e.labelColor,
						"font-size": F.value.style.chart.layout.plots.labels.fontSize,
						"text-anchor": "middle",
						style: x({
							userSelect: "none",
							pointerEvents: "none",
							opacity: N.value == null || N.value === t.id ? 1 : .1
						})
					}, E(e.rank), 13, Xe))), 256)) : h("", !0),
					F.value.style.chart.layout.plots.labels.displayedValue === "value" ? (S(!0), g(f, { key: 2 }, w(t.coordinates, (n) => (S(), g(f, null, [D(d)(n.value) ? (S(), g("rect", Me({
						key: 0,
						class: "transition-opacity"
					}, { ref_for: !0 }, cn(n), {
						style: { opacity: N.value == null || N.value === t.id ? 1 : .1 },
						onMouseenter: (n) => Q({
							...e.point,
							pointIndex: e.i
						}, t),
						onMouseleave: (n) => $({
							...e.point,
							pointIndex: e.i
						}, t),
						onClick: (n) => Q({
							...e.point,
							pointIndex: e.i
						}, t)
					}), null, 16, Ze)) : h("", !0), D(d)(n.value) && F.value.style.chart.layout.plots.labels.show ? (S(), g("text", {
						key: 1,
						class: "transition-opacity",
						x: n.x,
						y: n.y + F.value.style.chart.layout.plots.labels.fontSize / 3,
						fill: n.labelColor,
						"font-size": F.value.style.chart.layout.plots.labels.fontSize,
						"text-anchor": "middle",
						style: x({
							userSelect: "none",
							pointerEvents: "none",
							opacity: N.value == null || N.value === t.id ? 1 : .1
						})
					}, E(n.displayValue), 13, Qe)) : h("", !0)], 64))), 256)) : h("", !0)
				], 64))), 256)),
				F.value.style.chart.layout.nameLabels.leftLabels.show ? (S(), g("g", {
					key: 1,
					ref_key: "labelsLeft",
					ref: kt
				}, [(S(!0), g(f, null, w(sn.value.left.filter((e) => D(d)(e.value)), (e, t) => (S(), g("text", {
					class: "transition-opacity",
					x: U.value.left,
					y: e.y + F.value.style.chart.layout.nameLabels.fontSize / 3,
					fill: F.value.style.chart.layout.nameLabels.useSerieColor ? e.color : F.value.style.chart.layout.nameLabels.color,
					"font-size": F.value.style.chart.layout.nameLabels.fontSize,
					"font--weight": F.value.style.chart.layout.nameLabels.bold ? "bold" : "normal",
					"text-anchor": "end",
					innerHTML: D(r)({
						content: e.name,
						fontSize: F.value.style.chart.layout.nameLabels.fontSize,
						fill: F.value.style.chart.layout.nameLabels.useSerieColor ? e.color : F.value.style.chart.layout.nameLabels.color,
						x: U.value.left - F.value.style.chart.layout.nameLabels.offsetX,
						y: e.y + F.value.style.chart.layout.nameLabels.fontSize / 3,
						translateY: !0
					}),
					style: x({ opacity: N.value == null || N.value === e.id ? 1 : .1 }),
					onMouseenter: (t) => Q(e, e),
					onMouseleave: (t) => $(e, e),
					onClick: (t) => Mn(e, e)
				}, null, 44, $e))), 256))], 512)) : h("", !0),
				F.value.style.chart.layout.nameLabels.rightLabels.show ? (S(), g("g", {
					key: 2,
					ref_key: "labelsRight",
					ref: At
				}, [(S(!0), g(f, null, w(sn.value.right.filter((e) => D(d)(e.value)), (e, t) => (S(), g("text", {
					class: "transition-opacity",
					x: U.value.right,
					y: e.y + F.value.style.chart.layout.nameLabels.fontSize / 3,
					fill: F.value.style.chart.layout.nameLabels.useSerieColor ? e.color : F.value.style.chart.layout.nameLabels.color,
					"font-size": F.value.style.chart.layout.nameLabels.fontSize,
					"font--weight": F.value.style.chart.layout.nameLabels.bold ? "bold" : "normal",
					"text-anchor": "start",
					innerHTML: D(r)({
						content: e.name,
						fontSize: F.value.style.chart.layout.nameLabels.fontSize,
						fill: F.value.style.chart.layout.nameLabels.useSerieColor ? e.color : F.value.style.chart.layout.nameLabels.color,
						x: U.value.right + F.value.style.chart.layout.nameLabels.offsetX,
						y: e.y + F.value.style.chart.layout.nameLabels.fontSize / 3,
						translateY: !0
					}),
					style: x({ opacity: N.value == null || N.value === e.id ? 1 : .1 }),
					onMouseenter: (t) => Q(e, e),
					onMouseleave: (t) => $(e, e),
					onClick: (t) => Mn(e, e)
				}, null, 44, et))), 256))], 512)) : h("", !0),
				F.value.style.chart.layout.timeLabels.show ? (S(), g("g", {
					key: 3,
					ref_key: "timeLabelsEls",
					ref: jt
				}, [e.$slots["time-label"] ? (S(), g("g", tt, [(S(!0), g(f, null, w(K.value, (t, n) => (S(), g("g", null, [T(e.$slots, "time-label", Me({ ref_for: !0 }, {
					x: U.value.unitW * n + U.value.unitW / 2 + U.value.left,
					y: U.value.chartHeight + F.value.style.chart.layout.timeLabels.offsetY,
					fontSize: F.value.style.chart.layout.timeLabels.fontSize,
					fill: F.value.style.chart.layout.timeLabels.color,
					transform: `translate(${U.value.unitW * n + U.value.unitW / 2 + U.value.left}, ${U.value.chartHeight + F.value.style.chart.layout.timeLabels.offsetY}), rotate(${F.value.style.chart.layout.timeLabels.rotation})`,
					absoluteIndex: t.absoluteIndex,
					content: t.text,
					textAnchor: F.value.style.chart.layout.timeLabels.rotation > 0 ? "start" : F.value.style.chart.layout.timeLabels.rotation < 0 ? "end" : "middle",
					show: !0
				}), void 0, !0)]))), 256))])) : (S(), g("g", nt, [(S(!0), g(f, null, w(K.value, (e, t) => (S(), g("g", null, [String(e.text).includes("\n") ? (S(), g("text", {
					key: t + "-multi",
					"text-anchor": F.value.style.chart.layout.timeLabels.rotation > 0 ? "start" : F.value.style.chart.layout.timeLabels.rotation < 0 ? "end" : "middle",
					"font-size": F.value.style.chart.layout.timeLabels.fontSize,
					fill: F.value.style.chart.layout.timeLabels.color,
					transform: `
                                        translate(
                                        ${U.value.unitW * t + U.value.unitW / 2 + U.value.left},
                                        ${U.value.chartHeight + F.value.style.chart.layout.timeLabels.fontSize * 1.3 + F.value.style.chart.layout.timeLabels.offsetY}
                                        ),
                                        rotate(${F.value.style.chart.layout.timeLabels.rotation})
                                    `,
					innerHTML: D(r)({
						content: String(e.text),
						fontSize: F.value.style.chart.layout.timeLabels.fontSize,
						fill: F.value.style.chart.layout.timeLabels.color,
						x: 0,
						y: 0
					})
				}, null, 8, it)) : (S(), g("text", {
					class: "vue-data-ui-time-label",
					key: t,
					"text-anchor": F.value.style.chart.layout.timeLabels.rotation > 0 ? "start" : F.value.style.chart.layout.timeLabels.rotation < 0 ? "end" : "middle",
					"font-size": F.value.style.chart.layout.timeLabels.fontSize,
					"font-weight": F.value.style.chart.layout.timeLabels.bold ? "bold" : "normal",
					fill: F.value.style.chart.layout.timeLabels.color,
					transform: `translate(${U.value.unitW * t + U.value.unitW / 2 + U.value.left}, ${U.value.chartHeight + F.value.style.chart.layout.timeLabels.offsetY}), rotate(${F.value.style.chart.layout.timeLabels.rotation})`
				}, E(e.text), 9, rt))]))), 256))]))], 512)) : h("", !0),
				T(e.$slots, "svg", { svg: {
					drawingArea: U.value,
					data: W.value,
					isPrintingImg: D(Ut) || D(Wt) || D(Dn),
					isPrintingSvg: D(On)
				} }, void 0, !0)
			], 46, Ge)), e.$slots.hint ? (S(), g("div", at, [T(e.$slots, "hint", b(y({
				hint: F.value.a11y.translations.keyboardNavigation,
				isVisible: Mt.value
			})), void 0, !0)])) : h("", !0)]),
			e.$slots.watermark ? (S(), g("div", ot, [T(e.$slots, "watermark", b(y({ isPrinting: D(Ut) || D(Wt) || D(Dn) || D(On) })), void 0, !0)])) : h("", !0),
			vt.value && F.value.userOptions.buttons.table ? (S(), m(Ie(X.value.component), Me({ key: 6 }, X.value.props, {
				ref_key: "tableUnit",
				ref: M,
				onClose: Sn
			}), ke({
				content: O(() => [je(D(ut), {
					colNames: Z.value.colNames,
					head: Z.value.head,
					body: Z.value.body,
					config: Z.value.config,
					title: F.value.table.useDialog ? "" : X.value.title,
					withCloseButton: !F.value.table.useDialog,
					isCursorPointer: I.value,
					onClose: Sn
				}, {
					th: O(({ th: e }) => [_("div", { innerHTML: e }, null, 8, st)]),
					td: O(({ td: e }) => [Ae(E(e), 1)]),
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
			}, [F.value.table.useDialog ? {
				name: "title",
				fn: O(() => [Ae(E(X.value.title), 1)]),
				key: "0"
			} : void 0, F.value.table.useDialog ? {
				name: "actions",
				fn: O(() => [_("button", {
					tabindex: "0",
					class: "vue-ui-user-options-button",
					onClick: t[1] ||= (e) => wn(F.value.userOptions.callbacks.csv),
					style: x({ cursor: I.value ? "pointer" : "default" })
				}, [je(D(ct), {
					name: "fileCsv",
					stroke: X.value.props.color
				}, null, 8, ["stroke"])], 4)]),
				key: "1"
			} : void 0]), 1040)) : h("", !0),
			e.$slots.source ? (S(), g("div", {
				key: 7,
				ref_key: "source",
				ref: St,
				dir: "auto"
			}, [T(e.$slots, "source", {}, void 0, !0)], 512)) : h("", !0),
			T(e.$slots, "skeleton", {}, () => [D(Pt) ? (S(), m(he, { key: 0 })) : h("", !0)], !0)
		], 46, He));
	}
}, [["__scopeId", "data-v-f5c72ff2"]]);
//#endregion
export { Ve as n, ct as t };
