import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { t } from "./BaseIcon-BfndwIWE.js";
import { computed as n, createBlock as r, createCommentVNode as i, createElementBlock as a, createElementVNode as ee, createSlots as te, createTextVNode as ne, defineAsyncComponent as o, guardReactiveProps as re, mergeProps as ie, normalizeProps as ae, normalizeStyle as oe, onMounted as se, openBlock as s, ref as c, renderList as ce, renderSlot as le, resolveDynamicComponent as ue, shallowReactive as de, toDisplayString as fe, toHandlers as pe, toRefs as me, watch as l, withCtx as he } from "vue";
//#region src/components/vue-data-ui.vue
var u = /* @__PURE__ */ e({ default: () => d }), ge = { style: { width: "36px" } }, d = {
	__name: "vue-data-ui",
	props: {
		component: { type: String },
		dataset: { type: [
			Object,
			Array,
			Number,
			String
		] },
		config: { type: Object }
	},
	emits: /* @__PURE__ */ "cancel.change.copyAlt.dragEnd.datapointEnter.datapointLeave.dragStart.edit.focusLocation.hideSeries.hoverIndex.lap.onMidpointEnter.onMidpointLeave.onNodeClick.pause.play.postImage.rate.reset.resetZoom.restart.save.saveAnnotations.selectBranch.selectDatapoint.selectGroup.selectLegend.selectNut.selectPlot.selectRibbon.selectRoot.selectSide.selectX.showSeries.start.toggleAnnotator.toggleLock.toggleOpenState.toggleTable.toggleTooltip.toggleZoom.zoomEnd.zoomReset.zoomStart".split("."),
	setup(e, { expose: u, emit: d }) {
		let f = e, { component: _e, dataset: ve, config: ye } = me(f), p = {
			VueUi3dBar: o(() => import("./vue-ui-3d-bar-G3GsMDp6.js").then((e) => e.n)),
			VueUiAgePyramid: o(() => import("./vue-ui-age-pyramid-HW1Kiqib.js").then((e) => e.n)),
			VueUiAnnotator: o(() => import("./vue-ui-annotator-DfG79D4r.js").then((e) => e.n)),
			VueUiCandlestick: o(() => import("./vue-ui-candlestick-DvNtfrWd.js").then((e) => e.n)),
			VueUiChestnut: o(() => import("./vue-ui-chestnut-Cc2J1iOE.js").then((e) => e.n)),
			VueUiDashboard: o(() => import("./vue-ui-dashboard-BJzsxyrY.js").then((e) => e.n)),
			VueUiDigits: o(() => import("./vue-ui-digits-Uy498lu-.js").then((e) => e.n)),
			VueUiDonut: o(() => import("./vue-ui-donut-8RB-gL2J.js").then((e) => e.n)),
			VueUiDonutEvolution: o(() => import("./vue-ui-donut-evolution-CSYfyIbX.js").then((e) => e.n)),
			VueUiGalaxy: o(() => import("./vue-ui-galaxy-cpdWAYuu.js").then((e) => e.n)),
			VueUiGauge: o(() => import("./vue-ui-gauge-CCA1unzt.js").then((e) => e.n)),
			VueUiHeatmap: o(() => import("./vue-ui-heatmap-DaqaYsGY.js").then((e) => e.n)),
			VueUiKpi: o(() => import("./vue-ui-kpi-_UO0otlH.js").then((e) => e.n)),
			VueUiMiniLoader: o(() => import("./vue-ui-mini-loader-CtTewZJG.js").then((e) => e.n)),
			VueUiMolecule: o(() => import("./vue-ui-molecule-D0s8EZSv.js").then((e) => e.n)),
			VueUiMoodRadar: o(() => import("./vue-ui-mood-radar-BnvrlH3J.js").then((e) => e.n)),
			VueUiNestedDonuts: o(() => import("./vue-ui-nested-donuts-CPkrAtLj.js").then((e) => e.n)),
			VueUiOnion: o(() => import("./vue-ui-onion-Cs-P6c1S.js").then((e) => e.n)),
			VueUiQuadrant: o(() => import("./vue-ui-quadrant-H4BH2X1n.js").then((e) => e.n)),
			VueUiRadar: o(() => import("./vue-ui-radar-B-0WKG9B.js").then((e) => e.n)),
			VueUiRating: o(() => import("./vue-ui-rating-BDVSVFA0.js").then((e) => e.n)),
			VueUiRelationCircle: o(() => import("./vue-ui-relation-circle-B9O3HmHR.js").then((e) => e.n)),
			VueUiRings: o(() => import("./vue-ui-rings-lgGvyEW_.js").then((e) => e.n)),
			VueUiScatter: o(() => import("./vue-ui-scatter-Dzdkyw1C.js").then((e) => e.n)),
			VueUiSkeleton: o(() => import("./vue-ui-skeleton-E6Hbh29Z.js").then((e) => e.n)),
			VueUiSmiley: o(() => import("./vue-ui-smiley-5iHJeTPc.js").then((e) => e.n)),
			VueUiSparkbar: o(() => import("./vue-ui-sparkbar-iyq8Toli.js").then((e) => e.n)),
			VueUiSparkgauge: o(() => import("./vue-ui-sparkgauge-CJXxmdWD.js").then((e) => e.n)),
			VueUiSparkHistogram: o(() => import("./vue-ui-sparkhistogram-B_zDxZp5.js").then((e) => e.n)),
			VueUiSparkline: o(() => import("./vue-ui-sparkline-jQ1WegfT.js").then((e) => e.n)),
			VueUiSparkStackbar: o(() => import("./vue-ui-sparkstackbar-Bcipd2lT.js").then((e) => e.n)),
			VueUiTable: o(() => import("./vue-ui-table-DzCJOvjR.js").then((e) => e.n)),
			VueUiTableSparkline: o(() => import("./vue-ui-table-sparkline-Dc6HEQUQ.js").then((e) => e.n)),
			VueUiThermometer: o(() => import("./vue-ui-thermometer-DdrOgp8t.js").then((e) => e.n)),
			VueUiTiremarks: o(() => import("./vue-ui-tiremarks-B19X7xfh.js").then((e) => e.n)),
			VueUiTreemap: o(() => import("./vue-ui-treemap-Dd9pg6yW.js").then((e) => e.n)),
			VueUiVerticalBar: o(() => import("./vue-ui-horizontal-bar-DUMN2pwu.js").then((e) => e.n)),
			VueUiHorizontalBar: o(() => import("./vue-ui-horizontal-bar-DUMN2pwu.js").then((e) => e.n)),
			VueUiWaffle: o(() => import("./vue-ui-waffle-JmiCg9-q.js").then((e) => e.n)),
			VueUiWheel: o(() => import("./vue-ui-wheel-C4HHM1-P.js").then((e) => e.n)),
			VueUiXy: o(() => import("./vue-ui-xy-ChUQgqEu.js").then((e) => e.n)),
			VueUiTableHeatmap: o(() => import("./vue-ui-table-heatmap-D1bbPXKG.js").then((e) => e.n)),
			VueUiAccordion: o(() => import("./vue-ui-accordion-DegI2lzR.js").then((e) => e.n)),
			VueUiQuickChart: o(() => import("./vue-ui-quick-chart-Dn2OpR7I.js").then((e) => e.n)),
			VueUiCursor: o(() => import("./vue-ui-cursor-Cyzzm6Y6.js").then((e) => e.n)),
			VueUiSparkTrend: o(() => import("./vue-ui-spark-trend-BUQ1_7eU.js").then((e) => e.n)),
			VueUiStripPlot: o(() => import("./vue-ui-strip-plot-CjIOca_Y.js").then((e) => e.n)),
			VueUiDumbbell: o(() => import("./vue-ui-dumbbell-8eNZFzyp.js").then((e) => e.n)),
			VueUiWordCloud: o(() => import("./vue-ui-word-cloud-e86BT9TU.js").then((e) => e.n)),
			VueUiXyCanvas: o(() => import("./vue-ui-xy-canvas-1rbwMA1m.js").then((e) => e.n)),
			VueUiFlow: o(() => import("./vue-ui-flow-9VbVqIpA.js").then((e) => e.n)),
			VueUiParallelCoordinatePlot: o(() => import("./vue-ui-parallel-coordinate-plot-1msZmPiM.js").then((e) => e.n)),
			VueUiTimer: o(() => import("./vue-ui-timer-BpFDnonz.js").then((e) => e.n)),
			VueUiCarouselTable: o(() => import("./vue-ui-carousel-table-BQk8CkxQ.js").then((e) => e.n)),
			VueUiGizmo: o(() => import("./vue-ui-gizmo-BvShxRjx.js").then((e) => e.n)),
			VueUiStackbar: o(() => import("./vue-ui-stackbar-BH5oIVxw.js").then((e) => e.n)),
			VueUiStackline: o(() => import("./vue-ui-stackline-CAbvGgyL.js").then((e) => e.n)),
			VueUiBullet: o(() => import("./vue-ui-bullet-AWT4T1Yz.js").then((e) => e.n)),
			VueUiFunnel: o(() => import("./vue-ui-funnel-BJvc2n0l.js").then((e) => e.n)),
			VueUiHistoryPlot: o(() => import("./vue-ui-history-plot-S6MAWwzs.js").then((e) => e.n)),
			VueUiCirclePack: o(() => import("./vue-ui-circle-pack-Do8Bbh4z.js").then((e) => e.n)),
			VueUiWorld: o(() => import("./vue-ui-world-BYnB-6Gl.js").then((e) => e.n)),
			VueUiRidgeline: o(() => import("./vue-ui-ridgeline-D_8sALB0.js").then((e) => e.n)),
			VueUiChord: o(() => import("./vue-ui-chord-j2Qbi0HA.js").then((e) => e.n)),
			VueUiDag: o(() => import("./vue-ui-dag-0e_XFGOJ.js").then((e) => e.n)),
			VueUiGeo: o(() => import("./vue-ui-geo-BeFmMlu5.js").then((e) => e.n)),
			VueUiBump: o(() => import("./vue-ui-bump-DNDDJtRI.js").then((e) => e.n)),
			VueUiHill: o(() => import("./vue-ui-hill-BT5ixO2U.js").then((e) => e.n))
		}, be = {
			VueUi3dBar: ["config", "dataset"],
			VueUiAgePyramid: ["config", "dataset"],
			VueUiAnnotator: ["config", "dataset"],
			VueUiCandlestick: ["config", "dataset"],
			VueUiChestnut: ["config", "dataset"],
			VueUiDashboard: ["config", "dataset"],
			VueUiDigits: ["config", "dataset"],
			VueUiDonut: ["config", "dataset"],
			VueUiDonutEvolution: ["config", "dataset"],
			VueUiGalaxy: ["config", "dataset"],
			VueUiGauge: ["config", "dataset"],
			VueUiHeatmap: ["config", "dataset"],
			VueUiKpi: ["config", "dataset"],
			VueUiMiniLoader: ["config"],
			VueUiMolecule: ["config", "dataset"],
			VueUiMoodRadar: ["config", "dataset"],
			VueUiNestedDonuts: ["config", "dataset"],
			VueUiOnion: ["config", "dataset"],
			VueUiQuadrant: ["config", "dataset"],
			VueUiRadar: ["config", "dataset"],
			VueUiRating: ["config", "dataset"],
			VueUiRelationCircle: ["config", "dataset"],
			VueUiRings: ["config", "dataset"],
			VueUiScatter: ["config", "dataset"],
			VueUiSkeleton: ["config"],
			VueUiSmiley: ["config", "dataset"],
			VueUiSparkbar: ["config", "dataset"],
			VueUiSparkgauge: ["config", "dataset"],
			VueUiSparkHistogram: ["config", "dataset"],
			VueUiSparkline: ["config", "dataset"],
			VueUiSparkStackbar: ["config", "dataset"],
			VueUiTable: ["config", "dataset"],
			VueUiTableSparkline: ["config", "dataset"],
			VueUiThermometer: ["config", "dataset"],
			VueUiTiremarks: ["config", "dataset"],
			VueUiTreemap: ["config", "dataset"],
			VueUiVerticalBar: ["config", "dataset"],
			VueUiHorizontalBar: ["config", "dataset"],
			VueUiWaffle: ["config", "dataset"],
			VueUiWheel: ["config", "dataset"],
			VueUiXy: ["config", "dataset"],
			VueUiTableHeatmap: ["config", "dataset"],
			VueUiAccordion: ["config"],
			VueUiQuickChart: ["config", "dataset"],
			VueUiCursor: ["config"],
			VueUiSparkTrend: ["config", "dataset"],
			VueUiStripPlot: ["config", "dataset"],
			VueUiDumbbell: ["config", "dataset"],
			VueUiWordCloud: ["config", "dataset"],
			VueUiXyCanvas: ["config", "dataset"],
			VueUiFlow: ["config", "dataset"],
			VueUiParallelCoordinatePlot: ["config", "dataset"],
			VueUiTimer: ["config"],
			VueUiCarouselTable: ["config", "dataset"],
			VueUiGizmo: ["config", "dataset"],
			VueUiStackbar: ["config", "dataset"],
			VueUiStackline: ["config", "dataset"],
			VueUiBullet: ["config", "dataset"],
			VueUiFunnel: ["config", "dataset"],
			VueUiHistoryPlot: ["config", "dataset"],
			VueUiCirclePack: ["config", "dataset"],
			VueUiWorld: ["config", "dataset"],
			VueUiRidgeline: ["config", "dataset"],
			VueUiChord: ["config", "dataset"],
			VueUiDag: ["config", "dataset"],
			VueUiGeo: ["config", "dataset"],
			VueUiBump: ["config", "dataset"],
			VueUiHill: ["config", "dataset"]
		}, xe = d, m = n(() => !p[f.component]), Se = n(() => p[f.component] || null), h = c(null), Ce = n(() => {
			let e = be[f.component] || [], t = {};
			return e.includes("config") && (t.config = ye.value), e.includes("dataset") && (t.dataset = ve.value), t;
		}), g = c(() => null), _ = c(() => null), v = c(() => null), y = c(() => null), b = c(() => null), x = c(() => null), S = c(() => null), C = c(() => null), w = c(() => null), T = c(() => null), E = c(() => null), D = c(() => null), O = c(() => null), k = c(() => null), A = c(() => null), j = c(() => null), we = c(() => null), M = c(() => null), N = c(() => null), P = c(() => null), F = c(() => null), I = c(() => null), L = c(() => null), R = c(() => null), z = c(() => null), B = c(() => null), V = c(() => null), H = c(() => null), U = c(() => null), W = c(() => null), G = c(() => null), K = c(() => null), q = c(() => null), Te = c(() => null), Ee = c(() => null), De = c(() => null), J = c(() => null), Y = c(() => null), X = c(() => null), Oe = c(() => null), ke = c(() => null), Ae = c(() => null), je = c(() => null), Me = c(() => null), Ne = c(() => null), Pe = c(() => null), Fe = c(() => null), Ie = c(() => null);
		se(() => {
			m.value && console.error(`\n\nVue Data UI exception:\nThe provided component "${f.component}" does not exist. Check the spelling.\n\nAvailable components:\n\n${Object.keys(p).map((e) => `. ${e}\n`).join("")}`);
		}), l(h, async (e) => {
			e && (e.generatePdf && (g.value = e.generatePdf), e.generateImage && (v.value = e.generateImage), e.generateSvg && (y.value = e.generateSvg), e.generateCsv && (_.value = e.generateCsv), e.getItemsPositions && (b.value = e.getItemsPositions), e.toggleReadonly && (x.value = e.toggleReadonly), e.shoot && (S.value = e.shoot), e.close && (C.value = e.close), e.restoreOrder && (w.value = e.restoreOrder), e.recalculateHeight && (T.value = e.recalculateHeight), e.toggleLock && (E.value = e.toggleLock), e.toggleTable && (D.value = e.toggleTable), e.toggleLabels && (O.value = e.toggleLabels), e.toggleSort && (k.value = e.toggleSort), e.toggleStack && (A.value = e.toggleStack), e.toggleTooltip && (j.value = e.toggleTooltip), e.start && (we.value = e.start), e.pause && (M.value = e.pause), e.reset && (N.value = e.reset), e.restart && (P.value = e.restart), e.lap && (F.value = e.lap), e.toggleAnimation && (I.value = e.toggleAnimation), e.pauseAnimation && (L.value = e.pauseAnimation), e.resumeAnimation && (R.value = e.resumeAnimation), e.toggleAnnotator && (z.value = e.toggleAnnotator), e.selectNode && (B.value = e.selectNode), e.selectGroup && (V.value = e.selectGroup), e.selectRibbon && (H.value = e.selectRibbon), e.autoSize && (U.value = e.autoSize), e.resetZoom && (W.value = e.resetZoom), e.showSeries && (G.value = e.showSeries), e.hideSeries && (K.value = e.hideSeries), e.toggleZoom && (q.value = e.toggleZoom), e.onNodeClick && (Te.value = e.onNodeClick), e.onMidpointEnter && (Ee.value = e.onMidpointEnter), e.onMidpointLeave && (De.value = e.onMidpointLeave), e.zoomIn && (J.value = e.zoomIn), e.zoomOut && (Y.value = e.zoomOut), e.switchOrientation && (X.value = e.switchOrientation), e.focusLocation && (Oe.value = e.focusLocation), e.copyAlt && (ke.value = e.copyAlt), e.edit && (Ae.value = e.edit), e.save && (je.value = e.save), e.cancel && (Me.value = e.cancel), e.dragStart && (Ne.value = e.dragStart), e.dragEnd && (Pe.value = e.dragEnd), e.datapointEnter && (Fe.value = e.datapointEnter), e.datapointLeave && (Ie.value = e.datapointLeave));
		});
		let Le = () => {
			let e = /* @__PURE__ */ "selectLegend.selectDatapoint.toggleOpenState.saveAnnotations.selectRoot.selectBranch.selectNut.change.selectPlot.selectSide.rate.postImage.hoverIndex.selectX.toggleLock.toggleTooltip.start.pause.reset.restart.lap.toggleAnimation.pauseAnimation.resumeAnimation.toggleAnnotator.selectNode.selectGroup.selectRibbon.autoSize.toggleTable.resetZoom.showSeries.hideSeries.toggleZoom.onNodeClick.onMidpointEnter.onMidpointLeave.zoomIn.zoomOut.switchOrientation.focusLocation.zoomStart.zoomEnd.zoomReset.copyAlt.edit.save.cancel.dragStart.dragEnd.datapointEnter.datapointLeave".split("."), t = {};
			return e.forEach((e) => {
				t[e] = (...t) => xe(e, ...t);
			}), t;
		}, Z = de([]);
		function Q(e, t) {
			return new Promise((n, r) => {
				Z.push({
					method: e,
					args: t,
					resolve: n,
					reject: r
				});
			});
		}
		l(h, (e) => {
			if (e) for (; Z.length;) {
				let { method: t, args: n, resolve: r, reject: i } = Z.shift(), a = e[t];
				typeof a == "function" ? Promise.resolve().then(() => a(...n)).then(r).catch(i) : i(/* @__PURE__ */ Error(`Method ${t} not found on ${f.component}`));
			}
		}), u({
			getData(...e) {
				return h.value?.getData ? h.value.getData(...e) : Q("getData", e);
			},
			getImage(e = {}) {
				let { scale: t = 2 } = e;
				return h.value?.getImage ? h.value.getImage({ scale: t }) : Q("getImage", [{ scale: t }]);
			},
			autoSize: U,
			generatePdf: g,
			generateCsv: _,
			generateImage: v,
			generateSvg: y,
			getItemsPositions: b,
			toggleReadonly: x,
			shoot: S,
			close: C,
			restoreOrder: w,
			recalculateHeight: T,
			toggleLock: E,
			toggleTable: D,
			toggleLabels: O,
			toggleSort: k,
			toggleStack: A,
			toggleTooltip: j,
			start: we,
			pause: M,
			reset: N,
			restart: P,
			lap: F,
			pauseAnimation: L,
			resumeAnimation: R,
			toggleAnimation: I,
			toggleAnnotator: z,
			selectNode: B,
			selectGroup: V,
			selectRibbon: H,
			resetZoom: W,
			showSeries: G,
			hideSeries: K,
			toggleZoom: q,
			zoomIn: J,
			zoomOut: Y,
			switchOrientation: X,
			focusLocation: Oe,
			copyAlt: ke,
			save: je,
			cancel: Me
		});
		let $ = n(() => {
			let e = `The provided component ${f.component} does not exist.`;
			return [
				"VueUiIcon",
				"VueUiPattern",
				"Arrow"
			].includes(f.component) ? (e = `${f.component} is not supported by the VueDataUi universal component. You must import it individually.`, console.warn(e), {
				status: "notSupported",
				message: e
			}) : (console.warn(e), {
				status: "unknown",
				message: e
			});
		});
		return (e, n) => m.value ? (s(), a("div", {
			key: 0,
			style: oe({
				width: "100%",
				display: "flex",
				gap: "6px",
				alignItems: "center",
				color: $.value.status === "notSupported" ? "#FF9000" : "#FF0000"
			})
		}, [ee("div", ge, [$.value.status === "unknown" ? (s(), r(t, {
			key: 0,
			name: "moodFlat",
			stroke: "#FF0000"
		})) : i("", !0), $.value.status === "notSupported" ? (s(), r(t, {
			key: 1,
			name: "circleExclamation",
			stroke: "#FF9000"
		})) : i("", !0)]), ne(" " + fe($.value.message), 1)], 4)) : (s(), r(ue(Se.value), ie({
			key: 1,
			ref_key: "currentComponentRef",
			ref: h
		}, Ce.value, pe(Le())), te({ _: 2 }, [ce(e.$slots, (t, n) => ({
			name: n,
			fn: he((t) => [le(e.$slots, n, ae(re(t)))])
		}))]), 1040));
	}
};
//#endregion
export { u as n, d as t };
