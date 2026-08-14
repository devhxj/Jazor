import { computed as e, onMounted as t, onUnmounted as n, ref as r, watch as i, watchEffect as a } from "vue";
//#region src/usePanZoom.js
function o(o, s = {
	x: 0,
	y: 0,
	width: 100,
	height: 100
}, c = 1, l, u = null) {
	let d = r({ ...s }), f = r({ ...d.value }), p = r(1), m = r(!1), h = r(!1), g = r(0), _ = r(null), v = r(1), y = r({
		x: 0,
		y: 0
	}), b = e(() => f.value.x !== d.value.x || f.value.y !== d.value.y || f.value.width !== d.value.width || f.value.height !== d.value.height), x = null;
	function S(e) {
		if (e.length < 2) return 0;
		let t = e[0].clientX - e[1].clientX, n = e[0].clientY - e[1].clientY;
		return Math.hypot(t, n);
	}
	function C(e) {
		let t = o.value;
		if (!t) return {
			x: 0,
			y: 0
		};
		let n = t.createSVGPoint();
		n.x = e.clientX, n.y = e.clientY;
		let r = t.getScreenCTM()?.inverse();
		return r ? n.matrixTransform(r) : {
			x: 0,
			y: 0
		};
	}
	function w(e, t) {
		let n = o.value;
		if (!n) return {
			dx: 0,
			dy: 0
		};
		let r = Math.max(1, n.clientWidth), i = Math.max(1, n.clientHeight), a = f.value.width / r, s = f.value.height / i;
		return {
			dx: e * a,
			dy: t * s
		};
	}
	function T(e) {
		m.value = !0;
		let t = e.touches?.[0] || e;
		y.value = {
			x: t.clientX,
			y: t.clientY
		}, o.value && (o.value.style.cursor = "grabbing");
	}
	function E(e) {
		if (!m.value) return;
		let t = e.touches?.[0] || e, n = t.clientX - y.value.x, r = t.clientY - y.value.y;
		if (n === 0 && r === 0) return;
		let { dx: i, dy: a } = w(n, r);
		f.value.x -= i, f.value.y -= a, y.value = {
			x: t.clientX,
			y: t.clientY
		};
	}
	function D() {
		m.value = !1, o.value && (o.value.style.cursor = "");
	}
	function O(e) {
		u && u(), e.preventDefault(), j(e.deltaY > 0 ? .9 : 1.1, C(e));
	}
	function k(e) {
		e.preventDefault();
		let t = C(e);
		A(1.5 * (1 + c / 100), t);
	}
	function A(e, t) {
		x && cancelAnimationFrame(x);
		let n = p.value, r = n * e, i = null, a = (e) => e < .5 ? 2 * e * e : -1 + (4 - 2 * e) * e, o = (e) => {
			i ??= e;
			let s = Math.min((e - i) / 200, 1);
			j((n + (r - n) * a(s)) / p.value, t), x = s < 1 ? requestAnimationFrame(o) : null;
		};
		requestAnimationFrame(o);
	}
	function j(e, t) {
		let n = p.value * e, r = n / p.value, i = f.value.width / r, a = f.value.height / r, o = (t.x - f.value.x) * (1 - 1 / r), s = (t.y - f.value.y) * (1 - 1 / r);
		f.value.x += o, f.value.y += s, f.value.width = i, f.value.height = a, p.value = n;
	}
	function M(e) {
		e.touches.length === 2 ? (h.value = !0, g.value = S(e.touches), _.value = { ...f.value }, v.value = p.value) : (e.preventDefault(), T(e));
	}
	function N(e) {
		if (h.value && e.touches.length === 2) {
			e.preventDefault();
			let t = S(e.touches);
			if (!g.value || !t) return;
			let n = t / g.value, r = v.value * n / p.value, i = C({
				clientX: (e.touches[0].clientX + e.touches[1].clientX) / 2,
				clientY: (e.touches[0].clientY + e.touches[1].clientY) / 2
			});
			f.value = { ..._.value }, p.value = v.value, j(r, i);
		} else e.preventDefault(), E(e);
	}
	function P(e) {
		e.touches.length < 2 && (h.value = !1), D();
	}
	function F(e = !1) {
		if (!e) {
			f.value = { ...d.value }, p.value = 1;
			return;
		}
		let t = { ...f.value }, n = p.value, r = null, i = (e) => {
			r ??= e;
			let a = Math.min((e - r) / 300, 1);
			f.value = {
				x: t.x + (d.value.x - t.x) * a,
				y: t.y + (d.value.y - t.y) * a,
				width: t.width + (d.value.width - t.width) * a,
				height: t.height + (d.value.height - t.height) * a
			}, p.value = n + (1 - n) * a, a < 1 && requestAnimationFrame(i);
		};
		requestAnimationFrame(i);
	}
	function I(e, t = {}) {
		let { overwriteCurrentIfNotZoomed: n = !0 } = t, r = f.value.x !== d.value.x || f.value.y !== d.value.y || f.value.width !== d.value.width || f.value.height !== d.value.height;
		d.value = { ...e }, !r && n && (f.value = { ...d.value }, p.value = 1);
	}
	function L() {
		let e = o.value;
		e && (e.addEventListener("mousedown", T), e.addEventListener("mousemove", E), e.addEventListener("mouseup", D), e.addEventListener("mouseleave", D), e.addEventListener("wheel", O, { passive: !1 }), e.addEventListener("dblclick", k), e.addEventListener("touchstart", M, { passive: !1 }), e.addEventListener("touchmove", N, { passive: !1 }), e.addEventListener("touchend", P), e.addEventListener("touchcancel", P));
	}
	function R() {
		let e = o.value;
		e && (e.removeEventListener("mousedown", T), e.removeEventListener("mousemove", E), e.removeEventListener("mouseup", D), e.removeEventListener("mouseleave", D), e.removeEventListener("wheel", O), e.removeEventListener("dblclick", k), e.removeEventListener("touchstart", M), e.removeEventListener("touchmove", N), e.removeEventListener("touchend", P), e.removeEventListener("touchcancel", P));
	}
	t(() => {
		(!l || l.value) && L();
	}), n(R), a(() => {
		if (l) return l.value ? L() : R(), () => R();
	}), i(o, () => {
		R(), (!l || l.value) && L();
	});
	function z(e, t = !1) {
		let n = {
			x: f.value.x + f.value.width / 2,
			y: f.value.y + f.value.height / 2
		};
		t ? A(e, n) : j(e, n);
	}
	return {
		viewBox: f,
		resetZoom: F,
		isZoom: b,
		setInitialViewBox: I,
		scale: p,
		zoomByFactor: z
	};
}
//#endregion
export { o as t };
