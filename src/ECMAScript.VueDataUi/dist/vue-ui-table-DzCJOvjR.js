import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, C as r, D as i, Jt as a, Ot as o, Pt as ee, X as s, d as c, q as te, r as ne } from "./lib-Bttd6u5E.js";
import { t as re } from "./vue-ui-xy-ChUQgqEu.js";
import { n as ie, t as ae } from "./useHints-Dq_w2E8B.js";
import { t as oe } from "./useConfig-DlNpz6P8.js";
import { t as l } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as u } from "./BaseIcon-BfndwIWE.js";
import { t as se } from "./vue-ui-donut-8RB-gL2J.js";
import { Fragment as d, computed as f, createBlock as ce, createCommentVNode as p, createElementBlock as m, createElementVNode as h, createTextVNode as g, createVNode as le, defineAsyncComponent as ue, nextTick as de, normalizeClass as _, normalizeStyle as v, onMounted as fe, openBlock as y, ref as b, renderList as x, toDisplayString as S, unref as C, useCssVars as pe, vModelSelect as me, vModelText as he, watch as ge, withCtx as _e, withDirectives as w, withKeys as ve, withModifiers as ye } from "vue";
//#region src/components/vue-ui-table.vue
var be = /* @__PURE__ */ e({ default: () => ln }), xe = ["innerHTML"], Se = ["data-is-open"], Ce = { class: "vue-ui-table-export-hub-options" }, we = { class: "vue-ui-table-export-hub-option-wrapper" }, Te = { class: "label" }, Ee = ["innerHTML"], De = { class: "vue-ui-table-export-hub-option-wrapper" }, Oe = { class: "label" }, ke = ["innerHTML"], Ae = { class: "vue-ui-table-dialog-field" }, je = {
	class: "label vue-ui-table-dialog-input-label",
	style: { width: "100%" }
}, Me = { style: { width: "fit-content" } }, Ne = { class: "vue-ui-table" }, Pe = { key: 0 }, Fe = { key: 0 }, Ie = { key: 1 }, Le = {
	key: 0,
	style: {
		display: "flex",
		"align-items": "center",
		"justify-content": "flex-end"
	}
}, Re = ["innerHTML"], ze = {
	key: 0,
	style: { "margin-left": "3px" }
}, Be = { key: 0 }, Ve = { class: "th-filter" }, He = {
	key: 0,
	class: "th-date"
}, Ue = { class: "date-wrapper--inputs" }, We = { class: "date-fieldset" }, Ge = ["for"], Ke = [
	"id",
	"onUpdate:modelValue",
	"onInput"
], qe = { class: "date-fieldset" }, Je = ["for"], Ye = [
	"id",
	"onUpdate:modelValue",
	"onInput"
], Xe = { class: "date-wrapper--button" }, Ze = ["onClick"], Qe = ["innerHTML"], $e = ["innerHTML"], et = ["innerHTML"], tt = ["onClick", "disabled"], nt = [
	"placeholder",
	"onUpdate:modelValue",
	"name"
], rt = ["onClick"], it = ["innerHTML"], at = ["innerHTML"], ot = ["innerHTML"], st = ["onClick", "innerHTML"], ct = ["innerHTML"], lt = {
	key: 5,
	class: "th-range-filter"
}, ut = ["for"], dt = [
	"id",
	"max",
	"min",
	"onUpdate:modelValue"
], ft = [
	"id",
	"max",
	"min",
	"onUpdate:modelValue"
], pt = ["for"], mt = ["onClick", "disabled"], ht = ["id"], gt = ["onClick"], _t = ["onClick", "onKeyup"], vt = [
	"innerHTML",
	"onClick",
	"onKeyup"
], yt = ["data-row"], bt = ["data-row"], xt = [
	"data-row",
	"onClick",
	"onKeyup",
	"id"
], St = ["innerHTML"], Ct = { key: 1 }, wt = { key: 2 }, Tt = { key: 5 }, Et = ["innerHTML"], Dt = { key: 0 }, Ot = { style: { "margin-left": "12px" } }, kt = { class: "format-num" }, At = { style: { "margin-left": "12px" } }, jt = {
	key: 0,
	class: "format-num"
}, Mt = {
	key: 1,
	class: "format-num"
}, Nt = { key: 2 }, Pt = { style: { "margin-left": "12px" } }, Ft = {
	key: 0,
	class: "format-num"
}, It = {
	key: 1,
	class: "format-num"
}, Lt = { key: 2 }, Rt = {
	key: 1,
	class: "vue-ui-table-paginator format-num"
}, zt = {
	key: 2,
	class: "vue-ui-table-size-warning"
}, Bt = ["innerHTML"], Vt = {
	key: 4,
	class: "vue-ui-table-pagination format-num"
}, Ht = ["innerHTML", "disabled"], Ut = ["disabled"], Wt = {
	for: "pageScroller",
	style: { "font-size": "14px" }
}, Gt = ["max", "value"], Kt = { key: 1 }, qt = ["disabled"], Jt = ["innerHTML", "disabled"], Yt = { class: "vue-ui-table-chart-modal-options" }, Xt = ["innerHTML"], Zt = ["innerHTML"], Qt = ["innerHTML"], $t = { style: {
	width: "100%",
	height: "fit-content"
} }, en = { class: "vue-ui-table-fieldset" }, tn = { class: "vue-ui-table-fieldset-wrapper" }, nn = [
	"name",
	"id",
	"checked",
	"onInput"
], rn = ["for"], an = ["disabled"], on = ["innerHTML"], sn = { style: {
	width: "100%",
	"margin-bottom": "12px"
} }, cn = {
	key: 2,
	style: {
		width: "100%",
		"margin-bottom": "32px"
	}
}, ln = /*#__PURE__*/ l({
	__name: "vue-ui-table",
	props: {
		config: {
			type: Object,
			default() {
				return {};
			}
		},
		dataset: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	emits: ["page-change"],
	setup(e, { expose: l, emit: be }) {
		pe((e) => ({
			d1121f2a: An.value,
			f2e08f04: jn.value,
			v7663b4a0: Fn.value,
			v17c31306: Mn.value,
			v7dc13a5c: In.value,
			v76f5497a: Ln.value,
			fa998e08: Nn.value,
			v4d3bc194: Pn.value,
			v42ae93a6: Rn.value
		}));
		let ln = ue(() => import("./BaseDraggableDialog-LoqqwRtV.js").then((e) => e.n)), T = e, un = be, { vue_ui_table: dn } = oe(), E = te();
		b(null);
		let fn = b(null), pn = b(null), D = b({
			height: 316,
			type: "bar",
			width: 512
		}), O = b({
			ASC: 1,
			BAR: "bar",
			DATE: "date",
			DESC: -1,
			DONUT: "donut",
			LINE: "line",
			NUMERIC: "numeric",
			PERCENTAGE: "percentage",
			TEXT: "text"
		}), mn = b(100), hn = b(100);
		b(0), b(0), b(400), b(200);
		let gn = b({
			CELL: "smart-td-selected",
			FIRST_TD: "smart-td-selected-first",
			LAST_TD: "smart-td-selected-last",
			ROW: "smart-td-selected-neighbor"
		}), _n = b(void 0), vn = b(void 0), k = b({
			col: void 0,
			rows: []
		}), A = b(0), j = b(20), yn = b(!1), bn = b(!1), M = b({}), N = b({}), xn = b({}), P = b({}), F = b({}), I = b({}), L = b({}), R = b({}), z = b({}), B = b(T.config.rowsPerPage ? T.config.rowsPerPage : 25), Sn = b([.../* @__PURE__ */ new Set([
			10,
			25,
			50,
			100,
			250,
			500,
			T.config.rowsPerPage ? T.config.rowsPerPage : 25,
			T.dataset.body.length
		])].sort((e, t) => e - t)), Cn = b(void 0), V = b(void 0), wn = b(void 0), H = b(!1), Tn = b(0), En = b(""), Dn = b(""), U = b(!1), W = b(JSON.parse(JSON.stringify(T.dataset.body)).map((e, t) => ({
			...e,
			absoluteIndex: t
		}))), On = b(JSON.parse(JSON.stringify(T.dataset.body)).map((e, t) => ({
			...e,
			absoluteIndex: t
		}))), G = b(JSON.parse(JSON.stringify(T.dataset.header)).map((e, t) => ({
			average: Object.hasOwn(e, "average") ? e.average : !1,
			decimals: Object.hasOwn(e, "decimals") ? e.decimals : 0,
			isMultiselect: Object.hasOwn(e, "isMultiselect") ? e.isMultiselect : !1,
			isPercentage: Object.hasOwn(e, "isPercentage") ? e.isPercentage : !1,
			isSearch: Object.hasOwn(e, "isSearch") ? e.isSearch : !1,
			isSort: Object.hasOwn(e, "isSort") ? e.isSort : !1,
			name: e.name,
			percentageTo: Object.hasOwn(e, "percentageTo") ? e.percentageTo : void 0,
			prefix: Object.hasOwn(e, "prefix") ? e.prefix : "",
			rangeFilter: Object.hasOwn(e, "rangeFilter") ? e.rangeFilter : !1,
			suffix: Object.hasOwn(e, "suffix") ? e.suffix : "",
			sum: Object.hasOwn(e, "sum") ? e.sum : !1,
			type: e.type,
			index: t
		}))), kn = f(() => fn.value ? fn.value.getBoundingClientRect().height + 3 : 3), K = f(() => {
			if (!Object.keys(T.config || {}).length) return dn;
			let e = a({
				defaultConfig: dn,
				userConfig: T.config
			});
			return r(e);
		});
		ie({
			config: () => K.value,
			dataset: () => T.dataset,
			component: "VueUiTable",
			rules: [ae.noHint]
		});
		let q = f(() => K.value.useCursorPointer), An = f(() => K.value.style.th.buttons.cancel.inactive.backgroundColor), jn = f(() => K.value.style.th.buttons.cancel.inactive.color), Mn = f(() => K.value.style.th.buttons.cancel.active.backgroundColor), Nn = f(() => K.value.style.th.buttons.filter.active.backgroundColor), Pn = f(() => K.value.style.th.buttons.filter.active.color), Fn = f(() => o(Mn.value, .33)), In = f(() => n(Mn.value, 33)), Ln = f(() => o(Nn.value, .33)), Rn = f(() => n(Nn, 33)), zn = f(() => [...G.value].filter((e) => e.type === O.value.DATE)), Bn = f(() => ["", ...zn.value.map((e) => e.name)]), Vn = f(() => {
			let e = zn.value.find((e) => e.name === En.value);
			return e ? e.index : null;
		}), J = f(() => {
			let e = [];
			if (W.value.length) for (let t = 0; t < W.value.length; t += B.value) e.push(W.value.slice(t, t + B.value));
			return e;
		}), Y = f(() => J.value[A.value]), Hn = f(() => Vn.value == null ? [] : Y.value.map((e) => e.td[Vn.value])), Un = f(() => Object.keys(F.value).map((e) => ({
			index: e,
			name: T.dataset.header[e].name,
			options: F.value[e]
		}))), Wn = f(() => K.value.useChart && k.value.rows.length > 1), Gn = f(() => T.dataset.header.map((e) => e.type).includes(O.value.NUMERIC)), X = f(() => ({
			arrowSort: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M3 9l4 -4l4 4m-4 -4v14" /><path d="M21 15l-4 4l-4 -4m4 4v-14" /></svg>`,
			bar: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M3 12m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v6a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z" /><path d="M9 8m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v10a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z" /><path d="M15 4m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v14a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z" /><path d="M4 20l14 0" /></svg>`,
			chart: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 19l16 0" /><path d="M4 15l4 -6l4 2l4 -5l4 4" /></svg>`,
			chevronDown: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M6 9l6 6l6 -6" /></svg>`,
			chevronLeft: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value * 1.6}" height="${j.value * 1.6}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M15 6l-6 6l6 6" /></svg>`,
			chevronRight: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value * 1.6}" height="${j.value * 1.6}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M9 6l6 6l-6 6" /></svg>`,
			donut: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value * .8}" height="${j.value * .8}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M12 3v5m4 4h5" /><path d="M8.929 14.582l-3.429 2.918" /><path d="M12 12m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" /><path d="M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0" /></svg>`,
			export: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M12.5 21h-7.5a2 2 0 0 1 -2 -2v-14a2 2 0 0 1 2 -2h14a2 2 0 0 1 2 2v7.5" /><path d="M3 10h18" /><path d="M10 3v18" /><path d="M16 19h6" /><path d="M19 16l3 3l-3 3" /></svg>`,
			fileDownload: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M14 3v4a1 1 0 0 0 1 1h4" /><path d="M17 21h-10a2 2 0 0 1 -2 -2v-14a2 2 0 0 1 2 -2h7l5 5v11a2 2 0 0 1 -2 2z" /><path d="M12 17v-6" /><path d="M9.5 14.5l2.5 2.5l2.5 -2.5" /></svg>`,
			filter: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 4h16v2.172a2 2 0 0 1 -.586 1.414l-4.414 4.414v7l-6 2v-8.5l-4.48 -4.928a2 2 0 0 1 -.52 -1.345v-2.227z" /></svg>`,
			move: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M18 9l3 3l-3 3" /><path d="M15 12h6" /><path d="M6 9l-3 3l3 3" /><path d="M3 12h6" /><path d="M9 18l3 3l3 -3" /><path d="M12 15v6" /><path d="M15 6l-3 -3l-3 3" /><path d="M12 3v6" /></svg>`,
			sort09: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 15l3 3l3 -3" /><path d="M7 6v12" /><path d="M17 3a2 2 0 0 1 2 2v3a2 2 0 1 1 -4 0v-3a2 2 0 0 1 2 -2z" /><path d="M17 16m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0" /><path d="M19 16v3a2 2 0 0 1 -2 2h-1.5" /></svg>`,
			sort90: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 15l3 3l3 -3" /><path d="M7 6v12" /><path d="M17 14a2 2 0 0 1 2 2v3a2 2 0 1 1 -4 0v-3a2 2 0 0 1 2 -2z" /><path d="M17 5m-2 0a2 2 0 1 0 4 0a2 2 0 1 0 -4 0" /><path d="M19 5v3a2 2 0 0 1 -2 2h-1.5" /></svg>`,
			sortAZ: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M15 10v-5c0 -1.38 .62 -2 2 -2s2 .62 2 2v5m0 -3h-4" /><path d="M19 21h-4l4 -7h-4" /><path d="M4 15l3 3l3 -3" /><path d="M7 6v12" /></svg>`,
			sortZA: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M15 21v-5c0 -1.38 .62 -2 2 -2s2 .62 2 2v5m0 -3h-4" /><path d="M19 10h-4l4 -7h-4" /><path d="M4 15l3 3l3 -3" /><path d="M7 6v12" /></svg>`,
			sum: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M18 16v2a1 1 0 0 1 -1 1h-11l6 -7l-6 -7h11a1 1 0 0 1 1 1v2" /></svg>`,
			table: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" fill="white" d="M 10 2, 21 2, 21 21, 10 21Z"/><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M3 5a2 2 0 0 1 2 -2h14a2 2 0 0 1 2 2v14a2 2 0 0 1 -2 2h-14a2 2 0 0 1 -2 -2v-14z" /><path d="M3 10h18" /><path d="M10 3v18" /></svg>`,
			warning: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value * .8}" height="${j.value * .8}" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M10.24 3.957l-8.422 14.06a1.989 1.989 0 0 0 1.7 2.983h16.845a1.989 1.989 0 0 0 1.7 -2.983l-8.423 -14.06a1.989 1.989 0 0 0 -3.4 0z" /><path d="M12 9v4" /><path d="M12 17h.01" /></svg>`,
			grip: `<svg xmlns="http://www.w3.org/2000/svg" width="${j.value}" height="${j.value}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"><path d="M5 9m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" /><path d="M5 15m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" /><path d="M12 9m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" /><path d="M12 15m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" /><path d="M19 9m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" /><path d="M19 15m-1 0a1 1 0 1 0 2 0a1 1 0 1 0 -2 0" /></svg>`
		})), Kn = f(() => W.value.map((e) => e.td)), qn = f(() => ({
			sumPercentage: Number((k.value.rows.map((e) => e.value).reduce((e, t) => e + t, 0) * 100).toFixed(T.dataset.header[k.value.col].decimals)).toLocaleString(),
			sumRegular: Number(k.value.rows.map((e) => e.value).reduce((e, t) => e + t, 0).toFixed(T.dataset.header[k.value.col].decimals)).toLocaleString(),
			averagePercentage: Number((k.value.rows.map((e) => e.value).reduce((e, t) => e + t, 0) / k.value.rows.length * 100).toFixed(T.dataset.header[k.value.col].decimals)).toLocaleString(),
			averageRegular: Number((k.value.rows.map((e) => e.value).reduce((e, t) => e + t, 0) / k.value.rows.length).toFixed(T.dataset.header[k.value.col].decimals)).toLocaleString()
		})), Jn = b({
			width: 1e3,
			height: 500
		});
		function Yn({ width: e, height: t }) {
			Jn.value = {
				width: e,
				height: t
			};
		}
		let Xn = f(() => {
			if (!Wn.value) return [];
			let e = Jn.value.height, t = Jn.value.width, n = t / k.value.rows.length, r = Math.max(...k.value.rows.map((e) => e.value)), i = Math.min(...k.value.rows.map((e) => e.value)), a = T.dataset.header[k.value.col].isPercentage && T.dataset.header[k.value.col].percentageTo, ee = D.value.type === O.value.DONUT && V.value && V.value.name, s = T.dataset.header[k.value.col].name + (a ? ` / ${T.dataset.header[I.value[k.value.col].referenceIndex].name}` : "") + (ee ? ` ${K.value.translations.by} ${V.value.name}` : ""), te = T.dataset.header[k.value.col].prefix, re = T.dataset.header[k.value.col].suffix, ie = [{
				name: s,
				series: k.value.rows.map((e) => e.value),
				type: "line",
				useProgression: !0,
				smooth: K.value.style.chart.layout.line.smooth,
				color: K.value.style.chart.layout.line.stroke,
				useArea: K.value.style.chart.layout.line.useArea
			}], ae = [{
				name: s,
				series: k.value.rows.map((e) => e.value),
				type: "bar",
				useProgression: !0,
				color: K.value.style.chart.layout.bar.fill
			}], oe = K.value.style.chart.modal.backgroundColor, l = ne(oe), u = T.dataset.header[k.value.col].decimals, se = {
				useCssAnimation: !1,
				chart: {
					width: t,
					height: e - 250,
					backgroundColor: oe,
					color: l,
					labels: {
						fontSize: 18,
						prefix: te,
						suffix: re
					},
					grid: {
						stroke: o(l, .5),
						labels: {
							color: l,
							xAxisLabels: {
								color: l,
								show: Hn.value.length,
								values: Hn.value,
								datetimeFormatter: K.value.style.chart.layout.datetimeFormatter,
								showOnlyAtModulo: K.value.style.chart.layout.timeLabels.showOnlyAtModulo,
								modulo: K.value.style.chart.layout.timeLabels.modulo
							}
						}
					},
					highlighter: { color: l },
					legend: {
						color: l,
						show: !1
					},
					title: {
						text: s,
						color: l,
						textAlign: "center",
						fontSize: 18
					},
					tooltip: {
						showTimeLabel: Hn.value.length,
						backgroundOpacity: 30,
						color: l,
						backgroundColor: oe,
						showPercentage: !1,
						roundingValue: u
					},
					userOptions: { buttons: {
						pdf: !1,
						csv: !1,
						table: !1,
						annotator: !1
					} },
					zoom: {
						show: K.value.style.chart.layout.zoom.show,
						autoFit: K.value.style.chart.layout.zoom.autoFit,
						focusOnDrag: !0,
						minimap: { show: !0 }
					}
				},
				line: {
					labels: {
						show: !0,
						color: l,
						rounding: u
					},
					dot: {
						useSerieColor: !1,
						fill: K.value.style.chart.layout.line.plot.fill,
						strokeWidth: K.value.style.chart.layout.line.plot.strokeWidth
					}
				},
				bar: {
					useGradient: !1,
					border: {
						useSerieColor: !1,
						stroke: K.value.style.chart.layout.bar.stroke,
						strokeWidth: K.value.style.chart.layout.bar.strokeWidth
					},
					labels: {
						show: !0,
						color: l,
						rounding: u
					}
				}
			}, d = i >= 0 ? 0 : Math.abs(i), f = r + d, ce = T.dataset.header[k.value.col].isPercentage, p = k.value.rows.map((t, r) => ({
				x: n * r + n / 2,
				y: (1 - (t.value + d) / f) * e,
				value: ce ? t.value * 100 : t.value,
				suffix: ce ? "%" : T.dataset.header[k.value.col].suffix ? T.dataset.header[k.value.col].suffix : "",
				prefix: T.dataset.header[k.value.col].prefix ? T.dataset.header[k.value.col].prefix : "",
				index: t.index,
				absoluteValue: ce ? Math.abs(t.value) * 100 : Math.abs(t.value)
			}));
			return {
				donutConfig: {
					useCssAnimation: !1,
					userOptions: {
						position: "left",
						buttons: {
							pdf: !1,
							csv: !1,
							table: !1,
							annotator: !1
						}
					},
					style: { chart: {
						backgroundColor: oe,
						color: l,
						layout: {
							curvedMarkers: !0,
							donut: { strokeWidth: 64 },
							labels: {
								dataLabels: {
									suffix: ce ? "%" : "",
									prefix: te
								},
								value: { rounding: u },
								percentage: {
									color: l,
									rounding: u
								},
								name: { color: l },
								hollow: {
									average: {
										color: l,
										text: K.value.translations.average,
										value: { color: l }
									},
									total: {
										color: l,
										offsetY: -12,
										text: K.value.translations.total,
										value: {
											color: l,
											offsetY: -12
										}
									}
								}
							}
						},
						legend: {
							backgroundColor: oe,
							color: l,
							roundingValue: u,
							roundingPercentage: u
						},
						padding: {
							left: 12,
							right: 12
						},
						title: {
							text: s,
							color: l,
							fontSize: 18
						},
						tooltip: {
							backgroundOpacity: 30,
							color: l,
							backgroundColor: oe,
							roundingValue: u,
							roundingValue: u
						}
					} }
				},
				xyConfig: se,
				xyDatasetLine: ie,
				xyDatasetBar: ae,
				progression: p.length >= 2 && c(p)
			};
		});
		function Zn() {
			let e = V.value.options.map((e, t) => ({
				name: e,
				color: ee[t] || ee[t % ee.length],
				values: [Y.value.filter((t, n) => t.td[V.value.index] === e && k.value.rows.map((e) => e.index).includes(n)).map((e) => e.td[k.value.col]).reduce((e, t) => Math.abs(e) + Math.abs(t), 0)],
				absoluteValue: Y.value.filter((t, n) => t.td[V.value.index] === e && k.value.rows.map((e) => e.index).includes(n)).map((e) => e.td[k.value.col]).reduce((e, t) => e + t, 0)
			})).sort((e, t) => t.value - e.value);
			_n.value = e, de(() => {
				D.value.type = O.value.DONUT, U.value = !1;
			});
		}
		function Qn(e, t) {
			return !N.value[e] && (t.isSort || t.isSearch || t.isMultiselect || t.rangeFilter) && ![O.value.DATE].includes(t.type);
		}
		function $n(e = "all") {
			let n = T.dataset.header.map((e) => e.name), r = e === "all" ? W.value.map((e) => e.td) : Y.value.map((e) => e.td), a = [n].concat(r), o = i(a);
			t({
				csvContent: o,
				title: Dn.value
			});
		}
		function er() {
			let e = document.getElementsByClassName("th-dropdown");
			e.length && Array.from(e).forEach((e) => {
				e.dataset.isOpen = !1;
			});
		}
		function tr(e, t) {
			let n;
			clearTimeout(n), n = setTimeout(e, t);
		}
		function Z() {
			let e = document.getElementsByClassName(`tr_${E}`);
			Array.from(e).forEach((e) => {
				Array.from(e.getElementsByTagName("td")).forEach((e) => {
					e.dataset.row === "even" ? (e.style.background = K.value.style.rows.even.backgroundColor, e.style.color = K.value.style.rows.even.color) : (e.style.background = K.value.style.rows.odd.backgroundColor, e.style.color = K.value.style.rows.odd.color);
				});
			}), Array.from(e).forEach((e) => e.dataset.selected = "false"), A.value > J.value.length - 1 && (A.value = J.value.length - 1), H.value = !1, _n.value = void 0, Cn.value = void 0, D.value.type = O.value.BAR, k.value = {
				col: void 0,
				rows: []
			}, mn.value = 100, hn.value = 100;
		}
		function nr(e, t) {
			W.value = e.filter((e) => e.td[t] >= L.value[t].min && e[t] <= L.value[t].max);
		}
		function rr(e, t) {
			if (z.value[t] === O.value.ASC && (e = e.sort((e, n) => e[t] - n[t])), z.value[t] === O.value.DESC) e = e.sort((e, n) => n[t] - e[t]);
			else return 0;
		}
		function ir() {
			Z(), Object.keys(L.value).forEach((e) => {
				nr(W.value, e);
			}), Object.keys(z.value).forEach((e) => {
				rr(W.value, e);
			}), vn.value !== void 0 && rr(W.value, vn.value), T.dataset.header.forEach((e, t) => {
				if (e.isPercentage) {
					let e = I.value[t].referenceIndex, n = W.value.map((t) => t.td[e]).reduce((e, t) => e + t, 0);
					W.value.forEach((r) => {
						r.td[t] = r.td[e] / n;
					});
				}
			}), A.value > J.value.length - 1 && (A.value = J.value.length - 1), [-1].includes(A.value) && (A.value = 0);
		}
		function Q() {
			W.value = On.value.filter((e) => {
				for (let t in R.value) if (!e.td[t].toUpperCase().includes(R.value[t].toUpperCase())) return !1;
				for (let t in F.value) if (!F.value[t].some((n) => n === e.td[t])) return !1;
				for (let t in M.value) {
					let n = new Date(e.td[t]), r = new Date(M.value[t].from), i = new Date(M.value[t].to);
					if (n < r || n > i) return !1;
				}
				return !0;
			}), ir();
		}
		function ar(e) {
			return Kn.value.map((t) => t[e]).map((e) => isNaN(Number(e)) ? 0 : e).reduce((e, t) => e + t, 0) / W.value.length;
		}
		function or(e) {
			let t = T.dataset.body.map((t) => new Date(t.td[e])), n = new Date(Math.min(...t)), r = new Date(Math.max(...t)), i = n.getFullYear(), a = r.getFullYear(), o = String(n.getMonth() + 1).padStart(2, "0"), ee = String(r.getMonth() + 1).padStart(2, "0"), s = String(n.getDate()).padStart(2, "0"), c = String(r.getDate()).padStart(2, "0");
			return {
				from: `${i}-${o}-${s}`,
				to: `${a}-${ee}-${c}`
			};
		}
		function $(e) {
			return [...new Set(T.dataset.body.map((t) => t.td[e]))];
		}
		function sr(e) {
			return Kn.value.map((t) => t[e]).map((e) => isNaN(Number(e)) ? 0 : e).reduce((e, t) => e + t, 0);
		}
		function cr(e) {
			return e.includes(NaN);
		}
		function lr(e, t) {
			return !F.value[t] || F.value[t].includes(e);
		}
		function ur(e) {
			return !isNaN(Number(String(e).replaceAll("%", "")));
		}
		function dr(e, t) {
			let n = t.isSort, r = t.isSearch, i = t.isMultiselect && F.value[e], a = t.rangeFilter, o = (e) => {
				if (a && L.value[e]) return Math.round(L.value[e].min) === P.value[e].min && Math.round(L.value[e].max) === P.value[e].max;
			};
			if (n && r && i && a) return ["", void 0].includes(R.value[e]) && [void 0].includes(z.value[e]) && F.value[e].length === $(e).length && o(e);
			if (n && r && i) return ["", void 0].includes(R.value[e]) && [void 0].includes(z.value[e]) && F.value[e].length === $(e).length;
			if (n && r && a) return ["", void 0].includes(R.value[e]) && [void 0].includes(z.value[e]) && o(e);
			if (n && r) return ["", void 0].includes(R.value[e]) && [void 0].includes(z.value[e]);
			if (n && i && a) return [void 0].includes(z.value[e]) && F.value[e].length === $(e).length && o(e);
			if (n && i) return [void 0].includes(z.value[e]) && F.value[e].length === $(e).length;
			if (r && i && a) return ["", void 0].includes(R.value[e]) && F.value[e].length === $(e).length && o(e);
			if (r && i) return ["", void 0].includes(R.value[e]) && F.value[e].length === $(e).length;
			if (r && a) return ["", void 0].includes(R.value[e]) && o(e);
			if (r) return ["", void 0].includes(R.value[e]);
			if (n && a) return [void 0].includes(z.value[e]) && o(e);
			if (n) return [void 0].includes(z.value[e]);
			if (i && a) return F.value[e].length === $(e).length && o(e);
			if (i) return F.value[e].length === $(e).length;
		}
		function fr() {
			return {
				totalPages: J.value.length,
				itemsPerPage: B.value,
				currentPage: A.value,
				currentPageData: Y.value.map((e) => e.td)
			};
		}
		l({ getCurrentPageData: fr });
		function pr() {
			un("page-change", fr());
		}
		let mr = b(null);
		function hr(e) {
			if (Z(), e === "next" && A.value < J.value.length) {
				if (A.value + 1 > J.value.length - 1) return;
				A.value += 1;
			} else if (e === "previous" && A.value >= 1) --A.value;
			else {
				if (e - 1 < 0 || e > J.value.length || e === "previous") return;
				A.value = e - 1;
			}
			pr(), mr.value && mr.value.scrollTo({
				top: 0,
				left: 0,
				behavior: "smooth"
			});
		}
		function gr(e) {
			e.preventDefault();
			let t = e.keyCode;
			if (![
				38,
				40,
				37,
				39
			].includes(t)) return;
			let n = e.target.id.match(/cell_(\d+)_(\d+)_([0-9a-fA-F-]{36})/), r = parseInt(n[1]), i = parseInt(n[2]), a = document.getElementById(`cell_${r}_${i + 1}_${E}`), o = document.getElementById(`cell_${r}_${i - 1}_${E}`), ee = document.getElementById(`cell_${r + 1}_${i}_${E}`), s = document.getElementById(`cell_${r - 1}_${i}_${E}`), c;
			switch (!0) {
				case t === 39:
					c = a;
					break;
				case t === 37:
					c = o;
					break;
				case t === 38:
					c = s;
					break;
				case t === 40:
					c = ee;
					break;
				default: return;
			}
			c && (c.focus(), c.scrollIntoView({
				behavior: "smooth",
				block: "center"
			}));
		}
		function _r(e) {
			let t = 0;
			for (let n = 0; n < e.length; n += 1) t += e.charCodeAt(n);
			return t;
		}
		async function vr() {
			return new Promise((e) => {
				let t = [];
				G.value.forEach((e, n) => {
					if (e.isSearch && Object.assign(R.value, { [n]: "" }), e.isMultiselect && Object.assign(F.value, { [n]: $(n) }), e.type === O.value.DATE && (Object.assign(M.value, { [n]: or(n) }), Object.assign(xn.value, { [n]: !1 })), (e.isPercentage || e.percentageTo) && Object.assign(I.value, { [n]: {
						reference: e.percentageTo,
						referenceIndex: T.dataset.header.map((e) => e.name).indexOf(e.percentageTo)
					} }), e.rangeFilter && (Object.assign(L.value, { [n]: {
						min: Math.round(Math.min(...T.dataset.body.map((e) => e.td).map((e) => e[n]))),
						max: Math.round(Math.max(...T.dataset.body.map((e) => e.td).map((e) => e[n])))
					} }), Object.assign(P.value, { [n]: {
						min: Math.round(Math.min(...T.dataset.body.map((e) => e.td).map((e) => e[n]))),
						max: Math.round(Math.max(...T.dataset.body.map((e) => e.td).map((e) => e[n])))
					} })), e.isPercentage) {
						let r = T.dataset.header.map((e) => e.name).indexOf(e.percentageTo), i = T.dataset.body.map((e) => e.td[r]).reduce((e, t) => e + t, 0);
						t.push([
							n,
							r,
							i
						]);
					}
					e.type === O.value.NUMERIC && !e.isPercentage && Object.assign(N.value, { [n]: cr(T.dataset.body.map((e) => Number(e.td[n]))) });
				}), W.value.forEach((e, n) => {
					t.map((t) => {
						let [n, r, i] = t;
						e.td[n] = e.td[r] / i;
					}), e.td.forEach((t, r) => {
						T.dataset.header[r].type === O.value.TEXT && T.dataset.header[r].isSearch && (e[r] = _r(t)), T.dataset.header[r].type === O.value.DATE && (e[r] = new Date(t).getTime()), T.dataset.header[r].type === O.value.NUMERIC && (e[r] = isNaN(Number(t)) ? r : t), On.value[n][r] = e[r];
					});
				}), e(!0);
			});
		}
		function yr(e, t) {
			return new Promise((n, r) => {
				e().then((e) => {
					try {
						n(t(e));
					} catch (e) {
						r(e);
					}
				}).catch((e) => {
					r(e);
				});
			});
		}
		async function br(e) {
			M.value[e] = {
				from: or(e).from,
				to: or(e).to
			}, xn.value[e] = !1, await de(), Q();
		}
		function xr(e, t, n) {
			let r = n.currentTarget;
			clearTimeout(pn.value), r.classList.add("clicked"), pn.value = setTimeout(() => {
				r.classList.remove("clicked");
			}, 200), vn.value = void 0, t.rangeFilter && (L.value[e].min = P.value[e].min, L.value[e].max = P.value[e].max), t.isMultiselect ? (F.value[e] = $(e), t.type === O.value.TEXT && (z.value[e] = void 0), t.isSearch && (R.value[e] = "")) : t.type === O.value.NUMERIC ? z.value[e] = void 0 : t.type === O.value.TEXT ? (z.value[e] = void 0, R.value[e] = "") : t.type === O.value.DATE && (z.value[e] = void 0), Q();
		}
		function Sr({ td: e, rowIndex: t, colIndex: n, headerType: r, event: i }) {
			if (r !== O.value.NUMERIC || isNaN(Number(e))) {
				Z();
				return;
			}
			k.value.col !== n && Z();
			let a = i.currentTarget.parentNode;
			k.value.col = n, k.value.rows.map((e) => e.index).includes(t) ? (a.dataset.selected = "false", k.value.rows = k.value.rows.filter((e) => e.index !== t), i.currentTarget.classList.remove(gn.value.CELL), Array.from(a.children).forEach((e, t) => {
				e.dataset.row === "even" ? (e.style.background = K.value.style.rows.even.backgroundColor, e.style.color = K.value.style.rows.even.olor) : (e.style.background = K.value.style.rows.odd.backgroundColor, e.style.color = K.value.style.rows.odd.color);
			}), i.currentTarget.dataset.row === "even" ? (i.currentTarget.style.background = K.value.style.rows.even.backgroundColor, i.currentTarget.style.color = K.value.style.rows.even.color) : (i.currentTarget.style.background = K.value.style.rows.odd.backgroundColor, i.currentTarget.style.color = K.value.style.rows.odd.color)) : (a.dataset.selected = "true", k.value.rows.push({
				index: t,
				value: e
			}), Array.from(a.children).forEach((e, t) => {
				e.dataset.row === "even" ? (e.style.background = K.value.style.rows.even.selectedNeighbors.backgroundColor, e.style.color = K.value.style.rows.even.selectedNeighbors.color) : (e.style.background = K.value.style.rows.odd.selectedNeighbors.backgroundColor, e.style.color = K.value.style.rows.odd.selectedNeighbors.color);
			}), i.currentTarget.dataset.row === "odd" ? (i.currentTarget.style.background = K.value.style.rows.odd.selectedCell.backgroundColor, i.currentTarget.style.color = K.value.style.rows.odd.selectedCell.color) : (i.currentTarget.style.background = K.value.style.rows.even.selectedCell.backgroundColor, i.currentTarget.style.color = K.value.style.rows.even.selectedCell.color)), k.value.rows = k.value.rows.sort((e, t) => e.index - t.index), D.value.type === O.value.DONUT && k.value.rows.length > 0 && Zn();
		}
		function Cr(e) {
			k.value.col === e ? (Cn.value = void 0, Z()) : (Y.value.forEach((t, n) => {
				Sr({
					td: t.td[e],
					rowIndex: n,
					colIndex: e,
					headerType: O.value.NUMERIC,
					event: { currentTarget: document.getElementById(`cell_${n}_${e}_${E}`) }
				});
			}), Cn.value = e);
		}
		async function wr(e, t) {
			F.value[t].includes(e) ? F.value[t] = F.value[t].filter((t) => t !== e) : F.value[t].push(e), await de(), Q();
		}
		function Tr(e) {
			xn.value[e] = or(e).from !== M.value[e].from || or(e).to !== M.value[e].to;
		}
		function Er(e, t) {
			vn.value = e;
			let n = t.currentTarget;
			clearTimeout(pn.value), n.classList.add("clicked"), pn.value = setTimeout(() => {
				n.classList.remove("clicked");
			}, 200), z.value[e] === 1 ? z.value[e] = O.value.DESC : z.value[e] = O.value.ASC, ir();
		}
		function Dr(e, t, n) {
			let r = n.currentTarget;
			clearTimeout(pn.value), r.classList.add("clicked"), pn.value = setTimeout(() => {
				r.classList.remove("clicked");
			}, 200);
			let i = document.getElementById(`th_dropdown_${e}`);
			i.dataset.isOpen === "false" ? i.dataset.isOpen = "true" : i.dataset.isOpen = "false";
		}
		function Or(e) {
			Z(), !e || !e.target.value ? A.value = 0 : A.value = Number(e.target.value), pr();
		}
		let kr = b(null);
		fe(() => {
			if (T.dataset.header.length === 0) throw Error("vue-ui-table error: missing header data.\nProvide an array of objects of type:\n{\n name: string;\n type: string; ('text' | 'numeric' | 'date')\n average: boolean;\n decimals: number | undefined;\n sum: boolean;\n isSort:boolean;\n isSearch: boolean;\n isMultiselect: boolean;\n isPercentage: boolean;\n percentageTo: string; (or '')\n}");
			if (T.dataset.body.length === 0) throw Error("vue-ui-table error: missing body data");
			bn.value = !0, yr(vr, async () => {
				await de(), bn.value = !1;
			}), document.addEventListener("keydown", (e) => {
				let t = document.activeElement;
				(t && Array.from(t.classList).includes("td-focusable") && e.key.includes("Arrow") || e.code === "Space") && e.preventDefault();
			}), Dn.value = K.value.style.exportMenu.filename, En.value = zn.value[0]?.name ?? "";
		}), ge(() => T.dataset, (e) => {
			bn.value = !0, W.value = JSON.parse(JSON.stringify(e.body)).map((e, t) => ({
				...e,
				absoluteIndex: t
			})), On.value = JSON.parse(JSON.stringify(e.body)).map((e, t) => ({
				...e,
				absoluteIndex: t
			})), G.value = JSON.parse(JSON.stringify(e.header)).map((e, t) => ({
				average: Object.hasOwn(e, "average") ? e.average : !1,
				decimals: Object.hasOwn(e, "decimals") ? e.decimals : 0,
				isMultiselect: Object.hasOwn(e, "isMultiselect") ? e.isMultiselect : !1,
				isPercentage: Object.hasOwn(e, "isPercentage") ? e.isPercentage : !1,
				isSearch: Object.hasOwn(e, "isSearch") ? e.isSearch : !1,
				isSort: Object.hasOwn(e, "isSort") ? e.isSort : !1,
				name: e.name,
				percentageTo: Object.hasOwn(e, "percentageTo") ? e.percentageTo : void 0,
				prefix: Object.hasOwn(e, "prefix") ? e.prefix : "",
				rangeFilter: Object.hasOwn(e, "rangeFilter") ? e.rangeFilter : !1,
				suffix: Object.hasOwn(e, "suffix") ? e.suffix : "",
				sum: Object.hasOwn(e, "sum") ? e.sum : !1,
				type: e.type,
				index: t
			})), k.value = {
				col: void 0,
				rows: []
			}, Cn.value = void 0, B.value = T.config.rowsPerPage ? T.config.rowsPerPage : 25, I.value = {}, M.value = {}, xn.value = {}, N.value = {}, P.value = {}, L.value = {}, F.value = {}, R.value = {}, z.value = {}, H.value = !1, _n.value = void 0, V.value = void 0, wn.value = void 0, U.value = !1, k.value.col = void 0, k.value.rows = [], yr(vr, async () => {
				Or(), await de(), bn.value = !1;
			});
		}, {
			immediate: !0,
			deep: !0
		});
		let Ar = b(null);
		return ge(yn, (e) => {
			e && Ar.value && Ar.value.focus();
		}), ge(H, (e) => {
			e ? kr.value && kr.value.open() : kr.value && kr.value.close();
		}), (t, n) => (y(), m("div", {
			class: "vue-data-ui-component vue-ui-table-main",
			style: v(`font-family: ${K.value.fontFamily}`)
		}, [
			K.value.style.exportMenu.show ? (y(), m("div", {
				key: 0,
				class: "vue-ui-table-export-hub",
				style: v({ top: kn.value + "px" })
			}, [h("button", {
				onClick: n[0] ||= (e) => yn.value = !yn.value,
				innerHTML: X.value.export,
				style: v(`background:${K.value.style.exportMenu.backgroundColor};color:${K.value.style.exportMenu.color};cursor:${q.value ? "pointer" : "default"}`)
			}, null, 12, xe), h("div", {
				class: "vue-ui-table-export-hub-dropdown",
				"data-is-open": yn.value || "false",
				style: v(`background:${K.value.style.exportMenu.backgroundColor};color:${K.value.style.exportMenu.color}`)
			}, [
				n[24] ||= h("b", { class: "vue-ui-table-export-hub-title" }, " Export ", -1),
				h("button", {
					class: "close-dropdown",
					onClick: n[1] ||= (e) => yn.value = !1,
					style: v(`background:${K.value.style.closeButtons.backgroundColor};color:${K.value.style.closeButtons.color};border-radius:${K.value.style.closeButtons.borderRadius}`)
				}, " ✖ ", 4),
				h("div", Ce, [
					h("div", we, [h("div", Te, S(K.value.translations.exportAllLabel) + " (" + S(W.value.length) + ") ", 1), h("button", {
						id: "exportAll",
						onClick: n[2] ||= (e) => $n("all"),
						style: v(`background:${K.value.style.exportMenu.buttons.backgroundColor};color:${K.value.style.exportMenu.buttons.color};cursor:${q.value ? "pointer" : "default"}`)
					}, [h("div", { innerHTML: X.value.fileDownload }, null, 8, Ee), h("span", null, S(K.value.translations.exportAllButton), 1)], 4)]),
					h("div", De, [h("div", Oe, S(K.value.translations.exportPageLabel), 1), h("button", {
						id: "exportPage",
						onClick: n[3] ||= (e) => $n("page"),
						style: v(`background:${K.value.style.exportMenu.buttons.backgroundColor};color:${K.value.style.exportMenu.buttons.color};cursor:${q.value ? "pointer" : "default"}`)
					}, [h("div", { innerHTML: X.value.fileDownload }, null, 8, ke), h("span", null, S(K.value.translations.exportPageButton), 1)], 4)]),
					h("div", Ae, [h("label", je, [h("span", Me, S(K.value.translations.filename), 1), w(h("input", {
						name: "filename",
						ref_key: "filenameInputRef",
						ref: Ar,
						onKeydown: n[4] ||= ve(ye(() => {}, ["stop"]), ["space"]),
						pattern: ".*",
						class: "vue-ui-table-dialog-input",
						type: "text",
						"onUpdate:modelValue": n[5] ||= (e) => Dn.value = e
					}, null, 544), [[he, Dn.value]])]), Dn.value ? (y(), m("button", {
						key: 0,
						class: "vue-ui-table-dialog-field-button",
						onClick: n[6] ||= (e) => Dn.value = "",
						style: v({ cursor: q.value ? "pointer" : "default" })
					}, [le(u, {
						name: "close",
						stroke: K.value.style.exportMenu.color,
						size: 18
					}, null, 8, ["stroke"])], 4)) : p("", !0)])
				])
			], 12, Se)], 4)) : p("", !0),
			h("div", {
				class: "vue-ui-table__wrapper",
				style: v(`max-height:${K.value.maxHeight}px`),
				ref_key: "tableWrapper",
				ref: mr
			}, [h("table", Ne, [
				K.value.style.title.text ? (y(), m("caption", {
					key: 0,
					class: "vue-ui-table__caption",
					ref_key: "tableCaption",
					ref: fn,
					style: v({
						textAlign: K.value.style.title.textAlign,
						paddingLeft: K.value.style.title.paddingLeft + "px",
						paddingRight: K.value.style.title.paddingRight + "px",
						backgroundColor: K.value.style.title.backgroundColor,
						boxShadow: `${K.value.style.title.backgroundColor} -1px 0px 0px 0px`
					})
				}, [h("span", { style: v({
					fontSize: K.value.style.title.fontSize + "px",
					fontWeight: K.value.style.title.bold ? "bold" : "normal",
					color: K.value.style.title.color
				}) }, S(K.value.style.title.text), 5), K.value.style.title.subtitle.text ? (y(), m(d, { key: 0 }, [n[25] ||= h("br", null, null, -1), h("span", { style: v({
					fontSize: K.value.style.title.subtitle.fontSize,
					fontWeight: K.value.style.title.subtitle.bold ? "bold" : "normal",
					color: K.value.style.title.subtitle.color
				}) }, S(K.value.style.title.subtitle.text), 5)], 64)) : p("", !0)], 4)) : p("", !0),
				h("thead", {
					id: "tableHead",
					class: "vue-ui-table__head",
					style: v({
						background: K.value.style.th.backgroundColor,
						boxShadow: `-1px 0 0 ${K.value.style.th.backgroundColor}`,
						top: kn.value - 3 + "px"
					})
				}, [
					h("tr", null, [n[26] ||= h("th", { class: "invisible-cell" }, null, -1), (y(!0), m(d, null, x(G.value, (e, t) => (y(), m("th", {
						key: `thead_${t}`,
						style: v(`overflow: visible;background:${K.value.style.th.backgroundColor};color:${K.value.style.th.color};outline:${K.value.style.th.outline}`),
						class: _({ "th-has-nan": N.value[t] })
					}, [Gn.value && ([O.value.TEXT, O.value.DATE].includes(e.type) || e.isPercentage) ? (y(), m("span", Pe, [g(S(e.name) + " ", 1), e.isPercentage ? (y(), m("span", Fe, " / " + S(e.percentageTo), 1)) : p("", !0)])) : (y(), m("span", Ie, S(e.name), 1))], 6))), 128))]),
					Gn.value ? (y(), m(d, { key: 0 }, [
						h("tr", null, [n[27] ||= h("th", { class: "invisible-cell" }, null, -1), (y(!0), m(d, null, x(G.value, (e, t) => (y(), m("th", {
							key: `thead_${t}`,
							class: _({
								"th-numeric": !0,
								"th-has-nan": N.value[t]
							}),
							style: v(`background:${K.value.style.th.backgroundColor};color:${K.value.style.th.color};outline:${K.value.style.th.outline}`)
						}, [e.sum && !N.value[t] ? (y(), m("span", Le, [
							h("span", {
								innerHTML: X.value.sum,
								style: {
									"margin-bottom": "-4px",
									"margin-right": "3px"
								}
							}, null, 8, Re),
							g(" " + S(C(s)({
								p: e.prefix,
								v: Number(sr(t)),
								s: e.suffix,
								r: e.decimals
							})) + " ", 1),
							I.value[t] && e.percentageTo && !e.isPercentage ? (y(), m("span", ze, " (" + S(isNaN(sr(t) / sr(I.value[t].referenceIndex)) ? "-" : C(s)({
								v: sr(t) / sr(I.value[t].referenceIndex) * 100,
								s: "%",
								r: e.decimals
							})) + ") ", 1)) : p("", !0)
						])) : p("", !0)], 6))), 128))]),
						h("tr", null, [n[28] ||= h("th", { class: "invisible-cell" }, null, -1), (y(!0), m(d, null, x(G.value, (e, t) => (y(), m("th", {
							key: `thead_${t}`,
							class: _({
								"th-numeric": !0,
								"th-has-nan": N.value[t]
							}),
							style: v(`background:${K.value.style.th.backgroundColor};color:${K.value.style.th.color};outline:${K.value.style.th.outline}`)
						}, [e.average && !N.value[t] ? (y(), m("span", Be, " ~ " + S(isNaN(ar(t)) ? "" : C(s)({
							p: e.prefix,
							v: Number(ar(t)),
							s: e.suffix,
							r: e.decimals
						})), 1)) : p("", !0)], 6))), 128))]),
						h("tr", null, [n[31] ||= h("th", { class: "invisible-cell" }, null, -1), (y(!0), m(d, null, x(G.value, (e, t) => (y(), m("th", {
							key: `thead_${t}`,
							class: _({ "th-has-nan": N.value[t] }),
							style: v(`background:${K.value.style.th.backgroundColor};color:${K.value.style.th.color};outline:${K.value.style.th.outline}`)
						}, [h("div", Ve, [
							e.type === O.value.DATE && M.value[t] ? (y(), m("div", He, [h("div", Ue, [h("div", We, [h("label", { for: `from_${t}` }, S(K.value.translations.from), 9, Ge), w(h("input", {
								id: `from_${t}`,
								type: "date",
								"onUpdate:modelValue": (e) => M.value[t].from = e,
								onInput: (e) => {
									Q(), Tr(t);
								},
								style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border}`)
							}, null, 44, Ke), [[he, M.value[t].from]])]), h("div", qe, [h("label", { for: `to_${t}` }, S(K.value.translations.to), 9, Je), w(h("input", {
								id: `to_${t}`,
								type: "date",
								"onUpdate:modelValue": (e) => M.value[t].to = e,
								onInput: (e) => {
									Q(), Tr(t);
								},
								style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border}`)
							}, null, 44, Ye), [[he, M.value[t].to]])])]), h("div", Xe, [e.isSort ? (y(), m("button", {
								key: 0,
								onClick: (e) => Er(t, e),
								class: _({ "th-button-active": [O.value.DESC, O.value.ASC].includes(z.value[t]) }),
								style: v(`cursor:${q.value ? "pointer" : "default"}; background:${[O.value.DESC, O.value.ASC].includes(z.value[t]) ? "" : K.value.style.th.buttons.filter.inactive.backgroundColor};color:${[O.value.DESC, O.value.ASC].includes(z.value[t]) ? "" : K.value.style.th.buttons.filter.inactive.color}`)
							}, [z.value[t] === O.value.ASC ? (y(), m("span", {
								key: 0,
								innerHTML: [O.value.DATE].includes(e.type) ? X.value.sort09 : X.value.sortAZ
							}, null, 8, Qe)) : z.value[t] === O.value.DESC ? (y(), m("span", {
								key: 1,
								innerHTML: [O.value.DATE].includes(e.type) ? X.value.sort90 : X.value.sortZA
							}, null, 8, $e)) : (y(), m("span", {
								key: 2,
								innerHTML: X.value.arrowSort
							}, null, 8, et))], 14, Ze)) : p("", !0), h("button", {
								onClick: (n) => {
									br(t), xr(t, e, n);
								},
								disabled: !xn.value[t] && dr(t, e),
								class: "th-reset",
								style: v({ cursor: q.value ? "pointer" : "default" })
							}, " ✖ ", 12, tt)])])) : p("", !0),
							e.isSearch ? w((y(), m("input", {
								key: 1,
								placeholder: K.value.translations.inputPlaceholder,
								"onUpdate:modelValue": (e) => R.value[t] = e,
								onInput: n[7] ||= (e) => tr(Q, 400),
								name: `search_${t}`,
								style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border}`)
							}, null, 44, nt)), [[he, R.value[t]]]) : p("", !0),
							!N.value[t] && e.isSort && e.type !== O.value.DATE ? (y(), m("button", {
								key: 2,
								onClick: (e) => Er(t, e),
								class: _({ "th-button-active": [O.value.DESC, O.value.ASC].includes(z.value[t]) }),
								style: v(`cursor:${q.value ? "pointer" : "default"};background:${[O.value.DESC, O.value.ASC].includes(z.value[t]) ? "" : K.value.style.th.buttons.filter.inactive.backgroundColor};color:${[O.value.DESC, O.value.ASC].includes(z.value[t]) ? "" : K.value.style.th.buttons.filter.inactive.color}`)
							}, [z.value[t] === O.value.ASC ? (y(), m("span", {
								key: 0,
								innerHTML: [O.value.NUMERIC].includes(e.type) ? X.value.sort09 : X.value.sortZA
							}, null, 8, it)) : z.value[t] === O.value.DESC ? (y(), m("span", {
								key: 1,
								innerHTML: [O.value.NUMERIC].includes(e.type) ? X.value.sort90 : X.value.sortAZ
							}, null, 8, at)) : (y(), m("span", {
								key: 2,
								innerHTML: X.value.arrowSort
							}, null, 8, ot))], 14, rt)) : p("", !0),
							e.isMultiselect ? (y(), m("button", {
								key: 3,
								onClick: (n) => Dr(t, e, n),
								innerHTML: X.value.filter,
								class: _({ "th-button-active": F.value[t] && F.value[t].length !== $(t).length }),
								style: v(`cursor:${q.value ? "pointer" : "default"};background:${F.value[t] && F.value[t].length !== $(t).length ? "" : K.value.style.th.buttons.filter.inactive.backgroundColor};color:${F.value[t] && F.value[t].length !== $(t).length ? "" : K.value.style.th.buttons.filter.inactive.color}`)
							}, null, 14, st)) : p("", !0),
							k.value.col === t && Wn.value ? (y(), m("button", {
								key: 4,
								onClick: n[8] ||= (e) => H.value = !H.value,
								innerHTML: X.value.chart,
								class: _({ "th-button-active": H.value }),
								style: v(`cursor:${q.value ? "pointer" : "default"};background:${H.value ? "" : K.value.style.th.buttons.filter.inactive.backgroundColor};color:${H.value ? "" : K.value.style.th.buttons.filter.inactive.color}`)
							}, null, 14, ct)) : p("", !0),
							e.rangeFilter && L.value[t] && !N.value[t] ? (y(), m("div", lt, [
								h("label", { for: `rangeMin${t}` }, [...n[29] ||= [
									h("span", { style: { color: "grey" } }, "ᒥ", -1),
									g(" min ", -1),
									h("span", { style: { color: "grey" } }, "ᒣ", -1)
								]], 8, ut),
								w(h("input", {
									type: "number",
									id: `rangeMin${t}`,
									max: P.value[t].max,
									min: P.value[t].min,
									"onUpdate:modelValue": (e) => L.value[t].min = e,
									onInput: n[9] ||= (e) => tr(Q, 400),
									style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border}`)
								}, null, 44, dt), [[
									he,
									L.value[t].min,
									void 0,
									{ number: !0 }
								]]),
								w(h("input", {
									type: "number",
									id: `rangeMax${t}`,
									max: P.value[t].max,
									min: P.value[t].min,
									"onUpdate:modelValue": (e) => L.value[t].max = e,
									onInput: n[10] ||= (e) => tr(Q, 400),
									style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border}`)
								}, null, 44, ft), [[
									he,
									L.value[t].max,
									void 0,
									{ number: !0 }
								]]),
								h("label", { for: `rangeMax${t}` }, [...n[30] ||= [
									h("span", { style: { color: "grey" } }, "ᒪ", -1),
									g(" max ", -1),
									h("span", { style: { color: "grey" } }, "ᒧ", -1)
								]], 8, pt)
							])) : p("", !0),
							Qn(t, e) ? (y(), m("button", {
								key: 6,
								onClick: (n) => xr(t, e, n),
								disabled: dr(t, e),
								class: "th-reset",
								style: v({ cursor: q.value ? "pointer" : "default" })
							}, " ✖ ", 12, mt)) : p("", !0),
							e.isMultiselect ? (y(), m("div", {
								key: 7,
								class: "th-dropdown",
								"data-is-open": "false",
								id: `th_dropdown_${t}`,
								style: v(`background:${K.value.style.dropdowns.backgroundColor};color:${K.value.style.dropdowns.color}`)
							}, [h("button", {
								class: "close-dropdown",
								onClick: (n) => Dr(t, e, n),
								style: v(`cursor:${q.value ? "pointer" : "default"};background:${K.value.style.closeButtons.backgroundColor};color:${K.value.style.closeButtons.color}`)
							}, " ✖ ", 12, gt), (y(!0), m(d, null, x($(t), (e, n) => (y(), m("span", {
								class: "th-option",
								key: `th_option_${t}_${n}`,
								onClick: (n) => wr(e, t),
								onKeyup: [ve((n) => wr(e, t), ["enter"]), ve((n) => wr(e, t), ["space"])],
								style: v(`opacity:${lr(e, t) ? 1 : .5}`),
								tabindex: "0"
							}, [lr(e, t) ? (y(), m("span", {
								key: 0,
								style: v(`color:${K.value.style.dropdowns.icons.selected.color};margin-right:5px`),
								class: "th-icon-green"
							}, S(K.value.style.dropdowns.icons.selected.unicode), 5)) : (y(), m("span", {
								key: 1,
								style: v(`color:${K.value.style.dropdowns.icons.unselected.color};margin-right:5px`),
								class: "th-icon-red"
							}, S(K.value.style.dropdowns.icons.unselected.unicode), 5)), h("span", null, S(e), 1)], 44, _t))), 128))], 12, ht)) : p("", !0)
						])], 6))), 128))])
					], 64)) : p("", !0),
					h("tr", null, [n[32] ||= h("th", { class: "invisible-cell" }, null, -1), (y(!0), m(d, null, x(G.value, (e, t) => (y(), m("th", {
						key: `col_selector_${t}`,
						class: _({
							"vue-ui-table-col-selector": !N.value[t],
							"th-has-nan": N.value[t]
						}),
						style: v(`background:${t === Cn.value && !N.value[t] ? K.value.style.th.selected.backgroundColor : K.value.style.th.backgroundColor};color:${t === Cn.value && !N.value[t] ? K.value.style.th.selected.color : K.value.style.th.color};outline:${K.value.style.th.outline}`)
					}, [N.value[t] ? p("", !0) : (y(), m("div", {
						key: 0,
						innerHTML: G.value[t].type === O.value.NUMERIC ? X.value.chevronDown : "",
						class: _({ "col-selector": G.value[t].type === O.value.NUMERIC }),
						tabindex: "0",
						style: v({ cursor: q.value ? "pointer" : "default" }),
						onClick: ye((e) => Cr(t), ["stop"]),
						onKeyup: ve((e) => Cr(t), ["enter"])
					}, null, 46, vt))], 6))), 128))])
				], 4),
				h("tbody", {
					onClick: er,
					onKeydown: n[11] ||= (e) => gr(e)
				}, [(y(!0), m(d, null, x(Y.value, (t, n) => (y(), m("tr", {
					key: `tbody_${n}`,
					"data-row": n % 2 == 0 ? "odd" : "even",
					class: _(`tr_${C(E)}`),
					style: v(`${n % 2 == 0 ? `background:${K.value.style.rows.odd.backgroundColor};color:${K.value.style.rows.odd.color}` : `background:${K.value.style.rows.even.backgroundColor};color:${K.value.style.rows.even.color}`}`)
				}, [h("td", {
					class: "vue-ui-table-td-iteration",
					"data-row": n % 2 == 0 ? "odd" : "even",
					style: v({ outline: K.value.style.rows.outline })
				}, S(t.absoluteIndex + 1), 13, bt), (y(!0), m(d, null, x(t.td, (r, i) => (y(), m("td", {
					"data-row": n % 2 == 0 ? "odd" : "even",
					key: `td_${n}_${i}`,
					style: v(ur(r) || e.dataset.header[i].type === O.value.DATE ? `text-align:right;font-variant-numeric: tabular-nums;outline:${K.value.style.rows.outline}` : `outline:${K.value.style.rows.outline}`),
					onClick: (t) => Sr({
						td: r,
						rowIndex: n,
						colIndex: i,
						headerType: e.dataset.header[i].type,
						event: t
					}),
					onKeyup: [ve((t) => Sr({
						td: r,
						rowIndex: n,
						colIndex: i,
						headerType: e.dataset.header[i].type,
						event: t
					}), ["enter"]), ve((t) => Sr({
						td: r,
						rowIndex: n,
						colIndex: i,
						headerType: e.dataset.header[i].type,
						event: t
					}), ["space"])],
					class: _({
						"td-numeric": e.dataset.header[i].type === O.value.NUMERIC,
						"td-focusable": !0,
						"td-has-nan": N.value[i]
					}),
					id: `cell_${n}_${i}_${C(E)}`,
					tabindex: "0"
				}, [t.meta && t.meta.markerIndices.includes(i) && t.meta.unicodeIcon ? (y(), m("span", {
					key: 0,
					style: v(`color:${t.meta.color};margin-right:3px`),
					innerHTML: t.meta.unicodeIcon
				}, null, 12, St)) : p("", !0), e.dataset.header[i].type === O.value.DATE ? (y(), m("span", Ct, S(e.dataset.header[i].prefix) + " " + S(new Date(r).toLocaleString().slice(0, 10)) + " " + S(e.dataset.header[i].suffix), 1)) : e.dataset.header[i].isPercentage ? (y(), m("span", wt, S(C(s)({
					v: Number(r * 100),
					s: "%",
					r: e.dataset.header[i].decimals
				})), 1)) : I.value[i] && e.dataset.header[i].percentageTo && !e.dataset.header[i].isPercentage ? (y(), m("span", {
					key: 3,
					class: _({ "td-nan": isNaN(Number(r)) })
				}, S(isNaN(Number(r)) ? `${r} is not ${O.value.NUMERIC}` : C(s)({
					p: e.dataset.header[i].prefix,
					v: Number(r),
					s: e.dataset.header[i].suffix,
					r: e.dataset.header[i].decimals
				})) + " (" + S(isNaN(Number(r)) ? "" : C(s)({
					v: Number(r / sr(I.value[i].referenceIndex) * 100),
					s: "%",
					r: e.dataset.header[i].decimals
				})) + ") ", 3)) : e.dataset.header[i].type === O.value.NUMERIC ? (y(), m("span", {
					key: 4,
					class: _({ "td-nan": isNaN(Number(r)) })
				}, S(isNaN(Number(r)) ? `${r} is not ${O.value.NUMERIC}` : C(s)({
					p: e.dataset.header[i].prefix,
					v: Number(r.toFixed(e.dataset.header[i].decimals)),
					s: e.dataset.header[i].suffix,
					r: e.dataset.header[i].decimals
				})), 3)) : (y(), m("span", Tt, S(e.dataset.header[i].prefix) + " " + S(r) + " " + S(e.dataset.header[i].suffix), 1))], 46, xt))), 128))], 14, yt))), 128))], 32)
			])], 4),
			h("div", {
				class: _({
					"td-selector-info": !0,
					"td-selector-info--active": k.value.col !== void 0 && k.value.rows.length
				}),
				style: v(`background:${K.value.style.infoBar.backgroundColor};color:${K.value.style.infoBar.color}`)
			}, [k.value.col !== void 0 && k.value.rows.length ? (y(), m(d, { key: 0 }, [
				h("div", {
					innerHTML: X.value.table,
					class: "td-selector-icon"
				}, null, 8, Et),
				h("span", null, [
					h("b", null, [g(S(e.dataset.header[k.value.col].name) + " ", 1), e.dataset.header[k.value.col].isPercentage ? (y(), m("span", Dt, " / " + S(e.dataset.header[I.value[k.value.col].referenceIndex].name), 1)) : p("", !0)]),
					h("span", Ot, [g(S(K.value.translations.nb) + " : ", 1), h("b", kt, S(k.value.rows.length), 1)]),
					h("span", At, [
						g(S(K.value.translations.sum) + " : ", 1),
						e.dataset.header[k.value.col].isPercentage ? (y(), m("b", jt, S(qn.value.sumPercentage), 1)) : (y(), m("b", Mt, S(e.dataset.header[k.value.col].prefix) + " " + S(qn.value.sumRegular) + " " + S(e.dataset.header[k.value.col].suffix), 1)),
						e.dataset.header[k.value.col].isPercentage ? (y(), m("b", Nt, "%")) : p("", !0)
					]),
					h("span", Pt, [
						g(S(K.value.translations.average) + " : ", 1),
						e.dataset.header[k.value.col].isPercentage ? (y(), m("b", Ft, S(qn.value.averagePercentage), 1)) : (y(), m("b", It, S(e.dataset.header[k.value.col].prefix) + " " + S(qn.value.averageRegular) + " " + S(e.dataset.header[k.value.col].suffix), 1)),
						e.dataset.header[k.value.col].isPercentage ? (y(), m("b", Lt, "%")) : p("", !0)
					])
				]),
				h("button", {
					onClick: Z,
					class: "td-selector-info-reset",
					style: v(`cursor:${q.value ? "pointer" : "default"};background:${K.value.style.closeButtons.backgroundColor};color:${K.value.style.closeButtons.color};border-radius:${K.value.style.closeButtons.borderRadius}`)
				}, " ✖ ", 4)
			], 64)) : p("", !0)], 6),
			W.value.length > 10 ? (y(), m("div", Rt, [g(S(K.value.translations.totalRows) + " : " + S(e.dataset.body.length) + " | " + S(K.value.translations.paginatorLabel) + " : ", 1), W.value.length > 10 ? w((y(), m("select", {
				key: 0,
				id: "paginatorSelector",
				"onUpdate:modelValue": n[12] ||= (e) => B.value = e,
				onChange: n[13] ||= (e) => {
					Z(), pr();
				},
				style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border}`)
			}, [(y(!0), m(d, null, x(Sn.value, (t, n) => (y(), m(d, null, [W.value.length > t || e.dataset.body.length === t ? (y(), m("option", { key: `paginator_option_${n}` }, S(t), 1)) : p("", !0)], 64))), 256))], 36)), [[
				me,
				B.value,
				void 0,
				{ number: !0 }
			]]) : p("", !0)])) : p("", !0),
			B.value >= 250 ? (y(), m("div", zt, [h("span", { innerHTML: X.value.warning }, null, 8, Bt), g(S(K.value.translations.sizeWarning), 1)])) : p("", !0),
			J.value.length > 1 && J.value.length <= 10 ? (y(), m("div", {
				key: 3,
				class: "vue-ui-table-navigation-indicator",
				style: v(`background:${K.value.style.pagination.navigationIndicator.backgroundColor};width:calc(${A.value / (J.value.length - 1) * 100}%)`)
			}, null, 4)) : p("", !0),
			J.value.length > 1 ? (y(), m("div", Vt, [
				h("button", {
					class: "vue-ui-table-navigation",
					onClick: n[14] ||= ye((e) => hr("previous"), ["stop"]),
					innerHTML: X.value.chevronLeft,
					disabled: A.value === 0,
					style: v(`cursor:${q.value ? "pointer" : "default"};background:${K.value.style.pagination.buttons.backgroundColor};color:${K.value.style.pagination.buttons.color};opacity:${A.value === 0 ? K.value.style.pagination.buttons.opacityDisabled : 1}`)
				}, null, 12, Ht),
				J.value.length > 3 ? (y(), m(d, { key: 0 }, [
					h("button", {
						class: "vue-ui-table-navigation",
						onClick: n[15] ||= ye((e) => hr(1), ["stop"]),
						disabled: A.value === 0,
						style: v(`cursor:${q.value ? "pointer" : "default"};background:${K.value.style.pagination.buttons.backgroundColor};color:${K.value.style.pagination.buttons.color};opacity:${A.value === 0 ? K.value.style.pagination.buttons.opacityDisabled : 1}`)
					}, " 1 ", 12, Ut),
					J.value.length > 10 ? (y(), m("div", {
						key: 0,
						class: "vue-ui-table-page-scroller-wrapper",
						style: v({ cursor: q.value ? "pointer" : "default" })
					}, [h("label", Wt, S(K.value.translations.page) + " " + S(A.value + 1) + " / " + S(J.value.length), 1), h("input", {
						class: "vue-ui-table-page-scroller",
						id: "pageScroller",
						type: "range",
						step: "1",
						min: 0,
						max: J.value.length - 1,
						onInput: n[16] ||= (e) => Or(e),
						value: A.value,
						style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border};accent-color:${K.value.style.inputs.accentColor}`)
					}, null, 44, Gt)], 4)) : (y(), m("span", Kt, S(K.value.translations.page) + " " + S(A.value + 1) + " / " + S(J.value.length), 1)),
					h("button", {
						class: "vue-ui-table-navigation",
						onClick: n[17] ||= ye((e) => hr(J.value.length), ["stop"]),
						disabled: A.value === J.value.length - 1,
						style: v(`cursor:${q.value ? "pointer" : "default"};background:${K.value.style.pagination.buttons.backgroundColor};color:${K.value.style.pagination.buttons.color};opacity:${A.value === J.value.length - 1 ? K.value.style.pagination.buttons.opacityDisabled : 1}`)
					}, S(J.value.length), 13, qt)
				], 64)) : (y(), m(d, { key: 1 }, [g(S(K.value.translations.page) + " " + S(A.value + 1) + " / " + S(J.value.length), 1)], 64)),
				h("button", {
					class: "vue-ui-table-navigation",
					onClick: n[18] ||= ye((e) => hr("next"), ["stop"]),
					innerHTML: X.value.chevronRight,
					disabled: A.value === J.value.length - 1,
					style: v(`cursor:${q.value ? "pointer" : "default"};background:${K.value.style.pagination.buttons.backgroundColor};color:${K.value.style.pagination.buttons.color};opacity:${A.value === J.value.length - 1 ? K.value.style.pagination.buttons.opacityDisabled : 1}`)
				}, null, 12, Jt)
			])) : p("", !0),
			le(C(ln), {
				ref_key: "chartModal",
				ref: kr,
				backgroundColor: K.value.style.chart.modal.backgroundColor,
				headerColor: K.value.style.chart.modal.color,
				color: K.value.style.chart.modal.color,
				forcedHeight: 500,
				isCursorPointer: q.value,
				withPadding: "",
				withFullWidth: "",
				noLayerUpdate: "",
				onClose: n[23] ||= (e) => H.value = !1,
				onSize: Yn
			}, {
				before: _e(() => [h("div", Yt, [
					Un.value.length ? (y(), m("button", {
						key: 0,
						onClick: n[19] ||= (e) => U.value = !0,
						innerHTML: X.value.donut,
						class: _({ "is-active-chart": D.value.type === O.value.DONUT || U.value }),
						style: v(`cursor:${q.value ? "pointer" : "default"};background:${D.value.type === O.value.DONUT || U.value ? K.value.style.chart.modal.buttons.selected.backgroundColor : K.value.style.chart.modal.buttons.unselected.backgroundColor};color:${D.value.type === O.value.DONUT || U.value ? K.value.style.chart.modal.buttons.selected.color : K.value.style.chart.modal.buttons.unselected.color}`)
					}, null, 14, Xt)) : p("", !0),
					h("button", {
						onClick: n[20] ||= (e) => {
							D.value.type = O.value.LINE, U.value = !1;
						},
						innerHTML: X.value.chart,
						class: _({ "is-active-chart": D.value.type === O.value.LINE && !U.value }),
						style: v(`cursor:${q.value ? "pointer" : "default"};background:${D.value.type === O.value.LINE && !U.value ? K.value.style.chart.modal.buttons.selected.backgroundColor : K.value.style.chart.modal.buttons.unselected.backgroundColor};color:${D.value.type === O.value.LINE && !U.value ? K.value.style.chart.modal.buttons.selected.color : K.value.style.chart.modal.buttons.unselected.color}`)
					}, null, 14, Zt),
					h("button", {
						onClick: n[21] ||= (e) => {
							D.value.type = O.value.BAR, U.value = !1;
						},
						innerHTML: X.value.bar,
						class: _({ "is-active-chart": D.value.type === O.value.BAR && !U.value }),
						style: v(`cursor:${q.value ? "pointer" : "default"};background:${D.value.type === O.value.BAR && !U.value ? K.value.style.chart.modal.buttons.selected.backgroundColor : K.value.style.chart.modal.buttons.unselected.backgroundColor};color:${D.value.type === O.value.BAR && !U.value ? K.value.style.chart.modal.buttons.selected.color : K.value.style.chart.modal.buttons.unselected.color}`)
					}, null, 14, Qt)
				])]),
				content: _e(() => [h("div", $t, [
					U.value && Un.value.length ? (y(), m("div", {
						key: 0,
						style: v(`background:${K.value.style.chart.modal.backgroundColor};color:${K.value.style.chart.modal.color}`)
					}, [h("fieldset", en, [
						h("legend", null, S(K.value.translations.chooseCategoryColumn), 1),
						h("div", tn, [(y(!0), m(d, null, x(Un.value, (e, t) => (y(), m("div", {
							key: `donut_radio_${t}`,
							class: "vue-ui-table-fieldset-option"
						}, [h("input", {
							type: "radio",
							name: e.name,
							id: e.name,
							checked: V.value && e.name === V.value.name,
							onInput: (e) => V.value = Un.value[t],
							style: v(`background:${K.value.style.inputs.backgroundColor};color:${K.value.style.inputs.color};border:${K.value.style.inputs.border};accent-color:${K.value.style.inputs.accentColor}`)
						}, null, 44, nn), h("label", { for: e.name }, S(e.name), 9, rn)]))), 128))]),
						h("button", {
							class: "vue-ui-table-generate-donut",
							disabled: !V.value,
							onClick: Zn,
							style: v(`cursor:${q.value ? "pointer" : "default"};background:${K.value.style.chart.modal.buttons.selected.backgroundColor};color:${K.value.style.chart.modal.buttons.selected.color}`)
						}, [h("div", {
							style: { "margin-bottom": "-3px" },
							innerHTML: X.value.donut
						}, null, 8, on), g(" " + S(K.value.translations.makeDonut), 1)], 12, an)
					])], 4)) : p("", !0),
					[O.value.BAR, O.value.LINE].includes(D.value.type) && !U.value ? (y(), m(d, { key: 1 }, [
						Bn.value.length > 1 ? (y(), m("label", {
							key: 0,
							style: v({ color: K.value.style.chart.modal.color })
						}, [g(S(K.value.translations.xAxisLabels) + " ", 1), w(h("select", { "onUpdate:modelValue": n[22] ||= (e) => En.value = e }, [(y(!0), m(d, null, x(Bn.value, (e) => (y(), m("option", null, S(e), 1))), 256))], 512), [[me, En.value]])], 4)) : p("", !0),
						h("div", sn, [D.value.type === O.value.LINE ? (y(), ce(re, {
							key: `chart_line_${Tn.value}`,
							dataset: Xn.value.xyDatasetLine,
							config: Xn.value.xyConfig
						}, null, 8, ["dataset", "config"])) : p("", !0), D.value.type === O.value.BAR ? (y(), ce(re, {
							key: `chart_bar_${Tn.value}`,
							dataset: Xn.value.xyDatasetBar,
							config: Xn.value.xyConfig
						}, null, 8, ["dataset", "config"])) : p("", !0)]),
						k.value.rows.length >= 2 ? (y(), m("div", {
							key: 1,
							class: "chart-trend",
							style: v(`color:${K.value.style.chart.modal.color}`)
						}, [n[33] ||= h("span", null, "---", -1), g(" Trend: " + S(C(s)({
							v: Xn.value.progression.trend * 100,
							s: "%",
							r: 1
						})), 1)], 4)) : p("", !0)
					], 64)) : p("", !0),
					[O.value.DONUT].includes(D.value.type) && !U.value ? (y(), m("div", cn, [le(se, {
						dataset: _n.value,
						config: Xn.value.donutConfig
					}, null, 8, ["dataset", "config"])])) : p("", !0)
				])]),
				_: 1
			}, 8, [
				"backgroundColor",
				"headerColor",
				"color",
				"isCursorPointer"
			])
		], 4));
	}
}, [["__scopeId", "data-v-4acdb715"]]);
//#endregion
export { be as n, ln as t };
