//#region src/canvas-lib.js
function e(e, t, n, r = {}) {
	if (n <= 0) return;
	e.save();
	let { color: i = "black", lineWidth: a = 1, lineCap: o = "round", lineJoin: s = "round", lineDash: c = null, lineDashoffset: l = 0, shadowColor: u = null, shadowBlur: d = 0, shadowOffsetX: f = 0, shadowOffsetY: p = 0, fillStyle: m = "#FFFFFF" } = r;
	e.beginPath(), e.arc(t.x, t.y, n, 0, 2 * Math.PI), e.strokeStyle = i, e.lineWidth = a, e.lineCap = o, e.lineJoin = s, c && (e.setLineDash(c), e.lineDashoffset = l), u && (e.shadowColor = u, e.shadowOffsetX = f, e.shadowOffsetY = p), d && (e.shadowBlur = d), m && (e.fillStyle = m), e.fillStyle && e.fill(), e.stroke(), e.restore();
}
function t(e, t, n = {}) {
	if (!t.length) return;
	e.save();
	let { color: r = "black", lineWidth: i = 1, lineCap: a = "round", lineJoin: o = "round", lineDash: s = null, lineDashoffset: c = 0, shadowColor: l = null, shadowBlur: u = 0, shadowOffsetX: d = 0, shadowOffsetY: f = 0 } = n;
	e.beginPath(), e.moveTo(t[0].x, t[0].y);
	for (let n = 1; n < t.length; n += 1) e.lineTo(t[n].x, t[n].y);
	e.strokeStyle = r, e.lineWidth = i, e.lineCap = a, e.lineJoin = o, s && (e.setLineDash(s), e.lineDashoffset = c), l && (e.shadowColor = l, e.shadowOffsetX = d, e.shadowOffsetY = f), u && (e.shadowBlur = u), e.stroke(), e.restore();
}
function n(e, t, n = {}) {
	if (!t.length) return;
	e.save();
	let { strokeColor: r = "black", lineWidth: i = 1, lineCap: a = "round", lineJoin: o = "round", lineDash: s = null, lineDashoffset: l = 0, fillColor: u = null, gradient: d = null, shadowColor: f = null, shadowBlur: p = 0, shadowOffsetX: m = 0, shadowOffsetY: h = 0 } = n;
	e.beginPath(), e.moveTo(t[0].x, t[0].y);
	for (let n = 1; n < t.length; n += 1) e.lineTo(t[n].x, t[n].y);
	if (e.closePath(), e.strokeStyle = r, e.lineWidth = i, e.lineCap = a, e.lineJoin = o, s && (e.setLineDash(s), e.lineDashoffset = l), f && (e.shadowColor = f, e.shadowBlur = p, e.shadowOffsetX = m, e.shadowOffsetY = h), d) {
		let t;
		d.type === "linear" ? t = e.createLinearGradient(c(d.start.x, Number.MIN_VALUE), c(d.start.y, Number.MIN_VALUE), c(d.end.x, Number.MIN_VALUE * 2), c(d.end.y, Number.MIN_VALUE * 2)) : d.type === "radial" && (t = e.createRadialGradient(c(d.start.x, Number.MIN_VALUE), c(d.start.y, Number.MIN_VALUE), d.start.r || 0, c(d.end.x, Number.MIN_VALUE * 2), c(d.end.y, Number.MIN_VALUE * 2), d.end.r || 0)), t && d.stops && (d.stops.forEach((e) => {
			t.addColorStop(e.offset, e.color);
		}), e.fillStyle = t);
	} else u && (e.fillStyle = u);
	e.fillStyle && e.fill(), r && e.stroke(), e.restore();
}
function r(e, t, n = {}) {
	if (!t.length) return;
	e.save();
	let { strokeColor: r = "black", lineWidth: i = 1, lineCap: a = "round", lineJoin: o = "round", lineDash: s = null, lineDashoffset: l = 0, fillColor: u = null, gradient: d = null, shadowColor: f = null, shadowBlur: p = 0, shadowOffsetX: m = 0, shadowOffsetY: h = 0 } = n;
	e.beginPath(), e.moveTo(t[0].x, t[0].y);
	for (let n = 1; n < t.length; n += 1) e.lineTo(t[n].x, t[n].y);
	if (e.closePath(), e.strokeStyle = r, e.lineWidth = i, e.lineCap = a, e.lineJoin = o, s && (e.setLineDash(s), e.lineDashoffset = l), f && (e.shadowColor = f, e.shadowBlur = p, e.shadowOffsetX = m, e.shadowOffsetY = h), d) {
		let t;
		d.type === "linear" ? t = e.createLinearGradient(c(d.start.x, Number.MIN_VALUE), c(d.start.y, Number.MIN_VALUE), c(d.end.x, Number.MIN_VALUE * 2), c(d.end.y, Number.MIN_VALUE * 2)) : d.type === "radial" && (t = e.createRadialGradient(c(d.start.x, Number.MIN_VALUE), c(d.start.y, Number.MIN_VALUE), d.start.r || 0, c(d.end.x, Number.MIN_VALUE * 2), c(d.end.y, Number.MIN_VALUE * 2), d.end.r || 0)), t && d.stops && (d.stops.forEach((e) => {
			t.addColorStop(e.offset, e.color);
		}), e.fillStyle = t);
	} else u && (e.fillStyle = u);
	e.fillStyle && e.fill(), r && e.stroke(), e.restore();
}
function i(e, t, n, r, i = {}) {
	e.save();
	let { font: a = "16px sans-serif", color: o = "black", align: s = "start", baseline: c = "alphabetic", maxWidth: l = void 0, rotation: u = 0, shadowColor: d = null, shadowBlur: f = 0, shadowOffsetX: p = 0, shadowOffsetY: m = 0, strokeColor: h = null, lineWidth: g = 1, globalAlpha: _ = 1 } = i;
	e.font = a, e.fillStyle = o, e.textAlign = s, e.textBaseline = c, e.globalAlpha = _, d && (e.shadowColor = d, e.shadowBlur = f, e.shadowOffsetX = p, e.shadowOffsetY = m), h && (e.strokeStyle = h, e.lineWidth = g), u !== 0 && (e.translate(n, r), e.rotate(Math.PI / 180 * u), e.translate(-n, -r)), e.fillText(t, n, r, l), h && e.strokeText(t, n, r, l), e.restore();
}
function a(e, t = 20) {
	let n;
	return function(...r) {
		let i = this;
		n || (e.apply(i, r), n = !0, setTimeout(() => n = !1, t));
	};
}
function o(e, t, n = !1) {
	let r;
	return function(...i) {
		let a = this, o = function() {
			r = null, n || e.apply(a, i);
		}, s = n && !r;
		clearTimeout(r), r = setTimeout(o, t), s && e.apply(a, i);
	};
}
function s(e) {
	let t = document.createElement("canvas");
	return t.width = e.width || 1, t.height = e.height || 1, e.width ||= 1, e.height ||= 1, t.getContext("2d").drawImage(e, 0, 0), t;
}
function c(e, t) {
	return [
		null,
		void 0,
		NaN,
		Infinity,
		-Infinity
	].includes(e) ? t : e;
}
//#endregion
//#region src/useResponsive.js
function l({ chart: e, title: t = null, slicer: n = null, legend: r = null, source: i = null, noTitle: a = null, padding: o = null }) {
	let s = 0, c = 0, l = 0, u = 0, d = 0, f = 0, p = 0, m = 0, h = 0;
	if (e) {
		let { height: g, width: _ } = e.parentNode.getBoundingClientRect();
		l = t ? t.getBoundingClientRect().height : 0, u = n ? n.getBoundingClientRect().height : 0, d = r ? r.getBoundingClientRect().height : 0, f = i ? i.getBoundingClientRect().height : 0, p = a ? a.getBoundingClientRect().height : 0, o ? (m = o.top + o.bottom, h = o.right + o.left) : (m = 0, h = 0), s = g - l - u - d - f - p - m, c = _ - h;
	}
	return {
		width: c,
		height: s,
		heightTitle: l,
		heightNoTitle: p,
		heightSource: f,
		heightNoTitle: p,
		heightLegend: d
	};
}
//#endregion
export { t as a, i as c, o as i, a as l, e as n, n as o, s as r, r as s, l as t };
