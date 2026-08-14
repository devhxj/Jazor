import { toValue as e, watchEffect as t } from "vue";
//#region src/useHints.js
var n = {
	noHint: {
		test: () => !0,
		message: ["There is no advice available for this component at the moment 🏖️"]
	},
	emptyArray: {
		test: (e) => e.length === 0,
		message: [
			"👀 The dataset is empty.",
			"",
			"▶️ Check the documentation or the dataset TS type to see how to populate the dataset for this component."
		]
	},
	singleSeries: {
		test: (e) => e.length === 1,
		message: [
			"👀 The dataset only has a single series. Consider:",
			"",
			"▶️ Using a value display instead of a chart component, or using VueUiKpi."
		]
	}
};
function r({ config: n, dataset: r, component: i, rules: a }) {
	if (!a || a?.length === 0) return;
	let o;
	t(() => {
		if (!(e(n) ?? {}).devHints?.enable) {
			o = void 0;
			return;
		}
		let t = e(r);
		if (!t) {
			o = [
				"",
				i,
				"💬 Vue Data UI advice:",
				"---------------------",
				"",
				"❌ Invalid dataset",
				"",
				"---------------------",
				"Turn off advice by setting config.devHints.enable: false"
			].join("\n"), console.warn(o);
			return;
		}
		let s = a.filter((e) => e.test(t)).map((e) => e.message.join("\n")), c = s.length ? s.join("\n\n") : "✅ Your dataset is appropriate";
		if (c === o) return;
		o = c;
		let l = ["color: #888", "font-weight: normal"].join(";");
		console.info([
			"",
			`💬 ${i}`,
			"%cVue Data UI advice:%c",
			"%c-------------------%c",
			"",
			c,
			"",
			"%c-------------------",
			"Turn off advice by setting devHints.enable: false in the component's config%c"
		].join("\n"), l, "", l, "", l, "");
	});
}
//#endregion
export { r as n, n as t };
