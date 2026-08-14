import { Ot as e, X as t, q as n, t as r } from "./lib-Bttd6u5E.js";
import { t as i } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { t as a } from "./BaseIcon-BfndwIWE.js";
import { t as o } from "./ColorPicker--ayG2YHf.js";
import { Fragment as s, computed as c, createCommentVNode as l, createElementBlock as u, createElementVNode as d, createVNode as f, guardReactiveProps as p, nextTick as ee, normalizeClass as m, normalizeProps as h, normalizeStyle as g, onBeforeUnmount as te, onMounted as ne, openBlock as _, ref as v, renderList as y, renderSlot as b, toDisplayString as x, unref as re, vModelText as ie, watch as S, withCtx as ae, withDirectives as oe, withModifiers as se } from "vue";
//#region src/atoms/NonSvgPenAndPaper.vue
var ce = ["disabled"], le = [
	"data-mode",
	"xmlns",
	"viewBox"
], ue = [
	"cx",
	"cy",
	"r",
	"fill"
], de = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width"
], fe = [
	"x1",
	"y1",
	"x2",
	"y2",
	"stroke",
	"stroke-width",
	"marker-end"
], pe = [
	"d",
	"stroke",
	"stroke-width"
], me = [
	"x",
	"y",
	"fill",
	"font-size"
], he = ["x", "dy"], ge = [
	"d",
	"stroke",
	"stroke-width"
], _e = /*#__PURE__*/ i({
	__name: "NonSvgPenAndPaper",
	props: {
		parent: { type: HTMLElement },
		backgroundColor: {
			type: String,
			default: "#FFFFFF"
		},
		color: {
			type: String,
			default: "#2D353C"
		},
		active: {
			type: Boolean,
			default: !1
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		}
	},
	emits: ["close"],
	setup(i, { emit: _e }) {
		let C = i, ve = _e, w = v([]), T = v([]), ye = v("0 0 0 0"), E = v(C.color), D = v(1), O = v(null), k = v(!1), A = v(null), j = v({
			x: 0,
			y: 0
		}), M = v([""]), N = v({
			row: 0,
			col: 0
		}), P = v(16), be = 1, F = [
			"arrow",
			"text",
			"line",
			"draw"
		], I = v(0), L = c(() => F[I.value]), R = v(null);
		v(null), v(null);
		let z = v(`arrow-${n()}`), B = v(`arrow-def-${n()}`), xe = {
			arrow: "plotArrow",
			text: "text",
			line: "plotLine",
			draw: "annotator"
		};
		function Se() {
			I.value + 1 >= F.length ? I.value = 0 : I.value += 1;
		}
		function V() {
			return be++;
		}
		function Ce(e) {
			let t = J.value;
			if (!t) return {
				x: 0,
				y: 0
			};
			let n = t.createSVGPoint(), r = e.touches?.length || e.changedTouches?.length ? e.touches?.[0] || e.changedTouches?.[0] : null, i = r ? r.clientX : e.clientX, a = r ? r.clientY : e.clientY;
			n.x = i, n.y = a;
			let o = t.getScreenCTM()?.inverse();
			return o ? n.matrixTransform(o) : {
				x: 0,
				y: 0
			};
		}
		function H(e) {
			if (!O.value || L.value !== "text" || k.value) return;
			let { x: t, y: n } = Ce(e);
			j.value = {
				x: t,
				y: n
			}, M.value = [""], N.value = {
				row: 0,
				col: 0
			};
			let r = document.createElementNS("http://www.w3.org/2000/svg", "text");
			r.setAttribute("x", t), r.setAttribute("y", n), r.setAttribute("fill", E.value), r.setAttribute("font-size", P.value), r.setAttribute("font-family", "sans-serif"), r.setAttribute("class", "vue-data-ui-doodle"), r.setAttribute("dominant-baseline", "hanging"), r.setAttribute("pointer-events", "all");
			let i = document.createElementNS("http://www.w3.org/2000/svg", "tspan");
			i.setAttribute("x", t), i.setAttribute("dy", "0"), i.textContent = "", r.appendChild(i), r.style.pointerEvents = "none", r.style.userSelect = "none", O.value.appendChild(r), A.value = r, k.value = !0, window.addEventListener("keydown", we), window.addEventListener("mousedown", De, !0), Te(), Ee();
		}
		function we(e) {
			if (!k.value) return;
			let { row: t, col: n } = N.value, r = M.value.slice(), i = !1;
			if (e.key === "Enter") {
				let a = r[t], o = a.slice(0, n), s = a.slice(n);
				r.splice(t, 1, o, s), t += 1, n = 0, i = !0, e.preventDefault();
			} else if (e.key === "Backspace") {
				if (n > 0) r[t] = r[t].slice(0, n - 1) + r[t].slice(n), --n, i = !0;
				else if (t > 0) {
					let e = r[t - 1].length;
					r[t - 1] += r[t], r.splice(t, 1), --t, n = e, i = !0;
				}
				e.preventDefault();
			} else if (e.key === "Delete") n < r[t].length ? (r[t] = r[t].slice(0, n) + r[t].slice(n + 1), i = !0) : t < r.length - 1 && (r[t] += r[t + 1], r.splice(t + 1, 1), i = !0), e.preventDefault();
			else if (e.key === "ArrowLeft") n > 0 ? --n : t > 0 && (--t, n = r[t].length), i = !0, e.preventDefault();
			else if (e.key === "ArrowRight") n < r[t].length ? n += 1 : t < r.length - 1 && (t += 1, n = 0), i = !0, e.preventDefault();
			else if (e.key === "ArrowUp") t > 0 && (--t, n = Math.min(n, r[t].length), i = !0), e.preventDefault();
			else if (e.key === "ArrowDown") t < r.length - 1 && (t += 1, n = Math.min(n, r[t].length), i = !0), e.preventDefault();
			else if (e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey) r[t] = r[t].slice(0, n) + e.key + r[t].slice(n), n += 1, i = !0, e.preventDefault();
			else if (e.key === "Escape") {
				Oe(!0);
				return;
			} else e.key === "Tab" && e.preventDefault();
			i && (M.value = r, N.value = {
				row: t,
				col: n
			}, Te(), Ee());
		}
		function Te() {
			let e = A.value, { x: t, y: n } = j.value;
			for (; e.firstChild;) e.removeChild(e.firstChild);
			M.value.forEach((n, r) => {
				let i = document.createElementNS("http://www.w3.org/2000/svg", "tspan");
				i.setAttribute("x", t), i.setAttribute("dy", r === 0 ? "0" : `${P.value * 1.2}`), i.textContent = n.length ? n : "​", e.appendChild(i);
			});
		}
		function Ee() {
			let e = O.value.querySelector(".vue-data-ui-svg-caret");
			e && O.value.removeChild(e);
			let t = A.value;
			if (!t) return;
			let { x: n, y: r } = j.value, { row: i, col: a } = N.value, o = P.value, s = t.childNodes[i];
			if (!s) return;
			let c = s.textContent.slice(0, a);
			c.endsWith(" ") && (c += "\xA0");
			let l = document.createElementNS("http://www.w3.org/2000/svg", "text");
			l.setAttribute("x", n), l.setAttribute("y", r), l.setAttribute("font-size", o), l.setAttribute("font-family", "sans-serif"), l.textContent = c || "", O.value.appendChild(l);
			let u = l.getBBox();
			O.value.removeChild(l);
			let d = r + i * o * 1.2, f = n + u.width, p = document.createElementNS("http://www.w3.org/2000/svg", "rect");
			p.setAttribute("x", f), p.setAttribute("y", d), p.setAttribute("width", 1), p.setAttribute("height", o), p.setAttribute("fill", E.value), p.setAttribute("class", "vue-data-ui-svg-caret"), O.value.appendChild(p);
		}
		function De(e) {
			if (A.value && !A.value.contains(e.target)) {
				let e = A.value.children;
				e.length === 1 && (e[0].textContent === "" || e[0].textContent === "​") && A.value.remove(), Oe(!1);
			}
		}
		function Oe(e = !1) {
			window.removeEventListener("keydown", we), window.removeEventListener("mousedown", De, !0);
			let t = O.value?.querySelector(".vue-data-ui-svg-caret");
			t && O.value.removeChild(t);
			let n = M.value.every((e) => !e || e === "​");
			e || n || w.value.push({
				id: V(),
				type: "text",
				x: j.value.x,
				y: j.value.y,
				color: E.value,
				fontSize: P.value,
				lines: M.value.map((e) => e)
			}), A.value && O.value && O.value.contains(A.value) && O.value.removeChild(A.value), k.value = !1, A.value = null, M.value = [""], N.value = {
				row: 0,
				col: 0
			};
		}
		let U = c(() => e(C.color, .6));
		function W({ width: e, height: t }) {
			ye.value = `0 0 ${e} ${t}`;
		}
		let G = v(null);
		ne(() => {
			ee(() => {
				if (C.parent) {
					G.value = new ResizeObserver((e) => {
						for (let t of e) {
							let { width: e, height: n } = t.contentRect;
							W({
								width: e,
								height: n
							});
						}
					}), G.value.observe(C.parent);
					let { width: e, height: t } = C.parent.getBoundingClientRect();
					W({
						width: e,
						height: t
					});
				}
			}), O.value = J.value.querySelector("g");
		}), te(() => {
			G.value && G.value.disconnect();
		}), S(() => C.parent, (e) => {
			if (!e) return;
			let { width: t, height: n } = C.parent.getBoundingClientRect();
			W({
				width: t,
				height: n
			});
		}, { immediate: !0 }), S(L, (e) => {
			J.value && (J.value.removeEventListener("mousedown", H), J.value.removeEventListener("touchstart", H), e === "text" && (J.value.addEventListener("mousedown", H), J.value.addEventListener("touchstart", H, { passive: !1 })));
		});
		let K = v(!1), q = v(""), J = v(null), Y = v(!1), ke = v(null);
		S(() => C.active, (e) => {
			J.value && (J.value.style.touchAction = e ? "none" : "");
		}, { immediate: !0 });
		function Ae(e) {
			if (e.cancelable && e.preventDefault(), L.value !== "draw" || !J.value) return;
			K.value = !0;
			let { x: t, y: n } = Q(e);
			q.value = `M ${t} ${n}`;
		}
		function je(e) {
			if (e.cancelable && e.preventDefault(), L.value !== "draw" || !K.value || !J.value) return;
			let { x: t, y: n } = Q(e);
			q.value += ` ${t} ${n}`;
		}
		function Me() {
			let e = J.value;
			if (!e) return;
			let t = e.querySelector(`defs#${B.value}`);
			if (t || (t = document.createElementNS(r, "defs"), t.setAttribute("id", B.value), e.appendChild(t)), t.querySelector(`#${z.value}`)) return;
			let n = document.createElementNS(r, "marker");
			n.setAttribute("id", z.value), n.setAttribute("markerUnits", "strokeWidth"), n.setAttribute("markerWidth", "6"), n.setAttribute("markerHeight", "6"), n.setAttribute("refX", "4"), n.setAttribute("refY", "3"), n.setAttribute("orient", "auto"), n.setAttribute("viewBox", "0 0 6 6");
			let i = document.createElementNS(r, "path");
			i.setAttribute("d", `M 0 0 L 6 ${6 / 2} L 0 6 z`), i.setAttribute("fill", "context-stroke"), i.setAttribute("stroke", "none"), n.appendChild(i), t.appendChild(n);
		}
		function Ne(e) {
			if (e.cancelable && e.preventDefault(), !["line", "arrow"].includes(L.value) || !J.value || !O.value) return;
			L.value === "arrow" && Me(), Y.value = !0;
			let { x: t, y: n } = Q(e);
			ke.value = {
				x: t,
				y: n
			}, R.value = document.createElementNS(r, "line"), R.value.setAttribute("x1", t), R.value.setAttribute("y1", n), R.value.setAttribute("x2", t), R.value.setAttribute("y2", n), R.value.setAttribute("stroke", E.value), R.value.setAttribute("stroke-width", D.value), R.value.setAttribute("stroke-linecap", "round"), R.value.setAttribute("class", "vue-data-ui-doodle"), L.value === "arrow" && R.value.setAttribute("marker-end", `url(#${z.value})`), O.value.appendChild(R.value);
		}
		function Pe(e) {
			if (e.cancelable && e.preventDefault(), !["line", "arrow"].includes(L.value) || !Y.value || !R.value) return;
			let { x: t, y: n } = Q(e);
			R.value.setAttribute("x2", t), R.value.setAttribute("y2", n);
		}
		function X(e) {
			if (e?.cancelable && e.preventDefault(), !Y.value || !R.value) return;
			let { x: t, y: n } = Q(e);
			R.value.setAttribute("x2", t), R.value.setAttribute("y2", n), w.value.push({
				id: V(),
				type: L.value,
				x1: parseFloat(R.value.getAttribute("x1")),
				y1: parseFloat(R.value.getAttribute("y1")),
				x2: t,
				y2: n,
				strokeWidth: D.value,
				color: E.value
			}), T.value = [], O.value.contains(R.value) && O.value.removeChild(R.value), R.value = null, ke.value = null, Y.value = !1;
		}
		function Fe(e) {
			let t = e.trim().split(/\s+/);
			if (t.length < 4) return e;
			let n = t.slice(1).map(Number);
			if (n.length % 2 != 0) return e;
			let r = Ie(n), i = [`M ${r[0]} ${r[1]}`];
			for (let e = 2; e < r.length - 2; e += 2) {
				let t = r[e - 2], n = r[e - 1], a = r[e], o = r[e + 1], s = (t + a) / 2, c = (n + o) / 2;
				i.push(`Q ${t} ${n} ${s} ${c}`);
			}
			let a = r[r.length - 2], o = r[r.length - 1];
			return i.push(`L ${a} ${o}`), i.join(" ");
		}
		function Ie(e, t = 1) {
			let n = [...e];
			for (let r = 2; r < e.length - 2; r += 2) {
				let i = e[r], a = e[r + 1], o = e[r - 2], s = e[r - 1], c = e[r + 2], l = e[r + 3];
				n[r] = i + t * ((o + c) / 2 - i), n[r + 1] = a + t * ((s + l) / 2 - a);
			}
			return n;
		}
		function Le(e) {
			let t = e.trim().split(/\s+/), n = "", r = "", i = null, a = null;
			for (let e = 0; e < t.length; e += 1) {
				let o = t[e];
				if (isNaN(o)) {
					if (r = o, r === "M" || r === "L") i = parseFloat(t[++e]), a = parseFloat(t[++e]), n += `${r}${i} ${a}`;
					else if (r === "Q") {
						let r = parseFloat(t[++e]), o = parseFloat(t[++e]), s = parseFloat(t[++e]), c = parseFloat(t[++e]);
						n += r === i && o === a ? `t${s - i} ${c - a}` : `q${r - i} ${o - a} ${s - i} ${c - a}`, i = s, a = c;
					}
				} else {
					let s = parseFloat(o), c = parseFloat(t[++e]);
					if (r === "L") {
						let e = s - i, t = c - a;
						n += e === 0 ? `v${t}` : t === 0 ? `h${e}` : `l${e} ${t}`, i = s, a = c;
					} else if (r === "Q") {
						let r = s, o = c, l = parseFloat(t[++e]), u = parseFloat(t[++e]);
						n += r === i && o === a ? `t${l - i} ${u - a}` : `q${r - i} ${o - a} ${l - i} ${u - a}`, i = l, a = u;
					}
				}
			}
			return n;
		}
		function Z(e) {
			e?.cancelable && e.preventDefault(), L.value === "draw" && (K.value && (w.value.push({
				id: V(),
				strokeWidth: D.value,
				path: Le(Fe(q.value)),
				color: E.value
			}), T.value = [], q.value = ""), K.value = !1);
		}
		function Q(e) {
			if (!J.value) return {
				x: 0,
				y: 0
			};
			let t = J.value.getBoundingClientRect(), n = e.touches?.length || e.changedTouches?.length ? e.touches?.[0] || e.changedTouches?.[0] : null, r = n ? n.clientX : e.clientX, i = n ? n.clientY : e.clientY;
			return {
				x: r - t.left,
				y: i - t.top
			};
		}
		function Re() {
			if (w.value.length > 0) {
				let e = w.value.pop();
				T.value.push(e);
			}
		}
		function ze() {
			if (T.value.length > 0) {
				let e = T.value.pop();
				w.value.push(e);
			}
		}
		function Be() {
			w.value = [], T.value = [];
		}
		let $ = v(null);
		return (e, n) => (_(), u(s, null, [i.active ? (_(), u("div", {
			key: 0,
			"data-dom-to-png-ignore": "",
			class: m({
				"vue-ui-pen-and-paper-actions": !0,
				visible: i.active
			}),
			style: g({ backgroundColor: i.backgroundColor })
		}, [
			d("button", {
				class: "vue-ui-pen-and-paper-action",
				style: g({
					backgroundColor: i.backgroundColor,
					border: `1px solid ${U.value}`,
					cursor: i.isCursorPointer ? "pointer" : "default"
				}),
				onClick: n[0] ||= (e) => ve("close")
			}, [b(e.$slots, "annotator-action-close", {}, () => [f(a, {
				name: "close",
				stroke: i.color
			}, null, 8, ["stroke"])], !0)], 4),
			d("button", {
				class: m({ "vue-ui-pen-and-paper-action": !0 }),
				style: g({
					padding: "0 !important",
					cursor: i.isCursorPointer ? "pointer" : "default"
				})
			}, [f(o, {
				value: E.value,
				"onUpdate:value": n[1] ||= (e) => E.value = e,
				backgroundColor: i.backgroundColor,
				buttonBorderColor: U.value
			}, {
				"annotator-action-color": ae(({ color: t }) => [b(e.$slots, "annotator-action-color", h(p({ color: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"value",
				"backgroundColor",
				"buttonBorderColor"
			])], 4),
			d("button", {
				class: m(["vue-ui-pen-and-paper-action", { "vue-ui-pen-and-paper-action-active": L.value === "text" }]),
				onClick: n[2] ||= (e) => Se(),
				style: g({
					backgroundColor: i.backgroundColor,
					border: `1px solid ${U.value}`,
					cursor: i.isCursorPointer ? "pointer" : "default"
				})
			}, [b(e.$slots, "annotator-action-draw", h(p({ mode: L.value })), () => [f(a, {
				name: xe[L.value],
				stroke: i.color
			}, null, 8, ["name", "stroke"])], !0), d("div", { style: g({
				position: "absolute",
				bottom: "-20px",
				color: U.value,
				width: "100%",
				textAlign: "center",
				fontSize: "12px"
			}) }, x(re(t)({
				v: L.value === "text" ? P.value : D.value,
				s: "px",
				r: 1
			})), 5)], 6),
			d("button", {
				class: m({
					"vue-ui-pen-and-paper-action": !0,
					"vue-ui-pen-and-paper-action-disabled": !w.value.length
				}),
				disabled: !w.value.length,
				style: g({
					backgroundColor: i.backgroundColor,
					border: `1px solid ${U.value}`,
					marginTop: "20px",
					cursor: i.isCursorPointer ? "pointer" : "default"
				}),
				onClick: Re
			}, [b(e.$slots, "annotator-action-undo", h(p({ disabled: !w.value.length })), () => [f(a, {
				name: "refresh",
				stroke: i.color
			}, null, 8, ["stroke"])], !0)], 14, ce),
			d("button", {
				class: m({
					"vue-ui-pen-and-paper-action": !0,
					"vue-ui-pen-and-paper-action-disabled": !T.value.length
				}),
				style: g({
					backgroundColor: i.backgroundColor,
					border: `1px solid ${U.value}`,
					cursor: i.isCursorPointer ? "pointer" : "default"
				}),
				onClick: ze
			}, [b(e.$slots, "annotator-action-redo", h(p({ disabled: !T.value.length })), () => [f(a, {
				name: "refresh",
				stroke: i.color,
				style: { transform: "scaleX(-1)" }
			}, null, 8, ["stroke"])], !0)], 6),
			d("button", {
				class: m([{
					"vue-ui-pen-and-paper-action": !0,
					"vue-ui-pen-and-paper-action-disabled": !w.value.length
				}, "vue-ui-pen-and-paper-action"]),
				style: g({
					backgroundColor: i.backgroundColor,
					border: `1px solid ${U.value}`,
					cursor: i.isCursorPointer ? "pointer" : "default"
				}),
				onClick: Be
			}, [b(e.$slots, "annotator-action-delete", h(p({ disabled: !w.value.length })), () => [f(a, {
				name: "trash",
				stroke: i.color
			}, null, 8, ["stroke"])], !0)], 6),
			L.value === "draw" ? oe((_(), u("input", {
				key: 0,
				ref_key: "range",
				ref: $,
				type: "range",
				class: "vertical-range",
				min: .5,
				max: 12,
				step: .1,
				"onUpdate:modelValue": n[3] ||= (e) => D.value = e,
				style: g({ accentColor: i.color })
			}, null, 4)), [[ie, D.value]]) : l("", !0),
			L.value === "text" ? oe((_(), u("input", {
				key: 1,
				ref_key: "range",
				ref: $,
				type: "range",
				class: "vertical-range",
				min: 3,
				max: 48,
				step: .1,
				"onUpdate:modelValue": n[4] ||= (e) => P.value = e,
				style: g({ accentColor: i.color })
			}, null, 4)), [[ie, P.value]]) : l("", !0)
		], 6)) : l("", !0), (_(), u("svg", {
			"data-mode": L.value,
			ref_key: "svgElement",
			ref: J,
			xmlns: re(r),
			viewBox: ye.value,
			class: m({
				"vue-ui-pen-and-paper": !0,
				inactive: !i.active
			}),
			onMousedown: n[5] ||= (e) => L.value === "draw" ? Ae(e) : L.value === "line" || L.value === "arrow" ? Ne(e) : null,
			onMousemove: n[6] ||= (e) => L.value === "draw" ? je(e) : L.value === "line" || L.value === "arrow" ? Pe(e) : null,
			onMouseup: n[7] ||= (e) => L.value === "draw" ? Z(e) : L.value === "line" || L.value === "arrow" ? X(e) : null,
			onMouseleave: n[8] ||= (e) => L.value === "draw" ? Z(e) : L.value === "line" || L.value === "arrow" ? X(e) : null,
			onTouchstart: n[9] ||= se((e) => L.value === "draw" ? Ae(e) : L.value === "line" || L.value === "arrow" ? Ne(e) : null, ["prevent"]),
			onTouchmove: n[10] ||= se((e) => L.value === "draw" ? je(e) : L.value === "line" || L.value === "arrow" ? Pe(e) : null, ["prevent"]),
			onTouchend: n[11] ||= (e) => L.value === "draw" ? Z(e) : L.value === "line" || L.value === "arrow" ? X(e) : null,
			onTouchcancel: n[12] ||= (e) => L.value === "draw" ? Z(e) : L.value === "line" || L.value === "arrow" ? X(e) : null
		}, [d("g", {
			ref_key: "G",
			ref: O
		}, [(_(!0), u(s, null, y(w.value, (e) => (_(), u(s, { key: e.id }, [e.path && e.path.replace("M", "").split(" ").length === 2 ? (_(), u("circle", {
			key: 0,
			cx: e.path.replace("M", "").split(" ")[0],
			cy: e.path.replace("M", "").split(" ")[1],
			r: e.strokeWidth / 2,
			fill: e.color
		}, null, 8, ue)) : e.type === "line" ? (_(), u("line", {
			key: 1,
			class: "vue-ui-pen-and-paper-path",
			x1: e.x1,
			y1: e.y1,
			x2: e.x2,
			y2: e.y2,
			stroke: e.color,
			"stroke-width": e.strokeWidth
		}, null, 8, de)) : e.type === "arrow" ? (_(), u("line", {
			key: 2,
			class: "vue-ui-pen-and-paper-path",
			x1: e.x1,
			y1: e.y1,
			x2: e.x2,
			y2: e.y2,
			stroke: e.color,
			"stroke-width": e.strokeWidth,
			"marker-end": `url(#${z.value})`
		}, null, 8, fe)) : e.path ? (_(), u("path", {
			key: 3,
			class: "vue-ui-pen-and-paper-path",
			d: e.path,
			stroke: e.color,
			"stroke-width": e.strokeWidth,
			fill: "none"
		}, null, 8, pe)) : e.type === "text" ? (_(), u("text", {
			key: 4,
			x: e.x,
			y: e.y,
			fill: e.color,
			"font-size": e.fontSize,
			"font-family": "sans-serif",
			"dominant-baseline": "hanging",
			class: "vue-ui-pen-and-paper-text"
		}, [(_(!0), u(s, null, y(e.lines, (t, n) => (_(), u("tspan", {
			key: n,
			x: e.x,
			dy: n === 0 ? "0" : e.fontSize * 1.2
		}, x(t.length ? t : "​"), 9, he))), 128))], 8, me)) : l("", !0)], 64))), 128))], 512), K.value ? (_(), u("path", {
			key: 0,
			class: "vue-ui-pen-and-paper-path vue-ui-pen-and-paper-path-drawing",
			d: Fe(q.value),
			stroke: E.value,
			"stroke-width": D.value * 1.1,
			fill: "none"
		}, null, 8, ge)) : l("", !0)], 42, le))], 64));
	}
}, [["__scopeId", "data-v-42e6ba29"]]);
//#endregion
export { _e as default };
