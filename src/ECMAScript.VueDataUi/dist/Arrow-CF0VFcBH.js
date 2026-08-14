import { q as e } from "./lib-Bttd6u5E.js";
import { computed as t, createElementBlock as n, createElementVNode as r, openBlock as i, unref as a } from "vue";
//#region src/atoms/Arrow.vue
var o = { class: "vue-ui-element-arrow" }, s = [
	"id",
	"viewBox",
	"refX",
	"refY",
	"markerWidth",
	"markerHeight"
], c = ["d", "fill"], l = [
	"id",
	"viewBox",
	"refX",
	"refY",
	"markerWidth",
	"markerHeight"
], u = ["d", "fill"], d = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width",
	"stroke-linecap",
	"stroke-dasharray",
	"marker-end",
	"marker-start"
], f = {
	__name: "Arrow",
	props: {
		markerEnd: {
			type: Boolean,
			default: !0
		},
		markerSize: {
			type: Number,
			default: 10
		},
		markerStart: {
			type: Boolean,
			default: !1
		},
		stroke: {
			type: String,
			default: "#2D353C"
		},
		strokeDasharray: {
			type: Number,
			default: 0
		},
		strokeLinecap: {
			type: String,
			default: "round"
		},
		strokeWidth: {
			type: Number,
			default: 1
		},
		x1: {
			type: Number,
			default: 0
		},
		x2: {
			type: Number,
			default: 0
		},
		y1: {
			type: Number,
			default: 0
		},
		y2: {
			type: Number,
			default: 0
		}
	},
	setup(f) {
		let p = f, m = e(), h = t(() => `0 0 ${p.markerSize} ${p.markerSize}`), g = t(() => p.markerSize / 2), _ = t(() => g.value + p.markerSize / 10);
		return (e, t) => (i(), n("g", o, [r("defs", null, [r("marker", {
			id: `arrow_end_${a(m)}`,
			orient: "auto",
			viewBox: h.value,
			refX: g.value,
			refY: g.value,
			markerWidth: _.value,
			markerHeight: _.value
		}, [r("path", {
			d: `M 0 0 L ${f.markerSize} ${g.value} L 0 ${f.markerSize} z`,
			fill: f.stroke
		}, null, 8, c)], 8, s), r("marker", {
			id: `arrow_start_${a(m)}`,
			orient: "auto-start-reverse",
			viewBox: h.value,
			refX: g.value,
			refY: g.value,
			markerWidth: _.value,
			markerHeight: _.value
		}, [r("path", {
			d: `M 0 0 L ${f.markerSize} ${g.value} L 0 ${f.markerSize} z`,
			fill: f.stroke
		}, null, 8, u)], 8, l)]), r("line", {
			x1: f.x1,
			y1: f.y1,
			x2: f.x2,
			y2: f.y2,
			stroke: f.stroke,
			"stroke-width": f.strokeWidth,
			"stroke-linecap": f.strokeLinecap,
			"stroke-dasharray": f.strokeDasharray,
			"marker-end": f.markerEnd ? `url(#arrow_end_${a(m)})` : "",
			"marker-start": f.markerStart ? `url(#arrow_start_${a(m)})` : ""
		}, null, 8, d)]));
	}
};
//#endregion
export { f as t };
