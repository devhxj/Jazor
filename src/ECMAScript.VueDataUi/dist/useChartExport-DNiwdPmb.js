import { isRef as e, nextTick as t, onMounted as n, ref as r, unref as i, watch as a } from "vue";
//#region src/useSvgExport.js
var o = "http://www.w3.org/2000/svg", s = "http://www.w3.org/1999/xlink", c = "system-ui, -apple-system, \"Segoe UI\", Roboto, \"Helvetica Neue\", Arial, \"Noto Sans\", \"Liberation Sans\", sans-serif", l = /* @__PURE__ */ new WeakMap();
function u(e) {
	let t = l.get(e);
	return t || (t = getComputedStyle(e), l.set(e, t)), t;
}
function d() {
	l = /* @__PURE__ */ new WeakMap();
}
function f(e) {
	return document.createElementNS(o, e);
}
function p(e) {
	return `${e}, ${c}`;
}
function m({ svg: c, title: l, legend: m, legendItems: _, backgroundColor: v, stretchTitle: y = !1, titleEmbedded: b = !1 }) {
	let x = e(c) ? c : r(c), S = r(!1), C = /* @__PURE__ */ "fill.fill-opacity.stroke.stroke-width.stroke-opacity.opacity.color.stroke-linecap.stroke-linejoin.stroke-miterlimit.stroke-dasharray.stroke-dashoffset.font-family.font-size.font-style.font-weight.font-variant.letter-spacing.word-spacing.text-anchor.dominant-baseline.shape-rendering.vector-effect.paint-order.mix-blend-mode.isolation.filter.clip-path.mask.transform.transform-origin.visibility.display".split("."), w = /* @__PURE__ */ new Set(["font-family"]);
	async function T({ timeoutMs: e = 4e3, log: n = "warn" } = {}) {
		if (typeof window > "u" || typeof document > "u") return console.warn("SVG export only works in the browser."), null;
		let r = performance.now() + e, a = () => {
			let e = i(x);
			return e && document.contains(e) ? e : null;
		}, o = a();
		if (o) return o;
		let s = !1, c = (e) => {
			if (!s && e) try {
				e.disconnect();
			} catch {}
			s = !0;
		}, l, u = new Promise((e) => {
			try {
				l = new MutationObserver(() => {
					let t = a();
					t && (c(l), e(t));
				}), l.observe(document.documentElement, {
					childList: !0,
					subtree: !0
				});
			} catch {
				e(null);
			}
		});
		for (; performance.now() < r;) {
			let e = await Promise.race([u, new Promise((e) => requestAnimationFrame(e))]);
			if (await t(), o = a() || e, o) return c(l), o;
		}
		return c(l), console.warn("SVG element not found or not mounted (timed out)."), null;
	}
	n(async () => {
		try {
			await T(), S.value = !0;
		} catch {
			S.value = !1;
		}
	}), a(x, async () => {
		try {
			await T(), S.value = !0;
		} catch {
			S.value = !1;
		}
	});
	let E = /* @__PURE__ */ new Map();
	function D(e) {
		if (E.has(e)) return E.get(e);
		let t = f("svg");
		t.setAttribute("width", "0"), t.setAttribute("height", "0"), t.style.position = "absolute", t.style.left = "-99999px", t.style.top = "-99999px";
		let n = f(e);
		t.appendChild(n), document.body.appendChild(t);
		let r = window.getComputedStyle(n), i = {};
		for (let e of C) i[e] = r.getPropertyValue(e);
		return document.body.removeChild(t), E.set(e, i), i;
	}
	function O(e) {
		let t = u(e), n = D(e.tagName.toLowerCase()), r = [];
		for (let e of C) {
			let i = t.getPropertyValue(e) || "", a = n[e] || "";
			if (w.has(e)) {
				e === "font-family" && i && (i = p(i)), i && r.push(`${e}:${i};`);
				continue;
			}
			i && i !== a && r.push(`${e}:${i};`);
		}
		return r.join("");
	}
	function k(e, t) {
		e.getAttribute("width") && t.setAttribute("width", e.getAttribute("width")), e.getAttribute("height") && t.setAttribute("height", e.getAttribute("height")), e.getAttribute("viewBox") && t.setAttribute("viewBox", e.getAttribute("viewBox"));
		let n = [e, ...Array.from(e.querySelectorAll("*"))], r = [t, ...Array.from(t.querySelectorAll("*"))];
		for (let e = 0; e < n.length; e += 1) {
			let t = n[e], i = r[e], a = O(t);
			a && i.setAttribute("style", a), [
				"fill",
				"stroke",
				"filter",
				"clip-path",
				"mask"
			].forEach((e) => {
				let n = t.getAttribute(e);
				n && i.setAttribute(e, n);
			});
		}
	}
	function A(e) {
		let t = e.getAttribute("viewBox");
		if (!t) return null;
		let [n, r, i, a] = t.split(/\s+|,/).map(Number);
		return [
			n,
			r,
			i,
			a
		].some((e) => Number.isNaN(e)) ? null : {
			minX: n,
			minY: r,
			width: i,
			height: a
		};
	}
	function j(e) {
		e.getAttribute("xmlns") || e.setAttribute("xmlns", o), e.getAttribute("xmlns:xlink") || e.setAttribute("xmlns:xlink", s), e.getAttribute("xml:space") || e.setAttribute("xml:space", "preserve");
	}
	function M(e) {
		let t = document.createElement("div");
		return t.style.position = "absolute", t.style.left = "-99999px", t.style.top = "-99999px", t.style.width = "0", t.style.height = "0", document.body.appendChild(t), t.appendChild(e), t;
	}
	function N(e) {
		try {
			e.remove();
		} catch {}
	}
	function P(e, t, n, r, i) {
		if (!l.value || !l.value.text) return 0;
		let a = {
			text: l.value.text || "",
			color: l.value.color || "#111",
			fontSize: l.value.fontSize || 20,
			bold: !!l.value.bold,
			textAlign: (l.value?.textAlign || "center").toLowerCase(),
			paddingLeft: l.value.paddingLeft ?? 0,
			paddingRight: l.value.paddingRight ?? 0,
			subtitle: l.value.subtitle || null
		}, o = f("g");
		o.setAttribute("id", "__vdu_export_title"), e.appendChild(o);
		let s = n + a.paddingLeft, c = n + t - (a.paddingRight || 0), u = n + t / 2, d = "start", p = s;
		a.textAlign === "center" ? (d = "middle", p = u) : a.textAlign === "right" && (d = "end", p = c);
		let m = r, h = Math.round(a.fontSize * .4), g = f("text");
		g.setAttribute("x", String(p)), g.setAttribute("y", String(m + a.fontSize)), g.setAttribute("text-anchor", d), g.setAttribute("dominant-baseline", "ideographic"), g.setAttribute("style", `font-family:${i}; font-size:${a.fontSize}px; font-weight:${a.bold ? "700" : "400"}; fill:${a.color}; ${y ? "transform: scale(0.65, 1); transform-origin: center;" : ""}`), g.textContent = a.text, o.appendChild(g);
		let _ = a.fontSize;
		if (a.subtitle && a.subtitle.text) {
			let e = {
				color: a.subtitle.color || "#666",
				text: a.subtitle.text,
				fontSize: a.subtitle.fontSize || Math.max(12, Math.round(a.fontSize * .8)),
				bold: !!a.subtitle.bold
			}, t = f("text");
			t.setAttribute("x", String(p)), t.setAttribute("y", String(m + a.fontSize + h + e.fontSize)), t.setAttribute("text-anchor", d), t.setAttribute("dominant-baseline", "ideographic"), t.setAttribute("style", `font-family:${i}; font-size:${e.fontSize}px; font-weight:${e.bold ? "600" : "400"}; fill:${e.color}; ${y ? "transform: scale(0.65, 1); transform-origin: center;" : ""}`), t.textContent = e.text, o.appendChild(t), _ += h + e.fontSize;
		}
		return _ + Math.round(a.fontSize * .4);
	}
	function F(e, t, n, r, i) {
		if (!m.value || !m.value.show || !Array.isArray(_.value) || !_.value.length) return 0;
		let a = {
			bold: !!m.value.bold,
			backgroundColor: m.value.backgroundColor || "transparent",
			color: m.value.color || "#111",
			fontSize: m.value.fontSize || 14,
			paddingX: 14,
			paddingY: 10,
			itemGapX: 18,
			itemGapY: 12,
			markerSize: Math.max(8, Math.round((m.value.fontSize || 14) * .9)),
			markerTextGap: 8,
			lineHeight: Math.round((m.value.fontSize || 14) * 1.2),
			maxWidth: t
		}, o = f("g");
		o.setAttribute("id", "__vdu_export_legend"), o.setAttribute("transform", `translate(${n}, ${r})`), e.appendChild(o);
		let s = f("rect");
		s.setAttribute("rx", "6"), s.setAttribute("ry", "6"), s.setAttribute("fill", a.backgroundColor), o.appendChild(s);
		let c = f("g");
		o.appendChild(c);
		let l = f("text");
		l.setAttribute("x", "0"), l.setAttribute("y", "0"), l.setAttribute("style", `font-family:${i}; font-size:${a.fontSize}px; font-weight:${a.bold ? "600" : "400"}; opacity:0;`), c.appendChild(l);
		let u = (e) => (l.textContent = e || "", l.getComputedTextLength()), d = Math.max(1, a.maxWidth - a.paddingX * 2);
		function p(e, t) {
			let n = String(e || "").split(/\s+/).filter(Boolean), r = [];
			if (!n.length) return [""];
			let i = n.shift();
			for (; n.length;) {
				let e = `${i} ${n[0]}`;
				if (u(e) <= t) i = e, n.shift();
				else if (u(i) > t && i.length > 1) {
					let e = "";
					for (let n of i) {
						let r = e + n;
						if (u(r) <= t) e = r;
						else break;
					}
					let a = i.slice(e.length);
					r.push(e), i = a.length ? a : n.shift() || "";
				} else r.push(i), i = n.shift() || "";
			}
			i && r.push(i);
			let a = r[r.length - 1] || "";
			if (u(a) > t && a.length > 1) {
				let e = "";
				for (let n of a) {
					let r = e + n;
					if (u(r) <= t) e = r;
					else break;
				}
				let n = a.slice(e.length);
				r[r.length - 1] = e, n && r.push(n);
			}
			return r;
		}
		let h = Math.PI / 180;
		function g(e, t, n, r, i = -90) {
			let a = i * h, o = "";
			for (let i = 0; i < n; i += 1) {
				let s = a + i * 2 * Math.PI / n, c = e + Math.cos(s) * r, l = t + Math.sin(s) * r;
				o += (i === 0 ? "M" : "L") + c + "," + l;
			}
			return o + "Z";
		}
		function v(e, t, n = 5, r, i) {
			let a = -90 * h, o = "";
			for (let s = 0; s < n * 2; s += 1) {
				let c = s % 2 == 0 ? r : i, l = a + s * Math.PI / n, u = e + Math.cos(l) * c, d = t + Math.sin(l) * c;
				o += (s === 0 ? "M" : "L") + u + "," + d;
			}
			return o + "Z";
		}
		let y = _.value.map((e) => {
			let t = Math.max(1, d - (a.markerSize + a.markerTextGap)), n = p(e.name || "", t), r = Math.max(...n.map(u), 0), i = a.markerSize + a.markerTextGap + r, o = Math.max(a.markerSize, n.length * a.lineHeight);
			return {
				...e,
				lines: n,
				itemW: i,
				itemH: o
			};
		}), b = [], x = {
			items: [],
			width: 0,
			height: 0
		};
		for (let e of y) {
			let t = (x.items.length ? a.itemGapX : 0) + e.itemW;
			x.items.length && x.width + t > d ? (b.push(x), x = {
				items: [e],
				width: e.itemW,
				height: e.itemH
			}) : (x.items.push(e), x.width += t, x.height = Math.max(x.height, e.itemH));
		}
		x.items.length && b.push(x);
		let S = a.paddingY;
		for (let e of b) {
			let t = Math.max(a.paddingX, (a.maxWidth - e.width) / 2);
			for (let n = 0; n < e.items.length; n += 1) {
				let r = e.items[n], o = t + a.markerSize / 2, s = S + e.height / 2.5, l = a.markerSize / 2, u = String(r.shape || "rect").toLowerCase();
				if (u === "circle") {
					let e = f("circle");
					e.setAttribute("cx", String(o)), e.setAttribute("cy", String(s)), e.setAttribute("r", String(l * .8)), e.setAttribute("fill", r.color || "#000"), c.appendChild(e);
				} else if (u === "rect" || u === "square") {
					let e = f("path");
					e.setAttribute("d", g(o, s, 4, l, -45)), e.setAttribute("fill", r.color || "#000"), c.appendChild(e);
				} else if (u === "diamond") {
					let e = f("path");
					e.setAttribute("d", g(o, s, 4, l, 45)), e.setAttribute("fill", r.color || "#000"), c.appendChild(e);
				} else if (u === "triangle") {
					let e = f("path");
					e.setAttribute("d", g(o, s, 3, l, -90)), e.setAttribute("fill", r.color || "#000"), c.appendChild(e);
				} else if (u === "pentagon") {
					let e = f("path");
					e.setAttribute("d", g(o, s, 5, l, -90)), e.setAttribute("fill", r.color || "#000"), c.appendChild(e);
				} else if (u === "hexagon") {
					let e = f("path");
					e.setAttribute("d", g(o, s, 6, l, 0)), e.setAttribute("fill", r.color || "#000"), c.appendChild(e);
				} else if (u === "star") {
					let e = f("path"), t = l, n = l * .5;
					e.setAttribute("d", v(o, s, 5, t, n)), e.setAttribute("fill", r.color || "#000"), c.appendChild(e);
				} else {
					let n = f("rect");
					n.setAttribute("x", String(t)), n.setAttribute("y", String(S + (e.height - a.markerSize) / 2)), n.setAttribute("width", String(a.markerSize)), n.setAttribute("height", String(a.markerSize)), n.setAttribute("fill", r.color || "#000"), c.appendChild(n);
				}
				let d = t + a.markerSize + a.markerTextGap, p = S + (e.height - r.lines.length * a.lineHeight) / 2 + a.fontSize, m = f("text");
				m.setAttribute("x", String(d)), m.setAttribute("y", String(p)), m.setAttribute("style", `font-family:${i}; font-size:${a.fontSize}px; font-weight:${a.bold ? "600" : "400"}; fill:${a.color};`), m.setAttribute("dominant-baseline", "ideographic");
				let h = !0;
				for (let e of r.lines) {
					let t = f("tspan");
					h || t.setAttribute("dy", String(a.lineHeight)), t.setAttribute("x", String(d)), t.textContent = e, m.appendChild(t), h = !1;
				}
				c.appendChild(m), t += r.itemW + (n < e.items.length - 1 ? a.itemGapX : 0);
			}
			S += e.height + a.itemGapY;
		}
		b.length && (S -= a.itemGapY);
		let C = S + a.paddingY;
		return s.setAttribute("x", "0"), s.setAttribute("y", "0"), s.setAttribute("width", String(a.maxWidth)), s.setAttribute("height", String(C)), l.remove(), C + Math.round(a.fontSize * .4);
	}
	function I(e) {
		let t = e.cloneNode(!0);
		return t.querySelectorAll("script").forEach((e) => e.remove()), k(e, t), j(t), t;
	}
	function L(e) {
		let t = Array.from(e.childNodes).filter((e) => e.nodeType === 1 && e.nodeName.toLowerCase() === "defs"), n = Array.from(e.childNodes).filter((e) => e.nodeType !== 1 || e.nodeName.toLowerCase() !== "defs"), r = document.createElementNS(o, "g");
		r.setAttribute("id", "__vdu_export_content");
		for (let e of n) r.appendChild(e);
		for (let n of t) e.appendChild(n);
		return e.appendChild(r), r;
	}
	function R(e, t) {
		t <= 0 || e.setAttribute("transform", `translate(0, ${t})`);
	}
	function z(e, t) {
		if (t <= 0) return;
		let n = A(e), r = e.getAttribute("height");
		if (n) e.setAttribute("viewBox", `${n.minX} ${n.minY} ${n.width} ${n.height + t}`), r && e.setAttribute("height", String(Number(r) + t));
		else if (r) e.setAttribute("height", String(Number(r) + t));
		else {
			let n = e.getBBox();
			e.setAttribute("viewBox", `0 0 ${Math.max(1, n.width)} ${Math.max(1, n.height + t)}`);
		}
	}
	function B(e) {
		let t = A(e);
		if (t) return t.width;
		let n = Number(e.getAttribute("width"));
		return !Number.isNaN(n) && n > 0 ? n : e.getBBox().width || 0;
	}
	function V(e) {
		let t = A(e);
		if (t) return t.height;
		let n = Number(e.getAttribute("height"));
		return !Number.isNaN(n) && n > 0 ? n : e.getBBox().height || 0;
	}
	async function H(e) {
		let t = M(e);
		d();
		try {
			if (document.fonts?.ready) try {
				await Promise.race([document.fonts.ready, new Promise((e) => setTimeout(e, 4e3))]);
			} catch {}
			let t = L(e), n = A(e), r = n ? n.minX : 0, i = n ? n.minY : 0, a = Math.max(1, B(e)), o = Math.max(1, V(e)), s = p(u(e).getPropertyValue("font-family") || ""), c = (b ? null : P(e, a, r, i, s)) ?? 0, d = m && m.value && m.value.position ? String(m.value.position).toLowerCase() : "top";
			m && m.value && m.value.show && d === "top" && (c += F(e, a, r, i + c, s)), R(t, c);
			let f = 0;
			m && m.value && m.value.show && d === "bottom" && (f += F(e, a, r, i + c + o, s)), z(e, c + f), e.style.backgroundColor = v.value ?? "#FFF", await g(e, { mode: "raster" }), h(e, l.value.text ? -6 : -48);
		} finally {
			N(t), d();
		}
	}
	async function U() {
		let e = I(await T());
		await H(e);
		let t = `<?xml version="1.0" standalone="no"?>\n${new XMLSerializer().serializeToString(e)}`, n = new Blob([t], { type: "image/svg+xml;charset=utf-8" });
		return {
			blob: n,
			url: URL.createObjectURL(n),
			text: t,
			dataUrl: `data:image/svg+xml;charset=utf-8,${encodeURIComponent(t)}`
		};
	}
	async function W(e) {
		let { url: t } = await U(), n = e || l.value?.text || "chart", r = document.createElement("a");
		r.href = t, r.download = n.endsWith(".svg") ? n : `${n}.svg`, document.body.appendChild(r), r.click(), document.body.removeChild(r), URL.revokeObjectURL(t);
	}
	return {
		ready: S,
		getSvg: U,
		exportSvg: W
	};
}
function h(e, t = -50) {
	if (!t) return;
	let n = e.getAttribute("viewBox"), r = (t) => {
		if (!t) return;
		let n = e.getAttribute("height");
		if (!n) return;
		let r = /^(\d+(\.\d+)?)(px)?$/.exec(n.trim());
		if (!r) return;
		let i = parseFloat(r[1]) || 0, a = Math.max(1, i + t);
		e.setAttribute("height", String(a));
	};
	if (n) {
		let i = n.trim().split(/\s+|,/).map(Number);
		if (i.length !== 4 || i.some((e) => !Number.isFinite(e))) return;
		let [a, o, s, c] = i, l = o + t, u = -t, d = Math.max(1, c + u);
		e.setAttribute("viewBox", `${a} ${l} ${s} ${d}`), r(u);
		return;
	}
	let i = Number(e.getAttribute("width")), a = Number(e.getAttribute("height"));
	if (!(i > 0 && a > 0)) {
		let t = e.getBBox();
		i = Math.max(1, t.width || 1), a = Math.max(1, t.height || 1);
	}
	let o = t, s = -t, c = Math.max(1, a + s);
	e.setAttribute("viewBox", `0 ${o} ${i} ${c}`), (function() {
		let t = e.getAttribute("height");
		if (!t) return;
		let n = /^(\d+(\.\d+)?)(px)?$/.exec(t.trim());
		if (!n) return;
		let r = parseFloat(n[1]) || 0, i = Math.max(1, r + s);
		e.setAttribute("height", String(i));
	})();
}
async function g(e, { mode: t = "raster" } = {}) {
	let n = Array.from(e.querySelectorAll("foreignObject"));
	if (!n.length) return {
		converted: 0,
		rasterized: 0,
		skipped: 0,
		errors: 0
	};
	let r = 0, i = 0, a = 0, c = 0, l = (e) => [
		"div",
		"p",
		"span",
		"strong",
		"em",
		"b",
		"i",
		"br"
	].includes(e), d = (e) => {
		try {
			let t = e.firstElementChild;
			if (!t) return !1;
			let n = [t];
			for (; n.length;) {
				let e = n.pop();
				if (e.nodeType === 3) continue;
				if (e.nodeType !== 1) return !1;
				let t = (e.tagName || "").toLowerCase();
				if (!l(t) || e.querySelector?.("img,svg,canvas,video,foreignObject")) return !1;
				for (let t of Array.from(e.childNodes)) n.push(t);
			}
			return !0;
		} catch {
			return !1;
		}
	};
	function f(e) {
		try {
			let t = e.firstElementChild;
			if (!t) return null;
			let n = (e, t = 0) => Number.isFinite(+e) ? +e : t, r = n(e.getAttribute("x"), 0), i = n(e.getAttribute("y"), 0), a = n(e.getAttribute("width"), NaN);
			if (!Number.isFinite(a) || a <= 0) {
				let t = e.getBBox?.();
				a = Math.max(1, n(t?.width, 0));
			}
			let s = (e, t) => {
				if (!e) return Math.round(t * 1.2);
				let n = e.trim().toLowerCase();
				if (n === "normal") return Math.round(t * 1.2);
				if (n.endsWith("px")) return parseFloat(n) || Math.round(t * 1.2);
				if (n.endsWith("%")) return parseFloat(n) / 100 * t || Math.round(t * 1.2);
				if (n.endsWith("em")) return (parseFloat(n) || 1) * t;
				if (n.endsWith("rem")) {
					let e = parseFloat(getComputedStyle(document.documentElement).fontSize) || t;
					return (parseFloat(n) || 1) * e;
				}
				let r = Number(n);
				return Number.isFinite(r) ? r * t : Math.round(t * 1.2);
			}, c = (e) => {
				let t = (e || "").split(/\s+/).map((e) => {
					let t = (e || "").trim().toLowerCase();
					if (t.endsWith("px")) return parseFloat(t) || 0;
					if (t.endsWith("rem")) {
						let e = parseFloat(getComputedStyle(document.documentElement).fontSize) || 16;
						return (parseFloat(t) || 0) * e;
					}
					if (t.endsWith("em")) {
						let e = parseFloat(getComputedStyle(document.body).fontSize) || 16;
						return (parseFloat(t) || 0) * e;
					}
					let n = Number(t);
					return Number.isFinite(n) ? n : 0;
				}).filter(Number.isFinite);
				return t.length ? t.length === 1 ? [
					t[0],
					t[0],
					t[0],
					t[0]
				] : t.length === 2 ? [
					t[0],
					t[1],
					t[0],
					t[1]
				] : t.length === 3 ? [
					t[0],
					t[1],
					t[2],
					t[1]
				] : [
					t[0],
					t[1],
					t[2],
					t[3]
				] : [
					0,
					0,
					0,
					0
				];
			}, l = (e, t) => {
				let n = u(e), r = n.getPropertyValue("font-family") || t?.ff || "system-ui";
				r = p(r);
				let i = parseFloat(n.getPropertyValue("font-size"));
				Number.isFinite(i) || (i = t?.fs || 14);
				let a = n.getPropertyValue("font-weight") || t?.fw || "400", o = n.getPropertyValue("font-style") || t?.fsty || "normal", l = n.getPropertyValue("font-stretch") || t?.fstc || "normal", d = n.getPropertyValue("letter-spacing") || t?.lsp || "normal", f = n.getPropertyValue("word-spacing") || t?.wsp || "0px", m = n.getPropertyValue("font-kerning") || t?.kern || "auto", h = n.getPropertyValue("font-variant-ligatures") || t?.liga || "normal", g = n.getPropertyValue("text-transform") || t?.ttfm || "none", _ = n.getPropertyValue("color") || t?.fill || "#000", v = n.getPropertyValue("line-height"), y = !v || v.trim().toLowerCase() === "normal" ? t?.lh ?? Math.round(i * 1.2) : s(v, i), b = (n.getPropertyValue("text-align") || t?.ta || "start").trim().toLowerCase(), x = parseFloat(n.getPropertyValue("padding-top")) || 0, S = parseFloat(n.getPropertyValue("padding-right")) || 0, C = parseFloat(n.getPropertyValue("padding-bottom")) || 0, w = parseFloat(n.getPropertyValue("padding-left")) || 0, T = n.getPropertyValue("padding");
				if (T && x === 0 && S === 0 && C === 0 && w === 0) {
					let [e, t, n, r] = c(T);
					x = e, S = t, C = n, w = r;
				}
				let E = parseFloat(n.getPropertyValue("border-left-width")) || 0, D = parseFloat(n.getPropertyValue("border-right-width")) || 0, O = n.getPropertyValue("box-sizing") || "content-box";
				return {
					ff: r,
					fs: i,
					fw: a,
					fsty: o,
					fstc: l,
					lsp: d,
					wsp: f,
					kern: m,
					liga: h,
					ttfm: g,
					fill: _,
					lh: y,
					ta: b,
					padL: w,
					padR: S,
					padT: x,
					padB: C,
					bL: E,
					bR: D,
					box: O
				};
			}, d = (e) => /^(div|p|section|article|ul|ol|li|h[1-6])$/.test(e), f = { __para__: !0 }, m = { __br__: !0 }, h = [], g = l(t), _ = (e, t, n) => {
				if (e.nodeType === 3) {
					let r = (e.nodeValue || "").replace(/\s+/g, " ").trim();
					r && h.push({
						text: r,
						style: t,
						path: n.slice()
					});
					return;
				}
				if (e.nodeType !== 1) return;
				let r = e.tagName.toLowerCase();
				if (r === "br") {
					h.push({
						...m,
						path: n.slice()
					});
					return;
				}
				let i = l(e, t);
				n.push(e);
				for (let t of Array.from(e.childNodes)) _(t, i, n);
				n.pop(), d(r) && h.push({
					...f,
					path: n.slice()
				});
			};
			for (_(t, g, [t]); h.length && (h.at(-1).__para__ || h.at(-1).__br__);) h.pop();
			if (!h.length) return null;
			let v = h.filter((e) => !e.__para__ && !e.__br__), y = (() => {
				if (!v.length) return [t];
				let e = v.map((e) => e.path), n = Math.min(...e.map((e) => e.length)), r = 0;
				for (; r < n;) {
					let t = e[0][r];
					if (e.every((e) => e[r] === t)) r += 1;
					else break;
				}
				return e[0].slice(0, Math.max(1, r));
			})(), b = l(y[y.length - 1] || t), x = 0, S = 0, C = 0;
			for (let e of y) {
				let t = l(e);
				x += t.padL, S += t.padR, C += t.padT;
			}
			let w = [Math.max(1, a - x - S)];
			for (let e of y) try {
				let t = u(e);
				if (e.clientWidth && e.clientWidth > 0) {
					let n = parseFloat(t.paddingLeft) || 0, r = parseFloat(t.paddingRight) || 0, i = Math.max(1, e.clientWidth - n - r);
					w.push(i);
				}
				let n = t.width, r = parseFloat(n);
				if (Number.isFinite(r) && r > 0) {
					let e = t.boxSizing || t.getPropertyValue("box-sizing") || "content-box", n = parseFloat(t.paddingLeft) || 0, i = parseFloat(t.paddingRight) || 0, a = parseFloat(t.borderLeftWidth) || 0, o = parseFloat(t.borderRightWidth) || 0, s = e === "border-box" ? Math.max(1, r - n - i - a - o) : Math.max(1, r);
					w.push(s);
				}
			} catch {}
			let T = Math.floor(Math.min(...w.filter(Number.isFinite))) - 1;
			T > 0 || (T = 1);
			let E = "start", D = r + x;
			b.ta === "center" ? (E = "middle", D = r + x + T / 2) : (b.ta === "right" || b.ta === "end") && (E = "end", D = r + (a - S));
			let O = e.ownerSVGElement || e.closest("svg"), k = document.createElementNS(o, "text");
			k.setAttribute("x", String(D)), k.setAttribute("y", String(i + C)), k.setAttribute("text-anchor", E), k.setAttribute("dominant-baseline", "text-before-edge"), k.setAttribute("xml:space", "preserve");
			let A = document.createElementNS(o, "text");
			A.setAttribute("x", "0"), A.setAttribute("y", "0"), A.setAttribute("opacity", "0"), (O || e).appendChild(A);
			let j = (e) => `font-family:${e.ff}; font-size:${e.fs}px; font-weight:${e.fw}; font-style:${e.fsty}; font-stretch:${e.fstc}; letter-spacing:${e.lsp}; word-spacing:${e.wsp}; font-kerning:${e.kern}; font-variant-ligatures:${e.liga}; text-transform:${e.ttfm};`, M = (e, t) => (A.textContent = e || "", A.setAttribute("style", j(t)), A.getComputedTextLength()), N = (e) => M(" ", e), P = [], F = [], I = 0, L = () => {
				F.length && P.push({ segs: F.slice() }), F = [], I = 0;
			}, R = (e, t, n) => {
				let r = M(e, t);
				if (n) {
					let e = N(t);
					r += e, F.push({
						text: " ",
						...t,
						w: e,
						isSpace: !0
					});
				}
				F.push({
					text: e,
					...t,
					w: r
				}), I += r;
			}, z = (e, t, n) => {
				let r = "";
				for (let i of e) {
					let e = r + i;
					if (M(e, t) <= n) r = e;
					else break;
				}
				return r || e[0] || "";
			};
			for (let e of h) {
				if (e.__para__ || e.__br__) {
					L();
					continue;
				}
				let t = e.style, n = e.text.split(/(\s+)/).filter((e) => e.length > 0), r = (e, n) => {
					if (!e) return;
					let r = Math.max(1, T), i = M(e, t) + (n ? N(t) : 0);
					if (F.length === 0) if (i <= r) R(e, t, !1);
					else {
						let n = e;
						for (; n;) {
							let e = z(n, t, r);
							R(e, t, !1), n = n.slice(e.length), n && L();
						}
					}
					else if (I + i <= r) R(e, t, n);
					else if (L(), M(e, t) <= r) R(e, t, !1);
					else {
						let n = e;
						for (; n;) {
							let e = z(n, t, r);
							R(e, t, !1), n = n.slice(e.length), n && L();
						}
					}
				}, i = !1;
				for (let e of n) {
					if (/^\s+$/.test(e)) {
						i = !0;
						continue;
					}
					r(e, i), i = !1;
				}
			}
			L();
			for (let e = 0; e < P.length; e += 1) {
				let t = P[e].segs, n = Math.max(...t.map((e) => e.fs || b.fs), b.fs), r = !0;
				for (let i of t) {
					let t = document.createElementNS(o, "tspan");
					r &&= (t.setAttribute("x", String(D)), e > 0 && t.setAttribute("dy", String(n)), !1), t.setAttribute("style", `${j(i)} fill:${i.fill};`), t.textContent = i.text, k.appendChild(t);
				}
				if (!t.length && e > 0) {
					let e = document.createElementNS(o, "tspan");
					e.setAttribute("x", String(D)), e.setAttribute("dy", String(n)), e.textContent = "", k.appendChild(e);
				}
			}
			try {
				A.remove();
			} catch {}
			return k;
		} catch {
			return null;
		}
	}
	let m = (e) => {
		try {
			let t = document.createElement("div");
			t.style.position = "absolute", t.style.left = "-99999px", t.style.top = "-99999px", t.style.visibility = "hidden", t.style.pointerEvents = "none", t.style.width = "auto", t.style.height = "auto";
			let n = e.cloneNode(!0);
			n.style.width = "auto", n.style.height = "auto", n.style.display = "inline-block", n.style.maxWidth = "none", n.style.maxHeight = "none", n.style.boxSizing = "content-box", document.body.appendChild(t), t.appendChild(n);
			let r = n.getBoundingClientRect(), i = Math.ceil(r.width || n.scrollWidth || 0), a = Math.ceil(r.height || n.scrollHeight || 0);
			return t.remove(), {
				w: Math.max(1, i),
				h: Math.max(1, a)
			};
		} catch {
			return {
				w: 0,
				h: 0
			};
		}
	};
	for (let e of n) try {
		if (e.hasAttribute("data-no-svg-export")) {
			e.remove(), a += 1;
			continue;
		}
		if (d(e)) {
			let t = f(e);
			if (t) {
				e.parentNode.replaceChild(t, e), r += 1;
				continue;
			}
		}
		a += 1;
	} catch {
		c += 1;
	}
	if (t === "raster") {
		let t = Array.from(e.querySelectorAll("foreignObject"));
		for (let e of t) try {
			if (e.hasAttribute("data-no-svg-export")) {
				e.remove(), a += 1;
				continue;
			}
			await new Promise((e) => requestAnimationFrame(e));
			let t = e.getBBox(), n = Math.max(1, Math.ceil(t.width)), r = Math.max(1, Math.ceil(t.height)), c = e.firstElementChild;
			if (c) {
				let t = m(c), i = parseFloat(e.getAttribute("width") || "0") || 0, a = parseFloat(e.getAttribute("height") || "0") || 0;
				n = Math.max(n, i, t.w), r = Math.max(r, a, t.h);
			}
			if (!(n > 0 && r > 0)) {
				a += 1;
				continue;
			}
			let l = document.createElementNS(o, "svg");
			l.setAttribute("xmlns", o), l.setAttribute("xmlns:xlink", s), l.setAttribute("width", String(n)), l.setAttribute("height", String(r)), l.setAttribute("viewBox", `0 0 ${n} ${r}`);
			let u = e.cloneNode(!0);
			u.setAttribute("x", "0"), u.setAttribute("y", "0"), u.setAttribute("width", String(n)), u.setAttribute("height", String(r)), l.appendChild(u);
			let d = new XMLSerializer().serializeToString(l), f = "data:image/svg+xml;charset=utf-8," + encodeURIComponent(d), p = document.createElement("canvas");
			p.width = Math.max(1, Math.floor(n * 2)), p.height = Math.max(1, Math.floor(r * 2));
			let h = p.getContext("2d"), g = new Image();
			g.decoding = "sync", g.crossOrigin = "anonymous", await new Promise((e, t) => {
				g.onload = e, g.onerror = t, g.src = f;
			}), h.drawImage(g, 0, 0, p.width, p.height);
			let _ = p.toDataURL("image/png"), v = document.createElementNS(o, "image");
			v.setAttributeNS(s, "href", _), v.setAttribute("href", _), v.setAttribute("x", e.getAttribute("x") || String(t.x)), v.setAttribute("y", e.getAttribute("y") || String(t.y)), v.setAttribute("width", String(n)), v.setAttribute("height", String(r)), e.parentNode.replaceChild(v, e), i += 1;
		} catch {
			c += 1;
		}
	}
	return {
		converted: r,
		rasterized: i,
		skipped: a,
		errors: c
	};
}
//#endregion
//#region src/useChartExport.js
function _({ svg: e, title: n, legend: i, legendItems: a, backgroundColor: o, getSvgCallback: s, generateImage: c, titleEmbedded: l = !1, stretchTitle: u = !1 }) {
	let d = r(!1), f = r(!1), { exportSvg: p, getSvg: h } = m({
		svg: e,
		title: n,
		legend: i,
		legendItems: a,
		backgroundColor: o,
		titleEmbedded: l,
		stretchTitle: u
	});
	async function g({ isCb: e }) {
		f.value = !0, await t();
		try {
			if (e) {
				let { blob: e, url: t, text: n, dataUrl: r } = await h();
				await Promise.resolve(s()({
					blob: e,
					url: t,
					text: n,
					dataUrl: r
				}));
			} else await Promise.resolve(p());
		} finally {
			f.value = !1;
		}
	}
	function _(e) {
		if (e?.stage === "start") {
			d.value = !0;
			return;
		}
		if (e?.stage === "end") {
			d.value = !1;
			return;
		}
		c();
	}
	return {
		isCallbackImaging: d,
		isCallbackSvg: f,
		generateSvg: g,
		onGenerateImage: _
	};
}
//#endregion
export { _ as t };
