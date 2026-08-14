import { Bt as e, It as t } from "./lib-Bttd6u5E.js";
import { t as n } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { computed as r, createElementBlock as i, nextTick as a, normalizeStyle as o, onMounted as s, openBlock as c, ref as l, renderSlot as u, toRef as d, watch as f } from "vue";
var p = /*#__PURE__*/ n({
	__name: "SparkTooltip",
	props: {
		svgRef: { type: Object },
		x: {
			type: Number,
			required: !0
		},
		y: {
			type: Number,
			required: !0
		},
		prevX: {
			type: Number,
			required: !0
		},
		prevY: {
			type: Number,
			required: !0
		},
		offsetY: {
			type: Number,
			default: 0
		},
		background: { type: String },
		backgroundOpacity: {
			type: Number,
			default: 100
		},
		borderRadius: {
			type: Number,
			default: 2
		},
		borderWidth: {
			type: Number,
			default: 0
		},
		borderColor: {
			type: String,
			default: "#FFFFFF"
		},
		color: { type: String },
		fontSize: { type: Number }
	},
	setup(n) {
		let p = n, m = d(p.svgRef), h = l(null), g = l(0), _ = l(0), v = l(!1), y = r(() => e(p.background, p.backgroundOpacity)), b = async () => {
			if (!m.value || !h.value) return;
			let e = t({
				svgElement: m.value,
				element: h.value,
				x: p.x,
				y: p.y,
				offsetY: p.offsetY
			}), n = t({
				svgElement: m.value,
				element: h.value,
				x: p.prevX,
				y: p.prevY,
				offsetY: p.offsetY
			});
			!e || !n || (v.value = !1, g.value = n.top, _.value = n.left, await a(), setTimeout(() => {
				v.value = !0, g.value = e.top, _.value = e.left;
			}, 50));
		};
		return s(b), f(() => [
			p.x,
			p.y,
			p.prevX,
			p.prevY
		], () => b(), { immediate: !0 }), (e, t) => (c(), i("div", {
			ref_key: "tooltipRef",
			ref: h,
			class: "vue-data-ui-spark-tooltip",
			style: o({
				position: "fixed",
				top: `${g.value}px`,
				left: `${_.value}px`,
				pointerEvents: "none",
				background: y.value,
				color: p.color,
				fontSize: `${p.fontSize}px`,
				borderRadius: `${p.borderRadius}px`,
				border: `${p.borderWidth}px solid ${p.borderColor}`,
				transition: v.value ? "top 0.3s ease-out, left 0.3s ease-out" : "none"
			})
		}, [u(e.$slots, "default", {}, void 0, !0)], 4));
	}
}, [["__scopeId", "data-v-57d870b0"]]);
//#endregion
export { p as default };
