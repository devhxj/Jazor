import { computed as e, ref as t } from "vue";
//#region src/useUserOptionState.js
function n({ config: n }) {
	let r = e(() => n.userOptions.showOnChartHover), i = e(() => n.userOptions.keepStateOnChartLeave), a = t(!n.userOptions.showOnChartHover);
	function o(e = !1) {
		r.value && (a.value = e);
	}
	return {
		userOptionsVisible: a,
		keepUserOptionState: i,
		setUserOptionsVisibility: o
	};
}
//#endregion
export { n as t };
