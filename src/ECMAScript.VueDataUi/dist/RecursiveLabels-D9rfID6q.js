import { Fragment as e, createBlock as t, createCommentVNode as n, createElementBlock as r, openBlock as i, ref as a, renderList as o, resolveComponent as s, toDisplayString as c, watch as l } from "vue";
//#region src/atoms/RecursiveLabels.vue
var u = [
	"x",
	"y",
	"fill",
	"font-size"
], d = {
	__name: "RecursiveLabels",
	props: {
		color: {
			type: String,
			default: "#000000"
		},
		dataset: {
			type: Array,
			default: () => []
		},
		hoveredUid: {
			type: String,
			default: null
		}
	},
	emits: ["zoom", "hover"],
	setup(d, { emit: f }) {
		let p = d, m = a([]);
		return l(() => p.dataset, (e) => {
			m.value = e || [];
		}, { immediate: !0 }), (a, l) => {
			let f = s("RecursiveLabels", !0);
			return i(!0), r(e, null, o(m.value, (a, s) => (i(), r(e, { key: `level_${s}` }, [a.polygonPath && a.polygonPath.coordinates ? (i(), r(e, { key: 0 }, [(i(!0), r(e, null, o(a.polygonPath.coordinates, (e, t) => (i(), r("text", {
				key: `node_${s}_${t}`,
				x: e.x,
				y: e.y + a.circleRadius * 2,
				fill: d.color,
				"font-size": a.circleRadius,
				"text-anchor": "middle",
				style: {
					opacity: "0.8",
					"pointer-events": "none"
				}
			}, c(a.name), 9, u))), 128)), a.nodes && a.nodes.length > 0 ? (i(), t(f, {
				key: 0,
				dataset: a.nodes,
				color: d.color,
				hoveredUid: d.hoveredUid
			}, null, 8, [
				"dataset",
				"color",
				"hoveredUid"
			])) : n("", !0)], 64)) : n("", !0)], 64))), 128);
		};
	}
};
//#endregion
export { d as default };
