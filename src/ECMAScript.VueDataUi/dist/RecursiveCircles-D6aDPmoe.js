import { Fragment as e, createBlock as t, createCommentVNode as n, createElementBlock as r, createElementVNode as i, mergeProps as a, openBlock as o, ref as s, renderList as c, renderSlot as l, resolveComponent as u, watch as d, withCtx as f, withModifiers as p } from "vue";
//#region src/atoms/RecursiveCircles.vue
var m = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width",
	"onClick",
	"onMouseover"
], h = [
	"x",
	"y",
	"height",
	"width",
	"onClick",
	"onMouseover"
], g = {
	__name: "RecursiveCircles",
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
		},
		linkColor: {
			type: String,
			default: "#CCCCCC"
		},
		stroke: {
			type: String,
			default: "#FFFFFF"
		},
		strokeHovered: {
			type: String,
			default: "#000000"
		}
	},
	emits: ["click", "hover"],
	setup(g, { emit: _ }) {
		let v = g, y = _;
		function b(e) {
			y("click", e);
		}
		function x(e) {
			y("hover", e);
		}
		let S = s([]);
		return d(() => v.dataset, (e) => {
			S.value = e || [];
		}, { immediate: !0 }), (s, d) => {
			let _ = u("RecursiveCircles", !0);
			return o(!0), r(e, null, c(g.dataset, (u, v) => (o(), r(e, null, [u.polygonPath && u.polygonPath.coordinates ? (o(), r(e, { key: 0 }, [(o(!0), r(e, null, c(u.polygonPath.coordinates, (t, c) => (o(), r(e, { key: `node_${v}_${c}` }, [l(s.$slots, "node-svg", { nodeSvg: {
				x: t.x,
				y: t.y,
				radius: u.circleRadius,
				color: u.color,
				stroke: g.stroke,
				strokeWidth: u.circleRadius / 12,
				isSelected: g.hoveredUid && g.hoveredUid === u.uid,
				onClick: () => b(u),
				onEnter: () => x(u),
				onLeave: () => x(null)
			} }, () => [i("circle", {
				cx: t.x,
				cy: t.y,
				r: u.circleRadius,
				fill: `url(#gradient_${u.color})`,
				stroke: g.hoveredUid && g.hoveredUid === u.uid ? g.strokeHovered : g.stroke,
				"stroke-width": g.hoveredUid && g.hoveredUid === u.uid ? u.circleRadius / 6 : u.circleRadius / 12,
				style: { cursor: "pointer" },
				onClick: (e) => b(u),
				onMouseover: (e) => x(u),
				onMouseleave: d[0] ||= (e) => x(null)
			}, null, 40, m)]), s.$slots.node ? (o(), r("foreignObject", {
				key: 0,
				x: t.x - u.circleRadius,
				y: t.y - u.circleRadius,
				height: u.circleRadius * 2,
				width: u.circleRadius * 2,
				style: { overflow: "visible" },
				onClick: p((e) => b(u), ["stop"]),
				onMouseover: (e) => x(u),
				onMouseleave: d[1] ||= (e) => x(null)
			}, [l(s.$slots, "node", a({ ref_for: !0 }, { node: u }))], 40, h)) : n("", !0)], 64))), 128)), u.nodes && u.nodes.length > 0 ? (o(), t(_, {
				key: 0,
				dataset: u.nodes,
				color: g.color,
				stroke: g.stroke,
				strokeHovered: g.strokeHovered,
				hoveredUid: g.hoveredUid,
				onClick: b,
				onHover: x
			}, {
				node: f(({ node: e }) => [l(s.$slots, "node", a({ ref_for: !0 }, { node: e }))]),
				"node-svg": f(({ nodeSvg: e }) => [l(s.$slots, "node-svg", a({ ref_for: !0 }, { nodeSvg: e }))]),
				_: 2
			}, 1032, [
				"dataset",
				"color",
				"stroke",
				"strokeHovered",
				"hoveredUid"
			])) : n("", !0)], 64)) : n("", !0)], 64))), 256);
		};
	}
};
//#endregion
export { g as default };
