import { nextTick as e } from "vue";
//#region src/useFitSvgText.js
function t({ svgRef: t, unitWidth: n, fontSize: r = 12, step: i = .5, maxIterations: a = 60, hideUnderMin: o = !0 }) {
	let s = /* @__PURE__ */ new WeakMap(), c = (e) => {
		if (typeof e == "number") return e;
		if (!e) return NaN;
		let t = parseFloat(String(e).replace("px", ""));
		return Number.isFinite(t) ? t : NaN;
	}, l = (e) => {
		let t = c(e.getAttribute("font-size"));
		if (Number.isFinite(t)) return t;
		let n = window.getComputedStyle(e);
		return c(n.fontSize) || r;
	}, u = (e, t) => {
		e.setAttribute("font-size", String(t));
	}, d = (e) => {
		if (s.has(e)) u(e, s.get(e));
		else {
			let t = l(e);
			s.set(e, t), u(e, t);
		}
	}, f = (e) => {
		try {
			return e.getBBox().width || 0;
		} catch {
			return 0;
		}
	};
	return { fitText: async (r, s = 6) => {
		await e();
		let c = t?.value;
		if (!c) return;
		let p = c.querySelectorAll(r);
		if (!p.length) return;
		let m = Math.max(0, n.value);
		if (m <= 0) return;
		let h = [];
		p.forEach((e) => {
			let t = e.style.display, n = e.style.opacity;
			e.style.display = "", e.style.opacity = "0", d(e);
			let r = f(e), o = l(e), c = o, p = 0;
			if (r <= m) h.push({
				el: e,
				finalSize: o,
				fits: !0
			});
			else {
				for (; r > m && c > s && p < a;) c = Math.max(s, c - i), u(e, c), r = f(e), p += 1;
				let t = r <= m && c > s;
				h.push({
					el: e,
					finalSize: c,
					fits: t
				}), u(e, o);
			}
			e.style.display = t, e.style.opacity = n;
		});
		let g = o && h.some((e) => !e.fits);
		h.forEach(({ el: e, finalSize: t, fits: n }) => {
			g ? e.style.display = "none" : (e.style.display = "", u(e, t));
		});
	} };
}
//#endregion
export { t };
