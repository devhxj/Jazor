import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
//#region src/themes/vue_ui_wheel.json
var t = /* @__PURE__ */ e({
	celebration: () => a,
	celebrationNight: () => o,
	concrete: () => l,
	dark: () => i,
	default: () => u,
	hack: () => s,
	minimal: () => n,
	minimalDark: () => r,
	zen: () => c
}), n = {
	userOptions: { show: !1 },
	style: { chart: { layout: { wheel: { ticks: { gradient: { show: !1 } } } } } }
}, r = {
	userOptions: { show: !1 },
	style: { chart: {
		backgroundColor: "#1A1A1A",
		color: "#CCCCCC",
		layout: {
			wheel: { ticks: {
				inactiveColor: "#4A4A4A",
				gradient: { show: !1 }
			} },
			innerCircle: { stroke: "#3A3A3A" }
		},
		title: {
			color: "#CCCCCC",
			subtitle: { color: "#757575" }
		}
	} }
}, i = { style: { chart: {
	backgroundColor: "#1A1A1A",
	color: "#CCCCCC",
	layout: {
		wheel: { ticks: { inactiveColor: "#4A4A4A" } },
		innerCircle: { stroke: "#3A3A3A" }
	},
	title: {
		color: "#CCCCCC",
		subtitle: { color: "#757575" }
	}
} } }, a = { style: { chart: {
	backgroundColor: "#FFF8E1",
	color: "#424242",
	layout: {
		wheel: { ticks: {
			inactiveColor: "#5D403760",
			activeColor: "#D32F2F",
			gradient: { shiftHueIntensity: 0 }
		} },
		innerCircle: { stroke: "#5D403760" }
	},
	title: {
		color: "#424242",
		subtitle: { color: "#757575" }
	}
} } }, o = { style: { chart: {
	backgroundColor: "#1E1E1E",
	color: "#BDBDBD",
	layout: {
		wheel: { ticks: {
			inactiveColor: "#5D403780",
			activeColor: "#D32F2F",
			gradient: { shiftHueIntensity: 0 }
		} },
		innerCircle: { stroke: "#5D403760" }
	},
	title: {
		color: "#FFF8E1",
		subtitle: { color: "#BDBDBD" }
	}
} } }, s = { style: { chart: {
	backgroundColor: "#1A1A1A",
	color: "#99AA99",
	layout: {
		wheel: { ticks: {
			rounded: !1,
			inactiveColor: "#333333",
			activeColor: "#66CC66",
			gradient: { shiftHueIntensity: 15 }
		} },
		innerCircle: { show: !1 }
	},
	title: {
		color: "#66CC66",
		subtitle: { color: "#99AA99" }
	}
} } }, c = { style: { chart: {
	backgroundColor: "#fbfafa",
	color: "#8A9892",
	layout: {
		wheel: { ticks: {
			inactiveColor: "#F7EDE2",
			activeColor: "#B1A7AD",
			gradient: { shiftHueIntensity: 100 }
		} },
		innerCircle: { show: !1 }
	},
	title: {
		color: "#8A9892",
		subtitle: { color: "#99AA99" }
	}
} } }, l = { style: { chart: {
	backgroundColor: "#f6f6fb",
	color: "#50606C",
	layout: {
		wheel: { ticks: {
			rounded: !1,
			inactiveColor: "#BBCBC7",
			activeColor: "#6C94A0",
			gradient: { shiftHueIntensity: 10 }
		} },
		innerCircle: { stroke: "#BBCBC7" }
	},
	title: {
		color: "#50606C",
		subtitle: { color: "#718890" }
	}
} } }, u = {
	default: {},
	minimal: n,
	minimalDark: r,
	dark: i,
	celebration: a,
	celebrationNight: o,
	hack: s,
	zen: c,
	concrete: l
};
//#endregion
export { t as n, u as t };
