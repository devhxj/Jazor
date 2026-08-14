import { t as e } from "./dom-to-png-BYIvdTe8.js";
//#region src/pdf.js
async function t({ domElement: t, fileName: n, scale: r = 2, orientation: i = "auto", overflowTolerance: a = .2, aspectRatio: o = null }) {
	if (!t) return Promise.reject("No domElement provided");
	let s = typeof navigator < "u" && /^((?!chrome|android).)*safari/i.test(navigator.userAgent), c;
	try {
		c = (await import("jspdf")).default;
	} catch {
		return Promise.reject("jspdf is not installed. Run npm install jspdf");
	}
	let l = {
		width: 595.28,
		height: 841.89
	}, u = {
		width: 841.89,
		height: 595.28
	}, d = 1e3;
	function f(e) {
		if (e == null) return null;
		if (typeof e == "number" && e > 0) return {
			w: 1,
			h: e
		};
		if (typeof e == "string") {
			let t = e.split("/").map((e) => e.trim());
			if (t.length === 2) {
				let e = Number(t[0]), n = Number(t[1]);
				if (e > 0 && n > 0) return {
					w: e,
					h: n
				};
			} else if (t.length === 1) {
				let e = Number(t[0]);
				if (e > 0) return {
					w: 1,
					h: e
				};
			}
		}
		return null;
	}
	let p = f(o), m = p ? {
		width: d,
		height: d * (p.h / p.w)
	} : null, h = p ? {
		width: m.height,
		height: m.width
	} : null;
	if (s) try {
		await e({
			container: t,
			scale: r
		}), await e({
			container: t,
			scale: r
		}), await e({
			container: t,
			scale: r
		}), await e({
			container: t,
			scale: r
		});
	} catch {}
	let g = await e({
		container: t,
		scale: r
	});
	return await new Promise((e, t) => {
		let r = new window.Image();
		r.onload = function() {
			let t = .5, o = r.naturalWidth, s = r.naturalHeight, d = i === "auto" ? s >= o ? "p" : "l" : i, f = d === "l" ? p ? h : u : p ? m : l, _ = f.width / o, v = f.height / s, y = s * _, b = "single", x;
			y <= f.height + t ? x = _ : y <= f.height * (1 + a) ? x = Math.min(_, v) : (b = "multi", x = _);
			let S = o * x, C = s * x, w = (f.width - S) / 2, T = new c({
				orientation: d,
				unit: "pt",
				format: p ? [f.width, f.height] : "a4"
			});
			if (b === "single") {
				let e = (f.height - C) / 2;
				T.addImage(g, "PNG", w, e, S, C, "", "FAST");
			} else {
				let e = f.height / x, n = s, r = 0;
				for (; n > t;) T.addImage(g, "PNG", w, r, S, C, "", "FAST"), n -= e, r -= f.height, n > t && T.addPage();
			}
			T.save(`${n}.pdf`), e();
		}, r.onerror = (e) => t("Failed to load image for PDF: " + e), r.src = g;
	});
}
//#endregion
export { t as default };
