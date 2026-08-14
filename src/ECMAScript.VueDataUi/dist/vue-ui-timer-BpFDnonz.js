import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { q as t, qt as n, t as r } from "./lib-Bttd6u5E.js";
import { n as i, t as a } from "./useHints-Dq_w2E8B.js";
import { t as o } from "./useConfig-DlNpz6P8.js";
import { t as s } from "./useNestedProp-vPNvh7rV.js";
import { n as c } from "./Title-BE3qg9xl.js";
import { t as l } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ee, t as te } from "./useResponsive-ZtArZtUf.js";
import { t as u } from "./BaseIcon-BfndwIWE.js";
import { t as ne } from "./DefGrad-DVBqDjhO.js";
import { t as re } from "./useChartAccessibility-DYqac8yF.js";
import { computed as d, createBlock as ie, createCommentVNode as f, createElementBlock as p, createElementVNode as m, createVNode as h, defineAsyncComponent as ae, guardReactiveProps as g, mergeProps as _, normalizeProps as v, normalizeStyle as y, onBeforeUnmount as oe, onMounted as se, openBlock as b, ref as x, renderSlot as S, shallowRef as C, toDisplayString as ce, unref as w, watch as le } from "vue";
//#region src/timer.js
var ue = class {
	constructor(e, t, n, r = !0, i = !0) {
		this.interval = t, this.elapsed = 0, this.isPaused = !1;
		let a = new Blob(["\n            let interval;\n            let elapsed = 0;\n            let paused = false;\n            let startTime;\n            let tickInterval;\n\n            onmessage = function(e) {\n                const { action, data } = e.data;\n\n                switch(action) {\n                    case 'start':\n                        startTime = Date.now();\n                        tickInterval = data.interval;\n                        elapsed = 0;\n                        paused = false;\n                        interval = setInterval(() => {\n                            elapsed += tickInterval;\n                            postMessage({ elapsed, timestamp: Date.now() });\n                        }, tickInterval);\n                        break;\n                    \n                    case 'pause':\n                        paused = true;\n                        clearInterval(interval);\n                        elapsed = Date.now() - startTime;\n                        break;\n\n                    case 'resume':\n                        if (paused) {\n                            startTime = Date.now() - elapsed;\n                            interval = setInterval(() => {\n                                elapsed += tickInterval;\n                                postMessage({ elapsed, timestamp: Date.now() });\n                            }, tickInterval);\n                        }\n                        paused = false;\n                        break;\n\n                    case 'stop':\n                        clearInterval(interval);\n                        elapsed = 0;\n                        postMessage({ elapsed });\n                        break;\n\n                    case 'reset':\n                        elapsed = 0;\n                        clearInterval(interval);\n                        postMessage({ elapsed });\n                        break;\n\n                    case 'lap':\n                        postMessage({\n                            elapsed,\n                            timestamp: Date.now(),\n                            action: 'lap'\n                        });\n                        break;\n\n                    default:\n                        break;\n                }\n            };\n        "], { type: "application/javascript" }), o = URL.createObjectURL(a), s = new Worker(o);
		function c(e) {
			let t = Math.floor(e / 1e3), n = Math.floor(e % 1e3 / 10), a = Math.floor(t / 3600), o = Math.floor(t % 3600 / 60), s = t % 60, c = "";
			return i && (c += String(a).padStart(2, "0") + ":"), c += String(o).padStart(2, "0") + ":", c += String(s).padStart(2, "0"), r && (c += "." + String(n).padStart(2, "0")), c;
		}
		this.start = () => {
			this.isPaused = !1, s.postMessage({
				action: "start",
				data: { interval: this.interval }
			});
		}, this.pause = () => {
			this.isPaused ? this.resume() : (this.isPaused = !0, s.postMessage({ action: "pause" }));
		}, this.resume = () => {
			this.isPaused &&= (s.postMessage({ action: "resume" }), !1);
		}, this.stop = () => {
			s.postMessage({ action: "stop" }), this.isPaused = !1;
		}, this.reset = () => {
			s.postMessage({ action: "reset" }), this.elapsed = 0, this.isPaused = !1;
		}, this.restart = () => {
			this.stop(), this.start();
		}, this.lap = () => new Promise((e) => {
			s.postMessage({ action: "lap" }), s.addEventListener("message", (t) => {
				let { elapsed: n, timestamp: r, action: i } = t.data;
				if (i === "lap") {
					let t = c(n);
					e({
						timestamp: r || 0,
						elapsed: n,
						formatted: t
					});
				}
			}, { once: !0 });
		}), s.onmessage = (t) => {
			let { elapsed: n, timestamp: r } = t.data;
			this.elapsed = n, e({
				timestamp: r || 0,
				elapsed: this.elapsed,
				formatted: c(this.elapsed)
			});
		}, s.onerror = (e) => {
			n && n(e);
		};
	}
}, T = /* @__PURE__ */ e({ default: () => E }), de = ["xmlns", "viewBox"], fe = ["width", "height"], pe = { key: 1 }, me = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], he = [
	"d",
	"stroke",
	"stroke-width"
], ge = [
	"r",
	"fill",
	"stroke",
	"stroke-width"
], _e = [
	"r",
	"fill",
	"stroke",
	"stroke-width"
], ve = ["x", "y"], ye = { key: 5 }, be = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], xe = {
	key: 0,
	class: "vue-ui-timer-controls"
}, Se = ["title"], Ce = ["title"], we = ["title"], Te = ["title"], Ee = ["title"], E = /*#__PURE__*/ l({
	__name: "vue-ui-timer",
	props: { config: {
		type: Object,
		default() {
			return {};
		}
	} },
	emits: [
		"start",
		"pause",
		"reset",
		"restart",
		"lap"
	],
	setup(e, { expose: l, emit: T }) {
		let E = ae(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_timer: De } = o(), D = e, O = T, k = x(null), A = x(null), j = x(null), M = C(null), N = C(null), P = x(t()), F = x(0);
		se(() => {
			I();
		});
		function I() {
			if (L.value.responsive) {
				let e = ee(() => {
					let { width: e, height: t } = te({
						chart: k.value,
						title: L.value.style.title.text ? A.value : null,
						legend: j.value
					});
					requestAnimationFrame(() => {
						B.value.width = e, B.value.height = t, L.value.responsiveProportionalSizing ? (B.value.tracker.core = n({
							relator: Math.min(e, t),
							adjuster: L.value.style.width,
							source: 6 * L.value.stopwatch.tracker.radiusRatio,
							threshold: 1,
							fallback: 1
						}), B.value.tracker.aura = n({
							relator: Math.min(e, t),
							adjuster: L.value.style.width,
							source: 12 * L.value.stopwatch.tracker.aura.radiusRatio,
							threshold: 1,
							fallback: 1
						}), B.value.label = n({
							relator: Math.min(e, t),
							adjuster: L.value.style.width,
							source: L.value.stopwatch.label.fontSize,
							threshold: 10,
							fallback: 10
						})) : B.value.label = L.value.stopwatch.label.fontSize;
					});
				});
				M.value && (N.value && M.value.unobserve(N.value), M.value.disconnect()), M.value = new ResizeObserver(e), N.value = k.value.parentNode, M.value.observe(N.value);
			}
		}
		oe(() => {
			M.value && (N.value && M.value.unobserve(N.value), M.value.disconnect());
		});
		let L = d({
			get: () => z(),
			set: (e) => e
		});
		i({
			config: () => L.value,
			dataset: () => [],
			component: "VueUiTimer",
			rules: [a.noHint]
		});
		let R = d(() => L.value.useCursorPointer), { svgRef: Oe } = re({ config: L.value.style.title });
		function z() {
			return s({
				userConfig: D.config,
				defaultConfig: De
			});
		}
		le(() => D.config, (e) => {
			L.value = z(), I(), F.value += 1;
		}, { deep: !0 });
		let ke = d(() => {
			if (L.value.stopwatch.showHours && L.value.stopwatch.showHundredth) return "00:00:00.00";
			if (L.value.stopwatch.showHours && !L.value.stopwatch.showHundredth) return "00:00:00";
			if (!L.value.stopwatch.showHours && L.value.stopwatch.showHundredth) return "00:00.00";
			if (!L.value.stopwatch.showHours && !L.value.stopwatch.showHundredth) return "00:00";
		}), B = x({
			height: L.value.style.height,
			width: L.value.style.width,
			tracker: {
				core: 6 * L.value.stopwatch.tracker.radiusRatio,
				aura: 12 * L.value.stopwatch.tracker.aura.radiusRatio
			},
			label: L.value.stopwatch.label.fontSize
		}), V = x(0), H = new ue((e) => Ae(e), 10, "", L.value.stopwatch.showHundredth, L.value.stopwatch.showHours), U = x(!0), W = x(!1), G = x(!1);
		function K() {
			O("start"), U.value && H.start(), U.value = !1, W.value = !0;
		}
		function q() {
			W.value &&= (O("reset"), H.stop(), X.value = [], U.value = !0, !1);
		}
		function J() {
			G.value = !G.value, O("pause", V.value), H.pause();
		}
		function Y() {
			W.value && (G.value = !1, O("restart"), X.value = [], H.restart());
		}
		let X = x([]);
		async function Z() {
			if (!W.value || G.value) return;
			let e = await H.lap();
			e && (X.value.push(e), O("lap", X.value));
		}
		function Ae({ timestamp: e, elapsed: t, formatted: n }) {
			V.value = {
				timestamp: e,
				elapsed: t,
				formatted: n
			};
		}
		let Q = d(() => Math.min(B.value.width, B.value.height) / 2.5 * L.value.stopwatch.track.radiusRatio);
		function je(e, t) {
			return e * (360 / (t * 1e3)) % 360;
		}
		function Me(e) {
			let t = Math.PI / 180 * e;
			return {
				cx: B.value.width / 2 + Q.value * Math.cos(t),
				cy: B.value.height / 2 + Q.value * Math.sin(t)
			};
		}
		let $ = d(() => {
			let e = je(V.value.elapsed, L.value.stopwatch.cycleSeconds), { cx: t, cy: n } = Me(e - 90), r = +(e > 180);
			return {
				cx: t || B.value.width / 2,
				cy: n || B.value.height / 2 - Q.value,
				largeArcFlag: r,
				sweepFlag: 1
			};
		});
		return l({
			start: K,
			pause: J,
			reset: q,
			restart: Y,
			lap: Z
		}), (e, t) => (b(), p("div", {
			ref_key: "timerChart",
			ref: k,
			class: "vue-data-ui-component vue-ui-timer",
			style: y({
				fontFamily: L.value.style.fontFamily,
				width: "100%",
				height: L.value.responsive ? "100%" : "auto",
				textAlign: "center"
			})
		}, [
			L.value.style.title.text ? (b(), p("div", {
				key: 0,
				ref_key: "chartTitle",
				ref: A,
				style: y({
					width: "100%",
					background: L.value.style.backgroundColor
				})
			}, [(b(), ie(c, {
				key: `title_${F.value}`,
				config: {
					title: {
						cy: "title",
						...L.value.style.title
					},
					subtitle: {
						cy: "subtitle",
						...L.value.style.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 4)) : f("", !0),
			(b(), p("svg", {
				ref_key: "svgRef",
				ref: Oe,
				xmlns: w(r),
				viewBox: `0 0 ${B.value.width <= 0 ? 10 : B.value.width} ${B.value.height <= 0 ? 10 : B.value.height}`,
				style: y({
					maxWidth: "100%",
					overflow: "visible",
					background: L.value.style.backgroundColor
				})
			}, [
				h(w(E)),
				e.$slots["chart-background"] ? (b(), p("foreignObject", {
					key: 0,
					x: 0,
					y: 0,
					width: B.value.width <= 0 ? 10 : B.value.width,
					height: B.value.height <= 0 ? 10 : B.value.height,
					style: { pointerEvents: "none" }
				}, [S(e.$slots, "chart-background", {}, void 0, !0)], 8, fe)) : f("", !0),
				L.value.stopwatch.tracker.gradient.show ? (b(), p("defs", pe, [h(ne, {
					t: "radial",
					id: `tracker_gradient_${P.value}`,
					cx: "50%",
					cy: "50%",
					r: "50%",
					fx: "50%",
					fy: "50%",
					stops: [[
						"0%",
						L.value.stopwatch.tracker.gradient.color,
						1
					], [
						"100%",
						L.value.stopwatch.tracker.fill,
						1
					]]
				}, null, 8, ["id", "stops"])])) : f("", !0),
				m("circle", {
					cx: B.value.width / 2,
					cy: B.value.height / 2,
					r: Q.value,
					fill: L.value.stopwatch.track.fill,
					stroke: L.value.stopwatch.track.stroke,
					"stroke-width": L.value.stopwatch.track.strokeWidth
				}, null, 8, me),
				L.value.stopwatch.cycleTrack.show ? (b(), p("path", {
					key: 2,
					d: `M ${B.value.width / 2},${B.value.height / 2 - Q.value} A ${Q.value},${Q.value} 0 ${$.value.largeArcFlag},${$.value.sweepFlag} ${$.value.cx},${$.value.cy}`,
					stroke: L.value.stopwatch.cycleTrack.stroke,
					"stroke-width": L.value.stopwatch.cycleTrack.strokeWidth,
					"stroke-linecap": "round",
					fill: "none"
				}, null, 8, he)) : f("", !0),
				m("circle", _($.value, {
					r: B.value.tracker.core,
					fill: L.value.stopwatch.tracker.gradient.show ? `url(#tracker_gradient_${P.value})` : L.value.stopwatch.tracker.fill,
					stroke: L.value.stopwatch.tracker.stroke,
					"stroke-width": L.value.stopwatch.tracker.strokeWidth
				}), null, 16, ge),
				L.value.stopwatch.tracker.aura.show ? (b(), p("circle", _({ key: 3 }, $.value, {
					r: B.value.tracker.aura,
					fill: `${L.value.stopwatch.tracker.aura.fill}20`,
					stroke: L.value.stopwatch.tracker.aura.stroke,
					"stroke-width": L.value.stopwatch.tracker.aura.strokeWidth
				}), null, 16, _e)) : f("", !0),
				e.$slots.time ? (b(), p("foreignObject", {
					key: 4,
					x: B.value.width / 2,
					y: B.value.height / 2,
					height: "0.1",
					width: "0.1",
					style: { overflow: "visible" }
				}, [S(e.$slots, "time", v(g({
					...V.value,
					...B.value
				})), void 0, !0)], 8, ve)) : e.$slots.timeSvg ? (b(), p("g", ye, [S(e.$slots, "timeSvg", v(g({
					...V.value,
					...B.value
				})), void 0, !0)])) : (b(), p("text", {
					key: 6,
					x: B.value.width / 2,
					y: B.value.height / 2 + B.value.label / 4,
					"font-size": B.value.label,
					"text-anchor": "middle",
					fill: L.value.stopwatch.label.color,
					"font-weight": L.value.stopwatch.label.bold ? "bold" : "normal",
					style: { "font-variant-numeric": "tabular-nums !important" }
				}, ce(V.value.formatted || ke.value), 9, be))
			], 12, de)),
			m("div", {
				ref_key: "chartLegend",
				ref: j,
				style: y({
					width: "100%",
					backgroundColor: L.value.stopwatch.legend.backgroundColor
				})
			}, [
				e.$slots.controls ? f("", !0) : (b(), p("div", xe, [
					L.value.stopwatch.legend.buttons.start ? (b(), p("button", {
						key: 0,
						title: L.value.stopwatch.legend.buttonTitles.start,
						onClick: K,
						class: "vue-ui-timer-button",
						style: y({
							opacity: W.value ? .2 : 1,
							cursor: W.value ? "default" : R.value ? "pointer" : "default"
						})
					}, [h(u, {
						name: "play",
						stroke: L.value.stopwatch.legend.buttons.iconColor
					}, null, 8, ["stroke"])], 12, Se)) : f("", !0),
					L.value.stopwatch.legend.buttons.pause ? (b(), p("button", {
						key: 1,
						title: G.value ? L.value.stopwatch.legend.buttonTitles.resume : L.value.stopwatch.legend.buttonTitles.pause,
						onClick: J,
						class: "vue-ui-timer-button",
						style: y({
							opacity: W.value ? 1 : .2,
							cursor: W.value && R.value ? "pointer" : "default"
						})
					}, [h(u, {
						name: "pause",
						stroke: L.value.stopwatch.legend.buttons.iconColor
					}, null, 8, ["stroke"])], 12, Ce)) : f("", !0),
					L.value.stopwatch.legend.buttons.reset ? (b(), p("button", {
						key: 2,
						title: L.value.stopwatch.legend.buttonTitles.reset,
						onClick: q,
						class: "vue-ui-timer-button",
						style: y({
							opacity: W.value ? 1 : .2,
							cursor: W.value && R.value ? "pointer" : "default"
						})
					}, [h(u, {
						name: "stop",
						stroke: L.value.stopwatch.legend.buttons.iconColor
					}, null, 8, ["stroke"])], 12, we)) : f("", !0),
					L.value.stopwatch.legend.buttons.restart ? (b(), p("button", {
						key: 3,
						title: L.value.stopwatch.legend.buttonTitles.restart,
						onClick: Y,
						class: "vue-ui-timer-button",
						style: y({
							opacity: W.value ? 1 : .2,
							cursor: W.value && R.value ? "pointer" : "default"
						})
					}, [h(u, {
						name: "restart",
						stroke: L.value.stopwatch.legend.buttons.iconColor
					}, null, 8, ["stroke"])], 12, Te)) : f("", !0),
					L.value.stopwatch.legend.buttons.lap ? (b(), p("button", {
						key: 4,
						title: L.value.stopwatch.legend.buttonTitles.lap,
						onClick: Z,
						class: "vue-ui-timer-button",
						style: y({
							opacity: W.value && !G.value ? 1 : .2,
							cursor: W.value && !G.value && R.value ? "pointer" : "default"
						})
					}, [h(u, {
						name: "lap",
						stroke: L.value.stopwatch.legend.buttons.iconColor
					}, null, 8, ["stroke"])], 12, Ee)) : f("", !0)
				])),
				S(e.$slots, "controls", v(g({
					start: K,
					pause: J,
					reset: q,
					restart: Y,
					lap: Z,
					laps: X.value,
					isRunning: W.value,
					isPaused: G.value,
					...V.value
				})), void 0, !0),
				S(e.$slots, "laps", v(g({
					laps: X.value,
					lap: Z,
					isRunning: W.value,
					isPaused: G.value,
					...V.value
				})), void 0, !0)
			], 4)
		], 4));
	}
}, [["__scopeId", "data-v-64177013"]]);
//#endregion
export { T as n, E as t };
