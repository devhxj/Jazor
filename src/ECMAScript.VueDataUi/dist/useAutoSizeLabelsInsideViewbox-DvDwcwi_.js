import { isRef as e, onBeforeUnmount as t } from "vue";
//#region src/useAutoSizeLabelsInsideViewbox.js
function n(t) {
	return typeof t == "function" ? t() : e(t) ? t.value : t;
}
function r(e) {
	let t = String(e.getAttribute("viewBox") ?? "").trim().split(/[\s,]+/).map(Number);
	if (t.length !== 4 || t.some((e) => !Number.isFinite(e))) return null;
	let [n, r, i, a] = t;
	return {
		x: n,
		y: r,
		width: i,
		height: a
	};
}
function i(e, t) {
	let n = e.getBBox(), r = e.getScreenCTM?.(), i = t.getScreenCTM?.();
	if (!r || !i || !t.createSVGPoint) return n;
	let a;
	try {
		a = i.inverse();
	} catch {
		return n;
	}
	function o(e, n) {
		let i = t.createSVGPoint();
		return i.x = e, i.y = n, i.matrixTransform(r).matrixTransform(a);
	}
	let s = [
		o(n.x, n.y),
		o(n.x + n.width, n.y),
		o(n.x, n.y + n.height),
		o(n.x + n.width, n.y + n.height)
	], c = s.map((e) => e.x), l = s.map((e) => e.y), u = Math.min(...c), d = Math.max(...c), f = Math.min(...l), p = Math.max(...l);
	return {
		x: u,
		y: f,
		width: d - u,
		height: p - f
	};
}
function a(e, t, n, r = 1) {
	let a = i(e, t);
	return a.x >= n.x + r && a.x + a.width <= n.x + n.width - r && a.y >= n.y + r && a.y + a.height <= n.y + n.height - r;
}
function o({ el: e, svg: t, bounds: n, baseSize: r, minSize: i, padding: o, step: s = .5, attempts: c = 240 }) {
	let l = Number(r), u = Number(i);
	if (!Number.isFinite(l) || l <= 0) return 0;
	let d = Number.isFinite(u) ? Math.min(l, Math.max(0, u)) : Math.min(l, 6), f = l, p = c;
	for (e.setAttribute("font-size", String(f)); f > d && p > 0 && !a(e, t, n, o);) f = Math.max(d, f - s), e.setAttribute("font-size", String(f)), --p;
	return f;
}
function s(e) {
	let t = e.cloneNode(!0);
	return t.removeAttribute("id"), t.setAttribute("aria-hidden", "true"), t.setAttribute("focusable", "false"), t.classList.remove("vue-data-ui-transition"), t.classList.remove("vue-ui-onion-label"), t.style.setProperty("transition", "none", "important"), t.style.setProperty("animation", "none", "important"), t.style.setProperty("opacity", "0", "important"), t.style.setProperty("pointer-events", "none", "important"), t.style.setProperty("user-select", "none", "important"), t;
}
function c({ el: e, svg: t, bounds: n, baseSize: r, minSize: i, padding: a }) {
	let c = s(e), l = e.parentNode;
	if (!l) return Number(r) || 0;
	l.insertBefore(c, e.nextSibling);
	try {
		return o({
			el: c,
			svg: t,
			bounds: n,
			baseSize: r,
			minSize: i,
			padding: a
		});
	} finally {
		c.remove();
	}
}
function l({ svgRef: e, fontSize: i, minFontSize: a, sizeRef: o, labelClass: s, labelTypes: l = [], padding: u = 1 }) {
	let d = null;
	function f() {
		let e = n(l);
		return Array.isArray(e) && e.length ? e : [{
			selector: n(s),
			baseSize: i,
			minSize: a,
			sizeRef: o
		}];
	}
	function p() {
		let t = n(e);
		if (!t) return;
		let i = r(t);
		i && f().forEach((e) => {
			let r = n(e.selector);
			if (!r) return;
			let a = Array.from(t.querySelectorAll(r));
			if (!a.length) return;
			let s = n(e.baseSize), l = n(e.minSize), d = e.sizeRef ?? o, f = Infinity;
			a.forEach((e) => {
				let n = c({
					el: e,
					svg: t,
					bounds: i,
					baseSize: s,
					minSize: l,
					padding: u
				});
				e.setAttribute("font-size", String(n)), f = Math.min(f, n);
			}), d && "value" in d && Number.isFinite(f) && (d.value = f);
		});
	}
	function m() {
		d !== null && cancelAnimationFrame(d), d = requestAnimationFrame(() => {
			d = null, p();
		});
	}
	return t(() => {
		d !== null && (cancelAnimationFrame(d), d = null);
	}), {
		autoSizeLabels: m,
		autoSizeLabelsNow: p
	};
}
//#endregion
export { l as t };
