import { nextTick as e, onBeforeUnmount as t, ref as n, watch as r } from "vue";
//#region src/useTableResponsive.js
function i(i, a) {
	let o = n(!1), s = null;
	function c() {
		s &&= (s.disconnect(), null);
	}
	async function l() {
		c(), await e();
		let t = i.value;
		t && (s = new ResizeObserver((e) => {
			let t = e[0].contentRect.width;
			o.value = t < a.value;
		}), s.observe(t));
	}
	return r([i, a], () => {
		l();
	}, { immediate: !0 }), t(c), {
		isResponsive: o,
		start: l,
		stop: c
	};
}
//#endregion
export { i as t };
