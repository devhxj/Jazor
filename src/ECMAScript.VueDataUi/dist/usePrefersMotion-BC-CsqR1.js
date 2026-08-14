import { onBeforeUnmount as e, onMounted as t, ref as n } from "vue";
//#region src/usePrefersMotion.js
function r() {
	let r = n(!1);
	return t(() => {
		let t = window.matchMedia("(prefers-reduced-motion: reduce)"), n = () => {
			r.value = t.matches;
		};
		n(), t.addEventListener("change", n), e(() => {
			t.removeEventListener("change", n);
		});
	}), r;
}
//#endregion
export { r as t };
