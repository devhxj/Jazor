import { nextTick as e, onMounted as t, ref as n } from "vue";
//#region src/useChartAccessibility.js
function r({ config: r }) {
	let i = n(null), a = r?.text || "Chart visualization", o = r?.subtitle?.text || "";
	return t(() => {
		e(() => {
			i.value && (i.value.setAttribute("aria-label", `${a}${o ? `. ${o}` : ""}`), i.value.setAttribute("role", "img"), i.value.setAttribute("aria-live", "polite"));
		});
	}), { svgRef: i };
}
//#endregion
export { r as t };
