import { M as e, z as t } from "./lib-Bttd6u5E.js";
import { t as n } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { computed as r, createCommentVNode as i, createElementBlock as a, normalizeClass as o, normalizeStyle as s, openBlock as c } from "vue";
//#region src/atoms/Shape.vue
var l = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width"
], u = [
	"d",
	"fill",
	"stroke",
	"stroke-width"
], d = [
	"points",
	"fill",
	"stroke",
	"stroke-width"
], f = /*#__PURE__*/ n({
	__name: "Shape",
	props: {
		color: String,
		isSelected: {
			type: Boolean,
			default: !1
		},
		limit: {
			type: Number,
			default: 3
		},
		plot: Object,
		radius: Number,
		shape: String,
		stroke: String,
		strokeWidth: Number,
		zoom: {
			type: Number,
			default: 1.3
		},
		transition: { type: String },
		still: { type: Boolean }
	},
	emits: [
		"mouseover",
		"mouseout",
		"click"
	],
	setup(n, { emit: f }) {
		let p = n, m = f;
		function h(e) {
			return {
				circle: {
					points: 1,
					rotation: 0
				},
				line: {
					points: 2,
					rotation: 0
				},
				triangle: {
					points: 3,
					rotation: .52
				},
				square: {
					points: 4,
					rotation: .783
				},
				diamond: {
					points: 4,
					rotation: 0
				},
				pentagon: {
					points: 5,
					rotation: .95
				},
				hexagon: {
					points: 6,
					rotation: 0
				}
			}[e];
		}
		let g = r(() => h(p.shape)), _ = r(() => p.shape === "star" ? t({
			plot: {
				x: p.plot.x,
				y: p.plot.y
			},
			radius: p.radius * (p.isSelected ? p.zoom : 1)
		}) : null), v = r(() => e({
			plot: {
				x: p.plot.x,
				y: p.plot.y
			},
			radius: p.radius * (p.isSelected ? p.zoom : 1),
			sides: g.value.points,
			rotation: g.value.rotation
		}).path);
		return (e, t) => (c(), a("g", null, [
			g.value && g.value.points === 1 ? (c(), a("circle", {
				key: 0,
				class: o(["vdui-shape-circle", { "vdui-shape-no-transition": n.still }]),
				cx: n.plot.x,
				cy: n.plot.y,
				r: p.radius * (p.isSelected ? p.zoom : 1),
				fill: n.color,
				stroke: n.stroke,
				"stroke-width": n.strokeWidth,
				onMouseover: t[0] ||= (e) => m("mouseover"),
				onMouseout: t[1] ||= (e) => m("mouseout"),
				onClick: t[2] ||= (e) => m("click"),
				style: s({ transition: n.transition })
			}, null, 46, l)) : i("", !0),
			g.value && g.value.points >= n.limit ? (c(), a("path", {
				key: 1,
				class: o(["vdui-shape-polygon", { "vdui-shape-no-transition": n.still }]),
				d: v.value,
				fill: n.color,
				stroke: n.stroke,
				"stroke-width": n.strokeWidth,
				onMouseover: t[3] ||= (e) => m("mouseover"),
				onMouseout: t[4] ||= (e) => m("mouseout"),
				onClick: t[5] ||= (e) => m("click"),
				style: s({ transition: n.transition })
			}, null, 46, u)) : i("", !0),
			_.value ? (c(), a("polygon", {
				key: 2,
				class: o(["vdui-shape-star", { "vdui-shape-no-transition": n.still }]),
				points: _.value,
				fill: n.color,
				stroke: n.stroke,
				"stroke-width": n.strokeWidth,
				onMouseover: t[6] ||= (e) => m("mouseover"),
				onMouseout: t[7] ||= (e) => m("mouseout"),
				onClick: t[8] ||= (e) => m("click"),
				style: s({ transition: n.transition })
			}, null, 46, d)) : i("", !0)
		]));
	}
}, [["__scopeId", "data-v-848f7c20"]]);
//#endregion
export { f as t };
