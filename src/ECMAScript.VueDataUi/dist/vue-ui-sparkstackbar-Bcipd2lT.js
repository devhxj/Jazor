import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Bt as t, Jt as n, Kt as r, Pt as i, S as a, Vt as o, X as s, i as c, jt as ee, pt as te, q as ne, t as re, tt as ie, w as ae, xt as oe } from "./lib-Bttd6u5E.js";
import { n as se, t as ce } from "./useHints-Dq_w2E8B.js";
import { t as le } from "./useConfig-DlNpz6P8.js";
import { n as ue, t as de } from "./BaseScanner-DZvpgOjM.js";
import { t as l } from "./useNestedProp-vPNvh7rV.js";
import { t as fe } from "./useThemeCheck-C43Tcqmk.js";
import { t as pe } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as me } from "./DefGrad-DVBqDjhO.js";
import { t as he } from "./BaseLegendToggle-DZVucLnv.js";
import { t as ge } from "./A11yDataTable-DdRsVULz.js";
import { t as _e } from "./useChartAccessibility-DYqac8yF.js";
import { t as ve } from "./usePrefersMotion-BC-CsqR1.js";
import { t as ye } from "./vue_ui_sparkstackbar-BOjuQnZd.js";
import { Fragment as u, computed as d, createBlock as f, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createVNode as be, defineAsyncComponent as xe, guardReactiveProps as g, normalizeClass as _, normalizeProps as v, normalizeStyle as y, onMounted as Se, openBlock as b, ref as x, renderList as S, renderSlot as C, toDisplayString as w, toRefs as Ce, unref as T, useSlots as we, watch as Te, withCtx as Ee } from "vue";
//#region src/components/vue-ui-sparkstackbar.vue
var De = /* @__PURE__ */ e({ default: () => E }), Oe = ["id"], ke = { style: { position: "relative" } }, Ae = [
	"xmlns",
	"viewBox",
	"aria-describedby"
], je = {
	id: "stackPill",
	clipPathUnits: "objectBoundingBox"
}, Me = ["fill"], Ne = {
	key: 0,
	"clip-path": "url(#stackPill)"
}, Pe = [
	"x",
	"width",
	"height",
	"fill"
], Fe = [
	"x",
	"width",
	"height",
	"fill",
	"stroke"
], Ie = [
	"x",
	"width",
	"height",
	"onClick",
	"onMouseenter",
	"onMouseleave"
], Le = [
	"width",
	"height",
	"rx"
], Re = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, ze = [
	"aria-pressed",
	"aria-label",
	"onClick",
	"onFocus",
	"onKeydown"
], Be = { style: {
	display: "flex",
	"flex-direction": "row",
	"align-items": "center",
	gap: "4px",
	"justify-content": "center"
} }, Ve = ["height", "width"], He = ["fill"], Ue = {
	key: 3,
	ref: "source",
	dir: "auto"
}, E = /*#__PURE__*/ pe({
	__name: "vue-ui-sparkstackbar",
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
	emits: ["selectDatapoint", "selectLegend"],
	setup(e, { expose: pe, emit: De }) {
		let E = xe(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), We = xe(() => import("./Tooltip-DhjyfHwz.js")), { vue_ui_sparkstackbar: Ge } = le(), { isThemeValid: Ke, warnInvalidTheme: qe } = fe(), Je = ve(), Ye = we(), D = e, Xe = x(null), O = x(ne()), k = x(!1), A = x(""), j = x(null), M = x(!1), N = x(null), Ze = x({
			x: 0,
			y: 0
		}), P = x("pointer"), F = x(!1), I = x(null), Qe = x([]), L = x(H()), R = d(() => L.value.debug);
		Se(() => {
			Ye["chart-background"] && R.value && console.warn("VueUiSparkStackbar does not support the #chart-background slot.");
		}), se({
			config: () => L.value,
			dataset: () => D.dataset,
			component: "VueUiSparkStackbar",
			rules: [ce.emptyArray, {
				test: (e) => e.length > 6,
				message: [
					"👀 The number of series is > 6. Consider:",
					"",
					"▶️ Grouping small values dynamically into a single \"Other\" series.",
					"",
					"▶️ Using VueUiHorizontalBar instead to make the dataset breakdown easier to read."
				]
			}]
		});
		let $e = d(() => L.value.useCursorPointer), et = d(() => n({
			defaultConfig: { style: {
				backgroundColor: "#99999930",
				animation: { show: !1 },
				bar: { gradient: { inderlayerColor: "#6A6A6A" } },
				title: { backgroundColor: "transparent" }
			} },
			userConfig: L.value.skeletonConfig ?? {}
		})), { loading: z, FINAL_DATASET: B } = ue({
			...Ce(D),
			FINAL_CONFIG: L,
			prepareConfig: H,
			skeletonDataset: D.config?.skeletonDataset ?? [
				{
					name: "_",
					value: 8,
					color: "#808080"
				},
				{
					name: "_",
					value: 5,
					color: "#ADADAD"
				},
				{
					name: "_",
					value: 3,
					color: "#DBDBDB"
				}
			],
			skeletonConfig: n({
				defaultConfig: L.value,
				userConfig: et.value
			})
		}), { svgRef: V } = _e({ config: L.value.style.title });
		function H() {
			let e = l({
				userConfig: D.config,
				defaultConfig: Ge
			}), t = e.theme;
			if (!t) return e;
			if (!Ke.value(e)) return qe(e), e;
			let n = l({
				userConfig: ye[t] || D.config,
				defaultConfig: e
			}), a = l({
				userConfig: D.config,
				defaultConfig: n
			});
			return {
				...a,
				customPalette: a.customPalette.length ? a.customPalette : r[t] || i
			};
		}
		Te(() => D.config, (e) => {
			L.value = H(), nt();
		}, { deep: !0 }), Te(() => B.value, (e) => {
			W.value = B.value.map((e, t) => ({
				...e,
				color: e.color ? a(e.color) : U.value[t] || i[t] || i[t % i.length]
			})), tt();
		}, { deep: !0 });
		let U = d(() => ae(L.value.customPalette)), W = x(B.value.map((e, t) => ({
			...e,
			value: L.value.style.animation.show ? 0 : e.value || 0,
			color: e.color ? a(e.color) : U.value[t] || i[t] || i[t % i.length]
		}))), G = x(!0);
		function tt() {
			if (!L.value.style.animation.show || Je.value) {
				G.value = !1;
				return;
			}
			let e = L.value.style.animation.animationFrames, t = B.value.map((e) => e.value || 0), n = t.map((t) => t / e), r = t.reduce((e, t) => e + t, 0), o = 0;
			G.value = !0, W.value = B.value.map((e, t) => ({
				...e,
				value: 0,
				color: e.color ? a(e.color) : U.value[t] || i[t] || i[t % i.length]
			}));
			function s() {
				o += r / e, o < r ? (W.value = W.value.map((e, r) => ({
					...e,
					value: Math.min(e.value + n[r], t[r]),
					color: e.color ? a(e.color) : U.value[r] || i[r] || i[r % i.length]
				})), requestAnimationFrame(s)) : (G.value = !1, W.value = B.value.map((e, n) => ({
					...e,
					value: t[n],
					color: e.color ? a(e.color) : U.value[n] || i[n] || i[n % i.length],
					id: ne()
				})));
			}
			s();
		}
		Se(() => {
			nt();
		});
		function nt() {
			ee(D.dataset) ? ie({
				componentName: "VueUiSparkStackbar",
				type: "dataset",
				debug: R.value
			}) : R.value && D.dataset.forEach((e, t) => {
				te({
					datasetObject: e,
					requiredAttributes: ["name", "value"]
				}).forEach((e) => {
					ie({
						componentName: "VueUiSparkStackbar",
						type: "datasetSerieAttribute",
						property: e,
						index: t
					});
				});
			}), tt();
		}
		let K = x({
			width: 500,
			height: 16
		}), q = x([]), rt = d(() => B.value.map((e) => e.value || 0).filter((e, t) => !q.value.includes(t)).reduce((e, t) => e + t, 0)), J = d(() => W.value.map((e, t) => {
			let n = e.value || 0, r = n / rt.value, o = isNaN(r) ? 0 : r, c = o * K.value.width;
			return {
				...e,
				color: a(B.value[t]?.color ? B.value[t]?.color : U.value[t] || i[t] || i[t % i.length]),
				value: n,
				proportion: o,
				width: c,
				seriesIndex: t,
				proportionLabel: s({
					v: o * 100,
					s: "%",
					r: L.value.style.legend.percentage.rounding
				})
			};
		})), Y = d(() => J.value.filter((e, t) => !q.value.includes(t)));
		function it() {
			q.value.length ? q.value = [] : J.value.forEach((e, t) => {
				q.value.push(t);
			}), ct("selectLegend", Y.value);
		}
		function X(e) {
			q.value.includes(e) ? q.value = q.value.filter((t) => t !== e) : q.value.length < W.value.length - 1 && q.value.push(e), ct("selectLegend", Y.value);
		}
		function at(e) {
			return J.value.length ? J.value.find((t) => t.name === e) || (L.value.debug && console.warn(`VueUiSparkStackbar - Series name not found "${e}"`), null) : (L.value.debug && console.warn("VueUiSparkStackbar - There are no series to show."), null);
		}
		function ot(e) {
			let t = at(e);
			t !== null && q.value.includes(t.id) && X(t.seriesIndex);
		}
		function st(e) {
			let t = at(e);
			t !== null && (q.value.includes(t.id) || X(t.seriesIndex));
		}
		let Z = d(() => {
			let e = 0, t = [];
			for (let n = 0; n < Y.value.length; n += 1) t.push({
				...Y.value[n],
				start: e
			}), e += Y.value[n].width;
			return t;
		}), ct = De;
		function Q(e, t, n = !1) {
			ct("selectDatapoint", {
				datapoint: e,
				index: t
			}), L.value.events.datapointClick && !n && L.value.events.datapointClick({
				datapoint: e,
				seriesIndex: e.seriesIndex
			});
		}
		function lt({ datapoint: e, seriesIndex: t }) {
			k.value = !1, I.value = null, N.value = null, P.value = "pointer", L.value.events.datapointLeave && L.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: e.seriesIndex
			});
		}
		function ut({ datapoint: e, seriesIndex: t, triggerMode: n = "pointer" }) {
			if (M.value = !1, L.value.events.datapointEnter && L.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: e.seriesIndex
			}), !L.value.style.tooltip.show) return;
			P.value = n, j.value = {
				datapoint: e,
				seriesIndex: t,
				config: L.value,
				series: J.value
			}, k.value = !0, I.value = t;
			let r = L.value.style.tooltip.customFormat;
			if (oe(r)) try {
				let t = r({
					seriesIndex: e.seriesIndex,
					datapoint: e,
					series: J.value,
					config: L.value
				});
				typeof t == "string" && (A.value = t, M.value = !0);
			} catch {
				console.warn("Custom format cannot be applied."), M.value = !1;
			}
			if (!M.value) {
				let n = "";
				n += `<div style="width:100%;text-align:center;border-bottom:1px solid ${L.value.style.tooltip.borderColor};padding-bottom:6px;margin-bottom:3px;">${e.name}</div>`, n += `<div style="display:flex;flex-direction:row;gap:6px;align-items:center;"><svg viewBox="0 0 12 12" height="14" width="14"><circle cx="6" cy="6" r="6" stroke="none" fill="${e.color}"/></svg>`, n += `<b>${e.proportionLabel}</b>`, n += `<span>(${c(L.value.style.legend.value.formatter, e.value, s({
					p: L.value.style.legend.value.prefix,
					v: e.value,
					s: L.value.style.legend.value.suffix,
					r: L.value.style.legend.value.rounding
				}), {
					datapoint: e,
					seriesIndex: t
				})})</span>`, A.value = `<div>${n}</div>`;
			}
		}
		function $(e) {
			let t = Qe.value[e];
			t && typeof t.focus == "function" && t.focus();
		}
		function dt(e) {
			if (!Number.isFinite(e) || !V.value) return;
			let t = Z.value[e];
			if (!t) return;
			let n = V.value.getBoundingClientRect(), r = t.start + t.width / 2, i = K.value.height / 2;
			Ze.value = {
				x: n.left + r / K.value.width * n.width,
				y: n.top + i / K.value.height * n.height
			};
		}
		function ft() {
			N.value = null, F.value = !0;
		}
		function pt() {
			N.value = null, P.value = "pointer", k.value = !1, I.value = null, F.value = !1;
		}
		function mt(e) {
			if (!V.value || document.activeElement !== V.value || !Z.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "Enter" || e.key === " ", i = e.key === "Escape", a = e.key === "Home", o = e.key === "End";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), i) {
				N.value = null, P.value = "pointer", k.value = !1, I.value = null;
				return;
			}
			if (r) {
				if (N.value === null) return;
				let e = Z.value[N.value];
				if (!e) return;
				Q(e, e.seriesIndex);
				return;
			}
			let s = N.value;
			a ? s = 0 : o ? s = Z.value.length - 1 : s === null || s < 0 || s >= Z.value.length ? s = n ? 0 : Z.value.length - 1 : n ? (s += 1, s >= Z.value.length && (s = 0)) : t && (--s, s < 0 && (s = Z.value.length - 1));
			let c = Z.value[s];
			c && (N.value = s, dt(s), ut({
				datapoint: c,
				seriesIndex: c.seriesIndex,
				triggerMode: "keyboard"
			}));
		}
		function ht(e) {
			k.value = !1, N.value = null, P.value = "pointer", I.value = e.seriesIndex;
		}
		function gt() {
			N.value = null, P.value = "pointer", k.value = !1, I.value = null;
		}
		function _t(e, t, n) {
			let r = e.key === "Enter" || e.key === " ", i = e.key === "ArrowLeft" || e.key === "ArrowUp", a = e.key === "ArrowRight" || e.key === "ArrowDown", o = e.key === "Home", s = e.key === "End", c = e.key === "Escape";
			if (!(!r && !i && !a && !o && !s && !c)) {
				if (e.preventDefault(), e.stopPropagation(), c) {
					gt();
					return;
				}
				if (r) {
					X(n), Q(t, n, !0);
					return;
				}
				if (o) {
					$(0);
					return;
				}
				if (s) {
					$(J.value.length - 1);
					return;
				}
				if (i) {
					$(n <= 0 ? J.value.length - 1 : n - 1);
					return;
				}
				a && $(n >= J.value.length - 1 ? 0 : n + 1);
			}
		}
		let vt = d(() => ({
			headers: [
				L.value.a11y.translations.series,
				L.value.a11y.translations.percentage,
				L.value.a11y.translations.value
			],
			rows: J.value.map((e, t) => [
				e.name,
				q.value.includes(t) ? " - " : e.proportionLabel,
				c(L.value.style.legend.value.formatter, e.value, s({
					p: L.value.style.legend.value.prefix,
					v: e.value,
					s: L.value.style.legend.value.suffix,
					r: L.value.style.legend.value.rounding
				}), {
					datapoint: e,
					seriesIndex: t
				})
			])
		}));
		return pe({
			hideSeries: st,
			showSeries: ot
		}), (e, n) => (b(), m("div", {
			class: "vue-data-ui-component vue-ui-spark-stackbar",
			ref_key: "sparkstackbarChart",
			ref: Xe,
			style: y(`width:100%; background:${L.value.style.backgroundColor}`)
		}, [
			h("div", {
				id: `chart-instructions-${O.value}`,
				class: "sr-only"
			}, [h("p", null, w(L.value.a11y.translations.keyboardNavigation), 1)], 8, Oe),
			vt.value?.rows?.length ? (b(), f(ge, {
				key: 0,
				uid: O.value,
				head: vt.value.headers,
				body: vt.value.rows,
				notice: L.value.a11y.translations.tableAvailable,
				caption: L.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : p("", !0),
			L.value.style.title.text ? (b(), m("div", {
				key: 1,
				style: y(`width:calc(100% - 12px);background:transparent;margin:0 auto;margin:${L.value.style.title.margin};padding: 0 6px;text-align:${L.value.style.title.textAlign}`)
			}, [h("div", {
				class: "atom-title",
				style: y(`font-size:${L.value.style.title.fontSize}px;color:${L.value.style.title.color};font-weight:${L.value.style.title.bold ? "bold" : "normal"}`)
			}, w(L.value.style.title.text), 5), L.value.style.title.subtitle.text ? (b(), m("div", {
				key: 0,
				class: "atom-subtitle",
				style: y(`font-size:${L.value.style.title.subtitle.fontSize}px;color:${L.value.style.title.subtitle.color};font-weight:${L.value.style.title.subtitle.bold ? "bold" : "normal"}`)
			}, w(L.value.style.title.subtitle.text), 5)) : p("", !0)], 4)) : p("", !0),
			h("div", ke, [(b(), m("svg", {
				ref_key: "svgRef",
				ref: V,
				xmlns: T(re),
				width: "100%",
				viewBox: `0 0 ${K.value.width} ${K.value.height}`,
				"aria-describedby": `chart-instructions-${O.value}`,
				tabindex: "0",
				onFocus: ft,
				onBlur: pt,
				onKeydown: mt
			}, [
				be(T(E)),
				h("defs", null, [(b(!0), m(u, null, S(Z.value, (e, n) => (b(), f(me, {
					t: "linear",
					id: `stack_gradient_${n}_${O.value}`,
					key: `stack_gradient_${n}`,
					gradientTransform: "rotate(90)",
					stops: [
						[
							"0%",
							e.color,
							1
						],
						[
							"50%",
							T(t)(T(o)(e.color, .05), 100 - L.value.style.bar.gradient.intensity),
							1
						],
						[
							"100%",
							e.color,
							1
						]
					]
				}, null, 8, ["id", "stops"]))), 128)), h("clipPath", je, [h("rect", {
					x: "0.005",
					y: "-2",
					width: "0.99",
					height: "5",
					rx: "3",
					ry: "3",
					fill: L.value.style.backgroundColor
				}, null, 8, Me)])]),
				rt.value > 0 ? (b(), m("g", Ne, [
					(b(!0), m(u, null, S(Z.value, (e, t) => (b(), m("rect", {
						key: `stack_underlayer_${t}`,
						x: e.start,
						y: 0,
						width: e.width,
						height: K.value.height,
						fill: L.value.style.bar.gradient.underlayerColor,
						class: _({ animated: !G.value && !T(z) }),
						style: y({ opacity: I.value !== null && L.value.style.tooltip.show ? I.value === t ? 1 : .5 : 1 })
					}, null, 14, Pe))), 128)),
					(b(!0), m(u, null, S(Z.value, (e, t) => (b(), m("rect", {
						key: `stack_${t}`,
						x: e.start,
						y: 0,
						width: e.width,
						height: K.value.height,
						fill: L.value.style.bar.gradient.show ? `url(#stack_gradient_${t}_${O.value})` : e.color,
						stroke: L.value.style.backgroundColor,
						"stroke-linecap": "round",
						class: _({ animated: !G.value && !T(z) }),
						style: y({ opacity: I.value !== null && L.value.style.tooltip.show ? I.value === t ? 1 : .5 : 1 })
					}, null, 14, Fe))), 128)),
					(b(!0), m(u, null, S(Z.value, (e, t) => (b(), m("rect", {
						key: `stack_trap_${t}`,
						x: e.start,
						y: 0,
						width: e.width,
						height: K.value.height,
						fill: "transparent",
						stroke: "none",
						class: _({ animated: !G.value && !T(z) }),
						onClick: () => Q(e, t),
						onMouseenter: () => ut({
							datapoint: e,
							seriesIndex: t
						}),
						onMouseleave: (n) => lt({
							datapoint: e,
							seriesIndex: t
						})
					}, null, 42, Ie))), 128))
				])) : (b(), m("rect", {
					key: 1,
					x: 2,
					y: 1,
					width: K.value.width - 4,
					height: K.value.height - 2,
					stroke: "#CCCCCC",
					"stroke-width": "2",
					fill: "transparent",
					rx: (K.value.height - 4) / 2
				}, null, 8, Le))
			], 40, Ae)), e.$slots.hint ? (b(), m("div", Re, [C(e.$slots, "hint", v(g({
				hint: L.value.a11y.translations.keyboardNavigation,
				isVisible: F.value
			})), void 0, !0)])) : p("", !0)]),
			L.value.style.legend.show ? (b(), m("div", {
				key: 2,
				style: y(`background:transparent;margin:0 auto;margin:${L.value.style.legend.margin};justify-content:${L.value.style.legend.textAlign === "left" ? "flex-start" : L.value.style.legend.textAlign === "right" ? "flex-end" : "center"}`),
				class: "vue-ui-sparkstackbar-legend",
				"aria-label": "legend",
				role: "toolbar"
			}, [L.value.style.legend.selectAllToggle.show && J.value.length > 2 && !T(z) ? (b(), f(he, {
				key: 0,
				backgroundColor: L.value.style.legend.selectAllToggle.backgroundColor,
				color: L.value.style.legend.selectAllToggle.color,
				fontSize: L.value.style.legend.fontSize,
				checked: q.value.length > 0,
				isCursorPointer: $e.value,
				onToggle: it
			}, null, 8, [
				"backgroundColor",
				"color",
				"fontSize",
				"checked",
				"isCursorPointer"
			])) : p("", !0), (b(!0), m(u, null, S(J.value, (e, n) => (b(), m("div", {
				role: "button",
				tabindex: "0",
				"aria-pressed": q.value.includes(n),
				"aria-label": `${e.name}, ${q.value.includes(n) ? "hidden" : "visible"}, ${e.proportionLabel}`,
				style: y(`font-size:${L.value.style.legend.fontSize}px;cursor:${$e.value ? "pointer" : "default"}`),
				class: _({
					"vue-ui-sparkstackbar-legend-item": !0,
					"vue-ui-sparkstackbar-legend-item-unselected": q.value.includes(n)
				}),
				onClick: (t) => {
					X(n), Q(e, n, !0);
				},
				onFocus: (t) => ht(e),
				onBlur: gt,
				onKeydown: (t) => _t(t, e, n)
			}, [h("div", Be, [(b(), m("svg", {
				height: `${L.value.style.legend.fontSize}px`,
				width: `${L.value.style.legend.fontSize}px`,
				viewBox: "0 0 10 10"
			}, [h("defs", null, [be(me, {
				t: "radial",
				id: `legend_grad_${n}-${O.value}`,
				stops: [[
					"0%",
					T(z) ? "#FFFFFF" : T(t)(T(o)(e.color, .05), 100 - L.value.style.bar.gradient.intensity),
					1
				], [
					"100%",
					e.color,
					1
				]]
			}, null, 8, ["id", "stops"])]), h("circle", {
				cx: 5,
				cy: 5,
				r: 5,
				fill: L.value.style.bar.gradient.show ? `url(#legend_grad_${n}-${O.value})` : e.color
			}, null, 8, He)], 8, Ve)), T(z) ? p("", !0) : (b(), m(u, { key: 0 }, [h("span", { style: y(`color:${L.value.style.legend.name.color}; font-weight:${L.value.style.legend.name.bold ? "bold" : "normal"}`) }, w(e.name), 5), G.value ? p("", !0) : (b(), m(u, { key: 0 }, [L.value.style.legend.percentage.show ? (b(), m("span", {
				key: 0,
				style: y(`font-weight:${L.value.style.legend.percentage.bold ? "bold" : "normal"};color:${L.value.style.legend.percentage.color}`)
			}, w(q.value.includes(n) ? " - " : e.proportionLabel), 5)) : p("", !0), L.value.style.legend.value.show ? (b(), m("span", {
				key: 1,
				style: y(`font-weight:${L.value.style.legend.value.bold ? "bold" : "normal"};color:${L.value.style.legend.value.color}`)
			}, " (" + w(T(c)(L.value.style.legend.value.formatter, e.value, T(s)({
				p: L.value.style.legend.value.prefix,
				v: e.value,
				s: L.value.style.legend.value.suffix,
				r: L.value.style.legend.value.rounding
			}), {
				datapoint: e,
				seriesIndex: n
			})) + ") ", 5)) : p("", !0)], 64))], 64))])], 46, ze))), 256))], 4)) : p("", !0),
			be(T(We), {
				teleportTo: L.value.style.tooltip.teleportTo,
				show: k.value && L.value.style.tooltip.show,
				parent: Xe.value,
				backgroundColor: L.value.style.tooltip.backgroundColor,
				color: L.value.style.tooltip.color,
				fontSize: L.value.style.tooltip.fontSize,
				borderRadius: L.value.style.tooltip.borderRadius,
				borderColor: L.value.style.tooltip.borderColor,
				borderWidth: L.value.style.tooltip.borderWidth,
				backgroundOpacity: L.value.style.tooltip.backgroundOpacity,
				position: L.value.style.tooltip.position,
				content: A.value,
				isCustom: M.value,
				offsetX: L.value.style.tooltip.offsetX,
				offsetY: -124 + L.value.style.tooltip.offsetY,
				blockShiftY: !0,
				smooth: L.value.style.tooltip.smooth,
				backdropFilter: L.value.style.tooltip.backdropFilter,
				smoothForce: L.value.style.tooltip.smoothForce,
				smoothSnapThreshold: L.value.style.tooltip.smoothSnapThreshold,
				isA11yMode: P.value === "keyboard",
				a11yPosition: Ze.value
			}, {
				"tooltip-before": Ee(() => [C(e.$slots, "tooltip-before", v(g({ ...j.value })), void 0, !0)]),
				tooltip: Ee(() => [C(e.$slots, "tooltip", v(g({ ...j.value })), void 0, !0)]),
				"tooltip-after": Ee(() => [C(e.$slots, "tooltip-after", v(g({ ...j.value })), void 0, !0)]),
				_: 3
			}, 8, [
				"teleportTo",
				"show",
				"parent",
				"backgroundColor",
				"color",
				"fontSize",
				"borderRadius",
				"borderColor",
				"borderWidth",
				"backgroundOpacity",
				"position",
				"content",
				"isCustom",
				"offsetX",
				"offsetY",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			e.$slots.source ? (b(), m("div", Ue, [C(e.$slots, "source", {}, void 0, !0)], 512)) : p("", !0),
			C(e.$slots, "skeleton", {}, () => [T(z) ? (b(), f(de, { key: 0 })) : p("", !0)], !0)
		], 4));
	}
}, [["__scopeId", "data-v-46bfe738"]]);
//#endregion
export { De as n, E as t };
