import { t as e } from "./lib-Bttd6u5E.js";
//#region src/dom-to-png.js
async function t(e) {
	let t = e.querySelectorAll("img"), n = Array.from(t).map((e) => new Promise((t) => {
		if (!e.src || e.src.startsWith("data:")) return t();
		if (e.complete && e.naturalWidth !== 0) {
			try {
				let t = document.createElement("canvas");
				t.width = e.naturalWidth, t.height = e.naturalHeight, t.getContext("2d").drawImage(e, 0, 0), e.src = t.toDataURL();
			} catch {}
			return t();
		}
		let n = new window.Image();
		n.crossOrigin = "anonymous", n.onload = function() {
			try {
				let t = document.createElement("canvas");
				t.width = n.naturalWidth, t.height = n.naturalHeight, t.getContext("2d").drawImage(n, 0, 0), e.src = t.toDataURL();
			} catch {}
			t();
		}, n.onerror = function() {
			t();
		}, n.src = e.src;
	}));
	await Promise.all(n);
}
function n(e) {
	e.querySelectorAll("[data-dom-to-png-ignore]").forEach((e) => {
		e.remove();
	});
}
function r(e, t, n) {
	let i = window.getComputedStyle(t), a = t.getBoundingClientRect(), o = i.display.includes("flex"), s = {}, c = e.getAttribute("style");
	typeof c != "string" && (c = ""), c.split(";").forEach((e) => {
		if (typeof e == "string" && e.trim()) {
			let [t, n] = e.split(":");
			t && n !== void 0 && (s[t.trim()] = n.trim());
		}
	});
	for (let e = 0; e < i.length; e += 1) {
		let t = i[e];
		s[t] = i.getPropertyValue(t);
	}
	s.color = i.color, s["background-color"] = i.backgroundColor;
	let l = i.fontFamily || n || "";
	(!l || l.trim() === "" || l === "inherit" || l === "initial" || l.toLowerCase().startsWith("system-ui") || l.toLowerCase() === "sans-serif" || l.toLowerCase() === "serif" || l.toLowerCase() === "monospace") && (l = "Helvetica, Arial, sans-serif"), s["font-family"] = l, s["font-size"] = i.fontSize, s["font-weight"] = i.fontWeight, (o || i.display.includes("grid") || [
		"inline-block",
		"absolute",
		"fixed"
	].includes(i.position)) && (a.width > 0 && (s.width = a.width + "px"), a.height > 0 && (s.height = a.height + "px")), o && i.flexWrap === "nowrap" && (s["white-space"] = "nowrap"), [
		"box-sizing",
		"padding",
		"margin",
		"border"
	].forEach((e) => {
		s[e] = i.getPropertyValue(e);
	}), s.overflow = "visible", s["overflow-x"] = "visible", s["overflow-y"] = "visible";
	let u = "";
	for (let e in s) u += `${e}:${s[e]};`;
	e.setAttribute("style", u);
	let d = e.children || [], f = t.children || [];
	for (let e = 0; e < d.length; e++) d[e].nodeType === 1 && f[e] && r(d[e], f[e], l);
}
function i(e, t) {
	e.querySelectorAll("text").forEach((e) => {
		e.setAttribute("font-family", t), e.style.fontFamily = t;
	});
}
function a(e) {
	let t = new TextEncoder().encode(e), n = "";
	return t.forEach((e) => n += String.fromCharCode(e)), btoa(n);
}
function o(e, t, n) {
	let r = new XMLSerializer().serializeToString(e);
	r.match(/^<svg[^>]+xmlns="http\:\/\/www\.w3\.org\/2000\/svg"/) || (r = r.replace(/^<svg/, "<svg xmlns=\"http://www.w3.org/2000/svg\""));
	let i = window.btoa(unescape(encodeURIComponent(r))), a = new window.Image();
	return a.src = `data:image/svg+xml;base64,${i}`, new Promise((e, r) => {
		a.onload = function() {
			let r = document.createElement("canvas");
			r.width = t, r.height = n, r.getContext("2d").drawImage(a, 0, 0, t, n), e(r.toDataURL("image/png", 1));
		}, a.onerror = (e) => r(e);
	});
}
function s(e) {
	if (e.nodeType !== 1) return;
	let t = window.getComputedStyle(e), n = "";
	for (let e = 0; e < t.length; e++) {
		let r = t[e];
		n += `${r}:${t.getPropertyValue(r)};`;
	}
	e.setAttribute("style", n);
	for (let t = 0; t < e.children.length; t++) s(e.children[t]);
}
function c() {
	let e = [];
	for (let t of document.styleSheets) try {
		let n = t.cssRules;
		if (!n) continue;
		for (let t of n) (typeof CSSFontFaceRule < "u" && t instanceof CSSFontFaceRule || t.cssText.trim().startsWith("@font-face")) && e.push(t.cssText);
	} catch {
		continue;
	}
	return e;
}
function l(e) {
	let t = c();
	if (!t.length) return;
	let n = document.createElement("style");
	n.setAttribute("type", "text/css"), n.textContent = t.join("\n");
	let r = e.querySelector("defs") || document.createElementNS("http://www.w3.org/2000/svg", "defs");
	r.appendChild(n), e.querySelector("defs") || e.insertBefore(r, e.firstChild);
}
function u(e, t) {
	let n = e.querySelectorAll("foreignObject"), r = t.querySelectorAll("foreignObject");
	n.forEach((e, t) => {
		let n = r[t];
		if (!n) return;
		function i(e, t) {
			if (!e || !t) return;
			if (e.nodeType === 1 && t.nodeType === 1) {
				let n = window.getComputedStyle(t), r = "";
				for (let e = 0; e < n.length; e++) {
					let t = n[e];
					r += `${t}:${n.getPropertyValue(t)};`;
				}
				e.setAttribute("style", r);
			}
			let n = e.children || [], r = t.children || [];
			for (let e = 0; e < n.length; e++) i(n[e], r[e]);
		}
		i(e, n);
	});
}
function d(e) {
	e.querySelectorAll("foreignObject").forEach((e) => {
		let t = "", n = Array.from(e.querySelectorAll("*"));
		if (n.length !== 0) {
			for (let e of document.styleSheets) {
				let r;
				try {
					r = e.cssRules;
				} catch {
					continue;
				}
				if (r) {
					for (let e of r) if (!(typeof CSSStyleRule < "u" && !(e instanceof CSSStyleRule))) try {
						n.some((t) => t.matches(e.selectorText)) && (t += e.cssText + "\n");
					} catch {
						continue;
					}
				}
			}
			if (t) {
				let n = document.createElement("style");
				n.textContent = t, e.insertBefore(n, e.firstChild);
			}
		}
	});
}
function f(e) {
	e.querySelectorAll("foreignObject").forEach((e) => {
		let t = e.firstElementChild;
		t && t.tagName.toLowerCase() !== "svg" && t.setAttribute("xmlns", "http://www.w3.org/1999/xhtml");
	});
}
function p(e, t) {
	let n = e.querySelectorAll("foreignObject"), r = t.querySelectorAll("foreignObject");
	n.forEach((e, t) => {
		let n = r[t];
		if (!e || !n) return;
		let i = e.firstElementChild, a = n.firstElementChild;
		i && a && m(i, a);
	});
}
function m(e, t) {
	if (e.nodeType !== 1 || !t) return;
	r(e, t);
	let n = e.children || [], i = t.children || [];
	for (let e = 0; e < n.length; e++) m(n[e], i[e]);
}
function h(e, t) {
	let n = Array.from(t.querySelectorAll("img"));
	e.querySelectorAll("img").forEach((e) => {
		let t = e.getAttribute("src"), r = n.find((e) => e.getAttribute("src") === t);
		if (r) {
			let t = window.getComputedStyle(r), n = "";
			for (let e = 0; e < t.length; e++) {
				let r = t[e];
				n += `${r}:${t.getPropertyValue(r)};`;
			}
			e.setAttribute("style", n);
		}
	});
}
async function g({ container: c, scale: m = 2, base64: g = !1 }) {
	if (!c) throw Error("No container provided");
	await document.fonts.ready;
	let _ = window.getComputedStyle(c).fontFamily || "Helvetica";
	_.toLowerCase().includes("helvetica") || (_ += ", Helvetica");
	let v = c.cloneNode(!0), y = v.querySelectorAll("canvas"), b = c.querySelectorAll("canvas");
	for (let e = 0; e < b.length; e += 1) {
		let t = b[e], n = y[e];
		if (t && n) {
			let e = document.createElement("img");
			e.src = t.toDataURL("image/png"), e.width = t.width, e.height = t.height, e.style.width = t.style.width || t.width + "px", e.style.height = t.style.height || t.height + "px", n.replaceWith(e);
		}
	}
	let x = c.querySelector("svg[aria-label]"), S = v.querySelector("svg[aria-label]");
	if (x && S) {
		f(S), d(S), u(S, x), p(S, x), s(S), i(S, _), h(S, x), await t(S);
		let e = x.getBoundingClientRect(), n = e.width, r = e.height, a = await o(S, Math.round(n * m), Math.round(r * m)), c = document.createElement("img");
		c.src = a, c.width = n, c.height = r, c.style.width = n + "px", c.style.height = r + "px", S.parentNode.replaceChild(c, S);
	}
	r(v, c, _), h(v, c), n(v), await t(v);
	let { width: C, height: w } = c.getBoundingClientRect(), T = Math.round(Math.max(v.scrollWidth, C) * m), E = Math.round(Math.max(v.scrollHeight, w) * 1.01 * m), D = document.createElementNS(e, "svg");
	D.setAttribute("viewBox", `0 0 ${T} ${E}`), D.setAttribute("width", T), D.setAttribute("height", E), D.setAttribute("style", `font-family:${_};`);
	let O = document.createElementNS(e, "foreignObject");
	O.setAttribute("x", 0), O.setAttribute("y", 0), O.setAttribute("width", T), O.setAttribute("height", E), O.setAttribute("style", `font-family:${_};`), v.style.transform = `scale(${m})`, v.style.transformOrigin = "top left", v.style.width = C + "px", v.style.height = w + "px", v.style.background = window.getComputedStyle(c).backgroundColor, O.appendChild(v), l(D), D.appendChild(O);
	let k = a(new XMLSerializer().serializeToString(D)), A = new window.Image();
	A.crossOrigin = "anonymous";
	let j = `data:image/svg+xml;base64,${k}`;
	return A.src = j, g ? new Promise((e) => {
		e(j);
	}) : new Promise((e, t) => {
		A.onload = function() {
			try {
				let t = document.createElement("canvas");
				t.width = T, t.height = E, t.getContext("2d").drawImage(A, 0, 0, T, E), e(t.toDataURL("image/png", 1));
			} catch (e) {
				t("Failed to draw SVG on canvas: " + e);
			}
		}, A.onerror = function() {
			t("Failed to load SVG image for conversion");
		};
	});
}
//#endregion
export { g as t };
