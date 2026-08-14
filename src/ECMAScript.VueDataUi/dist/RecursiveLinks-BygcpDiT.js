import { Fragment as e, createBlock as t, createCommentVNode as n, createElementBlock as r, createElementVNode as i, openBlock as a, ref as o, renderList as s, resolveComponent as c, watch as l } from "vue";
//#region src/atoms/RecursiveLinks.vue
var u = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], d = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], f = {
	__name: "RecursiveLinks",
	props: {
		dataset: {
			type: Array,
			default: () => []
		},
		color: {
			type: String,
			default: "#DDDDDD"
		},
		backgroundColor: {
			type: String,
			default: "#FFFFFF"
		},
		useChildColor: {
			type: Boolean,
			default: !1
		}
	},
	setup(f) {
		let p = f, m = o([]);
		return l(() => p.dataset, (e) => {
			let t = e || [];
			t.forEach((e) => {
				e.nodes && e.nodes.length > 0 && e.nodes.forEach((t) => {
					t.ancestor !== e && (t.ancestor = e);
				});
			}), m.value = t;
		}, { immediate: !0 }), (o, l) => {
			let p = c("RecursiveLinks", !0);
			return a(), r(e, null, [(a(!0), r(e, null, s(m.value, (t, o) => (a(), r(e, { key: `level_${o}` }, [t.polygonPath && t.polygonPath.coordinates ? (a(!0), r(e, { key: 0 }, s(t.polygonPath.coordinates, (s, c) => (a(), r(e, { key: `node_${o}_${c}` }, [t.ancestor && t.ancestor.polygonPath ? (a(), r(e, { key: 0 }, [i("line", {
				x1: s.x,
				y1: s.y,
				x2: t.ancestor.polygonPath.coordinates[0].x,
				y2: t.ancestor.polygonPath.coordinates[0].y,
				stroke: f.backgroundColor,
				"stroke-width": t.strokeWidth * 1.5
			}, null, 8, u), i("line", {
				x1: s.x,
				y1: s.y,
				x2: t.ancestor.polygonPath.coordinates[0].x,
				y2: t.ancestor.polygonPath.coordinates[0].y,
				stroke: f.useChildColor ? t.color : f.color,
				"stroke-width": t.strokeWidth
			}, null, 8, d)], 64)) : n("", !0)], 64))), 128)) : n("", !0)], 64))), 128)), (a(!0), r(e, null, s(m.value, (i) => (a(), r(e, { key: `children_${i.uid || i.name}` }, [i.polygonPath && i.polygonPath.coordinates ? (a(), r(e, { key: 0 }, [i.nodes && i.nodes.length > 0 ? (a(), t(p, {
				key: 0,
				dataset: i.nodes,
				color: f.color,
				useChildColor: f.useChildColor,
				backgroundColor: f.backgroundColor
			}, null, 8, [
				"dataset",
				"color",
				"useChildColor",
				"backgroundColor"
			])) : n("", !0)], 64)) : n("", !0)], 64))), 128))], 64);
		};
	}
};
//#endregion
export { f as default };
