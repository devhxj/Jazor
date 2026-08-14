import { n as e } from "./patternUtils-CS7hn6at.js";
import { computed as t, createElementBlock as n, openBlock as r } from "vue";
//#region src/atoms/vue-ui-pattern-seed.vue
var i = [
	"id",
	"width",
	"height",
	"patternTransform",
	"innerHTML"
], a = {
	__name: "vue-ui-pattern-seed",
	props: {
		id: {
			type: String,
			required: !0
		},
		seed: {
			type: [String, Number],
			required: !0
		},
		foregroundColor: {
			type: String,
			default: "#1A1A1A"
		},
		backgroundColor: {
			type: String,
			default: "transparent"
		},
		maxSize: {
			type: Number,
			default: 24
		},
		minSize: {
			type: Number,
			default: 16
		},
		disambiguator: {
			type: [String, Number],
			default: ""
		}
	},
	setup(a) {
		let o = a, s = t(() => e(o.seed, {
			foregroundColor: o.foregroundColor,
			backgroundColor: o.backgroundColor,
			minimumSize: o.minSize,
			maximumSize: o.maxSize,
			disambiguator: o.disambiguator
		}));
		return (e, t) => (r(), n("pattern", {
			id: a.id,
			width: s.value.width,
			height: s.value.height,
			patternTransform: `rotate(${s.value.rotation})`,
			patternUnits: "userSpaceOnUse",
			innerHTML: s.value.contentMarkup
		}, null, 8, i));
	}
};
//#endregion
export { a as t };
