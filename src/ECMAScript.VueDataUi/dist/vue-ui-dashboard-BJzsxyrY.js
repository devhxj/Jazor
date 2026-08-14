import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { q as t } from "./lib-Bttd6u5E.js";
import { n, t as r } from "./useHints-Dq_w2E8B.js";
import { t as i } from "./useConfig-DlNpz6P8.js";
import { t as a } from "./usePrinter-DN5bYhTG.js";
import { t as ee } from "./useNestedProp-vPNvh7rV.js";
import { t as o } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as te } from "./useUserOptionState-DK-_1ddE.js";
import { t as ne } from "./UserOptions-Dt98of5H.js";
import { n as re } from "./PenAndPaper-DAE-tnEQ.js";
import { Fragment as s, computed as c, createBlock as l, createCommentVNode as u, createElementBlock as d, createElementVNode as f, createSlots as ie, defineAsyncComponent as p, guardReactiveProps as m, mergeProps as ae, normalizeClass as oe, normalizeProps as h, normalizeStyle as g, openBlock as _, ref as v, renderList as se, renderSlot as y, resolveDynamicComponent as ce, unref as b, useCssVars as le, watch as x, withCtx as S } from "vue";
//#region src/components/vue-ui-dashboard.vue
var C = /* @__PURE__ */ e({ default: () => T }), ue = ["id"], de = ["onMousedown", "onTouchstart"], fe = ["onMousedown", "onTouchstart"], pe = ["onMousedown", "onTouchstart"], me = ["onMousedown", "onTouchstart"], he = ["onMousedown", "onTouchstart"], w = 1, T = /*#__PURE__*/ o({
	__name: "vue-ui-dashboard",
	props: {
		dataset: Array,
		config: Object
	},
	emits: ["change", "copyAlt"],
	setup(e, { expose: o, emit: C }) {
		le((e) => ({
			f1be45ec: Oe.value,
			v5f77e8e0: ke.value
		}));
		let T = {
			VueDataUi: p(() => import("./vue-data-ui-xlN6di_n.js").then((e) => e.n)),
			VueUi3dBar: p(() => import("./vue-ui-3d-bar-G3GsMDp6.js").then((e) => e.n)),
			VueUiAccordion: p(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)),
			VueUiAgePyramid: p(() => import("./vue-ui-age-pyramid-HW1Kiqib.js").then((e) => e.n)),
			VueUiAnnotator: p(() => import("./vue-ui-annotator-DfG79D4r.js").then((e) => e.n)),
			VueUiCandlestick: p(() => import("./vue-ui-candlestick-DvNtfrWd.js").then((e) => e.n)),
			VueUiChestnut: p(() => import("./vue-ui-chestnut-Cc2J1iOE.js").then((e) => e.n)),
			VueUiDigits: p(() => import("./vue-ui-digits-Uy498lu-.js").then((e) => e.n)),
			VueUiDonut: p(() => import("./vue-ui-donut-8RB-gL2J.js").then((e) => e.n)),
			VueUiDonutEvolution: p(() => import("./vue-ui-donut-evolution-CSYfyIbX.js").then((e) => e.n)),
			VueUiDumbbell: p(() => import("./vue-ui-dumbbell-8eNZFzyp.js").then((e) => e.n)),
			VueUiFlow: p(() => import("./vue-ui-flow-9VbVqIpA.js").then((e) => e.n)),
			VueUiGalaxy: p(() => import("./vue-ui-galaxy-cpdWAYuu.js").then((e) => e.n)),
			VueUiGauge: p(() => import("./vue-ui-gauge-CCA1unzt.js").then((e) => e.n)),
			VueUiHeatmap: p(() => import("./vue-ui-heatmap-DaqaYsGY.js").then((e) => e.n)),
			VueUiKpi: p(() => import("./vue-ui-kpi-_UO0otlH.js").then((e) => e.n)),
			VueUiMiniLoader: p(() => import("./vue-ui-mini-loader-CtTewZJG.js").then((e) => e.n)),
			VueUiMolecule: p(() => import("./vue-ui-molecule-D0s8EZSv.js").then((e) => e.n)),
			VueUiMoodRadar: p(() => import("./vue-ui-mood-radar-BnvrlH3J.js").then((e) => e.n)),
			VueUiNestedDonuts: p(() => import("./vue-ui-nested-donuts-CPkrAtLj.js").then((e) => e.n)),
			VueUiOnion: p(() => import("./vue-ui-onion-Cs-P6c1S.js").then((e) => e.n)),
			VueUiParallelCoordinatePlot: p(() => import("./vue-ui-parallel-coordinate-plot-1msZmPiM.js").then((e) => e.n)),
			VueUiQuadrant: p(() => import("./vue-ui-quadrant-H4BH2X1n.js").then((e) => e.n)),
			VueUiQuickChart: p(() => import("./vue-ui-quick-chart-Dn2OpR7I.js").then((e) => e.n)),
			VueUiRadar: p(() => import("./vue-ui-radar-B-0WKG9B.js").then((e) => e.n)),
			VueUiRating: p(() => import("./vue-ui-rating-BDVSVFA0.js").then((e) => e.n)),
			VueUiRelationCircle: p(() => import("./vue-ui-relation-circle-B9O3HmHR.js").then((e) => e.n)),
			VueUiRings: p(() => import("./vue-ui-rings-lgGvyEW_.js").then((e) => e.n)),
			VueUiScatter: p(() => import("./vue-ui-scatter-Dzdkyw1C.js").then((e) => e.n)),
			VueUiSkeleton: p(() => import("./vue-ui-skeleton-E6Hbh29Z.js").then((e) => e.n)),
			VueUiSmiley: p(() => import("./vue-ui-smiley-5iHJeTPc.js").then((e) => e.n)),
			VueUiSparkHistogram: p(() => import("./vue-ui-sparkhistogram-B_zDxZp5.js").then((e) => e.n)),
			VueUiSparkStackbar: p(() => import("./vue-ui-sparkstackbar-Bcipd2lT.js").then((e) => e.n)),
			VueUiSparkTrend: p(() => import("./vue-ui-spark-trend-BUQ1_7eU.js").then((e) => e.n)),
			VueUiSparkbar: p(() => import("./vue-ui-sparkbar-iyq8Toli.js").then((e) => e.n)),
			VueUiSparkgauge: p(() => import("./vue-ui-sparkgauge-CJXxmdWD.js").then((e) => e.n)),
			VueUiSparkline: p(() => import("./vue-ui-sparkline-jQ1WegfT.js").then((e) => e.n)),
			VueUiStripPlot: p(() => import("./vue-ui-strip-plot-CjIOca_Y.js").then((e) => e.n)),
			VueUiTable: p(() => import("./vue-ui-table-DzCJOvjR.js").then((e) => e.n)),
			VueUiTableHeatmap: p(() => import("./vue-ui-table-heatmap-D1bbPXKG.js").then((e) => e.n)),
			VueUiTableSparkline: p(() => import("./vue-ui-table-sparkline-Dc6HEQUQ.js").then((e) => e.n)),
			VueUiThermometer: p(() => import("./vue-ui-thermometer-DdrOgp8t.js").then((e) => e.n)),
			VueUiTimer: p(() => import("./vue-ui-timer-BpFDnonz.js").then((e) => e.n)),
			VueUiTiremarks: p(() => import("./vue-ui-tiremarks-B19X7xfh.js").then((e) => e.n)),
			VueUiTreemap: p(() => import("./vue-ui-treemap-Dd9pg6yW.js").then((e) => e.n)),
			VueUiVerticalBar: p(() => import("./vue-ui-horizontal-bar-DUMN2pwu.js").then((e) => e.n)),
			VueUiHorizontalBar: p(() => import("./vue-ui-horizontal-bar-DUMN2pwu.js").then((e) => e.n)),
			VueUiWaffle: p(() => import("./vue-ui-waffle-JmiCg9-q.js").then((e) => e.n)),
			VueUiWheel: p(() => import("./vue-ui-wheel-C4HHM1-P.js").then((e) => e.n)),
			VueUiWordCloud: p(() => import("./vue-ui-word-cloud-e86BT9TU.js").then((e) => e.n)),
			VueUiXy: p(() => import("./vue-ui-xy-ChUQgqEu.js").then((e) => e.n)),
			VueUiXyCanvas: p(() => import("./vue-ui-xy-canvas-1rbwMA1m.js").then((e) => e.n)),
			VueUiCarouselTable: p(() => import("./vue-ui-carousel-table-BQk8CkxQ.js").then((e) => e.n)),
			VueUiGizmo: p(() => import("./vue-ui-gizmo-BvShxRjx.js").then((e) => e.n)),
			VueUiStackbar: p(() => import("./vue-ui-stackbar-BH5oIVxw.js").then((e) => e.n)),
			VueUiStackline: p(() => import("./vue-ui-stackline-CAbvGgyL.js").then((e) => e.n)),
			VueUiBullet: p(() => import("./vue-ui-bullet-AWT4T1Yz.js").then((e) => e.n)),
			VueUiFunnel: p(() => import("./vue-ui-funnel-BJvc2n0l.js").then((e) => e.n)),
			VueUiHistoryPlot: p(() => import("./vue-ui-history-plot-S6MAWwzs.js").then((e) => e.n)),
			VueUiCirclePack: p(() => import("./vue-ui-circle-pack-Do8Bbh4z.js").then((e) => e.n)),
			VueUiWorld: p(() => import("./vue-ui-world-BYnB-6Gl.js").then((e) => e.n)),
			VueUiChord: p(() => import("./vue-ui-chord-j2Qbi0HA.js").then((e) => e.n)),
			VueUiRidgeline: p(() => import("./vue-ui-ridgeline-D_8sALB0.js").then((e) => e.n)),
			VueUiDag: p(() => import("./vue-ui-dag-0e_XFGOJ.js").then((e) => e.n))
		}, { vue_ui_dashboard: ge } = i(), _e = v(null), ve = v(null), E = v(null), D = e, O = c(() => ee({
			userConfig: D.config,
			defaultConfig: ge
		}));
		n({
			config: () => O.value,
			dataset: () => D.dataset,
			component: "VueUiDashboard",
			rules: [r.noHint]
		});
		let k = c(() => O.value.userOptions.useCursorPointer), A = v(t()), j = v(O.value.locked);
		function ye() {
			j.value = !j.value;
		}
		x(() => D.config, () => {
			j.value = O.value.locked, Q.value = !O.value.userOptions.showOnChartHover;
		});
		function M() {
			return D.dataset.map((e, t) => ({
				...e,
				index: t
			}));
		}
		let N = v(M());
		x(() => D.dataset, () => {
			N.value = M();
		});
		let P = c(() => N.value.map((e) => ({
			...e,
			resolvedComponent: typeof e.component == "string" ? T[e.component] : e.component
		}))), F = v(null), I = v(null), L = v({
			x: 0,
			y: 0
		}), R = v({
			x: 0,
			y: 0
		}), z = v(null), B = v(!1), V = v(null), H = v(!1);
		function U(e) {
			let t = e.target;
			(t.tagName === "INPUT" && t.type === "range" || t.classList.contains("range-handle")) && (H.value = !0);
		}
		function W(e) {
			let t = e.target;
			t.tagName === "INPUT" && t.type === "range" && (H.value = !1);
		}
		let { isPrinting: be, isImaging: xe, generatePdf: G, generateImage: Se } = a({
			elementId: `vue-ui-dashboard_${A.value}`,
			fileName: O.value.userOptions.print.filename || "dashboard",
			options: {
				...O.value.userOptions.print,
				aspectRatio: O.value.style.board.aspectRatio
			}
		});
		function Ce(e) {
			if (!j.value && (B.value = !0, V.value = e, I.value === null)) {
				F.value = e, L.value = {
					x: event.clientX,
					y: event.clientY
				};
				let t = N.value[e], n = 100 - t.width, r = 100 - t.height;
				t.left < 0 && (t.left = 0), t.top < 0 && (t.top = 0), t.left > n && (t.left = n), t.top > r && (t.top = r), t.left < 0 && (t.left = 0), t.top < 0 && (t.top = 0), t.left + t.width > 100 && (t.left = 100 - t.width), t.top + t.height > 100 && (t.top = 100 - t.height);
			}
		}
		function K(e, t) {
			B.value = !0, V.value = e, I.value = {
				index: e,
				direction: t
			};
			let n = N.value[e];
			R.value = {
				x: event.clientX,
				y: event.clientY,
				initialWidth: n.width,
				initialHeight: n.height
			};
		}
		function q(e, t, n) {
			if (I.value.direction.includes("top")) {
				let t = e.height - n / z.value.offsetHeight * 100;
				t >= w && (e.top += n / z.value.offsetHeight * 100, e.height = t);
			}
			if (I.value.direction.includes("bottom")) {
				let t = e.height + n / z.value.offsetHeight * 100;
				t >= w && (e.height = t);
			}
			if (I.value.direction.includes("left")) {
				let n = e.width - t / z.value.offsetWidth * 100;
				n >= w && (e.left += t / z.value.offsetWidth * 100, e.width = n);
			}
			if (I.value.direction.includes("right")) {
				let n = e.width + t / z.value.offsetWidth * 100;
				n >= w && (e.width = n);
			}
		}
		function we(e) {
			if (!(j.value || H.value)) {
				if (B.value = !0, F.value !== null) {
					let t = N.value[F.value], n = e.clientX - L.value.x, r = e.clientY - L.value.y, i = t.left + n / z.value.offsetWidth * 100, a = t.top + r / z.value.offsetHeight * 100;
					i >= 0 && a >= 0 && i + t.width <= 100 && a + t.height <= 100 && (t.left = i, t.top = a), L.value = {
						x: e.clientX,
						y: e.clientY
					};
				}
				if (I.value !== null) {
					let t = N.value[I.value.index];
					q(t, e.clientX - R.value.x, e.clientY - R.value.y), R.value = {
						x: e.clientX,
						y: e.clientY
					};
				}
			}
		}
		let J = C;
		x(N, (e) => {
			e && !B.value && J("change", N.value);
		}, { deep: !0 });
		function Te() {
			F.value = null, I.value = null, B.value = !1, V.value = null, N.value.forEach((e) => {
				e.left = Math.round(e.left / 100 * 100), e.top = Math.round(e.top / 100 * 100), e.width = Math.round(e.width / 100 * 100), e.height = Math.round(e.height / 100 * 100);
			});
		}
		function Ee(e) {
			j.value || H.value || (B.value = !0, V.value = e, I.value === null && (F.value = e, L.value = {
				x: event.touches[0].clientX,
				y: event.touches[0].clientY
			}));
		}
		function Y(e, t, n) {
			if (B.value = !0, V.value = e, I.value === null) {
				I.value = {
					index: e,
					direction: t
				};
				let r = N.value[e];
				R.value = {
					x: n.touches[0].clientX,
					y: n.touches[0].clientY,
					initialWidth: r.width,
					initialHeight: r.height
				};
			}
		}
		function X(e) {
			if (B.value = !0, e.preventDefault(), I.value !== null) {
				let t = N.value[I.value.index];
				q(t, e.touches[0].clientX - R.value.x, e.touches[0].clientY - R.value.y), R.value = {
					x: e.touches[0].clientX,
					y: e.touches[0].clientY
				};
			}
		}
		function De(e) {
			if (!(j.value || H.value) && (B.value = !0, e.preventDefault(), F.value !== null)) {
				let t = N.value[F.value], n = e.touches[0].clientX - L.value.x, r = e.touches[0].clientY - L.value.y, i = t.left + n / z.value.offsetWidth * 100, a = t.top + r / z.value.offsetHeight * 100;
				i >= 0 && a >= 0 && i + t.width <= 100 && a + t.height <= 100 && (t.left = i, t.top = a), L.value = {
					x: e.touches[0].clientX,
					y: e.touches[0].clientY
				};
			}
		}
		function Z() {
			B.value = !1, V.value = null, F.value = null, I.value = null, N.value.forEach((e) => {
				e.left = Math.round(e.left / 100 * 100), e.top = Math.round(e.top / 100 * 100), e.width = Math.round(e.width / 100 * 100), e.height = Math.round(e.height / 100 * 100);
			});
		}
		let Oe = c(() => O.value.style.item.borderColor), ke = c(() => O.value.style.resizeHandles.backgroundColor), Ae = c(() => O.value.style.board.aspectRatio), je = c(() => O.value.style.board.backgroundColor), Me = c(() => O.value.style.board.border);
		function Ne() {
			return N.value;
		}
		let { userOptionsVisible: Q, setUserOptionsVisibility: Pe, keepUserOptionState: Fe } = te({ config: O.value }), $ = v(!1);
		function Ie() {
			$.value = !$.value;
		}
		function Le() {
			Pe(!0);
		}
		function Re() {
			Pe(!1);
		}
		async function ze() {
			if (J("copyAlt", {
				config: O.value,
				dataset: P.value
			}), !O.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(O.value.userOptions.callbacks.altCopy({
				config: O.value,
				dataset: P.value
			}));
		}
		return o({
			generatePdf: G,
			getItemsPositions: Ne,
			toggleLock: ye,
			copyAlt: ze
		}), (e, t) => (_(), d("div", {
			id: `vue-ui-dashboard_${A.value}`,
			onMousedown: U,
			onMouseup: W,
			onTouchstart: U,
			onTouchend: W,
			onMouseenter: Le,
			onMouseleave: Re,
			ref_key: "dashboardRef",
			ref: _e,
			style: { position: "relative" },
			class: "vue-data-ui-component"
		}, [
			f("div", {
				class: "vue-ui-dashboard-container",
				ref_key: "dashboardContainer",
				ref: z,
				style: g(`outline:${Me.value}; background:${je.value}; aspect-ratio:${Ae.value};${$.value ? "pointer-events:none" : ""}`)
			}, [f("div", {
				class: "vue-ui-dashboard-grid-container",
				ref: "container",
				onMousemove: we,
				onMouseup: Te,
				onTouchmove: De,
				onTouchend: Z,
				style: g(`background:${O.value.style.board.backgroundColor}`)
			}, [t[4] ||= f("div", { class: "vue-ui-dashboard-grid" }, null, -1), (_(!0), d(s, null, se(P.value, (n, r) => (_(), d("div", {
				key: n.id,
				class: oe({
					"vue-ui-dashboard-grid-item": !0,
					"vue-ui-dashboard-grid-item--locked": j.value
				}),
				style: g({
					width: `${n.width}%`,
					height: `${n.height}%`,
					left: `${n.left}%`,
					top: `${n.top}%`,
					cursor: "move",
					boxShadow: V.value === r ? "0 6px 12px -3px rgba(0,0,0,0.3)" : "",
					zIndex: V.value === r ? P.value.length + 1 : n.index,
					backgroundColor: O.value.style.item.backgroundColor
				}),
				onMousedown: (e) => Ce(r),
				onTouchstart: (e) => Ee(r, n)
			}, [
				j.value ? u("", !0) : (_(), d(s, { key: 0 }, [
					f("div", {
						class: "vue-ui-dashboard-resize-handle vue-ui-dashboard-top-left",
						onMousedown: (e) => K(r, "top-left"),
						onTouchstart: (e) => Y(r, "top-left", e),
						onTouchmove: t[0] ||= (e) => X(e),
						onTouchend: Z
					}, null, 40, fe),
					f("div", {
						class: "vue-ui-dashboard-resize-handle vue-ui-dashboard-top-right",
						onMousedown: (e) => K(r, "top-right"),
						onTouchstart: (e) => Y(r, "top-right", e),
						onTouchmove: t[1] ||= (e) => X(e),
						onTouchend: Z
					}, null, 40, pe),
					f("div", {
						class: "vue-ui-dashboard-resize-handle vue-ui-dashboard-bottom-left",
						onMousedown: (e) => K(r, "bottom-left"),
						onTouchstart: (e) => Y(r, "bottom-left", e),
						onTouchmove: t[2] ||= (e) => X(e),
						onTouchend: Z
					}, null, 40, me),
					f("div", {
						class: "vue-ui-dashboard-resize-handle vue-ui-dashboard-bottom-right",
						onMousedown: (e) => K(r, "bottom-right"),
						onTouchstart: (e) => Y(r, "bottom-right", e),
						onTouchmove: t[3] ||= (e) => X(e),
						onTouchend: Z
					}, null, 40, he)
				], 64)),
				y(e.$slots, "top", {
					item: n,
					index: r
				}, void 0, !0),
				n.resolvedComponent ? (_(), l(ce(n.resolvedComponent), ae({
					key: 1,
					ref_for: !0
				}, n.props), null, 16)) : y(e.$slots, "content", {
					item: n,
					index: r,
					left: n.left,
					top: n.top,
					height: n.height,
					width: n.width
				}, void 0, !0, 2),
				y(e.$slots, "bottom", {
					item: n,
					index: r
				}, void 0, !0)
			], 46, de))), 128))], 36)], 4),
			(_(), d("svg", {
				style: g({
					width: "100%",
					height: "100%",
					pointerEvents: "none",
					position: "absolute",
					top: 0,
					left: 0,
					zIndex: P.value.length + 1
				}),
				ref_key: "svgRef",
				ref: E
			}, null, 4)),
			O.value.userOptions.buttons.annotator && z.value && E.value ? (_(), l(re, {
				key: 0,
				color: O.value.style.board.color,
				backgroundColor: O.value.style.board.backgroundColor,
				active: $.value,
				svgRef: E.value,
				isCursorPointer: k.value,
				onClose: Ie,
				style: g({ zIndex: P.value.length + 1 })
			}, {
				"annotator-action-close": S(() => [y(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": S(({ color: t }) => [y(e.$slots, "annotator-action-color", h(m({ color: t })), void 0, !0)]),
				"annotator-action-draw": S(({ mode: t }) => [y(e.$slots, "annotator-action-draw", h(m({ mode: t })), void 0, !0)]),
				"annotator-action-undo": S(({ disabled: t }) => [y(e.$slots, "annotator-action-undo", h(m({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": S(({ disabled: t }) => [y(e.$slots, "annotator-action-redo", h(m({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": S(({ disabled: t }) => [y(e.$slots, "annotator-action-delete", h(m({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"color",
				"backgroundColor",
				"active",
				"svgRef",
				"isCursorPointer",
				"style"
			])) : u("", !0),
			(O.value.allowPrint || O.value.userOptions.show) && (O.value.userOptions.buttons.pdf || O.value.userOptions.buttons.img) ? (_(), l(ne, {
				key: 1,
				ref_key: "userOptionsRef",
				ref: ve,
				backgroundColor: O.value.style.board.backgroundColor,
				color: O.value.style.board.color,
				isPrinting: b(be),
				isImaging: b(xe),
				uid: A.value,
				position: O.value.userOptions.position,
				hasTooltip: !1,
				hasPdf: O.value.userOptions.buttons.pdf,
				hasImg: O.value.userOptions.buttons.img,
				hasXls: !1,
				hasTable: !1,
				hasLabel: !1,
				hasFullscreen: !1,
				hasAltCopy: O.value.userOptions.buttons.altCopy,
				chartElement: z.value,
				callbacks: O.value.userOptions.callbacks,
				hasAnnotator: O.value.userOptions.buttons.annotator,
				isAnnotation: $.value,
				printScale: O.value.userOptions.print.scale,
				titles: { ...O.value.userOptions.buttonTitles },
				isCursorPointer: k.value,
				onGeneratePdf: b(G),
				onGenerateImage: b(Se),
				onToggleAnnotator: Ie,
				onCopyAlt: ze,
				style: g({
					visibility: b(Fe) ? b(Q) ? "visible" : "hidden" : "visible",
					zIndex: P.value.length + 1
				})
			}, ie({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: S(({ isOpen: t, color: n }) => [y(e.$slots, "menuIcon", h(m({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: S(() => [y(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: S(() => [y(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: S(({ toggleAnnotator: t, isAnnotator: n }) => [y(e.$slots, "optionAnnotator", h(m({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: S(({ altCopy: t }) => [y(e.$slots, "optionAltCopy", h(m({ altCopy: t })), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: S(() => [y(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: S(() => [y(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "6"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"position",
				"hasPdf",
				"hasImg",
				"hasAltCopy",
				"chartElement",
				"callbacks",
				"hasAnnotator",
				"isAnnotation",
				"printScale",
				"titles",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"style"
			])) : u("", !0)
		], 40, ue));
	}
}, [["__scopeId", "data-v-842ab9e8"]]);
//#endregion
export { C as n, T as t };
