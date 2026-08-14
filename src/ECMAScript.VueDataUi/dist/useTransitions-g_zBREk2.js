import { computed as e, isRef as t, onMounted as n, onUnmounted as r, ref as i, watch as a } from "vue";
//#region src/useTransitions.js
function o(e) {
	return typeof e == "function" ? e() : t(e) ? e.value : e;
}
function s({ config: t, dataset: s }) {
	let c = e(() => o(t) ?? {}), l = e(() => o(s)), u = i(!!c.value.enable), d = null, f = null;
	function p() {
		d !== null && (clearTimeout(d), d = null);
	}
	function m() {
		p();
		let e = c.value, t = !!e.enable, n = Number(e.activationDelayMs) || 0;
		if (!t || n <= 0) {
			u.value = t;
			return;
		}
		u.value = !1, d = setTimeout(() => {
			u.value = !!c.value.enable, d = null;
		}, n);
	}
	function h(e) {
		f?.(), f = null, e && (f = a(l, m, { deep: !0 }));
	}
	return a(() => c.value.pauseOnDatasetChange, h, { immediate: !0 }), a(() => c.value.enable, (e) => {
		p(), u.value = !!e;
	}), a(() => c.value.activationDelayMs, () => {
		d !== null && m();
	}), n(() => {
		c.value.pauseOnLoad && m();
	}), r(() => {
		p(), f?.();
	}), {
		transitionEnabled: u,
		pause: m
	};
}
//#endregion
export { s as t };
