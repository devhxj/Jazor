import { Bt as e } from "./lib-Bttd6u5E.js";
import { Teleport as t, computed as n, createBlock as r, createCommentVNode as i, createElementBlock as a, createElementVNode as o, nextTick as s, normalizeClass as c, normalizeStyle as l, onMounted as u, onUnmounted as d, openBlock as f, ref as p, renderSlot as m, watch as h } from "vue";
//#region src/calcTooltipPosition.js
function g({ tooltip: e, chart: t, clientPosition: n, positionPreference: r = "center", defaultOffsetY: i = 24, defaultOffsetX: a = 0, blockShiftY: o = !1 }) {
	let s = p(a), c = p(i);
	if (e && t) {
		let { width: l, height: u } = e.getBoundingClientRect(), { right: d, left: f, bottom: p } = t.getBoundingClientRect();
		r === "center" && (s.value = n.x + l / 2 > d ? -l + (d - n.x) : n.x - l / 2 < f ? -l + (l - (n.x - f)) : -l / 2), r === "right" && (s.value = n.x + l + a > d ? -l + (d - n.x) : a), r === "left" && (s.value = n.x - l - a < f ? f - n.x : -l - a), n.y + u > p && !o && (c.value = -u - i);
	}
	return {
		top: n.y + c.value,
		left: n.x + s.value
	};
}
//#endregion
//#region src/event.js
function _(e, t, n) {
	u(() => e.addEventListener(t, n)), d(() => e.removeEventListener(t, n));
}
//#endregion
//#region src/useMouse.js
function v() {
	let e = p(0), t = p(0);
	return _(window, "mousemove", (n) => {
		e.value = n.clientX, t.value = n.clientY;
	}), {
		x: e,
		y: t
	};
}
//#endregion
//#region src/atoms/Tooltip.vue
var y = ["aria-hidden"], b = ["innerHTML"], x = {
	__name: "Tooltip",
	props: {
		teleportTo: {
			type: String,
			default: "body"
		},
		backgroundColor: {
			type: String,
			default: "#FFFFFF"
		},
		color: {
			type: String,
			default: "#000000"
		},
		content: String,
		maxWidth: {
			type: String,
			default: "300px"
		},
		parent: { type: Object },
		show: {
			type: Boolean,
			default: !1
		},
		isCustom: {
			type: Boolean,
			default: !1
		},
		fontSize: {
			type: [Number, String],
			default: 14
		},
		borderRadius: {
			type: Number,
			default: 4
		},
		borderColor: {
			type: String,
			default: "#e1e5e8"
		},
		borderWidth: {
			type: Number,
			default: 1
		},
		backgroundOpacity: {
			type: Number,
			default: 100
		},
		position: {
			type: String,
			default: "center"
		},
		offsetY: {
			type: Number,
			default: 24
		},
		offsetX: {
			type: Number,
			default: 0
		},
		blockShiftY: {
			type: Boolean,
			default: !1
		},
		isFullscreen: {
			type: Boolean,
			default: !1
		},
		smooth: {
			type: Boolean,
			default: !0
		},
		backdropFilter: {
			type: Boolean,
			default: !0
		},
		smoothForce: {
			type: Number,
			default: .18
		},
		smoothSnapThreshold: {
			type: Number,
			default: .25
		},
		isA11yMode: {
			type: Boolean,
			default: !1
		},
		a11yPosition: {
			type: Object,
			default: null
		}
	},
	setup(u, { expose: _ }) {
		let x = u, S = p(null), { x: C, y: w } = v(x.parent), T = p({
			x: 0,
			y: 0
		}), E = p({
			x: 0,
			y: 0
		}), D = p(null), O = p(null), k = p({
			scaleX: 1,
			scaleY: 1
		}), A = p(null);
		function j() {
			let e = N(S.value);
			if (D.value = e, !e) {
				O.value = null, k.value = {
					scaleX: 1,
					scaleY: 1
				}, A.value = null;
				return;
			}
			O.value = e.getBoundingClientRect(), k.value = P(e), A.value = x.parent?.getBoundingClientRect?.() || null;
		}
		function M(e) {
			return e ? !!(e.transform && e.transform !== "none" || e.perspective && e.perspective !== "none" || e.filter && e.filter !== "none" || e.backdropFilter && e.backdropFilter !== "none" || e.contain && e.contain.includes("paint") || e.willChange && (e.willChange.includes("transform") || e.willChange.includes("filter"))) : !1;
		}
		function N(e) {
			let t = e?.parentElement || null;
			for (; t && t !== document.documentElement;) {
				if (M(getComputedStyle(t))) return t;
				t = t.parentElement;
			}
			return null;
		}
		function P(e) {
			if (!e) return {
				scaleX: 1,
				scaleY: 1
			};
			let t = getComputedStyle(e).transform;
			if (!t || t === "none") return {
				scaleX: 1,
				scaleY: 1
			};
			try {
				let e = new DOMMatrixReadOnly(t), n = Math.hypot(e.a, e.b), r = Math.hypot(e.c, e.d);
				return {
					scaleX: n || 1,
					scaleY: r || 1
				};
			} catch {
				return {
					scaleX: 1,
					scaleY: 1
				};
			}
		}
		function F({ x: e, y: t }) {
			x.isA11yMode || (C.value = e, w.value = t);
		}
		let I = null;
		function L() {
			if (!x.show) {
				z();
				return;
			}
			if (!x.smooth) {
				E.value.x = T.value.x, E.value.y = T.value.y, z();
				return;
			}
			let e = T.value.x - E.value.x, t = T.value.y - E.value.y;
			if (Math.abs(e) <= x.smoothSnapThreshold && Math.abs(t) <= x.smoothSnapThreshold) {
				E.value.x = T.value.x, E.value.y = T.value.y, z();
				return;
			}
			E.value.x += e * x.smoothForce, E.value.y += t * x.smoothForce, I = requestAnimationFrame(L);
		}
		function R() {
			I == null && x.show && x.smooth && (I = requestAnimationFrame(L));
		}
		function z() {
			I != null && (cancelAnimationFrame(I), I = null);
		}
		h([C, w], ([e, t]) => {
			T.value.x = e, T.value.y = t, x.smooth ? R() : (E.value.x = e, E.value.y = t);
		}), h(() => x.show, async (e) => {
			if (!e) {
				z();
				return;
			}
			let t = C.value, n = w.value;
			T.value.x = t, T.value.y = n, E.value.x = t, E.value.y = n, await s(), j(), R();
		}), d(() => {
			z();
		});
		let B = n(() => x.isA11yMode && x.a11yPosition && typeof x.a11yPosition.x == "number" && typeof x.a11yPosition.y == "number" ? {
			x: x.a11yPosition.x,
			y: x.a11yPosition.y
		} : {
			x: E.value.x,
			y: E.value.y
		}), V = n(() => {
			let e = O.value;
			if (!e) return {
				x: B.value.x,
				y: B.value.y
			};
			let { scaleX: t, scaleY: n } = k.value;
			return {
				x: (B.value.x - e.left) / t,
				y: (B.value.y - e.top) / n
			};
		}), H = n(() => {
			let e = O.value, t = A.value;
			if (!e || !t) return x.parent;
			let { scaleX: n, scaleY: r } = k.value;
			return {
				...x.parent,
				getBoundingClientRect() {
					let i = t;
					return {
						left: (i.left - e.left) / n,
						top: (i.top - e.top) / r,
						right: (i.right - e.left) / n,
						bottom: (i.bottom - e.top) / r,
						width: i.width / n,
						height: i.height / r,
						x: (i.left - e.left) / n,
						y: (i.top - e.top) / r
					};
				}
			};
		}), U = n(() => {
			let e = g({
				tooltip: S.value,
				chart: H.value,
				clientPosition: V.value,
				positionPreference: x.position,
				defaultOffsetX: x.offsetX,
				defaultOffsetY: x.offsetY,
				blockShiftY: x.blockShiftY
			});
			return {
				top: Math.round(e.top),
				left: Math.round(e.left)
			};
		}), W = n(() => e(x.backgroundColor, x.backgroundOpacity)), G = n(() => {
			let e = {
				pointerEvents: "none",
				position: "fixed",
				top: "0px",
				left: "0px",
				transform: `translate3d(${U.value.left}px, ${U.value.top}px, 0)`,
				borderRadius: `${x.borderRadius}px`,
				border: `${x.borderWidth}px solid ${x.borderColor}`,
				zIndex: 2147483647
			};
			return x.isCustom || Object.assign(e, {
				background: W.value,
				color: x.color,
				maxWidth: x.maxWidth,
				fontSize: `${x.fontSize}px`
			}), e;
		});
		return _({ placeTooltip: F }), (e, n) => (f(), r(t, { to: u.isFullscreen ? u.parent : u.teleportTo }, [u.show ? (f(), a("div", {
			key: 0,
			ref_key: "tooltip",
			ref: S,
			role: "tooltip",
			"aria-hidden": !u.show,
			"aria-live": "polite",
			class: c({
				"vue-data-ui-custom-tooltip": u.isCustom,
				"vue-data-ui-tooltip": !u.isCustom,
				"vue-data-ui-tooltip-backdrop": u.backdropFilter
			}),
			style: l(G.value)
		}, [
			m(e.$slots, "tooltip-before"),
			m(e.$slots, "default"),
			m(e.$slots, "tooltip", {}, () => [o("div", { innerHTML: u.content }, null, 8, b)]),
			m(e.$slots, "tooltip-after")
		], 14, y)) : i("", !0)], 8, ["to"]));
	}
};
//#endregion
export { x as default };
