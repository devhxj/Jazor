import { computed as e, getCurrentInstance as t, getCurrentScope as n, nextTick as r, onMounted as i, onScopeDispose as a, shallowRef as o, toValue as s, unref as c, watch as l } from "vue";
//#region src/useMouseInElement.js
var u = typeof window < "u" ? window : void 0;
function d(e) {
	return Array.isArray(e) ? e : [e];
}
function f(e) {
	return Object.prototype.toString.call(e) === "[object Object]";
}
function p(e) {
	return e != null;
}
function m(e, n = !0, a) {
	a ?? t() ? i(e, a) : n ? e() : r(e);
}
function h(e, t) {
	return n() ? (a(e, t), !0) : !1;
}
function g(e) {
	let t = s(e);
	return t?.$el ?? t;
}
function _(t) {
	let n = o(!1);
	return m(() => {
		n.value = !0;
	}), e(() => (n.value, !!t()));
}
function v(...t) {
	let n = (e, t, n, r) => (e.addEventListener(t, n, r), () => {
		e.removeEventListener(t, n, r);
	}), r = e(() => {
		let e = d(s(t[0])).filter((e) => e != null);
		return e.every((e) => typeof e != "string") ? e : void 0;
	});
	return l(() => {
		let e = r.value;
		return [
			e?.map((e) => g(e)) ?? [u].filter((e) => e != null),
			d(s(e ? t[1] : t[0])),
			d(c(e ? t[2] : t[1])),
			s(e ? t[3] : t[2])
		];
	}, ([e, t, r, i], a, o) => {
		if (!e?.length || !t?.length || !r?.length) return;
		let s = f(i) ? { ...i } : i, c = e.flatMap((e) => t.flatMap((t) => r.map((r) => n(e, t, r, s))));
		o(() => {
			c.forEach((e) => e());
		});
	}, {
		immediate: !0,
		flush: "post"
	});
}
function y(t, n, r = {}) {
	let { window: i = u, ...a } = r, o, c = _(() => i && "ResizeObserver" in i), d = () => {
		o &&= (o.disconnect(), void 0);
	}, f = e(() => {
		let e = s(t);
		return Array.isArray(e) ? e.map((e) => g(e)) : [g(e)];
	}), p = l(f, (e) => {
		if (d(), c.value && i) {
			o = new i.ResizeObserver(n);
			for (let t of e) t && o.observe(t, a);
		}
	}, {
		immediate: !0,
		flush: "post"
	}), m = () => {
		d(), p();
	};
	return h(m), {
		isSupported: c,
		stop: m
	};
}
function b(t, n, r = {}) {
	let { window: i = u, ...a } = r, o, c = _(() => i && "MutationObserver" in i), f = () => {
		o &&= (o.disconnect(), void 0);
	}, m = e(() => {
		let e = d(s(t)).map((e) => g(e)).filter(p);
		return new Set(e);
	}), v = l(m, (e) => {
		f(), c.value && e.size && (o = new i.MutationObserver(n), e.forEach((e) => {
			o.observe(e, a);
		}));
	}, {
		immediate: !0,
		flush: "post"
	}), y = () => o?.takeRecords(), b = () => {
		v(), f();
	};
	return h(b), {
		isSupported: c,
		stop: b,
		takeRecords: y
	};
}
var x = {
	page: (e) => [e.pageX, e.pageY],
	client: (e) => [e.clientX, e.clientY],
	screen: (e) => [e.screenX, e.screenY],
	movement: (e) => typeof MouseEvent < "u" && e instanceof MouseEvent ? [e.movementX, e.movementY] : null
};
function S(e = {}) {
	let { type: t = "page", touch: n = !0, resetOnTouchEnds: r = !1, initialValue: i = {
		x: 0,
		y: 0
	}, window: a = u, target: s = a, scroll: c = !0, eventFilter: l } = e, d = null, f = 0, p = 0, m = o(i.x), h = o(i.y), g = o(null), _ = typeof t == "function" ? t : x[t], y = (e) => {
		let t = _(e);
		d = e, t && ([m.value, h.value] = t, g.value = "mouse"), a && (f = a.scrollX, p = a.scrollY);
	}, b = (e) => {
		if (e.touches.length > 0) {
			let t = _(e.touches[0]);
			t && ([m.value, h.value] = t, g.value = "touch");
		}
	}, S = () => {
		if (!d || !a) return;
		let e = _(d);
		typeof MouseEvent < "u" && d instanceof MouseEvent && e && (m.value = e[0] + a.scrollX - f, h.value = e[1] + a.scrollY - p);
	}, C = () => {
		m.value = i.x, h.value = i.y;
	}, w = l ? (e) => l(() => y(e), {}) : (e) => y(e), T = l ? (e) => l(() => b(e), {}) : (e) => b(e), E = l ? () => l(() => S(), {}) : () => S();
	if (s) {
		let e = { passive: !0 };
		v(s, ["mousemove", "dragover"], w, e), n && t !== "movement" && (v(s, ["touchstart", "touchmove"], T, e), r && v(s, "touchend", C, e)), c && t === "page" && v(a, "scroll", E, e);
	}
	return {
		x: m,
		y: h,
		sourceType: g
	};
}
function C(e, t = {}) {
	let { windowResize: n = !0, windowScroll: r = !0, handleOutside: i = !0, window: a = u } = t, s = t.type || "page", { x: c, y: d, sourceType: f } = S(t), p = o(e ?? a?.document.body), h = o(0), _ = o(0), x = o(0), C = o(0), w = o(0), T = o(0), E = o(!0);
	function D() {
		if (!a) return;
		let e = g(p);
		if (!(!e || !(e instanceof Element))) for (let t of e.getClientRects()) {
			let { left: e, top: n, width: r, height: o } = t;
			x.value = e + (s === "page" ? a.pageXOffset : 0), C.value = n + (s === "page" ? a.pageYOffset : 0), w.value = o, T.value = r;
			let l = c.value - x.value, u = d.value - C.value;
			if (E.value = r === 0 || o === 0 || l < 0 || u < 0 || l > r || u > o, (i || !E.value) && (h.value = l, _.value = u), !E.value) break;
		}
	}
	let O = [];
	function k() {
		O.forEach((e) => e()), O.length = 0;
	}
	if (m(() => {
		D();
	}), a) {
		let { stop: e } = y(p, D), { stop: t } = b(p, D, { attributeFilter: ["style", "class"] }), i = l([
			p,
			c,
			d
		], D);
		O.push(e, t, i), v(document, "mouseleave", () => {
			E.value = !0;
		}, { passive: !0 }), r && O.push(v("scroll", D, {
			capture: !0,
			passive: !0
		})), n && O.push(v("resize", D, { passive: !0 }));
	}
	return {
		x: c,
		y: d,
		sourceType: f,
		elementX: h,
		elementY: _,
		elementPositionX: x,
		elementPositionY: C,
		elementHeight: w,
		elementWidth: T,
		isOutside: E,
		stop: k
	};
}
//#endregion
//#region src/useTooltipPosition.js
function w(t) {
	let { elementX: n, elementWidth: r, isOutside: i } = C(e(() => {
		let e = s(t);
		return e ? e instanceof HTMLElement ? e : e.$el || null : null;
	}));
	return e(() => i.value || r.value === 0 ? "center" : n.value > r.value / 2 ? "left" : "right");
}
//#endregion
export { w as t };
