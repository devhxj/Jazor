import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { t } from "./dom-to-png-BYIvdTe8.js";
//#region src/img.js
var n = /* @__PURE__ */ e({ default: () => f }), r = "[data-dom-to-png-ignore-layout]";
function i() {
	return new Promise((e) => {
		typeof requestAnimationFrame == "function" ? requestAnimationFrame(e) : setTimeout(e, 0);
	});
}
function a(e) {
	let t = Number.parseFloat(e);
	return Number.isFinite(t) ? t : 0;
}
function o(e, t) {
	let n = e.parentElement;
	for (; n && n !== t;) {
		if (n.matches(r)) return !0;
		n = n.parentElement;
	}
	return !1;
}
function s(e) {
	return Array.from(e.querySelectorAll(r)).filter((t) => !o(t, e));
}
function c(e) {
	let t = e.getBoundingClientRect(), n = window.getComputedStyle(e);
	return n.display === "none" || n.position === "absolute" || n.position === "fixed" ? 0 : t.height + a(n.marginTop) + a(n.marginBottom);
}
function l(e, t, n, r = "") {
	e.style.setProperty(t, n, r);
}
function u(e, t) {
	t === null ? e.removeAttribute("style") : e.setAttribute("style", t);
}
async function d(e, t) {
	let n = s(e);
	if (!n.length) return t();
	let r = e.getAttribute("style"), a = e.getBoundingClientRect().height, o = n.reduce((e, t) => e + c(t), 0), d = n.map((e) => ({
		element: e,
		inlineStyle: e.getAttribute("style")
	}));
	d.forEach(({ element: e }) => {
		l(e, "display", "none", "important");
	}), await i();
	let f = e.getBoundingClientRect().height;
	if (o > 0 && f >= a - .5) {
		let t = Math.max(0, a - o);
		l(e, "height", `${t}px`, "important"), l(e, "max-height", `${t}px`, "important"), l(e, "overflow", "hidden", "important"), await i();
	}
	try {
		return await t();
	} finally {
		u(e, r), d.forEach(({ element: e, inlineStyle: t }) => {
			u(e, t);
		});
	}
}
async function f({ domElement: e, fileName: n, format: r = "png", scale: i = 2, base64: a = !1, img: o = !1 }) {
	if (!e) return Promise.reject("No element provided");
	let s = typeof navigator < "u" && /^((?!chrome|android).)*safari/i.test(navigator.userAgent);
	return await new Promise((e) => setTimeout(e, 200)), d(e, async () => {
		if (s) try {
			await t({
				container: e,
				scale: i
			}), await t({
				container: e,
				scale: i
			}), await t({
				container: e,
				scale: i
			}), await t({
				container: e,
				scale: i
			}), a && (await t({
				container: e,
				scale: i,
				base64: a
			}), await t({
				container: e,
				scale: i,
				base64: a
			}), await t({
				container: e,
				scale: i,
				base64: a
			}), await t({
				container: e,
				scale: i,
				base64: a
			}));
		} catch {}
		if (a && o) try {
			return {
				imageUri: await t({
					container: e,
					scale: i
				}).then((e) => e),
				base64: await t({
					container: e,
					scale: i,
					base64: a
				}).then((e) => e)
			};
		} catch (e) {
			console.error("Error generating image information for the chart", e);
		}
		else if (a) try {
			return t({
				container: e,
				scale: i,
				base64: a
			}).then((e) => e);
		} catch (e) {
			console.error("Error generating the base64 string of the chart", e);
		}
		else try {
			let a = await t({
				container: e,
				scale: i
			}), o = document.createElement("a");
			o.href = a, o.download = `${n}.${r}`, document.body.appendChild(o), o.click(), document.body.removeChild(o);
		} catch (e) {
			throw console.error("Error generating image:", e), e;
		}
	});
}
//#endregion
export { n, f as t };
