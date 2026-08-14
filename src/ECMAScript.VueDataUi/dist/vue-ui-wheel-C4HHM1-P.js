import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Jt as t, Ot as n, Vt as r, X as i, b as a, i as o, jt as s, kt as c, q as l, t as u, tt as d } from "./lib-Bttd6u5E.js";
import { n as f, t as p } from "./useHints-Dq_w2E8B.js";
import { t as m } from "./useConfig-DlNpz6P8.js";
import { t as h } from "./usePrinter-DN5bYhTG.js";
import { n as g, t as _ } from "./BaseScanner-DZvpgOjM.js";
import { t as v } from "./useNestedProp-vPNvh7rV.js";
import { t as y } from "./useThemeCheck-C43Tcqmk.js";
import { t as b } from "./useChartExport-DNiwdPmb.js";
import { t as x } from "./img-Bnokohej.js";
import { n as ee } from "./Title-BE3qg9xl.js";
import { t as S } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as C, t as te } from "./useResponsive-ZtArZtUf.js";
import { t as ne } from "./useUserOptionState-DK-_1ddE.js";
import { t as re } from "./useChartAccessibility-DYqac8yF.js";
import { t as ie } from "./usePrefersMotion-BC-CsqR1.js";
import { t as ae } from "./vue_ui_wheel-DZ_nR--t.js";
import { Fragment as w, computed as T, createBlock as E, createCommentVNode as D, createElementBlock as O, createElementVNode as oe, createSlots as se, createVNode as ce, defineAsyncComponent as le, guardReactiveProps as k, normalizeClass as A, normalizeProps as j, normalizeStyle as ue, onBeforeUnmount as de, onMounted as fe, openBlock as M, ref as N, renderList as P, renderSlot as F, shallowRef as pe, toDisplayString as me, toRefs as he, unref as I, useCssVars as ge, watch as _e, withCtx as L } from "vue";
//#region src/components/vue-ui-wheel.vue
var ve = /* @__PURE__ */ e({ default: () => ze }), ye = ["id"], be = [
	"xmlns",
	"viewBox",
	"aria-labelledby",
	"aria-describedby"
], xe = ["id"], Se = ["id"], Ce = [
	"x",
	"y",
	"width",
	"height"
], we = [
	"d",
	"stroke",
	"stroke-width"
], Te = [
	"cx",
	"cy",
	"r",
	"stroke",
	"stroke-width"
], Ee = { key: 0 }, De = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-linecap"
], Oe = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-linecap"
], ke = { key: 1 }, Ae = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], je = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], Me = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], Ne = [
	"x1",
	"x2",
	"y1",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-linecap"
], Pe = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], Fe = ["aria-label"], Ie = [
	"x",
	"y",
	"height"
], Le = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight",
	"stroke",
	"stroke-width"
], Re = {
	key: 4,
	class: "vue-data-ui-watermark"
}, ze = /*#__PURE__*/ S({
	__name: "vue-ui-wheel",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	emits: ["copyAlt"],
	setup(e, { expose: S, emit: ve }) {
		ge((e) => ({
			v79c1e0f9: Pt.value,
			v52b4fc9f: Ft.value,
			v52b4df32: It.value
		}));
		let ze = le(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Be = le(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Ve = le(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_wheel: He } = m(), { isThemeValid: Ue, warnInvalidTheme: We } = y(), R = ie(), z = e, Ge = ve, Ke = T(() => !!z.dataset && Object.keys(z.dataset).length), B = N(l()), qe = N(null), Je = N(0), V = N(null), Ye = N(null), Xe = N(null), Ze = N(null), Qe = N(0), H = N(ot());
		f({
			config: () => H.value,
			dataset: () => z.dataset,
			component: "VueUiWheel",
			rules: [p.noHint]
		});
		let $e = T(() => H.value.userOptions.useCursorPointer), et = T(() => t({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					animation: { use: !1 },
					layout: {
						wheel: { ticks: {
							activeColor: "#6A6A6A80",
							inactiveColor: "#CACACA80"
						} },
						innerCircle: { stroke: "#CACACA80" }
					}
				} }
			},
			userConfig: H.value.skeletonConfig ?? {}
		})), { loading: tt, FINAL_DATASET: U } = g({
			...he(z),
			FINAL_CONFIG: H,
			prepareConfig: ot,
			skeletonDataset: z.config?.skeletonDataset ?? { percentage: 50 },
			skeletonConfig: t({
				defaultConfig: H.value,
				userConfig: et.value
			})
		}), { userOptionsVisible: nt, setUserOptionsVisibility: rt, keepUserOptionState: it } = ne({ config: H.value }), { svgRef: at } = re({ config: H.value.style.chart.title });
		function ot() {
			let e = v({
				userConfig: z.config,
				defaultConfig: He
			}), t = e.theme;
			if (!t) return e;
			if (!Ue.value(e)) return We(e), e;
			let n = v({
				userConfig: ae[t] || z.config,
				defaultConfig: e
			});
			return v({
				userConfig: z.config,
				defaultConfig: n
			});
		}
		_e(() => z.config, (e) => {
			H.value = ot(), nt.value = !H.value.userOptions.showOnChartHover, mt(), Qe.value += 1;
		}, { deep: !0 });
		let { isPrinting: st, isImaging: ct, generatePdf: lt, generateImage: ut } = h({
			elementId: B.value,
			fileName: H.value.style.chart.title.text || "vue-ui-wheel",
			options: H.value.userOptions.print
		}), dt = T(() => H.value.userOptions.show && !H.value.style.chart.title.text), W = N({
			size: 360,
			height: 360,
			width: 360
		}), G = N(H.value.style.chart.layout.percentage.fontSize), K = T(() => ({
			radius: Math.min(W.value.width, W.value.height) * .9 / 2 * H.value.style.chart.layout.wheel.radiusRatio,
			centerX: W.value.width / 2,
			centerY: W.value.height / 2
		}));
		function ft(e, t = 1) {
			let n = 29.85;
			return {
				x: K.value.centerX + K.value.radius * Math.cos(n + e * Math.PI / 180) * t,
				y: K.value.centerY + K.value.radius * Math.sin(n + e * Math.PI / 180) * t
			};
		}
		let q = N(H.value.style.chart.animation.use && !R.value ? 0 : U.value.percentage || 0);
		_e(() => U.value, (e) => {
			H.value.style.chart.animation.use && !R.value ? Et(e.percentage) : q.value = e.percentage || 0;
		}, { deep: !0 });
		let J = pe(null), Y = pe(null);
		fe(() => {
			mt();
		});
		let pt = T(() => H.value.debug);
		function mt() {
			if (s(z.dataset) && d({
				componentName: "VueUiWheel",
				type: "dataset",
				debug: pt.value
			}), H.value.responsive) {
				let e = C(() => {
					let { width: e, height: t } = te({
						chart: V.value,
						title: H.value.style.chart.title.text ? Ye.value : null,
						source: Xe.value,
						noTitle: Ze.value
					});
					requestAnimationFrame(() => {
						W.value.width = e, W.value.height = t, G.value = H.value.style.chart.layout.percentage.fontSize / 360 * Math.min(e, t);
					});
				});
				J.value && (Y.value && J.value.unobserve(Y.value), J.value.disconnect()), J.value = new ResizeObserver(e), Y.value = V.value.parentNode, J.value.observe(Y.value);
			}
			Et(U.value.percentage || 0), H.value.style.chart.animation.use && !R.value || (q.value = U.value.percentage || 0);
		}
		de(() => {
			J.value && (Y.value && J.value.unobserve(Y.value), J.value.disconnect());
		});
		function ht([e, t, n], r) {
			let i = Math.cos(r), a = Math.sin(r);
			return [
				e,
				t * i - n * a,
				t * a + n * i
			];
		}
		function gt([e, t, n], r) {
			let i = r / (r - n);
			return [
				e * i,
				t * i,
				n,
				i
			];
		}
		function _t(e, t) {
			let n = e.replace("#", ""), r = parseInt(n.substring(0, 2), 16), i = parseInt(n.substring(2, 4), 16), a = parseInt(n.substring(4, 6), 16), o = 1 - Math.min(1, Math.max(0, H.value.style.chart.layout.wheel.ticks.shadeColorRatio3d)) * t, s = Math.max(0, Math.min(255, Math.round(r * o))), c = Math.max(0, Math.min(255, Math.round(i * o))), l = Math.max(0, Math.min(255, Math.round(a * o)));
			return `#${s.toString(16).padStart(2, "0")}${c.toString(16).padStart(2, "0")}${l.toString(16).padStart(2, "0")}`;
		}
		function vt({ cx: e, cy: t, radius: n, innerRatio: i = .8, count: a = 120, startDeg: o = 0, axDeg: s = 50, f: c = 520, baseStroke: l = 5, activeColor: u, inactiveColor: d, getActive: f }) {
			let p = s * Math.PI / 180, m = n, h = n * i, g = [];
			for (let n = 0; n < a; n += 1) {
				let i = (n / a * 360 + o) * Math.PI / 180, s = e + m * Math.cos(i), _ = t + m * Math.sin(i), v = e + h * Math.cos(i), y = t + h * Math.sin(i), b = [
					s - e,
					_ - t,
					0
				], x = [
					v - e,
					y - t,
					0
				], [ee, S, C] = ht(b, p), [te, ne, re] = ht(x, p), [ie, ae, , w] = gt([
					ee,
					S,
					C
				], c), [T, E, , D] = gt([
					te,
					ne,
					re
				], c), O = e + ie, oe = t + ae, se = e + T, ce = t + E, le = (Math.max(C, re) - -m * Math.sin(p)) / (2 * m * Math.sin(p) || 1), k = !f || f(n), A = H.value.style.chart.layout.wheel.ticks.gradient.show ? r(u, n * Q.value / Z.value * (H.value.style.chart.layout.wheel.ticks.gradient.shiftHueIntensity / 100)) : u, j = _t(k ? A : d, le), ue = Math.max(1.25, l * w * (Math.min(W.value.width, W.value.height) / 360));
				g.push({
					i: n,
					x1: O,
					y1: oe,
					x2: se,
					y2: ce,
					stroke: ue,
					color: j,
					z: Math.max(C, re)
				});
			}
			return g.sort((e, t) => e.z - t.z), g;
		}
		let yt = T(() => {
			if (!H.value.layout === "3d") return null;
			let e = Z.value, t = H.value.style.chart.layout.wheel.ticks.gradient.show ? r(H.value.style.chart.layout.wheel.ticks.activeColor, 0) : H.value.style.chart.layout.wheel.ticks.activeColor, n = H.value.style.chart.layout.wheel.ticks.inactiveColor, i = H.value.style.chart.layout.wheel.ticks.strokeWidth;
			return vt({
				cx: K.value.centerX,
				cy: K.value.centerY,
				radius: K.value.radius,
				innerRatio: H.value.style.chart.layout.wheel.ticks.sizeRatio,
				count: e,
				startDeg: -90,
				axDeg: H.value.style.chart.layout.wheel.tiltAngle3d,
				f: Math.min(W.value.width, W.value.height) * 1.45,
				baseStroke: i,
				activeColor: t,
				inactiveColor: n,
				getActive: (e) => q.value > e * Q.value
			});
		});
		function bt({ cx: e, cy: t, r: n, count: r = 180, startDeg: i = -90, axDeg: a = 50, f: o }) {
			let s = a * Math.PI / 180, c = [], l = 0;
			for (let a = 0; a < r; a += 1) {
				let u = (a / r * 360 + i) * Math.PI / 180, [d, f, p] = ht([
					n * Math.cos(u),
					n * Math.sin(u),
					0
				], s), [m, h, , g] = gt([
					d,
					f,
					p
				], o);
				l += g, c.push([e + m, t + h]);
			}
			let u = `M ${c[0][0]} ${c[0][1]}`;
			for (let e = 1; e < c.length; e += 1) u += ` L ${c[e][0]} ${c[e][1]}`;
			u += " Z";
			let d = l / r;
			return {
				d: u,
				avgScale: d,
				pts: c
			};
		}
		let X = T(() => {
			if (H.value.layout !== "3d") return null;
			let e = Math.min(W.value.width, W.value.height) * 1.45, t = H.value.style.chart.layout.wheel.tiltAngle3d, n = K.value.radius, { pts: r, avgScale: i } = (() => {
				let r = n, i = t, { d: a, avgScale: o, pts: s } = bt({
					cx: K.value.centerX,
					cy: K.value.centerY,
					r,
					startDeg: -90,
					axDeg: i,
					f: e
				});
				return {
					pts: s,
					avgScale: o
				};
			})(), a = Infinity, o = Infinity, s = -Infinity, c = -Infinity;
			for (let [e, t] of r) e < a && (a = e), t < o && (o = t), e > s && (s = e), t > c && (c = t);
			let l = H.value.style.chart.layout.wheel.ticks.strokeWidth / 360 * Math.min(W.value.width, W.value.height), u = H.value.style.chart.layout.innerCircle.strokeWidth || 0, d = .5 * Math.max(l, u * (i || 1)), f = Math.max(0, Number(H.value.style.chart.layout.wheel.ticks.depth3d) || 0), p = d;
			return {
				x: a - p,
				y: o - f - p,
				w: s - a + 2 * p,
				h: c - (o - f) + 2 * p
			};
		});
		function xt(e) {
			let t = Math.min(W.value.width, W.value.height) * 1.45, { d: n, avgScale: r } = bt({
				cx: K.value.centerX,
				cy: K.value.centerY,
				r: e,
				startDeg: -90,
				axDeg: H.value.style.chart.layout.wheel.tiltAngle3d,
				f: t
			}), i = (H.value.style.chart.layout.innerCircle.strokeWidth || 1) * r;
			return {
				d: n,
				stroke: H.value.style.chart.layout.innerCircle.stroke,
				strokeWidth: i
			};
		}
		let St = T(() => xt(Math.max(0, K.value.radius * .8 * H.value.style.chart.layout.innerCircle.radiusRatio)));
		function Ct({ cx: e, cy: t, r: n, aRad: r, ax: i, f: a }) {
			let [o, s, c] = ht([
				n * Math.cos(r),
				n * Math.sin(r),
				0
			], i), [l, u, , d] = gt([
				o,
				s,
				c
			], a);
			return {
				x: e + l,
				y: t + u,
				z: c,
				s: d
			};
		}
		function wt({ cx: e, cy: t, radius: n, innerRatio: i = .8, count: a = 120, startDeg: o = -87, axDeg: s = 45, f: c = 600, activeColor: l, inactiveColor: u, getActive: d, Y: f = 0 }) {
			let p = s * Math.PI / 180, m = n, h = n * i, g = 2 * Math.PI / a, _ = [];
			for (let n = 0; n < a; n += 1) {
				let i = o * Math.PI / 180 + g * n, s = i + g * Math.min(1, H.value.style.chart.layout.wheel.ticks.spacingRatio3d), v = Ct({
					cx: e,
					cy: t + f,
					r: m,
					aRad: i,
					ax: p,
					f: c
				}), y = Ct({
					cx: e,
					cy: t + f,
					r: m,
					aRad: s,
					ax: p,
					f: c
				}), b = Ct({
					cx: e,
					cy: t + f,
					r: h,
					aRad: s,
					ax: p,
					f: c
				}), x = Ct({
					cx: e,
					cy: t + f,
					r: h,
					aRad: i,
					ax: p,
					f: c
				}), ee = (v.z + y.z + x.z + b.z) / 4, S = _t(!d || d(n) ? H.value.style.chart.layout.wheel.ticks.gradient.show ? r(H.value.style.chart.layout.wheel.ticks.activeColor, 100 / a * n / 100 * (H.value.style.chart.layout.wheel.ticks.gradient.shiftHueIntensity / 100)) : l : u, (() => {
					let e = m * Math.sin(p) || 1;
					return (ee - -e) / (2 * e);
				})()), C = `M ${v.x} ${v.y} L ${y.x} ${y.y} L ${b.x} ${b.y} L ${x.x} ${x.y} Z`;
				_.push({
					i: n,
					d: C,
					fill: S,
					z: ee
				});
			}
			return _.sort((e, t) => e.z - t.z), _;
		}
		let Tt = T(() => {
			if (H.value.layout !== "3d") return null;
			let e = Z.value;
			return (t) => wt({
				cx: K.value.centerX,
				cy: K.value.centerY,
				radius: K.value.radius,
				innerRatio: H.value.style.chart.layout.wheel.ticks.sizeRatio,
				count: e,
				startDeg: -90,
				axDeg: H.value.style.chart.layout.wheel.tiltAngle3d,
				f: Math.min(W.value.width, W.value.height) * 1.45,
				activeColor: H.value.style.chart.layout.wheel.ticks.activeColor,
				inactiveColor: H.value.style.chart.layout.wheel.ticks.inactiveColor,
				getActive: (t) => q.value > 100 / e * t,
				Y: t
			});
		});
		function Et(e) {
			let t = H.value.style.chart.animation.speed, n = Math.abs(e - q.value) / (t * 120);
			function r() {
				q.value < e ? q.value = Math.min(q.value + n, e) : q.value > e && (q.value = Math.max(q.value - n, e)), q.value !== e && requestAnimationFrame(r);
			}
			r();
		}
		let Z = T(() => (pt.value && H.value.style.chart.layout.wheel.ticks.quantity < 12 && console.warn("VueUiWheel - The min number of ticks is 12"), pt.value && H.value.style.chart.layout.wheel.ticks.quantity > 200 && console.warn("VueUiWheel - The max number of ticks is 200"), Math.max(12, Math.min(H.value.style.chart.layout.wheel.ticks.quantity, 200)))), Q = T(() => 100 / Z.value), Dt = T(() => {
			let e = [];
			for (let t = 0; t < Z.value; t += 1) {
				let n = q.value > t * Q.value ? H.value.style.chart.layout.wheel.ticks.activeColor : H.value.style.chart.layout.wheel.ticks.inactiveColor, { x: i, y: a } = ft(W.value.size / Z.value * t), { x: o, y: s } = ft(W.value.size / Z.value * t, H.value.style.chart.layout.wheel.ticks.sizeRatio);
				e.push({
					x1: i,
					y1: a,
					x2: o,
					y2: s,
					color: H.value.style.chart.layout.wheel.ticks.gradient.show ? r(n, t * Q.value / Z.value * (H.value.style.chart.layout.wheel.ticks.gradient.shiftHueIntensity / 100)) : n
				});
			}
			return e;
		}), Ot = T(() => c({ series: Dt.value.map((e) => ({
			name: "",
			value: 1,
			color: e.color
		})) }, K.value.centerX, K.value.centerY, K.value.radius, K.value.radius, 1.99999, 2, 1, 360, 105.25, K.value.radius * (1 - H.value.style.chart.layout.wheel.ticks.sizeRatio))), kt = N(!1);
		function At(e) {
			kt.value = e, Je.value += 1;
		}
		let jt = N(!1);
		function Mt() {
			jt.value = !jt.value;
		}
		async function Nt({ scale: e = 2 } = {}) {
			if (!V.value) return;
			let { width: t, height: n } = V.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await x({
				domElement: V.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: H.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Pt = T(() => H.value.style.chart.layout.wheel.ticks.strokeWidth * 2), Ft = T(() => H.value.style.chart.layout.wheel.ticks.strokeWidth * 2 * .75), It = T(() => H.value.style.chart.layout.wheel.ticks.strokeWidth), $ = T(() => Math.max(1, Math.min(20, H.value.style.chart.layout.wheel.ticks.depth3d))), Lt = T(() => H.value.style.chart.backgroundColor), Rt = T(() => H.value.style.chart.title), { isCallbackImaging: zt, isCallbackSvg: Bt, generateSvg: Vt, onGenerateImage: Ht } = b({
			svg: at,
			title: Rt,
			legend: null,
			legendItems: null,
			backgroundColor: Lt,
			stretchTitle: !0,
			getSvgCallback: () => H.value.userOptions.callbacks.svg,
			generateImage: ut
		});
		async function Ut() {
			if (Ge("copyAlt", {
				config: H.value,
				dataset: U.value
			}), !H.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(H.value.userOptions.callbacks.altCopy({
				config: H.value,
				dataset: U.value
			}));
		}
		let Wt = T(() => `${B.value}-title`), Gt = T(() => `${B.value}-desc`), Kt = T(() => o(H.value.style.chart.layout.percentage.formatter, a(q.value), i({
			v: a(q.value),
			s: "%",
			r: H.value.style.chart.layout.percentage.rounding
		}))), qt = T(() => H.value.style.chart.title.text || ""), Jt = T(() => tt.value ? "..." : `${Kt.value}`);
		return S({
			getImage: Nt,
			generatePdf: lt,
			generateImage: ut,
			generateSvg: Vt,
			toggleAnnotator: Mt,
			toggleFullscreen: At,
			copyAlt: Ut
		}), (e, t) => (M(), O("div", {
			class: A(["vue-ui-wheel", {
				"vue-data-ui-component": !0,
				"vue-ui-wheel-3d-wrap": H.value.layout === "3d"
			}]),
			ref_key: "wheelChart",
			ref: V,
			id: B.value,
			style: ue(`font-family:${H.value.style.fontFamily};width:100%; text-align:center;background:${H.value.style.chart.backgroundColor};${H.value.responsive ? "height:100%" : ""}`),
			onMouseenter: t[0] ||= () => I(rt)(!0),
			onMouseleave: t[1] ||= () => I(rt)(!1)
		}, [
			H.value.userOptions.buttons.annotator ? (M(), E(I(ze), {
				key: 0,
				svgRef: I(at),
				backgroundColor: H.value.style.chart.backgroundColor,
				color: H.value.style.chart.color,
				active: jt.value,
				isCursorPointer: $e.value,
				onClose: Mt
			}, {
				"annotator-action-close": L(() => [F(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": L(({ color: t }) => [F(e.$slots, "annotator-action-color", j(k({ color: t })), void 0, !0)]),
				"annotator-action-draw": L(({ mode: t }) => [F(e.$slots, "annotator-action-draw", j(k({ mode: t })), void 0, !0)]),
				"annotator-action-undo": L(({ disabled: t }) => [F(e.$slots, "annotator-action-undo", j(k({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": L(({ disabled: t }) => [F(e.$slots, "annotator-action-redo", j(k({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": L(({ disabled: t }) => [F(e.$slots, "annotator-action-delete", j(k({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : D("", !0),
			dt.value ? (M(), O("div", {
				key: 1,
				ref_key: "noTitle",
				ref: Ze,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : D("", !0),
			H.value.style.chart.title.text ? (M(), O("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: Ye,
				style: "width:100%;background:transparent;padding-bottom:12px"
			}, [(M(), E(ee, {
				key: `title_${Qe.value}`,
				config: {
					title: {
						cy: "wheel-title",
						...H.value.style.chart.title
					},
					subtitle: {
						cy: "wheel-subtitle",
						...H.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : D("", !0),
			H.value.userOptions.show && Ke.value && (I(it) || I(nt)) ? (M(), E(I(Be), {
				ref_key: "details",
				ref: qe,
				key: `user_options_${Je.value}`,
				backgroundColor: H.value.style.chart.backgroundColor,
				color: H.value.style.chart.color,
				isPrinting: I(st),
				isImaging: I(ct),
				uid: B.value,
				hasPdf: H.value.userOptions.buttons.pdf,
				hasImg: H.value.userOptions.buttons.img,
				hasSvg: H.value.userOptions.buttons.svg,
				hasFullscreen: H.value.userOptions.buttons.fullscreen,
				hasAltCopy: H.value.userOptions.buttons.altCopy,
				hasXls: !1,
				isFullscreen: kt.value,
				position: H.value.userOptions.position,
				titles: { ...H.value.userOptions.buttonTitles },
				hasAnnotator: H.value.userOptions.buttons.annotator,
				isAnnotation: jt.value,
				chartElement: V.value,
				callbacks: H.value.userOptions.callbacks,
				printScale: H.value.userOptions.print.scale,
				isCursorPointer: $e.value,
				onToggleFullscreen: At,
				onGeneratePdf: I(lt),
				onGenerateImage: I(Ht),
				onGenerateSvg: I(Vt),
				onToggleAnnotator: Mt,
				onCopyAlt: Ut,
				style: ue({ visibility: I(it) ? I(nt) ? "visible" : "hidden" : "visible" })
			}, se({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: L(({ isOpen: t, color: n }) => [F(e.$slots, "menuIcon", j(k({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: L(() => [F(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: L(() => [F(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: L(() => [F(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: L(({ toggleFullscreen: t, isFullscreen: n }) => [F(e.$slots, "optionFullscreen", j(k({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: L(({ toggleAnnotator: t, isAnnotator: n }) => [F(e.$slots, "optionAnnotator", j(k({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: L(({ altCopy: t }) => [F(e.$slots, "optionAltCopy", j(k({ altCopy: t })), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: L(() => [F(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: L(() => [F(e.$slots, "custom-menu-after", {}, void 0, !0)]),
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
				"position",
				"titles",
				"hasAnnotator",
				"isAnnotation",
				"chartElement",
				"callbacks",
				"printScale",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : D("", !0),
			(M(), O("svg", {
				ref_key: "svgRef",
				ref: at,
				xmlns: I(u),
				class: A({
					"vue-data-ui-fullscreen--on": kt.value,
					"vue-data-ui-fulscreen--off": !kt.value,
					"vue-ui-wheel-3d-svg": H.value.layout === "3d"
				}),
				viewBox: H.value.layout === "3d" && !H.value.responsive ? `${X.value?.x - 10} ${X.value?.y ?? 0} ${X.value?.w + 20 ?? Math.max(10, W.value.width)} ${X.value?.h ?? Math.max(10, W.value.height)}` : `0 0 ${Math.max(10, W.value.width)} ${Math.max(10, W.value.height)}`,
				style: ue(`max-width:100%;overflow:visible;background:transparent;color:${H.value.style.chart.color}`),
				role: "img",
				"aria-labelledby": Wt.value,
				"aria-describedby": Gt.value
			}, [
				oe("title", { id: Wt.value }, me(qt.value), 9, xe),
				oe("desc", { id: Gt.value }, me(Jt.value), 9, Se),
				ce(I(Ve)),
				e.$slots["chart-background"] ? (M(), O("foreignObject", {
					key: 0,
					x: H.value.layout === "3d" && !H.value.responsive ? X.value?.x - 10 : 0,
					y: H.value.layout === "3d" && !H.value.responsive ? X.value?.y ?? 0 : 0,
					width: H.value.layout === "3d" && !H.value.responsive ? X.value?.w + 20 ?? Math.max(10, W.value.width) : Math.max(10, W.value.width),
					height: H.value.layout === "3d" && !H.value.responsive ? X.value?.h ?? Math.max(10, W.value.height) : Math.max(10, W.value.height),
					style: { pointerEvents: "none" }
				}, [F(e.$slots, "chart-background", {}, void 0, !0)], 8, Ce)) : D("", !0),
				H.value.layout === "3d" && St.value ? (M(), O("path", {
					key: 1,
					class: "vue-ui-wheel-inner-circle",
					d: St.value.d,
					stroke: H.value.style.chart.layout.innerCircle.stroke,
					"stroke-width": H.value.style.chart.layout.innerCircle.strokeWidth,
					fill: "none"
				}, null, 8, we)) : H.value.style.chart.layout.innerCircle.show ? (M(), O("circle", {
					key: 2,
					class: "vue-ui-wheel-inner-circle",
					cx: K.value.centerX,
					cy: K.value.centerY,
					r: Math.max(0, K.value.radius * H.value.style.chart.layout.innerCircle.radiusRatio * .8),
					stroke: H.value.style.chart.layout.innerCircle.stroke,
					"stroke-width": H.value.style.chart.layout.innerCircle.strokeWidth,
					fill: "none"
				}, null, 8, Te)) : D("", !0),
				H.value.layout === "3d" ? (M(), O(w, { key: 3 }, [H.value.style.chart.layout.wheel.ticks.type === "classic" ? (M(), O("g", Ee, [(M(!0), O(w, null, P($.value, (e) => (M(), O("g", null, [(M(!0), O(w, null, P(yt.value || [], (t) => (M(), O("line", {
					key: t.i,
					x1: t.x1,
					y1: t.y1 - e,
					x2: t.x2,
					y2: t.y2 - e,
					stroke: I(n)(t.color, .25 * e / 5),
					"stroke-width": H.value.style.chart.layout.wheel.ticks.strokeWidth / 360 * Math.min(W.value.width, W.value.height),
					"stroke-linecap": H.value.style.chart.layout.wheel.ticks.rounded ? "round" : "butt",
					class: A({
						"vue-ui-wheel-tick": !0,
						"vue-ui-tick-animated": H.value.style.chart.animation.use && !I(R) && t.i * Q.value <= q.value
					})
				}, null, 10, De))), 128))]))), 256)), (M(!0), O(w, null, P(yt.value || [], (e) => (M(), O("line", {
					key: e.i,
					x1: e.x1,
					y1: e.y1 - $.value,
					x2: e.x2,
					y2: e.y2 - $.value,
					stroke: e.color,
					"stroke-width": H.value.style.chart.layout.wheel.ticks.strokeWidth / 360 * Math.min(W.value.width, W.value.height),
					"stroke-linecap": H.value.style.chart.layout.wheel.ticks.rounded ? "round" : "butt",
					class: A({
						"vue-ui-wheel-tick": !0,
						"vue-ui-tick-animated": H.value.style.chart.animation.use && !I(R) && e.i * Q.value <= q.value
					})
				}, null, 10, Oe))), 128))])) : (M(), O("g", ke, [(M(!0), O(w, null, P($.value, (e) => (M(), O("g", null, [(M(!0), O(w, null, P(Tt.value(-e) || [], (e) => (M(), O("path", {
					key: e.i,
					d: e.d,
					fill: H.value.style.chart.layout.wheel.ticks.inactiveColor,
					stroke: H.value.style.chart.layout.wheel.ticks.stroke,
					"stroke-width": H.value.style.chart.layout.wheel.ticks.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					class: "vue-ui-wheel-tick"
				}, null, 8, Ae))), 128)), (M(!0), O(w, null, P(Tt.value(-e) || [], (t) => (M(), O("path", {
					key: t.i,
					d: t.d,
					fill: I(n)(t.fill, .5 * e / $.value),
					stroke: H.value.style.chart.layout.wheel.ticks.stroke,
					"stroke-width": H.value.style.chart.layout.wheel.ticks.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					class: A({
						"vue-ui-wheel-tick": !0,
						"vue-ui-tick-animated-3d": H.value.style.chart.animation.use && !I(R) && t.i * Q.value <= q.value
					})
				}, null, 10, je))), 128))]))), 256)), oe("g", null, [(M(!0), O(w, null, P(Tt.value(-$.value) || [], (e) => (M(), O("path", {
					key: e.i,
					d: e.d,
					fill: e.fill,
					stroke: H.value.style.chart.layout.wheel.ticks.stroke,
					"stroke-width": H.value.style.chart.layout.wheel.ticks.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					class: A({
						"vue-ui-wheel-tick": !0,
						"vue-ui-tick-animated-3d": H.value.style.chart.animation.use && !I(R) && e.i * Q.value <= q.value
					})
				}, null, 10, Me))), 128))])]))], 64)) : (M(), O(w, { key: 4 }, [H.value.style.chart.layout.wheel.ticks.type === "classic" ? (M(!0), O(w, { key: 0 }, P(Dt.value, (e, t) => (M(), O("line", {
					x1: e.x1,
					x2: e.x2,
					y1: e.y1,
					y2: e.y2,
					stroke: e.color,
					"stroke-width": H.value.style.chart.layout.wheel.ticks.strokeWidth / 360 * Math.min(W.value.width, W.value.height),
					"stroke-linecap": H.value.style.chart.layout.wheel.ticks.rounded ? "round" : "butt",
					class: A({
						"vue-ui-wheel-tick": !0,
						"vue-ui-tick-animated": H.value.style.chart.animation.use && !I(R) && t * Q.value <= q.value
					})
				}, null, 10, Ne))), 256)) : (M(!0), O(w, { key: 1 }, P(Ot.value, (e, t) => (M(), O("path", {
					d: e.arcSlice,
					fill: e.color,
					class: A({
						"vue-ui-wheel-tick": !0,
						"vue-ui-tick-animated": H.value.style.chart.animation.use && !I(R) && t * Q.value <= q.value
					}),
					stroke: H.value.style.chart.layout.wheel.ticks.stroke,
					"stroke-width": H.value.style.chart.layout.wheel.ticks.strokeWidth
				}, null, 10, Pe))), 256))], 64)),
				H.value.style.chart.layout.percentage.show ? (M(), O("g", {
					key: 5,
					role: "status",
					"aria-live": "polite",
					"aria-label": I(tt) ? "..." : Kt.value
				}, [I(tt) ? (M(), O("rect", {
					key: 0,
					x: K.value.centerX - 40,
					y: K.value.centerY - G.value / 2,
					width: 80,
					height: G.value,
					fill: "#6A6A6A80",
					rx: "3"
				}, null, 8, Ie)) : (M(), O("text", {
					key: 1,
					"aria-hidden": "true",
					x: K.value.centerX + H.value.style.chart.layout.percentage.offsetX,
					y: K.value.centerY + G.value / 3 + H.value.style.chart.layout.percentage.offsetY,
					"font-size": G.value,
					fill: H.value.style.chart.layout.wheel.ticks.gradient.show ? I(r)(H.value.style.chart.layout.wheel.ticks.activeColor, q.value / 100 * (H.value.style.chart.layout.wheel.ticks.gradient.shiftHueIntensity / 100)) : H.value.style.chart.layout.wheel.ticks.activeColor,
					"text-anchor": "middle",
					"font-weight": H.value.style.chart.layout.percentage.bold ? "bold" : "normal",
					style: { "font-variant-numeric": "tabluar-nums" },
					stroke: H.value.style.chart.layout.percentage.stroke,
					"stroke-width": H.value.style.chart.layout.percentage.strokeWidth,
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					"paint-order": "stroke fill",
					class: A({ "vue-ui-wheel-label": H.value.layout === "3d" })
				}, me(Kt.value), 11, Le))], 8, Fe)) : D("", !0),
				F(e.$slots, "svg", { svg: {
					...W.value,
					isPrintingImg: I(st) || I(ct) || I(zt),
					isPrintingSvg: I(Bt)
				} }, void 0, !0)
			], 14, be)),
			e.$slots.watermark ? (M(), O("div", Re, [F(e.$slots, "watermark", j(k({ isPrinting: I(st) || I(ct) || I(zt) || I(Bt) })), void 0, !0)])) : D("", !0),
			e.$slots.source ? (M(), O("div", {
				key: 5,
				ref_key: "source",
				ref: Xe,
				dir: "auto"
			}, [F(e.$slots, "source", {}, void 0, !0)], 512)) : D("", !0),
			F(e.$slots, "skeleton", {}, () => [I(tt) ? (M(), E(_, { key: 0 })) : D("", !0)], !0)
		], 46, ye));
	}
}, [["__scopeId", "data-v-8b3cd2bd"]]);
//#endregion
export { ve as n, ze as t };
