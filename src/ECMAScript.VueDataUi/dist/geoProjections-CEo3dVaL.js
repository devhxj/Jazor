//#region src/geoProjections.js
function e(e, t = []) {
	return {
		type: "FeatureCollection",
		features: e.features.map((e) => {
			let n = {};
			return t.forEach((t) => {
				e.properties && e.properties.hasOwnProperty(t) && (n[t] = e.properties[t]);
			}), {
				type: "Feature",
				geometry: e.geometry,
				properties: n
			};
		})
	};
}
var t = /* @__PURE__ */ new WeakMap(), n = /* @__PURE__ */ new WeakMap();
function r(e) {
	return e * Math.PI / 180;
}
function i(e) {
	return Math.round(e * 100) / 100;
}
function a(e, t) {
	return {
		cx: e / 2,
		cy: t / 2,
		radius: Math.min(e, t) / 2 * .95
	};
}
function o(e, t, n = [0, 0]) {
	let [i, o] = n, s = r(o);
	return {
		...a(e, t),
		lon0Rad: r(i),
		sinLat0: Math.sin(s),
		cosLat0: Math.cos(s)
	};
}
function s([e, t]) {
	let n = r(t);
	return {
		lon: e,
		lat: t,
		lonRad: r(e),
		sinLat: Math.sin(n),
		cosLat: Math.cos(n)
	};
}
function c(e, t) {
	let n = e.lonRad - t.lon0Rad, r = Math.sin(n), i = Math.cos(n);
	return {
		x: e.cosLat * r,
		y: t.cosLat0 * e.sinLat - t.sinLat0 * e.cosLat * i,
		z: t.sinLat0 * e.sinLat + t.cosLat0 * e.cosLat * i
	};
}
function l(e, t = [0, 0]) {
	return c(s(e), o(1, 1, t));
}
function u(e, t, n = 10) {
	let { cx: r, cy: i, radius: o } = a(e, t), s = r - o - n, c = i - o - n, l = r + o + n, u = i + o + n;
	return {
		minX: Math.floor(s),
		minY: Math.floor(c),
		width: Math.ceil(l - s),
		height: Math.ceil(u - c)
	};
}
function d(e, t) {
	return c(e, t).z >= -1e-9;
}
function f(e, t = [0, 0]) {
	return l(e, t).z >= -1e-9;
}
function p(e, t, n = !1) {
	let r = c(e, t);
	if (r.z < -1e-9) {
		if (!n) return [NaN, NaN];
		let e = Math.hypot(r.x, r.y);
		return !Number.isFinite(e) || e <= 1e-9 ? [NaN, NaN] : [t.cx + t.radius * (r.x / e), t.cy - t.radius * (r.y / e)];
	}
	return [t.cx + t.radius * r.x, t.cy - t.radius * r.y];
}
function m(e, t, n, r = [0, 0], i = !1) {
	return p(s(e), o(t, n, r), i);
}
function h(e) {
	return ((e + 180) % 360 + 360) % 360 - 180;
}
function g(e, t, n) {
	let r = h(t.lon - e.lon);
	return s([e.lon + r * n, e.lat + (t.lat - e.lat) * n]);
}
function _(e, t, n) {
	let r = 0, i = 1, a = d(e, n);
	for (let o = 0; o < 24; o += 1) {
		let o = (r + i) / 2;
		d(g(e, t, o), n) === a ? r = o : i = o;
	}
	return g(e, t, (r + i) / 2);
}
function v(e, t) {
	return Math.abs(e[0] - t[0]) < 1e-12 && Math.abs(e[1] - t[1]) < 1e-12;
}
function y(e) {
	if (!Array.isArray(e)) return [];
	let n = t.get(e);
	if (n) return n;
	let r = [];
	for (let t of e) {
		let e = r[r.length - 1];
		(!e || !v(e, t)) && r.push(t);
	}
	return r.length > 1 && v(r[0], r[r.length - 1]) && r.pop(), t.set(e, r), r;
}
function b(e) {
	if (!Array.isArray(e)) return [];
	let t = n.get(e);
	if (t) return t;
	let r = y(e).map(s);
	return n.set(e, r), r;
}
function x(e) {
	let t = "", n = 0, r = null, a = null;
	for (let o of e) {
		if (!o || !Number.isFinite(o[0]) || !Number.isFinite(o[1])) continue;
		let e = i(o[0]), s = i(o[1]);
		r !== null && Math.abs(r - e) < 1e-8 && Math.abs(a - s) < 1e-8 || (t += n === 0 ? `M${e},${s}` : `L${e},${s}`, r = e, a = s, n += 1);
	}
	return n < 3 ? "" : `${t}Z`;
}
function S(e, t) {
	let n = b(e), r = n.length;
	if (r < 3) return "";
	let i = Array(r), a = !1, o = !1;
	for (let e = 0; e < r; e += 1) i[e] = d(n[e], t), i[e] && (a = !0);
	for (let e = 0; e < r; e += 1) {
		let t = (e - 1 + r) % r;
		if (i[e] !== i[t]) {
			o = !0;
			break;
		}
	}
	if (!a && !o) return "";
	let s = [];
	for (let e = 0; e < r; e += 1) {
		let a = n[e], o = n[(e - 1 + r) % r], c = i[e];
		if (i[(e - 1 + r) % r] !== c) {
			let e = _(o, a, t);
			s.push(p(e, t, !1));
		}
		s.push(p(a, t, !c));
	}
	return x(s);
}
function C(e, t, n, r, i = [0, 0]) {
	return x(y(e).map((e) => t(e, n, r, i)));
}
function w(e, { projection: t = "globe", width: n, height: r, center: i = [0, 0] } = {}) {
	if (!e || !n || !r) return "";
	let a = T[t] || T.globe, s = t === "globe" ? o(n, r, i) : null, c = (e) => e.map((e) => t === "globe" ? S(e, s) : C(e, a, n, r, i)).filter(Boolean).join(" ");
	return e.type === "Polygon" ? c(e.coordinates) : e.type === "MultiPolygon" ? e.coordinates.map(c).filter(Boolean).join(" ") : "";
}
var T = {
	mercator([e, t], n, r, i) {
		t = Math.max(Math.min(t, 85.05113), -85.05113);
		let [a, o] = i, s = (e - a) * Math.PI / 180, c = n / (2 * Math.PI) * (s + Math.PI), l = (e) => Math.log(Math.tan(Math.PI / 4 + e * Math.PI / 180 / 2)), u = l(85.05113), d = l(-72);
		return [c, (l(t) - u) / (d - u) * r];
	},
	equirectangular([e, t], n, r, i) {
		let [a, o] = i;
		return t = Math.max(Math.min(t, 90), -80), [(e - a + 180) / 360 * n, (90 - (t - o)) / 170 * r];
	},
	robinson([e, t], n, r, i) {
		let a = [
			[1, 0],
			[.9986, .062],
			[.9954, .124],
			[.99, .186],
			[.9822, .248],
			[.973, .31],
			[.96, .372],
			[.9427, .434],
			[.9216, .4958],
			[.8962, .5571],
			[.8679, .6176],
			[.835, .6769],
			[.7986, .7346],
			[.7597, .7903],
			[.7186, .8435],
			[.6732, .8936],
			[.6213, .9394],
			[.5722, .9761],
			[.5322, 1]
		], [o, s] = i;
		e -= o, t = Math.max(-89.9999, Math.min(89.9999, t));
		let c = Math.abs(t), l = Math.floor(c / 5), u = c % 5 / 5, [d, f] = a[l], [p, m] = a[Math.min(l + 1, a.length - 1)], h = d + (p - d) * u, g = f + (m - f) * u, _ = e * Math.PI / 180, v = n / 2 + n / 2 * h * _ / Math.PI, y = r / 2 - r / 2 * Math.sign(t) * g / 1;
		return !Number.isFinite(v) || !Number.isFinite(y) ? [-9999, -9999] : [v, y];
	},
	mollweide([e, t], n, r, i) {
		t = Math.max(Math.min(t, 89.9999), -89.9999);
		let [a] = i, o = (e - a) * Math.PI / 180, s = t * Math.PI / 180, c = s, l = 1, u = 0;
		for (; Math.abs(l) > 1e-10 && u < 10;) l = -(2 * c + Math.sin(2 * c) - Math.PI * Math.sin(s)) / (2 + 2 * Math.cos(2 * c)), c += l, u++;
		isFinite(c) || (c = s > 0 ? Math.PI / 2 : -Math.PI / 2);
		let d = n / (2 * Math.SQRT2), f = (d * (2 * Math.SQRT2 / Math.PI) * o * Math.cos(c) + 2 * d) / (4.6 * d) * n, p = -d * Math.SQRT2 * Math.sin(c), m = -d * Math.SQRT2, h = d * Math.SQRT2, g = (p - m) / (h - m) * r;
		return [isFinite(f) ? f : 0, isFinite(g) ? g : 0];
	},
	winkelTripel([e, t], n, r, i) {
		let [a, o] = i, s = (e - a) * Math.PI / 180, c = t * Math.PI / 180, l = s * Math.cos(Math.PI / 6), u = c, d = Math.acos(Math.cos(c) * Math.cos(s / 2)), f = 2 * Math.cos(c) * Math.sin(s / 2) / (Math.sin(d) / d || 1), p = Math.sin(c) / (Math.sin(d) / d || 1);
		return [n / 2 + n / (2 * Math.PI) * (l + f) / 2, r / 2 - r / (2 * Math.PI) * (u + p) / 2];
	},
	aitoff([e, t], n, r, i) {
		Math.sinc || (Math.sinc = function(e) {
			return e === 0 ? 1 : Math.sin(Math.PI * e) / (Math.PI * e);
		});
		let [a, o] = i, s = (e - a) * Math.PI / 180, c = t * Math.PI / 180, l = Math.acos(Math.cos(c) * Math.cos(s / 2)), u = 0, d = 0;
		return l !== 0 && (u = 2 * Math.cos(c) * Math.sin(s / 2) / Math.sinc(l / Math.PI), d = Math.sin(c) / Math.sinc(l / Math.PI)), u = n / 2 + n / 4 * u, d = r / 2 - r / 2 * d, [u, d];
	},
	hammer([e, t], n, r, i) {
		let [a, o] = i, s = (e - a) * Math.PI / 180, c = t * Math.PI / 180, l = Math.sqrt(1 + Math.cos(c) * Math.cos(s / 2));
		return [n / 2 + n / 2 * (2 * Math.SQRT2 * Math.cos(c) * Math.sin(s / 2) / l) / 2.8284271247461903, r / 2 - r / 2 * (Math.SQRT2 * Math.sin(c) / l) / 1.4142135623730951];
	},
	bonne([e, t], n, r, i = [0, 0]) {
		let a = 45 * Math.PI / 180, [o, s] = i, c = (e - o) * Math.PI / 180, l = Math.max(Math.min(t, 89.9), -89.9) * Math.PI / 180, u = 1 / Math.tan(a), d = u + a - l, f = d === 0 ? 0 : c * Math.cos(l) / d, p = Math.min(n, r) / 2.6;
		return [n / 2 + p * d * Math.sin(f), r / 2 - p * (u - d * Math.cos(f))];
	},
	sinusoidal([e, t], n, r, i) {
		let [a, o] = i, s = (e - a) * Math.PI / 180, c = t * Math.PI / 180;
		return [n / 2 + n / (2 * Math.PI) * s * Math.cos(c), r / 2 - r / Math.PI * c];
	},
	gallPeters([e, t], n, r, i) {
		let [a, o] = i;
		return t = Math.max(Math.min(t, 89.9999), -89.9999), [(e - a + 180) / 360 * n, r / 2 - r / Math.PI * Math.sin(t * Math.PI / 180)];
	},
	vanDerGrinten([e, t], n, r, i = [0, 0]) {
		let a = Math.PI, o = Math.PI / 2, s = 1e-6, c = (e - (i[0] || 0)) * a / 180, l = (t - (i[1] || 0)) * a / 180, u = Math.abs(c), d = Math.abs(l), f, p;
		if (d < s) f = c, p = 0;
		else if (u < s || Math.abs(d - o) < s) f = 0, p = Math.sign(l) * a * Math.tan(Math.asin(d / o) / 2);
		else {
			let e = d / o, t = Math.cos(Math.asin(e)), n = .5 * Math.abs(a / c - c / a), r = n * n, i = t / (e + t - 1), s = i * (2 / e - 1), u = s * s, m = u + r, h = i - u, g = r + i;
			f = Math.sign(c) * a * (n * h + Math.sqrt(Math.max(0, r * h * h - m * (i * i - u)))) / m, p = Math.sign(l) * a * (s * g - n * Math.sqrt(Math.max(0, (r + 1) * m - g * g))) / m;
		}
		let m = n / 2 / a * .98, h = n / 2, g = r / 2;
		return [h + f * m, g - p * m];
	},
	globe(e, t, n, r = [0, 0]) {
		return m(e, t, n, r, !1);
	},
	azimuthalEquidistant([e, t], n, r, i = [0, 0]) {
		let a = (e) => e * Math.PI / 180, [o, s] = i, c = a(e - o), l = a(t), u = a(s), d = Math.sin(u) * Math.sin(l) + Math.cos(u) * Math.cos(l) * Math.cos(c), f = Math.acos(Math.max(-1, Math.min(1, d))), p = f === 0 ? 1 : f / Math.sin(f), m = p * Math.cos(l) * Math.sin(c), h = p * (Math.cos(u) * Math.sin(l) - Math.sin(u) * Math.cos(l) * Math.cos(c)), g = Math.min(n, r) / 2 / Math.PI, _ = n / 2, v = r / 2;
		return [_ + m * g, v - h * g];
	}
};
function E(e, t, n, r, i = [0, 0]) {
	if (e === T.globe) return u(n, r);
	let a = Infinity, o = Infinity, s = -Infinity, c = -Infinity;
	for (let l of t) {
		let t = l.geometry, u = t.type === "Polygon" ? [t.coordinates] : t.type === "MultiPolygon" ? t.coordinates : [];
		for (let t of u) for (let l of t) for (let [t, u] of l) {
			let [l, d] = e([t, u], n, r, i);
			!Number.isFinite(l) || !Number.isFinite(d) || (a = Math.min(a, l), s = Math.max(s, l), o = Math.min(o, d), c = Math.max(c, d));
		}
	}
	return !Number.isFinite(a) || !Number.isFinite(o) || !Number.isFinite(s) || !Number.isFinite(c) ? {
		minX: 0,
		minY: 0,
		width: n,
		height: r
	} : {
		minX: Math.floor(a) - 10,
		minY: Math.floor(o) - 10,
		width: Math.ceil(s - a) + 20,
		height: Math.ceil(c - o) + 20
	};
}
function D(e, t, n) {
	let r = e.find((e) => e.properties.admin === t || e.properties.name === t), i = e.find((e) => e.properties.admin === n || e.properties.name === n);
	return r && i && (r.geometry.type === "Polygon" && (r.geometry = {
		type: "MultiPolygon",
		coordinates: [r.geometry.coordinates]
	}), i.geometry.type === "Polygon" ? r.geometry.coordinates.push(i.geometry.coordinates) : i.geometry.type === "MultiPolygon" && r.geometry.coordinates.push(...i.geometry.coordinates), e = e.filter((e) => e.properties.admin !== n && e.properties.name !== n)), e;
}
function O(e, t) {
	let n = Array.isArray(t) ? t.map((e) => ({ ...e })) : t.features ? t.features.map((e) => ({ ...e })) : [];
	return e.style.chart.territory.showTaiwanAsPartOfChina && (n = D(n, "China", "Taiwan")), t.type === "FeatureCollection" ? {
		...t,
		features: n
	} : n;
}
var k = {
	projections: T,
	getProjectedBounds: E,
	setupTerritories: O,
	simplifyGeoJSON: e,
	geoToPath: w,
	getGlobeMetrics: a,
	getGlobeBounds: u,
	isGlobePointVisible: f
};
//#endregion
export { k as t };
