import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Ot as t, X as n, q as r, t as i } from "./lib-Bttd6u5E.js";
import { t as a } from "./BaseIcon-BfndwIWE.js";
import { t as o } from "./ColorPicker--ayG2YHf.js";
import { computed as s, createCommentVNode as c, createElementBlock as l, createElementVNode as u, createVNode as d, guardReactiveProps as f, nextTick as p, normalizeClass as m, normalizeProps as h, normalizeStyle as g, onBeforeUnmount as ee, onMounted as te, openBlock as _, ref as v, renderSlot as y, toDisplayString as ne, unref as re, vModelText as ie, watch as b, withCtx as ae, withDirectives as oe } from "vue";
//#region src/atoms/PenAndPaper.vue
var se = /* @__PURE__ */ e({ default: () => x }), ce = ["disabled"], x = {
	__name: "PenAndPaper",
	props: {
		svgRef: {
			type: [
				Object,
				null,
				void 0
			],
			required: !0
		},
		color: {
			type: String,
			default: "#2D353C"
		},
		backgroundColor: {
			type: String,
			default: "#FFFFFF"
		},
		active: {
			type: Boolean,
			default: !1
		},
		scale: {
			type: Number,
			default: 1
		},
		isCursorPointer: {
			type: Boolean,
			default: !1
		}
	},
	emits: ["close"],
	setup(e, { emit: se }) {
		let x = e, le = se, S = v([]), C = v([]), w = v(x.color), T = v(2), E = v(!1), D = v(""), O = v(null), k = v(null), A = v(null), j = v(null), M = v(`arrow-${r()}`), ue = v(`arrow-def-${r()}`), N = v(!1), P = v(null), F = v({
			x: 0,
			y: 0
		}), I = v([""]), L = v({
			row: 0,
			col: 0
		}), R = v(16), z = v(!1), de = [
			"arrow",
			"text",
			"line",
			"draw"
		], B = v(0), V = s(() => de[B.value]), fe = {
			arrow: "plotArrow",
			text: "text",
			line: "plotLine",
			draw: "annotator"
		}, pe = v("url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAABg2lDQ1BJQ0MgcHJvZmlsZQAAKJF9kT1Iw0AcxV9TpSIVh2YQcchQnSyIijhKFYtgobQVWnUwufQLmjQkKS6OgmvBwY/FqoOLs64OroIg+AHi6OSk6CIl/i8ptIjx4Lgf7+497t4BQrPKNKtnAtB020wn4lIuvyqFXhGGiAhCiMnMMpKZxSx8x9c9Any9i/Es/3N/jgG1YDEgIBHPMcO0iTeIZzZtg/M+scjKskp8Tjxu0gWJH7muePzGueSywDNFM5ueJxaJpVIXK13MyqZGPE0cVTWd8oWcxyrnLc5atc7a9+QvDBf0lQzXaY4ggSUkkYIEBXVUUIWNGK06KRbStB/38Q+7/hS5FHJVwMixgBo0yK4f/A9+d2sVpya9pHAc6H1xnI9RILQLtBqO833sOK0TIPgMXOkdf60JzH6S3uho0SNgcBu4uO5oyh5wuQMMPRmyKbtSkKZQLALvZ/RNeSByC/Sveb2193H6AGSpq+Ub4OAQGCtR9rrPu/u6e/v3TLu/H5C7crM1WjgWAAAABmJLR0QAqwB5AHWF+8OUAAAACXBIWXMAAC4jAAAuIwF4pT92AAAAB3RJTUUH5gwUExIUagzGcQAAABl0RVh0Q29tbWVudABDcmVhdGVkIHdpdGggR0lNUFeBDhcAAABfSURBVBjTldAxDoNQDIPhL0+q1L33P1AvAhN7xfK6WAgoLfSfrNiykpQtE+7RLzx2vgF9D3o8lWDmn1QVVMP0LZQGmNtqp1/cmou0XHdG/+sYeGZwFBqPCub8rkcvvAGvsi1VYarR8wAAAABJRU5ErkJggg==') 5 5, auto");
		function me() {
			B.value + 1 >= de.length ? B.value = 0 : B.value += 1;
		}
		function H(e) {
			if (!O.value || V.value !== "text" || N.value) return;
			let { x: t, y: n } = K(e);
			F.value = {
				x: t,
				y: n
			}, I.value = [""], L.value = {
				row: 0,
				col: 0
			}, z.value = !1;
			let r = document.createElementNS("http://www.w3.org/2000/svg", "text");
			r.setAttribute("x", t), r.setAttribute("y", n), r.setAttribute("fill", w.value), r.setAttribute("font-size", R.value * x.scale), r.setAttribute("font-family", "sans-serif"), r.setAttribute("class", "vue-data-ui-doodle"), r.setAttribute("dominant-baseline", "hanging"), r.setAttribute("pointer-events", "all");
			let i = document.createElementNS("http://www.w3.org/2000/svg", "tspan");
			i.setAttribute("x", t), i.setAttribute("dy", "0"), i.textContent = "", r.appendChild(i), r.style.pointerEvents = "none", r.style.userSelect = "none", O.value.appendChild(r), P.value = r, N.value = !0, window.addEventListener("keydown", he), window.addEventListener("mousedown", ye, !0), ge(), ve();
		}
		function he(e) {
			if (!N.value) return;
			let { row: t, col: n } = L.value, r = I.value.slice(), i = !1;
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
				be(!0);
				return;
			} else e.key === "Tab" && e.preventDefault();
			i && (I.value = r, L.value = {
				row: t,
				col: n
			}, r.some((e) => e.length > 0) && !z.value && P.value && (S.value.push(P.value), C.value = [], z.value = !0), ge(), ve());
		}
		function ge() {
			let e = P.value, { x: t } = F.value;
			for (; e.firstChild;) e.removeChild(e.firstChild);
			I.value.forEach((n, r) => {
				let i = document.createElementNS("http://www.w3.org/2000/svg", "tspan");
				i.setAttribute("x", t), i.setAttribute("dy", r === 0 ? "0" : `${R.value * 1.2 * x.scale}`), i.textContent = n.length ? n : "​", e.appendChild(i);
			});
		}
		let U = v(null);
		function W() {
			U.value !== null && (clearInterval(U.value), U.value = null);
		}
		function _e(e) {
			W();
			let t = !0;
			e.style.opacity = "1", U.value = setInterval(() => {
				if (!O.value || !e || !O.value.contains(e)) {
					W();
					return;
				}
				t = !t, e.style.opacity = t ? "1" : "0";
			}, 500);
		}
		function ve() {
			let e = O.value?.querySelector(".vue-data-ui-svg-caret");
			e && O.value && O.value.removeChild(e);
			let t = P.value;
			if (!t || !O.value) return;
			let { x: n, y: r } = F.value, { row: i, col: a } = L.value, o = R.value * x.scale, s = t.childNodes[i];
			if (!s) return;
			let c = s.textContent.slice(0, a);
			c.endsWith(" ") && (c += "\xA0");
			let l = document.createElementNS("http://www.w3.org/2000/svg", "text");
			l.setAttribute("x", n), l.setAttribute("y", r), l.setAttribute("font-size", o), l.setAttribute("font-family", "sans-serif"), l.textContent = c || "", O.value.appendChild(l);
			let u = l.getBBox();
			O.value.removeChild(l);
			let d = r + i * o * 1.2, f = n + u.width, p = document.createElementNS("http://www.w3.org/2000/svg", "rect");
			p.setAttribute("x", f), p.setAttribute("y", d), p.setAttribute("rx", 1), p.setAttribute("width", 2), p.setAttribute("height", o), p.setAttribute("fill", w.value), p.setAttribute("class", "vue-data-ui-svg-caret"), O.value.appendChild(p), _e(p);
		}
		function ye(e) {
			if (P.value && !P.value.contains(e.target)) {
				let e = P.value.children;
				e.length === 1 && (e[0].textContent === "" || e[0].textContent === "​") && P.value.remove(), be(!1);
			}
		}
		function be(e = !1) {
			window.removeEventListener("keydown", he), window.removeEventListener("mousedown", ye, !0), W();
			let t = O.value?.querySelector(".vue-data-ui-svg-caret");
			t && O.value && O.value.removeChild(t);
			let n = P.value?.children, r = !1;
			if (n && n.length === 1) {
				let e = n[0].textContent;
				r = !e || e === "​";
			}
			(e || r) && P.value && O.value && O.value.contains(P.value) && O.value.removeChild(P.value), N.value = !1, P.value = null, I.value = [""], L.value = {
				row: 0,
				col: 0
			}, z.value = !1;
		}
		let G = s(() => t(x.color, .6));
		function xe() {
			if (!O.value) return;
			let e = O.value.querySelector(".vue-data-ui-mask");
			if (e && O.value.removeChild(e), x.active) {
				let e = document.createElementNS("http://www.w3.org/2000/svg", "rect");
				e.setAttribute("class", "vue-data-ui-mask"), e.setAttribute("width", "100%"), e.setAttribute("height", "100%"), e.setAttribute("fill", "transparent"), e.setAttribute("pointer-events", "all"), O.value.insertBefore(e, O.value.firstChild);
			}
		}
		function K(e) {
			let t = x.svgRef;
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
		function Se(e) {
			let t = e.trim().split(/\s+/);
			if (t.length < 4) return e;
			let n = t.slice(1).map(Number);
			if (n.length % 2 != 0) return e;
			let r = Ce(n), i = [`M ${r[0]} ${r[1]}`];
			for (let e = 2; e < r.length - 2; e += 2) {
				let t = r[e - 2], n = r[e - 1], a = r[e], o = r[e + 1], s = (t + a) / 2, c = (n + o) / 2;
				i.push(`Q ${t} ${n} ${s} ${c}`);
			}
			let a = r[r.length - 2], o = r[r.length - 1];
			return i.push(`L ${a} ${o}`), i.join(" ");
		}
		function Ce(e, t = 1) {
			let n = [...e];
			for (let r = 2; r < e.length - 2; r += 2) {
				let i = e[r], a = e[r + 1], o = e[r - 2], s = e[r - 1], c = e[r + 2], l = e[r + 3];
				n[r] = i + t * ((o + c) / 2 - i), n[r + 1] = a + t * ((s + l) / 2 - a);
			}
			return n;
		}
		function we(e) {
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
		function q(e) {
			if (e.cancelable && e.preventDefault(), V.value !== "draw" || !x.active || !O.value) return;
			E.value = !0;
			let { x: t, y: n } = K(e);
			j.value = {
				x: t,
				y: n
			}, D.value = `M ${t} ${n}`, k.value = document.createElementNS(i, "path"), k.value.setAttribute("stroke", w.value), k.value.setAttribute("stroke-width", T.value * x.scale), k.value.setAttribute("fill", "none"), k.value.setAttribute("stroke-linecap", "round"), k.value.setAttribute("stroke-linejoin", "round"), k.value.setAttribute("class", "vue-data-ui-doodle"), O.value.appendChild(k.value);
		}
		function J(e) {
			if (e.cancelable && e.preventDefault(), !E.value || !O.value || !k.value) return;
			let { x: t, y: n } = K(e);
			D.value += ` ${t} ${n}`, k.value.setAttribute("d", D.value);
		}
		function Te() {
			let e = x.svgRef;
			if (!e) return;
			let t = e.querySelector(`defs#${ue.value}`);
			if (t || (t = document.createElementNS(i, "defs"), t.setAttribute("id", ue.value), e.appendChild(t)), t.querySelector(`#${M.value}`)) return;
			let n = document.createElementNS(i, "marker");
			n.setAttribute("id", M.value), n.setAttribute("markerUnits", "strokeWidth"), n.setAttribute("markerWidth", "6"), n.setAttribute("markerHeight", "6"), n.setAttribute("refX", "4"), n.setAttribute("refY", "3"), n.setAttribute("orient", "auto"), n.setAttribute("viewBox", "0 0 6 6");
			let r = document.createElementNS(i, "path");
			r.setAttribute("d", `M 0 0 L 6 ${6 / 2} L 0 6 z`), r.setAttribute("fill", "context-stroke"), r.setAttribute("stroke", "none"), n.appendChild(r), t.appendChild(n);
		}
		function Y(e) {
			if (e.cancelable && e.preventDefault(), !["line", "arrow"].includes(V.value) || !x.active || !O.value) return;
			V.value === "arrow" && Te(), E.value = !0;
			let { x: t, y: n } = K(e);
			j.value = {
				x: t,
				y: n
			}, A.value = document.createElementNS(i, "line"), A.value.setAttribute("stroke", w.value), A.value.setAttribute("stroke-width", T.value * x.scale), A.value.setAttribute("stroke-linecap", "round"), A.value.setAttribute("class", "vue-data-ui-doodle"), A.value.setAttribute("x1", t), A.value.setAttribute("y1", n), A.value.setAttribute("x2", t), A.value.setAttribute("y2", n), V.value === "arrow" && A.value.setAttribute("marker-end", `url(#${M.value})`), O.value.appendChild(A.value);
		}
		function X(e) {
			if (e.cancelable && e.preventDefault(), !["line", "arrow"].includes(V.value) || !E.value || !O.value || !A.value) return;
			let { x: t, y: n } = K(e);
			A.value.setAttribute("x2", t), A.value.setAttribute("y2", n);
		}
		function Z(e) {
			if (E.value && O.value && A.value) {
				let { x: t, y: n } = K(e);
				A.value.setAttribute("x2", t), A.value.setAttribute("y2", n), S.value.push(A.value), C.value = [], A.value = "";
			}
			E.value = !1;
		}
		function Q(e) {
			if (E.value && O.value && k.value) {
				let { x: t, y: n } = K(e);
				if (j.value && j.value.x === t && j.value.y === n) {
					let e = document.createElementNS("http://www.w3.org/2000/svg", "circle");
					e.setAttribute("cx", t), e.setAttribute("cy", n), e.setAttribute("r", T.value * x.scale / 2), e.setAttribute("fill", w.value), e.setAttribute("class", "vue-data-ui-doodle"), O.value.appendChild(e), S.value.push(e);
				} else {
					let e = k.value;
					e.setAttribute("d", we(Se(D.value))), S.value.push(e);
				}
				C.value = [], k.value = "";
			}
			E.value = !1;
		}
		function Ee() {
			if (S.value.length > 0) {
				let e = S.value.pop();
				C.value.push(e), e === P.value ? be(!0) : O.value && O.value.contains(e) && O.value.removeChild(e);
			}
		}
		function De() {
			if (C.value.length > 0) {
				let e = C.value.pop();
				S.value.push(e), O.value && O.value.appendChild(e);
			}
		}
		function Oe() {
			O.value && (O.value.innerHTML = ""), S.value = [], C.value = [], z.value = !1, xe();
		}
		b(V, (e) => {
			x.active && ($(), ke(), Ae());
		});
		function ke() {
			!x.svgRef || !x.active || (V.value === "draw" ? (x.svgRef.addEventListener("mousedown", q), x.svgRef.addEventListener("mousemove", J), x.svgRef.addEventListener("mouseup", Q), x.svgRef.addEventListener("mouseleave", Q), x.svgRef.addEventListener("touchstart", q, { passive: !1 }), x.svgRef.addEventListener("touchmove", J, { passive: !1 }), x.svgRef.addEventListener("touchend", Q), x.svgRef.addEventListener("touchcancel", Q)) : ["line", "arrow"].includes(V.value) ? (x.svgRef.addEventListener("mousedown", Y), x.svgRef.addEventListener("mousemove", X), x.svgRef.addEventListener("mouseup", Z), x.svgRef.addEventListener("touchstart", Y, { passive: !1 }), x.svgRef.addEventListener("touchmove", X, { passive: !1 }), x.svgRef.addEventListener("touchend", Z), x.svgRef.addEventListener("touchcancel", Z)) : V.value === "text" && (x.svgRef.addEventListener("mousedown", H), x.svgRef.addEventListener("touchstart", H, { passive: !1 })), O.value && (O.value.style.pointerEvents = "auto"));
		}
		function $() {
			x.svgRef && (x.svgRef.removeEventListener("mousedown", q), x.svgRef.removeEventListener("mousemove", J), x.svgRef.removeEventListener("mouseup", Q), x.svgRef.removeEventListener("mouseleave", Q), x.svgRef.removeEventListener("touchstart", q), x.svgRef.removeEventListener("touchmove", J), x.svgRef.removeEventListener("touchend", Q), x.svgRef.removeEventListener("touchcancel", Q), x.svgRef.removeEventListener("mousedown", Y), x.svgRef.removeEventListener("mousemove", X), x.svgRef.removeEventListener("mouseup", Z), x.svgRef.removeEventListener("touchstart", Y), x.svgRef.removeEventListener("touchmove", X), x.svgRef.removeEventListener("touchend", Z), x.svgRef.removeEventListener("touchcancel", Z), x.svgRef.removeEventListener("mousedown", H), x.svgRef.removeEventListener("touchstart", H), O.value && (O.value.style.pointerEvents = "none"));
		}
		b(() => x.active, (e) => {
			e ? ke() : $();
		}), b(() => x.active, () => {
			p(() => {
				xe();
			});
		}), b(() => x.active, (e) => {
			x.svgRef && (e ? x.svgRef.style.touchAction = "none" : x.svgRef.style.touchAction = "");
		}, { immediate: !0 });
		function Ae() {
			O.value && (V.value === "text" ? O.value.style.cursor = "text" : ["line", "arrow"].includes(V.value) ? O.value.style.cursor = "crosshair" : O.value.style.cursor = pe.value);
		}
		let je = v(null);
		return te(() => {
			p(() => {
				x.svgRef && (O.value = document.createElementNS("http://www.w3.org/2000/svg", "g"), O.value.setAttribute("class", "vue-data-ui-doodles"), x.svgRef.appendChild(O.value), Ae(), $());
			});
		}), b(() => x.active, async (e) => {
			e && (await p(), je.value?.focus());
		}, { flush: "post" }), ee(() => {
			W(), O.value && x.svgRef && (O.value.remove(), $());
		}), (t, r) => e.active ? (_(), l("div", {
			key: 0,
			"data-dom-to-png-ignore": "",
			class: "vue-ui-pen-and-paper-actions",
			style: g({ backgroundColor: e.backgroundColor })
		}, [
			u("button", {
				ref_key: "buttonToggle",
				ref: je,
				class: "vue-ui-pen-and-paper-action",
				onClick: r[0] ||= (e) => le("close"),
				style: g({
					backgroundColor: e.backgroundColor,
					border: `1px solid ${G.value}`,
					cursor: e.isCursorPointer ? "pointer" : "default"
				})
			}, [y(t.$slots, "annotator-action-close", {}, () => [d(a, {
				name: "close",
				stroke: e.color
			}, null, 8, ["stroke"])])], 4),
			u("button", {
				class: "vue-ui-pen-and-paper-action",
				tabindex: "-1",
				style: g(`padding: 0 !important; cursor: ${e.isCursorPointer ? "pointer" : "default"}`)
			}, [d(o, {
				value: w.value,
				"onUpdate:value": r[1] ||= (e) => w.value = e,
				backgroundColor: e.backgroundColor,
				buttonBorderColor: G.value,
				isCursorPointer: e.isCursorPointer
			}, {
				"annotator-action-color": ae(({ color: e }) => [y(t.$slots, "annotator-action-color", h(f({ color: e })))]),
				_: 3
			}, 8, [
				"value",
				"backgroundColor",
				"buttonBorderColor",
				"isCursorPointer"
			])], 4),
			u("button", {
				class: m(["vue-ui-pen-and-paper-action", { "vue-ui-pen-and-paper-action-active": V.value === "text" }]),
				onClick: r[2] ||= (e) => me(),
				style: g({
					backgroundColor: e.backgroundColor,
					border: `1px solid ${G.value}`,
					cursor: e.isCursorPointer ? "pointer" : "default"
				})
			}, [y(t.$slots, "annotator-action-draw", h(f({ mode: V.value })), () => [d(a, {
				name: fe[V.value],
				stroke: e.color
			}, null, 8, ["name", "stroke"])]), u("div", { style: g({
				position: "absolute",
				bottom: "-20px",
				color: G.value,
				width: "100%",
				textAlign: "center",
				fontSize: "12px",
				fontVariantNumeric: "tabular-nums"
			}) }, ne(re(n)({
				v: V.value === "text" ? Math.round(R.value) : Math.round(T.value),
				s: "px",
				r: 1
			})), 5)], 6),
			u("button", {
				class: m(["vue-ui-pen-and-paper-action", { "vue-ui-pen-and-paper-action-disabled": !S.value.length }]),
				disabled: !S.value.length,
				onClick: Ee,
				style: g({
					backgroundColor: e.backgroundColor,
					border: `1px solid ${G.value}`,
					marginTop: "20px",
					cursor: e.isCursorPointer ? "pointer" : "default"
				})
			}, [y(t.$slots, "annotator-action-undo", h(f({ disabled: !S.value.length })), () => [d(a, {
				name: "refresh",
				stroke: e.color
			}, null, 8, ["stroke"])])], 14, ce),
			u("button", {
				class: m(["vue-ui-pen-and-paper-action", { "vue-ui-pen-and-paper-action-disabled": !C.value.length }]),
				onClick: De,
				style: g({
					backgroundColor: e.backgroundColor,
					border: `1px solid ${G.value}`,
					cursor: e.isCursorPointer ? "pointer" : "default"
				})
			}, [y(t.$slots, "annotator-action-redo", h(f({ disabled: !C.value.length })), () => [d(a, {
				name: "refresh",
				stroke: e.color,
				style: { transform: "scaleX(-1)" }
			}, null, 8, ["stroke"])])], 6),
			u("button", {
				class: m(["vue-ui-pen-and-paper-action", { "vue-ui-pen-and-paper-action-disabled": !S.value.length }]),
				onClick: Oe,
				style: g({
					backgroundColor: e.backgroundColor,
					border: `1px solid ${G.value}`,
					cursor: e.isCursorPointer ? "pointer" : "default"
				})
			}, [y(t.$slots, "annotator-action-delete", h(f({ disabled: !S.value.length })), () => [d(a, {
				name: "trash",
				stroke: e.color
			}, null, 8, ["stroke"])])], 6),
			V.value === "text" ? oe((_(), l("input", {
				key: 0,
				ref: "range",
				type: "range",
				class: "vertical-range",
				min: 3,
				max: 48,
				step: 1,
				"onUpdate:modelValue": r[3] ||= (e) => R.value = e,
				style: g({ accentColor: e.color })
			}, null, 4)), [[ie, R.value]]) : oe((_(), l("input", {
				key: 1,
				ref: "range",
				type: "range",
				class: "vertical-range",
				min: .5,
				max: 12,
				step: .1,
				"onUpdate:modelValue": r[4] ||= (e) => T.value = e,
				style: g({ accentColor: e.color })
			}, null, 4)), [[ie, T.value]])
		], 4)) : c("", !0);
	}
};
//#endregion
export { x as n, se as t };
