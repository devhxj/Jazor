import { Fragment as e, createElementBlock as t, mergeProps as n, openBlock as r, renderList as i } from "vue";
//#region src/atoms/DefGrad.vue
var a = ["id"], o = [
	"offset",
	"stop-color",
	"stop-opacity"
], s = ["id"], c = [
	"offset",
	"stop-color",
	"stop-opacity"
], l = /*@__PURE__*/ Object.assign({ inheritAttrs: !1 }, {
	__name: "DefGrad",
	props: {
		id: {
			type: String,
			required: !0
		},
		t: {
			type: String,
			required: !0,
			validator: (e) => ["linear", "radial"].includes(e)
		},
		stops: {
			type: Array,
			required: !0,
			validator: (e) => e.every((e) => Array.isArray(e) && e.length === 3 && [
				"number",
				"string",
				"number"
			].includes(typeof e[0]) && typeof e[1] == "string" && typeof e[2] == "number")
		}
	},
	setup(l) {
		return (u, d) => l.t === "linear" ? (r(), t("linearGradient", n({ key: 0 }, u.$attrs, { id: l.id }), [(r(!0), t(e, null, i(l.stops, ([e, n, i], a) => (r(), t("stop", {
			key: a,
			offset: e,
			"stop-color": n,
			"stop-opacity": i
		}, null, 8, o))), 128))], 16, a)) : (r(), t("radialGradient", n({ key: 1 }, u.$attrs, { id: l.id }), [(r(!0), t(e, null, i(l.stops, ([e, n, i], a) => (r(), t("stop", {
			key: a,
			offset: e,
			"stop-color": n,
			"stop-opacity": i
		}, null, 8, c))), 128))], 16, s));
	}
});
//#endregion
export { l as t };
