import { t as e } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { Fragment as t, createElementBlock as n, createElementVNode as r, createTextVNode as i, openBlock as a, renderList as o, renderSlot as s, toDisplayString as c } from "vue";
//#region src/atoms/A11yDataTable.vue
var l = ["id"], u = { scope: "row" }, d = /*#__PURE__*/ e({
	__name: "A11yDataTable",
	props: {
		uid: {
			String,
			required: !0
		},
		head: {
			Array,
			default: () => []
		},
		body: {
			Array,
			default: () => []
		},
		caption: {
			String,
			default: "Data table"
		},
		notice: {
			String,
			default: "A data table is available below."
		}
	},
	setup(e) {
		return (d, f) => (a(), n("div", {
			id: `chart-data-table-${e.uid}`,
			class: "sr-only",
			"data-dom-to-png-ignore": ""
		}, [r("p", null, c(e.notice), 1), r("table", null, [
			r("caption", null, c(e.caption), 1),
			r("thead", null, [r("tr", null, [(a(!0), n(t, null, o(e.head, (t, r) => (a(), n("th", {
				role: "cell",
				key: `a11y-head-${r}-${e.uid}`,
				scope: "col"
			}, [s(d.$slots, "th", { th: t }, () => [i(c(t), 1)], !0)]))), 128))])]),
			r("tbody", null, [(a(!0), n(t, null, o(e.body, (l, f) => (a(), n("tr", { key: `a11y-body-${f}-${e.uid}` }, [r("th", u, c(l[0]), 1), (a(!0), n(t, null, o(l.slice(1), (t, r) => (a(), n("td", { key: `a11y-cell-${f}-${r}-${e.uid}` }, [s(d.$slots, "td", { td: t }, () => [i(c(t), 1)], !0)]))), 128))]))), 128))])
		])], 8, l));
	}
}, [["__scopeId", "data-v-1090a7c5"]]);
//#endregion
export { d as t };
