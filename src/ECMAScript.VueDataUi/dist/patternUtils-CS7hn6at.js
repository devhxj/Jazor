//#region src/patternUtils.js
function e(e) {
	let t = String(e ?? ""), n = 2166136261;
	for (let e = 0; e < t.length; e += 1) n ^= t.charCodeAt(e), n = Math.imul(n, 16777619);
	return n >>> 0;
}
function t(e) {
	let t = (Number.isFinite(e) ? e : 0) >>> 0;
	return function() {
		t += 1831565813;
		let e = t;
		return e = Math.imul(e ^ e >>> 15, e | 1), e ^= e + Math.imul(e ^ e >>> 7, e | 61), ((e ^ e >>> 14) >>> 0) / 4294967296;
	};
}
function n(e, t, n) {
	if (!Array.isArray(e) || e.length === 0) return console.error("VueUiPatternSeed - pickValue requires a non-empty array"), n;
	let r = typeof t == "function" ? t() : 0, i = e[Math.floor(r * e.length)];
	return i === void 0 ? (console.error("VueUiPatternSeed - pickValue selected an invalid index"), n ?? e[0]) : i;
}
function r(e) {
	return String(e ?? "").replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}
function i(e, t, n, i, a, o, s) {
	return `<line x1="${e}" y1="${t}" x2="${n}" y2="${i}" stroke="${r(a)}" stroke-width="${o}" opacity="${s}" shape-rendering="crispEdges" stroke-linecap="round" stroke-linejoin="round" />`;
}
function a(e, t, n, i, a) {
	return `<circle cx="${e}" cy="${t}" r="${n}" fill="${r(i)}" opacity="${a}" />`;
}
function o(e, t, n, i, a) {
	return `<path d="${e}" fill="${r(t)}" stroke="${r(n)}" stroke-width="${i}" opacity="${a}" stroke-linecap="round" stroke-linejoin="round" />`;
}
function s(e, t, n, i, a, o, s = 0, c = e + n / 2, l = t + i / 2) {
	return `<rect x="${e}" y="${t}" width="${n}" height="${i}" fill="${r(a)}" opacity="${o}"${s ? ` transform="rotate(${s} ${c} ${l})"` : ""} />`;
}
function c(e, t, n, r, i) {
	let a = n / 2;
	return o([
		`M ${e} ${t - a}`,
		`L ${e + a} ${t}`,
		`L ${e} ${t + a}`,
		`L ${e - a} ${t}`,
		"Z"
	].join(" "), r, "none", 0, i);
}
function l(e, t) {
	return Number.isFinite(e) && e > 0 ? e : t;
}
function u(e, t) {
	let n = l(e, 8), r = l(t, 20);
	if (n > r) {
		let e = n;
		n = r, r = e;
	}
	return {
		minimumSize: n,
		maximumSize: r
	};
}
function d(e, t, r, i) {
	let o = n([
		e * .08,
		e * .1,
		e * .12
	], i, e * .1);
	switch (n([
		"grid",
		"offsetGrid",
		"corners",
		"centered"
	], i, "grid")) {
		case "offsetGrid": return [
			a(e * .25, e * .25, o, t, r),
			a(e * .75, e * .25, o, t, r),
			a(e * .5, e * .5, o, t, r),
			a(e * .25, e * .75, o, t, r),
			a(e * .75, e * .75, o, t, r)
		].join("");
		case "corners": return [
			a(e * .2, e * .2, o, t, r),
			a(e * .8, e * .2, o, t, r),
			a(e * .2, e * .8, o, t, r),
			a(e * .8, e * .8, o, t, r)
		].join("");
		case "centered": return [
			a(e * .5, e * .2, o, t, r),
			a(e * .2, e * .5, o, t, r),
			a(e * .5, e * .5, o, t, r),
			a(e * .8, e * .5, o, t, r),
			a(e * .5, e * .8, o, t, r)
		].join("");
		default: return [
			a(e * .25, e * .25, o, t, r),
			a(e * .75, e * .25, o, t, r),
			a(e * .25, e * .75, o, t, r),
			a(e * .75, e * .75, o, t, r)
		].join("");
	}
}
function f(e, t, r, a, o) {
	let s = n([
		"diagonal",
		"vertical",
		"horizontal",
		"crosshatch",
		"grid"
	], o, "diagonal"), c = n([
		e * .28,
		e * .33,
		e * .4,
		e * .5
	], o, e * .33);
	switch (s) {
		case "vertical": return [
			0,
			c,
			c * 2
		].map((n) => i(n, 0, n, e, t, r, a)).join("");
		case "horizontal": return [
			0,
			c,
			c * 2
		].map((n) => i(0, n, e, n, t, r, a)).join("");
		case "crosshatch": return [
			i(-e, e, e, -e, t, r, a),
			i(0, e, e, 0, t, r, a),
			i(0, 0, e, e, t, r, a * .8),
			i(e, 0, 0, e, t, r * .8, a * .8)
		].join("");
		case "grid": return [i(e * .5, 0, e * .5, e, t, r, a), i(0, e * .5, e, e * .5, t, r, a)].join("");
		default: return [
			i(-e, e, e, -e, t, r, a),
			i(0, e, e, 0, t, r, a),
			i(0, e * 2, e * 2, 0, t, r, a)
		].join("");
	}
}
function p(e, t, r, i) {
	let a = n([
		"diamonds",
		"squares",
		"mixed"
	], i, "diamonds"), o = n([
		e * .16,
		e * .2,
		e * .24
	], i, e * .2);
	switch (a) {
		case "squares": return [
			s(e * .2, e * .2, o, o, t, r),
			s(e * .65, e * .2, o, o, t, r),
			s(e * .2, e * .65, o, o, t, r),
			s(e * .65, e * .65, o, o, t, r)
		].join("");
		case "mixed": return [
			c(e * .3, e * .3, o, t, r),
			s(e * .6, e * .2, o, o, t, r),
			c(e * .7, e * .7, o, t, r)
		].join("");
		default: return [
			c(e * .25, e * .25, o, t, r),
			c(e * .75, e * .25, o, t, r),
			c(e * .25, e * .75, o, t, r),
			c(e * .75, e * .75, o, t, r)
		].join("");
	}
}
function m(e, t, r) {
	if (!n([
		!0,
		!1,
		!1
	], r, !1)) return "";
	let i = n([
		"singleDot",
		"cornerDiamond",
		"softBand"
	], r, "singleDot"), o = n([
		.12,
		.16,
		.2
	], r, .16);
	switch (i) {
		case "cornerDiamond": return c(e * .5, e * .5, e * .3, t, o);
		case "softBand": return s(0, e * .4, e, e * .2, t, o);
		default: return a(e * .5, e * .5, e * .12, t, o);
	}
}
function h(e, t, r) {
	let i = n([
		1,
		1.25,
		1.5,
		1.75
	], r, 1.25), a = n([
		.4,
		.5,
		.6,
		.7
	], r, .5), o = n([
		"lines",
		"dots",
		"shapes"
	], r, "lines"), s = "";
	s = o === "lines" ? f(e, t, i, a, r) : o === "dots" ? d(e, t, a, r) : p(e, t, a, r);
	let c = m(e, t, r);
	return `${s}${c}`;
}
function g(i, a = {}) {
	try {
		let o = a?.disambiguator ?? "", s = `${String(i ?? "")}::${String(o)}`, c = a?.foregroundColor ?? "#111111", l = a?.backgroundColor ?? "transparent", { minimumSize: d, maximumSize: f } = u(a?.minimumSize, a?.maximumSize), p = t(e(s)), m = [];
		for (let e = d; e <= f; e += 2) m.push(e);
		let g = n(m.length > 0 ? m : [8], p, 8), _ = n([
			0,
			0,
			0,
			15,
			30,
			45,
			60,
			90
		], p, 0), v = [];
		if (l !== "transparent") {
			let e = r(l);
			v.push(`<rect x="0" y="0" width="${g}" height="${g}" fill="${e}" />`);
		}
		return v.push(h(g, c, p)), {
			width: g,
			height: g,
			rotation: _,
			patternType: "prettyUniquePattern",
			contentMarkup: v.join("")
		};
	} catch (e) {
		return console.error("VueUiPatternSeed - Failed to create seeded SVG pattern", e), {
			width: 8,
			height: 8,
			rotation: 0,
			patternType: "prettyUniquePattern",
			contentMarkup: ""
		};
	}
}
function _({ id: e, seed: t, foregroundColor: n, backgroundColor: i, maxSize: a, minSize: o, disambiguator: s }) {
	try {
		let c = g(t, {
			foregroundColor: n ?? "#1A1A1A",
			backgroundColor: i ?? "transparent",
			minimumSize: o,
			maximumSize: a,
			disambiguator: s
		});
		return `<defs><pattern id="${r(e)}" patternUnits="userSpaceOnUse" width="${c.width}" height="${c.height}" patternTransform="rotate(${c.rotation})">${c.contentMarkup}</pattern></defs>`;
	} catch (t) {
		return console.error("VueUiPatternSeed - Failed to create chart pattern slot markup", t), `<defs><pattern id="${r(e)}" patternUnits="userSpaceOnUse" width="8" height="8" patternTransform="rotate(0)"></pattern></defs>`;
	}
}
//#endregion
export { g as n, _ as t };
