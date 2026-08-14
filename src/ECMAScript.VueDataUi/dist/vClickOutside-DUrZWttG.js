//#region src/directives/vClickOutside.js
var e = {
	beforeMount(e, t) {
		e.clickOutsideEvent = function(n) {
			e === n.target || e.contains(n.target) || t.value(n);
		}, document.addEventListener("click", e.clickOutsideEvent);
	},
	unmounted(e) {
		document.removeEventListener("click", e.clickOutsideEvent);
	}
};
//#endregion
export { e as t };
