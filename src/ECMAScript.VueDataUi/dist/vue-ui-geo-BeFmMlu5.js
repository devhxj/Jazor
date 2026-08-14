import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Gt as t, Jt as n, S as r, q as i, t as a, xt as o } from "./lib-Bttd6u5E.js";
import { n as s } from "./useHints-Dq_w2E8B.js";
import { t as c } from "./useConfig-DlNpz6P8.js";
import { t as l } from "./usePrinter-DN5bYhTG.js";
import { n as u, t as d } from "./BaseScanner-DZvpgOjM.js";
import { t as f } from "./useNestedProp-vPNvh7rV.js";
import { t as p } from "./useThemeCheck-C43Tcqmk.js";
import { t as ee } from "./useChartExport-DNiwdPmb.js";
import { t as te } from "./img-Bnokohej.js";
import { n as ne } from "./Title-BE3qg9xl.js";
import { t as re } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ie, t as ae } from "./useResponsive-ZtArZtUf.js";
import { t as oe } from "./A11yDataTable-DdRsVULz.js";
import { t as se } from "./useUserOptionState-DK-_1ddE.js";
import { t as ce } from "./useChartAccessibility-DYqac8yF.js";
import { t as le } from "./usePanZoom-CYU3B4T3.js";
import { t as ue } from "./BaseZoomControls-BZvCnZEi.js";
import { t as de } from "./geoProjections-CEo3dVaL.js";
import { t as fe } from "./vue_ui_geo-B8TODs-G.js";
import { Fragment as pe, computed as m, createBlock as h, createCommentVNode as g, createElementBlock as _, createElementVNode as v, createSlots as me, createVNode as he, defineAsyncComponent as ge, guardReactiveProps as y, mergeProps as _e, nextTick as ve, normalizeClass as ye, normalizeProps as b, normalizeStyle as be, onBeforeUnmount as xe, onMounted as Se, openBlock as x, ref as S, renderList as Ce, renderSlot as C, toDisplayString as we, toRefs as Te, unref as w, watch as T, withCtx as E } from "vue";
//#region src/components/vue-ui-geo.vue
var Ee = /* @__PURE__ */ e({ default: () => Be }), De = ["id"], Oe = ["id"], ke = { style: { position: "relative" } }, Ae = [
	"xmlns",
	"viewBox",
	"aria-describedby",
	"id"
], je = ["transform"], Me = [
	"d",
	"fill",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Ne = [
	"d",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Pe = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width",
	"onMouseenter"
], Fe = [
	"cx",
	"cy",
	"r",
	"fill",
	"stroke",
	"stroke-width",
	"onMouseenter",
	"onMouseleave",
	"onClick"
], Ie = [
	"x",
	"y",
	"fill",
	"font-size"
], Le = {
	key: 0,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, Re = {
	key: 6,
	class: "vue-data-ui-watermark"
}, ze = 1.5, Be = /*#__PURE__*/ re({
	__name: "vue-ui-geo",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: [Array, Object],
			default() {
				return [];
			}
		}
	},
	emits: ["copyAlt"],
	setup(e, { expose: re, emit: Ee }) {
		let Be = ge(() => import("./Tooltip-DhjyfHwz.js")), Ve = ge(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), He = ge(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), Ue = ge(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), D = e, We = Ee, { vue_ui_geo: Ge } = c(), { isThemeValid: Ke, warnInvalidTheme: qe } = p(), O = S(i()), Je = S(0), Ye = S(0), k = S(null), A = S(!1), j = S(null), Xe = S(null), Ze = S(null), Qe = S(null), M = S(null), N = S(null), $e = S(null), et = S(null), P = S(null), tt = S({
			x: 0,
			y: 0
		}), F = S("pointer"), nt = S(!1), I = S(!1);
		function rt(e) {
			I.value = e, Je.value += 1;
		}
		function it() {
			let e = f({
				userConfig: D.config,
				defaultConfig: Ge
			}), t = e.theme;
			if (!t) return e;
			if (!Ke.value(e)) return qe(e), e;
			let n = f({
				userConfig: fe[t] || D.config,
				defaultConfig: e
			});
			return f({
				userConfig: D.config,
				defaultConfig: n
			});
		}
		let L = S(it());
		s({
			config: () => L.value,
			dataset: () => D.dataset,
			component: "VueUiGeo",
			rules: [{
				test: (e) => e.length > 100,
				message: [
					"👀 The number of data points > 100, which can make the chart hard to read. Consider:",
					"",
					"▶️ Using filters to reduce the number of data points displayed at the same time, and offer users some level of control."
				]
			}]
		});
		let at = m(() => L.value.userOptions.useCursorPointer), { userOptionsVisible: ot, keepUserOptionState: st } = se({ config: L.value }), { svgRef: R } = ce({ config: L.value.style.chart.title }), ct = m(() => n({
			defaultConfig: {
				map: { geoJson: {
					type: "FeatureCollection",
					features: [
						{
							type: "Feature",
							properties: { name: "Island A" },
							geometry: {
								type: "Polygon",
								coordinates: [[
									[-6, 2],
									[-4, 2],
									[-4, 4],
									[-6, 4],
									[-6, 2]
								]]
							}
						},
						{
							type: "Feature",
							properties: { name: "Island B" },
							geometry: {
								type: "Polygon",
								coordinates: [[
									[-2, -1],
									[1, -1],
									[1, 2],
									[-2, 2],
									[-2, -1]
								]]
							}
						},
						{
							type: "Feature",
							properties: { name: "Island C" },
							geometry: {
								type: "Polygon",
								coordinates: [[
									[3, -3],
									[5, -3],
									[5, -1],
									[3, -1],
									[3, -3]
								]]
							}
						},
						{
							type: "Feature",
							properties: { name: "Island D" },
							geometry: {
								type: "Polygon",
								coordinates: [[
									[4, 3],
									[7, 3],
									[7, 5],
									[4, 5],
									[4, 3]
								]]
							}
						}
					]
				} },
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					territory: {
						fill: "#99999950",
						stroke: "#8A8A8A",
						strokeWidth: .5
					}
				} }
			},
			userConfig: L.value.skeletonConfig ?? {}
		})), { loading: lt, FINAL_DATASET: ut } = u({
			...Te(D),
			FINAL_CONFIG: L,
			prepareConfig: it,
			allowEmptyDataset: !0,
			skeletonDataset: D.config?.skeletonDataset ?? [],
			skeletonConfig: n({
				defaultConfig: L.value,
				userConfig: ct.value
			})
		}), { isPrinting: dt, isImaging: ft, generatePdf: pt, generateImage: mt } = l({
			elementId: `vue-ui-geo_${O.value}`,
			fileName: L.value.style.chart.title.text || "vue-ui-geo",
			options: L.value.userOptions.print
		}), ht = m(() => L.value.style.chart.controls.show ? L.value.style.chart.controls.position === "top" ? $e.value?.$el ?? null : L.value.style.chart.controls.position === "bottom" ? et.value?.$el ?? null : null : null);
		function gt() {
			M.value && (N.value && M.value.unobserve(N.value), M.value.disconnect(), M.value = null, N.value = null);
		}
		let z = m(() => !!L.value.responsive && !I.value);
		T(() => I.value, async (e) => {
			if (e) {
				gt(), V.value = Number(L.value.style.chart.dimensions.width) || V.value, H.value = Number(L.value.style.chart.dimensions.height) || H.value, await ve(), A.value = !1, requestAnimationFrame(() => {
					A.value || en();
				});
				return;
			}
			L.value.responsive && (await ve(), _t());
		});
		function _t() {
			if (gt(), !z.value) return;
			let e = ie(() => {
				if (!k.value) return;
				let { width: e, height: t } = ae({
					chart: k.value,
					noTitle: vt.value ? Ze.value : null,
					title: L.value.style.chart.title.text ? Xe.value : null,
					legend: ht.value,
					source: Qe.value
				});
				requestAnimationFrame(() => {
					V.value = Math.max(.1, Number(e) || .1), H.value = Math.max(.1, Number(t) || .1) - 24;
				});
			});
			M.value = new ResizeObserver(e), N.value = k.value ? k.value.parentNode : null, N.value && M.value.observe(N.value), e();
		}
		let vt = m(() => L.value.userOptions.show && !L.value.style.chart.title.text), yt = S({ showTooltip: L.value.style.chart.tooltip.show }), B = S(L.value.style.chart.zoom.active);
		T(() => D.config, () => {
			L.value = it(), yt.value.showTooltip = L.value.style.chart.tooltip.show, B.value = L.value.style.chart.zoom.active, V.value = L.value.style.chart.dimensions.width, H.value = L.value.style.chart.dimensions.height, _t(), Ye.value += 1, Je.value += 1;
		}, { deep: !0 });
		let { projections: bt } = de, xt = m(() => L.value?.projection || "equirectangular"), St = m(() => {
			let e = bt?.[xt.value];
			return typeof e == "function" ? e : bt.equirectangular;
		}), Ct = m(() => L.value.map.geoJson ?? null), wt = {
			aitoff: {
				width: 1e3,
				height: 500
			},
			azimuthalEquidistant: {
				width: 1e3,
				height: 1e3
			},
			bonne: {
				width: 1e3,
				height: 1e3
			},
			equirectangular: {
				width: 1e3,
				height: 700
			},
			gallPeters: {
				width: 1e3,
				height: 800
			},
			globe: {
				width: 1e3,
				height: 1e3
			},
			hammer: {
				width: 1e3,
				height: 500
			},
			mercator: {
				width: 1e3,
				height: 750
			},
			mollweide: {
				width: 1e3,
				height: 600
			},
			robinson: {
				width: 1e3,
				height: 600
			},
			sinusoidal: {
				width: 1e3,
				height: 500
			},
			vanDerGrinten: {
				width: 1e3,
				height: 1e3
			},
			winkelTripel: {
				width: 1e3,
				height: 1e3
			}
		}, V = S(L.value.style.chart.dimensions.width), H = S(L.value.style.chart.dimensions.height), Tt = m(() => {
			let e = wt[xt.value] || wt.equirectangular;
			return {
				width: e.width,
				height: e.height
			};
		}), U = m(() => {
			let e = Tt.value, t = V.value, n = H.value, r = Number.isFinite(t) && t > 0, i = Number.isFinite(n) && n > 0;
			return r && i ? {
				width: t,
				height: n
			} : r && !i ? {
				width: t,
				height: t * (e.height / e.width)
			} : !r && i ? {
				width: n * (e.width / e.height),
				height: n
			} : {
				width: e.width,
				height: e.height
			};
		}), Et = m(() => {
			let e = L.value?.map?.center, t = [0, 0];
			if (!Array.isArray(e) || e.length !== 2) return t;
			let n = Number(e[0]), r = Number(e[1]);
			return !Number.isFinite(n) || !Number.isFinite(r) ? t : [n, r];
		});
		function W([e, t]) {
			return St.value([e, t], Tt.value.width, Tt.value.height, Et.value);
		}
		function Dt(e, t = {}) {
			let { defaultName: n = "", namePropertyCandidates: r = [
				"name",
				"nom",
				"admin",
				"NAME",
				"label",
				"title",
				"description",
				"DESCRIPTION",
				"NAME_1",
				"NAME_2",
				"NAME_3",
				"NAME_EN",
				"name:en",
				"name_en",
				"localname",
				"local_name"
			], includeNullGeometries: i = !1 } = t, a = {
				type: "FeatureCollection",
				features: []
			};
			function o(e) {
				return typeof e == "object" && !!e && !Array.isArray(e);
			}
			function s(e) {
				return !o(e) || typeof e.type != "string" ? !1 : e.type === "GeometryCollection" ? Array.isArray(e.geometries) : Object.hasOwn(e, "coordinates");
			}
			function c(e) {
				if (!o(e)) return "";
				for (let t of r) {
					let n = e[t];
					if (typeof n == "string" && n.trim()) return n.trim();
				}
				return "";
			}
			function l({ geometry: e, properties: t, fallbackName: r, featureIndex: i }) {
				let a = o(t) ? { ...t } : {}, s = c(a), l = n ? `${n} ${i + 1}` : "", u = s || (typeof r == "string" ? r.trim() : "") || l;
				return !a.name && u && (a.name = u), {
					type: "Feature",
					geometry: e,
					properties: a
				};
			}
			function u(e, t, n, r, a) {
				let c = Array.isArray(e?.geometries) ? e.geometries : [], d = r;
				for (let e = 0; e < c.length; e += 1) {
					let r = c[e];
					if (r) {
						if (o(r) && r.type === "GeometryCollection") {
							d = u(r, t, n, d, a);
							continue;
						}
						if (s(r)) {
							if (r.coordinates == null && r.type !== "GeometryCollection") {
								i && (a.push(l({
									geometry: null,
									properties: t,
									fallbackName: n,
									featureIndex: d
								})), d += 1);
								continue;
							}
							a.push(l({
								geometry: r,
								properties: t,
								fallbackName: n,
								featureIndex: d
							})), d += 1;
						}
					}
				}
				return d;
			}
			function d(e, t, n = 0) {
				let r = n;
				if (e == null) return r;
				if (Array.isArray(e)) {
					for (let n of e) r = d(n, t, r);
					return r;
				}
				if (!o(e) || typeof e.type != "string") return r;
				if (e.type === "FeatureCollection") {
					let n = Array.isArray(e.features) ? e.features : [];
					for (let e of n) r = d(e, t, r);
					return r;
				}
				if (e.type === "Feature") {
					let n = o(e.properties) ? e.properties : {}, a = c(n), d = e.geometry ?? null;
					return d == null ? (i && (t.push(l({
						geometry: null,
						properties: n,
						fallbackName: a,
						featureIndex: r
					})), r += 1), r) : o(d) && d.type === "GeometryCollection" ? u(d, n, a, r, t) : (s(d) && (t.push(l({
						geometry: d,
						properties: n,
						fallbackName: a,
						featureIndex: r
					})), r += 1), r);
				}
				return e.type === "GeometryCollection" ? u(e, {}, "", r, t) : (s(e) && (t.push(l({
					geometry: e,
					properties: {},
					fallbackName: "",
					featureIndex: r
				})), r += 1), r);
			}
			let f = [];
			return d(e, f, 0), {
				...a,
				features: f
			};
		}
		function Ot(e) {
			let t = Array.isArray(e?.features) ? e.features : [];
			function n(e) {
				if (!e || typeof e != "object") return "";
				let t = [
					e.name,
					e.nom,
					e.admin,
					e.NAME,
					e.label,
					e.title,
					e.description,
					e.DESCRIPTION,
					e.NAME_1,
					e.NAME_2,
					e.NAME_3,
					e.NAME_EN,
					e["name:en"],
					e.name_en,
					e.localname,
					e.local_name
				];
				for (let e of t) if (typeof e == "string" && e.trim()) return e.trim();
				for (let t of Object.values(e)) if (typeof t == "string" && t.trim()) return t.trim();
				return "";
			}
			return t.filter((e) => e && e.type === "Feature" && e.geometry).map((e, t) => {
				let r = e.properties || {}, i = n(r);
				return {
					feature: e,
					geometry: e.geometry,
					properties: r,
					name: i,
					uid: `map-feature-${O.value}-${t}`,
					index: t
				};
			});
		}
		let kt = m(() => Dt(Ct.value, {
			defaultName: "",
			includeNullGeometries: !1
		})), At = m(() => Ot(kt.value));
		function jt(e) {
			if (!e || typeof e.type != "string") return "";
			if (e.type === "GeometryCollection") return (Array.isArray(e.geometries) ? e.geometries : []).map((e) => jt(e)).filter(Boolean).join(" ");
			let t = (e) => {
				if (!Array.isArray(e) || e.length < 2) return null;
				let t = Number(e[0]), n = Number(e[1]);
				if (!Number.isFinite(t) || !Number.isFinite(n)) return null;
				let r = W([t, n]);
				if (!Array.isArray(r) || r.length < 2) return null;
				let i = Number(r[0]), a = Number(r[1]);
				return !Number.isFinite(i) || !Number.isFinite(a) ? null : [i, a];
			}, n = (e) => {
				let n = (e || []).map(t).filter(Boolean);
				return n.length < 2 ? "" : "M" + n.map(([e, t]) => `${e},${t}`).join("L");
			}, r = (e) => (e || []).map((e) => {
				let t = n(e);
				return t ? t + "Z" : "";
			}).filter(Boolean).join(" ");
			return e.type === "Polygon" ? r(e.coordinates) : e.type === "MultiPolygon" ? (e.coordinates || []).map(r).filter(Boolean).join(" ") : e.type === "LineString" ? n(e.coordinates) : e.type === "MultiLineString" ? (e.coordinates || []).map(n).filter(Boolean).join(" ") : "";
		}
		function Mt(e) {
			return e ? e.type === "GeometryCollection" ? (Array.isArray(e.geometries) ? e.geometries : []).flatMap((e) => Mt(e)) : e.type === "Point" ? Array.isArray(e.coordinates) ? [e.coordinates] : [] : e.type === "MultiPoint" && Array.isArray(e.coordinates) ? e.coordinates : [] : [];
		}
		function Nt(e) {
			return !e || typeof e.type != "string" ? !1 : e.type === "GeometryCollection" ? (Array.isArray(e.geometries) ? e.geometries : []).some((e) => Nt(e)) : e.type === "Point" || e.type === "MultiPoint";
		}
		let Pt = m(() => At.value.filter((e) => {
			let t = e.geometry;
			return t ? t.type === "GeometryCollection" ? (Array.isArray(t.geometries) ? t.geometries : []).some((e) => e?.type === "Polygon" || e?.type === "MultiPolygon") : t.type === "Polygon" || t.type === "MultiPolygon" : !1;
		})), Ft = m(() => At.value.filter((e) => {
			let t = e.geometry;
			return t ? t.type === "GeometryCollection" ? (Array.isArray(t.geometries) ? t.geometries : []).some((e) => e?.type === "LineString" || e?.type === "MultiLineString") : t.type === "LineString" || t.type === "MultiLineString" : !1;
		})), It = m(() => At.value.filter((e) => {
			let t = e.geometry;
			return t ? Nt(t) : !1;
		})), Lt = m(() => Pt.value.map((e) => {
			let t = jt(e.geometry);
			return t ? {
				...e,
				path: t
			} : null;
		}).filter(Boolean)), Rt = m(() => Ft.value.map((e) => {
			let t = jt(e.geometry);
			return t ? {
				...e,
				path: t
			} : null;
		}).filter(Boolean)), zt = m(() => {
			let e = [];
			function t(e = {}) {
				let t = e?.style && typeof e.style == "object" ? e.style : {}, n = e.radius ?? e.r ?? t.radius ?? t.r, i = e.color ?? e.fill ?? t.color ?? t.fill, a = e.stroke ?? t.stroke, o = e.strokeWidth ?? e.stroke_width ?? t.strokeWidth ?? t.stroke_width;
				return {
					radius: Number.isFinite(Number(n)) ? Number(n) : Number(L.value.style.chart.points.radius),
					fill: i != null && String(i).trim() ? r(String(i).trim()) : L.value.style.chart.points.fill,
					stroke: a != null && String(a).trim() ? r(String(a).trim()) : L.value.style.chart.points.stroke,
					strokeWidth: Number.isFinite(Number(o)) ? Number(o) : Number(L.value.style.chart.points.strokeWidth)
				};
			}
			for (let n of It.value) {
				let r = Mt(n.geometry);
				for (let i = 0; i < r.length; i += 1) {
					let a = r[i];
					if (!Array.isArray(a) || a.length < 2) continue;
					let o = Number(a[0]), s = Number(a[1]);
					if (!Number.isFinite(o) || !Number.isFinite(s)) continue;
					let c = W([o, s]), l = c?.[0], u = c?.[1];
					if (!Number.isFinite(l) || !Number.isFinite(u)) continue;
					let d = t(n?.feature?.properties && typeof n.feature.properties == "object" ? n.feature.properties : {});
					e.push({
						uid: `${n.uid}-geojson-point-${i}`,
						name: n.name || "",
						x: l,
						y: u,
						...d,
						originalFeature: n.feature,
						featureIndex: n.index,
						pointIndex: i
					});
				}
			}
			return e;
		});
		function Bt(e) {
			if (!e || typeof e.type != "string") return [];
			let t = [], n = (e) => {
				if (!Array.isArray(e) || e.length < 2) return;
				let n = Number(e[0]), r = Number(e[1]);
				if (!Number.isFinite(n) || !Number.isFinite(r)) return;
				let i = W([n, r]), a = i?.[0], o = i?.[1];
				!Number.isFinite(a) || !Number.isFinite(o) || t.push([a, o]);
			}, r = (e) => {
				if (Array.isArray(e)) {
					if (e.length >= 2 && typeof e[0] == "number" && typeof e[1] == "number") {
						n(e);
						return;
					}
					for (let t of e) r(t);
				}
			};
			if (e.type === "GeometryCollection" && Array.isArray(e.geometries)) {
				for (let n of e.geometries) t.push(...Bt(n));
				return t;
			}
			return e.type === "Point" ? (n(e.coordinates), t) : e.type === "MultiPoint" ? ((e.coordinates || []).forEach(n), t) : (r(e.coordinates), t);
		}
		function Vt(e) {
			let t = [];
			for (let n of e) t.push(...Bt(n.geometry));
			if (!t.length) return null;
			let n = Infinity, r = Infinity, i = -Infinity, a = -Infinity;
			for (let [e, o] of t) e < n && (n = e), o < r && (r = o), e > i && (i = e), o > a && (a = o);
			let o = i - n, s = a - r;
			return !(o > 0) || !(s > 0) ? null : {
				minX: n,
				minY: r,
				maxX: i,
				maxY: a,
				width: o,
				height: s
			};
		}
		let Ht = m(() => Vt(At.value));
		function Ut({ bounds: e, targetWidth: t, targetHeight: n, padding: r }) {
			if (!e || !(e.width > 0) || !(e.height > 0)) return {
				scale: 1,
				translateX: 0,
				translateY: 0,
				transform: ""
			};
			let i = Number.isFinite(Number(r)) ? Number(r) : 0, a = Math.max(1, t - i * 2), o = Math.max(1, n - i * 2), s = a / e.width, c = o / e.height, l = Math.min(s, c), u = e.width * l, d = e.height * l, f = i + (a - u) / 2, p = i + (o - d) / 2, ee = f - e.minX * l, te = p - e.minY * l;
			return {
				scale: l,
				translateX: ee,
				translateY: te,
				transform: `translate(${ee} ${te}) scale(${l})`
			};
		}
		let Wt = m(() => {
			let e = Ht.value;
			if (!e) return {
				scale: 1,
				translateX: 0,
				translateY: 0,
				transform: ""
			};
			let t = L.value?.map?.fitPadding, n = Number.isFinite(Number(t)) ? Number(t) : 0;
			return Ut({
				bounds: e,
				targetWidth: U.value.width,
				targetHeight: U.value.height,
				padding: n
			});
		}), Gt = m(() => `0 0 ${U.value.width} ${U.value.height}`), { viewBox: Kt, resetZoom: qt, setInitialViewBox: Jt, scale: Yt, zoomByFactor: Xt } = le(R, {
			x: 0,
			y: 0,
			width: U.value.width,
			height: U.value.height
		}, 1, B);
		function Zt() {
			B.value = !B.value;
		}
		function Qt(e) {
			let t = Wt.value, n = Number(t?.scale) || 1, r = Number(t?.translateX) || 0, i = Number(t?.translateY) || 0;
			return {
				x: e.x * n + r,
				y: e.y * n + i
			};
		}
		function $t() {
			let e = L.value?.map?.center, t = Number(e?.[0]), n = Number(e?.[1]), r = Array.isArray(e) && e.length === 2 && Number.isFinite(t) && Number.isFinite(n) && (t !== 0 || n !== 0), i = U.value.width, a = U.value.height;
			if (!r) return {
				x: 0,
				y: 0,
				width: i,
				height: a
			};
			let o = W([t, n]), s = o?.[0], c = o?.[1];
			if (!Number.isFinite(s) || !Number.isFinite(c)) return {
				x: 0,
				y: 0,
				width: i,
				height: a
			};
			let l = Qt({
				x: s,
				y: c
			});
			return {
				x: l.x - i / 2,
				y: l.y - a / 2,
				width: i,
				height: a
			};
		}
		function en() {
			let e = $t();
			Jt(e, { overwriteCurrentIfNotZoomed: !0 }), A.value = !0;
		}
		let tn = m(() => {
			let e = Kt.value;
			return e ? `${e.x} ${e.y} ${e.width} ${e.height}` : Gt.value;
		});
		function nn() {
			Xt(ze, !0);
		}
		function rn() {
			Xt(1 / ze, !0);
		}
		async function an() {
			let e = $t();
			Jt(e, { overwriteCurrentIfNotZoomed: !0 }), qt(!0);
		}
		T(() => [
			L.value.projection,
			L.value.map.center?.[0],
			L.value.map.center?.[1],
			L.value.map.fitPadding,
			Ct.value
		], async () => {
			A.value = !1, await ve(), requestAnimationFrame(() => {
				A.value || en();
			});
		}, { deep: !1 }), T(() => [
			V.value,
			H.value,
			z.value
		], async () => {
			z.value && (await ve(), Jt({
				x: 0,
				y: 0,
				width: U.value.width,
				height: U.value.height
			}, { overwriteCurrentIfNotZoomed: !0 }), A.value = !1, requestAnimationFrame(() => {
				A.value || en();
			}));
		}, { flush: "post" });
		function on(e) {
			return e ? e.type === "FeatureCollection" && Array.isArray(e.features) ? e.features.filter((e) => e?.type === "Feature" && e.geometry?.type === "Point").map((e, t) => {
				let n = e.properties || {}, r = n.name || n.label || n.title || `Point ${t + 1}`, i = e.geometry.coordinates;
				return {
					uid: `map-point-${O.value}-${t}`,
					name: r,
					coordinates: i,
					color: n.color ?? null,
					radius: n.radius ?? null,
					hoverRadiusRatio: L.value.style.chart.points.hoverRadiusRatio,
					description: n.description ?? null,
					original: e,
					index: t
				};
			}) : Array.isArray(e) ? e.map((e, t) => {
				let n = null, r = `Point ${t + 1}`, i = null, a = null, o = null;
				Array.isArray(e) && e.length >= 2 ? n = [Number(e[0]), Number(e[1])] : e && typeof e == "object" && (Array.isArray(e.coordinates) && e.coordinates.length >= 2 ? n = [Number(e.coordinates[0]), Number(e.coordinates[1])] : Number.isFinite(Number(e.lon)) && Number.isFinite(Number(e.lat)) && (n = [Number(e.lon), Number(e.lat)]), typeof e.name == "string" && e.name.trim() && (r = e.name), e.description != null && (i = e.description), e.color != null && (a = e.color), e.radius != null && (o = e.radius));
				let s = n ? n[0] : NaN, c = n ? n[1] : NaN;
				return !Number.isFinite(s) || !Number.isFinite(c) ? null : {
					uid: `map-point-${O.value}-${t}`,
					name: r,
					coordinates: [s, c],
					color: a,
					radius: o,
					hoverRadiusRatio: L.value.style.chart.points.hoverRadiusRatio,
					description: i,
					original: e,
					index: t
				};
			}).filter(Boolean) : typeof e == "object" ? Object.entries(e).map(([e, t], n) => {
				if (!t || typeof t != "object") return null;
				let r = Array.isArray(t.coordinates) ? t.coordinates : null;
				if (!r || r.length < 2) return null;
				let i = Number(r[0]), a = Number(r[1]);
				return !Number.isFinite(i) || !Number.isFinite(a) ? null : {
					uid: `map-point-${O.value}-${n}-${e}`,
					name: t.name || e,
					coordinates: [i, a],
					color: t.color ?? null,
					radius: t.radius ?? null,
					hoverRadiusRatio: L.value.style.chart.points.hoverRadiusRatio,
					description: t.description ?? null,
					original: t,
					index: n
				};
			}).filter(Boolean) : [] : [];
		}
		let sn = m(() => on(ut.value)), cn = m(() => sn.value.map((e) => {
			let t = W(e.coordinates), n = t?.[0], i = t?.[1];
			return !Number.isFinite(n) || !Number.isFinite(i) ? null : {
				...e,
				x: n,
				y: i,
				fill: e.color ? r(e.color) : L.value.style.chart.points.fill,
				radius: Number.isFinite(Number(e.radius)) ? Number(e.radius) : L.value.style.chart.points.radius
			};
		}).filter(Boolean)), G = m(() => cn.value.map((e, t) => ({
			...e,
			keyboardIndex: t
		})));
		function ln(e) {
			let n = Qt(e), r = t(n.x, n.y, R.value);
			r && (tt.value = {
				x: r.x,
				y: r.y
			});
		}
		let un = S(!1), dn = S(""), K = S(!1), q = S(null), J = S(null);
		function fn(e) {
			dn.value = e, un.value = !0;
		}
		function Y() {
			un.value = !1, dn.value = "";
		}
		function pn(e) {
			return String(e ?? "").replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll("\"", "&quot;").replaceAll("'", "&#039;");
		}
		function mn(e) {
			j.value = {
				datapoint: e,
				seriesIndex: e.index,
				series: sn.value,
				config: L.value
			}, K.value = !1;
			let t = L.value.style.chart.tooltip.customFormat;
			if (o(t)) try {
				let e = t(j.value);
				if (typeof e == "string") return K.value = !0, e;
			} catch {
				console.warn("Custom format cannot be applied."), K.value = !1;
			}
			return `<div><div style="font-weight:600">${pn(e.name)}</div>${e.description == null ? "" : `<div style="margin-top:6px">${pn(e.description)}</div>`}</div>`;
		}
		function hn(e) {
			K.value = !1;
			let t = L.value.style.chart.tooltip.customFormat;
			if (o(t)) try {
				let n = t({
					datapoint: e,
					config: L.value
				});
				if (typeof n == "string") return K.value = !0, n;
			} catch {
				console.warn("Custom format cannot be applied."), K.value = !1;
			}
			let n = typeof e?.name == "string" ? e.name.trim() : "";
			return n ? `<div><div style="font-weight:600">${pn(n)}</div></div>` : (K.value = !1, "");
		}
		function gn(e) {
			if (L.value.events.territoryEnter && L.value.events.territoryEnter({
				datapoint: e,
				seriesIndex: e.index
			}), !(typeof e?.name == "string" && e.name.trim()) && !L.value.style.chart.territory.hover.enabledWhenEmpty) {
				J.value = null, Y();
				return;
			}
			J.value = e.uid, fn(hn(e));
		}
		function _n(e) {
			J.value = null, Y(), L.value.events.territoryLeave && L.value.events.territoryLeave({
				datapoint: e,
				seriesIndex: e.index
			});
		}
		function vn(e) {
			L.value.events.territoryClick && L.value.events.territoryClick({
				datapoint: e,
				seriesIndex: e.index
			});
		}
		function yn(e, t = null, n = "pointer") {
			F.value = n, q.value = e.uid, Number.isInteger(t) && (P.value = t), n === "keyboard" && ln(e), fn(mn(e)), L.value.events.datapointEnter && L.value.events.datapointEnter({
				datapoint: e,
				seriesIndex: e.index
			});
		}
		function bn(e, t = "pointer") {
			(t !== "pointer" || F.value !== "keyboard") && (q.value = null, Y(), t !== "pointer" && (P.value = null, F.value = "pointer"), L.value.events.datapointLeave && L.value.events.datapointLeave({
				datapoint: e,
				seriesIndex: e.index
			}));
		}
		let xn = m(() => !!L.value.events.datapointClick && typeof L.value.events.datapointClick == "function");
		function Sn(e) {
			xn.value && L.value.events.datapointClick({
				datapoint: e,
				seriesIndex: e.index
			});
		}
		let X = m(() => L.value.style.chart.territory), Cn = m(() => L.value.style.chart.points);
		function wn(e) {
			let t = typeof e?.name == "string" ? e.name.trim() : "";
			if (!t) {
				J.value = null, Y();
				return;
			}
			let n = {
				name: t,
				uid: e.uid,
				index: e.featureIndex,
				properties: {}
			};
			J.value = e.uid, fn(hn(n));
		}
		function Tn() {
			J.value = null, Y();
		}
		let Z = S(!1);
		function En() {
			Z.value = !Z.value;
		}
		T(() => Z.value, (e) => {
			B.value = !e;
		});
		function Dn() {
			yt.value.showTooltip = !yt.value.showTooltip;
		}
		let On = m(() => L.value.style.chart.backgroundColor), kn = m(() => L.value.style.chart.title), { isCallbackImaging: An, isCallbackSvg: jn, generateSvg: Mn, onGenerateImage: Nn } = ee({
			svg: R,
			title: kn,
			legend: null,
			legendItems: null,
			backgroundColor: On,
			getSvgCallback: () => L.value.userOptions.callbacks.svg,
			generateImage: mt
		});
		async function Pn({ scale: e = 2 } = {}) {
			if (!k.value) return;
			let { width: t, height: n } = k.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await te({
				domElement: k.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: L.value.style.chart.title.text,
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let Q = S(!1), $ = null;
		function Fn() {
			B.value && (Q.value = !0);
		}
		function In() {
			Q.value = !1;
		}
		function Ln() {
			B.value && (Q.value = !0, $ && clearTimeout($), $ = setTimeout(() => {
				Q.value = !1, $ = null;
			}, 140));
		}
		xe(() => {
			$ && clearTimeout($);
		});
		async function Rn([e, t], { animated: n = !0 } = {}) {
			let r = Number(e), i = Number(t);
			if (!Number.isFinite(r) || !Number.isFinite(i)) return;
			let a = W([r, i]), o = a?.[0], s = a?.[1];
			if (!Number.isFinite(o) || !Number.isFinite(s)) return;
			let c = Qt({
				x: o,
				y: s
			}), l = Kt.value ? { ...Kt.value } : {
				x: 0,
				y: 0,
				width: U.value.width,
				height: U.value.height
			}, u = {
				x: c.x - l.width / 2,
				y: c.y - l.height / 2,
				width: l.width,
				height: l.height
			};
			if (!n) {
				Kt.value = u;
				return;
			}
			let d = { ...l }, f = { ...u }, p = null, ee = (e) => {
				p ??= e;
				let t = Math.min((e - p) / 250, 1);
				Kt.value = {
					x: d.x + (f.x - d.x) * t,
					y: d.y + (f.y - d.y) * t,
					width: d.width + (f.width - d.width) * t,
					height: d.height + (f.height - d.height) * t
				}, t < 1 && requestAnimationFrame(ee);
			};
			requestAnimationFrame(ee);
		}
		Se(async () => {
			_t(), await ve(), requestAnimationFrame(() => {
				A.value || (en(), (L.value.map.center?.[0] || L.value.map.center?.[1]) && Rn(L.value.map.center, { animated: !1 }));
			});
		}), xe(() => {
			gt();
		});
		async function zn() {
			if (We("copyAlt", {
				config: L.value,
				dataset: ut.value
			}), !L.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(L.value.userOptions.callbacks.altCopy({
				config: L.value,
				dataset: ut.value
			}));
		}
		function Bn(e, t) {
			let n = G.value;
			if (!n.length) return null;
			let r = n[e];
			if (!r) return null;
			let i = null, a = Infinity;
			for (let o = 0; o < n.length; o += 1) {
				if (o === e) continue;
				let s = n[o], c = s.x - r.x, l = s.y - r.y, u = !1, d = 0, f = 0;
				if (t === "right" && c > 0 && (u = !0, d = c, f = Math.abs(l)), t === "left" && c < 0 && (u = !0, d = Math.abs(c), f = Math.abs(l)), t === "down" && l > 0 && (u = !0, d = l, f = Math.abs(c)), t === "up" && l < 0 && (u = !0, d = Math.abs(l), f = Math.abs(c)), !u) continue;
				let p = d * d + f * f * 4;
				p < a && (a = p, i = o);
			}
			return i;
		}
		function Vn(e) {
			let t = G.value;
			return t.length ? e === "right" ? t.reduce((e, t, n, r) => t.x < r[e].x ? n : e, 0) : e === "left" ? t.reduce((e, t, n, r) => t.x > r[e].x ? n : e, 0) : e === "down" ? t.reduce((e, t, n, r) => t.y < r[e].y ? n : e, 0) : e === "up" ? t.reduce((e, t, n, r) => t.y > r[e].y ? n : e, 0) : 0 : null;
		}
		function Hn() {
			nt.value = !0;
		}
		function Un() {
			let e = P.value == null ? null : G.value[P.value];
			e ? bn(e, "keyboard") : (P.value = null, F.value = "pointer", Y(), q.value = null), nt.value = !1;
		}
		function Wn(e) {
			if (!R.value || Z.value || document.activeElement !== R.value || !G.value.length) return;
			let t = e.key === "ArrowRight", n = e.key === "ArrowLeft", r = e.key === "ArrowDown", i = e.key === "ArrowUp", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				let e = P.value == null ? null : G.value[P.value];
				e ? bn(e, "keyboard") : (P.value = null, F.value = "pointer", Y(), q.value = null);
				return;
			}
			if (a) {
				let e = P.value == null ? null : G.value[P.value];
				if (!e) return;
				Sn(e);
				return;
			}
			let s = t ? "right" : n ? "left" : r ? "down" : "up", c = null;
			if (P.value == null ? c = Vn(s) : (c = Bn(P.value, s), c ??= Vn(s)), c == null) return;
			let l = G.value[c];
			l && yn(l, c, "keyboard");
		}
		let Gn = m(() => ({
			headers: [
				"Name",
				"Latitude",
				"Longitude",
				"Description"
			],
			rows: sn.value.map((e) => {
				let t = Array.isArray(e.coordinates) ? e.coordinates[1] : null, n = Array.isArray(e.coordinates) ? e.coordinates[0] : null;
				return [
					e.name ?? "",
					Number.isFinite(t) ? String(t) : "",
					Number.isFinite(n) ? String(n) : "",
					e.description ?? ""
				];
			})
		}));
		return re({
			getImage: Pn,
			generatePdf: pt,
			generateImage: mt,
			generateSvg: Mn,
			toggleTooltip: Dn,
			toggleAnnotator: En,
			toggleFullscreen: rt,
			zoomIn: nn,
			zoomOut: rn,
			resetZoom: qt,
			focusLocation: Rn,
			copyAlt: zn
		}), (e, t) => (x(), _("div", {
			class: ye(`vue-data-ui-component vue-ui-geo ${I.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${z.value ? "vue-ui-geo-responsive" : ""} ${Q.value ? "vue-ui-geo-interacting" : ""}`),
			ref_key: "geoChart",
			ref: k,
			id: `map_${O.value}`,
			dir: "auto",
			style: be({
				fontFamily: L.value.style.fontFamily,
				width: "100%",
				backgroundColor: L.value.style.chart.backgroundColor,
				height: z.value ? `${H.value}px` : void 0
			})
		}, [
			v("div", {
				id: `chart-instructions-${O.value}`,
				class: "sr-only"
			}, [v("p", null, we(L.value.a11y.translations.keyboardNavigation), 1)], 8, Oe),
			Gn.value?.rows?.length ? (x(), h(oe, {
				key: 0,
				uid: O.value,
				head: Gn.value.headers,
				body: Gn.value.rows,
				notice: L.value.a11y.translations.tableAvailable,
				caption: L.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : g("", !0),
			vt.value ? (x(), _("div", {
				key: 1,
				ref_key: "noTitle",
				ref: Ze,
				class: "vue-data-ui-no-title-space",
				style: "height:36px; width: 100%;background:transparent"
			}, null, 512)) : g("", !0),
			L.value.style.chart.title.text ? (x(), _("div", {
				key: 2,
				ref_key: "chartTitle",
				ref: Xe,
				style: "width:100%;background:transparent;padding-bottom:12px"
			}, [(x(), h(ne, {
				key: `title_${Ye.value}`,
				config: {
					title: {
						cy: "geo-title",
						...L.value.style.chart.title
					},
					subtitle: {
						cy: "geo-subtitle",
						...L.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : g("", !0),
			L.value.userOptions.buttons.annotator ? (x(), h(w(He), {
				key: 3,
				svgRef: w(R),
				backgroundColor: L.value.style.chart.backgroundColor,
				color: L.value.style.chart.color,
				active: Z.value,
				isCursorPointer: at.value,
				onClose: En
			}, {
				"annotator-action-close": E(() => [C(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": E(({ color: t }) => [C(e.$slots, "annotator-action-color", b(y({ color: t })), void 0, !0)]),
				"annotator-action-draw": E(({ mode: t }) => [C(e.$slots, "annotator-action-draw", b(y({ mode: t })), void 0, !0)]),
				"annotator-action-undo": E(({ disabled: t }) => [C(e.$slots, "annotator-action-undo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": E(({ disabled: t }) => [C(e.$slots, "annotator-action-redo", b(y({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": E(({ disabled: t }) => [C(e.$slots, "annotator-action-delete", b(y({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : g("", !0),
			L.value.userOptions.show && (w(st) || w(ot)) ? (x(), h(w(Ve), {
				ref: "userOptionsRef",
				key: `user_options_${Je.value}`,
				backgroundColor: L.value.style.chart.backgroundColor,
				color: L.value.style.chart.color,
				isPrinting: w(dt),
				isImaging: w(ft),
				uid: O.value,
				hasXls: !1,
				hasTable: !1,
				hasTooltip: L.value.userOptions.buttons.tooltip && L.value.style.chart.tooltip.show,
				hasPdf: L.value.userOptions.buttons.pdf,
				hasImg: L.value.userOptions.buttons.img,
				hasSvg: L.value.userOptions.buttons.svg,
				hasFullscreen: L.value.userOptions.buttons.fullscreen,
				hasAltCopy: L.value.userOptions.buttons.altCopy,
				hasAnnotator: L.value.userOptions.buttons.annotator,
				hasZoom: L.value.userOptions.buttons.zoom,
				isZoom: B.value,
				isFullscreen: I.value,
				isTooltip: yt.value.showTooltip,
				titles: { ...L.value.userOptions.buttonTitles },
				chartElement: k.value,
				position: L.value.userOptions.position,
				isAnnotation: Z.value,
				callbacks: L.value.userOptions.callbacks,
				printScale: L.value.userOptions.print.scale,
				isCursorPointer: at.value,
				onToggleFullscreen: rt,
				onGeneratePdf: w(pt),
				onGenerateImage: w(Nn),
				onGenerateSvg: w(Mn),
				onToggleTooltip: Dn,
				onToggleAnnotator: En,
				onToggleZoom: Zt,
				onCopyAlt: zn,
				style: be({ visibility: w(st) ? w(ot) ? "visible" : "hidden" : "visible" })
			}, me({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: E(({ isOpen: t, color: n }) => [C(e.$slots, "menuIcon", b(y({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionTooltip ? {
					name: "optionTooltip",
					fn: E(() => [C(e.$slots, "optionTooltip", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: E(() => [C(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: E(() => [C(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: E(() => [C(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: E(({ toggleFullscreen: t, isFullscreen: n }) => [C(e.$slots, "optionFullscreen", b(y({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: E(({ toggleAnnotator: t, isAnnotator: n }) => [C(e.$slots, "optionAnnotator", b(y({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionZoom ? {
					name: "optionZoom",
					fn: E(({ toggleZoom: t, isZoomLocked: n }) => [C(e.$slots, "optionZoom", b(y({
						toggleZoom: t,
						isZoomLocked: n
					})), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: E(({ altCopy: t }) => [C(e.$slots, "optionAltCopy", b(y({ altCopy: t })), void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: E(() => [C(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "9"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: E(() => [C(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "10"
				} : void 0
			]), 1032, /* @__PURE__ */ "backgroundColor.color.isPrinting.isImaging.uid.hasTooltip.hasPdf.hasImg.hasSvg.hasFullscreen.hasAltCopy.hasAnnotator.hasZoom.isZoom.isFullscreen.isTooltip.titles.chartElement.position.isAnnotation.callbacks.printScale.isCursorPointer.onGeneratePdf.onGenerateImage.onGenerateSvg.style".split("."))) : g("", !0),
			L.value.style.chart.controls.position === "top" && L.value.style.chart.controls.show && !w(lt) ? (x(), h(ue, {
				key: 5,
				ref_key: "zoomControlsTop",
				ref: $e,
				config: L.value,
				scale: w(Yt),
				isFullscreen: I.value,
				isCursorPointer: at.value,
				onZoomIn: nn,
				onZoomOut: rn,
				onResetZoom: an
			}, null, 8, [
				"config",
				"scale",
				"isFullscreen",
				"isCursorPointer"
			])) : g("", !0),
			v("div", ke, [(x(), _("svg", {
				ref_key: "svgRef",
				ref: R,
				xmlns: w(a),
				viewBox: tn.value,
				preserveAspectRatio: "xMidYMid meet",
				"aria-describedby": `map-keyboard-instructions-${O.value}`,
				tabindex: "0",
				style: be({
					display: "block",
					width: "100%",
					height: z.value ? `${H.value}px` : "auto",
					background: L.value.style.chart.backgroundColor,
					touchAction: B.value ? "none" : "auto",
					cursor: B.value ? Q.value ? "grabbing" : "grab" : "default"
				}),
				id: `vue-ui-geo_${O.value}`,
				onPointerdown: Fn,
				onPointerup: In,
				onPointercancel: In,
				onPointerleave: In,
				onWheel: Ln,
				onFocus: Hn,
				onBlur: Un,
				onKeydown: Wn
			}, [he(w(Ue)), v("g", {
				transform: Wt.value.transform,
				style: be({ pointerEvents: Q.value ? "none" : "auto" })
			}, [
				v("g", null, [(x(!0), _(pe, null, Ce(Lt.value, (e) => (x(), _("path", {
					class: "vue-ui-geo-territory",
					key: e.uid,
					d: e.path,
					fill: J.value === e.uid ? X.value.hover.fill : X.value.fill,
					stroke: J.value === e.uid ? X.value.hover.stroke : X.value.stroke,
					"stroke-width": J.value === e.uid ? X.value.hover.strokeWidth : X.value.strokeWidth,
					"vector-effect": "non-scaling-stroke",
					onMouseenter: (t) => gn(e),
					onMouseleave: (t) => _n(e),
					onClick: (t) => vn(e)
				}, null, 40, Me))), 128))]),
				v("g", null, [(x(!0), _(pe, null, Ce(Rt.value, (e) => (x(), _("path", {
					class: "vue-ui-geo-territory",
					key: e.uid,
					d: e.path,
					fill: "none",
					stroke: J.value === e.uid ? X.value.hover.stroke : X.value.stroke,
					"stroke-width": J.value === e.uid ? X.value.hover.strokeWidth : X.value.strokeWidth,
					"vector-effect": "non-scaling-stroke",
					onMouseenter: (t) => gn(e),
					onMouseleave: (t) => _n(e),
					onClick: (t) => vn(e)
				}, null, 40, Ne))), 128))]),
				v("g", null, [(x(!0), _(pe, null, Ce(zt.value, (e) => (x(), _("circle", {
					key: e.uid,
					cx: e.x,
					cy: e.y,
					r: e.radius,
					fill: e.fill,
					stroke: e.stroke,
					"stroke-width": e.strokeWidth,
					"vector-effect": "non-scaling-stroke",
					onMouseenter: (t) => wn(e),
					onMouseleave: Tn
				}, null, 40, Pe))), 128))]),
				(x(!0), _(pe, null, Ce(cn.value, (t, n) => (x(), _("g", { key: t.uid }, [C(e.$slots, "datapoint", _e({ ref_for: !0 }, {
					point: t,
					onPointEnter: yn,
					onPointLeave: bn,
					onPointClick: Sn,
					highlighted: q.value === t.uid
				}), () => [v("circle", {
					class: ye({
						"vue-ui-geo-point": !0,
						"vue-ui-geo-point-with-event": xn.value && at.value
					}),
					cx: t.x,
					cy: t.y,
					r: q.value === t.uid ? t.radius * t.hoverRadiusRatio : t.radius,
					fill: t.fill,
					stroke: Cn.value.stroke,
					"stroke-width": Cn.value.strokeWidth,
					"vector-effect": "non-scaling-stroke",
					onMouseenter: (e) => yn(t, n),
					onMouseleave: (e) => bn(t),
					onClick: (e) => Sn(t)
				}, null, 42, Fe)], !0), L.value.style.chart.points.labels.show ? (x(), _("text", {
					key: 0,
					class: "vue-ui-geo-point-label",
					x: t.x,
					y: t.y + (q.value === t.uid ? t.radius * t.hoverRadiusRatio : t.radius) + L.value.style.chart.points.labels.offsetY + 1 * L.value.style.chart.points.labels.fontSizeRatio,
					"text-anchor": "middle",
					fill: L.value.style.chart.points.labels.color,
					"font-size": 1 * L.value.style.chart.points.labels.fontSizeRatio
				}, we(t.name), 9, Ie)) : g("", !0)]))), 128)),
				C(e.$slots, "svg", { svg: {
					drawingArea: tn.value,
					x: Ht.value?.minX ?? 0,
					y: Ht.value?.minY ?? 0,
					width: Ht.value?.width ?? 0,
					height: Ht.value?.height ?? 0,
					isPrintingImg: w(dt) || w(ft) || w(An),
					isPrintingSvg: w(jn),
					data: {
						areaPaths: Lt.value,
						linePaths: Rt.value,
						geoJsonPoints: zt.value,
						projectedPoints: cn.value
					}
				} }, void 0, !0)
			], 12, je)], 44, Ae)), e.$slots.hint ? (x(), _("div", Le, [C(e.$slots, "hint", b(y({
				hint: L.value.a11y.translations.keyboardNavigation,
				isVisible: nt.value
			})), void 0, !0)])) : g("", !0)]),
			e.$slots.watermark ? (x(), _("div", Re, [C(e.$slots, "watermark", b(y({ isPrinting: w(dt) || w(ft) || w(An) || w(jn) })), void 0, !0)])) : g("", !0),
			L.value.style.chart.controls.position === "bottom" && L.value.style.chart.controls.show && !w(lt) ? (x(), h(ue, {
				key: 7,
				ref_key: "zoomControlsBottom",
				ref: et,
				config: L.value,
				scale: w(Yt),
				isFullscreen: I.value,
				isCursorPointer: at.value,
				onZoomIn: nn,
				onZoomOut: rn,
				onResetZoom: an
			}, null, 8, [
				"config",
				"scale",
				"isFullscreen",
				"isCursorPointer"
			])) : g("", !0),
			he(w(Be), {
				teleportTo: L.value.style.chart.tooltip.teleportTo,
				show: yt.value.showTooltip && un.value,
				backgroundColor: L.value.style.chart.tooltip.backgroundColor,
				color: L.value.style.chart.tooltip.color,
				fontSize: L.value.style.chart.tooltip.fontSize,
				borderRadius: L.value.style.chart.tooltip.borderRadius,
				borderColor: L.value.style.chart.tooltip.borderColor,
				borderWidth: L.value.style.chart.tooltip.borderWidth,
				backgroundOpacity: L.value.style.chart.tooltip.backgroundOpacity,
				position: L.value.style.chart.tooltip.position,
				offsetX: L.value.style.chart.tooltip.offsetX,
				offsetY: L.value.style.chart.tooltip.offsetY,
				parent: k.value,
				content: dn.value,
				isCustom: K.value,
				isFullscreen: I.value,
				smooth: L.value.style.chart.tooltip.smooth,
				backdropFilter: L.value.style.chart.tooltip.backdropFilter,
				smoothForce: L.value.style.chart.tooltip.smoothForce,
				smoothSnapThreshold: L.value.style.chart.tooltip.smoothSnapThreshold,
				isA11yMode: F.value === "keyboard",
				a11yPosition: tt.value
			}, {
				"tooltip-before": E(() => [C(e.$slots, "tooltip-before", b(y({ ...j.value })), void 0, !0)]),
				tooltip: E(() => [C(e.$slots, "tooltip", b(y({ ...j.value })), void 0, !0)]),
				"tooltip-after": E(() => [C(e.$slots, "tooltip-after", b(y({ ...j.value })), void 0, !0)]),
				_: 3
			}, 8, [
				"teleportTo",
				"show",
				"backgroundColor",
				"color",
				"fontSize",
				"borderRadius",
				"borderColor",
				"borderWidth",
				"backgroundOpacity",
				"position",
				"offsetX",
				"offsetY",
				"parent",
				"content",
				"isCustom",
				"isFullscreen",
				"smooth",
				"backdropFilter",
				"smoothForce",
				"smoothSnapThreshold",
				"isA11yMode",
				"a11yPosition"
			]),
			e.$slots.source ? (x(), _("div", {
				key: 8,
				ref_key: "source",
				ref: Qe,
				dir: "auto"
			}, [C(e.$slots, "source", {}, void 0, !0)], 512)) : g("", !0),
			C(e.$slots, "skeleton", {}, () => [w(lt) ? (x(), h(d, { key: 0 })) : g("", !0)], !0)
		], 14, De));
	}
}, [["__scopeId", "data-v-0dc55d82"]]);
//#endregion
export { Ee as n, Be as t };
