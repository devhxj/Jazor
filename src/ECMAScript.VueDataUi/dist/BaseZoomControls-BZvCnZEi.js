import { t as e } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t } from "./BaseIcon-BfndwIWE.js";
import { r as n } from "./exposedLib-C0DPCgFj.js";
import { createCommentVNode as r, createElementBlock as i, createElementVNode as a, createVNode as o, normalizeClass as s, normalizeStyle as c, openBlock as l, toDisplayString as u, unref as d } from "vue";
//#region src/atoms/BaseZoomControls.vue
var f = ["disabled"], p = /*#__PURE__*/ e({
	__name: "BaseZoomControls",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		scale: {
			type: Number,
			default: 0
		},
		withDirection: {
			type: Boolean,
			default: !1
		},
		isFullscreen: {
			type: Boolean,
			default: !1
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		}
	},
	emits: [
		"zoomIn",
		"zoomOut",
		"resetZoom",
		"switchDirection"
	],
	setup(e, { emit: p }) {
		let m = p;
		return (p, h) => (l(), i("div", {
			class: s({
				"vue-data-ui-zoom-controls": !0,
				"vue-data-ui-zoom-controls-fullscreen": e.isFullscreen
			}),
			"data-dom-to-png-ignore": "",
			style: c({
				border: e.config.style.chart.controls.border,
				backgroundColor: e.config.style.chart.controls.backgroundColor,
				padding: e.config.style.chart.controls.padding,
				borderRadius: e.config.style.chart.controls.borderRadius,
				"--vue-data-ui-zoom-control-button-color": e.config.style.chart.controls.buttonColor,
				"--vue-data-ui-zoom-control-button-color-hover": d(n)(e.config.style.chart.controls.buttonColor, .2)
			})
		}, [
			a("button", {
				onClick: h[0] ||= (e) => m("zoomOut"),
				class: "vue-data-ui-zoom-controls-button",
				"data-cy-zoom-out": "",
				style: c({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [o(t, {
				name: "zoomMinus",
				stroke: e.config.style.chart.controls.color,
				size: e.config.style.chart.controls.fontSize * 1.2
			}, null, 8, ["stroke", "size"])], 4),
			a("button", {
				class: "vue-data-ui-zoom-controls-button-zoom",
				onClick: h[1] ||= (e) => m("resetZoom"),
				"data-cy-zoom-reset": "",
				style: c({
					color: e.config.style.chart.controls.color,
					width: e.config.style.chart.controls.fontSize * 4 + "px",
					borderRadius: e.config.style.chart.controls.borderRadius,
					fontSize: e.config.style.chart.controls.fontSize + "px",
					cursor: e.isCursorPointer ? "pointer" : "default"
				})
			}, u(Math.round(e.scale * 100)) + "% ", 5),
			a("button", {
				onClick: h[2] ||= (e) => m("zoomIn"),
				class: "vue-data-ui-zoom-controls-button",
				"data-cy-zoom-in": "",
				style: c({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [o(t, {
				name: "zoomPlus",
				stroke: e.config.style.chart.controls.color,
				size: e.config.style.chart.controls.fontSize * 1.2
			}, null, 8, ["stroke", "size"])], 4),
			a("button", {
				disabled: e.scale === 1,
				onClick: h[3] ||= (e) => m("resetZoom"),
				class: "vue-data-ui-zoom-controls-button",
				style: c({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [o(t, {
				name: "revert",
				stroke: e.config.style.chart.controls.color,
				size: e.config.style.chart.controls.fontSize * 1.2
			}, null, 8, ["stroke", "size"])], 12, f),
			e.withDirection ? (l(), i("button", {
				key: 0,
				onClick: h[4] ||= (e) => m("switchDirection"),
				class: "vue-data-ui-zoom-controls-button",
				style: c({ cursor: e.isCursorPointer ? "pointer" : "default" })
			}, [o(t, {
				name: "direction",
				stroke: e.config.style.chart.controls.color,
				size: e.config.style.chart.controls.fontSize * 1.2
			}, null, 8, ["stroke", "size"])], 4)) : r("", !0)
		], 6));
	}
}, [["__scopeId", "data-v-b5d92405"]]);
//#endregion
export { p as t };
