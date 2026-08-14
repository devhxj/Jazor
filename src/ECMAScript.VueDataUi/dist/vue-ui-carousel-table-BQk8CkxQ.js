import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { $ as t, Bt as n, D as r, jt as i, q as a, tt as o } from "./lib-Bttd6u5E.js";
import { n as ee, t as te } from "./useHints-Dq_w2E8B.js";
import { t as ne } from "./useConfig-DlNpz6P8.js";
import { t as re } from "./usePrinter-DN5bYhTG.js";
import { t as ie } from "./useNestedProp-vPNvh7rV.js";
import { t as ae } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as oe } from "./useUserOptionState-DK-_1ddE.js";
import { t as se } from "./usePrefersMotion-BC-CsqR1.js";
import { Fragment as s, computed as c, createBlock as ce, createCommentVNode as l, createElementBlock as u, createElementVNode as d, createSlots as le, createTextVNode as f, defineAsyncComponent as ue, guardReactiveProps as p, mergeProps as de, nextTick as fe, normalizeClass as pe, normalizeProps as m, normalizeStyle as h, onBeforeUnmount as g, onMounted as _, openBlock as v, ref as y, renderList as b, renderSlot as x, shallowRef as me, toDisplayString as S, unref as C, useSlots as he, watch as w, withCtx as T } from "vue";
//#region src/components/vue-ui-carousel-table.vue
var ge = /* @__PURE__ */ e({ default: () => E }), _e = ["id"], ve = ["aria-labelledby"], ye = ["id"], be = ["height"], xe = ["id"], Se = [
	"data-cell",
	"aria-label",
	"height"
], Ce = {
	key: 2,
	ref: "source",
	dir: "auto"
}, E = /*#__PURE__*/ ae({
	__name: "vue-ui-carousel-table",
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
	emits: ["copyAlt"],
	setup(e, { expose: ae, emit: ge }) {
		let E = ue(() => import("./vue-ui-skeleton-E6Hbh29Z.js").then((e) => e.n)), we = ue(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), { vue_ui_carousel_table: Te } = ne(), Ee = se(), D = e, De = ge, O = y(a()), Oe = y(!1), k = y(!!D.dataset), ke = he();
		_(() => {
			Ae();
		});
		let A = c({
			get: () => Pe(),
			set: (e) => e
		}), j = c(() => A.value.debug);
		_(() => {
			ke["chart-background"] && j.value && console.warn("VueUiCarouselTable does not support the #chart-background slot.");
		});
		function Ae() {
			i(D.dataset) ? o({
				componentName: "VueUiCarouselTable",
				type: "dataset",
				debug: j.value
			}) : ((!D.dataset.head || i(D.dataset.head)) && (o({
				componentName: "VueUiCarouselTable",
				type: "datasetAttribute",
				property: "head",
				debug: j.value
			}), k.value = !1), (!D.dataset.body || i(D.dataset.body)) && (o({
				componentName: "VueUiCarouselTable",
				type: "datasetAttribute",
				property: "body",
				debug: j.value
			}), k.value = !1)), Ee.value && (A.value.userOptions.buttons.animation = !1);
		}
		ee({
			config: () => A.value,
			dataset: () => D.dataset,
			component: "VueUiCarouselTable",
			rules: [te.noHint]
		});
		let je = c(() => A.value.userOptions.useCursorPointer), { userOptionsVisible: M, setUserOptionsVisibility: Me, keepUserOptionState: Ne } = oe({ config: A.value });
		function Pe() {
			return ie({
				userConfig: D.config,
				defaultConfig: Te
			});
		}
		w(() => D.config, (e) => {
			A.value = Pe(), M.value = !A.value.userOptions.showOnChartHover, Ae();
		}, { deep: !0 }), w(() => D.dataset, (e) => {
			U();
		}, { deep: !0 });
		let { isPrinting: Fe, isImaging: Ie, generatePdf: Le, generateImage: Re } = re({
			elementId: `carousel-table_${O.value}`,
			fileName: A.value.caption.text || "vue-ui-carousel-table",
			options: A.value.userOptions.print
		}), N = y({ showAnimation: A.value.animation.use }), P = y(null), ze = y(null), F = y(null), I = y(null), L = y(0), R = y(0), z = y(!1), B = y(null), V = y(null), H = y(0);
		function U() {
			B.value && (V.value = {
				elements: B.value.getElementsByTagName("tr"),
				heights: Array.from(B.value.getElementsByTagName("tr")).map((e) => e.getBoundingClientRect().height)
			});
		}
		_(U);
		let Be = c(() => !V.value || !V.value.heights.length ? 0 : Math.max(...V.value.heights) + L.value + R.value), Ve = c(() => D.dataset.body ? A.value.tbody.tr.visible <= D.dataset.body.length ? A.value.tbody.tr.visible : D.dataset.body.length : 0), He = c(() => (A.value.tbody.tr.height + A.value.tbody.tr.td.padding.top + A.value.tbody.tr.td.padding.bottom + A.value.tbody.tr.border.size * 2) * Ve.value + L.value + R.value), W = y(0), G = y(null), K = y(0), q = y(!1), Ue = y(0);
		_(() => {
			F.value && (L.value = F.value.getBoundingClientRect().height), I.value && (R.value = I.value.getBoundingClientRect().height), N.value.showAnimation && V.value && Ge();
		}), _(() => {
			if (P.value) {
				let e = P.value.querySelector("thead"), t = Array.from(B.value.querySelectorAll("tr"));
				function n() {
					let n = e.getBoundingClientRect().bottom;
					t.forEach((e) => {
						e.getBoundingClientRect().top < n ? e.style.visibility = "hidden" : e.style.visibility = "visible";
					});
				}
				P.value.addEventListener("scroll", n), n(), g(() => {
					P.value.removeEventListener("scroll", n);
				});
			}
		});
		function We(e) {
			Oe.value = e, Ue.value += 1;
		}
		function Ge() {
			Ee.value || !G.value && !q.value && (A.value.animation.type === "scroll" ? G.value = requestAnimationFrame(J) : G.value = requestAnimationFrame(qe));
		}
		function Ke() {
			if (!P.value) return !1;
			let { scrollTop: e, scrollHeight: t, clientHeight: n } = P.value;
			return e + n >= t;
		}
		function J(e) {
			q.value || (K.value ||= e, e - K.value >= A.value.animation.speedMs && (W.value += V.value.heights[H.value], (Ke() || H.value >= V.value.heights.length) && (W.value = 0, H.value = -1), H.value += 1, P.value && P.value.scrollTo({
				top: W.value,
				behavior: "smooth"
			}), K.value = e), G.value = requestAnimationFrame(J));
		}
		function qe(e) {
			if (q.value) return;
			K.value ||= e;
			let t = e - K.value, n = A.value.animation.speedMs / 4 / 1e3;
			t >= n && (W.value += n, W.value >= P.value.scrollHeight - P.value.clientHeight && (W.value = 0), P.value && P.value.scrollTo({
				top: W.value,
				behavior: "auto"
			}), K.value = e), G.value = requestAnimationFrame(qe);
		}
		function Y() {
			q.value = !0, cancelAnimationFrame(G.value), G.value = null;
		}
		g(Y);
		function X() {
			!q.value || !N.value.showAnimation || (q.value = !1, K.value = 0, Ge());
		}
		function Je() {
			A.value.animation.pauseOnHover && Y();
		}
		let Z = y(null);
		function Ye() {
			Y(), clearTimeout(Z.value);
		}
		function Xe() {
			clearTimeout(Z.value), Z.value = setTimeout(X, 1e3);
		}
		w(() => A.value.animation.use, (e) => {
			e ? (N.value.showAnimation = !0, X()) : (N.value.showAnimation = !1, Y());
		}), w(() => A.value.animation.type, (e) => {
			Y(), W.value = 0, H.value = 0, P.value.scrollTo({
				top: 0,
				behavior: "auto"
			}), X();
		});
		let Ze = c(() => A.value.responsiveBreakpoint), Q = me(null);
		_(() => {
			Q.value = new ResizeObserver((e) => {
				e.forEach((e) => {
					z.value = e.contentRect.width < Ze.value;
				}), L.value = F.value ? F.value.getBoundingClientRect().height : 0, R.value = I.value ? I.value.getBoundingClientRect().height : 0, H.value = 0, fe(() => {
					Y(), K.value = 0, W.value = 0, U(), X();
				});
			}), P.value && Q.value.observe(P.value);
		}), g(() => {
			Q.value && Q.value.disconnect();
		});
		function Qe() {
			Le();
		}
		function $e() {
			N.value.showAnimation = !N.value.showAnimation, N.value.showAnimation ? X() : Y();
		}
		function et(e = null) {
			fe(() => {
				let n = D.dataset.head.map((e, t) => [[D.dataset.body[t]]]), i = [[A.value.caption.text], [D.dataset.head.map((e) => [e])]].concat(n), a = r(i);
				e ? e(a) : t({
					csvContent: a,
					title: A.value.caption.text || "vue-ui-carousel-table"
				});
			});
		}
		async function $() {
			if (De("copyAlt", {
				config: A.value,
				dataset: D.dataset
			}), !A.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(A.value.userOptions.callbacks.altCopy({
				config: A.value,
				dataset: D.dataset
			}));
		}
		return ae({
			pauseAnimation: Y,
			resumeAnimation: X,
			toggleAnimation: $e,
			generateCsv: et,
			generatePdf: Qe,
			generateImage: Re,
			copyAlt: $
		}), (t, r) => (v(), u("div", {
			class: "vue-data-ui-component vue-ui-carousel-table",
			style: {
				position: "relative",
				overflow: "visible"
			},
			ref_key: "chartContainer",
			ref: ze,
			onMouseenter: r[5] ||= () => C(Me)(!0),
			onMouseleave: r[6] ||= () => C(Me)(!1)
		}, [
			d("div", {
				ref_key: "tableContainer",
				ref: P,
				id: `carousel-table_${O.value}`,
				style: h({
					height: C(Fe) || C(Ie) ? "auto" : `${Math.max(He.value, Be.value)}px`,
					containerType: "inline-size",
					position: "relative",
					overflow: "auto",
					fontFamily: A.value.fontFamily
				}),
				class: pe({
					"vue-ui-responsive": z.value,
					"is-playing": A.value.scrollbar.hide || !q.value && A.value.scrollbar.showOnlyOnHover
				}),
				onMouseover: r[0] ||= (e) => Je(),
				onMouseleave: r[1] ||= (e) => X(),
				onTouchstart: r[2] ||= (e) => Ye(),
				onTouchend: r[3] ||= (e) => Xe(),
				onTouchcancel: r[4] ||= (e) => Xe()
			}, [k.value ? (v(), u("table", {
				key: 0,
				class: "vue-data-ui-carousel-table",
				"aria-labelledby": `carousel-caption-${O.value}`,
				style: h({
					...A.value.style,
					border: `${A.value.border.size}px solid ${A.value.border.color}`,
					width: "100%",
					borderCollapse: "collapse",
					backgroundColor: A.value.tbody.backgroundColor
				})
			}, [
				d("caption", {
					ref_key: "caption",
					ref: F,
					class: "vue-data-ui-carousel-table-caption",
					id: `carousel-caption-${O.value}`,
					style: h({
						...A.value.caption.style,
						fontFamily: "inherit",
						position: "sticky",
						top: 0,
						zIndex: 2,
						paddingTop: A.value.caption.padding.top + "px",
						paddingRight: A.value.caption.padding.right + "px",
						paddingBottom: A.value.caption.padding.bottom + "px",
						paddingLeft: A.value.caption.padding.left + "px",
						boxShadow: z.value ? A.value.thead.tr.style.boxShadow : "none",
						minHeight: "36px",
						display: t.$slots.caption || A.value.caption.text || A.value.userOptions.show ? "" : "none"
					})
				}, [f(S(A.value.caption.text && !t.$slots.caption ? A.value.caption.text : "") + " ", 1), x(t.$slots, "caption", {}, void 0, !0)], 12, ye),
				d("thead", {
					role: "rowgroup",
					style: h({
						...A.value.thead.style,
						position: "sticky",
						top: `${t.$slots.caption || A.value.caption.text || A.value.userOptions.show ? L.value : 0}px`,
						zIndex: 1
					})
				}, [d("tr", {
					ref_key: "tableRow",
					ref: I,
					role: "row",
					style: h({
						...A.value.thead.tr.style,
						border: A.value.thead.tr.border.size ? `${A.value.thead.tr.border.size}px solid ${A.value.thead.tr.border.color}` : "none",
						boxShadow: z.value ? "none" : A.value.thead.tr.style.boxShadow
					}),
					height: `${A.value.thead.tr.height}px`
				}, [(v(!0), u(s, null, b(e.dataset.head, (e, n) => (v(), u("th", {
					role: "cell",
					key: `th_${n}`,
					id: `col-${n}`,
					scope: "col",
					style: h({
						...A.value.thead.tr.th.style,
						border: A.value.thead.tr.th.border.size ? `${A.value.thead.tr.th.border.size}px solid ${A.value.thead.tr.th.border.color}` : "none",
						paddingTop: A.value.thead.tr.th.padding.top + "px",
						paddingRight: A.value.thead.tr.th.padding.right + "px",
						paddingBottom: A.value.thead.tr.th.padding.bottom + "px",
						paddingLeft: A.value.thead.tr.th.padding.left + "px"
					})
				}, [f(S(t.$slots.th ? "" : e) + " ", 1), x(t.$slots, "th", de({ ref_for: !0 }, {
					th: e,
					colIndex: n
				}), void 0, !0)], 12, xe))), 128))], 12, be)], 4),
				e.dataset.body && e.dataset.head ? (v(), u("tbody", {
					key: 0,
					ref_key: "tbody",
					ref: B,
					"aria-live": "polite",
					style: { clipPath: "inset(0,0,0,0)" }
				}, [(v(!0), u(s, null, b(e.dataset.body, (r, i) => (v(), u("tr", { style: h({
					...A.value.tbody.tr.style,
					border: `${A.value.tbody.tr.border.size}px solid ${A.value.tbody.tr.border.color}`,
					verticalAlign: "middle"
				}) }, [(v(!0), u(s, null, b(r, (r, a) => (v(), u("td", {
					role: "cell",
					"data-cell": e.dataset.head[a] || "",
					"aria-label": `${e.dataset.head[a]}: ${r}`,
					style: h({
						...A.value.tbody.tr.td.style,
						border: `${A.value.tbody.tr.td.border.size}px solid ${A.value.tbody.tr.td.border.color}`,
						backgroundColor: C(n)(A.value.tbody.tr.td.style.backgroundColor, i % 2 == 0 && A.value.tbody.tr.td.alternateColor ? A.value.tbody.tr.td.alternateOpacity * 100 : 100),
						paddingTop: A.value.tbody.tr.td.padding.top + "px",
						paddingRight: A.value.tbody.tr.td.padding.right + "px",
						paddingBottom: A.value.tbody.tr.td.padding.bottom + "px",
						paddingLeft: A.value.tbody.tr.td.padding.left + "px",
						verticalAlign: "middle"
					}),
					height: `${A.value.tbody.tr.height}px`
				}, [f(S(t.$slots.td ? "" : r) + " ", 1), x(t.$slots, "td", de({ ref_for: !0 }, {
					td: r,
					rowIndex: i,
					colIndex: a
				}), void 0, !0)], 12, Se))), 256))], 4))), 256))], 512)) : l("", !0)
			], 12, ve)) : l("", !0)], 46, _e),
			k.value ? l("", !0) : (v(), ce(C(E), {
				key: 0,
				config: { type: "table" }
			})),
			A.value.userOptions.show && k.value && (C(Ne) || C(M)) ? (v(), ce(C(we), {
				ref: "details",
				key: `user_option_${Ue.value}`,
				backgroundColor: A.value.style.backgroundColor,
				color: A.value.style.color,
				isPrinting: C(Fe),
				isImaging: C(Ie),
				uid: O.value,
				hasTooltip: !1,
				hasPdf: A.value.userOptions.buttons.pdf,
				hasImg: A.value.userOptions.buttons.img,
				hasXls: A.value.userOptions.buttons.csv,
				hasTable: !1,
				hasLabel: !1,
				hasAnimation: A.value.userOptions.buttons.animation,
				isAnimation: !N.value.showAnimation,
				hasFullscreen: A.value.userOptions.buttons.fullscreen,
				hasAltCopy: A.value.userOptions.buttons.altCopy,
				isFullscreen: Oe.value,
				chartElement: ze.value,
				position: A.value.userOptions.position,
				titles: { ...A.value.userOptions.buttonTitles },
				zIndex: 3,
				offsetX: 12,
				callbacks: A.value.userOptions.callbacks,
				printScale: A.value.userOptions.print.scale,
				isCursorPointer: je.value,
				onGeneratePdf: Qe,
				onGenerateCsv: et,
				onGenerateImage: C(Re),
				onToggleAnimation: $e,
				onToggleFullscreen: We,
				onCopyAlt: $,
				style: h({ visibility: C(Ne) ? C(M) ? "visible" : "hidden" : "visible" })
			}, le({ _: 2 }, [
				t.$slots.menuIcon ? {
					name: "menuIcon",
					fn: T(({ isOpen: e, color: n }) => [x(t.$slots, "menuIcon", m(p({
						isOpen: e,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				t.$slots.optionPdf ? {
					name: "optionPdf",
					fn: T(() => [x(t.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				t.$slots.optionCsv ? {
					name: "optionCsv",
					fn: T(() => [x(t.$slots, "optionCsv", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				t.$slots.optionImg ? {
					name: "optionImg",
					fn: T(() => [x(t.$slots, "optionImg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				t.$slots.optionSvg ? {
					name: "optionSvg",
					fn: T(() => [x(t.$slots, "optionSvg", {}, void 0, !0)]),
					key: "4"
				} : void 0,
				t.$slots.optionAnimation ? {
					name: "optionAnimation",
					fn: T(({ toggleAnimation: e, isAnimated: n }) => [x(t.$slots, "optionAnimation", m(p({
						toggleAnimation: e,
						isAnimated: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				t.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: T(({ toggleFullscreen: e, isFullscreen: n }) => [x(t.$slots, "optionFullscreen", m(p({
						toggleFullscreen: e,
						isFullscreen: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				t.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: T(({ altCopy: e }) => [x(t.$slots, "optionAltCopy", m(p({ altCopy: e })), void 0, !0)]),
					key: "7"
				} : void 0,
				t.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: T(() => [x(t.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "8"
				} : void 0,
				t.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: T(() => [x(t.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "9"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"hasPdf",
				"hasImg",
				"hasXls",
				"hasAnimation",
				"isAnimation",
				"hasFullscreen",
				"hasAltCopy",
				"isFullscreen",
				"chartElement",
				"position",
				"titles",
				"callbacks",
				"printScale",
				"isCursorPointer",
				"onGenerateImage",
				"style"
			])) : l("", !0),
			t.$slots.source ? (v(), u("div", Ce, [x(t.$slots, "source", {}, void 0, !0)], 512)) : l("", !0)
		], 544));
	}
}, [["__scopeId", "data-v-42d3302a"]]);
//#endregion
export { ge as n, E as t };
