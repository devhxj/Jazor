import { computed as e } from "vue";
//#region src/useThemeCheck.js
function t() {
	let t = e(() => (e) => !e || !e.theme ? !1 : [
		"default",
		"dark",
		"celebration",
		"celebrationNight",
		"hack",
		"zen",
		"concrete",
		"minimal",
		"minimalDark"
	].includes(e.theme));
	function n(e) {
		e?.debug && console.warn(`Vue Data UI - "${e?.theme}" is not a valid theme.\n\nAvailable themes:\n\n- default (or "")\n- dark\n- celebration\n- celebrationNight \n- hack\n- zen\n- concrete`);
	}
	return {
		isThemeValid: t,
		warnInvalidTheme: n
	};
}
//#endregion
export { t };
