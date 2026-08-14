import { Ft as e } from "./lib-Bttd6u5E.js";
//#region src/labelUtils.js
function t({ config: t = {
	useValueParens: !1,
	usePercentageParens: !0,
	showValueFirst: !0
}, val: n, percentage: r, showVal: i = !0, showPercentage: a = !0 }) {
	if (!i && !a) return "";
	let o = {
		value: t.useValueParens ? e(n) : n,
		percentage: t.usePercentageParens ? e(r) : r
	};
	return i && a ? t.showValueFirst ? `${o.value} ${o.percentage}` : `${o.percentage} ${o.value}` : i ? o.value : a ? o.percentage : "";
}
function n({ rounding: e, num: t, filler: n = "-" }) {
	return t.toFixed(e).split("").map((e) => n).join("");
}
//#endregion
export { n, t };
