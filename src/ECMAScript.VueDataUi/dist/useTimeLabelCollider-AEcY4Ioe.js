import { nextTick as e, watch as t } from "vue";
//#region src/useTimeLabelCollider.js
function n({ timeLabelsEls: n, timeLabels: r, slicer: i, configRef: a, rotationPath: o, autoRotatePath: s, isAutoSize: c, setViewBox: l, forceResizeObserver: u, callback: d, targetClass: f = ".vue-data-ui-time-label", rotation: p = -30.0001, height: m = null, width: h = null }) {
	function g(e, t) {
		return t.reduce((e, t) => e && e[t], e);
	}
	function _(e, t, n) {
		t.slice(0, -1).reduce((e, t) => e[t], e)[t.slice(-1)] = n;
	}
	function v(e) {
		let t = /translate\(\s*([^\s,]+)\s*,\s*([^\s,]+)\s*\)/.exec(e);
		return t ? {
			x: parseFloat(t[1]),
			y: parseFloat(t[2])
		} : {
			x: 0,
			y: 0
		};
	}
	async function y() {
		await e();
		let t = n.value;
		if (!t) return;
		let r = Array.from(t.querySelectorAll(f)).map((e) => ({
			...v(e.getAttribute("transform")),
			width: e.getBBox().width
		})), i = !1;
		for (let e = 0; e < r.length && !i; e += 1) for (let t = e + 1; t < r.length; t += 1) {
			let n = r[e], a = r[t];
			if (!(n.x + n.width + 4 < a.x || a.x + a.width + 4 < n.x)) {
				i = !0;
				break;
			}
		}
		let s = g(a.value, o);
		i && !s ? (_(a.value, o, p), d && d({ collision: i }), c.value && l && u && (l(), u())) : !i && s === p && (_(a.value, o, 0), d && d({ collision: i }));
	}
	function b(e, t) {
		let n;
		return (...r) => {
			clearTimeout(n), n = setTimeout(() => e(...r), t);
		};
	}
	let x = b(y, 200);
	return m && h && t([() => h.value, () => m.value], async ([e, t], [n, r]) => {
		g(a.value, s) && (e !== n || t !== r ? x() : await y());
	}), t([
		() => r.value,
		() => g(a.value, o),
		() => i.value.start,
		() => i.value.end
	], async ([, , e, t], [, , n, r]) => {
		g(a.value, s) && (e !== n || t !== r ? x() : await y());
	}, { immediate: !0 }), { detectTimeLabelCollision: y };
}
//#endregion
export { n as t };
