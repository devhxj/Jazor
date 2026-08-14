import { t as e } from "./Shape-C21CMlWS.js";
import { t } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as n } from "./BaseIcon-BfndwIWE.js";
import { Fragment as r, computed as i, createCommentVNode as a, createElementBlock as o, createElementVNode as s, createVNode as c, normalizeClass as l, normalizeStyle as u, onMounted as d, openBlock as f, ref as p, renderList as m, renderSlot as h, toDisplayString as g, unref as _, useCssVars as v, withKeys as y } from "vue";
//#region src/atoms/DataTable.vue
var b = { class: "vue-ui-data-table" }, x = { style: {
	display: "flex",
	"align-items": "center",
	"justify-content": "flex-end",
	"padding-right": "3px",
	gap: "3px"
} }, S = { style: {
	width: "12px",
	height: "12px"
} }, C = {
	key: 0,
	height: "12",
	width: "12",
	viewBox: "0 0 20 20",
	style: { background: "none" }
}, w = ["fill"], T = ["data-cell"], E = {
	dir: "auto",
	style: {
		display: "flex",
		"align-items": "center",
		gap: "5px",
		"justify-content": "flex-end",
		width: "100%",
		"padding-right": "3px"
	}
}, D = {
	key: 0,
	height: "12",
	width: "12",
	viewBox: "0 0 20 20",
	style: {
		background: "none",
		overflow: "visible"
	}
}, O = /*#__PURE__*/ t({
	__name: "DataTable",
	props: {
		colNames: {
			type: Array,
			default() {
				return [];
			}
		},
		head: Array,
		body: Array,
		title: String,
		config: Object,
		withCloseButton: {
			type: Boolean,
			default: !0
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		}
	},
	emits: ["close"],
	setup(t, { emit: O }) {
		v((e) => ({ v7ded12c6: _(F) }));
		let k = t, { backgroundColor: A, color: j, outline: M } = k.config.th, { backgroundColor: N, color: P, outline: F } = k.config.td, I = i(() => k.config.breakpoint), L = p(null), R = p(!1);
		d(() => {
			let e = new ResizeObserver((e) => {
				e.forEach((e) => {
					R.value = e.contentRect.width < I.value;
				});
			});
			L.value && e.observe(L.value);
		});
		let z = O;
		return (i, d) => (f(), o("div", {
			ref_key: "tableContainer",
			ref: L,
			style: u(`width: 100%; container-type: inline-size; position:relative;${t.withCloseButton ? "padding-top: 36px;" : ""}overflow:auto`),
			class: l({
				"atom-data-table": !0,
				"vue-ui-responsive": R.value
			})
		}, [t.withCloseButton ? (f(), o("div", {
			key: 0,
			"data-dom-to-png-ignore": "",
			role: "button",
			tabindex: "0",
			style: u(`width:32px; position: absolute; top: 0; right:4px; padding: 0 0px; display: flex; align-items:center;justify-content:center;height: 36px; width: 32px; cursor:${t.isCursorPointer ? "pointer" : "default"}; background:${_(A)};`),
			onClick: d[0] ||= (e) => z("close"),
			onKeypress: d[1] ||= y((e) => z("close"), ["enter"])
		}, [c(n, {
			name: "close",
			stroke: _(j),
			"stroke-width": 2
		}, null, 8, ["stroke"])], 36)) : a("", !0), s("table", b, [
			s("caption", {
				style: u({
					backgroundColor: _(A),
					color: _(j),
					outline: _(M)
				}),
				class: "vue-ui-data-table__caption"
			}, g(t.title), 5),
			s("thead", null, [s("tr", {
				role: "row",
				style: u([{ "font-variant-numeric": "tabular-nums" }, {
					backgroundColor: _(A),
					color: _(j)
				}]),
				class: "vue-ui-data-table__thead-row"
			}, [(f(!0), o(r, null, m(t.head, (e, t) => (f(), o("th", {
				role: "cell",
				style: u({ outline: _(M) }),
				key: `th_${t}`
			}, [s("div", x, [s("div", S, [e?.color ? (f(), o("svg", C, [s("circle", {
				cx: "10",
				cy: "10",
				r: "10",
				fill: e.color
			}, null, 8, w)])) : a("", !0)]), h(i.$slots, "th", { th: e }, void 0, !0)])], 4))), 128))], 4)]),
			s("tbody", null, [(f(!0), o(r, null, m(t.body, (n, d) => (f(), o("tr", {
				role: "row",
				style: u([{ "font-variant-numeric": "tabular-nums" }, {
					backgroundColor: _(N),
					color: _(P)
				}]),
				class: l({
					"vue-ui-data-table__tbody__row": !0,
					"vue-ui-data-table__tbody__row-even": d % 2 == 0,
					"vue-ui-data-table__tbody__row-odd": d % 2 != 0
				})
			}, [(f(!0), o(r, null, m(n, (n, r) => (f(), o("td", {
				role: "cell",
				"data-cell": (t.colNames[r] && t.colNames[r].name ? t.colNames[r].name : "") || t.colNames[r] || "",
				style: u({ outline: _(F) }),
				class: "vue-ui-data-table__tbody__td"
			}, [s("div", E, [n?.color ? (f(), o("svg", D, [c(e, {
				plot: {
					x: 10,
					y: 10
				},
				color: n.color,
				radius: 9,
				shape: t.config.shape || n.shape || "circle"
			}, null, 8, ["color", "shape"])])) : a("", !0), h(i.$slots, "td", { td: n }, void 0, !0)])], 12, T))), 256))], 6))), 256))])
		])], 6));
	}
}, [["__scopeId", "data-v-c3927505"]]);
//#endregion
export { O as default };
