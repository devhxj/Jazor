import { ref as e } from "vue";
//#region src/usePrinter.js
function t({ elementId: t, fileName: n, canPrint: r = !0, options: i }) {
	let a = e(!1), o = e(!1), s = e(null);
	async function c() {
		!r || a.value || (a.value = !0, clearTimeout(s.value), s.value = setTimeout(async () => {
			if (r) try {
				let { default: e } = await import("./pdf-CVIDfCrS.js");
				await e({
					domElement: document.getElementById(t),
					fileName: n,
					orientation: i.orientation,
					overflowTolerance: i.overflowTolerance,
					scale: i.scale,
					aspectRatio: i.aspectRatio ?? null
				});
			} catch (e) {
				console.error("Error generating PDF:", e);
			} finally {
				a.value = !1;
			}
		}, 100));
	}
	async function l() {
		!r || o.value || (o.value = !0, clearTimeout(s.value), s.value = setTimeout(async () => {
			if (r) try {
				let { default: e } = await import("./img-Bnokohej.js").then((e) => e.n);
				await e({
					domElement: document.getElementById(t),
					fileName: n,
					format: "png",
					scale: i?.scale
				});
			} catch (e) {
				console.error("Error generating image:", e);
			} finally {
				o.value = !1;
			}
		}, 100));
	}
	return {
		generatePdf: c,
		generateImage: l,
		isPrinting: a,
		isImaging: o
	};
}
//#endregion
export { t };
